using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.IO;
using System.Linq;
//using System.Web.Script.Serialization;
using Newtonsoft.Json;
using eDoctrinaUtils;
using System.Threading;
using System.Text.RegularExpressions;
using System.Windows.Threading;
//using System.Windows.Forms;

namespace eDoctrinaOcrTestWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public static class ExtensionMethods
    {

        private static Action EmptyDelegate = delegate () { };


        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SaveButton.IsEnabled = false;
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;
            this.Title = "eDoctrina OCR Integration Test v. " + version + " (" + Environment.CurrentDirectory + ")";
        }
        //-------------------------------------------------------------------------
        private CollectionViewSource collectionViewSource;
        private FileView fileView;
        private MainWindowView view;
        //-------------------------------------------------------------------------
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (fileView != null && fileView.IsWorking)
            {
                fileView.Cancel();
            }
        }
        //-------------------------------------------------------------------------
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (fileView != null && fileView.IsWorking) return;
            var sourcePath = "";
            if (view.IsTestingMode)
            {
                if (view.EtalonPathTextBoxForeground != Brushes.Black || view.AppConfigPathTextBoxForeground != Brushes.Black) return;
                sourcePath = view.AppConfigPathTextBox;
            }
            else
            {
                if (view.PathTextBoxForeground != Brushes.Black) return;
                sourcePath = view.PathTextBox;
            }
            fileView = new FileView();
            fileView.WorkingCompleted += Completed;
            view.ErrorText = "Waiting...";
            fileView.WorkingAsync(view.IsTestingMode, sourcePath, view.EtalonPathTextBox);
        }
        //-------------------------------------------------------------------------
        private void ChangeGrouping(params string[] columns)
        {
            collectionViewSource.GroupDescriptions.Clear();
            foreach (var column in columns)
            {
                collectionViewSource.GroupDescriptions.Add(new PropertyGroupDescription(column));
            }
        }
        //-------------------------------------------------------------------------
        private void Completed(object sender, EventArgs e)
        {
            fileView.WorkingCompleted -= Completed;
            view.CVSSource = fileView.Files;
            ChangeGrouping("State");
            view.FilesCount = fileView.CountFiles.ToString();
            view.ErrorText = (view.IsTestingMode) ? CompletedTestingMode() : CompletedNotTestingMode();
            if (view.IsTestingMode)
            {
                try
                {
                    //StartButton.IsEnabled = false;
                    var defaults = new OcrAppConfig(fileView.SourcePath);
                    string jsonPath = defaults.OutputFolder_Success;
                    string[] fnArr = Directory.GetFiles(jsonPath, "*.json*", SearchOption.TopDirectoryOnly);
                    //string err = view.ErrorText;
                    Utils utils = new Utils();
                    int count = fnArr.Length;
                    foreach (var item in fnArr)
                    {
                        //view.ErrorText = count.ToString();
                        StartButton.Content = count.ToString();
                        StartButton.Refresh();
                        count--;
                        string s = File.ReadAllText(item);
                        AnswerVerification answerVerification = utils.GetAnswerVerification(s);
                        s = File.ReadAllText(answerVerification.audit.dataFileName).Trim();
                        //s = File.ReadAllText(Path.Combine(defaults.SuccessFolder
                        //   , answerVerification.data.district_id + "_"
                        //   + answerVerification.data.student_uid + "_"
                        //   + answerVerification.data.test_id + "_"
                        //   + answerVerification.data.index_of_first_question + "_"
                        //   + answerVerification.data.amout_of_questions
                        //   + ".csv")).Trim();
                        string[] stringsCSV = Regex.Split(s, Environment.NewLine);
                        if (answerVerification.answers.Length != stringsCSV.Length)
                        {
                            MessageBox.Show("answerVerification.answers.Length != .csv rows length");
                            break;
                        }
                        for (int i = 0; i < answerVerification.answers.Length; i++)
                        {
                            //this.Dispatcher.BeginInvoke(new Action(() => this.ErrorLabel.Content = i.ToString()), null);
                            //Thread.Sleep(50);
                            s = stringsCSV[i];
                            string s2 = answerVerification.data.district_id + ","
                             + answerVerification.data.student_uid + ","
                             + answerVerification.data.test_id + ",";
                            if (s.IndexOf(s2) != 0)
                            {
                                MessageBox.Show("Problem №1 in answerVerification.answers "
                                    + item + " " + i, ToString());
                                break;
                            }
                            s = s.Remove(0, s2.Length);
                            int index = s.IndexOf(",,EDOCOCR");
                            s = s.Substring(0, index);
                            string[] ss2 = s.Split(',');
                            if (ss2[0] != answerVerification.answers[i].idx.ToString())
                            {
                                MessageBox.Show("Problem №2 in answerVerification.answers "
                                    + item + " " + i, ToString());
                                break;
                            }
                            ss2 = ss2[1].Split('~');
                            if (answerVerification.answers[i].answers.Length != ss2.Length)
                            {
                                MessageBox.Show("Problem №3 in answerVerification.answers "
                                   + item + " " + i, ToString());
                                break;
                            }
                            for (int j = 0; j < ss2.Length; j++)
                            {//строки и подстроки
                                var itm = ss2[j];
                                string[] ss3 = itm.Split('|');
                                //string[] ss4 = answerVerification.answers[i].answers[j].cols[k].Split('|');
                                for (int k = 0; k < ss3.Length; k++)
                                {//несеолько пузырей в строке
                                    var item2 = ss3[k];
                                    if (answerVerification.answers[i].answers[j].cols[k] != item2)
                                    {
                                        MessageBox.Show("Problem №4 in answerVerification.answers "
                                      + item + " " + i, ToString());
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    //view.ErrorText = err;
                    //StartButton.IsEnabled = true;
                    StartButton.Content = "Start";
                }
            }
        }
        //-------------------------------------------------------------------------
        private string CompletedTestingMode()
        {
            var text = "";
            SaveButton.IsEnabled = false;
            SaveButton.Content = "Save etalon data files";
            view.EtalonFilesCount = fileView.CountEtalonFiles.ToString();
            view.CVSSource = fileView.ResultFiles;
            view.HasExtraFiles = fileView.CountExtra > 0;
            if (fileView.CountError != 0)
            {
                text = "Found file with error...";
            }
            else
                if (fileView.CountMissing != 0)
                text = "Found missing file...";
            else
                    if (fileView.CountExtra != 0)
                text = "Found extra file...";
            //var defaults = new OcrAppConfig(fileView.SourcePath);
            //string jsonPath = defaults.OutputFolder_Success;
            //string[] fnArr = Directory.GetFiles(jsonPath, "*.json*", SearchOption.TopDirectoryOnly);
            //string err = view.ErrorText;
            //try
            //{
            //    Utils utils = new Utils();
            //    int count = fnArr.Length;
            //    foreach (var item in fnArr)
            //    {
            //        view.ErrorText = count.ToString();
            //        //ErrorLabel.Content = count.ToString();
            //        ErrorLabel.Refresh();
            //        count--;
            //        string s = File.ReadAllText(item);
            //        AnswerVerification answerVerification = utils.GetAnswerVerification(s);
            //        s = File.ReadAllText(Path.Combine(defaults.SuccessFolder
            //            , answerVerification.data.district_id + "_"
            //            + answerVerification.data.student_uid + "_"
            //            + answerVerification.data.test_id + "_"
            //            + answerVerification.data.index_of_first_question + "_"
            //            + answerVerification.data.amout_of_questions
            //            + ".csv")).Trim();
            //        string[] ss = Regex.Split(s, Environment.NewLine);
            //        if (answerVerification.answers.Length != ss.Length)
            //        {
            //            MessageBox.Show("answerVerification.answers.Length != csv string.Length");
            //            break;
            //        }
            //        for (int i = 0; i < answerVerification.answers.Length; i++)
            //        {
            //            //this.Dispatcher.BeginInvoke(new Action(() => this.ErrorLabel.Content = i.ToString()), null);
            //            //Thread.Sleep(50);
            //            s = ss[i];
            //            string s2 = answerVerification.data.district_id + ","
            //             + answerVerification.data.student_uid + ","
            //             + answerVerification.data.test_id + ",";
            //            if (s.IndexOf(s2) != 0)
            //            {
            //                MessageBox.Show("Problem №1 in answerVerification.answers "
            //                    + item + " " + i, ToString());
            //                break;
            //            }
            //            s = s.Remove(0, s2.Length);
            //            int index = s.IndexOf(",,EDOCOCR");
            //            s = s.Substring(0, index);
            //            string[] ss2 = s.Split(',');
            //            if (ss2[0] != answerVerification.answers[i].idx.ToString())
            //            {
            //                MessageBox.Show("Problem №2 in answerVerification.answers "
            //                    + item + " " + i, ToString());
            //                break;
            //            }
            //            ss2 = ss2[1].Split('~');
            //            if (answerVerification.answers[i].answers.Length != ss2.Length)
            //            {
            //                MessageBox.Show("Problem №3 in answerVerification.answers "
            //                   + item + " " + i, ToString());
            //                break;
            //            }
            //            for (int j = 0; j < ss2.Length; j++)
            //            {
            //                var itm = ss2[j];
            //                string[] ss3 = itm.Split('|');
            //                for (int k = 0; k < ss3.Length; k++)
            //                {
            //                    var item2 = ss3[k];
            //                    if (answerVerification.answers[i].answers[j].cols[k] != item2)
            //                    {
            //                        MessageBox.Show("Problem №4 in answerVerification.answers "
            //                      + item + " " + i, ToString());
            //                        return text;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
            //finally
            //{
            //    view.ErrorText = err;
            //}    
            return text;
        }
        //-------------------------------------------------------------------------
        private string CompletedNotTestingMode()
        {
            var text = "";
            if (fileView.HasDuplicateSource)
            {
                text = "Found duplicate SHA1 in *.tiff file...";
                view.CVSSource = fileView.DuplicateFiles;
                ChangeGrouping("UniqueIdentifier");
                view.HasDuplicateOptions = true;
            }
            else
                if (fileView.CountUnicNames != fileView.CountTiffFiles)
                text = "Missed *.tiff files...";
            else
                    if (fileView.CountUnicNames != fileView.CountCsvFiles)
                text = "Has some missing *.csv files... Can create etalon data files";
            //else
            //if (fileView.HasDuplicateSha1Tiff)
            //    text = "Finding some duplicate SHA1 in *.tiff file with diferent SHA1 in *.csv file...";
            if (text != "")
            {
                if (fileView.CountUnicNames != fileView.CountCsvFiles && !fileView.HasDuplicateSource)
                    SaveButton.Content = "Save etalon data files";
                else
                {
                    text += " Can not create etalon data files!";
                    SaveButton.Content = "Delete duplicate files";
                    //SaveButton.IsEnabled = false;
                }
                SaveButton.IsEnabled = true;
            }
            else
            {
                SaveButton.IsEnabled = true;
            }
            return text;
        }
        //-------------------------------------------------------------------------
        private void CheckBox_CheckedChange(object sender, RoutedEventArgs e)
        {
            if (view.ShowAllFiles)
            {
                view.CVSSource = fileView.Files;
                ChangeGrouping("State");
            }
            else
            {
                view.CVSSource = fileView.DuplicateFiles;
                ChangeGrouping("UniqueIdentifier");
            }
        }
        //-------------------------------------------------------------------------
        #region Save Open File Folder
        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (view.CVSSource != null && ResultListView.SelectedItem != null)
            {
                try
                {
                    var selectedFile = (ResultListView.SelectedItem as FileItem).GetFullFileName();
                    System.Diagnostics.Process.Start("explorer.exe", String.Format("/select,\"{0}\"", selectedFile));
                }
                catch { }
            }
        }
        //-------------------------------------------------------------------------
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var confirmName = ShowConfirmWindow();
            fileView.AddResult(fileView.EtalonPath, confirmName);
        }
        //-------------------------------------------------------------------------

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            switch (SaveButton.Content as string)
            {
                case "Save etalon data files":
                    var confirmName = ShowConfirmWindow();
                    Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                    saveFileDialog.Filter = "CSV File (*.csv)|*.csv";
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        fileView.SaveResult(saveFileDialog.FileName, confirmName);
                    }

                    break;
                default:
                    var count = fileView.Files.GroupBy(x => x.UniqueIdentifier).Count();
                    break;
            }
        }
        //-------------------------------------------------------------------------
        private string ShowConfirmWindow()
        {
            ConfirmWindow modalWindow = new ConfirmWindow();
            modalWindow.Owner = this;
            view.ShowEffect = true;
            modalWindow.ShowDialog();
            var name = modalWindow.NameTextBox.Text;
            view.ShowEffect = false;
            return name;
        }
        //-------------------------------------------------------------------------
        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.InitialDirectory = "";
            openFileDialog.Filter = ((sender as Button).Name == "EtalonPathButton") ? "CSV File (*.csv)|*.csv" : "AppConfig File (appConfig.json)|appConfig.json";//|All files (*.*)|*.*
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == true)
            {
                if ((sender as Button).Name == "EtalonPathButton")
                    view.EtalonPathTextBox = openFileDialog.FileName;
                else
                    view.AppConfigPathTextBox = openFileDialog.FileName;
            }
        }
        //-------------------------------------------------------------------------
        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                view.PathTextBox = folderBrowserDialog.SelectedPath;
            }
        }
        #endregion
        //-------------------------------------------------------------------------
        #region PathTextBox & EtalonPathTextBox
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            switch ((sender as TextBox).Name)
            {
                case "PathTextBox":
                    if (view.PathTextBox == MainWindowView.PathDef)
                    {
                        view.PathTextBox = "";
                    }
                    break;
                case "EtalonPathTextBox":
                    if (view.EtalonPathTextBox == MainWindowView.EtalonPathDef)
                    {
                        view.EtalonPathTextBox = "";
                    }
                    break;
                case "AppConfigPathTextBox":
                    if (view.AppConfigPathTextBox == MainWindowView.AppConfigPathDef)
                    {
                        view.AppConfigPathTextBox = "";
                    }
                    break;
            }
        }
        //-------------------------------------------------------------------------
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            switch ((sender as TextBox).Name)
            {
                case "PathTextBox":
                    if (view.PathTextBox == "")
                    {
                        view.PathTextBox = MainWindowView.PathDef;
                    }
                    break;
                case "EtalonPathTextBox":
                    if (view.EtalonPathTextBox == "")
                    {
                        view.EtalonPathTextBox = MainWindowView.EtalonPathDef;
                    }
                    break;
                case "AppConfigPathTextBox":
                    if (view.AppConfigPathTextBox == "")
                    {
                        view.AppConfigPathTextBox = MainWindowView.AppConfigPathDef;
                    }
                    break;
            }
        }
        //-------------------------------------------------------------------------
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            switch ((sender as TextBox).Name)
            {
                case "PathTextBox":
                    view.PathTextBox = (sender as TextBox).Text;
                    break;
                case "EtalonPathTextBox":
                    view.EtalonPathTextBox = (sender as TextBox).Text;
                    break;
                case "AppConfigPathTextBox":
                    view.AppConfigPathTextBox = (sender as TextBox).Text;
                    break;
            }
        }
        #endregion
        //-------------------------------------------------------------------------
        #region Load/Save Settings
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SaveSettings();
        }
        //-------------------------------------------------------------------------
        private void Window_Initialized(object sender, EventArgs e)
        {
            view = new MainWindowView();
            this.DataContext = view;
            view.ViewEtaloneMode = Resources["ViewEtaloneMode"] as GridView;
            view.ViewTestingMode = Resources["ViewTestingMode"] as GridView;
            collectionViewSource = Resources["cvs"] as CollectionViewSource;
            LoadSettings();
        }

        private void LoadSettings()
        {
            var appSettings = new OcrTestWPFSettings();
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
                this.Width = appSettings.Fields.WindowWidth;
                this.Height = appSettings.Fields.WindowHeight;
                if (!String.IsNullOrEmpty(appSettings.Fields.Path))
                    view.PathTextBox = appSettings.Fields.Path;
                if (!String.IsNullOrEmpty(appSettings.Fields.EtalonPath))
                    view.EtalonPathTextBox = appSettings.Fields.EtalonPath;
                if (!String.IsNullOrEmpty(appSettings.Fields.AppConfigPath))
                    view.AppConfigPathTextBox = appSettings.Fields.AppConfigPath;
                view.TestingMode = (appSettings.Fields.IsTestingMode) ? 0 : 1;
            }
        }
        //-------------------------------------------------------------------------
        private void SaveSettings()
        {
            var appSettings = new OcrTestWPFSettings();
            appSettings.Fields.WindowState = this.WindowState;
            appSettings.Fields.WindowLeft = this.Left;
            appSettings.Fields.WindowTop = this.Top;
            appSettings.Fields.WindowWidth = this.Width;
            appSettings.Fields.WindowHeight = this.Height;
            if (view.PathTextBox != MainWindowView.PathDef)
                appSettings.Fields.Path = view.PathTextBox;
            if (view.EtalonPathTextBox != MainWindowView.EtalonPathDef)
                appSettings.Fields.EtalonPath = view.EtalonPathTextBox;
            if (view.AppConfigPathTextBox != MainWindowView.AppConfigPathDef)
                appSettings.Fields.AppConfigPath = view.AppConfigPathTextBox;
            appSettings.Fields.IsTestingMode = view.IsTestingMode;
            appSettings.Save();
        }
        #endregion
        //-------------------------------------------------------------------------
        private void CopyDoesntMatchFiles_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            string[] hash = new string[0];
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                foreach (FileItem item in ResultListView.Items)
                {
                    if (item.ToString() != "Doesn't match")
                        continue;
                    if (Array.IndexOf(hash, item.UniqueIdentifier) >= 0)
                        continue;
                    Array.Resize(ref hash, hash.Length + 1);
                    hash[hash.Length - 1] = item.UniqueIdentifier;
                    var oldPath = Path.Combine(item.FilePath, item.ShowFileName + ".tiff");
                    var newPath = Path.Combine(dialog.SelectedPath, item.ShowFileName + ".tiff");
                    System.IO.File.Copy(oldPath, newPath, true);
                }
            }
        }
        //-------------------------------------------------------------------------
        //-------------------------------------------------------------------------
        //-------------------------------------------------------------------------
    }
}