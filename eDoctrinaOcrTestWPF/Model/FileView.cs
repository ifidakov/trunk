using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace eDoctrinaOcrTestWPF
{
    public class FileView : EtalonFileView
    {
        #region EventHandler
        public event EventHandler WorkingCompleted;

        private void NotifyUpdated(EventHandler handler, object obj, EventArgs e)
        {
            if (handler != null) handler(obj, e);
        }
        #endregion

        public FileView()
        { }

        private IncomingFileItem inFiles;
        public IncomingFileItem InFiles
        {
            get { return inFiles; }
            private set { inFiles = value; }
        }

        private List<FileItem> files = new List<FileItem>();
        public List<FileItem> Files
        {
            get { return files; }
            private set { files = value; }
        }

        private List<FileItem> duplicateFiles = new List<FileItem>();
        public List<FileItem> DuplicateFiles
        {
            get { return resultFiles; }
            private set { resultFiles = value; }
        }

        private List<FileItem> etalonFiles = new List<FileItem>();
        public List<FileItem> EtalonFiles
        {
            get { return etalonFiles; }
            private set { etalonFiles = value; }
        }

        private List<FileItem> resultFiles = new List<FileItem>();
        public List<FileItem> ResultFiles
        {
            get { return resultFiles; }
            private set { resultFiles = value; }
        }

        public int CountAllFiles { get { return InFiles.AllFiles.Count; } }
        public int CountCsvFiles { get { return InFiles.CsvFiles.Count; } }
        public int CountTiffFiles { get { return InFiles.TiffFiles.Count; } }
        public int CountUnicNames { get { return InFiles.UniqueNames.Count; } }

        public int CountFiles { get { return Files.Count; } }
        public int CountEtalonFiles { get { return EtalonFiles.Count; } }

        public bool HasDuplicateSource { get; private set; }

        public bool IsTestingMode { get; private set; }
        public string SourcePath { get; private set; }
        public string EtalonPath { get; private set; }

        public bool IsWorking { get; private set; }
        System.Threading.CancellationTokenSource cancelSource = new System.Threading.CancellationTokenSource();
        //-------------------------------------------------------------------------
        private void FillFiles()
        {
            Files = new List<FileItem>();
            foreach (var file in InFiles.UniqueNames)
            {
                if (cancelSource.IsCancellationRequested) return;
                Files.Add(new FileItem(file));
            }
        }
        //-------------------------------------------------------------------------
        public void GetIncomingFiles()
        {
            if (String.IsNullOrEmpty(SourcePath)) throw new Exception("Wrong path!");
            if (!IsTestingMode)
                InFiles = new IncomingFileItem(SourcePath);
            else
            {
                var defaults = new eDoctrinaUtils.OcrAppConfig(SourcePath);
                if (defaults.exception != null)
                {
                    throw new Exception(defaults.exception.Message);
                }
                var Paths = new List<string>();
                Paths.Add(defaults.ErrorFolder);
                Paths.Add(defaults.SuccessFolder);
                Paths.Add(defaults.NotConfidentFolder);
                Paths.Add(defaults.NotSupportedSheetFolder);
                Paths.Add(defaults.EmptyScansFolder);
                Paths = Paths.Distinct().ToList();
                for (int i = 0; i < Paths.Count; i++)
                {
                    Paths[i] = (Paths[i].Contains(':')) ? Paths[i] : Path.Combine(Path.GetDirectoryName(SourcePath), Paths[i]);
                }
                InFiles = new IncomingFileItem(Paths);
            }
        }
        //-------------------------------------------------------------------------
        private void Working()
        {
            IsWorking = true;
            DuplicateFiles = new List<FileItem>();
            GetIncomingFiles();
            FillFiles();
            if (IsTestingMode)
            {
                EtalonFiles = Load(EtalonPath);
                Verify();
            }
            else
            {
                HasDuplicateSource = Files.Count != Files.GroupBy(x => x.UniqueIdentifier).Count();
                Files.GroupBy(x => x.UniqueIdentifier).ToList().ForEach(x => { if (x.Count() > 1) DuplicateFiles.AddRange(x.ToList()); });
            }
        }
        //-------------------------------------------------------------------------
        public void WorkingAsync(bool isTestingMode, string sourcePath, string etalonPath)
        {
            IsTestingMode = isTestingMode;
            SourcePath = sourcePath;
            EtalonPath = etalonPath;
            cancelSource = new System.Threading.CancellationTokenSource();
            var scheduler = System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext();
            System.Threading.Tasks.Task.Factory.StartNew(status =>  Working(), "Working")
                .ContinueWith((t) => Completed(t), scheduler);
        }
        //-------------------------------------------------------------------------
        private void Completed(System.Threading.Tasks.Task t)
        {       //if (t.IsCanceled) Log.LogMessage("Recognizing was cancelled");
            if (t.Exception != null) { }
            t.Dispose();
            IsWorking = false;
            NotifyUpdated(WorkingCompleted, null, null);
        }
        //-------------------------------------------------------------------------
        public void Cancel()
        {
            cancelSource.Cancel();
        }
        //-------------------------------------------------------------------------
        public void SaveResult(string path, string confirmName)
        {
            var queryResult = Files.GroupBy(x => x.FrameSha1);
            var resultList = queryResult.Select(x => x.First()).ToList();
            var resultList2 = queryResult.Where(x => x.Count() > 1).ToList();
            Save(resultList, path, confirmName);
        }
        //-------------------------------------------------------------------------
        public void AddResult(string path, string confirmName)
        {
            var queryResult = Files.GroupBy(x => x.FrameSha1);
            var resultList = queryResult.Select(x => x.First()).ToList();
            var resultList2 = queryResult.Where(x => x.Count() > 1).ToList();
            Add(resultList, path, confirmName);
        }
        //-------------------------------------------------------------------------
        public int CountMissing { get; private set; }
        public int CountExtra { get; private set; }
        public int CountOk { get; private set; }
        public int CountError { get; private set; }
        //-------------------------------------------------------------------------
        public void Verify()
        {
            var testFiles = Files.Where(x => x.State != VerifyFiles.wrongDataSha1
                && x.State != VerifyFiles.missingAudit).ToList();
            //var testFiles = Files;
            var querymissing = from qFiles in EtalonFiles
                               join qEtalonFiles in testFiles
                               on qFiles.UniqueIdentifier equals qEtalonFiles.UniqueIdentifier into gj
                               from sub in gj.DefaultIfEmpty()
                               where sub == null
                               select new FileItem(qFiles, VerifyFiles.missing, qFiles);
            CountMissing = querymissing.Count();
            var queryextra = from qFiles in testFiles
                             join qEtalonFiles in EtalonFiles
                             on qFiles.UniqueIdentifier equals qEtalonFiles.UniqueIdentifier into gj
                             from sub in gj.DefaultIfEmpty()
                             where sub == null
                             select new FileItem(qFiles, VerifyFiles.extra, qFiles);
            CountExtra = queryextra.Count();
            var queryok = from qFiles in testFiles
                          from qEtalonFiles in EtalonFiles
                          where qFiles.UniqueIdentifier == qEtalonFiles.UniqueIdentifier
                          && qFiles.DataSha1 == qEtalonFiles.DataSha1 && qFiles.Error == qEtalonFiles.Error
                          select new FileItem(qFiles, VerifyFiles.ok, qEtalonFiles);
            CountOk = queryok.Count();
            var queryerror = from qFiles in testFiles
                             from qEtalonFiles in EtalonFiles
                             where qFiles.UniqueIdentifier == qEtalonFiles.UniqueIdentifier
                             && (qFiles.DataSha1 != qEtalonFiles.DataSha1 || qFiles.Error != qEtalonFiles.Error)
                             select new FileItem(qFiles, VerifyFiles.error, qEtalonFiles);
            CountError = queryerror.Count();

            //ResultFiles = Files.Where(x => x.State != VerifyFiles.empty).ToList();
            ResultFiles = Files.Where(x => x.State == VerifyFiles.missingAudit).ToList();
            ResultFiles = Files.Where(x => x.State == VerifyFiles.wrongDataSha1).ToList();
            ResultFiles.AddRange(queryerror.ToList());
            ResultFiles.AddRange(queryextra.ToList());
            ResultFiles.AddRange(querymissing.ToList());
            ResultFiles.AddRange(queryok.ToList());
        }
        //-------------------------------------------------------------------------
        //-------------------------------------------------------------------------
        //-------------------------------------------------------------------------
    }
}
