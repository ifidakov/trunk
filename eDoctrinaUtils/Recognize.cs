using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Drawing.Imaging;
using System.Security.Cryptography;

namespace eDoctrinaUtils
{// ^[^\/]+\.Save\("
    public class Recognize
    {
        Utils utils = new Utils();
        IOHelper iOHelper = new IOHelper();
        Log log = new Log();
        AnswerVerification answerVerification = new AnswerVerification();
        public int bubbles_per_lineFLEX = 5;
        public int[] questionNumbers = new int[0];
        //public bool singleArea = false;
        public bool bubbles_per_lineErr = false;
        public DateTime begEx = DateTime.Now;
        string student_id = "";
        string district_id = "";
        string test_id = "";
        string amout_of_questions = "";

        //-------------------------------------------------------------------------
        static public Updates GetUpdates(string input)
        {
            Updates updates
            = JsonConvert.DeserializeObject<Updates>(input);
            return updates;
        }
        //-------------------------------------------------------------------------
        static public void CreateUpdatesJson(string ProductVersion)
        {
            Updates upd = new Updates();
            upd.version = ProductVersion;
            upd.description = @" 
Implementation tasks #185584, #185568.";
            upd.files = new string[]
            {
//"Miniatures\\test.jpg",
"eDoctrinaOcrUpdate.exe",
"eDoctrinaOcrUpdate.pdb",
"BitMiracle.LibTiff.NET.dll",
"BitMiracle.LibTiff.NET.xml",
"eDoctrinaOcrEd.exe",
"eDoctrinaOcrEd.pdb",
"eDoctrinaOcrWPF.exe",
"eDoctrinaOcrWPF.pdb",
"eDoctrinaUtils.dll",
"eDoctrinaUtils.pdb",
"eDoctrinaUtilsWPF.dll",
"eDoctrinaUtilsWPF.pdb",
"Emgu.CV.dll",
"Emgu.CV.UI.dll",
"Emgu.Util.dll",
"Installer.exe",
"Installer.pdb",
"itextsharp.dll",
"Newtonsoft.Json.dll",
"Newtonsoft.Json.pdb",
"Newtonsoft.Json.xml",
"opencv_core220.dll",
"opencv_highgui220.dll",
"opencv_imgproc220.dll",
"PDFLibNet.dll",
"startApp.exe",
"unins000.dat",
"unins000.exe",
"zxing.dll",
"zxing.pdb",
"zxing.presentation.dll",
"zxing.presentation.pdb",
"zxing.presentation.xml"
            };
            string output = JsonConvert.SerializeObject(upd);
            File.WriteAllText("updates.json", output);
        }
        //-------------------------------------------------------------------------
        #region Constructor
        public Recognize(string fileName, OcrAppConfig defaults, CancellationToken token, bool auto = false
            , bool normalizeBitmap = true)
        {
            SetCancellationToken(token);
            this.Auto = auto;
            this.defaults = defaults;
            recTools.frameFileName = fileName;

            //defaults.frameFileName = fileName;
            if (String.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                Exception = new Exception("File name is incorrect: " + fileName);
                log.LogMessage(Exception);
                NotifyUpdated(ExceptionEvent, Exception, null);
                return;
            }
            FileName = fileName;
            AuditFileName = utils.GetFileAuditName(fileName);
            #region Get Bitmap
            FramesAndBitmap fab = new FramesAndBitmap();
            Bitmap = fab.GetBitmapFromFile(FileName);
            if (fab.Exception != null)
            {
                Exception = fab.Exception;
                log.LogMessage(Exception);
                NotifyUpdated(ExceptionEvent, Exception, null);
                return;
            }
            //bool bppIndexed1 = false;
            //if (Bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format1bppIndexed)
            //    bppIndexed1 = true;

            if (normalizeBitmap)
                Bitmap = recTools.NormalizeBitmap(Bitmap, out exception);
            if (exception != null)
            {
                log.LogMessage(Exception);
                NotifyUpdated(ExceptionEvent, Exception, null);
                return;
            }


            //if (exception != null)
            //{
            //    if (auto)
            //    {
            //        fab.Exception = exception;
            //        Audit = new Audit(AuditFileName, out exception);
            //        exception = fab.Exception;
            //        Audit.error = exception.Message;
            //        if (defaults.MoveToNextProccessingFolderOnSheetIdentifierError)
            //        {
            //            utils.MoveBadFile(OcrAppConfig.TempFramesFolder, defaults.ManualNextProccessingFolder, fileName, Audit, fab.Exception.Message);//сохраняем файл в папку для дальнейшей обработки и удаляем из ТЕМПА
            //            File.Delete(fileName);

            //        }
            //        else
            //        {

            //        }
            //        return;
            //    }
            //}

            //else if (exception != null)
            //{

            //Bitmap = recTools.GetMonohromeNoIndexBitmap(Bitmap, false, true);
            //Bitmap.SetResolution(96, 96);

            #endregion
            #region Get audit file
            Audit = new Audit(AuditFileName, out exception);
            if (Audit == null || !File.Exists(AuditFileName))
            {
                log.LogMessage(Exception);
                NotifyUpdated(ExceptionEvent, Exception, null);
                return;
            }
            #endregion
            regionsList = recTools.GetAllRegions(ConfigsFolder);
            regionsListFLEX = recTools.GetAllRegionsFLEX(ConfigsFolder);
        }
        //-------------------------------------------------------------------------
        public void Dispose()
        {
            if (Bitmap != null)
            {
                Bitmap.Dispose();
                Bitmap = null;
            }
        }
        //-------------------------------------------------------------------------
        public void Rotate(RotateFlipType rotateFlipType)
        {
            Bitmap.RotateFlip(rotateFlipType);
        }
        //-------------------------------------------------------------------------
        public void SetCancellationToken(CancellationToken token)
        {
            cancellationToken = token;
            recTools.SetCancellationToken(cancellationToken);
        }
        #endregion
        //-------------------------------------------------------------------------
        #region public properties/params
        public RecognizeAction Status = RecognizeAction.Created;

        private Exception exception;
        public Exception Exception
        {
            get { return exception; }
            set { exception = value; }
        }

        private string fileName;
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        private string auditFileName;
        public string AuditFileName
        {
            get { return auditFileName; }
            set { auditFileName = value; }
        }

        private Audit audit;
        public Audit Audit
        {
            get { return audit; }
            set { audit = value; }
        }

        private Bitmap bitmap;
        public Bitmap Bitmap
        {
            get { return bitmap; }
            set { bitmap = value; }
        }
        public string QrCode { get; set; }
        public string[] QrCodeHeaders { get; set; }
        public string[] QrCodeValues { get; set; }
        //public IntPtr hbitmap
        //{
        //    get
        //    {
        //        return Bitmap.GetHbitmap();
        //    }
        //}
        #endregion
        //-------------------------------------------------------------------------
        #region private properties/params
        public bool Auto;
        private string ConfigsFolder
        {
            get
            {
                if (Auto)
                    return defaults.ConfigsFolder;
                return defaults.ManualConfigsFolder;
            }
        }

        public CancellationToken cancellationToken = new CancellationToken();
        public RecognitionTools recTools = new RecognitionTools();
        public ObservableCollection<BarCodeItem> BarCodeItems = new ObservableCollection<BarCodeItem>();
        public ObservableCollection<BubbleItem> BubbleItems = new ObservableCollection<BubbleItem>();

        public OcrAppConfig defaults;

        private string sheetIdentifier;
        public string SheetIdentifier
        {
            get { return sheetIdentifier; }
            set { sheetIdentifier = value; }
        }
        //private string testId;
        //public string TestId
        //{
        //    get { return testId; }
        //    set { testId = value; }
        //}
        public List<Regions> regionsList;
        public List<Regions> regionsListFLEX;
        public double darknessPercent = -1;//for DarknessManualySet.Checked in Editor
        public double darknessDifferenceLevel = -1;//for DarknessManualySet.Checked in Editor

        public double DarknessPercent
        {
            get { return regions.darknessPercent; }
        }
        public double DarknessDifferenceLevel
        {
            get { return regions.darknessDifferenceLevel; }
        }

        public Regions regions;
        public double filterType;
        private double allFilterType;
        private int allFilterCount;

        public decimal kx;
        public decimal ky;
        private int deltaY;
        public int lastBannerBottom;

        private string barCodesPrompt;
        public string BarCodesPrompt
        {
            get { return barCodesPrompt; }
            set
            {
                barCodesPrompt = value;
            }
        }
        public Rectangle curRect;
        public Rectangle etRect;
        private int regionOutputPosition;
        private double? percent_confident_text_region;
        private string lastSheetIdentifier;
        public string LastSheetIdentifier
        {
            get { return lastSheetIdentifier; }
            set { lastSheetIdentifier = value; }
        }

        public string[] headers;
        public string[] headersValues;
        public string[] allBarCodeNames;
        public string[] allBarCodeValues;
        public object[] totalOutput;
        public Dictionary<Bubble, Point[]> allContourMultiLine = new Dictionary<Bubble, Point[]>();
        public Rectangle[] factRectangle = new Rectangle[0];

        public RegionsArea[] areas;
        private int x1;
        private int y1;
        private int x2;
        private int y2;
        public Rectangle[] bubblesRegions;
        public Rectangle[] bubblesOfRegion;
        public int[] bubblesSubLinesCount;
        public int[] bubblesSubLinesStep;
        public int[] lineHeight;
        public string[] bubbleLines;
        public int numberOfBubblesRegion;
        public int answersPosition;
        public int indexAnswersPosition;
        public int indexOfFirstBubble;
        private string barcode;

        private Rectangle sheetIdentifierBarCodeRectangle;

        public int[] linesPerArea;
        public int[] bubblesPerLine;

        private int indexOfFirstQuestion;
        public int IndexOfFirstQuestion
        {
            get
            {
                return indexOfFirstQuestion;
            }
            set
            {
                indexOfFirstQuestion = value;
            }
        }

        public int maxAmoutOfQuestions;
        public int MaxAmoutOfQuestions
        {
            get
            {
                return maxAmoutOfQuestions;
            }
            //set
            //{
            //    maxAmoutOfQuestions = value;
            //}
        }

        private int amoutOfQuestions;
        public int AmoutOfQuestions
        {
            get
            {
                return amoutOfQuestions;
            }
            set
            {
                if (maxAmoutOfQuestions > 0 && value > maxAmoutOfQuestions)
                {
                    amoutOfQuestions = maxAmoutOfQuestions;
                }
                else
                {
                    amoutOfQuestions = value;
                }
            }
        }

        private bool notConfident;
        #endregion
        //-------------------------------------------------------------------------
        #region EventHandler && NotifyUpdated
        public static event EventHandler ExceptionEvent;
        public static event EventHandler<BarcodeEventArgs> FindedBarcodeControllEvent;
        public static event EventHandler ChangedBarCodesPrompt;
        public static event EventHandler<BubbleEventArgs> ChangedBubble;
        public void NotifyUpdated(EventHandler key, object obj, RecognizeEventArgs e)
        {
            var handler = key;
            if (handler != null) handler(obj, e);
        }
        public void NotifyUpdated(EventHandler<BarcodeEventArgs> key, object obj, BarcodeEventArgs e)
        {
            var handler = key;
            if (handler != null) handler(obj, e);
        }
        public void NotifyUpdated(EventHandler<BubbleEventArgs> key, object obj, BubbleEventArgs e)
        {
            var handler = key;
            if (handler != null) handler(obj, e);
        }
        #endregion
        //-------------------------------------------------------------------------
        #region RecAuto
        //-------------------------------------------------------------------------
        public void RecognizeAuto(ref string qrCodeText)
        {
            RecognizeAuto(ref lastSheetIdentifier, ref qrCodeText);
        }
        //-------------------------------------------------------------------------
        public void RecognizeAuto(ref string lastSheetIdentifier, ref string qrCodeText)
        {
            var errorFrame = false;
            RecAll(lastSheetIdentifier, ref qrCodeText);
            if (cancellationToken.IsCancellationRequested) return;
            if (Status == RecognizeAction.SearchMarkersFinished)
            {
                BarcodesRecognition(qrCodeText);
                if (cancellationToken.IsCancellationRequested) return;
                if (barCodesPrompt == "")
                {
                    BubblesRecognition();
                    if (cancellationToken.IsCancellationRequested) return;
                    //if (Auto) recTools.SetAdditionalOutputData(ref headers, ref totalOutput, regions, Path.GetFileName(FileName));//???????????????

                    RecAndFindBubble();
                    if (cancellationToken.IsCancellationRequested) return;
                    if (barCodesPrompt != "" || Exception != null)
                        errorFrame = true;
                }
                else
                    errorFrame = true;
            }
            else
                errorFrame = true;

            if (cancellationToken.IsCancellationRequested) return;
            VerifySheetAuto(errorFrame);
            //VerifySheet(BarCodeItems.ToList(), BubbleItems.ToList());
            Dispose();
        }
        #endregion
        //-------------------------------------------------------------------------
        #region RecAll
        public void RecAll(string SelectedBoxSheetItem, ref string qrCodeText)
        {
            RecAll(SelectedBoxSheetItem, false, ref qrCodeText);
        }
        //-------------------------------------------------------------------------
        public void RecAll(string SelectedBoxSheetItem, bool alignmentOnly
            , ref string qrCodeText, bool isRotate = false, bool isCut = false, bool ShetIdManualySet = false)
        {
            begEx = DateTime.Now;
            InitParamsBeforeSearchmarkers();
            lastSheetIdentifier = SelectedBoxSheetItem;

            if (alignmentOnly)
                SelectedSheetIdentifier(SelectedBoxSheetItem, ref qrCodeText, isRotate, isCut, ShetIdManualySet);
            else
                Searchmarkers(false, ref qrCodeText, isRotate, isCut, ShetIdManualySet);
            if (regions != null && regions.indexOfFirstBubble != 0)
                indexOfFirstBubble = regions.indexOfFirstBubble;
        }
        //-------------------------------------------------------------------------
        public void InitParamsBeforeSearchmarkers()
        {
            BarCodeItems.Clear();
            BubbleItems.Clear();
            AmoutOfQuestions = 0;
            indexOfFirstQuestion = 1;
            bubblesPerLine = new int[0];

            headers = new string[0];
            headersValues = new string[0];
            allBarCodeNames = new string[0];
            allBarCodeValues = new string[0];
            totalOutput = new object[0];

            sheetIdentifierBarCodeRectangle = Rectangle.Empty;
            curRect = Rectangle.Empty;
            etRect = Rectangle.Empty;

            bubblesRegions = new Rectangle[0];
            bubblesOfRegion = new Rectangle[0];
            bubblesSubLinesCount = new int[0];
            bubblesSubLinesStep = new int[0];
            lineHeight = new int[0];
            bubbleLines = new string[0];

            areas = new RegionsArea[0];
            regionOutputPosition = 0;
            x1 = 0;
            y1 = 0;
            x2 = 0;
            y2 = 0;
            kx = 1;
            ky = 1;
            deltaY = 0;
            numberOfBubblesRegion = 0;
            sheetIdentifier = "";
            barCodesPrompt = "";
            barcode = "";
            answersPosition = 0;
            indexAnswersPosition = 0;

            notConfident = false;

            filterType = 0;
            allFilterType = 0;
            allFilterCount = 0;
        }
        //-------------------------------------------------------------------------
        private void Searchmarkers(bool alignmentOnly, ref string qrCodeText
            , bool isRotate = false, bool isCut = false, bool ShetIdManualySet = false)
        {
            BubbleItems.Clear();
            if (alignmentOnly)
            {
                var tempbarCodesPrompt = barCodesPrompt;
                recTools.GetSheetIdentifier(ref bitmap, ref kx, ref ky, ref sheetIdentifier, ref lastSheetIdentifier
                    , regionsList, ref filterType, ref barCodesPrompt, out curRect, out etRect, deltaY
                    , defaults, ref sheetIdentifierBarCodeRectangle, alignmentOnly, ref qrCodeText, isRotate, isCut, ShetIdManualySet);
                if (barCodesPrompt != "" && tempbarCodesPrompt != "" && tempbarCodesPrompt != barCodesPrompt)
                    barCodesPrompt = tempbarCodesPrompt;
                sheetIdentifier = regions.SheetIdentifierName;
                lastSheetIdentifier = sheetIdentifier;
            }
            else
            {
                regions = recTools.GetSheetIdentifier(ref bitmap, ref kx, ref ky, ref sheetIdentifier, ref lastSheetIdentifier
                , regionsList, ref filterType, ref barCodesPrompt, out curRect, out etRect, deltaY
                , defaults, ref sheetIdentifierBarCodeRectangle, alignmentOnly, ref qrCodeText, isRotate, isCut, ShetIdManualySet);

                QrCode = qrCodeText;
                if (cancellationToken.IsCancellationRequested)
                    return;

                if (Auto && (barCodesPrompt != "" || regions == null) || !Auto && string.IsNullOrEmpty(QrCode) && (barCodesPrompt != "" || regions == null))//!!!
                {
                    filterType = 0;
                    //if (Auto || string.IsNullOrEmpty(QrCode))
                    //{
                    //    if (!Auto && barCodesPrompt != "Markers not found ")
                    //    {
                    //        return;
                    //    }

                    //    SheetIdentifierFixProblem();
                    //    return;
                    //}

                    //if (!string.IsNullOrEmpty(QrCode) && !Auto && barCodesPrompt == "Aligment error")
                    //{
                    //    QrCode = "";
                    //    //regions = null;
                    //}

                    //string pr = barCodesPrompt;
                    SheetIdentifierFixProblem();
                    lastSheetIdentifier = sheetIdentifier;
                    //barCodesPrompt = pr;
                    return;
                }
            }
            if (filterType > 0 && filterType < 10)
            {
                allFilterType += filterType;
                allFilterCount++;
            }
            //lastSheetIdentifier = sheetIdentifier;

            Status = RecognizeAction.SearchMarkersFinished;
        }
        //-------------------------------------------------------------------------
        private void SheetIdentifierFixProblem()
        {
            lastSheetIdentifier = "";
            Status = RecognizeAction.WaitingForUserResponse;
            NotifyUpdated(ChangedBarCodesPrompt, new RecognizeEventArgs(barCodesPrompt), null);
        }
        //-------------------------------------------------------------------------
        public void SelectedSheetIdentifier(string BoxSheetSelectedValue, ref string qrCodeText, bool isRotate = false
            , bool isCut = false, bool ShetIdManualySet = false)
        {
            Status = RecognizeAction.InProcess;
            string barcode = BoxSheetSelectedValue;
            regions = recTools.GetRegions(barcode, regionsList);
            if (regions == null)
            {
                SheetIdentifierFixProblem();
                return;
            }
            lastSheetIdentifier = BoxSheetSelectedValue;
            sheetIdentifier = lastSheetIdentifier;
            Searchmarkers(true, ref qrCodeText, isRotate, isCut, ShetIdManualySet);
        }
        #endregion
        //-------------------------------------------------------------------------
        #region Barcodes
        public void BarcodesRecognition(string qrCodeText = "")
        {
            var tempbarCodesPrompt = barCodesPrompt;
            ProcessingRegions(qrCodeText);
            if (barCodesPrompt == "Rotate180")
                return;

            if (!Auto && barCodesPrompt != "" && tempbarCodesPrompt != barCodesPrompt)
            {
                if (!barCodesPrompt.StartsWith("Aligment"))
                {
                    barCodesPrompt = tempbarCodesPrompt;
                }
            }
            if (!string.IsNullOrEmpty(qrCodeText))
            {
                if (!defaults.useStudentId)
                    filterType = (1 + filterType) / 2;
                else
                    filterType = 1;
            }
            else if (allFilterCount > 0)
                filterType = allFilterType / allFilterCount;
            if (Auto && barCodesPrompt != "")
                return;

            recTools.SetAdditionalOutputData(ref headers, ref totalOutput, regions, Path.GetFileName(FileName));
            Status = RecognizeAction.SearchBarcodesFinished;
        }
        //-------------------------------------------------------------------------
        public void ProcessingRegions(string qrCodeText = "")
        {
            string[] chars = new string[0];//, vals = new string[0]
            //string district_id = "D", test_id = "T", student_uid = "S", amout_of_questions = "Q", index_of_first_question = "F";
            string[] qrCodeFormatItemsId = new string[0];
            //string[] qrCodeFormatItems = new string[0];
            MatchCollection qrCodeFormatItemsVal = null;
            string barCodesPromptMem = barCodesPrompt;
            questionNumbers = new int[0];
            //QrCode = qrCodeText;
            if (qrCodeText != "")
            {
                var outputFileNameFormat = GetOutputFileNameFormat();
                chars = Regex.Split(qrCodeText, "\\d+");
                QrCodeValues = Regex.Split(qrCodeText.Remove(0, 1), "\\D+");
                for (int num = 0; num < regions.regions.Length; num++)
                {
                    Region regs = (Region)regions.regions[num];
                    bool? active = regs.active;
                    if (active == false)
                        continue;
                    if (regs.name != "qr_code")
                        continue;
                    qrCodeFormatItemsId = Regex.Split(regs.QRCodeFormat, "{\\w+}");
                    qrCodeFormatItemsVal = Regex.Matches(regs.QRCodeFormat, "{\\w+}");

                    QrCodeHeaders = new string[qrCodeFormatItemsVal.Count];
                    for (int i = 0; i < QrCodeHeaders.Length; i++)
                    {
                        QrCodeHeaders[i] = qrCodeFormatItemsVal[i].Value;
                    }
                }
            }
            if (QrCodeHeaders != null)
            {
                int index = Array.IndexOf(QrCodeHeaders, "{student_id}");
                if (index > -1)
                    student_id = QrCodeValues[index];
            }
            notConfident = false;
            Rectangle lastSymbolRectangle = Rectangle.Empty;
            //bool notRet = false;
            for (int num = 0; num < regions.regions.Length; num++)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;
                string barcodeMem = "";
                string type = regions.regions[num].type;
                bool? active = regions.regions[num].active;
                string name = regions.regions[num].name;
                if (active != false && type != "marker" && name != "sheetIdentifier")
                {
                    areas = regions.regions[num].areas;
                    RegionsArea arr = areas[0];
                    Rectangle rArr = new Rectangle(arr.left, arr.top, arr.width, arr.height);
                    percent_confident_text_region = regions.regions[num].percent_confident_text_region;
                    regionOutputPosition = regions.regions[num].outputPosition;
                    int rotateParameter = regions.regions[num].rotate;
                    // if (!barCodesPrompt.StartsWith("Markers"))
                    //{

                    int deltaX = 0;// (int)((etRect.X - curRect.X) / kx);

                    x1 = curRect.X + deltaX + (int)Math.Round((decimal)(rArr.Left - etRect.X) * kx);
                    y1 = curRect.Y + (int)Math.Round((decimal)(rArr.Top - etRect.Y) * ky);
                    x2 = curRect.X + deltaX + (int)Math.Round((decimal)(rArr.Right - etRect.X) * kx);
                    y2 = curRect.Y + (int)Math.Round((decimal)(rArr.Bottom - etRect.Y) * ky);
                    if (name == "lastBanner")// && SheetIdentifier != "100POINT"
                    {
                        if (barCodesPromptMem.StartsWith("M") || barCodesPromptMem.StartsWith("A"))
                        {
                            barCodesPrompt = "Aligment error 2";
                            deltaY = 0;
                            continue;
                        }
                        deltaY = recTools.LastBannerFind(Bitmap, x1, y1, x2, y2, out lastBannerBottom);//, filterType
                        if (Math.Abs(deltaY) > ((y2 - y1) / 2) + 1)
                        {
                            // notConfident = true;
                            barCodesPrompt = "Aligment error 3";
                            deltaY = 0;
                        }
                    }
                    //}
                    // else { }//Когда заполняются дефолтные значения при Markers Not Found
                    if (type.StartsWith("barCode"))
                    {
                        if (name == "sheetIdentifier")
                            continue;

                        if (name == "question_number_1")
                            lastSymbolRectangle = Rectangle.Empty;
                        if (name.StartsWith("question_number"))
                            if (AmoutOfQuestions > 0 && questionNumbers.Length >= AmoutOfQuestions)
                                continue;
                        Rectangle rn = new Rectangle(x1, y1, x2 - x1, y2 - y1);
                        //  if (!barCodesPrompt.StartsWith("Markers"))
                        //{
                        //barCodesPrompt = "";
                        int id = 0;
                        //oldRec:
                        if (qrCodeText == "" || id == -1 || QrCodeHeaders == null)
                        {
                            if (rArr == Rectangle.Empty)
                                continue;
                            GetBarCode(ref lastSymbolRectangle, num, ref barcodeMem, name, ref rn);
                            if (!Auto && SheetIdentifier == "FLEX")
                            {
                                if (recTools.result1 != null && recTools.result1.ResultPoints[0].X > recTools.result1.ResultPoints[1].X)
                                {//перевёрнут на 180
                                    BarCodesPrompt = "Rotate180";
                                    return;
                                }
                            }
                            if (name == "question_number_1")// && barCodesPrompt != ""
                            {
                                try
                                {
                                    IndexOfFirstQuestion = Convert.ToInt32(barcodeMem);
                                }
                                catch (Exception)
                                {
                                }
                            }
                            if (Auto && barCodesPrompt != "")//вставить IndexOfFirstQuestion и index_of_first_question для FLEX
                                return;//if (name == "question_number_1")
                        }
                        else
                        {
                            if (defaults.useStudentId)
                            {
                                id = Array.IndexOf(QrCodeHeaders, "{" + name + "}");
                                if (id < 0)
                                    continue;
                                barcode = QrCodeValues[id];
                                if (barcode == "0")//(name == "student_uid" || name == "student_id") && 
                                {
                                    barcode = "";
                                    barcodeMem = barcode;
                                    barCodesPrompt = "Error in " + name;
                                    if (Auto)
                                        return;
                                }
                                barcodeMem = barcode;
                            }
                            else
                            {
                                if (name == "student_id")
                                    continue;
                                id = Array.IndexOf(QrCodeHeaders, "{" + name + "}");
                                if (id < 0)
                                {
                                    if (rArr == Rectangle.Empty)
                                        continue;
                                    GetBarCode(ref lastSymbolRectangle, num, ref barcodeMem, name, ref rn);
                                    if (Auto && barCodesPrompt != "")
                                        return;
                                }
                                else
                                {
                                    barcode = QrCodeValues[id];
                                    barcodeMem = barcode;
                                }
                            }
                        }
                        //var bItem = new BarCodeItem(name, arr.type, barcode, barcodeMem, rn);
                        var bItem = new BarCodeItem(name, areas[1].type, barcode, barcodeMem, rn);
                        recTools.SetOutputValues(ref headers, ref headersValues, ref totalOutput, ref allBarCodeNames
                             , ref allBarCodeValues, bItem.Name, bItem.Value, regionOutputPosition);
                        try
                        {
                            if (name.StartsWith("question_number"))
                            {
                                Array.Resize(ref questionNumbers, questionNumbers.Length + 1);
                                questionNumbers[questionNumbers.Length - 1] = Convert.ToInt32(bItem.Value);
                            }
                            switch (name)
                            {
                                case "amout_of_questions":
                                    AmoutOfQuestions = Convert.ToInt32(bItem.Value);
                                    break;
                                case "index_of_first_question":
                                case "question_number_1":
                                    IndexOfFirstQuestion = Convert.ToInt32(bItem.Value);
                                    //notRet = true;
                                    break;
                                case "bubbles_per_line":
                                    //singleArea = true;
                                    //areas[0].bubblesPerLine = Convert.ToInt32(bItem.Value);
                                    //if (bubblesPerLine.Length == 0)
                                    //    bubblesPerLine = new int[areas.Length];
                                    //bubblesPerLine[0] = areas[0].bubblesPerLine;
                                    try
                                    {
                                        bubbles_per_lineFLEX = Convert.ToInt32(bItem.Value);
                                        switch (bubbles_per_lineFLEX)
                                        {
                                            case 5:
                                            case 6:
                                                foreach (var item in regionsListFLEX)
                                                {
                                                    if (item.regions[item.regions.Length - 1].areas[0].bubblesPerLine == bubbles_per_lineFLEX)
                                                    {
                                                        regions = item;
                                                        break;
                                                    }
                                                }
                                                break;
                                            default:
                                                bItem.Value = "";
                                                break;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        bItem.Value = "";
                                    }
                                    //areas[0].bubblesPerLine;
                                    break;
                            }
                        }
                        catch (Exception)
                        {
                            if (name == "index_of_first_question" || name == "question_number_1")
                            {
                                bItem.Value = IndexOfFirstQuestion.ToString();
                            }
                        }
                        //FindOrAddAndSetValueBarcode(bItem.Name, bItem.Value);
                        BarCodeItems.Add(bItem);
                        NotifyUpdated(FindedBarcodeControllEvent, null, new BarcodeEventArgs(bItem));
                    }
                    else
                        if (type == "bubblesRegions")
                    {
                        //indexOfFirstBubble = regions.regions[num].indexOfFirstBubble;
                        numberOfBubblesRegion = num;
                        //if (SheetIdentifier == "FLEX")
                        //{
                        //    areas[0].bubblesPerLine = bubbles_per_lineFLEX;
                        //}
                        recTools.SetSettings(ref bubblesRegions, ref bubblesOfRegion, ref bubblesSubLinesCount
                            , ref bubblesSubLinesStep, ref bubblesPerLine, ref lineHeight, ref linesPerArea
                            , out answersPosition, out indexAnswersPosition, ref totalOutput
                            , ref bubbleLines, regions.regions[num], kx, ky, curRect, etRect);
                    }
                }
            }
        }
        //-------------------------------------------------------------------------
        private void GetBarCode(ref Rectangle lastSymbolRectangle, int num, ref string barcodeMem, string name, ref Rectangle rn)
        {
            var currentNotConfident = false;
            var dualControl = (Auto) ? defaults.DualControl : true;
            barcode = recTools.GetBarCode
                    (
                      Bitmap
                    , ref currentNotConfident
                    , ref barCodesPrompt
                    , ref filterType
                    , ref barcodeMem
                    , x1, x2, y1, y2, kx, ky
                    , curRect, etRect
                    , deltaY
                    , regions.regions[num]
                    , dualControl
                    , percent_confident_text_region
                    , defaults.PercentConfidentText
                    , defaults.FontName
                    , ref rn
                    , ref lastSymbolRectangle
                    , Auto
                    );

            //Bitmap.Save("GetBarCode.bmp");

            if (currentNotConfident)
                notConfident = currentNotConfident;
            if (name == "bubbles_per_line")
            {
                switch (barcode)
                {
                    case "5":
                    case "6":
                        break;
                    default:
                        barCodesPrompt = "Invalid value in " + name;
                        bubbles_per_lineErr = true;
                        break;
                }
            }
            if ((!string.IsNullOrEmpty(barcodeMem) && filterType > 0 && filterType < 10)
                || string.IsNullOrEmpty(barcodeMem) && filterType > 0 && filterType < 3)//7!!!
            {
                allFilterType += filterType;
                allFilterCount++;
            }
        }
        //-------------------------------------------------------------------------
        public void FindOrAddAndSetValueBarcode(string name, string value, bool verify = true)
        {// public BarCodeItem
            BarCodeItem bItem;
            if (BarCodeItems.Any(x => x.Name == name))
            {
                bItem = BarCodeItems.First(x => x.Name == name);
                bItem.Value = value;
                bItem.Verify = false;
            }
            else
            {//для FANDP!!!!!!!!!!
                if (name == "student_uid")
                    bItem = new BarCodeItem(name, "Text", verify);//Rectangle???????????????
                else
                    bItem = new BarCodeItem(name, "numbersText", verify);
                bItem.Value = value;
                BarCodeItems.Add(bItem);
                NotifyUpdated(FindedBarcodeControllEvent, null, new BarcodeEventArgs(bItem));
                if (Array.IndexOf(allBarCodeNames, name) < 0)
                {
                    Array.Resize(ref allBarCodeValues, allBarCodeValues.Length + 1);
                    allBarCodeValues[allBarCodeValues.Length - 1] = value;
                    Array.Resize(ref allBarCodeNames, allBarCodeNames.Length + 1);
                    allBarCodeNames[allBarCodeNames.Length - 1] = name;
                }
            }
            //return bItem;
        }
        #endregion
        //-------------------------------------------------------------------------
        #region Bubbles
        public void BubblesRecognition(bool clear = true)
        {
            Status = RecognizeAction.SearchBubles;
            if (clear)
                BubbleItems.Clear();
            if (indexAnswersPosition < 1)
            {
                Exception = new Exception("Error in indexAnswersPosition");
                log.LogMessage(Exception);
                NotifyUpdated(ExceptionEvent, Exception, null);
                return;
            }
            if (answersPosition < 1)
            {
                Exception = new Exception("Error in answersPosition");
                log.LogMessage(Exception);
                NotifyUpdated(ExceptionEvent, Exception, null);
                return;
            }
            maxAmoutOfQuestions = linesPerArea.Sum();
            totalOutput[indexAnswersPosition - 1] = new string[0];
            totalOutput[answersPosition - 1] = new string[0];
            if (AmoutOfQuestions > maxAmoutOfQuestions)
            {
                AmoutOfQuestions = maxAmoutOfQuestions;
            }
            if (AmoutOfQuestions == 0)
            {
                AmoutOfQuestions = maxAmoutOfQuestions;
                FindOrAddAndSetValueBarcode("amout_of_questions", maxAmoutOfQuestions.ToString());
                if (SheetIdentifier == "FLEX")
                    FindOrAddAndSetValueBarcode("question_number_1", IndexOfFirstQuestion.ToString());
                else
                    FindOrAddAndSetValueBarcode("index_of_first_question", IndexOfFirstQuestion.ToString());
            }
        }
        //-------------------------------------------------------------------------
        public void SetDarkness(double darknessPercent, double darknessDifferenceLevel)
        {
            this.darknessPercent = darknessPercent;
            this.darknessDifferenceLevel = darknessDifferenceLevel;
        }
        Dictionary<Bubble, CheckedBubble> maxCountRectangles;
        //-------------------------------------------------------------------------
        public void RecAndFindBubble
            (
              bool empty = false
            //, Rectangle[] factRectangle = null
            //, Dictionary<Bubble, Point[]> allContourMultiLine = null
            )
        {
            //if (factRectangle == null)
            //{
            //factRectangle = new Rectangle[0];
            //allContourMultiLine = new Dictionary<Bubble, Point[]>();
            //}

            //var
            maxCountRectangles = AddMaxCountRectangles();

            //Bitmap.Save("recBitmap.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

            if (!barCodesPrompt.StartsWith("Markers") && !barCodesPrompt.StartsWith("Aligment"))//) || empty
            {
                barCodesPrompt = "";
                try
                {
                    if (Auto)
                    {
                        SetDarkness(regions.darknessPercent, regions.darknessDifferenceLevel);
                    }
                    recTools.BubblesRecognize
                            (
                              ref allContourMultiLine
                            , ref factRectangle
                            , Bitmap
                            , ref barCodesPrompt
                            , filterType
                            , true
                            , bubblesRegions
                            , bubblesOfRegion
                            , bubblesSubLinesCount
                            , bubblesSubLinesStep
                            , bubblesPerLine
                            , lineHeight
                            , linesPerArea
                            , answersPosition
                            , indexAnswersPosition
                            , totalOutput
                            , bubbleLines
                            , regions
                            , areas
                            , x1, x2, y1, y2
                            , kx, ky
                            , curRect, etRect
                            , deltaY
                            , AmoutOfQuestions, IndexOfFirstQuestion
                            , maxCountRectangles
                            , darknessPercent, darknessDifferenceLevel
                            , lastBannerBottom
                            //, deltaX
                            );

                    //Bitmap.Save("recBitmap.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                    if (Auto && barCodesPrompt != "" || cancellationToken.IsCancellationRequested)
                    {
                        return;//errorFrame = true;
                    }
                }
                catch (Exception ex)
                {
                    Exception = ex;
                    log.LogMessage(Exception);
                    barCodesPrompt = "Error in \"RecAndFindBubble\"";
                    Audit.error = ex.Message;
                    FillBubbleItems(maxCountRectangles);
                    //NotifyUpdated(ExceptionEvent, Exception, null);
                    NotifyUpdated(ChangedBubble, null, new BubbleEventArgs(false, BubbleItems, regions.regions[numberOfBubblesRegion].areas
                        , AmoutOfQuestions, maxAmoutOfQuestions, IndexOfFirstQuestion, linesPerArea, bubblesPerLine));
                    Status = RecognizeAction.SearchBublesFinished;
                    return;
                }
                if (barCodesPrompt == "")
                {
                    FillBubbleItems(maxCountRectangles);
                    //FillBubbleItemsRectangle(allContourMultiLine, factRectangle);//???
                    FindBubble(factRectangle, allContourMultiLine);
                }
                else
                    maxCountRectangles = AddMaxCountRectangles();
            }
            else
            {
                FillBubbleItems(maxCountRectangles);
                recTools.AppendOutput(ref totalOutput, indexAnswersPosition, IndexOfFirstQuestion.ToString(), indexAnswersPosition, indexOfFirstBubble);
            }
            UpdateGui();
        }
        //-------------------------------------------------------------------------
        public void UpdateGui()
        {
            Status = RecognizeAction.SearchBublesFinished;
            NotifyUpdated(ChangedBubble, null, new BubbleEventArgs(true, BubbleItems, regions.regions[numberOfBubblesRegion].areas
                , AmoutOfQuestions, maxAmoutOfQuestions, IndexOfFirstQuestion, linesPerArea, bubblesPerLine));
        }
        //-------------------------------------------------------------------------
        public void FillBubbleItems(Dictionary<Bubble, CheckedBubble> maxCountRectangles)
        {
            //DateTime dt = DateTime.Now;

            //int count = BubbleItems.Count;
            //if (maxCountRectangles.Count > BubbleItems.Count)
            //{
            for (int k = 0; k < maxCountRectangles.Count; k++)//count
            {
                BubbleItems.Add(new BubbleItem(maxCountRectangles.ElementAt(k).Key, maxCountRectangles.ElementAt(k).Value));
            }
            //}
            //else if (maxCountRectangles.Count < BubbleItems.Count)
            //{
            //    for (int k = count - 1; k >= maxCountRectangles.Count; k--)
            //    {
            //        BubbleItems.RemoveAt(k);
            //    }
            //}

            //TimeSpan ts = DateTime.Now - dt;
        }
        //-------------------------------------------------------------------------
        public void FillBubbleItemsRectangle(Dictionary<Bubble, Point[]> allContourMultiLine, Rectangle[] factRectangle)
        {//можно не использовать?
            //DateTime dt = DateTime.Now;
            var BubbleItemsDict = BubbleItems.ToDictionary(x => x.Bubble);
            //TimeSpan ts = DateTime.Now - dt;

            for (int k = 0; k < allContourMultiLine.Count; k++)
            {
                //BubbleItem item = null;
                try
                {
                    //item = BubbleItems.First(x => x.Bubble.Equals(allContourMultiLine.ElementAt(k).Key));
                    BubbleItemsDict[allContourMultiLine.ElementAt(k).Key].CheckedBubble.rectangle = factRectangle[k];
                }
                catch (Exception ex)
                {
                    Exception = ex;
                    break;
                }
            }
            //TimeSpan ts = DateTime.Now - dt;
        }
        //-------------------------------------------------------------------------
        public Dictionary<Bubble, CheckedBubble> AddMaxCountRectangles()
        {
            Dictionary<Bubble, CheckedBubble> maxCountRectangles = new Dictionary<Bubble, CheckedBubble>();
            //if (SheetIdentifier != "FLEX")
            //{
            int nextLine = 0;
            for (int k = 0; k < areas.Length; k++)
            {
                if (k > 0)
                {
                    nextLine += linesPerArea[k];
                }
                for (int o = 0; o < linesPerArea[k]; o++)
                {
                    if (o + 1 + nextLine > AmoutOfQuestions)
                        break;
                    for (int m = 0; m < areas[k].subLinesAmount + 1; m++)
                    {
                        if (o + 1 + nextLine > AmoutOfQuestions)
                            break;
                        for (int l = 0; l < areas[k].bubblesPerLine; l++)
                        {
                            if (o + 1 + nextLine > AmoutOfQuestions)
                                break;
                            Bubble b = new Bubble();
                            b.point = new Point(l, o + nextLine + IndexOfFirstQuestion);
                            b.subLine = m;
                            b.areaNumber = k;
                            //b.index = maxCountRectangles.Count;
                            maxCountRectangles.Add(b, new CheckedBubble());
                        }
                    }
                }
            }
            //}
            //else
            //{
            //    for (int o = 0; o < linesPerArea[0]; o++)
            //    {
            //        for (int l = 0; l < bubbles_per_lineFLEX; l++)
            //        {
            //            Bubble b = new Bubble();
            //            b.point = new Point(l, o + IndexOfFirstQuestion);
            //            b.subLine = 0;
            //            b.areaNumber = 0;
            //            //b.index = maxCountRectangles.Count;
            //            maxCountRectangles.Add(b, new CheckedBubble());
            //        }
            //    }
            //}
            return maxCountRectangles;
        }
        //-------------------------------------------------------------------------
        public void FindBubble(Rectangle[] factRectangle, Dictionary<Bubble, Point[]> allContourMultiLine
            , bool grid = false)
        {
            int bubblesPerWidth = 0, bubbleStepX = 0, bubbleStepY = 0;
            int areaNumber = -1;
            int goodBubbleNumber = 0;
            int prevGoodLine = 0, prevGoodLineY = 0;
            int maxBubblesDist = 12900;//25;// 
            int[] axisX = new int[1];
            int[] axisY = new int[1];
            int[] axisYSubline = new int[1];
            //Rectangle bubble1 = Rectangle.Empty;
            Rectangle bubble1 = bubblesOfRegion[0];
            Rectangle prevRectangle = Rectangle.Empty;
            Bubble bubble = new Bubble();
            double factStepY = bubble1.Height + bubbleStepY;

            //for (int i = 0; i < maxCountRectangles.Count; i++)
            //{
            //    var itm = maxCountRectangles.ElementAt(i);
            //    BubbleItems[i].CheckedBubble = itm.Value;
            //}

            if (axisX.Length == 1)
            {
                recTools.GetAxis
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
            }
            BubbleItem FirstBubbleItemOfArea = null;
            for (int k = 0; k < BubbleItems.Count; k++)
            {
                BubbleItem item = BubbleItems[k];
                //KeyValuePair<Bubble, CheckedBubble> item = maxCountRectangles.ElementAt(k);
                int bubblesRegion = item.Bubble.areaNumber;
                if (String.IsNullOrEmpty(areas[0].bubblesOrientation) || areas[0].bubblesOrientation == "horizontal")
                {
                    bubbleStepY = (int)Math.Round((decimal)(lineHeight[bubblesRegion] - bubble1.Height));
                }
                else
                {
                    bubbleStepY = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Height
                        - bubble1.Height * bubblesPerWidth) / (bubblesPerWidth - 1));
                }

                if (areaNumber != bubblesRegion)
                {
                    areaNumber = bubblesRegion;
                    //factStepY = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Height
                    // bubblesPerLine[bubblesRegion]));
                    if (String.IsNullOrEmpty(areas[0].bubblesOrientation)
                      || areas[0].bubblesOrientation == "horizontal")
                        bubbleStepY = (int)Math.Round((decimal)(lineHeight[bubblesRegion] - bubble1.Height));
                    else
                        bubbleStepY = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Height
                            - bubble1.Height * bubblesPerWidth) / (bubblesPerWidth - 1));
                    factStepY = bubble1.Height + bubbleStepY;

                    prevGoodLineY = 0; prevGoodLine = 0;
                    recTools.GetLineFactStep
                     (
                       ref factStepY
                     , ref prevGoodLine
                     , ref prevGoodLineY
                     , k
                     , areas[bubblesRegion]
                     , factRectangle
                     , allContourMultiLine
                     , bubblesRegion
                     //, minContourLength
                     );
                    bubblesPerWidth = bubblesPerLine[bubblesRegion];
                    bubble1 = bubblesOfRegion[bubblesRegion];
                    if (string.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
                        || areas[bubblesRegion].bubblesOrientation == "horizontal"
                        )
                        bubbleStepX = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Width
                            - bubble1.Width * bubblesPerWidth) / (bubblesPerWidth - 1));
                    else
                    {
                        if (areas[bubblesRegion].subLinesAmount == 0)
                        {
                            bubblesSubLinesStep[bubblesRegion] = (int)Math.Round((decimal)(areas[0].bubble.Width * 2) * kx);
                        }
                        bubbleStepX = bubblesSubLinesStep[bubblesRegion];
                        bubbleStepY = (int)Math.Round((decimal)(bubblesRegions[bubblesRegion].Height
                            / bubblesPerLine[bubblesRegion] - bubble1.Height));
                    }
                }
                if (item.CheckedBubble.rectangle.Size == new Size())
                {
                    //#region плохой пузырь // almost like in Recognize -> BadBubble()
                    if (!grid)
                        BadBubble
                            (item
                            , k
                            , maxBubblesDist
                            , bubblesRegion
                            , bubble1
                            , ref prevRectangle
                            , bubbleStepX
                            , factStepY
                            , axisY
                            );
                }
                else
                {//хороший пузырь
                    int factRect = Array.IndexOf(factRectangle, item.CheckedBubble.rectangle);
                    GoodBubble(item, k, bubblesRegion, ref factStepY, ref bubble, ref prevGoodLineY, item.CheckedBubble.rectangle.Y);
                    goodBubbleNumber = factRect;// k;
                    prevRectangle = new Rectangle(item.CheckedBubble.rectangle.X, item.CheckedBubble.rectangle.Y
                        , item.CheckedBubble.rectangle.Width, item.CheckedBubble.rectangle.Height);
                }
            }
            for (int k = 0; k < BubbleItems.Count; k++)
            {
                BubbleItem item = BubbleItems[k];
                int bubblesRegion = item.Bubble.areaNumber;
                if (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation) || areas[bubblesRegion].bubblesOrientation == "horizontal")
                {
                    bubbleStepY = (int)Math.Round((decimal)(lineHeight[bubblesRegion] - bubble1.Height));
                    if (k == 0)
                    {
                        FirstBubbleItemOfArea = BubbleItems[0];
                    }
                    else
                    {
                        if (FirstBubbleItemOfArea != null && FirstBubbleItemOfArea.Bubble.areaNumber != item.Bubble.areaNumber)
                        {
                            if (Math.Abs(item.CheckedBubble.rectangle.Y - FirstBubbleItemOfArea.CheckedBubble.rectangle.Y)
                                >= FirstBubbleItemOfArea.CheckedBubble.rectangle.Height)
                            {
                                //Exception = new Exception("Calibration error 3");
                                barCodesPrompt = "Calibration error 3";
                                var maxCountRectangles = AddMaxCountRectangles();
                                BubbleItems.Clear();
                                FillBubbleItems(maxCountRectangles);
                                break;
                            }
                            FirstBubbleItemOfArea = item;
                        }
                        else
                        {
                            if (FirstBubbleItemOfArea != null && item.Bubble.point.X == 0 && FirstBubbleItemOfArea.Bubble.point.Y != item.Bubble.point.Y)
                            {
                                if (Math.Abs(item.CheckedBubble.rectangle.X - FirstBubbleItemOfArea.CheckedBubble.rectangle.X)
                                    >= FirstBubbleItemOfArea.CheckedBubble.rectangle.Height)
                                {
                                    //Exception = new Exception("Calibration error 4");
                                    barCodesPrompt = "Calibration error 4";
                                    var maxCountRectangles = AddMaxCountRectangles();
                                    BubbleItems.Clear();
                                    FillBubbleItems(maxCountRectangles);
                                    break;
                                }
                            }
                        }
                    }
                }
                //else
                //{//для vertical

                //}
            }
        }
        //-------------------------------------------------------------------------
        private void GoodBubble(BubbleItem item, int k, int bubblesRegion
            , ref double factStepY, ref Bubble bubble, ref int prevGoodLineY, int factRectangle_k_Y)
        {//хороший пузырь 
            if (bubble.Equals(new Bubble()) && item.Bubble.subLine == 0)
            {
                bubble = item.Bubble;
                prevGoodLineY = factRectangle_k_Y;
            }
            else
            {
                try
                {
                    if (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation) || areas[bubblesRegion].bubblesOrientation == "horizontal")
                    {//определение текущего значения lineFactStep
                        if (item.Bubble.subLine == 0 && bubble.areaNumber == item.Bubble.areaNumber && bubble.point.Y != item.Bubble.point.Y)
                        {
                            factStepY = (double)(factRectangle_k_Y - prevGoodLineY) / (item.Bubble.point.Y - bubble.point.Y);
                            bubble = item.Bubble;
                            prevGoodLineY = factRectangle_k_Y;
                        }
                    }
                    else
                    {
                        if (bubble.areaNumber == item.Bubble.areaNumber && bubble.subLine == item.Bubble.subLine && item.Bubble.point.X != bubble.point.X)
                        {
                            factStepY = (double)(factRectangle_k_Y - prevGoodLineY) / (item.Bubble.point.X - bubble.point.X);
                            bubble = item.Bubble;
                            prevGoodLineY = factRectangle_k_Y; //item.Key.point.X;
                        }
                    }
                }
                catch (Exception)
                { }
            }
        }
        //-------------------------------------------------------------------------
        private void BadBubble(BubbleItem item, int k, int maxBubblesDist, int bubblesRegion
            , Rectangle bubble1, ref Rectangle prevRectangle, int bubbleStepX
            , double factStepY
            , int[] axisY = null
            )
        {//плохой пузырь
            int dist, minDist = int.MaxValue, numCont = -1;
            int n;
            var itm = BubbleItems[k];
            for (int kn = 1; kn < maxBubblesDist; kn++)
            {
                n = k + kn;
                if (n < BubbleItems.Count - 1)
                {
                    itm = BubbleItems[n];
                    //if ((regionRectangle.ElementAt(n).Value == regionRectangle.ElementAt(k).Value)
                    if (item.CheckedBubble.rectangle.Size != new Size() && (itm.Bubble.areaNumber == item.Bubble.areaNumber))
                    {
                        dist = Math.Abs(item.Bubble.point.X - itm.Bubble.point.X) + Math.Abs(item.Bubble.point.Y - itm.Bubble.point.Y) + Math.Abs(item.Bubble.subLine - itm.Bubble.subLine);
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
                    itm = BubbleItems[n];
                    if (itm.CheckedBubble.rectangle.Size != new Size() && (itm.Bubble.areaNumber == item.Bubble.areaNumber))
                    {
                        dist = Math.Abs(item.Bubble.point.X - itm.Bubble.point.X) + Math.Abs(item.Bubble.point.Y - itm.Bubble.point.Y) + Math.Abs(item.Bubble.subLine - itm.Bubble.subLine);
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
            if (numCont > -1)
            {//itm = замещающий, item = замещаемый
                int distX;
                int distY;
                int distYsub;
                int moveX;
                int moveY;
                itm = BubbleItems[numCont];
                if (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
                    || areas[bubblesRegion].bubblesOrientation == "horizontal")
                {
                    distX = item.Bubble.point.X - itm.Bubble.point.X;
                    distY = item.Bubble.point.Y - itm.Bubble.point.Y;
                    distYsub = item.Bubble.subLine - itm.Bubble.subLine;
                    moveX = (bubble1.Width + bubbleStepX) * distX;
                    moveY = (int)Math.Round((double)factStepY * distY + bubblesSubLinesStep[bubblesRegion] * distYsub);
                }
                else
                {
                    distY = item.Bubble.point.X - itm.Bubble.point.X;
                    distX = item.Bubble.point.Y - itm.Bubble.point.Y;
                    distYsub = item.Bubble.subLine - itm.Bubble.subLine;
                    moveY = (int)Math.Round((double)factStepY * distY);
                    moveX = (int)Math.Round((double)bubbleStepX * distX + (bubblesSubLinesStep[bubblesRegion]) * distYsub);
                }
                prevRectangle = new Rectangle
                    (BubbleItems[numCont].CheckedBubble.rectangle.X
                    , BubbleItems[numCont].CheckedBubble.rectangle.Y
                    , BubbleItems[numCont].CheckedBubble.rectangle.Width
                    , BubbleItems[numCont].CheckedBubble.rectangle.Height
                    );
                prevRectangle.Y += moveY;
                if (moveY != 0 && axisY != null && axisY.Length > 0 && axisY[0] != 0)
                {
                    if (item.Bubble.point.X == 0 && item.Bubble.subLine == 0)
                    {
                        int bestWal = int.MaxValue;
                        int delta = int.MaxValue;
                        foreach (int item2 in axisY)
                        {
                            int delta2 = Math.Abs(prevRectangle.Y - item2);
                            if (delta2 < delta)// bestY)
                            {
                                delta = delta2;
                                bestWal = item2;
                            }
                        }
                        if (bestWal != int.MaxValue)
                            prevRectangle.Y = bestWal;
                    }
                    else
                    {
                        if (BubbleItems[k - 1].Bubble.subLine == item.Bubble.subLine)
                        {
                            prevRectangle.Y = BubbleItems[k - 1].CheckedBubble.rectangle.Y;
                        }
                    }
                }
                if (item.Bubble.subLine > 0 && (String.IsNullOrEmpty(areas[bubblesRegion].bubblesOrientation)
                    || areas[bubblesRegion].bubblesOrientation == "horizontal"))
                {
                    prevRectangle.X = BubbleItems[k - areas[bubblesRegion].bubblesPerLine].CheckedBubble.rectangle.X;
                    BubbleItems[k].CheckedBubble.rectangle = new Rectangle(prevRectangle.X, prevRectangle.Y, prevRectangle.Width, prevRectangle.Height);
                }
                else
                {
                    prevRectangle.X += moveX;
                    BubbleItems[k].CheckedBubble.rectangle = new Rectangle(prevRectangle.X, prevRectangle.Y, prevRectangle.Width, prevRectangle.Height);
                }
            }
            else
            {//err
                barCodesPrompt = recTools.PromptCalibrationError(item.Bubble);
            }
        }
        #endregion
        //-------------------------------------------------------------------------
        #region Verify Sheet
        public void SetValueManual(BarCodeItem item)
        {
            int index = Array.IndexOf(allBarCodeNames, item.Name);
            if (index > -1)
            {
                allBarCodeValues[index] = item.Value;
                item.Verify = true;
            }
        }
        //-------------------------------------------------------------------------
        public void VerifySheetManual(List<BarCodeItem> BarCodeItems, List<BubbleItem> BubbleItems)
        {
            FillValues();
            if (iOHelper.CreateDirectory(defaults.ManualSuccessFolder))
            {
                var outputFileNameFormat = GetOutputFileNameFormat();
                var destFileNameTiff = GetNextFileName(Path.Combine(defaults.ManualSuccessFolder, outputFileNameFormat + ".tiff"));
                if (string.IsNullOrEmpty(defaults.OutputMode) || defaults.OutputMode.ToLower().IndexOf("tif") > -1)
                {
                    File.Copy(this.FileName, destFileNameTiff, true);
                    log.LogMessage("Save " + destFileNameTiff);
                }
                var destFileNameCsv = Path.ChangeExtension(destFileNameTiff, ".csv");
                if (string.IsNullOrEmpty(defaults.OutputMode) || defaults.OutputMode.ToLower().IndexOf("csv") > -1)
                {
                    recTools.WriteCsv(destFileNameCsv, totalOutput, answersPosition, indexAnswersPosition);
                    log.LogMessage("Save " + destFileNameCsv);
                }
                var destFileNameAudit = utils.GetFileAuditName(destFileNameCsv);
                Audit = Audit.GetFinalAuditForManual(destFileNameTiff, destFileNameCsv, utils.GetSHA1FromFile(destFileNameCsv));
                if (string.IsNullOrEmpty(defaults.OutputMode) || defaults.OutputMode.ToLower().IndexOf("audit") > -1)
                {
                    Audit.Save(destFileNameAudit);
                    log.LogMessage("Save " + destFileNameAudit);
                }
            }
        }
        //-------------------------------------------------------------------------
        public void VerifySheetAuto(bool errorFrame)
        {
            string direct = (errorFrame)
                ? (barCodesPrompt.StartsWith("Sheet") && defaults.MoveToNextProccessingFolderOnSheetIdentifierError)
                    ? defaults.ManualNextProccessingFolder : (barCodesPrompt.StartsWith("Unsupported"))
                        ? defaults.NotSupportedSheetFolder : (barCodesPrompt.StartsWith("Empty"))
                            ? defaults.EmptyScansFolder : defaults.ErrorFolder
                : (notConfident) ? defaults.NotConfidentFolder : defaults.SuccessFolder;

            if (defaults.RemoveEmptyScans && barCodesPrompt.StartsWith("Empty"))
            {
                log.LogMessage("Empty scan was removed " + Audit.fileName + " Page " + Audit.sourcePage.ToString());
                //if (!Directory.Exists(direct))
                //    Directory.CreateDirectory(direct);
                //    return;
            }

            if (!iOHelper.CreateDirectory(direct))
            {
                var message = "Could not find part of the path " + direct;
                Exception = new Exception(message);
                return;
            }
            try
            {
                string destFileNameCsv = "";
                string destFileNameTiff = "";
                ////теряются пиксели при умненьшении относительно оригинального размера картинки
                //byte[] tiffBytes = recTools.TiffImageBytes(Bitmap);

                //так не умненьшается оригинальный размер картинки\/
                Bitmap = (Bitmap)Bitmap.FromFile(recTools.frameFileName);//!!!

                Bitmap bmp;
                if (Bitmap.HorizontalResolution != Bitmap.VerticalResolution)
                {
                    if (Bitmap.HorizontalResolution > Bitmap.VerticalResolution)
                    {
                        bmp = new Bitmap
                            (
                              Bitmap.Width
                            , Bitmap.Height * (int)(Bitmap.HorizontalResolution / Bitmap.VerticalResolution)
                            , System.Drawing.Imaging.PixelFormat.Format24bppRgb
                            );
                        bmp.SetResolution(Bitmap.HorizontalResolution, Bitmap.HorizontalResolution);
                        Graphics g = Graphics.FromImage(bmp);
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                        g.DrawImage(Bitmap, 0, 0, bmp.Width, bmp.Height);
                        g.Dispose();
                        Bitmap = (Bitmap)bmp.Clone();
                        bmp.Dispose();
                    }
                    else
                    {
                        bmp = new Bitmap
                            (
                              Bitmap.Width * (int)(Bitmap.VerticalResolution / Bitmap.HorizontalResolution)
                            , Bitmap.Height
                            , System.Drawing.Imaging.PixelFormat.Format24bppRgb
                            );
                        bmp.SetResolution(Bitmap.HorizontalResolution, Bitmap.HorizontalResolution);
                        Graphics g = Graphics.FromImage(bmp);
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                        g.DrawImage(Bitmap, 0, 0, bmp.Width, bmp.Height);
                        g.Dispose();
                        Bitmap = (Bitmap)bmp.Clone();
                        bmp.Dispose();
                    }

                    //entryBitmap.Save("entryBitmap.bmp", ImageFormat.Bmp);
                }
                Bitmap.SetResolution(96, 96);

                //bmp = new Bitmap(Bitmap.Width, Bitmap.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                //using (Graphics g = Graphics.FromImage(bmp))
                //{
                //    g.DrawImage(Bitmap, 0, 0, Bitmap.Width, Bitmap.Height);
                //}
                byte[] tiffBytes = recTools.TiffImageBytes(ref bitmap);//bmp//Bitmap
                //bmp.Dispose();

                //if (Bitmap.PixelFormat.ToString().EndsWith("Indexed"))//тоже недостаточно памяти
                //    Bitmap = new Bitmap(Bitmap);
                //byte[] tiffBytes = recTools.TiffImageBytes((Bitmap)Bitmap.Clone());


                if (tiffBytes == null)
                    throw new Exception("TiffBytes equals null");

                if (errorFrame)
                {
                    var fnTiffErr = Path.Combine(direct, Path.GetFileName(FileName));
                    if (defaults.DoNotProcess)
                    {
                        try
                        {
                            string incFileName = audit.sourceFileName;
                            FileInfo fi = new FileInfo(Path.Combine("TempIncFile", incFileName));
                            if (barCodesPrompt.StartsWith("Empty"))
                            {
                                //if (!fi.Exists)
                                fi.MoveTo(Path.Combine("OUTPUT_WHITE", incFileName));
                            }
                            else
                            {
                                //if (!fi.Exists)
                                fi.MoveTo(Path.Combine("OUTPUT_NOT_WHITE", incFileName));
                            }
                            //iOHelper.DeleteFile(FileName);
                        }
                        catch (Exception)
                        {
                        }
                        iOHelper.DeleteFile(Path.ChangeExtension(FileName, "audit"));
                        tiffBytes = null;
                        return;
                    }
                    File.WriteAllBytes(fnTiffErr, tiffBytes);
                    //Bitmap.Save(fnTiffErr, System.Drawing.Imaging.ImageFormat.Tiff);
                    string sha1HashTiff = utils.GetSHA1FromFile(fnTiffErr);
                    destFileNameTiff = Path.Combine(direct, sha1HashTiff + ".tiff");

                    if (!File.Exists(destFileNameTiff))//???!!! лишнее?
                    {
                        File.Copy(fnTiffErr, destFileNameTiff, true);//???!!!
                    }
                    iOHelper.DeleteFile(fnTiffErr);//???!!!
                }
                else
                {
                    FillValues();
                    string outputFileNameFormat = GetOutputFileNameFormat();
                    if (outputFileNameFormat.IndexOf("{") >= 0)
                    {
                        for (int i = 0; i < QrCodeHeaders.Length; i++)
                        {
                            try
                            {
                                if (outputFileNameFormat.IndexOf(QrCodeHeaders[i]) >= 0)
                                    outputFileNameFormat = outputFileNameFormat.Replace(QrCodeHeaders[i], QrCodeValues[i]);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    destFileNameTiff = GetNextFileName(Path.Combine(direct, outputFileNameFormat + ".tiff"));
                    if (string.IsNullOrEmpty(defaults.OutputMode) || defaults.OutputMode.ToLower().IndexOf("tif") > -1)
                    {
                        File.WriteAllBytes(destFileNameTiff, tiffBytes);
                        //Bitmap.Save(destFileNameTiff, System.Drawing.Imaging.ImageFormat.Tiff);
                    }

                    destFileNameCsv = Path.ChangeExtension(destFileNameTiff, ".csv");
                    if (string.IsNullOrEmpty(defaults.OutputMode) || defaults.OutputMode.ToLower().IndexOf("csv") > -1)
                    {
                        recTools.WriteCsv(destFileNameCsv, totalOutput, answersPosition, indexAnswersPosition);
                    }
                }
                tiffBytes = null;
                var pageAudit = Audit.GetFinalAuditForAuto(errorFrame, destFileNameTiff, destFileNameCsv, barCodesPrompt);
                if (string.IsNullOrEmpty(defaults.OutputMode) || defaults.OutputMode.ToLower().IndexOf("audit") > -1)
                {
                    pageAudit.Save(utils.GetFileAuditName(destFileNameTiff));
                    log.LogMessage("Save " + Audit.fileName + " Page " + Audit.sourcePage.ToString());
                }
            }
            catch (Exception ex)
            {
                log.LogMessage(ex);
                Exception = ex;
                return;
            }
        }
        //-------------------------------------------------------------------------
        private void FillValues()
        {
            FillBarCodeItemsToTotalOutput();
            FillBubbleItemsToTotalOutput();
            CheckForCorrectIndexOfFirstQuestionInTotalOutput();
            AddOutputFileValueFromConfig();
            if (questionNumbers.Length > 0)
            {
                {
                    string[] s = totalOutput[indexAnswersPosition - 1] as string[];
                    for (int i = 0; i < questionNumbers.Length; i++)
                        s[i] = questionNumbers[i].ToString();
                }
            }
            if (!string.IsNullOrEmpty(defaults.OutputMode) && defaults.OutputMode.IndexOf("json") > -1)
            {
                int index = Array.IndexOf(allBarCodeNames, "district_id");
                if (index > -1)
                    district_id = allBarCodeValues[index];
                index = Array.IndexOf(allBarCodeNames, "test_id");
                if (index > -1)
                    test_id = allBarCodeValues[index];
                index = Array.IndexOf(allBarCodeNames, "amout_of_questions");
                if (index > -1)
                    amout_of_questions = allBarCodeValues[index];
                index = Array.IndexOf(allBarCodeNames, "student_uid");
                string studentUid = "";
                if (index > -1)
                    studentUid = allBarCodeValues[index];
                if (QrCodeHeaders != null)
                {
                    index = Array.IndexOf(QrCodeHeaders, "student_id");
                    if (index > -1)
                        student_id = QrCodeValues[index];
                }

                // string data = "{" + Environment.NewLine + "\"data\": {" + Environment.NewLine
                //     + "\"district_id: " + district_id + "," + Environment.NewLine
                //     + "\"test_id: " + this.test_id + "," + Environment.NewLine
                //     + "\"student_id: " + this.student_id + "," + Environment.NewLine;
                // string answers = "\"answers\": [" + Environment.NewLine;

                // string coordinanes = "\"coordinanes\": {" + Environment.NewLine
                //     + "\"questionBubbles\": [{" + Environment.NewLine;

                // Bubble bubble = BubbleItems[0].Bubble;
                // int row = 1;
                // answers += "{ \"idx\": " + bubble.point.Y.ToString() + "," + Environment.NewLine
                //     + "\"answers\": [{" + Environment.NewLine + "\"row\": " + row.ToString() + ","
                //     + Environment.NewLine + "\"cols\": [ ";
                // coordinanes += "{ \"idx\": " + bubble.point.Y.ToString() + "," + Environment.NewLine
                //     + "\"answers\": [{" + Environment.NewLine + "\"row\": " + row.ToString() + ","
                //     + Environment.NewLine + "\"cols\": [ ";

                // int subline = 0;
                // bool rowChecked = false;
                // for (int k = 0; k < BubbleItems.Count; k++)
                // {
                //     BubbleItem item = BubbleItems[k];
                //     coordinanes = SetBubbleRect(coordinanes, item);
                //     if (item.CheckedBubble.isChecked)
                //     {
                //         if (rowChecked)
                //         {
                //             answers += ",";
                //         }
                //         rowChecked = true;
                //         answers += item.Bubble.point.X;
                //     }
                //     if (item.Bubble.point.Y != bubble.point.Y)
                //     {
                //         row = 1;
                //         subline = 0;
                //         bubble = item.Bubble;
                //         rowChecked = false;
                //         answers += "]" + Environment.NewLine + "}]" + Environment.NewLine
                //             + "}," + Environment.NewLine + "{ \"idx\": " + bubble.point.Y.ToString() + "," + Environment.NewLine
                //             + "\"answers\": [{" + Environment.NewLine + "\"row\": " + row.ToString() + ","
                //             + Environment.NewLine + "\"cols\": [ ";
                //         coordinanes += "]" + Environment.NewLine + "}]" + Environment.NewLine
                //             + "}," + Environment.NewLine + "{ \"idx\": " + bubble.point.Y.ToString() + "," + Environment.NewLine
                //             + "\"answers\": [{" + Environment.NewLine + "\"row\": " + row.ToString() + ","
                //             + Environment.NewLine + "\"cols\": [ ";

                //     }
                //     if (item.Bubble.subLine != subline)
                //     {
                //         row++;
                //         rowChecked = false;
                //         answers += " ]" + Environment.NewLine + "}," + Environment.NewLine + "\"row\": " + row.ToString() + ","
                //                 + Environment.NewLine + "\"cols\": [ ";
                //         subline = item.Bubble.subLine;
                //         coordinanes += " ]" + Environment.NewLine + "}," + Environment.NewLine + "\"row\": " + row.ToString() + ","
                //                     + Environment.NewLine + "\"cols\": [ ";
                //         coordinanes = SetBubbleRect(coordinanes, item);

                //         subline = item.Bubble.subLine;
                //     }

                // }
                // answers += " ]" + Environment.NewLine + "}]" + Environment.NewLine
                //            + "}],";
                // coordinanes += " ]" + Environment.NewLine + "}]" + Environment.NewLine
                //+ "}],";

                // data += (answers + coordinanes);
                // data = data.Replace(",]", "]");

                //string dt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string processingType = "Manual";
                if (Auto)
                    processingType = "Auto";
                //string audit = Environment.NewLine +
                //    "\"audit\": {" + Environment.NewLine +
                //    "\"processedAt\": \"" + dt + "\"," + Environment.NewLine +
                //    "\"processingType\": " + processingType + ",";

                int districtId = Convert.ToInt32(district_id);
                int testId = Convert.ToInt32(test_id);
                int studentId = 0;
                if (!string.IsNullOrEmpty(student_id))
                    studentId = Convert.ToInt32(student_id);
                int amoutOfQuestions = Convert.ToInt32(amout_of_questions);
                answerVerification = new AnswerVerification();
                //AnswerVerification.QuestionBubble[] qb = new AnswerVerification.QuestionBubble[1];
                //AnswerVerification.QuestionBubble qbb = new AnswerVerification.QuestionBubble();
                answerVerification.data = new AnswerVerification.Data
                {
                    district_id = districtId,
                    test_id = testId,
                    student_id = studentId,
                    student_uid = studentUid,
                    amout_of_questions = amoutOfQuestions.ToString(),
                    index_of_first_question = IndexOfFirstQuestion.ToString()
                }
                ;
                answerVerification.answers = new AnswerVerification.Answers[amoutOfQuestions];
                answerVerification.coordinates = new AnswerVerification.Coordinate();//[amoutOfQuestions]
                answerVerification.image = new AnswerVerification.Img();
                double factor = (double)1024 / Bitmap.Height;
                int width = (int)Math.Round(Bitmap.Width * factor);

                answerVerification.coordinates.questionBubbles = new AnswerVerification.QuestionBubble[amoutOfQuestions];
                //answerVerification.data.coordinates.questionBubbles[0] = new AnswerVerification.QuestionBubble[1];
                index = 0;
                string[] indexRow = totalOutput[indexAnswersPosition - 1] as string[];//[i] ;
                for (int i = 0; i < amoutOfQuestions; i++)
                {
                    string[] str = totalOutput[answersPosition - 1] as string[];//indexAnswersPosition
                    string[] subRows = str[i].Split('~');
                    int rows = subRows.Length;
                    answerVerification.answers[i] = new AnswerVerification.Answers
                    {
                        //idx = i + IndexOfFirstQuestion,
                        idx = (int)Convert.ToUInt32(indexRow[i]),
                        answers = new AnswerVerification.Answer[rows]
                    }
                    ;
                    answerVerification.coordinates.questionBubbles[i].idx = (int)Convert.ToUInt32(indexRow[i]);// i + IndexOfFirstQuestion;// new AnswerVerification.QuestionBubble[1];
                    answerVerification.coordinates.questionBubbles[i].answers = new AnswerVerification.Row[rows];

                    for (int j = 0; j < rows; j++)
                    {
                        AnswerVerification.Bubble[] ab = new AnswerVerification.Bubble[0];
                        answerVerification.coordinates.questionBubbles[i].answers[j].row = j + 1;
                        for (int k = index; k < BubbleItems.Count; k++)
                        {
                            BubbleItem item = BubbleItems[k];
                            if (item.Bubble.point.Y != i + IndexOfFirstQuestion)
                            {
                                //k--;
                                index = k;
                                break;
                            }
                            Array.Resize(ref ab, ab.Length + 1);
                            ab[ab.Length - 1].pos = (k % bubblesPerLine[item.Bubble.areaNumber]) + 1;
                            ab[ab.Length - 1].x = (int)Math.Round(item.CheckedBubble.rectangle.X * factor);
                            ab[ab.Length - 1].y = (int)Math.Round(item.CheckedBubble.rectangle.Y * factor);
                            ab[ab.Length - 1].width = (int)Math.Round(item.CheckedBubble.rectangle.Width * factor);
                            ab[ab.Length - 1].height = (int)Math.Round(item.CheckedBubble.rectangle.Height * factor);
                            answerVerification.coordinates.questionBubbles[i].answers[j].cols
                                = (AnswerVerification.Bubble[])ab.Clone();
                            if (k < BubbleItems.Count - 1)
                            {
                                if (item.Bubble.areaNumber == BubbleItems[k + 1].Bubble.areaNumber
                                    && item.Bubble.subLine != BubbleItems[k + 1].Bubble.subLine)
                                {
                                    k++;
                                    index = k;
                                    break;
                                }
                            }
                        }
                        answerVerification.answers[i].answers[j].row = j + 1;
                        string[] rowValues = subRows[j].Split('|');
                        answerVerification.answers[i].answers[j].cols = new string[rowValues.Length];
                        for (int k = 0; k < rowValues.Length; k++)
                        {
                            answerVerification.answers[i].answers[j].cols[k] = rowValues[k];
                        }
                    }
                }
                //Rectangle rect = new Rectangle(0, 0, Bitmap.Width, Bitmap.Height);
                //System.Drawing.Imaging.BitmapData bmpData = Bitmap.LockBits(rect
                //    , System.Drawing.Imaging.ImageLockMode.ReadOnly, Bitmap.PixelFormat);
                //IntPtr ptr = bmpData.Scan0;
                //int bytes = Math.Abs(bmpData.Stride) * Bitmap.Height;
                //byte[] rgbValues = new byte[bytes];
                //Marshal.Copy(ptr, rgbValues, 0, bytes);
                //string imageTxt = System.Convert.ToBase64String(rgbValues);
                //Bitmap.UnlockBits(bmpData);
                //JavaScriptSerializer js = new JavaScriptSerializer();
                //string av = js.Serialize(answerVerification);
                //eDoctrinaUtils.SerializerHelper sh = new SerializerHelper();
                //av = sh.Serialise(answerVerification);
                //int height = (int)Math.Round(Bitmap.Height / d);
                Bitmap bmp = new Bitmap(width, 1024, PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(Bitmap, 0, 0, bmp.Width, bmp.Height);
                }
                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Png);

                //bmp.Save("bmp.Png", ImageFormat.Png);

                //string sha1hashFromFile = utils.GetSHA1FromFile("bmp.Png");
                byte[] byteArray = new byte[0];
                byteArray = ms.ToArray();
                SHA1Managed sha = new SHA1Managed();
                //byte[] hash = sha.ComputeHash(ms);
                byteArray = sha.ComputeHash(byteArray);
                string sha1hash = BitConverter.ToString(byteArray).Replace("-", String.Empty);

                byte[] byteArr = ms.ToArray();
                ms.Close();
                ms.Dispose();
                bmp.Dispose();
                string binStr = Convert.ToBase64String(byteArr);

                answerVerification.image.data = binStr;
                answerVerification.image.hash = sha1hash;// Audit.SHA1Hash;
                                                         //DateTime dt1 = DateTime.Now;
                if (!Auto)
                {
                    FileInfo fi = new FileInfo(fileName);
                    begEx = DateTime.Parse(fi.CreationTime.ToString());
                }
                TimeSpan ts = DateTime.Now - begEx;
                answerVerification.audit.processingTime = ts.TotalSeconds.ToString();
                answerVerification.audit.processingType = processingType;
                answerVerification.audit.processedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                answerVerification.audit.sourceFileHash = audit.sourceSHA1Hash;
                answerVerification.audit.sourceFilePageNo = audit.sourcePage;
                answerVerification.audit.serverName = defaults.ServerName;
                answerVerification.audit.dataFileName = GetNextFileName(Path.Combine(defaults.SuccessFolder, GetOutputFileNameFormat() + ".csv"));
                string output = JsonConvert.SerializeObject(answerVerification);
                //var jsonSerializer = new JsonSerializer();
                //var array = output.ToArray();
                //// Serialization bson
                //byte[] bytes;
                //string data = "";
                //using (var ms1 = new MemoryStream())
                //using (var bson = new Newtonsoft.Json.Bson.BsonWriter(ms1))
                //{
                //    //jsonSerializer.Serialize(bson, array, typeof(string[]));
                //    jsonSerializer.Serialize(bson, answerVerification);
                //    bytes = ms1.ToArray();
                //    data = Convert.ToBase64String(ms1.ToArray());
                //}
                //if (!Directory.Exists(defaults.OutputFolder_Success))
                //    Directory.CreateDirectory(defaults.OutputFolder_Success);
                //File.WriteAllBytes(Path.Combine(defaults.OutputFolder_Success, sha1hash + ".ocr.bson"), bytes);
                //File.WriteAllText(Path.Combine(defaults.OutputFolder_Success, sha1hash + ".ocrString.bson"), data);

                if (!Directory.Exists(defaults.OutputFolder_Success))
                    Directory.CreateDirectory(defaults.OutputFolder_Success);
                File.WriteAllText(Path.Combine(defaults.OutputFolder_Success, sha1hash + ".ocr.json"), output);

            }
        }
        //-------------------------------------------------------------------------
        public AnswerVerification GetAnswerVerification(string input)
        {
            AnswerVerification answerVerification
            = JsonConvert.DeserializeObject<AnswerVerification>(input);
            return answerVerification;
        }
        //-------------------------------------------------------------------------
        //public string Base64Encode(string plainText)
        //{
        //    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        //    return System.Convert.ToBase64String(plainTextBytes);
        //}
        ////-------------------------------------------------------------------------
        //public string Base64Encode(byte[] bytes)
        //{
        //    return System.Convert.ToBase64String(bytes);
        //}
        //-------------------------------------------------------------------------
        //private string SetBubbleRect(string coordinanes, BubbleItem item)
        //{
        //    coordinanes += Environment.NewLine
        //                + "{ \"pos\": " + item.Bubble.point.X.ToString()
        //                + " \"x\": " + item.CheckedBubble.rectangle.X.ToString()
        //                + ", \"y\": " + item.CheckedBubble.rectangle.Y.ToString()
        //                + ", \"height\": " + item.CheckedBubble.rectangle.Height.ToString()
        //                + ", \"width\": " + item.CheckedBubble.rectangle.Width.ToString() + " },";
        //    return coordinanes;
        //}
        //-------------------------------------------------------------------------
        private void FillBubbleItemsToTotalOutput()
        {
            Bubble[] currentLineBubbles = new Bubble[0];
            Bubble bubble2 = BubbleItems[0].Bubble;
            int currentLine = bubble2.point.Y;
            totalOutput[indexAnswersPosition - 1] = new string[0];
            totalOutput[answersPosition - 1] = new string[0];
            int[] subStrSet = new int[areas[bubble2.areaNumber].subLinesAmount];
            recTools.AppendOutput(ref totalOutput, indexAnswersPosition, currentLine.ToString(), indexAnswersPosition, indexOfFirstBubble);
            for (int k = 0; k < BubbleItems.Count; k++)
            {
                var item = BubbleItems[k];
                bubble2 = item.Bubble;
                if (bubble2.point.Y == currentLine)//есть ли значения в строке
                {
                    if (item.CheckedBubble.isChecked)
                    {
                        Array.Resize(ref currentLineBubbles, currentLineBubbles.Length + 1);
                        currentLineBubbles[currentLineBubbles.Length - 1] = bubble2;
                    }
                    if (k < BubbleItems.Count - 1)
                    {
                        continue;
                    }
                }
                if (currentLineBubbles.Length == 0)//если в строке нет значений
                {
                    if (subStrSet.Length > 0 && areas[bubble2.areaNumber].bubblesFormat == "multiple")
                    {
                        for (int n = 0; n < subStrSet.Length; n++)
                        {
                            subStrSet[n] = n;
                            recTools.AppendOutput(ref totalOutput, answersPosition, "~", indexAnswersPosition, indexOfFirstBubble);
                        }
                    }
                    else
                    {
                        recTools.AppendOutput(ref totalOutput, answersPosition, "", indexAnswersPosition, indexOfFirstBubble);
                    }
                }
                else//записываем значения текущей строки в totalOutput
                {
                    for (int j = 0; j < currentLineBubbles.Length; j++)
                    {
                        Bubble b2 = currentLineBubbles[j];
                        AppendOutput(b2, j, ref subStrSet, currentLineBubbles.Length - 1);
                    }
                }
                if (k < BubbleItems.Count - 1)
                {
                    Array.Resize(ref currentLineBubbles, 0);
                    currentLine = bubble2.point.Y;
                    recTools.AppendOutput(ref totalOutput, indexAnswersPosition, currentLine.ToString(), indexAnswersPosition, indexOfFirstBubble);
                    subStrSet = new int[areas[bubble2.areaNumber].subLinesAmount];
                    k--;
                }
            }
        }
        //-------------------------------------------------------------------------
        private void AppendOutput(Bubble bubble, int positionInLine, ref int[] subStrSet, int lengthcurrentLineBubbles)
        {
            if ((areas[bubble.areaNumber].bubblesFormat == "single") || bubble.subLine == 0)
            {
                if (positionInLine > 0)
                {
                    recTools.AppendOutput(ref totalOutput, answersPosition, "|", indexAnswersPosition, indexOfFirstBubble);
                }
            }
            else
            {
                if (subStrSet.Length > 0)
                {
                    for (int n = 0; n < bubble.subLine; n++)
                    {
                        if (subStrSet.Length > 0)
                        {
                            if (Array.IndexOf(subStrSet, bubble.subLine) < 0)
                            {
                                subStrSet[bubble.subLine - 1] = bubble.subLine;
                                recTools.AppendOutput(ref totalOutput, answersPosition, "~", indexAnswersPosition, indexOfFirstBubble);
                                break;
                            }
                            else
                            {
                                recTools.AppendOutput(ref totalOutput, answersPosition, "|", indexAnswersPosition, indexOfFirstBubble);
                                break;
                            }
                        }
                    }
                }
            }
            if ((areas[bubble.areaNumber].bubblesFormat == "single"))
            {
                int index = indexOfFirstBubble;
                if (areas[bubble.areaNumber].indexOfFirstBubble != 0)
                    index = areas[bubble.areaNumber].indexOfFirstBubble;
                recTools.AppendOutput(
                      ref totalOutput
                    , answersPosition
                    , (bubble.point.X + (bubble.subLine * areas[bubble.areaNumber].bubblesPerLine)).ToString()
                    , indexAnswersPosition
                    , index//indexOfFirstBubble
                    );
            }
            else
            {
                recTools.AppendOutput(ref totalOutput, answersPosition, bubble.point.X.ToString(), indexAnswersPosition, indexOfFirstBubble);
                if (bubble.subLine < subStrSet.Length && lengthcurrentLineBubbles == positionInLine)
                {
                    for (int n = bubble.subLine; n < subStrSet.Length; n++)
                    {
                        recTools.AppendOutput(ref totalOutput, answersPosition, "~", indexAnswersPosition, indexOfFirstBubble);
                    }
                }
            }
        }
        //-------------------------------------------------------------------------
        private void CheckForCorrectIndexOfFirstQuestionInTotalOutput()
        {
            try
            {
                string[] str = totalOutput[indexAnswersPosition - 1] as string[];
                int first = Convert.ToInt32(str[0]);
                if (first != IndexOfFirstQuestion)
                {
                    for (int k = 0; k < str.Length; k++)
                    {
                        str[k] = (IndexOfFirstQuestion + k).ToString();
                    }
                    totalOutput[indexAnswersPosition - 1] = str;
                }
            }
            catch (Exception) { }
        }
        //int temp = 0;
        //-------------------------------------------------------------------------
        private void AddOutputFileValueFromConfig()
        {
            foreach (KeyValuePair<int, string> item in defaults.OutputFileValue)
            {
                if (item.Key > totalOutput.Length)
                {
                    Array.Resize(ref totalOutput, item.Key);
                }
                if (item.Value.Equals("{PROCESSINGMODE}") && !Auto)
                {
                    totalOutput[item.Key - 1] = "M";//item.Value="M";
                }
                else if (item.Value.Equals("{PROCESSINGMODE}") && Auto)
                {
                    totalOutput[item.Key - 1] = "A";
                }
                else if (item.Value.Equals("{QRCODEMODE}") && !string.IsNullOrEmpty(QrCode))
                {
                    totalOutput[item.Key - 1] = "1";
                }
                else if (item.Value.Equals("{QRCODEMODE}") && string.IsNullOrEmpty(QrCode))
                {
                    totalOutput[item.Key - 1] = "0";
                }
                else
                {
                    totalOutput[item.Key - 1] = item.Value;
                }
            }
        }
        //-------------------------------------------------------------------------
        private void FillBarCodeItemsToTotalOutput()
        {
            for (int num = 0; num < regions.regions.Length; num++)
            {
                string type = regions.regions[num].type;
                bool? active = regions.regions[num].active;
                string name = regions.regions[num].name;

                if (active != false && type != "marker" && name != "sheetIdentifier")
                {
                    regionOutputPosition = regions.regions[num].outputPosition;
                    if (regionOutputPosition > 0)
                    {
                        foreach (var item in BarCodeItems)
                        {
                            //if (item.Name == "amout_of_questions")
                            //{
                            //    amout_of_questions = Convert.ToInt32(item.Value);
                            //}
                            if (item.Name == name)
                            {
                                int index = Array.IndexOf(allBarCodeNames, name);
                                if (index > -1)
                                {
                                    allBarCodeValues[index] = item.Value;
                                }
                                headersValues[regionOutputPosition - 1] = item.Value;
                                totalOutput[regionOutputPosition - 1] = item.Value;
                                break;
                            }
                        }
                    }
                }
            }
        }
        //-------------------------------------------------------------------------
        private string GetOutputFileNameFormat()
        {
            string outputFileNameFormat = "";
            if (regions != null)
            {
                outputFileNameFormat = regions.outputFileNameFormat;
            }
            for (int k = 0; k < allBarCodeNames.Length; k++)
            {
                int index = Array.IndexOf(headers, allBarCodeNames[k]);
                if (index >= 0)
                {
                    if (defaults.OutputFileValue.Keys.Contains(k))
                    {
                        allBarCodeValues[k] = defaults.OutputFileValue[k];
                    }
                }
                outputFileNameFormat = outputFileNameFormat.Replace("{" + allBarCodeNames[k] + "}", allBarCodeValues[k]);
            }
            if (outputFileNameFormat.IndexOf("{index_of_first_question}") >= 0)
            {
                outputFileNameFormat = outputFileNameFormat.Replace("{index_of_first_question}", indexOfFirstQuestion.ToString());
            }
            if (outputFileNameFormat.IndexOf("{amout_of_questions}") >= 0)
            {
                outputFileNameFormat = outputFileNameFormat.Replace("{amout_of_questions}", AmoutOfQuestions.ToString());
            }
            outputFileNameFormat = Regex.Replace(outputFileNameFormat, "{.+}", "");
            //outputFileNameFormat = outputFileNameFormat.Replace("__", "_");
            outputFileNameFormat = outputFileNameFormat.Trim('_');
            return outputFileNameFormat;
        }
        #endregion
        //-------------------------------------------------------------------------
        #region Helpers
        private string GetNextFileName(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return fileName;
            }
            int i = -1;
            while (File.Exists(fileName))
            {
                if (i < 0)
                {
                    fileName = fileName.Insert(fileName.LastIndexOf("."), "-1");
                    i = 1;
                }
                else
                {
                    string s = "-" + i.ToString() + ".";
                    int j = fileName.LastIndexOf(s);
                    fileName = fileName.Replace("-" + i.ToString() + ".", "");
                    i++;
                    fileName = fileName.Insert(j, "-" + i.ToString() + ".");
                }
            }
            return fileName;
        }
        #endregion
    }
}