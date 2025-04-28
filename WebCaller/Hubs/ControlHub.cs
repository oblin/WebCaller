using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace WebCaller.Hubs;

public class ControlHub : Hub
{
    private readonly ILogger<ControlHub> _logger;

    public ControlHub(ILogger<ControlHub> logger)
    {
        _logger = logger;
    }
    public override async Task OnConnectedAsync()
    {
        Log.Information("Client connected. ConnectionId: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        ConnectionManager.RemoveConnection(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task Register(string clientName)
    {
        Log.Information($"Client {clientName} connected. ConnectionId: {Context.ConnectionId}");
        ConnectionManager.AddConnection(Context.ConnectionId, clientName);
        await Task.CompletedTask;
    }
}
