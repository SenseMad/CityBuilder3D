using Scripts.Application.Interfaces;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.Presentation.UI
{
  public sealed class EconomyPanel : MonoBehaviour
  {
    [SerializeField] private UIDocument _uiDocument;

    private Label _goldLabel;

    private IEconomyService _economyService;

    private IDisposable _disposable;

    private void Start()
    {
      var root = _uiDocument.rootVisualElement;
      _goldLabel = root.Q<Label>("GoldLabel");

      OnGoldChanged(_economyService.GetGold());

      _disposable = _economyService.Gold.Subscribe(OnGoldChanged);
    }

    public void Initialize(IEconomyService economyService)
    {
      _economyService = economyService;
    }

    private void OnGoldChanged(long newGold)
    {
      _goldLabel.text = $"Золото: {newGold} (+{_economyService.GoldPerSecond} sec)";
    }

    private void OnDestroy()
    {
      _disposable?.Dispose();
    }
  }
}