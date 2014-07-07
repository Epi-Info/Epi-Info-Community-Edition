using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Epi.ImportExport;

namespace Epi.Windows.ImportExport.Dialogs
{
    public partial class UpgradeProblemDisplayDialog : Form
    {
        private ImportExportErrorList errorList;
        private Epi.Epi2000.Project sourceProject;
        BackgroundWorker worker;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pSourceProject">The Epi Info 2000 project to check</param>
        public UpgradeProblemDisplayDialog(Epi.Epi2000.Project pSourceProject)
        {
            InitializeComponent();
            lvProblems.Items.Clear();
            sourceProject = pSourceProject;
            this.errorList = new ImportExportErrorList();
        }

        /// <summary>
        /// Gets the list of errors
        /// </summary>
        public ImportExportErrorList ErrorList
        {
            get
            {
                return errorList;
            }
        }

        private void UpgradeProblemDisplayDialog_Load(object sender, EventArgs e)
        {
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.Visible = true;
            progressBar.Value = 0;
            progressBar.Minimum = 0;

            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.RunWorkerAsync(sourceProject);
        }

        private void EndProcessing()
        {
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Visible = true;
            progressBar.Value = 0;
            progressBar.Minimum = 0;

            ImageList imageList = new ImageList();            
            imageList.Images.Add("a", SystemIcons.Error);
            imageList.Images.Add("b", SystemIcons.Warning);
            imageList.Images.Add("c", SystemIcons.Information);

            lvProblems.Left = 10;

            lvProblems.View = System.Windows.Forms.View.Details;
            lvProblems.GridLines = true;
            lvProblems.LargeImageList = imageList;
            lvProblems.SmallImageList = imageList;
            ListViewGroup lvgErrorGroup = lvProblems.Groups.Add("errors", "Errors"), lvgWarningGroup = lvProblems.Groups.Add("warnings", "Warnings"), lvgNotificationsGroup = lvProblems.Groups.Add("notifications", "Notifications"); ;

            lvProblems.Scrollable = true;

            int errorCount = 0;
            int warningCount = 0;

            foreach (ImportExportMessage problem in this.errorList)
            {
                if (problem.MessageType == ImportExportMessageType.Error)
                {
                    ListViewItem item = new ListViewItem(SharedStrings.IMPORT_PREFIX_ERROR, "a");
                    item.Group = lvgErrorGroup;
                    item.SubItems.Add(problem.Code);
                    item.SubItems.Add(problem.Message);
                    lvProblems.Items.Add(item);                    
                    errorCount++;
                    Logger.Log(DateTime.Now + ":  3.5.x project upgrade check for " + sourceProject.Name + " generated the following error - " + problem.Code + " - " + problem.Message);
                }
            }

            foreach (ImportExportMessage problem in this.errorList)
            {
                if (problem.MessageType == ImportExportMessageType.Warning)
                {
                    ListViewItem item = new ListViewItem(SharedStrings.IMPORT_PREFIX_WARNING, "b");
                    item.Group = lvgWarningGroup;
                    item.SubItems.Add(problem.Code);
                    item.SubItems.Add(problem.Message);
                    lvProblems.Items.Add(item);
                    warningCount++;
                    Logger.Log(DateTime.Now + ":  3.5.x project upgrade check for " + sourceProject.Name + " generated the following warning - " + problem.Code + " - " + problem.Message);
                }
            }

            foreach (ImportExportMessage problem in this.errorList)
            {
                if (problem.MessageType == ImportExportMessageType.Notification)
                {
                    ListViewItem item = new ListViewItem(SharedStrings.IMPORT_PREFIX_NOTICE, "c");
                    item.Group = lvgNotificationsGroup;
                    item.SubItems.Add(problem.Code);
                    item.SubItems.Add(problem.Message);
                    lvProblems.Items.Add(item);
                    Logger.Log(DateTime.Now + ":  3.5.x project upgrade check for " + sourceProject.Name + " generated the following notification - " + problem.Code + " - " + problem.Message);
                }
            }

            lvProblems.Columns.Add(ImportExportSharedStrings.TYPE, 68, HorizontalAlignment.Left);
            lvProblems.Columns.Add(ImportExportSharedStrings.CODE, 58, HorizontalAlignment.Left);
            lvProblems.Columns.Add(ImportExportSharedStrings.MESSAGE, 900, HorizontalAlignment.Left);
            lvProblems.FullRowSelect = true;

            ListViewItem itemFinal = new ListViewItem(string.Empty, "c");
            itemFinal.Group = lvgNotificationsGroup;
            itemFinal.SubItems.Add("");
            
            if (errorCount > 0)
            {
                this.btnOK.Enabled = false;
                itemFinal.SubItems.Add(string.Format(ImportExportSharedStrings.UPGRADE_ERRORS_DETECTED, sourceProject.Name));                
            }
            else
            {
                if (warningCount > 0)
                {
                    itemFinal.SubItems.Add(string.Format(ImportExportSharedStrings.UPGRADE_WARNINGS_DETECTED, sourceProject.Name));
                }
                this.btnOK.Enabled = true;
            }

            if (warningCount == 0 && errorCount == 0)
            {                
                itemFinal.SubItems.Add(string.Format(ImportExportSharedStrings.UPGRADE_NO_PROBLEMS_DETECTED, sourceProject.Name));
            }

            lvProblems.Items.Add(itemFinal);

            //SystemIcons.Application
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.BeginInvoke(new SimpleEventHandler(EndProcessing));
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            CheckSourceProjectForProblems(this.sourceProject);            
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            worker.CancelAsync();
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Sets a status message to the status list box
        /// </summary>
        /// <param name="statusMessage"></param>
        private void AddStatusMessage(string statusMessage)
        {            
            txtProgress.Text = statusMessage;
        }

        private void CheckSourceProjectForProblems(Epi.Epi2000.Project sourceProject)
        {
            Epi.ImportExport.Epi2000.ProjectAnalyzer projectAnalyzer = new Epi.ImportExport.Epi2000.ProjectAnalyzer(sourceProject);
            projectAnalyzer.SetStatus += new UpdateStatusEventHandler(projectAnalyzer_SetStatus);
            projectAnalyzer.Analyze();
            this.errorList = projectAnalyzer.ErrorList;
        }

        void projectAnalyzer_SetStatus(string message)
        {
            this.BeginInvoke(new UpdateStatusEventHandler(AddStatusMessage), message);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }
    }
}
