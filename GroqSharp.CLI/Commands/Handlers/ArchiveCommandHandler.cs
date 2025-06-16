using GroqSharp.Commands.Interfaces;
using GroqSharp.Commands.Models;
using GroqSharp.Models;
using GroqSharp.Services;

namespace GroqSharp.Commands.Handlers
{
    public class ArchiveCommandHandler : ICommandProcessor
    {
        private readonly ConversationPersistenceService _persistence;

        public ArchiveCommandHandler(ConversationPersistenceService persistence)
        {
            _persistence = persistence;
        }

        public async Task<bool> ProcessCommand(string command, string[] args, CommandContext context)
        {
            if (!command.Equals("/archive", StringComparison.OrdinalIgnoreCase))
                return false;

            if (args.Length == 0)
            {
                Console.WriteLine("Usage: /archive list|load|delete|rename");
                return true;
            }

            var subCommand = args[0].ToLowerInvariant();
            var archives = _persistence.ListArchives();

            switch (subCommand)
            {
                case "list":
                    PrintArchiveList(archives);
                    break;

                case "load":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Specify archive index or ID to load.");
                        break;
                    }
                    if (_persistence.TryLoadArchive(args[1], out var loaded, out var fileName))
                    {
                        context.Conversation = loaded;
                        context.LoadedArchiveFileName = fileName;
                        Console.WriteLine("Archive loaded.");
                    }
                    else Console.WriteLine("Archive not found.");
                    break;

                case "delete":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Specify archive index or ID to delete.");
                        break;
                    }
                    if (_persistence.DeleteArchive(args[1]))
                        Console.WriteLine("Archive deleted.");
                    else
                        Console.WriteLine("Archive not found.");
                    break;

                case "rename":
                    if (args.Length < 3)
                    {
                        Console.WriteLine("Usage: /archive rename [id/index] [new name]");
                        break;
                    }
                    if (_persistence.RenameArchive(args[1], args[2]))
                        Console.WriteLine("Archive renamed.");
                    else
                        Console.WriteLine("Archive not found or rename failed.");
                    break;

                default:
                    Console.WriteLine("Unknown subcommand. Use list, load, delete, rename.");
                    break;
            }

            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/archive" };

        private void PrintArchiveList(List<Archive> archives)
        {
            for (int i = 0; i < archives.Count; i++)
            {
                Console.WriteLine($"{i + 1}. [{archives[i].Id}] {archives[i].Title}");
                Console.WriteLine($"   Preview: {archives[i].Preview}");
            }
        }
    }
}
