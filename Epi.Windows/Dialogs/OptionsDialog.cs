using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

using Epi;
using Epi.Resources;

using Epi.Data.Services;
using Epi.DataSets;
using Epi.Diagnostics;
using Epi.Windows.Globalization;
using Epi.Windows.Globalization.Forms;
using Epi.Windows.Globalization.Translators;

namespace Epi.Windows.Dialogs
{

	/// <summary>
	/// Dialog for setting system options
	/// </summary>
    public partial class OptionsDialog : DialogBase
	{
		#region Private Class Members
		private string imgfilename = string.Empty;
        private Configuration config;
		#endregion

        #region Enums
        /// <summary>
        /// Enumeration for tabs in the dialog
        /// </summary>
        public enum OptionsTabIndex
        {
            /// <summary>
            /// Enum for the General tab
            /// </summary>
            General = 0,

            /// <summary>
            /// Enum for the Language tab
            /// </summary>
            Language = 1,

            /// <summary>
            /// Enum for the Advanced tab
            /// </summary>
            Advanced = 2
        }
        #endregion Enums

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        [Obsolete("Use of default constructor not allowed", true)]
        public OptionsDialog()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
		/// Constructor
		/// </summary>
		/// <param name="frm"></param>
		public OptionsDialog(MainForm frm) : base(frm)
		{
			InitializeComponent();
            Construct();
		}

        /// <summary>
        /// Constructor to load with a specified tab
        /// </summary>
        /// <param name="frm"></param>
        /// <param name="tabIndex">The index of the Options tab selected</param>        
        public OptionsDialog(MainForm frm, OptionsTabIndex tabIndex)
            : base(frm)
        {
            InitializeComponent();
            Construct();
            this.tabControlDateTime.SelectTab((int)tabIndex);
        }

        private void Construct()
        {
            config = Configuration.GetNewInstance();
        }
		#endregion Constructors
		
		#region Public Delegates
        /// <summary>
        /// Declaration of delegate Apply Changes Handler
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="args">.NET supplied event parameters</param>
		public delegate void ApplyChangesHandler(object sender, EventArgs args);
		#endregion

		#region Public Events

        /// <summary>
        /// Declaration for the Apply Changes Handler
        /// </summary>
		public event ApplyChangesHandler ApplyChanges;
		#endregion

		#region Public Properties
		/// <summary>
		/// Accessor method for imgfilename
		/// </summary>
		public string ImgFileName
		{
			get
			{
				return imgfilename;
			}
			set
			{
				imgfilename = value;
			}
		}		
		#endregion Public Properties

		#region Event Handlers

		/// <summary>
		/// Handles the selected index changed event of the default data format combo box
		/// </summary>
		/// <param name="sender">.NET supplied object</param>
		/// <param name="e">.NET supplied event parameter</param>
		private void cmbDefaultDataFormat_SelectedIndexChanged(object sender, System.EventArgs e)
		{		
		}

		
		/// <summary>
		/// Displays EpiInfo.MNU in Wordpad
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void btnUserInterfaceSettings_Click(object sender, System.EventArgs e)
		{
			//System.Diagnostics.Process.Start("WordPad.exe");
			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo("WordPad.exe");
            startInfo.WorkingDirectory = config.Directories.Configuration;
            startInfo.Arguments = StringLiterals.DOUBLEQUOTES;
            startInfo.Arguments += Files.MnuFilePath;
            startInfo.Arguments += StringLiterals.DOUBLEQUOTES;
            startInfo.Verb = "Open";
			startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Maximized;
			System.Diagnostics.Process.Start(startInfo);

		}

		/// <summary>
		/// Attach data into combo boxes and disables Apply button
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void Options_Load(object sender, System.EventArgs e)
		{
            LoadLanguages();

            WinUtil.FillDataDriversList(cmbDatabaseFormat);

            //ctt7 commented out while working on data driver load mechanism
            //WinUtil.FillDataFormatsForRead(cmbDefaultDataFormat);
            LoadRepresentionsOfYes();
            LoadRepresentionsOfNo();
            LoadRepresentionsOfMissing();
            ShowSettings();
			btnApply.Enabled = false;

            LoadPluginTab();
		}

		/// <summary>
		/// Display database description for the selected data source
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cbxDataSources_SelectedIndexChanged(object sender, System.EventArgs e)
		{
            //ComboBox cmb = sender as ComboBox;
            //DbFormatType format = (DbFormatType)(int.Parse(cmb.SelectedValue.ToString()));
            //string description = AppData.Instance.GetDbFormatRow(format).Description;
            //lblDbDescription.Text = Localization.LocalizeString(description);
		}
		
        //private void settingsPanel_SettingChanged(object sender, System.EventArgs e)
        //{
        //    btnApply.Enabled = true;
        //}

        private void settingsPanelDateTime_SettingChanged(object sender, System.EventArgs e)
        {
            btnApply.Enabled = true;
        }

		/// <summary>
		/// Handles the click event of the Apply button
		/// </summary>
		/// <param name="sender">.NET supplied object</param>
		/// <param name="e">.NET supplied event parameter</param>
		private void btnApply_Click(object sender, System.EventArgs e)
		{
			Save();			

			btnApply.Enabled = false;
		}

		/// <summary>
		/// Handles the click event of the OK button
		/// </summary>
		/// <param name="sender">.NET supplied object</param>
		/// <param name="e">.NET supplied event parameter</param>
		private void btnOk_Click(object sender, System.EventArgs e)
		{
			Save();
            Close();
		}

		/// <summary>
		/// Cancel closes this dialog
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		/// <summary>
		/// FolderBrowserDialog is displayed and the selected path is set as the working directory
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void btnEllipse_Click(object sender, System.EventArgs e)
		{
			FolderBrowserDialog dialog = new FolderBrowserDialog();
			dialog.SelectedPath = txtWorkingDirectory.Text;
			DialogResult result = dialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				txtWorkingDirectory.Text = dialog.SelectedPath;
				this.btnApply.Enabled = true;
			}
		}

		/// <summary>
		/// Handles the selected index change of the languages list box
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void lbxLanguages_SelectedIndexChanged(object sender, System.EventArgs e)
		{
            btnDeleteTranslation.Enabled = (lbxLanguages.SelectedIndex > 0);
			this.btnApply.Enabled = true;
		}

		/// <summary>
		/// Resets the selections made
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void btnRestoreDefaults_Click(object sender, System.EventArgs e)
		{
			ShowSettings();
			// LoadDataFormats();
			this.btnApply.Enabled = false;
		}

		/// <summary>
		/// Event handler for SelectionChangeCommitted for all the combo boxes in this form
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void cmb_SelectionChangeCommitted(object sender, System.EventArgs e)
		{
			this.btnApply.Enabled = true;
		}

		/// <summary>
		/// Event handler for CheckChanged for all the checkboxes in this form
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void chkCommon_CheckedChanged(object sender, System.EventArgs e)
		{
			this.btnApply.Enabled = true;
		}

		/// <summary>
		/// Handles the selected index change of the tab control
		/// </summary>
		/// <param name="sender">Object that fired the event.</param>
		/// <param name="e">.NET supplied event args.</param>
		private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
		{
            //if(tabControl1.SelectedIndex == 2)
            //{
            //    ComboBox cmb = sender as ComboBox;
            //    DbFormatType format = (DbFormatType)(int.Parse(cmb.SelectedValue.ToString()));
            //    string description = AppData.Instance.GetDbFormatRow(format).Description;
            //    lblDbDescription.Text = Localization.LocalizeString(description);
            //}
		}

            #region Analysis Tab
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

        #endregion Event handlers

        #region Private Functions

        private void LoadPluginTab()
        {
            //lbxDataSourcePlugins.Items.Clear();
            DataView dv = config.DataDrivers.DefaultView;
            dv.Sort = "DisplayName";
            lbxDataDriverPlugins.DataSource = dv;
            lbxDataDriverPlugins.DisplayMember = "DisplayName";
            lbxDataDriverPlugins.ValueMember = "Type";

            dv = config.Gadget.DefaultView;
            if (dv.Count > 0)
            {
                dv.Sort = "DisplayName";
                lbxDashboardGadgetPlugins.DataSource = dv;
                lbxDashboardGadgetPlugins.DisplayMember = "DisplayName";
                lbxDashboardGadgetPlugins.ValueMember = "Type";
            }
        }

		/// <summary>
		/// Loads the working directory set by the user
		/// </summary>
        private void ShowSettings(Configuration pConfig = null)
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

            try
            {
                autoTouchKeyboard.Checked = settings.AutoTouchKeyboard;
            }
            catch { }

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
			//settingsPanel.ShowSettings();
			txtWorkingDirectory.Text = config.Directories.Working;
            txtMapKey.Text = config.Settings.MapServiceKey;
            object selectedItem = null;
            foreach (object item in lbxLanguages.Items)
            {
                if (((DropDownListItem)item).Key.Equals(config.Settings.Language))
                {
                    selectedItem = item;
                    break;
                }
            }
            lbxLanguages.SelectedItem = selectedItem;
			cmbDatabaseFormat.SelectedValue = config.Settings.DefaultDataDriver;
            cmbDefaultDataFormat.SelectedValue = config.Settings.DefaultDataFormatForRead;

            /*if (config.Settings.WebServiceAuthMode == null)
            {
                rbUseWindows.Checked = false;
                rbNoWindows.Checked = true;
            }
            else
            {*/

                if (config.Settings.WebServiceAuthMode == 1)
                {
                    rbUseWindows.Checked = true;
                }
                else
                {
                    rbNoWindows.Checked = true;
                }
                try
                {
                    if (config.Settings.EWEServiceAuthMode == 1)
                    {
                        EWErbUseWindows.Checked = true;
                    }
                    else
                    {
                        EWErbNoWindows.Checked = true;
                    }
                }
                catch (Exception)
                {
                    Configuration _config = Configuration.GetNewInstance();
                    _config.Settings.EWEServiceAuthMode = 0; // 0 = Anon, 1 = NT
                    Configuration.Save(_config);

                }
            //}
                try { 
               
                    if (config.Settings.WebServiceBindingMode == "wshttp")
                    {
                        rbWSHTTP.Checked = true;
                    }
                    else
                    {
                        rbBasic.Checked = true;
                    }
                    if (config.Settings.EWEServiceBindingMode == "wshttp")
                        {
                        EWErbWSHTTP.Checked = true;
                        }
                    else
                        {
                        EWErbBasic.Checked = true;
                        }
                }
                catch (Exception ex)
                {
                    rbBasic.Checked = true;
                   
                }

            txtEndpoint.Text = config.Settings.WebServiceEndpointAddress;
            EWEEndPointTextBox.Text = config.Settings.EWEServiceEndpointAddress;
		}

        private void LoadLanguages()
        {
            string installPath = Path.GetDirectoryName(Application.ExecutablePath);
            string searchPath = installPath.Trim().ToLower();

            if (searchPath[searchPath.Length - 1] != Path.DirectorySeparatorChar)
                searchPath += Path.DirectorySeparatorChar;

            if (Directory.Exists(searchPath))
            {
                List<string> files = new List<string>();
                string[] subdirs = Directory.GetDirectories(searchPath);
                foreach (string dir in subdirs)
                {
                    files.AddRange(Directory.GetFiles(dir, "*.Resources.dll", SearchOption.TopDirectoryOnly));
                }

                List<string> index = new List<string>();
                foreach (string file in files)
                {
                    string path = Path.GetDirectoryName(file).ToLower();
                    if (!index.Contains(path))
                    {
                        index.Add(path);
                    }
                }

                index.Sort();

                lbxLanguages.Items.Clear();

                foreach (string key in index)
                {
                    if (key == searchPath) continue;

                    string cultureName = key.Substring(key.LastIndexOf(Path.DirectorySeparatorChar) + 1);

                    CultureInfo ci;
                    try
                    {
                        ci = new CultureInfo(cultureName);
                    }
                    catch (ArgumentException)
                    {
                        continue;
                    }
                    int i = lbxLanguages.Items.Add(new DropDownListItem(cultureName, ci.EnglishName, key));
                }

                lbxLanguages.Items.Insert(0,
                    new DropDownListItem(
                    "",
                    "English (default)",
                    null)
                );

                lbxLanguages.SelectedIndex = 0;
            }
        }

		/// <summary>
		/// Saves all option changes to Configuration.Current.
		/// </summary>
		private void Save()
		{
            GetSettings(config);

            config.Directories.Working = txtWorkingDirectory.Text;
            config.Settings.MapServiceKey = txtMapKey.Text;

            if (lbxLanguages.SelectedItem != null)
            {
                if (string.Compare(config.Settings.Language, ((DropDownListItem)lbxLanguages.SelectedItem).Key) != 0)
                {
                    string sourceCulture = ((DropDownListItem)lbxLanguages.SelectedItem).Key;
                    System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(sourceCulture);
                    MsgBox.ShowInformation(SharedStrings.CHANGE_NOT_EFFECT_UNTIL_RESTART);
                    config.Settings.Language = sourceCulture;
                    this.Invalidate();
                }
            }

            if (rbUseWindows.Checked)
            {
                config.Settings.WebServiceAuthMode = 1;
            }
            else
            {
                config.Settings.WebServiceAuthMode = 0;
            }

            if (EWErbUseWindows.Checked)
                {
                config.Settings.EWEServiceAuthMode = 1;
                }
            else
                {
                config.Settings.EWEServiceAuthMode = 0;
                }

            if (rbWSHTTP.Checked)
            {
                config.Settings.WebServiceBindingMode = "wshttp";
            }
            else
            {
                config.Settings.WebServiceBindingMode = "basic";
            }
            if (EWErbWSHTTP.Checked)
                {
                config.Settings.EWEServiceBindingMode = "wshttp";
                }
            else
                {
                config.Settings.EWEServiceBindingMode = "basic";
                }
            config.Settings.WebServiceEndpointAddress = txtEndpoint.Text;
            config.Settings.EWEServiceEndpointAddress = this.EWEEndPointTextBox.Text;
            // Default data driver
            config.Settings.DefaultDataDriver = cmbDatabaseFormat.SelectedValue.ToString();

            Configuration.Save(config);
            if (this.ApplyChanges != null)
            {
                this.ApplyChanges(this, new EventArgs());
            }

            Configuration.Load(config.ConfigFilePath);
		}

        /// <summary>
        /// Saves the current settings to the configuration file.
        /// </summary>
        public void GetSettings(Configuration newConfig)
        {
            Epi.DataSets.Config.SettingsRow settings = newConfig.Settings;

            settings.AutoTouchKeyboard = autoTouchKeyboard.Checked;

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

        private void btnRemoveSource_Click(object sender, EventArgs e)
        {
            string s = string.Empty;

            if (lbxDataDriverPlugins.SelectedItem != null)
            {
                s = ((DataRowView)lbxDataDriverPlugins.SelectedItem).Row.ItemArray[1].ToString().ToLower();

                switch (s)
                {
                    case "epi.data.office.csvfilefactory, epi.data.office":
                    case "epi.data.office.accessdbfactory, epi.data.office":
                    case "epi.data.office.access2007dbfactory, epi.data.office":
                    case "epi.data.office.excel2007wbfactory, epi.data.office":
                    case "epi.data.office.excelwbfactory, epi.data.office":
                    case "epi.data.sqlserver.sqldbfactory, epi.data.sqlserver":
                        MsgBox.ShowInformation("The default Epi Info data sources may not be removed.");
                        return;
                }
            }

            if (string.IsNullOrEmpty(s))
            {
                return;
            }

            Config.DataDriverRow driverRowToRemove = null;

            foreach(Config.DataDriverRow driverRow in config.DataDrivers) 
            {
                if (driverRow[1].ToString().ToLower().Equals(s))
                {
                    driverRowToRemove = driverRow;
                    break;
                }                
            }            

            config.DataDrivers.RemoveDataDriverRow(driverRowToRemove);
            Configuration.Save(config);            
            LoadPluginTab();
        }

        private void btnImportGadget_Click(object sender, EventArgs e)
        {
            string xmlFilePath = string.Empty;
            string xmlFileName = string.Empty;
            string dllFileName = string.Empty;
            string directory = string.Empty;

            DialogResult result = openFileDialogDataDriver.ShowDialog(this);

            if (result.Equals(DialogResult.OK))
            {
                xmlFilePath = openFileDialogDataDriver.FileName;
                xmlFileName = openFileDialogDataDriver.SafeFileName;
                directory = xmlFilePath.Remove(xmlFilePath.Length - xmlFileName.Length, xmlFileName.Length);

                XmlDocument doc = new XmlDocument();

                if (!xmlFilePath.Equals(""))
                {
                    doc.Load(xmlFilePath);
                    XmlNode dataDriverNode = doc.DocumentElement.SelectSingleNode("/Gadget");

                    if (dataDriverNode != null)
                    {
                        XmlNode menuPlacement = doc.DocumentElement.SelectSingleNode("/Gadget/MenuPlacement");

                        string displayName = dataDriverNode.Attributes.GetNamedItem("DisplayName").Value;
                        string type = dataDriverNode.Attributes.GetNamedItem("Type").Value;
                        dllFileName = dataDriverNode.Attributes.GetNamedItem("FileName").Value;
                        int menuSection = int.Parse(menuPlacement.InnerText);

                        string[] filePaths = Directory.GetFiles(directory, dllFileName);

                        string currentFolder = Directory.GetCurrentDirectory();

                        foreach (string path in filePaths)
                        {
                            int index = path.LastIndexOf('\\');
                            string libraryFileName = path.Remove(0, index);
                            File.Copy(path, currentFolder + libraryFileName, true);
                        }

                        Configuration.OnGadgetImport(displayName, type, dllFileName, menuSection);
                        config = Configuration.GetNewInstance();
                        LoadPluginTab();
                    }
                }
            }
        }

        private void btnImportSource_Click(object sender, EventArgs e)
        {
            string xmlFilePath = string.Empty;
            string xmlFileName = string.Empty;
            string dllFileName = string.Empty;
            string directory = string.Empty;

            DialogResult result = openFileDialogDataDriver.ShowDialog(this);

            if (result.Equals(DialogResult.OK))
            {
                xmlFilePath = openFileDialogDataDriver.FileName;
                xmlFileName = openFileDialogDataDriver.SafeFileName;
                directory = xmlFilePath.Remove(xmlFilePath.Length - xmlFileName.Length, xmlFileName.Length);

                XmlDocument doc = new XmlDocument();

                if (!xmlFilePath.Equals(""))
                {
                    doc.Load(xmlFilePath);
                    XmlNode dataDriverNode = doc.DocumentElement.SelectSingleNode("/DataDriver");

                    if (dataDriverNode != null)
                    {
                        XmlNode metadataProviderNode = doc.DocumentElement.SelectSingleNode("/DataDriver/MetadataProvider");
                        XmlNode dataProviderNode = doc.DocumentElement.SelectSingleNode("/DataDriver/DataProvider");

                        string displayName = dataDriverNode.Attributes.GetNamedItem("DisplayName").Value;
                        string type = dataDriverNode.Attributes.GetNamedItem("Type").Value;
                        dllFileName = dataDriverNode.Attributes.GetNamedItem("FileName").Value;
                        bool isMetadataProvider = bool.Parse(metadataProviderNode.InnerText);
                        bool isDataProvider = bool.Parse(dataProviderNode.InnerText);

                        string[] filePaths = Directory.GetFiles(directory, dllFileName);

                        string currentFolder = Directory.GetCurrentDirectory();

                        foreach (string path in filePaths)
                        {
                            int index = path.LastIndexOf('\\');
                            string libraryFileName = path.Remove(0, index);
                            File.Copy(path, currentFolder + libraryFileName, true);
                        }

                        Configuration.OnDataDriverImport(displayName, type, dllFileName, isMetadataProvider, isDataProvider);

                        config = Configuration.GetNewInstance();

                        LoadPluginTab();
                    }
                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.bingmapsportal.com/");
        }

        private void btnManageLanguages_Click(object sender, EventArgs e)
        {
            Epi.Windows.Globalization.Forms.LocalizationManager lm = new Globalization.Forms.LocalizationManager();
            lm.ShowDialog();
            lbxLanguages.Items.Clear();
            LoadLanguages();
            object selectedItem = null;
            foreach (object item in lbxLanguages.Items)
            {
                if (((DropDownListItem)item).Key.Equals(config.Settings.Language))
                {
                    selectedItem = item;
                    break;
                }
            }
            lbxLanguages.SelectedItem = selectedItem;
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            SaveFileDialog exportSaveFileDialog = new SaveFileDialog();
            exportSaveFileDialog.Filter = "Microsoft Access File | *.mdb";
            if (exportSaveFileDialog.ShowDialog() != DialogResult.OK) return;

            string exportFilePath = exportSaveFileDialog.FileName;

            string sourceCulture;
            if (lbxLanguages.SelectedItem != null)
                sourceCulture = ((DropDownListItem)lbxLanguages.SelectedItem).Key;
            else
                sourceCulture = ((DropDownListItem)lbxLanguages.Items[0]).Key;

            string targetCulture = null;

            ExtractionMode mode = ExtractionMode.None;

            string installPath = Path.GetDirectoryName(Application.ExecutablePath);

            string languageDbPath = string.Empty;

            System.Threading.WaitCallback callback = new System.Threading.WaitCallback(this.QueueExctractionWorkerThread);

            object[] args = new object[6];
            args[0] = installPath;
            args[1] = sourceCulture;
            args[2] = targetCulture;
            args[3] = mode;
            args[4] = exportFilePath;
            args[5] = languageDbPath;

            System.Threading.ThreadPool.QueueUserWorkItem(callback, (object)args);
        }

        private void QueueExctractionWorkerThread(object state)
        {
            object[] args = state as object[];
            string installPath = (string)args[0];
            string sourceCulture = (string)args[1];
            string targetCulture = (string)args[2] ?? ResourceTool.DEFAULT_CULTURE;
            ExtractionMode mode = (ExtractionMode)args[3];
            string exportFilePath = (string)args[4];
            string languageDbPath = (string)args[5];

            try
            {
                ITranslator translator;
                switch (mode)
                {
                    case ExtractionMode.DatabaseTranslation:
                        translator = new LegacyLanguageDbTranslator();
                        ((LegacyLanguageDbTranslator)translator).ReadDatabase(languageDbPath);
                        break;
                    case ExtractionMode.Double:
                        translator = new ExpandedStringTranslator();
                        break;
                    case ExtractionMode.Reverse:
                        translator = new ReverseStringTranslator();
                        break;
                    case ExtractionMode.WebTranslation:
                        translator = new WebServiceTranslator();
                        break;
                    default:
                        translator = new NormalizedStringTranslator();
                        break;
                }
                ResourceTool.ExportLanguage(installPath, sourceCulture, targetCulture, translator, exportFilePath, new AppendLogCallback(this.AppendToLog));
                AppendToLog(string.Empty);
                MsgBox.ShowInformation(SharedStrings.LANGUAGE_READY);
            }
            catch (Exception ex)
            {
                MsgBox.ShowException(ex);
            }

        }

        private void AppendToLog(string message)
        {

            if (this.statusStrip.InvokeRequired)
            {
                AppendLogCallback del = new AppendLogCallback(this.AppendToLog);
                this.statusStrip.Invoke(del, message);
            }
            else
            {
                this.statusStrip.Text = message;
                Application.DoEvents();
            }
        }

        private void btnImportTranslation_Click(object sender, EventArgs e)
        {
            Import.ShowImportDialog(Path.GetDirectoryName(Application.ExecutablePath));             
            lbxLanguages.Items.Clear();
            LoadLanguages();
            RefreshLanguages();
        }

        private void btnDeleteTranslation_Click(object sender, EventArgs e)
        {
            try
            {
                if (lbxLanguages.SelectedIndex > 0)
                {
                    if (MessageBox.Show(SharedStrings.CONFIRM_LANGUAGE_DELETE, SharedStrings.CONFIRMATION, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
                    {

                        string language = ((DropDownListItem)lbxLanguages.SelectedItem).Key;
                        lbxLanguages.Items.Remove(lbxLanguages.SelectedItem);
                        System.IO.Directory.Delete(Path.GetDirectoryName(Application.ExecutablePath) + "\\" + language, true);
                        lbxLanguages.SelectedIndex = 0;
                        if (string.Compare(config.Settings.Language, ((DropDownListItem)lbxLanguages.SelectedItem).Key) != 0)
                        {
                            string sourceCulture = ((DropDownListItem)lbxLanguages.SelectedItem).Key;
                            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(sourceCulture);
                            config.Settings.Language = sourceCulture;
                            Configuration.Save(config);
                            if (this.ApplyChanges != null)
                            {
                                this.ApplyChanges(this, new EventArgs());
                            }
                        }
                        RefreshLanguages();
                    }
                }
            }
            catch (Exception ex)
            {
                MsgBox.ShowError(SharedStrings.LANGUAGE_REMOVAL_FAIL);
            }
        }

        private void RefreshLanguages()
        {
            object selectedItem = null;
            foreach (object item in lbxLanguages.Items)
            {
                if (((DropDownListItem)item).Key.Equals(config.Settings.Language))
                {
                    selectedItem = item;
                    break;
                }
            }
            lbxLanguages.SelectedItem = selectedItem;
        }

        private void rbUseWindows_CheckedChanged(object sender, EventArgs e)
        {
            if (rbUseWindows.Checked)
            {
                rbBasic.Checked = true;
                BindingGroupBox.Enabled = false;
            }
            else
            {
                BindingGroupBox.Enabled = true;
            }
        }

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

        #region Protected Events
        /// <summary>
        /// Opens a process to show the related help topic
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        protected override void btnHelp_Click(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.cdc.gov/epiinfo/user-guide/getting-started/introduction.html");
        }

        #endregion Protected Events

        private void EWErbUseWindows_CheckedChanged(object sender, EventArgs e)
        {
            if (EWErbUseWindows.Checked)
            {
                EWErbBasic.Checked = true;
                groupBox8.Enabled = false;
            }
            else
            {
                groupBox8.Enabled = true;
            }
        }

        private void Ping_Click(object sender, EventArgs e)
        {
            try
            {
                Save();
                string Message = ValidateEIWSSittings();
                if (Message.Contains("Success"))
                {
                    MessageBox.Show(SharedStrings.WEBSURVEY_PING_SUCCESS, "", MessageBoxButtons.OK);
                }
                else
                {
                    MessageBox.Show(Message, "", MessageBoxButtons.OK);
                }
            }catch(Exception ex){
                MessageBox.Show(SharedStrings.WEBSURVEY_PING_ERROR, "", MessageBoxButtons.OK);
            }

        }
        private string  ValidateEIWSSittings()
        {
            string  IsValid = "Success";

            try
            {
                
                SurveyManagerServiceV3.ManagerServiceV3Client client = Epi.Core.ServiceClient.ServiceClient.GetClientV3();
                SurveyManagerServiceV3.OrganizationRequest Request = new SurveyManagerServiceV3.OrganizationRequest();
                SurveyManagerServiceV3.OrganizationDTO orgDTO = new SurveyManagerServiceV3.OrganizationDTO();
                Request.Organization = orgDTO;
                var Result = client.GetOrganization(Request);

            }
            catch (Exception ex)
            {
                IsValid = ex.InnerException.ToString();
            }
            return IsValid;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
            Save();
            string Message = ValidateEWESittings();
            if (Message.Contains("Success"))
            {
                MessageBox.Show(SharedStrings.WEBSURVEY_PING_SUCCESS, "", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show(Message, "", MessageBoxButtons.OK);
            }
             }catch(Exception ex){
                 MessageBox.Show(SharedStrings.WEBSURVEY_PING_ERROR, "", MessageBoxButtons.OK);
            }
        }
        private string ValidateEWESittings()
        {
            string IsValid = "Success";

            try
            {
                 
                EWEManagerService.EWEManagerServiceClient   client = Epi.Core.ServiceClient.EWEServiceClient.GetClient();
                EWEManagerService.OrganizationRequest Request = new EWEManagerService.OrganizationRequest();
                EWEManagerService.OrganizationDTO orgDTO = new EWEManagerService.OrganizationDTO();
                Request.Organization = orgDTO;
                var Result = client.GetOrganization(Request);

            }
            catch (Exception ex)
            {
                IsValid = ex.InnerException.ToString();
            }
            return IsValid;
        }

        private void txtEndpoint_TextChanged(object sender, EventArgs e)
        {
            string URL =((System.Windows.Forms.TextBox)(sender)).Text;
            if (!string.IsNullOrEmpty (URL) && URL.ToUpper().Contains(".SVC"))
            {
            Ping.Enabled = true;
            }else{
             Ping.Enabled = false;
            }
        }

        private void EWEEndPointTextBox_TextChanged(object sender, EventArgs e)
       {
            string URL = ((System.Windows.Forms.TextBox)(sender)).Text;
            if (!string.IsNullOrEmpty(URL) && URL.ToUpper().Contains(".SVC"))
            {
                button1.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
            }
        }

    }

    /// <summary>
    /// class DropDownListItem
    /// </summary>
    public class DropDownListItem
    {
        #region Implementation
        private string key;

        /// <summary>
        /// Return the Key
        /// </summary>
        public string Key
        {
            get { return key; }
            set { key = value; }
        }

        private object value;

        /// <summary>
        /// Return the Value
        /// </summary>
        public object Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        private string text;

        /// <summary>
        /// Text Property
        /// </summary>
        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        /// <summary>
        /// DropDownListItem constructor
        /// </summary>
        /// <param name="key"></param>
        /// <param name="text"></param>
        /// <param name="value"></param>
        public DropDownListItem(string key, string text, object value)
        {
            this.key = key;
            this.value = value;
            this.text = text;
        }

        /// <summary>
        /// Returns string representation of this
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return text.ToString();
        }
        #endregion
    }
}

