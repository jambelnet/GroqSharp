using Microsoft.Extensions.Configuration;

namespace GroqSharp.Core.Helpers
{
    public static class ConfigurationHelper
    {
        public static async Task<IConfiguration> InitializeAsync(string fileName = "appsettings.json")
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(fileName, optional: true)
                .Build();

            if (string.IsNullOrWhiteSpace(config["Groq:ApiKey"]))
            {
                Console.WriteLine("Configuration missing or incomplete. Starting setup...");
                await SetupService.RunInitialSetupAsync(fileName);

                config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(fileName, optional: false)
                    .Build();
            }

            return config;
        }
    }
}
