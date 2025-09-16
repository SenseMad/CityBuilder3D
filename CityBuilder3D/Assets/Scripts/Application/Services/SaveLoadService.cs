using Cysharp.Threading.Tasks;
using Scripts.Application.DTO;
using Scripts.Application.Interfaces;
using Scripts.Application.MessageContracts.Events;
using MessagePipe;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Scripts.Application.Services
{
  public class SaveLoadService : ISaveLoadService
  {
    private readonly IPublisher<GameSavedEvent> _savedPublisher;
    private readonly IPublisher<GameLoadedEvent> _loadedPublisher;

    private readonly string _savePath;

    public SaveLoadService(IPublisher<GameSavedEvent> savedPublisher, IPublisher<GameLoadedEvent> loadedPublisher)
    {
      _savedPublisher = savedPublisher;
      _loadedPublisher = loadedPublisher;

      _savePath = Path.Combine(UnityEngine.Application.persistentDataPath, "SaveGame.json");
    }

    public async UniTask SaveAsync(SaveDataDto saveDataDto, CancellationToken cancellationToken = default)
    {
      try
      {
        var json = JsonUtility.ToJson(saveDataDto, true);
        await File.WriteAllTextAsync(_savePath, json, cancellationToken);

        _savedPublisher.Publish(new GameSavedEvent());
      }
      catch (System.Exception ex)
      {
        Debug.LogError($"Сохранить не удалось: {ex.Message}");
        throw;
      }
    }

    public async UniTask<SaveDataDto?> LoadAsync(CancellationToken cancellationToken = default)
    {
      try
      {
        if (!File.Exists(_savePath))
        {
          Debug.LogWarning("Файл сохранения не найден");
          return null;
        }

        var json = await File.ReadAllTextAsync(_savePath, cancellationToken);
        var saveDataDto = JsonUtility.FromJson<SaveDataDto>(json);

        if (saveDataDto == null)
        {
          Debug.LogWarning("Не удалось разобрать сохраненный файл");
          return null;
        }

        _loadedPublisher.Publish(new GameLoadedEvent());

        return saveDataDto;
      }
      catch (System.Exception ex)
      {
        Debug.LogError($"Сбой загрузки: {ex.Message}");
        return null;
      }
    }
  }
}