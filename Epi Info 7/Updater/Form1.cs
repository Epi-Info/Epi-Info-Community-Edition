﻿using System;
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
        private BackgroundWorker worker;
        private int failedDownloadCounter;

        public Form1()
        {
            InitializeComponent();
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            worker.RunWorkerAsync();
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

            DownloadUpdates();
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
                    worker.ReportProgress(pctint, fileName);
                    failedDownloadCounter = 0;
                    Download("ftp://ftp.cdc.gov/pub/software/epi_info/update_files/" + fileName, AppDomain.CurrentDomain.BaseDirectory + destination + fileName);
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
    }
}