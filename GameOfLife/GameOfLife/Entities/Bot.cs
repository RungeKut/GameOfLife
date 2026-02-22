using System;
using System.Linq;
using GameOfLife.Core;
using GameOfLife.Resources;
using GameOfLife.Genome;

// Псевдоним для разрешения конфликта имён: namespace GameOfLife.Genome vs class Genome
using GenomeClass = GameOfLife.Genome.Genome;

namespace GameOfLife.Entities
{
    /// <summary>
    /// Бот — автономный агент в экосистеме.
    /// 
    /// Бот обладает:
    /// - Уникальным идентификатором и координатами
    /// - Геномом, определяющим поведение и характеристики
    /// - Запасом энергии и здоровья
    /// - Инвентарём ресурсов
    /// - Программой принятия решений
    /// 
    /// Жизненный цикл бота:
    /// 1. Создание (Spawn) с геномом родителя или случайным
    /// 2. Активная фаза: Tick() каждый цикл симуляции
    /// 3. Смерть: при истощении энергии/здоровья или старости
    /// 4. Репродукция: создание потомка при достаточной энергии
    /// </summary>
    public class Bot : IEntity
    {
        #region Приватные поля

        /// <summary>
        /// Текущая горизонтальная координата.
        /// </summary>
        private int _x;

        /// <summary>
        /// Текущая вертикальная координата.
        /// </summary>
        private int _y;

        /// <summary>
        /// Текущий возраст в тиках симуляции.
        /// </summary>
        private int _age;

        /// <summary>
        /// Индекс текущей инструкции в программе движения.
        /// </summary>
        private int _movementProgramIndex;

        /// <summary>
        /// Счётчик выполнения текущей инструкции движения.
        /// </summary>
        private int _currentInstructionTick;

        #endregion

        #region Публичные свойства (реализация IEntity)

        /// <summary>
        /// Уникальный идентификатор бота.
        /// Присваивается при создании и не изменяется.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Горизонтальная координата бота в мире.
        /// </summary>
        public int X
        {
            get => _x;
            private set => _x = value;
        }

        /// <summary>
        /// Вертикальная координата бота в мире.
        /// </summary>
        public int Y
        {
            get => _y;
            private set => _y = value;
        }

        /// <summary>
        /// Индикатор активности бота.
        /// false означает, что бот мёртв и должен быть удалён.
        /// </summary>
        public bool IsActive { get; private set; }

        #endregion

        #region Состояние бота

        /// <summary>
        /// Текущий запас здоровья (0...MaxHealth).
        /// При достижении 0 бот умирает.
        /// </summary>
        public float Health { get; private set; }

        /// <summary>
        /// Текущий запас энергии (0...MaxEnergy).
        /// Расходуется на действия и метаболизм.
        /// </summary>
        public float Energy { get; private set; }

        /// <summary>
        /// Текущий возраст бота в тиках.
        /// </summary>
        public int Age => _age;

        /// <summary>
        /// Геном бота — наследуемые параметры и поведение.
        /// </summary>
        public Genome.Genome Genome { get; private set; }

        /// <summary>
        /// Инвентарь ресурсов, переносимых ботом.
        /// </summary>
        public ResourcePool Inventory { get; }

        /// <summary>
        /// Статистика бота для анализа и отладки.
        /// </summary>
        public BotStatistics Statistics { get; }

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт нового бота с заданными параметрами.
        /// </summary>
        /// <param name="id">Уникальный идентификатор.</param>
        /// <param name="x">Начальная координата X.</param>
        /// <param name="y">Начальная координата Y.</param>
        /// <param name="genome">Геном бота (создаётся случайно если null).</param>
        public Bot(int id, int x, int y, Genome.Genome genome = null)
        {
            Id = id;
            _x = x;
            _y = y;
            IsActive = true;

            Genome = genome ?? GenomeClass.CreateRandom(new Random());
            Inventory = new ResourcePool(maxCapacity: Genome.MaxEnergy);
            Statistics = new BotStatistics();

            // Инициализация состояния
            Health = Genome.MaxHealth;
            Energy = Genome.MaxEnergy * 0.5f; // Начинаем с половиной энергии
            _age = 0;
            _movementProgramIndex = 0;
            _currentInstructionTick = 0;
        }

        #endregion

        #region Основной метод обновления

        /// <summary>
        /// Выполняет один шаг логики бота.
        /// 
        /// Этот метод вызывается движком каждый тик симуляции.
        /// Он координирует все аспекты поведения бота:
        /// - Обновление возраста и метаболизма
        /// - Принятие решения на основе генома и контекста
        /// - Выполнение выбранного действия
        /// - Проверка условий смерти
        /// 
        /// Важно: метод не должен напрямую модифицировать мир.
        /// Все взаимодействия должны идти через запросы к движку.
        /// </summary>
        /// <param name="engine">Ссылка на движок для запросов.</param>
        public void Tick(SimulationEngine engine)
        {
            if (!IsActive)
                return;

            // 1. Обновляем возраст
            _age++;

            // 2. Расходуем энергию на метаболизм
            Energy -= Genome.GetMetabolismCost();

            // 3. Проверяем условия смерти
            if (CheckDeathConditions())
            {
                Die();
                return;
            }

            // 4. Определяем контекст поведения
            var context = DetermineBehaviorContext();

            // 5. Получаем решение от генома
            var decision = Genome.Decide(this, context);

            // 6. Выполняем действие
            ExecuteDecision(decision, engine);

            // 7. Обновляем программу движения
            UpdateMovementProgram();

            // 8. Обновляем статистику
            Statistics.Update(this);
        }

        /// <summary>
        /// Проверяет условия смерти бота.
        /// </summary>
        /// <returns>True если бот должен умереть.</returns>
        private bool CheckDeathConditions()
        {
            // Смерть от истощения энергии
            if (Energy <= 0)
            {
                Statistics.DeathReason = DeathReason.Starvation;
                return true;
            }

            // Смерть от потери здоровья
            if (Health <= 0)
            {
                Statistics.DeathReason = DeathReason.Injury;
                return true;
            }

            // Естественная смерть от старости
            if (_age >= Genome.MaxAge)
            {
                Statistics.DeathReason = DeathReason.OldAge;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Определяет текущий контекст поведения бота.
        /// </summary>
        private BehaviorContext DetermineBehaviorContext()
        {
            // Приоритет: угроза > голод > размножение > сбор > отдых
            if (IsThreatened())
                return BehaviorContext.Threatened;

            if (Energy < Genome.MaxEnergy * 0.3f)
                return BehaviorContext.Hungry;

            if (Genome.CanReproduce(Energy))
                return BehaviorContext.Reproducing;

            if (HasValuableResourcesNearby())
                return BehaviorContext.Gathering;

            return BehaviorContext.Idle;
        }

        #endregion

        #region Выполнение решений

        /// <summary>
        /// Выполняет принятое решение.
        /// </summary>
        private void ExecuteDecision(BehaviorDecision decision, SimulationEngine engine)
        {
            if (decision == null)
                return;

            switch (decision.Action)
            {
                case ActionType.Move:
                    TryMove(decision.TargetX, decision.TargetY, engine);
                    break;

                case ActionType.Gather:
                    TryGatherResources(decision.ResourceType, engine);
                    break;

                case ActionType.Attack:
                    TryAttack(decision.TargetX, decision.TargetY, engine);
                    break;

                case ActionType.Rest:
                    Rest();
                    break;

                case ActionType.Reproduce:
                    TryReproduce(engine);
                    break;
            }
        }

        /// <summary>
        /// Пытается переместиться в указанную клетку.
        /// </summary>
        private void TryMove(int targetX, int targetY, SimulationEngine engine)
        {
            // В полной реализации здесь была бы проверка:
            // - Занята ли целевая клетка
            // - Допустимо ли перемещение (границы, препятствия)
            // - Достаточно ли энергии
            // - Разрешение конфликтов с другими ботами

            // Упрощённая версия: перемещаем если в пределах мира
            if (targetX >= 0 && targetX < 100 && targetY >= 0 && targetY < 100)
            {
                X = targetX;
                Y = targetY;
                Energy -= 1.0f; // Стоимость перемещения
            }
        }

        /// <summary>
        /// Возвращает предпочтительное направление движения на основе программы генома.
        /// 
        /// Этот метод не раскрывает внутреннее состояние бота (индекс программы),
        /// но позволяет системе принятия решений узнать, куда бот "хочет" пойти.
        /// 
        /// Возвращаемое значение — это дельта (dx, dy) для применения к текущим координатам.
        /// </summary>
        /// <returns>Кортеж (dx, dy) представляющий желаемое смещение.</returns>
        public (int dx, int dy) GetPreferredMovementDelta()
        {
            if (Genome.MovementProgram.Count == 0)
                return (0, 0);

            // Вычисляем текущий индекс программы (локальная копия логики)
            int currentIndex = _movementProgramIndex % Genome.MovementProgram.Count;
            var instruction = Genome.MovementProgram[currentIndex];

            return instruction.GetDelta();
        }

        /// <summary>
        /// Пытается собрать ресурс в текущей клетке.
        /// </summary>
        private void TryGatherResources(ResourceType type, SimulationEngine engine)
        {
            // В полной реализации: запрос к ResourceManager
            // Упрощённо: если ресурс есть, забираем часть
            if (type != null && Inventory.HasEnough(type, 1))
            {
                float gathered = Math.Min(2, Inventory.GetAmount(type));
                Inventory.Remove(type, gathered);
                Energy += type.EnergyValue * gathered * 0.5f;
            }
        }

        /// <summary>
        /// Выполняет отдых: восстановление энергии.
        /// </summary>
        private void Rest()
        {
            Energy = Math.Min(Genome.MaxEnergy, Energy + Genome.MetabolismRate * 0.5f);
        }

        /// <summary>
        /// Пытается создать потомка.
        /// </summary>
        private void TryReproduce(SimulationEngine engine)
        {
            if (!Genome.CanReproduce(Energy))
                return;

            // В полной реализации: поиск свободной соседней клетки
            // и создание нового бота с мутировавшим геномом

            // Упрощённо: тратим энергию, увеличиваем счётчик
            Energy *= 0.5f; // Тратим половину энергии на размножение
            Statistics.OffspringCount++;
        }

        /// <summary>
        /// Пытается атаковать бота в целевой клетке.
        /// </summary>
        private void TryAttack(int targetX, int targetY, SimulationEngine engine)
        {
            // В полной реализации: поиск цели и расчёт урона
            Energy -= 2.0f; // Стоимость атаки
        }

        #endregion

        #region Программа движения

        /// <summary>
        /// Обновляет выполнение программы движения генома.
        /// </summary>
        private void UpdateMovementProgram()
        {
            if (Genome.MovementProgram.Count == 0)
                return;

            var instruction = Genome.MovementProgram[_movementProgramIndex];

            if (_currentInstructionTick == 0)
            {
                instruction.Reset();
            }

            if (!instruction.Advance())
            {
                // Продолжаем текущую инструкцию
                var (dx, dy) = instruction.GetDelta();
                TryMove(X + dx, Y + dy, null);
            }
            else
            {
                // Переходим к следующей инструкции
                _movementProgramIndex = (_movementProgramIndex + 1) % Genome.MovementProgram.Count;
                _currentInstructionTick = 0;
            }
        }

        #endregion

        #region Смерть и очистка

        /// <summary>
        /// Обрабатывает смерть бота.
        /// </summary>
        private void Die()
        {
            IsActive = false;

            // Оставляем ресурсы в инвентаре на месте смерти
            // (в полной реализации: передача в ResourceManager)

            Statistics.DeathTick = _age;
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Проверяет наличие угрозы поблизости.
        /// </summary>
        private bool IsThreatened()
        {
            // Упрощённая проверка: случайный шанс для демонстрации
            return new Random().NextDouble() < 0.01f;
        }

        /// <summary>
        /// Проверяет наличие ценных ресурсов поблизости.
        /// </summary>
        private bool HasValuableResourcesNearby()
        {
            // Упрощённая проверка: случайный шанс
            return new Random().NextDouble() < 0.1f;
        }

        /// <summary>
        /// Наносит урон здоровью бота.
        /// </summary>
        /// <param name="damage">Количество урона.</param>
        /// <returns>Фактический нанесённый урон (с учётом защиты).</returns>
        public float TakeDamage(float damage)
        {
            float mitigated = damage * (1 - Genome.DefenseFactor);
            Health = Math.Max(0, Health - mitigated);
            return mitigated;
        }

        /// <summary>
        /// Добавляет ресурс в инвентарь бота.
        /// </summary>
        public bool AddResource(ResourceType type, float amount)
        {
            return Inventory.Add(type, amount);
        }

        /// <summary>
        /// Возвращает расстояние до другой точки.
        /// </summary>
        public int DistanceTo(int targetX, int targetY)
        {
            return Math.Abs(X - targetX) + Math.Abs(Y - targetY);
        }

        #endregion
    }

    /// <summary>
    /// Статистика бота для анализа и отладки.
    /// </summary>
    public class BotStatistics
    {
        /// <summary>
        /// Причина смерти (если бот умер).
        /// </summary>
        public DeathReason? DeathReason { get; set; }

        /// <summary>
        /// Тик смерти (возраст на момент смерти).
        /// </summary>
        public int? DeathTick { get; set; }

        /// <summary>
        /// Количество произведённого потомства.
        /// </summary>
        public int OffspringCount { get; set; }

        /// <summary>
        /// Максимальный достигнутый уровень энергии.
        /// </summary>
        public float MaxEnergyReached { get; private set; }

        /// <summary>
        /// Общее количество собранных ресурсов.
        /// </summary>
        public float TotalResourcesGathered { get; private set; }

        /// <summary>
        /// Обновляет статистику на основе текущего состояния бота.
        /// </summary>
        public void Update(Bot bot)
        {
            MaxEnergyReached = Math.Max(MaxEnergyReached, bot.Energy);
            // TotalResourcesGathered обновляется при сборе ресурсов
        }
    }

    /// <summary>
    /// Возможные причины смерти бота.
    /// </summary>
    public enum DeathReason
    {
        /// <summary>
        /// Смерть от истощения энергии.
        /// </summary>
        Starvation,

        /// <summary>
        /// Смерть от потери здоровья (урон, токсичность).
        /// </summary>
        Injury,

        /// <summary>
        /// Естественная смерть от старости.
        /// </summary>
        OldAge,

        /// <summary>
        /// Смерть от радиоактивного облучения.
        /// </summary>
        Radiation,

        /// <summary>
        /// Смерть в результате атаки другим ботом.
        /// </summary>
        Combat
    }
}