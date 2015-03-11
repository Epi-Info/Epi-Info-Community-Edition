#region Namespaces

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using Epi.Enter;
using Epi.Windows;
using Epi.Windows.Dialogs;
using Epi.Windows.Docking;
using Epi.Windows.Enter.PresentationLogic;

//using Epi.Core.Interpreter;
using EpiInfo.Plugin;
using Epi.EnterCheckCodeEngine;

//using mshtml;
using System.Net;


#endregion  //Namespaces

namespace Epi.Windows.Enter
{
    public partial class EnterMainForm : MainForm, IProjectHost
    {
        public event GuiMediator.OpenViewEventHandler OpenViewEvent;
        public event GuiMediator.CloseViewEventHandler CloseViewEvent;
        public event GuiMediator.GotoRecordEventHandler GotoRecordEvent;
        public event GuiMediator.SaveRecordEventHander SaveRecordEvent;
        public event EventHandler MarkAsDeletedRecordEvent;
        public event EventHandler UnMarkDeletedRecordEvent;
        public event EventHandler CloseFormEvent;
        public event SaveRecordEventHandler RecordSaved;

        /// <summary>
        /// Page Changed
        /// </summary>
        public event EventHandler PageChanged;

        /// <summary>
        /// Parent Record Id
        /// </summary>
        public int ParentRecordId = -1;
        /// <summary>
        /// Continue Change Record
        /// </summary>
        
        private bool continueChangeRecord = true;
        private Image currentBackgroundImage;
        private string currentBackgroundImageLayout;
        private Color currentBackgroundColor;
        private DataTable bgTable;
        private RunTimeView runTimeView;
        private Configuration config;
        private ProjectPermissions permissions;
        private EnterUIConfig UIConfig { get; set; }
        private bool canInsertRecords = true;
        private bool canUpdateRecords = true;
        private bool canSelectRecords = true;
        private bool allowOnlyRecordOnly = false;
       
        private bool AllowOneRecordOnly
        {
            get
            {
                return this.allowOnlyRecordOnly;
            }
            set
            {
                this.allowOnlyRecordOnly = value;
                if (mediator != null)
                {
                    this.mediator.AllowOneRecordOnly = AllowOneRecordOnly;
                }
            }
        }

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        //[Obsolete("Use of default constructor not allowed", true)]
        public EnterMainForm()
        {
            config = Configuration.GetNewInstance();
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(config.Settings.Language);
            InitializeComponent();
            Construct(true);
        }

        public EnterMainForm(Project initialProject, View initialForm, bool showLinkedRecordsViewer, bool allowOneRecordOnly)
        {
            config = Configuration.GetNewInstance();
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(config.Settings.Language);
            InitializeComponent();
            Construct(showLinkedRecordsViewer);
            AllowOneRecordOnly = allowOneRecordOnly;
            LoadInitialForm(initialProject, initialForm);
        }

        public EnterMainForm(Project initialProject, View initialForm, EnterUIConfig uiConfig)
        {
            config = Configuration.GetNewInstance();
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(config.Settings.Language);
            InitializeComponent();
            UIConfig = uiConfig;
            Construct(UIConfig.ShowLinkedRecordsViewer);
            LoadInitialForm(initialProject, initialForm);
        }

        /// <summary>
        /// Constructor for Enter main form
        /// </summary>
        public EnterMainForm(EnterWindowsModule mod)
            : base(mod)
        {
            config = Configuration.GetNewInstance();
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(config.Settings.Language);
            InitializeComponent();
            Construct();
        }

        public bool ShowMenuStrip
        {
            get
            {
                return menuStrip1.Visible;
            }
            set
            {
                menuStrip1.Visible = value;
            }
        }

        public bool ShowToolStrip
        {
            get
            {
                return toolStrip1.Visible;
            }
            set
            {
                toolStrip1.Visible = value;
            }
        }

        private void LoadInitialForm(Project initialProject, View initialForm)
        {
                this.CurrentProject = initialProject;
                this.View = initialForm;
                FireOpenViewEvent(initialForm, "*");            
        }

        /// <summary>
        /// Construct Main Form
        /// </summary>
        private void Construct(bool showLinkedRecordsViewer = true)
        {
            DockManager.FastMoveDraw = false;
            DockManager.Style = DockVisualStyle.VS2005;

            viewExplorer = new ViewExplorer(this);
            //viewExplorer.PageSelected += new ViewExplorerPageSelectedEventHandler(viewExplorer_PageSelected);
            //viewExplorer.GoToFirstRecordSelected += new ViewExplorerGoToFirstRecordSelectedEventHandler(viewExplorer_GoToFirstRecord);
            //viewExplorer.GoToPreviousRecordSelected += new ViewExplorerGoToPreviousRecordSelectedEventHandler(viewExplorer_GoToPreviousRecord);
            //viewExplorer.GoToNextRecordSelected += new ViewExplorerGoToNextRecordSelectedEventHandler(viewExplorer_GoToNextRecord);
            //viewExplorer.GoToLastRecordSelected += new ViewExplorerGoToLastRecordSelectedEventHandler(viewExplorer_GoToLastRecord);
            //viewExplorer.GoToSpecifiedRecord += new ViewExplorerLoadRecordSpecifiedEventHandler(viewExplorer_GoToSpecifiedRecord);
            //viewExplorer.RunCheckCode += new ViewExplorerRunCheckCodeEventHandler(viewExplorer_RunCheckCode);
            this.toolStripRecordNumber.TextBox.PreviewKeyDown += new PreviewKeyDownEventHandler(toolStripPageNumber_PreviewKeyDown);

            viewExplorer.AllowClose = false;
            viewExplorer.AllowUnDock = false;
            canvas = new Canvas(this);
            canvas.AllowClose = false;
            canvas.AllowUnDock = false;

            LinkedRecordsViewer linkedRecordsViewer = new LinkedRecordsViewer(this);
            linkedRecordsViewer.AllowClose = false;
            linkedRecordsViewer.AllowUnDock = false;

            this.mediator = GuiMediator.Instance;
            this.mediator.Subscribe(this, viewExplorer, canvas, linkedRecordsViewer);
            this.mediator.RecordSaved += new EventHandler(mediator_RecordSaved);

            //this.mediator.AllowOneRecordOnly = AllowOneRecordOnly;
 
            //// This function docks a window directly to the container.
            enterDockManager.DockWindow(canvas, DockStyle.Fill);
            enterDockManager.DockWindow(viewExplorer, DockStyle.Left);
            if (showLinkedRecordsViewer)
            {
                viewExplorer.HostContainer.DockWindow(linkedRecordsViewer, DockStyle.Bottom);
            }
            canvas.HostContainer.SelectTab(0);

            FormName = SharedStrings.ENTER;

            this.WriteGC(viewExplorer);
            //InitializeMediator();

            //this.EpiInterpreter = new EpiInterpreterParser(Epi.Resources.ResourceLoader.GetEnterCompiledGrammarTable(), (IEnterCheckCode)mediator, Rule_Context.eRunMode.Enter);
            //this.EpiInterpreter = new EpiInterpreterParser((IEnterInterpreterHost)mediator);            

#if LINUX_BUILD
            excelLineListMenuItem.Visible = false;
#else
            excelLineListMenuItem.Visible = true;
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.ClassesRoot; Microsoft.Win32.RegistryKey excelKey = key.OpenSubKey("Excel.Application"); bool excelInstalled = excelKey == null ? false : true; key = Microsoft.Win32.Registry.ClassesRoot;
            excelKey = key.OpenSubKey("Excel.Application");
            excelInstalled = excelKey == null ? false : true;

            if (!excelInstalled)
            {
                excelLineListMenuItem.Visible = false;
            }
#endif
        }

        void mediator_RecordSaved(object sender, EventArgs e)
        {
            if (RecordSaved != null)
            {
                SaveRecordEventArgs args = new SaveRecordEventArgs(this.view, this.View.CurrentGlobalRecordId, "");
                RecordSaved(this.View.CurrentGlobalRecordId, args);
            }
        }

        #endregion  //Constructors

        #region Private Event Handlers

        public void ChangeBackgroundData(Page page)
        {
            int color;

            if (page != null)
            {
                this.bgTable = page.GetMetadata().GetPageBackgroundData(page);

                DataRow[] rows = bgTable.Select("BackgroundId = " + page.BackgroundId);
                if (rows.Length > 0)
                {
                    color = Convert.ToInt32(rows[0]["Color"]);
                    if (rows[0]["Image"] != System.DBNull.Value)
                    {
                        try
                        {
                            byte[] imageBytes = ((byte[])rows[0]["Image"]);

                            using (MemoryStream memStream = new MemoryStream(imageBytes.Length))
                            {
                                memStream.Seek(0, SeekOrigin.Begin);
                                memStream.Write(imageBytes, 0, imageBytes.Length);
                                memStream.Seek(0, SeekOrigin.Begin);

                                this.currentBackgroundImage = Image.FromStream(((Stream)memStream));
                            }
                        }
                        catch
                        {
                            this.currentBackgroundImage = null;
                        }
                    }
                    else
                    {
                        this.currentBackgroundImage = null;
                    }

                    this.currentBackgroundImageLayout = Convert.ToString(rows[0]["ImageLayout"]);
                    
                    if (color == 0)
                    {
                        this.currentBackgroundColor = SystemColors.Window;
                    }
                    else
                    {
                        this.currentBackgroundColor = Color.FromArgb(color);
                    }
                }
                else
                {
                    this.currentBackgroundImage = null;
                    this.currentBackgroundImageLayout = "None";
                    this.currentBackgroundColor = SystemColors.Window;
                }
            }
            else
            {
                this.currentBackgroundImage = null;
                this.currentBackgroundImageLayout = "None";
                this.currentBackgroundColor = SystemColors.Window;
            }
        }

        /// <summary>
        /// Handles the OnClosing event for the Enter main menu.
        /// </summary>
        /// <param name="e">.NET supplied event parameters</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            if (mediator.IsDirty)
            {
                DialogResult result = MsgBox.Show(SharedStrings.SAVE_CHANGES_TO_RECORD, SharedStrings.ENTER, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    bool shouldCheckRequiredFields = true;
                    if (!ContinueChangeRecord)
                    {
                        isClosed = 0;
                        e.Cancel = true;
                        MsgBox.ShowWarning(SharedStrings.CANNOT_CLOSE_RECORD_INVALID_DATA);
                        return;
                    }

                    if (this != null && this.View != null)
                    {
                        mediator.SetFieldData();
                        if (View.IsEmptyNewRecord() == false)
                        {
                            if (shouldCheckRequiredFields == true && !mediator.CheckViewRequiredFields())
                            {
                                isClosed = 0;
                                e.Cancel = true;
                                return;
                            }
                        }
                    }

                    try
                    {
                        this.SaveRecordEvent(this, e);
                    }
                    catch (Exception ex)
                    {
                        MsgBox.ShowException(ex);
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            isClosed = 1;

            if (this.CloseFormEvent != null)
            {
                this.CloseFormEvent(this, e);
            }

            if (this.Module != null)
            {
                this.Module.SetAllReferencesToNull();
                this.Module.Dispose();
            }
            
            this.Module = null;
            this.Dispose(true);
            base.OnClosing(e);
            
        }
        /// <summary>
        /// Handles the Click event of the Compact Database menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void compactDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            base.DisplayFeatureNotImplementedMessage();
        }

        /// <summary>
        /// Handles the Click event of the Options menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnOptionsClicked();

            this.mediator.SetFieldData();
            this.mediator.Canvas.UpdateSettings();
            this.mediator.ShowFieldDataOnForm();
        }

        /// <summary>
        /// Handles the Click event of the Find Records menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Epi.Windows.Enter.Dialogs.FindRecords findrecords = new Epi.Windows.Enter.Dialogs.FindRecords(view, this);
            DialogResult result = findrecords.ShowDialog();
            this.BringToFront();

            if (result == DialogResult.OK && IsRecordCloseable)
            {
                if (this.GotoRecordEvent != null)
                {
                    this.GotoRecordEvent(this, new GoToRecordEventArgs(findrecords.RecordId));
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the Exit menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the Click event of the Contents menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void contentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://wwwn.cdc.gov/epiinfo/user-guide/enter-data/introduction.html");
        }

        /// <summary>
        /// Handles the Click event of the Command Reference menu item        
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void commandReferenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            base.DisplayFeatureNotImplementedMessage();
        }

        /// <summary>
        /// Handles the Click event of the About Epi Info menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void aboutEpiInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnAboutClicked();
        }

        /// <summary>
        /// Handles the Click event of the New Record button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btiNew_Click(object sender, EventArgs e)
        {
            if (IsRecordCloseable)
            {
                GotoNewRecord();
            }            
        }

        /// <summary>
        /// Handles the Click event of the Open View menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void openViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowViewSelection();
        }

        /// <summary>
        /// Handles the Click event of the Close View menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void closeViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.View != null)
            {
                if (IsRecordCloseable == false)
                {
                    return;
                }

                if (CloseView() == false)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the New Record menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void newRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (IsRecordCloseable)
            {
                GotoNewRecord();
            }
        }

        /// <summary>
        /// The load method for Enter
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void Enter_Load(object sender, EventArgs e)
        {
            LoadRecentViews();
            statusBarToolStripMenuItem.Checked = config.Settings.ShowStatusBar;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (mediator != null)
            {
                bool enabled = true;
                
                if (this.view != null && !string.IsNullOrEmpty(this.view.RecStatusField.CurrentRecordValueString))
                {
                    enabled = viewExplorer.GetRecStatus(int.Parse(this.view.RecStatusField.CurrentRecordValueString));
                }

                mediator.Canvas.Render(enabled);
            }
        }

        /// <summary>
        /// Handles the MouseWheel event for the main form; primarily done to solve a problem related to not being able to scroll inside of the canvas when the main form has focus
        /// </summary>
        /// <param name="e">.NET supplied event parameters</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            mediator.FocusOnCanvasDesigner();
        }

        /// <summary>
        /// Handles the click event of a recent view menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void recentView_Click(object sender, EventArgs e)
        {
            try
            {
                string currentViewName = string.Empty;
                string currentProject = string.Empty;

                //The sender is the recentView menu item selected
                System.Windows.Forms.ToolStripMenuItem recentView = (System.Windows.Forms.ToolStripMenuItem)sender;

                //Grab the substring of the full path view name because all of the
                //recent views are preceded by a number and a space
                string filePath = recentView.Text.Substring(3, recentView.Text.LastIndexOf(":") - 3);
                bool shouldCheckRequiredFields = true;

                if (this.view != null)
                {
                    if (!ContinueChangeRecord)
                    {
                        MsgBox.ShowWarning(SharedStrings.CANNOT_CLOSE_RECORD_INVALID_DATA);
                        return;
                    }

                    mediator.SetFieldData();
                    if (View.IsEmptyNewRecord() == false)
                    {
                        if (shouldCheckRequiredFields == true && !mediator.CheckViewRequiredFields())
                        {
                            return;
                        }
                    }

                    currentViewName = this.view.Name;
                    currentProject = this.view.GetProject().FullName;

                    if (recentView.Text.Substring(recentView.Text.LastIndexOf(":") + 1) != currentViewName && currentProject.ToLower() != recentView.Text.ToLower())
                    {
                        if (!CloseView())
                        {
                            return;
                        }
                    }
                }

                try
                {
                    //this.CurrentProject = new Project(filePath, true); dpb
                    this.CurrentProject = new Project(filePath);
                }
                catch (Exception ex)
                {                    
                    MsgBox.ShowError(SharedStrings.ERROR_LOADING_PROJECT, ex);
                    return;
                }
                // Construct a new view object so that we are not
                // referencing the same object in both Make View and Enter.
                Epi.View NewView = CurrentProject.Metadata.GetViewByFullName(recentView.Text);
                if (NewView == null)
                {
                    string viewName = string.Empty;
                    if (recentView.Text.Length > 3)
                    {
                        viewName = recentView.Text.Remove(0, 3); // get rid of the first few characters, which is comprised of a number and an & symbol.
                    }
                    else
                    {
                        viewName = recentView.Text;
                    }

                    MsgBox.ShowError(string.Format(SharedStrings.ERROR_LOADING_VIEW, viewName));
                }
                else if (!NewView.IsRelatedView)
                {
                    if (this.OpenViewEvent != null)
                    {
                        this.View = NewView;                        
                        this.OpenViewEvent(this, new Epi.Windows.Enter.PresentationLogic.OpenViewEventArgs(NewView));
                    }
                }
                else
                {
                    MsgBox.ShowInformation(SharedStrings.CANNOT_OPEN_VIEW_RELATED);
                }

            }
            catch (FileNotFoundException ex)
            {
                MsgBox.ShowException(ex);
            }
            finally
            {
            }
        }
        int isClosed = 0;

        /// <summary>
        /// Loads view for editing in Make View
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void editViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool shouldCheckRequiredFields = true;
            // Check to see if the record is dirty. If so, prompt the user to save changes.
            if (mediator.IsDirty)
            {
                DialogResult result = MsgBox.Show(SharedStrings.SAVE_CHANGES_TO_RECORD, SharedStrings.ENTER, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    if (!ContinueChangeRecord)
                    {
                        isClosed = 0;
                        MsgBox.ShowWarning(SharedStrings.CANNOT_CLOSE_RECORD_INVALID_DATA);
                        return;
                    }

                    if (this != null && this.View != null)
                    {
                        mediator.SetFieldData();
                        if (View.IsEmptyNewRecord() == false)
                        {
                            if (shouldCheckRequiredFields && !mediator.CheckViewRequiredFields())
                            {
                                isClosed = 0;
                                return;
                            }
                        }
                    }

                    try
                    {
                        this.SaveRecordEvent(this, e);
                    }
                    catch (Exception ex)
                    {
                        MsgBox.ShowException(ex);
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
                else if (result == DialogResult.No)
                {
                    continueChangeRecord = true;
                    shouldCheckRequiredFields = false;
                }
            }

            if (System.Threading.Interlocked.CompareExchange(ref isClosed, 1, 0) > 0) return;

            if (this != null && this.view != null)
            {
                mediator.LoadMakeView();
                this.Dispose();
            }
        }

        /// <summary>
        /// Handles the Click event of the Save menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SaveRecordEvent != null && IsRecordCloseable)
            {
                this.SaveRecordEvent(this, e);
                UpdateRecordCount();
                EnableDisableCurrentRecord(true);
            }
        }

        /// <summary>
        /// Handles the Click event to navigate to the first record of a view
        /// </summary>
        /// <param name="sender">Object that fires the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void toolStripFirstButton_Click(object sender, EventArgs e)
        {
            if (GotoRecordEvent != null)
            {
                if (IsRecordCloseable)
                {
                    GotoRecordEvent(this, new GoToRecordEventArgs("<<"));
                    EnableDisableNavigationButtons();
                }
            }
         }

        /// <summary>
        /// Handles the Click event to navigate to the previous record of a view
        /// </summary>
        /// <param name="sender">Object that fires the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void toolStripPreviousButton_Click(object sender, EventArgs e)
        {
            if (GotoRecordEvent != null)
            {
                if (IsRecordCloseable)
                {
                    GotoRecordEvent(this, new GoToRecordEventArgs("<"));
                    EnableDisableNavigationButtons();
                }
            }
        }

        /// <summary>
        /// Handles the Click event to navigate to the next record of a view
        /// </summary>
        /// <param name="sender">Object that fires the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void toolStripNextButton_Click(object sender, EventArgs e)
        {
            if (GotoRecordEvent != null)
            {
                if (IsRecordCloseable)
                {
                    GotoRecordEvent(this, new GoToRecordEventArgs(">"));
                    EnableDisableNavigationButtons();
                }
            }
        }

        /// <summary>
        /// Handles the Click event to navigate to the last record of a view
        /// </summary>
        /// <param name="sender">Object that fires the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void toolStripLastButton_Click(object sender, EventArgs e)
        {
            if (GotoRecordEvent != null)
            {
                if (IsRecordCloseable)
                {
                    GotoRecordEvent(this, new GoToRecordEventArgs(">>"));
                    EnableDisableNavigationButtons();
                }
            }
        }

        /// <summary>
        /// Handles the Click event to mark a record as deleted
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void markAsDeletedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.MarkAsDeletedRecordEvent != null)
            {
                this.MarkAsDeletedRecordEvent(this, new EventArgs());                
            }
        }

        /// <summary>
        /// Handles the Click event of the undelete record menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void undeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.UnMarkDeletedRecordEvent != null)
            {
                this.UnMarkDeletedRecordEvent(this, new EventArgs());                
            }
        }

        /// <summary>
        /// Sets the focus to the menu strip if a menu item is selected
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET provided event parameters</param>
        private void menuStrip1_Click(object sender, EventArgs e)
        {
            menuStrip1.Focus();
        }

        /// <summary>
        /// Closes Enter module
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">Form closing event parameters</param>
        private void Enter_FormClosing(object sender, FormClosingEventArgs e)
        {
            //try
            //{
            //    if (this.SaveRecordEvent != null)
            //    {
            //        this.SaveRecordEvent(this, e);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MsgBox.ShowException(ex);
            //}
        }

       
        /// <summary>
        /// Sets the focus to the item clicked on the tool strip to lose focus from the canvas
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied tool strip click event parameters</param>
        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ((Control)sender).Focus();
        }

        /// <summary>
        /// Sets the focus to the menu item click on the menu strip to lose focus from the canvas
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied menu strip event parameters</param>
        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            ((Control)sender).Focus();
        }

        /// <summary>
        /// Loads the specified record, if it exists
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied key event arguments</param>
        private void toolStripRecordNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Tab || e.KeyCode == Keys.Enter)
            {
                if (!String.IsNullOrEmpty(toolStripRecordNumber.Text))
                {
                    //if (int.Parse(toolStripPageNumber.Text) <= this.view.GetRecordCount() && int.Parse(toolStripPageNumber.Text) != 0)
                    if (!toolStripRecordNumber.Text.Contains("*") && int.Parse(toolStripRecordNumber.Text) != 0)
                    {
                        //mediator.LoadSpecificRecord(toolStripPageNumber.Text.Trim());
                        if (GotoRecordEvent != null)
                        {
                            GotoRecordEvent(this, new GoToRecordEventArgs(toolStripRecordNumber.Text.Trim()));
                            EnableDisableNavigationButtons();
                        }
                    }
                    else
                    {
                        //mediator.LastRecordSelected();
                        if (GotoRecordEvent != null)
                        {
                            GotoRecordEvent(this, new GoToRecordEventArgs(">>"));
                            EnableDisableNavigationButtons();
                        }
                        
                    }
                }
                else
                {
                    toolStripRecordNumber.Text = this.view.CurrentRecordId.ToString();
                }
            }
        }

        /// <summary>
        /// Loads the specified record, if it exists
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied key event arguments</param>
        private void toolStripPageNumber_Leave(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(toolStripRecordNumber.Text))
            {
                //if (int.Parse(toolStripPageNumber.Text) <= this.view.GetRecordCount() && int.Parse(toolStripPageNumber.Text) != 0)
                if (!toolStripRecordNumber.Text.Contains("*") && int.Parse(toolStripRecordNumber.Text) != 0)
                {
                    //mediator.LoadSpecificRecord(toolStripPageNumber.Text.Trim());
                    if (GotoRecordEvent != null)
                    {
                        GotoRecordEvent(this, new GoToRecordEventArgs(toolStripRecordNumber.Text.Trim()));
                        EnableDisableNavigationButtons();
                    }
                }
                else
                {
                    //mediator.LastRecordSelected();
                    if (GotoRecordEvent != null)
                    {
                        GotoRecordEvent(this, new GoToRecordEventArgs(">>"));
                        EnableDisableNavigationButtons();
                    }
                }
            }
            else
            {
                toolStripRecordNumber.Text = this.view.CurrentRecordId.ToString();
            }
        }

        /// <summary>
        /// Previews the key pressed to see if is an acceptable input key
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void toolStripPageNumber_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
            {
                if (UserKeyboardInput.IsInputKey(e.KeyData))
                {
                    e.IsInputKey = true;
                }
            }
        }

        /// <summary>
        /// Navigate to the "home" view.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnHome_Click(object sender, EventArgs e)
        {
            if (this.CloseViewEvent != null)
            {
                Validate();
                bool returnHome = true;
                this.CloseViewEvent(returnHome, new EventArgs());
            }
        }

        /// <summary>
        /// Navigate to the parent view of the related view.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnBack_Click(object sender, EventArgs e)
        {
            if (this.CloseViewEvent != null)
            {
                Validate();
                bool returnHome = false;
                this.CloseViewEvent(returnHome, new EventArgs());
            }
        }
        #endregion  //Private Event Handlers

        #region Protected Methods
        /// <summary>
        /// Sets the window title.
        /// </summary>
        protected override void SetWindowTitle()
        {
            if (this.view != null)
            {
                base.SetWindowTitle(this.view.DisplayName);
            }
            else
            {
                base.SetWindowTitle();
            }
        }
        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Enables or disables the record navigation buttons
        /// </summary>
        private void EnableDisableNavigationButtons()
        {
            if (!string.IsNullOrEmpty(toolStripRecordNumber.Text) && !toolStripRecordNumber.Text.Contains("*"))
            {
                if (int.Parse(toolStripRecordNumber.Text) == 1)
                {
                    toolStripPreviousButton.Enabled = false;
                    toolStripFirstButton.Enabled = false;
                }
                else if (int.Parse(toolStripRecordNumber.Text) == int.Parse(toolStripRecordCount.Text))
                {
                    toolStripNextButton.Enabled = false;
                    toolStripLastButton.Enabled = false;
                }
                else
                {
                    toolStripPreviousButton.Enabled = true;
                    toolStripFirstButton.Enabled = true;
                    toolStripNextButton.Enabled = true;
                    toolStripLastButton.Enabled = true;
                }
            }
            else if (toolStripRecordNumber.Text.Contains("*"))
            {
                toolStripNextButton.Enabled = false;
                toolStripLastButton.Enabled = false;
            }

            if (int.Parse(toolStripRecordCount.Text) <= 0)
            {
                toolStripPreviousButton.Enabled = false;
                toolStripFirstButton.Enabled = false;
                toolStripNextButton.Enabled = false;
                toolStripLastButton.Enabled = false;
            }

            if (this.View.ReturnToParent == true && this.View.IsRelatedView == true)
            {
                toolStripPreviousButton.Enabled = false;
                toolStripFirstButton.Enabled = false;
                toolStripNextButton.Enabled = false;
                toolStripLastButton.Enabled = false;
                newRecordToolStripMenuItem.Enabled = false;
                btiNew.Enabled = false;
            }
            else if (this.View != null)
            {
                newRecordToolStripMenuItem.Enabled = true;
                btiNew.Enabled = true;
            }

            if (!canInsertRecords)
            {
                newRecordToolStripMenuItem.Enabled = false;
                btiNew.Enabled = false;
            }
        }

        /// <summary>
        /// Goes to a new record.
        /// </summary>
        private void GotoNewRecord()
        {
            if (this.GotoRecordEvent != null && canInsertRecords)
            {
              //--2101 
              //  this.GotoRecordEvent(this, new GoToRecordEventArgs("+"));
               this.GotoRecordEvent(this, new GoToRecordEventArgs(Constants.Plus));

            }
        }

        /// <summary>
        /// Updates the record count in the navigation subsection
        /// </summary>
        private void UpdateRecordCount()
        {
            toolStripRecordCount.Text = this.view.GetRecordCount().ToString();
            string currentRecord = this.runTimeView.CurrentRecordNumber.ToString();
            if (currentRecord.Equals("0"))
            {
                toolStripRecordNumber.Text = "*";
                EnableDisableNavigationButtons();
            }
            else if (currentRecord.Equals("-1")) // special case when user creates a new record and then saves it manually.
            {
                this.RunTimeView.UpdateCurrentRecordNumber();
                toolStripRecordNumber.Text = this.runTimeView.CurrentRecordNumber.ToString();
            }
            else
            {
                toolStripRecordNumber.Text = currentRecord;
                EnableDisableNavigationButtons();
            }
        }

        /// <summary>
        /// Loads recent viewed opened or created by user
        /// </summary>
        private void LoadRecentViews()
        {
            DataView dv = config.RecentViews.DefaultView;
            try
            {
                dv.Sort = "LastAccessed desc";
                recentViewsToolStripMenuItem.DropDownItems.Clear();

                int minRecentView = Math.Min(dv.Count, config.Settings.MRUViewsCount);
                for (int index = 0; index < minRecentView; index++)
                {
                    string viewFullPath = "&" + (index + 1) + " " + dv[index][ColumnNames.NAME].ToString();

                    System.Windows.Forms.ToolStripMenuItem recentViewItem = new System.Windows.Forms.ToolStripMenuItem(viewFullPath);
                    recentViewItem.Click += new EventHandler(recentView_Click);
                    recentViewsToolStripMenuItem.DropDownItems.Add(recentViewItem);
                }
                recentViewsToolStripMenuItem.Enabled = (recentViewsToolStripMenuItem.DropDownItems.Count > 0);
            }
            finally
            {
                dv.Sort = string.Empty;
            }
        }

        /// <summary>
        /// Loads a view from a command line
        /// </summary>
        private void LoadViewFromCommandLine()
        {
            try
            {
                ICommandLine commandLine = (ICommandLine)Module.GetService(typeof(ICommandLine));
                if (commandLine != null)
                {
                    string projectPath = commandLine.GetArgument("project");
                    if (projectPath != null)
                    {
                        Project project;
                        try
                        {
                            IProjectManager manager = Module.GetService(typeof(IProjectManager)) as IProjectManager;
                            if (manager == null)
                            {
                                throw new GeneralException("Project manager is not registered.");
                            }

                            project = manager.OpenProject(projectPath);
                        }
                        catch (Exception ex)
                        {
                            MsgBox.ShowError(SharedStrings.ERROR_LOADING_PROJECT, ex);
                            return;
                        }

                        CurrentProject = project;
                    }

                    string viewName = commandLine.GetArgument("view");
                    if (!string.IsNullOrEmpty(viewName))
                    {
                        this.view = CurrentProject.Metadata.GetViewByFullName(viewName);
                    }

                    if (this.view != null)
                    {
                        if (this.OpenViewEvent != null)
                        {
                            this.OpenViewEvent(this, new Epi.Windows.Enter.PresentationLogic.OpenViewEventArgs(this.view));
                        }
                    }
                }
            }
            finally
            {
            }
        }

        /// <summary>
        /// Loads a view from a command line
        /// </summary>
        public void LoadViewFromCommandLine(string projectPath, string viewName)
        {
            try
            {
                if (projectPath != null)
                {
                    Project project;
                    try
                    {
                        IProjectManager manager = Module.GetService(typeof(IProjectManager)) as IProjectManager;
                        if (manager == null)
                        {
                            throw new GeneralException("Project manager is not registered.");
                        }

                        project = manager.OpenProject(projectPath);
                    }
                    catch (Exception ex)
                    {
                        MsgBox.ShowError(SharedStrings.ERROR_LOADING_PROJECT, ex);
                        return;
                    }

                    CurrentProject = project;
                }

                if (!string.IsNullOrEmpty(viewName))
                {
                    this.view = CurrentProject.Metadata.GetViewByFullName(viewName);
                }

                if (this.view != null)
                {
                    if (this.OpenViewEvent != null)
                    {
                        this.OpenViewEvent(this, new Epi.Windows.Enter.PresentationLogic.OpenViewEventArgs(this.view));
                    }
                }
            }
            finally
            {
            }
        }

        /// <summary>
        /// Enables/Disables buttons to navigate through records
        /// </summary>        
        private void EnableRecordNavigationButtons(bool enable)
        {
            toolStripFirstButton.Enabled = enable;
            toolStripPreviousButton.Enabled = enable;
            toolStripRecordNumber.Enabled = enable;
            toolStripNextButton.Enabled = enable;
            toolStripLastButton.Enabled = enable;
            printableHTMLToolStripMenuItem.Enabled = enable;
            gridToolStripMenuItem.Enabled = enable;
            excelLineListMenuItem.Enabled = enable;
            LineListingToolStripButton.Enabled = enable;
            tsbDashboard.Enabled = enable;
            tsbMap.Enabled = enable;
        }

        private void ConfigureInterface()
        {
            if (UIConfig != null)
            {
                foreach (KeyValuePair<View, bool> kvp in UIConfig.AllowOneRecordOnly)
                {
                    if (kvp.Key.Name == this.view.Name || kvp.Key == this.view)
                    {
                        AllowOneRecordOnly = kvp.Value;
                        break;
                    }
                }

                foreach (KeyValuePair<View, bool> kvp in UIConfig.ShowDashboardButton)
                {
                    if (kvp.Key.Name == this.view.Name || kvp.Key == this.view)
                    {
                        tsbDashboard.Visible = kvp.Value;
                        break;
                    }
                }

                foreach (KeyValuePair<View, bool> kvp in UIConfig.ShowDeleteButtons)
                {
                    if (kvp.Key.Name == this.view.Name || kvp.Key == this.view)
                    {
                        btiMarkDeleted.Visible = kvp.Value;
                        btiUndelete.Visible = kvp.Value;
                        break;
                    }
                }

                foreach (KeyValuePair<View, bool> kvp in UIConfig.ShowEditFormButton)
                {
                    if (kvp.Key.Name == this.view.Name || kvp.Key == this.view)
                    {
                        btiEditView.Visible = kvp.Value;
                        break;
                    }
                }

                foreach (KeyValuePair<View, bool> kvp in UIConfig.ShowFileMenu)
                {
                    if (kvp.Key.Name == this.view.Name || kvp.Key == this.view)
                    {
                        menuStrip1.Visible = kvp.Value;
                        break;
                    }
                }

                foreach (KeyValuePair<View, bool> kvp in UIConfig.ShowFindButton)
                {
                    if (kvp.Key.Name == this.view.Name || kvp.Key == this.view)
                    {
                        btiFind.Visible = kvp.Value;
                        break;
                    }
                }

                foreach (KeyValuePair<View, bool> kvp in UIConfig.ShowLineListButton)
                {
                    if (kvp.Key.Name == this.view.Name || kvp.Key == this.view)
                    {
                        LineListingToolStripButton.Visible = kvp.Value;
                        break;
                    }
                }

                foreach (KeyValuePair<View, bool> kvp in UIConfig.ShowMapButton)
                {
                    if (kvp.Key.Name == this.view.Name || kvp.Key == this.view)
                    {
                        tsbMap.Visible = kvp.Value;
                        break;
                    }
                }

                foreach (KeyValuePair<View, bool> kvp in UIConfig.ShowNavButtons)
                {
                    if (kvp.Key.Name == this.view.Name || kvp.Key == this.view)
                    {
                        toolStripFirstButton.Visible = kvp.Value;
                        toolStripLastButton.Visible = kvp.Value;
                        toolStripNextButton.Visible = kvp.Value;
                        toolStripPreviousButton.Visible = kvp.Value;
                        break;
                    }
                }

                foreach (KeyValuePair<View, bool> kvp in UIConfig.ShowNewRecordButton)
                {
                    if (kvp.Key.Name == this.view.Name || kvp.Key == this.view)
                    {
                        btiNew.Visible = kvp.Value;
                        break;
                    }
                }

                foreach (KeyValuePair<View, bool> kvp in UIConfig.ShowOpenFormButton)
                {
                    if (kvp.Key.Name == this.view.Name || kvp.Key == this.view)
                    {
                        btiOpen.Visible = kvp.Value;
                        break;
                    }
                }

                foreach (KeyValuePair<View, bool> kvp in UIConfig.ShowPrintButton)
                {
                    if (kvp.Key.Name == this.view.Name || kvp.Key == this.view)
                    {
                        btiPrint.Visible = kvp.Value;
                        break;
                    }
                }

                foreach (KeyValuePair<View, bool> kvp in UIConfig.ShowRecordCounter)
                {
                    if (kvp.Key.Name == this.view.Name || kvp.Key == this.view)
                    {
                        toolStripRecordNumber.Visible = kvp.Value;
                        toolStripOfLabel.Visible = kvp.Value;
                        toolStripRecordCount.Visible = kvp.Value;
                        break;
                    }
                }

                foreach (KeyValuePair<View, bool> kvp in UIConfig.ShowSaveRecordButton)
                {
                    if (kvp.Key.Name == this.view.Name || kvp.Key == this.view)
                    {
                        btiSave.Visible = kvp.Value;
                        break;
                    }
                }

                foreach (KeyValuePair<View, bool> kvp in UIConfig.ShowToolbar)
                {
                    if (kvp.Key.Name == this.view.Name || kvp.Key == this.view)
                    {
                        toolStrip1.Visible = kvp.Value;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Opens the specified view.
        /// </summary>
        /// <param name="view"></param>
        public void OpenView(View view)
        {
            BeginBusy(SharedStrings.OPENING_VIEW);

            if (view != null)
            {
                this.view = view;
                if (this.homeView == null)
                {
                    this.homeView = this.view;
                }

                ConfigureInterface();

                isViewOpened = true;
                OnViewAccessed();

                if (view != null && view.IsRelatedView)
                {
                    tsbDashboard.Enabled = false;
                    tsbMap.Enabled = false;
                }
                
                CheckAndSetPermissions();
            }            

            EndBusy();
        }

        private bool CheckAndSetPermissions()
        {
            permissions = this.view.Project.CollectedData.GetDbDriver().GetPermissions();

            if (permissions == null || permissions.FullPermissions == true) { return true; }

            if (permissions.CanSelectRowsInMetaTables == false) { return false; }            

            canInsertRecords = true;

            if (!permissions.TablesWithInsertPermissions.Contains(view.TableName))
            {
                canInsertRecords = false;
            }

            if (canInsertRecords)
            {
                foreach (Page page in view.Pages)
                {
                    if (!permissions.TablesWithInsertPermissions.Contains(page.TableName))
                    {
                        canInsertRecords = false;
                        break;
                    }
                }
            }

            canUpdateRecords = true;

            if (!permissions.TablesWithUpdatePermissions.Contains(view.TableName))
            {
                canUpdateRecords = false;
            }            

            if (canInsertRecords)
            {
                foreach (Page page in view.Pages)
                {
                    if (!permissions.TablesWithUpdatePermissions.Contains(page.TableName))
                    {
                        canUpdateRecords = false;
                        break;
                    }
                }
            }

            return true;
        }

        public void InvokeOpenView(View view)
        {
            if (this.OpenViewEvent != null)
            {
                OpenViewEvent(this, new Epi.Windows.Enter.PresentationLogic.OpenViewEventArgs(view));
            }
        }

        /// <summary>
        /// Sets up the Enter Main Form
        /// </summary>
        public void Menu_Render()
        {
            if (!isViewOpened)
            {
                closeViewToolStripMenuItem.Enabled = false;
                mnuImportFromPhone.Enabled = false;
                mnuImportFromForm.Enabled = false;
                mnuImportFromWeb.Enabled = false;
                mnuImportFromDataPackage.Enabled = false;
                mnuPackageForTransport.Enabled = false;
                fromWebEnterToolStripMenuItem.Enabled = false;
                newRecordToolStripMenuItem.Enabled = false;
                saveToolStripMenuItem.Enabled = false;
                editViewToolStripMenuItem.Enabled = false;
                findToolStripMenuItem.Enabled = false;
                printToolStripMenuItem.Enabled = false;
                markAsDeletedToolStripMenuItem.Enabled = false;
                undeleteToolStripMenuItem.Enabled = false;
                btiEditView.Enabled = false;
                btiNew.Enabled = false;
                btiSave.Enabled = false;
                btiFind.Enabled = false;
                btiPrint.Enabled = false;
                btiMarkDeleted.Enabled = false;
                btiUndelete.Enabled = false;
            }
            else
            {
                closeViewToolStripMenuItem.Enabled = true;
                if (this.View.IsRelatedView && this.View.ReturnToParent)
                {
                    newRecordToolStripMenuItem.Enabled = false;
                    btiNew.Enabled = false;
                }
                else
                {
                    newRecordToolStripMenuItem.Enabled = true;
                    btiNew.Enabled = true;
                }
                saveToolStripMenuItem.Enabled = true;
                mnuImportFromPhone.Enabled = true;
                mnuImportFromForm.Enabled = true;
                mnuImportFromWeb.Enabled = true;
                mnuImportFromDataPackage.Enabled = true;
                mnuPackageForTransport.Enabled = true;
                fromWebEnterToolStripMenuItem.Enabled = true;
                editViewToolStripMenuItem.Enabled = true;
                findToolStripMenuItem.Enabled = true;
                printToolStripMenuItem.Enabled = true;
                markAsDeletedToolStripMenuItem.Enabled = this.view.CurrentRecordStatus  == 1;
                undeleteToolStripMenuItem.Enabled = this.view.CurrentRecordStatus  != 1;
                btiEditView.Enabled = true;
                btiNew.Enabled = true;
                btiSave.Enabled = false;
                btiFind.Enabled = true;
                btiPrint.Enabled = true;
                btiMarkDeleted.Enabled = false;
                btiUndelete.Enabled = false;
            }
        }

        /// <summary>
        /// Add recent view to EpiInfo.config file and display
        /// it on the menubar if it does not already exist in the recent
        /// view collection
        /// </summary>                             
        private void OnViewAccessed()
        {
            Configuration.OnViewAccessed(this.view);
            LoadRecentViews();
        }

        /// <summary>
        /// Sets up the view and its datatable, if necessary
        /// </summary>
        private void ShowViewSelection()
        {
            ViewSelectionDialog viewSelectionDialog = new ViewSelectionDialog(this, CurrentProject);
            DialogResult result = viewSelectionDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (this.view != null)
                {
                    if (IsRecordCloseable == false)
                    {
                        return;
                    }

                    if (CloseView() == false)
                    {
                        return;
                    }
                }

                // If the project was changed, update current project. Otherwise, use the current project
                if (CurrentProject == null || (string.Compare(CurrentProject.FilePath, viewSelectionDialog.CurrentProject.FilePath, true) != 0))
                {
                    CurrentProject = viewSelectionDialog.CurrentProject;
                }

                this.view = CurrentProject.Views.GetViewById(viewSelectionDialog.ViewId);
                viewSelectionDialog.Close();
                //--123
                // this.view.VerifyandUpdateViewSystemVars();
                ////
                if (!this.view.IsRelatedView)
                {
                    Project project = view.GetProject();

                    if (CurrentProject.CollectedData.TableExists(view.TableName) == false)
                    {
                        DataTablePropertiesDialog dataTableProperties = new DataTablePropertiesDialog(this, this.CurrentProject, this.View);
                        dataTableProperties.DataTableName = view.TableName;
                        dataTableProperties.ShowDialog();

                        if (dataTableProperties.DialogResult == DialogResult.OK)
                        {
                            view.SetTableName(dataTableProperties.DataTableName);
                            project.CollectedData.CreateDataTableForView(view, dataTableProperties.StartingIndex);

                            foreach (View descendantView in view.GetDescendantViews())
                            {
                                if (!project.CollectedData.TableExists(descendantView.TableName))
                                {
                                    descendantView.SetTableName(descendantView.Name);
                                    project.CollectedData.CreateDataTableForView(descendantView, 1);
                                }
                            }
                        }
                        else
                        {
                            MsgBox.ShowWarning("A form cannot be opened in Enter without first creating a data table.");
                            return;
                        }
                    }

                    if (this.OpenViewEvent != null)
                    {
                        this.OpenViewEvent(this, new Epi.Windows.Enter.PresentationLogic.OpenViewEventArgs(view));
                    }
                }
                else
                {
                    MsgBox.ShowInformation(SharedStrings.CANNOT_OPEN_VIEW_NO_DATA_TABLE);
                    return;
                }
            }
        }

        #endregion  Private Methods

        #region Public Properties

        /// <summary>
        /// Returns EnterEngine reference
        /// </summary>
        public new EnterWindowsModule Module
        {
            get
            {
                return (EnterWindowsModule)base.Module;
            }

            set { base.Module = value; }
        }

        /// <summary>
        /// Record Id from Find Records Dialog
        /// </summary>
        public string RecordId
        {
            get
            {
                return recordId;
            }
            set
            {
                recordId = value;
            }
        }

        /// <summary>
        /// Returns current view
        /// </summary>
        public View View
        {
            get
            {
                return view;
            }

            set { this.view = value; }
        }

        public RunTimeView RunTimeView
        {
            get
            {
                return runTimeView;
            }

            set { this.runTimeView = value; }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsRecordCloseable
        {
            get
            {
                bool shouldCheckRequiredFields = true;
                if (!ContinueChangeRecord)
                {
                    MsgBox.ShowWarning(SharedStrings.CANNOT_CLOSE_RECORD_INVALID_DATA);
                    return false;
                }

                if (this != null && this.view != null)
                {
                    mediator.SetFieldData();

                    if (View.IsEmptyNewRecord() == false)
                    {
                        if (shouldCheckRequiredFields && !mediator.CheckViewRequiredFields())
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Returns or sets the variable that controls whether or not a record can be closed
        /// </summary>
        public bool ContinueChangeRecord
        {
            get
            {
                return continueChangeRecord;
            }
            set
            {
                continueChangeRecord = value;
            }
        }


        /// <summary>
        /// Current background image
        /// </summary>
        public Image CurrentBackgroundImage
        {
            get
            {
                return this.currentBackgroundImage;
            }

            set
            {
                this.currentBackgroundImage = value;
            }
        }

        /// <summary>
        /// Current Background Image Layout
        /// </summary>
        public string CurrentBackgroundImageLayout
        {
            get
            {
                return this.currentBackgroundImageLayout;
            }

            set
            {
                this.currentBackgroundImageLayout = value;
            }
        }

        /// <summary>
        /// Current Background Color
        /// </summary>
        public Color CurrentBackgroundColor
        {
            get
            {
                if (currentBackgroundColor.IsEmpty == true)
                {
                    return SystemColors.Window;
                }
                else
                {
                    return this.currentBackgroundColor;
                }
            }

            set
            {
                this.currentBackgroundColor = value;
            }
        }

        /// <summary>
        /// Returns whether or not the form has closed
        /// </summary>
        public bool FormClosed
        {
            get
            {
                if (isClosed <= 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }            
        }

        /// <summary>
        /// Get the width of the ViewExplorer for positioning the canvas.
        /// </summary>
        public int ViewExplorerWidth
        {
            get { return viewExplorer.Width;}
        }

        #endregion  //Public Properties

        #region Public Methods

        /// <summary>
        /// Closes current view
        /// </summary>
        public bool CloseView()
        {
            // Get a confirmation from the user
            if (this.CloseViewEvent != null)
            {
                this.CloseViewEvent(this, new EventArgs());
                SetWindowTitle();
            }

            return true;
        }

        public bool OpenView(string openViewName)
        {
            if (this.CloseViewEvent != null)
            {
                if (this.currentProject.Views.Contains(openViewName) == true)
                {
                    View targetView = this.currentProject.Views[openViewName];

                    if (targetView.IsHomeParentOf(view))
                    {
                        this.CloseViewEvent(true, new EventArgs());
                    }
                    else if (targetView.IsParentOf(view))
                    {
                        this.CloseViewEvent(false, new EventArgs());
                    }
                    else
                    {


                        string currentViewName = string.Empty;
                        string currentProject = string.Empty;

                        bool shouldCheckRequiredFields = true;

                        if (this.view != null)
                        {
                            if (!ContinueChangeRecord)
                            {
                                MsgBox.ShowWarning(SharedStrings.CANNOT_CLOSE_RECORD_INVALID_DATA);

                            }

                            mediator.SetFieldData();
                            if (View.IsEmptyNewRecord() == false)
                            {
                                if (shouldCheckRequiredFields == true)
                                {
                                    bool allRequiredsComplete = mediator.CheckViewRequiredFields();

                                    if (allRequiredsComplete == false)
                                    {

                                    }
                                }
                            }

                            currentViewName = this.view.Name;
                            currentProject = this.view.GetProject().FullName;

                            //if (View.Text.Substring(View.Text.LastIndexOf(":") + 1) != currentViewName && currentProject.ToLower() != View.Text.ToLower())
                            //{
                            //    if (!CloseView())
                            //    {
                            //        //return;
                            //    }
                            //}

                            this.CloseViewEvent(this, new EventArgs());

                        }


                        View methodView = CurrentProject.Metadata.GetViewByFullName(openViewName);

                        if (methodView == null)
                        {
                            string name = string.Empty;
                            if (openViewName.Length > 3)
                            {
                                name = openViewName.Remove(0, 3);
                            }
                            else
                            {
                                name = openViewName;
                            }

                            MsgBox.ShowError(string.Format(SharedStrings.ERROR_LOADING_VIEW, name));
                        }
                        else if (!methodView.IsRelatedView)
                        {
                            if (this.OpenViewEvent != null)
                            {
                                this.View = methodView;
                                this.OpenViewEvent(this, new Epi.Windows.Enter.PresentationLogic.OpenViewEventArgs(methodView));
                            }
                        }
                        else
                        {
                            MsgBox.ShowInformation(SharedStrings.CANNOT_OPEN_VIEW_RELATED);
                        }

                        SetWindowTitle();
                    }
                }
            }

            return true;
        }

        public bool OpenPage(string pageName, string formName = "")
        {
            // Get a confirmation from the user
            if (this.CloseViewEvent != null)
            {
                this.CloseViewEvent(this, new EventArgs());
                SetWindowTitle();
            }

            return true;
        }

        /// <summary>
        /// Enables and disables controls based on the rec status
        /// </summary>
        /// <param name="enabled">True or false for enabled status</param>
        public void EnableDisableCurrentRecord(bool enabled)
        {
            btiMarkDeleted.Enabled = enabled;
            markAsDeletedToolStripMenuItem.Enabled = enabled;
            btiUndelete.Enabled = !enabled;
            undeleteToolStripMenuItem.Enabled = !enabled;
        }

        public void EnableDisableSaveButton(bool enable)
        {
            saveToolStripMenuItem.Enabled = enable;
            btiSave.Enabled = enable;
        }

        /// <summary>
        /// Disables delete and undelete buttons for a new record
        /// </summary>
        public void DisableCurrentRecord()
        {
            btiMarkDeleted.Enabled = false;
            markAsDeletedToolStripMenuItem.Enabled = false;
            btiUndelete.Enabled = false;
            undeleteToolStripMenuItem.Enabled = false;
        }


        /// <summary>
        /// Sets menu items when adding a new record
        /// </summary>
        public void AddNewRecordSettings()
        {
            //lblTotalRecords.Text = Epi.Windows.Localization.LocalizeString("New Record");
            toolStripLastButton.Enabled = false;
            toolStripNextButton.Enabled = false;


            int recordCount = this.view.GetRecordCount();
            if (recordCount != 0)
            {
                toolStripFirstButton.Enabled = true;
                toolStripPreviousButton.Enabled = true;
            }
            toolStripRecordNumber.Text = (recordCount + 1).ToString();
            toolStripRecordCount.Text = string.Empty;

            /*
            if (this.viewExplorer.ReturnToParentRecord)
            {
                toolStripFirstButton.Enabled = false;
                toolStripLastButton.Enabled = false;
                btiNew.Enabled = false;
                toolStripPageNumber.Enabled = false;
            }*/
        }

        /// <summary>
        /// Sets the record number for the main form
        /// </summary>
        public void SetRecordNumber()
        {
            int recordCount = this.view.GetRecordCount();
            toolStripRecordNumber.Text = (recordCount + 1).ToString();
            toolStripRecordCount.Text = string.Empty;
        }

        /// <summary>
        /// Updates the tool strip with record number info when navigated to the next record
        /// </summary>
        public void NextRecordSelected()
        {
            if (View.IsEmptyNewRecord() == false)
            {
                if (!(mediator.CheckViewRequiredFields()))
                    return;
                mediator.SaveRecord();
            }
        }

        /// <summary>
        /// Updates the tool strip with record number info when navigated to the last record
        /// </summary>
        public void LastRecordSelected()
        {
            if (View.IsEmptyNewRecord() == false)
            {
                if (!(mediator.CheckViewRequiredFields()))
                    return;
                mediator.SaveRecord();
            }
        }

        /// <summary>
        /// Reset the form
        /// </summary>
        public void Reset()
        {
            EnableRecordNavigationButtons(true);

            toolStripRecordNumber.Text = string.Empty;
            toolStripRecordCount.Text = string.Empty;

        }

        /// <summary>
        /// The record's status is displayed on the status bar
        /// </summary>
        public void DisplayRecordStatus()
        {
            if (!string.IsNullOrEmpty(this.view.RecStatusField.CurrentRecordValueString))
            {
                bool isUndeleted = viewExplorer.GetRecStatus(int.Parse(this.view.RecStatusField.CurrentRecordValueString));
                if (!isUndeleted)   //Record is marked for deletion
                {
                    UpdateAppSpecificInfo(SharedStrings.DELETED_RECORD);
                }
                else
                {
                    UpdateAppSpecificInfoDefault();
                }
            }
            else
            {
                UpdateAppSpecificInfoDefault();
            }
        }

        /// <summary>
        /// Enables/Disables buttons to navigate through records
        /// </summary>        
        public void RecordNavigation_Render()
        {
            toolStripFirstButton.Enabled = true;
            toolStripPreviousButton.Enabled = true;
            toolStripRecordNumber.Enabled = true;
            toolStripNextButton.Enabled = true;
            toolStripLastButton.Enabled = true;

            UpdateRecordCount();
        }

        public void Render()
        {
            if (view == null)
            {
                isViewOpened = false;
            }
            SetWindowTitle();
            Menu_Render();
            RecordNavigation_Render();

            // Update configuration
            
            statusBarToolStripMenuItem.Checked = config.Settings.ShowStatusBar;
            //btnHome.Visible = false;
            //btnHome.Enabled = false;
            toolStrip2.Visible = view.IsRelatedView;
        }

        public void LoadRecord(int recordId)
        {
            if (IsRecordCloseable == false)
            {
                return;
            }

            if (this.GotoRecordEvent != null)
            {
                canvas.HostContainer.SelectTab(0);
                this.GotoRecordEvent(this, new GoToRecordEventArgs(recordId.ToString()));
            }
        }

        #endregion  //Public Methods

        #region IProjectHost Members

        /// <summary>
        /// The current project
        /// </summary>
        public Project CurrentProject
        {
            get
            {
                return currentProject;
            }
            set
            {
                currentProject = value;
            }

        }
        Project currentProject;

        /// <summary>
        /// Sets the project host's current project
        /// </summary>
        private void SetHostCurrentProject(Project project)
        {
            IProjectHost host = Module.GetService(typeof(IProjectHost)) as IProjectHost;
            if (host == null)
            {
                throw new GeneralException("Project host is not registered.");
            }
            else
            {
                host.CurrentProject = project;
            }
        }


        public void FireOpenViewEvent(View pView, string record = "")
        {
            if (this.OpenViewEvent != null)
            {
                Epi.Windows.Enter.PresentationLogic.OpenViewEventArgs openArgs = new Epi.Windows.Enter.PresentationLogic.OpenViewEventArgs(pView);
                openArgs.RecordNumber = record;
                this.OpenViewEvent(this, openArgs);
            }
        }
        #endregion

        #region Event Handlers
        private void statusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnViewStatusBarClicked(sender as ToolStripMenuItem);
        }

        private void epiInfoLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WinUtil.OpenTextFile(Logger.GetLogFilePath());
        }

        private void printableHTMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = GetHTMLLineListing();
            try
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.FileName = fileName;
                    proc.StartInfo.UseShellExecute = true;
                    proc.Start();
                }
            }
            catch (System.ComponentModel.Win32Exception)
            {
                Epi.Windows.MsgBox.ShowError("A problem was encountered while displaying the HTML line list. Please check the configuration of the computer's default web browser.");
            }
        }

        private void excelLineListMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = GetHTMLLineListing();
            if (!string.IsNullOrEmpty(fileName))
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = "excel";
                proc.StartInfo.Arguments = "\"" + fileName + "\"";
                proc.StartInfo.UseShellExecute = true;
                proc.Start();
            }
        }

        private string GetImageExtension(MemoryStream stream)
        {
            System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
            Type Type = typeof(System.Drawing.Imaging.ImageFormat);
            System.Reflection.PropertyInfo[] imageFormatList = Type.GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            for (int i = 0; i != imageFormatList.Length; i++)
            {
                System.Drawing.Imaging.ImageFormat formatClass = (System.Drawing.Imaging.ImageFormat)imageFormatList[i].GetValue(null, null);
                if (formatClass.Guid.Equals(image.RawFormat.Guid))
                {
                    return imageFormatList[i].Name.ToLower();
                }
            }
            return "";
        }

        private string GetHTMLLineListing()
        {
            if (canvas.CurrentView != null)
            {
                Epi.Data.IDbDriver db = Epi.Data.DBReadExecute.GetDataDriver(canvas.CurrentView.Project.FilePath);

                DataTable data = db.GetTableData(View.TableName, "GlobalRecordId, UniqueKey");
                data.TableName = this.View.TableName;

                //DataTable data = db.Select(db.CreateQuery("SELECT * " + canvas.CurrentView.FromViewSQL));

                foreach (Page page in this.View.Pages)
                {
                    DataTable pageTable = db.GetTableData(page.TableName);
                    pageTable.TableName = page.TableName;

                    foreach (DataColumn dc in pageTable.Columns)
                    {
                        if (dc.ColumnName != "GlobalRecordId")
                        {
                            data.Columns.Add(dc.ColumnName, dc.DataType);
                        }
                    }

                    foreach (DataRow row in pageTable.Rows)
                    {
                        foreach (DataRow viewRow in data.Rows)
                        {
                            if (viewRow["GlobalRecordId"].ToString().Equals(row["GlobalRecordId"].ToString()))
                            {
                                foreach (DataColumn dc in pageTable.Columns)
                                {
                                    viewRow[dc.ColumnName] = row[dc.ColumnName];
                                }
                            }
                        }
                    }
                }

                try
                {
                    Util.SortColumnsByTabOrder(data, this.View);
                }
                catch
                {
                }

                string fileName = Path.GetTempPath() + Guid.NewGuid().ToString("N") + ".html";

                FileStream stream = File.OpenWrite(fileName);
                StreamWriter sw = new StreamWriter(stream);
                sw.WriteLine("<html><head><title>Line Listing - " + canvas.CurrentView.Name + "</title>");
                sw.WriteLine("<meta http-equiv=\"content-type\" content=\"text/html;charset=UTF-8\" />");
                sw.WriteLine("<meta name=\"author\" content=\"Epi Info 7\" />");
                sw.WriteLine(@"<style type=""text/css"">
                            BODY
                            {
                            font-size:16px; 
                            color:BLACK; 
                            font-weight:normal; 
                            font-style:normal; 
                            text-decoration:none; 
                            font-size: 11pt;
                            font-family: Calibri, Arial, sans-serif;
                            }
                            H1
                            {
                            color:#4a7ac9;
                            }
                            TABLE
                            {
                            border-collapse:collapse;
                            padding:5px;
                            }
                            TD
                            {
                            border-style:solid;
                            border-color:Black;
                            border-width:1px;
                            padding-left: 0.4em;
                            padding-right: 0.4em;
                            font-size: 10pt;
                            }
                            TH
                            {
                            border-style:solid;
                            border-color:Black;
                            border-width:1px;
                            padding-left: 0.4em;
                            padding-right: 0.4em;
                            background-color:#4a7ac9;
                            color:#FFFFFF;
                            min-width: 50px;
                            }
                            TD.Stats
                            {
                            border-style:none;
                            border-width:0px;
                            padding-left:5px;
                            padding-right:5px;
                            padding-bottom:0px;
                            }
                            TH.Stats
                            {
                            border-style:none;
                            border-width:0px;
                            background-color:#4a7ac9;
                            color:#FFFFFF;
                            padding:5px;
                            }
                            HR
                            {
                            background-color:#d2451e;
                            }
                            </style></head>");
                sw.WriteLine("<h1>" + canvas.CurrentView.Name + "</h1><hr><body><table>");
                foreach (DataColumn col in data.Columns)
                {
                    if (!col.ColumnName.ToLower().Contains("globalrecordid"))
                    {
                        sw.WriteLine("<th>" + col.ColumnName + "</th>");
                    }
                }
                foreach (DataRow row in data.Rows)
                {
                    sw.WriteLine("<tr>");
                    for (int x = 0; x < data.Columns.Count; x++)
                    {
                        if (!data.Columns[x].ColumnName.ToLower().Contains("globalrecordid"))
                        {
                            if (row[x] is byte[])
                            {
                                string extension = GetImageExtension(new MemoryStream((byte[])row[x]));
                                string imgFileName = Path.GetTempPath() + Guid.NewGuid().ToString("N") + "." + extension;

                                FileStream imgStream = File.OpenWrite(imgFileName);
                                BinaryWriter imgWriter = new BinaryWriter(imgStream);
                                imgWriter.Write((byte[])row[x]);
                                imgWriter.Close();
                                imgStream.Close();
                                sw.WriteLine("<td><img src=\"" + imgFileName + "\"/></td>");
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(row[x].ToString()) && data.Columns[x].DataType.ToString().Equals("System.DateTime") && View.Fields.Contains(data.Columns[x].ColumnName) && View.Fields[data.Columns[x].ColumnName] is Epi.Fields.DateField)
                                {
                                    DateTime dt = DateTime.Parse(row[x].ToString(), System.Globalization.CultureInfo.CurrentCulture);
                                    string dateValue = dt.ToString("d", System.Globalization.CultureInfo.CurrentCulture);
                                    sw.WriteLine("<td>" + dateValue + "</td>");
                                }
                                else if (!string.IsNullOrEmpty(row[x].ToString()) && data.Columns[x].DataType.ToString().Equals("System.DateTime") && View.Fields.Contains(data.Columns[x].ColumnName) && View.Fields[data.Columns[x].ColumnName] is Epi.Fields.TimeField)
                                {
                                    DateTime dt = DateTime.Parse(row[x].ToString(), System.Globalization.CultureInfo.CurrentCulture);
                                    string dateValue = dt.ToString("T", System.Globalization.CultureInfo.CurrentCulture);
                                    sw.WriteLine("<td>" + dateValue + "</td>");
                                }
                                else if (View.Fields.Contains(data.Columns[x].ColumnName) && (View.Fields[data.Columns[x].ColumnName] is Epi.Fields.YesNoField || View.Fields[data.Columns[x].ColumnName] is Epi.Fields.CheckBoxField))
                                {
                                    string value = row[x].ToString().ToLower();
                                    if (value.Equals("true") || value.Equals("1"))
                                    {
                                        value = config.Settings.RepresentationOfYes;
                                    }
                                    else if (value.Equals("false") || value.Equals("0"))
                                    {
                                        value = config.Settings.RepresentationOfNo;
                                    }
                                    else if (string.IsNullOrEmpty(value))
                                    {
                                        value = config.Settings.RepresentationOfMissing;
                                    }
                                    sw.WriteLine("<td>" + value + "</td>");
                                }
                                else
                                {
                                    sw.WriteLine("<td>" + row[x].ToString() + "</td>");
                                }
                            }
                        }
                    }
                    sw.WriteLine("</tr>");
                }
                sw.WriteLine("</table></body></html>");
                sw.Close();
                stream.Close();
                return fileName;
            }
            return null;
        }

        private void gridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (canvas.CurrentView != null)
            {
                Epi.Data.IDbDriver db = Epi.Data.DBReadExecute.GetDataDriver(canvas.CurrentView.Project.FilePath);
                LineListingViewer viewer = new LineListingViewer(canvas.CurrentView, db, canvas.CurrentView.Name);
                viewer.RecordSelected += new RecordSelectedHandler(LineListing_RecordSelected);
                viewer.Show();
            }
        }

        void LineListing_RecordSelected(int id)
        {
            LoadRecord(id);
        }

        #endregion Event Handlers


        private void WriteGC(object o)
        {
            Console.WriteLine("******* EnterMainForm  Collector Status ******");
            Console.WriteLine("Estimated bytes on heap: {0}", GC.GetTotalMemory(false));
            Console.WriteLine("This OS has {0} object generations.\n", GC.MaxGeneration + 1);
            Console.WriteLine("Generation of parameter object is: {0}", GC.GetGeneration(o));
        }

        private void enableCheckCodeExecutionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.mediator.IsExecutionEnabled = enableCheckCodeExecutionToolStripMenuItem.Checked;
        }

        private void enableCheckCodeErrorSupressionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.mediator.IsSuppressErrorsEnabled = enableCheckCodeErrorSupressionToolStripMenuItem.Checked;
        }

        private void mnuImportFromProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportFormDataFromForm(this.View);
        }

        private void mnuImportFromWebToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportFormDataFromWeb(this.View);
        }

        private void mnuImportFromDataPackage_Click(object sender, EventArgs e)
        {
            ImportFormDataFromPackage(this.View);
        }

        private void mnuPackageForTransportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PackageForTransport(this.View);
        }

        /// <summary>
        /// Method for showing the 'Package for transport' dialog box.
        /// </summary>
        /// <param name="sourceView">The form to be packaged.</param>
        private void PackageForTransport(View sourceView)
        {
            // Do not allow if it's a relational form of any sort
            if (sourceView.IsRelatedView)
            {
                MsgBox.ShowInformation(SharedStrings.ERROR_CREATE_PACKAGE_RELATED_FORM);
                return;
            }

            Epi.Windows.ImportExport.Dialogs.PackageForTransportDialog packageDialog = new Epi.Windows.ImportExport.Dialogs.PackageForTransportDialog(sourceView.Project.FilePath, sourceView);
            DialogResult result = packageDialog.ShowDialog();
        }

        /// <summary>
        /// Imports data from a similar form
        /// </summary>
        /// <param name="destinationView">The view whose data will be imported</param>
        private void ImportFormDataFromWeb(View destinationView)
        {
            if (destinationView.IsRelatedView)
            {
                return;
            }

            Epi.Enter.Forms.ImportWebDataForm importWebDataForm = new Epi.Enter.Forms.ImportWebDataForm(destinationView);
            DialogResult result = importWebDataForm.ShowDialog();
            
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (this.View != null)
                {
                    if (IsRecordCloseable == false)
                    {
                        return;
                    }

                    if (CloseView() == false)
                    {
                        return;
                    }
                }

                this.view = destinationView;

                Project project = view.Project;

                if (!project.CollectedData.TableExists(view.TableName))
                {
                    MsgBox.ShowError("Something that should never fail has failed.");
                }

                if (this.OpenViewEvent != null)
                {
                    this.OpenViewEvent(this, new Epi.Windows.Enter.PresentationLogic.OpenViewEventArgs(view));
                }
            }
        }
        private void ImportFormDataFromWebEnter(View destinationView)
            {
            if (destinationView.IsRelatedView)
                {
                return;
                }

            Epi.Enter.Forms.ImportWebEnterDataForm importWebDataForm = new Epi.Enter.Forms.ImportWebEnterDataForm(destinationView);
            DialogResult result = importWebDataForm.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
                {
                if (this.View != null)
                    {
                    if (IsRecordCloseable == false)
                        {
                        return;
                        }

                    if (CloseView() == false)
                        {
                        return;
                        }
                    }

                this.view = destinationView;

                Project project = view.Project;

                if (!project.CollectedData.TableExists(view.TableName))
                    {
                    MsgBox.ShowError("Something that should never fail has failed.");
                    }

                if (this.OpenViewEvent != null)
                    {
                    this.OpenViewEvent(this, new Epi.Windows.Enter.PresentationLogic.OpenViewEventArgs(view));
                    }
                }
            }
        /// <summary>
        /// Imports data from an Android phone.
        /// </summary>
        /// <param name="destinationView"></param>
        private void ImportDataFromPhone(View destinationView)
        {
            //if (destinationView.IsRelatedView)
            //{
            //    return;
            //}

            Epi.Enter.Forms.ImportPhoneDataForm importPhoneDataForm = new Epi.Enter.Forms.ImportPhoneDataForm(destinationView);
            DialogResult result = importPhoneDataForm.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (this.View != null)
                {
                    if (IsRecordCloseable == false)
                    {
                        return;
                    }

                    if (CloseView() == false)
                    {
                        return;
                    }
                }

                this.view = destinationView;

                Project project = view.Project;
                if (!project.CollectedData.TableExists(view.TableName))
                {
                    MsgBox.ShowError("Something that should never fail has failed.");
                }

                if (this.OpenViewEvent != null)
                {
                    this.OpenViewEvent(this, new Epi.Windows.Enter.PresentationLogic.OpenViewEventArgs(view));
                }
            }
        }

        /// <summary>
        /// Imports data from a similar form
        /// </summary>
        /// <param name="destinationView">The view whose data will be imported</param>
        private void ImportFormDataFromForm(View destinationView)
        {
            if (destinationView.IsRelatedView)
            {
                MsgBox.ShowInformation(SharedStrings.ERROR_IMPORT_FORM_RELATED_FORM);
                return;
            }

            Epi.Enter.Forms.ImportDataForm importDataForm = new Epi.Enter.Forms.ImportDataForm(destinationView);
            DialogResult result = importDataForm.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (this.View != null)
                {
                    if (IsRecordCloseable == false)
                    {
                        return;
                    }

                    if (CloseView() == false)
                    {
                        return;
                    }
                }

                this.view = destinationView;

                Project project = view.Project;
                if (!project.CollectedData.TableExists(view.TableName))
                {
                    MsgBox.ShowError("Something that should never fail has failed.");
                }

                if (this.OpenViewEvent != null)
                {
                    this.OpenViewEvent(this, new Epi.Windows.Enter.PresentationLogic.OpenViewEventArgs(view));
                }
            }
        }

        /// <summary>
        /// Imports data from a similar form that was contained in a data package (EDP file)
        /// </summary>
        /// <param name="destinationView">The view that will accept the incoming data</param>
        private void ImportFormDataFromPackage(View destinationView)
        {
            if (destinationView.IsRelatedView)
            {
                Epi.Windows.MsgBox.ShowInformation(SharedStrings.ERROR_IMPORT_PACKAGE_RELATED_FORM);
                return;
            }

            Epi.Windows.ImportExport.Dialogs.ImportEncryptedDataPackageDialog importDataForm = new Epi.Windows.ImportExport.Dialogs.ImportEncryptedDataPackageDialog(destinationView);
            DialogResult result = importDataForm.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                if (this.View != null)
                {
                    if (IsRecordCloseable == false)
                    {
                        return;
                    }

                    if (CloseView() == false)
                    {
                        return;
                    }
                }

                this.view = destinationView;

                Project project = view.Project;
                if (!project.CollectedData.TableExists(view.TableName))
                {
                    MsgBox.ShowError("Something that should never fail has failed.");
                }

                if (this.OpenViewEvent != null)
                {
                    this.OpenViewEvent(this, new Epi.Windows.Enter.PresentationLogic.OpenViewEventArgs(view));
                }
            }
        }

        private void mnuImportFromPhone_Click(object sender, EventArgs e)
        {
            ImportDataFromPhone(this.View);
            return;
            try
            {
                List<KeyValuePair<string, string>> images = new List<KeyValuePair<string, string>>();
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "\"Epi Info for Android\" sync files|*.epi7";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    DataSet ds = new DataSet();
                    ds.ReadXml(dialog.FileName);
                    if (canvas.CurrentView != null)
                    {
                        Epi.Data.IDbDriver db = Epi.Data.DBReadExecute.GetDataDriver(canvas.CurrentView.Project.FilePath);
                        string columns = "(GlobalRecordId,";
                        foreach (DataColumn col in ds.Tables[0].Columns)
                        {
                            columns += col.ColumnName + ",";
                            if (!canvas.CurrentView.Fields.Names.Contains(col.ColumnName))
                            {
                                throw new ApplicationException("The selected sync file is not compatible with the current view.");
                            }
                        }
                        columns = columns.Substring(0, columns.Length - 1);
                        columns += ")";
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            string recordId = Guid.NewGuid().ToString();
                            string values = "('" + recordId + "',";
                            foreach (DataColumn col in ds.Tables[0].Columns)
                            {
                                string value = string.IsNullOrEmpty(row[col.ColumnName].ToString()) ? "null" : row[col.ColumnName].ToString();
                                if (value.Equals("''"))
                                    value = "null";
                                if (value.Contains(".jpg'"))
                                    images.Add(new KeyValuePair<string,string>(col.ColumnName,value.Trim('\'')));
                                values += value + ",";
                            }
                            values = values.Substring(0, values.Length - 1);
                            values += ")";
                            string query = "INSERT INTO " + canvas.CurrentView.TableName + canvas.CurrentView.Pages[0].Id + " " + columns + " VALUES " + values;
                            int result1 = db.ExecuteNonQuery(db.CreateQuery(query));
                            if (canvas.CurrentView.Pages.Count > 1)
                            {
                                foreach (Page page in canvas.CurrentView.Pages)
                                {
                                    if (page.Id != canvas.CurrentView.Pages[0].Id)
                                    {
                                        string extraQuery = "INSERT INTO " + page.TableName + "(GlobalRecordId) VALUES ('" + recordId + "')";
                                        int extraResult = db.ExecuteNonQuery(db.CreateQuery(extraQuery));
                                    }
                                }
                            }

                            int result2 = db.ExecuteNonQuery(db.CreateQuery("INSERT INTO " + canvas.CurrentView.TableName + " (GlobalRecordId) VALUES ('" + recordId + "')"));

                            foreach (KeyValuePair<string, string> image in images)
                            {
                                string part1 = dialog.FileName.Substring(0, dialog.FileName.IndexOf("EpiInfo"));
                                string part2 = image.Value.Substring(image.Value.IndexOf("EpiInfo"), image.Value.Length - image.Value.IndexOf("EpiInfo"));
                                string fileName = part1 + part2;
                                byte[] imageBytes = Util.GetByteArrayFromImagePath(fileName);
                                string updateQueryString = "UPDATE " + canvas.CurrentView.Pages[0].TableName + " SET " + image.Key + " = @img WHERE GlobalRecordId='" + recordId + "'";
                                Data.Query updateQuery = db.CreateQuery(updateQueryString);
                                updateQuery.Parameters.Add(new Data.QueryParameter("@img", DbType.Binary, imageBytes));
                                int result3 = db.ExecuteNonQuery(updateQuery);
                            }
                        }
                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            MessageBox.Show(ds.Tables[0].Rows.Count.ToString() + " records have been successfully imported from the phone.", "Import Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            UpdateRecordCount();
                            if (this.GotoRecordEvent != null)
                            {
                                this.GotoRecordEvent(this, new GoToRecordEventArgs("<<"));
                            }
                        }
                    }
                }
            }
            catch (ApplicationException ex)
            {
                MessageBox.Show(ex.Message, "Incompatible Sync File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void tsbDashboard_Click(object sender, EventArgs e)
        {
            if (this.SaveRecordEvent != null && IsRecordCloseable)
            {
                this.SaveRecordEvent(this, e);
                UpdateRecordCount();
            }

            AnalyticsViewer analyticsViewer = new AnalyticsViewer(this);
            analyticsViewer.Render(canvas.CurrentView);
            analyticsViewer.Show();
        }

        private void tsbMap_Click(object sender, EventArgs e)
        {
            MapViewer mapViewer = new MapViewer(this);
            mapViewer.Render(canvas.CurrentView);
            mapViewer.Show();
        }

        private void dataDictionaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.runTimeView != null && this.runTimeView.View != null)
            {
                Epi.Windows.Enter.Forms.DataDictionary dataDictionaryDialog = new Epi.Windows.Enter.Forms.DataDictionary(this.runTimeView.View, this);
                DialogResult dialogResult = dataDictionaryDialog.ShowDialog();
            }
        }

       
        /// <summary>
        /// Handles the Click event of the print button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mediator.IsDirty == true)
            {
                DialogResult result = MessageBox.Show("Select [ OK ] to save the current record and print. To return without saving select [ Cancel ].", "Save > Print", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                if (((DialogResult)result) == DialogResult.Cancel)
                {
                    return;
                }

                if (this.SaveRecordEvent != null && IsRecordCloseable)
                {
                    this.SaveRecordEvent(this, e);
                    UpdateRecordCount();
                    EnableDisableCurrentRecord(true);
                }
            }

            if (mediator.IsDirty == false)
            {
                Epi.Windows.Enter.Dialogs.Print printDialog = new Dialogs.Print();
                printDialog.Show();
            }
        }

        private void copyShortcutToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.view != null)
                {
                    if (!this.view.IsRelatedView)
                    {
                        Clipboard.SetText(CharLiterals.DOUBLEQUOTES + System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + System.AppDomain.CurrentDomain.SetupInformation.ApplicationName + CharLiterals.DOUBLEQUOTES +
                           " /Project:" + CharLiterals.DOUBLEQUOTES + this.view.GetProject().FullName + CharLiterals.DOUBLEQUOTES +
                           " /View:" + CharLiterals.DOUBLEQUOTES + this.view.Name + CharLiterals.DOUBLEQUOTES);
                    }
                    else
                    {
                        MsgBox.ShowError(SharedStrings.CANNOT_CREATE_SHORTCUT_FOR_CHILD_FORM);
                    }
                }
                else
                {
                    Clipboard.SetText(CharLiterals.DOUBLEQUOTES + System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + System.AppDomain.CurrentDomain.SetupInformation.ApplicationName + CharLiterals.DOUBLEQUOTES);
                }
            }
            catch (FileNotFoundException ex)
            {
                MsgBox.ShowException(ex);
            }
            finally
            {
            }
        }
 private void fromWebEnterToolStripMenuItem_Click(object sender, EventArgs e)
            {
            ImportFormDataFromWebEnter(this.view);
            }

 private void enterDockManager_Paint(object sender, PaintEventArgs e)
 {

 }
    }
}
