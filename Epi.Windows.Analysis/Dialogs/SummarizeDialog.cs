using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Epi;
using Epi.Windows;
using Epi.Windows.Controls;
using System.Data;
using Epi.Analysis;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;
using Epi.Data.Services;

namespace Epi.Windows.Analysis.Dialogs
{
	/// <summary>
	/// Dialog for Summarize command
	/// </summary>
    public partial class SummarizeDialog : CommandDesignDialog
	{
		#region Private Class Members
		#endregion Private Class Members

		#region Constructor

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public SummarizeDialog()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor for the Summarize Dialog
        /// </summary>
        /// <param name="frm"></param>
        public SummarizeDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
		{
			InitializeComponent();
            Construct();
		}
		#endregion Constructors

		#region Event Handlers
		/// <summary>
		/// Clears all user input
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void btnClear_Click(object sender, System.EventArgs e)
		{
			cmbAggregate.Text = string.Empty;
			cmbVar.Text = string.Empty;
			txtIntoVar.Text = string.Empty;
			lbxVar.Items.Clear();
			cmbGroupBy.Text = string.Empty;
			lbxGroupBy.Text = string.Empty;
			cmbWeight.Text = string.Empty;
			txtOutput.Text = string.Empty;
			lbxGroupBy.Items.Clear();
			cmbWeight.SelectedIndex = -1;
			cmbAggregate.Focus();
		}
		/// <summary>
		/// Loads the aggregate combo box
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void SummarizeDialog_Load(object sender, System.EventArgs e)
		{
			LoadAggregates();
			//Localization.LocalizeComboBoxItems(cmbAggregate,false);
			LoadVariables();
		}
		/// <summary>
		/// Populate lbxvar with aggregate command each time Apply is clicked
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void btnApply_Click(object sender, System.EventArgs e)
		{
			//WordBuilder appends a space in each append and here there should be no space between two colons
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append(txtIntoVar.Text).Append(StringLiterals.SPACE);
			sb.Append(StringLiterals.COLON);
			sb.Append(StringLiterals.COLON);
			sb.Append(StringLiterals.SPACE);
			string aggregate = ((SupportedAggregate)(System.Convert.ToInt32(cmbAggregate.SelectedValue))).ToString();
			sb.Append(aggregate);
			sb.Append(Util.InsertInParantheses(cmbVar.Text));
			lbxVar.Items.Add(sb.ToString());

			cmbAggregate.Text = string.Empty ;
			cmbVar.Text = string.Empty;
			txtIntoVar.Text = string.Empty;
			btnApply.Enabled = false;
			txtOutput_TextChanged(txtOutput,e);
		}
		/// <summary>
		/// Enable OK and SaveOnly only when after aggregate variable is built and an output table name is  provided 
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void txtOutput_TextChanged(object sender, System.EventArgs e)
		{
			CheckForInputSufficiency();
		}
		/// <summary>
		/// Sets enabled property of Apply button
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void SetEnabledApply(object sender, System.EventArgs e)
		{
			if ((!string.IsNullOrEmpty(cmbAggregate.Text)) && (!string.IsNullOrEmpty(cmbVar.Text)) && (!string.IsNullOrEmpty(txtIntoVar.Text.Trim())))			
			{
				btnApply.Enabled = true;
			}
			else
			{
				btnApply.Enabled = false;
			}
		}
		/// <summary>
		/// Handles the SelectedIndexChanged event
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cmbGroupBy_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			lbxGroupBy.Items.Add(cmbGroupBy.SelectedItem.ToString());

		}
		/// <summary>
		/// Handles the DoubleClick event
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void lbxGroupBy_DoubleClick(object sender, System.EventArgs e)
		{
			if (lbxGroupBy.Items.Count > 0)
			{
				lbxGroupBy.Items.RemoveAt(lbxGroupBy.SelectedIndex);
			}
		}

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-summarize.html");
        }

		#endregion //Event Handlers

		#region Protected Methods
		/// <summary>
		/// Validates the input provided by the user
		/// </summary>
		/// <returns></returns>
		protected override bool ValidateInput()
		{
			base.ValidateInput ();
			if (lbxVar.Items.Count == 0)
			{
				ErrorMessages.Add(SharedStrings.NO_AGGREGATE_SPECIFIED);
			}
			if (string.IsNullOrEmpty(txtOutput.Text.Trim()))			
			{
				ErrorMessages.Add(SharedStrings.OUTPUT_TABLE_NOT_SPECIFIED);
			}
			return (ErrorMessages.Count == 0);
		}
		/// <summary>
		/// Generates command text 
		/// </summary>
		protected override void GenerateCommand()
		{
			WordBuilder command = new WordBuilder();
			command.Append(CommandNames.SUMMARIZE);
			//Append aggregates
			foreach (string item in lbxVar.Items) 
			{
				command.Append(item);
			}
			//Append output table
			command.Append(CommandNames.TO);
			command.Append(txtOutput.Text.Trim());

			//Append strata variables
			if (lbxGroupBy.Items.Count > 0)
			{
				command.Append(CommandNames.STRATAVAR);
				command.Append(StringLiterals.EQUAL);
				foreach (string item in lbxGroupBy.Items)
				{
					command.Append(item);
				}
			}
			//Append weighted variables
			if (cmbWeight.SelectedItem != null && !string.IsNullOrEmpty(cmbWeight.SelectedItem.ToString()))			
			{
				command.Append(CommandNames.WEIGHTVAR);
				command.Append(StringLiterals.EQUAL);
				command.Append(cmbWeight.SelectedItem.ToString());

			}
			CommandText = command.ToString();
		}
		/// <summary>
		/// Sets the enabled property of OK and SaveOnly button
		/// </summary>		
        public override void CheckForInputSufficiency()
		{
			bool inputValid = ValidateInput();
			btnOK.Enabled = inputValid;
			btnSaveOnly.Enabled = inputValid;
		}
		#endregion //Protected Methods

		#region Private Methods

        private void Construct()
        {
            if (!this.DesignMode)
            {
                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
            }
        }
        /// <summary>
		/// Attach aggregates 
		/// </summary>
		private void LoadAggregates()
		{
			LocalizedComboBox cbx = cmbAggregate;
			DataView dv = AppData.Instance.SupportedAggregatesDataTable.DefaultView;
		
			cbx.DataSource = dv;
			cbx.DisplayMember = ColumnNames.NAME;
			cbx.ValueMember = ColumnNames.ID;
			cbx.SelectedIndex = -1;
			cbx.SkipTranslation = false;
			
		}
		private void LoadVariables()
		{
			//First get the list of all variables
            // DataTable variables = GetAllVariablesAsDataTable(true, true, false, false);
            //DataTable variables = GetMemoryRegion().GetVariablesAsDataTable(
            //                                        VariableType.DataSource |
            //                                        VariableType.Standard);
            ////Sort the data
            //System.Data.DataView dv = variables.DefaultView;
            //dv.Sort = ColumnNames.NAME;
			
            //cmbVar.DataSource = dv;
            //cmbVar.DisplayMember = ColumnNames.NAME;
            //cmbVar.ValueMember = ColumnNames.NAME;
            VariableType scopeWord = VariableType.DataSource | VariableType.Standard;
            FillVariableCombo(cmbVar, scopeWord);
            cmbVar.SelectedIndex = -1;

            FillVariableCombo(cmbGroupBy, scopeWord);
            cmbGroupBy.SelectedIndex = -1;

            FillVariableCombo(cmbWeight, scopeWord);
            cmbWeight.SelectedIndex = -1;
		}
		#endregion //Private Methods
	}
}

