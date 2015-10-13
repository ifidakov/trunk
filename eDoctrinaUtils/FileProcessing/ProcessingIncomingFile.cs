using eDoctrinaUtils;
using System;
using System.IO;

namespace eDoctrinaUtils
{
    public class ProcessingIncomingFile
    {
        public ProcessingIncomingFileResult Result;
        private OcrAppConfig defaults;
        Utils utils = new Utils();
        IOHelper iOHelper = new IOHelper();
        Log log = new Log();

        //-------------------------------------------------------------------------
        public ProcessingIncomingFile(OcrAppConfig defaults, System.Collections.Generic.List<string> filesInInputOrTempFolder, bool isInputFolder)
        {
            this.defaults = defaults;
            foreach (var file in filesInInputOrTempFolder)
            {
                log.LogMessage("___" + "bgwForIncomingFile_DoWork file = " + file);
                if (utils.CanAccess(file))
                {
                    switch (isInputFolder.ToString().ToLower())
                    {
                        case "true":
                            FileInInputFolder(file);
                            break;
                        case "false":
                            FileInTempFolder(file);
                            break;
                    }
                    break;
                }
                else
                {
                    log.LogMessage("___" + "Can not access to file: " + file);
                }
            }
        }
        //-------------------------------------------------------------------------
        private void FileInInputFolder(string fileName)
        {
            if (defaults.DoNotProcess)
            {
                FileInfo fi = new FileInfo(fileName);
                string s = Path.Combine("TempIncFile", fi.Name);
                //if (!fi.Exists)
                //{
                fi.CopyTo(s, true);
                //}
            }

            var tempAudit = utils.CreateAuditAndMoveToTempFolder(fileName, OcrAppConfig.TempFolder);//переместили в папку темп тиф файл + создали аудит файл
            var tempFileName = OcrAppConfig.TempFolder + tempAudit.sourceSHA1Hash + Path.GetExtension(tempAudit.sourceFileName);
            Result = new ProcessingIncomingFileResult()
            {
                SourceFileName = fileName,
                FileName = tempFileName,
                Audit = tempAudit
            };
        }
        //-------------------------------------------------------------------------
        private void FileInTempFolder(string fileName)
        {
            Exception exception;
            var auditFileName = utils.GetFileAuditName(fileName);
            Audit sourceAudit = new Audit(auditFileName, out exception);
            if (sourceAudit == null || !File.Exists(auditFileName))
            {
                log.LogMessage("___" + "FileInTempFolder (sourceAudit == null) : true");
                if (exception != null)
                    log.LogMessage(exception);
                File.Copy(fileName, defaults.InputFolder + Path.GetFileName(fileName), true);//если файл без файла-аудита -> перемещение в папку InputFolder
                iOHelper.DeleteFile(fileName);
                return;
            }
            Result = new ProcessingIncomingFileResult()
            {
                SourceFileName = fileName,
                FileName = fileName,
                Audit = sourceAudit
            };
        }
        //-------------------------------------------------------------------------
        public int GetFrames()
        {
            Result.Audit.MadeArchiveAudit(defaults.ArchiveFolder);//сделали аудит файл для архива
            FramesAndBitmap fab = new FramesAndBitmap();
            fab.GetFrames(Result.FileName, Result.Audit);//разбило на фреймы (выделило странички по отдельным файлам в темпфрейм фолдер
            if (fab.Exception != null)
            {
                log.LogMessage(fab.Exception);
                //utils.MoveBadFile(OcrAppConfig.TempFolder, defaults.ManualNextProccessingFolder, Result.FileName, Result.Audit, fab.Exception.Message);//сохраняем файл в папку для дальнейшей обработки и удаляем из ТЕМПА
                //log.LogMessage("File " + Path.GetFileName(Result.FileName) + " was moved to '" + defaults.ManualNextProccessingFolder + "'");
                utils.MoveBadFile(OcrAppConfig.TempFolder, defaults.ErrorFolder, Result.FileName, Result.Audit, fab.Exception.Message);//сохраняем файл в папку для дальнейшей обработки и удаляем из ТЕМПА
                log.LogMessage("File " + Path.GetFileName(Result.FileName) + " was moved to '" + defaults.ErrorFolder + "'");
            }
            else
            {
                utils.MoveFileFromTemp(OcrAppConfig.TempFolder, Result.FileName, Result.Audit, Result.Audit.archiveFileName); //сохраняем файл в архив и удаляем из ТЕМПА
            }
            return fab.PageCount;
        }
    }

    public class ProcessingIncomingFileResult
    {
        public string SourceFileName { get; set; }
        public string FileName { get; set; }
        public Audit Audit { get; set; }
    }
}
