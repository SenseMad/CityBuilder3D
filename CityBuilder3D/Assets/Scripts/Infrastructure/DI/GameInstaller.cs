using Scripts.Domain.Models;
using Scripts.Application.Interfaces;
using Scripts.Application.MessageContracts.Events;
using MessagePipe;
using Scripts.Presentation.UI;
using System;
using UnityEngine;
using Scripts.Application.UseCases;
using VContainer;
using VContainer.Unity;
using Scripts.Application.Services;
using Scripts.Repositories.Building;

public sealed class GameInstaller : LifetimeScope
{
  [SerializeField] private BuildingCatalogSO _buildingCatalog;

  [Header("Panels")]
  [SerializeField] private SaveLoadPanel _saveLoadPanel;
  [SerializeField] private BuildingsPanel _buildingsPanel;
  [SerializeField] private EconomyPanel _economyPanel;

  [Header("Grid Settings")]
  [SerializeField] private int _gridWidth = 32;
  [SerializeField] private int _gridHeight = 32;

  protected override void Configure(IContainerBuilder builder)
  {
    builder.RegisterMessagePipe();

    builder.Register<ISaveLoadService, SaveLoadService>(Lifetime.Singleton);
    builder.Register<IEconomyService, EconomyService>(Lifetime.Singleton);
    builder.Register<IBuildingRepository, BuildingRepository>(Lifetime.Singleton);

    builder.Register< Scripts.Domain.Models.Grid >((c => new Scripts.Domain.Models.Grid(_gridWidth, _gridHeight)), Lifetime.Singleton);
    builder.Register<Func<string, BuildingType?>>(c => _buildingCatalog.GetBuildingType, Lifetime.Singleton);

    builder.Register<SaveGameUseCase>(Lifetime.Singleton);
    builder.Register<LoadGameUseCase>(Lifetime.Singleton);
    builder.Register<PlaceBuildingUseCase>(Lifetime.Singleton);
    builder.Register<MoveBuildingUseCase>(Lifetime.Singleton);
    builder.Register<RemoveBuildingUseCase>(Lifetime.Singleton);
    builder.Register<UpgradeBuildingUseCase>(Lifetime.Singleton);

    builder.Register<BuildingManager>(c =>
    {
      var catalog = _buildingCatalog;
      var repository = c.Resolve<IBuildingRepository>();
      var grid = c.Resolve<Scripts.Domain.Models.Grid>();
      return new BuildingManager(catalog, repository, grid);
    }, Lifetime.Singleton);

    builder.RegisterComponent(_saveLoadPanel);
    builder.RegisterComponent(_buildingsPanel);
    builder.RegisterComponent(_economyPanel);

    builder.RegisterBuildCallback(scope =>
    {
      var buildingManager = scope.Resolve<BuildingManager>();

      var savedSubscriber = scope.Resolve<ISubscriber<GameSavedEvent>>();
      var loadedSubscriber = scope.Resolve<ISubscriber<GameLoadedEvent>>();

      var saveUseCase = scope.Resolve<SaveGameUseCase>();
      var loadUseCase = scope.Resolve<LoadGameUseCase>();

      _saveLoadPanel.Initialize(saveUseCase, loadUseCase, savedSubscriber, loadedSubscriber, buildingManager);

      var placeUseCase = scope.Resolve<PlaceBuildingUseCase>();
      var moveUseCase = scope.Resolve<MoveBuildingUseCase>();
      var removeUseCase = scope.Resolve<RemoveBuildingUseCase>();
      var upgradeUseCase = scope.Resolve<UpgradeBuildingUseCase>();
      var placedSubscriber = scope.Resolve<ISubscriber<BuildingPlacedEvent>>();

      _buildingsPanel.Initialize(placeUseCase, moveUseCase, removeUseCase, upgradeUseCase, placedSubscriber, _buildingCatalog, buildingManager);

      var economyService = scope.Resolve<IEconomyService>();
      _economyPanel.Initialize(economyService);
    });
  }

}