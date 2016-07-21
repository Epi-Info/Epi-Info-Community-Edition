using System;
using System.Data;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Epi;
using Epi.Analysis;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;

namespace Epi.Windows.Analysis.Dialogs
{
	/// <summary>
	/// Dialog for Select command
	/// </summary>
    public partial class SelectDialog : CommandDesignDialog
	{
        int cursorPosition = 0;
		#region Constructor	

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public SelectDialog()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Construcotr for Select Dialog
        /// </summary>
        /// <param name="frm"></param>
        public SelectDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
		{
			InitializeComponent();
            Construct();
		}
		#endregion Constructor

		#region Event Handlers
		/// <summary>
		/// Common click event handler for build expression buttons
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void ClickHandler(object sender, System.EventArgs e)
		{
            //Control btn = (Control)sender;

            //if ((string)btn.Tag  == null)
            //{
            //    txtSelectCriteria.Text = txtSelectCriteria.Text.Insert(cursorPosition, btn.Text);
            //    txtSelectCriteria.Focus();
            //    txtSelectCriteria.Select(this.cursorPosition + btn.Text.Length + 1, 0);
            //}
            //else
            //{
            //    txtSelectCriteria.Text = txtSelectCriteria.Text.Insert(cursorPosition, (string)btn.Tag);
            //    txtSelectCriteria.Focus();
            //    txtSelectCriteria.Select(this.cursorPosition + btn.Tag.ToString().Length + 1, 0);
            //}

            Button btn = (Button)sender;
            string strInsert = StringLiterals.SPACE;
            if (txtSelectCriteria.SelectionLength > 0)
            {
                //Don't replace the trailing comma in 
                //  multi-parameter functions like YEARS or 
                //  trailing close paren if selected by mistake
                while (txtSelectCriteria.SelectionLength > 0 &&
                    (txtSelectCriteria.SelectedText.EndsWith(StringLiterals.SPACE) ||
                    txtSelectCriteria.SelectedText.EndsWith(StringLiterals.COMMA) ||
                    txtSelectCriteria.SelectedText.EndsWith(StringLiterals.PARANTHESES_CLOSE)))
                {
                    txtSelectCriteria.SelectionLength -= 1;
                }
            }

            if ((string)btn.Tag == null)
            {
                if ((string)btn.Text == StringLiterals.DOUBLEQUOTES)
                {
                    //if the button is for a double quote, only add a leading space if there is 
                    //  an even number of quotes in the expression so far.

                    strInsert = ((QuoteCountIsEven(txtSelectCriteria.Text, txtSelectCriteria.SelectionStart)) ? StringLiterals.SPACE : string.Empty);
                }
                txtSelectCriteria.SelectedText = strInsert + (string)btn.Text;
            }
            else
            {
                if ((string)btn.Tag == StringLiterals.DOUBLEQUOTES)
                {
                    //if the button is for a double quote, only add a leading space if there is 
                    //  an odd number of quotes in the expression so far.
                    strInsert = ((QuoteCountIsEven(txtSelectCriteria.Text, txtSelectCriteria.SelectionStart)) ? StringLiterals.SPACE : string.Empty);
                }
                txtSelectCriteria.SelectedText = strInsert + (string)btn.Tag;
            }
            //Remove spaces within (+) (.) and (-) as any spaces within the parentheses
            //  will cause an error.
            txtSelectCriteria.Text = ClearSpacesFromParens(txtSelectCriteria.Text);
            txtSelectCriteria.Select(txtSelectCriteria.Text.Length, 0);
            txtSelectCriteria.Focus();

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

		/// <summary>
		/// Populate txtSelectCriteria with selected value from cmbAvailable
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cmbAvailableVar_SelectedIndexChanged(object sender, System.EventArgs e)
		{
            if (!string.IsNullOrEmpty(cmbAvailableVar.Text))
            {
                string AvailableVarString = (FieldNameNeedsBrackets(cmbAvailableVar.Text) ? Util.InsertInSquareBrackets(cmbAvailableVar.Text) : cmbAvailableVar.Text);
                //insert a space before the variable only when:
                //  * textbox has length and 
                //  * the insertion point is at the end and 
                //  * nothing is selected and
                //  * there isn't already a space at the end and 
                //  * if there is a quote at the end it is an even (closing) quote (but not if its an odd quote)
                if ((txtSelectCriteria.TextLength > 0 && txtSelectCriteria.SelectionStart.Equals(txtSelectCriteria.TextLength)) && (txtSelectCriteria.SelectionLength.Equals(0)))
                {
                    if (txtSelectCriteria.Text.EndsWith("\""))
                    {
                        if (QuoteCountIsEven(txtSelectCriteria.Text, txtSelectCriteria.SelectionStart))
                        {
                            AvailableVarString = StringLiterals.SPACE + AvailableVarString;
                        }
                    }
                    else
                    {
                        AvailableVarString = StringLiterals.SPACE + AvailableVarString;
                    }
                }

                if (txtSelectCriteria.SelectionLength > 0)
                {
                    //Don't replace the trailing comma in 
                    //  multi-parameter functions like YEARS or 
                    //  trailing close paren if selected by mistake
                    while (txtSelectCriteria.SelectionLength > 0 &&
                        (txtSelectCriteria.SelectedText.EndsWith(StringLiterals.SPACE) ||
                        txtSelectCriteria.SelectedText.EndsWith(StringLiterals.COMMA) ||
                        txtSelectCriteria.SelectedText.EndsWith(StringLiterals.PARANTHESES_CLOSE)))
                    {
                        txtSelectCriteria.SelectionLength -= 1;
                    }
                }
                txtSelectCriteria.SelectedText = AvailableVarString;
                txtSelectCriteria.Focus();
                txtSelectCriteria.Text = ClearSpacesFromParens(txtSelectCriteria.Text);
                txtSelectCriteria.Select(txtSelectCriteria.Text.Length, 0);
                CheckForInputSufficiency();
            }
		}

        /// <summary>
		/// Clears user input
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void btnClear_Click(object sender, System.EventArgs e)
		{			
			txtSelectCriteria.Text = string.Empty;
			cmbAvailableVar.Text = string.Empty;
			CheckForInputSufficiency();
		}
		private void SelectDialog_Load(object sender, System.EventArgs e)
		{		
			LoadVariables();
			//Select is also used to cancel select. Enable OK button
			CheckForInputSufficiency();
		
		}
		private void txtSelectCriteria_Leave(object sender, System.EventArgs e)
		{
            cursorPosition = txtSelectCriteria.SelectionStart;
			CheckForInputSufficiency();
		}

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-select.html");
        }

        #endregion //Event Handlers

        #region Public Methods
        /// <summary>
        /// Sets the enabled property of OK and SaveOnly button
        /// </summary>
        public override void CheckForInputSufficiency()
        {
            bool inputValid = ValidateInput();
            btnOK.Enabled = inputValid;
            btnSaveOnly.Enabled = inputValid;
        }
        #endregion

        #region Protected Methods
        /// <summary>
		/// Generates command test
		/// </summary>
		protected override void GenerateCommand()
		{
			WordBuilder command = new WordBuilder();
			command.Append((CommandNames.SELECT).ToUpperInvariant());
			command.Append(txtSelectCriteria.Text.Trim());
			CommandText = command.ToString() ;
		}

		#endregion //Protected Methods

		#region Private Methods

        private void Construct()
        {
            if (!this.DesignMode)
            {
                Configuration config = Configuration.GetNewInstance();
                Epi.DataSets.Config.SettingsRow settings = Configuration.GetNewInstance().Settings;
                this.btnMissing.Text = settings.RepresentationOfMissing;
                this.btnYes.Text = settings.RepresentationOfYes;
                this.btnNo.Text = settings.RepresentationOfNo;

                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
            }
        }

        private void LoadVariables()
		{
            VariableType scopeWord = VariableType.DataSource | VariableType.Standard |
                                      VariableType.Global | VariableType.Permanent;

            FillVariableCombo(cmbAvailableVar, scopeWord);
			cmbAvailableVar.SelectedIndex = -1;
			this.cmbAvailableVar.SelectedIndexChanged += new System.EventHandler(this.cmbAvailableVar_SelectedIndexChanged);
			
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
        #endregion Private Methods

        #region Function Context Menu
        /// <summary>
        /// Handles the Click event of the  Functions and Operators context menu
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void FXClickHandler(object sender, System.EventArgs e)
        {
            ToolStripMenuItem FXItem = (ToolStripMenuItem)sender;

            if (txtSelectCriteria.SelectionLength > 0)
            {
                //Don't replace the trailing comma in 
                //  multi-parameter functions like YEARS or 
                //  trailing close paren if selected by mistake
                while (txtSelectCriteria.SelectionLength > 0 &&
                    (txtSelectCriteria.SelectedText.EndsWith(StringLiterals.SPACE) ||
                    txtSelectCriteria.SelectedText.EndsWith(StringLiterals.COMMA) ||
                    txtSelectCriteria.SelectedText.EndsWith(StringLiterals.PARANTHESES_CLOSE)))
                {
                    txtSelectCriteria.SelectionLength -= 1;
                }
            }
            txtSelectCriteria.SelectedText = StringLiterals.SPACE + FXItem.ToolTipText + StringLiterals.SPACE;
            txtSelectCriteria.Text = ClearSpacesFromParens(txtSelectCriteria.Text);
            txtSelectCriteria.Select(txtSelectCriteria.Text.Length, 0);
            txtSelectCriteria.Focus();
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

            ToolStripMenuItem mnuSTEP = new ToolStripMenuItem(CommandNames.STEP);
            mnuSTEP.ToolTipText = CommandNames.STEP + StringLiterals.PARANTHESES_OPEN + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.COMMA + StringLiterals.SPACE +
                SharedStrings.CNTXT_FXN_TMPLT_VAR_PARM + StringLiterals.SPACE + StringLiterals.PARANTHESES_CLOSE;
            mnuSTEP.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuTRUNC = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMFX_TRUNC);
            mnuTRUNC.ToolTipText = CommandNames.TRUNC + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuTRUNC.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuROUND = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_NUMFX_ROUND);
            mnuROUND.ToolTipText = CommandNames.ROUND + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuROUND.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuNTD = new ToolStripMenuItem(CommandNames.NUMTODATE);
            mnuNTD.ToolTipText = CommandNames.NUMTODATE + SharedStrings.CNTXT_FXN_TMPLT_DATEPARTS;
            mnuNTD.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuNTT = new ToolStripMenuItem(CommandNames.NUMTOTIME);
            mnuNTT.ToolTipText = CommandNames.NUMTOTIME + SharedStrings.CNTXT_FXN_TMPLT_TIMEPARTS;
            mnuNTT.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuPFROMZ = new ToolStripMenuItem(CommandNames.PFROMZ);
            mnuPFROMZ.ToolTipText = CommandNames.PFROMZ + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuPFROMZ.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuZSCORE = new ToolStripMenuItem(CommandNames.ZSCORE);
            mnuZSCORE.ToolTipText = CommandNames.ZSCORE + SharedStrings.CNTXT_FXN_TMPLT_ZSCORE;
            mnuZSCORE.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuRECORDCOUNT = new ToolStripMenuItem(CommandNames.RECORDCOUNT);
            mnuRECORDCOUNT.ToolTipText = CommandNames.RECORDCOUNT;
            mnuRECORDCOUNT.Click += new EventHandler(FXClickHandler);

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
            ToolStripMenuItem mnuCurrUser = new ToolStripMenuItem(SharedStrings.CNTXT_FXN_SYSFXN_CURRUSER);
            mnuCurrUser.ToolTipText = CommandNames.CURRENTUSER + StringLiterals.PARANTHESES_OPEN +
                StringLiterals.SPACE + StringLiterals.PARANTHESES_CLOSE;
            mnuCurrUser.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuEXISTS = new ToolStripMenuItem(CommandNames.EXISTS);
            mnuEXISTS.ToolTipText = CommandNames.EXISTS + SharedStrings.CNTXT_FXN_TMPLT_FILENAME;
            mnuEXISTS.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuFILEDATE = new ToolStripMenuItem(CommandNames.FILEDATE);
            mnuFILEDATE.ToolTipText = CommandNames.FILEDATE + SharedStrings.CNTXT_FXN_TMPLT_FILENAME;
            mnuFILEDATE.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuSYSTEMDATE = new ToolStripMenuItem(CommandNames.SYSTEMDATE);
            mnuSYSTEMDATE.ToolTipText = CommandNames.SYSTEMDATE;
            mnuSYSTEMDATE.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuSYSTEMTIME = new ToolStripMenuItem(CommandNames.SYSTEMTIME);
            mnuSYSTEMTIME.ToolTipText = CommandNames.SYSTEMTIME;
            mnuSYSTEMTIME.Click += new EventHandler(FXClickHandler);

            //ToolStripMenuItem mnuENVIRON = new ToolStripMenuItem("ENVIRON");
            //mnuENVIRON.ToolTipText = "ENVIRON( <name_of_env_variable> )";
            //mnuENVIRON.Click += new EventHandler(FXClickHandler);

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

            ToolStripMenuItem mnuSTRLEN = new ToolStripMenuItem(CommandNames.STRLEN);
            mnuSTRLEN.ToolTipText = CommandNames.STRLEN + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuSTRLEN.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuUPPERCASE = new ToolStripMenuItem(CommandNames.UPPERCASE);
            mnuUPPERCASE.ToolTipText = CommandNames.UPPERCASE + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuUPPERCASE.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuFINDTEXT = new ToolStripMenuItem(CommandNames.FINDTEXT);
            mnuFINDTEXT.ToolTipText = CommandNames.FINDTEXT + SharedStrings.CNTXT_FXN_TMPLT_FINDTEXT;
            mnuFINDTEXT.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuFORMAT = new ToolStripMenuItem(CommandNames.FORMAT);
            mnuFORMAT.ToolTipText = CommandNames.FORMAT + SharedStrings.CNTXT_FXN_TMPLT_VARIABLE;
            mnuFORMAT.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuLINEBREAK = new ToolStripMenuItem(CommandNames.LINEBREAK);
            mnuLINEBREAK.ToolTipText = CommandNames.LINEBREAK + StringLiterals.PARANTHESES_OPEN +
                StringLiterals.SPACE + StringLiterals.PARANTHESES_CLOSE;
            mnuLINEBREAK.Click += new EventHandler(FXClickHandler);
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
            mnuNums.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuABS, mnuEXP, mnuLN, mnuLOG, mnuNTD, mnuNTT, mnuRECORDCOUNT, mnuRND, mnuROUND, mnuSTEP, mnuSIN, mnuCOS, mnuTAN, mnuTRUNC, mnuPFROMZ, mnuZSCORE });
            mnuDates.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuYRS, mnuMOS, mnuDYS, mnuYR, mnuMO, mnuDY });
            mnuSys.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuCurrUser, mnuEXISTS, mnuFILEDATE, mnuSYSTEMDATE, mnuSYSTEMTIME });
            mnuTimes.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuHOURS, mnuMINUTES, mnuSECONDS, mnuHOUR, mnuMINUTE, mnuSECOND });
            mnuTxts.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuFINDTEXT, mnuFORMAT, mnuLINEBREAK, mnuSTRLEN, mnuSUBSTRING, mnuTXTTONUM, mnuTXTTODATE, mnuUPPERCASE });

            mnuFORMAT.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuDATEFORMATS, mnuNumberFormats });
            mnuDATEFORMATS.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuGeneralDate, mnuLongDate, mnuShortDate, mnuLongTime, mnuShortTime });
            mnuNumberFormats.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuGeneralNumber, mnuCurrency, mnuFixed, mnuStandard, mnuPercent, mnuScientific, mnuYN, mnuTF, mnuOnOff });
            return contextMenu;
        }

        #endregion  //Function Context Menu
		
	}
}
