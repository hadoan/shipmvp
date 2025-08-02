using Microsoft.Extensions.Logging;
using ShipMvp.Core.Abstractions;
using ShipMvp.Core.Security;

namespace ShipMvp.Core.Services
{
    public class ApplicationService
    {
        /// <summary>Shortcut for generating sequential GUIDs.</summary>
        protected IGuidGenerator GuidGenerator { get; }

        /// <summary>Shortcut for the current signed-in user.</summary>
        protected ICurrentUser CurrentUser { get; }

        /// <summary>Typed logger with the concrete service’s name.</summary>
        protected ILogger Logger { get; }

        protected ApplicationService(
            IGuidGenerator guidGenerator,
            ICurrentUser currentUser,
            ILoggerFactory loggerFactory)
        {
            GuidGenerator = guidGenerator;
            CurrentUser = currentUser;
            Logger = loggerFactory.CreateLogger(GetType());
        }
    }
}
