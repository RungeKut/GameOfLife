namespace GameOfLife.Entities
{
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
