using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Analysis;
using Epi;
using Epi.Windows;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;

namespace Epi.Windows.Analysis.Dialogs
{
	/// <summary>
	/// Dialog for Undefine variable command
	/// </summary>
    public partial class UndefineVariableDialog : CommandDesignDialog
	{

		#region Constructors

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public UndefineVariableDialog()
		{
			InitializeComponent();
        }

        /// <summary>
        /// Constructor for the UndefineVariable dialog
        /// </summary>
        /// <param name="frm"></param>
        public UndefineVariableDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
		{
			InitializeComponent();
            Construct();
        }
        private void Construct()
        {
            if (!this.DesignMode)
            {
                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
            }
		}
		#endregion Constructors


		#region Event Handlers
		/// <summary>
		/// Enables OK and Save Only if input validation passes
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cmbVarName_Leave(object sender, System.EventArgs e)
		{
			CheckForInputSufficiency();			
		}
		/// <summary>
		/// Clears user input
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void btnClear_Click(object sender, System.EventArgs e)
		{
			cmbVarName.Text = string.Empty;
			CheckForInputSufficiency();
		}

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://wwwn.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-undefine.html");
        }

		#endregion //Event Handlers

		#region Protected Methods
		/// <summary>
		/// Validates user input
		/// </summary>
		/// <returns></returns>
		protected override bool ValidateInput()
		{
			base.ValidateInput ();

			if (string.IsNullOrEmpty(cmbVarName.Text.Trim()))			
			{
				ErrorMessages.Add(SharedStrings.EMPTY_VARNAME);
			}
			return (ErrorMessages.Count == 0);
		}
		/// <summary>
		/// Generates command text
		/// </summary>
		protected override void GenerateCommand()
		{
            StringBuilder sb = new StringBuilder();
            sb.Append(CommandNames.UNDEFINE);
            sb.Append(StringLiterals.SPACE);
            sb.Append(FieldNameNeedsBrackets(cmbVarName.Text) ? Util.InsertInSquareBrackets(cmbVarName.Text) : cmbVarName.Text);
            CommandText = sb.ToString();
		}
		/// <summary>
		/// Sets OK and Save Only button enabled property
		/// </summary>
        public override void CheckForInputSufficiency()
		{
			bool inputValid = ValidateInput();
			btnOK.Enabled = inputValid;
			btnSaveOnly.Enabled = inputValid;
		}
		#endregion //Protected Methods

		private void UndefineVariableDialog_Load(object sender, System.EventArgs e)
		{
			LoadDefinedVariables();
		}

		#region Private Methods
		private void LoadDefinedVariables()
		{
            ////First get the list of all variables
            //// DataTable variables = GetAllVariablesAsDataTable(false, true, true, true);
            //DataTable variables = GetMemoryRegion().GetVariablesAsDataTable(
            //                                        VariableType.Standard |
            //                                        VariableType.Global |
            //                                        VariableType.Permanent);
            ////Sort the data
            //DataView dv = variables.DefaultView;
            //dv.Sort = ColumnNames.NAME;

            //cmbVarName.DataSource = dv;
            //cmbVarName.DisplayMember = ColumnNames.NAME;
            //cmbVarName.ValueMember = ColumnNames.NAME;
            VariableType scopeWord = VariableType.Standard | VariableType.Global | VariableType.Permanent;
            FillVariableCombo(cmbVarName, scopeWord);
			cmbVarName.SelectedIndex = -1;
		}
		#endregion Private Methods
	}
}

