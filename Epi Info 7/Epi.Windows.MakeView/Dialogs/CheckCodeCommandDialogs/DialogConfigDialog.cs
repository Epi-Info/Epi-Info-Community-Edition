#region Namespaces
using System.Collections;
using System.Collections.Generic;
using System;
using Epi.Windows.Controls;
using Epi.Data.Services;
using System.Data;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Epi.Windows.Dialogs;
using Epi;
using System.Text;
using EpiInfo.Plugin;

#endregion  //Namespaces

namespace Epi.Windows.MakeView.Dialogs.CheckCodeCommandDialogs
{
	/// <summary>
	/// ***** This is a wireframe and currently contains no functionality *****
	/// </summary>
    public partial class DialogConfigDialog : CheckCodeDesignDialog
	{
        #region Constructors		 
	
		/// <summary>
		/// Constructor for the class
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public DialogConfigDialog()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor of the Dialog Config dialog
        /// </summary>
        /// <param name="frm">The main form</param>
		public DialogConfigDialog(MainForm frm) : base(frm)
        {
            InitializeComponent();
        }



        /// <summary>
        /// Constructor of the Dialog Config dialog
        /// </summary>
        /// <param name="frm">The main form</param>
        public DialogConfigDialog(MainForm frm ,Project Project)
            : base(frm)
        {
            this.project = Project;
            InitializeComponent();
            Construct();
        }


        #endregion  //Constructors
        private Project project = null;
        private string multichoiceList = string.Empty;
        #region Private Event Handlers

        /// <summary>
        /// Handles the Click event of the Ok button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnOK_Click(object sender, System.EventArgs e)
		{
			if (!string.IsNullOrEmpty(txtTitle.Text) && !string.IsNullOrEmpty(txtPrompt.Text))			
			{
           //     Output = CommandNames.DIALOG + StringLiterals.SPACE + StringLiterals.DOUBLEQUOTES + txtPrompt.Text + "\"  TITLETEXT=\"" + txtTitle.Text + "\"";                
//              Output = "DIALOG \"" + txtPrompt.Text + "\"  TITLETEXT=\"" + txtTitle.Text + "\"";
                OnOK();
                this.DialogResult = DialogResult.OK;
				this.Hide();
               

			}
		}
        protected void OnOK()
        {
            if (ValidateInput() == true)
            {
                GenerateCommand();
             //   PreProcess();
                this.DialogResult = DialogResult.OK;
                this.Hide();
            }
            else
            {
                this.DialogResult = DialogResult.None;
                ShowErrorMessages();
            }
        }
        private void GenerateCommand()
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

            if (!this.rdbSimple.Checked)
            {
                sb.Append(cmbInputVar.Text).Append(StringLiterals.SPACE);  // Simple dialog
            }
            if (this.rdbGetVariable.Checked)
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
                    sb.Append(StringLiterals.DOUBLEQUOTES).Append(expression);
                    sb.Append(StringLiterals.DOUBLEQUOTES).Append(StringLiterals.SPACE);
                }
                expression = txtLength.Text;
                if (!string.IsNullOrEmpty(expression))
                {
                    sb.Append(StringLiterals.DOUBLEQUOTES).Append(expression);
                    sb.Append(StringLiterals.DOUBLEQUOTES).Append(StringLiterals.SPACE);
                }
            }
            else if (this.rdbListValues.Checked)
            {
                if (!string.IsNullOrEmpty(cmbDialogType.Text.Trim()))
                {
                    sb.Append(CommandNames.DBVALUES).Append(StringLiterals.SPACE);
                    //sb.Append(cmbDialogType.Text).Append(StringLiterals.SPACE);
                }
                sb.Append(cmbShowTable.Text).Append(StringLiterals.SPACE);
                sb.Append(cmbShowVar.Text).Append(StringLiterals.SPACE);
            }
            if (!string.IsNullOrEmpty(txtTitle.Text.Trim()))    // Title text
            {
                sb.Append(CommandNames.TITLETEXT);
                sb.Append(StringLiterals.EQUAL);
                sb.Append(StringLiterals.DOUBLEQUOTES);
                sb.Append(txtTitle.Text.Trim());
                sb.Append(StringLiterals.DOUBLEQUOTES);
            }
            Output = sb.ToString();
        }
		/// <summary>
		/// Cancel button closes this dialog 
		/// </summary>
		/// <param name="sender">Object that fired the event</param>
		/// <param name="e">.NET supplied event parameters</param>
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

        /// <summary>
        /// Handles the Text Changed event of the title
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtTitle_TextChanged(object sender, EventArgs e)
        {
            OnPromptOrTitleChanged();
        }

        /// <summary>
        /// Handles the Text Changed event of the prompt
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtPrompt_TextChanged(object sender, EventArgs e)
        {
            OnPromptOrTitleChanged();
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://wwwn.cdc.gov/epiinfo/user-guide/form-designer/how-to-Create-a-Dialog.html");
        }

        #endregion //Private Event Handlers       

        #region Private Methods

        private void OnPromptOrTitleChanged()
        {
            btnOK.Enabled = (!string.IsNullOrEmpty(txtPrompt.Text));
        }

        #endregion  //Private Methods

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtTitle.Text = string.Empty;
            txtPrompt.Text = string.Empty;
            cbxInputVariable.Text = string.Empty;
            cbxShowTable.Text = string.Empty;
            cbxShowVariable.Text = string.Empty;
            rdbSimple.Checked = true;
        }
        private void RadioButtonClick(object sender, System.EventArgs e)
        {
            if (rdbSimple.Checked)
            {
                ToggleControls(false, false);

            }
            else if (rdbGetVariable.Checked)
            {
                ToggleControls(true, false);
            }
            else if (rdbListValues.Checked)
            {
                ToggleControls(true, true);
                LoadShowTables();
            }
        }
        private void LoadShowTables()
        {
            if (project != null)
            {
                ComboBox cmb = cmbShowTable;
                if (cmb.DataSource == null)
                {
                    cmb.Items.Clear();
                    List<string> names = project.GetDataTableNames();
                    foreach (string name in names)
                    {
                        if (name != string.Empty)
                        {
                            cmb.Items.Add(name);
                        }
                    }
                    if (cmb.Items.Count > 0 && cmb.Visible)
                    {
                        cmb.SelectedIndex = 0;
                    }
                }
            }
        }
        private void ToggleControls(bool visible, bool listOfValuesVisible)
        {

            cbxInputVariable.Visible = visible;
            lblInputVariable.Visible = visible;
            cbxShowVariable.Visible = visible;
            lblShowVariable.Visible = visible;
            lblShowTable.Visible = visible;
            cbxShowTable.Visible = visible;



            //txtPrompt.Visible = visible;
            //Label2.Visible = visible;

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
            //btnSaveOnly.Enabled = !visible;
        }
        private void DialogConfigDialog_Load(object sender, EventArgs e)
        {
            ToggleControls(false, false);
            LoadInputVars(cmbInputVar);
            
        }
        private void SetVisible(bool hasSize, bool hasPattern)
        {
            txtLength.Visible = hasSize;
            lblLength.Visible = hasSize;
            cmbInputMask.Visible = hasPattern;
            lblInputMask.Visible = hasPattern;
            lblDialogType.Enabled = (hasSize);
            cmbDialogType.Enabled = (hasSize);
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
        private void cmbVarType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbVarType.SelectedIndex < 0 || rdbListValues.Checked)
            {
                return;
            }

            DataRow row = ((DataRowView)cmbVarType.SelectedItem).Row;
            bool hasSize = bool.Parse(row["HasSize"].ToString());
            bool hasPattern = bool.Parse(row["HasPattern"].ToString());
            SetVisible(hasSize, hasPattern);
            if (hasPattern)
            {
                string filter = ColumnNames.DATATYPEID + " = " + row[cmbVarType.ValueMember].ToString();
                DataRow[] masks = AppData.Instance.DataPatternsDataTable.Select(filter);
                this.cmbInputMask.DataSource = masks;
                this.cmbInputMask.DisplayMember = ColumnNames.EXPRESSION;
                this.cmbInputMask.ValueMember = ColumnNames.PATTERN_ID;
            }
            if (!hasSize)
            {
                cmbDialogType.SelectedIndex = -1;
            }
        }

        private void LoadVarTypes()
        {
            LocalizedComboBox cmb = cmbVarType;
            if (cmb.DataSource == null)
            {
                cmb.Items.Clear();
                this.cmbVarType.SelectedIndexChanged -= new System.EventHandler(this.cmbVarType_SelectedIndexChanged);
                cmb.DataSource = AppData.Instance.DataTypesDataTable.DefaultView;
                cmb.DisplayMember = ColumnNames.NAME;
                cmb.ValueMember = ColumnNames.DATATYPEID;
                cmb.SkipTranslation = false;
                cmb.SelectedIndex = 1;
                this.cmbVarType.SelectedIndexChanged += new System.EventHandler(this.cmbVarType_SelectedIndexChanged);
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
        private void SelectedIndexChanged(object sender, System.EventArgs e)
        {
            CheckForInputSufficiency();
        }
        public override void CheckForInputSufficiency()
        {
            bool inputValid = ValidateInput();
            btnOK.Enabled = inputValid;
       //     btnSaveOnly.Enabled = inputValid;
        }
        private void LoadInputVars(ComboBox cmb)
        {
            try
            {
                VariableScope scopeWord = VariableScope.Standard | VariableScope.Global | VariableScope.Permanent | VariableScope.DataSource;
              
                Epi.Windows.MakeView.Forms.MakeViewMainForm m = (Epi.Windows.MakeView.Forms.MakeViewMainForm)mainForm;
                this.EpiInterpreter = m.EpiInterpreter;

              List<EpiInfo.Plugin.IVariable> vars = this.EpiInterpreter.Context.GetVariablesInScope(scopeWord);
             

                
                if (cmb.Items.Count <= 0)
                {
                    cmb.Items.Clear();
                    foreach (EpiInfo.Plugin.IVariable var in vars)
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
            catch (Exception ex)
            {
            
            }
        }
        private void cmbShowTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadShowVars(cmbShowTable.Text);
        }

        private void LoadShowVars(string tableName)
        {
            if (project != null)
            {
                ComboBox cmb = cmbShowVar;
                cmb.Items.Clear();
                if (tableName != string.Empty)
                {
                    List<string> columnNames = project.GetTableColumnNames(tableName);
                    foreach (string name in columnNames)
                    {
                        // DEFECT #216
                        if (name != ColumnNames.REC_STATUS && name != ColumnNames.UNIQUE_KEY)
                        {
                            cmb.Items.Add(name);
                        }
                    }
                    cmbShowVar.SelectedIndex = 0;
                }
            }
        }
    }
}