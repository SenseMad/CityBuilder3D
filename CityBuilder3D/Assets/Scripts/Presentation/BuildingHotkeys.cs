using System;
using UnityEngine;

namespace Scripts.Presentation
{
  public sealed class BuildingHotkeys : MonoBehaviour
  {
    public event Action<int> OnSelectBuilding;
    public event Action OnDeleteSelectedBuilding;

    private CityBuilderInput _input;

    private void Awake()
    {
      _input = new CityBuilderInput();

      _input.Hotkeys.SelectBuilding1.performed += _ => OnSelectBuilding?.Invoke(0);
      _input.Hotkeys.SelectBuilding2.performed += _ => OnSelectBuilding?.Invoke(1);
      _input.Hotkeys.SelectBuilding3.performed += _ => OnSelectBuilding?.Invoke(2);

      _input.Hotkeys.DeleteBuilding.performed += _ => OnDeleteSelectedBuilding?.Invoke();
    }

    private void OnEnable() => _input.Enable();
    private void OnDisable() => _input.Disable();
  }
}