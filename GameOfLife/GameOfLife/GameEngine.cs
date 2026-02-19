using System;
using System.Drawing;
using System.Threading;

namespace GameOfLife
{
    public class GameEngine
    {
        public int CurrentGeneration { get; private set; }
        public bool[,] CurrentWorldState { get; private set; }
        public CellGenome[,] CellGenomes { get; private set; }
        public Environment[,] WorldEnvironment { get; private set; }
        public int Rows { get; private set; }
        public int Cols { get; private set; }

        // === Настройки эволюции ===
        public float MutationRate { get; set; } = 0.1f;
        public bool EnvironmentEnabled { get; set; } = true;
        public bool GenomeEnabled { get; set; } = true;

        public StatusEngine _statusEngine { get; set; }

        private Thread[,] _workers { get; set; }
        private int _threadsCount { get; set; }
        private int _threadsRows { get; set; }
        private int _threadsCols { get; set; }

        public GameEngine()
        {
            _statusEngine = StatusEngine.stop;
            _threadsCount = System.Environment.ProcessorCount;

            #region Оптимизация матрицы потоков
            _threadsRows = _threadsCount;
            _threadsCols = 1;
            while (true)
            {
                if ((_threadsRows > _threadsCols) && (_threadsRows % 2 == 0))
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

        public void ResizeWorld(Point2D worldSize)
        {
            this.Rows = (int)worldSize.Y;
            this.Cols = (int)worldSize.X;
            CurrentWorldState = new bool[this.Cols, this.Rows];

            if (GenomeEnabled)
                CellGenomes = new CellGenome[this.Cols, this.Rows];
            if (EnvironmentEnabled)
                WorldEnvironment = new Environment[this.Cols, this.Rows];
        }

        public void FillRandom(int density)
        {
            Random random = new Random();
            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    CurrentWorldState[x, y] = random.Next(density) == 0;

                    if (CurrentWorldState[x, y] && GenomeEnabled)
                        CellGenomes[x, y] = new CellGenome();

                    if (EnvironmentEnabled)
                        WorldEnvironment[x, y] = new Environment();
                }
            }
        }

        public void NextGeneration()
        {
            var newField = new bool[Cols, Rows];
            var newGenomes = GenomeEnabled ? new CellGenome[Cols, Rows] : null;

            Random rand = new Random();

            for (int x = 0; x < Cols; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    var neighboursCount = CountNeighbours(x, y, 1);
                    var hasLife = CurrentWorldState[x, y];
                    var genome = GenomeEnabled ? CellGenomes[x, y] : null;
                    var env = EnvironmentEnabled ? WorldEnvironment[x, y] : null;

                    float survivalModifier = env?.GetSurvivalModifier(genome) ?? 1.0f;

                    byte birthThreshold = genome?.BirthThreshold ?? 3;
                    byte survivalMin = genome?.SurvivalMin ?? 2;
                    byte survivalMax = genome?.SurvivalMax ?? 3;

                    if (!hasLife && neighboursCount == birthThreshold)
                    {
                        newField[x, y] = true;

                        if (GenomeEnabled)
                        {
                            CellGenome parentGenome = GetRandomNeighbourGenome(x, y);
                            if (parentGenome != null && rand.NextFloat() < (1.0f - MutationRate))
                                newGenomes[x, y] = new CellGenome(parentGenome);
                            else
                                newGenomes[x, y] = new CellGenome();
                        }
                    }
                    else if (hasLife && neighboursCount >= survivalMin && neighboursCount <= survivalMax)
                    {
                        if (rand.NextFloat() < survivalModifier)
                        {
                            newField[x, y] = true;
                            if (GenomeEnabled)
                                newGenomes[x, y] = genome;
                        }

                        if (EnvironmentEnabled && genome != null)
                            env?.ConsumeEnergy(genome.Metabolism);
                    }
                    else
                    {
                        newField[x, y] = false;
                    }
                }
            }

            CurrentWorldState = newField;
            if (GenomeEnabled)
                CellGenomes = newGenomes;

            if (EnvironmentEnabled)
            {
                for (int x = 0; x < Cols; x++)
                    for (int y = 0; y < Rows; y++)
                        WorldEnvironment[x, y]?.Regenerate();
            }

            CurrentGeneration++;
        }

        private CellGenome GetRandomNeighbourGenome(int x, int y)
        {
            Random rand = new Random();
            int attempts = 0;
            while (attempts < 8)
            {
                int nx = (x + rand.Next(-1, 2) + Cols) % Cols;
                int ny = (y + rand.Next(-1, 2) + Rows) % Rows;
                if (nx != x || ny != y)
                {
                    if (CurrentWorldState[nx, ny] && CellGenomes[nx, ny] != null)
                        return CellGenomes[nx, ny];
                }
                attempts++;
            }
            return null;
        }

        public bool[,] GetCurrentGeneration()
        {
            return CurrentWorldState;
        }

        public CellGenome GetCellGenome(int x, int y)
        {
            if (!GenomeEnabled || CellGenomes == null) return null;
            if (x < 0 || x >= Cols || y < 0 || y >= Rows) return null;
            return CellGenomes[x, y];
        }

        public Environment GetCellEnvironment(int x, int y)
        {
            if (!EnvironmentEnabled || WorldEnvironment == null) return null;
            if (x < 0 || x >= Cols || y < 0 || y >= Rows) return null;
            return WorldEnvironment[x, y];
        }

        private int CountNeighbours(int x, int y, int r)
        {
            int count = 0;
            for (int i = 0; i < (2 * r + 1); i++)
            {
                for (int j = 0; j < (2 * r + 1); j++)
                {
                    var col = (x + (i - 1) + Cols) % Cols;
                    var row = (y + (j - 1) + Rows) % Rows;
                    var isSelfChecking = col == x && row == y;
                    var hasLife = CurrentWorldState[col, row];
                    if (hasLife && !isSelfChecking) count++;
                }
            }
            return count;
        }

        private bool ValidateCellPosition(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Cols && y < Rows;
        }

        public void AddCell(int x, int y)
        {
            if (ValidateCellPosition(x, y))
            {
                CurrentWorldState[x, y] = true;
                if (GenomeEnabled && CellGenomes[x, y] == null)
                    CellGenomes[x, y] = new CellGenome();
            }
        }

        public void RemoveCell(int x, int y)
        {
            if (ValidateCellPosition(x, y))
                CurrentWorldState[x, y] = false;
        }
    }

    public static class RandomExtensions
    {
        private static Random _rand = new Random();
        public static float NextFloat(this Random rand)
        {
            return (float)_rand.NextDouble();
        }
    }
}