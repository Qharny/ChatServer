using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ChatServer
{
    /// <summary>
    /// Handles communication with a single connected client
    /// </summary>
    public class ClientHandler
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly string _clientId;
        private readonly Server _server;
        private string _username = string.Empty;
        private bool _isConnected = true;

        /// <summary>
        /// Event fired when this client disconnects
        /// </summary>
        public event EventHandler<string>? ClientDisconnected;

        /// <summary>
        /// Gets the client's unique identifier
        /// </summary>
        public string ClientId => _clientId;

        /// <summary>
        /// Gets the client's username
        /// </summary>
        public string Username => _username;

        /// <summary>
        /// Gets whether the client is currently connected
        /// </summary>
        public bool IsConnected => _isConnected;

        /// <summary>
        /// Creates a new client handler for the specified TcpClient
        /// </summary>
        public ClientHandler(TcpClient client, Server server)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _server = server ?? throw new ArgumentNullException(nameof(server));
            _stream = client.GetStream();
            _clientId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Starts handling the client connection asynchronously
        /// </summary>
        public async Task StartAsync()
        {
            try
            {
                Console.WriteLine($"Client {_clientId} connected from {_client.Client.RemoteEndPoint}");

                // Send welcome message
                var welcomeMessage = ChatMessage.CreateSystemMessage("Welcome to the chat server!");
                await SendMessageAsync(welcomeMessage);

                // Start the message processing loop
                await ProcessMessagesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client {_clientId}: {ex.Message}");
            }
            finally
            {
                await DisconnectAsync();
            }
        }

        /// <summary>
        /// Processes incoming messages from the client
        /// </summary>
        private async Task ProcessMessagesAsync()
        {
            var buffer = new byte[4096];
            var messageBuilder = new StringBuilder();

            while (_isConnected && _client.Connected)
            {
                try
                {
                    // Read data from the stream
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    
                    if (bytesRead == 0)
                    {
                        // Client has disconnected
                        break;
                    }

                    // Convert bytes to string and append to message builder
                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    messageBuilder.Append(receivedData);

                    // Process complete messages (messages are separated by newlines)
                    string fullMessage = messageBuilder.ToString();
                    string[] messages = fullMessage.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                    // Keep the last incomplete message in the builder
                    if (!fullMessage.EndsWith('\n'))
                    {
                        messageBuilder.Clear();
                        messageBuilder.Append(messages.LastOrDefault() ?? string.Empty);
                        messages = messages.Take(messages.Length - 1).ToArray();
                    }
                    else
                    {
                        messageBuilder.Clear();
                    }

                    // Process each complete message
                    foreach (string message in messages)
                    {
                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            await ProcessMessageAsync(message.Trim());
                        }
                    }
                }
                catch (IOException)
                {
                    // Client disconnected
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading from client {_clientId}: {ex.Message}");
                    break;
                }
            }
        }

        /// <summary>
        /// Processes a single message from the client
        /// </summary>
        private async Task ProcessMessageAsync(string messageJson)
        {
            try
            {
                // Deserialize the JSON message
                var chatMessage = JsonSerializer.Deserialize<ChatMessage>(messageJson);
                
                if (chatMessage == null)
                {
                    await SendMessageAsync(ChatMessage.CreateSystemMessage("Invalid message format"));
                    return;
                }

                // Handle different message types
                switch (chatMessage.Type.ToLower())
                {
                    case "connect":
                        await HandleConnectMessageAsync(chatMessage);
                        break;
                    case "chat":
                        await HandleChatMessageAsync(chatMessage);
                        break;
                    case "disconnect":
                        await HandleDisconnectMessageAsync(chatMessage);
                        break;
                    default:
                        await SendMessageAsync(ChatMessage.CreateSystemMessage($"Unknown message type: {chatMessage.Type}"));
                        break;
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON parsing error for client {_clientId}: {ex.Message}");
                await SendMessageAsync(ChatMessage.CreateSystemMessage("Invalid JSON format"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message from client {_clientId}: {ex.Message}");
                await SendMessageAsync(ChatMessage.CreateSystemMessage("Error processing your message"));
            }
        }

        /// <summary>
        /// Handles a connection message (sets username)
        /// </summary>
        private async Task HandleConnectMessageAsync(ChatMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.From))
            {
                await SendMessageAsync(ChatMessage.CreateSystemMessage("Username is required"));
                return;
            }

            _username = message.From.Trim();
            Console.WriteLine($"Client {_clientId} set username to: {_username}");

            // Notify all clients about the new connection
            var connectNotification = ChatMessage.CreateConnectMessage(_username);
            await _server.BroadcastMessageAsync(connectNotification, this);

            // Send confirmation to the client
            await SendMessageAsync(ChatMessage.CreateSystemMessage($"Connected as {_username}"));
        }

        /// <summary>
        /// Handles a chat message (broadcasts to all clients)
        /// </summary>
        private async Task HandleChatMessageAsync(ChatMessage message)
        {
            if (string.IsNullOrWhiteSpace(_username))
            {
                await SendMessageAsync(ChatMessage.CreateSystemMessage("Please set your username first"));
                return;
            }

            if (string.IsNullOrWhiteSpace(message.Message))
            {
                await SendMessageAsync(ChatMessage.CreateSystemMessage("Message cannot be empty"));
                return;
            }

            // Create a new chat message with the correct sender
            var chatMessage = ChatMessage.CreateChatMessage(_username, message.Message, message.To);
            
            // Broadcast to all clients
            await _server.BroadcastMessageAsync(chatMessage, this);
        }

        /// <summary>
        /// Handles a disconnect message
        /// </summary>
        private async Task HandleDisconnectMessageAsync(ChatMessage message)
        {
            await DisconnectAsync();
        }

        /// <summary>
        /// Sends a message to this client
        /// </summary>
        public async Task SendMessageAsync(ChatMessage message)
        {
            if (!_isConnected || !_client.Connected)
                return;

            try
            {
                // Serialize the message to JSON
                string jsonMessage = JsonSerializer.Serialize(message);
                
                // Add newline to separate messages
                string messageToSend = jsonMessage + "\n";
                
                // Convert to bytes and send
                byte[] messageBytes = Encoding.UTF8.GetBytes(messageToSend);
                await _stream.WriteAsync(messageBytes, 0, messageBytes.Length);
                await _stream.FlushAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to client {_clientId}: {ex.Message}");
                await DisconnectAsync();
            }
        }

        /// <summary>
        /// Disconnects the client and cleans up resources
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (!_isConnected)
                return;

            _isConnected = false;

            try
            {
                // Notify other clients about the disconnection
                if (!string.IsNullOrWhiteSpace(_username))
                {
                    var disconnectNotification = ChatMessage.CreateDisconnectMessage(_username);
                    await _server.BroadcastMessageAsync(disconnectNotification, this);
                }

                // Close the connection
                _stream?.Close();
                _client?.Close();
                
                Console.WriteLine($"Client {_clientId} ({_username}) disconnected");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during disconnect for client {_clientId}: {ex.Message}");
            }
            finally
            {
                // Fire the disconnection event
                ClientDisconnected?.Invoke(this, _clientId);
            }
        }
    }
} 