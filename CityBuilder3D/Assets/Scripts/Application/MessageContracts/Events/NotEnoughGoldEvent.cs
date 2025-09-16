namespace Scripts.Application.MessageContracts.Events
{
  public sealed class NotEnoughGoldEvent
  {
    public string BuildingType { get; }
    public int Cost { get; }
    public long CurrentGold { get; }

    public NotEnoughGoldEvent(string buildingType, int cost, long currentGold)
    {
      BuildingType = buildingType;
      Cost = cost;
      CurrentGold = currentGold;
    }
  }
}