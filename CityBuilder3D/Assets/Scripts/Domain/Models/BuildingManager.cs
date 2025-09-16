using Scripts.Application.Interfaces;
using Scripts.Presentation;
using System;
using System.Collections.Generic;
using UnityEngine;
using Scripts.Repositories.Building;
using Object = UnityEngine.Object;

namespace Scripts.Domain.Models
{
  public sealed class BuildingManager
  {
    private readonly BuildingCatalogSO _buildingCatalog;
    private readonly IBuildingRepository _repository;
    private readonly Grid _grid;

    private readonly List<GameObject> _spawnedBuildings = new();
    private readonly Dictionary<Guid, GameObject> _spawnedBuildingsDict = new();

    public BuildingManager(BuildingCatalogSO buildingCatalog, IBuildingRepository repository, Grid grid)
    {
      _buildingCatalog = buildingCatalog;
      _repository = repository;
      _grid = grid;
    }

    public void ClearAllBuildings()
    {
      foreach (var gameObject in _spawnedBuildings)
        Object.Destroy(gameObject);

      _spawnedBuildings.Clear();
      _spawnedBuildingsDict.Clear();
      _grid.ClearOccupation();
    }

    public void SpawnAllBuildingsFromRepository()
    {
      foreach (var building in _repository.GetAll())
        SpawnSingleBuilding(building);
    }

    public void SpawnSingleBuilding(Building building)
    {
      var type = _buildingCatalog.GetBuildingType(building.Type.Kind.ToString());
      if (type?.Prefab == null) return;

      var gameObject = Object.Instantiate(type.Prefab, new Vector3(building.X, 0, building.Y), Quaternion.identity);
      gameObject.name = $"Building_{building.Id}";

      if (!gameObject.TryGetComponent<BuildingView>(out var view))
        view = gameObject.AddComponent<BuildingView>();

      view.SetBuildingId(building.Id);

      _spawnedBuildings.Add(gameObject);
      _spawnedBuildingsDict[building.Id] = gameObject;

      _grid.Occupy(building.X, building.Y);
    }

    public GameObject GetBuildingObject(Guid buildingId)
    {
      _spawnedBuildingsDict.TryGetValue(buildingId, out var gameObject);
      return gameObject;
    }

    public Building GetBuildingById(Guid buildingId)
    {
      return _repository.FindById(buildingId);
    }

    public void MoveBuilding(Guid buildingId, int newX, int newY)
    {
      var gameObject = GetBuildingObject(buildingId);
      if (gameObject == null) return;

      gameObject.transform.position = new Vector3(newX, 0, newY);
    }

    public void RemoveBuilding(Guid buildingId)
    {
      if (_spawnedBuildingsDict.TryGetValue(buildingId, out var gameObject))
      {
        Object.Destroy(gameObject);
        _spawnedBuildingsDict.Remove(buildingId);
        _spawnedBuildings.Remove(gameObject);
      }
    }
  }
}