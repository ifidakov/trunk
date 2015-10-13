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
    public partial class RecognizeProcessingForm : Form
    {
        public RecognizeProcessingForm()
        {
            InitializeComponent();
        }

        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            //try
            //{
            //int[] i = new int[0];
            //int j = i[1];
            //Owner.Focus();
            //Hide();
            //Owner = null;
            //DialogResult = DialogResult.Abort;
            Close();
            //}
            //catch (Exception)
            //{
            //    //this.Dispose();
            //    //this = null;
            //}
        }

        private void RecognizeProcessingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (DialogResult == DialogResult.Abort)
            //    e.Cancel = true;
        }
    }
}
