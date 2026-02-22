using System;
using System.Collections.Generic;
using System.Linq;
using GameOfLife.Core;
using GameOfLife.Entities;
using GameOfLife.Resources;
using GameOfLife.Genome;

// Псевдоним для разрешения конфликта имён: namespace GameOfLife.Genome vs class Genome
using GenomeClass = GameOfLife.Genome.Genome;

namespace GameOfLife.Modes
{
    /// <summary>
    /// Режим симуляции "Экосистема" с автономными ботами.
    /// 
    /// В этом режиме мир населён агентами (ботами), которые:
    /// - Имеют геном, определяющий их поведение и характеристики
    /// - Потребляют и производят ресурсы
    /// - Взаимодействуют друг с другом и со средой
    /// - Эволюционируют через мутации и естественный отбор
    /// 
    /// Отличия от режима Conway:
    /// - Состояние клетки — не boolean, а сложная структура
    /// - Правила определяются геномами, а не фиксированной логикой
    /// - Присутствуют дополнительные системы: ресурсы, погода, аномалии
    /// - Симуляция асинхронная и событийно-ориентированная
    /// </summary>
    public class EcosystemMode : ISimulationMode
    {
        #region Реализация ISimulationMode

        /// <summary>
        /// Название режима симуляции.
        /// </summary>
        public string Name => "Ecosystem Simulation";

        /// <summary>
        /// Описание режима для отображения в UI.
        /// </summary>
        public string Description =>
            "Симуляция экосистемы с автономными ботами, ресурсами и эволюцией";

        #endregion

        #region Приватные поля

        /// <summary>
        /// Конфигурация режима.
        /// </summary>
        private SimulationConfig _config;

        /// <summary>
        /// Генератор случайных чисел для режима.
        /// Используется детерминированный seed для воспроизводимости.
        /// </summary>
        private Random _random;

        /// <summary>
        /// Коллекция всех активных ботов в мире.
        /// Ключ: уникальный ID бота, значение: экземпляр бота.
        /// </summary>
        private Dictionary<int, Bot> _bots;

        /// <summary>
        /// Карта распределения ресурсов по клеткам мира.
        /// Ключ: координата (x, y), значение: пул ресурсов клетки.
        /// </summary>
        private Dictionary<(int, int), ResourcePool> _worldResources;

        /// <summary>
        /// Счётчик для генерации уникальных ID ботов.
        /// </summary>
        private int _nextBotId;

        /// <summary>
        /// Параметры окружающей среды для текущего тика.
        /// </summary>
        private EnvironmentState _currentEnvironment;

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Количество активных ботов в симуляции.
        /// </summary>
        public int BotCount => _bots?.Count ?? 0;

        /// <summary>
        /// Текущий номер поколения симуляции.
        /// </summary>
        public long CurrentGeneration { get; private set; }

        #endregion

        #region Инициализация

        /// <summary>
        /// Инициализирует режим экосистемы с заданной конфигурацией.
        /// 
        /// Этот метод вызывается движком при переключении режима
        /// или при старте симуляции. Он подготавливает все
        /// внутренние структуры данных для работы.
        /// </summary>
        /// <param name="config">Конфигурация симуляции.</param>
        public void Initialize(SimulationConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _random = new Random(config.Seed);
            _bots = new Dictionary<int, Bot>();
            _worldResources = new Dictionary<(int, int), ResourcePool>();
            _nextBotId = 1;
            CurrentGeneration = 0;
            _currentEnvironment = new EnvironmentState();

            // Инициализируем ресурсы в мире
            InitializeWorldResources();
        }

        /// <summary>
        /// Распределяет начальные ресурсы по карте мира.
        /// Вызывается при инициализации режима.
        /// </summary>
        private void InitializeWorldResources()
        {
            // Создаём пулы ресурсов для каждой клетки
            for (int x = 0; x < _config.WorldWidth; x++)
            {
                for (int y = 0; y < _config.WorldHeight; y++)
                {
                    var pool = new ResourcePool(maxCapacity: 100);

                    // Случайное начальное распределение
                    if (_random.NextDouble() < 0.3)
                    {
                        pool.Add(ResourceType.Food, _random.Next(1, 10));
                    }
                    if (_random.NextDouble() < 0.2)
                    {
                        pool.Add(ResourceType.Water, _random.Next(1, 5));
                    }
                    if (_random.NextDouble() < 0.05)
                    {
                        pool.Add(ResourceType.Metal, _random.Next(1, 3));
                    }

                    _worldResources[(x, y)] = pool;
                }
            }
        }

        /// <summary>
        /// Создаёт и добавляет нового бота в мир.
        /// </summary>
        /// <param name="x">Начальная координата X.</param>
        /// <param name="y">Начальная координата Y.</param>
        /// <param name="genome">Геном нового бота.</param>
        /// <returns>Созданный экземпляр бота.</returns>
        public Bot SpawnBot(int x, int y, GenomeClass genome = null)
        {
            var newGenome = genome ?? Genome.CreateRandom(_random);
            var bot = new Bot(_nextBotId++, x, y, newGenome);
            _bots[bot.Id] = bot;
            return bot;
        }

        /// <summary>
        /// Заполняет мир случайными ботами.
        /// </summary>
        /// <param name="count">Количество ботов для создания.</param>
        /// <param name="density">Плотность размещения (0-1).</param>
        public void PopulateBots(int count, float density = 0.5f)
        {
            int placed = 0;
            int attempts = 0;
            int maxAttempts = count * 10;

            while (placed < count && attempts < maxAttempts)
            {
                attempts++;

                int x = _random.Next(_config.WorldWidth);
                int y = _random.Next(_config.WorldHeight);

                // Проверяем, свободна ли клетка
                if (!IsCellOccupied(x, y) && _random.NextDouble() < density)
                {
                    SpawnBot(x, y);
                    placed++;
                }
            }
        }

        #endregion

        #region Основной цикл симуляции

        /// <summary>
        /// Выполняет один шаг симуляции экосистемы.
        /// 
        /// Порядок выполнения:
        /// 1. Обновление окружающей среды (погода, ресурсы)
        /// 2. Обновление всех ботов (параллельно)
        /// 3. Обработка конфликтов и взаимодействий
        /// 4. Удаление мёртвых ботов
        /// 5. Инкремент поколения
        /// 
        /// Этот метод вызывается движком каждый тик.
        /// </summary>
        /// <param name="worldState">Текущее состояние мира.</param>
        /// <param name="generation">Номер поколения.</param>
        /// <returns>Новое состояние мира после шага.</returns>
        public bool[,] Step(bool[,] worldState, long generation)
        {
            CurrentGeneration = generation;

            // 1. Обновляем окружающую среду
            UpdateEnvironment();

            // 2. Обновляем ресурсы в мире
            UpdateWorldResources();

            // 3. Обновляем всех ботов
            UpdateBots();

            // 4. Удаляем мёртвых ботов
            CleanupDeadBots();

            // 5. Возвращаем упрощённое состояние для рендеринга
            return RenderToBooleanGrid(worldState);
        }

        /// <summary>
        /// Обновляет параметры окружающей среды.
        /// </summary>
        private void UpdateEnvironment()
        {
            // Простая модель: циклическое изменение "времени суток"
            _currentEnvironment.DayProgress =
                (CurrentGeneration % 1000) / 1000.0f;

            // Температура зависит от времени суток
            _currentEnvironment.Temperature =
                20 + (float)Math.Sin(_currentEnvironment.DayProgress * Math.PI * 2) * 10;
        }

        /// <summary>
        /// Обновляет распределение ресурсов в мире.
        /// </summary>
        private void UpdateWorldResources()
        {
            foreach (var kvp in _worldResources)
            {
                var pool = kvp.Value;

                // Естественное разложение ресурсов
                foreach (var type in ResourceType.All)
                {
                    if (type.DecayRate > 0 && pool.GetAmount(type) > 0)
                    {
                        float decay = pool.GetAmount(type) * type.DecayRate;
                        pool.Remove(type, decay);
                    }
                }

                // Случайная регенерация пищи
                if (_random.NextDouble() < 0.01)
                {
                    pool.Add(ResourceType.Food, _random.Next(1, 3));
                }
            }
        }

        /// <summary>
        /// Обновляет всех активных ботов.
        /// </summary>
        private void UpdateBots()
        {
            // Создаём копию ключей для безопасной итерации
            var botIds = _bots.Keys.ToList();

            foreach (var id in botIds)
            {
                if (_bots.TryGetValue(id, out var bot) && bot.IsActive)
                {
                    // Передаём ссылку на движок для запросов
                    // В полной реализации здесь был бы интерфейс IBotEngine
                    bot.Tick(null);
                }
            }
        }

        /// <summary>
        /// Удаляет мёртвых ботов из коллекции.
        /// </summary>
        private void CleanupDeadBots()
        {
            var deadIds = _bots
                .Where(kvp => !kvp.Value.IsActive)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var id in deadIds)
            {
                var bot = _bots[id];

                // Оставляем биомассу на месте смерти
                if (_worldResources.TryGetValue((bot.X, bot.Y), out var pool))
                {
                    pool.Add(ResourceType.Biomass, bot.Genome.MaxHealth * 0.3f);
                }

                _bots.Remove(id);
            }
        }

        /// <summary>
        /// Преобразует состояние экосистемы в булеву сетку для рендеринга.
        /// 
        /// Этот метод нужен для совместимости с интерфейсом ISimulationMode,
        /// который ожидает return типа bool[,]. В полной реализации
        /// рендерер должен работать с WorldState напрямую.
        /// </summary>
        private bool[,] RenderToBooleanGrid(bool[,] template)
        {
            var result = new bool[_config.WorldWidth, _config.WorldHeight];

            // Клетка "жива" если в ней есть бот или значительное количество ресурсов
            for (int x = 0; x < _config.WorldWidth; x++)
            {
                for (int y = 0; y < _config.WorldHeight; y++)
                {
                    bool hasBot = _bots.Values.Any(b => b.X == x && b.Y == y);
                    bool hasResources = _worldResources.TryGetValue((x, y), out var pool) &&
                                       pool.TotalWeight > 5;

                    result[x, y] = hasBot || hasResources;
                }
            }

            return result;
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Проверяет, занята ли клетка ботом.
        /// </summary>
        private bool IsCellOccupied(int x, int y)
        {
            return _bots.Values.Any(b => b.X == x && b.Y == y);
        }

        /// <summary>
        /// Получает пул ресурсов для указанной клетки.
        /// </summary>
        public ResourcePool GetCellResources(int x, int y)
        {
            if (x < 0 || x >= _config.WorldWidth ||
                y < 0 || y >= _config.WorldHeight)
                return null;

            if (!_worldResources.TryGetValue((x, y), out var pool))
            {
                pool = new ResourcePool();
                _worldResources[(x, y)] = pool;
            }

            return pool;
        }

        /// <summary>
        /// Находит ближайшего бота к указанной точке.
        /// </summary>
        /// <param name="x">Координата X точки поиска.</param>
        /// <param name="y">Координата Y точки поиска.</param>
        /// <param name="maxDistance">Максимальная дистанция поиска.</param>
        /// <returns>Ближайший бот или null если не найден.</returns>
        public Bot FindNearestBot(int x, int y, int maxDistance = 10)
        {
            Bot nearest = null;
            int minDist = int.MaxValue;

            foreach (var bot in _bots.Values)
            {
                int dx = Math.Abs(bot.X - x);
                int dy = Math.Abs(bot.Y - y);
                int dist = dx + dy; // Манхэттенское расстояние

                if (dist <= maxDistance && dist < minDist)
                {
                    nearest = bot;
                    minDist = dist;
                }
            }

            return nearest;
        }

        #endregion

        #region Применение правил (для совместимости)

        /// <summary>
        /// Применяет правила экосистемы к одной клетке.
        /// 
        /// В режиме экосистемы этот метод не используется напрямую,
        /// так как логика распределена по сущностям. Метод оставлен
        /// для совместимости с интерфейсом ISimulationMode.
        /// </summary>
        public bool ApplyRules(bool isAlive, int neighbourCount, long generation)
        {
            // В экосистеме правила определяются геномами ботов,
            // а не глобальной функцией. Возвращаем исходное значение.
            return isAlive;
        }

        #endregion
    }

    /// <summary>
    /// Состояние окружающей среды в текущий момент симуляции.
    /// 
    /// Содержит параметры, влияющие на поведение ботов:
    /// время суток, температура, погодные явления и т.д.
    /// </summary>
    public class EnvironmentState
    {
        /// <summary>
        /// Прогресс дня: 0.0 (рассвет) ... 0.5 (полдень) ... 1.0 (закат).
        /// </summary>
        public float DayProgress { get; set; }

        /// <summary>
        /// Температура среды в условных единицах.
        /// </summary>
        public float Temperature { get; set; } = 20.0f;

        /// <summary>
        /// Сила ветра: 0 (штиль) ... 1 (ураган).
        /// </summary>
        public float WindStrength { get; set; }

        /// <summary>
        /// Направление ветра в градусах (0 = север, 90 = восток).
        /// </summary>
        public float WindDirection { get; set; }

        /// <summary>
        /// Уровень радиоактивного фона в клетке.
        /// </summary>
        public float RadiationLevel { get; set; }

        /// <summary>
        /// Сила магнитных аномалий (влияет на навигацию).
        /// </summary>
        public float MagneticDistortion { get; set; }

        /// <summary>
        /// Проверяет, является ли текущий момент "днём".
        /// </summary>
        public bool IsDaytime => DayProgress > 0.25f && DayProgress < 0.75f;

        /// <summary>
        /// Проверяет, является ли текущий момент "ночью".
        /// </summary>
        public bool IsNighttime => !IsDaytime;
    }
}