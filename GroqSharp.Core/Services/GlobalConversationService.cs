using GroqSharp.Core.Enums;
using GroqSharp.Core.Helpers;
using GroqSharp.Core.Interfaces;
using GroqSharp.Core.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace GroqSharp.Core.Services
{
    public class GlobalConversationService : IGlobalConversationService
    {
        private readonly IModelResolver _modelResolver;
        private readonly ConcurrentDictionary<string, ConversationSession> _activeSessions = new();
        private readonly string _storagePath;

        public GlobalConversationService(IModelResolver modelResolver)
        {
            _modelResolver = modelResolver;

            _storagePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GroqSharp",
                "global_conversations");
            Directory.CreateDirectory(_storagePath);
        }

        public async Task<ConversationSession> GetOrCreateSessionAsync(string sessionId)
        {
            if (_activeSessions.TryGetValue(sessionId, out var existing))
                return existing;

            var session = new ConversationSession
            {
                SessionId = sessionId,
                Title = "New Conversation",
                LastModified = DateTime.UtcNow
            };

            await LoadSessionFromStorageAsync(session);
            _activeSessions.TryAdd(sessionId, session);
            return session;
        }

        public async Task SaveSessionAsync(string sessionId, ConversationService conversation)
        {
            if (_activeSessions.TryGetValue(sessionId, out var session))
            {
                try
                {
                    session.LastModified = DateTime.UtcNow;
                    session.Messages = conversation.GetFullHistory().ToList();

                    var filePath = GetSessionFilePath(sessionId);
                    var storageData = new ConversationData
                    {
                        Title = session.Title,
                        LastModified = session.LastModified,
                        Messages = session.Messages ?? new List<Message>(),
                    };

                    await File.WriteAllTextAsync(filePath,
                        JsonSerializer.Serialize(storageData, JsonDefaults.WriteIndented));
                }
                catch (IOException ioEx)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"[Warning] Could not save session '{sessionId}' — file is in use: {ioEx.Message}");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to save session {sessionId}: {ex.Message}");
                    throw;
                }
            }
        }

        public async Task<List<ConversationMeta>> ListAllConversationsAsync()
        {
            var files = Directory.GetFiles(_storagePath, "*.json");
            var result = new List<ConversationMeta>();

            foreach (var file in files)
            {
                try
                {
                    var content = await File.ReadAllTextAsync(file);
                    var data = JsonSerializer.Deserialize<ConversationData>(content) ?? new ConversationData();
                    var sessionId = Path.GetFileNameWithoutExtension(file);

                    result.Add(new ConversationMeta
                    {
                        SessionId = sessionId,
                        Title = data.Title,
                        LastModified = data.LastModified,
                        Preview = ContentHelpers.GetPreview(data.Messages.LastOrDefault()?.Content)
                    });
                }
                catch
                {
                    // Skip corrupted files
                    continue;
                }
            }

            return result.OrderByDescending(x => x.LastModified).ToList();
        }

        public async Task<bool> RenameConversationAsync(string sessionId, string newTitle)
        {
            if (string.IsNullOrWhiteSpace(newTitle))
                return false;

            var filePath = GetSessionFilePath(sessionId);

            if (_activeSessions.TryGetValue(sessionId, out var session))
            {
                session.Title = newTitle;
                return await RenameAndSaveSessionFile(sessionId, session.Title);
            }

            if (File.Exists(filePath))
            {
                try
                {
                    var data = await LoadConversationFile(filePath);
                    data.Title = newTitle;
                    data.LastModified = DateTime.UtcNow;
                    await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(data, JsonDefaults.WriteIndented));
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        private async Task<bool> RenameAndSaveSessionFile(string sessionId, string newTitle)
        {
            var filePath = GetSessionFilePath(sessionId);

            if (!File.Exists(filePath)) return false;

            try
            {
                var data = await LoadConversationFile(filePath);
                data.Title = newTitle;
                data.LastModified = DateTime.UtcNow;
                await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(data, JsonDefaults.WriteIndented));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteConversationAsync(string sessionId)
        {
            var filePath = GetSessionFilePath(sessionId);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }

            return false;
        }

        private async Task LoadSessionFromStorageAsync(ConversationSession session)
        {
            var filePath = GetSessionFilePath(session.SessionId);
            if (File.Exists(filePath))
            {
                var data = await LoadConversationFile(filePath);

                // hydrate flat fields
                session.Messages = data.Messages ?? new List<Message>();
                session.Model = _modelResolver.GetModelFor(GroqFeature.Default);
                session.Title = data.Title;
                session.LastModified = data.LastModified;
            }
        }

        private async Task<ConversationData> LoadConversationFile(string filePath)
        {
            try
            {
                var content = await File.ReadAllTextAsync(filePath);

                // First try to parse as ConversationData
                try
                {
                    return JsonSerializer.Deserialize<ConversationData>(content)
                           ?? new ConversationData();
                }
                catch
                {
                    // Fallback to old format (just Message list)
                    var messages = JsonSerializer.Deserialize<List<Message>>(content)
                                  ?? new List<Message>();
                    return new ConversationData
                    {
                        Messages = messages,
                        Title = GetTitleFromMessages(messages),
                        LastModified = File.GetLastWriteTimeUtc(filePath)
                    };
                }
            }
            catch
            {
                return new ConversationData();
            }
        }

        private string GetTitleFromMessages(List<Message> messages)
        {
            return messages.FirstOrDefault()?.Content ?? "Untitled Conversation";
        }

        private string GetSessionFilePath(string sessionId)
            => Path.Combine(_storagePath, $"{sessionId}.json");
    }
}
