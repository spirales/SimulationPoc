using Microsoft.AspNetCore.SignalR;

namespace SimulationServer.Hub.Broadcast;

public class SimulationChatHub : Microsoft.AspNetCore.SignalR.Hub
{
    private readonly ILogger<SimulationChatHub> _logger;
    public SimulationChatHub(ILogger<SimulationChatHub> logger)
    {
        _logger = logger;
    }
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Client connected: {Context.ConnectionId}");
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception!);
    }
    // Method to handle position updates with a Guid as actorId
    public async Task SendPositionUpdate(
        Guid actorId,
        double latitude,
        double longitude,
        DateTime timeStamp)
    {
        // Here you can process or save the received data
        _logger.LogInformation($"Received position update for actor {actorId} at {latitude}, {longitude} at {timeStamp}");
        Console.WriteLine($"Received position update for actor {actorId} ");
        await Clients.Others.SendAsync("ReceivePositionUpdate", actorId, latitude, longitude, timeStamp);
    }
}