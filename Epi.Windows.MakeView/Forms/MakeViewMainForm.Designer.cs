#region namespaces
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using System.Reflection;
using Epi.DataSets;
using Epi.Fields;
using Epi.Windows;
using Epi.Windows.Controls;
using Epi.Windows.Dialogs;
using Epi.Windows.MakeView.Utils;
using Epi.Windows.MakeView.Dialogs;
using Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs;

#endregion

namespace Epi.Windows.MakeView.Forms
{
    partial class MakeViewMainForm
    {

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MakeViewMainForm));
            this.MainToolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this.dockManager1 = new Epi.Windows.Docking.DockManager(this.components);
            this.MainMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.NewProjectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.NewProjectFromTemplateMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.NewViewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.NewPageMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.openProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuImportTemplate = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCopyToPhone = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripPublishToWebEnter = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPublishToWeb = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.recentProjectsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.exitMakeViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuEditCut = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuEditPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.deletePageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renamePageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusBarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.vocabularyFieldsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.epiInfoLogsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pageToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.insertPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.forToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setDefaultFontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setDefaultControlFontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.alignmentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.verticalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.horizontalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backgroundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pageSetupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displayDataDictionaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkCodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.importEpi6RecFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importCheckCodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.makeViewFromDataTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.upgradeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Import35xToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Import6xToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.makeProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.makeAccessPRJToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.makeSQLPRJToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createDataTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteDataTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enterDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutEpiInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MainToolStrip = new System.Windows.Forms.ToolStrip();
            this.btnNew = new System.Windows.Forms.ToolStripButton();
            this.btnOpen = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonCloseProject = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonUndo = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonRedo = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparatorAfterUndoRedo = new System.Windows.Forms.ToolStripSeparator();
            this.btnCheckCode = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.QuickPublishtoolStripButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.toWebSurveyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toWebEnterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ChangeModetoolStripDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.EIWSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.draftToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.finalToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.EWEToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.draftToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.finalToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.btnEnterData = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.orderOfFieldEntryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnSave = new System.Windows.Forms.ToolStripButton();
            this.btnCut = new System.Windows.Forms.ToolStripButton();
            this.btnCopy = new System.Windows.Forms.ToolStripButton();
            this.btnPaste = new System.Windows.Forms.ToolStripButton();
            this.MainToolStripContainer.ContentPanel.SuspendLayout();
            this.MainToolStripContainer.TopToolStripPanel.SuspendLayout();
            this.MainToolStripContainer.SuspendLayout();
            this.MainMenu.SuspendLayout();
            this.MainToolStrip.SuspendLayout();
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
            // MainToolStripContainer
            // 
            // 
            // MainToolStripContainer.ContentPanel
            // 
            this.MainToolStripContainer.ContentPanel.Controls.Add(this.dockManager1);
            resources.ApplyResources(this.MainToolStripContainer.ContentPanel, "MainToolStripContainer.ContentPanel");
            resources.ApplyResources(this.MainToolStripContainer, "MainToolStripContainer");
            this.MainToolStripContainer.Name = "MainToolStripContainer";
            // 
            // MainToolStripContainer.TopToolStripPanel
            // 
            this.MainToolStripContainer.TopToolStripPanel.Controls.Add(this.MainMenu);
            this.MainToolStripContainer.TopToolStripPanel.Controls.Add(this.MainToolStrip);
            // 
            // dockManager1
            // 
            this.dockManager1.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.dockManager1, "dockManager1");
            this.dockManager1.DockBorder = 20;
            this.dockManager1.DockType = Epi.Windows.Docking.DockContainerType.Document;
            this.dockManager1.FastDrawing = true;
            this.dockManager1.Name = "dockManager1";
            this.dockManager1.ShowIcons = true;
            this.dockManager1.SplitterWidth = 4;
            this.dockManager1.VisualStyle = Epi.Windows.Docking.DockVisualStyle.VS2003;
            // 
            // MainMenu
            // 
            resources.ApplyResources(this.MainMenu, "MainMenu");
            this.MainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.insertToolStripMenuItem,
            this.forToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.MainMenu.Name = "MainMenu";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewProjectMenuItem,
            this.NewProjectFromTemplateMenuItem,
            this.NewViewMenuItem,
            this.NewPageMenuItem,
            this.toolStripSeparator9,
            this.openProjectToolStripMenuItem,
            this.closeProjectToolStripMenuItem,
            this.mnuImportTemplate,
            this.mnuCopyToPhone,
            this.toolStripPublishToWebEnter,
            this.mnuPublishToWeb,
            this.toolStripSeparator3,
            this.recentProjectsToolStripMenuItem,
            this.toolStripSeparator5,
            this.exitMakeViewToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            // 
            // NewProjectMenuItem
            // 
            resources.ApplyResources(this.NewProjectMenuItem, "NewProjectMenuItem");
            this.NewProjectMenuItem.Name = "NewProjectMenuItem";
            this.NewProjectMenuItem.Click += new System.EventHandler(this.NewProjectMenuItem_Click);
            // 
            // NewProjectFromTemplateMenuItem
            // 
            resources.ApplyResources(this.NewProjectFromTemplateMenuItem, "NewProjectFromTemplateMenuItem");
            this.NewProjectFromTemplateMenuItem.Name = "NewProjectFromTemplateMenuItem";
            this.NewProjectFromTemplateMenuItem.Click += new System.EventHandler(this.NewProjectFromTemplateMenuItem_Click);
            // 
            // NewViewMenuItem
            // 
            resources.ApplyResources(this.NewViewMenuItem, "NewViewMenuItem");
            this.NewViewMenuItem.Name = "NewViewMenuItem";
            this.NewViewMenuItem.Click += new System.EventHandler(this.NewViewMenuItem_Click);
            // 
            // NewPageMenuItem
            // 
            resources.ApplyResources(this.NewPageMenuItem, "NewPageMenuItem");
            this.NewPageMenuItem.Name = "NewPageMenuItem";
            this.NewPageMenuItem.Click += new System.EventHandler(this.NewPageMenuItem_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            resources.ApplyResources(this.toolStripSeparator9, "toolStripSeparator9");
            // 
            // openProjectToolStripMenuItem
            // 
            resources.ApplyResources(this.openProjectToolStripMenuItem, "openProjectToolStripMenuItem");
            this.openProjectToolStripMenuItem.Name = "openProjectToolStripMenuItem";
            this.openProjectToolStripMenuItem.Click += new System.EventHandler(this.OpenProject_Click);
            // 
            // closeProjectToolStripMenuItem
            // 
            resources.ApplyResources(this.closeProjectToolStripMenuItem, "closeProjectToolStripMenuItem");
            this.closeProjectToolStripMenuItem.Name = "closeProjectToolStripMenuItem";
            this.closeProjectToolStripMenuItem.Click += new System.EventHandler(this.closeProjectToolStripMenuItem_Click);
            // 
            // mnuImportTemplate
            // 
            this.mnuImportTemplate.Name = "mnuImportTemplate";
            resources.ApplyResources(this.mnuImportTemplate, "mnuImportTemplate");
            this.mnuImportTemplate.Click += new System.EventHandler(this.mnuImportTemplate_Click);
            // 
            // mnuCopyToPhone
            // 
            resources.ApplyResources(this.mnuCopyToPhone, "mnuCopyToPhone");
            this.mnuCopyToPhone.Image = global::Epi.Windows.MakeView.Properties.Resources.android_icon;
            this.mnuCopyToPhone.Name = "mnuCopyToPhone";
            this.mnuCopyToPhone.Click += new System.EventHandler(this.mnuCopyToPhone_Click);
            // 
            // toolStripPublishToWebEnter
            // 
            resources.ApplyResources(this.toolStripPublishToWebEnter, "toolStripPublishToWebEnter");
            this.toolStripPublishToWebEnter.Image = global::Epi.Windows.MakeView.Properties.Resources.mail_send;
            this.toolStripPublishToWebEnter.Name = "toolStripPublishToWebEnter";
            this.toolStripPublishToWebEnter.Click += new System.EventHandler(this.toolStripPublishToWebEnter_Click);
            // 
            // mnuPublishToWeb
            // 
            resources.ApplyResources(this.mnuPublishToWeb, "mnuPublishToWeb");
            this.mnuPublishToWeb.Image = global::Epi.Windows.MakeView.Properties.Resources.mail_send;
            this.mnuPublishToWeb.Name = "mnuPublishToWeb";
            this.mnuPublishToWeb.Click += new System.EventHandler(this.publishNewSurveyToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            // 
            // recentProjectsToolStripMenuItem
            // 
            this.recentProjectsToolStripMenuItem.Name = "recentProjectsToolStripMenuItem";
            resources.ApplyResources(this.recentProjectsToolStripMenuItem, "recentProjectsToolStripMenuItem");
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
            // 
            // exitMakeViewToolStripMenuItem
            // 
            this.exitMakeViewToolStripMenuItem.Name = "exitMakeViewToolStripMenuItem";
            resources.ApplyResources(this.exitMakeViewToolStripMenuItem, "exitMakeViewToolStripMenuItem");
            this.exitMakeViewToolStripMenuItem.Click += new System.EventHandler(this.exitMakeViewToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.toolStripSeparator8,
            this.mnuEditCut,
            this.mnuEditCopy,
            this.mnuEditPaste,
            this.deletePageToolStripMenuItem,
            this.renamePageToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            resources.ApplyResources(this.editToolStripMenuItem, "editToolStripMenuItem");
            this.editToolStripMenuItem.Click += new System.EventHandler(this.editToolStripMenuItem_Click);
            // 
            // undoToolStripMenuItem
            // 
            resources.ApplyResources(this.undoToolStripMenuItem, "undoToolStripMenuItem");
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.toolStripButtonUndo_Click);
            // 
            // redoToolStripMenuItem
            // 
            resources.ApplyResources(this.redoToolStripMenuItem, "redoToolStripMenuItem");
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.Click += new System.EventHandler(this.toolStripButtonRedo_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            resources.ApplyResources(this.toolStripSeparator8, "toolStripSeparator8");
            // 
            // mnuEditCut
            // 
            this.mnuEditCut.Name = "mnuEditCut";
            resources.ApplyResources(this.mnuEditCut, "mnuEditCut");
            this.mnuEditCut.Click += new System.EventHandler(this.editCut_Click);
            // 
            // mnuEditCopy
            // 
            this.mnuEditCopy.Name = "mnuEditCopy";
            resources.ApplyResources(this.mnuEditCopy, "mnuEditCopy");
            this.mnuEditCopy.Click += new System.EventHandler(this.editCopy_Click);
            // 
            // mnuEditPaste
            // 
            this.mnuEditPaste.Name = "mnuEditPaste";
            resources.ApplyResources(this.mnuEditPaste, "mnuEditPaste");
            this.mnuEditPaste.Click += new System.EventHandler(this.editPaste_Click);
            // 
            // deletePageToolStripMenuItem
            // 
            resources.ApplyResources(this.deletePageToolStripMenuItem, "deletePageToolStripMenuItem");
            this.deletePageToolStripMenuItem.Name = "deletePageToolStripMenuItem";
            this.deletePageToolStripMenuItem.Click += new System.EventHandler(this.deletePageToolStripMenuItem_Click);
            // 
            // renamePageToolStripMenuItem
            // 
            resources.ApplyResources(this.renamePageToolStripMenuItem, "renamePageToolStripMenuItem");
            this.renamePageToolStripMenuItem.Name = "renamePageToolStripMenuItem";
            this.renamePageToolStripMenuItem.Click += new System.EventHandler(this.renamePageToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusBarToolStripMenuItem,
            this.vocabularyFieldsToolStripMenuItem,
            this.epiInfoLogsToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            resources.ApplyResources(this.viewToolStripMenuItem, "viewToolStripMenuItem");
            // 
            // statusBarToolStripMenuItem
            // 
            this.statusBarToolStripMenuItem.Checked = true;
            this.statusBarToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.statusBarToolStripMenuItem.Name = "statusBarToolStripMenuItem";
            resources.ApplyResources(this.statusBarToolStripMenuItem, "statusBarToolStripMenuItem");
            this.statusBarToolStripMenuItem.Click += new System.EventHandler(this.statusBarToolStripMenuItem_Click);
            // 
            // vocabularyFieldsToolStripMenuItem
            // 
            this.vocabularyFieldsToolStripMenuItem.CheckOnClick = true;
            this.vocabularyFieldsToolStripMenuItem.Name = "vocabularyFieldsToolStripMenuItem";
            resources.ApplyResources(this.vocabularyFieldsToolStripMenuItem, "vocabularyFieldsToolStripMenuItem");
            this.vocabularyFieldsToolStripMenuItem.Click += new System.EventHandler(this.VocabularyFieldsToolStripMenuItem_Click);
            // 
            // epiInfoLogsToolStripMenuItem
            // 
            this.epiInfoLogsToolStripMenuItem.Name = "epiInfoLogsToolStripMenuItem";
            resources.ApplyResources(this.epiInfoLogsToolStripMenuItem, "epiInfoLogsToolStripMenuItem");
            this.epiInfoLogsToolStripMenuItem.Click += new System.EventHandler(this.epiInfoLogsToolStripMenuItem_Click);
            // 
            // insertToolStripMenuItem
            // 
            this.insertToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pageToolStripMenuItem1,
            this.groupToolStripMenuItem});
            this.insertToolStripMenuItem.Name = "insertToolStripMenuItem";
            resources.ApplyResources(this.insertToolStripMenuItem, "insertToolStripMenuItem");
            // 
            // pageToolStripMenuItem1
            // 
            this.pageToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.insertPageToolStripMenuItem,
            this.addPageToolStripMenuItem});
            resources.ApplyResources(this.pageToolStripMenuItem1, "pageToolStripMenuItem1");
            this.pageToolStripMenuItem1.Name = "pageToolStripMenuItem1";
            // 
            // insertPageToolStripMenuItem
            // 
            this.insertPageToolStripMenuItem.Name = "insertPageToolStripMenuItem";
            resources.ApplyResources(this.insertPageToolStripMenuItem, "insertPageToolStripMenuItem");
            this.insertPageToolStripMenuItem.Click += new System.EventHandler(this.insertPageToolStripMenuItem_Click);
            // 
            // addPageToolStripMenuItem
            // 
            this.addPageToolStripMenuItem.Name = "addPageToolStripMenuItem";
            resources.ApplyResources(this.addPageToolStripMenuItem, "addPageToolStripMenuItem");
            this.addPageToolStripMenuItem.Click += new System.EventHandler(this.NewPageMenuItem_Click);
            // 
            // groupToolStripMenuItem
            // 
            resources.ApplyResources(this.groupToolStripMenuItem, "groupToolStripMenuItem");
            this.groupToolStripMenuItem.Name = "groupToolStripMenuItem";
            this.groupToolStripMenuItem.Click += new System.EventHandler(this.groupToolStripMenuItem_Click);
            // 
            // forToolStripMenuItem
            // 
            this.forToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setDefaultFontToolStripMenuItem,
            this.setDefaultControlFontToolStripMenuItem,
            this.alignmentToolStripMenuItem,
            this.backgroundToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.pageSetupToolStripMenuItem});
            this.forToolStripMenuItem.Name = "forToolStripMenuItem";
            resources.ApplyResources(this.forToolStripMenuItem, "forToolStripMenuItem");
            // 
            // setDefaultFontToolStripMenuItem
            // 
            resources.ApplyResources(this.setDefaultFontToolStripMenuItem, "setDefaultFontToolStripMenuItem");
            this.setDefaultFontToolStripMenuItem.Name = "setDefaultFontToolStripMenuItem";
            this.setDefaultFontToolStripMenuItem.Click += new System.EventHandler(this.setDefaultFontToolStripMenuItem_Click);
            // 
            // setDefaultControlFontToolStripMenuItem
            // 
            resources.ApplyResources(this.setDefaultControlFontToolStripMenuItem, "setDefaultControlFontToolStripMenuItem");
            this.setDefaultControlFontToolStripMenuItem.Name = "setDefaultControlFontToolStripMenuItem";
            this.setDefaultControlFontToolStripMenuItem.Click += new System.EventHandler(this.setDefaultControlFontToolStripMenuItem_Click);
            // 
            // alignmentToolStripMenuItem
            // 
            this.alignmentToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.verticalToolStripMenuItem,
            this.horizontalToolStripMenuItem});
            resources.ApplyResources(this.alignmentToolStripMenuItem, "alignmentToolStripMenuItem");
            this.alignmentToolStripMenuItem.Name = "alignmentToolStripMenuItem";
            // 
            // verticalToolStripMenuItem
            // 
            this.verticalToolStripMenuItem.Name = "verticalToolStripMenuItem";
            resources.ApplyResources(this.verticalToolStripMenuItem, "verticalToolStripMenuItem");
            this.verticalToolStripMenuItem.Click += new System.EventHandler(this.verticalToolStripMenuItem_Click);
            // 
            // horizontalToolStripMenuItem
            // 
            this.horizontalToolStripMenuItem.Name = "horizontalToolStripMenuItem";
            resources.ApplyResources(this.horizontalToolStripMenuItem, "horizontalToolStripMenuItem");
            this.horizontalToolStripMenuItem.Click += new System.EventHandler(this.horizontalToolStripMenuItem_Click);
            // 
            // backgroundToolStripMenuItem
            // 
            resources.ApplyResources(this.backgroundToolStripMenuItem, "backgroundToolStripMenuItem");
            this.backgroundToolStripMenuItem.Name = "backgroundToolStripMenuItem";
            this.backgroundToolStripMenuItem.Click += new System.EventHandler(this.backgroundToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            resources.ApplyResources(this.settingsToolStripMenuItem, "settingsToolStripMenuItem");
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // pageSetupToolStripMenuItem
            // 
            resources.ApplyResources(this.pageSetupToolStripMenuItem, "pageSetupToolStripMenuItem");
            this.pageSetupToolStripMenuItem.Name = "pageSetupToolStripMenuItem";
            this.pageSetupToolStripMenuItem.Click += new System.EventHandler(this.pageSetupToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.displayDataDictionaryToolStripMenuItem,
            this.checkCodeToolStripMenuItem,
            this.toolStripSeparator7,
            this.importEpi6RecFileToolStripMenuItem,
            this.importCheckCodeToolStripMenuItem,
            this.makeViewFromDataTableToolStripMenuItem,
            this.upgradeToolStripMenuItem,
            this.makeProjectToolStripMenuItem,
            this.createDataTableToolStripMenuItem,
            this.deleteDataTableToolStripMenuItem,
            this.copyViewToolStripMenuItem,
            this.enterDataToolStripMenuItem,
            this.toolStripSeparator6,
            this.optionsToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            resources.ApplyResources(this.toolsToolStripMenuItem, "toolsToolStripMenuItem");
            // 
            // displayDataDictionaryToolStripMenuItem
            // 
            resources.ApplyResources(this.displayDataDictionaryToolStripMenuItem, "displayDataDictionaryToolStripMenuItem");
            this.displayDataDictionaryToolStripMenuItem.Name = "displayDataDictionaryToolStripMenuItem";
            this.displayDataDictionaryToolStripMenuItem.Click += new System.EventHandler(this.displayDataDictionaryToolStripMenuItem_Click);
            // 
            // checkCodeToolStripMenuItem
            // 
            resources.ApplyResources(this.checkCodeToolStripMenuItem, "checkCodeToolStripMenuItem");
            this.checkCodeToolStripMenuItem.Name = "checkCodeToolStripMenuItem";
            this.checkCodeToolStripMenuItem.Click += new System.EventHandler(this.checkCodeToolStripMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            resources.ApplyResources(this.toolStripSeparator7, "toolStripSeparator7");
            // 
            // importEpi6RecFileToolStripMenuItem
            // 
            resources.ApplyResources(this.importEpi6RecFileToolStripMenuItem, "importEpi6RecFileToolStripMenuItem");
            this.importEpi6RecFileToolStripMenuItem.Name = "importEpi6RecFileToolStripMenuItem";
            // 
            // importCheckCodeToolStripMenuItem
            // 
            resources.ApplyResources(this.importCheckCodeToolStripMenuItem, "importCheckCodeToolStripMenuItem");
            this.importCheckCodeToolStripMenuItem.Name = "importCheckCodeToolStripMenuItem";
            // 
            // makeViewFromDataTableToolStripMenuItem
            // 
            resources.ApplyResources(this.makeViewFromDataTableToolStripMenuItem, "makeViewFromDataTableToolStripMenuItem");
            this.makeViewFromDataTableToolStripMenuItem.Name = "makeViewFromDataTableToolStripMenuItem";
            this.makeViewFromDataTableToolStripMenuItem.Click += new System.EventHandler(this.makeViewFromDataTableToolStripMenuItem_Click);
            // 
            // upgradeToolStripMenuItem
            // 
            this.upgradeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Import35xToolStripMenuItem,
            this.Import6xToolStripMenuItem});
            this.upgradeToolStripMenuItem.Name = "upgradeToolStripMenuItem";
            resources.ApplyResources(this.upgradeToolStripMenuItem, "upgradeToolStripMenuItem");
            // 
            // Import35xToolStripMenuItem
            // 
            this.Import35xToolStripMenuItem.Name = "Import35xToolStripMenuItem";
            resources.ApplyResources(this.Import35xToolStripMenuItem, "Import35xToolStripMenuItem");
            this.Import35xToolStripMenuItem.Click += new System.EventHandler(this.Import35xToolStripMenuItem_Click);
            // 
            // Import6xToolStripMenuItem
            // 
            resources.ApplyResources(this.Import6xToolStripMenuItem, "Import6xToolStripMenuItem");
            this.Import6xToolStripMenuItem.Name = "Import6xToolStripMenuItem";
            // 
            // makeProjectToolStripMenuItem
            // 
            this.makeProjectToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.makeAccessPRJToolStripMenuItem,
            this.makeSQLPRJToolStripMenuItem});
            this.makeProjectToolStripMenuItem.Name = "makeProjectToolStripMenuItem";
            resources.ApplyResources(this.makeProjectToolStripMenuItem, "makeProjectToolStripMenuItem");
            // 
            // makeAccessPRJToolStripMenuItem
            // 
            this.makeAccessPRJToolStripMenuItem.Name = "makeAccessPRJToolStripMenuItem";
            resources.ApplyResources(this.makeAccessPRJToolStripMenuItem, "makeAccessPRJToolStripMenuItem");
            this.makeAccessPRJToolStripMenuItem.Click += new System.EventHandler(this.makeAccessProjectToolStripMenuItem_Click);
            // 
            // makeSQLPRJToolStripMenuItem
            // 
            this.makeSQLPRJToolStripMenuItem.Name = "makeSQLPRJToolStripMenuItem";
            resources.ApplyResources(this.makeSQLPRJToolStripMenuItem, "makeSQLPRJToolStripMenuItem");
            this.makeSQLPRJToolStripMenuItem.Click += new System.EventHandler(this.makeSQLProjectToolStripMenuItem_Click);
            // 
            // createDataTableToolStripMenuItem
            // 
            resources.ApplyResources(this.createDataTableToolStripMenuItem, "createDataTableToolStripMenuItem");
            this.createDataTableToolStripMenuItem.Name = "createDataTableToolStripMenuItem";
            this.createDataTableToolStripMenuItem.Click += new System.EventHandler(this.createDataTableToolStripMenuItem_Click);
            // 
            // deleteDataTableToolStripMenuItem
            // 
            resources.ApplyResources(this.deleteDataTableToolStripMenuItem, "deleteDataTableToolStripMenuItem");
            this.deleteDataTableToolStripMenuItem.Name = "deleteDataTableToolStripMenuItem";
            this.deleteDataTableToolStripMenuItem.Click += new System.EventHandler(this.deleteDataTableToolStripMenuItem_Click);
            // 
            // copyViewToolStripMenuItem
            // 
            resources.ApplyResources(this.copyViewToolStripMenuItem, "copyViewToolStripMenuItem");
            this.copyViewToolStripMenuItem.Name = "copyViewToolStripMenuItem";
            this.copyViewToolStripMenuItem.Click += new System.EventHandler(this.copyViewToolStripMenuItem_Click);
            // 
            // enterDataToolStripMenuItem
            // 
            resources.ApplyResources(this.enterDataToolStripMenuItem, "enterDataToolStripMenuItem");
            this.enterDataToolStripMenuItem.Name = "enterDataToolStripMenuItem";
            this.enterDataToolStripMenuItem.Click += new System.EventHandler(this.btnEnterData_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            resources.ApplyResources(this.toolStripSeparator6, "toolStripSeparator6");
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            resources.ApplyResources(this.optionsToolStripMenuItem, "optionsToolStripMenuItem");
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
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
            // MainToolStrip
            // 
            resources.ApplyResources(this.MainToolStrip, "MainToolStrip");
            this.MainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnNew,
            this.btnOpen,
            this.toolStripButtonCloseProject,
            this.toolStripSeparator2,
            this.toolStripButtonUndo,
            this.toolStripButtonRedo,
            this.toolStripSeparatorAfterUndoRedo,
            this.btnCheckCode,
            this.toolStripSeparator10,
            this.QuickPublishtoolStripButton,
            this.ChangeModetoolStripDropDownButton,
            this.toolStripSeparator11,
            this.btnEnterData});
            this.MainToolStrip.Name = "MainToolStrip";
            this.MainToolStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.MainToolStrip_ItemClicked);
            // 
            // btnNew
            // 
            resources.ApplyResources(this.btnNew, "btnNew");
            this.btnNew.Name = "btnNew";
            this.btnNew.Click += new System.EventHandler(this.NewProjectMenuItem_Click);
            // 
            // btnOpen
            // 
            resources.ApplyResources(this.btnOpen, "btnOpen");
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Click += new System.EventHandler(this.OpenProject_Click);
            // 
            // toolStripButtonCloseProject
            // 
            resources.ApplyResources(this.toolStripButtonCloseProject, "toolStripButtonCloseProject");
            this.toolStripButtonCloseProject.Name = "toolStripButtonCloseProject";
            this.toolStripButtonCloseProject.Click += new System.EventHandler(this.closeProjectToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // toolStripButtonUndo
            // 
            this.toolStripButtonUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.toolStripButtonUndo, "toolStripButtonUndo");
            this.toolStripButtonUndo.Name = "toolStripButtonUndo";
            this.toolStripButtonUndo.Click += new System.EventHandler(this.toolStripButtonUndo_Click);
            // 
            // toolStripButtonRedo
            // 
            this.toolStripButtonRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.toolStripButtonRedo, "toolStripButtonRedo");
            this.toolStripButtonRedo.Name = "toolStripButtonRedo";
            this.toolStripButtonRedo.Click += new System.EventHandler(this.toolStripButtonRedo_Click);
            // 
            // toolStripSeparatorAfterUndoRedo
            // 
            this.toolStripSeparatorAfterUndoRedo.Name = "toolStripSeparatorAfterUndoRedo";
            resources.ApplyResources(this.toolStripSeparatorAfterUndoRedo, "toolStripSeparatorAfterUndoRedo");
            // 
            // btnCheckCode
            // 
            resources.ApplyResources(this.btnCheckCode, "btnCheckCode");
            this.btnCheckCode.Name = "btnCheckCode";
            this.btnCheckCode.Click += new System.EventHandler(this.btnCheckCode_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            resources.ApplyResources(this.toolStripSeparator10, "toolStripSeparator10");
            // 
            // QuickPublishtoolStripButton
            // 
            this.QuickPublishtoolStripButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toWebSurveyToolStripMenuItem,
            this.toWebEnterToolStripMenuItem});
            resources.ApplyResources(this.QuickPublishtoolStripButton, "QuickPublishtoolStripButton");
            this.QuickPublishtoolStripButton.Image = global::Epi.Windows.MakeView.Properties.Resources.Publish;
            this.QuickPublishtoolStripButton.Name = "QuickPublishtoolStripButton";
            // 
            // toWebSurveyToolStripMenuItem
            // 
            this.toWebSurveyToolStripMenuItem.Name = "toWebSurveyToolStripMenuItem";
            resources.ApplyResources(this.toWebSurveyToolStripMenuItem, "toWebSurveyToolStripMenuItem");
            this.toWebSurveyToolStripMenuItem.Click += new System.EventHandler(this.toWebSurveyToolStripMenuItem_Click);
            // 
            // toWebEnterToolStripMenuItem
            // 
            this.toWebEnterToolStripMenuItem.Name = "toWebEnterToolStripMenuItem";
            resources.ApplyResources(this.toWebEnterToolStripMenuItem, "toWebEnterToolStripMenuItem");
            this.toWebEnterToolStripMenuItem.Click += new System.EventHandler(this.toWebEnterToolStripMenuItem_Click);
            // 
            // ChangeModetoolStripDropDownButton
            // 
            this.ChangeModetoolStripDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.ChangeModetoolStripDropDownButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.EIWSToolStripMenuItem,
            this.EWEToolStripMenuItem});
            resources.ApplyResources(this.ChangeModetoolStripDropDownButton, "ChangeModetoolStripDropDownButton");
            this.ChangeModetoolStripDropDownButton.Name = "ChangeModetoolStripDropDownButton";
            this.ChangeModetoolStripDropDownButton.Click += new System.EventHandler(this.ChangeModetoolStripDropDownButton_Click);
            // 
            // EIWSToolStripMenuItem
            // 
            this.EIWSToolStripMenuItem.CheckOnClick = true;
            this.EIWSToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.draftToolStripMenuItem1,
            this.finalToolStripMenuItem1});
            this.EIWSToolStripMenuItem.Name = "EIWSToolStripMenuItem";
            resources.ApplyResources(this.EIWSToolStripMenuItem, "EIWSToolStripMenuItem");
            this.EIWSToolStripMenuItem.Click += new System.EventHandler(this.draftToolStripMenuItem_Click);
            // 
            // draftToolStripMenuItem1
            // 
            this.draftToolStripMenuItem1.Name = "draftToolStripMenuItem1";
            resources.ApplyResources(this.draftToolStripMenuItem1, "draftToolStripMenuItem1");
            this.draftToolStripMenuItem1.Click += new System.EventHandler(this.draftToolStripMenuItem1_Click);
            // 
            // finalToolStripMenuItem1
            // 
            this.finalToolStripMenuItem1.Name = "finalToolStripMenuItem1";
            resources.ApplyResources(this.finalToolStripMenuItem1, "finalToolStripMenuItem1");
            this.finalToolStripMenuItem1.Click += new System.EventHandler(this.finalToolStripMenuItem1_Click);
            // 
            // EWEToolStripMenuItem
            // 
            this.EWEToolStripMenuItem.CheckOnClick = true;
            this.EWEToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.draftToolStripMenuItem2,
            this.finalToolStripMenuItem2});
            this.EWEToolStripMenuItem.Name = "EWEToolStripMenuItem";
            resources.ApplyResources(this.EWEToolStripMenuItem, "EWEToolStripMenuItem");
            this.EWEToolStripMenuItem.Click += new System.EventHandler(this.finalToolStripMenuItem_Click);
            // 
            // draftToolStripMenuItem2
            // 
            this.draftToolStripMenuItem2.Name = "draftToolStripMenuItem2";
            resources.ApplyResources(this.draftToolStripMenuItem2, "draftToolStripMenuItem2");
            this.draftToolStripMenuItem2.Click += new System.EventHandler(this.draftToolStripMenuItem2_Click);
            // 
            // finalToolStripMenuItem2
            // 
            this.finalToolStripMenuItem2.Name = "finalToolStripMenuItem2";
            resources.ApplyResources(this.finalToolStripMenuItem2, "finalToolStripMenuItem2");
            this.finalToolStripMenuItem2.Click += new System.EventHandler(this.finalToolStripMenuItem2_Click);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            resources.ApplyResources(this.toolStripSeparator11, "toolStripSeparator11");
            // 
            // btnEnterData
            // 
            resources.ApplyResources(this.btnEnterData, "btnEnterData");
            this.btnEnterData.Name = "btnEnterData";
            this.btnEnterData.Click += new System.EventHandler(this.btnEnterData_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
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
            // 
            // orderOfFieldEntryToolStripMenuItem
            // 
            resources.ApplyResources(this.orderOfFieldEntryToolStripMenuItem, "orderOfFieldEntryToolStripMenuItem");
            this.orderOfFieldEntryToolStripMenuItem.Name = "orderOfFieldEntryToolStripMenuItem";
            this.orderOfFieldEntryToolStripMenuItem.Click += new System.EventHandler(this.orderOfFieldEntryToolStripMenuItem_Click);
            // 
            // btnSave
            // 
            this.btnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.btnSave, "btnSave");
            this.btnSave.Name = "btnSave";
            // 
            // btnCut
            // 
            this.btnCut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.btnCut, "btnCut");
            this.btnCut.Name = "btnCut";
            // 
            // btnCopy
            // 
            this.btnCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.btnCopy, "btnCopy");
            this.btnCopy.Name = "btnCopy";
            // 
            // btnPaste
            // 
            this.btnPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.btnPaste, "btnPaste");
            this.btnPaste.Name = "btnPaste";
            // 
            // MakeViewMainForm
            // 
            this.AllowDrop = true;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.MainToolStripContainer);
            this.Name = "MakeViewMainForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Closing);
            this.Load += new System.EventHandler(this.Form_Load);
            this.Controls.SetChildIndex(this.MainToolStripContainer, 0);
            this.MainToolStripContainer.ContentPanel.ResumeLayout(false);
            this.MainToolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this.MainToolStripContainer.TopToolStripPanel.PerformLayout();
            this.MainToolStripContainer.ResumeLayout(false);
            this.MainToolStripContainer.PerformLayout();
            this.MainMenu.ResumeLayout(false);
            this.MainMenu.PerformLayout();
            this.MainToolStrip.ResumeLayout(false);
            this.MainToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        void toolStripButtonRedo_Click(object sender, EventArgs e)
        {
            mediator.Redo();
        }

        void toolStripButtonUndo_Click(object sender, EventArgs e)
        {
            mediator.Undo();
        }

        #endregion Designer Generated Code

        private bool canCommandBeGenerated;
        /// <summary>
        /// project explorer
        /// </summary>
        public ProjectExplorer projectExplorer;
        private Canvas canvas;
        /// <summary>
        /// mediator
        /// </summary>
        public PresentationLogic.GuiMediator mediator;
        private System.ComponentModel.IContainer components = null;
        private ToolStripContainer MainToolStripContainer;
        private MenuStrip MainMenu;
        private ToolStrip MainToolStrip;
        private Epi.Windows.Docking.DockManager dockManager1;
        private ToolStripButton btnNew;
        private ToolStripButton btnOpen;
        private ToolStripButton btnSave;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton btnCut;
        private ToolStripButton btnCopy;
        private ToolStripButton btnPaste;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton btnCheckCode;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openProjectToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripSeparator toolStripSeparatorAfterUndoRedo;
        private ToolStripMenuItem recentProjectsToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem exitMakeViewToolStripMenuItem;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem insertToolStripMenuItem;
        private ToolStripMenuItem forToolStripMenuItem;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem mnuEditCut;
        private ToolStripMenuItem mnuEditCopy;
        private ToolStripMenuItem mnuEditPaste;
        private ToolStripMenuItem deletePageToolStripMenuItem;
        private ToolStripMenuItem renamePageToolStripMenuItem;
        private ToolStripMenuItem orderOfFieldEntryToolStripMenuItem;
        private ToolStripMenuItem pageToolStripMenuItem1;
        private ToolStripMenuItem groupToolStripMenuItem;
        private ToolStripMenuItem setDefaultFontToolStripMenuItem;
        private ToolStripMenuItem alignmentToolStripMenuItem;
        private ToolStripMenuItem backgroundToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem pageSetupToolStripMenuItem;
        private ToolStripMenuItem displayDataDictionaryToolStripMenuItem;
        private ToolStripMenuItem importEpi6RecFileToolStripMenuItem;
        private ToolStripMenuItem importCheckCodeToolStripMenuItem;
        private ToolStripMenuItem makeViewFromDataTableToolStripMenuItem;
        private ToolStripMenuItem createDataTableToolStripMenuItem;
        private ToolStripMenuItem deleteDataTableToolStripMenuItem;
        private ToolStripMenuItem copyViewToolStripMenuItem;
        private ToolStripMenuItem enterDataToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripMenuItem optionsToolStripMenuItem;
        private ToolStripMenuItem insertPageToolStripMenuItem;
        private ToolStripMenuItem addPageToolStripMenuItem;
        private ToolStripMenuItem verticalToolStripMenuItem;
        private ToolStripMenuItem horizontalToolStripMenuItem;
        private ToolStripMenuItem contentsToolStripMenuItem;
        private ToolStripMenuItem aboutEpiInfoToolStripMenuItem;
        private ToolStripMenuItem closeProjectToolStripMenuItem;
        private ToolStripButton toolStripButtonCloseProject;
        private ToolStripMenuItem statusBarToolStripMenuItem;
        private ToolStripMenuItem epiInfoLogsToolStripMenuItem;
        private Font currentFont;
        private ToolStripMenuItem vocabularyFieldsToolStripMenuItem;
        private ToolStripMenuItem checkCodeToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripMenuItem undoToolStripMenuItem;
        private ToolStripMenuItem redoToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripMenuItem NewProjectMenuItem;
        private ToolStripMenuItem NewProjectFromTemplateMenuItem;
        private ToolStripMenuItem NewViewMenuItem;
        private ToolStripMenuItem NewPageMenuItem;
        private ToolStripSeparator toolStripSeparator9;
        private ToolStripMenuItem mnuCopyToPhone;
        private ToolStripMenuItem setDefaultControlFontToolStripMenuItem;
        private ToolStripButton toolStripButtonUndo;
        private ToolStripButton toolStripButtonRedo;
        private ToolStripMenuItem mnuPublishToWeb;
        private ToolStripMenuItem mnuImportTemplate;
        private ToolStripMenuItem makeProjectToolStripMenuItem;
        private ToolStripMenuItem makeAccessPRJToolStripMenuItem;
        private ToolStripMenuItem makeSQLPRJToolStripMenuItem;
        private ToolStripMenuItem upgradeToolStripMenuItem;
        private ToolStripMenuItem Import35xToolStripMenuItem;
        private ToolStripMenuItem Import6xToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripSeparator toolStripSeparator11;
        private ToolStripDropDownButton ChangeModetoolStripDropDownButton;
        private ToolStripMenuItem EIWSToolStripMenuItem;
        private ToolStripMenuItem EWEToolStripMenuItem;
        private ToolStripMenuItem toolStripPublishToWebEnter;
        private ToolStripDropDownButton QuickPublishtoolStripButton;
        private ToolStripMenuItem toWebSurveyToolStripMenuItem;
        private ToolStripMenuItem toWebEnterToolStripMenuItem;
        private ToolStripButton btnEnterData;
        private ToolStripMenuItem draftToolStripMenuItem1;
        private ToolStripMenuItem finalToolStripMenuItem1;
        private ToolStripMenuItem draftToolStripMenuItem2;
        private ToolStripMenuItem finalToolStripMenuItem2;

    }
}
