using UniRx;

namespace Scripts.Application.Interfaces
{
  public interface IEconomyService
  {
    IReadOnlyReactiveProperty<long> Gold { get; }
    long GoldPerSecond { get; }

    long GetGold();
    bool TrySpend(long amount);
    void Add(long amount);
    void SetGold(long amount);
  }
}