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
                    sb.Append(b.ToString("X2").ToLowerInvariant());
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



        public string create_manifest_file(string root_directory)
        {
            System.Text.StringBuilder result = new StringBuilder();

            System.Collections.Generic.Dictionary<string, string> file_list = create_file_hash_dictionary(root_directory);

            foreach (KeyValuePair<string, string> kvp in file_list)
            {
                result.Append(kvp.Key);
                result.Append(":");
                result.AppendLine(kvp.Value);
            }

            return result.ToString();
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

        public List<string> GetManifestFileList()
        {
            List<string> result = new List<string>();

            // Get the object used to communicate with the server.
            System.Net.FtpWebRequest request = (System.Net.FtpWebRequest)System.Net.WebRequest.Create(System.Configuration.ConfigurationManager.AppSettings["ftp_site"] + "/m");
            request.Method = System.Net.WebRequestMethods.Ftp.ListDirectoryDetails;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new System.Net.NetworkCredential(System.Configuration.ConfigurationManager.AppSettings["ftp_user_id"], System.Configuration.ConfigurationManager.AppSettings["ftp_password"]);

            System.Net.FtpWebResponse response = (System.Net.FtpWebResponse)request.GetResponse();

            System.IO.Stream responseStream = response.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(responseStream);


            string line = reader.ReadLine();
            while (!string.IsNullOrEmpty(line))
            {
                if (line.StartsWith("-"))
                {
                    int start_index = line.LastIndexOf(' ');
                    result.Add(line.Substring(start_index).Trim());
                }
                line = reader.ReadLine();
            }

            /*
            drwxrwxrwx   1 user     group           0 Apr 12 11:10 .
            drwxrwxrwx   1 user     group           0 Apr 12 11:10 ..
            drwxrwxrwx   1 user     group           0 Apr 12 10:42 Epi_Info_7-1-5
            -rw-rw-rw-   1 user     group       10193 Apr 12 11:10 release-7.1.5.txt

            */


            // Console.WriteLine("Directory List Complete, status {0}", response.StatusDescription);



            reader.Close();
            response.Close();

            return result;
        }


        public string GetTextFileContent(string file_name)
        {
            string result = null;

            // Get the object used to communicate with the server.
            System.Net.FtpWebRequest request = (System.Net.FtpWebRequest)System.Net.WebRequest.Create(System.Configuration.ConfigurationManager.AppSettings["ftp_site"] + "/m/" + file_name);
            request.Method = System.Net.WebRequestMethods.Ftp.DownloadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new System.Net.NetworkCredential(System.Configuration.ConfigurationManager.AppSettings["ftp_user_id"], System.Configuration.ConfigurationManager.AppSettings["ftp_password"]);

            System.Net.FtpWebResponse response = (System.Net.FtpWebResponse)request.GetResponse();

            System.IO.Stream responseStream = response.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(responseStream);


            result = reader.ReadToEnd();

            reader.Close();
            response.Close();

            return result;
        }

        //http://stackoverflow.com/questions/12519290/downloading-files-using-ftpwebrequest
        public void DownloadFile(string userName, string password, string ftpSourceFilePath, string localDestinationFilePath)
        {
            int bytesRead = 0;
            byte[] buffer = new byte[2048];

            System.Net.FtpWebRequest request = CreateFtpWebRequest(ftpSourceFilePath, userName, password, true);
            request.Method = System.Net.WebRequestMethods.Ftp.DownloadFile;

            System.IO.Stream reader = request.GetResponse().GetResponseStream();
            if (System.IO.File.Exists(localDestinationFilePath))
            {
                System.IO.File.Delete(localDestinationFilePath);
            }
            System.IO.FileStream fileStream = new System.IO.FileStream(localDestinationFilePath, System.IO.FileMode.Create);

            while (true)
            {
                bytesRead = reader.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                    break;

                fileStream.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();
        }

        private System.Net.FtpWebRequest CreateFtpWebRequest(string ftpDirectoryPath, string userName, string password, bool keepAlive = false)
        {
            System.Net.FtpWebRequest request = (System.Net.FtpWebRequest)System.Net.WebRequest.Create(new Uri(ftpDirectoryPath));

            //Set proxy to null. Under current configuration if this option is not set then the proxy that is used will get an html response from the web content gateway (firewall monitoring system)
            request.Proxy = null;

            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = keepAlive;

            request.Credentials = new System.Net.NetworkCredential(userName, password);

            return request;
        }
    }
}
