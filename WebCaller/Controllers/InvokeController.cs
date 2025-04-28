using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebCaller.Hubs;

namespace WebCaller.Controllers;

[Route("invoke")]
[ApiController]
public class InvokeController : ControllerBase
{
    private readonly IHubContext<ControlHub> _hubContext;
    private readonly ILogger<InvokeController> _logger;

    public InvokeController(IHubContext<ControlHub> hubContext, ILogger<InvokeController> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    [HttpGet("connections")]
    public IActionResult GetAllConnections()
    {
        var connections = ConnectionManager.GetAllConnections()
            .Select(kv => new { ConnectionId = kv.Key, ClientName = kv.Value })
            .ToList();
        return Ok(connections);
    }

    [HttpPost]
    public async Task<IActionResult> Post(InvokeRequest invoke)
    {
        if (string.IsNullOrEmpty(invoke.ConnectionId))
        {
            _logger.LogWarning("Empty ConnectionId received.");
            return BadRequest("ConnectionId is required.");
        }

        _logger.LogInformation("Trying to invoke action {Action} on ConnectionId {ConnectionId}", invoke.Action, invoke.ConnectionId);

        await _hubContext.Clients.Client(invoke.ConnectionId).SendAsync("InvokeAction", invoke.Action);
        return Ok();
    }
}

public class InvokeRequest
{
    public required string ConnectionId { get; set; }
    public required string Action { get; set; }
}