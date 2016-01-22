using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Data.Common;
using System.Text;
using Epi.Analysis;
using Epi.Core.AnalysisInterpreter;
using Epi.Data;
using Epi.Windows.Controls;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;

namespace Epi.Windows.Analysis.Dialogs
{
	/// <summary>
	/// Dialog for Merge command
	/// </summary>
    public partial class MergeDialog : CommandDesignDialog
	{
        private object selectedDataSource = null;
        private bool ignoreDataFormatIndexChange = false;


		#region Constructor

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public MergeDialog()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor for the Merge Dialog
        /// </summary>
        /// <param name="frm"></param>
        public MergeDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
		{
			InitializeComponent();
		}
		#endregion Constructors

		#region Private Methods
/*
		/// <summary>
		/// Attach the dataformat combo box
		/// </summary>
		private void LoadDataFormats()
		{
			ComboBox cbx = cmbDataFormats;
            //TODO: Attach list from configuration
			DataView dv = Epi.Data.Services.AppData.Instance.DatabaseFormatsDataTable.DefaultView;
			dv.Sort = ColumnNames.POSITION;
			cbx.DataSource = dv;
			cbx.DisplayMember = ColumnNames.NAME;
			cbx.ValueMember = ColumnNames.ID;
		}*/


        private void LoadForm()
        {
            PopulateDataSourcePlugIns();

            
            try
            {
                Project project = null;
                if (this.EpiInterpreter.Context.CurrentProject != null)
                {
                    project = this.EpiInterpreter.Context.CurrentProject;
                }

                if (project != null)
                {
                    selectedDataSource = project;
                    //txtCurrentProj.Enabled = false;
                    //txtCurrentProj.Text = project.FilePath;
                    LoadTables();
                }


            }
            catch (CurrentProjectInvalidException ex)
            {
                Epi.Windows.MsgBox.ShowInformation(ex.Message);
                //throw;
            }

            //RefreshForm();
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
                dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(selectedPlugIn.Key);

                if (dbFactory.ArePrerequisitesMet())
                {
                    DbConnectionStringBuilder dbCnnStringBuilder = new DbConnectionStringBuilder();
                    IDbDriver db = dbFactory.CreateDatabaseObject(dbCnnStringBuilder);

                    IConnectionStringGui dialog = dbFactory.GetConnectionStringGuiForExistingDb();
                    DialogResult result = ((Form)dialog).ShowDialog();
                    if (result == DialogResult.OK)
                    {

                        bool success = false;

                        db.ConnectionString = dialog.DbConnectionStringBuilder.ToString();
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
            LoadTables();
        }

        private void LoadTables()
        {
            //lbxShowViews.DataSource = null;

            if (selectedDataSource is IDbDriver)
            {
                IDbDriver db = selectedDataSource as IDbDriver;
                this.txtDataSource.Text = db.ConnectionString;

                List<string> tableNames = db.GetTableNames();

                //lbxShowViews.DataSource = tableNames;
                lbxShowAll.DataSource = tableNames;
                

                /*
                rbAll.Checked = true;
                rbView.Checked = false;
                rbView.Enabled = false;
                lbxShowViews.SelectedIndex = -1;*/
            }
            else if (selectedDataSource is Project)
            {
                //rbView.Enabled = true;
                
                Project project = selectedDataSource as Project;
                txtDataSource.Text = project.FullName;

                List<string> tableNames = project.GetViewNames();
                this.lbxShowViews.DataSource = tableNames;

                tableNames = project.GetNonViewTableNames();
                this.lbxShowAll.DataSource = tableNames;

                this.lbxShowViews.SelectedIndex = -1;
                this.lbxShowAll.SelectedIndex = -1;
            }
            else
            {
                // Clear ...
                this.txtDataSource.Text = string.Empty;
                this.lbxShowViews.DataSource = null;
                this.lbxShowAll.DataSource = null;
                
            }
            tabctrlShow.TabPages[1].Show();
        }

		#endregion //Private Methods


        #region Protected Methods

        /// <summary>
        /// Generates the command text
        /// </summary>
        protected override void GenerateCommand()
        {
            StringBuilder command = new StringBuilder();
            command.Append(CommandNames.MERGE);

            command.Append(StringLiterals.SPACE);
            command.Append(StringLiterals.CURLY_BRACE_LEFT);
            command.Append(txtDataSource.Text);
            command.Append(StringLiterals.CURLY_BRACE_RIGHT);
            command.Append(StringLiterals.COLON);

            if (tabctrlShow.SelectedTab == tabctrlShow.TabPages[0])
            {
                command.Append(lbxShowViews.SelectedItem);
            }
            else
            {
                command.Append(lbxShowAll.SelectedItem);
            }

            command.Append(StringLiterals.SPACE);
            command.Append(txtKey.Text);

            CommandText = command.ToString();
        }
        /// <summary>
        /// Validates user input
        /// </summary>
        /// <returns>True/False depending upon whether error messages were found</returns>
        protected override bool ValidateInput()
        {
            // reset error mesages
            base.ValidateInput();

            /*
            if (string.IsNullOrEmpty(txtCurrentProj.Text))
            {
                ErrorMessages.Add(SharedStrings.SPECIFY_PROJECT);
            }*/

            if (string.IsNullOrEmpty(txtDataSource.Text))
            {
                ErrorMessages.Add(SharedStrings.SPECIFY_PROJECT);
            }

            if (string.IsNullOrEmpty(lbxShowViews.Text) && string.IsNullOrEmpty(lbxShowAll.Text))
            {
                ErrorMessages.Add(SharedStrings.NO_TABLE_SELECTED);
            }

            if (string.IsNullOrEmpty(txtKey.Text))
            {
                ErrorMessages.Add(SharedStrings.NO_RELATE_KEYS);
            }

            return (ErrorMessages.Count == 0);
        }
        /// <summary>
        /// Sets enabled property of OK and Save Only
        /// </summary>
        public override void CheckForInputSufficiency()
        {
            bool inputValid = ValidateInput();
            btnOK.Enabled = inputValid;
            btnSaveOnly.Enabled = inputValid;
            btnClear.Enabled = inputValid;
            //btnBuildKey.Enabled = inputValid;
        }

        #endregion //Protected Methods


		#region Event Handlers
		/// <summary>
		/// Calls method to load cmbDataFormats
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void Merge_Load(object sender, System.EventArgs e)
		{
            this.LoadForm();
            //LoadDataFormats();
		}


        private void btnClear_Click(object sender, EventArgs e)
        {
            cmbDataFormats.SelectedIndex = -1;
            LoadTables();
            //btnBuildKey.Enabled = false;
        }

        private void cmbDataFormats_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ignoreDataFormatIndexChange) return;
            this.selectedDataSource = null;
            LoadTables();
        }

        private void btnEllipse_Click(object sender, EventArgs e)
        {
            OpenSelectDataSourceDialog();
        }

        private void View_CheckedChanged(object sender, EventArgs e)
        {
            LoadTables();
        }

        private void btnBuildKey_Click(object sender, EventArgs e)
        {
            if (tabctrlShow.SelectedTab == tabctrlShow.TabPages[0])
            {
                if (lbxShowViews.SelectedIndex != -1)
                {
                    string relatedTableName = lbxShowViews.SelectedItem.ToString();
                    BuildKeyDialog buildKeyDialog = new BuildKeyDialog(mainForm);
                    buildKeyDialog.RelatedTable = relatedTableName;
                    buildKeyDialog.SelectedDataSource = selectedDataSource;
                    DialogResult result = buildKeyDialog.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        txtKey.Text = buildKeyDialog.Key;
                    }
                }
            }
            else
            {
                if (this.lbxShowAll.SelectedIndex != -1)
                {
                    string relatedTableName = this.lbxShowAll.SelectedItem.ToString();
                    BuildKeyDialog buildKeyDialog = new BuildKeyDialog(mainForm);
                    buildKeyDialog.RelatedTable = relatedTableName;
                    buildKeyDialog.SelectedDataSource = selectedDataSource;
                    DialogResult result = buildKeyDialog.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        txtKey.Text = buildKeyDialog.Key;
                    }
                }
            }
        }
        private void lbxShowViews_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnBuildKey.Enabled = (lbxShowViews.SelectedIndex != -1);
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/classic-analysis/How-to-Manage-Data-Use-the-MERGE-Command.html");
        }

		#endregion //Event Handlers		

        private void btnOK_Click_1(object sender, EventArgs e)
        {
                base.OnOK();
        }

        private void btnSaveOnly_Click_1(object sender, EventArgs e)
        {
            base.btnSaveOnly_Click(sender, e);
        }

        private void cmbDataFormats_SelectedValueChanged(object sender, EventArgs e)
        {
            string fromatNameSelected;

            fromatNameSelected = cmbDataFormats.SelectedItem.ToString();

            if (fromatNameSelected.Contains("Project"))
            {
                tabctrlShow.SelectedTab = tabPageViews;
                this.selectedDataSource = null;
                LoadTables();
            }
            else
            {
                tabctrlShow.SelectedTab = tabPageAll;
                this.selectedDataSource = null;
                LoadTables();
            }
        }
	}
}