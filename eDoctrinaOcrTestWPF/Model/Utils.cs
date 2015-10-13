using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using eDoctrinaUtils;

namespace eDoctrinaOcrTestWPF
{
    public class Utils
    {
        public static string GetSHA1FromFile(string destFileName)
        {
            string sha1hash = "";
            if (File.Exists(destFileName))
            {
                try
                {
                    using (FileStream stream = File.OpenRead(destFileName))
                    {
                        SHA1Managed sha = new SHA1Managed();
                        byte[] hash = sha.ComputeHash(stream);
                        sha1hash = BitConverter.ToString(hash).Replace("-", String.Empty);
                    }
                } catch { }
            }
            return sha1hash;
        }
        //-------------------------------------------------------------------------
        public static string[] GetSupportedFilesFromDirectory(string directory, SearchOption searchOption, string searchExtensions)
        {
            if (Directory.Exists(directory))
            {
                try
                {
                    return Directory.GetFiles(directory, "*.*", searchOption)
                        .Where(s => searchExtensions.ToLower().Split('|').Any(x => x == Path.GetExtension(s).ToLower()))
                        .OrderBy(x => File.GetLastAccessTime(x)).ToArray();
                }
                catch { }
            }
            return new string[0];
        }

        //-------------------------------------------------------------------------
        public AnswerVerification GetAnswerVerification(string input)
        {
            AnswerVerification answerVerification
            = Newtonsoft.Json.JsonConvert.DeserializeObject<AnswerVerification>(input);
            return answerVerification;
        }
    }
}
