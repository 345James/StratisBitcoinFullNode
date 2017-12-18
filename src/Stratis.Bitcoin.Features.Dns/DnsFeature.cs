using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stratis.Bitcoin.Builder.Feature;
using Stratis.Bitcoin.Utilities;

namespace Stratis.Bitcoin.Features.Dns
{
    /// <summary>
    /// Responsible for managing the Dns feature.
    /// </summary>
    public class DnsFeature : FullNodeFeature
    {
        /// <summary>
        /// Defines the peer address manager.
        /// </summary>
        private readonly IWhitelistManager whitelistManager;

        /// <summary>
        /// Defines the logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Factory for creating background async loop tasks.
        /// </summary>
        private readonly IAsyncLoopFactory asyncLoopFactory;

        /// <summary>
        /// Global application life cycle control - triggers when application shuts down.
        /// </summary>
        private readonly INodeLifetime nodeLifetime;

        /// <summary>
        /// The async loop to refresh the whitelist.
        /// </summary>
        private IAsyncLoop whitelistRefreshLoop;

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsFeature"/> class.
        /// </summary>
        /// <param name="whitelistManager">The whitelist manager.</param>
        /// <param name="loggerFactory">The factory to create the logger.</param>
        /// <param name="asyncLoopFactory">The asynchronous loop factory.</param>
        /// <param name="nodeLifetime">The node lifetime.</param>
        public DnsFeature(IWhitelistManager whitelistManager, ILoggerFactory loggerFactory, IAsyncLoopFactory asyncLoopFactory, INodeLifetime nodeLifetime)
        {
            Guard.NotNull(whitelistManager, nameof(whitelistManager));
            Guard.NotNull(loggerFactory, nameof(loggerFactory));
            Guard.NotNull(asyncLoopFactory, nameof(asyncLoopFactory));
            Guard.NotNull(nodeLifetime, nameof(nodeLifetime));

            this.whitelistManager = whitelistManager;
            this.logger = loggerFactory.CreateLogger(this.GetType().FullName);
            this.asyncLoopFactory = asyncLoopFactory;
            this.nodeLifetime = nodeLifetime;
        }

        /// <summary>
        /// Initializes the Dns feature.
        /// </summary>
        public override void Initialize()
        {
            this.logger.LogTrace("()");

            this.logger.LogInformation("Starting DNS...");
            this.StartWhitelistRefreshLoop();

            this.logger.LogTrace("(-)");
        }

        /// <summary>
        /// Disposes of the object.
        /// </summary>
        public override void Dispose()
        {
            this.logger.LogTrace("()");

            this.logger.LogInformation("Stopping DNS...");
            this.whitelistRefreshLoop?.Dispose();
            this.whitelistRefreshLoop = null;

            this.logger.LogTrace("(-)");
        }

        /// <summary>
        /// Starts the loop to refresh the whitelist.
        /// </summary>
        private void StartWhitelistRefreshLoop()
        {
            this.logger.LogTrace("()");

            this.whitelistRefreshLoop = this.asyncLoopFactory.Run($"{nameof(DnsFeature)}.WhitelistRefreshLoop", token =>
            {
                this.whitelistManager.RefreshWhitelist();
                return Task.CompletedTask;
            },
            this.nodeLifetime.ApplicationStopping,
            repeatEvery: TimeSpans.TenSeconds);

            this.logger.LogTrace("(-)");
        }
    }
}