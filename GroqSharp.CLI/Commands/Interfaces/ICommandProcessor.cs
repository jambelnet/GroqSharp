using GroqSharp.CLI.Commands.Models;

namespace GroqSharp.CLI.Commands.Interfaces
{
    public interface ICommandProcessor
    {
        Task<bool> ProcessCommand(string command, string[] args, CliSessionContext context);
        IEnumerable<string> GetAvailableCommands();
    }
}
