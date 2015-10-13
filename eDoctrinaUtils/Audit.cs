using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace eDoctrinaUtils
{
    [Serializable]
    public class Audit
    {
        public string fileName { get; set; }
        public string SHA1Hash { get; set; }
        public string type { get; set; }
        public string error { get; set; }
        public string dataFileName { get; set; }
        public string dataFileSHA1Hash { get; set; }
        public string sourceFilePath { get; set; }
        public string sourceFileName { get; set; }
        public string sourceSHA1Hash { get; set; }
        public string archiveFileName { get; set; }
        public int sourcePage { get; set; }
        Utils utils = new Utils();
        IOHelper iOHelper = new IOHelper();
        //-------------------------------------------------------------------------
        public Audit()
        { }
        //-------------------------------------------------------------------------
        public Audit(string fileName, string fileSHA1Hash)
        {
            var fi = new FileInfo(fileName);
            this.type = "inProgress";
            this.sourceFileName = fi.Name;
            this.sourceFilePath = fi.Directory.ToString();
            this.sourceSHA1Hash = fileSHA1Hash;
            this.SHA1Hash = fileSHA1Hash;
            this.fileName = fi.Name;
        }
        //-------------------------------------------------------------------------
        public Audit(string filePath, out Exception exception)
        {
            exception = null;
            var serializer = new SerializerHelper();
            var audit = serializer.GetAuditFromFile(filePath, out exception);
            if (audit != null)
            {
                this.fileName = audit.fileName;
                this.SHA1Hash = audit.SHA1Hash;
                this.type = audit.type;
                this.error = audit.error;
                this.dataFileName = audit.dataFileName;
                this.dataFileSHA1Hash = audit.dataFileSHA1Hash;
                this.sourceFilePath = audit.sourceFilePath;
                this.sourceFileName = audit.sourceFileName;
                this.sourceSHA1Hash = audit.sourceSHA1Hash;
                this.archiveFileName = audit.archiveFileName;
                this.sourcePage = audit.sourcePage;
            }
        }
        //-------------------------------------------------------------------------
        public Audit Clone()
        {
            var newAudit = new Audit()
            {
                fileName = this.fileName,
                SHA1Hash = this.SHA1Hash,
                type = this.type,
                error = this.error,
                dataFileName = this.dataFileName,
                dataFileSHA1Hash = this.dataFileSHA1Hash,
                sourceFilePath = this.sourceFilePath,
                sourceFileName = this.sourceFileName,
                sourceSHA1Hash = this.sourceSHA1Hash,
                archiveFileName = this.archiveFileName,
                sourcePage = this.sourcePage
            };
            return newAudit;
        }
        //-------------------------------------------------------------------------
        public void Save(string filePath)
        {
            var serializer = new SerializerHelper();
            serializer.SaveToFile(this, filePath, Encoding.ASCII);
        }
        //-------------------------------------------------------------------------
        public void MadeArchiveAudit(string archiveFolder)
        {
            this.type = "archived";
            this.SHA1Hash = sourceSHA1Hash;
            this.archiveFileName = GetArchiveFileName(sourceFileName, sourceSHA1Hash, archiveFolder);
        }
        //-------------------------------------------------------------------------
        public void MadeFailedAudit(string exMessage)
        {
            this.type = "failed";
            this.error = exMessage;
        }
        //-------------------------------------------------------------------------
        public Audit GetFrameAudit(string frameFileName, string frameSHA1Hash, int framePageNumber)
        {
            var audit = this.Clone();
            audit.type = "inProgress";
            audit.fileName = Path.GetFileName(frameFileName);
            audit.SHA1Hash = frameSHA1Hash;
            audit.sourcePage = framePageNumber;
            return audit;
        }
        //-------------------------------------------------------------------------
        public Audit GetFinalAuditForAuto(bool errorFrame, string fileName, string dataFileName, string error)
        {
            var audit = this.Clone();
            audit.fileName = Path.GetFileName(fileName);
            if (errorFrame)
            {
                audit.type = "failed";
                audit.error = error;
            }
            else
            {
                audit.type = "processedAutomatically";
                audit.dataFileSHA1Hash = utils.GetSHA1FromFile(dataFileName);
                audit.dataFileName = Path.GetFileName(dataFileName);
            }
            return audit;
        }
        //-------------------------------------------------------------------------
        public Audit GetFinalAuditForManual(string fileName, string dataFileName, string dataSHA1Hash)
        {
            var audit = this.Clone();
            audit.error = null;
            audit.type = "processedManually";
            audit.dataFileSHA1Hash = dataSHA1Hash;
            audit.dataFileName = Path.GetFileName(dataFileName);
            audit.fileName = Path.GetFileName(fileName);
            return audit;
        }
        //-------------------------------------------------------------------------
        #region private Helpers
        //-------------------------------------------------------------------------
        private string GetArchiveFileName(string fileName, string sourceSHA1Hash, string archiveFolder)
        {
            string archiveFileName = archiveFolder + DateTime.Today.Date.ToString("yyyy-MM-dd") + "\\";
            iOHelper.CreateDirectory(archiveFileName);
            archiveFileName += sourceSHA1Hash + Path.GetExtension(fileName);
            return archiveFileName;
        }
        #endregion
    }
}