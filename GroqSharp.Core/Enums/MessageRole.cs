using GroqSharp.Core.Converters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace GroqSharp.Core.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverterWithLowerCase))]
    public enum MessageRole
    {
        [EnumMember(Value = "user")]
        User,
        [EnumMember(Value = "assistant")]
        Assistant,
        [EnumMember(Value = "system")]
        System,
        [EnumMember(Value = "tool")]
        Tool
    }
}
