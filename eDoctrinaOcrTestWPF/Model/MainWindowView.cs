using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace eDoctrinaOcrTestWPF
{
    public class MainWindowView : NotifyPropertyHelper
    {
        public bool IsTestingMode { get; private set; }
        public int TestingMode
        {
            get { return (IsTestingMode) ? 0 : 1; }
            set
            {
                var temp = (value == 0);
                if (IsTestingMode != temp)
                {
                    IsTestingMode = temp;
                    ErrorText = "";
                    CVSSource = null;
                    HasDuplicateOptions = false;
                    HasExtraFiles = false;
                    NotifyPropertyChanged("TestingMode");
                    NotifyPropertyChanged("TestingModeVisibility");
                    NotifyPropertyChanged("ElseModeVisibility");
                    NotifyPropertyChanged("ViewResultListView");
                }
            }
        }

        public System.Windows.Controls.GridView ViewEtaloneMode;
        public System.Windows.Controls.GridView ViewTestingMode;
        public System.Windows.Controls.GridView ViewResultListView
        {
            get { return (IsTestingMode) ? ViewTestingMode : ViewEtaloneMode; }
        }

        private string errorText;
        public string ErrorText
        {
            get { return errorText; }
            set
            {
                if (errorText != value)
                {
                    errorText = value;
                    if (errorText == "Waiting...")
                    {
                        FilesCount = "0";
                        EtalonFilesCount = "0";
                        CVSSource = null;
                        HasDuplicateOptions = false;
                        HasExtraFiles = false;
                    }
                    NotifyPropertyChanged("ErrorText");
                    NotifyPropertyChanged("ErrorLabelVisibility");
                }
            }
        }

        private object cvsSource;
        public object CVSSource
        {
            get { return cvsSource; }
            set
            {
                if (cvsSource != value)
                {
                    cvsSource = value;
                    NotifyPropertyChanged("CVSSource");
                }
            }
        }

        public Visibility ErrorLabelVisibility//???
        {
            get { return GetVisibility(!String.IsNullOrEmpty(ErrorText)); }
        }

        public Visibility TestingModeVisibility
        {
            get { return GetVisibility(IsTestingMode); }
        }
        public Visibility ElseModeVisibility
        {
            get { return GetVisibility(!IsTestingMode); }
        }

        private bool hasExtraFiles;
        public bool HasExtraFiles
        {
            get { return hasExtraFiles; }
            set
            {
                if (hasExtraFiles != value)
                {
                    hasExtraFiles = value;
                    NotifyPropertyChanged("HasExtraFiles");
                    NotifyPropertyChanged("AddToEtalonButtonVisibility");
                }
            }
        }
        public Visibility AddToEtalonButtonVisibility
        {
            get { return GetVisibility(hasExtraFiles); }
        }

        private Visibility GetVisibility(bool value)
        {
            return (value) ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool hasDuplicateOptions;
        public bool HasDuplicateOptions
        {
            get { return hasDuplicateOptions; }
            set
            {
                if (hasDuplicateOptions != value)
                {
                    hasDuplicateOptions = value;
                    NotifyPropertyChanged("HasDuplicateOptions");
                    NotifyPropertyChanged("DuplicateOptionsVisibility");
                }
            }
        }
        public Visibility DuplicateOptionsVisibility
        {
            get { return GetVisibility(HasDuplicateOptions); }
        }
        
        private bool showAllFiles;
        public bool ShowAllFiles
        {
            get { return showAllFiles; }
            set
            {
                if (showAllFiles != value)
                {
                    showAllFiles = value;
                    NotifyPropertyChanged("ShowAllFiles");
                }
            }
        }

        private bool showEffect;
        public bool ShowEffect
        {
            get { return showEffect; }
            set
            {
                if (showEffect != value)
                {
                    showEffect = value;
                    NotifyPropertyChanged("ShowEffect");
                    NotifyPropertyChanged("CurrentEffect");
                }
            }
        }
        public System.Windows.Media.Effects.BlurEffect CurrentEffect
        {
            get
            {
                System.Windows.Media.Effects.BlurEffect objBlur = new System.Windows.Media.Effects.BlurEffect();
                objBlur.Radius = 4;
                return (ShowEffect) ? objBlur : null;
            }
        }

        private string filesCount = "0";
        public string FilesCount
        {
            get { return filesCount; }
            set
            {
                if (filesCount != value)
                {
                    filesCount = value;
                    NotifyPropertyChanged("FilesCount");
                }
            }
        }

        private string etalonFilesCount = "0";
        public string EtalonFilesCount
        {
            get { return etalonFilesCount; }
            set
            {
                if (etalonFilesCount != value)
                {
                    etalonFilesCount = value;
                    NotifyPropertyChanged("EtalonFilesCount");
                }
            }
        }

        public const string PathDef = @"enter path...";
        private string pathTextBox = PathDef;
        public string PathTextBox
        {
            get { return pathTextBox; }
            set
            {
                if (pathTextBox != value)
                {
                    pathTextBox = value;
                    NotifyPropertyChanged("PathTextBox");
                    NotifyPropertyChanged("PathTextBoxForeground");
                }
            }
        }
        public SolidColorBrush PathTextBoxForeground
        {
            get 
            {
                return (System.IO.Directory.Exists(PathTextBox)) ? Brushes.Black : Brushes.Red;
            }
        }

        public const string EtalonPathDef = @"choose etalon data file path or change 'Testing Mode'...";
        private string etalonPathTextBox = EtalonPathDef;
        public string EtalonPathTextBox
        {
            get { return etalonPathTextBox; }
            set
            {
                if (etalonPathTextBox != value)
                {
                    etalonPathTextBox = (value == "") ? EtalonPathDef : value;
                    NotifyPropertyChanged("EtalonPathTextBox");
                    NotifyPropertyChanged("EtalonPathTextBoxForeground");
                }
            }
        }
        public SolidColorBrush EtalonPathTextBoxForeground
        {
            get 
            {
                return (System.IO.File.Exists(EtalonPathTextBox)) ? Brushes.Black : Brushes.Red;
            }
        }

        public const string AppConfigPathDef = @"choose app config file...";
        private string appConfigPathTextBox = AppConfigPathDef;
        public string AppConfigPathTextBox
        {
            get { return appConfigPathTextBox; }
            set
            {
                if (appConfigPathTextBox != value)
                {
                    appConfigPathTextBox = (value == "") ? AppConfigPathDef : value;
                    NotifyPropertyChanged("AppConfigPathTextBox");
                    NotifyPropertyChanged("AppConfigPathTextBoxForeground");
                }
            }
        }
        public SolidColorBrush AppConfigPathTextBoxForeground
        {
            get
            {
                return (System.IO.File.Exists(AppConfigPathTextBox)) ? Brushes.Black : Brushes.Red;
            }
        }
    }
}
