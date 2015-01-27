using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Analysis;
using Epi.Core.AnalysisInterpreter;
using Epi.Data;
using Epi.Windows;
using Epi.Windows.Controls;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;
using Epi.Windows.Analysis.Forms;

namespace Epi.Windows.Analysis.Dialogs
{
    /// <summary>
    /// Dialog used for RunPGM command
    /// </summary>
    public partial class DefineUserCommandDialog : CommandDesignDialog
    {

        private AnalysisMainForm amf;
      //  private CommandContextMenuType CurrentContextMenuType;


        #region Constructors

        /// <summary>
        /// Constructor for dialog
        /// </summary>
        /// <param name="frm"></param>
        public DefineUserCommandDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
        {
            InitializeComponent();
            construct();
            CheckForInputSufficiency();
            amf = frm;
        }
        #endregion

        #region private members

        private DataTable programs;

        #region private methods
        private void construct()
        {
            if (!this.DesignMode)
            {
                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
            }
            
        }


        #endregion

        #endregion

        #region protected methods
      
        /// <summary>
        /// Enables/Disables controls
        /// </summary>
        public override void CheckForInputSufficiency()
        {          
            btnOK.Enabled = (String.IsNullOrEmpty(txtFileName.Text.Trim()) ? false : true) && (String.IsNullOrEmpty(textBoxCommandList.Text.Trim()) ? false : true);
            btnSaveOnly.Enabled = btnOK.Enabled;           
        }

        /// <summary>
        /// Generate Command
        /// </summary>
        protected override void GenerateCommand()
        {
            string cmdstring1 = "CMD " + txtFileName.Text + System.Environment.NewLine;
            string cmdstring2 = System.Environment.NewLine + "END-CMD";
            CommandText = cmdstring1 + textBoxCommandList.Text.ToString() + cmdstring2;          
        }
     
        #endregion

        #region event handlers

        private void txtFileName_Leave(object sender, EventArgs e)
        {
            txtFileName.Text = txtFileName.Text.Trim();
            CheckForInputSufficiency();
        }

        private void txtFileName_TextChanged(object sender, EventArgs e)
        {
            CheckForInputSufficiency();
        }

        private void textBoxCommandList_TextChanged(object sender, EventArgs e)
        {
            CheckForInputSufficiency();
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All Program Files|*.prj;*.pgm7|EpiInfo 7 Project|*.prj|PGM|*.pgm7";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtFileName.Text = dialog.FileName;
            }
        }

        private void cmbProgram_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckForInputSufficiency();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtFileName.Text = string.Empty;
            textBoxCommandList.Text = string.Empty;
            CheckForInputSufficiency();
            txtFileName.Focus();
        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://wwwn.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-runpgm.html");
        }

        #endregion

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

            ToolStripMenuItem mnuSummarize = new ToolStripMenuItem(SharedStrings.CNTXT_CMD_SUMMARIZE);
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
            mnuVariables.DropDownItems.AddRange(new ToolStripMenuItem[] { mnuAssign, mnuRecode, mnuDisplay });
            mnuSelectIf.DropDownItems.AddRange(new ToolStripMenuItem[] { mnuSelect, mnuCancelSelect, mnuIf, mnuSort, mnuCancelSort });
            mnuStatistics.DropDownItems.AddRange(new ToolStripMenuItem[] { mnuList, mnuFrequencies, mnuTables, mnuMatch, mnuMeans, mnuSummarize, mnuGraph, mnuMap });
            mnuAdvancedStatistics.DropDownItems.AddRange(new ToolStripMenuItem[] { mnuLinearRegression, mnuLogisticRegression, mnuKaplanMeierSurvival, mnuCoxProportionalHazards, mnuComplexSampleFrequencies, mnuComplexSampleMeans, mnuComplexSampleTables });
            mnuOutput.DropDownItems.AddRange(new ToolStripMenuItem[] { mnuHeader, mnuType, mnuRouteOut, mnuCloseOut, mnuPrintOut, mnuReports, mnuStoringOutput });
            mnuUserDefinedCommands.DropDownItems.AddRange(new ToolStripMenuItem[] { mnuUserCommand, mnuRunSavedProgram, mnuExecuteFile });
            mnuUserInteraction.DropDownItems.AddRange(new ToolStripMenuItem[] { mnuDialog, mnuBeep, mnuHelp, mnuQuitProgram });
            mnuOptions.DropDownItems.AddRange(new ToolStripMenuItem[] { mnuSet });

            return contextMenu;
        }


        # region Function Context Menu
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

        private void WriteToTextBox(object sender, string text)
        {
            if (textBoxCommandList.Lines.Length > 0)
            {
                textBoxCommandList.AppendText(Environment.NewLine);
            }
            textBoxCommandList.AppendText(text);          
        }

        private void GenerateCommandButton_Click(object sender, MouseEventArgs e)
        {
            //new CommandBlock          
        }

        private void GenerateCommandButton_MouseClick(object sender, MouseEventArgs e)
        {
            BuildCommandContextMenu().Show((Control)sender, e.Location);
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


    }
}
