using eDoctrinaUtils;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace eDoctrinaOcrEd
{
    public partial class BarCodeListItemControl : CustomListItemControl
    {
        public BarCodeListItemControl()
        {
            InitializeComponent();
        }

        public BarCodeListItemControl(BarCodeItem item)
            : this()
        {
            this.Item = item;
            if (Item != null)
            {
                comboBox1.DropDownStyle = ComboBoxStyle.Simple;
                comboBox1.DataBindings.Add("Text", Item, "Value", true, DataSourceUpdateMode.OnPropertyChanged, "");
                BorderPanel.DataBindings.Add("BackColor", Item, "BorderColor");
                DataBindings.Add("Text", Item, "Name");
                Name = item.Name;
            }
        }

        public BarCodeItem Item
        {
            get { return (item as BarCodeItem); }
            set
            {
                if (item != value)
                {
                    item = value;
                }
            }
        }

        #region Global Property
        public override string Text
        {
            get { return label1.Text; }
            set
            {
                if (label1.Text != value)
                {
                    label1.Text = value;
                }
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ComboBoxStyle DropDownStyle
        {
            get { return comboBox1.DropDownStyle; }
            set
            {
                if (comboBox1.DropDownStyle != value)
                {
                    comboBox1.DropDownStyle = value;
                }
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public object DataSource
        {
            get { return comboBox1.DataSource; }
            set
            {
                if (comboBox1.DataSource != value)
                {
                    comboBox1.DataSource = value;
                }
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ComboBox.ObjectCollection Items
        {
            get { return comboBox1.Items; }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public string DisplayMember
        {
            get { return comboBox1.DisplayMember; }
            set
            {
                if (comboBox1.DisplayMember != value)
                {
                    comboBox1.DisplayMember = value;
                }
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public int SelectedIndex
        {
            get { return comboBox1.SelectedIndex; }
            set
            {
                if (comboBox1.SelectedIndex != value)
                {
                    comboBox1.SelectedIndex = value;
                }
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public object SelectedItem
        {
            get { return comboBox1.SelectedItem; }
            set
            {
                if (comboBox1.SelectedItem != value)
                {
                    comboBox1.SelectedItem = value;
                }
            }
        }
        #endregion

        #region Events
        //-------------------------------------------------------------------------
        private void BarCodeListItemControl_FocusEnter(object sender, EventArgs e)
        {
            //comboBox1.Text = "0";
            //comboBox1.CausesValidation = true;
            //NotifyUpdated(BarCodeMouseEnter, this, e);
            //comboBox1.Focus();
            //comboBox1.Capture = true;
        }
        //-------------------------------------------------------------------------
        private void Ok_Click(object sender, EventArgs e)
        {
            NotifyUpdated(OkClick, this, e);
            //comboBox1.Capture = false;
            if (String.IsNullOrEmpty(comboBox1.Text))
            {
                EditorForm ef = (EditorForm)this.FindForm();
                if (ef == null)
                    return;
                if (ef.Status != StatusMessage.Verify)
                    return;
                if (ef.linsForm != null && (ef.linsForm.Visible))
                    return;
                ef.comboBoxFocused = true;
                ef.UpdateUI("Exception", "Empty value in " + Name + "!");
            }
        }
        //-------------------------------------------------------------------------
        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            NotifyUpdated(SelectedValueChanged, this, e);
        }
        //-------------------------------------------------------------------------
        public void BarCodeListItemControl_Click(object sender, System.EventArgs e)
        {
            NotifyUpdated(BarCodeMouseClick, this, e);//BarCodeMouseEnter
            if (!comboBox1.Focused)
            {
                comboBox1.Focus();
                //comboBox1.Capture = true;
            }
        }
        //-------------------------------------------------------------------------
        public void BarCodeListItemControl_Leave(object sender, System.EventArgs e)
        {
            NotifyUpdated(BarCodeMouseLeave, this, e);

            if (textChanged)
            {
                textChanged = false;
                comboBox1.Capture = false;
                btnCheck.PerformClick();
            }
        }
        //-------------------------------------------------------------------------
        private void BarCodeListItemControl_Select(object sender, System.EventArgs e)
        {
            NotifyUpdated("SelectItem", this, e);
        }
        //-------------------------------------------------------------------------
        #endregion

        #region EventHandler
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public event EventHandler OkClick;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public event EventHandler SelectedValueChanged;
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        //public event EventHandler BarCodeMouseEnter;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public event EventHandler BarCodeMouseClick;

        //-------------------------------------------------------------------------
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public event EventHandler BarCodeMouseLeave;
        #endregion

        public bool textChanged;
        //-------------------------------------------------------------------------
        private void comboBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                textChanged = false;
                btnCheck.PerformClick();
            }
            else
            {
                textChanged = true;
                comboBox1.ForeColor = Color.Black;
            }
        }
        //-------------------------------------------------------------------------
        private void comboBox1_MouseClick(object sender, MouseEventArgs e)
        {
            EditorForm ef = (EditorForm)this.FindForm();
            ef.pictureBox1_MouseEnter(sender, e);
        }
        //-------------------------------------------------------------------------
        public void radioButton1_Click(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            EditorForm ef = (EditorForm)this.FindForm();
            ef.rbtnGrid.Checked = false;
            ef.rbtnRotate.Checked = false;
            ef.rbtnBubbles.Checked = false;
            ef.rbtnCut.Checked = false;
            if (ef.linsForm != null)
                ef.linsForm.Hide();
            else
                ef.CreateLinsForm();
            ef.UncheckRbtn(sender, e);
        }
        //-------------------------------------------------------------------------
        private void BarCodeListItemControl_Enter(object sender, EventArgs e)
        {
            try
            {
                var item = sender as BarCodeListItemControl;
                EditorForm ef = (EditorForm)this.FindForm();
                if (item.Item.Rectangle.Width > item.Item.Rectangle.Height)
                    ef.UpdateZoomedImage(item.Item.Rectangle.X + item.Item.Rectangle.Width / 4, item.Item.Rectangle.Y);
                else
                    ef.UpdateZoomedImage(item.Item.Rectangle.X, item.Item.Rectangle.Bottom - item.Item.Rectangle.Height / 4);
            }
            catch (Exception)
            {
            }
        }
        //-------------------------------------------------------------------------
    }
}