using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Text;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Trace;
using ShipMvp.Domain.Analytics;
using ShipMvp.Integration.SemanticKernel.Plugins;
using ShipMvp.Integration.SemanticKernel.Plugins.DataExtraction;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ShipMvp.Integration.SemanticKernel.Infrastructure
{
    public class SemanticKernelService : ISemanticKernelService, IDisposable
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly ILogger<SemanticKernelService> _logger;
        private readonly ActivitySource _activitySource;
        private bool _disposed = false;

        const int TOKENS_PER_CHUNK = 6_000;
        const int TOKENS_PER_LINE = 256;

        public SemanticKernelService(
            IConfiguration configuration, 
            ILogger<SemanticKernelService> logger)
        {
            var openAIKey = configuration["Integrations:OpenAI:OpenAIKey"] ?? string.Empty;
            var openAIDeployment = configuration["Integrations:OpenAI:OpenAIDeployment"] ?? string.Empty;

            var geminiKey = configuration["Integrations:Google:GeminiKey"];
            var geminiModel = configuration["Integrations:Google:GeminiModel"]
                           ?? "gemini-1.5-pro-latest";

            var longTimeoutClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(4)
            };

            var kernelBuilder = Kernel.CreateBuilder();
            if (!string.IsNullOrWhiteSpace(openAIKey))
                kernelBuilder.AddOpenAIChatCompletion(
                    modelId: "gpt-4o",
                    apiKey: openAIKey,
                    serviceId: "gpt-4o");

            if (!string.IsNullOrWhiteSpace(geminiKey))
                kernelBuilder.AddGoogleAIGeminiChatCompletion(
                    modelId: geminiModel,
                    apiKey: geminiKey,
                    serviceId: "gemini");

            // 🟦 2-token, fast reasoning
            kernelBuilder.AddGoogleAIGeminiChatCompletion(
                modelId: "gemini-2.5-flash",
                apiKey: geminiKey,
                serviceId: "gemini-flash",
                httpClient: longTimeoutClient);

            // 🟧 high-accuracy, 32 k context
            kernelBuilder.AddGoogleAIGeminiChatCompletion(
                modelId: "gemini-2.5-pro",
                apiKey: geminiKey,
                serviceId: "gemini-pro",
                httpClient: longTimeoutClient);

            // 🟪 multimodal vision
            kernelBuilder.AddGoogleAIGeminiChatCompletion(
                modelId: "gemini-2.0-pro-vision",
                apiKey: geminiKey,
                serviceId: "gemini-vision");

            SetupPlugins(kernelBuilder);

            _kernel = kernelBuilder.Build();
            _kernel.ImportPluginFromObject(new ClassifyRowSkill(), nameof(ClassifyRowSkill));
            _kernel.ImportPluginFromObject(new DetectYearTypeSkill(), nameof(DetectYearTypeSkill));
            _kernel.ImportPluginFromObject(new ExtractTablesJsonFromFileSkill(), "DataExtraction");

            _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            _logger = logger;
            _activitySource = new ActivitySource("ShipMvp.LlmLogging");
        }

        // Helper for Langfuse Basic-auth header
        private static string BuildLangfuseAuth(string pub, string sec)
        {
            var bytes = Encoding.ASCII.GetBytes($"{pub}:{sec}");
            return Convert.ToBase64String(bytes);
        }

        private void SetupPlugins(IKernelBuilder kernelBuilder)
        {
            string projectRootDir = AppDomain.CurrentDomain.BaseDirectory;

            if (Directory.Exists(Path.Combine(projectRootDir, "Plugins")))
            {
                kernelBuilder.Plugins.AddFromPromptDirectory(Path.Combine(projectRootDir, "Plugins", "ExcelFormatter"));
                kernelBuilder.Plugins.AddFromPromptDirectory(Path.Combine(projectRootDir, "Plugins", "StructureAnalysis"));
                kernelBuilder.Plugins.AddFromPromptDirectory(Path.Combine(projectRootDir, "Plugins", "GenerateFormulaSkill"));
            }
        }

        public Kernel Kernel => _kernel;

        public async Task<FunctionResult> InvokeAsync(
               string pluginName,
               string functionName,
               KernelArguments arguments = null,
               CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            // Create custom activity for LLM logging
            using var activity = StartLlmActivity(pluginName, functionName, arguments);

            try
            {
                _logger?.LogInformation("Invoking plugin '{Plugin}' function '{Function}'", pluginName, functionName);
                
                var result = await _kernel.InvokeAsync(
                    pluginName,
                    functionName,
                    arguments ?? new KernelArguments(),
                    cancellationToken);

                // Create a new LlmLog instance with correct types
                var llmLog = new LlmLog(
                    Guid.NewGuid(),
                    pluginName,
                    functionName,
                    "modelId",
                    "serviceId",
                    "renderedPrompt",
                    "arguments",
                    null, // UserId
                    null, // SessionId
                    null  // RequestId
                );

                // Enrich activity with result data (handles null activity gracefully)
                EnrichActivityWithResult(activity, result);
                
                return result;
            }
            catch (Exception ex)
            {
                // Handle error in activity if it exists
                if (activity != null)
                {
                    activity.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity.SetTag("llm.error.message", ex.Message);
                    activity.SetTag("llm.error.type", ex.GetType().Name);
                }
                else
                {
                    // Log the error even if no activity is available
                    _logger?.LogError(ex, "LLM call failed for {Plugin}.{Function} (no activity tracking)", pluginName, functionName);
                }
                throw;
            }
        }

        private Activity? StartLlmActivity(string pluginName, string functionName, KernelArguments? arguments)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var activity = _activitySource.StartActivity($"{pluginName}.{functionName}");
            
            if (activity == null)
            {
                // Log when activity creation is skipped (this is normal in many scenarios)
                _logger?.LogDebug("Activity creation skipped for {Plugin}.{Function} - likely due to no listeners or sampling", 
                    pluginName, functionName);
                return null;
            }
            
            var (modelId, serviceId) = DetermineModelInfo(pluginName, functionName);
            
            // Set core LLM tags
            activity.SetTag("llm.plugin_name", pluginName);
            activity.SetTag("llm.function_name", functionName);
            activity.SetTag("llm.model_id", modelId);
            activity.SetTag("llm.service_id", serviceId);
            activity.SetTag("llm.request_id", activity.Id);
            
            // Add argument metadata
            if (arguments != null && arguments.Count > 0)
            {
                activity.SetTag("llm.arguments_count", arguments.Count);
                
                // Add specific arguments (but limit size)
                foreach (var arg in arguments)
                {
                    var value = SerializeArgumentForTag(arg.Value);
                    if (!string.IsNullOrEmpty(value))
                    {
                        activity.SetTag($"llm.arg.{arg.Key}", value);
                    }
                }
            }
            
            // Add session context if available
            var sessionId = ExtractSessionId(arguments);
            if (!string.IsNullOrEmpty(sessionId))
            {
                activity.SetTag("llm.session_id", sessionId);
            }
            
            return activity;
        }

        private static void EnrichActivityWithResult(Activity? activity, FunctionResult result)
        {
            if (activity == null) 
            {
                // Activity can be null when no listeners are active or due to sampling decisions
                // This is normal behavior in OpenTelemetry - just skip enrichment
                return;
            }

            // Add response data
            var responseContent = result.GetValue<string>() ?? "";
            activity.SetTag("llm.response_length", responseContent.Length);

            // Add response content (truncated for logging)
            if (responseContent.Length > 0)
            {
                activity.SetTag("llm.response_content", responseContent);
            }

            // Extract and set token usage from metadata
            if (result.Metadata != null)
            {
                foreach (var metadata in result.Metadata)
                {
                    // Serialize complex objects to JSON for metadata
                    var value = metadata.Value is string strValue ? strValue : JsonSerializer.Serialize(metadata.Value);
                    activity.SetTag($"llm.metadata.{metadata.Key}", value);
                }

                // Specific token mappings for different metadata formats
                if (result.Metadata.TryGetValue("PromptTokenCount", out var promptTokens))
                    activity.SetTag("llm.prompt_tokens", promptTokens);
                if (result.Metadata.TryGetValue("CandidatesTokenCount", out var responseTokens))
                    activity.SetTag("llm.response_tokens", responseTokens);
                if (result.Metadata.TryGetValue("TotalTokenCount", out var totalTokens))
                    activity.SetTag("llm.total_tokens", totalTokens);
                if (result.Metadata.TryGetValue("FinishReason", out var finishReason))
                    activity.SetTag("llm.finish_reason", finishReason?.ToString() ?? "");

                // Handle different metadata key formats (Google vs OpenAI)
                if (result.Metadata.TryGetValue("usage.input_tokens", out var inputTokens))
                    activity.SetTag("llm.prompt_tokens", inputTokens);
                if (result.Metadata.TryGetValue("usage.output_tokens", out var outputTokens))
                    activity.SetTag("llm.response_tokens", outputTokens);
            }

            // Add rendered prompt if available
            if (!string.IsNullOrEmpty(result.RenderedPrompt))
            {
                activity.SetTag("llm.rendered_prompt", result.RenderedPrompt);
            }
            else
            {
                // Fallback: Log metadata or response content as prompt if RenderedPrompt is missing
                activity.SetTag("llm.rendered_prompt", responseContent.Length > 0 ? responseContent : "Prompt not captured");
            }

            activity.SetStatus(ActivityStatusCode.Ok);

            // Serialize and set AdditionalMetadata as a tag
            var additionalMetadata = JsonSerializer.Serialize(result.Metadata, new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            activity.SetTag("llm.additional_metadata", additionalMetadata);
        }

        private static string? SerializeArgumentForTag(object? value)
        {
            if (value == null) return null;
            
            return value switch
            {
                string str when str.Length > 500 => str.Substring(0, 500) + "...<truncated>",
                string str => str,
                byte[] bytes => $"<binary_data_length_{bytes.Length}>",
                Stream => "<stream_data>",
                _ when value.ToString()?.Length > 500 => value.ToString()?.Substring(0, 500) + "...<truncated>",
                _ => value.ToString()
            };
        }

        private static string? ExtractSessionId(KernelArguments? arguments)
        {
            // Try to extract session ID from arguments
            if (arguments?.TryGetValue("sessionId", out var sessionId) == true)
                return sessionId?.ToString();
            if (arguments?.TryGetValue("session_id", out var sessionId2) == true)
                return sessionId2?.ToString();
            
            return null;
        }

        private static (string modelId, string serviceId) DetermineModelInfo(string pluginName, string functionName)
        {
            return pluginName.ToLower() switch
            {
                "dataextraction" => ("gemini-2.0-pro-vision", "gemini-vision"),
                "excelformatter" when functionName == "GenerateFormatActions" => ("gemini-2.5-pro", "gemini-pro"),
                "excelformatter" when functionName == "DetermineDriverCells" => ("gemini-2.5-flash", "gemini-flash"),
                "excelformatter" => ("gemini-2.5-pro", "gemini-pro"),
                _ => ("gemini-2.5-flash", "gemini-flash")
            };
        }

        public async Task<FunctionResult> InvokeExcelFormatterAsync(
            string snapshot,
            string topLeftCell,
            string topRightCell,
            string bottomLeftCell,
            string bottomRightCell,
            string rulesYaml)
        {
            string minimizedSnapshot = MinimizeJson(snapshot);
            var arguments = new KernelArguments
            {
                ["snapshot"] = minimizedSnapshot,
                ["topLeftCell"] = topLeftCell,
                ["topRightCell"] = topRightCell,
                ["bottomLeftCell"] = bottomLeftCell,
                ["bottomRightCell"] = bottomRightCell,
                ["rules_yaml"] = rulesYaml
            };
            
            return await InvokeAsync("ExcelFormatter", "GenerateFormatActions", arguments);
        }

        public async Task<FunctionResult> InvokeExcelFixFormatterAsync(
          string snapshot,
          string topLeftCell,
          string topRightCell,
          string bottomLeftCell,
          string bottomRightCell,
          string rulesYaml)
        {
            string minimizedSnapshot = MinimizeJson(snapshot);
            var arguments = new KernelArguments
            {
                ["snapshot"] = minimizedSnapshot,
                ["topLeftCell"] = topLeftCell,
                ["topRightCell"] = topRightCell,
                ["bottomLeftCell"] = bottomLeftCell,
                ["bottomRightCell"] = bottomRightCell,
                ["rules_yaml"] = rulesYaml
            };
            
            var result = await InvokeAsync("ExcelFormatter", "FixFormat", arguments);
            
            // Additional logging for specific method
            _logger?.LogInformation("-----------------------------");
            _logger?.LogInformation("Prompt: ");
            _logger?.LogInformation("-----------------------------");
            _logger?.LogInformation(result.RenderedPrompt);
            _logger?.LogInformation("-----------------------------");
            
            if (result.Metadata.TryGetValue("usage.input_tokens", out var inputTokens))
                _logger?.LogInformation("Input tokens used: {InputTokens}", inputTokens);
            if (result.Metadata.TryGetValue("usage.output_tokens", out var outputTokens))
                _logger?.LogInformation("Output tokens used: {OutputTokens}", outputTokens);
            
            _logger?.LogInformation("-----------------------------");
            
            return result;
        }

        public async Task<string> InvokeDetermineCellDriversAsync(string snapshot)
        {
            string minimizedSnapshot = MinimizeJson(snapshot);

#pragma warning disable SKEXP0050
            var lines = TextChunker.SplitPlainTextLines(minimizedSnapshot, TOKENS_PER_LINE);
            var chunks = TextChunker.SplitPlainTextParagraphs(lines, TOKENS_PER_CHUNK);
#pragma warning restore SKEXP0050

            var results = new List<string>();
            var i = 0;
            
            foreach (var chunk in chunks)
            {
                var args = new KernelArguments
                {
                    ["snapshot"] = chunk,
                    ["chunkNumber"] = i + 1,
                    ["totalChunks"] = chunks.Count,
                    ["tokensPerChunk"] = TOKENS_PER_CHUNK
                };
                i++;

                var partial = await InvokeAsync("ExcelFormatter", "DetermineDriverCells", args);

                // Additional logging for chunks
                if (partial.Metadata != null)
                {
                    if (partial.Metadata.TryGetValue("usage.input_tokens", out var inputTokens))
                        _logger?.LogInformation("Chunk {Chunk}: Input tokens used: {InputTokens}", i, inputTokens);
                    if (partial.Metadata.TryGetValue("usage.output_tokens", out var outputTokens))
                        _logger?.LogInformation("Chunk {Chunk}: Output tokens used: {OutputTokens}", i, outputTokens);
                }

                var json = partial.GetValue<string>();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    try
                    {
                        var arr = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json);
                        if (arr != null)
                            results.AddRange(arr);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Failed to parse chunk result as JSON array of strings: {Json}", json);
                    }
                }
            }
            
            _logger?.LogInformation("Processed {Count} chunks of driver cells.", results.Count);

            var consolidated = System.Text.Json.JsonSerializer.Serialize(results);
            _logger?.LogInformation("Consolidated driver cells: {Consolidated}", consolidated);
            return consolidated;
        }

        private string MinimizeJson(string json)
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            return System.Text.Json.JsonSerializer.Serialize(doc, new System.Text.Json.JsonSerializerOptions { WriteIndented = false });
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    _activitySource?.Dispose();
                    
                    // Note: We don't dispose _kernel here because it might be used by other components
                    // and its lifecycle is managed by the DI container's OpenTelemetry configuration
                    
                    _logger?.LogDebug("SemanticKernelService disposed");
                }

                _disposed = true;
            }
        }
    }
}

