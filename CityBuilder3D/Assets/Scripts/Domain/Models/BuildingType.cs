using UnityEngine;

namespace Scripts.Domain.Models
{
  public sealed class BuildingType
  {
    public BuildingKind Kind { get; }
    public int BaseCost { get; }
    public int[] UpgradeCosts { get; }
    public int[] GoldPerTick { get; }
    public GameObject Prefab { get; }

    public BuildingType(BuildingKind kind, int baseCost, int[] upgradeCosts, int[] goldPerTick, GameObject prefab)
    {
      Kind = kind;
      BaseCost = baseCost;
      UpgradeCosts = upgradeCosts;
      GoldPerTick = goldPerTick;
      Prefab = prefab;
    }
  }
}