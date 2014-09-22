using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using Epi.Analysis;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;

namespace Epi.Windows.Analysis.Dialogs
{
	/// <summary>
	/// Dialog for Match command
	/// </summary>
    public partial class MatchDialog : CommandDesignDialog
	{
		#region Constructor	

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public MatchDialog()
		{
			InitializeComponent();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frm"></param>
        public MatchDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
		{
			InitializeComponent();
            Construct();
		}
		#endregion Constructors

        #region Private Properties

        string strExposure = "";
        string strOutcome = "";
        string strWeight = "";

        #endregion Private Properties


        #region Private Methods

        private void Construct()
        {
            if (!this.DesignMode)
            {
                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
            }
        }
        #endregion Private Methods

        #region Event Handlers
        private void btnClear_Click(object sender, System.EventArgs e)
        {
            cmbOutcomeVar.Items.Clear();
            cmbExpVar.Items.Clear();
            cmbWeight.Items.Clear();
            lbxMatchVars.Items.Clear();
            MatchDialog_Load(this, null);
        }

        /// <summary>
        /// Event handler for some unimportant that has changed so that OK button can be conditioned
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void SomethingChanged(object sender, EventArgs e)
        {
            CheckForInputSufficiency();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbOutcomeVar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbOutcomeVar.SelectedIndex >= 0)
            {
                //Get the old variable chosen that was saved to the txt string.
                string strOld = strOutcome;
                string strNew = cmbOutcomeVar.Text;

                //make sure it isn't "" or the same as the new variable picked.
                if ((strOld.Length != 0) && (strOld != strNew))
                {
                    //add the former variable chosen back into the other lists
                    //cmbOutcomeVar.Items.Add(strOld);
                    cmbExpVar.Items.Add(strOld);
                    cmbMatchVar.Items.Add(strOld);
                    cmbWeight.Items.Add(strOld);
                }

                //record the new variable and remove it from the other lists
                strOutcome = strNew;
                //cmbOutcomeVar.Items.Remove(strNew);
                cmbExpVar.Items.Remove(strNew);
                cmbMatchVar.Items.Remove(strNew);
                cmbWeight.Items.Remove(strNew);
            }

            CheckForInputSufficiency();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbExpVar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbExpVar.SelectedIndex >= 0)
            {
                //Get the old variable chosen that was saved to the txt string.
                string strOld = strExposure;
                string strNew = cmbExpVar.Text;

                //make sure it isn't "" or the same as the new variable picked.
                if ((strOld.Length != 0) && (strOld != strNew))
                {
                    //add the former variable chosen back into the other lists
                    cmbOutcomeVar.Items.Add(strOld);
                    //cmbExpVar.Items.Add(strOld);
                    cmbMatchVar.Items.Add(strOld);
                    cmbWeight.Items.Add(strOld);
                }

                //record the new variable and remove it from the other lists
                strExposure = strNew;
                cmbOutcomeVar.Items.Remove(strNew);
                //cmbExpVar.Items.Remove(strNew);
                cmbMatchVar.Items.Remove(strNew);
                cmbWeight.Items.Remove(strNew);
            }
            CheckForInputSufficiency();
        }

        /// <summary>
        /// Handles the Attach Event for the form, fills all of the dialogs with a list of variables.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void MatchDialog_Load(object sender, EventArgs e)
        {
            VariableType scopeWord = VariableType.DataSource | VariableType.DataSourceRedefined |
                                     VariableType.Standard | VariableType.Global | VariableType.Permanent;
            FillVariableCombo(cmbOutcomeVar, scopeWord);
            cmbOutcomeVar.SelectedIndex = -1;
            FillVariableCombo(cmbExpVar, scopeWord);
            cmbExpVar.SelectedIndex = -1;
            FillVariableCombo(cmbMatchVar, scopeWord);
            cmbExpVar.SelectedIndex = -1;
            FillVariableCombo(cmbWeight, scopeWord);
            cmbWeight.SelectedIndex = -1;
        }

        /// <summary>
        /// Handles the Selected Index Change event for lbxMatchVars.  
        /// Moves the variable name back to the comboboxes
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void lbxMatchVars_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbxMatchVars.SelectedIndex >= 0)   // prevent the remove below from re-entering
            {
                string s = this.lbxMatchVars.SelectedItem.ToString();
                lbxMatchVars.Items.Remove(s);
                //remove square brackets before adding back to DDLs
                char[] cTrimParens = { '[', ']' };
                s = s.Trim(cTrimParens);
                cmbExpVar.Items.Add(s);
                cmbOutcomeVar.Items.Add(s);
                cmbWeight.Items.Add(s);
                cmbMatchVar.Items.Add(s);
                CheckForInputSufficiency();
            }
        }
        private void cmbMatchVar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbMatchVar.Text != string.Empty)
            {
                string s = cmbMatchVar.Text;
                lbxMatchVars.Items.Add(FieldNameNeedsBrackets(s) ? Util.InsertInSquareBrackets(s) : s);
                cmbExpVar.Items.Remove(s);
                cmbOutcomeVar.Items.Remove(s);
                cmbMatchVar.Items.Remove(s);
                cmbWeight.Items.Remove(s);
                CheckForInputSufficiency();
            }

        }

        #endregion //Event Handlers

        #region Protected Methods

        /// <summary>
        /// Validates user input
        /// </summary>
        /// <returns>true if there is no error; else false</returns>
        protected override bool ValidateInput()
        {
            base.ValidateInput();
            if (string.IsNullOrEmpty(cmbOutcomeVar.Text.Trim()))
            {
                ErrorMessages.Add(SharedStrings.EMPTY_VARNAME);
            }
            if (string.IsNullOrEmpty(this.cmbExpVar.Text.Trim()))
            {
                ErrorMessages.Add(SharedStrings.EMPTY_VARNAME);
            }
            if (this.cbxMatch.Checked && lbxMatchVars.Items.Count < 1)
            {
                ErrorMessages.Add(SharedStrings.EMPTY_VARNAME);
            }
            return (ErrorMessages.Count == 0);
        }

        /// <summary>
        /// Generates user command
        /// </summary>
        protected override void GenerateCommand()
        {
            StringBuilder sb = new StringBuilder();

            if (cbxMatch.Checked)
            {
                sb.Append(CommandNames.MATCH);
            }
            else
            {
                sb.Append(CommandNames.TABLES);
            }
            sb.Append(StringLiterals.SPACE);
            sb.Append(cmbExpVar.Text);
            sb.Append(StringLiterals.SPACE);
            sb.Append(FieldNameNeedsBrackets(cmbOutcomeVar.Text) ? Util.InsertInSquareBrackets(cmbOutcomeVar.Text) : cmbOutcomeVar.Text);
            sb.Append(StringLiterals.SPACE);
            if (cbxMatch.Checked)
            {
                sb.Append(CommandNames.MATCHVAR);
                sb.Append(StringLiterals.EQUAL);
                foreach (string s in lbxMatchVars.Items)
                {
                    sb.Append(s);
                    sb.Append(StringLiterals.SPACE);
                }
            }
            else if (lbxMatchVars.Items.Count > 0)
            {
                sb.Append(CommandNames.STRATAVAR);
                sb.Append(StringLiterals.EQUAL);
                foreach (string s in this.lbxMatchVars.Items)
                {
                    sb.Append(s);
                    sb.Append(StringLiterals.SPACE);
                }
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

    }
}