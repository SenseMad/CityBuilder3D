using System;

namespace Scripts.Application.MessageContracts.Events
{
  public sealed class GameLoadedEvent
  {
    public DateTime LoadedAt { get; }
    
    public GameLoadedEvent()
    {
      LoadedAt = DateTime.Now;
    }
  }
}