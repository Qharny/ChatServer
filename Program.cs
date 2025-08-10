using System.Net;

namespace ChatServer
{
    internal class Program
    {
        private static Server? _server;
        private static bool _isRunning = false;

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== TCP Chat Server ===");
            Console.WriteLine();

            // Default configuration
            int port = 8888;
            IPAddress ipAddress = IPAddress.Any;

            // Parse command line arguments if provided
            if (args.Length > 0)
            {
                if (int.TryParse(args[0], out int customPort) && customPort > 0 && customPort <= 65535)
                {
                    port = customPort;
                }
                else
                {
                    Console.WriteLine($"Invalid port number: {args[0]}. Using default port 8888.");
                }
            }

            if (args.Length > 1)
            {
                if (IPAddress.TryParse(args[1], out IPAddress? customIp))
                {
                    ipAddress = customIp;
                }
                else
                {
                    Console.WriteLine($"Invalid IP address: {args[1]}. Using default IP (any).");
                }
            }

            try
            {
                // Create and configure the server
                _server = new Server(ipAddress, port);
                
                // Subscribe to server events
                _server.ServerStarted += OnServerStarted;
                _server.ServerStopped += OnServerStopped;

                // Start the server
                _isRunning = true;
                await _server.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start server: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }
        }

        /// <summary>
        /// Event handler for when the server starts
        /// </summary>
        private static void OnServerStarted(object? sender, EventArgs e)
        {
            Console.WriteLine("Server is now running!");
            Console.WriteLine("Commands:");
            Console.WriteLine("  'list' - Show connected clients");
            Console.WriteLine("  'stop' - Stop the server");
            Console.WriteLine("  'help' - Show this help");
            Console.WriteLine();

            // Start the command processing loop in a separate task
            _ = Task.Run(ProcessCommandsAsync);
        }

        /// <summary>
        /// Event handler for when the server stops
        /// </summary>
        private static void OnServerStopped(object? sender, EventArgs e)
        {
            Console.WriteLine("Server has been stopped.");
            _isRunning = false;
        }

        /// <summary>
        /// Processes console commands from the user
        /// </summary>
        private static async Task ProcessCommandsAsync()
        {
            while (_isRunning && _server != null)
            {
                try
                {
                    Console.Write("> ");
                    string? command = Console.ReadLine()?.Trim().ToLower();

                    if (string.IsNullOrEmpty(command))
                        continue;

                    switch (command)
                    {
                        case "list":
                            ShowConnectedClients();
                            break;

                        case "stop":
                            await StopServerAsync();
                            break;

                        case "help":
                            ShowHelp();
                            break;

                        case "quit":
                        case "exit":
                            await StopServerAsync();
                            break;

                        default:
                            Console.WriteLine($"Unknown command: {command}. Type 'help' for available commands.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing command: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Shows the list of connected clients
        /// </summary>
        private static void ShowConnectedClients()
        {
            if (_server == null)
            {
                Console.WriteLine("Server is not available.");
                return;
            }

            var usernames = _server.GetConnectedUsernames().ToList();
            
            if (usernames.Count == 0)
            {
                Console.WriteLine("No clients connected.");
            }
            else
            {
                Console.WriteLine($"Connected clients ({usernames.Count}):");
                foreach (string username in usernames)
                {
                    Console.WriteLine($"  - {username}");
                }
            }
        }

        /// <summary>
        /// Stops the server gracefully
        /// </summary>
        private static async Task StopServerAsync()
        {
            if (_server == null)
            {
                Console.WriteLine("Server is not available.");
                return;
            }

            Console.WriteLine("Stopping server...");
            await _server.StopAsync();
            _isRunning = false;
        }

        /// <summary>
        /// Shows the help information
        /// </summary>
        private static void ShowHelp()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("  list  - Show connected clients");
            Console.WriteLine("  stop  - Stop the server");
            Console.WriteLine("  help  - Show this help");
            Console.WriteLine("  quit  - Stop the server and exit");
            Console.WriteLine("  exit  - Stop the server and exit");
        }
    }
}
