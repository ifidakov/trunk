using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace eDoctrinaUtils
{
    public class ProcessingTempFrameFile
    {
        private OcrAppConfig defaults;
        private CancellationToken cancellationToken;
        private string fileName;
        private string lastSheetIdentifier;
        private List<string> errorFiles;
        private IOHelper ioHelper;
        Utils utils = new Utils();
        Log log = new Log();

        //-------------------------------------------------------------------------
        public ProcessingTempFrameFile(CancellationToken cancellationToken, OcrAppConfig defaults, string fileName, List<string> errorFiles, IOHelper ioHelper, ref string lastSheetId)
        {
            this.defaults = defaults;
            this.fileName = fileName;
            this.errorFiles = errorFiles;
            this.ioHelper = ioHelper;
            this.cancellationToken = cancellationToken;
            this.lastSheetIdentifier = lastSheetId;
            string shId;
            Working(out shId);
            lastSheetId = shId;
        }
        //-------------------------------------------------------------------------
        private void Working(out string shId)
        {
            shId = "";
            var fileName2 = utils.GetFileForRecognize(fileName, OcrAppConfig.TempFramesFolder, false);
            if (fileName != fileName2)
            {
                log.LogMessage("___" + fileName + " convert to " + fileName2);
                fileName = fileName2;
            }
            if (String.IsNullOrEmpty(fileName))
            {
                log.LogMessage("___" + "Incorrect fileName in ProcessingTempFrameFile.DoWork");
                return;
            }
            var name = Path.GetFileName(fileName);

            //Recognize rec = new Recognize(fileName, defaults, cancellationToken, true, false);//для нормализации в основном потоке
            Recognize rec = new Recognize(fileName, defaults, cancellationToken, true, true);//для нормализации в фоновых потоках

            rec.LastSheetIdentifier = lastSheetIdentifier;
            //if (defaults.MoveToNextProccessingFolderOnSheetIdentifierError)
            //{
            //    utils.MoveBadFile(OcrAppConfig.TempFramesFolder, defaults.ManualNextProccessingFolder, fileName,rec. Audit, rec.Exception.Message);//сохраняем файл в папку для дальнейшей обработки и удаляем из ТЕМПА
            //    File.Delete(Path.Combine(OcrAppConfig.TempFramesFolder, fileName));

            //}
            //else
            //{
            //    errorFiles.Add(fileName);
            //}

            if (rec.Exception != null)
            {
                log.LogMessage(rec.Exception);
                return;
            }
            string qrCodeText = "";
            if (!cancellationToken.IsCancellationRequested)
            {
                log.LogMessage("Start " + name + " Page " + rec.Audit.sourcePage.ToString());
                rec.RecognizeAuto(ref qrCodeText);
            }
            if (!cancellationToken.IsCancellationRequested)
            {
                if (rec.Exception == null)
                {
                    var auditFileName = utils.GetFileAuditName(fileName);
                    ioHelper.DeleteFileExt(false, fileName, false);//!!!при "true" -  Процесс не может получить доступ к файлу
                    ioHelper.DeleteFileExt(false, auditFileName);
                    log.LogMessage("Finish " + name + " Page " + rec.Audit.sourcePage.ToString());
                }
                else
                {
                    log.LogMessage(rec.Exception);
                    lock (errorFiles)
                    {
                        if (errorFiles.Contains(fileName))
                        {
                            //utils.MoveBadFile(OcrAppConfig.TempFramesFolder, defaults.ManualNextProccessingFolder, fileName, rec.Audit, rec.Exception.Message);//сохраняем файл в папку для дальнейшей обработки и удаляем из ТЕМПА
                            //log.LogMessage("File " + name + " was moved to '" + defaults.ManualNextProccessingFolder + "'");
                            utils.MoveBadFile(OcrAppConfig.TempFramesFolder, defaults.ErrorFolder, fileName, rec.Audit, rec.Exception.Message);//сохраняем файл в папку для дальнейшей обработки и удаляем из ТЕМПА
                            log.LogMessage("File " + name + " was moved to '" + defaults.ErrorFolder + "'");

                            errorFiles.Remove(fileName);
                        }
                        else
                        {
                            errorFiles.Add(fileName);
                            log.LogMessage("File " + name + " will be restarted");
                        }
                    }
                }
            }
            shId = rec.LastSheetIdentifier;
        }
    }
}
