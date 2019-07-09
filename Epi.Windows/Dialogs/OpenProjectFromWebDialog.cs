#region Namespaces

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Epi.Data;
using Epi.DataSets;
using Epi.Data.Services;
using Epi.Windows;
using System.Text.RegularExpressions;
using System.ServiceModel;
using System.ServiceModel.Security;
using Epi.Web.Common.Exception;
using Epi.Web.Common.DTO;

#endregion

namespace Epi.Windows.Dialogs
{
    /// <summary>
    /// Dialog for project creation
    /// </summary>
    public partial class OpenProjectFromWebDialog : Epi.Windows.Dialogs.DialogBase
    {
        #region Private Data Members

        private bool projectLocationChanged;
        private DataTable projectTable;
        private ListViewColumnSorter lvwColumnSorter;
        private String OrganizationId { get; set; }
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

        private delegate void PublishDelegate(Epi.Web.Common.Message.PublishResponse Result);
        private BackgroundWorker SurveyInfoWorker;
        private BackgroundWorker UpdateWorker;

        private List<SurveyManagerServiceV4.SurveyInfoDTO> SurveyInfoList;

        private static object syncLock = new object(); 
        public string SurveyId { get; set; }
        private string SurveyName = "";
        private string DepartmentName = "";
        private string SurveyNumber = "";
        private string OrganizationName = "";
        private string OrganizationKey = "";
        private DateTime StartDate;
        private DateTime CloseDate;
        private bool IsSingleResponse;
        private bool IsDraftMode;
        private string IntroductionText;
        private string ExitText;
        private string TemplateXML;
        private string UserPublishKey;
        private int SurveyType;
        bool ignoreDataFormatIndexChange = false;
        private string projectName;
        private string projectLocation;
        private string dataDBInfo;

        ColumnHeader surveyIdHeader = new ColumnHeader();
        ColumnHeader surveyNameHeader = new ColumnHeader();
        ColumnHeader isDraftModeHeader = new ColumnHeader();
        ColumnHeader startDate = new ColumnHeader();
        ColumnHeader closeDate = new ColumnHeader();

        public String ProjectConnectionString
        {
            get { return collectedDataConnectionString; }
        }

        public String ProjectName
        {
            get { return txtProjectName.Text; }
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
        public string Template
        {
            get { return TemplateXML; }
        }

        #region Constructors

        public OpenProjectFromWebDialog(string orgIdGiven)
        {
            OrganizationId = orgIdGiven;

            InitializeComponent();
            project = new Project();
            metaDbInfo = new DbDriverInfo();
            collectedDataDBInfo = new DbDriverInfo();

            SurveyList.Dock = DockStyle.None;
            SurveyList.View = System.Windows.Forms.View.Details;
            SurveyList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            SurveyList.GridLines = false;
            SurveyList.Scrollable = true;
            SurveyList.FullRowSelect = true;
            SurveyList.AllowColumnReorder = true;
            SurveyList.HideSelection = false;
            SurveyList.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

            isDraftModeHeader.Text = "Mode";
            isDraftModeHeader.Width = 80;

            startDate.Text = "Start Date";
            startDate.Width = 80;

            closeDate.Text = "Close Date";
            closeDate.Width = 80;

            surveyIdHeader.Text = "Id";
            surveyIdHeader.Width = 240;

            surveyNameHeader.Text = "Name";
            surveyNameHeader.Width = SurveyList.Width - 
                isDraftModeHeader.Width -
                startDate.Width -
                closeDate.Width -
                surveyIdHeader.Width -
                3 - SystemInformation.VerticalScrollBarWidth;

            SurveyList.Columns.AddRange(
                new ColumnHeader[]
                {
                    surveyIdHeader,
                    surveyNameHeader,
                    isDraftModeHeader,
                    startDate,
                    closeDate
                }
            );

            lvwColumnSorter = new ListViewColumnSorter();
            SurveyList.ListViewItemSorter = lvwColumnSorter;
            
            List<string> oIds = GetRecentOrganizations();

            if(oIds.Count > 0)
            {
                PopulateOrganizations(oIds);
                //comboBoxOrgID.Text = oIds.First<string>();
            }
            
            this.comboBoxOrgID.TextChanged += ComboBoxOrgID_TextChanged;
            SurveyList.Resize += SurveyList_Resize;
        }

        private void SurveyList_Resize(object sender, EventArgs e)
        {
            surveyNameHeader.Width = SurveyList.Width -
                isDraftModeHeader.Width -
                startDate.Width -
                closeDate.Width -
                surveyIdHeader.Width -
                3 - SystemInformation.VerticalScrollBarWidth
            ;
        }

        private void PopulateListView()
        {
            if(SurveyInfoList != null && SurveyInfoList.Count > 0)
            {
                string surveyId, surveyName, draftMode, startDate, closeDate;

                List<SurveyInfoDTO> dOs = new List<SurveyInfoDTO>();

                SurveyList.Items.Clear();

                foreach(Epi.SurveyManagerServiceV4.SurveyInfoDTO si in SurveyInfoList)
                {
                    surveyId = si.SurveyId == null ? "" : si.SurveyId;
                    surveyName = si.SurveyName == null ? "" : si.SurveyName;
                    draftMode = si.IsDraftMode == null ? "" : (IsDraftMode == true ? "Draft" : "Published");
                    startDate = si.StartDate == null ? "" : si.StartDate.ToShortDateString();
                    closeDate = si.ClosingDate == null ? "" : si.ClosingDate.ToShortDateString();

                    if (string.IsNullOrEmpty(surveyId) == false)
                    {
                        ListViewItem item = new ListViewItem(surveyId, 0);

                        string subItemText = surveyName;
                        ListViewItem.ListViewSubItem subItem = new ListViewItem.ListViewSubItem();
                        subItem.Text = subItemText;
                        subItem.Tag = surveyId;

                        item.SubItems.Add(subItem);
                        item.SubItems.Add(draftMode);
                        item.SubItems.Add(startDate);
                        item.SubItems.Add(closeDate);
                        item.Tag = surveyId;
                        SurveyList.Items.Add(item);
                    }
                }
            }
            else
            {
                SurveyList.Visible = false;
            }
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        public OpenProjectFromWebDialog(MainForm mainForm)
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
            //if (this.projectTemplateListView.SelectedItems.Count == 0)
            //{
            //    Epi.Windows.MsgBox.ShowInformation("Please select a project template.");
            //    return;
            //}

            GetCollectedDataConnectionString();

            metaDbInfo.DBCnnStringBuilder = collectedDataDBInfo.DBCnnStringBuilder;
            metaDbInfo.DBName = collectedDataDBInfo.DBName;
            if (collectedDataDBInfo.DBCnnStringBuilder != null)
            {
                metadataConnectionString = collectedDataDBInfo.DBCnnStringBuilder.ConnectionString;
            }
            else
            {

            }
        }

        /// <summary>
        /// Handles the Change event of the project location's text
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        //private void txtProjectLocation_TextChanged(object sender, EventArgs e)
        //{
        //    //PrepopulateDatabaseLocations();
        //    projectLocationChanged = true;
        //}

        /// <summary>
        /// Handles the Click event of the Browse button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>

        /// <summary>
        /// Handles the Leave event of the Project location textbox
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        //private void txtProjectLocation_Leave(object sender, EventArgs e)
        //{

        //}

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

            //foreach (ListViewItem item in SurveyList.Items)
            //{
            //    string fullPath = item.SubItems[3].Text;

            //    if (fullPath.Contains(SurveyId))
            //    {
            //        item.Selected = true;
            //        item.Focused = true;
            //        break;
            //    }
            //}

            SurveyList.ColumnClick += new ColumnClickEventHandler(projectTemplateListView_ColumnClick);
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
            if (this.ValidateInput() == true)
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

        private bool IsValidFile()
        {
            bool exists = false;
            if (CheckIfProjectDirectoryExists())
            {
                exists = true;
            }
            return exists;
        }

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


        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Enables or disables the OK button depending on the state of other controls on the dialog
        /// </summary>
        protected virtual void EnableDisableOkButton()
        {
            //Check state of Project Name and View Name text boxes. If both have values, enable the OK button
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
            //bool isValid = IsValidFile();
            //if (isValid)
            //{
            //    Configuration config = Configuration.GetNewInstance();
            //    config.Directories.Project = txtProjectLocation.Text.Trim();
            //    Configuration.Save(config);
            //    projectLocationChanged = false;
            //}

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
                //collectedDataCnnDialog = GetSelectedCollectedDataDriver().GetConnectionStringGuiForNewDb();
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
            project.Name = "dpbrown";

            project.Location = "dpbrown";// Path.Combine(txtProjectLocation.Text, txtProjectName.Text);

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
            project.CollectedDataDriver = "cbxCollectedDataDriver.SelectedValue.ToString()";
            project.CollectedDataConnectionString = this.collectedDataDBInfo.DBCnnStringBuilder.ToString();
            project.CollectedData.Initialize(collectedDataDBInfo, "cbxCollectedDataDriver.SelectedValue.ToString()", true);

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

            this.SurveyList.Sort();
        }

        #endregion  Private Methods

        private void surveyListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((System.Windows.Forms.ListView)sender).SelectedItems.Count > 0)
            {
                string id = ((System.Windows.Forms.ListView)sender).SelectedItems[0].Tag.ToString();
                DoGetSurveyInfo(OrganizationKey, id);
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

        private void ComboBoxOrgID_TextChanged(object sender, System.EventArgs e)
        {
            HandleNewOrganization();
        }

        private void HandleNewOrganization()
        {
            // a4b6a687-610d-442a-a80c-d1c781087181

            bool validID = Regex.IsMatch(comboBoxOrgID.Text, "(^([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})$)");

            if (validID)
            {
                OrganizationKey = comboBoxOrgID.Text;

                SurveyManagerServiceV4.ManagerServiceV4Client client = Epi.Core.ServiceClient.ServiceClient.GetClientV4();
                SurveyManagerServiceV4.OrganizationRequest Request = new SurveyManagerServiceV4.OrganizationRequest();
                SurveyManagerServiceV4.OrganizationDTO orgDTO = new SurveyManagerServiceV4.OrganizationDTO();
                orgDTO.OrganizationKey = OrganizationKey;
                orgDTO.Organization = OrganizationKey;
                Request.Organization = orgDTO;

                SurveyManagerServiceV4.SurveyInfoRequest surveyInfoRequest = new SurveyManagerServiceV4.SurveyInfoRequest();
                SurveyManagerServiceV4.SurveyInfoDTO surveyInfoDTO = new SurveyManagerServiceV4.SurveyInfoDTO();


                SurveyManagerServiceV4.SurveyInfoResponse surInfoResponse = client.GetAllSurveysByOrgKey(OrganizationKey);
                SurveyInfoList = surInfoResponse.SurveyInfoList.ToList<SurveyManagerServiceV4.SurveyInfoDTO>();

                if (SurveyInfoList.Count > 0)
                {
                    SaveRecentOrganization(OrganizationKey);
                }

                PopulateListView();
            }
            else
            {
                SurveyList.Items.Clear();
            }
        }

        private void SaveRecentOrganization(string organizationId)
        {
            try
            {
                Configuration config = Configuration.GetNewInstance();

                if(config.RecentOrganization != null)
                {
                    DataTable orgsTable = config.RecentOrganization;
                    string filter = @"[Organization_Id] = '" + organizationId + @"'";
                    DataRow[] orgsRows = orgsTable.Select(filter);
                    if(orgsRows != null && orgsRows.Length == 1)
                    {
                        int index = orgsTable.Rows.IndexOf(orgsRows[0]);
                        orgsTable.AsDataView().Delete(index);
                        orgsTable.AcceptChanges();
                    }

                    DataRow newRecent = orgsTable.NewRow();
                    newRecent[config.RecentOrganization.Columns["Organization_Id"]] = organizationId;
                    newRecent[config.RecentOrganization.Columns["LastAccessed"]] = DateTime.Now;
                    orgsTable.Rows.Add(newRecent);
                    orgsTable.AcceptChanges();

                    Configuration.Save(config);

                }
            }
            catch { }
        }

        private List<string> GetRecentOrganizations()
        {
            List<string> orgs = new List<string>();

            try
            {
                Configuration config = Configuration.GetNewInstance();

                DataView orgsView = config.RecentOrganization.DefaultView;
                orgsView.Sort = "LastAccessed desc";
                DataTable sorted = orgsView.ToTable();

                foreach (System.Data.DataRow row in sorted.Rows)
                {
                    if(row["Organization_Id"] != null && row["Organization_Id"] is string)
                    {
                        orgs.Add((string)row["Organization_Id"]);
                    }    
                }
            }
            catch { }

            return orgs;
        }

        private void PopulateOrganizations(List<string> organizations)
        {
            try
            {
                ignoreDataFormatIndexChange = true;
                if (comboBoxOrgID.Items.Count == 0)
                {
                    comboBoxOrgID.Items.Clear();
                    comboBoxOrgID.Items.Add(string.Empty);

                    foreach (string orgId in organizations)
                    {
                        comboBoxOrgID.Items.Add(orgId);
                    }
                }
                comboBoxOrgID.SelectedIndex = 0;
            }
            finally
            {
                ignoreDataFormatIndexChange = false;
            }
        }

        private void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            //stopwatch.Stop();

            if (e.Result is Exception)
            {
            //    this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), SharedStrings.IMPORT_DATA_FAILED + " " + SharedStrings.IMPORT_DATA_TIME_ELAPSED + ": " + stopwatch.Elapsed.ToString());
            //    this.BeginInvoke(new SetStatusDelegate(SetStatusMessage), SharedStrings.IMPORT_DATA_FAILED + " " + SharedStrings.IMPORT_DATA_TIME_ELAPSED + ": " + stopwatch.Elapsed.ToString());
            }
            else
            {
            //    this.BeginInvoke(new SetStatusDelegate(AddStatusMessage), SharedStrings.IMPORT_DATA_COMPLETE + " " + SharedStrings.IMPORT_DATA_TIME_ELAPSED + ": " + stopwatch.Elapsed.ToString());
            //    this.BeginInvoke(new SetStatusDelegate(SetStatusMessage), SharedStrings.IMPORT_DATA_COMPLETE + " " + SharedStrings.IMPORT_DATA_TIME_ELAPSED + ": " + stopwatch.Elapsed.ToString());
            }

            if (string.IsNullOrEmpty(SurveyName) == false)
            {
                txtProjectName.Text = SurveyName;
                txtProjectName_TextChanged(new Object(), new EventArgs());
                txtProjectName_Leave(new Object(), new EventArgs());
            }
        }

        private void DoGetSurveyInfo(string orgKey, string surveyId)
        {
            try
            {

                SurveyManagerServiceV4.SurveyInfoRequest request = new SurveyManagerServiceV4.SurveyInfoRequest();
                request.Criteria = new SurveyManagerServiceV4.SurveyInfoCriteria();
                request.Criteria.OrganizationKey = new Guid(orgKey);
                request.Criteria.ReturnSizeInfoOnly = false;
                request.Criteria.SurveyType = -1;
                request.Criteria.PageNumber = -1;
                request.Criteria.PageSize = -1;

                request.Criteria.SurveyIdList = new string[] { surveyId };

                lock (syncLock)
                {
                    SurveyManagerServiceV4.SurveyInfoResponse result = new SurveyManagerServiceV4.SurveyInfoResponse();
                    this.Cursor = Cursors.WaitCursor;
                    SurveyInfoWorker = new BackgroundWorker();
                    SurveyInfoWorker.WorkerSupportsCancellation = true;
                    SurveyInfoWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(SurveyInfoworker_DoWork);

                    SurveyInfoWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(worker_WorkerCompleted);
                    object[] args = new object[2];
                    args[0] = request;
                    args[1] = result;
                    SurveyInfoWorker.RunWorkerAsync(args);
                }

            }
            catch (FaultException<CustomFaultException> cfe)
            {
                //  SurveyInfoResponseTextBox.AppendText("FaultException<CustomFaultException>:\n");
                /// SurveyInfoResponseTextBox.AppendText(cfe.ToString());
            }
            catch (FaultException fe)
            {
                // SurveyInfoResponseTextBox.AppendText("FaultException:\n");
                // SurveyInfoResponseTextBox.AppendText(fe.ToString());
            }
            catch (CommunicationException ce)
            {
                //  SurveyInfoResponseTextBox.AppendText("CommunicationException:\n");
                // SurveyInfoResponseTextBox.AppendText(ce.ToString());
            }
            catch (TimeoutException te)
            {
                // SurveyInfoResponseTextBox.AppendText("TimeoutException:\n");
                // SurveyInfoResponseTextBox.AppendText(te.ToString());
            }
            catch (Exception ex)
            {
                //  SurveyInfoResponseTextBox.AppendText("Exception:\n");
                //  SurveyInfoResponseTextBox.AppendText(ex.ToString());
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void SurveyInfoworker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                Configuration config = Configuration.GetNewInstance();
                var ServiceVersion = config.Settings.WebServiceEndpointAddress.ToLowerInvariant();

                if (!string.IsNullOrEmpty(ServiceVersion) && (ServiceVersion.Contains(Epi.Constants.surveyManagerservicev4)))
                {
                    SurveyManagerServiceV4.ManagerServiceV4Client client = Epi.Core.ServiceClient.ServiceClient.GetClientV4();

                    SurveyManagerServiceV4.SurveyInfoRequest request = (SurveyManagerServiceV4.SurveyInfoRequest)((object[])e.Argument)[0];
                    SurveyManagerServiceV4.SurveyInfoResponse result = (SurveyManagerServiceV4.SurveyInfoResponse)((object[])e.Argument)[1];

                    SurveyManagerServiceV4.SurveyInfoResponse response = client.GetSurveyInfo(request);

                    if (response.SurveyInfoList.Length > 0)
                    {
                        if (response != null && response.SurveyInfoList.Length > 0)
                        {
                            SurveyName = response.SurveyInfoList[0].SurveyName;
                            DepartmentName = response.SurveyInfoList[0].DepartmentName;
                            SurveyNumber = response.SurveyInfoList[0].SurveyNumber;
                            OrganizationName = response.SurveyInfoList[0].OrganizationName;
                            StartDate = response.SurveyInfoList[0].StartDate;
                            CloseDate = response.SurveyInfoList[0].ClosingDate;
                            IsDraftMode = response.SurveyInfoList[0].IsDraftMode;
                            IntroductionText = response.SurveyInfoList[0].IntroductionText;
                            ExitText = response.SurveyInfoList[0].ExitText;
                            TemplateXML = response.SurveyInfoList[0].XML;
                            SurveyType = response.SurveyInfoList[0].SurveyType;
                            UserPublishKey = response.SurveyInfoList[0].UserPublishKey.ToString();
                            SurveyId = response.SurveyInfoList[0].SurveyId;
                        }
                    }
                }
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                // this.BeginInvoke(new FinishWithCustomFaultExceptionDelegate(FinishWithCustomFaultException), cfe);

            }
            catch (FaultException fe)
            {
                //this.BeginInvoke(new FinishWithFaultExceptionDelegate(FinishWithFaultException), fe);

            }
            catch (SecurityNegotiationException sne)
            {
                //this.BeginInvoke(new FinishWithSecurityNegotiationExceptionDelegate(FinishWithSecurityNegotiationException), sne);

            }
            catch (CommunicationException ce)
            {
                //this.BeginInvoke(new FinishWithCommunicationExceptionDelegate(FinishWithCommunicationException), ce);

            }
            catch (TimeoutException te)
            {
                // this.BeginInvoke(new FinishWithTimeoutExceptionDelegate(FinishWithTimeoutException), te);

            }
            catch (Exception ex)
            {
                //this.BeginInvoke(new FinishWithExceptionDelegate(FinishWithException), ex);

            }
        }

        private void requestWorker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Configuration config = Configuration.GetNewInstance();

            if (e.Result != null && (e.Result is SurveyManagerService.SurveyAnswerRequest || e.Result is SurveyManagerServiceV2.SurveyAnswerRequest || e.Result is SurveyManagerServiceV3.SurveyAnswerRequest))
            {
                var ServiceVersion = config.Settings.WebServiceEndpointAddress.ToLowerInvariant();
                if (!string.IsNullOrEmpty(ServiceVersion) && (ServiceVersion.Contains(Epi.Constants.surveyManagerservice)))
                {
                    SurveyManagerService.SurveyAnswerRequest Request = (SurveyManagerService.SurveyAnswerRequest)e.Result;

                    //DoImport(Request);
                    // AddStatusMessage(SharedStrings.IMPORT_DATA_COMPLETE);
                }
                if (!string.IsNullOrEmpty(ServiceVersion) && (ServiceVersion.Contains(Epi.Constants.surveyManagerservicev2)))
                {
                    SurveyManagerServiceV2.SurveyAnswerRequest Request = (SurveyManagerServiceV2.SurveyAnswerRequest)e.Result;

                    //DoImportV2(Request);
                    //AddStatusMessage(SharedStrings.IMPORT_DATA_COMPLETE);
                }
                if (!string.IsNullOrEmpty(ServiceVersion) && (ServiceVersion.Contains(Epi.Constants.surveyManagerservicev3)))
                {
                    SurveyManagerServiceV3.SurveyAnswerRequest Request = (SurveyManagerServiceV3.SurveyAnswerRequest)e.Result;

                    //DoImportV3(Request);
                    //AddStatusMessage(SharedStrings.IMPORT_DATA_COMPLETE);
                }
            }
            else
            {
                //AddErrorStatusMessage(SharedStrings.IMPORT_ERROR_COMM_FAILED);
                //StopImport();
            }
        }
    }
}
