using iText.Kernel.Pdf;
using System.Text;

namespace GroqSharp.Utilities
{
    public static class PdfTextExtractor
    {
        public static string ExtractText(string filePath)
        {
            var text = new StringBuilder();
            using (var pdfReader = new PdfReader(filePath))
            using (var pdfDoc = new PdfDocument(pdfReader))
            {
                for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
                {
                    var pageText = iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page));
                    text.AppendLine(pageText);
                }
            }
            return text.ToString();
        }
    }
}
