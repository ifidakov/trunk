using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace eDoctrinaUtils
{
    public partial class Utils
    {
        IOHelper iOHelper = new IOHelper();
        Log log = new Log();

        public string GetFileAuditName(string fileName)
        {
            return Path.ChangeExtension(fileName, ".audit");
        }
        //-------------------------------------------------------------------------
        public bool CanAccess(string fileName)
        {
            try
            {
                var temp1 = File.OpenRead(fileName);
                temp1.Close();
                return true;
            }
            catch (Exception)//ex
            {
                //Log.LogMessage(ex.Message);
                return false;
            }
        }
        //-------------------------------------------------------------------------
        public string[] GetSupportedFilesFromDirectory(string directory, SearchOption searchOption)
        {
            return GetSupportedFilesFromDirectory(directory, searchOption, true);
        }
        //-------------------------------------------------------------------------
        public string[] GetSupportedFilesFromDirectory(string directory, SearchOption searchOption, bool sorting)
        {
            if (Directory.Exists(directory))
            {
                try
                {
                    var files = Directory.GetFiles(directory, "*.*", searchOption)
                        .Where(s => OcrAppConfig.SupportedExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
                    if (sorting)
                    {
                        files = files.OrderBy(x => File.GetCreationTime(x)).ToArray();//GetLastAccessTime 
                        //if (directory != OcrAppConfig.TempFramesFolder)
                        //{
                        //    Array.Reverse(files);
                        //}
                    }
                    return files;
                }
                catch { }
            }
            return new string[0];
        }
        //-------------------------------------------------------------------------
        public string CalcCRC16(string strInput)
        {
            ushort crc = 0x0000;
            byte[] data = GetBytesFromHexString(strInput);
            for (int i = 0; i < data.Length; i++)
            {
                crc ^= (ushort)(data[i] << 8);
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x8000) > 0)
                        crc = (ushort)((crc << 1) ^ 0x8005);
                    else
                        crc <<= 1;
                }
            }
            return crc.ToString("X4");
        }
        //-------------------------------------------------------------------------
        public Byte[] GetBytesFromHexString(string strInput)
        {
            Byte[] bytArOutput = new Byte[] { };
            if (!string.IsNullOrEmpty(strInput) && strInput.Length % 2 == 0)
            {
                System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary hexBinary = null;
                try
                {
                    hexBinary = System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary.Parse(strInput);
                    if (hexBinary != null)
                    {
                        bytArOutput = hexBinary.Value;
                    }
                }
                catch (Exception) //ex
                {
                    //MessageBox.Show(ex.Message);
                }
            }
            return bytArOutput;
        }
        //-------------------------------------------------------------------------
        public string GetSHA1FromFile(string destFileName)
        {
            string sha1hash = "";
            try
            {
                using (FileStream stream = File.OpenRead(destFileName))
                {
                    SHA1Managed sha = new SHA1Managed();
                    byte[] hash = sha.ComputeHash(stream);
                    sha1hash = BitConverter.ToString(hash).Replace("-", String.Empty);
                }
            }
            catch (Exception)
            {
                return "";
            }
            return sha1hash;
        }
        //-------------------------------------------------------------------------
        public Audit CreateAuditAndMoveToTempFolder(string fileName, string directory, bool delete = true)
        {
            string sourceSHA1Hash = GetSHA1FromFile(fileName);
            var tempFileName = directory + sourceSHA1Hash + Path.GetExtension(fileName);
            var tempAuditFileName = directory + sourceSHA1Hash + ".audit";
            Audit audit = new Audit(fileName, sourceSHA1Hash);
            if (CanAccess(fileName))
            {
                File.Copy(fileName, tempFileName, true);
                if (delete)
                {
                    iOHelper.DeleteFile(fileName);
                }
                audit.Save(tempAuditFileName);
            }
            return audit;
        }
        //-------------------------------------------------------------------------
        public string GetFileForRecognize(string fileName, string tempdirectory, bool showLog = true)
        {
            //bool useErrFolder = false;
            if (string.IsNullOrEmpty(tempdirectory))
            {
                //useErrFolder = true;
                tempdirectory = OcrAppConfig.TempEdFolder;
            }
            if (showLog) log.LogMessage("Get file for recognize: " + fileName);
            if (!File.Exists(fileName))
            {
                if (showLog) log.LogMessage("File does not exist " + fileName);
                return null;
            }
            string fileNameAudit = GetFileAuditName(fileName);
            var sha1hash = GetSHA1FromFile(fileName);
            if (File.Exists(fileNameAudit) && fileName.Contains(sha1hash) && fileName.Contains(tempdirectory))
            {
                return fileName;
            }
            if (fileName.Contains(sha1hash) && fileName.Contains(tempdirectory))
            {
                iOHelper.DeleteFile(fileNameAudit);
                var audit = new Audit(fileName, GetSHA1FromFile(fileName));
                audit.Save(GetFileAuditName(fileName));
                return fileName;
            }
            string destfileName = tempdirectory + sha1hash + Path.GetExtension(fileName);
            string destfileNameAudit = GetFileAuditName(destfileName);//The file exists.
            if (File.Exists(fileNameAudit))// && fileName.Contains(sha1hash)
            {
                //try
                //{
                //    File.Move(fileName, destfileName);
                //    File.Move(fileNameAudit, destfileNameAudit);
                //}
                //catch (Exception ex)
                //{
                //    log.LogMessage(ex);
                //    return null;
                //}
                try
                {
                    if (!File.Exists(destfileName))
                    {
                        File.Move(fileName, destfileName);
                    }
                    else
                    {
                        File.Delete(fileName);
                    }
                }
                catch (Exception ex)
                {
                    log.LogMessage(ex);
                }
                try
                {
                    if (!File.Exists(destfileNameAudit))
                    {
                        File.Move(fileNameAudit, destfileNameAudit);
                    }
                    else
                    {
                        File.Delete(fileNameAudit);
                    }
                }
                catch (Exception ex)
                {
                    log.LogMessage(ex);
                }
                return destfileName;
            }
            else
            {
                log.LogMessage("File " + fileNameAudit + " not exists");
                FileInfo fi = new FileInfo(fileName);
                DateTime begEx = DateTime.Parse(fi.CreationTime.ToString());
                TimeSpan ts = DateTime.Now - begEx;
                if (ts.TotalSeconds < 2)
                    return "";
            }
            CreateAuditAndMoveToTempFolder(fileName, tempdirectory);
            return destfileName;
        }
        //-------------------------------------------------------------------------
        public void MoveBadFile(string dir, string ManualNextProccessingFolder, string fileName, Audit audit, string exMessage)
        {
            if (!Directory.Exists(ManualNextProccessingFolder))
            {
                Directory.CreateDirectory(ManualNextProccessingFolder);
            }
            audit.MadeFailedAudit(exMessage);
            var destFileName = ManualNextProccessingFolder + audit.sourceSHA1Hash + Path.GetExtension(fileName);
            MoveFileFromTemp(dir, fileName, audit, destFileName);
        }
        //-------------------------------------------------------------------------
        public void MoveFileFromTemp(string dir, string incomingFile, Audit audit, string destFileName)
        {
            string tempFileName = dir + Path.GetFileName(incomingFile);
            string tempAuditFileName = GetFileAuditName(tempFileName);
            string destAuditFileName = GetFileAuditName(destFileName);
            try
            {
                audit.Save(destAuditFileName);
                File.Copy(tempFileName, destFileName, true);
                iOHelper.DeleteFile(tempAuditFileName);
                iOHelper.DeleteFile(tempFileName);
            }
            catch (Exception ex)
            {
                log.LogMessage(ex);
            }
        }
        //-------------------------------------------------------------------------
        public bool IsNumeric(string str)
        {
            long retNum;
            return IsNumeric(str, out retNum);
        }
        //-------------------------------------------------------------------------
        public bool IsNumeric(string str, out long retNum)
        {
            var isNum = Int64.TryParse(str, out retNum);
            return isNum;
        }
        //-------------------------------------------------------------------------
        public bool IsNumeric(string str, out int retNum)
        {
            var isNum = Int32.TryParse(str, out retNum);
            return isNum;
        }
        //-------------------------------------------------------------------------
    }

}