using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eDoctrinaUtils
{
    public class Recognize
    {
        public Exception exception;
        public string FileName;
        public string FileNameAudit;
        public Bitmap Image;
        Audit FileAudit;
        public static event EventHandler ExceptionEvent;
        public static event EventHandler FindedBarcodeControllEvent;
        public static event EventHandler ChangedBarCodesPrompt;
        private enum NotifyKey { Exception, FindedBarcodeControll, ChangedBarCodesPrompt }
        private void NotifyUpdated(NotifyKey key)
        {
            NotifyUpdated(key, null, null);
        }
        private void NotifyUpdated(NotifyKey key, object obj)
        {
            NotifyUpdated(key, obj, null);
        }
        private void NotifyUpdated(NotifyKey key, object obj, EventArgs e)
        {
            switch (key)
            {
                case NotifyKey.Exception:
                    var handler = ExceptionEvent;
                    if (handler != null) handler(obj, e);
                    break;
                case NotifyKey.FindedBarcodeControll:
                    handler = FindedBarcodeControllEvent;
                    if (handler != null) handler(obj, e);
                    break;
                case NotifyKey.ChangedBarCodesPrompt:
                    handler = ChangedBarCodesPrompt;
                    if (handler != null) handler(obj, e);
                    break;
            }
        }

        public Defaults defaults;
        public string[] sheetIdentifiers;
        public Recognize()
        { }
        public Recognize(string fileName, Defaults defaults)
        {
            this.defaults = defaults;
            if (String.IsNullOrEmpty(fileName) || !File.Exists(fileName))
            {
                exception = new Exception("File name is incorrect: " + fileName);
                NotifyUpdated(NotifyKey.Exception, exception);
                return;
            }
            FileName = fileName;
            FileNameAudit = fileName.Replace(new FileInfo(fileName).Extension, ".audit");
            #region Get Bitmap
            PDFLibNet.PDFWrapper pdfDoc = null;
            try
            {
                Image = Utils.GetBitmapFromFile(FileName, ref pdfDoc);
                Image = Utils.NormalizeBitmap(Image);
                Image = RecognitionTools.GetMonohromeNoIndexBitmap(Image);
            }
            catch (Exception ex)
            {
                exception = ex;
                NotifyUpdated(NotifyKey.Exception, exception);
                return;
            }
            finally
            {
                if (pdfDoc != null)
                {
                    pdfDoc.Dispose();
                    pdfDoc = null;
                }
            }
            #endregion
            #region Get audit file
            FileAudit = Utils.GetAuditFromFile(FileNameAudit, out exception);
            if (FileAudit == null)
            {
                NotifyUpdated(NotifyKey.Exception, exception);
                return;
            }
            #endregion
            regionsExt = RecognitionTools.GetRegionsExt(out sheetIdentifiers, Defaults.ManualConfigsFolder);
        }

        public List<RecognitionTools.RegionsExt> regionsExt;

        public Regions regions;
        double filterType = 1;
        decimal kx = 1, ky = 1;
        int deltaY = 0;
        public string sheetIdentifier = "";
        public string barCodesPrompt = "";
        Rectangle markerLTet = new Rectangle();
        Rectangle markerRTet = new Rectangle();
        Rectangle markerLBet = new Rectangle();
        Rectangle markerRBet = new Rectangle();
        Rectangle curRect = new Rectangle();
        Rectangle etRect = new Rectangle();
        int regionOutputPosition = 0;
        double? percent_confident_text_region;
        string lastSheetIdentifier;

        public string[] headers = new string[0];
        public string[] headersValues = new string[0];
        public string[] allBarCodeNames = new string[0];
        public string[] allBarCodeValues = new string[0];
        object[] totalOutput = new object[0];

        #region
        public void Searchmarkers(string lastSheetIdentifier)
        {
            this.lastSheetIdentifier = lastSheetIdentifier;
            regions = RecognitionTools.GetSheetIdentifier(ref Image, ref kx, ref ky, ref sheetIdentifier, ref lastSheetIdentifier
                , regionsExt, ref filterType, ref barCodesPrompt, sheetIdentifiers, Image, ref regionOutputPosition
                , ref markerLTet, ref markerRTet, ref markerLBet, ref markerRBet, out curRect, out etRect, deltaY
                , defaults.DualControl, percent_confident_text_region, defaults.PercentConfidentText
                , RecognitionTools.limSymbols, defaults.FontName);
            if (barCodesPrompt != "" || regions == null)
            {
                NotifyUpdated(NotifyKey.ChangedBarCodesPrompt);
            }
            lastSheetIdentifier = sheetIdentifier;
            RecognitionTools.SetOutputValues(ref headers, ref headersValues, ref totalOutput, ref allBarCodeNames
                , ref allBarCodeValues, "sheetIdentifier", sheetIdentifier, regionOutputPosition);


            maxCountRectangles.Clear();

            if (lastSheetIdentifier != sheetIdentifier)
            {
                // regions = WaitingForUserResponse(ref barcode, sheetIdentifiers, ref  sheetIdentifier, regionsExt);
            }
        }

        public Regions.Area[] areas;
        int x1 = 0;
        int y1 = 0;
        int x2 = 0;
        int y2 = 0;
        Rectangle[] bubblesRegions = new Rectangle[0];
        Rectangle[] bubblesOfRegion = new Rectangle[0];
        int[] bubblesSubLinesCount = new int[0];
        int[] bubblesSubLinesStep = new int[0];
        int[] lineHeight = new int[0];
        string[] bubbleLines = new string[0];
        int numberOfBubblesRegion = 0;
        int answersPosition = 0;
        int indexAnswersPosition = 0;
        int indexOfFirstBubble = 0;
        string barcode = "";
        string additionalOutputData = "";

        public void BarcodesRecognition()
        {
            ProcessingRegions(ref regions, ref areas, ref regionOutputPosition, ref barCodesPrompt
             , ref x1, ref y1, ref x2, ref y2, ref kx, ref ky, ref curRect, ref etRect, ref deltaY, Image
             , ref barcode, ref filterType, ref totalOutput
             , ref indexOfFirstBubble, ref numberOfBubblesRegion, ref bubblesRegions, ref bubblesOfRegion, ref bubblesSubLinesCount
             , ref bubblesSubLinesStep, ref lineHeight, ref answersPosition, ref indexAnswersPosition, ref bubbleLines);

            RecognitionTools.SetAdditionalOutputData(ref headers, ref totalOutput, ref additionalOutputData, regions, FileName);
        }

        Dictionary<RecognitionTools.Bubble, CheckedBubble> maxCountRectangles = new Dictionary<RecognitionTools.Bubble, CheckedBubble>();

        public void BubblesRecognition()
        {
            Rectangle[] factRectangle = new Rectangle[0];
            Dictionary<RecognitionTools.Bubble, Point[]> allContourMultiLine = new Dictionary<RecognitionTools.Bubble, Point[]>();

            maxCountRectangles = Utils.AddMaxCountRectangles(areas, amoutOfQuestions, indexOfFirstQuestion, linesPerArea);
            if (!barCodesPrompt.StartsWith("Markers"))
            {
                barCodesPrompt = "";
                try
                {
                    RecognitionTools.BubblesRecognize(ref allContourMultiLine, ref factRectangle, Image, ref barCodesPrompt
                                  , filterType, true, bubblesRegions, bubblesOfRegion, bubblesSubLinesCount, bubblesSubLinesStep, bubblesPerLine
                                  , lineHeight, linesPerArea, answersPosition, indexAnswersPosition, totalOutput, bubbleLines, regions, areas
                                  , x1, x2, y1, y2, kx, ky, curRect, etRect, deltaY, amoutOfQuestions, indexOfFirstQuestion, maxCountRectangles);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    NotifyUpdated(NotifyKey.Exception, exception);
                    return;
                }
                for (int k = 0; k < allContourMultiLine.Count; k++)
                {
                    if (!maxCountRectangles.Keys.Contains(allContourMultiLine.ElementAt(k).Key))
                    {
                        break;
                    }
                    maxCountRectangles[allContourMultiLine.ElementAt(k).Key].rectangle = factRectangle[k];
                }
                Utils.FindBubble(areas, ref maxCountRectangles, bubblesRegions, ref bubblesSubLinesStep, bubblesPerLine
                    , bubblesOfRegion, factRectangle, allContourMultiLine, kx, ref barCodesPrompt);
            }
            else
            {
                RecognitionTools.AppendOutput(ref totalOutput, indexAnswersPosition, indexOfFirstQuestion.ToString(), indexAnswersPosition, indexOfFirstBubble);
            }
        }
        public void Recognize1()
        {
        }
        public void Recognize2()
        {
        }
        public void Recognize3()
        {
        }
        #endregion
        public Rectangle[] barCodesRectangle = new Rectangle[0];
        public int[] linesPerArea;
        public int[] bubblesPerLine;
        public int amoutOfQuestions = 0;
        int indexOfFirstQuestion = 1;

        private void ProcessingRegions(ref Regions regions, ref Regions.Area[] areas, ref int regionOutputPosition, ref string barCodesPrompt
    , ref int x1, ref int y1, ref int x2, ref int y2, ref decimal kx, ref decimal ky, ref Rectangle curRect, ref Rectangle etRect, ref int deltaY, Bitmap bmp
    , ref string barcode, ref double filterType, ref object[] totalOutput
    , ref int indexOfFirstBubble, ref int numberOfBubblesRegion, ref Rectangle[] bubblesRegions, ref Rectangle[] bubblesOfRegion, ref int[] bubblesSubLinesCount
    , ref int[] bubblesSubLinesStep, ref int[] lineHeight, ref int answersPosition, ref int indexAnswersPosition, ref string[] bubbleLines)
        {
            for (int num = 0; num < regions.regions.Length; num++)
            {
                #region обработкa регионов processing regions
                string barcodeMem = "";
                string type = regions.regions[num].type;
                bool? active = regions.regions[num].active;
                string name = regions.regions[num].name;
                if (active != false && type != "marker" && name != "sheetIdentifier")
                {
                    areas = regions.regions[num].areas;
                    Regions.Area arr = areas[0];
                    Rectangle rArr = new Rectangle(arr.left, arr.top, arr.width, arr.height);
                    percent_confident_text_region = regions.regions[num].percent_confident_text_region;
                    regionOutputPosition = regions.regions[num].outputPosition;
                    int rotateParameter = regions.regions[num].rotate;
                    if (!barCodesPrompt.StartsWith("Markers"))
                    {
                        x1 = curRect.X + (int)Math.Round((decimal)(arr.left - etRect.X) * kx);
                        y1 = curRect.Y + (int)Math.Round((decimal)(arr.top - etRect.Y) * ky);
                        x2 = curRect.X + (int)Math.Round((decimal)(rArr.Right - etRect.X) * kx);
                        y2 = curRect.Y + (int)Math.Round((decimal)(rArr.Bottom - etRect.Y) * ky);
                        if (name == "lastBanner")
                        {
                            RecognitionTools.LastBannerFind(ref deltaY, bmp, x1, y1, x2, y2);
                        }
                    }
                    else { }
                    if (type.StartsWith("barCode"))
                    {
                        Rectangle rn = new Rectangle(x1, y1, x2 - x1, y2 - y1);
                        Array.Resize(ref barCodesRectangle, barCodesRectangle.Length + 1);
                        barCodesRectangle[barCodesRectangle.Length - 1] = rn;// rArr;
                        if (!barCodesPrompt.StartsWith("Markers"))
                        {
                            barCodesPrompt = "";
                            bool notConfident = false;
                            barcode = RecognitionTools.GetBarCode("", bmp, ref notConfident, ref barCodesPrompt, ref filterType, ref barcodeMem
                                , x1, x2, y1, y2, kx, ky, curRect, etRect, deltaY, regions.regions[num], false//dualControl
                                , percent_confident_text_region, defaults.PercentConfidentText, defaults.FontName, barCodesRectangle);
                        }
                        else
                        {
                            barcode = "";
                            barcodeMem = "";
                        }

                        NotifyUpdated(NotifyKey.FindedBarcodeControll, new string[] { name, arr.type, barcode, barcodeMem });

                        RecognitionTools.SetOutputValues(ref headers, ref headersValues, ref totalOutput, ref allBarCodeNames
                             , ref allBarCodeValues, name, barcode, regionOutputPosition);
                        try
                        {
                            switch (name)
                            {
                                case "amout_of_questions":
                                    amoutOfQuestions = Convert.ToInt32(barcode);
                                    break;
                                case "index_of_first_question":
                                    indexOfFirstQuestion = Convert.ToInt32(barcode);
                                    break;
                            }
                        }
                        catch (Exception)
                        { }
                    }//End if (type.StartsWith("barCode"))
                    else
                        if (type == "bubblesRegions")
                        {
                            indexOfFirstBubble = regions.regions[num].indexOfFirstBubble;
                            numberOfBubblesRegion = num;
                            RecognitionTools.SetSettings(ref bubblesRegions, ref bubblesOfRegion, ref bubblesSubLinesCount, ref bubblesSubLinesStep, ref bubblesPerLine
                                , ref lineHeight, ref linesPerArea, ref answersPosition, ref indexAnswersPosition, ref totalOutput, ref  bubbleLines, regions.regions[num]
                                , x1, x2, y1, y2, kx, ky, curRect, etRect, deltaY);
                        }
                }
                #endregion
            }//конец обработки регионов
        }

        public void IsReady2(Dictionary<int, string>  outputFileValues, eDoctrinaOcrEd.BubblesAreaControl bac, List<eDoctrinaOcrEd.BarCodeControl> barCodeControls)
        {
            var barCodes = "";
            System.Windows.Forms.CheckBox chb = bac.CheckBoxArr[0];
            var tag = chb.Tag;
            RecognitionTools.Bubble[] currentLineBubbles = new RecognitionTools.Bubble[0];
            RecognitionTools.Bubble bubble2 = (RecognitionTools.Bubble)tag;
            int currentLine = bubble2.point.Y;
            int[] subStrSet = new int[areas[0].subLinesAmount];
            totalOutput[answersPosition - 1] = new string[0];
            subStrSet = new int[areas[bubble2.areaNumber].subLinesAmount];

            for (int k = 0; k < bac.CheckBoxArr.Length; k++)
            {
                chb = bac.CheckBoxArr[k];
                bubble2 = (RecognitionTools.Bubble)tag;
                //totalOutput[answersPosition][0] = ;
                if (bubble2.point.Y == currentLine)
                {
                    if (chb.Checked)
                    {
                        Array.Resize(ref currentLineBubbles, currentLineBubbles.Length + 1);
                        currentLineBubbles[currentLineBubbles.Length - 1] = bubble2;
                    }
                    if (k < bac.CheckBoxArr.Length - 1)
                    {
                        continue;
                    }
                }
                if (currentLineBubbles.Length == 0)
                {
                    if (subStrSet.Length > 0 && areas[bubble2.areaNumber].bubblesFormat == "multiple")
                    {
                        for (int n = 0; n < subStrSet.Length; n++)
                        {
                            subStrSet[n] = n;
                            RecognitionTools.AppendOutput(ref totalOutput, answersPosition, "~", indexAnswersPosition, indexOfFirstBubble);
                        }
                    }
                    else
                    {
                        RecognitionTools.AppendOutput(ref totalOutput, answersPosition, "", indexAnswersPosition, indexOfFirstBubble);
                    }
                }
                else
                {
                    for (int j = 0; j < currentLineBubbles.Length; j++)
                    {
                        RecognitionTools.Bubble b2 = currentLineBubbles[j];
                        KeyValuePair<RecognitionTools.Bubble, double> item1 = new KeyValuePair<RecognitionTools.Bubble, double>(b2, 0);
                        RecognitionTools.AppendOutput(ref totalOutput, answersPosition, indexAnswersPosition, item1
                                       , j, ref subStrSet, areas[item1.Key.areaNumber], bubblesPerLine[b2.areaNumber], indexOfFirstBubble);
                    }
                }
                if (k < bac.CheckBoxArr.Length - 1)
                {
                    Array.Resize(ref currentLineBubbles, 0);
                    currentLine = bubble2.point.Y;
                    RecognitionTools.AppendOutput(ref totalOutput, indexAnswersPosition, currentLine.ToString(), indexAnswersPosition, indexOfFirstBubble);
                    subStrSet = new int[areas[bubble2.areaNumber].subLinesAmount];
                    k--;
                }
            }
            try
            {
                string[] str = totalOutput[indexAnswersPosition - 1] as string[];
                int first = Convert.ToInt32(str[0]);
                if (first != indexOfFirstQuestion)
                {
                    for (int k = 0; k < str.Length; k++)
                    {
                        str[k] = (indexOfFirstQuestion + k).ToString();
                    }
                    totalOutput[indexAnswersPosition - 1] = str;
                }
            }
            catch (Exception) { }
            var direct = Defaults.ManualSuccessFolder;
            foreach (KeyValuePair<int, string> item in outputFileValues)
            {
                if (item.Key > totalOutput.Length)
                {
                    Array.Resize(ref totalOutput, item.Key);
                }
                totalOutput[item.Key - 1] = item.Value;
            }
            for (int num = 0; num < regions.regions.Length; num++)
            {
                string type = regions.regions[num].type;
                bool? active = regions.regions[num].active;
                string name = regions.regions[num].name;

                if (active == false || type == "marker" || name == "sheetIdentifier")
                {
                    continue;
                }
                regionOutputPosition = regions.regions[num].outputPosition;
                if (regionOutputPosition > 0)
                {
                    foreach (var item in barCodeControls)
                    {
                        if (item.Name == name)
                        {
                            headersValues[regionOutputPosition - 1] = item.barCodeValue;
                            totalOutput[regionOutputPosition - 1] = item.barCodeValue;
                            break;
                        }
                    }
                }
            }
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
                    if (outputFileValues.Keys.Contains(k))
                    {
                        allBarCodeValues[k] = outputFileValues[k];
                    }
                }
                outputFileNameFormat = outputFileNameFormat.Replace
                    ("{" + allBarCodeNames[k] + "}", allBarCodeValues[k]);
            }
            var destFileName = direct + "\\" + outputFileNameFormat + ".csv";
            Utils.CreateDirectory(direct);
            destFileName = destFileName.Replace(@"\\", @"\");
            destFileName = Utils.GetNextFileName(destFileName);

            string destFileNameTiff;
            destFileNameTiff = Utils.GetNextFileName(direct + "\\" + outputFileNameFormat + ".tiff");
            //b.Save(destFileNameTiff, ImageFormat.Tiff);
            destFileNameTiff = destFileNameTiff.Replace(@"\\", @"\");
        repeat: try
            {
                File.Move(FileName, destFileNameTiff);
            }
            catch (Exception ex)
            {
                goto repeat;
            }
            var currentFileName = destFileNameTiff;
            var fi = new FileInfo(FileName);
            var fn1 = new FileInfo(FileName).Name;
            var fn2 = fn1.Replace(new FileInfo(FileName).Extension, ".audit");
            Utils.DeleteFile(Defaults.TempEdFolder + "\\" + fn2);
            StringBuilder[] sb = new StringBuilder[1];
            RecognitionTools.WriteCsv(false, destFileName, barCodesPrompt, sb, totalOutput
                 , answersPosition, indexAnswersPosition, additionalOutputData, barCodes);

        }
    }
}
