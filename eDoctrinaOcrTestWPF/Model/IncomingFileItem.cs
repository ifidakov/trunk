using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace eDoctrinaOcrTestWPF
{
    public class IncomingFileItem
    {
        //-------------------------------------------------------------------------
        public IncomingFileItem(string path)
        {
            Working(new List<string> { path }, SearchOption.AllDirectories);
        }
        //-------------------------------------------------------------------------
        public IncomingFileItem(List<string> sourcePaths)
        {
            Working(sourcePaths, SearchOption.TopDirectoryOnly);
        }
        //-------------------------------------------------------------------------
        private void Working(List<string> sourcePaths, SearchOption searchOption)
        {
            AllFiles = new List<string>();
            foreach (var path in sourcePaths)
            {
                AllFiles.AddRange(Utils.GetSupportedFilesFromDirectory(path, searchOption, ".csv|.tiff|.audit").ToList());
            }
            CsvFiles = AllFiles.FindAll(x => Path.GetExtension(x) == ".csv").ToList();
            TiffFiles = AllFiles.FindAll(x => Path.GetExtension(x) == ".tiff").ToList();
            AuditFiles = AllFiles.FindAll(x => Path.GetExtension(x) == ".audit").ToList();
            UniqueNames = new List<string>();
            AllFiles.ForEach((string name) => { UniqueNames.Add(Path.ChangeExtension(name, "")); });
            UniqueNames = UniqueNames.Distinct().ToList();
        }
        //-------------------------------------------------------------------------
        private List<string> uniqueNames;
        public List<string> UniqueNames
        {
            get { return uniqueNames; }
            private set { uniqueNames = value; }
        }
        //-------------------------------------------------------------------------
        private List<string> allFiles;
        public List<string> AllFiles
        {
            get { return allFiles; }
            private set { allFiles = value; }
        }
        //-------------------------------------------------------------------------
        private List<string> csvFiles;
        public List<string> CsvFiles
        {
            get { return csvFiles; }
            private set { csvFiles = value; }
        }
        //-------------------------------------------------------------------------
        private List<string> tiffFiles;
        public List<string> TiffFiles
        {
            get { return tiffFiles; }
            private set { tiffFiles = value; }
        }
        //-------------------------------------------------------------------------
        private List<string> auditFiles;
        public List<string> AuditFiles
        {
            get { return auditFiles; }
            private set { auditFiles = value; }
        }
        //-------------------------------------------------------------------------
        //-------------------------------------------------------------------------
        //-------------------------------------------------------------------------
        //-------------------------------------------------------------------------
    }
}
