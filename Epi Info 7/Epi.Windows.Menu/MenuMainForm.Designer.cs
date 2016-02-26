using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Data;
using System.Windows.Forms;
using Epi;
using Epi.Windows;
using Epi.Windows.Dialogs;
using System.Reflection;
using Epi.Windows.Menu.Dialogs;

namespace Epi.Windows.Menu
{
    partial class MenuMainForm
    {
        private System.Windows.Forms.MenuStrip mnuMainMenu;
        //private System.Windows.Forms.ToolStripMenuItem mniMenuItem;
        //private System.Windows.Forms.ToolStripMenuItem mniSubmenuItem;
        private System.Windows.Forms.ImageList imlImageList;
        //private System.Windows.Forms.PictureBox pbxPictureBox;
        private System.Windows.Forms.PictureBox pbxBackground;
        //private System.Windows.Forms.StatusBar sbStatusBar;
        //private System.Windows.Forms.StatusBarPanel sbpStatusBarPanel;
        //private System.Windows.Forms.ToolBar tbrToolBar;
        //private System.Windows.Forms.ToolBarButton tbbToolBarButton;
        //private System.Windows.Forms.ListBox lbxListBox;
        private System.Windows.Forms.ToolTip tltToolTip;
        private System.Windows.Forms.Button btnMakeView;
        private System.Windows.Forms.Button btnAnalyze;
        private System.Windows.Forms.Button btnEnterData;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnCreateMaps;
        private System.Windows.Forms.Button btnCreateReports;
        private System.Windows.Forms.Button btnWebsite;
        private System.ComponentModel.IContainer components = null;

        #region Designer generated code

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuMainForm));
            this.pbxBackground = new System.Windows.Forms.PictureBox();
            this.btnWebsite = new System.Windows.Forms.Button();
            this.btnCreateReports = new System.Windows.Forms.Button();
            this.btnCreateMaps = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnEnterData = new System.Windows.Forms.Button();
            this.btnAnalyze = new System.Windows.Forms.Button();
            this.btnMakeView = new System.Windows.Forms.Button();
            this.tltToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.imlImageList = new System.Windows.Forms.ImageList(this.components);
            this.mnuMainMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemStatusBar = new System.Windows.Forms.ToolStripMenuItem();
            this.epiInfoLogsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.makeViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enterDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.analyzeDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.classicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dashboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createMapsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statCalcToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sampleSizeAndPowerToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.populationSurveyToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.cohortOrCrossSectionalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unmatchedCasecontrolToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.chiSquareForTrendToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.tables2X22XNToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPoisson = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuBinomial = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMatchedPairCaseControl = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.videosToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.communityMessageBoardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contactHelpdeskToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.otherEpiResourcesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.activEpicomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openEpicomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutEpiInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnDashboard = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbxBackground)).BeginInit();
            this.mnuMainMenu.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // baseImageList
            // 
            this.baseImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("baseImageList.ImageStream")));
            this.baseImageList.Images.SetKeyName(0, "");
            this.baseImageList.Images.SetKeyName(1, "");
            this.baseImageList.Images.SetKeyName(2, "");
            this.baseImageList.Images.SetKeyName(3, "");
            this.baseImageList.Images.SetKeyName(4, "");
            this.baseImageList.Images.SetKeyName(5, "");
            this.baseImageList.Images.SetKeyName(6, "");
            this.baseImageList.Images.SetKeyName(7, "");
            this.baseImageList.Images.SetKeyName(8, "");
            this.baseImageList.Images.SetKeyName(9, "");
            this.baseImageList.Images.SetKeyName(10, "");
            this.baseImageList.Images.SetKeyName(11, "");
            this.baseImageList.Images.SetKeyName(12, "");
            this.baseImageList.Images.SetKeyName(13, "");
            this.baseImageList.Images.SetKeyName(14, "");
            this.baseImageList.Images.SetKeyName(15, "");
            this.baseImageList.Images.SetKeyName(16, "");
            this.baseImageList.Images.SetKeyName(17, "");
            this.baseImageList.Images.SetKeyName(18, "");
            this.baseImageList.Images.SetKeyName(19, "");
            this.baseImageList.Images.SetKeyName(20, "");
            this.baseImageList.Images.SetKeyName(21, "");
            this.baseImageList.Images.SetKeyName(22, "");
            this.baseImageList.Images.SetKeyName(23, "");
            this.baseImageList.Images.SetKeyName(24, "");
            this.baseImageList.Images.SetKeyName(25, "");
            this.baseImageList.Images.SetKeyName(26, "");
            this.baseImageList.Images.SetKeyName(27, "");
            this.baseImageList.Images.SetKeyName(28, "");
            this.baseImageList.Images.SetKeyName(29, "");
            this.baseImageList.Images.SetKeyName(30, "");
            this.baseImageList.Images.SetKeyName(31, "");
            this.baseImageList.Images.SetKeyName(32, "");
            this.baseImageList.Images.SetKeyName(33, "");
            this.baseImageList.Images.SetKeyName(34, "");
            this.baseImageList.Images.SetKeyName(35, "");
            this.baseImageList.Images.SetKeyName(36, "");
            this.baseImageList.Images.SetKeyName(37, "");
            this.baseImageList.Images.SetKeyName(38, "");
            this.baseImageList.Images.SetKeyName(39, "");
            this.baseImageList.Images.SetKeyName(40, "");
            this.baseImageList.Images.SetKeyName(41, "");
            this.baseImageList.Images.SetKeyName(42, "");
            this.baseImageList.Images.SetKeyName(43, "");
            this.baseImageList.Images.SetKeyName(44, "");
            this.baseImageList.Images.SetKeyName(45, "");
            this.baseImageList.Images.SetKeyName(46, "");
            this.baseImageList.Images.SetKeyName(47, "");
            this.baseImageList.Images.SetKeyName(48, "");
            this.baseImageList.Images.SetKeyName(49, "");
            this.baseImageList.Images.SetKeyName(50, "");
            this.baseImageList.Images.SetKeyName(51, "");
            this.baseImageList.Images.SetKeyName(52, "");
            this.baseImageList.Images.SetKeyName(53, "");
            this.baseImageList.Images.SetKeyName(54, "");
            this.baseImageList.Images.SetKeyName(55, "");
            this.baseImageList.Images.SetKeyName(56, "");
            this.baseImageList.Images.SetKeyName(57, "");
            this.baseImageList.Images.SetKeyName(58, "");
            this.baseImageList.Images.SetKeyName(59, "");
            this.baseImageList.Images.SetKeyName(60, "");
            this.baseImageList.Images.SetKeyName(61, "");
            this.baseImageList.Images.SetKeyName(62, "");
            this.baseImageList.Images.SetKeyName(63, "");
            this.baseImageList.Images.SetKeyName(64, "");
            this.baseImageList.Images.SetKeyName(65, "");
            this.baseImageList.Images.SetKeyName(66, "");
            this.baseImageList.Images.SetKeyName(67, "");
            // 
            // pbxBackground
            // 
            this.pbxBackground.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            resources.ApplyResources(this.pbxBackground, "pbxBackground");
            this.pbxBackground.Name = "pbxBackground";
            this.pbxBackground.TabStop = false;
            this.pbxBackground.Click += new System.EventHandler(this.pbxBackground_Click);
            // 
            // btnWebsite
            // 
            this.btnWebsite.BackColor = System.Drawing.SystemColors.Control;
            this.btnWebsite.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnWebsite.FlatAppearance.CheckedBackColor = System.Drawing.Color.DimGray;
            this.btnWebsite.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gainsboro;
            this.btnWebsite.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkSlateGray;
            resources.ApplyResources(this.btnWebsite, "btnWebsite");
            this.btnWebsite.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnWebsite.Name = "btnWebsite";
            this.btnWebsite.UseVisualStyleBackColor = false;
            this.btnWebsite.Click += new System.EventHandler(this.Website_Activate);
            // 
            // btnCreateReports
            // 
            this.btnCreateReports.BackColor = System.Drawing.SystemColors.Control;
            this.btnCreateReports.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnCreateReports.FlatAppearance.CheckedBackColor = System.Drawing.Color.DimGray;
            this.btnCreateReports.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gainsboro;
            this.btnCreateReports.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkSlateGray;
            resources.ApplyResources(this.btnCreateReports, "btnCreateReports");
            this.btnCreateReports.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnCreateReports.Name = "btnCreateReports";
            this.btnCreateReports.UseVisualStyleBackColor = false;
            this.btnCreateReports.Click += new System.EventHandler(this.CreateReports_Activate);
            // 
            // btnCreateMaps
            // 
            this.btnCreateMaps.BackColor = System.Drawing.SystemColors.Control;
            this.btnCreateMaps.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnCreateMaps.FlatAppearance.CheckedBackColor = System.Drawing.Color.DimGray;
            this.btnCreateMaps.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gainsboro;
            this.btnCreateMaps.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkSlateGray;
            resources.ApplyResources(this.btnCreateMaps, "btnCreateMaps");
            this.btnCreateMaps.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnCreateMaps.Name = "btnCreateMaps";
            this.btnCreateMaps.UseVisualStyleBackColor = false;
            this.btnCreateMaps.Click += new System.EventHandler(this.CreateMaps_Activate);
            // 
            // btnExit
            // 
            this.btnExit.BackColor = System.Drawing.SystemColors.Control;
            this.btnExit.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnExit.FlatAppearance.CheckedBackColor = System.Drawing.Color.DimGray;
            this.btnExit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gainsboro;
            this.btnExit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkSlateGray;
            resources.ApplyResources(this.btnExit, "btnExit");
            this.btnExit.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnExit.Name = "btnExit";
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new System.EventHandler(this.Exit_Activate);
            // 
            // btnEnterData
            // 
            this.btnEnterData.BackColor = System.Drawing.SystemColors.Control;
            this.btnEnterData.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnEnterData.FlatAppearance.CheckedBackColor = System.Drawing.Color.DimGray;
            this.btnEnterData.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gainsboro;
            this.btnEnterData.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkSlateGray;
            resources.ApplyResources(this.btnEnterData, "btnEnterData");
            this.btnEnterData.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnEnterData.Name = "btnEnterData";
            this.btnEnterData.UseVisualStyleBackColor = false;
            this.btnEnterData.Click += new System.EventHandler(this.EnterData_Activate);
            // 
            // btnAnalyze
            // 
            this.btnAnalyze.BackColor = System.Drawing.SystemColors.Control;
            this.btnAnalyze.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnAnalyze.FlatAppearance.CheckedBackColor = System.Drawing.Color.DimGray;
            this.btnAnalyze.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gainsboro;
            this.btnAnalyze.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkSlateGray;
            resources.ApplyResources(this.btnAnalyze, "btnAnalyze");
            this.btnAnalyze.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnAnalyze.Name = "btnAnalyze";
            this.btnAnalyze.UseVisualStyleBackColor = false;
            this.btnAnalyze.Click += new System.EventHandler(this.AnalyzeData_Activate);
            // 
            // btnMakeView
            // 
            this.btnMakeView.BackColor = System.Drawing.SystemColors.Control;
            this.btnMakeView.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnMakeView.FlatAppearance.CheckedBackColor = System.Drawing.Color.DimGray;
            this.btnMakeView.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gainsboro;
            this.btnMakeView.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkSlateGray;
            resources.ApplyResources(this.btnMakeView, "btnMakeView");
            this.btnMakeView.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnMakeView.Name = "btnMakeView";
            this.btnMakeView.UseVisualStyleBackColor = false;
            this.btnMakeView.Click += new System.EventHandler(this.MakeView_Activate);
            // 
            // imlImageList
            // 
            this.imlImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            resources.ApplyResources(this.imlImageList, "imlImageList");
            this.imlImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // mnuMainMenu
            // 
            this.mnuMainMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.mnuMainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.statCalcToolStripMenuItem,
            this.helpToolStripMenuItem});
            resources.ApplyResources(this.mnuMainMenu, "mnuMainMenu");
            this.mnuMainMenu.Name = "mnuMainMenu";
            this.mnuMainMenu.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.mnuMainMenu_ItemClicked);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.Exit_Activate);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemStatusBar,
            this.epiInfoLogsToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            resources.ApplyResources(this.viewToolStripMenuItem, "viewToolStripMenuItem");
            // 
            // toolStripMenuItemStatusBar
            // 
            this.toolStripMenuItemStatusBar.Checked = true;
            this.toolStripMenuItemStatusBar.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItemStatusBar.Name = "toolStripMenuItemStatusBar";
            resources.ApplyResources(this.toolStripMenuItemStatusBar, "toolStripMenuItemStatusBar");
            this.toolStripMenuItemStatusBar.Click += new System.EventHandler(this.toolStripMenuItemStatusBar_Click);
            // 
            // epiInfoLogsToolStripMenuItem
            // 
            this.epiInfoLogsToolStripMenuItem.Name = "epiInfoLogsToolStripMenuItem";
            resources.ApplyResources(this.epiInfoLogsToolStripMenuItem, "epiInfoLogsToolStripMenuItem");
            this.epiInfoLogsToolStripMenuItem.Click += new System.EventHandler(this.epiInfoLogsToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.makeViewToolStripMenuItem,
            this.enterDataToolStripMenuItem,
            this.analyzeDataToolStripMenuItem,
            this.createMapsToolStripMenuItem,
            this.toolStripSeparator1,
            this.optionsToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            resources.ApplyResources(this.toolsToolStripMenuItem, "toolsToolStripMenuItem");
            // 
            // makeViewToolStripMenuItem
            // 
            this.makeViewToolStripMenuItem.Name = "makeViewToolStripMenuItem";
            resources.ApplyResources(this.makeViewToolStripMenuItem, "makeViewToolStripMenuItem");
            this.makeViewToolStripMenuItem.Click += new System.EventHandler(this.MakeView_Activate);
            // 
            // enterDataToolStripMenuItem
            // 
            this.enterDataToolStripMenuItem.Name = "enterDataToolStripMenuItem";
            resources.ApplyResources(this.enterDataToolStripMenuItem, "enterDataToolStripMenuItem");
            this.enterDataToolStripMenuItem.Click += new System.EventHandler(this.EnterData_Activate);
            // 
            // analyzeDataToolStripMenuItem
            // 
            this.analyzeDataToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.classicToolStripMenuItem,
            this.dashboardToolStripMenuItem});
            this.analyzeDataToolStripMenuItem.Name = "analyzeDataToolStripMenuItem";
            resources.ApplyResources(this.analyzeDataToolStripMenuItem, "analyzeDataToolStripMenuItem");
            // 
            // classicToolStripMenuItem
            // 
            this.classicToolStripMenuItem.Name = "classicToolStripMenuItem";
            resources.ApplyResources(this.classicToolStripMenuItem, "classicToolStripMenuItem");
            this.classicToolStripMenuItem.Click += new System.EventHandler(this.AnalyzeData_Activate);
            // 
            // dashboardToolStripMenuItem
            // 
            this.dashboardToolStripMenuItem.Name = "dashboardToolStripMenuItem";
            resources.ApplyResources(this.dashboardToolStripMenuItem, "dashboardToolStripMenuItem");
            this.dashboardToolStripMenuItem.Click += new System.EventHandler(this.btnDashboard_Click);
            // 
            // createMapsToolStripMenuItem
            // 
            this.createMapsToolStripMenuItem.Name = "createMapsToolStripMenuItem";
            resources.ApplyResources(this.createMapsToolStripMenuItem, "createMapsToolStripMenuItem");
            this.createMapsToolStripMenuItem.Click += new System.EventHandler(this.CreateMaps_Activate);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            resources.ApplyResources(this.optionsToolStripMenuItem, "optionsToolStripMenuItem");
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.Options_Activate);
            // 
            // statCalcToolStripMenuItem
            // 
            this.statCalcToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sampleSizeAndPowerToolStripMenuItem1,
            this.chiSquareForTrendToolStripMenuItem1,
            this.tables2X22XNToolStripMenuItem,
            this.mnuPoisson,
            this.mnuBinomial,
            this.mnuMatchedPairCaseControl});
            this.statCalcToolStripMenuItem.Name = "statCalcToolStripMenuItem";
            resources.ApplyResources(this.statCalcToolStripMenuItem, "statCalcToolStripMenuItem");
            // 
            // sampleSizeAndPowerToolStripMenuItem1
            // 
            this.sampleSizeAndPowerToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.populationSurveyToolStripMenuItem1,
            this.cohortOrCrossSectionalToolStripMenuItem,
            this.unmatchedCasecontrolToolStripMenuItem1});
            this.sampleSizeAndPowerToolStripMenuItem1.Name = "sampleSizeAndPowerToolStripMenuItem1";
            resources.ApplyResources(this.sampleSizeAndPowerToolStripMenuItem1, "sampleSizeAndPowerToolStripMenuItem1");
            this.sampleSizeAndPowerToolStripMenuItem1.Click += new System.EventHandler(this.sampleSizeAndPowerToolStripMenuItem1_Click);
            // 
            // populationSurveyToolStripMenuItem1
            // 
            this.populationSurveyToolStripMenuItem1.Name = "populationSurveyToolStripMenuItem1";
            resources.ApplyResources(this.populationSurveyToolStripMenuItem1, "populationSurveyToolStripMenuItem1");
            this.populationSurveyToolStripMenuItem1.Click += new System.EventHandler(this.populationSurveyToolStripMenuItem_Click);
            // 
            // cohortOrCrossSectionalToolStripMenuItem
            // 
            this.cohortOrCrossSectionalToolStripMenuItem.Name = "cohortOrCrossSectionalToolStripMenuItem";
            resources.ApplyResources(this.cohortOrCrossSectionalToolStripMenuItem, "cohortOrCrossSectionalToolStripMenuItem");
            this.cohortOrCrossSectionalToolStripMenuItem.Click += new System.EventHandler(this.cohortOrCrossToolStripMenuItem_Click);
            // 
            // unmatchedCasecontrolToolStripMenuItem1
            // 
            this.unmatchedCasecontrolToolStripMenuItem1.Name = "unmatchedCasecontrolToolStripMenuItem1";
            resources.ApplyResources(this.unmatchedCasecontrolToolStripMenuItem1, "unmatchedCasecontrolToolStripMenuItem1");
            this.unmatchedCasecontrolToolStripMenuItem1.Click += new System.EventHandler(this.unmatchedCasecontrolToolStripMenuItem_Click);
            // 
            // chiSquareForTrendToolStripMenuItem1
            // 
            this.chiSquareForTrendToolStripMenuItem1.Name = "chiSquareForTrendToolStripMenuItem1";
            resources.ApplyResources(this.chiSquareForTrendToolStripMenuItem1, "chiSquareForTrendToolStripMenuItem1");
            this.chiSquareForTrendToolStripMenuItem1.Click += new System.EventHandler(this.chiSquareForTrendToolStripMenuItem_Click);
            // 
            // tables2X22XNToolStripMenuItem
            // 
            this.tables2X22XNToolStripMenuItem.Name = "tables2X22XNToolStripMenuItem";
            resources.ApplyResources(this.tables2X22XNToolStripMenuItem, "tables2X22XNToolStripMenuItem");
            this.tables2X22XNToolStripMenuItem.Click += new System.EventHandler(this.tables2x2ToolStripMenuItem_Click);
            // 
            // mnuPoisson
            // 
            this.mnuPoisson.Name = "mnuPoisson";
            resources.ApplyResources(this.mnuPoisson, "mnuPoisson");
            this.mnuPoisson.Click += new System.EventHandler(this.mnuPoisson_Click);
            // 
            // mnuBinomial
            // 
            this.mnuBinomial.Name = "mnuBinomial";
            resources.ApplyResources(this.mnuBinomial, "mnuBinomial");
            this.mnuBinomial.Click += new System.EventHandler(this.mnuBinomial_Click);
            // 
            // mnuMatchedPairCaseControl
            // 
            this.mnuMatchedPairCaseControl.Name = "mnuMatchedPairCaseControl";
            resources.ApplyResources(this.mnuMatchedPairCaseControl, "mnuMatchedPairCaseControl");
            this.mnuMatchedPairCaseControl.Click += new System.EventHandler(this.mnuMatchedPairCaseControl_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contentsToolStripMenuItem,
            this.videosToolStripMenuItem,
            this.communityMessageBoardToolStripMenuItem,
            this.contactHelpdeskToolStripMenuItem,
            this.otherEpiResourcesToolStripMenuItem,
            this.toolStripSeparator2,
            this.aboutEpiInfoToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            resources.ApplyResources(this.helpToolStripMenuItem, "helpToolStripMenuItem");
            // 
            // contentsToolStripMenuItem
            // 
            this.contentsToolStripMenuItem.Name = "contentsToolStripMenuItem";
            resources.ApplyResources(this.contentsToolStripMenuItem, "contentsToolStripMenuItem");
            this.contentsToolStripMenuItem.Click += new System.EventHandler(this.Contents_Activate);
            // 
            // videosToolStripMenuItem
            // 
            this.videosToolStripMenuItem.Name = "videosToolStripMenuItem";
            resources.ApplyResources(this.videosToolStripMenuItem, "videosToolStripMenuItem");
            this.videosToolStripMenuItem.Click += new System.EventHandler(this.videosToolStripMenuItem_Click);
            // 
            // communityMessageBoardToolStripMenuItem
            // 
            this.communityMessageBoardToolStripMenuItem.Name = "communityMessageBoardToolStripMenuItem";
            resources.ApplyResources(this.communityMessageBoardToolStripMenuItem, "communityMessageBoardToolStripMenuItem");
            this.communityMessageBoardToolStripMenuItem.Click += new System.EventHandler(this.communityMessageBoardToolStripMenuItem_Click);
            // 
            // contactHelpdeskToolStripMenuItem
            // 
            this.contactHelpdeskToolStripMenuItem.Name = "contactHelpdeskToolStripMenuItem";
            resources.ApplyResources(this.contactHelpdeskToolStripMenuItem, "contactHelpdeskToolStripMenuItem");
            this.contactHelpdeskToolStripMenuItem.Click += new System.EventHandler(this.contactHelpdeskToolStripMenuItem_Click);
            // 
            // otherEpiResourcesToolStripMenuItem
            // 
            this.otherEpiResourcesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.activEpicomToolStripMenuItem,
            this.openEpicomToolStripMenuItem});
            this.otherEpiResourcesToolStripMenuItem.Name = "otherEpiResourcesToolStripMenuItem";
            resources.ApplyResources(this.otherEpiResourcesToolStripMenuItem, "otherEpiResourcesToolStripMenuItem");
            // 
            // activEpicomToolStripMenuItem
            // 
            this.activEpicomToolStripMenuItem.Name = "activEpicomToolStripMenuItem";
            resources.ApplyResources(this.activEpicomToolStripMenuItem, "activEpicomToolStripMenuItem");
            this.activEpicomToolStripMenuItem.Click += new System.EventHandler(this.activEpicomToolStripMenuItem_Click);
            // 
            // openEpicomToolStripMenuItem
            // 
            this.openEpicomToolStripMenuItem.Name = "openEpicomToolStripMenuItem";
            resources.ApplyResources(this.openEpicomToolStripMenuItem, "openEpicomToolStripMenuItem");
            this.openEpicomToolStripMenuItem.Click += new System.EventHandler(this.openEpicomToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // aboutEpiInfoToolStripMenuItem
            // 
            this.aboutEpiInfoToolStripMenuItem.Name = "aboutEpiInfoToolStripMenuItem";
            resources.ApplyResources(this.aboutEpiInfoToolStripMenuItem, "aboutEpiInfoToolStripMenuItem");
            this.aboutEpiInfoToolStripMenuItem.Click += new System.EventHandler(this.AboutEpiInfo_Activate);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.Controls.Add(this.btnDashboard);
            this.groupBox1.Controls.Add(this.btnAnalyze);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.ForeColor = System.Drawing.Color.White;
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // btnDashboard
            // 
            this.btnDashboard.BackColor = System.Drawing.SystemColors.Control;
            this.btnDashboard.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnDashboard.FlatAppearance.CheckedBackColor = System.Drawing.Color.DimGray;
            this.btnDashboard.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gainsboro;
            this.btnDashboard.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkSlateGray;
            resources.ApplyResources(this.btnDashboard, "btnDashboard");
            this.btnDashboard.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnDashboard.Name = "btnDashboard";
            this.btnDashboard.UseVisualStyleBackColor = false;
            this.btnDashboard.Click += new System.EventHandler(this.btnDashboard_Click);
            // 
            // MenuMainForm
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnWebsite);
            this.Controls.Add(this.btnCreateReports);
            this.Controls.Add(this.btnCreateMaps);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnEnterData);
            this.Controls.Add(this.btnMakeView);
            this.Controls.Add(this.pbxBackground);
            this.Controls.Add(this.mnuMainMenu);
            this.MainMenuStrip = this.mnuMainMenu;
            this.Name = "MenuMainForm";
            this.Load += new System.EventHandler(this.MenuMainForm_Load);
            this.Controls.SetChildIndex(this.mnuMainMenu, 0);
            this.Controls.SetChildIndex(this.pbxBackground, 0);
            this.Controls.SetChildIndex(this.btnMakeView, 0);
            this.Controls.SetChildIndex(this.btnEnterData, 0);
            this.Controls.SetChildIndex(this.btnExit, 0);
            this.Controls.SetChildIndex(this.btnCreateMaps, 0);
            this.Controls.SetChildIndex(this.btnCreateReports, 0);
            this.Controls.SetChildIndex(this.btnWebsite, 0);
            this.Controls.SetChildIndex(this.groupBox1, 0);
            ((System.ComponentModel.ISupportInitialize)(this.pbxBackground)).EndInit();
            this.mnuMainMenu.ResumeLayout(false);
            this.mnuMainMenu.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion Designer generated code

        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem makeViewToolStripMenuItem;
        private ToolStripMenuItem enterDataToolStripMenuItem;
        private ToolStripMenuItem analyzeDataToolStripMenuItem;
        private ToolStripMenuItem createMapsToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem optionsToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem contentsToolStripMenuItem;
        private ToolStripMenuItem aboutEpiInfoToolStripMenuItem;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItemStatusBar;
        private ToolStripMenuItem epiInfoLogsToolStripMenuItem;
        private GroupBox groupBox1;
        private Button btnDashboard;
        private ToolStripMenuItem classicToolStripMenuItem;
        private ToolStripMenuItem dashboardToolStripMenuItem;
        private ToolStripMenuItem statCalcToolStripMenuItem;
        private ToolStripMenuItem sampleSizeAndPowerToolStripMenuItem1;
        private ToolStripMenuItem populationSurveyToolStripMenuItem1;
        private ToolStripMenuItem cohortOrCrossSectionalToolStripMenuItem;
        private ToolStripMenuItem unmatchedCasecontrolToolStripMenuItem1;
        private ToolStripMenuItem chiSquareForTrendToolStripMenuItem1;
        private ToolStripMenuItem tables2X22XNToolStripMenuItem;
        private ToolStripMenuItem videosToolStripMenuItem;
        private ToolStripMenuItem communityMessageBoardToolStripMenuItem;
        private ToolStripMenuItem contactHelpdeskToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem mnuPoisson;
        private ToolStripMenuItem mnuBinomial;
        private ToolStripMenuItem mnuMatchedPairCaseControl;
        private ToolStripMenuItem otherEpiResourcesToolStripMenuItem;
        private ToolStripMenuItem openEpicomToolStripMenuItem;
        private ToolStripMenuItem activEpicomToolStripMenuItem;
    }
}
