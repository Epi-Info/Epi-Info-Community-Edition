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
            Lib lib = new Lib();
            string file_name = System.Configuration.ConfigurationManager.AppSettings["selected_release"];
            
            string ftp_user_id  = System.Configuration.ConfigurationManager.AppSettings["ftp_user_id"];
            string ftp_password = System.Configuration.ConfigurationManager.AppSettings["ftp_password"];
            string[] file_list = System.Configuration.ConfigurationManager.AppSettings["release_text"].Split('\n');


            
            string download_directory = System.Configuration.ConfigurationManager.AppSettings["download_directory"];
            string root_directory = file_name.Substring(0, file_name.LastIndexOf('.'));

            string ftp_site = System.Configuration.ConfigurationManager.AppSettings["ftp_site"] + "s/" + root_directory;

            for (int i = 1; i < file_list.Length; i++)
            {
                string[] pair = file_list[i].Split(':');
                string source = ftp_site + pair[0];
                string destination = download_directory + root_directory + "/" + pair[0];


                if (i == 0)
                {
                    root_directory = pair[0] + "/";
                    System.IO.Directory.CreateDirectory(download_directory + root_directory);

                }
                else
                {
                    string target_directory = destination.Substring(0, destination.LastIndexOf('/'));
                    if (!System.IO.Directory.Exists(target_directory))
                    {
                        System.IO.Directory.CreateDirectory(target_directory);
                    }

                    lib.DownloadFile(ftp_user_id, ftp_password, source, destination);
                }
            }

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
                    System.Configuration.ConfigurationManager.AppSettings["release_text"] = this.VersionDetailTextBox.Text;
                    
                }
            }
        }

    }
}
