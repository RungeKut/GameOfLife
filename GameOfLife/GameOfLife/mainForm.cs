using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace GameOfLife
{
    public partial class mainForm : Form
    {
        private Graphics _graphics;
        private int _resolution;
        private GameEngine _gameEngine;
        private Size _prvSize;
        private int _zoomX;
        private int _zoomY;
        private int _pictureHeight;
        private int _pictureWidth;
        private int _zoomCount;
        private const int ZOOM_MAX = 500;
        public mainForm()
        {
            InitializeComponent();
            _resolution = (int)nudResolution.Value;

            _gameEngine = new GameEngine();

            _pictureHeight = pictureBox.Height;
            _pictureWidth = pictureBox.Width;

            this.Text = $"Generation {_gameEngine.CurrentGeneration}";
            _gameEngine.ResizeWorld
                        (
                            rows: (ulong)pictureBox.Height / (ulong)_resolution,
                            cols: (ulong)pictureBox.Width / (ulong)_resolution
                        );
            ResizePictureBox();
            pictureBox.MouseWheel += PictureBox_MouseWheel;
        }

        #region Отрисовка PictureBox
        private void DrawCurrentGeneration()
        {
            pictureBox.SuspendLayout();
            _graphics.Clear(Color.Black);

            var field = _gameEngine.GetCurrentGeneration();

            for (ulong x = 0; x < _gameEngine.Cols; x++)
            {
                for (ulong y = 0; y < (ulong)_pictureHeight; y++)
                {
                    if (field[x, y])
                    {
                        if (_resolution > 1)
                            _graphics.FillRectangle(Brushes.Crimson, (int)x * _resolution, (int)y * _resolution, _resolution - 1, _resolution - 1);
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
            _zoomX = pictureBox.Width / 2;
            _zoomY = pictureBox.Height / 2;
            _zoomCount = 0;
            pictureBox.Image = new Bitmap(_pictureWidth, _pictureHeight);
            _graphics = Graphics.FromImage(pictureBox.Image);
            DrawCurrentGeneration();
        }

        
        private void PictureBox_MouseWheel(object sender, MouseEventArgs e) // Событие вращения колеса
        {
            _zoomX = e.Location.X;
            _zoomY = e.Location.Y;
            if (e.Delta > 0) // Колесико вверх
            {
                _zoomCount++;
            }
            else // Колесико вниз
            {
                _zoomCount--;
            }
            if (_zoomCount < 0) { _zoomCount = 0; return; }
            if (_zoomCount > Math.Min(pictureBox.Height, pictureBox.Width) - ZOOM_MAX) { _zoomCount = ZOOM_MAX; return; }

            _pictureHeight = (int)_gameEngine.Rows * _resolution - _zoomCount * pictureBox.Height / pictureBox.Width;
            _pictureWidth = (int)_gameEngine.Cols * _resolution - _zoomCount;

            ResizePictureBox();
        }
        #endregion

        #region Управление
        private void StartGame()
        {
            if (timer1.Enabled) return;
            bStart.Text = "Pause";
            nudResolution.Enabled = false;
            nudDensity.Enabled = false;
            _resolution = (int)nudResolution.Value;

            timer1.Start();
            _gameEngine._statusEngine = StatusEngine.run;
        }

        private void PauseGame()
        {
            if (!timer1.Enabled) return;
            bStart.Text = "Resume";
            timer1.Stop();
            _gameEngine._statusEngine = StatusEngine.pause;
        }

        private void ResumeGame()
        {
            if (timer1.Enabled) return;
            bStart.Text = "Pause";
            timer1.Start();
            _gameEngine._statusEngine = StatusEngine.run;
        }

        private void StopGame()
        {
            if (!timer1.Enabled) return;
            bStart.Text = "Start";
            timer1.Stop();
            nudResolution.Enabled = true;
            nudDensity.Enabled = true;
            _gameEngine._statusEngine = StatusEngine.stop;
        }
        #endregion

        private void timer1_Tick(object sender, EventArgs e)
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
            if (e.Button == MouseButtons.Left)
            {
                ulong x = (ulong)e.Location.X / (ulong)_resolution;
                ulong y = (ulong)e.Location.Y / (ulong)_resolution;
                _gameEngine.AddCell(x, y);
            }
            if (e.Button == MouseButtons.Right)
            {
                ulong x = (ulong)e.Location.X / (ulong)_resolution;
                ulong y = (ulong)e.Location.Y / (ulong)_resolution;
                _gameEngine.RemoveCell(x, y);
            }
            DrawCurrentGeneration();
        }
        private void nudRefresh_ValueChanged(object sender, EventArgs e)
        {
            timer1.Interval = 1000 / (int)nudRefresh.Value;
        }

        private void bRnd_Click(object sender, EventArgs e)
        {
            _gameEngine.ResizeWorld
                        (
                            rows: (ulong)pictureBox.Height / (ulong)_resolution,
                            cols: (ulong)pictureBox.Width / (ulong)_resolution
                        );
            ResizePictureBox();
            _gameEngine.FillRandom((int)nudDensity.Minimum + (int)nudDensity.Maximum - (int)nudDensity.Value);
            DrawCurrentGeneration();
        }

        private void mainForm_ResizeEnd(object sender, EventArgs e)
        {
            if (this.Size == _prvSize) return;
            _gameEngine.ResizeWorld
                        (
                            rows: (ulong)pictureBox.Height / (ulong)_resolution,
                            cols: (ulong)pictureBox.Width / (ulong)_resolution
                        );
            ResizePictureBox();
            _prvSize = this.Size;
        }

        private void nudResolution_ValueChanged(object sender, EventArgs e)
        {
            _resolution = (int)nudResolution.Value;
        }

        private void mainForm_Shown(object sender, EventArgs e)
        {
            _prvSize = this.Size;
        }
        #endregion
    }
}
