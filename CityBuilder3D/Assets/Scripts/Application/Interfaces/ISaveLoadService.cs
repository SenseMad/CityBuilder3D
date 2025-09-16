using System.Threading;
using Cysharp.Threading.Tasks;
using Scripts.Application.DTO;

namespace Scripts.Application.Interfaces
{
  /// <summary>
  /// Асинхронный сервис сохранения/загрузки
  /// </summary>
  public interface ISaveLoadService
  {
    UniTask SaveAsync(SaveDataDto dto, CancellationToken cancellationToken = default);
    UniTask<SaveDataDto?> LoadAsync(CancellationToken cancellationToken = default);
  }
}