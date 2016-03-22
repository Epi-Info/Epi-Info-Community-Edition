#region //Namespaces
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Epi;
using Epi.Data.Services;
using Epi.Windows.Dialogs;
using EpiInfo.Plugin;
using System.Collections.Generic;


#endregion //Namespaces

namespace Epi.Windows.MakeView.Dialogs.CheckCodeCommandDialogs
{
	/// <summary>
	/// ***** This is a wireframe and currently contains no functionality *****
	/// </summary>
    public partial class VariableAssignmentDialog : CheckCodeDesignDialog
    {
        #region Constructors
        
        /// <summary>
        /// Constructor for the class
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public VariableAssignmentDialog()
		{
			InitializeComponent();
        }

        /// <summary>
        /// Constructor for the Variable Assignment dialog
        /// </summary>
        /// <param name="frm">The main form</param>
		public VariableAssignmentDialog(MainForm frm) : base(frm)
        {
            InitializeComponent();
            this.EpiInterpreter = ((Epi.Windows.MakeView.Forms.MakeViewMainForm)frm).EpiInterpreter;
        }

        #endregion  //Constructors

        #region Public Properties
        /// <summary>
        /// Sets the View for the dialog
        /// </summary>
        public override View View
        {
            set
            {
            
                VariableType scopeWord = VariableType.Standard | VariableType.Global |
                                                 VariableType.DataSource | VariableType.DataSourceRedefined | VariableType.Permanent;
                
                List<EpiInfo.Plugin.IVariable> vars = this.EpiInterpreter.Context.GetVariablesInScope((VariableScope)scopeWord);
                foreach (EpiInfo.Plugin.IVariable var in vars)
                {
                    if (!(var is Epi.Fields.PredefinedDataField) && !(var is Fields.LabelField) && !(var is Fields.MirrorField))
                    {
                        cbxAvailVariables.Items.Add(var.Name);
                        cbxAssignVariable.Items.Add(var.Name);
                    }
                }  
                              
                /*
                foreach (Fields.Field field in value.Fields)
                {
                    //--EI27
                   //  if (field is Fields.RenderableField && !(field is Fields.LabelField))
                    if (field is Fields.RenderableField && !(field is Fields.LabelField) && !(field is Fields.MirrorField))
                    {
                        cbxAvailVariables.Items.Add(field.Name);
                        cbxAssignVariable.Items.Add(field.Name);
                    }
                    //--EI-99
                    if (field is Fields.FirstSaveTimeField || field is Fields.LastSaveTimeField)
                       { cbxAvailVariables.Items.Add(field.Name); }
                   //--
                }  */
            }
        }
        #endregion  //Public Properties

        #region Private Event Handlers

  
        /// <summary>
        /// Handles the Click event of the OK button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnOk_Click(object sender, System.EventArgs e)
        {
            //Output = "ASSIGN " + cbxAssignVariable.SelectedItem.ToString() + " = " + txtAssignment.Text;
            WordBuilder command = new WordBuilder();

            command.Append(CommandNames.ASSIGN);
            command.Append(cbxAssignVariable.SelectedItem.ToString());
            command.Append("=");
            // Fix for defect #140 - prevent the resulting command from having two = signs if the user supplies one by accident.
            string expression = txtAssignment.Text.Trim();
            if (expression.Length > 0)
            {
                if (expression[0] == '=')
                {
                    expression = expression.Remove(0, 1);
                    expression = expression.Trim();
                }
            }
            command.Append(expression);
            Output = command.ToString();
            this.DialogResult = DialogResult.OK;
            this.Hide();
        }

        /// <summary>
        /// Cancel button closes this dialog 
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            cbxAssignVariable.Text = string.Empty;
            txtAssignment.Text = string.Empty;
            cbxAvailVariables.Text = string.Empty;
        }

        /// <summary>
        /// Handles the MouseDown event of the If Function button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnFunction_MouseDown(object sender, MouseEventArgs e)
        {
            BuildFunctionContextMenu().Show((Control)sender, e.Location);
        }


        #region Deleted Code
        //=======XXXXXXXXXXXXXXXX=======vvvvvvvvvv=============
        
        ///// <summary>
        ///// Handles the Click event of the Plus Sign button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnPlus_Click(object sender, EventArgs e)
        //{
        //    int position = txtAssignment.SelectionStart;
        //    txtAssignment.Text = txtAssignment.Text.Insert(position, "+ ");
        //    txtAssignment.Select(position + 2, 0);   
        //}

        ///// <summary>
        ///// Handles the Click event of the Minus Sign button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnMinus_Click(object sender, EventArgs e)
        //{
        //    int position = txtAssignment.SelectionStart;
        //    txtAssignment.Text = txtAssignment.Text.Insert(position, "- ");
        //    txtAssignment.Select(position + 2, 0);   
        //}

        ///// <summary>
        ///// Handles the Click event of the Multiplication button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnTimes_Click(object sender, EventArgs e)
        //{
        //    int position = txtAssignment.SelectionStart;
        //    txtAssignment.Text = txtAssignment.Text.Insert(position, "* ");
        //    txtAssignment.Select(position + 2, 0);   
        //}

        ///// <summary>
        ///// Handles the Click event of the Division Sign button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnDivide_Click(object sender, EventArgs e)
        //{
        //    int position = txtAssignment.SelectionStart;
        //    txtAssignment.Text = txtAssignment.Text.Insert(position, "/ ");
        //    txtAssignment.Select(position + 2, 0);   
        //}

        ///// <summary>
        ///// Handles the Click event of the Equal sign button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnEqual_Click(object sender, EventArgs e)
        //{
        //    int position = txtAssignment.SelectionStart;
        //    txtAssignment.Text = txtAssignment.Text.Insert(position, "= ");
        //    txtAssignment.Select(position + 2, 0);   
        //}

        ///// <summary>
        ///// Handles the Click event of the Less Than button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnLess_Click(object sender, EventArgs e)
        //{
        //    int position = txtAssignment.SelectionStart;
        //    txtAssignment.Text = txtAssignment.Text.Insert(position, "< ");
        //    txtAssignment.Select(position + 2, 0);   
        //}

        ///// <summary>
        ///// Handles the Click event of the Greater Than button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnGreat_Click(object sender, EventArgs e)
        //{
        //    int position = txtAssignment.SelectionStart;
        //    txtAssignment.Text = txtAssignment.Text.Insert(position, "> ");
        //    txtAssignment.Select(position + 2, 0);   
        //}

        ///// <summary>
        ///// Handles the Click event of the Ampersand button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnAmp_Click(object sender, EventArgs e)
        //{
        //    int position = txtAssignment.SelectionStart;
        //    txtAssignment.Text = txtAssignment.Text.Insert(position, "& ");
        //    txtAssignment.Select(position + 2, 0);   
        //}

        ///// <summary>
        ///// Handles the Click event of the Double Quotes button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnQuote_Click(object sender, EventArgs e)
        //{
        //    int position = txtAssignment.SelectionStart;
        //    txtAssignment.Text = txtAssignment.Text.Insert(position, "\"");
        //    txtAssignment.Select(position + 1, 0);   
        //}

        ///// <summary>
        ///// Handles the Click event of the Open Parenthesis button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnOpenParen_Click(object sender, EventArgs e)
        //{
        //    int position = txtAssignment.SelectionStart;
        //    txtAssignment.Text = txtAssignment.Text.Insert(position, "( ");
        //    txtAssignment.Select(position + 2, 0);   
        //}

        ///// <summary>
        ///// Handles the Click event of the Close Parenthesis button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnCloseParen_Click(object sender, EventArgs e)
        //{
        //    int position = txtAssignment.SelectionStart;
        //    txtAssignment.Text = txtAssignment.Text.Insert(position, ") ");
        //    txtAssignment.Select(position + 2, 0);   

        //    //int length = txtAssignment.Text.Length;            
        //    //if (length - 6 >= 0 &&
        //    //    txtAssignment.Text[length - 3] == ' ' &&
        //    //    (txtAssignment.Text[length - 4] == '.' || txtAssignment.Text[length - 4] == '-' || txtAssignment.Text[length - 4] == '+') &&
        //    //    txtAssignment.Text[length - 5] == ' ' &&
        //    //    txtAssignment.Text[length - 6] == '(')
        //    //{
        //    //    txtAssignment.Text = txtAssignment.Text.Remove(length - 3, 1);
        //    //    txtAssignment.Text = txtAssignment.Text.Remove(length - 5, 1);
        //    //}
        //    //txtAssignment.Select(txtAssignment.Text.Length, 0);

        //    //txtAssignment.Focus();
        //}

        ///// <summary>
        ///// Handles the Click event of the And button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnAnd_Click(object sender, EventArgs e)
        //{
        //    int position = txtAssignment.SelectionStart;
        //    txtAssignment.Text = txtAssignment.Text.Insert(position, "AND ");
        //    txtAssignment.Select(position + 4, 0);   
        //}

        ///// <summary>
        ///// Handles the Click event of the Or button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnOr_Click(object sender, EventArgs e)
        //{
        //    int position = txtAssignment.SelectionStart;
        //    txtAssignment.Text = txtAssignment.Text.Insert(position, "OR ");
        //    txtAssignment.Select(position + 3, 0);   
        //}

        ///// <summary>
        ///// Handles the Click event of the Yes button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnYes_Click(object sender, EventArgs e)
        //{
        //    int position = txtAssignment.SelectionStart;
        //    txtAssignment.Text = txtAssignment.Text.Insert(position, "(+) ");
        //    txtAssignment.Select(position + 4, 0);   
        //}

        ///// <summary>
        ///// Handles the Click event of the No button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnNo_Click(object sender, EventArgs e)
        //{
        //    int position = txtAssignment.SelectionStart;
        //    txtAssignment.Text = txtAssignment.Text.Insert(position, "(-) ");
        //    txtAssignment.Select(position + 4, 0);   
        //}

        ///// <summary>
        ///// Handles the Click event of the Missing button
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void btnMissing_Click(object sender, EventArgs e)
        //{
        //    int position = txtAssignment.SelectionStart;
        //    txtAssignment.Text = txtAssignment.Text.Insert(position, "(.) ");
        //    txtAssignment.Select(position + 4, 0);   
        //}


        //=======XXXXXXXXXXXXXXXX====^^^^^^^^^^^^^===========

        #endregion

        /// <summary>
        /// Method for testing whether or not a field name is a reserved word or 
        /// contains spaces requireing it be wrapped in square brackets
        /// </summary>
        protected virtual bool FieldNameNeedsBrackets(string strTestName)
        {
            Regex regex = new Regex("[\\w\\d]", RegexOptions.IgnoreCase);
            string strResultOfSymbolTest = regex.Replace(strTestName, string.Empty);
            return (strResultOfSymbolTest.Length > 0) | AppData.Instance.IsReservedWord(strTestName) | !(strTestName.Equals(Util.Squeeze(strTestName)));
        }


        /// <summary>
        /// Handles the Index Change event of the available variables combobox
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void cbxAvailVariables_SelectedIndexChanged(object sender, EventArgs e)
        {
            int position = txtAssignment.SelectionStart;
            txtAssignment.Text = txtAssignment.Text.Insert(position, cbxAvailVariables.SelectedItem.ToString() + " ");
            txtAssignment.Select(position + cbxAvailVariables.SelectedItem.ToString().Length + 1, 0);
        }

        /// <summary>
        /// Handles the Index Change event of the assigned variables combobox
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void cbxAssignVariable_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnAssignmentOrVariableChanged();
        }

        /// <summary>
        /// Handles the Text Changed event of the assignment textbox
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtAssignment_TextChanged(object sender, EventArgs e)
        {
            OnAssignmentOrVariableChanged();
        }

        /// <summary>
        /// Handles the Click event of the  Functions and Operators context menu
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void FXClickHandler(object sender, System.EventArgs e)
        {
            ToolStripMenuItem FXItem = (ToolStripMenuItem)sender;

            if (txtAssignment.SelectionLength > 0)
            {
                //Don't replace the trailing comma in 
                //  multi-parameter functions like YEARS or 
                //  trailing close paren if selected by mistake
                while (txtAssignment.SelectionLength > 0 &&
                    (txtAssignment.SelectedText.EndsWith(StringLiterals.SPACE) ||
                    txtAssignment.SelectedText.EndsWith(StringLiterals.COMMA) ||
                    txtAssignment.SelectedText.EndsWith(StringLiterals.PARANTHESES_CLOSE)))
                {
                    txtAssignment.SelectionLength -= 1;
                }
            }
            txtAssignment.SelectedText = StringLiterals.SPACE + FXItem.ToolTipText + StringLiterals.SPACE;
            txtAssignment.Text = ClearSpacesFromParens(txtAssignment.Text);
            txtAssignment.Select(txtAssignment.Text.Length, 0);
            txtAssignment.Focus();
        }
                   
        /// <summary>
        /// Builds a context menu for functions and operators
        /// </summary>
        /// <returns>A context menu for functions and operators</returns>
        private ContextMenuStrip BuildFunctionContextMenu()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            //==================================================================
            ToolStripMenuItem mnuOperators = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMCOPS);
            ToolStripMenuItem mnuExponent = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMCOPS_EXP);
            mnuExponent.ToolTipText = StringLiterals.CARET;
            mnuExponent.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuMOD = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMCOPS_MOD);
            mnuMOD.ToolTipText = CommandNames.MOD;
            mnuMOD.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuGT = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMCOPS_GT);
            mnuGT.ToolTipText = StringLiterals.GREATER_THAN;
            mnuGT.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuGTE = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMCOPS_GTE);
            mnuGTE.ToolTipText = StringLiterals.GREATER_THAN_OR_EQUAL;
            mnuGTE.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuEqual = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMCOPS_EQUAL);
            mnuEqual.ToolTipText = StringLiterals.EQUAL;
            mnuEqual.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuNE = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMCOPS_LTGT);
            mnuNE.ToolTipText = StringLiterals.LESS_THAN_OR_GREATER_THAN;
            mnuNE.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuLT = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMCOPS_LT);
            mnuLT.ToolTipText = StringLiterals.LESS_THAN;
            mnuLT.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuLTE = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMCOPS_LTE);
            mnuLTE.ToolTipText = StringLiterals.LESS_THAN_OR_EQUAL;
            mnuLTE.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuLIKE = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMCOPS_LIKE);
            mnuLIKE.ToolTipText = CommandNames.LIKE;
            mnuLIKE.Click += new EventHandler(FXClickHandler);
            //======================================================================
            ToolStripMenuItem mnuBools = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_LGOPS);
            ToolStripMenuItem mnuAND = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_LGOPS_AND);
            mnuAND.ToolTipText = CommandNames.AND;
            mnuAND.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuOR = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_LGOPS_OR);
            mnuOR.ToolTipText = CommandNames.OR;
            mnuOR.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuXOR = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_LGOPS_XOR);
            mnuXOR.ToolTipText = CommandNames.XOR;
            mnuXOR.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuNOT = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_LGOPS_NOT);
            mnuNOT.ToolTipText = CommandNames.NOT;
            mnuNOT.Click += new EventHandler(FXClickHandler);
            //=====================================================================
            ToolStripMenuItem mnuNums = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMFX);
            ToolStripMenuItem mnuEXP = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMFX_EXP);
            mnuEXP.ToolTipText = CommandNames.EXP + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuEXP.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuSIN = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMFX_SIN);
            mnuSIN.ToolTipText = CommandNames.SIN + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuSIN.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuCOS = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMFX_COS);
            mnuCOS.ToolTipText = CommandNames.COS + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuCOS.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuTAN = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMFX_TAN);
            mnuTAN.ToolTipText = CommandNames.TAN + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuTAN.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuLOG = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMFX_LOG);
            mnuLOG.ToolTipText = CommandNames.LOG + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuLOG.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuLN = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMFX_LN);
            mnuLN.ToolTipText = CommandNames.LN + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuLN.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuABS = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMFX_ABS);
            mnuABS.ToolTipText = CommandNames.ABS + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuABS.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuRND = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMFX_RND);
            mnuRND.ToolTipText = CommandNames.RND + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuRND.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuTRUNC = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMFX_TRUNC);
            mnuTRUNC.ToolTipText = CommandNames.TRUNC + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuTRUNC.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuROUND = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMFX_ROUND);
            mnuROUND.ToolTipText = CommandNames.ROUND + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuROUND.Click += new EventHandler(FXClickHandler);

            //ToolStripMenuItem mnuNTD = new ToolStripMenuItem("NUMTODATE");
            //mnuNTD.ToolTipText = "NUMTODATE( <year>, <month>, <day> )";
            //mnuNTD.Click += new EventHandler(FXClickHandler);

            //ToolStripMenuItem mnuNTT = new ToolStripMenuItem("NUMTOTIME");
            //mnuNTT.ToolTipText = "NUMTOTIME( <hour>, <minute>, <second> )";
            //mnuNTT.Click += new EventHandler(FXClickHandler);
            //===================================================================
            ToolStripMenuItem mnuDates = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_DATFX);
            ToolStripMenuItem mnuYRS = new ToolStripMenuItem(CommandNames.YEARS);
            mnuYRS.ToolTipText = CommandNames.YEARS + SharedStrings.CNTXT_FXN_DATFX_TMPLT2;
            mnuYRS.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuMOS = new ToolStripMenuItem(CommandNames.MONTHS);
            mnuMOS.ToolTipText = CommandNames.MONTHS + SharedStrings.CNTXT_FXN_DATFX_TMPLT2;
            mnuMOS.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuDYS = new ToolStripMenuItem(CommandNames.DAYS);
            mnuDYS.ToolTipText = CommandNames.DAYS + SharedStrings.CNTXT_FXN_DATFX_TMPLT2;
            mnuDYS.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuYR = new ToolStripMenuItem(CommandNames.YEAR);
            mnuYR.ToolTipText = CommandNames.YEAR + SharedStrings.CNTXT_FXN_DATFX_TMPLT1;
            mnuYR.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuMO = new ToolStripMenuItem(CommandNames.MONTH);
            mnuMO.ToolTipText = CommandNames.MONTH + SharedStrings.CNTXT_FXN_DATFX_TMPLT1;
            mnuMO.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuDY = new ToolStripMenuItem(CommandNames.DAY);
            mnuDY.ToolTipText = CommandNames.DAY + SharedStrings.CNTXT_FXN_DATFX_TMPLT1;
            mnuDY.Click += new EventHandler(FXClickHandler);

            //===================================================================
            ToolStripMenuItem mnuSys = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_SYSFXN);
            ToolStripMenuItem mnuSYSTEMDATE = new ToolStripMenuItem(CommandNames.SYSTEMDATE);
            mnuSYSTEMDATE.ToolTipText = CommandNames.SYSTEMDATE;
            mnuSYSTEMDATE.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuSYSTEMTIME = new ToolStripMenuItem(CommandNames.SYSTEMTIME);
            mnuSYSTEMTIME.ToolTipText = CommandNames.SYSTEMTIME;
            mnuSYSTEMTIME.Click += new EventHandler(FXClickHandler);

            //ToolStripMenuItem mnuENVIRON = new ToolStripMenuItem("ENVIRON");
            //mnuENVIRON.ToolTipText = "ENVIRON( <name_of_env_variable> )";
            //mnuENVIRON.Click += new EventHandler(FXClickHandler);

            //ToolStripMenuItem mnuEXISTS = new ToolStripMenuItem("EXISTS");
            //mnuEXISTS.ToolTipText = "EXISTS( <filename> )";
            //mnuEXISTS.Click += new EventHandler(FXClickHandler);

            //ToolStripMenuItem mnuFILEDATE = new ToolStripMenuItem("FILEDATE");
            //mnuFILEDATE.ToolTipText = "FILEDATE";
            //mnuFILEDATE.Click += new EventHandler(FXClickHandler);

            //===================================================================
            ToolStripMenuItem mnuTimes = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_TIMEFXN);
            ToolStripMenuItem mnuHOURS = new ToolStripMenuItem(CommandNames.HOURS);
            mnuHOURS.ToolTipText = CommandNames.HOURS + SharedStrings.CNTXT_FXN_DATFX_TMPLT2;
            mnuHOURS.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuMINUTES = new ToolStripMenuItem("MINUTES");
            mnuMINUTES.ToolTipText = CommandNames.MINUTES + SharedStrings.CNTXT_FXN_DATFX_TMPLT2;
            mnuMINUTES.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuSECONDS = new ToolStripMenuItem("SECONDS");
            mnuSECONDS.ToolTipText = CommandNames.SECONDS + SharedStrings.CNTXT_FXN_DATFX_TMPLT2;
            mnuSECONDS.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuHOUR = new ToolStripMenuItem("HOUR");
            mnuHOUR.ToolTipText = CommandNames.HOUR + SharedStrings.CNTXT_FXN_DATFX_TMPLT1;
            mnuHOUR.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuMINUTE = new ToolStripMenuItem("MINUTE");
            mnuMINUTE.ToolTipText = CommandNames.MINUTE + SharedStrings.CNTXT_FXN_DATFX_TMPLT1;
            mnuMINUTE.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuSECOND = new ToolStripMenuItem("SECOND");
            mnuSECOND.ToolTipText = CommandNames.SECOND + SharedStrings.CNTXT_FXN_DATFX_TMPLT1;
            mnuSECOND.Click += new EventHandler(FXClickHandler);

            //==================================================================
            ToolStripMenuItem mnuTxts = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_TEXTFXN);
            ToolStripMenuItem mnuTXTTONUM = new ToolStripMenuItem(CommandNames.TXTTONUM);
            mnuTXTTONUM.ToolTipText = CommandNames.TXTTONUM + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuTXTTONUM.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuTXTTODATE = new ToolStripMenuItem(CommandNames.TXTTODATE);
            mnuTXTTODATE.ToolTipText = CommandNames.TXTTODATE + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuTXTTODATE.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuSUBSTRING = new ToolStripMenuItem(CommandNames.SUBSTRING);
            mnuSUBSTRING.ToolTipText = CommandNames.SUBSTRING + SharedStrings.CNTXT_FXN_TMPLT_SUBSTRING;
            mnuSUBSTRING.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuUPPERCASE = new ToolStripMenuItem(CommandNames.UPPERCASE);
            mnuUPPERCASE.ToolTipText = CommandNames.UPPERCASE + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuUPPERCASE.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuFINDTEXT = new ToolStripMenuItem(CommandNames.FINDTEXT);
            mnuFINDTEXT.ToolTipText = CommandNames.FINDTEXT + SharedStrings.CNTXT_FXN_TMPLT_FINDTEXT;
            mnuFINDTEXT.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuFORMAT = new ToolStripMenuItem(CommandNames.FORMAT);
            mnuFORMAT.ToolTipText = CommandNames.FORMAT + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuFORMAT.Click += new EventHandler(FXClickHandler);

            //=======================================================================
            ToolStripMenuItem mnuDATEFORMATS = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_DATES);
            ToolStripMenuItem mnuGeneralDate = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_DATES_GEN);
            mnuGeneralDate.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.GENERAL_DATE_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuGeneralDate.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuLongDate = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_DATES_LONGDATE);
            mnuLongDate.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.LONG_DATE_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuLongDate.Click += new EventHandler(FXClickHandler);

            //ToolStripMenuItem mnuMediumDate = new ToolStripMenuItem("Medium Date");
            //mnuMediumDate.ToolTipText = "FORMAT( <variable>, \"Medium Date\")";
            //mnuMediumDate.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuShortDate = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_DATES_SHORTDATE);
            mnuShortDate.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.SHORT_DATE_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuShortDate.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuLongTime = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_DATES_LONGTIME);
            mnuLongTime.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.LONG_TIME_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuLongTime.Click += new EventHandler(FXClickHandler);

            //ToolStripMenuItem mnuMediumTime = new ToolStripMenuItem("Medium Time");
            //mnuMediumTime.ToolTipText = "FORMAT( <variable>, \"Medium Time\")";
            //mnuMediumTime.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuShortTime = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_DATES_SHORTTIME);
            mnuShortTime.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.SHORT_TIME_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuShortTime.Click += new EventHandler(FXClickHandler);

            //===========================================================================
            ToolStripMenuItem mnuNumberFormats = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_NUM);
            ToolStripMenuItem mnuGeneralNumber = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_NUM_GEN);
            mnuGeneralNumber.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.GENERAL_NUMBER_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuGeneralNumber.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuCurrency = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_NUM_CURRENCY);
            mnuCurrency.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.CURRENCY_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuCurrency.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuFixed = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_NUM_FIXED);
            mnuFixed.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.FIXED_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuFixed.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuStandard = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_NUM_STANDARD);
            mnuStandard.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.STANDARD_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuStandard.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuPercent = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_NUM_PERCENT);
            mnuPercent.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.PERCENT_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuPercent.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuScientific = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_NUM_SCIENTIFIC);
            mnuScientific.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.SCIENTIFIC_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuScientific.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuYN = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_NUM_YN);
            mnuYN.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.YESNO_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuYN.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuTF = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_NUM_TF);
            mnuTF.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.TRUE_FALSE_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuTF.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuOnOff = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_FRMT_NUM_ONOFF);
            mnuOnOff.ToolTipText = CommandNames.FORMAT +
                StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                CommandNames.ON_OFF_FORMAT + StringLiterals.PARANTHESES_CLOSE;
            mnuOnOff.Click += new EventHandler(FXClickHandler);

            contextMenu.Items.Add(mnuOperators);
            contextMenu.Items.Add(mnuBools);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.AddRange(new ToolStripMenuItem[] { mnuNums, mnuDates, mnuSys, mnuTimes, mnuTxts });

            mnuOperators.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuExponent, mnuMOD, mnuGT, mnuGTE, mnuEqual, mnuNE, mnuLT, mnuLTE, mnuLIKE });
            mnuBools.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuAND, mnuOR, mnuXOR, mnuNOT });
            mnuNums.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuEXP, mnuSIN, mnuCOS, mnuTAN, mnuLOG, mnuLN, mnuABS, mnuRND, mnuTRUNC, mnuROUND });
            mnuDates.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuYRS, mnuMOS, mnuDYS, mnuYR, mnuMO, mnuDY });
            mnuSys.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuSYSTEMDATE, mnuSYSTEMTIME });
            mnuTimes.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuHOURS, mnuMINUTES, mnuSECONDS, mnuHOUR, mnuMINUTE, mnuSECOND });
            mnuTxts.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuTXTTONUM, mnuTXTTODATE, mnuSUBSTRING, mnuUPPERCASE, mnuFINDTEXT, mnuFORMAT });

            mnuFORMAT.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuDATEFORMATS, mnuNumberFormats });
            mnuDATEFORMATS.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuGeneralDate, mnuLongDate, mnuShortDate, mnuLongTime, mnuShortTime });
            mnuNumberFormats.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuGeneralNumber, mnuCurrency, mnuFixed, mnuStandard, mnuPercent, mnuScientific, mnuYN, mnuTF, mnuOnOff });
            return contextMenu;
        }
        
        /// <summary>
        /// Common event handler for build expression buttons
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void ClickHandler(object sender, System.EventArgs e)
        {
            Button btn = (Button)sender;
            string strInsert = StringLiterals.SPACE;
            if (txtAssignment.SelectionLength > 0)
            {
                //Don't replace the trailing comma in 
                //  multi-parameter functions like YEARS or 
                //  trailing close paren if selected by mistake
                while (txtAssignment.SelectionLength > 0 &&
                    (txtAssignment.SelectedText.EndsWith(StringLiterals.SPACE) ||
                    txtAssignment.SelectedText.EndsWith(StringLiterals.COMMA) ||
                    txtAssignment.SelectedText.EndsWith(StringLiterals.PARANTHESES_CLOSE)))
                {
                    txtAssignment.SelectionLength -= 1;
                }
            }

            if ((string)btn.Tag == null)
            {
                if ((string)btn.Text == StringLiterals.DOUBLEQUOTES)
                {
                    //if the button is for a double quote, only add a leading space if there is 
                    //  an even number of quotes in the expression so far.

                    strInsert = ((QuoteCountIsEven(txtAssignment.Text, txtAssignment.SelectionStart)) ? StringLiterals.SPACE : string.Empty);
                }
                txtAssignment.SelectedText = strInsert + (string)btn.Text;
            }
            else
            {
                if ((string)btn.Tag == StringLiterals.DOUBLEQUOTES)
                {
                    //if the button is for a double quote, only add a leading space if there is 
                    //  an odd number of quotes in the expression so far.
                    strInsert = ((QuoteCountIsEven(txtAssignment.Text, txtAssignment.SelectionStart)) ? StringLiterals.SPACE : string.Empty);
                }
                txtAssignment.SelectedText = strInsert + (string)btn.Tag;
            }
            //Remove spaces within (+) (.) and (-) as any spaces within the parentheses
            //  will cause an error.
            txtAssignment.Text = ClearSpacesFromParens(txtAssignment.Text);
            txtAssignment.Select(txtAssignment.Text.Length, 0);
            txtAssignment.Focus();
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
            return strCheckedB.Trim();
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

        #endregion  //Private Event Handlers

        #region Private Methods

        /// <summary>
        /// Enables or disables OK button depending upon selection and/or entry
        /// </summary>
        private void OnAssignmentOrVariableChanged()
        {
            btnOk.Enabled = (cbxAssignVariable.SelectedItem != null && !string.IsNullOrEmpty(txtAssignment.Text));
        }

        #endregion  //Private Methods

    }
}

