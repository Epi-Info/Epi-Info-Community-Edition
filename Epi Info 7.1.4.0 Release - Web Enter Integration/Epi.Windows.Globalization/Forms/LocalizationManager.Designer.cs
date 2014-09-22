namespace Epi.Windows.Globalization.Forms
{
    partial class LocalizationManager
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LocalizationManager));
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.lblInstalledLanguages = new System.Windows.Forms.Label();
            this.lstInstalledLanguages = new System.Windows.Forms.ListBox();
            this.lblImport = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnExport = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.exportSaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.mapMetadata1 = new Epi.DataSets.MapMetadata();
            this.groupBoxExport = new System.Windows.Forms.GroupBox();
            this.txtLanguageDatabase = new System.Windows.Forms.TextBox();
            this.btnBrowseEpiDatabases = new System.Windows.Forms.Button();
            this.rbCreateLegacyEpiDatabase = new System.Windows.Forms.RadioButton();
            this.ddlCultures = new System.Windows.Forms.ComboBox();
            this.rbCreateExpansion = new System.Windows.Forms.RadioButton();
            this.rbCreateReverse = new System.Windows.Forms.RadioButton();
            this.rbCreateCopy = new System.Windows.Forms.RadioButton();
            this.rbCreateAutoTranslate = new System.Windows.Forms.RadioButton();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.panelMain = new System.Windows.Forms.Panel();
            this.btnTest = new System.Windows.Forms.Button();
            this.chkExportOptions = new System.Windows.Forms.CheckBox();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.statusStrip.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mapMetadata1)).BeginInit();
            this.groupBoxExport.SuspendLayout();
            this.panelMain.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.GripMargin = new System.Windows.Forms.Padding(0);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            resources.ApplyResources(this.statusStrip, "statusStrip");
            this.statusStrip.Name = "statusStrip";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Overflow = System.Windows.Forms.ToolStripItemOverflow.Always;
            resources.ApplyResources(this.toolStripStatusLabel, "toolStripStatusLabel");
            // 
            // btnImport
            // 
            resources.ApplyResources(this.btnImport, "btnImport");
            this.btnImport.Name = "btnImport";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // btnRemove
            // 
            resources.ApplyResources(this.btnRemove, "btnRemove");
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // lblInstalledLanguages
            // 
            resources.ApplyResources(this.lblInstalledLanguages, "lblInstalledLanguages");
            this.lblInstalledLanguages.Name = "lblInstalledLanguages";
            // 
            // lstInstalledLanguages
            // 
            this.lstInstalledLanguages.FormattingEnabled = true;
            resources.ApplyResources(this.lstInstalledLanguages, "lstInstalledLanguages");
            this.lstInstalledLanguages.Name = "lstInstalledLanguages";
            this.lstInstalledLanguages.SelectedIndexChanged += new System.EventHandler(this.lstInstalledLanguages_SelectedIndexChanged);
            // 
            // lblImport
            // 
            resources.ApplyResources(this.lblImport, "lblImport");
            this.lblImport.Name = "lblImport";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // btnExport
            // 
            resources.ApplyResources(this.btnExport, "btnExport");
            this.btnExport.Name = "btnExport";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.btnExport);
            this.groupBox1.Controls.Add(this.btnImport);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.lblImport);
            this.groupBox1.Controls.Add(this.lblInstalledLanguages);
            this.groupBox1.Controls.Add(this.btnRemove);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.lstInstalledLanguages);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // exportSaveFileDialog
            // 
            resources.ApplyResources(this.exportSaveFileDialog, "exportSaveFileDialog");
            // 
            // mapMetadata1
            // 
            this.mapMetadata1.DataSetName = "MapMetadata";
            this.mapMetadata1.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // groupBoxExport
            // 
            resources.ApplyResources(this.groupBoxExport, "groupBoxExport");
            this.groupBoxExport.Controls.Add(this.txtLanguageDatabase);
            this.groupBoxExport.Controls.Add(this.btnBrowseEpiDatabases);
            this.groupBoxExport.Controls.Add(this.rbCreateLegacyEpiDatabase);
            this.groupBoxExport.Controls.Add(this.ddlCultures);
            this.groupBoxExport.Controls.Add(this.rbCreateExpansion);
            this.groupBoxExport.Controls.Add(this.rbCreateReverse);
            this.groupBoxExport.Controls.Add(this.rbCreateCopy);
            this.groupBoxExport.Controls.Add(this.rbCreateAutoTranslate);
            this.groupBoxExport.Name = "groupBoxExport";
            this.groupBoxExport.TabStop = false;
            // 
            // txtLanguageDatabase
            // 
            resources.ApplyResources(this.txtLanguageDatabase, "txtLanguageDatabase");
            this.txtLanguageDatabase.Name = "txtLanguageDatabase";
            // 
            // btnBrowseEpiDatabases
            // 
            resources.ApplyResources(this.btnBrowseEpiDatabases, "btnBrowseEpiDatabases");
            this.btnBrowseEpiDatabases.Name = "btnBrowseEpiDatabases";
            this.btnBrowseEpiDatabases.UseVisualStyleBackColor = true;
            this.btnBrowseEpiDatabases.Click += new System.EventHandler(this.btnBrowseEpiDatabases_Click);
            // 
            // rbCreateLegacyEpiDatabase
            // 
            resources.ApplyResources(this.rbCreateLegacyEpiDatabase, "rbCreateLegacyEpiDatabase");
            this.rbCreateLegacyEpiDatabase.Name = "rbCreateLegacyEpiDatabase";
            this.rbCreateLegacyEpiDatabase.UseVisualStyleBackColor = true;
            this.rbCreateLegacyEpiDatabase.CheckedChanged += new System.EventHandler(this.rbCreateLegacyEpiDatabase_CheckedChanged);
            // 
            // ddlCultures
            // 
            resources.ApplyResources(this.ddlCultures, "ddlCultures");
            this.ddlCultures.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlCultures.FormattingEnabled = true;
            this.ddlCultures.Name = "ddlCultures";
            // 
            // rbCreateExpansion
            // 
            resources.ApplyResources(this.rbCreateExpansion, "rbCreateExpansion");
            this.rbCreateExpansion.Name = "rbCreateExpansion";
            this.rbCreateExpansion.UseVisualStyleBackColor = true;
            // 
            // rbCreateReverse
            // 
            resources.ApplyResources(this.rbCreateReverse, "rbCreateReverse");
            this.rbCreateReverse.Name = "rbCreateReverse";
            this.rbCreateReverse.UseVisualStyleBackColor = true;
            // 
            // rbCreateCopy
            // 
            resources.ApplyResources(this.rbCreateCopy, "rbCreateCopy");
            this.rbCreateCopy.Checked = true;
            this.rbCreateCopy.Name = "rbCreateCopy";
            this.rbCreateCopy.TabStop = true;
            this.rbCreateCopy.UseVisualStyleBackColor = true;
            // 
            // rbCreateAutoTranslate
            // 
            resources.ApplyResources(this.rbCreateAutoTranslate, "rbCreateAutoTranslate");
            this.rbCreateAutoTranslate.Name = "rbCreateAutoTranslate";
            this.rbCreateAutoTranslate.UseVisualStyleBackColor = true;
            this.rbCreateAutoTranslate.CheckedChanged += new System.EventHandler(this.rbCreateAutoTranslate_CheckedChanged);
            // 
            // btnExit
            // 
            resources.ApplyResources(this.btnExit, "btnExit");
            this.btnExit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnExit.Name = "btnExit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnHelp
            // 
            resources.ApplyResources(this.btnHelp, "btnHelp");
            this.btnHelp.Name = "btnHelp"; btnHelp.Enabled = false;
            this.btnHelp.UseVisualStyleBackColor = true;
            // 
            // panelMain
            // 
            this.panelMain.Controls.Add(this.btnTest);
            this.panelMain.Controls.Add(this.chkExportOptions);
            this.panelMain.Controls.Add(this.btnExit);
            this.panelMain.Controls.Add(this.label2);
            this.panelMain.Controls.Add(this.groupBoxExport);
            this.panelMain.Controls.Add(this.btnHelp);
            this.panelMain.Controls.Add(this.groupBox1);
            resources.ApplyResources(this.panelMain, "panelMain");
            this.panelMain.Name = "panelMain";
            // 
            // btnTest
            // 
            resources.ApplyResources(this.btnTest, "btnTest");
            this.btnTest.Name = "btnTest";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // chkExportOptions
            // 
            resources.ApplyResources(this.chkExportOptions, "chkExportOptions");
            this.chkExportOptions.Name = "chkExportOptions";
            this.chkExportOptions.UseVisualStyleBackColor = true;
            this.chkExportOptions.CheckedChanged += new System.EventHandler(this.chkExportOptions_CheckedChanged);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            resources.ApplyResources(this.importToolStripMenuItem, "importToolStripMenuItem");
            this.importToolStripMenuItem.Click += new System.EventHandler(this.importToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            resources.ApplyResources(this.menuStrip, "menuStrip");
            this.menuStrip.Name = "menuStrip";
            // 
            // LocalizationManager
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnExit;
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.Controls.Add(this.panelMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = true;
            this.MainMenuStrip = this.menuStrip;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LocalizationManager";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Load += new System.EventHandler(this.LocalizationManager_Load);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mapMetadata1)).EndInit();
            this.groupBoxExport.ResumeLayout(false);
            this.groupBoxExport.PerformLayout();
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.ListBox lstInstalledLanguages;
        private System.Windows.Forms.Label lblInstalledLanguages;
        private System.Windows.Forms.Label lblImport;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.SaveFileDialog exportSaveFileDialog;
        private Epi.DataSets.MapMetadata mapMetadata1;
        private System.Windows.Forms.GroupBox groupBoxExport;
        private System.Windows.Forms.RadioButton rbCreateExpansion;
        private System.Windows.Forms.RadioButton rbCreateReverse;
        private System.Windows.Forms.RadioButton rbCreateCopy;
        private System.Windows.Forms.RadioButton rbCreateAutoTranslate;
        private System.Windows.Forms.ComboBox ddlCultures;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.CheckBox chkExportOptions;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.TextBox txtLanguageDatabase;
        private System.Windows.Forms.Button btnBrowseEpiDatabases;
        private System.Windows.Forms.RadioButton rbCreateLegacyEpiDatabase;
    }
}