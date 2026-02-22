using GameOfLife.Core;
using GameOfLife.Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameOfLife.Entities
{
    /// <summary>
    /// Менеджер управления структурами в мире.
    /// 
    /// Отвечает за:
    /// - Создание и удаление структур
    /// - Поиск структур по координатам
    /// - Применение эффектов структур к ботам
    /// - Проверка занятости клеток структурами
    /// - Статистика и мониторинг структур
    /// 
    /// Архитектурные особенности:
    /// - Централизованное управление всеми структурами
    /// - Пространственная индексация для быстрого поиска
    /// - События для уведомления об изменениях
    /// - Поддержка множественных типов структур
    /// 
    /// Использование:
    /// var manager = new StructureManager();
    /// var structure = manager.BuildStructure(bot, x, y, StructureType.Shelter);
    /// var bonus = manager.GetDefenseBonus(bot);
    /// </summary>
    public class StructureManager
    {
        #region Приватные поля

        /// <summary>
        /// Все активные структуры.
        /// Ключ: ID структуры, Значение: структура.
        /// </summary>
        private readonly Dictionary<int, Structure> _structures;

        /// <summary>
        /// Структуры по координатам для быстрого поиска.
        /// Ключ: (x, y), Значение: список структур в клетке.
        /// </summary>
        private readonly Dictionary<(int, int), List<Structure>> _structuresByCell;

        /// <summary>
        /// Счётчик для генерации уникальных ID.
        /// </summary>
        private int _nextStructureId;

        /// <summary>
        /// Блокировка для потокобезопасных операций.
        /// </summary>
        private readonly object _lockObject = new object();

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Количество активных структур.
        /// </summary>
        public int ActiveStructureCount
        {
            get
            {
                lock (_lockObject)
                {
                    return _structures.Count(s => s.Value.IsActive);
                }
            }
        }

        /// <summary>
        /// Все активные структуры (только для чтения).
        /// </summary>
        public IReadOnlyCollection<Structure> AllStructures
        {
            get
            {
                lock (_lockObject)
                {
                    return _structures.Values.Where(s => s.IsActive).ToList().AsReadOnly();
                }
            }
        }

        #endregion

        #region События

        /// <summary>
        /// Событие создания структуры.
        /// </summary>
        public event Action<Structure> OnStructureBuilt;

        /// <summary>
        /// Событие разрушения структуры.
        /// </summary>
        public event Action<Structure> OnStructureDestroyed;

        /// <summary>
        /// Событие улучшения структуры.
        /// </summary>
        public event Action<Structure, int> OnStructureUpgraded;

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новый менеджер структур.
        /// </summary>
        public StructureManager()
        {
            _structures = new Dictionary<int, Structure>();
            _structuresByCell = new Dictionary<(int, int), List<Structure>>();
            _nextStructureId = 1;
        }

        #endregion

        #region Строительство

        /// <summary>
        /// Строит новую структуру в указанной клетке.
        /// </summary>
        /// <param name="owner">Владелец структуры (бот).</param>
        /// <param name="x">Координата X.</param>
        /// <param name="y">Координата Y.</param>
        /// <param name="type">Тип структуры.</param>
        /// <returns>Построенная структура или null если ошибка.</returns>
        public Structure BuildStructure(Bot owner, int x, int y, StructureType type)
        {
            if (owner == null)
                return null;

            if (!CanBuildAt(x, y))
                return null;

            // Проверка наличия ресурсов у бота
            if (!HasResourcesForBuild(owner, type))
                return null;

            lock (_lockObject)
            {
                // Создаём структуру
                var structure = new Structure(_nextStructureId++, x, y, type, owner);

                // Подписываемся на события структуры
                structure.OnDestroyed += OnStructureDestroyedInternal;
                structure.OnUpgraded += OnStructureUpgradedInternal;

                // Добавляем в коллекции
                _structures[structure.Id] = structure;
                AddToCellIndex(structure);

                // Списываем ресурсы у бота
                DeductBuildCost(owner, type);

                OnStructureBuilt?.Invoke(structure);

                return structure;
            }
        }

        /// <summary>
        /// Проверяет можно ли строить в указанной клетке.
        /// </summary>
        public bool CanBuildAt(int x, int y)
        {
            lock (_lockObject)
            {
                // Проверка наличия структуры в клетке
                if (_structuresByCell.ContainsKey((x, y)))
                {
                    var cellStructures = _structuresByCell[(x, y)];
                    if (cellStructures.Any(s => s.IsActive))
                        return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Проверяет наличие ресурсов для строительства.
        /// </summary>
        private bool HasResourcesForBuild(Bot bot, StructureType type)
        {
            if (bot.Inventory == null)
                return false;

            var cost = GetBuildCost(type);
            return bot.Inventory.HasEnoughMany(cost.ToDictionary());
        }

        /// <summary>
        /// Возвращает стоимость строительства для типа структуры.
        /// </summary>
        private ResourcePool GetBuildCost(StructureType type)
        {
            var cost = new ResourcePool(-1);

            switch (type)
            {
                case StructureType.Shelter:
                    cost.Add(ResourceType.Metal, 10);
                    break;
                case StructureType.Storage:
                    cost.Add(ResourceType.Metal, 15);
                    break;
                case StructureType.Base:
                    cost.Add(ResourceType.Metal, 50);
                    break;
                case StructureType.Trap:
                    cost.Add(ResourceType.Metal, 5);
                    break;
                case StructureType.Wall:
                    cost.Add(ResourceType.Metal, 8);
                    break;
                default:
                    cost.Add(ResourceType.Metal, 10);
                    break;
            }

            return cost;
        }

        /// <summary>
        /// Списывает стоимость строительства у бота.
        /// </summary>
        private void DeductBuildCost(Bot bot, StructureType type)
        {
            var cost = GetBuildCost(type);
            foreach (var resourceType in cost.ResourceTypes)
            {
                bot.Inventory.Remove(resourceType, cost.GetAmount(resourceType));
            }
        }

        #endregion

        #region Поиск структур

        /// <summary>
        /// Получает структуру в указанной клетке.
        /// </summary>
        public Structure GetStructureAt(int x, int y)
        {
            lock (_lockObject)
            {
                if (_structuresByCell.TryGetValue((x, y), out var structures))
                {
                    return structures.FirstOrDefault(s => s.IsActive);
                }
                return null;
            }
        }

        /// <summary>
        /// Получает все структуры в радиусе от точки.
        /// </summary>
        public List<Structure> GetStructuresInRange(int x, int y, int radius)
        {
            var result = new List<Structure>();

            lock (_lockObject)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        int checkX = x + dx;
                        int checkY = y + dy;

                        if (_structuresByCell.TryGetValue((checkX, checkY), out var structures))
                        {
                            result.AddRange(structures.Where(s => s.IsActive));
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Получает структуры владельца.
        /// </summary>
        public List<Structure> GetStructuresByOwner(Bot owner)
        {
            if (owner == null)
                return new List<Structure>();

            lock (_lockObject)
            {
                return _structures.Values
                    .Where(s => s.IsActive && s.IsOwnedBy(owner))
                    .ToList();
            }
        }

        /// <summary>
        /// Получает структуры по типу.
        /// </summary>
        public List<Structure> GetStructuresByType(StructureType type)
        {
            lock (_lockObject)
            {
                return _structures.Values
                    .Where(s => s.IsActive && s.Type == type)
                    .ToList();
            }
        }

        #endregion

        #region Эффекты структур

        /// <summary>
        /// Возвращает суммарный бонус к защите от всех структур.
        /// </summary>
        public float GetTotalDefenseBonus(Bot bot)
        {
            float totalBonus = 0;

            lock (_lockObject)
            {
                foreach (var structure in _structures.Values)
                {
                    if (structure.IsActive)
                    {
                        totalBonus += structure.GetDefenseBonus(bot);
                    }
                }
            }

            return Math.Min(0.9f, totalBonus); // Максимум 90% защиты
        }

        /// <summary>
        /// Возвращает суммарный бонус к регенерации энергии.
        /// </summary>
        public float GetTotalEnergyRegenBonus(Bot bot)
        {
            float totalBonus = 0;

            lock (_lockObject)
            {
                foreach (var structure in _structures.Values)
                {
                    if (structure.IsActive)
                    {
                        totalBonus += structure.GetEnergyRegenBonus(bot);
                    }
                }
            }

            return totalBonus;
        }

        #endregion

        #region Обновление

        /// <summary>
        /// Обновляет все структуры.
        /// Вызывается каждый тик симуляции.
        /// </summary>
        public void Update(SimulationEngine engine)
        {
            lock (_lockObject)
            {
                var toRemove = new List<int>();

                foreach (var kvp in _structures)
                {
                    if (kvp.Value.IsActive)
                    {
                        kvp.Value.Tick(engine);
                    }
                    else
                    {
                        toRemove.Add(kvp.Key);
                    }
                }

                // Удаляем разрушенные структуры
                foreach (var id in toRemove)
                {
                    RemoveStructureInternal(_structures[id]);
                }
            }
        }

        #endregion

        #region Внутренние обработчики

        private void OnStructureDestroyedInternal(Structure structure)
        {
            RemoveStructureInternal(structure);
            OnStructureDestroyed?.Invoke(structure);
        }

        private void OnStructureUpgradedInternal(Structure structure, int level)
        {
            OnStructureUpgraded?.Invoke(structure, level);
        }

        private void RemoveStructureInternal(Structure structure)
        {
            lock (_lockObject)
            {
                _structures.Remove(structure.Id);
                RemoveFromCellIndex(structure);
            }
        }

        private void AddToCellIndex(Structure structure)
        {
            var key = (structure.X, structure.Y);
            if (!_structuresByCell.ContainsKey(key))
            {
                _structuresByCell[key] = new List<Structure>();
            }
            _structuresByCell[key].Add(structure);
        }

        private void RemoveFromCellIndex(Structure structure)
        {
            var key = (structure.X, structure.Y);
            if (_structuresByCell.TryGetValue(key, out var structures))
            {
                structures.Remove(structure);
                if (structures.Count == 0)
                {
                    _structuresByCell.Remove(key);
                }
            }
        }

        #endregion

        #region Утилиты

        /// <summary>
        /// Очищает все структуры.
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                _structures.Clear();
                _structuresByCell.Clear();
            }
        }

        /// <summary>
        /// Возвращает статистику структур.
        /// </summary>
        public StructureStatistics GetStatistics()
        {
            lock (_lockObject)
            {
                var activeStructures = _structures.Values.Where(s => s.IsActive).ToList();

                return new StructureStatistics
                {
                    TotalStructures = activeStructures.Count,
                    StructuresByType = activeStructures.GroupBy(s => s.Type)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    AverageLevel = activeStructures.Count > 0
                        ? activeStructures.Average(s => s.Level)
                        : 0,
                    AverageHealth = activeStructures.Count > 0
                        ? activeStructures.Average(s => s.CurrentHealth)
                        : 0
                };
            }
        }

        #endregion
    }

    /// <summary>
    /// Статистика структур.
    /// </summary>
    public class StructureStatistics
    {
        /// <summary>
        /// Общее количество структур.
        /// </summary>
        public int TotalStructures { get; set; }

        /// <summary>
        /// Количество структур по типам.
        /// </summary>
        public Dictionary<StructureType, int> StructuresByType { get; set; }

        /// <summary>
        /// Средний уровень структур.
        /// </summary>
        public double AverageLevel { get; set; }

        /// <summary>
        /// Среднее здоровье структур.
        /// </summary>
        public double AverageHealth { get; set; }
    }
}