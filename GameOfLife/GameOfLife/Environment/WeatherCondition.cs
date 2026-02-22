namespace GameOfLife.Environment
{
    /// <summary>
    /// Типы погодных условий.
    /// </summary>
    public enum WeatherCondition
    {
        /// <summary>
        /// Ясная погода.
        /// Нормальные условия, нет специальных эффектов.
        /// </summary>
        Clear,

        /// <summary>
        /// Дождь.
        /// Увеличивает регенерацию ресурсов, повышает влажность.
        /// </summary>
        Rain,

        /// <summary>
        /// Буря.
        /// Сильный ветер, опасность для ботов, плохая видимость.
        /// </summary>
        Storm,

        /// <summary>
        /// Туман.
        /// Ограниченная видимость для ботов, слабый ветер.
        /// </summary>
        Fog
    }
}
