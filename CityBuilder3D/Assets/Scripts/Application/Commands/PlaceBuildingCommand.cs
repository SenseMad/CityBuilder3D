using Scripts.Domain.Models;

namespace Scripts.Application.Commands
{
  public sealed class PlaceBuildingCommand
  {
    public BuildingType Type { get; }
    public int X { get; }
    public int Y { get; }

    public PlaceBuildingCommand(BuildingType type, int x, int y)
    {
      Type = type;
      X = x;
      Y = y;
    }
  }
}