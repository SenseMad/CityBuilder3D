using Cysharp.Threading.Tasks;
using Scripts.Domain.Models;
using Scripts.Application.Interfaces;
using Scripts.Application.MessageContracts.Events;
using System;
using System.Threading;

namespace Scripts.Application.UseCases
{
  public sealed class RemoveBuildingUseCase
  {
    private readonly IBuildingRepository _buildingRepository;
    private readonly Grid _grid;
    private readonly IEventBus _eventBus;

    public RemoveBuildingUseCase(IBuildingRepository buildingRepository, Grid grid, IEventBus eventBus)
    {
      _buildingRepository = buildingRepository;
      _grid = grid;
      _eventBus = eventBus;
    }

    public async UniTask<Result> ExecuteAsync(Guid buildingId, CancellationToken cancellationToken = default)
    {
      var building = _buildingRepository.FindById(buildingId);
      if (building == null)
        return Result.Fail("Здание не найдено");

      var removed = _buildingRepository.Remove(buildingId);
      if (!removed)
        return Result.Fail("Не удалось удалить здание");

      _grid.Vacate(building.X, building.Y);

      _eventBus.Publish(new BuildingRemovedEvent(buildingId));

      return await UniTask.FromResult(Result.Ok());
    }
  }
}
