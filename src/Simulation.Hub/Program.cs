using SimulationServer.Hub.Broadcast;

var builder = WebApplication.CreateBuilder(args);
// Add logging services
builder.Logging.ClearProviders();
builder.Logging.AddConsole();  // Ensure console logging is enabled
builder.Logging.AddDebug();    // Optionally add debug logging
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
// Add SignalR service
builder.Services.AddSignalR();

var app = builder.Build();

// Configure middleware and map hub endpoint
app.UseRouting();

app.UseCors(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

app.UseEndpoints(endpoints =>
{
    _ = endpoints.MapHub<SimulationChatHub>("/realtime/positionupdates");
});

app.Run();