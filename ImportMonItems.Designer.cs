namespace MonItemsMerge
{
    partial class ImportMonItems
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label2 = new System.Windows.Forms.Label();
            this.txt_Target = new System.Windows.Forms.TextBox();
            this.btnOpenTarget = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.txt_Source = new System.Windows.Forms.TextBox();
            this.btnOpenSource = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.XDRadioButton = new System.Windows.Forms.RadioButton();
            this.GeeRadioButton = new System.Windows.Forms.RadioButton();
            this.SJradioButton = new System.Windows.Forms.RadioButton();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 15;
            this.label2.Text = "导出路径";
            // 
            // txt_Target
            // 
            this.txt_Target.Location = new System.Drawing.Point(70, 71);
            this.txt_Target.Name = "txt_Target";
            this.txt_Target.Size = new System.Drawing.Size(362, 21);
            this.txt_Target.TabIndex = 14;
            // 
            // btnOpenTarget
            // 
            this.btnOpenTarget.Location = new System.Drawing.Point(463, 69);
            this.btnOpenTarget.Name = "btnOpenTarget";
            this.btnOpenTarget.Size = new System.Drawing.Size(75, 21);
            this.btnOpenTarget.TabIndex = 13;
            this.btnOpenTarget.Text = "导出目录";
            this.btnOpenTarget.UseVisualStyleBackColor = true;
            this.btnOpenTarget.Click += new System.EventHandler(this.btnOpenTarget_Click);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(457, 111);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(90, 34);
            this.btnStart.TabIndex = 12;
            this.btnStart.Text = "开始执行";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(20, 111);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(412, 21);
            this.progressBar1.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 10;
            this.label1.Text = "配置路径";
            // 
            // txt_Source
            // 
            this.txt_Source.Location = new System.Drawing.Point(69, 28);
            this.txt_Source.Name = "txt_Source";
            this.txt_Source.Size = new System.Drawing.Size(363, 21);
            this.txt_Source.TabIndex = 9;
            // 
            // btnOpenSource
            // 
            this.btnOpenSource.Location = new System.Drawing.Point(462, 27);
            this.btnOpenSource.Name = "btnOpenSource";
            this.btnOpenSource.Size = new System.Drawing.Size(75, 21);
            this.btnOpenSource.TabIndex = 8;
            this.btnOpenSource.Text = "选择配置";
            this.btnOpenSource.UseVisualStyleBackColor = true;
            this.btnOpenSource.Click += new System.EventHandler(this.btnOpenSource_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(22, 140);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(72, 16);
            this.checkBox1.TabIndex = 17;
            this.checkBox1.Text = "导出日志";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // XDRadioButton
            // 
            this.XDRadioButton.AutoSize = true;
            this.XDRadioButton.Location = new System.Drawing.Point(203, 140);
            this.XDRadioButton.Name = "XDRadioButton";
            this.XDRadioButton.Size = new System.Drawing.Size(47, 16);
            this.XDRadioButton.TabIndex = 18;
            this.XDRadioButton.Text = "XDM2";
            this.XDRadioButton.UseVisualStyleBackColor = true;
            // 
            // GeeRadioButton
            // 
            this.GeeRadioButton.AutoSize = true;
            this.GeeRadioButton.Location = new System.Drawing.Point(273, 140);
            this.GeeRadioButton.Name = "GeeRadioButton";
            this.GeeRadioButton.Size = new System.Drawing.Size(65, 16);
            this.GeeRadioButton.TabIndex = 19;
            this.GeeRadioButton.Text = "GEE ASK";
            this.GeeRadioButton.UseVisualStyleBackColor = true;
            // 
            // SJradioButton
            // 
            this.SJradioButton.AutoSize = true;
            this.SJradioButton.Checked = true;
            this.SJradioButton.Location = new System.Drawing.Point(361, 140);
            this.SJradioButton.Name = "SJradioButton";
            this.SJradioButton.Size = new System.Drawing.Size(47, 16);
            this.SJradioButton.TabIndex = 20;
            this.SJradioButton.TabStop = true;
            this.SJradioButton.Text = "水晶";
            this.SJradioButton.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Checked = true;
            this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox2.Location = new System.Drawing.Point(100, 140);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(96, 16);
            this.checkBox2.TabIndex = 21;
            this.checkBox2.Text = "按怪物包分类";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // ImportMonItems
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(561, 162);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.SJradioButton);
            this.Controls.Add(this.GeeRadioButton);
            this.Controls.Add(this.XDRadioButton);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txt_Target);
            this.Controls.Add(this.btnOpenTarget);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txt_Source);
            this.Controls.Add(this.btnOpenSource);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImportMonItems";
            this.Text = "传奇2掉落导出工具";
            this.Load += new System.EventHandler(this.ImportMonItems_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txt_Target;
        private System.Windows.Forms.Button btnOpenTarget;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txt_Source;
        private System.Windows.Forms.Button btnOpenSource;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.RadioButton XDRadioButton;
        private System.Windows.Forms.RadioButton GeeRadioButton;
        private System.Windows.Forms.RadioButton SJradioButton;
        private System.Windows.Forms.CheckBox checkBox2;
    }
}