using System;
using System.Drawing;
using System.Windows.Forms;
using eDoctrinaUtils;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime;
using System.Runtime.CompilerServices;

using System.Collections.ObjectModel;
namespace eDoctrinaOcrEd
{
    public partial class BubblesAreaControl : UserControl
    {
        public BubblesAreaControl()
        {
            InitializeComponent();
        }
        private int indexOfFirstLine = 1;
        //-------------------------------------------------------------------------
        public int posY { get; set; }
        public int bubblesPerLine { get; set; }
        //public CheckBoxExt[] CheckBoxArr = new CheckBoxExt[0];
        public CheckBox[] CheckBoxArr = new CheckBox[0];
        public Label[] labelArr = new Label[0];
        public bool lockReport = false;
        public RegionsArea[] areas;
        //-------------------------------------------------------------------------
        //private int[] questionNumbers = new int[0];
        //public int[] QuestionNumbers
        //{
        //    get
        //    {
        //        return questionNumbers;
        //    }
        //    set
        //    {
        //        for (int i = 0; i < labelArr.Length; i++)
        //        {
        //            if (i > AmoutOfQuestions)
        //                break;

        //        }
        //    }
        //}
        //-------------------------------------------------------------------------
        public int bubbleCheckedInvert
        {
            get
            {
                return CheckBoxArr.Length;
            }
            set
            {
                lockReport = true;
                CheckBoxArr[value].Checked = !CheckBoxArr[value].Checked;
            }
        }
        //-------------------------------------------------------------------------
        public int indexOfFirstQuestion
        {
            get
            {
                return indexOfFirstLine;
            }
            set
            {
                indexOfFirstLine = value;
                if (labelArr.Length > 0)
                {
                    int index = indexOfFirstLine;
                    for (int i = 0; i < labelArr.Length; i++)
                    {
                        if (EditorForm.rec != null && EditorForm.rec.questionNumbers.Length > 0
                            && EditorForm.rec.questionNumbers.Length > i)
                        {
                            labelArr[i].Text = EditorForm.rec.questionNumbers[i].ToString();
                        }
                        else
                        {
                            labelArr[i].Text = index.ToString();
                        }
                        index++;
                    }
                }
            }
        }
        //-------------------------------------------------------------------------
        // public Regions.Area[] Areas
        public RegionsArea[] Areas
        {
            //get
            //{
            //    return areas;
            //}
            set
            {
                areas = value;
            }
        }
        //-------------------------------------------------------------------------
        //private int amoutOfQuestions = -1;

        //-------------------------------------------------------------------------
        public int AmoutOfQuestions
        {
            //get
            //{
            //    return amoutOfQuestions;
            //}
            set
            {
                //DateTime dt = DateTime.Now;
                //AmoutOfQuestions = value;
                CheckBox cb;
                //EditorForm f1 = (EditorForm)FindForm();//exc
                if (value > EditorForm.maxAmoutOfQuestions || value == labelArr.Length)
                    return;
                int currentArea = 0;
                int linesPerArea = EditorForm.linesPerArea[currentArea];
                int linesCount = 0;
                int currentLine = indexOfFirstLine;
                int bubblesPerLine = EditorForm.bubblesPerLine[currentArea];
                //RecognitionTools.Bubble bubble = new RecognitionTools.Bubble();
                Bubble bubble = new Bubble();
                if (CheckBoxArr.Length > 0)
                {
                    int newLength = 0;
                    currentLine = 1;
                    for (int i = 0; i < value; i++)
                    {
                        newLength += areas[currentArea].bubblesPerLine * (areas[currentArea].subLinesAmount + 1);
                        currentLine++;
                        if (currentLine - linesCount > EditorForm.linesPerArea[currentArea])
                        {
                            linesCount += EditorForm.linesPerArea[currentArea];
                            currentArea++;
                            if (areas.Length > currentArea)
                                bubblesPerLine = areas[currentArea].bubblesPerLine;
                        }
                    }
                    if (value < labelArr.Length)
                    {
                        for (int i = value; i < labelArr.Length; i++)
                            Controls.Remove(labelArr[i]);
                        for (int j = newLength; j < CheckBoxArr.Length; j++)
                            Controls.Remove(CheckBoxArr[j]);
                        Array.Resize(ref CheckBoxArr, newLength);
                        Array.Resize(ref labelArr, value);
                        this.Size = new Size(Width, CheckBoxArr[CheckBoxArr.Length - 1].Location.Y
                            + CheckBoxArr[CheckBoxArr.Length - 1].Width + 4);
                        return;
                    }
                    else
                    {
                        EditorForm vef = (EditorForm)this.FindForm();//(EditorForm)Application.OpenForms["EditorForm"];// 
                        if (vef == null)
                            return;
                        vef.bac = vef.CreateNewBubblesAreaControl
                            (
                              areas
                            , value
                            );
                        vef.SetCheckBoxes();
                        return;
                    }
                }
                else
                {
                    bubble.areaNumber = currentArea;
                    bubble.subLine = 0;
                    bubble.point = new Point(0, currentLine);
                    //bubble.index = CheckBoxArr.Length;

                    checkBox1.Tag = bubble;
                    lblLineNumber.Text = currentLine.ToString();
                    Array.Resize(ref CheckBoxArr, CheckBoxArr.Length + 1);
                    CheckBoxArr[0] = checkBox1;
                    Array.Resize(ref labelArr, labelArr.Length + 1);
                    labelArr[0] = lblLineNumber;
                    if (EditorForm.rec != null && EditorForm.rec.questionNumbers.Length > 0)
                    {
                        labelArr[0].Text = EditorForm.rec.questionNumbers[0].ToString();
                    }
                }
                linesCount = 0;// EditorForm.linesPerArea[currentArea];
                if (areas.Length != EditorForm.linesPerArea.Length)
                {
                    EditorForm ef = (EditorForm)Application.OpenForms["EditorForm"];
                    ef.GetAreasSettings();
                    areas = (RegionsArea[])EditorForm.rec.areas.Clone();
                }
                bubblesPerLine = areas[currentArea].bubblesPerLine;
                currentLine--;
                for (int i = 1; i <= value; i++)
                {
                    if (i - linesCount > EditorForm.linesPerArea[currentArea])
                    {
                        linesCount += EditorForm.linesPerArea[currentArea];
                        currentArea++;
                        if (areas.Length > currentArea)
                            bubblesPerLine = areas[currentArea].bubblesPerLine;
                    }
                    currentLine++;
                    if (i > 1)
                    {
                        cb = new CheckBox();
                        cb.UseVisualStyleBackColor = true;//!!!
                        cb.TabStop = false;
                        bubble.areaNumber = currentArea;
                        bubble.subLine = 0;
                        bubble.point = new Point(0, currentLine);
                        //bubble.index = CheckBoxArr.Length;

                        cb.Tag = bubble;
                        cb.CheckedChanged += new EventHandler(checkBox1_CheckedChanged);
                        cb.MouseEnter += new EventHandler(checkBox1_MouseEnter);
                        cb.Size = checkBox1.Size;
                        cb.Location = new Point
                            (
                              checkBox1.Location.X
                            , CheckBoxArr[CheckBoxArr.Length - 1].Location.Y + checkBox1.Height + 4
                            );
                        this.Controls.Add(cb);
                        Label lbl = new Label();
                        lbl.AutoSize = true;
                        lbl.Text = currentLine.ToString();
                        this.Controls.Add(lbl);
                        lbl.Location = new Point
                            (
                              lblLineNumber.Location.X
                            , CheckBoxArr[CheckBoxArr.Length - 1].Location.Y + checkBox1.Height + 4
                            );
                        Array.Resize(ref CheckBoxArr, CheckBoxArr.Length + 1);
                        CheckBoxArr[CheckBoxArr.Length - 1] = cb;
                        Array.Resize(ref labelArr, labelArr.Length + 1);
                        labelArr[labelArr.Length - 1] = lbl;
                        if (EditorForm.rec != null && EditorForm.rec.questionNumbers.Length > 0
                            && EditorForm.rec.questionNumbers.Length >= labelArr.Length)
                        {
                            labelArr[labelArr.Length - 1].Text = EditorForm.rec.questionNumbers[labelArr.Length - 1].ToString();
                        }

                    }
                    for (int k = 0; k <= areas[currentArea].subLinesAmount; k++)
                    {
                        if (k > 0)
                        {
                            cb = new CheckBox();
                            cb.TabStop = false;
                            bubble.areaNumber = currentArea;
                            bubble.subLine = k;
                            bubble.point = new Point(0, currentLine);
                            //bubble.index = CheckBoxArr.Length;

                            cb.Tag = bubble;
                            cb.CheckedChanged += new EventHandler(checkBox1_CheckedChanged);
                            cb.MouseEnter += new EventHandler(checkBox1_MouseEnter);
                            cb.Size = checkBox1.Size;
                            cb.Location = new Point
                                (
                                  checkBox1.Location.X
                                , CheckBoxArr[CheckBoxArr.Length - 1].Location.Y + checkBox1.Height + 4
                                );
                            this.Controls.Add(cb);
                            Array.Resize(ref CheckBoxArr, CheckBoxArr.Length + 1);
                            CheckBoxArr[CheckBoxArr.Length - 1] = cb;
                        }
                        for (int j = 1; j < bubblesPerLine; j++)
                        {
                            cb = new CheckBox();
                            cb.TabStop = false;
                            bubble.areaNumber = currentArea;
                            bubble.subLine = k;
                            bubble.point = new Point(j, currentLine);
                            //bubble.index = CheckBoxArr.Length;
                            cb.Tag = bubble;
                            cb.CheckedChanged += new EventHandler(checkBox1_CheckedChanged);
                            cb.MouseEnter += new EventHandler(checkBox1_MouseEnter);
                            cb.Size = checkBox1.Size;
                            this.Controls.Add(cb);
                            int step = 0;
                            if (j % 5 == 0)
                            {
                                step = 4;
                            }
                            else
                            {
                                step = 0;
                            }
                            cb.Location = new Point
                                (
                                  CheckBoxArr[CheckBoxArr.Length - 1].Location.X + CheckBoxArr[CheckBoxArr.Length - 1].Width + step
                                , CheckBoxArr[CheckBoxArr.Length - 1].Location.Y
                                );
                            Array.Resize(ref CheckBoxArr, CheckBoxArr.Length + 1);
                            CheckBoxArr[CheckBoxArr.Length - 1] = cb;
                        }
                    }
                }
                this.Size = new Size
                     (
                       Width
                     , CheckBoxArr[CheckBoxArr.Length - 1].Location.Y + CheckBoxArr[CheckBoxArr.Length - 1].Width + 4
                     );
                //TimeSpan ts = DateTime.Now - dt;
            }
        }
        //-------------------------------------------------------------------------      
        public void SetInvertBubbleChecked(BubbleItem value, bool controlPressed = false)
        {
            EditorForm ef = (EditorForm)this.FindForm();
            var item = FindCheckBox(value);
            if (item != null)
            {
                if (!controlPressed)
                {
                    lockReport = true;
                    item.Checked = !item.Checked;
                    ef.DrawBubble(value.CheckedBubble);
                }
                else
                {
                    lockReport = true;
                    item.Checked = true;
                    value.CheckedBubble.isChecked = true;
                    ef.DrawBubble(value.CheckedBubble);
                    for (int i = 0; i < CheckBoxArr.Length; i++)
                    {
                        var itm = CheckBoxArr[i];
                        Bubble bi = (Bubble)itm.Tag;
                        if (bi.areaNumber != value.Bubble.areaNumber
                            || bi.point.Y != value.Bubble.point.Y
                            || bi.Equals(value.Bubble))
                            continue;
                        itm.Checked = false;
                        EditorForm.rec.BubbleItems[i].CheckedBubble.isChecked = false;
                        ef.DrawBubble(EditorForm.rec.BubbleItems[i].CheckedBubble);
                    }
                    lockReport = false;
                }
            }
            ef.pictureBox1.Refresh();
        }
        //-------------------------------------------------------------------------
        public void Select(BubbleItem item)
        {
            var temp = FindCheckBox(item);
            if (temp != null)
                checkBox1_MouseEnter(temp, null);
        }
        //-------------------------------------------------------------------------
        private CheckBox FindCheckBox(BubbleItem value)
        {
            EditorForm ef = (EditorForm)this.FindForm();
            if (ef != null && EditorForm.rec != null && EditorForm.rec.BubbleItems != null)
            {
                int index = EditorForm.rec.BubbleItems.IndexOf(value);
                if (index > -1)
                    return CheckBoxArr[index];
            }
            return null;
        }
        //-------------------------------------------------------------------------
        public void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (lockReport)//
                return;
            EditorForm f1 = (EditorForm)this.FindForm();
            Bubble itm = (Bubble)(sender as CheckBox).Tag;
            //f1.SelectedBubble = itm;
            //var BubbleItemsDict = f1.BubbleItems.ToDictionary(x => x.Bubble);
            //BubbleItem item = f1.BubbleItems[itm.index];
            if (ModifierKeys == Keys.Control)
            {
                lockReport = true;
                CheckBox cb = (CheckBox)sender;
                cb.Checked = true;
                for (int i = 0; i < CheckBoxArr.Length; i++)
                {
                    var checkBox = CheckBoxArr[i];
                    if (checkBox == cb)
                    {
                        EditorForm.rec.BubbleItems[i].CheckedBubble.isChecked = true;
                        continue;
                    }
                    Bubble bi = (Bubble)checkBox.Tag;
                    if (bi.areaNumber != itm.areaNumber
                        || bi.point.Y != itm.point.Y
                        || bi.Equals(itm))
                        continue;
                    checkBox.Checked = false;
                    EditorForm.rec.BubbleItems[i].CheckedBubble.isChecked = false;
                    f1.DrawBubble(EditorForm.rec.BubbleItems[i].CheckedBubble);
                }
                lockReport = false;
            }
            //SetInvertBubbleChecked(item as BubbleItem, true);
            else
            {
                CheckBox cb = (CheckBox)sender;
                //if (lockReport)
                //{
                //    if (Enabled)
                //        f1.pnlBubbles.ScrollControlIntoView(cb);
                //    lockReport = false;
                //    return;
                //}
                Bubble bubble = (Bubble)cb.Tag;
                f1.SelectedBubble = bubble;
                f1.InvertSelectedBubble(bubble);
            }
        }
        //-------------------------------------------------------------------------
        private void checkBox1_MouseEnter(object sender, EventArgs e)
        {
            EditorForm f1 = (EditorForm)this.FindForm();
            CheckBox cb = (CheckBox)sender;
            int index = Array.IndexOf(CheckBoxArr, cb);
            if (lockReport)
            {
                f1.pnlBubbles.ScrollControlIntoView(cb);
                lockReport = false;
                return;
            }
            //Bubble bubble = (Bubble)cb.Tag;
            lockReport = true;
            f1.MouseEnterBubble(index);
        }
        //-------------------------------------------------------------------------
    }
}
