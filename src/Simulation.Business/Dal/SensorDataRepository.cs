using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Simulation.Business.Dal;

namespace SimulationServer.Business.Dal;


public interface ISensorDataRepository
{
    public Task<bool> Save(IReadOnlyCollection<SensorDataRow> items);
}

public sealed class SensorDataRepository : ISensorDataRepository
{
    private readonly IDbContextFactory<SimulationDbContext> _dbContextFactory;
    private readonly ILogger<ISensorDataRepository> _logger;
    public SensorDataRepository(IDbContextFactory<SimulationDbContext> dbContextFactory, ILogger<ISensorDataRepository> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }
    public async Task<bool> Save(IReadOnlyCollection<SensorDataRow> items)
    {
        if (items.Count == 0) return true;
        try
        {
            using var context = _dbContextFactory.CreateDbContext();
            await context.SensorsData.AddRangeAsync(items);
            return await context.SaveChangesAsync() != 0;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Error saving sensor data");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving sensor data");
            return false;
        }
    }
}
