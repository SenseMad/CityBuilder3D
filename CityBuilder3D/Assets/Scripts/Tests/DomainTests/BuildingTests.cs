using Scripts.Domain.Models;
using NUnit.Framework;
using System;

namespace Scripts.Tests.DomainTests
{
  public class BuildingTests
  {
    private BuildingType _houseType;

    [SetUp]
    public void Setup()
    {
      _houseType = new BuildingType(
        kind: BuildingKind.Farm,
        baseCost: 100,
        upgradeCosts: new[] { 50, 100 },
        goldPerTick: new[] { 10, 20, 40 },
        prefab: null
      );
    }

    [Test]
    public void Upgrade_IncreasesLevel_AndGoldPerTick()
    {
      var building = new Building(Guid.NewGuid(), _houseType, 0, 0, 0);

      building.Upgrade();
      Assert.AreEqual(1, building.Level);
      Assert.AreEqual(20, building.GetGoldPerTick());

      building.Upgrade();
      Assert.AreEqual(2, building.Level);
      Assert.AreEqual(40, building.GetGoldPerTick());
    }

    [Test]
    public void Upgrade_MaxLevel_Throws()
    {
      var building = new Building(Guid.NewGuid(), _houseType, 0, 0, 2);

      Assert.Throws<InvalidOperationException>(() => building.Upgrade());
    }

    [Test]
    public void Grid_CannotPlace_OccupiedCell()
    {
      var grid = new Grid(3, 3);
      grid.Occupy(1, 1);
      Assert.IsFalse(grid.CanPlace(1, 1));
      Assert.IsTrue(grid.CanPlace(0, 0));
    }
  }
}