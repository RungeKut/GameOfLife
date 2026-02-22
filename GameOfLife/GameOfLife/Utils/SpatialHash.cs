using System;
using System.Collections.Generic;
using GameOfLife.Entities;

namespace GameOfLife.Utils
{
    /// <summary>
    /// Пространственная хеш-таблица для быстрого поиска сущностей поблизости.
    /// 
    /// Проблема которую решает:
    /// При поиске соседей или ближайших ботов наивный подход требует
    /// проверки всех сущностей в мире (O(n) для каждой проверки).
    /// При 1000 ботах это 1,000,000 проверок за тик.
    /// 
    /// Решение:
    /// Пространство делится на ячейки (buckets). Каждая сущность
    /// хранится только в своей ячейке. При поиске проверяются
    /// только сущности в соседних ячейках.
    /// 
    /// Сложность операций:
    /// - Добавление сущности: O(1)
    /// - Удаление сущности: O(1)
    /// - Поиск соседей: O(k) где k = сущности в радиусе (не во всём мире)
    /// 
    /// Архитектурное разделение:
    /// - SpatialHash<T>: универсальная хеш-таблица для любых IEntity
    /// - BotSpatialHash: специализированная версия для ботов
    /// - ResourceSpatialHash: специализированная версия для ресурсов
    /// 
    /// Использование:
    /// var hash = new SpatialHash<Bot>(cellSize: 10);
    /// hash.Add(bot);
    /// var nearby = hash.GetNearby(bot.X, bot.Y, radius: 5);
    /// </summary>
    public class SpatialHash<T> where T : IEntity
    {
        #region Приватные поля

        /// <summary>
        /// Словарь ячеек пространства.
        /// 
        /// Ключ: координаты ячейки (cellX, cellY)
        /// Значение: список сущностей в этой ячейке
        /// 
        /// Используется Dictionary для быстрого доступа O(1).
        /// Пустые ячейки не хранятся в словаре (экономия памяти).
        /// </summary>
        private readonly Dictionary<(int, int), List<T>> _cells;

        /// <summary>
        /// Размер ячейки хеша в клетках мира.
        /// 
        /// Оптимальный размер зависит от типичного радиуса поиска:
        /// - Слишком маленький: много ячеек, сущности в многих ячейках
        /// - Слишком большой: мало ячеек, много сущностей в каждой
        /// - Рекомендация: 2-3x от типичного радиуса поиска
        /// 
        /// По умолчанию 10 клеток — баланс между памятью и скоростью.
        /// </summary>
        private readonly int _cellSize;

        /// <summary>
        /// Блокировка для потокобезопасных операций.
        /// 
        /// Защита от одновременной модификации из нескольких потоков
        /// при параллельной обработке ботов.
        /// </summary>
        private readonly object _lockObject = new object();

        /// <summary>
        /// Кэш последней запрошенной ячейки для оптимизации.
        /// 
        /// Если последовательные запросы приходят из одной области
        /// (бот перемещается постепенно), кэш ускоряет доступ.
        /// </summary>
        private (int, int) _lastCellKey;
        private List<T> _lastCellCache;

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Общее количество сущностей в хеш-таблице.
        /// 
        /// Вычисляется динамически как сумма всех сущностей
        /// во всех ячейках. Может быть медленной операцией
        /// при большом количестве ячеек.
        /// </summary>
        public int TotalEntities
        {
            get
            {
                lock (_lockObject)
                {
                    int count = 0;
                    foreach (var cell in _cells.Values)
                    {
                        count += cell.Count;
                    }
                    return count;
                }
            }
        }

        /// <summary>
        /// Количество заполненных ячеек пространства.
        /// 
        /// Показывает насколько плотно заполнен мир сущностями.
        /// Низкое значение = разреженное распределение.
        /// Высокое значение = плотное распределение.
        /// </summary>
        public int OccupiedCellCount
        {
            get
            {
                lock (_lockObject)
                {
                    return _cells.Count;
                }
            }
        }

        /// <summary>
        /// Размер ячейки хеша в клетках мира.
        /// 
        /// Только для чтения. Устанавливается в конструкторе
        /// и не может быть изменён после создания.
        /// </summary>
        public int CellSize => _cellSize;

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новую пространственную хеш-таблицу.
        /// 
        /// Инициализирует внутренние структуры данных
        /// и устанавливает размер ячейки.
        /// </summary>
        /// <param name="cellSize">
        /// Размер ячейки в клетках мира.
        /// Рекомендуемые значения: 5-20 в зависимости от радиуса поиска.
        /// </param>
        public SpatialHash(int cellSize = 10)
        {
            if (cellSize <= 0)
                throw new ArgumentException("Размер ячейки должен быть положительным", nameof(cellSize));

            _cells = new Dictionary<(int, int), List<T>>();
            _cellSize = cellSize;
            _lastCellKey = (0, 0);
            _lastCellCache = null;
        }

        #endregion

        #region Добавление и удаление сущностей

        /// <summary>
        /// Добавляет сущность в хеш-таблицу.
        /// 
        /// Вычисляет ячейку на основе координат сущности
        /// и добавляет сущность в список этой ячейки.
        /// 
        /// Если сущность уже существует в хеше, она не добавляется
        /// повторно (предотвращение дубликатов).
        /// 
        /// Сложность: O(1) в среднем случае.
        /// </summary>
        /// <param name="entity">
        /// Сущность для добавления.
        /// Должна быть не null и IsActive = true.
        /// </param>
        public void Add(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (!entity.IsActive)
                return;

            lock (_lockObject)
            {
                var key = GetCellKey(entity.X, entity.Y);

                // Очищаем кэш при модификации
                _lastCellCache = null;

                if (!_cells.ContainsKey(key))
                {
                    _cells[key] = new List<T>();
                }

                // Проверяем что сущность ещё не добавлена
                if (!_cells[key].Contains(entity))
                {
                    _cells[key].Add(entity);
                }
            }
        }

        /// <summary>
        /// Удаляет сущность из хеш-таблицы.
        /// 
        /// Находит ячейку сущности и удаляет её из списка.
        /// Если ячейка становится пустой, она удаляется из словаря
        /// для экономии памяти.
        /// 
        /// Сложность: O(n) где n = сущности в ячейке.
        /// </summary>
        /// <param name="entity">
        /// Сущность для удаления.
        /// </param>
        /// <returns>
        /// True если сущность была найдена и удалена.
        /// False если сущность не найдена в хеше.
        /// </returns>
        public bool Remove(T entity)
        {
            if (entity == null)
                return false;

            lock (_lockObject)
            {
                var key = GetCellKey(entity.X, entity.Y);

                // Очищаем кэш при модификации
                _lastCellCache = null;

                if (_cells.TryGetValue(key, out var cell))
                {
                    bool removed = cell.Remove(entity);

                    // Удаляем пустую ячейку для экономии памяти
                    if (cell.Count == 0)
                    {
                        _cells.Remove(key);
                    }

                    return removed;
                }

                return false;
            }
        }

        /// <summary>
        /// Обновляет позицию сущности в хеш-таблице.
        /// 
        /// Вызывается когда сущность перемещается.
        /// Удаляет сущность из старой ячейки и добавляет в новую.
        /// 
        /// Оптимизация: если сущность осталась в той же ячейке,
        /// операция пропускается для производительности.
        /// 
        /// Сложность: O(1) если та же ячейка, O(n) если новая.
        /// </summary>
        /// <param name="entity">
        /// Сущность которая переместилась.
        /// </param>
        public void UpdatePosition(T entity)
        {
            if (entity == null || !entity.IsActive)
                return;

            lock (_lockObject)
            {
                var oldKey = GetCellKey(entity.X, entity.Y);

                // Проверяем кэш последней ячейки
                if (_lastCellCache != null && _lastCellKey == oldKey)
                {
                    // Сущность в той же ячейке — ничего не делаем
                    if (_lastCellCache.Contains(entity))
                        return;
                }

                // Удаляем из всех ячеек (на случай если координаты изменились)
                RemoveWithoutLock(entity);

                // Добавляем в новую ячейку
                AddWithoutLock(entity);

                // Очищаем кэш
                _lastCellCache = null;
            }
        }

        /// <summary>
        /// Внутренний метод удаления без блокировки.
        /// Должен вызываться только внутри lock (_lockObject).
        /// </summary>
        private void RemoveWithoutLock(T entity)
        {
            var key = GetCellKey(entity.X, entity.Y);

            if (_cells.TryGetValue(key, out var cell))
            {
                cell.Remove(entity);

                if (cell.Count == 0)
                {
                    _cells.Remove(key);
                }
            }
        }

        /// <summary>
        /// Внутренний метод добавления без блокировки.
        /// Должен вызываться только внутри lock (_lockObject).
        /// </summary>
        private void AddWithoutLock(T entity)
        {
            var key = GetCellKey(entity.X, entity.Y);

            if (!_cells.ContainsKey(key))
            {
                _cells[key] = new List<T>();
            }

            if (!_cells[key].Contains(entity))
            {
                _cells[key].Add(entity);
            }
        }

        #endregion

        #region Поиск сущностей

        /// <summary>
        /// Получает все сущности в указанной клетке мира.
        /// 
        /// Быстрый доступ к сущностям в конкретной клетке.
        /// Используется для проверки занятости клетки перед
        /// перемещением или строительством.
        /// 
        /// Сложность: O(1) + O(k) где k = сущности в клетке.
        /// </summary>
        /// <param name="x">
        /// Координата X клетки мира.
        /// </param>
        /// <param name="y">
        /// Координата Y клетки мира.
        /// </param>
        /// <returns>
        /// Список сущностей в клетке (может быть пустым).
        /// </returns>
        public List<T> GetCell(int x, int y)
        {
            lock (_lockObject)
            {
                var key = GetCellKey(x, y);

                // Проверка кэша
                if (_lastCellCache != null && _lastCellKey == key)
                {
                    return _lastCellCache;
                }

                if (_cells.TryGetValue(key, out var cell))
                {
                    _lastCellKey = key;
                    _lastCellCache = cell;
                    return cell;
                }

                // Возвращаем пустой список если ячейка не найдена
                return new List<T>();
            }
        }

        /// <summary>
        /// Получает все сущности в радиусе от указанной точки.
        /// 
        /// Основной метод для поиска соседей, целей для атаки,
        /// партнёров для размножения и т.д.
        /// 
        /// Алгоритм:
        /// 1. Вычисляем диапазон ячеек которые покрывают радиус
        /// 2. Собираем все сущности из этих ячеек
        /// 3. Фильтруем по фактическому расстоянию (круг, не квадрат)
        /// 
        /// Сложность: O(k) где k = сущности в радиусе.
        /// </summary>
        /// <param name="x">
        /// Координата X центра поиска.
        /// </param>
        /// <param name="y">
        /// Координата Y центра поиска.
        /// </param>
        /// <param name="radius">
        /// Радиус поиска в клетках мира.
        /// </param>
        /// <returns>
        /// Список сущностей в радиусе (может быть пустым).
        /// </returns>
        public List<T> GetNearby(int x, int y, int radius = 5)
        {
            var result = new List<T>();

            lock (_lockObject)
            {
                // Вычисляем диапазон ячеек
                int minX = GetCellX(x - radius);
                int maxX = GetCellX(x + radius);
                int minY = GetCellY(y - radius);
                int maxY = GetCellY(y + radius);

                // Собираем сущности из всех ячеек в диапазоне
                for (int cellX = minX; cellX <= maxX; cellX++)
                {
                    for (int cellY = minY; cellY <= maxY; cellY++)
                    {
                        var key = (cellX, cellY);

                        if (_cells.TryGetValue(key, out var cell))
                        {
                            foreach (var entity in cell)
                            {
                                // Проверяем фактическое расстояние (круг)
                                int dx = entity.X - x;
                                int dy = entity.Y - y;
                                int distance = dx * dx + dy * dy; // Квадрат расстояния

                                if (distance <= radius * radius)
                                {
                                    result.Add(entity);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Находит ближайшую сущность к указанной точке.
        /// 
        /// Оптимизированная версия GetNearby которая возвращает
        /// только одну ближайшую сущность вместо списка.
        /// 
        /// Используется для:
        /// - Поиска ближайшей цели для атаки
        /// - Поиска ближайшего партнёра для размножения
        /// - Поиска ближайшего ресурса для сбора
        /// 
        /// Сложность: O(k) где k = сущности в радиусе.
        /// </summary>
        /// <param name="x">
        /// Координата X центра поиска.
        /// </param>
        /// <param name="y">
        /// Координата Y центра поиска.
        /// </param>
        /// <param name="maxDistance">
        /// Максимальная дистанция поиска.
        /// </param>
        /// <param name="predicate">
        /// Дополнительный фильтр сущностей (опционально).
        /// </param>
        /// <returns>
        /// Ближайшая сущность или null если не найдена.
        /// </returns>
        public T FindNearest(
            int x,
            int y,
            int maxDistance = 10,
            Func<T, bool> predicate = null)
        {
            T nearest = default(T);
            int minDistanceSq = maxDistance * maxDistance;

            lock (_lockObject)
            {
                int minX = GetCellX(x - maxDistance);
                int maxX = GetCellX(x + maxDistance);
                int minY = GetCellY(y - maxDistance);
                int maxY = GetCellY(y + maxDistance);

                for (int cellX = minX; cellX <= maxX; cellX++)
                {
                    for (int cellY = minY; cellY <= maxY; cellY++)
                    {
                        var key = (cellX, cellY);

                        if (_cells.TryGetValue(key, out var cell))
                        {
                            foreach (var entity in cell)
                            {
                                // Применяем фильтр если указан
                                if (predicate != null && !predicate(entity))
                                    continue;

                                // Проверяем расстояние
                                int dx = entity.X - x;
                                int dy = entity.Y - y;
                                int distanceSq = dx * dx + dy * dy;

                                if (distanceSq < minDistanceSq)
                                {
                                    minDistanceSq = distanceSq;
                                    nearest = entity;
                                }
                            }
                        }
                    }
                }
            }

            return nearest;
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Вычисляет координату X ячейки для данной координаты мира.
        /// </summary>
        private int GetCellX(int worldX)
        {
            // Обработка отрицательных координат
            if (worldX < 0)
                return (worldX - _cellSize + 1) / _cellSize;
            return worldX / _cellSize;
        }

        /// <summary>
        /// Вычисляет координату Y ячейки для данной координаты мира.
        /// </summary>
        private int GetCellY(int worldY)
        {
            // Обработка отрицательных координат
            if (worldY < 0)
                return (worldY - _cellSize + 1) / _cellSize;
            return worldY / _cellSize;
        }

        /// <summary>
        /// Вычисляет ключ ячейки для данных координат мира.
        /// </summary>
        private (int, int) GetCellKey(int x, int y)
        {
            return (GetCellX(x), GetCellY(y));
        }

        /// <summary>
        /// Очищает все сущности из хеш-таблицы.
        /// 
        /// Вызывается при сбросе мира или завершении симуляции.
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                _cells.Clear();
                _lastCellCache = null;
            }
        }

        /// <summary>
        /// Возвращает все сущности в хеш-таблице.
        /// 
        /// Используется для итерации по всем сущностям
        /// при глобальных операциях (статистика, сохранение).
        /// 
        /// Сложность: O(n) где n = все сущности.
        /// </summary>
        public List<T> GetAllEntities()
        {
            var result = new List<T>();

            lock (_lockObject)
            {
                foreach (var cell in _cells.Values)
                {
                    result.AddRange(cell);
                }
            }

            return result;
        }

        #endregion

        #region Статистика и отладка

        /// <summary>
        /// Возвращает статистику заполнения хеш-таблицы.
        /// 
        /// Используется для профилирования и настройки
        /// оптимального размера ячейки.
        /// </summary>
        public SpatialHashStatistics GetStatistics()
        {
            lock (_lockObject)
            {
                int totalEntities = 0;
                int maxCellSize = 0;
                int minCellSize = int.MaxValue;

                foreach (var cell in _cells.Values)
                {
                    int count = cell.Count;
                    totalEntities += count;
                    maxCellSize = Math.Max(maxCellSize, count);
                    minCellSize = Math.Min(minCellSize, count);
                }

                if (_cells.Count == 0)
                    minCellSize = 0;

                return new SpatialHashStatistics
                {
                    TotalEntities = totalEntities,
                    OccupiedCells = _cells.Count,
                    AverageEntitiesPerCell = _cells.Count > 0 ? (float)totalEntities / _cells.Count : 0,
                    MaxEntitiesInCell = maxCellSize,
                    MinEntitiesInCell = minCellSize,
                    CellSize = _cellSize
                };
            }
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Освобождает ресурсы хеш-таблицы.
        /// </summary>
        public void Dispose()
        {
            Clear();
        }

        #endregion
    }
}