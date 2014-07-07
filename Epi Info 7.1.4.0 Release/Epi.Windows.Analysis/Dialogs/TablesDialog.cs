using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using Epi.Analysis;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;
using Epi.Data.Services;

namespace Epi.Windows.Analysis.Dialogs
{
	/// <summary>
	/// Dialog for Tables command
	/// </summary>
    public partial class TablesDialog : CommandDesignDialog
	{
		#region Constructor	

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public TablesDialog()
		{
			InitializeComponent();
            Construct();
		}

        /// <summary>
        /// TablesDialog constructor
        /// </summary>
        /// <param name="frm"></param>
        public TablesDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
		{
			InitializeComponent();
            Construct();
		}
		#endregion Constructors

        #region Private Properties

        private string strOutcome = "";
        private string strExposure = "*";
        private string strWeight = "";

        #endregion Private Properties

        #region Private Methods

        private void Construct()
        {
            if (!this.DesignMode)
            {
                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
            }
            this.cbxMatch.Visible = false; // fix for defect #603
        }

        private void TablesDialog_Load( object sender, EventArgs e )
        {
            VariableType scopeWord = VariableType.DataSource | VariableType.DataSourceRedefined |
                                        VariableType.Standard;
            FillVariableCombo( cmbExposure, scopeWord );
            //cmbExposure.Items.Insert( 0, "*" );
            //cmbExposure.SelectedIndex = 0;
            cmbExposure.SelectedIndex = -1;
            FillVariableCombo( cmbOutcome, scopeWord );
            FillVariableCombo( cmbStratifyBy, scopeWord );
            cmbStratifyBy.SelectedIndex = -1;
            FillWeightVariableCombo( cmbWeight, scopeWord );
            cmbWeight.SelectedIndex = -1;
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
            sb.Append(FieldNameNeedsBrackets(cmbExposure.Text) ? Util.InsertInSquareBrackets(cmbExposure.Text) : cmbExposure.Text); 
            sb.Append(StringLiterals.SPACE);
            sb.Append(FieldNameNeedsBrackets(cmbOutcome.Text) ? Util.InsertInSquareBrackets(cmbOutcome.Text) : cmbOutcome.Text); 
            sb.Append(StringLiterals.SPACE);
            if (!string.IsNullOrEmpty(cmbWeight.Text))
            {
                sb.Append("WEIGHTVAR");
                sb.Append(StringLiterals.EQUAL);
                sb.Append(FieldNameNeedsBrackets(cmbWeight.Text) ? Util.InsertInSquareBrackets(cmbWeight.Text) : cmbWeight.Text);
                sb.Append(StringLiterals.SPACE);
            }
            if (cbxMatch.Checked)
            {
                sb.Append("MATCHVAR");
                sb.Append(StringLiterals.EQUAL);
                foreach (string s in this.lbxStratifyBy.Items)
                {
                    sb.Append(FieldNameNeedsBrackets(s) ? Util.InsertInSquareBrackets(s) : s); 
                    sb.Append(StringLiterals.SPACE);
                }
            }
            else if (lbxStratifyBy.Items.Count > 0)
            {
                sb.Append("STRATAVAR");
                sb.Append(StringLiterals.EQUAL);
                foreach (string s in this.lbxStratifyBy.Items)
                {
                    sb.Append(FieldNameNeedsBrackets(s) ? Util.InsertInSquareBrackets(s) : s);
                    sb.Append(StringLiterals.SPACE);
                }
            }


            if (!string.IsNullOrEmpty(this.txtOutput.Text))
            {
                sb.Append("OUTTABLE");
                sb.Append(StringLiterals.EQUAL);
                sb.Append(FieldNameNeedsBrackets(this.txtOutput.Text) ? Util.InsertInSquareBrackets(this.txtOutput.Text) : this.txtOutput.Text);
                sb.Append(StringLiterals.SPACE);
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
            // cmbStratifyby doubles as cmbMatchVar (which doesn't exist)
            if (cbxMatch.Checked && lbxStratifyBy.Items.Count == 0)
            {
                ErrorMessages.Add( SharedStrings.NO_MATCHVAR);
            }

            return (ErrorMessages.Count == 0);
        }

        #endregion Protected Methods

        #region Event Handlers
        private void btnClear_Click(object sender, System.EventArgs e)
        {
            cmbOutcome.Items.Clear();
            cmbStratifyBy.Items.Clear();
            cmbWeight.Items.Clear();
            txtOutput.Text = string.Empty;
            lbxStratifyBy.Items.Clear();
            TablesDialog_Load(this, null);

            CheckForInputSufficiency();
        }

        private void lbxStratifyBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbxStratifyBy.SelectedIndex >= 0)   // prevent the remove below from re-entering
            {
                string s = this.lbxStratifyBy.SelectedItem.ToString();
                cmbExposure.Items.Add(s);
                cmbOutcome.Items.Add(s);
                cmbStratifyBy.Items.Add(s);
                cmbWeight.Items.Add(s);
                lbxStratifyBy.Items.Remove(s);
            }
        }

        /// <summary>
        /// Handles the cmbStratifyBy SelectedIndexChanged event
        /// </summary>
        /// <remarks>Removes item from other comboboxes</remarks>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbStratifyBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbStratifyBy.SelectedIndex >= 0)
            {
                //Get the old variable chosen that was saved to the txt string.
                string strNew = cmbStratifyBy.Text;

                //record the new variable and remove it from the other lists
                lbxStratifyBy.Items.Add(strNew);
                cmbOutcome.Items.Remove(strNew);
                cmbExposure.Items.Remove(strNew);
                cmbStratifyBy.Items.Remove(strNew);
                cmbWeight.Items.Remove(strNew);
            }
            CheckForInputSufficiency();
        }


        private void cmbOutcome_SelectedIndexChanged( object sender, EventArgs e )
        {
            if (cmbOutcome.SelectedIndex >= 0)   // prevent the remove below from re-entering
            {
                //Get the old variable chosen that was saved to the txt string.
                string strOld = strOutcome;
                string strNew = cmbOutcome.Text;

                //make sure it isn't "" or the same as the new variable picked.
                if ((strOld.Length != 0) && (strOld != strNew))
                {
                    //add the former variable chosen back into the other lists
                    //cmbOutcome.Items.Add(strOld);
                    cmbExposure.Items.Add(strOld);
                    cmbStratifyBy.Items.Add(strOld);
                    cmbWeight.Items.Add(strOld);
                }

                //record the new variable and remove it from the other lists
                strOutcome = strNew;
                //cmbOutcome.Items.Remove(strNew);
                cmbExposure.Items.Remove(strNew);
                cmbStratifyBy.Items.Remove(strNew);
                cmbWeight.Items.Remove(strNew);
            }
            CheckForInputSufficiency();
        }

        private void cmbExposure_SelectedIndexChanged( object sender, EventArgs e )
        {
            if (cmbExposure.SelectedIndex >= 0)   // prevent the remove below from re-entering
            {
                //Get the old variable chosen that was saved to the txt string.
                string strOld = strExposure;
                string strNew = cmbExposure.Text;

                //make sure it isn't "" or the same as the new variable picked.
                if ((strOld.Length != 0) && (strOld != strNew) && !(strOld.Equals("*")))
                {
                    //add the former variable chosen back into the other lists
                    cmbOutcome.Items.Add(strOld);
                    //cmbExposure.Items.Add(strOld);
                    cmbStratifyBy.Items.Add(strOld);
                    cmbWeight.Items.Add(strOld);
                }

                //record the new variable and remove it from the other lists
                strExposure = strNew;
                cmbOutcome.Items.Remove(strNew);
                //cmbExposure.Items.Remove(strNew);
                cmbStratifyBy.Items.Remove(strNew);
                cmbWeight.Items.Remove(strNew);
            }
            CheckForInputSufficiency();
        }

        private void cmbWeight_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbWeight.SelectedIndex >= 0)   // prevent the remove below from re-entering
            {
                //Get the old variable chosen that was saved to the txt string.
                string strOld = strWeight;
                string strNew = cmbWeight.Text;

                //make sure it isn't "" or the same as the new variable picked.
                if ((strOld.Length != 0) && (strOld != strNew))
                {
                    //add the former variable chosen back into the other lists
                    cmbOutcome.Items.Add(strOld);
                    cmbExposure.Items.Add(strOld);
                    cmbStratifyBy.Items.Add(strOld);
                    //cmbWeight.Items.Add(strOld);
                }

                //record the new variable and remove it from the other lists
                strWeight = strNew;
                cmbOutcome.Items.Remove(strNew);
                cmbExposure.Items.Remove(strNew);
                cmbStratifyBy.Items.Remove(strNew);
                //cmbWeight.Items.Remove(strNew);
            }
        }

        private void cmbWeight_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                string s = cmbWeight.Text.ToString();
                //SelectedIndexChanged will add the var back to the other DDLs
                cmbWeight.Text = "";
                cmbWeight.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://wwwn.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-tables.html");
        }

        #endregion Event Handlers

        private void cbxMatch_CheckedChanged(object sender, EventArgs e)
        {
            if (cbxMatch.Checked)
            {
                this.Text = "Match";
                this.lblStratifyBy.Text = "Match Variables";
                this.lblStratifyBy.Font  = new Font(lblStratifyBy.Font, FontStyle.Bold);
            }
            else
            {
                this.Text = "Tables";
                this.lblStratifyBy.Text = "Stratify By";
                this.lblStratifyBy.Font = new Font(lblStratifyBy.Font, FontStyle.Regular);
            }
            CheckForInputSufficiency();
        }
    }
}