namespace GroqSharp.Core.Interfaces
{
    public interface IVisionService
    {
        Task<string> AnalyzeImageAsync(string imagePathOrUrl, string prompt, string? model = null);
    }
}
