using Scripts.Domain.Models;
using UnityEngine;

namespace Scripts.Repositories.Building
{
  [CreateAssetMenu(fileName = "BuildingCatalog", menuName = "CityBuilder/BuildingCatalog")]
  public class BuildingCatalogSO : ScriptableObject
  {
    public BuildingTypeSO[] Buildings;

    public BuildingType? GetBuildingType(string key)
    {
      foreach (var building in Buildings)
        if (building.Kind.ToString() == key)
          return building.ToDomainModel();

      return null;
    }
  }
}