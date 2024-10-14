using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ActorSensor.DataGenerator;

class Program
{
    private static readonly HttpClient client = new HttpClient();
    private static readonly string url = "http://localhost:5000/upload"; // The URL where the data will be posted
    private static bool isRunning = true;

    static async Task Main(string[] args)
    {
        Console.WriteLine("Press any key to stop the application.");

        // Run continuously until the user stops the console
        _ = Task.Run(() => MonitorKeyPress());

        // Optionally, run multiple threads to send data concurrently
        Task[] tasks = new Task[3]; // For example, we use 3 threads

        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = Task.Run(() => SendDataContinuously(i + 1));
        }

        await Task.WhenAll(tasks); // Wait for all tasks to complete (this will run indefinitely)
    }

    // Monitor for a key press to stop the loop
    static void MonitorKeyPress()
    {
        Console.ReadKey();
        isRunning = false;
    }

    // Method to send SensorData continuously
    static async Task SendDataContinuously(int threadId)
    {
        Console.WriteLine($"Thread {threadId} started.");

        while (isRunning)
        {
            var randomSensorData = GenerateSensorData();
            await SendSensorData(randomSensorData, threadId);
            // Delay between sends to simulate sensor intervals (e.g., 200 ms)
            await Task.Delay(200);
        }

        Console.WriteLine($"Thread {threadId} stopped.");
    }

    // Method to send SensorData
    public static async Task SendSensorData(SensorData sensorData, int threadId)
    {
        try
        {
            string jsonContent = JsonSerializer.Serialize(sensorData);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            Console.WriteLine($"Thread {threadId}: Sending data for SensorId: {sensorData.SensorId}");

            var response = await client.PostAsync(url + $"/{sensorData.ActorId}", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Thread {threadId}: Data sent successfully.");
            }
            else
            {
                Console.WriteLine($"Thread {threadId}: Failed to send data. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Thread {threadId}: Error sending data: {ex.Message}");
        }
    }
    public static SensorData GenerateSensorData()
    {
        var random = new Random();
        var latitude = RandomLatitudeInRomania(random);
        var longitude = RandomLongitudeInRomania(random);
        var actorId1 = Guid.Parse("3FA85F64-5717-4562-B3FC-2C963F66AFA6");
        var actorId2 = Guid.Parse("2A1A84C9-DD33-48AD-8858-92A78102CC4A");
        return new SensorData
        {
            SensorId = Guid.NewGuid(),
            ActorId = random.Next(0, 2) == 0 ? actorId1 : actorId2,
            TimeStamp = DateTime.UtcNow.ToString("o"),
            Latitude = latitude,
            Longitude = longitude
        };
    }

    public static double RandomLatitudeInRomania(Random random)
    {
        // Romania is roughly between latitudes 43.6 and 48.3
        return random.NextDouble() * (48.3 - 43.6) + 43.6;
    }

    public static double RandomLongitudeInRomania(Random random)
    {
        // Romania is roughly between longitudes 20.2 and 29.7
        return random.NextDouble() * (29.7 - 20.2) + 20.2;
    }
}

public record SensorData
{
    public required Guid SensorId { get; set; }
    public required Guid ActorId { get; set; }
    public required string TimeStamp { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}