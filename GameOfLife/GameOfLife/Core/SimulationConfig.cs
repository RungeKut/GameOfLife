using System;

namespace GameOfLife.Core
{
    /// <summary>
    /// Конфигурация симуляции.
    /// Содержит все настраиваемые параметры для движка.
    /// </summary>
    public class SimulationConfig
    {
        /// <summary>
        /// Ширина мира в клетках.
        /// </summary>
        public int WorldWidth { get; set; } = 100;

        /// <summary>
        /// Высота мира в клетках.
        /// </summary>
        public int WorldHeight { get; set; } = 100;

        /// <summary>
        /// Задержка между тиками симуляции в миллисекундах.
        /// Контролирует скорость выполнения.
        /// </summary>
        public int TickDelayMs { get; set; } = 100;

        /// <summary>
        /// Плотность случайного заполнения при инициализации (0-100).
        /// </summary>
        public int InitialDensity { get; set; } = 25;

        /// <summary>
        /// Максимальное количество поколений для автоматической остановки.
        /// -1 означает бесконечный режим.
        /// </summary>
        public int MaxGenerations { get; set; } = -1;

        /// <summary>
        /// Включить ли зацикливание границ мира (тороидальная топология).
        /// </summary>
        public bool WrapAround { get; set; } = true;

        /// <summary>
        /// Включить ли параллельные вычисления.
        /// </summary>
        public bool EnableParallelProcessing { get; set; } = true;

        /// <summary>
        /// Количество потоков для параллельной обработки.
        /// 0 означает использование всех доступных процессоров.
        /// </summary>
        public int ThreadCount { get; set; } = 0;

        /// <summary>
        /// Сид для генератора случайных чисел.
        /// 
        /// Используется для воспроизводимости симуляций:
        /// - Один и тот же сид = одинаковая последовательность случайных событий
        /// - Полезно для отладки, тестов и сравнения стратегий
        /// - Значение -1 означает использование случайного сида
        /// 
        /// Диапазон: любое целое число (включая отрицательные)
        /// </summary>
        public int Seed { get; set; } = -1;

        /// <summary>
        /// Получает эффективный сид для использования.
        /// Если Seed = -1, генерируется случайное значение.
        /// </summary>
        /// <returns>Целое число для инициализации Random.</returns>
        public int GetEffectiveSeed()
        {
            return Seed >= 0 ? Seed : System.Environment.TickCount;
        }

        /// <summary>
        /// Создаёт конфигурацию по умолчанию.
        /// </summary>
        public static SimulationConfig CreateDefault()
        {
            return new SimulationConfig
            {
                WorldWidth = 100,
                WorldHeight = 100,
                TickDelayMs = 100,
                InitialDensity = 25,
                MaxGenerations = -1,
                WrapAround = true,
                EnableParallelProcessing = true,
                ThreadCount = 0,
                Seed = -1
            };
        }

        /// <summary>
        /// Создаёт конфигурацию для headless режима (без графики).
        /// Максимальная производительность.
        /// </summary>
        public static SimulationConfig CreateHeadless(int width, int height)
        {
            return new SimulationConfig
            {
                WorldWidth = width,
                WorldHeight = height,
                TickDelayMs = 0, // Без задержки
                InitialDensity = 25,
                MaxGenerations = -1,
                WrapAround = true,
                EnableParallelProcessing = true,
                ThreadCount = 0,
                Seed = -1
            };
        }
    }
}