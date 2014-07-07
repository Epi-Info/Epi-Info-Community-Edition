using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Epi.Windows.Dialogs;

namespace Epi.Windows.MakeView.Dialogs
{
	/// <summary>
	/// Dialog to modify the global Grid Settings
	/// </summary>
    public partial class GridSettingsDialog : DialogBase
	{
        /// <summary>
        /// Settings Saved
        /// </summary>
        public event EventHandler SettingsSaved;

        #region Constructors

        /// <summary>
		/// Constructor for the class
		/// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
		public GridSettingsDialog()
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();
		}

        /// <summary>
        /// Constructor for Grid Settings dialog
        /// </summary>
        /// <param name="frm">The main form</param>
		public GridSettingsDialog(MainForm frm) : base(frm)
        {
            InitializeComponent();
            this.udGridSize.Validating += new CancelEventHandler(OnSettingChanged);
            this.Validating += new CancelEventHandler(OnSettingChanged);
            LoadSettings();
        }

        #endregion

        #region Event Handlers

        private void OnSettingChanged(object sender, CancelEventArgs e)
        {
            if (!ValidateInput())
            {
                e.Cancel = true;
            }
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

        /// <summary>
        /// Handles the click event of the OK button
        /// </summary>
        /// <param name="sender">.NET supplied object</param>
        /// <param name="e">.NET supplied event parameter</param>
        private void btnOK_Click(object sender, System.EventArgs e)
        {
            Save();
            if (SettingsSaved != null)
            {
                SettingsSaved(this, new EventArgs());
            }
            Close();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads the settings information from the Configuration file
        /// </summary>
        private void LoadSettings()
        {
            Configuration config = Configuration.GetNewInstance();
            chkSnapToGrid.Checked = config.Settings.SnapToGrid;
            chkShowGrid.Checked = config.Settings.ShowGrid;
            udGridSize.Text = config.Settings.GridSize.ToString();
            udGridSize.Value = config.Settings.GridSize;
            if (config.Settings.SnapPromptToGrid)
            {
                rbSnapToPrompt.Checked = true;
            }
            if (config.Settings.SnapInputControlToGrid)
            {
                rbSnapToField.Checked = true;
            }
            gbSnapToGrid.Enabled = chkSnapToGrid.Checked;
        }

        /// <summary>
        /// Saves the settings information to the Configuration file
        /// </summary>
        private void Save()
        {
            if (this.Validate())
            {
                Configuration config = Configuration.GetNewInstance();
                config.Settings.SnapToGrid = chkSnapToGrid.Checked;
                config.Settings.SnapPromptToGrid = rbSnapToPrompt.Checked;
                config.Settings.SnapInputControlToGrid = rbSnapToField.Checked;
                config.Settings.GridSize = Convert.ToInt32(udGridSize.Text);
                config.Settings.ShowGrid = chkShowGrid.Checked;
                Configuration.Save(config);
            }
        }

        private void chkSnapToGrid_CheckedChanged(object sender, EventArgs e)
        {
            gbSnapToGrid.Enabled = chkSnapToGrid.Checked;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// ValidateInput
        /// </summary>
        /// <returns>bool</returns>
        protected override bool ValidateInput()
        {
            if(String.IsNullOrEmpty(udGridSize.Text))
            {
                udGridSize.Text = "1";
                udGridSize.Value = 1;
            }
            
            if (Convert.ToInt32(udGridSize.Text)<1)
            {
                udGridSize.Text = "1";
                udGridSize.Value = 1;
            }

            if (Convert.ToInt32(udGridSize.Text) > 6)
            {
                udGridSize.Text = "6";
                udGridSize.Value = 6;
            }
            bool isValid = (!String.IsNullOrEmpty(udGridSize.Text) && (rbSnapToField.Checked || rbSnapToPrompt.Checked));
            btnOK.Enabled = isValid;
            return isValid;
        }

        #endregion
	}
}

