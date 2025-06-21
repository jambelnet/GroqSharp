namespace GroqSharp.Core.Interfaces
{
    public interface ITextToSpeechService
    {
        Task<byte[]> SynthesizeSpeechAsync(string text, string voice = "Arista-PlayAI");
    }
}
