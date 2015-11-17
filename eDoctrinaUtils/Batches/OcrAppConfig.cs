using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace eDoctrinaUtils
{
    public class OcrAppConfig : OcrAppConfigDefaults
    {
        #region param, const
        public string appConfigDateTime;
        //public string frameFileName;
        public Exception exception;
        public decimal ThreadsAmount;
        public bool AutoStart = AutoStartDefault;
        public int LogLength = LogLengthDefault;
        public bool DualControl = DualControlDefault;
        public double PercentConfidentText = PercentConfidentTextDefault;
        public string FontName = FontNameDefault;
        public string ServerName = ServerNameDefault;
        public string UpdateServerName = UpdateServerNameDefault;
        public string OutputMode = "";
        public bool SmartResize = SmartResizeDefault;
        public bool DoNotProcess = DoNotProcessDefault;
        public bool AutoRunEditorOnError = AutoRunEditorOnErrorDefault;
        public Dictionary<int, string> OutputFileValue;
        public KeyValuePair<int, string> ManualProcessingFlag;
        public Dictionary<int, string> SheetIdEnum;
        public bool MoveToNextProccessingFolderOnSheetIdentifierError = MoveToNextProccessingFolderOnSheetIdentifierErrorDefault;
        public bool RemoveEmptyScans;
        public double EmptyScanDarknessPercent;

        public bool useStudentId = false;
        public bool recQRCode = false;
        public static bool showLog = true;

        public static string SupportedExtensions;
        public List<string> NotSupportedSheets;
        IOHelper iOHelper = new IOHelper();
        Log log = new Log();

        #endregion

        public OcrAppConfig()
            : this(OcrAppConfig.AppConfigFileName)
        { }

        public OcrAppConfig(string appConfigFileName)
        {
            if (!File.Exists(appConfigFileName))
            {
                CreateDefaultAppConfig();
                appConfigFileName = OcrAppConfig.AppConfigFileName;
                var Message = "Error in " + appConfigFileName + " This file does not exist. Default AppConfig was created.";
                log.LogMessage(Message);
            }

            this.appConfigDateTime = File.GetLastWriteTime(appConfigFileName).ToString();

            var serializer = new SerializerHelper();
            Batches currentAppConfig = serializer.GetBatchesFromFile(appConfigFileName, out exception);
            if (currentAppConfig == null)
            {
                log.LogMessage(exception);
                var Message = "Error in " + OcrAppConfig.AppConfigFileName + ". The program will be closed";
                log.LogMessage(Message);
                exception = new Exception("Error in " + OcrAppConfig.AppConfigFileName + "\r\n" + exception.Message + "\r\nThe program will be closed");
                return;
            }

            bool active = false;
            foreach (var item in currentAppConfig.batches)
            {
                if (!item.active)
                {
                    continue;
                }
                active = true;
                useStudentId = item.useStudentId;
                recQRCode = item.recQRCode;
                InputFolder = item.inPath;
                ManualInputFolder = item.manualInPath;
                ErrorFolder = item.outPathError;
                ManualErrorFolder = item.manualErrorsFolder;
                ConfigsFolder = item.configsFolder;
                ManualConfigsFolder = item.manuaConfigsFolder;
                ArchiveFolder = item.outPathArchive;
                ManualArchiveFolder = item.manualOutPathArchive;
                NotConfidentFolder = item.outPathNotConfident;
                ManualNotConfidentFolder = item.manualOutPathNotConfident;
                SuccessFolder = item.outPathSuccess;
                OutputFolder_Success = item.outputFolder_Success;
                ManualSuccessFolder = item.manualSuccessFolder;
                ManualNextProccessingFolder = item.manualNextProccessingFolder;
                DualControl = item.dual_control_bar_codes;
                AutoRunEditorOnError = item.autoRunEditorOnError;
                if (!string.IsNullOrEmpty(item.serverName))
                    ServerName = item.serverName;
                else
                    ServerName = Environment.MachineName;

                if (!string.IsNullOrEmpty(item.updateServerName))
                    UpdateServerName = item.updateServerName;

                if (!string.IsNullOrEmpty(item.outputMode))
                    OutputMode = item.outputMode;

                if (!string.IsNullOrEmpty(item.baseTempFolder))
                {
                    BaseTempFolder = item.baseTempFolder;
                    TempFolder = Path.Combine(BaseTempFolder, "Temp\\");
                    TempFramesFolder = Path.Combine(BaseTempFolder, "TempFrames\\");
                    TempEdFolder = Path.Combine(BaseTempFolder, "TempEd\\");
                    LogsFolder = Path.Combine(BaseTempFolder, "Logs\\");
                }

                SmartResize = (item.smartResize == true) ? true : false;
                DoNotProcess = (item.doNotProcess == true) ? true : false;

                RemoveEmptyScans = (item.removeEmptyScans == true) ? true : false;

                PercentConfidentText = (item.percent_confident_text != null) ? (double)item.percent_confident_text : PercentConfidentTextDefault;
                FontName = (String.IsNullOrEmpty(item.fontName)) ? FontNameDefault : item.fontName;
                SupportedExtensions = (String.IsNullOrEmpty(item.fileMask)) ? SupportedExtensionsDefault : item.fileMask;
                MoveToNextProccessingFolderOnSheetIdentifierError = (item.moveToNextProccessingFolderOnSheetIdentifierError == true) ? true : false;

                EmptyScanDarknessPercent = (item.emptyScanDarknessPercent != null) ? (double)item.emptyScanDarknessPercent : EmptyScanDarknessPercentDefault;
                EmptyScansFolder = (String.IsNullOrEmpty(item.emptyScansPath)) ? ErrorFolder : item.emptyScansPath;

                NotSupportedSheetFolder = (String.IsNullOrEmpty(item.notSupportedSheetPath)) ? ErrorFolder : item.notSupportedSheetPath;
                NotSupportedSheets = (!String.IsNullOrEmpty(item.notSupportedSheets))
                    ? Regex.Split(item.notSupportedSheets, "\\s+").ToList() : (!String.IsNullOrEmpty(NotSupportedSheetsDefault))
                        ? Regex.Split(NotSupportedSheetsDefault, "\\s+").ToList() : new List<string>();

                OutputFileValue = new Dictionary<int, string>();
                foreach (Batches.OutputFileValue itm in item.outputFileValues)
                {
                    OutputFileValue.Add(itm.position, itm.value.ToString());
                }
                SheetIdEnum = new Dictionary<int, string>();
                if (item.sheetIdEnum != null)
                {
                    foreach (Batches.SheetIdEnum itm in item.sheetIdEnum)
                    {
                        SheetIdEnum.Add(itm.position, itm.value);
                    }
                }
                //ManualProcessingFlag = new KeyValuePair<int, string>
                //    (item.manualProcessingFlag.position, item.manualProcessingFlag.value as string);

                if (configsFolderDefault != ConfigsFolder)
                    CopyConfigsFile(ConfigsFolder);
                if (configsFolderDefault != ManualConfigsFolder)
                    CopyConfigsFile(ManualConfigsFolder);
                break;
            }
            if (!active)
            {
                exception = new Exception("Active batches not found. The program will be closed.");
                log.LogMessage(exception);
                return;
            }

            this.AutoStart = currentAppConfig.autoStart;
            this.LogLength = currentAppConfig.logLength;
        }

        private void CopyConfigsFile(string dirPath)
        {
            if (iOHelper.CreateDirectory(dirPath, false))
            {
                if (Directory.Exists(configsFolderDefault))
                {
                    foreach (var item in Directory.GetFiles(configsFolderDefault))
                    {
                        var newPath = Path.Combine(dirPath, Path.GetFileName(item));
                        if (!File.Exists(newPath))
                        {
                            try
                            {
                                File.Copy(item, newPath, true);
                            }
                            catch { }
                        }
                    }
                }
            }
        }

        public bool IsChangeAppConfig()
        {
            var dt = File.GetLastWriteTime(OcrAppConfig.AppConfigFileName).ToString();
            return this.appConfigDateTime != dt;//DateTime.Now.ToString();//
        }

        public Batches CreateDefaultAppConfig()
        {
            Batches batches = new Batches();
            batches.autoStart = AutoStartDefault;
            batches.logLength = LogLengthDefault;
            batches.email = new Batches.Email()
            {
                host = "//smtp.gmail.com",
                port = 587,
                from = "eDoctrinaOcr@gmail.com",
                password = "6eDoctrinaOcr6",
                emails = @"mkukharuk@itera-research.com
                        //slavryk@itera-research.com
                        //kvinokurova@itera-research.com",
                caption = "Bad answers sheet",
                message = "Test"
            };
            Batches.Batch[] b = new Batches.Batch[1];
            b[0] = new Batches.Batch
            {
                inPath = InputFolder,
                configsFolder = ConfigsFolder,
                outPathSuccess = SuccessFolder,
                outPathError = ErrorFolder,
                outPathNotConfident = NotConfidentFolder,
                outPathArchive = ArchiveFolder,
                manualInPath = ManualInputFolder,
                manualErrorsFolder = ManualErrorFolder,
                manualSuccessFolder = ManualSuccessFolder,
                manuaConfigsFolder = ManualConfigsFolder,
                manualOutPathArchive = ManualArchiveFolder,
                manualOutPathNotConfident = ManualNotConfidentFolder,
                manualNextProccessingFolder = ManualNextProccessingFolder,
                notSupportedSheetPath = NotSupportedSheetFolder,
                emptyScansPath = EmptyScansFolder,

                active = ActiveDefault,
                autoRunEditorOnError = AutoRunEditorOnErrorDefault,
                smartResize = SmartResizeDefault,
                dual_control_bar_codes = DualControlDefault,
                fileMask = SupportedExtensionsDefault,
                percent_confident_text = PercentConfidentTextDefault,
                fontName = FontNameDefault,

                notSupportedSheets = NotSupportedSheetsDefault,
                removeEmptyScans = RemoveEmptyScansDefault,
                emptyScanDarknessPercent = EmptyScanDarknessPercentDefault,
            };
            b[0].outputFileValues = new Batches.OutputFileValue[2];
            b[0].outputFileValues[0].position = 7;
            b[0].outputFileValues[0].value = "EDOCOCR";
            b[0].outputFileValues[1].position = 8;
            b[0].outputFileValues[1].value = 1;

            batches.batches = b;
            var serializer = new SerializerHelper();
            serializer.SaveToFile(batches, OcrAppConfig.AppConfigFileName, Encoding.Unicode);
            return batches;
        }
    }
}
