namespace Epi.Windows.MakeView.Forms
{
    partial class CheckCode
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CheckCode));
            this.tvCodeBlocks = new System.Windows.Forms.TreeView();
            this.codeText = new System.Windows.Forms.RichTextBox();
            this.CommandContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CheckCodeStatusBar = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.LineNumberLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.ColumnNumberLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.ContextLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel6 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.basicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.advancedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.PositionNumberLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.ValidateCodeToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.btnCancel = new System.Windows.Forms.ToolStripButton();
            this.btnSave = new System.Windows.Forms.ToolStripButton();
            this.btnPrint = new System.Windows.Forms.ToolStripButton();
            this.AddCodeBlockToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.mainImageList = new System.Windows.Forms.ImageList(this.components);
            this.StatusTextBox = new System.Windows.Forms.RichTextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnAddBlock = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.Commands = new System.Windows.Forms.ListBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.CheckCodeMenu = new System.Windows.Forms.MenuStrip();
            this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileValidate = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileSave = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFilePrint = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileClose = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditUndo = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditRedo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuEditCut = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuEditFind = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditFindNext = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditReplace = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFonts = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFontsSetEditorFont = new System.Windows.Forms.ToolStripMenuItem();
            this.CheckCodeFontDialog = new System.Windows.Forms.FontDialog();
            this.CommandContextMenu.SuspendLayout();
            this.CheckCodeStatusBar.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.CheckCodeMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // tvCodeBlocks
            // 
            resources.ApplyResources(this.tvCodeBlocks, "tvCodeBlocks");
            this.tvCodeBlocks.HideSelection = false;
            this.tvCodeBlocks.Name = "tvCodeBlocks";
            // 
            // codeText
            // 
            this.codeText.AcceptsTab = true;
            resources.ApplyResources(this.codeText, "codeText");
            this.codeText.AutoWordSelection = true;
            this.codeText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.codeText.ContextMenuStrip = this.CommandContextMenu;
            this.codeText.ForeColor = System.Drawing.Color.Blue;
            this.codeText.HideSelection = false;
            this.codeText.Name = "codeText";
            this.codeText.ShowSelectionMargin = true;
            this.codeText.TextChanged += new System.EventHandler(this.codeText_TextChanged);
            this.codeText.KeyUp += new System.Windows.Forms.KeyEventHandler(this.codeText_KeyUp);
            this.codeText.MouseDown += new System.Windows.Forms.MouseEventHandler(this.codeText_MouseDown);
            // 
            // CommandContextMenu
            // 
            this.CommandContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.CommandContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.CommandContextMenu.Name = "contextMenuStrip1";
            resources.ApplyResources(this.CommandContextMenu, "CommandContextMenu");
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            resources.ApplyResources(this.cutToolStripMenuItem, "cutToolStripMenuItem");
            this.cutToolStripMenuItem.Click += new System.EventHandler(this.CutToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            resources.ApplyResources(this.copyToolStripMenuItem, "copyToolStripMenuItem");
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            resources.ApplyResources(this.pasteToolStripMenuItem, "pasteToolStripMenuItem");
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            resources.ApplyResources(this.deleteToolStripMenuItem, "deleteToolStripMenuItem");
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // CheckCodeStatusBar
            // 
            this.CheckCodeStatusBar.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.CheckCodeStatusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.LineNumberLabel,
            this.toolStripStatusLabel2,
            this.ColumnNumberLabel,
            this.ContextLabel,
            this.toolStripStatusLabel6,
            this.toolStripDropDownButton1,
            this.toolStripStatusLabel3,
            this.PositionNumberLabel});
            resources.ApplyResources(this.CheckCodeStatusBar, "CheckCodeStatusBar");
            this.CheckCodeStatusBar.Name = "CheckCodeStatusBar";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            resources.ApplyResources(this.toolStripStatusLabel1, "toolStripStatusLabel1");
            // 
            // LineNumberLabel
            // 
            this.LineNumberLabel.Name = "LineNumberLabel";
            resources.ApplyResources(this.LineNumberLabel, "LineNumberLabel");
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            resources.ApplyResources(this.toolStripStatusLabel2, "toolStripStatusLabel2");
            // 
            // ColumnNumberLabel
            // 
            this.ColumnNumberLabel.Name = "ColumnNumberLabel";
            resources.ApplyResources(this.ColumnNumberLabel, "ColumnNumberLabel");
            // 
            // ContextLabel
            // 
            this.ContextLabel.Name = "ContextLabel";
            resources.ApplyResources(this.ContextLabel, "ContextLabel");
            // 
            // toolStripStatusLabel6
            // 
            this.toolStripStatusLabel6.Name = "toolStripStatusLabel6";
            resources.ApplyResources(this.toolStripStatusLabel6, "toolStripStatusLabel6");
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.basicToolStripMenuItem,
            this.advancedToolStripMenuItem});
            resources.ApplyResources(this.toolStripDropDownButton1, "toolStripDropDownButton1");
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            // 
            // basicToolStripMenuItem
            // 
            resources.ApplyResources(this.basicToolStripMenuItem, "basicToolStripMenuItem");
            this.basicToolStripMenuItem.Name = "basicToolStripMenuItem";
            // 
            // advancedToolStripMenuItem
            // 
            resources.ApplyResources(this.advancedToolStripMenuItem, "advancedToolStripMenuItem");
            this.advancedToolStripMenuItem.Name = "advancedToolStripMenuItem";
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            resources.ApplyResources(this.toolStripStatusLabel3, "toolStripStatusLabel3");
            // 
            // PositionNumberLabel
            // 
            this.PositionNumberLabel.Name = "PositionNumberLabel";
            resources.ApplyResources(this.PositionNumberLabel, "PositionNumberLabel");
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripMargin = new System.Windows.Forms.Padding(0);
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ValidateCodeToolStripButton,
            this.btnCancel,
            this.btnSave,
            this.btnPrint,
            this.AddCodeBlockToolStripButton});
            resources.ApplyResources(this.toolStrip1, "toolStrip1");
            this.toolStrip1.Name = "toolStrip1";
            // 
            // ValidateCodeToolStripButton
            // 
            resources.ApplyResources(this.ValidateCodeToolStripButton, "ValidateCodeToolStripButton");
            this.ValidateCodeToolStripButton.Name = "ValidateCodeToolStripButton";
            this.ValidateCodeToolStripButton.Click += new System.EventHandler(this.ValidateCodeToolStripButton_Click);
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSave
            // 
            resources.ApplyResources(this.btnSave, "btnSave");
            this.btnSave.Name = "btnSave";
            this.btnSave.Click += new System.EventHandler(this.SaveHandler);
            // 
            // btnPrint
            // 
            resources.ApplyResources(this.btnPrint, "btnPrint");
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // AddCodeBlockToolStripButton
            // 
            this.AddCodeBlockToolStripButton.Name = "AddCodeBlockToolStripButton";
            resources.ApplyResources(this.AddCodeBlockToolStripButton, "AddCodeBlockToolStripButton");
            // 
            // mainImageList
            // 
            this.mainImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("mainImageList.ImageStream")));
            this.mainImageList.TransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.mainImageList.Images.SetKeyName(0, "OpenFolder.bmp");
            this.mainImageList.Images.SetKeyName(1, "ClosedFolder.bmp");
            this.mainImageList.Images.SetKeyName(2, "CodeSnippet.bmp");
            this.mainImageList.Images.SetKeyName(3, "deleteSmall.bmp");
            // 
            // StatusTextBox
            // 
            resources.ApplyResources(this.StatusTextBox, "StatusTextBox");
            this.StatusTextBox.Name = "StatusTextBox";
            this.StatusTextBox.ReadOnly = true;
            this.StatusTextBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.StatusTextBox_MouseMove);
            this.StatusTextBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.StatusTextBox_MouseUp);
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.btnAddBlock);
            this.groupBox1.Controls.Add(this.tvCodeBlocks);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // btnAddBlock
            // 
            resources.ApplyResources(this.btnAddBlock, "btnAddBlock");
            this.btnAddBlock.Name = "btnAddBlock";
            this.btnAddBlock.UseVisualStyleBackColor = true;
            this.btnAddBlock.Click += new System.EventHandler(this.btnAddBlock_Click);
            // 
            // groupBox2
            // 
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Controls.Add(this.Commands);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // Commands
            // 
            resources.ApplyResources(this.Commands, "Commands");
            this.Commands.FormattingEnabled = true;
            this.Commands.Items.AddRange(new object[] {
            resources.GetString("Commands.Items"),
            resources.GetString("Commands.Items1"),
            resources.GetString("Commands.Items2"),
            resources.GetString("Commands.Items3"),
            resources.GetString("Commands.Items4"),
            resources.GetString("Commands.Items5"),
            resources.GetString("Commands.Items6"),
            resources.GetString("Commands.Items7"),
            resources.GetString("Commands.Items8"),
            resources.GetString("Commands.Items9"),
            resources.GetString("Commands.Items10"),
            resources.GetString("Commands.Items11"),
            resources.GetString("Commands.Items12"),
            resources.GetString("Commands.Items13"),
            resources.GetString("Commands.Items14"),
            resources.GetString("Commands.Items15"),
            resources.GetString("Commands.Items16"),
            resources.GetString("Commands.Items17"),
            resources.GetString("Commands.Items18"),
            resources.GetString("Commands.Items19"),
            resources.GetString("Commands.Items20"),
            resources.GetString("Commands.Items21"),
            resources.GetString("Commands.Items22")});
            this.Commands.Name = "Commands";
            this.Commands.SelectedIndexChanged += new System.EventHandler(this.Commands_SelectedIndexChanged);
            // 
            // groupBox3
            // 
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.Controls.Add(this.StatusTextBox);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.codeText);
            this.splitContainer1.Panel1.Controls.Add(this.groupBox3);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            // 
            // splitContainer2
            // 
            resources.ApplyResources(this.splitContainer2, "splitContainer2");
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.groupBox2);
            // 
            // CheckCodeMenu
            // 
            this.CheckCodeMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.CheckCodeMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFile,
            this.mnuEdit,
            this.mnuFonts});
            resources.ApplyResources(this.CheckCodeMenu, "CheckCodeMenu");
            this.CheckCodeMenu.Name = "CheckCodeMenu";
            // 
            // mnuFile
            // 
            this.mnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFileValidate,
            this.mnuFileSave,
            this.mnuFileSaveAs,
            this.mnuFilePrint,
            this.mnuFileClose});
            this.mnuFile.Name = "mnuFile";
            resources.ApplyResources(this.mnuFile, "mnuFile");
            // 
            // mnuFileValidate
            // 
            this.mnuFileValidate.Name = "mnuFileValidate";
            resources.ApplyResources(this.mnuFileValidate, "mnuFileValidate");
            this.mnuFileValidate.Click += new System.EventHandler(this.ValidateCodeToolStripButton_Click);
            // 
            // mnuFileSave
            // 
            this.mnuFileSave.Name = "mnuFileSave";
            resources.ApplyResources(this.mnuFileSave, "mnuFileSave");
            this.mnuFileSave.Click += new System.EventHandler(this.SaveHandler);
            // 
            // mnuFileSaveAs
            // 
            this.mnuFileSaveAs.Name = "mnuFileSaveAs";
            resources.ApplyResources(this.mnuFileSaveAs, "mnuFileSaveAs");
            this.mnuFileSaveAs.Click += new System.EventHandler(this.mnuFileSaveAs_Click);
            // 
            // mnuFilePrint
            // 
            this.mnuFilePrint.Name = "mnuFilePrint";
            resources.ApplyResources(this.mnuFilePrint, "mnuFilePrint");
            this.mnuFilePrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // mnuFileClose
            // 
            this.mnuFileClose.Name = "mnuFileClose";
            resources.ApplyResources(this.mnuFileClose, "mnuFileClose");
            this.mnuFileClose.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // mnuEdit
            // 
            this.mnuEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuEditUndo,
            this.mnuEditRedo,
            this.toolStripMenuItem2,
            this.mnuEditCut,
            this.mnuEditCopy,
            this.mnuEditPaste,
            this.toolStripMenuItem1,
            this.mnuEditFind,
            this.mnuEditFindNext,
            this.mnuEditReplace});
            this.mnuEdit.Name = "mnuEdit";
            resources.ApplyResources(this.mnuEdit, "mnuEdit");
            // 
            // mnuEditUndo
            // 
            this.mnuEditUndo.Name = "mnuEditUndo";
            resources.ApplyResources(this.mnuEditUndo, "mnuEditUndo");
            this.mnuEditUndo.Click += new System.EventHandler(this.mnuEditUndo_Click);
            // 
            // mnuEditRedo
            // 
            this.mnuEditRedo.Name = "mnuEditRedo";
            resources.ApplyResources(this.mnuEditRedo, "mnuEditRedo");
            this.mnuEditRedo.Click += new System.EventHandler(this.mnuEditRedo_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
            // 
            // mnuEditCut
            // 
            this.mnuEditCut.Name = "mnuEditCut";
            resources.ApplyResources(this.mnuEditCut, "mnuEditCut");
            this.mnuEditCut.Click += new System.EventHandler(this.mnuEditCut_Click);
            // 
            // mnuEditCopy
            // 
            this.mnuEditCopy.Name = "mnuEditCopy";
            resources.ApplyResources(this.mnuEditCopy, "mnuEditCopy");
            this.mnuEditCopy.Click += new System.EventHandler(this.mnuEditCopy_Click);
            // 
            // mnuEditPaste
            // 
            this.mnuEditPaste.Name = "mnuEditPaste";
            resources.ApplyResources(this.mnuEditPaste, "mnuEditPaste");
            this.mnuEditPaste.Click += new System.EventHandler(this.mnuEditPaste_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
            // 
            // mnuEditFind
            // 
            this.mnuEditFind.Name = "mnuEditFind";
            resources.ApplyResources(this.mnuEditFind, "mnuEditFind");
            this.mnuEditFind.Click += new System.EventHandler(this.mnuEditFind_Click);
            // 
            // mnuEditFindNext
            // 
            this.mnuEditFindNext.Name = "mnuEditFindNext";
            resources.ApplyResources(this.mnuEditFindNext, "mnuEditFindNext");
            this.mnuEditFindNext.Click += new System.EventHandler(this.mnuEditFindNext_Click);
            // 
            // mnuEditReplace
            // 
            this.mnuEditReplace.Name = "mnuEditReplace";
            resources.ApplyResources(this.mnuEditReplace, "mnuEditReplace");
            this.mnuEditReplace.Click += new System.EventHandler(this.mnuEditReplace_Click);
            // 
            // mnuFonts
            // 
            this.mnuFonts.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFontsSetEditorFont});
            this.mnuFonts.Name = "mnuFonts";
            resources.ApplyResources(this.mnuFonts, "mnuFonts");
            // 
            // mnuFontsSetEditorFont
            // 
            this.mnuFontsSetEditorFont.Name = "mnuFontsSetEditorFont";
            resources.ApplyResources(this.mnuFontsSetEditorFont, "mnuFontsSetEditorFont");
            this.mnuFontsSetEditorFont.Click += new System.EventHandler(this.mnuFontsSetEditorFont_Click);
            // 
            // CheckCode
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.CheckCodeStatusBar);
            this.Controls.Add(this.CheckCodeMenu);
            this.MainMenuStrip = this.CheckCodeMenu;
            this.MinimizeBox = false;
            this.Name = "CheckCode";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CheckCode_FormClosing);
            this.Shown += new System.EventHandler(this.CheckCode_Shown);
            this.ResizeEnd += new System.EventHandler(this.CheckCode_ResizeEnd);
            this.CommandContextMenu.ResumeLayout(false);
            this.CheckCodeStatusBar.ResumeLayout(false);
            this.CheckCodeStatusBar.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.CheckCodeMenu.ResumeLayout(false);
            this.CheckCodeMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TreeView tvCodeBlocks;
        public System.Windows.Forms.RichTextBox codeText;
        private System.Windows.Forms.StatusStrip CheckCodeStatusBar;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel LineNumberLabel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel ColumnNumberLabel;
        private System.Windows.Forms.ToolStripStatusLabel ContextLabel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel6;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem basicToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem advancedToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnCancel;
        private System.Windows.Forms.ToolStripButton btnSave;
        private System.Windows.Forms.ToolStripButton btnPrint;
        private System.Windows.Forms.ToolStripButton AddCodeBlockToolStripButton;
        private System.Windows.Forms.ContextMenuStrip CommandContextMenu;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ImageList mainImageList;
        private System.Windows.Forms.RichTextBox StatusTextBox;
        private System.Windows.Forms.ToolStripButton ValidateCodeToolStripButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnAddBlock;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ListBox Commands;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.ToolStripStatusLabel PositionNumberLabel;
        private System.Windows.Forms.MenuStrip CheckCodeMenu;
        private System.Windows.Forms.ToolStripMenuItem mnuEdit;
        private System.Windows.Forms.ToolStripMenuItem mnuEditFind;
        private System.Windows.Forms.ToolStripMenuItem mnuEditReplace;
        private System.Windows.Forms.ToolStripMenuItem mnuEditFindNext;
        private System.Windows.Forms.ToolStripMenuItem mnuEditUndo;
        private System.Windows.Forms.ToolStripMenuItem mnuEditRedo;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem mnuEditCut;
        private System.Windows.Forms.ToolStripMenuItem mnuEditCopy;
        private System.Windows.Forms.ToolStripMenuItem mnuEditPaste;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem mnuFile;
        private System.Windows.Forms.ToolStripMenuItem mnuFonts;
        private System.Windows.Forms.ToolStripMenuItem mnuFileValidate;
        private System.Windows.Forms.ToolStripMenuItem mnuFileSave;
        private System.Windows.Forms.ToolStripMenuItem mnuFileSaveAs;
        private System.Windows.Forms.ToolStripMenuItem mnuFilePrint;
        private System.Windows.Forms.ToolStripMenuItem mnuFileClose;
        private System.Windows.Forms.ToolStripMenuItem mnuFontsSetEditorFont;
        private System.Windows.Forms.FontDialog CheckCodeFontDialog;
    }
}