using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;
using GroqSharp.Core.Interfaces;
using GroqSharp.Core.Models;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class ArchiveCommandHandler : ICommandProcessor
    {
        private readonly IGlobalConversationService _conversationService;

        public ArchiveCommandHandler(IGlobalConversationService conversationService)
        {
            _conversationService = conversationService;
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
            var archives = await _conversationService.ListAllConversationsAsync();

            switch (subCommand)
            {
                case "list":
                    PrintArchiveList(archives);
                    break;

                case "load":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Specify archive ID to load.");
                        break;
                    }
                    var session = await _conversationService.GetOrCreateSessionAsync(args[1]);
                    await context.InitializeSession(args[1], session.Title);
                    Console.WriteLine($"Loaded conversation: {session.Title}");
                    break;

                case "delete":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Specify archive ID to delete.");
                        break;
                    }
                    if (await _conversationService.DeleteConversationAsync(args[1]))
                    {
                        Console.WriteLine("Conversation deleted.");
                    }
                    else
                    {
                        Console.WriteLine("Conversation not found.");
                    }
                    break;

                case "rename":
                    if (args.Length < 3)
                    {
                        Console.WriteLine("Usage: /archive rename [id] [new name]");
                        break;
                    }
                    if (await _conversationService.RenameConversationAsync(args[1], args[2]))
                        Console.WriteLine("Conversation renamed.");
                    else
                        Console.WriteLine("Conversation not found or rename failed.");
                    break;

                default:
                    Console.WriteLine("Unknown subcommand. Use list, load, delete, rename.");
                    break;
            }

            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/archive" };

        private void PrintArchiveList(List<ConversationMeta> archives)
        {
            for (int i = 0; i < archives.Count; i++)
            {
                Console.WriteLine($"{i + 1}. [{archives[i].SessionId}] {archives[i].Title}");
                Console.WriteLine($"   Last Modified: {archives[i].LastModified}");
                Console.WriteLine($"   Preview: {archives[i].Preview}");
            }
        }
    }
}
