using System;
using System.Data;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Epi;
using Epi.Windows;
using Epi.Analysis;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;

namespace Epi.Windows.Analysis.Dialogs
{
	/// <summary>
	/// Dialog for List command
	/// </summary>
    public partial class ListDialog : CommandDesignDialog
	{
		#region Constructor	

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public ListDialog()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// Constructor for the List dialog
        /// </summary>
        /// <param name="frm"></param>
        public ListDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
        {
            InitializeComponent();
            Construct();
        }

        private void Construct()
        {
            //disabling this feature for now
            this.rdbAllowUpdates.Enabled = false;

            if (!this.DesignMode)           // designer throws an error
            {
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
                this.cmbVar.Validating += new CancelEventHandler(cmbVar_Validating);
                this.cmbVar.Validated += new EventHandler(cmbVar_Validated);
                this.cmbVar.TextChanged += new EventHandler(cmbVar_TextChanged);
            }
        }

		#endregion Constructors

		#region Event Handlers
		/// <summary>
		/// Handles click event of Clear button
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void btnClear_Click(object sender, System.EventArgs e)
		{
			lbxVariables.Items.Clear();
			rdbWeb.Checked = true;
			cbxAllExcept.Checked = false;
			cmbVar.SelectedIndex = 0;
		}

        /// <summary>
		/// Handles the load event of List dialog
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void ListDialog_Load(object sender, System.EventArgs e)
		{
			LoadVariables();
			
		}
		/// <summary>
		/// Handles the SelectedIndexChanged event
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cmbVar_SelectedIndexChanged(object sender, System.EventArgs e)
		{
            this.Validate();
		}

        private void lbxVariables_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (lbxVariables.SelectedIndex != -1)
            {
                cmbVar.Items.Add(lbxVariables.Items[lbxVariables.SelectedIndex].ToString());
                lbxVariables.Items.RemoveAt(lbxVariables.SelectedIndex);
                if (lbxVariables.Items.Count < 1 && cmbVar.Text!=StringLiterals.STAR)
                {
                    btnOK.Enabled = false;
                    btnSaveOnly.Enabled = false;
                }
                else if (lbxVariables.Items.Count < 1 && cmbVar.Text == StringLiterals.STAR)
                {
                    btnOK.Enabled = true;
                    btnSaveOnly.Enabled = true;
                }
            }
        }

        private void cmbVar_Validated(object sender, EventArgs e)
        {
            if (cmbVar.SelectedItem != null && cmbVar.Text != StringLiterals.STAR)
            {
                lbxVariables.Items.Add(cmbVar.SelectedItem);
                cmbVar.Items.Remove(cmbVar.SelectedItem);
                if (lbxVariables.Items.Count > 0)
                {
                    btnOK.Enabled = true;
                    btnSaveOnly.Enabled = true;
                }
            }
        }

        private void cmbVar_Validating(object sender, CancelEventArgs e)
        {
            if (cmbVar.SelectedItem == null)
            {
                if (!String.IsNullOrEmpty(cmbVar.Text))
                {
                    e.Cancel = true;
                    cmbVar.Text = String.Empty;                    
                }
            }
        }

        private void cmbVar_TextChanged(object sender, EventArgs e)
        {
            if (cmbVar.Text == StringLiterals.STAR)
            {
                btnOK.Enabled = true;
                btnSaveOnly.Enabled = true;
            }
            else if (cmbVar.Text != StringLiterals.STAR && lbxVariables.Items.Count < 1)
            {
                btnOK.Enabled = false;
                btnSaveOnly.Enabled = false;
            }
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-list.html");
        }

        #endregion //Event Handlers

		#region Protected Methods
		/// <summary>
		/// Generates the command text
		/// </summary>
		protected override void GenerateCommand()
		{
			WordBuilder command = new WordBuilder();
			command.Append(CommandNames.LIST);
			if (lbxVariables.Items.Count > 0)
			{
				if (cbxAllExcept.Checked)
				{
					command.Append(StringLiterals.STAR);
                    command.Append(StringLiterals.SPACE);
					command.Append(CommandNames.EXCEPT);
				}

				foreach (string item in lbxVariables.Items)
				{
                    command.Append(FieldNameNeedsBrackets(item) ? Util.InsertInSquareBrackets(item) : item);
				}
			}
			else
			{
				command.Append(cmbVar.Text);
			}
            if (WinUtil.GetSelectedRadioButton(gbxDisplayMode) == rdbGrid)
			{
				command.Append(CommandNames.GRIDTABLE);
			}
            else if (WinUtil.GetSelectedRadioButton(gbxDisplayMode) == rdbAllowUpdates)
			{
				command.Append(CommandNames.UPDATE);
			}

			CommandText = command.ToString();
		}
		#endregion //Protected Methods

		#region Private Methods
		private void LoadVariables()

		{
			VariableType scopeWord = VariableType.DataSource | VariableType.DataSourceRedefined |
									 VariableType.Standard | VariableType.Global | VariableType.Permanent;

			FillVariableCombo(cmbVar, scopeWord);
            cmbVar.Items.Insert(0, "*");
			cmbVar.SelectedIndex = 0;
			//Data binding a combo box raises this event. Adding event handler here prevents this event being consumed prematurely
			this.cmbVar.SelectedIndexChanged += new System.EventHandler(this.cmbVar_SelectedIndexChanged);
			
		}
		#endregion Private Methods
	}
}
