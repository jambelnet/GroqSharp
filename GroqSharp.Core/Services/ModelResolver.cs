using GroqSharp.Core.Enums;
using GroqSharp.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GroqSharp.Core.Services
{
    public class ModelResolver : IModelResolver
    {
        private readonly IConfiguration _configuration;
        private readonly string _configurationPath;

        public ModelResolver(IConfiguration configuration, string configurationPath = "appsettings.json")
        {
            _configuration = configuration;
            _configurationPath = configurationPath;
        }

        public string GetModelFor(GroqFeature feature)
        {
            var groqSection = _configuration.GetSection("Groq");

            return feature switch
            {
                GroqFeature.Vision => groqSection["DefaultVisionModel"]
                    ?? throw new InvalidOperationException("DefaultVisionModel is missing in configuration."),
                GroqFeature.Speak => groqSection["DefaultTTSModel"]
                    ?? throw new InvalidOperationException("DefaultTTSModel is missing in configuration."),
                GroqFeature.Transcribe
                or GroqFeature.Translate => groqSection["DefaultWhisperModel"]
                    ?? throw new InvalidOperationException("DefaultWhisperModel is missing in configuration."),
                GroqFeature.Reasoning => groqSection["DefaultReasoningModel"]
                    ?? throw new InvalidOperationException("DefaultReasoningModel is missing in configuration."),
                GroqFeature.Agentic => groqSection["DefaultAgenticModel"]
                    ?? throw new InvalidOperationException("DefaultAgenticModel is missing in configuration."),
                GroqFeature.Default => groqSection["DefaultModel"]
                    ?? throw new InvalidOperationException("DefaultModel is missing in configuration."),
                _ => throw new ArgumentOutOfRangeException(nameof(feature), $"Unhandled GroqFeature: {feature}")
            };
        }

        public string GetModel() => GetModelFor(GroqFeature.Default);

        public void SetModel(string newModel)
        {
            if (!File.Exists(_configurationPath))
                throw new FileNotFoundException("appsettings.json not found", _configurationPath);

            var jsonText = File.ReadAllText(_configurationPath);
            var root = JsonNode.Parse(jsonText) ?? new JsonObject();

            var groqSection = root[GroqConfigKeys.Root]?.AsObject() ?? new JsonObject();
            groqSection[GroqConfigKeys.DefaultModel] = newModel;
            root[GroqConfigKeys.Root] = groqSection;

            var formatted = root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configurationPath, formatted);
        }
    }

    public static class GroqConfigKeys
    {
        public const string Root = "Groq";
        public const string DefaultModel = "DefaultModel";
        public const string FullDefaultModel = Root + ":" + DefaultModel;
    }
}
