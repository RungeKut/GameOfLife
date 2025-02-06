using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace GameOfLife
{
    public partial class mainForm : Form
    {
        private int _worldHeight;
        private int _worldWidht;
        private Graphics _graphics;
        private GameEngine _gameEngine;
        private Size _prvSize;
        private int _currentZoomX;
        private int _currentZoomY;
        private int _lastZoomX;
        private int _lastZoomY;
        private int _worldHeightDrawBegin = 0;
        private int _worldHeightDrawEnd;
        private int _worldWidthDrawBegin = 0;
        private int _worldWidthDrawEnd;
        private int _pictureHeight { get { return _worldHeight/_zoomCount; } }
        private int _pictureWidth { get { return _worldWidht/_zoomCount; } }
        private int _zoomCount = 1;
        private const int ZOOM_MAX = 100; // Это максимальная кратность увеличения _resolution

        public mainForm()
        {
            InitializeComponent();

            _gameEngine = new GameEngine();

            _worldHeight = (int)WorldHeightNumericUpDown.Value;
            _worldWidht = (int)WorldWidthNumericUpDown.Value;
            _gameEngine.ResizeWorld(_worldHeight, _worldWidht);
            this.Text = $"Generation {_gameEngine.CurrentGeneration}";
            ResizePictureBox();
            _worldHeightDrawEnd = _worldHeight > pictureBox.Height ? pictureBox.Height : _worldHeight;
            _worldWidthDrawEnd = _worldWidht > pictureBox.Width ? pictureBox.Width : _worldWidht;
            _currentZoomX = pictureBox.Width / 2;
            _currentZoomY = pictureBox.Height / 2;
            pictureBox.MouseWheel += PictureBox_MouseWheel;
        }

        #region Отрисовка PictureBox
        private void DrawCurrentGeneration()
        {
            pictureBox.SuspendLayout();
            _graphics.Clear(Color.Black);

            var field = _gameEngine.GetCurrentGeneration();

            for (int x = _worldWidthDrawBegin; x < _worldWidthDrawEnd; x++)
            {
                for (int y = _worldHeightDrawBegin; y < _worldHeightDrawEnd; y++)
                {
                    if (field[x, y])
                    {
                        if (_zoomCount > 1)
                            _graphics.FillRectangle(Brushes.Crimson, (int)x * _zoomCount, (int)y * _zoomCount, _zoomCount - 1, _zoomCount - 1);
                        else
                            _graphics.FillRectangle(Brushes.Crimson, (int)x, (int)y, 1, 1);
                    }
                }
            }
            pictureBox.ResumeLayout();
            pictureBox.Refresh();
        }

        private void ResizePictureBox()
        {
            pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);
            _graphics = Graphics.FromImage(pictureBox.Image);
            DrawCurrentGeneration();
        }

        
        private void PictureBox_MouseWheel(object sender, MouseEventArgs e) // Событие вращения колеса
        {
            
            int _offsetWidth = e.Location.X - (pictureBox.Width / 2) / _zoomCount;
            int _offsetHeight = e.Location.Y - (pictureBox.Height / 2) / _zoomCount;

            _worldHeightDrawBegin += _offsetHeight;
            _worldHeightDrawEnd += _offsetHeight;
            _worldWidthDrawBegin += _offsetWidth;
            _worldWidthDrawEnd += _offsetWidth;

            _lastZoomX = _currentZoomX;
            _lastZoomY = _currentZoomY;

            if (e.Delta > 0) // Колесико вверх
            {
                _zoomCount = _zoomCount * 2;
                if (_zoomCount > ZOOM_MAX) { _zoomCount = ZOOM_MAX; return; }
                _worldHeightDrawBegin += (pictureBox.Height / 2) / _zoomCount;
                _worldHeightDrawEnd -= (pictureBox.Height / 2) / _zoomCount;
                _worldWidthDrawBegin += (pictureBox.Width / 2) / _zoomCount;
                _worldWidthDrawEnd -= (pictureBox.Width / 2) / _zoomCount;
            }
            else // Колесико вниз
            {
                _zoomCount = _zoomCount / 2;
                if (_zoomCount < 1) { _zoomCount = 1; return; }
                _worldHeightDrawBegin -= (pictureBox.Height / 2) / _zoomCount;
                _worldHeightDrawEnd += (pictureBox.Height / 2) / _zoomCount;
                _worldWidthDrawBegin -= (pictureBox.Width / 2) / _zoomCount;
                _worldWidthDrawEnd += (pictureBox.Width / 2) / _zoomCount;
            }
            ResizePictureBox();
        }
        #endregion

        #region Управление
        private void StartGame()
        {
            if (timer.Enabled) return;
            bStart.Text = "Pause";
            nudDensity.Enabled = false;

            timer.Start();
            _gameEngine._statusEngine = StatusEngine.run;
        }

        private void PauseGame()
        {
            if (!timer.Enabled) return;
            bStart.Text = "Resume";
            timer.Stop();
            _gameEngine._statusEngine = StatusEngine.pause;
        }

        private void ResumeGame()
        {
            if (timer.Enabled) return;
            bStart.Text = "Pause";
            timer.Start();
            _gameEngine._statusEngine = StatusEngine.run;
        }

        private void StopGame()
        {
            if (!timer.Enabled) return;
            bStart.Text = "Start";
            timer.Stop();
            nudDensity.Enabled = true;
            _gameEngine._statusEngine = StatusEngine.stop;
        }
        #endregion

        private void timer_Tick(object sender, EventArgs e)
        {
            DrawCurrentGeneration();
            this.Text = $"Generation {_gameEngine.CurrentGeneration}";
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
            StopGame();
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            //if (e.Button == MouseButtons.Left)
            //{
            //    ulong x = (ulong)e.Location.X / (ulong)_resolution;
            //    ulong y = (ulong)e.Location.Y / (ulong)_resolution;
            //    _gameEngine.AddCell(x, y);
            //}
            //if (e.Button == MouseButtons.Right)
            //{
            //    ulong x = (ulong)e.Location.X / (ulong)_resolution;
            //    ulong y = (ulong)e.Location.Y / (ulong)_resolution;
            //    _gameEngine.RemoveCell(x, y);
            //}
            //DrawCurrentGeneration();
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
            _gameEngine.ResizeWorld(_worldHeight, _worldWidht);
        }

        private void WorldWidthNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            _worldWidht = (int)WorldWidthNumericUpDown.Value;
            _gameEngine.ResizeWorld(_worldHeight, _worldWidht);
        }

        private void nudDensity_ValueChanged(object sender, EventArgs e)
        {
            _gameEngine.FillRandom((int)nudDensity.Minimum + (int)nudDensity.Maximum - (int)nudDensity.Value);
            DrawCurrentGeneration();
        }
        #endregion
    }
}
