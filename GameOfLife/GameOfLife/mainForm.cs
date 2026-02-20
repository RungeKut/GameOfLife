using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

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

        // === НОВОЕ: Виртуальный рабочий стол ===
        private List<MonitorInfo> _monitors { get; set; }
        private List<WallpaperForm> _wallpaperForms = new List<WallpaperForm>();
        private int _cellSize = 1; // Размер клетки в пикселях

        public GameEngine GetGameEngine() => _gameEngine;

        public mainForm()
        {
            InitializeComponent();

            pictureBox.Visible = true;
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.BackColor = Color.Black;

            SaveControlPanelReferences();

            _monitors = VirtualDesktop.GetMonitors();
            _worldSize = VirtualDesktop.GetWorldSize(_monitors, _cellSize);

            _gameEngine = new GameEngine();
            pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);
            _graphics = Graphics.FromImage(pictureBox.Image);

            _gameEngine.ResizeWorld(_worldSize);
            ResizePictureBox();
            ResetZoom();
            pictureBox.MouseWheel += PictureBox_MouseWheel;
            CalculatingSize();
            UpdateFormTitle();
            CalculatingDrawBegin();

            GridCheckBox.Checked = false;

            // ✅ Инициализируем значения NumericUpDown
            UpdateWorldSizeControls();

            _trayManager = new TrayManager(this);

            try
            {
                Icon appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                if (appIcon != null)
                    _trayManager.SetIcon(appIcon);
            }
            catch { }

            _trayManager.ShowBalloonTip("Game of Life",
                "Приложение запущено. Обнаружено " + _monitors.Count + " монитор(а). Используйте иконку в трее для управления.",
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

        #region Отрисовка

        private void DrawCurrentGeneration()
        {
            if (_isWallpaperMode)
            {
                DrawToAllWallpapers();
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
                DrawCells(_graphics, pictureBox.Width, pictureBox.Height, 0, 0);
            }
            finally
            {
                pictureBox.ResumeLayout();
            }

            UpdateFormTitle();
            pictureBox.Refresh();
        }

        private void DrawToAllWallpapers()
        {
            foreach (var wpForm in _wallpaperForms)
            {
                // ✅ Обновляем видимость границ перед отрисовкой
                wpForm.SetWorldBorderVisibility(WorldBorderCheckBox.Checked);

                wpForm.DrawGeneration(_zoomCount, _worldWidthDrawBegin, _worldHeightDrawBegin,
                    _halfSizeAbroadCellWidth, _halfSizeAbroadCellHeight);
            }
        }

        private void DrawCells(Graphics g, int width, int height, int offsetX, int offsetY)
        {
            var _field = _gameEngine.GetCurrentGeneration();
            if (_field == null)
                return;

            int windowSizeWidth = width / _zoomCount;
            int windowSizeHeight = height / _zoomCount;
            float halfSizeAbroadCellWidth = Truncate((float)width / _zoomCount) / 2;
            float halfSizeAbroadCellHeight = Truncate((float)height / _zoomCount) / 2;

            // Сетка
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

            // Клетки
            for (int x = -1; x < windowSizeWidth + 1; x++)
            {
                int _tempX = x * _zoomCount + (int)(halfSizeAbroadCellWidth * _zoomCount);
                int globalX = x + _worldWidthDrawBegin + offsetX;
                int _worldX = ((globalX % (int)_worldSize.X) + (int)_worldSize.X) % (int)_worldSize.X;

                for (int y = -1; y < windowSizeHeight + 1; y++)
                {
                    int _tempY = y * _zoomCount + (int)(halfSizeAbroadCellHeight * _zoomCount);
                    int globalY = y + _worldHeightDrawBegin + offsetY;
                    int _worldY = ((globalY % (int)_worldSize.Y) + (int)_worldSize.Y) % (int)_worldSize.Y;

                    if (_worldX >= 0 && _worldX < _field.GetLength(0) &&
                        _worldY >= 0 && _worldY < _field.GetLength(1))
                    {
                        if (_field[_worldX, _worldY])
                        {
                            Color cellColor = Color.Crimson;
                            var genome = _gameEngine.GetCellGenome(_worldX, _worldY);
                            if (genome != null)
                                cellColor = genome.GenomeColor;

                            using (Brush brush = new SolidBrush(cellColor))
                            {
                                if (_zoomCount > 1)
                                    g.FillRectangle(brush, _tempX + 1, _tempY + 1, _zoomCount - 1, _zoomCount - 1);
                                else
                                    g.FillRectangle(brush, _tempX, _tempY, 1, 1);
                            }
                        }
                    }
                }
            }

            // ✅ ОТРИСОВКА ГРАНИЦ МИРА (в конце, поверх клеток)
            if (WorldBorderCheckBox != null && WorldBorderCheckBox.Checked)
            {
                DrawWorldBorder(g, width, height, halfSizeAbroadCellWidth, halfSizeAbroadCellHeight, offsetX, offsetY);
            }
        }

        // ✅ НОВЫЙ МЕТОД: Отрисовка границ мира (сетка, 1 пиксель)
        private void DrawWorldBorder(Graphics g, int width, int height,
            float halfSizeAbroadCellWidth, float halfSizeAbroadCellHeight,
            int offsetX, int offsetY)
        {
            using (Pen borderPen = new Pen(Color.White, 1))  // ✅ ТОЛЩИНА 1 ПИКСЕЛЬ
            {
                // Размер одной копии мира в пикселях
                int worldWidthPixels = (int)_worldSize.X * _zoomCount;
                int worldHeightPixels = (int)_worldSize.Y * _zoomCount;

                // Начальная позиция (левый верхний угол первой копии)
                int startX = (int)(halfSizeAbroadCellWidth * _zoomCount) + offsetX;
                int startY = (int)(halfSizeAbroadCellHeight * _zoomCount) + offsetY;

                // Нормализуем start в пределах одной копии мира
                startX = startX % worldWidthPixels;
                if (startX < 0) startX += worldWidthPixels;

                startY = startY % worldHeightPixels;
                if (startY < 0) startY += worldHeightPixels;

                // Сдвигаем назад, чтобы покрыть всю видимую область
                startX -= worldWidthPixels;
                startY -= worldHeightPixels;

                // ✅ Рисуем сетку горизонтальных линий (границы по Y)
                for (int y = startY; y < height; y += worldHeightPixels)
                {
                    if (y >= 0)
                        g.DrawLine(borderPen, 0, y, width, y);
                }

                // ✅ Рисуем сетку вертикальных линий (границы по X)
                for (int x = startX; x < width; x += worldWidthPixels)
                {
                    if (x >= 0)
                        g.DrawLine(borderPen, x, 0, x, height);
                }
            }
        }

        private float Truncate(float a) { return a - (float)Math.Truncate(a); }

        private void UpdateFormTitle()
        {
            if (!_isWallpaperMode)
            {
                this.Text = "Generation:" + _gameEngine.CurrentGeneration + " Zoom:" + _zoomCount + "x World:" + (int)_worldSize.X + "x" + (int)_worldSize.Y + " Monitors:" + _monitors.Count;
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
            // ✅ Работаем только в режиме формы
            if (_isWallpaperMode) return;

            _worldSize.Y = (int)WorldHeightNumericUpDown.Value;
            _gameEngine.ResizeWorld(_worldSize);
            DrawCurrentGeneration();

            // ✅ Обновляем заголовок
            UpdateFormTitle();
        }

        private void WorldWidthNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            // ✅ Работаем только в режиме формы
            if (_isWallpaperMode) return;

            _worldSize.X = (int)WorldWidthNumericUpDown.Value;
            _gameEngine.ResizeWorld(_worldSize);
            DrawCurrentGeneration();

            // ✅ Обновляем заголовок
            UpdateFormTitle();
        }

        private void UpdateWorldSizeControls()
        {
            // ✅ Обновляем значения полей при изменении размера мира
            if (WorldWidthNumericUpDown != null)
                WorldWidthNumericUpDown.Value = (int)_worldSize.X;

            if (WorldHeightNumericUpDown != null)
                WorldHeightNumericUpDown.Value = (int)_worldSize.Y;
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

        // Методы для управления геномами
        public void ToggleGenomes()
        {
            _gameEngine.GenomeEnabled = !_gameEngine.GenomeEnabled;
            _gameEngine.FillRandom(50);
            DrawCurrentGeneration();
            _trayManager.ShowBalloonTip("Геномы",
                _gameEngine.GenomeEnabled ? "Геномы включены" : "Геномы выключены",
                ToolTipIcon.Info, 1000);
        }

        public void SetMutationRate(float rate)
        {
            _gameEngine.MutationRate = rate;
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

            _monitors = VirtualDesktop.GetMonitors();

            _worldSize = VirtualDesktop.GetWorldSize(_monitors, _cellSize * _zoomCount);
            _gameEngine.ResizeWorld(_worldSize);

            this.Hide();

            _wallpaperForms.Clear();
            foreach (MonitorInfo monitor in _monitors)
            {
                var wpForm = new WallpaperForm(_gameEngine, monitor, _cellSize * _zoomCount, _worldSize);
                wpForm.InitializeBitmap();

                // ✅ Передаём настройку видимости границ
                wpForm.SetWorldBorderVisibility(WorldBorderCheckBox.Checked);

                _wallpaperForms.Add(wpForm);
            }

            _gameEngine.FillRandom(50);

            foreach (var wpForm in _wallpaperForms)
            {
                wpForm.ShowWallpaper();
            }

            DrawToAllWallpapers();
            System.Threading.Thread.Sleep(300);

            if (_gameEngine._statusEngine == StatusEngine.stop)
                StartGame();

            _trayManager.ShowBalloonTip("Режим обоев",
                "Приложение работает на " + _monitors.Count + " мониторе(ах). Мир: " + (int)_worldSize.X + "x" + (int)_worldSize.Y + " клеток. Зум: " + _zoomCount + "x",
                ToolTipIcon.Info, 2000);
        }

        private void DisableWallpaperMode()
        {
            foreach (var wpForm in _wallpaperForms)
            {
                wpForm.HideWallpaper();
                wpForm.DisposeResources();
            }
            _wallpaperForms.Clear();

            this.Show();
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;

            // ✅ Включаем ручное изменение размера в режиме формы
            if (WorldWidthNumericUpDown != null)
                WorldWidthNumericUpDown.Enabled = true;
            if (WorldHeightNumericUpDown != null)
                WorldHeightNumericUpDown.Enabled = true;

            // ✅ Восстанавливаем размер мира для режима формы
            _monitors = VirtualDesktop.GetMonitors();
            _worldSize = VirtualDesktop.GetWorldSize(_monitors, _cellSize);
            _gameEngine.ResizeWorld(_worldSize);

            // ✅ Обновляем значения в NumericUpDown
            UpdateWorldSizeControls();

            UpdateControlPanelVisibility();
            pictureBox.Visible = true;

            DrawToPictureBox();

            _isWallpaperMode = false;

            UpdateFormTitle();
        }

        private void UpdateControlPanelVisibility()
        {
            if (_controlPanelControls == null) return;

            foreach (Control ctrl in _controlPanelControls)
            {
                // ✅ Показываем панель только в режиме формы
                ctrl.Visible = _isControlPanelVisible && !_isWallpaperMode;
            }

            if (_mainMenu != null)
            {
                _mainMenu.Visible = _isControlPanelVisible && !_isWallpaperMode;
            }

            // ✅ Отдельно управляем NumericUpDown
            if (WorldWidthNumericUpDown != null)
                WorldWidthNumericUpDown.Enabled = !_isWallpaperMode;
            if (WorldHeightNumericUpDown != null)
                WorldHeightNumericUpDown.Enabled = !_isWallpaperMode;
        }

        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var wpForm in _wallpaperForms)
            {
                wpForm.DisposeResources();
            }
            _wallpaperForms.Clear();

            _trayManager?.Dispose();
            if (_isWallpaperMode) DisableWallpaperMode();
        }

        private bool _allowMouseDrawing { get; set; } = true;

        public void ToggleMouseDrawing()
        {
            _allowMouseDrawing = !_allowMouseDrawing;
            _trayManager.ShowBalloonTip("Рисование мышью",
                _allowMouseDrawing ? "Включено: клики рисуют клетки" : "Выключено: клики проходят сквозь обои",
                ToolTipIcon.Info, 1000);
        }

        // === МЕТОДЫ ДЛЯ УПРАВЛЕНИЯ ЗУМОМ ===

        public int GetCurrentZoom()
        {
            return _zoomCount;
        }

        public void SetZoomLevel(int zoomLevel)
        {
            if (zoomLevel < 1 || zoomLevel > ZOOM_MAX)
                return;

            int centerX = _currentWorldX;
            int centerY = _currentWorldY;

            int oldWorldWidth = (int)_worldSize.X;
            int oldWorldHeight = (int)_worldSize.Y;

            _zoomCount = zoomLevel;

            _monitors = VirtualDesktop.GetMonitors();

            // ✅ В режиме формы - размер от мониторов и зума
            if (!_isWallpaperMode)
                _worldSize = VirtualDesktop.GetWorldSize(_monitors, _cellSize * _zoomCount);

            _currentWorldX = (int)((centerX * _worldSize.X) / oldWorldWidth);
            _currentWorldY = (int)((centerY * _worldSize.Y) / oldWorldHeight);

            _gameEngine.ResizeWorld(_worldSize);

            // ✅ Обновляем NumericUpDown только в режиме формы
            if (!_isWallpaperMode)
                UpdateWorldSizeControls();

            CalculatingSize();
            CalculatingDrawBegin();

            if (_isWallpaperMode)
                DrawToAllWallpapers();
            else
                DrawToPictureBox();

            UpdateFormTitle();

            _trayManager.ShowBalloonTip("Зум изменён",
                "Зум: " + _zoomCount + "x, Мир: " + (int)_worldSize.X + "x" + (int)_worldSize.Y + " клеток",
                ToolTipIcon.Info, 1000);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            foreach (var wpForm in _wallpaperForms)
            {
                wpForm.DisposeResources();
            }
            _wallpaperForms.Clear();

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

        private void GenomeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _gameEngine.GenomeEnabled = GenomeCheckBox.Checked;
            _trayManager.ShowBalloonTip("Геномы",
                GenomeCheckBox.Checked ? "Геномы включены" : "Геномы выключены",
                ToolTipIcon.Info, 1000);
        }

        private void EnvironmentCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _gameEngine.EnvironmentEnabled = EnvironmentCheckBox.Checked;
            _trayManager.ShowBalloonTip("Среда",
                EnvironmentCheckBox.Checked ? "Динамическая среда включена" : "Среда отключена",
                ToolTipIcon.Info, 1000);
        }

        private void MutationRateNumeric_ValueChanged(object sender, EventArgs e)
        {
            _gameEngine.MutationRate = (float)MutationRateNumeric.Value / 100.0f;
        }

        private void ShowToxicityCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            DrawCurrentGeneration();
        }

        private void ParallelCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _gameEngine.ParallelProcessing = ParallelCheckBox.Checked;
            _trayManager.ShowBalloonTip("Параллелизм",
                ParallelCheckBox.Checked ?
                    $"Включено ({System.Environment.ProcessorCount} ядер)" :
                    "Выключено (последовательно)",
                ToolTipIcon.Info, 1000);
        }

        private void WorldBorderCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            DrawCurrentGeneration();
        }
    }
}