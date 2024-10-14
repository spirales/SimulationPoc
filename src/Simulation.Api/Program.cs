using Microsoft.EntityFrameworkCore;
using SimulationServer.Api.Infrastructure;
using SimulationServer.Business.Dal;
using SimulationServer.Business.Domain;
using SimulationServer.Business.Infrastructure;
using SimulationServer.Business.Services.ConsumerService;
using SimulationServer.Business.Services.StackingService;

var builder = WebApplication.CreateBuilder(args);
var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
if (args != null && args.Length != 0)
    environmentName = args[0];
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var broadcasterUrl = builder.Configuration.GetConnectionString("BroadcasterConnection");
builder.Services.AddLogging(builder => builder.AddConsole());
builder.Services.AddPooledDbContextFactory<SimulationDbContext>(options =>
            {
                options.UseLoggerFactory(null);
                options.UseSqlServer(connectionString, x => x.UseNetTopologySuite())
                       .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                       options.EnableSensitiveDataLogging(); 
            });
builder.Services.AddSingleton<ISensorDataRepository, SensorDataRepository>();
builder.Services.AddSingleton<IProducerRepository, ProducerRepository>();
builder.Services.AddSingleton<ISensorDataPublisher>(sr=>new SensorDataPublisher(broadcasterUrl!,sr.GetRequiredService<ILogger<SensorDataPublisher>>()));
builder.Services.AddSingleton<IRealTimeDataProcessor>(sr=>new RealTimeDataProcessor(
    sr.GetRequiredService<IProducerRepository>(),
    sr.GetRequiredService<ISensorDataPublisher>(),
    sr.GetRequiredService<ILogger<RealTimeDataProcessor>>(),
    TimeSpan.FromMilliseconds(200)));
builder.Services.AddSingleton<IConsumerRepositoryFactory>(
    new ConsumerRepositoryFactory(connectionString!)
    );
builder.Services.AddSingleton<IConsumerBusiness, ConsumerBusiness>();
builder.Services.AddHostedService<ConsumerWorker>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseMiddleware<ThrottlingMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
var sensorDataProcessor = app.Services.GetRequiredService<IRealTimeDataProcessor>();
app.MapPost("/upload/{actorId:guid}",async (Guid actorId, SensorData data) =>
{
    await sensorDataProcessor.Process(data);
    return Results.Ok();
})
.WithName("upload")
.WithOpenApi();
app.Run();

