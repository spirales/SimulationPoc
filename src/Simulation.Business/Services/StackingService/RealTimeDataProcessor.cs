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

    private readonly TimeSpan _expirationTimeout;
    public RealTimeDataProcessor(IProducerRepository producerRepository,
    ISensorDataPublisher sensorDataPublisher, ILogger<RealTimeDataProcessor> logger,TimeSpan expirationTimeout)
    {
        _producerRepository = producerRepository;
        _sensorDataPublisher = sensorDataPublisher;
        _logger = logger;
        _expirationTimeout = expirationTimeout;
    }

    public async Task<bool> Process(SensorData item)
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(_expirationTimeout);
        try
        {
            var token = cts.Token;
            var tasks = Task.WhenAll(
                _producerRepository.Push(Mapping.MapToStackRow(item), token),
                _sensorDataPublisher.Publish(item, token)
            );

            var completedTask = await Task.WhenAny(tasks, Task.Delay(_expirationTimeout, token));

            // If all tasks completed successfully before the timeout
            if (completedTask == tasks)
            {
                var results = await tasks;
                return results[0] && results[1];
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