using System;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stratis.Bitcoin.Features.Dns
{
    /// <summary>
    /// Defines a <see cref="JsonConverter"/> implementation for an <see cref="IPEndPoint"/> object.
    /// </summary>
    public class IPEndPointConverter : JsonConverter
    {
        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">The type of object to convert.</param>
        /// <returns><c>True</c> if the object can be converted otherwise returns false.</returns>
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(IPEndPoint));
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
            IPAddress address = jObject["Address"].ToObject<IPAddress>(serializer);
            int port = (int)jObject["Port"];
            return new IPEndPoint(address, port);
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The Newtonsoft.Json.JsonWriter to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IPEndPoint endPoint = (IPEndPoint)value;
            JObject jObject = new JObject();
            jObject.Add("Address", JToken.FromObject(endPoint.Address, serializer));
            jObject.Add("Port", endPoint.Port);
            jObject.WriteTo(writer);
        }
    }
}
