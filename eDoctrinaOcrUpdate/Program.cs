using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace eDoctrinaOcrUpdate
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        private static System.Threading.Mutex mInstance;
        private static string mAppName = "eDoctrinaOcrUpdate-" + Environment.CurrentDirectory.Replace(":", "").Replace("\\", "");

        [STAThread]
        static void Main()
        {
            bool tryCreateNewApp;
            mInstance = new System.Threading.Mutex(true, mAppName, out tryCreateNewApp);
            if (!tryCreateNewApp)
            {
                //MessageBox.Show("The program has been already started.");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UpdateForm());
        }
    }
}
