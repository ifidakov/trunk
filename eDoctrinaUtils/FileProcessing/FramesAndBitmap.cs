using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections.Generic;
using iTextSharp.text.pdf.parser;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;

namespace eDoctrinaUtils
{
    /// <summary>
    /// Helper class to extract images from a PDF file. Works with the most 
    /// common image types embedded in PDF files, as far as I can tell.
    /// </summary>
    /// <example>
    /// Usage example:
    /// <code>
    /// foreach (string filename in Directory.GetFiles(searchPath, "*.pdf", SearchOption.TopDirectoryOnly))
    /// {
    ///     Dictionary<string, System.Drawing.Image> images = ImageExtractor.ExtractImages(filename);
    ///     string directory = Path.GetDirectoryName(filename);
    /// 
    ///     foreach (string name in images.Keys)
    ///     {
    ///         images[name].Save(Path.Combine(directory, name));
    ///     }
    /// }
    /// </code>
    /// </example>
    public class ImageExtractor
    {
        #region Methods

        #region Public Methods

        /// <summary>
        /// Checks whether a specified page of a PDF file contains images.
        /// </summary>
        /// <returns>True if the page contains at least one image; false otherwise.</returns>
        public bool PageContainsImages(string filename, int pageNumber)
        {
            PdfReader reader = new PdfReader(filename);
            PdfReaderContentParser parser = new PdfReaderContentParser(reader);
            ImageRenderListener listener = null;
            parser.ProcessContent(pageNumber, (listener = new ImageRenderListener()));
            return listener.Images.Count > 0;
        }

        /// <summary>
        /// Extracts all images (of types that iTextSharp knows how to decode) from a PDF file.
        /// </summary>
        /// <returns>Returns a generic <see cref="Dictionary<string, System.Drawing.Image>", 
        /// where the key is a suggested file name, in the format: PDF filename without extension, 
        /// page number and image index in the page./></returns>
        public Dictionary<string, System.Drawing.Image> ExtractImages(string filename)
        {
            Dictionary<string, System.Drawing.Image> images = new Dictionary<string, System.Drawing.Image>();
            PdfReader reader = new PdfReader(filename);
            PdfReaderContentParser parser = new PdfReaderContentParser(reader);
            ImageRenderListener listener = null;

            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                parser.ProcessContent(i, (listener = new ImageRenderListener()));
                int index = 1;

                if (listener.Images.Count > 0)
                {
                    Console.WriteLine("Found {0} images on page {1}.", listener.Images.Count, i);

                    foreach (KeyValuePair<System.Drawing.Image, string> pair in listener.Images)
                    {
                        images.Add(string.Format("{0}_Page_{1}_Image_{2}{3                           Path.GetFileNameWithoutExtension(filename)", i.ToString("D4"), index.ToString("D4"), pair.Value), pair.Key);
                        index++;
                    }
                }
            }
            return images;
        }

        /// <summary>
        /// Extracts all images (of types that iTextSharp knows how to decode) 
        /// from a specified page of a PDF file.
        /// </summary>
        /// <returns>Returns a generic <see cref="Dictionary<string, System.Drawing.Image>", 
        /// where the key is a suggested file name, in the format: PDF filename without extension, 
        /// page number and image index in the page./></returns>
        public Dictionary<string, System.Drawing.Image> ExtractImages(string filename, int pageNumber)
        {
            Dictionary<string, System.Drawing.Image> images = new Dictionary<string, System.Drawing.Image>();
            PdfReader reader = new PdfReader(filename);
            PdfReaderContentParser parser = new PdfReaderContentParser(reader);
            ImageRenderListener listener = null;

            parser.ProcessContent(pageNumber, (listener = new ImageRenderListener()));
            int index = 1;

            if (listener.Images.Count > 0)
            {
                //Console.WriteLine("Found {0} images on page {1}.", listener.Images.Count, pageNumber);

                foreach (KeyValuePair<System.Drawing.Image, string> pair in listener.Images)
                {
                    images.Add(string.Format("{0}_Page_{1}_Image_{2}{3                       Path.GetFileNameWithoutExtension(filename)", pageNumber.ToString("D4"), index.ToString("D4"), pair.Value), pair.Key);
                    index++;
                }
            }
            return images;
        }

        #endregion Public Methods

        #endregion Methods
    }

    internal class ImageRenderListener : IRenderListener
    {
        #region Fields

        Dictionary<System.Drawing.Image, string> images = new Dictionary<System.Drawing.Image, string>();

        #endregion Fields

        #region Properties

        public Dictionary<System.Drawing.Image, string> Images
        {
            get { return images; }
        }

        #endregion Properties

        #region Methods

        #region Public Methods

        public void BeginTextBlock() { }

        public void EndTextBlock() { }

        public void RenderImage(ImageRenderInfo renderInfo)
        {
            PdfImageObject image = renderInfo.GetImage();
            PdfName filter = (PdfName)image.Get(PdfName.FILTER);

            //int width = Convert.ToInt32(image.Get(PdfName.WIDTH).ToString());
            //int bitsPerComponent = Convert.ToInt32(image.Get(PdfName.BITSPERCOMPONENT).ToString());
            //string subtype = image.Get(PdfName.SUBTYPE).ToString();
            //int height = Convert.ToInt32(image.Get(PdfName.HEIGHT).ToString());
            //int length = Convert.ToInt32(image.Get(PdfName.LENGTH).ToString());
            //string colorSpace = image.Get(PdfName.COLORSPACE).ToString();

            /* It appears to be safe to assume that when filter == null, PdfImageObject 
             * does not know how to decode the image to a System.Drawing.Image.
             * 
             * Uncomment the code above to verify, but when I've seen this happen, 
             * width, height and bits per component all equal zero as well. */
            if (filter != null)
            {
                System.Drawing.Image drawingImage = image.GetDrawingImage();

                string extension = ".";

                if (filter == PdfName.DCTDECODE)
                {
                    extension += PdfImageObject.ImageBytesType.JPG;
                }
                else if (filter == PdfName.JPXDECODE)
                {
                    extension += PdfImageObject.ImageBytesType.JP2;
                }
                else if (filter == PdfName.FLATEDECODE)
                {
                    extension += PdfImageObject.ImageBytesType.PNG;
                }
                //else if (filter == PdfName.LZWDECODE)
                //{
                //    extension += PdfImageObject.ImageBytesType.TIF;
                //}

                /* Rather than struggle with the image stream and try to figure out how to handle 
                 * BitMapData scan lines in various formats (like virtually every sample I've found 
                 * online), use the PdfImageObject.GetDrawingImage() method, which does the work for us. */
                this.Images.Add(drawingImage, extension);
            }
        }

        public void RenderText(TextRenderInfo renderInfo) { }

        #endregion Public Methods

        #endregion Methods
    }
    //-------------------------------------------------------------------------
    public class FramesAndBitmap
    {
        private Exception exception;
        public Exception Exception
        {
            get { return exception; }
            set { exception = value; }
        }
        private int pageCount;
        public int PageCount
        {
            get { return pageCount; }
            private set { pageCount = value; }
        }
        Utils utils = new Utils();
        IOHelper iOHelper = new IOHelper();
        Log log = new Log();

        //-------------------------------------------------------------------------
        #region GetFrames
        public void GetFrames(string fileName, Audit sourceAudit)
        {
            exception = null;
            if (IsPDFFile(fileName))
            {
                GetFramesFromPDF(fileName, sourceAudit, out exception);
            }
            else
            {
                GetFramesFromPic(fileName, sourceAudit, out exception);
            }
        }
        //-------------------------------------------------------------------------
        private Bitmap RenderImage(ImageRenderInfo renderInfo)
        {
            PdfImageObject image = renderInfo.GetImage();
            using (System.Drawing.Image dotnetImg = image.GetDrawingImage())
            {
                if (dotnetImg != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        dotnetImg.Save(ms, ImageFormat.Tiff);
                        Bitmap d = new Bitmap(dotnetImg);
                        return d;//d.Save(imgPath);
                    }
                }
            }
            return null;
        }
        //-------------------------------------------------------------------------
        private bool ExtractImages(String PDFSourcePath, Audit sourceAudit, out Exception exception)
        {
            //List<System.Drawing.Image> ImgList = new List<System.Drawing.Image>();

            exception = null;
            iTextSharp.text.pdf.RandomAccessFileOrArray RAFObj = null;
            iTextSharp.text.pdf.PdfReader PDFReaderObj = null;
            iTextSharp.text.pdf.PdfObject PDFObj = null;
            iTextSharp.text.pdf.PdfStream PDFStremObj = null;
            int pageNumber = 0;
            bool ok = false;
            try
            {
                RAFObj = new iTextSharp.text.pdf.RandomAccessFileOrArray(PDFSourcePath);
                PDFReaderObj = new iTextSharp.text.pdf.PdfReader(RAFObj, null);
                for (int i = 0; i < PDFReaderObj.XrefSize; i++)
                {
                    PDFObj = PDFReaderObj.GetPdfObject(i);

                    if ((PDFObj != null) && PDFObj.IsStream())
                    {
                        PDFStremObj = (iTextSharp.text.pdf.PdfStream)PDFObj;
                        iTextSharp.text.pdf.PdfObject subtype = PDFStremObj.Get(iTextSharp.text.pdf.PdfName.SUBTYPE);

                        if ((subtype != null) && subtype.ToString() == iTextSharp.text.pdf.PdfName.IMAGE.ToString())
                        {
                            try
                            {
                                iTextSharp.text.pdf.parser.PdfImageObject PdfImageObj =
                         new iTextSharp.text.pdf.parser.PdfImageObject((iTextSharp.text.pdf.PRStream)PDFStremObj);

                                System.Drawing.Image ImgPDF = PdfImageObj.GetDrawingImage();
                                pageNumber++;
                                if (pageNumber > PDFReaderObj.NumberOfPages)
                                    return false;
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
                pageNumber = 0;
                PageCount = PDFReaderObj.NumberOfPages;
                for (int i = 0; i < PDFReaderObj.XrefSize; i++)
                {
                    PDFObj = PDFReaderObj.GetPdfObject(i);

                    if ((PDFObj != null) && PDFObj.IsStream())
                    {
                        PDFStremObj = (iTextSharp.text.pdf.PdfStream)PDFObj;
                        iTextSharp.text.pdf.PdfObject subtype = PDFStremObj.Get(iTextSharp.text.pdf.PdfName.SUBTYPE);

                        if ((subtype != null) && subtype.ToString() == iTextSharp.text.pdf.PdfName.IMAGE.ToString())
                        {
                            try
                            {
                                iTextSharp.text.pdf.parser.PdfImageObject PdfImageObj =
                         new iTextSharp.text.pdf.parser.PdfImageObject((iTextSharp.text.pdf.PRStream)PDFStremObj);

                                System.Drawing.Image ImgPDF = PdfImageObj.GetDrawingImage();
                                pageNumber++;
                                //using (Bitmap bmp = new Bitmap(ImgPDF))
                                //{
                                //    SaveFrameFiles(PDFSourcePath, sourceAudit, bmp, pageNumber, PageCount);
                                try
                                {
                                    SaveFrameFiles(PDFSourcePath, sourceAudit, (Bitmap)ImgPDF, pageNumber, PageCount);
                                }
                                catch (Exception)
                                {
                                    ImgPDF.Dispose();
                                    continue;
                                }
                                ImgPDF.Dispose();
                                ok = true;
                                //}
                            }
                            catch (Exception)
                            { }
                        }
                    }
                }
                return ok;
            }
            catch (Exception)// ex
            {
                //log.LogMessage(PDFSourcePath + " Page" + pageNumber.ToString() + " ", ex);
                return false;
            }
            finally
            {
                PDFReaderObj.Close();
                PDFReaderObj.Dispose();
            }
        }
        //-------------------------------------------------------------------------
        private void GetFramesFromPDF(string fileName, Audit sourceAudit, out Exception exception)
        {
            PdfReader doc = null;
            PDFLibNet.PDFWrapper pdfDoc = null;

            int pageNumber = 0;
            exception = null;
            //ImageExtractor 
            try
            {
                if (ExtractImages(fileName, sourceAudit, out exception))
                    return;
                ////for (int i = 0; i < list.Count; i++)
                ////{
                ////    var item = list[i];
                ////    using (Bitmap bmp = new Bitmap(item))
                ////    {
                ////        SaveFrameFiles(fileName, sourceAudit, bmp, i + 1, list.Count);
                ////    }
                ////}
                ////list.Clear();
                //doc = new PdfReader(fileName);
                //for (pageNumber = 1; pageNumber <= doc.NumberOfPages; pageNumber++)
                //{
                //    //using (Bitmap bmp = GetBitmapFromPDFPage(doc, pageNumber))
                //    //{
                //    //    SaveFrameFiles(fileName, sourceAudit, bmp, pageNumber, doc.NumberOfPages);
                //    //}
                //    PdfDictionary dict = doc.GetPageN(pageNumber);
                //    PdfDictionary res = (PdfDictionary)(PdfReader.GetPdfObject(dict.Get(PdfName.RESOURCES)));
                //    PdfDictionary xobj = (PdfDictionary)(PdfReader.GetPdfObject(res.Get(PdfName.XOBJECT)));

                //    if (xobj != null)
                //    {
                //        foreach (PdfName name in xobj.Keys)
                //        {
                //            PdfObject obj = xobj.Get(name);
                //            if (obj.IsIndirect())
                //            {
                //                PdfDictionary tg = (PdfDictionary)(PdfReader.GetPdfObject(obj));
                //                PdfName subtype = (PdfName)(PdfReader.GetPdfObject(tg.Get(PdfName.SUBTYPE)));
                //                if (PdfName.IMAGE.Equals(subtype))
                //                {
                //                    int xrefIdx = ((PRIndirectReference)obj).Number;
                //                    PdfObject pdfObj = doc.GetPdfObject(xrefIdx);
                //                    PdfStream str = (PdfStream)(pdfObj);

                //                    iTextSharp.text.pdf.parser.PdfImageObject pdfImage =
                //                        new iTextSharp.text.pdf.parser.PdfImageObject((PRStream)str);
                //                    System.Drawing.Image ImgPDF = pdfImage.GetDrawingImage();

                //                    using (Bitmap bmp = new Bitmap(ImgPDF))
                //                    {
                //                        SaveFrameFiles(fileName, sourceAudit, bmp, pageNumber, doc.NumberOfPages);
                //                    }
                //                }
                //                else if (PdfName.FORM.Equals(subtype) || PdfName.GROUP.Equals(subtype))
                //                {
                //                    //images.AddRange(GetImagesFromPdfDict(tg, doc));
                //                }
                //            }
                //        }
                //    }
                //}
                try
                {
                    pdfDoc = GetPDFDoc(fileName);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    return;
                }
                int i = 0;
                if (pdfDoc != null)
                {
                    PageCount = pdfDoc.PageCount;
                    for (i = 0; i < PageCount; i++)
                    {
                        var bmp = GetBitmapFromPDFPage(pdfDoc, i + 1);
                        SaveFrameFiles(fileName, sourceAudit, bmp, i + 1, PageCount);
                        bmp.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogMessage(fileName + " Page" + pageNumber.ToString() + " ", ex);
            }
            finally
            {
                if (doc != null)
                {
                    doc.Close();
                    doc.Dispose();
                }
                if (pdfDoc != null)
                {
                    pdfDoc.Dispose();
                }
            }
        }
        //-------------------------------------------------------------------------
        //private void GetFramesFromPDF(string fileName, Audit sourceAudit, out Exception exception)
        //{
        //    exception = null;
        //    PDFLibNet.PDFWrapper pdfDoc = null;
        //    try
        //    {
        //        pdfDoc = GetPDFDoc(fileName);
        //    }
        //    catch (Exception ex)
        //    {
        //        exception = ex;
        //        return;
        //    }
        //    int i = 0;
        //    if (pdfDoc != null)
        //    {
        //        try
        //        {
        //            int fc = pdfDoc.PageCount;
        //            for (i = 0; i < fc; i++)
        //            {
        //                var bmp = GetBitmapFromPDFPage(pdfDoc, i + 1);
        //                SaveFrameFiles(fileName, sourceAudit, bmp, i + 1, fc);
        //                bmp.Dispose();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.LogMessage(fileName + " Page" + (i + 1).ToString() + " ", ex);
        //        }
        //    }
        //    pdfDoc.Dispose();
        //    pdfDoc = null;
        //}
        bool workGetFramesFromPic = false;
        //-------------------------------------------------------------------------
        private void GetFramesFromPic(string fileName, Audit sourceAudit, out Exception exception)
        {
            exception = null;
            if (workGetFramesFromPic)
                return;
            workGetFramesFromPic = true;
            Bitmap entryBitmap;
            try
            {
                entryBitmap = GetPicFile(fileName);
            }
            catch (Exception ex)
            {
                exception = ex;
                workGetFramesFromPic = false;
                return;
            }
            int i = 0;
            try
            {
                Guid[] guids = entryBitmap.FrameDimensionsList;
                FrameDimension fd = new FrameDimension(guids[0]);
                PageCount = entryBitmap.GetFrameCount(fd);
                for (i = 0; i < PageCount; i++)
                {
                    entryBitmap.SelectActiveFrame(fd, i);
                    //SaveFrameFiles(fileName, sourceAudit, entryBitmap, i + 1, PageCount);
                    var tempdir = OcrAppConfig.TempFolder + "SaveFrame\\";
                    iOHelper.CreateDirectory(tempdir);
                    string[] ss = Directory.GetFiles(tempdir);
                    for (int j = 0; j < ss.Length; j++)
                    {
                        try
                        {
                            iOHelper.DeleteFile(ss[j]);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    var temp = tempdir + sourceAudit.sourceSHA1Hash + "_" + i + 1 + ".tiff";
                    //iOHelper.DeleteFile(temp);
                    //Bitmap bmp = (Bitmap)entryBitmap.Clone();// new Bitmap(entryBitmap.Width, entryBitmap.Height, PixelFormat.Format24bppRgb);
                    //bmp = (Bitmap)bmp.GetThumbnailImage(bmp.Width, bmp.Height, null, IntPtr.Zero);
                    //bmp.SetResolution(entryBitmap.VerticalResolution, entryBitmap.HorizontalResolution);

                    //bmp.Save("SaveFrameFiles.bmp", ImageFormat.Bmp);
                    entryBitmap.Save(temp, ImageFormat.Tiff);
                    //bmp.Save(temp, ImageFormat.Tiff);
                    //bmp.Dispose();
                    var sha1Hash = utils.GetSHA1FromFile(temp);
                    //проверить на ""!!!
                    var tempFileName = OcrAppConfig.TempFramesFolder + sha1Hash + ".tiff";
                    var tempFileNameAudit = OcrAppConfig.TempFramesFolder + sha1Hash + ".audit";
                    log.LogMessage("Get frame page " + (i + 1).ToString() + " of " + pageCount.ToString() + " from " + sourceAudit.sourceFileName);
                    if (File.Exists(tempFileName) && File.Exists(tempFileNameAudit))
                    {
                        log.LogMessage("Frame exists page " + (i + 1).ToString() + " of " + pageCount.ToString() + " from " + sourceAudit.sourceFileName);
                        try
                        {
                            iOHelper.DeleteFile(temp);
                        }
                        catch (Exception)
                        {
                        }
                        //iOHelper.DeleteDirectory(tempdir, false);
                        //entryBitmap.Dispose();
                        //return;
                        continue;
                    }
                    var audit = sourceAudit.GetFrameAudit(tempFileName, sha1Hash, i + 1);
                    audit.Save(tempFileNameAudit);
                    //File.Move(temp, tempFileName);
                    File.Copy(temp, tempFileName, true);
                    //System.Windows.Forms.Application.DoEvents();
                    //try
                    //{
                    //    iOHelper.DeleteFile(temp);
                    //}
                    //catch (Exception ex)
                    //{
                    //    log.LogMessage(fileName + " Page" + (i + 1).ToString() + " (for) ", ex);
                    //    continue;
                    //}
                    
                    //iOHelper.DeleteDirectory(tempdir, false);

                    //entryBitmap.Dispose();//недопустимый параметр

                }
                entryBitmap.Dispose();
                workGetFramesFromPic = false;
            }
            catch (Exception ex)
            {
                workGetFramesFromPic = false;
                entryBitmap.Dispose();
                log.LogMessage(fileName + " Page" + (i + 1).ToString() + " ", ex);
            }
        }
        //-------------------------------------------------------------------------
        private void SaveFrameFiles(string fileName, Audit sourceAudit, Bitmap entryBitmap, int pageNumber, int pageCount)
        {
            if (workGetFramesFromPic)
                return;
            workGetFramesFromPic = true;
            try
            {
                var tempdir = OcrAppConfig.TempFolder + "SaveFrame\\";
                iOHelper.CreateDirectory(tempdir);
                var temp = tempdir + sourceAudit.sourceSHA1Hash + "_" + pageNumber + ".tiff";
                //iOHelper.DeleteFile(temp);

                string[] ss = Directory.GetFiles(tempdir);
                for (int j = 0; j < ss.Length; j++)
                {
                    try
                    {
                        iOHelper.DeleteFile(ss[j]);
                    }
                    catch (Exception)
                    {
                    }
                }



                //Bitmap bmp = (Bitmap)entryBitmap.Clone();// new Bitmap(entryBitmap.Width, entryBitmap.Height, PixelFormat.Format24bppRgb);
                //bmp = (Bitmap)bmp.GetThumbnailImage(bmp.Width, bmp.Height, null, IntPtr.Zero);
                //bmp.SetResolution(entryBitmap.VerticalResolution, entryBitmap.HorizontalResolution);

                //bmp.Save("SaveFrameFiles.bmp", ImageFormat.Bmp);
                entryBitmap.Save(temp, ImageFormat.Tiff);
                //bmp.Save(temp, ImageFormat.Tiff);
                //bmp.Dispose();
                var sha1Hash = utils.GetSHA1FromFile(temp);

                var tempFileName = OcrAppConfig.TempFramesFolder + sha1Hash + ".tiff";
                var tempFileNameAudit = OcrAppConfig.TempFramesFolder + sha1Hash + ".audit";
                if (File.Exists(tempFileName) && File.Exists(tempFileNameAudit))
                {
                    log.LogMessage("Frame exists page " + pageNumber.ToString() + " of " + pageCount.ToString() + " from " + sourceAudit.sourceFileName);
                    iOHelper.DeleteFile(temp);
                    //iOHelper.DeleteDirectory(tempdir, false);
                    workGetFramesFromPic = false;
                    return;
                }
                var audit = sourceAudit.GetFrameAudit(tempFileName, sha1Hash, pageNumber);
                audit.Save(tempFileNameAudit);
                log.LogMessage("Get frame page " + pageNumber.ToString() + " of " + pageCount.ToString() + " from " + sourceAudit.sourceFileName);
                //File.Move(temp, tempFileName);
                File.Copy(temp, tempFileName, true);
                //System.Windows.Forms.Application.DoEvents();
                //iOHelper.DeleteFile(temp);
                
                //iOHelper.DeleteDirectory(tempdir, false);

                //entryBitmap.Dispose();//недопустимый параметр
                workGetFramesFromPic = false;
            }
            catch (Exception)
            {
                workGetFramesFromPic = false;
            }
        }
        #endregion
        //-------------------------------------------------------------------------
        #region GetBitmap
        public Bitmap GetBitmapFromFile(string fileName)
        {
            Exception = null;
            try
            {
                if (IsPDFFile(fileName))
                {
                    return GetBitmapFromPDFFile(fileName, out exception);
                }
                else
                {
                    return GetBitmapFromPicFile(fileName, out exception);
                }
            }
            catch (Exception ex)
            {
                Exception = ex;
                return null;
            }
        }
        //-------------------------------------------------------------------------
        private Bitmap GetBitmapFromPicFile(string fileName, out Exception exception)
        {
            exception = null;
            var bmp = GetPicFile(fileName);
            var guids = bmp.FrameDimensionsList;
            var fd = new FrameDimension(guids[0]);
            if (bmp.GetFrameCount(fd) > 1)
            {//You are trying to open a multipage file that can not be correctly processed.
                exception = new Exception("An attempt to open a multipage file that can not be correctly processed.");
                bmp.Dispose();
                bmp = null;
            }
            else
            {
                bmp.SelectActiveFrame(fd, 0);
            }
            return bmp;
        }
        //-------------------------------------------------------------------------
        private Bitmap GetPicFile(string fileName)
        {
            return new Bitmap(fileName);
        }
        //-------------------------------------------------------------------------
        //private List<System.Drawing.Image> test(PdfDictionary tg, PdfReader doc)//,RandomAccessFileOrArray(data)
        //{

        //    List<System.Drawing.Image> ImgList = new List<System.Drawing.Image>();

        //    iTextSharp.text.pdf.RandomAccessFileOrArray RAFObj = null;
        //    iTextSharp.text.pdf.PdfReader PDFReaderObj = null;
        //    iTextSharp.text.pdf.PdfObject PDFObj = null;
        //    iTextSharp.text.pdf.PdfStream PDFStremObj = null;

        //    try
        //    {
        //        RAFObj = new iTextSharp.text.pdf.RandomAccessFileOrArray(data);
        //        PDFReaderObj = new iTextSharp.text.pdf.PdfReader(RAFObj, null);

        //        for (int i = 0; i <= PDFReaderObj.XrefSize - 1; i++)
        //        {
        //            PDFObj = PDFReaderObj.GetPdfObject(i);

        //            if ((PDFObj != null) && PDFObj.IsStream())
        //            {
        //                PDFStremObj = (iTextSharp.text.pdf.PdfStream)PDFObj;
        //                iTextSharp.text.pdf.PdfObject subtype = PDFStremObj.Get(iTextSharp.text.pdf.PdfName.SUBTYPE);

        //                if ((subtype != null) && subtype.ToString() == iTextSharp.text.pdf.PdfName.IMAGE.ToString())
        //                {
        //                    byte[] bytes = iTextSharp.text.pdf.PdfReader.GetStreamBytesRaw((iTextSharp.text.pdf.PRStream)PDFStremObj);

        //                    if ((bytes != null))
        //                    {
        //                        try
        //                        {
        //                            System.IO.MemoryStream MS = new System.IO.MemoryStream(bytes);

        //                            MS.Position = 0;
        //                            System.Drawing.Image ImgPDF = System.Drawing.Image.FromStream(MS);

        //                            ImgList.Add(ImgPDF);

        //                        }
        //                        catch (Exception)
        //                        {
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        PDFReaderObj.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}
        //-------------------------------------------------------------------------
        //private Bitmap GetBitmapFromPDFPage(PdfReader doc, int pageNumber, PdfDictionary dict = null)
        //{

        //    if (dict == null)
        //    {
        //        dict = doc.GetPageN(pageNumber);
        //    }
        //    PdfDictionary res = (PdfDictionary)(PdfReader.GetPdfObject(dict.Get(PdfName.RESOURCES)));
        //    PdfDictionary xobj = (PdfDictionary)(PdfReader.GetPdfObject(res.Get(PdfName.XOBJECT)));

        //    Bitmap b = NewMethod(doc, xobj);
        //    return b;
        //}

        //private Bitmap NewMethod(PdfReader doc, PdfDictionary xobj)
        //{
        //    if (xobj != null)
        //    {
        //        foreach (PdfName name in xobj.Keys)
        //        {
        //            PdfObject obj = xobj.Get(name);
        //            if (obj.IsIndirect())
        //            {
        //                PdfDictionary tg = (PdfDictionary)(PdfReader.GetPdfObject(obj));
        //                PdfName subtype = (PdfName)(PdfReader.GetPdfObject(tg.Get(PdfName.SUBTYPE)));
        //                if (PdfName.IMAGE.Equals(subtype))
        //                {
        //                    var index = Convert.ToInt32(((PRIndirectReference)obj).Number.ToString(System.Globalization.CultureInfo.InvariantCulture)); //retrieve the index reference to the stream object
        //                    var pdfObject = doc.GetPdfObject(index); //retrieve the stream object
        //                    var pdfStream = (PdfStream)pdfObject; //cast the object as string
        //                    var imageBytes = PdfReader.GetStreamBytesRaw((PRStream)pdfStream);
        //                    var decodedBytes = PdfReader.FlateDecode(imageBytes); //decode the raw image
        //                    var streamBytes = PdfReader.DecodePredictor(decodedBytes, pdfStream.GetAsDict(PdfName.DECODEPARMS)); //decode predict to filter the bytes
        //                    var width = tg.GetAsNumber(PdfName.WIDTH).IntValue; //retrieve the width
        //                    var height = tg.GetAsNumber(PdfName.HEIGHT).IntValue; //retrieve the height
        //                    var bitsPerComponent = tg.GetAsNumber(PdfName.BITSPERCOMPONENT).IntValue; //retrieve the BPC
        //                    var pixelFormat = PixelFormat.Format1bppIndexed;
        //                    switch (bitsPerComponent) //determine the BPC
        //                    {
        //                        case 1:
        //                            pixelFormat = PixelFormat.Format1bppIndexed;
        //                            break;
        //                        case 8:
        //                            pixelFormat = PixelFormat.Format8bppIndexed;
        //                            break;
        //                        case 24:
        //                            pixelFormat = PixelFormat.Format24bppRgb;
        //                            break;
        //                    }

        //                    //var bmpFile = "c:\temp\extractedimage.bmp";

        //                    using (var bmp = new Bitmap(width, height, pixelFormat))
        //                    {
        //                        var bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, pixelFormat);
        //                        var length = (int)Math.Ceiling(width * bitsPerComponent / 8.0);
        //                        for (int i = 0; i < height; i++)
        //                        {
        //                            int offset = i * length;
        //                            int scanOffset = i * bmpData.Stride;
        //                            Marshal.Copy(streamBytes, offset, new IntPtr(bmpData.Scan0.ToInt32() + scanOffset), length);
        //                        }
        //                        bmp.UnlockBits(bmpData);
        //                        return bmp; //save the file
        //                    }

        //                    //int xrefIdx = ((PRIndirectReference)obj).Number;
        //                    //PdfObject pdfObj = doc.GetPdfObject(xrefIdx);
        //                    //PdfStream str = (PdfStream)(pdfObj);

        //                    //iTextSharp.text.pdf.parser.PdfImageObject pdfImage =
        //                    //    new iTextSharp.text.pdf.parser.PdfImageObject((PRStream)str);
        //                    //System.Drawing.Image ImgPDF = pdfImage.GetDrawingImage();
        //                    ////List<System.Drawing.Image> ImgList = test(tg, doc);
        //                    //Bitmap bmp = new Bitmap(ImgPDF);
        //                    //return bmp;
        //                }
        //                else if (PdfName.FORM.Equals(subtype) || PdfName.GROUP.Equals(subtype))
        //                {
        //                    return NewMethod(doc, tg);
        //                }

        //                //    ////dict = doc.GetPageN(pageNumber);
        //                //    res = (PdfDictionary)(PdfReader.GetPdfObject(tg.Get(PdfName.RESOURCES)));
        //                //    xobj = (PdfDictionary)(PdfReader.GetPdfObject(res.Get(PdfName.XOBJECT)));
        //                //    if (xobj != null)
        //                //    {
        //                //        foreach (PdfName name2 in xobj.Keys)
        //                //        {
        //                //            obj = xobj.Get(name2);
        //                //            if (obj.IsIndirect())
        //                //            {
        //                //                tg = (PdfDictionary)(PdfReader.GetPdfObject(obj));
        //                //                subtype = (PdfName)(PdfReader.GetPdfObject(tg.Get(PdfName.SUBTYPE)));
        //                //                if (PdfName.IMAGE.Equals(subtype))
        //                //                {
        //                //                    int xrefIdx = ((PRIndirectReference)obj).Number;
        //                //                    PdfObject pdfObj = doc.GetPdfObject(xrefIdx);
        //                //                    PdfStream str = (PdfStream)(pdfObj);

        //                //                    iTextSharp.text.pdf.parser.PdfImageObject pdfImage =
        //                //                        new iTextSharp.text.pdf.parser.PdfImageObject((PRStream)str);
        //                //                    System.Drawing.Image img = pdfImage.GetDrawingImage();
        //                //                    Bitmap bmp = new Bitmap(img);
        //                //                    return bmp;
        //                //                }
        //                //            }
        //                //            try
        //                //            {
        //                //                //System.IO.MemoryStream MS = new System.IO.MemoryStream(data);

        //                //                //MS.Position = 0;
        //                //                //System.Drawing.Image ImgPDF = System.Drawing.Image.FromStream(MS);

        //                //                //ImgList.Add(ImgPDF);

        //                //            }
        //                //            catch (Exception)
        //                //            {
        //                //            }
        //                //            //PdfStream str = (PdfStream)(.);

        //                //iTextSharp.text.pdf.parser.PdfImageObject pdfImage =
        //                //    new iTextSharp.text.pdf.parser.PdfImageObject((PRStream)str);
        //                //System.Drawing.Image ImgPDF2 = pdfImage.GetDrawingImage();
        //                //Bitmap bmp = new Bitmap(ImgPDF2);
        //                //return bmp;
        //                //        }
        //                //    }
        //                //    //Bitmap bmp = new Bitmap(ImgPDF);
        //                //    //images.AddRange(GetImagesFromPdfDict(tg, doc));
        //                //}

        //            }
        //        }
        //    }
        //    return null;
        //}
        //-------------------------------------------------------------------------
        private Bitmap GetBitmapFromPDFFile(string fileName, out Exception exception)
        {
            exception = null;
            Bitmap bmp = null;
            var pdfDoc = GetPDFDoc(fileName);
            // PdfReader pdfDoc = new PdfReader(fileName);//GetPDFDoc(fileName);
            if (pdfDoc.PageCount > 1)
            //if (pdfDoc.NumberOfPages > 1)
            {//You are trying to open a multipage file that can not be correctly processed.
                exception = new Exception("An attempt to open a multipage file that can not be correctly processed.");
            }
            else
            {
                bmp = GetBitmapFromPDFPage(pdfDoc, 1);
            }
            pdfDoc.Dispose();
            pdfDoc = null;
            return bmp;
        }
        //-------------------------------------------------------------------------
        private PDFLibNet.PDFWrapper GetPDFDoc(string fileName)
        {
            PDFLibNet.PDFWrapper pdfDoc = new PDFLibNet.PDFWrapper();
            pdfDoc.UseMuPDF = true;
            pdfDoc.LoadPDF(fileName);
            return pdfDoc;
        }
        //-------------------------------------------------------------------------
        private Bitmap GetBitmapFromPDFPage(PDFLibNet.PDFWrapper pdfDoc, int pageNumber)
        {
            if (pdfDoc == null) return null;
            //try
            //{
            pdfDoc.CurrentPage = pageNumber;
            double dpi = pdfDoc.RenderDPI;
            System.Windows.Forms.PictureBox pic = new System.Windows.Forms.PictureBox();
            pic.Width = 2560 + 17;//PhysicalDimension = {Width = 2560.0 Height = 3328.0}
            //pic.Height = 3328;//PhysicalDimension = {Width = 2560.0 Height = 3312.0}
            pdfDoc.FitToWidth(pic.Handle);
            pic.Height = pdfDoc.PageHeight;
            //pictureBox1.Height = pdfDoc.PageHeight;
            pdfDoc.RenderPage(pic.Handle);
            pdfDoc.ClientBounds = new System.Drawing.Rectangle(0, 0, pdfDoc.PageWidth, pdfDoc.PageHeight);
            Bitmap backbuffer = new Bitmap(pdfDoc.PageWidth, pdfDoc.PageHeight);
            using (Graphics g = Graphics.FromImage(backbuffer))
            {//недопустимый параметр при 8 потоках
                pdfDoc.DrawPageHDC(g.GetHdc());
                g.ReleaseHdc();
            }
            pic.Dispose();
            //var entryBitmap = (Bitmap)backbuffer.Clone();
            //backbuffer.Dispose();

            return backbuffer;// entryBitmap;
            //}
            //catch (Exception)
            //{
            //    return null;
            //}
        }
        #endregion
        //-------------------------------------------------------------------------
        /// <summary>
        /// If File is Multipage Exception != null
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public void VerifyFileForMultipaging(string fileName)
        {
            var bmp = GetBitmapFromFile(fileName);
            if (bmp != null)
            {
                bmp.Dispose();
                bmp = null;
            }
        }
        //-------------------------------------------------------------------------
        /// <summary>
        /// Return true if fileName is PDF 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool IsPDFFile(string fileName)
        {
            if (Path.GetExtension(fileName).ToLower() == ".pdf")
                return true;
            return false;
        }
    }
}
