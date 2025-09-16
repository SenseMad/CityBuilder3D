using Cysharp.Threading.Tasks;
using Scripts.Domain.Models;
using Scripts.Application.MessageContracts.Events;
using MessagePipe;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;
using Scripts.Application.UseCases;

namespace Scripts.Presentation.UI
{
  public sealed class SaveLoadPanel : MonoBehaviour
  {
    [SerializeField] private UIDocument _uIDocument;

    [SerializeField] private float _autoSaveInterval = 5f;

    private CancellationTokenSource _autoSaveCts;

    private SaveGameUseCase _saveGameUse;
    private LoadGameUseCase _loadGameUse;

    private BuildingManager _buildingManager;

    private Button _saveButton;
    private Button _loadButton;
    private Label _statusLabel;

    private IDisposable _savedSubscriber;
    private IDisposable _loadedSubscriber;

    private void Start()
    {
      var root = _uIDocument.rootVisualElement;
      _saveButton = root.Q<Button>("SaveButton");
      _loadButton = root.Q<Button>("LoadButton");
      _statusLabel = root.Q<Label>("SaveStatusLabel");

      _saveButton.clicked += OnSaveClicked;
      _loadButton.clicked += OnLoadClicked;

      StartAutoSave();
    }

    private void OnDestroy()
    {
      _savedSubscriber?.Dispose();
      _loadedSubscriber?.Dispose();
    }

    public void Initialize(
      SaveGameUseCase saveGameUse, 
      LoadGameUseCase loadGameUse, 
      ISubscriber<GameSavedEvent> savedSubscriber, 
      ISubscriber<GameLoadedEvent> loadedSubscriber,
      BuildingManager buildingManager)
    {
      _saveGameUse = saveGameUse;
      _loadGameUse = loadGameUse;

      _savedSubscriber = savedSubscriber.Subscribe(_ => _statusLabel.text = "���� ���������!");
      _loadedSubscriber = loadedSubscriber.Subscribe(_ => _statusLabel.text = "���� ���������!");

      _buildingManager = buildingManager;
    }

    private async void OnSaveClicked()
    {
      _statusLabel.text = "����������...";
      var result = await _saveGameUse.ExecuteAsync();
      if (!result.IsSuccess)
        _statusLabel.text = $"��������� �� �������: {result.Error}";
    }
    
    private async void OnLoadClicked()
    {
      _statusLabel.text = "��������...";

      _buildingManager.ClearAllBuildings();

      var result = await _loadGameUse.ExecuteAsync();
      if (!result.IsSuccess)
      {
        _statusLabel.text = $"���� ��������: {result.Error}";
        return;
      }

      _buildingManager.SpawnAllBuildingsFromRepository();

      _statusLabel.text = "���� ���������!";
    }

    private void StartAutoSave()
    {
      _autoSaveCts = new CancellationTokenSource();
      AutoSaveLoop(_autoSaveCts.Token).Forget();
    }

    private async UniTaskVoid AutoSaveLoop(CancellationToken token)
    {
      try
      {
        while (!token.IsCancellationRequested)
        {
          await UniTask.Delay(TimeSpan.FromSeconds(_autoSaveInterval), cancellationToken: token);
          if (token.IsCancellationRequested) break;

          _statusLabel.text = "�������������� ����������...";
          var result = await _saveGameUse.ExecuteAsync(token);

          if (!result.IsSuccess)
            _statusLabel.text = $"�� ������� ��������� �������������� ����������: {result.Error}";
          else
            _statusLabel.text = "�������������� ����������!";
        }
      }
      catch (OperationCanceledException) { }
    }
  }
}