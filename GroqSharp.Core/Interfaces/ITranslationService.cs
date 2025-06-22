namespace GroqSharp.Core.Interfaces
{
    public interface ITranslationService
    {
        Task<string> TranslateAudioAsync(string filePath);
    }
}
