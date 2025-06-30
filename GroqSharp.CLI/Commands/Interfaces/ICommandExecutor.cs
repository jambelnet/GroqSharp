using GroqSharp.CLI.Commands.Models;

namespace GroqSharp.CLI.Commands.Interfaces
{
    public interface ICommandExecutor
    {
        Task<bool> ExecuteCommand(string input, CliSessionContext context);
    }
}
