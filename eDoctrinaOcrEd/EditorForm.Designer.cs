namespace eDoctrinaOcrEd
{
    partial class EditorForm
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
            this.components = new System.ComponentModel.Container();
            BoxSheet = new BarCodeListItemControl();
            miniatureList = new MiniatureListControl();
            barCodeList = new BarCodeListControl();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditorForm));
            this.MainMenu = new System.Windows.Forms.MenuStrip();
            this.fileMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openManualInputFolderMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openProcessingFolderMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openErrorsResultsFolderMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openSuccessResultsFolderMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openAutoInputFolderMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.sendProblemToSupportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ReportTestPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sendToSuportMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.SendAllToNextProcessingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.returnFilesAndQuitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitMainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.filesInQueueTextStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.filesInQueueStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tssbtnSetAside = new System.Windows.Forms.ToolStripSplitButton();
            this.tssmiReturnDeferredItems = new System.Windows.Forms.ToolStripMenuItem();
            this.tsslSetAside = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusTextLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.panel5 = new System.Windows.Forms.Panel();
            this.ActionMenu = new System.Windows.Forms.FlowLayoutPanel();
            this.VerifyButton = new System.Windows.Forms.Button();
            this.imageList24 = new System.Windows.Forms.ImageList(this.components);
            this.NextButton = new System.Windows.Forms.Button();
            this.DeleteButton = new System.Windows.Forms.Button();
            this.miniatureList = new eDoctrinaOcrEd.MiniatureListControl();
            this.btnSetAside = new System.Windows.Forms.Button();
            this.btnDeferred = new System.Windows.Forms.Button();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.ImagePanel = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pnlScrollBubble = new System.Windows.Forms.Panel();
            this.AnswerSheetMenu = new System.Windows.Forms.FlowLayoutPanel();
            this.StopRecognizeButton = new System.Windows.Forms.Button();
            this.btnOpenFile = new System.Windows.Forms.Button();
            this.RecognizeAllButton = new System.Windows.Forms.Button();
            this.RecognizeBubblesButton = new System.Windows.Forms.Button();
            this.SizeFitButton = new System.Windows.Forms.Button();
            this.SizeFullButton = new System.Windows.Forms.Button();
            this.SizePlusButton = new System.Windows.Forms.Button();
            this.SizeMinusButton = new System.Windows.Forms.Button();
            this.rbtnBubbles = new System.Windows.Forms.RadioButton();
            this.rbtnGrid = new System.Windows.Forms.RadioButton();
            this.RotateLeftButton = new System.Windows.Forms.Button();
            this.RotateRightButton = new System.Windows.Forms.Button();
            this.rbtnRotate = new System.Windows.Forms.RadioButton();
            this.btnRemoveNoise = new System.Windows.Forms.Button();
            this.rbtnCut = new System.Windows.Forms.RadioButton();
            this.splitBtnRestore = new eDoctrinaOcrEd.SplitButton();
            this.OpenFilesDirButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnInvert = new System.Windows.Forms.Button();
            this.DarknessGroupBox = new System.Windows.Forms.GroupBox();
            this.nudPerCentEmptyBubble = new System.Windows.Forms.NumericUpDown();
            this.DarknessManualySet = new System.Windows.Forms.CheckBox();
            this.nudPerCentBestBubble = new System.Windows.Forms.NumericUpDown();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.pnlBubbles = new System.Windows.Forms.Panel();
            this.nudZoom = new System.Windows.Forms.NumericUpDown();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.BubbleAreaMenu = new System.Windows.Forms.FlowLayoutPanel();
            this.ClearAllCheckMarksButton = new System.Windows.Forms.Button();
            this.btnGrid = new System.Windows.Forms.Button();
            this.imageList16 = new System.Windows.Forms.ImageList(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnCloseLblErr = new System.Windows.Forms.Button();
            this.lblErr = new System.Windows.Forms.Label();
            this.cbDoNotProcess = new System.Windows.Forms.CheckBox();
            this.toolTip2 = new System.Windows.Forms.ToolTip(this.components);
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.rbtnClear = new System.Windows.Forms.RadioButton();
            this.MainMenu.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.ActionMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.ImagePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.AnswerSheetMenu.SuspendLayout();
            this.DarknessGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPerCentEmptyBubble)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPerCentBestBubble)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudZoom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.BubbleAreaMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainMenu
            // 
            this.MainMenu.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMainMenuItem});
            this.MainMenu.Location = new System.Drawing.Point(0, 0);
            this.MainMenu.Name = "MainMenu";
            this.MainMenu.Size = new System.Drawing.Size(1364, 32);
            this.MainMenu.TabIndex = 0;
            this.MainMenu.Text = "menuStrip1";
            // 
            // fileMainMenuItem
            // 
            this.fileMainMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openFileMainMenuItem,
            this.openManualInputFolderMainMenuItem,
            this.openProcessingFolderMainMenuItem,
            this.openErrorsResultsFolderMainMenuItem,
            this.openSuccessResultsFolderMainMenuItem,
            this.openAutoInputFolderMainMenuItem,
            this.toolStripSeparator1,
            this.settingsToolStripMenuItem,
            this.toolStripSeparator4,
            this.sendProblemToSupportToolStripMenuItem,
            this.ReportTestPageToolStripMenuItem,
            this.sendToSuportMainMenuItem,
            this.toolStripSeparator3,
            this.SendAllToNextProcessingToolStripMenuItem,
            this.toolStripSeparator2,
            this.returnFilesAndQuitToolStripMenuItem,
            this.exitMainMenuItem,
            this.restartToolStripMenuItem});
            this.fileMainMenuItem.Name = "fileMainMenuItem";
            this.fileMainMenuItem.Size = new System.Drawing.Size(53, 28);
            this.fileMainMenuItem.Text = "File";
            // 
            // openFileMainMenuItem
            // 
            this.openFileMainMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openFileMainMenuItem.Image")));
            this.openFileMainMenuItem.Name = "openFileMainMenuItem";
            this.openFileMainMenuItem.Size = new System.Drawing.Size(385, 28);
            this.openFileMainMenuItem.Text = "Open custom file";
            this.openFileMainMenuItem.Click += new System.EventHandler(this.OpenFileMainMenuItem_Click);
            // 
            // openManualInputFolderMainMenuItem
            // 
            this.openManualInputFolderMainMenuItem.Image = global::eDoctrinaOcrEd.Properties.Resources.open_16;
            this.openManualInputFolderMainMenuItem.Name = "openManualInputFolderMainMenuItem";
            this.openManualInputFolderMainMenuItem.Size = new System.Drawing.Size(385, 28);
            this.openManualInputFolderMainMenuItem.Text = "Open input folder";
            this.openManualInputFolderMainMenuItem.Click += new System.EventHandler(this.OpenManualInputFolderMainMenuItem_Click);
            // 
            // openProcessingFolderMainMenuItem
            // 
            this.openProcessingFolderMainMenuItem.Image = global::eDoctrinaOcrEd.Properties.Resources.open_16;
            this.openProcessingFolderMainMenuItem.Name = "openProcessingFolderMainMenuItem";
            this.openProcessingFolderMainMenuItem.Size = new System.Drawing.Size(385, 28);
            this.openProcessingFolderMainMenuItem.Text = "Open processing folder";
            this.openProcessingFolderMainMenuItem.Click += new System.EventHandler(this.OpenProcessingFolderMainMenuItem_Click);
            // 
            // openErrorsResultsFolderMainMenuItem
            // 
            this.openErrorsResultsFolderMainMenuItem.Image = global::eDoctrinaOcrEd.Properties.Resources.open_16;
            this.openErrorsResultsFolderMainMenuItem.Name = "openErrorsResultsFolderMainMenuItem";
            this.openErrorsResultsFolderMainMenuItem.Size = new System.Drawing.Size(385, 28);
            this.openErrorsResultsFolderMainMenuItem.Text = "Open output errors folder";
            this.openErrorsResultsFolderMainMenuItem.Click += new System.EventHandler(this.OpenErrorsResultsFolderMainMenuItem_Click);
            // 
            // openSuccessResultsFolderMainMenuItem
            // 
            this.openSuccessResultsFolderMainMenuItem.Image = global::eDoctrinaOcrEd.Properties.Resources.open_16;
            this.openSuccessResultsFolderMainMenuItem.Name = "openSuccessResultsFolderMainMenuItem";
            this.openSuccessResultsFolderMainMenuItem.Size = new System.Drawing.Size(385, 28);
            this.openSuccessResultsFolderMainMenuItem.Text = "Open output success folder";
            this.openSuccessResultsFolderMainMenuItem.Click += new System.EventHandler(this.OpenSuccessResultsFolderMainMenuItem_Click);
            // 
            // openAutoInputFolderMainMenuItem
            // 
            this.openAutoInputFolderMainMenuItem.Image = global::eDoctrinaOcrEd.Properties.Resources.open_16;
            this.openAutoInputFolderMainMenuItem.Name = "openAutoInputFolderMainMenuItem";
            this.openAutoInputFolderMainMenuItem.Size = new System.Drawing.Size(385, 28);
            this.openAutoInputFolderMainMenuItem.Text = "Open automated version input folder";
            this.openAutoInputFolderMainMenuItem.Click += new System.EventHandler(this.OpenAutoInputFolderMainMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(382, 6);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(385, 28);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(382, 6);
            // 
            // sendProblemToSupportToolStripMenuItem
            // 
            this.sendProblemToSupportToolStripMenuItem.Name = "sendProblemToSupportToolStripMenuItem";
            this.sendProblemToSupportToolStripMenuItem.Size = new System.Drawing.Size(385, 28);
            this.sendProblemToSupportToolStripMenuItem.Text = "Send problem to support ";
            this.sendProblemToSupportToolStripMenuItem.Click += new System.EventHandler(this.sendProblemToSupportToolStripMenuItem_Click);
            // 
            // ReportTestPageToolStripMenuItem
            // 
            this.ReportTestPageToolStripMenuItem.Name = "ReportTestPageToolStripMenuItem";
            this.ReportTestPageToolStripMenuItem.Size = new System.Drawing.Size(385, 28);
            this.ReportTestPageToolStripMenuItem.Text = "Send test page to support";
            this.ReportTestPageToolStripMenuItem.Click += new System.EventHandler(this.ReportTestPageToolStripMenuItem_Click);
            // 
            // sendToSuportMainMenuItem
            // 
            this.sendToSuportMainMenuItem.Name = "sendToSuportMainMenuItem";
            this.sendToSuportMainMenuItem.Size = new System.Drawing.Size(385, 28);
            this.sendToSuportMainMenuItem.Text = "Send issue to development";
            this.sendToSuportMainMenuItem.Click += new System.EventHandler(this.SendToSupportMainMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(382, 6);
            // 
            // SendAllToNextProcessingToolStripMenuItem
            // 
            this.SendAllToNextProcessingToolStripMenuItem.Name = "SendAllToNextProcessingToolStripMenuItem";
            this.SendAllToNextProcessingToolStripMenuItem.Size = new System.Drawing.Size(385, 28);
            this.SendAllToNextProcessingToolStripMenuItem.Text = "Send all to \"Next processing\"";
            this.SendAllToNextProcessingToolStripMenuItem.Click += new System.EventHandler(this.SendAllToNextProcessingToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(382, 6);
            // 
            // returnFilesAndQuitToolStripMenuItem
            // 
            this.returnFilesAndQuitToolStripMenuItem.Name = "returnFilesAndQuitToolStripMenuItem";
            this.returnFilesAndQuitToolStripMenuItem.Size = new System.Drawing.Size(385, 28);
            this.returnFilesAndQuitToolStripMenuItem.Text = "Return files and quit";
            this.returnFilesAndQuitToolStripMenuItem.Click += new System.EventHandler(this.returnFilesAndQuitToolStripMenuItem_Click);
            // 
            // exitMainMenuItem
            // 
            this.exitMainMenuItem.Name = "exitMainMenuItem";
            this.exitMainMenuItem.Size = new System.Drawing.Size(385, 28);
            this.exitMainMenuItem.Text = "Exit";
            this.exitMainMenuItem.Click += new System.EventHandler(this.ExitMainMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.filesInQueueTextStatusLabel,
            this.filesInQueueStatusLabel,
            this.tssbtnSetAside,
            this.tsslSetAside,
            this.StatusTextLabel,
            this.StatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 464);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1364, 30);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // filesInQueueTextStatusLabel
            // 
            this.filesInQueueTextStatusLabel.Name = "filesInQueueTextStatusLabel";
            this.filesInQueueTextStatusLabel.Size = new System.Drawing.Size(135, 25);
            this.filesInQueueTextStatusLabel.Text = "Files in queue:";
            // 
            // filesInQueueStatusLabel
            // 
            this.filesInQueueStatusLabel.Name = "filesInQueueStatusLabel";
            this.filesInQueueStatusLabel.Padding = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.filesInQueueStatusLabel.Size = new System.Drawing.Size(30, 25);
            this.filesInQueueStatusLabel.Text = "0";
            this.filesInQueueStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tssbtnSetAside
            // 
            this.tssbtnSetAside.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tssbtnSetAside.DropDownButtonWidth = 21;
            this.tssbtnSetAside.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tssmiReturnDeferredItems});
            this.tssbtnSetAside.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tssbtnSetAside.Name = "tssbtnSetAside";
            this.tssbtnSetAside.Size = new System.Drawing.Size(118, 28);
            this.tssbtnSetAside.Text = "Set aside:";
            this.tssbtnSetAside.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.tssbtnSetAside.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.tssbtnSetAside.ToolTipText = "Set aside";
            this.tssbtnSetAside.Click += new System.EventHandler(this.tssbtnSetAside_Click);
            // 
            // tssmiReturnDeferredItems
            // 
            this.tssmiReturnDeferredItems.Name = "tssmiReturnDeferredItems";
            this.tssmiReturnDeferredItems.Size = new System.Drawing.Size(261, 28);
            this.tssmiReturnDeferredItems.Text = "Return deferred items";
            this.tssmiReturnDeferredItems.Click += new System.EventHandler(this.tssmiReturnDeferredItems_Click);
            // 
            // tsslSetAside
            // 
            this.tsslSetAside.Name = "tsslSetAside";
            this.tsslSetAside.Size = new System.Drawing.Size(20, 25);
            this.tsslSetAside.Text = "0";
            // 
            // StatusTextLabel
            // 
            this.StatusTextLabel.Name = "StatusTextLabel";
            this.StatusTextLabel.Size = new System.Drawing.Size(65, 25);
            this.StatusTextLabel.Text = "Status:";
            // 
            // StatusLabel
            // 
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(119, 25);
            this.StatusLabel.Text = "Initialization...";
            this.StatusLabel.TextChanged += new System.EventHandler(this.StatusLabel_TextChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 32);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            this.splitContainer1.Panel1MinSize = 236;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Panel2MinSize = 300;
            this.splitContainer1.Size = new System.Drawing.Size(1364, 432);
            this.splitContainer1.SplitterDistance = 394;
            this.splitContainer1.TabIndex = 0;
            this.splitContainer1.TabStop = false;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.panel5);
            this.splitContainer2.Panel1.Controls.Add(this.ActionMenu);
            // 
            // miniatureList
            // 
            this.miniatureList.DataSource = null;
            this.miniatureList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.miniatureList.Location = new System.Drawing.Point(0, 0);
            this.miniatureList.Name = "miniatureList";
            this.miniatureList.SelectedControl = null;
            this.miniatureList.SelectedIndex = 0;
            this.miniatureList.SelectedItem = null;
            this.miniatureList.Size = new System.Drawing.Size(307, 218);
            this.miniatureList.TabIndex = 0;
            this.miniatureList.TabStop = false;
            this.miniatureList.SelectedItemChanged += new System.EventHandler(this.MiniatureSelectedItemChanged);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.AutoScroll = true;
            this.splitContainer2.Panel2.Controls.Add(this.miniatureList);
            this.splitContainer2.Size = new System.Drawing.Size(390, 428);
            this.splitContainer2.SplitterDistance = 209;
            this.splitContainer2.TabIndex = 0;
            this.splitContainer2.TabStop = false;
            // 
            // BoxSheet
            // 
            this.BoxSheet.Dock = System.Windows.Forms.DockStyle.Top;
            this.BoxSheet.IsSelected = false;
            this.BoxSheet.Item = null;
            this.BoxSheet.Location = new System.Drawing.Point(0, 0);
            this.BoxSheet.Margin = new System.Windows.Forms.Padding(0);
            this.BoxSheet.Name = "BoxSheet";
            this.BoxSheet.Size = new System.Drawing.Size(307, 61);
            this.BoxSheet.TabIndex = 18;
            this.BoxSheet.TabStop = false;
            this.BoxSheet.Text = "Name";
            this.BoxSheet.OkClick += new System.EventHandler(this.BoxSheet_OkClick);
            this.BoxSheet.SelectedValueChanged += new System.EventHandler(this.BoxSheet_SelectedValueChanged);
            // 
            // barCodeList
            // 
            this.barCodeList.DataSource = null;
            this.barCodeList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.barCodeList.Location = new System.Drawing.Point(0, 61);
            this.barCodeList.Name = "barCodeList";
            this.barCodeList.SelectedControl = null;
            this.barCodeList.SelectedIndex = 0;
            this.barCodeList.SelectedItem = null;
            this.barCodeList.Size = new System.Drawing.Size(307, 115);
            this.barCodeList.TabIndex = 18;
            this.barCodeList.TabStop = false;
            this.barCodeList.OkButtonClick += new System.EventHandler(this.barCodeList_OkButtonClick);
            this.barCodeList.SelectedItemChanged += new System.EventHandler(this.BarCodeSelectedItemChanged);
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.barCodeList);
            this.panel5.Controls.Add(this.BoxSheet);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(0, 38);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(390, 171);
            this.panel5.TabIndex = 0;
            // 
            // ActionMenu
            // 
            this.ActionMenu.AutoSize = true;
            this.ActionMenu.Controls.Add(this.VerifyButton);
            this.ActionMenu.Controls.Add(this.NextButton);
            this.ActionMenu.Controls.Add(this.DeleteButton);
            this.ActionMenu.Controls.Add(this.btnSetAside);
            this.ActionMenu.Controls.Add(this.btnDeferred);
            this.ActionMenu.Dock = System.Windows.Forms.DockStyle.Top;
            this.ActionMenu.Location = new System.Drawing.Point(0, 0);
            this.ActionMenu.Name = "ActionMenu";
            this.ActionMenu.Size = new System.Drawing.Size(390, 38);
            this.ActionMenu.TabIndex = 0;
            this.ActionMenu.WrapContents = false;
            // 
            // VerifyButton
            // 
            this.VerifyButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.VerifyButton.ImageKey = "check_24.png";
            this.VerifyButton.ImageList = this.imageList24;
            this.VerifyButton.Location = new System.Drawing.Point(3, 3);
            this.VerifyButton.Name = "VerifyButton";
            this.VerifyButton.Size = new System.Drawing.Size(65, 32);
            this.VerifyButton.TabIndex = 1;
            this.VerifyButton.Text = "Verify";
            this.VerifyButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip1.SetToolTip(this.VerifyButton, "Move to  folder \"Success\" (Ctrl+M)");
            this.VerifyButton.UseVisualStyleBackColor = true;
            this.VerifyButton.Click += new System.EventHandler(this.VerifyButton_Click);
            // 
            // imageList24
            // 
            this.imageList24.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList24.ImageStream")));
            this.imageList24.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList24.Images.SetKeyName(0, "cancel_24.png");
            this.imageList24.Images.SetKeyName(1, "check_24.png");
            this.imageList24.Images.SetKeyName(2, "open_24.png");
            this.imageList24.Images.SetKeyName(3, "recyclebin_empty_24.png");
            this.imageList24.Images.SetKeyName(4, "redo_24.png");
            this.imageList24.Images.SetKeyName(5, "size2.ico");
            this.imageList24.Images.SetKeyName(6, "size3.ico");
            this.imageList24.Images.SetKeyName(7, "speech-balloon-green-b24.png");
            this.imageList24.Images.SetKeyName(8, "speech-balloon-green-r24.png");
            this.imageList24.Images.SetKeyName(9, "undo_24.png");
            this.imageList24.Images.SetKeyName(10, "zoom_in_24.png");
            this.imageList24.Images.SetKeyName(11, "zoom_out_24.png");
            this.imageList24.Images.SetKeyName(12, "next_24.png");
            this.imageList24.Images.SetKeyName(13, "clean_24.png");
            this.imageList24.Images.SetKeyName(14, "3x3_grid.ico");
            this.imageList24.Images.SetKeyName(15, "20121014022119950_easyicon_cn_24.ico");
            this.imageList24.Images.SetKeyName(16, "Icons8-Windows-8-Data-Grid.ico");
            this.imageList24.Images.SetKeyName(17, "1416579366_ic_rotate_right_48px-24.png");
            this.imageList24.Images.SetKeyName(18, "1416579582_grid-24.png");
            this.imageList24.Images.SetKeyName(19, "Check.ico");
            this.imageList24.Images.SetKeyName(20, "cut.ico");
            this.imageList24.Images.SetKeyName(21, "1423521066_326660.ico");
            this.imageList24.Images.SetKeyName(22, "Recykle.ico");
            // 
            // NextButton
            // 
            this.NextButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.NextButton.ImageKey = "next_24.png";
            this.NextButton.ImageList = this.imageList24;
            this.NextButton.Location = new System.Drawing.Point(74, 3);
            this.NextButton.Name = "NextButton";
            this.NextButton.Size = new System.Drawing.Size(78, 32);
            this.NextButton.TabIndex = 2;
            this.NextButton.Text = "AnyDoc";
            this.NextButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip1.SetToolTip(this.NextButton, "Move to next processing folder (Ctrl+N)");
            this.NextButton.UseVisualStyleBackColor = true;
            this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
            // 
            // DeleteButton
            // 
            this.DeleteButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.DeleteButton.ImageKey = "recyclebin_empty_24.png";
            this.DeleteButton.ImageList = this.imageList24;
            this.DeleteButton.Location = new System.Drawing.Point(158, 3);
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(71, 32);
            this.DeleteButton.TabIndex = 3;
            this.DeleteButton.Text = "Delete";
            this.DeleteButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip1.SetToolTip(this.DeleteButton, "Move to trash (Ctrl+D)");
            this.DeleteButton.UseVisualStyleBackColor = true;
            this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            // 
            // btnSetAside
            // 
            this.btnSetAside.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSetAside.ImageKey = "(отсутствует)";
            this.btnSetAside.ImageList = this.imageList24;
            this.btnSetAside.Location = new System.Drawing.Point(235, 3);
            this.btnSetAside.Name = "btnSetAside";
            this.btnSetAside.Size = new System.Drawing.Size(71, 32);
            this.btnSetAside.TabIndex = 4;
            this.btnSetAside.Text = "Set aside";
            this.btnSetAside.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip1.SetToolTip(this.btnSetAside, "Set aside");
            this.btnSetAside.UseVisualStyleBackColor = true;
            this.btnSetAside.Click += new System.EventHandler(this.btnSetAside_Click);
            // 
            // btnDeferred
            // 
            this.btnDeferred.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDeferred.ImageKey = "(отсутствует)";
            this.btnDeferred.ImageList = this.imageList24;
            this.btnDeferred.Location = new System.Drawing.Point(312, 3);
            this.btnDeferred.Name = "btnDeferred";
            this.btnDeferred.Size = new System.Drawing.Size(71, 32);
            this.btnDeferred.TabIndex = 5;
            this.btnDeferred.Text = "Deferred";
            this.btnDeferred.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip1.SetToolTip(this.btnDeferred, "Return deferred items");
            this.btnDeferred.UseVisualStyleBackColor = true;
            this.btnDeferred.Click += new System.EventHandler(this.btnDeferred_Click);
            // 
            // splitContainer3
            // 
            this.splitContainer3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.ImagePanel);
            this.splitContainer3.Panel1.Controls.Add(this.AnswerSheetMenu);
            this.splitContainer3.Panel1MinSize = 300;
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.splitContainer4);
            this.splitContainer3.Panel2.Controls.Add(this.BubbleAreaMenu);
            this.splitContainer3.Panel2MinSize = 100;
            this.splitContainer3.Size = new System.Drawing.Size(966, 432);
            this.splitContainer3.SplitterDistance = 858;
            this.splitContainer3.TabIndex = 0;
            this.splitContainer3.TabStop = false;
            // 
            // ImagePanel
            // 
            this.ImagePanel.AutoScroll = true;
            this.ImagePanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ImagePanel.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ImagePanel.Controls.Add(this.pictureBox1);
            this.ImagePanel.Controls.Add(this.pnlScrollBubble);
            this.ImagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ImagePanel.Location = new System.Drawing.Point(0, 40);
            this.ImagePanel.Name = "ImagePanel";
            this.ImagePanel.Size = new System.Drawing.Size(854, 388);
            this.ImagePanel.TabIndex = 0;
            this.ImagePanel.Tag = "";
            // 
            // pictureBox1
            // 
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(173, 46);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(320, 179);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            this.pictureBox1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseClick);
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
            this.pictureBox1.MouseEnter += new System.EventHandler(this.pictureBox1_MouseEnter);
            this.pictureBox1.MouseLeave += new System.EventHandler(this.pictureBox1_MouseLeave);
            this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseMove);
            this.pictureBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseUp);
            // 
            // pnlScrollBubble
            // 
            this.pnlScrollBubble.BackColor = System.Drawing.Color.Transparent;
            this.pnlScrollBubble.Location = new System.Drawing.Point(26, 78);
            this.pnlScrollBubble.Name = "pnlScrollBubble";
            this.pnlScrollBubble.Size = new System.Drawing.Size(26, 27);
            this.pnlScrollBubble.TabIndex = 0;
            // 
            // splitBtnRestore
            // 
            this.splitBtnRestore.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.splitBtnRestore.ImageKey = "20121014022119950_easyicon_cn_24.ico";
            this.splitBtnRestore.ImageList = this.imageList24;
            this.splitBtnRestore.Location = new System.Drawing.Point(573, 3);
            this.splitBtnRestore.Name = "splitBtnRestore";
            this.splitBtnRestore.Size = new System.Drawing.Size(91, 32);
            this.splitBtnRestore.TabIndex = 1;
            this.splitBtnRestore.Text = "Restore";
            this.splitBtnRestore.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip1.SetToolTip(this.splitBtnRestore, "Restore image");
            this.splitBtnRestore.UseVisualStyleBackColor = true;
            this.splitBtnRestore.EnabledChanged += new System.EventHandler(this.btnRestore_EnabledChanged);
            this.splitBtnRestore.Click += new System.EventHandler(this.btnRestore_Click);
            // 
            // AnswerSheetMenu
            // 
            this.AnswerSheetMenu.AutoSize = true;
            this.AnswerSheetMenu.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.AnswerSheetMenu.Controls.Add(this.StopRecognizeButton);
            this.AnswerSheetMenu.Controls.Add(this.btnOpenFile);
            this.AnswerSheetMenu.Controls.Add(this.RecognizeAllButton);
            this.AnswerSheetMenu.Controls.Add(this.RecognizeBubblesButton);
            this.AnswerSheetMenu.Controls.Add(this.SizeFitButton);
            this.AnswerSheetMenu.Controls.Add(this.SizeFullButton);
            this.AnswerSheetMenu.Controls.Add(this.SizePlusButton);
            this.AnswerSheetMenu.Controls.Add(this.SizeMinusButton);
            this.AnswerSheetMenu.Controls.Add(this.rbtnBubbles);
            this.AnswerSheetMenu.Controls.Add(this.rbtnGrid);
            this.AnswerSheetMenu.Controls.Add(this.RotateLeftButton);
            this.AnswerSheetMenu.Controls.Add(this.RotateRightButton);
            this.AnswerSheetMenu.Controls.Add(this.rbtnRotate);
            this.AnswerSheetMenu.Controls.Add(this.btnRemoveNoise);
            this.AnswerSheetMenu.Controls.Add(this.rbtnCut);
            this.AnswerSheetMenu.Controls.Add(this.rbtnClear);
            this.AnswerSheetMenu.Controls.Add(this.splitBtnRestore);
            this.AnswerSheetMenu.Controls.Add(this.OpenFilesDirButton);
            this.AnswerSheetMenu.Controls.Add(this.button1);
            this.AnswerSheetMenu.Controls.Add(this.btnInvert);
            this.AnswerSheetMenu.Controls.Add(this.DarknessGroupBox);
            this.AnswerSheetMenu.Dock = System.Windows.Forms.DockStyle.Top;
            this.AnswerSheetMenu.Location = new System.Drawing.Point(0, 0);
            this.AnswerSheetMenu.MaximumSize = new System.Drawing.Size(0, 44);
            this.AnswerSheetMenu.Name = "AnswerSheetMenu";
            this.AnswerSheetMenu.Size = new System.Drawing.Size(854, 40);
            this.AnswerSheetMenu.TabIndex = 0;
            this.AnswerSheetMenu.WrapContents = false;
            // 
            // StopRecognizeButton
            // 
            this.StopRecognizeButton.ImageKey = "cancel_24.png";
            this.StopRecognizeButton.ImageList = this.imageList24;
            this.StopRecognizeButton.Location = new System.Drawing.Point(3, 3);
            this.StopRecognizeButton.Name = "StopRecognizeButton";
            this.StopRecognizeButton.Size = new System.Drawing.Size(32, 32);
            this.StopRecognizeButton.TabIndex = 7;
            this.toolTip1.SetToolTip(this.StopRecognizeButton, "Stop Recognizing");
            this.StopRecognizeButton.UseVisualStyleBackColor = true;
            this.StopRecognizeButton.Visible = false;
            this.StopRecognizeButton.Click += new System.EventHandler(this.StopRecognizeButton_Click);
            // 
            // btnOpenFile
            // 
            this.btnOpenFile.ImageKey = "open_24.png";
            this.btnOpenFile.ImageList = this.imageList24;
            this.btnOpenFile.Location = new System.Drawing.Point(41, 3);
            this.btnOpenFile.Name = "btnOpenFile";
            this.btnOpenFile.Size = new System.Drawing.Size(32, 32);
            this.btnOpenFile.TabIndex = 18;
            this.toolTip1.SetToolTip(this.btnOpenFile, "Open processing file\r\n        in explorer");
            this.btnOpenFile.UseVisualStyleBackColor = true;
            this.btnOpenFile.Click += new System.EventHandler(this.btnOpenFile_Click);
            // 
            // RecognizeAllButton
            // 
            this.RecognizeAllButton.ImageKey = "speech-balloon-green-r24.png";
            this.RecognizeAllButton.ImageList = this.imageList24;
            this.RecognizeAllButton.Location = new System.Drawing.Point(79, 3);
            this.RecognizeAllButton.Name = "RecognizeAllButton";
            this.RecognizeAllButton.Size = new System.Drawing.Size(32, 32);
            this.RecognizeAllButton.TabIndex = 5;
            this.toolTip1.SetToolTip(this.RecognizeAllButton, "Recognize all");
            this.RecognizeAllButton.UseVisualStyleBackColor = true;
            this.RecognizeAllButton.Click += new System.EventHandler(this.RecognizeAllButton_Click);
            // 
            // RecognizeBubblesButton
            // 
            this.RecognizeBubblesButton.ImageKey = "speech-balloon-green-b24.png";
            this.RecognizeBubblesButton.ImageList = this.imageList24;
            this.RecognizeBubblesButton.Location = new System.Drawing.Point(117, 3);
            this.RecognizeBubblesButton.Name = "RecognizeBubblesButton";
            this.RecognizeBubblesButton.Size = new System.Drawing.Size(32, 32);
            this.RecognizeBubblesButton.TabIndex = 6;
            this.toolTip1.SetToolTip(this.RecognizeBubblesButton, "Recognize bubbles only");
            this.RecognizeBubblesButton.UseVisualStyleBackColor = true;
            this.RecognizeBubblesButton.Click += new System.EventHandler(this.RecognizeBubblesButton_Click);
            // 
            // SizeFitButton
            // 
            this.SizeFitButton.ImageKey = "size2.ico";
            this.SizeFitButton.ImageList = this.imageList24;
            this.SizeFitButton.Location = new System.Drawing.Point(155, 3);
            this.SizeFitButton.Name = "SizeFitButton";
            this.SizeFitButton.Size = new System.Drawing.Size(32, 32);
            this.SizeFitButton.TabIndex = 8;
            this.toolTip1.SetToolTip(this.SizeFitButton, "Fit in window");
            this.SizeFitButton.UseVisualStyleBackColor = true;
            this.SizeFitButton.Click += new System.EventHandler(this.SizeFitButton_Click);
            // 
            // SizeFullButton
            // 
            this.SizeFullButton.ImageKey = "size3.ico";
            this.SizeFullButton.ImageList = this.imageList24;
            this.SizeFullButton.Location = new System.Drawing.Point(193, 3);
            this.SizeFullButton.Name = "SizeFullButton";
            this.SizeFullButton.Size = new System.Drawing.Size(32, 32);
            this.SizeFullButton.TabIndex = 9;
            this.toolTip1.SetToolTip(this.SizeFullButton, "Natural size");
            this.SizeFullButton.UseVisualStyleBackColor = true;
            this.SizeFullButton.Click += new System.EventHandler(this.SizeFullButton_Click);
            // 
            // SizePlusButton
            // 
            this.SizePlusButton.ImageKey = "zoom_in_24.png";
            this.SizePlusButton.ImageList = this.imageList24;
            this.SizePlusButton.Location = new System.Drawing.Point(231, 3);
            this.SizePlusButton.Name = "SizePlusButton";
            this.SizePlusButton.Size = new System.Drawing.Size(32, 32);
            this.SizePlusButton.TabIndex = 10;
            this.toolTip1.SetToolTip(this.SizePlusButton, "+20%");
            this.SizePlusButton.UseVisualStyleBackColor = true;
            this.SizePlusButton.Click += new System.EventHandler(this.SizePlusButton_Click);
            // 
            // SizeMinusButton
            // 
            this.SizeMinusButton.ImageKey = "zoom_out_24.png";
            this.SizeMinusButton.ImageList = this.imageList24;
            this.SizeMinusButton.Location = new System.Drawing.Point(269, 3);
            this.SizeMinusButton.Name = "SizeMinusButton";
            this.SizeMinusButton.Size = new System.Drawing.Size(32, 32);
            this.SizeMinusButton.TabIndex = 11;
            this.toolTip1.SetToolTip(this.SizeMinusButton, "-20%");
            this.SizeMinusButton.UseVisualStyleBackColor = true;
            this.SizeMinusButton.Click += new System.EventHandler(this.SizeMinusButton_Click);
            // 
            // rbtnBubbles
            // 
            this.rbtnBubbles.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbtnBubbles.BackColor = System.Drawing.SystemColors.Control;
            this.rbtnBubbles.ImageKey = "1416579582_grid-24.png";
            this.rbtnBubbles.ImageList = this.imageList24;
            this.rbtnBubbles.Location = new System.Drawing.Point(307, 3);
            this.rbtnBubbles.Name = "rbtnBubbles";
            this.rbtnBubbles.Size = new System.Drawing.Size(32, 32);
            this.rbtnBubbles.TabIndex = 22;
            this.rbtnBubbles.TabStop = true;
            this.toolTip1.SetToolTip(this.rbtnBubbles, "Bubbles");
            this.rbtnBubbles.UseVisualStyleBackColor = false;
            this.rbtnBubbles.CheckedChanged += new System.EventHandler(this.rbtnBubbles_CheckedChanged);
            // 
            // rbtnGrid
            // 
            this.rbtnGrid.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbtnGrid.BackColor = System.Drawing.SystemColors.Control;
            this.rbtnGrid.ImageKey = "Icons8-Windows-8-Data-Grid.ico";
            this.rbtnGrid.ImageList = this.imageList24;
            this.rbtnGrid.Location = new System.Drawing.Point(345, 3);
            this.rbtnGrid.Name = "rbtnGrid";
            this.rbtnGrid.Size = new System.Drawing.Size(32, 32);
            this.rbtnGrid.TabIndex = 21;
            this.rbtnGrid.TabStop = true;
            this.toolTip1.SetToolTip(this.rbtnGrid, "Grid");
            this.rbtnGrid.UseVisualStyleBackColor = false;
            this.rbtnGrid.CheckedChanged += new System.EventHandler(this.rbtnRotate_CheckedChanged);
            this.rbtnGrid.Click += new System.EventHandler(this.btnGrid_Click);
            // 
            // RotateLeftButton
            // 
            this.RotateLeftButton.BackColor = System.Drawing.SystemColors.Control;
            this.RotateLeftButton.ImageKey = "undo_24.png";
            this.RotateLeftButton.ImageList = this.imageList24;
            this.RotateLeftButton.Location = new System.Drawing.Point(383, 3);
            this.RotateLeftButton.Name = "RotateLeftButton";
            this.RotateLeftButton.Size = new System.Drawing.Size(32, 32);
            this.RotateLeftButton.TabIndex = 12;
            this.toolTip1.SetToolTip(this.RotateLeftButton, "Rotate Left (Ctrl+ Left)");
            this.RotateLeftButton.UseVisualStyleBackColor = false;
            this.RotateLeftButton.Click += new System.EventHandler(this.RotateLeftButton_Click);
            // 
            // RotateRightButton
            // 
            this.RotateRightButton.ImageKey = "redo_24.png";
            this.RotateRightButton.ImageList = this.imageList24;
            this.RotateRightButton.Location = new System.Drawing.Point(421, 3);
            this.RotateRightButton.Name = "RotateRightButton";
            this.RotateRightButton.Size = new System.Drawing.Size(32, 32);
            this.RotateRightButton.TabIndex = 13;
            this.toolTip1.SetToolTip(this.RotateRightButton, "Rotate Right  (Ctrl+Right)");
            this.RotateRightButton.UseVisualStyleBackColor = true;
            this.RotateRightButton.Click += new System.EventHandler(this.RotateRightButton_Click);
            // 
            // rbtnRotate
            // 
            this.rbtnRotate.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbtnRotate.BackColor = System.Drawing.SystemColors.Control;
            this.rbtnRotate.ImageKey = "1416579366_ic_rotate_right_48px-24.png";
            this.rbtnRotate.ImageList = this.imageList24;
            this.rbtnRotate.Location = new System.Drawing.Point(459, 3);
            this.rbtnRotate.Name = "rbtnRotate";
            this.rbtnRotate.Size = new System.Drawing.Size(32, 32);
            this.rbtnRotate.TabIndex = 20;
            this.rbtnRotate.TabStop = true;
            this.toolTip1.SetToolTip(this.rbtnRotate, "Align");
            this.rbtnRotate.UseVisualStyleBackColor = false;
            this.rbtnRotate.CheckedChanged += new System.EventHandler(this.rbtnRotate_CheckedChanged);
            this.rbtnRotate.Click += new System.EventHandler(this.rbtnRotate_Click);
            // 
            // btnRemoveNoise
            // 
            this.btnRemoveNoise.BackColor = System.Drawing.SystemColors.Control;
            this.btnRemoveNoise.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnRemoveNoise.ForeColor = System.Drawing.Color.Blue;
            this.btnRemoveNoise.ImageKey = "clean_24.png";
            this.btnRemoveNoise.ImageList = this.imageList24;
            this.btnRemoveNoise.Location = new System.Drawing.Point(497, 3);
            this.btnRemoveNoise.Name = "btnRemoveNoise";
            this.btnRemoveNoise.Size = new System.Drawing.Size(32, 32);
            this.btnRemoveNoise.TabIndex = 15;
            this.toolTip1.SetToolTip(this.btnRemoveNoise, "Remove noise (Ctrl+R)");
            this.btnRemoveNoise.UseVisualStyleBackColor = false;
            this.btnRemoveNoise.Click += new System.EventHandler(this.btnRemoveNoise_Click);
            // 
            // rbtnCut
            // 
            this.rbtnCut.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbtnCut.BackColor = System.Drawing.SystemColors.Control;
            this.rbtnCut.ImageKey = "cut.ico";
            this.rbtnCut.ImageList = this.imageList24;
            this.rbtnCut.Location = new System.Drawing.Point(535, 3);
            this.rbtnCut.Name = "rbtnCut";
            this.rbtnCut.Size = new System.Drawing.Size(32, 32);
            this.rbtnCut.TabIndex = 23;
            this.rbtnCut.TabStop = true;
            this.toolTip1.SetToolTip(this.rbtnCut, "Cut selected area");
            this.rbtnCut.UseVisualStyleBackColor = false;
            this.rbtnCut.CheckedChanged += new System.EventHandler(this.rbtnCut_CheckedChanged);
            this.rbtnCut.Click += new System.EventHandler(this.rbtnRotate_Click);
            // 
            // OpenFilesDirButton
            // 
            this.OpenFilesDirButton.Enabled = false;
            this.OpenFilesDirButton.ImageKey = "open_24.png";
            this.OpenFilesDirButton.ImageList = this.imageList24;
            this.OpenFilesDirButton.Location = new System.Drawing.Point(611, 3);
            this.OpenFilesDirButton.Name = "OpenFilesDirButton";
            this.OpenFilesDirButton.Size = new System.Drawing.Size(32, 32);
            this.OpenFilesDirButton.TabIndex = 4;
            this.toolTip1.SetToolTip(this.OpenFilesDirButton, "Open the source file\r\n        in explorer\r\n");
            this.OpenFilesDirButton.UseVisualStyleBackColor = true;
            this.OpenFilesDirButton.Click += new System.EventHandler(this.OpenFilesDirButton_Click);
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.ImageKey = "Recykle.ico";
            this.button1.ImageList = this.imageList24;
            this.button1.Location = new System.Drawing.Point(649, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(32, 32);
            this.button1.TabIndex = 17;
            this.toolTip1.SetToolTip(this.button1, "Reprocess source file");
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnInvert
            // 
            this.btnInvert.ImageKey = "1423521066_326660.ico";
            this.btnInvert.ImageList = this.imageList24;
            this.btnInvert.Location = new System.Drawing.Point(687, 3);
            this.btnInvert.Name = "btnInvert";
            this.btnInvert.Size = new System.Drawing.Size(32, 32);
            this.btnInvert.TabIndex = 16;
            this.toolTip1.SetToolTip(this.btnInvert, "Invert color");
            this.btnInvert.UseVisualStyleBackColor = true;
            this.btnInvert.Click += new System.EventHandler(this.btnInvert_Click);
            // 
            // DarknessGroupBox
            // 
            this.DarknessGroupBox.Controls.Add(this.nudPerCentEmptyBubble);
            this.DarknessGroupBox.Controls.Add(this.DarknessManualySet);
            this.DarknessGroupBox.Controls.Add(this.nudPerCentBestBubble);
            this.DarknessGroupBox.Location = new System.Drawing.Point(725, 3);
            this.DarknessGroupBox.Name = "DarknessGroupBox";
            this.DarknessGroupBox.Size = new System.Drawing.Size(110, 34);
            this.DarknessGroupBox.TabIndex = 14;
            this.DarknessGroupBox.TabStop = false;
            this.toolTip1.SetToolTip(this.DarknessGroupBox, "Manually set");
            // 
            // nudPerCentEmptyBubble
            // 
            this.nudPerCentEmptyBubble.Enabled = false;
            this.nudPerCentEmptyBubble.Location = new System.Drawing.Point(19, 10);
            this.nudPerCentEmptyBubble.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.nudPerCentEmptyBubble.Name = "nudPerCentEmptyBubble";
            this.nudPerCentEmptyBubble.Size = new System.Drawing.Size(39, 20);
            this.nudPerCentEmptyBubble.TabIndex = 15;
            this.toolTip1.SetToolTip(this.nudPerCentEmptyBubble, "Maximum percentage of the empty bubbles");
            this.nudPerCentEmptyBubble.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.nudPerCentEmptyBubble.ValueChanged += new System.EventHandler(this.nudPerCentEmptyBubble_ValueChanged);
            // 
            // DarknessManualySet
            // 
            this.DarknessManualySet.AutoSize = true;
            this.DarknessManualySet.Location = new System.Drawing.Point(0, 1);
            this.DarknessManualySet.Name = "DarknessManualySet";
            this.DarknessManualySet.Size = new System.Drawing.Size(15, 14);
            this.DarknessManualySet.TabIndex = 14;
            this.DarknessManualySet.UseVisualStyleBackColor = true;
            this.DarknessManualySet.CheckedChanged += new System.EventHandler(this.DarknessManualySet_CheckedChanged);
            // 
            // nudPerCentBestBubble
            // 
            this.nudPerCentBestBubble.Enabled = false;
            this.nudPerCentBestBubble.Location = new System.Drawing.Point(64, 10);
            this.nudPerCentBestBubble.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.nudPerCentBestBubble.Name = "nudPerCentBestBubble";
            this.nudPerCentBestBubble.Size = new System.Drawing.Size(39, 20);
            this.nudPerCentBestBubble.TabIndex = 16;
            this.toolTip1.SetToolTip(this.nudPerCentBestBubble, "Percentage for better shaded bubble");
            this.nudPerCentBestBubble.Value = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.nudPerCentBestBubble.ValueChanged += new System.EventHandler(this.nudPerCentBestBubble_ValueChanged);
            // 
            // splitContainer4
            // 
            this.splitContainer4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(0, 38);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.AutoScroll = true;
            this.splitContainer4.Panel1.Controls.Add(this.pnlBubbles);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.nudZoom);
            this.splitContainer4.Panel2.Controls.Add(this.pictureBox2);
            this.splitContainer4.Size = new System.Drawing.Size(104, 394);
            this.splitContainer4.SplitterDistance = 264;
            this.splitContainer4.TabIndex = 1;
            // 
            // pnlBubbles
            // 
            this.pnlBubbles.AutoScroll = true;
            this.pnlBubbles.AutoSize = true;
            this.pnlBubbles.BackColor = System.Drawing.SystemColors.Control;
            this.pnlBubbles.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlBubbles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBubbles.Location = new System.Drawing.Point(0, 0);
            this.pnlBubbles.Name = "pnlBubbles";
            this.pnlBubbles.Size = new System.Drawing.Size(100, 260);
            this.pnlBubbles.TabIndex = 0;
            // 
            // nudZoom
            // 
            this.nudZoom.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.nudZoom.Location = new System.Drawing.Point(1, 1);
            this.nudZoom.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.nudZoom.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudZoom.Name = "nudZoom";
            this.nudZoom.Size = new System.Drawing.Size(32, 29);
            this.nudZoom.TabIndex = 1;
            this.toolTip1.SetToolTip(this.nudZoom, "Zoom factor");
            this.nudZoom.UpDownAlign = System.Windows.Forms.LeftRightAlignment.Left;
            this.nudZoom.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // pictureBox2
            // 
            this.pictureBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox2.Location = new System.Drawing.Point(0, 0);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(100, 122);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 0;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.MouseEnter += new System.EventHandler(this.pictureBox2_MouseEnter);
            // 
            // BubbleAreaMenu
            // 
            this.BubbleAreaMenu.Controls.Add(this.ClearAllCheckMarksButton);
            this.BubbleAreaMenu.Controls.Add(this.btnGrid);
            this.BubbleAreaMenu.Dock = System.Windows.Forms.DockStyle.Top;
            this.BubbleAreaMenu.Location = new System.Drawing.Point(0, 0);
            this.BubbleAreaMenu.Name = "BubbleAreaMenu";
            this.BubbleAreaMenu.Size = new System.Drawing.Size(104, 38);
            this.BubbleAreaMenu.TabIndex = 1;
            // 
            // ClearAllCheckMarksButton
            // 
            this.ClearAllCheckMarksButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.ClearAllCheckMarksButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ClearAllCheckMarksButton.ImageKey = "Check.ico";
            this.ClearAllCheckMarksButton.ImageList = this.imageList24;
            this.ClearAllCheckMarksButton.Location = new System.Drawing.Point(3, 3);
            this.ClearAllCheckMarksButton.Name = "ClearAllCheckMarksButton";
            this.ClearAllCheckMarksButton.Size = new System.Drawing.Size(32, 32);
            this.ClearAllCheckMarksButton.TabIndex = 17;
            this.ClearAllCheckMarksButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip1.SetToolTip(this.ClearAllCheckMarksButton, "Clear all check marks");
            this.ClearAllCheckMarksButton.UseVisualStyleBackColor = true;
            this.ClearAllCheckMarksButton.Click += new System.EventHandler(this.ClearAllCheckMarksButton_Click);
            // 
            // btnGrid
            // 
            this.btnGrid.Enabled = false;
            this.btnGrid.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnGrid.ImageKey = "Icons8-Windows-8-Data-Grid.ico";
            this.btnGrid.ImageList = this.imageList24;
            this.btnGrid.Location = new System.Drawing.Point(3, 41);
            this.btnGrid.Name = "btnGrid";
            this.btnGrid.Size = new System.Drawing.Size(75, 32);
            this.btnGrid.TabIndex = 18;
            this.btnGrid.Text = "Grid";
            this.btnGrid.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnGrid.UseVisualStyleBackColor = true;
            this.btnGrid.Visible = false;
            // 
            // imageList16
            // 
            this.imageList16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList16.ImageStream")));
            this.imageList16.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList16.Images.SetKeyName(0, "check_16.png");
            this.imageList16.Images.SetKeyName(1, "pics.ico");
            // 
            // timer1
            // 
            this.timer1.Interval = 200;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.SelectedPath = "C:\\Users\\Администратор\\Desktop";
            // 
            // btnCloseLblErr
            // 
            this.btnCloseLblErr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCloseLblErr.ImageKey = "pics.ico";
            this.btnCloseLblErr.ImageList = this.imageList16;
            this.btnCloseLblErr.Location = new System.Drawing.Point(1337, 4);
            this.btnCloseLblErr.Name = "btnCloseLblErr";
            this.btnCloseLblErr.Size = new System.Drawing.Size(24, 24);
            this.btnCloseLblErr.TabIndex = 3;
            this.btnCloseLblErr.TabStop = false;
            this.toolTip1.SetToolTip(this.btnCloseLblErr, "Verify value");
            this.btnCloseLblErr.UseVisualStyleBackColor = true;
            this.btnCloseLblErr.Visible = false;
            this.btnCloseLblErr.Click += new System.EventHandler(this.btnCloseLblErr_Click);
            // 
            // lblErr
            // 
            this.lblErr.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblErr.BackColor = System.Drawing.Color.Red;
            this.lblErr.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblErr.ForeColor = System.Drawing.Color.White;
            this.lblErr.ImageKey = "(отсутствует)";
            this.lblErr.Location = new System.Drawing.Point(248, 4);
            this.lblErr.Name = "lblErr";
            this.lblErr.Size = new System.Drawing.Size(1110, 24);
            this.lblErr.TabIndex = 1;
            this.lblErr.Text = "label1";
            this.lblErr.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblErr.Visible = false;
            this.lblErr.VisibleChanged += new System.EventHandler(this.lblErr_VisibleChanged);
            // 
            // cbDoNotProcess
            // 
            this.cbDoNotProcess.AutoSize = true;
            this.cbDoNotProcess.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.cbDoNotProcess.Location = new System.Drawing.Point(86, 3);
            this.cbDoNotProcess.Name = "cbDoNotProcess";
            this.cbDoNotProcess.Size = new System.Drawing.Size(156, 28);
            this.cbDoNotProcess.TabIndex = 2;
            this.cbDoNotProcess.Text = "Do not process";
            this.cbDoNotProcess.UseVisualStyleBackColor = true;
            this.cbDoNotProcess.CheckedChanged += new System.EventHandler(this.cbDoNotProcess_CheckedChanged);
            // 
            // toolTip2
            // 
            this.toolTip2.AutoPopDelay = 500;
            this.toolTip2.InitialDelay = 500;
            this.toolTip2.ReshowDelay = 100;
            this.toolTip2.ShowAlways = true;
            this.toolTip2.Popup += new System.Windows.Forms.PopupEventHandler(this.toolTip2_Popup);
            // 
            // timer2
            // 
            this.timer2.Enabled = true;
            this.timer2.Interval = 500;
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // rbtnClear
            // 
            this.rbtnClear.Appearance = System.Windows.Forms.Appearance.Button;
            this.rbtnClear.BackColor = System.Drawing.SystemColors.Control;
            this.rbtnClear.ImageKey = "(отсутствует)";
            this.rbtnClear.ImageList = this.imageList24;
            this.rbtnClear.Location = new System.Drawing.Point(573, 3);
            this.rbtnClear.Name = "rbtnClear";
            this.rbtnClear.Size = new System.Drawing.Size(32, 32);
            this.rbtnClear.TabIndex = 24;
            this.rbtnClear.TabStop = true;
            this.toolTip1.SetToolTip(this.rbtnClear, "Clear selected area");
            this.rbtnClear.UseVisualStyleBackColor = false;
            this.rbtnClear.CheckedChanged += new System.EventHandler(this.rbtnCut_CheckedChanged);
            this.rbtnClear.Click += new System.EventHandler(this.rbtnRotate_Click);
            // 
            // restartToolStripMenuItem
            // 
            this.restartToolStripMenuItem.Name = "restartToolStripMenuItem";
            this.restartToolStripMenuItem.Size = new System.Drawing.Size(385, 28);
            this.restartToolStripMenuItem.Text = "Restart";
            this.restartToolStripMenuItem.Click += new System.EventHandler(this.restartToolStripMenuItem_Click);
            // 
            // EditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1364, 494);
            this.Controls.Add(this.btnCloseLblErr);
            this.Controls.Add(this.cbDoNotProcess);
            this.Controls.Add(this.lblErr);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.MainMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.MainMenu;
            this.Name = "EditorForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditorForm_FormClosing);
            this.Load += new System.EventHandler(this.EditorForm_Load);
            this.Shown += new System.EventHandler(this.EditorForm_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.EditorForm_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.EditorForm_KeyUp);
            this.MainMenu.ResumeLayout(false);
            this.MainMenu.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ActionMenu.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel1.PerformLayout();
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.ImagePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.AnswerSheetMenu.ResumeLayout(false);
            this.DarknessGroupBox.ResumeLayout(false);
            this.DarknessGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPerCentEmptyBubble)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPerCentBestBubble)).EndInit();
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel1.PerformLayout();
            this.splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudZoom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.BubbleAreaMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip MainMenu;
        private System.Windows.Forms.ToolStripMenuItem fileMainMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openFileMainMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openManualInputFolderMainMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openProcessingFolderMainMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openErrorsResultsFolderMainMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openSuccessResultsFolderMainMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openAutoInputFolderMainMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem sendToSuportMainMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem exitMainMenuItem;
        private System.Windows.Forms.ToolStripMenuItem restartToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel filesInQueueTextStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel filesInQueueStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel StatusTextLabel;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Panel panel5;/////////////////////////////
        private System.Windows.Forms.Panel pnlScrollBubble;
        private System.Windows.Forms.FlowLayoutPanel AnswerSheetMenu;
        private System.Windows.Forms.FlowLayoutPanel ActionMenu;
        private System.Windows.Forms.FlowLayoutPanel BubbleAreaMenu;
        private System.Windows.Forms.Button VerifyButton;
        private System.Windows.Forms.Button NextButton;
        private System.Windows.Forms.Button DeleteButton;
        private System.Windows.Forms.Button OpenFilesDirButton;
        private System.Windows.Forms.Button RecognizeAllButton;
        private System.Windows.Forms.Button RecognizeBubblesButton;
        private System.Windows.Forms.Button StopRecognizeButton;
        private System.Windows.Forms.Button SizeFitButton;
        private System.Windows.Forms.Button SizeFullButton;
        private System.Windows.Forms.Button SizePlusButton;
        private System.Windows.Forms.Button SizeMinusButton;
        private System.Windows.Forms.Button RotateLeftButton;
        private System.Windows.Forms.Button RotateRightButton;
        private System.Windows.Forms.Button ClearAllCheckMarksButton;
        private System.Windows.Forms.GroupBox DarknessGroupBox;
        private System.Windows.Forms.NumericUpDown nudPerCentEmptyBubble;
        private System.Windows.Forms.CheckBox DarknessManualySet;
        private System.Windows.Forms.NumericUpDown nudPerCentBestBubble;
        private System.Windows.Forms.ImageList imageList24;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ImageList imageList16;
        private MiniatureListControl miniatureList;
        public BarCodeListControl barCodeList;
        private System.Windows.Forms.Panel ImagePanel;
        private System.Windows.Forms.Button btnGrid;
        public BarCodeListItemControl BoxSheet = new BarCodeListItemControl();
        public System.Windows.Forms.RadioButton rbtnRotate;
        public System.Windows.Forms.RadioButton rbtnGrid;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem SendAllToNextProcessingToolStripMenuItem;
        public System.Windows.Forms.RadioButton rbtnBubbles;
        public System.Windows.Forms.RadioButton rbtnCut;
        private System.Windows.Forms.Button btnRemoveNoise;
        private System.Windows.Forms.Label lblErr;
        private System.Windows.Forms.CheckBox cbDoNotProcess;
        public System.Windows.Forms.Button btnCloseLblErr;
        public System.Windows.Forms.SplitContainer splitContainer3;
        public System.Windows.Forms.SplitContainer splitContainer4;
        public System.Windows.Forms.Panel pnlBubbles;
        private System.Windows.Forms.PictureBox pictureBox2;
        public System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.Button btnInvert;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnOpenFile;
        private System.Windows.Forms.ToolTip toolTip2;
        private System.Windows.Forms.ToolStripMenuItem ReportTestPageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sendProblemToSupportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem returnFilesAndQuitToolStripMenuItem;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.ToolStripSplitButton tssbtnSetAside;
        private System.Windows.Forms.ToolStripMenuItem tssmiReturnDeferredItems;
        private System.Windows.Forms.ToolStripStatusLabel tsslSetAside;
        private System.Windows.Forms.Button btnSetAside;
        private System.Windows.Forms.Button btnDeferred;
        private System.Windows.Forms.NumericUpDown nudZoom;
        private SplitButton splitBtnRestore;
        public System.Windows.Forms.RadioButton rbtnClear;
    }
}