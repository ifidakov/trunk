namespace eDoctrinaOcrUpdate
{
    partial class UpdateForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateForm));
            this.chbRunEditor = new System.Windows.Forms.CheckBox();
            this.chbRunService = new System.Windows.Forms.CheckBox();
            this.btnExit = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // chbRunEditor
            // 
            this.chbRunEditor.AutoSize = true;
            this.chbRunEditor.Enabled = false;
            this.chbRunEditor.Location = new System.Drawing.Point(12, 33);
            this.chbRunEditor.Name = "chbRunEditor";
            this.chbRunEditor.Size = new System.Drawing.Size(145, 17);
            this.chbRunEditor.TabIndex = 0;
            this.chbRunEditor.Text = "Run eDoctrina Ocr Editor";
            this.chbRunEditor.UseVisualStyleBackColor = true;
            // 
            // chbRunService
            // 
            this.chbRunService.AutoSize = true;
            this.chbRunService.Enabled = false;
            this.chbRunService.Location = new System.Drawing.Point(12, 10);
            this.chbRunService.Name = "chbRunService";
            this.chbRunService.Size = new System.Drawing.Size(154, 17);
            this.chbRunService.TabIndex = 1;
            this.chbRunService.Text = "Run eDoctrina Ocr Service";
            this.chbRunService.UseVisualStyleBackColor = true;
            // 
            // btnExit
            // 
            this.btnExit.AccessibleRole = System.Windows.Forms.AccessibleRole.TitleBar;
            this.btnExit.Enabled = false;
            this.btnExit.Location = new System.Drawing.Point(284, 83);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 23);
            this.btnExit.TabIndex = 2;
            this.btnExit.Text = "Finish";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 88);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(162, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Check for updates, please wait...";
            // 
            // UpdateForm
            // 
            this.AcceptButton = this.btnExit;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(371, 113);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.chbRunService);
            this.Controls.Add(this.chbRunEditor);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdateForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "eDoctrina OCR Update";
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chbRunEditor;
        private System.Windows.Forms.CheckBox chbRunService;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Label label1;
    }
}

