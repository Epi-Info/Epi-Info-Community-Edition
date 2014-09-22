using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data;
using Epi.Analysis;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;

namespace Epi.Windows.Analysis.Dialogs
{
	/// <summary>
	/// Dialog for Means command
	/// </summary>
    public partial class MeansDialog : CommandDesignDialog
	{
        private string SetClauses = null;

        #region Constructors

        /// <summary>
        /// Default constructor - NOT TO BE USED FOR INSTANTIATION
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public MeansDialog()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// Constructor for the Means dialog
        /// </summary>
        /// <param name="frm"></param>
        public MeansDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
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

        private string strMeansOf = "*";
        private string strCrossTab = "";
        private string strWeight = "";
        private string strStrata = "";

        #endregion Private Properties

        #region Private Methods

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
            txtOutput.Text = string.Empty;
            lbxStratifyBy.Items.Clear();            
            MeansDialog_Load(this, null);
        }

        /// <summary>
        /// Handles the cmbMeansOf SelectedIndexChanged event
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbMeansOf_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbMeansOf.SelectedIndex >= 0)   // prevent the remove below from re-entering
            {
                //Get the old variable chosen that was saved to the txt string.
                string strOld = strMeansOf;
                string strNew = cmbMeansOf.Text;

                if (!strOld.Equals(StringLiterals.STAR))
                {
                    //make sure it isn't "" or the same as the new variable picked.
                    if ((strOld.Length != 0) && (strOld != strNew))
                    {
                        //add the former variable chosen back into the other lists
                        //cmbMeansOf.Items.Add(strOld);
                        cmbCrossTab.Items.Add(strOld);
                        cmbStratifyBy.Items.Add(strOld);
                        cmbWeight.Items.Add(strOld);
                    }
                }
                //record the new variable and remove it from the other lists
                strMeansOf = strNew;
                //cmbMeansOf.Items.Remove(strNew);
                cmbCrossTab.Items.Remove(strNew);
                cmbStratifyBy.Items.Remove(strNew);
                cmbWeight.Items.Remove(strNew);
            }

            CheckForInputSufficiency();

        }

        /// <summary>
        /// Handles the cmbStratifyBy SelectedIndexChanged event
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbStratifyBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbStratifyBy.SelectedIndex >= 0)
            {
                string strNew = cmbStratifyBy.Text;
                //remove the var from the other lists
                cmbMeansOf.Items.Remove(strNew);
                cmbCrossTab.Items.Remove(strNew);
                cmbStratifyBy.Items.Remove(strNew);
                cmbWeight.Items.Remove(strNew);
                lbxStratifyBy.Items.Add(FieldNameNeedsBrackets(strNew) ? Util.InsertInSquareBrackets(strNew) : strNew);
            }
        }

        /// <summary>
        /// Handles the Attach Event for the form, fills all of the dialogs with a list of variables.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void MeansDialog_Load(object sender, EventArgs e)
        {
            VariableType scopeWord = VariableType.DataSource | VariableType.DataSourceRedefined |
                                                    VariableType.Standard;
            FillVariableCombo(cmbMeansOf, scopeWord);
            //cmbMeansOf.Items.Insert(0, "*");  //ToDo: uncomment this when multivariate (*) is working
            //cmbMeansOf.SelectedIndex = 0;     //      and set SelectedIndex = 0
            cmbMeansOf.SelectedIndex = -1;
            FillVariableCombo(cmbCrossTab, scopeWord);
            cmbCrossTab.SelectedIndex = -1;
            FillVariableCombo(cmbStratifyBy, scopeWord);
            cmbStratifyBy.SelectedIndex = -1;
            FillVariableCombo(cmbWeight, scopeWord,DataType.Boolean | DataType.YesNo | DataType.Number );
            cmbWeight.SelectedIndex = -1;

            CheckForInputSufficiency();
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
                lbxStratifyBy.Items.Remove(s);
                //remove [square brackets] before adding back to other DDLs
                char[] cTrimParens = { '[', ']' };
                s = s.Trim(cTrimParens);
                cmbMeansOf.Items.Add(s);
                cmbStratifyBy.Items.Add(s);
                cmbCrossTab.Items.Add(s);
                cmbWeight.Items.Add(s);
            }
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
            string s;
            StringBuilder sb = new StringBuilder();
            sb.Append(CommandNames.MEANS);
            sb.Append(StringLiterals.SPACE);
            s = FieldNameNeedsBrackets(cmbMeansOf.Text) ? Util.InsertInSquareBrackets(cmbMeansOf.Text) : cmbMeansOf.Text;
            sb.Append((s==string.Empty)? "*" : s);
            sb.Append(StringLiterals.SPACE);
            if (!string.IsNullOrEmpty(cmbCrossTab.Text))
            {
                sb.Append(FieldNameNeedsBrackets(cmbCrossTab.Text) ? Util.InsertInSquareBrackets(cmbCrossTab.Text) : cmbCrossTab.Text);
                sb.Append(StringLiterals.SPACE);
            }

            if (lbxStratifyBy.Items.Count > 0)
            {
                sb.Append(CommandNames.STRATAVAR);
                sb.Append(StringLiterals.EQUAL);
                foreach (string item in lbxStratifyBy.Items)
                {
                    sb.Append(item);
                    sb.Append(StringLiterals.SPACE);
                }
            }
            if (!string.IsNullOrEmpty(cmbWeight.Text))
            {
                sb.Append( CommandNames.WEIGHTVAR );
                sb.Append( StringLiterals.EQUAL );
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

            /*
            if (!string.IsNullOrEmpty(this.SetClauses))
            {
                sb.Append(this.SetClauses);
            }*/

            CommandText = sb.ToString();
        }

        /// <summary>
        /// Validates user input
        /// </summary>
        /// <returns>true if there is no error; else false</returns>
        protected override bool ValidateInput()
        {
            base.ValidateInput();
            if (cmbMeansOf.SelectedIndex == -1)
            {
                ErrorMessages.Add(SharedStrings.MUST_SELECT_TERMS);
            }

            return (ErrorMessages.Count == 0);
        }


        #endregion //Protected Methods

        private void btnSettings_Click(object sender, EventArgs e)
        {
            SetDialog SD = new SetDialog((Epi.Windows.Analysis.Forms.AnalysisMainForm)mainForm);
            SD.isDialogMode = true;
            SD.ShowDialog();
            SetClauses = SD.CommandText;
            SD.Close();
        }

        private void cmbWeight_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Get the old variable chosen that was saved to the txt string.
            string strOld = strWeight;
            string strNew = cmbWeight.Text;

            //make sure it isn't "" or the same as the new variable picked.
            if ((strOld.Length != 0) && strOld != strNew)
            {
                //add the former variable chosen back into the other lists
                cmbMeansOf.Items.Add(strOld);
                cmbCrossTab.Items.Add(strOld);
                cmbStratifyBy.Items.Add(strOld);
                //cmbWeight.Items.Add(strOld);
            }

            //record the new variable and remove it from the other lists
            strWeight = strNew;
            cmbMeansOf.Items.Remove(strNew);
            cmbCrossTab.Items.Remove(strNew);
            cmbStratifyBy.Items.Remove(strNew);
            //cmbWeight.Items.Remove(strNew);
           
        }

        private void cmbCrossTab_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCrossTab.SelectedIndex >= 0)   // prevent the remove below from re-entering
            {
                //Get the old variable chosen that was saved to the txt string.
                string strOld = strCrossTab;
                string strNew = cmbCrossTab.Text;

                //make sure it isn't "" or the same as the new variable picked.
                if (strOld != strNew)
                {
                    //add the former variable chosen back into the other lists
                    cmbMeansOf.Items.Add(strOld);
                    //cmbCrossTab.Items.Add(strOld);
                    cmbStratifyBy.Items.Add(strOld);
                    cmbWeight.Items.Add(strOld);
                }

                //record the new variable and remove it from the other lists
                strCrossTab = strNew;
                cmbMeansOf.Items.Remove(strNew);
                //cmbCrossTab.Items.Remove(strNew);
                cmbStratifyBy.Items.Remove(strNew);
                cmbWeight.Items.Remove(strNew);
            }
        }

        private void cmbCrossTab_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                string s = cmbCrossTab.Text.ToString();
                //SelectedIndexChanged will add the var back to the other DDLs
                cmbCrossTab.Text = "";
                cmbCrossTab.SelectedIndex = -1;
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
    }
}
