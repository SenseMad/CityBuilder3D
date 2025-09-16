using Cysharp.Threading.Tasks;
using Scripts.Domain.Models;
using Scripts.Application.Interfaces;
using Scripts.Application.MessageContracts.Events;
using MessagePipe;
using System;
using System.Threading;
using Scripts.Application.Commands;

namespace Scripts.Application.UseCases
{
  public sealed class PlaceBuildingUseCase
  {
    private readonly IBuildingRepository _buildingRepository;
    private readonly IEconomyService _economyService;
    private readonly IPublisher<BuildingPlacedEvent> _publisher;
    private readonly IPublisher<NotEnoughGoldEvent> _notEnoughGoldPublisher;

    public Grid Grid { get; private set; }

    public PlaceBuildingUseCase(
      IBuildingRepository buildingRepository, 
      IEconomyService economyService, 
      Grid grid, 
      IPublisher<BuildingPlacedEvent> publisher,
      IPublisher<NotEnoughGoldEvent> notEnoughGoldPublisher)
    {
      _buildingRepository = buildingRepository;
      _economyService = economyService;
      Grid = grid;
      _publisher = publisher;
      _notEnoughGoldPublisher = notEnoughGoldPublisher;
    }

    public async UniTask<Result<Guid>> ExecuteAsync(PlaceBuildingCommand command, CancellationToken cancellationToken = default)
    {
      if (!Grid.IsInside(command.X, command.Y))
        return Result<Guid>.Fail("Позиция за сеткой");

      if (!Grid.CanPlace(command.X, command.Y))
        return Result<Guid>.Fail("Ячейка занята");

      var cost = command.Type.BaseCost;
      if (!_economyService.TrySpend(cost))
      {
        _notEnoughGoldPublisher.Publish(new NotEnoughGoldEvent(command.Type.Kind.ToString(), cost, _economyService.GetGold()));
        return Result<Guid>.Fail($"Не хватает золота чтобы установить. Нужно {cost} Золота");
      }

      var id = Guid.NewGuid();
      var building = new Building(id, command.Type, command.X, command.Y, level: 0);
      _buildingRepository.Add(building);
      Grid.Occupy(command.X, command.Y);

      _publisher.Publish(new BuildingPlacedEvent(id, command.Type.Kind.ToString(), command.X, command.Y, building.Level));

      return await UniTask.FromResult(Result<Guid>.Ok(id));
    }
  }
}