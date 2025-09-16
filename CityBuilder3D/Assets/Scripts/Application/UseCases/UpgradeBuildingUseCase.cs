using Cysharp.Threading.Tasks;
using MessagePipe;
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
    private readonly IPublisher<NotEnoughGoldEvent> _notEnoughPublisher;
    private readonly IPublisher<BuildingUpgradedEvent> _upgradedPublisher;

    public UpgradeBuildingUseCase(
      IBuildingRepository buildingRepository, 
      IEconomyService economyService, 
      IPublisher<NotEnoughGoldEvent> notEnoughPublisher, 
      IPublisher<BuildingUpgradedEvent> upgradedPublisher
      )
    {
      _buildingRepository = buildingRepository;
      _economyService = economyService;
      _notEnoughPublisher = notEnoughPublisher;
      _upgradedPublisher = upgradedPublisher;
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
        _notEnoughPublisher.Publish(new NotEnoughGoldEvent(building.Type.Kind.ToString(), cost, _economyService.GetGold()));
        return Result.Fail($"Не хватает золота чтобы улучшить: Нужно {cost} Золота");
      }

      building.Upgrade();
      _buildingRepository.Update(building);

      _upgradedPublisher.Publish(new BuildingUpgradedEvent(buildingId, building.Level));

      return await UniTask.FromResult(Result.Ok());
    }
  }
}
