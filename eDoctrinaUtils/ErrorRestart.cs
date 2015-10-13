using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace eDoctrinaUtils
{
    public enum KeyProgram { eDoctrinaOcr, eDoctrinaOcrEd };

    public class ErrorRestart
    {
        private KeyProgram Key;
        private string StartProgram;
        private string Caption;
        Log log = new Log();
        public ErrorRestart(KeyProgram key)
        {
            Key = key;
        }

        public void ReStart(Exception ex)
        {
            switch (Key)
            {
                case KeyProgram.eDoctrinaOcr:
                    StartProgram = "eDoctrinaOcrWPF.exe";
                    Caption = "Service (eDoctrinaOcr)";
                    ReStartProgram(ex);
                    break;
                case KeyProgram.eDoctrinaOcrEd:
                    StartProgram = "eDoctrinaOcrEd.exe";
                    Caption = "Editor (eDoctrinaOcrEd)";
                    ReStartProgram(ex);
                    break;
            }
        }

        private void ReStartProgram(Exception ex)
        {
            Caption += " restarting...";
            log.LogMessage(ex);
            string message = "Send log file to developers for fixing problem.\r\nThe program will be closed.";
            log.LogMessage(message);
            Process.Start(StartProgram);
            message = "MachineName: " + Environment.MachineName + "\r\n";
            message += "UserName: " + Environment.UserName + "\r\n";
            message += "OSVersion: " + Environment.OSVersion + "\r\n";
            message += "CurrentDirectory: " + Environment.CurrentDirectory + "\r\n";
            message += "\r\n" + log.GetExceptionMessage(ex, true);
            Send(message);
        }

        private void Send(string message)
        {
            var task = Task.Factory.StartNew(status =>
            {
                MailHelper mail = new MailHelper();
                mail.SendMailToAdminWithError(Caption, message);
            }, "SendMail").ContinueWith((t) =>
            {
                if (t.Exception != null) log.LogMessage(t.Exception);
            });
            task.Wait();
        }

        public void SendErrorMail(string message)
        {
            Send(message);
        }
    }
}
