using Xceed.Words.NET;

namespace GroqSharp.Core.Utilities
{
    public static class DocxTextExtractor
    {
        public static string ExtractText(string filePath)
        {
            using (var doc = DocX.Load(filePath))
            {
                return doc.Text;
            }
        }
    }
}
