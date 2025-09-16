using System;

namespace Scripts.Application.MessageContracts.Events
{
  public sealed class GameSavedEvent
  {
    public DateTime SavedAt { get; }

    public GameSavedEvent()
    {
      SavedAt = DateTime.Now;
    }
  }
}