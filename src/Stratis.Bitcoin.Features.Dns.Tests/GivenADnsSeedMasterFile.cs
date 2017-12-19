using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Stratis.Bitcoin.Features.Dns.Tests
{
    /// <summary>
    /// Tests for the<see cref="DnsSeedMasterFile"/> class.
    /// </summary>
    public class GivenADnsSeedMasterFile
    {
        /// <summary>
        /// Creates the serializer for loading and saving the master file contents.
        /// </summary>
        /// <returns></returns>
        private JsonSerializer CreateSerializer()
        {
            var settings = new Newtonsoft.Json.JsonSerializerSettings();
            settings.Converters.Add(new IPAddressResourceRecordConverter());
            settings.Formatting = Formatting.Indented;
            settings.TypeNameHandling = TypeNameHandling.All;

            return JsonSerializer.Create(settings);
        }

        [Fact]
        [Trait("DNS", "UnitTest")]
        public void WhenLoad_AndStreamIsNull_ThenArgumentNullExceptionIsThrown()
        {
            // Arrange.
            DnsSeedMasterFile masterFile = new DnsSeedMasterFile();
            Action a = () => { masterFile.Load(null); };

            // Act and assert.
            a.ShouldThrow<ArgumentNullException>().Which.Message.Should().Contain("stream");
        }

        [Fact]
        [Trait("DNS", "UnitTest")]
        public void WhenLoad_AndStreamContainsEntries_ThenEntriesArePopulated()
        {
            string domainName = "stratis.test.com";

            using (MemoryStream stream = new MemoryStream())
            {
                IList<IPAddressResourceRecord> resourceRecords = new List<IPAddressResourceRecord>()
                {
                    new IPAddressResourceRecord(new Domain(domainName), IPAddress.Parse("::ffff:192.168.0.1")),
                    new IPAddressResourceRecord(new Domain(domainName), IPAddress.Parse("::ffff:192.168.0.2")),
                    new IPAddressResourceRecord(new Domain(domainName), IPAddress.Parse("::ffff:192.168.0.3")),
                    new IPAddressResourceRecord(new Domain(domainName), IPAddress.Parse("::ffff:192.168.0.4"))
                };
                
                JsonSerializer serializer = this.CreateSerializer();
                
                using (var sw = new StreamWriter(stream))
                using (var jsonTextWriter = new JsonTextWriter(sw))
                {
                    serializer.Serialize(jsonTextWriter, resourceRecords);

                    jsonTextWriter.Flush();
                    stream.Seek(0, SeekOrigin.Begin);

                    DnsSeedMasterFile masterFile = new DnsSeedMasterFile();
                    masterFile.Load(stream);
                }

                // TODO: Add in the asserts to check the 2 lists match.
            }
        }

        [Fact]
        [Trait("DNS", "UnitTest")]
        public void WhenSave_AndStreamContainsEntries_ThenEntriesAreSaved()
        {
            // TODO: Add implementation.
        }

        [Fact]
        [Trait("DNS", "UnitTest")]
        public void WhenSave_AndStreamIsNull_ThenArgumentNullExceptionIsThrown()
        {
            // Arrange.
            DnsSeedMasterFile masterFile = new DnsSeedMasterFile();
            Action a = () => { masterFile.Save(null); };

            // Act and assert.
            a.ShouldThrow<ArgumentNullException>().Which.Message.Should().Contain("stream");
        }
    }
}
