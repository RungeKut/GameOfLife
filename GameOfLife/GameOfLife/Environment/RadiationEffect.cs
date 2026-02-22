namespace GameOfLife.Environment
{
    /// <summary>
    /// Результат воздействия радиации на бота.
    /// </summary>
    public class RadiationEffect
    {
        /// <summary>
        /// Нанесённый урон здоровью.
        /// </summary>
        public float DamageDealt { get; set; }

        /// <summary>
        /// Произошла ли мутация генома.
        /// </summary>
        public bool MutationOccurred { get; set; }

        /// <summary>
        /// Уровень радиации в точке нахождения бота.
        /// </summary>
        public float RadiationLevel { get; set; }
    }
}
