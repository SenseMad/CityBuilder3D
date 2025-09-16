using Cysharp.Threading.Tasks;
using Scripts.Application.DTO;
using Scripts.Application.Interfaces;
using Scripts.Application.MessageContracts.Events;
using System;
using System.Linq;
using System.Threading;

namespace Scripts.Application.UseCases
{
  public sealed class SaveGameUseCase
  {
    private readonly IBuildingRepository _buildingRepository;
    private readonly IEconomyService _economyService;
    private readonly ISaveLoadService _saveLoadService;
    private readonly IEventBus _eventBus;

    public SaveGameUseCase(IBuildingRepository buildingRepository, IEconomyService economyService, ISaveLoadService saveLoadService, IEventBus eventBus)
    {
      _buildingRepository = buildingRepository;
      _economyService = economyService;
      _saveLoadService = saveLoadService;
      _eventBus = eventBus;
    }

    public async UniTask<Result> ExecuteAsync(CancellationToken cancellationToken = default)
    {
      var saveDataDto = new SaveDataDto
      {
        Gold = _economyService.GetGold(),
        Buildings = _buildingRepository.GetAll().Select(building => new SaveDataDto.BuildingDto
        {
          Id = building.Id,
          Kind = building.Type.Kind.ToString(),
          X = building.X,
          Y = building.Y,
          Level = building.Level
        }).ToList()
      };

      try
      {
        await _saveLoadService.SaveAsync(saveDataDto, cancellationToken);
        _eventBus.Publish(new GameSavedEvent());

        return Result.Ok();
      }
      catch (Exception ex)
      {
        return Result.Fail($"Сохранить не удалось: {ex.Message}");
      }
    }
  }
}
