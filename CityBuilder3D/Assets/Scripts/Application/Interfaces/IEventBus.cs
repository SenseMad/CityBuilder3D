namespace Scripts.Application.Interfaces
{
  public interface IEventBus
  {
    void Publish<TEvent>(TEvent evt) where TEvent : class;
  }
}