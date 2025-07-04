using GroqSharp.Core.Models;

namespace GroqSharp.CLI.Utilities
{
    public static class ConsoleOutputHelper
    {
        private static void WriteLine(string message)
        {
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void WriteInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            WriteLine(message);
        }

        public static void WriteError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLine(message);
        }

        public static void WriteHighlight(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            WriteLine(message);
        }

        public static bool ShowError(string message)
        {
            WriteError(message);
            return true;
        }

        public static void DisplayResponse(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return;

            Console.ForegroundColor = ConsoleColor.Green;
            WriteLine("\n--- Response ---");
            WriteLine(content);
        }

        public static void DisplayExecutedTools(List<ExecutedTool>? tools, bool summaryOnly, bool verbose)
        {
            if (tools == null || tools.Count == 0 || (!summaryOnly && !verbose))
                return;

            Console.ForegroundColor = ConsoleColor.Yellow;
            WriteLine("\n--- Executed Tools ---");

            if (summaryOnly)
            {
                var top = tools.First();
                if (top != null)
                {
                    WriteLine($"Tool: {top.ToolName}");
                    WriteLine($"Output: {top.Output}");
                }
            }
            else
            {
                foreach (var tool in tools)
                {
                    WriteToolInfo(tool);
                }
            }
        }

        public static void WriteMessageEntry(Message msg)
        {
            if (msg == null || string.IsNullOrWhiteSpace(msg.Content))
                return;

            Console.ForegroundColor = msg.Role switch
            {
                "user" => ConsoleColor.Cyan,
                "assistant" => ConsoleColor.Green,
                _ => ConsoleColor.Gray
            };

            WriteLine($"[{msg.Role}] {msg.Content}");
        }

        public static void WriteToolInfo(ExecutedTool tool)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"  Tool:   {tool.ToolName}");
            Console.WriteLine($"  Input:  {tool.Input}");
            Console.WriteLine($"  Output: {tool.Output}");
            if (tool.Score.HasValue)
                Console.WriteLine($"  Score:  {tool.Score.Value:F4}");
        }
    }
}
