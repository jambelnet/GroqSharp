namespace GroqSharp.Core.Interfaces
{
    public interface IReasoningService
    {
        Task<string> AnalyzeAsync(string prompt, string model = "deepseek-r1-distill-llama-70b", string reasoningFormat = "raw", string reasoningEffort = "default");
    }
}
