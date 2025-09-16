using Cysharp.Threading.Tasks;
using Scripts.Domain.Models;
using Scripts.Application.Interfaces;
using Scripts.Application.MessageContracts.Events;
using System;
using System.Threading;

namespace Scripts.Application.UseCases
{
  public sealed class MoveBuildingUseCase
  {
    private readonly IBuildingRepository _buildingRepository;
    private readonly Grid _grid;
    private readonly IEventBus _eventBus;

    public MoveBuildingUseCase(IBuildingRepository buildingRepository, Grid grid, IEventBus eventBus)
    {
      _buildingRepository = buildingRepository;
      _grid = grid;
      _eventBus = eventBus;
    }

    public async UniTask<Result> ExecuteAsync(Guid buildingId, int toX, int toY, CancellationToken cancellationToken = default)
    {
      var building = _buildingRepository.FindById(buildingId);
      if (building == null)
        Result.Fail("«дание не найдено");

      if (!_grid.IsInside(toX, toY))
        return Result.Fail("÷елева€ внешн€€ сетка");
      if (!_grid.CanPlace(toX, toY))
        return Result.Fail("÷елева€ €чейка зан€та");

      _grid.Vacate(building.X, building.Y);
      building.MoveTo(toX, toY);
      _grid.Occupy(toX, toY);

      _buildingRepository.Update(building);

      _eventBus.Publish(new BuildingMovedEvent(buildingId, toX, toY));

      return await UniTask.FromResult(Result.Ok());
    }
  }
}