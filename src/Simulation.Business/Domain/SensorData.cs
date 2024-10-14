using System;

namespace SimulationServer.Business.Domain;

public class SensorData
{
    public Guid SensorId { get; set; }
    public Guid ActorId { get; set; }
    public DateTime TimeStamp { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
