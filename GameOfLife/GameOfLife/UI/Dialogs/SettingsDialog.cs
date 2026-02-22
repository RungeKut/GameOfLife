using System;
using System.Drawing;
using System.Windows.Forms;
using GameOfLife.Core;
using GameOfLife.Rendering;

namespace GameOfLife.UI.Dialogs
{
    /// <summary>
    /// Диалог настроек симуляции.
    /// 
    /// Этот диалог предоставляет пользователю интерфейс для:
    /// - Настройки параметров симуляции (размер мира, скорость)
    /// - Настройки визуализации (цвета, размер клетки, сетка)
    /// - Настройки производительности (параллелизм, потоки)
    /// - Настройки режима (Conway, Ecosystem)
    /// 
    /// Архитектурные особенности:
    /// - Полностью отделён от логики симуляции
    /// - Возвращает конфигурацию через свойства
    /// - Поддерживает сброс к значениям по умолчанию
    /// - Валидирует введённые значения
    /// 
    /// Использование:
    /// var dialog = new SettingsDialog(currentConfig);
    /// if (dialog.ShowDialog() == DialogResult.OK)
    /// {
    ///     var newConfig = dialog.GetConfiguration();
    ///     // Применить новую конфигурацию
    /// }
    /// </summary>
    public partial class SettingsDialog : Form
    {
        #region Приватные поля

        /// <summary>
        /// Текущая конфигурация для редактирования.
        /// </summary>
        private SimulationConfig _currentConfig;

        /// <summary>
        /// Конфигурация рендерера для редактирования.
        /// </summary>
        private RenderConfig _renderConfig;

        /// <summary>
        /// Флаг изменения настроек.
        /// </summary>
        private bool _hasChanges;

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Получает изменённую конфигурацию симуляции.
        /// </summary>
        public SimulationConfig GetSimulationConfig()
        {
            return _currentConfig;
        }

        /// <summary>
        /// Получает изменённую конфигурацию рендерера.
        /// </summary>
        public RenderConfig GetRenderConfig()
        {
            return _renderConfig;
        }

        /// <summary>
        /// Были ли внесены изменения.
        /// </summary>
        public bool HasChanges => _hasChanges;

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новый диалог настроек.
        /// 
        /// Инициализирует все элементы управления
        /// текущими значениями конфигурации.
        /// </summary>
        /// <param name="simConfig">
        /// Текущая конфигурация симуляции.
        /// </param>
        /// <param name="renderConfig">
        /// Текущая конфигурация рендерера.
        /// </param>
        public SettingsDialog(SimulationConfig simConfig, RenderConfig renderConfig)
        {
            InitializeComponent();

            _currentConfig = simConfig ?? SimulationConfig.CreateDefault();
            _renderConfig = renderConfig ?? RenderConfig.CreateDefault();
            _hasChanges = false;

            LoadCurrentSettings();
        }

        #endregion

        #region Загрузка настроек

        /// <summary>
        /// Загружает текущие настройки в элементы управления.
        /// 
        /// Заполняет все поля формы текущими значениями
        /// из конфигурации симуляции и рендерера.
        /// </summary>
        private void LoadCurrentSettings()
        {
            // Вкладка "Симуляция"
            numWorldWidth.Value = _currentConfig.WorldWidth;
            numWorldHeight.Value = _currentConfig.WorldHeight;
            numTickDelay.Value = _currentConfig.TickDelayMs;
            numInitialDensity.Value = _currentConfig.InitialDensity;
            chkWrapAround.Checked = _currentConfig.WrapAround;
            chkParallelProcessing.Checked = _currentConfig.EnableParallelProcessing;
            numThreads.Value = _currentConfig.ThreadCount;
            chkSeed.Checked = _currentConfig.Seed >= 0;
            numSeed.Value = _currentConfig.Seed >= 0 ? _currentConfig.Seed : 0;

            // Вкладка "Визуализация"
            numCellSize.Value = _renderConfig.CellSize;
            colorAliveColor.Color = _renderConfig.AliveColor;
            colorDeadColor.Color = _renderConfig.DeadColor;
            colorGridColor.Color = _renderConfig.GridColor;
            numGridThickness.Value = _renderConfig.GridThickness;
            chkShowGrid.Checked = _renderConfig.ShowGrid;
            chkShowGenerationInfo.Checked = _renderConfig.ShowGenerationInfo;

            // Вкладка "Режим"
            cmbSimulationMode.Items.Clear();
            cmbSimulationMode.Items.Add("Conway (Игра в жизнь)");
            cmbSimulationMode.Items.Add("Ecosystem (Экосистема)");
            cmbSimulationMode.SelectedIndex = 0; // По умолчанию Conway

            _hasChanges = false;
        }

        #endregion

        #region Сохранение настроек

        /// <summary>
        /// Сохраняет настройки из элементов управления.
        /// 
        /// Считывает все значения из полей формы
        /// и обновляет конфигурационные объекты.
        /// </summary>
        private void SaveCurrentSettings()
        {
            // Конфигурация симуляции
            _currentConfig.WorldWidth = (int)numWorldWidth.Value;
            _currentConfig.WorldHeight = (int)numWorldHeight.Value;
            _currentConfig.TickDelayMs = (int)numTickDelay.Value;
            _currentConfig.InitialDensity = (int)numInitialDensity.Value;
            _currentConfig.WrapAround = chkWrapAround.Checked;
            _currentConfig.EnableParallelProcessing = chkParallelProcessing.Checked;
            _currentConfig.ThreadCount = (int)numThreads.Value;
            _currentConfig.Seed = chkSeed.Checked ? (int)numSeed.Value : -1;

            // Конфигурация рендерера
            _renderConfig.CellSize = (int)numCellSize.Value;
            _renderConfig.AliveColor = colorAliveColor.Color;
            _renderConfig.DeadColor = colorDeadColor.Color;
            _renderConfig.GridColor = colorGridColor.Color;
            _renderConfig.GridThickness = (int)numGridThickness.Value;
            _renderConfig.ShowGrid = chkShowGrid.Checked;
            _renderConfig.ShowGenerationInfo = chkShowGenerationInfo.Checked;

            _hasChanges = true;
        }

        #endregion

        #region Валидация

        /// <summary>
        /// Проверяет корректность введённых значений.
        /// 
        /// Выполняет следующие проверки:
        /// - Размеры мира в допустимых пределах
        /// - Задержка между тиками положительная
        /// - Плотность от 0 до 100
        /// - Количество потоков не отрицательное
        /// 
        /// Возвращает сообщение об ошибке или null если всё OK.
        /// </summary>
        private string ValidateSettings()
        {
            if (numWorldWidth.Value < 10 || numWorldWidth.Value > 10000)
                return "Ширина мира должна быть от 10 до 10000";

            if (numWorldHeight.Value < 10 || numWorldHeight.Value > 10000)
                return "Высота мира должна быть от 10 до 10000";

            if (numTickDelay.Value < 0 || numTickDelay.Value > 10000)
                return "Задержка должна быть от 0 до 10000 мс";

            if (numInitialDensity.Value < 0 || numInitialDensity.Value > 100)
                return "Плотность должна быть от 0 до 100%";

            if (numThreads.Value < 0)
                return "Количество потоков не может быть отрицательным";

            if (numCellSize.Value < 1 || numCellSize.Value > 64)
                return "Размер клетки должен быть от 1 до 64 пикселей";

            return null;
        }

        #endregion

        #region Обработчики событий

        /// <summary>
        /// Обработчик кнопки OK.
        /// </summary>
        private void btnOK_Click(object sender, EventArgs e)
        {
            string error = ValidateSettings();

            if (error != null)
            {
                MessageBox.Show(
                    error,
                    "Ошибка валидации",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            SaveCurrentSettings();
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Обработчик кнопки Cancel.
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Обработчик кнопки сброса к умолчанию.
        /// </summary>
        private void btnReset_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Сбросить все настройки к значениям по умолчанию?",
                "Подтверждение сброса",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                _currentConfig = SimulationConfig.CreateDefault();
                _renderConfig = RenderConfig.CreateDefault();
                LoadCurrentSettings();
            }
        }

        /// <summary>
        /// Обработчик применения настроек без закрытия.
        /// </summary>
        private void btnApply_Click(object sender, EventArgs e)
        {
            string error = ValidateSettings();

            if (error != null)
            {
                MessageBox.Show(error, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SaveCurrentSettings();
            MessageBox.Show(
                "Настройки применены",
                "Применение настроек",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        /// <summary>
        /// Обработчик изменения любого поля.
        /// </summary>
        private void OnSettingChanged(object sender, EventArgs e)
        {
            _hasChanges = true;
        }

        /// <summary>
        /// Обработчик включения/выключения Seed.
        /// </summary>
        private void chkSeed_CheckedChanged(object sender, EventArgs e)
        {
            numSeed.Enabled = chkSeed.Checked;
            _hasChanges = true;
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Применяет настройки цвета к конфигурации.
        /// </summary>
        private void ApplyColorSettings()
        {
            _renderConfig.AliveColor = colorAliveColor.Color;
            _renderConfig.DeadColor = colorDeadColor.Color;
            _renderConfig.GridColor = colorGridColor.Color;
        }

        #endregion

        #region Обработчики выбора цвета

        /// <summary>
        /// Обработчик выбора цвета живой клетки.
        /// </summary>
        private void btnAliveColor_Click(object sender, EventArgs e)
        {
            colorAliveColor.Color = btnAliveColor.BackColor;
            if (colorAliveColor.ShowDialog() == DialogResult.OK)
            {
                btnAliveColor.BackColor = colorAliveColor.Color;
                _hasChanges = true;
            }
        }

        /// <summary>
        /// Обработчик выбора цвета фона.
        /// </summary>
        private void btnDeadColor_Click(object sender, EventArgs e)
        {
            colorDeadColor.Color = btnDeadColor.BackColor;
            if (colorDeadColor.ShowDialog() == DialogResult.OK)
            {
                btnDeadColor.BackColor = colorDeadColor.Color;
                _hasChanges = true;
            }
        }

        /// <summary>
        /// Обработчик выбора цвета сетки.
        /// </summary>
        private void btnGridColor_Click(object sender, EventArgs e)
        {
            colorGridColor.Color = btnGridColor.BackColor;
            if (colorGridColor.ShowDialog() == DialogResult.OK)
            {
                btnGridColor.BackColor = colorGridColor.Color;
                _hasChanges = true;
            }
        }

        #endregion
    }
}