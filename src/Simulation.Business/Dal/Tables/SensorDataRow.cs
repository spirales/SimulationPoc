using System;
using NetTopologySuite.Geometries;
namespace Simulation.Business.Dal;


public class SensorDataRow
{
    public long Id { get; set; }
    public Guid SensorId { get; set; }
    public Guid ActorId { get; set; } 
    public DateTime TimeStamp { get; set; }
    public required Point Coordinates { get; set; }
}
