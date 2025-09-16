using Cysharp.Threading.Tasks;
using MessagePipe;
using Scripts.Application.Interfaces;
using Scripts.Application.MessageContracts.Events;
using Scripts.Domain.Models;
using System;
using System.Threading;

namespace Scripts.Application.UseCases
{
  public sealed class MoveBuildingUseCase
  {
    private readonly IBuildingRepository _buildingRepository;
    private readonly Grid _grid;
    private readonly IPublisher<BuildingMovedEvent> _movedPublisher;

    public MoveBuildingUseCase(IBuildingRepository buildingRepository, Grid grid, IPublisher<BuildingMovedEvent> movedPublisher)
    {
      _buildingRepository = buildingRepository;
      _grid = grid;
      _movedPublisher = movedPublisher;
    }

    public async UniTask<Result> ExecuteAsync(Guid buildingId, int toX, int toY, CancellationToken cancellationToken = default)
    {
      var building = _buildingRepository.FindById(buildingId);
      if (building == null)
        return Result.Fail("Здание не найдено");

      if (!_grid.IsInside(toX, toY))
        return Result.Fail("Целевая внешняя сетка");
      if (!_grid.CanPlace(toX, toY))
        return Result.Fail("Целевая ячейка занята");

      _grid.Vacate(building.X, building.Y);
      building.MoveTo(toX, toY);
      _grid.Occupy(toX, toY);

      _buildingRepository.Update(building);

      _movedPublisher.Publish(new BuildingMovedEvent(buildingId, toX, toY));

      return await UniTask.FromResult(Result.Ok());
    }
  }
}