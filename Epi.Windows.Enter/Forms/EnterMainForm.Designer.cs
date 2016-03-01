namespace Epi.Windows.Enter
{
    /// <summary>
    /// The Enter module main form
    /// </summary>
    partial class EnterMainForm
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
            this.viewExplorer = null;
            this.canvas = null;
            this.currentBackgroundImage = null;
            this.currentBackgroundImageLayout = null;

            this.bgTable = null;
            this.runTimeView = null;
            this.view = null;
            this.currentProject = null;
            if (this.Module != null)
            {
                this.Module.Dispose();
            }
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EnterMainForm));
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.enterDockManager = new Epi.Windows.Docking.DockManager(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newRecordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuImportFromPhone = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuImportFromWeb = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuImportFromForm = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuImportFromDataPackage = new System.Windows.Forms.ToolStripMenuItem();
            this.fromWebEnterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPackageForTransport = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.recentViewsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.markAsDeletedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undeleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusBarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.epiInfoLogsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataDictionaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.compactDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableCheckCodeExecutionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableCheckCodeErrorSupressionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyShortcutToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutEpiInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btiOpen = new System.Windows.Forms.ToolStripButton();
            this.btiSave = new System.Windows.Forms.ToolStripButton();
            this.btiPrint = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.btiFind = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.btiNew = new System.Windows.Forms.ToolStripButton();
            this.toolStripFirstButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripPreviousButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripRecordNumber = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripOfLabel = new System.Windows.Forms.ToolStripLabel();
            this.toolStripRecordCount = new System.Windows.Forms.ToolStripLabel();
            this.toolStripNextButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripLastButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.btiMarkDeleted = new System.Windows.Forms.ToolStripButton();
            this.btiUndelete = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.LineListingToolStripButton = new System.Windows.Forms.ToolStripSplitButton();
            this.gridToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printableHTMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.excelLineListMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsbDashboard = new System.Windows.Forms.ToolStripButton();
            this.tsbMap = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.btiEditView = new System.Windows.Forms.ToolStripButton();
            this.btiHelp = new System.Windows.Forms.ToolStripButton();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.btnHome = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.btnBack = new System.Windows.Forms.ToolStripButton();
            this.BottomToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.TopToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.RightToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.LeftToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.ContentPanel = new System.Windows.Forms.ToolStripContentPanel();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.toolStrip2.SuspendLayout();
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
            this.baseImageList.Images.SetKeyName(68, "");
            this.baseImageList.Images.SetKeyName(69, "");
            this.baseImageList.Images.SetKeyName(70, "");
            this.baseImageList.Images.SetKeyName(71, "");
            this.baseImageList.Images.SetKeyName(72, "");
            this.baseImageList.Images.SetKeyName(73, "");
            this.baseImageList.Images.SetKeyName(74, "");
            this.baseImageList.Images.SetKeyName(75, "");
            this.baseImageList.Images.SetKeyName(76, "");
            this.baseImageList.Images.SetKeyName(77, "");
            this.baseImageList.Images.SetKeyName(78, "");
            this.baseImageList.Images.SetKeyName(79, "");
            this.baseImageList.Images.SetKeyName(80, "");
            this.baseImageList.Images.SetKeyName(81, "");
            this.baseImageList.Images.SetKeyName(82, "");
            this.baseImageList.Images.SetKeyName(83, "");
            this.baseImageList.Images.SetKeyName(84, "");
            this.baseImageList.Images.SetKeyName(85, "");
            this.baseImageList.Images.SetKeyName(86, "");
            this.baseImageList.Images.SetKeyName(87, "");
            this.baseImageList.Images.SetKeyName(88, "");
            this.baseImageList.Images.SetKeyName(89, "");
            this.baseImageList.Images.SetKeyName(90, "");
            this.baseImageList.Images.SetKeyName(91, "");
            this.baseImageList.Images.SetKeyName(92, "");
            this.baseImageList.Images.SetKeyName(93, "");
            this.baseImageList.Images.SetKeyName(94, "");
            this.baseImageList.Images.SetKeyName(95, "");
            this.baseImageList.Images.SetKeyName(96, "");
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.enterDockManager);
            resources.ApplyResources(this.toolStripContainer1.ContentPanel, "toolStripContainer1.ContentPanel");
            resources.ApplyResources(this.toolStripContainer1, "toolStripContainer1");
            this.toolStripContainer1.Name = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip2);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.menuStrip1);
            // 
            // enterDockManager
            // 
            this.enterDockManager.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.enterDockManager, "enterDockManager");
            this.enterDockManager.DockBorder = 20;
            this.enterDockManager.DockType = Epi.Windows.Docking.DockContainerType.Document;
            this.enterDockManager.FastDrawing = true;
            this.enterDockManager.Name = "enterDockManager";
            this.enterDockManager.ShowIcons = true;
            this.enterDockManager.SplitterWidth = 4;
            this.enterDockManager.VisualStyle = Epi.Windows.Docking.DockVisualStyle.VS2003;
            // 
            // menuStrip1
            // 
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuStrip1_ItemClicked);
            this.menuStrip1.Click += new System.EventHandler(this.menuStrip1_Click);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newRecordToolStripMenuItem,
            this.openViewToolStripMenuItem,
            this.editViewToolStripMenuItem,
            this.closeViewToolStripMenuItem,
            this.toolStripSeparator1,
            this.saveToolStripMenuItem,
            this.importDataToolStripMenuItem,
            this.mnuPackageForTransport,
            this.toolStripSeparator2,
            this.printToolStripMenuItem,
            this.toolStripSeparator3,
            this.recentViewsToolStripMenuItem,
            this.toolStripSeparator4,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            // 
            // newRecordToolStripMenuItem
            // 
            resources.ApplyResources(this.newRecordToolStripMenuItem, "newRecordToolStripMenuItem");
            this.newRecordToolStripMenuItem.Name = "newRecordToolStripMenuItem";
            this.newRecordToolStripMenuItem.Click += new System.EventHandler(this.newRecordToolStripMenuItem_Click);
            // 
            // openViewToolStripMenuItem
            // 
            this.openViewToolStripMenuItem.Name = "openViewToolStripMenuItem";
            resources.ApplyResources(this.openViewToolStripMenuItem, "openViewToolStripMenuItem");
            this.openViewToolStripMenuItem.Click += new System.EventHandler(this.openViewToolStripMenuItem_Click);
            // 
            // editViewToolStripMenuItem
            // 
            resources.ApplyResources(this.editViewToolStripMenuItem, "editViewToolStripMenuItem");
            this.editViewToolStripMenuItem.Name = "editViewToolStripMenuItem";
            this.editViewToolStripMenuItem.Click += new System.EventHandler(this.editViewToolStripMenuItem_Click);
            // 
            // closeViewToolStripMenuItem
            // 
            resources.ApplyResources(this.closeViewToolStripMenuItem, "closeViewToolStripMenuItem");
            this.closeViewToolStripMenuItem.Name = "closeViewToolStripMenuItem";
            this.closeViewToolStripMenuItem.Click += new System.EventHandler(this.closeViewToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // saveToolStripMenuItem
            // 
            resources.ApplyResources(this.saveToolStripMenuItem, "saveToolStripMenuItem");
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // importDataToolStripMenuItem
            // 
            this.importDataToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuImportFromPhone,
            this.mnuImportFromWeb,
            this.mnuImportFromForm,
            this.mnuImportFromDataPackage,
            this.fromWebEnterToolStripMenuItem});
            this.importDataToolStripMenuItem.Name = "importDataToolStripMenuItem";
            resources.ApplyResources(this.importDataToolStripMenuItem, "importDataToolStripMenuItem");
            // 
            // mnuImportFromPhone
            // 
            resources.ApplyResources(this.mnuImportFromPhone, "mnuImportFromPhone");
            this.mnuImportFromPhone.Image = global::Epi.Enter.Properties.Resources.android_icon;
            this.mnuImportFromPhone.Name = "mnuImportFromPhone";
            this.mnuImportFromPhone.Click += new System.EventHandler(this.mnuImportFromPhone_Click);
            // 
            // mnuImportFromWeb
            // 
            resources.ApplyResources(this.mnuImportFromWeb, "mnuImportFromWeb");
            this.mnuImportFromWeb.Name = "mnuImportFromWeb";
            this.mnuImportFromWeb.Click += new System.EventHandler(this.mnuImportFromWebToolStripMenuItem_Click);
            // 
            // mnuImportFromForm
            // 
            resources.ApplyResources(this.mnuImportFromForm, "mnuImportFromForm");
            this.mnuImportFromForm.Name = "mnuImportFromForm";
            this.mnuImportFromForm.Click += new System.EventHandler(this.mnuImportFromProjectToolStripMenuItem_Click);
            // 
            // mnuImportFromDataPackage
            // 
            resources.ApplyResources(this.mnuImportFromDataPackage, "mnuImportFromDataPackage");
            this.mnuImportFromDataPackage.Name = "mnuImportFromDataPackage";
            this.mnuImportFromDataPackage.Click += new System.EventHandler(this.mnuImportFromDataPackage_Click);
            // 
            // fromWebEnterToolStripMenuItem
            // 
            resources.ApplyResources(this.fromWebEnterToolStripMenuItem, "fromWebEnterToolStripMenuItem");
            this.fromWebEnterToolStripMenuItem.Name = "fromWebEnterToolStripMenuItem";
            this.fromWebEnterToolStripMenuItem.Click += new System.EventHandler(this.fromWebEnterToolStripMenuItem_Click);
            // 
            // mnuPackageForTransport
            // 
            resources.ApplyResources(this.mnuPackageForTransport, "mnuPackageForTransport");
            this.mnuPackageForTransport.Name = "mnuPackageForTransport";
            this.mnuPackageForTransport.Click += new System.EventHandler(this.mnuPackageForTransportToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // printToolStripMenuItem
            // 
            resources.ApplyResources(this.printToolStripMenuItem, "printToolStripMenuItem");
            this.printToolStripMenuItem.Name = "printToolStripMenuItem";
            this.printToolStripMenuItem.Click += new System.EventHandler(this.printToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            // 
            // recentViewsToolStripMenuItem
            // 
            this.recentViewsToolStripMenuItem.Name = "recentViewsToolStripMenuItem";
            resources.ApplyResources(this.recentViewsToolStripMenuItem, "recentViewsToolStripMenuItem");
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.findToolStripMenuItem,
            this.markAsDeletedToolStripMenuItem,
            this.undeleteToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            resources.ApplyResources(this.editToolStripMenuItem, "editToolStripMenuItem");
            // 
            // findToolStripMenuItem
            // 
            resources.ApplyResources(this.findToolStripMenuItem, "findToolStripMenuItem");
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            this.findToolStripMenuItem.Click += new System.EventHandler(this.findToolStripMenuItem_Click);
            // 
            // markAsDeletedToolStripMenuItem
            // 
            resources.ApplyResources(this.markAsDeletedToolStripMenuItem, "markAsDeletedToolStripMenuItem");
            this.markAsDeletedToolStripMenuItem.Name = "markAsDeletedToolStripMenuItem";
            this.markAsDeletedToolStripMenuItem.Click += new System.EventHandler(this.markAsDeletedToolStripMenuItem_Click);
            // 
            // undeleteToolStripMenuItem
            // 
            resources.ApplyResources(this.undeleteToolStripMenuItem, "undeleteToolStripMenuItem");
            this.undeleteToolStripMenuItem.Name = "undeleteToolStripMenuItem";
            this.undeleteToolStripMenuItem.Click += new System.EventHandler(this.undeleteToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusBarToolStripMenuItem,
            this.epiInfoLogsToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            resources.ApplyResources(this.viewToolStripMenuItem, "viewToolStripMenuItem");
            // 
            // statusBarToolStripMenuItem
            // 
            this.statusBarToolStripMenuItem.Name = "statusBarToolStripMenuItem";
            resources.ApplyResources(this.statusBarToolStripMenuItem, "statusBarToolStripMenuItem");
            this.statusBarToolStripMenuItem.Click += new System.EventHandler(this.statusBarToolStripMenuItem_Click);
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
            this.dataDictionaryToolStripMenuItem,
            this.toolStripMenuItem1,
            this.compactDatabaseToolStripMenuItem,
            this.toolStripMenuItem2,
            this.optionsToolStripMenuItem,
            this.enableCheckCodeExecutionToolStripMenuItem,
            this.enableCheckCodeErrorSupressionToolStripMenuItem,
            this.copyShortcutToClipboardToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            resources.ApplyResources(this.toolsToolStripMenuItem, "toolsToolStripMenuItem");
            // 
            // dataDictionaryToolStripMenuItem
            // 
            this.dataDictionaryToolStripMenuItem.Name = "dataDictionaryToolStripMenuItem";
            resources.ApplyResources(this.dataDictionaryToolStripMenuItem, "dataDictionaryToolStripMenuItem");
            this.dataDictionaryToolStripMenuItem.Click += new System.EventHandler(this.dataDictionaryToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
            // 
            // compactDatabaseToolStripMenuItem
            // 
            resources.ApplyResources(this.compactDatabaseToolStripMenuItem, "compactDatabaseToolStripMenuItem");
            this.compactDatabaseToolStripMenuItem.Name = "compactDatabaseToolStripMenuItem";
            this.compactDatabaseToolStripMenuItem.Click += new System.EventHandler(this.compactDatabaseToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            resources.ApplyResources(this.optionsToolStripMenuItem, "optionsToolStripMenuItem");
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
            // 
            // enableCheckCodeExecutionToolStripMenuItem
            // 
            this.enableCheckCodeExecutionToolStripMenuItem.Checked = true;
            this.enableCheckCodeExecutionToolStripMenuItem.CheckOnClick = true;
            this.enableCheckCodeExecutionToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enableCheckCodeExecutionToolStripMenuItem.Name = "enableCheckCodeExecutionToolStripMenuItem";
            resources.ApplyResources(this.enableCheckCodeExecutionToolStripMenuItem, "enableCheckCodeExecutionToolStripMenuItem");
            this.enableCheckCodeExecutionToolStripMenuItem.Click += new System.EventHandler(this.enableCheckCodeExecutionToolStripMenuItem_Click);
            // 
            // enableCheckCodeErrorSupressionToolStripMenuItem
            // 
            this.enableCheckCodeErrorSupressionToolStripMenuItem.Checked = true;
            this.enableCheckCodeErrorSupressionToolStripMenuItem.CheckOnClick = true;
            this.enableCheckCodeErrorSupressionToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enableCheckCodeErrorSupressionToolStripMenuItem.Name = "enableCheckCodeErrorSupressionToolStripMenuItem";
            resources.ApplyResources(this.enableCheckCodeErrorSupressionToolStripMenuItem, "enableCheckCodeErrorSupressionToolStripMenuItem");
            this.enableCheckCodeErrorSupressionToolStripMenuItem.Click += new System.EventHandler(this.enableCheckCodeErrorSupressionToolStripMenuItem_Click);
            // 
            // copyShortcutToClipboardToolStripMenuItem
            // 
            this.copyShortcutToClipboardToolStripMenuItem.Name = "copyShortcutToClipboardToolStripMenuItem";
            resources.ApplyResources(this.copyShortcutToClipboardToolStripMenuItem, "copyShortcutToClipboardToolStripMenuItem");
            this.copyShortcutToClipboardToolStripMenuItem.Click += new System.EventHandler(this.copyShortcutToClipboardToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contentsToolStripMenuItem,
            this.aboutEpiInfoToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            resources.ApplyResources(this.helpToolStripMenuItem, "helpToolStripMenuItem");
            // 
            // contentsToolStripMenuItem
            // 
            this.contentsToolStripMenuItem.Name = "contentsToolStripMenuItem";
            resources.ApplyResources(this.contentsToolStripMenuItem, "contentsToolStripMenuItem");
            this.contentsToolStripMenuItem.Click += new System.EventHandler(this.contentsToolStripMenuItem_Click);
            // 
            // aboutEpiInfoToolStripMenuItem
            // 
            this.aboutEpiInfoToolStripMenuItem.Name = "aboutEpiInfoToolStripMenuItem";
            resources.ApplyResources(this.aboutEpiInfoToolStripMenuItem, "aboutEpiInfoToolStripMenuItem");
            this.aboutEpiInfoToolStripMenuItem.Click += new System.EventHandler(this.aboutEpiInfoToolStripMenuItem_Click);
            // 
            // toolStrip1
            // 
            resources.ApplyResources(this.toolStrip1, "toolStrip1");
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btiOpen,
            this.btiSave,
            this.btiPrint,
            this.toolStripSeparator5,
            this.btiFind,
            this.toolStripSeparator6,
            this.btiNew,
            this.toolStripFirstButton,
            this.toolStripPreviousButton,
            this.toolStripRecordNumber,
            this.toolStripOfLabel,
            this.toolStripRecordCount,
            this.toolStripNextButton,
            this.toolStripLastButton,
            this.toolStripSeparator8,
            this.btiMarkDeleted,
            this.btiUndelete,
            this.toolStripSeparator7,
            this.LineListingToolStripButton,
            this.tsbDashboard,
            this.tsbMap,
            this.toolStripSeparator10,
            this.btiEditView,
            this.btiHelp});
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.toolStrip1_ItemClicked);
            // 
            // btiOpen
            // 
            resources.ApplyResources(this.btiOpen, "btiOpen");
            this.btiOpen.Name = "btiOpen";
            this.btiOpen.Click += new System.EventHandler(this.openViewToolStripMenuItem_Click);
            // 
            // btiSave
            // 
            resources.ApplyResources(this.btiSave, "btiSave");
            this.btiSave.Name = "btiSave";
            this.btiSave.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // btiPrint
            // 
            resources.ApplyResources(this.btiPrint, "btiPrint");
            this.btiPrint.Name = "btiPrint";
            this.btiPrint.Click += new System.EventHandler(this.printToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
            // 
            // btiFind
            // 
            resources.ApplyResources(this.btiFind, "btiFind");
            this.btiFind.Name = "btiFind";
            this.btiFind.Click += new System.EventHandler(this.findToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            resources.ApplyResources(this.toolStripSeparator6, "toolStripSeparator6");
            // 
            // btiNew
            // 
            this.btiNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.btiNew, "btiNew");
            this.btiNew.Name = "btiNew";
            this.btiNew.Click += new System.EventHandler(this.newRecordToolStripMenuItem_Click);
            // 
            // toolStripFirstButton
            // 
            this.toolStripFirstButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripFirstButton, "toolStripFirstButton");
            this.toolStripFirstButton.Name = "toolStripFirstButton";
            this.toolStripFirstButton.Click += new System.EventHandler(this.toolStripFirstButton_Click);
            // 
            // toolStripPreviousButton
            // 
            this.toolStripPreviousButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripPreviousButton, "toolStripPreviousButton");
            this.toolStripPreviousButton.Name = "toolStripPreviousButton";
            this.toolStripPreviousButton.Click += new System.EventHandler(this.toolStripPreviousButton_Click);
            // 
            // toolStripRecordNumber
            // 
            this.toolStripRecordNumber.AcceptsReturn = true;
            this.toolStripRecordNumber.AcceptsTab = true;
            resources.ApplyResources(this.toolStripRecordNumber, "toolStripRecordNumber");
            this.toolStripRecordNumber.Name = "toolStripRecordNumber";
            this.toolStripRecordNumber.Leave += new System.EventHandler(this.toolStripPageNumber_Leave);
            this.toolStripRecordNumber.KeyDown += new System.Windows.Forms.KeyEventHandler(this.toolStripRecordNumber_KeyDown);
            // 
            // toolStripOfLabel
            // 
            resources.ApplyResources(this.toolStripOfLabel, "toolStripOfLabel");
            this.toolStripOfLabel.Name = "toolStripOfLabel";
            // 
            // toolStripRecordCount
            // 
            resources.ApplyResources(this.toolStripRecordCount, "toolStripRecordCount");
            this.toolStripRecordCount.Name = "toolStripRecordCount";
            // 
            // toolStripNextButton
            // 
            this.toolStripNextButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripNextButton, "toolStripNextButton");
            this.toolStripNextButton.Name = "toolStripNextButton";
            this.toolStripNextButton.Click += new System.EventHandler(this.toolStripNextButton_Click);
            // 
            // toolStripLastButton
            // 
            this.toolStripLastButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripLastButton, "toolStripLastButton");
            this.toolStripLastButton.Name = "toolStripLastButton";
            this.toolStripLastButton.Click += new System.EventHandler(this.toolStripLastButton_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            resources.ApplyResources(this.toolStripSeparator8, "toolStripSeparator8");
            // 
            // btiMarkDeleted
            // 
            this.btiMarkDeleted.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.btiMarkDeleted, "btiMarkDeleted");
            this.btiMarkDeleted.Name = "btiMarkDeleted";
            this.btiMarkDeleted.Click += new System.EventHandler(this.markAsDeletedToolStripMenuItem_Click);
            // 
            // btiUndelete
            // 
            resources.ApplyResources(this.btiUndelete, "btiUndelete");
            this.btiUndelete.Name = "btiUndelete";
            this.btiUndelete.Click += new System.EventHandler(this.undeleteToolStripMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            resources.ApplyResources(this.toolStripSeparator7, "toolStripSeparator7");
            // 
            // LineListingToolStripButton
            // 
            this.LineListingToolStripButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gridToolStripMenuItem,
            this.printableHTMLToolStripMenuItem,
            this.excelLineListMenuItem});
            resources.ApplyResources(this.LineListingToolStripButton, "LineListingToolStripButton");
            this.LineListingToolStripButton.Name = "LineListingToolStripButton";
            this.LineListingToolStripButton.ButtonClick += new System.EventHandler(this.gridToolStripMenuItem_Click);
            // 
            // gridToolStripMenuItem
            // 
            resources.ApplyResources(this.gridToolStripMenuItem, "gridToolStripMenuItem");
            this.gridToolStripMenuItem.Name = "gridToolStripMenuItem";
            this.gridToolStripMenuItem.Click += new System.EventHandler(this.gridToolStripMenuItem_Click);
            // 
            // printableHTMLToolStripMenuItem
            // 
            resources.ApplyResources(this.printableHTMLToolStripMenuItem, "printableHTMLToolStripMenuItem");
            this.printableHTMLToolStripMenuItem.Name = "printableHTMLToolStripMenuItem";
            this.printableHTMLToolStripMenuItem.Click += new System.EventHandler(this.printableHTMLToolStripMenuItem_Click);
            // 
            // excelLineListMenuItem
            // 
            resources.ApplyResources(this.excelLineListMenuItem, "excelLineListMenuItem");
            this.excelLineListMenuItem.Name = "excelLineListMenuItem";
            this.excelLineListMenuItem.Click += new System.EventHandler(this.excelLineListMenuItem_Click);
            // 
            // tsbDashboard
            // 
            resources.ApplyResources(this.tsbDashboard, "tsbDashboard");
            this.tsbDashboard.Name = "tsbDashboard";
            this.tsbDashboard.Click += new System.EventHandler(this.tsbDashboard_Click);
            // 
            // tsbMap
            // 
            resources.ApplyResources(this.tsbMap, "tsbMap");
            this.tsbMap.Name = "tsbMap";
            this.tsbMap.Click += new System.EventHandler(this.tsbMap_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            resources.ApplyResources(this.toolStripSeparator10, "toolStripSeparator10");
            // 
            // btiEditView
            // 
            resources.ApplyResources(this.btiEditView, "btiEditView");
            this.btiEditView.Name = "btiEditView";
            this.btiEditView.Click += new System.EventHandler(this.editViewToolStripMenuItem_Click);
            // 
            // btiHelp
            // 
            resources.ApplyResources(this.btiHelp, "btiHelp");
            this.btiHelp.Name = "btiHelp";
            // 
            // toolStrip2
            // 
            resources.ApplyResources(this.toolStrip2, "toolStrip2");
            this.toolStrip2.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnHome,
            this.toolStripSeparator9,
            this.btnBack});
            this.toolStrip2.Name = "toolStrip2";
            // 
            // btnHome
            // 
            this.btnHome.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.btnHome, "btnHome");
            this.btnHome.Name = "btnHome";
            this.btnHome.Click += new System.EventHandler(this.btnHome_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            resources.ApplyResources(this.toolStripSeparator9, "toolStripSeparator9");
            // 
            // btnBack
            // 
            this.btnBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.btnBack, "btnBack");
            this.btnBack.Name = "btnBack";
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // BottomToolStripPanel
            // 
            resources.ApplyResources(this.BottomToolStripPanel, "BottomToolStripPanel");
            this.BottomToolStripPanel.Name = "BottomToolStripPanel";
            this.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.BottomToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            // 
            // TopToolStripPanel
            // 
            resources.ApplyResources(this.TopToolStripPanel, "TopToolStripPanel");
            this.TopToolStripPanel.Name = "TopToolStripPanel";
            this.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.TopToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            // 
            // RightToolStripPanel
            // 
            resources.ApplyResources(this.RightToolStripPanel, "RightToolStripPanel");
            this.RightToolStripPanel.Name = "RightToolStripPanel";
            this.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.RightToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            // 
            // LeftToolStripPanel
            // 
            resources.ApplyResources(this.LeftToolStripPanel, "LeftToolStripPanel");
            this.LeftToolStripPanel.Name = "LeftToolStripPanel";
            this.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.LeftToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            // 
            // ContentPanel
            // 
            resources.ApplyResources(this.ContentPanel, "ContentPanel");
            // 
            // EnterMainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolStripContainer1);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "EnterMainForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Enter_FormClosing);
            this.Load += new System.EventHandler(this.Enter_Load);
            this.Controls.SetChildIndex(this.toolStripContainer1, 0);
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        #region Private Members

        private System.Windows.Forms.ToolStripPanel BottomToolStripPanel;
        private System.Windows.Forms.ToolStripPanel TopToolStripPanel;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btiNew;
        private System.Windows.Forms.ToolStripButton btiOpen;
        private System.Windows.Forms.ToolStripButton btiSave;
        private System.Windows.Forms.ToolStripButton btiPrint;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton btiFind;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripButton btiMarkDeleted;
        private System.Windows.Forms.ToolStripButton btiUndelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripButton btiHelp;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newRecordToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openViewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeViewToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem recentViewsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem markAsDeletedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undeleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compactDatabaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contentsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutEpiInfoToolStripMenuItem;
        private System.Windows.Forms.ToolStripPanel RightToolStripPanel;
        private System.Windows.Forms.ToolStripPanel LeftToolStripPanel;
        private System.Windows.Forms.ToolStripContentPanel ContentPanel;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private Epi.Windows.Docking.DockManager enterDockManager;

        private ViewExplorer viewExplorer;
        private Canvas canvas;
        private PresentationLogic.GuiMediator mediator;
        private View view;
        private View homeView;
        private string recordId;
        public bool isViewOpened = false;
        
        private System.Windows.Forms.ToolStripButton toolStripFirstButton;
        private System.Windows.Forms.ToolStripButton toolStripPreviousButton;
        private System.Windows.Forms.ToolStripTextBox toolStripRecordNumber;
        private System.Windows.Forms.ToolStripLabel toolStripOfLabel;
        private System.Windows.Forms.ToolStripButton toolStripNextButton;
        private System.Windows.Forms.ToolStripButton toolStripLastButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;

        private System.Windows.Forms.ToolStripLabel toolStripRecordCount;
        private System.Windows.Forms.ToolStripMenuItem editViewToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton btiEditView;

        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem statusBarToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripButton btnHome;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripButton btnBack;
        #endregion  //Private Members
        private System.Windows.Forms.ToolStripMenuItem epiInfoLogsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSplitButton LineListingToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem gridToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printableHTMLToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripMenuItem enableCheckCodeExecutionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enableCheckCodeErrorSupressionToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton tsbDashboard;
        private System.Windows.Forms.ToolStripButton tsbMap;
        private System.Windows.Forms.ToolStripMenuItem excelLineListMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dataDictionaryToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem mnuPackageForTransport;
        private System.Windows.Forms.ToolStripMenuItem importDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mnuImportFromWeb;
        private System.Windows.Forms.ToolStripMenuItem mnuImportFromForm;
        private System.Windows.Forms.ToolStripMenuItem mnuImportFromPhone;
        private System.Windows.Forms.ToolStripMenuItem mnuImportFromDataPackage;
        private System.Windows.Forms.ToolStripMenuItem copyShortcutToClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fromWebEnterToolStripMenuItem;
    }
}