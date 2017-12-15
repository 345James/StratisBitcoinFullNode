using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NBitcoin.Protocol;
using Stratis.Bitcoin.Configuration;
using Stratis.Bitcoin.P2P;
using Stratis.Bitcoin.Utilities;
using Xunit;

namespace Stratis.Bitcoin.Features.Dns.Tests
{
    /// <summary>
    /// Defines unit tests for the <see cref="WhitelistManager"/> class.
    /// </summary>
    public class GivenAWhitelistManager
    {
        [Fact]
        [Trait("DNS", "UnitTest")]
        public void WhenConstructorCalled_AndDatetimeProviderIsNull_ThenArgumentNullExceptionIsThrown()
        {
            // Arrange.
            Action a = () => { new WhitelistManager(null, null, null); };

            // Act and Assert.
            a.ShouldThrow<ArgumentNullException>().Which.Message.Should().Contain("dateTimeProvider");
        }

        [Fact]
        [Trait("DNS", "UnitTest")]
        public void WhenConstructorCalled_AndPeerAddressManagerIsNull_ThenArgumentNullExceptionIsThrown()
        {
            // Arrange.
            IDateTimeProvider dateTimeProvider = new Mock<IDateTimeProvider>().Object;

            Action a = () => { new WhitelistManager(dateTimeProvider, null, null); };

            // Act and Assert.
            a.ShouldThrow<ArgumentNullException>().Which.Message.Should().Contain("peerAddressManager");
        }

        [Fact]
        [Trait("DNS", "UnitTest")]
        public void WhenConstructorCalled_AndNodeSettingsAreNull_ThenArgumentNullExceptionIsThrown()
        {
            // Arrange.
            IDateTimeProvider dateTimeProvider = new Mock<IDateTimeProvider>().Object;
            IPeerAddressManager peerAddressManager = new Mock<IPeerAddressManager>().Object;

            Action a = () => { new WhitelistManager(dateTimeProvider, peerAddressManager, null); };

            // Act and Assert.
            a.ShouldThrow<ArgumentNullException>().Which.Message.Should().Contain("nodeSettings");
        }

        [Fact]
        [Trait("DNS", "UnitTest")]
        public void WhenConstructorCalled_AndNodeSettingsConnectionManagerIsNull_ThenArgumentNullExceptionIsThrown()
        {
            // Arrange.
            IDateTimeProvider dateTimeProvider = new Mock<IDateTimeProvider>().Object;
            IPeerAddressManager peerAddressManager = new Mock<IPeerAddressManager>().Object;
            NodeSettings nodeSettings = NodeSettings.Default();
            nodeSettings.ConnectionManager = null;

            Action a = () => { new WhitelistManager(dateTimeProvider, peerAddressManager, nodeSettings); };

            // Act and Assert.
            a.ShouldThrow<ArgumentNullException>().Which.Message.Should().Contain("ConnectionManager");
        }

        [Fact]
        [Trait("DNS", "UnitTest")]
        public void WhenRefreshWhitelist_AndNoPeersAvailable_WhiteListIsEmpty()
        {
            // Arrange.
            IDateTimeProvider dateTimeProvider = new DateTimeProvider();

            Mock<IPeerAddressManager> mockPeerAddressManager = new Mock<IPeerAddressManager>();
            mockPeerAddressManager.Setup(p => p.Peers).Returns(new ConcurrentDictionary<System.Net.IPEndPoint, PeerAddress>());

            NodeSettings nodeSettings = NodeSettings.Default();

            // Act.
            WhitelistManager whitelistManager = new WhitelistManager(dateTimeProvider, mockPeerAddressManager.Object, nodeSettings);
            whitelistManager.RefreshWhitelist();

            IEnumerable<PeerAddress> whitelist = whitelistManager.Whitelist;

            // Assert.
            whitelist.Should().NotBeNull();
            whitelist.Should().BeEmpty();
        }
    }
}
