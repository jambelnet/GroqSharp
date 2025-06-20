﻿using GroqSharp.CLI.Commands.Interfaces;
using GroqSharp.CLI.Commands.Models;

namespace GroqSharp.CLI.Commands.Handlers
{
    public class HelpCommandHandler : ICommandProcessor
    {
        public async Task<bool> ProcessCommand(string command, string[] args, CommandContext context)
        {
            if (!command.Equals("/help", StringComparison.OrdinalIgnoreCase))
                return false;

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Available commands:");
            Console.WriteLine("  /models     - Show available models");
            Console.WriteLine("  /setmodel   - Change current model");
            Console.WriteLine("  /stream     - Start a streaming chat session");
            Console.WriteLine("  /new        - Start a new chat session");
            Console.WriteLine("  /history    - Show previous conversation messages");
            Console.WriteLine("  /clear      - Clear the current session");
            Console.WriteLine("  /archive    - Manage saved conversations (list, load, delete, rename)");
            Console.WriteLine("  /process    - Process a local file");
            Console.WriteLine("  /export     - Export AI output to file");
            Console.WriteLine("  /transcribe - Transcribe audio file to text");
            Console.WriteLine("  /speak      - Convert text to speech (and save audio)");
            Console.WriteLine("  /vision     - Analyze image content using LLM");
            Console.WriteLine("  /exit       - Quit the application");
            Console.WriteLine("  /help       - Show this help");
            Console.ResetColor();

            return true;
        }

        public IEnumerable<string> GetAvailableCommands() => new[] { "/help" };
    }
}
