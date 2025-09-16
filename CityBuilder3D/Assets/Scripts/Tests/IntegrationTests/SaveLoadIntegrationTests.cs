using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UniRx;
using NUnit.Framework;
using Scripts.Application.Commands;
using Scripts.Application.DTO;
using Scripts.Application.Interfaces;
using Scripts.Application.MessageContracts.Events;
using Scripts.Application.Services;
using Scripts.Application.UseCases;
using Scripts.Domain.Models;

namespace Scripts.Tests.IntegrationTests
{
  public class SaveLoadIntegrationTests
  {
    private sealed class InMemorySaveLoadService : ISaveLoadService
    {
      private SaveDataDto _data;

      public UniTask SaveAsync(SaveDataDto saveDataDto, CancellationToken cancellationToken = default)
      {
        _data = saveDataDto;
        return UniTask.CompletedTask;
      }

      public UniTask<SaveDataDto?> LoadAsync(CancellationToken cancellationToken = default)
      {
        return UniTask.FromResult<SaveDataDto?>(_data);
      }
    }

    private sealed class TestEconomyService : IEconomyService
    {
      private readonly ReactiveProperty<long> _gold = new ReactiveProperty<long>(0);
      public IReadOnlyReactiveProperty<long> Gold => _gold;
      public long GoldPerSecond => 0;
      public long GetGold() => _gold.Value;
      public bool TrySpend(long amount)
      {
        if (_gold.Value < amount) return false;
        _gold.Value -= amount; return true;
      }
      public void Add(long amount) { _gold.Value += amount; }
      public void SetGold(long amount) { _gold.Value = amount; }
    }

    private sealed class NullPublisher<T> : IPublisher<T>
    {
      public void Publish(T message) { }
    }

    [Test]
    public async Task Place_Save_Load_Invariants_Pass()
    {
      var repository = new BuildingRepository();
      var grid = new Grid(8, 8);
      var economy = new TestEconomyService();
      economy.SetGold(1000);

      var saveLoad = new InMemorySaveLoadService();

      var placedPub = new NullPublisher<BuildingPlacedEvent>();
      var notEnoughPub = new NullPublisher<NotEnoughGoldEvent>();
      var moveUseCase = new MoveBuildingUseCase(repository, grid, new NullPublisher<BuildingMovedEvent>());
      var removeUseCase = new RemoveBuildingUseCase(repository, grid, new NullPublisher<BuildingRemovedEvent>());
      var upgradeUseCase = new UpgradeBuildingUseCase(repository, economy, new NullPublisher<NotEnoughGoldEvent>(), new NullPublisher<BuildingUpgradedEvent>());

      var placeUseCase = new PlaceBuildingUseCase(repository, economy, grid, placedPub, notEnoughPub);

      var type = new BuildingType(BuildingKind.Farm, 100, new[] { 50, 100 }, new[] { 10, 20, 40 }, null);

      var placeResult = await placeUseCase.ExecuteAsync(new PlaceBuildingCommand(type, 1, 1));
      Assert.IsTrue(placeResult.IsSuccess);

      var saveUseCase = new SaveGameUseCase(repository, economy, saveLoad);
      var loadUseCase = new LoadGameUseCase(
        repository,
        economy,
        saveLoad,
        grid,
        kind => kind == BuildingKind.Farm.ToString() ? type : null,
        new NullPublisher<BuildingPlacedEvent>()
      );

      var saveRes = await saveUseCase.ExecuteAsync();
      Assert.IsTrue(saveRes.IsSuccess);

      repository.Clear();
      grid.ClearOccupation();
      economy.SetGold(0);

      var loadRes = await loadUseCase.ExecuteAsync();
      Assert.IsTrue(loadRes.IsSuccess);

      var buildings = repository.GetAll().ToList();
      Assert.AreEqual(1, buildings.Count);
      var b = buildings[0];
      Assert.AreEqual(1, b.X);
      Assert.AreEqual(1, b.Y);
      Assert.IsTrue(grid.IsOccupied(1, 1));

      Assert.AreEqual(900, economy.GetGold());
    }
  }
}