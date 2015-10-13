using System.IO;
using System.Xml.Serialization;
using System.Windows;

namespace eDoctrinaOcrTestWPF
{
    public class AppSettings
    {
        public AppSettings()
        { }

        protected string XMLFileName = "settings.xml";

        [XmlIgnore]
        public bool SettingsExists;

        public void WriteXml(object fields)
        {
            XmlSerializer ser = new XmlSerializer(fields.GetType());
            TextWriter writer = new StreamWriter(XMLFileName);
            ser.Serialize(writer, fields);
            writer.Close();
        }

        public object ReadXml(object fields)
        {
            SettingsExists = File.Exists(XMLFileName);
            if (SettingsExists)
            {
                XmlSerializer ser = new XmlSerializer(fields.GetType());
                TextReader reader = new StreamReader(XMLFileName);
                fields = ser.Deserialize(reader);
                reader.Close();
            }
            return fields;
        }
    }

    public class OcrTestWPFSettings : AppSettings
    {
        public OcrTestWPFSettings()
        {
            XMLFileName = "OcrTestWPFSettings.xml";
        }

        public void Save()
        {
            WriteXml(Fields);
        }

        public void Load()
        {
            Fields = ReadXml(Fields) as OcrTestWPFSettingsFields;
        }

        public OcrTestWPFSettingsFields Fields = new OcrTestWPFSettingsFields();

        public class OcrTestWPFSettingsFields
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
                set { windowLeft = value; }
            }

            private double windowTop;
            public double WindowTop
            {
                get { return windowTop; }
                set { windowTop = value; }
            }
            
            private double windowWidth;
            public double WindowWidth
            {
                get { return windowWidth; }
                set { windowWidth = value; }
            }
       
            private double windowHeight;
            public double WindowHeight
            {
                get { return windowHeight; }
                set { windowHeight = value; }
            }

            private string path;
            public string Path
            {
                get { return path; }
                set { path = value; }
            }

            private string etalonPath;
            public string EtalonPath
            {
                get { return etalonPath; }
                set { etalonPath = value; }
            }

            private string appConfigPath;
            public string AppConfigPath
            {
                get { return appConfigPath; }
                set { appConfigPath = value; }
            }

            private bool isTestingMode;
            public bool IsTestingMode
            {
                get { return isTestingMode; }
                set { isTestingMode = value; }
            }
        }
    }
}
