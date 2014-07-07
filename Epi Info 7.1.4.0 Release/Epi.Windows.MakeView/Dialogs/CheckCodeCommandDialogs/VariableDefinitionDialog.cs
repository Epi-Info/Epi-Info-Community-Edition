#region Namespaces

using System;
using System.Collections;
using System.Data;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Data.Services;
using Epi.Windows.Controls;
using Epi.Windows.Dialogs;
using Epi;

#endregion  //Namespaces

namespace Epi.Windows.MakeView.Dialogs.CheckCodeCommandDialogs
{
    public partial class VariableDefinitionDialog : CheckCodeDesignDialog
    {
        #region Constructors

        /// <summary>
		/// Constructor for the class
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public VariableDefinitionDialog()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor of the Variable Definition dialog
        /// </summary>
        /// <param name="frm">The main form</param>
		public VariableDefinitionDialog(MainForm frm) : base(frm)
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();
            LoadVarTypes();
        }

        #endregion  //Constructors

        #region Private Event Handlers

        /// <summary>
        /// Handles the Click event of the OK button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnOK_Click(object sender, System.EventArgs e)
		{            
            if (ValidateInput() == true)
            {
                GenerateCommand();               
                this.DialogResult = DialogResult.OK;
                this.Hide();
            }
            else
            {
                ShowErrorMessages();
            }                 
		}

		/// <summary>
		/// Handles the Click event of the Cancel button 
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

        /// <summary>
        /// Handles the Text Changed event of the variable text box
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtVariableName_TextChanged(object sender, EventArgs e)
        {
//            btnOK.Enabled = !string.IsNullOrEmpty(txtVariableName.Text);
            CheckForInputSufficiency();
        }

        /// <summary>
        /// Clears the content of the form
        /// </summary>
        /// <param name="sender">Object that fired the object</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnClear_Click(object sender, EventArgs e)
        {
            txtVariableName.Text = string.Empty;
            cmbVarType.SelectedIndex = -1;
            cmbVarType.Text = string.Empty;
            rbStandard.Checked = true;
        }

        /// <summary>
        /// Handles the Leave event of the variable name text box
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtVariableName_Leave(object sender, EventArgs e)
        {
            CheckForInputSufficiency();
        }

        #endregion  //Private Event Handlers

        #region Private Methods

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

        /// <summary>
        /// Generates the command syntax
        /// </summary>
        private void GenerateCommand()
        {
            StringBuilder sb = new StringBuilder();
            string variableName = txtVariableName.Text.Trim();
            string variableScope = (string)WinUtil.GetSelectedRadioButton(gbxScope).Tag;
            // If the Variable Scope is the same as default, then remove it.
            if (string.Compare(variableScope, Epi.Defaults.VariableScope.ToString(), true) == 0)
            {
                variableScope = string.Empty;
            }
            sb.Append(CommandNames.DEFINE);
            sb.Append(StringLiterals.SPACE).Append(variableName);
            sb.Append(StringLiterals.SPACE).Append(variableScope);
            if (!string.IsNullOrEmpty(cmbVarType.Text))
            {
                DataRow row = ((DataRowView)cmbVarType.SelectedItem).Row;
                string expression = row[ColumnNames.EXPRESSION].ToString();
                if (!string.IsNullOrEmpty(expression))
                {
                    sb.Append(StringLiterals.SPACE).Append(expression);
                }
            }

            Output = sb.ToString();
        }            

        #endregion  //Private Methods

        #region Protected Methods
      
        /// <summary>
        /// Sets enabled property of OK
        /// </summary>
        public override void CheckForInputSufficiency()
        {
            bool inputValid = ValidateInput();
            btnOK.Enabled = inputValid;
        }  

        /// <summary>
        /// Validates user input
        /// </summary>
        /// <returns>true if ErrorMessages.Count is 0; otherwise false</returns>
        protected override bool ValidateInput()
        {
            base.ValidateInput();
            if (string.IsNullOrEmpty(txtVariableName.Text.Trim()))
            {
                ErrorMessages.Add(SharedStrings.EMPTY_VARNAME);
            }
            return (ErrorMessages.Count == 0);
        }

        #endregion  //Protected Methods       

        private void gbxScope_Enter(object sender, EventArgs e)
        {

        }
     }
}