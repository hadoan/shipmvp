using System;
using System.Threading.Tasks;

namespace ShipMvp.Integration.SemanticKernel.Services
{
    public interface ISemanticKernelDraftService
    {
        Task<string> GenerateDraftAsync(string prompt, string? context, Guid userId);
    }

    public class SemanticKernelDraftService : ISemanticKernelDraftService
    {
        public Task<string> GenerateDraftAsync(string prompt, string? context, Guid userId)
        {
            // TODO: Integrate with Semantic Kernel
            var draft = $"[MOCK] Draft for user {userId}:\nPrompt: {prompt}\nContext: {context}";
            return Task.FromResult(draft);
        }
    }
}
