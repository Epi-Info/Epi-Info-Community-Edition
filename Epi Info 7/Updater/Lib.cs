using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Updater
{
    public class Lib
    {

        public string GetHash(string file_path)
        {
            String result;
            StringBuilder sb = new StringBuilder();
            System.Security.Cryptography.MD5 md5Hasher = System.Security.Cryptography.MD5.Create();

            using (System.IO.FileStream fs = System.IO.File.OpenRead(file_path))
            {
                foreach (Byte b in md5Hasher.ComputeHash(fs))
                    sb.Append(b.ToString("X2").ToLower());
            }

            result = sb.ToString();

            return result;
        }

        public KeyValuePair<string,string> CreateHashPairFromFilePath(string file_path)
        {
            string hash_value = GetHash(file_path);
            return new KeyValuePair<string, string>(file_path, hash_value);
        }


        public void IterateFileSystem<T,R>(System.IO.DirectoryInfo directoryInfo, Func<string, T> apply, List<T> result)
        {
            foreach (System.IO.FileInfo fileInfo in directoryInfo.GetFiles())
            {
                result.Add(apply(fileInfo.FullName));
            }

            foreach (System.IO.DirectoryInfo sub_directory_Info in directoryInfo.GetDirectories())
            {
                IterateFileSystem<T, R>(sub_directory_Info, apply, result);
            }
            
        }
    }
}
