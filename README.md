# TCP Chat Server - Complete Documentation

A robust, production-ready TCP-based chat server built in C# using .NET 8.0. This server provides real-time messaging capabilities with support for multiple concurrent clients, private messaging, user management, and comprehensive error handling.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Installation & Setup](#installation--setup)
- [Usage](#usage)
- [API Reference](#api-reference)
- [Message Protocol](#message-protocol)
- [Client Integration](#client-integration)
- [Testing](#testing)
- [Troubleshooting](#troubleshooting)
- [Development](#development)
- [License](#license)

## Overview

The TCP Chat Server is a high-performance, scalable chat application that enables real-time communication between multiple clients. Built with modern C# practices, it leverages async/await patterns, thread-safe collections, and JSON-based messaging for optimal performance and reliability.

### Key Benefits

- **High Performance**: Async I/O operations with minimal blocking
- **Scalable**: Thread-safe design supporting hundreds of concurrent users
- **Reliable**: Comprehensive error handling and graceful disconnection management
- **Extensible**: Modular architecture for easy feature additions
- **Cross-Platform**: Runs on Windows, Linux, and macOS

## Features

### Core Features
- **Multi-client Support**: Accepts and manages multiple simultaneous connections
- **Real-time Messaging**: Instant message delivery with broadcast and private messaging
- **User Management**: Automatic user tracking with join/leave notifications
- **Connection Notifications**: Real-time updates when users connect/disconnect
- **User List Management**: Dynamic user list updates and requests

### Advanced Features
- **Private Messaging**: Direct user-to-user communication
- **System Messages**: Server-generated notifications and status updates
- **Graceful Disconnection**: Proper cleanup and notification on client disconnect
- **Console Management**: Built-in server administration commands
- **Status Monitoring**: Real-time connection statistics and monitoring

### Technical Features
- **Thread-safe Operations**: Uses `ConcurrentDictionary` for safe client management
- **Async/Await Pattern**: Non-blocking operations throughout the application
- **JSON Message Format**: Standardized message serialization using System.Text.Json
- **Error Recovery**: Robust error handling with automatic client cleanup
- **Resource Management**: Proper disposal of network resources and memory

## Architecture

### System Architecture

```
┌─────────────────┐    TCP/IP    ┌─────────────────┐
│   Chat Client   │ ──────────── │  TCP Chat Server│
│                 │              │                 │
│ - User Interface│              │ - TcpListener   │
│ - Message Send  │              │ - ClientHandler │
│ - Message Recv  │              │ - Message Proc  │
└─────────────────┘              └─────────────────┘
         │                                │
         │                                │
┌─────────────────┐              ┌─────────────────┐
│   Chat Client   │              │   Chat Client   │
│                 │              │                 │
└─────────────────┘              └─────────────────┘
```

### Component Architecture

#### Core Components

1. **Program.cs** - Application entry point and console interface
2. **Server.cs** - Main server logic and connection management
3. **ClientHandler.cs** - Individual client communication handling
4. **ChatMessage.cs** - Message model and serialization
5. **TestClient.cs** - Testing client for development

#### Class Relationships

```
Program
  └── Server
       ├── TcpListener (accepts connections)
       ├── ConcurrentDictionary<string, ClientHandler>
       └── ClientHandler (per client)
            ├── TcpClient
            ├── NetworkStream
            └── ChatMessage processing
```

### Data Flow

1. **Connection**: Client connects → Server creates ClientHandler → User authentication
2. **Messaging**: Client sends JSON → Server deserializes → Processes → Broadcasts/Forwards
3. **Disconnection**: Client disconnects → Server cleanup → Notify other clients

## Installation & Setup

### Prerequisites

- **.NET 8.0 SDK** or later
- **Visual Studio 2022** or **Visual Studio Code**
- **Windows 10/11**, **Linux**, or **macOS**

### Installation Steps

1. **Clone or Download the Project**
   ```bash
   git clone <repository-url>
   cd ChatServer
   ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the Project**
   ```bash
   dotnet build
   ```

4. **Run the Server**
   ```bash
   dotnet run
   ```

### Configuration Options

The server supports various startup configurations:

```bash
# Default configuration (port 8888, all interfaces)
dotnet run

# Custom port
dotnet run 9999

# Custom port and IP address
dotnet run 9999 127.0.0.1

# Custom port and specific network interface
dotnet run 9999 192.168.1.100
```

## Usage

### Starting the Server

1. **Basic Start**
   ```bash
   dotnet run
   ```

2. **With Custom Configuration**
   ```bash
   dotnet run 9999 127.0.0.1
   ```

### Server Commands

Once running, the server provides these console commands:

| Command | Description | Example |
|---------|-------------|---------|
| `list` | Show all connected clients | `list` |
| `stop` | Stop the server gracefully | `stop` |
| `help` | Show available commands | `help` |
| `quit` | Stop server and exit | `quit` |
| `exit` | Stop server and exit | `exit` |

### Example Server Session

```
=== TCP Chat Server ===

Chat server started on 0.0.0.0:8888
Waiting for client connections...
Server is now running!
Commands:
  'list' - Show connected clients
  'stop' - Stop the server
  'help' - Show this help

> list
Connected clients (2):
  - Alice
  - Bob

> 
```

## API Reference

### Server Class

The main server class that manages connections and message broadcasting.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `ConnectedClientsCount` | `int` | Number of currently connected clients |
| `IsRunning` | `bool` | Whether the server is currently running |

#### Events

| Event | Description |
|-------|-------------|
| `ServerStarted` | Fired when the server starts successfully |
| `ServerStopped` | Fired when the server stops |

#### Methods

| Method | Description | Parameters |
|--------|-------------|------------|
| `StartAsync()` | Starts the server and begins accepting connections | None |
| `StopAsync()` | Stops the server and disconnects all clients | None |
| `BroadcastMessageAsync()` | Sends a message to all connected clients | `ChatMessage`, `ClientHandler?` |
| `SendMessageToUserAsync()` | Sends a message to a specific user | `ChatMessage`, `string` |
| `GetConnectedUsernames()` | Returns list of connected usernames | None |

### ClientHandler Class

Handles individual client connections and message processing.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `ClientId` | `string` | Unique identifier for the client |
| `Username` | `string` | Client's username |
| `IsConnected` | `bool` | Whether the client is connected |

#### Events

| Event | Description |
|-------|-------------|
| `ClientDisconnected` | Fired when the client disconnects |

#### Methods

| Method | Description | Parameters |
|--------|-------------|------------|
| `StartAsync()` | Starts handling the client connection | None |
| `SendMessageAsync()` | Sends a message to this client | `ChatMessage` |
| `DisconnectAsync()` | Disconnects the client | None |

### ChatMessage Class

Represents a chat message with JSON serialization support.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Type` | `string` | Message type (Connect, Chat, Disconnect, System, etc.) |
| `From` | `string` | Sender's username |
| `To` | `string` | Recipient's username (empty for broadcast) |
| `Message` | `string` | Message content |
| `Timestamp` | `DateTime` | Message creation timestamp |

#### Static Factory Methods

| Method | Description | Parameters |
|--------|-------------|------------|
| `CreateSystemMessage()` | Creates a system notification | `string message` |
| `CreateConnectMessage()` | Creates a connection notification | `string username` |
| `CreateDisconnectMessage()` | Creates a disconnection notification | `string username` |
| `CreateChatMessage()` | Creates a regular chat message | `string from`, `string message`, `string to` |
| `CreateUserListMessage()` | Creates a user list message | `List<string> usernames` |

## Message Protocol

### Message Format

All messages use JSON format with the following structure:

```json
{
  "type": "MessageType",
  "from": "username",
  "to": "recipient_username",
  "message": "message content",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

### Message Types

| Type | Description | Required Fields | Example |
|------|-------------|-----------------|---------|
| `Connect` | User connection/authentication | `from` | `{"type":"Connect","from":"Alice","message":"","timestamp":"..."}` |
| `Chat` | Regular chat message | `from`, `message` | `{"type":"Chat","from":"Alice","message":"Hello!","timestamp":"..."}` |
| `Private` | Private message | `from`, `to`, `message` | `{"type":"Private","from":"Alice","to":"Bob","message":"Secret","timestamp":"..."}` |
| `Disconnect` | User disconnection | `from` | `{"type":"Disconnect","from":"Alice","message":"","timestamp":"..."}` |
| `UserList` | Request user list | `from` | `{"type":"UserList","from":"Alice","message":"","timestamp":"..."}` |

### Message Flow Examples

#### 1. User Connection
```json
// Client sends
{"type":"Connect","from":"Alice","message":"","timestamp":"2024-01-01T12:00:00Z"}

// Server responds
{"type":"System","from":"Server","message":"Connected as Alice","timestamp":"2024-01-01T12:00:00Z"}
{"type":"UserList","from":"Server","message":"[\"Alice\",\"Bob\"]","timestamp":"2024-01-01T12:00:00Z"}

// Server broadcasts to others
{"type":"Connect","from":"Alice","message":"Alice has joined the chat","timestamp":"2024-01-01T12:00:00Z"}
```

#### 2. Chat Message
```json
// Client sends
{"type":"Chat","from":"Alice","message":"Hello everyone!","timestamp":"2024-01-01T12:00:01Z"}

// Server broadcasts to all clients
{"type":"Chat","from":"Alice","message":"Hello everyone!","timestamp":"2024-01-01T12:00:01Z"}
```

#### 3. Private Message
```json
// Client sends
{"type":"Private","from":"Alice","to":"Bob","message":"Secret message","timestamp":"2024-01-01T12:00:02Z"}

// Server sends to Bob
{"type":"Chat","from":"Alice","to":"Bob","message":"Secret message","timestamp":"2024-01-01T12:00:02Z"}

// Server confirms to Alice
{"type":"System","from":"Server","message":"Private message sent to Bob","timestamp":"2024-01-01T12:00:02Z"}
```

## Client Integration

### Client Requirements

Any TCP client can connect to the server if it supports:

1. **TCP Connection**: Connect to server IP and port
2. **JSON Serialization**: Send/receive JSON messages
3. **Newline Separation**: Messages separated by `\n`
4. **UTF-8 Encoding**: All text in UTF-8 format

### Integration Steps

1. **Establish Connection**
   ```csharp
   TcpClient client = new TcpClient();
   client.Connect("127.0.0.1", 8888);
   NetworkStream stream = client.GetStream();
   ```

2. **Send Connect Message**
   ```csharp
   var connectMessage = new ChatMessage("Connect", "YourUsername", "");
   string json = JsonSerializer.Serialize(connectMessage);
   byte[] data = Encoding.UTF8.GetBytes(json + "\n");
   await stream.WriteAsync(data, 0, data.Length);
   ```

3. **Send Chat Messages**
   ```csharp
   var chatMessage = new ChatMessage("Chat", "YourUsername", "Hello!");
   string json = JsonSerializer.Serialize(chatMessage);
   byte[] data = Encoding.UTF8.GetBytes(json + "\n");
   await stream.WriteAsync(data, 0, data.Length);
   ```

4. **Receive Messages**
   ```csharp
   byte[] buffer = new byte[4096];
   int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
   string received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
   var message = JsonSerializer.Deserialize<ChatMessage>(received);
   ```

### Example Client Implementation

See `TestClient.cs` for a complete example implementation.

## Testing

### Using the Test Client

The project includes a `TestClient` class for testing the server:

```csharp
// Create and start a test client
var client = new TestClient("127.0.0.1", 8888, "TestUser");
await client.StartAsync();
```

### Manual Testing

1. **Start the Server**
   ```bash
   dotnet run
   ```

2. **Connect Multiple Clients**
   - Use telnet: `telnet 127.0.0.1 8888`
   - Use netcat: `nc 127.0.0.1 8888`
   - Use the TestClient class

3. **Send Test Messages**
   ```json
   {"type":"Connect","from":"TestUser","message":"","timestamp":"2024-01-01T12:00:00Z"}
   {"type":"Chat","from":"TestUser","message":"Test message","timestamp":"2024-01-01T12:00:01Z"}
   ```

### Automated Testing

Create unit tests for individual components:

```csharp
[Test]
public async Task TestServerStartup()
{
    var server = new Server(IPAddress.Loopback, 8889);
    await server.StartAsync();
    Assert.IsTrue(server.IsRunning);
    await server.StopAsync();
}
```

## Troubleshooting

### Common Issues

#### 1. Port Already in Use
```
Error: Only one usage of each socket address (protocol/network address/port) is normally permitted
```
**Solution**: Use a different port or stop the existing process using the port.

#### 2. Connection Refused
```
Error: No connection could be made because the target machine actively refused it
```
**Solution**: Ensure the server is running and the port is correct.

#### 3. JSON Parsing Errors
```
Error: Invalid JSON format
```
**Solution**: Ensure messages follow the exact JSON format with proper escaping.

#### 4. Client Disconnection Issues
```
Error: Client disconnected unexpectedly
```
**Solution**: Check network connectivity and implement proper error handling in client.

### Performance Issues

#### High Memory Usage
- **Cause**: Large message buffers or memory leaks
- **Solution**: Implement message size limits and proper disposal

#### Slow Message Delivery
- **Cause**: Synchronous operations or large message queues
- **Solution**: Use async operations and implement message queuing

### Debugging

Enable detailed logging by modifying the console output in the source code:

```csharp
// Add debug logging
Console.WriteLine($"[DEBUG] Processing message: {messageJson}");
```

## Development

### Project Structure

```
ChatServer/
├── Program.cs          # Application entry point
├── Server.cs           # Main server logic
├── ClientHandler.cs    # Client connection handling
├── ChatMessage.cs      # Message model
├── TestClient.cs       # Testing client
├── ChatServer.csproj   # Project configuration
└── README.md          # Documentation
```

### Adding New Features

#### 1. New Message Types
```csharp
// In ChatMessage.cs
public static ChatMessage CreateCustomMessage(string from, string message)
{
    return new ChatMessage("Custom", from, message);
}
```

#### 2. New Server Commands
```csharp
// In Program.cs
case "custom":
    HandleCustomCommand();
    break;
```

#### 3. Enhanced Error Handling
```csharp
// Add try-catch blocks with specific error types
try
{
    // Operation
}
catch (SocketException ex)
{
    // Handle network errors
}
catch (JsonException ex)
{
    // Handle JSON parsing errors
}
```

### Code Style Guidelines

- Use **async/await** for all I/O operations
- Implement proper **error handling** with specific exception types
- Use **thread-safe collections** for shared data
- Follow **C# naming conventions**
- Add **XML documentation** for public methods
- Use **nullable reference types** where appropriate

### Performance Optimization

1. **Message Buffering**: Implement message queuing for high-traffic scenarios
2. **Connection Pooling**: Reuse connections for better performance
3. **Message Compression**: Compress large messages
4. **Database Integration**: Store messages for persistence
5. **Load Balancing**: Distribute load across multiple server instances

## License

This project is provided as-is for educational and development purposes. 

### Usage Rights

- **Educational Use**: Free to use for learning and educational purposes
- **Development Use**: Free to use for development and testing
- **Commercial Use**: Contact the author for commercial licensing

### Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

### Support

For support and questions:

1. Check the troubleshooting section
2. Review the API documentation
3. Examine the example code
4. Create an issue with detailed information

---

**Version**: 1.0.0  
**Last Updated**: January 2024  
**Author**: ChatServer Development Team 