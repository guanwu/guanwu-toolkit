using System.Collections.Generic;
using System.Text.Json;
using Guanwu.Toolkit.Serialization;

namespace Guanwu.Toolkit.Extensions.Serializer
{
    public static class SerializerExtension
    {
        public static string ToJson(this object input)
        {
            return JsonSerializer.Serialize(input);
        }

        public static object FromJson(this string input)
        {
            var serializerOptions = new JsonSerializerOptions {
                Converters = { new JsonDynamicConverter() }
            };
            return JsonSerializer.Deserialize<dynamic>(input, serializerOptions);
        }

        public static T FromJson<T>(this string input) where T : class
        {
            var serializerOptions = new JsonSerializerOptions {
                Converters = { new JsonDynamicConverter() }
            };
            return JsonSerializer.Deserialize<T>(input, serializerOptions);
        }

        public static object Select(this object input, string path)
        {
            var inputs = input as IDictionary<string, object>;
            return (input != null && inputs.ContainsKey(path))
                ? inputs[path] : default(object);
        }

        public static T Value<T>(this object input) where T : class
        {
            return input as T;
        }

        public static IEnumerable<T> Values<T>(this object inputs) where T : class
        {
            foreach (var input in (dynamic)inputs)
                yield return input as T;
        }

        public static T ToObject<T>(this object input) where T : class
        {
            return FromJson<T>(ToJson(input));
        }
    }
}
