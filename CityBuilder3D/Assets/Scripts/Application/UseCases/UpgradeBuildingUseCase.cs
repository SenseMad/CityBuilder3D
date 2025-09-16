using Cysharp.Threading.Tasks;
using Scripts.Application.Interfaces;
using Scripts.Application.MessageContracts.Events;
using System;
using System.Threading;

namespace Scripts.Application.UseCases
{
  public sealed class UpgradeBuildingUseCase
  {
    private readonly IBuildingRepository _buildingRepository;
    private readonly IEconomyService _economyService;
    private readonly IEventBus _eventBus;

    public UpgradeBuildingUseCase(IBuildingRepository buildingRepository, IEconomyService economyService, IEventBus eventBus)
    {
      _buildingRepository = buildingRepository;
      _economyService = economyService;
      _eventBus = eventBus;
    }

    public async UniTask<Result> ExecuteAsync(Guid buildingId, CancellationToken cancellationToken = default)
    {
      var building = _buildingRepository.FindById(buildingId);
      if (building == null)
        return Result.Fail("Здание не найдено");

      int cost;
      try
      {
        cost = building.GetUpgradeCost();
      }
      catch (InvalidOperationException)
      {
        return Result.Fail("Здание находится на максимальном уровне");
      }

      if (!_economyService.TrySpend(cost))
      {
        _eventBus.Publish(new NotEnoughGoldEvent(building.Type.Kind.ToString(), cost, _economyService.GetGold()));
        return Result.Fail($"Не хватает золота чтобы улучшить: Нужно {cost} Золота");
      }

      building.Upgrade();
      _buildingRepository.Update(building);

      _eventBus.Publish(new BuildingUpgradedEvent(buildingId, building.Level));

      return await UniTask.FromResult(Result.Ok());
    }
  }
}
