using System;
using System.Threading;
using System.Threading.Tasks;
using GameOfLife.Core;
using GameOfLife.Rendering;

namespace GameOfLife.Export
{
    /// <summary>
    /// Экспортер для сохранения симуляции в GIF.
    /// 
    /// Этот класс предоставляет высокоуровневый API для экспорта
    /// симуляции в анимированный GIF файл.
    /// 
    /// Особенности:
    /// - Асинхронный экспорт: не блокирует основной поток
    /// - Поддержка отмены: можно прервать экспорт через CancellationToken
    /// - Прогресс: callback для отображения прогресса пользователю
    /// - Headless режим: работает без UI для максимальной скорости
    /// 
    /// Типичное использование:
    /// var engine = new SimulationEngine(config);
    /// var exporter = new GifExporter(engine);
    /// await exporter.ExportAsync("output.gif", 100, fps: 15);
    /// 
    /// Важно:
    /// - Экспортер захватывает кадры из текущего состояния двигателя
    /// - Длинный экспорт может потребовать значительной памяти
    /// - Рекомендуется ограничивать количество кадров до 1000
    /// </summary>
    public class GifExporter : IDisposable
    {
        #region Поля

        /// <summary>
        /// Движок симуляции.
        /// 
        /// Используется для выполнения шагов симуляции и получения
        /// состояния мира для каждого кадра.
        /// 
        /// Движок должен быть инициализирован перед использованием
        /// экспортера (размер мира установлен, начальное состояние задано).
        /// </summary>
        private SimulationEngine _engine;

        /// <summary>
        /// Рендерер для записи кадров.
        /// 
        /// BitmapRenderer используется для отрисовки каждого кадра
        /// в память перед добавлением в GIF.
        /// 
        /// Рендерер создаётся один раз и переиспользуется для всех кадров.
        /// </summary>
        private BitmapRenderer _renderer;

        /// <summary>
        /// Токен отмены.
        /// 
        /// Используется для поддержки отмены асинхронной операции.
        /// Позволяет пользователю прервать экспорт если он занимает
        /// слишком много времени.
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новый экземпляр GifExporter.
        /// 
        /// Инициализирует внутренний BitmapRenderer и сохраняет
        /// ссылку на движок симуляции.
        /// 
        /// Движок должен быть валидным и инициализированным.
        /// </summary>
        /// <param name="engine">
        /// Движок симуляции для экспорта.
        /// Не может быть null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Выбрасывается если engine равен null.
        /// </exception>
        public GifExporter(SimulationEngine engine)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _renderer = new BitmapRenderer();
        }

        #endregion

        #region Методы экспорта

        /// <summary>
        /// Экспортирует симуляцию в GIF файл.
        /// 
        /// Этот метод выполняет полный цикл экспорта:
        /// 1. Настраивает рендерер с указанными параметрами
        /// 2. Начинает запись кадров
        /// 3. Для каждого поколения:
        ///    - Выполняет шаг симуляции
        ///    - Отрисовывает кадр
        ///    - Сохраняет кадр в буфер
        ///    - Вызывает callback прогресса
        /// 4. Завершает запись и сохраняет GIF файл
        /// 
        /// Метод асинхронный и может быть отменён через CancellationToken.
        /// </summary>
        /// <param name="outputPath">
        /// Путь к выходному GIF файлу.
        /// Директория должна существовать.
        /// </param>
        /// <param name="generations">
        /// Количество поколений для записи.
        /// Рекомендуемые значения: 50-500.
        /// Больше кадров = больше файл и дольше экспорт.
        /// </param>
        /// <param name="fps">
        /// Количество кадров в секунду.
        /// Рекомендуемые значения: 10-30.
        /// </param>
        /// <param name="cellSize">
        /// Размер клетки в пикселях.
        /// Влияет на размер изображения и файла.
        /// </param>
        /// <param name="progressCallback">
        /// Callback для отображения прогресса.
        /// Вызывается после каждого кадра с параметрами:
        /// - current: текущий кадр (1-based)
        /// - total: всего кадров
        /// </param>
        /// <returns>
        /// Task представляющий асинхронную операцию.
        /// </returns>
        public async Task ExportAsync(
            string outputPath,
            int generations,
            int fps = 10,
            int cellSize = 4,
            Action<int, int> progressCallback = null)
        {
            // Создаём токен отмены
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            // Настраиваем размер клетки
            _renderer.Config.CellSize = cellSize;

            // Начинаем запись
            _renderer.BeginRecording();

            Console.WriteLine($"Начало экспорта: {generations} поколений, {fps} FPS");

            try
            {
                // Основной цикл экспорта
                for (int i = 0; i < generations; i++)
                {
                    // Проверяем отмену
                    if (token.IsCancellationRequested)
                        break;

                    // Шаг симуляции
                    _engine.Step();

                    // Рендер кадра
                    _renderer.Render(_engine.GetWorldState());

                    // Вызываем callback прогресса
                    progressCallback?.Invoke(i + 1, generations);

                    // Выводим прогресс в консоль каждые 100 кадров
                    if ((i + 1) % 100 == 0)
                    {
                        Console.WriteLine($"Поколение {i + 1}/{generations}");
                    }

                    // Небольшая задержка чтобы не блокировать UI полностью
                    // Это позволяет UI оставаться отзывчивым во время экспорта
                    if (i % 10 == 0)
                    {
                        await Task.Delay(1, token);
                    }
                }

                // Завершаем запись и сохраняем файл
                _renderer.EndRecording(outputPath, fps);

                Console.WriteLine($"Экспорт завершён: {outputPath}");
                Console.WriteLine($"Всего кадров: {_renderer.RecordedFrameCount}");
            }
            catch (OperationCanceledException)
            {
                // Ожидаемая отмена
                Console.WriteLine("Экспорт отменён");
            }
            catch (Exception ex)
            {
                // Неожиданная ошибка
                Console.WriteLine($"Ошибка экспорта: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Отменяет экспорт.
        /// 
        /// Вызывает отмену CancellationToken, что приводит к
        /// прерыванию цикла экспорта в ExportAsync.
        /// 
        /// Метод безопасен для вызова даже если экспорт не запущен.
        /// </summary>
        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Освобождает ресурсы экспортера.
        /// 
        /// Выполняет следующие операции:
        /// 1. Освобождает CancellationTokenSource
        /// 2. Освобождает BitmapRenderer
        /// 
        /// После вызова этого метода экспортер не может использоваться.
        /// </summary>
        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
            _renderer?.Dispose();
        }

        #endregion
    }
}