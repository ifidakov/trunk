using eDoctrinaUtils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using vb = Microsoft.VisualBasic;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Script.Serialization;
//using System.Web.Script.Serialization;

namespace eDoctrinaOcrEd
{
    public partial class EditorForm : Form
    {
        public EditorForm()
        {
            InitializeComponent();
            this.Text = appTitle;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.DoWork += DoWork;
            backgroundWorker.RunWorkerCompleted += Completed;
            BW.DoWork += DoWorkBW;
            BW.WorkerSupportsCancellation = true;
            filesInQueueTimer = new Timer();
            filesInQueueTimer.Interval = 200;
            filesInQueueTimer.Tick += new System.EventHandler(FilesInQueueTimer_Tick);
        }

        private string appTitle = " eDoctrina OCR Editor v. " + Application.ProductVersion + " (" + Environment.CurrentDirectory + ")";
        //-------------------------------------------------------------------------
        SendToSupportForm sts;
        public static BarCodeListItemControl SelectedBarCodeItem = null;
        public static bool barCodeSel = false;
        public static bool ShetIdManualySet = false;
        public static int maxAmoutOfQuestions = 0;
        public static int[] bubblesPerLine = new int[0];
        public static int[] linesPerArea = new int[0];
        public BubblesAreaControl bac = new BubblesAreaControl();
        public StatusMessage Status = StatusMessage.NULL;
        public LinsForm linsForm = null;
        public SettingsForm1 settingsForm1 = null;

        private OcrAppConfig defaults;
        public ObservableCollection<MiniatureItem> MiniatureItems = new ObservableCollection<MiniatureItem>();
        private ObservableCollection<BarCodeItem> BarCodeItems = new ObservableCollection<BarCodeItem>();
        private RecognitionTools recTools = new RecognitionTools();
        Point begPoint = new Point();
        Point begPointZoom = new Point();
        Bitmap recBitmap = null;
        Utils utils = new Utils();
        IOHelper iOHelper = new IOHelper();
        Log log = new Log();
        Rectangle area = Rectangle.Empty;
        Rectangle areaNaturalSize = Rectangle.Empty;
        Rectangle[] allAreasNaturalSize = new Rectangle[0];
        int amout_of_questionsIndex = 0;
        //Regions regions = null;
        private string lastSheetId = "";
        private string lastTestId = "";
        private string testId = "";
        private string lastAmoutOfQuestions = "";
        private string amoutOfQuestions = "";
        private string lastIndexOfQuestion = "";
        private string indexOfQuestion = "";
        private string districtId = "";
        private string lastDistrictId = "";
        private Rectangle curRect = Rectangle.Empty;
        private Rectangle etRect = Rectangle.Empty;
        private Rectangle currentBarCodeRectangle = Rectangle.Empty;
        private Rectangle lastSymbolRectangle = Rectangle.Empty;
        private int deltaY = 0;
        List<string> errList = new List<string>();
        List<string> notAccess = new List<string>();
        private int countAllFraves = 0;
        private bool isInvert = false;
        private bool isRotate = false;
        private bool isCut = false;
        private bool isClear = false;
        private bool usedPrevTool = false;
        //private bool showCaption = false;
        private string barCodesPrompt = "";
        private string appDir = "";
        private TestUids testUids;
        private Control[] editorTools = new Control[0];
        private bool sendToSupport = false;
        private const int minCountFilesInTempFolder = 5;

        //FileSystemWatcher watcher = new FileSystemWatcher();

        //-------------------------------------------------------------------------
        #region Init LoadSettings SaveSettings
        public void UpdateUI(string key, object value)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate
                {
                    UpdateUII(key, value);
                }));
            }
            else
            {
                UpdateUII(key, value);
            }
        }
        //-------------------------------------------------------------------------
        private bool messageBoxShow = false;
        //-------------------------------------------------------------------------
        private void UpdateUII(string key, object value)
        {
            if (value.ToString().StartsWith("M") || value.ToString().StartsWith("A"))
                if (linsForm == null)
                {
                    btnGrid.PerformClick();
                }
            Invoke(new MethodInvoker(delegate
            {
                switch (key)
                {
                    case "Exception"://"eDoctrina OCR Editor"
                        if (!messageBoxShow && !DeleteButton.Focused)// && NextButton.Focused
                        {
                            messageBoxShow = true;
                            MessageBox.Show(value.ToString(), Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            messageBoxShow = false;
                            //comboBoxFocused = false;
                        }
                        break;
                    case "StatusLabel":
                        StatusLabel.Text = value.ToString();
                        break;
                    case "nudPerCentEmptyBubble":
                        nudPerCentEmptyBubble.Value = (decimal)value;
                        break;
                    case "nudPerCentBestBubble":
                        nudPerCentBestBubble.Value = (decimal)value;
                        break;
                    case "filesInQueueStatusLabel":
                        filesInQueueStatusLabel.Text = value.ToString();
                        break;
                }
            }));
        }
        //-------------------------------------------------------------------------
        private void InitEventsAndBindings()
        {
            Recognize.FindedBarcodeControllEvent += Recognize_FindedBarcodeControllEvent;
            Recognize.ChangedBarCodesPrompt += Recognize_ChangedBarCodesPrompt;
            Recognize.ExceptionEvent += Recognize_ExceptionEvent;
            Recognize.ChangedBubble += Recognize_ChangedBubble;

            miniatureList.DataSource = MiniatureItems;
            miniatureList.SelectedIndex = -1;
            barCodeList.DataSource = BarCodeItems;
            barCodeList.SelectedIndex = -1;
        }
        //-------------------------------------------------------------------------
        private void InitButtonsAndControls() //все кнопки к первоначальным значениям
        {
            pictureBox1.Size = new System.Drawing.Size
                (ImagePanel.DisplayRectangle.Width, ImagePanel.DisplayRectangle.Height);
            RotateLeftButton.Enabled = false;
            RotateRightButton.Enabled = false;
            SizeFitButton.Enabled = false;
            SizeFullButton.Enabled = false;
            SizePlusButton.Enabled = false;
            SizeMinusButton.Enabled = false;

            OpenFilesDirButton.Enabled = false;
            button1.Enabled = false;

            RecognizeAllButton.Enabled = false;
            RecognizeBubblesButton.Enabled = false;
            StopRecognizeButton.Enabled = false;

            VerifyButton.Enabled = false;

            // BoxSheet.SelectedIndex = -1;
            if (rec != null)
                rec.BubbleItems.Clear();
            BarCodeItems.Clear();
            pnlBubbles.Controls.Clear();//BubblesAC.Clear();
            bac = new BubblesAreaControl();
            Refresh();
        }
        //-------------------------------------------------------------------------
        private void Init()
        {
            ImagePanel.AutoScroll = false;
            LoadSettings();
            editorTools = new Control[]
            { this.RotateLeftButton
            , this.RotateRightButton
            , this.btnRemoveNoise
            , this.rbtnRotate
            //, this.splitBtnRestore
            , this.rbtnClear
            , this.rbtnCut
            , this.btnInvert
            };

            FileInfo fi = new FileInfo(Path.Combine(OcrAppConfigDefaults.BaseTempFolder, "testUids.json"));
            if (fi.Exists)
            {
                JavaScriptSerializer js = new JavaScriptSerializer();
                //Dictionary<string, eDoctrinaOcrEd.TestUids.AreaSettings[]> temp;
                try
                {
                    //temp = js.Deserialize<Dictionary<string, eDoctrinaOcrEd.TestUids.AreaSettings[]>>("testUids.json");//testUids
                    string s = File.ReadAllText(Path.Combine(OcrAppConfigDefaults.BaseTempFolder, "testUids.json"));
                    testUids = js.Deserialize<TestUids>(s);
                    if (testUids == null)
                    {
                        testUids = new TestUids();
                        testUids.Test = new Dictionary<string, eDoctrinaOcrEd.TestUids.AreaSettings[]>();
                    }
                }
                catch (Exception)
                {
                    testUids = new TestUids();
                    //testUids.Test = new Dictionary<TestUids.TestUid, RegionsArea[]>();
                    testUids.Test = new Dictionary<string, eDoctrinaOcrEd.TestUids.AreaSettings[]>();
                }
            }
            else
            {
                testUids = new TestUids();
                testUids.Test = new Dictionary<string, eDoctrinaOcrEd.TestUids.AreaSettings[]>();
            }
            defaults = new OcrAppConfig();
            if (defaults.exception != null)
            {
                UpdateUI("Exception", defaults.exception.Message);
                this.Close();
                return;
            }
            defaults.DualControl = true;
            if (!VerifyDirectory(defaults.ManualConfigsFolder) || Directory.GetFiles(defaults.ManualConfigsFolder, "*.json").Length == 0)
            {
                string message = "Config files not found. The program will be closed.";
                log.LogMessage(message);
                UpdateUI("Exception", message);
                this.Close();
                return;
            }
            if (!string.IsNullOrEmpty(OcrAppConfigDefaults.BaseTempFolder))
            {
                VerifyDirectory(OcrAppConfig.TempEdFolder);
            }
            BoxSheet.Text = "Sheet identifier";
            BoxSheet.DropDownStyle = ComboBoxStyle.DropDownList;

            AddMiniatureControlsAndSheetIdentifiers();
            InitButtonsAndControls();
        }
        //-------------------------------------------------------------------------
        private void AddMiniatureControlsAndSheetIdentifiers()
        {
            var sheetIdentifiers = GetSheetIdentifiers(defaults.ManualConfigsFolder);
            for (int i = sheetIdentifiers.Count - 1; i > -1; i--)
            {
                MiniatureItems.Add(new MiniatureItem()
                {
                    Index = i,
                    Name = sheetIdentifiers[i],
                    SheetIdentifierName = sheetIdentifiers[i],
                    SheetIdentifierImagePath = "Miniatures/" + sheetIdentifiers[i] + ".jpg"
                });
            }
            BoxSheet.DataSource = MiniatureItems;
            BoxSheet.DisplayMember = "Name";
            BoxSheet.SelectedIndex = -1;
        }
        //-------------------------------------------------------------------------
        private List<string> GetSheetIdentifiers(string configsFolder)
        {
            List<string> SheetIdentifiers = new List<string>();
            if (Directory.Exists(configsFolder))
            {
                foreach (var fileName in Directory.GetFiles(configsFolder, "*.json"))
                {
                    SheetIdentifiers.Add(Path.GetFileNameWithoutExtension(fileName));
                }
            }
            return SheetIdentifiers;
        }
        #endregion
        //-------------------------------------------------------------------------
        #region Settings
        public OcrEditorSettings appSettings;
        private void LoadSettings()
        {
            try
            {
                defaults = new OcrAppConfig();
                appSettings = new OcrEditorSettings();
                appSettings.Load();
                if (!appSettings.SettingsExists)
                {
                    SaveSettings();
                }
                else
                {
                    this.WindowState = appSettings.Fields.WindowState;
                    this.Location = appSettings.Fields.WindowLocation;
                    if (appSettings.Fields.WindowSize != new Size())
                        this.Size = appSettings.Fields.WindowSize;
                    try
                    {
                        this.splitContainer1.SplitterDistance = appSettings.Fields.SplitterDistanceActions;
                    }
                    catch (Exception)
                    {
                        //this.splitContainer1.SplitterDistance = 328;
                    }
                    this.splitContainer2.SplitterDistance = appSettings.Fields.SplitterDistanceBubble;
                    this.splitContainer3.SplitterDistance = appSettings.Fields.SplitterDistanceMiniature;
                    this.splitContainer4.SplitterDistance = appSettings.Fields.SplitterDistanceLens;
                    this.DarknessManualySet.Checked = appSettings.Fields.DarknessManualySet;
                    this.nudPerCentBestBubble.Value = appSettings.Fields.NudPerCentBestBubble;
                    this.nudPerCentEmptyBubble.Value = appSettings.Fields.NudPerCentEmptyBubble;
                    try
                    {
                        this.nudZoom.Value = appSettings.Fields.NudZoomValue;
                    }
                    catch (Exception)
                    {
                        this.nudZoom.Value = 2;
                    }
                }
            }
            catch (Exception)
            {
                SaveSettings();
            }
        }
        //-------------------------------------------------------------------------
        private void SaveSettings()
        {
            if (appSettings == null) appSettings = new OcrEditorSettings();
            appSettings.Fields.WindowState = this.WindowState;
            appSettings.Fields.WindowLocation = this.Location;
            appSettings.Fields.WindowSize = this.Size;
            appSettings.Fields.SplitterDistanceActions = this.splitContainer1.SplitterDistance;
            appSettings.Fields.SplitterDistanceBubble = this.splitContainer2.SplitterDistance;
            appSettings.Fields.SplitterDistanceMiniature = this.splitContainer3.SplitterDistance;
            appSettings.Fields.SplitterDistanceLens = this.splitContainer4.SplitterDistance;
            appSettings.Fields.DarknessManualySet = this.DarknessManualySet.Checked;
            appSettings.Fields.NudPerCentBestBubble = this.nudPerCentBestBubble.Value;
            appSettings.Fields.NudPerCentEmptyBubble = this.nudPerCentEmptyBubble.Value;
            appSettings.Fields.NudZoomValue = this.nudZoom.Value;
            //defaults.appConfigDateTime appSettings.Fields.
            appSettings.Save();
        }
        #endregion
        //-------------------------------------------------------------------------
        #region Main events
        private void EditorForm_Load(object sender, EventArgs e)
        {
            ////watcher.Changed += new FileSystemEventHandler(watcher_Changed);
            //watcher.Changed += new FileSystemEventHandler(OnChanged);
            //watcher.Changed += new FileSystemEventHandler(OnChanged); // Add event handlers.
            //watcher.Created += new FileSystemEventHandler(OnChanged);
            //watcher.Deleted += new FileSystemEventHandler(OnChanged);

            ////watcher.Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, OcrAppConfig.TempEdFolder);
            //watcher.Path = OcrAppConfig.TempEdFolder;
            //watcher.Filter = "*.tiff";
            //watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size;
            ////OnChanged(null, null);
            //watcher.EnableRaisingEvents = true;//Begin watching.

            Recognize.CreateUpdatesJson(ProductVersion.ToString());//!!!!!!

            string[] appDirArr = Regex.Split(Environment.CurrentDirectory, "\\W+");
            if (appDirArr.Length > 0)
            {
                appDir = appDirArr[appDirArr.Length - 1];
            }

            BoxSheet.radioButton1.Visible = false;
            BoxSheet.radioButton1.Checked = false;
            BoxSheet.rbtnText.Visible = false;
            BoxSheet.rbtnText.Checked = false;
            BoxSheet.btnCheck.Height += 4;
            //log.LogMessage("Load");
            BoxSheet.comboBox1.Size = new Size(BoxSheet.radioButton1.Location.X
                + BoxSheet.radioButton1.Width - 10, BoxSheet.comboBox1.Size.Height);
            //log.LogMessage("Load");

            //Control p = pictureBox1 as Control;//не работает
            //p.KeyDown += new KeyEventHandler(pictureBox1_KeyDown);
            InitEventsAndBindings();
            Init();
            log.LogMessage("Load");
            if (string.IsNullOrEmpty(defaults.ManualInputFolder))
            {
                string message = "The eDoctrina OCR Editor can not be started\r\n because is not present \"ManualInputFolder\"";
                log.LogMessage(message);
                UpdateUI("Exception", message);
                Close();
            }
        }
        ////-------------------------------------------------------------------------
        //void OnChanged(object source, FileSystemEventArgs e)
        //{
        //    var filesInTempEd = utils.GetSupportedFilesFromDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, OcrAppConfig.TempEdFolder)
        //         , SearchOption.TopDirectoryOnly, true);
        //    if (filesInTempEd.Length >= minCountFilesInTempFolder)
        //        return;
        //    string mf = defaults.ManualErrorFolder;
        //    for (int i = filesInTempEd.Length; i < minCountFilesInTempFolder; i++)
        //    {
        //        Invoke(new MethodInvoker(delegate
        //        {
        //            string fileName = GetFileForRecognizeFromFolders("", mf);
        //        }));
        //    }
        //}
        ////-------------------------------------------------------------------------
        private void EditorForm_Shown(object sender, EventArgs e)
        {
            log.LogMessage("Shown");
            filesInQueueTimer.Start();
            timer1.Start();
        }
        //-------------------------------------------------------------------------
        private void EditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            log.LogMessage("Closing");
            SaveSettings();
            ioHelper.ReDelete();
            var serializer = new SerializerHelper();
            serializer.SaveToFile(testUids, Path.Combine(OcrAppConfigDefaults.BaseTempFolder, "testUids.json"), Encoding.Unicode);
            if (ioHelper.ReDeleteList.Count > 0)
            {
                string message = "ioHelper.ReDeleteList.Count = " + ioHelper.ReDeleteList.Count;
                log.LogMessage(message);
                new ErrorRestart(KeyProgram.eDoctrinaOcrEd).SendErrorMail(message);
            }
        }
        //-------------------------------------------------------------------------
        private void StatusLabel_TextChanged(object sender, EventArgs e)
        {
            if (rpf != null) rpf.LabelTextBox.Text = StatusLabel.Text;
            log.LogMessage(StatusLabel.Text);
        }
        //-------------------------------------------------------------------------
        private void DarknessManualySet_CheckedChanged(object sender, EventArgs e)
        {
            if (DarknessManualySet.Checked)
            {
                nudPerCentBestBubble.Enabled = true;
                nudPerCentEmptyBubble.Enabled = true;
            }
            else
            {
                nudPerCentBestBubble.Enabled = false;
                nudPerCentEmptyBubble.Enabled = false;
            }
        }
        //-------------------------------------------------------------------------
        private void nudPerCentEmptyBubble_ValueChanged(object sender, EventArgs e)
        {

        }

        private void nudPerCentBestBubble_ValueChanged(object sender, EventArgs e)
        {

        }
        #endregion
        //-------------------------------------------------------------------------
        #region Main Menu
        //-------------------------------------------------------------------------
        private void OpenFileMainMenuItem_Click(object sender, EventArgs e)
        {
            log.LogMessage("OpenFile MainMenuItem Click");
            openFileDialog1.Filter = OcrAppConfig.SupportedExtensions + "|" + OcrAppConfig.SupportedExtensions.Replace(",", ";");
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FramesAndBitmap fab = new FramesAndBitmap();
                fab.VerifyFileForMultipaging(openFileDialog1.FileName);
                if (fab.Exception != null)
                {
                    UpdateUI("Exception", fab.Exception.Message);
                    return;
                }
                string fileName = utils.GetFileForRecognize(openFileDialog1.FileName, OcrAppConfig.TempEdFolder);
                Working(fileName);
            }
        }
        //-------------------------------------------------------------------------
        private void OpenManualInputFolderMainMenuItem_Click(object sender, EventArgs e)
        {
            log.LogMessage("OpenManualInputFolder MainMenuItem Click");
            if (VerifyDirectory(defaults.ManualInputFolder))
                Process.Start(defaults.ManualInputFolder);
        }
        //-------------------------------------------------------------------------
        private void OpenProcessingFolderMainMenuItem_Click(object sender, EventArgs e)
        {
            log.LogMessage("OpenProcessingFolder MainMenuItem Click");
            if (VerifyDirectory(OcrAppConfig.TempEdFolder))
                Process.Start(OcrAppConfig.TempEdFolder);
        }
        //-------------------------------------------------------------------------
        private void OpenErrorsResultsFolderMainMenuItem_Click(object sender, EventArgs e)
        {
            log.LogMessage("OpenErrorsResultsFolder MainMenuItem Click");
            if (VerifyDirectory(defaults.ManualErrorFolder))//not need
                Process.Start(defaults.ManualErrorFolder);
            if (defaults.ManualNotConfidentFolder != defaults.ManualSuccessFolder && defaults.ManualNotConfidentFolder != defaults.ManualErrorFolder)
            {
                if (VerifyDirectory(defaults.ManualNotConfidentFolder))
                    Process.Start(defaults.ManualNotConfidentFolder);
            }
            if (VerifyDirectory(defaults.ManualNextProccessingFolder))
                Process.Start(defaults.ManualNextProccessingFolder);
        }
        //-------------------------------------------------------------------------
        private void OpenSuccessResultsFolderMainMenuItem_Click(object sender, EventArgs e)
        {
            log.LogMessage("OpenSuccessResultsFolder MainMenuItem Click");
            if (VerifyDirectory(defaults.ManualSuccessFolder))
                Process.Start(defaults.ManualSuccessFolder);
        }
        //-------------------------------------------------------------------------
        private void OpenAutoInputFolderMainMenuItem_Click(object sender, EventArgs e)
        {
            log.LogMessage("OpenAutoInputFolder MainMenuItem Click");
            if (VerifyDirectory(defaults.InputFolder))
                Process.Start(defaults.InputFolder);
        }
        //-------------------------------------------------------------------------
        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            log.LogMessage("Settings MainMenuItem Click");
            if (settingsForm1 == null)
            {
                settingsForm1 = new SettingsForm1();
                settingsForm1.Owner = this;
                settingsForm1.btnSave.Click += new EventHandler(btnSave_Click);
                settingsForm1.btnCancel.Click += new EventHandler(btnCancel_Click);
                settingsForm1.FormClosed += new FormClosedEventHandler(settingsForm1_FormClosed);
                settingsForm1.chbSheetId.Checked = appSettings.Fields.ChbSheetId;
                settingsForm1.chbTestId.Checked = appSettings.Fields.TestId;
                settingsForm1.chbDistrictId.Checked = appSettings.Fields.DistrictId;
                settingsForm1.chbAmoutOfQuestions.Checked = appSettings.Fields.AmoutOfQuestions;
                settingsForm1.chbIndexOfFirstQuestion.Checked = appSettings.Fields.IndexOfFirstQuestion;
                settingsForm1.chBxNotConfirm.Checked = appSettings.Fields.NotConfirm;
                settingsForm1.chBxRecAfterCut.Checked = appSettings.Fields.RecAfterCut;
                settingsForm1.chBxUsePrevTool.Checked = appSettings.Fields.UsePrevTool;
            }
            settingsForm1.Show();
        }
        //-------------------------------------------------------------------------
        private void btnSave_Click(object sender, EventArgs e)
        {
            appSettings.Fields.ChbSheetId = settingsForm1.chbSheetId.Checked;
            appSettings.Fields.TestId = settingsForm1.chbTestId.Checked;
            appSettings.Fields.DistrictId = settingsForm1.chbDistrictId.Checked;
            appSettings.Fields.AmoutOfQuestions = settingsForm1.chbAmoutOfQuestions.Checked;
            appSettings.Fields.IndexOfFirstQuestion = settingsForm1.chbIndexOfFirstQuestion.Checked;

            appSettings.Fields.NotConfirm = settingsForm1.chBxNotConfirm.Checked;
            appSettings.Fields.RecAfterCut = settingsForm1.chBxRecAfterCut.Checked;
            appSettings.Fields.UsePrevTool = settingsForm1.chBxUsePrevTool.Checked;
            if (!appSettings.Fields.UsePrevTool)
            {
                foreach (var item in editorTools)
                {
                    item.BackColor = SystemColors.Control;
                }
            }
            appSettings.Save();
            settingsForm1.Close();
        }
        //-------------------------------------------------------------------------
        private void btnCancel_Click(object sender, EventArgs e)
        {
            settingsForm1.Close();
        }
        //-------------------------------------------------------------------------
        private void settingsForm1_FormClosed(object sender, FormClosedEventArgs e)
        {
            settingsForm1.Dispose();
            settingsForm1 = null;
        }
        //-------------------------------------------------------------------------
        private void sendProblemToSupportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            log.LogMessage("SendProblemToSupport MainMenuItem Click");
            sendToSupport = false;
            SendToSupport();
        }
        //-------------------------------------------------------------------------
        private void SendToSupportMainMenuItem_Click(object sender, EventArgs e)
        {
            log.LogMessage("ReportTestPage MainMenuItem Click");
            sendToSupport = true;
            SendToSupport();
        }
        //-------------------------------------------------------------------------
        private void ReportTestPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (rec != null)
                {
                    if (!Directory.Exists(OcrAppConfig.LogsFolder))
                    {
                        Directory.CreateDirectory(OcrAppConfig.LogsFolder);
                    }
                    else
                    {
                        string[] filesForDel = Directory.GetFiles(OcrAppConfig.LogsFolder, "*.*", SearchOption.TopDirectoryOnly)
                        .Where(s => OcrAppConfig.SupportedExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
                        foreach (var item in filesForDel)
                        {
                            try
                            {
                                File.Delete(item);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    List<string> attachments = new List<string>();
                    var fileNameScreenShot = MakeScreenShot();
                    attachments.Add(fileNameScreenShot);
                    var fileName = Path.Combine(OcrAppConfig.TempEdFolder, Path.GetFileName(rec.FileName));
                    FileInfo fi = new FileInfo(fileName);
                    //string newFileName = "Report test page" + fi.Extension;
                    string newFileName = Path.Combine(OcrAppConfig.LogsFolder, "Report test page_" + fi.Name);
                    File.Copy(fileName, newFileName, true);
                    attachments.Add(newFileName);

                    SendMailAsync("", "help@edoctrina.org", attachments, "Test page received");//ifidakov@itera-research.com//
                    DeleteButton.PerformClick();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //-------------------------------------------------------------------------
        private void ExitMainMenuItem_Click(object sender, EventArgs e)
        {
            log.LogMessage("Exit MainMenuItem Click");
            Close();
        }
        #endregion
        //-------------------------------------------------------------------------
        #region Menu Buttons (first toolbar) !!! Пересмотреть когда доступны
        private void VerifyButton_Click(object sender, EventArgs e)
        {
            log.LogMessage("Verify Button Click");
            Status = StatusMessage.Verify;
            for (int i = 0; i < barCodeList.ControlList.Count; i++)
            {
                BarCodeListItemControl item = (BarCodeListItemControl)barCodeList.ControlList[i];
                item.btnCheck.PerformClick();
                var itm = barCodeList.DataSource[i];
                if (!itm.Verify)
                {
                    MessageBox.Show("Invalid value in " + item.Name, Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }
            }
            //if (BarCodeItems.Any(x => !x.Verify))
            //    return;
            if (VerifyDirectory(defaults.ManualSuccessFolder))
            {
                try
                {
                    if (rec.BubbleItems.Count == 0)
                    {
                        for (int i = 0; i < bac.CheckBoxArr.Length; i++)
                        {
                            BubbleItem bi = new BubbleItem();
                            Bubble b = (Bubble)bac.CheckBoxArr[i].Tag;
                            bi.Bubble = b;
                            bi.CheckedBubble = new CheckedBubble();
                            bi.CheckedBubble.isChecked = bac.CheckBoxArr[i].Checked;
                            rec.BubbleItems.Add(bi);
                        }
                    }
                    DrawBubbleItems(false);
                    bool emty = true;
                    for (int k = 0; k < rec.BubbleItems.Count; k++)
                    {
                        if (rec.BubbleItems[k].CheckedBubble.isChecked)
                        {
                            emty = false;
                            break;
                        }
                    }
                    if (emty)
                    {
                        if (MessageBox.Show("Are you sure you want verify this sheet? No answers selected."
                            , Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        {
                            Status = StatusMessage.NULL;
                            return;
                        }
                    }

                    rec.VerifySheetManual(BarCodeItems.ToList(), rec.BubbleItems.ToList());
                    allAreasNaturalSize = new Rectangle[0];
                }
                catch (Exception)
                {
                    allAreasNaturalSize = new Rectangle[0];
                    VerifyButton.Enabled = false;
                    NextButton.Enabled = true;
                    DeleteButton.Enabled = true;
                    Status = StatusMessage.NULL;
                    return;
                }
                lastSheetId = rec.SheetIdentifier;
                lastTestId = testId;
                lastAmoutOfQuestions = rec.AmoutOfQuestions.ToString();
                lastIndexOfQuestion = rec.IndexOfFirstQuestion.ToString();
                lastDistrictId = districtId;
                errList.Clear();
                lblErr.Visible = false;
                DeleteTempFiles(rec.FileName, rec.AuditFileName);
                if (linsForm != null)
                    linsForm_FormClosing(null, new FormClosingEventArgs(CloseReason.None, false));
            }
        }
        //-------------------------------------------------------------------------
        private void NextButton_Click(object sender, EventArgs e)
        {
            log.LogMessage("Next Proccessing Button Click");
            if (rec == null) return;
            if (linsForm != null)
                linsForm_FormClosing(null, new FormClosingEventArgs(CloseReason.None, false));
            if (rec.SheetIdentifier != null)// && defaults.NotSupportedSheets.IndexOf(rec.SheetIdentifier) < 0
                lastSheetId = rec.SheetIdentifier;
            if (backgroundWorker.IsBusy)
            {
                CancelBackgroundWorker();
                Status = StatusMessage.Next;
            }
            else
            {
                DeleteToNextProccessingFolder();
            }
            allAreasNaturalSize = new Rectangle[0];
            errList.Clear();
            VerifyErrList();
        }
        //-------------------------------------------------------------------------
        private void DeleteToNextProccessingFolder()
        {
            var fileName = Path.GetFileName(rec.FileName);
            var fileNameAudit = utils.GetFileAuditName(fileName);
            if (VerifyDirectory(defaults.ManualNextProccessingFolder))
            {
                try
                {
                    File.Copy(OcrAppConfig.TempEdFolder + fileName, defaults.ManualNextProccessingFolder + fileName, true);
                    //File.Copy(OcrAppConfig.TempEdFolder + fileNameAudit, defaults.ManualNextProccessingFolder + fileNameAudit, true);
                    DeleteTempFiles(OcrAppConfig.TempEdFolder + fileName, OcrAppConfig.TempEdFolder + fileNameAudit);
                    Status = StatusMessage.NULL;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }
        }
        //-------------------------------------------------------------------------
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            log.LogMessage("Delete Button Click");
            if (linsForm != null)
                linsForm_FormClosing(null, new FormClosingEventArgs(CloseReason.None, false));
            //linsForm.Close();DeleteButton.Focus();
            if (rec == null) return;
            allAreasNaturalSize = new Rectangle[0];
            var fn = Path.GetFileName(rec.FileName);
            var fna = utils.GetFileAuditName(fn);
            if (Status == StatusMessage.Delete
                || MessageBox.Show("Files\r\n" + fn + "\r\nand\r\n" + fna + "\r\nwill be removed to trash, proceed?"
                , Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                if (string.IsNullOrEmpty(rec.SheetIdentifier) && defaults.NotSupportedSheets.IndexOf(rec.SheetIdentifier) >= 0)
                    lastSheetId = rec.SheetIdentifier;
                Status = StatusMessage.Delete;
                DeleteTempFiles(Path.Combine(OcrAppConfig.TempEdFolder, fn), Path.Combine(OcrAppConfig.TempEdFolder, fna), true);
                errList.Clear();
                VerifyErrList();
                Status = StatusMessage.NULL;
            }
        }
        //-------------------------------------------------------------------------
        private void btnSetAside_Click(object sender, EventArgs e)
        {
            string dir = Path.Combine(OcrAppConfig.TempEdFolder, "Deferred");
            try
            {
                VerifyDirectory(dir);
                var fn = Path.GetFileName(rec.FileName);
                FileInfo fi = new FileInfo(fn);
                File.Copy(rec.FileName, Path.Combine(dir, fi.Name), true);
                var fna = utils.GetFileAuditName(fn);
                if (!File.Exists(Path.Combine(OcrAppConfig.TempEdFolder, fna)))
                {
                    rec.Audit.Save(Path.Combine(OcrAppConfig.TempEdFolder, fna));
                }
                fi = new FileInfo(fna);
                File.Copy(Path.Combine(OcrAppConfig.TempEdFolder, fna), Path.Combine(dir, fi.Name), true);
                Status = StatusMessage.Delete;
                DeleteTempFiles(Path.Combine(OcrAppConfig.TempEdFolder, fn), Path.Combine(OcrAppConfig.TempEdFolder, fna));
            }
            catch (Exception)
            {
            }
            if (rec == null)
            {
                pictureBox1.Image = null;
            }
            errList.Clear();
            VerifyErrList();
            allAreasNaturalSize = new Rectangle[0];
            Status = StatusMessage.NULL;
        }
        //-------------------------------------------------------------------------
        private void tssmiReturnDeferredItems_Click(object sender, EventArgs e)
        {
            string dir = Path.Combine(OcrAppConfig.TempEdFolder, "Deferred");
            string[] fnArr = Directory.GetFiles(dir, "*.*", SearchOption.TopDirectoryOnly);// utils.GetSupportedFilesFromDirectory(dir, SearchOption.TopDirectoryOnly);
            for (int i = 0; i < fnArr.Length; i++)
            {
                try
                {
                    FileInfo fi = new FileInfo(fnArr[i]);
                    File.Move(fnArr[i], Path.Combine(OcrAppConfig.TempEdFolder, fi.Name));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            Application.DoEvents();
            if (Directory.Exists(dir))
            {
                fnArr = Directory.GetFiles(dir, "*.*", SearchOption.TopDirectoryOnly);
                tsslSetAside.Text = fnArr.Length.ToString();
            }
            else
                tsslSetAside.Text = "0";
            fnArr = utils.GetSupportedFilesFromDirectory(defaults.ManualInputFolder, SearchOption.TopDirectoryOnly, false);
            var fnTempArr = utils.GetSupportedFilesFromDirectory(OcrAppConfig.TempEdFolder, SearchOption.TopDirectoryOnly, false);
            int count = fnArr.Length + fnTempArr.Length;
            //UpdateUI("filesInQueueStatusLabel", fnArr.Count().ToString());
            UpdateUI("filesInQueueStatusLabel", count.ToString());
        }
        //-------------------------------------------------------------------------
        private void btnDeferred_Click(object sender, EventArgs e)
        {
            tssmiReturnDeferredItems_Click(null, null);
        }
        //-------------------------------------------------------------------------
        private void tssbtnSetAside_Click(object sender, EventArgs e)
        {
            if (!tssbtnSetAside.DropDownButtonPressed)
            {
                btnSetAside_Click(null, null);
            }
        }
        //-------------------------------------------------------------------------
        private void CancelBackgroundWorker()
        {
            UpdateUI("StatusLabel", "Canceling...");
            cancelSource.Cancel();
            if (backgroundWorker.workerThread == null)
            {
                //backgroundWorker = new AbortableBackgroundWorker();
                //backgroundWorker.DoWork += DoWork;
                //backgroundWorker.RunWorkerCompleted += Completed;
                if (rpf != null)
                {
                    StopRecognizeButton.Enabled = false;
                    rpf.DialogResult = DialogResult.Cancel;
                    //rpf.Dispose();
                    //rpf = null;
                }
            }
            else
                backgroundWorker.Abort();//.CancelAsync();
            if (!isRotate)
                btnRestore_Click(null, null);
        }
        //-------------------------------------------------------------------------
        private void SetCancelSource()
        {
            if (cancelSource.IsCancellationRequested)
            {
                cancelSource = new System.Threading.CancellationTokenSource();
                if (rec != null) rec.SetCancellationToken(cancelSource.Token);
            }
        }
        //-------------------------------------------------------------------------
        private void DeleteTempFiles(string fn1, string fn2, bool toRecycleBin = false)
        {
            SetCancelSource();
            pictureBox1.Image = null;
            if (animatedTimer != null)
            {
                animatedTimer.StopAnimation();
                animatedTimer = null;
            }
            if (rec != null)
            {
                rec.Dispose();
                rec = null;
            }
            //try
            //{
            //    vb.FileIO.FileSystem.DeleteFile(fn1, vb.FileIO.UIOption.OnlyErrorDialogs, vb.FileIO.RecycleOption.SendToRecycleBin);
            //}
            //catch (Exception)
            //{
            //}
            ioHelper.DeleteFileExt(toRecycleBin, fn1);
            ioHelper.DeleteFileExt(toRecycleBin, fn2);
            InitButtonsAndControls();
            timer1.Start();
        }
        private IOHelper ioHelper = new IOHelper();
        #endregion
        //-------------------------------------------------------------------------
        #region Menu Buttons (answer sheet toolbar) !!! Пересмотреть когда доступны
        private void OpenFilesDirButton_Click(object sender, EventArgs e)
        {
            log.LogMessage("OpenFilesDir Button Click");
            try
            {
                if (string.IsNullOrEmpty(rec.Audit.archiveFileName))
                {
                    MessageBox.Show("Archive file name not found in audit file"
                        , Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }
                //Process.Start("explorer.exe", string.Format("/select,\"{0}\""
                //    , Path.Combine(rec.Audit.sourceFilePath, rec.Audit.sourceFileName)));
                string archiveFileName = rec.Audit.archiveFileName;
                File.Open(archiveFileName, FileMode.Open);
                Process.Start("explorer.exe", string.Format("/select,\"{0}\"", rec.Audit.archiveFileName));
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }
        //-------------------------------------------------------------------------
        private void RecognizeAllButton_Click(object sender, EventArgs e)
        {
            log.LogMessage("Recognize All Button Click");
            //rec.BarCodesPrompt = "";//???
            if (cbDoNotProcess.Checked)
            {
                cbDoNotProcess.Checked = false;
                return;
            }
            Status = StatusMessage.NULL;
            try
            {
                //if (rec.Bitmap.PixelFormat != PixelFormat.Format24bppRgb)
                //{
                Exception ex;
                if (!isRotate)
                    rec.Bitmap = recTools.NormalizeBitmap(rec.Bitmap, out ex);
                ShowImage();//true
                //}
                //SizeFitButton.PerformClick();
                //lastSheetId = rec.SheetIdentifier;
                rec.areas = new RegionsArea[0]; // дублирование кода, см. стр. 362 в Recognize.cs
                if (linsForm != null)
                    linsForm_FormClosing(null, new FormClosingEventArgs(CloseReason.None, false));
                //linsForm.Close();
                UncheckAllRbtn();
                errList.Clear();
                lblErr.Visible = false;
                RecognizeAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                RecognizeAllButton.Enabled = true;
            }
        }
        //-------------------------------------------------------------------------
        private void UncheckAllRbtn()
        {
            UncheckRbtn(null, null);
            rbtnGrid.Checked = false;
            rbtnRotate.Checked = false;
            rbtnCut.Checked = false;
            rbtnClear.Checked = false;
        }
        //-------------------------------------------------------------------------
        private void RecognizeBubblesButton_Click(object sender, EventArgs e)
        {
            log.LogMessage("Recognize Bubbles Button Click");
            BarCodeItem item = BarCodeItems.First(b => b.Name == "amout_of_questions");
            if (!item.Verify)
            {
                BarCodeListItemControl itm = (BarCodeListItemControl)barCodeList.ControlList[BarCodeItems.IndexOf(item)];
                itm.btnCheck.PerformClick();
                if (!item.Verify)
                    return;
            }
            Status = StatusMessage.NULL;
            lastSheetId = rec.SheetIdentifier;
            if (rec.SheetIdentifier == "FLEX")
            {
                item = BarCodeItems.First(b => b.Name == "bubbles_per_line");
                int index = Array.IndexOf(rec.allBarCodeNames, item.Name);
                //BarCodeListItemControl bc = barCodeList.ControlList[index] as BarCodeListItemControl;
                switch (item.Barcode)//bc.comboBox1.Text
                {
                    case "5":
                    case "6":
                        rec.bubbles_per_lineErr = false;
                        break;
                    default:
                        MessageBox.Show("Invalid value in " + item.Name, Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        break;
                }
            }
            if (linsForm != null)
                linsForm_FormClosing(null, new FormClosingEventArgs(CloseReason.None, false));
            UncheckAllRbtn();
            try
            {
                RecognizeBubbles();
                ShowProcessingForm();
            }
            catch (Exception)
            {
                RecognizeAllButton.Enabled = true;
            }
        }
        //-------------------------------------------------------------------------
        private void StopRecognizeButton_Click(object sender, EventArgs e)
        {
            log.LogMessage("Cancel Recognize Button Click");
            CancelBackgroundWorker();
        }
        //-------------------------------------------------------------------------
        private void SizeFitButton_Click(object sender, EventArgs e)
        {
            Zoom = (((double)ImagePanel.Width / pictureBox1.Image.Width) < ((double)ImagePanel.Height / pictureBox1.Image.Height)) ?
                ((double)ImagePanel.Width / pictureBox1.Image.Width) : ((double)ImagePanel.Height / pictureBox1.Image.Height);
            SetZoom(Zoom);
            //SetScroll();
        }
        //-------------------------------------------------------------------------
        private void SizeFullButton_Click(object sender, EventArgs e)
        {
            SetZoom(1.0);
        }
        //-------------------------------------------------------------------------
        private void SizePlusButton_Click(object sender, EventArgs e)
        {
            SetZoom(Zoom * 1.2);
        }
        //-------------------------------------------------------------------------
        private void SizeMinusButton_Click(object sender, EventArgs e)
        {
            SetZoom(Zoom * 0.8);
        }
        //-------------------------------------------------------------------------
        private void RotateLeftButton_Click(object sender, EventArgs e)
        {
            log.LogMessage("Rotate Left Button Click");
            Rotate(RotateFlipType.Rotate270FlipNone);
            bmpRotate = (bmpRotate + 270) % 360;
            isRotate = true;
            rbtnRotate_Click(sender, e);
        }
        //-------------------------------------------------------------------------
        private void RotateRightButton_Click(object sender, EventArgs e)
        {
            log.LogMessage("Rotate Right Button Click");
            Rotate(RotateFlipType.Rotate90FlipNone);
            bmpRotate = (bmpRotate + 90) % 360;
            isRotate = true;
            rbtnRotate_Click(sender, e);
        }
        #endregion
        //-------------------------------------------------------------------------
        #region  Menu Buttons (bubble area toolbar)
        private void ClearAllCheckMarksButton_Click(object sender, EventArgs e)
        {
            area = Rectangle.Empty;
            areaNaturalSize = Rectangle.Empty;
            if (bac.CheckBoxArr != null)
            {
                bac.lockReport = true;
                for (int i = 0; i < bac.CheckBoxArr.Length; i++)
                {
                    bac.CheckBoxArr[i].Checked = false;
                    rec.BubbleItems[i].CheckedBubble.isChecked = false;
                    DrawBubble(rec.BubbleItems[i].CheckedBubble);
                }
                bac.lockReport = false;
                pictureBox1.Refresh();
            }
        }
        #endregion
        //-------------------------------------------------------------------------
        #region Zoom Rotate
        private double Zoom = 1.0;
        private int bmpRotate = 0;

        private void SetZoom(double value)
        {
            Zoom = value;
            pictureBox1.Width = (int)(pictureBox1.Image.Width * Zoom);
            pictureBox1.Height = (int)(pictureBox1.Image.Height * Zoom);
            SetScroll();
        }
        //-------------------------------------------------------------------------
        private void Rotate(RotateFlipType rotateFlipType)
        {
            if (rec != null)
            {
                rec.Rotate(rotateFlipType);
                ShowImage();
                if (BoxSheet.SelectedIndex == -1)
                {
                    BoxSheet.Focus();
                }
            }
        }
        #endregion
        //-------------------------------------------------------------------------
        #region cBoxSheet Events
        private void cBoxSheet_MouseMove(object sender, MouseEventArgs e)
        {
            //привязать к штрихкоду и анимировать...
        }

        private void BoxSheet_OkClick(object sender, EventArgs e)
        {

        }
        //-------------------------------------------------------------------------
        private void BoxSheet_SelectedValueChanged(object sender, EventArgs e)
        {
            if (BoxSheet.SelectedIndex == -1)
            {
                foreach (MiniatureListItemControl item in miniatureList.ControlList)
                    item.BackColor = SystemColors.Control;
                return;
            }
            if (Array.IndexOf(defaults.NotSupportedSheets.ToArray(), BoxSheet.SelectedItem.ToString()) > -1)
            {
                MessageBox.Show("Not supported sheet", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (backgroundWorker.IsBusy) return;
            if (linsForm != null)
                linsForm_FormClosing(null, new FormClosingEventArgs(CloseReason.None, false));

            //rec.SheetIdentifier = BoxSheet.SelectedItem.ToString();
            log.LogMessage("Sheet identifier: " + BoxSheet.SelectedItem.ToString());
            if (rec != null && (rec.Status == RecognizeAction.WaitingForUserResponse
                || rec.Status == RecognizeAction.Created//|| rec.Status == RecognizeAction.SearchBublesFinished
                || rec.Status == RecognizeAction.Cancelled))
            {
                rec.SheetIdentifier = BoxSheet.SelectedItem.ToString();
                btnGrid.Enabled = BoxSheet.SelectedIndex != -1 && !string.IsNullOrEmpty(rec.SheetIdentifier);
                rec.regions = recTools.GetRegions(rec.SheetIdentifier, rec.regionsList);
                rec.LastSheetIdentifier = rec.SheetIdentifier;

                var selectedItem = MiniatureItems.First(x => x.Name == rec.SheetIdentifier);
                if (selectedItem != null)
                    miniatureList.SelectedItem = selectedItem;

                if (rec.linesPerArea != null)
                    rec.maxAmoutOfQuestions = rec.linesPerArea.Sum();
                errList.Clear();
                lblErr.Visible = false;
                ShowImage(false);
                if (ShetIdManualySet)
                    RecognizeAll(true);
                else
                    RecognizeAll(false);
            }
        }
        #endregion
        //-------------------------------------------------------------------------
        #region ListControl Events
        private void MiniatureSelectedItemChanged(object sender, EventArgs e)
        {

        }
        //-------------------------------------------------------------------------
        private void BarCodeSelectedItemChanged(object sender, EventArgs e)
        {

        }
        //private void barCodeList_BarCodeMouseClick(object sender, EventArgs e)
        //{
        //    var item = sender as BarCodeListItemControl;
        //    if (item == null) return;
        //    try
        //    {
        //        barCodeList.SelectedControl = item;
        //        MouseEnterBarCode(item.Item);
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}
        //-------------------------------------------------------------------------
        private void barCodeList_BarCodeMouseClick(object sender, EventArgs e)
        {
            var item = sender as BarCodeListItemControl;
            if (item == null) return;
            try
            {
                if (SelectedBarCodeItem != null)
                    return;
                barCodeList.SelectedControl = item;
                MouseEnterBarCode(item.Item);
                //if (e != null)//дублируется в "BarCodeListItemControl_Enter"
                //    if (item.Item.Rectangle.Width > item.Item.Rectangle.Height)
                //        UpdateZoomedImage(item.Item.Rectangle.X + item.Item.Rectangle.Width / 4, item.Item.Rectangle.Y);
                //    else
                //        UpdateZoomedImage(item.Item.Rectangle.X, item.Item.Rectangle.Bottom - item.Item.Rectangle.Height / 4);
            }
            catch (Exception)
            {
            }
        }
        //-------------------------------------------------------------------------
        private void barCodeList_OkButtonClick(object sender, EventArgs e)
        {
            var obj = sender as BarCodeListItemControl;
            if (obj == null) return;
            VerifyBarCode(obj.Item);
            if (obj.Item.Verify)
                obj.comboBox1.ForeColor = Color.Black;
            if (obj.Item.Name == "amout_of_questions" && utils.IsNumeric(obj.Item.Value))
                SetAmoutOfQuestions(Convert.ToInt32(obj.Item.Value));
            else if (obj.Item.Name == "test_id" && utils.IsNumeric(obj.Item.Value))
                testId = obj.Item.Value;
            else if (obj.Item.Name == "district_id" && utils.IsNumeric(obj.Item.Value))
                districtId = obj.Item.Value;

            else if ((obj.Item.Name == "question_number_1"
                || obj.Item.Name == "index_of_first_question") && utils.IsNumeric(obj.Item.Value))
                indexOfQuestion = obj.Item.Value;
            else if (obj.Item.Name == "bubbles_per_line" && utils.IsNumeric(obj.Item.Value))
            {
                if (Status == StatusMessage.Verify || Status == StatusMessage.Delete)
                    return;
                switch (obj.Item.Value)
                {
                    case "5":
                    case "6":
                        rec.bubbles_per_lineFLEX = Convert.ToInt32(obj.Item.Value);
                        rec.bubbles_per_lineErr = false;
                        switch (rec.bubbles_per_lineFLEX)
                        {
                            case 5:
                            case 6:
                                foreach (var item in rec.regionsListFLEX)
                                {
                                    if (item.regions[item.regions.Length - 1].areas[0].bubblesPerLine == rec.bubbles_per_lineFLEX)
                                    {
                                        rec.regions = item;
                                        break;
                                    }
                                }
                                break;
                                //default:
                                //    bItem.Value = "";
                                //    break;
                        }
                        rec.bubblesPerLine[0] = Convert.ToInt32(obj.Item.Value);
                        rec.areas[0].bubblesPerLine = rec.bubblesPerLine[0];
                        bac = CreateNewBubblesAreaControl(rec.areas, rec.AmoutOfQuestions);
                        break;
                    default:
                        rec.bubbles_per_lineErr = true;
                        obj.comboBox1.ForeColor = Color.Red;
                        MessageBox.Show("Invalid value in " + obj.Item.Name, Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        break;
                }
            }
            //if (obj.Item.Name.StartsWith("question_number"))
            //{

            //}
            if (obj.Item.Name == "amout_of_questions"
                || obj.Item.Name == "test_id"
                || obj.Item.Name == "index_of_first_question"
                || obj.Item.Name == "question_number_1")
            {
                SetFactAreasSetting();
            }
        }
        //-------------------------------------------------------------------------
        private void VerifyBarCode() //for all barCodes
        {
            foreach (var item in BarCodeItems)
                VerifyBarCode(item);
        }
        //-------------------------------------------------------------------------
        private void VerifyBarCode(BarCodeItem item) //for custom barcodes
        {
            if (String.IsNullOrEmpty(item.Value))
            {
                item.Verify = false;
                MouseEnterBarCode(item);
                return;
            }
            int i = 0;
            bool isNumeric = true;
            if (item.TextType == "numbersText")
                isNumeric = utils.IsNumeric(item.Value, out i);
            //else
            //    isNumeric = utils.IsNumeric(item.Value);
            if (item.TextType == "numbersText" && !isNumeric)
            {
                item.Verify = false;
                UpdateUI("Exception", "Invalid value in " + item.Name + "!");
                return;
            }
            else
                item.Verify = true;
            if (errList.Contains("Error in " + item.Name))
            {
                errList.Remove("Error in " + item.Name);
                VerifyErrList();
            }
            if (Status == StatusMessage.Verify || Status == StatusMessage.Delete)
                return;
            item.Value = item.Value.Trim();
            if (item.Name.StartsWith("question_number"))
            {
                string number = item.Name;
                number = number.Remove(0, number.LastIndexOf("_") + 1);
                int index = Convert.ToInt32(number);
                index--;
                rec.questionNumbers[index] = Convert.ToInt32(item.Value);
                //BarCodeListItemControl b = barCodeList.ControlList[index] as BarCodeListItemControl;
                //rec.lineNumbers[index]=
                if (bac != null)
                    bac.indexOfFirstQuestion = 1;
                //BarCodeListItemControl b = barCodeList.ControlList[item.Name] as BarCodeListItemControl;
            }
            switch (item.Name)
            {
                case "district_id":
                    lastDistrictId = item.Value;
                    break;
                case "amout_of_questions":
                    bool largerValue = false;
                    //if (regions == null || rec.maxAmoutOfQuestions == 0)
                    //    regions = rec.regions;
                    if (i > rec.MaxAmoutOfQuestions)
                    {
                        UpdateUI("Exception", "Value is larger than allowed!");//MessageBoxIcon.Error
                        largerValue = true;
                        item.Value = rec.MaxAmoutOfQuestions.ToString();
                        amoutOfQuestions = rec.MaxAmoutOfQuestions.ToString();
                        rec.AmoutOfQuestions = rec.MaxAmoutOfQuestions;
                    }
                    else
                    {
                        rec.AmoutOfQuestions = i;
                        rec.allBarCodeValues[amout_of_questionsIndex] = i.ToString();
                        amoutOfQuestions = i.ToString();
                    }
                    if (linsForm == null)
                        CreateLinsForm();
                    else
                        InitLinsForm(false);
                    if (rec.AmoutOfQuestions <= factLinesPerArea[0])
                    {//пока только для 2 - х полей
                        if (factLinesPerArea.Length > 1)
                            factLinesPerArea[1] = 0;
                    }
                    else if (factLinesPerArea.Length > 1)
                    {
                        factLinesPerArea[1] = rec.AmoutOfQuestions - factLinesPerArea[0];
                    }
                    if (bac.CheckBoxArr.Length == 0)
                    {
                        GetAreasSettings();
                        //if (rec.areas.Length == 0)
                        //{
                        rec.areas = bubblesRegions.areas;
                        //}
                        maxCountRectangles = rec.AddMaxCountRectangles();
                        rec.FillBubbleItems(maxCountRectangles);//
                        if (rec.factRectangle.Length > 0)
                        {
                            rec.FillBubbleItemsRectangle(rec.allContourMultiLine, rec.factRectangle);
                            rec.FindBubble(rec.factRectangle, rec.allContourMultiLine, true);
                        }
                        //rec.BubbleItems = rec.rec.BubbleItems;
                        bac = CreateNewBubblesAreaControl(rec.areas, rec.AmoutOfQuestions);
                        bac.lockReport = true;
                        DrawBubbleItems();
                        bac.lockReport = false;
                        VerifyButton.Enabled = true;
                    }
                    else
                    {
                        if (rec.AmoutOfQuestions.ToString() != item.Value)
                        {
                            GetAreasSettings();
                            InitLinsForm(false);
                        }
                    }
                    if (rec.Status == RecognizeAction.SearchBublesFinished || rec.Status == RecognizeAction.Cancelled)//
                    {
                        if (rec.Status == RecognizeAction.Cancelled && bac == new BubblesAreaControl())
                            bac = CreateNewBubblesAreaControl(rec.areas, rec.AmoutOfQuestions);
                        if (rec.AmoutOfQuestions > i)
                        {
                            if (Status != StatusMessage.Verify && Status != StatusMessage.Delete)
                            {
                                ShowImage(false);
                                rec.AmoutOfQuestions = i;
                                bac.AmoutOfQuestions = i;
                                DrawBubbleItems();
                                pictureBox1.Refresh();
                            }
                        }
                        else
                        {
                            if (rec.AmoutOfQuestions == i || rec.allBarCodeValues[amout_of_questionsIndex] == i.ToString())
                            {
                                if (bac != null && bac != new BubblesAreaControl() && bac.labelArr.Length == i)
                                    return;
                            }
                            rec.AmoutOfQuestions = i;
                            if (linsForm != null)
                                if (linsForm.Visible)
                                    return;
                            rec.Status = RecognizeAction.SearchBubles;
                            RecognizeBubblesButton.Enabled = true;
                            RecognizeBubblesButton.PerformClick();
                        }
                        if (largerValue) return;
                    }
                    else
                    {
                        Status = StatusMessage.ChangeAmoutOfQuestions;
                        return;
                    }
                    break;
                case "question_number_1":
                case "index_of_first_question":
                    rec.IndexOfFirstQuestion = i;
                    bac.indexOfFirstQuestion = i;
                    break;
                //if (rec.Status == RecognizeAction.SearchBublesFinished)
                //{
                //rec.IndexOfFirstQuestion = i;
                //bac.indexOfFirstQuestion = i;
                //}
                //else
                //{
                //    Status = StatusMessage.ChangeIndexOfFirstQuestion;
                //    return;
                //}
                //break;
                default:
                    break;
            }

            rec.SetValueManual(item);   //f.btnExport.Focus();
        }
        //-------------------------------------------------------------------------
        private void MouseEnterBarCode(BarCodeItem item)
        {
            barCodeList.SelectedItem = item;
            if (linsForm != null)
                return;
            MoveScrollBubble(item.Rectangle);
            //UpdateZoomedImage(item.Rectangle.X, item.Rectangle.Y);
            AnimatedBarCode(item);
        }
        //-------------------------------------------------------------------------
        private void MoveScrollBubble(Rectangle rectangle)
        {
            try
            {
                int x = (int)Math.Round(rectangle.X * Zoom) + ImagePanel.AutoScrollPosition.X;
                int y = (int)Math.Round(rectangle.Y * Zoom) + ImagePanel.AutoScrollPosition.Y;
                pnlScrollBubble.Visible = false;
                pnlScrollBubble.BringToFront();
                pnlScrollBubble.Width = (int)Math.Round(rectangle.Width * Zoom);
                pnlScrollBubble.Height = (int)Math.Round(rectangle.Height * Zoom);
                pnlScrollBubble.Location = new Point(x, y);
                ImagePanel.ScrollControlIntoView(pnlScrollBubble);
            }
            catch (Exception) { }
        }
        #endregion
        //-------------------------------------------------------------------------
        #region BubblesAreaControl Events
        private void BubblesAreaControl2_BubbleMouseEnter(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            splitContainer4.Panel1.ScrollControlIntoView(cb);
            pnlBubbles.ScrollControlIntoView(cb);
            var item = (BubbleItem)cb.Tag;
            MoveScrollBubble(item.CheckedBubble.rectangle);
            AnimatedBubble(item);
            pictureBox1_Paint(null, null);
        }
        #endregion
        //-------------------------------------------------------------------------
        #region pictureBox1 Events
        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            Text = Text = countAllFraves.ToString() + " " + appDir + appTitle;
            pictureBox1.Cursor = Cursors.Default;
            UpdateZoomedImage(0, 0); //pictureBox2.Image = null;
            nudZoom.Visible = true;
        }
        //-------------------------------------------------------------------------
        public void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                int x = (int)Math.Round(e.X / Zoom);
                int y = (int)Math.Round(e.Y / Zoom);
                Point begPointZoom = new Point((int)Math.Round(begPoint.X / Zoom), (int)Math.Round(begPoint.Y / Zoom));
                if (rec != null && rec.Bitmap != null)
                {
                    if (rbtnCut.Checked)
                    {
                        if (!appSettings.Fields.NotConfirm)
                        {
                            if (MessageBox.Show("Cut selected area?"
                                 , Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information) != DialogResult.Yes
                                 )
                            {
                                return;
                            }
                        }
                        RecognizeBubblesButton.Enabled = false;
                        Bitmap bmp = new Bitmap(areaNaturalSize.Width, areaNaturalSize.Height, PixelFormat.Format24bppRgb);
                        bmp = recTools.CopyBitmap(rec.Bitmap, areaNaturalSize);
                        //bmp.Save("Cut.bmp", ImageFormat.Bmp);
                        //if (bmp.Width > bmp.Height)
                        //{
                        //    bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        //}
                        rec.Bitmap = (Bitmap)bmp.Clone();
                        bmp.Dispose();
                        ShowImage(true);
                        isCut = true;
                        //isRotate = true;
                        if (appSettings.Fields.RecAfterCut)
                        {
                            ShetIdManualySet = false;
                            RecognizeAllButton.PerformClick();
                        }
                        return;
                    }
                    else if (rbtnClear.Checked)
                    {
                        for (int x1 = areaNaturalSize.X; x1 < areaNaturalSize.Right; x1++)
                        {
                            for (int y1 = areaNaturalSize.Y; y1 < areaNaturalSize.Bottom; y1++)
                            {
                                rec.Bitmap.SetPixel(x1, y1, Color.FromArgb(255, 255, 255));
                            }
                        }

                        //rec.Bitmap.Save("recClear.bmp", ImageFormat.Bmp);
                        isClear = true;
                        ShowImage(true);
                        return;
                    }
                    if (SelectedBarCodeItem != null)
                    {
                        if (areaNaturalSize.Width > pictureBox1.Image.Width / 3 || areaNaturalSize.Height > pictureBox1.Image.Height / 3)
                        {
                            messageBoxShow = true;
                            MessageBox.Show("Select more precisely the area of recognition"
                                , Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            return;
                        }
                        bool notConfident = false;
                        string barCodesPrompt = "";
                        string barcodeMem = "";
                        eDoctrinaUtils.Region region = new eDoctrinaUtils.Region();

                        foreach (var item in rec.regionsList)
                        {
                            if (item.SheetIdentifierName == rec.SheetIdentifier)
                            {
                                foreach (var itm in item.regions)
                                {
                                    if (itm.name == SelectedBarCodeItem.Text)
                                    {
                                        region = itm;
                                        break;
                                    }
                                }
                                if (!region.Equals(new eDoctrinaUtils.Region()))
                                    break;
                            }
                        }
                        string barcode = "";
                        if (SelectedBarCodeItem.radioButton1.Checked)
                        {
                            barcode = recTools.GetBarCode
                           (
                             rec.Bitmap
                           , ref notConfident
                           , ref barCodesPrompt
                           , ref rec.filterType
                           , ref barcodeMem
                           , areaNaturalSize.X
                           , areaNaturalSize.X + areaNaturalSize.Width
                           , areaNaturalSize.Y
                           , areaNaturalSize.Y + areaNaturalSize.Height
                           , rec.kx
                           , rec.ky
                           , curRect
                           , etRect
                           , deltaY
                           , region
                           , true
                           , region.percent_confident_text_region
                           , rec.defaults.PercentConfidentText
                           , rec.defaults.FontName
                           , ref currentBarCodeRectangle
                           , ref lastSymbolRectangle
                           , false
                           , true
                           );
                        }
                        else
                        {
                            Rectangle r2 = Rectangle.Empty;
                            Bitmap bmp = recTools.CopyBitmap(rec.Bitmap, areaNaturalSize);
                            if (rec.regions != null && rec.regions.heightAndWidthRatio < 1 && region.rotate == 90)
                                bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            recTools.TextRecognizeExt
                               (
                                 rec.filterType
                               , region.percent_confident_text_region
                               , rec.DarknessPercent//percent_confident_text
                               , defaults.FontName
                               , ref barcode
                               , ref lastSymbolRectangle
                               , ref region.areas[1]
                               , false//filter
                               , ref r2
                               , ref bmp
                               , ""
                               , 0
                               , null
                               , true
                               );
                        }

                        if (!string.IsNullOrEmpty(barcode))
                        {
                            SelectedBarCodeItem.comboBox1.Text = barcode;
                            SelectedBarCodeItem.textChanged = true;
                            if (SelectedBarCodeItem.Name == "amout_of_questions")
                            {
                                if (linsForm == null)
                                    CreateLinsForm();

                                int amout_of_questions = -1;
                                try
                                {
                                    amout_of_questions = Convert.ToInt32(barcode);
                                }
                                catch (Exception)
                                {
                                }
                                SetAmoutOfQuestions(amout_of_questions);
                                RecognizeAllButton.Enabled = true;
                                RecognizeBubblesButton.Enabled = true;
                                SelectedBarCodeItem.btnCheck.PerformClick();
                            }
                            SelectedBarCodeItem.textChanged = true;
                            //SelectedBarCodeItem.btnCheck.Focus();
                            SelectedBarCodeItem.BarCodeListItemControl_Leave(SelectedBarCodeItem, e);
                            pictureBox1.Cursor = Cursors.Default;
                        }
                        else
                        {
                            if (areaNaturalSize.Width * areaNaturalSize.Height < 100)
                                return;
                            if (SelectedBarCodeItem.radioButton1.Checked)
                                MessageBox.Show("Bar code is not recognized", Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            else
                                MessageBox.Show("Text is not recognized", Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        }
                        return;
                    }

                    if (rbtnRotate.Checked)
                    {
                        if (Math.Abs(x - begPointZoom.X) + Math.Abs(y - begPointZoom.Y) < 200)
                            return;
                        double angle = 0;
                        if (Math.Abs(begPointZoom.X - x) > Math.Abs(begPointZoom.Y - y))
                        {
                            if (begPointZoom.X < x)
                                angle = -180 + recTools.GetAngle2(begPointZoom, new Point(x, y));
                            else
                                angle = -180 + recTools.GetAngle2(new Point(x, y), begPointZoom);
                        }
                        else
                        {
                            if (begPointZoom.Y < y)
                                angle = -90 + recTools.GetAngle2(begPointZoom, new Point(x, y));
                            else
                                angle = -90 + recTools.GetAngle2(new Point(x, y), begPointZoom);
                        }
                        Bitmap b1 = new Bitmap(rec.Bitmap.Width, rec.Bitmap.Height, PixelFormat.Format24bppRgb);
                        Graphics g2 = Graphics.FromImage(b1);
                        Color argbWhite = Color.FromArgb(255, 255, 255);
                        g2.Clear(argbWhite);
                        g2.InterpolationMode = InterpolationMode.NearestNeighbor;
                        using (Matrix m2 = new Matrix())
                        {
                            m2.RotateAt((float)angle, new System.Drawing.Point(rec.Bitmap.Width / 2, rec.Bitmap.Height / 2));
                            g2.Transform = m2;
                            try
                            {
                                g2.DrawImageUnscaledAndClipped(rec.Bitmap, new Rectangle(0, 0, rec.Bitmap.Width, rec.Bitmap.Height));
                            }
                            catch { }
                            g2.ResetTransform();
                        }
                        rec.Bitmap = (Bitmap)b1.Clone();

                        //bmp.Save("Alignment.bmp", ImageFormat.Bmp);

                        b1.Dispose();
                        g2.Dispose();
                        pictureBox1.Image = (Bitmap)rec.Bitmap.Clone();//new Bitmap(rec.Bitmap);
                        //SizeFitButton.PerformClick();
                        Refresh();
                        isRotate = true;
                        if (appSettings.Fields.RecAfterCut)
                        {
                            if (appSettings.Fields.RecAfterCut)// && rec.BarCodesPrompt == ""
                            {
                                if (BarCodeItems != null && BarCodeItems.Count > 0)
                                {
                                    bool verify = true;
                                    for (int j = 0; j < BarCodeItems.Count; j++)
                                    {
                                        BarCodeItem b = BarCodeItems[j] as BarCodeItem;
                                        if (!b.Verify)
                                        {
                                            verify = false;
                                            break;
                                        }
                                    }
                                    if (verify)
                                    {
                                        RecognizeBubblesButton.PerformClick();
                                    }
                                    else
                                        RecognizeAllButton.PerformClick();
                                }
                            }
                        }
                        return;
                    }
                    else if (rbtnGrid.Checked)//linsForm.rbtnGrid.Checked
                    {
                        if (linsForm == null || linsForm.IsDisposed)
                            CreateLinsForm();
                        if (rec.SheetIdentifier != "FANDP")
                            linsForm.Visible = true;// linsForm.Show();
                                                    //linsFormIsVisible = true;
                        if (areaNaturalSize.Left < 0)
                        {
                            areaNaturalSize.Width -= areaNaturalSize.Left;
                            areaNaturalSize.X = 0;
                        }
                        if (areaNaturalSize.Top < 0)
                        {
                            areaNaturalSize.Height -= areaNaturalSize.Y;
                            areaNaturalSize.Y = 0;
                        }
                        if (areaNaturalSize.Left + areaNaturalSize.Width > rec.Bitmap.Width)
                            areaNaturalSize.Width = rec.Bitmap.Width - areaNaturalSize.Left;

                        if (areaNaturalSize.Width * areaNaturalSize.Height < 500)
                        {
                            DrawAllAreasNaturalSize();
                            return;
                        }
                        if (etalonAreas.Length < 3)
                        {
                            if (areaNaturalSize.X < pictureBox1.Image.Width / 4)//+ areaNaturalSize.Width / 2
                            {//пока только для двух полей!!!
                                linsForm.nudArea.Value = 1;
                            }
                            else
                            {
                                if (rec.AmoutOfQuestions != 0 && rec.AmoutOfQuestions <= factLinesPerArea[0])
                                {
                                    if (factLinesPerArea.Length > 1)
                                    {
                                        factLinesPerArea[1] = 0;
                                        linsForm.nudArea.Value = 2;
                                        Application.DoEvents();
                                        linsForm.nudRows.Value = 0;
                                        if (MessageBox.Show("Are you sure you want to select the first area here?"
                                        , Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                        {
                                            linsForm.nudArea.Value = 1;
                                        }
                                        else
                                        {
                                            pictureBox1.Refresh();
                                            return;
                                        }
                                    }
                                }
                                else
                                    linsForm.nudArea.Value = 2;
                            }
                        }
                        factAreas[(int)linsForm.nudArea.Value - 1].left = areaNaturalSize.Left;// (int)(area.Left / Zoom);
                        factAreas[(int)linsForm.nudArea.Value - 1].top = areaNaturalSize.Top;// (int)(area.Top / Zoom);
                        factAreas[(int)linsForm.nudArea.Value - 1].height = areaNaturalSize.Height;// (int)(area.Height / Zoom);
                        factAreas[(int)linsForm.nudArea.Value - 1].width = areaNaturalSize.Width;// (int)(area.Width / Zoom);
                        allAreasNaturalSize[(int)linsForm.nudArea.Value - 1] = areaNaturalSize;
                        factAreasManualSet[(int)linsForm.nudArea.Value - 1] = true;
                        double kx = 1;
                        double ky = 1;
                        int deltaX = 0, deltaY = 0;
                        int manualSelectedArea = -1;
                        int questions = 0;
                        for (int i = 0; i < factAreas.Length; i++)
                        {
                            if (!factAreasManualSet[i])
                                continue;
                            else
                            {
                                double EtStepX = (double)
                                (etalonAreas[i].width - etalonAreas[i].bubble.Width
                                * bubblesRegions.areas[i].bubblesPerLine)
                                / (bubblesRegions.areas[i].bubblesPerLine - 1);
                                int widthEt = (int)(bubblesRegions.areas[i].bubble.Width
                                    * factAreas[i].bubblesPerLine + EtStepX * (factAreas[i].bubblesPerLine - 1));//(int)linsForm.nudCols.Value - 1);
                                kx = (double)factAreas[i].width / widthEt;
                                rec.kx = (decimal)kx;
                                int maxBubblesCountOfArea = factLinesPerArea[i];
                                //(int)Math.Round((double)bubblesRegions.areas[i].height / bubblesRegions.areas[i].lineHeight);
                                double EtStepY = (double)(bubblesRegions.areas[i].height
                                 - bubblesRegions.areas[i].bubble.Height * maxBubblesCountOfArea) / (maxBubblesCountOfArea - 1);
                                double heightEt = bubblesRegions.areas[i].bubble.Height * factLinesPerArea[i]
                                   + EtStepY * (factLinesPerArea[i] - 1)
                                   + (bubblesRegions.areas[i].subLineHeight * (factAreas[i].subLinesAmount));
                                ky = (double)factAreas[i].height / heightEt;
                                rec.ky = (decimal)ky;
                                deltaX = (int)Math.Round(factAreas[i].left * kx) - etalonAreas[i].left;
                                deltaY = (int)Math.Round(factAreas[i].top * ky) - etalonAreas[i].top;
                                curRect = new Rectangle(bubblesRegions.areas[i].left, bubblesRegions.areas[i].top, bubblesRegions.areas[i].width, bubblesRegions.areas[i].height);
                                etRect = new Rectangle(factAreas[i].left, factAreas[i].top, factAreas[i].width, factAreas[i].height);
                                manualSelectedArea = i;// break;
                            }
                        }
                        if (manualSelectedArea > -1)
                        {
                            for (int i = 0; i < factAreas.Length; i++)
                            {
                                if (factLinesPerArea[i] == 0)
                                    continue;
                                if (factAreasManualSet[i])
                                {
                                    questions += factLinesPerArea[i];
                                    continue;
                                }
                                if (factAreas[i].bubblesPerLine == 0)
                                    factAreas[i].bubblesPerLine = etalonAreas[i].bubblesPerLine;
                                if (rec.AmoutOfQuestions != 0 && questions >= rec.AmoutOfQuestions)
                                {
                                    allAreasNaturalSize[i] = Rectangle.Empty;
                                    break;
                                }
                                questions += factLinesPerArea[i];
                                if (factLinesPerArea[i] == 0)
                                {
                                    int amout_of_questions = rec.AmoutOfQuestions;
                                    for (int k = 0; k < rec.linesPerArea.Length; k++)
                                    {
                                        if (amout_of_questions > 0)
                                        {
                                            if (amout_of_questions <= rec.linesPerArea[k])
                                            {
                                                factLinesPerArea[k] = amout_of_questions;
                                                break;
                                            }
                                            else
                                            {
                                                factLinesPerArea[k] = rec.linesPerArea[k];
                                                amout_of_questions -= factLinesPerArea[k];
                                            }
                                        }
                                        else
                                            factLinesPerArea[k] = rec.linesPerArea[k];
                                    }
                                }
                                double EtStepX = (double)
                                     (etalonAreas[i].width - etalonAreas[i].bubble.Width
                                     * bubblesRegions.areas[i].bubblesPerLine)
                                     / (bubblesRegions.areas[i].bubblesPerLine - 1);
                                int widthEt = (int)(bubblesRegions.areas[i].bubble.Width
                                    * factAreas[i].bubblesPerLine + EtStepX * factAreas[i].bubblesPerLine - 1);//(int)linsForm.nudCols.Value - 1);
                                int manualNearestArea = 0;
                                int delta = int.MaxValue;
                                for (int k = 0; k < factAreasManualSet.Length; k++)
                                {
                                    if (!factAreasManualSet[k] || k == i)
                                        continue;
                                    int d = Math.Abs(etalonAreas[i].left - etalonAreas[k].left);
                                    if (d < delta)
                                    {
                                        delta = d;
                                        manualNearestArea = k;
                                    }
                                }
                                int maxBubblesCountOfArea = rec.linesPerArea[0];//кроме "FANDP"!!!
                                //int maxBubblesCountOfArea = (int)Math.Round((double)etalonAreas[i].height /etalonAreas[i].lineHeight);
                                //double EtStepY = (double)(bubblesRegions.areas[i].height
                                //- bubblesRegions.areas[i].bubble.Height * rec.linesPerArea[i]) / (rec.linesPerArea[i] - 1);
                                double EtStepY = (double)(bubblesRegions.areas[i].height
                                - bubblesRegions.areas[i].bubble.Height * maxBubblesCountOfArea) / (maxBubblesCountOfArea - 1);
                                double heightEt = bubblesRegions.areas[i].bubble.Height * factLinesPerArea[i]
                                   + EtStepY * (factLinesPerArea[i] - 1)
                                   + (etalonAreas[i].subLineHeight
                                   * (factAreas[i].subLinesAmount));//bubblesRegions.areas[i].subLinesAmount - 
                                Rectangle r = Rectangle.Empty;
                                if (etalonAreas[manualSelectedArea].left == etalonAreas[i].left)
                                    r.X = factAreas[i].left;
                                else
                                    //r.X = deltaX + (int)Math.Round((decimal)(etalonAreas[i].left * kx));//-deltaX +
                                    r.X = factAreas[0].left + (int)Math.Round((double)(etalonAreas[i].left - etalonAreas[manualNearestArea].left) * kx);
                                if (etalonAreas[manualSelectedArea].bubble.Y == etalonAreas[i].bubble.Y)
                                    r.Y = factAreas[manualSelectedArea].top;
                                else
                                    //r.Y = bubblesRegions.areas[i].top + (int)Math.Round((decimal)(deltaY));//* ky
                                    //r.Y = deltaY + (int)Math.Round((decimal)(etalonAreas[i].bubble.Y * ky));
                                    r.Y = factAreas[0].top + (int)Math.Round((double)(etalonAreas[i].top - etalonAreas[manualNearestArea].top) * ky);
                                r.Width = (int)(Math.Round(widthEt * kx));//deltaX +
                                int diff = factAreas[i].subLinesAmount - factAreas[manualNearestArea].subLinesAmount;
                                //int lineStep = (int)(Math.Round((double)(etalonAreas[manualNearestArea].lineHeight * ky)));
                                //r.Height = diff * (int)Math.Round((double)(etalonAreas[i].subLineHeight * ky))//factAreas[i].subLineHeight
                                ////+ lineStep
                                //+ ((int)(Math.Round(((double)(factAreas[manualNearestArea].height)//- ((int)(Math.Round((double)(EtStepY * ky))))
                                //* factLinesPerArea[i]) / factLinesPerArea[manualNearestArea])))
                                ////- lineStep/
                                //;
                                //int diff = factAreas[i].subLinesAmount - factAreas[manualNearestArea].subLinesAmount;
                                //r.Height = diff * (int)Math.Round((double)(etalonAreas[i].subLineHeight * ky))//factAreas[i].subLineHeight
                                //   - (maxBubblesCountOfArea - diff) * (int)Math.Round((double)(etalonAreas[i].lineHeight * ky))
                                //    + ((int)(Math.Round(((double)(factAreas[manualNearestArea].height)//- ((int)(Math.Round((double)(EtStepY * ky))))
                                //    * factLinesPerArea[i]) / factLinesPerArea[manualNearestArea])));
                                //r.Width = (int)(Math.Round(widthEt * kx));//deltaX +
                                r.Height = (int)(Math.Round(heightEt * ky));
                                if (r.X < 0)
                                    r.X = 0;
                                if (r.Y < 0)
                                    r.Y = 0;
                                //if (r.X<=)
                                //{

                                //}
                                if (r.Right > pictureBox1.Image.Width)
                                    r.Width = pictureBox1.Image.Width - r.X;
                                if (r.Bottom > pictureBox1.Image.Height)
                                    r.Height = pictureBox1.Image.Height - r.Y;
                                if (i == 0)
                                {
                                    if (etalonAreas[i].bubble.X < etalonAreas[i + 1].bubble.X)
                                    {
                                        if (r.Right >= etalonAreas[i + 1].bubble.X)
                                        {
                                            MessageBox.Show
                                                ("Please check that you have the correct number of columns"
                                                , Text, MessageBoxButtons.OK
                                                , MessageBoxIcon.Exclamation
                                                );
                                            allAreasNaturalSize[i] = Rectangle.Empty;
                                        }
                                        else//рисовать автоматически вычисленную
                                             if (linsForm.chbBuildAllAreas.Checked)
                                            allAreasNaturalSize[i] = r;
                                    }
                                }
                                else
                                {
                                    if (r.Width <= 0 || r.X < allAreasNaturalSize[0].Right)
                                    {
                                        MessageBox.Show
                                               ("Please check that you have the correct number of columns"
                                               , Text, MessageBoxButtons.OK
                                               , MessageBoxIcon.Exclamation
                                               );
                                        allAreasNaturalSize[i] = Rectangle.Empty;
                                    }
                                    else//рисовать автоматически вычисленную
                                        if (linsForm.chbBuildAllAreas.Checked)
                                        allAreasNaturalSize[i] = r;
                                }
                            }
                            DrawAllAreasNaturalSize();
                        }
                    }
                }
                if (animatedTimer != null)
                    animatedTimer.StartAnimation();
            }
            catch (Exception)
            {
            }
        }
        private void SetAmoutOfQuestions(int amout_of_questions)
        {
            //if (Status != StatusMessage.Verify || Status != StatusMessage.Delete || Status != StatusMessage.ChangeAmoutOfQuestions)
            //    return;
            //if (Status == StatusMessage.ChangeAmoutOfQuestions)
            //    return;
            if (linsForm == null)
                CreateLinsForm();
            if (amout_of_questions > -1 && linsForm != null)
            {
                //if (rec.AmoutOfQuestions == amout_of_questions)
                //    return;
                if (bac != null)
                    bac.AmoutOfQuestions = amout_of_questions;
                rec.AmoutOfQuestions = amout_of_questions;
                amout_of_questions = SetRowsValue(amout_of_questions);
                if (rec.BubbleItems.Count == 0)
                {
                    maxCountRectangles = rec.AddMaxCountRectangles();
                    rec.FillBubbleItems(maxCountRectangles);
                }
            }
        }
        //-------------------------------------------------------------------------
        private int SetRowsValue(int amout_of_questions)
        {
            //if (factLinesPerArea.Length == 0)
            factLinesPerArea = new int[etalonAreas.Length];
            for (int i = 0; i < rec.linesPerArea.Length; i++)
            {
                if (amout_of_questions > 0)
                {
                    if (amout_of_questions <= rec.linesPerArea[i])
                    {
                        factLinesPerArea[i] = amout_of_questions;
                        //factLinesPerArea[1] = 0;
                        if (i == 0 && linsForm.nudArea.Value == 1)
                            linsForm.nudRows.Value = factLinesPerArea[0];
                        else if (i == 1 && linsForm.nudArea.Value == 2)
                            linsForm.nudRows.Value = factLinesPerArea[1];
                        break;
                    }
                    else
                    {
                        factLinesPerArea[i] = rec.linesPerArea[i];
                        amout_of_questions -= factLinesPerArea[i];
                    }
                }
                else
                    factLinesPerArea[i] = rec.linesPerArea[i];
            }
            return amout_of_questions;
        }
        //-------------------------------------------------------------------------
        private void CreateBubblesAC(int count = 0)
        {
            if (bac.Controls.Count == 0)
                PrevRecognizeBubbles();
            else
                PrevRecognizeBubbles(false);
            rec.BubblesRecognition(false);
            var maxCountRectangles = rec.AddMaxCountRectangles();
            rec.FillBubbleItems(maxCountRectangles);
            rec.UpdateGui();
        }
        //-------------------------------------------------------------------------
        public void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            try
            {
                MouseEventArgs mouseEventArgs = e as MouseEventArgs;
                if (mouseEventArgs != null)
                {
                    //UpdateZoomedImage(x, y);
                }
                pictureBox1.Refresh();
                pictureBox1_Paint(null, null);
                nudZoom.Visible = false;
                if (barCodeSel)
                {
                    pictureBox1.Cursor = Cursors.Cross;
                    var itm = SelectedBarCodeItem;
                    if (itm != null)
                    {
                        StatusLabel.Text = "Show area of " + itm.Name;
                    }
                    else
                    {
                        StatusLabel.Text = "Select some bar code";
                    }
                }
                else if (rbtnGrid.Checked)
                {
                    StatusLabel.Text = "Show areas of bubbles";
                }
                else if (rbtnRotate.Checked)
                {
                    StatusLabel.Text = "Show line, which should be horizontal or vertical";
                }
                else if (rbtnCut.Checked || rbtnClear.Checked)
                {
                    pictureBox1.Cursor = Cursors.Cross;
                    StatusLabel.Text = "Show area of sheet";
                }
                else
                {
                    StatusLabel.Text = "";
                    pictureBox1.Cursor = Cursors.Default;
                }
            }
            catch (Exception)
            {
            }
        }
        //-------------------------------------------------------------------------
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                DrawAllAreasNaturalSize();
                if (pictureBox1.Image == null)
                {
                    btnGrid.Enabled = false;
                    //btnGrid.BackColor = SystemColors.Control;
                    splitBtnRestore.Enabled = false;
                    ImagePanel.AutoScroll = false;
                    barCodeSel = false;
                }
                else if (pictureBox1.Image != null)
                    splitBtnRestore.Enabled = true;
                else if (linsForm != null || barCodeSel || rbtnGrid.Checked || rbtnCut.Checked)// && area != new Rectangle()
                {
                    if (rbtnBubbles.Checked)
                        return;
                    DrawAllAreasNaturalSize();
                }
            }
            catch (Exception)
            {
            }
        }
        //-------------------------------------------------------------------------
        enum AreaResize
        {
            Left, Right, Top, Bottom, None
        }
        //-------------------------------------------------------------------------
        AreaResize areaResize = AreaResize.None;
        //-------------------------------------------------------------------------
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                int x = (int)Math.Round(e.X / Zoom);
                int y = (int)Math.Round(e.Y / Zoom);
                if (sender != null)
                {
                    Text = countAllFraves.ToString() + " " + appDir + appTitle + " X: " + x.ToString() + " Y: " + y.ToString();
                }
                UpdateZoomedImage(x, y);

                if ((linsForm != null && linsForm.Visible) || rbtnRotate.Checked || rbtnCut.Checked || barCodeSel || rbtnClear.Checked)
                {
                    Text = countAllFraves.ToString() + " " + appDir + appTitle + " X: " + x.ToString() + " Y: " + y.ToString();
                    if (animatedTimer != null)
                        animatedTimer.StopAnimation();
                    if (e.Button == MouseButtons.None && !rbtnRotate.Checked)// && area != new Rectangle()  !linsForm.rbtnAligment.Checked
                    {
                        for (int i = 0; i < allAreasNaturalSize.Length; i++)
                        {
                            //using (GraphicsPath gp = new GraphicsPath())
                            //{//new Point[]{
                            //    gp.AddPolygon
                            //        (
                            //        new Point[]
                            //        {
                            //          new Point(allAreasNaturalSize[i].X, allAreasNaturalSize[i].Y)
                            //        , new Point(allAreasNaturalSize[i].Right, allAreasNaturalSize[i].Y)
                            //        , new Point(allAreasNaturalSize[i].Right, allAreasNaturalSize[i].Bottom)
                            //        , new Point(allAreasNaturalSize[i].X, allAreasNaturalSize[i].Bottom)
                            //        , new Point(allAreasNaturalSize[i].X, allAreasNaturalSize[i].Y)
                            //        }
                            //        );
                            //    RectangleF bounds = gp.GetBounds();
                            //    //if (gp.IsVisible(x, y))
                            //    //    linsForm.nudArea.Value = i + 1;
                            //}

                            if (Math.Abs(x - allAreasNaturalSize[i].X) < 20
                                && y >= allAreasNaturalSize[i].Y
                                && y <= allAreasNaturalSize[i].Bottom)// && Math.Abs(y - allAreasNaturalSize[i].Y) < 10
                            {
                                pictureBox1.Cursor = Cursors.SizeWE;
                                areaNaturalSize = allAreasNaturalSize[i];
                                //linsForm.nudArea.Value = i + 1;
                                break;
                            }
                            else if (Math.Abs(x - allAreasNaturalSize[i].Right) < 20
                                && y >= allAreasNaturalSize[i].Y
                                && y <= allAreasNaturalSize[i].Bottom)
                            {
                                pictureBox1.Cursor = Cursors.SizeWE;
                                areaNaturalSize = allAreasNaturalSize[i];
                                //linsForm.nudArea.Value = i + 1;
                                break;
                            }
                            else if (Math.Abs(y - allAreasNaturalSize[i].Y) < 20
                                && x >= allAreasNaturalSize[i].X
                                && x <= allAreasNaturalSize[i].Right)
                            {
                                pictureBox1.Cursor = Cursors.SizeNS;
                                areaNaturalSize = allAreasNaturalSize[i];
                                //linsForm.nudArea.Value = i + 1;
                                break;
                            }
                            else if (Math.Abs(y - allAreasNaturalSize[i].Bottom) < 20
                                && x >= allAreasNaturalSize[i].X
                                && x <= allAreasNaturalSize[i].Right)
                            {
                                pictureBox1.Cursor = Cursors.SizeNS;
                                areaNaturalSize = allAreasNaturalSize[i];
                                //linsForm.nudArea.Value = i + 1;
                                break;
                            }
                            else// if (e.Button == MouseButtons.None)
                            {
                                pictureBox1.Cursor = Cursors.Cross;
                            }
                        }
                    }
                    else
                    {
                        if (!rbtnBubbles.Checked)
                            pictureBox1.Cursor = Cursors.Cross;
                    }
                    if (e.Button == MouseButtons.Left)//??? иногда возникает при не нажатой кнопке
                    {
                        if (animatedTimer != null
                            && (linsForm != null || rbtnRotate.Checked || rbtnGrid.Checked)
                            //&& area != new Rectangle()
                            )
                            animatedTimer.StopAnimation();
                        MoveScrollBubble(new Rectangle(x, y, 20, 20));
                        if (sender != null)
                            pictureBox1.Refresh();
                        Graphics g = pictureBox1.CreateGraphics();
                        if (rbtnRotate.Checked)//linsForm.rbtnAligment.Checked
                            g.DrawLine(new Pen(Brushes.Red, 1), new Point(e.X, e.Y), begPoint);
                        else
                        {
                            int delta = 0;
                            Rectangle r = Rectangle.Empty;
                            if (sender != null)
                            {
                                switch (areaResize)
                                {
                                    case AreaResize.Left:
                                        delta = (int)((e.X - begPoint.X) / Zoom);
                                        begPoint.X = e.X;
                                        areaNaturalSize = new Rectangle
                                            (
                                              areaNaturalSize.X + delta
                                            , areaNaturalSize.Y
                                            , areaNaturalSize.Width - delta
                                            , areaNaturalSize.Height
                                            );


                                        r = recTools.MultiplyRectangle(areaNaturalSize, Zoom);
                                        g.DrawRectangle(new Pen(Brushes.DarkViolet, 1), r);
                                        break;
                                    case AreaResize.Right:
                                        delta = (int)((e.X - begPoint.X) / Zoom);
                                        begPoint.X = e.X;
                                        areaNaturalSize = new Rectangle
                                            (
                                              areaNaturalSize.X
                                            , areaNaturalSize.Y
                                            , areaNaturalSize.Width + delta
                                            , areaNaturalSize.Height
                                            );
                                        r = recTools.MultiplyRectangle(areaNaturalSize, Zoom);
                                        g.DrawRectangle(new Pen(Brushes.DarkViolet, 1), r);
                                        break;
                                    case AreaResize.Top:
                                        delta = (int)((e.Y - begPoint.Y) / Zoom);
                                        begPoint.Y = e.Y;
                                        areaNaturalSize = new Rectangle
                                            (
                                              areaNaturalSize.X
                                            , areaNaturalSize.Y + delta
                                            , areaNaturalSize.Width
                                            , areaNaturalSize.Height - delta
                                            );
                                        r = recTools.MultiplyRectangle(areaNaturalSize, Zoom);
                                        g.DrawRectangle(new Pen(Brushes.DarkViolet, 1), r);
                                        break;
                                    case AreaResize.Bottom:
                                        delta = (int)((e.Y - begPoint.Y) / Zoom);
                                        begPoint.Y = e.Y;
                                        areaNaturalSize = new Rectangle
                                            (
                                              areaNaturalSize.X
                                            , areaNaturalSize.Y
                                            , areaNaturalSize.Width
                                            , areaNaturalSize.Height + delta
                                            );
                                        r = recTools.MultiplyRectangle(areaNaturalSize, Zoom);
                                        g.DrawRectangle(new Pen(Brushes.DarkViolet, 1), r);
                                        break;
                                    case AreaResize.None:
                                        int i = begPoint.X;
                                        int j = e.X;
                                        if (i > j)
                                            Swap(ref i, ref j);
                                        int k = begPoint.Y;
                                        int l = e.Y;
                                        if (k > l)
                                            Swap(ref k, ref l);
                                        area = new Rectangle(i, k, j - i, l - k);
                                        g.DrawRectangle(new Pen(Brushes.DarkViolet, 1), area);
                                        areaNaturalSize = recTools.MultiplyRectangle(area, 1 / Zoom);
                                        break;
                                }
                            }
                        }
                        g.Dispose();
                        return;
                    }
                }
                if (BarCodeItems == null || rec == null || animatedTimer == null) return;
                var item = animatedTimer.FindActiveRectangle(e, Zoom, BarCodeItems.ToList(), rec.BubbleItems.ToList());
                if (item == null)
                    return;
                if (barCodeSel)
                    return;

                if ((item as BarCodeItem) != null)
                {
                    barCodeList.Select(item as BarCodeItem);
                    var v = animatedTimer.ActiveBarCode;
                    if (animatedTimer != null && v != null && v != item)
                    {
                        AnimatedBarCodeClear();
                        animatedTimer.SetActiveValue((Bitmap)pictureBox1.Image, (BarCodeItem)item);
                    }
                }
                else if ((item as BubbleItem) != null)
                {
                    AnimatedBarCodeClear();
                    //bac.lockReport = true;
                    if (animatedTimer.ActiveBubbleItem != null && animatedTimer.ActiveBubbleItem != item)// 
                    {
                        DrawBubble(animatedTimer.ActiveBubbleItem.CheckedBubble);
                    }
                    if (sender != null)
                    {
                        BubbleItem itm = (BubbleItem)item;
                        bac.Select((BubbleItem)item);
                        bac.lockReport = true;
                    }
                    if (animatedTimer != null) animatedTimer.SetActiveValue((BubbleItem)item);
                }
            }
            catch (Exception)
            {
            }
        }
        //-------------------------------------------------------------------------
        private void DrawAllAreasNaturalSize()
        {
            if (rbtnGrid.Checked)
            {
                Graphics g = pictureBox1.CreateGraphics();
                for (int j1 = 0; j1 < allAreasNaturalSize.Length; j1++)
                {
                    Rectangle rn = recTools.MultiplyRectangle(allAreasNaturalSize[j1], Zoom);
                    if (factAreasManualSet[j1])
                        g.DrawRectangle(new Pen(Brushes.DarkViolet, 1), rn);
                    else
                        g.DrawRectangle(new Pen(Brushes.Red, 1), rn);
                }
                g.Dispose();
            }
        }
        //-------------------------------------------------------------------------
        private void Swap(ref int i, ref int j)
        {
            int k = i;
            i = j;
            j = k;
        }
        //-------------------------------------------------------------------------
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                begPoint = new Point(e.X, e.Y);
                begPointZoom = new Point((int)Math.Round(begPoint.X / Zoom), (int)Math.Round(begPoint.Y / Zoom));
                areaResize = AreaResize.None;

                if (linsForm != null)// || rbtnRotate.Checked || barCodeSel
                {
                    if (barCodeSel)
                        AnimatedBarCodeClear();
                    if (rbtnGrid.Checked)
                    {
                        if (lastSheetId != rec.SheetIdentifier)
                        {
                            BarCodeListItemControl item = (BarCodeListItemControl)barCodeList.ControlList[amout_of_questionsIndex];
                            if (string.IsNullOrEmpty(item.comboBox1.Text) || !utils.IsNumeric(item.comboBox1.Text))
                            {
                                MessageBox.Show("Please, specify the amout of questions"
                                , Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                return;
                            }
                        }
                        int x = (int)Math.Round(e.X / Zoom);
                        int y = (int)Math.Round(e.Y / Zoom);
                        if (e.Button == MouseButtons.Left)// && area != new Rectangle() && linsForm.rbtnGrid.Checked
                        {
                            if (Math.Abs(x - areaNaturalSize.X) < 20 && y >= areaNaturalSize.Y && y <= areaNaturalSize.Bottom)
                            {
                                areaResize = AreaResize.Left;
                            }
                            else if (Math.Abs(x - areaNaturalSize.Right) < 20 && y >= areaNaturalSize.Y && y <= areaNaturalSize.Bottom)
                            {
                                areaResize = AreaResize.Right;
                            }
                            else if (Math.Abs(y - areaNaturalSize.Y) < 20 && x >= areaNaturalSize.X && x <= areaNaturalSize.Right)
                            {
                                areaResize = AreaResize.Top;
                            }
                            else if (Math.Abs(y - areaNaturalSize.Bottom) < 20 && x >= areaNaturalSize.X && x <= areaNaturalSize.Right)
                            {
                                areaResize = AreaResize.Bottom;
                            }
                            else
                            {
                                areaResize = AreaResize.None;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        //-------------------------------------------------------------------------
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (BarCodeItems == null || rec == null || animatedTimer == null)
                    return;
                //int clics = e.Clicks;
                var item = animatedTimer.FindActiveRectangle(e, Zoom, BarCodeItems.ToList(), rec.BubbleItems.ToList());
                if (item == null)
                    return;
                if ((item as BarCodeItem) != null)
                {
                    (item as BarCodeItem).BorderColorOpacity = 30;//выбрать для редактирования контрол штрихкода
                }
                if ((item as BubbleItem) != null)
                {
                    //BubbleItem bi = (BubbleItem)item;
                    //int index = rec.areas[bi.Bubble.areaNumber].questionIndex;
                    //int count = rec.areas[bi.Bubble.areaNumber].bubblesPerLine;
                    BubbleItem itm = (BubbleItem)item;
                    if (ModifierKeys == Keys.Control)
                        bac.SetInvertBubbleChecked(item as BubbleItem, true);
                    else
                    {
                        itm.CheckedBubble.isChecked = !itm.CheckedBubble.isChecked;
                        bac.SetInvertBubbleChecked(item as BubbleItem);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        #endregion
        //-------------------------------------------------------------------------
        #region Animation
        AnimatedTimer animatedTimer = new AnimatedTimer();

        private void animatedTimer_Tick(object sender, EventArgs e)
        {
            UpdateZoomedImage();
            if ((barCodeSel && area != new Rectangle()) || (rbtnGrid.Checked && area != new Rectangle()) || rbtnRotate.Checked)//linsForm != null
                return;
            //if (linsForm != null)
            try
            {
                AnimatedBarCode(animatedTimer.ActiveBarCode);
                AnimatedBubble(animatedTimer.ActiveBubbleItem);
                DrawAllAreasNaturalSize();
            }
            catch { }
        }
        //-------------------------------------------------------------------------
        private void AnimatedBarCodeClear()
        {
            if (animatedTimer != null && animatedTimer.ActiveBarCode != null)
                using (Graphics g = Graphics.FromImage(pictureBox1.Image))
                {
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.DrawImage(animatedTimer.ActiveBarCodeBitmap, animatedTimer.ActiveBarCode.Rectangle);
                }
        }
        //-------------------------------------------------------------------------
        private void AnimatedBarCode(BarCodeItem item)
        {
            try
            {
                if (animatedTimer != null && item == null || item.Rectangle == Rectangle.Empty) return;
                AnimatedBarCodeClear();
                AnimatedBubbleClear();
                animatedTimer.SetActiveValue((Bitmap)pictureBox1.Image, item);//animatedTimer==null?
                if (animatedTimer.BorderColorOpacity == 255)
                {
                    pictureBox1.Refresh();
                    return;
                }
                using (Graphics g = Graphics.FromImage(pictureBox1.Image))
                {
                    Rectangle r = Rectangle.Empty;
                    r.Location = new Point(animatedTimer.ActiveBarCode.Rectangle.X, animatedTimer.ActiveBarCode.Rectangle.Y);
                    if (animatedTimer.ActiveBarCode.Rectangle.Width > animatedTimer.ActiveBarCode.Rectangle.Height)
                    {
                        r.Width = animatedTimer.ActiveBarCode.Rectangle.Height;
                        r.Height = animatedTimer.ActiveBarCode.Rectangle.Height - 1;
                    }
                    else
                    {
                        r.Width = animatedTimer.ActiveBarCode.Rectangle.Width;
                        r.Height = animatedTimer.ActiveBarCode.Rectangle.Width - 1;
                    }
                    Color c = Color.FromArgb(128, 0, 255, 0);
                    g.DrawEllipse(new Pen(c, 1), r);//Color.LightGreen
                    g.FillEllipse(new SolidBrush(c), r);
                }
                pictureBox1.Refresh();
            }
            catch (Exception)
            {
            }
        }
        //-------------------------------------------------------------------------
        private void AnimatedBubbleClear()
        {
            if (animatedTimer != null && animatedTimer.ActiveBubbleItem != null)
            {
                DrawBubble(animatedTimer.ActiveBubbleItem.CheckedBubble);
            }
        }
        //-------------------------------------------------------------------------
        private void AnimatedBubble(BubbleItem bubbleItem)
        {
            if (bubbleItem == null)
                return;
            AnimatedBarCodeClear();

            if (bac != null)
            {
                if (animatedTimer != null && animatedTimer.BorderColorOpacity != 255)
                {
                    CheckBox cb = bac.CheckBoxArr[rec.BubbleItems.IndexOf(bubbleItem)];
                    if (cb != null)
                    {
                        using (Graphics g = cb.CreateGraphics())
                        {
                            g.DrawRectangle(new Pen(Color.LightCyan, 1), new Rectangle
                                (
                                  cb.ClientRectangle.X
                                , cb.ClientRectangle.Y
                                , cb.ClientRectangle.Width - 3
                                , cb.ClientRectangle.Height - 2
                                ));
                        }
                    }
                }
                else
                {
                    bac.Refresh();
                }
            }

            AnimatedBubbleClear();
            if (animatedTimer != null) animatedTimer.SetActiveValue(bubbleItem);
            if (animatedTimer.BorderColorOpacity == 255)
            {
                pictureBox1.Refresh();
                return;
            }
            using (Graphics g = Graphics.FromImage(pictureBox1.Image))
            {
                g.DrawRectangle(new Pen(Color.LightCyan, 3), animatedTimer.ActiveBubbleItem.CheckedBubble.rectangle);
            }
            pictureBox1.Refresh();
        }
        //-------------------------------------------------------------------------
        public void DrawBubble(CheckedBubble checkedBubble)
        {
            if (checkedBubble.isChecked)
                using (Graphics g = Graphics.FromImage(pictureBox1.Image))
                    g.DrawRectangle(new Pen(Color.Red, 3), checkedBubble.rectangle);
            else
                using (Graphics g = Graphics.FromImage(pictureBox1.Image))
                    g.DrawRectangle(new Pen(Color.Blue, 3), checkedBubble.rectangle);
        }
        #endregion
        //-------------------------------------------------------------------------
        #region Timer filesInQueue
        public bool comboBoxFocused = true;
        private System.Windows.Forms.Timer filesInQueueTimer;
        private System.Threading.Tasks.Task filesInQueueTask;
        private void FilesInQueueTimer_Tick(object sender, EventArgs e)
        {
            if (filesInQueueTask == null || filesInQueueTask.IsCanceled || filesInQueueTask.IsCompleted || filesInQueueTask.IsFaulted)
            {
                if (!comboBoxFocused)
                    comboBoxSetFocus();
                filesInQueueTask = System.Threading.Tasks.Task.Factory.StartNew(status => filesInQueueTask_DoWork(), "filesInQueueTask").ContinueWith((t) =>
                {
                    if (t.Exception != null) log.LogMessage(t.Exception);
                });
            }
        }
        //-------------------------------------------------------------------------
        private void filesInQueueTask_DoWork()
        {
            if (VerifyDirectory(defaults.ManualInputFolder))
            {
                var fnArr = utils.GetSupportedFilesFromDirectory(defaults.ManualInputFolder, SearchOption.AllDirectories, false);
                var fnTempArr = utils.GetSupportedFilesFromDirectory(OcrAppConfig.TempEdFolder, SearchOption.TopDirectoryOnly, false);
                countAllFraves = fnArr.Length + fnTempArr.Length;
                //UpdateUI("filesInQueueStatusLabel", fnArr.Count().ToString());
                UpdateUI("filesInQueueStatusLabel", countAllFraves.ToString());
                string dir = Path.Combine(OcrAppConfig.TempEdFolder, "Deferred");
                fnArr = new string[0];
                if (Directory.Exists(dir))
                {
                    fnArr = utils.GetSupportedFilesFromDirectory(dir, SearchOption.TopDirectoryOnly);
                }
                countAllFraves += fnArr.Length;
                Invoke(new MethodInvoker(delegate
                {
                    Text = countAllFraves.ToString() + " " + appDir + appTitle;
                }));
            }
        }
        #endregion
        //-------------------------------------------------------------------------
        public void RestoreFromSourceFile()
        {
            try
            {
                rec.Bitmap = (Bitmap)Bitmap.FromFile(rec.recTools.frameFileName);
                Bitmap bmp;
                if (rec.Bitmap.HorizontalResolution != rec.Bitmap.VerticalResolution)
                {
                    if (rec.Bitmap.HorizontalResolution > rec.Bitmap.VerticalResolution)
                    {
                        bmp = new Bitmap
                            (
                              rec.Bitmap.Width
                            , rec.Bitmap.Height * (int)(rec.Bitmap.HorizontalResolution / rec.Bitmap.VerticalResolution)
                            , PixelFormat.Format24bppRgb
                            );
                        bmp.SetResolution(rec.Bitmap.HorizontalResolution, rec.Bitmap.HorizontalResolution);
                        Graphics g = Graphics.FromImage(bmp);
                        g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                        g.DrawImage(rec.Bitmap, 0, 0, bmp.Width, bmp.Height);
                        g.Dispose();
                        rec.Bitmap = (Bitmap)bmp.Clone();
                        bmp.Dispose();
                    }
                    else
                    {
                        bmp = new Bitmap
                            (
                              rec.Bitmap.Width * (int)(rec.Bitmap.VerticalResolution / rec.Bitmap.HorizontalResolution)
                            , rec.Bitmap.Height
                            , PixelFormat.Format24bppRgb
                            );
                        bmp.SetResolution(rec.Bitmap.HorizontalResolution, rec.Bitmap.HorizontalResolution);
                        Graphics g = Graphics.FromImage(bmp);
                        g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                        g.DrawImage(rec.Bitmap, 0, 0, bmp.Width, bmp.Height);
                        g.Dispose();
                        rec.Bitmap = (Bitmap)bmp.Clone();
                        bmp.Dispose();
                    }

                    //entryBitmap.Save("entryBitmap.bmp", ImageFormat.Bmp);
                }

                rec.Bitmap.SetResolution(96, 96);
                //Bitmap bmp = (Bitmap)Bitmap.FromFile(rec.recTools.frameFileName);
                //rec.Bitmap = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format24bppRgb);
                ////using (Graphics g = Graphics.FromImage(bmp))
                ////{
                ////    g.DrawImage
                ////        (
                ////          rec.Bitmap
                ////        , new Rectangle(0, 0, rec.Bitmap.Width, rec.Bitmap.Height)
                ////        , 0, 0, rec.Bitmap.Width, rec.Bitmap.Height, GraphicsUnit.Pixel
                ////        );
                ////}
                //Graphics gi = Graphics.FromImage(rec.Bitmap);//удаление индексированных цветов
                ////gi.InterpolationMode = InterpolationMode.HighQualityBilinear;//Bicubic.NearestNeighbor;
                //gi.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                //gi.Dispose();
                //bmp.Dispose();

                //rec.Bitmap.SetResolution(96, 96);
                //pictureBox1.Image = new Bitmap(rec.Bitmap);
                ShowImage();
                RecognizeBubblesButton.Enabled = false;
                RecognizeAllButton.Enabled = true;
                isInvert = false;
                isRotate = false;
                isCut = false;
                isClear = false;
                if (animatedTimer != null) animatedTimer.StopAnimation();
                rec.BubbleItems.Clear();
                //AnimatedBarCodeClear();
                //AnimatedBubbleClear();
                //animatedTimer.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }
        //-------------------------------------------------------------------------
        #region Timer Working
        private AbortableBackgroundWorker BW = new AbortableBackgroundWorker();
        //private int timeOut = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (defaults.IsChangeAppConfig())
            {
                SaveSettings();
                //this.StatusLabel.Text = OcrAppConfig.AppConfigFileName + " reloading";
                UpdateUI("StatusLabel", OcrAppConfig.AppConfigFileName + " reloading");
                Application.DoEvents();
                System.Threading.Thread.Sleep(1000);
                Init();
            }
            ioHelper.ReDelete();
            //if (BW.workerThread == null || !BW.IsBusy)
            //{
            //    var filesInTempEd = utils.GetSupportedFilesFromDirectory(OcrAppConfig.TempEdFolder
            //     , SearchOption.TopDirectoryOnly, true);
            //    if (filesInTempEd.Length < minCountFilesInTempFolder)
            //    {
            //        //BW.RunWorkerCompleted += CompletedBW;
            //        //BW.WorkerSupportsCancellation = true;
            //        //timeOut = 0;
            //        var countFiles = filesInTempEd.Length;
            //        try
            //        {
            //            BW.RunWorkerAsync(countFiles);
            //        }
            //        catch (Exception)
            //        {
            //            //timeOut = 0;
            //            BW.Abort();
            //        }
            //    }
            //else
            //{
            //    timeOut++;
            //    if (timeOut > 900)//150
            //    {
            //        timeOut = 0;
            //        BW.Abort();
            //    }
            //}
            //}
            if (!backgroundWorker.IsBusy)
            {
                //var filesInTempEd = utils.GetSupportedFilesFromDirectory(OcrAppConfig.TempEdFolder
                // , SearchOption.TopDirectoryOnly, true);
                //string fileName = GetFileForRecognizeFromFolders(OcrAppConfig.TempEdFolder, defaults.ManualErrorFolder);
                string fileName = GetFileForRecognizeFromFolders(OcrAppConfig.TempEdFolder, "");
                Working(fileName);
            }
            string dir = Path.Combine(OcrAppConfig.TempEdFolder, "Deferred");
            if (Directory.Exists(dir))
            {
                string[] fnArr = utils.GetSupportedFilesFromDirectory(dir, SearchOption.TopDirectoryOnly);
                tsslSetAside.Text = fnArr.Length.ToString();
            }
            else
                tsslSetAside.Text = "0";
        }
        //-------------------------------------------------------------------------
        private void timer2_Tick(object sender, EventArgs e)
        {
            if (BW.workerThread == null || !BW.IsBusy)
            {
                var filesInTempEd = utils.GetSupportedFilesFromDirectory(OcrAppConfig.TempEdFolder
                 , SearchOption.TopDirectoryOnly, true);
                if (filesInTempEd.Length < minCountFilesInTempFolder)
                {
                    var countFiles = filesInTempEd.Length;
                    try
                    {
                        BW.RunWorkerAsync(countFiles);
                    }
                    catch (Exception)
                    {
                        BW.Abort();
                    }
                }
            }
        }
        //-------------------------------------------------------------------------
        private void DoWorkBW(object sender, DoWorkEventArgs e)
        {
            try
            {
                int obj = (int)e.Argument;
                var fnArr = new string[0];
                if (VerifyDirectory(defaults.ManualInputFolder))
                {
                    fnArr = utils.GetSupportedFilesFromDirectory(defaults.ManualInputFolder, SearchOption.AllDirectories);
                }
                //for (int i = obj; i < minCountFilesInTempFolder; i++)
                //{
                //string fileName = GetFileForRecognizeFromFolders("", defaults.ManualInputFolder);//ManualErrorFolder);
                int count = 0;
                foreach (var file in fnArr)
                //for (int j = 0; j < fnArr.Count(); j++)
                {
                    //if (j >= fnArr.Count())
                    //{
                    //    return;
                    //}
                    //var file = fnArr[j];
                    if (!ioHelper.ReDeleteList.Contains(file))
                    {
                        if (utils.CanAccess(file))
                        {
                            string fileAudit = Path.ChangeExtension(file, ".audit");
                            if (!File.Exists(fileAudit))
                            {
                                log.LogMessage("File " + fileAudit + " not exists");
                                FileInfo fi = new FileInfo(file);
                                DateTime begEx = DateTime.Parse(fi.CreationTime.ToString());
                                TimeSpan ts = DateTime.Now - begEx;
                                if (ts.TotalSeconds < 2)
                                    continue;
                            }
                            if (File.Exists(fileAudit) && !utils.CanAccess(fileAudit))
                            {
                                if (!notAccess.Contains(fileAudit))
                                {
                                    notAccess.Add(fileAudit);
                                    log.LogMessage("Can not access to file: " + fileAudit);
                                }
                                continue;
                            }
                            else
                            {
                                if (notAccess.Contains(fileAudit))
                                {
                                    notAccess.Remove(fileAudit);
                                }
                            }
                            string s = utils.GetFileForRecognize(file, OcrAppConfig.TempEdFolder);
                            if (!string.IsNullOrEmpty(s))
                            {
                                int minCountFilesInTempFolderExt = minCountFilesInTempFolder;
                                if (fnArr.Length > 100)
                                {
                                    minCountFilesInTempFolderExt = 20;
                                }
                                else if (fnArr.Length > 200)
                                {
                                    minCountFilesInTempFolderExt = 50;
                                }
                                count++;
                                if (count + obj >= minCountFilesInTempFolderExt)
                                {
                                    return;
                                }
                            }
                        }
                        else
                        {
                            log.LogMessage("Can not access to file: " + file);
                        }
                    }
                }
                //}
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }
        //-------------------------------------------------------------------------
        //private void CompletedBW(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    //runWorker = false;
        //}
        //-------------------------------------------------------------------------
        private void Working(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                UpdateUI("StatusLabel", "Watching");
            }
            else
            {
                timer1.Stop();
                UpdateUI("StatusLabel", "Working");
                BitmapRecognize(fileName);
            }
        }
        #endregion
        //-------------------------------------------------------------------------
        #region Recognize
        public static Recognize rec;
        private void Recognize_ExceptionEvent(object sender, EventArgs e)
        {
            log.LogMessage(sender as Exception);
            UpdateUI("Exception", (sender as Exception).Message);
        }
        //-------------------------------------------------------------------------
        private void Recognize_ChangedBarCodesPrompt(object sender, EventArgs e)
        {
            Invoke(new MethodInvoker(delegate
              {
                  if (rec != null && rec.Status == RecognizeAction.WaitingForUserResponse)
                  {
                      if (rec.BarCodesPrompt != "")
                      {
                          UpdateUI("StatusLabel", rec.BarCodesPrompt);
                          lblErr.Text = rec.BarCodesPrompt;
                          if (!errList.Contains(rec.BarCodesPrompt))
                              errList.Add(rec.BarCodesPrompt);
                          lblErr.Visible = true;
                      }
                      else UpdateUI("StatusLabel", "Sheet identifier not recognized");

                      if (rec != null && (rec.BarCodesPrompt.StartsWith("S"))
                && !ShetIdManualySet && appSettings.Fields.ChbSheetId && lastSheetId != "")
                      {
                          rec.SheetIdentifier = lastSheetId;
                          var selectedItem = MiniatureItems.First(x => x.Name == rec.SheetIdentifier);
                          if (selectedItem != null)
                          {
                              BoxSheet.SelectedItem = selectedItem;
                              miniatureList.SelectedItem = selectedItem;
                              rec.SheetIdentifier = selectedItem.ToString();
                          }

                          //rec.BarCodesPrompt = "";

                          if (backgroundWorker.IsBusy)
                          {
                              //backgroundWorker.Abort();
                              CancelBackgroundWorker();
                              do
                              {
                                  Application.DoEvents();
                              } while (backgroundWorker.IsBusy);
                              System.Threading.Thread.Sleep(500);
                          }
                          //StopRecognizeButton.Enabled = true;
                          CloseProcessingForm();
                          rpf = null;
                          btnCloseLblErr_Click(null, null);
                          RecognizeAll(true);
                          return;
                      }
                      BoxSheet.SelectedIndex = -1;
                      miniatureList.SelectedItem = null;

                      VerifyButton.Enabled = false;

                      RecognizeAllButton.Enabled = true;//????
                      RecognizeBubblesButton.Enabled = false;
                      StopRecognizeButton.Enabled = false;

                      //DeleteButton, NextButton или выбирается BoxSheet
                      //должен замигать BoxSheet (обратить на себя внимание)

                      CloseProcessingForm();
                  }
              }));
        }
        //-------------------------------------------------------------------------
        private void Recognize_FindedBarcodeControllEvent(object sender, BarcodeEventArgs e)
        {
            if (e.BarCode != null)
            {
                Invoke(new MethodInvoker(delegate
                 {
                     BarCodeItems.Add(e.BarCode);
                     switch (e.BarCode.Name)
                     {
                         case "district_id":
                             if (appSettings.Fields.DistrictId
                                 && !string.IsNullOrEmpty(lastDistrictId)
                                 && string.IsNullOrEmpty(e.BarCode.Value))
                             {
                                 e.BarCode.Value = lastDistrictId;
                                 e.BarCode.Verify = true;
                                 BarCodeListItemControl b = barCodeList.ControlList[e.BarCode.Name] as BarCodeListItemControl;
                                 b.comboBox1.ForeColor = Color.Red;
                             }
                             break;
                         case "test_id":
                             if (appSettings.Fields.TestId
                                 && !string.IsNullOrEmpty(lastTestId)
                                 && string.IsNullOrEmpty(e.BarCode.Value))
                             {
                                 e.BarCode.Value = lastTestId;
                                 testId = lastTestId;
                                 e.BarCode.Verify = true;
                                 BarCodeListItemControl b = barCodeList.ControlList[e.BarCode.Name] as BarCodeListItemControl;
                                 b.comboBox1.ForeColor = Color.Red;
                             }
                             break;
                         case "amout_of_questions":
                             if (appSettings.Fields.AmoutOfQuestions
                                 && !string.IsNullOrEmpty(lastAmoutOfQuestions)
                                 && string.IsNullOrEmpty(e.BarCode.Value))
                             {
                                 e.BarCode.Value = lastAmoutOfQuestions;
                                 e.BarCode.Verify = true;
                                 rec.AmoutOfQuestions = Convert.ToInt32(lastAmoutOfQuestions);
                                 BarCodeListItemControl b = barCodeList.ControlList[e.BarCode.Name] as BarCodeListItemControl;
                                 b.comboBox1.ForeColor = Color.Red;
                                 rec.allBarCodeValues[Array.IndexOf(rec.allBarCodeNames, "amout_of_questions")] = e.BarCode.Value;
                                 //BarCodeListItemControl b = barCodeList.ControlList["amout_of_questions"] as BarCodeListItemControl;
                                 //b.btnCheck.PerformClick();
                                 //e.BarCode.Barcode = lastAmoutOfQuestions;
                                 //e.BarCode.BarcodeMem = lastAmoutOfQuestions;
                                 //e.BarCode.VerifyValue();
                             }
                             break;
                         case "question_number_1":
                         case "index_of_first_question":
                             if (appSettings.Fields.IndexOfFirstQuestion
                                 && !string.IsNullOrEmpty(lastIndexOfQuestion)
                                 && string.IsNullOrEmpty(e.BarCode.Value))
                             {
                                 e.BarCode.Value = lastIndexOfQuestion;
                                 e.BarCode.Verify = true;
                                 BarCodeListItemControl b = barCodeList.ControlList[e.BarCode.Name] as BarCodeListItemControl;
                                 b.comboBox1.ForeColor = Color.Red;
                             }
                             break;
                         default:
                             break;
                     }

                     if (e.BarCode.Name == "test_id")
                     {
                         testId = e.BarCode.Value;
                     }
                     else if (e.BarCode.Name == "district_id")
                     {
                         districtId = e.BarCode.Value;
                     }
                     else if (e.BarCode.Name == "amout_of_questions")
                     {
                         amoutOfQuestions = e.BarCode.Value;
                         if (bac.CheckBoxArr.Length != 0)
                         {
                             if (e.BarCode.Verify)
                                 bac.AmoutOfQuestions = Convert.ToInt32(e.BarCode.Value);
                         }
                     }
                     else if (e.BarCode.Name == "index_of_first_question" || e.BarCode.Name == "question_number_1")
                     {
                         indexOfQuestion = e.BarCode.Value;
                     }
                     //BarCodeItems.Add(e.BarCode);
                     barCodeList.Refresh();
                     if (!e.BarCode.Verify)
                     {
                         lblErr.Text = "Error in " + e.BarCode.Name;
                         lblErr.Visible = true;
                         if (!errList.Contains(lblErr.Text))
                             errList.Add(lblErr.Text);
                     }
                 }));
            }
        }
        //-------------------------------------------------------------------------
        private void AddBubblesAreaControl(BubbleEventArgs e)
        {
            if (Status == StatusMessage.ChangeAmoutOfQuestions)
                return;
            //linesPerArea = new int[rec.areas.Length];
            //bubblesPerLine = new int[rec.areas.Length];
            linesPerArea = new int[rec.linesPerArea.Length];
            bubblesPerLine = new int[rec.linesPerArea.Length];
            for (int j = 0; j < rec.linesPerArea.Length; j++)
            {
                if (j > rec.bubblesPerLine.Length - 1)
                    break;
                linesPerArea[j] = rec.linesPerArea[j];
                bubblesPerLine[j] = rec.bubblesPerLine[j];
            }
            rec.areas = (RegionsArea[])e.Areas.Clone();
            bac = CreateNewBubblesAreaControl(rec.areas, rec.AmoutOfQuestions);
            DrawBubbleItems();
            VerifyButton.Enabled = true;
        }
        //-------------------------------------------------------------------------
        private void DrawBubbleItems(bool draw = true)
        {
            if (pictureBox1.Image.PixelFormat.ToString().EndsWith("Indexed"))
            {
                pictureBox1.Image = new Bitmap(pictureBox1.Image);
            }

            for (int k = 0; k < rec.BubbleItems.Count; k++)
            {
                if (draw)
                    DrawBubble(rec.BubbleItems[k].CheckedBubble);
                //Bubble b = (Bubble)(bac.CheckBoxArr[k]as Control).Tag;
                //rec.BubbleItems[k].Bubble.index = b.index;
                if (k >= bac.CheckBoxArr.Length)
                {
                    var arr = rec.BubbleItems.ToArray();
                    Array.Resize(ref arr, bac.CheckBoxArr.Length);
                    rec.BubbleItems = new ObservableCollection<BubbleItem>(arr);
                    return;
                }
                bac.lockReport = rec.BubbleItems[k].CheckedBubble.isChecked;
                bac.CheckBoxArr[k].Checked = rec.BubbleItems[k].CheckedBubble.isChecked;
            }
            bac.Refresh();
        }
        //-------------------------------------------------------------------------
        private void Recognize_ChangedBubble(object sender, BubbleEventArgs e)
        {
            try
            {
                Invoke(new MethodInvoker(delegate
                  {
                      if (!DarknessManualySet.Checked)
                      {
                          UpdateUI("nudPerCentEmptyBubble", (decimal)rec.DarknessPercent);
                          UpdateUI("nudPerCentBestBubble", (decimal)rec.DarknessDifferenceLevel);
                      }
                      //DateTime dt = DateTime.Now;
                      AddBubblesAreaControl(e);
                      //TimeSpan ts = DateTime.Now - dt;
                      bac.Refresh();

                      pictureBox1.Refresh();
                      //lblErr.Visible = false;
                      VerifyButton.Enabled = true;
                      //VerifyButton.Focus();
                      if (rec.BarCodesPrompt != "")
                      {
                          UpdateUI("StatusLabel", rec.BarCodesPrompt);
                          lblErr.Text = rec.BarCodesPrompt;
                          if (!errList.Contains(lblErr.Text))
                              errList.Add(lblErr.Text);

                          lblErr.Visible = true;
                      }
                      else
                      {
                          UpdateUI("StatusLabel", "Ready");
                      }
                      //comboBoxSetFocus();
                  }));
            }
            catch (Exception)
            {
            }
        }
        //-------------------------------------------------------------------------
        private void ShowImage(bool resize = true)
        {
            if (rec != null && rec.Bitmap != null)
            {
                //if (pictureBox1.Image == null)
                //    bmpPres = (Bitmap)rec.Bitmap.Clone();
                //GC.GetTotalMemory(false);//????
                pictureBox1.Image = (Bitmap)rec.Bitmap.Clone();// new Bitmap(rec.Bitmap);
                if (resize)// || rec.SheetIdentifier == "FLEX"
                    SizeFitButton_Click(null, null);//SizeFitButton.PerformClick();
                Refresh();
            }
        }
        //-------------------------------------------------------------------------
        private void BitmapRecognize(string fileName)
        {
            SetCancelSource();
            log.LogMessage("Recognize file: " + fileName);
            Bitmap b = null;
            try
            {
                b = (Bitmap)Bitmap.FromFile(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid file " + fileName
                    + Environment.NewLine
                    + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                rec = new Recognize(fileName, new OcrAppConfig(), new System.Threading.CancellationToken(), false);
                //rec.Audit.error = ex.Message;//rec.Audit == null
                if (MessageBox.Show(rec.Exception.Message + @"
Open processing file in explorer?"
                            , Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    btnOpenFile.PerformClick();
                return;
            }
            double widthPerHeightPatio = 0;
            try
            {
                widthPerHeightPatio = (double)b.Width / b.Height;
            }
            catch (Exception)
            {
            }
            if (widthPerHeightPatio == 0 || widthPerHeightPatio > 12000)
            {
                MessageBox.Show("Bad input image", Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                //rec = new Recognize(fileName, new OcrAppConfig(), new System.Threading.CancellationToken(), false, false);//для нормализации в основном потоке
                rec = new Recognize(fileName, new OcrAppConfig(), new System.Threading.CancellationToken(), false);
                b.Dispose();
                return;
            }
            OpenFilesDirButton.Enabled = true;
            button1.Enabled = true;
            SizeFitButton.Enabled = true;
            SizeFullButton.Enabled = true;
            SizePlusButton.Enabled = true;
            SizeMinusButton.Enabled = true;
            RotateLeftButton.Enabled = true;
            RotateRightButton.Enabled = true;
            backgroundWorker.RunWorkerAsync(new string[] { "new Recognize", fileName });
        }
        //-------------------------------------------------------------------------
        private void RecognizeAll()
        {
            if (cbDoNotProcess.Checked)
                return;
            //rec.BarCodesPrompt = "";//new???
            RecognizeAll(false);
        }
        //-------------------------------------------------------------------------
        private void RecognizeAll(bool alignmentOnly)
        {
            if (backgroundWorker.IsBusy)
                return;
            SetCancelSource();
            if (!isRotate && rec.Bitmap.Width > rec.Bitmap.Height)
                rec.Bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
            animatedTimer = new AnimatedTimer();
            animatedTimer.Tick += animatedTimer_Tick;
            rec.BubbleItems.Clear();
            BarCodeItems.Clear();
            //BubblesAC.Clear();
            pnlBubbles.Controls.Clear(); //
            bac = new BubblesAreaControl();
            rec.factRectangle = new Rectangle[0];
            VerifyButton.Enabled = false;
            rbtnGrid.Enabled = true;
            RecognizeAllButton.Enabled = false;
            RecognizeBubblesButton.Enabled = false;
            StopRecognizeButton.Enabled = true;

            UpdateUI("StatusLabel", "Search markers, please wait ...");
            ShowImage(false);

            var value = (BoxSheet.SelectedIndex == -1) ? "" : BoxSheet.SelectedItem.ToString();
            var text = (alignmentOnly) ? "RecAllAlignmentOnly" : "RecAll";
            backgroundWorker.RunWorkerAsync(new string[] { text, value });
            if (rec.BarCodesPrompt != "Rotate180")// && rpf != null
                ShowProcessingForm();
            else
                rec.BarCodesPrompt = "";
        }
        //-------------------------------------------------------------------------
        private void RecognizeBarcodes(string qrCodeText = "")
        {
            var selectedItem = MiniatureItems.First(x => x.Name == rec.SheetIdentifier);
            if (selectedItem != null)
            {
                BoxSheet.SelectedItem = selectedItem;
                miniatureList.SelectedItem = selectedItem;
                foreach (MiniatureListItemControl item in miniatureList.ControlList)
                {
                    if (item.Name == selectedItem.Name)
                        item.BackColor = SystemColors.ActiveCaption;
                    else
                        item.BackColor = SystemColors.Control;
                }
                //MiniatureListItemControl mc
                //    = (MiniatureListItemControl)miniatureList.ControlList[selectedItem.Name];
                //mc.BackColor = SystemColors.ActiveCaption;
                //mc.Refresh();
                //mc.MiniatureListItemControl_Select(mc, null);выбирает контролл
            }
            //if (!rec.barCodesPrompt.StartsWith("Markers"))
            //{
            UpdateUI("StatusLabel", "Barcodes recognition, please wait ...");
            //}
            //btnGrid.Enabled = false;
            backgroundWorker.RunWorkerAsync(new string[] { "BarcodesRecognition", qrCodeText });
        }
        //-------------------------------------------------------------------------
        private void RecognizeBubbles()
        {
            PrevRecognizeBubbles();
            rec.BubblesRecognition();
            backgroundWorker.RunWorkerAsync(new string[] { "BubblesRecognition" });
            //ShowProcessingForm();
        }
        //-------------------------------------------------------------------------
        private void PrevRecognizeBubbles(bool clear = true)
        {
            ShowImage(false);
            SetCancelSource();

            VerifyButton.Enabled = false;
            //btnGrid.Enabled = true;
            RecognizeAllButton.Enabled = false;
            RecognizeBubblesButton.Enabled = false;
            StopRecognizeButton.Enabled = true;
            if (clear)
            {
                rec.BubbleItems.Clear();
                pnlBubbles.Controls.Clear(); //BubblesAC.Clear();
                bac = new BubblesAreaControl();
                rec.factRectangle = new Rectangle[0];
            }
            animatedTimer.SetActiveValue(null);
            //AnimatedBubbleClear();
            UpdateUI("StatusLabel", "Bubbles recognition, please wait ...");
            Refresh();

            if (!DarknessManualySet.Checked)
            {
                UpdateUI("nudPerCentEmptyBubble", (decimal)rec.DarknessPercent);
                UpdateUI("nudPerCentBestBubble", (decimal)rec.DarknessDifferenceLevel);
            }
            rec.SetDarkness((double)nudPerCentEmptyBubble.Value, (double)nudPerCentBestBubble.Value);
        }
        #endregion
        //-------------------------------------------------------------------------
        #region BackgroundWorker
        private AbortableBackgroundWorker backgroundWorker = new AbortableBackgroundWorker();
        System.Threading.CancellationTokenSource cancelSource = new System.Threading.CancellationTokenSource();
        Dictionary<Bubble, CheckedBubble> maxCountRectangles;
        //-------------------------------------------------------------------------
        private void DoWork(object sender, DoWorkEventArgs e)
        {
            if (e.Argument != null)
            {
                var obj = e.Argument as string[];
                if (obj != null)
                {
                    string key = obj.First();
                    string qrCodeText;
                    if (obj.Length == 2)
                        qrCodeText = obj[1];
                    else
                        qrCodeText = "";
                    if (ShetIdManualySet && key == "RecAll")
                    {
                        key = "RecAllAlignmentOnly";
                    }

                    switch (key)
                    {
                        case "new Recognize":
                            Status = StatusMessage.NULL;
                            isInvert = false;
                            isRotate = false;
                            isCut = false;
                            isClear = false;
                            usedPrevTool = false;
                            ShetIdManualySet = false;
                            string fileName = obj.Last();
                            rec = new Recognize(fileName, defaults, cancelSource.Token);
                            rec.BubbleItems = new ObservableCollection<BubbleItem>();
                            errList.Clear();
                            if (rec != null && rec.Audit != null && !string.IsNullOrEmpty(rec.Audit.error))
                            {
                                Invoke(new MethodInvoker(delegate
                                {
                                    lblErr.Text = rec.Audit.error;
                                    errList.Add(rec.Audit.error);
                                    lblErr.Visible = true;
                                }));
                            }
                            recBitmap = (Bitmap)rec.Bitmap.Clone();
                            Invoke(new MethodInvoker(delegate
                                {
                                    BoxSheet.SelectedIndex = -1;
                                }));
                            break;

                        case "RecAll":
                            //Invoke(new MethodInvoker(delegate
                            //    {
                            //        BoxSheet.SelectedIndex = -1;
                            //    }));

                            if (string.IsNullOrEmpty(obj.Last()) && !string.IsNullOrEmpty(lastSheetId))
                            {
                                rec.RecAll(lastSheetId, false, ref qrCodeText, isRotate, isCut, ShetIdManualySet);
                            }
                            else
                            {
                                rec.RecAll(obj.Last(), false, ref qrCodeText, isRotate);
                            }
                            if (rec.cancellationToken.IsCancellationRequested)
                                Invoke(new MethodInvoker(delegate
                                    {
                                        if (!isRotate)
                                            btnRestore_Click(null, null);
                                    }));
                            break;
                        case "RecAllAlignmentOnly":
                            rec.RecAll(obj.Last(), true, ref qrCodeText, isRotate, isCut, ShetIdManualySet);
                            break;
                        case "SelectedSheetIdentifier":
                            rec.SelectedSheetIdentifier(obj.Last(), ref qrCodeText, isRotate, isCut, ShetIdManualySet);
                            break;
                        case "BarcodesRecognition":
                            rec.BarcodesRecognition(qrCodeText);
                            break;
                        case "BubblesRecognition":
                            CreateEtalonAreas();
                            GetAreasSettings();
                            if (!string.IsNullOrEmpty(rec.Audit.error) && rec.Audit.error.StartsWith("Calibration"))
                            {
                                //if (rpf != null && rpf.Disposing)
                                //CloseProcessingForm();
                                rec.Audit.error = "";
                                Invoke(new MethodInvoker(delegate
                                {
                                    rbtnGrid.PerformClick();
                                }));
                                break;
                            }
                            if (rec.bubbles_per_lineErr)
                            {
                                if (rpf != null && rpf.Disposing)
                                    CloseProcessingForm();
                                MessageBox.Show("Please, specify correctly the bubbles per line"
                                , Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                break;
                            }
                            else// (!rec.bubbles_per_lineErr)
                                rec.RecAndFindBubble();
                            break;
                        case "BubblesRecognition2":
                            rec.Exception = null;
                            rec.BarCodesPrompt = "";
                            //rec.Status = RecognizeAction.SearchBublesFinished;
                            //var maxCountRectangles = rec.AddMaxCountRectangles();
                            //rec.BubblesRecognition();
                            int deltaX = 0;
                            //if (rec.bubbles_per_lineFLEX != 5)
                            //{
                            //    deltaX = 0;
                            //}

                            recTools.BubblesRecognize
                            (
                              ref rec.allContourMultiLine
                            , ref rec.factRectangle
                            , rec.Bitmap
                            , ref barCodesPrompt
                            , rec.filterType
                            , rec.defaults.SmartResize
                            , rec.bubblesRegions
                            , rec.bubblesOfRegion
                            , rec.bubblesSubLinesCount
                            , rec.bubblesSubLinesStep
                            , rec.bubblesPerLine
                            , rec.lineHeight
                            , rec.linesPerArea
                            , rec.answersPosition
                            , rec.indexAnswersPosition
                            , rec.totalOutput
                            , rec.bubbleLines
                            , rec.regions
                            , rec.areas
                            , 0, 0, 0, 0//x1, x2, y1, y2
                            , rec.kx, rec.ky
                            , rec.curRect
                            , rec.etRect
                            , 0//deltaY
                            , rec.AmoutOfQuestions
                            , rec.IndexOfFirstQuestion
                            , maxCountRectangles
                            , rec.darknessPercent
                            , rec.darknessDifferenceLevel
                            , rec.lastBannerBottom
                            , deltaX
                            );
                            rec.FillBubbleItems(maxCountRectangles);//
                            rec.FillBubbleItemsRectangle(rec.allContourMultiLine, rec.factRectangle);
                            if (rec.Exception != null)
                            {
                                //MessageBox.Show
                                //            ("Please check that you have the correct number of columns and sub rows"
                                //            , Text, MessageBoxButtons.OK
                                //            , MessageBoxIcon.Exclamation
                                //            );
                                //Invoke(new MethodInvoker(delegate
                                //{
                                //    CloseProcessingForm();
                                //}));
                                return;
                            }
                            rec.FindBubble(rec.factRectangle, rec.allContourMultiLine, true);
                            if (rec.Exception != null)
                                return;
                            if (bac.CheckBoxArr.Length == 0)
                            {
                                //AddBubblesAreaControl(new BubbleEventArgs(false, rec.BubbleItems, rec.areas, rec.AmoutOfQuestions, rec.maxAmoutOfQuestions, rec.IndexOfFirstQuestion, rec.linesPerArea, rec.bubblesPerLine));
                                GetAreasSettings();
                                Invoke(new MethodInvoker(delegate
                                {
                                    bac = CreateNewBubblesAreaControl(rec.areas, rec.AmoutOfQuestions);
                                    bac.lockReport = true;
                                    DrawBubbleItems();
                                    bac.lockReport = false;
                                }));
                            }

                            Invoke(new MethodInvoker(delegate
                                        {
                                            if (!string.IsNullOrEmpty(barCodesPrompt))
                                            {
                                                rec.BarCodesPrompt = barCodesPrompt;
                                                if (!errList.Contains(rec.BarCodesPrompt))
                                                    errList.Add(rec.BarCodesPrompt);
                                                lblErr.Visible = true;
                                            }
                                            pictureBox1.Refresh();
                                            VerifyButton.Enabled = true;
                                            //Status = StatusMessage.NULL;
                                            rec.Status = RecognizeAction.NULL;
                                        }));
                            //rec.UpdateGui();
                            break;
                        case "RemoveNoise":
                            //cancelSource = new System.Threading.CancellationTokenSource();
                            rec.Bitmap = recTools.RemoveNoise(rec.Bitmap);
                            pictureBox1.Image = (Bitmap)rec.Bitmap.Clone();
                            break;
                    }
                    //if (qrCodeText == "")
                    //    e.Result = key;
                    //else
                    e.Result = new string[2] { key, qrCodeText };
                }
            }
        }
        //-------------------------------------------------------------------------
        public void GetAreasSettings()
        {
            if (etalonAreas.Length == 0)
                CreateEtalonAreas();
            linesPerArea = new int[etalonAreas.Length];// [rec.areas.Length];
            bubblesPerLine = new int[etalonAreas.Length];//[rec.areas.Length];
            if (rec.areas.Length != etalonAreas.Length)
            {
                rec.areas = (RegionsArea[])etalonAreas.Clone();
            }
            if (rec.bubblesPerLine.Length == 0)
            {
                rec.bubblesPerLine = new int[etalonAreas.Length];
                for (int j = 0; j < etalonAreas.Length; j++)
                {
                    rec.bubblesPerLine[j] = etalonAreas[j].bubblesPerLine;
                }
            }
            for (int j = 0; j < etalonAreas.Length; j++)
            {
                linesPerArea[j] = rec.linesPerArea[j];
                bubblesPerLine[j] = rec.bubblesPerLine[j];
            }
        }
        //-------------------------------------------------------------------------
        private void Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (rec == null)
                return;
            ioHelper.ReDelete();
            if (rec.Exception != null && rec.Exception.Message.ToLower().IndexOf("file") > 0)
            {
                if (MessageBox.Show(rec.Exception.Message + @"
Open processing file in explorer?"
                            , Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    btnOpenFile.PerformClick();
                return;
            }
            if (rec.BarCodesPrompt == "Rotate180")
            {
                Application.DoEvents();
                rec.headers = new string[0];
                rec.headersValues = new string[0];
                rec.allBarCodeNames = new string[0];
                rec.allBarCodeValues = new string[0];
                rec.totalOutput = new object[0];
                rec.Bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                ShowImage(true);
                BarCodeItems.Clear();
                barCodeList.ControlList.Clear();
                rec.BarCodeItems.Clear();
                isRotate = true;
                RecognizeAll(true);
                return;
            }
            btnGrid.Enabled = BoxSheet.SelectedIndex != -1 && !string.IsNullOrEmpty(rec.SheetIdentifier);
            try
            {
                if (rec.DarknessPercent.Equals(null))
                    btnGrid.Enabled = false;
            }
            catch (Exception)
            {
                btnGrid.Enabled = false;
            }

            if (cancelSource.IsCancellationRequested)
            {
                RecognizeAllButton.Enabled = true;
                RecognizeBubblesButton.Enabled = false;
                StopRecognizeButton.Enabled = false;

                //VerifyButton.Enabled = false;

                rec.Status = RecognizeAction.Cancelled;
                UpdateUI("StatusLabel", "Recognizing was cancelled");
                CloseProcessingForm();

                if (Status == StatusMessage.Delete)
                {
                    DeleteButton.PerformClick();
                    return;
                }
                if (Status == StatusMessage.Next)
                {
                    NextButton.PerformClick();
                    return;
                }
                pictureBox1.Image = (Bitmap)rec.Bitmap.Clone();//
                ShowImage();

                if (appSettings.Fields.UsePrevTool && !usedPrevTool)
                {
                    foreach (var item in editorTools)
                    {
                        if (item.BackColor == Color.Cyan)
                        {
                            toolTip2.ToolTipIcon = ToolTipIcon.Info;
                            toolTip2.IsBalloon = true;
                            toolTip2.SetToolTip(item, "Used this tool");
                            toolTip2.ToolTipTitle = "Warning";
                            toolTip2.Show("Used this tool", item, 2000);
                            if (item.Name == "btnRemoveNoise")
                                continue;

                            //toolTip2.Hide(item);
                            Button b = item as Button;
                            if (b != null)
                            {
                                usedPrevTool = true;
                                b.PerformClick();
                                return;
                            }
                            RadioButton r = item as RadioButton;
                            if (r != null)
                            {
                                usedPrevTool = true;
                                r.Checked = true;
                                return;
                            }
                        }
                    }
                }

                if (rec != null && (BoxSheet.SelectedIndex == -1)
               && !ShetIdManualySet && appSettings.Fields.ChbSheetId && lastSheetId != "")
                {
                    rec.SheetIdentifier = lastSheetId;
                    var selectedItem = MiniatureItems.First(x => x.Name == rec.SheetIdentifier);
                    if (selectedItem != null)
                    {
                        BoxSheet.SelectedItem = selectedItem;
                        miniatureList.SelectedItem = selectedItem;
                        rec.SheetIdentifier = selectedItem.ToString();
                    }
                }
                else if (BoxSheet.SelectedIndex == -1 & !rec.Status.Equals(RecognizeAction.Cancelled))
                {
                    BoxSheet.comboBox1.DroppedDown = true;
                    //BoxSheet.comboBox1.SelectedItem = 0;
                }
                return;
            }
            if (e.Error != null)
            {
                log.LogMessage(e.Error);
                VerifyButton.Enabled = false;

                RecognizeAllButton.Enabled = true;
                RecognizeBubblesButton.Enabled = false;
                StopRecognizeButton.Enabled = false;

                var text = (rec != null) ? Path.GetFileName(rec.FileName) : "";
                string message = "Error in recognizing. Send this sheet (" + text + ") to developers for fixing problem.";
                UpdateUI("StatusLabel", message);
                CloseProcessingForm();
                UpdateUI("Exception", message);
                return;
            }
            if (e.Result != null)
            {
                var key = e.Result as string[];
                if (key == null || String.IsNullOrEmpty(key[0])) return;
                switch (key[0])
                {
                    case "new Recognize":
                        if (rec != null && rec.Exception != null)
                        {
                            DeleteToNextProccessingFolder(); //что сделать с файлом??????? 
                            return;
                        }
                        ShowImage();
                        RecognizeAllButton.Enabled = true;
                        if (rec.Audit != null && !string.IsNullOrEmpty(rec.Audit.error))
                        {
                            if (!rec.Audit.error.StartsWith("M") && !rec.Audit.error.StartsWith("S"))
                            {
                                rec.Status = RecognizeAction.WaitingForUserResponse;
                                rec.InitParamsBeforeSearchmarkers();
                                if (appSettings.Fields.ChbSheetId && !string.IsNullOrEmpty(lastSheetId))
                                {
                                    rec.Status = RecognizeAction.WaitingForUserResponse;

                                    var selectedItem = MiniatureItems.First(x => x.Name == lastSheetId);// rec.SheetIdentifier);
                                    rec.SheetIdentifier = lastSheetId;
                                    rec.LastSheetIdentifier = lastSheetId;
                                    if (selectedItem != null)
                                    {
                                        rec.SheetIdentifier = lastSheetId;
                                        BoxSheet.SelectedItem = selectedItem;
                                    }
                                    return;
                                }
                                //else if (rec.Audit.error.StartsWith("M") || rec.Audit.error.StartsWith("S"))
                                //{
                                //    SetUsePrevTool();
                                //    return;
                                //}
                            }
                            else
                            {
                                SetUsePrevTool();
                                rec.Status = RecognizeAction.WaitingForUserResponse;
                                return;
                            }
                        }
                        if (rec.Audit == null || string.IsNullOrEmpty(rec.Audit.error)
                            || (!rec.Audit.error.StartsWith("M") || !rec.Audit.error.StartsWith("S")))
                            RecognizeAll();
                        //else
                        //    RecognizeAllButton.Enabled = true;
                        break;
                    case "RecAll":
                        ShowImage();
                        if (rec.Status != RecognizeAction.SearchMarkersFinished) //what buttons enabled?
                        {
                            if (rec.regions == null || !string.IsNullOrEmpty(rec.QrCode))
                            {
                                if (rec.BarCodesPrompt == "")
                                    rec.BarCodesPrompt = "Sheet identifier is not recognized";
                                if (appSettings.Fields.UsePrevTool)
                                {
                                    foreach (var item in editorTools)
                                    {
                                        if (item.BackColor == Color.Cyan)
                                        {
                                            toolTip2.ToolTipIcon = ToolTipIcon.Info;
                                            toolTip2.IsBalloon = true;
                                            toolTip2.SetToolTip(item, "Used this tool");
                                            toolTip2.ToolTipTitle = "Warning";
                                            //toolTip2.Show("Used this tool", item
                                            //    , new Point (item.Bounds.X,item.Bounds.Y), 2000);
                                            toolTip2.Show("Used this tool", item, 2000);
                                            if (item.Name == "btnRemoveNoise")
                                                continue;

                                            //toolTip2.Hide(item);
                                            Button b = item as Button;
                                            if (b != null)
                                            {
                                                b.PerformClick();
                                                return;
                                            }
                                            RadioButton r = item as RadioButton;
                                            if (r != null)
                                            {
                                                r.Checked = true;
                                                return;
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                            else
                                return;
                        }

                        if (rec.regions != null && errList.Contains("Sheet identifier is not recognized"))
                        {
                            errList.Remove("Sheet identifier is not recognized");
                            VerifyErrList();
                        }

                        if (rec.SheetIdentifier == "FLEX")
                        {
                            SizeFitButton_Click(null, null);
                        }
                        RecognizeBarcodes(key[1]);
                        break;
                    case "RecAllAlignmentOnly":
                        if (rec.SheetIdentifier == "FLEX")
                            ShowImage();
                        else
                            ShowImage(false);
                        if (rec.Status != RecognizeAction.SearchMarkersFinished) //what buttons enabled?
                            return;
                        RecognizeBarcodes(key[1]);
                        break;
                    case "SelectedSheetIdentifier":
                        RecognizeBarcodes();
                        break;
                    case "BarcodesRecognition":
                        RecognizeBubbles();
                        break;
                    case "BubblesRecognition":
                    case "BubblesRecognition2":
                        RecognizeAllButton.Enabled = true;
                        RecognizeBubblesButton.Enabled = true;
                        StopRecognizeButton.Enabled = false;
                        //CloseProcessingForm();
                        if (key[0] == "BubblesRecognition2" && (rec.Exception != null || !string.IsNullOrEmpty(rec.BarCodesPrompt)))
                        {
                            if (rec.BarCodesPrompt.StartsWith("Calibration error"))
                            {
                                MessageBox.Show
                                            ("Area(s) error"
                                            , Text, MessageBoxButtons.OK
                                            , MessageBoxIcon.Exclamation
                                            );
                            }
                            else
                            {
                                MessageBox.Show
                                            ("Please check that you have the correct number of columns and sub rows"
                                            , Text, MessageBoxButtons.OK
                                            , MessageBoxIcon.Exclamation
                                            );
                            }
                            CloseProcessingForm();
                            return;
                        }
                        CloseProcessingForm();
                        if (rec.BarCodesPrompt != "")
                        {
                            if (!errList.Contains(rec.BarCodesPrompt))
                                errList.Add(rec.BarCodesPrompt);
                            lblErr.Visible = true;
                        }
                        break;
                    case "RemoveNoise":
                        if (appSettings.Fields.RecAfterCut)// && rec.BarCodesPrompt == ""
                        {
                            if (BarCodeItems != null && BarCodeItems.Count > 0)
                            {
                                bool verify = true;
                                for (int j = 0; j < BarCodeItems.Count; j++)
                                {
                                    BarCodeItem b = BarCodeItems[j] as BarCodeItem;
                                    if (!b.Verify)
                                    {
                                        verify = false;
                                        break;
                                    }
                                }
                                if (verify)
                                {
                                    RecognizeBubblesButton.PerformClick();
                                }
                                else
                                    RecognizeAllButton.PerformClick();
                            }
                            else
                                RecognizeAllButton.PerformClick();
                        }
                        else
                        {
                            CloseProcessingForm();
                        }
                        break;
                }
            }
            else if (rec.Exception != null && linsForm != null && linsForm.Visible == true)
            {
                CloseProcessingForm();
                Application.DoEvents();
                if (rec.BarCodesPrompt.StartsWith("Calibration error"))
                {
                    MessageBox.Show
                 (
                   rec.BarCodesPrompt
                 , Text, MessageBoxButtons.OK
                 , MessageBoxIcon.Exclamation
                 );
                }
                else
                {
                    MessageBox.Show
                 ("Please check that you have the correct number of columns and sub rows"
                 , Text, MessageBoxButtons.OK
                 , MessageBoxIcon.Exclamation
                 );
                }
                return;
            }
            if (Status == StatusMessage.ChangeIndexOfFirstQuestion || Status == StatusMessage.Verify)
            {
                if (rec.SheetIdentifier == "FLEX")
                    AplyValueBarCode("question_number_1");
                else
                    AplyValueBarCode("index_of_first_question");
            }

            if (Status == StatusMessage.ChangeAmoutOfQuestions || Status == StatusMessage.Verify)
                AplyValueBarCode("amout_of_questions");

            if (Status == StatusMessage.Verify)
                VerifyButton.PerformClick();
            if (rec != null && (rec.BarCodesPrompt != null && (rec.BarCodesPrompt.StartsWith("M")
                || rec.BarCodesPrompt.StartsWith("S")))
                && !ShetIdManualySet && appSettings.Fields.ChbSheetId && lastSheetId != "")
            {
                rec.SheetIdentifier = lastSheetId;
                var selectedItem = MiniatureItems.First(x => x.Name == rec.SheetIdentifier);
                if (selectedItem != null)
                {
                    BoxSheet.SelectedIndex = -1;
                    BoxSheet.SelectedItem = selectedItem;
                    miniatureList.SelectedItem = selectedItem;
                    rec.SheetIdentifier = selectedItem.ToString();
                }

                if (rec.BarCodesPrompt.StartsWith("M"))
                {
                    rec.Status = RecognizeAction.WaitingForUserResponse;// .SearchMarkersFinished
                    if (linsForm == null)
                        CreateLinsForm();
                    else
                        InitLinsForm(true);
                    foreach (BarCodeListItemControl item in barCodeList.ControlList)
                    {
                        if (item.Item.Name == "student_uid" || item.Item.Name == "student_id")
                            continue;
                        else if (appSettings.Fields.AmoutOfQuestions && item.Item.Name == "amout_of_questions")
                            item.Item.Value = lastAmoutOfQuestions;
                        else if (appSettings.Fields.DistrictId && item.Item.Name == "district_id")
                            item.Item.Value = lastDistrictId;
                        else if (appSettings.Fields.IndexOfFirstQuestion
                            && (item.Item.Name == "index_of_first_question" || item.Item.Name == "question_number_1"))
                            item.Item.Value = lastIndexOfQuestion;
                        item.comboBox1.ForeColor = Color.Red;
                    }
                }
                btnGrid.Enabled = true;
                rbtnGrid.Checked = true;
                btnGrid_Click(null, null);
                CloseProcessingForm();
                StopRecognizeButton.Enabled = true;
            }
        }
        //-------------------------------------------------------------------------
        public void UncheckRbtn(object sender, EventArgs e)
        {
            RadioButton rb = null;
            if (sender != null)
                rb = (RadioButton)sender;
            barCodeSel = false;
            area = Rectangle.Empty;
            areaNaturalSize = Rectangle.Empty;
            if (rb != null && !rb.Checked)
                return;
            foreach (BarCodeListItemControl item in barCodeList.ControlList)
            {
                if (item.radioButton1 == rb || item.rbtnText == rb)
                {
                    SelectedBarCodeItem = item;
                    barCodeSel = true;
                    continue;
                }
                item.radioButton1.Checked = false;
                item.rbtnText.Checked = false;
            }
            if (linsForm != null)
                linsForm.rbtnGrid_CheckedChanged(null, null);
            //rec.Status = RecognizeAction.Grid;
        }
        //-------------------------------------------------------------------------
        private void AplyValueBarCode(string name)
        {
            try
            {
                if (Status == StatusMessage.ChangeAmoutOfQuestions || Status == StatusMessage.ChangeIndexOfFirstQuestion)
                    Status = StatusMessage.NULL;
                var item = BarCodeItems.First(x => x.Name == name);
                VerifyBarCode(item);
            }
            catch (Exception)
            {
            }
        }
        #endregion
        //-------------------------------------------------------------------------
        #region File & Folders
        private bool VerifyDirectory(string dir)
        {
            if (iOHelper.CreateDirectory(dir))
                return true;
            else
            {
                var message = "Could not find part of the path " + dir;
                //Utils.SendMail(message);
                UpdateUI("Exception", message);
                return false;
            }
        }
        //-------------------------------------------------------------------------
        private string GetFileForRecognizeFromFolders(string tempdirectory, params string[] directory)
        {
            var listDir = directory.ToList();
            listDir.Insert(0, tempdirectory);
            foreach (var dir in listDir)
            {
                if (string.IsNullOrEmpty(dir) || dir != OcrAppConfig.TempEdFolder)//!!! || dir != OcrAppConfig.TempEdFolder <=test
                    continue;
                if (VerifyDirectory(dir))
                {
                    string[] fnArr;// = utils.GetSupportedFilesFromDirectory(dir, SearchOption.AllDirectories);
                    if (dir == OcrAppConfig.TempEdFolder)
                    {
                        fnArr = utils.GetSupportedFilesFromDirectory(dir, SearchOption.TopDirectoryOnly);
                    }
                    else
                    {
                        fnArr = utils.GetSupportedFilesFromDirectory(dir, SearchOption.AllDirectories);
                    }
                    //fnArr.Reverse();
                    //if (dir == OcrAppConfig.TempEdFolder)
                    //{
                    //    Array.Reverse(fnArr);
                    //}
                    if (fnArr.Count() > 0)
                    {
                        foreach (var file in fnArr)
                        {
                            if (!ioHelper.ReDeleteList.Contains(file))
                            {
                                if (utils.CanAccess(file))
                                {
                                    return utils.GetFileForRecognize(file, tempdirectory);//Процесс не может получить доступ
                                }
                                else
                                {
                                    log.LogMessage("Can not access to file: " + file);
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
        #endregion
        //-------------------------------------------------------------------------
        #region Modal Window
        public RecognizeProcessingForm rpf;
        private void ShowProcessingForm()
        {
            if (rpf != null && !rpf.IsDisposed && rpf.Visible && rpf.IsHandleCreated)
                return;
            //rpf = null;
            //Application.DoEvents();
            //if (backgroundWorker == null || backgroundWorker.workerThread == null)
            //    return;
            //if (rpf == null || rpf.IsDisposed)
            //{
            rpf = new RecognizeProcessingForm();
            rpf.FormClosing += rpf_FormClosed;
            rpf.Owner = this;
            rpf.LabelTextBox.Text = StatusLabel.Text;
            //}
            //if (!rpf.IsHandleCreated)
            //{
            rpf.ShowDialog();
            //}
            //}
            //else
            //{
            //    rpf.Visible = true;
            //}
        }
        //-------------------------------------------------------------------------
        private void rpf_FormClosed(object sender, FormClosingEventArgs e)
        {
            if (StopRecognizeButton.Enabled)
                StopRecognizeButton_Click(null, null);
            //StopRecognizeButton.PerformClick();
            RecognizeAllButton.Enabled = true;
            var f = sender as RecognizeProcessingForm;
            if (f != null)//rec.Status == RecognizeAction.Cancelled || rec.Status == RecognizeAction.SearchBublesFinished && )
            {
                //f.Owner = null;
                //Focus();
                //f.Dispose();
                f = null;
            }
            //rpf.Dispose();
            //rpf = null;//.Close();
        }
        //-------------------------------------------------------------------------
        private void CloseProcessingForm()
        {
            if (rpf != null)// && !rpf.IsDisposed
            {
                //this.Focus();
                rpf.Close();
                //SetUsePrevTool();
                if (!string.IsNullOrEmpty(rec.BarCodesPrompt) && appSettings.Fields.UsePrevTool)
                {
                    foreach (var item in editorTools)
                    {
                        if (item.BackColor == Color.Cyan)
                        {
                            toolTip2.ToolTipIcon = ToolTipIcon.Info;
                            toolTip2.IsBalloon = true;

                            toolTip2.SetToolTip(item, "Used this tool");
                            toolTip2.ToolTipTitle = "Warning";
                            //toolTip2.Show("Used this tool", item
                            //    , new Point (item.Bounds.X,item.Bounds.Y), 2000);
                            toolTip2.Show("Used this tool", item, 2000);

                            if (item.Name == "btnRemoveNoise")
                                continue;
                            //toolTip2.Hide(item);
                            Button b = item as Button;
                            if (b != null)
                            {
                                b.PerformClick();
                                return;
                            }
                            RadioButton r = item as RadioButton;
                            if (r != null)
                            {
                                r.Checked = true;
                                return;
                            }
                        }
                    }
                }


                if (rbtnGrid.Enabled)
                {
                    if (BoxSheet.SelectedIndex == -1)
                    {
                        rec.SheetIdentifier = "";
                    }
                    else
                    {
                        if (rec.Status == RecognizeAction.Cancelled
                            || rec.BarCodesPrompt.StartsWith("A")
                            || rec.BarCodesPrompt.StartsWith("M"))
                        {
                            Status = StatusMessage.NULL;
                            etalonAreas = new RegionsArea[0];
                            rbtnGrid.Checked = true;
                            btnGrid_Click(null, null);
                        }
                        //rpf.Dispose();
                        //rpf = null;
                        if (animatedTimer != null)
                            animatedTimer.StartAnimation();
                        comboBoxFocused = false;
                    }
                }
                else
                {
                    if (!ShetIdManualySet && appSettings.Fields.ChbSheetId && lastSheetId != "")
                    {
                        rec.SheetIdentifier = lastSheetId;
                        var selectedItem = MiniatureItems.First(x => x.Name == rec.SheetIdentifier);
                        if (selectedItem != null)
                        {
                            BoxSheet.SelectedItem = selectedItem;
                            miniatureList.SelectedItem = selectedItem;
                        }
                        //regions = recTools.GetRegions(lastSheetId, rec.regionsList);
                        //rec.regions = regions;
                        rbtnGrid.Enabled = true;
                        rbtnGrid.PerformClick();
                    }
                }
            }
            rpf = null;
        }

        private void SetUsePrevTool()
        {
            //if (!string.IsNullOrEmpty(rec.BarCodesPrompt) && appSettings.Fields.UsePrevTool)
            //{
            foreach (var item in editorTools)
            {
                if (item.BackColor == Color.Cyan)
                {
                    toolTip2.ToolTipIcon = ToolTipIcon.Info;
                    toolTip2.IsBalloon = true;

                    toolTip2.SetToolTip(item, "Used this tool");
                    toolTip2.ToolTipTitle = "Warning";
                    //toolTip2.Show("Used this tool", item
                    //    , new Point (item.Bounds.X,item.Bounds.Y), 2000);
                    toolTip2.Show("Used this tool", item, 2000);

                    if (item.Name == "btnRemoveNoise")
                        continue;
                    //toolTip2.Hide(item);
                    Button b = item as Button;
                    if (b != null)
                    {
                        b.PerformClick();
                        return;
                    }
                    RadioButton r = item as RadioButton;
                    if (r != null)
                    {
                        r.Checked = true;
                        return;
                    }
                }
            }
            //}
        }

        ////-------------------------------------------------------------------------
        //[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        //internal static extern IntPtr GetFocus();
        ////-------------------------------------------------------------------------
        //private Control GetFocusedControl()
        //{
        //    Control focusedControl = null;
        //    // To get hold of the focused control:
        //    IntPtr focusedHandle = GetFocus();
        //    if (focusedHandle != IntPtr.Zero)
        //        // Note that if the focused Control is not a .Net control, then this will return null.
        //        focusedControl = Control.FromHandle(focusedHandle);
        //    return focusedControl;
        //}
        //-------------------------------------------------------------------------
        private void comboBoxSetFocus()
        {
            if (rpf != null && rpf.Disposing)
                return;
            BarCodeItem item = null;
            for (int i = 0; i < BarCodeItems.Count; i++)
            {
                item = BarCodeItems[i];
                BarCodeListItemControl itm = (BarCodeListItemControl)barCodeList.ControlList[i];
                if (!item.Verify)
                {
                    if (itm != null)
                    {
                        itm.comboBox1.Focus();
                        if (itm.comboBox1.Focused)
                        {
                            comboBoxFocused = true;
                            itm.textChanged = true;
                        }
                        break;
                    }
                }
                if (item.Verify)
                    comboBoxFocused = true;
            }
            if ((item != null && item.Verify) && rec.BarCodesPrompt == "")
                VerifyButton.Focus();
            //else if (item != null)
            //    rbtnGrid.Focus();//.PerformClick();
        }
        #endregion
        //-------------------------------------------------------------------------
        #region Send mail to Support
        private void SendToSupport()
        {
            try
            {
                List<string> attachments = new List<string>();
                var fileNameScreenShot = MakeScreenShot();
                attachments.Add(fileNameScreenShot);
                if (sendToSupport)
                {
                    var logFileName = log.GetLogFileName();
                    attachments.Add(logFileName);
                    if (rec != null)
                    {
                        var fileName = Path.Combine(OcrAppConfig.TempEdFolder, Path.GetFileName(rec.FileName));
                        var fileNameAudit = utils.GetFileAuditName(fileName);
                        attachments.Add(fileName);
                        attachments.Add(fileNameAudit);
                    }
                }

                sts = new SendToSupportForm();
                sts.Owner = this;
                sts.btnSameAsPrevious.Click += new EventHandler(btnSameAsPrevious_Click);
                sts.Email = "help@edoctrina.org";
                if (sendToSupport)
                {
                    sts.Text = "Issue discovered";
                    sts.Email = "dev@edoctrina.org";
                    //if (appSettings != null && !String.IsNullOrWhiteSpace(appSettings.Fields.SendToSupportEmail))
                    //{
                    //    sts.Email = appSettings.Fields.SendToSupportEmail;
                    //}
                    sts.SubjectTextBox.Text = "Issue discovered";
                }
                else
                {
                    sts.Text = "Problem discovered";
                    sts.EmailTextBox.Enabled = false;
                    sts.SubjectTextBox.Text = "Problem discovered";

                    //if (appSettings != null && !String.IsNullOrWhiteSpace(appSettings.Fields.ReportTestPageEmail))
                    //{
                    //    if (appSettings != null && !String.IsNullOrWhiteSpace(appSettings.Fields.ReportTestPageEmail))
                    //    {
                    //        sts.Email = appSettings.Fields.ReportTestPageEmail;
                    //    }
                    //}
                    if (rec != null)
                    {
                        var fileName = Path.Combine(OcrAppConfig.TempEdFolder, Path.GetFileName(rec.FileName));
                        FileInfo fi = new FileInfo(fileName);
                        string newFileName = "Report test page" + fi.Extension;
                        File.Copy(fileName, newFileName, true);
                        attachments.Add(newFileName);
                    }
                }
                //sts.CommentTextBox.Focus();
                sts.ShowDialog();

                if (sts.DialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    SendMailAsync(sts.Message, sts.Email, attachments, "Answers scanning - " + sts.SubjectTextBox.Text);
                    if (!sendToSupport)
                        DeleteButton.PerformClick();
                }

                //if (appSettings != null && !String.IsNullOrWhiteSpace(sts.Email))
                //{
                //if (sendToSupport)
                //{
                //    appSettings.Fields.SendToSupportEmail = sts.Email;
                //}
                //else
                //{
                //    appSettings.Fields.ReportTestPageEmail = sts.Email;
                //}
                if (sendToSupport)
                {
                    appSettings.Fields.SendToSupportEmail = sts.SubjectTextBox.Text;
                }
                else
                {
                    appSettings.Fields.ReportTestPageEmail = sts.SubjectTextBox.Text;
                }

                appSettings.Save();
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //-------------------------------------------------------------------------
        void btnSameAsPrevious_Click(object sender, EventArgs e)
        {
            if (sendToSupport)
            {
                if (string.IsNullOrEmpty(appSettings.Fields.SendToSupportEmail))
                {
                    sts.SubjectTextBox.Text = "";
                }
                else
                {
                    sts.SubjectTextBox.Text = appSettings.Fields.SendToSupportEmail;//ReportTestPageEmail
                }

            }
            else
            {
                if (string.IsNullOrEmpty(appSettings.Fields.ReportTestPageEmail))
                {
                    sts.SubjectTextBox.Text = "";
                }
                else
                {
                    sts.SubjectTextBox.Text = appSettings.Fields.ReportTestPageEmail;//ReportTestPageEmail
                }
            }
        }
        //-------------------------------------------------------------------------
        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        public enum DeviceCap
        {
            VERTRES = 10,
            DESKTOPVERTRES = 117,
            PHYSICALOFFSETY = 113,
            /// <summary>
            /// Scaling factor x
            /// </summary>
            SCALINGFACTORX = 114,
            /// <summary>
            /// Scaling factor y
            /// </summary>
            SCALINGFACTORY = 115,
            // http://pinvoke.net/default.aspx/gdi32/GetDeviceCaps.html
        }
        //-------------------------------------------------------------------------
        private string MakeScreenShot()
        {
            if (!Directory.Exists(OcrAppConfig.LogsFolder))
            {
                Directory.CreateDirectory(OcrAppConfig.LogsFolder);
            }
            var screenShotPath = Path.Combine(OcrAppConfig.LogsFolder, "screenshot_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg");
            //Bitmap image = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = g.GetHdc();
            var LogicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
            var PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);
            float ScreenScalingFactor = (float)PhysicalScreenHeight / (float)LogicalScreenHeight;
            int width = (int)Math.Round((float)this.Size.Width * ScreenScalingFactor);
            int height = (int)Math.Round((float)this.Size.Height * ScreenScalingFactor);
            //int sfx = GetDeviceCaps(desktop, (int)DeviceCap.SCALINGFACTORX);
            //int sfy = GetDeviceCaps(desktop, (int)DeviceCap.PHYSICALOFFSETY);
            Size ScreenSize = new Size(width, height);
            Bitmap image = new Bitmap(ScreenSize.Width, ScreenSize.Height);
            g = Graphics.FromImage(image);
            g.CopyFromScreen(this.Location, Point.Empty, ScreenSize);
            //g.CopyFromScreen(Point.Empty, Point.Empty, new Size(Screen.PrimaryScreen.Bounds.Width
            //    , Screen.PrimaryScreen.Bounds.Height));
            image.Save(screenShotPath, System.Drawing.Imaging.ImageFormat.Jpeg);
            g.Dispose();
            return screenShotPath;
        }
        //-------------------------------------------------------------------------
        private void SendMailAsync(string message, string email, List<string> attachments, string caption = "")
        {
            System.Threading.Tasks.Task.Factory.StartNew(status =>
            {
                MailHelper mail = new MailHelper();
                mail.Attachments = attachments;
                if (caption == "")
                {
                    if (sendToSupport)
                    {
                        mail.SendMailToSupport(message, email);
                    }
                    else
                    {
                        mail.SendMailToSupport(message, email, "Report test page");
                    }
                }
                else
                {
                    mail.SendMailToSupport(message, email, caption);
                }
            }, attachments.First()).ContinueWith((t) => Completed(t));
        }
        //-------------------------------------------------------------------------
        private void Completed(System.Threading.Tasks.Task t)
        {
            try
            {
                File.Delete(t.AsyncState.ToString());
            }
            catch { }
            //if (t.IsCanceled) Log.LogMessage("Recognizing was cancelled");
            if (t.Exception != null) log.LogMessage(t.Exception);
            t.Dispose();
        }
        #endregion
        //-------------------------------------------------------------------------
        #region Hotkeys
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case (Keys.Tab):
                    for (int i = 0; i < BarCodeItems.Count; i++)
                    {
                        BarCodeItem item = BarCodeItems[i];
                        if (!item.Verify)
                        {
                            if (!VerifyButton.Focused)
                                comboBoxFocused = false;
                            break;
                        }
                    }
                    break;
                case (Keys.Control | Keys.T):
                    ReportTestPageToolStripMenuItem_Click(null, null);
                    return true;
                case (Keys.Control | Keys.N):
                    NextButton.PerformClick();
                    return true;
                case (Keys.Control | Keys.D):
                    DeleteButton.PerformClick();
                    return true;
                case (Keys.Control | Keys.M):
                    VerifyButton.PerformClick();
                    return true;
                case (Keys.Control | Keys.Right):
                    RotateRightButton.PerformClick();
                    return true;
                case (Keys.Control | Keys.Left):
                    RotateLeftButton.PerformClick();
                    return true;
                case (Keys.Down):
                    if (barCodeList != null && barCodeList.DataSource.Count > 0)
                    {
                        barCodeList.SelectNextBarCodeListItemControl(true);
                        return true;
                    }
                    break;
                case (Keys.Control | Keys.R):
                    btnRemoveNoise.PerformClick();
                    break;
                //case (Keys.Alt):
                //    pictureBox1.Refresh();
                //    break;
                case (Keys.Up):
                    if (barCodeList != null && barCodeList.DataSource.Count > 0)
                    {
                        barCodeList.SelectNextBarCodeListItemControl(false);
                        return true;
                    }
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion
        //-------------------------------------------------------------------------
        #region
        eDoctrinaUtils.Region bubblesRegions = new eDoctrinaUtils.Region();
        eDoctrinaUtils.RegionsArea[] factAreas = new RegionsArea[0];
        public eDoctrinaUtils.RegionsArea[] etalonAreas = new RegionsArea[0];
        bool[] factAreasManualSet = new bool[0];
        //eDoctrinaUtils.RegionsArea[] lastFactAreas = new RegionsArea[0];
        int[] factLinesPerArea = new int[0];
        //Rectangle barCode= Rectangle.Empty;
        //-------------------------------------------------------------------------
        public BubblesAreaControl CreateNewBubblesAreaControl(RegionsArea[] areas, int amoutOfQuestions)
        {//int[] bubblesPerLine,
            maxAmoutOfQuestions = rec.maxAmoutOfQuestions;
            maxCountRectangles = rec.AddMaxCountRectangles();
            if (bubblesPerLine.Length != 0 && areas.Length != bubblesPerLine.Length)
            {
                GetAreasSettings();
                areas = (RegionsArea[])rec.areas.Clone();
            }
            BubblesAreaControl bac = new BubblesAreaControl()
            {
                Name = "bac",
                //Enabled = false,
                Areas = areas,
                bubblesPerLine = areas[0].bubblesPerLine,// bubblesPerLine[0],
                indexOfFirstQuestion = rec.IndexOfFirstQuestion,// IndexOfFirstQuestion,
                AmoutOfQuestions = amoutOfQuestions
            };
            pnlBubbles.Controls.Clear();
            pnlBubbles.Width = bac.Width;
            pnlBubbles.Height = bac.Height;
            pnlBubbles.Controls.Add(bac);
            //bac.Visible = true;
            VerifyButton.Enabled = true;
            return bac;
        }
        //-------------------------------------------------------------------------
        public void SetCheckBoxes()
        {
            bac.Enabled = false;
            for (int k = 0; k < maxCountRectangles.Count; k++)
            {
                KeyValuePair<Bubble, CheckedBubble> item = maxCountRectangles.ElementAt(k);
                if (item.Value.isChecked)
                {
                    if (bac.bubbleCheckedInvert > k)
                        bac.bubbleCheckedInvert = k;
                    else
                        break;
                }
            }
            bac.Enabled = true;
        }
        //-------------------------------------------------------------------------
        private Bubble selectedBubble;
        public Bubble SelectedBubble
        {
            get
            { return selectedBubble; }
            set
            {
                try
                {
                    selectedBubble = value;
                }
                catch (Exception)
                { }
            }
        }
        //-------------------------------------------------------------------------
        public void InvertSelectedBubble(Bubble bubble)
        {
            BubbleItem item = null;
            try
            {
                //item = rec.BubbleItems[bubble.index];
                item = rec.BubbleItems.First(z => z.Bubble.Equals(SelectedBubble));
            }
            catch (Exception)
            {
            }
            if (item != null)
                item.CheckedBubble.isChecked = !item.CheckedBubble.isChecked;
        }
        //-------------------------------------------------------------------------
        public void MouseEnterBubble(int index)
        {
            try
            {
                var item = rec.BubbleItems[index];
                pnlBubbles.ScrollControlIntoView(bac.CheckBoxArr[index]);
                var itm = animatedTimer.ActiveBubbleItem;
                if (itm != null)
                {
                    int ind = rec.BubbleItems.IndexOf(itm);
                    bac.CheckBoxArr[ind].Refresh();
                    DrawBubble(itm.CheckedBubble);
                }
                bac.lockReport = false;
                //DrawBubbleItems();
                animatedTimer.SetActiveValue(rec.BubbleItems[index]);
                //var item = rec.BubbleItems[bubble.index];
                int x = (int)Math.Round(item.CheckedBubble.rectangle.X * Zoom);
                int y = (int)Math.Round(item.CheckedBubble.rectangle.Y * Zoom);

                pictureBox1_MouseMove(null, new MouseEventArgs(MouseButtons.Left, 1, x + 1, y + 1, 1));
                pnlScrollBubble.Width = (int)Math.Round(item.CheckedBubble.rectangle.Width * Zoom);
                pnlScrollBubble.Height = (int)Math.Round(item.CheckedBubble.rectangle.Height * Zoom);
                pnlScrollBubble.Location = new Point(x, y);
                MoveScrollBubble(item.CheckedBubble.rectangle);
                //ImagePanel.ScrollControlIntoView(pnlScrollBubble);
            }
            catch (Exception)
            { }
        }
        //-------------------------------------------------------------------------
        public void MouseEnterBubble(Bubble bubble)
        {
            try
            {
                var item = rec.BubbleItems.First(z => z.Bubble.Equals(bubble));
                //var item = rec.BubbleItems[bubble.index];
                int x = (int)Math.Round(item.CheckedBubble.rectangle.X * Zoom);
                int y = (int)Math.Round(item.CheckedBubble.rectangle.Y * Zoom);

                pictureBox1_MouseMove(null, new MouseEventArgs(MouseButtons.Left, 1, x + 1, y + 1, 1));
                pnlScrollBubble.Width = (int)Math.Round(item.CheckedBubble.rectangle.Width * Zoom);
                pnlScrollBubble.Height = (int)Math.Round(item.CheckedBubble.rectangle.Height * Zoom);
                pnlScrollBubble.Location = new Point(x, y);
                SetScroll();
                ImagePanel.ScrollControlIntoView(pnlScrollBubble);
            }
            catch (Exception)
            { }
        }
        //-------------------------------------------------------------------------
        public void SetScroll()
        {
            if (pictureBox1.Width > ImagePanel.Width || pictureBox1.Height > ImagePanel.Height)
            {
                ImagePanel.AutoScroll = true;
            }
            else
            {
                ImagePanel.AutoScroll = false;
                pictureBox1.Location = new Point(0, 0);
            }
        }
        //-------------------------------------------------------------------------
        private void btnGrid_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null || rec == null || string.IsNullOrEmpty(rec.SheetIdentifier))
                return;
            if (Status == StatusMessage.Delete)
            {
                return;
            }
            if (linsForm == null)
            {
                CreateLinsForm();
                if (linsForm == null)
                    return;
                linsForm.Size = appSettings.Fields.LinsWindowSize;
                linsForm.Visible = true;
            }
            else
            {
                InitLinsForm(false);
                linsForm.Show();
            }
            rec.BarCodesPrompt = "";
            linsForm.Location = appSettings.Fields.LinsWindowLocation;
        }
        //-------------------------------------------------------------------------
        public void CreateLinsForm(bool barCodeControlsBuilding = true)
        {
            if (Status == StatusMessage.Delete || Status == StatusMessage.Verify)
                return;
            linsForm = new LinsForm();
            linsForm.Visible = false;
            linsForm.WindowState = FormWindowState.Normal;
            linsForm.Location = appSettings.Fields.LinsWindowLocation;
            linsForm.Size = appSettings.Fields.LinsWindowSize;
            linsForm.Owner = this;
            linsForm.FormClosing += new FormClosingEventHandler(linsForm_FormClosing);
            linsForm.nudArea.ValueChanged += new EventHandler(nudArea_ValueChanged);
            linsForm.nudCols.ValueChanged += new EventHandler(nudCols_ValueChanged);
            linsForm.nudRows.ValueChanged += new EventHandler(nudRows_ValueChanged);
            linsForm.nudSubRows.ValueChanged += new EventHandler(nudSubRows_ValueChanged);
            linsForm.btnOk.Click += new EventHandler(btnOk_Click);
            linsForm.chbBuildAllAreas.CheckedChanged += ChbBuildAllAreas_CheckedChanged;
            linsForm.chbBuildAllAreas.Checked = appSettings.Fields.ChbBuildAllAreas;
            //eDoctrinaUtils.RegionsArea[] lastFactAreas = new RegionsArea[0];
            InitLinsForm(barCodeControlsBuilding);
        }

        private void ChbBuildAllAreas_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                for (int k = 0; k < factAreasManualSet.Length; k++)
                {
                    if (factAreasManualSet[k])
                        continue;
                    allAreasNaturalSize[k] = Rectangle.Empty;
                }
                pictureBox1_MouseUp(null, new MouseEventArgs(MouseButtons.Left, 0, 0, 0, 0));
                //DrawAllAreasNaturalSize();
                //pictureBox1.Refresh();
            }
            catch (Exception)
            {
            }
        }

        //-------------------------------------------------------------------------
        private void InitLinsForm(bool BarCodeControlsBuilding)
        {
            Application.DoEvents();
            int qn = 0;
            try
            {
                RecognizeAllButton.Enabled = true;
                CreateEtalonAreas();
                if (BarCodeControlsBuilding)// && (rec.areas == null || rec.areas.Length == 0))
                {
                    rec.areas = (RegionsArea[])etalonAreas.Clone();
                }
                if (linsForm != null && appSettings.Fields.LinsTrackBar1Value != 0)
                    linsForm.trackBar1.Value = appSettings.Fields.LinsTrackBar1Value;
                UpdateZoomedImage(0, 0);
                //regions = recTools.GetRegions(rec.SheetIdentifier, rec.regionsList);
                int amout_of_questions = -1;
                if (rec.regions == null)
                {
                    if (appSettings.Fields.ChbSheetId && !string.IsNullOrEmpty(lastSheetId))
                    {
                        rec.regions = recTools.GetRegions(lastSheetId, rec.regionsList);
                    }
                }
                for (int i = 0; i < rec.regions.regions.Length; i++)
                {
                    var item = rec.regions.regions[i];
                    if (item.value == rec.SheetIdentifier)
                        continue;
                    if (item.type.StartsWith("bar"))
                    {
                        if (item.name == "sheetIdentifier")
                            continue;
                        int j = Array.IndexOf(rec.allBarCodeNames, item.name);
                        if (j >= 0)
                        {
                            BarCodeListItemControl b = barCodeList.ControlList[j] as BarCodeListItemControl;
                            switch (item.name)
                            {
                                case "district_id":
                                    if (string.IsNullOrEmpty(b.comboBox1.Text))//item.value == null
                                    {
                                        if (appSettings.Fields.DistrictId && lastDistrictId != "")
                                        {
                                            rec.allBarCodeValues[j] = lastDistrictId;
                                            b.comboBox1.Text = lastDistrictId;
                                            item.value = lastDistrictId;
                                            b.comboBox1.ForeColor = Color.Red;
                                        }
                                    }
                                    break;
                                case "amout_of_questions":
                                    if (rec != null && rec.SheetIdentifier == "FLEX")
                                    {
                                        BarCodeListItemControl b2 = barCodeList.ControlList["bubbles_per_line"] as BarCodeListItemControl;
                                        if (string.IsNullOrEmpty(b2.comboBox1.Text) || !utils.IsNumeric(b2.comboBox1.Text))
                                        {
                                            b.comboBox1.Text = "";
                                            MessageBox.Show("You must specify bubbles_per_line."
                                                , Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);

                                            return;
                                        }
                                        rep:
                                        for (int k = 0; k < BarCodeItems.Count; k++)
                                        {
                                            var itm = BarCodeItems[k];
                                            //}
                                            //foreach (var item in BarCodeItems)//barCodeList.ControlList
                                            //{
                                            if (itm.Name.StartsWith("question_number"))
                                            {
                                                qn++;
                                                if (qn > rec.AmoutOfQuestions)
                                                {
                                                    BarCodeItems.Remove(itm);
                                                    barCodeList.ControlList.Remove(barCodeList.ControlList[k]);
                                                    rec.BarCodeItems.RemoveAt(k);
                                                    goto rep;
                                                }
                                            }
                                        }
                                        Array.Resize(ref rec.allBarCodeNames, BarCodeItems.Count);
                                        Array.Resize(ref rec.allBarCodeValues, BarCodeItems.Count);
                                        Array.Resize(ref rec.questionNumbers, rec.AmoutOfQuestions);
                                        BarCodeControlsBuilding = true;
                                    }
                                    amout_of_questionsIndex = j;
                                    string s = rec.allBarCodeValues[j];
                                    if (!string.IsNullOrEmpty(s))
                                    {
                                        s = b.comboBox1.Text;
                                        rec.allBarCodeValues[j] = s;
                                    }
                                    try
                                    {
                                        j = Convert.ToInt32(s);
                                        amout_of_questions = j;
                                        amoutOfQuestions = j.ToString();
                                        SetRowsValue(amout_of_questions);
                                        if (rec.allBarCodeValues[amout_of_questionsIndex] != s)
                                        {
                                            rec.allBarCodeValues[amout_of_questionsIndex] = s;
                                            SetAmoutOfQuestions(amout_of_questions);
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        if (appSettings.Fields.AmoutOfQuestions && lastAmoutOfQuestions != "")
                                        {
                                            b.comboBox1.Text = lastAmoutOfQuestions;
                                            b.comboBox1.ForeColor = Color.Red;
                                        }
                                        else
                                            amout_of_questions = -1;
                                    }
                                    break;
                            }
                            continue;
                        }
                        if (rec.QrCodeHeaders != null && rec.QrCodeHeaders.Length > 0 && rec.defaults.useStudentId)
                            continue;
                        if (item.areas != null && item.areas[0].width == 0)
                            continue;
                        string value = "";
                        bool verify = false;
                        if (item.name.StartsWith("question_number"))
                        {
                            string number = item.name;
                            number = number.Remove(0, number.LastIndexOf("_") + 1);
                            int index = Convert.ToInt32(number);
                            if (amout_of_questions > -1 && index > amout_of_questions)
                                continue;
                            if (rec.questionNumbers.Length < index)
                            {
                                Array.Resize(ref rec.questionNumbers, index);
                            }
                            index--;
                        }
                        switch (item.name)
                        {
                            case "district_id":
                                if (rec.QrCodeHeaders != null && rec.QrCodeHeaders.Length > 0)
                                {
                                    int index = Array.IndexOf(rec.QrCodeHeaders, "{" + item.name + "}");
                                    if (index > -1)
                                    {
                                        value = rec.QrCodeValues[index];
                                        districtId = value;
                                        verify = true;
                                    }
                                }
                                else if (appSettings.Fields.DistrictId && !string.IsNullOrEmpty(lastDistrictId))
                                    value = lastDistrictId;
                                break;
                            case "test_id":
                                if (rec.QrCodeHeaders != null && rec.QrCodeHeaders.Length > 0)
                                {
                                    int index = Array.IndexOf(rec.QrCodeHeaders, "{" + item.name + "}");
                                    if (index > -1)
                                    {
                                        value = rec.QrCodeValues[index];
                                        testId = value;
                                        verify = true;
                                    }
                                }
                                else if (appSettings.Fields.TestId && !string.IsNullOrEmpty(lastTestId))
                                {
                                    value = lastTestId;
                                    testId = lastTestId;
                                }
                                break;
                            case "amout_of_questions":
                                if (rec.QrCodeHeaders != null && rec.QrCodeHeaders.Length > 0)
                                {
                                    int index = Array.IndexOf(rec.QrCodeHeaders, "{" + item.name + "}");
                                    if (index > -1)
                                    {
                                        value = rec.QrCodeValues[index];
                                        amoutOfQuestions = value;
                                        rec.AmoutOfQuestions = Convert.ToInt32(value);
                                        verify = true;
                                    }
                                }
                                else if (appSettings.Fields.AmoutOfQuestions && !string.IsNullOrEmpty(lastAmoutOfQuestions))
                                    value = lastAmoutOfQuestions;
                                break;
                            case "question_number_1":
                            case "index_of_first_question":

                                if (rec.QrCodeHeaders != null && rec.QrCodeHeaders.Length > 0)
                                {
                                    int index = Array.IndexOf(rec.QrCodeHeaders, "{" + item.name + "}");
                                    if (index > -1)
                                    {
                                        value = rec.QrCodeValues[index];
                                        //indexOfQuestion = value;//???
                                        verify = true;
                                    }
                                }
                                else if (appSettings.Fields.IndexOfFirstQuestion && !string.IsNullOrEmpty(lastIndexOfQuestion))
                                    value = lastIndexOfQuestion;
                                break;
                            default:
                                value = "";
                                break;
                        }
                        if ((rec.QrCodeHeaders == null || rec.QrCodeHeaders.Length == 0)
                            && lastSheetId != "" && lastSheetId == rec.SheetIdentifier && lastTestId != "" && testId == lastTestId)
                        {
                            if (item.name == "amout_of_questions")
                            {
                                value = lastAmoutOfQuestions;
                                if (utils.IsNumeric(lastAmoutOfQuestions))
                                    rec.AmoutOfQuestions = Convert.ToInt32(lastAmoutOfQuestions);
                            }
                            else if (item.name == "index_of_first_question" || item.name == "question_number_1")
                            {
                                value = lastIndexOfQuestion;
                                if (utils.IsNumeric(lastIndexOfQuestion))
                                    rec.IndexOfFirstQuestion = Convert.ToInt32(lastIndexOfQuestion);
                            }
                        }

                        //var bItem = new BarCodeItem(item.name, "numbersText", value, value//item.type//rec.areas[1].type
                        //, new Rectangle(item.areas[0].left, item.areas[0].top, item.areas[0].width, item.areas[0].height));
                        var bItem = new BarCodeItem(item.name, item.areas[1].type, value, value//item.type//rec.areas[1].type
                        , new Rectangle(item.areas[0].left, item.areas[0].top, item.areas[0].width, item.areas[0].height));

                        recTools.SetOutputValues(ref rec.headers, ref rec.headersValues, ref rec.totalOutput, ref rec.allBarCodeNames
                               , ref rec.allBarCodeValues, bItem.Name, bItem.Value, item.outputPosition);

                        if (BarCodeControlsBuilding)
                            rec.FindOrAddAndSetValueBarcode(bItem.Name, value, verify);

                        if (item.name == "amout_of_questions")
                            amout_of_questionsIndex = rec.allBarCodeValues.Length - 1;
                        if (!verify || (item.name == "bubbles_per_line" && (bItem.Value != "5" || bItem.Value != "6")))
                        {
                            lblErr.Text = "Error in " + bItem.Name;
                            if (!errList.Contains(lblErr.Text))
                                errList.Add(lblErr.Text);
                            try
                            {
                                int index = Array.IndexOf(rec.allBarCodeNames, item.name);
                                BarCodeListItemControl b = barCodeList.ControlList[index] as BarCodeListItemControl;
                                b.comboBox1.ForeColor = Color.Red;
                            }
                            catch (Exception)
                            {
                            }
                            lblErr.Visible = true;
                        }
                        else if (item.name == "bubbles_per_line")
                        {
                            switch (bItem.Value)
                            {
                                case "5":
                                case "6":
                                    rec.bubbles_per_lineErr = false;
                                    break;
                                default:
                                    if (!errList.Contains(lblErr.Text))
                                        errList.Add(lblErr.Text);
                                    rec.bubbles_per_lineErr = true;
                                    try
                                    {
                                        int index = Array.IndexOf(rec.allBarCodeNames, item.name);
                                        BarCodeListItemControl b = barCodeList.ControlList[index] as BarCodeListItemControl;
                                        b.comboBox1.ForeColor = Color.Red;
                                    }
                                    catch (Exception)
                                    {
                                    }
                                    lblErr.Visible = true;
                                    break;
                            }
                        }
                    }
                    else if (item.type == "bubblesRegions")
                    {
                        rec.numberOfBubblesRegion = i;
                        bubblesRegions = item;
                        if (linsForm != null)
                        {
                            linsForm.nudArea.Maximum = item.areas.Length;
                            linsForm.nudArea.Value = 1;
                        }
                        if (factAreas.Length == 0)
                        {
                            for (int j = 0; j < rec.areas.Length; j++)
                                rec.areas[j].bubblesPerLine = item.areas[j].bubblesPerLine;
                        }

                        recTools.SetSettings
                            (
                              ref rec.bubblesRegions
                            , ref rec.bubblesOfRegion
                            , ref rec.bubblesSubLinesCount
                            , ref rec.bubblesSubLinesStep
                            , ref rec.bubblesPerLine
                            , ref rec.lineHeight
                            , ref rec.linesPerArea
                            , out rec.answersPosition
                            , out rec.indexAnswersPosition
                            , ref rec.totalOutput
                            , ref rec.bubbleLines
                            , item
                            , rec.kx, rec.ky
                            , rec.curRect, rec.etRect
                            );
                    }
                }
                if (BarCodeControlsBuilding)
                {
                    rec.maxAmoutOfQuestions = rec.linesPerArea.Sum();
                    allAreasNaturalSize = new Rectangle[bubblesRegions.areas.Length];
                    factAreasManualSet = new bool[bubblesRegions.areas.Length];

                    if (rec.SheetIdentifier == "FANDP")//rec.AmoutOfQuestions == 0
                    {
                        maxAmoutOfQuestions = rec.linesPerArea.Sum();
                        rec.AmoutOfQuestions = maxAmoutOfQuestions;
                        rec.FindOrAddAndSetValueBarcode("amout_of_questions", maxAmoutOfQuestions.ToString());
                        rec.FindOrAddAndSetValueBarcode("index_of_first_question", rec.IndexOfFirstQuestion.ToString());
                        BarCodeListItemControl b = barCodeList.ControlList["amout_of_questions"] as BarCodeListItemControl;
                        rbtnGrid.Enabled = false;//!!!
                        b.btnCheck.PerformClick();
                        linsForm.Close();
                        linsForm = null;
                        return;
                    }
                }
                //amout_of_questions = GetFactAreas(amout_of_questions);

                if (!SetFactAreasSetting())
                {
                    if (rec.AmoutOfQuestions > 0)
                    {
                        amout_of_questions = rec.AmoutOfQuestions;
                    }
                    amout_of_questions = GetFactAreas(amout_of_questions);
                    if (BarCodeControlsBuilding && lastSheetId != rec.SheetIdentifier || factAreas.Length == 0
                        || (amout_of_questions > 0 && amout_of_questions != factLinesPerArea.Sum()))
                    {
                        //if (factAreas.Length == 0)
                        //    amout_of_questions = GetFactAreas(amout_of_questions);

                        nudArea_ValueChanged(null, null);

                        for (int i = 0; i < factAreas.Length; i++)
                        {
                            factAreas[i].height = 0;
                        }
                    }
                    else
                    {
                        if (factLinesPerArea.Length == 0 && linsForm != null)
                            linsForm.nudRows.Value = rec.linesPerArea[0];
                        else
                        {
                            if (linsForm != null && BarCodeControlsBuilding)
                                linsForm.nudRows.Value = factLinesPerArea[0];
                        }
                        if (linsForm != null && BarCodeControlsBuilding)
                        {
                            linsForm.nudCols.Value = 4;// factAreas[0].bubblesPerLine;
                            linsForm.nudSubRows.Value = factAreas[0].subLinesAmount;
                        }
                    }
                }
                RecognizeBubblesButton.Enabled = true;
            }
            catch (Exception ex)
            {
                log.LogMessage(ex.Message);
            }
        }
        //-------------------------------------------------------------------------
        private bool SetFactAreasSetting()
        {
            try
            {
                string key = rec.SheetIdentifier + "_" + testId + "_" + amoutOfQuestions + "_" + indexOfQuestion;
                if (linsForm != null && testUids.Test.ContainsKey(key))
                {
                    int amout_of_questions = Convert.ToInt32(amoutOfQuestions);
                    TestUids.AreaSettings[] arrs = testUids.Test[key];
                    if (factAreas.Length == 0)
                        factAreas = new RegionsArea[rec.areas.Length];

                    for (int i = 0; i < arrs.Length; i++)
                    {
                        if (amout_of_questions > 0)
                        {
                            if (amout_of_questions <= rec.linesPerArea[i])
                            {
                                factLinesPerArea[i] = amout_of_questions;
                            }
                            else
                            {
                                factLinesPerArea[i] = rec.linesPerArea[i];
                                amout_of_questions -= factLinesPerArea[i];
                            }
                        }
                        else
                        {
                            factLinesPerArea[i] = 0;
                        }
                        if (linsForm != null && linsForm.nudArea.Value - 1 == i)
                        {
                            linsForm.nudRows.Value = factLinesPerArea[i];
                            linsForm.nudCols.Value = arrs[i].BubblesPrrLine;
                            linsForm.nudSubRows.Value = arrs[i].SubLineAmout;
                        }
                        factAreas[i].bubblesPerLine = arrs[i].BubblesPrrLine;
                        factAreas[i].subLinesAmount = arrs[i].SubLineAmout;
                    }
                    return true;
                }
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        //-------------------------------------------------------------------------
        private int GetFactAreas(int amout_of_questions)
        {
            //if (linsForm == null)
            //    CreateLinsForm();
            if (rec.areas.Length == 0)
            {
                rec.areas = bubblesRegions.areas;
            }
            //factAreas = new RegionsArea[rec.areas.Length];//[bubblesRegions.areas.Length];
            factAreas = new RegionsArea[etalonAreas.Length];
            factLinesPerArea = new int[etalonAreas.Length];//[rec.areas.Length];//[bubblesRegions.areas.Length];
            for (int i = 0; i < etalonAreas.Length; i++)//rec.areas.Length
            {
                factAreas[i].bubblesPerLine = etalonAreas[i].bubblesPerLine;//rec.areas bubblesRegions
                factAreas[i].subLinesAmount = etalonAreas[i].subLinesAmount;//bubblesRegions
                if (amout_of_questions > 0)
                {
                    if (amout_of_questions <= rec.linesPerArea[i])
                    {
                        factLinesPerArea[i] = amout_of_questions;
                    }
                    else
                    {
                        factLinesPerArea[i] = rec.linesPerArea[i];
                        amout_of_questions -= factLinesPerArea[i];
                    }
                }
                else
                    factLinesPerArea[i] = rec.linesPerArea[i];
                if (linsForm != null)
                {
                    if (i == 0 && linsForm.nudArea.Value == 1)
                        linsForm.nudRows.Value = factLinesPerArea[0];
                    else if (i == 1 && linsForm.nudArea.Value == 2)
                        linsForm.nudRows.Value = factLinesPerArea[1];
                }
            }
            return amout_of_questions;
        }
        //-------------------------------------------------------------------------
        private void CreateEtalonAreas()
        {
            //if (etalonAreas.Length == 0)
            //{
            foreach (var item in rec.regionsList)
            {
                if (item.SheetIdentifierName == rec.SheetIdentifier)
                {
                    foreach (var itm in item.regions)
                    {
                        if (itm.name == "answers")
                        {
                            if (rec.SheetIdentifier != "FLEX")
                            {
                                etalonAreas = (RegionsArea[])itm.areas.Clone();
                            }
                            else
                            {
                                switch (rec.bubbles_per_lineFLEX)
                                {
                                    case 5:
                                    case 6:
                                        foreach (var item2 in rec.regionsListFLEX)
                                        {
                                            if (item2.regions[item2.regions.Length - 1].areas[0].bubblesPerLine == rec.bubbles_per_lineFLEX)
                                            {
                                                rec.regions = item2;
                                                etalonAreas = (eDoctrinaUtils.RegionsArea[])item2.regions[item2.regions.Length - 1].areas.Clone();
                                                break;
                                            }
                                        }
                                        break;
                                }
                            }
                            break;
                        }
                    }
                    if (etalonAreas.Length != 0)
                        break;
                }
            }
            //}
        }
        //-------------------------------------------------------------------------
        void btnOk_Click(object sender, EventArgs e)
        {
            Status = StatusMessage.NULL;
            try
            {
                RecognizeBubblesButton.Enabled = false;
                for (int i = 0; i < factAreasManualSet.Length; i++)
                {
                    if (!factAreasManualSet[i])
                    {
                        int x = (int)Math.Round(factAreas[i].left * Zoom);
                        int y = (int)Math.Round(factAreas[i].top * Zoom);
                        areaNaturalSize = allAreasNaturalSize[i];
                        pictureBox1_MouseUp(null, new MouseEventArgs(MouseButtons.Left, 0, x, y, 0));
                        factAreasManualSet[i] = false;
                    }
                }
                eDoctrinaOcrEd.TestUids testUid = new TestUids();
                string testUidAmoutOfQuestions = "", testUidIndexOfFirstQuestion = "";
                for (int i = 0; i < barCodeList.ControlList.Count; i++)
                {
                    BarCodeListItemControl item = (BarCodeListItemControl)barCodeList.ControlList[i];
                    //}
                    //foreach (BarCodeListItemControl item in barCodeList.ControlList)
                    //{
                    RecognizeBubblesButton.Enabled = false;
                    if (item.Name == "amout_of_questions")
                    {
                        amout_of_questionsIndex = i;
                        if (string.IsNullOrEmpty(item.comboBox1.Text) || !utils.IsNumeric(item.comboBox1.Text))
                        {
                            if (factLinesPerArea[0] == 0)
                            {
                                item.btnCheck.PerformClick();
                                return;
                            }
                            item.comboBox1.Text = factLinesPerArea.Sum().ToString();
                            if (errList.Contains("Error in " + item.Name))
                            {
                                errList.Remove("Error in " + item.Name);
                                VerifyErrList();
                            }
                            break;
                        }
                        else
                        {
                            testUidAmoutOfQuestions = item.comboBox1.Text;
                        }
                    }
                    else if (item.Name == "index_of_first_question" || item.Name == "question_number_1")
                    {
                        if (string.IsNullOrEmpty(item.comboBox1.Text) || !utils.IsNumeric(item.comboBox1.Text))
                        {
                            item.comboBox1.Text = "1";
                            if (errList.Contains("Error in " + item.Name))
                            {
                                errList.Remove("Error in " + item.Name);
                                VerifyErrList();
                            }
                        }
                        else
                        {
                            testUidIndexOfFirstQuestion = item.comboBox1.Text;
                        }
                    }
                    else if (item.Name == "test_id")
                    {
                        if (string.IsNullOrEmpty(item.comboBox1.Text) || !utils.IsNumeric(item.comboBox1.Text))
                        {

                        }
                        else
                        {
                            testId = item.comboBox1.Text;
                        }
                    }
                    else if (item.Name == "district_id")
                    {
                        if (string.IsNullOrEmpty(item.comboBox1.Text) || !utils.IsNumeric(item.comboBox1.Text))
                        {

                        }
                        else
                        {
                            districtId = item.comboBox1.Text;
                        }
                    }

                }
                PrevRecognizeBubbles();
                //testUidTestId = testId;
                //testUidShetId = rec.SheetIdentifier;
                string key = rec.SheetIdentifier + "_" + testId + "_" + amoutOfQuestions + "_" + indexOfQuestion;
                //int index=testUids.
                eDoctrinaOcrEd.TestUids.AreaSettings[] arrs = new TestUids.AreaSettings[factAreas.Length];
                if (rec.SheetIdentifier == "FLEX")
                {
                    if (factAreas[0].bubblesPerLine > rec.bubbles_per_lineFLEX)
                    {
                        MessageBox.Show("Invalid value of cols."
                           , Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        rbtnGrid.Checked = true;
                        return;
                    }
                }
                for (int i = 0; i < factAreas.Length; i++)
                {
                    arrs[i].BubblesPrrLine = factAreas[i].bubblesPerLine;
                    arrs[i].SubLineAmout = factAreas[i].subLinesAmount;
                    //if (rec.areas[i].bubblesPerLine < factAreas[i].bubblesPerLine)
                    //{
                    //}
                }
                if (testUids.Test.ContainsKey(key))
                {
                    testUids.Test[key] = arrs;//factAreas;
                }
                else
                {
                    testUids.Test.Add(key, arrs);
                }
                areaNaturalSize = Rectangle.Empty;
                for (int i = 0; i < factAreas.Length; i++)
                {
                    if (factAreas[i].width == 0 && allAreasNaturalSize[i].Width != 0)
                    {
                        factAreas[i].left = allAreasNaturalSize[i].X;
                        factAreas[i].top = allAreasNaturalSize[i].Y;
                        factAreas[i].width = allAreasNaturalSize[i].Width;
                        factAreas[i].height = allAreasNaturalSize[i].Height;
                    }
                }
                double heightEt;
                double kx, ky;
                //rec.Status = RecognizeAction.SearchBublesFinished;
                rec.allContourMultiLine = new Dictionary<Bubble, Point[]>();
                rec.factRectangle = new Rectangle[0];
                Rectangle bubble = Rectangle.Empty;
                double factStepX, factStepY;
                int amout_of_questions = 0;
                Bubble bubble1 = new Bubble();
                //bubble1.point = new Point(rec.indexOfFirstBubble, rec.IndexOfFirstQuestion - 1);
                bubble1.point = new Point(0, rec.IndexOfFirstQuestion - 1);
                for (int i = 0; i < factAreas.Length; i++)
                {//Areas++
                    if (factAreas[i].left < 0 || factAreas[i].left > pictureBox1.Image.Width
                        || factAreas[i].width <= 0 || factAreas[i].width > pictureBox1.Image.Width
                        || factAreas[i].top < 0 || factAreas[i].top > pictureBox1.Image.Height
                        || factAreas[i].height <= 0 || factAreas[i].height > pictureBox1.Image.Height
                        || factAreas[i].lineHeight < 0)
                    {

                        factAreas[i] = new RegionsArea();
                    }
                    if (factAreas.Length > 1 && factAreas[0].height == 0 || (i > 0 && factAreas[i - 1].height == 0))
                    {
                        MessageBox.Show("You must specify all areas of bubbles."
                            , Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        rbtnGrid.Checked = true;
                        return;
                    }
                    if (factAreas[i].height == 0)
                        continue;
                    amout_of_questions += factLinesPerArea[i];

                    if (rec.kx == 0)
                        rec.kx = 1;
                    if (rec.ky == 0)
                        rec.ky = 1;

                    if (rec.kx != 1 || rec.ky != 1)
                    {//EditorForm.cs:строка 3737
                        rec.bubblesOfRegion[i] = new Rectangle((int)Math.Round(rec.bubblesOfRegion[i].X / rec.kx)
                        , (int)Math.Round(rec.bubblesOfRegion[i].Y / rec.ky), rec.bubblesOfRegion[i].Width, rec.bubblesOfRegion[i].Height);
                        rec.bubblesRegions[i].Width = (int)Math.Round(rec.bubblesRegions[i].Width / rec.kx);
                        rec.bubblesRegions[i].Height = (int)Math.Round(rec.bubblesRegions[i].Height / rec.ky);
                    }
                    double EtStepX = (double)
                        (bubblesRegions.areas[i].width - bubblesRegions.areas[i].bubble.Width
                        * bubblesRegions.areas[i].bubblesPerLine)
                        / (bubblesRegions.areas[i].bubblesPerLine - 1);
                    int widthEt = (int)(bubblesRegions.areas[i].bubble.Width
                        * factAreas[i].bubblesPerLine + EtStepX * (factAreas[i].bubblesPerLine - 1));//(int)linsForm.nudCols.Value - 1);
                    kx = (double)factAreas[i].width / widthEt;
                    rec.kx = (decimal)kx;
                    double EtStepY = 0;
                    if (rec.linesPerArea[i] > 1)
                    {
                        EtStepY = (double)(bubblesRegions.areas[i].height
                            - bubblesRegions.areas[i].bubble.Height * rec.linesPerArea[i]) / (rec.linesPerArea[i] - 1);
                    }
                    //double EtStepY = bubblesRegions.areas[i].lineHeight - bubblesRegions.areas[i].bubble.Height;
                    heightEt = bubblesRegions.areas[i].bubble.Height * factLinesPerArea[i]
                       + EtStepY * (factLinesPerArea[i] - 1)
                       + (bubblesRegions.areas[i].subLineHeight
                       * (factAreas[i].subLinesAmount));//bubblesRegions.areas[i].subLinesAmount - 
                    ky = (double)factAreas[i].height / heightEt;
                    rec.ky = (decimal)ky;

                    //rec.ProcessingRegions();

                    bubble.Width = (int)Math.Round(bubblesRegions.areas[i].bubble.Width * kx);
                    bubble.Height = (int)Math.Round(bubblesRegions.areas[i].bubble.Height * ky);
                    factStepX = (double)(factAreas[i].width + EtStepX * kx) / (double)factAreas[i].bubblesPerLine;
                    double distX = (double)factStepX - bubble.Width;
                    //double halfDistX = (double)distX / 2;
                    factAreas[i].lineHeight = (int)Math.Round(bubblesRegions.areas[i].lineHeight * ky);
                    factAreas[i].subLineHeight = (int)Math.Round(bubblesRegions.areas[i].subLineHeight * ky);
                    factAreas[i].bubble = new Rectangle((int)(rec.bubblesOfRegion[i].X * kx)
                                                            , (int)(rec.bubblesOfRegion[i].Y * ky)
                                                            , bubble.Width
                                                            , bubble.Height
                                                            );//???

                    rec.bubblesOfRegion[i] = new Rectangle((int)(rec.bubblesOfRegion[i].X * kx)
                        , (int)(rec.bubblesOfRegion[i].Y * ky), bubble.Width, bubble.Height);

                    rec.bubblesRegions[i].Width = (int)Math.Round(rec.bubblesRegions[i].Width * rec.kx);
                    rec.bubblesRegions[i].Height = (int)Math.Round(rec.bubblesRegions[i].Height * rec.ky);

                    factStepY = (double)((factAreas[i].height + EtStepY * ky)
                        - factAreas[i].subLineHeight * factAreas[i].subLinesAmount) / (double)factLinesPerArea[i];
                    Array.Resize(ref rec.factRectangle, rec.factRectangle.Length + 1);
                    rec.factRectangle[rec.factRectangle.Length - 1] = new Rectangle
                        (
                          factAreas[i].left
                        , factAreas[i].top
                        , bubble.Width
                        , bubble.Height
                        );

                    bubble1.areaNumber = i;
                    //bubble1.point = new System.Drawing.Point(rec.indexOfFirstBubble, bubble1.point.Y + 1);
                    bubble1.point = new System.Drawing.Point(0, bubble1.point.Y + 1);
                    bubble1.subLine = 0;
                    rec.allContourMultiLine.Add(bubble1, new System.Drawing.Point[5]
                {
                 new Point(rec.factRectangle[rec.factRectangle.Length - 1].Left, rec.factRectangle[rec.factRectangle.Length - 1].Top)
                ,new Point(rec.factRectangle[rec.factRectangle.Length - 1].Right, rec.factRectangle[rec.factRectangle.Length - 1].Top)
                ,new Point(rec.factRectangle[rec.factRectangle.Length - 1].Right, rec.factRectangle[rec.factRectangle.Length - 1].Bottom)
                ,new Point(rec.factRectangle[rec.factRectangle.Length - 1].Left, rec.factRectangle[rec.factRectangle.Length - 1].Bottom)
                ,new Point(rec.factRectangle[rec.factRectangle.Length - 1].Left, rec.factRectangle[rec.factRectangle.Length - 1].Top)
                });

                    //Bitmap b2 = (Bitmap)pictureBox1.Image.Clone();
                    //using (Graphics g = Graphics.FromImage(b2))
                    //{
                    for (int j = 0; j < factLinesPerArea[i]; j++)//(int)linsForm.nudRows.Value
                    {//line++
                        //g.DrawLine
                        //    (
                        //      new Pen(Color.Blue)
                        //    , factAreas[i].left
                        //    , (int)(factAreas[i].top + factStepY * j)
                        //    , factAreas[i].left + factAreas[i].width
                        //    , (int)(factAreas[i].top + factStepY * j)
                        //    );
                        if (j > 0)
                        {
                            Array.Resize(ref rec.factRectangle, rec.factRectangle.Length + 1);
                            rec.factRectangle[rec.factRectangle.Length - 1] = new Rectangle
                                (
                                  factAreas[i].left
                                , factAreas[i].top + (int)Math.Round((factStepY) * j)
                                , bubble.Width
                                , bubble.Height
                                );

                            //bubble1.point = new Point(rec.indexOfFirstBubble, bubble1.point.Y + 1);
                            bubble1.point = new Point(0, bubble1.point.Y + 1);
                            bubble1.subLine = 0;

                            rec.allContourMultiLine.Add(bubble1, new System.Drawing.Point[5]
                                {
                                 new Point(rec.factRectangle[rec.factRectangle.Length - 1].Left, rec.factRectangle[rec.factRectangle.Length - 1].Top)
                                ,new Point(rec.factRectangle[rec.factRectangle.Length - 1].Right, rec.factRectangle[rec.factRectangle.Length - 1].Top)
                                ,new Point(rec.factRectangle[rec.factRectangle.Length - 1].Right, rec.factRectangle[rec.factRectangle.Length - 1].Bottom)
                                ,new Point(rec.factRectangle[rec.factRectangle.Length - 1].Left, rec.factRectangle[rec.factRectangle.Length - 1].Bottom)
                                ,new Point(rec.factRectangle[rec.factRectangle.Length - 1].Left, rec.factRectangle[rec.factRectangle.Length - 1].Top)
                                });

                        }
                        for (int k = 0; k <= (int)bubblesRegions.areas[i].subLinesAmount; k++)
                        {
                            if (k > 0)
                            {//subLine++
                                Array.Resize(ref rec.factRectangle, rec.factRectangle.Length + 1);
                                rec.factRectangle[rec.factRectangle.Length - 1] = new Rectangle
                                (
                                  factAreas[i].left
                                , rec.factRectangle[rec.factRectangle.Length - 2].Y + factAreas[i].subLineHeight
                                , bubble.Width
                                , bubble.Height
                                );

                                bubble1.subLine = bubble1.subLine + 1;
                                //bubble1.point = new Point(rec.indexOfFirstBubble, bubble1.point.Y);
                                bubble1.point = new Point(0, bubble1.point.Y);
                                rec.allContourMultiLine.Add(bubble1, new System.Drawing.Point[5]
                                {
                                 new Point(rec.factRectangle[rec.factRectangle.Length - 1].Left, rec.factRectangle[rec.factRectangle.Length - 1].Top)
                                ,new Point(rec.factRectangle[rec.factRectangle.Length - 1].Right, rec.factRectangle[rec.factRectangle.Length - 1].Top)
                                ,new Point(rec.factRectangle[rec.factRectangle.Length - 1].Right, rec.factRectangle[rec.factRectangle.Length - 1].Bottom)
                                ,new Point(rec.factRectangle[rec.factRectangle.Length - 1].Left, rec.factRectangle[rec.factRectangle.Length - 1].Bottom)
                                ,new Point(rec.factRectangle[rec.factRectangle.Length - 1].Left, rec.factRectangle[rec.factRectangle.Length - 1].Top)
                                });
                            }

                            for (int l = 1; l < factAreas[i].bubblesPerLine; l++)//(int)linsForm.nudCols.Value
                            {//point.X++

                                //    g.DrawLine
                                //(
                                //new Pen(Color.Blue)
                                //, (int)(factAreas[i].left + factStepX * l)
                                //, factAreas[i].top
                                //, (int)(factAreas[i].left + factStepX * l)
                                //, factAreas[i].top + factAreas[i].height
                                //);

                                Array.Resize(ref rec.factRectangle, rec.factRectangle.Length + 1);
                                rec.factRectangle[rec.factRectangle.Length - 1] = new Rectangle
                                   (
                                     factAreas[i].left + (int)Math.Round((factStepX) * l)
                                   , rec.factRectangle[rec.factRectangle.Length - 2].Y
                                   , bubble.Width
                                   , bubble.Height
                                  );
                                bubble1.point = new Point(bubble1.point.X + 1, bubble1.point.Y);
                                rec.allContourMultiLine.Add(bubble1, new System.Drawing.Point[5]
                                {
                                 new Point(rec.factRectangle[rec.factRectangle.Length - 1].Left, rec.factRectangle[rec.factRectangle.Length - 1].Top)
                                ,new Point(rec.factRectangle[rec.factRectangle.Length - 1].Right, rec.factRectangle[rec.factRectangle.Length - 1].Top)
                                ,new Point(rec.factRectangle[rec.factRectangle.Length - 1].Right, rec.factRectangle[rec.factRectangle.Length - 1].Bottom)
                                ,new Point(rec.factRectangle[rec.factRectangle.Length - 1].Left, rec.factRectangle[rec.factRectangle.Length - 1].Bottom)
                                ,new Point(rec.factRectangle[rec.factRectangle.Length - 1].Left, rec.factRectangle[rec.factRectangle.Length - 1].Top)
                                });
                            }
                        }
                    }
                    //    g.DrawRectangle(new Pen(Color.Red), area);
                    //}
                    //b2.Save("Rectangles.bmp", ImageFormat.Bmp);
                    //b2.Dispose();
                }
                //rec.BubblesRecognition();
                //ShowProcessingForm();
                linsForm.btnOk.Enabled = false;

                rec.AmoutOfQuestions = amout_of_questions;
                BarCodeItems[amout_of_questionsIndex].Value = amout_of_questions.ToString();
                rec.areas = bubblesRegions.areas;// factAreas;
                //rec.UpdateGui();

                barCodesPrompt = "";
                maxCountRectangles = rec.AddMaxCountRectangles();
                rec.BubblesRecognition();
                //rec.BubblesRecognition(false);
                if (backgroundWorker.IsBusy)
                {
                    CancelBackgroundWorker();
                    //do
                    //{
                    //    Application.DoEvents();
                    //} while (backgroundWorker.IsBusy);
                    //System.Threading.Thread.Sleep(500);
                }
                backgroundWorker.RunWorkerAsync(new string[] { "BubblesRecognition2" });
                //Focus();
                ShowProcessingForm();

                //Bitmap b = (Bitmap)pictureBox1.Image.Clone();
                //using (Graphics g = Graphics.FromImage(b))
                //{
                //    foreach (Rectangle item in rec.factRectangle)
                //    {
                //        g.DrawRectangle(new Pen(Color.Red), item);
                //    }
                //}
                //b.Save("factRectangles.bmp", ImageFormat.Bmp);
                //b.Dispose();

                //area= Rectangle.Empty;
                linsForm.btnOk.Enabled = true;
                rpf = null;
                //Focus();
                //linsForm.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Send this sheet to developers for fixing problem."
                 , Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                log.LogMessage(ex);
                string message = "Send log file to developers for fixing problem.";
                log.LogMessage(message);

                linsForm.btnOk.Enabled = true;
                rpf = null;
            }
        }
        //-------------------------------------------------------------------------
        void nudSubRows_ValueChanged(object sender, EventArgs e)
        {
            factAreas[(int)linsForm.nudArea.Value - 1].subLinesAmount = (int)linsForm.nudSubRows.Value;
        }
        //-------------------------------------------------------------------------
        void nudCols_ValueChanged(object sender, EventArgs e)
        {
            factAreas[(int)linsForm.nudArea.Value - 1].bubblesPerLine = (int)linsForm.nudCols.Value;
        }
        //-------------------------------------------------------------------------
        void nudRows_ValueChanged(object sender, EventArgs e)
        {
            factLinesPerArea[(int)linsForm.nudArea.Value - 1] = (int)linsForm.nudRows.Value;
        }
        //-------------------------------------------------------------------------
        private void nudArea_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (sender == null)
                {
                    if (linsForm.nudCols.Maximum == 1000)
                    {
                        if (factAreas.Length == 0)
                        {
                            linsForm.nudCols.Value = bubblesRegions.areas[(int)linsForm.nudArea.Value - 1].bubblesPerLine;
                        }
                        else
                        {
                            linsForm.nudCols.Value = 4;// factAreas[(int)linsForm.nudArea.Value - 1].bubblesPerLine;
                        }
                        //linsForm.nudCols.Maximum = linsForm.nudCols.Value;
                    }
                    if (linsForm.nudRows.Value == 0)
                    {
                        linsForm.nudRows.Value = rec.linesPerArea[(int)linsForm.nudArea.Value - 1];
                    }
                    //linsForm.nudRows.Minimum = 1;
                    //linsForm.nudRows.Maximum = linsForm.nudRows.Value;
                    if (factAreas.Length == 0)
                        linsForm.nudSubRows.Value = bubblesRegions.areas[(int)linsForm.nudArea.Value - 1].subLinesAmount;
                    else
                        linsForm.nudSubRows.Value = factAreas[(int)linsForm.nudArea.Value - 1].subLinesAmount;
                    //linsForm.nudSubRows.Maximum = linsForm.nudSubRows.Value;
                }
                else
                {
                    //if (factAreas[(int)linsForm.nudArea.Value - 1].bubblesPerLine == 0)
                    //    factAreas[(int)linsForm.nudArea.Value - 1].bubblesPerLine = 1;
                    if (factAreas[(int)linsForm.nudArea.Value - 1].bubblesPerLine > 0)
                        linsForm.nudCols.Value = factAreas[(int)linsForm.nudArea.Value - 1].bubblesPerLine;
                    else
                        linsForm.nudCols.Value = etalonAreas[(int)linsForm.nudArea.Value - 1].bubblesPerLine;
                    linsForm.nudRows.Value = factLinesPerArea[(int)linsForm.nudArea.Value - 1];
                    linsForm.nudSubRows.Value = factAreas[(int)linsForm.nudArea.Value - 1].subLinesAmount;
                    //ClearAllCheckMarksButton_Click(null, null);
                }
            }
            catch (Exception)
            {
            }
        }
        //-------------------------------------------------------------------------
        void linsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            appSettings.Fields.LinsWindowLocation = linsForm.Location;
            appSettings.Fields.LinsWindowSize = linsForm.Size;
            appSettings.Fields.LinsTrackBar1Value = linsForm.trackBar1.Value;
            appSettings.Fields.ChbBuildAllAreas = linsForm.chbBuildAllAreas.Checked;
            //linsFormIsVisible = false;
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                linsForm.Hide();
                RecognizeAllButton.Enabled = true;
                //RecognizeBubblesButton.Enabled = true;
                return;
            }
            SaveSettings();
            linsForm.FormClosing -= new FormClosingEventHandler(linsForm_FormClosing);
            //btnRestore.Enabled = false;
            if (animatedTimer != null)
                animatedTimer.StartAnimation();
            area = Rectangle.Empty;
            RecognizeAllButton.Enabled = true;
            allAreasNaturalSize = new Rectangle[0];
            //recBitmap = null;
            linsForm.Dispose();
            linsForm = null;
        }
        int lastX = 0, lastY = 0;
        //-------------------------------------------------------------------------
        public void UpdateZoomedImage(int x = -1, int y = -1)
        {
            if (pictureBox1.Image == null)//linsForm == null
                return;
            // Calculate the width and height of the portion of the image we want
            // to show in the picZoom picturebox. This value changes when the zoom
            // factor is changed.
            if (x == -1)
            {
                x = lastX;
                y = lastY;
            }
            else
            {
                lastX = x;
                lastY = y;
            }

            int zoomWidth = pictureBox2.Width / (int)nudZoom.Value;//linsForm.pictureBox1.Width / 8;//_ZoomFactor;
            int zoomHeight = pictureBox2.Height / (int)nudZoom.Value;// 2;//linsForm.pictureBox1.Height / 8;//_ZoomFactor;

            // Calculate the horizontal and vertical midpoints for the crosshair
            // cursor and correct centering of the new image
            int halfWidth = zoomWidth / 2;
            int halfHeight = zoomHeight / 2;

            // Create a new temporary bitmap to fit inside the picZoom picturebox
            Bitmap tempBitmap = new Bitmap(zoomWidth, zoomHeight, PixelFormat.Format24bppRgb);

            // Create a temporary Graphics object to work on the bitmap
            Graphics bmGraphics = Graphics.FromImage(tempBitmap);

            // Clear the bitmap with the selected backcolor
            bmGraphics.Clear(Color.White);

            // Set the interpolation mode
            bmGraphics.InterpolationMode = InterpolationMode.HighQualityBilinear;//.HighQualityBicubic;

            // Draw the portion of the main image onto the bitmap
            // The target rectangle is already known now.
            // Here the mouse position of the cursor on the main image is used to
            // cut out a portion of the main image.
            bmGraphics.DrawImage(pictureBox1.Image,
                                 new Rectangle(0, 0, zoomWidth, zoomHeight),
                                 new Rectangle(x - halfWidth, y - halfHeight, zoomWidth, zoomHeight),
                                 GraphicsUnit.Pixel);

            // Draw the bitmap on the picZoom picturebox
            pictureBox2.Image = tempBitmap;//linsForm.pictureBox1.Image = tempBitmap;

            //Draw a crosshair on the bitmap to simulate the cursor position
            //bmGraphics.DrawLine(Pens.DarkViolet, halfWidth + 1, halfHeight - 9, halfWidth + 1, halfHeight - 1);
            //bmGraphics.DrawLine(Pens.DarkViolet, halfWidth + 1, halfHeight + 11, halfWidth + 1, halfHeight + 3);
            //bmGraphics.DrawLine(Pens.DarkViolet, halfWidth - 9, halfHeight + 1, halfWidth - 1, halfHeight + 1);
            //bmGraphics.DrawLine(Pens.DarkViolet, halfWidth + 11, halfHeight + 1, halfWidth + 3, halfHeight + 1);
            bmGraphics.DrawLine(new Pen(Color.LightSeaGreen, 3), halfWidth + 1, halfHeight - 19, halfWidth + 1, halfHeight - 1);
            bmGraphics.DrawLine(new Pen(Color.LightSeaGreen, 3), halfWidth + 1, halfHeight + 21, halfWidth + 1, halfHeight + 3);//MediumVioletRed
            bmGraphics.DrawLine(new Pen(Color.LightSeaGreen, 3), halfWidth - 19, halfHeight + 1, halfWidth - 1, halfHeight + 1);//LightGray
            bmGraphics.DrawLine(new Pen(Color.LightSeaGreen, 3), halfWidth + 21, halfHeight + 1, halfWidth + 3, halfHeight + 1);

            //bmGraphics.DrawLine(Pens.LightSeaGreen, halfWidth + 1, halfHeight - 9, halfWidth + 1, halfHeight - 1);
            //bmGraphics.DrawLine(Pens.LightSeaGreen, halfWidth + 1, halfHeight + 11, halfWidth + 1, halfHeight + 3);//MediumVioletRed
            //bmGraphics.DrawLine(Pens.LightSeaGreen, halfWidth - 9, halfHeight + 1, halfWidth - 1, halfHeight + 1);//LightGray
            //bmGraphics.DrawLine(Pens.LightSeaGreen, halfWidth + 11, halfHeight + 1, halfWidth + 3, halfHeight + 1);

            bmGraphics.Dispose();

            // Refresh the picZoom picturebox to reflect the changes
            pictureBox2.Refresh(); //linsForm.pictureBox1.Refresh();
        }
        //-------------------------------------------------------------------------
        private void btnRestore_Click(object sender, EventArgs e)
        {
            MouseEventArgs e2 = e as MouseEventArgs;
            if (e2 != null)
                if (e2.X >= splitBtnRestore.Size.Width - 21)
                    return;
            if (recBitmap == null)
                return;
            RecognizeAllButton.Enabled = true;
            if (sender == null)
            {
                if (isInvert || isRotate || isCut || isClear)
                    return;
            }
            isRotate = false;
            isCut = false;
            isClear = false;
            rec.Bitmap = (Bitmap)recBitmap.Clone();
            //pictureBox1.Image = (Bitmap)rec.Bitmap.Clone();//new Bitmap(rec.Bitmap);
            ShowImage();
            if (animatedTimer != null) animatedTimer.StopAnimation();
            rec.BubbleItems.Clear();
            AnimatedBarCodeClear();
            AnimatedBubbleClear();
            //animatedTimer.Clear();
        }
        //-------------------------------------------------------------------------
        private void rbtnRotate_CheckedChanged(object sender, EventArgs e)
        {
            SelectedBarCodeItem = null;
            UncheckRbtn(sender, e);
            pictureBox1.Refresh();
            if (linsForm != null)
                linsForm.Hide();
        }
        //-------------------------------------------------------------------------
        private void rbtnBubbles_CheckedChanged(object sender, EventArgs e)
        {
            SelectedBarCodeItem = null;
            UncheckRbtn(sender, e);
            pictureBox1.Refresh();
            if (linsForm != null)
                linsForm.Hide();
            if (animatedTimer != null)
                animatedTimer.StartAnimation();
        }
        //-------------------------------------------------------------------------
        private void rbtnCut_CheckedChanged(object sender, EventArgs e)
        {
            SelectedBarCodeItem = null;
            UncheckRbtn(sender, e);
            pictureBox1.Refresh();
            if (animatedTimer != null)
                animatedTimer.StopAnimation();
            if (linsForm != null)
                linsForm.Hide();
        }
        //-------------------------------------------------------------------------
        private void btnGrid_EnabledChanged(object sender, EventArgs e)
        {
            //rbtnCut.Enabled = btnGrid.Enabled;
            //rbtnRotate.Enabled = btnGrid.Enabled;
            rbtnGrid.Enabled = btnGrid.Enabled;
            rbtnBubbles.Enabled = btnGrid.Enabled;
            OpenFilesDirButton.Enabled = btnGrid.Enabled;
            //btnRestore.Enabled = btnGrid.Enabled;
            if (!rbtnGrid.Enabled)
            {
                rbtnRotate.Checked = false;
                rbtnCut.Checked = false;
                rbtnGrid.Checked = false;
                rbtnBubbles.Checked = false;
                SelectedBarCodeItem = null;
                area = Rectangle.Empty;
                areaNaturalSize = Rectangle.Empty;
            }
        }
        //-------------------------------------------------------------------------
        private void SendAllToNextProcessingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("All  files from folders\r\n"
                + OcrAppConfig.TempEdFolder + "\r\nand\r\n"
                + defaults.ManualInputFolder + "\r\nwill be moved to folder\r\n"
                + defaults.ManualNextProccessingFolder
                + "\r\nProceed?"
                , Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes
                )
            {
                string[] fnArr = Directory.GetFiles(defaults.ManualInputFolder, "*.*", SearchOption.AllDirectories);
                foreach (string item in fnArr)
                {
                    try
                    {
                        FileInfo fi = new FileInfo(item);
                        fi.MoveTo(defaults.ManualNextProccessingFolder + "\\" + fi.Name);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                }
                NextButton.PerformClick();
            }
        }
        //-------------------------------------------------------------------------
        private void btnRemoveNoise_Click(object sender, EventArgs e)
        {
            rbtnRotate_Click(sender, e);
            if (rec == null || rec.Bitmap == null)
                return;
            UpdateUI("StatusLabel", "Remove noise, please wait ...");
            try
            {
                backgroundWorker.RunWorkerAsync(new string[] { "RemoveNoise" });
                ShowProcessingForm();
            }
            catch (Exception)
            {
            }
        }
        //-------------------------------------------------------------------------
        private void cbDoNotProcess_CheckedChanged(object sender, EventArgs e)
        {
            if (cbDoNotProcess.Checked)
            {
                lblErr.Text = "Do not process";
                lblErr.Visible = true;
                RecognizeBubblesButton.Enabled = false;
            }
            else
            {
                lblErr.Visible = false;
                if (rec != null)
                    RecognizeAllButton.Enabled = true;
                try
                {
                    RecognizeAllButton.PerformClick();
                }
                catch (Exception)
                {
                }
            }
        }
        //-------------------------------------------------------------------------
        private void btnCloseLblErr_Click(object sender, EventArgs e)
        {
            if (lblErr.Text == "Do not process")
                cbDoNotProcess.Checked = false;
            if (errList.Contains(lblErr.Text))
                errList.Remove(lblErr.Text);
            VerifyErrList();
        }
        //-------------------------------------------------------------------------
        private void VerifyErrList()
        {
            if (errList.Count > 0)
            {
                lblErr.Text = errList[errList.Count - 1];
                lblErr.Visible = true;
            }
            else if (lblErr.Text != "Do not process")
                lblErr.Visible = false;
        }
        //-------------------------------------------------------------------------
        private void lblErr_VisibleChanged(object sender, EventArgs e)
        {
            btnCloseLblErr.Visible = lblErr.Visible;
        }
        //-------------------------------------------------------------------------
        private void EditorForm_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyValue == 18)//e.KeyData == Keys.Alt
                {
                    pictureBox1.Image = (Bitmap)rec.Bitmap.Clone();
                }

                foreach (BarCodeListItemControl item in barCodeList.ControlList)
                {
                    if (item.comboBox1.Focused)
                    {
                        return;
                    }
                }
                if (e.KeyCode == Keys.Delete)
                {
                    DeleteButton.PerformClick();
                }
            }
            catch (Exception)
            {
            }
        }
        //-------------------------------------------------------------------------
        private void btnRestore_EnabledChanged(object sender, EventArgs e)
        {
            //btnRemoveNoise.Enabled = btnRestore.Enabled;
            rbtnCut.Enabled = splitBtnRestore.Enabled; //btnGrid.Enabled;
            rbtnRotate.Enabled = splitBtnRestore.Enabled;
            //btnInvert.Enabled = btnRestore.Enabled;
        }
        //-------------------------------------------------------------------------
        private void btnInvert_Click(object sender, EventArgs e)
        {
            rbtnRotate_Click(sender, e);
            if (rec == null || rec.Bitmap == null)
                return;
            Bitmap copy = new Bitmap(rec.Bitmap.Width, rec.Bitmap.Height);
            Exception ex;
            copy = recTools.NormalizeBitmap(copy, out ex);
            ImageAttributes ia = new ImageAttributes();
            ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                                        {
                                            new float[] {-1, 0, 0, 0, 0},
                                            new float[] {0, -1, 0, 0, 0},
                                            new float[] {0, 0, -1, 0, 0},
                                            new float[] {0, 0, 0, 1, 0},
                                            new float[] {1, 1, 1, 0, 1}
                                        });
            ia.SetColorMatrix(colorMatrix);
            Graphics g = Graphics.FromImage(copy);
            g.DrawImage(rec.Bitmap, new Rectangle(0, 0, rec.Bitmap.Width, rec.Bitmap.Height)
                , 0, 0, rec.Bitmap.Width, rec.Bitmap.Height, GraphicsUnit.Pixel, ia);
            g.Dispose();
            rec.Bitmap = (Bitmap)copy.Clone();
            copy.SetResolution(300, 300);
            //if (copy.Palette.Entries.Length == 0)
            //{
            //    ColorPalette palette = copy.Palette;
            //    Color[] entries = new Color[2] { Color.FromArgb(255, 255, 255, 255), Color.FromArgb(255, 0, 0, 0) };// palette.Entries;
            //    //entries[0] = Color.FromArgb(255, 255, 255, 255);
            //    //entries[1] = Color.FromArgb(255, 0, 0, 0);
            //    copy.Palette = palette;
            //    //copy.Palette.Entries.SetValue(Color.FromArgb(255, 255, 255, 255), 0);
            //    //copy.Palette.Entries.SetValue(Color.FromArgb(Color.FromArgb(255, 0, 0, 0),1);
            //    //Color.FromArgb(255, 0, 0, 0);//=new Color[2]{Color.FromArgb(255, 255, 255, 255),Color.FromArgb(255, 0, 0, 0};
            //}
            //if (copy.Palette.Entries.Length >= 2)
            //{
            //    Color p0 = copy.Palette.Entries[0];
            //    Color p1 = copy.Palette.Entries[1];

            //    if ((p0.R << 16 + p0.G << 8 + p0.B) < (p1.R << 16 + p1.G << 8 + p1.B))
            //    {
            //        // Swap the palette entries.
            //        copy.Palette.Entries[0] = p1;
            //        copy.Palette.Entries[1] = p0;
            //    }
            //}
            copy.Save(Path.Combine(OcrAppConfig.TempEdFolder, rec.Audit.fileName));
            pictureBox1.Image = (Bitmap)rec.Bitmap.Clone();
            //pictureBox1.Image.Save(Path.Combine(OcrAppConfig.TempEdFolder, rec.Audit.fileName));
            //pictureBox1.Image.Save("isInvert.bmp", ImageFormat.Bmp);
            //byte[] tiffBytes = recTools.TiffImageBytes(copy);
            //File.WriteAllBytes(Path.Combine(OcrAppConfig.TempEdFolder, rec.Audit.fileName), tiffBytes);
            //tiffBytes = null;
            copy.Dispose();
            isInvert = true;
            DrawBubbleItems();
        }
        //-------------------------------------------------------------------------
        private void button1_Click(object sender, EventArgs e)
        {
            log.LogMessage("Reprocess source Button Click");
            if (string.IsNullOrEmpty(rec.Audit.archiveFileName))
            {
                MessageBox.Show("Archive file name not found in audit file"
                    , Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            if (MessageBox.Show("Send to reprocess a source file?"
            , Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                try
                {
                    FileInfo fi = new FileInfo(rec.Audit.archiveFileName);
                    FileInfo fi2 = new FileInfo(defaults.InputFolder);
                    string s = Path.Combine(fi2.DirectoryName, fi.Name);
                    fi.CopyTo(Path.Combine(fi2.DirectoryName, fi.Name));
                }
                catch (Exception ex)
                { MessageBox.Show(ex.Message); }
            }
        }
        //-------------------------------------------------------------------------
        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            log.LogMessage("OpenFilesDir Button Click");
            try
            {
                //FileInfo fi = new FileInfo(rec.Audit.fileName);
                Process.Start("explorer.exe", string.Format("/select,\"{0}\""
                    , rec.FileName));//Path.Combine(OcrAppConfig.TempEdFolder, 
                //OpenProcessingFolderMainMenuItem_Click(null, null);
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }

        private void EditorForm_VisibleChanged(object sender, EventArgs e)
        {

        }
        //-------------------------------------------------------------------------
        private void EditorForm_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyValue == 18)//e.KeyData == Keys.Alt
                {
                    DrawBubbleItems();
                    pictureBox1.Refresh();
                }
            }
            catch (Exception)
            {
            }
        }
        //-------------------------------------------------------------------------
        private void rbtnRotate_Click(object sender, EventArgs e)
        {
            if (!appSettings.Fields.UsePrevTool)
                return;
            foreach (var item in editorTools)
            {
                if (item != sender)
                    item.BackColor = SystemColors.Control;
                else
                {
                    if (item.BackColor == SystemColors.Control)
                        item.BackColor = Color.Cyan;
                    //else
                    //    item.BackColor = SystemColors.Control;
                }
            }
        }
        //-------------------------------------------------------------------------
        private void returnFilesAndQuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("All  files from folders\r\n"
    + OcrAppConfig.TempEdFolder + "\r\nwill be moved to folder\r\n"
    + defaults.ManualInputFolder
    + "\r\nProceed?"
    , Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes
    )
            {
                timer2.Stop();
                timer2.Enabled = false;
                string[] fnArr = Directory.GetFiles(OcrAppConfig.TempEdFolder, "*.*", SearchOption.TopDirectoryOnly);
                foreach (string item in fnArr)
                {
                    try
                    {
                        FileInfo fi = new FileInfo(item);
                        fi.MoveTo(Path.Combine(defaults.ManualInputFolder, fi.Name));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                }
                Close();
            }
        }

        private void toolTip2_Popup(object sender, PopupEventArgs e)
        {
            //if (toolTip1.Active)
            //{
            //    toolTip2.Active = false;
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {//запустить экзешник для рестарта редактора и
         //ProcessStartInfo psi = new ProcessStartInfo();
         //psi.Arguments =new string[1] { Application.ExecutablePath };
         //psi.FileName = "startApp.exe";
         //Process.Start(psi);
            try
            {
                log.LogMessage("Manual restart");
                SaveSettings();
                if (testUids.Test.Count > 0)
                {
                    var serializer = new SerializerHelper();
                    serializer.SaveToFile(testUids, Path.Combine(OcrAppConfigDefaults.BaseTempFolder, "testUids.json"), Encoding.Unicode);
                }
            }
            catch (Exception)
            {
            }
            Process.Start("startApp.exe", Application.ExecutablePath);// + " a b c"
            Environment.Exit(0);
            //try
            //{
            //    Application.Restart();
            //}
            //catch (Exception)
            //{
            //}
        }
        //-------------------------------------------------------------------------
        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveSettings();
            Process.Start("eDoctrinaOcrUpdate.exe");
        }
        //-------------------------------------------------------------------------
        private void pictureBox2_MouseEnter(object sender, EventArgs e)
        {
            nudZoom.Visible = true;
        }
        //-------------------------------------------------------------------------
        //private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if (e.X > nudZoom.Bounds.X && e.X < nudZoom.Bounds.Right
        //        && e.Y > nudZoom.Bounds.Y && e.Y < nudZoom.Bounds.Bottom)
        //        nudZoom.Visible = true;
        //    else
        //        nudZoom.Visible = false;
        //}
        //-------------------------------------------------------------------------
        //private void ImagePanel_MouseEnter(object sender, EventArgs e)
        //{
        //    nudZoom.Visible = false;
        //}
        //-------------------------------------------------------------------------
        //-------------------------------------------------------------------------
        //-------------------------------------------------------------------------
        //-------------------------------------------------------------------------
        #endregion
    }
}