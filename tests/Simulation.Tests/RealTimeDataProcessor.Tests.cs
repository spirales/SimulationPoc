using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Simulation.Business.Dal.Tables;
using SimulationServer.Business.Dal;
using SimulationServer.Business.Domain;
using SimulationServer.Business.Services.StackingService;

namespace Simulation.Tests;
public class RealTimeDataProcessorTests
{
    private readonly IProducerRepository _mockProducerRepository;
    private readonly ISensorDataPublisher _mockSensorDataPublisher;
    private readonly ILogger<RealTimeDataProcessor> _mockLogger;
    private readonly RealTimeDataProcessor _sut;  // System Under Test (SUT)
    private readonly TimeSpan expirationTimeout = TimeSpan.FromMilliseconds(500);  // Default timeout for the tests

    public RealTimeDataProcessorTests()
    {
        _mockProducerRepository = Substitute.For<IProducerRepository>();
        _mockSensorDataPublisher = Substitute.For<ISensorDataPublisher>();
        _mockLogger = Substitute.For<ILogger<RealTimeDataProcessor>>();

        _sut = new RealTimeDataProcessor(_mockProducerRepository, _mockSensorDataPublisher, _mockLogger, expirationTimeout);
    }

    // Test for both Push and Publish success
    [Theory]
    [InlineData(true, true, true)]   // Both tasks succeed
    [InlineData(false, true, false)] // Push fails, Publish succeeds
    [InlineData(true, false, false)] // Push succeeds, Publish fails
    [InlineData(false, false, false)]// Both tasks fail
    public async Task Process_ShouldReturnExpectedResult_BasedOnPushAndPublishSuccess(bool pushSuccess, bool publishSuccess, bool expectedResult)
    {
        // Arrange: Mock Push and Publish behavior
        _mockProducerRepository.Push(Arg.Any<StackRow>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(pushSuccess));

        _mockSensorDataPublisher.Publish(Arg.Any<SensorData>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(publishSuccess));

        var sensorData = new SensorData();  // Create a test SensorData object

        // Act: Call the Process method
        var result = await _sut.Process(sensorData);

        // Assert: Verify the result matches the expected outcome
        Assert.Equal(expectedResult, result);
    }

    // Test for task cancellation due to timeout
    [Fact]
    public async Task Process_ShouldReturnFalse_WhenTimeoutOccurs()
    {
        // Arrange: Set up Push and Publish to never complete (simulate long-running tasks)
        _mockProducerRepository.Push(Arg.Any<StackRow>(), Arg.Any<CancellationToken>())
            .Returns(async call =>
            {
                await Task.Delay(10000); // Simulate long-running task
                return true;
            });

        _mockSensorDataPublisher.Publish(Arg.Any<SensorData>(), Arg.Any<CancellationToken>())
            .Returns(async call =>
            {
                await Task.Delay(10000); // Simulate long-running task
                return true;
            });

        var sensorData = new SensorData();

        // Act: Call the Process method, expecting it to timeout
        var startWatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await _sut.Process(sensorData);
        startWatch.Stop();
        // Assert: Verify that the result is false (timeout)
        Assert.False(result);
        Assert.True(startWatch.ElapsedMilliseconds < 600); // Verify that the method returned within 1 second
     }

    // Test for exception handling in Push
    [Fact]
    public async Task Process_ShouldReturnFalse_WhenPushThrowsException()
    {
        // Arrange: Set up Push to throw an exception
        _mockProducerRepository.Push(Arg.Any<StackRow>(), Arg.Any<CancellationToken>()).ThrowsAsync(new Exception("Push failed"));

        _mockSensorDataPublisher.Publish(Arg.Any<SensorData>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true)); // Publish succeeds

        var sensorData = new SensorData();

        // Act: Call the Process method
        var result = await _sut.Process(sensorData);

        // Assert: Verify the result is false due to the exception
        Assert.False(result);
        _mockLogger.Received().LogError(Arg.Any<Exception>(), "An error occurred while processing the data");
    }

    // Test for exception handling in Publish
    [Fact]
    public async Task Process_ShouldReturnFalse_WhenPublishThrowsException()
    {
        // Arrange: Set up Publish to throw an exception
        _mockSensorDataPublisher.Publish(Arg.Any<SensorData>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Publish failed"));

        _mockProducerRepository.Push(Arg.Any<StackRow>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true)); // Push succeeds

        var sensorData = new SensorData();

        // Act: Call the Process method
        var result = await _sut.Process(sensorData);

        // Assert: Verify the result is false due to the exception
        Assert.False(result);
        _mockLogger.Received().LogError(Arg.Any<Exception>(), "An error occurred while processing the data");
    }
}
