using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Installer
{
    public partial class InstallerForm : Form
    {
        public InstallerForm()
        {
            InitializeComponent();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            label1.Text = "Installing, please wait...";
            Refresh();
            Application.DoEvents();
            try
            {
                Process[] processes = Process.GetProcessesByName("eDoctrinaOcrEd");
                foreach (Process proc in processes)
                {
                    string fn = proc.MainModule.FileName;
                    //\/для x32
                    //MainWindowTitle = "1 Release eDoctrina OCR Editor v. 1.0.0.459 (D:\\E\\Visual_C#\\2008 Проекты C#\\svn\\eDoctrinaOcr\\trunk\\Release)"
                    //string fn = proc.MainModule.FileName;
                    //fn.Remove(0, fn.IndexOf("("));
                    //fn.TrimEnd(')');
                    FileInfo fi = new FileInfo(fn);
                    FileInfo fi2 = new FileInfo(Application.ExecutablePath);
                    if (fi.Directory.ToString().Equals(fi2.Directory.ToString()))
                    {
                        chbRunEditor.Checked = true;
                        proc.Kill();
                        //proc.CloseMainWindow();
                        //proc.WaitForExit();
                        break;
                    }
                }
                processes = Process.GetProcessesByName("eDoctrinaOcrWPF");
                foreach (Process proc in processes)
                {
                    string fn = proc.MainModule.FileName;
                    FileInfo fi = new FileInfo(fn);
                    FileInfo fi2 = new FileInfo(Application.ExecutablePath);
                    if (fi.Directory.ToString().Equals(fi2.Directory.ToString()))
                    {
                        chbRunService.Checked = true;
                        proc.Kill();
                        break;
                    }
                }
                Thread.Sleep(1000);
                foreach (string item in Directory.GetFiles("Updates"))
                {
                    FileInfo fi = new FileInfo(item);
                    string source = Path.Combine("Updates", fi.Name);
                    File.Copy(source, fi.Name, true);
                    File.SetAttributes(fi.Name, FileAttributes.Normal);
                }
                label1.Text = "Installing completed successfully";
            }
            catch (Exception ex)
            {
                label1.Text = "Installation failed!";
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            btnExit.Enabled = true;
            chbRunEditor.Enabled = true;
            chbRunService.Enabled = true;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (chbRunService.Checked)
            {
                Process.Start("eDoctrinaOcrWPF.exe");
            }

            if (chbRunEditor.Checked)
            {
                Process.Start("eDoctrinaOcrEd.exe");
            }
            Close();
        }
    }
}
