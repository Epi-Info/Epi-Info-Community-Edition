namespace Epi.Windows.Dialogs
{
    partial class OptionsDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		#region Designer generated code

	
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsDialog));
            this.tabControlDateTime = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label9 = new System.Windows.Forms.Label();
            this.txtMapKey = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.cmbDefaultDataFormat = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbDatabaseFormat = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.btnEllipse = new System.Windows.Forms.Button();
            this.txtWorkingDirectory = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnUserInterfaceSettings = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnUseDefault = new System.Windows.Forms.Button();
            this.btnBrowseImage = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.pbxPreview = new System.Windows.Forms.PictureBox();
            this.tabLanguage = new System.Windows.Forms.TabPage();
            this.statusStrip = new System.Windows.Forms.Label();
            this.btnDeleteTranslation = new System.Windows.Forms.Button();
            this.btnNew = new System.Windows.Forms.Button();
            this.btnImportTranslation = new System.Windows.Forms.Button();
            this.btnManageLanguages = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lbxLanguages = new System.Windows.Forms.ListBox();
            this.tabAnalysis = new System.Windows.Forms.TabPage();
            this.settingsPanel = new Epi.Windows.Controls.SettingsPanel();
            this.tabPlugIns = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btnRemoveGadget = new System.Windows.Forms.Button();
            this.lbxDashboardGadgetPlugins = new System.Windows.Forms.ListBox();
            this.btnImportGadget = new System.Windows.Forms.Button();
            this.groupDataDrivers = new System.Windows.Forms.GroupBox();
            this.btnRemoveDriver = new System.Windows.Forms.Button();
            this.btnImportDriver = new System.Windows.Forms.Button();
            this.lbxDataDriverPlugins = new System.Windows.Forms.ListBox();
            this.tabPageWebSurvey = new System.Windows.Forms.TabPage();
            this.txtEndpoint = new System.Windows.Forms.TextBox();
            this.BindingGroupBox = new System.Windows.Forms.GroupBox();
            this.rbWSHTTP = new System.Windows.Forms.RadioButton();
            this.rbBasic = new System.Windows.Forms.RadioButton();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.rbNoWindows = new System.Windows.Forms.RadioButton();
            this.rbUseWindows = new System.Windows.Forms.RadioButton();
            this.lblEndpoint = new System.Windows.Forms.Label();
            this.tabPageWebEnter = new System.Windows.Forms.TabPage();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.EWErbWSHTTP = new System.Windows.Forms.RadioButton();
            this.EWErbBasic = new System.Windows.Forms.RadioButton();
            this.EWEEndPointTextBox = new System.Windows.Forms.TextBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.EWErbNoWindows = new System.Windows.Forms.RadioButton();
            this.EWErbUseWindows = new System.Windows.Forms.RadioButton();
            this.label10 = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnRestoreDefaults = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.openFileDialogDataDriver = new System.Windows.Forms.OpenFileDialog();
            this.tabControlDateTime.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbxPreview)).BeginInit();
            this.tabLanguage.SuspendLayout();
            this.tabAnalysis.SuspendLayout();
            this.tabPlugIns.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupDataDrivers.SuspendLayout();
            this.tabPageWebSurvey.SuspendLayout();
            this.BindingGroupBox.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.tabPageWebEnter.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.SuspendLayout();
            // 
            // baseImageList
            // 
            this.baseImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            resources.ApplyResources(this.baseImageList, "baseImageList");
            this.baseImageList.ImageStream = null;
            // 
            // tabControlDateTime
            // 
            this.tabControlDateTime.Controls.Add(this.tabGeneral);
            this.tabControlDateTime.Controls.Add(this.tabLanguage);
            this.tabControlDateTime.Controls.Add(this.tabAnalysis);
            this.tabControlDateTime.Controls.Add(this.tabPlugIns);
            this.tabControlDateTime.Controls.Add(this.tabPageWebSurvey);
            this.tabControlDateTime.Controls.Add(this.tabPageWebEnter);
            resources.ApplyResources(this.tabControlDateTime, "tabControlDateTime");
            this.tabControlDateTime.Name = "tabControlDateTime";
            this.tabControlDateTime.SelectedIndex = 0;
            this.tabControlDateTime.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.groupBox5);
            this.tabGeneral.Controls.Add(this.groupBox1);
            this.tabGeneral.Controls.Add(this.label7);
            this.tabGeneral.Controls.Add(this.btnEllipse);
            this.tabGeneral.Controls.Add(this.txtWorkingDirectory);
            this.tabGeneral.Controls.Add(this.groupBox3);
            this.tabGeneral.Controls.Add(this.groupBox2);
            resources.ApplyResources(this.tabGeneral, "tabGeneral");
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.linkLabel1);
            this.groupBox5.Controls.Add(this.label9);
            this.groupBox5.Controls.Add(this.txtMapKey);
            this.groupBox5.Controls.Add(this.label1);
            resources.ApplyResources(this.groupBox5, "groupBox5");
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.TabStop = false;
            // 
            // linkLabel1
            // 
            resources.ApplyResources(this.linkLabel1, "linkLabel1");
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.TabStop = true;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            // 
            // txtMapKey
            // 
            resources.ApplyResources(this.txtMapKey, "txtMapKey");
            this.txtMapKey.Name = "txtMapKey";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.cmbDefaultDataFormat);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.cmbDatabaseFormat);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // cmbDefaultDataFormat
            // 
            resources.ApplyResources(this.cmbDefaultDataFormat, "cmbDefaultDataFormat");
            this.cmbDefaultDataFormat.Name = "cmbDefaultDataFormat";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // cmbDatabaseFormat
            // 
            resources.ApplyResources(this.cmbDatabaseFormat, "cmbDatabaseFormat");
            this.cmbDatabaseFormat.Name = "cmbDatabaseFormat";
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // btnEllipse
            // 
            resources.ApplyResources(this.btnEllipse, "btnEllipse");
            this.btnEllipse.Name = "btnEllipse";
            this.btnEllipse.Click += new System.EventHandler(this.btnEllipse_Click);
            // 
            // txtWorkingDirectory
            // 
            resources.ApplyResources(this.txtWorkingDirectory, "txtWorkingDirectory");
            this.txtWorkingDirectory.Name = "txtWorkingDirectory";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnUserInterfaceSettings);
            this.groupBox3.Controls.Add(this.label5);
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // btnUserInterfaceSettings
            // 
            resources.ApplyResources(this.btnUserInterfaceSettings, "btnUserInterfaceSettings");
            this.btnUserInterfaceSettings.Name = "btnUserInterfaceSettings";
            this.btnUserInterfaceSettings.Click += new System.EventHandler(this.btnUserInterfaceSettings_Click);
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnUseDefault);
            this.groupBox2.Controls.Add(this.btnBrowseImage);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.pbxPreview);
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // btnUseDefault
            // 
            resources.ApplyResources(this.btnUseDefault, "btnUseDefault");
            this.btnUseDefault.Name = "btnUseDefault";
            this.btnUseDefault.Click += new System.EventHandler(this.btnUseDefault_Click);
            // 
            // btnBrowseImage
            // 
            resources.ApplyResources(this.btnBrowseImage, "btnBrowseImage");
            this.btnBrowseImage.Name = "btnBrowseImage";
            this.btnBrowseImage.Click += new System.EventHandler(this.btnBrowseImage_Click);
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // pbxPreview
            // 
            this.pbxPreview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            resources.ApplyResources(this.pbxPreview, "pbxPreview");
            this.pbxPreview.Name = "pbxPreview";
            this.pbxPreview.TabStop = false;
            // 
            // tabLanguage
            // 
            this.tabLanguage.Controls.Add(this.statusStrip);
            this.tabLanguage.Controls.Add(this.btnDeleteTranslation);
            this.tabLanguage.Controls.Add(this.btnNew);
            this.tabLanguage.Controls.Add(this.btnImportTranslation);
            this.tabLanguage.Controls.Add(this.btnManageLanguages);
            this.tabLanguage.Controls.Add(this.label4);
            this.tabLanguage.Controls.Add(this.label3);
            this.tabLanguage.Controls.Add(this.lbxLanguages);
            resources.ApplyResources(this.tabLanguage, "tabLanguage");
            this.tabLanguage.Name = "tabLanguage";
            this.tabLanguage.UseVisualStyleBackColor = true;
            // 
            // statusStrip
            // 
            resources.ApplyResources(this.statusStrip, "statusStrip");
            this.statusStrip.Name = "statusStrip";
            // 
            // btnDeleteTranslation
            // 
            resources.ApplyResources(this.btnDeleteTranslation, "btnDeleteTranslation");
            this.btnDeleteTranslation.Name = "btnDeleteTranslation";
            this.btnDeleteTranslation.Click += new System.EventHandler(this.btnDeleteTranslation_Click);
            // 
            // btnNew
            // 
            resources.ApplyResources(this.btnNew, "btnNew");
            this.btnNew.Name = "btnNew";
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // btnImportTranslation
            // 
            resources.ApplyResources(this.btnImportTranslation, "btnImportTranslation");
            this.btnImportTranslation.Name = "btnImportTranslation";
            this.btnImportTranslation.Click += new System.EventHandler(this.btnImportTranslation_Click);
            // 
            // btnManageLanguages
            // 
            resources.ApplyResources(this.btnManageLanguages, "btnManageLanguages");
            this.btnManageLanguages.Name = "btnManageLanguages";
            this.btnManageLanguages.Click += new System.EventHandler(this.btnManageLanguages_Click);
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // lbxLanguages
            // 
            this.lbxLanguages.Items.AddRange(new object[] {
            resources.GetString("lbxLanguages.Items")});
            resources.ApplyResources(this.lbxLanguages, "lbxLanguages");
            this.lbxLanguages.Name = "lbxLanguages";
            this.lbxLanguages.SelectedIndexChanged += new System.EventHandler(this.lbxLanguages_SelectedIndexChanged);
            // 
            // tabAnalysis
            // 
            this.tabAnalysis.Controls.Add(this.settingsPanel);
            resources.ApplyResources(this.tabAnalysis, "tabAnalysis");
            this.tabAnalysis.Name = "tabAnalysis";
            this.tabAnalysis.UseVisualStyleBackColor = true;
            // 
            // settingsPanel
            // 
            resources.ApplyResources(this.settingsPanel, "settingsPanel");
            this.settingsPanel.Name = "settingsPanel";
            this.settingsPanel.RepresentationOfYesChanged += new System.EventHandler(this.settingsPanel_SettingChanged);
            this.settingsPanel.RepresentationOfNoChanged += new System.EventHandler(this.settingsPanel_SettingChanged);
            this.settingsPanel.RepresentationOfMissingChanged += new System.EventHandler(this.settingsPanel_SettingChanged);
            this.settingsPanel.ShowSelectCriteriaChanged += new System.EventHandler(this.settingsPanel_SettingChanged);
            this.settingsPanel.ShowGraphicsChanged += new System.EventHandler(this.settingsPanel_SettingChanged);
            this.settingsPanel.ShowHyperlinksChanged += new System.EventHandler(this.settingsPanel_SettingChanged);
            this.settingsPanel.ShowPromptChanged += new System.EventHandler(this.settingsPanel_SettingChanged);
            this.settingsPanel.ShowPercentsChanged += new System.EventHandler(this.settingsPanel_SettingChanged);
            this.settingsPanel.ShowTablesOutputChanged += new System.EventHandler(this.settingsPanel_SettingChanged);
            this.settingsPanel.ShowIncludeMissingChanged += new System.EventHandler(this.settingsPanel_SettingChanged);
            this.settingsPanel.StatisticsLevelChanged += new System.EventHandler(this.settingsPanel_SettingChanged);
            this.settingsPanel.ProcessRecordsChanged += new System.EventHandler(this.settingsPanel_SettingChanged);
            // 
            // tabPlugIns
            // 
            this.tabPlugIns.Controls.Add(this.groupBox4);
            this.tabPlugIns.Controls.Add(this.groupDataDrivers);
            resources.ApplyResources(this.tabPlugIns, "tabPlugIns");
            this.tabPlugIns.Name = "tabPlugIns";
            this.tabPlugIns.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btnRemoveGadget);
            this.groupBox4.Controls.Add(this.lbxDashboardGadgetPlugins);
            this.groupBox4.Controls.Add(this.btnImportGadget);
            resources.ApplyResources(this.groupBox4, "groupBox4");
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.TabStop = false;
            // 
            // btnRemoveGadget
            // 
            resources.ApplyResources(this.btnRemoveGadget, "btnRemoveGadget");
            this.btnRemoveGadget.Name = "btnRemoveGadget";
            this.btnRemoveGadget.UseVisualStyleBackColor = true;
            // 
            // lbxDashboardGadgetPlugins
            // 
            this.lbxDashboardGadgetPlugins.FormattingEnabled = true;
            resources.ApplyResources(this.lbxDashboardGadgetPlugins, "lbxDashboardGadgetPlugins");
            this.lbxDashboardGadgetPlugins.Name = "lbxDashboardGadgetPlugins";
            // 
            // btnImportGadget
            // 
            resources.ApplyResources(this.btnImportGadget, "btnImportGadget");
            this.btnImportGadget.Name = "btnImportGadget";
            this.btnImportGadget.UseVisualStyleBackColor = true;
            this.btnImportGadget.Click += new System.EventHandler(this.btnImportGadget_Click);
            // 
            // groupDataDrivers
            // 
            this.groupDataDrivers.Controls.Add(this.btnRemoveDriver);
            this.groupDataDrivers.Controls.Add(this.btnImportDriver);
            this.groupDataDrivers.Controls.Add(this.lbxDataDriverPlugins);
            resources.ApplyResources(this.groupDataDrivers, "groupDataDrivers");
            this.groupDataDrivers.Name = "groupDataDrivers";
            this.groupDataDrivers.TabStop = false;
            // 
            // btnRemoveDriver
            // 
            resources.ApplyResources(this.btnRemoveDriver, "btnRemoveDriver");
            this.btnRemoveDriver.Name = "btnRemoveDriver";
            this.btnRemoveDriver.UseVisualStyleBackColor = true;
            this.btnRemoveDriver.Click += new System.EventHandler(this.btnRemoveSource_Click);
            // 
            // btnImportDriver
            // 
            resources.ApplyResources(this.btnImportDriver, "btnImportDriver");
            this.btnImportDriver.Name = "btnImportDriver";
            this.btnImportDriver.UseVisualStyleBackColor = true;
            this.btnImportDriver.Click += new System.EventHandler(this.btnImportSource_Click);
            // 
            // lbxDataDriverPlugins
            // 
            this.lbxDataDriverPlugins.FormattingEnabled = true;
            resources.ApplyResources(this.lbxDataDriverPlugins, "lbxDataDriverPlugins");
            this.lbxDataDriverPlugins.Name = "lbxDataDriverPlugins";
            // 
            // tabPageWebSurvey
            // 
            this.tabPageWebSurvey.Controls.Add(this.txtEndpoint);
            this.tabPageWebSurvey.Controls.Add(this.BindingGroupBox);
            this.tabPageWebSurvey.Controls.Add(this.groupBox6);
            this.tabPageWebSurvey.Controls.Add(this.lblEndpoint);
            resources.ApplyResources(this.tabPageWebSurvey, "tabPageWebSurvey");
            this.tabPageWebSurvey.Name = "tabPageWebSurvey";
            this.tabPageWebSurvey.UseVisualStyleBackColor = true;
            // 
            // txtEndpoint
            // 
            resources.ApplyResources(this.txtEndpoint, "txtEndpoint");
            this.txtEndpoint.Name = "txtEndpoint";
            // 
            // BindingGroupBox
            // 
            this.BindingGroupBox.Controls.Add(this.rbWSHTTP);
            this.BindingGroupBox.Controls.Add(this.rbBasic);
            resources.ApplyResources(this.BindingGroupBox, "BindingGroupBox");
            this.BindingGroupBox.Name = "BindingGroupBox";
            this.BindingGroupBox.TabStop = false;
            // 
            // rbWSHTTP
            // 
            resources.ApplyResources(this.rbWSHTTP, "rbWSHTTP");
            this.rbWSHTTP.Name = "rbWSHTTP";
            this.rbWSHTTP.TabStop = true;
            this.rbWSHTTP.UseVisualStyleBackColor = true;
            // 
            // rbBasic
            // 
            resources.ApplyResources(this.rbBasic, "rbBasic");
            this.rbBasic.Name = "rbBasic";
            this.rbBasic.TabStop = true;
            this.rbBasic.UseVisualStyleBackColor = true;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.rbNoWindows);
            this.groupBox6.Controls.Add(this.rbUseWindows);
            resources.ApplyResources(this.groupBox6, "groupBox6");
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.TabStop = false;
            // 
            // rbNoWindows
            // 
            resources.ApplyResources(this.rbNoWindows, "rbNoWindows");
            this.rbNoWindows.Name = "rbNoWindows";
            this.rbNoWindows.TabStop = true;
            this.rbNoWindows.UseVisualStyleBackColor = true;
            // 
            // rbUseWindows
            // 
            resources.ApplyResources(this.rbUseWindows, "rbUseWindows");
            this.rbUseWindows.Name = "rbUseWindows";
            this.rbUseWindows.TabStop = true;
            this.rbUseWindows.UseVisualStyleBackColor = true;
            this.rbUseWindows.CheckedChanged += new System.EventHandler(this.rbUseWindows_CheckedChanged);
            // 
            // lblEndpoint
            // 
            resources.ApplyResources(this.lblEndpoint, "lblEndpoint");
            this.lblEndpoint.Name = "lblEndpoint";
            // 
            // tabPageWebEnter
            // 
            this.tabPageWebEnter.Controls.Add(this.groupBox8);
            this.tabPageWebEnter.Controls.Add(this.EWEEndPointTextBox);
            this.tabPageWebEnter.Controls.Add(this.groupBox7);
            this.tabPageWebEnter.Controls.Add(this.label10);
            resources.ApplyResources(this.tabPageWebEnter, "tabPageWebEnter");
            this.tabPageWebEnter.Name = "tabPageWebEnter";
            this.tabPageWebEnter.UseVisualStyleBackColor = true;
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.EWErbWSHTTP);
            this.groupBox8.Controls.Add(this.EWErbBasic);
            resources.ApplyResources(this.groupBox8, "groupBox8");
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.TabStop = false;
            // 
            // EWErbWSHTTP
            // 
            resources.ApplyResources(this.EWErbWSHTTP, "EWErbWSHTTP");
            this.EWErbWSHTTP.Name = "EWErbWSHTTP";
            this.EWErbWSHTTP.TabStop = true;
            this.EWErbWSHTTP.UseVisualStyleBackColor = true;
            // 
            // EWErbBasic
            // 
            resources.ApplyResources(this.EWErbBasic, "EWErbBasic");
            this.EWErbBasic.Name = "EWErbBasic";
            this.EWErbBasic.TabStop = true;
            this.EWErbBasic.UseVisualStyleBackColor = true;
            // 
            // EWEEndPointTextBox
            // 
            resources.ApplyResources(this.EWEEndPointTextBox, "EWEEndPointTextBox");
            this.EWEEndPointTextBox.Name = "EWEEndPointTextBox";
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.EWErbNoWindows);
            this.groupBox7.Controls.Add(this.EWErbUseWindows);
            resources.ApplyResources(this.groupBox7, "groupBox7");
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.TabStop = false;
            // 
            // EWErbNoWindows
            // 
            resources.ApplyResources(this.EWErbNoWindows, "EWErbNoWindows");
            this.EWErbNoWindows.Name = "EWErbNoWindows";
            this.EWErbNoWindows.TabStop = true;
            this.EWErbNoWindows.UseVisualStyleBackColor = true;
            // 
            // EWErbUseWindows
            // 
            resources.ApplyResources(this.EWErbUseWindows, "EWErbUseWindows");
            this.EWErbUseWindows.Name = "EWErbUseWindows";
            this.EWErbUseWindows.TabStop = true;
            this.EWErbUseWindows.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            resources.ApplyResources(this.label10, "label10");
            this.label10.Name = "label10";
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources(this.btnOk, "btnOk");
            this.btnOk.Name = "btnOk";
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnRestoreDefaults
            // 
            resources.ApplyResources(this.btnRestoreDefaults, "btnRestoreDefaults");
            this.btnRestoreDefaults.Name = "btnRestoreDefaults";
            this.btnRestoreDefaults.Click += new System.EventHandler(this.btnRestoreDefaults_Click);
            // 
            // btnApply
            // 
            resources.ApplyResources(this.btnApply, "btnApply");
            this.btnApply.Name = "btnApply";
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnHelp
            // 
            resources.ApplyResources(this.btnHelp, "btnHelp");
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.UseVisualStyleBackColor = true;
            // 
            // openFileDialogDataDriver
            // 
            resources.ApplyResources(this.openFileDialogDataDriver, "openFileDialogDataDriver");
            // 
            // OptionsDialog
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.tabControlDateTime);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.btnRestoreDefaults);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsDialog";
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.Options_Load);
            this.tabControlDateTime.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbxPreview)).EndInit();
            this.tabLanguage.ResumeLayout(false);
            this.tabLanguage.PerformLayout();
            this.tabAnalysis.ResumeLayout(false);
            this.tabPlugIns.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupDataDrivers.ResumeLayout(false);
            this.tabPageWebSurvey.ResumeLayout(false);
            this.tabPageWebSurvey.PerformLayout();
            this.BindingGroupBox.ResumeLayout(false);
            this.BindingGroupBox.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.tabPageWebEnter.ResumeLayout(false);
            this.tabPageWebEnter.PerformLayout();
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.ResumeLayout(false);

		}
		#endregion



        private System.Windows.Forms.TabControl tabControlDateTime;
		private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TabPage tabLanguage;
		private System.Windows.Forms.TabPage tabGeneral;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.PictureBox pbxPreview;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button btnBrowseImage;
        private System.Windows.Forms.Button btnUserInterfaceSettings;
		private System.Windows.Forms.TabPage tabAnalysis;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.ListBox lbxLanguages;
		private System.Windows.Forms.Button btnImportTranslation;
		private System.Windows.Forms.Button btnManageLanguages;
		private System.Windows.Forms.Button btnNew;
		private System.Windows.Forms.Button btnDeleteTranslation;
		private System.Windows.Forms.Button btnRestoreDefaults;
		private System.Windows.Forms.Button btnApply;
		private System.Windows.Forms.Label label7;
		private Epi.Windows.Controls.SettingsPanel settingsPanel;
		private System.Windows.Forms.TextBox txtWorkingDirectory;
        private System.Windows.Forms.Button btnEllipse;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbDatabaseFormat;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cmbDefaultDataFormat;
        private System.Windows.Forms.TabPage tabPlugIns;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Button btnUseDefault;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button btnRemoveGadget;
        private System.Windows.Forms.ListBox lbxDashboardGadgetPlugins;
        private System.Windows.Forms.Button btnImportGadget;
        private System.Windows.Forms.GroupBox groupDataDrivers;
        private System.Windows.Forms.Button btnRemoveDriver;
        private System.Windows.Forms.Button btnImportDriver;
        private System.Windows.Forms.ListBox lbxDataDriverPlugins;
        private System.Windows.Forms.OpenFileDialog openFileDialogDataDriver;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtMapKey;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label statusStrip;
        private System.Windows.Forms.TabPage tabPageWebSurvey;
        private System.Windows.Forms.Label lblEndpoint;
        private System.Windows.Forms.TextBox txtEndpoint;
        private System.Windows.Forms.GroupBox BindingGroupBox;
        private System.Windows.Forms.RadioButton rbWSHTTP;
        private System.Windows.Forms.RadioButton rbBasic;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.RadioButton rbNoWindows;
        private System.Windows.Forms.RadioButton rbUseWindows;
        private System.Windows.Forms.TabPage tabPageWebEnter;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.RadioButton EWErbWSHTTP;
        private System.Windows.Forms.RadioButton EWErbBasic;
        private System.Windows.Forms.TextBox EWEEndPointTextBox;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.RadioButton EWErbNoWindows;
        private System.Windows.Forms.RadioButton EWErbUseWindows;
        private System.Windows.Forms.Label label10;
    }
}