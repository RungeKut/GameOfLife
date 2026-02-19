using System;
using System.Drawing;
using System.Windows.Forms;

namespace GameOfLife
{
    public class WallpaperForm : Form
    {
        private GameEngine _gameEngine;
        private MonitorInfo _monitor;
        private Bitmap _wallpaperBitmap;
        private Graphics _wallpaperGraphics;
        private int _cellSize;
        private Point2D _worldSize;
        private bool _isDisposed = false;

        public WallpaperForm(GameEngine gameEngine, MonitorInfo monitor, int cellSize, Point2D worldSize)
        {
            _gameEngine = gameEngine;
            _monitor = monitor;
            _cellSize = cellSize;
            _worldSize = worldSize;

            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = false;
            this.ShowInTaskbar = false;
            this.ControlBox = false;
            this.BackColor = Color.Black;

            // Двойная буферизация
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.DoubleBuffered = true;

            // Устанавливаем размер и позицию под монитор
            this.Bounds = monitor.Screen.Bounds;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_wallpaperBitmap != null && !_isDisposed)
            {
                e.Graphics.DrawImageUnscaled(_wallpaperBitmap, 0, 0);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Пусто - рисуем сами в OnPaint
        }

        public void InitializeBitmap()
        {
            if (_wallpaperBitmap != null)
            {
                _wallpaperBitmap.Dispose();
                _wallpaperGraphics?.Dispose();
            }

            _wallpaperBitmap = new Bitmap(_monitor.Screen.Bounds.Width, _monitor.Screen.Bounds.Height);
            _wallpaperGraphics = Graphics.FromImage(_wallpaperBitmap);

            // Оптимизации для скорости
            _wallpaperGraphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            _wallpaperGraphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            _wallpaperGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            _wallpaperGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
        }

        public void DrawGeneration(int zoomCount, int worldWidthDrawBegin, int worldHeightDrawBegin,
            float halfSizeAbroadCellWidth, float halfSizeAbroadCellHeight)
        {
            if (_wallpaperGraphics == null || _isDisposed)
                return;

            _wallpaperGraphics.Clear(Color.Black);

            var field = _gameEngine.GetCurrentGeneration();
            if (field == null)
                return;

            int windowSizeWidth = _monitor.Screen.Bounds.Width / zoomCount;
            int windowSizeHeight = _monitor.Screen.Bounds.Height / zoomCount;

            // Смещение начала отрисовки с учётом позиции монитора в виртуальном столе
            int offsetX = _monitor.X / zoomCount;
            int offsetY = _monitor.Y / zoomCount;

            // Рисуем клетки только для этого монитора
            for (int x = -1; x < windowSizeWidth + 1; x++)
            {
                int tempX = x * zoomCount + (int)(halfSizeAbroadCellWidth * zoomCount);

                // Глобальная координата в мире с учётом позиции монитора
                int globalX = x + worldWidthDrawBegin + offsetX;
                // Нормализуем для зацикленного мира
                int worldX = ((globalX % (int)_worldSize.X) + (int)_worldSize.X) % (int)_worldSize.X;

                for (int y = -1; y < windowSizeHeight + 1; y++)
                {
                    int tempY = y * zoomCount + (int)(halfSizeAbroadCellHeight * zoomCount);

                    int globalY = y + worldHeightDrawBegin + offsetY;
                    int worldY = ((globalY % (int)_worldSize.Y) + (int)_worldSize.Y) % (int)_worldSize.Y;

                    // Проверка границ массива
                    if (worldX >= 0 && worldX < field.GetLength(0) &&
                        worldY >= 0 && worldY < field.GetLength(1))
                    {
                        if (field[worldX, worldY])
                        {
                            if (zoomCount > 1)
                                _wallpaperGraphics.FillRectangle(Brushes.Crimson, tempX + 1, tempY + 1, zoomCount - 1, zoomCount - 1);
                            else
                                _wallpaperGraphics.FillRectangle(Brushes.Crimson, tempX, tempY, 1, 1);
                        }
                    }
                }
            }

            this.Invalidate();
        }

        public void ShowWallpaper()
        {
            this.Show();
            Application.DoEvents();
            System.Threading.Thread.Sleep(100);

            IntPtr workerW = WallpaperHelper.GetWorkerW();
            if (workerW != IntPtr.Zero)
            {
                WallpaperHelper.SetParent(this.Handle, workerW);
                WallpaperHelper.MoveWindow(this.Handle,
                    _monitor.Screen.Bounds.X,
                    _monitor.Screen.Bounds.Y,
                    _monitor.Screen.Bounds.Width,
                    _monitor.Screen.Bounds.Height,
                    true);
                WallpaperHelper.ShowWindow(this.Handle, 5);
            }

            Application.DoEvents();
        }

        public void HideWallpaper()
        {
            WallpaperHelper.SetParent(this.Handle, IntPtr.Zero);
            this.Hide();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _isDisposed = true;
            _wallpaperBitmap?.Dispose();
            _wallpaperGraphics?.Dispose();
            base.OnFormClosing(e);
        }

        public void DisposeResources()
        {
            _isDisposed = true;
            _wallpaperBitmap?.Dispose();
            _wallpaperGraphics?.Dispose();
        }
    }
}