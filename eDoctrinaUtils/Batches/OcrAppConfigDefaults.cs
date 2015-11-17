using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eDoctrinaUtils
{
    public class OcrAppConfigDefaults : OcrAppConfigFolders
    {
        public const string ServerNameDefault = "";
        public const string UpdateServerNameDefault = "";
        public const string AppConfigFileName = "appConfig.json";

        protected const bool AutoStartDefault = true;
        protected const int LogLengthDefault = 100;

        protected const bool ActiveDefault = true;
        protected const bool AutoRunEditorOnErrorDefault = false;
        protected const string SupportedExtensionsDefault = "*.tif,*.tiff,*.pdf,*.jpg,*.jpeg,*.png";
        protected const string FontNameDefault = "Arial";
        public static string BaseTempFolder = "";
        protected const double PercentConfidentTextDefault = 30;
        protected const bool DualControlDefault = true;
        protected const bool SmartResizeDefault = true;
        protected const bool DoNotProcessDefault = false;
        protected const bool MoveToNextProccessingFolderOnSheetIdentifierErrorDefault = false;

        protected const bool RemoveEmptyScansDefault = false;
        protected const double EmptyScanDarknessPercentDefault = 2;
        protected const string NotSupportedSheetsDefault = "FANDP FLEXRUBRIC";
    }
}
