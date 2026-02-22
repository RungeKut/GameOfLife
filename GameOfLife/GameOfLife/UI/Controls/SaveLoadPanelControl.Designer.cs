namespace GameOfLife.UI.Controls
{
    partial class SaveLoadPanelControl
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

            // Ввод имени сохранения
            this.lblSaveName = new System.Windows.Forms.Label();
            this.txtSaveName = new System.Windows.Forms.TextBox();

            // Кнопки операций
            this.btnSave = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();

            // Список сохранений
            this.grpSaves = new System.Windows.Forms.GroupBox();
            this.listBoxSaves = new System.Windows.Forms.ListBox();

            // Информация о сохранении
            this.grpSaveInfo = new System.Windows.Forms.GroupBox();
            this.lblSaveInfo = new System.Windows.Forms.Label();

            // Автосохранение
            this.grpAutoSave = new System.Windows.Forms.GroupBox();
            this.chkAutoSave = new System.Windows.Forms.CheckBox();
            this.lblAutoSaveInterval = new System.Windows.Forms.Label();
            this.numAutoSaveInterval = new System.Windows.Forms.NumericUpDown();
            this.lblAutoSaveMinutes = new System.Windows.Forms.Label();

            // Tooltips
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);

            // panelTop
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Height = 60;
            this.panelTop.Padding = new System.Windows.Forms.Padding(5);

            // lblSaveName
            this.lblSaveName.AutoSize = true;
            this.lblSaveName.Location = new System.Drawing.Point(5, 10);
            this.lblSaveName.Text = "Имя сохранения:";
            this.lblSaveName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);

            // txtSaveName
            this.txtSaveName.Location = new System.Drawing.Point(5, 30);
            this.txtSaveName.Size = new System.Drawing.Size(180, 20);
            this.txtSaveName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtSaveName.MaxLength = 50;
            this.txtSaveName.TextChanged += new System.EventHandler(this.txtSaveName_TextChanged);
            this.toolTip.SetToolTip(this.txtSaveName, "Введите имя для нового сохранения");

            // btnSave
            this.btnSave.Location = new System.Drawing.Point(195, 30);
            this.btnSave.Size = new System.Drawing.Size(90, 25);
            this.btnSave.Text = "Сохранить";
            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Enabled = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            this.toolTip.SetToolTip(this.btnSave, "Сохранить текущее состояние симуляции");

            // panelTop.Controls
            this.panelTop.Controls.Add(this.lblSaveName);
            this.panelTop.Controls.Add(this.txtSaveName);
            this.panelTop.Controls.Add(this.btnSave);

            // panelMiddle
            this.panelMiddle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMiddle.Padding = new System.Windows.Forms.Padding(5);

            // grpSaves
            this.grpSaves.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpSaves.Text = "Список сохранений";
            this.grpSaves.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.grpSaves.Padding = new System.Windows.Forms.Padding(5);

            // listBoxSaves
            this.listBoxSaves.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxSaves.Font = new System.Drawing.Font("Consolas", 9F);
            this.listBoxSaves.FormattingEnabled = true;
            this.listBoxSaves.ItemHeight = 15;
            this.listBoxSaves.SelectedIndexChanged += new System.EventHandler(this.listBoxSaves_SelectedIndexChanged);
            this.toolTip.SetToolTip(this.listBoxSaves, "Список всех сохранений. Выберите для загрузки или удаления.");

            // grpSaves.Controls
            this.grpSaves.Controls.Add(this.listBoxSaves);

            // panelMiddle.Controls
            this.panelMiddle.Controls.Add(this.grpSaves);

            // panelBottom
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Height = 150;
            this.panelBottom.Padding = new System.Windows.Forms.Padding(5);

            // grpSaveInfo
            this.grpSaveInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpSaveInfo.Height = 70;
            this.grpSaveInfo.Text = "Информация о сохранении";
            this.grpSaveInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.grpSaveInfo.Padding = new System.Windows.Forms.Padding(5);

            // lblSaveInfo
            this.lblSaveInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSaveInfo.Font = new System.Drawing.Font("Consolas", 8F);
            this.lblSaveInfo.Text = "Сохранение не выбрано";
            this.lblSaveInfo.Padding = new System.Windows.Forms.Padding(3);

            // grpSaveInfo.Controls
            this.grpSaveInfo.Controls.Add(this.lblSaveInfo);

            // grpAutoSave
            this.grpAutoSave.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpAutoSave.Height = 50;
            this.grpAutoSave.Text = "Автосохранение";
            this.grpAutoSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.grpAutoSave.Padding = new System.Windows.Forms.Padding(5);

            // chkAutoSave
            this.chkAutoSave.AutoSize = true;
            this.chkAutoSave.Location = new System.Drawing.Point(10, 20);
            this.chkAutoSave.Text = "Включить";
            this.chkAutoSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.chkAutoSave.CheckedChanged += new System.EventHandler(this.chkAutoSave_CheckedChanged);
            this.toolTip.SetToolTip(this.chkAutoSave, "Включить автоматическое сохранение");

            // lblAutoSaveInterval
            this.lblAutoSaveInterval.AutoSize = true;
            this.lblAutoSaveInterval.Location = new System.Drawing.Point(100, 22);
            this.lblAutoSaveInterval.Text = "Интервал:";
            this.lblAutoSaveInterval.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);

            // numAutoSaveInterval
            this.numAutoSaveInterval.Location = new System.Drawing.Point(155, 18);
            this.numAutoSaveInterval.Size = new System.Drawing.Size(50, 20);
            this.numAutoSaveInterval.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.numAutoSaveInterval.Minimum = 1;
            this.numAutoSaveInterval.Maximum = 60;
            this.numAutoSaveInterval.Value = 5;
            this.numAutoSaveInterval.ValueChanged += new System.EventHandler(this.numAutoSaveInterval_ValueChanged);
            this.toolTip.SetToolTip(this.numAutoSaveInterval, "Интервал автосохранения в минутах");

            // lblAutoSaveMinutes
            this.lblAutoSaveMinutes.AutoSize = true;
            this.lblAutoSaveMinutes.Location = new System.Drawing.Point(210, 22);
            this.lblAutoSaveMinutes.Text = "мин";
            this.lblAutoSaveMinutes.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);

            // grpAutoSave.Controls
            this.grpAutoSave.Controls.Add(this.chkAutoSave);
            this.grpAutoSave.Controls.Add(this.lblAutoSaveInterval);
            this.grpAutoSave.Controls.Add(this.numAutoSaveInterval);
            this.grpAutoSave.Controls.Add(this.lblAutoSaveMinutes);

            // btnLoad
            this.btnLoad.Location = new System.Drawing.Point(5, 120);
            this.btnLoad.Size = new System.Drawing.Size(90, 25);
            this.btnLoad.Text = "Загрузить";
            this.btnLoad.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnLoad.Enabled = false;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            this.toolTip.SetToolTip(this.btnLoad, "Загрузить выбранное сохранение");

            // btnDelete
            this.btnDelete.Location = new System.Drawing.Point(100, 120);
            this.btnDelete.Size = new System.Drawing.Size(90, 25);
            this.btnDelete.Text = "Удалить";
            this.btnDelete.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnDelete.Enabled = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            this.toolTip.SetToolTip(this.btnDelete, "Удалить выбранное сохранение");

            // btnRefresh
            this.btnRefresh.Location = new System.Drawing.Point(195, 120);
            this.btnRefresh.Size = new System.Drawing.Size(90, 25);
            this.btnRefresh.Text = "Обновить";
            this.btnRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            this.toolTip.SetToolTip(this.btnRefresh, "Обновить список сохранений");

            // panelBottom.Controls
            this.panelBottom.Controls.Add(this.grpSaveInfo);
            this.panelBottom.Controls.Add(this.grpAutoSave);
            this.panelBottom.Controls.Add(this.btnLoad);
            this.panelBottom.Controls.Add(this.btnDelete);
            this.panelBottom.Controls.Add(this.btnRefresh);

            // SaveLoadPanelControl
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
        private System.Windows.Forms.Label lblSaveName;
        private System.Windows.Forms.TextBox txtSaveName;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.GroupBox grpSaves;
        private System.Windows.Forms.ListBox listBoxSaves;
        private System.Windows.Forms.GroupBox grpSaveInfo;
        private System.Windows.Forms.Label lblSaveInfo;
        private System.Windows.Forms.GroupBox grpAutoSave;
        private System.Windows.Forms.CheckBox chkAutoSave;
        private System.Windows.Forms.Label lblAutoSaveInterval;
        private System.Windows.Forms.NumericUpDown numAutoSaveInterval;
        private System.Windows.Forms.Label lblAutoSaveMinutes;
        private System.Windows.Forms.ToolTip toolTip;

        #endregion
    }
}