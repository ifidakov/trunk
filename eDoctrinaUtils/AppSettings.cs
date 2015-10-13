using System.IO;
using System.Xml.Serialization;

namespace eDoctrinaUtils
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
            //TextWriter writer = new StreamWriter(Path.Combine(appSettings. XMLFileName);
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
}
