namespace GroqSharp.Core.Helpers
{
    public static class ContentHelpers
    {
        public static string AsString(object content) => content?.ToString() ?? string.Empty;

        public static string GetPreview(object content, int maxLength = 50)
        {
            var text = content?.ToString() ?? "";
            return text.Length > maxLength ? text[..maxLength] : text;
        }
    }
}
