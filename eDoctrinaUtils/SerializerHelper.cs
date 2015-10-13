using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace eDoctrinaUtils
{
    public class SerializerHelper
    {
        public string Serialise(object obj)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            string s = "";// js.Serialize(obj);
            try
            {
                s = js.Serialize(obj);
            }
            catch (Exception)
            {
            }
            s = Regex.Replace(s, ",\"\\w+\":0|\"\\w+\":0,", "");
            s = Regex.Replace(s, ",\"\\w+\":null|\"\\w+\":null,", "");
            s = s.Replace(",\"", Environment.NewLine + ",\"");
            s = s.Replace("[{", "[" + Environment.NewLine + "{");
            s = s.Replace(":{", ":" + Environment.NewLine + "{");
            s = s.Replace("},{", "}" + Environment.NewLine + ",{");
            //s = s.Replace("}", Environment.NewLine + "}");
            s = s.Replace("]", Environment.NewLine + "]");
            return s;
        }
        //-------------------------------------------------------------------------
        public void SaveToFile(object obj, string fileName, Encoding encoding)
        {
            string str = Serialise(obj);//??? Процесс не может получить доступ к файлу "C:\eDoctrina\NY\Temp\TempEd\CAE15FFB5B97D2EDF426BCCB76BEAA76FEC6B11E.audit"
            using (StreamWriter sw = new StreamWriter(fileName, false, encoding))
            {
                sw.Write(str);
            }
        }
        //-------------------------------------------------------------------------
        private string GetStringByFile(string fileName)
        {
            string s = "";
            try
            {
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                s = sr.ReadToEnd();
                sr.Close();
                fs.Close();
            }
            catch (Exception)
            { }
            return s;
        }
        //-------------------------------------------------------------------------
        public Batches GetBatchesFromFile(string fileName, out Exception exception)
        {
            exception = null;
            string settingsFile = GetStringByFile(fileName);
            JavaScriptSerializer js = new JavaScriptSerializer();
            try
            {
                return js.Deserialize<Batches>(settingsFile);
            }
            catch (Exception ex)
            {
                exception = ex;
                return null;
            }
        }
        //-------------------------------------------------------------------------
        public Audit GetAuditFromFile(string fileName, out Exception exception)
        {
            exception = null;
            string stringFile = GetStringByFile(fileName);
            JavaScriptSerializer js = new JavaScriptSerializer();
            try
            {
                return js.Deserialize<Audit>(stringFile);
            }
            catch (Exception ex)
            {
                exception = ex;
                return null;
            }
        }
        //-------------------------------------------------------------------------
        public Regions GetRegionsFromFile(string fileName, out Exception exception)
        {
            exception = null;
            string stringFile = GetStringByFile(fileName);
            JavaScriptSerializer js = new JavaScriptSerializer();
            try
            {
                return js.Deserialize<Regions>(stringFile);
            }
            catch (Exception ex)
            {
                exception = ex;
                return null;
            }
        }
    }

}
