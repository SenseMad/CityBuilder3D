using Cysharp.Threading.Tasks;
using MessagePipe;
using Scripts.Application.Interfaces;
using Scripts.Application.MessageContracts.Events;
using Scripts.Domain.Models;
using System;
using System.Threading;

namespace Scripts.Application.UseCases
{
  public sealed class RemoveBuildingUseCase
  {
    private readonly IBuildingRepository _buildingRepository;
    private readonly Grid _grid;
    private readonly IPublisher<BuildingRemovedEvent> _removedPublisher;

    public RemoveBuildingUseCase(IBuildingRepository buildingRepository, Grid grid, IPublisher<BuildingRemovedEvent> removedPublisher)
    {
      _buildingRepository = buildingRepository;
      _grid = grid;
      _removedPublisher = removedPublisher;
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

      _removedPublisher.Publish(new BuildingRemovedEvent(buildingId));

      return await UniTask.FromResult(Result.Ok());
    }
  }
}
