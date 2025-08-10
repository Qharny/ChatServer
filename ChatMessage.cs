using System.Text.Json.Serialization;

namespace ChatServer
{
    /// <summary>
    /// Represents a chat message that can be sent between clients
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// Type of message (Connect, Disconnect, Chat, System)
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Username of the sender
        /// </summary>
        [JsonPropertyName("from")]
        public string From { get; set; } = string.Empty;

        /// <summary>
        /// Username of the recipient (empty for broadcast messages)
        /// </summary>
        [JsonPropertyName("to")]
        public string To { get; set; } = string.Empty;

        /// <summary>
        /// The actual message content
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the message was created
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Creates a new chat message
        /// </summary>
        public ChatMessage() { }

        /// <summary>
        /// Creates a new chat message with specified parameters
        /// </summary>
        public ChatMessage(string type, string from, string message, string to = "")
        {
            Type = type;
            From = from;
            To = to;
            Message = message;
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a system message
        /// </summary>
        public static ChatMessage CreateSystemMessage(string message)
        {
            return new ChatMessage("System", "Server", message);
        }

        /// <summary>
        /// Creates a connection notification message
        /// </summary>
        public static ChatMessage CreateConnectMessage(string username)
        {
            return new ChatMessage("Connect", username, $"{username} has joined the chat");
        }

        /// <summary>
        /// Creates a disconnection notification message
        /// </summary>
        public static ChatMessage CreateDisconnectMessage(string username)
        {
            return new ChatMessage("Disconnect", username, $"{username} has left the chat");
        }

        /// <summary>
        /// Creates a regular chat message
        /// </summary>
        public static ChatMessage CreateChatMessage(string from, string message, string to = "")
        {
            return new ChatMessage("Chat", from, message, to);
        }
    }
} 