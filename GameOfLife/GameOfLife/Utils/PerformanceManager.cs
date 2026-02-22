using System.Collections.Generic;

namespace GameOfLife.Utils
{
    /// <summary>
    /// Менеджер счётчиков производительности.
    /// 
    /// Управляет коллекцией счётчиков для разных
    /// частей симуляции. Предоставляет сводную статистику.
    /// 
    /// Использование:
    /// var manager = new PerformanceManager();
    /// var counter = manager.GetCounter("Bot.Tick");
    /// counter.Start();
    /// ...
    /// counter.Stop();
    /// Console.WriteLine(manager.GetSummary());
    /// </summary>
    public class PerformanceManager
    {
        #region Приватные поля

        /// <summary>
        /// Коллекция всех счётчиков.
        /// </summary>
        private readonly Dictionary<string, PerformanceCounter> _counters;

        /// <summary>
        /// Блокировка для потокобезопасных операций.
        /// </summary>
        private readonly object _lockObject = new object();

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новый менеджер счётчиков.
        /// </summary>
        public PerformanceManager()
        {
            _counters = new Dictionary<string, PerformanceCounter>();
        }

        #endregion

        #region Управление счётчиками

        /// <summary>
        /// Получает или создаёт счётчик по имени.
        /// </summary>
        public PerformanceCounter GetCounter(string name)
        {
            lock (_lockObject)
            {
                if (!_counters.ContainsKey(name))
                {
                    _counters[name] = new PerformanceCounter(name);
                }
                return _counters[name];
            }
        }

        /// <summary>
        /// Сбрасывает все счётчики.
        /// </summary>
        public void ResetAll()
        {
            lock (_lockObject)
            {
                foreach (var counter in _counters.Values)
                {
                    counter.Reset();
                }
            }
        }

        /// <summary>
        /// Возвращает сводную статистику всех счётчиков.
        /// </summary>
        public string GetSummary()
        {
            lock (_lockObject)
            {
                var summary = new System.Text.StringBuilder();
                summary.AppendLine("=== Performance Summary ===");

                foreach (var counter in _counters.Values)
                {
                    summary.AppendLine(counter.ToString());
                }

                return summary.ToString();
            }
        }

        #endregion
    }
}
