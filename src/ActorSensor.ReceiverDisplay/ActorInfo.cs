using System;

namespace ActorSensor.ReceiverDisplay;

public class ActorInfo
{
    public required string ActorId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime LastUpdated { get; set; }
    public required string AdditionalInfo { get; set; } // You can add more properties as needed
}