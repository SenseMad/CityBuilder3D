using System.Collections.Generic;
using Scripts.Domain.Models;
using System;

namespace Scripts.Application.Interfaces
{
  public interface IBuildingRepository
  {
    void Add(Building building);
    bool Remove(Guid id);
    bool Update(Building building);

    Building? FindById(Guid id);
    Building? FindAt(int x, int y);
    IEnumerable<Building> GetAll();
    void Clear();
  }
}