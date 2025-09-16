using Scripts.Application.Interfaces;
using UniRx;

namespace Scripts.Application.Services
{
  public sealed class EventBus : IEventBus
  {
    private readonly Subject<object> _subject = new();

    public void Publish<TEvent>(TEvent evt) where TEvent : class
    {
      _subject.OnNext(evt);
    }
  }
}