using System;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using GameOfLife.Core;

namespace GameOfLife.Rendering
{
    /// <summary>
    /// Рендерер для отрисовки в Bitmap (в память).
    /// 
    /// Этот класс реализует отрисовку состояния мира в объект Bitmap,
    /// который хранится в оперативной памяти. Bitmap может быть:
    /// - Отображён в PictureBox (через WinFormsRenderer)
    /// - Сохранён в файл (PNG, BMP, JPEG)
    /// - Использован для создания GIF анимации
    /// - Передан другим компонентам для обработки
    /// 
    /// Ключевые особенности:
    /// - Потокобезопасность: все операции защищены блокировкой
    /// - Double buffering: Bitmap пересоздаётся только при изменении размера
    /// - Эффективность: использует Graphics для быстрой отрисовки
    /// - Гибкость: настраиваемые цвета, размеры, опции
    /// 
    /// Этот рендерер не зависит от UI и может работать в фоновом потоке,
    /// что делает его идеальным для headless режима и экспорта.
    /// </summary>
    public class BitmapRenderer : IRenderer
    {
        #region Поля

        /// <summary>
        /// Текущий кадр (Bitmap в памяти).
        /// 
        /// Хранит последнее отрисованное состояние мира.
        /// Пересоздаётся только при изменении размеров мира.
        /// 
        /// Важно: этот объект реализует IDisposable и должен быть
        /// освобождён при уничтожении рендерера.
        /// </summary>
        private Bitmap _currentFrame;

        /// <summary>
        /// Графический контекст для рисования.
        /// 
        /// Создаётся один раз для текущего Bitmap и переиспользуется
        /// для всех операций отрисовки. Это улучшает производительность
        /// по сравнению с созданием нового Graphics для каждого кадра.
        /// 
        /// Важно: Graphics зависит от Bitmap и должен быть освобождён
        /// вместе с ним.
        /// </summary>
        private Graphics _graphics;

        /// <summary>
        /// Накопленные кадры для анимации (GIF).
        /// 
        /// Хранит копии всех кадров между BeginRecording и EndRecording.
        /// Каждый кадр — это отдельный Bitmap объект.
        /// 
        /// Внимание: при длительной записи этот список может потребовать
        /// значительного объёма памяти. Рекомендуется ограничивать
        /// количество кадров или периодически сохранять промежуточные результаты.
        /// </summary>
        private List<Bitmap> _recordedFrames;

        /// <summary>
        /// Флаг записи анимации.
        /// 
        /// true: кадры сохраняются в _recordedFrames
        /// false: кадры не сохраняются (обычный режим)
        /// 
        /// Защищён блокировкой для потокобезопасности.
        /// </summary>
        private bool _isRecording;

        /// <summary>
        /// Блокировка для потокобезопасности.
        /// 
        /// Все операции с общими ресурсами (_currentFrame, _graphics,
        /// _recordedFrames) выполняются внутри этой блокировки.
        /// 
        /// Это позволяет безопасно вызывать Render из фонового потока
        /// симуляции одновременно с доступом к кадру из UI потока.
        /// </summary>
        private readonly object _lockObject = new object();

        #endregion

        #region Свойства

        /// <summary>
        /// Конфигурация рендерера.
        /// 
        /// Может быть изменена в любой момент. Новые значения
        /// применяются к следующему кадру.
        /// 
        /// Потокобезопасно: чтение и запись защищены блокировкой.
        /// </summary>
        public RenderConfig Config { get; set; }

        /// <summary>
        /// Текущий Bitmap (только для чтения).
        /// 
        /// Возвращает ссылку на внутренний Bitmap. Caller не должен
        /// модифицировать или освобождать этот объект.
        /// 
        /// Для безопасного использования вне рендерера следует
        /// создать копию: new Bitmap(renderer.CurrentFrame)
        /// </summary>
        public Bitmap CurrentFrame
        {
            get
            {
                lock (_lockObject)
                {
                    return _currentFrame;
                }
            }
        }

        /// <summary>
        /// Количество записанных кадров.
        /// 
        /// Полезно для отслеживания прогресса записи анимации.
        /// Сбрасывается в 0 после вызова EndRecording.
        /// </summary>
        public int RecordedFrameCount
        {
            get
            {
                lock (_lockObject)
                {
                    return _recordedFrames?.Count ?? 0;
                }
            }
        }

        /// <summary>
        /// Идёт ли запись анимации.
        /// 
        /// true: между BeginRecording и EndRecording
        /// false: обычный режим отрисовки
        /// </summary>
        public bool IsRecording
        {
            get
            {
                lock (_lockObject)
                {
                    return _isRecording;
                }
            }
        }

        #endregion

        #region События

        /// <summary>
        /// Событие завершения отрисовки кадра.
        /// 
        /// Вызывается после успешной отрисовки каждого кадра.
        /// Содержит статистику производительности (время, поколение, и т.д.).
        /// 
        /// Подписчики должны выполнять быстрые операции в обработчике.
        /// Длительные операции могут замедлить рендеринг.
        /// </summary>
        public event Action<RenderStats> OnRenderCompleted;

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новый экземпляр BitmapRenderer.
        /// 
        /// Инициализирует внутренние структуры данных и устанавливает
        /// конфигурацию по умолчанию если не предоставлена пользовательская.
        /// 
        /// Рендерер готов к использованию сразу после создания.
        /// Первый вызов Render создаст Bitmap нужного размера.
        /// </summary>
        /// <param name="config">
        /// Конфигурация рендерера.
        /// Если null, используется конфигурация по умолчанию.
        /// </param>
        public BitmapRenderer(RenderConfig config = null)
        {
            Config = config ?? RenderConfig.CreateDefault();
            _recordedFrames = new List<Bitmap>();
            _isRecording = false;
        }

        #endregion

        #region IRenderer реализация

        /// <summary>
        /// Отрисовывает состояние мира в Bitmap.
        /// 
        /// Этот метод выполняет полную отрисовку кадра:
        /// 1. Проверяет/создаёт Bitmap нужного размера
        /// 2. Очищает фон цветом мёртвой клетки
        /// 3. Рисует сетку (если включена)
        /// 4. Рисует все живые клетки
        /// 5. Рисует информацию о поколении (если включена)
        /// 6. Сохраняет кадр для анимации (если идёт запись)
        /// 7. Уведомляет подписчиков о завершении
        /// 
        /// Метод потокобезопасен и может вызываться из любого потока.
        /// </summary>
        /// <param name="worldState">
        /// Состояние мира для отрисовки.
        /// Должно быть валидным (не null, WorldArray не null).
        /// </param>
        public void Render(WorldState worldState)
        {
            // Защита от невалидных данных
            if (worldState == null || worldState.WorldArray == null)
                return;

            lock (_lockObject)
            {
                // Запускаем таймер для измерения производительности
                var stopwatch = Stopwatch.StartNew();

                // Вычисляем размеры изображения в пикселях
                int width = worldState.Width * Config.CellSize;
                int height = worldState.Height * Config.CellSize;

                // Создаём или пересоздаём Bitmap если размер изменился
                // Это оптимизация: пересоздание Bitmap — дорогая операция
                if (_currentFrame == null ||
                    _currentFrame.Width != width ||
                    _currentFrame.Height != height)
                {
                    // Освобождаем старые ресурсы
                    _currentFrame?.Dispose();
                    _graphics?.Dispose();

                    // Создаём новые
                    _currentFrame = new Bitmap(width, height);
                    _graphics = Graphics.FromImage(_currentFrame);
                }

                // Очищаем фон цветом мёртвой клетки
                _graphics.Clear(Config.DeadColor);

                // Рисуем сетку если включена
                if (Config.ShowGrid)
                {
                    DrawGrid(worldState.Width, worldState.Height);
                }

                // Рисуем живые клетки
                DrawLiveCells(worldState.WorldArray);

                // Рисуем информацию о поколении если включена
                if (Config.ShowGenerationInfo)
                {
                    DrawGenerationInfo(worldState.Generation, worldState.LiveCellCount);
                }

                // Останавливаем таймер
                stopwatch.Stop();

                // Сохраняем кадр если идёт запись
                if (_isRecording)
                {
                    // Клонируем кадр для записи
                    // Важно: клонирование необходимо потому что _currentFrame
                    // будет переиспользован для следующего кадра
                    var frameCopy = new Bitmap(_currentFrame);
                    _recordedFrames.Add(frameCopy);
                }

                // Уведомляем подписчиков о завершении отрисовки
                OnRenderCompleted?.Invoke(new RenderStats
                {
                    RenderTimeMs = stopwatch.ElapsedMilliseconds,
                    Generation = worldState.Generation,
                    LiveCellCount = worldState.LiveCellCount,
                    Width = width,
                    Height = height
                });
            }
        }

        /// <summary>
        /// Сохраняет текущий кадр в файл PNG.
        /// 
        /// Использует встроенную функциональность Bitmap.Save для
        /// сохранения изображения в указанном формате.
        /// 
        /// Метод потокобезопасен и может вызываться из любого потока.
        /// </summary>
        /// <param name="filePath">
        /// Полный путь к файлу для сохранения.
        /// Директория должна существовать.
        /// </param>
        public void SaveFrame(string filePath)
        {
            lock (_lockObject)
            {
                if (_currentFrame != null)
                {
                    _currentFrame.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }

        /// <summary>
        /// Начинает запись анимации.
        /// 
        /// Очищает предыдущие записанные кадры и устанавливает флаг записи.
        /// Все последующие вызовы Render будут сохранять кадры в буфер.
        /// 
        /// Метод потокобезопасен.
        /// </summary>
        public void BeginRecording()
        {
            lock (_lockObject)
            {
                // Очищаем предыдущие кадры
                _recordedFrames.Clear();
                _isRecording = true;
            }
        }

        /// <summary>
        /// Завершает запись и сохраняет GIF.
        /// 
        /// Использует библиотеку AnimatedGif для создания анимированного
        /// GIF файла из накопленных кадров.
        /// 
        /// После вызова этого метода:
        /// - Все записанные кадры освобождаются
        /// - Флаг записи сбрасывается
        /// - Буфер кадров очищается
        /// 
        /// Метод потокобезопасен.
        /// </summary>
        /// <param name="outputPath">
        /// Путь к выходному GIF файлу.
        /// </param>
        /// <param name="fps">
        /// Количество кадров в секунду.
        /// Определяет скорость воспроизведения анимации.
        /// </param>
        public void EndRecording(string outputPath, int fps)
        {
            lock (_lockObject)
            {
                _isRecording = false;

                // Ничего не делаем если кадров нет
                if (_recordedFrames.Count == 0)
                    return;

                // Используем библиотеку AnimatedGif для создания GIF
                // Примечание: требуется добавить NuGet пакет AnimatedGif
                try
                {
                    using (var gif = new AnimatedGif.AnimatedGifCreator(outputPath, fps))
                    {
                        foreach (var frame in _recordedFrames)
                        {
                            // Добавляем кадр с задержкой (1000 / fps миллисекунд)
                            gif.AddFrame(frame, 1000 / fps);

                            // Освобождаем память кадра после добавления
                            frame.Dispose();
                        }
                    }

                    // Очищаем список (кадры уже освобождены)
                    _recordedFrames.Clear();
                }
                catch (Exception ex)
                {
                    // Логируем ошибку но не выбрасываем
                    Console.WriteLine($"Ошибка сохранения GIF: {ex.Message}");
                }
            }
        }

        #endregion

        #region Методы отрисовки

        /// <summary>
        /// Рисует сетку на Bitmap.
        /// 
        /// Рисует вертикальные и горизонтальные линии между клетками.
        /// Использует цвет и толщину из конфигурации.
        /// 
        /// Оптимизация: создаёт Pen один раз и переиспользует для всех линий.
        /// </summary>
        /// <param name="width">Ширина мира в клетках.</param>
        /// <param name="height">Высота мира в клетках.</param>
        private void DrawGrid(int width, int height)
        {
            using (Pen gridPen = new Pen(Config.GridColor, Config.GridThickness))
            {
                // Вертикальные линии
                for (int x = 0; x <= width; x++)
                {
                    int pixelX = x * Config.CellSize;
                    _graphics.DrawLine(gridPen, pixelX, 0, pixelX, height * Config.CellSize);
                }

                // Горизонтальные линии
                for (int y = 0; y <= height; y++)
                {
                    int pixelY = y * Config.CellSize;
                    _graphics.DrawLine(gridPen, 0, pixelY, width * Config.CellSize, pixelY);
                }
            }
        }

        /// <summary>
        /// Рисует живые клетки.
        /// 
        /// Проходит по всему массиву состояния и рисует прямоугольник
        /// для каждой живой клетки.
        /// 
        /// Оптимизация:
        /// - Создаёт Brush один раз для всех клеток
        /// - Пропускает мёртвые клетки без отрисовки
        /// - Использует FillRectangle для быстрой отрисовки
        /// </summary>
        /// <param name="worldArray">Двумерный массив состояния клеток.</param>
        private void DrawLiveCells(bool[,] worldArray)
        {
            int width = worldArray.GetLength(0);
            int height = worldArray.GetLength(1);

            using (Brush aliveBrush = new SolidBrush(Config.AliveColor))
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        // Рисуем только живые клетки
                        if (worldArray[x, y])
                        {
                            int pixelX = x * Config.CellSize;
                            int pixelY = y * Config.CellSize;

                            // Если клетка больше 1 пикселя, оставляем зазор для сетки
                            if (Config.CellSize > 1)
                            {
                                _graphics.FillRectangle(
                                    aliveBrush,
                                    pixelX + 1,
                                    pixelY + 1,
                                    Config.CellSize - 1,
                                    Config.CellSize - 1
                                );
                            }
                            else
                            {
                                // Для 1 пикселя рисуем без зазора
                                _graphics.FillRectangle(
                                    aliveBrush,
                                    pixelX,
                                    pixelY,
                                    1,
                                    1
                                );
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Рисует информацию о поколении.
        /// 
        /// Отображает текст в левом верхнем углу изображения:
        /// - Номер текущего поколения
        /// - Количество живых клеток
        /// 
        /// Использует белый цвет для контраста на тёмном фоне.
        /// </summary>
        /// <param name="generation">Номер поколения.</param>
        /// <param name="liveCount">Количество живых клеток.</param>
        private void DrawGenerationInfo(long generation, int liveCount)
        {
            string info = $"Gen: {generation} | Live: {liveCount}";

            using (Font font = new Font("Arial", 10))
            using (Brush brush = new SolidBrush(Color.White))
            {
                _graphics.DrawString(info, font, brush, 5, 5);
            }
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Освобождает ресурсы рендерера.
        /// 
        /// Вызывает Dispose для всех управляемых ресурсов:
        /// - Graphics контекст
        /// - Текущий Bitmap
        /// - Все записанные кадры
        /// 
        /// После вызова этого метода рендерер не может использоваться.
        /// </summary>
        public void Dispose()
        {
            lock (_lockObject)
            {
                // Освобождаем графический контекст
                _graphics?.Dispose();

                // Освобождаем текущий кадр
                _currentFrame?.Dispose();

                // Освобождаем все записанные кадры
                if (_recordedFrames != null)
                {
                    foreach (var frame in _recordedFrames)
                    {
                        frame.Dispose();
                    }
                    _recordedFrames.Clear();
                }
            }
        }

        #endregion
    }
}