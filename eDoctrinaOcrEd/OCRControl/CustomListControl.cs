using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace eDoctrinaOcrEd
{
    public partial class CustomListControl : Control
    {
        public CustomListControl()
        {
            InitializeComponent();
        }

        //protected virtual void DataSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    AddComponent();
        //    RemoveComponent();
        //}

        public virtual ControlCollection ControlList
        {
            get { return ListPanel.Controls; }
            //set
            //{
            //    if (controlList != value)
            //    {
            //        controlList = value;
            //        if (controlList != null) 
            //            controlList.CollectionChanged += DataSource_CollectionChanged;
            //    }
            //}
        }

        [DefaultValue(-1)]
        public int selectedIndex;
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                if (selectedIndex != value)
                {
                    selectedIndex = value;
                    NotifyUpdated("SelectedIndexChanged", SelectedIndex, null);
                }
            }
        }

        private CustomListItemControl selectedControl;
        public CustomListItemControl SelectedControl
        {
            get { return selectedControl; }
            set
            {
                if (selectedControl != value)
                {
                    if (selectedControl != null) selectedControl.IsSelected = false;
                    selectedControl = value;
                    selectedControl.IsSelected = true;
                    NotifyUpdated("SelectedItemChanged", selectedControl, null);
                }
                if (selectedControl != null) ScrollControlIntoView(selectedControl);
            }
        }

        protected virtual void AddComponent(CustomListItemControl control)
        {
            if (this.ListPanel.Controls.Contains(control))
                return;
            control.Width = this.Width;
            control.SelectItem += IsSelected;
            this.ListPanel.Controls.Add(control);
        }

        protected virtual void RemoveComponent(Control control)
        {
            if (!this.Controls.Contains(control))
                return;
            this.ListPanel.Controls.Remove(control);
        }

        private void IsSelected(object sender, EventArgs e)
        {
            var obj = sender as CustomListItemControl;
            if (obj != null)
            {
                SelectedControl = obj;
                //SelectedIndex = obj.Item.Index;
            }
        }

        public void ScrollControlIntoView(Control control)
        {
            if (control != null)
            {
                panel1.ScrollControlIntoView(control);
            }
        }

        public Control FindControlByName(string name)
        {
            var temp = ListPanel.Controls.Find(name, false);
            if (temp.Count() > 0)
            {
                return temp.First();
            }
            return null;
        }

        public int GetIndex(string name)
        {
            return GetIndex(FindControlByName(name));
        }
      
        public int GetIndex(Control control)
        {
            return ListPanel.Controls.GetChildIndex(control);
        }

        #region EventHandler
        //-------------------------------------------------------------------------
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public event EventHandler SelectedItemChanged;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public event EventHandler SelectedIndexChanged;
        protected void NotifyUpdated(string key, object obj, EventArgs e)
        {
            switch (key)
            {
                case "SelectedItemChanged":
                    var handler = SelectedItemChanged;
                    if (handler != null) handler(obj, e);
                    break;
                case "SelectedIndexChanged":
                    handler = SelectedIndexChanged;
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
