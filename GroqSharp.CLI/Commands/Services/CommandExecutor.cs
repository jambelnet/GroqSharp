using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using Microsoft.Extensions.DependencyInjection;

namespace GroqSharp.CLI.Commands.Services
{
    public class CommandExecutor : ICommandExecutor
    {
        private readonly IServiceProvider _provider;

        public CommandExecutor(IServiceProvider provider)
        {
            _provider = provider;
        }

        public async Task<bool> ExecuteCommand(string input, CliSessionContext context)
        {
            var router = _provider.GetRequiredService<ICommandRouter>();
            return await router.RouteCommand(input, context);
        }
    }
}
