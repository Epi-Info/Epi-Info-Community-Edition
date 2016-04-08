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


        public void IterateFileSystem<T>(System.IO.DirectoryInfo directoryInfo, Func<string, T> apply, List<T> result)
        {
            foreach (System.IO.FileInfo fileInfo in directoryInfo.GetFiles())
            {
                result.Add(apply(fileInfo.FullName));
            }

            foreach (System.IO.DirectoryInfo sub_directory_Info in directoryInfo.GetDirectories())
            {
                IterateFileSystem<T>(sub_directory_Info, apply, result);
            }
            
        }

        public System.Collections.Generic.Dictionary<string,string> create_file_hash_dictionary(string root_directory)
        {
            System.Collections.Generic.Dictionary<string, string> result = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);

            System.Text.StringBuilder output = new StringBuilder();

            System.IO.DirectoryInfo root_directory_info = new System.IO.DirectoryInfo(root_directory);
            List<KeyValuePair<string, string>> kvp_list = new List<KeyValuePair<string, string>>();
            IterateFileSystem<KeyValuePair<string, string>>(root_directory_info, x => { return CreateHashPairFromFilePath(x); }, kvp_list);

            foreach (KeyValuePair<string, string> kvp in kvp_list)
            {
                string file_path = kvp.Key.Replace(root_directory,"");
                if( file_path.StartsWith("\\"))
                {
                    file_path = file_path.Substring(1,file_path.Length - 1);
                }

                result.Add(file_path, kvp.Value);
            }

            return result;
        }
    }
}
