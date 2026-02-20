using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameOfLife
{
    public class GameEngine
    {
        public int CurrentGeneration { get; private set; }
        public bool[,] CurrentWorldState { get; private set; }
        public CellGenome[,] CellGenomes { get; private set; }
        public Environment[,] WorldEnvironment { get; private set; }
        public int[,] CellEnergy { get; private set; }      // ✅ Энергия каждой клетки
        public int[,] CellCooldowns { get; private set; }   // ✅ Перезарядка атаки

        public int Rows { get; private set; }
        public int Cols { get; private set; }

        // === Настройки симуляции ===
        public float MutationRate { get; set; } = 0.1f;
        public bool EnvironmentEnabled { get; set; } = true;
        public bool GenomeEnabled { get; set; } = true;
        public bool MovementEnabled { get; set; } = true;
        public bool HuntingEnabled { get; set; } = true;
        public bool ParallelProcessing { get; set; } = true;

        public StatusEngine _statusEngine { get; set; }

        // ✅ Thread-local Random для каждого потока (без блокировок)
        private ThreadLocal<Random> _threadRandom = new ThreadLocal<Random>(() =>
            new Random(Guid.NewGuid().GetHashCode()));

        public GameEngine()
        {
            _statusEngine = StatusEngine.stop;
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

            // ✅ Инициализируем энергию и перезарядку
            CellEnergy = new int[this.Cols, this.Rows];
            CellCooldowns = new int[this.Cols, this.Rows];
        }

        public void FillRandom(int density)
        {
            // ✅ Параллельное заполнение для больших миров
            if (ParallelProcessing && Cols > 100)
            {
                Parallel.For(0, Cols, x =>
                {
                    var rand = _threadRandom.Value;
                    for (int y = 0; y < Rows; y++)
                    {
                        CurrentWorldState[x, y] = rand.Next(density) == 0;

                        if (CurrentWorldState[x, y] && GenomeEnabled)
                        {
                            CellGenomes[x, y] = new CellGenome();
                            CellEnergy[x, y] = CellGenomes[x, y].MaxEnergy * 10;
                            CellCooldowns[x, y] = 0;
                        }

                        if (EnvironmentEnabled)
                            WorldEnvironment[x, y] = new Environment();
                    }
                });
            }
            else
            {
                var rand = new Random();
                for (int x = 0; x < Cols; x++)
                {
                    for (int y = 0; y < Rows; y++)
                    {
                        CurrentWorldState[x, y] = rand.Next(density) == 0;

                        if (CurrentWorldState[x, y] && GenomeEnabled)
                        {
                            CellGenomes[x, y] = new CellGenome();
                            CellEnergy[x, y] = CellGenomes[x, y].MaxEnergy * 10;
                            CellCooldowns[x, y] = 0;
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

            // ✅ ПАРАЛЛЕЛЬНАЯ ОБРАБОТКА для больших миров
            if (ParallelProcessing && Cols > 50 && Rows > 50)
            {
                Parallel.For(0, Cols, new ParallelOptions
                {
                    MaxDegreeOfParallelism = System.Environment.ProcessorCount
                }, x =>
                {
                    var rand = _threadRandom.Value;
                    for (int y = 0; y < Rows; y++)
                    {
                        ProcessCell(x, y, newField, newGenomes, newEnergy, newCooldowns, rand);
                    }
                });
            }
            else
            {
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

            // ✅ Регенерация среды (тоже параллельно)
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

        // ✅ Обработка одной клетки (вынесена для параллелизма)
        private void ProcessCell(int x, int y, bool[,] newField, CellGenome[,] newGenomes,
            int[,] newEnergy, int[,] newCooldowns, Random rand)
        {
            var neighboursCount = CountNeighbours(x, y, 1);
            var hasLife = CurrentWorldState[x, y];
            var genome = GenomeEnabled ? CellGenomes[x, y] : null;
            var env = EnvironmentEnabled ? WorldEnvironment[x, y] : null;
            var energy = hasLife ? CellEnergy[x, y] : 0;
            var cooldown = hasLife ? CellCooldowns[x, y] : 0;

            // ✅ Расчёт модификатора выживания от среды (инлайн для скорости)
            float survivalModifier = 1.0f;
            if (env != null && GenomeEnabled)
            {
                survivalModifier += env.NutrientLevel * 0.08f;
                survivalModifier -= env.Toxicity * 0.15f;
                survivalModifier += env.OxygenLevel * 0.05f;
                survivalModifier -= env.RadiationLevel * 0.2f;

                if (genome != null)
                {
                    float tempDiff = Math.Abs(env.Temperature - genome.Metabolism);
                    survivalModifier -= tempDiff * 0.08f;

                    if (env.Moisture > 5 && genome.ReproductionRate > 5)
                        survivalModifier += 0.15f;

                    if (genome.Metabolism > 7 && env.Toxicity > 5)
                        survivalModifier -= 0.2f;

                    if (genome.VisionRange > 7 && env.NutrientLevel > 5)
                        survivalModifier += 0.1f;
                }

                survivalModifier = Math.Max(0.1f, Math.Min(2.0f, survivalModifier));
            }

            byte birthThreshold = genome?.BirthThreshold ?? 3;
            byte survivalMin = genome?.SurvivalMin ?? 2;
            byte survivalMax = genome?.SurvivalMax ?? 3;

            // ✅ Обработка охоты
            if (hasLife && HuntingEnabled && genome != null && genome.HuntStrength > 0 && cooldown <= 0)
            {
                TryHunt(x, y, genome, env, newField, newGenomes, newEnergy, rand);
                cooldown = genome.HuntCooldown;
            }

            // ✅ Обработка движения
            if (hasLife && MovementEnabled && genome != null && genome.MovementTendency > 5 && energy > genome.MovementCost)
            {
                TryMove(x, y, genome, env, newField, newGenomes, newEnergy, newCooldowns, ref energy, rand);
            }

            // ✅ Обработка защиты (снаряды)
            if (hasLife && genome != null && genome.ProjectileSpawnRate > 0 && rand.Next(10) < genome.ProjectileSpawnRate)
            {
                TrySpawnProjectile(x, y, genome, newField, newGenomes, newEnergy, rand);
            }

            // ✅ Рождение новой клетки
            if (!hasLife && neighboursCount == birthThreshold)
            {
                newField[x, y] = true;
                env?.OnCellBirth();

                if (GenomeEnabled)
                {
                    CellGenome parentGenome = GetRandomNeighbourGenome(x, y);
                    if (parentGenome != null && (float)rand.NextDouble() < (1.0f - MutationRate))
                        newGenomes[x, y] = new CellGenome(parentGenome);
                    else
                        newGenomes[x, y] = new CellGenome();

                    newEnergy[x, y] = newGenomes[x, y].MaxEnergy * 10;
                }
            }
            // ✅ Выживание существующей клетки
            else if (hasLife && neighboursCount >= survivalMin && neighboursCount <= survivalMax)
            {
                // ✅ ПРОВЕРКА: клетка умирает от голода если энергия <= 0
                if (energy <= 0)
                {
                    env?.OnCellDeath((byte)(genome?.MaxEnergy ?? 5));
                    newField[x, y] = false;
                }
                else if ((float)rand.NextDouble() < survivalModifier)
                {
                    newField[x, y] = true;
                    if (GenomeEnabled)
                        newGenomes[x, y] = genome;

                    // ✅ Потребление энергии за цикл жизни
                    int energyCost = genome?.Metabolism ?? 1;
                    newEnergy[x, y] = Math.Max(0, energy - energyCost);
                    newCooldowns[x, y] = Math.Max(0, cooldown - 1);

                    env?.OnCellSurvival(genome?.Metabolism ?? 1);
                }
                else
                {
                    env?.OnCellDeath((byte)(genome?.MaxEnergy ?? 5));
                    newField[x, y] = false;
                }
            }
            else if (hasLife)
            {
                // Смерть от перенаселения или голода
                env?.OnCellDeath((byte)(genome?.MaxEnergy ?? 5));
                newField[x, y] = false;
            }
        }

        // ✅ Охота на соседние клетки
        private void TryHunt(int x, int y, CellGenome hunterGenome, Environment env,
            bool[,] newField, CellGenome[,] newGenomes, int[,] newEnergy, Random rand)
        {
            int huntRange = hunterGenome.HuntRange;
            var state = CurrentWorldState;
            var genomes = CellGenomes;

            for (int dx = -huntRange; dx <= huntRange; dx++)
            {
                for (int dy = -huntRange; dy <= huntRange; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = (x + dx + Cols) % Cols;
                    int ny = (y + dy + Rows) % Rows;

                    if (state[nx, ny] && genomes[nx, ny] != null)
                    {
                        var preyGenome = genomes[nx, ny];

                        // Проверка: сила охоты vs сила защиты
                        if (hunterGenome.HuntStrength > preyGenome.DefenseStrength ||
                            rand.Next(10) < hunterGenome.HuntStrength)
                        {
                            // Успешная охота - убиваем жертву
                            newField[nx, ny] = false;
                            newEnergy[nx, ny] = 0;

                            // ✅ Хищник получает энергию от жертвы
                            int energyGain = preyGenome.MaxEnergy * 5;
                            CellEnergy[x, y] = Math.Min(hunterGenome.MaxEnergy * 10, CellEnergy[x, y] + energyGain);

                            env?.OnHunt(hunterGenome.HuntStrength);
                            return;
                        }
                    }
                }
            }
        }

        // ✅ Движение клетки в поисках лучших условий
        private void TryMove(int x, int y, CellGenome genome, Environment env,
            bool[,] newField, CellGenome[,] newGenomes, int[,] newEnergy, int[,] newCooldowns,
            ref int energy, Random rand)
        {
            int moveSpeed = genome.MovementSpeed;
            int moveCost = genome.MovementCost;

            int bestX = x, bestY = y;
            float bestScore = -1000;
            var state = CurrentWorldState;

            // Ищем лучшую позицию для движения
            for (int dx = -moveSpeed; dx <= moveSpeed; dx++)
            {
                for (int dy = -moveSpeed; dy <= moveSpeed; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = (x + dx + Cols) % Cols;
                    int ny = (y + dy + Rows) % Rows;

                    // Не двигаемся в занятую клетку
                    if (state[nx, ny]) continue;

                    // Оцениваем позицию
                    float score = 0;
                    var targetEnv = EnvironmentEnabled ? WorldEnvironment[nx, ny] : null;
                    if (targetEnv != null)
                    {
                        score += targetEnv.NutrientLevel * 2;
                        score -= targetEnv.Toxicity * 3;
                        score += targetEnv.OxygenLevel;
                    }

                    // Избегаем хищников (для не-хищников)
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

            // Двигаемся если нашли лучшую позицию
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
                // Остаёмся на месте
                newField[x, y] = true;
                newGenomes[x, y] = genome;
                newEnergy[x, y] = energy;
                newCooldowns[x, y] = CellCooldowns[x, y];
            }
        }

        // ✅ Создание снаряда для защиты
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
                newEnergy[nx, ny] = 5; // Снаряды живут недолго
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

        // ✅ Подсчёт хищников в радиусе зрения
        private int CountPredatorNeighbours(int x, int y, int range)
        {
            int count = 0;
            var state = CurrentWorldState;
            var genomes = CellGenomes;

            for (int dx = -range; dx <= range; dx++)
            {
                for (int dy = -range; dy <= range; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = (x + dx + Cols) % Cols;
                    int ny = (y + dy + Rows) % Rows;

                    if (state[nx, ny] && genomes[nx, ny] != null)
                    {
                        if (genomes[nx, ny].CellType == CellType.Predator)
                            count++;
                    }
                }
            }
            return count;
        }

        // ✅ Получение генома от случайного соседа для наследования
        private CellGenome GetRandomNeighbourGenome(int x, int y)
        {
            var rand = _threadRandom.Value;
            var state = CurrentWorldState;
            var genomes = CellGenomes;

            for (int i = 0; i < 8; i++)
            {
                int nx = (x + rand.Next(-1, 2) + Cols) % Cols;
                int ny = (y + rand.Next(-1, 2) + Rows) % Rows;

                if ((nx != x || ny != y) && state[nx, ny] && genomes[nx, ny] != null)
                    return genomes[nx, ny];
            }
            return null;
        }

        // ✅ Оптимизированный подсчёт соседей
        private int CountNeighbours(int x, int y, int r)
        {
            int count = 0;
            int cols = Cols;
            int rows = Rows;
            var state = CurrentWorldState;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (i == 1 && j == 1) continue;

                    int col = (x + i - 1 + cols) % cols;
                    int row = (y + j - 1 + rows) % rows;

                    if (state[col, row]) count++;
                }
            }
            return count;
        }

        public bool[,] GetCurrentGeneration() => CurrentWorldState;

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

        private bool ValidateCellPosition(int x, int y) =>
            x >= 0 && y >= 0 && x < Cols && y < Rows;

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
}