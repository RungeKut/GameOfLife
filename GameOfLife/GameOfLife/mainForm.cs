using System;
using System.Drawing;
using System.Windows.Forms;

namespace GameOfLife
{
    public partial class mainForm : Form
    {
        private Point2D _worldSize { get; set; }
        private Graphics _graphics { get; set; }
        internal GameEngine _gameEngine { get; set; }
        private Size _prvSize { get; set; }
        private int _offsetWorldX { get; set; }
        private int _offsetWorldY { get; set; }
        private int _currentWorldX { get; set; }
        private int _currentWorldY { get; set; }
        private int _worldHeightDrawBegin { get; set; }
        private int _worldWidthDrawBegin { get; set; }
        private int _offsetWidth { get; set; }
        private int _offsetHeight { get; set; }
        private float _halfSizeWidth { get; set; }
        private float _halfSizeHeight { get; set; }
        private float _halfSizeAbroadCellWidth { get; set; }
        private float _halfSizeAbroadCellHeight { get; set; }
        private int _windowSizeWidth { get; set; }
        private int _windowSizeHeight { get; set; }
        private int _zoomCount { get; set; }
        private const int ZOOM_MAX = 64;

        // === ПОЛЯ ДЛЯ ТРЕЯ И ОБОЕВ ===
        private TrayManager _trayManager { get; set; }
        private bool _isWallpaperMode { get; set; } = false;
        private bool _isControlPanelVisible { get; set; } = true;
        private Control[] _controlPanelControls { get; set; }
        private MenuStrip _mainMenu { get; set; }

        // === НОВОЕ: Bitmap для быстрой отрисовки в режиме обоев ===
        private Bitmap _wallpaperBitmap { get; set; }
        private Graphics _wallpaperGraphics { get; set; }

        public GameEngine GetGameEngine() => _gameEngine;

        public mainForm()
        {
            InitializeComponent();

            // ✅ КРИТИЧНО: Включаем двойную буферизацию для устранения мерцания
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.DoubleBuffered = true;

            this.BackColor = Color.Black;
            pictureBox.BackColor = Color.Black;
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.Visible = true;

            SaveControlPanelReferences();

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

            GridCheckBox.Checked = false;

            _trayManager = new TrayManager(this);

            try
            {
                Icon appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                if (appIcon != null)
                    _trayManager.SetIcon(appIcon);
            }
            catch { }

            _trayManager.ShowBalloonTip("Game of Life",
                "Приложение запущено. Нажмите правой кнопкой на иконку в трее для управления.",
                ToolTipIcon.Info, 3000);
        }

        private void SaveControlPanelReferences()
        {
            var controls = new System.Collections.Generic.List<Control>();
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl != pictureBox && !(ctrl is MenuStrip) && !(ctrl is StatusStrip))
                {
                    controls.Add(ctrl);
                }
            }
            _controlPanelControls = controls.ToArray();

            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is MenuStrip menu)
                {
                    _mainMenu = menu;
                    break;
                }
            }

            _isControlPanelVisible = true;
        }

        // ✅ Переопределяем OnPaint для быстрой отрисовки Bitmap
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_isWallpaperMode && _wallpaperBitmap != null)
            {
                // ✅ Копируем готовый Bitmap на экран одним вызовом
                e.Graphics.DrawImageUnscaled(_wallpaperBitmap, 0, 0);
            }
        }

        // ✅ Отключаем стандартную очистку фона
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Пусто - рисуем сами в OnPaint
        }

        #region Отрисовка

        private void DrawCurrentGeneration()
        {
            if (_isWallpaperMode)
            {
                DrawToWallpaperBitmap();
            }
            else
            {
                DrawToPictureBox();
            }
        }

        private void DrawToPictureBox()
        {
            if (pictureBox == null || pictureBox.Image == null || _graphics == null)
                return;

            pictureBox.SuspendLayout();

            try
            {
                _graphics.Clear(Color.Black);
                DrawCells(_graphics, pictureBox.Width, pictureBox.Height);
            }
            finally
            {
                pictureBox.ResumeLayout();
            }

            UpdateFormTitle();
            pictureBox.Refresh();
        }

        // ✅ НОВЫЙ МЕТОД: Отрисовка в Bitmap для режима обоев
        private void DrawToWallpaperBitmap()
        {
            // Создаём Bitmap если нет или размер изменился
            if (_wallpaperBitmap == null ||
                _wallpaperBitmap.Width != this.Width ||
                _wallpaperBitmap.Height != this.Height)
            {
                _wallpaperBitmap?.Dispose();
                _wallpaperGraphics?.Dispose();

                _wallpaperBitmap = new Bitmap(this.Width, this.Height);
                _wallpaperGraphics = Graphics.FromImage(_wallpaperBitmap);

                // ✅ Включаем сглаживание для скорости
                _wallpaperGraphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                _wallpaperGraphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                _wallpaperGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                _wallpaperGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            }

            // ✅ Рисуем всё поколение в память
            _wallpaperGraphics.Clear(Color.Black);
            DrawCells(_wallpaperGraphics, this.Width, this.Height);

            // ✅ Обновляем форму (копирует Bitmap на экран одним вызовом)
            this.Invalidate();
        }

        private void DrawCells(Graphics g, int width, int height)
        {
            var _field = _gameEngine.GetCurrentGeneration();
            if (_field == null)
                return;

            // Пересчитываем параметры для текущего размера
            int windowSizeWidth = width / _zoomCount;
            int windowSizeHeight = height / _zoomCount;
            float halfSizeAbroadCellWidth = Truncate((float)width / _zoomCount) / 2;
            float halfSizeAbroadCellHeight = Truncate((float)height / _zoomCount) / 2;

            // Сетка (только не в режиме обоев)
            if (GridCheckBox.Checked && !_isWallpaperMode)
            {
                using (Pen _style = new Pen(Color.DarkGray, 1))
                {
                    for (int x = 0; x < windowSizeWidth + 1; x++)
                    {
                        int _tempX = x * _zoomCount + (int)(halfSizeAbroadCellWidth * _zoomCount);
                        g.DrawLine(_style, _tempX, 0, _tempX, height);
                    }
                    for (int y = 0; y < windowSizeHeight + 1; y++)
                    {
                        int _tempY = y * _zoomCount + (int)(halfSizeAbroadCellHeight * _zoomCount);
                        g.DrawLine(_style, 0, _tempY, width, _tempY);
                    }
                }
            }

            // ✅ Клетки - рисуем напрямую в Graphics
            for (int x = -1; x < windowSizeWidth + 1; x++)
            {
                int _tempX = x * _zoomCount + (int)(halfSizeAbroadCellWidth * _zoomCount);
                int _worldX;
                if (x + _worldWidthDrawBegin >= 0)
                    _worldX = (int)((x + _worldWidthDrawBegin) % _worldSize.X);
                else
                    _worldX = (int)((_worldSize.X + x + _worldWidthDrawBegin) % _worldSize.X);

                for (int y = -1; y < windowSizeHeight + 1; y++)
                {
                    int _tempY = y * _zoomCount + (int)(halfSizeAbroadCellHeight * _zoomCount);
                    int _worldY;
                    if (y + _worldHeightDrawBegin >= 0)
                        _worldY = (int)((y + _worldHeightDrawBegin) % _worldSize.Y);
                    else
                        _worldY = (int)((_worldSize.Y + y + _worldHeightDrawBegin) % _worldSize.Y);

                    if (_field[_worldX, _worldY])
                    {
                        if (_zoomCount > 1)
                            g.FillRectangle(Brushes.Crimson, _tempX + 1, _tempY + 1, _zoomCount - 1, _zoomCount - 1);
                        else
                            g.FillRectangle(Brushes.Crimson, _tempX, _tempY, 1, 1);
                    }
                }
            }
        }

        private float Truncate(float a) { return a - (float)Math.Truncate(a); }

        private void UpdateFormTitle()
        {
            if (!_isWallpaperMode)
            {
                this.Text = $"Generation:{_gameEngine.CurrentGeneration} Zoom:{_zoomCount} world_X:{_offsetWorldX} world_Y:{_offsetWorldY}";
            }
        }

        private void ResizePictureBox()
        {
            if (pictureBox.Image != null)
            {
                pictureBox.Image.Dispose();
            }
            pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);

            if (_graphics != null)
            {
                _graphics.Dispose();
            }
            _graphics = Graphics.FromImage(pictureBox.Image);
        }

        private void CalculatingScale(MouseEventArgs e)
        {
            float _mouseOffsetWidth = e.Location.X - (float)pictureBox.Width / 2;
            float _mouseOffsetHeight = e.Location.Y - (float)pictureBox.Height / 2;
            float cellOffsetWidth = (float)(_mouseOffsetWidth > 0 ? 0.5 : -0.5);
            float cellOffsetHeight = (float)(_mouseOffsetHeight > 0 ? 0.5 : -0.5);
            _offsetWidth = (int)Math.Truncate((_mouseOffsetWidth / _zoomCount + cellOffsetWidth) % _worldSize.X);
            _offsetHeight = (int)Math.Truncate((_mouseOffsetHeight / _zoomCount + cellOffsetHeight) % _worldSize.Y);

            if (_offsetWidth >= 0) _offsetWorldX = (int)((_currentWorldX + _offsetWidth) % _worldSize.X);
            else if (_currentWorldX >= -_offsetWidth) _offsetWorldX = (int)((_currentWorldX + _offsetWidth) % _worldSize.X);
            else _offsetWorldX = (int)((_currentWorldX + (_worldSize.X + _offsetWidth)) % _worldSize.X);

            if (_offsetHeight >= 0) _offsetWorldY = (int)((_currentWorldY + _offsetHeight) % _worldSize.Y);
            else if (_currentWorldY >= -_offsetHeight) _offsetWorldY = (int)((_currentWorldY + _offsetHeight) % _worldSize.Y);
            else _offsetWorldY = (int)((_currentWorldY + (_worldSize.Y + _offsetHeight)) % _worldSize.Y);
        }

        private void CalculatingSize()
        {
            _halfSizeWidth = (float)pictureBox.Width / (2 * _zoomCount);
            _halfSizeHeight = (float)pictureBox.Height / (2 * _zoomCount);
            _windowSizeWidth = pictureBox.Width / _zoomCount;
            _windowSizeHeight = pictureBox.Height / _zoomCount;
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

        private void PictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            if (_isWallpaperMode) return;

            bool update = false;
            if (e.Delta > 0)
            {
                _zoomCount = _zoomCount << 1;
                if (_zoomCount > ZOOM_MAX) { _zoomCount = ZOOM_MAX; }
                else update = true;
            }
            else
            {
                _zoomCount = _zoomCount >> 1;
                if (_zoomCount < 1) { _zoomCount = 1; }
                else update = true;
            }
            if (update)
            {
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
            bStart.Text = "Pause ";
            bStop.Text = "Stop ";
            nudDensity.Enabled = false;
            timer.Start();
            _gameEngine._statusEngine = StatusEngine.run;
        }

        private void PauseGame()
        {
            if (!timer.Enabled) return;
            bStart.Text = "Resume ";
            bStop.Text = "Reset ";
            timer.Stop();
            _gameEngine._statusEngine = StatusEngine.pause;
        }

        private void ResumeGame()
        {
            if (timer.Enabled) return;
            bStart.Text = "Pause ";
            bStop.Text = "Stop ";
            timer.Start();
            _gameEngine._statusEngine = StatusEngine.run;
        }

        private void StopGame()
        {
            if (!timer.Enabled) return;
            bStart.Text = "Start ";
            bStop.Text = "Reset ";
            timer.Stop();
            nudDensity.Enabled = true;
            _gameEngine._statusEngine = StatusEngine.stop;
        }
        #endregion

        private void timer_Tick(object sender, EventArgs e)
        {
            _gameEngine.NextGeneration();
            DrawCurrentGeneration();

            if (_gameEngine.CurrentGeneration % 10 == 0)
            {
                _trayManager.UpdateMenuState();
            }
        }

        #region Обработчики событий формы
        private void bStart_Click(object sender, EventArgs e)
        {
            if (_gameEngine != null)
                switch (_gameEngine._statusEngine)
                {
                    case StatusEngine.stop: StartGame(); break;
                    case StatusEngine.run: PauseGame(); break;
                    case StatusEngine.pause: ResumeGame(); break;
                }
            else StartGame();
        }

        private void bStop_Click(object sender, EventArgs e)
        {
            if (_gameEngine != null)
                switch (_gameEngine._statusEngine)
                {
                    case StatusEngine.run: StopGame(); break;
                    default: _gameEngine.ResizeWorld(_worldSize); DrawCurrentGeneration(); break;
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

        #region Методы для TrayManager
        public void TriggerStartPause()
        {
            if (_gameEngine != null)
            {
                switch (_gameEngine._statusEngine)
                {
                    case StatusEngine.stop: StartGame(); break;
                    case StatusEngine.run: PauseGame(); break;
                    case StatusEngine.pause: ResumeGame(); break;
                }
            }
        }

        public void TriggerStop()
        {
            if (_gameEngine != null)
            {
                switch (_gameEngine._statusEngine)
                {
                    case StatusEngine.run: StopGame(); break;
                    default: _gameEngine.ResizeWorld(_worldSize); DrawCurrentGeneration(); break;
                }
            }
        }

        public void TriggerRandom()
        {
            _gameEngine.FillRandom((int)nudDensity.Minimum + (int)nudDensity.Maximum - (int)nudDensity.Value);
            DrawCurrentGeneration();
        }

        public void ToggleWallpaperMode()
        {
            _isWallpaperMode = !_isWallpaperMode;
            if (_isWallpaperMode)
            {
                EnableWallpaperMode();
            }
            else
            {
                DisableWallpaperMode();
            }
            _trayManager.UpdateMenuState();
        }

        public void ToggleControlPanel()
        {
            _isControlPanelVisible = !_isControlPanelVisible;
            UpdateControlPanelVisibility();
            _trayManager.UpdateMenuState();
        }

        public void CloseApplication()
        {
            _trayManager?.ShowBalloonTip("Game of Life", "Приложение закрывается...", ToolTipIcon.Info, 1000);
            this.Close();
        }

        public bool IsWallpaperMode => _isWallpaperMode;
        public bool IsControlPanelVisible => _isControlPanelVisible;

        private void EnableWallpaperMode()
        {
            _isWallpaperMode = true;

            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = false;
            this.ShowInTaskbar = false;
            this.ControlBox = false;

            // ✅ Создаём Bitmap для обоев
            _wallpaperBitmap = new Bitmap(this.Width, this.Height);
            _wallpaperGraphics = Graphics.FromImage(_wallpaperBitmap);

            // ✅ Оптимизации для скорости
            _wallpaperGraphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            _wallpaperGraphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            _wallpaperGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            _wallpaperGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

            pictureBox.Visible = false;

            _gameEngine.FillRandom(50);
            DrawToWallpaperBitmap();

            UpdateControlPanelVisibility();

            this.Show();
            Application.DoEvents();
            System.Threading.Thread.Sleep(200);

            WallpaperHelper.SetAsWallpaper(this);

            if (_gameEngine._statusEngine == StatusEngine.stop)
                StartGame();

            // ✅ Первая отрисовка
            this.Invalidate();
            this.Update();
            Application.DoEvents();

            _trayManager.ShowBalloonTip("Режим обоев",
                "Приложение работает в фоне. Используйте иконку в трее для управления.",
                ToolTipIcon.Info, 2000);
        }

        private void DisableWallpaperMode()
        {
            // ✅ Очищаем Bitmap обоев
            _wallpaperBitmap?.Dispose();
            _wallpaperGraphics?.Dispose();
            _wallpaperBitmap = null;
            _wallpaperGraphics = null;

            WallpaperHelper.RestoreToNormal(this);

            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.WindowState = FormWindowState.Normal;
            this.TopMost = false;
            this.ShowInTaskbar = true;
            this.ControlBox = true;

            pictureBox.Visible = true;

            UpdateControlPanelVisibility();

            DrawToPictureBox();
        }

        private void UpdateControlPanelVisibility()
        {
            if (_controlPanelControls == null) return;

            foreach (Control ctrl in _controlPanelControls)
            {
                ctrl.Visible = _isControlPanelVisible && !_isWallpaperMode;
            }

            if (_mainMenu != null)
            {
                _mainMenu.Visible = _isControlPanelVisible && !_isWallpaperMode;
            }
        }

        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _wallpaperBitmap?.Dispose();
            _wallpaperGraphics?.Dispose();
            _trayManager?.Dispose();
            if (_isWallpaperMode) DisableWallpaperMode();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _wallpaperBitmap?.Dispose();
            _wallpaperGraphics?.Dispose();
            _trayManager?.Dispose();
            if (_isWallpaperMode) DisableWallpaperMode();
            base.OnFormClosing(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F12) { ToggleWallpaperMode(); return true; }
            if (keyData == Keys.Space) { TriggerStartPause(); _trayManager.UpdateMenuState(); return true; }
            if (keyData == Keys.F5) { TriggerRandom(); return true; }
            if (keyData == Keys.Escape && _isWallpaperMode) { ToggleWallpaperMode(); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion
    }
}