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
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.timer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.WorldWidthNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.WorldHeightNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRefresh)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDensity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // GridCheckBox
            // 
            this.GridCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.GridCheckBox.AutoSize = true;
            this.GridCheckBox.BackColor = System.Drawing.Color.Transparent;
            this.GridCheckBox.Location = new System.Drawing.Point(2, 1);
            this.GridCheckBox.Name = "GridCheckBox";
            this.GridCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.GridCheckBox.Size = new System.Drawing.Size(45, 17);
            this.GridCheckBox.TabIndex = 13;
            this.GridCheckBox.Text = "Grid";
            this.GridCheckBox.UseVisualStyleBackColor = false;
            this.GridCheckBox.CheckedChanged += new System.EventHandler(this.GridCheckBox_CheckedChanged);
            // 
            // WorldWidthNumericUpDown
            // 
            this.WorldWidthNumericUpDown.BackColor = System.Drawing.Color.White;
            this.WorldWidthNumericUpDown.ForeColor = System.Drawing.SystemColors.InfoText;
            this.WorldWidthNumericUpDown.Location = new System.Drawing.Point(401, 139);
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
            this.WorldWidthNumericUpDown.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.WorldWidthNumericUpDown.Size = new System.Drawing.Size(80, 20);
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
            this.WorldHeightNumericUpDown.BackColor = System.Drawing.Color.White;
            this.WorldHeightNumericUpDown.ForeColor = System.Drawing.SystemColors.InfoText;
            this.WorldHeightNumericUpDown.Location = new System.Drawing.Point(401, 99);
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
            this.WorldHeightNumericUpDown.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.WorldHeightNumericUpDown.Size = new System.Drawing.Size(80, 20);
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
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label5.Location = new System.Drawing.Point(402, 123);
            this.label5.Margin = new System.Windows.Forms.Padding(0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 15);
            this.label5.TabIndex = 8;
            this.label5.Text = "WorldWidth";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label4.Location = new System.Drawing.Point(396, 83);
            this.label4.Margin = new System.Windows.Forms.Padding(0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(86, 15);
            this.label4.TabIndex = 6;
            this.label4.Text = "WorldHeight";
            // 
            // nudRefresh
            // 
            this.nudRefresh.BackColor = System.Drawing.Color.White;
            this.nudRefresh.ForeColor = System.Drawing.SystemColors.InfoText;
            this.nudRefresh.Location = new System.Drawing.Point(401, 59);
            this.nudRefresh.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudRefresh.Name = "nudRefresh";
            this.nudRefresh.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.nudRefresh.Size = new System.Drawing.Size(80, 20);
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
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label3.Location = new System.Drawing.Point(421, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 16);
            this.label3.TabIndex = 4;
            this.label3.Text = "Refresh";
            // 
            // bStop
            // 
            this.bStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.bStop.Location = new System.Drawing.Point(296, 3);
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
            this.bRnd.Location = new System.Drawing.Point(134, 3);
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
            this.bStart.Location = new System.Drawing.Point(215, 3);
            this.bStart.Name = "bStart";
            this.bStart.Size = new System.Drawing.Size(75, 23);
            this.bStart.TabIndex = 11;
            this.bStart.Text = "Start";
            this.bStart.UseVisualStyleBackColor = true;
            this.bStart.Click += new System.EventHandler(this.bStart_Click);
            // 
            // nudDensity
            // 
            this.nudDensity.BackColor = System.Drawing.Color.White;
            this.nudDensity.ForeColor = System.Drawing.SystemColors.InfoText;
            this.nudDensity.Location = new System.Drawing.Point(401, 19);
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
            this.nudDensity.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.nudDensity.Size = new System.Drawing.Size(80, 20);
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
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(401, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "RndDensity";
            // 
            // pictureBox
            // 
            this.pictureBox.BackColor = System.Drawing.Color.Black;
            this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox.Location = new System.Drawing.Point(0, 0);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(484, 461);
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            this.pictureBox.SizeChanged += new System.EventHandler(this.pictureBox_SizeChanged);
            this.pictureBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseClick);
            this.pictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseMove);
            // 
            // timer
            // 
            this.timer.Interval = 200;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // mainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 461);
            this.Controls.Add(this.GridCheckBox);
            this.Controls.Add(this.WorldWidthNumericUpDown);
            this.Controls.Add(this.WorldHeightNumericUpDown);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.nudDensity);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.bStart);
            this.Controls.Add(this.nudRefresh);
            this.Controls.Add(this.bRnd);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.bStop);
            this.Controls.Add(this.pictureBox);
            this.Name = "mainForm";
            this.Text = "GameOfLife";
            this.Shown += new System.EventHandler(this.mainForm_Shown);
            this.ResizeEnd += new System.EventHandler(this.mainForm_ResizeEnd);
            ((System.ComponentModel.ISupportInitialize)(this.WorldWidthNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.WorldHeightNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRefresh)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDensity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
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
    }
}

