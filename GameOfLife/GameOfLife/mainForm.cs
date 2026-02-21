using System;
using System.Drawing;
using System.Windows.Forms;
using GameOfLife.Core;

namespace GameOfLife
{
    /// <summary>
    /// Основная форма приложения.
    /// Отвечает за отображение мира, управление камерой (зум, панорамирование)
    /// и взаимодействие пользователя с миром (рисование клеток мышью).
    /// 
    /// Теперь работает через события SimulationEngine, без прямой зависимости от логики.
    /// </summary>
    public partial class mainForm : Form
    {
        #region Поля движка

        /// <summary>
        /// Новый движок симуляции (из Core).
        /// </summary>
        private SimulationEngine _simulationEngine { get; set; }

        /// <summary>
        /// Режим симуляции (Conway, Ecosystem и т.д.).
        /// </summary>
        private ISimulationMode _simulationMode { get; set; }

        #endregion

        #region Поля отрисовки

        /// <summary>
        /// Размер мира в клетках.
        /// </summary>
        private Point2D _worldSize { get; set; }

        /// <summary>
        /// Объект рисования графики (Graphics для Bitmap).
        /// </summary>
        private Graphics _graphics { get; set; }

        /// <summary>
        /// Текущий Bitmap для отрисовки в PictureBox.
        /// </summary>
        private Bitmap _currentBitmap { get; set; }

        #endregion

        #region Поля камеры и навигации

        /// <summary>
        /// Предыдущий размер формы (для отслеживания изменений).
        /// </summary>
        private Size _previousSize { get; set; }

        /// <summary>
        /// Горизонтальная мировая координата мыши (в клетках).
        /// </summary>
        private int _offsetWorldX { get; set; }

        /// <summary>
        /// Вертикальная мировая координата мыши (в клетках).
        /// </summary>
        private int _offsetWorldY { get; set; }

        /// <summary>
        /// Текущая горизонтальная координата центра окна в мире (в клетках).
        /// </summary>
        private int _currentWorldX { get; set; }

        /// <summary>
        /// Текущая вертикальная координата центра окна в мире (в клетках).
        /// </summary>
        private int _currentWorldY { get; set; }

        /// <summary>
        /// Горизонтальная мировая координата начала отрисовки (в клетках).
        /// </summary>
        private int _worldWidthDrawBegin { get; set; }

        /// <summary>
        /// Вертикальная мировая координата начала отрисовки (в клетках).
        /// </summary>
        private int _worldHeightDrawBegin { get; set; }

        /// <summary>
        /// Горизонтальное смещение курсора мыши относительно центра окна (в клетках).
        /// </summary>
        private int _offsetWidth { get; set; }

        /// <summary>
        /// Вертикальное смещение курсора мыши относительно центра окна (в клетках).
        /// </summary>
        private int _offsetHeight { get; set; }

        /// <summary>
        /// Горизонтальный размер половины окна (в клетках, точный).
        /// </summary>
        private float _halfSizeWidth { get; set; }

        /// <summary>
        /// Вертикальный размер половины окна (в клетках, точный).
        /// </summary>
        private float _halfSizeHeight { get; set; }

        /// <summary>
        /// Горизонтальный размер части клеток за пределами окна (в пикселях).
        /// </summary>
        private float _halfSizeAbroadCellWidth { get; set; }

        /// <summary>
        /// Вертикальный размер части клеток за пределами окна (в пикселях).
        /// </summary>
        private float _halfSizeAbroadCellHeight { get; set; }

        /// <summary>
        /// Горизонтальный размер окна (в клетках, зависит от масштаба).
        /// </summary>
        private int _windowSizeWidth { get; set; }

        /// <summary>
        /// Вертикальный размер окна (в клетках, зависит от масштаба).
        /// </summary>
        private int _windowSizeHeight { get; set; }

        /// <summary>
        /// Размер клетки в пикселях (зум).
        /// </summary>
        private int _zoomCount { get; set; }

        /// <summary>
        /// Максимальный размер клетки в пикселях.
        /// </summary>
        private const int ZOOM_MAX = 64;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор формы.
        /// Инициализирует движок, подписывается на события, настраивает UI.
        /// </summary>
        public mainForm()
        {
            InitializeComponent();

            // Создаём движок с конфигурацией из UI
            var config = CreateConfigFromUI();
            _simulationEngine = new SimulationEngine(config);

            // Устанавливаем режим симуляции (Conway по умолчанию)
            _simulationMode = new Modes.ConwayMode();
            _simulationEngine.SetMode(_simulationMode);

            // Подписываемся на события движка
            _simulationEngine.OnWorldUpdated += OnWorldUpdated;
            _simulationEngine.OnStatusChanged += OnStatusChanged;

            // Инициализируем Bitmap для отрисовки
            _currentBitmap = new Bitmap(pictureBox.Width, pictureBox.Height);
            pictureBox.Image = _currentBitmap;
            _graphics = Graphics.FromImage(_currentBitmap);

            // Инициализируем размер мира из UI
            _worldSize = new Point2D(
                (int)WorldWidthNumericUpDown.Value,
                (int)WorldHeightNumericUpDown.Value
            );

            // Настраиваем начальные параметры
            ResizePictureBox();
            ResetZoom();
            pictureBox.MouseWheel += PictureBox_MouseWheel;
            CalculatingSize();
            UpdateFormTitle();
            CalculatingDrawBegin();

            // Отрисовываем начальное состояние
            DrawCurrentGeneration();
        }

        #endregion

        #region Создание конфигурации

        /// <summary>
        /// Создаёт конфигурацию симуляции на основе значений из UI.
        /// </summary>
        private SimulationConfig CreateConfigFromUI()
        {
            return new SimulationConfig
            {
                WorldWidth = (int)WorldWidthNumericUpDown.Value,
                WorldHeight = (int)WorldHeightNumericUpDown.Value,
                InitialDensity = (int)nudDensity.Value,
                TickDelayMs = 1000 / (int)nudRefresh.Value,
                WrapAround = true, // Зацикливание границ включено
                EnableParallelProcessing = true,
                ThreadCount = 0
            };
        }

        #endregion

        #region Обработка событий движка

        /// <summary>
        /// Обработчик события обновления мира.
        /// Вызывается движком после каждого шага симуляции.
        /// </summary>
        private void OnWorldUpdated(WorldState state)
        {
            // Отрисовка должна выполняться в UI потоке
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<WorldState>(OnWorldUpdated), state);
                return;
            }

            DrawCurrentGeneration();
        }

        /// <summary>
        /// Обработчик события изменения статуса симуляции.
        /// </summary>
        private void OnStatusChanged(SimulationStatus status)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<SimulationStatus>(OnStatusChanged), status);
                return;
            }

            UpdateButtonsByStatus(status);
            UpdateFormTitle();
        }

        /// <summary>
        /// Обновляет текст кнопок в соответствии со статусом симуляции.
        /// </summary>
        private void UpdateButtonsByStatus(SimulationStatus status)
        {
            switch (status)
            {
                case SimulationStatus.Stop:
                    bStart.Text = "Start";
                    bStop.Text = "Reset";
                    nudDensity.Enabled = true;
                    break;

                case SimulationStatus.Run:
                    bStart.Text = "Pause";
                    bStop.Text = "Stop";
                    nudDensity.Enabled = false;
                    break;

                case SimulationStatus.Pause:
                    bStart.Text = "Resume";
                    bStop.Text = "Reset";
                    nudDensity.Enabled = false;
                    break;
            }
        }

        #endregion

        #region Отрисовка

        /// <summary>
        /// Отрисовывает текущее поколение мира в PictureBox.
        /// Вызывается по событию OnWorldUpdated или при изменении пользователем.
        /// </summary>
        private void DrawCurrentGeneration()
        {
            if (_graphics == null || _currentBitmap == null)
                return;

            pictureBox.SuspendLayout();
            _graphics.Clear(Color.Black);

            // Получаем состояние мира из движка
            var worldState = _simulationEngine.GetWorldArray();

            // Отрисовка сетки (если включена)
            if (GridCheckBox.Checked)
            {
                using (Pen gridPen = new Pen(Color.DarkGray, 1))
                {
                    for (int x = 0; x < _windowSizeWidth + 1; x++)
                    {
                        int tempX = x * _zoomCount + (int)(_halfSizeAbroadCellWidth * _zoomCount);
                        _graphics.DrawLine(gridPen, tempX, 0, tempX, pictureBox.Height);
                    }

                    for (int y = 0; y < _windowSizeHeight + 1; y++)
                    {
                        int tempY = y * _zoomCount + (int)(_halfSizeAbroadCellHeight * _zoomCount);
                        _graphics.DrawLine(gridPen, 0, tempY, pictureBox.Width, tempY);
                    }
                }
            }

            // Отрисовка живых клеток
            for (int x = -1; x < _windowSizeWidth + 1; x++)
            {
                int tempX = x * _zoomCount + (int)(_halfSizeAbroadCellWidth * _zoomCount);
                int worldX = GetWrappedWorldX(x + _worldWidthDrawBegin);

                for (int y = -1; y < _windowSizeHeight + 1; y++)
                {
                    int tempY = y * _zoomCount + (int)(_halfSizeAbroadCellHeight * _zoomCount);
                    int worldY = GetWrappedWorldY(y + _worldHeightDrawBegin);

                    // Проверяем, жива ли клетка
                    if (worldX >= 0 && worldX < _simulationEngine.Width &&
                        worldY >= 0 && worldY < _simulationEngine.Height &&
                        worldState[worldX, worldY])
                    {
                        if (_zoomCount > 1)
                        {
                            _graphics.FillRectangle(
                                Brushes.Crimson,
                                tempX + 1, tempY + 1,
                                _zoomCount - 1, _zoomCount - 1
                            );
                        }
                        else
                        {
                            _graphics.FillRectangle(
                                Brushes.Crimson,
                                tempX, tempY, 1, 1
                            );
                        }
                    }
                }
            }

            pictureBox.ResumeLayout();
            pictureBox.Refresh();
        }

        /// <summary>
        /// Получает координату X с учётом зацикливания границ.
        /// </summary>
        private int GetWrappedWorldX(int x)
        {
            if (_simulationEngine.Config.WrapAround)
            {
                return ((x % _simulationEngine.Width) + _simulationEngine.Width) % _simulationEngine.Width;
            }
            else
            {
                return Math.Max(0, Math.Min(x, _simulationEngine.Width - 1));
            }
        }

        /// <summary>
        /// Получает координату Y с учётом зацикливания границ.
        /// </summary>
        private int GetWrappedWorldY(int y)
        {
            if (_simulationEngine.Config.WrapAround)
            {
                return ((y % _simulationEngine.Height) + _simulationEngine.Height) % _simulationEngine.Height;
            }
            else
            {
                return Math.Max(0, Math.Min(y, _simulationEngine.Height - 1));
            }
        }

        /// <summary>
        /// Обновляет заголовок формы с информацией о состоянии.
        /// </summary>
        private void UpdateFormTitle()
        {
            this.Text = $"Generation:{_simulationEngine.CurrentGeneration} " +
                       $"Zoom:{_zoomCount} " +
                       $"world_X:{_offsetWorldX} " +
                       $"world_Y:{_offsetWorldY} " +
                       $"mouse_X:{_offsetWidth:F2} " +
                       $"mouse_Y:{_offsetHeight:F2} " +
                       $"hs_X:{_halfSizeWidth:F2} " +
                       $"hs_Y:{_halfSizeHeight:F2} " +
                       $"Live:{_simulationEngine.LiveCellCount} " +
                       $"Status:{_simulationEngine.Status}";
        }

        /// <summary>
        /// Пересоздаёт Bitmap при изменении размера PictureBox.
        /// </summary>
        private void ResizePictureBox()
        {
            _currentBitmap?.Dispose();
            _currentBitmap = new Bitmap(pictureBox.Width, pictureBox.Height);
            pictureBox.Image = _currentBitmap;

            _graphics?.Dispose();
            _graphics = Graphics.FromImage(_currentBitmap);
        }

        #endregion

        #region Вычисления координат и масштаба

        /// <summary>
        /// Вычисляет мировые координаты мыши.
        /// </summary>
        private void CalculatingScale(MouseEventArgs e)
        {
            // Координаты мыши относительно центра изображения (пиксели)
            float mouseOffsetWidth = e.Location.X - (float)pictureBox.Width / 2;
            float mouseOffsetHeight = e.Location.Y - (float)pictureBox.Height / 2;

            // Коррекция для точного попадания в клетку
            float cellOffsetWidth = mouseOffsetWidth > 0 ? 0.5f : -0.5f;
            float cellOffsetHeight = mouseOffsetHeight > 0 ? 0.5f : -0.5f;

            // Смещение в клетках
            _offsetWidth = (int)Math.Truncate((mouseOffsetWidth / _zoomCount + cellOffsetWidth));
            _offsetHeight = (int)Math.Truncate((mouseOffsetHeight / _zoomCount + cellOffsetHeight));

            // Мировые координаты с учётом центра камеры
            _offsetWorldX = GetWrappedWorldX(_currentWorldX + _offsetWidth);
            _offsetWorldY = GetWrappedWorldY(_currentWorldY + _offsetHeight);
        }

        /// <summary>
        /// Вычисляет размеры окна в клетках.
        /// </summary>
        private void CalculatingSize()
        {
            // Размер половины изображения в клетках
            _halfSizeWidth = (float)pictureBox.Width / (2 * _zoomCount);
            _halfSizeHeight = (float)pictureBox.Height / (2 * _zoomCount);

            // Размер окна в клетках
            _windowSizeWidth = pictureBox.Width / _zoomCount;
            _windowSizeHeight = pictureBox.Height / _zoomCount;

            // Размер части клетки за пределами экрана
            _halfSizeAbroadCellWidth = Truncate((float)pictureBox.Width / _zoomCount) / 2;
            _halfSizeAbroadCellHeight = Truncate((float)pictureBox.Height / _zoomCount) / 2;
        }

        /// <summary>
        /// Вычисляет начало отрисовки мира.
        /// </summary>
        private void CalculatingDrawBegin()
        {
            _worldWidthDrawBegin = _currentWorldX - (int)Math.Truncate(_halfSizeWidth);
            _worldHeightDrawBegin = _currentWorldY - (int)Math.Truncate(_halfSizeHeight);
        }

        /// <summary>
        /// Возвращает дробную часть числа.
        /// </summary>
        private float Truncate(float value)
        {
            return value - (float)Math.Truncate(value);
        }

        /// <summary>
        /// Сбрасывает зум и позицию камеры в начальные значения.
        /// </summary>
        private void ResetZoom()
        {
            _zoomCount = 1;
            _worldWidthDrawBegin = 0;
            _worldHeightDrawBegin = 0;
            _currentWorldX = _simulationEngine.Width / 2;
            _currentWorldY = _simulationEngine.Height / 2;
        }

        #endregion

        #region Управление мышью

        /// <summary>
        /// Обработчик клика мыши по PictureBox.
        /// </summary>
        private void pictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            MouseExecuter(e);
        }

        /// <summary>
        /// Обработчик движения мыши по PictureBox.
        /// </summary>
        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            MouseExecuter(e);
        }

        /// <summary>
        /// Выполняет действие в зависимости от кнопки мыши.
        /// </summary>
        private void MouseExecuter(MouseEventArgs e)
        {
            CalculatingScale(e);

            switch (e.Button)
            {
                case MouseButtons.Left:
                    _simulationEngine.AddCell(_offsetWorldX, _offsetWorldY);
                    DrawCurrentGeneration();
                    break;

                case MouseButtons.Right:
                    _simulationEngine.RemoveCell(_offsetWorldX, _offsetWorldY);
                    DrawCurrentGeneration();
                    break;
            }

            UpdateFormTitle();
        }

        /// <summary>
        /// Обработчик прокрутки колеса мыши (зум).
        /// </summary>
        private void PictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            bool update = false;

            if (e.Delta > 0) // Колесико вверх - увеличение
            {
                _zoomCount = _zoomCount << 1;
                if (_zoomCount > ZOOM_MAX)
                {
                    _zoomCount = ZOOM_MAX;
                }
                else
                {
                    update = true;
                }
            }
            else // Колесико вниз - уменьшение
            {
                _zoomCount = _zoomCount >> 1;
                if (_zoomCount < 1)
                {
                    _zoomCount = 1;
                }
                else
                {
                    update = true;
                }
            }

            if (update)
            {
                // Запоминаем новую позицию центра
                _currentWorldX = _offsetWorldX;
                _currentWorldY = _offsetWorldY;

                CalculatingScale(e);
                CalculatingSize();
                CalculatingDrawBegin();
                DrawCurrentGeneration();
            }
        }

        #endregion

        #region Управление симуляцией

        /// <summary>
        /// Запускает симуляцию.
        /// </summary>
        private void StartGame()
        {
            if (_simulationEngine.IsRunning)
                return;

            _simulationEngine.Start();
        }

        /// <summary>
        /// Ставит симуляцию на паузу.
        /// </summary>
        private void PauseGame()
        {
            if (!_simulationEngine.IsRunning)
                return;

            _simulationEngine.Pause();
        }

        /// <summary>
        /// Возобновляет симуляцию после паузы.
        /// </summary>
        private void ResumeGame()
        {
            if (_simulationEngine.Status != SimulationStatus.Pause)
                return;

            _simulationEngine.Resume();
        }

        /// <summary>
        /// Останавливает симуляцию полностью.
        /// </summary>
        private void StopGame()
        {
            _simulationEngine.Stop();
            _simulationEngine.FillRandom(_simulationEngine.Config.InitialDensity);
            DrawCurrentGeneration();
        }

        /// <summary>
        /// Выполняет один шаг симуляции.
        /// </summary>
        private void StepGame()
        {
            _simulationEngine.Step();
        }

        #endregion

        #region Обработчики событий UI

        /// <summary>
        /// Обработчик кнопки Start/Pause/Resume.
        /// </summary>
        private void bStart_Click(object sender, EventArgs e)
        {
            switch (_simulationEngine.Status)
            {
                case SimulationStatus.Stop:
                    StartGame();
                    break;

                case SimulationStatus.Run:
                    PauseGame();
                    break;

                case SimulationStatus.Pause:
                    ResumeGame();
                    break;
            }
        }

        /// <summary>
        /// Обработчик кнопки Stop/Reset.
        /// </summary>
        private void bStop_Click(object sender, EventArgs e)
        {
            if (_simulationEngine.Status == SimulationStatus.Run ||
                _simulationEngine.Status == SimulationStatus.Pause)
            {
                StopGame();
            }
            else
            {
                _simulationEngine.ResizeWorld(
                    (int)WorldWidthNumericUpDown.Value,
                    (int)WorldHeightNumericUpDown.Value
                );
                DrawCurrentGeneration();
            }
        }

        /// <summary>
        /// Обработчик кнопки Random.
        /// </summary>
        private void bRnd_Click(object sender, EventArgs e)
        {
            _simulationEngine.FillRandom((int)nudDensity.Value);
            DrawCurrentGeneration();
        }

        /// <summary>
        /// Обработчик изменения скорости обновления.
        /// </summary>
        private void nudRefresh_ValueChanged(object sender, EventArgs e)
        {
            // Обновляем конфигурацию движка
            // В новой архитектуре это можно сделать через свойство Config
        }

        /// <summary>
        /// Обработчик изменения плотности случайного заполнения.
        /// </summary>
        private void nudDensity_ValueChanged(object sender, EventArgs e)
        {
            // Плотность применяется при следующем FillRandom
        }

        /// <summary>
        /// Обработчик изменения размера формы.
        /// </summary>
        private void mainForm_ResizeEnd(object sender, EventArgs e)
        {
            if (this.Size == _previousSize)
                return;

            ResizePictureBox();
            CalculatingSize();
            CalculatingDrawBegin();
            DrawCurrentGeneration();
            _previousSize = this.Size;
        }

        /// <summary>
        /// Обработчик первого показа формы.
        /// </summary>
        private void mainForm_Shown(object sender, EventArgs e)
        {
            _previousSize = this.Size;
        }

        /// <summary>
        /// Обработчик изменения ширины мира.
        /// </summary>
        private void WorldWidthNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            _worldSize.X = (int)WorldWidthNumericUpDown.Value;
            _simulationEngine.ResizeWorld((int)WorldWidthNumericUpDown.Value, _simulationEngine.Height);
            ResetZoom();
            CalculatingSize();
            CalculatingDrawBegin();
            DrawCurrentGeneration();
        }

        /// <summary>
        /// Обработчик изменения высоты мира.
        /// </summary>
        private void WorldHeightNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            _worldSize.Y = (int)WorldHeightNumericUpDown.Value;
            _simulationEngine.ResizeWorld(_simulationEngine.Width, (int)WorldHeightNumericUpDown.Value);
            ResetZoom();
            CalculatingSize();
            CalculatingDrawBegin();
            DrawCurrentGeneration();
        }

        /// <summary>
        /// Обработчик изменения размера PictureBox.
        /// </summary>
        private void pictureBox_SizeChanged(object sender, EventArgs e)
        {
            ResizePictureBox();
            CalculatingSize();
            CalculatingDrawBegin();
            DrawCurrentGeneration();
        }

        /// <summary>
        /// Обработчик включения/выключения сетки.
        /// </summary>
        private void GridCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            DrawCurrentGeneration();
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Освобождает ресурсы формы.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Отписываемся от событий движка
                if (_simulationEngine != null)
                {
                    _simulationEngine.OnWorldUpdated -= OnWorldUpdated;
                    _simulationEngine.OnStatusChanged -= OnStatusChanged;
                    _simulationEngine.Dispose();
                }

                // Освобождаем графические ресурсы
                _graphics?.Dispose();
                _currentBitmap?.Dispose();

                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}