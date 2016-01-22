using System;
using System.Collections;
using System.Collections.Generic;
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
    /// Dialog for KMSurvival command
    /// </summary>
    public partial class KaplanMeierSurvivalDialog : CommandDesignDialog
    {
        #region Constructor

        /// <summary>
        /// Default constructor - NOT TO BE USED FOR INSTANTIATION
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public KaplanMeierSurvivalDialog()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// KaplanMeierSurvivalDialog constructor
        /// </summary>
        /// <param name="frm"></param>
        public KaplanMeierSurvivalDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
        {
            InitializeComponent();
            Construct();
        }
        #endregion Constructors

        #region Private Properties
        private string txtCensoredVar;
        private string txtTimeVar;
        private string txtGroupVar;
        private string txtWeightVar;
        private string txtOtherVar;
        private string txtStrataVar;

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

        private void KaplanMeierSurvivalDialog_Load(object sender, EventArgs e)
        {
            VariableType scopeWord = VariableType.DataSource | VariableType.DataSourceRedefined |
                                        VariableType.Standard;
            FillVariableCombo(cmbCensoredVar, scopeWord);
            FillVariableCombo(cmbTimeVar, scopeWord);
            FillVariableCombo(cmbGroupVar, scopeWord);
            FillVariableCombo(cmbWeightVar, scopeWord);
            cmbWeightVar.SelectedIndex = -1;
            FillVariableCombo(cmbOther, scopeWord);
            FillVariableCombo(cmbStrataVar, scopeWord);

            cmbTimeUnit.Items.Add("");
            cmbTimeUnit.Items.Add("Days");
            cmbTimeUnit.Items.Add("Hours");
            cmbTimeUnit.Items.Add("Months");
            cmbTimeUnit.Items.Add("Weeks");
            cmbTimeUnit.Items.Add("Years");

            cmbGraphType.Items.Add("None");
            //cmbGraphType.Items.Add("Hazard Function");   'commented graph types are available in Epi 3 CoxPH, but not KMSurv (yet)
            //cmbGraphType.Items.Add("Log-Log Survival");
            //cmbGraphType.Items.Add("Log-Log-Observed");
            //cmbGraphType.Items.Add("Observed");
            cmbGraphType.Items.Add("Survival Probability");
            //cmbGraphType.Items.Add("Survival-Observed");
            cmbGraphType.Items.Add("Data Table Only");
            cmbGraphType.SelectedIndex = 1;

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
            sb.Append(CommandNames.KMSURVIVAL);

            sb.Append(StringLiterals.SPACE);
            sb.Append(FieldNameNeedsBrackets(cmbTimeVar.Text) ? Util.InsertInSquareBrackets(cmbTimeVar.Text) : cmbTimeVar.Text);
            sb.Append(StringLiterals.SPACE);
            sb.Append(StringLiterals.EQUAL);
            sb.Append(StringLiterals.SPACE);
            sb.Append(FieldNameNeedsBrackets(cmbGroupVar.Text) ? Util.InsertInSquareBrackets(cmbGroupVar.Text) : cmbGroupVar.Text);
            sb.Append(StringLiterals.SPACE);

            //foreach (object obj in lbxOther.Items)
            //{
            //    sb.Append(obj.ToString());
            //    sb.Append(StringLiterals.SPACE);
            //}

            /*
            foreach (object obj in lbxExtendedTerms.Items)
            {
                sb.Append(obj.ToString());
                sb.Append(StringLiterals.SPACE);
            }
            
            foreach (object obj in lbxInteractionTerms.Items)
            {
                sb.Append(obj.ToString());
                sb.Append(StringLiterals.SPACE);
            }
            */
            sb.Append(StringLiterals.STAR);
            sb.Append(StringLiterals.SPACE);
            sb.Append(FieldNameNeedsBrackets(cmbCensoredVar.Text) ? Util.InsertInSquareBrackets(cmbCensoredVar.Text) : cmbCensoredVar.Text);
            sb.Append(StringLiterals.SPACE);
            sb.Append(StringLiterals.PARANTHESES_OPEN);
            sb.Append(StringLiterals.SPACE);

            if (this.EpiInterpreter.Context.CurrentRead != null)
            {
                List<System.Data.DataRow> Rows = this.EpiInterpreter.Context.GetOutput();
                System.Data.DataTable tempTbl = this.EpiInterpreter.Context.DataSet.Tables["output"];
                Dictionary<string,string> table =  new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
                //System.Data.DataTable table = Epi.Data.DBReadExecute.GetDataTable(this.EpiInterpreter.Context.CurrentRead.File, "Select Distinct [" + cmbCensoredVar.Text + "] from [" + this.EpiInterpreter.Context.CurrentRead.Identifier + "]");
//                System.Data.DataTable table = Epi.Data.DBReadExecute.GetDataTable(this.EpiInterpreter.Context.CurrentRead.File, "Select Distinct [" + cmbCensoredVar.Text + "] " + this.EpiInterpreter.Context.CurrentProject.GetViewByName(this.EpiInterpreter.Context.CurrentRead.Identifier).FromViewSQL);


                string type;
                type = tempTbl.Columns[cmbCensoredVar.Text].DataType.ToString();
                if (type.Equals("System.Byte") || type.Equals("System.Int16") || type.Equals("System.Boolean"))
                {
                    Dictionary<string, string> setProperties = this.EpiInterpreter.Context.GetGlobalSettingProperties();
                    if (cmbUncensoredValue.Text.ToString().Equals(setProperties["RepresentationOfYes"].ToString()))
                    {
                        sb.Append(StringLiterals.EPI_REPRESENTATION_OF_TRUE);
                    }
                    else if (cmbUncensoredValue.Text.ToString().Equals(setProperties["RepresentationOfNo"].ToString()))
                    {
                        sb.Append(StringLiterals.EPI_REPRESENTATION_OF_FALSE);
                    }
                    else if (cmbUncensoredValue.Text.ToString().Equals(setProperties["RepresentationOfMissing"].ToString()))
                    {
                        sb.Append(StringLiterals.EPI_REPRESENTATION_OF_MISSING);
                    }
                    else
                    {
                        sb.Append(cmbUncensoredValue.Text.ToString());
                    }
                }
                else if (!IsNumeric(cmbUncensoredValue.Text))
                {
                    sb.Append(StringLiterals.DOUBLEQUOTES);
                    sb.Append(cmbUncensoredValue.Text);
                    sb.Append(StringLiterals.DOUBLEQUOTES);
                }
                else
                {
                    sb.Append(cmbUncensoredValue.Text);
                }
            }

            sb.Append(StringLiterals.SPACE);
            sb.Append(StringLiterals.PARANTHESES_CLOSE);
            sb.Append(StringLiterals.SPACE);
            if (!string.IsNullOrEmpty(cmbTimeUnit.Text))
            {
                sb.Append("TIMEUNIT=");
                sb.Append(StringLiterals.DOUBLEQUOTES).Append(cmbTimeUnit.Text).Append(StringLiterals.DOUBLEQUOTES);
                sb.Append(StringLiterals.SPACE);
            }

            if (!string.IsNullOrEmpty(cmbWeightVar.Text))
            {
                sb.Append("WEIGHTVAR=");
                sb.Append(FieldNameNeedsBrackets(cmbWeightVar.Text) ? Util.InsertInSquareBrackets(cmbWeightVar.Text) : cmbWeightVar.Text);
                sb.Append(StringLiterals.SPACE);
            }

            if (!string.IsNullOrEmpty(txtOutput.Text))
            {
                sb.Append("OUTTABLE=");
                sb.Append(txtOutput.Text);
                sb.Append(StringLiterals.SPACE);
            }

            if (!string.IsNullOrEmpty(cmbGraphType.Text))
            {
                sb.Append("GRAPHTYPE=");
                sb.Append(StringLiterals.DOUBLEQUOTES).Append(cmbGraphType.Text).Append(StringLiterals.DOUBLEQUOTES);
                sb.Append(StringLiterals.SPACE);
            }

            //if (!string.IsNullOrEmpty(cmbConfLimits.Text))
            //{
            //    sb.Append("PVALUE=");
            //    sb.Append(cmbConfLimits.Text);
            //}

            //if (!string.IsNullOrEmpty(cmbStrataVar.Text))
            //{
            //    sb.Append("STRATAVAR=");
            //    foreach (string s in this.lbxStrataVar.Items)
            //    {
            //        sb.Append(s).Append(StringLiterals.SPACE);
            //    }
            //}
            CommandText = sb.ToString();
        }

        /// <summary>
        /// Determines if an object's value can be parsed to an int
        /// </summary>
        /// <returns>true if the value of the object is a number; else false</returns>
        public bool IsNumeric(object value)
        {
            var isNumeric = false;
            int actualValue;
            if (value != null && int.TryParse(value.ToString(), out actualValue))
            {
                isNumeric = true;
            }
            return isNumeric;
        }
        
        /// <summary>
        /// Validates user input
        /// </summary>
        /// <returns>true if there is no error; else false</returns>
        protected override bool ValidateInput()
        {
            base.ValidateInput();
            //if (cmbOther.Text.Length == 0)
            //{
            //    ErrorMessages.Add(SharedStrings.MUST_SELECT_TERMS);
            //}
            if (cmbCensoredVar.SelectedIndex == -1)            
            {
                ErrorMessages.Add(SharedStrings.MUST_SELECT_CENSORED);
            }
            if (cmbUncensoredValue.SelectedIndex == -1)
            {
                ErrorMessages.Add(SharedStrings.MUST_SELECT_UNCENSORED_VALUE);
            }
            if (cmbTimeVar.SelectedIndex == -1)
            {
                ErrorMessages.Add(SharedStrings.MUST_SELECT_TIME_VAR);
            }
            if (cmbGroupVar.SelectedIndex == -1)
            {
                ErrorMessages.Add(SharedStrings.MUST_SELECT_GROUP);
            }            
            
            // cmbStratifyby doubles as cmbMatchVar (which doesn't exist)
            //if (cbxMatch.Checked && string.IsNullOrEmpty(cmbStratifyBy.ToString()))
            //{
            //    ErrorMessages.Add(SharedStrings.NO_MATCHVAR);
            //}

            return (ErrorMessages.Count == 0);
        }

        #endregion Protected Methods

        #region Event Handlers

        /// <summary>
        /// Handles the btnClear_Click event
        /// </summary>
        /// <remarks>Removes all items from comboboxes and other dialog controls</remarks>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void btnClear_Click(object sender, System.EventArgs e)
        {
            //First column on KaplanMeierSurvivalDialog
            //variable cmb's cleared when dialog loads during FillVariableValues()
            cmbConfLimits.SelectedIndex = -1;
            txtOutput.Text = string.Empty;

            //Second column on KaplanMeierSurvivalDialog
            cmbUncensoredValue.Items.Clear();
            cmbTimeUnit.SelectedIndex = -1;

            lbxOther.Items.Clear(); //Predictor variables
            lbxInteractionTerms.Items.Clear();

            //Third column 
            lbxStrataVars.Items.Clear();

            //Fourth col
            lbxExtendedTerms.Items.Clear();

            //Last col
            cmbGraphType.Items.Clear();
            lbxGraphVars.Items.Clear();
            //chkCustomGraph;  Checkbox checked or unchecked when dialog cleared?
            KaplanMeierSurvivalDialog_Load(this, null);
            CheckForInputSufficiency();
        }

        /// <summary>
        /// Handles the cmbCensoredVar_SelectedIndexChanged event
        /// </summary>
        /// <remarks>Removes item from other comboboxes</remarks>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbCensoredVar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCensoredVar.SelectedIndex >= 0)   // prevent the remove below from re-entering
            {
                //Get the old variable chosen that was saved to the txt string.
                string strOld = txtCensoredVar;
                string strNew = cmbCensoredVar.Text;

                //make sure s isn't "" or the same as the new variable picked.
                if ((strOld.Length > 0) && (strOld != strNew))
                {
                    //add the former variable chosen back into the other lists
                    cmbTimeVar.Items.Add(strOld);
                    cmbGroupVar.Items.Add(strOld);
                    cmbWeightVar.Items.Add(strOld);
                    cmbOther.Items.Add(strOld);
                    cmbStrataVar.Items.Add(strOld);
                }

                //remove the new variable chosen from the other lists
                cmbTimeVar.Items.Remove(strNew);
                cmbGroupVar.Items.Remove(strNew);
                cmbWeightVar.Items.Remove(strNew);
                cmbOther.Items.Remove(strNew);
                cmbStrataVar.Items.Remove(strNew);
                lbxOther.Items.Remove(strNew);

                //check that recordset not null
                if (this.EpiInterpreter.Context.CurrentRead != null)
                {
                    List<System.Data.DataRow> Rows = this.EpiInterpreter.Context.GetOutput();
                    System.Data.DataTable tempTbl = this.EpiInterpreter.Context.DataSet.Tables["output"];
                    Dictionary<string, string> table = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    string type;
                    type = tempTbl.Columns[cmbCensoredVar.Text].DataType.ToString();


                    foreach (System.Data.DataRow row in tempTbl.Rows)
                    {
                        string Key = row[cmbCensoredVar.Text].ToString();
                        if(!table.ContainsKey(Key))
                        {
                            table.Add(Key, Key);
                        }
                    }

                    cmbUncensoredValue.Items.Clear();
                    Dictionary<string, string> setProperties = this.EpiInterpreter.Context.GetGlobalSettingProperties();

                    if (type.Equals("System.Byte") || type.Equals("System.Int16") || type.Equals("System.Boolean"))
                    {
                        cmbUncensoredValue.Items.Add(setProperties["RepresentationOfYes"]);
                        cmbUncensoredValue.Items.Add(setProperties["RepresentationOfNo"]);
                        if (! type.Equals("System.Boolean"))
                        {
                            cmbUncensoredValue.Items.Add(setProperties["RepresentationOfMissing"]);
                        }
                    }
                    else
                    {
                        int i = 0;
                        foreach(KeyValuePair<string,string> kvp in table)
                        {
                            if (kvp.Key.Length > 0)
                            {
                                cmbUncensoredValue.Items.Add(kvp.Key);
                            }

                            i++;
                        }
                        cmbUncensoredValue.Items.Add(setProperties["RepresentationOfMissing"]);

                        if (i > 2)
                        {
                            MsgBox.ShowInformation(SharedStrings.WARNING_HIGH_CENSOR_VAL_COUNT);
                        }
                    }

                    if (cmbUncensoredValue.Items.Count > 0)
                    {
                        cmbUncensoredValue.Enabled = true;
                    }
                }
            }
            CheckForInputSufficiency();
        }

        /// <summary>
        /// Handles the cmbTimeVar_SelectedIndexChanged event
        /// </summary>
        /// <remarks>Removes item from other comboboxes</remarks>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void cmbTimeVar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTimeVar.SelectedIndex >= 0)   // prevent the remove below from re-entering
            {
                //Get the old variable chosen that was saved to the txt string.
                string strOld = txtTimeVar;
                string strNew = cmbTimeVar.Text;

                //make sure s isn't "" or the same as the new variable picked.
                if ((strOld.Length > 0) && (strOld != strNew))
                {
                    //add the former variable chosen back into the other lists
                    cmbCensoredVar.Items.Add(strOld);
                    //cmbTimeVar.Items.Add(strOld);
                    cmbGroupVar.Items.Add(strOld);
                    cmbWeightVar.Items.Add(strOld);
                    cmbOther.Items.Add(strOld);
                    cmbStrataVar.Items.Add(strOld);
                }

                //remove the new variable chosen from the other lists
                cmbCensoredVar.Items.Remove(strNew);
                //cmbTimeVar.Items.Remove(strNew);
                cmbGroupVar.Items.Remove(strNew);
                cmbWeightVar.Items.Remove(strNew);
                cmbOther.Items.Remove(strNew);
                cmbStrataVar.Items.Remove(strNew);
                lbxOther.Items.Remove(strNew);
            }
            CheckForInputSufficiency();
        }

        /// <summary>
        /// Handles the lbxOther SelectedIndexChanged event
        /// </summary>
        /// <remarks>Removes item from other comboboxes</remarks>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>

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
            if (cmbOther.SelectedIndex >= 0)
            {
                //Get the old variable chosen that was saved to the txt string.
                string strOld = txtOtherVar;
                string strNew = cmbOther.Text;

                //make sure s isn't "" or the same as the new variable picked.
                if ((strOld.Length > 0) && (strOld != strNew))
                {
                    //add the former variable chosen back into the other lists
                    cmbCensoredVar.Items.Add(strOld);
                    cmbTimeVar.Items.Add(strOld);
                    cmbGroupVar.Items.Add(strOld);
                    cmbWeightVar.Items.Add(strOld);
                    //cmbOther.Items.Add(strOld);
                    cmbStrataVar.Items.Add(strOld);
                }

                //remove the new variable chosen from the other lists
                cmbCensoredVar.Items.Remove(strNew);
                cmbTimeVar.Items.Remove(strNew);
                cmbGroupVar.Items.Remove(strNew);
                cmbWeightVar.Items.Remove(strNew);
                //cmbOther.Items.Remove(strNew);
                cmbStrataVar.Items.Remove(strNew);
                lbxOther.Items.Remove(strNew);

                //add the new variable to the lbxOther ListBox
                if (!((lbxOther.Items.Contains(strNew)) || (lbxOther.Items.Contains("(" + strNew + ")"))))
                {
                    lbxOther.Items.Add(strNew);
                }
            }
            CheckForInputSufficiency();
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

                foreach (object obj in lbxOther.SelectedItems)
                {
                    string s = obj.ToString();

                    if (!(s.Contains("(") || (s.Contains(")"))))
                    {
                        interactionTerm = interactionTerm + s.ToString();
                        interactionTerm = interactionTerm + StringLiterals.SPACE;
                    }
                    else
                    {
                        return;
                    }
                }

                interactionTerm = interactionTerm.Trim();
                interactionTerm = interactionTerm.Replace(StringLiterals.SPACE, StringLiterals.STAR);
                lbxOther.SelectedItems.Clear();
                lbxInteractionTerms.Items.Add(interactionTerm);
            }
        }

        private void cmbGroupVar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbGroupVar.SelectedIndex >= 0)   // prevent the remove below from re-entering
            {
                //Get the old variable chosen that was saved to the txt string.
                string strOld = txtGroupVar;
                string strNew = cmbGroupVar.Text;

                //make sure s isn't "" or the same as the new variable picked.
                if ((strOld.Length > 0) && (strOld != strNew))
                {
                    //add the former variable chosen back into the other lists
                    cmbCensoredVar.Items.Add(strOld);
                    cmbTimeVar.Items.Add(strOld);
                    //cmbGroupVar.Items.Add(strOld);
                    cmbWeightVar.Items.Add(strOld);
                    cmbOther.Items.Add(strOld);
                    cmbStrataVar.Items.Add(strOld);
                }

                //remove the new variable chosen from the other lists
                cmbCensoredVar.Items.Remove(strNew);
                cmbTimeVar.Items.Remove(strNew);
                //cmbGroupVar.Items.Remove(strNew);
                cmbWeightVar.Items.Remove(strNew);
                cmbOther.Items.Remove(strNew);
                cmbStrataVar.Items.Remove(strNew);
                lbxOther.Items.Remove(strNew);
            }
            CheckForInputSufficiency();
        }
                
        private void cmbWeightVar_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Get the old variable chosen that was saved to the txt string.
            string strOld = txtWeightVar;
            string strNew = cmbWeightVar.Text;

            //make sure it isn't "" or the same as the new variable picked.
            if ((strOld.Length > 0) && (strOld != strNew))
            {
                //add the former variable chosen back into the other lists
                cmbCensoredVar.Items.Add(strOld);
                cmbTimeVar.Items.Add(strOld);
                cmbGroupVar.Items.Add(strOld);
                cmbOther.Items.Add(strOld);
                cmbStrataVar.Items.Add(strOld);
            }

            if (cmbWeightVar.SelectedIndex >= 0)   // prevent the remove below from re-entering
            {
                //remove the new variable chosen from the other lists
                cmbCensoredVar.Items.Remove(strNew);
                cmbTimeVar.Items.Remove(strNew);
                cmbGroupVar.Items.Remove(strNew);
                cmbOther.Items.Remove(strNew);
                cmbStrataVar.Items.Remove(strNew);
                lbxOther.Items.Remove(strNew);
            }
        }

        private void cmbStrataVar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbStrataVar.SelectedIndex >= 0)
            {
                //Get the old variable chosen that was saved to the txt string.
                string strOld = txtStrataVar;
                string strNew = cmbStrataVar.Text;

                //make sure s isn't "" or the same as the new variable picked.
                if ((strOld.Length > 0) && (strOld != strNew))
                {
                    //add the former variable chosen back into the other lists
                    cmbCensoredVar.Items.Add(strOld);
                    cmbTimeVar.Items.Add(strOld);
                    cmbGroupVar.Items.Add(strOld);
                    cmbWeightVar.Items.Add(strOld);
                    cmbOther.Items.Add(strOld);
                    //cmbStrataVar.Items.Add(strOld);
                }

                //remove the new variable chosen from the other lists
                cmbCensoredVar.Items.Remove(strNew);
                cmbTimeVar.Items.Remove(strNew);
                cmbGroupVar.Items.Remove(strNew);
                cmbWeightVar.Items.Remove(strNew);
                cmbOther.Items.Remove(strNew);
                //cmbStrataVar.Items.Remove(strNew);
                lbxOther.Items.Remove(strNew);
            }
        }
        
        private void lbxOther_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Delete)||(e.KeyCode == Keys.Back))
            {
                lbxOther.Items.Remove(lbxOther.SelectedItem);
            }
        }

        private void lbxStrataVars_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Delete) || (e.KeyCode == Keys.Back))
            {
                lbxStrataVars.Items.Remove(lbxStrataVars.SelectedItem);
            }
        }

        private void lbxInteractionTerms_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Delete) || (e.KeyCode == Keys.Back))
            {
                lbxInteractionTerms.Items.Remove(lbxInteractionTerms.SelectedItem);
            }
        }

        private void lbxExtendedTerms_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Delete) || (e.KeyCode == Keys.Back))
            {
                lbxExtendedTerms.Items.Remove(lbxExtendedTerms.SelectedItem);
            }
        }

        private void cmbCensoredVar_Click(object sender, EventArgs e)
        {
            if (cmbCensoredVar.SelectedIndex >= 0)  
            {
                txtCensoredVar = cmbCensoredVar.Text;
            }
            else
            {
                txtCensoredVar = "";
            }
        }

        private void cmbTimeVar_Click(object sender, EventArgs e)
        {
            if (cmbTimeVar.SelectedIndex >= 0) 
            {
                txtTimeVar = cmbTimeVar.Text;
            }
            else
            {
                txtTimeVar = "";
            }
        }

        private void cmbGroupVar_Click(object sender, EventArgs e)
        {
            if (cmbGroupVar.SelectedIndex >= 0)
            {
                txtGroupVar = cmbGroupVar.Text;
            }
            else
            {
                txtGroupVar = "";
            }
        }

        private void cmbWeightVar_Click(object sender, EventArgs e)
        {
            if (cmbWeightVar.SelectedIndex >= 0)
            {
                txtWeightVar = cmbWeightVar.Text;
            }
            else
            {
                txtWeightVar = "";
            }
        }

        private void cmbOther_Click(object sender, EventArgs e)
        {
            if (cmbOther.SelectedIndex >= 0)
            {
                txtOtherVar = cmbOther.Text;
            }
            else
            {
                txtOtherVar = "";
            }
        }

        private void cmbStrataVar_Click(object sender, EventArgs e)
        {
            if (cmbStrataVar.SelectedIndex >= 0)
            {
                txtStrataVar = cmbStrataVar.Text;
            }
            else
            {
                txtStrataVar = "";
            }
        }

        private void cmbWeightVar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                //SelectedIndexChanged will add the var back to the other DDLs
                cmbWeightVar.Text = "";
                cmbWeightVar.SelectedIndex = -1;
            }

        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-KMSURVIVAL.html");
        }

        #endregion Event Handlers

        private void cmbUncensoredValue_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckForInputSufficiency();
        }

    }
}
