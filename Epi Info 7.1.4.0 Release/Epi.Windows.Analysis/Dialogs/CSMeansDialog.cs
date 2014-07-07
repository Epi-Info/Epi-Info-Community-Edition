using System;
using System.Data;
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
	/// Dialog for Means command
	/// </summary>
    public partial class ComplexSampleMeansDialog : CommandDesignDialog
	{
        private string SetClauses = null;

        #region Constructors

        /// <summary>
        /// Default constructor - NOT TO BE USED FOR INSTANTIATION
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public ComplexSampleMeansDialog()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// Constructor for the Means dialog
        /// </summary>
        /// <param name="frm"></param>
        public ComplexSampleMeansDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
        {
            InitializeComponent();
            Construct();
        }
        
        private void Construct()
        {
            if (!this.DesignMode)           // designer throws an error
            {
                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
            }

            //IProjectHost host = Module.GetService(typeof(IProjectHost)) as IProjectHost;
            //if (host == null)
            //{
            //    throw new GeneralException("No project is hosted by service provider.");
            //}

            //// get project reference
            //Project project = host.CurrentProject;
            //if (project == null)
            //{
            //    //A native DB has been read, no longer need to show error message
            //    //throw new GeneralException("You must first open a project.");
            //    //MessageBox.Show("You must first open a project.");
            //    //this.Close();
            //}
        }
        #endregion Constructors

        #region Private Properties

        private string txtMeansOf = string.Empty;
        private string txtCrossTab = string.Empty;
        private string txtWeight = string.Empty;
        private string txtPSU = string.Empty;
        private string txtStratifyBy = string.Empty;

        #endregion Private Properties

        #region Private methods

        #endregion

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

        #region Event Handlers
        /// <summary>
        /// Handles the btnClear Click event
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void btnClear_Click(object sender, System.EventArgs e)
        {
            cmbMeansOf.Items.Clear();
            cmbMeansOf.Text = string.Empty;
            cmbStratifyBy.Items.Clear();
            cmbCrossTab.Items.Clear();
            cmbCrossTab.Text = string.Empty;
            cmbWeight.Items.Clear();
            cmbWeight.Text = string.Empty;
            cmbPSU.Text = string.Empty;
            txtOutput.Text = string.Empty;
            //lbxStratifyBy.Items.Clear();            
            ComplexSampleMeansDialog_Load(this, null);
        }

        /// <summary>
        /// Handles the cmbStratifyBy SelectedIndexChanged event
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbStratifyBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbStratifyBy.Text != StringLiterals.SPACE)
            {
                //Get the old variable chosen that was saved to the txt string.
                string strOld = txtStratifyBy;
                string strNew = cmbStratifyBy.Text;
                //make sure it isn't "" or the same as the new variable picked.
                if ((strOld.Length > 0) && (strOld != strNew))
                {
                    cmbMeansOf.Items.Add(strOld);
                    cmbCrossTab.Items.Add(strOld);
                    //cmbStratifyBy.Items.Add(strOld);
                    cmbWeight.Items.Add(strOld);
                    cmbPSU.Items.Add(strOld);
                }
                if (cmbStratifyBy.SelectedIndex >= 0)
                //lbxStratifyBy.Items.Add(s);
                cmbMeansOf.Items.Remove(strNew);
                cmbCrossTab.Items.Remove(strNew);
                //cmbStratifyBy.Items.Remove(strNew);
                cmbWeight.Items.Remove(strNew);
                cmbPSU.Items.Remove(strNew);
                this.txtStratifyBy = strNew;
            }
        }

        /// <summary>
        /// Handles the Attach Event for the form, fills all of the dialogs with a list of variables.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void ComplexSampleMeansDialog_Load(object sender, EventArgs e)
        {
            VariableType scopeWord = VariableType.DataSource | VariableType.DataSourceRedefined |
                                                    VariableType.Standard;
            FillVariableCombo(cmbMeansOf, scopeWord);
            cmbMeansOf.SelectedIndex = -1;
            FillVariableCombo(cmbCrossTab, scopeWord);
            cmbCrossTab.SelectedIndex = -1;
            FillVariableCombo(cmbStratifyBy, scopeWord);
            cmbStratifyBy.SelectedIndex = -1;
            FillVariableCombo(cmbWeight, scopeWord,DataType.Boolean | DataType.YesNo | DataType.Number );
            cmbWeight.SelectedIndex = -1;
            FillVariableCombo(cmbPSU, scopeWord);
            cmbStratifyBy.SelectedIndex = -1;
        }


        /// <summary>
        /// Handles the Selected Index Change event for lbxStratifyBy.  
        /// Moves the variable name back to the comboboxes
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void lbxStratifyBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbxStratifyBy.SelectedIndex >= 0)   // prevent the remove below from re-entering
            {
                string s = this.lbxStratifyBy.SelectedItem.ToString();
                cmbMeansOf.Items.Add(s);
                cmbCrossTab.Items.Add(s);
                cmbStratifyBy.Items.Add(s);
                cmbWeight.Items.Add(s);
                cmbPSU.Items.Add(s); 
                lbxStratifyBy.Items.Remove(s);
            }
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
        /// Handles the Click event for btnSettings.  
        /// Instantiates a new Settings dialog, sets its dialog mode = True.
        /// Shows the dialog and returns any settings.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void btnSettings_Click(object sender, EventArgs e)
        {
            SetDialog SD = new SetDialog((Epi.Windows.Analysis.Forms.AnalysisMainForm)mainForm);
            SD.isDialogMode = true;
            SD.ShowDialog();
            SetClauses = SD.CommandText;
            SD.Close();
        }

        /// <summary>
        /// Handles the cmbMeansOf SelectedIndexChanged event
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbMeansOf_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbMeansOf.SelectedIndex >= 0)
            {
                string s = txtMeansOf;
                if ((s.Length > 0) && (s != cmbMeansOf.Text))
                {
                    //cmbMeansOf.Items.Add(s);
                    cmbCrossTab.Items.Add(s);
                    cmbStratifyBy.Items.Add(s);
                    cmbWeight.Items.Add(s);
                    cmbPSU.Items.Add(s);
                }
                s = this.cmbMeansOf.SelectedItem.ToString();
                //cmbMeansOf.Items.Remove(s);
                cmbCrossTab.Items.Remove(s);
                cmbStratifyBy.Items.Remove(s);
                cmbWeight.Items.Remove(s);
                cmbPSU.Items.Remove(s);
                this.txtMeansOf = s;
            }
            CheckForInputSufficiency();
        }

        /// <summary>
        /// Handles the Click event for cmbMeansOf.  
        /// Saves the content of the text to the temp storage or resets.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbMeansOf_Click(object sender, EventArgs e)
        {
            txtMeansOf = (cmbMeansOf.SelectedIndex >= 0) ? cmbMeansOf.Text : String.Empty;
        }

        /// <summary>
        /// Handles the cmbCrossTab SelectedIndexChanged event
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbCrossTab_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Get the old variable chosen that was saved to the txt string.
            string strOld = txtCrossTab;
            string strNew = cmbCrossTab.Text;
            //make sure it isn't "" or the same as the new variable picked.
            if ((strOld.Length > 0) && (strOld != strNew))
            {
                    cmbMeansOf.Items.Add(strOld);
                    //cmbCrossTab.Items.Add(strOld);
                    cmbStratifyBy.Items.Add(strOld);
                    cmbWeight.Items.Add(strOld);
                    cmbPSU.Items.Add(strOld);
            }
            
            if (cmbCrossTab.SelectedIndex>=0)
            {
                cmbMeansOf.Items.Remove(strNew);
                //cmbCrossTab.Items.Remove(strNew);
                cmbStratifyBy.Items.Remove(strNew);
                cmbWeight.Items.Remove(strNew);
                cmbPSU.Items.Remove(strNew);
                this.txtCrossTab = strNew;
            }
        }

        /// <summary>
        /// Handles the Click event for cmbCrossTab.  
        /// Saves the content of the text to the temp storage or resets.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbCrossTab_Click(object sender, EventArgs e)
        {
            txtCrossTab = (cmbCrossTab.SelectedIndex >= 0) ? cmbCrossTab.Text : String.Empty;
        }

        
        /// <summary>
        /// Handles the KeyDown event for cmbCrossTab.  
        /// Allows to delete the optional value for CrossTab.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbCrossTab_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                //SelectedIndexChanged will add the var back to the other DDLs
                cmbCrossTab.Text = "";
                cmbCrossTab.SelectedIndex = -1;
            }
        }


        /// <summary>
        /// Handles the cmbWeight SelectedIndexChanged event
        /// </summary>
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
                cmbMeansOf.Items.Add(strOld);
                cmbCrossTab.Items.Add(strOld);
                cmbStratifyBy.Items.Add(strOld);
                //cmbWeight.Items.Add(strOld);
                cmbPSU.Items.Add(strOld);
            }

            if (cmbWeight.SelectedIndex >= 0)
            {
                cmbMeansOf.Items.Remove(strNew);
                cmbCrossTab.Items.Remove(strNew);
                cmbStratifyBy.Items.Remove(strNew);
                //cmbWeight.Items.Remove(strNew);
                cmbPSU.Items.Remove(strNew);
                this.txtWeight = strNew;
            }
        }


        /// <summary>
        /// Handles the Click event for cmbWeight.  
        /// Saves the content of the text to the temp storage or resets.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbWeight_Click(object sender, EventArgs e)
        {
            txtWeight = (cmbWeight.SelectedIndex >= 0) ? cmbWeight.Text : String.Empty;
        }

        /// <summary>
        /// Handles the cmbPSU SelectedIndexChanged event
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbPSU_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPSU.SelectedIndex >= 0)
            {
                string s = txtPSU;
                if ((s.Length > 0) && (s != cmbPSU.Text))
                {
                    cmbMeansOf.Items.Add(s);
                    cmbCrossTab.Items.Add(s);
                    cmbStratifyBy.Items.Add(s);
                    cmbWeight.Items.Add(s);
                    //cmbPSU.Items.Add(s);
                }
                s = this.cmbPSU.SelectedItem.ToString();
                cmbMeansOf.Items.Remove(s);
                cmbCrossTab.Items.Remove(s);
                cmbStratifyBy.Items.Remove(s);
                cmbWeight.Items.Remove(s);
                //cmbPSU.Items.Remove(s);
                this.txtPSU = s;
            }
            CheckForInputSufficiency();
        }

        /// <summary>
        /// Handles the Click event for cmbPSU.  
        /// Saves the content of the text to the temp storage or resets.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbPSU_Click(object sender, EventArgs e)
        {
            txtPSU = (cmbPSU.SelectedIndex >= 0) ? cmbPSU.Text : String.Empty;
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
        /// Handles the Leave event for Number of Columns textbox.  
        /// <remarks>Clears the textbox if only a 0 exists.</remarks>
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void txtNumCol_Leave(object sender, EventArgs e)
        {
            if (txtNumCol.Text.Equals("0")) txtNumCol.Text = "";
        }


        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://wwwn.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-means.html");
        }

        #endregion //Event Handlers

        #region Protected Methods
        /// <summary>
        /// Generates the command text
        /// </summary>
        protected override void GenerateCommand()
        {
            StringBuilder command = new StringBuilder ();
            command.Append(CommandNames.MEANS);
            command.Append(StringLiterals.SPACE);
            command.Append(cmbMeansOf.Text);
            if (cmbCrossTab.SelectedIndex >= 0)
            {
                command.Append(StringLiterals.SPACE);
                command.Append(cmbCrossTab.Text);
            }

//            if (lbxStratifyBy.Items.Count > 0)
            if (cmbStratifyBy.SelectedIndex >= 0)
            {
                command.Append(StringLiterals.SPACE);
                command.Append(CommandNames.STRATAVAR);
                command.Append(StringLiterals.EQUAL);
                command.Append(cmbStratifyBy.Text);
                /*
                foreach (string item in lbxStratifyBy.Items)
                {
                    command.Append(item);
                    command.Append(StringLiterals.SPACE);
                }
                */
            }

            if (cmbWeight.SelectedIndex >= 0)
            {
                command.Append(StringLiterals.SPACE);
                command.Append(CommandNames.WEIGHTVAR);
                command.Append( StringLiterals.EQUAL );
                command.Append( cmbWeight.Text );
            }

            if (!string.IsNullOrEmpty(txtOutput.Text))
            {
                command.Append(StringLiterals.SPACE);
                command.Append(CommandNames.OUTTABLE);
                command.Append(StringLiterals.EQUAL);
                command.Append(txtOutput.Text);
            }

            if (!string.IsNullOrEmpty(this.SetClauses))
            {
                command.Append(StringLiterals.SPACE);
                command.Append(this.SetClauses);
            }

            command.Append(StringLiterals.SPACE);
            command.Append(CommandNames.PSUVAR);
            command.Append(StringLiterals.EQUAL);
            command.Append(cmbPSU.Text);

            if (txtNumCol.TextLength > 0)
            {
                command.Append(StringLiterals.SPACE);
                command.Append(CommandNames.COLUMNSIZE);
                command.Append(StringLiterals.EQUAL);
                command.Append(txtNumCol.Text);
            }
            if (cbxNoLineWrap.Checked)
            {
                command.Append(StringLiterals.SPACE);
                command.Append(CommandNames.NOWRAP);
            }

            CommandText = command.ToString();
        }

        /// <summary>
        /// Validates user input
        /// </summary>
        /// <returns>True/False depending upon whether error messages were found</returns>
        protected override bool ValidateInput()
        {
            base.ValidateInput();

            if (this.cmbMeansOf.SelectedIndex == -1 )
            {
                ErrorMessages.Add(SharedStrings.SELECT_VARIABLE);
            }
            if (this.cmbPSU.SelectedIndex == -1)
            {
                ErrorMessages.Add(SharedStrings.MUST_SELECT_PSU);
            }
            //if (!string.IsNullOrEmpty(txtOutput.Text.Trim()))
            //{
            //    currentProject.CollectedData.TableExists(txtOutput.Text.Trim());
            //    ErrorMessages.Add(SharedStrings.TABLE_EXISTS_OUTPUT);

            //}
            return (ErrorMessages.Count == 0);
        }

        #endregion //Protected Methods
    }
}
