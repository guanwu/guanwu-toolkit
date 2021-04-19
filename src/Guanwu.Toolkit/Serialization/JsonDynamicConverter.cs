using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Guanwu.Toolkit.Serialization
{
    public class JsonDynamicConverter : JsonConverter<dynamic>
    {
        public override dynamic Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.False)
                return false;
            if (reader.TokenType == JsonTokenType.None)
                return string.Empty;
            if (reader.TokenType == JsonTokenType.Null)
                return null;
            if (reader.TokenType == JsonTokenType.Number)
                return reader.TryGetInt64(out long l) ? l : reader.GetDecimal();
            if (reader.TokenType == JsonTokenType.String)
                return reader.GetString();
            if (reader.TokenType == JsonTokenType.True)
                return true;

            if (reader.TokenType == JsonTokenType.StartObject)
                return ReadObject(ReadRootElement(ref reader));
            if (reader.TokenType == JsonTokenType.StartArray)
                return ReadArray(ReadRootElement(ref reader));

            return ReadRootElement(ref reader).Clone();
        }

        private JsonElement ReadRootElement(ref Utf8JsonReader reader)
        {
            return JsonDocument.ParseValue(ref reader).RootElement;
        }

        private object ReadObject(JsonElement element)
        {
            var item = new ExpandoObject();
            foreach (var obj in element.EnumerateObject()) {
                var key = obj.Name;
                var value = ReadValue(obj.Value);
                item.TryAdd(key, value);
            }
            return item;
        }

        private object ReadValue(JsonElement element)
        {
            switch (element.ValueKind) {
                case JsonValueKind.Array:
                    return ReadArray(element);
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                    return null;
                case JsonValueKind.Number:
                    return element.GetDecimal();
                case JsonValueKind.Object:
                    return ReadObject(element);
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.Undefined:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(element));
            }
        }

        private object ReadArray(JsonElement element)
        {
            var list = new List<object>();
            foreach (var item in element.EnumerateArray())
                list.Add(ReadValue(item));
            return list.ToArray();
        }

        public override void Write(
            Utf8JsonWriter writer,
            dynamic value,
            JsonSerializerOptions options)
        {
            // throw new NotImplementedException();
        }
    }

}
