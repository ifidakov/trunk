using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eDoctrinaUtils
{
    public class IOHelper
    {
        Log log = new Log();

        //-------------------------------------------------------------------------
        public void CreateDirectory(params string[] dirName)
        {
            foreach (var temp in dirName)
            {
                CreateDirectory(temp);
            }
        }
        //-------------------------------------------------------------------------
        public bool CreateDirectory(string dirName, bool showLog = true)
        {
            if (!Directory.Exists(dirName))
            {
                try
                {
                    Directory.CreateDirectory(dirName);
                }//-2147024893
                catch (Exception)
                {
                    if (showLog)
                    {
                        var message = "Could not find part of the path \"" + dirName + "\"";
                        log.LogMessage(message);
                    }
                    return false;
                }
            }
            return true;
        }
        //-------------------------------------------------------------------------
       public bool DeleteDirectory(string dirName, bool showLog = true)
        {
            if (Directory.Exists(dirName))
            {
                try
                {
                    Directory.Delete(dirName);
                }
                catch (Exception ex)
                {
                    if (showLog)
                    {
                        log.LogMessage(ex);
                    }
                    return false;
                }
            }
            return true;
        }
        //-------------------------------------------------------------------------
        public void DeleteFile(string fileName)
        {
            DeleteFile(false, fileName);
        }
        //-------------------------------------------------------------------------
        public bool DeleteFile(bool deleteToRecycleBin, string fileName)
        {
            if (File.Exists(fileName))
            {
                try
                {
                    if (deleteToRecycleBin)
                    {
                        RecycleBin.DeleteToRecycleBin(fileName, RecycleBin.FileOperationFlags.FOF_WANTNUKEWARNING);
                        log.LogMessage("File was moved to RecycleBin : " + fileName);//Log for Editor
                    }
                    else
                    {
                        File.Delete(fileName);
                    }
                }
                catch (Exception ex)
                {
                    log.LogMessage(ex.Message);
                    return false;
                }
            }
            return true;
        }
        //-------------------------------------------------------------------------
        public List<string> ReDeleteList = new List<string>();
        public void DeleteFileExt(bool deleteToRecycleBin, string fileName)
        {
            DeleteFileExt(deleteToRecycleBin, fileName, true);
        }
        //-------------------------------------------------------------------------
        public void DeleteFileExt(bool deleteToRecycleBin, string fileName, bool showLog)
        {
            lock (ReDeleteList)
            {
                if (File.Exists(fileName))
                {
                    try
                    {
                        if (deleteToRecycleBin)
                        {
                            RecycleBin.DeleteToRecycleBin(fileName, RecycleBin.FileOperationFlags.FOF_WANTNUKEWARNING);
                            if (showLog) log.LogMessage("File was moved to RecycleBin : " + fileName);//Log for Editor
                        }
                        else
                        {
                            File.Delete(fileName);
                        }
                        if (ReDeleteList.Contains(fileName))
                            ReDeleteList.Remove(fileName);
                    }
                    catch (Exception ex)
                    {
                        if (showLog) log.LogMessage(ex.Message);
                        if (!ReDeleteList.Contains(fileName))
                            ReDeleteList.Add(fileName);
                    }
                }
                else
                {
                    if (ReDeleteList.Contains(fileName))
                        ReDeleteList.Remove(fileName);
                }
            }
        }
        //-------------------------------------------------------------------------
        public void ReDelete()
        {
            Task.Factory.StartNew(() => { Delete(); }).ContinueWith((t) =>
                {
                    if (t.Exception != null) log.LogMessage(t.Exception);
                });
        }
        //-------------------------------------------------------------------------
        public void Delete()
        {
            for (int i = 0; i < ReDeleteList.Count; i++)
            {
                var fileName = ReDeleteList[i];
                DeleteFileExt(true, fileName, false);
                if (!ReDeleteList.Contains(fileName)) i--;
            }
        }
    }
}
