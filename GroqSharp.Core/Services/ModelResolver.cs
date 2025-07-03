using GroqSharp.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace GroqSharp.Core.Services
{
    public class ModelResolver : IModelResolver
    {
        private readonly IConfiguration _configuration;

        // Hardcoded fallback defaults
        private const string DefaultModel = "llama-3.3-70b-versatile";
        private const string DefaultVisionModel = "meta-llama/llama-4-scout-17b-16e-instruct";
        private const string DefaultTTSModel = "playai-tts";
        private const string DefaultWhisperModel = "whisper-large-v3-turbo";
        private const string DefaultReasoningModel = "deepseek-r1-distill-llama-70b";
        private const string DefaultAgenticModel = "compound-beta";

        public ModelResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetModelFor(string command)
        {
            var groqSection = _configuration.GetSection("Groq");

            return command switch
            {
                "/vision" => groqSection["DefaultVisionModel"] ?? DefaultVisionModel,
                "/speak" => groqSection["DefaultTTSModel"] ?? DefaultTTSModel,
                "/transcribe" or "/translate" => groqSection["DefaultWhisperModel"] ?? DefaultWhisperModel,
                "/reason" => groqSection["DefaultReasoningModel"] ?? DefaultReasoningModel,
                "/agent" => groqSection["DefaultAgenticModel"] ?? DefaultAgenticModel,
                _ => groqSection["DefaultModel"] ?? DefaultModel
            };
        }
    }
}
