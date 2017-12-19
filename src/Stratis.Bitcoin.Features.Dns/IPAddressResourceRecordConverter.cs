using System;
using System.Net;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stratis.Bitcoin.Features.Dns
{
    /// <summary>
    /// Defines a <see cref="JsonConverter"/> implementation for an <see cref="IPAddressResourceRecord"/> object.
    /// </summary>
    public class IPAddressResourceRecordConverter : JsonConverter
    {
        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">The type of object to convert.</param>
        /// <returns><c>True</c> if the object can be converted otherwise returns false.</returns>
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(IPAddressResourceRecord) || objectType == typeof(IResourceRecord));
        }
        
        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">The type of object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            IPAddress ipaddress = IPAddress.Parse(jObject["IPAddress"].Value<string>());
            Domain domain = new Domain(jObject["Name"].Value<string>());

            return new IPAddressResourceRecord(domain, ipaddress);
        }
        
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The Newtonsoft.Json.JsonWriter to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IPAddressResourceRecord resourceRecord = (IPAddressResourceRecord)value;
            JObject jObject = new JObject
            {
                { "IPAddress", resourceRecord.IPAddress.ToString() },
                { "Name", resourceRecord.Name.ToString()}
            };

            jObject.WriteTo(writer);
        }
    }
}
