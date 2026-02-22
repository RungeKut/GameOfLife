using System;
using System.Threading;
using System.Threading.Tasks;
using GameOfLife.Rendering;

namespace GameOfLife.Core
{
    /// <summary>
    /// Главный движок симуляции.
    /// Управляет жизненным циклом симуляции, обновлением состояния мира
    /// и уведомлением подписчиков об изменениях.
    /// 
    /// Этот класс полностью независим от UI и графики.
    /// </summary>
    public class SimulationEngine : IDisposable
    {
        #region События для подписчиков

        /// <summary>
        /// Событие вызывается после каждого обновления состояния мира.
        /// Передаёт текущее состояние мира для рендеринга.
        /// </summary>
        public event Action<WorldState> OnWorldUpdated;

        /// <summary>
        /// Событие вызывается после завершения каждого тика.
        /// Передаёт номер текущего поколения.
        /// </summary>
        public event Action<long> OnTickCompleted;

        /// <summary>
        /// Событие вызывается при изменении статуса симуляции.
        /// </summary>
        public event Action<SimulationStatus> OnStatusChanged;

        #endregion

        #region Приватные поля

        /// <summary>
        /// Конфигурация симуляции.
        /// </summary>
        private SimulationConfig _config;

        /// <summary>
        /// Карта мира (состояние клеток).
        /// </summary>
        private bool[,] _worldState;

        /// <summary>
        /// Буфер для следующего состояния (double buffering).
        /// Позволяет избежать аллокаций в цикле.
        /// </summary>
        private bool[,] _nextWorldState;

        /// <summary>
        /// Приватное поле для подсчёта живых клеток.
        /// Используется для атомарных операций Interlocked.
        /// </summary>
        private int _liveCellCount;

        /// <summary>
        /// Токен отмены для асинхронного выполнения.
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Задача асинхронного выполнения.
        /// </summary>
        private Task _runTask;

        /// <summary>
        /// Блокировка для потокобезопасного доступа.
        /// </summary>
        private readonly object _lockObject = new object();

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Текущий статус симуляции.
        /// </summary>
        public SimulationStatus Status { get; private set; }

        /// <summary>
        /// Номер текущего поколения.
        /// </summary>
        public long CurrentGeneration { get; private set; }

        /// <summary>
        /// Ширина мира в клетках.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Высота мира в клетках.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Общее количество живых клеток (только для чтения извне).
        /// </summary>
        public int LiveCellCount => _liveCellCount;

        /// <summary>
        /// Конфигурация симуляции (только для чтения).
        /// </summary>
        public SimulationConfig Config => _config;

        /// <summary>
        /// Индикатор работы движка.
        /// </summary>
        public bool IsRunning => Status == SimulationStatus.Run;

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новый экземпляр движка симуляции.
        /// </summary>
        /// <param name="config">Конфигурация симуляции. Если null, используется конфигурация по умолчанию.</param>
        public SimulationEngine(SimulationConfig config = null)
        {
            _config = config ?? SimulationConfig.CreateDefault();
            Status = SimulationStatus.Stop;
            CurrentGeneration = 0;
            
            InitializeWorld();
        }

        #endregion

        #region Инициализация

        /// <summary>
        /// Инициализирует мир с размерами из конфигурации.
        /// </summary>
        private void InitializeWorld()
        {
            Width = _config.WorldWidth;
            Height = _config.WorldHeight;

            // Выделяем память для текущего и следующего состояния
            _worldState = new bool[Width, Height];
            _nextWorldState = new bool[Width, Height];

            _liveCellCount = 0;
            CurrentGeneration = 0;
        }

        /// <summary>
        /// Изменяет размер мира.
        /// Вызывает пересоздание массивов состояния.
        /// </summary>
        /// <param name="width">Новая ширина мира.</param>
        /// <param name="height">Новая высота мира.</param>
        public void ResizeWorld(int width, int height)
        {
            lock (_lockObject)
            {
                if (width <= 0 || height <= 0)
                    throw new ArgumentException("Размеры мира должны быть положительными");

                Width = width;
                Height = height;
                _config.WorldWidth = width;
                _config.WorldHeight = height;

                // Пересоздаём массивы
                _worldState = new bool[Width, Height];
                _nextWorldState = new bool[Width, Height];

                _liveCellCount = 0;
                CurrentGeneration = 0;

                // Уведомляем подписчиков об изменении
                OnWorldUpdated?.Invoke(GetWorldState());
            }
        }

        /// <summary>
        /// Заполняет мир случайными клетками.
        /// </summary>
        /// <param name="density">Плотность заполнения (0-100 процентов).</param>
        public void FillRandom(int density)
        {
            lock (_lockObject)
            {
                density = Math.Max(0, Math.Min(100, density));
                
                var random = new Random();
                _liveCellCount = 0;

                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        // Шанс рождения клетки
                        bool isAlive = random.Next(100) < density;
                        _worldState[x, y] = isAlive;
                        
                        if (isAlive)
                            _liveCellCount++;
                    }
                }

                CurrentGeneration = 0;
                OnWorldUpdated?.Invoke(GetWorldState());
            }
        }

        /// <summary>
        /// Очищает мир (убивает все клетки).
        /// </summary>
        public void ClearWorld()
        {
            lock (_lockObject)
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        _worldState[x, y] = false;
                    }
                }

                _liveCellCount = 0;
                CurrentGeneration = 0;
                OnWorldUpdated?.Invoke(GetWorldState());
            }
        }

        #endregion

        #region Управление симуляцией

        /// <summary>
        /// Запускает симуляцию.
        /// Если уже запущена, ничего не делает.
        /// </summary>
        public void Start()
        {
            if (Status == SimulationStatus.Run)
                return;

            Status = SimulationStatus.Run;
            OnStatusChanged?.Invoke(Status);

            // Запускаем асинхронный цикл
            _cancellationTokenSource = new CancellationTokenSource();
            _runTask = RunAsync(_cancellationTokenSource.Token);
        }

        /// <summary>
        /// Ставит симуляцию на паузу.
        /// </summary>
        public void Pause()
        {
            if (Status != SimulationStatus.Run)
                return;

            Status = SimulationStatus.Pause;
            OnStatusChanged?.Invoke(Status);

            // Отменяем текущий цикл
            _cancellationTokenSource?.Cancel();
        }

        /// <summary>
        /// Возобновляет симуляцию после паузы.
        /// </summary>
        public void Resume()
        {
            if (Status != SimulationStatus.Pause)
                return;

            Status = SimulationStatus.Run;
            OnStatusChanged?.Invoke(Status);

            // Запускаем новый цикл
            _cancellationTokenSource = new CancellationTokenSource();
            _runTask = RunAsync(_cancellationTokenSource.Token);
        }

        /// <summary>
        /// Останавливает симуляцию полностью.
        /// Сбрасывает поколение в 0.
        /// </summary>
        public void Stop()
        {
            if (Status == SimulationStatus.Stop)
                return;

            Status = SimulationStatus.Stop;
            OnStatusChanged?.Invoke(Status);

            // Отменяем цикл
            _cancellationTokenSource?.Cancel();
            
            // Сбрасываем поколение
            CurrentGeneration = 0;
            OnWorldUpdated?.Invoke(GetWorldState());
        }

        /// <summary>
        /// Выполняет один шаг симуляции.
        /// Используется в пошаговом режиме или когда симуляция на паузе.
        /// </summary>
        public void Step()
        {
            lock (_lockObject)
            {
                ComputeNextGeneration();
                CurrentGeneration++;
                OnWorldUpdated?.Invoke(GetWorldState());
                OnTickCompleted?.Invoke(CurrentGeneration);
            }
        }

        /// <summary>
        /// Асинхронный цикл симуляции.
        /// Выполняется в фоновом потоке.
        /// </summary>
        /// <param name="token">Токен отмены.</param>
        private async Task RunAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested && Status == SimulationStatus.Run)
                {
                    // Проверяем лимит поколений
                    if (_config.MaxGenerations > 0 && CurrentGeneration >= _config.MaxGenerations)
                    {
                        Stop();
                        break;
                    }

                    // Выполняем один шаг
                    Step();

                    // Задержка между тиками (если не headless режим)
                    if (_config.TickDelayMs > 0)
                    {
                        await Task.Delay(_config.TickDelayMs, token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ожиданная отмена
            }
            catch (Exception ex)
            {
                // Логируем ошибку и останавливаем симуляцию
                Console.WriteLine($"Ошибка в симуляции: {ex.Message}");
                Stop();
            }
        }

        #endregion

        #region Вычисления

        /// <summary>
        /// Вычисляет следующее поколение мира.
        /// Применяет правила Conway к каждой клетке.
        /// </summary>
        private void ComputeNextGeneration()
        {
            _liveCellCount = 0;

            // Параллельная обработка по строкам (если включено)
            if (_config.EnableParallelProcessing)
            {
                Parallel.For(0, Height, y =>
                {
                    for (int x = 0; x < Width; x++)
                    {
                        ComputeCell(x, y);
                    }
                });
            }
            else
            {
                // Последовательная обработка
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        ComputeCell(x, y);
                    }
                }
            }

            // Меняем буферы местами
            SwapBuffers();

            // Уведомляем рендерер
            _renderer?.Render(GetWorldState());
        }

        /// <summary>
        /// Вычисляет состояние одной клетки в следующем поколении.
        /// </summary>
        /// <param name="x">Координата X.</param>
        /// <param name="y">Координата Y.</param>
        private void ComputeCell(int x, int y)
        {
            int neighbours = CountNeighbours(x, y);
            bool isAlive = _worldState[x, y];
            bool willBeAlive = false;

            // Правила Conway
            if (isAlive)
            {
                // Живая клетка остаётся живой при 2 или 3 соседях
                willBeAlive = neighbours == 2 || neighbours == 3;
            }
            else
            {
                // Мёртвая клетка оживает при ровно 3 соседях
                willBeAlive = neighbours == 3;
            }

            _nextWorldState[x, y] = willBeAlive;

            // Считаем живые клетки (атомарно для параллельного режима)
            if (willBeAlive)
            {
                Interlocked.Increment(ref _liveCellCount);
            }
        }

        /// <summary>
        /// Подсчитывает количество живых соседей вокруг клетки.
        /// Учитывает зацикливание границ если включено в конфигурации.
        /// </summary>
        /// <param name="x">Координата X центральной клетки.</param>
        /// <param name="y">Координата Y центральной клетки.</param>
        /// <returns>Количество живых соседей (0-8).</returns>
        private int CountNeighbours(int x, int y)
        {
            int count = 0;

            // Проходим по 8 соседним клеткам
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    // Пропускаем саму клетку
                    if (dx == 0 && dy == 0)
                        continue;

                    // Вычисляем координаты соседа
                    int nx = x + dx;
                    int ny = y + dy;

                    // Обрабатываем границы
                    if (_config.WrapAround)
                    {
                        // Тороидальная топология (зацикливание)
                        nx = (nx + Width) % Width;
                        ny = (ny + Height) % Height;
                    }
                    else
                    {
                        // Ограниченные границы (пропускаем соседей за пределами)
                        if (nx < 0 || nx >= Width || ny < 0 || ny >= Height)
                            continue;
                    }

                    // Считаем живого соседа
                    if (_worldState[nx, ny])
                        count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Меняет буферы состояния местами.
        /// После вычисления следующего поколения оно становится текущим.
        /// </summary>
        private void SwapBuffers()
        {
            var temp = _worldState;
            _worldState = _nextWorldState;
            _nextWorldState = temp;
        }

        #endregion

        #region Доступ к состоянию

        /// <summary>
        /// Получает состояние конкретной клетки.
        /// </summary>
        /// <param name="x">Координата X.</param>
        /// <param name="y">Координата Y.</param>
        /// <returns>True если клетка жива, иначе false.</returns>
        public bool GetCell(int x, int y)
        {
            lock (_lockObject)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    return false;

                return _worldState[x, y];
            }
        }

        /// <summary>
        /// Устанавливает состояние конкретной клетки.
        /// </summary>
        /// <param name="x">Координата X.</param>
        /// <param name="y">Координата Y.</param>
        /// <param name="isAlive">Состояние клетки.</param>
        public void SetCell(int x, int y, bool isAlive)
        {
            lock (_lockObject)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    return;

                // Обновляем счётчик живых клеток
                if (_worldState[x, y] != isAlive)
                {
                    if (isAlive)
                        _liveCellCount++;
                    else
                        _liveCellCount--;

                    _worldState[x, y] = isAlive;
                }
            }
        }

        /// <summary>
        /// Добавляет живую клетку (алиас для SetCell true).
        /// </summary>
        public void AddCell(int x, int y)
        {
            SetCell(x, y, true);
        }

        /// <summary>
        /// Удаляет клетку (алиас для SetCell false).
        /// </summary>
        public void RemoveCell(int x, int y)
        {
            SetCell(x, y, false);
        }

        /// <summary>
        /// Получает полное состояние мира.
        /// Возвращает копию для безопасности.
        /// </summary>
        /// <returns>Двумерный массив состояния клеток.</returns>
        public bool[,] GetWorldArray()
        {
            lock (_lockObject)
            {
                var result = new bool[Width, Height];
                Array.Copy(_worldState, result, Width * Height);
                return result;
            }
        }

        /// <summary>
        /// Получает объект состояния мира для передачи подписчикам.
        /// </summary>
        /// <returns>Объект WorldState.</returns>
        public WorldState GetWorldState()
        {
            return new WorldState
            {
                Generation = CurrentGeneration,
                Width = Width,
                Height = Height,
                LiveCellCount = _liveCellCount,
                Status = Status,
                WorldArray = GetWorldArray()
            };
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Освобождает ресурсы движка.
        /// </summary>
        public void Dispose()
        {
            Stop();
            _cancellationTokenSource?.Dispose();
            _runTask?.Wait();
        }

        #endregion
		
		#region Режимы симуляции
		
		/// <summary>
		/// Текущий режим симуляции.
		/// </summary>
		private ISimulationMode _mode;
		
		/// <summary>
		/// Устанавливает режим симуляции.
		/// </summary>
		public void SetMode(ISimulationMode mode)
		{
			_mode = mode;
			_mode.Initialize(_config);
		}
		
		#endregion
		
		#region Рендеринг
		
		/// <summary>
		/// Текущий рендерер.
		/// </summary>
		private IRenderer _renderer;
		
		/// <summary>
		/// Устанавливает рендерер для визуализации.
		/// </summary>
		/// <param name="renderer">Рендерер для использования.</param>
		public void SetRenderer(IRenderer renderer)
		{
			// Отписываемся от старого рендерера
			if (_renderer != null)
			{
				_renderer.OnRenderCompleted -= OnRenderCompleted;
				_renderer.Dispose();
			}
		
			_renderer = renderer;
		
			// Подписываемся на новый рендерер
			if (_renderer != null)
			{
				_renderer.OnRenderCompleted += OnRenderCompleted;
			}
		}
		
		/// <summary>
		/// Получает текущий рендерер.
		/// </summary>
		public IRenderer GetRenderer()
		{
			return _renderer;
		}
		
		/// <summary>
		/// Обработчик завершения отрисовки кадра.
		/// </summary>
		private void OnRenderCompleted(RenderStats stats)
		{
			// Можно добавить логирование или статистику
			// Console.WriteLine($"Render: Gen {stats.Generation}, Time: {stats.RenderTimeMs}ms");
		}
		
		#endregion
    }
}