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
using Epi.Windows.Controls;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;

namespace Epi.Windows.Analysis.Dialogs
{
	/// <summary>
	/// Dialog for Relate command
	/// </summary>
    public partial class RelateDialog : CommandDesignDialog
	{
		#region Constructors

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public RelateDialog()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor for the Relate dialog
        /// </summary>
        /// <param name="frm"></param>
        public RelateDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
		{
			InitializeComponent();
            Construct();
            this.EpiInterpreter = frm.EpiInterpreter;
		}
		#endregion Constructors		

        #region Pivate Attributes
        private object selectedDataSource = null;
        private bool ignoreDataFormatIndexChange = false;
        #endregion Pivate Attributes

        #region Private Methods

        private void Construct()
        {
            if (!this.DesignMode)
            {
                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
            }
        }
        private void LoadForm()
        {
            PopulateDataSourcePlugIns();


            if (this.EpiInterpreter.Context.CurrentRead.IsEpi7ProjectRead)
            {
                selectedDataSource = new Project(this.EpiInterpreter.Context.CurrentRead.File);
                LoadTables();
            }
            
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
                            txtDataSource.Text = filePath;
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
                IDbDriverFactory dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(selectedPlugIn.Key);
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
                LoadTables();
            }
        }

        private void LoadTables()
        {
            lbxDataSourceObjects.DataSource = null;

            if (selectedDataSource is IDbDriver)
            {
                IDbDriver db = selectedDataSource as IDbDriver;
                this.txtDataSource.Text = db.ConnectionString;

                List<string> tableNames = db.GetTableNames();

                lbxDataSourceObjects.DataSource = tableNames;
                rbAll.Checked = true;
                rbView.Checked = false;
                rbView.Enabled = false;
                lbxDataSourceObjects.SelectedIndex = -1;
            }
            else if (selectedDataSource is Project)
            {
                rbView.Enabled = true;
                Project project = selectedDataSource as Project;
                txtDataSource.Text = project.FullName;

                if (rbView.Checked)
                {
                    List<string> tableNames = project.GetViewNames();
                    lbxDataSourceObjects.DataSource = tableNames;
                }
                else if (rbAll.Checked)
                {
                    List<string> tableNames = project.GetNonViewTableNames();
                    lbxDataSourceObjects.DataSource = tableNames;
                }
                lbxDataSourceObjects.SelectedIndex = -1;
            }
            else
            {
                // Clear ...
                this.txtDataSource.Text = string.Empty;
                lbxDataSourceObjects.DataSource = null;
                rbView.Enabled = true;
            }
        }

        #endregion Private Methods

        #region Protected Methods

        /// <summary>
        /// Generates the command text
        /// </summary>
        protected override void GenerateCommand()
        {
            StringBuilder command = new StringBuilder();
            command.Append(CommandNames.RELATE);
            if (!txtDataSource.Text.Equals(this.EpiInterpreter.Context.CurrentRead.File.Trim(StringLiterals.SINGLEQUOTES.ToCharArray()), StringComparison.CurrentCultureIgnoreCase))
            {
                command.Append(StringLiterals.SPACE);
                command.Append(StringLiterals.CURLY_BRACE_LEFT);
                command.Append(txtDataSource.Text);
                command.Append(StringLiterals.CURLY_BRACE_RIGHT);
                command.Append(StringLiterals.COLON);
            }
            else
            {
                command.Append(StringLiterals.SPACE);
            }
            command.Append(lbxDataSourceObjects.SelectedItem);
            command.Append(StringLiterals.SPACE);
            command.Append(txtKey.Text);
            if (cbxUnmatched.Checked)
            {
                command.Append( StringLiterals.SPACE ).Append( SharedStrings.WORD_ALL );
            }

            CommandText = command.ToString();
        }
        /// <summary>
        /// Validates user input
        /// </summary>
        /// <returns>True/False depending upon whether error messages were found</returns>
        protected override bool ValidateInput()
        {
            base.ValidateInput();
            //if (string.IsNullOrEmpty( txtCurrentProj.Text ))
            //{
            //    ErrorMessages.Add( SharedStrings.SPECIFY_PROJECT );
            //}

            if (string.IsNullOrEmpty( txtDataSource.Text ))
            {
                ErrorMessages.Add( SharedStrings.SPECIFY_PROJECT );
            }

            if (string.IsNullOrEmpty( lbxDataSourceObjects.Text ))
            {
                ErrorMessages.Add( SharedStrings.NO_TABLE_SELECTED );            
            }

            if (string.IsNullOrEmpty(txtKey.Text))
            {
                //if (!cmbDataFormats.SelectedItem.ToString().Equals("Epi Info 7 Project"))
                ErrorMessages.Add( SharedStrings.NO_RELATE_KEYS );
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
            btnBuildKey.Enabled = true;
        }

        #endregion //Protected Methods

        #region Event handlers

        private void btnClear_Click( object sender, EventArgs e )
        {
            cmbDataFormats.SelectedIndex = -1;
            LoadTables();
            btnBuildKey.Enabled = false;
        }

        private void RelateDialog_Load( object sender, EventArgs e )
        {
            LoadForm();
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
            if (lbxDataSourceObjects.SelectedIndex != -1)
            {
                string relatedTableName = lbxDataSourceObjects.SelectedItem.ToString();
                BuildKeyDialog buildKeyDialog = new BuildKeyDialog(mainForm);
                buildKeyDialog.RelatedTable = relatedTableName;
                buildKeyDialog.SelectedDataSource = selectedDataSource;
                DialogResult result = buildKeyDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    txtKey.Text = buildKeyDialog.Key;
                }

                this.CheckForInputSufficiency();
            }
        }
        private void lbxDataSourceObjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnBuildKey.Enabled = (lbxDataSourceObjects.SelectedIndex != -1);
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://wwwn.cdc.gov/epiinfo/user-guide/classic-analysis/How-to-Manage-Data-Use-Related-Forms.html");
        }

        #endregion Event handlers



    }
}

