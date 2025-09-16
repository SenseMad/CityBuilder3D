using Cysharp.Threading.Tasks;
using Scripts.Domain.Models;
using Scripts.Application.Interfaces;
using Scripts.Application.MessageContracts.Events;
using System;
using System.Threading;

namespace Scripts.Application.UseCases
{
  public sealed class LoadGameUseCase
  {
    private readonly IBuildingRepository _buildingRepository;
    private readonly IEconomyService _economyService;
    private readonly ISaveLoadService _saveLoadService;
    private readonly Grid _grid;
    private readonly Func<string, BuildingType?> _buildingTypeResolver;
    private readonly IEventBus _eventBus;

    public LoadGameUseCase(IBuildingRepository buildingRepository, IEconomyService economyService, ISaveLoadService saveLoadService, Grid grid, Func<string, BuildingType?> buildingTypeResolver, IEventBus eventBus)
    {
      _buildingRepository = buildingRepository;
      _economyService = economyService;
      _saveLoadService = saveLoadService;
      _grid = grid;
      _buildingTypeResolver = buildingTypeResolver;
      _eventBus = eventBus;
    }

    public async UniTask<Result> ExecuteAsync(CancellationToken cancellationToken = default)
    {
      try
      {
        var saveDataDto = await _saveLoadService.LoadAsync(cancellationToken);
        if (saveDataDto == null)
          return Result.Fail("Сохраненные данные не найдены");

        _buildingRepository.Clear();
        _grid.ClearOccupation();

        var targetGold = saveDataDto.Gold;
        var currentGold = _economyService.GetGold();
        if (targetGold > currentGold)
          _economyService.Add(targetGold - currentGold);

        foreach (var building in saveDataDto.Buildings)
        {
          var type = _buildingTypeResolver(building.Kind);
          if (type == null)
            continue;

          var newBuilding = new Building(building.Id != Guid.Empty ? building.Id : Guid.NewGuid(), type, building.X, building.Y, building.Level);

          _buildingRepository.Add(newBuilding);
          _grid.Occupy(building.X, building.Y);

          _eventBus.Publish(new BuildingPlacedEvent(newBuilding.Id, type.Kind.ToString(), newBuilding.X, newBuilding.Y, newBuilding.Level));
        }

        _eventBus.Publish(new GameLoadedEvent());
        return Result.Ok();
      }
      catch (Exception ex)
      {
        return Result.Fail($"Сбой загрузки: {ex.Message}");
      }
    }
  }
}
