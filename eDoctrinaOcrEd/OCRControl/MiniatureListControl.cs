using eDoctrinaUtils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Forms;

namespace eDoctrinaOcrEd
{
    public partial class MiniatureListControl : CustomListControl
    {
        public MiniatureListControl()
        {
            InitializeComponent();
        }

        private ObservableCollection<MiniatureItem> dataSource;
        public ObservableCollection<MiniatureItem> DataSource
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

        private void DataSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
                if (e.NewItems != null)
                    foreach (var item in e.NewItems)
                    {
                        AddComponent(new MiniatureListItemControl(item as MiniatureItem));
                    }
            if (e.Action == NotifyCollectionChangedAction.Remove)
                if (e.OldItems != null)
                    foreach (var item in e.OldItems)
                    {
                        RemoveComponent(new MiniatureListItemControl(item as MiniatureItem));
                    }
            if (e.Action == NotifyCollectionChangedAction.Reset)
            { this.ControlList.Clear(); }
        }
         
        private MiniatureItem selectedItem;
        public MiniatureItem SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (selectedItem != value)
                {
                    selectedItem = value;
                    NotifyUpdated("SelectedItemChanged", selectedItem, null);
                }
                if (selectedItem != null) ScrollControlIntoView(FindControlByName(selectedItem.Name));
            }
        }
    }
}
