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


namespace Epi.Windows.MakeView.Dialogs
{
   
	/// <summary>
	/// Dialog for Read command
	/// </summary>
    public partial class ReadDialog : DialogBase
	{
        /// <summary>
        /// Selected Table
        /// </summary>
        public string SelectedTable = null;

        /// <summary>
        /// New View Name 
        /// </summary>
        public string NewViewName = null;

        private bool ButtonOkClicked = false;

        /// <summary>
        /// Selected DataSource
        /// </summary>
        public IDbDriver SelectedDataSource
        {
            get
            {
                if (this.selectedDataSource == null)
                {
                    return null;
                }
                else
                {
                    if (this.selectedDataSource is Project)
                    {
                        Project P = (Project)this.selectedDataSource;
                        IDbDriver result = this.GetDbDriver(P.CollectedDataDriver);
                        result.ConnectionString = P.CollectedDataConnectionString;
                        return result;
                    }
                    else
                    {
                        return (IDbDriver)this.selectedDataSource;
                    }
                }
            }   
        }

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
        private Project sourceProject;
        private Project selectedProject;
		private object selectedDataSource;
        private System.Collections.Hashtable SourceProjectNames;
        private bool ignoreDataFormatIndexChange = false;
        //--Ei-148
        List<string> Sourcedatatables = new List<string>();
        List<string> SourceNonViewnames = new List<string>();
        private int count = 1; 
        //--
        #endregion Private Attributes

        #region Constructor


        /// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		protected ReadDialog()
		{
			InitializeComponent();
        }
		
		/// <summary>
		/// Constructor for Read dialog
		/// </summary>
        public ReadDialog(Epi.Windows.MakeView.Forms.MakeViewMainForm frm)
            : base(frm)
		{
			InitializeComponent();
            Construct();
            SourceProjectNames = new Hashtable();
           
            if (frm.projectExplorer != null)
            {
                if (frm.projectExplorer.currentPage != null)
                {
                    this.sourceProject = frm.projectExplorer.currentPage.view.Project;
                    this.selectedProject = this.sourceProject;
                    this.selectedDataSource = this.selectedProject;
                    //--EI-48
                    //Adds datatable names to viewlist to enable other tables in project
                    List<string> SourceViewnames = this.sourceProject.GetViewNames();
                    SourceNonViewnames = this.sourceProject.GetNonViewTableNames();
                    foreach(string str in SourceViewnames)
                    {
                        View MView = this.sourceProject.GetViewByName(str);
                        DataTable ViewPages = MView.GetMetadata().GetPagesForView(MView.Id);
                        foreach(DataRow dt in ViewPages.Rows)
                        {
                            string ViewdataTable = MView.TableName + dt[ColumnNames.PAGE_ID];
                            Sourcedatatables.Add(ViewdataTable);
                        }
                        if (SourceNonViewnames.Contains(str)) { SourceNonViewnames.Remove(str); }
                    }
                    foreach(string str in Sourcedatatables)
                    {
                        SourceViewnames.Add(str);
                        if (SourceNonViewnames.Contains(str)) { SourceNonViewnames.Remove(str);}
                    }
                    
                   //--                    
                    
                    foreach (string s in this.sourceProject.GetNonViewTableNames())
                    {
                        string key = s.ToUpperInvariant().Trim();
                        if (!SourceProjectNames.Contains(key)) 
                        {
                           if (SourceViewnames.Contains(s))
                             {
                                SourceProjectNames.Add(key, true);
                             }
                        }
                    }
                    foreach (string s in this.sourceProject.GetViewNames())
                    {
                        string key = s.ToUpperInvariant().Trim();
                        if (!SourceProjectNames.Contains(key))
                        {
                            SourceProjectNames.Add(key, true);
                        }
                    }
                    
                }
            }
        }
        #endregion Constructor

		#region Private Methods
        private void Construct()
        {
            if (!this.DesignMode)           // designer throws an error
            {
                //this.btnOK.Click += new System.EventHandler(this.btnOK_Click);

            }
        }

        private void LoadForm()
        {
            PopulateDataSourcePlugIns();


 /*

            IProjectHost host = Module.GetService(typeof(IProjectHost)) as IProjectHost;
            try
            {
               
                Project project = null;
                if (host != null)
                {
                    project = host.CurrentProject;
                }

                project = Epi.Core.Interpreter.Reduction.CurrentProject;

                if (project != null)
                {
                    this.selectedProject = project;
                    this.selectedDataSource = project;
                }
            }
            catch (CurrentProjectInvalidException ex)
            {
                if (host != null)
                {
                    host.CurrentProject = null;
                }

                Epi.Windows.MsgBox.ShowInformation(ex.Message);
                //throw;
            }*/

            RefreshForm();
        }

        private void RefreshForm()
        {
            lvDataSourceObjects.Groups.Clear();
            lvDataSourceObjects.Items.Clear();

            //txtCurrentProj.Text = (selectedProject == null) ? "(none)" : selectedProject.FullName;

            if (selectedDataSource is IDbDriver)
            {
                IDbDriver db = selectedDataSource as IDbDriver;
                this.txtDataSource.Text = db.ConnectionString;

                List<string> tableNames = db.GetTableNames();

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

                ListViewGroup tablesGroup = new ListViewGroup("Tables");
                this.lvDataSourceObjects.Groups.Add(tablesGroup);
                foreach (string s in project.GetNonViewTableNames())                    
                {
                    ListViewItem newItem = new ListViewItem(new string[] { s, "Table" }, tablesGroup);
                    this.lvDataSourceObjects.Items.Add(newItem);
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

                    //cmbDataSourcePlugIns.Items.Add(new ComboBoxItem(null, "Epi Info 7 Project", null));

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
            ComboBoxItem selectedPlugIn = cmbDataSourcePlugIns.SelectedItem as ComboBoxItem;

            if (selectedPlugIn == null)
            {
                throw new GeneralException("No data source plug-in is selected in combo box.");
            }

            if (selectedPlugIn.Key == null) // default project
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = SharedStrings.SELECT_DATA_SOURCE;
                openFileDialog.Filter = "Epi Info " + SharedStrings.PROJECT_FILE + " (*.prj)|*.prj";

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
                            IProjectManager manager = Module.GetService(typeof(IProjectManager)) as IProjectManager;
                            if (manager == null)
                            {
                                throw new GeneralException("Project manager is not registered.");
                            }
                            selectedDataSource = manager.OpenProject(filePath);
                            this.SelectedTable = null;
                            
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
                switch(selectedPlugIn.Key)
                {
                    case Configuration.SqlDriver://"Epi.Data.SqlServer.SqlDBFactory, Epi.Data.SqlServer":
                        dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.SqlDriver);
                        break;
                    case Configuration.MySQLDriver: //"Epi.Data.MySQL.MySQLDBFactory, Epi.Data.MySQL":
                        dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.MySQLDriver);                        
                        break;
                    case Configuration.PostgreSQLDriver: 
                        dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.PostgreSQLDriver);
                        break;
                    case Configuration.ExcelDriver:
                        dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.ExcelDriver);
                        break;
                    case Configuration.Excel2007Driver:
                        dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.Excel2007Driver);
                        break;
                    case Configuration.AccessDriver: //"Epi.Data.Office.AccessDBFactory, Epi.Data.Office":
                        dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.AccessDriver);
                        break;
                    case Configuration.Access2007Driver:
                        dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.Access2007Driver);
                        break;
                    case Configuration.CsvDriver:
                        dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.CsvDriver);
                        break;
                    case Configuration.WebDriver: //"Epi.Data.WebDriver":
                        dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.WebDriver);
                      break;
                    default:
                        dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.AccessDriver);
                        break;
                }


                DbConnectionStringBuilder dbCnnStringBuilder = new DbConnectionStringBuilder();
                IDbDriver db = dbFactory.CreateDatabaseObject(dbCnnStringBuilder);  
                //IDbDriver db = DatabaseFactoryCreator.CreateDatabaseInstance(selectedPlugIn.Key);
                //IConnectionStringGui dialog = db.GetConnectionStringGuiForExistingDb();
                ////IConnectionStringGui dialog = this.selectedProject.Metadata.DBFactory.GetConnectionStringGuiForExistingDb();   
                IConnectionStringGui dialog = dbFactory.GetConnectionStringGuiForExistingDb();      
                DialogResult result = ((Form)dialog).ShowDialog();
                if (result == DialogResult.OK)
                {

                    bool success = false;

                    db.ConnectionString = dialog.DbConnectionStringBuilder.ToString() ;
                    txtDataSource.Text = db.ConnectionDescription;

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
                        this.SelectedTable = null;
                    }
                    else
                    {
                        this.selectedDataSource = null;
                        this.SelectedTable = null;
                    }

                }
                else
                {
                    this.selectedDataSource = null;
                    this.SelectedTable = null;
                }
            }

            RefreshForm();
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
            
			if (string.IsNullOrEmpty(this.txtViewName.Text))			
			{
				ErrorMessages.Add(SharedStrings.SPECIFY_PROJECT);
			}/*
			if (cmbDataSourcePlugIns.SelectedIndex == -1)
			{
				ErrorMessages.Add(SharedStrings.SPECIFY_DATAFORMAT);
			}*/
			if (string.IsNullOrEmpty(txtDataSource.Text))			
			{
				ErrorMessages.Add(SharedStrings.SPECIFY_DATASOURCE);
			}
			if (this.lvDataSourceObjects.SelectedIndices.Count == 0)
			{
				ErrorMessages.Add(SharedStrings.SPECIFY_TABLE_OR_VIEW);
			}
			return (ErrorMessages.Count == 0);
		}

	
		/// <summary>
		/// Checks if the input provided is sufficient and enables control buttons accordingly.
		/// </summary>
        public override void CheckForInputSufficiency()
		{
			bool inputValid = ValidateInput();
            if (inputValid && !string.IsNullOrEmpty(txtViewName.Text) && txtViewName.Text != "enter new view name")
            {
                if (!this.SourceProjectNames.Contains(txtViewName.Text.ToUpperInvariant().Trim()))
                {
                    btnOK.Enabled = true;
                    NewViewName = txtViewName.Text;
                }
                else
                {
                    btnOK.Enabled = false;
                    NewViewName = null;
                }
            }
            else
            {
                btnOK.Enabled = false;
                NewViewName = null;
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

            this.selectedDataSource = null;

            // TODO: Review this code. Select known database driver from configuration or prj file
            this.RefreshForm();
        }

        private void btnClear_Click(object sender, System.EventArgs e)
        {
            lvDataSourceObjects.SelectedItems.Clear();
            SetDataSourceToSelectedProject();
        }

        private void txtCurrentProj_TextChanged(object sender, System.EventArgs e)
        {
            //CheckForInputSufficiency();
        }

        private void txtDataSource_TextChanged(object sender, System.EventArgs e)
        {
            //CheckForInputSufficiency();
        }

        private void btnFindDataSource_Click(object sender, System.EventArgs e)
        {
            OpenSelectDataSourceDialog();
        }

        private void lvDataSourceObjects_SelectedIndexChanged(object sender, System.EventArgs e)
        {

            ListView LV = ((ListView)sender);

            if (LV.SelectedItems.Count  > 0)
            {
                this.SelectedTable = LV.SelectedItems[0].Text;
                this.txtViewName.Text = this.SelectedTable.Replace("$", string.Empty);
                //---Ei-48
                if (SourceNonViewnames.Contains(txtViewName.Text)) { txtViewName.Text = txtViewName.Text + count.ToString(); } 
                //
            }
            else
            {
                this.SelectedTable = null;
            }

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
                ListBox LB = ((ListBox)sender);
                if (LB.SelectedIndex > -1)
                {
                    this.SelectedTable = LB.SelectedItem.ToString();
                }
                else
                {
                    this.SelectedTable = null;
                }

                //btnOK_Click(sender, e);
            }
        }


        #endregion Event Handlers

        private void btnOK_Click(object sender, EventArgs e)
        {
            bool valid = true;
            string validationMessage = string.Empty;

            valid = View.IsValidViewName(txtViewName.Text, ref validationMessage);

            if (!valid)
            {
                MsgBox.ShowError(validationMessage);
                return;
            }
            //---Ei-48
              //case where tablename exists in current project
              if (SourceNonViewnames.Contains(txtViewName.Text.Trim()))
              {
                  validationMessage = SharedStrings.INVALID_VIEW_NAME_DUPLICATE + " " +  txtViewName.Text ;
                  MsgBox.ShowError(validationMessage);
                  return;
              }
            //--
            ButtonOkClicked = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.selectedProject = null;
            this.selectedDataSource = null;
            this.SelectedTable = null;

            this.Close();
        }


        private IDbDriver GetDbDriver(string pKey)
        {
            IDbDriverFactory dbFactory = null;
            switch (pKey)
            {
                case "Epi.Data.SqlServer.SqlDBFactory, Epi.Data.SqlServer":
                    dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.SqlDriver);
                    break;
                case "Epi.Data.MySQL.MySQLDBFactory, Epi.Data.MySQL":
                    dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.MySQLDriver);

                    break;
                case "Epi.Data.Office.AccessDBFactory, Epi.Data.Office":
                    dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.AccessDriver);
                    break;
                //case "":
                //    dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.ExcelDriver);
                //  break;
                default:
                    dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.AccessDriver);
                    break;
            }


            DbConnectionStringBuilder dbCnnStringBuilder = new DbConnectionStringBuilder();
            IDbDriver db = dbFactory.CreateDatabaseObject(dbCnnStringBuilder);

            db.ConnectionString = dbCnnStringBuilder.ToString();

            return db;
        }

        private void txtViewName_TextChanged(object sender, EventArgs e)
        {
            CheckForInputSufficiency();
        }

        private void ReadDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ButtonOkClicked)
            {
                this.selectedProject = null;
                this.selectedDataSource = null;
                this.SelectedTable = null;
            }
        }
    }

  
   
}