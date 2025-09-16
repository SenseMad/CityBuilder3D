using System;
using UnityEngine;

namespace Scripts.Presentation
{
  public sealed class BuildingView : MonoBehaviour
  {
    public Guid BuildingId { get; private set; }

    public void SetBuildingId(Guid buildingId)
    {
      BuildingId = buildingId;
    }
  }
}