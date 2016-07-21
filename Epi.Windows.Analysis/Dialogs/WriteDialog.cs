using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Data.Common;
using Epi;
using Epi.Data;
using Epi.Analysis;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;

namespace Epi.Windows.Analysis.Dialogs
{
	/// <summary>
	/// Dialog for Write command
	/// </summary>
    public partial class WriteDialog : CommandDesignDialog
	{

        private object selectedDataSource;

		#region Constructor

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public WriteDialog()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor for the Write dialog
        /// </summary>
        /// <param name="frm"></param>
        public WriteDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
		{
			InitializeComponent();
            Construct();
		}
		#endregion Constructors

		#region Private Methods

        /// <summary>
        /// Set event handlers (in base) for OK and SaveOnly events
        /// </summary>
        private void Construct()
        {
            if (!this.DesignMode)
            {
                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);

                PopulateDataSourcePlugIns();
            }
        }
		/// <summary>
		/// Loads cmbOutputFormats
		/// </summary>
		private void LoadOutputFormats()
		{
			ComboBox cbx = cmbOutputFormat;

            //TODO: Attach list from configuration
            //DataView dv = AppData.Instance.DatabaseFormatsDataTable.DefaultView;
			//dv.Sort = ColumnNames.POSITION;
			//cbx.DataSource = dv;
			cbx.DisplayMember = ColumnNames.NAME;
			cbx.ValueMember = ColumnNames.ID;
            cbx.SelectedIndex = 0;

            
		}

		private void LoadVariables()
		{
            VariableType scopeWord = VariableType.DataSource | VariableType.Standard | VariableType.Global;
            FillVariableListBox(lbxVariables, scopeWord);
            ////First get the list of all variables
            //// DataTable variables = GetAllVariablesAsDataTable(true, true, true, false);
            //DataTable variables = GetMemoryRegion().GetVariablesAsDataTable(
            //                                        VariableType.DataSource |
            //                                        VariableType.Standard |
            //                                        VariableType.Global);
            ////Sort the data
            //System.Data.DataView dv = variables.DefaultView;
            //dv.Sort = ColumnNames.NAME;
            //lbxVariables.DataSource = dv;
            //lbxVariables.DisplayMember = ColumnNames.NAME;
            //lbxVariables.ValueMember = ColumnNames.NAME;
			lbxVariables.SelectedIndex = -1;
			this.lbxVariables.SelectedIndexChanged += new System.EventHandler(this.lbxVariables_SelectedIndexChanged);
		}

        private void SomethingChanged(object sender, EventArgs e)
        {
            cmbDataTable.Enabled = (cmbOutputFormat.Text != "Text");
            CheckForInputSufficiency();
        }

        /// <summary>
        /// Attach the DataFormats combobox with supported data formats
        /// </summary>
        private void PopulateDataSourcePlugIns()
        {
            Configuration config = Configuration.GetNewInstance();

            cmbOutputFormat.Items.Clear();

            foreach (Epi.DataSets.Config.DataDriverRow row in config.DataDrivers)
            {
                cmbOutputFormat.Items.Add(new ComboBoxItem(row.Type, row.DisplayName, null));
            }

            //cmbOutputFormat.Items.Add(new ComboBoxItem(null, "Text", null));
            cmbOutputFormat.SelectedIndex = 0;
        }

		#endregion Private Methods

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

		#region Event Handlers
		/// <summary>
		/// Loads the combo box with valid data formats
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void Write_Load(object sender, System.EventArgs e)
		{
			LoadOutputFormats();
			LoadVariables();
		}


        ///// <summary>
        ///// Sets Enabled property of cbxAllExcept
        ///// </summary>
        ///// <param name="sender">Object that fired the event.</param>
        ///// <param name="e">.NET supplied event args.</param>
        //private void cbxAll_CheckChanged(object sender, System.EventArgs e)
        //{
        //    if (cbxAll.Checked)
        //    {
				
        //        cbxAllExcept.Enabled = false;
        //        cbxAllExcept.Checked = false;
        //    }
        //    else
        //    {
        //        cbxAllExcept.Enabled = true;
        //    }
        //}


		/// <summary>
		/// Clears user input
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void btnClear_Click(object sender, System.EventArgs e)
		{
			lbxVariables.SelectedIndex = -1;
            txtFileName.Text = string.Empty;
            cmbDataTable.Text = string.Empty;
            cmbDataTable.SelectedIndex = -1;
            cmbDataTable.SelectedIndex = -1;
            this.cmbDataTable.SelectedItem = null;
            //cbxAll.Checked = true;
		}
		
        private void btnGetFile_Click(object sender, System.EventArgs e)
		{
            ComboBoxItem selectedPlugIn = cmbOutputFormat.SelectedItem as ComboBoxItem;

            if (selectedPlugIn == null)
            {
                throw new GeneralException("No data source plug-in is selected in combo box.");
            }

            if (selectedPlugIn.Key == null) // default project
            {
                OpenFileDialog dlg = new OpenFileDialog();
                if (cmbOutputFormat.Text == "Text" || cmbOutputFormat.Text.ToUpperInvariant() == "FLAT ASCII FILE")
                {
                    dlg.Filter = "Text Files (*.txt) |*.txt";
                }
                else
                {
                    dlg.Filter = "Database Files (*.mdb) |*.mdb";
                }
                dlg.CheckFileExists = false;
                dlg.CheckPathExists = false;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    if (cmbOutputFormat.Text.ToUpperInvariant() == "TEXT" || cmbOutputFormat.Text.ToUpperInvariant() == "FLAT ASCII FILE")
                    {
                        if (!dlg.FileName.EndsWith(".txt") && dlg.FileName.EndsWith(".csv"))
                        {
                            txtFileName.Text = dlg.FileName + ".csv";
                        }
                        else
                        {
                            txtFileName.Text = dlg.FileName;
                        }
                    }
                    else
                    {
                        txtFileName.Text = dlg.FileName;
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
                    case "Epi.Data.Office.Access2007DBFactory, Epi.Data.Office":
                        dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.Access2007Driver);
                        break;
                    case "Epi.Data.Office.Excel2007WBFactory, Epi.Data.Office":
                        dbFactory = DbDriverFactoryCreator.GetDbDriverFactory(Configuration.Excel2007Driver);
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
                dialog.ShouldIgnoreNonExistance = true;
                DialogResult result = ((Form)dialog).ShowDialog();
                if (result == DialogResult.OK)
                {
                    bool success = false;

                    db.ConnectionString = dialog.DbConnectionStringBuilder.ToString();
                    txtFileName.Text = db.ConnectionString;

                    try
                    {
                        success = db.TestConnection();
                    }
                    catch
                    {
                        success = false;
                        //MessageBox.Show("Could not connect to selected data source.");
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

            if (selectedDataSource is IDbDriver)
            {
                IDbDriver db = selectedDataSource as IDbDriver;
                //this.txtDataSource.Text = db.ConnectionString;

                System.Collections.Generic.List<string> tableNames = db.GetTableNames();

                foreach (string tableName in tableNames)
                {
                    ComboBoxItem newItem = new ComboBoxItem(tableName, tableName,tableName);
                    this.cmbDataTable.Items.Add(newItem);
                }

                
            }
            else if (selectedDataSource is Project)
            {

                Project project = selectedDataSource as Project;
                //txtDataSource.Text = (selectedDataSource == selectedProject) ? SharedStrings.CURRENT_PROJECT : project.FullName;

                foreach (string s in project.GetViewNames())
                {
                    ComboBoxItem newItem = new ComboBoxItem(s, s, s);
                    this.cmbDataTable.Items.Add(newItem);
                }
                
            }
		}

        private void lbxVariables_SelectedIndexChanged(object sender, System.EventArgs e)
		{
				//cbxAll.Checked = false;
            //if no variables are selected, then ALL (*) are requested by default
            //if AllExcept is checked, require at least one variable to be selected
            if (lbxVariables.SelectedItems.Count < 1)
            {
                cbxAllExcept.Checked = false;
                cbxAllExcept.Enabled = false;
            }
            else
            {
                cbxAllExcept.Enabled = true;
            }
				//cbxAllExcept.Checked = false;
            CheckForInputSufficiency();
		}

        private void FileNameChanged(object sender, EventArgs e)
        {
            cmbDataTable.SelectedIndex = -1;
            if (cmbOutputFormat.Text == "Text")
            {
                cmbDataTable.Enabled = (cmbOutputFormat.Text != "Text");
            }
            CheckForInputSufficiency();
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-write.html");
        }

		#endregion //Event Handlers

        #region Public Methods
        /// <summary>
        /// Checks if input is sufficient and Enables control buttons accordingly
        /// </summary>
        public override void CheckForInputSufficiency()
        {
            bool inputValid = ValidateInput();
            btnOK.Enabled = inputValid;
            btnSaveOnly.Enabled = inputValid;
        }
        #endregion Public Methods

        #region Protected Methods

        protected override void OnOK()
        {
            try
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                System.Collections.Generic.List<string> ProblemVariableList = new System.Collections.Generic.List<string>();
                if (cbxAllExcept.Checked)
                {
                    
                    foreach (string s in lbxVariables.Items)
                    {
                        if (!this.lbxVariables.SelectedItems.Contains(s))
                        {
                            if (s.IndexOf(' ') == 0)
                            {
                                

                                if (ProblemVariableList.Count == 0)
                                {
                                    sb.Append("[");
                                }
                                else
                                {
                                    sb.Append(" ,[");
                                }
                                sb.Append(s);
                                sb.Append("] ");
                                ProblemVariableList.Add(s);
                            }

                        }
                    }
                }
                else
                {
                    foreach (string s in lbxVariables.SelectedItems)
                    {
                        if (s.IndexOf(' ') == 0)
                        {
                            if (ProblemVariableList.Count == 0)
                            {
                                sb.Append("[");
                            }
                            else
                            {
                                sb.Append(" ,[");
                            }
                            sb.Append(s);
                            sb.Append("] ");
                            ProblemVariableList.Add(s);
                        }
                    }
                }

                if (ProblemVariableList.Count > 0)
                {
                    Epi.Windows.MsgBox.ShowError(string.Format(SharedStrings.EXPORT_CANNOT_PROCEED_LEADING_TRAILING_SPACES, sb.ToString()));
                    return;
                }
                else if (selectedDataSource is IDbDriver)
                {
                    
                    Type csv = Type.GetType("Epi.Data.Office.CsvFile, Epi.Data.Office");
                    if (selectedDataSource.GetType().AssemblyQualifiedName == csv.AssemblyQualifiedName)
                    {
                        cmbDataTable.Text = cmbDataTable.Text.Replace('.', '#');
                        if (!cmbDataTable.Text.Contains("#"))
                        {
                            //cmbDataTable.Text = cmbDataTable.Text + "#txt";
                            cmbDataTable.Text = cmbDataTable.Text + "#csv";
                        }
                    }


                    IDbDriver db = selectedDataSource as IDbDriver;
                    if (db.TableExists(cmbDataTable.Text))
                    {
                        DataTable temp = db.Select(db.CreateQuery("SELECT COUNT (*) FROM " + cmbDataTable.Text));
                        if (temp.Rows.Count > 0)
                        {
                            int count = (int)temp.Rows[0][0];
                            if (count > 0)
                            {
                                if (rdbAppend.Checked)
                                {
                                    if (MessageBox.Show(string.Format(SharedStrings.EXISTING_TABLE_APPEND, cmbDataTable.Text, count.ToString()), "Existing Table", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.No)
                                        return;
                                }
                                else
                                {
                                    if (MessageBox.Show(string.Format(SharedStrings.EXISTING_TABLE_REPLACE, cmbDataTable.Text, count.ToString()), "Existing Table", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.No)
                                        return;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //
            }
            base.OnOK();
        }

        /// <summary>
        /// Validates user input
        /// </summary>
        /// <returns>true if ErrorMessages.Count is 0; otherwise false</returns>
        protected override bool ValidateInput()
        {
            base.ValidateInput();
            //if no variables are selected, assume ALL (*) are requested
            //if cbxAllExcept is checked, at least one variable must be selected.
            if (lbxVariables.SelectedItems.Count < 1 && cbxAllExcept.Checked)
            {
                ErrorMessages.Add(SharedStrings.NO_VARS_SELECTED);
            }

            if (String.IsNullOrEmpty(txtFileName.Text.Trim()))
            {
                ErrorMessages.Add(SharedStrings.NO_FILENAME);
            }
            //If it's not text it requires a tablename
            if (cmbOutputFormat.Text != "Text" &&
                String.IsNullOrEmpty(cmbDataTable.Text.Trim()))
            {
                ErrorMessages.Add(SharedStrings.NO_TABLE_SELECTED);
            }
            return (ErrorMessages.Count == 0);
        }

        /// <summary>
        /// Generates command text
        /// </summary>
        protected override void GenerateCommand()
        {
            WordBuilder command = new WordBuilder();
            command.Append(CommandNames.WRITE);
            command.Append((rdbReplace.Checked) ? CommandNames.REPLACE : CommandNames.APPEND);
            if (cmbOutputFormat.Text.ToUpperInvariant() == "TEXT" || cmbOutputFormat.Text.ToUpperInvariant() == "FLAT ASCII FILE")
            {
                command.Append("\"TEXT\"");
            }
            else
            {
                command.Append("\"Epi7\"");
            }
            command.Append("{" + txtFileName.Text.Trim() + "}");

            command.Append(":");
            command.Append(FieldNameNeedsBrackets(cmbDataTable.Text) ? Util.InsertInSquareBrackets(cmbDataTable.Text) : cmbDataTable.Text);

            if (lbxVariables.SelectedItems.Count < 1)
            {
                command.Append(StringLiterals.STAR);
            }
            else
            {
                if (cbxAllExcept.Checked)
                {
                    command.Append("* EXCEPT");
                }
                foreach (string s in lbxVariables.SelectedItems)
                {
                    command.Append(FieldNameNeedsBrackets(s) ? Util.InsertInSquareBrackets(s) : s);
                }
            }
            CommandText = command.ToString();
        }
        #endregion Protected Methods

        private void FormatChanged(object sender, EventArgs e)
        {
            if (cmbOutputFormat.Text.ToLowerInvariant().Contains("server"))
            {
                lblDataTable.Text = "Destination Table";
            }
            else if (cmbOutputFormat.Text.ToLowerInvariant().Contains("file"))
            {
                lblDataTable.Text = "Destination File";
            }
            else
            {
                lblDataTable.Text = "Destination Table";
            }
            txtFileName.Text = string.Empty;
            cmbDataTable.Text = string.Empty;
            cmbDataTable.Items.Clear();

            SomethingChanged(sender, e);
        }


    }
}
