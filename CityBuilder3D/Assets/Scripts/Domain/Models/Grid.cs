using System.Collections.Generic;

namespace Scripts.Domain.Models
{
  public sealed class Grid
  {
    public int Width { get; }
    public int Height { get; }

    private readonly HashSet<(int x, int y)> _occupied = new();

    public Grid(int width, int height)
    {
      Width = width;
      Height = height;
    }

    /// <summary>
    /// Проверяет, находится ли клетка внутри границ сетки
    /// </summary>
    /// <param name="x">Координата X</param>
    /// <param name="y">Координата Y</param>
    /// <returns>true, если клетка внутри сетки; иначе false</returns>
    public bool IsInside(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;
    /// <summary>
    /// Проверяет, занята ли клетка
    /// </summary>
    /// <param name="x">Координата X</param>
    /// <param name="y">Координата Y</param>
    /// <returns>true, если клетка занята; иначе false</returns>
    public bool IsOccupied(int x, int y) => _occupied.Contains((x, y));
    /// <summary>
    /// Проверяет, можно ли поставить объект на данную клетку
    /// </summary>
    /// <param name="x">Координата X</param>
    /// <param name="y">Координата Y</param>
    /// <returns>true, если клетка внутри сетки и свободна; иначе false</returns>
    public bool CanPlace(int x, int y) => IsInside(x, y) && !_occupied.Contains((x, y));

    /// <summary>
    /// Отмечает клетку как занятую
    /// </summary>
    /// <param name="x">Координата X</param>
    /// <param name="y">Координата Y</param>
    public void Occupy(int x, int y) => _occupied.Add((x, y));
    /// <summary>
    /// Освобождает клетку
    /// </summary>
    /// <param name="x">Координата X</param>
    /// <param name="y">Координата Y</param>
    public void Vacate(int x, int y) => _occupied.Remove((x, y));
    /// <summary>
    /// Cброс всех занятых клеток
    /// </summary>
    public void ClearOccupation() => _occupied.Clear();
  }
}