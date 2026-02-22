using GameOfLife.Core;
using GameOfLife.Resources;
using System;

namespace GameOfLife.Entities
{
    /// <summary>
    /// Базовый класс для всех структур в мире.
    /// 
    /// Структура — это стационарный объект который:
    /// - Занимает одну или несколько клеток мира
    /// - Имеет прочность и может быть разрушен
    /// - Предоставляет бонусы находящимся рядом ботам
    /// - Требует ресурсов для строительства
    /// - Может иметь владельца (бота который построил)
    /// 
    /// Архитектурное разделение:
    /// - Structure: базовый класс с общей логикой
    /// - Shelter: укрытие для защиты от аномалий
    /// - Storage: хранилище для ресурсов
    /// - Base: комплексная структура с множественными функциями
    /// - Trap: ловушка для нанесения урона врагам
    /// 
    /// Жизненный цикл структуры:
    /// 1. Планирование: бот выбирает место и тип структуры
    /// 2. Строительство: тратятся ресурсы, структура создаётся
    /// 3. Эксплуатация: структура предоставляет бонусы
    /// 4. Разрушение: структура теряет прочность и удаляется
    /// </summary>
    public class Structure : IEntity
    {
        #region Приватные поля

        /// <summary>
        /// Уникальный идентификатор структуры.
        /// </summary>
        private readonly int _id;

        /// <summary>
        /// Владелец структуры (бот который построил).
        /// </summary>
        private Bot _owner;

        /// <summary>
        /// Текущая прочность структуры.
        /// </summary>
        private float _currentHealth;

        /// <summary>
        /// Возраст структуры в тиках.
        /// </summary>
        private int _age;

        #endregion

        #region Публичные свойства (реализация IEntity)

        /// <summary>
        /// Уникальный идентификатор структуры.
        /// </summary>
        public int Id => _id;

        /// <summary>
        /// Горизонтальная координата структуры в мире.
        /// </summary>
        public int X { get; protected set; }

        /// <summary>
        /// Вертикальная координата структуры в мире.
        /// </summary>
        public int Y { get; protected set; }

        /// <summary>
        /// Индикатор активности структуры.
        /// false если структура разрушена.
        /// </summary>
        public bool IsActive { get; protected set; }

        #endregion

        #region Свойства структуры

        /// <summary>
        /// Тип структуры.
        /// </summary>
        public StructureType Type { get; protected set; }

        /// <summary>
        /// Максимальная прочность структуры.
        /// </summary>
        public float MaxHealth { get; protected set; }

        /// <summary>
        /// Текущая прочность структуры.
        /// </summary>
        public float CurrentHealth
        {
            get => _currentHealth;
            private set
            {
                _currentHealth = Math.Max(0, Math.Min(MaxHealth, value));
                if (_currentHealth <= 0)
                {
                    Destroy();
                }
            }
        }

        /// <summary>
        /// Владелец структуры.
        /// </summary>
        public Bot Owner
        {
            get => _owner;
            protected set => _owner = value;
        }

        /// <summary>
        /// Возраст структуры в тиках.
        /// </summary>
        public int Age => _age;

        /// <summary>
        /// Стоимость строительства в ресурсах.
        /// </summary>
        public ResourcePool BuildCost { get; protected set; }

        /// <summary>
        /// Стоимость ремонта в ресурсах.
        /// </summary>
        public ResourcePool RepairCost { get; protected set; }

        /// <summary>
        /// Радиус действия бонусов структуры.
        /// </summary>
        public int EffectRadius { get; protected set; }

        /// <summary>
        /// Уровень структуры (для улучшаемых структур).
        /// </summary>
        public int Level { get; protected set; } = 1;

        /// <summary>
        /// Максимальный уровень структуры.
        /// </summary>
        public int MaxLevel { get; protected set; } = 5;

        #endregion

        #region События

        /// <summary>
        /// Событие повреждения структуры.
        /// </summary>
        public event Action<Structure, float> OnDamaged;

        /// <summary>
        /// Событие разрушения структуры.
        /// </summary>
        public event Action<Structure> OnDestroyed;

        /// <summary>
        /// Событие улучшения структуры.
        /// </summary>
        public event Action<Structure, int> OnUpgraded;

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новую структуру.
        /// </summary>
        /// <param name="id">Уникальный идентификатор.</param>
        /// <param name="x">Координата X.</param>
        /// <param name="y">Координата Y.</param>
        /// <param name="type">Тип структуры.</param>
        /// <param name="owner">Владелец структуры.</param>
        public Structure(int id, int x, int y, StructureType type, Bot owner = null)
        {
            _id = id;
            X = x;
            Y = y;
            Type = type;
            _owner = owner;
            IsActive = true;
            _age = 0;

            // Инициализация параметров по типу структуры
            InitializeByType();

            _currentHealth = MaxHealth;
        }

        #endregion

        #region Инициализация

        /// <summary>
        /// Инициализирует параметры структуры по типу.
        /// </summary>
        private void InitializeByType()
        {
            switch (Type)
            {
                case StructureType.Shelter:
                    MaxHealth = 100;
                    EffectRadius = 3;
                    BuildCost = CreateCost(ResourceType.Metal, 10);
                    RepairCost = CreateCost(ResourceType.Metal, 5);
                    MaxLevel = 3;
                    break;

                case StructureType.Storage:
                    MaxHealth = 50;
                    EffectRadius = 1;
                    BuildCost = CreateCost(ResourceType.Metal, 15);
                    RepairCost = CreateCost(ResourceType.Metal, 8);
                    MaxLevel = 5;
                    break;

                case StructureType.Base:
                    MaxHealth = 500;
                    EffectRadius = 10;
                    BuildCost = CreateCost(ResourceType.Metal, 50);
                    RepairCost = CreateCost(ResourceType.Metal, 25);
                    MaxLevel = 10;
                    break;

                case StructureType.Trap:
                    MaxHealth = 20;
                    EffectRadius = 1;
                    BuildCost = CreateCost(ResourceType.Metal, 5);
                    RepairCost = CreateCost(ResourceType.Metal, 2);
                    MaxLevel = 3;
                    break;

                case StructureType.Wall:
                    MaxHealth = 200;
                    EffectRadius = 0;
                    BuildCost = CreateCost(ResourceType.Metal, 8);
                    RepairCost = CreateCost(ResourceType.Metal, 4);
                    MaxLevel = 5;
                    break;

                default:
                    MaxHealth = 100;
                    EffectRadius = 1;
                    BuildCost = CreateCost(ResourceType.Metal, 10);
                    RepairCost = CreateCost(ResourceType.Metal, 5);
                    MaxLevel = 3;
                    break;
            }
        }

        /// <summary>
        /// Создаёт стоимость строительства.
        /// </summary>
        private ResourcePool CreateCost(ResourceType type, float amount)
        {
            var pool = new ResourcePool(-1);
            pool.Add(type, amount);
            return pool;
        }

        #endregion

        #region Основной метод обновления

        /// <summary>
        /// Выполняет один шаг логики структуры.
        /// </summary>
        public virtual void Tick(SimulationEngine engine)
        {
            if (!IsActive)
                return;

            _age++;

            // Естественный износ структуры
            ApplyNaturalDecay();

            // Применение эффектов к ботам поблизости
            ApplyEffects(engine);
        }

        #endregion

        #region Повреждение и ремонт

        /// <summary>
        /// Наносит урон структуре.
        /// </summary>
        /// <param name="damage">Количество урона.</param>
        /// <returns>Фактический нанесённый урон.</returns>
        public virtual float TakeDamage(float damage)
        {
            if (!IsActive)
                return 0;

            // Учёт уровня структуры для снижения урона
            float mitigatedDamage = damage * (1 - (Level - 1) * 0.1f);
            mitigatedDamage = Math.Max(1, mitigatedDamage);

            CurrentHealth -= mitigatedDamage;
            OnDamaged?.Invoke(this, mitigatedDamage);

            return mitigatedDamage;
        }

        /// <summary>
        /// Ремонтирует структуру.
        /// </summary>
        /// <param name="amount">Количество восстановления.</param>
        /// <returns>Фактически восстановленное здоровье.</returns>
        public virtual float Repair(float amount)
        {
            if (!IsActive)
                return 0;

            float oldHealth = CurrentHealth;
            CurrentHealth += amount;
            float repaired = CurrentHealth - oldHealth;

            return repaired;
        }

        /// <summary>
        /// Применяет естественный износ структуры.
        /// </summary>
        private void ApplyNaturalDecay()
        {
            // Структуры старше 1000 тиков начинают изнашиваться
            if (_age > 1000)
            {
                float decayRate = 0.01f * (1 + (_age - 1000) / 1000f);
                CurrentHealth -= decayRate;
            }
        }

        /// <summary>
        /// Уничтожает структуру.
        /// </summary>
        protected virtual void Destroy()
        {
            IsActive = false;
            OnDestroyed?.Invoke(this);

            // Оставляем ресурсы при разрушении
            DropResources();
        }

        /// <summary>
        /// Оставляет ресурсы при разрушении структуры.
        /// </summary>
        protected virtual void DropResources()
        {
            // В полной реализации: передача ресурсов в ResourceManager
            // Возврат части стоимости строительства владельцу
        }

        #endregion

        #region Эффекты структуры

        /// <summary>
        /// Применяет эффекты структуры к ботам поблизости.
        /// </summary>
        protected virtual void ApplyEffects(SimulationEngine engine)
        {
            // Переопределяется в подклассах
        }

        /// <summary>
        /// Проверяет находится ли бот в радиусе действия структуры.
        /// </summary>
        public bool IsBotInRange(Bot bot)
        {
            if (EffectRadius <= 0)
                return false;

            int dx = Math.Abs(bot.X - X);
            int dy = Math.Abs(bot.Y - Y);
            int distance = dx + dy;

            return distance <= EffectRadius;
        }

        /// <summary>
        /// Возвращает бонус к защите для ботов в радиусе.
        /// </summary>
        public virtual float GetDefenseBonus(Bot bot)
        {
            if (!IsBotInRange(bot))
                return 0;

            return 0.1f * Level; // 10% за уровень
        }

        /// <summary>
        /// Возвращает бонус к регенерации энергии.
        /// </summary>
        public virtual float GetEnergyRegenBonus(Bot bot)
        {
            if (!IsBotInRange(bot))
                return 0;

            return 0.5f * Level; // 0.5 энергии за уровень
        }

        #endregion

        #region Улучшение структуры

        /// <summary>
        /// Проверяет можно ли улучшить структуру.
        /// </summary>
        public bool CanUpgrade()
        {
            return IsActive && Level < MaxLevel;
        }

        /// <summary>
        /// Улучшает структуру до следующего уровня.
        /// </summary>
        /// <param name="upgradeCost">Стоимость улучшения.</param>
        /// <returns>True если улучшение успешно.</returns>
        public virtual bool Upgrade(ResourcePool upgradeCost)
        {
            if (!CanUpgrade())
                return false;

            // Проверка наличия ресурсов
            if (upgradeCost != null && upgradeCost.TotalWeight > 0)
            {
                // В полной реализации: проверка и списание ресурсов
            }

            Level++;

            // Увеличение параметров с уровнем
            MaxHealth *= 1.2f;
            CurrentHealth = MaxHealth;
            EffectRadius += 1;

            OnUpgraded?.Invoke(this, Level);
            return true;
        }

        /// <summary>
        /// Возвращает стоимость улучшения для следующего уровня.
        /// </summary>
        public ResourcePool GetUpgradeCost()
        {
            var cost = new ResourcePool(-1);
            float multiplier = 1.5f * Level;
            cost.Add(ResourceType.Metal, 10 * multiplier);
            return cost;
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Возвращает процент текущего здоровья.
        /// </summary>
        public float GetHealthPercentage()
        {
            if (MaxHealth <= 0)
                return 0;
            return CurrentHealth / MaxHealth * 100;
        }

        /// <summary>
        /// Проверяет принадлежит ли структура боту.
        /// </summary>
        public bool IsOwnedBy(Bot bot)
        {
            return _owner == bot;
        }

        /// <summary>
        /// Передаёт структуру другому владельцу.
        /// </summary>
        public void TransferOwnership(Bot newOwner)
        {
            _owner = newOwner;
        }

        /// <summary>
        /// Возвращает строковое представление структуры.
        /// </summary>
        public override string ToString()
        {
            return $"{Type} (Lvl:{Level}) at ({X},{Y}) HP:{CurrentHealth:F0}/{MaxHealth:F0}";
        }

        #endregion
    }

    /// <summary>
    /// Типы структур в мире.
    /// </summary>
    public enum StructureType
    {
        /// <summary>
        /// Укрытие: защита от аномалий и погоды.
        /// </summary>
        Shelter,

        /// <summary>
        /// Хранилище: дополнительное место для ресурсов.
        /// </summary>
        Storage,

        /// <summary>
        /// База: комплексная структура с множественными функциями.
        /// </summary>
        Base,

        /// <summary>
        /// Ловушка: наносит урон врагам.
        /// </summary>
        Trap,

        /// <summary>
        /// Стена: блокирует перемещение.
        /// </summary>
        Wall,

        /// <summary>
        /// Генератор: производит энергию.
        /// </summary>
        Generator,

        /// <summary>
        /// Ферма: производит пищу.
        /// </summary>
        Farm,

        /// <summary>
        /// Лаборатория: ускоряет исследования.
        /// </summary>
        Laboratory
    }
}