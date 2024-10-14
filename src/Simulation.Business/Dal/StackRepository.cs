using System;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Simulation.Business.Dal.Tables;
namespace SimulationServer.Business.Dal;
public interface IConsumerRepository : IDisposable
{

    public Task<IReadOnlyCollection<string>> Pop(int batchSize);

    public Task Commit();
    public Task Rollback();
    public Task<int> GetSize();

}
public interface IProducerRepository
{

    public Task Push(StackRow item, CancellationToken cancellationToken);
    public Task<int> GetSize();

}
public sealed class ProducerRepository : IProducerRepository
{
    private readonly IDbContextFactory<SimulationDbContext> _contextFactory;
    private readonly ILogger<ProducerRepository> _logger;

    public ProducerRepository(IDbContextFactory<SimulationDbContext> contextFactory, ILogger<ProducerRepository> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }
    public Task<int> GetSize()
    {
        throw new NotImplementedException();
    }



    public async Task Push(StackRow item, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Start Push task at: {0}", DateTime.UtcNow.ToString("O"));
            using var context = await _contextFactory.CreateDbContextAsync();
            context.Stack.Add(item);
            _logger.LogInformation("Push started, token state: {0}", cancellationToken.IsCancellationRequested ? "Canceled" : "Active");
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogInformation(e.Message + " " + DateTime.UtcNow.ToString("O"));
            // _logger.LogError(e, "An error occurred while pushing the data");
            throw;
        }
        finally
        {
            _logger.LogInformation("End Push task at: {0}", DateTime.UtcNow.ToString("O"));
        }
    }

}
public sealed class ConsumerRepository : IConsumerRepository
{
    string _query = @"
            WITH TopElements AS (
                SELECT TOP (@Count) Value
                FROM Stack 
                ORDER BY InsertedAt DESC
            )
            DELETE FROM TopElements
            OUTPUT DELETED.Value;
        ";
    private readonly string _connectionString;
    private SqlConnection? _connection;
    private SqlTransaction? _transaction;
    public ConsumerRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
    public Task<int> GetSize()
    {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyCollection<string>> Pop(int batchSize)
    {
        var result = new List<string>();
        if (_connection == null)
        {
            _connection = new SqlConnection(_connectionString);
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }
        using var command = new SqlCommand(_query, _connection, _transaction);
        command.Parameters.AddWithValue("@Count", batchSize);
        using var reader = await command.ExecuteReaderAsync();
        if (reader != null && reader.HasRows)
        {
            while (await reader.ReadAsync())
            {
                var itemValue = reader["Value"].ToString();
                if (itemValue != null)
                {
                    result.Add(itemValue);
                }
            }
        }
        return result;
    }

    public async Task Commit()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("Transaction has already been committed or rolled back.");
        }
        await _transaction.CommitAsync();
    }

    public async Task Rollback()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("Transaction has already been committed or rolled back.");
        }
        await _transaction.RollbackAsync();
    }
    public void Dispose()
    {
        _connection?.Dispose();
        _transaction?.Dispose();
    }
}

