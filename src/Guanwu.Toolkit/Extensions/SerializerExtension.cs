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

        public static dynamic FromJson(this string input)
        {
            var serializerOptions = new JsonSerializerOptions {
                Converters = { new JsonDynamicConverter() }
            };
            return JsonSerializer.Deserialize<dynamic>(input, serializerOptions);
        }

        public static dynamic ToDynamic(this object input)
        {
            return FromJson(ToJson(input));
        }
    }

}
