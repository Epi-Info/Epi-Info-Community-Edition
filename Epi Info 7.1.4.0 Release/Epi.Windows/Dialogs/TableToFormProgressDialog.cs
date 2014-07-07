using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Epi.Data;

namespace Epi.Windows.Dialogs
{
    public partial class TableToFormProgressDialog : Form
    {
        #region Private Members
        /// <summary>
        /// The data driver for the data to be converted
        /// </summary>
        IDbDriver sourceDriver;

        /// <summary>
        /// The data driver for the project
        /// </summary>
        IDbDriver destinationDriver;

        /// <summary>
        /// The project that will contain the converted data
        /// </summary>
        Project project;

        /// <summary>
        /// The name of the form in the project that will contain the converted data
        /// </summary>
        string destinationViewName;

        /// <summary>
        /// Table name in the source data set
        /// </summary>
        string tableName;

        /// <summary>
        /// List of columns in the source data set to import
        /// </summary>
        List<string> columnNames;

        /// <summary>
        /// The worker that will conduct the data import
        /// </summary>
        BackgroundWorker worker;

        /// <summary>
        /// Whether the operation has completed
        /// </summary>
        bool complete = false;

        /// <summary>
        /// Whether the operation was cancelled
        /// </summary>
        bool cancelled = false;

        /// <summary>
        /// Used to map which field names go with which column names, e.g. if the user imported a field like "First Name", the space isn't valid and gets deleted.
        /// </summary>
        List<Epi.ImportExport.ColumnConversionInfo> columnMapping;
        #endregion // Private Members

        public TableToFormProgressDialog(Project pProject, string pDestinationViewName, IDbDriver pSourceDriver, string pTableName, List<Epi.ImportExport.ColumnConversionInfo> pColumnMapping)
        {
            InitializeComponent();

            project = pProject;
            destinationViewName = pDestinationViewName;
            sourceDriver = pSourceDriver;
            tableName = pTableName;
            columnMapping = pColumnMapping;

            columnNames = new List<string>();

            foreach (Epi.ImportExport.ColumnConversionInfo cci in this.columnMapping)
            {
                columnNames.Add(cci.SourceColumnName);
            }            
            
            destinationDriver = project.CollectedData.GetDatabase();
        }

        private void TableToFormProgressDialog_Load(object sender, EventArgs e)
        {            
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.RunWorkerAsync();
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            complete = true;
            if (cancelled)
            {
                Epi.Windows.MsgBox.ShowWarning("Table to form import process was cancelled. Some or all data may be missing.");
            }
            else
            {
                Epi.Windows.MsgBox.ShowInformation("Table to form import process complete.");
            }
            this.Close();            
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            //Epi.ImportExport.TableToFormDataConverter tfc = new ImportExport.TableToFormDataConverter(project, destinationView, sourceDriver, tableName, columnNames, columnMapping);
            //tfc.SetMaxProgressBarValue += new SetMaxProgressBarValueDelegate(tfc_SetMaxProgressBarValue);
            //tfc.SetProgressBar += new SetProgressBarDelegate(tfc_SetProgressBar);
            //tfc.SetStatus += new UpdateStatusEventHandler(tfc_SetStatus);
            //tfc.CheckForCancellation += new CheckForCancellationHandler(tfc_CheckForCancellation);
            //tfc.DoConversion();

            Epi.ImportExport.TableToFormMetadataConverter metadataConverter = new Epi.ImportExport.TableToFormMetadataConverter(project, destinationViewName, sourceDriver, tableName, columnMapping);
            metadataConverter.SetMaxProgressBarValue += new SetMaxProgressBarValueDelegate(dataConverter_SetMaxProgressBarValue);
            metadataConverter.SetProgressBar += new SetProgressBarDelegate(dataConverter_SetProgressBar);
            metadataConverter.SetStatus += new UpdateStatusEventHandler(dataConverter_SetStatus);
            metadataConverter.CheckForCancellation += new CheckForCancellationHandler(dataConverter_CheckForCancellation);
            metadataConverter.Convert();

            View view = project.Views[destinationViewName];            

            view.SetTableName(view.Name);

            project.CollectedData.CreateDataTableForView(view, 1);

            Epi.ImportExport.TableToFormDataConverter dataConverter = new Epi.ImportExport.TableToFormDataConverter(project, view, sourceDriver, tableName, columnMapping);
            dataConverter.SetMaxProgressBarValue += new SetMaxProgressBarValueDelegate(dataConverter_SetMaxProgressBarValue);
            dataConverter.SetProgressBar += new SetProgressBarDelegate(dataConverter_SetProgressBar);
            dataConverter.SetStatus += new UpdateStatusEventHandler(dataConverter_SetStatus);
            dataConverter.CheckForCancellation += new CheckForCancellationHandler(dataConverter_CheckForCancellation);
            dataConverter.Convert();
            dataConverter.Dispose();
        }

        bool dataConverter_CheckForCancellation()
        {
            if (worker.CancellationPending == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SetStatusMessage(string message)
        {
            this.txtStatus.Text = message;
        }

        void SetProgressBar(double progress)
        {
            if (this.progressBar.Value + (int)progress <= this.progressBar.Maximum)
            {
                this.progressBar.Value = this.progressBar.Value + (int)progress;
            }
        }

        void SetProgressBarMaxValue(double maxValue)
        {
            this.progressBar.Maximum = (int)maxValue;
        }

        void dataConverter_SetStatus(string message)
        {
            //this.txtStatus.Text = message;
            this.BeginInvoke(new UpdateStatusEventHandler(SetStatusMessage), message);
        }

        void dataConverter_SetProgressBar(double progress)
        {
            //this.progressBar.Value = this.progressBar.Value + (int)progress;
            this.BeginInvoke(new SetProgressBarDelegate(SetProgressBar), progress);
        }

        void dataConverter_SetMaxProgressBarValue(double maxProgress)
        {
            //this.progressBar.Maximum = (int)maxProgress;
            this.BeginInvoke(new SetMaxProgressBarValueDelegate(SetProgressBarMaxValue), maxProgress);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (!complete)
            {
                DialogResult result = Epi.Windows.MsgBox.Show("Cancelling this operation will result in an incomplete import of the data. Proceed?", "Cancel operation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    worker.CancelAsync();
                    btnCancel.Enabled = false;
                    cancelled = true;
                    txtStatus.Text = "Cancelling...";
                }
            }
        }

        private void TableToFormProgressDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!complete)
            {
                DialogResult result = Epi.Windows.MsgBox.Show("Cancelling this operation will result in an incomplete import of the data. Proceed?", "Cancel operation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    worker.CancelAsync();
                    btnCancel.Enabled = false;
                    cancelled = true;
                    txtStatus.Text = "Cancelling...";
                }

                e.Cancel = true;
            }
        }
    }
}
