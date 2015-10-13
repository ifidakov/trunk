using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace eDoctrinaOcrEd
{
    public partial class SplitButton : Button
    {
        public SplitButton()
        {
            InitializeComponent();
            //DrawTriangle(Color.Black);
        }
        //-------------------------------------------------------------------------
        private void SplitButton_Paint(object sender, PaintEventArgs e)
        {
            //DrawTriangle(Color.Black);
        }
        //-------------------------------------------------------------------------
        private void DrawTriangle(Color color)
        {
            Point point1 = new Point(this.Size.Width - 22, 13);
            Point point2 = new Point(this.Size.Width - 14, 22);
            Point point3 = new Point(this.Size.Width - 5, 13);
            Point[] curvePoints = { point1, point2, point3 };

            Graphics g = this.CreateGraphics();
            g.FillPolygon(new SolidBrush(color), curvePoints);
            g.Dispose();
        }
        //-------------------------------------------------------------------------
        private void SplitButton_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.X >= this.Size.Width - 21)
                {
                    DrawTriangle(Color.DodgerBlue);
                }
                else
                {
                    //if (contextMenuStrip1.Visible)
                    //contextMenuStrip1.Close();
                    DrawTriangle(Color.Black);
                }
            }
            catch (Exception)
            {
            }
        }
        //-------------------------------------------------------------------------
        private void contextMenuStrip1_MouseEnter(object sender, EventArgs e)
        {
            DrawTriangle(Color.DodgerBlue);
        }
        //-------------------------------------------------------------------------
        private void SplitButton_MouseLeave(object sender, EventArgs e)
        {
            Refresh();
        }
        //-------------------------------------------------------------------------
        private void contextMenuStrip1_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            Refresh();// DrawTriangle(Color.Black);
        }
        //-------------------------------------------------------------------------
        private void contextMenuStrip1_Click(object sender, EventArgs e)
        {
            EditorForm ef = (EditorForm)this.FindForm();
            ef.RestoreFromSourceFile();
        }
        //-------------------------------------------------------------------------
        private void SplitButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.X >= this.Size.Width - 21)
            {
                Point point = this.PointToScreen(Point.Empty);
                Point pt = new Point(this.Size.Width - 13, this.Size.Height);
                if (!contextMenuStrip1.Visible)
                {
                    contextMenuStrip1.Show(point.X + pt.X
                        - contextMenuStrip1.Width / 2, point.Y + pt.Y);
                    contextMenuStrip1.Items[0].Select();
                }
                DrawTriangle(Color.DodgerBlue);
            }
            else
            {
                contextMenuStrip1.Close();
                DrawTriangle(Color.Black);
                //if (contextMenuStrip1.Visible)
            }

        }
        //-------------------------------------------------------------------------

    }
}
