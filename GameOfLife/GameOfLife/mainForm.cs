using System;
using System.Drawing;
using System.Windows.Forms;

namespace GameOfLife
{
    public partial class mainForm : Form
    {
        /// <summary>
        /// Размер мира
        /// </summary>
        private Point2D _worldSize { get; set; }
        /// <summary>
        /// Объект рисования графики
        /// </summary>
        private Graphics _graphics { get; set; }
        /// <summary>
        /// Объект мира
        /// </summary>
        private GameEngine _gameEngine { get; set; }
        /// <summary>
        /// Предыдущий размер виндовс-формы
        /// </summary>
        private Size _prvSize { get; set; }
        /// <summary>
        /// Горизонтальная мировая координата мыши
        /// </summary>
        private int _offsetWorldX { get; set; }
        /// <summary>
        /// Вертикальная мировая координата мыши
        /// </summary>
        private int _offsetWorldY { get; set; }
        /// <summary>
        /// Текущая горизонтальная координата центра окна в мире (в клетках)
        /// </summary>
        private int _currentWorldX { get; set; }
        /// <summary>
        /// Текущая вертикальная координата центра окна в мире (в клетках)
        /// </summary>
        private int _currentWorldY { get; set; }
        /// <summary>
        /// Горизонтальная мировая координата начала отрисовки окна (в клетках)
        /// </summary>
        private int _worldHeightDrawBegin { get; set; }
        /// <summary>
        /// Вертикальная мировая координата начала отрисовки окна (в клетках)
        /// </summary>
        private int _worldWidthDrawBegin { get; set; }
        /// <summary>
        /// Горизонтальное смещение курсора мыши отностительно центра окна (в клетках)
        /// </summary>
        private int _offsetWidth { get; set; }
        /// <summary>
        /// Вертикальное смещение курсора мыши отностительно центра окна (в клетках)
        /// </summary>
        private int _offsetHeight { get; set; }
        /// <summary>
        /// Горизонтальный размер половины мира (в клетках) точный
        /// </summary>
        private float _halfSizeWidth { get; set; }
        /// <summary>
        /// Вертикальный размер половины мира (в клетках) точный
        /// </summary>
        private float _halfSizeHeight { get; set; }
        /// <summary>
        /// Горизонтальный размер части клеток, которые уходят за пределы окна слева и справа (в пикселях)
        /// </summary>
        private float _halfSizeAbroadCellWidth { get; set; }
        /// <summary>
        /// Вертикальный размер части клеток, которые уходят за пределы окна сверху и снизу (в пикселях)
        /// </summary>
        private float _halfSizeAbroadCellHeight { get; set; }
        /// <summary>
        /// Горизонтальный размер окна (в клетках) зависит от масштаба
        /// </summary>
        private int _windowSizeWidth { get; set; }
        /// <summary>
        /// Вертикальный размер окна (в клетках) зависит от масштаба
        /// </summary>
        private int _windowSizeHeight { get; set; }
        /// <summary>
        /// Размер клетки (в пикселях)
        /// </summary>
        private int _zoomCount { get; set; }
        /// <summary>
        /// Максимальный размер клетки (в пикселях)
        /// </summary>
        private const int ZOOM_MAX = 64;

        public mainForm()
        {
            InitializeComponent();
            _gameEngine = new GameEngine();
            pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);
            _graphics = Graphics.FromImage(pictureBox.Image);
            _worldSize = new Point2D((int)WorldHeightNumericUpDown.Value, (int)WorldWidthNumericUpDown.Value);
            _gameEngine.ResizeWorld(_worldSize);
            ResizePictureBox();
            ResetZoom();
            pictureBox.MouseWheel += PictureBox_MouseWheel;
            CalculatingSize();
            UpdateFormTitle();
            CalculatingDrawBegin();
        }

        #region Отрисовка PictureBox

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
                    _gameEngine.AddCell(_offsetWorldX, _offsetWorldY);
                    DrawCurrentGeneration();
                    break;
                case MouseButtons.None:
                    break;
                case MouseButtons.Right:
                    _gameEngine.RemoveCell(_offsetWorldX, _offsetWorldY);
                    DrawCurrentGeneration();
                    break;
                case MouseButtons.Middle:
                    break;
                case MouseButtons.XButton1:
                    break;
                case MouseButtons.XButton2:
                    break;
                default:
                    break;
            }
            UpdateFormTitle();
        }

        private float Truncate(float a) { return a - (float)Math.Truncate(a); }

        private void DrawCurrentGeneration()
        {
            pictureBox.SuspendLayout();
            _graphics.Clear(Color.Black);

            var _field = _gameEngine.GetCurrentGeneration();

            if (GridCheckBox.Checked)
            {
                Pen _style = new Pen(Color.DarkGray, 1);// цвет линии и ширина
                for (int x = 0; x < _windowSizeWidth + 1; x++)
                {
                    int _tempX = x * _zoomCount + (int)(_halfSizeAbroadCellWidth * _zoomCount);
                    _graphics.DrawLine(_style, _tempX, 0, _tempX, pictureBox.Height);
                }
                for (int y = 0; y < _windowSizeHeight + 1; y++)
                {
                    int _tempY = y * _zoomCount + (int)(_halfSizeAbroadCellHeight * _zoomCount);
                    _graphics.DrawLine(_style, 0, _tempY, pictureBox.Width, _tempY);
                }
            }

            for (int x = -1; x < _windowSizeWidth + 1; x++)
            {
                int _tempX = x * _zoomCount + (int)(_halfSizeAbroadCellWidth * _zoomCount);
                int _worldX;
                if (x + _worldWidthDrawBegin >= 0) _worldX = (int)((x + _worldWidthDrawBegin) % _worldSize.X);
                else _worldX = (int)((_worldSize.X + x + _worldWidthDrawBegin) % _worldSize.X);

                for (int y = -1; y < _windowSizeHeight + 1; y++)
                {
                    int _tempY = y * _zoomCount + (int)(_halfSizeAbroadCellHeight * _zoomCount);
                    int _worldY;
                    if (y + _worldHeightDrawBegin >= 0) _worldY = (int)((y + _worldHeightDrawBegin) % _worldSize.Y);
                    else _worldY = (int)((_worldSize.Y + y + _worldHeightDrawBegin) % _worldSize.Y);

                    if (_field[_worldX, _worldY])
                    {
                        if (_zoomCount > 1)
                            _graphics.FillRectangle(Brushes.Crimson, _tempX + 1, _tempY + 1, _zoomCount - 1, _zoomCount - 1);
                        else
                            _graphics.FillRectangle(Brushes.Crimson, _tempX, _tempY, 1, 1);
                    }
                }
            }
            pictureBox.ResumeLayout();
            UpdateFormTitle();
            pictureBox.Refresh();
        }

        private void UpdateFormTitle()
        {
            this.Text = $"Generation:{_gameEngine.CurrentGeneration} Zoom:{_zoomCount} world_X:{_offsetWorldX} world_Y:{_offsetWorldY} mouse_X:{_offsetWidth:F2} mouse_Y:{_offsetHeight:F2} hs_X:{_halfSizeWidth:F2} hs_Y:{_halfSizeHeight:F2}";
        }

        private void ResizePictureBox()
        {
            pictureBox.Image.Dispose();
            pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);
            _graphics.Dispose();
            _graphics = Graphics.FromImage(pictureBox.Image);
        }

        private void CalculatingScale(MouseEventArgs e)
        {
            // Координаты мыши относительно центра изображения (пиксели)
            float _mouseOffsetWidth = e.Location.X - (float)pictureBox.Width / 2;
            float _mouseOffsetHeight = e.Location.Y - (float)pictureBox.Height / 2;
            // Координаты мыши относительно центра изображения с учетом масштаба и размера мира (количество клеток)
            float cellOffsetWidth = (float)(_mouseOffsetWidth > 0 ? 0.5 : -0.5);
            float cellOffsetHeight = (float)(_mouseOffsetHeight > 0 ? 0.5 : -0.5);
            _offsetWidth = (int)Math.Truncate((_mouseOffsetWidth / _zoomCount + cellOffsetWidth) % _worldSize.X);
            _offsetHeight = (int)Math.Truncate((_mouseOffsetHeight / _zoomCount + cellOffsetHeight) % _worldSize.Y);
            // Координаты центра окна в мире
            if (_offsetWidth >= 0) _offsetWorldX = (int)((_currentWorldX + _offsetWidth) % _worldSize.X);
            else if (_currentWorldX >= -_offsetWidth) _offsetWorldX = (int)((_currentWorldX + _offsetWidth) % _worldSize.X);
            else _offsetWorldX = (int)((_currentWorldX + (_worldSize.X + _offsetWidth)) % _worldSize.X);

            if (_offsetHeight >= 0) _offsetWorldY = (int)((_currentWorldY + _offsetHeight) % _worldSize.Y);
            else if (_currentWorldY >= -_offsetHeight) _offsetWorldY = (int)((_currentWorldY + _offsetHeight) % _worldSize.Y);
            else _offsetWorldY = (int)((_currentWorldY + (_worldSize.Y + _offsetHeight)) % _worldSize.Y);
        }

        private void CalculatingSize()
        {
            // Размер половины изображения с учетом масштаба (количество клеток)
            _halfSizeWidth = (float)pictureBox.Width / (2 * _zoomCount);
            _halfSizeHeight = (float)pictureBox.Height / (2 * _zoomCount);
            // Размер окна на мир с учетом масштаба (количество клеток)
            _windowSizeWidth = pictureBox.Width / _zoomCount;
            _windowSizeHeight = pictureBox.Height / _zoomCount;
            //Размер половины неубирающейся части клетки на экран
            _halfSizeAbroadCellWidth = Truncate((float)pictureBox.Width / _zoomCount) / 2;
            _halfSizeAbroadCellHeight = Truncate((float)pictureBox.Height / _zoomCount) / 2;
        }

        private void ResetZoom()
        {
            _zoomCount = 1;
            _worldWidthDrawBegin = 0;
            _worldHeightDrawBegin = 0;
            _currentWorldX = (int)(_worldSize.X / 2);
            _currentWorldY = (int)(_worldSize.Y / 2);
        }

        // Масштабирование относительно координат мыши
        private void PictureBox_MouseWheel(object sender, MouseEventArgs e) // Событие вращения колеса
        {
            bool update = false;
            if (e.Delta > 0) // Колесико вверх
            {
                _zoomCount = _zoomCount << 1;
                if (_zoomCount > ZOOM_MAX) { _zoomCount = ZOOM_MAX; }
                else update = true;
            }
            else // Колесико вниз
            {
                _zoomCount = _zoomCount >> 1;
                if (_zoomCount < 1) { _zoomCount = 1; }
                else update = true;
            }
            if (update)
            {
                // запоминаем перестроенные координаты
                _currentWorldX = _offsetWorldX;
                _currentWorldY = _offsetWorldY;

                CalculatingScale(e);
                CalculatingSize();
                CalculatingDrawBegin();
                DrawCurrentGeneration();
            }

        }
        private void CalculatingDrawBegin()
        {
            _worldWidthDrawBegin = (int)((_currentWorldX - (int)Math.Truncate(_halfSizeWidth)) % _worldSize.X);
            _worldHeightDrawBegin = (int)((_currentWorldY - (int)Math.Truncate(_halfSizeHeight)) % _worldSize.Y);
        }
        #endregion

        #region Управление
        private void StartGame()
        {
            if (timer.Enabled) return;
            bStart.Text = "Pause";
            bStop.Text = "Stop";
            nudDensity.Enabled = false;

            timer.Start();
            _gameEngine._statusEngine = StatusEngine.run;
        }

        private void PauseGame()
        {
            if (!timer.Enabled) return;
            bStart.Text = "Resume";
            bStop.Text = "Reset";
            timer.Stop();
            _gameEngine._statusEngine = StatusEngine.pause;
        }

        private void ResumeGame()
        {
            if (timer.Enabled) return;
            bStart.Text = "Pause";
            bStop.Text = "Stop";
            timer.Start();
            _gameEngine._statusEngine = StatusEngine.run;
        }

        private void StopGame()
        {
            if (!timer.Enabled) return;
            bStart.Text = "Start";
            bStop.Text = "Reset";
            timer.Stop();
            nudDensity.Enabled = true;
            _gameEngine._statusEngine = StatusEngine.stop;
        }
        #endregion

        private void timer_Tick(object sender, EventArgs e)
        {
            DrawCurrentGeneration();
            _gameEngine.NextGeneration();
        }

        #region Обработчики событий формы
        private void bStart_Click(object sender, EventArgs e)
        {
            if (_gameEngine != null)
                switch (_gameEngine._statusEngine)
                {
                    case StatusEngine.stop:
                        StartGame();
                        break;
                    case StatusEngine.run:
                        PauseGame();
                        break;
                    case StatusEngine.pause:
                        ResumeGame();
                        break;
                }
            else
            {
                StartGame();
            }
        }

        private void bStop_Click(object sender, EventArgs e)
        {
            if (_gameEngine != null)
                switch (_gameEngine._statusEngine)
                {
                    case StatusEngine.run:
                        StopGame();
                        break;
                    default:
                        _gameEngine.ResizeWorld(_worldSize);
                        DrawCurrentGeneration();
                        break;
                }
        }

        private void nudRefresh_ValueChanged(object sender, EventArgs e)
        {
            timer.Interval = 1000 / (int)nudRefresh.Value;
        }

        private void bRnd_Click(object sender, EventArgs e)
        {
            _gameEngine.FillRandom((int)nudDensity.Minimum + (int)nudDensity.Maximum - (int)nudDensity.Value);
            DrawCurrentGeneration();
        }

        private void mainForm_ResizeEnd(object sender, EventArgs e)
        {
            if (this.Size == _prvSize) return;
            ResizePictureBox();
            CalculatingSize();
            CalculatingDrawBegin();
            DrawCurrentGeneration();
            _prvSize = this.Size;
        }

        private void mainForm_Shown(object sender, EventArgs e)
        {
            _prvSize = this.Size;
        }

        private void WorldHeightNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            _worldSize.Y = (int)WorldHeightNumericUpDown.Value;
            _gameEngine.ResizeWorld(_worldSize);
        }

        private void WorldWidthNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            _worldSize.X = (int)WorldWidthNumericUpDown.Value;
            _gameEngine.ResizeWorld(_worldSize);
        }

        private void nudDensity_ValueChanged(object sender, EventArgs e)
        {
            _gameEngine.FillRandom((int)nudDensity.Minimum + (int)nudDensity.Maximum - (int)nudDensity.Value);
            DrawCurrentGeneration();
        }

        private void pictureBox_SizeChanged(object sender, EventArgs e)
        {
            ResizePictureBox();
            CalculatingSize();
            CalculatingDrawBegin();
            DrawCurrentGeneration();
        }

        private void GridCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            DrawCurrentGeneration();
        }
        #endregion
    }
}
