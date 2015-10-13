namespace eDoctrinaOcrEd
{
    partial class RecognizeProcessingForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RecognizeProcessingForm));
            this.CancelRecButton = new System.Windows.Forms.Button();
            this.LabelTextBox = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // CancelRecButton
            // 
            this.CancelRecButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.CancelRecButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelRecButton.Image = global::eDoctrinaOcrEd.Properties.Resources.cancel_24;
            this.CancelRecButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.CancelRecButton.Location = new System.Drawing.Point(266, 41);
            this.CancelRecButton.Name = "CancelRecButton";
            this.CancelRecButton.Size = new System.Drawing.Size(75, 36);
            this.CancelRecButton.TabIndex = 0;
            this.CancelRecButton.Text = "Cancel";
            this.CancelRecButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.CancelRecButton.UseVisualStyleBackColor = true;
            this.CancelRecButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // LabelTextBox
            // 
            this.LabelTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LabelTextBox.Location = new System.Drawing.Point(12, 12);
            this.LabelTextBox.Name = "LabelTextBox";
            this.LabelTextBox.Size = new System.Drawing.Size(247, 98);
            this.LabelTextBox.TabIndex = 1;
            this.LabelTextBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // RecognizeProcessingForm
            // 
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.Dialog;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(353, 119);
            this.Controls.Add(this.LabelTextBox);
            this.Controls.Add(this.CancelRecButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RecognizeProcessingForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Recognition in progress...";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RecognizeProcessingForm_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button CancelRecButton;
        public System.Windows.Forms.Label LabelTextBox;
    }
}