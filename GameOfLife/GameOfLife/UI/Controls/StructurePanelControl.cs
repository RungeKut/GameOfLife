using System;
using System.Drawing;
using System.Windows.Forms;
using GameOfLife.Entities;
using GameOfLife.Resources;

namespace GameOfLife.UI.Controls
{
    /// <summary>
    /// Панель управления строительством структур.
    /// 
    /// Этот контрол предоставляет пользователю интерфейс для:
    /// - Выбора типа структуры для строительства
    /// - Просмотра стоимости строительства
    /// - Просмотра параметров структуры (здоровье, радиус действия)
    /// - Управления улучшением существующих структур
    /// - Просмотра статистики структур в мире
    /// 
    /// Архитектурные особенности:
    /// - Полностью отделён от логики симуляции
    /// - Использует события для уведомления об действиях пользователя
    /// - Поддерживает динамическое обновление доступности кнопок
    /// - Интегрируется с системой ресурсов для проверки стоимости
    /// 
    /// Использование:
    /// var panel = new StructurePanelControl();
    /// panel.OnStructureSelected += (type) => { /* строительство */ };
    /// panel.UpdateAvailableResources(botInventory);
    /// </summary>
    public partial class StructurePanelControl : UserControl
    {
        #region Приватные поля

        /// <summary>
        /// Менеджер структур для получения информации о построенных структурах.
        /// </summary>
        private StructureManager _structureManager;

        /// <summary>
        /// Текущий инвентарь бота для проверки доступности ресурсов.
        /// </summary>
        private ResourcePool _currentInventory;

        /// <summary>
        /// Выбранный тип структуры для строительства.
        /// </summary>
        private StructureType _selectedStructureType;

        /// <summary>
        /// Флаг блокировки интерфейса во время операций.
        /// </summary>
        private bool _isUpdating;

        #endregion

        #region События

        /// <summary>
        /// Событие выбора типа структуры для строительства.
        /// 
        /// Вызывается когда пользователь выбирает структуру из списка.
        /// Аргумент: выбранный тип структуры.
        /// 
        /// Подписчик должен обработать выбор и инициировать строительство.
        /// </summary>
        public event Action<StructureType> OnStructureSelected;

        /// <summary>
        /// Событие запроса на строительство структуры.
        /// 
        /// Вызывается когда пользователь нажимает кнопку "Построить".
        /// Аргументы: тип структуры, координаты X, координаты Y.
        /// 
        /// Подписчик должен проверить ресурсы и создать структуру.
        /// </summary>
        public event Action<StructureType, int, int> OnBuildRequested;

        /// <summary>
        /// Событие запроса на улучшение структуры.
        /// 
        /// Вызывается когда пользователь нажимает кнопку "Улучшить".
        /// Аргумент: ID структуры для улучшения.
        /// 
        /// Подписчик должен проверить ресурсы и улучшить структуру.
        /// </summary>
        public event Action<int> OnUpgradeRequested;

        /// <summary>
        /// Событие запроса на ремонт структуры.
        /// 
        /// Вызывается когда пользователь нажимает кнопку "Ремонт".
        /// Аргумент: ID структуры для ремонта.
        /// 
        /// Подписчик должен проверить ресурсы и отремонтировать структуру.
        /// </summary>
        public event Action<int> OnRepairRequested;

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Менеджер структур для получения информации.
        /// </summary>
        public StructureManager StructureManager
        {
            get => _structureManager;
            set
            {
                _structureManager = value;
                UpdateStructureList();
            }
        }

        /// <summary>
        /// Текущий инвентарь ресурсов.
        /// </summary>
        public ResourcePool CurrentInventory
        {
            get => _currentInventory;
            set
            {
                _currentInventory = value;
                UpdateButtonAvailability();
            }
        }

        /// <summary>
        /// Выбранный тип структуры.
        /// </summary>
        public StructureType SelectedStructureType
        {
            get => _selectedStructureType;
            private set
            {
                _selectedStructureType = value;
                UpdateStructureInfo();
            }
        }

        #endregion

        #region Конструктор

        /// <summary>
        /// Создаёт новую панель управления структурами.
        /// 
        /// Инициализирует компоненты интерфейса:
        /// - Список типов структур
        /// - Список построенных структур
        /// - Информационные метки
        /// - Кнопки управления
        /// </summary>
        public StructurePanelControl()
        {
            InitializeComponent();
            InitializeStructureTypes();
            _isUpdating = false;
        }

        #endregion

        #region Инициализация

        /// <summary>
        /// Инициализирует список доступных типов структур.
        /// 
        /// Добавляет все типы структур из enum StructureType
        /// в ComboBox для выбора пользователем.
        /// </summary>
        private void InitializeStructureTypes()
        {
            cmbStructureType.Items.Clear();

            foreach (StructureType type in Enum.GetValues(typeof(StructureType)))
            {
                cmbStructureType.Items.Add(GetStructureTypeName(type));
            }

            if (cmbStructureType.Items.Count > 0)
            {
                cmbStructureType.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Получает человеко-читаемое название типа структуры.
        /// </summary>
        private string GetStructureTypeName(StructureType type)
        {
            switch (type)
            {
                case StructureType.Shelter:
                    return "Укрытие";
                case StructureType.Storage:
                    return "Хранилище";
                case StructureType.Base:
                    return "База";
                case StructureType.Trap:
                    return "Ловушка";
                case StructureType.Wall:
                    return "Стена";
                case StructureType.Generator:
                    return "Генератор";
                case StructureType.Farm:
                    return "Ферма";
                case StructureType.Laboratory:
                    return "Лаборатория";
                default:
                    return type.ToString();
            }
        }

        #endregion

        #region Обновление интерфейса

        /// <summary>
        /// Обновляет список построенных структур.
        /// 
        /// Вызывается при изменении состояния мира.
        /// Заполняет listBox с информацией о каждой структуре:
        /// - Тип и уровень
        /// - Координаты
        /// - Текущее здоровье
        /// - Владелец
        /// </summary>
        public void UpdateStructureList()
        {
            if (_isUpdating || _structureManager == null)
                return;

            _isUpdating = true;

            try
            {
                listBoxStructures.Items.Clear();

                foreach (var structure in _structureManager.AllStructures)
                {
                    string info = string.Format(
                        "{0} (Lvl:{1}) at ({2},{3}) HP:{4:F0}/{5:F0}",
                        GetStructureTypeName(structure.Type),
                        structure.Level,
                        structure.X,
                        structure.Y,
                        structure.CurrentHealth,
                        structure.MaxHealth
                    );

                    listBoxStructures.Items.Add(new StructureListItem(structure));
                }

                if (listBoxStructures.Items.Count > 0)
                {
                    listBoxStructures.SelectedIndex = 0;
                }
            }
            finally
            {
                _isUpdating = false;
            }
        }

        /// <summary>
        /// Обновляет информацию о выбранной структуре.
        /// 
        /// Вызывается при выборе структуры в списке.
        /// Отображает детальную информацию:
        /// - Полное описание
        /// - Стоимость улучшения
        /// - Доступные действия
        /// </summary>
        private void UpdateStructureInfo()
        {
            if (listBoxStructures.SelectedItem is StructureListItem item)
            {
                var structure = item.Structure;

                lblStructureInfo.Text = string.Format(
                    "Тип: {0}\nУровень: {1}/{2}\nЗдоровье: {3:F0}/{4:F0} ({5:F1}%)\n" +
                    "Владелец: {6}\nРадиус: {7}\nСтоимость улучшения: {8}",
                    GetStructureTypeName(structure.Type),
                    structure.Level,
                    structure.MaxLevel,
                    structure.CurrentHealth,
                    structure.MaxHealth,
                    structure.GetHealthPercentage(),
                    structure.Owner?.Id.ToString() ?? "Никто",
                    structure.EffectRadius,
                    GetUpgradeCostString(structure)
                );

                // Обновляем доступность кнопок
                btnUpgrade.Enabled = structure.CanUpgrade();
                btnRepair.Enabled = structure.CurrentHealth < structure.MaxHealth;
            }
            else
            {
                lblStructureInfo.Text = "Структура не выбрана";
                btnUpgrade.Enabled = false;
                btnRepair.Enabled = false;
            }
        }

        /// <summary>
        /// Обновляет доступность кнопок строительства.
        /// 
        /// Проверяет наличие ресурсов в инвентаре
        /// и включает/выключает кнопки строительства.
        /// </summary>
        public void UpdateButtonAvailability()
        {
            if (_currentInventory == null)
            {
                btnBuild.Enabled = false;
                return;
            }

            // Проверяем доступность ресурсов для выбранной структуры
            var cost = GetBuildCost(SelectedStructureType);
            btnBuild.Enabled = _currentInventory.HasEnoughMany(cost.ToDictionary());
        }

        /// <summary>
        /// Получает строку стоимости улучшения.
        /// </summary>
        private string GetUpgradeCostString(Structure structure)
        {
            var cost = structure.GetUpgradeCost();
            var parts = new System.Collections.Generic.List<string>();

            foreach (var type in cost.ResourceTypes)
            {
                parts.Add(string.Format("{0:F0} {1}", cost.GetAmount(type), type.Name));
            }

            return parts.Count > 0 ? string.Join(", ", parts) : "Бесплатно";
        }

        /// <summary>
        /// Получает стоимость строительства для типа структуры.
        /// </summary>
        private ResourcePool GetBuildCost(StructureType type)
        {
            var cost = new ResourcePool(-1);

            switch (type)
            {
                case StructureType.Shelter:
                    cost.Add(ResourceType.Metal, 10);
                    break;
                case StructureType.Storage:
                    cost.Add(ResourceType.Metal, 15);
                    break;
                case StructureType.Base:
                    cost.Add(ResourceType.Metal, 50);
                    break;
                case StructureType.Trap:
                    cost.Add(ResourceType.Metal, 5);
                    break;
                case StructureType.Wall:
                    cost.Add(ResourceType.Metal, 8);
                    break;
                default:
                    cost.Add(ResourceType.Metal, 10);
                    break;
            }

            return cost;
        }

        #endregion

        #region Обработчики событий

        /// <summary>
        /// Обработчик выбора типа структуры.
        /// </summary>
        private void cmbStructureType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbStructureType.SelectedItem != null)
            {
                string selectedName = cmbStructureType.SelectedItem.ToString();

                // Находим соответствующий тип структуры
                foreach (StructureType type in Enum.GetValues(typeof(StructureType)))
                {
                    if (GetStructureTypeName(type) == selectedName)
                    {
                        SelectedStructureType = type;
                        OnStructureSelected?.Invoke(type);
                        UpdateButtonAvailability();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Обработчик выбора структуры из списка.
        /// </summary>
        private void listBoxStructures_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateStructureInfo();
        }

        /// <summary>
        /// Обработчик кнопки строительства.
        /// </summary>
        private void btnBuild_Click(object sender, EventArgs e)
        {
            // В полной реализации: открытие режима строительства на карте
            MessageBox.Show(
                "Режим строительства: кликните на карту для размещения структуры",
                "Строительство",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        /// <summary>
        /// Обработчик кнопки улучшения.
        /// </summary>
        private void btnUpgrade_Click(object sender, EventArgs e)
        {
            if (listBoxStructures.SelectedItem is StructureListItem item)
            {
                OnUpgradeRequested?.Invoke(item.Structure.Id);
            }
        }

        /// <summary>
        /// Обработчик кнопки ремонта.
        /// </summary>
        private void btnRepair_Click(object sender, EventArgs e)
        {
            if (listBoxStructures.SelectedItem is StructureListItem item)
            {
                OnRepairRequested?.Invoke(item.Structure.Id);
            }
        }

        /// <summary>
        /// Обработчик обновления списка структур.
        /// </summary>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            UpdateStructureList();
        }

        #endregion

        #region Вспомогательные классы

        /// <summary>
        /// Элемент списка для отображения структуры.
        /// 
        /// Обёртка вокруг Structure для корректного
        /// отображения в ListBox с переопределением ToString().
        /// </summary>
        private class StructureListItem
        {
            public Structure Structure { get; }

            public StructureListItem(Structure structure)
            {
                Structure = structure;
            }

            public override string ToString()
            {
                return string.Format(
                    "{0} ({1},{2}) Lvl:{3} HP:{4:F0}/{5:F0}",
                    Enum.GetName(typeof(StructureType), Structure.Type),
                    Structure.X,
                    Structure.Y,
                    Structure.Level,
                    Structure.CurrentHealth,
                    Structure.MaxHealth
                );
            }
        }

        #endregion
    }
}