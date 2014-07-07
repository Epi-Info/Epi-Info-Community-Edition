using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Text;
using Epi;
using Epi.Collections;
using Epi.Windows;
using Epi.Analysis;
using Epi.Windows.Dialogs;
using Epi.Windows.Analysis;
using Epi.Data.Services;

namespace Epi.Windows.Analysis.Dialogs
{
	/// <summary>
	/// Set Dialog
	/// </summary>
    public partial class SetDialog : CommandDesignDialog
    {
        /// <summary>
        /// Boolean isDialogMode
        /// </summary>
        public bool isDialogMode = false;

        #region Public Interface
		#region Constructor

		/// <summary>
		/// Default constructor - NOT TO BE USED FOR INSTANTIATION
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public SetDialog()
		{
			InitializeComponent();
		}

        /// <summary>
        /// Constructor for SetDialog
        /// </summary>
        /// <param name="frm">Main form</param>
        public SetDialog(Epi.Windows.Analysis.Forms.AnalysisMainForm frm)
            : base(frm)
		{
			InitializeComponent();
            Construct();
		}
		#endregion Constructors
        #endregion Public Interface

        #region Protected Interface
        /// <summary>
        /// Builds the command text
        /// </summary>		
        protected override void GenerateCommand()
        {
            Configuration config = Configuration.GetNewInstance();
            KeyValuePairCollection kvPairs = new KeyValuePairCollection();
            kvPairs.Delimiter = CharLiterals.SPACE;

            if ((settingsPanel.RepresentationOfYes != config.Settings.RepresentationOfYes) || (hasYesAsChanged))
            {
                kvPairs.Add(new KeyValuePair(ShortHands.YES, Util.InsertInDoubleQuotes(settingsPanel.RepresentationOfYes)));
            }
            if ((settingsPanel.RepresentationOfNo != config.Settings.RepresentationOfNo) || (hasNoAsChanged))
            {
                kvPairs.Add(new KeyValuePair(ShortHands.NO, Util.InsertInDoubleQuotes(settingsPanel.RepresentationOfNo)));
            }
            if ((settingsPanel.RepresentationOfMissing != config.Settings.RepresentationOfMissing) || (hasMissingAsChanged))
            {
                kvPairs.Add(new KeyValuePair(ShortHands.MISSING, Util.InsertInDoubleQuotes(settingsPanel.RepresentationOfMissing)));
            }
            if ((settingsPanel.ShowGraphics != config.Settings.ShowGraphics) || (hasShowGraphicChanged))
            {
                kvPairs.Add(new KeyValuePair(CommandNames.FREQGRAPH,
                    Epi.Util.GetShortHand(settingsPanel.ShowGraphics)));
            }
            if ((settingsPanel.ShowHyperlinks != config.Settings.ShowHyperlinks) || (hasShowHyperlinkChanged))
            {
                kvPairs.Add(new KeyValuePair(CommandNames.HYPERLINKS,
                    Epi.Util.GetShortHand(settingsPanel.ShowHyperlinks)));
            }
            if ((settingsPanel.ShowPercents != config.Settings.ShowPercents) || (hasShowPercentsChanged))
            {
                kvPairs.Add(new KeyValuePair(CommandNames.PERCENTS,
                    Epi.Util.GetShortHand(settingsPanel.ShowPercents)));
            }
            if ((settingsPanel.ShowSelectCriteria != config.Settings.ShowSelection) || (hasShowSelectChanged))
            {
                kvPairs.Add(new KeyValuePair(CommandNames.SELECT,
                    Epi.Util.GetShortHand(settingsPanel.ShowSelectCriteria)));
            }
            if ((settingsPanel.ShowPrompt != config.Settings.ShowCompletePrompt) || (hasShowPromptChanged))
            {
                kvPairs.Add(new KeyValuePair(CommandNames.SHOWPROMPTS,
                    Epi.Util.GetShortHand(settingsPanel.ShowPrompt)));
            }
            if ((settingsPanel.ShowTablesOutput != config.Settings.ShowTables) || (hasShowTablesChanged))
            {
                kvPairs.Add(new KeyValuePair(CommandNames.TABLES,
                    Epi.Util.GetShortHand(settingsPanel.ShowTablesOutput)));
            }
            if ((settingsPanel.ShowIncludeMissing != config.Settings.IncludeMissingValues) || (hasIncludeMissingChanged))
            {
                kvPairs.Add(new KeyValuePair(CommandNames.MISSING,
                    Epi.Util.GetShortHand(settingsPanel.ShowIncludeMissing)));
            }
            if (hasStatisticsLevelChanged)
            {
                RadioButton rbSelected = settingsPanel.StatisticLevel;
                StatisticsLevel levelIdSelected = (StatisticsLevel)short.Parse(rbSelected.Tag.ToString());
                string levelTagSelected = AppData.Instance.GetStatisticsLevelById(levelIdSelected).Tag;
                kvPairs.Add(new KeyValuePair(CommandNames.STATISTICS, levelTagSelected));
            }
            if (hasProcessRecordsChanged)
            {
                RadioButton rbSelected = settingsPanel.ProcessRecords;
                RecordProcessingScope scopeIdSelected = (RecordProcessingScope)short.Parse(rbSelected.Tag.ToString());
                string scopeTagSelected = AppData.Instance.GetRecordProcessessingScopeById(scopeIdSelected).Tag;
                kvPairs.Add(new KeyValuePair(CommandNames.PROCESS, scopeTagSelected));
            }

            WordBuilder command = new WordBuilder();
            //Generate command only if there are key value pairs
            if (kvPairs.Count > 0)
            {
                if (!this.isDialogMode)
                {
                    command.Append(CommandNames.SET);
                }
                command.Append(kvPairs.ToString());
                if (!this.isDialogMode)
                {
                    command.Append(" END-SET\n");
                }
                this.CommandText = command.ToString();
            }
            else
            {
                this.CommandText = string.Empty;
            }
        }

        
        /// <summary>
        /// Validates user input
        /// </summary>
        /// <returns>True/False depending upon whether errors were generated</returns>
        protected override bool ValidateInput()
        {
            return base.ValidateInput();
        }
        #endregion Protected Interface

        #region Private Attributes
        private bool hasYesAsChanged;
        private bool hasNoAsChanged;
        private bool hasMissingAsChanged;
        private bool hasShowPromptChanged;
        private bool hasShowGraphicChanged;
        private bool hasShowHyperlinkChanged;
        private bool hasShowSelectChanged;
        private bool hasShowPercentsChanged;
        private bool hasShowTablesChanged;
        private bool hasStatisticsLevelChanged;
        private bool hasIncludeMissingChanged;
        private bool hasProcessRecordsChanged;
        #endregion Private Attributes

        #region Private Methods

        private void Construct()
        {
            if (!this.DesignMode)           // designer throws an error
            {
                this.btnSaveOnly.Click += new System.EventHandler(this.btnSaveOnly_Click);
                this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            }
        }
	
		/// <summary>
		/// Reset hasChanged values
		/// </summary>
		private void ResetHasChangedValues(bool changed)
		{
			hasIncludeMissingChanged = changed;
			hasMissingAsChanged = changed;
			hasNoAsChanged = changed;
			hasYesAsChanged = changed;
			hasShowGraphicChanged = changed;
			hasShowHyperlinkChanged = changed;
			hasShowPercentsChanged = changed;
			hasShowPromptChanged = changed;
			hasShowSelectChanged = changed;
			hasShowTablesChanged = changed; 
			hasStatisticsLevelChanged = changed;
			hasProcessRecordsChanged = changed;
			
		}
	
		#endregion //Private Methods

        #region Event Handlers
        /// <summary>
        /// Handles Attach event of dialog
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void Set_Load(object sender, System.EventArgs e)
        {
            Configuration config = Configuration.GetNewInstance();

            this.EpiInterpreter.Context.ApplyOverridenConfigSettings(config);
            this.settingsPanel.LoadLists();
            this.settingsPanel.ShowSettings(config);
            ResetHasChangedValues(false);
        }

        /// <summary>
        /// Occurs when the value of RepresentationOfYes property changes.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void settingsPanel_RepresentationOfYesChanged(object sender, System.EventArgs e)
        {
            hasYesAsChanged = true;
        }

        /// <summary>
        /// Occurs when the value of RepresentationOfNo property changes.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void settingsPanel_RepresentationOfNoChanged(object sender, System.EventArgs e)
        {
            hasNoAsChanged = true;
        }
        /// <summary>
        /// Occurs when the value of RepresentationOfMissing property changes.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void settingsPanel_RepresentationOfMissingChanged(object sender, System.EventArgs e)
        {
            hasMissingAsChanged = true;
        }
        /// <summary>
        /// Occurs when ShowPrompt property changes.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void settingsPanel_ShowPromptChanged(object sender, System.EventArgs e)
        {
            hasShowPromptChanged = true;
        }
        /// <summary>
        /// Occurs when the value of ShowSelectCriteria property changes.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void settingsPanel_ShowSelectCriteriaChanged(object sender, System.EventArgs e)
        {
            hasShowSelectChanged = true;
        }
        /// <summary>
        /// Occurs when the value of ShowGraphics property changes.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void settingsPanel_ShowGraphicsChanged(object sender, System.EventArgs e)
        {
            hasShowGraphicChanged = true;
        }
        /// <summary>
        /// Occurs when the value of ShowPercents property changes.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void settingsPanel_ShowPercentsChanged(object sender, System.EventArgs e)
        {
            hasShowPercentsChanged = true;
        }
        /// <summary>
        /// Occurs when the value of ShowHyperlinks property changes.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void settingsPanel_ShowHyperlinksChanged(object sender, System.EventArgs e)
        {
            hasShowHyperlinkChanged = true;
        }
        /// <summary>
        /// Occurs when the value of ShowTablesOutput property changes.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void settingsPanel_ShowTablesOutputChanged(object sender, System.EventArgs e)
        {
            hasShowTablesChanged = true;
        }
        /// <summary>
        /// Occurs when the value of StatisticsLevel property changes.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void settingsPanel_StatisticsLevelChanged(object sender, System.EventArgs e)
        {
            hasStatisticsLevelChanged = true;

        }
        /// <summary>
        /// Occurs when the value of ProcessRecords property changes.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void settingsPanel_ProcessRecordsChanged(object sender, System.EventArgs e)
        {
            hasProcessRecordsChanged = true;

        }
        /// <summary>
        /// Occurs when the value of ShowIncludeMissing property changes.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void settingsPanel_ShowIncludeMissingChanged(object sender, System.EventArgs e)
        {
            hasIncludeMissingChanged = true;

        }

        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://wwwn.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-set.html");
        }

        #endregion //Event Handlers

        private void btnClear_Click(object sender, EventArgs e)
        {
            settingsPanel.ShowSettings();
        }

        /// <summary>
        /// Occurs when the OK button is clicked 
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnOK_Click(object sender, EventArgs e)
        {
            if (!this.isDialogMode)
            {
                base.btnOK_Click(sender, e);
            }
            else
            {
                base.btnSaveOnly_Click(sender, e);
            }
        }
	}
}