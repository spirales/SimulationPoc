using SimulationServer.Business.Dal;

namespace SimulationServer.Business.Infrastructure;

public interface IConsumerRepositoryFactory
{
    public IConsumerRepository Create();
}
public class ConsumerRepositoryFactory : IConsumerRepositoryFactory
{
    private readonly string _connectionString;
    public ConsumerRepositoryFactory(string connectionString)
    {
        _connectionString = connectionString;
    }
    public IConsumerRepository Create()
    {
        return new ConsumerRepository(_connectionString);
    }
}
