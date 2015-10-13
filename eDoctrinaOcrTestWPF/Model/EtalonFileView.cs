using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace eDoctrinaOcrTestWPF
{
    public class EtalonFileView
    {
        public List<FileItem> Load(string path)
        {
            return GetEtaloneFromCsv(path);
        }

        public void Save(List<FileItem> files, string path, string confirmName)
        {
            WriteEtaloneCsv(path, files, confirmName, false);
        }

        public void Add(List<FileItem> files, string path, string confirmName)
        {
            WriteEtaloneCsv(path, files, confirmName, true);
        }

        private void WriteEtaloneCsv(string destFileName, List<FileItem> files, string confirmName, bool append)
        {
            using (StreamWriter swToCSV = new StreamWriter(destFileName, append, Encoding.ASCII))
            {
                foreach (var file in files)
                {
                    swToCSV.WriteLine(file.SourceSha1 + "," + file.SourcePage + "," + file.DataSha1 + ","
                        + file.CorrectFileName + "," + file.FrameSha1 + "," + file.Error + "," + confirmName);
                }
            }
        }

        private List<FileItem> GetEtaloneFromCsv(string destFileName)
        {
            List<FileItem> files = new List<FileItem>();
            using (StreamReader swToCSV = new StreamReader(destFileName, Encoding.ASCII))
            {
                while (!swToCSV.EndOfStream)
                {
                    var str = swToCSV.ReadLine();
                    if (str.Contains(","))
                    {
                        var temp = str.Split(',');
                        if (temp.Count() < 7)
                            Array.Resize(ref temp, 7);
                        files.Add(new FileItem()
                        {
                            SourceSha1 = temp[0],
                            SourcePage = temp[1],
                            DataSha1 = temp[2],
                            CorrectFileName = temp[3],
                            FrameSha1 = temp[4],
                            Error = temp[5],
                            AutorName = temp[6]
                        });
                    }
                }
            }
            return files;
        }
    }
}
