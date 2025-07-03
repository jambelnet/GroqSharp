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

        public async Task<bool> ProcessCommand(string command, string[] args, CliSessionContext context)
        {
            if (!command.Equals("/archive", StringComparison.OrdinalIgnoreCase))
                return false;

            if (args.Length == 0)
            {
                PrintUsage();
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
                    await HandleLoad(args, context);
                    break;

                case "delete":
                    await HandleDelete(args);
                    break;

                case "rename":
                    await HandleRename(args);
                    break;

                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Unknown subcommand. Use: list, load, delete, rename.");
                    Console.ResetColor();
                    break;
            }

            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/archive" };

        private void PrintUsage()
        {
            Console.WriteLine("Usage: /archive list|load [id]|delete [id]|rename [id] [new name]");
        }

        private void PrintArchiveList(List<ConversationMeta> archives)
        {
            if (archives.Count == 0)
            {
                Console.WriteLine("No saved conversations found.");
                return;
            }

            for (int i = 0; i < archives.Count; i++)
            {
                Console.WriteLine($"{i + 1}. [{archives[i].SessionId}] {archives[i].Title}");
                Console.WriteLine($"   Last Modified: {archives[i].LastModified}");
                Console.WriteLine($"   Preview: {archives[i].Preview}");
            }
        }

        private async Task HandleLoad(string[] args, CliSessionContext context)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Specify archive ID to load.");
                return;
            }

            var session = await _conversationService.GetOrCreateSessionAsync(args[1]);
            await context.InitializeAsync(args[1], session.Title);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Loaded conversation: {session.Title}");
            Console.ResetColor();
        }

        private async Task HandleDelete(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Specify archive ID to delete.");
                return;
            }

            if (await _conversationService.DeleteConversationAsync(args[1]))
                Console.WriteLine("Conversation deleted.");
            else
                Console.WriteLine("Conversation not found.");
        }

        private async Task HandleRename(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: /archive rename [id] [new name]");
                return;
            }

            if (await _conversationService.RenameConversationAsync(args[1], args[2]))
                Console.WriteLine("Conversation renamed.");
            else
                Console.WriteLine("Conversation not found or rename failed.");
        }
    }
}
