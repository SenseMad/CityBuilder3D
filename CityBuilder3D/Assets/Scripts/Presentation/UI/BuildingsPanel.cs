using Scripts.Application.Commands;
using Cysharp.Threading.Tasks;
using Scripts.Domain.Models;
using Scripts.Application.MessageContracts.Events;
using MessagePipe;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Scripts.Application.UseCases;
using Scripts.Repositories.Building;

namespace Scripts.Presentation.UI
{
  public sealed class BuildingsPanel : MonoBehaviour
  {
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private BuildingHotkeys _hotkeys;

    private PlaceBuildingUseCase _placeBuildingUseCase;
    private MoveBuildingUseCase _moveBuildingUseCase;
    private RemoveBuildingUseCase _removeBuildingUseCase;
    private UpgradeBuildingUseCase _upgradeBuildingUseCase;
    private ISubscriber<BuildingPlacedEvent> _buildingPlacedSubscriber;

    private VisualElement _root;
    private GroupBox _buttonsContainer;
    private Button _moveButton;
    private Button _removeButton;
    private Button _upgradeButton;

    private Label _statusLabel;

    private BuildingType _selectedBuildingType;
    private GameObject _ghostBuilding;

    private BuildingManager _buildingManager;

    private IDisposable _buildingPlacedDisposable;

    private BuildingCatalogSO _buildingCatalog;

    private GameObject _selectedBuildingGO;
    private Material[] _originalMaterials;
    private Guid _selectedBuildingId;
    private bool _isMoveMode;

    private void Awake()
    {
      _hotkeys.OnSelectBuilding += index => SelectBuildingByIndex(index);
      _hotkeys.OnDeleteSelectedBuilding += DeleteSelectedBuilding;
    }

    private void Start()
    {
      _root = _uiDocument.rootVisualElement;
      _buttonsContainer = _root.Q<GroupBox>("BuildingsButtons");
      _statusLabel = _root.Q<Label>("BuildingStatusLabel");

      _moveButton = _root.Q<Button>("MoveButton");
      _removeButton = _root.Q<Button>("RemoveButton");
      _upgradeButton = _root.Q<Button>("UpgradeButton");

      SetActivateBuildingInteractionButtons(false);

      _moveButton.clicked += () => EnterMoveMode();
      _removeButton.clicked += () => RemoveSelectedBuildingAsync().Forget();
      _upgradeButton.clicked += () => UpgradeSelectedBuildingAsync().Forget();

      Populate();
    }

    private void Update()
    {
      var mousePos = GetMouseWorldPosition();
      var cellX = Mathf.RoundToInt(mousePos.x);
      var cellY = Mathf.RoundToInt(mousePos.z);

      if (_isMoveMode && Input.GetMouseButtonDown(0))
      {
        MoveBuildingAsync(_selectedBuildingId, cellX, cellY).Forget();
        _isMoveMode = false;
      }

      if (Input.GetMouseButtonDown(0) && _ghostBuilding == null)
      {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit))
        {
          var view = hit.collider.GetComponentInParent<BuildingView>();
          if (view != null)
          {
            _selectedBuildingId = view.BuildingId;
            _statusLabel.text = $"Выбранно здание {_selectedBuildingId}";

            SetBuildingColor(view.gameObject, Color.yellow);

            SetActivateBuildingInteractionButtons(true);
          }
        }
      }

      if (Input.GetMouseButtonDown(1))
      {
        DestroyGhost();
        _statusLabel.text = "";
      }

      if (_selectedBuildingType == null || _ghostBuilding == null)
        return;

      _ghostBuilding.transform.position = new Vector3(cellX, 0, cellY);

      var grid = _placeBuildingUseCase.Grid;
      var canPlace = grid.CanPlace(cellX, cellY);
      SetGhostColor(_ghostBuilding, canPlace ? Color.gray : Color.red);

      if (Input.GetMouseButtonDown(0) && canPlace)
        PlaceBuildingAsync(cellX, cellY).Forget();
    }

    private void OnDestroy()
    {
      _buildingPlacedDisposable?.Dispose();
      if (_ghostBuilding != null)
        Destroy(_ghostBuilding);
    }

    public void Initialize(
      PlaceBuildingUseCase placeBuildingUseCase,
      MoveBuildingUseCase moveBuildingUseCase,
      RemoveBuildingUseCase removeBuildingUseCase,
      UpgradeBuildingUseCase upgradeBuildingUseCase,
      ISubscriber<BuildingPlacedEvent> buildingPlacedSubscriber,
      BuildingCatalogSO buildingCatalogSO,
      BuildingManager buildingManager
      )
    {
      _placeBuildingUseCase = placeBuildingUseCase;
      _moveBuildingUseCase = moveBuildingUseCase;
      _removeBuildingUseCase = removeBuildingUseCase;
      _upgradeBuildingUseCase = upgradeBuildingUseCase;
      _buildingPlacedSubscriber = buildingPlacedSubscriber;
      _buildingCatalog = buildingCatalogSO;
      _buildingManager = buildingManager;

      _buildingPlacedDisposable = _buildingPlacedSubscriber.Subscribe(OnBuildingPlaced);
    }

    public void Populate()
    {
      _buttonsContainer.Clear();

      foreach (var typeSO in _buildingCatalog.Buildings)
      {
        var localType = typeSO;
        var button = new Button(() => OnBuildingSelected(localType.ToDomainModel()))
        {
          text = $"{localType.Kind} ({localType.BaseCost}G)"
        };

        _buttonsContainer.Add(button);
      }
    }

    private async UniTaskVoid PlaceBuildingAsync(int x, int y)
    {
      var command = new PlaceBuildingCommand(_selectedBuildingType, x, y);
      var result = await _placeBuildingUseCase.ExecuteAsync(command);

      if (!result.IsSuccess)
        _statusLabel.text = result.Error;
      else
        _placeBuildingUseCase.Grid.Occupy(x, y);
    }

    private async UniTaskVoid MoveBuildingAsync(Guid id, int x, int y)
    {
      var result = await _moveBuildingUseCase.ExecuteAsync(id, x, y);

      if (!result.IsSuccess)
      {
        _statusLabel.text = result.Error;
        return;
      }

      _statusLabel.text = $"Здание переехало в ({x},{y})";

      _buildingManager.MoveBuilding(id, x, y);

      DestroyGhost();

      _isMoveMode = false;
    }

    private async UniTaskVoid RemoveSelectedBuildingAsync()
    {
      if (_selectedBuildingId == Guid.Empty)
      {
        _statusLabel.text = "Сначала выберите здание";
        return;
      }

      var result = await _removeBuildingUseCase.ExecuteAsync(_selectedBuildingId);

      if (!result.IsSuccess)
      {
        _statusLabel.text = result.Error;
        return;
      }

      _buildingManager.RemoveBuilding(_selectedBuildingId);

      _statusLabel.text = "Здание удалено";

      DestroyGhost();
    }

    private async UniTaskVoid UpgradeSelectedBuildingAsync()
    {
      if (_selectedBuildingId == Guid.Empty)
      {
        _statusLabel.text = "Сначала выберите здание";
        return;
      }

      var result = await _upgradeBuildingUseCase.ExecuteAsync(_selectedBuildingId);
      _statusLabel.text = result.IsSuccess ? "Здание улучшено" : result.Error;
    }

    private void OnBuildingSelected(BuildingType type)
    {
      _selectedBuildingType = type;

      if (_ghostBuilding != null)
        Destroy(_ghostBuilding);

      _ghostBuilding = Instantiate(type.Prefab, Vector3.zero, Quaternion.identity);
      _ghostBuilding.GetComponent<Collider>().enabled = false;
      SetGhostMaterialTransparent(_ghostBuilding);
    }

    private void OnBuildingPlaced(BuildingPlacedEvent e)
    {
      _statusLabel.text = $"{e.Type} placed at ({e.X},{e.Y})";

      if (_buildingManager != null)
      {
        var buildingType = _buildingCatalog.GetBuildingType(e.Type);
        if (buildingType != null)
        {
          var building = new Building(e.BuildingId, buildingType, e.X, e.Y, e.Level);
          _buildingManager.SpawnSingleBuilding(building);
        }
      }

      DestroyGhost();
    }

    private void SetActivateBuildingInteractionButtons(bool value)
    {
      _moveButton.visible = value;
      _removeButton.visible = value;
      _upgradeButton.visible = value;
    }

    private void DestroyGhost()
    {
      if (_ghostBuilding != null)
      {
        Destroy(_ghostBuilding);
        _ghostBuilding = null;
      }

      RestoreOriginalMaterials(_selectedBuildingGO);

      _selectedBuildingId = Guid.Empty;
      _selectedBuildingType = null;

      SetActivateBuildingInteractionButtons(false);
    }

    private void EnterMoveMode()
    {
      if (_selectedBuildingId == Guid.Empty)
      {
        _statusLabel.text = "Сначала выберите здание";
        return;
      }

      _isMoveMode = true;
      _statusLabel.text = "Режим перемещения: нажмите на ячейку, чтобы разместить здание";

      var building = _buildingManager.GetBuildingById(_selectedBuildingId);
      if (building != null)
      {
        if (_ghostBuilding != null)
          Destroy(_ghostBuilding);

        _ghostBuilding = Instantiate(building.Type.Prefab, new Vector3(building.X, 0, building.Y), Quaternion.identity);
        SetGhostMaterialTransparent(_ghostBuilding);
      }
    }

    private void SetGhostColor(GameObject ghost, Color color)
    {
      var renderers = ghost.GetComponentsInChildren<Renderer>();
      foreach (var r in renderers)
      {
        r.material.color = color;
      }
    }

    private void SetGhostMaterialTransparent(GameObject ghost)
    {
      var renderers = ghost.GetComponentsInChildren<Renderer>();
      foreach (var r in renderers)
      {
        var mat = r.material;
        var c = mat.color;
        c.a = 0.5f;
        mat.color = c;
      }
    }

    private void SetBuildingColor(GameObject buildingGO, Color color)
    {
      if (_selectedBuildingGO != null)
        RestoreOriginalMaterials(_selectedBuildingGO);

      _selectedBuildingGO = buildingGO;

      var renderers = _selectedBuildingGO.GetComponentsInChildren<Renderer>();
      _originalMaterials = new Material[renderers.Length];
      for (int i = 0; i < renderers.Length; i++)
      {
        _originalMaterials[i] = renderers[i].material;
        var matCopy = new Material(renderers[i].material)
        {
          color = color
        };
        renderers[i].material = matCopy;
      }
    }

    private void RestoreOriginalMaterials(GameObject buildingGO)
    {
      if (_originalMaterials == null)
        return;

      var renderers = buildingGO.GetComponentsInChildren<Renderer>();
      for (int i = 0; i < renderers.Length && i < _originalMaterials.Length; i++)
        renderers[i].material = _originalMaterials[i];

      _selectedBuildingGO = null;
      _originalMaterials = null;
    }

    private Vector3 GetMouseWorldPosition()
    {
      var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray, out var hit))
      {
        return hit.point;
      }
      return Vector3.zero;
    }

    private void SelectBuildingByIndex(int index)
    {
      var type = _buildingCatalog.Buildings[index].ToDomainModel();
      OnBuildingSelected(type);
    }

    private void DeleteSelectedBuilding()
    {
      if (_selectedBuildingId != Guid.Empty)
        RemoveSelectedBuildingAsync().Forget();
    }
  }
}