using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NBitcoin;
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
            Action a = () => { new WhitelistManager(null, null, null, null); };

            // Act and Assert.
            a.ShouldThrow<ArgumentNullException>().Which.Message.Should().Contain("dateTimeProvider");
        }

        [Fact]
        [Trait("DNS", "UnitTest")]
        public void WhenConstructorCalled_AndLoggerFactoryIsNull_ThenArgumentNullExceptionIsThrown()
        {
            // Arrange.
            IDateTimeProvider dateTimeProvider = new Mock<IDateTimeProvider>().Object;
            
            Action a = () => { new WhitelistManager(dateTimeProvider, null, null, null); };

            // Act and Assert.
            a.ShouldThrow<ArgumentNullException>().Which.Message.Should().Contain("loggerFactory");
        }

        [Fact]
        [Trait("DNS", "UnitTest")]
        public void WhenConstructorCalled_AndPeerAddressManagerIsNull_ThenArgumentNullExceptionIsThrown()
        {
            // Arrange.
            IDateTimeProvider dateTimeProvider = new Mock<IDateTimeProvider>().Object;
            ILoggerFactory loggerFactory = new Mock<ILoggerFactory>().Object;

            Action a = () => { new WhitelistManager(dateTimeProvider, loggerFactory, null, null); };

            // Act and Assert.
            a.ShouldThrow<ArgumentNullException>().Which.Message.Should().Contain("peerAddressManager");
        }

        [Fact]
        [Trait("DNS", "UnitTest")]
        public void WhenConstructorCalled_AndNodeSettingsAreNull_ThenArgumentNullExceptionIsThrown()
        {
            // Arrange.
            IDateTimeProvider dateTimeProvider = new Mock<IDateTimeProvider>().Object;
            ILoggerFactory loggerFactory = new Mock<ILoggerFactory>().Object;
            IPeerAddressManager peerAddressManager = new Mock<IPeerAddressManager>().Object;

            Action a = () => { new WhitelistManager(dateTimeProvider, loggerFactory, peerAddressManager, null); };

            // Act and Assert.
            a.ShouldThrow<ArgumentNullException>().Which.Message.Should().Contain("nodeSettings");
        }

        [Fact]
        [Trait("DNS", "UnitTest")]
        public void WhenConstructorCalled_AndNodeSettingsConnectionManagerIsNull_ThenArgumentNullExceptionIsThrown()
        {
            // Arrange.
            IDateTimeProvider dateTimeProvider = new Mock<IDateTimeProvider>().Object;
            ILoggerFactory loggerFactory = new Mock<ILoggerFactory>().Object;
            IPeerAddressManager peerAddressManager = new Mock<IPeerAddressManager>().Object;
            NodeSettings nodeSettings = NodeSettings.Default();
            nodeSettings.ConnectionManager = null;

            Action a = () => { new WhitelistManager(dateTimeProvider, loggerFactory, peerAddressManager, nodeSettings); };

            // Act and Assert.
            a.ShouldThrow<ArgumentNullException>().Which.Message.Should().Contain("ConnectionManager");
        }

        [Fact]
        [Trait("DNS", "UnitTest")]
        public void WhenRefreshWhitelist_AndNoPeersAvailable_ThenWhiteListIsEmpty()
        {
            // Arrange.
            IDateTimeProvider dateTimeProvider = new DateTimeProvider();

            Mock<ILogger> mockLogger = new Mock<ILogger>();
            Mock<ILoggerFactory> mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
            ILoggerFactory loggerFactory = mockLoggerFactory.Object;

            Mock<IPeerAddressManager> mockPeerAddressManager = new Mock<IPeerAddressManager>();
            mockPeerAddressManager.Setup(p => p.Peers).Returns(new ConcurrentDictionary<System.Net.IPEndPoint, PeerAddress>());

            NodeSettings nodeSettings = NodeSettings.Default();

            // Act.
            WhitelistManager whitelistManager = new WhitelistManager(dateTimeProvider, loggerFactory, mockPeerAddressManager.Object, nodeSettings);
            whitelistManager.RefreshWhitelist();

            IEnumerable<PeerAddress> whitelist = whitelistManager.Whitelist;

            // Assert.
            whitelist.Should().NotBeNull();
            whitelist.Should().BeEmpty();
        }

        [Fact]
        [Trait("DNS", "UnitTest")]
        public void WenRefreshWhitelist_AndActivePeersAvailable_ThenWhitelistContainsActivePeers()
        {
            // Arrange.
            Mock<IDateTimeProvider> mockDateTimeProvider = new Mock<IDateTimeProvider>();

            Mock<ILogger> mockLogger = new Mock<ILogger>();
            Mock<ILoggerFactory> mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
            ILoggerFactory loggerFactory = mockLoggerFactory.Object;

            mockDateTimeProvider.Setup(d => d.GetTimeOffset()).Returns(new DateTimeOffset(new DateTime(2017, 8, 30, 1, 2, 3))).Verifiable();
            IDateTimeProvider dateTimeProvider = mockDateTimeProvider.Object;

            int inactiveTimePeriod = 2000;

            IPAddress activeIpAddressOne = IPAddress.Parse("::ffff:192.168.0.1");
            NetworkAddress activeNetworkAddressOne = new NetworkAddress(activeIpAddressOne, 80);

            IPAddress activeIpAddressTwo = IPAddress.Parse("::ffff:192.168.0.2");
            NetworkAddress activeNetworkAddressTwo = new NetworkAddress(activeIpAddressTwo, 80);

            IPAddress activeIpAddressThree = IPAddress.Parse("::ffff:192.168.0.3");
            NetworkAddress activeNetworkAddressThree = new NetworkAddress(activeIpAddressThree, 80);

            IPAddress activeIpAddressFour = IPAddress.Parse("::ffff:192.168.0.4");
            NetworkAddress activeNetworkAddressFour = new NetworkAddress(activeIpAddressFour, 80);

            List<Tuple<NetworkAddress, DateTimeOffset>> testDataSet = new List<Tuple<NetworkAddress, DateTimeOffset>>()
            {
                new Tuple<NetworkAddress, DateTimeOffset> (activeNetworkAddressOne,  dateTimeProvider.GetTimeOffset().AddSeconds(-inactiveTimePeriod).AddSeconds(10)),
                new Tuple<NetworkAddress, DateTimeOffset>(activeNetworkAddressTwo, dateTimeProvider.GetTimeOffset().AddSeconds(-inactiveTimePeriod).AddSeconds(20)),
                new Tuple<NetworkAddress, DateTimeOffset>(activeNetworkAddressThree, dateTimeProvider.GetTimeOffset().AddSeconds(-inactiveTimePeriod).AddSeconds(30)),
                new Tuple<NetworkAddress, DateTimeOffset>(activeNetworkAddressFour, dateTimeProvider.GetTimeOffset().AddSeconds(-inactiveTimePeriod).AddSeconds(40))
            };

            IPeerAddressManager peerAddressManager = this.CreateTestPeerAddressManager(testDataSet);
            NodeSettings nodeSettings = NodeSettings.Default();
            nodeSettings.DnsActivePeerThresholdInSeconds = inactiveTimePeriod;

            WhitelistManager whitelistManager = new WhitelistManager(dateTimeProvider, loggerFactory, peerAddressManager, nodeSettings);

            // Act.
            whitelistManager.RefreshWhitelist();

            // Assert.
            IEnumerable<PeerAddress> whitelist = whitelistManager.Whitelist;
            whitelist.Should().NotBeNullOrEmpty();
            whitelist.Should().HaveSameCount(testDataSet);

            foreach (Tuple<NetworkAddress, DateTimeOffset> testData in testDataSet)
            {
                whitelist.SingleOrDefault(w => w.NetworkAddress.Endpoint.Match(testData.Item1.Endpoint)).Should().NotBeNull("the peer should be in the whitelist as it is active");
            }
        }

        [Fact]
        [Trait("DNS", "UnitTest")]
        public void WhenRefreshWhitelist_AndOwnIPInPeers_AndNotRunningFullNode_ThenWhitelistDoesNotContainOwnIP()
        {
            // Arrange.
            Mock<IDateTimeProvider> mockDateTimeProvider = new Mock<IDateTimeProvider>();

            Mock<ILogger> mockLogger = new Mock<ILogger>();
            Mock<ILoggerFactory> mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
            ILoggerFactory loggerFactory = mockLoggerFactory.Object;

            mockDateTimeProvider.Setup(d => d.GetTimeOffset()).Returns(new DateTimeOffset(new DateTime(2017, 8, 30, 1, 2, 3))).Verifiable();
            IDateTimeProvider dateTimeProvider = mockDateTimeProvider.Object;

            int inactiveTimePeriod = 5000;

            IPAddress activeIpAddressOne = IPAddress.Parse("::ffff:192.168.0.1");
            NetworkAddress activeNetworkAddressOne = new NetworkAddress(activeIpAddressOne, 80);

            IPAddress activeIpAddressTwo = IPAddress.Parse("::ffff:192.168.0.2");
            NetworkAddress activeNetworkAddressTwo = new NetworkAddress(activeIpAddressTwo, 80);

            IPAddress activeIpAddressThree = IPAddress.Parse("::ffff:192.168.0.3");
            NetworkAddress activeNetworkAddressThree = new NetworkAddress(activeIpAddressThree, 80);

            List<Tuple<NetworkAddress, DateTimeOffset>> activeTestDataSet = new List<Tuple<NetworkAddress, DateTimeOffset>>()
            {
                new Tuple<NetworkAddress, DateTimeOffset> (activeNetworkAddressOne,  dateTimeProvider.GetTimeOffset().AddSeconds(-inactiveTimePeriod).AddSeconds(10)),
                new Tuple<NetworkAddress, DateTimeOffset>(activeNetworkAddressTwo, dateTimeProvider.GetTimeOffset().AddSeconds(-inactiveTimePeriod).AddSeconds(20)),
                new Tuple<NetworkAddress, DateTimeOffset>(activeNetworkAddressThree, dateTimeProvider.GetTimeOffset().AddSeconds(-inactiveTimePeriod).AddSeconds(30)),
            };

            IPAddress externalIPAdress = IPAddress.Parse("::ffff:192.168.99.99");
            int externalPort = 80;
            NetworkAddress externalNetworkAddress = new NetworkAddress(externalIPAdress, externalPort);

            List<Tuple<NetworkAddress, DateTimeOffset>> externalIPTestDataSet = new List<Tuple<NetworkAddress, DateTimeOffset>>()
            {
                new Tuple<NetworkAddress, DateTimeOffset> (externalNetworkAddress,  dateTimeProvider.GetTimeOffset().AddSeconds(-inactiveTimePeriod).AddSeconds(40)),
            };

            IPeerAddressManager peerAddressManager = this.CreateTestPeerAddressManager(activeTestDataSet.Union(externalIPTestDataSet).ToList());

            string[] args = new string[] {
                $"-dnspeeractivethreshold={inactiveTimePeriod.ToString()}",
                $"-externalip={externalNetworkAddress.Endpoint.Address.ToString()}",
                $"-port={externalPort.ToString()}",
            };

            Network network = Network.StratisTest;
            NodeSettings nodeSettings = new NodeSettings(network.Name, network).LoadArguments(args);
            nodeSettings.DnsActivePeerThresholdInSeconds = inactiveTimePeriod;
            nodeSettings.DnsFullNode = false;

            WhitelistManager whitelistManager = new WhitelistManager(dateTimeProvider, loggerFactory, peerAddressManager, nodeSettings);

            // Act.
            whitelistManager.RefreshWhitelist();

            // Assert.
            IEnumerable<PeerAddress> whitelist = whitelistManager.Whitelist;
            whitelist.Should().NotBeNullOrEmpty();
            whitelist.Should().HaveSameCount(activeTestDataSet);

            // Active peers.
            foreach (Tuple<NetworkAddress, DateTimeOffset> testData in activeTestDataSet)
            {
                whitelist.SingleOrDefault(w => w.NetworkAddress.Endpoint.Match(testData.Item1.Endpoint)).Should().NotBeNull("the peer should be in the whitelist as it is active");
            }

            // External IP.
            whitelist.SingleOrDefault(w => w.NetworkAddress.Endpoint.Match(externalNetworkAddress.Endpoint)).Should().BeNull("the external IP peer should not be in the whitelist as it is inactive");
        }

        [Fact]
        [Trait("DNS", "UnitTest")]
        public void WhenRefreshWhitelist_AndOwnIPInPeers_AndAreRunningFullNode_ThenWhitelistDoesContainOwnIP()
        {
            // Arrange.
            Mock<IDateTimeProvider> mockDateTimeProvider = new Mock<IDateTimeProvider>();

            Mock<ILogger> mockLogger = new Mock<ILogger>();
            Mock<ILoggerFactory> mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
            ILoggerFactory loggerFactory = mockLoggerFactory.Object;

            mockDateTimeProvider.Setup(d => d.GetTimeOffset()).Returns(new DateTimeOffset(new DateTime(2017, 8, 30, 1, 2, 3))).Verifiable();
            IDateTimeProvider dateTimeProvider = mockDateTimeProvider.Object;

            int inactiveTimePeriod = 5000;

            IPAddress activeIpAddressOne = IPAddress.Parse("::ffff:192.168.0.1");
            NetworkAddress activeNetworkAddressOne = new NetworkAddress(activeIpAddressOne, 80);

            IPAddress activeIpAddressTwo = IPAddress.Parse("::ffff:192.168.0.2");
            NetworkAddress activeNetworkAddressTwo = new NetworkAddress(activeIpAddressTwo, 80);

            IPAddress activeIpAddressThree = IPAddress.Parse("::ffff:192.168.0.3");
            NetworkAddress activeNetworkAddressThree = new NetworkAddress(activeIpAddressThree, 80);

            IPAddress externalIPAdress = IPAddress.Parse("::ffff:192.168.99.99");
            int externalPort = 80;
            NetworkAddress externalNetworkAddress = new NetworkAddress(externalIPAdress, externalPort);
            
            List<Tuple<NetworkAddress, DateTimeOffset>> activeTestDataSet = new List<Tuple<NetworkAddress, DateTimeOffset>>()
            {
                new Tuple<NetworkAddress, DateTimeOffset> (activeNetworkAddressOne,  dateTimeProvider.GetTimeOffset().AddSeconds(-inactiveTimePeriod).AddSeconds(10)),
                new Tuple<NetworkAddress, DateTimeOffset>(activeNetworkAddressTwo, dateTimeProvider.GetTimeOffset().AddSeconds(-inactiveTimePeriod).AddSeconds(20)),
                new Tuple<NetworkAddress, DateTimeOffset>(activeNetworkAddressThree, dateTimeProvider.GetTimeOffset().AddSeconds(-inactiveTimePeriod).AddSeconds(30)),
                new Tuple<NetworkAddress, DateTimeOffset> (externalNetworkAddress,  dateTimeProvider.GetTimeOffset().AddSeconds(-inactiveTimePeriod).AddSeconds(40))
            };

            IPeerAddressManager peerAddressManager = this.CreateTestPeerAddressManager(activeTestDataSet);

            string[] args = new string[] {
                $"-dnspeeractivethreshold={inactiveTimePeriod.ToString()}",
                $"-externalip={externalNetworkAddress.Endpoint.Address.ToString()}",
                $"-port={externalPort.ToString()}",
            };

            Network network = Network.StratisTest;
            NodeSettings nodeSettings = new NodeSettings(network.Name, network).LoadArguments(args);
            nodeSettings.DnsFullNode = true;
            nodeSettings.DnsActivePeerThresholdInSeconds = inactiveTimePeriod;

            WhitelistManager whitelistManager = new WhitelistManager(dateTimeProvider, loggerFactory, peerAddressManager, nodeSettings);

            // Act.
            whitelistManager.RefreshWhitelist();

            // Assert.
            IEnumerable<PeerAddress> whitelist = whitelistManager.Whitelist;
            whitelist.Should().NotBeNullOrEmpty();
            whitelist.Should().HaveSameCount(activeTestDataSet);

            // Active peers.
            foreach (Tuple<NetworkAddress, DateTimeOffset> testData in activeTestDataSet)
            {
                whitelist.SingleOrDefault(w => w.NetworkAddress.Endpoint.Match(testData.Item1.Endpoint)).Should().NotBeNull("the peer should be in the whitelist as it is active");
            }            
        }

        [Fact]
        [Trait("DNS", "UnitTest")]
        public void WhenRefreshWhitelist_AndInactivePeersInWhitelist_ThenWhitelistDoesNotContainInActivePeers()
        {
            // Arrange.
            Mock<IDateTimeProvider> mockDateTimeProvider = new Mock<IDateTimeProvider>();

            Mock<ILogger> mockLogger = new Mock<ILogger>();
            Mock<ILoggerFactory> mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
            ILoggerFactory loggerFactory = mockLoggerFactory.Object;

            mockDateTimeProvider.Setup(d => d.GetTimeOffset()).Returns(new DateTimeOffset(new DateTime(2017, 8, 30, 1, 2, 3))).Verifiable();
            IDateTimeProvider dateTimeProvider = mockDateTimeProvider.Object;

            int inactiveTimePeriod = 3000;

            IPAddress activeIpAddressOne = IPAddress.Parse("::ffff:192.168.0.1");
            NetworkAddress activeNetworkAddressOne = new NetworkAddress(activeIpAddressOne, 80);

            IPAddress activeIpAddressTwo = IPAddress.Parse("::ffff:192.168.0.2");
            NetworkAddress activeNetworkAddressTwo = new NetworkAddress(activeIpAddressTwo, 80);

            IPAddress activeIpAddressThree = IPAddress.Parse("::ffff:192.168.0.3");
            NetworkAddress activeNetworkAddressThree = new NetworkAddress(activeIpAddressThree, 80);

            IPAddress activeIpAddressFour = IPAddress.Parse("::ffff:192.168.0.4");
            NetworkAddress activeNetworkAddressFour = new NetworkAddress(activeIpAddressFour, 80);

            List<Tuple<NetworkAddress, DateTimeOffset>> testDataSet = new List<Tuple<NetworkAddress, DateTimeOffset>>()
            {
                new Tuple<NetworkAddress, DateTimeOffset> (activeNetworkAddressOne,  dateTimeProvider.GetTimeOffset().AddSeconds(-inactiveTimePeriod).AddSeconds(10)),
                new Tuple<NetworkAddress, DateTimeOffset>(activeNetworkAddressTwo, dateTimeProvider.GetTimeOffset().AddSeconds(-inactiveTimePeriod).AddSeconds(20)),
                new Tuple<NetworkAddress, DateTimeOffset>(activeNetworkAddressThree, dateTimeProvider.GetTimeOffset().AddSeconds(-inactiveTimePeriod).AddSeconds(30)),
                new Tuple<NetworkAddress, DateTimeOffset>(activeNetworkAddressFour, dateTimeProvider.GetTimeOffset().AddSeconds(-inactiveTimePeriod).AddSeconds(40))
            };

            IPeerAddressManager peerAddressManager = this.CreateTestPeerAddressManager(testDataSet);
            NodeSettings nodeSettings = NodeSettings.Default();
            nodeSettings.DnsActivePeerThresholdInSeconds = inactiveTimePeriod;

            WhitelistManager whitelistManager = new WhitelistManager(dateTimeProvider, loggerFactory, peerAddressManager, nodeSettings);

            // Act.
            whitelistManager.RefreshWhitelist();

            // Assert.
            IEnumerable<PeerAddress> whitelist = whitelistManager.Whitelist;
            whitelist.Should().NotBeNullOrEmpty();
            whitelist.Should().HaveSameCount(testDataSet);

            foreach (Tuple<NetworkAddress, DateTimeOffset> testData in testDataSet)
            {
                whitelist.SingleOrDefault(w => w.NetworkAddress.Endpoint.Match(testData.Item1.Endpoint)).Should().NotBeNull("the peer should be in the whitelist as it is active");
            }
        }

        private IPeerAddressManager CreateTestPeerAddressManager(List<Tuple<NetworkAddress, DateTimeOffset>> testDataSet)
        {
            ConcurrentDictionary<IPEndPoint, PeerAddress> peers = new ConcurrentDictionary<IPEndPoint, PeerAddress>();

            string dataFolderDirectory = Path.Combine(AppContext.BaseDirectory, "WhitelistTests");

            if (Directory.Exists(dataFolderDirectory))
            {
                Directory.Delete(dataFolderDirectory, true);
            }
            Directory.CreateDirectory(dataFolderDirectory);

            var peerFolder = new DataFolder(new NodeSettings { DataDir = dataFolderDirectory });

            IPeerAddressManager peerAddressManager = new PeerAddressManager(peerFolder);

            foreach (Tuple<NetworkAddress, DateTimeOffset> testData in testDataSet)
            {
                peerAddressManager.AddPeer(testData.Item1, IPAddress.Loopback);
                peerAddressManager.PeerHandshaked(testData.Item1.Endpoint, testData.Item2);
            }

            return peerAddressManager;
        }
    }
}