namespace StackExchange.Contrib.Profiling.Storage
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    // https://github.com/okta/okta-sdk-dotnet/blob/master/src/Okta.Sdk/Internal/RecursiveDictionaryConverter.cs
    // this would also work, but seemed "hacky" https://stackoverflow.com/a/19140420/7644876
    public sealed class RecursiveDictionaryConverter : CustomCreationConverter<IDictionary<string, object>>
    {
        /// <inheritdoc />
        public override IDictionary<string, object> Create(Type objectType)
        {
            return new Dictionary<string, object>(StringComparer.Ordinal);
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
            // We want to handle explicit objects and
            // also nested objects (which might be dictionaries)
        {
            return objectType == typeof(object);
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            // Deserialize nested objects as dictionaries
            var isObject = reader.TokenType == JsonToken.StartObject || reader.TokenType == JsonToken.Null;
            if (isObject)
            {
                return base.ReadJson(reader, objectType, existingValue, serializer);
            }

            // Deserialize arrays as List<object>
            var isArray = reader.TokenType == JsonToken.StartArray;
            if (isArray)
            {
                var list = new List<object>();

                while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                {
                    var listObject = reader.TokenType == JsonToken.StartObject
                        ? base.ReadJson(reader, objectType, existingValue, serializer)
                        : serializer.Deserialize(reader);

                    list.Add(listObject);
                }

                return list;
            }

            // If not, fall back to standard deserialization (for numbers, etc)
            return serializer.Deserialize(reader);
        }
    }
}