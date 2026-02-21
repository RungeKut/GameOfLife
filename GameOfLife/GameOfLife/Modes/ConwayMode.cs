namespace GameOfLife.Modes
{
    using GameOfLife.Core;

    /// <summary>
    /// Режим симуляции "Игра в жизнь" Конвея.
    /// Классические правила:
    /// - Живая клетка с 2-3 соседями выживает
    /// - Мёртвая клетка с 3 соседями оживает
    /// - В остальных случаях клетка умирает или остаётся мёртвой
    /// </summary>
    public class ConwayMode : ISimulationMode
    {
        /// <summary>
        /// Название режима.
        /// </summary>
        public string Name => "Conway's Game of Life";

        /// <summary>
        /// Описание режима.
        /// </summary>
        public string Description => "Классическая игра в жизнь Джона Конвея";

        /// <summary>
        /// Конфигурация режима.
        /// </summary>
        private SimulationConfig _config;

        /// <summary>
        /// Инициализирует режим Конвея.
        /// </summary>
        public void Initialize(SimulationConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Выполняет один шаг симуляции по правилам Конвея.
        /// </summary>
        public bool[,] Step(bool[,] worldState, long generation)
        {
            int width = worldState.GetLength(0);
            int height = worldState.GetLength(1);
            bool[,] newState = new bool[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int neighbours = CountNeighbours(worldState, x, y, width, height);
                    newState[x, y] = ApplyRules(worldState[x, y], neighbours, generation);
                }
            }

            return newState;
        }

        /// <summary>
        /// Применяет классические правила Конвея к клетке.
        /// </summary>
        public bool ApplyRules(bool isAlive, int neighbourCount, long generation)
        {
            if (isAlive)
            {
                // Живая клетка выживает при 2 или 3 соседях
                return neighbourCount == 2 || neighbourCount == 3;
            }
            else
            {
                // Мёртвая клетка оживает при ровно 3 соседях
                return neighbourCount == 3;
            }
        }

        /// <summary>
        /// Подсчитывает количество живых соседей.
        /// Учитывает зацикливание границ.
        /// </summary>
        private int CountNeighbours(bool[,] world, int x, int y, int width, int height)
        {
            int count = 0;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    int nx = (x + dx + width) % width;
                    int ny = (y + dy + height) % height;

                    if (world[nx, ny])
                        count++;
                }
            }

            return count;
        }
    }
}