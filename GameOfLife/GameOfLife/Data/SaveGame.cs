using System;
using System.Collections.Generic;
using GameOfLife.Core;
using GameOfLife.Entities;
using GameOfLife.Resources;
using GameOfLife.Environment;

namespace GameOfLife.Data
{
    /// <summary>
    /// Данные сохранения состояния симуляции.
    /// 
    /// Этот класс содержит все данные необходимые для полного
    /// восстановления состояния симуляции после загрузки.
    /// 
    /// Структура данных:
    /// - Метаданные (версия, дата, режим)
    /// - Конфигурация симуляции
    /// - Состояние мира (клетки)
    /// - Состояние ботов (для экосистемы)
    /// - Состояние ресурсов
    /// - Параметры окружающей среды
    /// - Статистика симуляции
    /// 
    /// Версионирование:
    /// Каждая версия сохранения имеет номер формата.
    /// При загрузке проверяется совместимость версий.
    /// При необходимости выполняется миграция данных.
    /// 
    /// Текущая версия формата: 1
    /// </summary>
    public class SaveGame
    {
        #region Константы

        /// <summary>
        /// Текущая версия формата сохранения.
        /// 
        /// Увеличивается при изменении структуры данных.
        /// При загрузке старых версий выполняется миграция.
        /// </summary>
        public const int CurrentSaveVersion = 1;

        /// <summary>
        /// Магическое число для идентификации формата.
        /// 
        /// Используется для проверки что файл является
        /// сохранением GameOfLife а не другим файлом.
        /// </summary>
        public const string MagicNumber = "GOLSAVE";

        #endregion

        #region Метаданные

        /// <summary>
        /// Версия формата сохранения.
        /// 
        /// Используется для проверки совместимости
        /// и выполнения миграции при необходимости.
        /// </summary>
        public int SaveVersion { get; set; } = CurrentSaveVersion;

        /// <summary>
        /// Дата создания сохранения.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Дата последнего изменения.
        /// </summary>
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Название сохранения.
        /// 
        /// Пользовательское имя для идентификации
        /// в списке сохранений.
        /// </summary>
        public string SaveName { get; set; } = "Untitled";

        /// <summary>
        /// Описание сохранения.
        /// 
        /// Дополнительная информация о сохранении
        /// (цель, особенности, заметки).
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Автор сохранения.
        /// </summary>
        public string Author { get; set; } = "";

        #endregion

        #region Параметры симуляции

        /// <summary>
        /// Режим симуляции (Conway, Ecosystem).
        /// 
        /// Определяет какие дополнительные данные
        /// должны быть загружены.
        /// </summary>
        public string SimulationMode { get; set; } = "Conway";

        /// <summary>
        /// Конфигурация симуляции.
        /// 
        /// Содержит все настраиваемые параметры:
        /// - Размеры мира
        /// - Задержка между тиками
        /// - Параметры параллелизма
        /// - Seed для случайных чисел
        /// </summary>
        public SimulationConfig Config { get; set; }

        /// <summary>
        /// Номер текущего поколения.
        /// 
        /// Позволяет продолжить симуляцию с того же места.
        /// </summary>
        public long CurrentGeneration { get; set; }

        #endregion

        #region Состояние мира

        /// <summary>
        /// Ширина мира в клетках.
        /// </summary>
        public int WorldWidth { get; set; }

        /// <summary>
        /// Высота мира в клетках.
        /// </summary>
        public int WorldHeight { get; set; }

        /// <summary>
        /// Состояние клеток мира.
        /// 
        /// Двумерный массив где true = живая клетка,
        /// false = мёртвая клетка.
        /// 
        /// Для режима Conway это основное состояние.
        /// Для режима Ecosystem это упрощённое представление.
        /// </summary>
        public bool[,] WorldState { get; set; }

        /// <summary>
        /// Количество живых клеток.
        /// 
        /// Кэшированное значение для быстрой проверки
        /// без подсчёта всего массива.
        /// </summary>
        public int LiveCellCount { get; set; }

        #endregion

        #region Состояние экосистемы (для режима Ecosystem)

        /// <summary>
        /// Список сохранённых ботов.
        /// 
        /// Содержит полное состояние каждого бота:
        /// - Позиция и состояние
        /// - Геном с параметрами
        /// - Инвентарь ресурсов
        /// - Статистика
        /// 
        /// Пусто для режима Conway.
        /// </summary>
        public List<SavedBot> Bots { get; set; } = new List<SavedBot>();

        /// <summary>
        /// Распределение ресурсов по клеткам.
        /// 
        /// Ключ: координата (x, y)
        /// Значение: список ресурсов в клетке
        /// 
        /// Пусто для режима Conway.
        /// </summary>
        public Dictionary<string, List<SavedResource>> WorldResources { get; set; }
            = new Dictionary<string, List<SavedResource>>();

        /// <summary>
        /// Параметры окружающей среды.
        /// 
        /// Содержит состояние погодной системы,
        /// радиоактивных зон, магнитных аномалий.
        /// 
        /// Пусто для режима Conway.
        /// </summary>
        public SavedEnvironment Environment { get; set; }

        #endregion

        #region Статистика

        /// <summary>
        /// Общая статистика симуляции.
        /// 
        /// Содержит агрегированные данные:
        /// - Максимальное количество поколений
        /// - Пиковое количество живых клеток
        /// - Общее количество созданных ботов
        /// - Время симуляции
        /// </summary>
        public SimulationStatistics Statistics { get; set; } = new SimulationStatistics();

        #endregion

        #region Конструкторы

        /// <summary>
        /// Создаёт новое пустое сохранение.
        /// </summary>
        public SaveGame()
        {
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }

        /// <summary>
        /// Создаёт сохранение из состояния движка.
        /// </summary>
        /// <param name="engine">Движок симуляции.</param>
        /// <param name="mode">Режим симуляции.</param>
        public SaveGame(SimulationEngine engine, string mode)
        {
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
            SimulationMode = mode;
            Config = engine.Config;
            CurrentGeneration = engine.CurrentGeneration;
            WorldWidth = engine.Width;
            WorldHeight = engine.Height;
            WorldState = engine.GetWorldArray();
            LiveCellCount = engine.LiveCellCount;
        }

        #endregion

        #region Методы валидации

        /// <summary>
        /// Проверяет валидность данных сохранения.
        /// 
        /// Выполняет следующие проверки:
        /// - Версия формата совместима
        /// - Размеры мира корректны
        /// - Массив состояния соответствует размерам
        /// - Обязательные поля заполнены
        /// </summary>
        /// <returns>True если данные валидны, иначе false.</returns>
        public bool Validate()
        {
            // Проверка версии
            if (SaveVersion > CurrentSaveVersion)
                return false;

            // Проверка размеров мира
            if (WorldWidth <= 0 || WorldHeight <= 0)
                return false;

            // Проверка массива состояния
            if (WorldState == null)
                return false;

            if (WorldState.GetLength(0) != WorldWidth ||
                WorldState.GetLength(1) != WorldHeight)
                return false;

            // Проверка конфигурации
            if (Config == null)
                return false;

            return true;
        }

        /// <summary>
        /// Проверяет совместимость версии сохранения.
        /// </summary>
        /// <returns>True если версия совместима, иначе false.</returns>
        public bool IsVersionCompatible()
        {
            return SaveVersion <= CurrentSaveVersion;
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Создаёт ключ для координаты в словаре ресурсов.
        /// </summary>
        public static string GetResourceKey(int x, int y)
        {
            return $"{x},{y}";
        }

        /// <summary>
        /// Парсит ключ координаты из строки.
        /// </summary>
        public static (int x, int y) ParseResourceKey(string key)
        {
            var parts = key.Split(',');
            return (int.Parse(parts[0]), int.Parse(parts[1]));
        }

        #endregion
    }
}