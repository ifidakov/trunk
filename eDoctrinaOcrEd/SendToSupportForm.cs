using eDoctrinaUtils;
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
    public partial class SendToSupportForm : Form
    {
        public SendToSupportForm()
        {
            InitializeComponent();
        }

        public string Message;
        public string Email;
        //-------------------------------------------------------------------------
        private void SendToSupportForm_Load(object sender, EventArgs e)
        {
            this.EmailTextBox.Text = Email;
        }
        //-------------------------------------------------------------------------
        private void SendButton_Click(object sender, EventArgs e)
        {
            Message = this.CommentTextBox.Text;
            Email = this.EmailTextBox.Text;
            DialogResult = DialogResult.OK;
            this.Close();
        }
        //-------------------------------------------------------------------------
        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
        //-------------------------------------------------------------------------
        private void CommentTextBox_MouseEnter(object sender, EventArgs e)
        {
            AcceptButton = null;
        }
        //-------------------------------------------------------------------------
        private void CommentTextBox_MouseLeave(object sender, EventArgs e)
        {
            AcceptButton = SendButton;
        }
        //-------------------------------------------------------------------------
        //-------------------------------------------------------------------------
        //-------------------------------------------------------------------------
        //-------------------------------------------------------------------------
    }
}
