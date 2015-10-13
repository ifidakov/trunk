using System;
using System.Drawing;
using System.Windows.Forms;
using eDoctrinaUtils;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace eDoctrinaOcrEd
{
    public partial class BubblesAreaControl2 : UserControl
    {
        public BubblesAreaControl2()
        {
            InitializeComponent();
        }
        Log log = new Log();

        //-------------------------------------------------------------------------
        public void FillBubblesAreaControl(RegionsArea[] Areas, int amoutOfQuestions, int maxAmoutOfQuestions, int indexOfFirstQuestion, int[] LinesPerArea, int[] BubblesPerLine)
        {
            this.Areas = Areas;
            this.LinesPerArea = LinesPerArea;
            this.BubblesPerLine = BubblesPerLine;
            this.IndexOfFirstQuestion = indexOfFirstQuestion;
            this.maxAmoutOfQuestions = maxAmoutOfQuestions;
            this.AmoutOfQuestions = amoutOfQuestions;
        }
        //-------------------------------------------------------------------------
        private RegionsArea[] Areas;
        public int[] LinesPerArea;
        public int[] BubblesPerLine;
        //-------------------------------------------------------------------------
        private int maxAmoutOfQuestions;
        private int amoutOfQuestions;
        public int AmoutOfQuestions
        {
            get
            {
                return amoutOfQuestions;
            }
            set
            {
                if (amoutOfQuestions != value)
                {
                    amoutOfQuestions = value;
                    //this.Visible = false;
                    ChangeAmoutOfQuestions();
                    //this.Visible = true;
                }
            }
        }
        //-------------------------------------------------------------------------
        private void ChangeAmoutOfQuestions()
        {
            if ((maxAmoutOfQuestions != 0 && amoutOfQuestions > maxAmoutOfQuestions) //не может быть!!!!!!!!!!!!!!
                || amoutOfQuestions == LabelList.Count)
            {
                return;
            }
            if (CheckBoxList.Count > 0)
            {
                if (amoutOfQuestions < LabelList.Count)
                {
                    log.LogMessage("DownScaleAmoutOfQuestions");
                    DownScaleAmoutOfQuestions();
                }
                else
                {
                    log.LogMessage("IncreaseAmoutOfQuestions");
                    IncreaseAmoutOfQuestions();
                }
                return;
            }
            else
            {
                log.LogMessage("CreateNewBubblesArea");
                CreateNewBubblesArea();
            }
        }
        //-------------------------------------------------------------------------
        private int indexOfFirstQuestion = 1;
        public int IndexOfFirstQuestion
        {
            get
            {
                return indexOfFirstQuestion;
            }
            set
            {
                if (indexOfFirstQuestion != value)
                {
                    indexOfFirstQuestion = value;
                    ChangeIndexOfFirstQuestion();
                }
            }
        }
        //-------------------------------------------------------------------------
        private void ChangeIndexOfFirstQuestion()
        {
            if (LabelList.Count > 0)
            {
                int index = indexOfFirstQuestion;
                foreach (var item in LabelList)
                {
                    item.Text = index.ToString();
                    index++;
                }
            }
        }
        //-------------------------------------------------------------------------
        public List<CheckBox> CheckBoxList = new List<CheckBox>();
        public List<Label> LabelList = new List<Label>();
        //-------------------------------------------------------------------------
        private ObservableCollection<BubbleItem> dataSource;
        public ObservableCollection<BubbleItem> DataSource
        {
            get { return dataSource; }
            set
            {
                if (dataSource != value)
                {
                    dataSource = value;
                    if (dataSource != null)
                    {
                        dataSource.CollectionChanged += DataSource_CollectionChanged;
                    }
                }
            }
        }
        //-------------------------------------------------------------------------
        private void DataSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            { dataSource.CollectionChanged -= DataSource_CollectionChanged; }
            if (e.Action == NotifyCollectionChangedAction.Add)
                if (e.NewItems != null)
                    foreach (var item in e.NewItems)
                    {
                        try
                        {
                            CheckBoxList[e.NewStartingIndex].DataBindings.Add("BackColor", item as BubbleItem, "BorderColor");
                        }
                        catch { }
                        //   var bItem = new BarCodeListItemControl(item as BarCodeItem);
                        //bItem.OkClick += (o, ea) => { NotifyUpdated(OkButtonClick, o, ea); };
                        //bItem.BarCodeMouseEnter += (o, ea) => { NotifyUpdated(BarCodeMouseEnter, o, ea); };
                        //bItem.BarCodeMouseLeave += (o, ea) => { NotifyUpdated(BarCodeMouseLeave, o, ea); };
                        //AddComponent(bItem);
                    }
            if (e.Action == NotifyCollectionChangedAction.Remove)
                if (e.OldItems != null)
                    foreach (var item in e.OldItems)
                    {
                        //RemoveComponent(new BarCodeListItemControl(item as BarCodeItem));
                    }

            if (e.Action != NotifyCollectionChangedAction.Add && e.Action != NotifyCollectionChangedAction.Reset)
            { }
        }
        //-------------------------------------------------------------------------      
        public void SetInvertBubbleChecked(BubbleItem value, bool controlPressed = false)
        {
            var item = FindCheckBox(value);
            if (item != null)
            {
                if (!controlPressed)
                    item.Checked = !item.Checked;
                else
                {
                    item.Checked = true;
                    foreach (var itm in CheckBoxList)
                    {
                        BubbleItem bi = (BubbleItem)itm.Tag;
                        if (bi.Bubble.areaNumber != value.Bubble.areaNumber
                            || bi.Bubble.point.Y != value.Bubble.point.Y
                            || bi.Equals(value))
                            continue;
                        itm.Checked = false;
                        EditorForm ef = (EditorForm)this.FindForm();
                        ef.DrawBubble(bi.CheckedBubble);
                    }
                }
            }
        }
        //-------------------------------------------------------------------------
        public void Select(BubbleItem item)
        {
            var temp = FindCheckBox(item);
            if (temp != null)
            {
                NotifyUpdated(BubbleMouseEnter, temp, null);
            }
        }
        //-------------------------------------------------------------------------
        private CheckBox FindCheckBox(BubbleItem value)
        {
            foreach (var item in CheckBoxList)
            {
                if (((BubbleItem)item.Tag).Equals(value))
                {
                    return item;
                }
            }
            return null;
        }
        //-------------------------------------------------------------------------
        private CheckBox CreateCheckBox(Point point)
        {
            var cb = new CheckBox();
            cb.Padding = checkBox1.Padding;
            cb.Size = checkBox1.Size;
            cb.TabStop = false;
            cb.Location = point;
            cb.Tag = DataSource[CheckBoxList.Count];
            cb.Checked = DataSource[CheckBoxList.Count].CheckedBubble.isChecked;
            cb.DataBindings.Add("BackColor", DataSource[CheckBoxList.Count], "BorderColor");
            cb.DataBindings.Add("Checked", DataSource[CheckBoxList.Count].CheckedBubble, "isChecked", true
                , DataSourceUpdateMode.OnPropertyChanged);
            cb.CheckedChanged += new EventHandler(checkBox1_CheckedChanged);
            cb.MouseEnter += new EventHandler(checkBox1_MouseEnter);
            cb.Click += new EventHandler(checkBox1_Click);
            return cb;
        }
        //-------------------------------------------------------------------------
        private Label CreateLabel(int currentLine, Point point)
        {
            Label lbl = new Label();
            lbl.AutoSize = true;
            lbl.Text = currentLine.ToString();
            lbl.Location = point;
            lbl.TabStop = false;
            return lbl;
        }
        //-------------------------------------------------------------------------  
        private void CreateNewBubblesArea()
        {
            //DateTime dt = DateTime.Now;
            int currentArea = 0;
            int linesPerArea = LinesPerArea[currentArea];
            int linesCount = 0;
            int currentLine = indexOfFirstQuestion;
            int bubblesPerLine = BubblesPerLine[currentArea];
            CheckBox cb = CreateCheckBox(checkBox1.Location);
            this.Controls.Add(cb);
            CheckBoxList.Add(cb);
            Label lbl = CreateLabel(currentLine, lblLineNumber.Location);
            this.Controls.Add(lbl);
            LabelList.Add(lbl);
            linesCount = 0;
            currentLine--;
            for (int i = 1; i <= amoutOfQuestions; i++)//новый вопрос
            {
                if (i - linesCount > LinesPerArea[currentArea])
                {
                    linesCount += LinesPerArea[currentArea];
                    currentArea++;
                    if (Areas.Length > currentArea)
                    {
                        bubblesPerLine = Areas[currentArea].bubblesPerLine;
                    }
                }
                currentLine++;
                if (i > 1)
                {
                    cb = CreateCheckBox(new Point(checkBox1.Location.X, CheckBoxList.Last().Location.Y + checkBox1.Height + 4));
                    this.Controls.Add(cb);
                    lbl = CreateLabel(currentLine, new Point(lblLineNumber.Location.X, CheckBoxList.Last().Location.Y + checkBox1.Height + 4));
                    this.Controls.Add(lbl);
                    CheckBoxList.Add(cb);
                    LabelList.Add(lbl);
                }
                for (int k = 0; k <= Areas[currentArea].subLinesAmount; k++)//подстоки
                {
                    if (k > 0)
                    {
                        cb = CreateCheckBox(new Point(checkBox1.Location.X, CheckBoxList.Last().Location.Y + checkBox1.Height + 4));
                        this.Controls.Add(cb);
                        CheckBoxList.Add(cb);
                    }
                    for (int j = 1; j < bubblesPerLine; j++)//заполняется стpока 
                    {
                        int step = (j % 5 == 0) ? 4 : 0;
                        cb = CreateCheckBox(new Point(CheckBoxList.Last().Location.X + CheckBoxList.Last().Width + step, CheckBoxList.Last().Location.Y));
                        this.Controls.Add(cb);
                        CheckBoxList.Add(cb);
                    }
                }
            }
            this.Size = new Size(Width, CheckBoxList.Last().Location.Y + CheckBoxList.Last().Width + 4);
            //TimeSpan ts = DateTime.Now - dt;
        }
        //-------------------------------------------------------------------------  
        private void DownScaleAmoutOfQuestions()
        {
            int currentArea = 0;
            int linesPerArea = LinesPerArea[currentArea];
            int linesCount = 0;
            int currentLine = indexOfFirstQuestion;
            int bubblesPerLine = BubblesPerLine[currentArea];
            int newLength = 0;
            currentLine = 1;
            for (int i = 0; i < amoutOfQuestions; i++)
            {
                newLength += Areas[currentArea].bubblesPerLine * (Areas[currentArea].subLinesAmount + 1);
                currentLine++;
                if (currentLine - linesCount > LinesPerArea[currentArea])
                {
                    linesCount += LinesPerArea[currentArea];
                    currentArea++;
                    if (Areas.Length > currentArea)
                    {
                        bubblesPerLine = Areas[currentArea].bubblesPerLine;
                    }
                }
            }
            for (int i = amoutOfQuestions; i < LabelList.Count; i++)
            {
                Controls.Remove(LabelList[i]);
            }
            for (int j = newLength; j < CheckBoxList.Count; j++)
            {
                Controls.Remove(CheckBoxList[j]);
                if (DataSource.Count > newLength)
                    DataSource.RemoveAt(newLength);
            }
            CheckBoxList.RemoveRange(newLength, CheckBoxList.Count - newLength);
            LabelList.RemoveRange(amoutOfQuestions, LabelList.Count - amoutOfQuestions);
            this.Size = new Size(Width, CheckBoxList.Last().Location.Y + CheckBoxList.Last().Width + 4);
            return;
        }
        //-------------------------------------------------------------------------
        private void IncreaseAmoutOfQuestions()
        {
            //Clear();
            //CreateNewBubblesArea();
        }
        //-------------------------------------------------------------------------
        public void Clear()
        {
            //Visible = false;
            Size = new System.Drawing.Size(1, 1);
            Controls.Clear();
            CheckBoxList.Clear();
            LabelList.Clear();
            Areas = null;
            LinesPerArea = null;
            BubblesPerLine = null;
            maxAmoutOfQuestions = 0;
            AmoutOfQuestions = 0;
            IndexOfFirstQuestion = 1;
            //Visible = true;
        }
        //-------------------------------------------------------------------------
        #region Events
        void checkBox1_Click(object sender, EventArgs e)
        {
            if ((ModifierKeys == Keys.Control))
            {
                CheckBox cb = (CheckBox)sender;
                cb.Checked = true;
                BubbleItem value = (BubbleItem)cb.Tag;
                foreach (var itm in CheckBoxList)
                {
                    BubbleItem bi = (BubbleItem)itm.Tag;
                    if (bi.Bubble.areaNumber != value.Bubble.areaNumber
                        || bi.Bubble.point.Y != value.Bubble.point.Y
                        || bi.Equals(value))
                        continue;
                    itm.Checked = false;
                    EditorForm ef = (EditorForm)this.FindForm();
                    ef.DrawBubble(bi.CheckedBubble);
                }
            }
        }
        //-------------------------------------------------------------------------
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            NotifyUpdated(BubbleCheckedChanged, sender, e);
        }
        //-------------------------------------------------------------------------
        private void checkBox1_MouseEnter(object sender, EventArgs e)
        {
            NotifyUpdated(BubbleMouseEnter, sender, e);
        }
        #endregion
        //-------------------------------------------------------------------------
        #region EventHandler
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public event EventHandler BubbleMouseEnter;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public event EventHandler BubbleCheckedChanged;

        protected void NotifyUpdated(EventHandler handler, object obj, EventArgs e)
        {
            if (handler != null) handler(obj, e);
        }
        #endregion
    }
}
