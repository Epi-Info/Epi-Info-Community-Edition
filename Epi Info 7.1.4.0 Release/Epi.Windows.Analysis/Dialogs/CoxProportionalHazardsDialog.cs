using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using Epi.Analysis;
using Epi.Data.Services;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;

namespace Epi.Windows.Analysis.Dialogs
{
    /// <summary>
    /// Dialog for CoxPH command
    /// </summary>
    public partial class CoxProportionalHazardsDialog : CommandDesignDialog
    {
        #region Constructor

        /// <summary>
        /// Default constructor - NOT TO BE USED FOR INSTANTIATION
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public CoxProportionalHazardsDialog()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// CoxProportionalHazardsDialog constructor
        /// </summary>
        /// <param name="frm"></param>
        public CoxProportionalHazardsDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
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

        List<string> lstGraphVars = new List<string>();
        private string txtGraphType = "Survival Probability";
        private bool bolCustomizeGraph = false;

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

        private void CoxProportionalHazardsDialog_Load(object sender, EventArgs e)
        {
            VariableType scopeWord = VariableType.DataSource | VariableType.DataSourceRedefined |
                                        VariableType.Standard;

            if (this.cmbCensoredVar.Text.Length > 0)
            {
                this.cmbCensoredVar.Text = null;
            }

            FillVariableCombo(this.cmbCensoredVar, scopeWord);
            FillVariableCombo(this.cmbTimeVar, scopeWord);
            FillVariableCombo(this.cmbGroupVar, scopeWord);
            FillVariableCombo(this.cmbWeightVar, scopeWord);
            this.cmbWeightVar.SelectedIndex = -1;
            FillVariableCombo(this.cmbOther, scopeWord);
            FillVariableCombo(this.cmbStrataVar, scopeWord);

            this.cmbTimeUnit.Items.Add("");
            this.cmbTimeUnit.Items.Add("Days");
            this.cmbTimeUnit.Items.Add("Hours");
            this.cmbTimeUnit.Items.Add("Months");
            this.cmbTimeUnit.Items.Add("Weeks");
            this.cmbTimeUnit.Items.Add("Years");

            this.cmbConfLimits.Text = "95%";
            this.txtGraphType = "Survival Probability";
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
            sb.Append(CommandNames.COXPH);
            sb.Append(StringLiterals.SPACE);
            sb.Append(FieldNameNeedsBrackets(cmbTimeVar.Text) ? Util.InsertInSquareBrackets(cmbTimeVar.Text) : cmbTimeVar.Text); 
            sb.Append(StringLiterals.SPACE);
            sb.Append(StringLiterals.EQUAL);
            sb.Append(StringLiterals.SPACE);
            sb.Append(StringLiterals.PARANTHESES_OPEN);
            sb.Append(FieldNameNeedsBrackets(cmbGroupVar.Text) ? Util.InsertInSquareBrackets(cmbGroupVar.Text) : cmbGroupVar.Text);
            //sb.Append(cmbGroupVar.Text);
            sb.Append(StringLiterals.PARANTHESES_CLOSE);
            sb.Append(StringLiterals.SPACE);

            foreach (object obj in lbxOther.Items)
            {
                string s = obj.ToString();
                sb.Append(FieldNameNeedsBrackets(s) ? Util.InsertInSquareBrackets(s) : s); 
                sb.Append(StringLiterals.SPACE);
            }

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
                //System.Data.DataTable table = new Epi.Data.cDataSetHelper().SelectDistinct("TableForCombo", tempTbl, cmbCensoredVar.Text);
                //System.Data.DataTable table = Epi.Data.DBReadExecute.GetDataTable(this.EpiInterpreter.Context.CurrentRead.File, "Select Distinct [" + cmbCensoredVar.Text + "] from [" + this.EpiInterpreter.Context.CurrentRead.Identifier + "]");
                //System.Data.DataTable table = Epi.Data.DBReadExecute.GetDataTable(this.EpiInterpreter.Context.CurrentRead.File, "Select Distinct [" + cmbCensoredVar.Text + "] " + this.EpiInterpreter.Context.CurrentProject.GetViewByName(this.EpiInterpreter.Context.CurrentRead.Identifier).FromViewSQL);

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
                sb.Append(CommandNames.TIMEUNIT);
                sb.Append(StringLiterals.EQUAL);
                sb.Append(StringLiterals.DOUBLEQUOTES).Append(cmbTimeUnit.Text).Append(StringLiterals.DOUBLEQUOTES);
                sb.Append(StringLiterals.SPACE);
            }

            if (!string.IsNullOrEmpty(cmbWeightVar.Text))
            {
                sb.Append(CommandNames.WEIGHTVAR);
                sb.Append(StringLiterals.EQUAL);
                sb.Append(FieldNameNeedsBrackets(cmbWeightVar.Text) ? Util.InsertInSquareBrackets(cmbWeightVar.Text) : cmbWeightVar.Text);
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
                sb.Append(StringLiterals.SPACE);
            }

            if (!string.IsNullOrEmpty(cmbStrataVar.Text))
            {
                sb.Append(CommandNames.STRATAVAR);
                sb.Append(StringLiterals.EQUAL);
                sb.Append(FieldNameNeedsBrackets(cmbStrataVar.Text) ? Util.InsertInSquareBrackets(cmbStrataVar.Text) : cmbStrataVar.Text);
                sb.Append(StringLiterals.SPACE);
            }

            if (bolCustomizeGraph)
            {
                sb.Append(CommandNames.DIALOG);
                sb.Append(StringLiterals.SPACE);
            }

            sb.Append(CommandNames.GRAPH);
            sb.Append(StringLiterals.EQUAL);
            foreach (string var in lstGraphVars)
            {
                sb.Append(FieldNameNeedsBrackets(var) ? Util.InsertInSquareBrackets(var) : var);
                sb.Append(StringLiterals.SPACE);
            }

            if (this.txtGraphType.Length > 0)
            {
                sb.Append(CommandNames.GRAPHTYPE);
                sb.Append(StringLiterals.EQUAL);
                sb.Append(StringLiterals.DOUBLEQUOTES);
                sb.Append(txtGraphType);
                sb.Append(StringLiterals.DOUBLEQUOTES);
                sb.Append(StringLiterals.SPACE);
            }

            CommandText = sb.ToString();
        }
                
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
            //Must select a Censored variable and a Value for Uncensored cases
            //Cannot have either one be blank.
            if (cmbCensoredVar.SelectedIndex == -1)
            {
                ErrorMessages.Add(SharedStrings.MUST_SELECT_CENSORED);
            }
            if (cmbUncensoredValue.SelectedIndex == -1)
            {
                ErrorMessages.Add(SharedStrings.MUST_SELECT_UNCENSORED_VALUE);
            }
            //Must select a Time variable
            if (cmbTimeVar.SelectedIndex == -1)
            {
                   ErrorMessages.Add(SharedStrings.MUST_SELECT_TIME_VAR);
            }
            //Must select either a GroupVar or one or more Predictors (OtherVars), or both.
            //Cannot have both GroupVar and lbxOther be blank.
            if ((cmbGroupVar.SelectedIndex == -1) && (lbxOther.Items.Count == 0))
            {
                ErrorMessages.Add(SharedStrings.MUST_SELECT_GROUP_OR_PREDICTOR);
            }

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
            //First column on CoxProportionalHazardsDialog
            cmbCensoredVar.Items.Clear();
            cmbTimeVar.Items.Clear();
            cmbGroupVar.Items.Clear();
            cmbWeightVar.Items.Clear();
            cmbConfLimits.Items.Clear();
            txtOutput.Text = string.Empty;

            //Second column on CoxProportionalHazardsDialog
            cmbUncensoredValue.Items.Clear();
            cmbTimeUnit.Items.Clear();
            cmbOther.Items.Clear();  //Predictor variables
            lbxOther.Items.Clear();
            lbxInteractionTerms.Items.Clear();

            //Third column 
            cmbStrataVar.Items.Clear();
            lbxStrataVars.Items.Clear();

            //Fourth col
            lbxExtendedTerms.Items.Clear();

            //Graph Options
            lstGraphVars.Clear();
            txtGraphType = "Survival Probability";
            bolCustomizeGraph = false;
            
            CoxProportionalHazardsDialog_Load(this, null);

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
                string s = txtCensoredVar;

                //make sure s isn't "" or the same as the new variable picked.
                if ((s.Length != 0) && (s != cmbCensoredVar.Text))
                {
                    //add the former variable chosen back into the other lists
                    cmbTimeVar.Items.Add(s);
                    cmbGroupVar.Items.Add(s);
                    cmbWeightVar.Items.Add(s);
                    cmbOther.Items.Add(s);
                    cmbStrataVar.Items.Add(s);
                }

                //remove the new variable chosen from the other lists
                s = cmbCensoredVar.Text;
                cmbTimeVar.Items.Remove(s);
                cmbGroupVar.Items.Remove(s);
                cmbWeightVar.Items.Remove(s);
                cmbOther.Items.Remove(s);
                cmbStrataVar.Items.Remove(s);
                lbxOther.Items.Remove(s);

                //check that recordset not null

                if (this.EpiInterpreter.Context.CurrentRead != null)
                {
                    List<System.Data.DataRow> Rows = this.EpiInterpreter.Context.GetOutput();
                    System.Data.DataTable tempTbl = this.EpiInterpreter.Context.DataSet.Tables["output"];
                    Dictionary<string, string> table = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    foreach (System.Data.DataRow row in Rows)
                    {
                        string Key = row[cmbCensoredVar.Text].ToString();
                        if (!table.ContainsKey(Key))
                        {
                            table.Add(Key, Key);
                        }
                    }

                    //System.Data.DataTable table = Epi.Data.DBReadExecute.GetDataTable(this.EpiInterpreter.Context.CurrentRead.File, "Select Distinct [" + cmbCensoredVar.Text + "] from [" + this.EpiInterpreter.Context.CurrentRead.Identifier + "]");
                    //System.Data.DataTable table = Epi.Data.DBReadExecute.GetDataTable(this.EpiInterpreter.Context.CurrentRead.File, "Select Distinct [" + cmbCensoredVar.Text + "] " + this.EpiInterpreter.Context.CurrentProject.GetViewByName(this.EpiInterpreter.Context.CurrentRead.Identifier).FromViewSQL);

                    string type;
                    type = tempTbl.Columns[cmbCensoredVar.Text].DataType.ToString(); 
                    //this.EpiInterpreter.Context.CurrentDataTable.Columns(kvp.Key).DataType.ToString()
                    //If type.Equals("System.Byte") 
                    cmbUncensoredValue.Items.Clear();
                    Dictionary<string, string> setProperties = this.EpiInterpreter.Context.GetGlobalSettingProperties();

                    if (type.Equals("System.Byte") || type.Equals("System.Int16") || type.Equals("System.Boolean"))
                    {
                        cmbUncensoredValue.Items.Add(setProperties["RepresentationOfYes"]);
                        cmbUncensoredValue.Items.Add(setProperties["RepresentationOfNo"]);
                        if (!type.Equals("System.Boolean"))
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
                string s = "";
                //Get the old variable chosen that was saved to the txt string.
                s = txtTimeVar.ToString();

                //make sure it isn't "" or the same as the new variable picked.
                if ((s.Length != 0) && (s != cmbTimeVar.Text))
                {
                    //add the former variable chosen back into the other lists
                    cmbCensoredVar.Items.Add(s);
                    cmbGroupVar.Items.Add(s);
                    cmbWeightVar.Items.Add(s);
                    cmbOther.Items.Add(s);
                    cmbStrataVar.Items.Add(s);
                }

                //remove the new variable chosen from the other lists
                s = cmbTimeVar.Text;
                cmbCensoredVar.Items.Remove(s);
                cmbGroupVar.Items.Remove(s);
                cmbWeightVar.Items.Remove(s);
                cmbOther.Items.Remove(s);
                cmbStrataVar.Items.Remove(s);
                lbxOther.Items.Remove(s);
            }
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
                string s = "";
                //Get the old variable chosen that was saved to the txt string.
                s = txtOtherVar.ToString();

                //make sure it isn't "" or the same as the new variable picked.
                if ((s.Length != 0) && (s != cmbOther.Text))
                {
                    //add the former variable chosen back into the other lists
                    cmbCensoredVar.Items.Add(s);
                    cmbTimeVar.Items.Add(s);
                    cmbGroupVar.Items.Add(s);
                    cmbWeightVar.Items.Add(s);
                    cmbStrataVar.Items.Add(s);
                }

                //remove the new variable chosen from the other lists
                s = cmbOther.Text;
                cmbCensoredVar.Items.Remove(s);
                cmbTimeVar.Items.Remove(s);
                cmbGroupVar.Items.Remove(s);
                cmbOther.Items.Remove(s);
                cmbWeightVar.Items.Remove(s);
                cmbStrataVar.Items.Remove(s);
                //add the new variable to the lbxOther ListBox
                if (!((lbxOther.Items.Contains(s)) || (lbxOther.Items.Contains("(" + s + ")"))))
                {
                    lbxOther.Items.Add(s);
                }
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
                string s = "";
                //Get the old variable chosen that was saved to the txt string.
                s = txtGroupVar.ToString();

                //make sure it isn't "" or the same as the new variable picked.
                if ((s.Length != 0) && (s != cmbGroupVar.Text))
                {
                    //add the former variable chosen back into the other lists
                    cmbCensoredVar.Items.Add(s);
                    cmbTimeVar.Items.Add(s);
                    cmbWeightVar.Items.Add(s);
                    cmbOther.Items.Add(s);
                    cmbStrataVar.Items.Add(s);
                    lstGraphVars.Remove(s);
                }

                //remove the new variable chosen from the other lists
                s = cmbGroupVar.Text;
                cmbCensoredVar.Items.Remove(s);
                cmbTimeVar.Items.Remove(s);
                cmbWeightVar.Items.Remove(s);
                cmbOther.Items.Remove(s);
                cmbStrataVar.Items.Remove(s);
                lbxOther.Items.Remove(s);
                if (!lstGraphVars.Contains(s))
                {
                    lstGraphVars.Add(s);
                }
            }
        }
                
        private void cmbWeightVar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbWeightVar.SelectedIndex >= 0)   // prevent the remove below from re-entering
            {
                string s = "";
                //Get the old variable chosen that was saved to the txt string.
                s = txtWeightVar.ToString();

                //make sure it isn't "" or the same as the new variable picked.
                if ((s.Length != 0) && (s != cmbWeightVar.Text))
                {
                    //add the former variable chosen back into the other lists
                    cmbCensoredVar.Items.Add(s);
                    cmbTimeVar.Items.Add(s);
                    cmbGroupVar.Items.Add(s);
                    cmbOther.Items.Add(s);
                    cmbStrataVar.Items.Add(s);
                }

                //remove the new variable chosen from the other lists
                s = cmbWeightVar.Text;
                cmbCensoredVar.Items.Remove(s);
                cmbTimeVar.Items.Remove(s);
                cmbGroupVar.Items.Remove(s);
                cmbOther.Items.Remove(s);
                cmbStrataVar.Items.Remove(s);
                lbxOther.Items.Remove(s);
            }
        }

        private void cmbStrataVar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbStrataVar.SelectedIndex >= 0)
            {
                string s = "";
                //Get the old variable chosen that was saved to the txt string.
                s = txtStrataVar.ToString();
                
                //make sure it isn't "" or the same as the new variable picked.
                if ((s.Length != 0) && (s != cmbStrataVar.Text))
                {
                    //add the former variable chosen back into the other lists
                    cmbCensoredVar.Items.Add(s);
                    cmbTimeVar.Items.Add(s);
                    cmbGroupVar.Items.Add(s);
                    cmbWeightVar.Items.Add(s);
                    cmbOther.Items.Add(s);
                    lstGraphVars.Remove(s);
                }
                //remove the new variable chosen from the other lists
                s = cmbStrataVar.Text;
                cmbCensoredVar.Items.Remove(s);
                cmbTimeVar.Items.Remove(s);
                cmbGroupVar.Items.Remove(s);
                cmbWeightVar.Items.Remove(s);
                cmbOther.Items.Remove(s);
                lbxOther.Items.Remove(s);
                if (!lstGraphVars.Contains(s))
                {
                    lstGraphVars.Add(s);
                }
                
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

        #endregion Event Handlers

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

        private void cmbUncensoredValue_SelectedIndexChanged(object sender, EventArgs e)
        {
            //CheckForInputSufficiency();
        }

        private void cmbCensoredVar_TextChanged(object sender, EventArgs e)
        {
            CheckForInputSufficiency();
        }

        private void cmbTimeVar_TextChanged(object sender, EventArgs e)
        {
            CheckForInputSufficiency();
        }

        private void cmbGroupVar_TextChanged(object sender, EventArgs e)
        {
            CheckForInputSufficiency();
        }

        private void cmbUncensoredValue_TextChanged(object sender, EventArgs e)
        {
            CheckForInputSufficiency();
        }

        private void cmbOther_TextChanged(object sender, EventArgs e)
        {
            CheckForInputSufficiency();
        }

        private void btnOptions_Click(object sender, EventArgs e)
        {
            CoxPHGraphOptionsDialog GOD = new CoxPHGraphOptionsDialog((Epi.Windows.Analysis.Forms.AnalysisMainForm)mainForm, this.lstGraphVars, this.txtGraphType, this.bolCustomizeGraph);
            GOD.ShowDialog();
            this.lstGraphVars = GOD.lstGraphVars;
            this.txtGraphType = GOD.txtGraphType;
            this.bolCustomizeGraph = GOD.bolCustomizeGraph;
            GOD.Close();
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://wwwn.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-coxph.html");
        }

    }
}