using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GameOfLife.Utils
{
    /// <summary>
    /// Монитор производительности симуляции.
    /// 
    /// Этот класс предоставляет инструменты для профилирования
    /// различных частей симуляции и выявления узких мест.
    /// 
    /// Возможности:
    /// - Измерение времени выполнения операций
    /// - Сбор статистики по кадрам/тикам
    /// - Выявление медленных операций
    /// - Экспорт отчётов о производительности
    /// 
    /// Использование:
    /// var monitor = new PerformanceMonitor();
    /// monitor.Start("Update");
    /// // ... код ...
    /// monitor.Stop("Update");
    /// Console.WriteLine(monitor.GetReport());
    /// </summary>
    public class PerformanceMonitor : IDisposable
    {
        #region Приватные поля

        /// <summary>
        /// Словарь счётчиков производительности.
        /// Ключ: имя операции, Значение: счётчик.
        /// </summary>
        private readonly Dictionary<string, PerformanceCounter> _counters;

        /// <summary>
        /// Словарь активных таймеров.
        /// Ключ: имя операции, Значение: время начала.
        /// </summary>
        private readonly Dictionary<string, long> _activeTimers;

        /// <summary>
        /// Блокировка для потокобезопасных операций.
        /// </summary>
        private readonly object _lockObject = new object();

        /// <summary>
        /// Общий таймер сессии.
        /// </summary>
        private readonly Stopwatch _sessionTimer;

        /// <summary>
        /// Количество тиков симуляции.
        /// </summary>
        private long _tickCount;

        /// <summary>
        /// Флаг активности монитора.
        /// </summary>
        private bool _isEnabled;

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Включён ли мониторинг производительности.
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        /// <summary>
        /// Общее время сессии в миллисекундах.
        /// </summary>
        public long SessionTimeMs => _sessionTimer.ElapsedMilliseconds;

        /// <summary>
        /// Количество тиков симуляции.
        /// </summary>
        public long TickCount => _tickCount;

        /// <summary>
        /// Средняя длительность тика в миллисекундах.
        /// </summary>
        public double AverageTickTimeMs
        {
            get
            {
                if (_tickCount <= 0)
                    return 0;
                return (double)SessionTimeMs / _tickCount;
            }
        }

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новый монитор производительности.
        /// </summary>
        /// <param name="isEnabled">
        /// Включить ли мониторинг по умолчанию.
        /// </param>
        public PerformanceMonitor(bool isEnabled = true)
        {
            _counters = new Dictionary<string, PerformanceCounter>();
            _activeTimers = new Dictionary<string, long>();
            _sessionTimer = Stopwatch.StartNew();
            _isEnabled = isEnabled;
            _tickCount = 0;
        }

        #endregion

        #region Измерение времени

        /// <summary>
        /// Запускает измерение времени операции.
        /// </summary>
        /// <param name="operationName">
        /// Имя измеряемой операции.
        /// </param>
        public void Start(string operationName)
        {
            if (!_isEnabled)
                return;

            lock (_lockObject)
            {
                if (!_activeTimers.ContainsKey(operationName))
                {
                    _activeTimers[operationName] = Stopwatch.GetTimestamp();
                }
            }
        }

        /// <summary>
        /// Останавливает измерение времени операции.
        /// </summary>
        /// <param name="operationName">
        /// Имя измеряемой операции.
        /// </param>
        /// <returns>
        /// Длительность операции в миллисекундах.
        /// </returns>
        public double Stop(string operationName)
        {
            if (!_isEnabled)
                return 0;

            lock (_lockObject)
            {
                if (!_activeTimers.TryGetValue(operationName, out var startTime))
                    return 0;

                var endTime = Stopwatch.GetTimestamp();
                var elapsedMs = (endTime - startTime) * 1000.0 / Stopwatch.Frequency;

                _activeTimers.Remove(operationName);

                // Обновляем счётчик
                if (!_counters.ContainsKey(operationName))
                {
                    _counters[operationName] = new PerformanceCounter(operationName);
                }

                _counters[operationName].Record(elapsedMs);

                return elapsedMs;
            }
        }

        /// <summary>
        /// Записывает время одной операции (упрощённый метод).
        /// </summary>
        /// <param name="operationName">
        /// Имя операции.
        /// </param>
        /// <param name="elapsedMs">
        /// Длительность в миллисекундах.
        /// </param>
        public void Record(string operationName, double elapsedMs)
        {
            if (!_isEnabled)
                return;

            lock (_lockObject)
            {
                if (!_counters.ContainsKey(operationName))
                {
                    _counters[operationName] = new PerformanceCounter(operationName);
                }

                _counters[operationName].Record(elapsedMs);
            }
        }

        /// <summary>
        /// Инкрементирует счётчик тиков.
        /// </summary>
        public void IncrementTick()
        {
            if (!_isEnabled)
                return;

            lock (_lockObject)
            {
                _tickCount++;
            }
        }

        #endregion

        #region Отчёты

        /// <summary>
        /// Генерирует текстовый отчёт о производительности.
        /// </summary>
        /// <returns>
        /// Строка с отчётом о производительности.
        /// </returns>
        public string GetReport()
        {
            var sb = new StringBuilder();

            lock (_lockObject)
            {
                sb.AppendLine("=== Отчёт о производительности ===");
                sb.AppendLine($"Время сессии: {SessionTimeMs} мс");
                sb.AppendLine($"Количество тиков: {TickCount}");
                sb.AppendLine($"Среднее время тика: {AverageTickTimeMs:F2} мс");
                sb.AppendLine($"FPS: {GetFps():F1}");
                sb.AppendLine();
                sb.AppendLine("Детали по операциям:");
                sb.AppendLine();

                foreach (var kvp in _counters)
                {
                    var counter = kvp.Value;
                    sb.AppendLine($"  {counter.Name}:");
                    sb.AppendLine($"    Вызовов: {counter.CallCount}");
                    sb.AppendLine($"    Среднее: {counter.AverageMs:F2} мс");
                    sb.AppendLine($"    Мин: {counter.MinMs:F2} мс");
                    sb.AppendLine($"    Макс: {counter.MaxMs:F2} мс");
                    sb.AppendLine($"    Всего: {counter.TotalMs:F2} мс");
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Вычисляет текущий FPS.
        /// </summary>
        /// <returns>
        /// Кадров в секунду.
        /// </returns>
        public double GetFps()
        {
            if (SessionTimeMs <= 0)
                return 0;
            return TickCount * 1000.0 / SessionTimeMs;
        }

        /// <summary>
        /// Находит самую медленную операцию.
        /// </summary>
        /// <returns>
        /// Имя самой медленной операции или null.
        /// </returns>
        public string GetSlowestOperation()
        {
            lock (_lockObject)
            {
                string slowest = null;
                double maxTime = 0;

                foreach (var kvp in _counters)
                {
                    if (kvp.Value.AverageMs > maxTime)
                    {
                        maxTime = kvp.Value.AverageMs;
                        slowest = kvp.Key;
                    }
                }

                return slowest;
            }
        }

        /// <summary>
        /// Сбрасывает всю статистику.
        /// </summary>
        public void Reset()
        {
            lock (_lockObject)
            {
                _sessionTimer.Restart();
                _tickCount = 0;
                _counters.Clear();
                _activeTimers.Clear();
            }
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Освобождает ресурсы монитора.
        /// </summary>
        public void Dispose()
        {
            _sessionTimer.Stop();
        }

        #endregion
    }

    /// <summary>
    /// Счётчик производительности для одной операции.
    /// </summary>
    public class PerformanceCounter
    {
        #region Приватные поля

        /// <summary>
        /// Имя операции.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Количество вызовов.
        /// </summary>
        private int _callCount;

        /// <summary>
        /// Общее время в миллисекундах.
        /// </summary>
        private double _totalMs;

        /// <summary>
        /// Минимальное время в миллисекундах.
        /// </summary>
        private double _minMs;

        /// <summary>
        /// Максимальное время в миллисекундах.
        /// </summary>
        private double _maxMs;

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Имя операции.
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Количество вызовов.
        /// </summary>
        public int CallCount => _callCount;

        /// <summary>
        /// Среднее время в миллисекундах.
        /// </summary>
        public double AverageMs
        {
            get
            {
                if (_callCount <= 0)
                    return 0;
                return _totalMs / _callCount;
            }
        }

        /// <summary>
        /// Минимальное время в миллисекундах.
        /// </summary>
        public double MinMs => _minMs;

        /// <summary>
        /// Максимальное время в миллисекундах.
        /// </summary>
        public double MaxMs => _maxMs;

        /// <summary>
        /// Общее время в миллисекундах.
        /// </summary>
        public double TotalMs => _totalMs;

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новый счётчик производительности.
        /// </summary>
        /// <param name="name">
        /// Имя операции.
        /// </param>
        public PerformanceCounter(string name)
        {
            _name = name;
            _callCount = 0;
            _totalMs = 0;
            _minMs = double.MaxValue;
            _maxMs = double.MinValue;
        }

        #endregion

        #region Методы

        /// <summary>
        /// Записывает время выполнения операции.
        /// </summary>
        /// <param name="elapsedMs">
        /// Длительность в миллисекундах.
        /// </param>
        public void Record(double elapsedMs)
        {
            _callCount++;
            _totalMs += elapsedMs;
            _minMs = Math.Min(_minMs, elapsedMs);
            _maxMs = Math.Max(_maxMs, elapsedMs);
        }

        /// <summary>
        /// Сбрасывает статистику счётчика.
        /// </summary>
        public void Reset()
        {
            _callCount = 0;
            _totalMs = 0;
            _minMs = double.MaxValue;
            _maxMs = double.MinValue;
        }

        /// <summary>
        /// Возвращает строковое представление счётчика.
        /// </summary>
        public override string ToString()
        {
            return $"{_name}: {_callCount} вызовов, среднее {AverageMs:F2} мс";
        }

        #endregion
    }
}