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
    public partial class CustomListItemControl : Control
    {
        public CustomListItemControl()
        {
            InitializeComponent();
        }
        //-------------------------------------------------------------------------
        public CustomListItemControl(object item)
            : this()
        {
            this.item = item;
            if (item != null)
            {
                this.Dock = System.Windows.Forms.DockStyle.Top;
                this.Location = new System.Drawing.Point(0, 0);
            }
        }

        protected object item;

        //-------------------------------------------------------------------------
        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                }
            }
        }
        //-------------------------------------------------------------------------
        protected override void OnClick(EventArgs e)
        {
            NotifyUpdated(SelectItem, this, e);
        }

        #region Event
        public event EventHandler SelectItem;
        //-------------------------------------------------------------------------
        protected void NotifyUpdated(string key, object obj, EventArgs e)
        {
            switch (key)
            {
                case "SelectItem":
                    var handler = SelectItem;
                    if (handler != null) handler(obj, e);
                    break;
            }
        }
        //-------------------------------------------------------------------------
        protected void NotifyUpdated(EventHandler handler, object obj, EventArgs e)
        {
            if (handler != null) handler(obj, e);
        }
        //-------------------------------------------------------------------------
        #endregion
    }
}
