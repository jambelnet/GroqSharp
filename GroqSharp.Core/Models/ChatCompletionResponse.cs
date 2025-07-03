namespace GroqSharp.Core.Models
{
    public class ChatCompletionResponse
    {
        public List<Choice> Choices { get; set; } = new();

        public class Choice
        {
            public Message? Message { get; set; }
        }
    }
}
