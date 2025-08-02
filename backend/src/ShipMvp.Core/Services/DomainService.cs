using Microsoft.Extensions.Logging;
using ShipMvp.Core.Abstractions;

namespace ShipMvp.Core.Services
{
    public class DomainService
    {
        /// <summary>Sequential GUIDs for entities &amp; events.</summary>
        protected IGuidGenerator GuidGenerator { get; }

        /// <summary>Typed logger with the concrete service’s name.</summary>
        protected ILogger Logger { get; }

        protected DomainService(
            IGuidGenerator guidGenerator,
            ILoggerFactory loggerFactory)
        {
            GuidGenerator = guidGenerator;
            Logger = loggerFactory.CreateLogger(GetType());
        }
    }
}
