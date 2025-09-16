using Scripts.Domain.Models;
using Scripts.Application.Interfaces;
using System;
using System.Linq;
using UniRx;

namespace Scripts.Application.Services
{
  public class EconomyService : IEconomyService, IDisposable
  {
    private readonly IBuildingRepository _buildingRepository;
    private readonly ReactiveProperty<long> _gold = new(0);
    private readonly IDisposable _incomeSubscription;

    public IReadOnlyReactiveProperty<long> Gold => _gold;

    public long GoldPerSecond => _buildingRepository.GetAll().Sum(b => b.GetGoldPerTick());

    public EconomyService(IBuildingRepository buildingRepository)
    {
      _buildingRepository = buildingRepository;

      _incomeSubscription = Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(_ => ApplyIncome());
    }

    public void Add(long amount) => _gold.Value += amount;

    public long GetGold() => _gold.Value;

    public bool TrySpend(long amount)
    {
      if (_gold.Value < amount)
        return false;

      _gold.Value -= amount;
      return true;
    }

    public void SetGold(long amount)
    {
      _gold.Value = amount;
    }

    private void ApplyIncome()
    {
      foreach (var building in _buildingRepository.GetAll())
      {
        long income = building.GetGoldPerTick();
        _gold.Value += income;
      }
    }

    public void Dispose()
    {
      _incomeSubscription.Dispose();
    }
  }
}