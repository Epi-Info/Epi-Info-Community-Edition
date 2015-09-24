using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Epi;
using Epi.Core;
using Epi.Data;
using Epi.Fields;
using Epi.ImportExport;
using Epi.ImportExport.ProjectPackagers;
using Epi.Windows;

namespace Epi.Windows.ImportExport.Dialogs
{
    /// <summary>
    /// Dialog used for importing EDP7 files.
    /// </summary>
    public partial class ImportEncryptedDataPackageDialog : Form
    {
        #region Private Members
        private Project sourceProject;
        private readonly Project destinationProject;
        private IDbDriver sourceProjectDataDriver;
        private View sourceView;
        private readonly View destinationView;
        private IDbDriver destinationProjectDataDriver;
        private Configuration config;        
        private BackgroundWorker importWorker;
        private static object syncLock = new object();
        private Stopwatch stopwatch;
        private bool importFinished = false;
        private string password;
        private List<string> packagePaths;
        private bool update = false;
        private bool append = true;
        private int runningCount = 1;
        private bool closeOnFinish = false;

        private string customSalt = "";
        private string customInitVector = "";
        private int customIterations = 4;
        #endregion // Private Members

        #region Delegates
        private delegate void SetStatusDelegate(string statusMessage);
        #endregion // Delegates

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public ImportEncryptedDataPackageDialog()
        {
            InitializeComponent();
            Construct(DataMergeType.UpdateAndAppend);
        }

        /// <summary>
        /// Constructor
        /// </summary>        
        /// <param name="destinationView">The form within the project that will accept the packaged data</param>
        /// <param name="mergeType">Determines how to handle data merging during the import.</param>
        public ImportEncryptedDataPackageDialog(View destinationView, DataMergeType mergeType = DataMergeType.UpdateAndAppend)
        {
            InitializeComponent();
            this.destinationView = destinationView;
            destinationProject = this.destinationView.Project;
            Construct(mergeType);
        }

        /// <summary>
        /// Constructor
        /// </summary>        
        /// <param name="destinationView">The form that will accept the data</param>
        /// <param name="packagePath">The package to be imported</param>
        /// <param name="mergeType">Determines how to handle data merging during the import.</param>
        public ImportEncryptedDataPackageDialog(View destinationView, string packagePath, DataMergeType mergeType = DataMergeType.UpdateAndAppend)
        {
            InitializeComponent();
            this.destinationView = destinationView;
            destinationProject = this.destinationView.Project;

            if (System.IO.File.Exists(packagePath))
            {
                checkboxBatchImport.Checked = false;
                txtPackageFile.Text = packagePath;
            }
            else if (System.IO.Directory.Exists(packagePath))
            {
                checkboxBatchImport.Checked = true;
                txtPackageFile.Text = packagePath;
            }
            else
            {
                throw new System.IO.FileNotFoundException(ImportExportSharedStrings.ERROR_PACKAGE_DOESNT_EXIST, packagePath);
            }

            Construct(mergeType);
        }        
        #endregion // Constructors

        #region Public Properties
        /// <summary>
        /// Gets/sets whether to close the window after finishing.
        /// </summary>
        public bool CloseOnFinish
        {
            get
            {
                return this.closeOnFinish;
            }
            set
            {
                this.closeOnFinish = value;
            }
        }
        #endregion // Public Properties

        private void Construct(DataMergeType mergeType)
        {
            config = Configuration.GetNewInstance();

            switch (mergeType)
            {
                case DataMergeType.UpdateAndAppend:
                    //this.cmbImportType.SelectedIndex = 0;
                    rdbUpdateAndAppend.Checked = true;
                    break;
                case DataMergeType.UpdateOnly:
                    //this.cmbImportType.SelectedIndex = 1;
                    rdbUpdate.Checked = true;
                    break;
                case DataMergeType.AppendOnly:
                    //this.cmbImportType.SelectedIndex = 2;
                    rdbAppend.Checked = true;
                    break;
            }
        }

        private IDbDriver SetupSourceProject()
        {
            sourceProjectDataDriver = DBReadExecute.GetDataDriver(sourceProject.FilePath);
            return sourceProjectDataDriver;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (checkboxBatchImport.Checked)
            {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string directoryPath = folderBrowserDialog.SelectedPath;
                    if (System.IO.Directory.Exists(directoryPath))
                    {
                        txtPackageFile.Text = directoryPath;
                    }
                }
            }
            else
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = ImportExportSharedStrings.SELECT_EDP_FILE;
                openFileDialog.Filter = "Epi Info Encrypted Data Package (*.edp7)|*.edp7";

                openFileDialog.FilterIndex = 1;
                openFileDialog.Multiselect = false;

                DialogResult result = openFileDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName.Trim();
                    if (System.IO.File.Exists(filePath))
                    {
                        txtPackageFile.Text = filePath;                        
                    }
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {   
            StartImportPackage();
        }

        protected virtual bool ValidateInput()
        {
            if (string.IsNullOrEmpty(txtPackageFile.Text))
            {
                Epi.Windows.MsgBox.ShowError(ImportExportSharedStrings.ERROR_NO_PACKAGE_SELECTED);
                return false;
            }

            // If it's a batch import, check to see if the directory exists
            if (checkboxBatchImport.Checked)
            {
                if (!System.IO.Directory.Exists(txtPackageFile.Text))
                {
                    Epi.Windows.MsgBox.ShowError(ImportExportSharedStrings.ERROR_DIRECTORY_DOESNT_EXIST);
                    return false;
                }
            }
            // otherwise, check to see if the file exists
            else
            {
                if (!System.IO.File.Exists(txtPackageFile.Text))
                {
                    Epi.Windows.MsgBox.ShowError(ImportExportSharedStrings.ERROR_PACKAGE_DOESNT_EXIST);
                    return false;
                }
            }

            return true;
        }

        private void CallbackSetupProgressBar(double maxProgress)
        {
            this.BeginInvoke(new SetMaxProgressBarValueDelegate(SetupProgressBar), maxProgress);
        }

        private void CallbackFinishUnpackaging()
        {
            runningCount++;
        }

        private void SetupProgressBar(double maxProgress)
        {   
            progressBar.Minimum = 0;
            progressBar.Maximum = (int)maxProgress;
        }

        private void EnableControls()
        {
            btnBrowse.Enabled = true;
            btnOK.Enabled = true;
            btnCancel.Enabled = true;
            txtPackageFile.Enabled = true;

            checkboxBatchImport.Enabled = true;
            rdbUpdateAndAppend.Enabled = true;
            rdbAppend.Enabled = true;
            rdbUpdate.Enabled = true;
            btnAdvanced.Enabled = true;
            txtPassword.Enabled = true;
        }

        private void DisableControls()
        {
            btnBrowse.Enabled = false;
            btnOK.Enabled = false;
            btnCancel.Enabled = false;
            txtPackageFile.Enabled = false;

            checkboxBatchImport.Enabled = false;
            rdbUpdateAndAppend.Enabled = false;
            rdbAppend.Enabled = false;
            rdbUpdate.Enabled = false;
            btnAdvanced.Enabled = false;
            txtPassword.Enabled = false;
        }

        public virtual void StartImportPackage()
        {
            if (!ValidateInput())
            {
                return;
            }

            progressBar.Style = ProgressBarStyle.Marquee;

            if (importWorker != null && importWorker.WorkerSupportsCancellation)
            {
                importWorker.CancelAsync();
            }

            this.Cursor = Cursors.WaitCursor;

            string importTypeDescription;
            if (rdbUpdateAndAppend.Checked)
            {
                update = true;
                append = true;
                importTypeDescription = ImportExportSharedStrings.IMPORT_DATA_UPDATE_MATCHING + " " + ImportExportSharedStrings.IMPORT_DATA_APPEND_NONMATCHING;
            }
            else if (rdbUpdate.Checked)
            {
                update = true;
                append = false;
                importTypeDescription = ImportExportSharedStrings.IMPORT_DATA_UPDATE_MATCHING + " " + ImportExportSharedStrings.IMPORT_DATA_IGNORE_NONMATCHING;
            }
            else 
            {
                update = false;
                append = true;
                importTypeDescription = ImportExportSharedStrings.IMPORT_DATA_APPEND_NONMATCHING + " " + ImportExportSharedStrings.IMPORT_DATA_IGNORE_MATCHING;
            }

            progressBar.Visible = true;
            progressBar.Style = ProgressBarStyle.Marquee;

            lbxStatus.Items.Clear();

            //AddNotificationStatusMessage("Package creation initiated by user " + System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString() + ".");

            DisableControls();

            password = txtPassword.Text;
            packagePaths = new List<string>();

            if (checkboxBatchImport.Checked)
            {
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(txtPackageFile.Text);
                foreach (System.IO.FileInfo f in dir.GetFiles("*.edp7"))
                {
                    packagePaths.Add(f.FullName);
                }
                AddNotificationStatusMessage(string.Format(ImportExportSharedStrings.START_BATCH_IMPORT, packagePaths.Count.ToString(), txtPackageFile.Text));
            }
            else
            {
                packagePaths.Add(txtPackageFile.Text);
                AddNotificationStatusMessage(string.Format(ImportExportSharedStrings.START_SINGLE_IMPORT, txtPackageFile.Text));
            }

            AddNotificationStatusMessage(importTypeDescription);

            stopwatch = new Stopwatch();
            stopwatch.Start();

            importWorker = new BackgroundWorker();
            importWorker.WorkerSupportsCancellation = true;
            importWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(importWorker_DoWork);
            importWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(importWorker_WorkerCompleted);
            importWorker.RunWorkerAsync();   
        }

        protected virtual void EndImportPackage(object result)
        {
            stopwatch.Stop();

            if (result is Exception)
            {
                Exception ex = result as Exception;

                if (ex != null)
                {
                    if (ex is System.Security.Cryptography.CryptographicException)
                    {
                        string msg = ImportExportSharedStrings.IMPORT_FAIL_DECRYPT;
                        this.BeginInvoke(new SetStatusDelegate(AddNotificationStatusMessage), msg);
                        this.BeginInvoke(new SetStatusDelegate(SetStatusMessage), msg);
                    }
                    else
                    {
                        string msg = string.Format(ImportExportSharedStrings.IMPORT_FAIL_EXCEPTION, ex.Message);
                        this.BeginInvoke(new SetStatusDelegate(AddNotificationStatusMessage), msg);
                        this.BeginInvoke(new SetStatusDelegate(SetStatusMessage), msg);
                    }
                }
            }
            else
            {
                string msg = string.Format(ImportExportSharedStrings.IMPORT_SUCCESS, stopwatch.Elapsed.ToString());
                this.BeginInvoke(new SetStatusDelegate(AddNotificationStatusMessage), msg);
                this.BeginInvoke(new SetStatusDelegate(SetStatusMessage), msg);
            }

            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 0;

            EnableControls();

            importFinished = true;

            this.Cursor = Cursors.Default;

            if (this.CloseOnFinish)
            {
                this.Close();
            }
        }

        /// <summary>
        /// Handles the WorkerCompleted event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void importWorker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            EndImportPackage(e.Result);
        }

        /// <summary>
        /// Provides callback to the UI thread for setting the progress bar value.
        /// </summary>
        private void CallbackSetProgressBar(double value)
        {
            this.BeginInvoke(new SetProgressBarDelegate(SetProgressBarValue), value);
        }


        /// <summary>
        /// Provides callback to the UI thread for resetting the progress bar.
        /// </summary>
        private void CallbackResetProgressBar()
        {
            this.BeginInvoke(new SetProgressBarDelegate(SetProgressBarValue), 1);
        }

        private void CallbackSetStatusMessage(string message)
        {
            this.BeginInvoke(new SetStatusDelegate(SetStatusMessage), message);
        }

        private void CallbackAddStatusMessage(string message)
        {
            this.BeginInvoke(new UpdateStatusEventHandler(AddNotificationStatusMessage), message);
        }

        private void CallbackIncrementProgressBar(double value)
        {
            this.BeginInvoke(new SetProgressBarDelegate(IncrementProgressBarValue), value);
        }

        /// <summary>
        /// Sets the status message of the import form
        /// </summary>
        /// <param name="message">The status message to display</param>
        private void SetStatusMessage(string message)
        {            
            txtProgress.Text = message;
        }

        /// <summary>
        /// Sets the progress bar to a given value
        /// </summary>
        /// <param name="value">The value by which to increment</param>
        private void SetProgressBarValue(double value)
        {
            progressBar.Style = ProgressBarStyle.Continuous;
            if (value > progressBar.Maximum)
            {
                progressBar.Style = ProgressBarStyle.Marquee;
            }
            else
            {
                progressBar.Value = (int)value;
            }
        }

        /// <summary>
        /// Increments the progress bar by a given value
        /// </summary>
        /// <param name="value">The value by which to increment</param>
        private void IncrementProgressBarValue(double value)
        {
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Increment((int)value);            
        }

        /// <summary>
        /// Adds a status message to the status list box
        /// </summary>
        /// <param name="statusMessage"></param>
        private void AddNotificationStatusMessage(string statusMessage)
        {
            string message = DateTime.Now + ": " + statusMessage;
            lbxStatus.Items.Add(message);
            Logger.Log(message);
        }

        /// <summary>
        /// Handles the DoWorker event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void importWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (syncLock)
            {
                foreach (string packagePath in packagePaths)
                {
                    try
                    {
                        string str = Configuration.DecryptFileToString(packagePath, txtPassword.Text);

                        if (!str.StartsWith("[[EPIINFO"))
                        {
                            #region LEGACY_IMPORT
                            List<string> paths = new List<string>();
                            paths.Add(packagePath);
                            ProjectUnpackager projectUnpackager = new ProjectUnpackager(destinationView, paths, password);
                            projectUnpackager.SetProgressBar += new SetProgressBarDelegate(CallbackIncrementProgressBar);
                            projectUnpackager.SetStatus += new UpdateStatusEventHandler(CallbackSetStatusMessage);
                            projectUnpackager.AddStatusMessage += new UpdateStatusEventHandler(CallbackAddStatusMessage);
                            projectUnpackager.SetMaxProgressBarValue += new SetMaxProgressBarValueDelegate(CallbackSetupProgressBar);
                            projectUnpackager.FinishUnpackage += new SimpleEventHandler(CallbackFinishUnpackaging);
                            projectUnpackager.Update = update;
                            projectUnpackager.Append = append;

                            try
                            {
                                if (!(string.IsNullOrEmpty(customSalt) && string.IsNullOrEmpty(customInitVector)))
                                {
                                    projectUnpackager.SetCustomDecryptionParameters(customSalt, customInitVector, customIterations);
                                }
                                projectUnpackager.UnpackageProjects();
                            }
                            catch (Exception ex)
                            {
                                e.Result = ex;
                            }
                            #endregion // LEGACY_IMPORT
                        }
                        else
                        {
                            str = str.Remove(0, 24);

                            string plainText = ImportExportHelper.UnZip(str);

                            XmlDocument xmlDataPackage = new XmlDocument();
                            xmlDataPackage.LoadXml(plainText);

                            XmlNodeList xnList = xmlDataPackage.SelectNodes("/DataPackage/Form/KeyFields");

                            if (xnList.Count == 0 || (xnList.Count == 1 && xnList[0].ChildNodes.Count == 0))
                            {
                                XmlDataUnpackager xmlUP = new Epi.ImportExport.ProjectPackagers.XmlDataUnpackager(destinationView, xmlDataPackage);
                                xmlUP.StatusChanged += new UpdateStatusEventHandler(CallbackSetStatusMessage);
                                xmlUP.UpdateProgress += new SetProgressBarDelegate(CallbackSetProgressBar);
                                xmlUP.ResetProgress += new SimpleEventHandler(CallbackResetProgressBar);
                                xmlUP.MessageGenerated += new UpdateStatusEventHandler(CallbackAddStatusMessage);
                                xmlUP.ImportFinished += new EventHandler(xmlUP_ImportFinished);
                                xmlUP.Append = append;
                                xmlUP.Update = update;
                                xmlUP.Unpackage();
                            }
                            else
                            {
                                XmlMultiKeyDataUnpackager xmlUP = new Epi.ImportExport.ProjectPackagers.XmlMultiKeyDataUnpackager(destinationView, xmlDataPackage);
                                xmlUP.StatusChanged += new UpdateStatusEventHandler(CallbackSetStatusMessage);
                                xmlUP.UpdateProgress += new SetProgressBarDelegate(CallbackSetProgressBar);
                                xmlUP.ResetProgress += new SimpleEventHandler(CallbackResetProgressBar);
                                xmlUP.MessageGenerated += new UpdateStatusEventHandler(CallbackAddStatusMessage);
                                xmlUP.ImportFinished += new EventHandler(xmlUP_ImportFinished);
                                
                                xmlUP.Append = append;
                                xmlUP.Update = update;
                                xmlUP.Unpackage();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        e.Result = ex;
                    }
                }
            }
        }

        void xmlUP_ImportFinished(object sender, EventArgs e)
        {
            if (sender is XmlDataUnpackager)
            {
                // Note: This section can be uncommented to show exactly how many records were updated or appended.

                //ImportInfo info = (sender as XmlDataUnpackager).ImportInfo;
                //CallbackAddStatusMessage("Forms processed: " + info.FormsProcessed);
                //CallbackAddStatusMessage("Grids processed: " + info.GridsProcessed);
                
                //foreach (KeyValuePair<View, int> kvp in info.RecordsUpdated)
                //{
                //    CallbackAddStatusMessage("Records updated on form " + kvp.Key.Name + ": " + kvp.Value);
                //}
                //foreach (KeyValuePair<View, int> kvp in info.RecordsAppended)
                //{
                //    CallbackAddStatusMessage("Records appended on form " + kvp.Key.Name + ": " + kvp.Value);
                //}

                //CallbackAddStatusMessage("Total records updated across all forms: " + info.TotalRecordsUpdated);
                //CallbackAddStatusMessage("Total records appended across all forms: " + info.TotalRecordsAppended);
            }
        }

        private void txtPackageFile_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPackageFile.Text))
            {
                btnOK.Enabled = true;
            }
            else
            {
                btnOK.Enabled = false;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (importFinished)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            else
            {
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            }

            this.Close();
        }

        private void btnAdvanced_Click(object sender, EventArgs e)
        {
            PackageAdvancedOptionsDialog paoDialog = new PackageAdvancedOptionsDialog();
            DialogResult result = paoDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (paoDialog.Salt.Length == 32 && paoDialog.InitVector.Length == 16)
                {
                    customSalt = paoDialog.Salt;
                    customInitVector = paoDialog.InitVector;
                    customIterations = paoDialog.Iterations;
                }
            }            
        }

        private void checkboxBatchImport_CheckedChanged(object sender, EventArgs e)
        {
            txtPackageFile.Text = string.Empty;            
        }

        private void cmsStatus_Click(object sender, EventArgs e)
        {
            if (lbxStatus.Items.Count > 0)
            {
                string StatusText = string.Empty;
                foreach (string item in lbxStatus.Items)
                {
                    StatusText = StatusText + System.Environment.NewLine + item;
                }
                Clipboard.SetText(StatusText);
            }
        }
    }
}
