using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Text;
using Epi;
using Epi.Data;
using Epi.Analysis;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;

namespace Epi.Windows.Analysis.Dialogs
{
   
	/// <summary>
	/// Dialog for Read command
	/// </summary>
    public partial class ReadDataSourceDialog : CommandDesignDialog
	{
        #region Private Nested Class
        private class ComboBoxItem
        {
            #region Implementation
            private string key;
            public string Key
            {
                get { return key; }
                set { key = value; }
            }

            private object value;
            public object Value
            {
                get { return this.value; }
                set { this.value = value; }
            }

            private string text;
            public string Text
            {
                get { return text; }
                set { text = value; }
            }

            public ComboBoxItem(string key, string text, object value)
            {
                this.key = key;
                this.value = value;
                this.text = text;
            }

            public override string ToString()
            {
                return text.ToString();
            }
            #endregion
        }
        #endregion Private Nested Class

        #region Private Attributes
        private Project selectedProject;
		private object selectedDataSource;
        private string selectedDataProvider;
        private string mruSelectedDatabaseName;
        private bool ignoreDataFormatIndexChange = false;
        private bool usingPassword;
        private string savedConnectionStringDescription = string.Empty;
        #endregion Private Attributes

        #region Constructor

        /// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		protected ReadDataSourceDialog()
		{
			InitializeComponent();
            Construct();
        }

        public ReadDataSourceDialog(bool isForMap)
            : base(null)
        {
            InitializeComponent();
            Construct();
        }
		
		/// <summary>
		/// Constructor for Read dialog
		/// </summary>
        public ReadDataSourceDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
		{
			InitializeComponent();
            Construct();
            this.EpiInterpreter = frm.EpiInterpreter;
        }
        #endregion Constructor

		#region Private Methods
        private void Construct()
        {
            if (!this.DesignMode)           // designer throws an error
            {
                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
            }
        }

        private void LoadForm()
        {
            PopulateDataSourcePlugIns();
            PopulateRecentDataSources();

            try
            {
                Project project = null;
                if (this.mainForm != null)
                {
                    if (this.mainForm.CurrentProject != null)
                    {
                        project = this.mainForm.CurrentProject;
                    }
                }
                if (this.EpiInterpreter != null)
                    project = this.EpiInterpreter.Context.CurrentProject;

                if (project != null)
                {
                    this.selectedProject = project;
                    this.selectedDataSource = project;
                }
                else
                {
                    string filePath = System.IO.Directory.GetCurrentDirectory().ToString() + "\\Projects\\Sample.prj";
                    if (System.IO.File.Exists(filePath))
                    {
                        Project prj = new Project(filePath);
                        try
                        {
                            selectedDataSource = prj;
                            this.selectedProject = prj;
                            this.selectedDataSource = prj;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Could not load project: \n\n" + ex.Message);
                            return;
                        }
                    }
                }
            }
            catch (CurrentProjectInvalidException ex)
            {
                if (this.mainForm != null)
                {
                    this.mainForm.CurrentProject = null;
                }

                Epi.Windows.MsgBox.ShowInformation(ex.Message);
            }

            RefreshForm();
        }

        private void RefreshForm()
        {
            lvDataSourceObjects.Groups.Clear();
            lvDataSourceObjects.Items.Clear();

            if (selectedDataSource is IDbDriver)
            {
                IDbDriver db = selectedDataSource as IDbDriver;

                switch (db.ToString())
                {
                    case "Epi.Data.Office.AccessDatabase":
                    case "Epi.Data.Office.Access2007Database":
                    case "Epi.Data.Office.ExcelWorkbook":
                    case "Epi.Data.Office.Excel2007Workbook":
                        this.txtDataSource.Text = db.DataSource;
                        break;
                    default:
                        this.txtDataSource.Text = db.ConnectionString;
                        break;
                }

                List<string> tableNames = db.GetTableNames();

			    foreach (string tableName in tableNames)
			    {
                    ListViewItem newItem = new ListViewItem(new string[] { tableName, tableName});
				    this.lvDataSourceObjects.Items.Add(newItem);
			    }

                gbxExplorer.Enabled = true;
            }
            else if (selectedDataSource is Project)
            {
                Project project = selectedDataSource as Project;
                txtDataSource.Text = (selectedDataSource == selectedProject) ? SharedStrings.CURRENT_PROJECT : project.FullName;

                try
                {
                    if (chkViews.Checked)
                    {
                        ListViewGroup viewGroup = new ListViewGroup("Epi Info Forms");
                        this.lvDataSourceObjects.Groups.Add(viewGroup);

                        foreach (string s in project.GetViewNames())
                        {
                            ListViewItem newItem = new ListViewItem(new string[] { s, "View" }, viewGroup);
                            this.lvDataSourceObjects.Items.Add(newItem);
                        }
                    }
                    if (chkTables.Checked)
                    {
                        ListViewGroup tablesGroup = new ListViewGroup("Tables");
                        this.lvDataSourceObjects.Groups.Add(tablesGroup);
                        foreach (string s in project.GetNonViewTableNames())
                        {
                            ListViewItem newItem = new ListViewItem(new string[] { s, "Table" }, tablesGroup);
                            this.lvDataSourceObjects.Items.Add(newItem);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Epi.Windows.MsgBox.ShowException(ex);
                    txtDataSource.Text = string.Empty;
                    this.lvDataSourceObjects.Items.Clear();
                    this.selectedDataSource = null;
                    return;
                }
                gbxExplorer.Enabled = true;
            }
            else
            {
                // Clear ...
                this.txtDataSource.Text = "(none)";
                this.lvDataSourceObjects.Items.Clear();// DataSource = null;

                gbxExplorer.Enabled = false;
            }

            this.CheckForInputSufficiency();
        }

		/// <summary>
		/// Attach the DataFormats combobox with supported data formats
		/// </summary>
		private void PopulateDataSourcePlugIns()
		{
            try
            {
                Configuration config = Configuration.GetNewInstance();
                ignoreDataFormatIndexChange = true;
                if (cmbDataSourcePlugIns.Items.Count == 0)
                {
                    cmbDataSourcePlugIns.Items.Clear();

                    cmbDataSourcePlugIns.Items.Add(new ComboBoxItem(null, "Epi Info 7 Project", null));

                    foreach (Epi.DataSets.Config.DataDriverRow row in config.DataDrivers)
                    { 
                        cmbDataSourcePlugIns.Items.Add(new ComboBoxItem(row.Type,row.DisplayName,null));
                    }
                }
                cmbDataSourcePlugIns.SelectedIndex = 0; 
            }
            finally
            {
                ignoreDataFormatIndexChange = false;
            }
		}

        /// <summary>
        /// Attach the Recent data sources combobox with the list of recent sources
        /// </summary>
        private void PopulateRecentDataSources()
        {
            try
            {
                Configuration config = Configuration.GetNewInstance();
                ignoreDataFormatIndexChange = true;
                if (cmbRecentSources.Items.Count == 0)
                {
                    cmbRecentSources.Items.Clear();
                    cmbRecentSources.Items.Add(string.Empty);

                    foreach (Epi.DataSets.Config.RecentDataSourceRow row in config.RecentDataSources)
                    {
                        cmbRecentSources.Items.Add(new ComboBoxItem(row.DataProvider, row.Name, row.ConnectionString));
                    }
                }
                cmbRecentSources.SelectedIndex = 0;
            }
            finally
            {
                ignoreDataFormatIndexChange = false;
            }
        }

        private void OpenSelectProjectDialog()
        {
            try
            {
                Project oldSelectedProject = this.selectedProject;
                Project newSelectedProject = base.SelectProject();

                if (newSelectedProject != null)
                {
                    this.selectedProject = newSelectedProject;

                    if (oldSelectedProject != newSelectedProject)
                    {
                        SetDataSourceToSelectedProject();
                    }
                    
                    this.RefreshForm();
                }
            }
            catch (Exception ex)
            {
                MsgBox.ShowException(ex);
            }
        }

        private void SetDataSourceToSelectedProject()
        {
            this.selectedDataSource = selectedProject;
            this.cmbDataSourcePlugIns.SelectedIndex = 0;
        }

        private void OpenSelectDataSourceDialog()
        {
            bool formNeedsRefresh = false;
            ComboBoxItem selectedPlugIn = cmbDataSourcePlugIns.SelectedItem as ComboBoxItem;

            if (selectedPlugIn == null)
            {
                throw new GeneralException("No data source plug-in is selected in combo box.");
            }

            if (selectedPlugIn.Key == null)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = SharedStrings.SELECT_DATA_SOURCE;
                openFileDialog.Filter = "Epi Info " + SharedStrings.PROJECT_FILE + " (*.prj)|*.prj";
                openFileDialog.InitialDirectory = System.IO.Directory.GetCurrentDirectory(); 
                openFileDialog.FilterIndex = 1;
                openFileDialog.Multiselect = false;

                DialogResult result = openFileDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    this.cmbRecentSources.SelectedIndex = -1;
                    string filePath = openFileDialog.FileName.Trim();
                    if (System.IO.File.Exists(filePath))
                    {
                        try
                        {
                            selectedDataSource = new Project(filePath);
                            formNeedsRefresh = true;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Could not load project: \n\n" + ex.Message);
                            return;
                        }
                    }
                }
            }
            else
            {
                IDbDriverFactory dbFactory = null;
                
                selectedDataProvider = selectedPlugIn.Key;

                dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(selectedPlugIn.Key);
                if (dbFactory.ArePrerequisitesMet())
                {

                    DbConnectionStringBuilder dbCnnStringBuilder = new DbConnectionStringBuilder();
                    IDbDriver db = dbFactory.CreateDatabaseObject(dbCnnStringBuilder);

                    IConnectionStringGui dialog = dbFactory.GetConnectionStringGuiForExistingDb();

                    if (!string.IsNullOrEmpty(savedConnectionStringDescription))
                    {
                        int splitIndex = savedConnectionStringDescription.IndexOf("::");
                        if (splitIndex > -1)
                        {
                            string serverName = savedConnectionStringDescription.Substring(0, splitIndex);
                            string databaseName = savedConnectionStringDescription.Substring(splitIndex + 2, savedConnectionStringDescription.Length - splitIndex - 2);
                            dialog.SetDatabaseName(databaseName);
                            dialog.SetServerName(serverName);
                        }
                    }

                    DialogResult result = ((Form)dialog).ShowDialog();
                    UsingPassword = dialog.UsesPassword;
                    if (result == DialogResult.OK)
                    {
                        this.savedConnectionStringDescription = dialog.ConnectionStringDescription;
                        this.cmbRecentSources.SelectedIndex = -1;
                        bool success = false;
                        db.ConnectionString = dialog.DbConnectionStringBuilder.ToString();

                        try
                        {
                            success = db.TestConnection();
                        }
                        catch
                        {
                            success = false;
                            MessageBox.Show("Could not connect to selected data source.");
                        }

                        if (success)
                        {
                            this.selectedDataSource = db;
                            formNeedsRefresh = true;
                        }
                        else
                        {
                            this.selectedDataSource = null;
                        }
                    }
                    else
                    {
                        this.selectedDataSource = null;
                    }
                }
                else
                {
                    MessageBox.Show(dbFactory.PrerequisiteMessage, "Prerequisites not found");
                }
            }
            if (formNeedsRefresh)
            {
                RefreshForm();
            }
        }

		#endregion Private Methods

		#region Protected Methods
		
		/// <summary>
		/// Validates input
		/// </summary>
		/// <returns>true if validation passes; else false</returns>
		protected override bool ValidateInput()
		{
			base.ValidateInput ();
			if (cmbDataSourcePlugIns.SelectedIndex == -1)
			{
				ErrorMessages.Add(SharedStrings.SPECIFY_DATAFORMAT);
			}
			if (string.IsNullOrEmpty(txtDataSource.Text))			
			{
				ErrorMessages.Add(SharedStrings.SPECIFY_DATASOURCE);
			}
			if (this.lvDataSourceObjects.SelectedIndices.Count == 0)
			{
                if (!(this.selectedDataSource is Epi.Data.Office.SharePointList))
                    ErrorMessages.Add(SharedStrings.SPECIFY_TABLE_OR_VIEW);
			}
			return (ErrorMessages.Count == 0);
		}
		
		/// <summary>
		/// Generate the Read command
		/// </summary>
		protected override void GenerateCommand()
		{
            // add to the config
            Configuration config = Configuration.GetNewInstance();
            string name = string.Empty;
            string connectionString = string.Empty;
            string dataProvider = string.Empty;

            if (SelectedDataSource is IDbDriver)
            {
                IDbDriver db = selectedDataSource as IDbDriver;
                name = db.DbName;
                connectionString = db.ConnectionString;
                dataProvider = SelectedDataProvider;
            }
            else if (SelectedDataSource is Project)
            {
                Project project = selectedDataSource as Project;
                name = project.FileName;
                connectionString = project.FilePath;
                dataProvider = project.CollectedDataDriver;
            }

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(connectionString))
            {
                Configuration.OnDataSourceAccessed(name, Configuration.Encrypt(connectionString), dataProvider);
            }

			StringBuilder sb = new StringBuilder();
			sb.Append(Epi.CommandNames.READ);
			sb.Append(StringLiterals.SPACE);

            if (this.cmbRecentSources.SelectedItem != null && !string.IsNullOrEmpty(this.cmbRecentSources.SelectedItem.ToString()))
            {
                //sb.Append(mruSelectedDatabaseName);
                //sb.Append(StringLiterals.COLON);
                sb.Append("{").Append("config:").Append(mruSelectedDatabaseName).Append("}");
                sb.Append(StringLiterals.COLON);
            }
            else if (UsingPassword)
            {
                sb.Append("{").Append("config:").Append(name).Append("}");
                sb.Append(StringLiterals.COLON);
            }
            else
            {
                string dsName = txtDataSource.Text;

                if (dsName.Equals(SharedStrings.CURRENT_PROJECT))
                {
                    dsName = selectedProject.FilePath;
                }
                if (!(cmbDataSourcePlugIns.SelectedItem.ToString().Equals("Epi Info 7 Project")))
                {
                    selectedProject = null;
                }
                if (!string.IsNullOrEmpty(dsName) && !dsName.Equals(SharedStrings.CURRENT_PROJECT))
                {
                    //sb.Append("{").Append(((IDbDriver)SelectedDataSource).ConnectionString).Append("}");                    
                    sb.Append("{").Append(dsName).Append("}");
                }
                if (!dsName.Equals(SharedStrings.CURRENT_PROJECT))
                {
                    sb.Append(StringLiterals.COLON);
                }
            }

			//Delete this line once complex read is implemented
            if (!(this.selectedDataSource is Epi.Data.Office.SharePointList))
            {
                string Identifier = lvDataSourceObjects.SelectedItems[0].Text;
                sb.Append(this.FieldNameNeedsBrackets(Identifier) ? Util.InsertInSquareBrackets(Identifier) : Identifier);
            }
            else
                sb.Append("listName");
			sb.Append(StringLiterals.SPACE);
            
			CommandText = sb.ToString();
		}

        public object SelectedDataSource
        {
            get
            {
                return selectedDataSource;
            }
        }

        public string SelectedDataMember
        {
            get
            {
                return lvDataSourceObjects.SelectedItems[0].Text;
            }
        }

        public string SelectedDataProvider
        {
            get
            {
                return selectedDataProvider;
            }
        }

        private bool UsingPassword
        {
            get
            {
                return this.usingPassword;
            }
            set
            {
                this.usingPassword = value;
            }
        }

		/// <summary>
		/// Before executing the command, preprocesses information gathered from the dialog.
		/// If the current project has changed, updates Global.CurrentProject
		/// </summary>
		protected override void PreProcess()
		{
			base.PreProcess();

            // dcs0 8/13/2008 the below doesn't seem necessary, the command processor should do all of this
            //IProjectHost host = Module.GetService(typeof(IProjectHost)) as IProjectHost;
            //if (host == null)
            //{
            //    throw new GeneralException("No project is hosted by service provider.");
            //}

            //if ((host.CurrentProject == null) || (!host.CurrentProject.Equals(this.selectedProject)))
            //{
            //    Configuration config = Configuration.GetNewInstance();
            //    host.CurrentProject = this.selectedProject;
            //    config.CurrentProjectFilePath = selectedProject.FullName;
            //    Configuration.Save(config);
            //}
            
            //string y = SelectedDataSource.CollectedDataConnectionString;
		}
		
		/// <summary>
		/// Checks if the input provided is sufficient and enables control buttons accordingly.
		/// </summary>
        public override void CheckForInputSufficiency()
		{
			bool inputValid = ValidateInput();
			btnOK.Enabled = inputValid;
			btnSaveOnly.Enabled = inputValid;
		}        

		#endregion Protected Methods			

        #region Event Handlers

        private void ReadDialog_Load(object sender, System.EventArgs e)
        {
            LoadForm();
        }

        private void btnFindProject_Click(object sender, System.EventArgs e)
        {
            OpenSelectProjectDialog();
        }

        private void cmbDataSourcePlugIns_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (ignoreDataFormatIndexChange) return;

            ComboBox CB = (ComboBox) sender;
            if (CB.SelectedIndex == 0)
            {
                chkViews.Enabled = true;
                chkTables.Enabled = true;
            }
            else
            {
                chkViews.Enabled = false;
                chkTables.Enabled = false;

                chkViews.Checked = true;
                chkTables.Checked = true;
            }
            this.selectedDataSource = null;

            // TODO: Review this code. Select known database driver from configuration or prj file
            this.RefreshForm();
        }

        private void cmbRecentSources_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                //this.RefreshForm();
                if (cmbRecentSources.SelectedItem != null && !string.IsNullOrEmpty(cmbRecentSources.SelectedItem.ToString()))
                {
                    string connectionString = Configuration.Decrypt(((ComboBoxItem)cmbRecentSources.SelectedItem).Value.ToString());
                    string name = ((ComboBoxItem)cmbRecentSources.SelectedItem).Text.ToString();
                    string provider = ((ComboBoxItem)cmbRecentSources.SelectedItem).Key.ToString();

                    mruSelectedDatabaseName = name;
                    selectedDataProvider = provider;

                    if (name.ToLower().EndsWith(".prj"))
                    {
                        Project project = new Project(connectionString);
                        this.selectedDataSource = project;
                        this.selectedProject = project;
                    }
                    else
                    {
                        IDbDriverFactory dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(provider);
                        if (dbFactory.ArePrerequisitesMet())
                        {
                            DbConnectionStringBuilder dbCnnStringBuilder = new DbConnectionStringBuilder();
                            IDbDriver db = dbFactory.CreateDatabaseObject(dbCnnStringBuilder);
                            db.ConnectionString = connectionString;
                            this.selectedDataSource = db;
                        }
                    }
                }
                else if (cmbRecentSources.SelectedItem != null && string.IsNullOrEmpty(cmbRecentSources.SelectedItem.ToString()))
                {
                    mruSelectedDatabaseName = string.Empty;
                }

                RefreshForm();
            }
            catch (DirectoryNotFoundException ex)
            {
                MsgBox.ShowException(ex);
                mruSelectedDatabaseName = string.Empty;
                cmbRecentSources.SelectedIndex = -1;
            }
            catch (FileNotFoundException ex)
            {
                MsgBox.ShowException(ex);
                mruSelectedDatabaseName = string.Empty;
                cmbRecentSources.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MsgBox.ShowException(ex);
                mruSelectedDatabaseName = string.Empty;
                cmbRecentSources.SelectedIndex = -1;
            }
        }

        private void btnClear_Click(object sender, System.EventArgs e)
        {
            lvDataSourceObjects.SelectedItems.Clear(); //.SelectedIndex = -1;
            SetDataSourceToSelectedProject();
        }

        private void txtCurrentProj_TextChanged(object sender, System.EventArgs e)
        {
            CheckForInputSufficiency();
        }

        private void txtDataSource_TextChanged(object sender, System.EventArgs e)
        {
            CheckForInputSufficiency();
        }

        private void btnFindDataSource_Click(object sender, System.EventArgs e)
        {
            OpenSelectDataSourceDialog();
        }

        private void lvDataSourceObjects_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            CheckForInputSufficiency();
        }

        private void lvDataSourceObjects_DataSourceChanged(object sender, System.EventArgs e)
        {
            if (lvDataSourceObjects.Items.Count > 0)
            {
                lvDataSourceObjects.SelectedIndices.Clear();
            }
        }

        private void lvDataSourceObjects_Resize(object sender, EventArgs e)
        {
            // leave space for scroll bar
            this.lvDataSourceObjects.Columns[0].Width = this.lvDataSourceObjects.Width - 25;
        }

        private void lvDataSourceObjects_DoubleClick(object sender, EventArgs e)
        {
            if (this.btnOK.Enabled)
            {
                base.OnOK();
            }
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!(chkViews.Checked) && !(chkTables.Checked))
            {
                chkViews.Checked = true;
            }
            RefreshForm();
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-read.html");
        }
        #endregion Event Handlers
        
    }
}
