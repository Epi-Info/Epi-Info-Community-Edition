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
	/// Dialog for Complex Sample Tables command (TABLES or MATCH).
	/// </summary>
    public partial class ComplexSampleTablesDialog : CommandDesignDialog
	{
		#region Constructor	

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public ComplexSampleTablesDialog()
		{
			InitializeComponent();
            Construct();
		}

        /// <summary>
        /// TablesDialog constructor
        /// </summary>
        /// <param name="frm"></param>
        public ComplexSampleTablesDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
		{
			InitializeComponent();
            Construct();
		}
		#endregion Constructors

        #region Private Properties

        string txtExposure = String.Empty;
        string txtWeight = String.Empty;
        string txtStratifyBy = String.Empty;
        string txtPSU = String.Empty;
        string txtOutcome = String.Empty;

        #endregion Private Properties

        #region Private Methods

        protected void Construct()
        {
            if (!this.DesignMode)
            {
                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
            }
        }

        /// <summary>
        /// Loads the Complex Sample Tables dialog.
        /// </summary>
        private void TablesDialog_Load(object sender, EventArgs e)
        {
            VariableType scopeWord = VariableType.DataSource | VariableType.DataSourceRedefined |
                                        VariableType.Standard;
            FillVariableCombo( cmbExposure, scopeWord );
            cmbExposure.Items.Insert( 0, "*" );
            cmbExposure.SelectedIndex = -1;
            FillVariableCombo( cmbOutcome, scopeWord );
            FillVariableCombo( cmbStratifyBy, scopeWord );
            cmbStratifyBy.SelectedIndex = -1;
            FillVariableCombo(cmbWeight, scopeWord);
            FillVariableCombo(cmbPSU, scopeWord);
            cmbWeight.SelectedIndex = -1;
            cmbPSU.SelectedIndex = -1;
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
            if (cbxMatch.Checked)
            {
                sb.Append(CommandNames.MATCH);
            }
            else
            {
                sb.Append(CommandNames.TABLES);
            }

            sb.Append(StringLiterals.SPACE);
            sb.Append(cmbExposure.Text);
            sb.Append(StringLiterals.SPACE);
            sb.Append(cmbOutcome.Text );
            sb.Append(StringLiterals.SPACE);
            if (cbxMatch.Checked)
            {
                sb.Append(CommandNames.MATCHVAR); 
                sb.Append(StringLiterals.EQUAL);
                sb.Append(cmbStratifyBy.Text);
                
                //foreach (string s in this.lbxStratifyBy.Items)
                //{
                //    sb.Append(s);
                //    sb.Append(StringLiterals.SPACE);
                //}
            }
            //else if (lbxStratifyBy.Items.Count > 0 )
            else if (cmbStratifyBy.SelectedIndex >= 0 )
            {
                sb.Append(CommandNames.STRATAVAR);
                sb.Append(StringLiterals.EQUAL);
                sb.Append(cmbStratifyBy.Text);
                //foreach (string s in this.lbxStratifyBy.Items)
                //{
                //    sb.Append(s);
                //    sb.Append(StringLiterals.SPACE);
                //}
            }
            if (cmbWeight.SelectedIndex >= 0)
            {
                sb.Append(StringLiterals.SPACE);
                sb.Append(CommandNames.WEIGHTVAR);
                sb.Append(StringLiterals.EQUAL);
                sb.Append(cmbWeight.Text);
            }
            sb.Append(StringLiterals.SPACE);
            sb.Append(CommandNames.PSUVAR);
            sb.Append(StringLiterals.EQUAL);
            sb.Append(cmbPSU.Text);
            if (txtOutput.TextLength > 0)
            {
                sb.Append(StringLiterals.SPACE);
                sb.Append(CommandNames.OUTTABLE);
                sb.Append(StringLiterals.EQUAL);
                sb.Append(txtOutput.Text);
            }
            if (txtNumCol.TextLength > 0)
            {
                sb.Append(StringLiterals.SPACE);
                sb.Append(CommandNames.COLUMNSIZE);
                sb.Append(StringLiterals.EQUAL);
                sb.Append(txtNumCol.Text);
            }
            if (cbxNoLineWrap.Checked)
            {
                sb.Append(StringLiterals.SPACE);
                sb.Append(CommandNames.NOWRAP);
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
            if (cmbExposure.SelectedIndex == -1)
            {
                ErrorMessages.Add( SharedStrings.MUST_SELECT_EXPOSURE );
            }
            if (cmbOutcome.SelectedIndex == -1)
            {
                ErrorMessages.Add( SharedStrings.MUST_SELECT_OUTCOME );
            }
            if (cmbPSU.SelectedIndex == -1)
            {
                ErrorMessages.Add(SharedStrings.MUST_SELECT_PSU);
            }
            // cmbStratifyby doubles as cmbMatchVar (which doesn't exist)
            //if (cbxMatch.Checked && (lbxStratifyBy.Items.Count < 1))
            if (cbxMatch.Checked && (cmbStratifyBy.SelectedIndex < 0))
            {
                ErrorMessages.Add( SharedStrings.NO_MATCHVAR);
            }

            return (ErrorMessages.Count == 0);
        }

        #endregion Protected Methods

        #region Event Handlers
        /// <summary>
        /// Handles the btnClear Click event
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void btnClear_Click(object sender, System.EventArgs e)
        {
            cmbOutcome.Items.Clear();
            cmbStratifyBy.Items.Clear();
            cmbWeight.Items.Clear();
            cmbPSU.Text = string.Empty;
            txtOutput.Text = string.Empty;
            //lbxStratifyBy.Items.Clear();
            TablesDialog_Load(this, null);
        }

        /// <summary>
        /// Handles the lbxStratifyBy SelectedIndexChanged event.
        /// </summary>
        /// <remarks>Removes item from StratifyBy listbox; Adds it back to the other comboboxes.</remarks>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void lbxStratifyBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbxStratifyBy.SelectedIndex >= 0)   // prevent the remove below from re-entering
            {
                string s = this.lbxStratifyBy.SelectedItem.ToString();
                cmbExposure.Items.Add(s);
                cmbWeight.Items.Add(s);
                cmbPSU.Items.Add(s);
                cmbOutcome.Items.Add(s);
                cmbStratifyBy.Items.Add(s);
                lbxStratifyBy.Items.Remove(s);
            }
            CheckForInputSufficiency();
        }

        /// <summary>
        /// Handles the cmbStratifyBy SelectedIndexChanged event.
        /// </summary>
        /// <remarks>Removes item from other comboboxes; Inserts item into StratifyBy listbox.</remarks>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbStratifyBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Get the old variable chosen that was saved to the txt string.
            string strOld = txtStratifyBy;
            string strNew = cmbStratifyBy.Text;
            //make sure it isn't "" or the same as the new variable picked.
            if ((strOld.Length > 0) && (strOld != strNew))
            {
                cmbExposure.Items.Add(strOld);
                cmbWeight.Items.Add(strOld);
                cmbPSU.Items.Add(strOld);
                cmbOutcome.Items.Add(strOld);
                //cmbStratifyBy.Items.Add(strOld);
            }
            if (cmbStratifyBy.SelectedIndex >= 0)
            {
                cmbExposure.Items.Remove(strNew);
                cmbWeight.Items.Remove(strNew);
                cmbPSU.Items.Remove(strNew);
                cmbOutcome.Items.Remove(strNew);
                //cmbStratifyBy.Items.Remove(strNew);
                this.txtStratifyBy = strNew;
            }
            CheckForInputSufficiency();
        }

        /// <summary>
        /// Handles the cmbOutcome SelectedIndexChanged event
        /// </summary>
        /// <remarks>Removes item from other comboboxes</remarks>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbOutcome_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Get the old variable chosen that was saved to the txt string.
            string strOld = txtOutcome;
            string strNew = cmbOutcome.Text;
            //make sure it isn't "" or the same as the new variable picked.
            if ((strOld.Length > 0) && (strOld != strNew))
            {
                cmbExposure.Items.Add(strOld);
                cmbWeight.Items.Add(strOld);
                cmbPSU.Items.Add(strOld);
                //cmbOutcome.Items.Add(strOld);
                cmbStratifyBy.Items.Add(strOld);
            }
            if (cmbOutcome.SelectedIndex >= 0)
            {
                cmbExposure.Items.Remove(strNew);
                cmbWeight.Items.Remove(strNew);
                cmbPSU.Items.Remove(strNew);
                //cmbOutcome.Items.Remove(strNew);
                cmbStratifyBy.Items.Remove(strNew);
                this.txtExposure = strNew;
            }
            CheckForInputSufficiency();
        }

        /// <summary>
        /// Handles the cmbExposure SelectedIndexChanged event
        /// </summary>
        /// <remarks>Removes item from other comboboxes</remarks>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbExposure_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Get the old variable chosen that was saved to the txt string.
            string strOld = txtExposure;
            string strNew = cmbExposure.Text;
            //make sure it isn't "" or the same as the new variable picked.
            if ((strOld.Length > 0) && (strOld != strNew))
            {
                //cmbExposure.Items.Add(strOld);
                cmbWeight.Items.Add(strOld);
                cmbPSU.Items.Add(strOld);
                cmbOutcome.Items.Add(strOld);
                cmbStratifyBy.Items.Add(strOld);
            }
            if (cmbExposure.SelectedIndex >= 0)
            {
                //cmbExposure.Items.Remove(strNew);
                cmbWeight.Items.Remove(strNew);
                cmbPSU.Items.Remove(strNew);
                cmbOutcome.Items.Remove(strNew);
                cmbStratifyBy.Items.Remove(strNew);
                this.txtExposure = strNew;
            }
            CheckForInputSufficiency();
        }

        /// <summary>
        /// Handles the KeyDown event for cmbWeight.  
        /// Allows to delete the optional value for Weight.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbWeight_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                //SelectedIndexChanged will add the var back to the other DDLs
                cmbWeight.Text = "";
                cmbWeight.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Handles the Click event for cmbExposure.  
        /// Saves the content of the text to the temp storage or resets.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbExposure_Click(object sender, EventArgs e)
        {
            txtExposure = (cmbExposure.SelectedIndex >= 0) ? cmbExposure.Text : "";
        }

        /// <summary>
        /// Handles the Click event for cmbOutcome.  
        /// Saves the content of the text to the temp storage or resets.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbOutcome_Click(object sender, EventArgs e)
        {
            txtOutcome = (cmbOutcome.SelectedIndex >= 0) ? cmbOutcome.Text : "";
        }

        /// <summary>
        /// Handles the Click event for cmbPSU.  
        /// Saves the content of the text to the temp storage or resets.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbPSU_Click(object sender, EventArgs e)
        {
            txtPSU = (cmbPSU.SelectedIndex >= 0) ? cmbPSU.Text : "";
        }

        /// <summary>
        /// Handles the Click event for cmbWeight.  
        /// Saves the content of the text to the temp storage or resets.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbWeight_Click(object sender, EventArgs e)
        {
            txtWeight = (cmbWeight.SelectedIndex >= 0) ? cmbWeight.Text : "";
        }


        /// <summary>
        /// Handles the cmbWeight SelectedIndexChanged event
        /// </summary>
        /// <remarks>Removes item from other comboboxes</remarks>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbWeight_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Get the old variable chosen that was saved to the txt string.
            string strOld = txtWeight;
            string strNew = cmbWeight.Text;
            //make sure it isn't "" or the same as the new variable picked.
            if ((strOld.Length > 0) && (strOld != strNew))
            {
                cmbExposure.Items.Add(strOld);
                //cmbWeight.Items.Add(strOld);
                cmbPSU.Items.Add(strOld);
                cmbOutcome.Items.Add(strOld);
                cmbStratifyBy.Items.Add(strOld);
                }
            if (cmbWeight.SelectedIndex >= 0)
            {
                cmbExposure.Items.Remove(strNew);
                //cmbWeight.Items.Remove(strNew);
                cmbPSU.Items.Remove(strNew);
                cmbOutcome.Items.Remove(strNew);
                cmbStratifyBy.Items.Remove(strNew);
                this.txtWeight = strNew;
            }
        }

        /// <summary>
        /// Handles the cmbPSU SelectedIndexChanged event
        /// </summary>
        /// <remarks>Removes item from other comboboxes</remarks>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbPSU_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Get the old variable chosen that was saved to the txt string.
            string strOld = txtPSU;
            string strNew = cmbPSU.Text;
            //make sure it isn't "" or the same as the new variable picked.
            if ((strOld.Length > 0) && (strOld != strNew))
            {
                    cmbExposure.Items.Add(strOld);
                    cmbWeight.Items.Add(strOld);
                    //cmbPSU.Items.Add(strOld);
                    cmbOutcome.Items.Add(strOld);
                    cmbStratifyBy.Items.Add(strOld);
            }
            if (cmbPSU.SelectedIndex >= 0)
            {
                cmbExposure.Items.Remove(strNew);
                cmbWeight.Items.Remove(strNew);
                //cmbPSU.Items.Remove(strNew);
                cmbOutcome.Items.Remove(strNew);
                cmbStratifyBy.Items.Remove(strNew);
                this.txtExposure = strNew;
            }
            CheckForInputSufficiency();
        }

        /// <summary>
        /// Handles the KeyPress event for txtNumCol.  
        /// Allows only digits, delete, and backspace strokes; Limits to two digits length.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void txtNumCol_KeyPress(object sender, KeyPressEventArgs e)
        {
            string keyInput = e.KeyChar.ToString();
            if (e.KeyChar.ToString().Equals("\b"))
            {
                // Backspace key is OK
            }
            else if (e.KeyChar.Equals(Keys.Delete))
            {
                // Delete key is OK
            }
            else if (e.KeyChar.ToString().Equals("0"))
            {
                if (txtNumCol.TextLength == 0) e.Handled = true;
            }
            else if ((txtNumCol.TextLength <= 1) && (Char.IsDigit(e.KeyChar)))
            {
                // Digits are OK
            }
            else
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-tables.html");
        }

        #endregion Event Handlers

        /// <summary>
        /// Handles the Click event for Match checkbox.  
        /// <remarks>Sets lblStratifyBy font to bold indicating items are required.</remarks>
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cbxMatch_Click(object sender, EventArgs e)
        {
            if (cbxMatch.Checked)
            {
                lblStratifyBy.Text = "Match Variables";
                lblStratifyBy.Font = new Font(lblStratifyBy.Font, FontStyle.Bold);
            }
            else
            {
                lblStratifyBy.Text = "Stratify By";
                lblStratifyBy.Font = new Font(lblStratifyBy.Font, FontStyle.Regular);
            }
            CheckForInputSufficiency();
        }

        /// <summary>
        /// Handles the Leave event for Number of Columns textbox.  
        /// <remarks>Clears the textbox if only a 0 exists.</remarks>
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void txtNumCol_Leave(object sender, EventArgs e)
        {
            if (txtNumCol.Text.Equals("0")) txtNumCol.Text = "";
        }
    }   
}