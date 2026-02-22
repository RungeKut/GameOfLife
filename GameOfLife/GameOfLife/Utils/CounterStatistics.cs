namespace GameOfLife.Utils
{
    /// <summary>
    /// Статистика счётчика производительности.
    /// </summary>
    public class CounterStatistics
    {
        /// <summary>
        /// Название счётчика.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Количество вызовов.
        /// </summary>
        public int CallCount { get; set; }

        /// <summary>
        /// Общее время в миллисекундах.
        /// </summary>
        public long TotalElapsedMs { get; set; }

        /// <summary>
        /// Среднее время в миллисекундах.
        /// </summary>
        public double AverageElapsedMs { get; set; }

        /// <summary>
        /// Минимальное время в миллисекундах.
        /// </summary>
        public long MinElapsedMs { get; set; }

        /// <summary>
        /// Максимальное время в миллисекундах.
        /// </summary>
        public long MaxElapsedMs { get; set; }

        /// <summary>
        /// Время последнего измерения в миллисекундах.
        /// </summary>
        public long LastElapsedMs { get; set; }
    }
}
