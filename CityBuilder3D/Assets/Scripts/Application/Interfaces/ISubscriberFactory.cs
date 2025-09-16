using MessagePipe;

namespace Scripts.Application.Interfaces
{
  public interface ISubscriberFactory
  {
    ISubscriber<T> CreateSubscriber<T>();
  }
}