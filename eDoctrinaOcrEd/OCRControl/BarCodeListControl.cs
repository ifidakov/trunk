using eDoctrinaUtils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace eDoctrinaOcrEd
{
    public partial class BarCodeListControl : CustomListControl
    {
        public BarCodeListControl()
        {
            InitializeComponent();
        }

        private ObservableCollection<BarCodeItem> dataSource;
        public ObservableCollection<BarCodeItem> DataSource
        {
            get { return dataSource; }
            set
            {
                if (dataSource != value)
                {
                    dataSource = value;
                    if (dataSource != null)
                        dataSource.CollectionChanged += DataSource_CollectionChanged;
                }
            }
        }

        private BarCodeItem selectedItem;
        public BarCodeItem SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (selectedItem != value)//!EditorForm.barCodeSel && 
                {
                    selectedItem = value;
                    NotifyUpdated("SelectedItemChanged", selectedItem, null);
                }
                if (selectedItem != null)
                    ScrollControlIntoView(FindControlByName(selectedItem.Name));
            }
        }

        private void DataSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                if (e.NewItems != null)
                    foreach (var item in e.NewItems)
                    {
                        var bItem = new BarCodeListItemControl(item as BarCodeItem);
                        bItem.OkClick += (o, ea) => { NotifyUpdated(OkButtonClick, o, ea); };
                        //bItem.BarCodeMouseEnter += (o, ea) => { NotifyUpdated(BarCodeMouseEnter, o, ea); };
                        bItem.BarCodeMouseClick += (o, ea) => { NotifyUpdated(BarCodeMouseClick, o, ea); };
                        bItem.BarCodeMouseLeave += (o, ea) => { NotifyUpdated(BarCodeMouseLeave, o, ea); };
                        AddComponent(bItem);
                    }
            if (e.Action == NotifyCollectionChangedAction.Remove)
                if (e.OldItems != null)
                    foreach (var item in e.OldItems)
                    {
                        RemoveComponent(new BarCodeListItemControl(item as BarCodeItem));
                    }
            if (e.Action == NotifyCollectionChangedAction.Reset)
            { this.ControlList.Clear(); }
            if (e.Action != NotifyCollectionChangedAction.Add && e.Action != NotifyCollectionChangedAction.Reset)
            { }
        }

        public void Select(BarCodeItem item)
        {
            var temp = FindControlByName(item.Name);
            if (temp != null)
            {
                NotifyUpdated(BarCodeMouseClick, temp, null);
            }
        }

        public void SelectNextBarCodeListItemControl(bool forward) //It needs to find best way
        {
            if (forward)
            {
                var ctr = GetNextControl(this.SelectedControl, true);
                ctr = GetNextControl(ctr, true);
                ctr = GetNextControl(ctr, true);
                ctr = GetNextControl(ctr, true);
                ctr = GetNextControl(ctr, true);
                this.SelectNextControl(ctr, true, true, true, true);
            }
            else
            {
                this.SelectNextControl(this.SelectedControl, false, true, true, true);
            }
        }

        #region EventHandler
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public event EventHandler OkButtonClick;
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        //public event EventHandler BarCodeMouseEnter;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public event EventHandler BarCodeMouseClick;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public event EventHandler BarCodeMouseLeave;
        #endregion
    }
}
