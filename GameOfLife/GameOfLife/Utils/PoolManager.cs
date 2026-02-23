using System;
using System.Collections.Generic;

namespace GameOfLife.Utils
{
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

        /// <summary>
        /// Возвращает сводную статистику всех пулов.
        /// </summary>
        /// <returns>Строка со статистикой.</returns>
        public string GetStatistics()
        {
            lock (_lockObject)
            {
                if (_pools.Count == 0)
                    return "Нет активных пулов";

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("=== Статистика пулов объектов ===");

                foreach (var kvp in _pools)
                {
                    // Используем рефлексию для вызова GetStatistics на каждом пуле
                    if (kvp.Value is IDisposable disposable)
                    {
                        var method = disposable.GetType().GetMethod("GetStatistics");
                        if (method != null)
                        {
                            var stats = method.Invoke(disposable, null);
                            sb.AppendLine($"  {kvp.Key}: {stats}");
                        }
                    }
                }

                return sb.ToString();
            }
        }
    }
}
