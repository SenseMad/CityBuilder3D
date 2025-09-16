using System;

namespace Scripts.Application.MessageContracts.Events
{
  public sealed class BuildingUpgradedEvent
  {
    public Guid BuildingId { get; }
    public int NewLevel { get; }

    public BuildingUpgradedEvent(Guid buildingId, int newLevel)
    {
      BuildingId = buildingId;
      NewLevel = newLevel;
    }
  }
}