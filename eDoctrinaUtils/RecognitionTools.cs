using BitMiracle.LibTiff.Classic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using ZXing;
using ZXing.Common;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

using Emgu.CV.CvEnum;
using Emgu.CV;
using Emgu.CV.Structure;

//using BarcodeLib.BarcodeReader;
namespace eDoctrinaUtils
{// ^[^\/]+\.Save\("
    public class RecognitionTools
    {
        //-------------------------------------------------------------------------
        private System.Threading.CancellationToken token;
        public void SetCancellationToken(System.Threading.CancellationToken token)
        {
            this.token = token;
        }
        //-------------------------------------------------------------------------
        public Result result1 = null;
        public string frameFileName;
        private string[] symbols = new string[]
        { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"
         ,"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O"
         ,"P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "a", "b", "c", "d"
         , "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s"
         , "t", "u", "v", "w", "x", "y", "z"};
        private string[] symbolsBig = new string[]
        { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"
         ,"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O"
         ,"P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};

        private string[] recognNumb = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "I" };
        //private string[] recognNumbAndBigLat = new string[] 
        //{ "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" 
        // ,"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O" 
        // ,"P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};
        //private string[] recognNumbAndBigLat = new string[] 
        //{ "0", "1","A", "B", "E", "F", "G","I", "K", "L", "M", "N", "O" 
        // ,"P",  "S", "T"};
        private string[] recognNumbAndBigLat = new string[] { "0", "1", "A", "B", "F", "G", "I", "M", "P", "S" };
        private int limSymbols = 3;
        private const int VerticalBarcodeBorder = 20;
        private const int contourMaxLength = 2000;
        Log log = new Log();
        //double? percent_confident_text_region;

        //-------------------------------------------------------------------------
        private int MarkersFind
             (
               Bitmap bmpEntry
             , ref Rectangle markerLT
             , ref Rectangle markerRT
             , ref Rectangle markerLB
             , ref Rectangle markerRB
             , Rectangle markerLTet
             )
        {
            double d = (double)bmpEntry.Width / bmpEntry.Height;
            if (d < .5 || d > 2)
                return 0;
            int maxNum = -1;
            int factor = 4;
            markerLT = Rectangle.Empty;
            markerRT = Rectangle.Empty;
            markerLB = Rectangle.Empty;
            markerRB = Rectangle.Empty;

            //DateTime dt = DateTime.Now;
            //bmpEntry = (Bitmap)bmpEntry.GetThumbnailImage(bmpEntry.Width, bmpEntry.Height, null, IntPtr.Zero);
            Bitmap bmp = null;
            for (int factorIter = 0; factorIter < 2; factorIter++)//< 2 <- долго ищет, но находит на зашумлённых сканах
            {
                if (factorIter > 0)
                    factor = 1;
                if (factor == 1)
                {
                    bmp = (Bitmap)bmpEntry.Clone();
                    bmp = ConvertTo1Bit(ref bmp);
                }
                else
                {
                    bmp = new Bitmap(bmpEntry.Width / factor, bmpEntry.Height / factor, PixelFormat.Format24bppRgb);
                    try
                    {
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.InterpolationMode = InterpolationMode.HighQualityBilinear;// ;HighQualityBicubic
                            //g.CompositingQuality = CompositingQuality.HighQuality;
                            //g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            //g.SmoothingMode = SmoothingMode.HighQuality;

                            GraphicsUnit unit = GraphicsUnit.Pixel;
                            RectangleF rect = bmp.GetBounds(ref unit);
                            //g.DrawImage(bmpEntry, 0, 0, bmp.Width, bmp.Height);//out of memory
                            g.DrawImage(bmpEntry, rect);
                        }
                        //}
                        //finally
                        //{
                        //    bmp.Dispose();
                        //}
                    }
                    catch (Exception)
                    {
                        continue;
                        //bmp.Dispose();
                        //return 0;
                    }
                }
                //bmp.Save("Marker.bmp", ImageFormat.Bmp);
                //bmp = RaspFilter(bmp, 1, new Rectangle(0, 0, bmp.Width, bmp.Height), true, true);
                //bmp = RaspFilter(bmp, 1, new Rectangle(0, 0, bmp.Width, bmp.Height), true);
                //bmp.Save("Marker2.bmp", ImageFormat.Bmp);
                Color c;
                Color argbWhite = Color.FromArgb(255, 255, 255);
                int x, y;
                int width, height, markerBoundX, markerBoundY;
                markerBoundX = bmp.Width / 8;
                markerBoundY = bmp.Height / 8;
                int caliberWidth = bmp.Width / 80;
                int caliberHeight = bmp.Width / 80;
                if (caliberWidth < 2 || caliberHeight < 2)
                {
                    bmp.Dispose();
                    return 0;
                }
                int startXleft = caliberWidth / 2;
                int startXright = bmp.Width - caliberWidth / 2;
                double brightness = .88;
                //}
                for (int iter = 0; iter < 3; iter++)
                {
                    markerLT = Rectangle.Empty;
                    markerRT = Rectangle.Empty;
                    markerLB = Rectangle.Empty;
                    markerRB = Rectangle.Empty;
                    int white = 0, black = 0;
                    System.Drawing.Point[] curvePoints = new Point[0];
                    //System.Drawing.Point[] curvePointsLT = new Point[0];
                    //System.Drawing.Point[] curvePointsRT = new Point[0];
                    //System.Drawing.Point[] curvePointsLB = new Point[0];
                    //System.Drawing.Point[] curvePointsRB = new Point[0];
                    //try
                    //{
                    switch (iter)
                    {
                        case 1:
                        case 2:
                            bmp = RaspFilter(ref bmp, 1, new Rectangle(0, 0, bmp.Width, bmp.Height), true, true);
                            bmp = RaspFilter(ref bmp, 1, new Rectangle(0, 0, bmp.Width, bmp.Height), true);
                            break;
                            //case 3:
                            //    bmp = RaspFilter(bmp, 1, new Rectangle(0, 0, bmp.Width, bmp.Height), true, true);
                            //    bmp = RaspFilter(bmp, 1, new Rectangle(0, 0, bmp.Width, bmp.Height), true);
                            //    break;

                            //case 2:
                            //    caliberHeight *= 2;
                            //    caliberWidth *= 2;
                            //    break;
                    }

                    for (int k = 0; k < 3; k++)
                    {
                        #region
                        switch (k)
                        {
                            case 0:
                                markerBoundX = bmp.Width / 8;
                                markerBoundY = bmp.Height / 8;
                                break;
                            case 1:
                                markerBoundX = bmp.Width / 4;
                                markerBoundY = bmp.Height / 4;
                                break;
                            case 2:
                                markerBoundX = bmp.Width / 3;
                                markerBoundY = bmp.Height / 2;
                                break;
                        }
                        for (y = startXleft; y < markerBoundY; y += caliberWidth)
                        {
                            for (x = startXleft; x < markerBoundX; x += caliberWidth)
                            {
                                if (token.IsCancellationRequested)
                                {
                                    bmp.Dispose();
                                    return 0;
                                }
                                c = bmp.GetPixel(x, y);
                                if (c.GetBrightness() >= brightness)//(c == argbWhite)
                                    continue;
                                curvePoints = ContourFindSpeed(ref bmp, x, y, 0, false, true);
                                if (curvePoints.Length == contourMaxLength)
                                {
                                    x += startXleft;
                                    continue;
                                }
                                Rectangle r = GetRectangle(curvePoints);
                                width = r.Width; height = r.Height;
                                //if ((r.X < caliberWidth && r.Y < caliberHeight)
                                //    && width > markerBoundX || height > markerBoundY)
                                //{//засветка левый верхний
                                //    if ((width > (bmp.Width - bmp.Width / 8))
                                //        || (height > (bmp.Height - bmp.Height / 8)))
                                //    {
                                //        using (GraphicsPath gp = new GraphicsPath())
                                //        {
                                //            gp.AddPolygon(curvePoints);
                                //            RectangleF bounds = gp.GetBounds();
                                //            if (gp.IsVisible(bmp.Width - bmp.Width / 8, bmp.Height - bmp.Height / 8)
                                //                || gp.IsVisible(bmp.Width / 8, bmp.Height - bmp.Height / 8)
                                //                || gp.IsVisible(bmp.Width - bmp.Width / 8, bmp.Height / 8)
                                //                || gp.IsVisible(bmp.Width / 8, bmp.Height / 8)
                                //                )
                                //            {
                                //                x++;//= 10;
                                //                continue;
                                //            }
                                //        }
                                //        using (Graphics g = Graphics.FromImage(bmp))
                                //        {
                                //            g.FillPolygon(new SolidBrush(argbWhite), curvePoints);
                                //            g.DrawPolygon(new Pen(argbWhite), curvePoints);//Color.Red//
                                //        }
                                //        //bmp.Save("curvePoints.bmp", ImageFormat.Bmp);
                                //        continue;
                                //    }
                                //    AnchorPoints anchorPoints = GetAnchorPoints(curvePoints, r);
                                //    if (anchorPoints.LeftMinValue.X == 0
                                //        || anchorPoints.LeftBottom.X == 0
                                //        || anchorPoints.LeftTop.X == 0)
                                //    {
                                //        using (Graphics g = Graphics.FromImage(bmp))
                                //        {
                                //            g.FillPolygon(new SolidBrush(argbWhite), curvePoints);
                                //            g.DrawPolygon(new Pen(argbWhite), curvePoints);
                                //        }
                                //        continue;
                                //    }
                                //    x += startXleft;
                                //}
                                double prop;
                                if (width == 0 || height == 0)
                                {
                                    continue;
                                }
                                if (width > height)
                                {
                                    prop = width / height;
                                }
                                else
                                {
                                    prop = height / width;
                                }
                                //using (Graphics g = Graphics.FromImage(bmp))
                                //{
                                //    g.DrawRectangle(new Pen(Color.Blue), r);
                                //}
                                //bmp.Save("Rectangle.bmp", ImageFormat.Bmp);

                                if (prop > 2.5)
                                {
                                    //x += 10;
                                    continue;
                                }
                                if (width > caliberWidth && height > caliberHeight)
                                {
                                    //using (GraphicsPath gp = new GraphicsPath())
                                    //{
                                    //    gp.AddPolygon(curvePointsLT);
                                    for (int i = r.X + r.Width / 8; i < r.Right - r.Width / 8; i += 2)
                                    {
                                        for (int j = r.Y + r.Height / 8; j < r.Bottom - r.Height / 8; j += 2)
                                        {
                                            //if (!gp.IsVisible(i, j))
                                            //    continue;
                                            c = bmp.GetPixel(i, j);
                                            //brightness = c.GetBrightness();
                                            //if (brightness > .5)
                                            //{
                                            //    white++;
                                            //}
                                            if (c.GetBrightness() >= brightness)//(c == argbWhite)
                                            {
                                                white++;
                                            }
                                            else
                                            {
                                                black++;
                                            }
                                        }
                                    }
                                    //}
                                }
                                else
                                {
                                    x += startXleft;
                                    continue;
                                }

                                //if (black / 2 > white)

                                //curvePointsLT = ResectAppendix(curvePointsLT);
                                //markerLT = GetRectangle(curvePointsLT);
                                if (black / 2 > white)
                                //if (white == 0)
                                {
                                    //using (Graphics g = Graphics.FromImage(bmp))
                                    //{
                                    //    g.DrawRectangle(new Pen(Color.Red), r);
                                    //}
                                    //bmp.Save("Rectangle.bmp", ImageFormat.Bmp);
                                    markerLT = r;
                                    goto nextStep;
                                }
                                black = 0; white = 0;
                                x += startXleft;
                            }
                        }
                        if (markerLT != new Rectangle())
                        {
                            break;
                        }
                        #endregion
                    }
                    nextStep:
                    black = 0; white = 0;
                    for (int k = 0; k < 3; k++)
                    {
                        #region
                        switch (k)
                        {
                            case 0:
                                markerBoundX = bmp.Width / 8;
                                markerBoundY = bmp.Height / 8;
                                break;
                            case 1:
                                markerBoundX = bmp.Width / 4;
                                markerBoundY = bmp.Height / 4;
                                break;
                            case 2:
                                markerBoundX = bmp.Width / 3;
                                markerBoundY = bmp.Height / 2;
                                break;
                        }
                        for (x = startXright; x > bmp.Width - markerBoundX; x -= caliberWidth)//справа на лево
                        {
                            for (y = startXleft; y < markerBoundY; y += caliberWidth)//сверху вниз bmp.Height / 8 
                            {
                                if (token.IsCancellationRequested)
                                {
                                    bmp.Dispose();
                                    return 0;
                                }
                                c = bmp.GetPixel(x, y);
                                if (c.GetBrightness() >= brightness)//(c == argbWhite)
                                    continue;
                                curvePoints = ContourFindSpeed(ref bmp, x, y, 1, false, true);
                                if (curvePoints.Length == contourMaxLength)
                                {
                                    x -= startXleft;
                                    continue;
                                }

                                Rectangle r = GetRectangle(curvePoints);
                                width = r.Width; height = r.Height;
                                //if ((r.X < caliberWidth && r.Y < bmp.Width - caliberHeight)
                                //    && width > markerBoundX || height > markerBoundY)
                                //{//засветка правый верхний
                                //    if ((width > (bmp.Width - bmp.Width / 8))
                                //        || (height > (bmp.Height - bmp.Height / 8)))
                                //    {
                                //        using (GraphicsPath gp = new GraphicsPath())
                                //        {
                                //            gp.AddPolygon(curvePoints);
                                //            RectangleF bounds = gp.GetBounds();
                                //            if (gp.IsVisible(bmp.Width - bmp.Width / 8, bmp.Height - bmp.Height / 8)
                                //                || gp.IsVisible(bmp.Width / 8, bmp.Height - bmp.Height / 8)
                                //                || gp.IsVisible(bmp.Width - bmp.Width / 8, bmp.Height / 8)
                                //                || gp.IsVisible(bmp.Width / 8, bmp.Height / 8)
                                //                )
                                //            {
                                //                x--;//= 10;
                                //                continue;
                                //            }
                                //        }
                                //        //using (Graphics g = Graphics.FromImage(bmp))
                                //        //{
                                //        //    g.DrawPolygon(new Pen(Color.Red), curvePoints);
                                //        //}
                                //        //bmp.Save("curvePoints.bmp", ImageFormat.Bmp);

                                //        using (Graphics g = Graphics.FromImage(bmp))
                                //        {
                                //            g.FillPolygon(new SolidBrush(argbWhite), curvePoints);
                                //            g.DrawPolygon(new Pen(argbWhite), curvePoints);
                                //        }
                                //        continue;
                                //    }
                                //    AnchorPoints anchorPoints = GetAnchorPoints(curvePoints, r);
                                //    if (anchorPoints.RightBottom.X == bmp.Width - 1
                                //        || anchorPoints.RightBottom.X == bmp.Width - 1
                                //        || anchorPoints.RightTop.Y == 0
                                //        || anchorPoints.RightBottom.Y == bmp.Height - 1)
                                //    {
                                //        using (Graphics g = Graphics.FromImage(bmp))
                                //        {
                                //            g.FillPolygon(new SolidBrush(argbWhite), curvePoints);
                                //            g.DrawPolygon(new Pen(argbWhite), curvePoints);
                                //        }
                                //        continue;
                                //    }

                                //    x -= startXleft;//= 10;
                                //    continue;
                                //}
                                if (width == 0 || height == 0)
                                    continue;
                                double prop;
                                if (width > height)
                                    prop = width / height;
                                else
                                    prop = height / width;
                                if (prop > 2.5)
                                {
                                    x -= startXleft;
                                    continue;
                                }
                                if (width > caliberWidth && height > caliberHeight)
                                {
                                    //using (GraphicsPath gp = new GraphicsPath())
                                    //{
                                    //    gp.AddPolygon(curvePointsRT);
                                    for (int i = r.X + r.Width / 8; i < r.Right - r.Width / 8; i += 2)
                                    {
                                        for (int j = r.Y + r.Height / 8; j < r.Bottom - r.Height / 8; j += 2)
                                        {
                                            //if (!gp.IsVisible(i, j))
                                            //    continue;
                                            c = bmp.GetPixel(i, j);
                                            if (c.GetBrightness() >= brightness)//(c == argbWhite)
                                                white++;
                                            else
                                                black++;
                                        }
                                    }
                                    //}
                                }
                                else
                                {
                                    x -= startXleft;
                                    continue;
                                }
                                //using (Graphics g = Graphics.FromImage(bmp))
                                //{
                                //    g.DrawRectangle(new Pen(Color.Red), r);
                                //}
                                //bmp.Save("Rectangle2.bmp", ImageFormat.Bmp);
                                if (black / 2 > white)
                                // if (white == 0)
                                {
                                    black = 0; white = 0;
                                    markerRT = r;
                                    //using (Graphics g = Graphics.FromImage(bmp))
                                    //{
                                    //    g.DrawPolygon(new Pen(Color.Red), curvePoints);
                                    //}
                                    //bmp.Save("Rectangle.bmp", ImageFormat.Bmp);

                                    goto nextStep2;
                                }
                                x -= startXleft;
                            }
                            if (markerRT != new Rectangle())
                                break;
                        }
                        #endregion
                    }
                    nextStep2:
                    //нижние маркеры
                    for (int k = 0; k < 3; k++)
                    {
                        #region
                        switch (k)
                        {
                            case 0:
                                markerBoundX = bmp.Width / 8;
                                markerBoundY = bmp.Height / 8;
                                break;
                            case 1:
                                markerBoundX = bmp.Width / 4;
                                markerBoundY = bmp.Height / 4;
                                break;
                            case 2:
                                markerBoundX = bmp.Width / 3;
                                markerBoundY = bmp.Height / 2;
                                break;
                        }
                        for (y = bmp.Height - startXleft; y > bmp.Height - markerBoundY; y -= caliberWidth)//снизу вверх 
                        {
                            for (x = startXleft; x < markerBoundX; x += caliberWidth)
                            {
                                //if (token.IsCancellationRequested)
                                //    return 0;
                                c = bmp.GetPixel(x, y);
                                if (c.GetBrightness() >= brightness)//(c == argbWhite)
                                    continue;

                                curvePoints = ContourFindSpeed(ref bmp, x, y, 0, false, true);
                                if (curvePoints.Length == contourMaxLength)
                                {
                                    x += startXleft;
                                    continue;
                                }

                                Rectangle r = GetRectangle(curvePoints);
                                width = r.Width; height = r.Height;
                                //if ((r.X < caliberWidth && r.Y < bmp.Height - caliberHeight)
                                //    && width > markerBoundX || height > markerBoundY)
                                //{
                                //    if ((width > (bmp.Width - bmp.Width / 8))
                                //      || (height > (bmp.Height - bmp.Height / 8)))
                                //    {
                                //        using (GraphicsPath gp = new GraphicsPath())
                                //        {
                                //            gp.AddPolygon(curvePoints);
                                //            RectangleF bounds = gp.GetBounds();
                                //            if (gp.IsVisible(bmp.Width - bmp.Width / 8, bmp.Height - bmp.Height / 8)
                                //                || gp.IsVisible(bmp.Width / 8, bmp.Height - bmp.Height / 8)
                                //                || gp.IsVisible(bmp.Width - bmp.Width / 8, bmp.Height / 8)
                                //                || gp.IsVisible(bmp.Width / 8, bmp.Height / 8)
                                //                )
                                //            {
                                //                x += startXleft;
                                //                continue;
                                //            }
                                //        }
                                //        using (Graphics g = Graphics.FromImage(bmp))
                                //        {
                                //            g.FillPolygon(new SolidBrush(argbWhite), curvePoints);
                                //            g.DrawPolygon(new Pen(argbWhite), curvePoints);//Color.Red
                                //            //bmp.Save("curvePoints.bmp", ImageFormat.Bmp);
                                //        }
                                //        continue;
                                //    }
                                //    AnchorPoints anchorPoints = GetAnchorPoints(curvePoints, r);
                                //    if (anchorPoints.LeftMinValue.X == 0
                                //        || anchorPoints.LeftBottom.X == 0
                                //        || anchorPoints.LeftTop.X == 0
                                //        || anchorPoints.LeftTop.Y == 0
                                //        || anchorPoints.RightBottom.Y == bmp.Height - 1)
                                //    {
                                //        using (Graphics g = Graphics.FromImage(bmp))
                                //        {//засветка левый нижний
                                //            g.FillPolygon(new SolidBrush(argbWhite), curvePoints);
                                //            g.DrawPolygon(new Pen(argbWhite), curvePoints);
                                //        }
                                //        continue;
                                //    }
                                //    x += startXleft;
                                //    continue;
                                //    //else
                                //    //{//goto nextStep3;
                                //    //}
                                //}
                                if (width == 0 || height == 0)
                                {
                                    continue;
                                }
                                double prop;
                                if (width > height)
                                    prop = (double)width / height;
                                else
                                    prop = (double)height / width;
                                if (prop > 2.5)
                                {
                                    x += startXleft;
                                    continue;
                                }

                                if (width > caliberWidth && height > caliberHeight)
                                {
                                    //using (GraphicsPath gp = new GraphicsPath())
                                    //{
                                    //    gp.AddPolygon(curvePointsLB);
                                    for (int i = r.X + r.Width / 8; i < r.Right - r.Width / 8; i += 2)
                                    {
                                        for (int j = r.Y + r.Height / 8; j < r.Bottom - r.Height / 8; j += 2)
                                        {
                                            //if (!gp.IsVisible(i, j))
                                            //continue;
                                            c = bmp.GetPixel(i, j);
                                            if (c.GetBrightness() >= brightness)//(c == argbWhite)
                                                white++;
                                            else
                                                black++;
                                        }
                                    }
                                    //}
                                }
                                else
                                {
                                    x += startXleft;
                                    continue;
                                }
                                if (black / 2 > white)
                                //if (white == 0)
                                {
                                    //Bitmap b2 = (Bitmap)bmp.Clone();
                                    //using (Graphics g = Graphics.FromImage(b2))
                                    //{
                                    //    g.DrawPolygon(new Pen(Color.Red), curvePoints);
                                    //    g.DrawRectangle(new Pen(Color.Yellow), r);
                                    //}
                                    //b2.Save("markerLB.bmp", ImageFormat.Bmp);
                                    //b2.Dispose();
                                    black = 0; white = 0;
                                    markerLB = r;
                                    goto nextStep3;
                                }
                                black = 0; white = 0;
                                x += startXleft;
                            }
                        }
                        if (markerLB != new Rectangle())
                            break;
                        #endregion
                    }
                    nextStep3:
                    for (int k = 0; k < 3; k++)
                    {
                        #region
                        switch (k)
                        {
                            case 0:
                                markerBoundX = bmp.Width / 8;
                                markerBoundY = bmp.Height / 8;
                                break;
                            case 1:
                                markerBoundX = bmp.Width / 4;
                                markerBoundY = bmp.Height / 4;
                                break;
                            case 2:
                                markerBoundX = bmp.Width / 3;
                                markerBoundY = bmp.Height / 2;
                                break;
                        }
                        for (y = bmp.Height - startXleft; y > bmp.Height - markerBoundY; y -= caliberWidth)//снизу вверх
                        {
                            if (markerRB != new Rectangle())
                                break;
                            for (x = startXright; x > bmp.Width - markerBoundX; x -= caliberWidth)//справа на лево
                            {
                                //if (token.IsCancellationRequested)
                                //    return 0;
                                c = bmp.GetPixel(x, y);
                                if (c.GetBrightness() >= brightness)//(c == argbWhite)
                                    continue;
                                curvePoints = ContourFindSpeed(ref bmp, x, y, 1, false, true);
                                if (curvePoints.Length == contourMaxLength)
                                {
                                    x -= startXleft;
                                    continue;
                                }

                                Rectangle r = GetRectangle(curvePoints);
                                width = r.Width; height = r.Height;
                                if (width == 0 || height == 0)
                                    continue;
                                //if (iter)
                                //{
                                //    curvePointsRB = ResectAppendix(curvePointsRB);
                                //    r = GetRectangle(curvePointsRB);
                                //}
                                double prop;
                                if (width > height)
                                    prop = (double)width / height;
                                else
                                    prop = (double)height / width;
                                if (prop > 2.5)
                                {
                                    x -= startXleft;
                                    continue;
                                }

                                if (width > caliberWidth && height > caliberHeight)
                                {
                                    //using (GraphicsPath gp = new GraphicsPath())
                                    //{
                                    //gp.AddPolygon(curvePointsRB);
                                    for (int i = r.X + r.Width / 8; i < r.Right - r.Width / 8; i += 2)
                                    {
                                        for (int j = r.Y + r.Height / 8; j < r.Bottom - r.Height / 8; j += 2)
                                        {
                                            //if (!gp.IsVisible(i, j))
                                            //continue;
                                            c = bmp.GetPixel(i, j);
                                            if (c.GetBrightness() >= brightness)//(c == argbWhite)
                                                white++;
                                            else
                                                black++;
                                        }
                                    }
                                }
                                //}
                                else
                                {
                                    black = 0; white = 0;
                                    continue;
                                }
                                if (black / 2 > white)
                                //if (white == 0)
                                {
                                    //using (Graphics g = Graphics.FromImage(bmp))
                                    //{
                                    //    g.DrawPolygon(new Pen(Color.Red), curvePoints);
                                    //    //g.DrawRectangle(new Pen(Color.Blue), r);
                                    //}
                                    //bmp.Save("Rectangle.bmp", ImageFormat.Bmp);
                                    markerRB = r;
                                    break;
                                }
                                black = 0; white = 0;
                                x -= startXleft;
                            }
                        }
                        if (markerRB != new Rectangle())
                            break;
                        #endregion
                    }


                    //Bitmap b = (Bitmap)bmp.Clone();
                    //using (Graphics g = Graphics.FromImage(b))
                    //{
                    //    g.DrawRectangle(new Pen(Color.Cyan), markerLT);
                    //    g.DrawRectangle(new Pen(Color.Red), markerRT);
                    //    g.DrawRectangle(new Pen(Color.DeepPink), markerRB);
                    //    g.DrawRectangle(new Pen(Color.OrangeRed), markerLB);
                    //}
                    //b.Save("markersAll.bmp", ImageFormat.Bmp);
                    //b.Dispose();

                    if ((markerLT != Rectangle.Empty
                        && markerRT != Rectangle.Empty
                        && markerLB == Rectangle.Empty
                        && markerRB == Rectangle.Empty)
                        ||
                        (markerLT == Rectangle.Empty
                        && markerRT == Rectangle.Empty
                        && markerLB != Rectangle.Empty
                        && markerRB != Rectangle.Empty))
                        return 2;

                    markerLT = MultiplyRectangle(markerLT, factor);
                    markerRT = MultiplyRectangle(markerRT, factor);
                    markerLB = MultiplyRectangle(markerLB, factor);
                    markerRB = MultiplyRectangle(markerRB, factor);

                    int[] sizesMark = new int[4];
                    sizesMark[0] = markerLT.Width * markerLT.Height;
                    sizesMark[1] = markerRT.Width * markerRT.Height;
                    sizesMark[2] = markerLB.Width * markerLB.Height;
                    sizesMark[3] = markerRB.Width * markerRB.Height;
                    int maxSize = 0;
                    int mSize = int.MaxValue;
                    int minNum = -1;
                    for (int i = 0; i < 4; i++)
                    {
                        if (sizesMark[i] < mSize)
                        {
                            mSize = sizesMark[i];
                            minNum = i;
                        }
                        if (sizesMark[i] > maxSize)
                        {
                            maxSize = sizesMark[i];
                            maxNum = i;
                        }
                    }
                    mSize = 0;
                    maxSize = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        if (i == minNum || i == maxNum || sizesMark[i] == 0)
                            //if (sizesMark[i] == 0)
                            continue;
                        mSize += sizesMark[i];
                        maxSize++;
                    }
                    if (maxSize == 0)
                    {
                        bmp.Dispose();
                        return 0;
                    }
                    maxNum = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        if (sizesMark[i] != 0)
                            maxNum++;
                    }

                    mSize = mSize / maxSize;
                    maxSize = (int)(mSize / 3);//10 мало? 11 много 
                    if (maxNum > 2)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (sizesMark[i] != 0 && Math.Abs(sizesMark[i] - mSize) > maxSize)
                            {
                                switch (i)
                                {
                                    case 0:
                                        //curvePointsLT = ResectAppendix(curvePointsLT);
                                        //markerLT = GetRectangle(curvePointsLT);
                                        //markerLT = MultiplyRectangle(markerLT, factor);
                                        //sizesMark[i] = markerLT.Width * markerLT.Height;
                                        //if (Math.Abs(sizesMark[i] - mSize) > maxSize)
                                        //{
                                        markerLT = Rectangle.Empty;
                                        maxNum--;
                                        //    if (maxNum < 3)
                                        //    {
                                        //        return maxNum;
                                        //    }
                                        //}
                                        break;
                                    case 1:
                                        //curvePointsRT = ResectAppendix(curvePointsRT);
                                        //markerRT = GetRectangle(curvePointsRT);
                                        //markerRT = MultiplyRectangle(markerRT, factor);
                                        //sizesMark[i] = markerRT.Width * markerRT.Height;
                                        //if (Math.Abs(sizesMark[i] - mSize) > maxSize)
                                        //{
                                        markerRT = Rectangle.Empty;
                                        maxNum--;
                                        //    if (maxNum < 3)
                                        //    {
                                        //        return maxNum;
                                        //    }
                                        //}
                                        break;
                                    case 2:
                                        //curvePointsLB = ResectAppendix(curvePointsLB);
                                        //markerLB = GetRectangle(curvePointsLB);
                                        //sizesMark[i] = markerLB.Width * markerLB.Height;
                                        //if (Math.Abs(sizesMark[i] - mSize) > maxSize)
                                        //{
                                        markerLB = Rectangle.Empty;
                                        maxNum--;
                                        //    if (maxNum < 3)
                                        //    {
                                        //        return maxNum;
                                        //    }
                                        //}
                                        break;
                                    default:
                                        //markerRB= Rectangle.Empty;
                                        //curvePointsRB = ResectAppendix(curvePointsRB);
                                        //markerRB = GetRectangle(curvePointsRB);
                                        //markerRB = MultiplyRectangle(markerRB, factor);
                                        //sizesMark[i] = markerRB.Width * markerRB.Height;
                                        //if (Math.Abs(sizesMark[i] - mSize) > maxSize)
                                        //{
                                        markerRB = Rectangle.Empty;
                                        maxNum--;
                                        //    if (maxNum < 3)
                                        //    {
                                        //        return maxNum;
                                        //    }
                                        //}
                                        break;
                                }
                            }
                        }
                    }
                    if (maxNum > 2)
                    {
                        if (markerRT != new Rectangle() && markerRB != new Rectangle())
                        {
                            if (Math.Abs(markerRT.X - markerRB.X) > markerRB.Width + markerRB.Width / 2)
                                continue;
                            if (markerLTet != new Rectangle() && IsSquare(markerLTet))
                                if (Math.Abs(markerRT.X - markerRB.X) > markerRB.Width / 2)
                                    continue;
                            if (Math.Abs(markerRT.Right - markerRB.Right) > markerRB.Width)
                                continue;
                        }
                        if (markerLT != new Rectangle() && markerLB != new Rectangle())
                        {
                            if (Math.Abs(markerLT.X - markerLB.X) > markerLB.Width)
                                continue;
                            if (Math.Abs(markerLT.X - markerLB.X) > markerLB.Width)
                                continue;
                        }
                        if (markerLT != new Rectangle() && markerRT != new Rectangle())
                        {
                            if (Math.Abs(markerLT.X - markerLB.X) > markerRT.Width)
                                continue;
                        }

                        bmp.Dispose();
                        return maxNum;
                    }
                    //bmp.Save("markres2.bmp", ImageFormat.Bmp);
                }
                //}
                //TimeSpan ts = DateTime.Now - dt;
                if (maxNum == 4 || factorIter > 0)
                {
                    bmp.Dispose();
                    return maxNum;
                }

                if (maxNum > 2)
                {
                    if (markerRT != new Rectangle() && markerRB != new Rectangle())
                    {
                        if (Math.Abs(markerRT.X - markerRB.X) > markerRB.Width + markerRB.Width / 2)
                            continue;
                        if (markerLTet != new Rectangle() && IsSquare(markerLTet))
                            if (Math.Abs(markerRT.X - markerRB.X) > markerRB.Width / 2)
                                continue;
                        if (Math.Abs(markerRT.Right - markerRB.Right) > markerRB.Width)
                            continue;
                    }
                    if (markerLT != new Rectangle() && markerLB != new Rectangle())
                    {
                        if (Math.Abs(markerLT.X - markerLB.X) > markerLB.Width)
                            continue;
                        if (Math.Abs(markerLT.X - markerLB.X) > markerLB.Width)
                            continue;
                    }
                    if (markerLT != new Rectangle() && markerRT != new Rectangle())
                    {
                        if (Math.Abs(markerLT.X - markerLB.X) > markerRT.Width)
                            continue;
                    }
                    bmp.Dispose();
                    return maxNum;
                }

                //if (maxNum > 2)
                //{
                //    bmp.Dispose();
                //    return maxNum;
                //}
                //else if (factorIter > 0)
                //{
                //    bmp.Dispose();
                //    return maxNum;
                //}
            }
            if (bmp != null)
                bmp.Dispose();
            return maxNum;
        }
        //-------------------------------------------------------------------------
        //private System.Drawing.Point[] ResectAppendix
        //    (System.Drawing.Point[] curvePoints
        //    , int factor = 2
        //    , int length = 15
        //    , int delta = 4)
        //{
        //    int curvePointsLengthPart = curvePoints.Length / factor;
        //    for (int i = 0; i < curvePoints.Length - 1; i++)
        //    {
        //        Point p = curvePoints[i];
        //        for (int j = -delta; j < delta + 1; j++)
        //        {
        //            for (int k = -delta; k < delta + 1; k++)
        //            {
        //                int index = Array.IndexOf(curvePoints, new Point(p.X + j, p.Y + k), i + 1);
        //                int dist = index - i;
        //                if (dist < curvePoints.Length - curvePointsLengthPart && dist > length)
        //                {
        //                    for (int l = i; l < index + 1; l++)
        //                    {
        //                        curvePoints[l] = p;
        //                    }
        //                    i = index;
        //                }
        //            }
        //        }
        //    }
        //    return curvePoints;
        //}
        //-------------------------------------------------------------------------
        public Rectangle MultiplyRectangle(Rectangle r, double factor)
        {
            if (factor == 1)
                return r;
            r.X = (int)Math.Round((double)r.X * factor);
            r.Y = (int)Math.Round((double)r.Y * factor);
            r.Width = (int)Math.Round((double)r.Width * factor);
            r.Height = (int)Math.Round((double)r.Height * factor);
            return r;
        }
        //-------------------------------------------------------------------------
        private Point[] ContourFindSpeed
            (
              LockBitmap lockBitmap
            , int x
            , int y
            , int direction = 0
            , bool err = false
            , bool getInitialColor = false
            , bool blackPicselFind = false
            , int limitPoints = contourMaxLength
            , double brightness = .88
            , bool getAllPoints = false
            )
        {
            Color argbWhite = Color.FromArgb(255, 255, 255);
            Color initialColor;
            Color color;
            int Course, MCourseByte;
            int iter = 0;
            int limitIter = limitPoints * 4;
            if (blackPicselFind)
            {
                switch (direction)
                {
                    #region
                    case 0://вперёд
                        do
                        {
                            color = lockBitmap.GetPixel(x, y);
                            //if (color == Color.FromArgb(255, 0, 0, 0))
                            if (color.GetBrightness() < brightness)//(color != argbWhite)
                                break;
                            //lockBitmap.SetPixel(x, y, Color.Red);
                            x++;
                            if (x >= lockBitmap.Width)
                                //Bitmap b = (Bitmap)bmp.Clone();
                                //b.Save("strips.bmp", ImageFormat.Bmp);
                                //b.Dispose();
                                return new System.Drawing.Point[0];
                        } while (true);
                        break;
                    case 1://назад 
                        color = lockBitmap.GetPixel(x, y);
                        //while (color == argbWhite && x < lockBitmap.Width - 1)
                        while (color.GetBrightness() > brightness && x < lockBitmap.Width - 1)
                        {
                            x--;
                            color = lockBitmap.GetPixel(x, y);
                        }
                        break;
                    case 2://вниз
                        do
                        {
                            color = lockBitmap.GetPixel(x, y);
                            //if (color == Color.FromArgb(255, 0, 0, 0))
                            if (color.GetBrightness() < brightness)//(color != argbWhite)
                                break;
                            y++;
                            if (x >= lockBitmap.Height)
                                return new System.Drawing.Point[0];
                        } while (true);
                        break;
                    default:
                        break;
                        #endregion
                }
            }
            switch (direction)
            {
                case 0:
                    Course = 7; MCourseByte = 7;//вперёд
                    break;
                case 1:
                    Course = 3; MCourseByte = 3;//назад 
                    break;
                case 2:
                    Course = 1; MCourseByte = 1;//вниз
                    break;
                default:
                    Course = 5; MCourseByte = 5;//вверх
                    break;
            }

            if (getInitialColor)
                initialColor = lockBitmap.GetPixel(x, y);
            else
                initialColor = Color.FromArgb(255, 0, 0, 0);
            //if (closing)
            //{
            //    return null;
            //}
            System.Drawing.Point prevPoint = new System.Drawing.Point(x, y);
            System.Drawing.Point firstStep = new System.Drawing.Point();

            switch (direction)
            {
                #region
                case 0:
                    if (x == 0)
                        break;
                    color = lockBitmap.GetPixel(x - 1, y);
                    //if (color == initialColor)
                    if (color.GetBrightness() < brightness)//(color != argbWhite)
                    {//не на контуре 
                        for (int i = x - 1; i > 1; i--)
                        {
                            color = lockBitmap.GetPixel(i - 1, y);
                            //if (color != initialColor)
                            if (color.GetBrightness() > brightness)//(color == argbWhite)
                            {
                                x = i;
                                break;
                            }
                        }
                    }
                    break;
                case 1:
                    if (x == lockBitmap.Width)
                        break;
                    color = lockBitmap.GetPixel(x + 1, y);
                    //if (color == initialColor)
                    if (color.GetBrightness() < brightness)//(color != argbWhite)
                    {//не на контуре 
                        for (int i = x + 1; i < lockBitmap.Width - 2; i++)
                        {
                            color = lockBitmap.GetPixel(i + 1, y);
                            //if (color != initialColor)
                            if (color.GetBrightness() > brightness)//(color == argbWhite)
                            {
                                x = i;
                                break;
                            }
                        }
                    }
                    break;
                case 2://вниз
                    if (y == 0)
                        break;
                    color = lockBitmap.GetPixel(x, y - 1);
                    //if (color != initialColor)
                    if (color.GetBrightness() < brightness)//(color != argbWhite)
                    {//не на контуре 
                        for (int i = y - 1; i < lockBitmap.Width - 2; i--)
                        {
                            color = lockBitmap.GetPixel(x, i - 1);
                            //if (color != initialColor)
                            if (color.GetBrightness() > brightness)//(color == argbWhite)
                            {
                                x = i;
                                break;
                            }
                        }
                    }
                    break;
                default:
                    if (y >= lockBitmap.Height)
                        break;
                    color = lockBitmap.GetPixel(x, y + 1);
                    //if (color != initialColor)
                    if (color.GetBrightness() < brightness)//(color != argbWhite)
                    {//не на контуре 
                        for (int i = y + 1; i < lockBitmap.Height - 1; i++)
                        {
                            color = lockBitmap.GetPixel(x, i + 1);
                            //if (color != initialColor)
                            if (color.GetBrightness() > brightness)//(color == argbWhite)
                            {
                                y = i;
                                break;
                            }
                        }
                    }
                    break;
                    #endregion
            }
            System.Drawing.Point[] contur = new System.Drawing.Point[1] { new System.Drawing.Point(x, y) };
            //conturClear = new System.Drawing.Point[1] { new System.Drawing.Point(x, y) };
            bool firstPoint = false;
            int Xcurs = x, Ycurs = y;
            System.Drawing.Point p = ConturNextStepSpeed(lockBitmap, lockBitmap, initialColor, ref Xcurs, ref Ycurs, ref Course);
            if (p == null)//начальная точка белая
                return new System.Drawing.Point[] { new System.Drawing.Point(x, y) };
            firstStep = p;
            int firstCourse = Course;
            prevPoint = new System.Drawing.Point(x, y);
            do
            {
                #region
                if (token.IsCancellationRequested)
                    return contur;
                iter++;
                if (limitPoints > 0 && contur.Length >= limitPoints)
                    return contur;
                p = ConturNextStepSpeed(lockBitmap, lockBitmap, initialColor, ref Xcurs, ref Ycurs, ref Course);
                int px = p.X;
                if (p == prevPoint && firstCourse == Course)
                    return contur;
                if (firstPoint)
                {
                    if (p == firstStep)
                    {
                        Array.Resize(ref contur, contur.Length + 1);
                        contur[contur.Length - 1] = contur[0];
                        return contur;
                    }
                    else
                        firstPoint = false;
                }
                if (contur.Length > 0 && p == contur[0] && !firstPoint)
                    firstPoint = true;
                if (err)
                {
                    if (p.X >= contur[0].X + lockBitmap.Width)
                        return null;
                    if (p.Y <= contur[0].Y - lockBitmap.Height)
                        return null;
                }
                int index = Array.IndexOf(contur, p);
                if (lockBitmap.Height < lockBitmap.Height && index > 0)
                {
                    if (index > 0)
                    {
                        Array.Resize(ref contur, contur.Length + 1);
                        contur[contur.Length - 1] = contur[0];
                        return contur;
                    }
                }
                if (getAllPoints)
                {
                    Array.Resize(ref contur, contur.Length + 1);
                    contur[contur.Length - 1] = prevPoint;
                }
                else if (MCourseByte != Course)
                {
                    if (contur[contur.Length - 1] != prevPoint)
                    {
                        Array.Resize(ref contur, contur.Length + 1);
                        contur[contur.Length - 1] = prevPoint;
                    }
                }
                prevPoint = new System.Drawing.Point(Xcurs, Ycurs);
                #endregion
            } while (iter < limitIter);
            lockBitmap.UnlockBits();
            return contur;
        }
        //-------------------------------------------------------------------------
        private Point[] ContourFindSpeed
            (
              ref Bitmap entryBitmap
            , int x
            , int y
            , int direction = 0
            , bool err = false
            , bool getInitialColor = false
            , bool blackPicselFind = false
            , int limitPoints = contourMaxLength
            , double brightness = .88
            , bool getAllPoints = false
            )
        {
            //Bitmap bmp = (Bitmap)entryBitmap.Clone();

            //LockBitmap lockBitmap = new LockBitmap(bmp);
            //lockBitmap.LockBits();

            //Color argbWhite = Color.FromArgb(255, 255, 255);
            Color initialColor;
            Color color;
            int Course, MCourseByte;
            int iter = 0;
            int limitIter = limitPoints * 4;
            if (blackPicselFind)
            {
                if (x >= entryBitmap.Width || y >= entryBitmap.Height)
                    return new System.Drawing.Point[0];

                switch (direction)
                {
                    #region
                    case 0://вперёд
                        do
                        {
                            if (x >= entryBitmap.Width || x < 0)
                                //{
                                //Bitmap b = (Bitmap)bmp.Clone();
                                //bmp.Save("strips.bmp", ImageFormat.Bmp);
                                //b.Dispose();

                                return new System.Drawing.Point[0];
                            //}
                            color = entryBitmap.GetPixel(x, y);
                            //if (color == Color.FromArgb(255, 0, 0, 0))
                            if (color.GetBrightness() < brightness)//(color != argbWhite)
                                break;
                            //lockBitmap.SetPixel(x, y, Color.Red);
                            x++;
                        } while (true);
                        break;
                    case 1://назад 
                        color = entryBitmap.GetPixel(x, y);
                        //while (color == argbWhite && x < lockBitmap.Width - 1)
                        while (color.GetBrightness() > brightness && x < entryBitmap.Width - 1)
                        {
                            x--;
                            color = entryBitmap.GetPixel(x, y);
                        }
                        break;
                    case 2://вниз
                        do
                        {
                            if (x >= entryBitmap.Height)
                                return new System.Drawing.Point[0];
                            color = entryBitmap.GetPixel(x, y);
                            //if (color == Color.FromArgb(255, 0, 0, 0))
                            if (color.GetBrightness() < brightness)//(color != argbWhite)
                                break;
                            y++;
                        } while (true);
                        break;
                    default:
                        break;
                        #endregion
                }
            }
            switch (direction)
            {
                case 0:
                    Course = 7; MCourseByte = 7;//вперёд
                    break;
                case 1:
                    Course = 3; MCourseByte = 3;//назад 
                    break;
                case 2:
                    Course = 1; MCourseByte = 1;//вниз
                    break;
                default:
                    Course = 5; MCourseByte = 5;//вверх
                    break;
            }

            if (getInitialColor)
                initialColor = entryBitmap.GetPixel(x, y);
            else
                initialColor = Color.FromArgb(255, 0, 0, 0);
            //if (closing)
            //{
            //    return null;
            //}
            System.Drawing.Point prevPoint = new System.Drawing.Point(x, y);
            System.Drawing.Point firstStep = new System.Drawing.Point();

            switch (direction)
            {
                #region
                case 0:
                    if (x == 0)
                        break;
                    color = entryBitmap.GetPixel(x - 1, y);
                    //double br = color.GetBrightness();
                    //if (color == initialColor)
                    if (color.GetBrightness() < brightness)//(color != argbWhite)
                    {//не на контуре 
                        for (int i = x - 1; i > 0; i--)
                        {
                            color = entryBitmap.GetPixel(i - 1, y);
                            //if (color != initialColor)
                            if (color.GetBrightness() > brightness)//(color == argbWhite)
                            {
                                x = i;
                                break;
                            }
                            if (i == 1)
                            {
                                x = 0;
                                break;
                            }
                        }
                    }
                    break;
                case 1:
                    if (x == entryBitmap.Width - 1)
                        break;
                    color = entryBitmap.GetPixel(x + 1, y);
                    //if (color == initialColor)
                    if (color.GetBrightness() < brightness)//(color != argbWhite)
                    {//не на контуре 
                        for (int i = x + 1; i < entryBitmap.Width - 1; i++)
                        {
                            color = entryBitmap.GetPixel(i + 1, y);
                            //if (color != initialColor)
                            if (color.GetBrightness() > brightness)//(color == argbWhite)
                            {
                                x = i;
                                break;
                            }
                            if (i == entryBitmap.Width - 2)
                            {
                                x = entryBitmap.Width - 1;
                                break;
                            }
                        }
                    }
                    break;
                case 2://вниз
                    if (y == 0)
                        break;
                    color = entryBitmap.GetPixel(x, y - 1);
                    //if (color != initialColor)
                    if (color.GetBrightness() < brightness)//(color != argbWhite)
                    {//не на контуре 
                     //for (int i = y - 1; i < entryBitmap.Width - 1; i--)
                        for (int i = y; i < 1; i--)
                        {
                            color = entryBitmap.GetPixel(x, i - 1);
                            //if (color != initialColor)
                            if (color.GetBrightness() > brightness)//(color == argbWhite)
                            {
                                y = i;
                                break;
                            }
                            if (i == 1)
                            {
                                y = 0;
                                break;
                            }
                        }
                    }
                    break;
                default:
                    if (y >= entryBitmap.Height - 1)
                        break;
                    color = entryBitmap.GetPixel(x, y + 1);
                    //if (color != initialColor)
                    if (color.GetBrightness() < brightness)//(color != argbWhite)
                    {//не на контуре 
                        for (int i = y + 1; i < entryBitmap.Height - 1; i++)
                        {
                            color = entryBitmap.GetPixel(x, i + 1);
                            //if (color != initialColor)
                            if (color.GetBrightness() > brightness)//(color == argbWhite)
                            {
                                y = i;
                                break;
                            }
                            if (i == entryBitmap.Height - 2)
                            {
                                y = entryBitmap.Height - 1;
                                break;
                            }
                        }
                    }
                    break;
                    #endregion
            }
            System.Drawing.Point[] contur = new System.Drawing.Point[1] { new System.Drawing.Point(x, y) };
            //conturClear = new System.Drawing.Point[1] { new System.Drawing.Point(x, y) };
            bool firstPoint = false;
            int Xcurs = x, Ycurs = y;
            System.Drawing.Point p = ConturNextStepSpeed(ref entryBitmap, ref entryBitmap, initialColor, ref Xcurs, ref Ycurs, ref Course, brightness);
            if (p == null)
                //начальная точка белая
                return new System.Drawing.Point[] { new System.Drawing.Point(x, y) };
            firstStep = p;
            int firstCourse = Course;
            prevPoint = new System.Drawing.Point(x, y);
            do
            {
                #region
                if (token.IsCancellationRequested)
                    return contur;
                iter++;
                if (limitPoints > 0 && contur.Length >= limitPoints)
                    return contur;
                p = ConturNextStepSpeed(ref entryBitmap, ref entryBitmap, initialColor, ref Xcurs, ref Ycurs, ref Course, brightness);
                int px = p.X;
                if (p == prevPoint && firstCourse == Course)
                    return contur;
                if (firstPoint)
                {
                    if (p == firstStep)
                    {
                        Array.Resize(ref contur, contur.Length + 1);
                        contur[contur.Length - 1] = contur[0];
                        return contur;
                    }
                    else
                        firstPoint = false;
                }
                if (contur.Length > 0 && p == contur[0] && !firstPoint)
                {
                    firstPoint = true;
                }
                if (err)
                {
                    if (p.X >= contur[0].X + entryBitmap.Width)
                        return null;
                    if (p.Y <= contur[0].Y - entryBitmap.Height)
                        return null;
                }
                int index = Array.IndexOf(contur, p);
                if (entryBitmap.Height < entryBitmap.Height && index > 0)
                {
                    if (index > 0)
                    {
                        Array.Resize(ref contur, contur.Length + 1);
                        contur[contur.Length - 1] = contur[0];
                        return contur;
                    }
                }
                if (getAllPoints)
                {
                    Array.Resize(ref contur, contur.Length + 1);
                    contur[contur.Length - 1] = prevPoint;
                }
                else if (MCourseByte != Course)
                {
                    if (contur[contur.Length - 1] != prevPoint)
                    {
                        Array.Resize(ref contur, contur.Length + 1);
                        contur[contur.Length - 1] = prevPoint;
                    }
                }
                prevPoint = new System.Drawing.Point(Xcurs, Ycurs);
                #endregion
            } while (iter < limitIter);
            return contur;
        }

        //-------------------------------------------------------------------------
        private void GetOuterContourSpeed
            (
              LockBitmap bmp
            , ref Point[] contour
            , ref Rectangle r
            , out Point p
            , int contourLength = 20000
            , double brightness = .88
            )
        {
            Color argbWhite = Color.FromArgb(255, 255, 255);
            Color color = argbWhite;
            r = GetRectangle(contour, out p);
            //r = GetRectangle(contour);
            try
            {
                color = bmp.GetPixel(p.X - 1, p.Y);
            }
            catch (Exception)
            {
            }
            if (contour.Length < contourLength
                && color.GetBrightness() < brightness)//внутренний контур
            {
                contour = ContourFindSpeed
                (
                  bmp
                , p.X
                , p.Y
                , 0//вперёд
                , false
                , true
                , false
                , contourLength
                , brightness
                );
                r = GetRectangle(contour, out p);
            }
        }
        //-------------------------------------------------------------------------
        private void GetOuterContourSpeed
            (
              ref Bitmap bmp
            , ref Point[] contour
            , ref Rectangle r
            , out Point p
            , int contourLength = 20000
            , double brightness = .88
            )
        {
            Color argbWhite = Color.FromArgb(255, 255, 255);
            Color color = argbWhite;

            //LockBitmap lockBitmap = new LockBitmap(bmp);
            //lockBitmap.LockBits();

            r = GetRectangle(contour, out p);
            //r = GetRectangle(contour);
            try
            {
                color = bmp.GetPixel(p.X - 1, p.Y);
            }
            catch (Exception)
            {
            }
            if (contour.Length < contourLength
                && color.GetBrightness() < brightness)//внутренний контур
            {
                contour = ContourFindSpeed
                (
                  ref bmp
                , p.X
                , p.Y
                , 0//вперёд
                , false
                , true
                , false
                , contourLength
                , brightness
                );
                r = GetRectangle(contour, out p);
            }
            //lockBitmap.UnlockBits();
        }
        //-------------------------------------------------------------------------
        private System.Drawing.Point ConturNextStepSpeed
            (
              ref Bitmap bmp
            , ref Bitmap entryBitmap
            , Color initialColor
            , ref int Xcurs
            , ref int Ycurs
            , ref int Course
            , double brightness = .88
            )
        {
            System.Drawing.Point point = new System.Drawing.Point(Xcurs, Ycurs);
            Color c;
            Color argbWhite = Color.FromArgb(255, 255, 255);
            int MCourseByte = Course;
            byte i; int XSc1 = 0, YSc1 = 0;
            Course = (Course + 4) % 8; //инверсия курса, кольцевой счетчик 
            for (i = 0; i < 8; i++)
            {
                Course = (Course + 1) % 8;//шаг курса
                switch (Course)
                {
                    case 0://вперёд вниз
                        XSc1 = 1; YSc1 = 1;
                        break;
                    case 1://вниз
                        XSc1 = 0; YSc1 = 1;
                        break;
                    case 2://назад вниз
                        XSc1 = -1; YSc1 = 1;
                        break;
                    case 3://назад 
                        XSc1 = -1; YSc1 = 0;
                        break;
                    case 4://назад вверх
                        XSc1 = -1; YSc1 = -1;
                        break;
                    case 5://вверх
                        XSc1 = 0; YSc1 = -1;
                        break;
                    case 6://вперёд вверх
                        XSc1 = 1; YSc1 = -1;
                        break;
                    case 7://вперёд
                        XSc1 = 1; YSc1 = 0;
                        break;
                    default:
                        break;
                }
                int nX = Xcurs + XSc1;
                if (nX < 0 || nX >= bmp.Width)
                    continue;
                int nY = Ycurs + YSc1;
                if (nY < 0 || nY >= bmp.Height)
                {
                    if (bmp.Height < entryBitmap.Height)
                    {//не используется?
                        if (nY < 0)
                        {
                            if (Xcurs < bmp.Width - 1)
                            {
                                c = bmp.GetPixel(Xcurs + 1, 0);
                                if (c.GetBrightness() < brightness)//(c != argbWhite)//(c == initialColor)
                                {
                                    Course = 7;//вперёд
                                    Xcurs++;
                                    point = new System.Drawing.Point(Xcurs, Ycurs);
                                    return point;
                                }
                                else
                                {
                                    c = bmp.GetPixel(Xcurs + 1, Ycurs + 1);
                                    if (c.GetBrightness() < brightness)//(c != argbWhite)//(c == initialColor)
                                    {
                                        Course = 0;//вперёд вниз
                                        Xcurs++;
                                        Ycurs++;
                                        point = new System.Drawing.Point(Xcurs, Ycurs);
                                        return point;
                                    }
                                    else
                                    {//идём по верхнему краю, впереди и внизу нет initialColor

                                        c = bmp.GetPixel(Xcurs, Ycurs + 1);
                                        if (c.GetBrightness() < brightness)//(c != argbWhite)//(c == initialColor)
                                        {
                                            Course = 1;//вниз
                                            Ycurs++;
                                            point = new System.Drawing.Point(Xcurs, Ycurs);
                                            return point;
                                        }
                                        //continue;
                                        c = bmp.GetPixel(Xcurs - 1, Ycurs + 1);
                                        if (c.GetBrightness() < brightness)//(c != argbWhite)//(c == initialColor)
                                        {
                                            Course = 2;//назад вниз
                                            Xcurs--;
                                            Ycurs++;
                                            point = new System.Drawing.Point(Xcurs, Ycurs);
                                            return point;
                                        }
                                        Course = 7;
                                        Xcurs++;
                                        point = new System.Drawing.Point(Xcurs, Ycurs);
                                        return point;
                                    }
                                }
                            }
                            else
                            {//не используется?
                                if (Ycurs < bmp.Height - 1)
                                //    c = bmp.GetPixel(Xcurs - 1, 1);
                                //if (c != argbWhite)//(c == initialColor)
                                {
                                    Course = 1;//вниз
                                    Ycurs++;
                                    point = new System.Drawing.Point(Xcurs, Ycurs);
                                    return point;
                                }
                                else
                                {
                                    Course = 3;//назад 
                                    Xcurs--;
                                    point = new System.Drawing.Point(Xcurs, Ycurs);
                                    return point;
                                }
                            }
                        }
                        else
                        {
                            if (Xcurs == 0)
                            {
                                Course = 5;//вверх 
                                Ycurs--;
                                point = new System.Drawing.Point(Xcurs, Ycurs);
                                return point;
                            }
                            if (Xcurs > 0)
                            {
                                c = bmp.GetPixel(Xcurs - 1, Ycurs);
                                if (c.GetBrightness() < brightness)//(c != argbWhite)//(c == initialColor)
                                {
                                    Course = 3;//назад 
                                    Xcurs--;
                                    point = new System.Drawing.Point(Xcurs, Ycurs);
                                    return point;
                                }
                                else
                                {
                                    c = bmp.GetPixel(Xcurs - 1, Ycurs - 1);
                                    if (c.GetBrightness() < brightness)//(c != argbWhite)//(c == initialColor)
                                    {
                                        Course = 4;//назад вверх
                                        Xcurs--;
                                        Ycurs--;
                                        point = new System.Drawing.Point(Xcurs, Ycurs);
                                        return point;
                                    }
                                    else
                                    {
                                        c = bmp.GetPixel(Xcurs, Ycurs - 1);
                                        if (c.GetBrightness() < brightness)//(c != argbWhite)//(c == initialColor)
                                        {
                                            Course = 5;//вверх 
                                            Ycurs--;
                                            point = new System.Drawing.Point(Xcurs, Ycurs);
                                            return point;
                                        }
                                        if (Xcurs < bmp.Width - 1)
                                        {
                                            c = bmp.GetPixel(Xcurs + 1, Ycurs - 1);
                                            if (c.GetBrightness() < brightness)//(c != argbWhite)//(c == initialColor)
                                            {
                                                Course = 6;//вперёд вверх
                                                Xcurs++;
                                                Ycurs--;
                                                point = new System.Drawing.Point(Xcurs, Ycurs);
                                                return point;
                                            }
                                        }
                                    }
                                }
                                Course = 3;//назад 
                                Xcurs--;
                                point = new System.Drawing.Point(Xcurs, Ycurs);
                                return point;
                            }
                            c = bmp.GetPixel(Xcurs, Ycurs - 1);
                            if (c.GetBrightness() < brightness)//(c != argbWhite)//(c == initialColor)
                            {
                                Course = 5;//вверх 
                                Ycurs--;
                                point = new System.Drawing.Point(Xcurs, Ycurs);
                                return point;
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                try
                {//???
                    //System.Windows.Forms.Application.DoEvents();
                    c = bmp.GetPixel(nX, nY);//HResult=-2146233079 уже заблокирована.
                }
                catch (Exception)
                {
                    return point;
                }
                if (c.GetBrightness() < brightness)//(c != argbWhite)//(c == initialColor)
                {
                    Xcurs = nX;
                    Ycurs = nY;
                    point = new System.Drawing.Point(Xcurs, Ycurs);
                    return point;
                    //Course 0://вперёд вниз
                    //Course 1://вниз
                    //Course 2://назад вниз
                    //Course 3://назад 
                    //Course 4://назад вверх
                    //Course 5://вверх
                    //Course 6://вперёд вверх
                    //Course 7://вперёд
                }
            }
            return point;
        }
        //-------------------------------------------------------------------------
        private System.Drawing.Point ConturNextStepSpeed
            (
              LockBitmap bmp
            , LockBitmap entryBitmap
            , Color initialColor
            , ref int Xcurs
            , ref int Ycurs
            , ref int Course
            , double brightness = .88
            )
        {
            System.Drawing.Point point = new System.Drawing.Point(Xcurs, Ycurs);
            Color c;
            Color argbWhite = Color.FromArgb(255, 255, 255);
            int MCourseByte = Course;
            byte i; int XSc1 = 0, YSc1 = 0;
            Course = (Course + 4) % 8; //инверсия курса, кольцевой счетчик 
            for (i = 0; i < 8; i++)
            {
                Course = (Course + 1) % 8;//шаг курса
                switch (Course)
                {
                    case 0://вперёд вниз
                        XSc1 = 1; YSc1 = 1;
                        break;
                    case 1://вниз
                        XSc1 = 0; YSc1 = 1;
                        break;
                    case 2://назад вниз
                        XSc1 = -1; YSc1 = 1;
                        break;
                    case 3://назад 
                        XSc1 = -1; YSc1 = 0;
                        break;
                    case 4://назад вверх
                        XSc1 = -1; YSc1 = -1;
                        break;
                    case 5://вверх
                        XSc1 = 0; YSc1 = -1;
                        break;
                    case 6://вперёд вверх
                        XSc1 = 1; YSc1 = -1;
                        break;
                    case 7://вперёд
                        XSc1 = 1; YSc1 = 0;
                        break;
                    default:
                        break;
                }
                int nX = Xcurs + XSc1;
                if (nX < 0 || nX >= bmp.Width)
                {
                    continue;
                }
                int nY = Ycurs + YSc1;
                if (nY < 0 || nY >= bmp.Height)
                {
                    if (bmp.Height < entryBitmap.Height)
                    {//не используется?
                        if (nY < 0)
                        {
                            if (Xcurs < bmp.Width - 1)
                            {
                                c = bmp.GetPixel(Xcurs + 1, 0);
                                if (c.GetBrightness() < brightness)//(c != argbWhite)//(c == initialColor)
                                {
                                    Course = 7;//вперёд
                                    Xcurs++;
                                    point = new System.Drawing.Point(Xcurs, Ycurs);
                                    return point;
                                }
                                else
                                {
                                    c = bmp.GetPixel(Xcurs + 1, Ycurs + 1);
                                    if (c.GetBrightness() < brightness)//(c != argbWhite)//(c == initialColor)
                                    {
                                        Course = 0;//вперёд вниз
                                        Xcurs++;
                                        Ycurs++;
                                        point = new System.Drawing.Point(Xcurs, Ycurs);
                                        return point;
                                    }
                                    else
                                    {//идём по верхнему краю, впереди и внизу нет initialColor

                                        c = bmp.GetPixel(Xcurs, Ycurs + 1);
                                        if (c.GetBrightness() < brightness)//(c != argbWhite)//(c == initialColor)
                                        {
                                            Course = 1;//вниз
                                            Ycurs++;
                                            point = new System.Drawing.Point(Xcurs, Ycurs);
                                            return point;
                                        }
                                        //continue;
                                        c = bmp.GetPixel(Xcurs - 1, Ycurs + 1);
                                        if (c.GetBrightness() < brightness)//(c != argbWhite)//(c == initialColor)
                                        {
                                            Course = 2;//назад вниз
                                            Xcurs--;
                                            Ycurs++;
                                            point = new System.Drawing.Point(Xcurs, Ycurs);
                                            return point;
                                        }
                                        Course = 7;
                                        Xcurs++;
                                        point = new System.Drawing.Point(Xcurs, Ycurs);
                                        return point;
                                    }
                                }
                            }
                            else
                            {//не используется?
                                if (Ycurs < bmp.Height - 1)
                                //    c = bmp.GetPixel(Xcurs - 1, 1);
                                //if (c != argbWhite)//(c == initialColor)
                                {
                                    Course = 1;//вниз
                                    Ycurs++;
                                    point = new System.Drawing.Point(Xcurs, Ycurs);
                                    return point;
                                }
                                else
                                {
                                    Course = 3;//назад 
                                    Xcurs--;
                                    point = new System.Drawing.Point(Xcurs, Ycurs);
                                    return point;
                                }
                            }
                        }
                        else
                        {
                            if (Xcurs == 0)
                            {
                                Course = 5;//вверх 
                                Ycurs--;
                                point = new System.Drawing.Point(Xcurs, Ycurs);
                                return point;
                            }
                            if (Xcurs > 0)
                            {
                                c = bmp.GetPixel(Xcurs - 1, Ycurs);
                                if (c.GetBrightness() < brightness)//(c != argbWhite)//(c == initialColor)
                                {
                                    Course = 3;//назад 
                                    Xcurs--;
                                    point = new System.Drawing.Point(Xcurs, Ycurs);
                                    return point;
                                }
                                else
                                {
                                    c = bmp.GetPixel(Xcurs - 1, Ycurs - 1);
                                    if (c.GetBrightness() < brightness)//(c != argbWhite)//(c == initialColor)
                                    {
                                        Course = 4;//назад вверх
                                        Xcurs--;
                                        Ycurs--;
                                        point = new System.Drawing.Point(Xcurs, Ycurs);
                                        return point;
                                    }
                                    else
                                    {
                                        c = bmp.GetPixel(Xcurs, Ycurs - 1);
                                        if (c.GetBrightness() < brightness)//(c != argbWhite)//(c == initialColor)
                                        {
                                            Course = 5;//вверх 
                                            Ycurs--;
                                            point = new System.Drawing.Point(Xcurs, Ycurs);
                                            return point;
                                        }
                                        if (Xcurs < bmp.Width - 1)
                                        {
                                            c = bmp.GetPixel(Xcurs + 1, Ycurs - 1);
                                            if (c.GetBrightness() < brightness)//(c != argbWhite)//(c == initialColor)
                                            {
                                                Course = 6;//вперёд вверх
                                                Xcurs++;
                                                Ycurs--;
                                                point = new System.Drawing.Point(Xcurs, Ycurs);
                                                return point;
                                            }
                                        }
                                    }
                                }
                                Course = 3;//назад 
                                Xcurs--;
                                point = new System.Drawing.Point(Xcurs, Ycurs);
                                return point;
                            }
                            c = bmp.GetPixel(Xcurs, Ycurs - 1);
                            if (c.GetBrightness() < brightness)//(c != argbWhite)//(c == initialColor)
                            {
                                Course = 5;//вверх 
                                Ycurs--;
                                point = new System.Drawing.Point(Xcurs, Ycurs);
                                return point;
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                c = bmp.GetPixel(nX, nY);
                if (c.GetBrightness() < brightness)//(c != argbWhite)//(c == initialColor)
                {
                    Xcurs = nX;
                    Ycurs = nY;
                    point = new System.Drawing.Point(Xcurs, Ycurs);
                    return point;
                    //Course 0://вперёд вниз
                    //Course 1://вниз
                    //Course 2://назад вниз
                    //Course 3://назад 
                    //Course 4://назад вверх
                    //Course 5://вверх
                    //Course 6://вперёд вверх
                    //Course 7://вперёд
                }
            }
            return point;
        }
        //-------------------------------------------------------------------------
        private Point[] ContourFind
            (
              Bitmap entryBitmap
            , int x
            , int y
            , int direction = 0
            , bool err = false
            , bool getInitialColor = false
            , bool blackPicselFind = false
            , int limitPoints = contourMaxLength
            , bool getAllPoints = false
            )
        {
            //Bitmap bmp = (Bitmap)entryBitmap.Clone();
            //using (Bitmap bmp = (Bitmap)entryBitmap.Clone())
            //{

            Color argbWhite = Color.FromArgb(255, 255, 255);
            Color initialColor;
            Color color;
            int Course, MCourseByte;
            int iter = 0;
            int limitIter = limitPoints * 4;
            if (blackPicselFind)
            {
                switch (direction)
                {
                    #region
                    case 0://вперёд
                        do
                        {
                            //if (x < 0)
                            //{

                            //}
                            if (x >= entryBitmap.Width)
                                return new System.Drawing.Point[0];
                            color = entryBitmap.GetPixel(x, y);
                            //if (color == Color.FromArgb(255, 0, 0, 0))
                            if (color != argbWhite)
                            {
                                break;
                            }
                            x++;
                        } while (true);
                        break;
                    case 1://назад 
                        color = entryBitmap.GetPixel(x, y);
                        while (color == argbWhite && x < entryBitmap.Width - 1)
                        {
                            x--;
                            color = entryBitmap.GetPixel(x, y);
                        }
                        break;
                    case 2://вниз
                        do
                        {
                            if (x >= entryBitmap.Height)
                                return new System.Drawing.Point[0];
                            color = entryBitmap.GetPixel(x, y);
                            //if (color == Color.FromArgb(255, 0, 0, 0))
                            if (color != argbWhite)
                            {
                                break;
                            }
                            y++;
                        } while (true);
                        break;
                    default:
                        break;
                        #endregion
                }
            }
            switch (direction)
            {
                case 0:
                    Course = 7; MCourseByte = 7;//вперёд
                    break;
                case 1:
                    Course = 3; MCourseByte = 3;//назад 
                    break;
                case 2:
                    Course = 1; MCourseByte = 1;//вниз
                    break;
                default:
                    Course = 5; MCourseByte = 5;//вверх
                    break;
            }

            if (getInitialColor)
            {
                initialColor = entryBitmap.GetPixel(x, y);
            }
            else
            {
                initialColor = Color.FromArgb(255, 0, 0, 0);
            }
            System.Drawing.Point prevPoint = new System.Drawing.Point(x, y);
            System.Drawing.Point firstStep = new System.Drawing.Point();

            switch (direction)
            {
                #region
                case 0:
                    if (x == 0)
                    {
                        break;
                    }
                    color = entryBitmap.GetPixel(x - 1, y);
                    //if (color == initialColor)
                    if (color != argbWhite)
                    {//не на контуре 
                        for (int i = x - 1; i > 1; i--)
                        {
                            color = entryBitmap.GetPixel(i - 1, y);
                            //if (color != initialColor)
                            if (color == argbWhite)
                            {
                                x = i;
                                break;
                            }
                        }
                    }
                    break;
                case 1:
                    if (x == entryBitmap.Width)
                    {
                        break;
                    }
                    color = entryBitmap.GetPixel(x + 1, y);
                    //if (color == initialColor)
                    if (color != argbWhite)
                    {//не на контуре 
                        for (int i = x + 1; i < entryBitmap.Width - 2; i++)
                        {
                            color = entryBitmap.GetPixel(i + 1, y);
                            //if (color != initialColor)
                            if (color == argbWhite)
                            {
                                x = i;
                                break;
                            }
                        }
                    }
                    break;
                case 2://вниз
                    if (y == 0)
                    {
                        break;
                    }
                    color = entryBitmap.GetPixel(x, y - 1);
                    //if (color != initialColor)
                    if (color != argbWhite)
                    {//не на контуре 
                        for (int i = y - 1; i < entryBitmap.Width - 2; i--)
                        {
                            color = entryBitmap.GetPixel(x, i - 1);
                            //if (color != initialColor)
                            if (color == argbWhite)
                            {
                                x = i;
                                break;
                            }
                        }
                    }
                    break;
                default:
                    if (y >= entryBitmap.Height)
                    {
                        break;
                    }
                    color = entryBitmap.GetPixel(x, y + 1);
                    //if (color != initialColor)
                    if (color != argbWhite)
                    {//не на контуре 
                        for (int i = y + 1; i < entryBitmap.Height - 1; i++)
                        {
                            color = entryBitmap.GetPixel(x, i + 1);
                            //if (color != initialColor)
                            if (color == argbWhite)
                            {
                                y = i;
                                break;
                            }
                        }
                    }
                    break;
                    #endregion
            }
            System.Drawing.Point[] contur = new System.Drawing.Point[1] { new System.Drawing.Point(x, y) };
            //conturClear = new System.Drawing.Point[1] { new System.Drawing.Point(x, y) };
            bool firstPoint = false;
            int Xcurs = x, Ycurs = y;
            System.Drawing.Point p = ConturNextStep(ref entryBitmap, ref entryBitmap, initialColor, ref Xcurs, ref Ycurs, ref Course);
            if (p == null)
            {//начальная точка белая
                return new System.Drawing.Point[] { new System.Drawing.Point(x, y) };
            }
            firstStep = p;
            int firstCourse = Course;
            prevPoint = new System.Drawing.Point(x, y);
            do
            {
                #region
                if (token.IsCancellationRequested)
                    return contur;
                iter++;
                if (limitPoints > 0 && contur.Length >= limitPoints)
                {
                    return contur;
                }
                p = ConturNextStep(ref entryBitmap, ref entryBitmap, initialColor, ref Xcurs, ref Ycurs, ref Course);
                int px = p.X;
                if (p == prevPoint && firstCourse == Course)
                {
                    return contur;
                }
                if (firstPoint)
                {
                    if (p == firstStep)
                    {
                        Array.Resize(ref contur, contur.Length + 1);
                        contur[contur.Length - 1] = contur[0];
                        return contur;
                    }
                    else
                    {
                        firstPoint = false;
                    }
                }
                if (contur.Length > 0 && p == contur[0] && !firstPoint)
                {
                    {
                        firstPoint = true;
                    }
                }
                if (err)
                {
                    if (p.X >= contur[0].X + entryBitmap.Width)
                    {
                        return null;
                    }
                    if (p.Y <= contur[0].Y - entryBitmap.Height)
                    {
                        return null;
                    }
                }
                int index = Array.IndexOf(contur, p);
                if (entryBitmap.Height < entryBitmap.Height && index > 0)
                {
                    if (index > 0)
                    {
                        Array.Resize(ref contur, contur.Length + 1);
                        contur[contur.Length - 1] = contur[0];
                        return contur;
                    }
                }

                if (!getAllPoints && MCourseByte != Course)
                {
                    if (contur[contur.Length - 1] != prevPoint)
                    {
                        Array.Resize(ref contur, contur.Length + 1);
                        contur[contur.Length - 1] = prevPoint;
                    }
                }
                prevPoint = new System.Drawing.Point(Xcurs, Ycurs);
                #endregion
            } while (iter < limitIter);
            return contur;
            //}
        }
        //-------------------------------------------------------------------------
        private Rectangle GetRectangle(Point[] curvePoints, out Point leftAnchor, int incrWidth = 0, int incrHeight = 0)
        //, int incrWidth = 1, int incrHeight = 1//???
        {
            leftAnchor = new System.Drawing.Point(int.MaxValue, 0);
            if (curvePoints.Length == 0)
            {
                return new Rectangle();
            }
            int x = curvePoints[0].X, width = curvePoints[0].X
              , y = curvePoints[0].Y, height = curvePoints[0].Y;
            int leftY = 0;

            for (int k = 1; k < curvePoints.Length; k++)
            {
                System.Drawing.Point p = curvePoints[k];
                if (p.X < x)
                {
                    x = p.X;
                    leftY = p.Y;
                }
                else if (p.X > width)
                {
                    width = p.X;
                }
                if (p.Y < y)
                {
                    y = p.Y;
                }
                else if (p.Y > height)
                {
                    height = p.Y;
                }
                if (leftAnchor.X > p.X)
                {
                    leftAnchor = p;
                }
            }
            width = width - x + incrWidth;
            height = height - y + incrHeight;
            return new Rectangle(x, y, width, height);
        }
        //-------------------------------------------------------------------------
        private Rectangle GetRectangle(ref Point[] curvePoints, int factor)
        {
            if (curvePoints.Length == 0)
            {
                return new Rectangle();
            }
            System.Drawing.Point p = curvePoints[0];
            p.X /= factor;
            p.Y /= factor;
            curvePoints[0] = p;

            int x = curvePoints[0].X, width = curvePoints[0].X
              , y = curvePoints[0].Y, height = curvePoints[0].Y;
            int leftY = 0;

            for (int k = 1; k < curvePoints.Length; k++)
            {
                p = curvePoints[k];
                p.X /= factor;
                p.Y /= factor;
                curvePoints[k] = p;
                if (p.X < x)
                {
                    x = p.X;
                    leftY = p.Y;
                }
                else if (p.X > width)
                {
                    width = p.X;
                }
                if (p.Y < y)
                {
                    y = p.Y;
                }
                else if (p.Y > height)
                {
                    height = p.Y;
                }
            }
            width = width - x;// + 1
            height = height - y;// + 1
            return new Rectangle(x, y, width, height);
        }
        //-------------------------------------------------------------------------
        private Rectangle GetRectangle(Point[] curvePoints, int addSize = 0)
        {
            if (curvePoints.Length == 0)
            {
                return new Rectangle();
            }
            int x = curvePoints[0].X, width = curvePoints[0].X
              , y = curvePoints[0].Y, height = curvePoints[0].Y;
            int leftY = 0;
            for (int k = 1; k < curvePoints.Length; k++)
            {
                System.Drawing.Point p = curvePoints[k];
                if (p.X < x)
                {
                    x = p.X;
                    leftY = p.Y;
                }
                else if (p.X > width)
                {
                    width = p.X;
                }
                if (p.Y < y)
                {
                    y = p.Y;
                }
                else if (p.Y > height)
                {
                    height = p.Y;
                }
            }
            width = width - x + addSize;
            height = height - y + addSize;
            return new Rectangle(x, y, width, height);
        }
        //-------------------------------------------------------------------------
        private Rectangle VerticalBarcodeDetect
            (
              Bitmap bmp
            , int x
            , int y
            , int barcodeHeight
            , bool bilinear = true
            , double brightness = .95
            , int maxCount = 2
            )
        {
            Rectangle barcode = Rectangle.Empty;
            Color argbWhite = Color.FromArgb(255, 255, 255);
            Color c = argbWhite;
            Point[] curvePoints;
            const int factor = 12;
            barcodeHeight /= factor;//4
            int count = 0;
            int bottomBorder = 0;
            Bitmap b2 = null;
            try
            {
                b2 = new Bitmap(bmp.Width / factor, bmp.Height / factor, PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(b2))
                {
                    if (bilinear)
                        g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                    else
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(bmp, 0, 0, b2.Width, b2.Height);
                }
                //b2.Save("test2.bmp", ImageFormat.Bmp);
                x = x / factor; y = y / factor;
                int yn = y;
                int dist = yn - barcodeHeight * 8;
                if (dist < 0)
                    dist = 0;
                if (x > b2.Width)
                    x = b2.Width - 10;
                if (y < 0)
                    y = 0;
                //try
                //{
                c = b2.GetPixel(x, y);
                //}
                //catch (Exception)
                //{
                //}
                if (c.GetBrightness() < brightness)//(c != argbWhite)
                {
                    while ((c.GetBrightness() < brightness && y > 0) || count < maxCount)//c != argbWhite
                    {
                        y--;
                        //if (y == dist / 2)
                        //    return new Rectangle();
                        //try
                        //{
                        c = b2.GetPixel(x, y);
                        if (c.GetBrightness() >= brightness)
                            count++;
                        else
                            count = 0;
                        //}
                        //catch (Exception)
                        //{
                        //}

                        //b2.SetPixel(x, y, Color.Red);
                        //b2.Save("test2.bmp", ImageFormat.Bmp);
                    }
                    y += count;//y++;
                }
                else
                {
                    dist = yn + barcodeHeight * 16;
                    if (dist > b2.Height)
                        dist = b2.Height - 1;
                    while ((c.GetBrightness() >= brightness && y < b2.Height) || count < maxCount)
                    {
                        y++;
                        if (y > dist)
                            return new Rectangle();
                        //try
                        //{
                        c = b2.GetPixel(x, y);

                        //double b = c.GetBrightness();

                        if (c.GetBrightness() < brightness)
                            count++;
                        else
                            count = 0;
                        //}
                        //catch (Exception)
                        //{
                        //}
                        //b2.SetPixel(x, y, Color.Red);
                        //b2.Save("test2.bmp", ImageFormat.Bmp);
                    }
                    y -= count;
                }
                count = 0;
                int y2 = y;
                while ((c.GetBrightness() < brightness && y2 < b2.Height) || count < maxCount)//c != argbWhite
                {
                    y2++;
                    if (y2 >= b2.Height)
                        return new Rectangle();
                    c = b2.GetPixel(x, y2);
                    if (c.GetBrightness() >= brightness)
                        count++;
                    else
                        count = 0;
                }
                bottomBorder = y2 - count;
                while (y < b2.Height)
                {
                    if (y >= b2.Height)
                        return new Rectangle();
                    curvePoints = ContourFindSpeed(ref b2, x, y, 0, false, true, false, contourMaxLength, brightness);
                    barcode = GetRectangle(curvePoints);
                    if (barcode.Height > barcodeHeight - barcodeHeight / 2)
                        break;
                    y++;
                }

                //curvePoints = ContourFindSpeed(b2, x, y, 2, false, true, false, contourMaxLength, brightness);
                //barcode = GetRectangle(curvePoints);
                if (bottomBorder > barcode.Y)
                    barcode = new Rectangle(barcode.X - 1, barcode.Y, barcode.Width + 2, bottomBorder - barcode.Y);
                else
                    barcode = new Rectangle(barcode.X - 1, barcode.Y, barcode.Width + 2, barcode.Height);
                int area = barcode.Width * barcode.Height;
                while (area <= barcodeHeight && barcode.Bottom < b2.Height && y < b2.Height - 1)
                {
                    y++;
                    //try
                    //{
                    c = b2.GetPixel(x, y);
                    //}
                    //catch (Exception)
                    //{
                    //}
                    while (c.GetBrightness() > brightness && y < b2.Height - 1)
                    {
                        //b2.SetPixel(x, y, Color.Red);
                        //b2.Save("ContourFind.bmp", ImageFormat.Bmp);  
                        y++;
                        if (y > dist)
                            return new Rectangle();
                        if (y == b2.Height - 1)
                            return new Rectangle();
                        //try
                        //{
                        c = b2.GetPixel(x, y);
                        //}
                        //catch (Exception)
                        //{
                        //}
                    }
                    curvePoints = ContourFindSpeed(ref b2, x, y, 2, false, true, false, contourMaxLength, brightness);
                    if (curvePoints.Length == 0 || curvePoints.Length == contourMaxLength)//
                        //{
                        continue;
                    //return new Rectangle();
                    //}
                    barcode = GetRectangle(curvePoints);
                    area = barcode.Width * barcode.Height;
                }
                //if (barcode.Y < y)
                //    barcode = new Rectangle(barcode.X, y, barcode.Width, barcode.Height - (y - barcode.Y));

                //using (Graphics g = Graphics.FromImage(b2))
                //{
                //    g.DrawRectangle(new Pen(Color.Red), barcode);
                //}
                //b2.Save("VerticalBarcode.bmp", ImageFormat.Bmp);

                barcode = MultiplyRectangle(barcode, factor);
                return (new Rectangle
                    (
                      barcode.X
                    , barcode.Y - VerticalBarcodeBorder
                    , barcode.Width + 10
                    , barcode.Height + VerticalBarcodeBorder * 2
                    ));
                //return (new Rectangle(barcode.X + 5, barcode.Y - 10, barcode.Width - 10, barcode.Height + 20));
                //return (new Rectangle(barcode.X - 10, barcode.Y - 10, barcode.Width + 20, barcode.Height + 20));
            }
            catch (Exception)
            {
                return new Rectangle();
            }
            finally
            {
                b2.Dispose();
            }

        }
        //-------------------------------------------------------------------------
        private Rectangle HorisontalBarcodeDetect
            (
              Bitmap bmp
            , int x
            , int y
            , int barcodeHeight
            , bool bilinear = true
            , double brightness = .89
            )
        {
            Rectangle barcode = Rectangle.Empty;
            Point[] curvePoints;
            int factor = 12;
            barcodeHeight /= factor;
            //barcodeHeight = barcodeHeight / factor * 2;
            int dist = 0;
            int count = 0;
            int maxCount = 2;
            int rightBorder = 0;
            Bitmap b2 = null;
            try
            {
                b2 = new Bitmap(bmp.Width / factor, bmp.Height / factor, PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(b2))
                {
                    if (bilinear)
                        g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                    else
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(bmp, 0, 0, b2.Width, b2.Height);
                }

                x = x / factor;
                //x++;
                y = y / factor;
                int xn = x;
                Color c = b2.GetPixel(x, y);

                //b2.SetPixel(x, y, Color.Red);
                //b2.Save("test2.bmp", ImageFormat.Bmp);

                dist = xn - barcodeHeight * 4;
                if (dist < 0)
                    dist = 0;

                //bool blackPxFound = false;
                //if (c.GetBrightness() < brightness)//(c != argbWhite)
                //{
                //    while ((c.GetBrightness() < brightness && x > 0) || count < maxCount)//c != argbWhite
                //    {
                //        x--;
                //        if (x == dist)
                //            return new Rectangle();
                //        c = b2.GetPixel(x, y);
                //        if (c.GetBrightness() >= brightness)
                //            count++;
                //        else
                //            count = 0;

                //        //b2.SetPixel(x, y, Color.Red);
                //        //b2.Save("test2.bmp", ImageFormat.Bmp);
                //    }
                //    if (count >= maxCount)
                //    {
                //        blackPxFound = true;  
                //    }
                //    x += count;//x++;
                //}


                //if (blackPxFound)//(c != argbWhite)

                if (c.GetBrightness() < brightness)//(c != argbWhite)
                {
                    while ((c.GetBrightness() < brightness && x > 0) || count < maxCount)//c != argbWhite
                    {
                        x--;
                        if (x == dist)
                            return new Rectangle();
                        c = b2.GetPixel(x, y);
                        if (c.GetBrightness() >= brightness)
                            count++;
                        else
                            count = 0;

                        //b2.SetPixel(x, y, Color.Red);
                        //b2.Save("test2.bmp", ImageFormat.Bmp);
                    }
                    x += count;//x++;
                }
                else
                {
                    dist = xn + barcodeHeight * 4;
                    if (dist > b2.Width)
                        dist = b2.Width - 1;
                    while ((c.GetBrightness() > brightness && x < b2.Width) || count < maxCount)
                    {
                        x++;
                        if (x > dist)
                            return new Rectangle();
                        c = b2.GetPixel(x, y);
                        if (c.GetBrightness() <= brightness)
                            count++;
                        else
                            count = 0;

                        //b2.SetPixel(x, y, Color.Red);
                        //b2.Save("test2.bmp", ImageFormat.Bmp);

                    }
                    x -= count;
                }
                count = 0;
                int x2 = x;
                while ((c.GetBrightness() < brightness && x2 < b2.Width) || count < maxCount)//c != argbWhite
                {
                    x2++;
                    if (x2 >= b2.Width)
                        return new Rectangle();
                    c = b2.GetPixel(x2, y);
                    if (c.GetBrightness() >= brightness)
                        count++;
                    else
                        count = 0;
                }
                rightBorder = x2 - count;

                while (x < b2.Width)
                {
                    if (x >= b2.Width)
                        return new Rectangle();
                    curvePoints = ContourFindSpeed(ref b2, x, y, 0, false, true, false, contourMaxLength, brightness);
                    barcode = GetRectangle(curvePoints);
                    if (barcode.Height > barcodeHeight - barcodeHeight / 2)
                        break;
                    x++;
                }

                //if ()
                //    continue;

                if (barcode.Right < rightBorder)
                    barcode = new Rectangle(barcode.X, barcode.Y - 1, rightBorder - barcode.X, barcode.Height + 2);
                else
                    barcode = new Rectangle(barcode.X, barcode.Y - 1, barcode.Width, barcode.Height + 2);

                int area = barcode.Width * barcode.Height;
                dist = xn + barcodeHeight * 4;
                if (dist > b2.Width)
                    dist = b2.Width - 1;
                while (area <= barcodeHeight && barcode.Right < b2.Width)
                {
                    x++;
                    c = b2.GetPixel(x, y);
                    while (c.GetBrightness() > brightness && x < b2.Width)
                    {
                        x++;
                        if (x > dist)
                            return new Rectangle();
                        //if (x == b2.Width - 1)
                        //{
                        //    return new Rectangle();
                        //}
                        c = b2.GetPixel(x, y);
                    }
                    curvePoints = ContourFindSpeed(ref b2, x, y, 0, false, true, false, contourMaxLength, brightness);
                    if (curvePoints.Length == contourMaxLength)//curvePoints.Length == 0 ||  
                        continue;
                    //{
                    //    return new Rectangle();
                    //}
                    barcode = GetRectangle(curvePoints);
                    area = barcode.Width * barcode.Height;
                }
                //if (barcode.X < x)
                //    barcode = new Rectangle(x, barcode.Y, barcode.Width - (x - barcode.X), barcode.Height);

                //using (Graphics g = Graphics.FromImage(b2))
                //{
                //    g.DrawRectangle(new Pen(Color.Red), barcode);
                //}
                //b2.Save("HorisontalBarcode.bmp", ImageFormat.Bmp);

                barcode = MultiplyRectangle(barcode, factor);
                return (new Rectangle(barcode.X - 20, barcode.Y, barcode.Width + 40, barcode.Height));
                //return (new Rectangle(barcode.X - 10, barcode.Y + 10, barcode.Width + 20, barcode.Height - 10));
                //return (new Rectangle(barcode.X - 10, barcode.Y - 10, barcode.Width + 20, barcode.Height + 20));
            }
            catch (Exception)
            {
                return new Rectangle();
            }
            finally
            {
                b2.Dispose();
            }
        }
        //-------------------------------------------------------------------------
        private struct AnchorPoints
        {
            public Point LeftTop { get; set; }
            public Point LeftBottom { get; set; }
            public Point RightTop { get; set; }
            public Point RightBottom { get; set; }
            public Point LeftMinValue { get; set; }
        }
        //-------------------------------------------------------------------------
        private AnchorPoints GetAnchorPoints(Point[] curvePoints, Rectangle rectangle)
        {
            AnchorPoints anchorPoints = new AnchorPoints();
            anchorPoints.LeftTop = new System.Drawing.Point(0, int.MaxValue);
            anchorPoints.RightTop = new System.Drawing.Point(int.MaxValue, 0);
            anchorPoints.RightBottom = new System.Drawing.Point(0, 0);
            anchorPoints.LeftBottom = new System.Drawing.Point(0, 0);
            anchorPoints.LeftMinValue = new System.Drawing.Point(int.MaxValue, 0);
            foreach (System.Drawing.Point item in curvePoints)
            {
                if (item.X == rectangle.X && anchorPoints.LeftTop.Y > item.Y)//
                {
                    anchorPoints.LeftTop = item;//pLT
                }
                if (item.Y == rectangle.Y && anchorPoints.RightTop.X > item.X)//
                {
                    anchorPoints.RightTop = item;// pRT = item;
                }
                if (item.X == rectangle.Right && anchorPoints.RightBottom.Y < item.Y)//
                {
                    anchorPoints.RightBottom = item;//pRB = item;
                }
                if (item.Y == rectangle.Bottom && anchorPoints.LeftBottom.X < item.X)//
                {
                    anchorPoints.LeftBottom = item;//pLB = item;
                }
                if (anchorPoints.LeftMinValue.X > item.X)
                {
                    anchorPoints.LeftMinValue = item;
                }
            }
            return anchorPoints;
        }
        //-------------------------------------------------------------------------
        public Bitmap CopyBitmap(Bitmap srcBitmap, Rectangle section)
        {
            //if (srcBitmap.VerticalResolution != 96 || srcBitmap.HorizontalResolution != 96)
            //    srcBitmap.SetResolution(96, 96);

            if (section.Size == new Size())
                section.Size = new Size(1, 1);
            if (section.X < 0)
                section.X = 0;
            else if (section.X >= srcBitmap.Width)
                section.X = srcBitmap.Width - section.Width;
            if (section.Y < 0)
                section.Y = 0;
            else if (section.Y >= srcBitmap.Height)
                section.Y = srcBitmap.Height - section.Height;
            if (section.Right > srcBitmap.Width)
                section = new Rectangle(section.X, section.Y, srcBitmap.Width - section.X, section.Height);
            if (section.Bottom > srcBitmap.Height)
                section = new Rectangle(section.X, section.Y, section.Width, srcBitmap.Height - section.Y);
            if (section.Width <= 0 || section.Height <= 0)
                section.Size = new Size(1, 1);
            // Вырезаем выбранный кусок картинки
            using (Bitmap bmp = new Bitmap(section.Width, section.Height, PixelFormat.Format24bppRgb))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.DrawImage(srcBitmap, 0, 0, section, GraphicsUnit.Pixel);

                    //bmp.Save("section.bmp", ImageFormat.Bmp);

                }//Возвращаем кусок картинки.
                return (Bitmap)bmp.Clone();
            }
        }
        //-------------------------------------------------------------------------
        private System.Drawing.Point ConturNextStep
            (
              ref Bitmap bmp
            , ref Bitmap entryBitmap
            , Color initialColor
            , ref int Xcurs
            , ref int Ycurs
            , ref int Course
            )
        {
            System.Drawing.Point point = new System.Drawing.Point(Xcurs, Ycurs);
            Color c;
            Color argbWhite = Color.FromArgb(255, 255, 255);
            int MCourseByte = Course;
            byte i; int XSc1 = 0, YSc1 = 0;
            Course = (Course + 4) % 8; //инверсия курса кольцевой счетчик 
            for (i = 0; i < 8; i++)
            {
                Course = (Course + 1) % 8;//шаг курса
                switch (Course)
                {
                    case 0://вперёд вниз
                        XSc1 = 1; YSc1 = 1;
                        break;
                    case 1://вниз
                        XSc1 = 0; YSc1 = 1;
                        break;
                    case 2://назад вниз
                        XSc1 = -1; YSc1 = 1;
                        break;
                    case 3://назад 
                        XSc1 = -1; YSc1 = 0;
                        break;
                    case 4://назад вверх
                        XSc1 = -1; YSc1 = -1;
                        break;
                    case 5://вверх
                        XSc1 = 0; YSc1 = -1;
                        break;
                    case 6://вперёд вверх
                        XSc1 = 1; YSc1 = -1;
                        break;
                    case 7://вперёд
                        XSc1 = 1; YSc1 = 0;
                        break;
                    default:
                        break;
                }
                int nX = Xcurs + XSc1;
                if (nX < 0 || nX >= bmp.Width)
                {
                    continue;
                }
                int nY = Ycurs + YSc1;
                if (nY < 0 || nY >= bmp.Height)
                {
                    if (bmp.Height < entryBitmap.Height)
                    {//не используется?
                        if (nY < 0)
                        {
                            if (Xcurs < bmp.Width - 1)
                            {
                                c = bmp.GetPixel(Xcurs + 1, 0);
                                if (c != argbWhite)//(c == initialColor)
                                {
                                    Course = 7;//вперёд
                                    Xcurs++;
                                    point = new System.Drawing.Point(Xcurs, Ycurs);
                                    return point;
                                }
                                else
                                {
                                    c = bmp.GetPixel(Xcurs + 1, Ycurs + 1);
                                    if (c != argbWhite)//(c == initialColor)
                                    {
                                        Course = 0;//вперёд вниз
                                        Xcurs++;
                                        Ycurs++;
                                        point = new System.Drawing.Point(Xcurs, Ycurs);
                                        return point;
                                    }
                                    else
                                    {//идём по верхнему краю, впереди и внизу нет initialColor

                                        c = bmp.GetPixel(Xcurs, Ycurs + 1);
                                        if (c != argbWhite)//(c == initialColor)
                                        {
                                            Course = 1;//вниз
                                            Ycurs++;
                                            point = new System.Drawing.Point(Xcurs, Ycurs);
                                            return point;
                                        }
                                        //continue;
                                        c = bmp.GetPixel(Xcurs - 1, Ycurs + 1);
                                        if (c != argbWhite)//(c == initialColor)
                                        {
                                            Course = 2;//назад вниз
                                            Xcurs--;
                                            Ycurs++;
                                            point = new System.Drawing.Point(Xcurs, Ycurs);
                                            return point;
                                        }
                                        Course = 7;
                                        Xcurs++;
                                        point = new System.Drawing.Point(Xcurs, Ycurs);
                                        return point;
                                    }
                                }
                            }
                            else
                            {//не используется?
                                if (Ycurs < bmp.Height - 1)
                                //    c = bmp.GetPixel(Xcurs - 1, 1);
                                //if (c != argbWhite)//(c == initialColor)
                                {
                                    Course = 1;//вниз
                                    Ycurs++;
                                    point = new System.Drawing.Point(Xcurs, Ycurs);
                                    return point;
                                }
                                else
                                {
                                    Course = 3;//назад 
                                    Xcurs--;
                                    point = new System.Drawing.Point(Xcurs, Ycurs);
                                    return point;
                                }
                            }
                        }
                        else
                        {
                            if (Xcurs == 0)
                            {
                                Course = 5;//вверх 
                                Ycurs--;
                                point = new System.Drawing.Point(Xcurs, Ycurs);
                                return point;
                            }
                            if (Xcurs > 0)
                            {
                                c = bmp.GetPixel(Xcurs - 1, Ycurs);
                                if (c != argbWhite)//(c == initialColor)
                                {
                                    Course = 3;//назад 
                                    Xcurs--;
                                    point = new System.Drawing.Point(Xcurs, Ycurs);
                                    return point;
                                }
                                else
                                {
                                    c = bmp.GetPixel(Xcurs - 1, Ycurs - 1);
                                    if (c != argbWhite)//(c == initialColor)
                                    {
                                        Course = 4;//назад вверх
                                        Xcurs--;
                                        Ycurs--;
                                        point = new System.Drawing.Point(Xcurs, Ycurs);
                                        return point;
                                    }
                                    else
                                    {
                                        c = bmp.GetPixel(Xcurs, Ycurs - 1);
                                        if (c != argbWhite)//(c == initialColor)
                                        {
                                            Course = 5;//вверх 
                                            Ycurs--;
                                            point = new System.Drawing.Point(Xcurs, Ycurs);
                                            return point;
                                        }
                                        if (Xcurs < bmp.Width - 1)
                                        {
                                            c = bmp.GetPixel(Xcurs + 1, Ycurs - 1);
                                            if (c != argbWhite)//(c == initialColor)
                                            {
                                                Course = 6;//вперёд вверх
                                                Xcurs++;
                                                Ycurs--;
                                                point = new System.Drawing.Point(Xcurs, Ycurs);
                                                return point;
                                            }
                                        }
                                    }
                                }
                                Course = 3;//назад 
                                Xcurs--;
                                point = new System.Drawing.Point(Xcurs, Ycurs);
                                return point;
                            }
                            c = bmp.GetPixel(Xcurs, Ycurs - 1);
                            if (c != argbWhite)//(c == initialColor)
                            {
                                Course = 5;//вверх 
                                Ycurs--;
                                point = new System.Drawing.Point(Xcurs, Ycurs);
                                return point;
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                c = bmp.GetPixel(nX, nY);
                if (c != argbWhite)//(c == initialColor)
                {
                    Xcurs = nX;
                    Ycurs = nY;
                    point = new System.Drawing.Point(Xcurs, Ycurs);
                    return point;
                    //Course 0://вперёд вниз
                    //Course 1://вниз
                    //Course 2://назад вниз
                    //Course 3://назад 
                    //Course 4://назад вверх
                    //Course 5://вверх
                    //Course 6://вперёд вверх
                    //Course 7://вперёд
                }
            }
            return point;
        }
        //-------------------------------------------------------------------------
        private string TextRecognize
            (
              int x1, int y1, int x2, int y2
            , ref Bitmap bmp
            , string type
            , ref Rectangle lastSymbolRectangle
            , double? percent_confident_text_region
            , double percent_confident_text
            , int lim = 0
            , bool filter = true
            , string fontName = "Arial"
            , double filterType = double.MaxValue
            , bool glueFilterUsed = false
            , bool manual = false
            )
        {
            bmp = ConvertTo1Bit(ref bmp);
            Bitmap bmpPres = null;// = (Bitmap)bmp.Clone();

            //bmp.Save("TextRecognize.bmp", ImageFormat.Bmp);

            double percent_confident;

            if (filterType != double.MaxValue && filterType > 10)
                filterType = 0;
            Font font = new Font(fontName, 36, FontStyle.Regular);
            //if (filterType <= .75 && filterType > 0)
            //if (filterType <= 1.5 && filterType > 0)
            if (!glueFilterUsed && filterType < 1 && filterType > 0)
            {
                bmp = GlueFilter(ref bmp, new Rectangle(), 2, true, filterType);//GlueFilter(bmp);
                //glueFilterUsed = true;
                font = new Font(fontName, 36, FontStyle.Bold);
            }
            glueFilterUsed = false;

            //bmp.Save("TextRecognize2.bmp", ImageFormat.Bmp);

            //if (filterType != -1 && filterType != double.MaxValue && (filterType >= .75 || filterType < .7))
            font = new Font(fontName, 36, FontStyle.Bold);

            Brush brush = new SolidBrush(Color.Black);//Albertus Medium Albertus Extra Bold
            Bitmap clipped = null;
            if (percent_confident_text_region != null)
                percent_confident = (double)percent_confident_text_region / 100;
            else
                percent_confident = percent_confident_text / 100;
            try
            {
                string[] recognArr;
                switch (type)
                {
                    case "numbersText":
                        recognArr = recognNumb;
                        break;
                    case "capitalText":
                        recognArr = recognNumbAndBigLat;
                        break;
                    default:
                        recognArr = recognNumb;//symbolsBig;
                        break;
                }
                Rectangle rn = new Rectangle(x1, y1, x2 - x1, y2 - y1);

                int k = 0, y;
                string text = "";
                //bool textIsVertical = rn.Width > rn.Height;
                //Rectangle prevRectangle= Rectangle.Empty;
                Color color;
                Color argbWhite = Color.FromArgb(255, 255, 255);
                bool raspFilterUsed = false;
                int raspFilterBound = -1;
                y = bmp.Height / 2;// -rn.Height;
                int dist;
                if (!manual && bmp.Width > bmp.Height)
                    dist = bmp.Width / 2;
                else
                    dist = bmp.Width - 1;
                Rectangle r = Rectangle.Empty;
                x2 = dist;
                x1 = 0;
                while (k <= dist && k < bmp.Width)
                {
                    #region
                    if (token.IsCancellationRequested)
                        return null;
                    Point[] contour = new Point[0];
                    r = Rectangle.Empty;
                    Point p;

                    for (int i = 0; i < 3; i++)
                    {
                        switch (i)
                        {
                            case 1:
                                y -= rn.Height / 4;
                                break;
                            case 2:
                                y += rn.Height / 2;
                                break;
                        }
                        for (k = x1; k < x2; k++)
                        {
                            if (k >= bmp.Width)
                                break;
                            color = bmp.GetPixel(k, y);
                            //bmp.SetPixel(k, y, Color.Red);
                            //bmp.Save("TextRecognize.bmp", ImageFormat.Bmp);
                            if (color != argbWhite)
                            {
                                //if (k == 0)
                                //{
                                contour = ContourFind(bmp, k, y);
                                GetOuterContour(bmp, ref contour, ref r, out p);
                                if (r.X == 0)
                                // k = r.Right;
                                {
                                    k++;
                                    r = Rectangle.Empty;
                                    continue;
                                }
                                //}
                                //bmp.Save("TextRecognize.bmp", ImageFormat.Bmp);
                                //contour = ContourFind(bmp, k, y);
                                //if (contour.Length < 4)
                                if (r.Width * r.Height < 20)
                                {
                                    //Bitmap b = (Bitmap)bmp.Clone();
                                    //b.SetPixel(k, y, Color.Red);
                                    //using (Graphics g = Graphics.FromImage(b))
                                    //{
                                    //    //g.DrawPolygon(new Pen(Color.Red), curvePointsRT);
                                    //    g.DrawPolygon(new Pen(Color.Cyan), contour);
                                    //}
                                    //b.Save("TextRecognize3.bmp", ImageFormat.Bmp);
                                    //b.Dispose();
                                    //x1++;
                                    {
                                        k++;
                                        continue;
                                    }
                                }
                                //GetOuterContour(bmp, ref contour, ref r, out p);
                                break;
                            }
                        }
                        if (r != new Rectangle() || text != "")
                            break;
                    }
                    if (r == Rectangle.Empty)
                        break;
                    if (text != "" && r.X <= lastSymbolRectangle.X)
                    {
                        k++;
                        x1 = k;
                        r = Rectangle.Empty;
                        continue;
                    }
                    //if (text == "" && r.Height < rn.Height / 2 - rn.Height / 4)
                    //{//???
                    //    k++;
                    //    x1++;
                    //    continue;
                    //}
                    if (lastSymbolRectangle != new Rectangle()
                        && r.Height < (lastSymbolRectangle.Height - lastSymbolRectangle.Height / 6))
                    {
                        lastSymbolRectangle = Rectangle.Empty;
                        font = new Font(fontName, 36, FontStyle.Bold);
                        if (!glueFilterUsed)
                        {
                            GlueBitmap
                                (
                                  x2
                                , ref bmp
                                , ref rn
                                , ref k
                                , y
                                , ref glueFilterUsed
                                , ref contour
                                , ref r
                                );
                            //continue;
                        }
                        else
                        {
                            x1++;
                            continue;
                        }
                    }
                    else if (lastSymbolRectangle == Rectangle.Empty && !glueFilterUsed)
                    {
                        if (r.Height < rn.Height / 2 - rn.Height / 16)
                        {
                            GlueBitmap
                                (
                                  x2
                                , ref bmp
                                , ref rn
                                , ref k
                                , y
                                , ref glueFilterUsed
                                , ref contour
                                , ref r
                                );
                            //continue;
                        }
                    }

                    if (lastSymbolRectangle == Rectangle.Empty && glueFilterUsed && r.Height < rn.Height / 3)//???
                        break;
                    //r.Width++; r.Height++;
                    if (r.Width > r.Height * 1.5)
                    {
                        if (!raspFilterUsed)
                        {
                            raspFilterUsed = true;
                            bmp = RaspFilter(ref bmp, 3, r, true);

                            //bmp.Save("clipped.bmp", ImageFormat.Bmp);

                            for (k = r.X; k < x2 + rn.Height; k++)
                            {
                                color = bmp.GetPixel(k, y);
                                //bmp.SetPixel(k, y, Color.Green);
                                if (color != argbWhite)
                                {
                                    contour = ContourFind(bmp, k, y);
                                    GetOuterContour(bmp, ref contour, ref r, out p);
                                    break;
                                }
                            }
                        }
                    }//ниже для "Courier"
                    if (raspFilterBound > -1)
                    {
                        if (r.Y >= raspFilterBound)
                        {
                            raspFilterBound = -1;
                            raspFilterUsed = false;
                        }
                    }

                    clip: clipped = new Bitmap(r.Width, r.Height, PixelFormat.Format24bppRgb);
                    try
                    {
                        using (Graphics g2 = Graphics.FromImage(clipped))
                        {
                            g2.Clear(argbWhite);
                            g2.DrawImage(bmp, -r.X, -r.Y);
                        }
                    }
                    catch (Exception) { }

                    //clipped.Save("clipped.bmp", ImageFormat.Bmp);

                    if (filter && lastSymbolRectangle != new Rectangle()
                        && (lastSymbolRectangle.Height + lastSymbolRectangle.Height / 8
                        ) < clipped.Height)
                    {
                        if (filter && raspFilterUsed)
                            return text;

                        raspFilterBound = r.Bottom;
                        raspFilterUsed = true;
                        bmp = RaspFilter(ref bmp, 2, r, true);
                        bmp = RaspFilter(ref bmp, 2, r, true, false);
                        continue;
                    }

                    int count = 1;
                    double d = (double)clipped.Height / clipped.Width;
                    if (d < .75)
                    {
                        d = (double)clipped.Width / clipped.Height;
                        if (lastSymbolRectangle != new Rectangle())
                            d = (double)clipped.Width / (lastSymbolRectangle.Width + lastSymbolRectangle.Width / 16);
                        count = (int)Math.Round(d);
                        if (count == 1 && d > 1)
                            count = 2;
                    }
                    bmpPres = (Bitmap)clipped.Clone();
                    if (filter && d > 1 && !raspFilterUsed)// count > 1
                    {
                        raspFilterUsed = true;
                        bmp = RaspFilter(ref bmp, 2, r, true);
                        bmp = RaspFilter(ref bmp, 2, r, true, false);
                        continue;
                    }
                    int symbolWidth = clipped.Width / count;
                    clipped = new Bitmap(symbolWidth, clipped.Height, PixelFormat.Format24bppRgb);
                    //font = new Font(fontName, 36, FontStyle.Regular);
                    //if (filterType < 1)
                    //{
                    //    font = new Font(fontName, 36, FontStyle.Bold);
                    //}
                    for (int i = 0; i < count; i++)
                    {
                        clipped = CopyBitmap(bmpPres, new Rectangle(new System.Drawing.Point(symbolWidth * i, 0), clipped.Size));
                        int memNumb = 0;
                        double maxProc = 0;


                        for (int ns = 0; ns < recognArr.Length; ns++)
                        {
                            string symbol = recognArr[ns];
                            //if (symbol == "G")
                            //{

                            //}
                            Bitmap bmp2 = new Bitmap(100, 80, PixelFormat.Format24bppRgb);
                            Graphics g2 = Graphics.FromImage(bmp2);
                            g2.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;// SingleBitPerPixelGridFit;
                            g2.Clear(argbWhite);
                            g2.DrawString(symbol, font, brush, 0, 0);

                            //bmp2.Save("DrawString.bmp", ImageFormat.Bmp);

                            Rectangle r2 = new Rectangle(-1, -1, -1, -1);
                            GetBounds(bmp2, ref r2); //r2.Width++;

                            //double kr1 = (double)clipped.Height / clipped.Width;
                            //double kr2 = (double)r2.Height / r2.Width;
                            //double kr = Math.Abs(kr1 - kr2);
                            //if (kr > 1.6)//1
                            //{
                            //    continue;
                            //}
                            Bitmap bmp3 = new Bitmap(r2.Width, r2.Height, PixelFormat.Format24bppRgb);
                            g2 = Graphics.FromImage(bmp3);
                            g2.InterpolationMode = InterpolationMode.NearestNeighbor;
                            g2.DrawImage(bmp2, -r2.X, -r2.Y);
                            bmp2 = new Bitmap(clipped.Width, clipped.Height, PixelFormat.Format24bppRgb);
                            g2 = Graphics.FromImage(bmp2);
                            g2.InterpolationMode = InterpolationMode.NearestNeighbor;
                            //растянуть нарисованный символ до размера вырезанного
                            g2.DrawImage(bmp3, 0, 0, clipped.Width, clipped.Height);//- 1- 1
                            g2.Dispose();

                            //bmp2.Save("bmp2.bmp", ImageFormat.Bmp);
                            //bmp3.Save("bmp3.bmp", ImageFormat.Bmp);
                            //clipped.Save("clipped2.bmp", ImageFormat.Bmp);

                            int matchPics = 0;
                            int blackBmp2 = 0, blackClipped = 0;//
                            for (int m = 0; m < bmp2.Width; m++)
                            {
                                for (int n = 0; n < bmp2.Height; n++)
                                {
                                    color = bmp2.GetPixel(m, n);
                                    if (color != argbWhite)
                                    {
                                        blackBmp2++;
                                    }
                                    Color c = clipped.GetPixel(m, n);
                                    if (c != argbWhite)
                                    {
                                        blackClipped++;
                                    }

                                    if (color == c)//color != argbWhite && 
                                    {
                                        matchPics++;
                                    }
                                    //if (color != argbWhite && c != argbWhite)//color == c)//
                                    //{
                                    //    matchPics++;
                                    //}
                                }
                            }
                            bmp2.Dispose();
                            bmp3.Dispose();
                            //double totalProc = (double)matchPics / blackClipped;
                            //double totalProc = (double)matchPics / blackBmp2;
                            double totalProc = (double)matchPics / (clipped.Width * clipped.Height);//(clipped.Width * clipped.Height) - matchPics blackBmp2
                            //double isBlank = (double)blackClipped / (clipped.Width * clipped.Height);
                            if (maxProc < totalProc)// && isBlank < .6
                            {
                                memNumb = ns;
                                maxProc = totalProc;
                                if (totalProc == 1)
                                    break;
                            }
                            //else if (isBlank >= .6)
                            //{
                            //    //bmpPres.Dispose();
                            //    return "";
                            //}
                        }
                        if (maxProc > percent_confident)
                        {
                            if (recognArr[memNumb] == "I" && type == "numbersText" && !glueFilterUsed)
                            {
                                GlueBitmap
                                    (
                                      x2
                                    , ref bmp
                                    , ref rn
                                    , ref k
                                    , y
                                    , ref glueFilterUsed
                                    , ref contour
                                    , ref r
                                    );
                                goto clip;
                            }

                            text += recognArr[memNumb];
                            if (count > 1 && i == count - 1)
                                raspFilterUsed = false;

                            if (text.Length > 25)
                                return "";

                            if (type == "capitalText" && lim > 0 && text.Length >= lim)
                            {
                                if (Regex.Match(text, @"[\D]").Success)
                                    return text;
                                else
                                    lim++;
                            }
                        }
                        else
                        {
                            lastSymbolRectangle = Rectangle.Empty;
                            return "";
                        }
                    }
                    lastSymbolRectangle = new Rectangle(r.X, r.Y, clipped.Width, clipped.Height);// r;
                    x1 = r.Right + 1;
                    k = x1;
                    x2 = r.Right + clipped.Height;
                    if (x2 >= bmp.Width)
                        x2 = bmp.Width - 1;
                    y = lastSymbolRectangle.Y + lastSymbolRectangle.Height / 2;
                    dist = x2;
                    #endregion
                }
                if (type == "capitalText")
                {
                    if (!Regex.Match(text, @"[\D]").Success)
                    {
                        lastSymbolRectangle = Rectangle.Empty;
                        return "";
                    }
                }
                else if (type == "numbersText" || recognArr == recognNumb)
                {
                    if (Regex.Match(text, @"[\D]").Success)
                    {
                        lastSymbolRectangle = Rectangle.Empty;
                        return "";
                    }
                }
                return text;
            }
            catch (Exception)
            {
                return "";
            }
            finally
            {
                if (bmpPres != null)
                    bmpPres.Dispose();
                try
                {
                    if (clipped != null)
                        clipped.Dispose();
                    font.Dispose();
                    brush.Dispose();
                }
                catch (Exception)
                { }
            }
        }
        //-------------------------------------------------------------------------
        private void GlueBitmap(int x2, ref Bitmap bmp, ref Rectangle rn, ref int k, int y, ref bool glueFilterUsed, ref Point[] contour, ref Rectangle r)
        {
            Color color;
            Color argbWhite = Color.FromArgb(255, 255, 255);

            //bmp.Save("GlueBitmap.bmp", ImageFormat.Bmp);

            if (!glueFilterUsed)
            {
                glueFilterUsed = true;
                bmp = GlueFilter(ref bmp, new Rectangle(), 2, true, .2);

                //bmp.Save("GlueBitmap.bmp", ImageFormat.Bmp);

                for (k = r.X; k < x2 + rn.Height; k++)
                {
                    color = bmp.GetPixel(k, y);
                    //bmp.SetPixel(k, y, Color.Green);
                    if (color != argbWhite)
                    {
                        contour = ContourFind(bmp, k, y);
                        Point p;
                        GetOuterContour(bmp, ref contour, ref r, out p);
                        break;
                    }
                }
            }
        }
        //-------------------------------------------------------------------------
        private void GetOuterContour(Bitmap bmp, ref Point[] contour, ref Rectangle r, out Point p, int contourLength = 20000)
        {
            Color argbWhite = Color.FromArgb(255, 255, 255);
            Color color = argbWhite;

            //System.Drawing.Point p;
            r = GetRectangle(contour, out p);
            //r = GetRectangle(contour);
            try
            {
                color = bmp.GetPixel(p.X - 1, p.Y);
            }
            catch (Exception)
            {
            }
            if (contour.Length < contourLength
                && color != argbWhite)//внутренний контур
            {
                contour = ContourFind
                (
                  bmp
                , p.X
                , p.Y
                , 0//вперёд
                , false
                , true
                , false
                , contourLength
                );
                r = GetRectangle(contour, out p);//, 1, 1
            }
        }
        //-------------------------------------------------------------------------
        private void GetBounds(Bitmap bmp, ref Rectangle r, Rectangle r2 = new Rectangle())
        {
            Color color;
            Color argbWhite = Color.FromArgb(255, 255, 255);
            if (r2 == Rectangle.Empty)
                r2 = new Rectangle(0, 0, bmp.Width, bmp.Height);
            for (int j = r2.X; j < r2.X + r2.Width; j++)
            {
                if (r.X != -1)
                    break;
                for (int l = r2.Y; l < r2.Y + r2.Height; l++)
                {
                    color = bmp.GetPixel(j, l);
                    if (color == argbWhite)
                        continue;
                    else
                    {
                        r.X = j;
                        break;
                    }
                }
            }
            for (int j = r2.Y; j < r2.Y + r2.Height; j++)
            {
                if (r.Y != -1)
                    break;
                for (int l = r2.X; l < r2.X + r2.Width; l++)
                {
                    color = bmp.GetPixel(l, j);
                    if (color == argbWhite)
                        continue;
                    else
                    {
                        r.Y = j;
                        break;
                    }
                }
            }

            for (int l = r2.Y + r2.Height - 1; l >= r2.Y; l--)
            {
                if (r.Height != -1)
                    break;
                for (int j = r2.X; j < r2.X + r2.Width; j++)
                {
                    color = bmp.GetPixel(j, l);
                    if (color == argbWhite)
                        continue;
                    else
                    {
                        r.Height = l - r.Y;// + 1 add+1//???
                        break;
                    }
                }
            }

            for (int j = r2.X + r2.Width - 1; j >= r2.X; j--)
            {
                if (r.Width != -1)
                    break;
                for (int l = r2.Y + r2.Height - 1; l >= r2.Y; l--)
                {
                    color = bmp.GetPixel(j, l);
                    if (color == argbWhite)
                        continue;
                    else
                    {
                        r.Width = j - r.X;// + 1add+1???
                        break;
                    }
                }
            }
        }
        //-------------------------------------------------------------------------
        public Point[] MoveContour(Point[] contour, int deltaX, int deltaY)
        {
            for (int i = 0; i < contour.Length; i++)
            {
                System.Drawing.Point item = contour[i];
                item.X += deltaX;
                item.Y += deltaY;
                contour[i] = item;
            }
            return contour;
        }
        //-------------------------------------------------------------------------
        private Bitmap RestoreBarcode(ref Bitmap bmp, int dist = 0)
        {
            if (dist == 0)
                dist = bmp.Height / 2;
            Color argbWhite = Color.FromArgb(255, 255, 255);
            //bmp.Save("RestoreBarcode.bmp", ImageFormat.Bmp);
            //LockBitmap lockBitmap = new LockBitmap(bmp);
            //lockBitmap.LockBits();
            bool black = false;
            for (int x = 0; x < bmp.Width; x++)
            {
                int prewBlackPoint = 0;
                for (int y = 0; y < bmp.Height; y++)
                {
                    //Color c = lockBitmap.GetPixel(x, y);
                    Color c = bmp.GetPixel(x, y);
                    if (y == bmp.Height - 1)
                    {
                        if (c == argbWhite)
                        {
                            if (bmp.Height - 1 - prewBlackPoint < dist)
                            {
                                for (int y2 = prewBlackPoint + 1; y2 < y; y2++)
                                    bmp.SetPixel(x, y2, Color.Black);//lockBitmap
                            }
                        }
                    }
                    if (c != argbWhite && !black)
                    {
                        black = true;
                        prewBlackPoint = y;
                    }
                    else
                    {
                        if (black && c != argbWhite && y - prewBlackPoint < dist)
                        {
                            for (int y2 = prewBlackPoint + 1; y2 < y; y2++)
                            {
                                bmp.SetPixel(x, y2, Color.Black);//lockBitmap
                            }
                        }
                        if (c != argbWhite)
                        {
                            prewBlackPoint = y;
                            //black = false;
                        }
                    }
                }
            }
            //lockBitmap.UnlockBits();
            return bmp;
        }
        //-------------------------------------------------------------------------
        //[HandleProcessCorruptedStateExceptions]
        //private Bitmap GlueFilter2
        //    (
        //      ref Bitmap entrybmp
        //    , Rectangle r = new  Rectangle()
        //    , int iter = 2
        //    , bool bw = true
        //    , double filterType = double.MaxValue
        //    )
        //{//Фильтр "клей"
        //    //entrybmp.Save("bmp.bmp", ImageFormat.Bmp);
        //    Bitmap bmp = (Bitmap)entrybmp.Clone();
        //    //using (Graphics g = Graphics.FromImage(lockBitmap))
        //    //{

        //    //}
        //    try
        //    {
        //        if (r == Rectangle.Empty)
        //        {
        //            r.Width = bmp.Width;
        //            r.Height = bmp.Height;
        //        }
        //        using (Bitmap b2 = new Bitmap(bmp.Width * 2, bmp.Height * 2, PixelFormat.Format24bppRgb))
        //        {
        //            for (int i = 0; i < iter; i++)
        //            {
        //                using (Graphics g = Graphics.FromImage(b2))
        //                {
        //                    g.InterpolationMode = InterpolationMode.HighQualityBilinear;
        //                    g.DrawImage(bmp, 0, 0, b2.Width, b2.Height);
        //                }
        //                //b2.Save("b2I" + i + "U1.bmp", ImageFormat.Bmp);
        //                //bmp.Save("bmpI" + i + "U1.bmp", ImageFormat.Bmp);
        //                using (Graphics g = Graphics.FromImage(bmp))
        //                {
        //                    g.InterpolationMode = InterpolationMode.HighQualityBilinear;
        //                    g.DrawImage(b2, 0, 0, bmp.Width, bmp.Height);
        //                }
        //                //b2.Save("b2I" + i + "U2.bmp", ImageFormat.Bmp);
        //                //bmp.Save("bmpI" + i + "U2.bmp", ImageFormat.Bmp);
        //                using (Graphics g = Graphics.FromImage(b2))
        //                {
        //                    g.InterpolationMode = InterpolationMode.Bilinear;
        //                    g.DrawImage(bmp, 0, 0, b2.Width, b2.Height);
        //                }
        //                using (Graphics g = Graphics.FromImage(bmp))
        //                {
        //                    g.InterpolationMode = InterpolationMode.Bilinear;
        //                    g.DrawImage(b2, 0, 0, bmp.Width, bmp.Height);
        //                }
        //                try
        //                {
        //                    if (bw)
        //                    {
        //                        double lim = .55;
        //                        if (filterType != double.MaxValue)
        //                        {
        //                            if (filterType > 0 && filterType <= .3)
        //                            {
        //                                lim = .83;
        //                            }
        //                            if (filterType > 0 && filterType < .35)
        //                            {
        //                                lim = .82;
        //                            }
        //                            if (filterType > 0 && filterType <= .375)
        //                            {
        //                                lim = .81;
        //                            }
        //                            if (filterType > 0 && filterType <= .4)
        //                            {
        //                                lim = .78;
        //                            }
        //                            else if (filterType > 0 && filterType < .57)
        //                            {
        //                                lim = .76;
        //                            }
        //                            else if (filterType <= .6)
        //                            {
        //                                lim = .74;
        //                            }
        //                            else if (filterType <= .75)
        //                            {
        //                                lim = .73;
        //                            }
        //                            else if (filterType <= .8)
        //                            {
        //                                lim = .72;//5;
        //                            }
        //                            else if (filterType <= .9)
        //                            {
        //                                //iter = 1;
        //                                lim = .7;
        //                            }
        //                            else if (filterType >= 1)
        //                            {
        //                                bmp = binaryzeMap(bmp, bmp.Width, bmp.Height, 7);
        //                                entrybmp = (Bitmap)bmp.Clone();
        //                                return entrybmp;
        //                            }
        //                        }

        //                        for (int x = 0; x < bmp.Width; x++)
        //                        {
        //                            for (int y = 0; y < bmp.Height; y++)
        //                            {
        //                                if (token.IsCancellationRequested) return entrybmp;
        //                                Color c = bmp.GetPixel(x, y);
        //                                double f = c.GetBrightness();
        //                                if (f > lim)
        //                                {
        //                                    bmp.SetPixel(x, y, Color.White);
        //                                    continue;
        //                                }
        //                                bmp.SetPixel(x, y, Color.Black);
        //                            }
        //                        }
        //                    }
        //                }
        //                catch (Exception)
        //                { }
        //            }
        //        }
        //        entrybmp = (Bitmap)bmp.Clone();
        //    }
        //    catch (Exception ex)
        //    {
        //        var message = "GlueFilter " + "\n" + "<<" + "\n" + Environment.StackTrace.Trim() + "\n" + ">>";
        //        log.LogMessage("___" + message);
        //        log.LogMessage(ex);
        //        new ErrorRestart(KeyProgram.eDoctrinaOcr).SendErrorMail(message);
        //    }
        //    finally
        //    {
        //        //entrybmp.Save("entrybmp2.bmp", ImageFormat.Bmp);
        //        bmp.Dispose();
        //    }
        //    return entrybmp;
        //}
        //-------------------------------------------------------------------------
        [HandleProcessCorruptedStateExceptions]
        private Bitmap GlueFilter
            (
              ref Bitmap entrybmp
            , Rectangle r = new Rectangle()
            , int iter = 2
            , bool bw = true
            , double filterType = double.MaxValue
            )
        {//Фильтр "клей"
            //entrybmp.Save("entrybmp.entrybmp", ImageFormat.Bmp);
            if (r == Rectangle.Empty)
            {
                r.Width = entrybmp.Width;
                r.Height = entrybmp.Height;
            }
            using (Bitmap b2 = new Bitmap(entrybmp.Width * 2, entrybmp.Height * 2, PixelFormat.Format24bppRgb))
            {
                for (int i = 0; i < iter; i++)
                {
                    using (Graphics g = Graphics.FromImage(b2))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                        g.DrawImage(entrybmp, 0, 0, b2.Width, b2.Height);
                    }
                    using (Graphics g = Graphics.FromImage(entrybmp))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                        g.DrawImage(b2, 0, 0, entrybmp.Width, entrybmp.Height);
                    }
                    using (Graphics g = Graphics.FromImage(b2))
                    {
                        g.InterpolationMode = InterpolationMode.Bilinear;
                        g.DrawImage(entrybmp, 0, 0, b2.Width, b2.Height);
                    }
                    using (Graphics g = Graphics.FromImage(entrybmp))
                    {
                        g.InterpolationMode = InterpolationMode.Bilinear;
                        g.DrawImage(b2, 0, 0, entrybmp.Width, entrybmp.Height);
                    }
                    if (bw)
                    {
                        double lim = .55;
                        if (filterType != double.MaxValue)
                        {
                            if (filterType > 0 && filterType <= .3)
                            {
                                lim = .83;
                            }
                            if (filterType > 0 && filterType < .35)
                            {
                                lim = .82;
                            }
                            if (filterType > 0 && filterType <= .375)
                            {
                                lim = .81;
                            }
                            if (filterType > 0 && filterType <= .4)
                            {
                                lim = .78;
                            }
                            else if (filterType > 0 && filterType < .57)
                            {
                                lim = .76;
                            }
                            else if (filterType <= .6)
                            {
                                lim = .74;
                            }
                            else if (filterType <= .75)
                            {
                                lim = .73;
                            }
                            else if (filterType <= .8)
                            {
                                lim = .72;//5;
                            }
                            else if (filterType <= .9)
                            {
                                //iter = 1;
                                lim = .7;
                            }
                            else if (filterType >= 1)
                            {
                                entrybmp = binaryzeMap(entrybmp, entrybmp.Width, entrybmp.Height, 7);
                                return entrybmp;
                            }

                            using (Image<Bgr, Byte> img = new Image<Bgr, Byte>(entrybmp))
                            {
                                //Color argbWhite = Color.FromArgb(255, 255, 255);
                                Bgr white = new Bgr(255, 255, 255);//argbWhite
                                Bgr black = new Bgr(0, 0, 0);//Color.Black
                                for (int x = 0; x < img.Width; x++)
                                {
                                    for (int y = 0; y < img.Height; y++)
                                    {
                                        if (token.IsCancellationRequested) return img.ToBitmap();
                                        Bgr bgr = img[new Point(x, y)];
                                        Color c = Color.FromArgb((int)bgr.Red, (int)bgr.Green, (int)bgr.Blue);
                                        double f = c.GetBrightness();
                                        if (f > lim)
                                        {
                                            img[new Point(x, y)] = white;
                                            continue;
                                        }
                                        img[new Point(x, y)] = black;
                                    }
                                }

                                //img.Save("img.bmp");

                                entrybmp = img.ToBitmap();
                            }
                        }
                    }
                }
            }
            //entrybmp.Save("entrybmp.bmp", ImageFormat.Bmp);
            return entrybmp;
        }
        //-------------------------------------------------------------------------
        private Bitmap RaspFilter
            (
              ref Bitmap bmp
            , int iteration = 1
            , Rectangle r = new Rectangle()
            , bool roughly = false
            , bool vetical = true
            //, double brightness = .88// пока не используется, можно использовать по дефолту brightness = 0 
            )
        {//Фильтр "рашпиль"

            //DateTime dt = DateTime.Now;

            //bmp.Save("RaspFilter.bmp", ImageFormat.Bmp);

            //bmp = ConvertTo1Bit(ref bmp);
            Bgr bgr;
            Color argbWhite = Color.FromArgb(255, 255, 255);
            Bgr white = new Bgr(argbWhite);
            //Color color = Color.FromArgb((int)white.Red, (int)white.Green, (int)white.Blue);
            //Bgr white = (Bgr)(argbWhite); ; // B, G, R
            if (r == Rectangle.Empty)
            {
                r.Width = bmp.Width;
                r.Height = bmp.Height;
            }
            using (Image<Bgr, Byte> img = new Image<Bgr, Byte>(bmp))
            {
                for (int k = 0; k < iteration; k++)
                {
                    if (!vetical)
                    {
                        for (int j = r.Y; j < r.Bottom; j++)
                        {
                            for (int i = r.X; i < r.Right; i++)
                            {
                                if (token.IsCancellationRequested)
                                    return img.ToBitmap(); ;

                                bgr = img[new Point(i, j)];

                                if (bgr.Equals(white))
                                    continue;
                                if (i < img.Width - 1)
                                    bgr = img[new Point(i + 1, j)];
                                if (bgr.Equals(white))
                                {
                                    if (!roughly && i > 0)
                                        bgr = img[new Point(i - 1, j)];
                                    if (bgr.Equals(white))
                                        img[new Point(i, j)] = white;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int j = r.X; j < r.Right; j++)
                        {
                            for (int i = r.Y; i < r.Bottom; i++)
                            {
                                if (token.IsCancellationRequested)
                                    return img.ToBitmap();
                                bgr = img[new Point(j, i)];
                                if (bgr.Equals(white))
                                    continue;

                                if (i < img.Height - 1)
                                {//не достигнут низ картинки
                                    bgr = img[new Point(j, i + 1)];//проверяем следующий lockBitmap
                                    if (bgr.Equals(white))
                                    {
                                        if (!roughly && i > 0)
                                            bgr = img[new Point(j, i - 1)];
                                        if (bgr.Equals(white))
                                            img[new Point(j, i)] = white;
                                    }
                                }//если белый - устанавливаем белым предидущий
                            }
                        }
                    }
                }
                //TimeSpan ts = DateTime.Now - dt;
                //img.Save("RaspFilter2.bmp");
                return img.ToBitmap();
            }
        }
        //-------------------------------------------------------------------------
        public Regions GetRegions(string barcode, List<Regions> regionsList)
        {
            if (!String.IsNullOrWhiteSpace(barcode))
            {
                return regionsList.Find(x => x.SheetIdentifierName.StartsWith(barcode));
            }
            return null;
        }
        //-------------------------------------------------------------------------
        public int GetRegionsIndex(string searchItemName, List<Regions> regionsList)
        {
            if (!String.IsNullOrWhiteSpace(searchItemName))
            {
                return regionsList.FindIndex(x => x.SheetIdentifierName == searchItemName);
            }
            return -1;
        }
        //-------------------------------------------------------------------------
        //[System.Runtime.InteropServices.DllImport("gdi32.dll")]
        //private static extern bool DeleteObject(IntPtr hObject);

        //[System.Runtime.InteropServices.DllImport("user32.dll")]
        //private static extern int InvalidateRect(IntPtr hwnd, IntPtr rect, int bErase);

        //[System.Runtime.InteropServices.DllImport("user32.dll")]
        //private static extern IntPtr GetDC(IntPtr hwnd);

        //[System.Runtime.InteropServices.DllImport("gdi32.dll")]
        //private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        //[System.Runtime.InteropServices.DllImport("user32.dll")]
        //private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        //[System.Runtime.InteropServices.DllImport("gdi32.dll")]
        //private static extern int DeleteDC(IntPtr hdc);

        //[System.Runtime.InteropServices.DllImport("gdi32.dll")]
        //private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        //[System.Runtime.InteropServices.DllImport("gdi32.dll")]
        //private static extern int BitBlt(IntPtr hdcDst, int xDst, int yDst, int w, int h, IntPtr hdcSrc, int xSrc, int ySrc, int rop);
        //private int SRCCOPY = 0x00CC0020;

        //[System.Runtime.InteropServices.DllImport("gdi32.dll")]
        //private static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFO bmi, uint Usage, out IntPtr bits, IntPtr hSection, uint dwOffset);
        //private uint BI_RGB = 0;
        //private uint DIB_RGB_COLORS = 0;
        //[System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
        //private struct BITMAPINFO
        //{
        //    public uint biSize;
        //    public int biWidth, biHeight;
        //    public short biPlanes, biBitCount;
        //    public uint biCompression, biSizeImage;
        //    public int biXPelsPerMeter, biYPelsPerMeter;
        //    public uint biClrUsed, biClrImportant;
        //    [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 256)]
        //    public uint[] cols;
        //}
        //private uint MAKERGB(int r, int g, int b)
        //{
        //    return ((uint)(b & 255)) | ((uint)((r & 255) << 8)) | ((uint)((g & 255) << 16));
        //}
        ////-------------------------------------------------------------------------
        ///// <summary>
        ///// Copies a bitmap into a 1bpp bitmap of the same dimensions, slowly, using code from Bob Powell's GDI+ faq http://www.bobpowell.net/onebit.htm
        ///// </summary>
        ///// <param name="b">original bitmap</param>
        ///// <returns>a 1bpp copy of the bitmap</returns>
        ////-------------------------------------------------------------------------
        //private Bitmap FaqCopyTo1bpp(Bitmap b)
        //{
        //    int w = b.Width, h = b.Height;
        //    Rectangle r = new Rectangle(0, 0, w, h);
        //    if (b.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
        //    {
        //        Bitmap temp = new Bitmap(w, h, PixelFormat.Format32bppPArgb);
        //        Graphics g = System.Drawing.Graphics.FromImage(temp);
        //        g.DrawImage(b, r, 0, 0, w, h, GraphicsUnit.Pixel);
        //        g.Dispose();
        //        b = temp;
        //    }
        //    BitmapData bdat = b.LockBits(r, ImageLockMode.ReadOnly, b.PixelFormat);
        //    Bitmap b0 = new Bitmap(w, h, PixelFormat.Format1bppIndexed);
        //    BitmapData b0dat = b0.LockBits(r, ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);
        //    for (int y = 0; y < h; y++)
        //    {
        //        for (int x = 0; x < w; x++)
        //        {
        //            int index = y * bdat.Stride + (x * 4);
        //            if (Color.FromArgb(Marshal.ReadByte(bdat.Scan0, index + 2), Marshal.ReadByte(bdat.Scan0, index + 1), Marshal.ReadByte(bdat.Scan0, index)).GetBrightness() > 0.5f)
        //            {
        //                int index0 = y * b0dat.Stride + (x >> 3);
        //                byte p = Marshal.ReadByte(b0dat.Scan0, index0);
        //                byte mask = (byte)(0x80 >> (x & 0x7));
        //                Marshal.WriteByte(b0dat.Scan0, index0, (byte)(p | mask));
        //            }
        //        }
        //    }
        //    b0.UnlockBits(b0dat);
        //    b.UnlockBits(bdat);
        //    return b0;
        //}
        //-------------------------------------------------------------------------
        private float GetAngle(double x1, double y1, double x2, double y2, double x3, double y3, string angle = "b")
        {    //angle = getAngle(0, 0, width - 20, 0, width - 20, countWhiteL);
            //angle = -getAngle(0, 0, width - 20, 0, width - 20, countWhiteR);
            double A, B, C, angleA, angleB, angleC;//
            A = Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
            B = Math.Sqrt(Math.Pow((x3 - x2), 2) + Math.Pow((y3 - y2), 2));
            C = Math.Sqrt(Math.Pow((x1 - x3), 2) + Math.Pow((y1 - y3), 2));
            double p = (A + B + C) / 2;
            double S = Math.Sqrt(p * (p - A) * (p - B) * (p - C));
            angleA = Math.Asin((2 * S) / (A * B)) * (180 / Math.PI);
            angleB = Math.Asin((2 * S) / (B * C)) * (180 / Math.PI);
            angleC = 180 - angleA - angleB;
            angleC = Math.Asin((2 * S) / (A * C)) * (180 / Math.PI);
            //Console.Write("Углы треугольника: {0}, {1}, {2}", angleA, angleB, angleC);
            //Console.ReadKey();
            //return (float)angleB;
            switch (angle)
            {
                case "a":
                    return (float)angleA;
                case "b":
                    return (float)angleB;

                default:
                    return (float)angleC;
            }
        }
        //-------------------------------------------------------------------------
        public string BarcodeRecognize
            (
              Bitmap bmp
            , Rectangle r
            , ref double filterType
            , ref string barcodeType
            , bool useRaspFilter = false
            )
        {
            Bitmap bmp2 = null;
            string text = "";
            const int iter = 5;
            //bmp = ConvertTo1Bit(bmp);

            //bmp.Save("ConvertTo1Bit.bmp", ImageFormat.Bmp);

            try
            {
                for (int i = 0; i < iter; i++)
                {
                    text = "";
                    int x = r.X;
                    int y = r.Y;
                    int width = r.Width;
                    int height = r.Height;
                    //Color argbWhite = Color.FromArgb(255, 255, 255);
                    bmp2 = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                    Graphics g0 = Graphics.FromImage(bmp2);
                    g0.DrawImage(bmp, -x, -y);
                    g0.Dispose();

                    //bmp2.Save("Barcode.bmp", ImageFormat.Bmp);

                    bmp2 = ConvertTo1Bit(ref bmp2);

                    //bmp2.Save("Barcode.bmp", ImageFormat.Bmp);
                    SelectBarcode(ref bmp2);

                    //bmp2.Save("Barcode.bmp", ImageFormat.Bmp);

                    //if (i == -1)
                    if (useRaspFilter)
                    {
                        if (i == 4)
                            //{
                            //    binaryzeMap(bmp2, bmp2.Width, bmp2.Height, 5);
                            RestoreBarcode(ref bmp2, bmp2.Height / 4);
                        //}
                        if (filterType == -1 || filterType == 0 || filterType > 1.2)//> 2 || filterType == 0
                        {
                            bmp2 = RaspFilter(ref bmp2, 2, new Rectangle(), true);
                            GetFilterType(ref filterType, ref bmp2);
                            if (filterType == -1)
                                filterType = 2;
                            if (filterType > 0 && filterType <= 1.15)
                                bmp2 = RestoreBarcode(ref bmp2);
                        }
                        else
                        {
                            if (i == 0 && filterType > 1)
                                bmp2 = RaspFilter(ref bmp2, 2, new Rectangle());
                            else if (i == 2)
                            {
                                bmp2 = RaspFilter(ref bmp2, 2, new Rectangle(), true);
                                bmp2 = RaspFilter(ref bmp2, 3, new Rectangle(), true);
                            }
                            else if (i == 3)
                                bmp2 = RaspFilter(ref bmp2, 3, new Rectangle(), true);
                            else
                                if (i == 1)
                                bmp2 = RaspFilter(ref bmp2, 1, new Rectangle());//, true
                            if (i != 2 && i != 4)
                                if (filterType > 0 && filterType <= 1.2)// 1.5 много 1.125 мало
                                    bmp2 = RestoreBarcode(ref bmp2);
                        }
                    }
                    else
                        GetFilterType(ref filterType, ref bmp2);

                    //bmp2.Save("Barcode2.bmp", ImageFormat.Bmp);

                    //Bitmap b = (Bitmap)bmp2.Clone();
                    //bmp2.Dispose();
                    Rectangle rect = new Rectangle(0, 0, bmp2.Width, bmp2.Height);
                    BitmapData bmpData = bmp2.LockBits(rect, ImageLockMode.ReadOnly, bmp2.PixelFormat);
                    IntPtr ptr = bmpData.Scan0;
                    int bytes = Math.Abs(bmpData.Stride) * bmp2.Height;
                    byte[] rgbValues = new byte[bytes];
                    Marshal.Copy(ptr, rgbValues, 0, bytes);
                    LuminanceSource source = new RGBLuminanceSource(rgbValues, bmp2.Width, bmp2.Height);
                    BinaryBitmap bitmap = new BinaryBitmap(new HybridBinarizer(source));
                    ZXing.BarcodeReader reader1 = new ZXing.BarcodeReader();
                    reader1.Options.TryHarder = true;
                    reader1.Options.PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.CODE_39 };
                    result1 = reader1.Decode(rgbValues, bmp2.Width, bmp2.Height, RGBLuminanceSource.BitmapFormat.Unknown);
                    bmp2.UnlockBits(bmpData);
                    //b.Dispose();
                    bmpData = null;
                    bmp2.Dispose();
                    if (result1 != null)
                    {
                        text = result1.Text;
                        //if (barcodeType == "capitalText")
                        //{
                        //    if (!Regex.Match(text, @"[\D]").Success)
                        //        text = "";
                        //}
                        if (barcodeType == "capitalText")
                        {
                            if (!Regex.Match(text, @"[\D]").Success)
                            {
                                barcodeType = text;
                                return "";
                            }
                        }
                        else if (barcodeType == "numbersText")
                        {
                            if (Regex.Match(text, @"[\D]").Success)
                            {
                                if (i < iter - 1)
                                    continue;
                                else
                                    return "";
                            }
                        }
                        if (i == 2)
                            filterType = 2;
                        if (filterType == 0 && i == 0)
                            filterType = 1;

                        return "*" + text + "*";
                    }
                    else
                    {
                        if (!useRaspFilter)
                            return "";
                    }
                    #region //
                    //List<int> barCode = new List<int>();
                    //int[] col = new int[0];
                    //int whiteAll = 0, blackAll = 0;
                    //    //for (int j = 0; j < bmp2.Height; j++)
                    //    //{
                    //    //    isBlackBeg = false;
                    //    //    for (int i = 0; i < bmp2.Width; i++)
                    //    //    {
                    //    //        color = bmp2.GetPixel(i, j);
                    //    //        if (!isBlackBeg && color == argbWhite)
                    //    //        {
                    //    //            continue;
                    //    //        }
                    //    //        else
                    //    //        {
                    //    //            if (color != argbWhite)
                    //    //            {
                    //    //                isBlackBeg = true;

                    //    //            }
                    //    //        }
                    //    //    }
                    //    //}

                    //    for (int l = 0; l < bmp2.Width; l++)
                    //    {
                    //        blackAll = 0; whiteAll = 0;
                    //        for (int k = 0; k < bmp2.Height; k++)
                    //        {
                    //            color = bmp2.GetPixel(l, k);
                    //            if (color == argbWhite)
                    //            {
                    //                whiteAll++;
                    //                continue;
                    //            }
                    //            blackAll++;
                    //        }
                    //        Array.Resize(ref col, col.Length + 1);
                    //        if (whiteAll >= blackAll)
                    //        {
                    //            col[col.Length - 1] = 1;
                    //        }
                    //        else
                    //        {
                    //            col[col.Length - 1] = 0;
                    //        }
                    //    }
                    //    whiteAll = 0; blackAll = 0;
                    //    int middleBs = 0, middleWs = 0, middleBl = 0, middleWl = 0;
                    //    int maxBs = 0, maxWs = 0;
                    //    int beg = 0;
                    //    int ns = 0;
                    //    for (int k = 0; ns < 10; k++)
                    //    {
                    //        if (col[0] == 1)
                    //        {
                    //            col[0] = 0;
                    //            do
                    //            {
                    //                k++;
                    //            } while (col[k] == 1);
                    //            beg = k;
                    //        }
                    //        if (col[k] == 0)
                    //        {
                    //            if (whiteAll > 0)
                    //            {
                    //                ns++;
                    //                switch (ns)
                    //                {
                    //                    case 2:
                    //                        middleWl += whiteAll;
                    //                        break;
                    //                    case 4:
                    //                    case 6:
                    //                    case 8:
                    //                    case 10:
                    //                        middleWs += whiteAll;
                    //                        if (maxWs < whiteAll)
                    //                        {
                    //                            maxWs = whiteAll;
                    //                        }

                    //                        break;
                    //                }
                    //                whiteAll = 0;
                    //            }
                    //            blackAll++;
                    //        }
                    //        else
                    //        {
                    //            if (blackAll > 0)
                    //            {
                    //                ns++;
                    //                switch (ns)
                    //                {
                    //                    case 1:
                    //                    case 3:
                    //                    case 9:
                    //                        middleBs += blackAll;
                    //                        if (maxBs < blackAll)
                    //                        {
                    //                            maxBs = blackAll;
                    //                        }
                    //                        break;
                    //                    case 5:
                    //                    case 7:
                    //                        middleBl += blackAll;
                    //                        break;
                    //                }
                    //                blackAll = 0;
                    //            }
                    //            whiteAll++;
                    //        }

                    //    }
                    //    blackAll = 0; whiteAll = 0;
                    //    bmp2.Dispose();
                    //    ns = 0;
                    //    for (int k = col.Length - 1; ns < 10; k--)
                    //    {
                    //        if (col[col.Length - 1] == 1)
                    //        {
                    //            col[col.Length - 1] = 0;
                    //            do
                    //            {
                    //                k--;
                    //            } while (col[k] == 1);
                    //        }
                    //        if (col[k] == 0)
                    //        {
                    //            if (whiteAll > 0)
                    //            {
                    //                ns++;
                    //                switch (ns)
                    //                {
                    //                    case 2:
                    //                    case 4:
                    //                    case 6:
                    //                    case 10:
                    //                        middleWs += whiteAll;
                    //                        break;
                    //                    case 8:
                    //                        middleWl += whiteAll;
                    //                        break;
                    //                }
                    //                whiteAll = 0;
                    //            }
                    //            blackAll++;
                    //        }
                    //        else
                    //        {
                    //            if (blackAll > 0)
                    //            {
                    //                ns++;
                    //                switch (ns)
                    //                {
                    //                    case 1:
                    //                    case 7:
                    //                    case 9:
                    //                        middleBs += blackAll;
                    //                        break;
                    //                    case 3:
                    //                    case 5:
                    //                        middleBl += blackAll;
                    //                        break;
                    //                }
                    //                blackAll = 0;
                    //            }
                    //            whiteAll++;
                    //        }
                    //    }
                    //    middleBl = (int)Math.Round((double)middleBl / 4);
                    //    middleBs = (int)Math.Round((double)middleBs / 7);
                    //    middleWl = (int)Math.Round((double)middleWl / 2);
                    //    middleWs = (int)Math.Round((double)middleWs / 7);
                    //    if (!useRaspFilter)
                    //    {
                    //        filterType = middleBl / middleBs;
                    //    }
                    //    if (filterType > 6)
                    //    {
                    //        middleBs *= 5;
                    //    }
                    //    else if (filterType > 3)
                    //    {
                    //        middleBs *= 3;
                    //    }
                    //    else
                    //    {
                    //        middleBs *= 2;
                    //    }
                    //    if (middleWl / middleWs > 3)
                    //    {
                    //        middleWs *= 3;
                    //    }
                    //    else
                    //    {
                    //        middleWs *= 2;
                    //    }

                    //    whiteAll = 0; blackAll = 0;

                    //    //for (int k = beg; k < col.Length - 1; k++)
                    //    //{
                    //    //    if (col[k] == 0)
                    //    //    {
                    //    //        if (whiteAll > 0)
                    //    //        {
                    //    //            if (whiteAll > maxWs)//8middleWs
                    //    //            {
                    //    //                barCode.Add(2);//wL
                    //    //            }
                    //    //            else
                    //    //            {
                    //    //                barCode.Add(-2);//wC
                    //    //            }
                    //    //            whiteAll = 0;
                    //    //        }
                    //    //        blackAll++;
                    //    //    }
                    //    //    else
                    //    //    {
                    //    //        if (blackAll > 0)
                    //    //        {
                    //    //            if (blackAll <= maxBs)//middleBs
                    //    //            {
                    //    //                barCode.Add(-1);//bC
                    //    //            }
                    //    //            else
                    //    //            {
                    //    //                barCode.Add(1);//bL
                    //    //            }
                    //    //            blackAll = 0;
                    //    //        }
                    //    //        whiteAll++;
                    //    //    }
                    //    //}
                    //    //if (blackAll > 0)
                    //    //{
                    //    //    if (blackAll <= maxBs)//middleBs
                    //    //    {
                    //    //        barCode.Add(-1);//bC
                    //    //    }
                    //    //    else
                    //    //    {
                    //    //        barCode.Add(1);//bL
                    //    //    }
                    //    //    blackAll = 0;
                    //    //}


                    //    for (int k = beg; k < col.Length - 1; k++)
                    //    {
                    //        if (col[k] == 0)
                    //        {
                    //            if (whiteAll > 0)
                    //            {
                    //                if (whiteAll >= middleWs)//8
                    //                {
                    //                    barCode.Add(2);//wL
                    //                }
                    //                else
                    //                {
                    //                    barCode.Add(-2);//wC
                    //                }
                    //                whiteAll = 0;
                    //            }
                    //            blackAll++;
                    //        }
                    //        else
                    //        {
                    //            if (blackAll > 0)
                    //            {
                    //                if (blackAll <= middleBs)
                    //                {
                    //                    barCode.Add(-1);//bC
                    //                }
                    //                else
                    //                {
                    //                    barCode.Add(1);//bL
                    //                }
                    //                blackAll = 0;
                    //            }
                    //            whiteAll++;
                    //        }
                    //    }
                    //    if (blackAll > 0)
                    //    {
                    //        if (blackAll <= middleBs)
                    //        {
                    //            barCode.Add(-1);//bC
                    //        }
                    //        else
                    //        {
                    //            barCode.Add(1);//bL
                    //        }
                    //        blackAll = 0;
                    //    }
                    //    if (barCode.Count == 0)
                    //    {
                    //        return "";
                    //    }
                    //    string[] value = new string[1] { "" };
                    //    string ins = "";
                    //    for (int k = 0; k < barCode.Count; k++)
                    //    {
                    //        if (value[value.Length - 1].Length == 9)
                    //        {
                    //            Array.Resize(ref value, value.Length + 1);
                    //            value[value.Length - 1] = "";
                    //            continue;
                    //        }
                    //        switch (barCode[k])
                    //        {
                    //            case -1:
                    //                ins = "b";
                    //                break;
                    //            case 1:
                    //                ins = "B";
                    //                break;
                    //            case -2:
                    //                ins = "w";
                    //                break;
                    //            case 2:
                    //                ins = "W";
                    //                break;
                    //            default:
                    //                break;
                    //        }
                    //        value[value.Length - 1] += ins;
                    //    }
                    //    ins = "";
                    //    for (int j = 0; j < value.Length; j++)
                    //    {
                    //        string item = value[j];                             //bWbwBwBwb
                    //        int i = Array.IndexOf(code39Comb, item);
                    //        if (i >= 0)
                    //        {
                    //            ins += code39Symb[i];
                    //        }
                    //        else
                    //        {
                    //            if (j == value.Length - 1) //&& item.StartsWith(code39Comb[code39Comb.Length - 1])                            
                    //            {
                    //                ins += "*";
                    //            }
                    //        }
                    //    }

                    //    //sb.Append(" - stop label\r\n");
                    //    return ins;
                    #endregion
                }
                return "*" + text + "*";
            }
            catch (Exception)
            {
                return "**";
            }
            finally
            {
                if (bmp2 != null)
                    bmp2.Dispose();
            }
        }
        //-------------------------------------------------------------------------
        private int GetUpperBound(Bitmap bmp2)
        {
            //bmp2.Save("BarcodeRes.bmp", ImageFormat.Bmp);
            Color argbWhite = Color.FromArgb(255, 255, 255);
            int black = 0;
            int whiteAll = 0, blackAll = 0;

            for (int j = 0; j < bmp2.Height; j++)
            {
                black = 0;
                for (int i = 0; i < bmp2.Width; i++)
                {
                    Color c = bmp2.GetPixel(i, j);
                    if (c != argbWhite)
                    {
                        black++;
                        blackAll++;
                    }
                    else
                    {
                        whiteAll++;
                    }
                    if (i == bmp2.Width - 1)
                    {
                        double proc = (double)black / bmp2.Width;
                        if (proc >= .25)
                            return j;
                    }
                }
            }
            return bmp2.Width;
        }
        //-------------------------------------------------------------------------
        private bool SelectBarcode(ref Bitmap bmp2)
        {
            //bmp2.Save("BarcodeRes.bmp", ImageFormat.Bmp);
            if (bmp2.Width < bmp2.Height)
            {
                bmp2.RotateFlip(RotateFlipType.Rotate90FlipNone);
            }
            Color argbWhite = Color.FromArgb(255, 255, 255);
            int black = 0, y1 = 0, y2 = 0;
            int whiteAll = 0, blackAll = 0;
            y1 = GetUpperBound(bmp2);
            if (y1 == bmp2.Width)
                return false;
            bool y1set = false, y2set = false;
            //for (int k = 0; k < 1; k++)
            //{
            y1set = true;
            for (int j = y1; j < bmp2.Height; j++)
            {
                if (y2set)
                    break;
                black = 0;
                for (int i = 0; i < bmp2.Width; i++)
                {
                    Color c = bmp2.GetPixel(i, j);
                    if (c != argbWhite)
                    {
                        black++;
                        blackAll++;
                    }
                    else
                    {
                        whiteAll++;
                    }
                    if (i == bmp2.Width - 1)
                    {
                        double proc = (double)black / bmp2.Width;
                        if (proc >= .25)
                        {
                            if (y1set)
                                continue;
                            y1 = j;
                            y1set = true;
                        }
                        else
                        {
                            if (y1set)
                            {
                                if (proc < .02)
                                {
                                    y2 = j;
                                    y2set = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            //    double d = (double)whiteAll / blackAll;
            //    if (d > 1.5)
            //    {
            //        bmp2 = binaryzeMap(bmp2, bmp2.Width, bmp2.Height, 7);
            //        //bmp2.Save("BarcodeRes2.bmp", ImageFormat.Bmp);
            //    }
            //    else
            //    {
            //        break;
            //    }
            //}

            if (y1set)
            {
                if (!y2set)
                    y2 = bmp2.Height;
                Bitmap bmp3 = new Bitmap(bmp2.Width, bmp2.Height - y1 - (bmp2.Height - y2), PixelFormat.Format24bppRgb);
                if (bmp3.Height < bmp2.Height / 8)
                {
                    bmp3.Dispose();
                    return false;
                }
                bmp3 = CopyBitmap(bmp2, new Rectangle(0, y1, bmp3.Width, bmp3.Height));
                bmp2 = (Bitmap)bmp3.Clone();
                bmp3.Dispose();
                //bmp2.Save("BarcodeRes.bmp", ImageFormat.Bmp);
            }
            return true;
        }
        //-------------------------------------------------------------------------
        private void GetFilterType(ref double filterType, ref Bitmap bmp2)
        {
            //bmp2.Save("BarcodeRes.bmp", ImageFormat.Bmp);
            if (!SelectBarcode(ref bmp2))
            {
                filterType = 0;
                return;
            }
            Color argbWhite = Color.FromArgb(255, 255, 255);
            int whiteAll = 0, blackAll = 0, lp = 0;
            int black = 0;
            bool br = false;
            for (int j = 0; j < bmp2.Width; j++)
            {
                if (br)
                    break;
                black = 0;
                for (int i = 0; i < bmp2.Height; i++)
                {
                    Color c = bmp2.GetPixel(j, i);
                    if (c != argbWhite)
                    {
                        black++;
                        blackAll++;
                    }
                    else
                    {
                        whiteAll++;
                    }
                    if (i == bmp2.Height - 1)
                    {
                        double proc = (double)black / bmp2.Height;
                        if (proc >= .2)
                        {
                            lp = j;
                            br = true;
                            break;
                        }
                    }
                }
            }
            int rp = bmp2.Width - 1;
            blackAll = 0; br = false;
            for (int j = bmp2.Width - 1; j >= 0; j--)
            {
                if (br)
                    break;
                black = 0;
                for (int i = 0; i < bmp2.Height; i++)
                {
                    Color c = bmp2.GetPixel(j, i);
                    if (c != argbWhite)
                    {
                        black++;
                        blackAll++;
                    }
                    else
                    {
                        whiteAll++;
                    }
                    if (i == bmp2.Height - 1)
                    {
                        double proc = (double)black / bmp2.Height;
                        if (proc >= .2)
                        {
                            rp = j;
                            br = true;
                            break;
                        }
                    }
                }
            }

            List<int> barCode = new List<int>();
            int[] col = new int[0];
            whiteAll = 0; blackAll = 0;
            int white = 0, black2 = 0;
            Color color;
            int strips = 0;
            br = false;
            //bmp2 = binaryzeMap(bmp2, bmp2.Width, bmp2.Height, 3);
            //bmp2.Save("Barcode.bmp", ImageFormat.Bmp);
            for (int l = lp; l < rp; l++)//bmp2.Width
            {
                blackAll = 0; whiteAll = 0;
                for (int k = 0; k < bmp2.Height; k++)
                {
                    color = bmp2.GetPixel(l, k);
                    if (color == argbWhite)
                    {
                        whiteAll++;
                        white++;
                        continue;
                    }
                    blackAll++;
                    black2++;
                }
                Array.Resize(ref col, col.Length + 1);
                if (whiteAll > blackAll)
                {
                    col[col.Length - 1] = 1;
                    if (br)
                    {
                        br = false;
                        strips++;
                        //Bitmap b = (Bitmap)bmp2.Clone();
                        //using (Graphics g = Graphics.FromImage(b))
                        //{
                        //    //g.DrawPolygon(new Pen(Color.Red), curvePointsRT);
                        //    g.DrawLine(new Pen(Color.Cyan), l, 0, l, b.Height);
                        //}
                        //b.Save("strips.bmp", ImageFormat.Bmp);
                        //b.Dispose();
                    }
                }
                else
                {
                    if (!br)
                    {
                        br = true;
                        strips++;
                        //Bitmap b = (Bitmap)bmp2.Clone();
                        //using (Graphics g = Graphics.FromImage(b))
                        //{
                        //    //g.DrawPolygon(new Pen(Color.Red), curvePointsRT);
                        //    g.DrawLine(new Pen(Color.Red), l, 0, l, b.Height);
                        //}
                        //b.Save("strips.bmp", ImageFormat.Bmp);
                        //b.Dispose();
                    }
                    col[col.Length - 1] = 0;
                }
                if (strips == 10)
                {
                    //Bitmap b = (Bitmap)bmp2.Clone();
                    //using (Graphics g = Graphics.FromImage(b))
                    //{
                    //    //g.DrawPolygon(new Pen(Color.Red), curvePointsRT);
                    //    g.DrawRectangle(new Pen(Color.Red), new Rectangle(lp, 0, l - lp, b.Height));
                    //}
                    //b.Save("strips.bmp", ImageFormat.Bmp);
                    //b.Dispose();
                    //double d = (double)white / black2;
                    if (l - lp <= bmp2.Height / 2 - bmp2.Height / 16)//
                    {
                        filterType = (double)(l - lp) / bmp2.Height;
                        filterType *= 1.2;
                        return;
                    }
                }
            }
            //Bitmap b = (Bitmap)bmp2.Clone();
            //using (Graphics g = Graphics.FromImage(b))
            //{
            //    //g.DrawPolygon(new Pen(Color.Red), curvePointsRT);
            //    g.DrawRectangle(new Pen(Color.Red), new Rectangle(lp, 0, bmp2.Height / 4, b.Height));
            //}
            //b.Save("strips.bmp", ImageFormat.Bmp);
            //b.Dispose();
            //double d = (double)white / black2;
            whiteAll = 0; blackAll = 0;
            int middleBs = 0, middleWs = 0, middleBl = 0, middleWl = 0;
            int maxBs = 0, maxWs = 0;
            int beg = 0;
            int ns = 0;
            for (int k = 0; ns < 10; k++)
            {
                if (col[0] == 1)
                {
                    col[0] = 0;
                    do
                    {
                        k++;
                    } while (k < col.Length && col[k] == 1);
                    beg = k;
                }
                if (k == col.Length)
                {
                    break;
                }
                if (col[k] == 0)
                {
                    if (whiteAll > 0)
                    {
                        ns++;
                        switch (ns)
                        {
                            case 2:
                                middleWl += whiteAll;
                                break;
                            case 4:
                            case 6:
                            case 8:
                            case 10:
                                middleWs += whiteAll;
                                if (maxWs < whiteAll)
                                {
                                    maxWs = whiteAll;
                                }

                                break;
                        }
                        whiteAll = 0;
                    }
                    blackAll++;
                }
                else
                {
                    if (blackAll > 0)
                    {
                        ns++;
                        switch (ns)
                        {
                            case 1:
                            case 3:
                            case 9:
                                middleBs += blackAll;
                                if (maxBs < blackAll)
                                {
                                    maxBs = blackAll;
                                }
                                break;
                            case 5:
                            case 7:
                                middleBl += blackAll;
                                break;
                        }
                        blackAll = 0;
                    }
                    whiteAll++;
                }
            }
            blackAll = 0; whiteAll = 0;
            ns = 0;
            for (int k = col.Length - 1; ns < 10; k--)
            {
                if (col[col.Length - 1] == 1)
                {
                    col[col.Length - 1] = 0;
                    do
                    {
                        k--;
                    } while (col[k] == 1);
                }
                if (k == -1)
                {
                    break;
                }
                if (col[k] == 0)
                {
                    if (whiteAll > 0)
                    {
                        ns++;
                        switch (ns)
                        {
                            case 2:
                            case 4:
                            case 6:
                            case 10:
                                middleWs += whiteAll;
                                break;
                            case 8:
                                middleWl += whiteAll;
                                break;
                        }
                        whiteAll = 0;
                    }
                    blackAll++;
                }
                else
                {
                    if (blackAll > 0)
                    {
                        ns++;
                        switch (ns)
                        {
                            case 1:
                            case 7:
                            case 9:
                                middleBs += blackAll;
                                break;
                            case 3:
                            case 5:
                                middleBl += blackAll;
                                break;
                        }
                        blackAll = 0;
                    }
                    whiteAll++;
                }
            }
            middleBl = (int)Math.Round((double)middleBl / 4);
            middleBs = (int)Math.Round((double)middleBs / 7);
            middleWl = (int)Math.Round((double)middleWl / 2);
            middleWs = (int)Math.Round((double)middleWs / 7);
            if (middleWs == 0)
            {
                filterType = 2;
            }
            else
            {
                filterType = (((double)middleBs / (double)middleWs) + ((double)middleBl / (double)middleWl)) / 2;
                //filterType = (double)middleBl / (double)middleWl;
            }
            //if (filterType < .2)
            //    filterType = 2;
        }
        //-------------------------------------------------------------------------
        private void GetSymbolsForRecognition(List<Regions> regionsList)
        {
            List<string> sheets = regionsList.Select(x => x.SheetIdentifierName).ToList();
            List<string> sheetIdentifiersShort = new List<string>();
            limSymbols = (limSymbols > 1) ? limSymbols - 1 : 1;
            do
            {
                limSymbols++;
                sheetIdentifiersShort = new List<string>();
                foreach (var sheetIdentifier in sheets)
                {
                    if (sheetIdentifier.Length >= limSymbols)
                    {
                        string sheetIdentifier3 = "";
                        for (int i = 0; i < sheetIdentifier.Length; i++)
                        {
                            sheetIdentifier3 = sheetIdentifier.Substring(0, limSymbols + i);
                            if (Regex.Match(sheetIdentifier3, @"[\D]").Success)
                            {
                                break;
                            }
                        }
                        sheetIdentifiersShort.Add(sheetIdentifier3);
                    }
                    else
                    {
                        sheetIdentifiersShort.Add(sheetIdentifier);
                    }
                }
            } while (sheetIdentifiersShort.Distinct().Count() != sheetIdentifiersShort.Count);

            List<string> symbols = new List<string>();
            sheetIdentifiersShort.Add("D");//для BIG
            sheetIdentifiersShort.ForEach(x => x.ToList().ForEach(z => symbols.Add(z.ToString())));
            recognNumbAndBigLat = symbols.Distinct().ToArray();
            //Array.Resize(ref recognNumbAndBigLat, recognNumbAndBigLat.Length + 3);
            //recognNumbAndBigLat[recognNumbAndBigLat.Length - 3] = "f";
            //recognNumbAndBigLat[recognNumbAndBigLat.Length - 2] = "l";
            //recognNumbAndBigLat[recognNumbAndBigLat.Length - 1] = "a";
        }
        //-------------------------------------------------------------------------
        //public UInt16 CRC16(byte[] buf, int len)
        //{
        //    UInt16 crc = 0xFFFF;

        //    for (int pos = 0; pos < len; pos++)
        //    {
        //        crc ^= (UInt16)buf[pos];

        //        for (int i = 0; i < 8; i++)
        //        {    // Loop over each bit
        //            if ((crc & 0x0001) == 0x0001)
        //            {
        //                crc >>= 1;
        //                crc ^= 0xA001;
        //            }
        //            else
        //                crc >>= 1;
        //        }
        //    }
        //    return crc;
        //}
        //-------------------------------------------------------------------------
        private UInt16 CRC16(string s)
        {
            UInt16 crc = 0xFFFF;

            for (int pos = 0; pos < s.Length; pos++)
            {
                crc ^= (UInt16)Convert.ToSByte(s[pos]);//byte //(UInt16)buf[pos];

                for (int i = 0; i < 8; i++)
                {    // Loop over each bit
                    if ((crc & 0x0001) == 0x0001)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                        crc >>= 1;
                }
            }
            return crc;
        }
        //-------------------------------------------------------------------------
        //public UInt16 ModRTU_CRC16(byte[] buf, int len)
        //{
        //    UInt16 crc = 0xFFFF;

        //    for (int pos = 0; pos < len; pos++)
        //    {
        //        crc ^= (UInt16)buf[pos];          // XOR byte into least sig. byte of crc

        //        for (int i = 8; i != 0; i--)
        //        {    // Loop over each bit
        //            if ((crc & 0x0001) != 0)
        //            {      // If the LSB is set
        //                crc >>= 1;                    // Shift right and XOR 0xA001
        //                crc ^= 0xA001;
        //            }
        //            else                            // Else LSB is not set
        //                crc >>= 1;                    // Just shift right
        //        }
        //    }
        //    // Note, this number has low and high bytes swapped, so use it accordingly (or swap bytes)
        //    return crc;
        //}
        //-------------------------------------------------------------------------
        public bool IsEmptyScan(Bitmap bmp, double emptyScanDarknessPercent, bool findEmptyScan = true)
        {
            if (!findEmptyScan)
                return false;
            bmp = ResizeBitmap(bmp, 8, InterpolationMode.HighQualityBilinear);//Bilinear
            //bmp.Save("findEmptyScan.bmp", ImageFormat.Bmp);
            int black = 0;
            for (int x = bmp.Width / 16; x < bmp.Width - bmp.Width / 16; x++)
            {
                for (int y = bmp.Height / 16; y < bmp.Height - bmp.Height / 16; y++)
                {
                    Color color = bmp.GetPixel(x, y);
                    float gb = color.GetBrightness();
                    //if (gb < .3)
                    if (color.GetBrightness() < .75)
                        black++;
                }
            }
            double blackPercent = (double)((double)black / ((bmp.Width - bmp.Width / 16) * (bmp.Height - bmp.Height / 16)));
            return (blackPercent * 100 < emptyScanDarknessPercent) ? true : false;
        }
        //-------------------------------------------------------------------------
        Regions GetSheetIdentifierExpress
           (
             ref Bitmap bmp
           , ref Rectangle markerLT
           , ref Rectangle markerRT
           , ref Rectangle markerLB
           , ref Rectangle markerRB
           , ref decimal kx
           , ref decimal ky
           , ref string sheetIdentifier
           , ref string lastSheetIdentifier
           , List<Regions> regionsList
           , ref double filterType
           , ref string barCodesPrompt
           , out Rectangle curRect
           , out Rectangle etRect
           , int deltaY
           , OcrAppConfig defaults
           , ref Rectangle sheetIdentifierBarCodeRectangle
           , bool alignmentOnly
           , bool isRotate = false
           , bool isCut = false
           , bool ShetIdManualySet = false
           )
        {
            Regions regions = null;
            Image<Bgr, Byte> img = null;
            bool halfUsed = false;
            newIter:
            using (Bitmap bmpPres = (Bitmap)bmp.Clone())
            {
                int limSymbols = this.limSymbols;

                Rectangle markerLTet = Rectangle.Empty;
                Rectangle markerRTet = Rectangle.Empty;
                Rectangle markerLBet = Rectangle.Empty;
                Rectangle markerRBet = Rectangle.Empty;

                //Rectangle markerLT= Rectangle.Empty;
                //Rectangle markerRT= Rectangle.Empty;
                //Rectangle markerLB= Rectangle.Empty;
                //Rectangle markerRB= Rectangle.Empty;
                curRect = Rectangle.Empty;
                etRect = Rectangle.Empty;
                //Rectangle prevRectangle= Rectangle.Empty;
                int x1 = 0;
                int y1 = 0;
                int x2 = 0;
                int y2 = 0;
                bool squareCur = false;
                bool squareEt = false;
                bool markerExist = false;
                bool useRaspFilter = false;//, barCodeRecErr = false;
                string barcode = "";
                int deltaX = 0, deltaxEt = 0;
                RegionsArea[] areas = new RegionsArea[0];
                int rotateParameter = 0;
                double heightAndWidthRatioBmp = (double)bmp.Width / bmp.Height;
                double heightAndWidthRatio = (lastSheetIdentifier != "")
                    ? regionsList[GetRegionsIndex(lastSheetIdentifier, regionsList)].heightAndWidthRatio
                    : regionsList[0].heightAndWidthRatio;
                if (((heightAndWidthRatio < 1 && heightAndWidthRatioBmp > 1)
                    || (heightAndWidthRatio > 1 && heightAndWidthRatioBmp < 1)))//!isRotate && 
                {
                    bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    bmpPres.RotateFlip(RotateFlipType.Rotate270FlipNone);// = (Bitmap)bmp.Clone();
                }
                //Result result1 = null;
                //int regionsNumb = -1;
                int lastSheetIdentifierIndex = -1;

                for (int f = 0; f < regionsList.Count; f++)
                {
                    if (lastSheetIdentifier != "")
                    {
                        string ls = lastSheetIdentifier;
                        switch (lastSheetIdentifier)
                        {
                            case "100POINT":
                                lastSheetIdentifierIndex = 0;
                                break;
                            case "BIG":
                            case "SMALL":
                                lastSheetIdentifierIndex = 1;
                                break;
                            case "FLEX":
                                lastSheetIdentifierIndex = 2;
                                break;
                            default:
                                break;
                        }
                        regions = regionsList.First(x => x.SheetIdentifierName == ls);
                    }
                    else
                    {
                        switch (f)
                        {
                            case 0:
                                regions = regionsList.First(x => x.SheetIdentifierName == "100POINT");
                                if (f == lastSheetIdentifierIndex)
                                    continue;
                                break;
                            case 1:
                                if (f == lastSheetIdentifierIndex)
                                    continue;
                                regions = regionsList.First(x => x.SheetIdentifierName == "BIG");
                                break;
                            case 2:
                                f++;
                                if (f == lastSheetIdentifierIndex)
                                    continue;
                                regions = regionsList.First(x => x.SheetIdentifierName == "FLEX");
                                break;
                            default:
                                f++;
                                regions = regionsList.First(x => x.SheetIdentifierName == "FANDP");
                                break;
                        }
                    }

                    //regions = regionsList[f];
                    bool rotate180 = false;
                    if (isRotate)
                        rotate180 = true;
                    Beg:
                    int markerFound = 0;
                    for (int num = 0; num < regions.regions.Length; num++)
                    {
                        #region for
                        if (regions.heightAndWidthRatio != 0 && regions.heightAndWidthRatio > 1
                            && (double)((double)bmp.Width / (double)bmp.Height) < 1)
                            bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);

                        if (token.IsCancellationRequested)
                            return null;

                        //bmp.Save("222.bmp", ImageFormat.Bmp);

                        if (regions.regions[num].active == false)
                            continue;
                        double? percent_confident_text_region = regions.regions[num].percent_confident_text_region;
                        var name = regions.regions[num].name;
                        rotateParameter = regions.regions[num].rotate;
                        string type = regions.regions[num].type;
                        areas = regions.regions[num].areas;
                        RegionsArea arr = areas[0];// regions.regions[num].area;
                        int x = 0, y = 0;

                        if (type == "marker")
                        {
                            #region if
                            switch (name)
                            {
                                case "leftTopBlackBox":
                                    markerLTet = new Rectangle(arr.left, arr.top, arr.width, arr.height);
                                    markerFound++;
                                    break;
                                case "rightTopBlackBox":
                                    markerRTet = new Rectangle(arr.left, arr.top, arr.width, arr.height);
                                    markerFound++;
                                    break;
                                case "leftBottomBlackBox":
                                    markerLBet = new Rectangle(arr.left, arr.top, arr.width, arr.height);
                                    markerFound++;
                                    break;
                                case "rightBottomBlackBox":
                                    markerRBet = new Rectangle(arr.left, arr.top, arr.width, arr.height);
                                    markerFound++;
                                    break;
                                default:
                                    break;
                            }

                            if (markerFound == 4)
                            {
                                #region
                                int markersCount = MarkersFind(bmp, ref markerLT, ref markerRT, ref markerLB, ref markerRB, markerLTet);
                                //bmp.Save("MarkersFind.bmp", ImageFormat.Bmp);

                                if (!markerExist && markersCount == 2
                                    && !halfUsed && !isRotate && !isCut && !ShetIdManualySet)//img == null
                                {
                                    halfUsed = true;
                                    //Exception ex;
                                    if (markerLT != Rectangle.Empty && markerRT != Rectangle.Empty)
                                    {
                                        //bmp = CopyBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height / 2));
                                        bmp = (Bitmap)Bitmap.FromFile(frameFileName);
                                        if (bmp.Width > bmp.Height)
                                        {
                                            bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                        }

                                        //bmp.Save("half.bmp", ImageFormat.Bmp);

                                        img = new Image<Bgr, Byte>(bmp);
                                        if (img.Width > img.Height)
                                            img.ROI = new Rectangle(0, bmp.Height / 2, bmp.Width, bmp.Height / 2);
                                        //}

                                        //img = img.Rotate(180, new Bgr());
                                        bmp = (Bitmap)img.ToBitmap().Clone();
                                        //bmp.Save("half.bmp", ImageFormat.Bmp);
                                        img.Dispose();
                                        //bmp = NormalizeBitmap(bmp, out ex);
                                        bmp = ConvertTo1Bit(ref bmp);
                                        f--;
                                        goto newIter;
                                    }
                                    else if (markerLB != Rectangle.Empty && markerRB != Rectangle.Empty)
                                    {
                                        bmp = (Bitmap)Bitmap.FromFile(frameFileName);
                                        if (bmp.Width > bmp.Height)
                                        {
                                            bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                        }

                                        //bmp.Save("half.bmp", ImageFormat.Bmp);

                                        img = new Image<Bgr, Byte>(bmp);
                                        img.ROI = new Rectangle(0, bmp.Height / 2, bmp.Width, bmp.Height / 2);
                                        bmp = (Bitmap)img.ToBitmap().Clone();
                                        img.Dispose();
                                        //bmp = CopyBitmap(bmp, new Rectangle(0, 0, bmp.Width / 2, bmp.Height));
                                        //bmp.Save("half.bmp", ImageFormat.Bmp);
                                        //bmp = NormalizeBitmap(bmp, out ex);
                                        bmp = ConvertTo1Bit(ref bmp);
                                        //bmp.Save("half.bmp", ImageFormat.Bmp);
                                        f--;
                                        goto newIter;
                                    }
                                }

                                if (markersCount > 2)
                                    markerExist = true;
                                if (!alignmentOnly && markersCount < 3)
                                {
                                    if (lastSheetIdentifier != "")
                                    {//выбрать следующий json
                                        lastSheetIdentifier = "";
                                        f = -1;
                                        break;
                                    }
                                    if (markerExist)
                                    {
                                        barCodesPrompt = "Sheet identifier is not recognized";
                                        return null;
                                    }
                                    barCodesPrompt = (markersCount == 0 && IsEmptyScan(bmp
                                        , defaults.EmptyScanDarknessPercent, defaults.RemoveEmptyScans))
                                        ? "Empty scan" : "Markers not found ";
                                    {
                                        if (halfUsed)
                                        {
                                            bmp = (Bitmap)Bitmap.FromFile(frameFileName);
                                            Exception ex;
                                            bmp = NormalizeBitmap(bmp, out ex);
                                        }
                                        return null;
                                    }
                                }
                                if (defaults.DoNotProcess)
                                    return null;
                                GetRectangles(ref curRect, ref etRect, ref deltaxEt, ref deltaX, markerLT, markerRT, markerRB
                                    , markerLB, markerLTet, markerRTet, markerRBet, markerLBet);
                                if (!alignmentOnly)
                                {
                                    CompareRect(ref markerLTet, ref markerLT, ref markerRT, ref markerRB, ref markerLB, ref squareCur, ref squareEt);

                                    if (squareCur != squareEt)
                                    {//выбрать следующий json
                                        if (lastSheetIdentifier != "")
                                        {
                                            if (lastSheetIdentifier == "FLEX" && (double)((double)bmpPres.Width / (double)bmpPres.Height) > 1)
                                                bmpPres.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                            f = -1;
                                            lastSheetIdentifier = "";
                                        }
                                        bmp = (Bitmap)bmpPres.Clone();

                                        //bmp.Save("222.bmp", ImageFormat.Bmp);

                                        if (MarkersFind(bmp, ref markerLT, ref markerRT, ref markerLB, ref markerRB, markerLTet) < 3)
                                        {
                                            barCodesPrompt = "Markers not found ";
                                            return null;
                                        }
                                        break;
                                    }
                                    if (!squareEt && !squareCur)//для 100POINT
                                    {
                                        if ((markerRT != new Rectangle() && markerRT.Width > markerRT.Height)
                                            || (markerLB != new Rectangle() && markerLB.Width < markerLB.Height))
                                        {
                                            rotate180 = true;
                                            bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                            if (MarkersFind(bmp, ref markerLT, ref markerRT, ref markerLB, ref markerRB, markerLTet) < 3)
                                            {
                                                barCodesPrompt = "Markers not found ";
                                                return null;
                                            }
                                        }
                                    }

                                    GetRectangles(ref curRect, ref etRect, ref deltaxEt, ref deltaX, markerLT, markerRT, markerRB
                                        , markerLB, markerLTet, markerRTet, markerRBet, markerLBet);
                                }
                                if (etRect.Width != 0)
                                    kx = (decimal)curRect.Width / etRect.Width;
                                if (etRect.Height != 0)
                                    ky = (decimal)curRect.Height / etRect.Height;
                                barCodesPrompt = AlignmentSheet
                                    (
                                      ref curRect
                                    , ref etRect
                                    , ref deltaxEt
                                    , ref deltaX
                                    , ref kx
                                    , ref ky
                                    , ref bmp
                                    , bmpPres
                                    , ref markerLT
                                    , ref markerRT
                                    , ref markerRB
                                    , ref markerLB
                                    , markerLTet
                                    , markerRTet
                                    , markerRBet
                                    , markerLBet
                                    , isRotate);
                                if (barCodesPrompt.StartsWith("Alignment"))
                                    return null;

                                if (halfUsed && barCodesPrompt == "Markers not found ")
                                {
                                    bmp = (Bitmap)Bitmap.FromFile(frameFileName);
                                    Exception ex;
                                    bmp = NormalizeBitmap(bmp, out ex);
                                    return null;

                                }
                                if (markerExist && !squareCur && !squareEt)
                                {//для 100POINT
                                    return regions;
                                }

                                if (barCodesPrompt != "")
                                {
                                    bmp = (Bitmap)bmpPres.Clone();
                                    if (!alignmentOnly)
                                    {
                                        if (lastSheetIdentifier != "")
                                        {
                                            lastSheetIdentifier = "";
                                            f = -1;
                                        }
                                        if (MarkersFind(bmp, ref markerLT, ref markerRT, ref markerLB, ref markerRB, markerLTet) < 3)
                                        {
                                            barCodesPrompt = "Markers not found ";
                                            return null;
                                        }
                                        break;
                                    }
                                }
                                if (alignmentOnly)
                                {
                                    Rectangle rArr = Rectangle.Empty;
                                    foreach (var item in regions.regions)
                                    {
                                        if (item.name == "sheetIdentifier")
                                        {
                                            rArr = new Rectangle
                                                (
                                                  item.areas[0].left
                                                , item.areas[0].top
                                                , item.areas[0].width
                                                , item.areas[0].height
                                                );
                                            x1 = curRect.X + (int)Math.Round((decimal)(item.areas[0].left - etRect.X) * kx);
                                            y1 = curRect.Y + (int)Math.Round((decimal)(item.areas[0].top - etRect.Y) * ky);
                                            x2 = curRect.X + (int)Math.Round((decimal)(rArr.Right - etRect.X) * kx);
                                            y2 = curRect.Y + (int)Math.Round((decimal)(rArr.Bottom - etRect.Y) * ky);
                                            Rectangle rn = new Rectangle(x1, y1, x2 - x1, y2 - y1);
                                            sheetIdentifierBarCodeRectangle = rn;// rArr;
                                        }
                                    }
                                    return null;
                                }

                                //if (regionsNumb > -1)
                                //    return regions;
                                CompareRect(ref markerLTet, ref markerLT, ref markerRT, ref markerRB, ref markerLB, ref squareCur, ref squareEt);

                                if (squareCur != squareEt)
                                {//выбрать следующий json
                                    if (lastSheetIdentifier != "")
                                    {
                                        lastSheetIdentifier = "";
                                        f = -1;
                                    }
                                    bmp = (Bitmap)bmpPres.Clone();
                                    if (MarkersFind(bmp, ref markerLT, ref markerRT, ref markerLB, ref markerRB, markerLTet) < 3)
                                    {
                                        barCodesPrompt = "Markers not found ";
                                        return null;
                                    }
                                    break;
                                }
                                #endregion
                            }
                            #endregion
                        }
                        else
                            if (type.StartsWith("barCode"))
                        {
                            #region else if
                            if (name != "sheetIdentifier")
                                continue;

                            Rectangle rArr = new Rectangle(arr.left, arr.top, arr.width, arr.height);
                            x1 = curRect.X + (int)Math.Round((decimal)(arr.left - etRect.X) * kx);
                            y1 = curRect.Y + (int)Math.Round((decimal)(arr.top - etRect.Y) * ky);
                            x2 = curRect.X + (int)Math.Round((decimal)(rArr.Right - etRect.X) * kx);
                            y2 = curRect.Y + (int)Math.Round((decimal)(rArr.Bottom - etRect.Y) * ky);
                            x = (x1 + x2) / 2;
                            y = (y1 + y2) / 2;
                            Rectangle rn = new Rectangle(x1, y1, x2 - x1, y2 - y1);

                            sheetIdentifierBarCodeRectangle = rn;
                            if (rArr.Width > rArr.Height)
                            {
                                #region horisontalBarcode
                                Rectangle r3 = new Rectangle
                                  (
                                    rArr.X - rArr.Width / 4
                                  , rArr.Y - rArr.Height / 2
                                  , rArr.Width * 2
                                  , rArr.Height * 3
                                  );
                                string barcodeType = areas[1].type;
                                if (rotateParameter != 0)
                                {
                                    Bitmap bmp2 = new Bitmap(r3.Width, r3.Height);
                                    bmp2 = CopyBitmap(bmp, r3);
                                    bmp2.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                    barcode = BarcodeRecognize
                                      (
                                         bmp2, new Rectangle(new System.Drawing.Point(), bmp2.Size)
                                       , ref filterType
                                       , ref barcodeType
                                       );
                                    bmp2.Dispose();
                                }
                                else
                                {
                                    barcode = BarcodeRecognize
                                      (
                                         bmp
                                       , r3
                                       , ref filterType
                                       , ref barcodeType
                                       );
                                }
                                barcode = barcode.Trim('*');
                                if (barcode != "")
                                    try
                                    {
                                        regions = regionsList.First(reg => reg.SheetIdentifierName == barcode);
                                    }
                                    catch (Exception)
                                    {
                                        return null;
                                    }

                                if (barcode != "" && regions != null)
                                    return regions;
                                if (!rotate180)
                                {
                                    rotateParameter = 0;
                                    rotate180 = true;
                                    bmp = (Bitmap)bmpPres.Clone();
                                    useRaspFilter = false;
                                    bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                    if (regions.heightAndWidthRatio != 0 && regions.heightAndWidthRatio > 1
                                        && bmp.Width < bmp.Height)//((double)bmp.Width / bmp.Height) < 1)
                                        bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                    // bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);

                                    if (MarkersFind(bmp, ref markerLT, ref markerRT, ref markerLB, ref markerRB, markerLTet) < 3)
                                    {
                                        barCodesPrompt = "Markers not found ";
                                        return null;
                                    }
                                    goto Beg;
                                }
                                else
                                {
                                    if (lastSheetIdentifier != "")
                                    {
                                        if (!isRotate && (lastSheetIdentifier == "FLEX" && (double)((double)bmpPres.Width / (double)bmpPres.Height) > 1))
                                            bmpPres.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                        f = -1;
                                        lastSheetIdentifier = "";
                                    }
                                    bmp = (Bitmap)bmpPres.Clone();

                                    //bmp.Save("222.bmp", ImageFormat.Bmp);

                                    if (MarkersFind(bmp, ref markerLT, ref markerRT, ref markerLB, ref markerRB, markerLTet) < 3)
                                    {
                                        barCodesPrompt = "Markers not found ";
                                        return null;
                                    }
                                    break;
                                }
                                #endregion
                            }
                            else
                            {
                                #region verticalBarcode
                                Rectangle r3 = new Rectangle
                                  (
                                    rn.X - rArr.Width / 4
                                  , rn.Y - rArr.Height / 2
                                  , rn.Width * 2
                                  , rn.Height * 2
                                  );
                                Bitmap bmp3 = new Bitmap(r3.Width, r3.Height, PixelFormat.Format24bppRgb);
                                bmp3 = CopyBitmap(bmp, r3);

                                //bmp3.Save("bmp3.bmp", ImageFormat.Bmp);

                                if (rotateParameter != 0)
                                    bmp3.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                else
                                    bmp3.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                string barcodeType = areas[1].type;
                                barcode = BarcodeRecognize
                                    (
                                      bmp3
                                    , new Rectangle(new System.Drawing.Point(), bmp3.Size)
                                    , ref filterType
                                    , ref barcodeType
                                    , useRaspFilter
                                    );
                                barcode = barcode.Trim('*');
                                //bmp3.Save("VerticalBarcode.bmp", ImageFormat.Bmp);
                                bmp3.Dispose();
                                if (barcode != "")
                                {
                                    try
                                    {
                                        regions = regionsList.First(reg => reg.SheetIdentifierName == barcode);
                                    }
                                    catch (Exception)
                                    {
                                        return null;
                                    }
                                }
                                if (barcode != "" && regions != null)
                                    return regions;
                                if (!rotate180)
                                {
                                    rotate180 = true;
                                    rotateParameter = 0;
                                    bmp = (Bitmap)bmpPres.Clone();
                                    bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);

                                    if (MarkersFind(bmp, ref markerLT, ref markerRT, ref markerLB, ref markerRB, markerLTet) < 3)
                                    {
                                        barCodesPrompt = "Markers not found ";
                                        return null;
                                    }
                                    goto Beg;
                                }
                                else
                                {
                                    if (lastSheetIdentifier != "")
                                    {
                                        f = -1;
                                        lastSheetIdentifier = "";
                                    }
                                    bmp = (Bitmap)bmpPres.Clone();
                                    break;
                                }
                                #endregion
                            }
                            //}
                            #endregion
                        }
                        #endregion
                    }
                }
                return null;
            }
        }
        //-------------------------------------------------------------------------
        private void TrackShot(Image<Bgr, byte> laneBed)//?????
        {
            var laneBedGray = laneBed.Convert<Gray, byte>();

            var circles = laneBedGray.HoughCircles(
                new Gray(150),  //cannyThreshold
                new Gray(75),   //circleAccumulatorThreshold
                2.0,            //Resolution of the accumulator used to detect centers of the circles
                20.0,           //min distance 
                5,              //min radius
                50              //max radius
                )[0];           //Get the circles from the first channel

            foreach (CircleF circle in circles)
                laneBed.Draw(circle, new Bgr(Color.Brown), 2);
        }

        //-------------------------------------------------------------------------
        public Regions GetSheetIdentifier
            (
              ref Bitmap bmp
            , ref decimal kx
            , ref decimal ky
            , ref string sheetIdentifier
            , ref string lastSheetIdentifier
            , List<Regions> regionsList
            , ref double filterType
            , ref string barCodesPrompt
            , out Rectangle curRect
            , out Rectangle etRect
            , int deltaY
            , OcrAppConfig defaults
            , ref Rectangle sheetIdentifierBarCodeRectangle
            , bool alignmentOnly
            , ref string qrCodeText
            , bool isRotate = false
            , bool isCut = false
            , bool ShetIdManualySet = false
            )
        {
            bool halfUsed = false;
            qrCodeText = "";
            repeat:
            Regions regions = null;
            using (Bitmap bmpPres = (Bitmap)bmp.Clone())
            {
                int limSymbols = this.limSymbols;

                Rectangle markerLTet = Rectangle.Empty;
                Rectangle markerRTet = Rectangle.Empty;
                Rectangle markerLBet = Rectangle.Empty;
                Rectangle markerRBet = Rectangle.Empty;

                Rectangle markerLT = Rectangle.Empty;
                Rectangle markerRT = Rectangle.Empty;
                Rectangle markerLB = Rectangle.Empty;
                Rectangle markerRB = Rectangle.Empty;
                curRect = Rectangle.Empty;
                etRect = Rectangle.Empty;
                //Rectangle prevRectangle= Rectangle.Empty;
                Rectangle lastSymbolRectangle = Rectangle.Empty;
                int x1 = 0;
                int y1 = 0;
                int x2 = 0;
                int y2 = 0;
                bool squareCur = false;
                bool squareEt = false;
                bool markerExist = false;
                bool useRaspFilter = false, barCodeRecErr = false;
                string barcode = "";
                int deltaX = 0, deltaxEt = 0;
                RegionsArea[] areas = new RegionsArea[0];
                int rotateParameter = 0;
                double heightAndWidthRatioBmp = (double)bmp.Width / bmp.Height;

                //bmp.Save("GetSheetIdentifier.bmp", ImageFormat.Bmp);// = NormalizeBitmap(bmp, out ex);

                double heightAndWidthRatio = (lastSheetIdentifier != "")
                    ? regionsList[GetRegionsIndex(lastSheetIdentifier, regionsList)].heightAndWidthRatio
                    : regionsList[0].heightAndWidthRatio;
                if (!isRotate && ((heightAndWidthRatio < 1 && heightAndWidthRatioBmp > 1)
                    || (heightAndWidthRatio > 1 && heightAndWidthRatioBmp < 1)))
                {//Bitmap bmp2 = (Bitmap)Bitmap.FromFile(frameFileName); ??? читать QR код из ненормализированного битмапа
                    bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);//bmp2 = ConvertTo1Bit(ref bmp2);
                    bmpPres.RotateFlip(RotateFlipType.Rotate270FlipNone);// = (Bitmap)bmp.Clone();
                }
                Result result1 = null;
                int regionsNumb = -1;
                if (defaults.recQRCode && defaults.SheetIdEnum.Count > 0
                    //&& (string.IsNullOrEmpty(sheetIdentifier) || !alignmentOnly
                    //)
                    )
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Bitmap b;
                        switch (i)
                        {
                            case 0:
                                //b = CopyBitmap(bmp, new Rectangle(bmp.Width - bmp.Width / 8, 0, bmp.Width / 8, bmp.Height / 8));
                                b = CopyBitmap(bmp, new Rectangle(bmp.Width / 2, 0, bmp.Width / 2, bmp.Height / 2));
                                b = ConvertTo1Bit(ref b);
                                break;
                            default:
                                b = CopyBitmap(bmp, new Rectangle(0, bmp.Height / 2, bmp.Width / 2, bmp.Height / 2));
                                b = ConvertTo1Bit(ref b);
                                break;
                        }

                        //b.Save("qr.bmp", ImageFormat.Bmp);

                        //ZXing.BarcodeReader reader = new ZXing.BarcodeReader();
                        //var result = reader.Decode(b);
                        //string[] results = BarcodeLib.BarcodeReader.BarcodeReader.read(b, BarcodeLib.BarcodeReader.BarcodeReader.QRCODE);
                        Rectangle rect = new Rectangle(0, 0, b.Width, b.Height);
                        BitmapData bmpData = b.LockBits(rect, ImageLockMode.ReadOnly, b.PixelFormat);
                        IntPtr ptr = bmpData.Scan0;
                        int bytes = Math.Abs(bmpData.Stride) * b.Height;
                        byte[] rgbValues = new byte[bytes];
                        Marshal.Copy(ptr, rgbValues, 0, bytes);
                        LuminanceSource source = new RGBLuminanceSource(rgbValues, b.Width, b.Height);
                        BinaryBitmap bitmap = new BinaryBitmap(new HybridBinarizer(source));
                        ZXing.BarcodeReader reader1 = new ZXing.BarcodeReader();
                        reader1.Options.TryHarder = true;
                        //reader1.Options.PureBarcode = true;
                        reader1.Options.PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE };

                        //ZXing.QrCode.Internal.Version version;
                        //var version = ZXing.QrCode.Internal.Version.getVersionForNumber(4);
                        //reader1.TryInverted = true;
                        //reader1.TryHarder = false;

                        result1 = reader1.Decode(rgbValues, b.Width, b.Height, RGBLuminanceSource.BitmapFormat.RGB24);
                        //result1 = reader1.Decode(source);
                        b.UnlockBits(bmpData);
                        bmpData = null;
                        b.Dispose();
                        string[] chars = new string[0];
                        string[] vals = new string[0];
                        char crc = 'C';
                        if (result1 != null)
                        {
                            halfUsed = false;
                            //if (result1.ResultPoints[0].Y < result1.ResultPoints[1].Y)
                            if (result1.ResultPoints[0].Y < result1.ResultPoints[1].Y && Math.Abs(result1.ResultPoints[0].Y - result1.ResultPoints[1].Y) > 10)
                            {
                                bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                bmpPres.RotateFlip(RotateFlipType.Rotate270FlipNone);// = (Bitmap)bmp.Clone();
                            }
                            qrCodeText = result1.Text;
                            string s = result1.Text.Substring(0, result1.Text.IndexOf(crc.ToString()));//+1V4
                            UInt16 UInt16CRC16 = CRC16(s);
                            chars = Regex.Split(qrCodeText, "\\d+");
                            vals = Regex.Split(qrCodeText.Remove(0, 1), "\\D+");

                            int id = Array.IndexOf(chars, crc.ToString());
                            if (id < 0)
                            {
                                qrCodeText = "";
                                break;
                            }
                            else
                            {
                                if (vals[id] != UInt16CRC16.ToString())
                                {
                                    qrCodeText = "";
                                    break;
                                }
                            }
                            id = Array.IndexOf(chars, "V");
                            if (id < 0)
                            {
                                qrCodeText = "";
                                break;
                            }
                            id = Convert.ToInt32(vals[id]);
                            if (id != 4)
                            {
                                qrCodeText = "";
                                break;
                            }

                            id = Array.IndexOf(chars, "I");
                            if (id >= 0)
                            {
                                id = Convert.ToInt32(vals[id]);
                                sheetIdentifier = defaults.SheetIdEnum[id];
                                regions = GetRegions(sheetIdentifier, regionsList);
                                regionsNumb = regionsList.IndexOf(regions);
                                alignmentOnly = true;
                            }
                            break;
                        }
                        else if (i > 0 && halfUsed && string.IsNullOrEmpty(qrCodeText))
                        {
                            barCodesPrompt = "Markers not found ";
                            bmp = (Bitmap)Bitmap.FromFile(frameFileName);
                            Exception ex;
                            bmp = NormalizeBitmap(bmp, out ex);
                            return null;
                        }
                    }
                }

                if (regionsNumb > -1)
                {
                    barcode = sheetIdentifier;
                    lastSheetIdentifier = sheetIdentifier;
                }
                string lastSheetIdentifierMem = lastSheetIdentifier;
                if (regions == null)
                {
                    regions = GetSheetIdentifierExpress
                       (
                         ref bmp
                       , ref markerLT
                       , ref markerRT
                       , ref markerLB
                       , ref markerRB
                       , ref kx
                       , ref ky
                       , ref sheetIdentifier
                       , ref lastSheetIdentifier
                       , regionsList
                       , ref filterType
                       , ref barCodesPrompt
                       , out curRect
                       , out etRect
                       , deltaY
                       , defaults
                       , ref sheetIdentifierBarCodeRectangle
                       , alignmentOnly
                       , isRotate
                       , isCut
                       , ShetIdManualySet
                       );
                }
                if (barCodesPrompt == "Markers not found ")
                {
                    return null;
                }
                if (regions != null && regionsNumb == -1)
                {
                    sheetIdentifier = regions.SheetIdentifierName;
                    goto sheetIdentifierFound;
                }
                else
                    lastSheetIdentifier = lastSheetIdentifierMem;

                int lastSheetIdentifierIndex = -1;
                for (int f = 0; f < regionsList.Count; f++)
                {
                    #region for

                    if (lastSheetIdentifier != "")
                    {
                        sheetIdentifier = lastSheetIdentifier;
                        f = GetRegionsIndex(lastSheetIdentifier, regionsList);
                        lastSheetIdentifierIndex = f;
                    }
                    else
                    {
                        if (lastSheetIdentifierIndex == f)
                            continue;
                        if (barcode != "" && barcode == regionsList[f].SheetIdentifierName)
                            break;
                        sheetIdentifier = regionsList[f].SheetIdentifierName;
                        if (sheetIdentifier == lastSheetIdentifier)
                            continue;
                    }
                    //regions = regionsList[f].Clone(regionsList[f]);
                    regions = regionsList[f];
                    //if (regions.SheetIdentifierName == "FLEX")
                    //if (regions.heightAndWidthRatio != 0 && regions.heightAndWidthRatio > 1 && bmp.Width / bmp.Height < 1)
                    //{
                    //    bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    //    //linesPerArea=new linesPerArea[1]{4}
                    //}

                    //double? percent_confident_text_region = regions.percent_confident_text_region;
                    bool rotate180 = false;
                    if (isRotate)
                        rotate180 = true;
                    Beg:
                    int markerFound = 0;
                    for (int num = 0; num < regions.regions.Length; num++)
                    {
                        #region for//!isRotate &&
                        if (!isRotate && ((regions.heightAndWidthRatio != 0
                            && regions.heightAndWidthRatio > 1 && bmp.Width < bmp.Height)
                            || regions.heightAndWidthRatio == 0 && bmp.Width > bmp.Height))
                            bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);

                        if (token.IsCancellationRequested)
                            return null;
                        //bmp.Save("222.bmp", ImageFormat.Bmp);
                        if (regions.regions[num].active == false)
                            continue;
                        double? percent_confident_text_region = regions.regions[num].percent_confident_text_region;
                        var name = regions.regions[num].name;
                        rotateParameter = regions.regions[num].rotate;
                        string type = regions.regions[num].type;
                        areas = regions.regions[num].areas;
                        RegionsArea arr = areas[0];// regions.regions[num].area;
                        int x = 0, y = 0;

                        if (type == "marker")
                        {
                            #region if
                            switch (name)
                            {
                                case "leftTopBlackBox":
                                    markerLTet = new Rectangle(arr.left, arr.top, arr.width, arr.height);
                                    markerFound++;
                                    break;
                                case "rightTopBlackBox":
                                    markerRTet = new Rectangle(arr.left, arr.top, arr.width, arr.height);
                                    markerFound++;
                                    break;
                                case "leftBottomBlackBox":
                                    markerLBet = new Rectangle(arr.left, arr.top, arr.width, arr.height);
                                    markerFound++;
                                    break;
                                case "rightBottomBlackBox":
                                    markerRBet = new Rectangle(arr.left, arr.top, arr.width, arr.height);
                                    markerFound++;
                                    break;
                                default:
                                    break;
                            }
                            //double markerEtWH;//нигде не используется
                            //double markerWH;//нигде не используется
                            //double diff;//нигде не используется

                            if (markerFound == 4)
                            {
                                #region
                                if (halfUsed)
                                {
                                    barCodesPrompt = "Markers not found ";
                                    if (!isRotate)
                                        bmp = (Bitmap)Bitmap.FromFile(frameFileName);
                                    Exception ex;
                                    bmp = NormalizeBitmap(bmp, out ex);

                                    return null;
                                }
                                int markersCount = MarkersFind(bmp, ref markerLT, ref markerRT, ref markerLB, ref markerRB, markerLTet);

                                //bmp.Save("MarkersFind.bmp", ImageFormat.Bmp);

                                if (!alignmentOnly && !markerExist && markersCount == 2
                                    && !halfUsed && !isRotate && !isCut && !ShetIdManualySet)
                                {
                                    halfUsed = true;
                                    alignmentOnly = false;
                                    //Exception ex;
                                    if (markerLT != Rectangle.Empty && markerRT != Rectangle.Empty)
                                    {
                                        //bmp = CopyBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height / 2));
                                        Image<Bgr, Byte> img;
                                        bmp = (Bitmap)Bitmap.FromFile(frameFileName);
                                        if (bmp.Width > bmp.Height)
                                        {
                                            bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                        }

                                        //bmp.Save("half.bmp", ImageFormat.Bmp);

                                        img = new Image<Bgr, Byte>(bmp);
                                        img.ROI = new Rectangle(0, 0, bmp.Width, bmp.Height / 2);
                                        bmp = (Bitmap)img.ToBitmap().Clone();

                                        //bmp.Save("half.bmp", ImageFormat.Bmp);

                                        img.Dispose();
                                        //bmp = NormalizeBitmap(bmp, out ex);
                                        bmp = ConvertTo1Bit(ref bmp);
                                        f--;
                                        goto repeat;//Beg
                                    }
                                    else if (markerLB != Rectangle.Empty && markerRB != Rectangle.Empty)
                                    {
                                        Image<Bgr, Byte> img;
                                        bmp = (Bitmap)Bitmap.FromFile(frameFileName);
                                        if (bmp.Width > bmp.Height)
                                        {
                                            bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                        }

                                        //bmp.Save("half.bmp", ImageFormat.Bmp);

                                        img = new Image<Bgr, Byte>(bmp);
                                        img.ROI = new Rectangle(0, bmp.Height / 2, bmp.Width, bmp.Height / 2);
                                        bmp = (Bitmap)img.ToBitmap().Clone();
                                        img.Dispose();
                                        //bmp.Save("half.bmp", ImageFormat.Bmp);
                                        //bmp = NormalizeBitmap(bmp, out ex);
                                        bmp = ConvertTo1Bit(ref bmp);
                                        //bmp.Save("half.bmp", ImageFormat.Bmp);
                                        f--;
                                        goto repeat;//Beg
                                    }
                                }

                                if (markersCount > 2)
                                    markerExist = true;
                                if (!alignmentOnly && markersCount < 3)
                                {
                                    if (lastSheetIdentifier != "")
                                    {//выбрать следующий json
                                        lastSheetIdentifier = "";
                                        f = -1;
                                        break;
                                    }
                                    if (markerExist)
                                    {
                                        barCodesPrompt = "Sheet identifier is not recognized";
                                        return null;
                                    }
                                    barCodesPrompt = (markersCount == 0 && IsEmptyScan(bmp
                                        , defaults.EmptyScanDarknessPercent, defaults.RemoveEmptyScans))
                                        ? "Empty scan" : "Markers not found ";
                                    {
                                        return null;
                                    }
                                }
                                if (defaults.DoNotProcess)
                                    return null;
                                GetRectangles(ref curRect, ref etRect, ref deltaxEt, ref deltaX, markerLT, markerRT, markerRB
                                    , markerLB, markerLTet, markerRTet, markerRBet, markerLBet);
                                if (!alignmentOnly)
                                {
                                    CompareRect(ref markerLTet, ref markerLT, ref markerRT, ref markerRB, ref markerLB, ref squareCur, ref squareEt);

                                    if (squareCur != squareEt)
                                    {//выбрать следующий json
                                        if (lastSheetIdentifier != "")
                                        {
                                            lastSheetIdentifier = "";
                                            f = -1;
                                        }
                                        //bmp = (Bitmap)bmpPres.Clone();
                                        break;
                                    }
                                    if (!squareEt && !squareCur)
                                    {
                                        if ((markerRT != new Rectangle() && markerRT.Width > markerRT.Height)
                                            || (markerLB != new Rectangle() && markerLB.Width < markerLB.Height))
                                        {
                                            rotate180 = true;
                                            bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                            if (MarkersFind(bmp, ref markerLT, ref markerRT, ref markerLB, ref markerRB, markerLTet) < 3)
                                            {
                                                barCodesPrompt = "Markers not found ";
                                                return null;
                                            }
                                        }
                                    }

                                    GetRectangles(ref curRect, ref etRect, ref deltaxEt, ref deltaX, markerLT, markerRT, markerRB
                                        , markerLB, markerLTet, markerRTet, markerRBet, markerLBet);
                                }
                                if (etRect.Width != 0)
                                    kx = (decimal)curRect.Width / etRect.Width;
                                if (etRect.Height != 0)
                                    ky = (decimal)curRect.Height / etRect.Height;
                                barCodesPrompt = AlignmentSheet
                                    (
                                      ref curRect
                                    , ref etRect
                                    , ref deltaxEt
                                    , ref deltaX
                                    , ref kx
                                    , ref ky
                                    , ref bmp
                                    , bmpPres
                                    , ref markerLT
                                    , ref markerRT
                                    , ref markerRB
                                    , ref markerLB
                                    , markerLTet
                                    , markerRTet
                                    , markerRBet
                                    , markerLBet
                                    , isRotate
                                    );
                                if (barCodesPrompt.StartsWith("Alignment"))
                                    return null;

                                if (barCodesPrompt != "")
                                {
                                    bmp = (Bitmap)bmpPres.Clone();
                                    if (!alignmentOnly)
                                    {
                                        if (lastSheetIdentifier != "")
                                        {
                                            lastSheetIdentifier = "";
                                            f = -1;
                                        }
                                        if (MarkersFind(bmp, ref markerLT, ref markerRT, ref markerLB, ref markerRB, markerLTet) < 3)
                                        {
                                            barCodesPrompt = "Markers not found ";
                                            return null;
                                        }
                                        break;
                                    }
                                }

                                if (regionsNumb > -1)
                                    return regions;

                                if (alignmentOnly)
                                {
                                    Rectangle rArr = Rectangle.Empty;
                                    foreach (var item in regions.regions)
                                    {
                                        if (item.name == "sheetIdentifier")
                                        {
                                            rArr = new Rectangle
                                                (
                                                  item.areas[0].left
                                                , item.areas[0].top
                                                , item.areas[0].width
                                                , item.areas[0].height
                                                );
                                            x1 = curRect.X + (int)Math.Round((decimal)(item.areas[0].left - etRect.X) * kx);
                                            y1 = curRect.Y + (int)Math.Round((decimal)(item.areas[0].top - etRect.Y) * ky);
                                            x2 = curRect.X + (int)Math.Round((decimal)(rArr.Right - etRect.X) * kx);
                                            y2 = curRect.Y + (int)Math.Round((decimal)(rArr.Bottom - etRect.Y) * ky);
                                            Rectangle rn = new Rectangle(x1, y1, x2 - x1, y2 - y1);
                                            sheetIdentifierBarCodeRectangle = rn;// rArr;
                                            break;
                                        }
                                    }
                                    return null;
                                }

                                //if (regionsNumb > -1)
                                //    return regions;
                                CompareRect(ref markerLTet, ref markerLT, ref markerRT, ref markerRB, ref markerLB, ref squareCur, ref squareEt);

                                if (squareCur != squareEt)
                                {//выбрать следующий json
                                    if (lastSheetIdentifier != "")
                                    {
                                        lastSheetIdentifier = "";
                                        f = -1;
                                    }
                                    bmp = (Bitmap)bmpPres.Clone();
                                    if (MarkersFind(bmp, ref markerLT, ref markerRT, ref markerLB, ref markerRB, markerLTet) < 3)
                                    {
                                        barCodesPrompt = "Markers not found ";
                                        return null;
                                    }
                                    break;
                                }
                                #endregion
                            }
                            #endregion
                        }
                        else
                            if (type.StartsWith("barCode"))
                        {
                            #region else if
                            if (name != "sheetIdentifier")
                                continue;
                            barCodeRecErr = false;
                            string barcodeMem = "";

                            Rectangle rArr = new Rectangle(arr.left, arr.top, arr.width, arr.height);
                            x1 = curRect.X + (int)Math.Round((decimal)(arr.left - etRect.X) * kx);
                            y1 = curRect.Y + (int)Math.Round((decimal)(arr.top - etRect.Y) * ky);
                            x2 = curRect.X + (int)Math.Round((decimal)(rArr.Right - etRect.X) * kx);
                            y2 = curRect.Y + (int)Math.Round((decimal)(rArr.Bottom - etRect.Y) * ky);
                            x = (x1 + x2) / 2;
                            y = (y1 + y2) / 2;
                            Rectangle rn = new Rectangle(x1, y1, x2 - x1, y2 - y1);

                            sheetIdentifierBarCodeRectangle = rn;
                            if (rArr.Width > rArr.Height)
                            {
                                #region horisontalBarcode
                                int barcodeHeight = (int)Math.Round((decimal)rArr.Height * ky) * 3 / 4;
                                //Rectangle r = horisontalBarcodeDetect(bmp, x, y, barcodeHeight);
                                Rectangle r = HorisontalBarcodeDetect(bmp, rn.X + rn.Width, y, barcodeHeight);
                                if (Math.Abs(sheetIdentifierBarCodeRectangle.Height - r.Height) > sheetIdentifierBarCodeRectangle.Height / 4)
                                    r = HorisontalBarcodeDetect(bmp, rn.X + rn.Height, y, barcodeHeight, false);

                                if (x <= 0 || x >= bmp.Width
                                    || y <= 0 || y >= bmp.Height
                                    || r.Width <= 0 || r.Width >= bmp.Width
                                    || r.Height <= 0 || r.Height >= bmp.Height)
                                {
                                    if (!rotate180)
                                    {
                                        rotateParameter = 0;
                                        rotate180 = true;
                                        bmp = (Bitmap)bmpPres.Clone();
                                        useRaspFilter = false;
                                        bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);

                                        if (MarkersFind(bmp, ref markerLT, ref markerRT, ref markerLB, ref markerRB, markerLTet) < 3)
                                        {
                                            barCodesPrompt = "Markers not found ";
                                            return null;
                                        }
                                        goto Beg;
                                    }
                                    if (type != "barCode&Text")
                                        break;
                                }
                                else
                                {
                                    if (Math.Abs(sheetIdentifierBarCodeRectangle.Height - r.Height) < sheetIdentifierBarCodeRectangle.Height / 4)
                                        sheetIdentifierBarCodeRectangle = r;// rArr;
                                    else
                                    {
                                        Rectangle r2 = HorisontalBarcodeDetect(bmp, rn.X + rn.Height, y, barcodeHeight, false);
                                        if ((x <= 0 || x >= bmp.Width
                                            || y <= 0 || y >= bmp.Height
                                            || r.Width <= 0 || r.Width >= bmp.Width
                                            || r.Height <= 0 || r.Height >= bmp.Height))
                                        {
                                            r = new Rectangle
                                                          (
                                                            rArr.X - rArr.Width / 4
                                                          , rArr.Y - rArr.Height / 2
                                                          , rArr.Width * 2
                                                          , rArr.Height * 3
                                                          );
                                        }
                                        else
                                            r = r2;
                                        if (Math.Abs(sheetIdentifierBarCodeRectangle.Height - r.Height) < sheetIdentifierBarCodeRectangle.Height / 4)
                                            sheetIdentifierBarCodeRectangle = r;// rArr;
                                        else
                                        {
                                            filterType = -1;
                                            useRaspFilter = true;
                                        }
                                    }
                                    string barcodeType = areas[1].type;
                                    if (rotateParameter != 0)
                                    {
                                        Bitmap bmp2 = new Bitmap(r.Width, r.Height);
                                        bmp2 = CopyBitmap(bmp, r);
                                        bmp2.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                        barcode = BarcodeRecognize
                                          (
                                             bmp2, new Rectangle(new System.Drawing.Point()
                                           , bmp2.Size)
                                           , ref filterType
                                           , ref barcodeType
                                           );
                                        bmp2.Dispose();
                                    }
                                    else
                                    {
                                        barcode = BarcodeRecognize
                                          (
                                             bmp
                                           , r
                                           , ref filterType
                                           , ref barcodeType
                                           );
                                    }
                                    int length = barcode.Length;
                                    barcode = barcode.Trim('*');
                                    if (!string.IsNullOrEmpty(barcode) && barcode == regionsList[f].SheetIdentifierName)
                                    {
                                        regions = regionsList[f];
                                        sheetIdentifier = regionsList[f].SheetIdentifierName;
                                        if (defaults.NotSupportedSheets.Contains(barcode))
                                        {
                                            barCodesPrompt = "Unsupported answer sheet ";
                                            return null;
                                        }
                                        goto sheetIdentifierFound;
                                    }
                                    else
                                    {
                                        if (!Regex.Match(barcode, @"[\D]").Success)
                                        {
                                            //if (lastSheetIdentifier != "")
                                            //{
                                            if (!rotate180)
                                            {
                                                rotate180 = true;
                                                bmp = (Bitmap)bmpPres.Clone();
                                                bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                                goto Beg;
                                            }
                                            //bmp.Dispose();//???
                                            bmp = (Bitmap)bmpPres.Clone();
                                            if (MarkersFind(bmp, ref markerLT, ref markerRT, ref markerLB, ref markerRB, markerLTet) < 3)
                                            {
                                                barCodesPrompt = "Markers not found ";
                                                return null;
                                            }
                                            break;
                                        }
                                        Regions regionsTemp = GetRegions(barcode, regionsList);
                                        if (regionsTemp != null)
                                        {
                                            regions = regionsTemp;
                                            sheetIdentifier = regionsTemp.SheetIdentifierName;
                                            goto sheetIdentifierFound;
                                        }
                                    }
                                    if (GetRegionsIndex(barcode, regionsList) < 0)
                                    {
                                        barcodeType = areas[1].type;
                                        useRaspFilter = true;
                                        if (rotateParameter != 0)
                                        {
                                            Bitmap bmp2 = new Bitmap(r.Width, r.Height, PixelFormat.Format24bppRgb);
                                            bmp2 = CopyBitmap(bmp, r);
                                            bmp2.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                            barcode = BarcodeRecognize
                                              (
                                                 bmp2
                                               , new Rectangle(new System.Drawing.Point()
                                               , bmp2.Size)
                                               , ref filterType
                                               , ref barcodeType
                                               , useRaspFilter
                                               );
                                            bmp2.Dispose();
                                        }
                                        else
                                        {
                                            barcode = BarcodeRecognize
                                           (
                                             bmp
                                           , r
                                           , ref filterType
                                           , ref barcodeType
                                           , useRaspFilter
                                           );
                                        }
                                        barcode = barcode.Trim('*');
                                        if (length > 0 && length - 2 == barcode.Length)
                                        {
                                            if (barcode == regionsList[f].SheetIdentifierName)
                                            {
                                                goto sheetIdentifierFound;
                                            }
                                        }
                                    }
                                    if (type != "barCode&Text")
                                    {
                                        break;
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                #region verticalBarcode
                                Rectangle r3 = new Rectangle
                                  (
                                    rn.X - rArr.Width / 4
                                  , rn.Y - rArr.Height / 2
                                  , rn.Width * 2
                                  , rn.Height * 2
                                  );
                                Bitmap bmp3 = new Bitmap(r3.Width, r3.Height, PixelFormat.Format24bppRgb);
                                bmp3 = CopyBitmap(bmp, r3);
                                if (rotateParameter != 0)
                                    bmp3.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                else
                                    bmp3.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                string barcodeType = areas[1].type;
                                barcode = BarcodeRecognize
                                    (
                                      bmp3
                                    , new Rectangle(new System.Drawing.Point(), bmp3.Size)
                                    , ref filterType
                                    , ref barcodeType
                                    , useRaspFilter
                                    );
                                barcode = barcode.Trim('*');
                                //bmp3.Save("VerticalBarcode.bmp", ImageFormat.Bmp);
                                bmp3.Dispose();
                                if (f > -1 && barcode == regionsList[f].SheetIdentifierName)
                                    goto sheetIdentifierFound;

                                int barcodeHeight = (int)Math.Round((decimal)rArr.Width * kx) * 3 / 4;
                                Rectangle r = Rectangle.Empty;
                                r = VerticalBarcodeDetect(bmp, x, rn.Y + rn.Width, barcodeHeight);

                                //using (Graphics g = Graphics.FromImage(bmp))
                                //{
                                //    g.DrawRectangle(new Pen(Color.Red), r);
                                //}
                                //bmp.Save("VerticalBarcode.bmp", ImageFormat.Bmp);

                                if (Math.Abs(sheetIdentifierBarCodeRectangle.Width - r.Width) - 24 > sheetIdentifierBarCodeRectangle.Width / 4)
                                {//24 добавляет VerticalBarcodeDetect
                                    r = VerticalBarcodeDetect(bmp, x, rn.Y + rn.Width, barcodeHeight, false);
                                }
                                rotate:
                                if (x <= 0 || x >= bmp.Width
                             || y <= 0 || y >= bmp.Height
                             || r.Width <= 0 || r.Width >= bmp.Width
                             || r.Height <= 0 || r.Height >= bmp.Height)
                                {
                                    if (!rotate180)
                                    {
                                        rotate180 = true;
                                        rotateParameter = 0;
                                        bmp = (Bitmap)bmpPres.Clone();
                                        useRaspFilter = false;
                                        if (MarkersFind(bmp, ref markerLT, ref markerRT, ref markerLB, ref markerRB, markerLTet) < 3)
                                        {
                                            barCodesPrompt = "Markers not found ";
                                            return null;
                                        }
                                        GetRectangles(ref curRect, ref etRect, ref deltaxEt, ref deltaX, markerLT, markerRT, markerRB
                                         , markerLB, markerLTet, markerRTet, markerRBet, markerLBet);
                                        if ((etRect.Width > etRect.Height & curRect.Width < curRect.Height)
                                            || (etRect.Width < etRect.Height & curRect.Width > curRect.Height))
                                        {
                                            useRaspFilter = false;
                                            bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);

                                            if (MarkersFind(bmp, ref markerLT, ref markerRT, ref markerLB, ref markerRB, markerLTet) < 3)
                                            {
                                                barCodesPrompt = "Markers not found ";
                                                return null;
                                            }
                                        }
                                        bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);

                                        if (MarkersFind(bmp, ref markerLT, ref markerRT, ref markerLB, ref markerRB, markerLTet) < 3)
                                        {
                                            barCodesPrompt = "Markers not found ";
                                            return null;
                                        }
                                        goto Beg;
                                    }
                                }
                                else
                                {
                                    if (Math.Abs(sheetIdentifierBarCodeRectangle.Width - r.Width) - 24 <= sheetIdentifierBarCodeRectangle.Width / 4)
                                    {
                                        sheetIdentifierBarCodeRectangle = r;
                                        if (rotateParameter == 90)
                                            deltaY = r.Bottom - y2 - VerticalBarcodeBorder;
                                        else
                                            deltaY = r.Y - y1 + VerticalBarcodeBorder;
                                        if (Math.Abs(deltaY) > r.Width * 2)
                                            deltaY = 0;
                                        //deltaY = r.Y - y1;
                                    }
                                    else
                                    {
                                        Rectangle r2 = VerticalBarcodeDetect(bmp, x, rn.Y + rn.Width, barcodeHeight, false);
                                        if (x <= 0 || x >= bmp.Width
                                             || y <= 0 || y >= bmp.Height
                                             || r2.Width <= 0 || r2.Width >= bmp.Width / 3
                                             || r2.Height <= 0 || r2.Height >= bmp.Height / 3)
                                        {
                                            r = new Rectangle
                                                      (
                                                        rArr.X - rArr.Width / 4
                                                      , rArr.Y - rArr.Height / 2
                                                      , rArr.Width * 2
                                                      , rArr.Height * 3
                                                      );
                                        }
                                        else
                                            r = r2;

                                        if (Math.Abs(sheetIdentifierBarCodeRectangle.Width - r.Width) - 24 <= sheetIdentifierBarCodeRectangle.Width / 4)
                                        {
                                            sheetIdentifierBarCodeRectangle = r;
                                            if (rotateParameter == 90)
                                                deltaY = r.Bottom - y2;
                                            else
                                                deltaY = r.Y - y1;
                                            if (Math.Abs(deltaY) > r.Width * 2)
                                                deltaY = 0;//deltaY = r.Y - y1;
                                        }
                                        else
                                        {
                                            filterType = -1;
                                            useRaspFilter = true;
                                            deltaY = 0;
                                        }
                                    }

                                    Bitmap bmp2 = new Bitmap(r.Width, r.Height, PixelFormat.Format24bppRgb);
                                    bmp2 = CopyBitmap(bmp, r);
                                    if (rotateParameter != 0)
                                        bmp2.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                    else
                                        bmp2.RotateFlip(RotateFlipType.Rotate270FlipNone);
                                    barcodeType = areas[1].type;
                                    barcode = BarcodeRecognize
                                        (
                                          bmp2
                                        , new Rectangle(new System.Drawing.Point(), bmp2.Size)
                                        , ref filterType
                                        , ref barcodeType
                                        , useRaspFilter
                                        );
                                    barcode = barcode.Trim('*');
                                    if (f == -1)
                                    {
                                        bmp2.Dispose();
                                        break;
                                    }

                                    if (barcodeType != "capitalText")
                                    {
                                        if (!rotate180)
                                        {
                                            bmp2.Dispose();
                                            r = Rectangle.Empty;
                                            goto rotate;
                                        }
                                    }
                                    if (barcode == regionsList[f].SheetIdentifierName)
                                    {
                                        bmp2.Dispose();
                                        if (defaults.NotSupportedSheets.Contains(barcode))
                                        {
                                            barCodesPrompt = "Unsupported answer sheet ";
                                            return null;
                                        }
                                        goto sheetIdentifierFound;
                                    }
                                    if (GetRegionsIndex(barcode, regionsList) < 0)//barcode != "" && 
                                    {
                                        useRaspFilter = true;
                                        barcodeType = areas[1].type;
                                        barcode = BarcodeRecognize
                                            (
                                             bmp2
                                           , new Rectangle(new System.Drawing.Point(), bmp2.Size)
                                           , ref filterType
                                           , ref barcodeType
                                           , useRaspFilter
                                           );
                                        bmp2.Dispose();
                                        if (barcodeType != "capitalText")
                                        {
                                            if (!rotate180)
                                            {
                                                r = Rectangle.Empty;
                                                goto rotate;
                                            }
                                        }

                                        barcode = barcode.Trim('*');
                                        if (barcode == regionsList[f].SheetIdentifierName)
                                        {
                                            goto sheetIdentifierFound;
                                        }
                                        else
                                            if (barcode != "" && GetRegionsIndex(barcode, regionsList) >= 0)
                                        {
                                            Regions regionsTemp = GetRegions(barcode, regionsList);
                                            if (regionsTemp != null)
                                            {
                                                regions = regionsTemp;
                                                sheetIdentifier = regionsTemp.SheetIdentifierName;
                                                goto sheetIdentifierFound;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Regions regionsTemp = GetRegions(barcode, regionsList);
                                        if (regionsTemp != null)
                                        {
                                            regions = regionsTemp;
                                            sheetIdentifier = regionsTemp.SheetIdentifierName;
                                            goto sheetIdentifierFound;
                                        }

                                        //выбрать следующий json
                                        if (lastSheetIdentifier != "")
                                        {
                                            if (!rotate180)
                                            {
                                                rotate180 = true;
                                                bmp = (Bitmap)bmpPres.Clone();
                                                bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                                goto Beg;
                                            }
                                            else
                                            {
                                                if (lastSheetIdentifier != "")
                                                {
                                                    if (!isRotate && (lastSheetIdentifier == "FLEX"
                                                        && (double)((double)bmpPres.Width / (double)bmpPres.Height) > 1))
                                                        bmpPres.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                                    f = -1;
                                                    lastSheetIdentifier = "";
                                                }
                                                //bmp = (Bitmap)bmpPres.Clone();
                                                //break;
                                            }
                                            lastSheetIdentifier = "";
                                            f = -1;
                                            break;
                                        }
                                        bmp = (Bitmap)bmpPres.Clone();
                                        if (MarkersFind(bmp, ref markerLT, ref markerRT, ref markerLB, ref markerRB, markerLTet) < 3)
                                        {
                                            barCodesPrompt = "Markers not found ";
                                            return null;
                                        }
                                        break;
                                    }
                                    if (type != "barCode&Text")
                                        break;
                                }
                                #endregion
                            }
                            if (type == "barCode&Text")
                            {
                                if (f == -1)
                                {
                                    break;
                                }
                                for (int i = 0; i < 2; i++)
                                {
                                    arr = areas[1];// regions.regions[num + 1].area;
                                    rArr = new Rectangle(arr.left, arr.top, arr.width, arr.height);
                                    x1 = curRect.X + (int)Math.Round((decimal)(arr.left - etRect.X) * kx);
                                    y1 = deltaY + curRect.Y + (int)Math.Round((decimal)(arr.top - etRect.Y) * ky);
                                    x2 = curRect.X + (int)Math.Round((decimal)(rArr.Right - etRect.X) * kx);
                                    y2 = deltaY + curRect.Y + (int)Math.Round((decimal)(rArr.Bottom - etRect.Y) * ky);

                                    Regions reg = regionsList[f];

                                    //bmp.Save("barCodeRecErr.bmp", ImageFormat.Bmp);

                                    //string ss = arr.type;

                                    TextRecognizeTotal
                                   (
                                     bmp
                                   , filterType
                                   , barcodeMem
                                   , x1, x2, y1, y2
                                   , arr.type
                                   , percent_confident_text_region
                                   , defaults.PercentConfidentText
                                   , defaults.FontName
                                   , ref lastSymbolRectangle
                                   , ref barcode
                                   , ref y
                                   , ref arr
                                   , rotateParameter
                                   , limSymbols
                                   , regionsList
                                   );


                                    if (!string.IsNullOrEmpty(barcode) && barcode == regionsList[f].SheetIdentifierName)
                                    {
                                        regions = GetRegions(barcode, regionsList);
                                        barCodeRecErr = false;
                                        goto sheetIdentifierFound;
                                    }
                                    else
                                    {
                                        Regions regionsTemp = null;
                                        if (limSymbols > 0 && barcode != null && barcode.Length >= limSymbols)
                                        {
                                            regionsTemp = GetRegions(barcode, regionsList);
                                        }
                                        if (regionsTemp != null)
                                        {
                                            regions = regionsTemp;
                                            sheetIdentifier = regionsTemp.SheetIdentifierName;
                                            barCodeRecErr = false;
                                            goto sheetIdentifierFound;
                                        }
                                        else
                                            barcode = "";
                                    }
                                    //if (barCodeRecErr && !rotate180)//barcode == ""
                                    //{
                                    useRaspFilter = false;
                                    //rotateParameter = 0;
                                    //rotate180 = true;
                                    bmp = (Bitmap)bmpPres.Clone();
                                    try
                                    {
                                        bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);//GDI+
                                    }
                                    catch (Exception)
                                    {
                                        barCodesPrompt = "Markers not found ";
                                        return null;
                                    }

                                    //bmp.Save("barCodeRecErr.bmp", ImageFormat.Bmp);

                                    if (MarkersFind(bmp, ref markerLT, ref markerRT, ref markerLB, ref markerRB, markerLTet) < 3)
                                    {
                                        barCodesPrompt = "Markers not found ";
                                        return null;
                                    }

                                    GetRectangles(ref curRect, ref etRect, ref deltaxEt, ref deltaX, markerLT, markerRT, markerRB
                                  , markerLB, markerLTet, markerRTet, markerRBet, markerLBet);
                                    if (etRect.Width != 0)
                                        kx = (decimal)curRect.Width / etRect.Width;
                                    if (etRect.Height != 0)
                                        ky = (decimal)curRect.Height / etRect.Height;
                                    barCodesPrompt = AlignmentSheet
                                        (
                                          ref curRect
                                        , ref etRect
                                        , ref deltaxEt
                                        , ref deltaX
                                        , ref kx
                                        , ref ky
                                        , ref bmp
                                        , bmpPres
                                        , ref markerLT
                                        , ref markerRT
                                        , ref markerRB
                                        , ref markerLB
                                        , markerLTet
                                        , markerRTet
                                        , markerRBet
                                        , markerLBet
                                        , isRotate
                                        );
                                    filterType = 0;

                                    //bmp.Save("barCodeRecErr.bmp", ImageFormat.Bmp);

                                    if (barCodesPrompt.StartsWith("Alignment"))
                                        return null;
                                }
                                if (barcode == "")
                                {
                                    barCodeRecErr = true;
                                    //deltaY = 0;
                                }

                                if (lastSheetIdentifier != "")
                                {
                                    lastSheetIdentifier = "";
                                    f = -1;
                                }
                                if (!rotate180)
                                {
                                    rotate180 = true;
                                    bmp = (Bitmap)bmpPres.Clone();
                                    bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                                    goto Beg;
                                }

                                bmp = (Bitmap)bmpPres.Clone();
                                break;
                            }//End if(type == "barCode&Text")
                            #endregion
                        }
                        #endregion
                    }
                    #endregion
                }
                errSheetIdentifier:
                lastSheetIdentifier = "";
                if (barcode != sheetIdentifier)
                {
                    barCodesPrompt = "Sheet identifier is not recognized";
                    return null;
                }
                if (sheetIdentifier == "")
                {
                    barCodesPrompt = "Sheet Identifier not found";
                    return null;
                }
                sheetIdentifierFound:
                if (defaults.DualControl && barCodeRecErr)
                {
                    barcode = " ";
                    goto errSheetIdentifier;
                }
                if (regions != null && defaults.NotSupportedSheets.Contains(regions.SheetIdentifierName))
                {
                    barCodesPrompt = "Unsupported answer sheet ";
                    return null;
                }
                return regions;
            }
        }
        //-------------------------------------------------------------------------
        private void CompareRect
            (
              ref Rectangle markerLTet
            , ref Rectangle markerLT
            , ref Rectangle markerRT
            , ref Rectangle markerRB
            , ref Rectangle markerLB
            , ref bool squareCur
            , ref bool squareEt
            )
        {
            squareEt = IsSquare(markerLTet);
            //squareCur = true;
            for (int i = 0; i < 5; i++)
            {
                if (markerLT != new Rectangle())
                {
                    squareCur = IsSquare(markerLT);
                    if (squareCur)
                        break;
                }
                if (markerRT != new Rectangle())
                {
                    squareCur = IsSquare(markerRT);
                    if (squareCur)
                        break;
                }
                if (markerRB != new Rectangle())
                {
                    squareCur = IsSquare(markerRB);
                    if (squareCur)
                        break;
                }
                if (markerLB != new Rectangle())
                {
                    squareCur = IsSquare(markerLB);
                    if (squareCur)
                        break;
                }
            }
        }
        //-------------------------------------------------------------------------
        private void GlueBmp(ref double filterType, ref Bitmap bmp2)
        {
            //bmp2.Save("GlueBmp.bmp", ImageFormat.Bmp);
            if (filterType != double.MaxValue && filterType > 10)
                filterType = 0;
            if (filterType <= 1 && filterType > 0)//1.2
                bmp2 = GlueFilter(ref bmp2, new Rectangle(), 2, true, filterType /= 1.2);//1.5
        }
        //-------------------------------------------------------------------------
        private string AlignmentSheet
            (
              ref Rectangle curRect
            , ref Rectangle etRect
            , ref int deltaxEt
            , ref int deltaX
            , ref decimal kx
            , ref decimal ky
            , ref Bitmap bmp
            , Bitmap bmpPres
            , ref Rectangle markerLT
            , ref Rectangle markerRT
            , ref Rectangle markerRB
            , ref Rectangle markerLB
            , Rectangle markerLTet
            , Rectangle markerRTet
            , Rectangle markerRBet
            , Rectangle markerLBet
            , bool isRotate = false)
        {
            if (isRotate)
            {
                GetRectangles(ref curRect, ref etRect, ref deltaxEt, ref deltaX, markerLT, markerRT, markerRB
               , markerLB, markerLTet, markerRTet, markerRBet, markerLBet);

                //Bitmap b2 = (Bitmap)bmp.Clone();
                //using (Graphics g = Graphics.FromImage(b2))
                //{
                //    g.DrawRectangle(new Pen(Color.Green), curRect);
                //}
                //b2.Save("curRect.bmp", ImageFormat.Bmp);
                //b2.Dispose();
                if (curRect.Width != 0 && etRect.Width != 0 && curRect.Height != 0 && etRect.Height != 0)
                {
                    kx = (decimal)curRect.Width / etRect.Width;//Попытка деления на нуль
                    ky = (decimal)curRect.Height / etRect.Height;
                }
                return "";
            }
            string barCodesPrompt = "";
            if (curRect == Rectangle.Empty)
            {
                barCodesPrompt = "Aligment error 1";
                return barCodesPrompt;
            }
            float angle = 0;

            //bmp.Save("AlignmentSheet.bmp", ImageFormat.Bmp);

            if (markerLT != new Rectangle() && markerLB != new Rectangle())
            {
                //angle = Math.Sign(diff1) * GetAngle
                //    (markerLB.X
                //    , markerLT.Bottom//.Y
                //    , markerLB.X + deltaX - delta
                //    , markerLT.Bottom//.Y
                //    , markerLB.X
                //    , markerLB.Bottom
                //    );
                angle = (float)GetAngle2
                    (
                          new Point(markerLB.Right, markerLB.Y)
                        , new Point(markerLT.Right, markerLT.Bottom)
                        );
                float angleEt = (float)GetAngle2
                    (
                        new Point(markerLBet.Right, markerLBet.Y)
                      , new Point(markerLTet.Right, markerLTet.Bottom)
                      );
                angle = angle - angleEt;
            }
            else
            {
                //angle = Math.Sign(diff1) * GetAngle
                //    (markerRB.X
                //    , markerRT.Y
                //    , markerRB.X + deltaX - delta
                //    , markerRT.Y
                //    , markerRB.X
                //    , markerRB.Bottom
                //    );
                angle = (float)GetAngle2(new Point(markerRB.X, markerRB.Y), new Point(markerRT.X, markerRT.Bottom));
                float angleEt = (float)GetAngle2(new Point(markerRBet.X, markerRBet.Y), new Point(markerRTet.X, markerRTet.Bottom));
                angle = angle - angleEt;
            }
            //}
            for (int j = 0; j < 3; j++)
            {
                if (float.IsNaN(angle))
                    break;
                if ((!float.IsNaN(angle) && angle != 0) || angle == 0)
                    if (Math.Abs(angle) < .08)//.16 - много
                        break;
                //else
                //    break;
                Bitmap b1;
                try
                {
                    b1 = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format24bppRgb);
                }
                catch (Exception)
                {
                    barCodesPrompt = "Aligment error. Bad sheet";
                    return barCodesPrompt;
                }
                Graphics g2 = Graphics.FromImage(b1);
                Color argbWhite = Color.FromArgb(255, 255, 255);
                g2.Clear(argbWhite);
                //g2.InterpolationMode = InterpolationMode.HighQualityBicubic;//замедляет работу, пользы мало
                g2.InterpolationMode = InterpolationMode.NearestNeighbor;
                using (Matrix m2 = new Matrix())
                {
                    m2.RotateAt((float)angle, new System.Drawing.Point(bmp.Width / 2, bmp.Height / 2));
                    g2.Transform = m2;
                    try
                    {
                        g2.DrawImageUnscaledAndClipped(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                    }
                    catch { }
                    g2.ResetTransform();
                }

                //b1.Save("AlignmentHighQualityBilinear.bmp", ImageFormat.Bmp);

                //b1 = ConvertTo1Bit(ref b1);//для InterpolationMode.HighQualityBicubic
                bmp = (Bitmap)b1.Clone();

                //bmp.Save("Alignment.bmp", ImageFormat.Bmp);

                b1.Dispose();
                g2.Dispose();
                if (MarkersFind(bmp, ref markerLT, ref markerRT, ref markerLB, ref markerRB, markerLTet) < 3)
                {
                    barCodesPrompt = "Markers not found ";
                    return barCodesPrompt;
                }
                GetRectangles(ref curRect, ref etRect, ref deltaxEt, ref deltaX, markerLT, markerRT, markerRB
                , markerLB, markerLTet, markerRTet, markerRBet, markerLBet);

                //Bitmap b2 = (Bitmap)bmp.Clone();
                //using (Graphics g = Graphics.FromImage(b2))
                //{
                //    g.DrawRectangle(new Pen(Color.Green), curRect);
                //}
                //b2.Save("curRect.bmp", ImageFormat.Bmp);
                //b2.Dispose();


                kx = (decimal)curRect.Width / etRect.Width;
                ky = (decimal)curRect.Height / etRect.Height;
                //delta = (int)Math.Round((decimal)deltaxEt * kx);
                angle = 0;
                //if (deltaxEt * kx != deltaX)
                //{
                //decimal diff1 = deltaxEt * kx - deltaX;
                if (markerLT != new Rectangle() && markerLB != new Rectangle())
                {
                    //angle = Math.Sign(diff1) * GetAngle
                    //    (markerLB.X
                    //    , markerLT.Y
                    //    , markerLB.X + deltaX - delta
                    //    , markerLT.Y
                    //    , markerLB.X
                    //    , markerLB.Bottom
                    //    );
                    angle = (float)GetAngle2(new Point(markerLB.Right, markerLB.Y), new Point(markerLT.Right, markerLT.Bottom));
                    float angleEt = (float)GetAngle2(new Point(markerLBet.Right, markerLBet.Y), new Point(markerLTet.Right, markerLTet.Bottom));
                    angle = angle - angleEt;
                }
                else
                {
                    //angle = Math.Sign(diff1) * GetAngle
                    //    (markerRB.X
                    //    , markerRT.Y
                    //    , markerRB.X + deltaX - delta
                    //    , markerRT.Y
                    //    , markerRB.X
                    //    , markerRB.Bottom
                    //    );
                    angle = (float)GetAngle2(new Point(markerRB.X, markerRB.Y), new Point(markerRT.X, markerRT.Bottom));
                    float angleEt = (float)GetAngle2(new Point(markerRBet.X, markerRBet.Y), new Point(markerRTet.X, markerRTet.Bottom));
                    angle = angle - angleEt;
                }
            }
            //}
            //if (angle > .5)
            if (!float.IsNaN(angle) && Math.Abs(angle) > .6)
                barCodesPrompt = "Aligment error sheet";
            return barCodesPrompt;
        }
        //-------------------------------------------------------------------------
        private bool InsideContour(Point[] contour)
        {
            try
            {
                using (GraphicsPath gp = new GraphicsPath())
                {
                    gp.AddPolygon(contour);
                    RectangleF bounds = gp.GetBounds();
                    return gp.IsVisible(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        //-------------------------------------------------------------------------
        private bool InsideContour(KeyValuePair<Bubble, Point[]> itm)
        {
            try
            {
                using (GraphicsPath gp = new GraphicsPath())
                {
                    gp.AddPolygon(itm.Value);
                    RectangleF bounds = gp.GetBounds();
                    return gp.IsVisible(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        //-------------------------------------------------------------------------
        private void GetRectangles(ref Rectangle curRect, ref Rectangle etRect
            , ref int deltaxEt, ref int deltaX, Rectangle markerLT, Rectangle markerRT
            , Rectangle markerRB, Rectangle markerLB, Rectangle markerLTet
            , Rectangle markerRTet, Rectangle markerRBet, Rectangle markerLBet)
        {
            if (markerLB != new Rectangle() && markerRT != new Rectangle() && markerRB != new Rectangle())
            {
                //curRect = new Rectangle(markerLB.X, markerRT.Bottom//Y
                //    , markerRB.Right - markerLB.X, markerLB.Bottom - markerRT.Bottom);// Y
                //etRect = new Rectangle(markerLBet.X, markerRTet.Bottom//Y
                //    , markerRBet.Right - markerLBet.X, markerLBet.Bottom - markerRTet.Bottom);// Y
                curRect = new Rectangle(markerLB.X, markerRT.Bottom//Y
                    , markerRB.Right - markerLB.X, markerLB.Y - markerRT.Bottom);// Y
                etRect = new Rectangle(markerLBet.X, markerRTet.Bottom//Y
                    , markerRBet.Right - markerLBet.X, markerLBet.Y - markerRTet.Bottom);// Y

                deltaxEt = markerRTet.X - markerRBet.X;
                deltaX = markerRT.X - markerRB.X;
                //deltaxEt = markerRTet.Bottom - markerRBet.X;
                //deltaX = markerRT.X - markerRB.X;
            }
            else if (markerLT != new Rectangle() && markerRT != new Rectangle() && markerRB != new Rectangle())
            {
                curRect = new Rectangle(markerLT.X, markerRT.Bottom//Y
                    , markerRT.Right - markerLT.X, markerRB.Bottom - markerRT.Bottom);//Y
                etRect = new Rectangle(markerLTet.X, markerRTet.Bottom//Y
                    , markerRTet.Right - markerLTet.X, markerRBet.Bottom - markerRTet.Bottom);//Y
                deltaxEt = markerRTet.X - markerRBet.X;
                deltaX = markerRT.X - markerRB.X;

            }
            else if (markerLB != new Rectangle() && markerLT != new Rectangle() && markerRB != new Rectangle())
            {
                //curRect = new Rectangle(markerLB.X, markerLT.Bottom//Y
                //    , markerRB.Right - markerLB.X, markerLB.Bottom - markerLT.Bottom);//Y
                //etRect = new Rectangle(markerLBet.X, markerLTet.Bottom//Y
                //    , markerRBet.Right - markerLBet.X, markerLBet.Bottom - markerLTet.Bottom);//Y
                curRect = new Rectangle(markerLB.Right, markerLT.Bottom//Y
                    , markerRB.Right - markerLB.X, markerLB.Bottom - markerLT.Bottom);//Y
                etRect = new Rectangle(markerLBet.Right, markerLTet.Bottom//Y
                    , markerRBet.Right - markerLBet.X, markerLBet.Bottom - markerLTet.Bottom);//Y
                deltaxEt = markerLTet.X - markerLBet.X;
                deltaX = markerLT.X - markerLB.X;
            }
            else if (markerLB != new Rectangle() && markerRT != new Rectangle() && markerLT != new Rectangle())
            {
                //curRect = new Rectangle(markerLB.X, markerRT.Bottom//Y
                //    , markerRT.Right - markerLB.X, markerLB.Bottom - markerRT.Bottom);// Y
                //etRect = new Rectangle(markerLBet.X, markerRTet.Bottom//Y
                //    , markerRTet.Right - markerLBet.X, markerLBet.Bottom - markerRTet.Bottom);// Y
                curRect = new Rectangle(markerLB.Right, markerRT.Bottom//Y
                    , markerRT.Right - markerLB.Right, markerLB.Bottom - markerRT.Bottom);// Y
                etRect = new Rectangle(markerLBet.Right, markerRTet.Bottom//Y
                    , markerRTet.Right - markerLBet.Right, markerLBet.Bottom - markerRTet.Bottom);// Y
                deltaxEt = markerLTet.X - markerLBet.X;
                deltaX = markerLT.X - markerLB.X;
            }
        }
        //-------------------------------------------------------------------------
        private bool IsSquare(Rectangle r)
        {
            double d;
            if (r.Width > r.Height)
            {
                d = (double)r.Width / r.Height;
            }
            else
            {
                d = (double)r.Height / r.Width;
            }
            if (d > 1.5)
            {
                return false;
            }
            return true;
            //if (Math.Abs(r.Width - r.Height) < (r.Width + r.Height) / 16)
            //{
            //    return true;
            //}
            //return false;
        }
        //-------------------------------------------------------------------------
        public void SetOutputValues(ref string[] headers, ref string[] headersValues, ref object[] totalOutput
            , ref string[] allBarCodeNames, ref string[] allBarCodeValues, string name, string value, int regionOutputPosition)
        {
            if (regionOutputPosition > 0)
            {
                if (headers.Length < regionOutputPosition)
                {
                    Array.Resize(ref headers, regionOutputPosition);
                    Array.Resize(ref headersValues, regionOutputPosition);
                    Array.Resize(ref totalOutput, regionOutputPosition);
                }
                headers[regionOutputPosition - 1] = name;
                headersValues[regionOutputPosition - 1] = value;
                totalOutput[regionOutputPosition - 1] = value;
            }
            Array.Resize(ref allBarCodeNames, allBarCodeNames.Length + 1);
            allBarCodeNames[allBarCodeNames.Length - 1] = name;
            Array.Resize(ref allBarCodeValues, allBarCodeValues.Length + 1);
            allBarCodeValues[allBarCodeValues.Length - 1] = value;
        }
        //-------------------------------------------------------------------------
        public string GetBarCode
            (
              Bitmap bmp
            , ref bool notConfident
            , ref string barCodesPrompt
            , ref double filterType
            , ref string barcodeMem
            , int x1, int x2, int y1, int y2
            , decimal kx
            , decimal ky
            , Rectangle curRect
            , Rectangle etRect
            , int deltaY
            , Region region
            , bool dualControl
            , double? percent_confident_text_region
            , double percent_confident_text
            , string fontName
            , ref Rectangle currentBarCodeRectangle
            , ref Rectangle lastSymbolRectangle
            , bool autoVersion = true
            , bool manual = false
            )
        {
            //if (rec.cancellationToken.IsCancellationRequested) return "";

            bool useRaspFilter = false;
            int length = 0;
            string barcode = "";
            int deltaX = 0;
            //Rectangle lastSymbolRectangle= Rectangle.Empty;
            int x = (x1 + x2) / 2;
            int y = (y1 + y2) / 2;
            bool err = false;
            Rectangle rn = new Rectangle(x1, y1, x2 - x1, y2 - y1);
            RegionsArea arr = region.areas[0];
            int rotateParameter = region.rotate;
            Rectangle rArr = new Rectangle(arr.left, arr.top, arr.width, arr.height);
            if (rArr.Right - arr.left > rArr.Bottom - arr.top)
            {//horisontalBarcodeDetect
                #region if
                int barcodeHeight = (int)Math.Round((decimal)(rArr.Bottom - arr.top) * ky) * 3 / 4;
                //Rectangle r = HorisontalBarcodeDetect(bmp, x, y, barcodeHeight);
                //Rectangle r = HorisontalBarcodeDetect(bmp, rn.X + rn.Height, y, barcodeHeight);//rn.X + rn.Height
                Rectangle r = Rectangle.Empty;
                if (manual)
                    r = new Rectangle(x1, y1, x2 - x1, y2 - y1);
                else
                {
                    //r = new Rectangle
                    //            (
                    //              rn.X - rn.Width / 4
                    //            , rn.Y - rn.Height / 2
                    //            , rn.Width * 2
                    //            , rn.Height * 3
                    //            );
                    ////GetDeltaY(bmp, ref deltaY, ref currentBarCodeRectangle, ref r2);
                    //BarcodeRecognize2
                    //    (bmp, ref filterType, ref region, ref useRaspFilter, ref length, ref barcode, ref err
                    //       ,r
                    //       //new Rectangle
                    //       // (
                    //       //   rn.X - rn.Width / 4
                    //       // , rn.Y - rn.Height / 2
                    //       // , rn.Width * 2
                    //       // , rn.Height * 3
                    //       // )
                    //        );
                }
                if (barcode != "")
                {
                    GetDeltaY(bmp, ref deltaY, ref currentBarCodeRectangle, ref r);
                }
                else
                {
                    if (!manual)
                    {
                        r = HorisontalBarcodeDetect(bmp, rn.X + rn.Height, y, barcodeHeight);
                    }
                    if (x <= 0 || x >= bmp.Width
                        || y <= 0 || y >= bmp.Height
                        || r.Width <= 0 || r.Width >= bmp.Width / 3
                        || r.Height <= 0 || r.Height >= bmp.Height / 3)
                        r = HorisontalBarcodeDetect(bmp, rn.X + rn.Height, y, barcodeHeight, false, .9);

                    if (x <= 0 || x >= bmp.Width
                        || y <= 0 || y >= bmp.Height
                        || r.Width <= 0 || r.Width >= bmp.Width / 3
                        || r.Height <= 0 || r.Height >= bmp.Height / 3)
                    {
                        r = new Rectangle
                                    (
                                      rn.X - rn.Height// / 4
                                    , rn.Y - rn.Height / 2
                                    , rn.Width * 2
                                    , rn.Height * 2
                                    );

                        BarcodeRecognize2
                             (bmp, ref filterType, ref region, ref useRaspFilter, ref length, ref barcode, ref err, r);

                        if (barcode != "")
                        {
                            GetDeltaY(bmp, ref deltaY, ref currentBarCodeRectangle, ref r);
                        }
                        if (barcode == "" && dualControl && autoVersion)
                        {
                            barCodesPrompt = "Error in " + region.name;
                            return "";
                        }
                    }
                    else
                    {
                        //using (Graphics g = Graphics.FromImage(bmp))
                        //{
                        //    g.DrawRectangle(new Pen(Color.Red), r);
                        //}
                        //bmp.Save("HorisontalBarcode.bmp", ImageFormat.Bmp);
                        if (manual || Math.Abs(currentBarCodeRectangle.Height - r.Height) - 24 <= currentBarCodeRectangle.Height / 4)
                        {
                            currentBarCodeRectangle = r;
                            deltaY = r.Y - y1 + 12;
                        }
                        else
                        {
                            r = HorisontalBarcodeDetect(bmp, rn.X + rn.Height, y, barcodeHeight, false);
                            if (x <= 0 || x >= bmp.Width
                                || y <= 0 || y >= bmp.Height
                                || r.Width <= 0 || r.Width >= bmp.Width / 3
                                || r.Height <= 0 || r.Height >= bmp.Height / 3)
                            {
                                if (dualControl)// && autoVersion
                                {
                                    barCodesPrompt = "Error in " + region.name;
                                    return "";
                                }
                            }
                            if (Math.Abs(currentBarCodeRectangle.Height - r.Height) - 24 <= currentBarCodeRectangle.Height / 4)
                            {//48=12*4 размер увеличивает HorisontalBarcodeDetect 
                                currentBarCodeRectangle = r;
                                deltaY = r.Y - y1 + 12;
                            }
                            else
                            {
                                //using (Graphics g = Graphics.FromImage(bmp))
                                //{
                                //    g.DrawRectangle(new Pen(Color.Red), r);
                                //}
                                //bmp.Save("HorisontalBarcode.bmp", ImageFormat.Bmp);
                                filterType = -1;
                                deltaY = r.Y - currentBarCodeRectangle.Y;
                                if (Math.Abs(deltaY) > currentBarCodeRectangle.Height / 8)
                                {
                                    GetDeltaY(bmp, ref deltaY, ref currentBarCodeRectangle, ref r);
                                }
                                //else
                                //{
                                //    deltaY = 0;
                                //}
                                //useRaspFilter = true;
                            }
                        }
                        BarcodeRecognize2(bmp, ref filterType, ref region, ref useRaspFilter, ref length, ref barcode, ref err, r);
                    }
                }

                #endregion

                if (barcode != "")
                    if (r.X > rn.X + rn.Height)
                        deltaX = r.X - rn.X;//old format
            }
            else
            {
                #region verticalBarcodeDetect
                int barcodeHeight = (int)Math.Round((decimal)(rArr.Right - arr.left) * kx) * 3 / 4;
                //Rectangle r = verticalBarcodeDetect(bmp, x, y, barcodeHeight);
                Rectangle r;
                if (!manual)
                {
                    if (region.name.StartsWith("question_number"))
                    {
                        r = new Rectangle
                                      (
                                        rn.X - rn.Width
                                      , rn.Y - rn.Height / 2
                                      , rn.Width * 4
                                      , rn.Height + rn.Height / 2
                                      );

                    }
                    else if (rotateParameter == 90)
                    {
                        r = new Rectangle
                                      (
                                        rn.X - rn.Width
                                      , rn.Y - rn.Height / 2
                                      , rn.Width * 4
                                      , rn.Height + rn.Height / 2
                                      );
                    }
                    else
                    {
                        r = new Rectangle
                                      (
                                        rn.X - rn.Width / 2
                                      , rn.Y - rn.Height / 4
                                      , rn.Width * 2
                                      , rn.Height
                                      );
                    }
                    BarcodeRecognize2
                         (bmp, ref filterType, ref region, ref useRaspFilter, ref length, ref barcode, ref err, r);
                    if (barcode != "")
                    {
                        currentBarCodeRectangle = rn;
                        //GetDeltaY(bmp, ref deltaY, ref currentBarCodeRectangle, ref r);
                    }
                    else
                    {
                        r = VerticalBarcodeDetect(bmp, x, rn.Y + rn.Width, barcodeHeight);
                    }
                }
                else
                {
                    r = new Rectangle(x1, y1, x2 - x1, y2 - y1);
                    currentBarCodeRectangle = r;
                    GetBarCode2(bmp, ref filterType, ref region, ref useRaspFilter, ref length, ref barcode, ref err, ref arr, rotateParameter, ref r);
                }
                if (r.Height > barcodeHeight * 20 && barcode == "")
                    r = VerticalBarcodeDetect(bmp, x, rn.Y + rn.Width, barcodeHeight, true, .95, 1);
                if (barcode == "" && x <= 0 || x >= bmp.Width
                    || y <= 0 || y >= bmp.Height
                    || r.Width <= 0 || r.Width >= bmp.Width
                    || r.Height <= 0 || r.Height >= bmp.Height)
                    r = VerticalBarcodeDetect(bmp, x, rn.Y + rn.Width, barcodeHeight, false);
                if (barcode == "" && r.Height > barcodeHeight * 24)
                    r = VerticalBarcodeDetect(bmp, x, rn.Y + rn.Width, barcodeHeight, false, .95, 1);
                //using (Graphics g = Graphics.FromImage(bmp))
                //{
                //    g.DrawRectangle(new Pen(Color.Red), r);
                //}
                //bmp.Save("VerticalBarcodeDetect.bmp", ImageFormat.Bmp);

                if (manual || Math.Abs(currentBarCodeRectangle.Width - r.Width) - 24 <= currentBarCodeRectangle.Width / 4
                    && r.Width > currentBarCodeRectangle.Width - currentBarCodeRectangle.Width / 4)
                {
                    currentBarCodeRectangle = r;
                    if (rotateParameter == 90)
                        deltaY = r.Bottom - y2 - VerticalBarcodeBorder;
                    else
                        deltaY = r.Y - y1 + VerticalBarcodeBorder;
                    if (Math.Abs(deltaY) > r.Width * 2)
                        deltaY = 0;
                }
                else if (barcode == "")
                {
                    Rectangle r2 = VerticalBarcodeDetect(bmp, x, rn.Y + rn.Width, barcodeHeight, false);
                    if (r2.Height > barcodeHeight * 24)
                        r2 = VerticalBarcodeDetect(bmp, x, rn.Y + rn.Width, barcodeHeight, false, .95, 1);
                    if (r2.Width <= 0 || r2.Width >= bmp.Width
                        || r2.Height <= 0 || r2.Height >= bmp.Height)
                    {
                        if (Math.Abs(currentBarCodeRectangle.Width - r2.Width) - 24 < currentBarCodeRectangle.Width / 4
                            && r2.Width > currentBarCodeRectangle.Width - currentBarCodeRectangle.Width / 4)
                        {
                            currentBarCodeRectangle = r;

                            if (rotateParameter == 90)
                                deltaY = r.Bottom - y2 - VerticalBarcodeBorder;
                            else
                                deltaY = r.Y - y1 + VerticalBarcodeBorder;

                            if (Math.Abs(deltaY) > r.Width)// * 2
                                deltaY = 0;
                        }
                        else
                        {
                            filterType = -1;
                            useRaspFilter = true;
                        }
                    }
                    else
                    {
                        if (Math.Abs(currentBarCodeRectangle.Width - r2.Width) < currentBarCodeRectangle.Height / 8
                            && r2.Width > currentBarCodeRectangle.Width - currentBarCodeRectangle.Width / 4)
                        {
                            currentBarCodeRectangle = r2;
                            r = r2;
                            if (rotateParameter == 90)
                                deltaY = r.Bottom - y2 - VerticalBarcodeBorder;
                            else
                                deltaY = r.Y - y1 + VerticalBarcodeBorder;
                            if (Math.Abs(deltaY) > r.Width * 2)//
                                //{
                                //Bitmap b = new Bitmap(r.Width, r.Height);
                                //CopyRotateBitmap(bmp, rotateParameter, ref r, ref b);
                                //b.Save("CopyRotateBitmap.bmp", ImageFormat.Bmp);
                                deltaY = 0;
                            //}
                        }
                        else
                        {
                            filterType = -1;
                            useRaspFilter = true;
                        }
                    }
                }
                if (barcode == "" && r.Width > 0 && r.Height > 0 && r.Height < barcodeHeight * 24)
                {
                    GetBarCode2(bmp, ref filterType, ref region, ref useRaspFilter, ref length, ref barcode, ref err, ref arr, rotateParameter, ref r);
                }
                //}
                #endregion
            }
            err = false;
            if ((barcode != "" && length > 0 && length - 2 == barcode.Length))
            {
                //if (region.name.StartsWith("question_number"))
                //{
                //    //deltaY = currentBarCodeRectangle.Y - y1 - VerticalBarcodeBorder;
                //}
                //if (region.type == "barCode&Text" && region.areas[1].type == "numbersText")
                //{
                //    if (Regex.Match(barcode, @"[\D]").Success)
                //    {
                //        err = true;
                //        barcode = "";
                //    }
                //}
            }
            else
            {
                err = true;
                deltaY = 0;
            }
            if (manual)
            {
                barcodeMem = barcode;
                return barcode;
            }
            if (dualControl || err)
            {
                if (region.type == "barCode&Text")
                {
                    if (dualControl && barcode == "" && autoVersion)
                    {
                        barCodesPrompt = "Error in " + region.name;
                        return "";
                    }
                    //if (dualControl && barcode != "")
                    //{
                    barcodeMem = barcode;
                    //}
                    arr = region.areas[1];
                    rArr = new Rectangle(arr.left, arr.top, arr.width, arr.height);
                    x1 = deltaX + curRect.X + (int)Math.Round((decimal)(arr.left - etRect.X) * kx);
                    y1 = deltaY + curRect.Y + (int)Math.Round((decimal)(arr.top - etRect.Y) * ky);
                    x2 = deltaX + curRect.X + (int)Math.Round((decimal)(rArr.Right - etRect.X) * kx);
                    y2 = deltaY + curRect.Y + (int)Math.Round((decimal)(rArr.Bottom - etRect.Y) * ky);
                    if (region.name.StartsWith("question_number"))
                        y1 -= rArr.Height / 2;//больше диапазон поиска

                    TextRecognizeTotal
                        (
                          bmp
                        , filterType
                        , barcodeMem
                        , x1, x2, y1, y2
                        , region.areas[1].type//ref region
                        , percent_confident_text_region
                        , percent_confident_text, fontName
                        , ref lastSymbolRectangle
                        , ref barcode
                        , ref y
                        , ref arr, rotateParameter
                        );
                }
                if (barcodeMem == "" && barcode == "")
                {
                    barCodesPrompt = "Error in " + region.name;
                    return "";
                }
                else
                {
                    if (dualControl)
                    {
                        if (barcodeMem != barcode)
                        {
                            barCodesPrompt = "Error in " + region.name;
                            if (autoVersion) return "";
                        }
                    }
                    notConfident = (barcodeMem != barcode) ? true : false;
                }
            }
            return barcode;
        }
        //-------------------------------------------------------------------------
        private void GetBarCode2(Bitmap bmp, ref double filterType, ref Region region, ref bool useRaspFilter, ref int length, ref string barcode, ref bool err, ref RegionsArea arr, int rotateParameter, ref Rectangle r)
        {
            Bitmap bmp2 = new Bitmap(r.Width, r.Height, PixelFormat.Format24bppRgb);
            CopyRotateBitmap(bmp, rotateParameter, ref r, ref bmp2);
            string barcodeType = region.areas[1].type;
            barcode = BarcodeRecognize
                       (
                         bmp2//, ref 
                       , new Rectangle(new System.Drawing.Point(), bmp2.Size)
                       , ref filterType
                       , ref barcodeType
                       , useRaspFilter
                       );
            length = barcode.Length;
            barcode = barcode.Trim('*');
            err = false;
            if (barcode == "")
            {
                err = true;
            }
            if ((err || length - 2 != barcode.Length) && !useRaspFilter)
            {
                useRaspFilter = true;
                barcodeType = arr.type;
                barcode = BarcodeRecognize
                       (
                         bmp2
                       , new Rectangle(new System.Drawing.Point(), bmp2.Size)
                       , ref filterType
                       , ref barcodeType
                       , useRaspFilter
                       );
                length = barcode.Length;
                barcode = barcode.Trim('*');
            }
            bmp2.Dispose();
        }
        //-------------------------------------------------------------------------
        private void GetDeltaY(Bitmap bmp, ref int deltaY, ref Rectangle currentBarCodeRectangle, ref Rectangle r)
        {
            Bitmap b = CopyBitmap(bmp, r);
            Bitmap b2 = (Bitmap)b.Clone();
            //b2.Save("HorisontalBarcode.bmp", ImageFormat.Bmp);
            //b2 = RaspFilter(b2, 2, new Rectangle(), true);
            //b2.Save("HorisontalBarcode.bmp", ImageFormat.Bmp);
            if (SelectBarcode(ref b))//???
            {
                int upperBound = GetUpperBound(b2);
                deltaY = r.Y - currentBarCodeRectangle.Y + upperBound;
            }
            else
                deltaY = 0;
            b.Dispose();
            b2.Dispose();
        }
        //-------------------------------------------------------------------------
        private void BarcodeRecognize2
            (
            Bitmap bmp
            , ref double filterType
            , ref Region region
            , ref bool useRaspFilter
            , ref int length
            , ref string barcode
            , ref bool err
            , Rectangle r
            )//ref
        {
            string barcodeType = region.areas[1].type;
            barcode = BarcodeRecognize
                       (
                         bmp
                       , r
                       , ref filterType
                       , ref barcodeType
                       , false//useRaspFilter//???
                       );
            length = barcode.Length;
            barcode = barcode.Trim('*');
            if (barcode == "")
                err = true;
            //else
            //{
            //    //deltaY = r.Y - y1 - 5;// ;
            //}
            if ((err || length - 2 != barcode.Length) && !useRaspFilter)
            {
                useRaspFilter = true;
                barcodeType = region.areas[1].type;
                barcode = BarcodeRecognize
                      (
                         bmp
                       , r
                       , ref filterType
                       , ref barcodeType
                       , useRaspFilter
                       );
                length = barcode.Length;
                barcode = barcode.Trim('*');
            }
        }
        //-------------------------------------------------------------------------
        private void TextRecognizeTotal
            (
              Bitmap bmp
            , double filterType
            , string barcodeMem
            , int x1, int x2, int y1, int y2
            , string textType//  ref Region region
            , double? percent_confident_text_region
            , double percent_confident_text
            , string fontName
            , ref Rectangle lastSymbolRectangle
            , ref string barcode
            , ref int y
            , ref RegionsArea arr
            , int rotateParameter
            , int lim = 0
            , List<Regions> regionsList = null
            )
        {
            bool filter = false;

            //bmp.Save("TextRecognizeTotal.bmp", ImageFormat.Bmp);

            for (int iFilter = 0; iFilter < 2; iFilter++)
            {
                #region for
                if (iFilter == 1)
                    filter = true;
                Rectangle r2 = new Rectangle(x1, y1, x2 - x1, y2 - y1);
                Bitmap bmp2;// = new Bitmap(bmp);
                if (r2.Width < r2.Height)
                {
                    bmp2 = new Bitmap(r2.Height * 25, r2.Height * 2, PixelFormat.Format24bppRgb);
                    bmp2 = CopyBitmap
                              (
                                 bmp
                               , new Rectangle(new System.Drawing.Point(r2.X - (int)r2.Height // / 2 //- r2.Width / 6//???
                               , r2.Y - r2.Height / 2)
                               , bmp2.Size)
                               );
                    //GlueBmp(ref filterType, ref bmp2);
                    //bmp2.Save("Text.bmp", ImageFormat.Bmp);

                    bmp2 = ConvertTo1Bit(ref bmp2);

                    y = bmp2.Height / 2;// -rn.Height;
                    int dist = bmp2.Width / 4;
                    int k = 0;
                    while (k < dist)
                    {
                        Point[] contour = new Point[0];
                        Rectangle r = Rectangle.Empty;
                        for (int i = 0; i < 3; i++)
                        {
                            switch (i)
                            {
                                case 1:
                                    y -= bmp2.Height / 4;
                                    break;
                                case 2:
                                    y += bmp2.Height / 2;
                                    break;
                            }
                            for (k = 0; k < dist; k++)
                            {
                                Color color = bmp2.GetPixel(k, y);
                                //bmp2.SetPixel(k, y, Color.Green);
                                //bmp2.Save("SetPixel.bmp", ImageFormat.Bmp);
                                Color argbWhite = Color.FromArgb(255, 255, 255);
                                if (color != argbWhite)
                                {
                                    contour = ContourFind(bmp2, k, y);
                                    //using (Graphics g = Graphics.FromImage(bmp2))
                                    //{
                                    //    g.DrawPolygon(new Pen(Color.Blue), contour);
                                    //}
                                    //bmp2.Save("Text.bmp", ImageFormat.Bmp);

                                    Point p;
                                    GetOuterContour(bmp2, ref contour, ref r, out p);
                                    if (r.Width * r.Height < 100)
                                        continue;
                                    break;
                                }
                            }
                            if (r.Width * r.Height != 0)
                            {
                                //using (Graphics g = Graphics.FromImage(bmp2))
                                //{
                                //    g.DrawRectangle(new Pen(Color.Red), r);
                                //}
                                //bmp2.Save("Text.bmp", ImageFormat.Bmp);

                                if (r.Y == 0 && r.Height != bmp2.Height)
                                {
                                    bmp2 = CopyBitmap
                                       (
                                          bmp
                                        , new Rectangle(new System.Drawing.Point(r2.X - r2.Height
                                        , r2.Y - r2.Height / 2 - r2.Height / 4)
                                        , bmp2.Size)
                                        );
                                    //bmp2 = ConvertTo1Bit(ref bmp2);
                                    continue;
                                }
                                else if (r.Bottom == bmp2.Height && r.Height != bmp2.Height)
                                {
                                    bmp2 = CopyBitmap
                                       (
                                          bmp
                                        , new Rectangle(new System.Drawing.Point(r2.X - r2.Height
                                        , r2.Y - r2.Height / 2 + r2.Height / 4)
                                        , bmp2.Size)
                                        );
                                    //bmp2 = ConvertTo1Bit(ref bmp2);
                                    continue;
                                }

                                if (r.Y < r.Height / 8)//16
                                {
                                    bmp2 = CopyBitmap
                                                 (
                                                  bmp
                                                 , new Rectangle(new System.Drawing.Point(r2.X - r2.Width, r2.Y - r2.Height / 4)
                                                 , bmp2.Size
                                                 )
                                                 );
                                    //bmp2 = ConvertTo1Bit(ref bmp2);
                                }
                                else if (r.Bottom > bmp2.Height - r.Height / 8)//16
                                {
                                    bmp2 = CopyBitmap
                                            (
                                             bmp
                                            , new Rectangle(new System.Drawing.Point(r2.X - r2.Width, r2.Y + r2.Height / 4)
                                            , bmp2.Size
                                            )
                                            );
                                    //bmp2 = ConvertTo1Bit(ref bmp2);
                                }
                                break;
                            }
                        }
                        //bmp2.Save("Text.bmp", ImageFormat.Bmp);
                        if (r != new Rectangle())
                            break;
                    }
                    TextRecognizeExt
                        (
                          filterType
                        , percent_confident_text_region
                        , percent_confident_text, fontName
                        , ref barcode
                        , ref lastSymbolRectangle
                        , ref arr
                        , filter
                        , ref r2
                        , ref bmp2
                        , barcodeMem
                        , lim
                        , regionsList
                        );
                }
                else
                {
                    if (rotateParameter != 0)
                    {
                        bmp2 = new Bitmap(r2.Width * 2, r2.Width * 20, PixelFormat.Format24bppRgb);
                        bmp2 = CopyBitmap
                            (
                              bmp
                            , new Rectangle(new System.Drawing.Point(r2.X - r2.Width / 2
                                , r2.Bottom - r2.Width * 18), bmp2.Size)// * 20+2
                            );
                        //bmp2 = ConvertTo1Bit(ref bmp2);
                        bmp2.RotateFlip(RotateFlipType.Rotate90FlipNone);

                        TextRecognizeExt
                         (filterType
                         , percent_confident_text_region
                         , percent_confident_text, fontName
                         , ref barcode
                         , ref lastSymbolRectangle
                         , ref arr
                         , filter
                         , ref r2
                         , ref bmp2
                         , barcodeMem
                         , lim
                         , regionsList
                         );
                    }
                    else
                    {
                        bmp2 = new Bitmap(r2.Width * 2, r2.Width * 10, PixelFormat.Format24bppRgb);
                        bmp2 = CopyBitmap
                               (
                                 bmp
                               , new Rectangle(new System.Drawing.Point(r2.X - r2.Width / 2, r2.Y - r2.Height)
                               , bmp2.Size
                               )
                               );
                        //bmp2 = ConvertTo1Bit(ref bmp2);
                        bmp2.RotateFlip(RotateFlipType.Rotate270FlipNone);

                        TextRecognizeExt
                            (
                              filterType
                            , percent_confident_text_region
                            , percent_confident_text, fontName
                            , ref barcode
                            , ref lastSymbolRectangle
                            , ref arr
                            , filter
                            , ref r2
                            , ref bmp2
                            , barcodeMem
                            , lim
                            , regionsList
                            );
                    }
                }
                bmp2.Dispose();
                if (!String.IsNullOrEmpty(barcode))
                {
                    //if (region.areas[1].type == "numbersText")
                    if (textType == "numbersText")
                    {
                        if (Regex.Match(barcode, @"[\D]").Success)
                            barcode = "";
                        else
                            break;
                    }
                    else
                        break;
                }
                #endregion
            }
        }
        //-------------------------------------------------------------------------
        private void CopyRotateBitmap(Bitmap bmp, int rotateParameter, ref Rectangle r, ref Bitmap bmp2)
        {
            bmp2 = CopyBitmap(bmp, r);
            if (rotateParameter != 0)
            {
                bmp2.RotateFlip(RotateFlipType.Rotate90FlipNone);
            }
            else
            {
                bmp2.RotateFlip(RotateFlipType.Rotate270FlipNone);
            }
        }
        //-------------------------------------------------------------------------
        public void TextRecognizeExt
            (
              double filterType
            , double? percent_confident_text_region
            , double percent_confident_text
            , string fontName
            , ref string barcode
            , ref Rectangle lastSymbolRectangle
            , ref RegionsArea arr
            , bool filter
            , ref Rectangle r2
            , ref Bitmap bmp2
            , string barcodeMem
            , int lim = 0
            , List<Regions> regionsList = null
            , bool manual = false
            )
        {
            for (int i = 0; i < 4; i++)
            {
                if (i > 0 && filterType > 0 && filterType < 1.2)// || (filterType > 0 && filterType < .4)
                    bmp2 = GlueFilter(ref bmp2, new Rectangle(0, 0, bmp2.Width, bmp2.Height), 2, true, filterType);
                //bool useGlueFilter = false;
                barcode = TextRecognize
                    (
                      0
                    , 0
                    , r2.Width
                    , r2.Height * 2
                    , ref bmp2
                    , arr.type
                    , ref lastSymbolRectangle
                    , percent_confident_text_region
                    , percent_confident_text
                    , lim
                    , filter
                    , fontName
                    , filterType
                    , true
                    , manual
                    );
                if (regionsList != null)
                {
                    if (barcode.StartsWith("1OOP"))
                        barcode = "100POINT";

                    Regions regionsTemp = GetRegions(barcode, regionsList);
                    if (regionsTemp == null)
                        //{
                        //    bmp2 = RaspFilter(bmp2, 1, new Rectangle(0, 0, bmp2.Width, bmp2.Height), true, true);
                        //    bmp2 = RaspFilter(bmp2, 1, new Rectangle(0, 0, bmp2.Width, bmp2.Height), true);
                        continue;
                    //}
                }
                if (barcodeMem != "" && barcodeMem == barcode)
                    break;
                if (barcodeMem == "" && barcode != "")
                    break;
                if (filterType >= 1)
                {
                    //bmp2.Save("TextRecognize3.bmp", ImageFormat.Bmp);
                    if (i == 0)
                        bmp2 = RaspFilter(ref bmp2);
                    else if (i == 1)
                        bmp2 = RaspFilter(ref bmp2, 2, new Rectangle(), true);
                    else
                        break;
                    //bmp2.Save("TextRecognize3.bmp", ImageFormat.Bmp);
                }
            }

            //bmp2.Save("TextRecognizeExt.bmp", ImageFormat.Bmp);
            bmp2.Dispose();
        }
        //-------------------------------------------------------------------------
        public void SetSettings(ref Rectangle[] bubblesRegions, ref Rectangle[] bubblesOfRegion
            , ref int[] bubblesSubLinesCount, ref int[] bubblesSubLinesStep, ref int[] bubblesPerLine, ref int[] lineHeight, ref int[] linesPerArea
            , out int answersPosition, out int indexAnswersPosition, ref object[] totalOutput, ref string[] bubbleLines
            , Region region, decimal kx, decimal ky, Rectangle curRect, Rectangle etRect)
        {
            indexAnswersPosition = region.indexOutputPosition;
            if (totalOutput.Length <= indexAnswersPosition)
                Array.Resize(ref totalOutput, indexAnswersPosition);
            totalOutput[indexAnswersPosition - 1] = new string[0];
            answersPosition = region.outputPosition;
            if (totalOutput.Length <= answersPosition)
                Array.Resize(ref totalOutput, answersPosition);
            totalOutput[answersPosition - 1] = new string[0];
            Array.Resize(ref bubblesRegions, region.areas.Length);
            Array.Resize(ref bubblesOfRegion, region.areas.Length);
            Array.Resize(ref bubblesSubLinesCount, region.areas.Length);
            Array.Resize(ref bubblesSubLinesStep, region.areas.Length);
            Array.Resize(ref bubblesPerLine, region.areas.Length);
            Array.Resize(ref lineHeight, region.areas.Length);
            Array.Resize(ref linesPerArea, region.areas.Length);
            for (int k = 0; k < region.areas.Length; k++)
            {
                Array.Resize(ref bubbleLines, bubbleLines.Length + 1);

                RegionsArea arr = region.areas[k];
                bubbleLines[bubbleLines.Length - 1] = arr.bubbleLines;

                Rectangle rArr = new Rectangle(arr.left, arr.top, arr.width, arr.height);
                int step = region.areas[k].lineHeight - region.areas[k].bubble.Height;
                if (String.IsNullOrEmpty(region.areas[k].bubblesOrientation) || region.areas[k].bubblesOrientation == "horizontal")
                {
                    linesPerArea[k] = (int)Math.Round((decimal)(arr.height + step) / region.areas[k].lineHeight);
                    bubblesSubLinesStep[k] = (int)Math.Round((decimal)region.areas[k].subLineHeight * ky);
                    //bubbleLines[k] = linesPerArea[k].ToString();
                }
                else
                {
                    linesPerArea[k] = (int)Math.Round((decimal)(arr.width) / region.areas[k].lineHeight);
                    bubblesSubLinesStep[k] = (region.areas[k].subLinesAmount > 0)
                        ? (int)Math.Round((decimal)(region.areas[k].subLineHeight - region.areas[k].bubble.Width) * kx)
                        : (int)Math.Round((decimal)(region.areas[k].bubble.Width * 2) * kx);
                }
                int x1 = curRect.X + (int)Math.Round((decimal)(arr.left - etRect.X) * kx);
                int y1 = curRect.Y + (int)Math.Round((decimal)(arr.top - etRect.Y) * ky);
                int x2 = curRect.X + (int)Math.Round((decimal)(rArr.Right - etRect.X) * kx);
                int y2 = curRect.Y + (int)Math.Round((decimal)(rArr.Bottom - etRect.Y) * ky);

                //int x1 = curRect.X + (int)Math.Round((decimal)(arr.left * kx));
                //int y1 = curRect.Y + (int)Math.Round((decimal)(arr.top - etRect.Y) * ky); //etRect.Y - curRect.Y + (int)Math.Round((decimal)(arr.top * ky));
                //int x2 = etRect.X - curRect.X + (int)Math.Round((decimal)(rArr.Right * kx));
                //int y2 = curRect.Y + (int)Math.Round((decimal)(rArr.Bottom - etRect.Y) * ky); //curRect.Y - etRect.Y + (int)Math.Round((decimal)(rArr.Bottom * ky));

                bubblesRegions[k] = new Rectangle(x1, y1, x2 - x1, y2 - y1);
                bubblesSubLinesCount[k] = region.areas[k].subLinesAmount;
                bubblesPerLine[k] = (int)region.areas[k].bubblesPerLine;
                bubblesOfRegion[k] = region.areas[k].bubble;
                lineHeight[k] = (int)Math.Round((decimal)(region.areas[k].lineHeight * ky));
                rArr = new Rectangle(bubblesOfRegion[k].X, bubblesOfRegion[k].Y, bubblesOfRegion[k].Width, bubblesOfRegion[k].Height);
                x1 = curRect.X + (int)Math.Round((decimal)(bubblesOfRegion[k].X - etRect.X) * kx);
                y1 = curRect.Y + (int)Math.Round((decimal)(bubblesOfRegion[k].Y - etRect.Y) * ky);
                x2 = curRect.X + (int)Math.Round((decimal)(rArr.Right - etRect.X) * kx);
                y2 = curRect.Y + (int)Math.Round((decimal)(rArr.Bottom - etRect.Y) * ky);
                bubblesOfRegion[k] = new Rectangle(x1, y1, x2 - x1, y2 - y1);
            }
        }
        //-------------------------------------------------------------------------
        public void SetAdditionalOutputData(ref string[] headers, ref object[] totalOutput
            , Regions regions, string fileName)
        {
            string additionalOutputData = "";
            if (headers.Length > 0)
            {
                string[] additionalOutput = new string[0];
                if (regions.additionalOutputData != null)
                {
                    foreach (var itm in regions.additionalOutputData)
                    {
                        if (itm.outputPosition > totalOutput.Length)
                        {
                            Array.Resize(ref totalOutput, itm.outputPosition);
                        }
                        if (itm.value != "%fileName%")
                        {
                            totalOutput[itm.outputPosition - 1] = itm.value;
                        }
                        else
                        {
                            totalOutput[itm.outputPosition - 1] = fileName;
                        }
                        if (itm.outputPosition < headers.Length)
                        {
                            if (itm.value != "%fileName%")
                            {
                                totalOutput[itm.outputPosition - 1] = itm.value;
                                headers[itm.outputPosition - 1] = itm.value;
                            }
                            else
                            {
                                totalOutput[itm.outputPosition - 1] = fileName;
                                headers[itm.outputPosition - 1] = fileName;
                            }
                        }
                        else
                        {
                            int output = itm.outputPosition - headers.Length - 2;
                            if (output > 0)
                            {
                                Array.Resize(ref additionalOutput, output);
                                if (itm.value != "%fileName%")
                                {
                                    additionalOutput[output - 1] = itm.value;
                                }
                                else
                                {
                                    additionalOutput[output - 1] = fileName;
                                }
                            }
                        }
                    }
                }
                for (int j = 0; j < additionalOutput.Length; j++)
                {
                    additionalOutputData += "," + additionalOutput[j];
                }
            }
        }
        //-------------------------------------------------------------------------
        public int LastBannerFind(Bitmap bmpEntry, int x1, int y1, int x2, int y2, out int lastBannerBottom)//, double filterType
        {
            Rectangle lastBanner = new Rectangle(x1, y1, x2 - x1, y2 - y1);
            //bmpEntry.Save("bmpEntry.bmp", ImageFormat.Bmp);
            int factor = 4;
            double brightness = .95;
            lastBannerBottom = 0;
            Rectangle lastBannerMem = lastBanner;
            Rectangle r = Rectangle.Empty;
            //Bitmap bmp = (Bitmap)bmpEntry.Clone();
            for (int i = 0; i < 2; i++)
            {
                lastBanner = lastBannerMem;
                int xn = lastBanner.X / factor - 12 / factor;
                int yn = lastBanner.Y / factor + lastBanner.Height / 2 / factor;
                int part = lastBanner.Height / factor / 8;

                Bitmap bmp;
                lastBanner = MultiplyRectangle(lastBannerMem, (double)1 / factor);
                if (factor != 1)
                {
                    bmp = new Bitmap(bmpEntry.Width / factor, bmpEntry.Height / factor, PixelFormat.Format24bppRgb);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBilinear;//HighQualityBicubic QualityBilinear;
                        g.DrawImage(bmpEntry, 0, 0, bmp.Width, bmp.Height);
                    }
                    //bmp = GlueFilter(bmp, new Rectangle(), 2, true, filterType);
                }
                else
                {
                    bmp = (Bitmap)bmpEntry.Clone();
                    bmp = ConvertTo1Bit(ref bmp);
                }
                //bmp = binaryzeMap(bmp, bmp.Width, bmp.Height, 5);

                //Bitmap b2 = (Bitmap)bmp.Clone();
                //using (Graphics g = Graphics.FromImage(b2))
                //{
                //    //g.DrawPolygon(new Pen(Color.Cyan), prevCurve);
                //    g.DrawEllipse(new Pen(Color.Red), xn - 4, yn - 4, 8, 8);
                //}
                //b2.Save("corners.bmp", ImageFormat.Bmp);
                //b2.Dispose();

                System.Drawing.Point[] prevCurve = new System.Drawing.Point[0];
                do
                {
                    if (token.IsCancellationRequested)
                    {
                        lastBannerBottom = 0;
                        return lastBannerMem.Height;
                    }

                    if (i == 0)
                        prevCurve = ContourFindSpeed(ref bmp, xn, yn, 0, false, true, true, 20000, brightness);
                    else
                        prevCurve = ContourFindSpeed(ref bmp, xn, yn, 0, false, true, true, 200000, brightness, true);

                    if (prevCurve.Length == 0)
                    {
                        while ((xn < bmp.Width - 1) && prevCurve.Length == 0)
                        {
                            xn++;
                            prevCurve = ContourFindSpeed(ref bmp, xn, yn, 0, false, true, true, 20000, brightness, true);
                        }
                        if (prevCurve.Length == 0)
                            lastBanner = Rectangle.Empty;
                        break;
                    }
                    r = GetRectangle(prevCurve);

                    //Bitmap b2 = (Bitmap)bmp.Clone();
                    //using (Graphics g = Graphics.FromImage(b2))
                    //{
                    //    g.DrawPolygon(new Pen(Color.Cyan), prevCurve);
                    //    g.DrawRectangle(new Pen(Color.Red), r);
                    //}
                    //b2.Save("lastBanner.bmp", ImageFormat.Bmp);

                    Point p;
                    GetOuterContourSpeed(ref bmp, ref prevCurve, ref r, out p, contourMaxLength, brightness);
                    xn++;// = r.Right + 1;
                } while (xn < bmp.Width - 1 && (r.Width < lastBanner.Width - part) && r.Height < lastBanner.Height - part);

                //Bitmap b2 = (Bitmap)bmp.Clone();
                //using (Graphics g = Graphics.FromImage(b2))
                //{
                //    g.DrawPolygon(new Pen(Color.Cyan), prevCurve);
                //    g.DrawRectangle(new Pen(Color.Red), r);
                //}
                //b2.Save("lastBanner.bmp", ImageFormat.Bmp);
                //b2.Dispose();

                bmp.Dispose();//раскомментировать!!! 

                r = MultiplyRectangle(r, factor);
                int deltaY = r.Bottom - lastBannerMem.Bottom;// / factor
                if (Math.Abs(deltaY) > (y2 - y1) / 4)//8???(r.Width < lastBannerMem.Width - lastBannerMem.Width / 4) ||
                {
                    if (factor != 1)
                    {
                        factor = 1;
                        continue;
                    }
                    Point[] hl = GetHorisontalLines(prevCurve, 0, 0, bmp);

                    //Point[] corners = GetCorners(prevCurve, 128, 15);

                    //Bitmap b = (Bitmap)bmp.Clone();
                    //foreach (Point item in hl)
                    //{
                    //    using (Graphics g = Graphics.FromImage(b))
                    //    {
                    //        //g.DrawPolygon(new Pen(Color.Cyan), prevCurve);
                    //        g.DrawEllipse(new Pen(Color.Red), item.X - 4, item.Y - 4, 8, 8);
                    //    }
                    //}
                    //b.Save("corners.bmp", ImageFormat.Bmp);
                    //b.Dispose();

                    int num = -1;
                    int deltaYmem = int.MaxValue;
                    int maxDist = lastBannerMem.Width / 16;
                    for (int k = 0; k < hl.Length - 1; k += 2)
                    {
                        int dist = Math.Abs(hl[k].X - hl[k + 1].X);
                        if (dist > maxDist)
                        {
                            double angle = GetAngle2(hl[k], hl[k + 1]);
                            if (angle < -170)
                                angle = 180 + angle;
                            if (Math.Abs(angle) > .8)
                                continue;// return lastBannerMem.Height;
                            if (lastBanner.X - hl[k].X > maxDist || lastBanner.X - hl[k + 1].X > maxDist)
                                continue;
                            deltaY = lastBannerMem.Bottom - hl[k].Y;
                            if (Math.Abs(deltaY) < Math.Abs(deltaYmem))
                            {
                                deltaYmem = deltaY;
                                num = k;
                            }
                        }
                    }
                    if (num > -1 && Math.Abs(lastBannerMem.Bottom - hl[num].Y) - 1 <= lastBannerMem.Height / 2)
                    {
                        lastBannerBottom = hl[num].Y;
                        return hl[num].Y - lastBannerMem.Bottom;
                    }

                    //deltaYmem = int.MaxValue;
                    //foreach (var item in prevCurve)
                    //{
                    //    deltaY = lastBannerMem.Bottom - item.Y;
                    //    if (Math.Abs(deltaY) < Math.Abs(deltaYmem))
                    //    {
                    //        deltaYmem = deltaY;
                    //    }
                    //}
                    //lastBannerBottom = lastBannerMem.Bottom + deltaY;
                    //return deltaY;

                    lastBannerBottom = 0;
                    return lastBannerMem.Height;
                }
                else
                {
                    lastBannerBottom = r.Bottom;
                    return r.Bottom - lastBannerMem.Bottom;
                }
            }
            lastBannerBottom = r.Bottom;
            return r.Bottom - lastBannerMem.Bottom;
        }
        //-------------------------------------------------------------------------
        private Point[] GetHorisontalLines(Point[] prevCurve, int dist = 0, double angleDiff = 0, Bitmap bmp = null)
        {
            if (dist == 0)
                dist = prevCurve.Length / 128;
            if (angleDiff == 0)
                angleDiff = 8;//5
            Point[] horisontalLines = new Point[0];
            //int height = r.Height / 2;
            if (prevCurve.Length == 0 || prevCurve.Length < dist)
                return horisontalLines;

            double oldAngle = GetAngle2(prevCurve[0], prevCurve[dist]);
            bool horisontalLineGeg = false;
            for (int j = 0; j < prevCurve.Length - dist; j++)
            {
                Point p1 = prevCurve[j];
                ////bmp.SetPixel(p1.X, p1.Y, Color.Red);
                Point p2 = prevCurve[j + dist];
                //double angle = GetAngle(p1.X, p1.Y, p2.X, p2.Y, p1.X, p2.Y, "c");
                double angle = GetAngle2(p1, p2);
                if (Math.Abs(angle) < angleDiff || Math.Abs(Math.Abs(angle) - 175) <= angleDiff)
                {
                    if (!horisontalLineGeg)
                    {
                        horisontalLineGeg = true;
                        Array.Resize(ref horisontalLines, horisontalLines.Length + 1);
                        horisontalLines[horisontalLines.Length - 1] = p1;
                    }
                }
                else
                {
                    if (horisontalLineGeg)
                    {
                        for (int i = 0; i < dist; i++)
                        {
                            p2 = prevCurve[j - i];
                            angle = GetAngle2(p1, p2);
                            if (Math.Abs(angle) < angleDiff || Math.Abs(Math.Abs(angle) - 175) <= angleDiff)
                            {
                                horisontalLineGeg = false;
                                Array.Resize(ref horisontalLines, horisontalLines.Length + 1);
                                horisontalLines[horisontalLines.Length - 1] = p2;
                                break;
                            }
                        }
                        if (horisontalLineGeg)
                        {
                            Array.Resize(ref horisontalLines, horisontalLines.Length + 1);
                            horisontalLines[horisontalLines.Length - 1] = p1;
                        }
                        horisontalLineGeg = false;
                        if (Math.Abs(horisontalLines[horisontalLines.Length - 1].X - horisontalLines[horisontalLines.Length - 2].X) < dist)
                        {
                            Array.Resize(ref horisontalLines, horisontalLines.Length - 2);
                        }
                        j += dist;
                    }
                }
            }
            if (horisontalLines.Length % 2 == 0)
                return horisontalLines;
            double angleEnd = GetAngle2(horisontalLines[horisontalLines.Length - 1], prevCurve[prevCurve.Length - 1]);
            if (Math.Abs(angleEnd) < angleDiff || Math.Abs(Math.Abs(angleEnd) - 175) <= angleDiff)
            {
                if (Math.Abs(horisontalLines[horisontalLines.Length - 1].X - prevCurve[prevCurve.Length - 1].X) < dist)
                {
                    Array.Resize(ref horisontalLines, horisontalLines.Length - 1);
                }

                Array.Resize(ref horisontalLines, horisontalLines.Length + 1);
                horisontalLines[horisontalLines.Length - 1] = prevCurve[prevCurve.Length - 1];
            }
            return horisontalLines;
        }

        //-------------------------------------------------------------------------
        //private Point[] GetCorners(Point[] prevCurve, int dist = 0, double angleDiff = 0)
        //{
        //    if (dist == 0)
        //    {
        //        dist = prevCurve.Length / 128;
        //    }
        //    if (angleDiff == 0)
        //    {
        //        angleDiff = 5;
        //    }
        //    Point[] corners = new Point[0];
        //    //int height = r.Height / 2;
        //    double oldAngle = GetAngle2(ref prevCurve[0], ref prevCurve[dist]);
        //    bool cornersBeg = false;
        //    for (int j = 0; j < prevCurve.Length - dist; j++)
        //    {
        //        Point p1 = prevCurve[j];
        //        ////bmp.SetPixel(p1.X, p1.Y, Color.Red);
        //        Point p2 = prevCurve[j + dist];
        //        //double angle = GetAngle(p1.X, p1.Y, p2.X, p2.Y, p1.X, p2.Y, "c");
        //        double angle = GetAngle2(ref p1, ref p2);
        //        if (!double.IsNaN(angle))
        //        {
        //            if (Math.Abs(oldAngle - angle) > angleDiff)
        //            {
        //                oldAngle = angle;
        //                cornersBeg = true;

        //                //j += dist;
        //                //Bitmap b = (Bitmap)bmp.Clone();
        //                //using (Graphics g = Graphics.FromImage(b))
        //                //{
        //                //    g.DrawLine(new Pen(Color.Red), p1, p2);
        //                //}
        //                //b.Save("strips.bmp", ImageFormat.Bmp);
        //                //b.Dispose();
        //            }
        //            else
        //            {
        //                if (cornersBeg)
        //                {
        //                    oldAngle = angle;
        //                    cornersBeg = false;
        //                    Array.Resize(ref corners, corners.Length + 1);
        //                    if (j > 1)//dist
        //                    {
        //                        corners[corners.Length - 1] = prevCurve[j - 1];//dist
        //                    }
        //                    else
        //                    {
        //                        corners[corners.Length - 1] = prevCurve[j];
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (!double.IsNaN(oldAngle))
        //            {
        //                oldAngle = angle;
        //                //j += dist;
        //                //Bitmap b = (Bitmap)bmp.Clone();
        //                //using (Graphics g = Graphics.FromImage(b))
        //                //{
        //                //    g.DrawLine(new Pen(Color.Red), p1, p2);
        //                //}
        //                //b.Save("strips.bmp", ImageFormat.Bmp);
        //                //b.Dispose();
        //            }
        //        }
        //    }
        //    return corners;
        //}
        //-------------------------------------------------------------------------
        public double GetAngle2(Point p1, Point p2)
        {
            //double angle = Math.Atan2(p1.X - p2.Y, p1.X - p2.X) / Math.PI * 180;
            //double angle = Math.Atan2(p1.X - p2.Y, p1.X - p2.X) / Math.PI * 180;
            //angle = (angle < 0) ? angle + 360 : angle;//Без этого диапазон от 0...180 и -1...-180

            System.Windows.Vector vector1 = new System.Windows.Vector(p1.X - p2.X, p1.Y - p2.Y);
            System.Windows.Vector vector2 = new System.Windows.Vector(p1.X, 0);
            Double angleBetween;
            // angleBetween is approximately equal to 0.9548
            angleBetween = System.Windows.Vector.AngleBetween(vector1, vector2);

            return angleBetween;
            //return angle;
        }
        //-------------------------------------------------------------------------
        public void GetAxis
            (
              ref int[] axisX
            , ref int[] axisY
            , ref Rectangle[] factRectangle
            , Dictionary<Bubble, Point[]> allContourMultiLine
            , Rectangle bubble1
            , ref int[] axisYSubline
            //, bool smartResize
            //, int factor
            )
        {
            int caliberX = bubble1.Width;// / 8;
            int caliberY = bubble1.Height;// / 8;
            for (int k = 0; k < allContourMultiLine.Count; k++)
            {
                KeyValuePair<Bubble, System.Drawing.Point[]> item = allContourMultiLine.ElementAt(k);
                if (item.Value.Length > 0)
                {
                    if (factRectangle[k].Size != new System.Drawing.Size())
                    {
                        if (axisX[0] == 0)
                        {
                            axisX[0] = factRectangle[k].Location.X;
                        }
                        else
                        {
                            bool newPoint = true;
                            for (int l = 0; l < axisX.Length; l++)
                            {
                                if (Math.Abs(axisX[l] - factRectangle[k].Location.X) < caliberX)
                                {
                                    axisX[l] += factRectangle[k].Location.X;
                                    axisX[l] /= 2;
                                    newPoint = false;
                                }
                            }
                            if (newPoint)
                            {
                                Array.Resize(ref axisX, axisX.Length + 1);
                                axisX[axisX.Length - 1] = factRectangle[k].Location.X;
                            }
                        }
                        if (axisY[0] == 0)
                        {
                            axisY[0] = factRectangle[k].Location.Y;
                            axisYSubline[0] = allContourMultiLine.ElementAt(k).Key.subLine;
                        }
                        else
                        {
                            bool newPoint = true;
                            for (int l = 0; l < axisY.Length; l++)
                            {
                                if (Math.Abs(axisY[l] - factRectangle[k].Location.Y) < caliberY)
                                {
                                    axisY[l] += factRectangle[k].Location.Y;
                                    axisY[l] /= 2;
                                    newPoint = false;
                                }
                            }
                            if (newPoint)
                            {
                                Array.Resize(ref axisY, axisY.Length + 1);
                                Array.Resize(ref axisYSubline, axisYSubline.Length + 1);
                                axisY[axisY.Length - 1] = factRectangle[k].Location.Y;
                                axisYSubline[axisY.Length - 1] = allContourMultiLine.ElementAt(k).Key.subLine;
                            }
                        }
                    }
                }
            }
            int dx = 0, dy = 0;
            for (int k = 0; k < allContourMultiLine.Count; k++)
            {
                KeyValuePair<Bubble, System.Drawing.Point[]> item = allContourMultiLine.ElementAt(k);
                if (item.Value.Length == 0)
                {
                    if (factRectangle[k].Size == new System.Drawing.Size())
                    {
                        for (int l = 0; l < axisX.Length; l++)
                        {
                            if (Math.Abs(axisX[l] - factRectangle[k].Location.X + dx) < caliberX)
                            {
                                int d = factRectangle[k].Location.X - axisX[l];
                                factRectangle[k].Location
                                    = new System.Drawing.Point(
                                        axisX[l]
                                        , factRectangle[k].Location.Y);
                                dx = d;
                            }
                        }
                        for (int l = 0; l < axisY.Length; l++)
                        {
                            if (Math.Abs(axisY[l] - factRectangle[k].Location.Y + dy) < caliberY)
                            {
                                int d = factRectangle[k].Location.Y - axisY[l];
                                factRectangle[k].Location
                                    = new System.Drawing.Point(
                                        factRectangle[k].Location.X
                                        , axisY[l]
                                         );
                                dy = d;
                            }
                        }
                    }
                }
            }
        }
        //-------------------------------------------------------------------------
        public void GetLineFactStep
            (
            ref double lineFactStep
            , ref int prevGoodLine
            , ref int prevGoodLineY
            , int startNumber
            , RegionsArea area
            , Rectangle[] factRectangle
            , Dictionary<Bubble, Point[]> allContourMultiLine
            , int bubblesRegion
            , int minContourLength = int.MaxValue
            )
        {
            int nextGoodLine = 0;
            int nextGoodLineY = 0;
            int goodContour1 = 0, goodContour2 = 0;
            for (int k = startNumber; k < factRectangle.Length; k++)
            {//определение "lineFactStep"
                if (allContourMultiLine.ElementAt(k).Key.areaNumber != bubblesRegion)
                {
                    if (nextGoodLine > prevGoodLine && goodContour2 > 0)
                    {
                        nextGoodLineY /= goodContour2;
                        lineFactStep = (double)(nextGoodLineY - prevGoodLineY) / (nextGoodLine - prevGoodLine);
                    }
                    break;
                }
                if (factRectangle[k].Size != new Size()
                    && allContourMultiLine.ElementAt(k).Key.subLine == 0
                    && allContourMultiLine.ElementAt(k).Value.Length < minContourLength
                    && InsideContour(allContourMultiLine.ElementAt(k)))
                {
                    if (String.IsNullOrEmpty(area.bubblesOrientation)
                          || area.bubblesOrientation == "horizontal")
                    {
                        if (prevGoodLine == 0 || prevGoodLine == allContourMultiLine.ElementAt(k).Key.point.Y)
                        {
                            prevGoodLine = allContourMultiLine.ElementAt(k).Key.point.Y;
                            prevGoodLineY += factRectangle[k].Y;
                            goodContour1++;
                        }
                        else
                        {
                            if (goodContour1 > 0)
                            {
                                prevGoodLineY /= goodContour1;
                                goodContour1 = 0;
                            }
                            if (nextGoodLine == 0 || nextGoodLine == allContourMultiLine.ElementAt(k).Key.point.Y)
                            {
                                nextGoodLine = allContourMultiLine.ElementAt(k).Key.point.Y;
                                nextGoodLineY += factRectangle[k].Y;
                                goodContour2++;
                            }
                            else
                            {
                                nextGoodLineY /= goodContour2;
                                lineFactStep = (double)(nextGoodLineY - prevGoodLineY) / (nextGoodLine - prevGoodLine);
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (prevGoodLine == 0 || prevGoodLine == allContourMultiLine.ElementAt(k).Key.point.X + 1)
                        {
                            prevGoodLine = allContourMultiLine.ElementAt(k).Key.point.X + 1;
                            prevGoodLineY += factRectangle[k].Y;
                            goodContour1++;
                        }
                        else
                        {
                            if (goodContour1 > 0)
                            {
                                prevGoodLineY /= goodContour1;
                                goodContour1 = 0;
                            }
                            if (nextGoodLine == 0 || nextGoodLine == allContourMultiLine.ElementAt(k).Key.point.X + 1)
                            {
                                nextGoodLine = allContourMultiLine.ElementAt(k).Key.point.X + 1;
                                nextGoodLineY += factRectangle[k].Y;
                                goodContour2++;
                            }
                            else
                            {
                                nextGoodLineY /= goodContour2;
                                lineFactStep = (double)(nextGoodLineY - prevGoodLineY) / (nextGoodLine - prevGoodLine);
                                break;
                            }
                        }
                    }
                }
            }
        }
        //-------------------------------------------------------------------------
        private Bitmap ResizeBitmap(Bitmap bmp, int factor, InterpolationMode im = InterpolationMode.HighQualityBilinear)
        {
            Bitmap b = new Bitmap(bmp.Width / factor, bmp.Height / factor, PixelFormat.Format24bppRgb);//Format16bppRgb555
            Graphics gi = Graphics.FromImage(b);
            //gi.InterpolationMode = InterpolationMode.Low;
            gi.InterpolationMode = im;
            try
            {
                gi.DrawImage(bmp, 0, 0, b.Width, b.Height);
            }
            catch (Exception)
            {
                gi.Dispose();
                b.Dispose();
                return bmp;
            }

            gi.Dispose();
            //b = GetMonohromeNoIndexBitmap(b);
            bmp = (Bitmap)b.Clone();
            b.Dispose();
            //bmp = binaryzeMap(bmp, bmp.Width, bmp.Height, 1);
            //bmp.Save("ResizeBitmap.bmp", ImageFormat.Bmp);
            return bmp;
        }
        //-------------------------------------------------------------------------
        private byte[] ImageToByte(Bitmap img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
        public class LockBitmap
        {
            Bitmap source = null;
            IntPtr Iptr = IntPtr.Zero;
            BitmapData bitmapData = null;

            public byte[] Pixels { get; set; }
            public int Depth { get; private set; }
            public int Width { get; private set; }
            public int Height { get; private set; }

            public LockBitmap(Bitmap source)
            {
                this.source = source;
            }

            /// <summary>
            /// Lock bitmap data
            /// </summary>
            public void LockBits()
            {
                try
                {
                    // Get width and height of bitmap
                    Width = source.Width;
                    Height = source.Height;

                    // get total locked pixels count
                    int PixelCount = Width * Height;

                    // Create rectangle to lock
                    Rectangle rect = new Rectangle(0, 0, Width, Height);

                    // get source bitmap pixel format size
                    Depth = System.Drawing.Bitmap.GetPixelFormatSize(source.PixelFormat);

                    // Check if bpp (Bits Per Pixel) is 8, 24, or 32
                    if (Depth != 8 && Depth != 24 && Depth != 32)
                    {
                        throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
                    }

                    // Lock bitmap and return bitmap data
                    bitmapData = source.LockBits(rect, ImageLockMode.ReadWrite,
                                                 source.PixelFormat);

                    // create byte array to copy pixel values
                    int step = Depth / 8;
                    Pixels = new byte[PixelCount * step];
                    Iptr = bitmapData.Scan0;

                    // Copy data from pointer to array
                    Marshal.Copy(Iptr, Pixels, 0, Pixels.Length);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            /// <summary>
            /// Unlock bitmap data
            /// </summary>
            public void UnlockBits()
            {
                try
                {
                    // Copy data from byte array to pointer
                    Marshal.Copy(Pixels, 0, Iptr, Pixels.Length);

                    // Unlock bitmap data
                    source.UnlockBits(bitmapData);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            /// <summary>
            /// Get the color of the specified pixel
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public Color GetPixel(int x, int y)
            {
                Color clr = Color.Empty;

                // Get color components count
                int cCount = Depth / 8;

                // Get start index of the specified pixel
                int i = ((y * Width) + x) * cCount;

                if (i > Pixels.Length - cCount || i < 0)
                    throw new IndexOutOfRangeException();
                if (Depth == 24) // For 24 bpp get Red, Green and Blue
                {
                    byte b = Pixels[i];
                    byte g = Pixels[i + 1];
                    byte r = Pixels[i + 2];
                    clr = Color.FromArgb(r, g, b);
                }
                else if (Depth == 32) // For 32 bpp get Red, Green, Blue and Alpha
                {
                    byte b = Pixels[i];
                    byte g = Pixels[i + 1];
                    byte r = Pixels[i + 2];
                    byte a = Pixels[i + 3]; // a
                    clr = Color.FromArgb(a, r, g, b);
                }
                else if (Depth == 8)
                // For 8 bpp get color value (Red, Green and Blue values are the same)
                {
                    byte c = Pixels[i];
                    clr = Color.FromArgb(c, c, c);
                }
                return clr;
            }

            /// <summary>
            /// Set the color of the specified pixel
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="color"></param>
            public void SetPixel(int x, int y, Color color)
            {
                // Get color components count
                int cCount = Depth / 8;

                // Get start index of the specified pixel
                int i = ((y * Width) + x) * cCount;
                if (Depth == 24) // For 24 bpp set Red, Green and Blue
                {
                    Pixels[i] = color.B;
                    Pixels[i + 1] = color.G;
                    Pixels[i + 2] = color.R;
                }
                else if (Depth == 32) // For 32 bpp set Red, Green, Blue and Alpha
                {
                    Pixels[i] = color.B;
                    Pixels[i + 1] = color.G;
                    Pixels[i + 2] = color.R;
                    Pixels[i + 3] = color.A;
                }
                else if (Depth == 8)
                // For 8 bpp set color value (Red, Green and Blue values are the same)
                {
                    Pixels[i] = color.B;
                }
            }
        }
        //-------------------------------------------------------------------------
        private Bitmap binaryzeMap(Bitmap bmp, int width, int height, double localZoneSize, int xb = 0, int yb = 0)
        {
            //DateTime dt = DateTime.Now;
            var imagePixelsSumMap = new int[width * height];
            //http://www.codeproject.com/Tips/240428/Work-with-bitmap-faster-with-Csharp
            LockBitmap lockBitmap = new LockBitmap(bmp);
            lockBitmap.LockBits();

            for (var x = xb; x < width; x++)
            {
                for (var y = yb; y < height; y++)
                {
                    Color c1 = lockBitmap.GetPixel(x, y);
                    int r = c1.R;
                    int index = x + y * width;
                    int a = r;
                    int b;

                    if (x > 0)
                    {
                        c1 = lockBitmap.GetPixel(x - 1, y);
                        b = c1.R;
                    }
                    else
                    {
                        b = 0;
                    }
                    int c;
                    if (y > 0)
                    {
                        c1 = lockBitmap.GetPixel(x, y - 1);
                        c = c1.R;
                    }
                    else
                    {
                        c = 0;
                    }

                    int d;
                    if (x > 0 && y > 0)
                    {
                        c1 = lockBitmap.GetPixel(x - 1, y - 1);
                        d = c1.R;
                    }
                    else
                    {
                        d = 0;
                    }
                    imagePixelsSumMap[index] = a + b + c - d;
                }
            }
            int leftSize = (int)Math.Ceiling(localZoneSize / 2);
            int rightSize = (int)Math.Floor(localZoneSize / 2);
            var zoneCount = (localZoneSize * localZoneSize);
            for (var x = localZoneSize; x < width - localZoneSize; x++)
            {
                for (var y = localZoneSize; y < height - localZoneSize; y++)
                {
                    int index = (int)(x + y * width);
                    var a = imagePixelsSumMap[(int)((x - leftSize) + (y - leftSize) * width)];
                    var b = imagePixelsSumMap[(int)((x + rightSize) + (y - leftSize) * width)];
                    var c = imagePixelsSumMap[(int)((x - leftSize) + (y + rightSize) * width)];
                    var d = imagePixelsSumMap[(int)((x + rightSize) + (y + rightSize) * width)];
                    var alpha = (d - c - b + a) / zoneCount;
                    Color c1 = lockBitmap.GetPixel((int)x, (int)y);
                    int r = c1.R;
                    if (r < alpha + 200)
                    {
                        lockBitmap.SetPixel((int)x, (int)y, Color.Black);
                    }
                    else
                    {
                        lockBitmap.SetPixel((int)x, (int)y, Color.White);
                    }
                }
            }
            lockBitmap.UnlockBits();
            //TimeSpan ts = DateTime.Now - dt;
            //string s = ts.ToString();
            return bmp;
        }
        //-------------------------------------------------------------------------
        private byte[] BitmapToByteArray(Bitmap bitmap)
        {
            BitmapData bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            int numbytes = bmpdata.Stride * bitmap.Height;
            byte[] bytedata = new byte[numbytes];
            IntPtr ptr = bmpdata.Scan0;
            Marshal.Copy(ptr, bytedata, 0, numbytes);
            bitmap.UnlockBits(bmpdata);
            return bytedata;
        }
        //-------------------------------------------------------------------------
        private int GetIndex(Dictionary<Bubble, Point[]> allContourMultiLine, Bubble b)
        {
            for (int index = allContourMultiLine.Count - 1; index >= 0; index--)
            {
                if (allContourMultiLine.ElementAt(index).Key.Equals(b))
                    return index;
            }
            return -1;
        }
        //-------------------------------------------------------------------------
        public void AppendOutput(ref object[] totalOutput, int indexPosition, string s, int indexAnswersPosition, int indexOfFirstBubble)
        {
            if (indexOfFirstBubble != 0 && indexPosition != indexAnswersPosition)
            {
                if (Microsoft.VisualBasic.Information.IsNumeric(s))
                {
                    int si = Convert.ToInt32(s);
                    si += indexOfFirstBubble;
                    s = si.ToString();
                }
            }
            string[] ar = totalOutput[indexAnswersPosition - 1] as string[];
            if (indexPosition == indexAnswersPosition)
            {
                Array.Resize(ref ar, ar.Length + 1);
                ar[ar.Length - 1] = s;
                totalOutput[indexPosition - 1] = ar;
            }
            else
            {
                string[] ar2 = totalOutput[indexPosition - 1] as string[];
                if (ar.Length > ar2.Length)
                {
                    Array.Resize(ref ar2, ar.Length);
                    ar2[ar.Length - 1] = s;
                }
                else
                {
                    ar2[ar.Length - 1] += s;
                }
                totalOutput[indexPosition - 1] = ar2;
            }
        }
        ////-------------------------------------------------------------------------
        //public void BubblesRecognize
        //    (
        //      ref Dictionary<Bubble, Point[]> allContourMultiLine
        //    , ref Rectangle[] factRectangle
        //    , Bitmap bmp
        //    , ref string barCodesPrompt
        //    , double filterType
        //    , bool smartResize
        //    , Rectangle[] bubblesRegions
        //    , Rectangle[] bubblesOfRegion
        //    , int[] bubblesSubLinesCount
        //    , int[] bubblesSubLinesStep
        //    , int[] bubblesPerLine
        //    , int[] lineHeight
        //    , int[] linesPerArea
        //    , int answersPosition
        //    , int indexAnswersPosition
        //    , object[] totalOutput
        //    , string[] bubbleLines
        //    , Regions regions
        //    , RegionsArea[] areas
        //    , int x1, int x2, int y1, int y2
        //    , decimal kx, decimal ky
        //    , Rectangle curRect, Rectangle etRect
        //    , int deltaY
        //    , int amoutOfQuestions
        //    , int indexOfFirstQuestion
        //    , Dictionary<Bubble, CheckedBubble> maxCountRectangles
        //    , double darknessPercent
        //    , double darknessDifferenceLevel
        //    , int lastBannerBottom
        //    , int deltaX = 0
        //    )
        //{

        //    //bmp.Save("BubblesRecognize.bmp");
        //    //try
        //    //{
        //        double brightness = .88;// .5;//
        //        //LockBitmap lockBitmap = null;// new LockBitmap(bmp);
        //        //try
        //        //{
        //        //allContourMultiLine = new Dictionary<Bubble, System.Drawing.Point[]>();

        //        //DateTime dt = DateTime.Now;

        //        bool calcPercentOnly = false;
        //        if (factRectangle.Length > 0)
        //            calcPercentOnly = true;
        //        Rectangle bubble1 = bubblesOfRegion[0];
        //        int caliberWidth = bubble1.Width / 6;//8
        //        int caliberHeight = bubble1.Height / 6;//8

        //        //#region Conversion To grayscale
        //        //Image<Gray, byte> grayImage = new Image<Gray, byte>(bmp);
        //        //Image<Bgr, byte> imgColor = new Image<Bgr, byte>(bmp);
        //        //Bgr c = imgColor[0, 0];
        //        //Color color = Color.FromArgb(0, (int)c.Red, (int)c.Green, (int)c.Blue);
        //        //#endregion

        //        //#region  Image normalization and inversion (if required)
        //        //Ellipse
        //        //Bitmap bt = grayImage.ToBitmap();
        //        //bt.Save("bt1.bmp", ImageFormat.Bmp);
        //        //grayImage = grayImage.ThresholdBinary(new Gray(128), new Gray(255));//thresholdValue
        //        //bt.Save("bt2.bmp", ImageFormat.Bmp);
        //        //bt.Dispose();
        //        ////if (invert)
        //        ////{
        //        ////    grayImage._Not();
        //        ////}
        //        //#endregion

        //        //#region Extracting the Contours
        //        ////grayImage.ROI = (bubblesRegions[0]);
        //        //grayImage.Save("grayImage.bmp");
        //        //using (MemStorage storage = new MemStorage())
        //        //{

        //        //    for (Contour<Point> contours = grayImage.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_TREE, storage); contours != null; contours = contours.HNext)
        //        //    {

        //        //        Contour<Point> currentContour = contours.ApproxPoly(contours.Perimeter * 0.015, storage);
        //        //        if (currentContour.Area > 1000)
        //        //        {
        //        //            CvInvoke.cvDrawContours(imgColor, contours, new MCvScalar(255), new MCvScalar(255), -1, 2, Emgu.CV.CvEnum.LINE_TYPE.EIGHT_CONNECTED, new Point(0, 0));
        //        //            imgColor.Draw(currentContour.BoundingRectangle, new Bgr(0, 255, 0), 1);
        //        //        }
        //        //    }
        //        //}
        //        //imgColor.Save("imgColor.bmp");
        //        //#endregion
        //        //grayImage.ROI = Rectangle.Empty;

        //        //bmp.LockBits(Rectangle.Empty,ImageLockMode.ReadOnly,PixelFormat.Format24bppRgb);
        //        //bmp.UnlockBits(null);
        //        System.Windows.Forms.Application.DoEvents();
        //        const int iter = 3;
        //        using (Bitmap bmpPres = (Bitmap)bmp.Clone())
        //        {
        //            bmp = ConvertTo1Bit(ref bmp);//!!!!
        //            for (int i = 0; i < iter; i++)
        //            {
        //                //lockBitmap = new LockBitmap(bmp);
        //                //lockBitmap.LockBits();
        //                barCodesPrompt = "";
        //                //bool openContour = false;
        //                int bubblesPerWidth = 0, bubblesPerHeight = 0
        //                   , bubbleStepX = 0, bubbleStepY = 0, xn, yn;
        //                Color color;
        //                Color argbWhite = Color.FromArgb(255, 255, 255);
        //                Rectangle prevRectangle = Rectangle.Empty;
        //                System.Drawing.Point[] prevCurve = new System.Drawing.Point[0];
        //                int maxAmoutOfQuestions = linesPerArea.Sum();
        //                if (amoutOfQuestions == 0 || amoutOfQuestions > maxAmoutOfQuestions)
        //                    amoutOfQuestions = maxAmoutOfQuestions;
        //                int bubblesRegion = 0;
        //                int diffBubble = 0;
        //                bubblesPerWidth = bubblesPerLine[0];
        //                bubblesPerHeight = linesPerArea[0];
        //                int lineFactStepY;

        //                if (String.IsNullOrEmpty(areas[0].bubblesOrientation)
        //                    || areas[0].bubblesOrientation == "horizontal")
        //                {
        //                    xn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].X - bubble1.Width / 8));
        //                    yn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y + bubble1.Height / 2));
        //                    bubbleStepX = (int)Math.Round((decimal)(bubblesRegions[0].Width
        //                        - bubble1.Width * bubblesPerWidth) / (bubblesPerWidth - 1));
        //                    bubbleStepY = (int)Math.Round((decimal)(lineHeight[0] - bubble1.Height));
        //                    lineFactStepY = lineHeight[bubblesRegion];// -bubbleStepY;
        //                    diffBubble = (int)((bubble1.Width + bubbleStepX));
        //                }
        //                else
        //                {
        //                    xn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].X + bubble1.Width / 2));
        //                    yn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y - bubble1.Height / 8));
        //                    bubbleStepY = (int)Math.Round((decimal)(bubblesRegions[0].Height
        //                        - bubble1.Height * bubblesPerWidth) / (bubblesPerWidth - 1));

        //                    lineFactStepY = lineHeight[bubblesRegion];// -bubbleStepY;

        //                    bubbleStepX = (int)Math.Round((decimal)(lineHeight[0] - bubble1.Height));
        //                    diffBubble = bubble1.Height + bubbleStepY;
        //                }
        //                if (calcPercentOnly)
        //                    goto CalcPercent;

        //                yn += deltaY;
        //                Rectangle r1 = Rectangle.Empty;
        //                Point[] contour1 = new Point[] { };
        //                factRectangle = new Rectangle[0];
        //                bool endBubblesRegions = false;

        //                int posX = 0, posY = 0;
        //                int correct = 0;
        //                int bubblesSubLine = 0;
        //                allContourMultiLine = new Dictionary<Bubble, System.Drawing.Point[]>();
        //                Bubble bubble = new Bubble();
        //                if (indexOfFirstQuestion == 0)
        //                {
        //                    indexOfFirstQuestion = (areas[0].questionIndex != 0) ? (int)areas[0].questionIndex : 1;
        //                }
        //                int areaNumber = -1;
        //                int contourLength = 10000;
        //                int regionLines = 0;
        //                Point firstBubblePerLineLocation = new Point(bubblesRegions[bubblesRegion].X
        //                    , bubblesRegions[bubblesRegion].Y + deltaY);
        //                int prevGoodBubbleLine = indexOfFirstQuestion;
        //                int prevGoodBubbleSubLine = 0;
        //                int prevGoodBubbleLineLocationY = firstBubblePerLineLocation.Y;
        //                int firstBubbleRegion = 0;
        //                for (int line = indexOfFirstQuestion; line < indexOfFirstQuestion + amoutOfQuestions; line++)
        //                {
        //                    #region for
        //                    //if (token.IsCancellationRequested)
        //                    //    return;
        //                    if (allContourMultiLine.Count > 0)
        //                    {
        //                        bubble = allContourMultiLine.Last().Key;
        //                        if (line - bubble.point.Y > 1)
        //                            endBubblesRegions = true;
        //                    }
        //                    if (endBubblesRegions)
        //                    {
        //                        barCodesPrompt = "The error in determining the regions of bubbles";
        //                        if (i > 0)
        //                        {
        //                            bmp = (Bitmap)bmpPres.Clone();
        //                            return;
        //                        }
        //                        else
        //                            break;
        //                    }
        //                    posX = 0; posY = 0;
        //                    Bubble prevBubble = new Bubble();
        //                    Bubble firstBubblePerLine = new Bubble();
        //                    firstBubblePerLine.subLine = bubblesSubLine;
        //                    firstBubblePerLine.point = new System.Drawing.Point(0, line);
        //                    firstBubblePerLine.areaNumber = bubblesRegion;
        //                    Rectangle firstBubblePerLineRectangle = new Rectangle(bubble1.X, bubble1.Y, bubble1.Width, bubble1.Height);
        //                    if (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
        //                              || areas[bubblesRegion].bubblesOrientation == "horizontal")
        //                    {
        //                        #region horizontal
        //                    Rep:
        //                        for (int k = xn; k < bubblesRegions[bubblesRegion].Right + bubble1.Width / 4; k++)
        //                        {
        //                            bool badBubble = false;
        //                            if (xn <= 0 || xn >= bmp.Width || yn <= 0 || yn >= bmp.Height)
        //                            {
        //                                barCodesPrompt = "Calibration error";
        //                                bmp = (Bitmap)bmpPres.Clone();
        //                                return;
        //                            }
        //                            color = bmp.GetPixel(k, yn);//???заблокирована

        //                            //Bitmap b23 = (Bitmap)bmp.Clone();
        //                            //using (Graphics g = Graphics.FromImage(b23))
        //                            //{
        //                            //    g.DrawEllipse(new Pen(Color.Red), k - 4, yn - 4, 8, 8);
        //                            //}
        //                            //b23.Save("bubbles.bmp", ImageFormat.Bmp);
        //                            //b23.Dispose();

        //                            double f = color.GetBrightness();
        //                            //if (color != argbWhite)
        //                            if (f < brightness)
        //                            {
        //                                #region color Not White

        //                                //Bitmap b13 = (Bitmap)bmp.Clone();
        //                                //using (Graphics g = Graphics.FromImage(b13))
        //                                //{
        //                                //    g.DrawEllipse(new Pen(Color.Red), k - 4, yn - 4, 8, 8);
        //                                //}
        //                                //b13.Save("bubbles.bmp", ImageFormat.Bmp);
        //                                //b13.Dispose();

        //                                contour1 = ContourFindSpeed
        //                                    (
        //                                      ref bmp
        //                                    , k
        //                                    , yn
        //                                    , 0
        //                                    , false
        //                                    , true
        //                                    , false
        //                                    , contourLength
        //                                    , brightness
        //                                    );
        //                                r1 = GetRectangle(contour1);
        //                                //System.Drawing.Point p;
        //                                //GetOuterContourSpeed
        //                                //    (
        //                                //      bmp
        //                                //    , ref contour1
        //                                //    , ref r1
        //                                //    , out p
        //                                //    , contourLength
        //                                //    , brightness
        //                                //    );

        //                                if (r1 == prevRectangle)
        //                                {
        //                                    if (r1.Right < xn)//чрезмерный возврат влево
        //                                        brightness -= .01;
        //                                    continue;
        //                                }
        //                                prevRectangle = new Rectangle(r1.X, r1.Y, r1.Width, r1.Height);

        //                                //Bitmap b2 = (Bitmap)bmp.Clone();
        //                                //using (Graphics g = Graphics.FromImage(b2))
        //                                //{
        //                                //    g.DrawPolygon(new Pen(Color.Red), contour1);
        //                                //    g.DrawRectangle(new Pen(Color.Green), r1);
        //                                //}
        //                                //b2.Save("bubbles2.bmp", ImageFormat.Bmp);
        //                                //b2.Dispose();

        //                                if (k >= bubblesRegions[bubblesRegion].Right - 1 + bubble1.Width / 4)
        //                                {
        //                                    bubble = new Bubble();
        //                                    bubble.areaNumber = bubblesRegion;
        //                                    bubble.point = bubble.point = new System.Drawing.Point(0, line);
        //                                    bubble.subLine = 0;
        //                                    if (!allContourMultiLine.ContainsKey(bubble))
        //                                    {
        //                                        //if (r1.Y < lastBannerBottom)
        //                                        //    correct += 2;
        //                                        correct++;
        //                                        k = xn - 1;
        //                                        switch (correct)
        //                                        {
        //                                            case 1:
        //                                                yn -= bubble1.Height / 2;
        //                                                break;
        //                                            case 2:
        //                                                yn += bubble1.Height;
        //                                                break;
        //                                            case 3:
        //                                            case 4:
        //                                            case 5:
        //                                                yn += bubble1.Height / 2;
        //                                                break;
        //                                            default:
        //                                                if (i > 1)
        //                                                    barCodesPrompt = PromptCalibrationError(bubble);
        //                                                else
        //                                                    barCodesPrompt = null;
        //                                                break;
        //                                        }
        //                                        if (barCodesPrompt != "")
        //                                            break;

        //                                        continue;
        //                                    }
        //                                    else
        //                                        correct = 0;
        //                                }

        //                                //if (p == new System.Drawing.Point(int.MaxValue, 0))
        //                                //{
        //                                //    k += bubbleStepX;
        //                                //    continue;
        //                                //}
        //                                if (r1.Width * r1.Height <= 0)
        //                                {

        //                                    //Bitmap b = (Bitmap)bmp.Clone();
        //                                    //using (Graphics g = Graphics.FromImage(b))
        //                                    //{
        //                                    //    g.DrawEllipse(new Pen(Color.Red, 3), r1.X - 6, r1.Y - 6, 12, 12);
        //                                    //}
        //                                    //b.Save("badBubble.bmp", ImageFormat.Bmp);
        //                                    //b.Dispose();

        //                                    k++;
        //                                    continue;
        //                                }

        //                                decimal dec = (decimal)(r1.X - bubblesRegions[bubblesRegion].X) / diffBubble;
        //                                if (diffBubble == 0)
        //                                    posX = 0;
        //                                else
        //                                    posX = (int)Math.Round(dec);
        //                                int posXReal = posX;
        //                                if ((posX < 0 && contour1.Length < contourLength) || (posX == 0 && r1.Y < lastBannerBottom))
        //                                {
        //                                    if (Math.Abs(r1.Width - bubble1.Width) <= caliberWidth
        //                                      && Math.Abs(r1.Height - bubble1.Height) <= caliberHeight
        //                                      && Array.IndexOf(factRectangle, r1) < 0)
        //                                    {
        //                                        int tmp = (bubblesRegions[bubblesRegion].X - r1.X);
        //                                        //if (tmp < caliberWidth / 4)
        //                                        //    bubblesRegions[bubblesRegion].X = r1.X;
        //                                        if (tmp < bubble1.Width)
        //                                            bubblesRegions[bubblesRegion].X = r1.X;

        //                                    }
        //                                    posX = 0;
        //                                }
        //                                int count;
        //                                if (r1.X < xn)
        //                                    if (posXReal < 0 && r1.Right > xn)
        //                                        count = (int)Math.Round((double)(r1.Right - xn) / diffBubble);
        //                                    else
        //                                        count = (int)Math.Round((double)(r1.Right - k) / diffBubble);
        //                                else
        //                                    count = (int)Math.Round((double)(r1.Width) / diffBubble);
        //                                if (count < 0)
        //                                    count = 0;
        //                                if (count > 1)
        //                                    posX += count;//вставляем на всякий случай

        //                                if (posX >= bubblesPerWidth)
        //                                    posX = bubblesPerWidth - 1;

        //                                if (contour1.Length >= contourLength)
        //                                    posX = bubblesPerWidth - 1;

        //                                bubble = new Bubble();
        //                                bubble.areaNumber = bubblesRegion;
        //                                bubble.point = new System.Drawing.Point(posX, line);
        //                                bubble.subLine = bubblesSubLine;
        //                                bubble.areaNumber = bubblesRegion;
        //                                if (line - bubble.point.Y > 1)
        //                                    endBubblesRegions = true;
        //                                //if (prevRectangle == r1)
        //                                //{
        //                                //    continue;
        //                                //}
        //                                //prevRectangle = r1;
        //                                if (Math.Abs(r1.Width - bubble1.Width) <= caliberWidth
        //                                      && Math.Abs(r1.Height - bubble1.Height) <= caliberHeight
        //                                      && Array.IndexOf(factRectangle, r1) < 0
        //                                      && InsideContour(contour1))
        //                                {
        //                                    #region хороший пузырь

        //                                    //Bitmap b2 = (Bitmap)bmp.Clone();
        //                                    //using (Graphics g = Graphics.FromImage(b2))
        //                                    //{
        //                                    //    g.DrawPolygon(new Pen(Color.Red), contour1);
        //                                    //    g.DrawRectangle(new Pen(Color.Green), r1);
        //                                    //}
        //                                    //b2.Save("bubbles2.bmp", ImageFormat.Bmp);
        //                                    //b2.Dispose();

        //                                    if (line == 1 && correct != 0)
        //                                    {
        //                                        correct = 0;
        //                                        int ynCalculated = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y + bubble1.Height / 2));
        //                                        deltaY = ynCalculated - yn;
        //                                        lastBannerBottom -= deltaY;
        //                                    }

        //                                    if (allContourMultiLine.Count == 0 && posX > 0 && Math.Abs(dec) < (decimal).7)
        //                                    {
        //                                        posX = 0;
        //                                        bubblesRegions[bubblesRegion].X = r1.X;
        //                                    }
        //                                    if (posX == 0 && r1.Y + r1.Width / 2 < lastBannerBottom)
        //                                    {
        //                                        barCodesPrompt = "Aligment error an axis \"Y\"";
        //                                        bmp = (Bitmap)bmpPres.Clone();
        //                                        return;
        //                                    }

        //                                    if (contourLength == 10000)
        //                                        contourLength = contour1.Length * 8;
        //                                    if (bubble.areaNumber == bubblesRegion)
        //                                    {
        //                                        if (prevGoodBubbleSubLine == bubblesSubLine && line > prevGoodBubbleLine && r1.Y > prevGoodBubbleLineLocationY)
        //                                            lineFactStepY = (r1.Y - prevGoodBubbleLineLocationY) / (line - prevGoodBubbleLine);
        //                                    }
        //                                    prevGoodBubbleLine = line;
        //                                    prevGoodBubbleLineLocationY = r1.Y;
        //                                    prevGoodBubbleSubLine = bubblesSubLine;
        //                                    if (posX == 0 && !allContourMultiLine.Keys.Contains(bubble))//!!!
        //                                    {
        //                                        firstBubblePerLineLocation = r1.Location;
        //                                        firstBubbleRegion = bubblesRegion;
        //                                        firstBubblePerLineRectangle = r1;
        //                                        firstBubblePerLine = new Bubble();
        //                                        firstBubblePerLine.point = new System.Drawing.Point(posX, line);
        //                                        firstBubblePerLine.subLine = bubblesSubLine;
        //                                    }
        //                                    if (posX > 0 && posX < bubblesPerWidth - 1)
        //                                    {
        //                                        bubble = new Bubble();
        //                                        bubble.areaNumber = bubblesRegion;
        //                                        bubble.point = new System.Drawing.Point(posX - 1, line);
        //                                        bubble.subLine = bubblesSubLine;
        //                                        if (!allContourMultiLine.Keys.Contains(bubble))
        //                                        {
        //                                            int np = posX - 1;
        //                                            for (int l = np - 1; l > -1; l--)
        //                                            {
        //                                                np = l;
        //                                                bubble.point = new System.Drawing.Point(np, line);
        //                                                if (allContourMultiLine.Keys.Contains(bubble))
        //                                                {
        //                                                    np++;
        //                                                    break;
        //                                                }
        //                                            }
        //                                            for (int l = np; l < posX; l++)
        //                                            {
        //                                                bubble.point = new System.Drawing.Point(l, line);
        //                                                allContourMultiLine.Add(bubble, new System.Drawing.Point[0]);
        //                                                Array.Resize(ref factRectangle, factRectangle.Length + 1);
        //                                                factRectangle[factRectangle.Length - 1]
        //                                                    = new Rectangle(r1.X - diffBubble * (posX - l), r1.Y, 0, 0);
        //                                            }
        //                                        }
        //                                    }
        //                                    bubble = new Bubble();
        //                                    bubble.areaNumber = bubblesRegion;
        //                                    bubble.point = new System.Drawing.Point(posX, line);
        //                                    bubble.subLine = bubblesSubLine;

        //                                    if (!allContourMultiLine.Keys.Contains(bubble))
        //                                    {
        //                                        allContourMultiLine.Add(bubble, contour1);
        //                                        Array.Resize(ref factRectangle, factRectangle.Length + 1);
        //                                        factRectangle[factRectangle.Length - 1] = r1;
        //                                        if (diffBubble > bubble1.Width * 2)
        //                                            k = r1.Right + diffBubble / 2;//bubble1.Width;
        //                                        else
        //                                            k = r1.Right + 1;
        //                                    }
        //                                    else
        //                                    {
        //                                        if (bubble.Equals(prevBubble))
        //                                            k += bubbleStepX;
        //                                        else
        //                                        {
        //                                            prevBubble = bubble;
        //                                            int index = GetIndex(allContourMultiLine, bubble);
        //                                            if (factRectangle[index].Size == new System.Drawing.Size())//!!!
        //                                            {
        //                                                allContourMultiLine[bubble] = contour1;
        //                                                factRectangle[index] = r1;
        //                                                //удаление вставленных на всякий случай
        //                                                //Array.Resize(ref  factRectangle, index + 1);
        //                                                //for (int l = allContourMultiLine.Count - 1; l >= index + 1; l--)
        //                                                //{
        //                                                //    allContourMultiLine.Remove(allContourMultiLine.ElementAt(l).Key);
        //                                                //}
        //                                            }
        //                                            k = r1.Right + 1;
        //                                        }
        //                                    }
        //                                    int tmp = (r1.Y + r1.Height / 4) - yn;
        //                                    if (tmp < r1.Height / 4)
        //                                        yn = r1.Y + r1.Height / 2;
        //                                    #endregion
        //                                }
        //                                else
        //                                {
        //                                    #region плохой пузырь
        //                                    if (correct == 1 && r1.Y < lastBannerBottom)//&& count > bubblesPerWidth
        //                                    {//врезались в баннер
        //                                        correct++;
        //                                        yn += bubble1.Height;
        //                                        goto Rep;
        //                                    }

        //                                    try
        //                                    {
        //                                        if (posXReal > 0 && allContourMultiLine.Count == 0 && !badBubble)
        //                                        {
        //                                            if (i > 1)
        //                                            {
        //                                                barCodesPrompt = "Aligment error an axis \"X\"";
        //                                                bmp = (Bitmap)bmpPres.Clone();
        //                                                return;
        //                                            }
        //                                        }

        //                                        if (posX > 0)// && posX < bubblesPerWidth - 1
        //                                        {
        //                                            bubble = new Bubble();
        //                                            bubble.areaNumber = bubblesRegion;
        //                                            bubble.point = new System.Drawing.Point(posX - 1, line);
        //                                            bubble.subLine = bubblesSubLine;
        //                                            if (posX != 0 && r1.Y < lastBannerBottom)// 
        //                                            {
        //                                                if (k - xn > bubble1.Width + bubble1.Width / 2)
        //                                                {//кривой баннер
        //                                                    if (prevGoodBubbleLine != bubble.point.Y
        //                                                        && prevGoodBubbleSubLine != bubble.subLine)
        //                                                    {
        //                                                        k = xn - 1;
        //                                                        yn += bubble1.Height;
        //                                                        continue;
        //                                                    }
        //                                                }
        //                                            }

        //                                            if (!allContourMultiLine.Keys.Contains(bubble))
        //                                            {
        //                                                int np = posX - 1;
        //                                                for (int l = np - 1; l > -1; l--)
        //                                                {
        //                                                    np = l;
        //                                                    bubble.point = new System.Drawing.Point(np, line);
        //                                                    if (allContourMultiLine.Keys.Contains(bubble))
        //                                                    {
        //                                                        np++;
        //                                                        break;
        //                                                    }
        //                                                }

        //                                                for (int l = np; l < posX; l++)
        //                                                {
        //                                                    bubble.point = new System.Drawing.Point(l, line);
        //                                                    allContourMultiLine.Add(bubble, new System.Drawing.Point[0]);
        //                                                    Array.Resize(ref factRectangle, factRectangle.Length + 1);

        //                                                    //using (Graphics g = Graphics.FromImage(bmp))
        //                                                    //{
        //                                                    //    g.DrawPolygon(new Pen(Color.Red), contour1);
        //                                                    //    g.DrawRectangle(new Pen(Color.Green), r1);
        //                                                    //}
        //                                                    //bmp.Save("bubbles.bmp", ImageFormat.Bmp);

        //                                                    SetFactRectangle
        //                                                        (
        //                                                          factRectangle
        //                                                        , bubbleStepY
        //                                                        , diffBubble
        //                                                        , ref firstBubblePerLineLocation
        //                                                        , ref prevGoodBubbleLine
        //                                                        , line
        //                                                        , ref firstBubblePerLine
        //                                                        , ref firstBubblePerLineRectangle
        //                                                        , l
        //                                                        , ref firstBubbleRegion
        //                                                        , bubblesRegion
        //                                                        , bubblesSubLine
        //                                                        , ref prevGoodBubbleLineLocationY
        //                                                        , ref prevGoodBubbleSubLine
        //                                                        , bubblesSubLinesStep[bubblesRegion]
        //                                                        , lineFactStepY
        //                                                        );

        //                                                    //Bitmap b1 = (Bitmap)bmp.Clone();
        //                                                    //using (Graphics g = Graphics.FromImage(b1))
        //                                                    //{
        //                                                    //    g.DrawRectangle(new Pen(Color.Red)
        //                                                    //        , new Rectangle(factRectangle[factRectangle.Length - 1].X
        //                                                    //                      , factRectangle[factRectangle.Length - 1].Y
        //                                                    //                      , bubble1.Width
        //                                                    //                      , bubble1.Height));
        //                                                    //}
        //                                                    //b1.Save("factRectangle.bmp", ImageFormat.Bmp);
        //                                                    //b1.Dispose();

        //                                                    if (i > 0 && bubble.point.X == 0 && bubble.point.Y > 1
        //                                                        && r1.Y - factRectangle[factRectangle.Length - 2].Y >
        //                                                        diffBubble + diffBubble / 2)
        //                                                    {
        //                                                        barCodesPrompt = "Calibration error";
        //                                                        bmp = (Bitmap)bmpPres.Clone();
        //                                                        return;
        //                                                    }
        //                                                }
        //                                            }
        //                                        }

        //                                        bubble = new Bubble();
        //                                        bubble.areaNumber = bubblesRegion;
        //                                        bubble.point = new System.Drawing.Point(posX, line);
        //                                        bubble.subLine = bubblesSubLine;

        //                                        if (!allContourMultiLine.Keys.Contains(bubble))
        //                                        {
        //                                            allContourMultiLine.Add(bubble, new System.Drawing.Point[0]);
        //                                            Array.Resize(ref factRectangle, factRectangle.Length + 1);
        //                                            SetFactRectangle
        //                                                (
        //                                                  factRectangle
        //                                                , bubbleStepY
        //                                                , diffBubble
        //                                                , ref firstBubblePerLineLocation
        //                                                , ref prevGoodBubbleLine
        //                                                , line
        //                                                , ref firstBubblePerLine
        //                                                , ref firstBubblePerLineRectangle
        //                                                , posX
        //                                                , ref firstBubbleRegion
        //                                                , bubblesRegion
        //                                                , bubblesSubLine
        //                                                , ref prevGoodBubbleLineLocationY
        //                                                , ref prevGoodBubbleSubLine
        //                                                , bubblesSubLinesStep[bubblesRegion]
        //                                                , lineFactStepY
        //                                                );

        //                                            //Bitmap b1 = (Bitmap)bmp.Clone();
        //                                            //using (Graphics g = Graphics.FromImage(b1))
        //                                            //{
        //                                            //    g.DrawRectangle(new Pen(Color.Red)
        //                                            //        , new Rectangle(factRectangle[factRectangle.Length - 1].X
        //                                            //                      , factRectangle[factRectangle.Length - 1].Y
        //                                            //                      , bubble1.Width
        //                                            //                      , bubble1.Height));
        //                                            //}
        //                                            //b1.Save("factRectangle.bmp", ImageFormat.Bmp);
        //                                            //b1.Dispose();

        //                                        }
        //                                    }
        //                                    catch (Exception)
        //                                    {
        //                                    }
        //                                    k++;
        //                                    #endregion
        //                                }
        //                                #endregion
        //                            }
        //                            else
        //                            {
        //                                #region color == argbWhite

        //                                //Bitmap b = (Bitmap)bmp.Clone();
        //                                //using (Graphics g = Graphics.FromImage(b))
        //                                //{
        //                                //    g.DrawEllipse(new Pen(Color.Red), k - 4, yn - 4, 8, 8);
        //                                //}
        //                                //b.Save("strips.bmp", ImageFormat.Bmp);
        //                                //b.Dispose();

        //                                if (k >= bubblesRegions[bubblesRegion].Right - 1 + bubble1.Width / 4)
        //                                {
        //                                    bubble = new Bubble();
        //                                    bubble.areaNumber = bubblesRegion;
        //                                    bubble.point = bubble.point = new System.Drawing.Point(0, line);
        //                                    bubble.subLine = 0;
        //                                    if (!allContourMultiLine.ContainsKey(bubble))
        //                                    {
        //                                        //if (r1.Y < lastBannerBottom)
        //                                        //    correct += 2;
        //                                        correct++;
        //                                        k = xn - 1;
        //                                        switch (correct)
        //                                        {
        //                                            case 1:
        //                                                yn -= bubble1.Height / 2;
        //                                                break;
        //                                            case 2:
        //                                                yn += bubble1.Height;
        //                                                break;
        //                                            case 3:
        //                                                //case 4:
        //                                                //case 5:
        //                                                yn += bubble1.Height / 2;
        //                                                break;
        //                                            default:
        //                                                if (i > 1)
        //                                                    barCodesPrompt = PromptCalibrationError(bubble);
        //                                                else
        //                                                    barCodesPrompt = null;
        //                                                break;
        //                                        }
        //                                        if (barCodesPrompt != "")
        //                                            break;

        //                                        continue;
        //                                    }
        //                                    else
        //                                        correct = 0;
        //                                }
        //                                #endregion
        //                            }
        //                        }
        //                        #endregion
        //                    }
        //                    else
        //                    {
        //                        #region vertical
        //                        for (int k = yn; k < bubblesRegions[bubblesRegion].Bottom + bubble1.Height / 8; k++)
        //                        {
        //                            color = argbWhite;
        //                            try
        //                            {
        //                                color = bmp.GetPixel(xn, k);
        //                            }
        //                            catch (Exception)
        //                            {
        //                                break;
        //                            }
        //                            double f = color.GetBrightness();
        //                            //if (color != argbWhite)
        //                            if (f < brightness)
        //                            {
        //                                #region color != argbWhite
        //                                contour1 = ContourFindSpeed
        //                                    (
        //                                      ref bmp
        //                                    , xn
        //                                    , k
        //                                    , 2//вниз
        //                                    , false
        //                                    , true
        //                                    , false
        //                                    , contourLength
        //                                    , brightness
        //                                    );
        //                                System.Drawing.Point p;
        //                                GetOuterContourSpeed
        //                                   (
        //                                     ref bmp
        //                                   , ref contour1
        //                                   , ref r1
        //                                   , out p
        //                                   , contourLength
        //                                   , brightness
        //                                );
        //                                if (contour1 == null
        //                                    || r1.Size.Equals(new System.Drawing.Size(0, 0)))
        //                                {
        //                                    continue;
        //                                }
        //                                if (p == new System.Drawing.Point(int.MaxValue, 0))
        //                                {
        //                                    k += bubbleStepY;
        //                                    continue;
        //                                }
        //                                posX = (int)Math.Round((decimal)(r1.Y - bubblesRegions[bubblesRegion].Y) / diffBubble);
        //                                if (posX < 0 && contour1.Length < contourLength)
        //                                {
        //                                    if (Math.Abs(r1.Width - bubble1.Width) <= caliberWidth
        //                                      && Math.Abs(r1.Height - bubble1.Height) <= caliberHeight
        //                                      && Array.IndexOf(factRectangle, r1) < 0
        //                                      && InsideContour(contour1))
        //                                    {
        //                                        int tmp = (bubblesRegions[bubblesRegion].Y - r1.Y);
        //                                        if (tmp < caliberHeight / 4)
        //                                            bubblesRegions[bubblesRegion].Y = r1.Y;
        //                                    }
        //                                    posX = 0;
        //                                }
        //                                int count = (int)Math.Round((double)(r1.Bottom - k) / diffBubble);
        //                                //if (count == 0)
        //                                //{
        //                                //    continue;
        //                                //} 
        //                                if (count > 1)
        //                                    posX += count;//вставляем на всякий случай

        //                                if (posX >= bubblesPerWidth)
        //                                    posX = bubblesPerWidth - 1;
        //                                if (contour1.Length >= contourLength)
        //                                    posX = bubblesPerWidth - 1;

        //                                bubble = new Bubble();
        //                                bubble.areaNumber = bubblesRegion;
        //                                bubble.point = new System.Drawing.Point(posX, line);
        //                                bubble.subLine = bubblesSubLine;
        //                                if (line - bubble.point.Y > 1)
        //                                    endBubblesRegions = true;

        //                                if (prevRectangle == r1)
        //                                    continue;
        //                                prevRectangle = r1;

        //                                if (Math.Abs(r1.Width - bubble1.Width) <= caliberWidth
        //                                      && Math.Abs(r1.Height - bubble1.Height) <= caliberHeight
        //                                      && Array.IndexOf(factRectangle, r1) < 0)
        //                                {//хороший пузырь

        //                                    if (posX == 0)
        //                                    {
        //                                        firstBubblePerLineLocation = r1.Location;
        //                                        firstBubblePerLine = new Bubble();
        //                                        firstBubblePerLine.point = new System.Drawing.Point(posX, line);
        //                                    }
        //                                    if (contourLength == 10000)
        //                                        contourLength = contour1.Length * 4;
        //                                    try
        //                                    {
        //                                        if (posX > 0 && posX < bubblesPerWidth - 1)
        //                                        {
        //                                            bubble = new Bubble();
        //                                            bubble.areaNumber = bubblesRegion;
        //                                            bubble.point = new System.Drawing.Point(posX - 1, line);
        //                                            bubble.subLine = bubblesSubLine;

        //                                            if (!allContourMultiLine.Keys.Contains(bubble))
        //                                            {
        //                                                int np = posX - 1;
        //                                                for (int l = np - 1; l > -1; l--)
        //                                                {
        //                                                    np = l;
        //                                                    bubble.point = new System.Drawing.Point(np, line);
        //                                                    if (allContourMultiLine.Keys.Contains(bubble))
        //                                                    {
        //                                                        np++;
        //                                                        break;
        //                                                    }
        //                                                }
        //                                                for (int l = np; l < posX; l++)
        //                                                {
        //                                                    bubble.point = new System.Drawing.Point(l, line);
        //                                                    allContourMultiLine.Add(bubble, new System.Drawing.Point[0]);
        //                                                    Array.Resize(ref factRectangle, factRectangle.Length + 1);
        //                                                    factRectangle[factRectangle.Length - 1]
        //                                                        = new Rectangle(r1.X - diffBubble * (posX - l), r1.Y, 0, 0);
        //                                                }
        //                                            }
        //                                        }
        //                                        bubble = new Bubble();
        //                                        bubble.areaNumber = bubblesRegion;
        //                                        bubble.point = new System.Drawing.Point(posX, line);
        //                                        bubble.subLine = bubblesSubLine;
        //                                        if (!allContourMultiLine.Keys.Contains(bubble))
        //                                        {
        //                                            allContourMultiLine.Add(bubble, contour1);
        //                                            Array.Resize(ref factRectangle, factRectangle.Length + 1);
        //                                            factRectangle[factRectangle.Length - 1] = r1;
        //                                            k = r1.Bottom + 1;
        //                                        }
        //                                        else
        //                                        {
        //                                            if (bubble.Equals(prevBubble))
        //                                            {
        //                                                k += bubbleStepY;
        //                                            }
        //                                            else
        //                                            {
        //                                                int index = GetIndex(allContourMultiLine, bubble);
        //                                                if (factRectangle[index].Size == new System.Drawing.Size())//!!!
        //                                                {
        //                                                    prevBubble = bubble;
        //                                                    allContourMultiLine[bubble] = contour1;
        //                                                    factRectangle[index] = r1;
        //                                                    //удаление вставленных на всякий случай
        //                                                    //Array.Resize(ref  factRectangle, index + 1);
        //                                                    //for (int l = allContourMultiLine.Count - 1; l >= index + 1; l--)
        //                                                    //{
        //                                                    //    allContourMultiLine.Remove(allContourMultiLine.ElementAt(l).Key);
        //                                                    //}
        //                                                }
        //                                                k = r1.Bottom + 1;
        //                                            }
        //                                        }
        //                                        int tmp = (r1.X + r1.Width / 4) - xn;
        //                                        if (tmp < r1.Width / 4)
        //                                            xn = r1.X + r1.Width / 2;
        //                                    }
        //                                    catch { }
        //                                }
        //                                else
        //                                {
        //                                    #region плохой пузырь
        //                                    try
        //                                    {
        //                                        if (posX > 0)
        //                                        {
        //                                            bubble = new Bubble();
        //                                            bubble.areaNumber = bubblesRegion;
        //                                            bubble.point = new System.Drawing.Point(posX - 1, line);
        //                                            bubble.subLine = bubblesSubLine;
        //                                            if (!allContourMultiLine.Keys.Contains(bubble))
        //                                            {
        //                                                int np = posX - 1;
        //                                                for (int l = np - 1; l > -1; l--)
        //                                                {
        //                                                    np = l;
        //                                                    bubble.point = new System.Drawing.Point(np, line);
        //                                                    if (allContourMultiLine.Keys.Contains(bubble))
        //                                                    {
        //                                                        np++;
        //                                                        break;
        //                                                    }
        //                                                }
        //                                                for (int l = np; l < posX; l++)
        //                                                {
        //                                                    bubble.point = new System.Drawing.Point(l, line);
        //                                                    allContourMultiLine.Add(bubble, new System.Drawing.Point[0]);
        //                                                    Array.Resize(ref factRectangle, factRectangle.Length + 1);
        //                                                    factRectangle[factRectangle.Length - 1]
        //                                                        = new Rectangle(firstBubblePerLineLocation.X
        //                                                           , firstBubblePerLineLocation.Y
        //                                                           - diffBubble * (firstBubblePerLine.point.X - l)
        //                                                           , 0, 0);
        //                                                }
        //                                            }
        //                                        }

        //                                        bubble = new Bubble();
        //                                        bubble.areaNumber = bubblesRegion;
        //                                        bubble.point = new System.Drawing.Point(posX, line);
        //                                        bubble.subLine = bubblesSubLine;
        //                                        if (!allContourMultiLine.Keys.Contains(bubble))
        //                                        {
        //                                            allContourMultiLine.Add(bubble, new System.Drawing.Point[0]);
        //                                            Array.Resize(ref factRectangle, factRectangle.Length + 1);
        //                                            factRectangle[factRectangle.Length - 1]
        //                                                    = new Rectangle
        //                                                        (
        //                                                           firstBubblePerLineLocation.X
        //                                                         , firstBubblePerLineLocation.Y + diffBubble * (posX - firstBubblePerLine.point.X)
        //                                                         , 0
        //                                                         , 0
        //                                                         );
        //                                        }
        //                                    }
        //                                    catch (Exception)
        //                                    {
        //                                    }
        //                                    k += (diffBubble - diffBubble / 2);
        //                                    #endregion
        //                                }
        //                                #endregion
        //                            }
        //                            else
        //                            {
        //                                #region color == argbWhite
        //                                if (k >= bubblesRegions[bubblesRegion].Bottom - 1 + bubble1.Width / 4)
        //                                {
        //                                    bubble = new Bubble();
        //                                    bubble.areaNumber = bubblesRegion;
        //                                    bubble.point = bubble.point = new System.Drawing.Point(0, line);
        //                                    bubble.subLine = 0;
        //                                    if (!allContourMultiLine.ContainsKey(bubble))
        //                                    {
        //                                        correct++;
        //                                        k = xn - 1;
        //                                        switch (correct)
        //                                        {
        //                                            case 1:
        //                                                yn -= bubble1.Height / 2;
        //                                                break;
        //                                            case 2:
        //                                            case 3:
        //                                            case 4:
        //                                            case 5:
        //                                                yn += bubble1.Height;
        //                                                break;
        //                                            default:
        //                                                barCodesPrompt = PromptCalibrationError(bubble);
        //                                                break;
        //                                        }
        //                                        continue;
        //                                    }
        //                                    else
        //                                        correct = 0;
        //                                }
        //                                #endregion
        //                            }
        //                        }
        //                        #endregion
        //                    }
        //                    if (barCodesPrompt != "")
        //                        break;
        //                    if (allContourMultiLine.Count == 0)
        //                    {
        //                        barCodesPrompt = "The error in determining the regions of bubbles";
        //                        break;
        //                    }

        //                    if (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
        //                        || areas[bubblesRegion].bubblesOrientation == "horizontal")
        //                    {
        //                        #region horizontal
        //                        //try
        //                        //{
        //                        if (bubblesSubLinesCount[bubblesRegion] > 0)
        //                        {
        //                            bubblesSubLine++;
        //                            if (bubblesSubLine <= bubblesSubLinesCount[bubblesRegion])
        //                            {
        //                                yn += bubblesSubLinesStep[bubblesRegion];
        //                                line--;
        //                            }
        //                            else
        //                            {
        //                                bubblesSubLine--;
        //                                yn -= bubblesSubLinesStep[bubblesRegion] * bubblesSubLine;
        //                                bubblesSubLine = 0;
        //                                yn += (bubble1.Height + bubbleStepY);// lineFactStepY;// lineHeight[bubblesRegion];// diffBubble;// (bubble1.Height + bubbleStepY);factStepY;// 
        //                            }
        //                        }
        //                        else
        //                            yn += lineFactStepY;// (bubble1.Height + bubbleStepY);
        //                        if (bubblesSubLine == 0)
        //                        {
        //                            KeyValuePair<Bubble, System.Drawing.Point[]> kvp
        //                            = allContourMultiLine.ElementAt(allContourMultiLine.Count - 1);
        //                            bubble = kvp.Key;
        //                            if (line + 1 - bubble.point.Y == 2)
        //                            {//пустая строка
        //                                for (int l = 0; l < bubblesSubLinesCount[bubblesRegion] + 1; l++)
        //                                {
        //                                    for (int k = 0; k < bubblesPerLine[bubblesRegion]; k++)
        //                                    {
        //                                        bubble = new Bubble();
        //                                        bubble.areaNumber = bubblesRegion;
        //                                        bubble.point = new System.Drawing.Point(k, line);
        //                                        bubble.subLine = l;
        //                                        allContourMultiLine.Add(bubble, new System.Drawing.Point[0]);
        //                                        Array.Resize(ref factRectangle, factRectangle.Length + 1);
        //                                        factRectangle[factRectangle.Length - 1]
        //                                            = new Rectangle(
        //                                                     firstBubblePerLineLocation.X
        //                                                     + diffBubble * k
        //                                                   , firstBubblePerLineLocation.Y
        //                                                   , 0
        //                                                   , 0
        //                                                   );
        //                                    }
        //                                }
        //                            }
        //                            regionLines++;
        //                            if (regionLines >= bubblesPerHeight
        //                                )//|| yn >= bubblesRegions[bubblesRegion].Bottom
        //                            {
        //                                regionLines = 0;
        //                                bubblesRegion++;
        //                                if (bubblesRegions.Length > bubblesRegion)
        //                                {
        //                                    bubble1 = bubblesOfRegion[bubblesRegion];
        //                                    bubblesPerWidth = bubblesPerLine[bubblesRegion];
        //                                    bubblesPerHeight = linesPerArea[bubblesRegion];
        //                                    if (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
        //                                        || areas[bubblesRegion].bubblesOrientation == "horizontal")
        //                                    {
        //                                        xn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].X - bubble1.Width / 8));
        //                                        yn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y + bubble1.Height / 2)
        //                                           + deltaY);

        //                                        bubbleStepX = (int)Math.Round((decimal)(
        //                                            bubblesRegions[bubblesRegion].Width
        //                                            - bubble1.Width * bubblesPerWidth)
        //                                            / (bubblesPerWidth - 1));
        //                                        //bubbleStepY = (int)Math.Round((decimal)(lineHeight[bubblesRegion] - bubble1.Height));
        //                                        diffBubble = bubble1.Width + bubbleStepX;
        //                                    }
        //                                    else
        //                                    {
        //                                        xn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].X + bubble1.Width / 2));
        //                                        yn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y - bubble1.Height / 4)
        //                                           + deltaY);
        //                                        bubbleStepY = (int)Math.Round((decimal)(
        //                                            bubblesRegions[bubblesRegion].Height
        //                                            - bubble1.Height * bubblesPerWidth)
        //                                            / (bubblesPerWidth - 1));
        //                                        bubbleStepX = (int)Math.Round((decimal)(lineHeight[bubblesRegion] - bubble1.Height));
        //                                        diffBubble = bubble1.Height + bubbleStepY;
        //                                    }
        //                                }
        //                                else
        //                                    break;// endBubblesRegions = true;
        //                            }
        //                        }
        //                        //}
        //                        //catch (Exception)
        //                        //{
        //                        //}

        //                        #endregion
        //                    }
        //                    else
        //                    {
        //                        #region vertical
        //                        if (bubblesSubLinesCount[bubblesRegion] > 0)
        //                        {
        //                            bubblesSubLine++;
        //                            if (bubblesSubLine <= bubblesSubLinesCount[bubblesRegion])
        //                            {
        //                                xn += bubblesSubLinesStep[bubblesRegion];
        //                                line--;
        //                            }
        //                            else
        //                            {
        //                                bubblesSubLine--;
        //                                xn -= bubblesSubLinesStep[bubblesRegion] * bubblesSubLine;
        //                                bubblesSubLine = 0;
        //                                xn += (bubble1.Width + bubbleStepY);
        //                            }
        //                        }
        //                        else
        //                            xn += (bubble1.Width + bubbleStepY);
        //                        if (bubblesSubLine == 0)
        //                        {
        //                            KeyValuePair<Bubble, System.Drawing.Point[]> kvp
        //                             = allContourMultiLine.ElementAt(allContourMultiLine.Count - 1);
        //                            bubble = kvp.Key;
        //                            if (line + 1 - bubble.point.Y == 2)
        //                            {//пустая строка
        //                                for (int l = 0; l < bubblesSubLinesCount[bubblesRegion] + 1; l++)
        //                                {
        //                                    for (int k = 0; k < bubblesPerLine[bubblesRegion]; k++)
        //                                    {
        //                                        bubble = new Bubble();
        //                                        bubble.areaNumber = bubblesRegion;
        //                                        bubble.point = new System.Drawing.Point(k, line);
        //                                        bubble.subLine = l;
        //                                        allContourMultiLine.Add(bubble, new System.Drawing.Point[0]);
        //                                        Array.Resize(ref factRectangle, factRectangle.Length + 1);
        //                                        factRectangle[factRectangle.Length - 1]
        //                                            = new Rectangle(
        //                                                     firstBubblePerLineLocation.X
        //                                                   , firstBubblePerLineLocation.Y
        //                                                   + diffBubble * k
        //                                                   , 0
        //                                                   , 0
        //                                                   );
        //                                    }
        //                                }
        //                            }
        //                            regionLines++;
        //                            if (regionLines == bubblesPerHeight
        //                               ) //|| xn >= bubblesRegions[bubblesRegion].Right
        //                            {
        //                                regionLines = 0;
        //                                bubblesRegion++;
        //                                if (bubblesRegions.Length > bubblesRegion)
        //                                {
        //                                    bubble1 = bubblesOfRegion[bubblesRegion];
        //                                    bubblesPerWidth = bubblesPerLine[bubblesRegion];
        //                                    bubblesPerHeight = linesPerArea[bubblesRegion];
        //                                    if (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
        //                                        || areas[bubblesRegion].bubblesOrientation == "horizontal")
        //                                    {
        //                                        xn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].X - bubble1.Width / 8));
        //                                        yn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y + bubble1.Height / 2));
        //                                        bubbleStepX = (int)Math.Round((decimal)(
        //                                            bubblesRegions[bubblesRegion].Width
        //                                            - bubble1.Width * bubblesPerWidth)
        //                                            / (bubblesPerWidth - 1));
        //                                        bubbleStepY = (int)Math.Round((decimal)(lineHeight[bubblesRegion] - bubble1.Height));
        //                                        diffBubble = bubble1.Width + bubbleStepX;

        //                                    }
        //                                    else
        //                                    {
        //                                        xn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].X + bubble1.Width / 2));
        //                                        yn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y - bubble1.Height / 4));
        //                                        bubbleStepY = (int)Math.Round((decimal)(
        //                                            bubblesRegions[bubblesRegion].Height
        //                                            - bubble1.Height * bubblesPerWidth)
        //                                            / (bubblesPerWidth - 1));
        //                                        bubbleStepX = (int)Math.Round((decimal)(lineHeight[bubblesRegion] - bubble1.Height));
        //                                        diffBubble = bubble1.Height + bubbleStepY;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                        yn += deltaY;
        //                        #endregion
        //                    }
        //                    firstBubblePerLineLocation
        //                    = new System.Drawing.Point
        //                        (bubblesRegions[bubblesRegion].X
        //                        , bubblesRegions[bubblesRegion].Y + deltaY
        //                        );
        //                    #endregion
        //                }
        //                int goodBubble = 0;
        //                int baddBubble = 0;
        //                if (barCodesPrompt == null)
        //                {
        //                    goodBubble = 0;
        //                    baddBubble = 1;
        //                    barCodesPrompt = "";
        //                    goto CalibrationError;
        //                }
        //            CalcPercent:
        //                int minContourLength = int.MaxValue;
        //                int maxBubblesDist;
        //                int goodBubbleNumber = -1;
        //                const int factor = 2;
        //                int[] axisX = new int[1];
        //                int[] axisY = new int[1];
        //                if (i > 0)
        //                {
        //                    foreach (var item in maxCountRectangles)
        //                    {
        //                        item.Value.isChecked = false;
        //                    }
        //                }
        //                //Rectangle[] factRectangleMem = new Rectangle[0];
        //                //if (maxCountRectangles != null)
        //                //{
        //                //    factRectangleMem = new Rectangle[factRectangle.Length];
        //                //    Array.Copy(factRectangle, factRectangleMem, factRectangle.Length);
        //                //}

        //                for (int k = 0; k < allContourMultiLine.Count; k++)
        //                {
        //                    #region
        //                    KeyValuePair<Bubble, System.Drawing.Point[]> item = allContourMultiLine.ElementAt(k);
        //                    if (k < allContourMultiLine.Count - 1)
        //                    {
        //                        if (factRectangle.ElementAt(k).Height > 0 && factRectangle.ElementAt(k + 1).Height > 0)
        //                        {
        //                            KeyValuePair<Bubble, System.Drawing.Point[]> itm = allContourMultiLine.ElementAt(k + 1);
        //                            if (itm.Key.point.Y == item.Key.point.Y && itm.Key.subLine == item.Key.subLine)
        //                            {
        //                                if (String.IsNullOrEmpty(areas[item.Key.areaNumber].bubblesOrientation)
        //                                    || areas[item.Key.areaNumber].bubblesOrientation == "horizontal")
        //                                {
        //                                    if (Math.Abs(factRectangle.ElementAt(k).Y
        //                                        - factRectangle.ElementAt(k + 1).Y) > factRectangle.ElementAt(k).Height / 2)
        //                                    {
        //                                        barCodesPrompt = PromptCalibrationError(item.Key);
        //                                        if (i > 0)
        //                                        {
        //                                            bmp = (Bitmap)bmpPres.Clone();
        //                                            return;
        //                                        }
        //                                        else
        //                                            break;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    if (Math.Abs(factRectangle.ElementAt(k).X - factRectangle.ElementAt(k + 1).X)
        //                                        > factRectangle.ElementAt(k).Width / 2)
        //                                    {
        //                                        barCodesPrompt = PromptCalibrationError(item.Key);
        //                                        if (i > 0)
        //                                        {
        //                                            bmp = (Bitmap)bmpPres.Clone();
        //                                            return;
        //                                        }
        //                                        else
        //                                            break;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }

        //                    if (item.Value.Length > 0)
        //                        if (item.Value.Length < minContourLength)
        //                            minContourLength = item.Value.Length;

        //                    #endregion
        //                }
        //                minContourLength = minContourLength + minContourLength / 3;

        //                //Bitmap b = (Bitmap)bmp.Clone();
        //                //foreach (Rectangle item in factRectangle)
        //                //{
        //                //    using (Graphics g = Graphics.FromImage(b))
        //                //    {
        //                //        g.DrawRectangle(new Pen(Color.Red), item);
        //                //    }
        //                //}
        //                //b.Save("factRectangles.bmp", ImageFormat.Bmp);
        //                //b.Dispose();

        //                double blackBrightness = .7;
        //                if (filterType > 4)
        //                    blackBrightness = .5;
        //                else if (filterType > 1.3)
        //                    blackBrightness = .6;
        //                else if (filterType > .8)
        //                    blackBrightness = .65;

        //                if (smartResize)
        //                {
        //                    bmp = (Bitmap)bmpPres.Clone();
        //                    bmp = ResizeBitmap(bmp, factor);

        //                    //bmp.Save("ResizeBitmap2.bmp", ImageFormat.Bmp);

        //                    //if (filterType > 0 && filterType < .95)//1//.88 .65)//<= .75
        //                    //    binaryzeMap(bmp, bmp.Width, bmp.Height, 3);

        //                    //lockBitmap = new LockBitmap(bmp);
        //                    //lockBitmap.LockBits();

        //                    if (i == 0)
        //                        if (filterType > 0 && filterType < .5)
        //                            bmp = ConvertTo1Bit(ref bmp, .92f);
        //                        else if (filterType > 0 && filterType < .6)
        //                            bmp = ConvertTo1Bit(ref bmp, .9f);
        //                        else if (filterType > 0 && filterType < .7)
        //                            bmp = ConvertTo1Bit(ref bmp, .85f);
        //                        else if (filterType > 0 && filterType < .8)
        //                            bmp = ConvertTo1Bit(ref bmp, .8f);
        //                        else if (filterType > 0 && filterType < 2 && filterType > 1.3)
        //                            bmp = ConvertTo1Bit(ref bmp, .72f);
        //                        else if (filterType > 2)
        //                            bmp = ConvertTo1Bit(ref bmp, .6f);
        //                        else
        //                            bmp = ConvertTo1Bit(ref bmp, .75f);
        //                    //bmp = ResizeBitmap(bmp, factor);

        //                    //bmp.Save("ResizeBitmap3.bmp", ImageFormat.Bmp);
        //                }
        //                maxBubblesDist = 25;// 49;//
        //                goodBubbleNumber = GetGoodBubbleNumber(allContourMultiLine, factRectangle, minContourLength, goodBubbleNumber);
        //                prevCurve = new System.Drawing.Point[0];
        //                bubblesRegion = 0;
        //                bubblesPerWidth = bubblesPerLine[bubblesRegion];
        //                bubblesPerHeight = linesPerArea[bubblesRegion];
        //                int currentLine = indexOfFirstQuestion;

        //                Dictionary<Bubble, double> currentLineBubbles = new Dictionary<Bubble, double>();
        //                double bestPercent = darknessDifferenceLevel / 100;
        //                bubble = new Bubble();
        //                int prevGoodLine = 0, prevGoodLineY = 0;
        //                areaNumber = -1;
        //                double factStepY = 0;
        //                goodBubble = 0;
        //                baddBubble = 0;

        //                for (int k = 0; k < allContourMultiLine.Count; k++)
        //                {
        //                    #region калибровка, замена, распознавание пузырей
        //                    //if (token.IsCancellationRequested)
        //                    //    return;

        //                    KeyValuePair<Bubble, System.Drawing.Point[]> item = allContourMultiLine.ElementAt(k);
        //                    posX = item.Key.point.X;
        //                    posY = item.Key.point.Y;
        //                    bubblesRegion = item.Key.areaNumber;

        //                    if (areaNumber != bubblesRegion)
        //                    {
        //                        areaNumber = bubblesRegion;
        //                        //factStepY = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Height
        //                        // bubblesPerLine[bubblesRegion]));
        //                        if (String.IsNullOrEmpty(areas[0].bubblesOrientation)
        //                          || areas[0].bubblesOrientation == "horizontal")
        //                            bubbleStepY = (int)Math.Round((decimal)(lineHeight[bubblesRegion] - bubble1.Height));
        //                        else
        //                            bubbleStepY = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Height
        //                                - bubble1.Height * bubblesPerWidth) / (bubblesPerWidth - 1));
        //                        factStepY = bubble1.Height + bubbleStepY;
        //                        double factStepYCalc = factStepY;
        //                        prevGoodLineY = 0; prevGoodLine = 0;
        //                        GetLineFactStep
        //                         (
        //                           ref factStepY
        //                         , ref prevGoodLine
        //                         , ref prevGoodLineY
        //                         , k
        //                         , areas[bubblesRegion]
        //                         , factRectangle
        //                         , allContourMultiLine
        //                         , bubblesRegion
        //                         , minContourLength
        //                         );
        //                        if (!calcPercentOnly && Math.Abs(factStepYCalc - factStepY) > factStepYCalc / 4)
        //                        {
        //                            barCodesPrompt = "Calibration error 2";
        //                            bmp = (Bitmap)bmpPres.Clone();
        //                            return;
        //                        }
        //                        bubblesPerWidth = bubblesPerLine[bubblesRegion];
        //                        bubble1 = bubblesOfRegion[bubblesRegion];
        //                        if (string.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
        //                            || areas[bubblesRegion].bubblesOrientation == "horizontal")
        //                            bubbleStepX = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Width
        //                                - bubble1.Width * bubblesPerWidth) / (bubblesPerWidth - 1));
        //                        else
        //                        {
        //                            if (areas[bubblesRegion].subLinesAmount == 0)
        //                            {
        //                                bubblesSubLinesStep[bubblesRegion] = (int)Math.Round((decimal)(areas[0].bubble.Width * 2) * kx);
        //                            }
        //                            bubbleStepX = bubblesSubLinesStep[bubblesRegion];
        //                            bubbleStepY = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Height
        //                                / bubblesPerLine[bubblesRegion] - bubble1.Height));
        //                        }
        //                    }
        //                    //bool openCircuit = InsideContour(item);
        //                    if (item.Value.Length == 0 || item.Value.Length > minContourLength || !InsideContour(item))
        //                    {
        //                        #region плохой пузырь // almost like in Recognize -> BadBubble()
        //                        int dist, minDist = int.MaxValue, numCont = -1;
        //                        int n;
        //                        baddBubble++;
        //                        //if (!openCircuit)
        //                        //    if (factRectangle[k].Size!=new Size())
        //                        //        openContour = true;

        //                        KeyValuePair<Bubble, System.Drawing.Point[]> itm = allContourMultiLine.ElementAt(k);
        //                        for (int kn = 1; kn < maxBubblesDist; kn++)
        //                        {
        //                            n = k + kn;
        //                            if (n <= allContourMultiLine.Count - 1)
        //                            {
        //                                itm = allContourMultiLine.ElementAt(n);
        //                                //if ((regionRectangle.ElementAt(n).Value == regionRectangle.ElementAt(k).Value)
        //                                if ((itm.Key.areaNumber == item.Key.areaNumber)
        //                                    && itm.Value.Length != 0
        //                                    && itm.Value.Length <= minContourLength)
        //                                {
        //                                    dist = Math.Abs(item.Key.point.X - itm.Key.point.X)
        //                                         + Math.Abs(item.Key.point.Y - itm.Key.point.Y)
        //                                         + Math.Abs(item.Key.subLine - itm.Key.subLine);
        //                                    if (dist < minDist)
        //                                    {
        //                                        if (!InsideContour(itm))
        //                                            continue;
        //                                    }
        //                                    if (dist <= 1)
        //                                    {
        //                                        numCont = n;
        //                                        break;
        //                                    }
        //                                    else
        //                                    {
        //                                        if (dist < minDist)
        //                                        {
        //                                            minDist = dist;
        //                                            numCont = n;
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                            n = k - kn;
        //                            if (n > -1)
        //                            {
        //                                itm = allContourMultiLine.ElementAt(n);
        //                                if ((itm.Key.areaNumber == item.Key.areaNumber)
        //                                    && itm.Value.Length != 0 && itm.Value.Length <= minContourLength)
        //                                {
        //                                    dist = Math.Abs(item.Key.point.X - itm.Key.point.X)
        //                                         + Math.Abs(item.Key.point.Y - itm.Key.point.Y)
        //                                         + Math.Abs(item.Key.subLine - itm.Key.subLine);
        //                                    if (dist < minDist)
        //                                    {
        //                                        if (!InsideContour(itm))
        //                                            continue;
        //                                    }
        //                                    if (dist <= 1)
        //                                    {
        //                                        numCont = n;
        //                                        break;
        //                                    }
        //                                    else
        //                                    {
        //                                        if (dist < minDist)
        //                                        {
        //                                            minDist = dist;
        //                                            numCont = n;
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        if (numCont > -1)
        //                        {//itm = замещающий, item = замещаемый
        //                            int distX;
        //                            int distY;
        //                            int distYsub;
        //                            int moveX;
        //                            int moveY;
        //                            itm = allContourMultiLine.ElementAt(numCont);
        //                            if (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
        //                                  || areas[bubblesRegion].bubblesOrientation == "horizontal")
        //                            {
        //                                distX = item.Key.point.X - itm.Key.point.X;
        //                                distY = item.Key.point.Y - itm.Key.point.Y;
        //                                distYsub = item.Key.subLine - itm.Key.subLine;
        //                                moveX = (bubble1.Width + bubbleStepX) * distX;
        //                                moveY = (int)Math.Round((double)factStepY * distY
        //                                       + bubblesSubLinesStep[bubblesRegion] * distYsub);//* signY Math.Sign(itm.Key.point.Y));
        //                            }
        //                            else
        //                            {
        //                                distY = item.Key.point.X - itm.Key.point.X;
        //                                distX = item.Key.point.Y - itm.Key.point.Y;
        //                                distYsub = item.Key.subLine - itm.Key.subLine;
        //                                moveY = (int)Math.Round((double)factStepY * distY);
        //                                moveX = (int)Math.Round((double)bubbleStepX * distX
        //                                       + (bubblesSubLinesStep[bubblesRegion]) * distYsub);
        //                            }
        //                            prevCurve = new System.Drawing.Point[allContourMultiLine.ElementAt(numCont).Value.Length];
        //                            allContourMultiLine.ElementAt(numCont).Value.CopyTo(prevCurve, 0);
        //                            prevCurve = MoveContour(prevCurve, moveX, moveY);
        //                            prevRectangle = new Rectangle
        //                                (
        //                                  factRectangle[numCont].X
        //                                , factRectangle[numCont].Y
        //                                , factRectangle[numCont].Width
        //                                , factRectangle[numCont].Height
        //                                );
        //                            prevRectangle.X += moveX;
        //                            prevRectangle.Y += moveY;
        //                            factRectangle[k] = new Rectangle
        //                                (
        //                                  prevRectangle.X
        //                                , prevRectangle.Y
        //                                , prevRectangle.Width
        //                                , prevRectangle.Height
        //                                );
        //                        }
        //                        else
        //                            if (goodBubbleNumber > -1 && factRectangle[goodBubbleNumber].Size != new System.Drawing.Size())
        //                            {
        //                                if (axisX.Length == 1)
        //                                {
        //                                    GetAxis
        //                                        (
        //                                          ref axisX
        //                                        , ref axisY
        //                                        , ref factRectangle
        //                                        , allContourMultiLine
        //                                        , bubble1
        //                                        //, smartResize
        //                                        //, factor
        //                                        );

        //                                    //Bitmap b2 = (Bitmap)bmp.Clone();
        //                                    //foreach (var line in axisY)
        //                                    //{
        //                                    //    using (Graphics g = Graphics.FromImage(b2))
        //                                    //    {
        //                                    //        g.DrawLine(new Pen(Color.Red), new Point(0, line / factor)
        //                                    //            , new Point(b2.Width, line / factor));
        //                                    //    }
        //                                    //}
        //                                    //b2.Save("allAxisY.bmp", ImageFormat.Bmp);
        //                                    //b2.Dispose();


        //                                    if ((amoutOfQuestions <= bubblesPerHeight && axisY.Length < amoutOfQuestions)
        //                                        || (amoutOfQuestions >= bubblesPerHeight && axisY.Length < bubblesPerHeight))
        //                                    {
        //                                        barCodesPrompt = "Calibration error an axis \"Y\"";
        //                                        //if (i > 0)
        //                                        //{
        //                                        List<int> listAxisY = axisY.ToList();
        //                                        double lfsd = 0;
        //                                        for (int index = 0; index < listAxisY.Count - 1; index++)
        //                                        {
        //                                            //int yPrev = listAxisY[index];
        //                                            //int yNext = listAxisY[index + 1];
        //                                            int yDist = listAxisY[index + 1] - listAxisY[index];
        //                                            int lines = (int)Math.Round(((double)yDist / (lineFactStepY))); //* ky
        //                                            if (lines > 1)
        //                                            {
        //                                                int lfs = yDist / lines;
        //                                                lfsd = (double)yDist / lines;
        //                                                lineFactStepY = lfs;
        //                                                for (int j = 0; j < lines - 1; j++)
        //                                                {
        //                                                    //listAxisY.Insert(index + 1 + j, listAxisY[index] + (lfs * (j + 1)));
        //                                                    listAxisY.Insert(index + 1 + j
        //                                                        , listAxisY[index] + (int)Math.Round((double)lfsd * (j + 1)));
        //                                                }
        //                                                index += (lines - 1);
        //                                            }
        //                                        }
        //                                        if ((amoutOfQuestions <= bubblesPerHeight && listAxisY.Count < amoutOfQuestions)
        //                                           || (amoutOfQuestions >= bubblesPerHeight && axisY.Length < bubblesPerHeight))
        //                                        {
        //                                            if (amoutOfQuestions >= bubblesPerHeight && listAxisY.Count < bubblesPerHeight)
        //                                            {
        //                                                for (int j = listAxisY.Count; j < bubblesPerHeight; j++)
        //                                                {
        //                                                    listAxisY.Add(listAxisY[listAxisY.Count - 1] + lineFactStepY);
        //                                                }
        //                                            }
        //                                            else if (amoutOfQuestions <= bubblesPerHeight && listAxisY.Count < amoutOfQuestions)
        //                                            {
        //                                                int lastItem = listAxisY.Count - 1;
        //                                                int mult = 0;
        //                                                if (lfsd == 0)
        //                                                    lfsd = lineFactStepY;
        //                                                for (int j = listAxisY.Count; j < amoutOfQuestions; j++)
        //                                                {
        //                                                    mult++;
        //                                                    //listAxisY.Add(listAxisY[listAxisY.Count - 1] + lineFactStepY);
        //                                                    listAxisY.Add(listAxisY[lastItem] + (int)Math.Round((double)lfsd * mult));
        //                                                }
        //                                            }
        //                                        }
        //                                        axisY = listAxisY.ToArray();

        //                                        //Bitmap b4 = (Bitmap)bmp.Clone();
        //                                        //foreach (var line in listAxisY)
        //                                        //{
        //                                        //    using (Graphics g = Graphics.FromImage(b4))
        //                                        //    {
        //                                        //        g.DrawLine(new Pen(Color.Red), new Point(0, line / factor)
        //                                        //            , new Point(b4.Width, line / factor));
        //                                        //    }
        //                                        //}
        //                                        //b4.Save("allAxisY2.bmp", ImageFormat.Bmp);
        //                                        //b4.Dispose();

        //                                        int yb = 0;
        //                                        if (String.IsNullOrEmpty(areas[0].bubblesOrientation)
        //                                        || areas[0].bubblesOrientation == "horizontal")
        //                                        {
        //                                            yb = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y + bubble1.Height / 2));
        //                                        }
        //                                        else
        //                                        {
        //                                            yb = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y - bubble1.Height / 8));
        //                                        }

        //                                        yb += deltaY;
        //                                        if ((amoutOfQuestions <= bubblesPerHeight && axisY.Length < amoutOfQuestions)
        //                                            || (amoutOfQuestions >= bubblesPerHeight && axisY.Length < bubblesPerHeight)
        //                                            || Math.Abs(yb - axisY[0]) > lineFactStepY)
        //                                        {
        //                                            if (i == iter - 1)
        //                                            {
        //                                                bmp = (Bitmap)bmpPres.Clone();
        //                                                return;
        //                                            }
        //                                            else
        //                                                break;
        //                                        }

        //                                        barCodesPrompt = "";
        //                                        //goodBubbleNumber = GetGoodBubbleNumber(allContourMultiLine, factRectangle, minContourLength, goodBubbleNumber);
        //                                        //}
        //                                        //else
        //                                        //    break;
        //                                    }
        //                                }
        //                                prevCurve = new System.Drawing.Point[allContourMultiLine.ElementAt(goodBubbleNumber).Value.Length];
        //                                allContourMultiLine.ElementAt(goodBubbleNumber).Value.CopyTo(prevCurve, 0);
        //                                int moveX = factRectangle[k].X - factRectangle[goodBubbleNumber].X;
        //                                int moveY = factRectangle[k].Y - factRectangle[goodBubbleNumber].Y;

        //                                prevRectangle = new Rectangle
        //                                (
        //                                  factRectangle[goodBubbleNumber].X
        //                                , factRectangle[goodBubbleNumber].Y
        //                                , factRectangle[goodBubbleNumber].Width
        //                                , factRectangle[goodBubbleNumber].Height
        //                                );


        //                                prevRectangle.X += moveX;
        //                                int bestWal = int.MaxValue;
        //                                int delta = int.MaxValue;
        //                                foreach (int item2 in axisX)
        //                                {
        //                                    int delta2 = Math.Abs(prevRectangle.X - item2);
        //                                    if (delta2 < delta)// bestY)
        //                                    {
        //                                        delta = delta2;
        //                                        bestWal = item2;
        //                                    }
        //                                }
        //                                delta = bestWal - prevRectangle.X;
        //                                if (Math.Abs(delta) > bubble1.Width)
        //                                    delta = 0;
        //                                else
        //                                    prevRectangle.X = bestWal;

        //                                moveX += delta;

        //                                prevRectangle.Y += moveY;
        //                                bestWal = int.MaxValue;
        //                                delta = int.MaxValue;
        //                                foreach (int item2 in axisY)
        //                                {
        //                                    int delta2 = Math.Abs(prevRectangle.Y - item2);
        //                                    if (delta2 < delta)// bestY)
        //                                    {
        //                                        delta = delta2;
        //                                        bestWal = item2;
        //                                    }
        //                                }
        //                                delta = bestWal - prevRectangle.Y;
        //                                if (Math.Abs(delta) > bubble1.Height)
        //                                    delta = 0;
        //                                else
        //                                    prevRectangle.Y = bestWal;

        //                                moveY += delta;
        //                                prevRectangle.Y = bestWal;

        //                                prevCurve = MoveContour(prevCurve, moveX, moveY);

        //                                factRectangle[k] = new Rectangle
        //                                    (
        //                                      prevRectangle.X
        //                                    , prevRectangle.Y
        //                                    , prevRectangle.Width
        //                                    , prevRectangle.Height
        //                                    );
        //                            }
        //                            else
        //                            {//err
        //                                barCodesPrompt = PromptCalibrationError(item.Key);
        //                                if (i > 0)
        //                                {
        //                                    bmp = (Bitmap)bmpPres.Clone();
        //                                    return;
        //                                }
        //                                else
        //                                    break;
        //                            }
        //                        #endregion
        //                    }
        //                    else
        //                    {
        //                        #region хороший пузырь // аналогичен Recognize -> GoodBubble()
        //                        if (bubble.Equals(new Bubble()) && item.Key.subLine == 0)
        //                        {
        //                            bubble = item.Key;
        //                            prevGoodLineY = factRectangle[k].Y;
        //                        }
        //                        else
        //                        {
        //                            if (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
        //                                 || areas[bubblesRegion].bubblesOrientation == "horizontal")
        //                            {
        //                                if (item.Key.subLine == 0)
        //                                {//определение текущего значения lineFactStep
        //                                    if (bubble.areaNumber == item.Key.areaNumber)
        //                                    {
        //                                        if (bubble.point.Y != item.Key.point.Y)
        //                                        {
        //                                            factStepY = (double)(factRectangle[k].Y - prevGoodLineY)
        //                                                / (item.Key.point.Y - bubble.point.Y);
        //                                            bubble = item.Key;
        //                                            prevGoodLineY = factRectangle[k].Y;
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (bubble.areaNumber == item.Key.areaNumber)
        //                                {
        //                                    if (bubble.subLine == item.Key.subLine)
        //                                    {
        //                                        if ((item.Key.point.X != bubble.point.X))
        //                                        {
        //                                            factStepY = (double)(factRectangle[k].Y - prevGoodLineY)
        //                                                / (item.Key.point.X - bubble.point.X);
        //                                            bubble = item.Key;
        //                                            prevGoodLineY = factRectangle[k].Y; //item.Key.point.X;
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        #endregion
        //                        goodBubble++;
        //                        goodBubbleNumber = k;
        //                        //prevRectangle = factRectangle[k];
        //                        //prevCurve = item.Value;
        //                        prevRectangle = new Rectangle
        //                            (factRectangle[k].X
        //                            , factRectangle[k].Y
        //                            , factRectangle[k].Width
        //                            , factRectangle[k].Height
        //                            );
        //                        prevCurve = new System.Drawing.Point[item.Value.Length];
        //                        item.Value.CopyTo(prevCurve, 0);
        //                    }
        //                    if (smartResize)
        //                    {
        //                        System.Drawing.Point[] pts = prevCurve;
        //                        prevCurve = new System.Drawing.Point[pts.Length];
        //                        pts.CopyTo(prevCurve, 0);
        //                        Rectangle r = GetRectangle(ref prevCurve, factor);
        //                        prevRectangle = new Rectangle(r.X, r.Y, r.Width, r.Height);

        //                        //Bitmap b4 = (Bitmap)bmp.Clone();
        //                        //using (Graphics g = Graphics.FromImage(b4))
        //                        //{
        //                        //    g.DrawPolygon(new Pen(Color.Cyan), prevCurve);
        //                        //    g.DrawRectangle(new Pen(Color.Red), prevRectangle);//factRectangle[k] r
        //                        //    //g.DrawRectangle(new Pen(Color.Blue), factRectangle[k]);
        //                        //}
        //                        //b4.Save("allBubbles.bmp", ImageFormat.Bmp);
        //                        //b4.Dispose();

        //                    }
        //                    if (k > 0 && posX > 0 && posX - allContourMultiLine.ElementAt(k - 1).Key.point.X == 1)//item.Key.point.X>0
        //                    {
        //                        if (diffBubble < bubble1.Width * 2 && factRectangle[k].X - factRectangle[k - 1].Right > factRectangle[k].Width)
        //                        {
        //                            barCodesPrompt = PromptCalibrationError(bubble);
        //                            if (i > 1)
        //                            {
        //                                bmp = (Bitmap)bmpPres.Clone();
        //                                return;
        //                            }
        //                            else
        //                                break;
        //                        }
        //                    }
        //                    if (prevCurve.Length == 0)
        //                    {
        //                        barCodesPrompt = PromptCalibrationError(bubble);
        //                        if (i > 1)
        //                        {
        //                            bmp = (Bitmap)bmpPres.Clone();
        //                            return;
        //                        }
        //                        else
        //                            break;
        //                    }
        //                    double black = 0, all = 0;
        //                    int white = 0;
        //                    if (darknessPercent > 0)
        //                    {
        //                        #region darknessPercent > 0
        //                        using (GraphicsPath gp = new GraphicsPath())
        //                        {
        //                            gp.AddPolygon(prevCurve);
        //                            bool cont = false;
        //                            for (yn = prevRectangle.Y + 1; yn < prevRectangle.Bottom; yn++)
        //                            {
        //                                for (xn = prevRectangle.X + 1; xn < prevRectangle.Right; xn++)
        //                                {
        //                                    all++;
        //                                    if (gp.IsVisible(xn, yn))
        //                                    {
        //                                        cont = true;
        //                                        if (xn <= 0 || xn >= bmp.Width || yn <= 0 || yn >= bmp.Height)
        //                                        {
        //                                            barCodesPrompt = "Calibration error";
        //                                            bmp = (Bitmap)bmpPres.Clone();
        //                                            return;
        //                                        }
        //                                        color = bmp.GetPixel(xn, yn);

        //                                        //if (color.R != color.G || color.R != color.B)
        //                                        //{
        //                                        //    lockBitmap.UnlockBits();
        //                                        //    bmp = (Bitmap)bmpPres.Clone();
        //                                        //    BubblesRecognizeOld
        //                                        //    (
        //                                        //      out  allContourMultiLine
        //                                        //    , ref factRectangle
        //                                        //    , bmp
        //                                        //    , ref  barCodesPrompt
        //                                        //    , filterType
        //                                        //    , smartResize
        //                                        //    , bubblesRegions
        //                                        //    , bubblesOfRegion
        //                                        //    , bubblesSubLinesCount
        //                                        //    , bubblesSubLinesStep
        //                                        //    , bubblesPerLine
        //                                        //    , lineHeight, linesPerArea
        //                                        //    , answersPosition
        //                                        //    , indexAnswersPosition
        //                                        //    , totalOutput
        //                                        //    , bubbleLines
        //                                        //    , regions
        //                                        //    , areas
        //                                        //    , x1, x2, y1, y2, kx, ky
        //                                        //    , curRect, etRect
        //                                        //    , deltaY
        //                                        //    , amoutOfQuestions
        //                                        //    , indexOfFirstQuestion
        //                                        //    , maxCountRectangles
        //                                        //    , darknessPercent
        //                                        //    , darknessDifferenceLevel
        //                                        //    , indexOfFirstBubble = 0
        //                                        //    );
        //                                        //    return;
        //                                        //}
        //                                        double f = color.GetBrightness();
        //                                        //lockBitmap.SetPixel(xn, yn, Color.Red);
        //                                        //if (color != argbWhite)
        //                                        if (f <= blackBrightness)//brightness.9
        //                                            black++;
        //                                        //} 
        //                                        //catch { }
        //                                    }
        //                                    else
        //                                    {
        //                                        if (cont)
        //                                        {
        //                                            cont = false;
        //                                            break;
        //                                        }
        //                                    }
        //                                    continue;
        //                                }
        //                            }
        //                            double perCent = (black / all) * 100;
        //                            if (perCent >= darknessPercent)
        //                                currentLineBubbles.Add(item.Key, perCent);
        //                        }
        //                        #endregion
        //                    }
        //                    else
        //                    {
        //                        #region else
        //                        yn = prevRectangle.Y + prevRectangle.Height / 2;
        //                        y1 = yn;
        //                        x1 = prevRectangle.X;
        //                        xn = x1;
        //                        x2 = prevRectangle.Width / 8;
        //                        y2 = prevRectangle.Height / 8;
        //                        for (yn = yn - y2; yn <= y1 + y2; yn++)//несколько линий по "Y"
        //                        {
        //                            for (xn = x1 + x2; xn < prevRectangle.Right - x2; xn++)
        //                            {
        //                                if (xn == x1 + 3 * x2)
        //                                {
        //                                    xn += 3 * x2;
        //                                }
        //                                //color = lockBitmap.GetPixel(xn, yn);
        //                                if (xn <= 0 || xn >= bmp.Width || yn <= 0 || yn >= bmp.Height)
        //                                {
        //                                    barCodesPrompt = "Calibration error";
        //                                    bmp = (Bitmap)bmpPres.Clone();
        //                                    return;
        //                                }

        //                                color = bmp.GetPixel(xn, yn);
        //                                double f = color.GetBrightness();
        //                                //if (color != argbWhite)
        //                                if (f < .5)
        //                                {
        //                                    black++;
        //                                }
        //                                else
        //                                {
        //                                    white++;
        //                                }
        //                            }
        //                        }
        //                        if (filterType > 3)
        //                        {
        //                            if (white < black * 2)
        //                            {
        //                                currentLineBubbles.Add(item.Key, 0);
        //                                if (maxCountRectangles != null)
        //                                {
        //                                    if (!maxCountRectangles.Keys.Contains(item.Key))
        //                                    {
        //                                        if (i > 0)
        //                                        {
        //                                            bmp = (Bitmap)bmpPres.Clone();
        //                                            return;
        //                                        }
        //                                        else
        //                                            break;
        //                                    }
        //                                    maxCountRectangles[item.Key].isChecked = true;
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (filterType > 0 && filterType < .6)
        //                            {
        //                                if (white < black * 2)
        //                                {
        //                                    currentLineBubbles.Add(item.Key, 0);
        //                                    if (maxCountRectangles != null)
        //                                    {
        //                                        if (!maxCountRectangles.Keys.Contains(item.Key))
        //                                        {
        //                                            if (i > 0)
        //                                            {
        //                                                bmp = (Bitmap)bmpPres.Clone();
        //                                                return;
        //                                            }
        //                                            else
        //                                                break;
        //                                        }
        //                                        maxCountRectangles[item.Key].isChecked = true;
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (white < black)
        //                                {
        //                                    currentLineBubbles.Add(item.Key, 0);
        //                                    if (maxCountRectangles != null)
        //                                    {
        //                                        if (!maxCountRectangles.Keys.Contains(item.Key))
        //                                        {
        //                                            if (i > 0)
        //                                            {
        //                                                bmp = (Bitmap)bmpPres.Clone();
        //                                                return;
        //                                            }
        //                                            else
        //                                                break;
        //                                        }
        //                                        maxCountRectangles[item.Key].isChecked = true;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        #endregion
        //                    }
        //                    if (k >= allContourMultiLine.Count - 1
        //                        || item.Key.point.Y != allContourMultiLine.ElementAt(k + 1).Key.point.Y)
        //                    {
        //                        #region результаты по строкам
        //                        bool lineChecked = false;
        //                        if (currentLineBubbles.Count == 0)
        //                        {
        //                            lineChecked = true;
        //                        }
        //                        else
        //                        {
        //                            if (darknessPercent > 0)
        //                            {
        //                                if (currentLineBubbles.Count > 1)
        //                                {
        //                                    double maxPerCent = -1;
        //                                    //System.Drawing.Point maxPerCentKey = new System.Drawing.Point(-1, 0);
        //                                    for (int j = 0; j < currentLineBubbles.Count; j++)
        //                                    {
        //                                        KeyValuePair<Bubble, double> item1 = currentLineBubbles.ElementAt(j);
        //                                        if (maxPerCent < item1.Value)
        //                                        {
        //                                            //maxPerCentKey = item1.Key.point;
        //                                            maxPerCent = item1.Value;
        //                                        }
        //                                    }
        //                                    //maxPerCent = 0;
        //                                    //for (int j = 0; j < currentLineBubbles.Count; j++)
        //                                    //{
        //                                    //    KeyValuePair<Bubble, double> item1 = currentLineBubbles.ElementAt(j);
        //                                    //    if (item1.Key.point == maxPerCentKey)
        //                                    //    {
        //                                    //        continue;
        //                                    //    }
        //                                    //    maxPerCent += item1.Value;
        //                                    //}
        //                                    //maxPerCent /= (currentLineBubbles.Count - 1);//Count - 1 <=правильно
        //                                    for (int j = 0; j < currentLineBubbles.Count; j++)
        //                                    {
        //                                        KeyValuePair<Bubble, double> item1 = currentLineBubbles.ElementAt(j);
        //                                        //if (item1.Value > maxPerCent * bestPercent)
        //                                        if (item1.Value * bestPercent > maxPerCent)
        //                                        {
        //                                            if (maxCountRectangles != null)
        //                                            {
        //                                                if (!maxCountRectangles.Keys.Contains(currentLineBubbles.ElementAt(j).Key))
        //                                                {
        //                                                    if (i > 0)
        //                                                    {
        //                                                        bmp = (Bitmap)bmpPres.Clone();
        //                                                        return;
        //                                                    }
        //                                                    else
        //                                                        break;
        //                                                }
        //                                                maxCountRectangles[item1.Key].isChecked = true;
        //                                                lineChecked = true;
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    if (maxCountRectangles != null)
        //                                    {
        //                                        if (!maxCountRectangles.Keys.Contains(currentLineBubbles.ElementAt(0).Key))
        //                                        {
        //                                            if (i > 0)
        //                                            {
        //                                                bmp = (Bitmap)bmpPres.Clone();
        //                                                return;
        //                                            }
        //                                            else
        //                                                break;
        //                                        }
        //                                        maxCountRectangles[currentLineBubbles.ElementAt(0).Key].isChecked = true;
        //                                        lineChecked = true;
        //                                    }
        //                                }
        //                            }
        //                            if (!lineChecked)
        //                            {
        //                                for (int j = 0; j < currentLineBubbles.Count; j++)
        //                                {
        //                                    if (maxCountRectangles != null)
        //                                    {
        //                                        if (!maxCountRectangles.Keys.Contains(currentLineBubbles.ElementAt(j).Key))
        //                                        {
        //                                            if (i > 0)
        //                                            {
        //                                                bmp = (Bitmap)bmpPres.Clone();
        //                                                return;
        //                                            }
        //                                            else
        //                                                break;
        //                                        }
        //                                        maxCountRectangles[currentLineBubbles.ElementAt(j).Key].isChecked = true;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        currentLineBubbles.Clear();
        //                        currentLine++;
        //                        #endregion
        //                    }
        //                    //Bitmap b = (Bitmap)bmp.Clone();
        //                    //using (Graphics g = Graphics.FromImage(b))
        //                    //{
        //                    //    g.DrawPolygon(new Pen(Color.Cyan), prevCurve);
        //                    //    g.DrawRectangle(new Pen(Color.Red), prevRectangle);//factRectangle[k]
        //                    //}
        //                    //b.Save("strips2.bmp", ImageFormat.Bmp);
        //                    //b.Dispose();

        //                    #endregion
        //                }
        //            CalibrationError:

        //                //Bitmap b3 = (Bitmap)bmpPres.Clone();
        //                //foreach (Rectangle item in factRectangle)
        //                //{
        //                //    using (Graphics g = Graphics.FromImage(b3))
        //                //    {
        //                //        g.DrawRectangle(new Pen(Color.Red), item);
        //                //    }
        //                //}
        //                //b3.Save("factRectangles2.bmp", ImageFormat.Bmp);
        //                //b3.Dispose();

        //                double percentGoodBubble = (double)goodBubble / (goodBubble + baddBubble);
        //                if (barCodesPrompt == "" && percentGoodBubble < .05)//.05 .25 .15
        //                    barCodesPrompt = "Calibration error of bubbles";
        //                if (!string.IsNullOrEmpty(barCodesPrompt) && filterType <= 2 && i < iter - 1)//<= 1.2
        //                {
        //                    //lockBitmap.UnlockBits();
        //                    bmp = (Bitmap)bmpPres.Clone();
        //                    //bmp = ConvertTo1Bit(ref bmp);
        //                    for (int j = 0; j < 2; j++)
        //                    {
        //                        using (Bitmap b2 = new Bitmap(bmp.Width / 2, bmp.Height / 2, PixelFormat.Format24bppRgb))
        //                        {
        //                            using (Graphics g = Graphics.FromImage(b2))
        //                            {
        //                                g.InterpolationMode = InterpolationMode.HighQualityBilinear;
        //                                g.DrawImage(bmp, 0, 0, b2.Width, b2.Height);
        //                            }
        //                            using (Graphics g = Graphics.FromImage(bmp))
        //                            {
        //                                g.InterpolationMode = InterpolationMode.HighQualityBilinear;
        //                                g.DrawImage(b2, 0, 0, bmp.Width, bmp.Height);
        //                            }
        //                        }
        //                    }

        //                    //bmp.Save("Calibration error.bmp", ImageFormat.Bmp);

        //                    if (i == 0)
        //                    {
        //                        if (filterType < 1)//.95
        //                            brightness = .95;// .88;
        //                        else
        //                            brightness = .9;
        //                        int ower = caliberHeight / 2;
        //                        //if (ower < 4)
        //                        //    caliberHeight += 4;
        //                        //else
        //                        caliberHeight += ower;
        //                        //ower = caliberWidth / 2;
        //                        //if (ower < 4)
        //                        //    caliberWidth += 4;
        //                        //else
        //                        //caliberWidth += ower;
        //                    }
        //                    else
        //                        brightness = .8;
        //                }
        //                else
        //                    break;
        //            }
        //            bmp = (Bitmap)bmpPres.Clone();
        //            //bmp.Save("bmpr.bmp", ImageFormat.Bmp);
        //        }
        //        //TimeSpan ts = DateTime.Now - dt;
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    throw ex;
        //    //}
        //    //finally
        //    //{
        //    //    bmp = (Bitmap)bmpPres.Clone(); 
        //    //    //try
        //    //    //{
        //    //    //    lockBitmap.UnlockBits();
        //    //    //}
        //    //    //catch (Exception)
        //    //    //{
        //    //    //}
        //    //}
        //}

        //-------------------------------------------------------------------------
        public void BubblesRecognize
            (
              ref Dictionary<Bubble, Point[]> allContourMultiLine
            , ref Rectangle[] factRectangle
            , Bitmap bmp
            , ref string barCodesPrompt
            , double filterType
            , bool smartResize
            , Rectangle[] bubblesRegions
            , Rectangle[] bubblesOfRegion
            , int[] bubblesSubLinesCount
            , int[] bubblesSubLinesStep
            , int[] bubblesPerLine
            , int[] lineHeight
            , int[] linesPerArea
            , int answersPosition
            , int indexAnswersPosition
            , object[] totalOutput
            , string[] bubbleLines
            , Regions regions
            , RegionsArea[] areas
            , int x1, int x2, int y1, int y2
            , decimal kx, decimal ky
            , Rectangle curRect, Rectangle etRect
            , int deltaY
            , int amoutOfQuestions
            , int indexOfFirstQuestion
            , Dictionary<Bubble, CheckedBubble> maxCountRectangles
            , double darknessPercent
            , double darknessDifferenceLevel
            , int lastBannerBottom
            , int deltaX = 0
            )
        {

            //bmp.Save("BubblesRecognize.bmp");

            double brightness = .88;// .5;//
            Bitmap bmpPres = (Bitmap)bmp.Clone();
            try
            {
                //allContourMultiLine = new Dictionary<Bubble, System.Drawing.Point[]>();

                //DateTime dt = DateTime.Now;

                bool calcPercentOnly = false;
                if (factRectangle.Length > 0)
                    calcPercentOnly = true;
                Rectangle bubble1 = bubblesOfRegion[0];
                int caliberWidth = bubble1.Width / 6;//8
                int caliberHeight = bubble1.Height / 6;//8
                int maxBubblesDist = 10;//25// 49;//
                //System.Windows.Forms.Application.DoEvents();
                const int iter = 3;
                bmpPres = ConvertTo1Bit(ref bmpPres);//!!!!
                for (int i = 0; i < iter; i++)
                {
                    barCodesPrompt = "";
                    //bool openContour = false;
                    int bubblesPerWidth = 0, bubblesPerHeight = 0
                       , bubbleStepX = 0, bubbleStepY = 0, xn, yn;
                    Color color;
                    Color argbWhite = Color.FromArgb(255, 255, 255);
                    Rectangle prevRectangle = Rectangle.Empty;
                    System.Drawing.Point[] prevCurve = new System.Drawing.Point[0];
                    int maxAmoutOfQuestions = linesPerArea.Sum();
                    if (amoutOfQuestions == 0 || amoutOfQuestions > maxAmoutOfQuestions)
                        amoutOfQuestions = maxAmoutOfQuestions;
                    int bubblesRegion = 0;
                    int diffBubble = 0;
                    bubblesPerWidth = bubblesPerLine[0];
                    bubblesPerHeight = linesPerArea[0];
                    int lineFactStepY;

                    if (String.IsNullOrEmpty(areas[0].bubblesOrientation)
                        || areas[0].bubblesOrientation == "horizontal")
                    {
                        xn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].X - bubble1.Width / 8));
                        yn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y + bubble1.Height / 2));
                        bubbleStepX = (int)Math.Round((decimal)(bubblesRegions[0].Width
                            - bubble1.Width * bubblesPerWidth) / (bubblesPerWidth - 1));
                        bubbleStepY = (int)Math.Round((decimal)(lineHeight[0] - bubble1.Height));
                        lineFactStepY = lineHeight[bubblesRegion];// -bubbleStepY;
                        diffBubble = (int)((bubble1.Width + bubbleStepX));
                    }
                    else
                    {
                        xn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].X + bubble1.Width / 2));
                        yn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y - bubble1.Height / 8));
                        bubbleStepY = (int)Math.Round((decimal)(bubblesRegions[0].Height
                            - bubble1.Height * bubblesPerWidth) / (bubblesPerWidth - 1));

                        lineFactStepY = lineHeight[bubblesRegion];// -bubbleStepY;

                        bubbleStepX = (int)Math.Round((decimal)(lineHeight[0] - bubble1.Height));
                        diffBubble = bubble1.Height + bubbleStepY;
                    }
                    if (calcPercentOnly)
                        goto CalcPercent;

                    yn += deltaY;
                    Rectangle r1 = Rectangle.Empty;
                    Point[] contour1 = new Point[] { };
                    factRectangle = new Rectangle[0];
                    bool endBubblesRegions = false;

                    int posX = 0, posY = 0;
                    int correct = 0;
                    int bubblesSubLine = 0;
                    allContourMultiLine = new Dictionary<Bubble, System.Drawing.Point[]>();
                    Bubble bubble = new Bubble();
                    if (indexOfFirstQuestion == 0)
                    {
                        indexOfFirstQuestion = (areas[0].questionIndex != 0) ? (int)areas[0].questionIndex : 1;
                    }
                    int areaNumber = -1;
                    int contourLength = 10000;
                    int regionLines = 0;
                    Point firstBubblePerLineLocation = new Point(bubblesRegions[bubblesRegion].X
                        , bubblesRegions[bubblesRegion].Y + deltaY);
                    int prevGoodBubbleLine = indexOfFirstQuestion;
                    int prevGoodBubbleSubLine = 0;
                    int prevGoodBubbleLineLocationY = firstBubblePerLineLocation.Y;
                    int firstBubbleRegion = 0;
                    for (int line = indexOfFirstQuestion; line < indexOfFirstQuestion + amoutOfQuestions; line++)
                    {
                        #region for
                        if (token.IsCancellationRequested)
                            return;
                        if (allContourMultiLine.Count > 0)
                        {
                            bubble = allContourMultiLine.Last().Key;
                            if (line - bubble.point.Y > 1)
                                endBubblesRegions = true;
                        }
                        if (endBubblesRegions)
                        {
                            barCodesPrompt = "The error in determining the regions of bubbles";
                            if (i > 0)
                                return;
                            else
                                break;
                        }
                        posX = 0; posY = 0;
                        Bubble prevGoodBubble = new Bubble();
                        Bubble firstBubblePerLine = new Bubble();
                        firstBubblePerLine.subLine = bubblesSubLine;
                        firstBubblePerLine.point = new System.Drawing.Point(0, line);
                        firstBubblePerLine.areaNumber = bubblesRegion;
                        Rectangle firstBubblePerLineRectangle = new Rectangle(bubble1.X, bubble1.Y, bubble1.Width, bubble1.Height);
                        if (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
                                  || areas[bubblesRegion].bubblesOrientation == "horizontal")
                        {
                            #region horizontal
                            Rep:
                            for (int k = xn; k < bubblesRegions[bubblesRegion].Right + bubble1.Width / 4; k++)
                            {
                                bool badBubble = false;
                                if (xn <= 0 || xn >= bmpPres.Width || yn <= 0 || yn >= bmpPres.Height)
                                {
                                    barCodesPrompt = "Calibration error 9";
                                    return;
                                }
                                color = bmpPres.GetPixel(k, yn);//???заблокирована

                                //Bitmap b23 = (Bitmap)bmpPres.Clone();
                                //using (Graphics g = Graphics.FromImage(b23))
                                //{
                                //    g.DrawEllipse(new Pen(Color.Red), k - 4, yn - 4, 8, 8);
                                //}
                                //b23.Save("bubbles.bmp", ImageFormat.Bmp);
                                //b23.Dispose();

                                double f = color.GetBrightness();
                                //if (color != argbWhite)
                                if (f < brightness)
                                {
                                    #region color Not White

                                    //Bitmap b13 = (Bitmap)bmpPres.Clone();
                                    //using (Graphics g = Graphics.FromImage(b13))
                                    //{
                                    //    g.DrawEllipse(new Pen(Color.Red), k - 4, yn - 4, 8, 8);
                                    //}
                                    //b13.Save("bubbles.bmp", ImageFormat.Bmp);
                                    //b13.Dispose();

                                    contour1 = ContourFindSpeed
                                        (
                                          ref bmpPres
                                        , k
                                        , yn
                                        , 0
                                        , false
                                        , true
                                        , false
                                        , contourLength
                                        , brightness
                                        );
                                    r1 = GetRectangle(contour1);
                                    //System.Drawing.Point p;
                                    //GetOuterContourSpeed
                                    //    (
                                    //      bmpPres
                                    //    , ref contour1
                                    //    , ref r1
                                    //    , out p
                                    //    , contourLength
                                    //    , brightness
                                    //    );
                                    if (r1.Width * r1.Height < 10)
                                        continue;
                                    if (r1 == prevRectangle)
                                    {
                                        if (r1.Right < xn)//чрезмерный возврат влево
                                            brightness -= .01;
                                        continue;
                                    }
                                    prevRectangle = new Rectangle(r1.X, r1.Y, r1.Width, r1.Height);

                                    //Bitmap b2 = (Bitmap)bmpPres.Clone();
                                    //using (Graphics g = Graphics.FromImage(b2))
                                    //{
                                    //    g.DrawPolygon(new Pen(Color.Red), contour1);
                                    //    g.DrawRectangle(new Pen(Color.Green), r1);
                                    //}
                                    //b2.Save("bubbles2.bmp", ImageFormat.Bmp);
                                    //b2.Dispose();

                                    if (k >= bubblesRegions[bubblesRegion].Right - 1 + bubble1.Width / 4)
                                    {
                                        bubble = new Bubble();
                                        bubble.areaNumber = bubblesRegion;
                                        bubble.point = bubble.point = new System.Drawing.Point(0, line);
                                        bubble.subLine = 0;
                                        if (!allContourMultiLine.ContainsKey(bubble))
                                        {
                                            //if (r1.Y < lastBannerBottom)
                                            //    correct += 2;
                                            correct++;
                                            k = xn - 1;
                                            switch (correct)
                                            {
                                                case 1:
                                                    yn -= bubble1.Height / 2;
                                                    break;
                                                case 2:
                                                    yn += bubble1.Height;
                                                    break;
                                                case 3:
                                                case 4:
                                                case 5:
                                                    yn += bubble1.Height / 2;
                                                    break;
                                                default:
                                                    if (i > 1)
                                                        barCodesPrompt = PromptCalibrationError(bubble);
                                                    else
                                                        barCodesPrompt = null;
                                                    break;
                                            }
                                            if (barCodesPrompt != "")
                                                break;

                                            continue;
                                        }
                                        else
                                            correct = 0;
                                    }

                                    //if (p == new System.Drawing.Point(int.MaxValue, 0))
                                    //{
                                    //    k += bubbleStepX;
                                    //    continue;
                                    //}
                                    if (r1.Width * r1.Height <= 0)
                                    {

                                        //Bitmap b = (Bitmap)bmpPres.Clone();
                                        //using (Graphics g = Graphics.FromImage(b))
                                        //{
                                        //    g.DrawEllipse(new Pen(Color.Red, 3), r1.X - 6, r1.Y - 6, 12, 12);
                                        //}
                                        //b.Save("badBubble.bmp", ImageFormat.Bmp);
                                        //b.Dispose();

                                        k++;
                                        continue;
                                    }
                                    //bool isCalcPos = false;
                                    //CalcPos:
                                    decimal dec = (decimal)(r1.X - bubblesRegions[bubblesRegion].X) / diffBubble;
                                    if (diffBubble == 0)
                                        posX = 0;
                                    else
                                        posX = (int)Math.Round(dec);
                                    int posXReal = posX;
                                    if ((posX < 0 && contour1.Length < contourLength) || (posX == 0 && r1.Y < lastBannerBottom))
                                    {
                                        if (Math.Abs(r1.Width - bubble1.Width) <= caliberWidth
                                          && Math.Abs(r1.Height - bubble1.Height) <= caliberHeight
                                          && Array.IndexOf(factRectangle, r1) < 0)
                                        {
                                            int tmp = (bubblesRegions[bubblesRegion].X - r1.X);
                                            //if (tmp < caliberWidth / 4)
                                            //    bubblesRegions[bubblesRegion].X = r1.X;
                                            if (tmp < bubble1.Width)
                                                bubblesRegions[bubblesRegion].X = r1.X;
                                        }
                                        posX = 0;
                                    }
                                    if (posX < 0)//???
                                        posX = 0;
                                    int count;
                                    if (r1.X < xn)
                                        if (posXReal < 0 && r1.Right > xn)
                                            count = (int)Math.Round((double)(r1.Right - xn) / diffBubble);
                                        else
                                            count = (int)Math.Round((double)(r1.Right - k) / diffBubble);
                                    else
                                        count = (int)Math.Round((double)(r1.Width) / diffBubble);
                                    if (count < 0)
                                        count = 0;
                                    if (count > 1)
                                        posX += count;//вставляем на всякий случай

                                    if (posX >= bubblesPerWidth)
                                        posX = bubblesPerWidth - 1;

                                    if (contour1.Length >= contourLength)//!!!убрать?
                                        posX = bubblesPerWidth - 1;

                                    bubble = new Bubble();
                                    bubble.areaNumber = bubblesRegion;
                                    bubble.point = new System.Drawing.Point(posX, line);
                                    bubble.subLine = bubblesSubLine;
                                    bubble.areaNumber = bubblesRegion;
                                    if (line - bubble.point.Y > 1)
                                        endBubblesRegions = true;

                                    if (line == 1)//new???
                                    {
                                        if (posX > 0)
                                        {
                                            if (filterType > .9)
                                            {
                                                if (allContourMultiLine.Count == 0 && correct == 0 && count == 1)
                                                {
                                                    correct++;
                                                    k = xn - 1;
                                                    yn -= bubble1.Height / 2;
                                                    continue;
                                                }
                                            }
                                        }
                                    }

                                    if (Math.Abs(r1.Width - bubble1.Width) <= caliberWidth
                                          && Math.Abs(r1.Height - bubble1.Height) <= caliberHeight
                                          && Array.IndexOf(factRectangle, r1) < 0
                                          && InsideContour(contour1))
                                    {
                                        #region хороший пузырь

                                        //Bitmap b2 = (Bitmap)bmpPres.Clone();
                                        //using (Graphics g = Graphics.FromImage(b2))
                                        //{
                                        //    g.DrawPolygon(new Pen(Color.Red), contour1);
                                        //    g.DrawRectangle(new Pen(Color.Green), r1);
                                        //}
                                        //b2.Save("bubbles2.bmp", ImageFormat.Bmp);
                                        //b2.Dispose();

                                        if (line == 1 && correct != 0)
                                        {
                                            correct = 0;
                                            int ynCalculated = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y + bubble1.Height / 2));
                                            deltaY = ynCalculated - yn;
                                            //deltaY = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y - r1.Y));
                                            lastBannerBottom -= deltaY;
                                        }
                                        //else
                                        //{
                                        //}
                                        //if (posX == 0)
                                        //{
                                        //correct = 0;
                                        //int ynCalculated = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y + bubble1.Height / 2));
                                        //deltaY = ynCalculated - yn;
                                        //deltaY = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y - r1.Y));
                                        //lastBannerBottom -= deltaY;
                                        //}

                                        if (allContourMultiLine.Count == 0
                                            && (posX > 0 && Math.Abs(dec) < (decimal).7) || posX == 0)
                                        {
                                            posX = 0;
                                            bubblesRegions[bubblesRegion].X = r1.X;
                                        }
                                        if (posX == 0 && r1.Y + r1.Width / 2 < lastBannerBottom)
                                        {
                                            barCodesPrompt = "Aligment error an axis \"Y\"";
                                            return;
                                        }

                                        if (contourLength == 10000)
                                            contourLength = contour1.Length * 8;
                                        if (bubble.areaNumber == bubblesRegion)
                                        {
                                            if (prevGoodBubbleSubLine == bubblesSubLine && line > prevGoodBubbleLine && r1.Y > prevGoodBubbleLineLocationY)
                                                lineFactStepY = (r1.Y - prevGoodBubbleLineLocationY) / (line - prevGoodBubbleLine);
                                        }
                                        prevGoodBubbleLine = line;
                                        prevGoodBubbleLineLocationY = r1.Y;
                                        prevGoodBubbleSubLine = bubblesSubLine;
                                        if (posX == 0 && !allContourMultiLine.Keys.Contains(bubble))//!!!
                                        {
                                            firstBubblePerLineLocation = r1.Location;
                                            firstBubbleRegion = bubblesRegion;
                                            firstBubblePerLineRectangle = r1;
                                            firstBubblePerLine = new Bubble();
                                            firstBubblePerLine.point = new System.Drawing.Point(posX, line);
                                            firstBubblePerLine.subLine = bubblesSubLine;
                                        }
                                        else if (posX > 0 && posX < bubblesPerWidth)// - 1!!!
                                        {
                                            //if (bubble.Equals(prevGoodBubble))
                                            //{
                                            //    //здесь можно рассчитать фактическую длину bubblesRegions[bubblesRegion]
                                            //    if (allContourMultiLine.Count > 0
                                            //       && allContourMultiLine.ElementAt(allContourMultiLine.Count - 1).Key.point.Y == line)
                                            //    {
                                            //        if (!isCalcPos && !factRectangle[factRectangle.Length - 1].Equals(r1))
                                            //        {
                                            //            diffBubble = (r1.Left - bubblesRegions[bubblesRegion].X) / (posX + 1);// + bubbleStepX
                                            //            isCalcPos = true;
                                            //            goto CalcPos;
                                            //        }
                                            //    }
                                            //}
                                            //if (allContourMultiLine.Count > 0
                                            //    && allContourMultiLine.ElementAt(allContourMultiLine.Count - 1).Key.point.Y == line)
                                            //{
                                            //    diffBubble = (r1.Left - bubblesRegions[bubblesRegion].X) / posX;// + bubbleStepX???
                                            //}
                                            bubble = new Bubble();
                                            bubble.areaNumber = bubblesRegion;
                                            bubble.point = new System.Drawing.Point(posX - 1, line);
                                            bubble.subLine = bubblesSubLine;
                                            if (!allContourMultiLine.Keys.Contains(bubble))
                                            {
                                                int np = posX - 1;
                                                for (int l = np - 1; l > -1; l--)
                                                {
                                                    np = l;
                                                    bubble.point = new System.Drawing.Point(np, line);
                                                    if (allContourMultiLine.Keys.Contains(bubble))
                                                    {
                                                        np++;
                                                        break;
                                                    }
                                                }
                                                for (int l = np; l < posX; l++)
                                                {
                                                    bubble.point = new System.Drawing.Point(l, line);
                                                    allContourMultiLine.Add(bubble, new System.Drawing.Point[0]);
                                                    Array.Resize(ref factRectangle, factRectangle.Length + 1);
                                                    factRectangle[factRectangle.Length - 1]
                                                        = new Rectangle(r1.X - diffBubble * (posX - l), r1.Y, 0, 0);
                                                }
                                            }
                                        }
                                        bubble = new Bubble();
                                        bubble.areaNumber = bubblesRegion;
                                        bubble.point = new System.Drawing.Point(posX, line);
                                        bubble.subLine = bubblesSubLine;

                                        if (!allContourMultiLine.Keys.Contains(bubble))
                                        {
                                            allContourMultiLine.Add(bubble, contour1);
                                            Array.Resize(ref factRectangle, factRectangle.Length + 1);
                                            factRectangle[factRectangle.Length - 1] = r1;
                                            if (diffBubble > bubble1.Width * 2)
                                                k = r1.Right + diffBubble / 2;
                                            else
                                                k = r1.Right + 1;
                                        }
                                        else
                                        {
                                            if (bubble.Equals(prevGoodBubble))
                                            {
                                                if (factRectangle[factRectangle.Length - 1].Equals(r1))
                                                    k += bubbleStepX;
                                                //else
                                                //{//здесь можно рассчитать фактическую длину bubblesRegions[bubblesRegion]
                                                //    if (allContourMultiLine.Count > 0
                                                //       && allContourMultiLine.ElementAt(allContourMultiLine.Count - 1).Key.point.Y == line)
                                                //    {
                                                //        if (!isCalcPos)
                                                //        {
                                                //            diffBubble = (r1.Left - bubblesRegions[bubblesRegion].X) / posX;// + bubbleStepX
                                                //            isCalcPos = true;
                                                //            goto CalcPos;
                                                //        }
                                                //    }
                                                //}
                                            }
                                            else
                                            {
                                                prevGoodBubble = bubble;
                                                int index = GetIndex(allContourMultiLine, bubble);
                                                if (factRectangle[index].Size == new System.Drawing.Size())//!!!
                                                {
                                                    allContourMultiLine[bubble] = contour1;
                                                    factRectangle[index] = r1;
                                                    //удаление вставленных на всякий случай
                                                    //Array.Resize(ref  factRectangle, index + 1);
                                                    //for (int l = allContourMultiLine.Count - 1; l >= index + 1; l--)
                                                    //{
                                                    //    allContourMultiLine.Remove(allContourMultiLine.ElementAt(l).Key);
                                                    //}
                                                }
                                                k = r1.Right + 1;
                                            }
                                        }
                                        int tmp = (r1.Y + r1.Height / 4) - yn;
                                        if (tmp < r1.Height / 4)
                                            yn = r1.Y + r1.Height / 2;
                                        //int tmp = (r1.Y + r1.Height / 2) - yn;
                                        //if (Math.Abs(tmp) < r1.Height / 4)
                                        //    yn = r1.Y + r1.Height / 2;
                                        prevGoodBubble = bubble;
                                        #endregion
                                    }
                                    else
                                    {
                                        #region плохой пузырь

                                        //Bitmap b3 = (Bitmap)bmpPres.Clone();
                                        //using (Graphics g = Graphics.FromImage(b3))
                                        //{
                                        //    g.DrawPolygon(new Pen(Color.Red), contour1);
                                        //    g.DrawRectangle(new Pen(Color.Green), r1);
                                        //}
                                        //b3.Save("bubbles2.bmp", ImageFormat.Bmp);
                                        //b3.Dispose();

                                        //if (line == 1)
                                        //{
                                        //    if (posX > 0)
                                        //    {
                                        //        if (filterType > .9)
                                        //        {
                                        //            if (allContourMultiLine.Count == 0 && correct == 0)
                                        //            {
                                        //                correct++;
                                        //                k = xn - 1;
                                        //                yn -= bubble1.Height / 2;
                                        //                continue;
                                        //            }
                                        //        }
                                        //    }
                                        //}

                                        if (correct == 1 && r1.Y < lastBannerBottom)//&& count > bubblesPerWidth
                                        {//врезались в баннер
                                            correct++;
                                            yn += bubble1.Height;
                                            goto Rep;
                                        }

                                        try
                                        {
                                            if (posXReal > 0 && allContourMultiLine.Count == 0 && !badBubble)
                                            {
                                                if (i > 1)
                                                {
                                                    barCodesPrompt = "Aligment error an axis \"X\"";
                                                    return;
                                                }
                                            }

                                            if (posX > 0)// && posX < bubblesPerWidth - 1
                                            {
                                                bubble = new Bubble();
                                                bubble.areaNumber = bubblesRegion;
                                                bubble.point = new System.Drawing.Point(posX - 1, line);
                                                bubble.subLine = bubblesSubLine;
                                                if (posX != 0 && r1.Y < lastBannerBottom)// 
                                                {
                                                    if (k - xn > bubble1.Width + bubble1.Width / 2)
                                                    {//кривой баннер
                                                        if (prevGoodBubbleLine != bubble.point.Y
                                                            && prevGoodBubbleSubLine != bubble.subLine)
                                                        {
                                                            k = xn - 1;
                                                            yn += bubble1.Height;
                                                            continue;
                                                        }
                                                    }
                                                }

                                                if (!allContourMultiLine.Keys.Contains(bubble))
                                                {
                                                    int np = posX - 1;
                                                    for (int l = np - 1; l > -1; l--)
                                                    {
                                                        np = l;
                                                        bubble.point = new System.Drawing.Point(np, line);
                                                        if (allContourMultiLine.Keys.Contains(bubble))
                                                        {
                                                            np++;
                                                            break;
                                                        }
                                                    }

                                                    for (int l = np; l < posX; l++)
                                                    {
                                                        bubble.point = new System.Drawing.Point(l, line);
                                                        allContourMultiLine.Add(bubble, new System.Drawing.Point[0]);
                                                        Array.Resize(ref factRectangle, factRectangle.Length + 1);

                                                        //using (Graphics g = Graphics.FromImage(bmpPres))
                                                        //{
                                                        //    g.DrawPolygon(new Pen(Color.Red), contour1);
                                                        //    g.DrawRectangle(new Pen(Color.Green), r1);
                                                        //}
                                                        //bmpPres.Save("bubbles.bmp", ImageFormat.Bmp);

                                                        SetFactRectangle
                                                            (
                                                              factRectangle
                                                            , bubbleStepY
                                                            , diffBubble
                                                            , ref firstBubblePerLineLocation
                                                            , ref prevGoodBubbleLine
                                                            , line
                                                            , ref firstBubblePerLine
                                                            , ref firstBubblePerLineRectangle
                                                            , l
                                                            , ref firstBubbleRegion
                                                            , bubblesRegion
                                                            , bubblesSubLine
                                                            , ref prevGoodBubbleLineLocationY
                                                            , ref prevGoodBubbleSubLine
                                                            , bubblesSubLinesStep[bubblesRegion]
                                                            , lineFactStepY
                                                            );

                                                        //Bitmap b1 = (Bitmap)bmpPres.Clone();
                                                        //using (Graphics g = Graphics.FromImage(b1))
                                                        //{
                                                        //    g.DrawRectangle(new Pen(Color.Red)
                                                        //        , new Rectangle(factRectangle[factRectangle.Length - 1].X
                                                        //                      , factRectangle[factRectangle.Length - 1].Y
                                                        //                      , bubble1.Width
                                                        //                      , bubble1.Height));
                                                        //}
                                                        //b1.Save("factRectangle.bmp", ImageFormat.Bmp);
                                                        //b1.Dispose();

                                                        if (i > 0 && bubble.point.X == 0 && bubble.point.Y > 1
                                                            && r1.Y - factRectangle[factRectangle.Length - 2].Y >
                                                            diffBubble + diffBubble / 2)
                                                        {
                                                            barCodesPrompt = "Calibration error 7";
                                                            return;
                                                        }
                                                    }
                                                }
                                            }

                                            bubble = new Bubble();
                                            bubble.areaNumber = bubblesRegion;
                                            bubble.point = new System.Drawing.Point(posX, line);
                                            bubble.subLine = bubblesSubLine;

                                            if (!allContourMultiLine.Keys.Contains(bubble))
                                            {
                                                allContourMultiLine.Add(bubble, new System.Drawing.Point[0]);
                                                Array.Resize(ref factRectangle, factRectangle.Length + 1);
                                                SetFactRectangle
                                                    (
                                                      factRectangle
                                                    , bubbleStepY
                                                    , diffBubble
                                                    , ref firstBubblePerLineLocation
                                                    , ref prevGoodBubbleLine
                                                    , line
                                                    , ref firstBubblePerLine
                                                    , ref firstBubblePerLineRectangle
                                                    , posX
                                                    , ref firstBubbleRegion
                                                    , bubblesRegion
                                                    , bubblesSubLine
                                                    , ref prevGoodBubbleLineLocationY
                                                    , ref prevGoodBubbleSubLine
                                                    , bubblesSubLinesStep[bubblesRegion]
                                                    , lineFactStepY
                                                    );

                                                //Bitmap b1 = (Bitmap)bmpPres.Clone();
                                                //using (Graphics g = Graphics.FromImage(b1))
                                                //{
                                                //    g.DrawRectangle(new Pen(Color.Red)
                                                //        , new Rectangle(factRectangle[factRectangle.Length - 1].X
                                                //                      , factRectangle[factRectangle.Length - 1].Y
                                                //                      , bubble1.Width
                                                //                      , bubble1.Height));
                                                //}
                                                //b1.Save("factRectangle.bmp", ImageFormat.Bmp);
                                                //b1.Dispose();

                                            }
                                        }
                                        catch (Exception)
                                        {
                                        }
                                        k++;
                                        #endregion
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region color == argbWhite

                                    //Bitmap b = (Bitmap)bmpPres.Clone();
                                    //using (Graphics g = Graphics.FromImage(b))
                                    //{
                                    //    g.DrawEllipse(new Pen(Color.Red), k - 4, yn - 4, 8, 8);
                                    //}
                                    //b.Save("strips.bmp", ImageFormat.Bmp);
                                    //b.Dispose();

                                    if (k >= bubblesRegions[bubblesRegion].Right - 1 + bubble1.Width / 4)
                                    {
                                        bubble = new Bubble();
                                        bubble.areaNumber = bubblesRegion;
                                        bubble.point = bubble.point = new System.Drawing.Point(0, line);
                                        bubble.subLine = 0;



                                        if (!allContourMultiLine.ContainsKey(bubble))
                                        {
                                            //if (r1.Y < lastBannerBottom)
                                            //    correct += 2;
                                            correct++;
                                            k = xn - 1;
                                            switch (correct)
                                            {
                                                case 1:
                                                    yn -= bubble1.Height / 2;
                                                    break;
                                                case 2:
                                                    yn += bubble1.Height;
                                                    break;
                                                case 3:
                                                    //case 4:
                                                    //case 5:
                                                    yn += bubble1.Height / 2;
                                                    break;
                                                default:
                                                    if (i > 1)//сделать то же для vertical
                                                        barCodesPrompt = PromptCalibrationError(bubble);
                                                    else
                                                        barCodesPrompt = null;
                                                    break;
                                            }
                                            if (barCodesPrompt != "")
                                                break;

                                            continue;
                                        }
                                        else
                                            correct = 0;
                                    }
                                    #endregion
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            #region vertical
                            for (int k = yn; k < bubblesRegions[bubblesRegion].Bottom + bubble1.Height / 8; k++)
                            {
                                color = argbWhite;
                                try
                                {
                                    color = bmpPres.GetPixel(xn, k);
                                }
                                catch (Exception)
                                {
                                    break;
                                }

                                //Bitmap b23 = (Bitmap)bmpPres.Clone();
                                //using (Graphics g = Graphics.FromImage(b23))
                                //{
                                //    g.DrawEllipse(new Pen(Color.Red), xn - 4, k - 4, 8, 8);
                                //}
                                //b23.Save("bubbles.bmp", ImageFormat.Bmp);
                                //b23.Dispose();

                                double f = color.GetBrightness();
                                //if (color != argbWhite)
                                if (f < brightness)
                                {
                                    #region color != argbWhite
                                    contour1 = ContourFindSpeed
                                        (
                                          ref bmpPres
                                        , xn
                                        , k
                                        , 2//вниз
                                        , false
                                        , true
                                        , false
                                        , contourLength
                                        , brightness
                                        );
                                    System.Drawing.Point p;
                                    GetOuterContourSpeed
                                       (
                                         ref bmpPres
                                       , ref contour1
                                       , ref r1
                                       , out p
                                       , contourLength
                                       , brightness
                                    );
                                    if (contour1 == null
                                        || r1.Width * r1.Height < 10)
                                        continue;

                                    if (p == new System.Drawing.Point(int.MaxValue, 0))
                                    {
                                        k += bubbleStepY;
                                        continue;
                                    }

                                    //Bitmap b3 = (Bitmap)bmpPres.Clone();
                                    //using (Graphics g = Graphics.FromImage(b3))
                                    //{
                                    //    g.DrawPolygon(new Pen(Color.Red), contour1);
                                    //    g.DrawRectangle(new Pen(Color.Green), r1);
                                    //}
                                    //b3.Save("bubbles2.bmp", ImageFormat.Bmp);
                                    //b3.Dispose();

                                    posX = (int)Math.Round((decimal)(r1.Y - bubblesRegions[bubblesRegion].Y) / diffBubble);
                                    if (posX < 0 && contour1.Length < contourLength)
                                    {
                                        if (Math.Abs(r1.Width - bubble1.Width) <= caliberWidth
                                          && Math.Abs(r1.Height - bubble1.Height) <= caliberHeight
                                          && Array.IndexOf(factRectangle, r1) < 0
                                          && InsideContour(contour1))
                                        {
                                            int tmp = (bubblesRegions[bubblesRegion].Y - r1.Y);
                                            if (tmp < caliberHeight / 4)
                                                bubblesRegions[bubblesRegion].Y = r1.Y;
                                        }
                                        posX = 0;
                                    }
                                    if (posX < 0)
                                        posX = 0;
                                    int count = (int)Math.Round((double)(r1.Bottom - k) / diffBubble);
                                    //if (count == 0)
                                    //{
                                    //    continue;
                                    //} 
                                    if (count > 1)
                                        posX += count;//вставляем на всякий случай

                                    if (posX >= bubblesPerWidth)
                                        posX = bubblesPerWidth - 1;
                                    //if (contour1.Length >= contourLength)
                                    //    posX = bubblesPerWidth - 1;

                                    bubble = new Bubble();
                                    bubble.areaNumber = bubblesRegion;
                                    bubble.point = new System.Drawing.Point(posX, line);
                                    bubble.subLine = bubblesSubLine;
                                    if (line - bubble.point.Y > 1)
                                        endBubblesRegions = true;

                                    if (prevRectangle == r1)
                                        continue;
                                    prevRectangle = r1;
                                    //deltaY = 0;//!!!
                                    if (Math.Abs(r1.Width - bubble1.Width) <= caliberWidth
                                          && Math.Abs(r1.Height - bubble1.Height) <= caliberHeight
                                          && Array.IndexOf(factRectangle, r1) < 0)
                                    {//хороший пузырь

                                        if (posX == 0 && bubblesRegion == 0)//&& correct != 0
                                        {
                                            deltaY = -(int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y - r1.Y));
                                            lastBannerBottom -= deltaY;
                                        }


                                        if (posX == 0)
                                        {
                                            firstBubblePerLineLocation = r1.Location;
                                            firstBubblePerLine = new Bubble();
                                            firstBubblePerLine.point = new System.Drawing.Point(posX, line);
                                        }
                                        if (contourLength == 10000)
                                            contourLength = contour1.Length * 4;
                                        try
                                        {
                                            if (posX > 0 && posX < bubblesPerWidth)// - 1!!!
                                            {
                                                bubble = new Bubble();
                                                bubble.areaNumber = bubblesRegion;
                                                bubble.point = new System.Drawing.Point(posX - 1, line);
                                                bubble.subLine = bubblesSubLine;

                                                if (!allContourMultiLine.Keys.Contains(bubble))
                                                {
                                                    int np = posX - 1;
                                                    for (int l = np - 1; l > -1; l--)
                                                    {
                                                        np = l;
                                                        bubble.point = new System.Drawing.Point(np, line);
                                                        if (allContourMultiLine.Keys.Contains(bubble))
                                                        {
                                                            np++;
                                                            break;
                                                        }
                                                    }
                                                    for (int l = np; l < posX; l++)
                                                    {
                                                        bubble.point = new System.Drawing.Point(l, line);
                                                        allContourMultiLine.Add(bubble, new System.Drawing.Point[0]);
                                                        Array.Resize(ref factRectangle, factRectangle.Length + 1);
                                                        factRectangle[factRectangle.Length - 1]
                                                            = new Rectangle(r1.X - diffBubble * (posX - l), r1.Y, 0, 0);
                                                    }
                                                }
                                            }
                                            bubble = new Bubble();
                                            bubble.areaNumber = bubblesRegion;
                                            bubble.point = new System.Drawing.Point(posX, line);
                                            bubble.subLine = bubblesSubLine;
                                            if (!allContourMultiLine.Keys.Contains(bubble))
                                            {
                                                allContourMultiLine.Add(bubble, contour1);
                                                Array.Resize(ref factRectangle, factRectangle.Length + 1);
                                                factRectangle[factRectangle.Length - 1] = r1;
                                                k = r1.Bottom + 1;
                                            }
                                            else
                                            {
                                                if (bubble.Equals(prevGoodBubble))
                                                {
                                                    k += bubbleStepY;
                                                }
                                                else
                                                {
                                                    int index = GetIndex(allContourMultiLine, bubble);
                                                    if (factRectangle[index].Size == new System.Drawing.Size())//!!!
                                                    {
                                                        prevGoodBubble = bubble;
                                                        allContourMultiLine[bubble] = contour1;
                                                        factRectangle[index] = r1;
                                                        //удаление вставленных на всякий случай
                                                        //Array.Resize(ref  factRectangle, index + 1);
                                                        //for (int l = allContourMultiLine.Count - 1; l >= index + 1; l--)
                                                        //{
                                                        //    allContourMultiLine.Remove(allContourMultiLine.ElementAt(l).Key);
                                                        //}
                                                    }
                                                    k = r1.Bottom + 1;
                                                }
                                            }
                                            int tmp = (r1.X + r1.Width / 4) - xn;
                                            if (tmp < r1.Width / 4)
                                                xn = r1.X + r1.Width / 2;
                                        }
                                        catch { }
                                    }
                                    else
                                    {
                                        #region плохой пузырь
                                        try
                                        {
                                            if (posX > 0)
                                            {
                                                bubble = new Bubble();
                                                bubble.areaNumber = bubblesRegion;
                                                bubble.point = new System.Drawing.Point(posX - 1, line);
                                                bubble.subLine = bubblesSubLine;
                                                if (!allContourMultiLine.Keys.Contains(bubble))
                                                {
                                                    int np = posX - 1;
                                                    for (int l = np - 1; l > -1; l--)
                                                    {
                                                        np = l;
                                                        bubble.point = new System.Drawing.Point(np, line);
                                                        if (allContourMultiLine.Keys.Contains(bubble))
                                                        {
                                                            np++;
                                                            break;
                                                        }
                                                    }
                                                    for (int l = np; l < posX; l++)
                                                    {
                                                        bubble.point = new System.Drawing.Point(l, line);
                                                        allContourMultiLine.Add(bubble, new System.Drawing.Point[0]);
                                                        Array.Resize(ref factRectangle, factRectangle.Length + 1);
                                                        factRectangle[factRectangle.Length - 1]
                                                            = new Rectangle(firstBubblePerLineLocation.X
                                                               , firstBubblePerLineLocation.Y
                                                               - diffBubble * (firstBubblePerLine.point.X - l)
                                                               , 0, 0);
                                                    }
                                                }
                                            }

                                            bubble = new Bubble();
                                            bubble.areaNumber = bubblesRegion;
                                            bubble.point = new System.Drawing.Point(posX, line);
                                            bubble.subLine = bubblesSubLine;
                                            if (!allContourMultiLine.Keys.Contains(bubble))
                                            {
                                                allContourMultiLine.Add(bubble, new System.Drawing.Point[0]);
                                                Array.Resize(ref factRectangle, factRectangle.Length + 1);
                                                factRectangle[factRectangle.Length - 1]
                                                        = new Rectangle
                                                            (
                                                               firstBubblePerLineLocation.X
                                                             , firstBubblePerLineLocation.Y + diffBubble * (posX - firstBubblePerLine.point.X)
                                                             , 0
                                                             , 0
                                                             );
                                            }
                                        }
                                        catch (Exception)
                                        {
                                        }
                                        k += (diffBubble - diffBubble / 2);
                                        #endregion
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region color == argbWhite
                                    if (k >= bubblesRegions[bubblesRegion].Bottom - 1 + bubble1.Height / 8)
                                    {
                                        if (bubble.subLine > 0)
                                        {
                                            if (bubble.point.X < bubblesPerLine[bubblesRegion] - 1)
                                            {
                                                int bubbleSubline = bubble.subLine;
                                                for (int l = bubble.point.X + 1; l < bubblesPerLine[bubblesRegion]; l++)
                                                {
                                                    bubble = new Bubble();
                                                    bubble.areaNumber = bubblesRegion;
                                                    bubble.point = new System.Drawing.Point(l, line);
                                                    bubble.subLine = bubbleSubline;
                                                    allContourMultiLine.Add(bubble, new System.Drawing.Point[0]);
                                                    Array.Resize(ref factRectangle, factRectangle.Length + 1);
                                                    factRectangle[factRectangle.Length - 1]
                                                        = new Rectangle(
                                                                 firstBubblePerLineLocation.X
                                                               , firstBubblePerLineLocation.Y
                                                               + diffBubble * l
                                                               , 0
                                                               , 0
                                                               );
                                                }
                                            }
                                        }
                                        else
                                        {
                                            bubble = new Bubble();
                                            bubble.areaNumber = bubblesRegion;
                                            bubble.point = bubble.point = new System.Drawing.Point(0, line);
                                            bubble.subLine = 0;
                                            if (!allContourMultiLine.ContainsKey(bubble))
                                            {
                                                correct++;
                                                k = xn - 1;
                                                switch (correct)
                                                {
                                                    case 1:
                                                        yn -= bubble1.Height / 2;
                                                        break;
                                                    case 2:
                                                    case 3:
                                                    case 4:
                                                    case 5:
                                                        yn += bubble1.Height;
                                                        break;
                                                    default:
                                                        if (i > 1)//сделать то же для vertical
                                                            barCodesPrompt = PromptCalibrationError(bubble);
                                                        else
                                                            barCodesPrompt = null;
                                                        break;
                                                }
                                                if (barCodesPrompt != "")
                                                    break;

                                                continue;
                                            }
                                            else
                                                correct = 0;

                                            //            barCodesPrompt = PromptCalibrationError(bubble);
                                            //            break;
                                            //    }
                                            //    continue;
                                            //}
                                            //else
                                            //    correct = 0;
                                        }
                                    }
                                    #endregion
                                }
                            }
                            #endregion
                        }
                        if (barCodesPrompt != "")
                            break;
                        if (allContourMultiLine.Count == 0)
                        {
                            barCodesPrompt = "The error in determining the regions of bubbles";
                            break;
                        }

                        if (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
                            || areas[bubblesRegion].bubblesOrientation == "horizontal")
                        {
                            #region horizontal
                            //try
                            //{
                            if (bubblesSubLinesCount[bubblesRegion] > 0)
                            {
                                bubblesSubLine++;
                                if (bubblesSubLine <= bubblesSubLinesCount[bubblesRegion])
                                {
                                    yn += bubblesSubLinesStep[bubblesRegion];
                                    line--;
                                }
                                else
                                {
                                    bubblesSubLine--;
                                    yn -= bubblesSubLinesStep[bubblesRegion] * bubblesSubLine;
                                    bubblesSubLine = 0;
                                    yn += (bubble1.Height + bubbleStepY);// lineFactStepY;// lineHeight[bubblesRegion];// diffBubble;// (bubble1.Height + bubbleStepY);factStepY;// 
                                }
                            }
                            else
                                yn += lineFactStepY;// (bubble1.Height + bubbleStepY);
                            if (bubblesSubLine == 0)
                            {
                                KeyValuePair<Bubble, System.Drawing.Point[]> kvp
                                = allContourMultiLine.ElementAt(allContourMultiLine.Count - 1);
                                bubble = kvp.Key;
                                if (line + 1 - bubble.point.Y == 2)
                                {//пустая строка
                                    for (int l = 0; l < bubblesSubLinesCount[bubblesRegion] + 1; l++)
                                    {
                                        for (int k = 0; k < bubblesPerLine[bubblesRegion]; k++)
                                        {
                                            bubble = new Bubble();
                                            bubble.areaNumber = bubblesRegion;
                                            bubble.point = new System.Drawing.Point(k, line);
                                            bubble.subLine = l;
                                            allContourMultiLine.Add(bubble, new System.Drawing.Point[0]);
                                            Array.Resize(ref factRectangle, factRectangle.Length + 1);
                                            factRectangle[factRectangle.Length - 1]
                                                = new Rectangle(
                                                         firstBubblePerLineLocation.X
                                                         + diffBubble * k
                                                       , firstBubblePerLineLocation.Y
                                                       , 0
                                                       , 0
                                                       );
                                        }
                                    }
                                }
                                regionLines++;
                                if (regionLines >= bubblesPerHeight
                                    )//|| yn >= bubblesRegions[bubblesRegion].Bottom
                                {
                                    regionLines = 0;
                                    bubblesRegion++;
                                    if (bubblesRegions.Length > bubblesRegion)
                                    {
                                        bubble1 = bubblesOfRegion[bubblesRegion];
                                        bubblesPerWidth = bubblesPerLine[bubblesRegion];
                                        bubblesPerHeight = linesPerArea[bubblesRegion];
                                        if (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
                                            || areas[bubblesRegion].bubblesOrientation == "horizontal")
                                        {
                                            xn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].X - bubble1.Width / 8));
                                            yn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y + bubble1.Height / 2)
                                               + deltaY);

                                            bubbleStepX = (int)Math.Round((decimal)(
                                                bubblesRegions[bubblesRegion].Width
                                                - bubble1.Width * bubblesPerWidth)
                                                / (bubblesPerWidth - 1));
                                            //bubbleStepY = (int)Math.Round((decimal)(lineHeight[bubblesRegion] - bubble1.Height));
                                            diffBubble = bubble1.Width + bubbleStepX;
                                        }
                                        else
                                        {
                                            xn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].X + bubble1.Width / 2));
                                            yn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y - bubble1.Height / 4)
                                               + deltaY);
                                            bubbleStepY = (int)Math.Round((decimal)(
                                                bubblesRegions[bubblesRegion].Height
                                                - bubble1.Height * bubblesPerWidth)
                                                / (bubblesPerWidth - 1));
                                            bubbleStepX = (int)Math.Round((decimal)(lineHeight[bubblesRegion] - bubble1.Height));
                                            diffBubble = bubble1.Height + bubbleStepY;
                                        }
                                    }
                                    else
                                        break;// endBubblesRegions = true;
                                }
                            }
                            //}
                            //catch (Exception)
                            //{
                            //}

                            #endregion
                        }
                        else
                        {
                            #region vertical
                            if (bubblesSubLinesCount[bubblesRegion] > 0)
                            {
                                bubblesSubLine++;
                                if (bubblesSubLine <= bubblesSubLinesCount[bubblesRegion])
                                {
                                    xn += bubblesSubLinesStep[bubblesRegion];
                                    line--;
                                }
                                else
                                {
                                    bubblesSubLine--;
                                    xn -= bubblesSubLinesStep[bubblesRegion] * bubblesSubLine;
                                    bubblesSubLine = 0;
                                    xn += (bubble1.Width + bubbleStepY);
                                }
                            }
                            else
                                xn += (bubble1.Width + bubbleStepY);
                            if (bubblesSubLine == 0)
                            {
                                KeyValuePair<Bubble, System.Drawing.Point[]> kvp
                                 = allContourMultiLine.ElementAt(allContourMultiLine.Count - 1);
                                bubble = kvp.Key;
                                if (line + 1 - bubble.point.Y == 2)
                                {//пустая строка
                                    for (int l = 0; l < bubblesSubLinesCount[bubblesRegion] + 1; l++)
                                    {
                                        for (int k = 0; k < bubblesPerLine[bubblesRegion]; k++)
                                        {
                                            bubble = new Bubble();
                                            bubble.areaNumber = bubblesRegion;
                                            bubble.point = new System.Drawing.Point(k, line);
                                            bubble.subLine = l;
                                            allContourMultiLine.Add(bubble, new System.Drawing.Point[0]);
                                            Array.Resize(ref factRectangle, factRectangle.Length + 1);
                                            factRectangle[factRectangle.Length - 1]
                                                = new Rectangle(
                                                         firstBubblePerLineLocation.X
                                                       , firstBubblePerLineLocation.Y
                                                       + diffBubble * k
                                                       , 0
                                                       , 0
                                                       );
                                        }
                                    }
                                }
                                regionLines++;
                                if (regionLines == bubblesPerHeight
                                   ) //|| xn >= bubblesRegions[bubblesRegion].Right
                                {
                                    regionLines = 0;
                                    bubblesRegion++;
                                    if (bubblesRegions.Length > bubblesRegion)
                                    {
                                        bubble1 = bubblesOfRegion[bubblesRegion];
                                        bubblesPerWidth = bubblesPerLine[bubblesRegion];
                                        bubblesPerHeight = linesPerArea[bubblesRegion];
                                        if (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
                                            || areas[bubblesRegion].bubblesOrientation == "horizontal")
                                        {
                                            xn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].X - bubble1.Width / 8));
                                            yn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y + bubble1.Height / 2));
                                            bubbleStepX = (int)Math.Round((decimal)(
                                                bubblesRegions[bubblesRegion].Width
                                                - bubble1.Width * bubblesPerWidth)
                                                / (bubblesPerWidth - 1));
                                            bubbleStepY = (int)Math.Round((decimal)(lineHeight[bubblesRegion] - bubble1.Height));
                                            diffBubble = bubble1.Width + bubbleStepX;

                                        }
                                        else
                                        {
                                            xn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].X + bubble1.Width / 2));
                                            yn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y - bubble1.Height / 4));
                                            bubbleStepY = (int)Math.Round((decimal)(
                                                bubblesRegions[bubblesRegion].Height
                                                - bubble1.Height * bubblesPerWidth)
                                                / (bubblesPerWidth - 1));
                                            bubbleStepX = (int)Math.Round((decimal)(lineHeight[bubblesRegion] - bubble1.Height));
                                            diffBubble = bubble1.Height + bubbleStepY;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            yn += deltaY;
                            #endregion
                        }
                        firstBubblePerLineLocation
                        = new System.Drawing.Point
                            (bubblesRegions[bubblesRegion].X
                            , bubblesRegions[bubblesRegion].Y + deltaY
                            );
                        #endregion
                    }
                    int goodBubble = 0;
                    int baddBubble = 0;
                    if (barCodesPrompt == null)
                    {
                        goodBubble = 0;
                        baddBubble = 1;
                        barCodesPrompt = "";
                        goto CalibrationError;
                    }
                    CalcPercent:
                    int minContourLength = int.MaxValue;
                    //int maxBubblesDist;
                    int goodBubbleNumber = -1;
                    const int factor = 2;
                    int[] axisX = new int[1];
                    int[] axisY = new int[1];
                    int[] axisYSubline = new int[1];
                    if (i > 0)
                    {
                        //maxCountRectangles = AddMaxCountRectangles();
                        foreach (var item in maxCountRectangles)
                        {
                            item.Value.isChecked = false;
                            item.Value.rectangle = new Rectangle();
                        }
                    }

                    for (int k = 0; k < allContourMultiLine.Count; k++)
                    {
                        #region
                        KeyValuePair<Bubble, System.Drawing.Point[]> item = allContourMultiLine.ElementAt(k);
                        if (k < allContourMultiLine.Count - 1)
                        {
                            if (factRectangle.ElementAt(k).Height > 0 && factRectangle.ElementAt(k + 1).Height > 0)
                            {
                                KeyValuePair<Bubble, System.Drawing.Point[]> itm = allContourMultiLine.ElementAt(k + 1);
                                if (itm.Key.point.Y == item.Key.point.Y && itm.Key.subLine == item.Key.subLine)
                                {
                                    if (String.IsNullOrEmpty(areas[item.Key.areaNumber].bubblesOrientation)
                                        || areas[item.Key.areaNumber].bubblesOrientation == "horizontal")
                                    {
                                        if (Math.Abs(factRectangle.ElementAt(k).Y
                                            - factRectangle.ElementAt(k + 1).Y) > factRectangle.ElementAt(k).Height / 2)
                                        {
                                            barCodesPrompt = PromptCalibrationError(item.Key);
                                            if (i > 0)
                                                return;
                                            else
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        if (Math.Abs(factRectangle.ElementAt(k).X - factRectangle.ElementAt(k + 1).X)
                                            > factRectangle.ElementAt(k).Width / 2)
                                        {
                                            barCodesPrompt = PromptCalibrationError(item.Key);
                                            if (i > 0)
                                                return;
                                            else
                                                break;
                                        }
                                    }
                                }
                            }
                        }

                        if (item.Value.Length > 0)
                            if (item.Value.Length < minContourLength)
                                minContourLength = item.Value.Length;

                        #endregion
                    }
                    minContourLength = minContourLength + minContourLength / 3;

                    //Bitmap b = (Bitmap)bmpPres.Clone();
                    //foreach (Rectangle item in factRectangle)
                    //{
                    //    using (Graphics g = Graphics.FromImage(b))
                    //    {
                    //        g.DrawRectangle(new Pen(Color.Red), item);
                    //    }
                    //}
                    //b.Save("factRectangles.bmp", ImageFormat.Bmp);
                    //b.Dispose();

                    double blackBrightness;
                    if (filterType > 0 && filterType < .49)//.5
                        blackBrightness = .92f;
                    else if (filterType > 0 && filterType < .6)
                        blackBrightness = .88f;

                    else if (filterType > 0 && filterType < .7)
                        blackBrightness = .85f;
                    else if (filterType > 0 && filterType < .8)
                        blackBrightness = .83f;//.8f
                    else if (filterType > 0 && filterType <= 1)
                        blackBrightness = .8f;//.75
                    else if (filterType > 0 && filterType < 2 && filterType > 1.35)
                        blackBrightness = .72f;
                    else if (filterType > 0 && filterType < 2 && filterType > 1)
                        blackBrightness = .75f;
                    else if (filterType > 2)
                        blackBrightness = .55f;
                    else
                        blackBrightness = .6f;


                    //if (filterType > 0 && filterType < .5)
                    //    blackBrightness = .92f;
                    //else if (filterType > 0 && filterType < .6)
                    //    blackBrightness = .88f;
                    //else if (filterType > 0 && filterType < .7)
                    //    blackBrightness = .85f;
                    //else if (filterType > 0 && filterType < .8)
                    //    blackBrightness = .83f;//.8f
                    //else if (filterType > 0 && filterType <= .9)
                    //    blackBrightness = .8f;//.75
                    ////else if (filterType > 0 && filterType <= 1)
                    ////    blackBrightness = .75;//.8f;//
                    //else if (filterType > 0 && filterType < 2 && filterType > 1.35)
                    //    blackBrightness = .72f;
                    //else if (filterType > 0 && filterType < 2 && filterType > 1.1)
                    //    blackBrightness = .75f;
                    //else if (filterType > 2)
                    //    blackBrightness = .55f;
                    //else
                    //    blackBrightness = .6f;

                    //if (filterType > 4)
                    //    blackBrightness = .5;
                    //else if (filterType > 1.3)
                    //    blackBrightness = .6;
                    //else if (filterType > .8)
                    //    blackBrightness = .65;

                    if (smartResize)
                    {
                        bmpPres = (Bitmap)bmp.Clone();
                        bmpPres = ResizeBitmap(bmpPres, factor);

                        //bmpPres.Save("ResizeBitmap2.bmp", ImageFormat.Bmp);

                        //if (filterType > 0 && filterType < .95)//1//.88 .65)//<= .75
                        //    binaryzeMap(bmpPres, bmpPres.Width, bmpPres.Height, 3);

                        //lockBitmap = new LockBitmap(bmpPres);
                        //lockBitmap.LockBits();

                        //if (i == 0)
                        //    if (filterType > 0 && filterType < .5)
                        //        bmpPres = ConvertTo1Bit(ref bmpPres, .92f);
                        //    else if (filterType > 0 && filterType < .6)
                        //        bmpPres = ConvertTo1Bit(ref bmpPres, .88f);
                        //    else if (filterType > 0 && filterType < .7)
                        //        bmpPres = ConvertTo1Bit(ref bmpPres, .85f);
                        //    else if (filterType > 0 && filterType < .8)
                        //        bmpPres = ConvertTo1Bit(ref bmpPres, .8f);
                        //    else if (filterType > 0 && filterType < 1)
                        //        bmpPres = ConvertTo1Bit(ref bmpPres, .75f);
                        //    else if (filterType > 0 && filterType < 2 && filterType > 1.3)
                        //        bmpPres = ConvertTo1Bit(ref bmpPres, .72f);
                        //    else if (filterType > 2)
                        //        bmpPres = ConvertTo1Bit(ref bmpPres, .6f);
                        //    else
                        //        bmpPres = ConvertTo1Bit(ref bmpPres, .7f);

                        //bmpPres = ResizeBitmap(bmpPres, factor);

                        //bmpPres.Save("ResizeBitmap3.bmp", ImageFormat.Bmp);
                    }
                    //maxBubblesDist = 10;//25// 49;//
                    goodBubbleNumber = GetGoodBubbleNumber(allContourMultiLine, factRectangle, minContourLength, goodBubbleNumber);
                    prevCurve = new System.Drawing.Point[0];
                    bubblesRegion = 0;
                    bubblesPerWidth = bubblesPerLine[bubblesRegion];
                    bubblesPerHeight = linesPerArea[bubblesRegion];
                    int currentLine = indexOfFirstQuestion;

                    Dictionary<Bubble, double> currentLineBubbles = new Dictionary<Bubble, double>();
                    double bestPercent = darknessDifferenceLevel / 100;
                    bubble = new Bubble();
                    int prevGoodLine = 0, prevGoodLineY = 0;
                    areaNumber = -1;
                    double factStepY = 0;
                    goodBubble = 0;
                    baddBubble = 0;

                    for (int k = 0; k < allContourMultiLine.Count; k++)
                    {
                        #region калибровка, замена, распознавание пузырей
                        //if (token.IsCancellationRequested)
                        //    return;

                        KeyValuePair<Bubble, System.Drawing.Point[]> item = allContourMultiLine.ElementAt(k);
                        posX = item.Key.point.X;
                        posY = item.Key.point.Y;
                        bubblesRegion = item.Key.areaNumber;

                        if (areaNumber != bubblesRegion)
                        {
                            areaNumber = bubblesRegion;
                            //factStepY = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Height
                            // bubblesPerLine[bubblesRegion]));
                            bubblesPerWidth = bubblesPerLine[areaNumber];
                            bubblesPerHeight = linesPerArea[areaNumber];
                            if (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
                              || areas[areaNumber].bubblesOrientation == "horizontal")
                                bubbleStepY = (int)Math.Round((decimal)(lineHeight[bubblesRegion] - bubble1.Height));
                            else
                                bubbleStepY = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Height
                                    - bubble1.Height * bubblesPerWidth) / (bubblesPerWidth - 1));
                            factStepY = bubble1.Height + bubbleStepY;
                            double factStepYCalc = factStepY;
                            prevGoodLineY = 0; prevGoodLine = 0;
                            GetLineFactStep
                             (
                               ref factStepY
                             , ref prevGoodLine
                             , ref prevGoodLineY
                             , k
                             , areas[bubblesRegion]
                             , factRectangle
                             , allContourMultiLine
                             , bubblesRegion
                             , minContourLength
                             );
                            if ((string.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
                                || areas[bubblesRegion].bubblesOrientation == "horizontal")
                                && !calcPercentOnly && Math.Abs(factStepYCalc - factStepY) > factStepYCalc / 4)
                            {
                                barCodesPrompt = "Calibration error 2";
                                return;
                            }
                            bubblesPerWidth = bubblesPerLine[bubblesRegion];
                            bubble1 = bubblesOfRegion[bubblesRegion];
                            if (string.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
                                || areas[bubblesRegion].bubblesOrientation == "horizontal")
                                bubbleStepX = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Width
                                    - bubble1.Width * bubblesPerWidth) / (bubblesPerWidth - 1));
                            else
                            {
                                if (areas[bubblesRegion].subLinesAmount == 0)
                                {
                                    bubblesSubLinesStep[bubblesRegion] = (int)Math.Round((decimal)(areas[bubblesRegion].bubble.Width * 2) * kx);
                                }
                                bubbleStepX = bubblesSubLinesStep[bubblesRegion];
                                bubbleStepY = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Height
                                    / bubblesPerLine[bubblesRegion] - bubble1.Height));
                            }
                        }
                        //bool openCircuit = InsideContour(item);
                        if (item.Value.Length == 0 || item.Value.Length > minContourLength || !InsideContour(item))
                        {
                            #region плохой пузырь // almost like in Recognize -> BadBubble()
                            int dist, minDist = int.MaxValue, numCont = -1;
                            int n;
                            baddBubble++;
                            //if (!openCircuit)
                            //    if (factRectangle[k].Size!=new Size())
                            //        openContour = true;
                            KeyValuePair<Bubble, System.Drawing.Point[]> itm = new KeyValuePair<Bubble, Point[]>();
                            if (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
                             || areas[bubblesRegion].bubblesOrientation == "horizontal")
                            {
                                //KeyValuePair<Bubble, System.Drawing.Point[]> 
                                itm = allContourMultiLine.ElementAt(k);
                                for (int kn = 1; kn < maxBubblesDist; kn++)
                                {
                                    n = k + kn;
                                    if (n <= allContourMultiLine.Count - 1)
                                    {
                                        itm = allContourMultiLine.ElementAt(n);
                                        //if ((regionRectangle.ElementAt(n).Value == regionRectangle.ElementAt(k).Value)
                                        if ((itm.Key.areaNumber == item.Key.areaNumber)
                                            && itm.Value.Length != 0
                                            && itm.Value.Length <= minContourLength)
                                        {
                                            dist = Math.Abs(item.Key.point.X - itm.Key.point.X)
                                                 + Math.Abs(item.Key.point.Y - itm.Key.point.Y)
                                                 + Math.Abs(item.Key.subLine - itm.Key.subLine);
                                            if (dist < minDist)
                                            {
                                                if (!InsideContour(itm))
                                                    continue;
                                            }
                                            if (dist <= 1)
                                            {
                                                numCont = n;
                                                break;
                                            }
                                            else
                                            {
                                                if (dist < minDist)
                                                {
                                                    minDist = dist;
                                                    numCont = n;
                                                }
                                            }
                                        }
                                    }
                                    n = k - kn;
                                    if (n > -1)
                                    {
                                        itm = allContourMultiLine.ElementAt(n);
                                        if ((itm.Key.areaNumber == item.Key.areaNumber)
                                            && itm.Value.Length != 0 && itm.Value.Length <= minContourLength)
                                        {
                                            dist = Math.Abs(item.Key.point.X - itm.Key.point.X)
                                                 + Math.Abs(item.Key.point.Y - itm.Key.point.Y)
                                                 + Math.Abs(item.Key.subLine - itm.Key.subLine);
                                            if (dist < minDist)
                                            {
                                                if (!InsideContour(itm))
                                                    continue;
                                            }
                                            if (dist <= 1)
                                            {
                                                numCont = n;
                                                break;
                                            }
                                            else
                                            {
                                                if (dist < minDist)
                                                {
                                                    minDist = dist;
                                                    numCont = n;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            //numCont = -1;
                            if (numCont > -1 && (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
                               || areas[bubblesRegion].bubblesOrientation == "horizontal"))
                            {//itm = замещающий, item = замещаемый
                                int distX;
                                int distY;
                                int distYsub;
                                int moveX;
                                int moveY;
                                itm = allContourMultiLine.ElementAt(numCont);
                                if (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
                                      || areas[bubblesRegion].bubblesOrientation == "horizontal")
                                {
                                    distX = item.Key.point.X - itm.Key.point.X;
                                    distY = item.Key.point.Y - itm.Key.point.Y;
                                    distYsub = item.Key.subLine - itm.Key.subLine;
                                    moveX = (bubble1.Width + bubbleStepX) * distX;
                                    moveY = (int)Math.Round((double)factStepY * distY
                                           + bubblesSubLinesStep[bubblesRegion] * distYsub);//* signY Math.Sign(itm.Key.point.Y));
                                }
                                else
                                {
                                    distY = item.Key.point.X - itm.Key.point.X;
                                    distX = item.Key.point.Y - itm.Key.point.Y;
                                    distYsub = item.Key.subLine - itm.Key.subLine;
                                    moveY = (int)Math.Round((double)factStepY * distY);
                                    moveX = (int)Math.Round((double)bubbleStepX * distX
                                           + (bubblesSubLinesStep[bubblesRegion]) * distYsub);
                                }
                                prevCurve = new System.Drawing.Point[allContourMultiLine.ElementAt(numCont).Value.Length];
                                allContourMultiLine.ElementAt(numCont).Value.CopyTo(prevCurve, 0);
                                prevCurve = MoveContour(prevCurve, moveX, moveY);
                                prevRectangle = new Rectangle
                                    (
                                      factRectangle[numCont].X
                                    , factRectangle[numCont].Y
                                    , factRectangle[numCont].Width
                                    , factRectangle[numCont].Height
                                    );
                                prevRectangle.X += moveX;
                                prevRectangle.Y += moveY;
                                factRectangle[k] = new Rectangle
                                    (
                                      prevRectangle.X
                                    , prevRectangle.Y
                                    , prevRectangle.Width
                                    , prevRectangle.Height
                                    );
                            }
                            else
                            if (goodBubbleNumber > -1 && factRectangle[goodBubbleNumber].Size != new System.Drawing.Size())
                            {
                                if (axisX.Length == 1)
                                {
                                    GetAxis
                                        (
                                          ref axisX
                                        , ref axisY
                                        , ref factRectangle
                                        , allContourMultiLine
                                        , bubble1
                                        , ref axisYSubline
                                        //, smartResize
                                        //, factor
                                        );

                                    //Bitmap b2 = (Bitmap)bmpPres.Clone();
                                    //foreach (var line in axisY)
                                    //{
                                    //    using (Graphics g = Graphics.FromImage(b2))
                                    //    {
                                    //        g.DrawLine(new Pen(Color.Red), new Point(0, line / factor)
                                    //            , new Point(b2.Width, line / factor));
                                    //    }
                                    //}
                                    //foreach (var line in axisX)
                                    //{
                                    //    using (Graphics g = Graphics.FromImage(b2))
                                    //    {
                                    //        g.DrawLine(new Pen(Color.Blue), new Point(line / factor, 0)
                                    //            , new Point(line / factor, b2.Height));
                                    //    }
                                    //}
                                    //b2.Save("allAxisY.bmp", ImageFormat.Bmp);
                                    //b2.Dispose();

                                    if ((amoutOfQuestions <= bubblesPerHeight && axisY.Length < amoutOfQuestions)
                                        || (amoutOfQuestions >= bubblesPerHeight && axisY.Length < bubblesPerHeight))
                                    {
                                        barCodesPrompt = "Calibration error an axis \"Y\"";
                                        if (Array.IndexOf(axisYSubline, 1) < 0)
                                        {
                                            List<int> listAxisY = axisY.ToList();
                                            List<int> listAxisYSubline = axisYSubline.ToList();
                                            double lfsd = 0;
                                            for (int index = 0; index < listAxisY.Count - 1; index++)
                                            {
                                                int yDist = listAxisY[index + 1] - listAxisY[index];
                                                int lines = (int)Math.Round(((double)yDist / (lineFactStepY))); //* ky
                                                if (lines > 1)
                                                {
                                                    int lfs = yDist / lines;
                                                    lfsd = (double)yDist / lines;
                                                    lineFactStepY = lfs;
                                                    for (int j = 0; j < lines - 1; j++)
                                                    {
                                                        listAxisY.Insert(index + 1 + j
                                                            , listAxisY[index] + (int)Math.Round((double)lfsd * (j + 1)));
                                                        listAxisYSubline.Insert(index + 1 + j, 0);
                                                    }
                                                    index += (lines - 1);
                                                }
                                            }
                                            if ((amoutOfQuestions <= bubblesPerHeight && listAxisY.Count < amoutOfQuestions)
                                               || (amoutOfQuestions >= bubblesPerHeight && axisY.Length < bubblesPerHeight))
                                            {
                                                if (amoutOfQuestions >= bubblesPerHeight && listAxisY.Count < bubblesPerHeight)
                                                {
                                                    for (int j = listAxisY.Count; j < bubblesPerHeight; j++)
                                                    {
                                                        listAxisY.Add(listAxisY[listAxisY.Count - 1] + lineFactStepY);
                                                        listAxisYSubline.Add(0);
                                                    }
                                                }
                                                else if (amoutOfQuestions <= bubblesPerHeight && listAxisY.Count < amoutOfQuestions)
                                                {
                                                    int lastItem = listAxisY.Count - 1;
                                                    int mult = 0;
                                                    if (lfsd == 0)
                                                        lfsd = lineFactStepY;
                                                    for (int j = listAxisY.Count; j < amoutOfQuestions; j++)
                                                    {
                                                        mult++;
                                                        listAxisY.Add(listAxisY[lastItem] + (int)Math.Round((double)lfsd * mult));
                                                        listAxisYSubline.Add(0);
                                                    }
                                                }
                                            }
                                            axisY = listAxisY.ToArray();
                                            axisYSubline = listAxisYSubline.ToArray();

                                            //Bitmap b4 = (Bitmap)bmpPres.Clone();
                                            //foreach (var line in listAxisY)
                                            //{
                                            //    using (Graphics g = Graphics.FromImage(b4))
                                            //    {
                                            //        g.DrawLine(new Pen(Color.Red), new Point(0, line / factor)
                                            //            , new Point(b4.Width, line / factor));
                                            //    }
                                            //}
                                            //foreach (var line in axisX)
                                            //{
                                            //    using (Graphics g = Graphics.FromImage(b4))
                                            //    {
                                            //        g.DrawLine(new Pen(Color.Blue), new Point(line / factor, 0)
                                            //            , new Point(line / factor, b4.Height));
                                            //    }
                                            //}
                                            //b4.Save("allAxisXY.bmp", ImageFormat.Bmp);
                                            //b4.Dispose();

                                            int yb = 0;//areas[bubblesRegion].bubblesOrientation == "horizontal"
                                            if (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
                                            || areas[bubblesRegion].bubblesOrientation == "horizontal")
                                            {
                                                yb = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y + bubble1.Height / 2));
                                            }
                                            else
                                            {
                                                yb = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y - bubble1.Height / 8));
                                            }

                                            yb += deltaY;
                                            if ((amoutOfQuestions <= bubblesPerHeight && axisY.Length < amoutOfQuestions)
                                                || (amoutOfQuestions >= bubblesPerHeight && axisY.Length < bubblesPerHeight)
                                                || Math.Abs(yb - axisY[0]) > lineFactStepY)
                                            {
                                                if (i == iter - 1)
                                                    return;
                                                else
                                                    break;
                                            }

                                            barCodesPrompt = "";
                                        }
                                    }
                                }
                                prevCurve = new System.Drawing.Point[allContourMultiLine.ElementAt(goodBubbleNumber).Value.Length];
                                allContourMultiLine.ElementAt(goodBubbleNumber).Value.CopyTo(prevCurve, 0);
                                int moveX = factRectangle[k].X - factRectangle[goodBubbleNumber].X;
                                int moveY = factRectangle[k].Y - factRectangle[goodBubbleNumber].Y;

                                prevRectangle = new Rectangle
                                (
                                  factRectangle[goodBubbleNumber].X
                                , factRectangle[goodBubbleNumber].Y
                                , factRectangle[goodBubbleNumber].Width
                                , factRectangle[goodBubbleNumber].Height
                                );


                                prevRectangle.X += moveX;
                                int bestWal = int.MaxValue;
                                int delta = int.MaxValue;
                                foreach (int item2 in axisX)
                                {
                                    int delta2 = Math.Abs(prevRectangle.X - item2);
                                    if (delta2 < delta)
                                    {
                                        delta = delta2;
                                        bestWal = item2;
                                    }
                                }
                                delta = bestWal - prevRectangle.X;
                                if (Math.Abs(delta) > bubble1.Width)
                                    delta = 0;
                                else
                                    prevRectangle.X = bestWal;

                                moveX += delta;

                                prevRectangle.Y += moveY;
                                bestWal = int.MaxValue;
                                delta = int.MaxValue;
                                int bestWalIndex = -1;
                                for (int j = 0; j < axisY.Length; j++)
                                {
                                    int item2 = axisY[j];
                                    int delta2 = Math.Abs(prevRectangle.Y - item2);
                                    if (delta2 < delta)// bestY)
                                    {
                                        delta = delta2;
                                        bestWal = item2;
                                        bestWalIndex = j;
                                    }
                                }
                                delta = bestWal - prevRectangle.Y;
                                if (Math.Abs(delta) > bubble1.Height)
                                    delta = 0;
                                else
                                    prevRectangle.Y = bestWal;

                                moveY += delta;
                                prevRectangle.Y = bestWal;
                                //if (bestWalIndex == -1)
                                //{
                                //    bestWalIndex = -1;
                                //}
                                if (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
                                    || areas[bubblesRegion].bubblesOrientation == "horizontal"
                                    && itm.Key.subLine != axisYSubline[bestWalIndex])
                                {//только для "horizontal"!!!
                                    int subLineStep = itm.Key.subLine - axisYSubline[bestWalIndex];
                                    moveY += (subLineStep * bubblesSubLinesStep[bubblesRegion]);
                                    prevRectangle.Y += (subLineStep * bubblesSubLinesStep[bubblesRegion]);
                                }
                                prevCurve = MoveContour(prevCurve, moveX, moveY);

                                factRectangle[k] = new Rectangle
                                    (
                                      prevRectangle.X
                                    , prevRectangle.Y
                                    , prevRectangle.Width
                                    , prevRectangle.Height
                                    );
                            }
                            else
                            {//err
                                barCodesPrompt = PromptCalibrationError(item.Key);
                                if (i > 0)
                                    return;
                                else
                                    break;
                            }
                            #endregion
                        }
                        else
                        {
                            #region хороший пузырь // аналогичен Recognize -> GoodBubble()
                            if (bubble.Equals(new Bubble()) && item.Key.subLine == 0)
                            {
                                bubble = item.Key;
                                prevGoodLineY = factRectangle[k].Y;
                            }
                            else
                            {
                                if (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
                                     || areas[bubblesRegion].bubblesOrientation == "horizontal")
                                {
                                    if (item.Key.subLine == 0)
                                    {//определение текущего значения lineFactStep
                                        if (bubble.areaNumber == item.Key.areaNumber)
                                        {
                                            if (bubble.point.Y != item.Key.point.Y)
                                            {
                                                factStepY = (double)(factRectangle[k].Y - prevGoodLineY)
                                                    / (item.Key.point.Y - bubble.point.Y);
                                                //bubble = item.Key;//???
                                                prevGoodLineY = factRectangle[k].Y;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (bubble.areaNumber == item.Key.areaNumber)
                                    {
                                        if (bubble.subLine == item.Key.subLine)
                                        {
                                            if ((item.Key.point.X != bubble.point.X))
                                            {
                                                factStepY = (double)(factRectangle[k].Y - prevGoodLineY)
                                                    / (item.Key.point.X - bubble.point.X);
                                                //bubble = item.Key;//???
                                                prevGoodLineY = factRectangle[k].Y; //item.Key.point.X;
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                            goodBubble++;
                            bubble = item.Key;
                            goodBubbleNumber = k;
                            //prevRectangle = factRectangle[k];
                            //prevCurve = item.Value;
                            prevRectangle = new Rectangle
                                (factRectangle[k].X
                                , factRectangle[k].Y
                                , factRectangle[k].Width
                                , factRectangle[k].Height
                                );
                            prevCurve = new System.Drawing.Point[item.Value.Length];
                            item.Value.CopyTo(prevCurve, 0);
                        }
                        if (smartResize)
                        {
                            System.Drawing.Point[] pts = prevCurve;
                            prevCurve = new System.Drawing.Point[pts.Length];
                            pts.CopyTo(prevCurve, 0);
                            Rectangle r = GetRectangle(ref prevCurve, factor);
                            prevRectangle = new Rectangle(r.X, r.Y, r.Width, r.Height);

                            //Bitmap b4 = (Bitmap)bmpPres.Clone();
                            //using (Graphics g = Graphics.FromImage(b4))
                            //{
                            //    g.DrawPolygon(new Pen(Color.Cyan), prevCurve);
                            //    g.DrawRectangle(new Pen(Color.Red), prevRectangle);//factRectangle[k] r
                            //    //g.DrawRectangle(new Pen(Color.Blue), factRectangle[k]);
                            //}
                            //b4.Save("allBubbles.bmp", ImageFormat.Bmp);
                            //b4.Dispose();

                        }
                        if (k > 0 && posX > 0 && posX - allContourMultiLine.ElementAt(k - 1).Key.point.X == 1)//item.Key.point.X>0
                        {
                            if (diffBubble < bubble1.Width * 2 && factRectangle[k].X - factRectangle[k - 1].Right > factRectangle[k].Width)
                            {
                                barCodesPrompt = PromptCalibrationError(bubble);
                                if (i > 1)
                                    return;
                                else
                                    break;
                            }
                        }
                        if (prevCurve.Length == 0)
                        {
                            barCodesPrompt = PromptCalibrationError(bubble);
                            if (i > 1)
                                return;
                            else
                                break;
                        }
                        double black = 0, all = 0;
                        int white = 0;
                        if (darknessPercent > 0)
                        {
                            #region darknessPercent > 0
                            using (GraphicsPath gp = new GraphicsPath())
                            {
                                gp.AddPolygon(prevCurve);
                                bool cont = false;
                                for (yn = prevRectangle.Y + 1; yn < prevRectangle.Bottom; yn++)
                                {
                                    for (xn = prevRectangle.X + 1; xn < prevRectangle.Right; xn++)
                                    {
                                        all++;
                                        if (gp.IsVisible(xn, yn))
                                        {
                                            cont = true;
                                            if (xn <= 0 || xn >= bmpPres.Width || yn <= 0 || yn >= bmpPres.Height)
                                            {
                                                barCodesPrompt = "Calibration error";
                                                return;
                                            }
                                            color = bmpPres.GetPixel(xn, yn);

                                            //if (color.R != color.G || color.R != color.B)
                                            //{
                                            //    lockBitmap.UnlockBits();
                                            //    bmpPres = (Bitmap)bmpPres.Clone();
                                            //    BubblesRecognizeOld
                                            //    (
                                            //      out  allContourMultiLine
                                            //    , ref factRectangle
                                            //    , bmpPres
                                            //    , ref  barCodesPrompt
                                            //    , filterType
                                            //    , smartResize
                                            //    , bubblesRegions
                                            //    , bubblesOfRegion
                                            //    , bubblesSubLinesCount
                                            //    , bubblesSubLinesStep
                                            //    , bubblesPerLine
                                            //    , lineHeight, linesPerArea
                                            //    , answersPosition
                                            //    , indexAnswersPosition
                                            //    , totalOutput
                                            //    , bubbleLines
                                            //    , regions
                                            //    , areas
                                            //    , x1, x2, y1, y2, kx, ky
                                            //    , curRect, etRect
                                            //    , deltaY
                                            //    , amoutOfQuestions
                                            //    , indexOfFirstQuestion
                                            //    , maxCountRectangles
                                            //    , darknessPercent
                                            //    , darknessDifferenceLevel
                                            //    , indexOfFirstBubble = 0
                                            //    );
                                            //    return;
                                            //}
                                            double f = color.GetBrightness();
                                            //lockBitmap.SetPixel(xn, yn, Color.Red);
                                            //if (color != argbWhite)
                                            if (f <= blackBrightness)//brightness.9
                                                black++;
                                            //} 
                                            //catch { }
                                        }
                                        else
                                        {
                                            if (cont)
                                            {
                                                cont = false;
                                                break;
                                            }
                                        }
                                        continue;
                                    }
                                }
                                double perCent = (black / all) * 100;
                                if (perCent >= darknessPercent)
                                    currentLineBubbles.Add(item.Key, perCent);
                            }
                            #endregion
                        }
                        else
                        {
                            #region else
                            yn = prevRectangle.Y + prevRectangle.Height / 2;
                            y1 = yn;
                            x1 = prevRectangle.X;
                            xn = x1;
                            x2 = prevRectangle.Width / 8;
                            y2 = prevRectangle.Height / 8;
                            for (yn = yn - y2; yn <= y1 + y2; yn++)//несколько линий по "Y"
                            {
                                for (xn = x1 + x2; xn < prevRectangle.Right - x2; xn++)
                                {
                                    if (xn == x1 + 3 * x2)
                                    {
                                        xn += 3 * x2;
                                    }
                                    //color = lockBitmap.GetPixel(xn, yn);
                                    if (xn <= 0 || xn >= bmpPres.Width || yn <= 0 || yn >= bmpPres.Height)
                                    {
                                        barCodesPrompt = "Calibration error 8";
                                        return;
                                    }

                                    color = bmpPres.GetPixel(xn, yn);
                                    double f = color.GetBrightness();
                                    //if (color != argbWhite)
                                    if (f < .5)
                                    {
                                        black++;
                                    }
                                    else
                                    {
                                        white++;
                                    }
                                }
                            }
                            if (filterType > 3)
                            {
                                if (white < black * 2)
                                {
                                    currentLineBubbles.Add(item.Key, 0);
                                    if (maxCountRectangles != null)
                                    {
                                        if (!maxCountRectangles.Keys.Contains(item.Key))
                                        {
                                            if (i > 0)
                                                return;
                                            else
                                                break;
                                        }
                                        maxCountRectangles[item.Key].isChecked = true;
                                    }
                                }
                            }
                            else
                            {
                                if (filterType > 0 && filterType < .6)
                                {
                                    if (white < black * 2)
                                    {
                                        currentLineBubbles.Add(item.Key, 0);
                                        if (maxCountRectangles != null)
                                        {
                                            if (!maxCountRectangles.Keys.Contains(item.Key))
                                            {
                                                if (i > 0)
                                                    return;
                                                else
                                                    break;
                                            }
                                            maxCountRectangles[item.Key].isChecked = true;
                                        }
                                    }
                                }
                                else
                                {
                                    if (white < black)
                                    {
                                        currentLineBubbles.Add(item.Key, 0);
                                        if (maxCountRectangles != null)
                                        {
                                            if (!maxCountRectangles.Keys.Contains(item.Key))
                                            {
                                                if (i > 0)
                                                    return;
                                                else
                                                    break;
                                            }
                                            maxCountRectangles[item.Key].isChecked = true;
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                        if (k >= allContourMultiLine.Count - 1
                            || item.Key.point.Y != allContourMultiLine.ElementAt(k + 1).Key.point.Y)
                        {
                            #region результаты по строкам
                            bool lineChecked = false;
                            if (currentLineBubbles.Count == 0)
                            {
                                lineChecked = true;
                            }
                            else
                            {
                                if (darknessPercent > 0)
                                {
                                    if (currentLineBubbles.Count > 1)
                                    {
                                        double maxPerCent = -1;
                                        //System.Drawing.Point maxPerCentKey = new System.Drawing.Point(-1, 0);
                                        for (int j = 0; j < currentLineBubbles.Count; j++)
                                        {
                                            KeyValuePair<Bubble, double> item1 = currentLineBubbles.ElementAt(j);
                                            if (maxPerCent < item1.Value)
                                            {
                                                //maxPerCentKey = item1.Key.point;
                                                maxPerCent = item1.Value;
                                            }
                                        }
                                        //maxPerCent = 0;
                                        //for (int j = 0; j < currentLineBubbles.Count; j++)
                                        //{
                                        //    KeyValuePair<Bubble, double> item1 = currentLineBubbles.ElementAt(j);
                                        //    if (item1.Key.point == maxPerCentKey)
                                        //    {
                                        //        continue;
                                        //    }
                                        //    maxPerCent += item1.Value;
                                        //}
                                        //maxPerCent /= (currentLineBubbles.Count - 1);//Count - 1 <=правильно
                                        for (int j = 0; j < currentLineBubbles.Count; j++)
                                        {
                                            KeyValuePair<Bubble, double> item1 = currentLineBubbles.ElementAt(j);
                                            //if (item1.Value > maxPerCent * bestPercent)
                                            if (item1.Value * bestPercent > maxPerCent)
                                            {
                                                if (maxCountRectangles != null)
                                                {
                                                    if (!maxCountRectangles.Keys.Contains(currentLineBubbles.ElementAt(j).Key))
                                                    {
                                                        if (i > 0)
                                                            return;
                                                        else
                                                            break;
                                                    }
                                                    maxCountRectangles[item1.Key].isChecked = true;
                                                    lineChecked = true;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (maxCountRectangles != null)
                                        {
                                            if (!maxCountRectangles.Keys.Contains(currentLineBubbles.ElementAt(0).Key))
                                            {
                                                if (i > 0)
                                                    return;
                                                else
                                                    break;
                                            }
                                            maxCountRectangles[currentLineBubbles.ElementAt(0).Key].isChecked = true;
                                            lineChecked = true;
                                        }
                                    }
                                }
                                if (!lineChecked)
                                {
                                    for (int j = 0; j < currentLineBubbles.Count; j++)
                                    {
                                        if (maxCountRectangles != null)
                                        {
                                            if (!maxCountRectangles.Keys.Contains(currentLineBubbles.ElementAt(j).Key))
                                            {
                                                if (i > 0)
                                                    return;
                                                else
                                                    break;
                                            }
                                            maxCountRectangles[currentLineBubbles.ElementAt(j).Key].isChecked = true;
                                        }
                                    }
                                }
                            }
                            currentLineBubbles.Clear();
                            currentLine++;
                            #endregion
                        }
                        //Bitmap b = (Bitmap)bmpPres.Clone();
                        //using (Graphics g = Graphics.FromImage(b))
                        //{
                        //    g.DrawPolygon(new Pen(Color.Cyan), prevCurve);
                        //    g.DrawRectangle(new Pen(Color.Red), prevRectangle);//factRectangle[k]
                        //}
                        //b.Save("strips2.bmp", ImageFormat.Bmp);
                        //b.Dispose();

                        #endregion
                    }
                    CalibrationError:

                    //Bitmap b3 = (Bitmap)bmp.Clone();// bmpPres.Clone();
                    //foreach (Rectangle item in factRectangle)
                    //{
                    //    using (Graphics g = Graphics.FromImage(b3))
                    //    {
                    //        g.DrawRectangle(new Pen(Color.Red), item);
                    //    }
                    //}
                    //b3.Save("factRectangles2.bmp", ImageFormat.Bmp);
                    //b3.Dispose();

                    for (int k = 0; k < allContourMultiLine.Count - 1; k++)
                    {
                        KeyValuePair<Bubble, System.Drawing.Point[]> item = allContourMultiLine.ElementAt(k);
                        KeyValuePair<Bubble, System.Drawing.Point[]> itemNext = allContourMultiLine.ElementAt(k + 1);

                        //Bitmap b = (Bitmap)bmp.Clone();
                        //using (Graphics g = Graphics.FromImage(b))
                        //{
                        //    //g.DrawRectangle(new Pen(Color.Red), MultiplyRectangle(factRectangle[k], factor));
                        //    g.DrawRectangle(new Pen(Color.Red),factRectangle[k]);
                        //}
                        //b.Save("Calibration error 5.bmp", ImageFormat.Bmp);
                        //b.Dispose();

                        if (maxCountRectangles.ContainsKey(item.Key))
                            maxCountRectangles[item.Key].rectangle = factRectangle[k];
                        else//можно не выдавать ошибку, но доработать rec.FindBubble
                            barCodesPrompt = "Calibration error 5";

                        if (k == allContourMultiLine.Count - 2)
                        {
                            if (maxCountRectangles.ContainsKey(itemNext.Key))
                                maxCountRectangles[itemNext.Key].rectangle = factRectangle[k + 1];
                            else
                                barCodesPrompt = "Calibration error 5";
                        }
                        if ((string.IsNullOrEmpty(areas[item.Key.areaNumber].bubblesOrientation)
                              || areas[item.Key.areaNumber].bubblesOrientation == "horizontal"))
                        {
                            if (item.Key.areaNumber == itemNext.Key.areaNumber
                                && item.Key.point.Y == itemNext.Key.point.Y
                                && item.Key.subLine == itemNext.Key.subLine)
                            {
                                if (Math.Abs(factRectangle[k].Y - factRectangle[k + 1].Y) > factRectangle[k].Height / 2)
                                {
                                    barCodesPrompt = "Aligment error an axis \"Y\"";
                                    break;
                                }
                            }
                        }
                    }
                    double percentGoodBubble = (double)goodBubble / (goodBubble + baddBubble);
                    if (barCodesPrompt == "" && percentGoodBubble < .1)//.05 .25!!! .15
                        barCodesPrompt = "Calibration error of bubbles";
                    if (!string.IsNullOrEmpty(barCodesPrompt) && filterType <= 2 && i < iter - 1)//<= 1.2
                    {
                        //lockBitmap.UnlockBits();
                        bmpPres = (Bitmap)bmp.Clone();
                        //bmpPres = ConvertTo1Bit(ref bmpPres);
                        maxBubblesDist = 4;
                        for (int j = 0; j < 2; j++)
                        {
                            using (Bitmap b2 = new Bitmap(bmpPres.Width / 2, bmpPres.Height / 2, PixelFormat.Format24bppRgb))
                            {
                                using (Graphics g = Graphics.FromImage(b2))
                                {
                                    g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                                    g.DrawImage(bmpPres, 0, 0, b2.Width, b2.Height);
                                }
                                using (Graphics g = Graphics.FromImage(bmpPres))
                                {
                                    g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                                    g.DrawImage(b2, 0, 0, bmpPres.Width, bmpPres.Height);
                                }
                            }
                        }

                        //bmpPres.Save("Calibration error.bmp", ImageFormat.Bmp);

                        if (i == 0)
                        {
                            if (filterType < 1)//.95
                                brightness = .95;// .88;
                            else
                                brightness = .9;
                            int ower = caliberHeight / 2;
                            //if (ower < 4)
                            //    caliberHeight += 4;
                            //else
                            caliberHeight += ower;
                            //ower = caliberWidth / 2;
                            //if (ower < 4)
                            //    caliberWidth += 4;
                            //else
                            //caliberWidth += ower;
                        }
                        else
                            brightness = .8;
                    }
                    else
                        break;
                }
                //TimeSpan ts = DateTime.Now - dt;
                //}
                //catch (Exception ex)
                //{
                //    throw ex;
            }
            finally
            {
                bmpPres.Dispose();
                //try
                //{
                //    lockBitmap.UnlockBits();
                //}
                //catch (Exception)
                //{
                //}
            }
        }
        //-------------------------------------------------------------------------
        private int GetGoodBubbleNumber(Dictionary<Bubble, Point[]> allContourMultiLine, Rectangle[] factRectangle, int minContourLength, int goodBubbleNumber)
        {
            for (int k = 0; k < allContourMultiLine.Count - 1; k++)
            {
                KeyValuePair<Bubble, System.Drawing.Point[]> item = allContourMultiLine.ElementAt(k);
                if (factRectangle[k].Size != new System.Drawing.Size(0, 0)
                    && item.Value.Length <= minContourLength
                    && InsideContour(item))
                {
                    goodBubbleNumber = k;
                    break;
                }
            }
            return goodBubbleNumber;
        }
        //-------------------------------------------------------------------------
        //public void BubblesRecognizeOld
        //    (
        //      out Dictionary<Bubble, Point[]> allContourMultiLine
        //    , ref Rectangle[] factRectangle
        //    , Bitmap bmp
        //    , ref string barCodesPrompt
        //    , double filterType
        //    , bool smartResize
        //    , Rectangle[] bubblesRegions, Rectangle[] bubblesOfRegion
        //    , int[] bubblesSubLinesCount
        //    , int[] bubblesSubLinesStep
        //    , int[] bubblesPerLine
        //    , int[] lineHeight, int[] linesPerArea
        //    , int answersPosition
        //    , int indexAnswersPosition
        //    , object[] totalOutput
        //    , string[] bubbleLines
        //    , Regions regions
        //    , RegionsArea[] areas
        //    , int x1, int x2, int y1, int y2, decimal kx, decimal ky
        //    , Rectangle curRect, Rectangle etRect
        //    , int deltaY
        //    , int amoutOfQuestions
        //    , int indexOfFirstQuestion
        //    , Dictionary<Bubble, CheckedBubble> maxCountRectangles
        //    , double darknessPercent
        //    , double darknessDifferenceLevel
        //    , int lastBannerBottom
        //    , int indexOfFirstBubble = 0
        //    )
        //{
        //    allContourMultiLine = new Dictionary<Bubble, System.Drawing.Point[]>();
        //    using (Bitmap bmpPres = (Bitmap)bmp.Clone())
        //    {
        //        for (int i = 0; i < 2; i++)
        //        {
        //            int bubblesPerWidth = 0, bubblesPerHeight = 0
        //               , bubbleStepX = 0, bubbleStepY = 0, xn, yn;
        //            Color color;
        //            Color argbWhite = Color.FromArgb(255, 255, 255);
        //            Rectangle prevRectangle= Rectangle.Empty;
        //            System.Drawing.Point[] prevCurve = new System.Drawing.Point[0];
        //            int maxAmoutOfQuestions = linesPerArea.Sum();
        //            if (amoutOfQuestions == 0 || amoutOfQuestions > maxAmoutOfQuestions)
        //            {
        //                amoutOfQuestions = maxAmoutOfQuestions;
        //            }
        //            int bubblesRegion = 0;
        //            Rectangle bubble1 = bubblesOfRegion[0];
        //            int diffBubble;
        //            bubblesPerWidth = bubblesPerLine[0];
        //            bubblesPerHeight = linesPerArea[0];
        //            if (String.IsNullOrEmpty(areas[0].bubblesOrientation)
        //                || areas[0].bubblesOrientation == "horizontal")
        //            {
        //                xn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].X - bubble1.Width / 8));
        //                yn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y + bubble1.Height / 2));
        //                bubbleStepX = (int)Math.Round((decimal)(bubblesRegions[0].Width
        //                    - bubble1.Width * bubblesPerWidth) / (bubblesPerWidth - 1));
        //                bubbleStepY = (int)Math.Round((decimal)(lineHeight[0] - bubble1.Height));
        //                diffBubble = bubble1.Width + bubbleStepX;
        //            }
        //            else
        //            {
        //                xn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].X + bubble1.Width / 2));
        //                yn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y - bubble1.Height / 8));
        //                bubbleStepY = (int)Math.Round((decimal)(bubblesRegions[0].Height
        //                    - bubble1.Height * bubblesPerWidth) / (bubblesPerWidth - 1));
        //                bubbleStepX = (int)Math.Round((decimal)(lineHeight[0] - bubble1.Height));
        //                diffBubble = bubble1.Height + bubbleStepY;
        //            }
        //            yn += deltaY;
        //            Rectangle r1= Rectangle.Empty;
        //            Point[] contour1 = new Point[] { };
        //            factRectangle = new Rectangle[0];
        //            bool endBubblesRegions = false;

        //            int posX = 0, posY = 0;
        //            int caliberWidth = bubble1.Width / 8;
        //            int caliberHeight = bubble1.Height / 8;
        //            int correct = 0;
        //            int bubblesSubLine = 0;
        //            allContourMultiLine = new Dictionary<Bubble, System.Drawing.Point[]>();
        //            Bubble bubble = new Bubble();
        //            if (indexOfFirstQuestion == 0)
        //            {
        //                indexOfFirstQuestion = (areas[0].questionIndex != 0) ? (int)areas[0].questionIndex : 1;
        //            }
        //            int areaNumber = -1;
        //            int contourLength = 10000;
        //            int regionLines = 0;
        //            Point firstBubblePerLineLocation = new Point(bubblesRegions[bubblesRegion].X, bubblesRegions[bubblesRegion].Y + deltaY);
        //            int prevGoodBubbleLine = -1;
        //            int prevGoodBubbleSubLine = -1;
        //            int prevGoodBubbleLineLocationY = prevGoodBubbleLine;
        //            int firstBubbleRegion = 0;
        //            for (int line = indexOfFirstQuestion; line < indexOfFirstQuestion + amoutOfQuestions; line++)
        //            {
        //                #region for
        //                if (allContourMultiLine.Count > 0)
        //                {
        //                    bubble = allContourMultiLine.Last().Key;
        //                    if (line - bubble.point.Y > 1)
        //                    {
        //                        endBubblesRegions = true;
        //                    }
        //                }
        //                if (endBubblesRegions)
        //                {
        //                    barCodesPrompt = "The error in determining the regions of bubbles";
        //                    if (i > 0)
        //                        return;
        //                    else
        //                        break;
        //                }
        //                posX = 0; posY = 0;
        //                Bubble prevBubble = new Bubble();
        //                Bubble firstBubblePerLine = new Bubble();
        //                firstBubblePerLine.subLine = bubblesSubLine;
        //                firstBubblePerLine.point = new System.Drawing.Point(0, line);
        //                firstBubblePerLine.areaNumber = bubblesRegion;
        //                Rectangle firstBubblePerLineRectangle = new Rectangle(bubble1.X, bubble1.Y, bubble1.Width, bubble1.Height);
        //                if (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
        //                          || areas[bubblesRegion].bubblesOrientation == "horizontal")
        //                {
        //                    #region horizontal
        //                    for (int k = xn; k < bubblesRegions[bubblesRegion].Right + bubble1.Width / 4; k++)
        //                    {
        //                        color = argbWhite;
        //                        try
        //                        {
        //                            color = bmp.GetPixel(k, yn);
        //                        }
        //                        catch (Exception)
        //                        {
        //                            break;
        //                        }
        //                        if (color != argbWhite)
        //                        {
        //                            #region color Not White
        //                            contour1 = ContourFind(bmp, k, yn, 0, false, true, false, contourLength);
        //                            System.Drawing.Point p;
        //                            GetOuterContour(bmp, ref contour1, ref r1, out p);
        //                            if (contour1 == null
        //                                || r1.Size.Equals(new System.Drawing.Size()))
        //                            {
        //                                continue;
        //                            }
        //                            if (p == new System.Drawing.Point(int.MaxValue, 0))
        //                            {
        //                                k += bubbleStepX;
        //                                continue;
        //                            }
        //                            //if (Math.Abs(yn - p.Y) > diffBubble / 2)
        //                            //{
        //                            //    continue;
        //                            //}

        //                            if (diffBubble == 0)
        //                            {
        //                                posX = 0;
        //                            }
        //                            else
        //                            {
        //                                posX = (int)Math.Round((decimal)(r1.X - bubblesRegions[bubblesRegion].X) / diffBubble);
        //                            }

        //                            //using (Graphics g = Graphics.FromImage(bmp))
        //                            //{
        //                            //    g.DrawPolygon(new Pen(Color.Red), contour1);
        //                            //    g.DrawRectangle(new Pen(Color.Green), r1);
        //                            //}
        //                            //bmp.Save("bubbles.bmp", ImageFormat.Bmp);

        //                            if (posX < 0 && contour1.Length < contourLength)
        //                            {
        //                                if (Math.Abs(r1.Width - bubble1.Width) <= caliberWidth
        //                                  && Math.Abs(r1.Height - bubble1.Height) <= caliberHeight
        //                                  && Array.IndexOf(factRectangle, r1) < 0)
        //                                {
        //                                    int tmp = (bubblesRegions[bubblesRegion].X - r1.X);
        //                                    if (tmp < caliberWidth / 4)
        //                                    {
        //                                        bubblesRegions[bubblesRegion].X = r1.X;
        //                                    }
        //                                }
        //                                posX = 0;
        //                            }
        //                            int count = (int)Math.Round((double)(r1.Right - k) / diffBubble);
        //                            //if (count == 0)
        //                            //{
        //                            //    continue;
        //                            //}
        //                            if (count > 1)
        //                            {//вставляем на всякий случай
        //                                posX += count;
        //                            }
        //                            if (posX >= bubblesPerWidth)
        //                            {
        //                                posX = bubblesPerWidth - 1;
        //                            }
        //                            if (contour1.Length >= contourLength)
        //                            {
        //                                posX = bubblesPerWidth - 1;
        //                            }
        //                            bubble = new Bubble();
        //                            bubble.areaNumber = bubblesRegion;
        //                            bubble.point = new System.Drawing.Point(posX, line);
        //                            bubble.subLine = bubblesSubLine;
        //                            bubble.areaNumber = bubblesRegion;
        //                            if (line - bubble.point.Y > 1)
        //                            {
        //                                endBubblesRegions = true;
        //                            }
        //                            //if (prevRectangle == r1)
        //                            //{
        //                            //    continue;
        //                            //}
        //                            //prevRectangle = r1;
        //                            if (Math.Abs(r1.Width - bubble1.Width) <= caliberWidth
        //                                  && Math.Abs(r1.Height - bubble1.Height) <= caliberHeight
        //                                  && Array.IndexOf(factRectangle, r1) < 0)
        //                            {
        //                                #region хороший пузырь
        //                                if (contourLength == 10000)
        //                                {
        //                                    contourLength = contour1.Length * 8;
        //                                }
        //                                try
        //                                {
        //                                    prevGoodBubbleLine = line;
        //                                    prevGoodBubbleLineLocationY = r1.Y;
        //                                    prevGoodBubbleSubLine = bubblesSubLine;
        //                                    if (posX == 0 && !allContourMultiLine.Keys.Contains(bubble))//!!!
        //                                    {
        //                                        firstBubblePerLineLocation = r1.Location;
        //                                        firstBubbleRegion = bubblesRegion;
        //                                        firstBubblePerLineRectangle = r1;
        //                                        firstBubblePerLine = new Bubble();
        //                                        firstBubblePerLine.point = new System.Drawing.Point(posX, line);
        //                                        firstBubblePerLine.subLine = bubblesSubLine;
        //                                    }
        //                                    if (posX > 0 && posX < bubblesPerWidth - 1)
        //                                    {
        //                                        bubble = new Bubble();
        //                                        bubble.areaNumber = bubblesRegion;
        //                                        bubble.point = new System.Drawing.Point(posX - 1, line);
        //                                        bubble.subLine = bubblesSubLine;
        //                                        if (!allContourMultiLine.Keys.Contains(bubble))
        //                                        {
        //                                            int np = posX - 1;
        //                                            for (int l = np - 1; l > -1; l--)
        //                                            {
        //                                                np = l;
        //                                                bubble.point = new System.Drawing.Point(np, line);
        //                                                if (allContourMultiLine.Keys.Contains(bubble))
        //                                                {
        //                                                    np++;
        //                                                    break;
        //                                                }
        //                                            }
        //                                            for (int l = np; l < posX; l++)
        //                                            {
        //                                                bubble.point = new System.Drawing.Point(l, line);
        //                                                allContourMultiLine.Add(bubble, new System.Drawing.Point[0]);
        //                                                Array.Resize(ref factRectangle, factRectangle.Length + 1);
        //                                                factRectangle[factRectangle.Length - 1]
        //                                                    = new Rectangle(r1.X - diffBubble * (posX - l), r1.Y, 0, 0);
        //                                            }
        //                                        }
        //                                    }
        //                                    bubble = new Bubble();
        //                                    bubble.areaNumber = bubblesRegion;
        //                                    bubble.point = new System.Drawing.Point(posX, line);
        //                                    bubble.subLine = bubblesSubLine;

        //                                    if (!allContourMultiLine.Keys.Contains(bubble))
        //                                    {
        //                                        allContourMultiLine.Add(bubble, contour1);
        //                                        Array.Resize(ref factRectangle, factRectangle.Length + 1);
        //                                        factRectangle[factRectangle.Length - 1] = r1;
        //                                        k = r1.Right + 1;
        //                                    }
        //                                    else
        //                                    {
        //                                        if (bubble.Equals(prevBubble))
        //                                        {
        //                                            k += bubbleStepX;
        //                                        }
        //                                        else
        //                                        {
        //                                            prevBubble = bubble;
        //                                            int index = GetIndex(allContourMultiLine, bubble);
        //                                            if (factRectangle[index].Size == new System.Drawing.Size())//!!!
        //                                            {
        //                                                allContourMultiLine[bubble] = contour1;
        //                                                factRectangle[index] = r1;
        //                                                //удаление вставленных на всякий случай
        //                                                //Array.Resize(ref  factRectangle, index + 1);
        //                                                //for (int l = allContourMultiLine.Count - 1; l >= index + 1; l--)
        //                                                //{
        //                                                //    allContourMultiLine.Remove(allContourMultiLine.ElementAt(l).Key);
        //                                                //}
        //                                            }
        //                                            k = r1.Right + 1;
        //                                        }
        //                                    }
        //                                    int tmp = (r1.Y + r1.Height / 2) - yn;
        //                                    if (tmp < r1.Height / 4)
        //                                    {
        //                                        yn = r1.Y + r1.Height / 2;
        //                                    }
        //                                }
        //                                catch (Exception)
        //                                {
        //                                }
        //                                #endregion
        //                            }
        //                            else
        //                            {
        //                                #region плохой пузырь
        //                                try
        //                                {
        //                                    if (posX > 0)// && posX < bubblesPerWidth - 1
        //                                    {
        //                                        bubble = new Bubble();
        //                                        bubble.areaNumber = bubblesRegion;
        //                                        bubble.point = new System.Drawing.Point(posX - 1, line);
        //                                        bubble.subLine = bubblesSubLine;
        //                                        if (!allContourMultiLine.Keys.Contains(bubble))
        //                                        {
        //                                            int np = posX - 1;
        //                                            for (int l = np - 1; l > -1; l--)
        //                                            {
        //                                                np = l;
        //                                                bubble.point = new System.Drawing.Point(np, line);
        //                                                if (allContourMultiLine.Keys.Contains(bubble))
        //                                                {
        //                                                    np++;
        //                                                    break;
        //                                                }
        //                                            }

        //                                            for (int l = np; l < posX; l++)
        //                                            {
        //                                                bubble.point = new System.Drawing.Point(l, line);
        //                                                allContourMultiLine.Add(bubble, new System.Drawing.Point[0]);
        //                                                Array.Resize(ref factRectangle, factRectangle.Length + 1);

        //                                                //using (Graphics g = Graphics.FromImage(bmp))
        //                                                //{
        //                                                //    g.DrawPolygon(new Pen(Color.Red), contour1);
        //                                                //    g.DrawRectangle(new Pen(Color.Green), r1);
        //                                                //}
        //                                                //bmp.Save("bubbles.bmp", ImageFormat.Bmp);

        //                                                SetFactRectangle
        //                                                    (
        //                                                      factRectangle
        //                                                    , bubbleStepY
        //                                                    , diffBubble
        //                                                    , ref firstBubblePerLineLocation
        //                                                    , ref prevGoodBubbleLine
        //                                                    , line
        //                                                    , ref firstBubblePerLine
        //                                                    , ref firstBubblePerLineRectangle
        //                                                    , l
        //                                                    , ref firstBubbleRegion
        //                                                    , bubblesRegion
        //                                                    , bubblesSubLine
        //                                                    , ref prevGoodBubbleLineLocationY
        //                                                    , ref prevGoodBubbleSubLine
        //                                                    , bubblesSubLinesStep[bubblesRegion]
        //                                                    , lineFactStepY
        //                                                    );
        //                                            }
        //                                        }
        //                                    }

        //                                    bubble = new Bubble();
        //                                    bubble.areaNumber = bubblesRegion;
        //                                    bubble.point = new System.Drawing.Point(posX, line);
        //                                    bubble.subLine = bubblesSubLine;

        //                                    if (!allContourMultiLine.Keys.Contains(bubble))
        //                                    {
        //                                        allContourMultiLine.Add(bubble, new System.Drawing.Point[0]);
        //                                        Array.Resize(ref factRectangle, factRectangle.Length + 1);
        //                                        SetFactRectangle
        //                                            (
        //                                              factRectangle
        //                                            , bubbleStepY
        //                                            , diffBubble
        //                                            , ref firstBubblePerLineLocation
        //                                            , ref prevGoodBubbleLine
        //                                            , line
        //                                            , ref firstBubblePerLine
        //                                            , ref firstBubblePerLineRectangle
        //                                            , posX
        //                                            , ref firstBubbleRegion
        //                                            , bubblesRegion
        //                                            , bubblesSubLine
        //                                            , ref prevGoodBubbleLineLocationY
        //                                            , ref prevGoodBubbleSubLine
        //                                            , bubblesSubLinesStep[bubblesRegion]
        //                                            , lineFactStepY
        //                                            );
        //                                        //using (Graphics g = Graphics.FromImage(bmp))
        //                                        //{
        //                                        //    g.DrawRectangle(new Pen(Color.Green)
        //                                        //        , new Rectangle(factRectangle[factRectangle.Length - 1].X
        //                                        //            , factRectangle[factRectangle.Length - 1].Y
        //                                        //            , 60, 30));
        //                                        //}
        //                                        //bmp.Save("factRectangle.bmp", ImageFormat.Bmp);
        //                                    }
        //                                }
        //                                catch (Exception)
        //                                {
        //                                }
        //                                k += (diffBubble - diffBubble / 2);
        //                                #endregion
        //                            }
        //                            #endregion
        //                        }
        //                        else
        //                        {
        //                            #region color == argbWhite
        //                            //Bitmap b = (Bitmap)bmp.Clone();
        //                            //using (Graphics g = Graphics.FromImage(b))
        //                            //{
        //                            //    //g.DrawPolygon(new Pen(Color.Red), curvePointsRT);
        //                            //    b.SetPixel(k, yn, Color.Red);
        //                            //}
        //                            //b.Save("strips.bmp", ImageFormat.Bmp);
        //                            //b.Dispose();

        //                            if (k >= bubblesRegions[bubblesRegion].Right - 1 + bubble1.Width / 4)
        //                            {
        //                                bubble = new Bubble();
        //                                bubble.areaNumber = bubblesRegion;
        //                                bubble.point = bubble.point = new System.Drawing.Point(0, line);
        //                                bubble.subLine = 0;
        //                                if (!allContourMultiLine.ContainsKey(bubble))
        //                                {
        //                                    correct++;
        //                                    k = xn - 1;
        //                                    switch (correct)
        //                                    {
        //                                        case 1:
        //                                            yn -= bubble1.Height / 2;
        //                                            break;
        //                                        case 2:
        //                                        case 3:
        //                                        case 4:
        //                                        case 5:
        //                                            yn += bubble1.Height;
        //                                            break;
        //                                        default:
        //                                            //bmp.SetPixel(k, yn, Color.Red);
        //                                            //bmp.SetPixel(k - 1, yn, Color.Red);
        //                                            //bmp.SetPixel(k + 1, yn - 1, Color.Red);
        //                                            //bmp.SetPixel(k - 1, yn + 1, Color.Red);
        //                                            //pictureBox1.Image = (Bitmap)bmp.Clone();
        //                                            //Application.DoEvents();
        //                                            //fs.Close();
        //                                            //return;
        //                                            barCodesPrompt = PromptCalibrationError(bubble);
        //                                            break;//return;
        //                                    }
        //                                    if (barCodesPrompt != "")
        //                                    {
        //                                        break;
        //                                    }
        //                                    continue;
        //                                }
        //                                else
        //                                {
        //                                    correct = 0;
        //                                }
        //                            }
        //                            #endregion
        //                        }
        //                    }
        //                    #endregion
        //                }
        //                else
        //                {
        //                    #region vertical
        //                    for (int k = yn; k < bubblesRegions[bubblesRegion].Bottom + bubble1.Height / 8; k++)
        //                    {
        //                        color = argbWhite;
        //                        try
        //                        {
        //                            color = bmp.GetPixel(xn, k);
        //                        }
        //                        catch (Exception)
        //                        {
        //                            break;
        //                        }
        //                        if (color != argbWhite)
        //                        {
        //                            #region color != argbWhite
        //                            contour1 = ContourFind
        //                                (
        //                                  bmp
        //                                , xn
        //                                , k
        //                                , 2//вниз
        //                                , false
        //                                , true
        //                                , false
        //                                , contourLength
        //                                );
        //                            System.Drawing.Point p;
        //                            GetOuterContour
        //                               (
        //                                 bmp
        //                               , ref contour1
        //                               , ref r1
        //                               , out p
        //                               );
        //                            if (contour1 == null
        //                                || r1.Size.Equals(new System.Drawing.Size(0, 0)))
        //                            {
        //                                continue;
        //                            }
        //                            if (p == new System.Drawing.Point(int.MaxValue, 0))
        //                            {
        //                                k += bubbleStepY;
        //                                continue;
        //                            }
        //                            //if (Math.Abs(xn - p.X) > diffBubble / 2)
        //                            //{
        //                            //    continue;
        //                            //}
        //                            posX = (int)Math.Round((decimal)(r1.Y - bubblesRegions[bubblesRegion].Y) / diffBubble);
        //                            if (posX < 0 && contour1.Length < contourLength)
        //                            {
        //                                if (Math.Abs(r1.Width - bubble1.Width) <= caliberWidth
        //                                  && Math.Abs(r1.Height - bubble1.Height) <= caliberHeight
        //                                  && Array.IndexOf(factRectangle, r1) < 0)
        //                                {
        //                                    int tmp = (bubblesRegions[bubblesRegion].Y - r1.Y);
        //                                    if (tmp < caliberHeight / 4)
        //                                    {
        //                                        bubblesRegions[bubblesRegion].Y = r1.Y;
        //                                    }
        //                                }
        //                                posX = 0;
        //                            }
        //                            int count = (int)Math.Round((double)(r1.Bottom - k) / diffBubble);
        //                            //if (count == 0)
        //                            //{
        //                            //    continue;
        //                            //} 
        //                            if (count > 1)
        //                            {//вставляем на всякий случай
        //                                posX += count;
        //                            }
        //                            if (posX >= bubblesPerWidth)
        //                            {
        //                                posX = bubblesPerWidth - 1;
        //                            }
        //                            if (contour1.Length >= contourLength)
        //                            {
        //                                posX = bubblesPerWidth - 1;
        //                            }

        //                            bubble = new Bubble();
        //                            bubble.areaNumber = bubblesRegion;
        //                            bubble.point = new System.Drawing.Point(posX, line);
        //                            bubble.subLine = bubblesSubLine;
        //                            if (line - bubble.point.Y > 1)
        //                            {
        //                                endBubblesRegions = true;
        //                            }

        //                            if (prevRectangle == r1)
        //                            {
        //                                continue;
        //                            }
        //                            prevRectangle = r1;

        //                            if (Math.Abs(r1.Width - bubble1.Width) <= caliberWidth
        //                                  && Math.Abs(r1.Height - bubble1.Height) <= caliberHeight
        //                                  && Array.IndexOf(factRectangle, r1) < 0)
        //                            {//хороший пузырь

        //                                if (posX == 0)
        //                                {
        //                                    firstBubblePerLineLocation = r1.Location;
        //                                    firstBubblePerLine = new Bubble();
        //                                    firstBubblePerLine.point = new System.Drawing.Point(posX, line);
        //                                }
        //                                if (contourLength == 10000)
        //                                {
        //                                    contourLength = contour1.Length * 4;
        //                                }
        //                                try
        //                                {
        //                                    if (posX > 0 && posX < bubblesPerWidth - 1)
        //                                    {
        //                                        bubble = new Bubble();
        //                                        bubble.areaNumber = bubblesRegion;
        //                                        bubble.point = new System.Drawing.Point(posX - 1, line);
        //                                        bubble.subLine = bubblesSubLine;

        //                                        if (!allContourMultiLine.Keys.Contains(bubble))
        //                                        {
        //                                            int np = posX - 1;
        //                                            for (int l = np - 1; l > -1; l--)
        //                                            {
        //                                                np = l;
        //                                                bubble.point = new System.Drawing.Point(np, line);
        //                                                if (allContourMultiLine.Keys.Contains(bubble))
        //                                                {
        //                                                    np++;
        //                                                    break;
        //                                                }
        //                                            }
        //                                            for (int l = np; l < posX; l++)
        //                                            {
        //                                                bubble.point = new System.Drawing.Point(l, line);
        //                                                allContourMultiLine.Add(bubble, new System.Drawing.Point[0]);
        //                                                Array.Resize(ref factRectangle, factRectangle.Length + 1);
        //                                                factRectangle[factRectangle.Length - 1]
        //                                                    = new Rectangle(r1.X - diffBubble * (posX - l), r1.Y, 0, 0);
        //                                            }
        //                                        }
        //                                    }
        //                                    bubble = new Bubble();
        //                                    bubble.areaNumber = bubblesRegion;
        //                                    bubble.point = new System.Drawing.Point(posX, line);
        //                                    bubble.subLine = bubblesSubLine;
        //                                    if (!allContourMultiLine.Keys.Contains(bubble))
        //                                    {
        //                                        allContourMultiLine.Add(bubble, contour1);
        //                                        Array.Resize(ref factRectangle, factRectangle.Length + 1);
        //                                        factRectangle[factRectangle.Length - 1] = r1;
        //                                        k = r1.Bottom + 1;
        //                                    }
        //                                    else
        //                                    {
        //                                        if (bubble.Equals(prevBubble))
        //                                        {
        //                                            k += bubbleStepY;
        //                                        }
        //                                        else
        //                                        {
        //                                            int index = GetIndex(allContourMultiLine, bubble);
        //                                            if (factRectangle[index].Size == new System.Drawing.Size())//!!!
        //                                            {
        //                                                prevBubble = bubble;
        //                                                allContourMultiLine[bubble] = contour1;
        //                                                factRectangle[index] = r1;
        //                                                //удаление вставленных на всякий случай
        //                                                //Array.Resize(ref  factRectangle, index + 1);
        //                                                //for (int l = allContourMultiLine.Count - 1; l >= index + 1; l--)
        //                                                //{
        //                                                //    allContourMultiLine.Remove(allContourMultiLine.ElementAt(l).Key);
        //                                                //}
        //                                            }
        //                                            k = r1.Bottom + 1;
        //                                        }
        //                                    }
        //                                    int tmp = (r1.X + r1.Width / 2) - xn;
        //                                    if (tmp < r1.Width / 4)
        //                                    {
        //                                        xn = r1.X + r1.Width / 2;
        //                                    }
        //                                }
        //                                catch { }
        //                            }
        //                            else
        //                            {
        //                                #region плохой пузырь
        //                                try
        //                                {
        //                                    if (posX > 0)
        //                                    {
        //                                        bubble = new Bubble();
        //                                        bubble.areaNumber = bubblesRegion;
        //                                        bubble.point = new System.Drawing.Point(posX - 1, line);
        //                                        bubble.subLine = bubblesSubLine;
        //                                        if (!allContourMultiLine.Keys.Contains(bubble))
        //                                        {
        //                                            int np = posX - 1;
        //                                            for (int l = np - 1; l > -1; l--)
        //                                            {
        //                                                np = l;
        //                                                bubble.point = new System.Drawing.Point(np, line);
        //                                                if (allContourMultiLine.Keys.Contains(bubble))
        //                                                {
        //                                                    np++;
        //                                                    break;
        //                                                }
        //                                            }
        //                                            for (int l = np; l < posX; l++)
        //                                            {
        //                                                bubble.point = new System.Drawing.Point(l, line);
        //                                                allContourMultiLine.Add(bubble, new System.Drawing.Point[0]);
        //                                                Array.Resize(ref factRectangle, factRectangle.Length + 1);
        //                                                factRectangle[factRectangle.Length - 1]
        //                                                    = new Rectangle(firstBubblePerLineLocation.X
        //                                                       , firstBubblePerLineLocation.Y
        //                                                       - diffBubble * (firstBubblePerLine.point.X - l)
        //                                                       , 0, 0);
        //                                            }
        //                                        }
        //                                    }

        //                                    bubble = new Bubble();
        //                                    bubble.areaNumber = bubblesRegion;
        //                                    bubble.point = new System.Drawing.Point(posX, line);
        //                                    bubble.subLine = bubblesSubLine;
        //                                    if (!allContourMultiLine.Keys.Contains(bubble))
        //                                    {
        //                                        allContourMultiLine.Add(bubble, new System.Drawing.Point[0]);
        //                                        Array.Resize(ref factRectangle, factRectangle.Length + 1);
        //                                        factRectangle[factRectangle.Length - 1]
        //                                                = new Rectangle
        //                                                    (
        //                                                       firstBubblePerLineLocation.X
        //                                                     , firstBubblePerLineLocation.Y + diffBubble * (posX - firstBubblePerLine.point.X)
        //                                                     , 0
        //                                                     , 0
        //                                                     );
        //                                    }
        //                                }
        //                                catch (Exception)
        //                                {
        //                                }
        //                                k += (diffBubble - diffBubble / 2);
        //                                #endregion
        //                            }
        //                            #endregion
        //                        }
        //                        else
        //                        {
        //                            #region color == argbWhite
        //                            if (k >= bubblesRegions[bubblesRegion].Bottom - 1 + bubble1.Width / 4)
        //                            {
        //                                bubble = new Bubble();
        //                                bubble.areaNumber = bubblesRegion;
        //                                bubble.point = bubble.point = new System.Drawing.Point(0, line);
        //                                bubble.subLine = 0;
        //                                if (!allContourMultiLine.ContainsKey(bubble))
        //                                {
        //                                    correct++;
        //                                    k = xn - 1;
        //                                    switch (correct)
        //                                    {
        //                                        case 1:
        //                                            yn -= bubble1.Height / 2;
        //                                            break;
        //                                        case 2:
        //                                        case 3:
        //                                        case 4:
        //                                        case 5:
        //                                            yn += bubble1.Height;
        //                                            break;
        //                                        default:
        //                                            //bmp.SetPixel(k, yn, Color.Red);
        //                                            //bmp.SetPixel(k - 1, yn, Color.Red);
        //                                            //bmp.SetPixel(k + 1, yn - 1, Color.Red);
        //                                            //bmp.SetPixel(k - 1, yn + 1, Color.Red);
        //                                            //pictureBox1.Image = (Bitmap)bmp.Clone();
        //                                            //Application.DoEvents();
        //                                            //fs.Close();
        //                                            //return;
        //                                            barCodesPrompt = PromptCalibrationError(bubble);
        //                                            break;//return;
        //                                    }
        //                                    continue;
        //                                }
        //                                else
        //                                {
        //                                    correct = 0;
        //                                }
        //                            }
        //                            #endregion
        //                        }
        //                    }
        //                    #endregion
        //                }
        //                if (barCodesPrompt != "")
        //                {
        //                    break;
        //                }
        //                if (allContourMultiLine.Count == 0)
        //                {
        //                    barCodesPrompt = "The error in determining the regions of bubbles";
        //                    break;
        //                }
        //                if (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
        //                    || areas[bubblesRegion].bubblesOrientation == "horizontal")
        //                {
        //                    #region horizontal
        //                    //try
        //                    //{
        //                    if (bubblesSubLinesCount[bubblesRegion] > 0)
        //                    {
        //                        bubblesSubLine++;
        //                        if (bubblesSubLine <= bubblesSubLinesCount[bubblesRegion])
        //                        {
        //                            yn += bubblesSubLinesStep[bubblesRegion];
        //                            line--;
        //                        }
        //                        else
        //                        {
        //                            bubblesSubLine--;
        //                            yn -= bubblesSubLinesStep[bubblesRegion] * bubblesSubLine;
        //                            bubblesSubLine = 0;
        //                            yn += (bubble1.Height + bubbleStepY);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        yn += (bubble1.Height + bubbleStepY);
        //                    }
        //                    if (bubblesSubLine == 0)
        //                    {
        //                        KeyValuePair<Bubble, System.Drawing.Point[]> kvp
        //                        = allContourMultiLine.ElementAt(allContourMultiLine.Count - 1);
        //                        bubble = kvp.Key;
        //                        if (line + 1 - bubble.point.Y == 2)
        //                        {//пустая строка
        //                            for (int l = 0; l < bubblesSubLinesCount[bubblesRegion] + 1; l++)
        //                            {
        //                                for (int k = 0; k < bubblesPerLine[bubblesRegion]; k++)
        //                                {
        //                                    bubble = new Bubble();
        //                                    bubble.areaNumber = bubblesRegion;
        //                                    bubble.point = new System.Drawing.Point(k, line);
        //                                    bubble.subLine = l;
        //                                    allContourMultiLine.Add(bubble, new System.Drawing.Point[0]);
        //                                    Array.Resize(ref factRectangle, factRectangle.Length + 1);
        //                                    factRectangle[factRectangle.Length - 1]
        //                                        = new Rectangle(
        //                                                 firstBubblePerLineLocation.X
        //                                                 + diffBubble * k
        //                                               , firstBubblePerLineLocation.Y
        //                                               , 0
        //                                               , 0
        //                                               );
        //                                }
        //                            }
        //                        }
        //                        regionLines++;
        //                        if (regionLines >= bubblesPerHeight
        //                            )//|| yn >= bubblesRegions[bubblesRegion].Bottom
        //                        {
        //                            regionLines = 0;
        //                            bubblesRegion++;
        //                            if (bubblesRegions.Length > bubblesRegion)
        //                            {
        //                                bubble1 = bubblesOfRegion[bubblesRegion];
        //                                bubblesPerWidth = bubblesPerLine[bubblesRegion];
        //                                bubblesPerHeight = linesPerArea[bubblesRegion];
        //                                if (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
        //                                    || areas[bubblesRegion].bubblesOrientation == "horizontal")
        //                                {
        //                                    xn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].X - bubble1.Width / 8));
        //                                    yn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y + bubble1.Height / 2));
        //                                    bubbleStepX = (int)Math.Round((decimal)(
        //                                        bubblesRegions[bubblesRegion].Width
        //                                        - bubble1.Width * bubblesPerWidth)
        //                                        / (bubblesPerWidth - 1));
        //                                    //bubbleStepY = (int)Math.Round((decimal)(lineHeight[bubblesRegion] - bubble1.Height));
        //                                    diffBubble = bubble1.Width + bubbleStepX;
        //                                }
        //                                else
        //                                {
        //                                    xn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].X + bubble1.Width / 2));
        //                                    yn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y - bubble1.Height / 4));
        //                                    bubbleStepY = (int)Math.Round((decimal)(
        //                                        bubblesRegions[bubblesRegion].Height
        //                                        - bubble1.Height * bubblesPerWidth)
        //                                        / (bubblesPerWidth - 1));
        //                                    bubbleStepX = (int)Math.Round((decimal)(lineHeight[bubblesRegion] - bubble1.Height));
        //                                    diffBubble = bubble1.Height + bubbleStepY;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                break;// endBubblesRegions = true;
        //                            }
        //                        }
        //                    }
        //                    //}
        //                    //catch (Exception)
        //                    //{
        //                    //}

        //                    #endregion
        //                }
        //                else
        //                {
        //                    #region vertical
        //                    if (bubblesSubLinesCount[bubblesRegion] > 0)
        //                    {
        //                        bubblesSubLine++;
        //                        if (bubblesSubLine <= bubblesSubLinesCount[bubblesRegion])
        //                        {
        //                            xn += bubblesSubLinesStep[bubblesRegion];
        //                            line--;
        //                        }
        //                        else
        //                        {
        //                            bubblesSubLine--;
        //                            xn -= bubblesSubLinesStep[bubblesRegion] * bubblesSubLine;
        //                            bubblesSubLine = 0;
        //                            xn += (bubble1.Width + bubbleStepY);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        xn += (bubble1.Width + bubbleStepY);
        //                    }
        //                    if (bubblesSubLine == 0)
        //                    {
        //                        KeyValuePair<Bubble, System.Drawing.Point[]> kvp
        //                         = allContourMultiLine.ElementAt(allContourMultiLine.Count - 1);
        //                        bubble = kvp.Key;
        //                        if (line + 1 - bubble.point.Y == 2)
        //                        {//пустая строка
        //                            for (int l = 0; l < bubblesSubLinesCount[bubblesRegion] + 1; l++)
        //                            {
        //                                for (int k = 0; k < bubblesPerLine[bubblesRegion]; k++)
        //                                {
        //                                    bubble = new Bubble();
        //                                    bubble.areaNumber = bubblesRegion;
        //                                    bubble.point = new System.Drawing.Point(k, line);
        //                                    bubble.subLine = l;
        //                                    allContourMultiLine.Add(bubble, new System.Drawing.Point[0]);
        //                                    Array.Resize(ref factRectangle, factRectangle.Length + 1);
        //                                    factRectangle[factRectangle.Length - 1]
        //                                        = new Rectangle(
        //                                                 firstBubblePerLineLocation.X
        //                                               , firstBubblePerLineLocation.Y
        //                                               + diffBubble * k
        //                                               , 0
        //                                               , 0
        //                                               );
        //                                }
        //                            }
        //                        }
        //                        regionLines++;
        //                        if (regionLines == bubblesPerHeight
        //                           ) //|| xn >= bubblesRegions[bubblesRegion].Right
        //                        {
        //                            regionLines = 0;
        //                            bubblesRegion++;
        //                            if (bubblesRegions.Length > bubblesRegion)
        //                            {
        //                                bubble1 = bubblesOfRegion[bubblesRegion];
        //                                bubblesPerWidth = bubblesPerLine[bubblesRegion];
        //                                bubblesPerHeight = linesPerArea[bubblesRegion];
        //                                if (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
        //                                    || areas[bubblesRegion].bubblesOrientation == "horizontal")
        //                                {
        //                                    xn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].X - bubble1.Width / 8));
        //                                    yn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y + bubble1.Height / 2));
        //                                    bubbleStepX = (int)Math.Round((decimal)(
        //                                        bubblesRegions[bubblesRegion].Width
        //                                        - bubble1.Width * bubblesPerWidth)
        //                                        / (bubblesPerWidth - 1));
        //                                    bubbleStepY = (int)Math.Round((decimal)(lineHeight[bubblesRegion] - bubble1.Height));
        //                                    diffBubble = bubble1.Width + bubbleStepX;

        //                                }
        //                                else
        //                                {
        //                                    xn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].X + bubble1.Width / 2));
        //                                    yn = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Y - bubble1.Height / 4));
        //                                    bubbleStepY = (int)Math.Round((decimal)(
        //                                        bubblesRegions[bubblesRegion].Height
        //                                        - bubble1.Height * bubblesPerWidth)
        //                                        / (bubblesPerWidth - 1));
        //                                    bubbleStepX = (int)Math.Round((decimal)(lineHeight[bubblesRegion] - bubble1.Height));
        //                                    diffBubble = bubble1.Height + bubbleStepY;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                break;
        //                            }
        //                        }
        //                    }
        //                    yn += deltaY;
        //                    #endregion
        //                }
        //                firstBubblePerLineLocation
        //                = new System.Drawing.Point
        //                    (bubblesRegions[bubblesRegion].X
        //                    , bubblesRegions[bubblesRegion].Y + deltaY
        //                    );
        //                #endregion
        //            }
        //            int minContourLength = int.MaxValue;
        //            int maxBubblesDist;
        //            int goodBubbleNumber = -1;
        //            const int factor = 2;
        //            int[] axisX = new int[1];
        //            int[] axisY = new int[1];
        //            //Rectangle[] factRectangleMem = new Rectangle[0];
        //            //if (maxCountRectangles != null)
        //            //{
        //            //    factRectangleMem = new Rectangle[factRectangle.Length];
        //            //    Array.Copy(factRectangle, factRectangleMem, factRectangle.Length);
        //            //}

        //            for (int k = 0; k < allContourMultiLine.Count; k++)
        //            {
        //                #region
        //                KeyValuePair<Bubble, System.Drawing.Point[]> item = allContourMultiLine.ElementAt(k);
        //                try
        //                {
        //                    if (k < allContourMultiLine.Count - 1)
        //                    {
        //                        if (factRectangle.ElementAt(k).Height > 0 && factRectangle.ElementAt(k + 1).Height > 0)
        //                        {
        //                            KeyValuePair<Bubble, System.Drawing.Point[]> itm = allContourMultiLine.ElementAt(k + 1);
        //                            if (itm.Key.point.Y == item.Key.point.Y && itm.Key.subLine == item.Key.subLine)
        //                            {
        //                                if (String.IsNullOrEmpty(areas[item.Key.areaNumber].bubblesOrientation)
        //                                    || areas[item.Key.areaNumber].bubblesOrientation == "horizontal")
        //                                {
        //                                    if (Math.Abs(factRectangle.ElementAt(k).Y - factRectangle.ElementAt(k + 1).Y) > factRectangle.ElementAt(k).Height / 2)
        //                                    {
        //                                        barCodesPrompt = PromptCalibrationError(item.Key);
        //                                        if (i > 0)
        //                                            return;
        //                                        else
        //                                            break;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    if (Math.Abs(factRectangle.ElementAt(k).X - factRectangle.ElementAt(k + 1).X) > factRectangle.ElementAt(k).Width / 2)
        //                                    {
        //                                        barCodesPrompt = PromptCalibrationError(item.Key);
        //                                        if (i > 0)
        //                                            return;
        //                                        else
        //                                            break;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                catch { }

        //                if (item.Value.Length > 0)
        //                {
        //                    if (item.Value.Length < minContourLength)
        //                    {
        //                        minContourLength = item.Value.Length;
        //                    }
        //                }
        //                #endregion
        //            }
        //            minContourLength = minContourLength + minContourLength / 4;

        //            if (smartResize)
        //            {
        //                bmp = ResizeBitmap(bmp, factor);
        //                if (filterType > 0 && filterType < 1)//.65)//<= .75
        //                {
        //                    //bmp.Save("ResizeBitmap2.bmp", ImageFormat.Bmp);
        //                    binaryzeMap(bmp, bmp.Width, bmp.Height, 3);//5 bmp =GlueFilter(bmp,new Rectangle(0,0,bmp.Width,bmp.Height),1);//
        //                }
        //                //bmp.Save("ResizeBitmap3.bmp", ImageFormat.Bmp);
        //            }
        //            maxBubblesDist = 25;// 49;//10
        //            for (int k = 0; k < allContourMultiLine.Count - 1; k++)
        //            {
        //                KeyValuePair<Bubble, System.Drawing.Point[]> item = allContourMultiLine.ElementAt(k);
        //                if (factRectangle[k].Size != new System.Drawing.Size(0, 0)
        //                    && item.Value.Length <= minContourLength
        //                    && InsideContour(item))
        //                {
        //                    goodBubbleNumber = k;
        //                    break;
        //                }
        //            }
        //            prevCurve = new System.Drawing.Point[0];
        //            bubblesRegion = 0;
        //            bubblesPerHeight = linesPerArea[bubblesRegion];
        //            int currentLine = indexOfFirstQuestion;

        //            bubblesPerWidth = bubblesPerLine[bubblesRegion];
        //            Dictionary<Bubble, double> currentLineBubbles = new Dictionary<Bubble, double>();
        //            double bestPercent = darknessDifferenceLevel / 100;//nudPerCentBestBubble.Value
        //            bubble = new Bubble();
        //            int prevGoodLine = 0, prevGoodLineY = 0;
        //            areaNumber = -1;
        //            double factStepY = 0;
        //            int goodBubble = 0;
        //            int baddBubble = 0;

        //            for (int k = 0; k < allContourMultiLine.Count; k++)
        //            {
        //                #region калибровка, замена, распознавание пузырей
        //                KeyValuePair<Bubble, System.Drawing.Point[]> item = allContourMultiLine.ElementAt(k);
        //                posX = item.Key.point.X;
        //                posY = item.Key.point.Y;
        //                bubblesRegion = item.Key.areaNumber;
        //                if (areaNumber != bubblesRegion)
        //                {
        //                    areaNumber = bubblesRegion;
        //                    //factStepY = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Height
        //                    // bubblesPerLine[bubblesRegion]));
        //                    if (String.IsNullOrEmpty(areas[0].bubblesOrientation)
        //                      || areas[0].bubblesOrientation == "horizontal")
        //                    {
        //                        bubbleStepY = (int)Math.Round((decimal)(lineHeight[bubblesRegion] - bubble1.Height));
        //                    }
        //                    else
        //                    {
        //                        bubbleStepY = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Height
        //                            - bubble1.Height * bubblesPerWidth) / (bubblesPerWidth - 1));
        //                    }
        //                    factStepY = bubble1.Height + bubbleStepY;

        //                    prevGoodLine = 0; prevGoodLineY = 0;
        //                    GetLineFactStep
        //                     (
        //                       ref factStepY
        //                     , ref prevGoodLine
        //                     , ref prevGoodLineY
        //                     , k
        //                     , areas[bubblesRegion]
        //                     , factRectangle
        //                     , allContourMultiLine
        //                     , bubblesRegion
        //                     );
        //                    bubblesPerWidth = bubblesPerLine[bubblesRegion];
        //                    bubble1 = bubblesOfRegion[bubblesRegion];
        //                    if (string.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
        //                        || areas[bubblesRegion].bubblesOrientation == "horizontal")
        //                    {
        //                        bubbleStepX = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Width
        //                            - bubble1.Width * bubblesPerWidth) / (bubblesPerWidth - 1));
        //                    }
        //                    else
        //                    {
        //                        if (areas[bubblesRegion].subLinesAmount == 0)
        //                        {
        //                            bubblesSubLinesStep[bubblesRegion] = (int)Math.Round((decimal)(areas[0].bubble.Width * 2) * kx);
        //                        }
        //                        bubbleStepX = bubblesSubLinesStep[bubblesRegion];
        //                        bubbleStepY = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Height
        //                            / bubblesPerLine[bubblesRegion] - bubble1.Height));
        //                    }
        //                }
        //                if (item.Value.Length == 0 || item.Value.Length > minContourLength || !InsideContour(item))
        //                {
        //                    #region плохой пузырь // almost like in Recognize -> BadBubble()
        //                    int dist, minDist = int.MaxValue, numCont = -1;
        //                    int n;
        //                    baddBubble++;

        //                    KeyValuePair<Bubble, System.Drawing.Point[]> itm = allContourMultiLine.ElementAt(k);
        //                    for (int kn = 1; kn < maxBubblesDist; kn++)
        //                    {
        //                        n = k + kn;
        //                        if (n < allContourMultiLine.Count - 1)
        //                        {
        //                            itm = allContourMultiLine.ElementAt(n);
        //                            //if ((regionRectangle.ElementAt(n).Value == regionRectangle.ElementAt(k).Value)
        //                            if ((itm.Key.areaNumber == item.Key.areaNumber)
        //                                && itm.Value.Length != 0
        //                                && itm.Value.Length <= minContourLength)
        //                            {
        //                                dist = Math.Abs(item.Key.point.X - itm.Key.point.X)
        //                                     + Math.Abs(item.Key.point.Y - itm.Key.point.Y)
        //                                     + Math.Abs(item.Key.subLine - itm.Key.subLine);
        //                                if (dist < minDist)
        //                                    if (!InsideContour(itm))
        //                                        continue;
        //                                if (dist <= 1)
        //                                {
        //                                    numCont = n;
        //                                    break;
        //                                }
        //                                else
        //                                {
        //                                    if (dist < minDist)
        //                                    {
        //                                        minDist = dist;
        //                                        numCont = n;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        n = k - kn;
        //                        if (n > -1)
        //                        {
        //                            itm = allContourMultiLine.ElementAt(n);
        //                            if ((itm.Key.areaNumber == item.Key.areaNumber)
        //                                && itm.Value.Length != 0 && itm.Value.Length <= minContourLength)
        //                            {
        //                                dist = Math.Abs(item.Key.point.X - itm.Key.point.X)
        //                                     + Math.Abs(item.Key.point.Y - itm.Key.point.Y)
        //                                     + Math.Abs(item.Key.subLine - itm.Key.subLine);
        //                                if (dist < minDist)
        //                                {
        //                                    if (!InsideContour(itm))
        //                                        continue;
        //                                }
        //                                if (dist <= 1)
        //                                {
        //                                    numCont = n;
        //                                    break;
        //                                }
        //                                else
        //                                {
        //                                    if (dist < minDist)
        //                                    {
        //                                        minDist = dist;
        //                                        numCont = n;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                    if (numCont > -1)
        //                    {//itm = замещающий, item = замещаемый
        //                        int distX;
        //                        int distY;
        //                        int distYsub;
        //                        int moveX;
        //                        int moveY;
        //                        itm = allContourMultiLine.ElementAt(numCont);
        //                        if (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
        //                              || areas[bubblesRegion].bubblesOrientation == "horizontal")
        //                        {
        //                            distX = item.Key.point.X - itm.Key.point.X;
        //                            distY = item.Key.point.Y - itm.Key.point.Y;
        //                            distYsub = item.Key.subLine - itm.Key.subLine;
        //                            moveX = (bubble1.Width + bubbleStepX) * distX;
        //                            moveY = (int)Math.Round((double)factStepY * distY
        //                                   + bubblesSubLinesStep[bubblesRegion] * distYsub);//* signY Math.Sign(itm.Key.point.Y));
        //                        }
        //                        else
        //                        {
        //                            distY = item.Key.point.X - itm.Key.point.X;
        //                            distX = item.Key.point.Y - itm.Key.point.Y;
        //                            distYsub = item.Key.subLine - itm.Key.subLine;
        //                            moveY = (int)Math.Round((double)factStepY * distY);
        //                            moveX = (int)Math.Round((double)bubbleStepX * distX
        //                                   + (bubblesSubLinesStep[bubblesRegion]) * distYsub);
        //                        }
        //                        prevCurve = new System.Drawing.Point[allContourMultiLine.ElementAt(numCont).Value.Length];
        //                        allContourMultiLine.ElementAt(numCont).Value.CopyTo(prevCurve, 0);
        //                        prevCurve = moveContour(prevCurve, moveX, moveY);
        //                        prevRectangle = new Rectangle
        //                            (
        //                              factRectangle[numCont].X
        //                            , factRectangle[numCont].Y
        //                            , factRectangle[numCont].Width
        //                            , factRectangle[numCont].Height
        //                            );
        //                        prevRectangle.X += moveX;
        //                        prevRectangle.Y += moveY;
        //                        factRectangle[k] = new Rectangle
        //                            (prevRectangle.X
        //                            , prevRectangle.Y
        //                            , prevRectangle.Width
        //                            , prevRectangle.Height
        //                            );
        //                    }
        //                    else
        //                        if (goodBubbleNumber > -1 && factRectangle[goodBubbleNumber].Size != new System.Drawing.Size())
        //                        {
        //                            if (axisX.Length == 1)
        //                            {
        //                                GetAxis
        //                                    (
        //                                      ref axisX
        //                                    , ref axisY
        //                                    , ref factRectangle
        //                                    , allContourMultiLine
        //                                    , bubble1
        //                                    //, smartResize
        //                                    //, factor
        //                                    );
        //                                if ((amoutOfQuestions <= bubblesPerHeight && axisY.Length < amoutOfQuestions)
        //                                    || (amoutOfQuestions >= bubblesPerHeight && axisY.Length < bubblesPerHeight))
        //                                {
        //                                    barCodesPrompt = "Calibration error an axis \"Y\"";
        //                                    if (i > 0)
        //                                        return;
        //                                    else
        //                                        break;
        //                                }
        //                            }
        //                            prevCurve = new System.Drawing.Point[allContourMultiLine.ElementAt(goodBubbleNumber).Value.Length];
        //                            allContourMultiLine.ElementAt(goodBubbleNumber).Value.CopyTo(prevCurve, 0);
        //                            int moveX = factRectangle[k].X - factRectangle[goodBubbleNumber].X;
        //                            int moveY = factRectangle[k].Y - factRectangle[goodBubbleNumber].Y;
        //                            prevCurve = moveContour(prevCurve, moveX, moveY);
        //                            //prevRectangle = factRectangle[goodBubbleNumber];
        //                            prevRectangle = new Rectangle
        //                            (factRectangle[goodBubbleNumber].X
        //                            , factRectangle[goodBubbleNumber].Y
        //                            , factRectangle[goodBubbleNumber].Width
        //                            , factRectangle[goodBubbleNumber].Height
        //                            );
        //                            prevRectangle.X += moveX;
        //                            prevRectangle.Y += moveY;
        //                            int bestWal = int.MaxValue;
        //                            int delta = int.MaxValue;
        //                            foreach (int item2 in axisX)
        //                            {
        //                                int delta2 = Math.Abs(prevRectangle.X - item2);
        //                                if (delta2 < delta)// bestY)
        //                                {
        //                                    delta = delta2;
        //                                    bestWal = item2;
        //                                }
        //                            }
        //                            prevRectangle.X = bestWal;
        //                            bestWal = int.MaxValue;
        //                            delta = int.MaxValue;

        //                            foreach (int item2 in axisY)
        //                            {
        //                                int delta2 = Math.Abs(prevRectangle.Y - item2);
        //                                if (delta2 < delta)// bestY)
        //                                {
        //                                    delta = delta2;
        //                                    bestWal = item2;
        //                                }
        //                            }
        //                            prevRectangle.Y = bestWal;
        //                            factRectangle[k] = new Rectangle
        //                                (
        //                                  prevRectangle.X
        //                                , prevRectangle.Y
        //                                , prevRectangle.Width
        //                                , prevRectangle.Height
        //                                );
        //                        }
        //                        else
        //                        {//err
        //                            barCodesPrompt = PromptCalibrationError(item.Key);
        //                            if (i > 0)
        //                                return;
        //                            else
        //                                break;
        //                        }
        //                    #endregion
        //                }
        //                else
        //                {
        //                    #region хороший пузырь // аналогичен Recognize -> GoodBubble()
        //                    if (bubble.Equals(new Bubble()) && item.Key.subLine == 0)
        //                    {
        //                        bubble = item.Key;
        //                        prevGoodLineY = factRectangle[k].Y;
        //                    }
        //                    else
        //                    {
        //                        //try
        //                        //{
        //                        if (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
        //                             || areas[bubblesRegion].bubblesOrientation == "horizontal")
        //                        {
        //                            if (item.Key.subLine == 0)
        //                            {//определение текущего значения lineFactStep
        //                                if (bubble.areaNumber == item.Key.areaNumber)
        //                                {
        //                                    if (bubble.point.Y != item.Key.point.Y)
        //                                    {
        //                                        factStepY = (double)(factRectangle[k].Y - prevGoodLineY)
        //                                            / (item.Key.point.Y - bubble.point.Y);
        //                                        bubble = item.Key;
        //                                        prevGoodLineY = factRectangle[k].Y;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (bubble.areaNumber == item.Key.areaNumber)
        //                            {
        //                                if (bubble.subLine == item.Key.subLine)
        //                                {
        //                                    if ((item.Key.point.X != bubble.point.X))
        //                                    {
        //                                        factStepY = (double)(factRectangle[k].Y - prevGoodLineY)
        //                                            / (item.Key.point.X - bubble.point.X);
        //                                        bubble = item.Key;
        //                                        prevGoodLineY = factRectangle[k].Y; //item.Key.point.X;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        //}
        //                        //catch (Exception)
        //                        //{ }
        //                    }
        //                    #endregion
        //                    goodBubble++;
        //                    goodBubbleNumber = k;
        //                    //prevRectangle = factRectangle[k];
        //                    //prevCurve = item.Value;
        //                    prevRectangle = new Rectangle
        //                        (factRectangle[k].X
        //                        , factRectangle[k].Y
        //                        , factRectangle[k].Width
        //                        , factRectangle[k].Height
        //                        );
        //                    prevCurve = new System.Drawing.Point[item.Value.Length];
        //                    item.Value.CopyTo(prevCurve, 0);
        //                }
        //                if (smartResize)
        //                {
        //                    System.Drawing.Point[] pts = prevCurve;
        //                    prevCurve = new System.Drawing.Point[pts.Length];
        //                    pts.CopyTo(prevCurve, 0);
        //                    Rectangle r = GetRectangle(ref prevCurve, factor);
        //                    //prevRectangle = GetRectangle(ref prevCurve, factor);
        //                    //allContourMultiLine[item.Key] = prevCurve;// pts;
        //                    prevRectangle = new Rectangle(r.X, r.Y, r.Width, r.Height);

        //                    //Bitmap b = (Bitmap)bmp.Clone();
        //                    //using (Graphics g = Graphics.FromImage(b))
        //                    //{
        //                    //    g.DrawPolygon(new Pen(Color.Cyan), prevCurve);
        //                    //    g.DrawRectangle(new Pen(Color.Red), prevRectangle);//factRectangle[k]
        //                    //}
        //                    //b.Save("strips.bmp", ImageFormat.Bmp);
        //                    //b.Dispose();

        //                }
        //                if (prevCurve.Length == 0)
        //                {
        //                    barCodesPrompt = PromptCalibrationError(bubble);
        //                    if (i > 0)
        //                        return;
        //                    else
        //                        break;
        //                }
        //                double black = 0, all = 0;
        //                int white = 0;
        //                if (darknessPercent > 0)
        //                {
        //                    #region darknessPercent > 0
        //                    using (GraphicsPath gp = new GraphicsPath())
        //                    {
        //                        gp.AddPolygon(prevCurve);
        //                        bool cont = false;
        //                        for (yn = prevRectangle.Y + 1; yn < prevRectangle.Bottom; yn++)
        //                        {
        //                            for (xn = prevRectangle.X + 1; xn < prevRectangle.Right; xn++)
        //                            {
        //                                if (gp.IsVisible(xn, yn))
        //                                {
        //                                    all++;
        //                                    cont = true;
        //                                    //try
        //                                    //{
        //                                    color = bmp.GetPixel(xn, yn);
        //                                    double f = color.GetBrightness();
        //                                    //if (color != argbWhite)
        //                                    if (f <= .7)
        //                                        black++;
        //                                    //}
        //                                    //catch { }
        //                                }
        //                                else
        //                                {
        //                                    if (cont)
        //                                    {
        //                                        cont = false;
        //                                        break;
        //                                    }
        //                                }
        //                                continue;
        //                            }
        //                        }
        //                        double perCent = (black / all) * 100;
        //                        //if (filterType > 0 && filterType < 1)
        //                        //{
        //                        //    if (perCent * 2 > darknessPercent)
        //                        //    {
        //                        //        currentLineBubbles.Add(item.Key, perCent);
        //                        //    }
        //                        //}
        //                        //else
        //                        {
        //                            if (perCent >= darknessPercent)
        //                            {
        //                                currentLineBubbles.Add(item.Key, perCent);
        //                            }
        //                        }
        //                    }
        //                    #endregion
        //                }
        //                else
        //                {
        //                    #region else
        //                    yn = prevRectangle.Y + prevRectangle.Height / 2;
        //                    y1 = yn;
        //                    x1 = prevRectangle.X;
        //                    xn = x1;
        //                    x2 = prevRectangle.Width / 8;
        //                    y2 = prevRectangle.Height / 8;
        //                    for (yn = yn - y2; yn <= y1 + y2; yn++)//несколько линий по "Y"
        //                    {
        //                        for (xn = x1 + x2; xn < prevRectangle.Right - x2; xn++)
        //                        {
        //                            if (xn == x1 + 3 * x2)
        //                            {
        //                                xn += 3 * x2;
        //                            }
        //                            color = bmp.GetPixel(xn, yn);
        //                            double f = color.GetBrightness();
        //                            //if (color != argbWhite)
        //                            if (f < .5)
        //                                black++;
        //                            else
        //                                white++;
        //                        }
        //                    }
        //                    if (filterType > 3)
        //                    {
        //                        if (white < black * 2)
        //                        {
        //                            currentLineBubbles.Add(item.Key, 0);
        //                            if (maxCountRectangles != null)
        //                            {
        //                                if (!maxCountRectangles.Keys.Contains(item.Key))
        //                                {
        //                                    if (i > 0)
        //                                        return;
        //                                    else
        //                                        break;
        //                                }
        //                                maxCountRectangles[item.Key].isChecked = true;
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (filterType > 0 && filterType < .6)
        //                        {
        //                            if (white < black * 2)
        //                            {
        //                                currentLineBubbles.Add(item.Key, 0);
        //                                if (maxCountRectangles != null)
        //                                {
        //                                    if (!maxCountRectangles.Keys.Contains(item.Key))
        //                                    {
        //                                        if (i > 0)
        //                                            return;
        //                                        else
        //                                            break;
        //                                    }
        //                                    maxCountRectangles[item.Key].isChecked = true;
        //                                }
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (white < black)
        //                            {
        //                                currentLineBubbles.Add(item.Key, 0);
        //                                if (maxCountRectangles != null)
        //                                {
        //                                    if (!maxCountRectangles.Keys.Contains(item.Key))
        //                                    {
        //                                        if (i > 0)
        //                                            return;
        //                                        else
        //                                            break;
        //                                    }
        //                                    maxCountRectangles[item.Key].isChecked = true;
        //                                }
        //                            }
        //                        }
        //                    }
        //                    #endregion
        //                }
        //                if (k >= allContourMultiLine.Count - 1
        //                    || item.Key.point.Y != allContourMultiLine.ElementAt(k + 1).Key.point.Y)
        //                {
        //                    #region результаты по строкам
        //                    bool lineChecked = false;
        //                    if (currentLineBubbles.Count == 0)
        //                    {
        //                        lineChecked = true;
        //                    }
        //                    else
        //                    {
        //                        if (darknessPercent > 0)
        //                        {
        //                            if (currentLineBubbles.Count > 1)
        //                            {
        //                                double maxPerCent = -1;
        //                                //System.Drawing.Point maxPerCentKey = new System.Drawing.Point(-1, 0);
        //                                for (int j = 0; j < currentLineBubbles.Count; j++)
        //                                {
        //                                    KeyValuePair<Bubble, double> item1 = currentLineBubbles.ElementAt(j);
        //                                    if (maxPerCent < item1.Value)
        //                                    {
        //                                        //maxPerCentKey = item1.Key.point;
        //                                        maxPerCent = item1.Value;
        //                                    }
        //                                }
        //                                //maxPerCent = 0;
        //                                //for (int j = 0; j < currentLineBubbles.Count; j++)
        //                                //{
        //                                //    KeyValuePair<Bubble, double> item1 = currentLineBubbles.ElementAt(j);
        //                                //    if (item1.Key.point == maxPerCentKey)
        //                                //    {
        //                                //        continue;
        //                                //    }
        //                                //    maxPerCent += item1.Value;
        //                                //}
        //                                //maxPerCent /= (currentLineBubbles.Count - 1);//Count - 1 <=правильно
        //                                for (int j = 0; j < currentLineBubbles.Count; j++)
        //                                {
        //                                    KeyValuePair<Bubble, double> item1 = currentLineBubbles.ElementAt(j);
        //                                    //if (item1.Value > maxPerCent * bestPercent)
        //                                    if (item1.Value * bestPercent > maxPerCent)
        //                                    {
        //                                        if (maxCountRectangles != null)
        //                                        {
        //                                            if (!maxCountRectangles.Keys.Contains(currentLineBubbles.ElementAt(j).Key))
        //                                            {
        //                                                if (i > 0)
        //                                                    return;
        //                                                else
        //                                                    break;
        //                                            }
        //                                            maxCountRectangles[item1.Key].isChecked = true;
        //                                            lineChecked = true;
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (maxCountRectangles != null)
        //                                {
        //                                    if (!maxCountRectangles.Keys.Contains(currentLineBubbles.ElementAt(0).Key))
        //                                    {
        //                                        if (i > 0)
        //                                            return;
        //                                        else
        //                                            break;
        //                                    }
        //                                    maxCountRectangles[currentLineBubbles.ElementAt(0).Key].isChecked = true;
        //                                    lineChecked = true;
        //                                }
        //                            }
        //                        }
        //                        if (!lineChecked)
        //                        {
        //                            for (int j = 0; j < currentLineBubbles.Count; j++)
        //                            {
        //                                if (maxCountRectangles != null)
        //                                {
        //                                    if (!maxCountRectangles.Keys.Contains(currentLineBubbles.ElementAt(j).Key))
        //                                    {
        //                                        if (i > 0)
        //                                            return;
        //                                        else
        //                                            break;
        //                                    }
        //                                    maxCountRectangles[currentLineBubbles.ElementAt(j).Key].isChecked = true;
        //                                }
        //                            }
        //                        }
        //                    }
        //                    currentLineBubbles.Clear();
        //                    currentLine++;
        //                    #endregion
        //                }
        //                #endregion
        //            }
        //            double percentGoodBubble = (double)goodBubble / (goodBubble + baddBubble);
        //            if (barCodesPrompt == "" && percentGoodBubble < .1)
        //            {
        //                barCodesPrompt = "Calibration error of bubbles";
        //            }
        //            if (!string.IsNullOrEmpty(barCodesPrompt) && i == 0 && filterType <= 1)
        //            {
        //                bmp = (Bitmap)bmpPres.Clone();
        //                for (int j = 0; j < 2; j++)
        //                {
        //                    using (Bitmap b2 = new Bitmap(bmp.Width / 2, bmp.Height / 2))
        //                    {
        //                        using (Graphics g = Graphics.FromImage(b2))
        //                        {
        //                            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
        //                            g.DrawImage(bmp, 0, 0, b2.Width, b2.Height);
        //                        }
        //                        //b2.Save("b2I" + i + "U1.bmp", ImageFormat.Bmp);
        //                        //bmp.Save("bmpI" + i + "U1.bmp", ImageFormat.Bmp);
        //                        using (Graphics g = Graphics.FromImage(bmp))
        //                        {
        //                            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
        //                            g.DrawImage(b2, 0, 0, bmp.Width, bmp.Height);
        //                        }
        //                    }
        //                }
        //                //bmp.Save("Calibration error.bmp", ImageFormat.Bmp);
        //                //brightness = .9;
        //                barCodesPrompt = "";
        //            }
        //            else
        //                break;
        //        }
        //    }
        //}
        //-------------------------------------------------------------------------
        private void SetFactRectangle
            (
              Rectangle[] factRectangle
            , int bubbleStepY
            , int diffBubble
            , ref Point firstBubblePerLineLocation
            , ref int firstBubbleLine
            , int line
            , ref Bubble firstBubblePerLine
            , ref Rectangle firstBubblePerLineRectangle
            , int posX
            , ref int firstBubbleRegion
            , int bubblesRegion
            , int bubblesSubLine
            , ref int prevGoodBubbleLineLocationY
            , ref int prevGoodBubbleSubLine
            , int bubblesSubLinesStep
            , int lineFactStepY
            )
        {
            if (firstBubbleRegion != bubblesRegion)
            {
                firstBubbleRegion = bubblesRegion;
                prevGoodBubbleLineLocationY = firstBubblePerLineLocation.Y;
                firstBubbleLine = line;
            }
            int diffBubbleY = bubbleStepY + firstBubblePerLineRectangle.Height;
            if (prevGoodBubbleSubLine == firstBubblePerLine.subLine
                && (firstBubbleLine == -1 || firstBubbleLine == line))// || bubblesRegion != firstBubbleRegion
            {
                factRectangle[factRectangle.Length - 1] = new Rectangle
                    (firstBubblePerLineLocation.X + diffBubble * posX
                    , prevGoodBubbleLineLocationY + (line - firstBubbleLine) * diffBubbleY, 0, 0);
            }
            else
            {
                if (bubblesSubLine == prevGoodBubbleSubLine)
                {
                    factRectangle[factRectangle.Length - 1] = new Rectangle
                        (firstBubblePerLineLocation.X + diffBubble * posX
                        , prevGoodBubbleLineLocationY//2
                        + (line - firstBubbleLine)
                        * lineFactStepY, 0, 0);//diffBubbleY
                }
                else
                {
                    //if (bubblesRegion == firstBubbleRegion)
                    if (prevGoodBubbleLineLocationY > -1)
                    {
                        factRectangle[factRectangle.Length - 1] = new Rectangle
                            (firstBubblePerLineLocation.X + diffBubble * posX
                            , prevGoodBubbleLineLocationY + (line - firstBubbleLine)
                            * diffBubbleY
                            + (bubblesSubLine - prevGoodBubbleSubLine) * bubblesSubLinesStep, 0, 0);
                    }
                    else
                    {
                        factRectangle[factRectangle.Length - 1] = new Rectangle
                            (firstBubblePerLineLocation.X + diffBubble * posX
                            , firstBubblePerLineLocation.Y
                            + (line - firstBubbleLine)
                            * (bubbleStepY + firstBubblePerLineRectangle.Height)
                            + (bubblesSubLine - prevGoodBubbleSubLine) * bubblesSubLinesStep, 0, 0);//prevGoodBubbleSubLine2
                    }
                }
            }
        }
        //-------------------------------------------------------------------------
        public string PromptCalibrationError(Bubble bubble)
        {
            return "Calibration error of bubbles: area "
                + bubble.areaNumber.ToString() + " line "
                + bubble.point.Y.ToString() + " bubble "
                + bubble.point.X.ToString() + " subline "
                + bubble.subLine.ToString();
        }
        //-------------------------------------------------------------------------
        public List<Regions> GetAllRegions(string configsFolder)
        {
            List<Regions> regionsList = new List<Regions>();
            if (Directory.Exists(configsFolder))
            {
                foreach (var fileName in Directory.GetFiles(configsFolder, "*.json"))
                {
                    Exception exception;
                    var serializer = new SerializerHelper();
                    Regions reg = serializer.GetRegionsFromFile(Path.GetFullPath(fileName), out exception);
                    if (reg == null)
                    {
                        log.LogMessage("Error in " + Path.GetFullPath(fileName) + Environment.NewLine, exception);
                        continue;
                    }
                    reg.SheetIdentifierName = reg.regions.First(x => x.name == "sheetIdentifier").value;//Path.GetFileNameWithoutExtension(fileName)
                    regionsList.Add(reg);
                }
            }
            GetSymbolsForRecognition(regionsList);
            return regionsList;
        }
        //-------------------------------------------------------------------------
        public List<Regions> GetAllRegionsFLEX(string configsFolder)
        {
            List<Regions> regionsListFLEX = new List<Regions>();
            if (Directory.Exists(configsFolder))
            {
                foreach (var fileName in Directory.GetFiles(configsFolder, "*.flex"))
                {
                    Exception exception;
                    var serializer = new SerializerHelper();
                    Regions reg = serializer.GetRegionsFromFile(Path.GetFullPath(fileName), out exception);
                    if (reg == null)
                    {
                        log.LogMessage("Error in " + Path.GetFullPath(fileName) + Environment.NewLine, exception);
                        continue;
                    }
                    regionsListFLEX.Add(reg);
                }
            }
            return regionsListFLEX;
        }
        //-------------------------------------------------------------------------
        public void WriteCsv(string destFileName, object[] totalOutput
            , int answersPosition, int indexAnswersPosition)
        {
            StringBuilder[] sb = new StringBuilder[1];
            using (StreamWriter swToCSV = new StreamWriter(destFileName, false, Encoding.ASCII))
            {
                sb = new StringBuilder[3];
                string[] iap = totalOutput[indexAnswersPosition - 1] as string[];
                string[] ap = totalOutput[answersPosition - 1] as string[];
                if (indexAnswersPosition < answersPosition)
                {
                    sb[0] = GetStringBuilder(totalOutput, 0, indexAnswersPosition - 1);
                    sb[1] = GetStringBuilder(totalOutput, indexAnswersPosition, answersPosition - 1);
                    sb[2] = GetStringBuilder(totalOutput, answersPosition, totalOutput.Length);
                    for (int k = 0; k < ap.Length; k++)
                    {
                        if (sb[2] != null)
                        {
                            swToCSV.WriteLine(sb[0].ToString() + iap[k] + "," + sb[1].ToString() + ap[k] + "," + sb[2].ToString());
                        }
                        else
                        {
                            swToCSV.WriteLine(sb[0].ToString() + iap[k] + "," + sb[1].ToString() + ap[k]);
                        }
                    }
                }
                else
                {
                    sb[0] = GetStringBuilder(totalOutput, 0, answersPosition - 1);
                    sb[1] = GetStringBuilder(totalOutput, answersPosition, indexAnswersPosition - 1);
                    sb[2] = GetStringBuilder(totalOutput, indexAnswersPosition, totalOutput.Length);
                    for (int k = 0; k < ap.Length; k++)
                    {
                        if (sb[2] != null)
                        {
                            swToCSV.WriteLine(sb[0].ToString() + ap[k] + "," + sb[1].ToString() + iap[k] + "," + sb[2].ToString());
                        }
                        else
                        {
                            swToCSV.WriteLine(sb[0].ToString() + ap[k] + "," + sb[1].ToString() + iap[k]);
                        }
                    }
                }
            }
        }
        //-------------------------------------------------------------------------
        private StringBuilder GetStringBuilder(object[] totalOutput, int indexBeg, int indexEnd)
        {
            StringBuilder sb = new StringBuilder();
            for (int k = indexBeg; k < indexEnd; k++)
            {
                if (totalOutput[k] == null)
                {
                    sb.Append(",");
                }
                else
                {
                    Type type = totalOutput[k].GetType();
                    if (type.Name == "String")
                    {
                        sb.Append(totalOutput[k].ToString());
                        if (k < totalOutput.Length - 1)
                        {
                            sb.Append(",");
                        }
                    }
                }
            }
            return sb;
        }
        //-------------------------------------------------------------------------
        #region Bitmap
        public Bitmap NormalizeBitmap(Bitmap entryBitmap, out Exception exception)
        {
            //entryBitmap.Save("entryBitmap.bmp", ImageFormat.Bmp);
            //entryBitmap = (Bitmap)entryBitmap.GetThumbnailImage(entryBitmap.Width, entryBitmap.Height, null, IntPtr.Zero);
            exception = null;
            Bitmap bmp;
            if (entryBitmap.HorizontalResolution != entryBitmap.VerticalResolution)
            {
                if (entryBitmap.HorizontalResolution > entryBitmap.VerticalResolution)
                {
                    bmp = new Bitmap
                        (
                          entryBitmap.Width
                        , entryBitmap.Height * (int)(entryBitmap.HorizontalResolution / entryBitmap.VerticalResolution)
                        , PixelFormat.Format24bppRgb
                        );
                    bmp.SetResolution(entryBitmap.HorizontalResolution, entryBitmap.HorizontalResolution);
                    Graphics g = Graphics.FromImage(bmp);
                    g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                    g.DrawImage(entryBitmap, 0, 0, bmp.Width, bmp.Height);
                    g.Dispose();
                    entryBitmap = (Bitmap)bmp.Clone();
                    bmp.Dispose();
                }
                else
                {
                    bmp = new Bitmap
                        (
                          entryBitmap.Width * (int)(entryBitmap.VerticalResolution / entryBitmap.HorizontalResolution)
                        , entryBitmap.Height
                        , PixelFormat.Format24bppRgb
                        );
                    bmp.SetResolution(entryBitmap.HorizontalResolution, entryBitmap.HorizontalResolution);
                    Graphics g = Graphics.FromImage(bmp);
                    g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                    g.DrawImage(entryBitmap, 0, 0, bmp.Width, bmp.Height);
                    g.Dispose();
                    entryBitmap = (Bitmap)bmp.Clone();
                    bmp.Dispose();
                }

                //entryBitmap.Save("entryBitmap.bmp", ImageFormat.Bmp);
            }

            entryBitmap.SetResolution(96, 96);

            if (entryBitmap.Width > entryBitmap.Height)
            {
                entryBitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
            }
            double d = (double)entryBitmap.Width / 2560;
            int height = 0;
            int width = 2560;
            if (d != 0)
                height = (int)(entryBitmap.Height / d);
            if (height > 6000)//3328//
            {
                d = (double)entryBitmap.Height / 3328;
                width = (int)Math.Round(entryBitmap.Width / d);
                height = 3328;
                if (width > 2560)
                {
                    width = entryBitmap.Width;//d = (double)3328 / entryBitmap.Height;
                    height = entryBitmap.Height;//3328;// 
                }
            }
            if (d == 0 || height == 0)//3328
            {
                //exception = new Exception("Bad image");
                return entryBitmap;
            }

            //return entryBitmap = new Bitmap(entryBitmap, new Size(width, height));
            bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);//3328.Format16bppRgb555
            Graphics gi = Graphics.FromImage(bmp);//удаление индексированных цветов
            gi.InterpolationMode = InterpolationMode.HighQualityBilinear;//Bicubic.NearestNeighbor;
            gi.DrawImage(entryBitmap, 0, 0, bmp.Width, bmp.Height);
            gi.Dispose();
            entryBitmap = (Bitmap)bmp.Clone();
            bmp.Dispose();
            //DateTime dt = DateTime.Now;

            ////bmp = bmp = ConvertTo1Bit(bmp);//bmp = binaryzeMap(bmp, bmp.Width, bmp.Height, 3)

            ////TimeSpan ts = DateTime.Now - dt;
            ////bmp.Save("NormalizeBitmap.bmp", ImageFormat.Bmp);
            return entryBitmap;
        }
        ////-------------------------------------------------------------------------
        //public Bitmap NormalizeBitmap(Bitmap entryBitmap, out Exception exception)
        //{//работает, даёт больше ошибок
        //    Image<Bgr, Byte> img = new Image<Bgr, byte>(entryBitmap);

        //    exception = null;
        //    Bitmap bmp;
        //    if (entryBitmap.HorizontalResolution != entryBitmap.VerticalResolution)
        //    {
        //        if (entryBitmap.HorizontalResolution > entryBitmap.VerticalResolution)
        //        {
        //            img = img.Resize(entryBitmap.Width
        //                 , entryBitmap.Height * (int)(entryBitmap.HorizontalResolution / entryBitmap.VerticalResolution)
        //                 , INTER.CV_INTER_NN);
        //        }
        //        else
        //        {
        //            img = img.Resize(entryBitmap.Width * (int)(entryBitmap.VerticalResolution / entryBitmap.HorizontalResolution)
        //                , entryBitmap.Height, INTER.CV_INTER_NN);
        //        }

        //        //entryBitmap.Save("entryBitmap.bmp", ImageFormat.Bmp);
        //        //img.Save("Image.bmp");
        //    }


        //    if (img.Width > img.Height)
        //    {
        //        img = img.Rotate(90, new Emgu.CV.Structure.Bgr(Color.Gray), false);
        //    }
        //    double d = (double)entryBitmap.Width / 2560;
        //    int height = 0;
        //    int width = 2560;
        //    if (d != 0)
        //        height = (int)(entryBitmap.Height / d);
        //    if (height > 6000)//3328//
        //    {
        //        d = (double)entryBitmap.Height / 3328;
        //        width = (int)Math.Round(entryBitmap.Width / d);
        //        height = 3328;
        //        if (width > 2560)
        //        {
        //            width = entryBitmap.Width;//d = (double)3328 / entryBitmap.Height;
        //            height = entryBitmap.Height;//3328;// 
        //        }
        //    }
        //    if (d == 0 || height == 0)//3328
        //    {
        //        bmp = img.ToBitmap();
        //        img.Dispose();
        //        return bmp;
        //    }

        //    img.Resize(width, height, INTER.CV_INTER_AREA);
        //    //img.Save("Image.bmp");

        //    //img.ROI = new Rectangle(0, 0, 10, 10);//работает
        //    //Image<Gray, Byte> gray = img.Convert<Gray, Byte>().PyrDown().PyrUp();
        //    //gray.ROI = new Rectangle(0, 0, 10,10);
        //    //gray.ROI = Rectangle.Empty;//освободить

        //    ////gray.Save("grayImage.bmp");
        //    //gray.Dispose();

        //    //entryBitmap.Dispose();
        //    bmp = img.ToBitmap();
        //    img.Dispose();
        //    bmp = ConvertTo1Bit(bmp);
        //    return bmp;
        //}
        //-------------------------------------------------------------------------
        public Bitmap RemoveNoise(Bitmap bmp, int iter = 1)
        {
            for (int i = 0; i < iter; i++)
            {
                bmp = RaspFilter(ref bmp, 1, new Rectangle());
                bmp = RaspFilter(ref bmp, 1, new Rectangle(), true);
            }
            return bmp;
        }

        //-------------------------------------------------------------------------
        public Bitmap ConvertTo1Bit(ref Bitmap input, float threshold = .55f)
        {
            if (input.PixelFormat == PixelFormat.Format1bppIndexed)
                return input;
            //float[][] gray_matrix ={
            //    new float[] {0.5f, 0.5f, 0.5f, 0, 0}, //input = binaryzeMap(input, input.Width, input.Height, 3);
            //    new float[] {0.5f, 0.5f, 0.5f, 0, 0}, //return input;// плохо и долго
            //    new float[] {0.5f, 0.5f, 0.5f, 0, 0},
            //    new float[] {0,    0,    0,    1, 0},
            //    new float[] {0,    0,    0,    0, 1}  };

            float[][] gray_matrix = new float[][] {
                new float[] { 0.299f, 0.299f, 0.299f, 0, 0 },
                new float[] { 0.587f, 0.587f, 0.587f, 0, 0 },
                new float[] { 0.114f, 0.114f, 0.114f, 0, 0 },
                new float[] { 0,      0,      0,      1, 0 },
                new float[] { 0,      0,      0,      0, 1 }  };
            ColorMatrix clrMatrix = new ColorMatrix(gray_matrix);//ptsArray
            ImageAttributes imgAttribs = new ImageAttributes();
            imgAttribs.SetColorMatrix(clrMatrix, ColorMatrixFlag.Default, ColorAdjustType.Default);
            imgAttribs.SetThreshold(threshold);//75f
            //if (input.PixelFormat == PixelFormat.Format1bppIndexed)
            if (input.PixelFormat.ToString().EndsWith("Indexed"))
                input = new Bitmap(input);
            Graphics g2 = Graphics.FromImage(input);
            g2.DrawImage(input, new Rectangle(0, 0, input.Width, input.Height)
                , 0, 0, input.Width, input.Height, GraphicsUnit.Pixel, imgAttribs);
            g2.Dispose();

            //input.Save("ConvertTo1Bit.bmp", ImageFormat.Bmp);

            return input;
        }
        ////-------------------------------------------------------------------------
        //public static double GetGreyLevel(byte r, byte g, byte b)
        //{
        //    return (r * 0.299 + g * 0.587 + b * 0.114) / 255;
        //}
        //-------------------------------------------------------------------------
        //public Bitmap GetMonohromeNoIndexBitmap(Bitmap entryBitmap, bool monohrome = true, bool noIndex = true)
        //{
        //    try
        //    {
        //        //TimeSpan ts;
        //        //DateTime dt;
        //        //int bitsPerPixel = entryBitmap.PixelFormat.BitsPerPixel;
        //        //if (monohrome)
        //        //{
        //        //    //dt = DateTime.Now;
        //        entryBitmap = ConvertTo1Bit(entryBitmap); //entryBitmap = CopyToBpp(entryBitmap, 1);//преобразование к однобитному
        //        //    //ts = DateTime.Now - dt;
        //        //}
        //        //else if (noIndex)
        //        ////{
        //        //entryBitmap = entryBitmap.Clone(new Rectangle(0, 0, entryBitmap.Width, entryBitmap.Height), PixelFormat.Format24bppRgb);//Format16bppRgb555
        //        ////}

        //        //b0 = new Bitmap(entryBitmap.Width, entryBitmap.Height
        //        //   , System.Drawing.Imaging.PixelFormat.Format16bppGrayScale);
        //        //Graphics gi = Graphics.FromImage(b0);//удаление индексированных цветов
        //        ////gi.DrawImage(entryBitmap, 0, 0);
        //        //gi.DrawImageUnscaledAndClipped(entryBitmap, new Rectangle(0, 0, b0.Width, b0.Height));
        //        //gi.Dispose();
        //        //entryBitmap.Dispose();
        //    }
        //    catch (Exception)
        //    { }
        //    return entryBitmap;
        //}
        ////-------------------------------------------------------------------------
        //private Bitmap CopyToBpp(Bitmap b, int bpp)
        //{
        //    if (bpp != 1 && bpp != 8) throw new System.ArgumentException("1 or 8", "bpp");
        //    // Plan: built into Windows GDI is the ability to convert
        //    // bitmaps from one format to another. Most of the time, this
        //    // job is actually done by the graphics hardware accelerator card
        //    // and so is extremely fast. The rest of the time, the job is done by
        //    // very fast native code.
        //    // We will call into this GDI functionality from C#. Our plan:
        //    // (1) Convert our Bitmap into a GDI hbitmap (ie. copy unmanaged->managed)
        //    // (2) Create a GDI monochrome hbitmap
        //    // (3) Use GDI "BitBlt" function to copy from hbitmap into monochrome (as above)
        //    // (4) Convert the monochrone hbitmap into a Bitmap (ie. copy unmanaged->managed)

        //    int w = b.Width, h = b.Height;
        //    IntPtr hbm = b.GetHbitmap(); // this is step (1)
        //    //
        //    // Step (2): create the monochrome bitmap.
        //    // "BITMAPINFO" is an interop-struct which we define below.
        //    // In GDI terms, it's a BITMAPHEADERINFO followed by an array of two RGBQUADs
        //    BITMAPINFO bmi = new BITMAPINFO();
        //    bmi.biSize = 40;  // the size of the BITMAPHEADERINFO struct
        //    bmi.biWidth = w;
        //    bmi.biHeight = h;
        //    bmi.biPlanes = 1; // "planes" are confusing. We always use just 1. Read MSDN for more info.
        //    bmi.biBitCount = (short)bpp; // ie. 1bpp or 8bpp
        //    bmi.biCompression = BI_RGB; // ie. the pixels in our RGBQUAD table are stored as RGBs, not palette indexes
        //    bmi.biSizeImage = (uint)(((w + 7) & 0xFFFFFFF8) * h / 8);
        //    bmi.biXPelsPerMeter = 1000000; // not really important
        //    bmi.biYPelsPerMeter = 1000000; // not really important
        //    // Now for the colour table.
        //    uint ncols = (uint)1 << bpp; // 2 colours for 1bpp; 256 colours for 8bpp
        //    bmi.biClrUsed = ncols;
        //    bmi.biClrImportant = ncols;
        //    bmi.cols = new uint[256]; // The structure always has fixed size 256, even if we end up using fewer colours
        //    if (bpp == 1) { bmi.cols[0] = MAKERGB(0, 0, 0); bmi.cols[1] = MAKERGB(255, 255, 255); }
        //    else { for (int i = 0; i < ncols; i++) bmi.cols[i] = MAKERGB(i, i, i); }
        //    // For 8bpp we've created an palette with just greyscale colours.
        //    // You can set up any palette you want here. Here are some possibilities:
        //    // greyscale: for (int i=0; i<256; i++) bmi.cols[i]=MAKERGB(i,i,i);
        //    // rainbow: bmi.biClrUsed=216; bmi.biClrImportant=216; int[] colv=new int[6]{0,51,102,153,204,255};
        //    //          for (int i=0; i<216; i++) bmi.cols[i]=MAKERGB(colv[i/36],colv[(i/6)%6],colv[i%6]);
        //    // optimal: a difficult topic: http://en.wikipedia.org/wiki/Color_quantization
        //    // 
        //    // Now create the indexed bitmap "hbm0"
        //    IntPtr bits0; // not used for our purposes. It returns a pointer to the raw bits that make up the bitmap.
        //    IntPtr hbm0 = CreateDIBSection(IntPtr.Zero, ref bmi, DIB_RGB_COLORS, out bits0, IntPtr.Zero, 0);
        //    //
        //    // Step (3): use GDI's BitBlt function to copy from original hbitmap into monocrhome bitmap
        //    // GDI programming is kind of confusing... nb. The GDI equivalent of "Graphics" is called a "DC".
        //    IntPtr sdc = GetDC(IntPtr.Zero);       // First we obtain the DC for the screen
        //    // Next, create a DC for the original hbitmap
        //    IntPtr hdc = CreateCompatibleDC(sdc); SelectObject(hdc, hbm);
        //    // and create a DC for the monochrome hbitmap
        //    IntPtr hdc0 = CreateCompatibleDC(sdc); SelectObject(hdc0, hbm0);
        //    // Now we can do the BitBlt:
        //    BitBlt(hdc0, 0, 0, w, h, hdc, 0, 0, SRCCOPY);
        //    // Step (4): convert this monochrome hbitmap back into a Bitmap:
        //    System.Drawing.Bitmap b0 = System.Drawing.Bitmap.FromHbitmap(hbm0);
        //    //
        //    // Finally some cleanup.
        //    DeleteDC(hdc);
        //    DeleteDC(hdc0);
        //    ReleaseDC(IntPtr.Zero, sdc);
        //    DeleteObject(hbm);
        //    DeleteObject(hbm0);
        //    //
        //    return b0;
        //}
        ///// <summary>
        ///// Draws a bitmap onto the screen. Note: this will be overpainted
        ///// by other windows when they come to draw themselves. Only use it
        ///// if you want to draw something quickly and can't be bothered with forms.
        ///// </summary>
        ///// <param name="b">the bitmap to draw on the screen</param>
        ///// <param name="x">x screen coordinate</param>
        ///// <param name="y">y screen coordinate</param>
        ////-------------------------------------------------------------------------
        //private void SplashImage(Bitmap b, int x, int y)
        //{ // Drawing onto the screen is supported by GDI, but not by the Bitmap/Graphics class.
        //    // So we use interop:
        //    // (1) Copy the Bitmap into a GDI hbitmap
        //    IntPtr hbm = b.GetHbitmap();
        //    // (2) obtain the GDI equivalent of a "Graphics" for the screen
        //    IntPtr sdc = GetDC(IntPtr.Zero);
        //    // (3) obtain the GDI equivalent of a "Graphics" for the hbitmap
        //    IntPtr hdc = CreateCompatibleDC(sdc);
        //    SelectObject(hdc, hbm);
        //    // (4) Draw from the hbitmap's "Graphics" onto the screen's "Graphics"
        //    BitBlt(sdc, x, y, b.Width, b.Height, hdc, 0, 0, SRCCOPY);
        //    // and do boring GDI cleanup:
        //    DeleteDC(hdc);
        //    ReleaseDC(IntPtr.Zero, sdc);
        //    DeleteObject(hbm);
        //}
        //-------------------------------------------------------------------------
        public byte[] TiffImageBytes(ref Bitmap bitmap)
        {
            byte[] tiffBytes = GetTiffImageBytes(ref bitmap, true, 1);//false
            if (tiffBytes == null)
                return tiffBytes;

            TypeConverter tc = TypeDescriptor.GetConverter(typeof(Bitmap));
            bitmap = (Bitmap)tc.ConvertFrom(tiffBytes);//exc!!!
            bitmap.SetResolution(300, 300);
            Color p0 = bitmap.Palette.Entries[0];
            Color p1 = bitmap.Palette.Entries[1];

            if ((p0.R << 16 + p0.G << 8 + p0.B) < (p1.R << 16 + p1.G << 8 + p1.B))
            {
                // Swap the palette entries.
                bitmap.Palette.Entries[0] = p1;
                bitmap.Palette.Entries[1] = p0;
                ////Image<Bgr, Byte> img = new Image<Bgr, Byte>(bitmap);
                //////using (Image<Bgr, Byte> img = new Image<Bgr, Byte>(bitmap))
                //////{
                //////for (int i = 0; i < img.Width; i++)
                //////{
                //////    for (int j = 0; j < img.Height; j++)
                //////    {
                //////        Bgr bgr = img[new Point(i, j)];
                //////        Color c = Color.FromArgb((int)bgr.Red, (int)bgr.Green, (int)bgr.Blue);
                //////        img[new Point(i, j)] = new Bgr(Color.FromArgb(255 - c.R, 255 - c.G, 255 - c.B));
                //////    }
                //////}
                ////img = img.Not();
                ////bitmap = (Bitmap)img.ToBitmap().Clone();//img.ToBitmap();//
                ////img.Dispose();
                //////img.Save("Invert.bmp");
                //////}
                //////TimeSpan ts = DateTime.Now - dt;
                ////tiffBytes = GetTiffImageBytes(ref bitmap, false);

                Bitmap copy = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb);
                ImageAttributes ia = new ImageAttributes();
                ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                                        {
                                            new float[] {-1, 0, 0, 0, 0},
                                            new float[] {0, -1, 0, 0, 0},
                                            new float[] {0, 0, -1, 0, 0},
                                            new float[] {0, 0, 0, 1, 0},
                                            new float[] {1, 1, 1, 0, 1}
                                        });
                ia.SetColorMatrix(colorMatrix);
                Graphics g = Graphics.FromImage(copy);//недостаточно памяти\/
                g.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height), 0, 0, bitmap.Width, bitmap.Height
                    , GraphicsUnit.Pixel, ia);
                g.Dispose();
                //tiffBytes = GetTiffImageBytes(copy, true, 1);//с ", 1" инверсия
                tiffBytes = GetTiffImageBytes(ref copy, true);//bitmap
                //copy.Save("tiffBytes.bmp", ImageFormat.Bmp);
                copy.Dispose(); ia.Dispose();
            }
            return tiffBytes;
        }
        //-------------------------------------------------------------------------
        private byte[] GetTiffImageBytes(ref Bitmap img, bool byScanlines, int photometric = 0)
        {
            try
            {
                byte[] raster = GetImageRasterBytes(img);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (Tiff tif = Tiff.ClientOpen("InMemory", "w", ms, new TiffStream()))
                    {
                        if (tif == null)
                            return null;

                        tif.SetField(TiffTag.IMAGEWIDTH, img.Width);
                        tif.SetField(TiffTag.IMAGELENGTH, img.Height);
                        tif.SetField(TiffTag.COMPRESSION, Compression.CCITTFAX4);//NONE
                        tif.SetField(TiffTag.PHOTOMETRIC, photometric);// Photometric.MINISBLACK

                        tif.SetField(TiffTag.ROWSPERSTRIP, img.Height);

                        tif.SetField(TiffTag.XRESOLUTION, 300);//img.HorizontalResolution
                        tif.SetField(TiffTag.YRESOLUTION, 300);//img.VerticalResolution

                        tif.SetField(TiffTag.SUBFILETYPE, 0);
                        tif.SetField(TiffTag.BITSPERSAMPLE, 1);
                        tif.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);
                        tif.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);

                        tif.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                        tif.SetField(TiffTag.T6OPTIONS, 0);
                        tif.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.INCH);

                        tif.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);

                        int tiffStride = tif.ScanlineSize();
                        int stride = raster.Length / img.Height;

                        if (byScanlines)
                        {
                            // raster stride MAY be bigger than TIFF stride (due to padding in raster bits)
                            for (int i = 0, offset = 0; i < img.Height; i++)
                            {
                                bool res = tif.WriteScanline(raster, offset, i, 0);
                                if (!res)
                                    return null;

                                offset += stride;
                            }
                        }
                        else
                        {
                            if (tiffStride < stride)
                            {
                                // raster stride is bigger than TIFF stride
                                // this is due to padding in raster bits
                                // we need to create correct TIFF strip and write it into TIFF

                                byte[] stripBits = new byte[tiffStride * img.Height];
                                for (int i = 0, rasterPos = 0, stripPos = 0; i < img.Height; i++)
                                {
                                    System.Buffer.BlockCopy(raster, rasterPos, stripBits, stripPos, tiffStride);
                                    rasterPos += stride;
                                    stripPos += tiffStride;
                                }

                                // Write the information to the file
                                int n = tif.WriteEncodedStrip(0, stripBits, stripBits.Length);
                                if (n <= 0)
                                    return null;
                            }
                            else
                            {
                                // Write the information to the file
                                int n = tif.WriteEncodedStrip(0, raster, raster.Length);
                                if (n <= 0)
                                    return null;
                            }
                        }
                    }

                    return ms.GetBuffer();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        //-------------------------------------------------------------------------
        private byte[] GetImageRasterBytes(Bitmap img)
        {
            // Specify full image
            Rectangle rect = new Rectangle(0, 0, img.Width, img.Height);

            //Bitmap bmp = img;
            byte[] bits = null;

            //try
            //{
            // Lock the managed memory
            //if (img.PixelFormat.ToString().EndsWith("Indexed"))
            //    img = (Bitmap)img.Clone(new Rectangle(0, 0, img.Width, img.Height), PixelFormat.Format1bppIndexed);
            // bmp = convertToBitonal(img);

            BitmapData bmpdata = img.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format1bppIndexed);

            // Declare an array to hold the bytes of the bitmap.
            bits = new byte[bmpdata.Stride * bmpdata.Height];

            // Copy the sample values into the array.
            Marshal.Copy(bmpdata.Scan0, bits, 0, bits.Length);

            // Release managed memory
            img.UnlockBits(bmpdata);
            //}
            //finally
            //{
            //    img.Dispose();
            //    //if (bmp != img)
            //    //    bmp.Dispose();
            //}

            return bits;
        }
        //-------------------------------------------------------------------------
        private Bitmap convertToBitonal(Bitmap original)
        {
            int sourceStride;
            byte[] sourceBuffer = extractBytes(original, out sourceStride);

            // Create destination bitmap
            Bitmap destination = new Bitmap(original.Width, original.Height,
                PixelFormat.Format1bppIndexed);

            destination.SetResolution(original.HorizontalResolution, original.VerticalResolution);

            // Lock destination bitmap in memory
            BitmapData destinationData = destination.LockBits(
                new Rectangle(0, 0, destination.Width, destination.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);

            // Create buffer for destination bitmap bits
            int imageSize = destinationData.Stride * destinationData.Height;
            byte[] destinationBuffer = new byte[imageSize];

            int sourceIndex = 0;
            int destinationIndex = 0;
            int pixelTotal = 0;
            byte destinationValue = 0;
            int pixelValue = 128;
            int height = destination.Height;
            int width = destination.Width;
            int threshold = 500;

            for (int y = 0; y < height; y++)
            {
                sourceIndex = y * sourceStride;
                destinationIndex = y * destinationData.Stride;
                destinationValue = 0;
                pixelValue = 128;

                for (int x = 0; x < width; x++)
                {
                    // Compute pixel brightness (i.e. total of Red, Green, and Blue values)
                    pixelTotal = sourceBuffer[sourceIndex + 1] + sourceBuffer[sourceIndex + 2] +
                        sourceBuffer[sourceIndex + 3];

                    if (pixelTotal > threshold)
                        destinationValue += (byte)pixelValue;

                    if (pixelValue == 1)
                    {
                        destinationBuffer[destinationIndex] = destinationValue;
                        destinationIndex++;
                        destinationValue = 0;
                        pixelValue = 128;
                    }
                    else
                    {
                        pixelValue >>= 1;
                    }

                    sourceIndex += 4;
                }

                if (pixelValue != 128)
                    destinationBuffer[destinationIndex] = destinationValue;
            }

            Marshal.Copy(destinationBuffer, 0, destinationData.Scan0, imageSize);
            destination.UnlockBits(destinationData);
            return destination;
        }
        //-------------------------------------------------------------------------
        private byte[] extractBytes(Bitmap original, out int stride)
        {
            Bitmap source = null;

            try
            {
                // If original bitmap is not already in 32 BPP, ARGB format, then convert
                if (original.PixelFormat != PixelFormat.Format32bppArgb)
                {
                    source = new Bitmap(original.Width, original.Height, PixelFormat.Format32bppArgb);
                    source.SetResolution(original.HorizontalResolution, original.VerticalResolution);
                    using (Graphics g = Graphics.FromImage(source))
                    {
                        g.DrawImageUnscaled(original, 0, 0);
                    }
                }
                else
                {
                    source = original;
                }

                // Lock source bitmap in memory
                BitmapData sourceData = source.LockBits(
                    new Rectangle(0, 0, source.Width, source.Height),
                    ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                // Copy image data to binary array
                int imageSize = sourceData.Stride * sourceData.Height;
                byte[] sourceBuffer = new byte[imageSize];
                Marshal.Copy(sourceData.Scan0, sourceBuffer, 0, imageSize);

                // Unlock source bitmap
                source.UnlockBits(sourceData);

                stride = sourceData.Stride;
                return sourceBuffer;
            }
            finally
            {
                if (source != original)
                    source.Dispose();
            }

        }
        #endregion
        //-------------------------------------------------------------------------
    }
}