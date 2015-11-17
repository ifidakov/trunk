using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace startApp
{
    public partial class Form1 : Form
    {
        public string arg;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        public Form1(string arg)
        {
            //InitializeComponent();
            Thread.Sleep(500);
            //Process.Start(arg);
            Process.Start("eDoctrinaOcrEd.exe");
            Environment.Exit(0);
        }
    }
}
