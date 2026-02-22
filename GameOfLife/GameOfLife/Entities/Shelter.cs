using GameOfLife.Core;
using GameOfLife.Environment;
using GameOfLife.Resources;
using System;

namespace GameOfLife.Entities
{
    /// <summary>
    /// Укрытие — структура для защиты ботов от аномалий и погоды.
    /// 
    /// Особенности укрытия:
    /// - Снижает урон от радиоактивных зон на 50-90%
    /// - Защищает от экстремальных температур
    /// - Снижает воздействие магнитных аномалий
    /// - Предоставляет бонус к регенерации здоровья
    /// - Может быть улучшено для увеличения радиуса защиты
    /// 
    /// Стратегия использования:
    /// - Строить в опасных зонах (радиация, аномалии)
    /// - Размещать рядом с ресурсными точками
    /// - Улучшать для увеличения радиуса действия
    /// - Ремонтировать при повреждении
    /// 
    /// Наследует Structure с дополнительной логикой защиты.
    /// </summary>
    public class Shelter : Structure
    {
        #region Приватные поля

        /// <summary>
        /// Текущий уровень защиты от радиации.
        /// </summary>
        private float _radiationProtection;

        /// <summary>
        /// Текущий уровень защиты от температуры.
        /// </summary>
        private float _temperatureProtection;

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Процент защиты от радиации (0.0 - 0.9).
        /// </summary>
        public float RadiationProtection => _radiationProtection;

        /// <summary>
        /// Процент защиты от температурного урона.
        /// </summary>
        public float TemperatureProtection => _temperatureProtection;

        /// <summary>
        /// Бонус к регенерации здоровья для ботов в укрытии.
        /// </summary>
        public float HealthRegenBonus { get; private set; }

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новое укрытие.
        /// </summary>
        public Shelter(int id, int x, int y, Bot owner = null)
            : base(id, x, y, StructureType.Shelter, owner)
        {
            InitializeProtectionLevels();
        }

        #endregion

        #region Инициализация

        /// <summary>
        /// Инициализирует уровни защиты.
        /// </summary>
        private void InitializeProtectionLevels()
        {
            // Базовая защита зависит от уровня структуры
            _radiationProtection = 0.5f + (Level - 1) * 0.1f;
            _temperatureProtection = 0.3f + (Level - 1) * 0.1f;
            HealthRegenBonus = 0.5f * Level;
        }

        #endregion

        #region Переопределённые методы

        /// <summary>
        /// Обновляет укрытие каждый тик.
        /// </summary>
        public override void Tick(SimulationEngine engine)
        {
            base.Tick(engine);

            // Обновляем уровни защиты при улучшении
            if (IsActive)
            {
                _radiationProtection = Math.Min(0.9f, 0.5f + (Level - 1) * 0.1f);
                _temperatureProtection = Math.Min(0.8f, 0.3f + (Level - 1) * 0.1f);
                HealthRegenBonus = 0.5f * Level;
            }
        }

        /// <summary>
        /// Применяет эффекты укрытия к ботам.
        /// </summary>
        protected override void ApplyEffects(SimulationEngine engine)
        {
            base.ApplyEffects(engine);

            // В полной реализации: применение защиты к ботам в радиусе
            // через EnvironmentManager
        }

        /// <summary>
        /// Возвращает бонус к защите.
        /// </summary>
        public override float GetDefenseBonus(Bot bot)
        {
            float baseBonus = base.GetDefenseBonus(bot);

            if (IsBotInRange(bot))
            {
                // Дополнительная защита внутри укрытия
                return baseBonus + 0.2f;
            }

            return baseBonus;
        }

        /// <summary>
        /// Улучшает укрытие.
        /// </summary>
        public override bool Upgrade(ResourcePool upgradeCost)
        {
            bool success = base.Upgrade(upgradeCost);

            if (success)
            {
                InitializeProtectionLevels();
            }

            return success;
        }

        #endregion

        #region Методы защиты

        /// <summary>
        /// Применяет защиту от радиации к боту.
        /// </summary>
        public float ApplyRadiationProtection(Bot bot, float incomingRadiation)
        {
            if (!IsBotInRange(bot) || !IsActive)
                return incomingRadiation;

            return incomingRadiation * (1 - _radiationProtection);
        }

        /// <summary>
        /// Применяет защиту от температуры к боту.
        /// </summary>
        public float ApplyTemperatureProtection(Bot bot, float temperatureDamage)
        {
            if (!IsBotInRange(bot) || !IsActive)
                return temperatureDamage;

            return temperatureDamage * (1 - _temperatureProtection);
        }

        /// <summary>
        /// Применяет регенерацию здоровья к боту.
        /// </summary>
        public void ApplyHealthRegen(Bot bot)
        {
            if (!IsBotInRange(bot) || !IsActive)
                return;

            // В полной реализации: bot.Health += HealthRegenBonus
        }

        #endregion
    }
}