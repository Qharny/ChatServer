# TCP Chat Server

A robust TCP-based chat server built in C# using .NET 8.0. This server can handle multiple concurrent clients and provides real-time messaging capabilities.

## Features

- **Multi-client Support**: Accepts multiple clients simultaneously
- **Thread-safe Operations**: Uses `ConcurrentDictionary` for safe client management
- **Async/Await Pattern**: Non-blocking operations with `TcpListener` and `TcpClient`
- **JSON Message Format**: Uses System.Text.Json for message serialization
- **Graceful Error Handling**: Handles client disconnections and network errors
- **Connection Notifications**: Broadcasts join/leave notifications to all clients
- **Console Interface**: Built-in commands for server management

## Message Format

The server uses a simple JSON message format:

```json
{
  "type": "Chat|Connect|Disconnect|System",
  "from": "username",
  "to": "recipient_username",
  "message": "message content",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

## Building and Running

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022 or Visual Studio Code

### Build the Project
```bash
dotnet build
```

### Run the Server
```bash
# Run with default settings (port 8888, all interfaces)
dotnet run

# Run with custom port
dotnet run 9999

# Run with custom port and IP
dotnet run 9999 127.0.0.1
```

## Server Commands

Once the server is running, you can use these console commands:

- `list` - Show all connected clients
- `stop` - Stop the server gracefully
- `help` - Show available commands
- `quit` or `exit` - Stop the server and exit

## Project Structure

- **Program.cs** - Main entry point and console interface
- **Server.cs** - Core server logic for accepting connections and broadcasting
- **ClientHandler.cs** - Handles individual client connections and message processing
- **ChatMessage.cs** - Message model with JSON serialization support

## Client Integration

The server is designed to work with any TCP client that can:
1. Connect to the specified IP and port
2. Send JSON messages in the defined format
3. Handle newline-separated messages
4. Process incoming JSON messages

### Example Client Message Flow

1. **Connect**: Send a message with `type: "Connect"` and `from: "username"`
2. **Chat**: Send messages with `type: "Chat"`, `from: "username"`, and `message: "content"`
3. **Disconnect**: Send a message with `type: "Disconnect"`

## Error Handling

The server includes comprehensive error handling for:
- Network connection issues
- Invalid JSON messages
- Client disconnections
- Resource cleanup

## Windows Forms Integration

This server is designed to work seamlessly with Windows Forms clients. The JSON message format and TCP communication protocol make it easy to integrate with any .NET client application.

## License

This project is provided as-is for educational and development purposes. 