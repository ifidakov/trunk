using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace eDoctrinaOcrEd
{
    public partial class LinsForm : Form
    {
        public LinsForm()
        {
            InitializeComponent();
        }

        //-------------------------------------------------------------------------
        private void LinsForm_Load(object sender, EventArgs e)
        {
            trackBar1.Location = new Point(statusStrip1.Location.X + 3, statusStrip1.Location.Y + 3);
            trackBar1_Scroll(null, null);
        }
        //-------------------------------------------------------------------------
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            Opacity = (double)trackBar1.Value / 100;
        }
        //-------------------------------------------------------------------------
        public void rbtnGrid_CheckedChanged(object sender, EventArgs e)
        {
            //groupBox1.Enabled = rbtnGrid.Checked;
            EditorForm ef = (EditorForm)this.Owner;
            if (ef.rbtnRotate.Checked)
            {
                toolStripStatusLabel3.Text = "Show line";
                toolStripStatusLabel3.ToolTipText = "Show line, which should be horizontal or vertical";
            }
            else if (EditorForm.barCodeSel)
            {
                toolStripStatusLabel3.Text = "Show areas";
                toolStripStatusLabel3.ToolTipText = "Show areas of bar codes";
            }
            else
            {
                toolStripStatusLabel3.Text = "Show areas";
                toolStripStatusLabel3.ToolTipText = "Show areas of bubbles";
            }
            ef.barCodeList.SelectedItem = null;
            ef.pictureBox1_MouseEnter(null, null);
        }
        //-------------------------------------------------------------------------
        private void nudRows_Leave(object sender, EventArgs e)
        {
            Control c = nudRows as Control;
            if (string.IsNullOrEmpty(c.Text))
                nudRows.Value = 0;
        }

        private void nudCols_ValueChanged(object sender, EventArgs e)
        {

        }

        private void nudRows_ValueChanged(object sender, EventArgs e)
        {

        }

        private void nudSubRows_ValueChanged(object sender, EventArgs e)
        {

        }
        //-------------------------------------------------------------------------
    }
}
