using System;
using DNS.Protocol.ResourceRecords;
using Newtonsoft.Json;

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
            return (objectType == typeof(IPAddressResourceRecord));
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
            return serializer.Deserialize(reader, typeof(IPAddressResourceRecord));
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The Newtonsoft.Json.JsonWriter to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value, typeof(IPAddressResourceRecord));
        }
    }
}
