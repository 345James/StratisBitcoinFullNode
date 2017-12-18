using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using NBitcoin.Protocol;
using Stratis.Bitcoin.Configuration;
using Stratis.Bitcoin.P2P;
using Stratis.Bitcoin.Utilities;

namespace Stratis.Bitcoin.Features.Dns
{
    /// <summary>
    /// Responsible for managing the whitelist used by the DNS server as a master file.
    /// </summary>
    public class WhitelistManager : IWhitelistManager
    {
        /// <summary>
        /// Defines the provider for the datetime.
        /// </summary>
        private readonly IDateTimeProvider dateTimeProvider;

        /// <summary>
        /// Defines the logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Defines the manager implementation for peer addresses used to populate the whitelist.
        /// </summary>
        private readonly IPeerAddressManager peerAddressManager;

        /// <summary>
        /// Defines the peroid in seconds that the peer should last have been seen to be included in the whitelist.
        /// </summary>
        private readonly int dnsPeerBlacklistThresholdInSeconds;

        /// <summary>
        /// Defines the external endpoint for the dns node.
        /// </summary>
        private readonly IPEndPoint externalEndpoint;

        /// <summary>
        /// Defines if DNS server daemon is running as full node <c>true</c> or not <c>false</c>.
        /// </summary>
        private readonly bool fullNodeMode = false;

        /// <summary>
        /// TODO: This will be removed after the DNS object is managed by this class.
        /// </summary>
        public IEnumerable<PeerAddress> Whitelist { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WhitelistManager"/> class.
        /// </summary>
        /// <param name="dateTimeProvider">The provider for datetime.</param>
        /// <param name="loggerFactory">The factory to create the logger.</param>
        /// <param name="peerAddressManager">The manager implementation for peer addresses.</param>
        /// <param name="nodeSettings">The node settings.</param>
        public WhitelistManager(IDateTimeProvider dateTimeProvider, ILoggerFactory loggerFactory, IPeerAddressManager peerAddressManager, NodeSettings nodeSettings)
        {
            Guard.NotNull(dateTimeProvider, nameof(dateTimeProvider));
            Guard.NotNull(loggerFactory, nameof(loggerFactory));
            Guard.NotNull(peerAddressManager, nameof(peerAddressManager));
            Guard.NotNull(nodeSettings, nameof(nodeSettings));
            Guard.NotNull(nodeSettings.ConnectionManager, nameof(nodeSettings.ConnectionManager));

            this.dateTimeProvider = dateTimeProvider;
            this.logger = loggerFactory.CreateLogger(this.GetType().FullName);
            this.peerAddressManager = peerAddressManager;
            this.dnsPeerBlacklistThresholdInSeconds = nodeSettings.DnsPeerBlacklistThresholdInSeconds;
            this.externalEndpoint = nodeSettings.ConnectionManager.ExternalEndpoint;
            this.fullNodeMode = nodeSettings.DnsFullNode;

            this.Whitelist = new List<PeerAddress>();            
        }

        /// <summary>
        /// Refreshes the managed whitelist.
        /// </summary>
        public void RefreshWhitelist()
        {
            this.logger.LogTrace("()");

            DateTimeOffset activePeerLimit = this.dateTimeProvider.GetTimeOffset().AddSeconds(-this.dnsPeerBlacklistThresholdInSeconds);

            var whitelist = this.peerAddressManager.Peers.Where(p => p.Value.LastConnectionHandshake > activePeerLimit).Select(p => p.Value);

            if (!this.fullNodeMode)
            {
                // Exclude the current external ip address from DNS as its not a full node.
                whitelist = whitelist.Where(p => !p.NetworkAddress.Endpoint.Match(this.externalEndpoint));
            }

            // TODO: change this to swap out the whitelist master file in DNS.
            this.Whitelist = whitelist;

            this.logger.LogTrace("(-)");
        }
    }
}