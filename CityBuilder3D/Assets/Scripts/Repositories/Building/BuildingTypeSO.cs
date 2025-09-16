using Scripts.Domain.Models;
using UnityEngine;

namespace Scripts.Repositories.Building
{
  [CreateAssetMenu(fileName = "BuildingTypeSO", menuName = "CityBuilder/BuildingType")]
  public class BuildingTypeSO : ScriptableObject
  {
    public BuildingKind Kind;
    public int BaseCost;
    public int[] UpgradeCosts;
    public int[] GoldPerTick;
    public GameObject Prefab;

    public BuildingType ToDomainModel()
    {
      return new BuildingType(Kind, BaseCost, UpgradeCosts, GoldPerTick, Prefab);
    }
  }
}