using System;

namespace Simulation.Business.Dal;

public class ActorRow
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Informations { get; set; }
    public DateTime InsertTime { get; set; }
    public DateTime ModifiedAt { get; set; }
}
