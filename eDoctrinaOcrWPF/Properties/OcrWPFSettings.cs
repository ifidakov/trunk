using eDoctrinaUtils;
using System.Windows;

namespace eDoctrinaOcrWPF
{
    public class OcrWPFSettings : AppSettings
    {
        public OcrWPFSettings()
        {
            XMLFileName = System.IO.Path.Combine(OcrAppConfigDefaults.BaseTempFolder, "OcrWPFSettings.xml");
        }

        public void Save()
        {
            WriteXml(Fields);
        }

        public void Load()
        {
            Fields = ReadXml(Fields) as OcrWPFSettingsFields;
        }

        public OcrWPFSettingsFields Fields = new OcrWPFSettingsFields();

        public class OcrWPFSettingsFields
        {
            private WindowState windowState;
            public WindowState WindowState
            {
                get { return windowState; }
                set { windowState = value; }
            }

            private double windowLeft;
            public double WindowLeft
            {
                get { return windowLeft; }
                set
                { windowLeft = value; }
            }

            private double windowTop;
            public double WindowTop
            {
                get { return windowTop; }
                set
                { windowTop = value; }
            }

            private double windowWidth;
            public double WindowWidth
            {
                get { return windowWidth; }
                set
                { windowWidth = value; }
            }

            private double windowHeight;
            public double WindowHeight
            {
                get { return windowHeight; }
                set
                { windowHeight = value; }
            }
            private decimal threadsCount = System.Environment.ProcessorCount;
            public decimal ThreadsCount
            {
                get { return threadsCount; }
                set
                { threadsCount = value; }
            }
        }
    }
}
