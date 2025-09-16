using System;

namespace Scripts.Domain.Models
{
  public sealed class Building
  {
    public Guid Id { get; }
    public BuildingType Type { get; }
    public int Level { get; private set; }
    public int X { get; private set; }
    public int Y { get; private set; }

    public Building(Guid id, BuildingType type, int x, int y, int level = 0)
    {
      Id = id;
      Type = type;
      X = x;
      Y = y;
      Level = level;
    }

    public void MoveTo(int x, int y)
    {
      X = x;
      Y = y;
    }

    public int GetUpgradeCost()
    {
      if (Level >= Type.UpgradeCosts.Length)
        throw new InvalidOperationException("Максимальный уровень");

      return Type.UpgradeCosts[Level];
    }

    public int GetGoldPerTick() => Type.GoldPerTick[Math.Min(Level, Type.GoldPerTick.Length - 1)];

    public void Upgrade()
    {
      if (Level >= Type.UpgradeCosts.Length)
        throw new InvalidOperationException("Максимальный уровень");

      Level++;
    }
  }
}