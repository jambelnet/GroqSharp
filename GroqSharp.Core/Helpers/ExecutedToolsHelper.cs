using GroqSharp.Core.Models;

namespace GroqSharp.Core.Helpers
{
    public static class ExecutedToolsHelper
    {
        public static void PatchExecutedTools(this Message message, string? defaultToolName = null, string? defaultInput = null)
        {
            if (message?.ExecutedTools == null)
                return;

            foreach (var tool in message.ExecutedTools)
            {
                if (string.IsNullOrEmpty(tool.ToolName))
                    tool.ToolName = defaultToolName ?? "GroqSharp";

                if (string.IsNullOrEmpty(tool.Input))
                    tool.Input = defaultInput ?? "(no input)";
            }
        }
    }
}
