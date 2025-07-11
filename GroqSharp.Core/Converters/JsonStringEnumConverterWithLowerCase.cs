using System.Text.Json;
using System.Text.Json.Serialization;

namespace GroqSharp.Core.Converters
{
    public class JsonStringEnumConverterWithLowerCase : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
            => typeToConvert.IsEnum;

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var converterType = typeof(LowercaseEnumConverter<>).MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(converterType);
        }

        private class LowercaseEnumConverter<T> : JsonConverter<T> where T : struct, Enum
        {
            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => Enum.TryParse<T>(reader.GetString(), true, out var value) ? value : default;

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
                => writer.WriteStringValue(value.ToString().ToLowerInvariant());
        }
    }
}
