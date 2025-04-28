using System.Collections.Concurrent;

namespace WebCaller.Hubs;

public static class ConnectionManager
{
    private static readonly ConcurrentDictionary<string, string> _connections = new();

    public static void AddConnection(string connectionId, string clientName)
    {
        _connections[connectionId] = clientName;
    }

    public static void RemoveConnection(string connectionId)
    {
        _connections.TryRemove(connectionId, out _);
    }

    public static IReadOnlyDictionary<string, string> GetAllConnections() => _connections;

}