using System;
using System.Drawing;
using System.Windows.Forms;

namespace GameOfLife
{
    public class TrayManager : IDisposable
    {
        private NotifyIcon _notifyIcon;
        private ContextMenuStrip _contextMenu;
        private mainForm _mainForm;

        private ToolStripMenuItem _startPauseMenuItem;
        private ToolStripMenuItem _stopMenuItem;
        private ToolStripMenuItem _randomMenuItem;
        private ToolStripMenuItem _wallpaperMenuItem;
        private ToolStripMenuItem _settingsMenuItem;
        private ToolStripMenuItem _exitMenuItem;

        public TrayManager(mainForm form)
        {
            _mainForm = form;
            InitializeTray();
        }

        private void InitializeTray()
        {
            _contextMenu = new ContextMenuStrip();

            // ✅ ИСПРАВЛЕНО: Используем Keys.None вместо Keys.Space
            _startPauseMenuItem = new ToolStripMenuItem("Старт/Пауза", null, OnStartPauseClick);
            _startPauseMenuItem.ShortcutKeys = Keys.None; // Было: Keys.Space
            _contextMenu.Items.Add(_startPauseMenuItem);

            _stopMenuItem = new ToolStripMenuItem("Стоп/Сброс", null, OnStopClick);
            _contextMenu.Items.Add(_stopMenuItem);

            _contextMenu.Items.Add(new ToolStripSeparator());

            // ✅ F5 - допустимая клавиша
            _randomMenuItem = new ToolStripMenuItem("Случайная генерация", null, OnRandomClick);
            _randomMenuItem.ShortcutKeys = Keys.F5;
            _contextMenu.Items.Add(_randomMenuItem);

            _contextMenu.Items.Add(new ToolStripSeparator());

            // ✅ F12 - допустимая клавиша
            _wallpaperMenuItem = new ToolStripMenuItem("Режим обоев", null, OnWallpaperClick);
            _wallpaperMenuItem.ShortcutKeys = Keys.F12;
            _contextMenu.Items.Add(_wallpaperMenuItem);

            _contextMenu.Items.Add(new ToolStripSeparator());

            _settingsMenuItem = new ToolStripMenuItem("Показать панель управления", null, OnSettingsClick);
            _contextMenu.Items.Add(_settingsMenuItem);

            _contextMenu.Items.Add(new ToolStripSeparator());

            // ✅ Alt+F4 - допустимая комбинация
            _exitMenuItem = new ToolStripMenuItem("Выход", null, OnExitClick);
            _exitMenuItem.ShortcutKeys = Keys.Alt | Keys.F4;
            _contextMenu.Items.Add(_exitMenuItem);

            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Text = "Game of Life",
                ContextMenuStrip = _contextMenu,
                Visible = true
            };

            _notifyIcon.DoubleClick += OnNotifyIconDoubleClick;
            _notifyIcon.MouseClick += OnNotifyIconMouseClick;

            UpdateMenuState();
        }

        private void OnNotifyIconDoubleClick(object sender, EventArgs e)
        {
            _mainForm.ToggleControlPanel();
        }

        private void OnNotifyIconMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _mainForm.BringToFront();
                _mainForm.Activate();
            }
        }

        private void OnStartPauseClick(object sender, EventArgs e)
        {
            _mainForm.TriggerStartPause();
            UpdateMenuState();
        }

        private void OnStopClick(object sender, EventArgs e)
        {
            _mainForm.TriggerStop();
            UpdateMenuState();
        }

        private void OnRandomClick(object sender, EventArgs e)
        {
            _mainForm.TriggerRandom();
        }

        private void OnWallpaperClick(object sender, EventArgs e)
        {
            _mainForm.ToggleWallpaperMode();
            UpdateMenuState();
        }

        private void OnSettingsClick(object sender, EventArgs e)
        {
            _mainForm.ToggleControlPanel();
        }

        private void OnExitClick(object sender, EventArgs e)
        {
            _mainForm.CloseApplication();
        }

        public void UpdateMenuState()
        {
            // ✅ ИСПРАВЛЕНО: Добавлен public метод для доступа к _gameEngine
            var engine = _mainForm.GetGameEngine();
            if (engine == null) return;

            switch (engine._statusEngine)
            {
                case StatusEngine.run:
                    _startPauseMenuItem.Text = "Пауза";
                    break;
                case StatusEngine.pause:
                    _startPauseMenuItem.Text = "Продолжить";
                    break;
                case StatusEngine.stop:
                    _startPauseMenuItem.Text = "Старт";
                    break;
            }

            _wallpaperMenuItem.Text = _mainForm.IsWallpaperMode ? "Выйти из режима обоев" : "Режим обоев";
            _wallpaperMenuItem.Checked = _mainForm.IsWallpaperMode;

            _settingsMenuItem.Text = _mainForm.IsControlPanelVisible ? "Скрыть панель управления" : "Показать панель управления";
        }

        public void SetIcon(Icon icon)
        {
            if (icon != null)
            {
                _notifyIcon.Icon = icon;
            }
        }

        public void ShowBalloonTip(string title, string message, ToolTipIcon icon = ToolTipIcon.Info, int timeout = 2000)
        {
            _notifyIcon.ShowBalloonTip(timeout, title, message, icon);
        }

        public void Dispose()
        {
            _notifyIcon?.Dispose();
            _contextMenu?.Dispose();
        }
    }
}