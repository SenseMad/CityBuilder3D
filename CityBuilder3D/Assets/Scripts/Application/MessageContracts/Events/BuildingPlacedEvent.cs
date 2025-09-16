using System;

namespace Scripts.Application.MessageContracts.Events
{
  public sealed class BuildingPlacedEvent
  {
    public Guid BuildingId { get; }
    public string Type { get; }
    public int X { get; }
    public int Y { get; }
    public int Level { get; }

    public BuildingPlacedEvent(Guid buildingId, string type, int x, int y, int level)
    {
      BuildingId = buildingId;
      Type = type;
      X = x;
      Y = y;
      Level = level;
    }
  }
}