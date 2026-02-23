using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GameOfLife.Utils
{
    /// <summary>
    /// Монитор производительности симуляции.
    /// </summary>
    public class PerformanceMonitor : IDisposable
    {
        private readonly Dictionary<string, PerformanceCounter> _counters;
        private readonly Dictionary<string, long> _activeTimers;
        private readonly object _lockObject = new object();
        private readonly Stopwatch _sessionTimer;
        private long _tickCount;
        private bool _isEnabled;

        public bool IsEnabled { get => _isEnabled; set => _isEnabled = value; }
        public long SessionTimeMs => _sessionTimer.ElapsedMilliseconds;
        public long TickCount => _tickCount;

        public double AverageTickTimeMs
        {
            get => _tickCount > 0 ? (double)SessionTimeMs / _tickCount : 0;
        }

        public PerformanceMonitor(bool isEnabled = true)
        {
            _counters = new Dictionary<string, PerformanceCounter>();
            _activeTimers = new Dictionary<string, long>();
            _sessionTimer = Stopwatch.StartNew();
            _isEnabled = isEnabled;
            _tickCount = 0;
        }

        public void Start(string operationName)
        {
            if (!_isEnabled) return;
            lock (_lockObject)
            {
                if (!_activeTimers.ContainsKey(operationName))
                {
                    _activeTimers[operationName] = Stopwatch.GetTimestamp();
                }
            }
        }

        public double Stop(string operationName)
        {
            if (!_isEnabled) return 0;
            lock (_lockObject)
            {
                if (!_activeTimers.TryGetValue(operationName, out var startTime))
                    return 0;

                var endTime = Stopwatch.GetTimestamp();
                var elapsedMs = (endTime - startTime) * 1000.0 / Stopwatch.Frequency;
                _activeTimers.Remove(operationName);

                if (!_counters.ContainsKey(operationName))
                {
                    _counters[operationName] = new PerformanceCounter(operationName);
                }
                _counters[operationName].Record(elapsedMs);
                return elapsedMs;
            }
        }

        public void Record(string operationName, double elapsedMs)
        {
            if (!_isEnabled) return;
            lock (_lockObject)
            {
                if (!_counters.ContainsKey(operationName))
                {
                    _counters[operationName] = new PerformanceCounter(operationName);
                }
                _counters[operationName].Record(elapsedMs);
            }
        }

        public void IncrementTick()
        {
            if (!_isEnabled) return;
            lock (_lockObject) { _tickCount++; }
        }

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
                foreach (var kvp in _counters)
                {
                    var c = kvp.Value;
                    sb.AppendLine($"  {c.Name}:");
                    sb.AppendLine($"    Вызовов: {c.CallCount}");
                    sb.AppendLine($"    Среднее: {c.AverageMs:F2} мс");
                    sb.AppendLine($"    Мин: {c.MinMs:F2} мс");
                    sb.AppendLine($"    Макс: {c.MaxMs:F2} мс");
                    sb.AppendLine($"    Всего: {c.TotalMs:F2} мс");
                }
            }
            return sb.ToString();
        }

        public double GetFps()
        {
            return SessionTimeMs > 0 ? TickCount * 1000.0 / SessionTimeMs : 0;
        }

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

        public void Dispose() => _sessionTimer.Stop();
    }
}