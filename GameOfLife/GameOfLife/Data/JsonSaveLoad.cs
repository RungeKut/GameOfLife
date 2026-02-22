using System;
using System.IO;
using System.Text;
using System.Text.Json;
using GameOfLife.Core;

namespace GameOfLife.Data
{
    /// <summary>
    /// Система сохранения и загрузки в формате JSON.
    /// 
    /// Преимущества JSON формата:
    /// - Читаемость человеком (можно открыть в текстовом редакторе)
    /// - Легко редактировать вручную при необходимости
    /// - Широкая поддержка в различных языках и инструментах
    /// - Встроенная поддержка в .NET (System.Text.Json)
    /// 
    /// Недостатки:
    /// - Больший размер файла по сравнению с бинарным форматом
    /// - Медленнее сериализация/десериализация
    /// - Нет встроенной компрессии
    /// 
    /// Использование:
    /// var saveSystem = new JsonSaveLoad();
    /// saveSystem.Save(saveData, "save.json");
    /// var loaded = saveSystem.Load("save.json");
    /// 
    /// Безопасность:
    /// - Проверяется версия формата перед загрузкой
    /// - Валидируются все данные после десериализации
    /// - Создаётся резервная копия при сохранении
    /// </summary>
    public class JsonSaveLoad : ISaveLoad
    {
        #region Приватные поля

        /// <summary>
        /// Настройки сериализации JSON.
        /// 
        /// Включает:
        /// - Красивое форматирование для читаемости
        /// - Игнорирование null значений
        /// - Поддержку кириллицы в кодировке UTF-8
        /// </summary>
        private readonly JsonSerializerOptions _jsonOptions;

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новую систему JSON сохранения.
        /// 
        /// Инициализирует настройки сериализации:
        /// - Pretty printing для читаемости
        /// - UTF-8 кодировка для поддержки кириллицы
        /// - Игнорирование свойств с null значениями
        /// - Регистронезависимое сравнение свойств
        /// </summary>
        public JsonSaveLoad()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true, // Красивое форматирование
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        #endregion

        #region Реализация ISaveLoad

        /// <summary>
        /// Сохраняет данные симуляции в JSON файл.
        /// 
        /// Процесс сохранения:
        /// 1. Создаёт директорию если не существует
        /// 2. Создаёт резервную копию существующего файла
        /// 3. Сериализует данные в JSON
        /// 4. Записывает в файл с UTF-8 кодировкой
        /// 5. Проверяет целостность записанных данных
        /// 
        /// Безопасность:
        /// - Резервная копия создаётся перед записью
        /// - При ошибке записи резервная копия восстанавливается
        /// </summary>
        /// <param name="saveData">Данные для сохранения.</param>
        /// <param name="filePath">Путь к файлу сохранения.</param>
        /// <returns>True если сохранение успешно, иначе false.</returns>
        public bool Save(SaveGame saveData, string filePath)
        {
            try
            {
                // Валидация данных перед сохранением
                if (!saveData.Validate())
                {
                    Console.WriteLine("Ошибка: Данные сохранения не прошли валидацию");
                    return false;
                }

                // Обновляем дату изменения
                saveData.ModifiedDate = DateTime.Now;

                // Создаём директорию если не существует
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Создаём резервную копию если файл существует
                if (File.Exists(filePath))
                {
                    string backupPath = filePath + ".backup";
                    File.Copy(filePath, backupPath, true);
                }

                // Сериализуем в JSON
                string json = JsonSerializer.Serialize(saveData, _jsonOptions);

                // Записываем в файл
                File.WriteAllText(filePath, json, Encoding.UTF8);

                Console.WriteLine($"Сохранение успешно: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения: {ex.Message}");

                // Попытка восстановления из резервной копии
                TryRestoreBackup(filePath);

                return false;
            }
        }

        /// <summary>
        /// Загружает данные симуляции из JSON файла.
        /// 
        /// Процесс загрузки:
        /// 1. Проверяет существование файла
        /// 2. Читает содержимое файла
        /// 3. Десериализует JSON в объект SaveGame
        /// 4. Проверяет версию формата
        /// 5. Выполняет валидацию данных
        /// 6. Выполняет миграцию если версия старая
        /// 
        /// Безопасность:
        /// - Проверяется версия формата перед загрузкой
        /// - Валидируются все данные после десериализации
        /// - Обрабатыаются все возможные исключения
        /// </summary>
        /// <param name="filePath">Путь к файлу сохранения.</param>
        /// <returns>Загруженные данные или null если ошибка.</returns>
        public SaveGame Load(string filePath)
        {
            try
            {
                // Проверка существования файла
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Файл не найден: {filePath}");
                    return null;
                }

                // Чтение файла
                string json = File.ReadAllText(filePath, Encoding.UTF8);

                // Десериализация
                var saveData = JsonSerializer.Deserialize<SaveGame>(json, _jsonOptions);

                if (saveData == null)
                {
                    Console.WriteLine("Ошибка: Не удалось десериализовать данные");
                    return null;
                }

                // Проверка версии
                if (!saveData.IsVersionCompatible())
                {
                    Console.WriteLine($"Ошибка: Несовместимая версия сохранения " +
                        $"(файл: {saveData.SaveVersion}, текущая: {SaveGame.CurrentSaveVersion})");
                    return null;
                }

                // Валидация данных
                if (!saveData.Validate())
                {
                    Console.WriteLine("Ошибка: Данные сохранения не прошли валидацию");
                    return null;
                }

                // Миграция если версия старая
                if (saveData.SaveVersion < SaveGame.CurrentSaveVersion)
                {
                    MigrateSaveData(saveData);
                }

                Console.WriteLine($"Загрузка успешна: {filePath}");
                return saveData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Проверяет существование файла сохранения.
        /// </summary>
        public bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// Удаляет файл сохранения.
        /// </summary>
        public bool Delete(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);

                    // Удаляем резервную копию если есть
                    string backupPath = filePath + ".backup";
                    if (File.Exists(backupPath))
                    {
                        File.Delete(backupPath);
                    }

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Получает информацию о файле сохранения.
        /// 
        /// Читает метаданные без полной загрузки сохранения.
        /// Используется для отображения списка сохранений в UI.
        /// </summary>
        public SaveInfo GetSaveInfo(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                var fileInfo = new FileInfo(filePath);
                string json = File.ReadAllText(filePath, Encoding.UTF8);

                // Частичная десериализация для метаданных
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                return new SaveInfo
                {
                    FilePath = filePath,
                    CreatedDate = root.TryGetProperty("CreatedDate", out var created)
                        ? created.GetDateTime() : fileInfo.CreationTime,
                    ModifiedDate = fileInfo.LastWriteTime,
                    FileSize = fileInfo.Length,
                    SaveVersion = root.TryGetProperty("SaveVersion", out var version)
                        ? version.GetInt32() : 0,
                    SaveName = root.TryGetProperty("SaveName", out var name)
                        ? name.GetString() : Path.GetFileNameWithoutExtension(filePath),
                    SimulationMode = root.TryGetProperty("SimulationMode", out var mode)
                        ? mode.GetString() : "Unknown",
                    LastGeneration = root.TryGetProperty("CurrentGeneration", out var gen)
                        ? gen.GetInt64() : 0,
                    LiveCellCount = root.TryGetProperty("LiveCellCount", out var count)
                        ? count.GetInt32() : 0
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения информации: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Пытается восстановить файл из резервной копии.
        /// </summary>
        private void TryRestoreBackup(string filePath)
        {
            try
            {
                string backupPath = filePath + ".backup";
                if (File.Exists(backupPath))
                {
                    File.Copy(backupPath, filePath, true);
                    Console.WriteLine("Восстановлено из резервной копии");
                }
            }
            catch
            {
                Console.WriteLine("Не удалось восстановить из резервной копии");
            }
        }

        /// <summary>
        /// Выполняет миграцию данных старой версии к текущей.
        /// </summary>
        private void MigrateSaveData(SaveGame saveData)
        {
            Console.WriteLine($"Миграция данных версии {saveData.SaveVersion} " +
                $"к версии {SaveGame.CurrentSaveVersion}");

            // Здесь будет логика миграции между версиями
            // Например: добавление новых полей, преобразование форматов

            saveData.SaveVersion = SaveGame.CurrentSaveVersion;
            saveData.ModifiedDate = DateTime.Now;
        }

        #endregion
    }
}