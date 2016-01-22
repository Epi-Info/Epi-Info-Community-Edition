using System;
using System.Collections;
using System.Data;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using Epi.Collections;
using Epi.Analysis;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;

namespace Epi.Windows.Analysis.Dialogs
{
	/// <summary>
	/// Dialog for Display command
	/// </summary>
    public partial class DisplayDialog : CommandDesignDialog
	{
		#region Private Class Members
		private const int scaleBy = 200;
		private int startingHeight;
		#endregion //Private Class Members

		#region Constructors

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public DisplayDialog()
		{
			InitializeComponent();
        }

		/// <summary>
		/// Constructor for Display dialog
		/// </summary>
        public DisplayDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
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


		#endregion //Constructors


        #region Protected Methods

        /// <summary>
        /// Generate the DISPLAY command
        /// </summary>
        protected override void GenerateCommand()
        {
            StringBuilder sb = new StringBuilder();

            // Add Command name
            sb.Append(Epi.CommandNames.DISPLAY);
            sb.Append(StringLiterals.SPACE);
            if (rdbViews.Checked)
            {
                sb.Append(Epi.CommandNames.DBVIEWS);
                string s = txtDatabase.Text;
                if (!string.IsNullOrEmpty(s))
                {
                    sb.Append(StringLiterals.SPACE).Append(StringLiterals.SINGLEQUOTES);
                    sb.Append(s).Append(StringLiterals.SINGLEQUOTES);
                }
            }
            else if (rdbTables.Checked)
            {
                sb.Append(Epi.CommandNames.TABLES);
                string s = txtDatabase.Text;
                if (!string.IsNullOrEmpty(s))
                {
                    sb.Append(StringLiterals.SPACE).Append(StringLiterals.SINGLEQUOTES);
                    sb.Append(s).Append(StringLiterals.SINGLEQUOTES);
                }
            }
            else
            {
                sb.Append(CommandNames.DBVARIABLES);
                switch (cmbVariables.SelectedIndex)
                {
                    case 1:
                        sb.Append(StringLiterals.SPACE).Append(CommandNames.DEFINE);
                        break;
                    case 2:
                        sb.Append(StringLiterals.SPACE).Append(CommandNames.FIELDVAR);
                        break;
                    case 3:
                        sb.Append(StringLiterals.SPACE).Append(CommandNames.LIST).Append(GetSelectedVariables());
                        break;
                    default:
                        break;
                }
                if (this.cmbVariables.SelectedIndex > 3)
                {
                }
            }
            if (!string.IsNullOrEmpty(this.txtOutput.Text))
            {
                sb.Append(StringLiterals.SPACE);
                sb.Append(CommandNames.OUTTABLE).Append(StringLiterals.EQUAL).Append(txtOutput.Text);
            }

            CommandText = sb.ToString();
        }

        /// <summary>
        /// Before executing the command, preprocesses information gathered from the dialog.
        /// If the current project has changed, updates Global.CurrentProject
        /// </summary>
        protected override void PreProcess()
        {
            //base.PreProcess();
        }

        /// <summary>
        /// Checks if the input provided is sufficient and enables control buttons accordingly.
        /// </summary>
        public override void CheckForInputSufficiency()
        {
            bool inputValid = ValidateInput();
            btnOK.Enabled = inputValid;
            btnSaveOnly.Enabled = inputValid;
        }

        /// <summary>
        /// Validates user input
        /// </summary>
        /// <returns>True/False depending upon whether error messages were found</returns>
        protected override bool ValidateInput()
        {
            base.ValidateInput();

            if (this.cmbVariables.Text == "--Selected variables" && this.lbxVariables.SelectedItems.Count == 0)
            {
                ErrorMessages.Add("Please select variables to display.");
            }
            return (ErrorMessages.Count == 0);
        }
        #endregion

        #region Event Handlers

        private void btnEllipse_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files(*.mdb)|" + "*.mdb";
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                this.txtDatabase.Text = dlg.FileName;
            }
        }

        /// <summary>
		/// Sets txtRecAffected text with value selected from cmbAvailableVar
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cmbVariables_SelectedIndexChanged(object sender, System.EventArgs e)
		{
				EnableListbox(cmbVariables.SelectedIndex == 3);
		}
		/// <summary>
		/// Common event handler for radiobuttons
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void RadioButtonClick(object sender, System.EventArgs e)
		{
			if (rdbVariables.Checked)
			{
				lblFrom.Text = "&From";
				ToggleControls(false);
			}
			else
			{
				lblFrom.Text = "&Database (Blank for current)";
				ToggleControls(true);
			}
		}
		/// <summary>
		/// Clears user selection
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void btnClear_Click(object sender, System.EventArgs e)
		{
			rdbVariables.Checked = true;
			ToggleControls(false);

		}

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-display.html");
        }

		#endregion //Event Handlers

		#region Private Methods

        private string GetSelectedVariables()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in lbxVariables.SelectedItems)
            {
                sb.Append(StringLiterals.SPACE);
                sb.Append(FieldNameNeedsBrackets(s) ? Util.InsertInSquareBrackets(s) : s);
            }
            return sb.ToString();
        }

		/// <summary>
		/// Show and hide the listbox; resizing in the process
		/// </summary>
		/// <param name="enabled">Whether to enable or disable the 'Variables' listbox</param>
		private void EnableListbox(bool enabled)
		{
			int ofs;
			this.lbxVariables.Visible = enabled;
			this.lbxVariables.Enabled = enabled;
			ofs = 0;
			if (!enabled && (this.Height > startingHeight))
			{
					ofs = -scaleBy;
			}
			else if (enabled && (this.Height <= startingHeight))
			{
					ofs = scaleBy;
			}
			this.Height += ofs;
			btnOK.Top += ofs;
			btnCancel.Top += ofs;
			btnClear.Top += ofs;
			btnSaveOnly.Top += ofs;
			btnHelp.Top += ofs;			
		}

		/// <summary>
		/// Show and hide controls based on radiobutton selection
		/// </summary>
		/// <param name="visible">Boolean to determine whether controls will be visible</param>
		private void ToggleControls(bool visible)
		{
			cmbVariables.Visible = !(visible);
			txtDatabase.Visible = visible;
			btnEllipse.Visible = visible;
		}

		/// <summary>
		/// Loads Variables into the combo
		/// </summary>
		private void LoadComboVariables()
		{
			try
			{
				// Clear the list in case there's anything there
				cmbVariables.Items.Clear();
				// next add the predefineds so they'll show up first
				cmbVariables.Items.Add("--Variables currently available");
				cmbVariables.Items.Add("--Defined variables currently available");
				cmbVariables.Items.Add("--Field variables currently available");
				cmbVariables.Items.Add("--Selected variables");

                // get project reference
                Project project = this.EpiInterpreter.Context.CurrentProject;

                if (project != null)
                {
                    ViewCollection views = project.Views;
                    if (views != null)
                    {
                        foreach (View view in views)
                        {
                            cmbVariables.Items.Add(view.Name);
                        }
                    }

                    cmbVariables.SelectedIndex = 0;
                }
            }
			catch (Exception ex)
			{
				Epi.Windows.MsgBox.ShowInformation(ex.Message);
			}
			finally
			{
			}//finally

		}

        private void DisplayDialog_Load(object sender, System.EventArgs e)
        {
            VariableType scopeWord = VariableType.DataSource | VariableType.Standard | VariableType.Global | VariableType.Permanent;
            FillVariableListBox(lbxVariables, scopeWord);
            lbxVariables.SelectedIndex = -1;

            startingHeight = this.Height;

            LoadComboVariables();
            this.cmbVariables.SelectedIndexChanged += new System.EventHandler(this.cmbVariables_SelectedIndexChanged);
        }

		#endregion //Private Methods


	
	}
}

