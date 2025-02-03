using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameOfLife
{
    public partial class mainForm : Form
    {
        private Graphics _graphics;
        private int _resolution;
        private GameEngine _gameEngine;
        /// <summary>
        /// Матрица потоков
        /// </summary>
        private Thread[,] _workers;
        /// <summary>
        /// Максимальное количество потоков
        /// </summary>
        private int _threadsCount;
        /// <summary>
        /// Число строк в матрице потоков
        /// </summary>
        private int _threadsRows;
        /// <summary>
        /// Число столбцов в матрице потоков
        /// </summary>
        private int _threadsCols;
        public mainForm()
        {
            InitializeComponent();
            _threadsCount = Environment.ProcessorCount;
            //Находим оптимальный размер матрицы потоков, чтобы вдальнейшем удобно соотнести рендер изображений
            _threadsRows = _threadsCount;
            _threadsCols = 1;
            while (true)
            {
                if ((_threadsRows > _threadsCols) & (_threadsRows % 2 == 0))
                {
                    _threadsRows = _threadsRows / 2;
                    _threadsCols = _threadsCols * 2;
                }
                else break;
            }
            if (_threadsRows > _threadsCols)
            {
                int _temp = _threadsCols;
                _threadsCols = _threadsRows;
                _threadsRows = _temp;
            }
        }

        private void StartGame()
        {
            if (timer1.Enabled) return;
            bStart.Text = "Pause";
            nudResolution.Enabled = false;
            nudDensity.Enabled = false;
            _resolution = (int)nudResolution.Value;

            //Распараллеливание
            /*
            this.rows = rows;
            this.cols = cols;
            field = new bool[cols, rows];
            _workers = new List<Thread>();
            for (int i = 0; i < _threadsCount; i++)
            {
                int size = 2048 / _threadsCount;
                var start = i * size;
                var end = i == _threadsCount ? 2047 : start + size - 1;

                WorkerParams workerParams = new WorkerParams()
                {
                    StartIndex = start,
                    EndIndex = end
                };
                _workers.Add(new Thread(MWorker)
                {
                    IsBackground = true,
                    Name = i.ToString()
                });
                _workers[i].Start(workerParams);
            }*/

            _gameEngine = new GameEngine
                (
                    rows: pictureBox1.Height / _resolution,
                    cols: pictureBox1.Width / _resolution,
                    density: (int)nudDensity.Minimum + (int)nudDensity.Maximum - (int)nudDensity.Value
                );

            this.Text = $"Generation {_gameEngine.CurrentGeneration}";

            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            _graphics = Graphics.FromImage(pictureBox1.Image);
            timer1.Start();
            _gameEngine.Status = GameEngine.StatusEngine.run;
        }

        private void PauseGame()
        {
            if (!timer1.Enabled) return;
            bStart.Text = "Resume";
            timer1.Stop();
            _gameEngine.Status = GameEngine.StatusEngine.pause;
        }

        private void ResumeGame()
        {
            if (timer1.Enabled) return;
            bStart.Text = "Pause";
            timer1.Start();
            _gameEngine.Status = GameEngine.StatusEngine.run;
        }

        private void DrawNextGeneration()
        {
            _graphics.Clear(Color.Black);

            var field = _gameEngine.GetCurrentGeneration();

            for (int x = 0; x < field.GetLength(0); x++)
            {
                for (int y = 0; y < field.GetLength(1); y++)
                {
                    if (field[x, y])
                    {
                        _graphics.FillRectangle(Brushes.Crimson, x * _resolution, y * _resolution, _resolution - 1, _resolution - 1);
                    }
                }
            }

            pictureBox1.Refresh();
            this.Text = $"Generation {_gameEngine.CurrentGeneration}";
            _gameEngine.NextGeneration();
        }

        private void StopGame()
        {
            if (!timer1.Enabled) return;
            bStart.Text = "Start";
            timer1.Stop();
            nudResolution.Enabled = true;
            nudDensity.Enabled = true;
            _gameEngine.Status = GameEngine.StatusEngine.stop;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DrawNextGeneration();
        }

        private void bStart_Click(object sender, EventArgs e)
        {
            if (_gameEngine != null)
                switch (_gameEngine.Status)
                {
                    case GameEngine.StatusEngine.stop:
                        StartGame();
                        break;
                    case GameEngine.StatusEngine.run:
                        PauseGame();
                        break;
                    case GameEngine.StatusEngine.pause:
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

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!timer1.Enabled) return;
            if (e.Button == MouseButtons.Left)
            {
                var x = e.Location.X / _resolution;
                var y = e.Location.Y / _resolution;
                _gameEngine.AddCell(x, y);
            }
            if (e.Button == MouseButtons.Right)
            {
                var x = e.Location.X / _resolution;
                var y = e.Location.Y / _resolution;
                _gameEngine.RemoveCell(x, y);
            }
        }

        private void nudRefresh_ValueChanged(object sender, EventArgs e)
        {
            timer1.Interval = 1000 / (int)nudRefresh.Value;
        }
    }
}
