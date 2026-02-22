namespace GameOfLife.Parallel
{
    /// <summary>
    /// Статистика обработки процессора.
    /// 
    /// Используется для мониторинга производительности
    /// и выявления узких мест симуляции.
    /// </summary>
    public class ProcessingStatistics
    {
        /// <summary>
        /// Время обработки в миллисекундах.
        /// </summary>
        public long ProcessingTimeMs { get; set; }

        /// <summary>
        /// Количество использованных потоков.
        /// </summary>
        public int ThreadsUsed { get; set; }

        /// <summary>
        /// Количество обработанных сущностей.
        /// </summary>
        public int EntitiesProcessed { get; set; }

        /// <summary>
        /// Количество ячеек SpatialHash.
        /// </summary>
        public int SpatialHashCells { get; set; }

        /// <summary>
        /// Среднее количество сущностей на ячейку.
        /// </summary>
        public float AvgEntitiesPerCell { get; set; }

        /// <summary>
        /// Вычисляет примерную производительность (сущностей в секунду).
        /// </summary>
        public float EntitiesPerSecond
        {
            get
            {
                if (ProcessingTimeMs <= 0)
                    return 0;
                return EntitiesProcessed * 1000.0f / ProcessingTimeMs;
            }
        }

        /// <summary>
        /// Возвращает строковое представление статистики.
        /// </summary>
        public override string ToString()
        {
            return $"Time: {ProcessingTimeMs}ms, " +
                   $"Entities: {EntitiesProcessed}, " +
                   $"Threads: {ThreadsUsed}, " +
                   $"Perf: {EntitiesPerSecond:F0} ent/s";
        }
    }
}
