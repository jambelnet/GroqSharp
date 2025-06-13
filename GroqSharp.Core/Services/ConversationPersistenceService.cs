using GroqSharp.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GroqSharp.Services;

public class ConversationPersistenceService
{
    private readonly string _storagePath;

    public ConversationPersistenceService()
    {
        _storagePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "GroqSharp",
            "conversations");

        Directory.CreateDirectory(_storagePath);
    }

    public void SaveConversation(List<Message> messages)
    {
        if (messages == null || messages.Count == 0)
            return;

        string title = Slugify(messages.First().Content);
        string guid = Guid.NewGuid().ToString("N")[..8];
        string fileName = $"{title}__{guid}.json";

        var filePath = Path.Combine(_storagePath, fileName);
        File.WriteAllText(filePath, JsonSerializer.Serialize(messages));
    }

    public void SaveConversation(string fileName, List<Message> messages)
    {
        if (string.IsNullOrWhiteSpace(fileName) || messages == null || messages.Count == 0)
            return;

        var filePath = Path.Combine(_storagePath, $"{fileName}.json");
        File.WriteAllText(filePath, JsonSerializer.Serialize(messages));
    }

    public List<Message> LoadConversation(string fileIdOrIndex)
    {
        var path = ResolveFilePath(fileIdOrIndex);
        return path != null
            ? JsonSerializer.Deserialize<List<Message>>(File.ReadAllText(path)) ?? []
            : [];
    }

    public void DeleteConversation(string fileIdOrIndex)
    {
        var path = ResolveFilePath(fileIdOrIndex);
        if (path != null && File.Exists(path))
            File.Delete(path);
    }

    public List<(string Id, string Title)> ListArchives()
    {
        var files = Directory.GetFiles(_storagePath, "*.json");
        return files.Select(path =>
        {
            var id = Path.GetFileNameWithoutExtension(path);
            var content = File.ReadAllText(path);
            var messages = JsonSerializer.Deserialize<List<Message>>(content);
            var preview = messages?.FirstOrDefault()?.Content?.Trim()?.Substring(0, Math.Min(30, messages.FirstOrDefault()?.Content.Length ?? 0)) ?? "[Empty]";
            return (id, preview);
        }).ToList();
    }

    public bool TryLoadArchive(string idOrIndex, out ConversationService loaded, out string fileName)
    {
        var file = ResolveFilePath(idOrIndex);
        if (file == null)
        {
            loaded = null!;
            fileName = null!;
            return false;
        }

        var messages = JsonSerializer.Deserialize<List<Message>>(File.ReadAllText(file));
        loaded = new ConversationService(10);
        loaded.LoadMessages(messages ?? []);
        fileName = Path.GetFileNameWithoutExtension(file);
        return true;
    }

    public bool DeleteArchive(string idOrIndex)
    {
        var file = ResolveFilePath(idOrIndex);
        if (file == null) return false;

        File.Delete(file);
        return true;
    }

    public bool RenameArchive(string idOrIndex, string newTitle)
    {
        var file = ResolveFilePath(idOrIndex);
        if (file == null) return false;

        var guid = ExtractGuidFromFileName(Path.GetFileNameWithoutExtension(file));
        if (guid == null) return false;

        string newSlug = Slugify(newTitle);
        string newPath = Path.Combine(_storagePath, $"{newSlug}__{guid}.json");

        if (File.Exists(newPath)) return false;

        File.Move(file, newPath);
        return true;
    }

    private string? ResolveFilePath(string idOrIndex)
    {
        var files = Directory.GetFiles(_storagePath, "*.json");

        string? byName = files.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == idOrIndex);
        if (byName != null) return byName;

        if (int.TryParse(idOrIndex, out int idx) && idx > 0 && idx <= files.Length)
            return files[idx - 1];

        return null;
    }

    private string? ExtractGuidFromFileName(string filename)
    {
        var match = Regex.Match(filename, @"__(?<guid>[a-zA-Z0-9]{8})$");
        return match.Success ? match.Groups["guid"].Value : null;
    }

    private string Slugify(string input)
    {
        input = input.ToLowerInvariant().Trim();
        input = Regex.Replace(input, @"[^a-z0-9\s-]", ""); // remove special chars
        input = Regex.Replace(input, @"\s+", "-");         // replace spaces with dashes
        return input.Length > 40 ? input[..40] : input;    // limit length
    }
}
