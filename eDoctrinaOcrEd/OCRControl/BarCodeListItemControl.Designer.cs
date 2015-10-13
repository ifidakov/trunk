namespace eDoctrinaOcrEd
{
    partial class BarCodeListItemControl
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BarCodeListItemControl));
            this.imageList3 = new System.Windows.Forms.ImageList(this.components);
            this.ItemPanel = new System.Windows.Forms.Panel();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.rbtnText = new System.Windows.Forms.RadioButton();
            this.btnCheck = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.BorderPanel = new System.Windows.Forms.Panel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.ItemPanel.SuspendLayout();
            this.BorderPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageList3
            // 
            this.imageList3.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList3.ImageStream")));
            this.imageList3.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList3.Images.SetKeyName(0, "check_16.png");
            this.imageList3.Images.SetKeyName(1, "1416523633_62744.ico");
            this.imageList3.Images.SetKeyName(2, "1416523659_barcode_2.png");
            this.imageList3.Images.SetKeyName(3, "1416524644_174865.ico");
            this.imageList3.Images.SetKeyName(4, "1416525781_barcode.png");
            this.imageList3.Images.SetKeyName(5, "Oxygen-Icons.org-Oxygen-Actions-draw-text.ico");
            // 
            // ItemPanel
            // 
            this.ItemPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ItemPanel.BackColor = System.Drawing.SystemColors.Control;
            this.ItemPanel.Controls.Add(this.radioButton1);
            this.ItemPanel.Controls.Add(this.rbtnText);
            this.ItemPanel.Controls.Add(this.btnCheck);
            this.ItemPanel.Controls.Add(this.comboBox1);
            this.ItemPanel.Controls.Add(this.label1);
            this.ItemPanel.Location = new System.Drawing.Point(4, 4);
            this.ItemPanel.Name = "ItemPanel";
            this.ItemPanel.Size = new System.Drawing.Size(238, 56);
            this.ItemPanel.TabIndex = 0;
            this.ItemPanel.Click += new System.EventHandler(this.BarCodeListItemControl_Select);
            this.ItemPanel.MouseLeave += new System.EventHandler(this.BarCodeListItemControl_Leave);
            // 
            // radioButton1
            // 
            this.radioButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radioButton1.Appearance = System.Windows.Forms.Appearance.Button;
            this.radioButton1.ImageKey = "1416525781_barcode.png";
            this.radioButton1.ImageList = this.imageList3;
            this.radioButton1.Location = new System.Drawing.Point(182, 19);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(24, 26);
            this.radioButton1.TabIndex = 0;
            this.toolTip1.SetToolTip(this.radioButton1, "Bar code recognition");
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.Click += new System.EventHandler(this.radioButton1_Click);
            // 
            // rbtnText
            // 
            this.rbtnText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.rbtnText.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbtnText.ImageKey = "Oxygen-Icons.org-Oxygen-Actions-draw-text.ico";
            this.rbtnText.ImageList = this.imageList3;
            this.rbtnText.Location = new System.Drawing.Point(154, 19);
            this.rbtnText.Name = "rbtnText";
            this.rbtnText.Size = new System.Drawing.Size(24, 26);
            this.rbtnText.TabIndex = 0;
            this.toolTip1.SetToolTip(this.rbtnText, "Text recognition");
            this.rbtnText.UseVisualStyleBackColor = true;
            this.rbtnText.Click += new System.EventHandler(this.radioButton1_Click);
            // 
            // btnCheck
            // 
            this.btnCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCheck.ImageKey = "check_16.png";
            this.btnCheck.ImageList = this.imageList3;
            this.btnCheck.Location = new System.Drawing.Point(209, 19);
            this.btnCheck.Name = "btnCheck";
            this.btnCheck.Size = new System.Drawing.Size(24, 26);
            this.btnCheck.TabIndex = 2;
            this.btnCheck.TabStop = false;
            this.toolTip1.SetToolTip(this.btnCheck, "Verify value");
            this.btnCheck.UseVisualStyleBackColor = true;
            this.btnCheck.Click += new System.EventHandler(this.Ok_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple;
            this.comboBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(7, 20);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(144, 24);
            this.comboBox1.TabIndex = 1;
            this.comboBox1.SelectedValueChanged += new System.EventHandler(this.comboBox1_SelectedValueChanged);
            this.comboBox1.Click += new System.EventHandler(this.BarCodeListItemControl_Click);
            this.comboBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.comboBox1_KeyPress);
            this.comboBox1.Leave += new System.EventHandler(this.BarCodeListItemControl_Leave);
            this.comboBox1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.comboBox1_MouseClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name";
            this.label1.Click += new System.EventHandler(this.BarCodeListItemControl_Click);
            // 
            // BorderPanel
            // 
            this.BorderPanel.BackColor = System.Drawing.SystemColors.Control;
            this.BorderPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.BorderPanel.Controls.Add(this.ItemPanel);
            this.BorderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BorderPanel.Location = new System.Drawing.Point(0, 0);
            this.BorderPanel.Margin = new System.Windows.Forms.Padding(0);
            this.BorderPanel.Name = "BorderPanel";
            this.BorderPanel.Padding = new System.Windows.Forms.Padding(4, 4, 8, 8);
            this.BorderPanel.Size = new System.Drawing.Size(250, 68);
            this.BorderPanel.TabIndex = 0;
            // 
            // BarCodeListItemControl
            // 
            this.Controls.Add(this.BorderPanel);
            this.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Size = new System.Drawing.Size(250, 68);
            this.TabStop = false;
            this.Click += new System.EventHandler(this.BarCodeListItemControl_Click);
            this.Enter += new System.EventHandler(this.BarCodeListItemControl_Enter);
            this.MouseLeave += new System.EventHandler(this.BarCodeListItemControl_Leave);
            this.ItemPanel.ResumeLayout(false);
            this.ItemPanel.PerformLayout();
            this.BorderPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList imageList3;
        private System.Windows.Forms.Panel ItemPanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel BorderPanel;
        public System.Windows.Forms.RadioButton radioButton1;
        public System.Windows.Forms.ComboBox comboBox1;
        public System.Windows.Forms.RadioButton rbtnText;
        private System.Windows.Forms.ToolTip toolTip1;
        public System.Windows.Forms.Button btnCheck;
    }
}
