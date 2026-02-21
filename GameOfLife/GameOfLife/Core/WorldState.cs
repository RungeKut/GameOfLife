namespace GameOfLife.Core
{
    /// <summary>
    /// Снимок состояния мира на текущий момент.
    /// Используется для передачи данных от движка к рендерерам и UI.
    /// 
    /// Этот класс иммутабелен (только для чтения) после создания.
    /// </summary>
    public class WorldState
    {
        /// <summary>
        /// Номер текущего поколения.
        /// </summary>
        public long Generation { get; internal set; }

        /// <summary>
        /// Ширина мира в клетках.
        /// </summary>
        public int Width { get; internal set; }

        /// <summary>
        /// Высота мира в клетках.
        /// </summary>
        public int Height { get; internal set; }

        /// <summary>
        /// Количество живых клеток.
        /// </summary>
        public int LiveCellCount { get; internal set; }

        /// <summary>
        /// Текущий статус симуляции.
        /// </summary>
        public SimulationStatus Status { get; internal set; }

        /// <summary>
        /// Двумерный массив состояния клеток.
        /// true = живая, false = мёртвая.
        /// </summary>
        public bool[,] WorldArray { get; internal set; }

        /// <summary>
        /// Вычисляет процент живых клеток от общего количества.
        /// </summary>
        /// <returns>Процент от 0 до 100.</returns>
        public double GetLiveCellPercentage()
        {
            if (Width <= 0 || Height <= 0)
                return 0;

            int totalCells = Width * Height;
            return (double)LiveCellCount / totalCells * 100;
        }

        /// <summary>
        /// Проверяет, жива ли клетка по координатам.
        /// </summary>
        /// <param name="x">Координата X.</param>
        /// <param name="y">Координата Y.</param>
        /// <returns>True если клетка жива и координаты валидны.</returns>
        public bool IsCellAlive(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return false;

            return WorldArray[x, y];
        }

        /// <summary>
        /// Создаёт копию состояния мира.
        /// </summary>
        /// <returns>Новый объект WorldState с копией данных.</returns>
        public WorldState Clone()
        {
            return new WorldState
            {
                Generation = this.Generation,
                Width = this.Width,
                Height = this.Height,
                LiveCellCount = this.LiveCellCount,
                Status = this.Status,
                WorldArray = this.WorldArray // Массив копируется в SimulationEngine
            };
        }
    }
}