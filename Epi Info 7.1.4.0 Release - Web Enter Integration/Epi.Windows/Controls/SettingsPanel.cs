#region Namespaces

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Text;
using Epi.Data.Services;
using Epi.Windows;

#endregion

namespace Epi.Windows.Controls
{

	/// <summary>
	/// Settings Panel
	/// </summary>
    public partial class SettingsPanel : System.Windows.Forms.UserControl
	{

		#region Public Event Handlers

		/// <summary>
		/// Occurs when the value of RepresentationOfYes property changes.
		/// </summary>
		public event System.EventHandler RepresentationOfYesChanged;

		/// <summary>
		/// Occurs when the value of RepresentationOfNo property changes.
		/// </summary>
		public event System.EventHandler RepresentationOfNoChanged;

		/// <summary>
		/// Occurs when the value of RepresentationOfMissing property changes.
		/// </summary>
		public event System.EventHandler RepresentationOfMissingChanged;

		/// <summary>
		/// Occurs when the value of ShowSelectCriteria property changes.
		/// </summary>
		public event System.EventHandler ShowSelectCriteriaChanged;

		/// <summary>
		/// Occurs when the value of ShowGraphics property changes.
		/// </summary>
		public event System.EventHandler ShowGraphicsChanged;

		/// <summary>
		/// Occurs when the value of ShowHyperlinks property changes.
		/// </summary>
		public event System.EventHandler ShowHyperlinksChanged;

		/// <summary>
		/// Occurs when the value of ShowPrompy property changes.
		/// </summary>
		public event System.EventHandler ShowPromptChanged;

		/// <summary>
		/// Occurs when the value of ShowPercents property changes.
		/// </summary>
		public event System.EventHandler ShowPercentsChanged;

		/// <summary>
		/// Occurs when the value of ShowTablesOutput property changes.
		/// </summary>
		public event System.EventHandler ShowTablesOutputChanged;

		/// <summary>
		/// Occurs when the value of ShowIncludeMissing property changes.
		/// </summary>
		public event System.EventHandler ShowIncludeMissingChanged;

		/// <summary>
		/// Occurs when the value of StatisticsLevel property changes.
		/// </summary>
		public event System.EventHandler StatisticsLevelChanged;

		/// <summary>
		/// Occurs when the value of ProcessRecords property changes.
		/// </summary>
		public event System.EventHandler ProcessRecordsChanged;

		/// <summary>
		/// Occurs when the value of RepresentationOfYes property changes.
		/// </summary>
		public event System.EventHandler RepresentationOfYesTextChanged;

		/// <summary>
		/// Occurs when the value of RepresentationOfNo property changes.
		/// </summary>
		public event System.EventHandler RepresentationOfNoTextChanged;

		/// <summary>
		/// Occurs when the value of RepresentationOfMissing property changes.
		/// </summary>
		public event System.EventHandler RepresentationOfMissingTextChanged;
		
		#endregion

		#region Constructor
	
		/// <summary>
		/// Constructor for SettingsPanel
		/// </summary>
		public SettingsPanel()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
		}

		#endregion //Constructor

		#region Public Methods

		/// <summary>
		/// Loads data of the settings pannel.
		/// </summary>
		public void LoadMe()
		{
			LoadLists();
			ShowSettings();
		}

        /// <summary>
        /// Loads lists for representations of Yes, No and Missing
        /// </summary>
		public void LoadLists()
		{
			LoadRepresentionsOfYes();
			LoadRepresentionsOfNo();
			LoadRepresentionsOfMissing();
		}

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

		/// <summary>
		/// Saves the current settings to the configuration file.
		/// </summary>
        public void GetSettings(Configuration newConfig)
		{
            Epi.DataSets.Config.SettingsRow settings = newConfig.Settings;

            // Representation of boolean values ...
            settings.RepresentationOfYes = cmbYesAs.Text;
            settings.RepresentationOfNo = cmbNoAs.Text;
            settings.RepresentationOfMissing = cmbMissingAs.Text;

            // HTML output options ...
            settings.ShowCompletePrompt = cbxShowPrompt.Checked;
            settings.ShowSelection = cbxSelectCriteria.Checked;
            settings.ShowPercents = cbxPercents.Checked;
            settings.ShowGraphics = cbxGraphics.Checked;
            settings.ShowTables = cbxTablesOutput.Checked;
            settings.ShowHyperlinks = cbxHyperlinks.Checked;

            // Statistics Options
            settings.StatisticsLevel = int.Parse(WinUtil.GetSelectedRadioButton(gbxStatistics).Tag.ToString());
            settings.PrecisionForStatistics = numericUpDownPrecision.Value;

            // Record Processing
            settings.RecordProcessingScope = int.Parse(WinUtil.GetSelectedRadioButton(gbxProcessRecords).Tag.ToString());
            settings.IncludeMissingValues = cbxIncludeMissing.Checked;			
		}

		#endregion Public Methods

		#region Public Properties

		/// <summary>
		/// Gets a value indicating how to represent the value of Yes
		/// </summary>
		public string RepresentationOfYes
		{
			get
			{
				return cmbYesAs.Text;
			}
		}

		/// <summary>
		/// Gets a value indicating how to represent the value of No
		/// </summary>
		public string RepresentationOfNo
		{
			get
			{
				return cmbNoAs.Text;
			}
		}

		/// <summary>
		/// Gets a value indicating how to represent the value of Missing
		/// </summary>
		public string RepresentationOfMissing
		{
			get
			{
				return cmbMissingAs.Text;
			}
		}

		/// <summary>
		/// Gets a value indicating whether to display the full prompt for variable name
		/// </summary>
		public bool ShowPrompt
		{
			get
			{
				return cbxShowPrompt.Checked;
			}
		}

		/// <summary>
		/// Gets a value indicating whether to display frequency graph next to the table
		/// </summary>
		public bool ShowGraphics
		{
			get
			{
				return cbxGraphics.Checked;
			}
		}

		/// <summary>
		/// Gets a value indicating whether to include hyperlinks in the output
		/// </summary>
		public bool ShowHyperlinks
		{
			get
			{
				return cbxHyperlinks.Checked;
			}
		}

		/// <summary>
		/// Gets a value indicating whether to display the select criteria
		/// </summary>
		public bool ShowSelectCriteria
		{
			get
			{
				return cbxSelectCriteria.Checked;
			}
		}

		/// <summary>
		/// Gets a value indicating whether to include percantages in Tables output
		/// </summary>
		public bool ShowPercents
		{
			get
			{
				return cbxPercents.Checked;
			}
		}

		/// <summary>
		/// Gets a value indicating whether to include tabular data in Frequency and Tables output
		/// </summary>
		public bool ShowTablesOutput
		{
			get
			{
				return cbxTablesOutput.Checked;
			}
		}

		/// <summary>
		/// Gets a value indicating whether to include missing values in Analysis
		/// </summary>
		public bool ShowIncludeMissing
		{
			get
			{
				return cbxIncludeMissing.Checked;
			}
		}

		/// <summary>
		/// Gets a value indicating the level of detail desired in statistical procedures
		/// </summary>
		public RadioButton StatisticLevel
		{
			get
			{
                return WinUtil.GetSelectedRadioButton(gbxStatistics);
			}
		}

		/// <summary>
		/// Gets a value indicating whether to include deleted, undeleted or all records in statistical procedures
		/// </summary>
		public RadioButton ProcessRecords
		{
			get
			{
                return WinUtil.GetSelectedRadioButton(gbxProcessRecords);
			}
		}

		#endregion //Public Properties

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

		#region Event Handlers

		private void SettingsPanel_Load(object sender, System.EventArgs e)
		{
			//rdbNormal.Tag = ((short)RecordProcessingScope.Undeleted).ToString();  whut that heck?
			//LoadMe();

		}

		/// <summary>
		/// Occurs when RepresentationOfYes property changes. 
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cmbYesAs_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (this.RepresentationOfYesChanged != null) 
			{ 
				this.RepresentationOfYesChanged(sender, e); 
			} 

		}

		/// <summary>
		/// Occurs when RepresentationOfYes Text property changes. 
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cmbYesAs_TextChanged(object sender, System.EventArgs e)
		{
			if (this.RepresentationOfYesTextChanged != null) 
			{ 
				this.RepresentationOfYesTextChanged(sender, e); 
			} 

		}

		/// <summary>
		///Occurs when RepresentationOfNoproperty changes. 
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cmbNoAs_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (this.RepresentationOfNoChanged != null) 
			{ 
				this.RepresentationOfNoChanged(sender, e); 
			} 
		}

		private void cmbNoAs_TextChanged(object sender, EventArgs e)
		{
			if (this.RepresentationOfNoTextChanged != null)
			{
				this.RepresentationOfNoTextChanged(sender,e);
			}
		}

		/// <summary>
		/// Occurs when RepresentationOfMissing property changes. 
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cmbMissingAs_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (this.RepresentationOfMissingChanged != null) 
			{ 
				this.RepresentationOfMissingChanged(sender, e); 
			} 
		}

		/// <summary>
		/// Occurs when RepresentationOfMssing property changes.
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cmbMissingAs_TextChanged(object sender, EventArgs e)
		{
			if (this.RepresentationOfMissingTextChanged != null)
			{
				this.RepresentationOfMissingTextChanged(sender,e);
			}
		}

		/// <summary>
		/// Occurs when ShowPrompt property changes.
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cbxShowPrompt_CheckedChanged(object sender, System.EventArgs e)
		{
			if (this.ShowPromptChanged != null) 
			{ 
				this.ShowPromptChanged(sender, e); 
			} 
		}

		/// <summary>
		/// Occurs when ShowSelectCriteria property changes.
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cbxSelectCriteria_CheckedChanged(object sender, System.EventArgs e)
		{
			if (this.ShowSelectCriteriaChanged != null) 
			{ 
				this.ShowSelectCriteriaChanged(sender, e); 
			} 
		}

		/// <summary>
		/// Occurs when ShowGraphics property changes.
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cbxGraphics_CheckedChanged(object sender, System.EventArgs e)
		{
			if (this.ShowGraphicsChanged != null) 
			{ 
				this.ShowGraphicsChanged(sender, e); 
			} 
		}

		/// <summary>
		/// Occurs when ShowPercents property changes.
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cbxPercents_CheckedChanged(object sender, System.EventArgs e)
		{
			if (this.ShowPercentsChanged != null) 
			{ 
				this.ShowPercentsChanged(sender, e); 
			} 
		}

		/// <summary>
		/// Occurs when ShowHyperlinks property changes.
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cbxHyperlinks_CheckedChanged(object sender, System.EventArgs e)
		{
			if (this.ShowHyperlinksChanged != null) 
			{ 
				this.ShowHyperlinksChanged(sender, e); 
			} 
		}

		/// <summary>
		/// Occurs when ShowTablesOutput property changes.
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cbxTablesOutput_CheckedChanged(object sender, System.EventArgs e)
		{
			if (this.ShowTablesOutputChanged != null) 
			{ 
				this.ShowTablesOutputChanged(sender, e); 
			} 
		}

		/// <summary>
		/// Occurs when the value of StatisticsLevel property changes.
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void StatisticsRadioButtonClick(object sender, System.EventArgs e)
		{
			if (this.StatisticsLevelChanged != null) 
			{ 
				this.StatisticsLevelChanged(sender, e); 
			} 
			
		}

		/// <summary>
		///Occures when the value of ProcessRecords changes.
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void ProcessRecordsRadioButtonClick(object sender, System.EventArgs e)
		{
			if (this.ProcessRecordsChanged != null) 
			{ 
				this.ProcessRecordsChanged(sender, e); 
			} 
			
		}

		/// <summary>
		/// Occurs when ShowIncludeMissinge property changes.
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cbxIncludeMissing_CheckedChanged(object sender, System.EventArgs e)
		{
			if (this.ShowIncludeMissingChanged != null) 
			{ 
				this.ShowIncludeMissingChanged(sender, e); 
			} 
		}
		
		
		#endregion Event Handlers

		
		
	}
}
