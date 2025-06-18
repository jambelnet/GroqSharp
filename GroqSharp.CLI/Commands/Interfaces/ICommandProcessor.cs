using GroqSharp.CLI.Commands.Models;

namespace GroqSharp.CLI.Commands.Interfaces
{
    public interface ICommandProcessor
    {
        Task<bool> ProcessCommand(string command, string[] args, CommandContext context);
        IEnumerable<string> GetAvailableCommands();
    }
}
