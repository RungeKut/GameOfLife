using System;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace GameOfLife
{
    public partial class mainForm : Form
    {
        /// <summary>
        /// Горизонтальный размер мира
        /// </summary>
        private int _worldWidth { get; set; }
        /// <summary>
        /// Вертикальный размер мира
        /// </summary>
        private int _worldHeight { get; set; }
        private Graphics _graphics { get; set; }
        private GameEngine _gameEngine { get; set; }
        private Size _prvSize { get; set; }
        private int _offsetWorldX { get; set; }
        private int _offsetWorldY { get; set; }
        private int _currentWorldX { get; set; }
        private int _currentWorldY { get; set; }
        private int _worldHeightDrawBegin { get; set; }
        private int _worldHeightDrawEnd { get; set; }
        private int _worldWidthDrawBegin { get; set; }
        private int _worldWidthDrawEnd { get; set; }
        private int _offsetWidth { get; set; }
        private int _offsetHeight { get; set; }
        private float _halfSizeWidth { get; set; }
        private float _halfSizeHeight { get; set; }
        private float _halfSizeAbroadCellWidth { get; set; }
        private float _halfSizeAbroadCellHeight { get; set; }
        private int _windowSizeWidth { get; set; }
        private int _windowSizeHeight { get; set; }
        /// <summary>
        /// Количество клеток, убираемое с каждой меньшей стороны (с большей стороны - сколько получится)
        /// </summary>
        private int _zoomCount { get; set; }
        /// <summary>
        /// Минимальное количество клеток, остающееся по меньшей стороне при максимальном увеличении
        /// </summary>
        private const int ZOOM_MAX = 64;
        /// <summary>
        /// Отношение ширины изображения к высоте
        /// </summary>
        private float _pictureAspectRatio { get; set; }

        public mainForm()
        {
            InitializeComponent();
            _zoomCount = 1;
            _gameEngine = new GameEngine();
            _pictureAspectRatio = (float)pictureBox.Width / (float)pictureBox.Height;
            pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);
            _graphics = Graphics.FromImage(pictureBox.Image);
            _worldHeight = (int)WorldHeightNumericUpDown.Value;
            _worldWidth = (int)WorldWidthNumericUpDown.Value;
            _gameEngine.ResizeWorld(_worldHeight, _worldWidth);
            ResizePictureBox();
            ResetZoom();
            pictureBox.MouseWheel += PictureBox_MouseWheel;
            CalculatingSize();
            UpdateFormTitle();
        }

        #region Отрисовка PictureBox

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            CalculatingScale(e);
            if (e.Button == MouseButtons.Left)
            {
                _gameEngine.AddCell(_offsetWorldX, _offsetWorldY);
                DrawCurrentGeneration();
            }
            if (e.Button == MouseButtons.Right)
            {
                _gameEngine.RemoveCell(_offsetWorldX, _offsetWorldY);
                DrawCurrentGeneration();
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
                if (x + _worldWidthDrawBegin >= 0) _worldX = (x + _worldWidthDrawBegin) % _worldWidth;
                else _worldX = _worldWidth + (x + _worldWidthDrawBegin) % _worldWidth;

                for (int y = -1; y < _windowSizeHeight + 1; y++)
                {
                    int _tempY = y * _zoomCount + (int)(_halfSizeAbroadCellHeight * _zoomCount);
                    int _worldY;
                    if (y + _worldHeightDrawBegin >= 0) _worldY = (y + _worldHeightDrawBegin) % _worldHeight;
                    else _worldY = _worldHeight + (y + _worldHeightDrawBegin) % _worldHeight;

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
            _pictureAspectRatio = (float)pictureBox.Width / (float)pictureBox.Height;
            pictureBox.Image.Dispose();
            pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);
            _graphics.Dispose();
            _graphics = Graphics.FromImage(pictureBox.Image);
            DrawCurrentGeneration();
        }

        private void CalculatingScale(MouseEventArgs e)
        {
            // Координаты мыши относительно центра изображения (пиксели)
            float _mouseOffsetWidth = e.Location.X - (float)pictureBox.Width / 2;
            float _mouseOffsetHeight = e.Location.Y - (float)pictureBox.Height / 2;
            // Координаты мыши относительно центра изображения с учетом масштаба и размера мира (количество клеток)
            float cellOffsetWidth = (float)(_mouseOffsetWidth > 0 ? 0.5 : -0.5);
            float cellOffsetHeight = (float)(_mouseOffsetHeight > 0 ? 0.5 : -0.5);
            _offsetWidth = (int)Math.Truncate((_mouseOffsetWidth / _zoomCount + cellOffsetWidth) % _worldWidth);
            _offsetHeight = (int)Math.Truncate((_mouseOffsetHeight / _zoomCount + cellOffsetHeight) % _worldHeight);
            // Координаты центра окна в мире
            if (_offsetWidth >= 0) _offsetWorldX = (_currentWorldX + _offsetWidth) % _worldWidth;
            else if (_currentWorldX >= -_offsetWidth) _offsetWorldX = (_currentWorldX + _offsetWidth) % _worldWidth;
            else _offsetWorldX = (_currentWorldX + (_worldWidth + _offsetWidth)) % _worldWidth;

            if (_offsetHeight >= 0) _offsetWorldY = (_currentWorldY + _offsetHeight) % _worldHeight;
            else if (_currentWorldY >= -_offsetHeight) _offsetWorldY = (_currentWorldY + _offsetHeight) % _worldHeight;
            else _offsetWorldY = (_currentWorldY + (_worldHeight + _offsetHeight)) % _worldHeight;
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
            _worldWidthDrawEnd = _worldWidth > pictureBox.Width ? pictureBox.Width : _worldWidth;
            _worldHeightDrawEnd = _worldHeight > pictureBox.Height ? pictureBox.Height : _worldHeight;
            _currentWorldX = _worldWidth / 2;
            _currentWorldY = _worldHeight / 2;
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

                _worldWidthDrawBegin = (_currentWorldX - (int)Math.Truncate(_halfSizeWidth)) % _worldWidth;
                _worldHeightDrawBegin = (_currentWorldY - (int)Math.Truncate(_halfSizeHeight)) % _worldHeight;

                DrawCurrentGeneration();
            }
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
                        _gameEngine.ResizeWorld(_worldHeight, _worldWidth);
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
            _prvSize = this.Size;
        }

        private void mainForm_Shown(object sender, EventArgs e)
        {
            _prvSize = this.Size;
        }

        private void WorldHeightNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            _worldHeight = (int)WorldHeightNumericUpDown.Value;
            _gameEngine.ResizeWorld(_worldHeight, _worldWidth);
        }

        private void WorldWidthNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            _worldWidth = (int)WorldWidthNumericUpDown.Value;
            _gameEngine.ResizeWorld(_worldHeight, _worldWidth);
        }

        private void nudDensity_ValueChanged(object sender, EventArgs e)
        {
            _gameEngine.FillRandom((int)nudDensity.Minimum + (int)nudDensity.Maximum - (int)nudDensity.Value);
            DrawCurrentGeneration();
        }
        #endregion
    }
}
