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
	/// Dialog for Tables command
	/// </summary>
    public partial class LogisticRegressionDialog : CommandDesignDialog
	{
		#region Constructor	

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public LogisticRegressionDialog()
		{
			InitializeComponent();
            Construct();
		}

        /// <summary>
        /// LinearRegressionDialog constructor
        /// </summary>
        /// <param name="frm"></param>
        public LogisticRegressionDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
		{
			InitializeComponent();
            Construct();
		}
		#endregion Constructors

        #region Private Properties
        private string txtOutcomeVar = String.Empty;
        private string txtOtherVar = String.Empty;
        private string txtMatchVar = String.Empty;
        private string txtWeightVar = String.Empty;

        #endregion Private Properties

        #region Private Methods

        private void Construct()
        {
            if (!this.DesignMode)
            {
                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
                this.btnModifyTerm.Click += new System.EventHandler(this.btnModifyTerm_Click);
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
            }
        }

        private void LinearRegressionDialog_Load( object sender, EventArgs e )
        {
            VariableType scopeWord = VariableType.DataSource | VariableType.DataSourceRedefined |
                                        VariableType.Standard;
            FillVariableCombo(cmbOther, scopeWord);
            FillVariableCombo(cmbOutcome, scopeWord);            
            FillVariableCombo(cmbWeight, scopeWord);
            FillVariableCombo(cmbMatch, scopeWord);

            cmbConfLimits.Items.Add("");
            cmbConfLimits.Items.Add("90%");
            cmbConfLimits.Items.Add("95%");
            cmbConfLimits.Items.Add("99%");

            //Initialize or clear the DDL variable filters.
            txtOutcomeVar = "";
            txtOtherVar = "";
            txtMatchVar = "";
            txtWeightVar = "";
        }


        #endregion Private Methods

        #region Public Methods
        /// <summary>
        /// Sets enabled property of OK and Save Only
        /// </summary>
        public override void CheckForInputSufficiency()
        {
            bool inputValid = ValidateInput();
            btnOK.Enabled = inputValid;
            btnSaveOnly.Enabled = inputValid;
        }
        #endregion Public Methods

        #region Protected Methods
        /// <summary>
        /// Generates the command text
        /// </summary>
        protected override void GenerateCommand()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(CommandNames.LOGISTIC);
            sb.Append(StringLiterals.SPACE);
            sb.Append(FieldNameNeedsBrackets(cmbOutcome.Text) ? Util.InsertInSquareBrackets(cmbOutcome.Text) : cmbOutcome.Text);
            sb.Append(StringLiterals.EQUAL);
            foreach (object obj in lbxOther.Items)
            {
                sb.Append(obj.ToString());
                sb.Append(StringLiterals.SPACE);
            }

            foreach (object obj in lbxInteractionTerms.Items)
            {
                sb.Append(obj.ToString());
                sb.Append(StringLiterals.SPACE);
            }

            if (!string.IsNullOrEmpty(cmbMatch.Text))
            {
                sb.Append(CommandNames.MATCHVAR);
                sb.Append(StringLiterals.EQUAL);
                sb.Append(FieldNameNeedsBrackets(cmbMatch.Text) ? Util.InsertInSquareBrackets(cmbMatch.Text) : cmbMatch.Text);
                sb.Append(StringLiterals.SPACE);
            }

            if (checkboxNoIntercept.Checked == true)
            {
                sb.Append(CommandNames.NOINTERCEPT);
                sb.Append(StringLiterals.SPACE);
            }

            if (!string.IsNullOrEmpty(cmbWeight.Text))
            {
                sb.Append(CommandNames.WEIGHTVAR);
                sb.Append(StringLiterals.EQUAL);
                sb.Append(FieldNameNeedsBrackets(cmbWeight.Text) ? Util.InsertInSquareBrackets(cmbWeight.Text) : cmbWeight.Text);
                sb.Append(StringLiterals.SPACE);
            }

            if (!string.IsNullOrEmpty(txtOutput.Text))
            {
                sb.Append(CommandNames.OUTTABLE);
                sb.Append(StringLiterals.EQUAL);
                sb.Append(txtOutput.Text);
                sb.Append(StringLiterals.SPACE);
            }

            if (!string.IsNullOrEmpty(cmbConfLimits.Text))
            {
                sb.Append(CommandNames.PVALUE);
                sb.Append(StringLiterals.EQUAL);
                sb.Append(cmbConfLimits.Text);                
            }            
            CommandText = sb.ToString();
        }
        /// <summary>
        /// Validates user input
        /// </summary>
        /// <returns>true if there is no error; else false</returns>
        protected override bool ValidateInput()
        {            
            base.ValidateInput();
            if (lbxOther.Items.Count == 0)
            {
                ErrorMessages.Add( SharedStrings.MUST_SELECT_TERMS );
            }
            if (!(cmbOutcome.Text.Length > 0))
            {
                ErrorMessages.Add( SharedStrings.MUST_SELECT_OUTCOME );
            }
            //// cmbStratifyby doubles as cmbMatchVar (which doesn't exist)
            //if (cbxMatch.Checked && string.IsNullOrEmpty(cmbStratifyBy.ToString()))
            //{
            //    ErrorMessages.Add( SharedStrings.NO_MATCHVAR);
            //}

            return (ErrorMessages.Count == 0);
        }

        #endregion Protected Methods

        #region Event Handlers
        private void btnClear_Click(object sender, System.EventArgs e)
        {
            //DDLs are cleared in the dialog_Load event
            txtOutput.Text = string.Empty;
            cmbConfLimits.Items.Clear();
            lbxOther.Items.Clear();
            lbxInteractionTerms.Items.Clear();

            LinearRegressionDialog_Load(this, null);
            CheckForInputSufficiency();
        }

        private void lbxOther_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbxOther.SelectedItems.Count >= 1 && lbxOther.SelectedItems.Count <= 2)
            {
                btnModifyTerm.Enabled = true;
            }
            else
            {
                btnModifyTerm.Enabled = false;
            }

            if (lbxOther.SelectedItems.Count == 2)
            {
                btnModifyTerm.Text = "Make Interaction";
            }
            else
            {
                btnModifyTerm.Text = "Make Dummy";

                if (lbxOther.SelectedItems.Count == 1 && lbxOther.SelectedItem.ToString().Contains("("))
                {
                    btnModifyTerm.Text = "Make Continuous";
                }
            }
        }

        /// <summary>
        /// Handles the cmbOther SelectedIndexChanged event
        /// </summary>
        /// <remarks>Removes item from other comboboxes</remarks>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbOther_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbOther.SelectedIndex >= 0)   // prevent the remove below from re-entering
            {
                string s = cmbOther.Text.ToString();
                if (s.Length != 0)
                {
                    lbxOther.Items.Add(FieldNameNeedsBrackets(s) ? Util.InsertInSquareBrackets(s) : s);
                    cmbOther.Items.Remove(s);
                    cmbOutcome.Items.Remove(s);
                    cmbMatch.Items.Remove(s);
                    cmbWeight.Items.Remove(s);

                    CheckForInputSufficiency();
                }
            }
        }

        private void cmbOutcome_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbOutcome.SelectedIndex >= 0)   // prevent the remove below from re-entering
            {
                //Get the old variable chosen that was saved to the txt string.
                string strOld = txtOutcomeVar;
                string strNew = cmbOutcome.Text;

                //make sure it isn't "" or the same as the new variable picked.
                if ((strOld.Length != 0) && (strOld != strNew))
                {
                    //add the former variable chosen back into the other lists
                    //cmbOutcome.Items.Add(strOld);
                    cmbOther.Items.Add(strOld);
                    cmbMatch.Items.Add(strOld);
                    cmbWeight.Items.Add(strOld);
                }

                //record the new variable and remove it from the other lists
                txtOutcomeVar = strNew;
                //cmbOutcome.Items.Remove(strNew);
                cmbOther.Items.Remove(strNew);
                cmbMatch.Items.Remove(strNew);
                cmbWeight.Items.Remove(strNew);

                CheckForInputSufficiency();
            }
        }
        
        private void btnModifyTerm_Click(object sender, EventArgs e)
        {
            if (lbxOther.SelectedItems.Count == 1 && !lbxOther.SelectedItem.ToString().Contains("("))
            {
                string s = lbxOther.SelectedItem.ToString();
                lbxOther.Items.Remove(s);
                lbxOther.Items.Add("(" + s + ")");
            }
            else if (lbxOther.SelectedItems.Count == 1 && lbxOther.SelectedItem.ToString().Contains("(")) 
            {
                string s = lbxOther.SelectedItem.ToString();
                lbxOther.Items.Remove(s);
                lbxOther.Items.Add(s.Replace("(", string.Empty).Replace(")", string.Empty));
            }
            else if (lbxOther.SelectedItems.Count == 2)
            {
                string interactionTerm = string.Empty;
                if (!(lbxOther.SelectedItems[0].ToString().Contains("(") || (lbxOther.SelectedItems[1].ToString().Contains("("))))
                {
                    lbxInteractionTerms.Items.Add(lbxOther.SelectedItems[0].ToString() + StringLiterals.STAR + lbxOther.SelectedItems[1].ToString());
                    lbxOther.SelectedItems.Clear();
                }
            }
        }

        private void lbxOther_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                while (lbxOther.SelectedItems.Count > 0) 
                {
                    string s = lbxOther.SelectedItem.ToString();
                    lbxOther.Items.Remove(s);
                    //if Dummy variable, remove parentheses from var before adding back to DDLs
                    char[] cTrimParens = { '(', ')', '[', ']' };
                    s = s.Trim(cTrimParens);
                    cmbOutcome.Items.Add(s);
                    cmbOther.Items.Add(s);
                    cmbMatch.Items.Add(s);
                    cmbWeight.Items.Add(s);

                    CheckForInputSufficiency();
                }
            }
        }

        private void cmbMatch_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Get the old variable chosen that was saved to the txt string.
            string strOld = txtMatchVar;
            string strNew = cmbMatch.Text;

            //make sure it isn't "" or the same as the new variable picked.
            if ((strOld.Length != 0) && (strOld != strNew))
            {
                //add the former variable chosen back into the other lists
                cmbOutcome.Items.Add(strOld);
                cmbOther.Items.Add(strOld);
                //cmbMatch.Items.Add(strOld);
                cmbWeight.Items.Add(strOld);
            }

            //record the new variable and remove it from the other lists
            txtMatchVar = strNew;
            cmbOutcome.Items.Remove(strNew);
            cmbOther.Items.Remove(strNew);
            //cmbMatch.Items.Remove(strNew);
            cmbWeight.Items.Remove(strNew);
        }

        private void cmbMatch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                string s = cmbMatch.Text.ToString();
                //cmbOutcome.Items.Add(s);
                //cmbOther.Items.Add(s);
                //cmbMatch.Items.Add(s);
                //SelectedIndexChanged will add the var back to the other DDLs
                cmbMatch.Text = "";
                cmbMatch.SelectedIndex = -1;
            }
        }


        private void cmbWeight_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Get the old variable chosen that was saved to the txt string.
            string strOld = txtWeightVar;
            string strNew = cmbWeight.Text;

            //make sure it isn't "" or the same as the new variable picked.
            if ((strOld.Length != 0) && (strOld != strNew))
            {
                //add the former variable chosen back into the other lists
                cmbOutcome.Items.Add(strOld);
                cmbOther.Items.Add(strOld);
                cmbMatch.Items.Add(strOld);
                //cmbWeight.Items.Add(strOld);
            }

            //record the new variable and remove it from the other lists
            txtWeightVar = strNew;
            cmbOutcome.Items.Remove(strNew);
            cmbOther.Items.Remove(strNew);
            cmbMatch.Items.Remove(strNew);
            //cmbWeight.Items.Remove(strNew);

        }

        private void cmbWeight_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                //SelectedIndexChanged will add the var back to the other DDLs
                cmbWeight.Text = "";
                cmbWeight.SelectedIndex = -1;
            }
        }

        private void lbxInteractionTerms_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                while (lbxInteractionTerms.SelectedItems.Count > 0)
                {
                    lbxInteractionTerms.Items.Remove(lbxInteractionTerms.SelectedItem);
                }
            }

        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-logistic.html");
        }

        #endregion Event Handlers

    }
}