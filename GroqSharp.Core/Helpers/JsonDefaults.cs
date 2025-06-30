using System.Text.Json;
using System.Text.Json.Serialization;

namespace GroqSharp.Core.Helpers
{
    public static class JsonDefaults
    {
        public static readonly JsonSerializerOptions WriteIndented = new()
        {
            WriteIndented = true
        };

        public static readonly JsonSerializerOptions InWhenWritingNulldented = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
}
