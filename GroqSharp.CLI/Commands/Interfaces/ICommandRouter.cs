using GroqSharp.CLI.Commands.Models;

namespace GroqSharp.CLI.Commands.Interfaces
{
    public interface ICommandRouter
    {
        /// <summary>
        /// Routes a full command line input to the appropriate command handler.
        /// Executes the command without expecting a return value.
        /// </summary>
        /// <param name="input">The full command line input, e.g. "/vision https://url image prompt"</param>
        /// <param name="context">The current command execution context.</param>
        /// <returns>True if a handler was found and executed successfully; otherwise, false.</returns>
        Task<bool> RouteCommand(string input, CommandContext context);

        /// <summary>
        /// Routes and executes a command, returning any string output (e.g. for export).
        /// </summary>
        /// <param name="commandLine">The command line input to execute.</param>
        /// <param name="context">The current command execution context.</param>
        /// <returns>The output string if produced by the command; otherwise, null.</returns>
        Task<string?> RunCommandAsync(string commandLine, CommandContext context);
    }
}
