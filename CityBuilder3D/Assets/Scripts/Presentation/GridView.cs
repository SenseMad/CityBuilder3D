using UnityEngine;

namespace Scripts.Presentation
{
  public sealed class GridView : MonoBehaviour
  {
    [SerializeField] private int _width = 32;
    [SerializeField] private int _height = 32;
    [SerializeField] private float _cellSize = 1f;
    [SerializeField] private Material _freeCellMaterial;
    [SerializeField] private Material _blockedCellMaterial;

    private GameObject[,] _cells;

    private void Awake()
    {
      _cells = new GameObject[_width, _height];

      for (int x = 0; x < _width; x++)
      {
        for (int y = 0; y < _height; y++)
        {
          var cellGameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
          cellGameObject.transform.position = new Vector3(x, 0, y);
          cellGameObject.transform.rotation = Quaternion.Euler(90, 0, 0);
          cellGameObject.transform.localScale = Vector3.one * _cellSize;
          cellGameObject.GetComponent<MeshRenderer>().material = _freeCellMaterial;

          _cells[x, y] = cellGameObject;
        }
      }
    }

    public void HighlightCell(int x, int y, bool canPlace)
    {
      if (x < 0 || x >= _width || y < 0 || y >= _height)
        return;

      _cells[x, y].GetComponent<MeshRenderer>().material = canPlace ? _freeCellMaterial : _blockedCellMaterial;
    }
  }
}