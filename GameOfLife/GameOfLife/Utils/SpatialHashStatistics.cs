namespace GameOfLife.Utils
{
    /// <summary>
    /// Статистика пространственной хеш-таблицы.
    /// 
    /// Используется для профилирования производительности
    /// и настройки оптимального размера ячейки.
    /// </summary>
    public class SpatialHashStatistics
    {
        /// <summary>
        /// Общее количество сущностей в хеше.
        /// </summary>
        public int TotalEntities { get; set; }

        /// <summary>
        /// Количество заполненных ячеек.
        /// </summary>
        public int OccupiedCells { get; set; }

        /// <summary>
        /// Среднее количество сущностей на ячейку.
        /// 
        /// Оптимальное значение: 5-20 сущностей на ячейку.
        /// Слишком высокое = ячейки слишком большие.
        /// Слишком низкое = ячейки слишком маленькие.
        /// </summary>
        public float AverageEntitiesPerCell { get; set; }

        /// <summary>
        /// Максимальное количество сущностей в одной ячейке.
        /// 
        /// Высокое значение указывает на кластеризацию сущностей
        /// что может снизить производительность поиска.
        /// </summary>
        public int MaxEntitiesInCell { get; set; }

        /// <summary>
        /// Минимальное количество сущностей в ячейке.
        /// </summary>
        public int MinEntitiesInCell { get; set; }

        /// <summary>
        /// Размер ячейки в клетках мира.
        /// </summary>
        public int CellSize { get; set; }

        /// <summary>
        /// Возвращает строковое представление статистики.
        /// </summary>
        public override string ToString()
        {
            return $"Entities: {TotalEntities}, Cells: {OccupiedCells}, " +
                   $"Avg: {AverageEntitiesPerCell:F1}, Max: {MaxEntitiesInCell}";
        }
    }
}
