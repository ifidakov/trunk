using eDoctrinaUtils;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace eDoctrinaOcrEd
{
    public partial class MiniatureListItemControl : CustomListItemControl
    {
        public MiniatureListItemControl()
        {
            InitializeComponent();
        }

        public MiniatureListItemControl(MiniatureItem item)
            : this()
        {
            this.Item = item;
            if (Item != null)
            {
                label1.DataBindings.Add("Text", Item, "Name");
                pictureBox1.DataBindings.Add("Image", Item, "SheetIdentifierImage");
                this.Dock = System.Windows.Forms.DockStyle.Top;
                this.Location = new System.Drawing.Point(0, 0);
                this.Name = item.Name;
                this.Text = item.Name;
            }
        }

        public MiniatureItem Item
        {
            get { return (item as MiniatureItem); }
            set
            {
                if (item != value)
                {
                    item = value;
                }
            }
        }

        public void MiniatureListItemControl_Select(object sender, System.EventArgs e)
        {
            NotifyUpdated("SelectItem", this, e);
            //Control itm = (Control)sender;
            //MiniatureListItemControl miniatureListItemControl = (MiniatureListItemControl)itm.Parent;
            //var parent = Parent.Controls;
            for (int i = 0; i < Parent.Controls.Count; i++)
            {
                MiniatureListItemControl item = (MiniatureListItemControl)Parent.Controls[i];
                if (item.Name == Name)
                {
                    IsSelected = true;
                    this.BackColor = SystemColors.ActiveCaption;
                    EditorForm ef = (EditorForm)FindForm();
                    if (ef != null)
                    {
                        try
                        {
                            EditorForm.ShetIdManualySet = true;
                            ef.BoxSheet.SelectedIndex = i;
                            var selectedItem = ef.MiniatureItems.First(x => x.Name == EditorForm.rec.SheetIdentifier);
                            ef.BoxSheet.SelectedItem = selectedItem;
                            EditorForm.rec.SheetIdentifier = Name;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                else
                {
                    item.IsSelected = false;
                    item.BackColor = SystemColors.Control;
                }
                //if (IsSelected)
                //{
                //    color = this.BackColor;
                //    this.BackColor = SystemColors.ActiveCaption;
                //}
                //else this.BackColor =  color;
            }
        }

        private Color color = SystemColors.Control;

        protected override void OnResize(System.EventArgs e)
        {
            base.OnResize(e);
            if (item != null && pictureBox1.Image != null)
            {
                double d = (double)pictureBox1.Image.Height / pictureBox1.Image.Width;
                Height = label1.Height + (int)Math.Round(Width * d);
            }
        }
    }
}
