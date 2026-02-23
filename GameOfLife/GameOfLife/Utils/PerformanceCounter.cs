using System;
using System.Diagnostics;

namespace GameOfLife.Utils
{
    /// <summary>
    /// Счётчик производительности для измерения времени операций.
    /// </summary>
    public class PerformanceCounter
    {
        private readonly string _name;
        private int _callCount;
        private double _totalMs;
        private double _minMs;
        private double _maxMs;

        public string Name => _name;
        public int CallCount => _callCount;
        public double AverageMs => _callCount > 0 ? _totalMs / _callCount : 0;
        public double MinMs => _minMs;
        public double MaxMs => _maxMs;
        public double TotalMs => _totalMs;

        public PerformanceCounter(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _callCount = 0;
            _totalMs = 0;
            _minMs = double.MaxValue;
            _maxMs = double.MinValue;
        }

        public void Record(double elapsedMs)
        {
            _callCount++;
            _totalMs += elapsedMs;
            _minMs = Math.Min(_minMs, elapsedMs);
            _maxMs = Math.Max(_maxMs, elapsedMs);
        }

        public void Reset()
        {
            _callCount = 0;
            _totalMs = 0;
            _minMs = double.MaxValue;
            _maxMs = double.MinValue;
        }

        public override string ToString()
        {
            return $"{_name}: {_callCount} вызовов, среднее {AverageMs:F2} мс";
        }
    }
}