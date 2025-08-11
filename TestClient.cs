using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ChatServer
{
    /// <summary>
    /// Simple test client for testing the chat server
    /// This is for testing purposes only - not part of the main server
    /// </summary>
    public class TestClient
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly string _username;
        private bool _isConnected = true;

        public TestClient(string serverIp, int port, string username)
        {
            _client = new TcpClient();
            _client.Connect(serverIp, port);
            _stream = _client.GetStream();
            _username = username;
        }

        public async Task StartAsync()
        {
            try
            {
                Console.WriteLine($"Connected to server as {_username}");

                // Send connect message
                var connectMessage = ChatMessage.CreateConnectMessage(_username);
                await SendMessageAsync(connectMessage);

                // Start receiving messages in background
                _ = Task.Run(ReceiveMessagesAsync);

                // Start sending messages
                await SendUserMessagesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                await DisconnectAsync();
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[4096];
            var messageBuilder = new StringBuilder();

            while (_isConnected && _client.Connected)
            {
                try
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    messageBuilder.Append(receivedData);

                    string fullMessage = messageBuilder.ToString();
                    string[] messages = fullMessage.Split('\n', StringSplitOptions.RemoveEmptyEntries);

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

                    foreach (string message in messages)
                    {
                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            ProcessReceivedMessage(message.Trim());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiving message: {ex.Message}");
                    break;
                }
            }
        }

        private void ProcessReceivedMessage(string messageJson)
        {
            try
            {
                var message = JsonSerializer.Deserialize<ChatMessage>(messageJson);
                if (message != null)
                {
                    string timestamp = message.Timestamp.ToString("HH:mm:ss");
                    Console.WriteLine($"[{timestamp}] {message.From}: {message.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing message: {ex.Message}");
            }
        }

        private async Task SendUserMessagesAsync()
        {
            Console.WriteLine("Type your messages (type 'quit' to exit):");
            
            while (_isConnected && _client.Connected)
            {
                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;

                if (input.ToLower() == "quit")
                {
                    break;
                }

                try
                {
                    var chatMessage = ChatMessage.CreateChatMessage(_username, input);
                    await SendMessageAsync(chatMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending message: {ex.Message}");
                }
            }
        }

        private async Task SendMessageAsync(ChatMessage message)
        {
            if (!_isConnected || !_client.Connected) return;

            try
            {
                string jsonMessage = JsonSerializer.Serialize(message);
                string messageToSend = jsonMessage + "\n";
                byte[] messageBytes = Encoding.UTF8.GetBytes(messageToSend);
                await _stream.WriteAsync(messageBytes, 0, messageBytes.Length);
                await _stream.FlushAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                await DisconnectAsync();
            }
        }

        private async Task DisconnectAsync()
        {
            if (!_isConnected) return;

            _isConnected = false;

            try
            {
                var disconnectMessage = ChatMessage.CreateDisconnectMessage(_username);
                await SendMessageAsync(disconnectMessage);
            }
            catch { }

            try
            {
                _stream?.Close();
                _client?.Close();
                Console.WriteLine("Disconnected from server");
            }
            catch { }
        }
    }
} 