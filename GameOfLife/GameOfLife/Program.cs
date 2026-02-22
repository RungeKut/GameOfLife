using System;
using System.Windows.Forms;
using GameOfLife.Core;
using GameOfLife.Rendering;
using GameOfLife.Export;

namespace GameOfLife
{
    /// <summary>
    /// Точка входа приложения.
    /// 
    /// Этот класс отвечает за:
    /// - Парсинг аргументов командной строки
    /// - Выбор режима запуска (UI, Headless, Export, Test)
    /// - Инициализацию соответствующих компонентов
    /// 
    /// Поддерживаемые режимы:
    /// - UI (по умолчанию): запуск с графическим интерфейсом
    /// - Headless: запуск без графики для максимальной скорости
    /// - Export: экспорт симуляции в GIF файл
    /// - Test: запуск тестов для проверки функциональности
    /// 
    /// Использование из командной строки:
    /// GameOfLife.exe              → UI режим
    /// GameOfLife.exe --headless   → Headless режим
    /// GameOfLife.exe --export     → Export режим
    /// GameOfLife.exe --test       → Test режим
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// 
        /// Метод помечен атрибутом STAThread для корректной работы
        /// Windows Forms (требуется однопоточный apartment).
        /// 
        /// Последовательность выполнения:
        /// 1. Парсинг аргументов командной строки
        /// 2. Выбор режима запуска
        /// 3. Вызов соответствующего метода запуска
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Определяем режим запуска из аргументов
            var runMode = ParseRunMode(args);

            // Запускаем соответствующий режим
            switch (runMode)
            {
                case RunMode.UI:
                    RunUIMode();
                    break;

                case RunMode.Headless:
                    RunHeadlessMode(args);
                    break;

                case RunMode.Export:
                    RunExportMode(args);
                    break;

                case RunMode.Test:
                    RunTestMode();
                    break;

                default:
                    RunUIMode();
                    break;
            }
        }

        /// <summary>
        /// Определяет режим запуска из аргументов командной строки.
        /// 
        /// Поддерживаемые аргументы:
        /// - --headless: запуск без графического интерфейса
        /// - --export: экспорт симуляции в GIF
        /// - --test: запуск тестового режима
        /// 
        /// Если аргументы не предоставлены или не распознаны,
        /// используется режим UI по умолчанию.
        /// 
        /// Сравнение нечувствительно к регистру (--Headless = --headless).
        /// </summary>
        /// <param name="args">Аргументы командной строки.</param>
        /// <returns>Определённый режим запуска.</returns>
        private static RunMode ParseRunMode(string[] args)
        {
            // Пустые аргументы = UI режим
            if (args == null || args.Length == 0)
                return RunMode.UI;

            // Ищем известный аргумент
            foreach (var arg in args)
            {
                if (arg.ToLower() == "--headless")
                    return RunMode.Headless;

                if (arg.ToLower() == "--export")
                    return RunMode.Export;

                if (arg.ToLower() == "--test")
                    return RunMode.Test;
            }

            // По умолчанию UI режим
            return RunMode.UI;
        }

        /// <summary>
        /// Запускает режим с пользовательским интерфейсом.
        /// 
        /// Это стандартный режим работы приложения:
        /// - Отображается форма mainForm
        /// - Пользователь может управлять симуляцией
        /// - Визуализация происходит в реальном времени
        /// 
        /// Метод блокируется до закрытия формы.
        /// </summary>
        private static void RunUIMode()
        {
            // Включаем визуальные стили Windows
            Application.EnableVisualStyles();

            // Настраиваем рендеринг текста
            Application.SetCompatibleTextRenderingDefault(false);

            // Запускаем главную форму
            Application.Run(new mainForm());
        }

        /// <summary>
        /// Запускает режим без графики (headless).
        /// 
        /// Этот режим используется для:
        /// - Быстрых вычислений без накладных расходов на отрисовку
        /// - Запуска на серверах без дисплея
        /// - Тестирования производительности
        /// - Пакетной обработки симуляций
        /// 
        /// Особенности:
        /// - Используется NullRenderer (никакой отрисовки)
        /// - Задержка между тиками = 0 (максимальная скорость)
        /// - Вывод статистики в консоль
        /// 
        /// Производительность:
        /// - В 50-100 раз быстрее UI режима
        /// - Зависит от размера мира и сложности правил
        /// </summary>
        /// <param name="args">Аргументы командной строки (не используются).</param>
        private static void RunHeadlessMode(string[] args)
        {
            Console.WriteLine("Запуск в headless режиме (без графики)...");

            // Создаём конфигурацию для headless
            // Большой мир, без задержек, параллельные вычисления
            var config = SimulationConfig.CreateHeadless(500, 500);
            var engine = new SimulationEngine(config);

            // Устанавливаем NullRenderer (без отрисовки)
            // Это ключевой момент: движок работает но ничего не рисует
            engine.SetRenderer(new NullRenderer());

            // Заполняем мир случайными клетками
            engine.FillRandom(25);

            // Выводим информацию о конфигурации
            Console.WriteLine($"Мир: {config.WorldWidth}x{config.WorldHeight}");
            Console.WriteLine($"Начало симуляции...");

            // Запускаем таймер для измерения производительности
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            int targetGenerations = 1000;

            // Основной цикл симуляции
            for (int i = 0; i < targetGenerations; i++)
            {
                engine.Step();

                // Выводим прогресс каждые 100 поколений
                if (i % 100 == 0)
                {
                    Console.WriteLine($"Поколение {i}, Живых клеток: {engine.LiveCellCount}");
                }
            }

            // Останавливаем таймер
            stopwatch.Stop();

            // Выводим итоговую статистику
            Console.WriteLine($"\nЗавершено!");
            Console.WriteLine($"Время выполнения: {stopwatch.ElapsedMilliseconds} мс");
            Console.WriteLine($"Поколений в секунду: {targetGenerations * 1000.0 / stopwatch.ElapsedMilliseconds:F2}");
        }

        /// <summary>
        /// Запускает режим экспорта в GIF.
        /// 
        /// Этот режим используется для:
        /// - Создания анимаций симуляции
        /// - Генерации контента для презентаций
        /// - Сохранения интересных паттернов
        /// 
        /// Процесс экспорта:
        /// 1. Создаётся движок с конфигурацией для экспорта
        /// 2. Инициализируется GifExporter
        /// 3. Выполняется асинхронный экспорт
        /// 4. Результат сохраняется в output.gif
        /// 
        /// Важно:
        /// - Экспорт может занять несколько секунд
        /// - Размер файла зависит от количества кадров и размера клетки
        /// - Рекомендуется ограничивать до 100-200 кадров
        /// </summary>
        /// <param name="args">Аргументы командной строки (не используются).</param>
        private static void RunExportMode(string[] args)
        {
            Console.WriteLine("Запуск режима экспорта...");

            // Создаём конфигурацию для экспорта
            // Средний размер для баланса качества и скорости
            var config = SimulationConfig.CreateHeadless(200, 200);
            var engine = new SimulationEngine(config);

            // Заполняем случайными клетками
            engine.FillRandom(25);

            // Создаём экспортер
            var exporter = new GifExporter(engine);

            try
            {
                // Запускаем экспорт
                // Параметры:
                // - outputPath: имя выходного файла
                // - generations: количество кадров
                // - fps: скорость воспроизведения
                // - cellSize: размер клетки в пикселях
                exporter.ExportAsync(
                    outputPath: "output.gif",
                    generations: 100,
                    fps: 10,
                    cellSize: 5
                ).Wait();
            }
            catch (Exception ex)
            {
                // Обрабатываем ошибки экспорта
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            finally
            {
                // Освобождаем ресурсы экспортера
                exporter.Dispose();
            }
        }

        /// <summary>
        /// Запускает тестовый режим.
        /// 
        /// Этот режим используется для:
        /// - Проверки базовой функциональности
        /// - Отладки компонентов
        /// - Демонстрации работы API
        /// 
        /// Тесты включают:
        /// 1. Создание и инициализацию движка
        /// 2. Настройку рендерера
        /// 3. Несколько шагов симуляции
        /// 4. Проверку корректности работы
        /// 
        /// Выводит результаты в консоль.
        /// </summary>
        private static void RunTestMode()
        {
            Console.WriteLine("Запуск тестового режима...");

            // Создаём конфигурацию по умолчанию
            var config = SimulationConfig.CreateDefault();
            var engine = new SimulationEngine(config);

            // Тест рендерера
            var renderer = new BitmapRenderer();
            engine.SetRenderer(renderer);

            // Заполняем случайными клетками
            engine.FillRandom(30);

            // Выполняем несколько шагов
            for (int i = 0; i < 10; i++)
            {
                engine.Step();
                Console.WriteLine($"Поколение {engine.CurrentGeneration}: {engine.LiveCellCount} живых");
            }

            // Освобождаем ресурсы
            renderer.Dispose();
            engine.Dispose();

            Console.WriteLine("\nВсе тесты пройдены!");
        }
    }

    /// <summary>
    /// Режимы запуска приложения.
    /// 
    /// Это перечисление определяет доступные режимы работы приложения.
    /// Каждый режим имеет свою точку входа и конфигурацию.
    /// </summary>
    public enum RunMode
    {
        /// <summary>
        /// Режим с пользовательским интерфейсом.
        /// Стандартный режим работы приложения.
        /// </summary>
        UI,

        /// <summary>
        /// Режим без графики (консоль).
        /// Используется для быстрых вычислений.
        /// </summary>
        Headless,

        /// <summary>
        /// Режим экспорта в GIF/PNG.
        /// Используется для сохранения анимаций.
        /// </summary>
        Export,

        /// <summary>
        /// Тестовый режим.
        /// Используется для отладки и проверки.
        /// </summary>
        Test
    }
}