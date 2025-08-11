using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.Threading;
using System.Threading.Tasks;

namespace ShipMvp.Integration.SemanticKernel.Infrastructure
{
    public interface ISemanticKernelService
    {
        public Kernel Kernel { get; }

        Task<FunctionResult> InvokeAsync(
            string pluginName,
            string functionName,
            KernelArguments arguments = null,
            CancellationToken cancellationToken = default);

        Task<FunctionResult> InvokeExcelFormatterAsync(
            string snapshot,
            string topLeftCell,
            string topRightCell,
            string bottomLeftCell,
            string bottomRightCell,
            string rulesYaml);

        Task<FunctionResult> InvokeExcelFixFormatterAsync(
            string snapshot,
            string topLeftCell,
            string topRightCell,
            string bottomLeftCell,
            string bottomRightCell,
            string rulesYaml);

        Task<string> InvokeDetermineCellDriversAsync(
            string snapshot);
    }

}
