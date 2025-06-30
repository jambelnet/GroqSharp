using GroqSharp.CLI.Commands.Models;
using GroqSharp.CLI.Extensions;
using GroqSharp.CLI.Services;
using GroqSharp.Core.Extensions;
using GroqSharp.Core.Helpers;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            var config = await ConfigurationHelper.InitializeAsync(); // move to Core or CLI.Util
            var services = new ServiceCollection();

            services.AddGroqSharpCore(config);
            services.AddGroqSharpCommands();
            services.AddScoped<CliSessionContext>();

            using var provider = services.BuildServiceProvider();
            await CliAppRunner.RunAsync(provider); // central command loop
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Fatal error: {ex.Message}");
            Console.ResetColor();
        }
    }
}
