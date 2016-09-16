using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Runtime.InteropServices;
using Epi.Windows.MakeView.Excel;
using Epi.Data;
using Epi.Data.Services;

namespace Epi.Windows.MakeView.Dialogs
{
    public partial class ProjectFromExcelDialog : Epi.Windows.Dialogs.DialogBase
    {
        #region Private Data Members
        private bool projectLocationChanged;
        private DataTable projectTable;
        #endregion

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

        public XDocument Template
        {
            get { return _template; }
        }

        #endregion  //Public Properties

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
        protected XDocument _template;
        #endregion  //Protected Data Members

        public string SelectedExcelPath { get; set; }

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
            get { return txtNewProjectLocation.Text; }
        }
        public DbDriverInfo DriverInfo
        {
            get { return collectedDataDBInfo; }
        }
        public String DataDBInfo
        {
            get { return cbxCollectedDataDriver.SelectedValue.ToString(); }
        }
        public ProjectFromExcelDialog()
        {
            InitializeComponent();
        }
        public ProjectFromExcelDialog(string selectedExcel)
        {
            InitializeComponent();

            SelectedExcelPath = selectedExcel;

            project = new Project();
            metaDbInfo = new DbDriverInfo();
            collectedDataDBInfo = new DbDriverInfo();

            Epi.Template template = new Epi.Template();
            projectTable = template.GetProjectTable("Projects");
        }
        /// <summary>
        /// Loads the Project Creation Dialog
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void ProjectFromExcelDialog_Load(object sender, EventArgs e)
        {
            if (!this.DesignMode)
            {
                txtNewProjectLocation.Text = Configuration.GetNewInstance().Directories.Project;
                WinUtil.FillMetadataDriversList(cbxCollectedDataDriver);

                Configuration config = Configuration.GetNewInstance();
                cbxCollectedDataDriver.SelectedValue = config.Settings.DefaultDataDriver;
                txtProjectName.Focus();
            }

            //foreach (ListViewItem item in projectTemplateListView.Items)
            //{
            //    string fullPath = item.SubItems[3].Text;

            //    if (fullPath.Contains(SelectedTemplatePath))
            //    {
            //        item.Selected = true;
            //        item.Focused = true;
            //        break;
            //    }
            //}

            //projectTemplateListView.ColumnClick += new ColumnClickEventHandler(projectTemplateListView_ColumnClick);
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

        private void Start()
        {
            Microsoft.Office.Interop.Excel.Application xlApp;
            Microsoft.Office.Interop.Excel.Workbook xlWorkbook;

            try
            {
                xlApp = new Microsoft.Office.Interop.Excel.Application();
                xlWorkbook = xlApp.Workbooks.Open(dExcelPath.Text);
            }
            catch
            {
                MessageBox.Show("The necessary Microsoft Excel runtime files were not found on this machine. Please download them from:\n\r\n\rhttps://www.microsoft.com/en-us/download/details.aspx?id=3508", "Missing Components", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, 0, "https://www.microsoft.com/en-us/download/details.aspx?id=3508", "");
                return;
            }

            try
            {
                BuildXml BuildXml = new BuildXml();
                XmlDocument RootXml = new XmlDocument();
                string currentPath = Environment.CurrentDirectory;
                RootXml.Load("./Excel/RootTemplate.xml");
                XDocument XRoot = BuildXml.ToXDocument(RootXml);
                XDocument NewXmlDoc = new XDocument(XRoot);

                List<Card> PageList = SetCardValues(xlWorkbook);

                string filename = Path.GetFileNameWithoutExtension(dExcelPath.Text);
                _template = BuildXml.BuildNewXml(PageList, NewXmlDoc, filename);
            }
            catch(Exception e)
            {
                MessageBox.Show("Invalid input detected");
                //dExcelPath.Text = string.Empty;
            }
            finally
            {
                // Close the Workbook.
                xlWorkbook.Close(false);

                // Relase COM Object by decrementing the reference count.
                Marshal.ReleaseComObject(xlWorkbook);

                // Close Excel application.
                xlApp.Quit();

                // Release COM object.
                Marshal.FinalReleaseComObject(xlApp);
            }
        }

        private static List<Card> SetCardValues(Microsoft.Office.Interop.Excel.Workbook xlWorkbook)
        {
            List<Card> cardList = new List<Card>();
            Microsoft.Office.Interop.Excel.Worksheet xlWorksheet = (Microsoft.Office.Interop.Excel.Worksheet)xlWorkbook.Sheets.get_Item(1);

            // Get the range of cells which has data.
            Microsoft.Office.Interop.Excel.Range xlRange = xlWorksheet.UsedRange;

            // Get an object array of all of the cells in the worksheet with their values.
            object[,] valueArray = (object[,])xlRange.get_Value(Microsoft.Office.Interop.Excel.XlRangeValueDataType.xlRangeValueDefault);
            for (int row = 2; row <= xlWorksheet.UsedRange.Rows.Count; ++row)
            {

                Card Page = new Card();
                if (valueArray[row, 1] != null && valueArray[row, 1].ToString() != "")
                {
                    Page.Question = valueArray[row, 1].ToString();
                }
                if (valueArray[row, 2] != null && valueArray[row, 2].ToString() != "")
                {
                    Page.Variable_Name = valueArray[row, 2].ToString();
                }
                if (valueArray[row, 3] != null && valueArray[row, 3].ToString() != "")
                {
                    Page.Question_Type = GetFieldType(valueArray[row, 3].ToString());
                }
                if (valueArray[row, 4] != null && valueArray[row, 4].ToString() != "")
                {
                    Page.Required = bool.Parse(valueArray[row, 4].ToString());
                }
                if (valueArray[row, 5] != null && valueArray[row, 5].ToString() != "")
                {
                    Page.List_Values = GetDropDownList(valueArray[row, 5].ToString(), xlWorkbook);
                }
                if (valueArray[row, 6] != null && valueArray[row, 6].ToString() != "")
                {
                    Page.If_Condition = valueArray[row, 6].ToString();
                }
                if (valueArray[row, 7] != null && valueArray[row, 7].ToString() != "")
                {
                    Page.Then_Question = valueArray[row, 7].ToString();
                }
                if (valueArray[row, 8] != null && valueArray[row, 8].ToString() != "")
                {
                    Page.Else_Question = valueArray[row, 8].ToString();
                }
                Page.PageName = "Page " + (row - 1).ToString();
                Page.PageId = row - 1;
                cardList.Add(Page);
            }
            return cardList;
        }

        private static int GetFieldType(string fieldType)
        {
            int type = 0;

            switch (fieldType)
            {
                case "Checkbox":
                    type = (int)FieldType.Types.Checkbox;
                    break;
                case "Text":
                    type = (int)FieldType.Types.Text;
                    break;
                case "Numeric":
                    type = (int)FieldType.Types.Numeric;
                    break;
                case "Yes/No":
                    type = (int)FieldType.Types.YesNo;
                    break;
                case "Options":
                    type = (int)FieldType.Types.Options;
                    break;
                case "Dropdown":
                    type = (int)FieldType.Types.Dropdown;
                    break;
                case "Date":
                    type = (int)FieldType.Types.Date;
                    break;
                default:
                    type = 0;
                    break;
            }

            return type;
        }

        private static List<string> GetDropDownList(string DropDownValue, Microsoft.Office.Interop.Excel.Workbook xlWorkbook)
        {
            List<string> ValueList = new List<string>();

            Microsoft.Office.Interop.Excel.Worksheet xlWorksheet = (Microsoft.Office.Interop.Excel.Worksheet)xlWorkbook.Sheets.get_Item(DropDownValue);
            Microsoft.Office.Interop.Excel.Range xlRange = xlWorksheet.UsedRange;
            object[,] valueArray = (object[,])xlRange.get_Value(Microsoft.Office.Interop.Excel.XlRangeValueDataType.xlRangeValueDefault);
            for (int row = 1; row <= xlWorksheet.UsedRange.Rows.Count; ++row)
            {
                if (valueArray[row, 1] != null && valueArray[row, 1].ToString() != "")
                {
                    ValueList.Add(valueArray[row, 1].ToString());
                }
            }
            return ValueList;
        }


        private MetadataSource GetSelectedMetadataSource()
        {
            return MetadataSource.SameDb;
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

            project.Location = Path.Combine(txtNewProjectLocation.Text, txtProjectName.Text);

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
            if (!string.IsNullOrEmpty(txtProjectName.Text) && !string.IsNullOrEmpty(txtNewProjectLocation.Text))
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
            string path = txtNewProjectLocation.Text.Trim();
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
                        txtNewProjectLocation.Text = string.Empty;
                        txtNewProjectLocation.Focus();
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
            string fullPath = Path.Combine(txtNewProjectLocation.Text, txtProjectName.Text + Epi.FileExtensions.EPI_PROJ);
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
            if (!string.IsNullOrEmpty(txtProjectName.Text) && !string.IsNullOrEmpty(txtNewProjectLocation.Text))
            {
                {
                    Epi.Data.IDbDriverFactory factory = GetSelectedCollectedDataDriver();
                    if(factory != null)
                    { 
                        collectedDataDBInfo.DBCnnStringBuilder = GetSelectedCollectedDataDriver().RequestDefaultConnection(txtProjectName.Text.Trim(), txtProjectName.Text.Trim());
                        txtCollectedData.Text = collectedDataDBInfo.DBCnnStringBuilder.ConnectionString;
                        collectedDataConnectionString = txtCollectedData.Text;
                    }
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
            if (!string.IsNullOrEmpty(txtProjectName.Text) && !string.IsNullOrEmpty(txtNewProjectLocation.Text))
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

            project.Location = Path.Combine(txtNewProjectLocation.Text, projectNameCandidate);

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
            if (txtNewProjectLocation.Text.Trim() == string.Empty)
            {
                if (ErrorMessages.Count == 0) txtNewProjectLocation.Focus();
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

        /// <summary>
        /// Saves project inform to the configuration file
        /// </summary>
        private void OnProjectLocationChanged()
        {
            bool isValid = IsValidFile();
            if (isValid)
            {
                Configuration config = Configuration.GetNewInstance();
                config.Directories.Project = txtNewProjectLocation.Text.Trim();
                Configuration.Save(config);
                projectLocationChanged = false;
            }
            EnableDisableOkButton();
        }

        private void txtProjectName_TextChanged(object sender, EventArgs e)
        {
            //string projectNameCandidate = txtProjectName.Text;
            //string projectNameFromTemplate = txtProjectName.Text;

            //this.txtProjectName.TextChanged -= new System.EventHandler(this.txtProjectName_TextChanged);
            //this.txtProjectName.Leave -= new System.EventHandler(this.txtProjectName_Leave);

            //if (string.IsNullOrEmpty(projectNameFromTemplate) == false)
            //{
            //    int i = 2;

            //    while (IsValid_NEW_ProjectName(projectNameCandidate) == false)
            //    {
            //        projectNameCandidate = string.Format("{0}{1}", projectNameFromTemplate, i++);
            //    }
            //}

            //this.txtProjectName.TextChanged += new System.EventHandler(this.txtProjectName_TextChanged);
            //this.txtProjectName.Leave += new System.EventHandler(this.txtProjectName_Leave);

            //if (txtProjectName.Text != projectNameCandidate)
            //{
            //    txtProjectName.Text = projectNameCandidate;
            //}

            //txtProjectName_TextChanged(new Object(), new EventArgs());
            //txtProjectName_Leave(new Object(), new EventArgs());

            EnableDisableOkButton();
            PrepopulateCollectedDataLocation();
        }

        /// <summary>
        /// Handles the Leave event of the project name
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtProjectName_Leave(object sender, EventArgs e)
        {
            bool valid = ValidateProjectName();
            if (!string.IsNullOrEmpty(txtNewProjectLocation.Text) && valid == true)
            {
                bool isValidFile = IsValidFile();
                if (isValidFile)
                {
                    PrepopulateDatabaseLocations();
                }
            }
            EnableDisableOkButton();
        }

        protected virtual void EnableDisableOkButton()
        {
            // Check state of Project Name and View Name text boxes. If both have values, enable the OK button
            if (txtProjectName.Text.Length > 0 && txtNewProjectLocation.Text.Length > 0 && dExcelPath.Text.Length > 0)
            {
                btnOk.Enabled = true;
            }
            else
            {
                btnOk.Enabled = false;
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
        /// Handles the Change event of the project location's text
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtProjectLocation_TextChanged(object sender, EventArgs e)
        {
            PrepopulateDatabaseLocations();
            projectLocationChanged = true;
            EnableDisableOkButton();
        }

        #region Button_Clicks
        private void btnBrowseInput_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Excel files (*.xls, *.xlsx)|*.xls;*.xlsx";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                dExcelPath.Text = dialog.FileName;
                if(txtProjectName.Text == "")
                {
                    txtProjectName.Text = Path.GetFileNameWithoutExtension(dExcelPath.Text);
                }
            }
        }
        private void btnBrowseProjectLocation_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = txtNewProjectLocation.Text;
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtNewProjectLocation.Text = dialog.SelectedPath;
                OnProjectLocationChanged();
                PrepopulateDatabaseLocations();
            }
        }
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
                throw new Exception("Please enter project name.");
            }
        }
        private void btnOk_Click(object sender, EventArgs e)
        {
            Start();
        }
        #endregion Button_Clicks

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

        private void dExcelPath_TextChanged(object sender, EventArgs e)
        {
            EnableDisableOkButton();
        }
    }
}
