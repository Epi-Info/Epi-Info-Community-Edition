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

            if ((cmbYesAs.Text != config.Settings.RepresentationOfYes) || (hasYesAsChanged))
            {
                kvPairs.Add(new KeyValuePair(ShortHands.YES, Util.InsertInDoubleQuotes(cmbYesAs.Text)));
            }
            if ((cmbNoAs.Text != config.Settings.RepresentationOfNo) || (hasNoAsChanged))
            {
                kvPairs.Add(new KeyValuePair(ShortHands.NO, Util.InsertInDoubleQuotes(cmbNoAs.Text)));
            }
            if ((cmbMissingAs.Text != config.Settings.RepresentationOfMissing) || (hasMissingAsChanged))
            {
                kvPairs.Add(new KeyValuePair(ShortHands.MISSING, Util.InsertInDoubleQuotes(cmbMissingAs.Text)));
            }
            if ((cbxGraphics.Checked != config.Settings.ShowGraphics) || (hasShowGraphicChanged))
            {
                kvPairs.Add(new KeyValuePair(CommandNames.FREQGRAPH,
                    Epi.Util.GetShortHand(cbxGraphics.Checked)));
            }
            if ((cbxHyperlinks.Checked != config.Settings.ShowHyperlinks) || (hasShowHyperlinkChanged))
            {
                kvPairs.Add(new KeyValuePair(CommandNames.HYPERLINKS,
                    Epi.Util.GetShortHand(cbxHyperlinks.Checked)));
            }
            if ((cbxPercents.Checked != config.Settings.ShowPercents) || (hasShowPercentsChanged))
            {
                kvPairs.Add(new KeyValuePair(CommandNames.PERCENTS,
                    Epi.Util.GetShortHand(cbxPercents.Checked)));
            }
            if ((cbxSelectCriteria.Checked != config.Settings.ShowSelection) || (hasShowSelectChanged))
            {
                kvPairs.Add(new KeyValuePair(CommandNames.SELECT,
                    Epi.Util.GetShortHand(cbxSelectCriteria.Checked)));
            }
            if ((cbxShowPrompt.Checked != config.Settings.ShowCompletePrompt) || (hasShowPromptChanged))
            {
                kvPairs.Add(new KeyValuePair(CommandNames.SHOWPROMPTS,
                    Epi.Util.GetShortHand(cbxShowPrompt.Checked)));
            }
            if ((cbxTablesOutput.Checked != config.Settings.ShowTables) || (hasShowTablesChanged))
            {
                kvPairs.Add(new KeyValuePair(CommandNames.TABLES,
                    Epi.Util.GetShortHand(cbxTablesOutput.Checked)));
            }
            if ((cbxIncludeMissing.Checked != config.Settings.IncludeMissingValues) || (hasIncludeMissingChanged))
            {
                kvPairs.Add(new KeyValuePair(CommandNames.MISSING,
                    Epi.Util.GetShortHand(cbxIncludeMissing.Checked)));
            }
            if (hasStatisticsLevelChanged)
            {
                StatisticsLevel levelIdSelected = (StatisticsLevel)short.Parse(WinUtil.GetSelectedRadioButton(gbxStatistics).Tag.ToString());
                string levelTagSelected = AppData.Instance.GetStatisticsLevelById(levelIdSelected).Tag;
                kvPairs.Add(new KeyValuePair(CommandNames.STATISTICS, levelTagSelected));
            }
            if (hasProcessRecordsChanged)
            {
                RecordProcessingScope scopeIdSelected = (RecordProcessingScope)short.Parse(WinUtil.GetSelectedRadioButton(gbxProcessRecords).Tag.ToString());
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

        #region Public Methods

        /// <summary>
        /// Loads the settings from the configuration file.
        /// </summary>
        public void ShowSettings(Configuration pConfig = null)
        {
            Configuration config = null;

            if (pConfig == null)
            {
                config = Configuration.GetNewInstance();
            }
            else
            {
                config = pConfig;
            }



            Epi.DataSets.Config.SettingsRow settings = config.Settings;

            // Representation of boolean values ...
            cmbYesAs.SelectedItem = settings.RepresentationOfYes;
            cmbNoAs.SelectedItem = settings.RepresentationOfNo;
            cmbMissingAs.SelectedItem = settings.RepresentationOfMissing;

            // HTML output options ...
            cbxShowPrompt.Checked = settings.ShowCompletePrompt;
            cbxSelectCriteria.Checked = settings.ShowSelection;
            cbxPercents.Checked = settings.ShowPercents;
            cbxGraphics.Checked = settings.ShowGraphics;
            cbxHyperlinks.Checked = settings.ShowHyperlinks;
            cbxTablesOutput.Checked = settings.ShowTables;

            // Statistics Options
            WinUtil.SetSelectedRadioButton(settings.StatisticsLevel.ToString(), gbxStatistics);
            numericUpDownPrecision.Value = settings.PrecisionForStatistics;

            // Record Processing
            WinUtil.SetSelectedRadioButton(settings.RecordProcessingScope.ToString(), gbxProcessRecords);
            cbxIncludeMissing.Checked = settings.IncludeMissingValues;
        }
        #endregion //Public Methods

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

        #region Private Methods

        /// <summary>
        /// Loads combobox with values for RepresentationsOfYes from application data file
        /// </summary>
        private void LoadRepresentionsOfYes()
        {
            string currentRepresentationOfYes = Configuration.GetNewInstance().Settings.RepresentationOfYes;
            DataView dv = AppData.Instance.RepresentationsOfYesDataTable.DefaultView;
            cmbYesAs.Items.Clear();
            if (!string.IsNullOrEmpty(currentRepresentationOfYes))
            {
                cmbYesAs.Items.Add(currentRepresentationOfYes);
            }
            dv.Sort = ColumnNames.POSITION;
            foreach (DataRowView row in dv)
            {
                string name = row[ColumnNames.NAME].ToString();
                if (name != currentRepresentationOfYes)
                {
                    cmbYesAs.Items.Add(row[ColumnNames.NAME]);
                }
            }
            cmbYesAs.SelectedIndex = 0;
        }

        /// <summary>
        /// Loads combobox with values for RepresentationsOfNo from application data file
        /// </summary>
        private void LoadRepresentionsOfNo()
        {
            string currentRepresentationOfNo = Configuration.GetNewInstance().Settings.RepresentationOfNo;
            DataView dv = AppData.Instance.RepresentationsOfNoDataTable.DefaultView;
            cmbNoAs.Items.Clear();
            if (!string.IsNullOrEmpty(currentRepresentationOfNo))
            {
                cmbNoAs.Items.Add(currentRepresentationOfNo);
            }
            dv.Sort = ColumnNames.POSITION;
            foreach (DataRowView row in dv)
            {
                string name = row[ColumnNames.NAME].ToString();
                if (name != currentRepresentationOfNo)
                {
                    cmbNoAs.Items.Add(name);
                }
            }
            cmbNoAs.SelectedIndex = 0;
        }

        /// <summary>
        /// Loads combobox with values for RepresentationsOfMissing from application data file
        /// </summary>
        private void LoadRepresentionsOfMissing()
        {
            string currentRepresentationOfMissing = Configuration.GetNewInstance().Settings.RepresentationOfMissing;
            DataView dv = AppData.Instance.RepresentationsOfMissingDataTable.DefaultView;
            cmbMissingAs.Items.Clear();
            if (!string.IsNullOrEmpty(currentRepresentationOfMissing))
            {
                cmbMissingAs.Items.Add(currentRepresentationOfMissing);
            }
            dv.Sort = ColumnNames.POSITION;
            foreach (DataRowView row in dv)
            {
                string name = row[ColumnNames.NAME].ToString();
                if (name != currentRepresentationOfMissing)
                {
                    cmbMissingAs.Items.Add(name);
                }
            }
            cmbMissingAs.SelectedIndex = 0;
        }


        #endregion Private Methods
	
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
            this.LoadRepresentionsOfYes();
            this.LoadRepresentionsOfNo();
            this.LoadRepresentionsOfMissing();
            this.ShowSettings(config);
            ResetHasChangedValues(false);
        }

        //##########################################################

        		/// <summary>
		/// Occurs when RepresentationOfYes property changes. 
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cmbYesAs_SelectedIndexChanged(object sender, System.EventArgs e)
		{		
            hasYesAsChanged = true;
		}

		/// <summary>
		/// Occurs when RepresentationOfYes Text property changes. 
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cmbYesAs_TextChanged(object sender, System.EventArgs e)
		{
            hasYesAsChanged = true;
		}

		/// <summary>
		///Occurs when RepresentationOfNoproperty changes. 
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cmbNoAs_SelectedIndexChanged(object sender, System.EventArgs e)
		{
            hasNoAsChanged = true;
		}

		private void cmbNoAs_TextChanged(object sender, EventArgs e)
		{
            hasNoAsChanged = true;
		}

		/// <summary>
		/// Occurs when RepresentationOfMissing property changes. 
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cmbMissingAs_SelectedIndexChanged(object sender, System.EventArgs e)
		{
            hasMissingAsChanged = true;
		}

		/// <summary>
		/// Occurs when RepresentationOfMssing property changes.
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cmbMissingAs_TextChanged(object sender, EventArgs e)
		{
            hasMissingAsChanged = true;
		}

		/// <summary>
		/// Occurs when ShowPrompt property changes.
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cbxShowPrompt_CheckedChanged(object sender, System.EventArgs e)
		{
            hasShowPromptChanged = true;
		}

		/// <summary>
		/// Occurs when ShowSelectCriteria property changes.
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cbxSelectCriteria_CheckedChanged(object sender, System.EventArgs e)
		{
            hasShowSelectChanged = true;
		}

		/// <summary>
		/// Occurs when ShowGraphics property changes.
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cbxGraphics_CheckedChanged(object sender, System.EventArgs e)
		{
            hasShowGraphicChanged = true;
		}

		/// <summary>
		/// Occurs when ShowPercents property changes.
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cbxPercents_CheckedChanged(object sender, System.EventArgs e)
		{
            hasShowPercentsChanged = true;
		}

		/// <summary>
		/// Occurs when ShowHyperlinks property changes.
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cbxHyperlinks_CheckedChanged(object sender, System.EventArgs e)
		{
            hasShowHyperlinkChanged = true;
		}

		/// <summary>
		/// Occurs when ShowTablesOutput property changes.
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cbxTablesOutput_CheckedChanged(object sender, System.EventArgs e)
		{
            hasShowTablesChanged = true;
		}

		/// <summary>
		/// Occurs when the value of StatisticsLevel property changes.
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void StatisticsRadioButtonClick(object sender, System.EventArgs e)
		{
            hasStatisticsLevelChanged = true;
		}

		/// <summary>
		///Occures when the value of ProcessRecords changes.
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void ProcessRecordsRadioButtonClick(object sender, System.EventArgs e)
		{
            hasProcessRecordsChanged = true;
		}

		/// <summary>
		/// Occurs when ShowIncludeMissinge property changes.
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cbxIncludeMissing_CheckedChanged(object sender, System.EventArgs e)
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
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/command-reference/analysis-commands-set.html");
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ShowSettings();
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

        #endregion //Event Handlers

    }
}