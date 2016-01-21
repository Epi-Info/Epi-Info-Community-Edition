#region Namespaces

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Epi;
using Epi.Data.Services;
using Epi.Windows.Dialogs;
using VariableCollection = Epi.Collections.NamedObjectCollection<Epi.IVariable>;

#endregion  Namespaces

namespace Epi.Windows.MakeView.Dialogs
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
        public AssignDialog(Epi.Windows.MakeView.Forms.MakeViewMainForm frm)
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
        public AssignDialog(Epi.Windows.MakeView.Forms.MakeViewMainForm frm, bool showSave)
            : base(frm)
        {
            InitializeComponent();
            if (showSave)
            {
                showSaveOnly = false;
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
                Construct();
            }
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
            VariableType scopeWord = VariableType.Standard | VariableType.Global | 
                                                 VariableType.DataSource | VariableType.DataSourceRedefined | VariableType.Permanent;
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

            ToolStripMenuItem mnuEpiWeek = new ToolStripMenuItem(CommandNames.EPIWEEK);
            mnuEpiWeek.ToolTipText = CommandNames.EPIWEEK + "( <start_date>, {<first_day_of_week>} )";
            mnuEpiWeek.Click += new EventHandler(FXClickHandler);

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
            //--123
            ToolStripMenuItem mnuFIRSTSAVETIME = new ToolStripMenuItem(CommandNames.FIRSTSAVETIME);
            mnuFIRSTSAVETIME.ToolTipText = CommandNames.FIRSTSAVETIME;
            mnuFIRSTSAVETIME.Click += new EventHandler(FXClickHandler);

            ToolStripMenuItem mnuLASTSAVETIME = new ToolStripMenuItem(CommandNames.LASTSAVETIME);
            mnuLASTSAVETIME.ToolTipText = CommandNames.LASTSAVETIME;
            mnuLASTSAVETIME.Click += new EventHandler(FXClickHandler);
            //--

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
            mnuDates.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuYRS, mnuMOS, mnuDYS, mnuYR, mnuMO, mnuDY, mnuEpiWeek });
            mnuSys.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuCurrUser, mnuEXISTS, mnuFILEDATE, mnuSYSTEMDATE, mnuSYSTEMTIME, mnuFIRSTSAVETIME, mnuLASTSAVETIME });
            mnuTimes.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuHOURS, mnuMINUTES, mnuSECONDS, mnuHOUR, mnuMINUTE, mnuSECOND });
            mnuTxts.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuFINDTEXT, mnuFORMAT, mnuLINEBREAK, mnuSTRLEN, mnuSUBSTRING, mnuTXTTONUM, mnuTXTTODATE, mnuUPPERCASE });

            mnuFORMAT.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuDATEFORMATS, mnuNumberFormats });
            mnuDATEFORMATS.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuGeneralDate, mnuLongDate, mnuShortDate, mnuLongTime, mnuShortTime });
            mnuNumberFormats.DropDown.Items.AddRange(new ToolStripMenuItem[] { mnuGeneralNumber, mnuCurrency, mnuFixed, mnuStandard, mnuPercent, mnuScientific, mnuYN, mnuTF, mnuOnOff });
            return contextMenu;
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
            WordBuilder command = new WordBuilder();
            
            command.Append(CommandNames.ASSIGN);
            command.Append(cmbAssignVar.Text);
            command.Append("=");
            // Fix for defect #20 - prevent the resulting command from having to = signs if the user supplies one by accident.
            string expression = txtExpression.Text.Trim();
            if (expression.Length > 0)
            {
                if (expression[0] == '=')
                {
                    expression = expression.Remove(0, 1);
                    expression = expression.Trim();
                }
            }
            command.Append(expression);
            CommandText = command.ToString();
        }

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



            //Button btn = (Button)sender;
            ////txtExpression.Text += StringLiterals.SPACE;
            //bool checkForParenthesis = false;
            //int position = txtExpression.SelectionStart;
            //if ((string)btn.Tag == null)
            //{
            //    string textToInsert = btn.Text;
            //    if (textToInsert == "&&")
            //    {
            //        textToInsert = "&";
            //    }
            //    txtExpression.Text = txtExpression.Text.Insert(position, textToInsert + " ");
            //    txtExpression.Select(position + btn.Text.Length + 1, 0);  
            //    if(btn.Text.Contains(")")) 
            //    {
            //        checkForParenthesis = true;
            //    }
            //}
            //else
            //{
            //    string textToInsert = btn.Text;
            //    if (textToInsert == "&&")
            //    {
            //        textToInsert = "&";
            //    }
            //    txtExpression.Text = txtExpression.Text.Insert(position, textToInsert + " ");
            //    txtExpression.Select(position + btn.Text.Length + 1, 0);  
            //    if (((string)btn.Tag).Contains(")"))
            //    {
            //        checkForParenthesis = true;
            //    }
            //}            
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
        /// Handles the Change event of the index of the available variables combobox
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void cbxAvailVariables_SelectedIndexChanged(object sender, EventArgs e)
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

        private void cmbAvailVar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(cmbAvailVar.Text))
            {
                string AvailableVarString = (FieldNameNeedsBrackets(cmbAvailVar.Text) ? Util.InsertInSquareBrackets(cmbAvailVar.Text) : cmbAvailVar.Text);
                if (txtExpression.TextLength > 0 && txtExpression.SelectionLength.Equals(0))
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
        /// Handles the MouseDown event of the If Function button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnFunction_MouseDown(object sender, MouseEventArgs e)
        {
            BuildFunctionContextMenu().Show((Control)sender, e.Location);
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/command-reference/check-commands-Assign.html");
        }

        
        #endregion Event Handlers      
    }
}

