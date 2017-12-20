using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            // Arrange.
            using (MemoryStream stream = new MemoryStream())
            {
                string domainName = "stratis.test.com";
                DnsSeedMasterFile masterFile = new DnsSeedMasterFile();

                IList<IPAddressResourceRecord> testResourceRecords = new List<IPAddressResourceRecord>()
                {
                    new IPAddressResourceRecord(new Domain(domainName), IPAddress.Parse("192.168.0.1")),
                    new IPAddressResourceRecord(new Domain(domainName), IPAddress.Parse("192.168.0.2")),
                    new IPAddressResourceRecord(new Domain(domainName), IPAddress.Parse("192.168.0.3")),
                    new IPAddressResourceRecord(new Domain(domainName), IPAddress.Parse("192.168.0.4"))
                };

                JsonSerializer serializer = this.CreateSerializer();

                using (var sw = new StreamWriter(stream))
                using (var jsonTextWriter = new JsonTextWriter(sw))
                {
                    serializer.Serialize(jsonTextWriter, testResourceRecords);

                    jsonTextWriter.Flush();
                    stream.Seek(0, SeekOrigin.Begin);
                    
                    // Act.
                    masterFile.Load(stream);
                }

                // Assert.
                Domain domain = new Domain(domainName);
                Question question = new Question(domain, RecordType.A);

                IList<IResourceRecord> resourceRecords = masterFile.Get(question);
                resourceRecords.Should().NotBeNullOrEmpty();

                IList<IPAddressResourceRecord> ipAddressResourceRecords = resourceRecords.OfType<IPAddressResourceRecord>().ToList();
                ipAddressResourceRecords.Should().HaveSameCount(testResourceRecords);

                foreach (IPAddressResourceRecord testResourceRecord in testResourceRecords)
                {
                    ipAddressResourceRecords.SingleOrDefault(i => i.IPAddress.Equals(testResourceRecord.IPAddress)).Should().NotBeNull();
                }
            }
        }

        [Fact]
        [Trait("DNS", "UnitTest")]
        public void WhenSave_AndStreamContainsEntries_ThenEntriesAreSaved()
        {
            // TODO:
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
