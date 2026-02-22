using System;

namespace GameOfLife.Entities
{
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
}
