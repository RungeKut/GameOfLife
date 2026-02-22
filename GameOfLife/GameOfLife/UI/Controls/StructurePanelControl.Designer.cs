namespace GameOfLife.UI.Controls
{
    partial class StructurePanelControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            // Основные панели
            this.panelTop = new System.Windows.Forms.Panel();
            this.panelMiddle = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();

            // Выбор типа структуры
            this.lblStructureType = new System.Windows.Forms.Label();
            this.cmbStructureType = new System.Windows.Forms.ComboBox();

            // Информация о структуре
            this.grpStructureInfo = new System.Windows.Forms.GroupBox();
            this.lblStructureInfo = new System.Windows.Forms.Label();

            // Список построенных структур
            this.grpBuiltStructures = new System.Windows.Forms.GroupBox();
            this.listBoxStructures = new System.Windows.Forms.ListBox();

            // Кнопки управления
            this.btnBuild = new System.Windows.Forms.Button();
            this.btnUpgrade = new System.Windows.Forms.Button();
            this.btnRepair = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();

            // Статистика
            this.grpStatistics = new System.Windows.Forms.GroupBox();
            this.lblTotalStructures = new System.Windows.Forms.Label();
            this.lblTotalValue = new System.Windows.Forms.Label();

            // Tooltips
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);

            // panelTop
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Height = 70;
            this.panelTop.Padding = new System.Windows.Forms.Padding(5);

            // lblStructureType
            this.lblStructureType.AutoSize = true;
            this.lblStructureType.Location = new System.Drawing.Point(5, 10);
            this.lblStructureType.Text = "Тип структуры:";
            this.lblStructureType.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);

            // cmbStructureType
            this.cmbStructureType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStructureType.Location = new System.Drawing.Point(5, 30);
            this.cmbStructureType.Size = new System.Drawing.Size(180, 23);
            this.cmbStructureType.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.cmbStructureType.SelectedIndexChanged += new System.EventHandler(this.cmbStructureType_SelectedIndexChanged);
            this.toolTip.SetToolTip(this.cmbStructureType, "Выберите тип структуры для строительства");

            // btnBuild
            this.btnBuild.Location = new System.Drawing.Point(195, 30);
            this.btnBuild.Size = new System.Drawing.Size(100, 25);
            this.btnBuild.Text = "Построить";
            this.btnBuild.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnBuild.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.btnBuild.ForeColor = System.Drawing.Color.White;
            this.btnBuild.Enabled = false;
            this.btnBuild.Click += new System.EventHandler(this.btnBuild_Click);
            this.toolTip.SetToolTip(this.btnBuild, "Начать строительство выбранной структуры");

            // panelTop.Controls
            this.panelTop.Controls.Add(this.lblStructureType);
            this.panelTop.Controls.Add(this.cmbStructureType);
            this.panelTop.Controls.Add(this.btnBuild);

            // panelMiddle
            this.panelMiddle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMiddle.Padding = new System.Windows.Forms.Padding(5);

            // grpBuiltStructures
            this.grpBuiltStructures.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpBuiltStructures.Text = "Построенные структуры";
            this.grpBuiltStructures.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.grpBuiltStructures.Padding = new System.Windows.Forms.Padding(5);

            // listBoxStructures
            this.listBoxStructures.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxStructures.Font = new System.Drawing.Font("Consolas", 9F);
            this.listBoxStructures.FormattingEnabled = true;
            this.listBoxStructures.ItemHeight = 15;
            this.listBoxStructures.SelectedIndexChanged += new System.EventHandler(this.listBoxStructures_SelectedIndexChanged);
            this.toolTip.SetToolTip(this.listBoxStructures, "Список всех построенных структур. Выберите для управления.");

            // grpBuiltStructures.Controls
            this.grpBuiltStructures.Controls.Add(this.listBoxStructures);

            // panelMiddle.Controls
            this.panelMiddle.Controls.Add(this.grpBuiltStructures);

            // panelBottom
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Height = 140;
            this.panelBottom.Padding = new System.Windows.Forms.Padding(5);

            // grpStructureInfo
            this.grpStructureInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpStructureInfo.Height = 80;
            this.grpStructureInfo.Text = "Информация о структуре";
            this.grpStructureInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.grpStructureInfo.Padding = new System.Windows.Forms.Padding(5);

            // lblStructureInfo
            this.lblStructureInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStructureInfo.Font = new System.Drawing.Font("Consolas", 8F);
            this.lblStructureInfo.Text = "Структура не выбрана";
            this.lblStructureInfo.Padding = new System.Windows.Forms.Padding(3);

            // grpStructureInfo.Controls
            this.grpStructureInfo.Controls.Add(this.lblStructureInfo);

            // grpStatistics
            this.grpStatistics.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpStatistics.Height = 55;
            this.grpStatistics.Text = "Статистика";
            this.grpStatistics.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.grpStatistics.Padding = new System.Windows.Forms.Padding(5);

            // lblTotalStructures
            this.lblTotalStructures.AutoSize = true;
            this.lblTotalStructures.Location = new System.Drawing.Point(10, 20);
            this.lblTotalStructures.Text = "Всего структур:";
            this.lblTotalStructures.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);

            // lblTotalValue
            this.lblTotalValue.AutoSize = true;
            this.lblTotalValue.Location = new System.Drawing.Point(100, 20);
            this.lblTotalValue.Text = "0";
            this.lblTotalValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.lblTotalValue.ForeColor = System.Drawing.Color.FromArgb(0, 120, 215);

            // grpStatistics.Controls
            this.grpStatistics.Controls.Add(this.lblTotalStructures);
            this.grpStatistics.Controls.Add(this.lblTotalValue);

            // btnUpgrade
            this.btnUpgrade.Location = new System.Drawing.Point(5, 105);
            this.btnUpgrade.Size = new System.Drawing.Size(90, 25);
            this.btnUpgrade.Text = "Улучшить";
            this.btnUpgrade.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnUpgrade.Enabled = false;
            this.btnUpgrade.Click += new System.EventHandler(this.btnUpgrade_Click);
            this.toolTip.SetToolTip(this.btnUpgrade, "Улучшить выбранную структуру до следующего уровня");

            // btnRepair
            this.btnRepair.Location = new System.Drawing.Point(100, 105);
            this.btnRepair.Size = new System.Drawing.Size(90, 25);
            this.btnRepair.Text = "Ремонт";
            this.btnRepair.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnRepair.Enabled = false;
            this.btnRepair.Click += new System.EventHandler(this.btnRepair_Click);
            this.toolTip.SetToolTip(this.btnRepair, "Отремонтировать выбранную структуру");

            // btnRefresh
            this.btnRefresh.Location = new System.Drawing.Point(195, 105);
            this.btnRefresh.Size = new System.Drawing.Size(90, 25);
            this.btnRefresh.Text = "Обновить";
            this.btnRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            this.toolTip.SetToolTip(this.btnRefresh, "Обновить список структур");

            // panelBottom.Controls
            this.panelBottom.Controls.Add(this.grpStructureInfo);
            this.panelBottom.Controls.Add(this.grpStatistics);
            this.panelBottom.Controls.Add(this.btnUpgrade);
            this.panelBottom.Controls.Add(this.btnRepair);
            this.panelBottom.Controls.Add(this.btnRefresh);

            // StructurePanelControl
            this.Controls.Add(this.panelMiddle);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelTop);

            this.MinimumSize = new System.Drawing.Size(300, 400);
            this.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
        }

        #endregion

        #region Поля

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel panelMiddle;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Label lblStructureType;
        private System.Windows.Forms.ComboBox cmbStructureType;
        private System.Windows.Forms.GroupBox grpStructureInfo;
        private System.Windows.Forms.Label lblStructureInfo;
        private System.Windows.Forms.GroupBox grpBuiltStructures;
        private System.Windows.Forms.ListBox listBoxStructures;
        private System.Windows.Forms.Button btnBuild;
        private System.Windows.Forms.Button btnUpgrade;
        private System.Windows.Forms.Button btnRepair;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.GroupBox grpStatistics;
        private System.Windows.Forms.Label lblTotalStructures;
        private System.Windows.Forms.Label lblTotalValue;
        private System.Windows.Forms.ToolTip toolTip;

        #endregion
    }
}