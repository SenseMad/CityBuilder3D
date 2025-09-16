using Cysharp.Threading.Tasks;
using MessagePipe;
using Scripts.Application.Interfaces;
using Scripts.Application.MessageContracts.Events;
using Scripts.Domain.Models;
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
    private readonly IPublisher<BuildingPlacedEvent> _buildingPlacedEvent;


    public LoadGameUseCase(
      IBuildingRepository buildingRepository, 
      IEconomyService economyService, 
      ISaveLoadService saveLoadService, 
      Grid grid, 
      Func<string, BuildingType?> buildingTypeResolver,
      IPublisher<BuildingPlacedEvent> buildingPlacedEvent
      )
    {
      _buildingRepository = buildingRepository;
      _economyService = economyService;
      _saveLoadService = saveLoadService;
      _grid = grid;
      _buildingTypeResolver = buildingTypeResolver;
      _buildingPlacedEvent = buildingPlacedEvent;
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

          if (!Guid.TryParse(building.Id, out Guid parsedId))
            parsedId = Guid.NewGuid();

          var newBuilding = new Building(parsedId, type, building.X, building.Y, building.Level);

          _buildingRepository.Add(newBuilding);
          _grid.Occupy(building.X, building.Y);

          _buildingPlacedEvent.Publish(new BuildingPlacedEvent(newBuilding.Id, type.Kind.ToString(), newBuilding.X, newBuilding.Y, newBuilding.Level));
        }

        return Result.Ok();
      }
      catch (Exception ex)
      {
        return Result.Fail($"Сбой загрузки: {ex.Message}");
      }
    }
  }
}
