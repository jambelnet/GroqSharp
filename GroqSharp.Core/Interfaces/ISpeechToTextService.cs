namespace GroqSharp.Core.Interfaces
{
    public interface ISpeechToTextService
    {
        Task<string> TranscribeAudioAsync(string filePath, string? model = null);
    }
}
