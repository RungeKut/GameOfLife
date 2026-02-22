using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GameOfLife.Data;

namespace GameOfLife.UI.Controls
{
    /// <summary>
    /// Панель управления сохранениями и загрузками.
    /// 
    /// Этот контрол предоставляет пользователю интерфейс для:
    /// - Создания новых сохранений
    /// - Загрузки существующих сохранений
    /// - Удаления ненужных сохранений
    /// - Просмотра информации о сохранениях
    /// - Настройки автосохранения
    /// 
    /// Архитектурные особенности:
    /// - Полностью отделён от логики сохранения
    /// - Использует SaveLoadManager для всех операций
    /// - Поддерживает асинхронные операции сохранения
    /// - Отображает прогресс операций
    /// 
    /// Использование:
    /// var panel = new SaveLoadPanelControl();
    /// panel.SaveLoadManager = saveLoadManager;
    /// panel.OnSaveRequested += (name) => { /* сохранение */ };
    /// </summary>
    public partial class SaveLoadPanelControl : UserControl
    {
        #region Приватные поля

        /// <summary>
        /// Менеджер сохранений для всех операций.
        /// </summary>
        private SaveLoadManager _saveLoadManager;

        /// <summary>
        /// Список текущих сохранений.
        /// </summary>
        private List<SaveInfo> _currentSaves;

        /// <summary>
        /// Флаг блокировки интерфейса во время операций.
        /// </summary>
        private bool _isOperating;

        #endregion

        #region События

        /// <summary>
        /// Событие запроса на сохранение.
        /// 
        /// Вызывается когда пользователь нажимает кнопку "Сохранить".
        /// Аргумент: имя сохранения.
        /// 
        /// Подписчик должен выполнить сохранение состояния.
        /// </summary>
        public event Action<string> OnSaveRequested;

        /// <summary>
        /// Событие запроса на загрузку.
        /// 
        /// Вызывается когда пользователь нажимает кнопку "Загрузить".
        /// Аргумент: имя сохранения.
        /// 
        /// Подписчик должен выполнить загрузку состояния.
        /// </summary>
        public event Action<string> OnLoadRequested;

        /// <summary>
        /// Событие запроса на удаление.
        /// 
        /// Вызывается когда пользователь нажимает кнопку "Удалить".
        /// Аргумент: имя сохранения.
        /// 
        /// Подписчик должен выполнить удаление файла.
        /// </summary>
        public event Action<string> OnDeleteRequested;

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Менеджер сохранений.
        /// </summary>
        public SaveLoadManager SaveLoadManager
        {
            get => _saveLoadManager;
            set
            {
                _saveLoadManager = value;
                if (_saveLoadManager != null)
                {
                    RefreshSaveList();
                }
            }
        }

        /// <summary>
        /// Включено ли автосохранение.
        /// </summary>
        public bool AutoSaveEnabled
        {
            get => chkAutoSave.Checked;
            set => chkAutoSave.Checked = value;
        }

        /// <summary>
        /// Интервал автосохранения в минутах.
        /// </summary>
        public int AutoSaveIntervalMinutes
        {
            get => (int)numAutoSaveInterval.Value;
            set => numAutoSaveInterval.Value = Math.Min(numAutoSaveInterval.Maximum, Math.Max(numAutoSaveInterval.Minimum, value));
        }

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новую панель управления сохранениями.
        /// 
        /// Инициализирует компоненты интерфейса:
        /// - Список сохранений
        /// - Поля ввода имени
        /// - Кнопки управления
        /// - Настройки автосохранения
        /// </summary>
        public SaveLoadPanelControl()
        {
            InitializeComponent();
            _currentSaves = new List<SaveInfo>();
            _isOperating = false;
            InitializeAutoSaveSettings();
        }

        #endregion

        #region Инициализация

        /// <summary>
        /// Инициализирует настройки автосохранения.
        /// 
        /// Устанавливает значения по умолчанию:
        /// - Интервал: 5 минут
        /// - Включено: false
        /// </summary>
        private void InitializeAutoSaveSettings()
        {
            numAutoSaveInterval.Minimum = 1;
            numAutoSaveInterval.Maximum = 60;
            numAutoSaveInterval.Value = 5;
            chkAutoSave.Checked = false;
        }

        #endregion

        #region Управление списком сохранений

        /// <summary>
        /// Обновляет список сохранений из менеджера.
        /// 
        /// Вызывается при открытии панели и после операций.
        /// Заполняет listBox с информацией о каждом сохранении:
        /// - Имя сохранения
        /// - Дата создания
        /// - Размер файла
        /// - Номер поколения
        /// </summary>
        public void RefreshSaveList()
        {
            if (_isOperating || _saveLoadManager == null)
                return;

            _isOperating = true;

            try
            {
                listBoxSaves.Items.Clear();
                _currentSaves = _saveLoadManager.GetSaveList();

                foreach (var save in _currentSaves)
                {
                    string info = string.Format(
                        "{0} | {1} | Gen: {2} | {3:F1} KB",
                        save.SaveName,
                        save.ModifiedDate.ToString("dd.MM.yyyy HH:mm"),
                        save.LastGeneration,
                        save.FileSize / 1024.0
                    );

                    listBoxSaves.Items.Add(new SaveListItem(save));
                }

                UpdateButtons();
            }
            finally
            {
                _isOperating = false;
            }
        }

        /// <summary>
        /// Обновляет доступность кнопок.
        /// 
        /// Проверяет наличие выбранного сохранения
        /// и включает/выключает кнопки загрузки и удаления.
        /// </summary>
        private void UpdateButtons()
        {
            bool hasSelection = listBoxSaves.SelectedItem != null;
            btnLoad.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;
            btnSave.Enabled = !string.IsNullOrWhiteSpace(txtSaveName.Text);
        }

        /// <summary>
        /// Получает информацию о выбранном сохранении.
        /// </summary>
        private SaveInfo GetSelectedSaveInfo()
        {
            if (listBoxSaves.SelectedItem is SaveListItem item)
            {
                return item.SaveInfo;
            }
            return null;
        }

        #endregion

        #region Операции сохранения

        /// <summary>
        /// Выполняет сохранение с указанным именем.
        /// 
        /// Вызывается при нажатии кнопки "Сохранить".
        /// Проверяет валидность имени и инициирует сохранение.
        /// </summary>
        private void PerformSave()
        {
            string saveName = txtSaveName.Text.Trim();

            if (string.IsNullOrWhiteSpace(saveName))
            {
                MessageBox.Show(
                    "Введите имя сохранения",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            // Проверка на существующее сохранение
            if (_saveLoadManager.SaveExists(saveName))
            {
                var result = MessageBox.Show(
                    string.Format("Сохранение '{0}' уже существует. Перезаписать?", saveName),
                    "Подтверждение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result != DialogResult.Yes)
                    return;
            }

            OnSaveRequested?.Invoke(saveName);
        }

        /// <summary>
        /// Выполняет загрузку выбранного сохранения.
        /// 
        /// Вызывается при нажатии кнопки "Загрузить".
        /// Запрашивает подтверждение и инициирует загрузку.
        /// </summary>
        private void PerformLoad()
        {
            var saveInfo = GetSelectedSaveInfo();

            if (saveInfo == null)
                return;

            var result = MessageBox.Show(
                string.Format("Загрузить сохранение '{0}'?\nТекущий прогресс будет потерян.", saveInfo.SaveName),
                "Подтверждение загрузки",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                OnLoadRequested?.Invoke(saveInfo.SaveName);
            }
        }

        /// <summary>
        /// Выполняет удаление выбранного сохранения.
        /// 
        /// Вызывается при нажатии кнопки "Удалить".
        /// Запрашивает подтверждение и инициирует удаление.
        /// </summary>
        private void PerformDelete()
        {
            var saveInfo = GetSelectedSaveInfo();

            if (saveInfo == null)
                return;

            var result = MessageBox.Show(
                string.Format("Удалить сохранение '{0}'?\nЭто действие нельзя отменить.", saveInfo.SaveName),
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                OnDeleteRequested?.Invoke(saveInfo.SaveName);
                RefreshSaveList();
            }
        }

        #endregion

        #region Обработчики событий

        /// <summary>
        /// Обработчик кнопки сохранения.
        /// </summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            PerformSave();
        }

        /// <summary>
        /// Обработчик кнопки загрузки.
        /// </summary>
        private void btnLoad_Click(object sender, EventArgs e)
        {
            PerformLoad();
        }

        /// <summary>
        /// Обработчик кнопки удаления.
        /// </summary>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            PerformDelete();
        }

        /// <summary>
        /// Обработчик кнопки обновления списка.
        /// </summary>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshSaveList();
        }

        /// <summary>
        /// Обработчик выбора сохранения в списке.
        /// </summary>
        private void listBoxSaves_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtons();
            UpdateSaveInfo();
        }

        /// <summary>
        /// Обработчик изменения имени сохранения.
        /// </summary>
        private void txtSaveName_TextChanged(object sender, EventArgs e)
        {
            UpdateButtons();
        }

        /// <summary>
        /// Обработчик изменения настроек автосохранения.
        /// </summary>
        private void chkAutoSave_CheckedChanged(object sender, EventArgs e)
        {
            if (_saveLoadManager != null)
            {
                if (chkAutoSave.Checked)
                {
                    _saveLoadManager.AutoSaveInterval = (int)numAutoSaveInterval.Value * 60 * 1000;
                    _saveLoadManager.EnableAutoSave();
                }
                else
                {
                    _saveLoadManager.DisableAutoSave();
                }
            }
        }

        /// <summary>
        /// Обработчик изменения интервала автосохранения.
        /// </summary>
        private void numAutoSaveInterval_ValueChanged(object sender, EventArgs e)
        {
            if (_saveLoadManager != null && chkAutoSave.Checked)
            {
                _saveLoadManager.AutoSaveInterval = (int)numAutoSaveInterval.Value * 60 * 1000;
            }
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Обновляет информацию о выбранном сохранении.
        /// 
        /// Отображает детальную информацию:
        /// - Дата создания
        /// - Размер файла
        /// - Режим симуляции
        /// - Статистика
        /// </summary>
        private void UpdateSaveInfo()
        {
            var saveInfo = GetSelectedSaveInfo();

            if (saveInfo != null)
            {
                lblSaveInfo.Text = string.Format(
                    "Имя: {0}\nСоздано: {1}\nИзменено: {2}\n" +
                    "Размер: {3:F1} KB\nРежим: {4}\nПоколение: {5}\nЖивых клеток: {6}",
                    saveInfo.SaveName,
                    saveInfo.CreatedDate.ToString("dd.MM.yyyy HH:mm:ss"),
                    saveInfo.ModifiedDate.ToString("dd.MM.yyyy HH:mm:ss"),
                    saveInfo.FileSize / 1024.0,
                    saveInfo.SimulationMode,
                    saveInfo.LastGeneration,
                    saveInfo.LiveCellCount
                );
            }
            else
            {
                lblSaveInfo.Text = "Сохранение не выбрано";
            }
        }

        #endregion

        #region Вспомогательные классы

        /// <summary>
        /// Элемент списка для отображения сохранения.
        /// 
        /// Обёртка вокруг SaveInfo для корректного
        /// отображения в ListBox с переопределением ToString().
        /// </summary>
        private class SaveListItem
        {
            public SaveInfo SaveInfo { get; }

            public SaveListItem(SaveInfo saveInfo)
            {
                SaveInfo = saveInfo;
            }

            public override string ToString()
            {
                return string.Format(
                    "{0} | {1} | Gen: {2}",
                    SaveInfo.SaveName,
                    SaveInfo.ModifiedDate.ToString("dd.MM HH:mm"),
                    SaveInfo.LastGeneration
                );
            }
        }

        #endregion
    }
}