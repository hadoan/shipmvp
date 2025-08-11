using OpenTelemetry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ShipMvp.Integration.SemanticKernel.Infrastructure;

/// <summary>
/// OpenTelemetry processor that captures LLM telemetry data and forwards it to our custom logging system
/// </summary>
public sealed class LlmTelemetryProcessor : BaseProcessor<Activity>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LlmTelemetryProcessor> _logger;

    public LlmTelemetryProcessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        
        // Create logger safely
        try
        {
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            _logger = loggerFactory?.CreateLogger<LlmTelemetryProcessor>() 
                     ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<LlmTelemetryProcessor>.Instance;
        }
        catch
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<LlmTelemetryProcessor>.Instance;
        }
    }

    public override void OnEnd(Activity data)
    {
        try
        {
            // Only process activities from our custom LLM logging source
            if (data.Source?.Name == "ShipMvp.LlmLogging")
            {
                ProcessLlmActivity(data);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing LLM activity in telemetry processor");
        }
        
        base.OnEnd(data);
    }

    private void ProcessLlmActivity(Activity activity)
    {
        try
        {
            // Extract LLM data from activity
            var llmData = ExtractLlmDataFromActivity(activity);
            if (llmData != null)
            {
                // Fire-and-forget async logging to avoid blocking
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var llmLoggingService = scope.ServiceProvider
                            .GetService<Application.Infrastructure.Analytics.Services.ILlmLoggingService>();
                        
                        if (llmLoggingService == null)
                        {
                            _logger.LogDebug("LlmLoggingService not available, skipping activity logging");
                            return;
                        }

                        // Start logging entry
                        var logEntry = await llmLoggingService.StartLoggingAsync(
                            llmData.PluginName,
                            llmData.FunctionName,
                            llmData.ModelId,
                            llmData.ServiceId,
                            llmData.RenderedPrompt,
                            llmData.Arguments,
                            llmData.SessionId,
                            llmData.RequestId);

                        // Complete logging with results
                        if (activity.Status == ActivityStatusCode.Ok)
                        {
                            await llmLoggingService.CompleteLoggingAsync(
                                logEntry.Id,
                                llmData.Response,
                                activity.Duration,
                                llmData.Metadata);
                        }
                        else
                        {
                            var exception = new Exception(llmData.ErrorMessage ?? "LLM call failed");
                            await llmLoggingService.LogErrorAsync(
                                logEntry.Id,
                                exception,
                                activity.Duration);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to log LLM activity data");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting LLM data from activity");
        }
    }

    private LlmActivityData? ExtractLlmDataFromActivity(Activity activity)
    {
        try
        {
            // Extract core data from activity tags
            var pluginName = GetTagValue(activity, "llm.plugin_name");
            var functionName = GetTagValue(activity, "llm.function_name");
            var modelId = GetTagValue(activity, "llm.model_id") ?? "unknown";
            var serviceId = GetTagValue(activity, "llm.service_id") ?? "unknown";

            if (string.IsNullOrEmpty(pluginName) || string.IsNullOrEmpty(functionName))
            {
                _logger.LogDebug("Missing plugin or function name in activity tags");
                return null;
            }

            // Extract additional data
            var renderedPrompt = GetTagValue(activity, "llm.rendered_prompt") ?? "Prompt not captured";
            var response = GetTagValue(activity, "llm.response_content") ?? "";
            var sessionId = GetTagValue(activity, "llm.session_id");
            var requestId = GetTagValue(activity, "llm.request_id") ?? activity.Id;
            var errorMessage = GetTagValue(activity, "llm.error.message");

            // Build arguments from activity tags
            var arguments = ExtractArgumentsFromActivity(activity);
            
            // Build metadata from activity tags
            var metadata = ExtractMetadataFromActivity(activity);
            var addtionalMetadata = GetTagValue(activity, "llm.additional_metadata");

            return new LlmActivityData
            {
                PluginName = pluginName,
                FunctionName = functionName,
                ModelId = modelId,
                ServiceId = serviceId,
                RenderedPrompt = renderedPrompt,
                Response = response,
                Arguments = arguments,
                SessionId = sessionId,
                RequestId = requestId,
                ErrorMessage = errorMessage,
                Metadata = metadata,
                AddtionalMetadata = addtionalMetadata
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract LLM data from activity {ActivityId}", activity.Id);
            return null;
        }
    }

    private static string? GetTagValue(Activity activity, string tagName)
    {
        return activity.GetTagItem(tagName)?.ToString();
    }

    private static string ExtractArgumentsFromActivity(Activity activity)
    {
        try
        {
            var args = new Dictionary<string, object>();
            
            // Extract argument count
            var argsCount = GetTagValue(activity, "llm.arguments_count");
            if (!string.IsNullOrEmpty(argsCount))
            {
                args["arguments_count"] = argsCount;
            }
            
            // Extract individual arguments
            foreach (var tag in activity.Tags)
            {
                if (tag.Key.StartsWith("llm.arg.") && !string.IsNullOrEmpty(tag.Value))
                {
                    var argName = tag.Key.Substring(8); // Remove "llm.arg." prefix
                    args[argName] = tag.Value;
                }
            }

            return args.Count > 0 ? JsonSerializer.Serialize(args) : "{}";
        }
        catch
        {
            return "{}";
        }
    }

    private static Dictionary<string, object> ExtractMetadataFromActivity(Activity activity)
    {
        var metadata = new Dictionary<string, object>();
        
        try
        {
            // Extract token usage and other metadata
            foreach (var tag in activity.Tags)
            {
                if (tag.Key.StartsWith("llm.") && !string.IsNullOrEmpty(tag.Value))
                {
                    var key = tag.Key.Substring(4); // Remove "llm." prefix
                    
                    // Skip already processed tags
                    if (IsProcessedTag(key)) continue;
                    
                    // Try to parse numeric values
                    if (int.TryParse(tag.Value, out var intValue))
                    {
                        metadata[key] = intValue;
                    }
                    else if (decimal.TryParse(tag.Value, out var decimalValue))
                    {
                        metadata[key] = decimalValue;
                    }
                    else
                    {
                        metadata[key] = tag.Value;
                    }
                }
            }

            // Ensure we have the core token metadata keys that ILlmLoggingService expects
            if (metadata.TryGetValue("prompt_tokens", out var promptTokens))
                metadata["PromptTokenCount"] = promptTokens;
            if (metadata.TryGetValue("response_tokens", out var responseTokens))
                metadata["CandidatesTokenCount"] = responseTokens;
            if (metadata.TryGetValue("total_tokens", out var totalTokens))
                metadata["TotalTokenCount"] = totalTokens;
            if (metadata.TryGetValue("finish_reason", out var finishReason))
                metadata["FinishReason"] = finishReason;
        }
        catch
        {
            // Return empty metadata on error
        }
        
        return metadata;
    }

    private static bool IsProcessedTag(string key)
    {
        // Skip tags that are already processed in the main extraction
        return key is "plugin_name" or "function_name" or "model_id" or "service_id" 
                   or "rendered_prompt" or "response_content" or "session_id" 
                   or "request_id" or "error.message" or "arguments_count"
                   || key.StartsWith("arg.");
    }
}

/// <summary>
/// Data extracted from OpenTelemetry activity for LLM logging
/// </summary>
public sealed class LlmActivityData
{
    public string PluginName { get; set; } = string.Empty;
    public string FunctionName { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;
    public string RenderedPrompt { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public string? RequestId { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();

    public string? AddtionalMetadata { get; set; }
}
