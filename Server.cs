using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
    /// <summary>
    /// Main server class that handles accepting connections and broadcasting messages
    /// </summary>
    public class Server
    {
        private readonly TcpListener _listener;
        private readonly ConcurrentDictionary<string, ClientHandler> _clients;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning = false;

        /// <summary>
        /// Event fired when the server starts
        /// </summary>
        public event EventHandler? ServerStarted;

        /// <summary>
        /// Event fired when the server stops
        /// </summary>
        public event EventHandler? ServerStopped;

        /// <summary>
        /// Gets the number of currently connected clients
        /// </summary>
        public int ConnectedClientsCount => _clients.Count;

        /// <summary>
        /// Gets whether the server is currently running
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// Creates a new chat server instance
        /// </summary>
        /// <param name="ipAddress">IP address to listen on</param>
        /// <param name="port">Port to listen on</param>
        public Server(IPAddress ipAddress, int port)
        {
            _listener = new TcpListener(ipAddress, port);
            _clients = new ConcurrentDictionary<string, ClientHandler>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Creates a new chat server instance that listens on all interfaces
        /// </summary>
        /// <param name="port">Port to listen on</param>
        public Server(int port) : this(IPAddress.Any, port)
        {
        }

        /// <summary>
        /// Starts the server and begins accepting client connections
        /// </summary>
        public async Task StartAsync()
        {
            if (_isRunning)
            {
                Console.WriteLine("Server is already running");
                return;
            }

            try
            {
                _listener.Start();
                _isRunning = true;

                var endpoint = _listener.LocalEndpoint;
                Console.WriteLine($"Chat server started on {endpoint}");
                Console.WriteLine("Waiting for client connections...");

                // Fire the server started event
                ServerStarted?.Invoke(this, EventArgs.Empty);

                // Start accepting clients in a loop
                await AcceptClientsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting server: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Continuously accepts new client connections
        /// </summary>
        private async Task AcceptClientsAsync()
        {
            while (_isRunning && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    // Accept a new client connection
                    TcpClient client = await _listener.AcceptTcpClientAsync(_cancellationTokenSource.Token);
                    
                    // Create a new client handler
                    var clientHandler = new ClientHandler(client, this);
                    
                    // Subscribe to the client's disconnection event
                    clientHandler.ClientDisconnected += OnClientDisconnected;
                    
                    // Add the client to our collection
                    _clients.TryAdd(clientHandler.ClientId, clientHandler);
                    
                    Console.WriteLine($"New client connected. Total clients: {_clients.Count}");
                    
                    // Start handling the client in a separate task
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await clientHandler.StartAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error in client handler task: {ex.Message}");
                        }
                    }, _cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    // Server is being stopped
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting client: {ex.Message}");
                    
                    // If the listener is no longer active, break the loop
                    if (!_listener.Server.IsBound)
                        break;
                }
            }
        }

        /// <summary>
        /// Handles client disconnection events
        /// </summary>
        private void OnClientDisconnected(object? sender, string clientId)
        {
            if (sender is ClientHandler clientHandler)
            {
                // Remove the client from our collection
                _clients.TryRemove(clientId, out _);
                
                // Unsubscribe from the event
                clientHandler.ClientDisconnected -= OnClientDisconnected;
                
                Console.WriteLine($"Client {clientId} removed. Total clients: {_clients.Count}");
            }
        }

        /// <summary>
        /// Broadcasts a message to all connected clients
        /// </summary>
        /// <param name="message">The message to broadcast</param>
        /// <param name="excludeClient">Optional client to exclude from the broadcast</param>
        public async Task BroadcastMessageAsync(ChatMessage message, ClientHandler? excludeClient = null)
        {
            if (message == null)
                return;

            var tasks = new List<Task>();

            foreach (var client in _clients.Values)
            {
                // Skip the excluded client
                if (excludeClient != null && client.ClientId == excludeClient.ClientId)
                    continue;

                // Send the message to this client
                tasks.Add(client.SendMessageAsync(message));
            }

            // Wait for all messages to be sent
            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
            }
        }

        /// <summary>
        /// Sends a message to a specific client by username
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <param name="username">The username of the target client</param>
        public async Task SendMessageToUserAsync(ChatMessage message, string username)
        {
            if (message == null || string.IsNullOrWhiteSpace(username))
                return;

            var targetClient = _clients.Values.FirstOrDefault(c => 
                string.Equals(c.Username, username, StringComparison.OrdinalIgnoreCase));

            if (targetClient != null)
            {
                await targetClient.SendMessageAsync(message);
            }
        }

        /// <summary>
        /// Gets a list of all connected usernames
        /// </summary>
        public IEnumerable<string> GetConnectedUsernames()
        {
            return _clients.Values
                .Where(c => !string.IsNullOrWhiteSpace(c.Username))
                .Select(c => c.Username)
                .ToList();
        }

        /// <summary>
        /// Stops the server and disconnects all clients
        /// </summary>
        public async Task StopAsync()
        {
            if (!_isRunning)
            {
                Console.WriteLine("Server is not running");
                return;
            }

            Console.WriteLine("Stopping server...");

            // Stop accepting new connections
            _isRunning = false;
            _cancellationTokenSource.Cancel();

            // Stop the listener
            _listener.Stop();

            // Disconnect all clients
            var disconnectTasks = _clients.Values.Select(client => client.DisconnectAsync());
            await Task.WhenAll(disconnectTasks);

            // Clear the clients collection
            _clients.Clear();

            Console.WriteLine("Server stopped");
            
            // Fire the server stopped event
            ServerStopped?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Disposes of server resources
        /// </summary>
        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _listener?.Stop();
        }
    }
} 