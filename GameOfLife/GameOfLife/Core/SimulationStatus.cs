namespace GameOfLife.Core
{
    /// <summary>
    /// Перечисление состояний симуляции.
    /// Заменяет StatusEngine из старой архитектуры.
    /// </summary>
    public enum SimulationStatus
    {
        /// <summary>
        /// Симуляция остановлена. Можно изменять параметры мира.
        /// </summary>
        Stop,

        /// <summary>
        /// Симуляция запущена и выполняется.
        /// </summary>
        Run,

        /// <summary>
        /// Симуляция на паузе. Состояние сохранено, можно возобновить.
        /// </summary>
        Pause
    }
}