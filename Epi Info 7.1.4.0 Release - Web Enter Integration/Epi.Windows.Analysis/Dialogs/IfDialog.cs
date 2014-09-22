using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Epi;
using Epi.Analysis;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;
using Epi.Windows.Analysis.Forms;

namespace Epi.Windows.Analysis.Dialogs
{
	/// <summary>
	/// Dialog for If command
	/// </summary>
	public partial class IfDialog : CommandDesignDialog
	{
		private AnalysisMainForm amf;
        private CommandContextMenuType CurrentContextMenuType;

		#region Constructor	

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
		[Obsolete("Use of default constructor not allowed", true)]
		public IfDialog()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Constructor for If Dialog
		/// </summary>
		/// <param name="frm"></param>
		public IfDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
			: base(frm)
		{
			InitializeComponent();
			Construct();
			amf = frm;
		}
		#endregion Constructors

		#region Event Handlers

        /// <summary>
		/// Clears user input
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void btnClear_Click(object sender, System.EventArgs e)
		{
			txtIfCondition.Text = string.Empty;
			cmbAvailableVar.Text = string.Empty;
			txtThen.Text = string.Empty;
			txtElse.Text = string.Empty;
		}

		/// <summary>
		/// Handles combo box SelectionIndexChanged event
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cmbAvailableVar_SelectedIndexChanged(object sender, System.EventArgs e)
		{
            TextBox txtTargetBox = new TextBox();
            switch (CurrentContextMenuType)
            {
                case CommandContextMenuType.If:
                    txtTargetBox = txtIfCondition;
                    break;
                case CommandContextMenuType.Then:
                    txtTargetBox = txtThen;
                    break;
                case CommandContextMenuType.Else:
                    txtTargetBox = txtElse;
                    break;
            }

            if (!string.IsNullOrEmpty(cmbAvailableVar.Text))
            {
                string AvailableVarString = (FieldNameNeedsBrackets(cmbAvailableVar.Text) ? Util.InsertInSquareBrackets(cmbAvailableVar.Text) : cmbAvailableVar.Text);
                //insert a space before the variable only when:
                //  * textbox has length and insertion point is at the end and nothing is selected
                //  * there isn't already a space at the end
                //  * there is an even quote at the end (but not if its an odd quote)
                if ((txtTargetBox.TextLength > 0 && txtTargetBox.SelectionStart.Equals(txtTargetBox.TextLength)) && (txtTargetBox.SelectionLength.Equals(0)))
                {
                    if (txtTargetBox.Text.EndsWith("\""))
                    {
                        if(QuoteCountIsEven(txtTargetBox.Text, txtTargetBox.SelectionStart))
                        {
                            AvailableVarString = StringLiterals.SPACE + AvailableVarString;
                        }
                    }
                    else
                    {
                        AvailableVarString = StringLiterals.SPACE + AvailableVarString;
                    }
                }

                if (txtTargetBox.SelectionLength > 0)
                {
                    //Don't replace the trailing comma in 
                    //  multi-parameter functions like YEARS or 
                    //  trailing close paren if selected by mistake
                    while (txtTargetBox.SelectionLength > 0 &&
                        (txtTargetBox.SelectedText.EndsWith(StringLiterals.SPACE) ||
                        txtTargetBox.SelectedText.EndsWith(StringLiterals.COMMA) ||
                        txtTargetBox.SelectedText.EndsWith(StringLiterals.PARANTHESES_CLOSE)))
                    {
                        txtTargetBox.SelectionLength -= 1;
                    }
                }
                txtTargetBox.SelectedText = AvailableVarString;
                txtTargetBox.Focus();
                txtTargetBox.Text = ClearSpacesFromParens(txtTargetBox.Text);
                txtTargetBox.Select(txtTargetBox.Text.Length, 0);
            }

		}

        /// <summary>
        /// Handles txtIfCondition TextChanged event
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void txtIfCondition_TextChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = (String.IsNullOrEmpty(txtIfCondition.Text.Trim()) ? false : true) && (String.IsNullOrEmpty(txtThen.Text.Trim()) ? false : true);
            btnSaveOnly.Enabled = btnOK.Enabled;
        }

        /// <summary>
        /// Handles txtThenCondition TextChanged event
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void txtThen_TextChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = (String.IsNullOrEmpty(txtIfCondition.Text.Trim()) ? false : true) && (String.IsNullOrEmpty(txtThen.Text.Trim()) ? false : true);
            btnSaveOnly.Enabled = btnOK.Enabled;
        }

        /// <summary>
        /// Handles txtIfCondition MouseDown event
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void txtIfCondition_MouseDown(object sender, MouseEventArgs e)
        {
            CurrentContextMenuType = CommandContextMenuType.If;
        }

        /// <summary>
        /// Handles txtThen MouseDown event
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void txtThen_MouseDown(object sender, MouseEventArgs e)
        {
            CurrentContextMenuType = CommandContextMenuType.Then;
        }

        /// <summary>
        /// Handles txtElse MouseDown event
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void txtElse_MouseDown(object sender, MouseEventArgs e)
        {
            CurrentContextMenuType = CommandContextMenuType.Else;
        }

        /// <summary>
        /// Handles txtIfCondition MouseClick event
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void txtIfCondition_MouseClick(object sender, MouseEventArgs e)
        {
            CurrentContextMenuType = CommandContextMenuType.If;
        }

        /// <summary>
        /// Handles txtThen MouseClick event
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void txtThen_MouseClick(object sender, MouseEventArgs e)
        {
            CurrentContextMenuType = CommandContextMenuType.Then;
        }

        /// <summary>
        /// Handles txtElse MouseClick event
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void txtElse_MouseClick(object sender, MouseEventArgs e)
        {
            CurrentContextMenuType = CommandContextMenuType.Else;
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://wwwn.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-if-then-else.html");
        }

		#endregion //Event Handlers		

		#region Private Methods
		/// <summary>
		/// Generates the command text
		/// </summary>
		protected override void GenerateCommand()
		{
			StringBuilder command = new StringBuilder();
			command.Append(CommandNames.IF);
			command.Append(StringLiterals.SPACE);

			command.Append(this.txtIfCondition.Text);

			command.Append(StringLiterals.SPACE);
			command.Append(CommandNames.THEN);
			command.Append(Environment.NewLine);

			foreach (string line in this.txtThen.Lines)
			{
				if (String.IsNullOrEmpty(line) == false)
				{
					command.Append(StringLiterals.TAB).Append(line);
					command.Append(Environment.NewLine);
				}
			}

			if (!command.ToString().EndsWith(Environment.NewLine))
			{
				command.Append(Environment.NewLine);
			}

			if (!string.IsNullOrEmpty(this.txtElse.Text))
			{
				command.Append(CommandNames.ELSE);
				command.Append(Environment.NewLine);

				foreach (string line in this.txtElse.Lines)
				{
					if (String.IsNullOrEmpty(line) == false)
					{
						command.Append(StringLiterals.TAB).Append(line);
						command.Append(Environment.NewLine);
					}
				}

				if (!command.ToString().EndsWith(Environment.NewLine))
				{
					command.Append(Environment.NewLine);
				}
			}
			command.Append(CommandNames.END);

			CommandText = command.ToString();
		}
		/// <summary>
		/// Run time mode constructor
		/// </summary>
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

		/// <summary>
		/// Fill available variables combobox
		/// </summary>
		private void LoadVariables()
		{
			VariableType scopeWord = VariableType.DataSource | VariableType.Standard | VariableType.Global | VariableType.Permanent;
			FillVariableCombo(cmbAvailableVar, scopeWord);
			cmbAvailableVar.SelectedIndex = -1;
			this.cmbAvailableVar.SelectedIndexChanged += new System.EventHandler(this.cmbAvailableVar_SelectedIndexChanged);

		}

		private void IfDialog_Load(object sender, EventArgs e)
		{
			LoadVariables();
		}

        private string ShowCommandDesignDialog(Epi.Windows.Analysis.Dialogs.CommandDesignDialog dialog)
		{
			DialogResult result = dialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				return (dialog.CommandText + Environment.NewLine);
			}
			return string.Empty;
		}

		private void WriteToTextBox(object sender, string text)
		{
            if (CurrentContextMenuType == CommandContextMenuType.Then)
            {
                if (txtThen.Lines.Length > 0)
                {
                    txtThen.AppendText(Environment.NewLine);
                }
                txtThen.AppendText(text);
            }
            else
            {
                if (txtElse.Lines.Length > 0)
                {
                    txtElse.AppendText(Environment.NewLine);
                }
                txtElse.AppendText(text);
            }

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
            TextBox txtTargetBox = new TextBox();
            switch (CurrentContextMenuType)
            {
                case CommandContextMenuType.If:
                    txtTargetBox = txtIfCondition;
                    break;
                case CommandContextMenuType.Then:
                    txtTargetBox = txtThen;
                    break;
                case CommandContextMenuType.Else:
                    txtTargetBox = txtElse;
                    break;
            }
            if (txtTargetBox.SelectionLength > 0)
            {
                //Don't replace the trailing comma in 
                //  multi-parameter functions like YEARS or 
                //  trailing close paren if selected by mistake
                while (txtTargetBox.SelectionLength > 0 &&
                    (txtTargetBox.SelectedText.EndsWith(StringLiterals.SPACE) ||
                    txtTargetBox.SelectedText.EndsWith(StringLiterals.COMMA) ||
                    txtTargetBox.SelectedText.EndsWith(StringLiterals.PARANTHESES_CLOSE)))
                {
                    txtTargetBox.SelectionLength -= 1;
                }
            }

            if ((string)btn.Tag == null)
            {
                if ((string)btn.Text == StringLiterals.DOUBLEQUOTES)
                {
                    //if the button is for a double quote, only add a leading space if there is 
                    //  an even number of quotes in the expression so far.

                    strInsert = ((QuoteCountIsEven(txtTargetBox.Text, txtTargetBox.SelectionStart)) ? StringLiterals.SPACE : string.Empty);
                }
                txtTargetBox.SelectedText = strInsert + (string)btn.Text;
            }
            else
            {
                if ((string)btn.Tag == StringLiterals.DOUBLEQUOTES)
                {
                    //if the button is for a double quote, only add a leading space if there is 
                    //  an odd number of quotes in the expression so far.
                    strInsert = ((QuoteCountIsEven(txtTargetBox.Text, txtTargetBox.SelectionStart)) ? StringLiterals.SPACE : string.Empty);
                }
                txtTargetBox.SelectedText = strInsert + (string)btn.Tag;
            }
            //Remove spaces within (+) (.) and (-) as any spaces within the parentheses
            //  will cause an error.
            txtTargetBox.Text = ClearSpacesFromParens(txtTargetBox.Text);
            txtTargetBox.Select(txtTargetBox.Text.Length, 0);
            txtTargetBox.Focus();
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

        #region Command Context Menu
        /// <summary>
        /// Builds a context menu for commands
        /// </summary>
        /// <returns>A commands context menu</returns>
        private ContextMenuStrip BuildCommandContextMenu()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem mnuData = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_DATA);
            ToolStripMenuItem mnuVariables = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_VARIABLES);
            ToolStripMenuItem mnuSelectIf = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_SELECTIF);
            ToolStripMenuItem mnuStatistics = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_STATS);
            ToolStripMenuItem mnuAdvancedStatistics = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_ADVSTATS);
            mnuAdvancedStatistics.Enabled = false;
            ToolStripMenuItem mnuOutput = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_OUTPUT);
            ToolStripMenuItem mnuUserDefinedCommands = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_USERCMDS);
            ToolStripMenuItem mnuUserInteraction = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_USERINTERACTION);
            ToolStripMenuItem mnuOptions = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_OPTIONS);
            
            ToolStripMenuItem mnuAssign = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_ASSIGN);
            mnuAssign.Click += new EventHandler(mnuAssign_Click);

            ToolStripMenuItem mnuBeep = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_BEEP);
            mnuBeep.Click += new EventHandler(mnuBeep_Click);
            
            ToolStripMenuItem mnuCancelSelect = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_CANCELSELECT);
            mnuCancelSelect.Click += new EventHandler(mnuCancelSelect_Click);
            
            ToolStripMenuItem mnuCancelSort = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_CANCELSORT);
            mnuCancelSort.Click += new EventHandler(mnuCancelSort_Click);

            ToolStripMenuItem mnuCloseOut = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_CLOSEOUT);
            mnuCloseOut.Click += new EventHandler(mnuCloseOut_Click);

            ToolStripMenuItem mnuComplexSampleFrequencies = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_COMPSAMPFREQ);
            mnuComplexSampleFrequencies.Enabled = false;
            mnuComplexSampleFrequencies.Click += new EventHandler(mnuComplexSampleFrequencies_Click);

            ToolStripMenuItem mnuComplexSampleMeans = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_COMPSAMPMEANS);
            mnuComplexSampleMeans.Enabled = false;
            mnuComplexSampleMeans.Click += new EventHandler(mnuComplexSampleMeans_Click);

            ToolStripMenuItem mnuComplexSampleTables = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_COMPSAMPTABLES);
            mnuComplexSampleTables.Enabled = false;
            mnuComplexSampleTables.Click += new EventHandler(mnuComplexSampleTables_Click);

            ToolStripMenuItem mnuCoxProportionalHazards = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_COXPH);
            mnuCoxProportionalHazards.Enabled = false;
            mnuCoxProportionalHazards.Click += new EventHandler(mnuCoxProportionalHazards_Click);

            ToolStripMenuItem mnuDeleteFileTable = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_DELETEFILETABLE);
            mnuDeleteFileTable.Click += new EventHandler(mnuDeleteFileTable_Click);

            ToolStripMenuItem mnuDeleteRecords = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_DELETERECORDS);
            mnuDeleteRecords.Click += new EventHandler(mnuDeleteRecords_Click);
            
            ToolStripMenuItem mnuDialog = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_DIALOG);
            mnuDialog.Click += new EventHandler(mnuDialog_Click);

            ToolStripMenuItem mnuDisplay = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_DISPLAY);
            mnuDisplay.Click += new EventHandler(mnuDisplay_Click);
            
            ToolStripMenuItem mnuExecuteFile = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_EXECUTEFILE);
            mnuExecuteFile.Click += new EventHandler(mnuExecuteFile_Click);

            ToolStripMenuItem mnuFrequencies = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_FREQ);
            mnuFrequencies.Click += new EventHandler(mnuFrequencies_Click);
            
            ToolStripMenuItem mnuGraph = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_GRAPH);
            mnuGraph.Enabled = false;
            mnuGraph.Click += new EventHandler(mnuGraph_Click);

            ToolStripMenuItem mnuHeader = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_HEADER);
            mnuHeader.Click += new EventHandler(mnuHeader_Click);
   
            ToolStripMenuItem mnuHelp = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_HELP);
            mnuHelp.Enabled = false;
            mnuHelp.Click += new EventHandler(mnuHelp_Click);

            ToolStripMenuItem mnuIf = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_IF);
            mnuIf.Click += new EventHandler(mnuIf_Click);
         
            ToolStripMenuItem mnuKaplanMeierSurvival = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_KMSURV);
            mnuKaplanMeierSurvival.Enabled = false;
            mnuKaplanMeierSurvival.Click += new EventHandler(mnuKaplanMeierSurvival_Click);

            ToolStripMenuItem mnuLinearRegression = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_LINEARREGRESSION);
            mnuLinearRegression.Enabled = false;
            mnuLinearRegression.Click += new EventHandler(mnuLinearRegression_Click);

            ToolStripMenuItem mnuList = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_LIST);
            mnuList.Click += new EventHandler(mnuList_Click);
                        
            ToolStripMenuItem mnuLogisticRegression = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_LOGISTICREGRESSION);
            mnuLogisticRegression.Enabled = false;
            mnuLogisticRegression.Click += new EventHandler(mnuLogisticRegression_Click);

            ToolStripMenuItem mnuMap = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_MAP);
            mnuMap.Enabled = false;
            mnuMap.Click += new EventHandler(mnuMap_Click);

            ToolStripMenuItem mnuMatch = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_MATCH);
            mnuMatch.Click += new EventHandler(mnuMatch_Click);

            ToolStripMenuItem mnuMeans = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_MEANS);
            mnuMeans.Click += new EventHandler(mnuMeans_Click);

            ToolStripMenuItem mnuMerge = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_MERGE);
            mnuMerge.Click += new EventHandler(mnuMerge_Click);

            ToolStripMenuItem mnuPrintOut = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_PRINTOUT);
            mnuPrintOut.Click += new EventHandler(mnuPrintOut_Click);

            ToolStripMenuItem mnuQuitProgram = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_QUIT);
            mnuQuitProgram.Click += new EventHandler(mnuQuitProgram_Click);

            ToolStripMenuItem mnuRead = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_READ);
            mnuRead.Click += new EventHandler(mnuRead_Click);

            ToolStripMenuItem mnuRecode = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_RECODE);
            mnuRecode.Click += new EventHandler(mnuRecode_Click);

            ToolStripMenuItem mnuRelate = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_RELATE);
            mnuRelate.Click += new EventHandler(mnuRelate_Click);

            ToolStripMenuItem mnuReports = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_REPORTS);
            mnuReports.Click += new EventHandler(mnuReports_Click);

            ToolStripMenuItem mnuRouteOut = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_ROUTEOUT);
            mnuRouteOut.Click += new EventHandler(mnuRouteOut_Click);

            ToolStripMenuItem mnuRunSavedProgram = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_RUNSAVED);
            mnuRunSavedProgram.Click += new EventHandler(mnuRunSavedProgram_Click);

            ToolStripMenuItem mnuSelect = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_SELECT);
            mnuSelect.Click += new EventHandler(mnuSelect_Click);

            ToolStripMenuItem mnuSet = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_SET);
            mnuSet.Click += new EventHandler(mnuSet_Click);

            ToolStripMenuItem mnuSort = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_SORT);
            mnuSort.Click += new EventHandler(mnuSort_Click);

            ToolStripMenuItem mnuStoringOutput = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_STORINGOUTPUT);
            mnuStoringOutput.Click += new EventHandler(mnuStoringOutput_Click);

            ToolStripMenuItem  mnuSummarize = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_SUMMARIZE);
            mnuSummarize.Click += new EventHandler(mnuSummarize_Click);

            ToolStripMenuItem mnuTables = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_TABLES);
            mnuTables.Click += new EventHandler(mnuTables_Click);

            ToolStripMenuItem mnuType = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_TYPE);
            mnuType.Click += new EventHandler(mnuType_Click);

            ToolStripMenuItem mnuUndeleteRecords = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_UNDELETERECORDS);
            mnuUndeleteRecords.Click += new EventHandler(mnuUndeleteRecords_Click);

            ToolStripMenuItem mnuUserCommand = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_USERCOMMAND);
            mnuUserCommand.Click += new EventHandler(mnuUserCommand_Click);

            ToolStripMenuItem mnuWrite = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_WRITE);
            mnuWrite.Click += new EventHandler(mnuWrite_Click);

            contextMenu.Items.AddRange(new ToolStripMenuItem[] { mnuData, 
                mnuVariables, 
                mnuSelectIf, 
                mnuStatistics, 
                mnuAdvancedStatistics, 
                mnuOutput, 
                mnuUserDefinedCommands, 
                mnuUserInteraction, 
                mnuOptions });

                mnuData.DropDownItems.AddRange(new ToolStripMenuItem[] { mnuRead, mnuRelate, mnuWrite, mnuMerge, mnuDeleteFileTable, mnuDeleteRecords, mnuUndeleteRecords });
                mnuVariables.DropDownItems.AddRange(new ToolStripMenuItem[] { mnuAssign, mnuRecode, mnuDisplay}); 
                mnuSelectIf.DropDownItems.AddRange(new ToolStripMenuItem[] { mnuSelect, mnuCancelSelect, mnuIf, mnuSort, mnuCancelSort});
                mnuStatistics.DropDownItems.AddRange(new ToolStripMenuItem[] { mnuList, mnuFrequencies, mnuTables, mnuMatch, mnuMeans, mnuSummarize, mnuGraph, mnuMap});
                mnuAdvancedStatistics.DropDownItems.AddRange(new ToolStripMenuItem[] { mnuLinearRegression, mnuLogisticRegression, mnuKaplanMeierSurvival, mnuCoxProportionalHazards, mnuComplexSampleFrequencies, mnuComplexSampleMeans, mnuComplexSampleTables });
                mnuOutput.DropDownItems.AddRange(new ToolStripMenuItem[] { mnuHeader, mnuType, mnuRouteOut, mnuCloseOut, mnuPrintOut, mnuReports, mnuStoringOutput});
                mnuUserDefinedCommands.DropDownItems.AddRange(new ToolStripMenuItem[] {mnuUserCommand, mnuRunSavedProgram,  mnuExecuteFile });
                mnuUserInteraction.DropDownItems.AddRange(new ToolStripMenuItem[] { mnuDialog, mnuBeep, mnuHelp, mnuQuitProgram });
                mnuOptions.DropDownItems.AddRange(new ToolStripMenuItem[] { mnuSet });

            return contextMenu;
        }

        private void mnuAssign_Click(object sender, EventArgs e)
        {
            AssignDialog dialog = new AssignDialog(this.amf, false);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuBeep_Click(object sender, EventArgs e)
        {
            BeepDialog dialog = new BeepDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuCancelSelect_Click(object sender, EventArgs e)
        {
            CancelSelect dialog = new CancelSelect(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuCancelSort_Click(object sender, EventArgs e)
        {
            CancelSort dialog = new CancelSort(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuCloseOut_Click(object sender, EventArgs e)
        {
            CloseoutDialog dialog = new CloseoutDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuComplexSampleFrequencies_Click(object sender, EventArgs e)
        {
            DisplayFeatureNotImplementedMessage();
        }

        private void mnuComplexSampleMeans_Click(object sender, EventArgs e)
        {
            DisplayFeatureNotImplementedMessage();
        }

        private void mnuComplexSampleTables_Click(object sender, EventArgs e)
        {
            DisplayFeatureNotImplementedMessage();
        }

        private void mnuCoxProportionalHazards_Click(object sender, EventArgs e)
        {
            DisplayFeatureNotImplementedMessage();
        }

        private void mnuDeleteFileTable_Click(object sender, EventArgs e)
        {
            DeleteFileTableDialog dialog = new DeleteFileTableDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuDeleteRecords_Click(object sender, EventArgs e)
        {
            DeleteRecordsDialog dialog = new DeleteRecordsDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuDialog_Click(object sender, EventArgs e)
        {
            DialogDialog dialog = new DialogDialog(this.amf, false);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuDisplay_Click(object sender, EventArgs e)
        {
            DisplayDialog dialog = new DisplayDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuExecuteFile_Click(object sender, EventArgs e)
        {
            DisplayFeatureNotImplementedMessage();
        }

        private void mnuFrequencies_Click(object sender, EventArgs e)
        {
            FrequencyDialog dialog = new FrequencyDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuGraph_Click(object sender, EventArgs e)
        {
            DisplayFeatureNotImplementedMessage();
        }

        private void mnuHeader_Click(object sender, EventArgs e)
        {
            HeaderoutDialog dialog = new HeaderoutDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuHelp_Click(object sender, EventArgs e)
        {
            HelpDialog dialog = new HelpDialog(this.amf, false);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuIf_Click(object sender, EventArgs e)
        {
            IfDialog dialog = new IfDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuKaplanMeierSurvival_Click(object sender, EventArgs e)
        {
            DisplayFeatureNotImplementedMessage();
        }

        private void mnuLinearRegression_Click(object sender, EventArgs e)
        {
            DisplayFeatureNotImplementedMessage();
        }

        private void mnuLogisticRegression_Click(object sender, EventArgs e)
        {
            DisplayFeatureNotImplementedMessage();
        }

        private void mnuList_Click(object sender, EventArgs e)
        {
            ListDialog dialog = new ListDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuMap_Click(object sender, EventArgs e)
        {
            DisplayFeatureNotImplementedMessage();
        }

        private void mnuMatch_Click(object sender, EventArgs e)
        {
            MatchDialog dialog = new MatchDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuMeans_Click(object sender, EventArgs e)
        {
            MeansDialog dialog = new MeansDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuMerge_Click(object sender, EventArgs e)
        {
            MergeDialog dialog = new MergeDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuPrintOut_Click(object sender, EventArgs e)
        {
            PrintoutDialog dialog = new PrintoutDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuQuitProgram_Click(object sender, EventArgs e)
        {
            QuitDialog dialog = new QuitDialog(this.amf, false);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuRead_Click(object sender, EventArgs e)
        {
            ReadDialog dialog = new ReadDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuRecode_Click(object sender, EventArgs e)
        {
            RecodeDialog dialog = new RecodeDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuRelate_Click(object sender, EventArgs e)
        {
            RelateDialog dialog = new RelateDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuReports_Click(object sender, EventArgs e)
        {
            DisplayFeatureNotImplementedMessage();
        }

        private void mnuRouteOut_Click(object sender, EventArgs e)
        {
            RouteoutDialog dialog = new RouteoutDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuRunSavedProgram_Click(object sender, EventArgs e)
        {
            RunSavedPGMDialog dialog = new RunSavedPGMDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuSelect_Click(object sender, EventArgs e)
        {
            SelectDialog dialog = new SelectDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuSet_Click(object sender, EventArgs e)
        {
            SetDialog dialog = new SetDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuSort_Click(object sender, EventArgs e)
        {
            SortDialog dialog = new SortDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuStoringOutput_Click(object sender, EventArgs e)
        {
            DisplayFeatureNotImplementedMessage();
        }

        private void mnuSummarize_Click(object sender, EventArgs e)
        {
            SummarizeDialog dialog = new SummarizeDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuTables_Click(object sender, EventArgs e)
        {
            TablesDialog dialog = new TablesDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuType_Click(object sender, EventArgs e)
        {
            TypeoutDialog dialog = new TypeoutDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuUndeleteRecords_Click(object sender, EventArgs e)
        {
            UndeleteRecordsDialog dialog = new UndeleteRecordsDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        private void mnuUserCommand_Click(object sender, EventArgs e)
        {
            DisplayFeatureNotImplementedMessage();
        }

        private void mnuWrite_Click(object sender, EventArgs e)
        {
            WriteDialog dialog = new WriteDialog(this.amf);
            WriteToTextBox(sender, ShowCommandDesignDialog(dialog));
        }

        #endregion

        #region Function Context Menu

        /// <summary>
        /// Handles the MouseDown event of the Then button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnThen_MouseDown(object sender, MouseEventArgs e)
        {
            CurrentContextMenuType = CommandContextMenuType.Then;
            BuildCommandContextMenu().Show((Control)sender, e.Location);
        }

        /// <summary>
        /// Handles the Mouse Down event of the Else button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnElse_MouseDown(object sender, MouseEventArgs e)
        {
            CurrentContextMenuType = CommandContextMenuType.Else;
            BuildCommandContextMenu().Show((Control)sender, e.Location);
        }

        /// <summary>
        /// Handles the MouseDown event of the If Function button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnFunction_MouseDown(object sender, MouseEventArgs e)
        {
            CurrentContextMenuType = CommandContextMenuType.If;
            BuildFunctionContextMenu().Show((Control)sender, e.Location);
        }

        /// <summary>
        /// Handles the MouseDown event of the Functions button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnFunctions_MouseDown(object sender, MouseEventArgs e)
        {
            BuildFunctionContextMenu().Show((Control)sender, e.Location);
        }


        /// <summary>
        /// Handles the MouseDown event of the Then Function button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnFunctionThen_MouseDown(object sender, MouseEventArgs e)
        {
            CurrentContextMenuType = CommandContextMenuType.Then;
            BuildFunctionContextMenu().Show((Control)sender, e.Location);
        }

        /// <summary>
        /// Handles the MouseDown event of the Else Function button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnFunctionElse_MouseDown(object sender, MouseEventArgs e)
        {
            CurrentContextMenuType = CommandContextMenuType.Else;
            BuildFunctionContextMenu().Show((Control)sender, e.Location);
        }

        /// <summary>
        /// Handles the Click event of the  Functions and Operators context menu
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void FXClickHandler(object sender, System.EventArgs e)
        {
            ToolStripMenuItem FXItem = (ToolStripMenuItem)sender;
            TextBox txtTargetBox = new TextBox();
            switch (CurrentContextMenuType)
            {
                case CommandContextMenuType.If:
                    txtTargetBox = txtIfCondition;
                    break;
                case CommandContextMenuType.Then:
                    txtTargetBox = txtThen;
                    break;
                case CommandContextMenuType.Else:
                    txtTargetBox = txtElse;
                    break;
            }

            if (txtTargetBox.SelectionLength > 0)
            {
                //Don't replace the trailing comma in 
                //  multi-parameter functions like YEARS or 
                //  trailing close paren if selected by mistake
                while (txtTargetBox.SelectionLength > 0 &&
                    (txtTargetBox.SelectedText.EndsWith(StringLiterals.SPACE) ||
                    txtTargetBox.SelectedText.EndsWith(StringLiterals.COMMA) ||
                    txtTargetBox.SelectedText.EndsWith(StringLiterals.PARANTHESES_CLOSE)))
                {
                    txtTargetBox.SelectionLength -= 1;
                }
            }
            txtTargetBox.SelectedText = StringLiterals.SPACE + FXItem.ToolTipText + StringLiterals.SPACE;
            txtTargetBox.Text = ClearSpacesFromParens(txtTargetBox.Text);
            txtTargetBox.Select(txtTargetBox.Text.Length, 0);
            txtTargetBox.Focus();
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

        #region Private Enumerations

        private enum CommandContextMenuType
        {
            If,
            Then,
            Else
        }

        #endregion  //Private Enumerations
    }
}

