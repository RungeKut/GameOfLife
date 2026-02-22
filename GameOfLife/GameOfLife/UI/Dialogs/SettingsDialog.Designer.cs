namespace GameOfLife.UI.Dialogs
{
    partial class SettingsDialog
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

        #region Код, автоматически созданный конструктором форм

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            tabControl = new System.Windows.Forms.TabControl();
            tabSimulation = new System.Windows.Forms.TabPage();
            lblWorldWidth = new System.Windows.Forms.Label();
            numWorldWidth = new System.Windows.Forms.NumericUpDown();
            lblWorldHeight = new System.Windows.Forms.Label();
            numWorldHeight = new System.Windows.Forms.NumericUpDown();
            lblTickDelay = new System.Windows.Forms.Label();
            numTickDelay = new System.Windows.Forms.NumericUpDown();
            lblInitialDensity = new System.Windows.Forms.Label();
            numInitialDensity = new System.Windows.Forms.NumericUpDown();
            chkWrapAround = new System.Windows.Forms.CheckBox();
            chkParallelProcessing = new System.Windows.Forms.CheckBox();
            lblThreads = new System.Windows.Forms.Label();
            numThreads = new System.Windows.Forms.NumericUpDown();
            chkSeed = new System.Windows.Forms.CheckBox();
            numSeed = new System.Windows.Forms.NumericUpDown();
            tabVisualization = new System.Windows.Forms.TabPage();
            lblCellSize = new System.Windows.Forms.Label();
            numCellSize = new System.Windows.Forms.NumericUpDown();
            lblAliveColor = new System.Windows.Forms.Label();
            btnAliveColor = new System.Windows.Forms.Button();
            lblDeadColor = new System.Windows.Forms.Label();
            btnDeadColor = new System.Windows.Forms.Button();
            lblGridColor = new System.Windows.Forms.Label();
            btnGridColor = new System.Windows.Forms.Button();
            lblGridThickness = new System.Windows.Forms.Label();
            numGridThickness = new System.Windows.Forms.NumericUpDown();
            chkShowGrid = new System.Windows.Forms.CheckBox();
            chkShowGenerationInfo = new System.Windows.Forms.CheckBox();
            tabMode = new System.Windows.Forms.TabPage();
            lblSimulationMode = new System.Windows.Forms.Label();
            cmbSimulationMode = new System.Windows.Forms.ComboBox();
            lblModeDescription = new System.Windows.Forms.Label();
            btnOK = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            btnApply = new System.Windows.Forms.Button();
            btnReset = new System.Windows.Forms.Button();
            colorAliveColor = new System.Windows.Forms.ColorDialog();
            colorDeadColor = new System.Windows.Forms.ColorDialog();
            colorGridColor = new System.Windows.Forms.ColorDialog();
            toolTip = new System.Windows.Forms.ToolTip(components);
            tabControl.SuspendLayout();
            tabSimulation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numWorldWidth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numWorldHeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numTickDelay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numInitialDensity).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numThreads).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSeed).BeginInit();
            tabVisualization.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCellSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numGridThickness).BeginInit();
            tabMode.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabSimulation);
            tabControl.Controls.Add(tabVisualization);
            tabControl.Controls.Add(tabMode);
            tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            tabControl.Location = new System.Drawing.Point(0, 0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new System.Drawing.Size(550, 284);
            tabControl.TabIndex = 0;
            // 
            // tabSimulation
            // 
            tabSimulation.AutoScroll = true;
            tabSimulation.Controls.Add(lblWorldWidth);
            tabSimulation.Controls.Add(numWorldWidth);
            tabSimulation.Controls.Add(lblWorldHeight);
            tabSimulation.Controls.Add(numWorldHeight);
            tabSimulation.Controls.Add(lblTickDelay);
            tabSimulation.Controls.Add(numTickDelay);
            tabSimulation.Controls.Add(lblInitialDensity);
            tabSimulation.Controls.Add(numInitialDensity);
            tabSimulation.Controls.Add(chkWrapAround);
            tabSimulation.Controls.Add(chkParallelProcessing);
            tabSimulation.Controls.Add(lblThreads);
            tabSimulation.Controls.Add(numThreads);
            tabSimulation.Controls.Add(chkSeed);
            tabSimulation.Controls.Add(numSeed);
            tabSimulation.Location = new System.Drawing.Point(4, 24);
            tabSimulation.Name = "tabSimulation";
            tabSimulation.Padding = new System.Windows.Forms.Padding(10);
            tabSimulation.Size = new System.Drawing.Size(542, 256);
            tabSimulation.TabIndex = 0;
            tabSimulation.Text = "Симуляция";
            // 
            // lblWorldWidth
            // 
            lblWorldWidth.AutoSize = true;
            lblWorldWidth.Location = new System.Drawing.Point(10, 15);
            lblWorldWidth.Name = "lblWorldWidth";
            lblWorldWidth.Size = new System.Drawing.Size(89, 15);
            lblWorldWidth.TabIndex = 0;
            lblWorldWidth.Text = "Ширина мира:";
            // 
            // numWorldWidth
            // 
            numWorldWidth.Location = new System.Drawing.Point(150, 13);
            numWorldWidth.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numWorldWidth.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            numWorldWidth.Name = "numWorldWidth";
            numWorldWidth.Size = new System.Drawing.Size(100, 21);
            numWorldWidth.TabIndex = 1;
            numWorldWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            toolTip.SetToolTip(numWorldWidth, "Ширина мира в клетках (10-10000)");
            numWorldWidth.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // lblWorldHeight
            // 
            lblWorldHeight.AutoSize = true;
            lblWorldHeight.Location = new System.Drawing.Point(10, 45);
            lblWorldHeight.Name = "lblWorldHeight";
            lblWorldHeight.Size = new System.Drawing.Size(87, 15);
            lblWorldHeight.TabIndex = 2;
            lblWorldHeight.Text = "Высота мира:";
            // 
            // numWorldHeight
            // 
            numWorldHeight.Location = new System.Drawing.Point(150, 43);
            numWorldHeight.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numWorldHeight.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            numWorldHeight.Name = "numWorldHeight";
            numWorldHeight.Size = new System.Drawing.Size(100, 21);
            numWorldHeight.TabIndex = 3;
            numWorldHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            toolTip.SetToolTip(numWorldHeight, "Высота мира в клетках (10-10000)");
            numWorldHeight.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // lblTickDelay
            // 
            lblTickDelay.AutoSize = true;
            lblTickDelay.Location = new System.Drawing.Point(10, 75);
            lblTickDelay.Name = "lblTickDelay";
            lblTickDelay.Size = new System.Drawing.Size(94, 15);
            lblTickDelay.TabIndex = 4;
            lblTickDelay.Text = "Задержка (мс):";
            // 
            // numTickDelay
            // 
            numTickDelay.Location = new System.Drawing.Point(150, 73);
            numTickDelay.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numTickDelay.Name = "numTickDelay";
            numTickDelay.Size = new System.Drawing.Size(100, 21);
            numTickDelay.TabIndex = 5;
            numTickDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            toolTip.SetToolTip(numTickDelay, "Задержка между тиками в миллисекундах (0-10000)");
            numTickDelay.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // lblInitialDensity
            // 
            lblInitialDensity.AutoSize = true;
            lblInitialDensity.Location = new System.Drawing.Point(10, 105);
            lblInitialDensity.Name = "lblInitialDensity";
            lblInitialDensity.Size = new System.Drawing.Size(96, 15);
            lblInitialDensity.TabIndex = 6;
            lblInitialDensity.Text = "Плотность (%):";
            // 
            // numInitialDensity
            // 
            numInitialDensity.Location = new System.Drawing.Point(150, 103);
            numInitialDensity.Name = "numInitialDensity";
            numInitialDensity.Size = new System.Drawing.Size(100, 21);
            numInitialDensity.TabIndex = 7;
            numInitialDensity.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            toolTip.SetToolTip(numInitialDensity, "Плотность случайного заполнения (0-100%)");
            numInitialDensity.Value = new decimal(new int[] { 25, 0, 0, 0 });
            // 
            // chkWrapAround
            // 
            chkWrapAround.AutoSize = true;
            chkWrapAround.Checked = true;
            chkWrapAround.CheckState = System.Windows.Forms.CheckState.Checked;
            chkWrapAround.Location = new System.Drawing.Point(10, 135);
            chkWrapAround.Name = "chkWrapAround";
            chkWrapAround.Size = new System.Drawing.Size(153, 19);
            chkWrapAround.TabIndex = 8;
            chkWrapAround.Text = "Зацикливание границ";
            toolTip.SetToolTip(chkWrapAround, "Включить тороидальную топологию мира");
            // 
            // chkParallelProcessing
            // 
            chkParallelProcessing.AutoSize = true;
            chkParallelProcessing.Checked = true;
            chkParallelProcessing.CheckState = System.Windows.Forms.CheckState.Checked;
            chkParallelProcessing.Location = new System.Drawing.Point(10, 160);
            chkParallelProcessing.Name = "chkParallelProcessing";
            chkParallelProcessing.Size = new System.Drawing.Size(187, 19);
            chkParallelProcessing.TabIndex = 9;
            chkParallelProcessing.Text = "Параллельные вычисления";
            toolTip.SetToolTip(chkParallelProcessing, "Использовать многопоточность для вычислений");
            // 
            // lblThreads
            // 
            lblThreads.AutoSize = true;
            lblThreads.Location = new System.Drawing.Point(10, 190);
            lblThreads.Name = "lblThreads";
            lblThreads.Size = new System.Drawing.Size(130, 15);
            lblThreads.TabIndex = 10;
            lblThreads.Text = "Количество потоков:";
            // 
            // numThreads
            // 
            numThreads.Location = new System.Drawing.Point(150, 188);
            numThreads.Maximum = new decimal(new int[] { 64, 0, 0, 0 });
            numThreads.Name = "numThreads";
            numThreads.Size = new System.Drawing.Size(100, 21);
            numThreads.TabIndex = 11;
            numThreads.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            toolTip.SetToolTip(numThreads, "0 = использовать все доступные процессоры");
            // 
            // chkSeed
            // 
            chkSeed.AutoSize = true;
            chkSeed.Location = new System.Drawing.Point(10, 220);
            chkSeed.Name = "chkSeed";
            chkSeed.Size = new System.Drawing.Size(154, 19);
            chkSeed.TabIndex = 12;
            chkSeed.Text = "Фиксированный seed:";
            toolTip.SetToolTip(chkSeed, "Использовать фиксированный seed для воспроизводимости");
            chkSeed.CheckedChanged += chkSeed_CheckedChanged;
            // 
            // numSeed
            // 
            numSeed.Enabled = false;
            numSeed.Location = new System.Drawing.Point(150, 218);
            numSeed.Maximum = new decimal(new int[] { int.MaxValue, 0, 0, 0 });
            numSeed.Name = "numSeed";
            numSeed.Size = new System.Drawing.Size(100, 21);
            numSeed.TabIndex = 13;
            numSeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            toolTip.SetToolTip(numSeed, "Seed для генератора случайных чисел");
            numSeed.Value = new decimal(new int[] { 42, 0, 0, 0 });
            // 
            // tabVisualization
            // 
            tabVisualization.AutoScroll = true;
            tabVisualization.Controls.Add(lblCellSize);
            tabVisualization.Controls.Add(numCellSize);
            tabVisualization.Controls.Add(lblAliveColor);
            tabVisualization.Controls.Add(btnAliveColor);
            tabVisualization.Controls.Add(lblDeadColor);
            tabVisualization.Controls.Add(btnDeadColor);
            tabVisualization.Controls.Add(lblGridColor);
            tabVisualization.Controls.Add(btnGridColor);
            tabVisualization.Controls.Add(lblGridThickness);
            tabVisualization.Controls.Add(numGridThickness);
            tabVisualization.Controls.Add(chkShowGrid);
            tabVisualization.Controls.Add(chkShowGenerationInfo);
            tabVisualization.Location = new System.Drawing.Point(4, 24);
            tabVisualization.Name = "tabVisualization";
            tabVisualization.Padding = new System.Windows.Forms.Padding(10);
            tabVisualization.Size = new System.Drawing.Size(542, 312);
            tabVisualization.TabIndex = 1;
            tabVisualization.Text = "Визуализация";
            // 
            // lblCellSize
            // 
            lblCellSize.AutoSize = true;
            lblCellSize.Location = new System.Drawing.Point(10, 15);
            lblCellSize.Name = "lblCellSize";
            lblCellSize.Size = new System.Drawing.Size(97, 15);
            lblCellSize.TabIndex = 0;
            lblCellSize.Text = "Размер клетки:";
            // 
            // numCellSize
            // 
            numCellSize.Location = new System.Drawing.Point(150, 13);
            numCellSize.Maximum = new decimal(new int[] { 64, 0, 0, 0 });
            numCellSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numCellSize.Name = "numCellSize";
            numCellSize.Size = new System.Drawing.Size(100, 21);
            numCellSize.TabIndex = 1;
            numCellSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            toolTip.SetToolTip(numCellSize, "Размер клетки в пикселях (1-64)");
            numCellSize.Value = new decimal(new int[] { 4, 0, 0, 0 });
            // 
            // lblAliveColor
            // 
            lblAliveColor.AutoSize = true;
            lblAliveColor.Location = new System.Drawing.Point(10, 45);
            lblAliveColor.Name = "lblAliveColor";
            lblAliveColor.Size = new System.Drawing.Size(123, 15);
            lblAliveColor.TabIndex = 2;
            lblAliveColor.Text = "Цвет живой клетки:";
            // 
            // btnAliveColor
            // 
            btnAliveColor.BackColor = System.Drawing.Color.Crimson;
            btnAliveColor.Location = new System.Drawing.Point(150, 40);
            btnAliveColor.Name = "btnAliveColor";
            btnAliveColor.Size = new System.Drawing.Size(100, 25);
            btnAliveColor.TabIndex = 3;
            btnAliveColor.Text = "Выбрать цвет";
            toolTip.SetToolTip(btnAliveColor, "Выбрать цвет для живых клеток");
            btnAliveColor.UseVisualStyleBackColor = false;
            btnAliveColor.Click += btnAliveColor_Click;
            // 
            // lblDeadColor
            // 
            lblDeadColor.AutoSize = true;
            lblDeadColor.Location = new System.Drawing.Point(10, 75);
            lblDeadColor.Name = "lblDeadColor";
            lblDeadColor.Size = new System.Drawing.Size(75, 15);
            lblDeadColor.TabIndex = 4;
            lblDeadColor.Text = "Цвет фона:";
            // 
            // btnDeadColor
            // 
            btnDeadColor.BackColor = System.Drawing.Color.Black;
            btnDeadColor.ForeColor = System.Drawing.Color.White;
            btnDeadColor.Location = new System.Drawing.Point(150, 70);
            btnDeadColor.Name = "btnDeadColor";
            btnDeadColor.Size = new System.Drawing.Size(100, 25);
            btnDeadColor.TabIndex = 5;
            btnDeadColor.Text = "Выбрать цвет";
            toolTip.SetToolTip(btnDeadColor, "Выбрать цвет фона (мёртвых клеток)");
            btnDeadColor.UseVisualStyleBackColor = false;
            btnDeadColor.Click += btnDeadColor_Click;
            // 
            // lblGridColor
            // 
            lblGridColor.AutoSize = true;
            lblGridColor.Location = new System.Drawing.Point(10, 105);
            lblGridColor.Name = "lblGridColor";
            lblGridColor.Size = new System.Drawing.Size(76, 15);
            lblGridColor.TabIndex = 6;
            lblGridColor.Text = "Цвет сетки:";
            // 
            // btnGridColor
            // 
            btnGridColor.BackColor = System.Drawing.Color.DarkGray;
            btnGridColor.Location = new System.Drawing.Point(150, 100);
            btnGridColor.Name = "btnGridColor";
            btnGridColor.Size = new System.Drawing.Size(100, 25);
            btnGridColor.TabIndex = 7;
            btnGridColor.Text = "Выбрать цвет";
            toolTip.SetToolTip(btnGridColor, "Выбрать цвет для сетки");
            btnGridColor.UseVisualStyleBackColor = false;
            btnGridColor.Click += btnGridColor_Click;
            // 
            // lblGridThickness
            // 
            lblGridThickness.AutoSize = true;
            lblGridThickness.Location = new System.Drawing.Point(10, 135);
            lblGridThickness.Name = "lblGridThickness";
            lblGridThickness.Size = new System.Drawing.Size(97, 15);
            lblGridThickness.TabIndex = 8;
            lblGridThickness.Text = "Толщина сетки:";
            // 
            // numGridThickness
            // 
            numGridThickness.Location = new System.Drawing.Point(150, 133);
            numGridThickness.Maximum = new decimal(new int[] { 5, 0, 0, 0 });
            numGridThickness.Name = "numGridThickness";
            numGridThickness.Size = new System.Drawing.Size(100, 21);
            numGridThickness.TabIndex = 9;
            numGridThickness.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            toolTip.SetToolTip(numGridThickness, "Толщина линии сетки в пикселях (0-5)");
            numGridThickness.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // chkShowGrid
            // 
            chkShowGrid.AutoSize = true;
            chkShowGrid.Location = new System.Drawing.Point(10, 165);
            chkShowGrid.Name = "chkShowGrid";
            chkShowGrid.Size = new System.Drawing.Size(132, 19);
            chkShowGrid.TabIndex = 10;
            chkShowGrid.Text = "Показывать сетку";
            toolTip.SetToolTip(chkShowGrid, "Отображать сетку между клетками");
            // 
            // chkShowGenerationInfo
            // 
            chkShowGenerationInfo.AutoSize = true;
            chkShowGenerationInfo.Location = new System.Drawing.Point(10, 190);
            chkShowGenerationInfo.Name = "chkShowGenerationInfo";
            chkShowGenerationInfo.Size = new System.Drawing.Size(254, 19);
            chkShowGenerationInfo.TabIndex = 11;
            chkShowGenerationInfo.Text = "Показывать информацию о поколении";
            toolTip.SetToolTip(chkShowGenerationInfo, "Отображать номер поколения и количество живых клеток");
            // 
            // tabMode
            // 
            tabMode.Controls.Add(lblSimulationMode);
            tabMode.Controls.Add(cmbSimulationMode);
            tabMode.Controls.Add(lblModeDescription);
            tabMode.Location = new System.Drawing.Point(4, 24);
            tabMode.Name = "tabMode";
            tabMode.Padding = new System.Windows.Forms.Padding(10);
            tabMode.Size = new System.Drawing.Size(542, 312);
            tabMode.TabIndex = 2;
            tabMode.Text = "Режим";
            // 
            // lblSimulationMode
            // 
            lblSimulationMode.AutoSize = true;
            lblSimulationMode.Location = new System.Drawing.Point(10, 15);
            lblSimulationMode.Name = "lblSimulationMode";
            lblSimulationMode.Size = new System.Drawing.Size(115, 15);
            lblSimulationMode.TabIndex = 0;
            lblSimulationMode.Text = "Режим симуляции:";
            // 
            // cmbSimulationMode
            // 
            cmbSimulationMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbSimulationMode.Location = new System.Drawing.Point(150, 13);
            cmbSimulationMode.Name = "cmbSimulationMode";
            cmbSimulationMode.Size = new System.Drawing.Size(200, 23);
            cmbSimulationMode.TabIndex = 1;
            toolTip.SetToolTip(cmbSimulationMode, "Выберите режим симуляции");
            // 
            // lblModeDescription
            // 
            lblModeDescription.AutoSize = true;
            lblModeDescription.Location = new System.Drawing.Point(10, 45);
            lblModeDescription.MaximumSize = new System.Drawing.Size(350, 0);
            lblModeDescription.Name = "lblModeDescription";
            lblModeDescription.Size = new System.Drawing.Size(185, 15);
            lblModeDescription.TabIndex = 2;
            lblModeDescription.Text = "Описание режима будет здесь";
            // 
            // btnOK
            // 
            btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnOK.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            btnOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            btnOK.ForeColor = System.Drawing.Color.White;
            btnOK.Location = new System.Drawing.Point(380, 244);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(75, 25);
            btnOK.TabIndex = 1;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = false;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            btnCancel.Location = new System.Drawing.Point(465, 244);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(75, 25);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Отмена";
            btnCancel.Click += btnCancel_Click;
            // 
            // btnApply
            // 
            btnApply.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnApply.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            btnApply.Location = new System.Drawing.Point(295, 244);
            btnApply.Name = "btnApply";
            btnApply.Size = new System.Drawing.Size(75, 25);
            btnApply.TabIndex = 3;
            btnApply.Text = "Применить";
            btnApply.Click += btnApply_Click;
            // 
            // btnReset
            // 
            btnReset.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnReset.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            btnReset.Location = new System.Drawing.Point(10, 244);
            btnReset.Name = "btnReset";
            btnReset.Size = new System.Drawing.Size(75, 25);
            btnReset.TabIndex = 4;
            btnReset.Text = "Сброс";
            btnReset.Click += btnReset_Click;
            // 
            // SettingsDialog
            // 
            ClientSize = new System.Drawing.Size(550, 284);
            Controls.Add(tabControl);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
            Controls.Add(btnApply);
            Controls.Add(btnReset);
            Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsDialog";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Настройки симуляции";
            tabControl.ResumeLayout(false);
            tabSimulation.ResumeLayout(false);
            tabSimulation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numWorldWidth).EndInit();
            ((System.ComponentModel.ISupportInitialize)numWorldHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)numTickDelay).EndInit();
            ((System.ComponentModel.ISupportInitialize)numInitialDensity).EndInit();
            ((System.ComponentModel.ISupportInitialize)numThreads).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSeed).EndInit();
            tabVisualization.ResumeLayout(false);
            tabVisualization.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCellSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)numGridThickness).EndInit();
            tabMode.ResumeLayout(false);
            tabMode.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        #region Поля

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabSimulation;
        private System.Windows.Forms.TabPage tabVisualization;
        private System.Windows.Forms.TabPage tabMode;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Label lblWorldWidth;
        private System.Windows.Forms.NumericUpDown numWorldWidth;
        private System.Windows.Forms.Label lblWorldHeight;
        private System.Windows.Forms.NumericUpDown numWorldHeight;
        private System.Windows.Forms.Label lblTickDelay;
        private System.Windows.Forms.NumericUpDown numTickDelay;
        private System.Windows.Forms.Label lblInitialDensity;
        private System.Windows.Forms.NumericUpDown numInitialDensity;
        private System.Windows.Forms.CheckBox chkWrapAround;
        private System.Windows.Forms.CheckBox chkParallelProcessing;
        private System.Windows.Forms.Label lblThreads;
        private System.Windows.Forms.NumericUpDown numThreads;
        private System.Windows.Forms.CheckBox chkSeed;
        private System.Windows.Forms.NumericUpDown numSeed;
        private System.Windows.Forms.Label lblCellSize;
        private System.Windows.Forms.NumericUpDown numCellSize;
        private System.Windows.Forms.Label lblAliveColor;
        private System.Windows.Forms.ColorDialog colorAliveColor;
        private System.Windows.Forms.Button btnAliveColor;
        private System.Windows.Forms.Label lblDeadColor;
        private System.Windows.Forms.ColorDialog colorDeadColor;
        private System.Windows.Forms.Button btnDeadColor;
        private System.Windows.Forms.Label lblGridColor;
        private System.Windows.Forms.ColorDialog colorGridColor;
        private System.Windows.Forms.Button btnGridColor;
        private System.Windows.Forms.Label lblGridThickness;
        private System.Windows.Forms.NumericUpDown numGridThickness;
        private System.Windows.Forms.CheckBox chkShowGrid;
        private System.Windows.Forms.CheckBox chkShowGenerationInfo;
        private System.Windows.Forms.Label lblSimulationMode;
        private System.Windows.Forms.ComboBox cmbSimulationMode;
        private System.Windows.Forms.Label lblModeDescription;
        private System.Windows.Forms.ToolTip toolTip;

        #endregion
    }
}