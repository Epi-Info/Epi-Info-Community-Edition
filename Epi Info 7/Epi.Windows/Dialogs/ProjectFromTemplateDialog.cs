#region Namespaces

using System;
using System.Collections;
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
using Epi.Windows;
using System.Text.RegularExpressions;

#endregion

namespace Epi.Windows.Dialogs
{
    /// <summary>
    /// Dialog for project creation
    /// </summary>
    public partial class ProjectFromTemplateDialog : Epi.Windows.Dialogs.DialogBase
    {
        #region Private Data Members
        private bool projectLocationChanged;
        private DataTable projectTable;
        private ListViewColumnSorter lvwColumnSorter;
        #endregion

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
        /// <summary>
        /// The selected project template fully qualified path
        /// </summary>
        protected String projectTemplatePath;
        #endregion  //Protected Data Members

        public string SelectedTemplatePath{ get; set; }

        public String ProjectName
        {
            get { return txtProjectName.Text; }
        }

        public String ProjectConnectionString
        {
            get { return collectedDataConnectionString; }
        }
        public String ProjectLocation
        {
            get { return txtProjectLocation.Text; }
        }
        public DbDriverInfo DriverInfo
        {
            get { return collectedDataDBInfo; }
        }
        public String DataDBInfo
        {
            get { return cbxCollectedDataDriver.SelectedValue.ToString(); }
        }

        #region Constructors
       
        public ProjectFromTemplateDialog(string selectedTemplatePath)
        {
            SelectedTemplatePath = selectedTemplatePath;
            
            InitializeComponent();
            project = new Project();
            metaDbInfo = new DbDriverInfo();
            collectedDataDBInfo = new DbDriverInfo();

            projectTemplateListView.Dock = DockStyle.None;
            projectTemplateListView.View = System.Windows.Forms.View.Details;
            projectTemplateListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            projectTemplateListView.GridLines = false;
            projectTemplateListView.Scrollable = true;
            projectTemplateListView.FullRowSelect = true;
            projectTemplateListView.AllowColumnReorder = true;
            projectTemplateListView.HideSelection = false;
            
            ColumnHeader templateName = new ColumnHeader();
            templateName.Text = "Template Name";
            templateName.Width = -1;

            ColumnHeader creationDate = new ColumnHeader();
            creationDate.Text = "Creation Date";
            creationDate.Width = -1;

            ColumnHeader description = new ColumnHeader();
            description.Text = "Description";
            description.Width = -1;

            ColumnHeader path = new ColumnHeader();
            path.Text = "Path";
            path.Width = -1;

            projectTemplateListView.Columns.AddRange(
                new ColumnHeader[] 
                { 
                    templateName,
                    creationDate,
                    description,
                    path
                });

            lvwColumnSorter = new ListViewColumnSorter();
            projectTemplateListView.ListViewItemSorter = lvwColumnSorter;

            Epi.Template template = new Template();
            projectTable = template.GetProjectTable("Projects");

            foreach (DataRow row in template.GetProjectTable("Forms").Rows)
            {
                projectTable.Rows.Add(row.ItemArray);
            }

            foreach (DataRow row in projectTable.Rows)
            {
                string itemText = row["TemplateName"].ToString();

                if(string.IsNullOrEmpty(itemText) == false)
                {
                    ListViewItem item = new ListViewItem(itemText, 0);

                    string subItemText = row["TemplateCreateDate"].ToString();
                    ListViewItem.ListViewSubItem subItem = new ListViewItem.ListViewSubItem();
                    subItem.Text = subItemText;
                    subItem.Tag = System.DateTime.Now;

                    item.SubItems.Add(subItem);
                    item.SubItems.Add("");
                    item.SubItems.Add(row["TemplatePath"].ToString());
                    item.Tag = row["TemplatePath"].ToString();
                    projectTemplateListView.Items.Add(item);
                }
            }
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        public ProjectFromTemplateDialog(MainForm mainForm)
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

        /// <summary>
        /// ProjectTemplatePath Property
        /// </summary>
        public String ProjectTemplatePath
        {
            get { return projectTemplatePath; }
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
            if(this.projectTemplateListView.SelectedItems.Count == 0) 
            {
                Epi.Windows.MsgBox.ShowInformation("Please select a project template.");
                return;
            }
           
            GetCollectedDataConnectionString();
            
            metaDbInfo.DBCnnStringBuilder = collectedDataDBInfo.DBCnnStringBuilder;
            metaDbInfo.DBName = collectedDataDBInfo.DBName;
            if (collectedDataDBInfo.DBCnnStringBuilder != null)
            {
                metadataConnectionString = collectedDataDBInfo.DBCnnStringBuilder.ConnectionString;
            }
            else
            {
                throw new Exception("Please enter project name.");
            } 
        }

        /// <summary>
        /// Handles the Change event of the project location's text
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtProjectLocation_TextChanged(object sender, EventArgs e)
        {
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
                WinUtil.FillMetadataDriversList(cbxCollectedDataDriver);

                Configuration config = Configuration.GetNewInstance();
                cbxCollectedDataDriver.SelectedValue = config.Settings.DefaultDataDriver;
                txtProjectName.Focus();
            }

            foreach (ListViewItem item in projectTemplateListView.Items)
            {
                string fullPath = item.SubItems[3].Text;

                if (fullPath.Contains(SelectedTemplatePath))
                {
                    item.Selected = true;
                    item.Focused = true;
                    break;
                }
            }

            projectTemplateListView.ColumnClick += new ColumnClickEventHandler(projectTemplateListView_ColumnClick);
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
                this.DialogResult = DialogResult.OK;
                this.Hide();
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

                    break;
                case MetadataSource.DifferentDb:
                    //BuildMetaData();
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

            PrepopulateCollectedDataLocation();

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

        private void cbxCollectedDataDriver_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (!cbxCollectedDataDriver.SelectedValue.Equals(Configuration.AccessDriver))
            {
                btnBuildCollectedDataConnectionString.Enabled = true;
            }
            else
            {
                btnBuildCollectedDataConnectionString.Enabled = false;
                GetMetaConnectionString();
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
            //BuildMetaData();
        }

        /// <summary>
        /// Builds the meta data connection string when the Different Database radion button is selected
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event arguments</param>
        private void rdbDifferentDb_CheckedChanged(object sender, EventArgs e)
        {
            //BuildMetaData();
        }

        /// <summary>
        /// Builds the meta data connection string when the Xml radio button is selected
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void rdbXml_CheckedChanged(object sender, EventArgs e)
        {
            //BuildMetaData();
        }
        #endregion  //Event Handlers

        #region Protected Methods
        ///// <summary>
        ///// Validates input provided on the dialog.
        ///// </summary>
        ///// <returns></returns>
        //protected override bool ValidateInput()
        //{
        //    base.ValidateInput();

        //    // Validate Project name
        //    if (txtProjectName.Text.Trim() == string.Empty)
        //    {
        //        if (ErrorMessages.Count == 0) txtProjectName.Focus();
        //        ErrorMessages.Add(SharedStrings.PROJECT_NAME_MISSING);
        //    }
        //    else
        //    {                
        //        bool valid = ValidateProjectName();
        //        if (valid == false) ErrorMessages.Add(SharedStrings.INVALID_PROJECT_NAME);
        //    }

        //    // Validate project location
        //    if (txtProjectLocation.Text.Trim() == string.Empty)
        //    {
        //        if (ErrorMessages.Count == 0) txtProjectLocation.Focus();
        //        ErrorMessages.Add(SharedStrings.PROJECT_LOCATION_REQUIRED);
        //    }

        //    // Validate database
        //    if (txtCollectedData.Text.Trim() == string.Empty)
        //    {
        //        if (ErrorMessages.Count == 0) txtCollectedData.Focus();
        //        ErrorMessages.Add(SharedStrings.COLLECTED_DATA_INFORMATION_REQUIRED);
        //    }

        //    return (ErrorMessages.Count == 0);
        //}

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Enables or disables the OK button depending on the state of other controls on the dialog
        /// </summary>
        protected virtual void EnableDisableOkButton()
        {
            // Check state of Project Name and View Name text boxes. If both have values, enable the OK button
            if (txtProjectName.Text.Length > 0)
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
                Configuration config = Configuration.GetNewInstance();
                config.Directories.Project = txtProjectLocation.Text.Trim();
                Configuration.Save(config);
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
                collectedDataCnnDialog.SetDatabaseName(this.ProjectName);
                
                DialogResult result = ((Form)collectedDataCnnDialog).ShowDialog();
                if (result == DialogResult.OK)
                {
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
                string dataDriverType = cbxCollectedDataDriver.SelectedValue.ToString();
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

            // Collected data ...
            project.CollectedDataDbInfo = this.collectedDataDBInfo;
            project.CollectedDataDriver = cbxCollectedDataDriver.SelectedValue.ToString();
            project.CollectedDataConnectionString = this.collectedDataDBInfo.DBCnnStringBuilder.ToString();
            project.CollectedData.Initialize(collectedDataDBInfo, cbxCollectedDataDriver.SelectedValue.ToString(), true);


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
                foreach (string s in tableNames)
                {
                    if (project.CollectedData.TableExists(s))
                    {
                        projectExists = true;
                        break;
                    }
                }

                if (projectExists)
                {
                    DialogResult result = Epi.Windows.MsgBox.Show("An Epi Info 7 project may already exist in the specified database. Continuing the project creation process will cause the project's metadata to be overwritten.\n\n" +
                        "Do you wish to proceed with the creation of the project and overwrite any existing project metadata?  This action cannot be undone.", "Overwrite existing Epi Info 7 project metadata", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
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
            MetadataDbProvider typedMetadata = project.Metadata as MetadataDbProvider;
            typedMetadata.AttachDbDriver(project.CollectedData.GetDbDriver());
            typedMetadata.CreateMetadataTables();

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
                    if (true)
                    {
                        try
                        {
                            if (collectedDataDBInfo.DBCnnStringBuilder != null)
                            {
                                if (String.IsNullOrEmpty(collectedDataDBInfo.DBCnnStringBuilder.ToString()))
                                {
                                    collectedDataDBInfo.DBCnnStringBuilder = GetSelectedCollectedDataDriver().RequestDefaultConnection(txtProjectName.Text.Trim(), txtProjectName.Text.Trim());
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
                            txtCollectedData.Text = null;
                            MsgBox.ShowWarning(SharedStrings.WARNING_CANNOT_SET_COLLECTED_DATA_TEXT + "  " + ex.StackTrace);
                        }

                        collectedDataConnectionString = txtCollectedData.Text;
                        metadataConnectionString = collectedDataConnectionString;
                    }
                    else
                    {
                        metaDbInfo.DBCnnStringBuilder = null;
                        metadataConnectionString = string.Empty;
                    }
                }
            }
            else
            {
                metaDbInfo.DBCnnStringBuilder = null;
                collectedDataDBInfo.DBCnnStringBuilder = null;
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
                if (!Directory.Exists(path) && !string.IsNullOrEmpty(path))
                {
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

                    if (validDrive == false)
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
            IDbDriverFactory metaDBFactory = GetSelectedCollectedDataDriver();
            DbDriverInfo dbInfo = new DbDriverInfo();
            dbInfo.DBCnnStringBuilder = metaDBFactory.RequestNewConnection(txtCollectedData.Text.Trim());
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
                    collectedDataDBInfo.DBCnnStringBuilder = GetSelectedCollectedDataDriver().RequestDefaultConnection(txtProjectName.Text.Trim(), txtProjectName.Text.Trim());
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
        /// Builds the collected data connection string
        /// </summary>
        private void BuildCollectedData()
        {
            if (!string.IsNullOrEmpty(txtProjectName.Text) && !string.IsNullOrEmpty(txtProjectLocation.Text))
            {
                if (cbxCollectedDataDriver.SelectedValue != null)
                {
                    if (cbxCollectedDataDriver.SelectedValue.Equals(Configuration.AccessDriver))
                    {
                        collectedDataDBInfo.DBCnnStringBuilder = GetSelectedCollectedDataDriver().RequestDefaultConnection(txtProjectName.Text.Trim(), txtProjectName.Text.Trim());
                        txtCollectedData.Text = CollectedDataDBInfo.DBCnnStringBuilder.ConnectionString;
                    }
                    else
                    {
                        CollectedDataDBInfo.DBCnnStringBuilder = null;
                        txtCollectedData.Text = string.Empty;
                    }
                }
            }
        }

        private MetadataSource GetSelectedMetadataSource()
        {
            return MetadataSource.SameDb;
        }

        private void projectTemplateListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if ( e.Column == lvwColumnSorter.SortColumn )
            {
	            if (lvwColumnSorter.Order == SortOrder.Ascending)
	            {
		            lvwColumnSorter.Order = SortOrder.Descending;
	            }
	            else
	            {
		            lvwColumnSorter.Order = SortOrder.Ascending;
	            }
            }
            else
            {
	            lvwColumnSorter.SortColumn = e.Column;
	            lvwColumnSorter.Order = SortOrder.Ascending;
            }

            this.projectTemplateListView.Sort();
        }

        #endregion  Private Methods

        private void projectTemplateListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((System.Windows.Forms.ListView)sender).SelectedItems.Count > 0)
            {
                projectTemplatePath = ((System.Windows.Forms.ListView)sender).SelectedItems[0].Tag.ToString();
                string select = string.Format("TemplatePath = '{0}'", projectTemplatePath);
                DataRow[] rows = projectTable.Select(select);

                txtProjectName.Enabled = true;

                string validationMessage = string.Empty;
                string projectNameFromTemplate = rows[0][ColumnNames.NAME].ToString();
                projectNameFromTemplate = Regex.Replace(projectNameFromTemplate, "[^a-zA-Z0-9_]", "");
                string projectNameCandidate = projectNameFromTemplate;

                if (string.IsNullOrEmpty(projectNameFromTemplate) == false)
                {
                    int i = 2;

                    while (IsValid_NEW_ProjectName(projectNameCandidate) == false)
                    {
                        projectNameCandidate = string.Format("{0}{1}", projectNameFromTemplate, i++);
                    }
                }

                txtProjectName.Text = projectNameCandidate;
                txtProjectName_TextChanged(new Object(), new EventArgs());
                txtProjectName_Leave(new Object(), new EventArgs());
            }
        }

        private bool IsValid_NEW_ProjectName(string projectNameCandidate)
        {
            string validationMessage = string.Empty;
            txtProjectName.Text = projectNameCandidate;
            BuildCollectedData();

            if (Project.IsValidProjectName(projectNameCandidate, ref validationMessage) == false)
            {
                return false;
            }

            project = new Project();
            project.Name = projectNameCandidate;

            project.Location = Path.Combine(txtProjectLocation.Text, projectNameCandidate);

            if (collectedDataDBInfo.DBCnnStringBuilder != null 
                && collectedDataDBInfo.DBCnnStringBuilder.ContainsKey("Provider") 
                && collectedDataDBInfo.DBCnnStringBuilder["Provider"] == "Microsoft.Jet.OLEDB.4.0")
            {
                collectedDataDBInfo.DBCnnStringBuilder["Data Source"] = project.FilePath.Substring(0, project.FilePath.Length - 4) + ".mdb";
            }

            if (File.Exists(project.FilePath))
            {
                return false;
            }

            return true;
        }

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

            return (ErrorMessages.Count == 0);
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
                txtProjectName.Focus();
            }

            return valid;
        }
    }

// <summary>
/// This class is an implementation of the 'IComparer' interface.
/// </summary>
public class ListViewColumnSorter : IComparer
{
	private int ColumnToSort;
    private SortOrder OrderOfSort;
	private CaseInsensitiveComparer ObjectCompare;

	public ListViewColumnSorter()
	{
		ColumnToSort = 0;
		OrderOfSort = SortOrder.Ascending;
		ObjectCompare = new CaseInsensitiveComparer();
	}

	public int Compare(object a, object b)
	{
		int compareResult;
		ListViewItem listviewA, listviewB;

        listviewA = (ListViewItem)a;
        listviewB = (ListViewItem)b;

        compareResult = ObjectCompare.Compare(listviewA.SubItems[ColumnToSort].Text, listviewB.SubItems[ColumnToSort].Text);

        if(listviewA.SubItems[ColumnToSort].Tag is System.DateTime)
        {
            DateTime dateTimeA;
            DateTime dateTimeB;
            if (DateTime.TryParse(listviewA.SubItems[ColumnToSort].Text, out dateTimeA) && DateTime.TryParse(listviewB.SubItems[ColumnToSort].Text, out dateTimeB))
            {
                compareResult = ObjectCompare.Compare(dateTimeA, dateTimeB);
            }
        }

		if (OrderOfSort == SortOrder.Ascending)
		{
			return compareResult;
		}
		else if (OrderOfSort == SortOrder.Descending)
		{
			return (-compareResult);
		}
		else
		{
			return 0;
		}
	}
    
	public int SortColumn
	{
		set
		{
			ColumnToSort = value;
		}
		get
		{
			return ColumnToSort;
		}
	}

	public SortOrder Order
	{
		set
		{
			OrderOfSort = value;
		}
		get
		{
			return OrderOfSort;
		}
	}
}

}
