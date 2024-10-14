using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using SimulationServer.Business.Domain;

namespace SimulationServer.Business.Services.StackingService;

public interface ISensorDataPublisher
{
    public Task<bool> Publish(SensorData item, CancellationToken cancellationToken);
}

public class SensorDataPublisher : ISensorDataPublisher
{
    private readonly HubConnection _signalRConnection;
    private readonly ILogger<SensorDataPublisher> _logger;

    public SensorDataPublisher(string hubConnectionAddress, ILogger<SensorDataPublisher> logger)
    {
        _logger = logger;
        _signalRConnection = new HubConnectionBuilder()
            .WithUrl(hubConnectionAddress)

             .ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Debug);
                logging.AddConsole();
            })
            .Build();
    }

    public async Task<bool> Publish(SensorData item, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Start Publish at: {0}", DateTime.UtcNow.ToString("O"));
            if (_signalRConnection.State == HubConnectionState.Disconnected)
            {
                await _signalRConnection.StartAsync(cancellationToken);
            }
            await _signalRConnection.SendAsync("SendPositionUpdate", item.ActorId, item.Latitude, item.Longitude, item.TimeStamp, cancellationToken);
            _logger.LogInformation("End Publish succesfully at: {0}", DateTime.UtcNow.ToString("O"));
            return true;
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Error while publishing sensor data");
            return false;
        }
        finally
        {
            _logger.LogInformation("End  Publish at: {0}", DateTime.UtcNow.ToString("O"));
        }
    }
}
