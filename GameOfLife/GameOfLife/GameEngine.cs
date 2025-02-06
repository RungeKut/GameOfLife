using System;
using System.Threading;

namespace GameOfLife
{
    public class GameEngine
    {
        /// <summary>
        /// Текущий номер 
        /// </summary>
        public int CurrentGeneration { get; private set; }
        /// <summary>
        /// Текущее состояние мира
        /// </summary>
        public bool[,] CurrentWorldState { get; private set; }
        /// <summary>
        /// Горизонтальный размер мира
        /// </summary>
        public int Rows { get; private set; }
        /// <summary>
        /// Вертикальный размер мира
        /// </summary>
        public int Cols { get; private set; }
        /// <summary>
        /// Матрица потоков
        /// </summary>
        private Thread[,] _workers { get; set; }
        /// <summary>
        /// Максимальное количество потоков
        /// </summary>
        private int _threadsCount { get; set; }
        /// <summary>
        /// Число строк в матрице потоков
        /// </summary>
        private int _threadsRows { get; set; }
        /// <summary>
        /// Число столбцов в матрице потоков
        /// </summary>
        private int _threadsCols { get; set; }
        /// <summary>
        /// Состояние движка
        /// </summary>
        public StatusEngine _statusEngine { get; set; }

        public GameEngine()
        {
            _statusEngine = StatusEngine.stop; // Начальное состояние
            _threadsCount = Environment.ProcessorCount; // Получаем количество логических процессоров

            #region Находим оптимальный размер матрицы потоков, чтобы вдальнейшем удобно соотнести расчет мира (и рендер изображений)
            _threadsRows = _threadsCount;
            _threadsCols = 1;
            while (true)
            {
                if ((_threadsRows > _threadsCols) & (_threadsRows % 2 == 0))
                {
                    _threadsRows = _threadsRows / 2;
                    _threadsCols = _threadsCols * 2;
                }
                else break;
            }
            if (_threadsRows > _threadsCols)
            {
                int _temp = _threadsCols;
                _threadsCols = _threadsRows;
                _threadsRows = _temp;
            }
            #endregion
        }

        public void ResizeWorld(int rows, int cols)
        {
            this.Rows = rows;
            this.Cols = cols;
            CurrentWorldState = new bool[cols, rows];
        }

        public void FillRandom(int density)
        {
            Random random = new Random();
            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    CurrentWorldState[x, y] = random.Next(density) == 0;
                }
            }
        }

        public bool[,] ApplyLifeRules(bool[,] currentWorldState, int cols, int rows)
        {
            bool[,] newWorldState = new bool[cols, rows]; // Создаем новое состояние мира
            return newWorldState;
        }

        public void NextGeneration()
        {
            var newField = new bool[Cols, Rows];

            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    var neighboursCount = CountNeighbours(x, y, 1);
                    var hasLife = CurrentWorldState[x, y];

                    if (!hasLife && neighboursCount == 3)
                    {
                        newField[x, y] = true;
                    }
                    else if (hasLife && (neighboursCount < 2 || neighboursCount > 3))
                    {
                        newField[x, y] = false;
                    }
                    else
                    {
                        newField[x, y] = CurrentWorldState[x, y];
                    }
                }
            }
            CurrentWorldState = newField;
            CurrentGeneration++;
        }

        public bool[,] GetCurrentGeneration()
        {
            var result = new bool[Cols, Rows];
            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    result[x, y] = CurrentWorldState[x, y];
                }
            }
            return result;
        }

        /// <summary>
        /// Подсчет количества соседей
        /// </summary>
        /// <param name="x">Горизонтальная координата</param>
        /// <param name="y">Вертикальная координата</param>
        /// <param name="r">Считать в радиусе</param>
        /// <returns></returns>
        private int CountNeighbours(int x, int y, int r)
        {
            // Итак допустим я нахожусь по координатам x,y
            int count = 0; // Переменная для подсчета количества моих соседей
            for (int i = 0; i < (2 * r + 1); i++) // Определяем ширину квадрата вокруг меня заданным радиусом
            {
                for (int j = 0; j < (2 * r + 1); j++) // Определяем высоту квадрата вокруг меня заданным радиусом
                {
                    var col = (x + (i - 1) + Cols) % Cols; // Определяем горизонтальную координату соседа учитывая границы мира (закольцовывание по горизонтали)
                    var row = (y + (j - 1) + Rows) % Rows; // Определяем вертикальную координату соседа учитывая границы мира (закольцовывание по вертикали)
                    var isSelfChecking = col == x && row == y; // Проверяем, не совпадают ли координаты соседа с моими.
                    var hasLife = CurrentWorldState[col, row]; // Смотрим есть сосед по текущим координатам или нет
                    if (hasLife && !isSelfChecking) count++; // Ну и увеличиваем счетчик соседей если он есть по текущим координатам и если он это не я
                }
            }
            return count;
        }

        private bool ValidateCellPosition(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Cols && y < Rows;
        }

        private void UpdateCell(int x, int y, bool state)
        {
            if (ValidateCellPosition(x, y))
                CurrentWorldState[x, y] = state;
        }

        public void AddCell(int x, int y)
        {
            UpdateCell(x, y, state: true);
        }

        public void RemoveCell(int x, int y)
        {
            UpdateCell(x, y, state: false);
        }
    }
}
