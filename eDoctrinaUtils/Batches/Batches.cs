using System;

namespace eDoctrinaUtils
{
    [Serializable]
    public class Batches
    {
        public bool autoStart { get; set; }
        public int threadsAmount { get; set; }
        public int logLength { get; set; }
        public Batch[] batches { get; set; }
        public Email email { get; set; }

        public struct Batch
        {
            public bool active { get; set; }
            public bool recStudentUid { get; set; }
            public bool recQRCode { get; set; }
            public bool autoRunEditorOnError { get; set; }
            public string serverName { get; set; }
            public string outputMode { get; set; }
            public string baseTempFolder { get; set; }
            public string fileMask { get; set; }
            public string inPath { get; set; }
            public string testPath { get; set; }
            public string configsFolder { get; set; }
            public string outPathSuccess { get; set; }
            public string outputFolder_Success { get; set; }
            public string outPathError { get; set; }
            public string outPathNotConfident { get; set; }
            public string outPathArchive { get; set; }
            public string manualInPath { get; set; }
            public string manualErrorsFolder { get; set; }
            public string manualSuccessFolder { get; set; }
            public string manualNextProccessingFolder { get; set; }
            public string manualOutPathNotConfident { get; set; }
            public string manuaConfigsFolder { get; set; }
            public string manualOutPathArchive { get; set; }
            public string notSupportedSheetPath { get; set; }
            public string emptyScansPath { get; set; }

            public string fontName { get; set; }
            public double? percent_confident_text { get; set; }
            public bool dual_control_bar_codes { get; set; }
            public bool? smartResize { get; set; }
            public bool? doNotProcess { get; set; }
            public bool useStudentId { get; set; }
            public OutputFileValue[] outputFileValues { get; set; }
            //public OutputFileValue manualProcessingFlag { get; set; }
            public bool? moveToNextProccessingFolderOnSheetIdentifierError { get; set; }

            public string notSupportedSheets { get; set; }
            public bool? removeEmptyScans { get; set; }
            public double? emptyScanDarknessPercent { get; set; }
            public SheetIdEnum[] sheetIdEnum { get; set; }
        }
        public struct SheetIdEnum
        {
            public int position { get; set; }
            public string value { get; set; }
        }

        public struct OutputFileValue
        {
            public int position { get; set; }
            public object value { get; set; }
        }

        public class Email
        {
            public string host { get; set; }
            public int port { get; set; }
            public string from { get; set; }
            public string emails { get; set; }
            public string password { get; set; }
            public string caption { get; set; }
            public string message { get; set; }
        }
    }
}
