using GroqSharp.Commands.Models;

namespace GroqSharp.Commands.Interfaces
{
    public interface ICommandProcessor
    {
        Task<bool> ProcessCommand(string command, string[] args, CommandContext context);
        IEnumerable<string> GetAvailableCommands();
    }
}
