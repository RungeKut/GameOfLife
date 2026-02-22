using GameOfLife.Core;
using GameOfLife.Environment;
using GameOfLife.Genome;
using GameOfLife.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
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

        #region Намерения действий

        /// <summary>
        /// Создаёт намерение действия для следующего тика.
        /// 
        /// В отличие от Tick() который выполняет действие сразу,
        /// этот метод только декларирует желание бота выполнить
        /// действие без фактического изменения состояния мира.
        /// 
        /// Намерение будет передано в ConflictResolver который
        /// разрешит конфликты с другими ботами перед выполнением.
        /// </summary>
        /// <returns>Намерение действия или null если бот неактивен.</returns>
        public ActionIntention CreateIntention()
        {
            if (!IsActive)
                return null;

            // В полной реализации: анализ состояния и создание намерения
            // на основе генома и контекста
            return new ActionIntention(this)
            {
                Type = IntentionType.Stay,
                TargetX = X,
                TargetY = Y,
                Priority = Energy
            };
        }

        /// <summary>
        /// Выполняет разрешённое намерение действия.
        /// 
        /// Вызывается после разрешения конфликтов только для
        /// намерений с IsResolved = true и IsSuccessful = true.
        /// </summary>
        /// <param name="intention">Разрешённое намерение для выполнения.</param>
        public void ExecuteIntention(ActionIntention intention)
        {
            if (!intention.IsSuccessful)
                return;

            switch (intention.Type)
            {
                case IntentionType.Move:
                    MoveTo(intention.TargetX, intention.TargetY);
                    break;
                case IntentionType.Attack:
                    ExecuteAttack(intention.TargetX, intention.TargetY);
                    break;
                case IntentionType.Gather:
                    GatherResources();
                    break;
                case IntentionType.Build:
                    BuildStructure();
                    break;
                case IntentionType.Rest:
                    Rest();
                    break;
            }
        }

        /// <summary>
        /// Обработка ситуации когда намерение было отклонено.
        /// 
        /// Вызывается для ботов чьи намерения не прошли разрешение
        /// конфликтов. Бот может:
        /// - Остаться на текущей клетке
        /// - Попытаться найти альтернативное действие
        /// - Получить урон от конфликта
        /// </summary>
        /// <param name="rejectedIntention">Отклонённое намерение.</param>
        public void OnIntentionRejected(ActionIntention rejectedIntention)
        {
            // Бот остаётся на текущей позиции
            // В полной реализации: поиск альтернативного действия
        }

        #endregion

        #region Выполнение намерений

        /// <summary>
        /// Перемещает бота в указанную клетку.
        /// Вызывается только для разрешённых намерений.
        /// </summary>
        private void MoveTo(int targetX, int targetY)
        {
            // Проверка границ мира
            if (targetX < 0 || targetX >= 100 || targetY < 0 || targetY >= 100)
                return;

            // Проверка достаточно ли энергии
            if (Energy < 1.0f)
                return;

            // Перемещаем бота
            _x = targetX;
            _y = targetY;
            Energy -= 1.0f; // Стоимость перемещения
        }

        /// <summary>
        /// Выполняет атаку на целевую клетку.
        /// Вызывается только для разрешённых намерений.
        /// </summary>
        private void ExecuteAttack(int targetX, int targetY)
        {
            // Проверка достаточно ли энергии
            if (Energy < 2.0f)
                return;

            // В полной реализации: поиск цели в целевой клетке и нанесение урона
            // Сейчас: просто тратим энергию
            Energy -= 2.0f;
        }

        /// <summary>
        /// Собирает ресурсы в текущей клетке.
        /// Заглушка для Этапа 5.
        /// </summary>
        private void GatherResources()
        {
            // TODO: Этап 6 - Реализация системы ресурсов
            // В полной реализации: запрос к ResourceManager мира
        }

        /// <summary>
        /// Строит структуру в текущей клетке.
        /// Заглушка для Этапа 5.
        /// </summary>
        private void BuildStructure()
        {
            // TODO: Этап 6 - Реализация системы строительства
            // В полной реализации: проверка материалов, создание Structure
        }

        #endregion

        #region Воздействие окружающей среды

        /// <summary>
        /// Применяет воздействие погоды на бота.
        /// 
        /// Вызывается из Tick() после основных действий.
        /// Учитывает:
        /// - Расход энергии из-за температуры
        /// - Вероятность сдувания ветром
        /// - Ограничения движения в шторм
        /// </summary>
        /// <param name="weather">
        /// Система погоды мира.
        /// </param>
        public void ApplyWeatherEffect(WeatherSystem weather)
        {
            if (!IsActive)
                return;

            // Модификатор расхода энергии из-за погоды
            float costModifier = weather.GetEnergyCostModifier();
            Energy -= Genome.GetMetabolismCost() * (costModifier - 1.0f);

            // Проверка на сдувание ветром
            if (weather.WillBotBeBlownAway(Energy, Genome.MaxEnergy))
            {
                var (dx, dy) = weather.GetBlowDirection();

                // Перемещаем бота в направлении ветра
                int newX = X + dx;
                int newY = Y + dy;

                // В полной реализации: проверка границ и занятости клетки
                if (newX >= 0 && newX < 100 && newY >= 0 && newY < 100)
                {
                    _x = newX;
                    _y = newY;
                }
            }
        }

        /// <summary>
        /// Применяет воздействие радиации на бота.
        /// 
        /// Вызывается из Tick() после основных действий.
        /// Учитывает:
        /// - Урон здоровью
        /// - Вероятность мутации генома
        /// </summary>
        /// <param name="radiationManager">
        /// Менеджер радиоактивных зон.
        /// </param>
        public void ApplyRadiationEffect(RadiationManager radiationManager)
        {
            if (!IsActive)
                return;

            var effect = radiationManager.ApplyRadiationToBot(this);

            // Логирование для отладки
            if (effect.DamageDealt > 0)
            {
                // Console.WriteLine($"Bot {Id} took {effect.DamageDealt:F1} radiation damage");
            }
            if (effect.MutationOccurred)
            {
                // Console.WriteLine($"Bot {Id} mutated due to radiation");
            }
        }

        /// <summary>
        /// Применяет воздействие магнитных аномалий на бота.
        /// 
        /// Вызывается из Tick() при выполнении движения.
        /// Учитывает:
        /// - Дезориентацию направления
        /// - Ошибки в программе движения
        /// </summary>
        /// <param name="anomalies">
        /// Список магнитных аномалий в мире.
        /// </param>
        /// <param name="direction">
        /// Исходное направление движения.
        /// </param>
        /// <returns>
        /// Возможно искажённое направление.
        /// </returns>
        public Genome.MovementDirection ApplyMagneticEffect(
            List<MagneticField> anomalies,
            Genome.MovementDirection direction,
            Random random)
        {
            if (!IsActive)
                return direction;

            foreach (var anomaly in anomalies)
            {
                if (anomaly.IsActive && anomaly.ContainsPoint(X, Y))
                {
                    return anomaly.GetDistortedDirection(direction, X, Y, random);
                }
            }

            return direction;
        }

        #endregion
    }
}