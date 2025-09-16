using Cysharp.Threading.Tasks;
using Scripts.Application.DTO;
using Scripts.Application.Interfaces;
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

    public SaveGameUseCase(IBuildingRepository buildingRepository, IEconomyService economyService, ISaveLoadService saveLoadService)
    {
      _buildingRepository = buildingRepository;
      _economyService = economyService;
      _saveLoadService = saveLoadService;
    }

    public async UniTask<Result> ExecuteAsync(CancellationToken cancellationToken = default)
    {
      var saveDataDto = new SaveDataDto
      {
        Gold = _economyService.GetGold(),
        Buildings = _buildingRepository.GetAll().Select(building => new SaveDataDto.BuildingDto
        {
          Id = building.Id.ToString(),
          Kind = building.Type.Kind.ToString(),
          X = building.X,
          Y = building.Y,
          Level = building.Level
        }).ToList()
      };

      try
      {
        await _saveLoadService.SaveAsync(saveDataDto, cancellationToken);

        return Result.Ok();
      }
      catch (Exception ex)
      {
        return Result.Fail($"Сохранить не удалось: {ex.Message}");
      }
    }
  }
}
