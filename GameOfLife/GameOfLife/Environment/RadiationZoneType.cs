namespace GameOfLife.Environment
{
    /// <summary>
    /// Тип происхождения радиоактивной зоны.
    /// </summary>
    public enum RadiationZoneType
    {
        /// <summary>
        /// Естественная зона.
        /// Создана при генерации мира, не распадается.
        /// </summary>
        Natural,

        /// <summary>
        /// Искусственная зона.
        /// Создана деятельностью ботов (отходы, аварии).
        /// </summary>
        Artificial,

        /// <summary>
        /// Временная зона.
        /// Распадётся со временем (DecayRate > 0).
        /// </summary>
        Temporary
    }
}
