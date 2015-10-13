using System;
using System.Linq;

namespace eDoctrinaUtils
{
    public class OcrAppConfigFolders
    {
        private string CorrectPath(string path)
        {
            if (path.Contains('/'))
                path = path.Replace('/', '\\');
            if (path.Last() != '\\')
                path += '\\';
            return path;
        }

        public static string TempFolder = "Temp\\";
        public static string TempFramesFolder = "TempFrames\\";
        public static string TempEdFolder = "TempEd\\";
        public static string LogsFolder = "Logs\\";
        protected string configsFolderDefault = "Configs\\";
        private string configsFolder;
        public string ConfigsFolder
        {
            get
            {
                if (String.IsNullOrEmpty(configsFolder))
                    configsFolder = configsFolderDefault;
                return configsFolder;
            }
            set
            {
                configsFolder = (String.IsNullOrEmpty(value)) ? configsFolderDefault : CorrectPath(value);
            }
        }
        private string manualConfigsFolder;
        public string ManualConfigsFolder
        {
            get
            {
                if (String.IsNullOrEmpty(manualConfigsFolder))
                    manualConfigsFolder = ConfigsFolder;
                return manualConfigsFolder;
            }
            set
            {
                manualConfigsFolder = (String.IsNullOrEmpty(value)) ? ConfigsFolder : CorrectPath(value);
            }
        }

        private string inputFolderDefault = "Input\\Queue\\";
        private string inputFolder;
        public string InputFolder
        {
            get
            {
                if (String.IsNullOrEmpty(inputFolder))
                    inputFolder = inputFolderDefault;
                return inputFolder;
            }
            set
            {
                inputFolder = (String.IsNullOrEmpty(value)) ? inputFolderDefault : CorrectPath(value);
            }
        }

        private string archiveFolderDefault = "Input\\Archive\\";
        private string archiveFolder;
        public string ArchiveFolder
        {
            get
            {
                if (String.IsNullOrEmpty(archiveFolder))
                    archiveFolder = archiveFolderDefault;
                return archiveFolder;
            }
            set
            {
                archiveFolder = (String.IsNullOrEmpty(value)) ? archiveFolderDefault : CorrectPath(value);
            }
        }
        private string manualArchiveFolder;
        public string ManualArchiveFolder
        {
            get
            {
                if (String.IsNullOrEmpty(manualArchiveFolder))
                    manualArchiveFolder = ArchiveFolder;
                return manualArchiveFolder;
            }
            set
            {
                manualArchiveFolder = (String.IsNullOrEmpty(value)) ? ArchiveFolder : CorrectPath(value);
            }
        }

        private string errorFolderDefault = "Results\\Errors\\";
        private string errorFolder;
        public string ErrorFolder
        {
            get
            {
                if (String.IsNullOrEmpty(errorFolder))
                    errorFolder = errorFolderDefault;
                return errorFolder;
            }
            set
            {
                errorFolder = (String.IsNullOrEmpty(value)) ? errorFolderDefault : CorrectPath(value);
            }
        }
        private string manualInputFolder;
        public string ManualInputFolder
        {
            get
            {
                //if (String.IsNullOrEmpty(manualInputFolder))
                //    manualInputFolder = ErrorFolder;
                return manualInputFolder;
            }
            set
            {
                //manualInputFolder = (String.IsNullOrEmpty(value)) ? ErrorFolder : CorrectPath(value);
                manualInputFolder = value;
            }
        }
        private string manualErrorFolder;
        public string ManualErrorFolder
        {
            get
            {
                if (String.IsNullOrEmpty(manualErrorFolder))
                    manualErrorFolder = ErrorFolder;
                return manualErrorFolder;
            }
            set
            {
                manualErrorFolder = (String.IsNullOrEmpty(value)) ? ErrorFolder : CorrectPath(value);
            }
        }
        //-------------------------------------------------------------------------
        private string successFolderDefault = "Results\\Success\\";
        //-------------------------------------------------------------------------
        private string successFolder;
        public string SuccessFolder
        {
            get
            {
                if (String.IsNullOrEmpty(successFolder))
                    successFolder = successFolderDefault;
                return successFolder;
            }
            set
            {
                successFolder = (String.IsNullOrEmpty(value)) ? successFolderDefault : CorrectPath(value);
            }
        }
        //-------------------------------------------------------------------------
        private string outputFolder_Success;
        public string OutputFolder_Success
        {
            get
            {
                if (String.IsNullOrEmpty(outputFolder_Success))
                    outputFolder_Success = successFolderDefault;
                return outputFolder_Success;
            }
            set
            {
                outputFolder_Success = (String.IsNullOrEmpty(value)) ? successFolderDefault : CorrectPath(value);
            }
        }
        //-------------------------------------------------------------------------
        private string manualSuccessFolder;
        public string ManualSuccessFolder
        {
            get
            {
                if (String.IsNullOrEmpty(manualSuccessFolder))
                    manualSuccessFolder = SuccessFolder;
                return manualSuccessFolder;
            }
            set
            {
                manualSuccessFolder = (String.IsNullOrEmpty(value)) ? SuccessFolder : CorrectPath(value);
            }
        }

        private string notConfidentFolderDefault = "Results\\Not Сonfident\\";
        private string notConfidentFolder;
        public string NotConfidentFolder
        {
            get
            {
                if (String.IsNullOrEmpty(notConfidentFolder))
                    notConfidentFolder = notConfidentFolderDefault;
                return notConfidentFolder;
            }
            set
            {
                notConfidentFolder = (String.IsNullOrEmpty(value)) ? notConfidentFolderDefault : CorrectPath(value);
            }
        }
        private string manualNotConfidentFolder;
        public string ManualNotConfidentFolder
        {
            get
            {
                if (String.IsNullOrEmpty(manualNotConfidentFolder))
                    manualNotConfidentFolder = NotConfidentFolder;
                return manualNotConfidentFolder;
            }
            set
            {
                manualNotConfidentFolder = (String.IsNullOrEmpty(value)) ? NotConfidentFolder : CorrectPath(value);
            }
        }

        private string manualNextProccessingFolderDefault = "Results\\Next Proccessing\\";
        private string manualNextProccessingFolder;
        public string ManualNextProccessingFolder
        {
            get
            {
                if (String.IsNullOrEmpty(manualNextProccessingFolder))
                    manualNextProccessingFolder = manualNextProccessingFolderDefault;
                return manualNextProccessingFolder;
            }
            set
            {
                manualNextProccessingFolder = (String.IsNullOrEmpty(value)) ? manualNextProccessingFolderDefault : CorrectPath(value);
            }
        }

        private string notSupportedSheetFolderDefault = "Results\\Unsupported Answer Sheet\\";
        private string notSupportedSheetFolder;
        public string NotSupportedSheetFolder
        {
            get
            {
                if (String.IsNullOrEmpty(notSupportedSheetFolder))
                    notSupportedSheetFolder = notSupportedSheetFolderDefault;
                return notSupportedSheetFolder;
            }
            set
            {
                notSupportedSheetFolder = (String.IsNullOrEmpty(value)) ? notSupportedSheetFolderDefault : CorrectPath(value);
            }
        }

        private string emptyScansFolderDefault = "Results\\Empty Scans\\";
        private string emptyScansFolder;
        public string EmptyScansFolder
        {
            get
            {
                if (String.IsNullOrEmpty(emptyScansFolder))
                    emptyScansFolder = emptyScansFolderDefault;
                return emptyScansFolder;
            }
            set
            {
                emptyScansFolder = (String.IsNullOrEmpty(value)) ? emptyScansFolderDefault : CorrectPath(value);
            }
        }
    }
}
