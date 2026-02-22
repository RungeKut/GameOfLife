using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GameOfLife.Utils
{
    /// <summary>
    /// Универсальный пул объектов для переиспользования.
    /// 
    /// Проблема которую решает:
    /// Частое создание и уничтожение объектов (особенно в циклах)
    /// создаёт нагрузку на Garbage Collector что приводит к
    /// микро-фризам и снижению производительности.
    /// 
    /// Решение:
    /// Объекты не уничтожаются а возвращаются в пул для
    /// повторного использования. Это снижает количество
    /// аллокаций памяти и нагрузку на GC.
    /// 
    /// Типичные сценарии использования:
    /// - Временные объекты в циклах симуляции
    /// - Объекты действий/намерений
    /// - Временные списки и коллекции
    /// - Объекты событий
    /// 
    /// Использование:
    /// var pool = new ObjectPool<MyClass>(() => new MyClass());
    /// var obj = pool.Get();
    /// // ... использование ...
    /// pool.Return(obj);
    /// </summary>
    /// <typeparam name="T">
    /// Тип объектов в пуле.
    /// </typeparam>
    public class ObjectPool<T> : IDisposable where T : class
    {
        #region Приватные поля

        /// <summary>
        /// Контейнер для хранения объектов пула.
        /// ConcurrentBag обеспечивает потокобезопасность.
        /// </summary>
        private readonly ConcurrentBag<T> _objects;

        /// <summary>
        /// Фабрика для создания новых объектов.
        /// </summary>
        private readonly Func<T> _factory;

        /// <summary>
        /// Действие для сброса объекта перед возвратом в пул.
        /// </summary>
        private readonly Action<T> _resetAction;

        /// <summary>
        /// Максимальный размер пула.
        /// </summary>
        private readonly int _maxSize;

        /// <summary>
        /// Текущее количество объектов в пуле.
        /// </summary>
        private int _currentSize;

        /// <summary>
        /// Общее количество созданных объектов.
        /// </summary>
        private int _totalCreated;

        /// <summary>
        /// Количество повторных использований.
        /// </summary>
        private int _reuseCount;

        /// <summary>
        /// Блокировка для потокобезопасных операций.
        /// </summary>
        private readonly object _lockObject = new object();

        /// <summary>
        /// Флаг утилизации пула.
        /// </summary>
        private bool _disposed;

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Текущее количество объектов в пуле.
        /// </summary>
        public int CurrentSize => _currentSize;

        /// <summary>
        /// Максимальный размер пула.
        /// </summary>
        public int MaxSize => _maxSize;

        /// <summary>
        /// Общее количество созданных объектов.
        /// </summary>
        public int TotalCreated => _totalCreated;

        /// <summary>
        /// Количество повторных использований.
        /// </summary>
        public int ReuseCount => _reuseCount;

        /// <summary>
        /// Процент повторных использований.
        /// </summary>
        public double ReuseRate
        {
            get
            {
                int total = _totalCreated + _reuseCount;
                if (total <= 0)
                    return 0;
                return (double)_reuseCount / total * 100;
            }
        }

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новый пул объектов.
        /// </summary>
        /// <param name="factory">
        /// Фабрика для создания новых объектов.
        /// </param>
        /// <param name="maxSize">
        /// Максимальный размер пула.
        /// По умолчанию 1000 объектов.
        /// </param>
        /// <param name="resetAction">
        /// Действие для сброса объекта перед возвратом.
        /// Опционально.
        /// </param>
        public ObjectPool(
            Func<T> factory,
            int maxSize = 1000,
            Action<T> resetAction = null)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _maxSize = maxSize;
            _resetAction = resetAction;
            _objects = new ConcurrentBag<T>();
            _currentSize = 0;
            _totalCreated = 0;
            _reuseCount = 0;
            _disposed = false;
        }

        #endregion

        #region Основные методы

        /// <summary>
        /// Получает объект из пула.
        /// 
        /// Если пул содержит объекты, возвращается один из них.
        /// Если пул пуст, создаётся новый объект через фабрику.
        /// 
        /// Важно: после использования объект должен быть
        /// возвращён в пул через метод Return().
        /// </summary>
        /// <returns>
        /// Объект типа T из пула или новый объект.
        /// </returns>
        public T Get()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            if (_objects.TryTake(out var obj))
            {
                lock (_lockObject)
                {
                    _currentSize--;
                    _reuseCount++;
                }

                // Сбрасываем объект если указано действие сброса
                _resetAction?.Invoke(obj);

                return obj;
            }
            else
            {
                lock (_lockObject)
                {
                    _totalCreated++;
                }

                return _factory();
            }
        }

        /// <summary>
        /// Возвращает объект в пул.
        /// 
        /// Объект будет переиспользован при следующем вызове Get().
        /// Если пул полон, объект будет уничтожен.
        /// 
        /// Важно: не используйте объект после возврата в пул.
        /// </summary>
        /// <param name="obj">
        /// Объект для возврата в пул.
        /// </param>
        public void Return(T obj)
        {
            if (_disposed || obj == null)
                return;

            lock (_lockObject)
            {
                if (_currentSize >= _maxSize)
                {
                    // Пул полон, объект будет уничтожен GC
                    return;
                }

                _objects.Add(obj);
                _currentSize++;
            }
        }

        /// <summary>
        /// Очищает пул от всех объектов.
        /// 
        /// Все объекты в пуле будут уничтожены.
        /// Используйте при завершении работы или сбросе состояния.
        /// </summary>
        public void Clear()
        {
            if (_disposed)
                return;

            lock (_lockObject)
            {
                _objects.Clear();
                _currentSize = 0;
            }
        }

        /// <summary>
        /// Предварительно заполняет пул объектами.
        /// 
        /// Используйте для избежания создания объектов
        /// во время критических участков кода.
        /// </summary>
        /// <param name="count">
        /// Количество объектов для создания.
        /// </param>
        public void Preallocate(int count)
        {
            if (_disposed)
                return;

            for (int i = 0; i < count; i++)
            {
                var obj = _factory();
                Return(obj);
            }
        }

        #endregion

        #region Статистика

        /// <summary>
        /// Возвращает строку со статистикой пула.
        /// </summary>
        public string GetStatistics()
        {
            return $"ObjectPool<{typeof(T).Name}>: " +
                   $"Size={_currentSize}/{_maxSize}, " +
                   $"Created={_totalCreated}, " +
                   $"Reuse={_reuseCount} ({ReuseRate:F1}%)";
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Освобождает ресурсы пула.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                Clear();
                _disposed = true;
            }
        }

        #endregion
    }

    /// <summary>
    /// Менеджер пулов объектов.
    /// 
    /// Управляет множественными пулами для разных типов объектов.
    /// Предоставляет централизованный доступ и статистику.
    /// </summary>
    public class PoolManager : IDisposable
    {
        #region Приватные поля

        /// <summary>
        /// Словарь пулов по типам объектов.
        /// </summary>
        private readonly Dictionary<string, object> _pools;

        /// <summary>
        /// Блокировка для потокобезопасных операций.
        /// </summary>
        private readonly object _lockObject = new object();

        /// <summary>
        /// Флаг утилизации.
        /// </summary>
        private bool _disposed;

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новый менеджер пулов.
        /// </summary>
        public PoolManager()
        {
            _pools = new Dictionary<string, object>();
            _disposed = false;
        }

        #endregion

        #region Управление пулами

        /// <summary>
        /// Получает или создаёт пул для указанного типа.
        /// </summary>
        /// <typeparam name="T">
        /// Тип объектов в пуле.
        /// </typeparam>
        /// <param name="factory">
        /// Фабрика для создания объектов.
        /// </param>
        /// <param name="maxSize">
        /// Максимальный размер пула.
        /// </param>
        /// <param name="resetAction">
        /// Действие сброса объекта.
        /// </param>
        /// <returns>
        /// Пул объектов для указанного типа.
        /// </returns>
        public ObjectPool<T> GetPool<T>(
            Func<T> factory,
            int maxSize = 1000,
            Action<T> resetAction = null) where T : class
        {
            lock (_lockObject)
            {
                string key = typeof(T).FullName;

                if (!_pools.ContainsKey(key))
                {
                    _pools[key] = new ObjectPool<T>(factory, maxSize, resetAction);
                }

                return (ObjectPool<T>)_pools[key];
            }
        }

        /// <summary>
        /// Очищает все пулы.
        /// </summary>
        public void ClearAll()
        {
            lock (_lockObject)
            {
                foreach (var pool in _pools.Values)
                {
                    if (pool is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }

                _pools.Clear();
            }
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Освобождает ресурсы менеджера.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                ClearAll();
                _disposed = true;
            }
        }

        #endregion
    }
}