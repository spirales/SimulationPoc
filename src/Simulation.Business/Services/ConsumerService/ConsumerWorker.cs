using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SimulationServer.Business.Services.ConsumerService;

public class ConsumerWorker : BackgroundService
{
    private readonly IConsumerBusiness _consumerBusiness;
    private readonly ILogger<ConsumerWorker> _logger;
    public ConsumerWorker(IConsumerBusiness consumerBusiness, ILogger<ConsumerWorker> logger)
    {
        _consumerBusiness = consumerBusiness;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {

                    var result = await _consumerBusiness.Consume();

                    if (!result)
                    {
                        await Task.Delay(100, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Service iteration failure");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Service failure");
        }
    }
}
