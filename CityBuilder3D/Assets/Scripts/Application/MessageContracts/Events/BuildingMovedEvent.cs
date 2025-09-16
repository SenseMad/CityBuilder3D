using System;

namespace Scripts.Application.MessageContracts.Events
{
  public sealed class BuildingMovedEvent
  {
    public Guid BuildingId { get; }
    public int X { get; }
    public int Y { get; }

    public BuildingMovedEvent(Guid buildingId, int x, int y)
    {
      BuildingId = buildingId;
      X = x;
      Y = y;
    }
  }
}