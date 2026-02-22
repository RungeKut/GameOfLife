using System;
using System.Collections.Generic;
using System.Linq;

namespace GameOfLife.Genome
{
    /// <summary>
    /// Геном бота — набор наследуемых параметров и программ поведения.
    /// 
    /// Геном определяет:
    /// - Физиологические характеристики бота (метаболизм, здоровье)
    /// - Поведенческие склонности (агрессия, социальность)
    /// - Программу движения и принятия решений
    /// - Вероятность и параметры мутаций
    /// 
    /// Геномы могут:
    /// - Мутировать случайным образом при репликации
    /// - Скрещиваться для создания потомства
    /// - Эволюционировать под давлением естественного отбора
    /// </summary>
    public class Genome
    {
        #region Константы и настройки по умолчанию

        /// <summary>
        /// Стандартная вероятность мутации гена.
        /// Значение 0.01 = 1% шанс мутации при репликации.
        /// </summary>
        public const float DefaultMutationRate = 0.01f;

        /// <summary>
        /// Максимальное отклонение при мутации числового гена.
        /// Применяется как множитель к текущему значению.
        /// </summary>
        public const float MaxMutationDelta = 0.2f;

        #endregion

        #region Физиологические гены

        /// <summary>
        /// Скорость метаболизма: расход энергии за тик.
        /// 
        /// Диапазон: 0.1 - 3.0
        /// - Низкие значения: бот тратит мало энергии, но медленнее действует
        /// - Высокие значения: бот активен, но быстро истощается
        /// 
        /// Влияет на:
        /// - Базовый расход энергии каждый тик
        /// - Максимальную скорость выполнения действий
        /// - Потребность в частом поиске ресурсов
        /// </summary>
        public float MetabolismRate { get; set; } = 1.0f;

        /// <summary>
        /// Максимальный запас здоровья бота.
        /// 
        /// Диапазон: 50 - 500
        /// - Низкие значения: бот уязвим, но требует меньше ресурсов
        /// - Высокие значения: бот вынослив, но медленнее размножается
        /// 
        /// Здоровье уменьшается при:
        /// - Получении урона от атак
        /// - Контакте с токсичными ресурсами
        /// - Воздействии радиоактивных зон
        /// </summary>
        public float MaxHealth { get; set; } = 100.0f;

        /// <summary>
        /// Максимальный запас энергии бота.
        /// 
        /// Диапазон: 50 - 1000
        /// - Определяет "топливный бак" бота
        /// - Влияет на дальность миграции и частоту действий
        /// 
        /// Энергия расходуется на:
        /// - Перемещение по карте
        /// - Выполнение действий (атака, строительство)
        /// - Базовый метаболизм
        /// </summary>
        public float MaxEnergy { get; set; } = 200.0f;

        /// <summary>
        /// Максимальная продолжительность жизни в тиках.
        /// 
        /// Диапазон: 100 - 10000
        /// - Низкие значения: быстрая смена поколений, ускоренная эволюция
        /// - Высокие значения: стабильные популяции, медленная адаптация
        /// 
        /// После достижения этого возраста бот умирает независимо
        /// от состояния здоровья и энергии (естественная смерть).
        /// </summary>
        public int MaxAge { get; set; } = 1000;

        #endregion

        #region Поведенческие гены

        /// <summary>
        /// Склонность к агрессии: вероятность атаки при встрече.
        /// 
        /// Диапазон: 0.0 - 1.0
        /// - 0.0: бот никогда не атакует первым
        /// - 1.0: бот атакует при любой возможности
        /// 
        /// Влияет на:
        /// - Решение о нападении на других ботов
        /// - Реакцию на угрозу (бегство или контратака)
        /// - Приоритет сбора ресурсов vs охоты
        /// </summary>
        public float Aggression { get; set; } = 0.3f;

        /// <summary>
        /// Склонность к кооперации: вероятность помощи сородичам.
        /// 
        /// Диапазон: 0.0 - 1.0
        /// - 0.0: бот игнорирует или избегает других
        /// - 1.0: бот активно ищет и помогает сородичам
        /// 
        /// Влияет на:
        /// - Обмен ресурсами с другими ботами
        /// - Совместную защиту от угроз
        /// - Формирование групп и колоний
        /// </summary>
        public float Sociability { get; set; } = 0.2f;

        /// <summary>
        /// Предпочтение типов ресурсов для сбора.
        /// 
        /// Значение в диапазоне [0, 1] для каждого типа:
        /// - Высокое значение: бот приоритетно ищет этот ресурс
        /// - Низкое значение: бот игнорирует этот тип
        /// 
        /// Используется системой принятия решений для выбора
        /// цели при поиске ресурсов. Может эволюционировать
        /// в ответ на доступность ресурсов в среде.
        /// </summary>
        public Dictionary<Resources.ResourceType, float> ResourcePreferences { get; set; }

        /// <summary>
        /// Скорость перемещения: клеток за тик.
        /// 
        /// Диапазон: 1 - 5
        /// - 1: бот перемещается на 1 клетку за тик (стандарт)
        /// - 5: бот может перескочить несколько клеток
        /// 
        /// Более высокая скорость требует больше энергии
        /// и может быть ограничена типом местности.
        /// </summary>
        public int MovementSpeed { get; set; } = 1;

        /// <summary>
        /// Фактор защиты: снижение получаемого урона.
        /// 
        /// Диапазон: 0.0 - 0.9
        /// - 0.0: бот получает полный урон
        /// - 0.9: бот получает только 10% урона
        /// 
        /// Защита может обеспечиваться:
        /// - Естественной бронёй (эволюционная черта)
        /// - Построенными укрытиями (внешний фактор)
        /// - Избеганием опасных зон (поведенческий фактор)
        /// </summary>
        public float DefenseFactor { get; set; } = 0.0f;

        #endregion

        #region Гены репликации

        /// <summary>
        /// Порог энергии для начала размножения.
        /// 
        /// Диапазон: 0.3 - 0.9 (от MaxEnergy)
        /// - Низкие значения: бот размножается часто, но с малым запасом
        /// - Высокие значения: бот копит энергию, но размножается редко
        /// 
        /// При достижении этого порога бот может инициировать
        /// процесс размножения если есть свободная соседняя клетка.
        /// </summary>
        public float ReproductionThreshold { get; set; } = 0.7f;

        /// <summary>
        /// Вероятность мутации при репликации генома.
        /// 
        /// Диапазон: 0.0 - 0.1
        /// - 0.0: потомство идентично родителю
        /// - 0.1: 10% генов могут измениться случайно
        /// 
        /// Мутации — двигатель эволюции:
        /// - Позволяют адаптироваться к изменяющейся среде
        /// - Создают разнообразие для естественного отбора
        /// - Но слишком высокая частота дестабилизирует популяцию
        /// </summary>
        public float MutationRate { get; set; } = DefaultMutationRate;

        #endregion

        #region Программа поведения

        /// <summary>
        /// Последовательность команд движения на один цикл.
        /// 
        /// Каждая инструкция указывает направление и длительность.
        /// Программа выполняется циклически, если не прервана
        /// внешними событиями (угроза, найден ресурс и т.д.).
        /// 
        /// Пример программы:
        /// [North(2), East(1), Stay(3), South(2)]
        /// означает: 2 шага на север, 1 на восток, 3 стоять, 2 на юг
        /// </summary>
        public List<MovementInstruction> MovementProgram { get; set; }

        /// <summary>
        /// Приоритеты действий в различных ситуациях.
        /// 
        /// Ключ: тип ситуации (голод, угроза, поиск партнёра)
        /// Значение: приоритет действия (0-100, выше = важнее)
        /// 
        /// Используется системой принятия решений для выбора
        /// следующего действия при множестве возможных вариантов.
        /// </summary>
        public Dictionary<BehaviorContext, int> ActionPriorities { get; set; }

        #endregion

        #region Конструкторы

        /// <summary>
        /// Создаёт пустой геном со значениями по умолчанию.
        /// </summary>
        public Genome()
        {
            ResourcePreferences = new Dictionary<Resources.ResourceType, float>();
            MovementProgram = new List<MovementInstruction>();
            ActionPriorities = new Dictionary<BehaviorContext, int>();
            InitializeDefaultPreferences();
        }

        /// <summary>
        /// Создаёт копию существующего генома.
        /// Используется при репликации без мутаций.
        /// </summary>
        /// <param name="other">Геном для копирования.</param>
        public Genome(Genome other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            // Копируем простые свойства
            MetabolismRate = other.MetabolismRate;
            MaxHealth = other.MaxHealth;
            MaxEnergy = other.MaxEnergy;
            MaxAge = other.MaxAge;
            Aggression = other.Aggression;
            Sociability = other.Sociability;
            MovementSpeed = other.MovementSpeed;
            DefenseFactor = other.DefenseFactor;
            ReproductionThreshold = other.ReproductionThreshold;
            MutationRate = other.MutationRate;

            // Глубокое копирование коллекций
            ResourcePreferences = new Dictionary<Resources.ResourceType, float>(
                other.ResourcePreferences);
            MovementProgram = new List<MovementInstruction>(
                other.MovementProgram.Select(i => new MovementInstruction(i)));
            ActionPriorities = new Dictionary<BehaviorContext, int>(
                other.ActionPriorities);
        }

        #endregion

        #region Инициализация

        /// <summary>
        /// Инициализирует предпочтения ресурсов значениями по умолчанию.
        /// Вызывается конструктором для новых геномов.
        /// </summary>
        private void InitializeDefaultPreferences()
        {
            // По умолчанию бот предпочитает пищу и воду
            foreach (var type in Resources.ResourceType.All)
            {
                ResourcePreferences[type] = type.IsEnergy ? 0.8f : 0.2f;
            }

            // Инициализируем приоритеты действий
            ActionPriorities[BehaviorContext.Hungry] = 90;
            ActionPriorities[BehaviorContext.Threatened] = 95;
            ActionPriorities[BehaviorContext.Idle] = 50;
            ActionPriorities[BehaviorContext.Reproducing] = 80;
        }

        /// <summary>
        /// Создаёт случайный геном для начальной популяции.
        /// Используется при генерации мира.
        /// </summary>
        /// <param name="random">Генератор случайных чисел.</param>
        /// <returns>Новый геном со случайными параметрами.</returns>
        public static Genome CreateRandom(Random random)
        {
            var genome = new Genome
            {
                MetabolismRate = (float)(0.5 + random.NextDouble() * 2.0),
                MaxHealth = 50 + (float)(random.NextDouble() * 450),
                MaxEnergy = 100 + (float)(random.NextDouble() * 900),
                MaxAge = 200 + random.Next(800),
                Aggression = (float)random.NextDouble(),
                Sociability = (float)random.NextDouble(),
                MovementSpeed = 1 + random.Next(4),
                DefenseFactor = (float)(random.NextDouble() * 0.5),
                ReproductionThreshold = 0.4f + (float)(random.NextDouble() * 0.4),
                MutationRate = (float)(random.NextDouble() * 0.05)
            };

            // Случайные предпочтения ресурсов
            foreach (var type in Resources.ResourceType.All)
            {
                genome.ResourcePreferences[type] = (float)random.NextDouble();
            }

            // Случайная программа движения
            int programLength = 3 + random.Next(8);
            for (int i = 0; i < programLength; i++)
            {
                var direction = (MovementDirection)random.Next(
                    Enum.GetValues(typeof(MovementDirection)).Length);
                var duration = 1 + random.Next(5);
                genome.MovementProgram.Add(
                    new MovementInstruction(direction, duration));
            }

            return genome;
        }

        #endregion

        #region Мутации

        /// <summary>
        /// Применяет случайные мутации к геному.
        /// 
        /// Каждый ген имеет шанс MutationRate быть изменённым.
        /// Изменение происходит в диапазоне +/- MaxMutationDelta
        /// от текущего значения с ограничением допустимых границ.
        /// 
        /// Метод модифицирует текущий геном (in-place).
        /// Для создания мутировавшей копии используйте Clone().Mutate().
        /// </summary>
        /// <param name="random">Генератор случайных чисел.</param>
        public void Mutate(Random random)
        {
            // Вспомогательная функция для мутации float-свойства (без ref)
            void MutateFloatProperty(Func<float> getter, Action<float> setter, float min, float max)
            {
                if (random.NextDouble() < MutationRate)
                {
                    float currentValue = getter();
                    float delta = (float)(random.NextDouble() - 0.5) * 2 * MaxMutationDelta * currentValue;
                    float newValue = Math.Max(min, Math.Min(max, currentValue + delta));
                    setter(newValue);
                }
            }

            // Мутация физиологических генов
            MutateFloatProperty(() => MetabolismRate, v => MetabolismRate = v, 0.1f, 3.0f);
            MutateFloatProperty(() => MaxHealth, v => MaxHealth = v, 50, 500);
            MutateFloatProperty(() => MaxEnergy, v => MaxEnergy = v, 50, 1000);
            MutateFloatProperty(() => Aggression, v => Aggression = v, 0, 1);
            MutateFloatProperty(() => Sociability, v => Sociability = v, 0, 1);
            MutateFloatProperty(() => DefenseFactor, v => DefenseFactor = v, 0, 0.9f);
            MutateFloatProperty(() => ReproductionThreshold, v => ReproductionThreshold = v, 0.3f, 0.9f);
            MutateFloatProperty(() => MutationRate, v => MutationRate = v, 0, 0.1f);

            // Мутация целочисленных генов
            if (random.NextDouble() < MutationRate)
            {
                int delta = random.Next(-1, 2);
                MaxAge = Math.Max(100, Math.Min(10000, MaxAge + delta * 100));
            }

            if (random.NextDouble() < MutationRate)
            {
                int delta = random.Next(-1, 2);
                MovementSpeed = Math.Max(1, Math.Min(5, MovementSpeed + delta));
            }

            // Мутация предпочтений ресурсов
            foreach (var key in ResourcePreferences.Keys.ToList())
            {
                if (random.NextDouble() < MutationRate)
                {
                    float currentValue = ResourcePreferences[key];
                    float delta = (float)(random.NextDouble() - 0.5) * 0.4f;
                    ResourcePreferences[key] = Math.Max(0, Math.Min(1, currentValue + delta));
                }
            }

            // Мутация программы движения
            MutateMovementProgram(random);
        }

        /// <summary>
        /// Применяет мутации к программе движения.
        /// </summary>
        private void MutateMovementProgram(Random random)
        {
            // Шанс добавить новую инструкцию
            if (random.NextDouble() < MutationRate && MovementProgram.Count < 20)
            {
                var direction = (MovementDirection)random.Next(
                    Enum.GetValues(typeof(MovementDirection)).Length);
                var duration = 1 + random.Next(5);
                int insertPos = random.Next(MovementProgram.Count + 1);
                MovementProgram.Insert(insertPos,
                    new MovementInstruction(direction, duration));
            }

            // Шанс удалить инструкцию
            if (random.NextDouble() < MutationRate && MovementProgram.Count > 1)
            {
                int removePos = random.Next(MovementProgram.Count);
                MovementProgram.RemoveAt(removePos);
            }

            // Шанс изменить существующие инструкции
            for (int i = 0; i < MovementProgram.Count; i++)
            {
                if (random.NextDouble() < MutationRate)
                {
                    var instruction = MovementProgram[i];

                    // Мутация направления
                    if (random.NextDouble() < 0.5)
                    {
                        var directions = Enum.GetValues(typeof(MovementDirection));
                        instruction.Direction = (MovementDirection)
                            directions.GetValue(random.Next(directions.Length));
                    }

                    // Мутация длительности
                    if (random.NextDouble() < 0.5)
                    {
                        instruction.Duration = Math.Max(1,
                            instruction.Duration + random.Next(-2, 3));
                    }

                    MovementProgram[i] = instruction;
                }
            }
        }

        #endregion

        #region Скрещивание

        /// <summary>
        /// Создаёт потомка путём скрещивания двух родительских геномов.
        /// 
        /// Для каждого гена случайно выбирается значение от одного
        /// из родителей (равномерное распределение).
        /// 
        /// После скрещивания к потомку применяются мутации
        /// с вероятностью, усреднённой от родителей.
        /// </summary>
        /// <param name="parent1">Первый родитель.</param>
        /// <param name="parent2">Второй родитель.</param>
        /// <param name="random">Генератор случайных чисел.</param>
        /// <returns>Новый геном-потомок.</returns>
        public static Genome Crossover(Genome parent1, Genome parent2, Random random)
        {
            if (parent1 == null)
                throw new ArgumentNullException(nameof(parent1));
            if (parent2 == null)
                throw new ArgumentNullException(nameof(parent2));

            var child = new Genome();

            // Вспомогательная функция для выбора гена
            T Pick<T>(T a, T b) => random.NextDouble() < 0.5 ? a : b;

            // Скрещивание простых свойств
            child.MetabolismRate = Pick(parent1.MetabolismRate, parent2.MetabolismRate);
            child.MaxHealth = Pick(parent1.MaxHealth, parent2.MaxHealth);
            child.MaxEnergy = Pick(parent1.MaxEnergy, parent2.MaxEnergy);
            child.MaxAge = Pick(parent1.MaxAge, parent2.MaxAge);
            child.Aggression = Pick(parent1.Aggression, parent2.Aggression);
            child.Sociability = Pick(parent1.Sociability, parent2.Sociability);
            child.MovementSpeed = Pick(parent1.MovementSpeed, parent2.MovementSpeed);
            child.DefenseFactor = Pick(parent1.DefenseFactor, parent2.DefenseFactor);
            child.ReproductionThreshold = Pick(parent1.ReproductionThreshold,
                parent2.ReproductionThreshold);
            child.MutationRate = (parent1.MutationRate + parent2.MutationRate) / 2;

            // Скрещивание предпочтений ресурсов
            foreach (var type in Resources.ResourceType.All)
            {
                child.ResourcePreferences[type] = Pick(
                    parent1.ResourcePreferences[type],
                    parent2.ResourcePreferences[type]);
            }

            // Скрещивание программы движения (чередование инструкций)
            int minLength = Math.Min(
                parent1.MovementProgram.Count,
                parent2.MovementProgram.Count);
            for (int i = 0; i < minLength; i++)
            {
                child.MovementProgram.Add(Pick(
                    parent1.MovementProgram[i],
                    parent2.MovementProgram[i]));
            }

            // Добавляем оставшиеся инструкции от более длинного родителя
            if (parent1.MovementProgram.Count > minLength)
            {
                for (int i = minLength; i < parent1.MovementProgram.Count; i++)
                {
                    child.MovementProgram.Add(
                        new MovementInstruction(parent1.MovementProgram[i]));
                }
            }
            else if (parent2.MovementProgram.Count > minLength)
            {
                for (int i = minLength; i < parent2.MovementProgram.Count; i++)
                {
                    child.MovementProgram.Add(
                        new MovementInstruction(parent2.MovementProgram[i]));
                }
            }

            // Применяем мутации к потомку
            child.Mutate(random);

            return child;
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Возвращает базовый расход энергии за тик.
        /// </summary>
        public float GetMetabolismCost()
        {
            return MetabolismRate;
        }

        /// <summary>
        /// Проверяет, готов ли бот к размножению.
        /// </summary>
        /// <param name="currentEnergy">Текущая энергия бота.</param>
        /// <returns>True если энергия выше порога репродукции.</returns>
        public bool CanReproduce(float currentEnergy)
        {
            return currentEnergy >= MaxEnergy * ReproductionThreshold;
        }

        /// <summary>
        /// Создаёт независимую копию генома.
        /// </summary>
        /// <returns>Новый экземпляр с теми же данными.</returns>
        public Genome Clone()
        {
            return new Genome(this);
        }

        #endregion
    }

    /// <summary>
    /// Контекст поведения для системы принятия решений.
    /// 
    /// Определяет ситуацию, в которой находится бот,
    /// и используется для выбора приоритетного действия
    /// на основе генома.
    /// </summary>
    public enum BehaviorContext
    {
        /// <summary>
        /// Бот не имеет срочных целей.
        /// Может исследовать, искать ресурсы или отдыхать.
        /// </summary>
        Idle,

        /// <summary>
        /// Уровень энергии бота низкий.
        /// Приоритет: поиск и потребление ресурсов.
        /// </summary>
        Hungry,

        /// <summary>
        /// Бот обнаружил угрозу (враг, аномалия).
        /// Приоритет: защита, бегство или контратака.
        /// </summary>
        Threatened,

        /// <summary>
        /// Бот имеет достаточно энергии для размножения.
        /// Приоритет: поиск места и создание потомства.
        /// </summary>
        Reproducing,

        /// <summary>
        /// Бот находится в зоне с ценными ресурсами.
        /// Приоритет: сбор и накопление.
        /// </summary>
        Gathering,

        /// <summary>
        /// Бот строит или использует структуру.
        /// Приоритет: завершение строительного действия.
        /// </summary>
        Building
    }
}