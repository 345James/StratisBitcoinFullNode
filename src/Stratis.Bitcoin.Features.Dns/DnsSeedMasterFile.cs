using System.Collections.Generic;
using System.IO;
using System.Linq;
using DNS.Protocol.ResourceRecords;
using Newtonsoft.Json;
using Stratis.Bitcoin.Utilities;

namespace Stratis.Bitcoin.Features.Dns
{
    /// <summary>
    /// This class defines a DNS masterfile used to cache the whitelisted peers discovered by the DNS Seed service that supports saving
    /// and loading from a stream.
    /// </summary>
    public class DnsSeedMasterFile : MasterFile, IMasterFile
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

        /// <summary>
        /// Loads the saved masterfile from the specified stream.
        /// </summary>
        /// <param name="stream">The stream containing the masterfile.</param>
        public void Load(Stream stream)
        {
            Guard.NotNull(stream, nameof(stream));

            using (JsonTextReader textReader = new JsonTextReader(new StreamReader(stream)))
            {
                JsonSerializer serializer = this.CreateSerializer();                
                List<IPAddressResourceRecord> ipAddressResourceRecords = serializer.Deserialize<List<IPAddressResourceRecord>>(textReader);

                base.entries = ipAddressResourceRecords.ToList<IResourceRecord>();
            }
        }

        /// <summary>
        /// Saves the cached masterfile to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to write the masterfile to.</param>
        public void Save(Stream stream)
        {
            Guard.NotNull(stream, nameof(stream));

            using (JsonTextWriter textWriter = new JsonTextWriter(new StreamWriter(stream)))
            {
                JsonSerializer serializer = this.CreateSerializer();
                
                serializer.Serialize(textWriter, this.entries.OfType<IPAddressResourceRecord>());
                textWriter.Flush();
            }
        }
    }
}
