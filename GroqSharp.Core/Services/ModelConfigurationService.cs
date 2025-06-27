using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GroqSharp.Core.Services
{
    public class ModelConfigurationService
    {
        private readonly string _configPath;
        private readonly IConfiguration _config;

        public ModelConfigurationService(IConfiguration config, string configPath = "appsettings.json")
        {
            _config = config;
            _configPath = configPath;
        }

        public string GetModel()
        {
            if (!File.Exists(_configPath))
                return GetFallbackModel();

            var jsonText = File.ReadAllText(_configPath);
            var root = JsonNode.Parse(jsonText);
            var model = root?[GroqConfigKeys.Root]?[GroqConfigKeys.DefaultModel]?.ToString();

            return !string.IsNullOrWhiteSpace(model) ? model : GetFallbackModel();
        }

        public void SetModel(string newModel)
        {
            if (!File.Exists(_configPath))
                throw new FileNotFoundException("appsettings.json not found", _configPath);

            var jsonText = File.ReadAllText(_configPath);
            var root = JsonNode.Parse(jsonText) ?? new JsonObject();

            var groqSection = root[GroqConfigKeys.Root]?.AsObject() ?? new JsonObject();
            groqSection[GroqConfigKeys.DefaultModel] = newModel;
            root[GroqConfigKeys.Root] = groqSection;

            var formatted = root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, formatted);
        }

        private string GetFallbackModel()
        {
            return _config.GetSection(GroqConfigKeys.FullDefaultModel)?.Value ?? "llama-3.3-70b-versatile";
        }
    }

    public static class GroqConfigKeys
    {
        public const string Root = "Groq";
        public const string DefaultModel = "DefaultModel";
        public const string FullDefaultModel = Root + ":" + DefaultModel;
    }
}
