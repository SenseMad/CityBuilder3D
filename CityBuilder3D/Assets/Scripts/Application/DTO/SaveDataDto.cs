using System;
using System.Collections.Generic;

namespace Scripts.Application.DTO
{
  [Serializable]
  public class SaveDataDto
  {
    public long Gold;
    public List<BuildingDto> Buildings = new();

    [Serializable]
    public class BuildingDto
    {
      public Guid Id;
      public string Kind = "";
      public int X;
      public int Y;
      public int Level;
    }
  }
}