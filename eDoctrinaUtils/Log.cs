using System;
using System.IO;

namespace eDoctrinaUtils
{
    public class Log
    {
        private static object locker = new object();
        //------------------------------------------------------------------------------------------
        private string mLogFile = Path.Combine(OcrAppConfig.LogsFolder + "Application_%Y%M%D.log");
        //------------------------------------------------------------------------------------------
        public Log()
        { }
        //------------------------------------------------------------------------------------------
        void SetLogFile(string LogFile)
        {
            mLogFile = LogFile;
        }
        //------------------------------------------------------------------------------------------
        public string GetLogFileName()
        {
            if (!Directory.Exists(OcrAppConfig.LogsFolder))
            {
                Directory.CreateDirectory(OcrAppConfig.LogsFolder);
            }
            mLogFile = Path.Combine(OcrAppConfig.LogsFolder + "Application_%Y%M%D.log"); 
            var application = Environment.StackTrace;
            if (application.Contains("eDoctrinaOcrEd."))
            { application = "eDoctrinaOcrEd"; }
            else
                if (application.Contains("eDoctrinaOcrTestWPF."))
                { application = "eDoctrinaOcrTestWPF"; }
                else
                    if (application.Contains("eDoctrinaOcrWPF."))
                    { application = "eDoctrinaOcrWPF"; }
                    else
                    { application = ""; }
            string FN = mLogFile;
            FN = FN.Replace("Application", application);
            FN = FN.Replace("%Y", DateTime.Now.Year.ToString().PadLeft(4, '0'));
            FN = FN.Replace("%M", DateTime.Now.Month.ToString().PadLeft(2, '0'));
            FN = FN.Replace("%D", DateTime.Now.Day.ToString().PadLeft(2, '0'));
            return FN;
        }
        //------------------------------------------------------------------------------------------
        public void LogMessage(string Message)
        {
            if (!OcrAppConfig.showLog)
                return;
            LogMessage(GetLogFileName(), Message);
        }
        //------------------------------------------------------------------------------------------
        public void LogMessage(string FileName, string Message)
        {
            if (!OcrAppConfig.showLog)
                return;
            Message = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " : " + Message;
            if ((FileName != null) && (FileName != ""))
            {
                try
                {
                    lock (locker)
                        using (StreamWriter w = File.AppendText(FileName))
                        {
                            w.WriteLine(Message);
                        }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            NotifyUpdated(Message);
        }
        //------------------------------------------------------------------------------------------
        public void LogMessage(Exception E)
        {
            LogMessage(null, E);
        }
        //------------------------------------------------------------------------------------------
        public void LogMessage(string Message, Exception E)
        {
            if (String.IsNullOrEmpty(Message))
                Message = "";
            if (E != null)
            {
                LogMessage(Message + GetExceptionMessage(E, false));
                LogMessage("___" + Message + GetExceptionMessage(E, true));
            }
            else
            {
                LogMessage(Message + "Exception: Unknown.");
            }
        }
        //------------------------------------------------------------------------------------------
        public string GetExceptionMessage(Exception E, bool WithStack)
        {
            string Message = E.Message;
            string Stack = E.StackTrace;
            if (Stack == null) Stack = "";
            while (E.InnerException != null)
            {
                E = E.InnerException;
                Message += "\n" + E.Message;
                Stack += "\n\n" + E.StackTrace;
            }
            return "Exception:" + Message.Trim()
                + (!WithStack
                ? ""
                : "\n" + "<<"
                + "\n" + Stack.Trim()
                + "\n" + ">>"
                );
        }
        //------------------------------------------------------------------------------------------
        static public event EventHandler LogedMessage;
        private void NotifyUpdated(object obj)
        {
            var handler = LogedMessage;
            if (handler != null) handler(obj, null);
        }
    }
}
