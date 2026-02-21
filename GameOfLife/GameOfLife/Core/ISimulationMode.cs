namespace GameOfLife.Core
{
    /// <summary>
    /// Интерфейс режима симуляции.
    /// Позволяет добавлять новые типы симуляций без изменения ядра.
    /// 
    /// Примеры режимов:
    /// - ConwayMode: классическая игра в жизнь
    /// - EcosystemMode: симуляция с ботами и ресурсами
    /// - CustomMode: пользовательские правила
    /// </summary>
    public interface ISimulationMode
    {
        /// <summary>
        /// Название режима симуляции.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Описание режима.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Инициализирует режим с заданной конфигурацией.
        /// Вызывается при создании или смене режима.
        /// </summary>
        /// <param name="config">Конфигурация симуляции.</param>
        void Initialize(SimulationConfig config);

        /// <summary>
        /// Выполняет один шаг симуляции.
        /// Вызывается каждый тик движком.
        /// </summary>
        /// <param name="worldState">Текущее состояние мира.</param>
        /// <param name="generation">Номер поколения.</param>
        /// <returns>Новое состояние мира после применения правил.</returns>
        bool[,] Step(bool[,] worldState, long generation);

        /// <summary>
        /// Применяет правила к одной клетке.
        /// Может быть переопределено для разных режимов.
        /// </summary>
        /// <param name="isAlive">Текущее состояние клетки.</param>
        /// <param name="neighbourCount">Количество живых соседей.</param>
        /// <param name="generation">Номер поколения.</param>
        /// <returns>Новое состояние клетки.</returns>
        bool ApplyRules(bool isAlive, int neighbourCount, long generation);
    }
}