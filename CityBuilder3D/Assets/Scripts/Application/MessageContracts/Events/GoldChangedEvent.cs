namespace Scripts.Application.MessageContracts.Events
{
  public readonly struct GoldChangedEvent
  {
    public long Amount { get; }
    public GoldChangedEvent(long amount) => Amount = amount;
  }
}
