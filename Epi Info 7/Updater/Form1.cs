using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.IO;
using System.Threading;
using System.Text;
using System.Windows.Forms;

namespace Updater
{
    public partial class Form1 : Form
    {
        private BackgroundWorker background_worker;

        private int failedDownloadCounter;

        public Form1()
        {
            InitializeComponent();
            background_worker = new BackgroundWorker();
            background_worker.WorkerReportsProgress = true;
            background_worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            background_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            background_worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Lib lib = new Lib();
            AvailableVersionSetGroupBox.Text = "Available Versions: [" + System.Configuration.ConfigurationManager.AppSettings["ftp_site"] + "//" + System.Configuration.ConfigurationManager.AppSettings["ftp_directory"] + "]";

            foreach (string fileName in lib.GetManifestFileList())
            {
                this.AvailableVersionSetListBox.Items.Add(fileName);
            }
        }

        private void ExecuteDownload()
        {
            //worker = new BackgroundWorker();
            background_worker.RunWorkerAsync();
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            lblStatus.Text = "Updating " + e.UserState.ToString();
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Epi Info has been updated.", "Update Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();            
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Process[] processes = Process.GetProcessesByName("EpiInfo");
            foreach (Process process in processes)
            {
                process.Kill();
            }

            processes = Process.GetProcessesByName("Menu");
            foreach (Process process in processes)
            {
                process.Kill();
            }

            processes = Process.GetProcessesByName("MakeView");
            foreach (Process process in processes)
            {
                process.Kill();
            }

            processes = Process.GetProcessesByName("Enter");
            foreach (Process process in processes)
            {
                process.Kill();
            }

            processes = Process.GetProcessesByName("Analysis");
            foreach (Process process in processes)
            {
                process.Kill();
            }

            processes = Process.GetProcessesByName("AnalysisDashboard");
            foreach (Process process in processes)
            {
                process.Kill();
            }

            //DownloadUpdates();
            //DownloadDescription();

            string file_name = System.Configuration.ConfigurationManager.AppSettings["selected_release"];
            string destination = System.Configuration.ConfigurationManager.AppSettings["download_directory"] + "/" + file_name;

            DownloadFile(System.Configuration.ConfigurationManager.AppSettings["ftp_user_id"], System.Configuration.ConfigurationManager.AppSettings["ftp_password"], System.Configuration.ConfigurationManager.AppSettings["ftp_site"] + "m/" + file_name, destination);


        }

        private void DownloadUpdates()
        {
            try
            {
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.Load("ftp://ftp.cdc.gov/pub/software/epi_info/updates.xml");
                List<System.Xml.XmlElement> elements = new List<System.Xml.XmlElement>();

                int counter = 0;
                foreach (System.Xml.XmlElement element in doc.DocumentElement.ChildNodes)
                {
                    string fileName = string.Empty;
                    string destination = string.Empty;
                    foreach (System.Xml.XmlElement downloadElement in element.ChildNodes)
                    {
                        if (downloadElement.Name.Equals("fileName"))
                        {
                            fileName = downloadElement.InnerText.Trim();
                        }
                        if (downloadElement.Name.Equals("destination"))
                        {
                            destination = downloadElement.InnerText.Trim();
                        }
                    }
                    counter++;
                    double pct = (100.0 * counter) / doc.DocumentElement.ChildNodes.Count;
                    int pctint = (int)Math.Truncate(pct);
                    background_worker.ReportProgress(pctint, fileName);
                    failedDownloadCounter = 0;
                    //Download("ftp://ftp.cdc.gov/pub/software/epi_info/update_files/" + fileName, AppDomain.CurrentDomain.BaseDirectory + destination + fileName);
                    Download(System.Configuration.ConfigurationManager.AppSettings["latest_file_set_url"] + fileName, "c:\\temp\\" + fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not update Epi Info. Please ensure that you have Read/Write/Execute privileges on your Epi Info folder or request your Administrator to download and install the new version for you.", "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }            
        }

        private void DownloadDescription()
        {
            try
            {

                string fileName = "release-7.1.5.txt";
                string destination = "c:\\temp\\" + fileName;

                DownloadFile(System.Configuration.ConfigurationManager.AppSettings["ftp_user_id"], System.Configuration.ConfigurationManager.AppSettings["ftp_password"], System.Configuration.ConfigurationManager.AppSettings["ftp_site"] + System.Configuration.ConfigurationManager.AppSettings["ftp_directory"] + fileName, destination);

                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not update Epi Info. Please ensure that you have Read/Write/Execute privileges on your Epi Info folder or request your Administrator to download and install the new version for you.", "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }


        private void Download(string url, string destination)
        {            
            using (WebClient wcDownload = new WebClient())
            {
                FtpWebRequest webRequest;
                FtpWebResponse webResponse = null;
                try
                {
                    webRequest = (FtpWebRequest)WebRequest.Create(url);

                    webResponse = (FtpWebResponse)webRequest.GetResponse();
                    
                    wcDownload.DownloadFile(url, destination);
                    wcDownload.Dispose();
                }
                catch (Exception ex)
                {
                    failedDownloadCounter++;
                    if (failedDownloadCounter < 20)
                    {
                        webResponse.Close();
                        Thread.Sleep(5000);
                        Download(url, destination);
                    }
                    else
                    {
                        throw ex;
                    }
                }
                finally
                {
                    if (webResponse != null)
                    {
                        webResponse.Close();
                    }
                }
            }
        }

        //http://stackoverflow.com/questions/12519290/downloading-files-using-ftpwebrequest
        private void DownloadFile(string userName, string password, string ftpSourceFilePath, string localDestinationFilePath)
        {
            int bytesRead = 0;
            byte[] buffer = new byte[2048];

            FtpWebRequest request = CreateFtpWebRequest(ftpSourceFilePath, userName, password, true);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            Stream reader = request.GetResponse().GetResponseStream();
            if (File.Exists(localDestinationFilePath))
            {
                File.Delete(localDestinationFilePath);
            }
            FileStream fileStream = new FileStream(localDestinationFilePath, FileMode.Create);

            while (true)
            {
                bytesRead = reader.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                    break;

                fileStream.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();
        }

        private FtpWebRequest CreateFtpWebRequest(string ftpDirectoryPath, string userName, string password, bool keepAlive = false)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(ftpDirectoryPath));

            //Set proxy to null. Under current configuration if this option is not set then the proxy that is used will get an html response from the web content gateway (firewall monitoring system)
            request.Proxy = null;

            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = keepAlive;

            request.Credentials = new NetworkCredential(userName, password);

            return request;
        }

        private void CreateLocalHashButton_Click(object sender, EventArgs e)
        {
            OutputTextBox.Clear();
            System.Text.StringBuilder output = new StringBuilder();
            Updater.Lib lib = new Lib();
            
            System.Collections.Generic.Dictionary<string,string> file_list = lib.create_file_hash_dictionary(@"C:\work-space-set\epi-info-set\epiinfo\Epi Info 7\build\debug");

            //System.Collections.Generic.Dictionary<string, string> file_list = lib.create_file_hash_dictionary(@"C:\work-space-set\Epi_Info_7-1-5");
            int File_Count = file_list.Count;

            foreach (KeyValuePair<string, string> kvp in file_list)
            {
                output.Append(kvp.Key);
                output.Append(":");
                output.AppendLine(kvp.Value);
            }

            OutputTextBox.Text = "NumberOfFiles: " + File_Count.ToString() + "\r\n" + output.ToString();
        }

        private void ExecuteDownloadButton_Click(object sender, EventArgs e)
        {
            background_worker.RunWorkerAsync();
        }

        private void SearchForFolderDialogButton_Click(object sender, EventArgs e)
        {
            this.DownloadFolderBrowserDialog.ShowNewFolderButton = false;
            this.DownloadFolderBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
            DialogResult result = this.DownloadFolderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                // the code here will be executed if the user presses Open in
                // the dialog.
                string foldername = this.DownloadFolderBrowserDialog.SelectedPath;
                SelectedDownloadFolderTextBox.Text = foldername;
                System.Configuration.ConfigurationManager.AppSettings["download_directory"] = foldername;
            }
        }

       


        private void AvailableVersionSetListBox_SelectedValueChanged(object sender, EventArgs e)
        {
            ListBox lb = (System.Windows.Forms.ListBox)sender;
            if (lb.SelectedItem != null)
            {
                Lib lib = new Lib();
                string selected_file_name = lb.SelectedItem.ToString();
                if (!string.IsNullOrWhiteSpace(selected_file_name))
                {
                    System.Configuration.ConfigurationManager.AppSettings["selected_release"] = selected_file_name;
                    this.VersionDetailTextBox.Text = lib.GetTextFileContent(selected_file_name);
                }
            }
        }

    }
}
