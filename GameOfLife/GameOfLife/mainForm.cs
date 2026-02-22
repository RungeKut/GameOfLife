using System;
using System.Drawing;
using System.Windows.Forms;
using GameOfLife.Core;
using GameOfLife.Rendering;
using GameOfLife.Modes;

namespace GameOfLife
{
    /// <summary>
    /// Основная форма приложения Game of Life.
    /// </summary>
    public partial class mainForm : Form
    {
        #region Поля движка и рендеринга

        private SimulationEngine _simulationEngine;
        private WinFormsRenderer _renderer;
        private ISimulationMode _currentMode;

        #endregion

        #region Поля навигации и камеры

        private Point2D _worldSize;
        private Size _previousSize;
        private int _offsetWorldX;
        private int _offsetWorldY;
        private int _currentWorldX;
        private int _currentWorldY;
        private int _worldWidthDrawBegin;
        private int _worldHeightDrawBegin;
        private int _offsetWidth;
        private int _offsetHeight;
        private float _halfSizeWidth;
        private float _halfSizeHeight;
        private float _halfSizeAbroadCellWidth;
        private float _halfSizeAbroadCellHeight;
        private int _windowSizeWidth;
        private int _windowSizeHeight;

        // Инициализация при объявлении (защита от DivideByZero)
        private int _zoomCount = 1;
        private const int ZOOM_MAX = 64;

        #endregion

        #region Конструктор

        public mainForm()
        {
            InitializeComponent();

            var config = CreateConfigFromUI();
            _simulationEngine = new SimulationEngine(config);

            _currentMode = new ConwayMode();
            _simulationEngine.SetMode(_currentMode);

            _renderer = new WinFormsRenderer(pictureBox);
            _simulationEngine.SetRenderer(_renderer);

            // Подписываемся на события движка
            _simulationEngine.OnStatusChanged += OnStatusChanged;
            _simulationEngine.OnWorldUpdated += OnWorldUpdated;

            _worldSize = new Point2D(
                (int)WorldWidthNumericUpDown.Value,
                (int)WorldHeightNumericUpDown.Value
            );

            ResizePictureBox();
            ResetZoom();
            pictureBox.MouseWheel += PictureBox_MouseWheel;
            CalculatingSize();
            UpdateFormTitle();
            CalculatingDrawBegin();
            DrawCurrentGeneration();
        }

        #endregion

        #region Создание конфигурации

        private SimulationConfig CreateConfigFromUI()
        {
            return new SimulationConfig
            {
                WorldWidth = (int)WorldWidthNumericUpDown.Value,
                WorldHeight = (int)WorldHeightNumericUpDown.Value,
                InitialDensity = (int)nudDensity.Value,
                TickDelayMs = 1000 / (int)nudRefresh.Value,
                WrapAround = true,
                EnableParallelProcessing = true,
                ThreadCount = 0
            };
        }

        #endregion

        #region Обработка событий движка

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

        private void OnWorldUpdated(WorldState state)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<WorldState>(OnWorldUpdated), state);
                return;
            }

            DrawCurrentGeneration();
            // ✅ КЛЮЧЕВОЕ ИЗМЕНЕНИЕ: Обновляем заголовок с номером поколения
            UpdateFormTitle();
        }

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

        private void DrawCurrentGeneration()
        {
            if (_renderer != null && _simulationEngine != null)
            {
                // ✅ КЛЮЧЕВОЕ ИЗМЕНЕНИЕ: Передаём позицию камеры в рендерер
                // Это обеспечивает соответствие между отображением и координатами мыши
                _renderer.SetCameraPosition(_currentWorldX, _currentWorldY);

                var worldState = _simulationEngine.GetWorldState();
                _renderer.Render(worldState);
            }
        }

        /// <summary>
        /// Обновляет заголовок формы с информацией о состоянии симуляции.
        /// </summary>
        private void UpdateFormTitle()
        {
            this.Text = string.Format(
                "Generation:{0} Zoom:{1} world_X:{2} world_Y:{3} mouse_X:{4:F2} mouse_Y:{5:F2} hs_X:{6:F2} hs_Y:{7:F2} Live:{8} Status:{9}",
                _simulationEngine.CurrentGeneration,
                _zoomCount,
                _offsetWorldX,
                _offsetWorldY,
                _offsetWidth,
                _offsetHeight,
                _halfSizeWidth,
                _halfSizeHeight,
                _simulationEngine.LiveCellCount,
                _simulationEngine.Status);
        }

        private void ResizePictureBox()
        {
            if (pictureBox.Image != null)
            {
                pictureBox.Image.Dispose();
            }
            pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);
        }

        #endregion

        #region Вычисления координат и масштаба

        private void CalculatingScale(MouseEventArgs e)
        {
            float mouseOffsetWidth = e.Location.X - (float)pictureBox.Width / 2;
            float mouseOffsetHeight = e.Location.Y - (float)pictureBox.Height / 2;

            float cellOffsetWidth = mouseOffsetWidth > 0 ? 0.5f : -0.5f;
            float cellOffsetHeight = mouseOffsetHeight > 0 ? 0.5f : -0.5f;

            _offsetWidth = (int)Math.Truncate((mouseOffsetWidth / _zoomCount + cellOffsetWidth));
            _offsetHeight = (int)Math.Truncate((mouseOffsetHeight / _zoomCount + cellOffsetHeight));

            _offsetWorldX = GetWrappedWorldX(_currentWorldX + _offsetWidth);
            _offsetWorldY = GetWrappedWorldY(_currentWorldY + _offsetHeight);
        }

        private void CalculatingSize()
        {
            if (_zoomCount <= 0)
            {
                _zoomCount = 1;
            }

            _halfSizeWidth = (float)pictureBox.Width / (2 * _zoomCount);
            _halfSizeHeight = (float)pictureBox.Height / (2 * _zoomCount);

            _windowSizeWidth = pictureBox.Width / _zoomCount;
            _windowSizeHeight = pictureBox.Height / _zoomCount;

            _halfSizeAbroadCellWidth = Truncate((float)pictureBox.Width / _zoomCount) / 2;
            _halfSizeAbroadCellHeight = Truncate((float)pictureBox.Height / _zoomCount) / 2;
        }

        private void CalculatingDrawBegin()
        {
            _worldWidthDrawBegin = _currentWorldX - (int)Math.Truncate(_halfSizeWidth);
            _worldHeightDrawBegin = _currentWorldY - (int)Math.Truncate(_halfSizeHeight);
        }

        private float Truncate(float value)
        {
            return value - (float)Math.Truncate(value);
        }

        private void ResetZoom()
        {
            _zoomCount = 1;
            _worldWidthDrawBegin = 0;
            _worldHeightDrawBegin = 0;
            _currentWorldX = _simulationEngine.Width / 2;
            _currentWorldY = _simulationEngine.Height / 2;
        }

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

        #endregion

        #region Управление мышью

        private void pictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            MouseExecuter(e);
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            MouseExecuter(e);
        }

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
        /// Зум относительно клетки под курсором.
        /// </summary>
        private void PictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            bool update = false;

            // Сначала вычисляем координаты мыши ДО изменения зума
            CalculatingScale(e);

            if (e.Delta > 0)
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
            else
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
                // Сохраняем клетку под курсором в центре после зума
                _currentWorldX = _offsetWorldX;
                _currentWorldY = _offsetWorldY;

                // Синхронизируем зум формы с рендерером
                if (_renderer != null)
                {
                    _renderer.Config.CellSize = _zoomCount;
                }

                CalculatingScale(e);
                CalculatingSize();
                CalculatingDrawBegin();
                DrawCurrentGeneration();
                UpdateFormTitle();
            }
        }

        #endregion

        #region Управление симуляцией

        private void StartGame()
        {
            if (_simulationEngine.IsRunning)
                return;
            _simulationEngine.Start();
        }

        private void PauseGame()
        {
            if (!_simulationEngine.IsRunning)
                return;
            _simulationEngine.Pause();
        }

        private void ResumeGame()
        {
            if (_simulationEngine.Status != SimulationStatus.Pause)
                return;
            _simulationEngine.Resume();
        }

        private void StopGame()
        {
            _simulationEngine.Stop();
            _simulationEngine.FillRandom(_simulationEngine.Config.InitialDensity);
            DrawCurrentGeneration();
        }

        private void StepGame()
        {
            _simulationEngine.Step();
        }

        #endregion

        #region Обработчики событий UI

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

        private void bRnd_Click(object sender, EventArgs e)
        {
            _simulationEngine.FillRandom((int)nudDensity.Value);
            DrawCurrentGeneration();
        }

        private void nudRefresh_ValueChanged(object sender, EventArgs e)
        {
        }

        private void nudDensity_ValueChanged(object sender, EventArgs e)
        {
        }

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

        private void mainForm_Shown(object sender, EventArgs e)
        {
            _previousSize = this.Size;
        }

        private void WorldWidthNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            _worldSize.X = (int)WorldWidthNumericUpDown.Value;
            _simulationEngine.ResizeWorld((int)WorldWidthNumericUpDown.Value, _simulationEngine.Height);
            ResetZoom();
            CalculatingSize();
            CalculatingDrawBegin();
            DrawCurrentGeneration();
        }

        private void WorldHeightNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            _worldSize.Y = (int)WorldHeightNumericUpDown.Value;
            _simulationEngine.ResizeWorld(_simulationEngine.Width, (int)WorldHeightNumericUpDown.Value);
            ResetZoom();
            CalculatingSize();
            CalculatingDrawBegin();
            DrawCurrentGeneration();
        }

        private void pictureBox_SizeChanged(object sender, EventArgs e)
        {
            // Пересоздаём Bitmap PictureBox
            ResizePictureBox();

            // Пересчитываем параметры камеры
            CalculatingSize();
            CalculatingDrawBegin();

            // ✅ Принудительно перерисовываем через рендерер
            // Это нужно чтобы WinFormsRenderer обновил тайлинг под новый размер
            if (_renderer != null && _simulationEngine != null)
            {
                var worldState = _simulationEngine.GetWorldState();
                _renderer.Render(worldState);
            }
        }

        private void GridCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_renderer != null)
            {
                _renderer.Config.ShowGrid = GridCheckBox.Checked;
                DrawCurrentGeneration();
            }
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_simulationEngine != null)
                {
                    _simulationEngine.OnStatusChanged -= OnStatusChanged;
                    _simulationEngine.OnWorldUpdated -= OnWorldUpdated;
                    _simulationEngine.Dispose();
                }

                if (_renderer != null)
                {
                    _renderer.Dispose();
                }

                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}