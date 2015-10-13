using eDoctrinaUtils;
using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace eDoctrinaOcrEd
{
    static class Program
    {
        private static System.Threading.Mutex mInstance;
        private static string mAppName = "eDoctrinaOcrEd-" + Environment.CurrentDirectory.Replace(":", "").Replace("\\", "");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [HandleProcessCorruptedStateExceptions]
        [STAThread]
        static void Main()
        {
            bool tryCreateNewApp;
            mInstance = new System.Threading.Mutex(true, mAppName, out tryCreateNewApp);
            if (!tryCreateNewApp)
            {
                MessageBox.Show("The program has been already started.");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new EditorForm());
            }
            catch (Exception ex)
            {
                //Log.LogMessage(ex);
                //string message = "Send log file to developers for fixing problem.\r\nThe program will be closed.";
                //Log.LogMessage(message);
                //mInstance.ReleaseMutex();
                mInstance.Close();
                mInstance.Dispose();
                new ErrorRestart(KeyProgram.eDoctrinaOcrEd).ReStart(ex);
            }
        }
    }
}
