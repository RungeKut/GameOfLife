using System;
using System.Diagnostics;

namespace GameOfLife.Utils
{
    /// <summary>
    /// Счётчик производительности для профилирования симуляции.
    /// 
    /// Проблема которую решает:
    /// При оптимизации важно знать какие части кода
    /// потребляют больше всего времени. Без измерений
    /// оптимизация становится угадыванием.
    /// 
    /// Решение:
    /// PerformanceCounter позволяет измерять время выполнения
    /// различных операций и накапливать статистику.
    /// 
    /// Архитектурные особенности:
    /// - Минимальные накладные расходы на измерения
    /// - Поддержка множественных счётчиков
    /// - Статистика (мин, макс, среднее, всего вызовов)
    /// - Потокобезопасность
    /// 
    /// Использование:
    /// var counter = new PerformanceCounter("Bot.Tick");
    /// counter.Start();
    /// bot.Tick(engine);
    /// counter.Stop();
    /// Console.WriteLine(counter.GetStatistics());
    /// </summary>
    public class PerformanceCounter
    {
        #region Приватные поля

        /// <summary>
        /// Название счётчика для идентификации.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Таймер для измерения времени выполнения.
        /// </summary>
        private readonly Stopwatch _stopwatch;

        /// <summary>
        /// Блокировка для потокобезопасных операций.
        /// </summary>
        private readonly object _lockObject = new object();

        /// <summary>
        /// Время последнего измерения в миллисекундах.
        /// </summary>
        private long _lastElapsedMs;

        /// <summary>
        /// Общее накопленное время всех измерений.
        /// </summary>
        private long _totalElapsedMs;

        /// <summary>
        /// Количество измерений.
        /// </summary>
        private int _callCount;

        /// <summary>
        /// Минимальное время измерения.
        /// </summary>
        private long _minElapsedMs;

        /// <summary>
        /// Максимальное время измерения.
        /// </summary>
        private long _maxElapsedMs;

        /// <summary>
        /// Время начала последнего измерения.
        /// </summary>
        private DateTime _startTime;

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Название счётчика.
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Среднее время выполнения в миллисекундах.
        /// </summary>
        public double AverageElapsedMs
        {
            get
            {
                lock (_lockObject)
                {
                    return _callCount > 0 ? (double)_totalElapsedMs / _callCount : 0;
                }
            }
        }

        /// <summary>
        /// Общее количество вызовов.
        /// </summary>
        public int CallCount
        {
            get
            {
                lock (_lockObject)
                {
                    return _callCount;
                }
            }
        }

        /// <summary>
        /// Общее время всех измерений в миллисекундах.
        /// </summary>
        public long TotalElapsedMs
        {
            get
            {
                lock (_lockObject)
                {
                    return _totalElapsedMs;
                }
            }
        }

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новый счётчик производительности.
        /// </summary>
        /// <param name="name">
        /// Название счётчика для идентификации.
        /// </param>
        public PerformanceCounter(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _stopwatch = new Stopwatch();
            _minElapsedMs = long.MaxValue;
            _maxElapsedMs = long.MinValue;
        }

        #endregion

        #region Измерение времени

        /// <summary>
        /// Запускает измерение времени.
        /// 
        /// Вызывается перед началом измеряемой операции.
        /// </summary>
        public void Start()
        {
            _startTime = DateTime.Now;
            _stopwatch.Restart();
        }

        /// <summary>
        /// Останавливает измерение и обновляет статистику.
        /// 
        /// Вызывается после завершения измеряемой операции.
        /// </summary>
        public void Stop()
        {
            lock (_lockObject)
            {
                _stopwatch.Stop();
                _lastElapsedMs = _stopwatch.ElapsedMilliseconds;
                _totalElapsedMs += _lastElapsedMs;
                _callCount++;
                _minElapsedMs = Math.Min(_minElapsedMs, _lastElapsedMs);
                _maxElapsedMs = Math.Max(_maxElapsedMs, _lastElapsedMs);
            }
        }

        /// <summary>
        /// Измеряет время выполнения действия.
        /// 
        /// Удобный метод для измерения одного вызова.
        /// Автоматически вызывает Start() и Stop().
        /// </summary>
        /// <param name="action">
        /// Действие для измерения.
        /// </param>
        /// <returns>
        /// Время выполнения в миллисекундах.
        /// </returns>
        public long Measure(Action action)
        {
            Start();
            action();
            Stop();
            return _lastElapsedMs;
        }

        #endregion

        #region Статистика

        /// <summary>
        /// Возвращает полную статистику счётчика.
        /// </summary>
        public CounterStatistics GetStatistics()
        {
            lock (_lockObject)
            {
                return new CounterStatistics
                {
                    Name = _name,
                    CallCount = _callCount,
                    TotalElapsedMs = _totalElapsedMs,
                    AverageElapsedMs = _callCount > 0 ? (double)_totalElapsedMs / _callCount : 0,
                    MinElapsedMs = _minElapsedMs == long.MaxValue ? 0 : _minElapsedMs,
                    MaxElapsedMs = _maxElapsedMs == long.MinValue ? 0 : _maxElapsedMs,
                    LastElapsedMs = _lastElapsedMs
                };
            }
        }

        /// <summary>
        /// Сбрасывает всю статистику счётчика.
        /// 
        /// Вызывается в начале нового тика симуляции
        /// для сбора статистики только текущего тика.
        /// </summary>
        public void Reset()
        {
            lock (_lockObject)
            {
                _lastElapsedMs = 0;
                _totalElapsedMs = 0;
                _callCount = 0;
                _minElapsedMs = long.MaxValue;
                _maxElapsedMs = long.MinValue;
            }
        }

        /// <summary>
        /// Возвращает строковое представление статистики.
        /// </summary>
        public override string ToString()
        {
            var stats = GetStatistics();
            return $"{_name}: {stats.CallCount} calls, " +
                   $"avg: {stats.AverageElapsedMs:F2}ms, " +
                   $"total: {stats.TotalElapsedMs}ms";
        }

        #endregion
    }
}