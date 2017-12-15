using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Stratis.Bitcoin.P2P;
using Stratis.Bitcoin.Utilities;
using Xunit;

namespace Stratis.Bitcoin.Features.Dns.Tests
{
    /// <summary>
    /// Tests for the <see cref="DnsFeature"/> class.
    /// </summary>
    public class GivenADnsFeature
    {
        [Fact]
        [Trait("DNS", "UnitTest")]
        public void WhenConstructorCalled_AndPeerAddressManagerIsNull_ThenArgumentNullExceptionIsThrown()
        {
            // Arrange.
            Action a = () => { new DnsFeature(null, null, null, null); };

            // Act and Assert.
            a.ShouldThrow<ArgumentNullException>().Which.Message.Should().Contain("whitelistManager");
        }

        [Fact]
        [Trait("DNS", "UnitTest")]
        public void WhenConstructorCalled_AndLoggerFactoryIsNull_ThenArgumentNullExceptionIsThrown()
        {
            // Arrange.
            IWhitelistManager whitelistManager = new Mock<IWhitelistManager>().Object;
            Action a = () => { new DnsFeature(whitelistManager, null, null, null); };

            // Act and Assert.
            a.ShouldThrow<ArgumentNullException>().Which.Message.Should().Contain("loggerFactory");
        }
        
        [Fact]
        [Trait("DNS", "UnitTest")]
        public void WhenConstructorCalled_AndAsyncLoopFactoryIsNull_ThenArgumentNullExceptionIsThrown()
        {
            // Arrange.
            IWhitelistManager whitelistManager = new Mock<IWhitelistManager>().Object;
            ILoggerFactory loggerFactory = new Mock<ILoggerFactory>().Object;
            
            Action a = () => { new DnsFeature(whitelistManager, loggerFactory, null, null); };

            // Act and Assert.
            a.ShouldThrow<ArgumentNullException>().Which.Message.Should().Contain("asyncLoopFactory");
        }

        [Fact]
        [Trait("DNS", "UnitTest")]
        public void WhenConstructorCalled_AndNodeLifetimeIsNull_ThenArgumentNullExceptionIsThrown()
        {
            // Arrange.
            IWhitelistManager whitelistManager = new Mock<IWhitelistManager>().Object;
            ILoggerFactory loggerFactory = new Mock<ILoggerFactory>().Object;
            IAsyncLoopFactory asyncLoopFactory = new Mock<IAsyncLoopFactory>().Object;

            Action a = () => { new DnsFeature(whitelistManager, loggerFactory, asyncLoopFactory, null); };

            // Act and Assert.
            a.ShouldThrow<ArgumentNullException>().Which.Message.Should().Contain("nodeLifetime");
        }

        [Fact]
        [Trait("Dns", "UnitTest")]
        public void WhenConstructorCalled_AndAllParametersValid_ThenTypeCreated()
        {
            // Arrange.
            IWhitelistManager whitelistManager = new Mock<IWhitelistManager>().Object;
            ILoggerFactory loggerFactory = new Mock<ILoggerFactory>().Object;
            IAsyncLoopFactory asyncLoopFactory = new Mock<IAsyncLoopFactory>().Object;
            INodeLifetime nodeLifetime = new Mock<INodeLifetime>().Object;

            // Act.
            DnsFeature feature = new DnsFeature(whitelistManager, loggerFactory, asyncLoopFactory, nodeLifetime);

            // Assert.
            feature.Should().NotBeNull();
        }
    }
}
