using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using Epi;
using Epi.Windows;
using Epi.Windows.Controls;
using Epi.Data.Services;
using VariableCollection = Epi.Collections.NamedObjectCollection<Epi.IVariable>;


namespace Epi.Windows.Analysis.Dialogs
{
	/// <summary>
	/// Dialog for Dialog command
	/// </summary>
    public partial class DialogDialog : CommandDesignDialog
	{
		#region Constructors

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public DialogDialog()
		{
			InitializeComponent();
		}
		
        /// <summary>
        /// Constructor for Dialog dialog
        /// </summary>
        /// <param name="frm">The main form</param>
		public DialogDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm): base(frm)
		{
			InitializeComponent();
            mainForm = frm;
            Construct();
        }

        /// <summary>
        /// Constructor for Dialog dialog that passes the project
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="project">The current project</param>
        public DialogDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm, Project project)
            : base(frm)
        {
            InitializeComponent();
            mainForm = frm;
            this.project = project;
            Construct();
        }

        /// <summary>
        /// Constructor for DialogDialog that takes a bool to Show btnSaveOnly
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="showSave">Boolean to denote whether to display the Save Only button on the dialog</param>
        public DialogDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm, bool showSave)
            : base(frm)
        {
            InitializeComponent();
            mainForm = frm;
            if (showSave)
            {
                showSaveOnly = true;
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
            }
            Construct();
        }

        #endregion Constructor

        #region private Attributes
        private Project project = null;
        //private MainForm mainForm = null;     dcs0 8/19/2008
        private string multichoiceList = string.Empty;
        private bool showSaveOnly = false;
        #endregion private Attributes

        #region Public Methods
        #endregion Public Methods

        #region Protected Methods
        
        /// <summary>
        /// Validates user input
        /// </summary>
        /// <returns>True/False depending upon whether error messages were generated</returns>
        protected override bool ValidateInput()
        {
            #region Preconditions
            if (!base.ValidateInput())
            {
                return false;
            }
            #endregion Preconditions

            if ((rdbGetVar.Checked) || (rdbListofValues.Checked))
			{
				if (string.IsNullOrEmpty(cmbInputVar.Text.Trim()))				
				{
					ErrorMessages.Add(SharedStrings.NO_INPUT_VARIABLE);
				}
				if (cmbVarType.SelectedIndex == 1 && string.IsNullOrEmpty(cmbDialogType.Text.Trim()))				
				{
					ErrorMessages.Add(SharedStrings.NO_DIALOG_FORMAT);
				}
				if (rdbListofValues.Checked)
				{
					if (string.IsNullOrEmpty(cmbShowVar.Text.Trim()))					
					{
						ErrorMessages.Add(SharedStrings.NO_SHOW_VARIABLE);
					}
				}
			}
			
			return (ErrorMessages.Count == 0);
		}

		/// <summary>
		/// Generates command text
		/// </summary>
		protected override void GenerateCommand()
		{
            string expression = string.Empty;
            DataRow row = null;

            StringBuilder sb = new StringBuilder();
			sb.Append(CommandNames.DIALOG);             // "DIALOG"
			sb.Append(StringLiterals.SPACE);
			
			sb.Append(StringLiterals.DOUBLEQUOTES);
			sb.Append(txtPrompt.Text.Trim());           // Prompt
			sb.Append(StringLiterals.DOUBLEQUOTES);
			sb.Append(StringLiterals.SPACE);

            if (!this.rdbSimple.Checked)                // Simple dialog
			{
                sb.Append(FieldNameNeedsBrackets(cmbInputVar.Text) ? Util.InsertInSquareBrackets(cmbInputVar.Text) : cmbInputVar.Text);
                sb.Append(StringLiterals.SPACE);  
			}
            if (this.rdbGetVar.Checked)
			{
                if (cmbDialogType.Enabled)
                {
                    row = ((DataRowView)cmbDialogType.SelectedItem).Row;
                    expression = row[ColumnNames.EXPRESSION].ToString();
                    if (expression == "<StringList>")       // StringList for multi-choice
                    {
                        sb.Append(this.multichoiceList);
                    }
                    else
                    {
                        sb.Append(expression);              // TEXTINPUT
                        expression = string.Empty;
                    }
                }
                else if (string.IsNullOrEmpty(expression))
                {
                    row = ((DataRowView)cmbVarType.SelectedItem).Row;  // DataType
                    expression = row[ColumnNames.EXPRESSION].ToString();
                    sb.Append(expression);
                }
                sb.Append(StringLiterals.SPACE);

                expression = cmbInputMask.Text;
                if (!string.IsNullOrEmpty(expression))
                {
                    sb.Append(StringLiterals.DOUBLEQUOTES);
                    sb.Append(expression);
                    sb.Append(StringLiterals.DOUBLEQUOTES);
                    sb.Append(StringLiterals.SPACE);
                }
                expression = txtLength.Text;
                if (!string.IsNullOrEmpty(expression))
                {
                    sb.Append(StringLiterals.DOUBLEQUOTES);
                    sb.Append(expression);
                    sb.Append(StringLiterals.DOUBLEQUOTES);
                    sb.Append(StringLiterals.SPACE);
                }
            }
            else if (this.rdbListofValues.Checked)
            {
                if (!string.IsNullOrEmpty(cmbDialogType.Text.Trim()))
                {
                    sb.Append(CommandNames.DBVALUES);
                    sb.Append(StringLiterals.SPACE);
                    //sb.Append(cmbDialogType.Text).Append(StringLiterals.SPACE);
                }
                sb.Append(cmbShowTable.Text);
                sb.Append(StringLiterals.SPACE);
                sb.Append(FieldNameNeedsBrackets(cmbShowVar.Text) ? Util.InsertInSquareBrackets(cmbShowVar.Text) : cmbShowVar.Text);
                sb.Append(StringLiterals.SPACE);
            }
            if (!string.IsNullOrEmpty(txtTitle.Text.Trim()))    // Title text
            {
                sb.Append(CommandNames.TITLETEXT);
                sb.Append(StringLiterals.EQUAL);
                sb.Append(StringLiterals.DOUBLEQUOTES);
                sb.Append(txtTitle.Text.Trim());
                sb.Append(StringLiterals.DOUBLEQUOTES);
            }
			CommandText = sb.ToString();
		}

		/// <summary>
		/// Sets the enabled property of OK and SaveOnly button
		/// </summary>		
        public override void CheckForInputSufficiency()
		{
			bool inputValid = ValidateInput();
			btnOK.Enabled = inputValid;
			btnSaveOnly.Enabled = inputValid ;
		}
		#endregion //Protected Methods

		#region Private Methods

        private void AlignControls()
        {
            int left = lblVarType.Left;
            lblShowTable.Left = left;
            lblShowTable.Top = lblDialogType.Top;
            cmbShowTable.Left = left;
            cmbShowTable.Top = cmbDialogType.Top;
            lblShowVar.Left = left;
            lblShowVar.Top = lblLength.Top;
            cmbShowVar.Left = left;
            cmbShowVar.Top = txtLength.Top;
            lblInputMask.Left = left;
            lblInputMask.Top = lblLength.Top;
            cmbInputMask.Left = left;
            cmbInputMask.Top = txtLength.Top;
        }

        private void Construct()
        {
            if (!this.DesignMode)
            {
                AlignControls();
                LoadVarTypes();
                LoadDialogTypes();
                LoadShowTables();
                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            }
        }

        /// <summary>
        /// Repositions buttons on dialog
        /// </summary>
        private void RepositionButtons()
        {
            int x = btnClear.Left;
            int y = btnClear.Top;
            btnClear.Location = new Point(btnCancel.Left, y);
            btnCancel.Location = new Point(btnOK.Left, y);
            btnOK.Location = new Point(btnSaveOnly.Left, y);
            btnSaveOnly.Location = new Point(x, y);
        }
        
		/// <summary>
		/// Show and hide controls based on dialog type
		/// </summary>
		/// <param name="visible">Boolean to denote whether controls should be visible or not</param>
		/// <param name="listOfValuesVisible"></param>
		private void ToggleControls(bool visible, bool listOfValuesVisible)
		{
            lblInputVar.Visible = visible;
            cmbInputVar.Visible = visible;
            lblVarType.Visible = visible;
            cmbVarType.Visible = visible;
            lblDialogType.Visible = (visible && !listOfValuesVisible);
            cmbDialogType.Visible = (visible && !listOfValuesVisible);
            lblLength.Visible = (visible && !listOfValuesVisible);
            txtLength.Visible = (visible && !listOfValuesVisible);
            lblShowTable.Visible = (visible && listOfValuesVisible);
            cmbShowTable.Visible = (visible && listOfValuesVisible);
            lblShowVar.Visible = (visible && listOfValuesVisible);
            cmbShowVar.Visible = (visible && listOfValuesVisible);
			btnOK.Enabled = !visible;
			btnSaveOnly.Enabled = !visible;
		}


        /// <summary>
        /// Loads the InputVar combo
        /// </summary>
        private void LoadInputVars(ComboBox cmb)
        {
            VariableType scopeWord = VariableType.Standard | VariableType.Global | VariableType.Permanent | VariableType.DataSource;
            VariableCollection vars = GetMemoryRegion().GetVariablesInScope(scopeWord);
            if (cmb.Items.Count <= 0)
            {
                cmb.Items.Clear();
                foreach (IVariable var in vars)
                {
                    if (var.Name != ColumnNames.REC_STATUS && var.Name != ColumnNames.UNIQUE_KEY)
                    {
                        cmb.Items.Add(var.Name.ToString());
                    }
                }
                // TODO - will need to put variable type somewhere
                //cmb.DataSource = GetDefinedVars().DefaultView;
                //cmb.DisplayMember = ColumnNames.NAME;
                //cmb.ValueMember = ColumnNames.DATA_TYPE;
                cmb.SelectedIndex = -1;
            }
        }
        /// <summary>
        /// Loads the Variable Type combo box
        /// </summary>
        private void LoadVarTypes()
        {
            LocalizedComboBox cmb = cmbVarType;
            if (cmb.DataSource == null)
            {
                cmb.Items.Clear();
                this.cmbVarType.SelectedIndexChanged -= new System.EventHandler( this.cmbVarType_SelectedIndexChanged );
                cmb.DataSource = AppData.Instance.DataTypesDataTable.DefaultView;
                cmb.DisplayMember = ColumnNames.NAME;
                cmb.ValueMember = ColumnNames.DATATYPEID;
                cmb.SkipTranslation = false;
                cmb.SelectedIndex = 1;
                this.cmbVarType.SelectedIndexChanged += new System.EventHandler( this.cmbVarType_SelectedIndexChanged );
            }
            //Localization.LocalizeComboBoxItems(cmb,false);

        }
        /// <summary>
		/// Loads the Dialog Type combo box
		/// </summary>
		private void LoadDialogTypes()
		{
			LocalizedComboBox cmb = cmbDialogType;
            if (cmb.DataSource == null)
            {
                cmb.Items.Clear();
                cmb.DataSource = AppData.Instance.DialogFormatsDataTable.DefaultView;
                cmb.DisplayMember = ColumnNames.NAME;
                cmb.ValueMember = ColumnNames.ID;
                cmb.SkipTranslation = false;
                cmb.SelectedIndex = 0;
                //Localization.LocalizeComboBoxItems(cmb,false);
            }
		}

        /// <summary>
        /// Loads the ShowTable combo
        /// </summary>
        private void LoadShowTables()
        {
            if (project != null)
            {
                ComboBox cmb = cmbShowTable;
                if (cmb.DataSource == null)
                {
                    cmb.Items.Clear();
                    List<string> names = project.GetDataTableNames();
                    List<string> codeTableNames = project.GetCodeTableNames();

                    IEnumerable<string> union = names.Union(codeTableNames);

                    names = union.ToList<string>();
                    names.Sort();
                    names.Remove(string.Empty);
                    cmb.Items.AddRange(names.ToArray());

                    if (cmb.Items.Count > 0 && cmb.Visible)
                    {
                        cmb.SelectedIndex = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Loads the ShowVar combo
        /// </summary>
        private void LoadShowVars(string tableName)
        {
            if (project != null)
            {
                ComboBox cmb = cmbShowVar;
                cmb.Items.Clear();
                if (tableName != string.Empty)
                {
                    View viewCandidate = project.Metadata.GetViewByFullName(tableName);

                    if (viewCandidate != null)
                    {
                        tableName = tableName + viewCandidate.Id;
                    }

                    List<string> columnNames = project.GetTableColumnNames(tableName);
                    foreach (string name in columnNames)
                    {
                        cmb.Items.Add(name);
                    }
                    cmbShowVar.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        /// Gets the list of selections for the combo in the MultiSelect dialog
        /// </summary>
        /// <returns> List of selections as comma-separated string </returns>
        /// 
        private string GetSelectionList()
        {
            string selections = string.Empty;
            DialogListDialog dlg = new DialogListDialog(mainForm);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                selections = dlg.Selections;
            }
            return selections;

        }



		#endregion //Private Methods

		#region Event Handlers

        /// <summary>
        /// gets a reference to the current project
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void DialogDialog_Load(object sender, EventArgs e)
        {
            // get project reference
            
            project = this.EpiInterpreter.Context.CurrentProject;

            btnSaveOnly.Visible = showSaveOnly;
            if (showSaveOnly)
            {
                RepositionButtons();
            }
            LoadInputVars(cmbInputVar);
        }

		/// <summary>
		/// Clears all user input
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void btnClear_Click(object sender, System.EventArgs e)
		{
			txtPrompt.Text = string.Empty;
			txtTitle.Text = string.Empty;
			rdbSimple.Checked = true;
			ToggleControls(false,false);
		}
		/// <summary>
		/// Common event handler for radiobuttons click 
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void RadioButtonClick(object sender, System.EventArgs e)
		{
			if (rdbSimple.Checked)
			{
				ToggleControls(false,false);
				
			}
			else if (rdbGetVar.Checked)
			{
				ToggleControls(true,false);
			}
			else if (rdbListofValues.Checked)
			{
				ToggleControls(true,true);
                LoadShowTables();
			}
		}

        /// <summary>
        /// Handles SelectedIndexChanged event of Dialog Type combo
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbDialogType_SelectedIndexChanged(object sender, EventArgs e)
        {
            LocalizedComboBox cmb = cmbDialogType;
            multichoiceList = string.Empty;
            txtLength.Enabled = (cmb.SelectedIndex < 1);
            lblLength.Enabled = (cmb.SelectedIndex < 1);
            if (cmb.DataSource != null && cmbDialogType.SelectedIndex > 0)
            {
                CheckForInputSufficiency();
                DataRow row = ((DataRowView)cmb.SelectedItem).Row;
                if (string.Compare(row[ColumnNames.EXPRESSION].ToString(), "<StringList>") == 0)
                {
                    multichoiceList = GetSelectionList();
                }
            }

        }
        /// <summary>
		/// Handles SelectedIndexChanged event of most combo boxes
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void SelectedIndexChanged(object sender, System.EventArgs e)
		{
			CheckForInputSufficiency();			
		}

        private void cmbShowTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadShowVars(cmbShowTable.Text);
        }

        private void SetVisible( bool hasSize, bool hasPattern )
        {
            txtLength.Visible = hasSize;
            lblLength.Visible = hasSize;
            cmbInputMask.Visible = hasPattern;
            lblInputMask.Visible = hasPattern;
            lblDialogType.Enabled = (hasSize);
            cmbDialogType.Enabled = (hasSize);
        }

        private void cmbVarType_SelectedIndexChanged( object sender, EventArgs e )
        {
            if (cmbVarType.SelectedIndex < 0 || rdbListofValues.Checked)
            {
                return;
            }

            DataRow row = ((DataRowView)cmbVarType.SelectedItem).Row;
            bool hasSize = bool.Parse( row["HasSize"].ToString() );
            bool hasPattern = bool.Parse( row["HasPattern"].ToString() );
            SetVisible(hasSize, hasPattern);
            if (hasPattern)
            {
                string filter = ColumnNames.DATATYPEID + " = " + row[cmbVarType.ValueMember].ToString();
                DataRow[] masks = AppData.Instance.DataPatternsDataTable.Select( filter );
                this.cmbInputMask.DataSource = masks;
                this.cmbInputMask.DisplayMember = ColumnNames.EXPRESSION;
                this.cmbInputMask.ValueMember = ColumnNames.PATTERN_ID;
            }
            if (!hasSize)
            {
                cmbDialogType.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-dialog.html");
        }

        #endregion //Event Handlers




	}
}