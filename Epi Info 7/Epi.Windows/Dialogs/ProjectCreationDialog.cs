#region Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Epi.Data;
using Epi.DataSets;
using Epi.Data.Services;
// using MySql.Data;
using Epi.Windows;

using System.Text.RegularExpressions;

#endregion

namespace Epi.Windows.Dialogs
{
    /// <summary>
    /// Dialog for project creation
    /// </summary>
    public partial class ProjectCreationDialog : Epi.Windows.Dialogs.DialogBase
    {
        #region Private Data Members
        private bool projectLocationChanged;
        private string savedConnectionStringDescription = string.Empty;
        #endregion  //Private Data Members

        #region Protected Data Members

        /// <summary>
        /// The connection string for the meta data
        /// </summary>
        protected string metadataConnectionString;

        /// <summary>
        /// The connection string for the collected data
        /// </summary>
        protected string collectedDataConnectionString;

        /// <summary>
        /// The project to be created
        /// </summary>
        protected Project project;
        #endregion  //Protected Data Members

        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ProjectCreationDialog()
        {
            InitializeComponent();
            project = new Project();
            metaDbInfo = new DbDriverInfo();
            collectedDataDBInfo = new DbDriverInfo();
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        public ProjectCreationDialog(MainForm mainForm)
            : base(mainForm)
        {
            #region Input Validation
            if (mainForm == null)
            {
                throw new ArgumentNullException("mainform");
            }

            #endregion  //Input Validation

            InitializeComponent();            
            metaDbInfo = new DbDriverInfo();
            collectedDataDBInfo = new DbDriverInfo();
            txtProjectLocation.ReadOnly = true;
        }

        #endregion  //Constructors

        #region Public Properties

        /// <summary>
        /// Gets the created project
        /// </summary>
        public Project Project
        {
            get
            {
                return project;
            }
        }

        /// <summary>
        /// Gets the user's suggested name for the project's first view
        /// </summary>
        public string ViewName
        {
            get
            {
                return txtViewName.Text;
            }
        }

        /// <summary>
        /// Gets the user's suggested name for the project
        /// </summary>
        public string ProjectName
        {
            get
            {
                return txtProjectName.Text;
            }
        }

        /// <summary>
        /// metaDbInfo Attribute
        /// </summary>
        protected DbDriverInfo metaDbInfo;

        /// <summary>
        /// metaDbInfo property (watch the case!)
        /// </summary>
        public DbDriverInfo MetaDBInfo
        {
            get { return metaDbInfo; }
            set { metaDbInfo = value; }
        }

        /// <summary>
        /// collectedDataDBInfo Attribute
        /// </summary>
        protected DbDriverInfo collectedDataDBInfo;

        /// <summary>
        /// CollectedDataDBInfo Property (watch the case!)
        /// </summary>
        public DbDriverInfo CollectedDataDBInfo
        {
            get { return collectedDataDBInfo; }
            set { collectedDataDBInfo = value; }
        }

        #endregion  //Public Properties

        #region Event Handlers

        /// <summary>
        /// Handles the Click event to select a meta data repository
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnBuildMetadataConnectionString_Click(object sender, EventArgs e)
        {
            GetMetadataConnectionString();
        }

        /// <summary>
        /// Handles the Click event to select a collected data repository
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnBuildCollectedDataConnectionString_Click(object sender, EventArgs e)
        {
           GetCollectedDataConnectionString();
            
            if (rdbSameDb.Checked)
            {
                metaDbInfo.DBCnnStringBuilder = collectedDataDBInfo.DBCnnStringBuilder;
                metaDbInfo.DBName = collectedDataDBInfo.DBName;
                if (collectedDataDBInfo.DBCnnStringBuilder != null)
                {
                    metadataConnectionString = collectedDataDBInfo.DBCnnStringBuilder.ConnectionString;
                }

                txtMetadata.Text = metadataConnectionString;
            }
        }

        /// <summary>
        /// Handles the Change event of the project location's text
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtProjectLocation_TextChanged(object sender, EventArgs e)
        {
            //EnableDisableOkButton();
            //EnableDisableGroups();
            PrepopulateDatabaseLocations();
            projectLocationChanged = true;
        }

        /// <summary>
        /// Handles the Click event of the Browse button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnBrowseProjectLocation_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = txtProjectLocation.Text;
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtProjectLocation.Text = dialog.SelectedPath;
                OnProjectLocationChanged();
                PrepopulateDatabaseLocations();
            }
        }

        /// <summary>
        /// Handles the Leave event of the Project location textbox
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtProjectLocation_Leave(object sender, EventArgs e)
        {
            if (projectLocationChanged)
            {
                OnProjectLocationChanged();
            }
        }

        /// <summary>
        /// Loads the Project Creation Dialog
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void ProjectCreationDialog_Load(object sender, EventArgs e)
        {
            if (!this.DesignMode)
            {
                txtProjectLocation.Text = Configuration.GetNewInstance().Directories.Project;
                WinUtil.FillMetadataDriversList(cbxMetadataDriver);
                WinUtil.FillMetadataDriversList(cbxCollectedDataDriver);

                // Select the default value from config file ...
                Configuration config = Configuration.GetNewInstance();
                cbxMetadataDriver.SelectedValue = config.Settings.DefaultDataDriver;
                cbxCollectedDataDriver.SelectedValue = config.Settings.DefaultDataDriver;
                txtProjectName.Focus();
            }
        }

        /// <summary>
        /// Handles the Click event of the Cancel button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Handles the Click event of the OK button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected virtual void btnOk_Click(object sender, EventArgs e)
        {
            if (ValidateInput() == true)
            {
                bool result = CreateProject();
                if (result == true)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Hide();
                }
                EndBusy();
            }
            else
            {
                ShowErrorMessages();
            }
        }

        /// <summary>
        /// Handles the Change event of the metadata driver selection
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void cbxMetadataDriver_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (GetSelectedMetadataSource())
            {
                case MetadataSource.SameDb:
                case MetadataSource.Xml:
                    cbxMetadataDriver.Enabled = false;
                    txtMetadata.Enabled = false;
                    btnBuildMetadataConnectionString.Enabled = false;
                    txtMetadata.Clear();
                    break;
                case MetadataSource.DifferentDb:
                    cbxMetadataDriver.Enabled = true;
                    txtMetadata.Enabled = true;
                    btnBuildMetadataConnectionString.Enabled = true;
                    BuildMetaData();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the project name
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtProjectName_TextChanged(object sender, EventArgs e)
        {
            EnableDisableOkButton();
        }

        /// <summary>
        /// Handles the TextChanged event of the View name
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtViewName_TextChanged(object sender, EventArgs e)
        {
            EnableDisableOkButton();
        }

        /// <summary>
        /// Handles the Leave event of the project name
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtProjectName_Leave(object sender, EventArgs e)
        {
            // Don't do anything if they leave it blank - they may be trying to press 'Cancel'. Validation for blank project names
            // will be done on an OK button press anyway.
            if (string.IsNullOrEmpty(txtProjectName.Text))
            {
                return;
            }

            bool valid = ValidateProjectName();
            if (!string.IsNullOrEmpty(txtProjectLocation.Text) && valid == true)
            {
                bool isValidFile = IsValidFile();
                if (isValidFile)
                {
                    PrepopulateDatabaseLocations();
                }
            }
        }

        /// <summary>
        /// Handles the Leave event of the view name
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtViewName_Leave(object sender, EventArgs e)
        {
            ValidateViewName();
        }

        private void cbxCollectedDataDriver_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rdbSameDb.Checked)
            {
                if (cbxCollectedDataDriver.SelectedIndex != cbxMetadataDriver.SelectedIndex)
                {
                    cbxMetadataDriver.SelectedIndex = cbxCollectedDataDriver.SelectedIndex;
                }

                if (!cbxCollectedDataDriver.SelectedValue.Equals(Configuration.AccessDriver))
                {
                    txtMetadata.Text = string.Empty;
                    btnBuildCollectedDataConnectionString.Enabled = true;
                }
                else
                {
                    btnBuildCollectedDataConnectionString.Enabled = false;
                    GetMetaConnectionString();
                    txtMetadata.Text = metadataConnectionString;
                    //txtMetadata.Text = txtCollectedData.Text;
                }
            }

            PrepopulateCollectedDataLocation();
        }

        /// <summary>
        /// Handles the Checked event of the Use Same Database radio button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void rdbSameDb_CheckedChanged(object sender, EventArgs e)
        {
            cbxMetadataDriver.Enabled = false;
            txtMetadata.Clear();
            txtMetadata.Enabled = false;
            btnBuildMetadataConnectionString.Enabled = false;
            BuildMetaData();
        }

        /// <summary>
        /// Builds the meta data connection string when the Different Database radion button is selected
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event arguments</param>
        private void rdbDifferentDb_CheckedChanged(object sender, EventArgs e)
        {
            cbxMetadataDriver.Enabled = true;
            txtMetadata.Enabled = true;
            btnBuildMetadataConnectionString.Enabled = true;
            cbxMetadataDriver.SelectedIndex = -1;
            txtMetadata.Text = string.Empty;
            //BuildCollectedData();
            BuildMetaData();
        }

        /// <summary>
        /// Builds the meta data connection string when the Xml radio button is selected
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void rdbXml_CheckedChanged(object sender, EventArgs e)
        {
            cbxMetadataDriver.Enabled = false;
            cbxMetadataDriver.SelectedIndex = -1;
            txtMetadata.Clear();
            txtMetadata.Enabled = false;
            btnBuildMetadataConnectionString.Enabled = false;
            //BuildCollectedData();
            BuildMetaData();
        }
        #endregion  //Event Handlers

        #region Protected Methods
        /// <summary>
        /// Validates input provided on the dialog.
        /// </summary>
        /// <returns></returns>
        protected override bool ValidateInput()
        {
            base.ValidateInput();

            // Validate Project name
            bool validProjectName = ValidateProjectName();
            if (validProjectName == false) { return false; }

            // Validate project location
            if (txtProjectLocation.Text.Trim() == string.Empty)
            {
                if (ErrorMessages.Count == 0) txtProjectLocation.Focus();
                ErrorMessages.Add(SharedStrings.PROJECT_LOCATION_REQUIRED);
            }

            // Validate database
            if (txtCollectedData.Text.Trim() == string.Empty)
            {
                if (ErrorMessages.Count == 0) txtCollectedData.Focus();
                ErrorMessages.Add(SharedStrings.COLLECTED_DATA_INFORMATION_REQUIRED);
            }

            // Validate view name
            bool validViewName = ValidateViewName();
            if (validViewName == false) { return false; };            

            // Validate database
            if (txtMetadata.Text.Trim() == string.Empty)
            {
                if (ErrorMessages.Count == 0) txtMetadata.Focus();
                ErrorMessages.Add(SharedStrings.METADATA_DATABASE_INFORMATION_REQUIRED);
            }
            else
            {
                if (rdbDifferentDb.Checked && txtMetadata.Text.Trim().Equals(txtCollectedData.Text.Trim(), StringComparison.CurrentCultureIgnoreCase))
                {
                    ErrorMessages.Add(SharedStrings.INVALID_METADATA_DATABASE);
                }
            }

            return (ErrorMessages.Count == 0);
        }

        #endregion Protected Methods


        #region Private Methods

        /// <summary>
        /// Enables or disables the OK button depending on the state of other controls on the dialog
        /// </summary>
        protected virtual void EnableDisableOkButton()
        {
            // Check state of Project Name and View Name text boxes. If both have values, enable the OK button
            if (txtProjectName.Text.Length > 0 && txtViewName.Text.Length > 0)
            {
                btnOk.Enabled = true;
            }
            else
            {
                btnOk.Enabled = false;
            }
        }

        /// <summary>
        /// Saves project inform to the configuration file
        /// </summary>
        private void OnProjectLocationChanged()
        {
            bool isValid = IsValidFile();
            if (isValid)
            {
                //Configuration config = Configuration.GetNewInstance();
                //config.Directories.Project = txtProjectLocation.Text.Trim();
                //Configuration.Save(config);
                projectLocationChanged = false;
            }            
        }

        /// <summary>
        /// Obtains metadata connection string once metadata repository browse button is selected
        /// </summary>
        private void GetMetadataConnectionString()
        {
            IConnectionStringGui metaDataCnnDialog = GetSelectedMetadataDriver().GetConnectionStringGuiForNewDb();
            DialogResult result = ((Form)metaDataCnnDialog).ShowDialog();
            if (result == DialogResult.OK)
            {
                metaDbInfo.DBCnnStringBuilder = metaDataCnnDialog.DbConnectionStringBuilder;
                metaDbInfo.DBName = metaDataCnnDialog.PreferredDatabaseName;
                metadataConnectionString = metaDbInfo.DBCnnStringBuilder.ConnectionString;
                txtMetadata.Text = metadataConnectionString;
            }
        }

        /// <summary>
        /// Obtains collected data connection string once collected data repository browse button is selected
        /// </summary>
        private void GetCollectedDataConnectionString()
        {
            IConnectionStringGui collectedDataCnnDialog = null;
            try
            {
                collectedDataCnnDialog = GetSelectedCollectedDataDriver().GetConnectionStringGuiForNewDb();
            }
            catch (Exception ex)
            {
                MsgBox.ShowWarning(SharedStrings.UNABLE_CONNECT_TO_DATABASE + "  " + ex.Message);
            }
            if (collectedDataCnnDialog != null)
            {
                if (!string.IsNullOrEmpty(savedConnectionStringDescription))
                {
                    int splitIndex = savedConnectionStringDescription.IndexOf("::");
                    if (splitIndex > -1)
                    {
                        string serverName = savedConnectionStringDescription.Substring(0, splitIndex);
                        string databaseName = savedConnectionStringDescription.Substring(splitIndex + 2, savedConnectionStringDescription.Length - splitIndex - 2);
                        collectedDataCnnDialog.SetDatabaseName(databaseName);
                        collectedDataCnnDialog.SetServerName(serverName);
                    }
                }

                DialogResult result = ((Form)collectedDataCnnDialog).ShowDialog();
                if (result == DialogResult.OK)
                {
                    savedConnectionStringDescription = collectedDataCnnDialog.ConnectionStringDescription;
                    txtCollectedData.Text = collectedDataCnnDialog.DbConnectionStringBuilder.ToString();
                    collectedDataConnectionString = collectedDataCnnDialog.DbConnectionStringBuilder.ToString();
                    CollectedDataDBInfo.DBCnnStringBuilder = collectedDataCnnDialog.DbConnectionStringBuilder;

                    if (CollectedDataDBInfo.DBCnnStringBuilder != null)
                    {
                        CollectedDataDBInfo.DBCnnStringBuilder.ConnectionString = collectedDataConnectionString;
                    }
                    else
                    {
                        CollectedDataDBInfo.DBCnnStringBuilder = GetSelectedCollectedDataDriver().RequestDefaultConnection("Collected_" + txtProjectName.Text.Trim());
                    }
                    CollectedDataDBInfo.DBName = collectedDataCnnDialog.PreferredDatabaseName;
                }
            }
            else
            {
                DialogResult result = ((Form)collectedDataCnnDialog).ShowDialog();
                if (result == DialogResult.OK)
                {
                    MsgBox.ShowWarning(SharedStrings.SELECT_DATABASE);
                }
            }
        }

        /// <summary>
        /// Obtains the meta data driver factory
        /// </summary>
        /// <returns>Instance of the metadata driver factory</returns>
        private IDbDriverFactory GetSelectedMetadataDriver()
        {
            try
            {
                string dataDriverType = cbxMetadataDriver.SelectedValue.ToString();
                return DbDriverFactoryCreator.GetDbDriverFactory(dataDriverType);
            }
            catch (Exception ex)
            {
                MsgBox.ShowWarning(SharedStrings.UNABLE_CREATE_DATABASE + "  " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Obtains the collected data driver factory
        /// </summary>
        /// <returns>Instance of the collected data driver factory</returns>
        private IDbDriverFactory GetSelectedCollectedDataDriver()
        {
            string dataDriverType = null;
            try
            {
                if (cbxCollectedDataDriver.SelectedValue != null)
                {
                    dataDriverType = cbxCollectedDataDriver.SelectedValue.ToString();
                }
                else
                {
                    throw new Exception(SharedStrings.SELECT_DATABASE_TYPE);
                }
                IDbDriverFactory dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(dataDriverType);
                return dbFactory;
            }
            catch (Exception ex)
            {
                MsgBox.ShowWarning(SharedStrings.UNABLE_CREATE_DATABASE + "  " + ex.Message);
                return null;
            }
        }        

        /// <summary>
        /// Saves project information
        /// </summary>
        protected virtual bool CreateProject()
        {
            BeginBusy(SharedStrings.CREATING_PROJECT);

            project = new Project();
            project.Name = txtProjectName.Text;

            project.Location = Path.Combine(txtProjectLocation.Text, txtProjectName.Text);

            if (collectedDataDBInfo.DBCnnStringBuilder.ContainsKey("Provider") && collectedDataDBInfo.DBCnnStringBuilder["Provider"] == "Microsoft.Jet.OLEDB.4.0")
            {
                collectedDataDBInfo.DBCnnStringBuilder["Data Source"] = project.FilePath.Substring(0, project.FilePath.Length - 4) + ".mdb";
            }
            
            if (!Directory.Exists(project.Location))
            {
                Directory.CreateDirectory(project.Location);
            }

            project.Id = project.GetProjectId();
            if (File.Exists(project.FilePath))
            {
                DialogResult dr = MsgBox.Show(string.Format(SharedStrings.PROJECT_ALREADY_EXISTS, project.FilePath), SharedStrings.PROJECT_ALREADY_EXISTS_TITLE, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                switch (dr)
                {
                    case DialogResult.Yes:
                        break;
                    case DialogResult.No:
                        return false;
                }
            }            

            project.Description = txtProjectDescription.Text;

            // Collected data ...
            project.CollectedDataDbInfo = this.collectedDataDBInfo;
            project.CollectedDataDriver = cbxCollectedDataDriver.SelectedValue.ToString();
            project.CollectedDataConnectionString = this.collectedDataDBInfo.DBCnnStringBuilder.ToString();
            try
            {
                project.CollectedData.Initialize(collectedDataDBInfo, cbxCollectedDataDriver.SelectedValue.ToString(), true);
            }
            catch (Exception ex)
            {
                MsgBox.Show(string.Format("{0}{1}{1}{2}", SharedStrings.UNABLE_TO_INITIALIZE_PROJECT, Environment.NewLine, ex.Message), SharedStrings.UNABLE_TO_INITIALIZE_PROJECT, MessageBoxButtons.OK, MessageBoxIcon.None);
                return false;
            }

            // Check that there isn't an Epi 7 project already here.
            if (project.CollectedDataDriver != "Epi.Data.Office.AccessDBFactory, Epi.Data.Office")
            {
                List<string> tableNames = new List<string>();
                tableNames.Add("metaBackgrounds");
                tableNames.Add("metaDataTypes");
                tableNames.Add("metaDbInfo");
                tableNames.Add("metaFields");
                tableNames.Add("metaFieldTypes");
                tableNames.Add("metaGridColumns");
                tableNames.Add("metaImages");
                tableNames.Add("metaLayerRenderTypes");
                tableNames.Add("metaLayers");
                tableNames.Add("metaMapLayers");
                tableNames.Add("metaMapPoints");
                tableNames.Add("metaMaps");
                tableNames.Add("metaPages");
                tableNames.Add("metaPatterns");
                tableNames.Add("metaPrograms");
                tableNames.Add("metaViews");

                bool projectExists = false;
                try
                {
                    foreach (string s in tableNames)
                    {
                        if (project.CollectedData.TableExists(s))
                        {
                            projectExists = true;
                            break;
                        }
                    }
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    Epi.Windows.MsgBox.ShowError("There was a problem accessing the database. Check to make sure you have adequate permissions to access the database and try again.", ex);
                    return false;
                }

                if (projectExists)
                {
                    DialogResult result = Epi.Windows.MsgBox.Show(SharedStrings.WARNING_PROJECT_MAY_ALREADY_EXIST, SharedStrings.WARNING_PROJECT_MAY_ALREADY_EXIST_SHORT, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.Cancel)
                    {
                        Logger.Log(DateTime.Now + ":  " + "Project creation aborted by user [" + System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString() + "] after being prompted to overwrite existing Epi Info 7 project metadata.");
                        return false;
                    }
                    else
                    {
                        Logger.Log(DateTime.Now + ":  " + "Project creation proceeded by user [" + System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString() + "] after being prompted to overwrite existing Epi Info 7 project metadata.");
                    }
                }
            }

            Logger.Log(DateTime.Now + ":  " + string.Format("Project [{0}] created in {1} by user [{2}].", project.Name, project.Location, System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString()));

            // Metadata ..
            project.MetadataSource = GetSelectedMetadataSource();
            
            switch (project.MetadataSource)
            {
                case MetadataSource.SameDb: // Metadata is stored in the same db.            
                    MetadataDbProvider typedMetadata = project.Metadata as MetadataDbProvider;
                    // Get Collected data's Db driver and attach it to Metadata.
                    typedMetadata.AttachDbDriver(project.CollectedData.GetDbDriver());
                    typedMetadata.CreateMetadataTables();
                    break;

                case MetadataSource.DifferentDb: // Metadata to be stored in a different database            
                    // Get Metadata driver and connection string from the dialog and initialize metadata object.
                    project.MetadataDriver = cbxMetadataDriver.SelectedValue.ToString();
                    project.MetadataConnectionString = metadataConnectionString;
                    project.MetaDbInfo = metaDbInfo;

                    typedMetadata = project.Metadata as MetadataDbProvider;
                    typedMetadata.Initialize(metaDbInfo, cbxMetadataDriver.SelectedValue.ToString(), true);
                    break;
            }

            // KKM4 TODO: Temporary band-aid. Revisit ASAP.
            if (!string.IsNullOrEmpty(txtViewName.Text))
            {
                // Creation of project complete. Now create the view.
                project.CreateView(txtViewName.Text);
            }
            
            try
            {
                project.Save();
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                MsgBox.ShowException(ex);
                return false;
            }
        }

        /// <summary>
        /// Prepopulates the database location for the collected data repository and metadata
        /// These are displayed on the dialog once the user 
        /// </summary>
        protected void PrepopulateDatabaseLocations()
        {
            if (!string.IsNullOrEmpty(txtProjectName.Text) && !string.IsNullOrEmpty(txtProjectLocation.Text))
            {
                if (cbxCollectedDataDriver.SelectedValue != null)
                {
                    //LINUXTEST
                    //WinUtil.ShowTraceMessage("same radio is checked");
                    if (rdbSameDb.Checked == true)
                    {
                        try
                        {
                            if (collectedDataDBInfo.DBCnnStringBuilder != null)
                            {
                                if (String.IsNullOrEmpty(collectedDataDBInfo.DBCnnStringBuilder.ToString()))
                                {
                                    collectedDataDBInfo.DBCnnStringBuilder = GetSelectedCollectedDataDriver().RequestDefaultConnection(txtProjectName.Text.Trim());
                                }
                            }
                            else
                            {
                                collectedDataDBInfo.DBCnnStringBuilder = GetSelectedCollectedDataDriver().RequestDefaultConnection(txtProjectName.Text.Trim(), txtProjectName.Text.Trim());
                            }

                        }
                        catch (Exception ex)
                        {
                            collectedDataDBInfo.DBCnnStringBuilder = null;
                            MsgBox.ShowWarning(SharedStrings.WARNING_CANNOT_CONNECT_TO_COLLECTED_DATA_DATABASE + "  " + ex.StackTrace);
                        }

                        try
                        {
                            metaDbInfo.DBCnnStringBuilder = collectedDataDBInfo.DBCnnStringBuilder;
                        }
                        catch (Exception ex)
                        {
                            metaDbInfo.DBCnnStringBuilder = null;
                            MsgBox.ShowWarning(SharedStrings.WARNING_CANNOT_CONNECT_TO_METADATA_DATABASE + "  " + ex.StackTrace);
                        }

                        try
                        {
                            txtCollectedData.Text = collectedDataDBInfo.DBCnnStringBuilder.ConnectionString;

                        }
                        catch (Exception ex)
                        {
                            metaDbInfo.DBCnnStringBuilder = null;
                            collectedDataDBInfo.DBCnnStringBuilder = null;
                            txtMetadata.Text = null;
                            txtCollectedData.Text = null;
                            MsgBox.ShowWarning(SharedStrings.WARNING_CANNOT_SET_COLLECTED_DATA_TEXT + "  " + ex.StackTrace);
                        }

                        txtMetadata.Text = txtCollectedData.Text;
                        collectedDataConnectionString = txtCollectedData.Text;
                        metadataConnectionString = collectedDataConnectionString;
                    }
                    else
                    {
                        //LINUXTEST
                        //WinUtil.ShowTraceMessage("same radio is NOT checked", "err");
                        if (!cbxMetadataDriver.Text.Trim().Equals(SharedStrings.XML))
                        {
                            metaDbInfo.DBCnnStringBuilder = null;
                            txtMetadata.Text = string.Empty;
                            metadataConnectionString = txtMetadata.Text;
                        }
                    }
                }
            }
            else
            {
                metaDbInfo.DBCnnStringBuilder = null;
                collectedDataDBInfo.DBCnnStringBuilder = null;
                txtMetadata.Text = string.Empty;
                txtCollectedData.Text = string.Empty;
                collectedDataConnectionString = string.Empty;
            }
        }

        /// <summary>
        /// Determines if the user specified a valid
        /// directory to store the project.  If not,
        /// it gives the user the option to create the directory.
        /// </summary>
        private bool CheckIfProjectDirectoryExists()
        {
            bool valid = true;
            string path = txtProjectLocation.Text.Trim();
            try
            {
                //Determine whether the directory exists.
                if (!Directory.Exists(path) && !string.IsNullOrEmpty(path))
                {
                    //Ensure valid drive is entered
                    bool validDrive = false;
                    String[] dirs = Directory.GetLogicalDrives();
                    foreach (string dir in dirs)
                    {
                        if (dir == @path.Substring(0, 3))
                        {
                            validDrive = true;
                            break;
                        }
                    }

                    if (validDrive)
                    {
                        DialogResult result = MsgBox.ShowQuestion(SharedStrings.CREATE_DIRECTORY);
                        if (result == DialogResult.Yes)
                        {
                            try
                            {
                                //create the directory
                                DirectoryInfo di = Directory.CreateDirectory(path);
                            }
                            catch (Exception ex)
                            {
                                //creation of the directory failed
                                MsgBox.ShowException(ex);
                                txtProjectLocation.Text = string.Empty;
                                txtProjectLocation.Focus();
                                valid = false;
                            }
                        }
                        else
                        {
                            txtProjectLocation.Text = string.Empty;
                            txtProjectLocation.Focus();
                            valid = false;
                        }
                    }
                    else
                    {
                        MessageBox.Show(SharedStrings.INVALID_PROJECT_LOCATION, SharedStrings.INVALID_PROJECT_LOCATION_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtProjectLocation.Text = string.Empty;
                        txtProjectLocation.Focus();
                        valid = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MsgBox.ShowException(ex);
                valid = false;
            }
            return valid;
        }

        /// <summary>
        /// Checks to see if the project file already exists
        /// </summary>
        /// <returns>True/False depending upon whether the project file already exists</returns>
        private bool CheckIfProjectFileAlreadyExists()
        {
            bool exists = false;
            string fullPath = Path.Combine(txtProjectLocation.Text, txtProjectName.Text + Epi.FileExtensions.EPI_PROJ);
            if (File.Exists(fullPath))
            {
              exists = true;
            }
            return exists;
        }

        /// <summary>
        /// Checks to see if the project file name and location are valid
        /// </summary>
        /// <returns>True/False depending upon whether the project name and path are valid</returns>
        private bool IsValidFile()
        {
            bool exists = false;
            if (CheckIfProjectDirectoryExists())
            {
                exists = true;
            }
            return exists;
        }

        //DbConnectionStringBuilder cnnInfo;
        /// <summary>
        /// Obtains the meta data collection string of the project file path input by the user
        /// </summary>
        private void GetMetaConnectionString()
        {
            IDbDriverFactory metaDBFactory = GetSelectedMetadataDriver();
            DbDriverInfo dbInfo = new DbDriverInfo();
            dbInfo.DBCnnStringBuilder = metaDBFactory.RequestNewConnection(txtMetadata.Text.Trim());
            this.MetaDBInfo = dbInfo;
            if (this.metaDbInfo.DBCnnStringBuilder != null)
            {
                metadataConnectionString = this.metaDbInfo.DBCnnStringBuilder.ConnectionString;
            }
        }

        /// <summary>
        /// Obtains the collected data collection string of the project file path input by the user 
        /// </summary>
        private void GetCollectedConnectionString()
        {
            IDbDriverFactory collectedDBFactory = GetSelectedCollectedDataDriver();

            DbDriverInfo dbInfo = new DbDriverInfo();
            dbInfo.DBCnnStringBuilder = collectedDBFactory.RequestNewConnection(txtCollectedData.Text.Trim());
            this.CollectedDataDBInfo = dbInfo;
            if (this.CollectedDataDBInfo.DBCnnStringBuilder != null)
            {
                collectedDataConnectionString = this.CollectedDataDBInfo.DBCnnStringBuilder.ToString();
            }
        }

        /// <summary>
        /// Prepopulates collected data information on the interface
        /// </summary>
        private void PrepopulateCollectedDataLocation()
        {
            if (!string.IsNullOrEmpty(txtProjectName.Text) && !string.IsNullOrEmpty(txtProjectLocation.Text))
            {
                {
                    //DEFECT # 357 always display the default connection string when the DDL changes.
                    //if (String.IsNullOrEmpty(collectedDataDBInfo.DBCnnStringBuilder.ToString()))
                    //{
                        collectedDataDBInfo.DBCnnStringBuilder = GetSelectedCollectedDataDriver().RequestDefaultConnection(txtProjectName.Text.Trim());
                    //}
                    txtCollectedData.Text = collectedDataDBInfo.DBCnnStringBuilder.ConnectionString;
                    collectedDataConnectionString = txtCollectedData.Text;
                }
            }
            else
            {
                collectedDataDBInfo.DBCnnStringBuilder = null;
                txtCollectedData.Text = string.Empty;
                collectedDataConnectionString = string.Empty;
            }
        }

        /// <summary>
        /// Validates the project name
        /// </summary>
        private bool ValidateProjectName()
        {
            bool valid = true;
            string validationMessage = string.Empty;

            valid = Project.IsValidProjectName(ProjectName, ref validationMessage);

            if (!valid)
            {
                MsgBox.ShowError(validationMessage);
            }

            return valid;
        }

        /// <summary>
        /// Validates the view name
        /// </summary>
        private bool ValidateViewName()
        {
            bool valid = true;
            string validationMessage = string.Empty;            
            
            valid = View.IsValidViewName(ViewName, ref validationMessage);            

            if (!valid)
            {
                MsgBox.ShowError(validationMessage);
            }

            return valid;
        }

        /// <summary>
        /// Builds the metadata connection string
        /// </summary>
        private void BuildMetaData()
        {
            if (cbxCollectedDataDriver.SelectedValue != null)
            {
                if (rdbSameDb.Checked)
                {
                    if (!(cbxCollectedDataDriver.Text.Trim().Equals(SharedStrings.XML)))
                    {
                        cbxMetadataDriver.SelectedValue = cbxCollectedDataDriver.SelectedValue;
                        txtMetadata.Text = txtCollectedData.Text;
                    }
                    else
                    {
                        cbxMetadataDriver.SelectedIndex = -1;
                        txtMetadata.Text = string.Empty;
                    }
                }
                else if (rdbDifferentDb.Checked)
                {
                    if (cbxMetadataDriver.SelectedValue != null)
                    {
                        metaDbInfo.DBCnnStringBuilder = GetSelectedMetadataDriver().RequestDefaultConnection(String.Format("{0}{1}", SharedStrings.METADATA_PREFIX, txtProjectName.Text.Trim()));
                        txtMetadata.Text = metaDbInfo.DBCnnStringBuilder.ConnectionString;
                    }
                    else
                    {
                        cbxMetadataDriver.SelectedValue = -1;
                        txtMetadata.Text = string.Empty;
                    }
                }
                else if (rdbXml.Checked)
                {
                    string projectPath = Path.Combine(txtProjectLocation.Text, txtProjectName.Text + Epi.FileExtensions.EPI_PROJ);
                    txtMetadata.Text = projectPath;
                    metadataConnectionString = txtMetadata.Text;
                    
                }
            }
            //}
        }

        /// <summary>
        /// Builds the collected data connection string
        /// </summary>
        private void BuildCollectedData()
        {
            if (!string.IsNullOrEmpty(txtProjectName.Text) && !string.IsNullOrEmpty(txtProjectLocation.Text))
            {
                if (cbxCollectedDataDriver.SelectedValue != null)
                {
                    if (rdbSameDb.Checked)
                    {
                        if (!(cbxCollectedDataDriver.Text.Trim().Equals(SharedStrings.XML)))
                        {
                            if (cbxCollectedDataDriver.SelectedValue.Equals(Configuration.AccessDriver))
                            {
                                collectedDataDBInfo.DBCnnStringBuilder = GetSelectedCollectedDataDriver().RequestDefaultConnection(txtProjectName.Text.Trim());
                                txtCollectedData.Text = CollectedDataDBInfo.DBCnnStringBuilder.ConnectionString;
                            }
                            else
                            {
                                CollectedDataDBInfo.DBCnnStringBuilder = null;
                                txtCollectedData.Text = string.Empty;
                            }

                            cbxMetadataDriver.SelectedValue = cbxCollectedDataDriver.SelectedValue;
                            txtMetadata.Text = txtCollectedData.Text.Trim();
                        }

                    }
                    else if (rdbDifferentDb.Checked)
                    {
                        cbxMetadataDriver.SelectedIndex = -1;
                        txtMetadata.Text = string.Empty;
                    }
                    else if (rdbXml.Checked)
                    {
                        string projectPath = Path.Combine(txtProjectLocation.Text, txtProjectName.Text + Epi.FileExtensions.EPI_PROJ);
                        txtMetadata.Text = projectPath;
                        metadataConnectionString = txtMetadata.Text;
                        
                    }
                }
            }
        }

        private MetadataSource GetSelectedMetadataSource()
        {
            if (rdbSameDb.Checked) return MetadataSource.SameDb;
            else if (rdbDifferentDb.Checked) return MetadataSource.DifferentDb;
            else if (rdbXml.Checked) return MetadataSource.Xml;
            else return MetadataSource.Unknown;
        }

        #endregion  Private Methods
    }
}
