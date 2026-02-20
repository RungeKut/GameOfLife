using GameOfLife;
using System;
using System.Drawing;
using System.Windows.Forms;

public class WallpaperForm : Form
{
    private GameEngine _gameEngine;
    private MonitorInfo _monitor;
    private Bitmap _wallpaperBitmap;
    private Graphics _wallpaperGraphics;
    private int _cellSize;
    private Point2D _worldSize;
    private bool _isDisposed = false;
    private bool _showWorldBorder = true;  // ✅ Добавлено

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

        this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        this.SetStyle(ControlStyles.UserPaint, true);
        this.DoubleBuffered = true;

        this.Bounds = monitor.Screen.Bounds;
    }

    // ✅ Метод для установки видимости границ
    public void SetWorldBorderVisibility(bool visible)
    {
        _showWorldBorder = visible;
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

        int offsetX = _monitor.X / zoomCount;
        int offsetY = _monitor.Y / zoomCount;

        for (int x = -1; x < windowSizeWidth + 1; x++)
        {
            int tempX = x * zoomCount + (int)(halfSizeAbroadCellWidth * zoomCount);
            int globalX = x + worldWidthDrawBegin + offsetX;
            int worldX = ((globalX % (int)_worldSize.X) + (int)_worldSize.X) % (int)_worldSize.X;

            for (int y = -1; y < windowSizeHeight + 1; y++)
            {
                int tempY = y * zoomCount + (int)(halfSizeAbroadCellHeight * zoomCount);
                int globalY = y + worldHeightDrawBegin + offsetY;
                int worldY = ((globalY % (int)_worldSize.Y) + (int)_worldSize.Y) % (int)_worldSize.Y;

                if (worldX >= 0 && worldX < field.GetLength(0) &&
                    worldY >= 0 && worldY < field.GetLength(1))
                {
                    if (field[worldX, worldY])
                    {
                        Color cellColor = Color.Crimson;
                        var genome = _gameEngine.GetCellGenome(worldX, worldY);
                        if (genome != null)
                            cellColor = genome.GenomeColor;

                        using (Brush brush = new SolidBrush(cellColor))
                        {
                            if (zoomCount > 1)
                                _wallpaperGraphics.FillRectangle(brush, tempX + 1, tempY + 1, zoomCount - 1, zoomCount - 1);
                            else
                                _wallpaperGraphics.FillRectangle(brush, tempX, tempY, 1, 1);
                        }
                    }
                }
            }
        }

        // ✅ ОТРИСОВКА ГРАНИЦ МИРА
        DrawWorldBorder(_wallpaperGraphics, zoomCount, halfSizeAbroadCellWidth, halfSizeAbroadCellHeight, offsetX, offsetY);

        this.Invalidate();
    }

    // ✅ НОВЫЙ МЕТОД: Отрисовка границ мира на обоях (сетка, 1 пиксель)
    private void DrawWorldBorder(Graphics g, int zoomCount,
        float halfSizeAbroadCellWidth, float halfSizeAbroadCellHeight,
        int offsetX, int offsetY)
    {
        using (Pen borderPen = new Pen(Color.White, 1))  // ✅ ТОЛЩИНА 1 ПИКСЕЛЬ
        {
            int worldWidthPixels = (int)_worldSize.X * zoomCount;
            int worldHeightPixels = (int)_worldSize.Y * zoomCount;

            int startX = (int)(halfSizeAbroadCellWidth * zoomCount) + offsetX;
            int startY = (int)(halfSizeAbroadCellHeight * zoomCount) + offsetY;

            // Нормализуем
            startX = startX % worldWidthPixels;
            if (startX < 0) startX += worldWidthPixels;

            startY = startY % worldHeightPixels;
            if (startY < 0) startY += worldHeightPixels;

            startX -= worldWidthPixels;
            startY -= worldHeightPixels;

            // Горизонтальные линии
            for (int y = startY; y < _monitor.Screen.Bounds.Height; y += worldHeightPixels)
            {
                if (y >= 0)
                    g.DrawLine(borderPen, 0, y, _monitor.Screen.Bounds.Width, y);
            }

            // Вертикальные линии
            for (int x = startX; x < _monitor.Screen.Bounds.Width; x += worldWidthPixels)
            {
                if (x >= 0)
                    g.DrawLine(borderPen, x, 0, x, _monitor.Screen.Bounds.Height);
            }
        }
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