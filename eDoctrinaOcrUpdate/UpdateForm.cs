using eDoctrinaUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace eDoctrinaOcrUpdate
{
    public partial class UpdateForm : Form
    {
        public UpdateForm()
        {
            InitializeComponent();
        }
        //private static System.Threading.Mutex mInstance;
        //private static string mAppName = "eDoctrinaOcrEd-" + Environment.CurrentDirectory.Replace(":", "").Replace("\\", "");
        private OcrAppConfig defaults;
        Updates upd = null;
        private void MainForm_Load(object sender, EventArgs e)
        {
            Text += " version" + Application.ProductVersion.ToString();
        }
        //[Serializable]
        //public class Updates
        //{
        //    public string version { get; set; }
        //    public string description { get; set; }
        //    public string[] files { get; set; }
        //}
        //-------------------------------------------------------------------------
        //static public Updates GetUpdates(string input)
        //{
        //    Updates updates
        //    = JsonConvert.DeserializeObject<Updates>(input);
        //    return updates;
        //}

        private void MainForm_Shown(object sender, EventArgs e)
        {//"updateServerName": "http://edococr.etlspace.com/" //ftp://edococr@edococr.etlspace.com/ edococr ooz9tai3Eing
            Refresh();
            Application.DoEvents();
            try
            {
                defaults = new OcrAppConfig();
                if (string.IsNullOrEmpty(defaults.UpdateServerName))
                {
                    MessageBox.Show("Not set Updates server name."
                        , Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    upd = null;
                    Close();
                    return;
                }
                WebClient client = new WebClient();
                string input;
                if (defaults.UpdateServerName.StartsWith("http"))
                {
                    input = client.DownloadString(Path.Combine(defaults.UpdateServerName, "updates.json"));
                }
                else
                {
                    input = File.ReadAllText(Path.Combine(defaults.UpdateServerName, "updates.json"));
                }
                upd = Recognize.GetUpdates(input);
                if (upd.version.Equals(Application.ProductVersion.ToString()))
                {
                    MessageBox.Show("You are using the latest version of the application."
                        , Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Close();
                    return;
                }
                else
                {
                    if (MessageBox.Show("There is new version" + upd.version + " found, your version is "
                        + Application.ProductVersion.ToString() + Environment.NewLine
                       + "----------------------------------" + Environment.NewLine
                       + "Description:" + Environment.NewLine + upd.description + Environment.NewLine
                       + "----------------------------------" + Environment.NewLine
                       + "Do you want to install update?"
                     , Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    {
                        Close();
                        return;
                    }
                }
                label1.Text = "Installing updates, please wait...";
                if (!Directory.Exists("Updates"))
                    Directory.CreateDirectory("Updates");
                else
                {
                    DirectoryInfo dirInfo = new DirectoryInfo("Updates");
                    foreach (FileInfo file in dirInfo.GetFiles())
                    {
                        file.Delete();
                    }
                }
                FileInfo fi = new FileInfo(Application.ExecutablePath);
                string currentDirectory = fi.Directory.ToString();
                if (defaults.UpdateServerName.StartsWith("http"))
                {
                    foreach (string item in upd.files)
                    {
                        fi = new FileInfo(item);

                        string dest = Path.Combine("Updates", fi.Name);
                        string source = Path.Combine(defaults.UpdateServerName, fi.Name);
                        Uri uri = new Uri(source);
                        label1.Text = "Loading " + fi.Name;
                        Invoke(new MethodInvoker(delegate
                        {
                            label1.Refresh();
                        }));
                        Application.DoEvents();
                        client.DownloadFile(uri, dest);
                    }
                }
                else
                {
                    foreach (string item in upd.files)
                    {
                        fi = new FileInfo(item);
                        string dest = Path.Combine("Updates", fi.Name);
                        label1.Text = "Loading " + fi.Name;
                        Invoke(new MethodInvoker(delegate
                        {
                            label1.Refresh();
                        }));
                        Application.DoEvents();
                        string source = Path.Combine(defaults.UpdateServerName, item);
                        File.Copy(Path.Combine(defaults.UpdateServerName, item), dest);
                    }
                }
                //Process[] processes = Process.GetProcessesByName("eDoctrinaOcrEd");
                //foreach (Process proc in processes)
                //{
                //    string fn = proc.MainModule.FileName;
                //    fi = new FileInfo(fn);
                //    FileInfo fi2 = new FileInfo(Application.ExecutablePath);
                //    if (fi.Directory.ToString().Equals(fi2.Directory.ToString()))
                //    {
                //        chbRunEditor.Checked = true;
                //        proc.Kill();
                //        //proc.CloseMainWindow();
                //        //proc.WaitForExit();
                //        break;
                //    }
                //}
                //processes = Process.GetProcessesByName("eDoctrinaOcrWPF");
                //foreach (Process proc in processes)
                //{
                //    string fn = proc.MainModule.FileName;
                //    fi = new FileInfo(fn);
                //    FileInfo fi2 = new FileInfo(Application.ExecutablePath);
                //    if (fi.Directory.ToString().Equals(fi2.Directory.ToString()))
                //    {
                //        chbRunService.Checked = true;
                //        proc.Kill();
                //        break;
                //    }
                //}
                //processes = Process.GetProcessesByName("eDoctrinaUtils.dll");
                //foreach (Process proc in processes)
                //{
                //    string fn = proc.MainModule.FileName;
                //    fi = new FileInfo(fn);
                //    FileInfo fi2 = new FileInfo(Application.ExecutablePath);
                //    if (fi.Directory.ToString().Equals(fi2.Directory.ToString()))
                //    {
                //        proc.Kill();
                //        break;
                //    }
                //}
                //Thread.Sleep(500);
                //defaults = null;
                foreach (string item in upd.files)
                {
                    fi = new FileInfo(item);
                    if (!fi.Directory.ToString().Equals(currentDirectory) || item.StartsWith("Installer"))
                    {
                        if (!Directory.Exists(fi.Directory.ToString()))
                            Directory.CreateDirectory(fi.Directory.ToString());
                        string source = Path.Combine("Updates", fi.Name);
                        File.Delete(item);
                        File.Move(source, item);
                        File.SetAttributes(item, FileAttributes.Normal);
                    }
                }
                label1.Text = "Loading completed successfully";
                TopMost = false;
                Process.Start("Installer.exe");
                Thread.Sleep(500);
                Close();

            }
            catch (Exception ex)
            {
                label1.Text = "Installation failed!";
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Close();
            }
            //btnExit.Enabled = true;
            //chbRunEditor.Enabled = true;
            //chbRunService.Enabled = true;

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            //if (chbRunService.Checked)
            //{
            //    Process.Start("eDoctrinaOcrWPF.exe");
            //}

            //if (chbRunEditor.Checked)
            //{
            //    Process.Start("eDoctrinaOcrEd.exe");
            //}
            //Process.Start("Installer.exe");
            //Close();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //    if (upd != null)
            //    {
            //       this.Dispose();
            //        FileInfo fi = new FileInfo(Application.ExecutablePath);
            //        string currentDirectory = fi.Directory.ToString();

            //        foreach (string item in upd.files)
            //        {
            //            fi = new FileInfo(item);
            //            if (!fi.Directory.ToString().Equals(currentDirectory))
            //            {
            //                Directory.CreateDirectory(fi.Directory.ToString());
            //            }
            //            string source = Path.Combine("Updates", fi.Name);
            //            File.Copy(source, item, true);
            //        }
            //    }
            //    if (chbRunService.Checked)
            //    {
            //        Process.Start("eDoctrinaOcrWPF.exe");
            //    }

            //    if (chbRunEditor.Checked)
            //    {
            //        Process.Start("eDoctrinaOcrEd.exe");
            //    }
        }
    }
}
