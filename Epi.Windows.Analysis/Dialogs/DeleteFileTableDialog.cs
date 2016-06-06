using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Analysis;
using Epi.Core.AnalysisInterpreter;
using Epi.Data;
using Epi.Windows;
using Epi.Windows.Controls;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;

namespace Epi.Windows.Analysis.Dialogs
{
	/// <summary>
	/// Dialog for Delete File/Table command
	/// </summary>
    public partial class DeleteFileTableDialog : CommandDesignDialog
	{
		#region Constructors

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public DeleteFileTableDialog()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor for DeleteFileTableDialog
        /// </summary>
        /// <param name="frm"></param>
        public DeleteFileTableDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
		{
			InitializeComponent();
            construct();
		}
		#endregion Constructors

        #region Private Members
            //System.Data.DataTable tables;
            //System.Data.DataTable views;
        private object selectedDataSource = null;
        private bool ignoreDataFormatIndexChange = false;
        #endregion
		
		#region Protected Methods
		/// <summary>
		/// Validates user input
		/// </summary>
		/// <returns>true if there is no error; else false</returns>
		protected override bool ValidateInput()
		{
			base.ValidateInput();

            if (WinUtil.GetSelectedRadioButton(gbxDelete) == rdbFiles)
			{
				if (string.IsNullOrEmpty(txtFileName.Text.Trim()))					 
				{
					ErrorMessages.Add(SharedStrings.NO_FILES_SELECTED);
				}
			}
            else if (WinUtil.GetSelectedRadioButton(gbxDelete) == rdbTable)
			{	
				if (string.IsNullOrEmpty(cmbTableName.Text.Trim()))					
				{
					ErrorMessages.Add(SharedStrings.NO_TABLE_SELECTED);
				}
			}
            else if (WinUtil.GetSelectedRadioButton(gbxDelete) == rdbView)
			{
				if (string.IsNullOrEmpty(cmbTableName.Text.Trim()))					
				{
					ErrorMessages.Add(SharedStrings.NO_VIEW_SELECTED);
				}
			}
				
			
			return (ErrorMessages.Count == 0);
			
		}
		/// <summary>
		/// Generates user command
		/// </summary>
		protected override void GenerateCommand()
		{
			StringBuilder sb = new StringBuilder();
            sb.Append(CommandNames.DELETE);
            if (rdbFiles.Checked)
            {
                sb.Append(StringLiterals.SPACE);
                sb.Append(StringLiterals.CURLY_BRACE_LEFT);
                if (txtFileName.Text.StartsWith("Provider=Microsoft.Jet.OLEDB.4.0"))
                {
                    sb.Append(txtFileName.Text.Substring(txtFileName.Text.IndexOf("ource=") + 6, txtFileName.Text.Length - txtFileName.Text.IndexOf("ource=") - 6));
                }
                else
                {
                    sb.Append(txtFileName.Text);
                }
                sb.Append(StringLiterals.CURLY_BRACE_RIGHT);
            }
            else
            {
                if (txtFileName.Text.Length > 0)
                {
                    sb.Append(StringLiterals.SPACE);
                    sb.Append(CommandNames.TABLES);
                    sb.Append(StringLiterals.SPACE);
                    sb.Append(StringLiterals.CURLY_BRACE_LEFT);
                    if (txtFileName.Text.StartsWith("Provider=Microsoft.Jet.OLEDB.4.0") || txtFileName.Text.StartsWith("Provider=Microsoft.ACE.OLEDB.12.0"))
                    {
                        if (txtFileName.Text.Substring(txtFileName.Text.IndexOf("ource=") + 6).IndexOf(';')!=-1)                      
                        sb.Append(txtFileName.Text.Substring(txtFileName.Text.IndexOf("ource=") + 6, txtFileName.Text.Substring(txtFileName.Text.IndexOf("ource=") + 6).IndexOf(';')));
                        else
                            sb.Append(txtFileName.Text.Substring(txtFileName.Text.IndexOf("ource=") + 6));
                    }
                    else
                    {
                        sb.Append(txtFileName.Text);
                    }
                    sb.Append(StringLiterals.CURLY_BRACE_RIGHT);
                    sb.Append(StringLiterals.COLON);
                    sb.Append(cmbTableName.Text);
                }
                else
                {
                    sb.Append(StringLiterals.SPACE);
                    sb.Append(CommandNames.TABLES);
                    sb.Append(StringLiterals.SPACE);
                    sb.Append(cmbTableName.Text);
                }
            }

			if (cbkRunSilent.Checked)
			{
				sb.Append(StringLiterals.SPACE).Append(CommandNames.RUNSILENT);
			}
			if(cbxSaveDataTables.Checked)
			{
				sb.Append(StringLiterals.SPACE).Append(CommandNames.SAVEDATA);
			}
			CommandText = sb.ToString();
		}
		/// <summary>
		/// Sets enabled property of OK and Save Only
		/// </summary>
        public override void CheckForInputSufficiency()
		{
			bool inputValid = ValidateInput();
			btnOK.Enabled = inputValid;
			btnSaveOnly.Enabled = inputValid;
		}
		#endregion //Protected Methods

		#region Event Handlers
		/// <summary>
		/// Displays OpenFileDialog
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void btnEllipse_Click(object sender, System.EventArgs e)
        {
            if (rdbFiles.Checked)
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "All Files(*.*)|" + "*.*";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtFileName.Text = dialog.FileName;
                }
                
            }
            else
            {
                OpenSelectDataSourceDialog();
            }
            #region oldcode
            /*OpenFileDialog dialog = new OpenFileDialog();
            if (rdbFiles.Checked)
            {
                dialog.Filter = "All Files(*.*)|" + "*.*";
            }
            else if(rdbTable.Checked)
            {
                dialog.Filter = "All Files(*.mdb)|" + "*.mdb";
            }

			if(dialog.ShowDialog() == DialogResult.OK)
			{
                txtFileName.Text = dialog.FileName;

                #region oldcode
                //throw new NotImplementedException("Wait code for deleting any database table");

                //try
                //{
                //    txtFileName.Text = dialog.FileName;
                //    if (txtFileName.Text.Length > 0  && rdbTable.Checked) //fill table combo for the selected DB
                //    {
                //        string cnnstring = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + txtFileName.Text;   // +";User Id = admin; PWD =;";
                //        try
                //        {

                //            //IDbDriver db = DatabaseFactoryCreator.CreateDatabaseInstance("Epi.Data.Office.AccessDatabase, Epi.Data.Office", cnnstring);
                //            //tables =db.GetTableSchema();

                //            //DataTable viewsAndTables = new DataTable("UserTables");
                //            //viewsAndTables.Columns.Add(ColumnNames.NAME);
                //            //DataRow[] rows = tables.Select("TABLE_NAME not like 'meta%'");
                //            //DataRow dataRow;
                //            //foreach (DataRow row in rows)
                //            //{
                //            //    string tableName = row["Table_Name"].ToString();
                //            //    dataRow = viewsAndTables.NewRow();
                //            //    dataRow[ColumnNames.NAME] = tableName;
                //            //    viewsAndTables.Rows.Add(dataRow);
                //            //}

                            
                //            //cmbTableName.DataSource = null;
                //            //cmbTableName.Refresh();
                //            //cmbTableName.DataSource = viewsAndTables;
                //            //cmbTableName.DisplayMember = ColumnNames.NAME;
                //        }
                //        catch (Exception ex)
                //        {
                //            MsgBox.ShowApplicationException(ex);
                //        }
                //        finally
                //        {
                //        }
                //    }
                //}
                //finally
                //{
                //}
                #endregion
            }*/
            #endregion
        }
		/// <summary>
		/// Clear text
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void btnClear_Click(object sender, System.EventArgs e)
		{
			txtFileName.Text = string.Empty;
            cmbTableName.SelectedIndex = -1;
			//cmbTableName.SelectedText = string.Empty;
			cbkRunSilent.Checked = false;
			rdbFiles.Checked = true;
			RadioButtonClick(rdbFiles,e);
		}
		/// <summary>
		/// Common event handler for radio button click event
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void RadioButtonClick(object sender, System.EventArgs e)
		{   
            btnEllipse.Enabled = true;
            txtFileName.Enabled = true;
            txtFileName.Text = string.Empty;

			if (rdbFiles.Checked)
			{
				ToggleControls(false,false);
			}
			else if (rdbTable.Checked)
			{

				ToggleControls(true,false);

                cmbTableName.DataSource = null;
                cmbTableName.Refresh();
                if (this.EpiInterpreter.Context.CurrentProject != null)
                {
                    cmbTableName.DataSource = this.EpiInterpreter.Context.CurrentProject.GetNonViewTableNames();
                }
                //cmbTableName.DisplayMember = ColumnNames.NAME;
                
                
			}
			else if (rdbView.Checked)
			{
                btnEllipse.Enabled = false;
                txtFileName.Enabled = false;
                cbxSaveDataTables.Checked = true;
				ToggleControls(true,true);
				lblTableName.Visible = false;

                cmbTableName.DataSource = null;
                cmbTableName.Refresh();
                cmbTableName.DataSource = ((AnalysisWindowsModule)Module).CurrentProject.GetViewNames(); 
                //cmbTableName.DisplayMember = ColumnNames.NAME;

			}
		}
		/// <summary>
		/// When focus leaves txtFileName enable OK and SaveOnly buttons
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void txtFileName_Leave(object sender, System.EventArgs e)
		{
			CheckForInputSufficiency();
		}
		/// <summary>
		/// Enable OK and SaveOnly only when there is text in FileName
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void txtFileName_TextChanged(object sender, System.EventArgs e)
		{
			CheckForInputSufficiency();			
		}
		/// <summary>
		/// Common event handler for validating table name
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void TableNameValidate(object sender, System.EventArgs e)
		{
			CheckForInputSufficiency();			
		}


        private void cmbDataFormats_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ignoreDataFormatIndexChange) return;
            this.selectedDataSource = null;
            LoadTables();
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-delete-file-tables.html");
        }


		#endregion //Event Handlers

		#region Private Methods
        private void construct()
        {
            if (!this.DesignMode)
            {
                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
            }
            PopulateDataSourcePlugIns();
        }		/// <summary>
		/// Based on radiobutton selection, controls are shown/hidden
		/// </summary>
		/// <param name="visible">True if visible; otherwise false</param>
		/// <param name="saveDataTablesVisible">True to show DataTables; otherwise false</param>
		private void ToggleControls(bool visible, bool saveDataTablesVisible)
		{
			lblFileName.Visible = !(visible);
            lblDataFormat.Visible = visible;
            cmbDataFormats.Visible = visible;
			lblDatabase.Visible = visible;
			lblTableName.Visible = visible;
			cmbTableName.Visible = visible;
			cbxSaveDataTables.Visible = saveDataTablesVisible;
			lblViewName.Visible = saveDataTablesVisible;
			
		}

        private void PopulateDataSourcePlugIns()
        {
            try
            {
                Configuration config = Configuration.GetNewInstance();
                if (cmbDataFormats.Items.Count == 0)
                {
                    cmbDataFormats.Items.Clear();

                    cmbDataFormats.Items.Add(new ComboBoxItem(null, "Epi Info 7 Project", null));

                    foreach (Epi.DataSets.Config.DataDriverRow row in config.DataDrivers)
                    {
                        cmbDataFormats.Items.Add(new ComboBoxItem(row.Type, row.DisplayName, null));
                    }
                }
                cmbDataFormats.SelectedIndex = 0;
            }
            finally
            {
                ignoreDataFormatIndexChange = false;
            }
        }

        private void OpenSelectDataSourceDialog()
        {
            ComboBoxItem selectedPlugIn = cmbDataFormats.SelectedItem as ComboBoxItem;

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
                            selectedDataSource = new Project(filePath);
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
                switch (selectedPlugIn.Key)
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
                    case "Epi.Data.Office.ExcelWBFactory, Epi.Data.Office":
                        dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.ExcelDriver);
                        break;
                    case "Epi.Data.Office.Excel2007WBFactory, Epi.Data.Office":
                        dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.Excel2007Driver);
                        break;
                    case "Epi.Data.Office.Access2007DBFactory, Epi.Data.Office":
                        dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.Access2007Driver);
                        break;
                    case "Epi.Data.Office.CsvFileFactory, Epi.Data.Office":
                        dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.CsvDriver);
                        break;
                    case "Epi.Data.PostgreSQL.PostgreSQLDBFactory, Epi.Data.PostgreSQL":
                        dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.PostgreSQLDriver);
                        break;
                    default:
                        dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.AccessDriver);
                        break;
                }

                DbConnectionStringBuilder dbCnnStringBuilder = new DbConnectionStringBuilder();
                IDbDriver db = dbFactory.CreateDatabaseObject(dbCnnStringBuilder);
                IConnectionStringGui dialog = dbFactory.GetConnectionStringGuiForExistingDb();
                DialogResult result = ((Form)dialog).ShowDialog();
                if (result == DialogResult.OK)
                {

                    bool success = false;

                    db.ConnectionString = dialog.DbConnectionStringBuilder.ToString();
                    txtFileName.Text = db.ConnectionDescription;

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
            LoadTables();
        }

        private void LoadTables()
        {
            cmbTableName.DataSource = null;

            if (selectedDataSource is IDbDriver)
            {
                IDbDriver db = selectedDataSource as IDbDriver;
                this.txtFileName.Text = db.ConnectionString;

                List<string> tableNames = db.GetTableNames();

                cmbTableName.DataSource = tableNames;
                cmbTableName.SelectedIndex = -1;
            }
            else if (selectedDataSource is Project)
            {
                Project project = selectedDataSource as Project;
                txtFileName.Text = project.FullName;
                
                if (rdbView.Checked)
                {
                    List<string> tableNames = project.GetViewNames();
                    cmbTableName.DataSource = tableNames;
                }
                else if (rdbTable.Checked)
                {
                    List<string> tableNames = project.GetNonViewTableNames();
                    cmbTableName.DataSource = tableNames;
                }
                cmbTableName.SelectedIndex = -1;
            }
            else
            {
                // Clear ...
                this.txtFileName.Text = string.Empty;
                cmbTableName.DataSource = null;
            }
        }

		#endregion //Private Methods





	}
}

