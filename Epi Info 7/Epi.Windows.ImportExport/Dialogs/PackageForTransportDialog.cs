#region Using
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Epi.Fields;
using Epi.Data;
using Epi.ImportExport;
using Epi.ImportExport.ProjectPackagers;
using Epi.Windows;
#endregion // Using

namespace Epi.Windows.ImportExport.Dialogs
{
    /// <summary>
    /// User interface designed for handling project packaging. Project packages are MDB files that have been compressed and encrypted.
    /// The UI should let the user pick a project and form within that project to package, specify what columns to remove from the package,
    /// and prompt the user for a password.
    /// </summary>
    public partial class PackageForTransportDialog : Form
    {
        #region Private Members
        private Configuration config;
        private Dictionary<string, List<string>> fieldsToNull;
        private Dictionary<string, List<string>> gridColumnsToNull;
        private Project sourceProject;
        private BackgroundWorker packageWorker;
        private static object syncLock = new object();
        private Stopwatch stopwatch;
        private bool packageFinished = false;
        private string FormName = "";
        private Query selectQuery;
        private string customSalt = "";
        private string customInitVector = "";
        private int customIterations = 4;
        private bool includeCodeTables = false;
        private bool includeGridData = true;
        private bool appendTimestamp = false;
        private string packageName = "";
        private FormInclusionType formInclusionType = FormInclusionType.AllDescendants;
        private List<IRowFilterCondition> rowFilterConditions;
        #endregion // Private Members

        #region Delegates
        private delegate void SetStatusDelegate(string statusMessage);
        #endregion // Delegates

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        public PackageForTransportDialog()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PackageForTransportDialog(bool setSmallSize)
        {
            InitializeComponent();
            Construct();
            if (setSmallSize)
            {
                SetSmallSize();
            }
        }

        /// <summary>
        /// Constructor with default project
        /// </summary>
        /// <param name="projectPath">Path the source project</param>
        /// <param name="view">The view to set on the UI</param>
        public PackageForTransportDialog(string projectPath, View view)
        {
            InitializeComponent();

            this.txtProjectPath.Text = projectPath;
            if (cmbPackageForm.Items.Contains(view.Name))
            {
                this.cmbPackageForm.SelectedItem = view.Name;
                this.btnBrowseProject.Enabled = false;
                this.cmbPackageForm.Enabled = false;
            }
            this.Form = view;

            Construct();
        }

        /// <summary>
        /// Constructor with default project and/or default package
        /// </summary>
        /// <param name="projectPath">Path the source project</param>
        /// <param name="view">The view to set on the UI</param>
        /// <param name="packagePath">The path to the package</param>
        public PackageForTransportDialog(string projectPath, View view, string packagePath)
        {
            InitializeComponent();

            this.txtProjectPath.Text = projectPath;
            if (cmbPackageForm.Items.Contains(view.Name))
            {
                this.cmbPackageForm.SelectedItem = view.Name;
                btnBrowseProject.Enabled = false;
            }

            this.txtPackagePath.Text = packagePath;

            Construct();
        }

        /// <summary>
        /// Constructor with package script
        /// </summary>
        /// <param name="scriptPath">The path to the packaging script</param>
        public PackageForTransportDialog(string scriptPath)
        {
            InitializeComponent();
            Construct();
            CreateFromXml(scriptPath);
        }

        /// <summary>
        /// Constructor with package script
        /// </summary>
        /// <param name="scriptPath">The path to the packaging script</param>
        /// <param name="setSmallSize">Whether or not to use 'small size'</param>
        public PackageForTransportDialog(string scriptPath, bool setSmallSize)
        {
            InitializeComponent();
            Construct();
            CreateFromXml(scriptPath);
            if (setSmallSize)
            {
                SetSmallSize();
            }
        }
        #endregion // Constructors

        #region Private Properties
        /// <summary>
        /// Gets/sets whether or not to include descendant (related) forms. See the FormInclusionType.cs file for
        /// more details on the underlying data type.
        /// </summary>
        /// <remarks>
        /// This setting is used to determine whether related form data are included or not, and if so, to what level
        /// such form data should be included. This setting is useful if the sender of the data package has many child
        /// forms and doesn't feel the need to include all of that related data due to file size limitations, or
        /// perhaps that data is irrelevant to the recipient.
        /// </remarks>
        private FormInclusionType FormInclusionType
        {
            get
            {
                return this.formInclusionType;
            }
            set
            {
                this.formInclusionType = value;
            }
        }

        /// <summary>
        /// Gets/sets whether the package should include data for the code tables.
        /// </summary>
        /// <remarks>
        /// The word 'code table' refers to data tables within the database that store the possible
        /// choices users can make in drop-down list fields. For example, a drop-down list field for
        /// 'Gender' would typically have its values stored in a table called 'codeGender'. Including
        /// the code table data can be useful in determining differences between forms during the
        /// import process, but can also increase the file size if there happen to be a lot of code
        /// tables in the database.
        /// </remarks>
        private bool IncludeCodeTables
        {
            get
            {
                return this.includeCodeTables;
            }
            set
            {
                this.includeCodeTables = value;
            }
        }

        /// <summary>
        /// Gets/sets whether the package file name should have a timestamp appended to it.
        /// </summary>
        private bool AppendTimestamp
        {
            get
            {
                return this.appendTimestamp;
            }
            set
            {
                this.appendTimestamp = value;
            }
        }

        /// <summary>
        /// Gets/sets the name of the resulting package file.
        /// </summary>
        private string PackageName
        {
            get
            {
                return this.packageName;
            }
            set
            {
                this.packageName = value;
            }
        }

        /// <summary>
        /// Gets/sets the path where the package should be placed on the disk.
        /// </summary>
        private string PackagePath { get; set; }

        /// <summary>
        /// Gets/sets whether or not to include grid data.
        /// </summary>
        /// <remarks>
        /// Grid Data in this context refers to any data table in the database that stores information
        /// for Grid fields (see GridField.cs in Epi.Core). Grids may be used to store various types of
        /// information that may not be relevant to whomever is being sent the data package, and this
        /// setting allows that data to be excluded in order to space.
        /// </remarks>
        private bool IncludeGridData
        {
            get
            {
                return this.includeGridData;
            }
            set
            {
                this.includeGridData = value;
            }
        }
        #endregion // Private Properties

        #region Public Properties
        /// <summary>
        /// Gets/sets the list of fields to use as custom match keys.
        /// </summary>
        public List<Field> KeyFields { get; private set; }

        /// <summary>
        /// Gets/sets whether or not to close the dialog on completion of the packaging.
        /// </summary>
        public bool CloseOnFinish { get; set; }

        /// <summary>
        /// Gets/sets the top-most form being packaged
        /// </summary>
        public View Form { get; private set; }
        #endregion // Public Properties

        #region Private Methods
        /// <summary>
        /// Constructs the class
        /// </summary>
        private void Construct()
        {
            CloseOnFinish = false;
            KeyFields = new List<Field>();
            EnableDisableButtons();
            config = Configuration.GetNewInstance();

            if (!string.IsNullOrEmpty(txtProjectPath.Text))
            {
                sourceProject = new Project(txtProjectPath.Text);
            }
            else
            {
                sourceProject = new Project();
            }

            rowFilterConditions = new List<IRowFilterCondition>();

            AddNotificationStatusMessage(ImportExportSharedStrings.READY);
        }

        /// <summary>
        /// Sets 'small size' mode.
        /// </summary>
        private void SetSmallSize()
        {
            this.Size = new Size(483, 139);
            this.grpDataRemoval.Visible = false;
            this.txtPackageName.Visible = false;
            this.txtPackagePath.Visible = false;
            this.txtPassword.Visible = false;
            //this.txtProgress.Visible = false;
            this.txtProjectPath.Visible = false;
            this.txtVerifyPassword.Visible = false;
            this.lblDataRemoval.Visible = false;
            this.lblPackageName.Visible = false;
            this.lblPackagePath.Visible = false;
            this.lblPassword.Visible = false;
            this.lblProjectPath.Visible = false;
            this.lblVerifyPassword.Visible = false;

            //this.btnCancel.Visible = false;
            //this.btnOK.Visible = false;
            this.btnLoad.Visible = false;
            this.btnSave.Visible = false;

            this.lbxStatus.Visible = false;
            this.btnBrowsePackage.Visible = false;
            this.btnBrowseProject.Visible = false;

            this.cmbPackageForm.Visible = false;

            this.checkboxIncludeTime.Visible = false;
        }

        /// <summary>
        /// Validates user input and displayes messages when the input is incorrect.
        /// </summary>
        /// <returns>bool; represents whether or not the input is correct and the packaging can proceed.</returns>
        private bool ValidateInput()
        {
            if (string.IsNullOrEmpty(txtProjectPath.Text))
            {
                Epi.Windows.MsgBox.ShowError(ImportExportSharedStrings.ERROR_NO_PACKAGE);
                return false;
            }

            if (!System.IO.File.Exists(txtProjectPath.Text))
            {
                Epi.Windows.MsgBox.ShowError(ImportExportSharedStrings.ERROR_PROJECT_DOESNT_EXIST);
                return false;
            }

            if (cmbPackageForm.SelectedIndex == -1)
            {
                Epi.Windows.MsgBox.ShowError(ImportExportSharedStrings.ERROR_NO_FORM);
                return false;
            }

            if (string.IsNullOrEmpty(txtPackagePath.Text))
            {
                Epi.Windows.MsgBox.ShowError(ImportExportSharedStrings.ERROR_SPECIFY_PACKAGE_PATH);
                return false;
            }

            if (string.IsNullOrEmpty(txtPackageName.Text))
            {
                Epi.Windows.MsgBox.ShowError(ImportExportSharedStrings.ERROR_SPECIFY_PACKAGE_NAME);
                return false;
            }
            else
            {
                string packageName = txtPackageName.Text;
                for (int i = 0; i < packageName.Length; i++)
                {
                    string viewChar = packageName.Substring(i, 1);
                    System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(viewChar, "[@ A-Za-z0-9_.]");
                    if (!m.Success)
                    {
                        Epi.Windows.MsgBox.ShowError(string.Format(ImportExportSharedStrings.ERROR_PACKAGE_NAME_HAS_INVALID_CHARACTER, viewChar));
                        return false;
                    }
                }
            }

            if (!System.IO.Directory.Exists(txtPackagePath.Text))
            {
                Epi.Windows.MsgBox.ShowError(ImportExportSharedStrings.ERROR_PACKAGE_PATH_DOESNT_EXIST);
                return false;
            }

            if (!(txtPassword.Text.Equals(txtVerifyPassword.Text)))
            {
                Epi.Windows.MsgBox.ShowError(ImportExportSharedStrings.ERROR_PASSWORDS_DONT_MATCH);
                return false;
            }

            System.IO.FileInfo fi = new System.IO.FileInfo(txtProjectPath.Text);
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(txtPackagePath.Text);

            if (fi.Directory.Name.Equals(di.Name) || fi.Directory.Name.ToLower().Equals(di.Name.ToLower()) || fi.Directory == di)
            {
                Epi.Windows.MsgBox.ShowError(ImportExportSharedStrings.ERROR_SAME_FOLDER);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Starts package creation
        /// </summary>
        public void StartCreatePackage()
        {
            if (!ValidateInput())
            {
                return;
            }

            string packagePath = txtPackagePath.Text + sourceProject.Name;

            if (packageWorker != null && packageWorker.WorkerSupportsCancellation)
            {
                packageWorker.CancelAsync();
            }

            this.Cursor = Cursors.WaitCursor;

            progressBar.Visible = true;
            progressBar.Style = ProgressBarStyle.Marquee;

            lbxStatus.Items.Clear();

            AddNotificationStatusMessage(string.Format(ImportExportSharedStrings.PACKAGE_STARTED, System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString()));

            btnBrowsePackage.Enabled = false;
            btnBrowseProject.Enabled = false;
            btnOK.Enabled = false;
            btnCancel.Enabled = false;
            btnLoad.Enabled = false;
            btnSave.Enabled = false;
            cmbPackageForm.Enabled = false;
            txtPackagePath.Enabled = false;
            txtProjectPath.Enabled = false;
            txtPassword.Enabled = false;
            txtVerifyPassword.Enabled = false;
            txtPackageName.Enabled = false;
            checkboxIncludeTime.Enabled = false;

            btnAdvanced.Enabled = false;
            btnRowFilter.Enabled = false;
            btnFormDataRemoval.Enabled = false;
            btnGridDataRemoval.Enabled = false;

            stopwatch = new Stopwatch();
            stopwatch.Start();

            FormName = cmbPackageForm.SelectedItem.ToString();
            PackageName = txtPackageName.Text;
            PackagePath = txtPackagePath.Text;

            packageWorker = new BackgroundWorker();
            packageWorker.WorkerSupportsCancellation = true;
            packageWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(packageWorker_DoWork);
            packageWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(packageWorker_WorkerCompleted);
            packageWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Ends package creation.
        /// </summary>
        /// <param name="result">The result from the packaging attempt.</param>
        private void EndCreatePackage(object result)
        {
            stopwatch.Stop();

            if (result == null || (result is bool && (bool)result == true))
            {
                this.BeginInvoke(new SetStatusDelegate(AddNotificationStatusMessage), string.Format(ImportExportSharedStrings.PACKAGE_ENDED, stopwatch.Elapsed.ToString()));
                this.BeginInvoke(new SetStatusDelegate(SetStatusMessage), string.Format(ImportExportSharedStrings.PACKAGE_ENDED, stopwatch.Elapsed.ToString()));
            }
            else if (result is Exception)
            {
                this.BeginInvoke(new SetStatusDelegate(AddNotificationStatusMessage), string.Format(ImportExportSharedStrings.PACKAGE_FAILED_EXCEPTION, ((Exception)result).Message));
                this.BeginInvoke(new SetStatusDelegate(SetStatusMessage), string.Format(ImportExportSharedStrings.PACKAGE_FAILED_EXCEPTION, ((Exception)result).Message));

                Exception ex = ((Exception)result);
                Logger.Log(DateTime.Now + " :  Ex Message: " + ex.Message);
                Logger.Log(DateTime.Now + " :  Ex InnerException: " + ex.InnerException);
                Logger.Log(DateTime.Now + " :  Ex Source: " + ex.Source);
                Logger.Log(DateTime.Now + " :  Ex StackTrace: " + ex.StackTrace);
                Logger.Log(DateTime.Now + " :  Ex TargetSite: " + ex.TargetSite);
            }
            else
            {
                this.BeginInvoke(new SetStatusDelegate(AddNotificationStatusMessage), string.Format(ImportExportSharedStrings.PACKAGE_FAILED, stopwatch.Elapsed.ToString()));
                this.BeginInvoke(new SetStatusDelegate(SetStatusMessage), string.Format(ImportExportSharedStrings.PACKAGE_FAILED, stopwatch.Elapsed.ToString()));
            }

            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 0;

            btnBrowsePackage.Enabled = true;
            btnBrowseProject.Enabled = true;
            btnOK.Enabled = true;
            btnCancel.Enabled = true;
            btnLoad.Enabled = true;
            btnSave.Enabled = true;
            cmbPackageForm.Enabled = true;
            txtPackagePath.Enabled = true;
            txtProjectPath.Enabled = true;
            txtPassword.Enabled = true;
            txtVerifyPassword.Enabled = true;
            txtPackageName.Enabled = true;
            checkboxIncludeTime.Enabled = true;

            btnAdvanced.Enabled = true;
            btnRowFilter.Enabled = true;
            btnFormDataRemoval.Enabled = true;
            btnGridDataRemoval.Enabled = true;

            packageFinished = true;

            this.Cursor = Cursors.Default;

            if (this.CloseOnFinish)
            {
                this.Close();
            }
        }

        /// <summary>
        /// Enables and disables the OK button.
        /// </summary>
        private void EnableDisableButtons()
        {
            if (!string.IsNullOrEmpty(txtPackagePath.Text.Trim()) && !string.IsNullOrEmpty(txtProjectPath.Text.Trim()))
            {
                btnOK.Enabled = true;
            }
            else
            {
                btnOK.Enabled = false;
            }

            if (cmbPackageForm.SelectedIndex >= 0)
            {
                btnFormDataRemoval.Enabled = true;
                btnGridDataRemoval.Enabled = true;
                btnRowFilter.Enabled = true;
                btnAdvanced.Enabled = true;
            }
            else
            {
                btnFormDataRemoval.Enabled = false;
                btnGridDataRemoval.Enabled = false;
                btnRowFilter.Enabled = false;
                btnAdvanced.Enabled = false;
            }
        }

        /// <summary>
        /// Provides callback to the UI thread for setting the current status message.
        /// </summary>
        /// <param name="message">The message to send</param>
        private void CallbackSetStatusMessage(string message)
        {
            this.BeginInvoke(new SetStatusDelegate(SetStatusMessage), message);
        }

        /// <summary>
        /// Provides callback to the UI thread for adding a message to the status list box.
        /// </summary>
        /// <param name="message">The message to add</param>
        private void CallbackAddStatusMessage(string message)
        {
            this.BeginInvoke(new UpdateStatusEventHandler(AddNotificationStatusMessage), message);
        }

        /// <summary>
        /// Provides callback to the UI thread for incrementing the progress bar.
        /// </summary>
        private void CallbackIncrementProgressBar(double value)
        {
            this.BeginInvoke(new SetProgressBarDelegate(IncrementProgressBarValue), value);
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

        /// <summary>
        /// Provides callback to the UI thread for setting the progress bar's maximum value.
        /// </summary>
        /// <param name="maxProgress">The max progress</param>
        private void CallbackSetupProgressBar(double maxProgress)
        {
            this.BeginInvoke(new SetMaxProgressBarValueDelegate(SetupProgressBar), maxProgress);
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
        /// Increments the progress bar by a given value
        /// </summary>
        /// <param name="value">The value by which to increment</param>
        private void IncrementProgressBarValue(double value)
        {
            progressBar.Style = ProgressBarStyle.Continuous;
            if (value > progressBar.Maximum)
            {
                progressBar.Style = ProgressBarStyle.Marquee;
            }
            else
            {
                progressBar.Increment((int)value);
            }
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
        /// Sets up the progress bar's max value.
        /// </summary>
        /// <param name="maxProgress">The max value of the progress bar</param>
        private void SetupProgressBar(double maxProgress)
        {
            progressBar.Minimum = 0;
            progressBar.Maximum = (int)maxProgress;
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
        /// Saves the current state of the user interface (sans passwords) as a packaging script which can later be loaded
        /// and used to re-run the data packaging routine.
        /// </summary>
        private void SaveScript()
        {
            if (string.IsNullOrEmpty(txtProjectPath.Text) ||
                string.IsNullOrEmpty(txtPackagePath.Text) ||
                cmbPackageForm.SelectedIndex == -1)
            {
                return;
            }
            SaveFileDialog savePackageScriptDialog = new SaveFileDialog();
            savePackageScriptDialog.InitialDirectory = sourceProject.Location;
            savePackageScriptDialog.DefaultExt = ".pks7";
            savePackageScriptDialog.Filter = "Epi Info 7 Project Package Script|*.pks7";

            DialogResult result = savePackageScriptDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {

                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("ProjectPackagingScript");

                string xmlString =
                "<projectLocation>" + txtProjectPath.Text + "</projectLocation>" +
                "<formToPackage>" + cmbPackageForm.SelectedItem.ToString() + "</formToPackage>" +
                "<packageLocation>" + txtPackagePath.Text + "</packageLocation>" +
                "<packageName>" + txtPackageName.Text + "</packageName>" +
                "<includeGridData>" + IncludeGridData + "</includeGridData>" +
                "<includeCodeTables>" + IncludeCodeTables + "</includeCodeTables>" +
                "<formInclusionType>" + ((int)FormInclusionType).ToString() + "</formInclusionType>" +
                "<appendTimestamp>" + AppendTimestamp + "</appendTimestamp>";

                xmlString += "<fieldsToNull>";
                foreach (KeyValuePair<string, List<string>> kvp in this.fieldsToNull)
                {
                    foreach (string fieldName in kvp.Value)
                    {
                        xmlString += "<fieldToNull formName=\"" + kvp.Key + "\">";
                        xmlString += fieldName;
                        xmlString += "</fieldToNull>";
                    }
                }
                xmlString += "</fieldsToNull>";

                xmlString += "<keyFields>";
                foreach (Field field in this.KeyFields)
                {
                    xmlString += "<keyField formName=\"" + field.View.Name + "\">";
                    xmlString += field.Name;
                    xmlString += "</keyField>";
                }
                xmlString += "</keyFields>";

                if (selectQuery != null)
                {
                    xmlString += "<rowFilter>";

                    xmlString += "<rowFilterQuery>";
                    xmlString += selectQuery.SqlStatement.Replace(">", "&gt;").Replace("<", "&lt;");
                    xmlString += "</rowFilterQuery>";

                    xmlString += "<rowFilterParameters>";

                    foreach (QueryParameter param in selectQuery.Parameters)
                    {
                        xmlString += "<rowFilterParameter>";
                        xmlString += "<dbType>" + ((int)param.DbType).ToString() + "</dbType>";
                        xmlString += "<name>" + param.ParameterName + "</name>";
                        xmlString += "<value>" + param.Value + "</value>";
                        xmlString += "</rowFilterParameter>";
                    }
                    xmlString += "</rowFilterParameters>";

                    xmlString += "<rowFilterConditions>";

                    foreach (IRowFilterCondition rowFc in this.rowFilterConditions)
                    {
                        XmlNode node = rowFc.Serialize(doc);
                        xmlString += "<rowFilterCondition filterType=\"" + node.Attributes[0].Value + "\">";
                        xmlString += node.InnerXml;
                        xmlString += "</rowFilterCondition>";
                    }
                    xmlString += "</rowFilterConditions>";

                    xmlString += "</rowFilter>";
                }

                if (this.gridColumnsToNull != null)
                {
                    xmlString += "<gridColumnsToNull>";
                    foreach (KeyValuePair<string, List<string>> kvp in this.gridColumnsToNull)
                    {
                        foreach (string columnName in kvp.Value)
                        {
                            string[] gridFieldInfo = kvp.Key.ToString().Split(':');

                            xmlString += "<gridColumnToNull formName=\"" + gridFieldInfo[0] + "\" gridName=\"" + gridFieldInfo[1] + "\">";
                            xmlString += columnName;
                            xmlString += "</gridColumnToNull>";
                        }
                    }
                    xmlString += "</gridColumnsToNull>";
                }

                root.InnerXml = xmlString;

                System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(savePackageScriptDialog.FileName, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                root.WriteTo(writer);
                writer.Close();
                Epi.Windows.MsgBox.ShowInformation(ImportExportSharedStrings.SCRIPT_SAVED);
            }
        }

        /// <summary>
        /// Loads a packaging script from disk.
        /// </summary>
        private void LoadScript(string scriptPath = "")
        {
            if (string.IsNullOrEmpty(scriptPath))
            {
                OpenFileDialog openPackageScriptDialog = new OpenFileDialog();
                openPackageScriptDialog.Filter = "Epi Info 7 Project Package Script|*.pks7";

                DialogResult result = openPackageScriptDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    CreateFromXml(openPackageScriptDialog.FileName);
                }
            }
            else
            {
                CreateFromXml(scriptPath);
            }
        }

        /// <summary>
        /// Loads a packaging script.
        /// </summary>
        private void CreateFromXml(string scriptPath)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(scriptPath))
            {
                throw new ArgumentNullException("scriptPath");
            }
            else if (!System.IO.File.Exists(scriptPath))
            {
                throw new System.IO.FileNotFoundException("The specified data packaging script was not found.", scriptPath);
            }
            #endregion // Input Validation

            XmlDocument doc = new XmlDocument();
            doc.Load(scriptPath);

            foreach (XmlElement element in doc.DocumentElement.ChildNodes)
            {
                switch (element.Name.ToLower())
                {
                    case "projectlocation":
                        txtProjectPath.Text = element.InnerText;
                        break;
                    case "formtopackage":
                        cmbPackageForm.SelectedItem = element.InnerText;
                        break;
                    case "packagelocation":
                        txtPackagePath.Text = element.InnerText;
                        break;
                    case "packagename":
                        txtPackageName.Text = element.InnerText;
                        break;
                    case "includegriddata":
                        IncludeGridData = bool.Parse(element.InnerText);
                        break;
                    case "includecodetables":
                        IncludeCodeTables = bool.Parse(element.InnerText);
                        break;
                    case "appendtimestamp":
                        AppendTimestamp = bool.Parse(element.InnerText);
                        checkboxIncludeTime.Checked = AppendTimestamp;
                        break;
                    case "forminclusiontype":
                        FormInclusionType = ((FormInclusionType)Int32.Parse(element.InnerText));
                        break;
                    case "fieldstonull":
                        CreateNullFieldListFromXML(element);
                        break;
                    case "keyfields":
                        CreateKeyFieldListFromXML(element);
                        break;
                    case "gridcolumnstonull":
                        CreateNullGridColumnListFromXML(element);
                        break;
                    case "rowfilter":
                        CreateRowFiltersFromXML(element);
                        break;
                }
            }
            AddNotificationStatusMessage(string.Format(ImportExportSharedStrings.SCRIPT_LOADED, scriptPath));
        }

        /// <summary>
        /// Helper method for the CreateFromXml method. Loads a list of row filter conditions from XML.
        /// </summary>
        /// <param name="element">XmlElement to process</param>
        private void CreateRowFiltersFromXML(XmlElement element)
        {
            if (element.Name.ToLower().Equals("rowfilter"))
            {
                foreach (XmlElement childElement in element.ChildNodes)
                {
                    if (childElement.Name.ToLower().Equals("rowfilterquery"))
                    {
                        selectQuery = sourceProject.CollectedData.GetDbDriver().CreateQuery(childElement.InnerText.Replace("&gt;", ">").Replace("&lt;", "<"));
                    }
                    else if (childElement.Name.ToLower().Equals("rowfilterparameters"))
                    {
                        foreach (XmlElement gcElement in childElement.ChildNodes)
                        {
                            System.Data.DbType dbType = System.Data.DbType.String;
                            string name = "";
                            object value = null;

                            foreach (XmlElement ggcElement in gcElement.ChildNodes)
                            {
                                switch (ggcElement.Name.ToLower())
                                {
                                    case "dbtype":
                                        dbType = ((DbType)Int32.Parse(ggcElement.InnerText));
                                        break;
                                    case "name":
                                        name = ggcElement.InnerText;
                                        break;
                                    case "value":
                                        value = ggcElement.InnerText;
                                        break;
                                }
                            }

                            QueryParameter queryParameter = new QueryParameter(name, dbType, value);
                            if (selectQuery != null)
                            {
                                selectQuery.Parameters.Add(queryParameter);
                            }
                        }
                    }
                    else if (childElement.Name.ToLower().Equals("rowfilterconditions"))
                    {
                        List<IRowFilterCondition> conditions = new List<IRowFilterCondition>();

                        foreach (XmlElement grandChildElement in childElement.ChildNodes)
                        {
                            if (grandChildElement.HasAttribute("filterType"))
                            {
                                string strType = grandChildElement.Attributes["filterType"].Value + ",Epi.ImportExport";
                                Type filterType = Type.GetType(strType, false, true);
                                if (filterType != null)
                                {
                                    IRowFilterCondition condition = (IRowFilterCondition)Activator.CreateInstance(filterType, new object[] { });
                                    condition.CreateFromXml(grandChildElement);
                                    conditions.Add(condition);
                                }
                            }
                        }

                        this.rowFilterConditions = conditions;
                    }
                }
            }

            string filter = String.Empty;
            if (this.rowFilterConditions.Count > 0)
            {
                filter = ImportExportSharedStrings.SCRIPT_FILTERS;
                WordBuilder wb = new WordBuilder(" and ");
                foreach (IRowFilterCondition rfc in this.rowFilterConditions)
                {
                    wb.Add(rfc.Description);
                }
                filter = filter + wb.ToString();
                CallbackAddStatusMessage(filter);
            }
        }

        /// <summary>
        /// Helper method for the CreateFromXml method. Loads a list of grid columns that should be nulled out during
        /// the data packaging process.
        /// </summary>
        /// <param name="element">XmlElement to process</param>
        private void CreateNullGridColumnListFromXML(XmlElement element)
        {
            if (element.Name.ToLower().Equals("gridcolumnstonull"))
            {
                foreach (XmlElement columnElement in element.ChildNodes)
                {
                    if (columnElement.Name.ToLower().Equals("gridcolumntonull") && columnElement.Attributes.Count == 2 && columnElement.Attributes[0].Name.ToLower().Equals("formname") && columnElement.Attributes[1].Name.ToLower().Equals("gridname"))
                    {
                        string formName = columnElement.Attributes[0].Value;
                        string gridName = columnElement.Attributes[1].Value;

                        string gridInfo = formName + ":" + gridName;

                        string columnName = columnElement.InnerText;

                        if (gridColumnsToNull.Keys.Contains(gridInfo) && !gridColumnsToNull[gridInfo].Contains(columnName))
                        {
                            gridColumnsToNull[gridInfo].Add(columnName);
                        }
                        else if (!gridColumnsToNull.Keys.Contains(gridInfo))
                        {
                            gridColumnsToNull.Add(gridInfo, new List<string>());
                            gridColumnsToNull[gridInfo].Add(columnName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Helper method for the CreateFromXml method. Loads a list of fields that should be nulled out during
        /// the data packaging process.
        /// </summary>
        /// <param name="element">XmlElement to process</param>
        private void CreateNullFieldListFromXML(XmlElement element)
        {
            if (element.Name.ToLower().Equals("fieldstonull"))
            {
                foreach (XmlElement fieldElement in element.ChildNodes)
                {
                    if (fieldElement.Name.ToLower().Equals("fieldtonull") && fieldElement.Attributes.Count > 0 && fieldElement.Attributes[0].Name.ToLower().Equals("formname"))
                    {
                        string formName = fieldElement.Attributes[0].Value;
                        string fieldName = fieldElement.InnerText;

                        if (fieldsToNull.Keys.Contains(formName) && !fieldsToNull[formName].Contains(fieldName))
                        {
                            fieldsToNull[formName].Add(fieldName);
                        }
                        else if (!fieldsToNull.Keys.Contains(formName))
                        {
                            fieldsToNull.Add(formName, new List<string>());
                            fieldsToNull[formName].Add(fieldName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Helper method for the CreateFromXml method. Loads a list of fields that should be nulled out during
        /// the data packaging process.
        /// </summary>
        /// <param name="element">XmlElement to process</param>
        private void CreateKeyFieldListFromXML(XmlElement element)
        {
            if (element.Name.ToLower().Equals("keyfields"))
            {
                foreach (XmlElement fieldElement in element.ChildNodes)
                {
                    if (fieldElement.Name.ToLower().Equals("keyfield"))
                    {
                        string fieldName = fieldElement.InnerText;

                        if (!this.Form.Fields.Contains(fieldName))
                        {
                            MsgBox.ShowError(string.Format(ImportExportSharedStrings.ERROR_PACKAGER_MATCH_NOT_FOUND, fieldName.ToString()) + " " + ImportExportSharedStrings.ERROR_PACKAGER_CUSTOM_MATCH_NOT_USED);
                            return;
                        }
                        else if (this.Form.Fields.Contains(fieldName))
                        {
                            Field field = this.Form.Fields[fieldName];
                            if (!KeyFields.Contains(field))
                            {
                                KeyFields.Add(field);
                            }
                        }
                    }
                }
            }
        }
        #endregion // Private Methods

        #region Event Handlers
        /// <summary>
        /// Handles the Click event for the OK button (a.k.a. The 'Package' button)
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            StartCreatePackage();
        }

        /// <summary>
        /// Handles the WorkerCompleted event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void packageWorker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            EndCreatePackage(e.Result);
        }

        /// <summary>
        /// Handles the DoWorker event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void packageWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (syncLock)
            {
                try
                {
                    XmlDataPackager xmlDP = new XmlDataPackager(sourceProject.Views[FormName], this.PackageName);

                    foreach (KeyValuePair<string, List<string>> kvp in gridColumnsToNull)
                    {
                        foreach (string gridColumnName in kvp.Value)
                        {
                            xmlDP.AddGridColumnToNull(gridColumnName, kvp.Key);
                        }
                    }
                    //xmlDP.GridColumnsToNull = gridColumnsToNull;

                    foreach (KeyValuePair<string, List<string>> kvp in fieldsToNull)
                    {
                        foreach (string fieldName in kvp.Value)
                        {
                            xmlDP.AddFieldToNull(fieldName, kvp.Key);
                        }
                    }
                    //xmlDP.FieldsToNull = fieldsToNull;

                    CallbackSetupProgressBar(100);

                    Dictionary<string, Epi.ImportExport.Filters.RowFilters> filters = new Dictionary<string, Epi.ImportExport.Filters.RowFilters>();
                    filters.Add(FormName, new Epi.ImportExport.Filters.RowFilters(sourceProject.CollectedData.GetDatabase()));
                    foreach (IRowFilterCondition rfc in rowFilterConditions)
                    {
                        filters[FormName].Add(rfc);
                    }
                    xmlDP.Filters = filters;
                    xmlDP.StatusChanged += new UpdateStatusEventHandler(CallbackSetStatusMessage);
                    xmlDP.UpdateProgress += new SetProgressBarDelegate(CallbackSetProgressBar);
                    xmlDP.ResetProgress += new SimpleEventHandler(CallbackResetProgressBar);
                    xmlDP.KeyFields = KeyFields;
                    XmlDocument package = xmlDP.PackageForm();

                    string fileName = PackageName;

                    if (AppendTimestamp)
                    {
                        DateTime dt = DateTime.UtcNow;
                        string dateDisplayValue = string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:s}", dt);
                        dateDisplayValue = dateDisplayValue.Replace(':', '-'); // The : must be replaced otherwise the encryption fails
                        fileName = PackageName + "_" + dateDisplayValue;
                    }

                    ExportInfo exportInfo = xmlDP.ExportInfo;
                    foreach (KeyValuePair<View, int> kvp in exportInfo.RecordsPackaged)
                    {
                        CallbackAddStatusMessage("Form " + kvp.Key.Name + ": " + kvp.Value + " records packaged.");
                    }

                    // TODO: Remove this before release! This output is for testing purposes only!
                    // package.Save(@PackagePath + "\\" + fileName + ".xml");

                    CallbackSetStatusMessage("Compressing package...");
                    string compressedText = ImportExportHelper.Zip(package.OuterXml);
                    compressedText = "[[EPIINFO7_DATAPACKAGE]]" + compressedText;

                    CallbackSetStatusMessage("Encrypting package...");
                    Configuration.EncryptStringToFile(compressedText, @PackagePath + "\\" + fileName + ".edp7", txtPassword.Text);

                    e.Result = true;
                }
                catch (Exception ex)
                {
                    e.Result = ex;
                }
            }
        }

        /// <summary>
        /// Handles the click event for the project browse button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnBrowseProject_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Epi Info " + SharedStrings.PROJECT_FILE + " (*.prj)|*.prj";
            openFileDialog.Title = SharedStrings.SELECT_DATA_SOURCE;
            openFileDialog.InitialDirectory = config.Directories.Project;
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;
            openFileDialog.CheckFileExists = true;
            DialogResult result = openFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                txtProjectPath.Text = openFileDialog.FileName;
                sourceProject = new Project(openFileDialog.FileName);
            }
        }

        /// <summary>
        /// Handles the click event for the package path browse button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnBrowsePackage_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            DialogResult result = folderDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                txtPackagePath.Text = folderDialog.SelectedPath;
            }
        }

        /// <summary>
        /// Handles the text changed event for the package path text box
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtPackagePath_TextChanged(object sender, EventArgs e)
        {
            EnableDisableButtons();
        }

        /// <summary>
        /// Handles the text changed event for the project path text box
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtProjectPath_TextChanged(object sender, EventArgs e)
        {
            EnableDisableButtons();

            try
            {
                sourceProject = new Project(txtProjectPath.Text);

                foreach (View view in sourceProject.Views)
                {
                    if (view.IsRelatedView == false)
                    {
                        cmbPackageForm.Items.Add(view.Name);
                    }
                }
            }
            catch (System.Security.Cryptography.CryptographicException ex)
            {
                Epi.Windows.MsgBox.ShowException(ex);
            }
        }

        /// <summary>
        /// Handles the selected index changed event for the package form combo box
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void cmbPackageForm_SelectedIndexChanged(object sender, EventArgs e)
        {
            //FillForms();

            fieldsToNull = new Dictionary<string, List<string>>();
            gridColumnsToNull = new Dictionary<string, List<string>>();

            if (fieldsToNull.Count == 0)
            {
                foreach (View view in sourceProject.Views)
                {
                    fieldsToNull.Add(view.Name, new List<string>());
                }
            }

            if (gridColumnsToNull.Count == 0)
            {
                foreach (View view in sourceProject.Views)
                {
                    gridColumnsToNull.Add(view.Name, new List<string>());
                }
            }

            if (cmbPackageForm.SelectedIndex != 0)
            {
                this.Form = sourceProject.Views[cmbPackageForm.SelectedItem.ToString()];
            }

            EnableDisableButtons();
        }

        /// <summary>
        /// Handles the click event for the cancel button.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            if (this.sourceProject != null)
            {
                this.sourceProject.Dispose();
            }
            this.Close();
        }

        /// <summary>
        /// Handles the click event for the save script button.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveScript();
        }

        /// <summary>
        /// Handles the click event for the load script button.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadScript();
        }

        /// <summary>
        /// Handles the click event for the Remove Columns button.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnColumnRemoval_Click(object sender, EventArgs e)
        {
            if (cmbPackageForm.SelectedIndex != -1)
            {
                FormName = cmbPackageForm.SelectedItem.ToString();
                PackageColumnRemovalDialog pcrDialog = new PackageColumnRemovalDialog(sourceProject, FormName, fieldsToNull);
                DialogResult result = pcrDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    fieldsToNull = pcrDialog.FieldsToNull;
                }
            }
        }

        /// <summary>
        /// Handles the click event for the Remove Grid Columns button.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnGridDataRemoval_Click(object sender, EventArgs e)
        {
            if (cmbPackageForm.SelectedIndex != -1)
            {
                FormName = cmbPackageForm.SelectedItem.ToString();
                PackageGridRemovalDialog grDialog = new PackageGridRemovalDialog(sourceProject, FormName, gridColumnsToNull);
                DialogResult result = grDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    gridColumnsToNull = grDialog.FieldsToNull;
                }
            }
        }

        /// <summary>
        /// Handles the click event for the Advanced button.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnAdvanced_Click(object sender, EventArgs e)
        {
            if (cmbPackageForm.SelectedIndex != -1)
            {
                PackageAdvancedOptionsDialog paoDialog = new PackageAdvancedOptionsDialog(IncludeCodeTables, IncludeGridData, FormInclusionType);

                if (this.customSalt.Length == 32 && this.customInitVector.Length == 16)
                {
                    paoDialog = new PackageAdvancedOptionsDialog(IncludeCodeTables, IncludeGridData, FormInclusionType, customInitVector, customSalt, customIterations);
                }

                DialogResult result = paoDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    if (paoDialog.Salt.Length == 32 && paoDialog.InitVector.Length == 16)
                    {
                        customSalt = paoDialog.Salt;
                        customInitVector = paoDialog.InitVector;
                        customIterations = paoDialog.Iterations;
                    }

                    this.FormInclusionType = paoDialog.FormInclusionType;
                    this.IncludeCodeTables = paoDialog.IncludeCodeTables;
                    this.IncludeGridData = paoDialog.IncludeGridData;
                }
            }
        }

        private void btnKeyFields_Click(object sender, EventArgs e)
        {
            foreach (View otherForm in this.sourceProject.Views)
            {
                if (otherForm.Name != Form.Name && Epi.ImportExport.ImportExportHelper.IsFormDescendant(otherForm, Form))
                {
                    MsgBox.ShowInformation(ImportExportSharedStrings.ERROR_PACKAGER_CUSTKEYS_WITH_RELATED);
                    return;
                }
            }

            if (cmbPackageForm.SelectedIndex != -1)
            {
                FormName = cmbPackageForm.SelectedItem.ToString();
                SelectMatchFields matchDialog = new SelectMatchFields(sourceProject, sourceProject.Views[FormName], KeyFields);

                DialogResult result = matchDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    KeyFields = matchDialog.KeyFields;
                }
            }
        }

        /// <summary>
        /// Handles the Click event for the row filter button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnRowFilter_Click(object sender, EventArgs e)
        {
            if (cmbPackageForm.SelectedIndex != -1)
            {
                FormName = cmbPackageForm.SelectedItem.ToString();
                PackageRecordSelectionDialog prsDialog = new PackageRecordSelectionDialog(sourceProject, FormName, rowFilterConditions);
                prsDialog.AttachDatabase(sourceProject.CollectedData.GetDbDriver());
                this.rowFilterConditions = prsDialog.RowFilterConditions;

                DialogResult result = prsDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    selectQuery = prsDialog.SelectQuery;
                    rowFilterConditions = prsDialog.RowFilterConditions;
                }
            }
        }

        /// <summary>
        /// Handles the Load event for the dialog
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void PackageForTransportDialog_Load(object sender, EventArgs e)
        {
            EnableDisableButtons();
        }

        /// <summary>
        /// Handles the check changed event for the include timestamp check box
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void checkboxIncludeTime_CheckedChanged(object sender, EventArgs e)
        {
            if (checkboxIncludeTime.Checked)
            {
                AppendTimestamp = true;
            }
            else
            {
                AppendTimestamp = false;
            }
        }

        /// <summary>
        /// Handles the TextChanged event for the package name text box
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtPackageName_TextChanged(object sender, EventArgs e)
        {
            PackageName = txtPackageName.Text;
        }

        /// <summary>
        /// Handles the KeyDown event for the package name text box
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtPackageName_KeyDown(object sender, KeyEventArgs e)
        {
        }

        /// <summary>
        /// Handles the KeyPress event for the package name text box
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtPackageName_KeyPress(object sender, KeyPressEventArgs e)
        {
        }
        #endregion // Event Handlers

        private void lblProjectPath_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
