using iText.Html2pdf;
using iText.Kernel.Pdf;
using System.Text.Json;

namespace GroqSharp.Core.Utilities
{
    public static class FileProcessor
    {
        public static readonly string[] SupportedInputFormats = { ".txt", ".pdf", ".docx", ".html" };
        public static readonly string[] SupportedOutputFormats = { "txt", "html", "pdf", "json" };
        public const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public static string ReadFileContent(string filePath)
        {
            ValidateFile(filePath);
            return File.ReadAllText(filePath);
        }

        public static string ReadFileAsBase64(string filePath)
        {
            ValidateFile(filePath);
            byte[] fileBytes = File.ReadAllBytes(filePath);
            return Convert.ToBase64String(fileBytes);
        }

        public static string ExtractTextFromFile(string filePath)
        {
            ValidateFile(filePath);
            var extension = Path.GetExtension(filePath).ToLower();

            return extension switch
            {
                ".txt" => File.ReadAllText(filePath),
                ".pdf" => PdfTextExtractor.ExtractText(filePath),
                ".docx" => DocxTextExtractor.ExtractText(filePath),
                ".html" or ".htm" => HtmlToTextConverter.Convert(File.ReadAllText(filePath)),
                _ => throw new NotSupportedException($"File type {extension} not supported. Supported types: {string.Join(", ", SupportedInputFormats)}")
            };
        }

        public static void ExportToFile(string content, string outputPath, string format = "txt")
        {
            if (string.IsNullOrWhiteSpace(outputPath))
                return;

            format = format?.ToLower() ?? "txt";

            if (!SupportedOutputFormats.Contains(format))
                throw new NotSupportedException($"Format {format} not supported. Supported formats: {string.Join(", ", SupportedOutputFormats)}");

            var finalPath = Path.HasExtension(outputPath)
                ? Path.ChangeExtension(outputPath, format)
                : $"{outputPath}.{format}";

            switch (format)
            {
                case "txt":
                    File.WriteAllText(finalPath, content);
                    break;

                case "html":
                    File.WriteAllText(finalPath,
                        $"<!DOCTYPE html><html><head><meta charset='UTF-8'></head><body>{content}</body></html>");
                    break;

                case "pdf":
                    var writer = new PdfWriter(finalPath);
                    var pdf = new PdfDocument(writer).GetWriter();
                    HtmlConverter.ConvertToPdf(
                        $"<html><body>{content}</body></html>",
                        pdf);
                    pdf.Close();
                    break;

                case "json":
                    File.WriteAllText(finalPath,
                        JsonSerializer.Serialize(new { Content = content }));
                    break;
            }
        }

        public static void ConvertFileFormat(string inputPath, string outputPath)
        {
            var content = ExtractTextFromFile(inputPath);
            var outputFormat = Path.GetExtension(outputPath).TrimStart('.');
            ExportToFile(content, outputPath, outputFormat);
        }

        private static void ValidateFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > MaxFileSize)
                throw new InvalidOperationException($"File too large. Max size: {MaxFileSize / (1024 * 1024)}MB");

            var extension = Path.GetExtension(filePath).ToLower();
            if (!SupportedInputFormats.Contains(extension))
                throw new NotSupportedException($"File type {extension} not supported");
        }
    }
}