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
using Epi.Data.Services;

using VariableCollection = Epi.Collections.NamedObjectCollection<Epi.IVariable>;
namespace Epi.Windows.Analysis.Dialogs
{
	/// <summary>
	/// Dialog for Frequency command
	/// </summary>
    public partial class FrequencyDialog : CommandDesignDialog
    {
        #region Private Fields
        private Project currentProject;
        private string SetClauses = null;
        private string strWeightVar = "";
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor - NOT TO BE USED FOR INSTANTIATION
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public FrequencyDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for the Frequency dialog
        /// </summary>
        /// <param name="frm"></param>
        public FrequencyDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
        {
            InitializeComponent();
            Construct();
        }

        private void Construct()
        {
            if (!this.DesignMode) // designer throws an error
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
            //else
            //{
            //    currentProject = project;
            //}
        }
        #endregion Constructors


        #region Protected Methods
        /// <summary>
        /// Generates the command text
        /// </summary>
        protected override void GenerateCommand()
        {
            StringBuilder command = new StringBuilder();
            command.Append(CommandNames.FREQ);
            command.Append(StringLiterals.SPACE);
            if (this.cbxAllExcept.Checked)
            {
                command.Append(StringLiterals.STAR);
                command.Append(StringLiterals.SPACE);
                command.Append(CommandNames.EXCEPT);
                command.Append(StringLiterals.SPACE);
            }
            if (this.lbxVariables.Items.Count > 0)
            {
                foreach (string item in lbxVariables.Items)
                {
                    command.Append(FieldNameNeedsBrackets(item) ? Util.InsertInSquareBrackets(item) : item); 
                    command.Append(StringLiterals.SPACE);
                }
            }
            else
            {
                command.Append(StringLiterals.STAR);
                command.Append(StringLiterals.SPACE);
            }
            if (lbxStratifyBy.Items.Count > 0)
            {
                command.Append(CommandNames.STRATAVAR);
                command.Append(StringLiterals.EQUAL);
                foreach (string item in lbxStratifyBy.Items)
                {
                    command.Append(FieldNameNeedsBrackets(item) ? Util.InsertInSquareBrackets(item) : item);
                    command.Append(StringLiterals.SPACE);
                }
            }
            if (cmbWeight.Text != string.Empty)
            {
                command.Append(CommandNames.WEIGHTVAR);
                command.Append(StringLiterals.EQUAL);
                command.Append(FieldNameNeedsBrackets(cmbWeight.Text) ? Util.InsertInSquareBrackets(cmbWeight.Text) : cmbWeight.Text);
                command.Append(StringLiterals.SPACE);
            }

            if (txtOutput.Text != string.Empty)
            {
                command.Append(CommandNames.OUTTABLE);
                command.Append(StringLiterals.EQUAL);
                command.Append(txtOutput.Text.ToString());
                command.Append(StringLiterals.SPACE);
            }

            /*
            if (!string.IsNullOrEmpty(this.SetClauses))
            {
                command.Append(this.SetClauses);
                command.Append(StringLiterals.SPACE);
            }*/

            CommandText = command.ToString();
        }

        /// <summary>
        /// Output Table
        /// </summary>
        protected void OutputTable()
        {

        }

        
        /// <summary>
        /// Validates user input
        /// </summary>
        /// <returns>True/False depending upon whether error messages were found</returns>
        protected override bool ValidateInput()
        {
            base.ValidateInput();

            if (this.cbxAllExcept.Checked && this.lbxVariables.Items.Count == 0)
            {
                ErrorMessages.Add(SharedStrings.SELECT_EXCLUSION_VARIABLES);
            }
            if (this.lbxVariables.Items.Count == 0)
            {
                if (this.cmbVariables.Text.Equals(""))
                {
                    ErrorMessages.Add(SharedStrings.SELECT_VARIABLE);
                }
            }
            //if (!string.IsNullOrEmpty(txtOutput.Text.Trim()))
            //{
            //    currentProject.CollectedData.TableExists(txtOutput.Text.Trim());
            //    ErrorMessages.Add(SharedStrings.TABLE_EXISTS_OUTPUT);

            //}
            return (ErrorMessages.Count == 0);
        }

        #endregion //Protected Methods

        #region Private methods
        //private void LoadVariables(ComboBox cmb, DataTable tbl, bool numericOnly)
        //{
        //    String cn = ColumnNames.NAME;
        //    String dt = ColumnNames.DATA_TYPE;
        //    string s;
        //    foreach (DataRow row in tbl.Rows)
        //    {
        //        if (!numericOnly || (Int32.Parse(row[dt].ToString()) == (Int32)DataType.Number))
        //        {
        //            s = row[cn].ToString();
        //            if (s != ColumnNames.REC_STATUS && s != ColumnNames.UNIQUE_KEY)
        //            {
        //                cmb.Items.Add(row[cn].ToString());
        //            }
        //        }
        //    }
        //}

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
        private void btnClear_Click(object sender, System.EventArgs e)
        {
            cmbVariables.Items.Clear();
            cmbStratifyBy.Items.Clear();
            cmbWeight.Items.Clear();
            cbxAllExcept.Checked = false;
            lbxVariables.Items.Clear();
            lbxStratifyBy.Items.Clear();
            cmbWeight.Text = string.Empty;
            txtOutput.Text = string.Empty;
            cmbStratifyBy.Text = string.Empty;
            FrequencyDialog_Load(this, null);

            CheckForInputSufficiency();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbVariables_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbVariables.Text != StringLiterals.STAR)
            {
                string s = cmbVariables.Text;
                lbxVariables.Items.Add(s);
                cmbStratifyBy.Items.Remove(s);
                cmbVariables.Items.Remove(s);
                cmbWeight.Items.Remove(s);
                this.cbxAllExcept.Enabled = true;
            }
            else
            {
                this.cbxAllExcept.Checked = false;
                this.cbxAllExcept.Enabled = false;
            }
            
            CheckForInputSufficiency();

        }

        /// <summary>
        /// Handles the SelectedIndexChanged event
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbStratifyBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbStratifyBy.Text != StringLiterals.SPACE)
            {
                string s = cmbStratifyBy.Text;
                lbxStratifyBy.Items.Add(s);
                cmbStratifyBy.Items.Remove(s);
                cmbVariables.Items.Remove(s);
                cmbWeight.Items.Remove(s);
            }
        }

        /// <summary>
        /// Handles the Attach Event for the form, fills all of the dialogs with a list of variables.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void FrequencyDialog_Load(object sender, EventArgs e)
        {
            VariableType scopeWord = VariableType.DataSource | VariableType.DataSourceRedefined | 
                                     VariableType.Standard | VariableType.Global;

            FillVariableCombo(cmbVariables, scopeWord);
            cmbVariables.Items.Add("*");
            if (cmbVariables.Items[0].ToString().Equals("*"))
            {
                cmbVariables.SelectedIndex = 0;
            }
            
            FillVariableCombo(cmbStratifyBy, scopeWord);
            cmbStratifyBy.SelectedIndex = -1;

            FillWeightVariableCombo(cmbWeight, scopeWord);
            cmbWeight.SelectedIndex = -1;
        }

        /// <summary>
        /// Handles the Selected Index Change event for lbxVariables.  
        /// Moves the variable name back to the comboboxes
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void lbxVariables_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbxVariables.SelectedIndex >= 0)   // prevent the remove below from re-entering
            {
                string s = lbxVariables.SelectedItem.ToString();
                cmbVariables.Items.Add(s);
                cmbStratifyBy.Items.Add(s);
                cmbWeight.Items.Add(s);
                lbxVariables.Items.Remove(s);
            }

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
                cmbVariables.Items.Add(s);
                cmbStratifyBy.Items.Add(s);
                cmbWeight.Items.Add(s);
                lbxStratifyBy.Items.Remove(s);
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            SetDialog SD = new SetDialog(mainForm);
            SD.isDialogMode = true;
            SD.ShowDialog();
            SetClauses = SD.CommandText;
            SD.Close();
        }

        private void cmbWeight_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Get the old variable chosen that was saved to the txt string.
            string strOld = strWeightVar;
            string strNew = cmbWeight.Text;

            //make sure it isn't "" or the same as the new variable picked.
            if ((strOld.Length != 0) && (strOld != strNew))
            {
                //add the former variable chosen back into the other lists
                cmbVariables.Items.Add(strOld);
                cmbStratifyBy.Items.Add(strOld);
            }

            //record the new variable and remove it from the other lists
            strWeightVar = strNew;
            cmbVariables.Items.Remove(strNew);
            cmbStratifyBy.Items.Remove(strNew);
            
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

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://wwwn.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-freq.html");
        }

        #endregion //Event Handlers
    }
}

