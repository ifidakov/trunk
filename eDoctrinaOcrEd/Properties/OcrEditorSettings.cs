using System.Windows.Forms;
using System.Drawing;
using eDoctrinaUtils;

namespace eDoctrinaOcrEd
{
    public class OcrEditorSettings : AppSettings
    {
        public OcrEditorSettings()
        {
            XMLFileName = System.IO.Path.Combine(OcrAppConfigDefaults.BaseTempFolder, "OcrEditorSettings.xml");
        }

        public void Save()
        {
            WriteXml(Fields);
        }

        public void Load()
        {
            Fields = ReadXml(Fields) as OcrEditorSettingsFields;
        }

        public OcrEditorSettingsFields Fields = new OcrEditorSettingsFields();

        public class OcrEditorSettingsFields
        {
            private FormWindowState windowState;
            public FormWindowState WindowState
            {
                get { return windowState; }
                set { windowState = value; }
            }

            private Point location;
            public Point WindowLocation
            {
                get { return location; }
                set { location = value; }
            }

            private Size size;
            public Size WindowSize
            {
                get { return size; }
                set { size = value; }
            }

            private bool chbBuildAllAreas;
            public bool ChbBuildAllAreas
            {
                get { return chbBuildAllAreas; }
                set { chbBuildAllAreas = value; }
            }

            private Point linsLocation;
            public Point LinsWindowLocation
            {
                get { return linsLocation; }
                set { linsLocation = value; }
            }

            private Size linsSize;
            public Size LinsWindowSize
            {
                get { return linsSize; }
                set { linsSize = value; }
            }

            private bool usePrevTool;
            public bool UsePrevTool
            {
                get { return usePrevTool; }
                set { usePrevTool = value; }
            }

            private int linsTrackBar1Value;
            public int LinsTrackBar1Value
            {
                get { return linsTrackBar1Value; }
                set { linsTrackBar1Value = value; }
            }

            //trackBar1

            private int splitterDistanceActions;
            public int SplitterDistanceActions
            {
                get { return splitterDistanceActions; }
                set { splitterDistanceActions = value; }
            }

            private int splitterDistanceBubble;
            public int SplitterDistanceBubble
            {
                get { return splitterDistanceBubble; }
                set { splitterDistanceBubble = value; }
            }

            private int splitterDistanceMiniature;
            public int SplitterDistanceMiniature
            {
                get { return splitterDistanceMiniature; }
                set { splitterDistanceMiniature = value; }
            }

            private int splitterDistanceLens;
            public int SplitterDistanceLens
            {
                get { return splitterDistanceLens; }
                set { splitterDistanceLens = value; }
            }

            private bool darknessManualySet;
            public bool DarknessManualySet
            {
                get { return darknessManualySet; }
                set { darknessManualySet = value; }
            }

            private bool notConfirm;
            public bool NotConfirm
            {
                get { return notConfirm; }
                set { notConfirm = value; }
            }

            private bool recAfterCut;
            public bool RecAfterCut
            {
                get { return recAfterCut; }
                set { recAfterCut = value; }
            }

            private decimal nudPerCentBestBubble;
            public decimal NudPerCentBestBubble
            {
                get { return nudPerCentBestBubble; }
                set { nudPerCentBestBubble = value; }
            }

            private decimal nudPerCentEmptyBubble;
            public decimal NudPerCentEmptyBubble
            {
                get { return nudPerCentEmptyBubble; }
                set { nudPerCentEmptyBubble = value; }
            }

            private string reportTestPageEmail;
            public string ReportTestPageEmail
            {
                get { return reportTestPageEmail; }
                set { reportTestPageEmail = value; }
            }
            
            private string sendToSupportEmail;
            public string SendToSupportEmail
            {
                get { return sendToSupportEmail; }
                set { sendToSupportEmail = value; }
            }

            private bool chbSheetId;
            public bool ChbSheetId
            {
                get { return chbSheetId; }
                set { chbSheetId = value; }
            }

            private bool districtId;
            public bool DistrictId
            {
                get { return districtId; }
                set { districtId = value; }
            }

            private bool testId;
            public bool TestId
            {
                get { return testId; }
                set { testId = value; }
            }

            private bool amoutOfQuestions;
            public bool AmoutOfQuestions
            {
                get { return amoutOfQuestions; }
                set { amoutOfQuestions = value; }
            }

            private bool indexOfFirstQuestion;
            public bool IndexOfFirstQuestion
            {
                get { return indexOfFirstQuestion; }
                set { indexOfFirstQuestion = value; }
            }//nudZoomValue

            private decimal nudZoomValue;
            public decimal NudZoomValue
            {
                get { return nudZoomValue; }
                set { nudZoomValue = value; }
            }
        }
    }
}
