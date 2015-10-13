namespace eDoctrinaOcrEd
{
    partial class SettingsForm1
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chbIndexOfFirstQuestion = new System.Windows.Forms.CheckBox();
            this.chbAmoutOfQuestions = new System.Windows.Forms.CheckBox();
            this.chbTestId = new System.Windows.Forms.CheckBox();
            this.chbDistrictId = new System.Windows.Forms.CheckBox();
            this.chbSheetId = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chBxRecAfterCut = new System.Windows.Forms.CheckBox();
            this.chBxNotConfirm = new System.Windows.Forms.CheckBox();
            this.chBxUsePrevTool = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chbIndexOfFirstQuestion);
            this.groupBox1.Controls.Add(this.chbAmoutOfQuestions);
            this.groupBox1.Controls.Add(this.chbTestId);
            this.groupBox1.Controls.Add(this.chbDistrictId);
            this.groupBox1.Controls.Add(this.chbSheetId);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(281, 152);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Set previous value if it is not recognized";
            // 
            // chbIndexOfFirstQuestion
            // 
            this.chbIndexOfFirstQuestion.AutoSize = true;
            this.chbIndexOfFirstQuestion.Location = new System.Drawing.Point(6, 126);
            this.chbIndexOfFirstQuestion.Name = "chbIndexOfFirstQuestion";
            this.chbIndexOfFirstQuestion.Size = new System.Drawing.Size(162, 20);
            this.chbIndexOfFirstQuestion.TabIndex = 5;
            this.chbIndexOfFirstQuestion.Text = "index_of_first_question";
            this.chbIndexOfFirstQuestion.UseVisualStyleBackColor = true;
            // 
            // chbAmoutOfQuestions
            // 
            this.chbAmoutOfQuestions.AutoSize = true;
            this.chbAmoutOfQuestions.Location = new System.Drawing.Point(6, 100);
            this.chbAmoutOfQuestions.Name = "chbAmoutOfQuestions";
            this.chbAmoutOfQuestions.Size = new System.Drawing.Size(147, 20);
            this.chbAmoutOfQuestions.TabIndex = 4;
            this.chbAmoutOfQuestions.Text = "amout_of_questions";
            this.chbAmoutOfQuestions.UseVisualStyleBackColor = true;
            // 
            // chbTestId
            // 
            this.chbTestId.AutoSize = true;
            this.chbTestId.Location = new System.Drawing.Point(6, 74);
            this.chbTestId.Name = "chbTestId";
            this.chbTestId.Size = new System.Drawing.Size(66, 20);
            this.chbTestId.TabIndex = 3;
            this.chbTestId.Text = "test_id";
            this.chbTestId.UseVisualStyleBackColor = true;
            // 
            // chbDistrictId
            // 
            this.chbDistrictId.AutoSize = true;
            this.chbDistrictId.Location = new System.Drawing.Point(6, 48);
            this.chbDistrictId.Name = "chbDistrictId";
            this.chbDistrictId.Size = new System.Drawing.Size(83, 20);
            this.chbDistrictId.TabIndex = 2;
            this.chbDistrictId.Text = "district_id";
            this.chbDistrictId.UseVisualStyleBackColor = true;
            // 
            // chbSheetId
            // 
            this.chbSheetId.AutoSize = true;
            this.chbSheetId.Location = new System.Drawing.Point(6, 21);
            this.chbSheetId.Name = "chbSheetId";
            this.chbSheetId.Size = new System.Drawing.Size(115, 20);
            this.chbSheetId.TabIndex = 1;
            this.chbSheetId.Text = "Sheet identifier";
            this.chbSheetId.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(125, 299);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(218, 299);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.chBxNotConfirm);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.groupBox2.Location = new System.Drawing.Point(12, 170);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(281, 59);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Cutting settings";
            // 
            // chBxRecAfterCut
            // 
            this.chBxRecAfterCut.AutoSize = true;
            this.chBxRecAfterCut.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.chBxRecAfterCut.Location = new System.Drawing.Point(18, 261);
            this.chBxRecAfterCut.Name = "chBxRecAfterCut";
            this.chBxRecAfterCut.Size = new System.Drawing.Size(191, 20);
            this.chBxRecAfterCut.TabIndex = 7;
            this.chBxRecAfterCut.Text = "Recognize after processing";
            this.chBxRecAfterCut.UseVisualStyleBackColor = true;
            // 
            // chBxNotConfirm
            // 
            this.chBxNotConfirm.AutoSize = true;
            this.chBxNotConfirm.Location = new System.Drawing.Point(6, 21);
            this.chBxNotConfirm.Name = "chBxNotConfirm";
            this.chBxNotConfirm.Size = new System.Drawing.Size(271, 20);
            this.chBxNotConfirm.TabIndex = 6;
            this.chBxNotConfirm.Text = "Do not show confirmation dialog of cutting";
            this.chBxNotConfirm.UseVisualStyleBackColor = true;
            // 
            // chBxUsePrevTool
            // 
            this.chBxUsePrevTool.AutoSize = true;
            this.chBxUsePrevTool.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.chBxUsePrevTool.Location = new System.Drawing.Point(18, 235);
            this.chBxUsePrevTool.Name = "chBxUsePrevTool";
            this.chBxUsePrevTool.Size = new System.Drawing.Size(190, 20);
            this.chBxUsePrevTool.TabIndex = 8;
            this.chBxUsePrevTool.Text = "Use the previous editor tool";
            this.chBxUsePrevTool.UseVisualStyleBackColor = true;
            // 
            // SettingsForm1
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(305, 334);
            this.Controls.Add(this.chBxRecAfterCut);
            this.Controls.Add(this.chBxUsePrevTool);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SettingsForm1";
            this.ShowInTaskbar = false;
            this.Text = "eDoctrina OCR Editor Settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.CheckBox chbAmoutOfQuestions;
        public System.Windows.Forms.CheckBox chbTestId;
        public System.Windows.Forms.CheckBox chbDistrictId;
        public System.Windows.Forms.CheckBox chbSheetId;
        public System.Windows.Forms.Button btnSave;
        public System.Windows.Forms.Button btnCancel;
        public System.Windows.Forms.CheckBox chbIndexOfFirstQuestion;
        private System.Windows.Forms.GroupBox groupBox2;
        public System.Windows.Forms.CheckBox chBxRecAfterCut;
        public System.Windows.Forms.CheckBox chBxNotConfirm;
        public System.Windows.Forms.CheckBox chBxUsePrevTool;
    }
}