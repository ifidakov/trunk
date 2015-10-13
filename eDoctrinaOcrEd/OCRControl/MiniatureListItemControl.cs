using eDoctrinaUtils;
using System;
using System.Drawing;

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

        private void MiniatureListItemControl_Select(object sender, System.EventArgs e)
        {
            NotifyUpdated("SelectItem", this, e);
            if (IsSelected)
            {
                color = this.BackColor;
                this.BackColor = SystemColors.ActiveCaption;
            }
            else this.BackColor = color;
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
