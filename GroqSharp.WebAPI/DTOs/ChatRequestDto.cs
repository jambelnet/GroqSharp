namespace GroqSharp.WebAPI.DTOs
{
    public class ChatRequestDto
    {
        public string Message { get; set; } = string.Empty;
        public bool Stream { get; set; } = false;
    }
}
