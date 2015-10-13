using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace eDoctrinaUtils
{
    public class AnimatedTimer
    {
        //-------------------------------------------------------------------------
        public AnimatedTimer()
        {
            InitTimer();
        }

        public int Interval
        {
            get { return AnimationTimer.Interval; }
            set { AnimationTimer.Interval = value; }
        }
        //-------------------------------------------------------------------------
        private byte borderColorOpacity = 255;
        public byte BorderColorOpacity
        {
            get
            {
                return borderColorOpacity;
            }
            set
            {
                if (borderColorOpacity != value)
                {
                    borderColorOpacity = value;
                }
            }
        }
        //-------------------------------------------------------------------------
        private Bitmap activeBarCodeBitmap;
        public Bitmap ActiveBarCodeBitmap
        {
            get
            {
                return activeBarCodeBitmap;
            }
            private set
            {
                //  if (bitmap != value)
                {
                    activeBarCodeBitmap = value;
                }
            }
        }
        //-------------------------------------------------------------------------
        private BarCodeItem activeBarCode;
        public BarCodeItem ActiveBarCode
        {
            get
            {
                return activeBarCode;
            }
            private set
            {
                if (activeBarCode != value)
                {
                    StopAnimation();
                    activeBarCode = value;
                    if (value != null) ActiveBubbleItem = null;
                    StartAnimation();
                }
            }
        }
        //-------------------------------------------------------------------------
        private BubbleItem activeBubbleItem;
        public BubbleItem ActiveBubbleItem
        {
            get
            {
                return activeBubbleItem;
            }
            private set
            {
                if (activeBubbleItem != value)
                {
                    StopAnimation();
                    activeBubbleItem = value;
                    if (value != null) ActiveBarCode = null;
                    StartAnimation();
                }
            }
        }
        //-------------------------------------------------------------------------
        public void SetActiveValue(Bitmap bitmap, BarCodeItem item)
        {
            this.ActiveBarCode = item;
            this.ActiveBarCodeBitmap = CopyBitmap(bitmap, item.Rectangle);
        }

        public void SetActiveValue(BubbleItem item)
        {
            this.ActiveBubbleItem = item;
        }
        //-------------------------------------------------------------------------
        public object FindActiveRectangle(MouseEventArgs e, double Zoom, List<BarCodeItem> BarCodeItems, List<BubbleItem> BubbleItems)
        {
            int x = (int)Math.Round(e.X / Zoom);
            int y = (int)Math.Round(e.Y / Zoom);
            if (BarCodeItems != null)
                foreach (var item in BarCodeItems)
                {
                    if ((x > item.Rectangle.Left && x < item.Rectangle.Right) && (y > item.Rectangle.Top && y < item.Rectangle.Bottom))
                    {
                        return item;
                    }
                }
            if (BubbleItems != null)
                foreach (var item in BubbleItems)
                {
                    if ((x > item.CheckedBubble.rectangle.Left && x < item.CheckedBubble.rectangle.Right) && (y > item.CheckedBubble.rectangle.Top && y < item.CheckedBubble.rectangle.Bottom))
                    {
                        return item;
                    }
                }
            return null;
        }
        //-------------------------------------------------------------------------
        public void Clear()
        {
            StopAnimation();
            this.ActiveBubbleItem = null;
            this.ActiveBarCode = null;
            this.ActiveBarCodeBitmap.Dispose();
            this.ActiveBarCodeBitmap = null;
            InitTimer();
        }
        //-------------------------------------------------------------------------
        #region Timer
        //-------------------------------------------------------------------------
        public void StartAnimation()
        {
            if (AnimationTimer != null) AnimationTimer.Start();
        }
        //-------------------------------------------------------------------------
        public void StopAnimation()
        {
            if (AnimationTimer != null)
            {
                AnimationTimer.Stop();
                if (ActiveBarCode != null) ActiveBarCode.BorderColorOpacity = 255;
                if (ActiveBubbleItem != null) ActiveBubbleItem.BorderColorOpacity = 255;
            }
        }
        //-------------------------------------------------------------------------
        private Timer AnimationTimer;

        private void InitTimer()
        {
            AnimationTimer = new Timer();
            AnimationTimer.Interval = 500;
            AnimationTimer.Tick += AnimationTimer_Tick;
        }
        //-------------------------------------------------------------------------
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (ActiveBarCode != null)
            {
                if (BorderColorOpacity != 0)//ActiveBarCode.BorderColorOpacity
                {
                    ActiveBarCode.BorderColorOpacity = 0;//анимация бордера 
                    BorderColorOpacity = 0;
                }
                else
                {
                    ActiveBarCode.BorderColorOpacity = 255;//анимация бордера 
                    BorderColorOpacity = 255;
                }
                NotifyUpdated(Tick, this, e);
            }
            if (ActiveBubbleItem != null)
            {
                if (ActiveBubbleItem.BorderColorOpacity != 0)
                {
                    ActiveBubbleItem.BorderColorOpacity = 0;
                    BorderColorOpacity = 0;
                }
                else
                {
                    ActiveBubbleItem.BorderColorOpacity = 255;
                    BorderColorOpacity = 255;
                }
                NotifyUpdated(Tick, this, e);
            }
        }
        //-------------------------------------------------------------------------
        public event EventHandler Tick;
        private void NotifyUpdated(EventHandler handler, object obj, EventArgs e)
        {
            if (handler != null) handler(obj, e);
        }
        #endregion
        //-------------------------------------------------------------------------
        private Bitmap CopyBitmap(Bitmap bitmap, Rectangle newRectangle)
        {
            if (newRectangle.X < 0)
            {
                newRectangle.X = 0;
            }
            else
                if (newRectangle.X >= bitmap.Width)
                {
                    newRectangle.X = bitmap.Width - newRectangle.Width;
                }
            if (newRectangle.Y < 0)
            {
                newRectangle.Y = 0;
            }
            else
                if (newRectangle.Y >= bitmap.Height)
                {
                    newRectangle.Y = bitmap.Height - newRectangle.Height;
                }
            if (newRectangle.Right > bitmap.Width)
            {
                newRectangle = new Rectangle(newRectangle.X, newRectangle.Y, bitmap.Width - newRectangle.X, newRectangle.Height);
            }
            if (newRectangle.Bottom > bitmap.Height)
            {
                newRectangle = new Rectangle(newRectangle.X, newRectangle.Y, newRectangle.Width, bitmap.Height - newRectangle.Y);
            }
            // Вырезаем выбранный кусок картинки
            Bitmap bmp = new Bitmap(newRectangle.Width, newRectangle.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.DrawImage(bitmap, 0, 0, newRectangle, GraphicsUnit.Pixel);
            }//Возвращаем кусок картинки.
            return bmp;
        }
    }
    //-------------------------------------------------------------------------
    //-------------------------------------------------------------------------
    //-------------------------------------------------------------------------
    //-------------------------------------------------------------------------
    //-------------------------------------------------------------------------
}