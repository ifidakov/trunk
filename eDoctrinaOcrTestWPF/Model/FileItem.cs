using System;
using System.IO;
using System.Linq;

namespace eDoctrinaOcrTestWPF
{
    public class FileItem : EtalonFileItem
    {
        public FileItem()
        { }
        //-------------------------------------------------------------------------
        public FileItem(FileItem item, VerifyFiles newState, FileItem etalonItem)
        {
            SourcePage = item.SourcePage;
            SourceSha1 = item.SourceSha1;
            Error = item.Error;

            FilePath = item.FilePath;
            UniqueName = item.UniqueName;
            CorrectFileName = item.CorrectFileName;
            DataSha1 = item.DataSha1;
            FrameSha1 = item.FrameSha1;
            State = newState;
            AutorName = (newState == VerifyFiles.extra) ? "" : etalonItem.AutorName;

            this.etalonItem = etalonItem;
            if (State == VerifyFiles.error)
            {
                EtalonDataSha1 = (DataSha1 == etalonItem.DataSha1) ? EtalonDataSha1 : etalonItem.DataSha1;
                EtalonError = (Error == etalonItem.Error) ? EtalonError : etalonItem.Error;
                EtalonCorrectFileName = (CorrectFileName == etalonItem.CorrectFileName) ? EtalonCorrectFileName : etalonItem.CorrectFileName;
            }
        }
        //-------------------------------------------------------------------------
        public FileItem(string file)
        {
            FilePath = Path.GetDirectoryName(file);
            UniqueName = Path.GetFileNameWithoutExtension(file);
            CorrectFileName = UniqueName.Split('-').First();

            var fullName = file;

            fullName = Path.ChangeExtension(fullName, ".csv");
            if (!File.Exists(fullName)) State = VerifyFiles.missingCsv;
            var sha1Csv = Utils.GetSHA1FromFile(fullName);

            fullName = Path.ChangeExtension(fullName, ".tiff");
            if (!File.Exists(fullName)) State = VerifyFiles.missingTiff;

            fullName = Path.ChangeExtension(fullName, ".audit");
            if (!File.Exists(fullName)) State = VerifyFiles.missingAudit;

            if (State != VerifyFiles.missingAudit)
            {
                Exception exception;
                audit = new eDoctrinaUtils.Audit(fullName, out exception);

                if (audit != null)
                {
                    SourcePage = audit.sourcePage.ToString();
                    SourceSha1 = audit.sourceSHA1Hash;
                    DataSha1 = (String.IsNullOrEmpty(audit.dataFileSHA1Hash)) ? "" : audit.dataFileSHA1Hash;
                    FrameSha1 = audit.SHA1Hash;
                    Error = audit.error;

                    SourceFilePath = audit.sourceFilePath;
                    SourceFileName = audit.sourceFileName;
                }
                if (DataSha1 != sha1Csv)
                    State = VerifyFiles.wrongDataSha1;
            }
        }
        //-------------------------------------------------------------------------
        public FileItem etalonItem;

        public eDoctrinaUtils.Audit audit { get; private set; }
        public string SourceFilePath { get; private set; }
        public string SourceFileName { get; set; }
        public string FilePath { get; private set; }
        public string UniqueName { get; set; }
        public VerifyFiles State { get; set; }
        public string ShowFileName
        {
            get
            {
                return (State == VerifyFiles.missing) ? CorrectFileName : UniqueName;
            }
        }

        public string UniqueIdentifier
        {
            get { return SourceSha1 + SourcePage; }
        }
        public string EtalonDataSha1 { get; private set; }
        public string EtalonError { get; private set; }
        public string EtalonCorrectFileName { get; private set; }

        public string GetFullFileName()
        {
            if (!String.IsNullOrEmpty(FrameSha1))
                return Path.ChangeExtension(Path.Combine(FilePath, UniqueName), ".tiff");
            return Path.ChangeExtension(Path.Combine(FilePath, UniqueName), ".csv");
        }
        //-------------------------------------------------------------------------
        public override string ToString()
        {
            switch (State)
            {
                case VerifyFiles.empty:
                    return "";
                case VerifyFiles.ok:
                    return "ok";
                case VerifyFiles.missing:
                    return "missing";
                case VerifyFiles.extra:
                    return "extra";
                case VerifyFiles.error:
                    return "Doesn't match";
                case VerifyFiles.missingTiff:
                    return "missing tiff file";
                case VerifyFiles.missingCsv:
                    return "missing csv file";
                case VerifyFiles.wrongDataSha1:
                    return "Data sha1 doesn't match";
            }
            return State.ToString();
        }
    }
    //-------------------------------------------------------------------------
    //-------------------------------------------------------------------------
    //-------------------------------------------------------------------------

}
