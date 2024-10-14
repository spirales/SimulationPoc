using Microsoft.Extensions.Logging;
using SimulationServer.Business.Dal;
using SimulationServer.Business.Domain;
using SimulationServer.Business.Infrastructure;

namespace SimulationServer.Business.Services.StackingService;

public interface IRealTimeDataProcessor
{
    public Task<bool> Process(SensorData item);
}
public class RealTimeDataProcessor : IRealTimeDataProcessor
{
    private readonly IProducerRepository _producerRepository;
    private readonly ISensorDataPublisher _sensorDataPublisher;

    private readonly ILogger<RealTimeDataProcessor> _logger;

    private readonly TimeSpan expirationTimeout = TimeSpan.FromMilliseconds(200);
    public RealTimeDataProcessor(IProducerRepository producerRepository,
    ISensorDataPublisher sensorDataPublisher, ILogger<RealTimeDataProcessor> logger)
    {
        _producerRepository = producerRepository;
        _sensorDataPublisher = sensorDataPublisher;
        _logger = logger;
    }

    public async Task<bool> Process(SensorData item)
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(expirationTimeout);
        try
        {
            var token = cts.Token;
            var tasks = Task.WhenAll(
                _producerRepository.Push(Mapping.MapToStackRow(item), token),
                _sensorDataPublisher.Publish(item, token)
            );

            var completedTask = await Task.WhenAny(tasks, Task.Delay(expirationTimeout, token));

            // If all tasks completed successfully before the timeout
            if (completedTask == tasks)
            {
                await tasks; // Ensure to observe any exceptions from the tasks
                return true;
            }

            // If timeout occurred, cancel tasks and return false
            cts.Cancel();
            return false;
        }
        catch (TaskCanceledException e)
        {
            _logger.LogError(e, "Task was cancelled due to timeout");
            return false;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while processing the data");
            return false;
        }
    }
}