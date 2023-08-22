using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using Epi;
using Epi.Data;
using Epi.Windows.Dialogs;
using System.IO;

namespace Epi.Windows.Dialogs
{
   
	/// <summary>
	/// Dialog for Read command
	/// </summary>
    public partial class BaseReadDialog : DialogBase
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
        private string selectedDataProvider;
        private string mruSelectedDatabaseName;
        private Project selectedProject;
		private object selectedDataSource;
        private bool ignoreDataFormatIndexChange = false;
        private string savedConnectionStringDescription = string.Empty;
        private string sqlQuery = string.Empty;
        private Configuration config;
        private object module;
        #endregion Private Attributes

        #region Constructor

		public BaseReadDialog()
		{
			InitializeComponent();
            Construct();
        }

        public BaseReadDialog(object Module)
        {
            module = Module;
            InitializeComponent();
            Construct();
        }

        public BaseReadDialog(object Module, IDbDriver database)
        {
            module = Module;
            InitializeComponent();
            Construct();
            selectedDataSource = database;
        }

        public BaseReadDialog(object Module, Project project)
        {
            module = Module;
            InitializeComponent();
            Construct();
            selectedDataSource = project;
            selectedProject = project;
        }

        #endregion Constructor

		#region Private Methods
        private void Construct()
        {
            config = Configuration.GetNewInstance();
            if (!this.DesignMode)           // designer throws an error
            {
                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            }
        }

        private void btnOK_Click(object sender, System.EventArgs e)
        {
            OnOK();
        }

        private void LoadForm()
        {
            PopulateDataSourcePlugIns();
            PopulateRecentDataSources();

            try
            {
                //Project project = null;

                //string filePath = System.IO.Directory.GetCurrentDirectory().ToString() + "\\Projects\\Sample.prj";
                //if (System.IO.File.Exists(filePath))
                //{
                //    Project prj = new Project(filePath);
                //    try
                //    {
                //        selectedDataSource = prj;
                //        this.selectedProject = prj;
                //        this.selectedDataSource = prj;
                //    }
                //    catch (Exception ex)
                //    {
                //        MessageBox.Show("Could not load project: \n\n" + ex.Message);
                //        return;
                //    }
                //}
            }
            catch (CurrentProjectInvalidException ex)
            {
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
                this.txtDataSource.Text = db.DataSource;
                var SelectedDataSource = db.ConnectionString.Split('@');

                if (SelectedDataSource[0].Contains("Epi Info Web Survey") || SelectedDataSource[0].Contains("Epi Info Cloud Data Capture"))
                {
                    this.txtDataSource.Text = "Epi Info Web & Cloud Services";
                }

                List<string> tableNames = db.GetTableNames();
                if (cmbDataSourcePlugIns.SelectedItem.ToString().Contains("JSON"))
                {
                    System.IO.DirectoryInfo d = new System.IO.DirectoryInfo(db.DataSource);
                    FileInfo[] jsonfiles = d.GetFiles("*.json");
                    tableNames.Clear();
                    foreach (FileInfo file in jsonfiles)
                    {
                        tableNames.Add(file.Name);
                    }
                }

                foreach (string tableName in tableNames)
                {
                    ListViewItem newItem = new ListViewItem(new string[] { tableName, tableName });
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
                        ListViewGroup viewGroup = new ListViewGroup("Forms", "Epi Info Forms");
                        this.lvDataSourceObjects.Groups.Add(viewGroup);

                        foreach (string s in project.GetViewNames())
                        {
                            ListViewItem newItem = new ListViewItem(new string[] { s, "View" }, viewGroup);
                            this.lvDataSourceObjects.Items.Add(newItem);
                        }
                    }
                    if (chkTables.Checked)
                    {
                        ListViewGroup tablesGroup = new ListViewGroup("Tables", "Tables");
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
                openFileDialog.InitialDirectory = config.Directories.Project;
                openFileDialog.FilterIndex = 1;
                openFileDialog.Multiselect = false;

                DialogResult result = openFileDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName.Trim();
                    if (System.IO.File.Exists(filePath))
                    {
                        try
                        {
                            selectedDataSource = new Project(filePath);
                            formNeedsRefresh = true;
                        }
                        catch (System.Security.Cryptography.CryptographicException ex)
                        {
                            MsgBox.ShowError(string.Format(SharedStrings.ERROR_CRYPTO_KEYS, ex.Message));
                            return;
                        }
                        catch (Exception ex)
                        {
                            MsgBox.ShowError(SharedStrings.CANNOT_OPEN_PROJECT_FILE + "\n\nError details: " + ex.Message);
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
                  //  dialog.UseManagerService 
                    if (result == DialogResult.OK && dialog.DbConnectionStringBuilder!=null)
                    {
                        this.savedConnectionStringDescription = dialog.ConnectionStringDescription;
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
                        if (selectedPlugIn.Text == "Epi Info Web & Cloud Services")
                        {

                        }

                        else
                        {
                            this.selectedDataSource = null;
                        }
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
			if (this.lvDataSourceObjects.SelectedIndices.Count == 0 && string.IsNullOrEmpty(SQLQuery))
			{
                ErrorMessages.Add(SharedStrings.SPECIFY_TABLE_OR_VIEW);
			}
			return (ErrorMessages.Count == 0);
		}
		
		/// <summary>
		/// Generate the Read command
		/// </summary>
        protected void GenerateCommand()
        {
            if (!(cmbDataSourcePlugIns.SelectedItem.ToString().Equals("Epi Info 7 Project")))
            {
                selectedProject = null;
            }
        }

        public object SelectedDataSource
        {
            get
            {
                return selectedDataSource;
            }
        }

        public string SQLQuery
        {
            get
            {
                return this.sqlQuery;
            }
            set
            {
                this.sqlQuery = value;
            }
        }

        public string SelectedDataMember
        {
            get
            {
                if (string.IsNullOrEmpty(SQLQuery) && lvDataSourceObjects.SelectedItems.Count > 0)
                {
                    return lvDataSourceObjects.SelectedItems[0].Text;
                }
                else
                {
                    return SQLQuery;
                }
            }
        }

        public bool IsFormSelected
        {
            get
            {
                ListViewGroup tables = lvDataSourceObjects.Groups["Tables"];
                ListViewGroup forms = lvDataSourceObjects.Groups["Forms"];

                if (lvDataSourceObjects.SelectedItems[0].Group.Equals(tables))
                {
                    return false;
                }
                else if (lvDataSourceObjects.SelectedItems[0].Group.Equals(forms))
                {
                    return true;
                }
                return false;
            }
        }

        public string SelectedDataProvider
        {
            get
            {
                return selectedDataProvider;
            }
        }

		/// <summary>
		/// Before executing the command, preprocesses information gathered from the dialog.
		/// If the current project has changed, updates Global.CurrentProject
		/// </summary>
		protected void PreProcess()
		{
			//base.PreProcess();

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

            // add to the config            
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
		}
		
		/// <summary>
		/// Checks if the input provided is sufficient and enables control buttons accordingly.
		/// </summary>
        public override void CheckForInputSufficiency()
		{
			bool inputValid = ValidateInput();
			btnOK.Enabled = inputValid;
            if (this.SelectedDataSource is IDbDriver)
            {
                btnAdvanced.Enabled = true;
            }
            else
            {
                btnAdvanced.Enabled = false;
            }

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
                OnOK();
            }
        }

        private void OnOK()
        {
            if (ValidateInput() == true)
            {
                GenerateCommand();
                PreProcess();
                this.DialogResult = DialogResult.OK;
                this.Hide();
            }
            else
            {
                this.DialogResult = DialogResult.None;
                ShowErrorMessages();
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

        #endregion Event Handlers

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

                    if (name.ToLowerInvariant().EndsWith(".prj"))
                    {
                        Project project = new Project(connectionString);

                        try
                        {
                            project.CollectedData.TestConnection();
                        }
                        catch (Exception ex)
                        {
                            Epi.Windows.MsgBox.ShowException(ex);
                            return;
                        }

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

                            try
                            {
                                db.TestConnection();
                            }
                            catch (Exception ex)
                            {
                                Epi.Windows.MsgBox.ShowException(ex);
                                return;
                            }

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
            catch (System.IO.DirectoryNotFoundException ex)
            {
                MsgBox.ShowException(ex);
                mruSelectedDatabaseName = string.Empty;
                cmbRecentSources.SelectedIndex = -1;
            }
            catch (System.IO.FileNotFoundException ex)
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

        private void btnAdvanced_Click(object sender, EventArgs e)
        {
            if (SelectedDataSource is IDbDriver)
            {
                AdvancedReadDialog advReadDialog = new AdvancedReadDialog(this.SelectedDataSource as IDbDriver);
                advReadDialog.SQLQuery = this.SQLQuery;
                DialogResult result = advReadDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    sqlQuery = advReadDialog.SQLQuery;
                    OnOK();
                }
                else
                {
                    this.DialogResult = System.Windows.Forms.DialogResult.None;
                }
            }
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            try
            {
                if (module != null)
                {
                    string key = module.ToString();

                    if (key.Contains("Text: Map"))
                    {
                        System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/maps/introduction.html");
                    }
                    else
                        if (key.Contains("Text: Dashboard"))
                        {
                            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/visual-dashboard/VisualDashboardIntro.html");
                        }
                        else
                            if (key.Contains("Text: Table-to-Form"))
                            {
                                System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/form-designer/how-to-make-form-from-table.html");
                            }

                            else
                            {
                                System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/support/userguide.html");
                            }
                }
            }
            catch
            {

            }
        }
    }
}
