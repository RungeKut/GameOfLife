namespace GameOfLife
{
    partial class mainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.EnvironmentCheckBox = new System.Windows.Forms.CheckBox();
            this.GenomeCheckBox = new System.Windows.Forms.CheckBox();
            this.GridCheckBox = new System.Windows.Forms.CheckBox();
            this.WorldWidthNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.WorldHeightNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.nudRefresh = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.bStop = new System.Windows.Forms.Button();
            this.bRnd = new System.Windows.Forms.Button();
            this.bStart = new System.Windows.Forms.Button();
            this.nudDensity = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.MutationRateNumeric = new System.Windows.Forms.NumericUpDown();
            this.MutationRateLabel = new System.Windows.Forms.Label();
            this.ShowToxicityCheckBox = new System.Windows.Forms.CheckBox();
            this.ParallelCheckBox = new System.Windows.Forms.CheckBox();
            this.WorldBorderCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WorldWidthNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.WorldHeightNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRefresh)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDensity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MutationRateNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.MutationRateNumeric);
            this.splitContainer1.Panel1.Controls.Add(this.EnvironmentCheckBox);
            this.splitContainer1.Panel1.Controls.Add(this.WorldBorderCheckBox);
            this.splitContainer1.Panel1.Controls.Add(this.ParallelCheckBox);
            this.splitContainer1.Panel1.Controls.Add(this.ShowToxicityCheckBox);
            this.splitContainer1.Panel1.Controls.Add(this.GenomeCheckBox);
            this.splitContainer1.Panel1.Controls.Add(this.GridCheckBox);
            this.splitContainer1.Panel1.Controls.Add(this.WorldWidthNumericUpDown);
            this.splitContainer1.Panel1.Controls.Add(this.WorldHeightNumericUpDown);
            this.splitContainer1.Panel1.Controls.Add(this.label5);
            this.splitContainer1.Panel1.Controls.Add(this.label4);
            this.splitContainer1.Panel1.Controls.Add(this.nudRefresh);
            this.splitContainer1.Panel1.Controls.Add(this.MutationRateLabel);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.bStop);
            this.splitContainer1.Panel1.Controls.Add(this.bRnd);
            this.splitContainer1.Panel1.Controls.Add(this.bStart);
            this.splitContainer1.Panel1.Controls.Add(this.nudDensity);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.menuStrip1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pictureBox);
            this.splitContainer1.Size = new System.Drawing.Size(949, 487);
            this.splitContainer1.SplitterDistance = 88;
            this.splitContainer1.TabIndex = 0;
            // 
            // EnvironmentCheckBox
            // 
            this.EnvironmentCheckBox.AutoSize = true;
            this.EnvironmentCheckBox.Checked = true;
            this.EnvironmentCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.EnvironmentCheckBox.Location = new System.Drawing.Point(4, 324);
            this.EnvironmentCheckBox.Name = "EnvironmentCheckBox";
            this.EnvironmentCheckBox.Size = new System.Drawing.Size(57, 17);
            this.EnvironmentCheckBox.TabIndex = 15;
            this.EnvironmentCheckBox.Text = "Среда";
            this.EnvironmentCheckBox.UseVisualStyleBackColor = true;
            this.EnvironmentCheckBox.CheckedChanged += new System.EventHandler(this.EnvironmentCheckBox_CheckedChanged);
            // 
            // GenomeCheckBox
            // 
            this.GenomeCheckBox.AutoSize = true;
            this.GenomeCheckBox.Checked = true;
            this.GenomeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.GenomeCheckBox.Location = new System.Drawing.Point(4, 310);
            this.GenomeCheckBox.Name = "GenomeCheckBox";
            this.GenomeCheckBox.Size = new System.Drawing.Size(58, 17);
            this.GenomeCheckBox.TabIndex = 15;
            this.GenomeCheckBox.Text = "Геном";
            this.GenomeCheckBox.UseVisualStyleBackColor = true;
            this.GenomeCheckBox.CheckedChanged += new System.EventHandler(this.GenomeCheckBox_CheckedChanged);
            // 
            // GridCheckBox
            // 
            this.GridCheckBox.AutoSize = true;
            this.GridCheckBox.Location = new System.Drawing.Point(4, 296);
            this.GridCheckBox.Name = "GridCheckBox";
            this.GridCheckBox.Size = new System.Drawing.Size(45, 17);
            this.GridCheckBox.TabIndex = 13;
            this.GridCheckBox.Text = "Grid";
            this.GridCheckBox.UseVisualStyleBackColor = true;
            this.GridCheckBox.CheckedChanged += new System.EventHandler(this.GridCheckBox_CheckedChanged);
            // 
            // WorldWidthNumericUpDown
            // 
            this.WorldWidthNumericUpDown.Location = new System.Drawing.Point(4, 173);
            this.WorldWidthNumericUpDown.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.WorldWidthNumericUpDown.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.WorldWidthNumericUpDown.Name = "WorldWidthNumericUpDown";
            this.WorldWidthNumericUpDown.Size = new System.Drawing.Size(72, 20);
            this.WorldWidthNumericUpDown.TabIndex = 9;
            this.WorldWidthNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.WorldWidthNumericUpDown.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.WorldWidthNumericUpDown.ValueChanged += new System.EventHandler(this.WorldWidthNumericUpDown_ValueChanged);
            // 
            // WorldHeightNumericUpDown
            // 
            this.WorldHeightNumericUpDown.Location = new System.Drawing.Point(4, 129);
            this.WorldHeightNumericUpDown.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.WorldHeightNumericUpDown.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.WorldHeightNumericUpDown.Name = "WorldHeightNumericUpDown";
            this.WorldHeightNumericUpDown.Size = new System.Drawing.Size(72, 20);
            this.WorldHeightNumericUpDown.TabIndex = 7;
            this.WorldHeightNumericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.WorldHeightNumericUpDown.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.WorldHeightNumericUpDown.ValueChanged += new System.EventHandler(this.WorldHeightNumericUpDown_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label5.Location = new System.Drawing.Point(-2, 155);
            this.label5.Margin = new System.Windows.Forms.Padding(0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 15);
            this.label5.TabIndex = 8;
            this.label5.Text = "WorldWidth";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label4.Location = new System.Drawing.Point(-2, 110);
            this.label4.Margin = new System.Windows.Forms.Padding(0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(86, 15);
            this.label4.TabIndex = 6;
            this.label4.Text = "WorldHeight";
            // 
            // nudRefresh
            // 
            this.nudRefresh.Location = new System.Drawing.Point(4, 77);
            this.nudRefresh.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudRefresh.Name = "nudRefresh";
            this.nudRefresh.Size = new System.Drawing.Size(72, 20);
            this.nudRefresh.TabIndex = 5;
            this.nudRefresh.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudRefresh.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nudRefresh.ValueChanged += new System.EventHandler(this.nudRefresh_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.Location = new System.Drawing.Point(1, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 16);
            this.label3.TabIndex = 4;
            this.label3.Text = "Refresh";
            // 
            // bStop
            // 
            this.bStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.bStop.Location = new System.Drawing.Point(4, 267);
            this.bStop.Name = "bStop";
            this.bStop.Size = new System.Drawing.Size(75, 23);
            this.bStop.TabIndex = 12;
            this.bStop.Text = "Reset";
            this.bStop.UseVisualStyleBackColor = true;
            this.bStop.Click += new System.EventHandler(this.bStop_Click);
            // 
            // bRnd
            // 
            this.bRnd.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.bRnd.Location = new System.Drawing.Point(4, 209);
            this.bRnd.Name = "bRnd";
            this.bRnd.Size = new System.Drawing.Size(75, 23);
            this.bRnd.TabIndex = 10;
            this.bRnd.Text = "Rnd";
            this.bRnd.UseVisualStyleBackColor = true;
            this.bRnd.Click += new System.EventHandler(this.bRnd_Click);
            // 
            // bStart
            // 
            this.bStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.bStart.Location = new System.Drawing.Point(4, 238);
            this.bStart.Name = "bStart";
            this.bStart.Size = new System.Drawing.Size(75, 23);
            this.bStart.TabIndex = 11;
            this.bStart.Text = "Start";
            this.bStart.UseVisualStyleBackColor = true;
            this.bStart.Click += new System.EventHandler(this.bStart_Click);
            // 
            // nudDensity
            // 
            this.nudDensity.Location = new System.Drawing.Point(4, 26);
            this.nudDensity.Maximum = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.nudDensity.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nudDensity.Name = "nudDensity";
            this.nudDensity.Size = new System.Drawing.Size(72, 20);
            this.nudDensity.TabIndex = 3;
            this.nudDensity.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudDensity.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.nudDensity.ValueChanged += new System.EventHandler(this.nudDensity_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(1, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "RndDensity";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(84, 24);
            this.menuStrip1.TabIndex = 14;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // pictureBox
            // 
            this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox.Location = new System.Drawing.Point(0, 0);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(853, 483);
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            this.pictureBox.SizeChanged += new System.EventHandler(this.pictureBox_SizeChanged);
            // 
            // timer
            // 
            this.timer.Interval = 200;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // MutationRateNumeric
            // 
            this.MutationRateNumeric.Location = new System.Drawing.Point(4, 363);
            this.MutationRateNumeric.Name = "MutationRateNumeric";
            this.MutationRateNumeric.Size = new System.Drawing.Size(73, 20);
            this.MutationRateNumeric.TabIndex = 16;
            this.MutationRateNumeric.ValueChanged += new System.EventHandler(this.MutationRateNumeric_ValueChanged);
            // 
            // MutationRateLabel
            // 
            this.MutationRateLabel.AutoSize = true;
            this.MutationRateLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MutationRateLabel.Location = new System.Drawing.Point(3, 344);
            this.MutationRateLabel.Name = "MutationRateLabel";
            this.MutationRateLabel.Size = new System.Drawing.Size(143, 16);
            this.MutationRateLabel.TabIndex = 4;
            this.MutationRateLabel.Text = "Шанс мутации (%):";
            // 
            // ShowToxicityCheckBox
            // 
            this.ShowToxicityCheckBox.AutoSize = true;
            this.ShowToxicityCheckBox.Location = new System.Drawing.Point(4, 389);
            this.ShowToxicityCheckBox.Name = "ShowToxicityCheckBox";
            this.ShowToxicityCheckBox.Size = new System.Drawing.Size(141, 17);
            this.ShowToxicityCheckBox.TabIndex = 15;
            this.ShowToxicityCheckBox.Text = "Показать токсичность";
            this.ShowToxicityCheckBox.UseVisualStyleBackColor = true;
            this.ShowToxicityCheckBox.CheckedChanged += new System.EventHandler(this.ShowToxicityCheckBox_CheckedChanged);
            // 
            // ParallelCheckBox
            // 
            this.ParallelCheckBox.AutoSize = true;
            this.ParallelCheckBox.Checked = true;
            this.ParallelCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ParallelCheckBox.Location = new System.Drawing.Point(4, 403);
            this.ParallelCheckBox.Name = "ParallelCheckBox";
            this.ParallelCheckBox.Size = new System.Drawing.Size(166, 17);
            this.ParallelCheckBox.TabIndex = 15;
            this.ParallelCheckBox.Text = "Параллельные вычисления";
            this.ParallelCheckBox.UseVisualStyleBackColor = true;
            this.ParallelCheckBox.CheckedChanged += new System.EventHandler(this.ParallelCheckBox_CheckedChanged);
            // 
            // WorldBorderCheckBox
            // 
            this.WorldBorderCheckBox.AutoSize = true;
            this.WorldBorderCheckBox.Checked = true;
            this.WorldBorderCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.WorldBorderCheckBox.Location = new System.Drawing.Point(4, 417);
            this.WorldBorderCheckBox.Name = "WorldBorderCheckBox";
            this.WorldBorderCheckBox.Size = new System.Drawing.Size(99, 17);
            this.WorldBorderCheckBox.TabIndex = 15;
            this.WorldBorderCheckBox.Text = "Границы мира";
            this.WorldBorderCheckBox.UseVisualStyleBackColor = true;
            this.WorldBorderCheckBox.CheckedChanged += new System.EventHandler(this.WorldBorderCheckBox_CheckedChanged);
            // 
            // mainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(949, 487);
            this.Controls.Add(this.splitContainer1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "mainForm";
            this.Text = "GameOfLife";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.mainForm_FormClosing);
            this.Shown += new System.EventHandler(this.mainForm_Shown);
            this.ResizeEnd += new System.EventHandler(this.mainForm_ResizeEnd);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.WorldWidthNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.WorldHeightNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRefresh)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDensity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MutationRateNumeric)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button bStop;
        private System.Windows.Forms.Button bStart;
        private System.Windows.Forms.NumericUpDown nudDensity;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.NumericUpDown nudRefresh;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Button bRnd;
        private System.Windows.Forms.NumericUpDown WorldWidthNumericUpDown;
        private System.Windows.Forms.NumericUpDown WorldHeightNumericUpDown;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox GridCheckBox;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.CheckBox EnvironmentCheckBox;
        private System.Windows.Forms.CheckBox GenomeCheckBox;
        private System.Windows.Forms.NumericUpDown MutationRateNumeric;
        private System.Windows.Forms.Label MutationRateLabel;
        private System.Windows.Forms.CheckBox ShowToxicityCheckBox;
        private System.Windows.Forms.CheckBox ParallelCheckBox;
        private System.Windows.Forms.CheckBox WorldBorderCheckBox;
    }
}

