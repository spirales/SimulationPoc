using Microsoft.Extensions.Logging;
using Simulation.Business.Dal;
using SimulationServer.Business.Dal;
using SimulationServer.Business.Infrastructure;

namespace SimulationServer.Business.Services.ConsumerService;
/// <summary>
/// historize data, used to save data into an 
/// </summary>
public interface IConsumerBusiness
{
    public Task<bool> Consume();
}
public class ConsumerBusiness : IConsumerBusiness
{
    private readonly IConsumerRepositoryFactory _consumerFactory;
    private readonly ISensorDataRepository _sensorDataRepository;
    private readonly ILogger<ConsumerBusiness> _logger;
    private readonly int _batchSize = 100;

    public ConsumerBusiness(IConsumerRepositoryFactory consumerFactory,
        ISensorDataRepository sensorDataRepository,
        ILogger<ConsumerBusiness> logger)
    {
        _consumerFactory = consumerFactory;
        _sensorDataRepository = sensorDataRepository;
        _logger = logger;

    }

    public async Task<bool> Consume()
    {

        using var consumer = _consumerFactory.Create();
        try
        {
            var items = await consumer.Pop(_batchSize);

            if (items.Count == 0) return false;

            var sensorDataItems = items.Select(Mapping.MapToSensorData)
                                       .Where(static x => x != null)
                                       .Cast<SensorDataRow>();

            var result = await _sensorDataRepository.Save(sensorDataItems.ToArray());

            if (result)
            {
                await consumer.Commit();
            }
            else
            {
                await consumer.Rollback();
            }
            return result;
        }
        catch (Exception ex)
        {
            await consumer.Rollback();
            _logger.LogError(ex, "Error consuming data");
            return false;
        }

    }


}
