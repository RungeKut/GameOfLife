using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameOfLife.Core;

namespace GameOfLife.Data
{
    /// <summary>
    /// Менеджер системы сохранений.
    /// 
    /// Управляет всеми операциями сохранения и загрузки:
    /// - Создание новых сохранений
    /// - Загрузка существующих сохранений
    /// - Управление списком сохранений
    /// - Автосохранение
    /// - Резервное копирование
    /// 
    /// Архитектурные особенности:
    /// - Поддержка множественных форматов через ISaveLoad
    /// - Централизованное управление путями сохранения
    /// - Валидация и миграция данных
    /// - События для уведомления об изменениях
    /// 
    /// Использование:
    /// var manager = new SaveLoadManager();
    /// manager.Save("mysave");
    /// manager.Load("mysave");
    /// var saves = manager.GetSaveList();
    /// </summary>
    public class SaveLoadManager : IDisposable
    {
        #region Приватные поля

        /// <summary>
        /// Система сохранения (JSON, Binary, etc).
        /// </summary>
        private readonly ISaveLoad _saveSystem;

        /// <summary>
        /// Базовая директория для сохранений.
        /// </summary>
        private readonly string _saveDirectory;

        /// <summary>
        /// Блокировка для потокобезопасных операций.
        /// </summary>
        private readonly object _lockObject = new object();

        /// <summary>
        /// Таймер для автосохранения.
        /// </summary>
        private System.Timers.Timer _autoSaveTimer;

        /// <summary>
        /// Интервал автосохранения в миллисекундах.
        /// </summary>
        private int _autoSaveInterval = 300000; // 5 минут

        /// <summary>
        /// Флаг включённого автосохранения.
        /// </summary>
        private bool _autoSaveEnabled;

        /// <summary>
        /// Последнее сохранённое состояние.
        /// </summary>
        private SaveGame _lastSaveData;

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Включено ли автосохранение.
        /// </summary>
        public bool AutoSaveEnabled
        {
            get => _autoSaveEnabled;
            set
            {
                _autoSaveEnabled = value;
                if (_autoSaveTimer != null)
                {
                    _autoSaveTimer.Enabled = value;
                }
            }
        }

        /// <summary>
        /// Интервал автосохранения в миллисекундах.
        /// </summary>
        public int AutoSaveInterval
        {
            get => _autoSaveInterval;
            set
            {
                _autoSaveInterval = value;
                if (_autoSaveTimer != null)
                {
                    _autoSaveTimer.Interval = value;
                }
            }
        }

        /// <summary>
        /// Расширение файлов сохранений.
        /// </summary>
        public string SaveExtension => ".golsave";

        #endregion

        #region События

        /// <summary>
        /// Событие успешного сохранения.
        /// </summary>
        public event Action<string> OnSaveCompleted;

        /// <summary>
        /// Событие успешной загрузки.
        /// </summary>
        public event Action<string> OnLoadCompleted;

        /// <summary>
        /// Событие ошибки сохранения/загрузки.
        /// </summary>
        public event Action<string, string> OnSaveLoadError;

        /// <summary>
        /// Событие автосохранения.
        /// </summary>
        public event Action OnAutoSave;

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новый менеджер сохранений.
        /// </summary>
        /// <param name="saveDirectory">
        /// Директория для сохранений.
        /// По умолчанию: %APPDATA%/GameOfLife/Saves
        /// </param>
        /// <param name="saveSystem">
        /// Система сохранения.
        /// По умолчанию: JsonSaveLoad
        /// </param>
        public SaveLoadManager(
            string saveDirectory = null,
            ISaveLoad saveSystem = null)
        {
            // Директория по умолчанию
            if (string.IsNullOrEmpty(saveDirectory))
            {
                string appData = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.ApplicationData);
                _saveDirectory = Path.Combine(appData, "GameOfLife", "Saves");
            }
            else
            {
                _saveDirectory = saveDirectory;
            }

            // Создаём директорию если не существует
            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
            }

            // Система сохранения по умолчанию
            _saveSystem = saveSystem ?? new JsonSaveLoad();

            // Настраиваем таймер автосохранения
            _autoSaveTimer = new System.Timers.Timer(_autoSaveInterval);
            _autoSaveTimer.Elapsed += OnAutoSaveTimerElapsed;
            _autoSaveTimer.Enabled = false;

            Console.WriteLine($"SaveLoadManager инициализирован: {_saveDirectory}");
        }

        #endregion

        #region Основные методы сохранения

        /// <summary>
        /// Сохраняет состояние симуляции.
        /// 
        /// Создаёт новое сохранение с указанным именем.
        /// Если имя не указано, используется имя по умолчанию.
        /// </summary>
        /// <param name="engine">Движок симуляции.</param>
        /// <param name="mode">Режим симуляции.</param>
        /// <param name="saveName">Имя сохранения.</param>
        /// <returns>Путь к файлу сохранения или null.</returns>
        public string Save(
            SimulationEngine engine,
            string mode,
            string saveName = null)
        {
            lock (_lockObject)
            {
                try
                {
                    // Генерируем имя файла
                    if (string.IsNullOrEmpty(saveName))
                    {
                        saveName = $"Save_{DateTime.Now:yyyyMMdd_HHmmss}";
                    }

                    string filePath = GetSaveFilePath(saveName);

                    // Создаём данные сохранения
                    var saveData = new SaveGame(engine, mode);
                    saveData.SaveName = saveName;

                    // Сохраняем
                    bool success = _saveSystem.Save(saveData, filePath);

                    if (success)
                    {
                        _lastSaveData = saveData;
                        OnSaveCompleted?.Invoke(filePath);
                        Console.WriteLine($"Сохранено: {filePath}");
                        return filePath;
                    }
                    else
                    {
                        OnSaveLoadError?.Invoke(filePath, "Ошибка сохранения");
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    OnSaveLoadError?.Invoke("", ex.Message);
                    Console.WriteLine($"Ошибка сохранения: {ex.Message}");
                    return null;
                }
            }
        }

        /// <summary>
        /// Сохраняет указанные данные.
        /// </summary>
        public bool SaveData(SaveGame saveData, string saveName)
        {
            lock (_lockObject)
            {
                string filePath = GetSaveFilePath(saveName);
                bool success = _saveSystem.Save(saveData, filePath);

                if (success)
                {
                    _lastSaveData = saveData;
                    OnSaveCompleted?.Invoke(filePath);
                }
                else
                {
                    OnSaveLoadError?.Invoke(filePath, "Ошибка сохранения");
                }

                return success;
            }
        }

        /// <summary>
        /// Загружает сохранение по имени.
        /// </summary>
        /// <param name="saveName">Имя сохранения.</param>
        /// <returns>Загруженные данные или null.</returns>
        public SaveGame Load(string saveName)
        {
            lock (_lockObject)
            {
                try
                {
                    string filePath = GetSaveFilePath(saveName);
                    var saveData = _saveSystem.Load(filePath);

                    if (saveData != null)
                    {
                        _lastSaveData = saveData;
                        OnLoadCompleted?.Invoke(filePath);
                        Console.WriteLine($"Загружено: {filePath}");
                        return saveData;
                    }
                    else
                    {
                        OnSaveLoadError?.Invoke(filePath, "Ошибка загрузки");
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    OnSaveLoadError?.Invoke("", ex.Message);
                    Console.WriteLine($"Ошибка загрузки: {ex.Message}");
                    return null;
                }
            }
        }

        /// <summary>
        /// Загружает сохранение из полного пути.
        /// </summary>
        public SaveGame LoadFromPath(string filePath)
        {
            lock (_lockObject)
            {
                var saveData = _saveSystem.Load(filePath);
                if (saveData != null)
                {
                    _lastSaveData = saveData;
                    OnLoadCompleted?.Invoke(filePath);
                }
                return saveData;
            }
        }

        #endregion

        #region Управление списком сохранений

        /// <summary>
        /// Получает список всех сохранений.
        /// </summary>
        /// <returns>Список информации о сохранениях.</returns>
        public List<SaveInfo> GetSaveList()
        {
            lock (_lockObject)
            {
                var saves = new List<SaveInfo>();

                try
                {
                    string[] files = Directory.GetFiles(
                        _saveDirectory,
                        $"*{SaveExtension}");

                    foreach (var file in files)
                    {
                        var info = _saveSystem.GetSaveInfo(file);
                        if (info != null)
                        {
                            saves.Add(info);
                        }
                    }

                    // Сортируем по дате изменения (новые первые)
                    return saves.OrderByDescending(s => s.ModifiedDate).ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка получения списка сохранений: {ex.Message}");
                    return new List<SaveInfo>();
                }
            }
        }

        /// <summary>
        /// Удаляет сохранение по имени.
        /// </summary>
        public bool DeleteSave(string saveName)
        {
            lock (_lockObject)
            {
                string filePath = GetSaveFilePath(saveName);
                return _saveSystem.Delete(filePath);
            }
        }

        /// <summary>
        /// Проверяет существует ли сохранение.
        /// </summary>
        public bool SaveExists(string saveName)
        {
            string filePath = GetSaveFilePath(saveName);
            return _saveSystem.Exists(filePath);
        }

        /// <summary>
        /// Получает путь к файлу сохранения.
        /// </summary>
        private string GetSaveFilePath(string saveName)
        {
            // Очищаем имя от недопустимых символов
            saveName = MakeValidFileName(saveName);
            return Path.Combine(_saveDirectory, saveName + SaveExtension);
        }

        /// <summary>
        /// Делает имя файла допустимым.
        /// </summary>
        private string MakeValidFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name;
        }

        #endregion

        #region Автосохранение

        /// <summary>
        /// Включает автосохранение.
        /// </summary>
        public void EnableAutoSave()
        {
            AutoSaveEnabled = true;
            Console.WriteLine($"Автосохранение включено (интервал: {_autoSaveInterval}мс)");
        }

        /// <summary>
        /// Выключает автосохранение.
        /// </summary>
        public void DisableAutoSave()
        {
            AutoSaveEnabled = false;
            Console.WriteLine("Автосохранение выключено");
        }

        /// <summary>
        /// Обработчик таймера автосохранения.
        /// </summary>
        private void OnAutoSaveTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            OnAutoSave?.Invoke();
            Console.WriteLine("Автосохранение...");
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Освобождает ресурсы менеджера.
        /// </summary>
        public void Dispose()
        {
            _autoSaveTimer?.Dispose();
            Console.WriteLine("SaveLoadManager освобождён");
        }

        #endregion
    }
}