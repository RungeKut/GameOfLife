using System;
using System.Drawing;
using System.Threading.Tasks;

namespace GameOfLife
{
    public class GameEngine
    {
        public int CurrentGeneration { get; private set; }
        public bool[,] CurrentWorldState { get; private set; }
        public CellGenome[,] CellGenomes { get; private set; }
        public Environment[,] WorldEnvironment { get; private set; }
        public int[,] CellEnergy { get; private set; }
        public int[,] CellCooldowns { get; private set; }

        public int Rows { get; private set; }
        public int Cols { get; private set; }

        public float MutationRate { get; set; } = 0.1f;
        public bool EnvironmentEnabled { get; set; } = true;
        public bool GenomeEnabled { get; set; } = true;
        public bool MovementEnabled { get; set; } = true;
        public bool HuntingEnabled { get; set; } = true;
        public bool ParallelProcessing { get; set; } = true; // ✅ Включить параллелизм

        public StatusEngine _statusEngine { get; set; }

        private int _maxDegreeOfParallelism { get; set; }

        public GameEngine()
        {
            _statusEngine = StatusEngine.stop;
            // ✅ Используем количество логических ядер процессора
            _maxDegreeOfParallelism = System.Environment.ProcessorCount;
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

            CellEnergy = new int[this.Cols, this.Rows];
            CellCooldowns = new int[this.Cols, this.Rows];
        }

        public void FillRandom(int density)
        {
            Random random = new Random();

            // ✅ Параллельное заполнение мира
            if (ParallelProcessing && Cols > 100)
            {
                Parallel.For(0, Cols, x =>
                {
                    var localRandom = new Random(x + DateTime.Now.Millisecond);
                    for (int y = 0; y < Rows; y++)
                    {
                        CurrentWorldState[x, y] = localRandom.Next(density) == 0;

                        if (CurrentWorldState[x, y] && GenomeEnabled)
                        {
                            CellGenomes[x, y] = new CellGenome();
                            CellEnergy[x, y] = CellGenomes[x, y].MaxEnergy * 10;
                        }

                        if (EnvironmentEnabled)
                            WorldEnvironment[x, y] = new Environment();
                    }
                });
            }
            else
            {
                for (int x = 0; x < Cols; x++)
                {
                    for (int y = 0; y < Rows; y++)
                    {
                        CurrentWorldState[x, y] = random.Next(density) == 0;

                        if (CurrentWorldState[x, y] && GenomeEnabled)
                        {
                            CellGenomes[x, y] = new CellGenome();
                            CellEnergy[x, y] = CellGenomes[x, y].MaxEnergy * 10;
                        }

                        if (EnvironmentEnabled)
                            WorldEnvironment[x, y] = new Environment();
                    }
                }
            }
        }

        public void NextGeneration()
        {
            var newField = new bool[Cols, Rows];
            var newGenomes = GenomeEnabled ? new CellGenome[Cols, Rows] : null;
            var newEnergy = new int[Cols, Rows];
            var newCooldowns = new int[Cols, Rows];

            // ✅ Создаём локальный Random для каждого потока
            var localRandoms = new Random[System.Environment.ProcessorCount];
            for (int i = 0; i < localRandoms.Length; i++)
                localRandoms[i] = new Random(Guid.NewGuid().GetHashCode());

            if (ParallelProcessing && Cols > 50 && Rows > 50)
            {
                // ✅ ПАРАЛЛЕЛЬНАЯ ОБРАБОТКА
                Parallel.For(0, Cols, new ParallelOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism }, x =>
                {
                    var rand = localRandoms[x % localRandoms.Length];

                    for (int y = 0; y < Rows; y++)
                    {
                        ProcessCell(x, y, newField, newGenomes, newEnergy, newCooldowns, rand);
                    }
                });
            }
            else
            {
                // ✅ Последовательная обработка для маленьких миров
                var rand = new Random();
                for (int x = 0; x < Cols; x++)
                {
                    for (int y = 0; y < Rows; y++)
                    {
                        ProcessCell(x, y, newField, newGenomes, newEnergy, newCooldowns, rand);
                    }
                }
            }

            // ✅ Применяем изменения
            CurrentWorldState = newField;
            if (GenomeEnabled)
                CellGenomes = newGenomes;
            CellEnergy = newEnergy;
            CellCooldowns = newCooldowns;

            // ✅ Регенерация среды (можно тоже распараллелить)
            if (EnvironmentEnabled)
            {
                if (ParallelProcessing && Cols > 50)
                {
                    Parallel.For(0, Cols, x =>
                    {
                        for (int y = 0; y < Rows; y++)
                            WorldEnvironment[x, y]?.Regenerate();
                    });
                }
                else
                {
                    for (int x = 0; x < Cols; x++)
                        for (int y = 0; y < Rows; y++)
                            WorldEnvironment[x, y]?.Regenerate();
                }
            }

            CurrentGeneration++;
        }

        // ✅ Вынесена логика обработки клетки в отдельный метод
        private void ProcessCell(int x, int y, bool[,] newField, CellGenome[,] newGenomes,
            int[,] newEnergy, int[,] newCooldowns, Random rand)
        {
            var neighboursCount = CountNeighbours(x, y, 1);
            var hasLife = CurrentWorldState[x, y];
            var genome = GenomeEnabled ? CellGenomes[x, y] : null;
            var env = EnvironmentEnabled ? WorldEnvironment[x, y] : null;
            var energy = hasLife ? CellEnergy[x, y] : 0;
            var cooldown = hasLife ? CellCooldowns[x, y] : 0;

            float survivalModifier = env?.GetSurvivalModifier(genome) ?? 1.0f;

            byte birthThreshold = genome?.BirthThreshold ?? 3;
            byte survivalMin = genome?.SurvivalMin ?? 2;
            byte survivalMax = genome?.SurvivalMax ?? 3;

            // Обработка охоты
            if (hasLife && HuntingEnabled && genome != null && genome.HuntStrength > 0 && cooldown <= 0)
            {
                TryHunt(x, y, genome, env, newField, newGenomes, newEnergy, rand);
                cooldown = genome.HuntCooldown;
            }

            // Обработка движения
            if (hasLife && MovementEnabled && genome != null && genome.MovementTendency > 5 && energy > genome.MovementCost)
            {
                TryMove(x, y, genome, env, newField, newGenomes, newEnergy, newCooldowns, ref energy, rand);
            }

            // Обработка защиты (снаряды)
            if (hasLife && genome != null && genome.ProjectileSpawnRate > 0 && rand.Next(10) < genome.ProjectileSpawnRate)
            {
                TrySpawnProjectile(x, y, genome, newField, newGenomes, newEnergy, rand);
            }

            // Рождение
            if (!hasLife && neighboursCount == birthThreshold)
            {
                newField[x, y] = true;
                env?.OnCellBirth();

                if (GenomeEnabled)
                {
                    CellGenome parentGenome = GetRandomNeighbourGenome(x, y);
                    if (parentGenome != null && rand.NextFloat() < (1.0f - MutationRate))
                        newGenomes[x, y] = new CellGenome(parentGenome);
                    else
                        newGenomes[x, y] = new CellGenome();

                    newEnergy[x, y] = newGenomes[x, y].MaxEnergy * 10;
                }
            }
            // Выживание
            else if (hasLife && neighboursCount >= survivalMin && neighboursCount <= survivalMax)
            {
                if (rand.NextFloat() < survivalModifier && energy > 0)
                {
                    newField[x, y] = true;
                    if (GenomeEnabled)
                        newGenomes[x, y] = genome;

                    int energyCost = genome?.Metabolism ?? 1;
                    newEnergy[x, y] = Math.Max(0, energy - energyCost);
                    newCooldowns[x, y] = Math.Max(0, cooldown - 1);

                    env?.OnCellSurvival(genome?.Metabolism ?? 1);
                }
                else
                {
                    env?.OnCellDeath((byte)(genome?.MaxEnergy ?? 5));
                }
            }
            else if (hasLife)
            {
                env?.OnCellDeath((byte)(genome?.MaxEnergy ?? 5));
            }
        }

        // ✅ Охота (требует осторожности при параллелизме)
        private void TryHunt(int x, int y, CellGenome hunterGenome, Environment env,
            bool[,] newField, CellGenome[,] newGenomes, int[,] newEnergy, Random rand)
        {
            int huntRange = hunterGenome.HuntRange;

            for (int dx = -huntRange; dx <= huntRange; dx++)
            {
                for (int dy = -huntRange; dy <= huntRange; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = (x + dx + Cols) % Cols;
                    int ny = (y + dy + Rows) % Rows;

                    // ✅ Проверяем текущее состояние (не новое)
                    if (CurrentWorldState[nx, ny] && CellGenomes[nx, ny] != null)
                    {
                        var preyGenome = CellGenomes[nx, ny];

                        if (hunterGenome.HuntStrength > preyGenome.DefenseStrength ||
                            rand.Next(10) < hunterGenome.HuntStrength)
                        {
                            // ✅ Убиваем жертву в НОВОМ массиве (безопасно для параллелизма)
                            newField[nx, ny] = false;
                            newEnergy[nx, ny] = 0;

                            int energyGain = preyGenome.MaxEnergy * 5;
                            CellEnergy[x, y] = Math.Min(hunterGenome.MaxEnergy * 10, CellEnergy[x, y] + energyGain);

                            env?.OnHunt(hunterGenome.HuntStrength);
                            return;
                        }
                    }
                }
            }
        }

        // ✅ Движение
        private void TryMove(int x, int y, CellGenome genome, Environment env,
            bool[,] newField, CellGenome[,] newGenomes, int[,] newEnergy, int[,] newCooldowns,
            ref int energy, Random rand)
        {
            int moveSpeed = genome.MovementSpeed;
            int moveCost = genome.MovementCost;

            int bestX = x, bestY = y;
            float bestScore = -1000;

            for (int dx = -moveSpeed; dx <= moveSpeed; dx++)
            {
                for (int dy = -moveSpeed; dy <= moveSpeed; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = (x + dx + Cols) % Cols;
                    int ny = (y + dy + Rows) % Rows;

                    if (CurrentWorldState[nx, ny]) continue;

                    float score = 0;
                    var targetEnv = EnvironmentEnabled ? WorldEnvironment[nx, ny] : null;
                    if (targetEnv != null)
                    {
                        score += targetEnv.NutrientLevel * 2;
                        score -= targetEnv.Toxicity * 3;
                        score += targetEnv.OxygenLevel;
                    }

                    if (genome.CellType != CellType.Predator)
                    {
                        int predatorCount = CountPredatorNeighbours(nx, ny, genome.VisionRange);
                        score -= predatorCount * 10;
                    }

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestX = nx;
                        bestY = ny;
                    }
                }
            }

            if (bestX != x || bestY != y)
            {
                newField[bestX, bestY] = true;
                newField[x, y] = false;
                newGenomes[bestX, bestY] = genome;
                newEnergy[bestX, bestY] = energy - moveCost;
                newCooldowns[bestX, bestY] = CellCooldowns[x, y];

                env?.OnCellMove((byte)moveCost);
            }
            else
            {
                newField[x, y] = true;
                newGenomes[x, y] = genome;
                newEnergy[x, y] = energy;
                newCooldowns[x, y] = CellCooldowns[x, y];
            }
        }

        // ✅ Создание снаряда
        private void TrySpawnProjectile(int x, int y, CellGenome parentGenome,
            bool[,] newField, CellGenome[,] newGenomes, int[,] newEnergy, Random rand)
        {
            int dx = rand.Next(-1, 2);
            int dy = rand.Next(-1, 2);

            if (dx == 0 && dy == 0) return;

            int nx = (x + dx + Cols) % Cols;
            int ny = (y + dy + Rows) % Rows;

            if (!CurrentWorldState[nx, ny])
            {
                newField[nx, ny] = true;
                newGenomes[nx, ny] = CreateProjectileGenome(parentGenome);
                newEnergy[nx, ny] = 5;
            }
        }

        private CellGenome CreateProjectileGenome(CellGenome parent)
        {
            var projectile = new CellGenome
            {
                MovementTendency = 10,
                MovementSpeed = 3,
                HuntStrength = parent.ProjectileDamage,
                DefenseStrength = 0,
                MaxEnergy = 2,
                Metabolism = 10
            };
            projectile.SetCellType(CellType.Projectile);
            return projectile;
        }

        private int CountPredatorNeighbours(int x, int y, int range)
        {
            int count = 0;
            for (int dx = -range; dx <= range; dx++)
            {
                for (int dy = -range; dy <= range; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = (x + dx + Cols) % Cols;
                    int ny = (y + dy + Rows) % Rows;

                    if (CurrentWorldState[nx, ny] && CellGenomes[nx, ny] != null)
                    {
                        if (CellGenomes[nx, ny].CellType == CellType.Predator)
                            count++;
                    }
                }
            }
            return count;
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

        public int GetCellEnergy(int x, int y)
        {
            if (CellEnergy == null) return 0;
            if (x < 0 || x >= Cols || y < 0 || y >= Rows) return 0;
            return CellEnergy[x, y];
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
                {
                    CellGenomes[x, y] = new CellGenome();
                    CellEnergy[x, y] = CellGenomes[x, y].MaxEnergy * 10;
                }
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