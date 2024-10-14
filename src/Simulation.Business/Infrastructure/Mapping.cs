using System.Text.Json;
using NetTopologySuite.Geometries;
using Simulation.Business.Dal;
using Simulation.Business.Dal.Tables;
using SimulationServer.Business.Domain;

namespace SimulationServer.Business.Infrastructure;

public static class Mapping
{
    public static JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions { WriteIndented = true };
    public static Actor MapFromActorRow(ActorRow actorRow)
    {
        return new Actor
        {
            Id = actorRow.Id,
            Name = actorRow.Name,
            Informations = actorRow.Informations,
            InsertTime = actorRow.InsertTime,
            ModifiedAt = actorRow.ModifiedAt
        };

    }

    public static ActorRow MapToActorRow(Actor actor)
    {

        return new ActorRow
        {
            Id = actor.Id,
            Name = actor.Name,
            Informations = actor.Informations,
            InsertTime = actor.InsertTime,
            ModifiedAt = actor.ModifiedAt
        };
    }

    public static SensorData MapFromSensorsDataRow(SensorDataRow sensorsDataRow)
    {
        return new SensorData
        {
            SensorId = sensorsDataRow.SensorId,
            ActorId = sensorsDataRow.ActorId,
            TimeStamp = sensorsDataRow.TimeStamp,
            Latitude = sensorsDataRow.Coordinates.X,
            Longitude = sensorsDataRow.Coordinates.Y
        };
    }

    public static SensorDataRow MapToSensorsDataRow(SensorData sensorsData)
    {
        var coordinates = new Point(sensorsData.Latitude, sensorsData.Longitude) { SRID = 4326 };
        return new SensorDataRow
        {
            SensorId = sensorsData.SensorId,
            ActorId = sensorsData.ActorId,
            TimeStamp = sensorsData.TimeStamp,
            Coordinates = new Point(sensorsData.Latitude, sensorsData.Longitude) { SRID = 4326 }
        };
    }

    public static StackRow MapToStackRow(SensorData sensorData)
    {
        return new StackRow
        {
            Value = JsonSerializer.Serialize(sensorData, JsonSerializerOptions),
            InsertTime = DateTime.UtcNow
        };
    }
    public static SensorDataRow? MapToSensorData(string stackValue)
    {
        if (string.IsNullOrEmpty(stackValue))
        {
            return null;
        };
        var sensorData = JsonSerializer.Deserialize<SensorData>(stackValue, JsonSerializerOptions);
        return MapToSensorsDataRow(sensorData!);
    }
}
