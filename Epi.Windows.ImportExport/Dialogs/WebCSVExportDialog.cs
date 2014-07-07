using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Epi.ImportExport;

namespace Epi.Windows.ImportExport.Dialogs
{
    public partial class WebCSVExportDialog : Form
    {
        private string exportFileName = string.Empty;

        public WebCSVExportDialog()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();            
            dialog.Filter = "Comma separated values file (*.csv)|*.csv";
            dialog.Title = "Select Web survey export file";
            dialog.FilterIndex = 1;            
            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                exportFileName = dialog.FileName;                
                txtFilePath.Text = exportFileName;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (ValidateInput())
            {
                BackgroundWorker exportWorker = new BackgroundWorker();
                exportWorker.WorkerSupportsCancellation = true;
                exportWorker.DoWork += new DoWorkEventHandler(exportWorker_DoWork);
                exportWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(exportWorker_RunWorkerCompleted);
                exportWorker.RunWorkerAsync();
            }
        }

        void exportWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.BeginInvoke(new SimpleEventHandler(EndExport));
        }

        void exportWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Guid surveyKey = new Guid(txtWebSurveyKey.Text);
            Guid orgKey = new Guid(txtOrgKey.Text);
            Guid secKey = new Guid(txtSecKey.Text);
            WebSurveyCSVExporter exporter = new WebSurveyCSVExporter(surveyKey, orgKey, secKey, txtFilePath.Text);
            exporter.SetStatus += new UpdateStatusEventHandler(exporter_SetStatus);
            exporter.FinishExport += new SimpleEventHandler(exporter_FinishExport);
            exporter.ExportFailed += new UpdateStatusEventHandler(exporter_ExportFailed);
            try
            {
                exporter.Export();
            }
            catch (Exception ex)
            {
                this.Invoke(new SimpleEventHandler(EndExport));
                MsgBox.ShowException(ex);
            }
        }

        void exporter_ExportFailed(string message)
        {
            this.Invoke(new SimpleEventHandler(EndExport));
            MsgBox.ShowError(message);
        }

        void exporter_FinishExport()
        {
            MsgBox.ShowInformation("Export complete.");

            this.Invoke(new SimpleEventHandler(EndExport));
            
            this.Invoke(new SimpleEventHandler(Close));
        }

        void exporter_SetStatus(string message)
        {
            this.BeginInvoke(new UpdateStatusEventHandler(SetStatus), message);
        }

        private void SetStatus(string message)
        {
            this.txtStatus.Text = message;
        }

        private void StartExport()
        {
            btnBrowse.Enabled = false;
            btnOK.Enabled = false;
            btnCancel.Enabled = false;
            txtOrgKey.Enabled = false;
            txtWebSurveyKey.Enabled = false;
            txtSecKey.Enabled = false;
            
            progressBar.Style = ProgressBarStyle.Marquee;
        }

        private void EndExport()
        {
            btnBrowse.Enabled = true;
            btnOK.Enabled = true;
            btnCancel.Enabled = true;
            txtOrgKey.Enabled = true;
            txtWebSurveyKey.Enabled = true;
            txtSecKey.Enabled = true;
            progressBar.Style = ProgressBarStyle.Continuous;
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrEmpty(txtFilePath.Text.Trim()))
            {
                Epi.Windows.MsgBox.ShowError("No file path specified.");
                return false;
            }

            if (string.IsNullOrEmpty(txtWebSurveyKey.Text.Trim()))
            {
                Epi.Windows.MsgBox.ShowError("No web survey key specified.");
                return false;
            }

            if (string.IsNullOrEmpty(txtOrgKey.Text.Trim()))
            {
                Epi.Windows.MsgBox.ShowError("No organization key specified.");
                return false;
            }

            if (string.IsNullOrEmpty(txtSecKey.Text.Trim()))
            {
                Epi.Windows.MsgBox.ShowError("No security token specified.");
                return false;
            }

            if (string.IsNullOrEmpty(Configuration.GetNewInstance().Settings.WebServiceEndpointAddress.Trim()))
            {
                Epi.Windows.MsgBox.ShowError("No web survey endpoint address has been specified. Please enter the endpoint address using the Epi Info 7 Options menu.");
                return false;
            }

            try
            {
                Guid g = new Guid(txtWebSurveyKey.Text);
                g = new Guid(txtOrgKey.Text);
                g = new Guid(txtSecKey.Text);
            }
            catch (Exception ex)
            {
                Epi.Windows.MsgBox.ShowError("One or more keys is invalid. Error: " + ex.Message);
                return false;
            }

            return true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
