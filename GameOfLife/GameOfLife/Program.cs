using System;
using System.Windows.Forms;
using GameOfLife.Core;

namespace GameOfLife
{
    /// <summary>
    /// Точка входа приложения.
    /// Поддерживает разные режимы запуска через аргументы командной строки.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Парсим аргументы командной строки
            var runMode = ParseRunMode(args);

            switch (runMode)
            {
                case RunMode.UI:
                    // Обычный режим с интерфейсом
                    RunUIMode();
                    break;

                case RunMode.Headless:
                    // Режим без графики (максимальная скорость)
                    RunHeadlessMode(args);
                    break;

                case RunMode.Test:
                    // Тестовый режим
                    RunTestMode();
                    break;

                default:
                    RunUIMode();
                    break;
            }
        }

        /// <summary>
        /// Определяет режим запуска из аргументов.
        /// </summary>
        private static RunMode ParseRunMode(string[] args)
        {
            if (args == null || args.Length == 0)
                return RunMode.UI;

            foreach (var arg in args)
            {
                if (arg.ToLower() == "--headless")
                    return RunMode.Headless;

                if (arg.ToLower() == "--test")
                    return RunMode.Test;
            }

            return RunMode.UI;
        }

        /// <summary>
        /// Запускает режим с пользовательским интерфейсом.
        /// </summary>
        private static void RunUIMode()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new mainForm());
        }

        /// <summary>
        /// Запускает режим без графики.
        /// Используется для быстрых вычислений и экспорта.
        /// </summary>
        private static void RunHeadlessMode(string[] args)
        {
            Console.WriteLine("Запуск в headless режиме (без графики)...");

            // Создаём движок с конфигурацией для headless
            var config = SimulationConfig.CreateHeadless(500, 500);
            var engine = new SimulationEngine(config);

            // Заполняем случайными клетками
            engine.FillRandom(25);

            Console.WriteLine($"Мир: {config.WorldWidth}x{config.WorldHeight}");
            Console.WriteLine($"Начало симуляции...");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Запускаем на 1000 поколений
            int targetGenerations = 1000;
            for (int i = 0; i < targetGenerations; i++)
            {
                engine.Step();

                // Выводим прогресс каждые 100 поколений
                if (i % 100 == 0)
                {
                    Console.WriteLine($"Поколение {i}, Живых клеток: {engine.LiveCellCount}");
                }
            }

            stopwatch.Stop();

            Console.WriteLine($"\nЗавершено!");
            Console.WriteLine($"Время выполнения: {stopwatch.ElapsedMilliseconds} мс");
            Console.WriteLine($"Поколений в секунду: {targetGenerations * 1000.0 / stopwatch.ElapsedMilliseconds:F2}");
            Console.WriteLine($"Среднее время на поколение: {stopwatch.ElapsedMilliseconds / targetGenerations:F2} мс");
        }

        /// <summary>
        /// Запускает тестовый режим для проверки функциональности.
        /// </summary>
        private static void RunTestMode()
        {
            Console.WriteLine("Запуск тестового режима...");

            var config = SimulationConfig.CreateDefault();
            var engine = new SimulationEngine(config);

            // Тест 1: Создание и инициализация
            Console.WriteLine("Тест 1: Инициализация...");
            Console.WriteLine($"Размер мира: {engine.Width}x{engine.Height}");
            Console.WriteLine($"Статус: {engine.Status}");

            // Тест 2: Случайное заполнение
            Console.WriteLine("\nТест 2: Случайное заполнение...");
            engine.FillRandom(30);
            Console.WriteLine($"Живых клеток: {engine.LiveCellCount}");

            // Тест 3: Несколько шагов симуляции
            Console.WriteLine("\nТест 3: Симуляция...");
            for (int i = 0; i < 10; i++)
            {
                engine.Step();
                Console.WriteLine($"Поколение {engine.CurrentGeneration}: {engine.LiveCellCount} живых");
            }

            // Тест 4: Изменение размера
            Console.WriteLine("\nТест 4: Изменение размера...");
            engine.ResizeWorld(50, 50);
            Console.WriteLine($"Новый размер: {engine.Width}x{engine.Height}");

            Console.WriteLine("\nВсе тесты пройдены!");
        }
    }

    /// <summary>
    /// Режимы запуска приложения.
    /// </summary>
    public enum RunMode
    {
        /// <summary>
        /// Режим с пользовательским интерфейсом.
        /// </summary>
        UI,

        /// <summary>
        /// Режим без графики (консоль).
        /// </summary>
        Headless,

        /// <summary>
        /// Тестовый режим.
        /// </summary>
        Test
    }
}