#region Namespaces

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Windows.Forms;
using Epi;
using Epi.Data.Services;
using Epi.Windows.Controls;

#endregion  //Namespaces

namespace Epi.Windows.Analysis.Dialogs
{
	/// <summary>
	/// Dialog for Define Variable command
	/// </summary>
    public partial class DefineVariableDialog : CommandDesignDialog
	{
		#region Constructors

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public DefineVariableDialog()
		{
			InitializeComponent();
            //if (!this.DesignMode)           // designer throws an error
            //{
            //    this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            //    this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
            //}
		}

		/// <summary>
		/// Constructor for DefineVariable dialog
		/// </summary>
        public DefineVariableDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
		{
			InitializeComponent();
            Construct();
		}

        /// <summary>
        /// Oveloaded constructor for DefineVariableDialog which
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="showSave">Boolean to denote whether to show the Save Only button on the dialog</param>
        public DefineVariableDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm, bool showSave)
            : base(frm)
        {
            InitializeComponent();
            if (showSave)
            {
                showSaveOnly = true;
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
            }
            Construct();
        }
		#endregion Constructors

        #region Private Attributes
        private bool showSaveOnly = false;
        #endregion Private Attributes

        #region Public Methods
        #endregion Public Methods

        #region Private Methods
        private void Construct()
        {
            if (!this.DesignMode)
            {
                LoadVarTypes();
                this.btnOK.Click += new System.EventHandler( this.btnOK_Click );
            }
        }

        /// <summary>
        /// Reposition buttons on dialog
        /// </summary>
        private void RepositionButtons()
        {
            if (btnSaveOnly.Visible)
            {
                int x = btnClear.Left;
                int y = btnClear.Top;
                btnClear.Location = new Point(btnCancel.Left, y);
                btnCancel.Location = new Point(btnOK.Left, y);
                btnOK.Location = new Point(btnSaveOnly.Left, y);
                btnSaveOnly.Location = new Point(x, y);
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
                cmb.DataSource = AppData.Instance.DataTypesDataTable.DefaultView;
                cmb.DisplayMember = ColumnNames.NAME;
                cmb.ValueMember = ColumnNames.DATATYPEID;
                cmb.SkipTranslation = false;
                cmb.SelectedIndex = -1;
            }
            //Localization.LocalizeComboBoxItems(cmb,false);

        }

        #endregion  //Private Methods

        #region Event Handlers
        /// <summary>
		/// Clears user input
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void btnClear_Click(object sender, System.EventArgs e)
		{
			txtVarName.Text = string.Empty;
            cmbVarType.SelectedIndex = -1;
            cmbVarType.SelectedIndex = -1;
            txtDLLObjectDef.Text = string.Empty;
            rdbStandard.Checked = true;
            this.cmbVarType.SelectedItem = null;
		}

		/// <summary>
		/// Enables OK and Save Only if validation passes
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void txtVarName_Leave(object sender, System.EventArgs e)
		{
            CheckForInputSufficiency();
        }

        /// <summary>
        /// Loads the dialog
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void DefineVariableDialog_Load(object sender, EventArgs e)
        {
            btnSaveOnly.Visible = showSaveOnly;
            if (showSaveOnly)
            {
                RepositionButtons();
            }
        }

        /// <summary>
        /// Handles the Text Changed event of the variable name textbox
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtVarName_TextChanged(object sender, EventArgs e)
        {
            CheckForInputSufficiency();	
        }

        private void cmbVarType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbVarType.Text.Equals("Object"))
            {
                lblDLLObjectDef.Enabled = true;
                txtDLLObjectDef.Enabled = true;
            }
            else
            {
                txtDLLObjectDef.Text = string.Empty;
                lblDLLObjectDef.Enabled = false;
                txtDLLObjectDef.Enabled = false;
            }
            CheckForInputSufficiency();
        }

        private void txtDLLObjectDef_TextChanged(object sender, EventArgs e)
        {
            CheckForInputSufficiency();
        }


        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-define.html");
        }

        #endregion Event Handlers

		#region Protected Methods
		/// <summary>
		/// Validates user input
		/// </summary>
		/// <returns>true if ErrorMessages.Count is 0; otherwise false</returns>
		protected override bool ValidateInput()
		{
			base.ValidateInput();
            lblVarNameErr.Text = string.Empty;
            lblVarNameErr.Visible = false;
            if (string.IsNullOrEmpty(txtVarName.Text.Trim()))			
			{
				ErrorMessages.Add(SharedStrings.EMPTY_VARNAME);
			}
            else
            {
                string strTestForSymbols = txtVarName.Text;
                Regex regex = new Regex("[\\w\\d]", RegexOptions.IgnoreCase);
                string strResultOfSymbolTest = regex.Replace(strTestForSymbols, string.Empty);
                string strMessage = string.Empty;
                if (strResultOfSymbolTest.Length > 0)
                {
                    strMessage = string.Format(SharedStrings.INVALID_CHARS_IN_VAR_NAME, strResultOfSymbolTest);
                    ErrorMessages.Add(strMessage);
                    lblVarNameErr.Text = strMessage;
                    lblVarNameErr.Visible = true;
                }
                if (AppData.Instance.IsReservedWord(txtVarName.Text.Trim()))
                {
                    strMessage = SharedStrings.VAR_NAME_IS_RESERVED;
                    ErrorMessages.Add(strMessage);
                    lblVarNameErr.Text = strMessage;
                    lblVarNameErr.Visible = true;
                }
                
                //ToDo: Check for duplicate variable names. 
                //Something like done in the Rule_Define.cs with this.Context.MemoryRegion.IsVariableInScope(txtVarName.Text)

                if (string.IsNullOrEmpty(txtDLLObjectDef.Text.Trim()) && cmbVarType.Text.Equals("Object"))
                {
                    ErrorMessages.Add(SharedStrings.EMPTY_DLL_DEF);
                }

            }
            return (ErrorMessages.Count == 0);
		}
		/// <summary>
		/// Generates command text
		/// </summary>
		protected override void GenerateCommand()
		{
			StringBuilder sb = new StringBuilder();
			string variableName = txtVarName.Text.Trim() ;
            string variableScope = (string)WinUtil.GetSelectedRadioButton(gbxScope).Tag;
			// If the Variable Scope is the same as default, then remove it.
			 if (string.Compare(variableScope, Epi.Defaults.VariableScope.ToString(), true) == 0)
			 {
			 	variableScope = string.Empty;
			 }
             sb.Append(CommandNames.DEFINE).Append(StringLiterals.SPACE);
            sb.Append(variableName).Append(StringLiterals.SPACE);
            if (!string.IsNullOrEmpty(variableScope))
            {
                sb.Append(variableScope).Append(StringLiterals.SPACE);
            }
            if (!string.IsNullOrEmpty(cmbVarType.Text))
            {
                DataRow row = ((DataRowView)cmbVarType.SelectedItem).Row;
                string expression = row[ColumnNames.EXPRESSION].ToString();
                if (!string.IsNullOrEmpty(expression))
                {
                    sb.Append(expression).Append(StringLiterals.SPACE);
                }

            }
            if (!string.IsNullOrEmpty(txtDLLObjectDef.Text.Trim()))
            {
                sb.Append(StringLiterals.DOUBLEQUOTES).Append(txtDLLObjectDef.Text.Trim()).Append(StringLiterals.DOUBLEQUOTES);
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
		#endregion Protected Methods       

        private void txtVarName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                e.SuppressKeyPress = true;
            }
        }
    }
}