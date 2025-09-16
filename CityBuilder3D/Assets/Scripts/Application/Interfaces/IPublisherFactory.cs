using MessagePipe;

namespace Scripts.Application.Interfaces
{
  public interface IPublisherFactory
  {
    IPublisher<T> CreatePublisher<T>();
  }
}