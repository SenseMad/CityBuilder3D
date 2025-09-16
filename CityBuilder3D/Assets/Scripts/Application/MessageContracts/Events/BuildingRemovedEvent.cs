using System;

namespace Scripts.Application.MessageContracts.Events
{
  public sealed class BuildingRemovedEvent
  {
    public Guid BuildingId { get; }
    
    public BuildingRemovedEvent(Guid buildingId)
    {
      BuildingId = buildingId;
    }
  }
}