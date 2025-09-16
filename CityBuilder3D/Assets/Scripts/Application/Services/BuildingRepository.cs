using Scripts.Domain.Models;
using Scripts.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scripts.Application.Services
{
  public sealed class BuildingRepository : IBuildingRepository
  {
    private readonly Dictionary<Guid, Building> _buildings = new();

    public void Add(Building building)
    {
      _buildings[building.Id] = building;
    }

    public bool Remove(Guid id)
    {
      return _buildings.Remove(id);
    }

    public bool Update(Building building)
    {
      if (!_buildings.ContainsKey(building.Id))
        return false;

      _buildings[building.Id] = building;
      return true;
    }

    public Building FindById(Guid id)
    {
      _buildings.TryGetValue(id, out var building);
      return building;
    }

    public Building FindAt(int x, int y)
    {
      return _buildings.Values.FirstOrDefault(building => building.X == x && building.Y == y);
    }

    public IEnumerable<Building> GetAll() => _buildings.Values;

    public void Clear() => _buildings.Clear();
  }
}