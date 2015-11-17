using eDoctrinaUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
//using RegEx = System.Text.RegularExpressions;
namespace eDoctrinaOcrWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            version = assembly.GetName().Version;
            this.Title = "eDoctrina OCR Service v. " + version + " (" + Environment.CurrentDirectory + ")";
            Log.LogedMessage += ShowLogMessage;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            timer.Tick += timer_Tick;
            ShowLog(true);
        }
        Utils utils = new Utils();
        IOHelper iOHelper = new IOHelper();
        Log log = new Log();
        private string lastSheetIdentifier = "";
        private string appDir = "";
        Version version;
        //-------------------------------------------------------------------------
        #region Load/Save Settings
        //-------------------------------------------------------------------------
        private void LoadSettings()
        {
            try
            {
                defaults = new OcrAppConfig();
                var appSettings = new OcrWPFSettings();
                appSettings.Load();
                if (!appSettings.SettingsExists)
                {
                    SaveSettings();
                }
                else
                {
                    this.WindowState = appSettings.Fields.WindowState;
                    this.Left = appSettings.Fields.WindowLeft;
                    this.Top = appSettings.Fields.WindowTop;
                    if (appSettings.Fields.WindowWidth > 0)
                        this.Width = appSettings.Fields.WindowWidth;
                    if (appSettings.Fields.WindowHeight > 0)
                        this.Height = appSettings.Fields.WindowHeight;
                    this.nudThreadsCount.Value = appSettings.Fields.ThreadsCount;
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
            var appSettings = new OcrWPFSettings();
            appSettings.Fields.WindowState = this.WindowState;
            if (double.IsNaN(Left))
                appSettings.Fields.WindowLeft = 0;
            else
                appSettings.Fields.WindowLeft = this.Left;
            if (double.IsNaN(Top))
                appSettings.Fields.WindowTop = 0;
            else
                appSettings.Fields.WindowTop = this.Top;
            appSettings.Fields.WindowWidth = this.Width;
            appSettings.Fields.WindowHeight = this.Height;
            appSettings.Fields.ThreadsCount = this.nudThreadsCount.Value;
            appSettings.Save();
        }
        #endregion
        //-------------------------------------------------------------------------
        #region Init + Log + UpdateUI
        private delegate void ObjectDelegate(string key, object value);
        private void UpdateUI(string key, object value)
        {
            Dispatcher.Invoke(new ObjectDelegate(UpdateUII), key, value);
        }
        string fileQueue = "0";
        string frameQueue = "0";
        //-------------------------------------------------------------------------
        private void UpdateUII(string key, object value)
        {
            switch (key)
            {
                case "LbLQueue":
                    if (cancelSource.IsCancellationRequested) value = "0";
                    fileQueue = value.ToString();
                    lblQueue.Content = value.ToString();
                    Title = fileQueue + "(" + frameQueue + ")"
                        + appDir + " eDoctrina OCR Service v. " + version + " (" + Environment.CurrentDirectory + ")";
                    break;
                case "lblFramesQueue":
                    if (cancelSource.IsCancellationRequested) value = "0";
                    frameQueue = value.ToString();
                    lblFramesQueue.Content = frameQueue;
                    Title = fileQueue + "(" + frameQueue + ")"
                       + appDir + " eDoctrina OCR Service v. " + version + " (" + Environment.CurrentDirectory + ")";
                    break;
                case "lblFilesInWwork":
                    value = framesInWork.Count.ToString();
                    lblFilesInWork.Content = value.ToString();
                    break;
                case "lblFile":
                    //lblFile.Content = value.ToString();
                    //System.Windows.Controls.ToolTip tt = new System.Windows.Controls.ToolTip();
                    //tt.ToolTip = value.ToString();
                    break;
                case "lblTime":
                    lblTime.Content = value.ToString();
                    break;
                case "toolStripStatusLabel1":
                    if (cancelSource.IsCancellationRequested) value = "Stopping";
                    if (StatusTextBlock.Text != value.ToString())
                    {
                        StatusTextBlock.Text = value.ToString();
                        log.LogMessage(StatusTextBlock.Text.ToString());
                    }
                    break;
                case "nudThreadsCount":
                    if (defaults != null)
                    {
                        defaults.ThreadsAmount = nudThreadsCount.Value;
                    }
                    break;
                case "textBox1":
                    while (defaults.LogLength <= TextLog.LineCount)
                    {
                        TextLog.Text = TextLog.Text.Remove(TextLog.GetCharacterIndexFromLineIndex(TextLog.LineCount - 1) - 1);
                    }
                    TextLog.Text = TextLog.Text.Insert(0, value.ToString() + "\r\n");
                    break;
                case "Exception":
                    //Dispatcher.BeginInvoke(new Action(delegate
                    //{
                    labelMessages.Content = value;
                    labelMessages.Visibility = Visibility.Visible;
                    //}));

                    //MessageBox.Show(value.ToString(), Title, MessageBoxButton.OK, MessageBoxImage.Stop);
                    break;
                case "Message":
                    MessageBox.Show(value.ToString(), Title, MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case "ExceptionClose":
                    UpdateUI("Exception", value);
                    this.Close();
                    break;
            }
        }
        //-------------------------------------------------------------------------
        private void ShowLog(bool state)
        {
            OcrAppConfig.showLog = state;
            TextLog.IsEnabled = state;
            if (state)
            {
                TextLog.Visibility = System.Windows.Visibility.Visible;
                LogCloseButton.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                TextLog.Visibility = System.Windows.Visibility.Collapsed;
                LogCloseButton.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
        //-------------------------------------------------------------------------
        private void ShowLogMessage(object sender, EventArgs e)
        {
            if (sender != null && defaults != null)
            {
                string s = sender as string;
                if (s.Contains(" : ___"))
                    return;
                UpdateUI("textBox1", s);
            }
        }
        //-------------------------------------------------------------------------
        private OcrAppConfig defaults;
        private void Init()
        {
            defaults = new OcrAppConfig();
            if (defaults.exception != null)
            {
                UpdateUI("ExceptionClose", defaults.exception.Message);
                return;
            }
            if (string.IsNullOrEmpty(defaults.UpdateServerName))
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    MessageBox.Show("Not set update server", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                    btnUpdate.IsEnabled = false;
                }));
            }
            else
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    btnUpdate.IsEnabled = true;
                }));

            if (defaults.DualControl)
            {
                defaults.NotConfidentFolder = defaults.ErrorFolder;
            }
            if (!defaults.AutoStart)
            {
                UpdateUI("toolStripStatusLabel1", "Stopped");
            }
            if (defaults.DoNotProcess)
            {
                VerifyDirectory("TempIncFile");
                VerifyDirectory("OUTPUT_NOT_WHITE");
                VerifyDirectory("OUTPUT_WHITE");//Not white double
                VerifyDirectory("OUTPUT_NOT_WHITE_DOUBLE");
            }
            if (!VerifyDirectory(defaults.ConfigsFolder) || Directory.GetFiles(defaults.ConfigsFolder, "*.json").Length == 0)
            {
                string message = "Config files not found\r\nThe program will be closed.";
                log.LogMessage(message);
                UpdateUI("ExceptionClose", message);
                return;
            }
            UpdateUI("nudThreadsCount", null);
            if (defaults.AutoRunEditorOnError && string.IsNullOrEmpty(defaults.ManualInputFolder))
            {
                string message = "The eDoctrina OCR Editor can not be started\r\n because is not present \"ManualInputFolder\"";
                log.LogMessage(message);
                UpdateUI("Message", message);
            }
        }
        #endregion

        #region Main events
        //-------------------------------------------------------------------------
        private void Window_Initialized(object sender, EventArgs e)
        {
            LoadSettings();
        }
        //-------------------------------------------------------------------------
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            labelMessages.Visibility = Visibility.Hidden;
            string[] appDirArr = System.Text.RegularExpressions.Regex.Split(Environment.CurrentDirectory, "\\W+");
            if (appDirArr.Length > 0)
            {
                appDir = appDirArr[appDirArr.Length - 1];
            }
            Init();
            Start();
        }
        //-------------------------------------------------------------------------
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Stop();
            UpdateUI("toolStripStatusLabel1", "Closing");
            SaveSettings();
        }
        //-------------------------------------------------------------------------
        private void FolderTestsButton_Click(object sender, RoutedEventArgs e)
        {
            log.LogMessage("___" + "FolderTestsButton_Click");
            if (VerifyDirectory(defaults.InputFolder))
                Process.Start(defaults.InputFolder);
        }
        //-------------------------------------------------------------------------
        private void FolderSuccessResultsButton_Click(object sender, RoutedEventArgs e)
        {
            log.LogMessage("___" + "FolderSuccessResultsButton_Click");
            if (VerifyDirectory(defaults.SuccessFolder))
                Process.Start(defaults.SuccessFolder);
        }
        //-------------------------------------------------------------------------
        private void FolderErrorsResultsButton_Click(object sender, RoutedEventArgs e)
        {
            log.LogMessage("___" + "FolderErrorsResultsButton_Click");
            if (defaults.NotConfidentFolder != defaults.SuccessFolder && defaults.NotConfidentFolder != defaults.ErrorFolder)
            {
                if (VerifyDirectory(defaults.NotConfidentFolder))
                    Process.Start(defaults.NotConfidentFolder);
            }
            if (VerifyDirectory(defaults.ErrorFolder))
                Process.Start(defaults.ErrorFolder);
        }
        //-------------------------------------------------------------------------
        private void FolderArchiveButton_Click(object sender, RoutedEventArgs e)
        {
            log.LogMessage("___" + "FolderArchiveButton_Click");
            if (VerifyDirectory(defaults.ArchiveFolder))
                Process.Start(defaults.ArchiveFolder);
        }
        //-------------------------------------------------------------------------
        private void LogButton_Click(object sender, RoutedEventArgs e)
        {
            ShowLog(!TextLog.IsEnabled);
        }
        //-------------------------------------------------------------------------
        private void LogCloseButton_Click(object sender, RoutedEventArgs e)
        {
            ShowLog(false);
        }
        //-------------------------------------------------------------------------
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            isPause = false;
            Start();
        }
        //-------------------------------------------------------------------------
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            isPause = false;
            Stop();
        }
        //-------------------------------------------------------------------------
        private void nudThreadsCount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            if (defaults != null)
            {
                defaults.ThreadsAmount = nudThreadsCount.Value;
            }
            //SaveSettings();
        }
        #endregion
        //-------------------------------------------------------------------------
        #region Timer + Work
        private void Start()
        {
            if (!defaults.AutoStart || !timer.IsEnabled)
            {
                defaults.AutoStart = true;
                cancelSource = new System.Threading.CancellationTokenSource();
                UpdateUI("toolStripStatusLabel1", "Starting");
                UpdateUI("toolStripStatusLabel1", "Watching");
                timer.Start();
            }
        }
        //-------------------------------------------------------------------------
        private void Stop()
        {
            if (defaults.AutoStart)
            {
                UpdateUI("toolStripStatusLabel1", "Stopping");
                defaults.AutoStart = false;
                timer.Stop();
                cancelSource.Cancel();
                UpdateUI("LbLQueue", "0");
                UpdateUI("lblFramesQueue", "0");
            }
        }
        //-------------------------------------------------------------------------
        private System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
        System.Threading.CancellationTokenSource cancelSource = new System.Threading.CancellationTokenSource();
        private void timer_Tick(object sender, EventArgs e)
        {
            if (mainTask == null || mainTask.IsCanceled || mainTask.IsCompleted || mainTask.IsFaulted)
            {
                mainTask = Task.Factory.StartNew(status => mainTask_DoWork(), "mainTask").ContinueWith((t) => Completed(t));
            }
        }
        //-------------------------------------------------------------------------
        private Task mainTask;
        private void mainTask_DoWork()
        {
            IsChangeAppConfig();
            StartEditor();
            FilesForRecognition();
            ioHelper.ReDelete();
        }
        //-------------------------------------------------------------------------
        private void IsChangeAppConfig()
        {
            if (defaults.IsChangeAppConfig())
            {
                Init();
                log.LogMessage(OcrAppConfig.AppConfigFileName + " reloading");
            }
        }
        //-------------------------------------------------------------------------
        private bool canStartEditor = true;
        private void StartEditor()
        {
            if (!string.IsNullOrEmpty(defaults.ManualInputFolder) && defaults.AutoRunEditorOnError && VerifyDirectory(defaults.ErrorFolder, "StartEditor"))
            {
                string[] FilesInErrorFolder = utils.GetSupportedFilesFromDirectory(defaults.ErrorFolder, SearchOption.AllDirectories);
                if (FilesInErrorFolder.Length > 0)
                {
                    Process[] pr = Process.GetProcessesByName("eDoctrinaOcrEd");
                    if (pr.Length == 0)
                    {
                        log.LogMessage("___" + "Editor is starting...");
                        Process.Start("eDoctrinaOcrEd.exe");
                        SendMail();
                    }
                }
            }
        }
        //-------------------------------------------------------------------------
        private bool canRecognitionFile = true;
        private bool canFindDouble = false;
        private void FilesForRecognition()
        {
            if (VerifyDirectory(OcrAppConfig.TempFramesFolder, "RecognitionFile"))
            {
                var filesInTempFramesFolder = utils.GetSupportedFilesFromDirectory(OcrAppConfig.TempFramesFolder
                    , SearchOption.TopDirectoryOnly, true);
                var countFiles = filesInTempFramesFolder.Length;
                UpdateUI("lblFramesQueue", countFiles.ToString());
                FilesForWork(false, countFiles);
                FilesForWork(true, countFiles);

                if (countFiles > 0)
                {
                    canFindDouble = true;
                    if (defaults.ThreadsAmount > 0)
                        UpdateUI("toolStripStatusLabel1", "Working");
                    WorkingTempFramesFolder(filesInTempFramesFolder);
                }
                else
                {
                    if (defaults.DoNotProcess && canFindDouble)
                    {
                        string folder = "OUTPUT_WHITE";// defaults.EmptyScansFolder;
                        //string Not white
                        var filesInInputOrTempFolder = utils.GetSupportedFilesFromDirectory
                            (folder, SearchOption.TopDirectoryOnly);
                        //var countFiles = filesInInputOrTempFolder.Length; //string folder = (isInputFolder) ? defaults.InputFolder : OcrAppConfig.TempFolder;
                        if (filesInInputOrTempFolder.Length > 0)
                        {
                            UpdateUI("toolStripStatusLabel1", "Search duplicates");
                            foreach (string item in filesInInputOrTempFolder)
                            {
                                FileInfo fi1 = new FileInfo(item);
                                string name1 = fi1.Name;
                                name1 = System.Text.RegularExpressions.Regex.Replace(name1, "-\\d+", "");
                                name1 = System.Text.RegularExpressions.Regex.Replace(name1, fi1.Extension, "");
                                var filesInInputNotWhite = utils.GetSupportedFilesFromDirectory
                               ("OUTPUT_NOT_WHITE", SearchOption.TopDirectoryOnly);
                                foreach (var itm in filesInInputNotWhite)
                                {
                                    FileInfo fi2 = new FileInfo(itm);
                                    string name2 = fi2.Name;
                                    name2 = System.Text.RegularExpressions.Regex.Replace(name2, "-\\d+", "");
                                    name2 = System.Text.RegularExpressions.Regex.Replace(name2, fi2.Extension, "");
                                    if (name1 == name2)
                                    {
                                        string dest = Path.Combine("OUTPUT_NOT_WHITE_DOUBLE", fi2.Name);
                                        try
                                        {
                                            fi2.MoveTo(dest);
                                        }
                                        catch (Exception)
                                        {
                                        }
                                        try
                                        {
                                            fi1.Delete();
                                        }
                                        catch (Exception)
                                        {
                                        }
                                    }
                                }
                            }
                        }
                        canFindDouble = false;
                    }
                    UpdateUI("toolStripStatusLabel1", "Watching");
                }
            }
        }
        //-------------------------------------------------------------------------
        private void DoWork(object sender, DoWorkEventArgs e)
        {
            var obj = (int)e.Argument;
            //FilesForWork(true, obj);
            runWorker = true;
            //e.Result = e.Argument;
            var searchOption = SearchOption.AllDirectories;
            var filesInInputOrTempFolder = utils.GetSupportedFilesFromDirectory(defaults.InputFolder, searchOption);
            var countFiles = filesInInputOrTempFolder.Length;
            UpdateUI("LbLQueue", countFiles.ToString());
            if (countFiles > 0)
            {
                //if (defaults.AutoStart && defaults.ThreadsAmount > 0 && (defaults.ThreadsAmount * 2 > countFilesInTempFramesFolder || 10 > countFilesInTempFramesFolder))
                int freeThreadsAmount = (int)((defaults.ThreadsAmount * 8 > 10) ? defaults.ThreadsAmount * 8 : 10) - obj;
                if (defaults.AutoStart && defaults.ThreadsAmount > 0)// && freeThreadsAmount > 0
                {
                    if (incomingFileTask == null || incomingFileTask.IsCanceled || incomingFileTask.IsCompleted || incomingFileTask.IsFaulted)
                    {
                        log.LogMessage("___" + "Runing Task incomingFileTask");
                        incomingFileTask = Task.Factory.StartNew(status
                            => incomingFileTask_DoWork(new List<string>(filesInInputOrTempFolder), true, freeThreadsAmount), "incomingFileTask")
                            .ContinueWith((t) => Completed(t));
                    }
                }
            }
        }
        //-------------------------------------------------------------------------
        bool runWorker = false;
        //-------------------------------------------------------------------------
        private void Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            runWorker = false;
        }
        //-------------------------------------------------------------------------
        private bool canFilesForWork = true;
        private void FilesForWork(bool isInputFolder, int countFilesInTempFramesFolder)
        {
            string folder = (isInputFolder) ? defaults.InputFolder : OcrAppConfig.TempFolder;
            if (VerifyDirectory(folder, "FilesForWork"))
            {
                if (isInputFolder && (framesInWork.Count > defaults.ThreadsAmount * 8 || runWorker))
                    return;
                else
                {
                    if (!isPause && isInputFolder && !runWorker)
                    {
                        BackgroundWorker BW = new BackgroundWorker();
                        BW.DoWork += DoWork;
                        BW.RunWorkerCompleted += Completed;
                        BW.WorkerSupportsCancellation = true;
                        var filesInTempFramesFolder2 = utils.GetSupportedFilesFromDirectory(OcrAppConfig.TempFramesFolder
                            , SearchOption.TopDirectoryOnly, true);//+, true
                        var countFiles2 = filesInTempFramesFolder2.Length;
                        BW.RunWorkerAsync(countFiles2);
                        return;
                    }
                    if (isInputFolder)
                        return;
                }
                var searchOption = (isInputFolder) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                string[] filesInInputOrTempFolder = new string[0];
                if (!isInputFolder || isInputFolder && (framesInWork.Count < defaults.ThreadsAmount * 8 && !runWorker))
                {
                    filesInInputOrTempFolder = utils.GetSupportedFilesFromDirectory(folder, searchOption);
                }

                var countFiles = filesInInputOrTempFolder.Length;
                if (isInputFolder && (framesInWork.Count < defaults.ThreadsAmount * 8 && !runWorker))
                    UpdateUI("LbLQueue", countFiles.ToString());
                if (countFiles > 0)
                {
                    //if (defaults.AutoStart && defaults.ThreadsAmount > 0 && (defaults.ThreadsAmount * 2 > countFilesInTempFramesFolder || 10 > countFilesInTempFramesFolder))
                    int freeThreadsAmount = (int)((defaults.ThreadsAmount * 8 > 10) ? defaults.ThreadsAmount * 8 : 10) - countFilesInTempFramesFolder;
                    if (defaults.AutoStart && defaults.ThreadsAmount > 0)// && freeThreadsAmount > 0
                    {
                        if (incomingFileTask == null || incomingFileTask.IsCanceled || incomingFileTask.IsCompleted || incomingFileTask.IsFaulted)
                        {
                            log.LogMessage("___" + "Runing Task incomingFileTask");
                            incomingFileTask = Task.Factory.StartNew(status
                                => incomingFileTask_DoWork(new List<string>(filesInInputOrTempFolder), isInputFolder, freeThreadsAmount), "incomingFileTask")
                                .ContinueWith((t) => Completed(t));
                        }
                    }
                }
            }
        }
        //-------------------------------------------------------------------------
        private bool canArchive = true;
        private Task incomingFileTask;
        //-------------------------------------------------------------------------
        private void incomingFileTask_DoWork(List<string> filesInInputOrTempFolder, bool isInputFolder, int freeThreadsAmount)
        {
            if (VerifyDirectory(defaults.ArchiveFolder, "ArchiveFolder"))
            {
                while (freeThreadsAmount > 0 && !cancelSource.IsCancellationRequested)
                {
                    var incomingFile = new ProcessingIncomingFile(defaults, filesInInputOrTempFolder, isInputFolder);
                    if (incomingFile.Result != null)
                    {
                        var pageCount = incomingFile.GetFrames();
                        freeThreadsAmount = freeThreadsAmount - ((pageCount > 0) ? pageCount : 1);
                        filesInInputOrTempFolder.Remove(incomingFile.Result.SourceFileName);
                    }
                    else { return; }
                }
            }
        }
        //-------------------------------------------------------------------------
        private List<string> framesInWork = new List<string>();
        private void WorkingTempFramesFolder(string[] incomingFiles)
        {
            foreach (var file in incomingFiles)
            {
                int count;
                bool containsFile;
                bool containsFileInReDeleteList;
                lock (framesInWork)
                {
                    count = framesInWork.Count;
                    containsFile = framesInWork.Contains(file);
                }
                lock (ioHelper.ReDeleteList)
                {
                    containsFileInReDeleteList = ioHelper.ReDeleteList.Contains(file);
                }
                if (count >= defaults.ThreadsAmount)
                {
                    log.LogMessage("___" + "WorkingTempFramesFolder No free thread");
                    break;
                }
                if (!containsFile && !containsFileInReDeleteList)
                {
                    if (utils.CanAccess(file))
                    {
                        AddTask(file);
                        var taskName = file;
                        log.LogMessage("___" + "WorkingTempFramesFolder " + "DoWork " + (count + 1).ToString() + " Start");
                        Task.Factory.StartNew(state => DoWork(taskName, DateTime.Now, ref lastSheetIdentifier), taskName, cancelSource.Token)
                            .ContinueWith((t) => Completed(t));
                    }
                    else
                    {
                        log.LogMessage("___" + "Can not access to file: " + file);
                    }
                }
            }
        }
        //-------------------------------------------------------------------------
        private List<string> errorFiles = new List<string>();
        private IOHelper ioHelper = new IOHelper();
        private void DoWork(string fileName, DateTime dateTime, ref string lastSheetIdentifier)
        {
            log.LogMessage("___" + "DoWork Begin");
            new ProcessingTempFrameFile(cancelSource.Token, defaults, fileName, errorFiles, ioHelper, ref lastSheetIdentifier);
            RemoveTask(fileName);
            if (!cancelSource.IsCancellationRequested)
            {
                //UpdateUI("lblFile", Path.GetFileName(fileName));
                double d = (DateTime.Now - dateTime).TotalSeconds;
                d = Math.Round(d, 3);
                UpdateUI("lblTime", d.ToString() + " sec");
            }
            log.LogMessage("___" + "DoWork End");
        }
        //-------------------------------------------------------------------------
        private void Completed(Task t)
        {
            if (t.Status.ToString() != "RanToCompletion" && t.AsyncState.ToString() != "mainTask")
                log.LogMessage("___" + "Completed Task : " + t.AsyncState.ToString() + " with Status : " + t.Status.ToString());
            RemoveTask(t.AsyncState.ToString());
            //if (t.IsCanceled) Log.LogMessage("Recognizing was cancelled");
            if (t.Exception != null)
            {
                log.LogMessage(t.Exception);
            }
            t.Dispose();
            GC.Collect();
        }
        //-------------------------------------------------------------------------
        private void AddTask(string fileName)
        {
            lock (framesInWork)
            {
                UpdateUI("lblFilesInWwork", "");
                framesInWork.Add(fileName);
                log.LogMessage("___" + "AddTask framesInWork.Add(file) : " + fileName);
            }
        }
        //-------------------------------------------------------------------------
        private void RemoveTask(string fileName)
        {
            lock (framesInWork)
            {
                if (framesInWork.Contains(fileName))
                {
                    framesInWork.Remove(fileName);
                    log.LogMessage("___" + "RemoveTask framesInWork.Remove(file) : " + fileName);
                }
                UpdateUI("lblFilesInWwork", "");
            }
        }
        //-------------------------------------------------------------------------
        private void RemoveAllTask()
        {
            lock (framesInWork)
            {
                framesInWork.Clear();
                log.LogMessage("___" + "RemoveAllTask framesInWork.Clear()");
                UpdateUI("lblFilesInWwork", "");
            }
        }
        #endregion

        #region VerifyDirectory
        //-------------------------------------------------------------------------
        private bool VerifyDirectory(string dir, string key = "")
        {
            if (iOHelper.CreateDirectory(dir, false))
            {
                if (dir == OcrAppConfig.TempFolder || dir == OcrAppConfig.TempFramesFolder)
                    return true;
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    labelMessages.Visibility = Visibility.Hidden;
                }));

                switch (key)
                {
                    case "StartEditor":
                        if (!canStartEditor) canStartEditor = true;
                        break;
                    case "FilesForWork":
                        if (!canFilesForWork) canFilesForWork = true;
                        break;
                    case "RecognitionFile":
                        if (!canRecognitionFile) canRecognitionFile = true;
                        break;
                    case "ArchiveFolder":
                        if (!canArchive) canArchive = true;
                        break;
                }
                return true;
            }
            else
            {
                switch (key)
                {
                    case "StartEditor":
                        if (canStartEditor)
                        {
                            canStartEditor = false;
                            GetVerifyDirectoryMessage(dir);
                        }
                        break;
                    case "FilesForWork":
                        if (canFilesForWork)
                        {
                            canFilesForWork = false;
                            GetVerifyDirectoryMessage(dir);
                        }
                        break;
                    case "RecognitionFile":
                        if (canRecognitionFile)
                        {
                            canRecognitionFile = false;
                            GetVerifyDirectoryMessage(dir);
                        }
                        break;
                    case "ArchiveFolder":
                        if (canArchive)
                        {
                            canArchive = false;
                            GetVerifyDirectoryMessage(dir);
                        }
                        break;
                    case "":
                        GetVerifyDirectoryMessage(dir);
                        break;
                }
                return false;
            }
        }
        //-------------------------------------------------------------------------
        private void GetVerifyDirectoryMessage(string dir)
        {
            var message = "Could not find part of the path " + dir;
            log.LogMessage(message);
            SendMail(message);
            UpdateUI("Exception", message);
        }
        #endregion
        //-------------------------------------------------------------------------
        private void SendMail(string caption = null)
        {
            Task.Factory.StartNew(status =>
            {
                MailHelper mail = new MailHelper();
                mail.SendMail(caption);
            }, "SendMail").ContinueWith((t) => Completed(t));
        }
        //-------------------------------------------------------------------------
        bool isPause = false;
        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            isPause = true;
        }
        //-------------------------------------------------------------------------
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveSettings();
                //StopButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                Process.Start("eDoctrinaOcrUpdate.exe");

                //WebClient client = new WebClient();
                //string reply = "";
                //if (defaults.UpdateServerName.StartsWith("http"))
                //{
                //    reply = client.DownloadString(Path.Combine(defaults.UpdateServerName, "edoc_ocr_update", "updateInfo.txt"));
                //}
                //else
                //{
                //    reply = File.ReadAllText(Path.Combine(defaults.UpdateServerName, "updateInfo.txt"));
                //}
                //string[] papams = Regex.Split(reply, "\\s+");
                //if (papams.Length == 0)
                //{
                //    MessageBox.Show("Update error", Title, MessageBoxButton.OK, MessageBoxImage.Stop);
                //    return;
                //}
                ////if (version.Revision.ToString() == papams[0])
                //if (version.ToString() == papams[0])
                //{
                //    MessageBox.Show("You are using the latest version of the application."
                //        , Title, MessageBoxButton.OK, MessageBoxImage.Information);
                //    return;
                //}
                //if (MessageBox.Show(//"Updates found. Install them?"
                //            "Version "+ version.ToString()+ " found. Install them?"
                //            , Title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                //{
                //    return;
                //}
                //StopButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                //if (VerifyDirectory("Updates"))
                //{
                //    try
                //    {
                //        DirectoryInfo dirInfo = new DirectoryInfo("Updates");
                //        foreach (FileInfo file in dirInfo.GetFiles())
                //        {
                //            file.Delete();
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Stop);
                //        StartButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent)); ;// Start();
                //        return;
                //    }
                //}
                //for (int i = 1; i < papams.Length; i++)
                //{
                //    try
                //    {
                //        UpdateUI("toolStripStatusLabel1", "Loading " + papams[i]);
                //        string dest = Path.Combine("Updates", papams[i]);
                //        if (defaults.UpdateServerName.StartsWith("http"))
                //        {
                //            string source = Path.Combine(defaults.UpdateServerName, "edoc_ocr_update", papams[i]);
                //            Uri uri = new Uri(source);
                //            client.DownloadFile(uri, dest);
                //        }
                //        else
                //            File.Copy(Path.Combine(defaults.UpdateServerName, papams[i]), dest);
                //}
                //        catch (Exception ex)
                //        {
                //            MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Stop);
                //            StartButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
                //            return;
                //        }
                //    }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Stop);
                StartButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            }
        }
        //-------------------------------------------------------------------------
        //-------------------------------------------------------------------------
        //-------------------------------------------------------------------------
        //-------------------------------------------------------------------------
    }
}