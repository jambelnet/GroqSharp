using GroqSharp.Models;
using System.Text.Json;

namespace GroqSharp.Services
{
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

        public void SaveConversation(string sessionId, List<Message> messages)
        {
            var filePath = Path.Combine(_storagePath, $"{sessionId}.json");
            File.WriteAllText(filePath, JsonSerializer.Serialize(messages));
        }

        public List<Message> LoadConversation(string sessionId)
        {
            var filePath = Path.Combine(_storagePath, $"{sessionId}.json");
            return File.Exists(filePath)
                ? JsonSerializer.Deserialize<List<Message>>(File.ReadAllText(filePath))
                : new List<Message>();
        }

        public void DeleteConversation(string sessionId)
        {
            var filePath = Path.Combine(_storagePath, $"{sessionId}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
