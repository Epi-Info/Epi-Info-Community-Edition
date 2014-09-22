#region Namespaces

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Windows.Forms;
using Epi;
using Epi.Data.Services;
using Epi.Windows.Dialogs;
using VariableCollection = Epi.Collections.NamedObjectCollection<Epi.IVariable>;

#endregion  Namespaces

namespace Epi.Windows.Analysis.Dialogs
{
    /// <summary>
    /// Designs the ASSIGN command for makeView and Analysis
    /// </summary>
    public partial class AssignDialog : CommandDesignDialog
    {
        #region Constructors
        /// <summary>
        /// Constructor for the class
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public AssignDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor for AssignDialog
        /// </summary>
        /// <param name="frm">The main form</param>
        public AssignDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// Constructor for AssignDialog.  if showSave, enable the SaveOnly button
        /// </summary>
        /// <param name="frm">The main form</param>
        /// <param name="showSave">Boolean to denote whether to display Save Only button on dialog</param>
        public AssignDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm, bool showSave)
            : base(frm)
        {
            InitializeComponent();
            if (showSave)
            {
                showSaveOnly = true;
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
                
            }
            Construct();
        }

        #endregion Constructors

        #region Private Attributes
        private bool showSaveOnly = false;
        #endregion Private Attributes

        #region Public Methods

        /// <summary>
        /// Checks if input is sufficient and Enables control buttons accordingly
        /// </summary>
        public override void CheckForInputSufficiency()
        {
            bool inputValid = ValidateInput();
            btnOK.Enabled = inputValid;
            btnSaveOnly.Enabled = inputValid;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Loads the dialog
        /// </summary>
        private void Construct()
        {
            VariableType scopeWord = VariableType.Standard | VariableType.Global | VariableType.Permanent |
                                                 VariableType.DataSource | VariableType.DataSourceRedefined;
            FillVariableCombo(cmbAvailVar, scopeWord);
            FillVariableCombo(cmbAssignVar, scopeWord);
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            Configuration config = Configuration.GetNewInstance();
            Epi.DataSets.Config.SettingsRow settings = Configuration.GetNewInstance().Settings;
            this.btnMissing.Text = settings.RepresentationOfMissing;
            this.btnYes.Text = settings.RepresentationOfYes;
            this.btnNo.Text = settings.RepresentationOfNo;
        }

        /// <summary>
        /// Repositions buttons on dialog
        /// </summary>
        private void RepositionButtons()
        {
            int x = this.btnClear.Left;
            int y = btnClear.Top;
            btnClear.Location = new Point(btnFunctions.Left, y);
            btnFunctions.Location = new Point(btnCancel.Left, y);
            btnCancel.Location = new Point(btnOK.Left, y);
            btnOK.Location = new Point(btnSaveOnly.Left, y);
            btnSaveOnly.Location = new Point(x, y);
        }

        /// <summary>
        /// Returns TRUE if the count of doublequotes in a string is even.
        /// </summary>
        /// <param name="input">String input to check</param>
        /// <param name="stopPos">String position to end checking (start of selection to be replaced).</param>
        /// <returns> Returns TRUE if the count of doublequotes in the input string is even.</returns>
        private static bool QuoteCountIsEven(string input, int stopPos)
        {
            int result = 0;
            char q = '\"';
            for (int i = 0; i < stopPos; i++)
            {
                if (q == input[i]) result++;
            }
            return (0 == result % 2);
        }

        #endregion Private Methods

        #region Protected Methods
        /// <summary>
        /// Validates user input
        /// </summary>
        /// <returns>true if ErrorMessages.Count is 0; otherwise false</returns>
        protected override bool ValidateInput()
        {
            base.ValidateInput();

            if (string.IsNullOrEmpty(cmbAssignVar.Text.Trim()))
            {
                ErrorMessages.Add(SharedStrings.NO_ASSIGN_VAR);
            }
            if (string.IsNullOrEmpty(txtExpression.Text.Trim()))
            {
                ErrorMessages.Add(SharedStrings.NO_ASSIGN_EXPRESSION);
            }
            return (ErrorMessages.Count == 0);
        }

        /// <summary>
        /// Generates command text
        /// </summary>
        protected override void GenerateCommand()
        {
            StringBuilder command = new StringBuilder();
            command.Append(CommandNames.ASSIGN);
            command.Append(StringLiterals.SPACE);
            command.Append(FieldNameNeedsBrackets(cmbAssignVar.Text) ? Util.InsertInSquareBrackets(cmbAssignVar.Text) : cmbAssignVar.Text);
            if (!txtExpression.Text.Trim().StartsWith(StringLiterals.EQUAL))
            {
                command.Append(StringLiterals.SPACE + StringLiterals.EQUAL + StringLiterals.SPACE);
            }
            else
            {
                command.Append(StringLiterals.SPACE);
            }
            command.Append(ClearSpacesFromParens(txtExpression.Text));
            CommandText = command.ToString();
        }

        #endregion Protected methods

        #region Event Handlers

        /// <summary>
        /// Common event handler for build expression buttons
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void ClickHandler(object sender, System.EventArgs e)
        {
            Button btn = (Button)sender;
            string strInsert = StringLiterals.SPACE;
            if (txtExpression.SelectionLength > 0)
            {
                //Don't replace the trailing comma in 
                //  multi-parameter functions like YEARS or 
                //  trailing close paren if selected by mistake
                while (txtExpression.SelectionLength > 0 &&
                    (txtExpression.SelectedText.EndsWith(StringLiterals.SPACE) ||
                    txtExpression.SelectedText.EndsWith(StringLiterals.COMMA) ||
                    txtExpression.SelectedText.EndsWith(StringLiterals.PARANTHESES_CLOSE)))
                {
                    txtExpression.SelectionLength -= 1;
                }
            } 

            if ((string)btn.Tag == null)
            {
                if ((string)btn.Text == StringLiterals.DOUBLEQUOTES)
                {
                    //if the button is for a double quote, only add a leading space if there is 
                    //  an even number of quotes in the expression so far.

                    strInsert = ((QuoteCountIsEven(txtExpression.Text, txtExpression.SelectionStart)) ? StringLiterals.SPACE : string.Empty);
                }
                txtExpression.SelectedText = strInsert + (string)btn.Text;
            }
            else
            {
                if ((string)btn.Tag == StringLiterals.DOUBLEQUOTES)
                {
                    //if the button is for a double quote, only add a leading space if there is 
                    //  an odd number of quotes in the expression so far.
                    strInsert = ((QuoteCountIsEven(txtExpression.Text, txtExpression.SelectionStart)) ? StringLiterals.SPACE : string.Empty);
                }
                txtExpression.SelectedText = strInsert + (string)btn.Tag;
            }            
            //Remove spaces within (+) (.) and (-) as any spaces within the parentheses
            //  will cause an error.
            txtExpression.Text = ClearSpacesFromParens(txtExpression.Text);
            txtExpression.Select(txtExpression.Text.Length, 0);
            txtExpression.Focus();
        }

        /// <summary>
        /// Removes any embedded spaces within parentheses that could cause errors with
        ///  yes (+), no (.), and missing (.) and trims leading/trailing spaces.
        ///  also from less than or equal, greater than or equal, and not equal;  
        /// </summary>
        /// <param name="input">Input string to search</param>
        private string ClearSpacesFromParens(string input)
        {
            string strCheckedA;
            string strCheckedB;
            strCheckedA = Regex.Replace(input, @"\s+\(\s+\+\s\)", " (+)"); //removes spaces within ( + ) to make (+) yes
            strCheckedB = Regex.Replace(strCheckedA, @"\s+\(\s+\-\s+\)", " (-)");  //removes spaces in (-) no
            strCheckedA = Regex.Replace(strCheckedB, @"\s+\(\s+\.\s+\)", " (.)");  //removes spaces in (.) missing
            strCheckedB = Regex.Replace(strCheckedA, @"\s+\<\s+\=", " <=");  //removes spaces in <= less than or equal
            strCheckedA = Regex.Replace(strCheckedB, @"\s+\>\s+\=", " >=");  // >  = becomes >=  greater than or equal
            strCheckedB = Regex.Replace(strCheckedA, @"\s+\<\s+\>", " <>");  //  <  >  becomes <> not equal
            strCheckedA = Regex.Replace(strCheckedB, @"\s+", " ");  // consecutive spaces "    " become " " one
            return strCheckedA.Trim();
        }

        /// <summary>
        /// Handles the Change event of the index of the assigned variables combobox
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void cbxAssignVariable_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnAssignmentOrVariableChanged();
        }

        /// <summary>
        /// Handles the text changed event of the assignment textbox
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtAssignment_TextChanged(object sender, EventArgs e)
        {
            OnAssignmentOrVariableChanged();
        }

        /// <summary>
        /// Enables the OK button depending upon user selection or input
        /// </summary>
        private void OnAssignmentOrVariableChanged()
        {
            /*if (!string.IsNullOrEmpty(cmbAssignVar.Text))
            {
                txtExpression.Text = cmbAssignVar.Text + " = ";
            }*/
            btnOK.Enabled = (cmbAssignVar.SelectedItem != null && !string.IsNullOrEmpty(txtExpression.Text));
        }

        /// <summary>
        /// Handles the Event change event for items on the dialog
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void SomethingChanged(object sender, EventArgs e)
        {
            CheckForInputSufficiency();
        }

        /// <summary>
        /// Loads the Assign Dialog
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void AssignDialog_Load(object sender, EventArgs e)
        {
            btnSaveOnly.Visible = showSaveOnly;
            if (showSaveOnly)
            {
                RepositionButtons();
            }
        }

        /// <summary>
        /// Clears the dialog
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnClear_Click(object sender, EventArgs e)
        {
            txtExpression.Text = string.Empty;
            cmbAssignVar.SelectedIndex = -1;
            cmbAvailVar.SelectedIndex = -1;
        }

        /// <summary>
        /// Handles the Change event of the index of the available variables combobox
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void cmbAvailVar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(cmbAvailVar.Text))
            {
                string AvailableVarString = (FieldNameNeedsBrackets(cmbAvailVar.Text) ? Util.InsertInSquareBrackets(cmbAvailVar.Text) : cmbAvailVar.Text);
                if (!(txtExpression.TextLength.Equals(0)))
                {
                    AvailableVarString = StringLiterals.SPACE + AvailableVarString;
                }
                if (txtExpression.SelectionLength > 0)
                {
                    //Don't replace the trailing comma in 
                    //  multi-parameter functions like YEARS or 
                    //  trailing close paren if selected by mistake
                    while (txtExpression.SelectionLength > 0 && 
                        (txtExpression.SelectedText.EndsWith(StringLiterals.SPACE) || 
                        txtExpression.SelectedText.EndsWith(StringLiterals.COMMA) || 
                        txtExpression.SelectedText.EndsWith(StringLiterals.PARANTHESES_CLOSE)))
                    {
                        txtExpression.SelectionLength -= 1;
                    }
                } 
                //int quoteCount
                txtExpression.SelectedText = AvailableVarString;
                txtExpression.Focus();
                txtExpression.Text = ClearSpacesFromParens(txtExpression.Text);
                txtExpression.Select(txtExpression.Text.Length, 0);
            }
        }

        /// <summary>
        /// Handles the MouseDown event of the Function f(x) button 
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnFunction_MouseDown(object sender, MouseEventArgs e)
        {
            cmsFunction.Show((Control)sender, e.Location);
        }

        /// <summary>
        /// Handles the Click event of the  Functions and Operators context menu
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void FXClickHandler(object sender, System.EventArgs e)
        {
            ToolStripMenuItem FXItem = (ToolStripMenuItem)sender;
            if (txtExpression.SelectionLength > 0)
            {
                //Don't replace the trailing comma in 
                //  multi-parameter functions like YEARS or 
                //  trailing close paren if selected by mistake
                while (txtExpression.SelectionLength > 0 && 
                    (txtExpression.SelectedText.EndsWith(StringLiterals.SPACE) || 
                    txtExpression.SelectedText.EndsWith(StringLiterals.COMMA) || 
                    txtExpression.SelectedText.EndsWith(StringLiterals.PARANTHESES_CLOSE)))
                {
                    txtExpression.SelectionLength -= 1;
                }
            } 
            txtExpression.SelectedText = StringLiterals.SPACE + FXItem.ToolTipText + StringLiterals.SPACE;
            txtExpression.Text = ClearSpacesFromParens(txtExpression.Text);
            txtExpression.Select(txtExpression.Text.Length, 0);
            txtExpression.Focus();
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://wwwn.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-assign.html");
        }

        #endregion Event Handlers
    }
}

