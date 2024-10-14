using System;

namespace Simulation.Business.Dal.Tables;

public class StackRow
{
    public int  Id { get; set; }
    public required string Value { get; set; }
    public DateTime InsertTime { get; set; }
}
