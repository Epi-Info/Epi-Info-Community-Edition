#region Namespaces
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Reflection;
using Epi.Data;
using Epi.DataSets;
using Epi.Fields;
using Epi.Windows;
using Epi.Windows.Controls;
using Epi.Windows.Dialogs;
using Epi.Windows.Docking;
using Epi.Windows.MakeView;
using Epi.Windows.MakeView.Utils;
using Epi.Windows.MakeView.Dialogs;
using Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs;
using EpiInfo.Plugin;
using PortableDevices;
using Epi.Web.Common.Exception;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Threading;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Epi.Windows.MakeView.PresentationLogic;
#endregion Namespaces

namespace Epi.Windows.MakeView.Forms
{
    /// <summary>
    /// Main form for the MakeView module.
    /// </summary>	
    public partial class MakeViewMainForm : MainForm
    {
        public IEnterInterpreter Interpreter;
        
        private Image currentBackgroundImage;
        private string currentBackgroundImagePath;
        private string currentBackgroundImageLayout;
        private Color currentBackgroundColor;
        private bool applyBackgroundToAllPages;
        private DataTable bgTable;
        private ProjectPermissions projectPermissions;
        private System.IO.FileSystemWatcher _templateWatcher;
        private const int PAGE_BREAK_LIMIT = 32; // fields per page
        private string OrganizationKey ;
        private string EWEOrganizationKey;
        private string UserPublishKey = null;
        private bool openForViewingOnly = false;
        private string SurveyName = "";
        private string DepartmentName = "";
        private string SurveyNumber = "";
        private string OrganizationName = "";
        private DateTime StartDate;
        private DateTime CloseDate;
        private bool IsSingleResponse;
        private bool IsDraftMode;
        private bool IsEWEDraftMode;
        private string IntroductionText;
        private string ExitText;
        private string TemplateXML;
        private int SurveyType;
        private string SurveyId;
        private string WebServiceBindingMode;
        private int WebServiceAuthMode;
        private string WebServiceEndpointAddress;
        BackgroundWorker worker = null;
        SplashScreenForm sf = null;
        private string RepublishOrgKey = null;
        private bool iscancel = false;
        //private GuiMediator mediater = null;
        /// <summary>
        /// PageChanged EventHandler
        /// </summary>
        public event EventHandler PageChanged;

        /// <summary>
        /// GridSettingsChanged EventHandler
        /// </summary>
        public event EventHandler GridSettingsChanged;

        #region Constructors

        /// <summary>
        /// Default Constructor - For exclusive use by the designer
        /// </summary>
        //[Obsolete("Use of default constructor not allowed", true)]
        public MakeViewMainForm()
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Configuration.GetNewInstance().Settings.Language);
            InitializeComponent();
            Construct();  
        }

        /// <summary>
        /// Default constructor for the form.
        /// </summary>
        public MakeViewMainForm(MakeViewWindowsModule mod)
            : base(mod)
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Configuration.GetNewInstance().Settings.Language);
            InitializeComponent();
            Construct();            
        }

        /// <summary>
        /// Constructs the form
        /// </summary>
        private void Construct()
        {
            DockManager.FastMoveDraw = false;
            DockManager.Style = DockVisualStyle.VS2005;
            projectExplorer = new ProjectExplorer(this);
            projectExplorer.PageSelected += new ProjectExplorerPageSelectedEventHandler(projectExplorer_PageSelected);
            projectExplorer.CreateNewOpenField += new ProjectExplorerNewOpenFieldEventHandler(projectExplorer_CreateNewOpenField);
            projectExplorer.CreateNewProjectFromTemplate += new ProjectExplorerNewProjectFromTemplateEventHandler(projectExplorer_CreateNewProjectFromTemplate);
            projectExplorer.CreateFieldLayoutControl += new ProjectExplorerCreateFieldLayoutControlEventHandler(projectExplorer_CreateFieldLayoutControl);
            projectExplorer.AllowClose = false;
            projectExplorer.AllowUnDock = false;

            canvas = new Canvas(this);
            canvas.AllowClose = false;
            canvas.AllowUnDock = false;
            this.KeyPreview = true;

            dockManager1.DockWindow(canvas, DockStyle.Fill);
            dockManager1.DockWindow(projectExplorer, DockStyle.Left);
            FormName = SharedStrings.MAKE_VIEW_WINDOW_TITLE;

            InitializeMediator();

            Configuration config = Configuration.GetNewInstance();
            string execPath = config.Directories.Templates;

            _templateWatcher = new System.IO.FileSystemWatcher();

            _templateWatcher.Path = execPath.TrimEnd(new char[]{'\\'});
            _templateWatcher.Filter = "*.*";

            _templateWatcher.Changed += new FileSystemEventHandler(_templateWatcher_Changed);
            _templateWatcher.Created += new FileSystemEventHandler(_templateWatcher_Changed);
            _templateWatcher.Deleted += new FileSystemEventHandler(_templateWatcher_Changed);
            _templateWatcher.Renamed += new RenamedEventHandler(_templateWatcher_Renamed);

            _templateWatcher.EnableRaisingEvents = true;

            WebServiceBindingMode = config.Settings.WebServiceBindingMode;
            WebServiceAuthMode = config.Settings.WebServiceAuthMode;
            WebServiceEndpointAddress = config.Settings.WebServiceEndpointAddress;
        }

        void _templateWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate { projectExplorer.UpdateTemplates(); });
        }

        void _templateWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate { projectExplorer.UpdateTemplates(); });
        }

        void projectExplorer_CreateNewProjectFromTemplate(string templatePathFragment)
        {
            try
            {
                NewProjectFromTemplate(templatePathFragment);
            }
            catch (Exception ex)
            {
                MsgBox.ShowException(ex);
                return;
            }
        }

        public void projectExplorer_CreateNewOpenField(MetaFieldType type)
        {
            Field field = mediator.CreateNewOpenField(type, mediator.Canvas.PagePanel, new Point(3,3), false);

            IFieldControl newControl = null;
            ArrayList controlArrayList = new ArrayList();

            foreach (Control fieldControl in mediator.Canvas.PagePanel.Controls)
            {
                if (fieldControl is IFieldControl && ((IFieldControl)fieldControl).Field == field)
                {
                    newControl = (IFieldControl)fieldControl;
                    controlArrayList.Add(newControl);
                }

                if (controlArrayList.Count == 2) break;
            }

            mediator.SelectedControlsChanged(controlArrayList, false);

            if (field != null && field.Id > 0 && field is RenderableField)
            {
                RenderableField renderField = (RenderableField)field;

                int left = (int)(renderField.ControlLeftPositionPercentage * mediator.Canvas.PagePanel.Width);
                int right = (int)((renderField.ControlLeftPositionPercentage + renderField.ControlWidthPercentage) * mediator.Canvas.PagePanel.Width);
                
                int top = (int)(renderField.ControlTopPositionPercentage * mediator.Canvas.PagePanel.Height);
                int bottom = (int)((renderField.ControlTopPositionPercentage + renderField.ControlHeightPercentage) * mediator.Canvas.PagePanel.Height);

                if (field is FieldWithSeparatePrompt)
                {
                    int promptLeft = (int)(((FieldWithSeparatePrompt)renderField).PromptLeftPositionPercentage * mediator.Canvas.PagePanel.Width);
                    int promptTop = (int)(((FieldWithSeparatePrompt)renderField).PromptTopPositionPercentage * mediator.Canvas.PagePanel.Height);

                    left = promptLeft < left ? promptLeft : left;
                    top = promptTop < top ? promptTop : top;
                }

                int width = right - left;
                int height = bottom - top;

                bool conflict = false;
                bool relocated = false;

                do
                {
                    conflict = false;

                    foreach (Control fieldControl in mediator.Canvas.PagePanel.Controls)
                    {
                        if (fieldControl is IFieldControl)
                        {
                            bool isSameField = ((Field)((IFieldControl)fieldControl).Field) == field;

                            Rectangle eachRectangle = RectangleToScreen(fieldControl.Bounds);
                            Rectangle candidateRectangle = RectangleToScreen(new Rectangle(left, top, width, height));

                            if (false == isSameField && eachRectangle.IntersectsWith(candidateRectangle))
                            {
                                conflict = true;
                                break;
                            }
                        }
                    }

                    if (conflict)
                    {
                        top = top + height + 5;
                        relocated = true;
                    }

                    if (top + height > mediator.Canvas.PagePanel.Height)
                    {
                        conflict = false;
                        relocated = false;
                    }
                }
                while (conflict);

                if (relocated)
                {
                    foreach (KeyValuePair<IFieldControl, Point> kvp in mediator.SelectedFieldControls)
                    {
                        Control control = (Control)kvp.Key;
                        control.Left = kvp.Value.X + left;
                        control.Top = kvp.Value.Y + top;
                    }

                    mediator.PersistSelectedFieldControlCollection();
                    mediator.InitialSelectedControlLocations.Clear();
                    mediator.SelectedFieldControls.Clear();
                }
            }
        }

        void projectExplorer_CreateFieldLayoutControl(Size size)
        {
            mediator.CreateFieldFootprintControl(size);
        }

        #endregion

        #region Public Properties

        new public IEnterInterpreter EpiInterpreter
        {
            get { return this.Interpreter; }
            set { this.Interpreter = value; }
        }

        /// <summary>
        /// Redefined module Property
        /// </summary>
        public new MakeViewWindowsModule Module
        {
            get
            {
                return base.Module as MakeViewWindowsModule;
            }
        }

        /// <summary>
        /// Sets and gets where a command can be generated depending upon 
        /// which item is selected in the editor tool window fields combobox
        /// </summary>
        public bool CanCommandBeGenerated
        {
            get
            {
                return canCommandBeGenerated;
            }
            set
            {
                canCommandBeGenerated = value;
            }
        }

        /// <summary>
        /// Returns the currently selected view
        /// </summary>
        public View CurrentView
        {
            get { return this.CurrentPage.view; }
        }

        /// <summary>
        /// Returns the currently selected Page
        /// </summary>
        public Page CurrentPage
        {
            get
            {
                return this.projectExplorer.currentPage;
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
        /// Current background image path
        /// </summary>
        public string CurrentBackgroundImagePath
        {
            get
            {
                return this.currentBackgroundImagePath;
            }

            set
            {
                this.currentBackgroundImagePath = value;
            }
        }

        /// <summary>
        /// Current background image layout
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
        /// Current background color
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
        /// Apply background to all pages?
        /// </summary>
        public bool ApplyBackgroundToAllPages
        {
            get
            {
                return this.applyBackgroundToAllPages;
            }

            set
            {
                this.applyBackgroundToAllPages = value;
            }
        }

        /// <summary>
        /// Create DataTable item enabled
        /// </summary>
        public bool CreateDataTableItemEnabled
        {
            get
            {
                return createDataTableToolStripMenuItem.Enabled;
            }
            set
            {
                this.createDataTableToolStripMenuItem.Enabled = value;
            }
        }

        /// <summary>
        /// Dlete DataTable item enabled
        /// </summary>
        public bool DeleteDataTableItemEnabled
        {
            get
            {
                return deleteDataTableToolStripMenuItem.Enabled;
            }
            set
            {
                this.deleteDataTableToolStripMenuItem.Enabled = value;
            }
        }

        /// <summary>
        /// Rename Page Menu Item enabled
        /// </summary>
        public bool RenamePageItemEnabled
        {
            get
            {
                return renamePageToolStripMenuItem.Enabled;
            }
            set
            {
                this.renamePageToolStripMenuItem.Enabled = value;
            }
        }

        /// <summary>
        /// Delete Page Menu Item enabled
        /// </summary>
        public bool DeletePageItemEnabled
        {
            get
            {
                return deletePageToolStripMenuItem.Enabled;
            }
            set
            {
                this.deletePageToolStripMenuItem.Enabled = value;
            }
        }

        #endregion Public Properties

        #region Protected Methods
        /// <summary>
        /// Sets window title
        /// </summary>
        protected override void SetWindowTitle()
        {
            if (projectExplorer.SelectedPage != null)
            {
                SetWindowTitle(projectExplorer.SelectedPage.DisplayName);
            }
            else
            {
                base.SetWindowTitle();
            }
        }

        #endregion Protected Methods        

        #region Public Methods
        /// <summary>
        /// Enables and disables the 'Create Data Table' and 'Delete Data Table' menu items based on whether or not the View has a data table.
        /// </summary>
        public void SetDataTableMenuItems()
        {
            if (mediator.Project == null)
            {
                this.CreateDataTableItemEnabled = false;
                this.DeleteDataTableItemEnabled = false;
            }
            else if (mediator.ProjectExplorer.currentPage != null && !string.IsNullOrEmpty(mediator.ProjectExplorer.currentPage.view.TableName) && mediator.Project.CollectedData.TableExists(mediator.ProjectExplorer.currentPage.view.TableName))
            {
                this.CreateDataTableItemEnabled = false;
                this.DeleteDataTableItemEnabled = true;
            }
            else
            {
                this.CreateDataTableItemEnabled = true;
                this.DeleteDataTableItemEnabled = false;
            }
        }

        /// <summary>
        /// Enables and disables the 'Rename Page' and 'Delete Page' menu items based on whether or not a Page node is selected in the project explorer.
        /// </summary>
        public void SetRenameDeletePageMenuItems()
        {
            if (mediator.Project == null)
            {
                this.RenamePageItemEnabled = false;
                this.DeletePageItemEnabled = false;
            }
            else if (mediator.ProjectExplorer.IsPageNodeSelected)
            {
                this.RenamePageItemEnabled = true;
                this.DeletePageItemEnabled = true;
            }
            else
            {
                this.RenamePageItemEnabled = false;
                this.DeletePageItemEnabled = false;
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
                    Project project = new Project(projectPath);
                    worker = new BackgroundWorker();
                    worker.WorkerSupportsCancellation = true;
                    worker.DoWork += new DoWorkEventHandler(worker_DoWork);                
                    worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
                    worker.RunWorkerAsync();
                    OpenProject(project);
                }

                if (viewName != null)
                {
                    projectExplorer.SelectView(viewName);
                }
            }
            finally
            {
                worker.CancelAsync();
                worker.DoWork -= new DoWorkEventHandler(worker_DoWork);
                worker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
                worker.Dispose();
                if (sf != null)
                {                  
                    sf.Dispose();
                }
            }
        }

        public void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            if (sf == null)
            {
                sf = new SplashScreenForm();
                sf.ShowDialog();
            }
            while (worker.CancellationPending == false)
            {                           
            }
            if (worker.CancellationPending)
            {
                e.Cancel = true;               
            }
        }       

        private void worker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                if (sf != null)
                {
                    sf.Close();
                    sf.Dispose();
                    worker.DoWork -= new DoWorkEventHandler(worker_DoWork);
                    worker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
                    worker.Dispose();
                }
            }
        }



        public void LoadTemplateFile(string templateFilePath)
        {
            try
            {
                if (templateFilePath != null)
                {
                    GetTemplate(templateFilePath);
                }
            }
            finally
            {
            }
        }


        /// <summary>
        /// Runs a command
        /// </summary>
        /// <param name="command"></param>
        internal void RunCommand(string command)
        {
            try
            {
                //EpiInterpreterParser EIParser = new EpiInterpreterParser(Epi.Resources.ResourceLoader.GetEnterCompiledGrammarTable(), (IEnterCheckCode)mediator, Rule_Context.eRunMode.Enter);
                //EpiInterpreterParser EIParser = new EpiInterpreterParser((IEnterInterpreterHost)mediator);

                command = command.Trim().Trim(System.Environment.NewLine.ToCharArray());
                if (!string.IsNullOrEmpty(command))
                {
                    this.Interpreter.Parse(command.Trim());
                }
            }
            catch (Exception ex)
            {
                MsgBox.ShowException(ex);
            }
        }

        /// <summary>
        /// GetMediator
        /// </summary>
        /// <returns>GuiMediator</returns>
        public PresentationLogic.GuiMediator GetMediator()
        {
            return mediator;
        }

        /// <summary>
        /// Enables the undo button
        /// </summary>
        public bool UndoButtonEnabled
        {
            set { toolStripButtonUndo.Enabled = value; undoToolStripMenuItem.Enabled = value; }
        }

        /// <summary>
        /// Enables the redo button
        /// </summary>
        public bool RedoButtonEnabled
        {
            set { toolStripButtonRedo.Enabled = value; redoToolStripMenuItem.Enabled = value; }
        }

        /// <summary>
        /// Get the metadata about the background and update the current background properties of the main form
        /// </summary>
        /// <param name="page"></param>
        public void ChangeBackgroundData()
        {
            int color;

            if (projectExplorer.currentPage != null)
            {
                this.bgTable = projectExplorer.currentPage.GetMetadata().GetPageBackgroundData(projectExplorer.currentPage);

                DataRow[] rows = bgTable.Select("BackgroundId=" + projectExplorer.currentPage.BackgroundId);
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
        #endregion // Public Methods

        #region Private Methods
        /// <summary>
        /// Checks the permissions the current user has on the database and enables/disables UI elements correspondingly
        /// </summary>
        /// <returns>Whether or not the user has sufficient access to open this project based on the database permissions.</returns>
        private bool CheckAndSetPermissions()
        {
            openForViewingOnly = false;

            if (projectPermissions == null || projectPermissions.FullPermissions == true)
            {
                // No permissions settings are present so assume it's okay to proceed. TODO: Change this to a permissions object.
                return true;
            }

            if (projectPermissions.CanSelectRowsInMetaTables && 
                (
                !projectPermissions.CanUpdateRowsInMetaTables ||
                !projectPermissions.CanInsertRowsInMetaTables ||
                !projectPermissions.CanDeleteRowsInMetaTables)
                )
            {
                openForViewingOnly = true;
            }
            else
            {
                openForViewingOnly = false;
            }

            return true;
        }

        /// <summary>
        /// Obtains information for project to be opened
        /// and displays it in the treenode, explorer
        /// </summary>
        private void OpenProject(Project project)
        {
            Epi.Data.IDbDriver db = project.CollectedData.GetDbDriver();
            projectPermissions = db.GetPermissions();
            if (!CheckAndSetPermissions())
            {
                Epi.Windows.MsgBox.ShowInformation(SharedStrings.INADEQUATE_PERMISSIONS_ACCESS);                
                return;
            }
            else if (this.openForViewingOnly)
            {
                Epi.Windows.MsgBox.ShowInformation(SharedStrings.INADEQUATE_PERMISSIONS_MODIFY );
                this.projectExplorer.OpenForViewingOnly = true;
            }
            else
            {
                this.projectExplorer.OpenForViewingOnly = false;
            }

            canvas.HideUpdateStart(SharedStrings.OPENING_PROJECT);
            canvas.OpenForViewingOnly = this.openForViewingOnly;
            
            mediator.Project = project;
            if (this.Interpreter == null)
            {
                Assembly a = Assembly.Load(project.EnterMakeviewIntepreter);
                Type myType = a.GetType(project.EnterMakeviewIntepreter + ".EpiInterpreterParser");
                this.Interpreter = (IEnterInterpreter)Activator.CreateInstance(myType, new object[] { this.mediator });
                this.Interpreter.Host = this.mediator;
            }

            EnableFeatures();

            try
            {
                bool containsData = false;
                
                foreach (View view in mediator.Project.Views)
                {
                    if (mediator.Project.CollectedData.TableExists(view.TableName))
                    {
                        containsData = true;
                    }
                }

                if (containsData == false)
                {
                    mediator.Project.Metadata.RemovePageOutlierFields();
                }
                
                projectExplorer.LoadProject(project);

                OnProjectAccessed(project);
                
                //try
                //{
                //    Configuration config = Configuration.GetNewInstance();
                //    if (config.Settings.Republish_IsRepbulishable == true)
                //    {
                //        if (!string.IsNullOrWhiteSpace(this.CurrentView.WebSurveyId))
                //        {
                //            mnuPublishToWeb.DropDownItems[0].Enabled = false;
                //            mnuPublishToWeb.DropDownItems[1].Enabled = true;
                //            mnuPublishToWeb.DropDownItems[2].Enabled = true;
                //            mnuPublishToWeb.DropDownItems[3].Enabled = true;
                //            mnuPublishToWeb.DropDownItems[4].Enabled = true;
                //        }
                //        else
                //        {
                //            mnuPublishToWeb.DropDownItems[0].Enabled = true;
                //            mnuPublishToWeb.DropDownItems[1].Enabled = false;
                //            mnuPublishToWeb.DropDownItems[2].Enabled = false;
                //            mnuPublishToWeb.DropDownItems[3].Enabled = false;
                //            mnuPublishToWeb.DropDownItems[4].Enabled = false;
                //        }
                //    }
                //    else
                //    {
                //        mnuPublishToWeb.DropDownItems[0].Enabled = true;
                //        mnuPublishToWeb.DropDownItems[1].Enabled = false;
                //        mnuPublishToWeb.DropDownItems[2].Enabled = false;
                //        mnuPublishToWeb.DropDownItems[3].Enabled = false;
                //        mnuPublishToWeb.DropDownItems[4].Enabled = false;
                //    }
                //}
                //catch (Exception ex)
                //{
                //    mnuPublishToWeb.DropDownItems[0].Enabled = true;
                //    mnuPublishToWeb.DropDownItems[1].Enabled = false;
                //    mnuPublishToWeb.DropDownItems[2].Enabled = false;
                //    mnuPublishToWeb.DropDownItems[3].Enabled = false;
                //    mnuPublishToWeb.DropDownItems[4].Enabled = false;
                //}

                this.SetPublishMenuItems(this.CurrentView);
                SetDataTableMenuItems();
                mediator.InitialData = new InitialCollectedData(project.CollectedData);
                canvas.HideUpdateEnd();
            }
            finally
            {
                canvas.WaitPanelForceHide();
            }

            if (this.openForViewingOnly)
            {
                canvas.EnablePagePanel(false);
            }
            else
            {                
                canvas.EnablePagePanel(true);
            }
        }

        /// <summary>
        /// Add recent project to EpiInfo.config file and display
        /// it on the menubar if it does not already exist in the recent
        /// project collection
        /// </summary>
        /// <param name="project">project accessed</param>
        private void OnProjectAccessed(Project project)
        {
            #region Input Validation
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }
            #endregion Input validation

            Epi.Configuration.OnProjectAccessed(project);
            LoadRecentProjects();
        }

        /// <summary>
        /// Closes the current project
        /// </summary>
        /// <returns>True/False;Depending upon whether current project is closed</returns>
        private bool CloseCurrentProject()
        {
            bool closed = false;
            mediator.Canvas.HideUpdateStart(SharedStrings.CLOSING_PROJECT);

            if (mediator.ProjectExplorer.currentPage != null && mediator.Project != null && !openForViewingOnly)
            { 
                if (mediator.Project.CollectedData.TableExists(mediator.ProjectExplorer.currentPage.view.TableName))
                {
                    try
                    {
                        mediator.Project.CollectedData.SynchronizeDataTable(mediator.ProjectExplorer.currentPage.view);
                    }
                    catch
                    {
                        MsgBox.ShowWarning(SharedStrings.WARNING_CANNOT_SYNCHRONIZE);
                    }
                }
            }

            SetDataTableMenuItems();
            mediator.Reset();
            UpdateWindowTitle();
            DisableFeatures();
            
            ChangeBackgroundData();
            if (this.PageChanged != null)
            {
                PageChanged(this, new EventArgs());
            }
            closed = true;

            if (closed)
            {
                this.Text = SharedStrings.MAKE_VIEW_WINDOW_TITLE;
            }

            mediator.Canvas.WaitPanelForceHide();

            return closed;
        }

        /// <summary>
        /// Closes the current MakeView
        /// </summary>
        private void CloseCurrentMakeView()
        {
            mediator.CloseAll();
            this.Dispose();
        }

        /// <summary>
        /// The Project Manager
        /// </summary>
        protected IProjectManager ProjectManager
        {
            get
            {
                IProjectManager manager = Module.GetService(typeof(IProjectManager)) as IProjectManager;
                if (manager == null)
                {
                    throw new GeneralException(SharedStrings.ERROR_PM_NOT_REGISTERED);
                }
                return manager;
            }
        }

        /// <summary>
        /// Loads the project from the command line
        /// </summary>
        private void LoadProjectFromCommandLine()
        {
            try
            {
                ICommandLine commandLine = (ICommandLine)this.Module.GetService(typeof(ICommandLine));
                if (commandLine != null)
                {
                    string projectPath = commandLine.GetArgument("project");
                    if (projectPath != null)
                    {
                        Project project = ProjectManager.OpenProject(projectPath);
                        OpenProject(project);
                    }
                    string viewName = commandLine.GetArgument("view");
                    if (viewName != null)
                    {
                        projectExplorer.SelectView(viewName);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(SharedStrings.ERROR_LOADING_PROJECT_CMDLINE, ex); 
            }
        }

        /// <summary>
        /// Loads recent projects opened or created by user
        /// </summary>		
        private void LoadRecentProjects()
        {
            Configuration config = Configuration.GetNewInstance();
            DataView dv = config.RecentProjects.DefaultView;
            try
            {
                dv.Sort = "LastAccessed desc";
                recentProjectsToolStripMenuItem.DropDownItems.Clear();
                int countToShow = Math.Min(dv.Count, config.Settings.MRUProjectsCount);
                for (int index = 0; index < countToShow; index++)
                {
                    string projectFullPath = "&" + (index + 1) + " " + dv[index]["Location"].ToString();

                    ToolStripMenuItem recentProjectItem = new ToolStripMenuItem(projectFullPath);
                    recentProjectItem.Click += new EventHandler(recentProjectItem_Click);
                    recentProjectsToolStripMenuItem.DropDownItems.Add(recentProjectItem);
                }
                recentProjectsToolStripMenuItem.Enabled = (recentProjectsToolStripMenuItem.DropDownItems.Count > 0);
                if (config.Settings.Republish_IsRepbulishable && (!string.IsNullOrEmpty(config.Settings.WebServiceEndpointAddress)))
                    {
                      QuickPublishtoolStripButton.Visible = true;
                      ChangeModetoolStripDropDownButton.Visible = true;
                      toolStripSeparator10.Visible = true;                     
                    }
                if (config.Settings.Republish_IsRepbulishable && (!string.IsNullOrEmpty(config.Settings.EWEServiceEndpointAddress)))
                {
                    QuickPublishtoolStripButton.Visible = true;
                    ChangeModetoolStripDropDownButton.Visible = true;                   
                    toolStripSeparator11.Visible = true;
                }
                else 
                    {
                    QuickPublishtoolStripButton.Visible = false;
                    ChangeModetoolStripDropDownButton.Visible = false;
                    toolStripSeparator10.Visible = false;
                    toolStripSeparator11.Visible = false;
                    
                    }
            }
            finally
            {
                dv.Sort = string.Empty;
            }
        }

        /// <summary>
        /// Initialized the mediator
        /// </summary>
        private void InitializeMediator()
        {
            mediator = PresentationLogic.GuiMediator.CreateInstance(this);
            mediator.Canvas = canvas;
            mediator.ProjectExplorer = projectExplorer;
            mediator.BeginBusyEvent += new BeginBusyEventHandler(BeginBusy);
            mediator.EndBusyEvent += new EndBusyEventHandler(EndBusy);
            mediator.ProgressReportBeginEvent += new ProgressReportBeginEventHandler(this.ProgressReportBegin);
            mediator.ProgressReportUpdateEvent += new ProgressReportUpdateEventHandler(this.ProgressReportUpdate);
            mediator.ProgressReportEndEvent += new SimpleEventHandler(this.ProgressReportEnd);
        }

        /// <summary>
        /// Runs the wizard for a new project
        /// </summary>
        private void RunWizardForNewProject()
        {
            ProjectCreationDialog dialog = new ProjectCreationDialog();
            dialog.ShowDialog();

            if (dialog.DialogResult == DialogResult.OK)
            {
                BeginBusy(SharedStrings.CREATING_PROJECT);
                Project project = dialog.Project;

                mediator.Project = project;
                if (this.Interpreter == null)
                {
                    Assembly assembly = Assembly.Load(project.EnterMakeviewIntepreter);
                    Type myType = assembly.GetType(project.EnterMakeviewIntepreter + ".EpiInterpreterParser");
                    this.Interpreter = (IEnterInterpreter)Activator.CreateInstance(myType, new object[] { this.mediator });
                    this.Interpreter.Host = this.mediator;
                }

                projectExplorer.LoadProject(project);
                projectExplorer.AddPageToCurrentView();
                EnableFeatures();
                OnProjectAccessed(project);
                dialog.Close();
                EndBusy();
            }
        }

        private void RunWizardForNewProjectFromTemplate(string selectedTemplate = null)
        {
            selectedTemplate = selectedTemplate == null ? string.Empty : selectedTemplate;
            
            ProjectFromTemplateDialog dialog = new ProjectFromTemplateDialog(selectedTemplate);

            string projectName = string.Empty;
            string projectDescription = string.Empty;
            string projectLocation = string.Empty;
            string dataDBInfo = string.Empty;
            Data.DbDriverInfo dbDriverInfo = new Data.DbDriverInfo();
            string projectTemplatePath = string.Empty;

            try
            {
                dialog.ShowDialog();

                if (dialog.DialogResult == DialogResult.OK)
                {
                    CloseCurrentProject();
                    projectName = dialog.ProjectName;
                    projectLocation = dialog.ProjectLocation;
                    dataDBInfo = dialog.DataDBInfo;
                    dbDriverInfo = dialog.DriverInfo;
                    projectTemplatePath = dialog.ProjectTemplatePath;
                }
                else
                {
                    return;
                }
            }
            finally
            {
                dialog.Dispose();
                GC.Collect();
                Refresh();
            }

            canvas.HideUpdateStart(SharedStrings.CREATING_PROJECT);

            Project newProject = new Project();

            newProject = newProject.CreateProject(
                projectName,
                projectDescription,
                projectLocation,
                dataDBInfo,
                dbDriverInfo);

            if (newProject != null)
            {
                mediator.Project = newProject;
                if (this.Interpreter == null)
                {
                    Assembly assembly = Assembly.Load(newProject.EnterMakeviewIntepreter);
                    Type myType = assembly.GetType(newProject.EnterMakeviewIntepreter + ".EpiInterpreterParser");
                    this.Interpreter = (IEnterInterpreter)Activator.CreateInstance(myType, new object[] { this.mediator });
                    this.Interpreter.Host = this.mediator;
                }

                canvas.UpdateHidePanel(SharedStrings.LOADING_PROJECT);

                projectExplorer.LoadProject(newProject);

                Template template = new Template(this.mediator);
                template.CreateFromTemplate(projectTemplatePath);

                //EnableFeatures();
                OnProjectAccessed(newProject);

                // The code below is needed to catch a condition where the ParentView property of each View object is
                // not set during the creation of the template. Instead of re-writing this code in the template creation
                // process, we simply force the metadata to be refreshed (which assigns the ParentView property correctly).
                newProject.views = null;
                newProject.LoadViews();
            }

            canvas.HideUpdateEnd();
            EnableFeatures();
        }

        /// <summary>
        /// Enables menu items 
        /// </summary>
        private void EnableFeatures()
        {
            if (!openForViewingOnly)
            {
                deletePageToolStripMenuItem.Enabled = true;
                renamePageToolStripMenuItem.Enabled = true;
                NewPageMenuItem.Enabled = true;
                NewViewMenuItem.Enabled = true;
                pageToolStripMenuItem1.Enabled = true;
                insertPageToolStripMenuItem.Enabled = true;
                groupToolStripMenuItem.Enabled = true;
                addPageToolStripMenuItem.Enabled = true;
                saveToolStripMenuItem.Enabled = true;
                orderOfFieldEntryToolStripMenuItem.Enabled = true;
                checkCodeToolStripMenuItem.Enabled = true;
                btnCheckCode.Enabled = true;
                backgroundToolStripMenuItem.Enabled = true;
                pageSetupToolStripMenuItem.Enabled = true;
                alignmentToolStripMenuItem.Enabled = true;
                this.mediator.Canvas.EnablePagePanel(true);
                UndoButtonEnabled = true;
                RedoButtonEnabled = true;
                makeViewFromDataTableToolStripMenuItem.Enabled = true;// false;//true;
            }
            else
            {
                deletePageToolStripMenuItem.Enabled = false;
                renamePageToolStripMenuItem.Enabled = false;
                NewPageMenuItem.Enabled = false;
                NewViewMenuItem.Enabled = false;
                pageToolStripMenuItem1.Enabled = false;
                insertPageToolStripMenuItem.Enabled = false;
                groupToolStripMenuItem.Enabled = false;
                addPageToolStripMenuItem.Enabled = false;
                saveToolStripMenuItem.Enabled = false;
                orderOfFieldEntryToolStripMenuItem.Enabled = false;
                checkCodeToolStripMenuItem.Enabled = false;
                btnCheckCode.Enabled = false;
                backgroundToolStripMenuItem.Enabled = false;
                pageSetupToolStripMenuItem.Enabled = false;
                alignmentToolStripMenuItem.Enabled = false;
                this.mediator.Canvas.EnablePagePanel(false);
                UndoButtonEnabled = false;                
                RedoButtonEnabled = false;
                makeViewFromDataTableToolStripMenuItem.Enabled = false;
            }

            closeProjectToolStripMenuItem.Enabled = true;
            mnuCopyToPhone.Enabled = true;
            mnuPublishToWeb.Enabled = true;

            toolStripButtonCloseProject.Enabled = true;
            enterDataToolStripMenuItem.Enabled = true;
            
            btnEnterData.Enabled = true;
            btnSave.Enabled = true;
            settingsToolStripMenuItem.Enabled = true;            
            setDefaultFontToolStripMenuItem.Enabled = true;
            setDefaultControlFontToolStripMenuItem.Enabled = true;
            
            displayDataDictionaryToolStripMenuItem.Enabled = true;

            if (projectExplorer.SelectedPage != null && !openForViewingOnly)
            {
                View currentView = this.mediator.ProjectExplorer.SelectedPage.GetView();
                if (currentView.Project.CollectedData.TableExists(currentView.TableName))
                {
                    deleteDataTableToolStripMenuItem.Enabled = true;
                }
            }
            else
            { 
                deleteDataTableToolStripMenuItem.Enabled = false;
            }
        }

        /// <summary>
        /// Disables menu items 
        /// </summary>
        private void DisableFeatures()
        {
            deletePageToolStripMenuItem.Enabled = false;
            renamePageToolStripMenuItem.Enabled = false;
            NewPageMenuItem.Enabled = false;
            NewViewMenuItem.Enabled = false;
            pageToolStripMenuItem1.Enabled = false;

            insertPageToolStripMenuItem.Enabled = true;
            groupToolStripMenuItem.Enabled = false;
            addPageToolStripMenuItem.Enabled = false;
            saveToolStripMenuItem.Enabled = false;
            orderOfFieldEntryToolStripMenuItem.Enabled = false;
            closeProjectToolStripMenuItem.Enabled = false;
            mnuCopyToPhone.Enabled = false;
            mnuPublishToWeb.Enabled = false;

            toolStripButtonCloseProject.Enabled = false;
            enterDataToolStripMenuItem.Enabled = false;
            checkCodeToolStripMenuItem.Enabled = false;
            btnCheckCode.Enabled = false;
            btnEnterData.Enabled = false;
            btnSave.Enabled = false;
            settingsToolStripMenuItem.Enabled = false;
            backgroundToolStripMenuItem.Enabled = false;
            pageSetupToolStripMenuItem.Enabled = false;
            setDefaultFontToolStripMenuItem.Enabled = false;
            setDefaultControlFontToolStripMenuItem.Enabled = false;
            alignmentToolStripMenuItem.Enabled = false;
            this.mediator.Canvas.EnablePagePanel(false);
            displayDataDictionaryToolStripMenuItem.Enabled = false;
            UndoButtonEnabled = false;
            RedoButtonEnabled = false;

            if (projectExplorer.SelectedPage != null)
            {
                View currentView = this.mediator.ProjectExplorer.SelectedPage.GetView();
                if (currentView.Project.CollectedData.TableExists(currentView.TableName))
                    deleteDataTableToolStripMenuItem.Enabled = true;
            }
            else
                deleteDataTableToolStripMenuItem.Enabled = false;
        }

        /// <summary>
        /// Creates a new view
        /// </summary>
        /// <param name="project">The current project</param>
        /// <returns>Newly created view</returns>
        private View PromptUserToCreateView(Project project)
        {
            View newView = null;
            CreateViewDialog viewDialog = new CreateViewDialog(this, project);
            viewDialog.ShowDialog();
            if (viewDialog.DialogResult == DialogResult.OK)
            {
                string newViewName = viewDialog.ViewName;
                newView = project.CreateView(newViewName);
                // Create a page
                newView.CreatePage(SharedStrings.NEW_PAGE, 0);
            }
            viewDialog.Close();
            return newView;
        }

        /// <summary>
        /// Creates a data table for the view if the user desires to do so
        /// </summary>
        private bool CreateViewDataTable()
        {
            if (this.mediator.ProjectExplorer.SelectedPage != null && this.mediator.Project != null)
            {
                View currentView = this.mediator.ProjectExplorer.SelectedPage.GetView();

                if (currentView.Project.CollectedData.TableExists(currentView.TableName) == false)
                {                    
                    DialogResult result = MsgBox.Show(SharedStrings.DATA_TABLE_NOT_CREATED, SharedStrings.CREATE_NEW_DATA_TABLE, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        Dialogs.TableDefinitionDialog tableDefinition = new Dialogs.TableDefinitionDialog(currentView);
                        tableDefinition.ShowDialog();

                        if (tableDefinition.DialogResult == DialogResult.OK)
                        {
                            this.Cursor = Cursors.WaitCursor;
                            
                            currentView.SetTableName(tableDefinition.DataTableName);
                            this.mediator.Project.CollectedData.CreateDataTableForView(currentView, tableDefinition.StartingID);
                            this.createDataTableToolStripMenuItem.Enabled = false;

                            foreach (View view in currentView.GetDescendantViews())
                            {
                                if (!this.mediator.Project.CollectedData.TableExists(view.TableName))
                                {
                                    view.SetTableName(view.Name);
                                    this.mediator.Project.CollectedData.CreateDataTableForView(view, 1);
                                }
                            }
                            
                            this.Cursor = Cursors.Default;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    this.createDataTableToolStripMenuItem.Enabled = false;
                }
            }
            return true;
        }

        /// <summary>
        ///  Checks the form to be published for invalid field types
        /// </summary>
        /// <returns>
        /// Returns a string listing field names of fields with invalid field type.
        /// </returns>
        private string ListFieldsNotSupportedForWeb()
        {
            string invalidFields = String.Empty;
            if (this.mediator.ProjectExplorer.SelectedPage != null && this.mediator.Project != null)
            {
                View currentView = this.mediator.ProjectExplorer.SelectedPage.GetView();
                foreach (Epi.Fields.Field field in currentView.Fields)
                {
                    switch (field.FieldType.ToString())
                    {
                        case "Codes":
                        case "TextUppercase":
                        case "GUID":
                        case "PhoneNumber":
                        case "DateTime":
                        case "Image":
                        case "Mirror":
                        case "Grid":
                            invalidFields += field.Name + " (" + field.FieldType + ");";
                            break;
                    }
                }
            }
            return invalidFields;
        }

        #endregion Private Methods

        #region Event Handlers

        /// <summary>
        /// Handles the KeyDown event for the main form
        /// </summary>
        /// <param name="e">.NET supplied event parameters</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            mediator.OnShortcutKeyPressed(e);
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
        /// Handles the Page Selected event of the project explorer
        /// </summary>
        /// <param name="page">The page selected</param>
        private void projectExplorer_PageSelected(Page page)
        {
            this.Text = SharedStrings.MAKE_VIEW_WINDOW_TITLE + " - [" + page.GetProject().FilePath + "\\" + page.GetView().Name + "\\" + page.Name + "]";
            projectExplorer.SaveFieldVariables();
            ChangeBackgroundData();

            if (this.PageChanged != null)
            {
                this.PageChanged(this, new EventArgs());
            }
        }

        /// <summary>
        /// Handles the Click event of the recent project item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void recentProjectItem_Click(object sender, EventArgs e)
        {
            if (projectExplorer.IsProjectLoaded)
            {
                if (CloseCurrentProject() == false)
                {
                    return;
                }
            }

            try
            {
                Project project = new Project(((ToolStripMenuItem)sender).Text.Substring(3));
                OpenProject(project);
            }
            catch (Exception ex)
            {
                MsgBox.ShowException(ex);
                return;
            }
        }

        /// <summary>
        /// Handles the request to close forms of the mediator
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void mediator_CloseFormsRequested(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the click event of the Open menu item
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">.NET supplied event args.</param>
        private void OpenProject_Click(object sender, System.EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // openFileDialog.Filter = "Epi2000 and Epi7 Project Files (*.mdb;*.prj)|*.mdb;*.prj|Epi2000 Project files (*.mdb)|*.mdb |Epi7 Project Files (*.prj)|*.prj";
#if LINUX_BUILD
            openFileDialog.Filter = "*.prj";
#else
            openFileDialog.Filter = SharedStrings.EI7_PROJECT_FILES + " (*.prj)|*.prj";
#endif

            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;
            Configuration config = Configuration.GetNewInstance();
            openFileDialog.InitialDirectory = config.Directories.Project;
            DialogResult res = openFileDialog.ShowDialog();
            if (res == DialogResult.OK)
            {
                try
                {
                    if (projectExplorer.IsProjectLoaded)
                    {
                        if (CloseCurrentProject() == false)
                        {
                            return;
                        }
                    }

                    string filePath = openFileDialog.FileName.Trim();
                    if (filePath.ToLower(System.Globalization.CultureInfo.CurrentCulture).EndsWith(FileExtensions.EPI_PROJ))
                    {
                        // This is an Epi 7 project. Open it.
                        Project project = new Project(filePath);

                        if (project.CollectedData.FullName.Contains("MS Access"))
                        {
                            string databaseFileName = project.CollectedData.DataSource.Replace("Data Source=".ToLower(), string.Empty);
                            if (System.IO.File.Exists(databaseFileName) == false)
                            {
                                MsgBox.ShowError(string.Format(SharedStrings.DATASOURCE_NOT_FOUND, databaseFileName));
                                return;
                            }
                        }

                        OpenProject(project);
                    }
                }
                catch (System.Security.Cryptography.CryptographicException ex)
                {
                    MsgBox.ShowError(string.Format(SharedStrings.ERROR_CRYPTO_KEYS, ex.Message));
                    return;
                }
                catch (Exception ex)
                {
                    MsgBox.ShowException(ex);
                    return;
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (mediator != null)
            {
                mediator.SetCanvasDisplayProperties();
                //MessageBox.Show(this.WindowState.ToString());                
            }
        }

        /// <summary>
        /// Loads MakeView screen
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void Form_Load(object sender, System.EventArgs e)
        {
            LoadRecentProjects();
            //LoadProjectFromCommandLine();
        }

        /// <summary>
        /// Close MakeView screen
        /// </summary>
        private void Form_Closing(object sender, FormClosingEventArgs e)
        {
            if (projectExplorer.IsProjectLoaded)
            {
                if (mediator.OkayToClearClipboard())
                {
                    if (CloseCurrentProject() == false)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
            CloseCurrentMakeView();
        }

        /// <summary>
        /// Handles the Click event of the delete page menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void deletePageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            projectExplorer.DeleteCurrentPage();
        }

        /// <summary>
        /// Handles the Click event of the rename page menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void renamePageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            projectExplorer.RenameCurrentPage();
        }

        /// <summary>
        /// Handles the Click event of the Order of Field Entry menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void orderOfFieldEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabOrderDialog tabOrder = new TabOrderDialog(projectExplorer.SelectedPage, projectExplorer.SelectedPage.TabOrderForFields);
            tabOrder.ShowDialog();
        }

        /// <summary>
        /// Handles the Click event of the Copy View menu item
        /// </summary>
        /// <param name="sender">Object that fied the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void copyViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyViewDialog dialog = new CopyViewDialog(this);
            dialog.ShowDialog();
        }

        /// <summary>
        /// Handles the Click event of the Background Tool menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void backgroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BackgroundDialog dialog = new BackgroundDialog(this);
            dialog.SettingsSaved += new EventHandler(this.canvas.OnSettingsSaved);
            dialog.ShowDialog();
        }

        /// <summary>
        /// Handles the Click event of the Settings Tool menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GridSettingsDialog dialog = new GridSettingsDialog(this);
            dialog.SettingsSaved += new EventHandler(this.canvas.OnSettingsSaved);
            dialog.ShowDialog();
            if (this.PageChanged != null)
            {
                GridSettingsChanged(this, new EventArgs());
            }
        }

        /// <summary>
        /// Handles the Click event of the Page Setup menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void pageSetupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Graphics graphics = this.CreateGraphics();
            //float dpi_x = graphics.DpiX;
            //float dpi_y = graphics.DpiY;
            
            Epi.Windows.MakeView.Dialogs.PageSetupDialog dialog = new Epi.Windows.MakeView.Dialogs.PageSetupDialog(this);
            dialog.SettingsSaved += new EventHandler(this.canvas.OnSettingsSaved);
            dialog.ShowDialog();
        }

        /// <summary>
        /// Handles the Click event of the Make View From Data Table menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void makeViewFromDataTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.projectExplorer != null)
            {
                if (this.projectExplorer.currentPage != null)
                {                    
                    ReadDialog dialog = new ReadDialog(this);
                    dialog.ShowDialog();
                    if (!string.IsNullOrEmpty(dialog.SelectedTable))
                    {
                        Epi.Data.IDbDriver DB = dialog.SelectedDataSource;
                        DataTable DT = DB.GetTopTwoTable(dialog.SelectedTable);                        

                        List<Epi.ImportExport.ColumnConversionInfo> columnMapping = new List<Epi.ImportExport.ColumnConversionInfo>();

                        int runningPageNumber = 1;
                        int runningTabIndex = 1;
                        int runningTopPosition = 2;

                        foreach (DataColumn dc in DT.Columns)
                        {
                            string validName = dc.ColumnName;
                            string validationMessage = string.Empty;

                            if (validName.Contains(" "))
                            {
                                validName = Util.Squeeze(validName);

                                int iterator = 1;
                                while (DT.Columns.Contains(validName))
                                {
                                    validName = validName + iterator.ToString();
                                }
                            }

                            if (!Util.IsFirstCharacterALetter(validName))
                            {
                                validName = "N" + validName;
                                int iterator = 1;
                                while (DT.Columns.Contains(validName))
                                {
                                    validName = validName + iterator.ToString();
                                }
                            }

                            if (Epi.Data.Services.AppData.Instance.IsReservedWord(validName))
                            {
                                validName = validName + "_RW";
                                int iterator = 1;
                                while (DT.Columns.Contains(validName))
                                {
                                    validName = validName + iterator.ToString();
                                }
                            }

                            Epi.ImportExport.ColumnConversionInfo cci = new Epi.ImportExport.ColumnConversionInfo();
                            cci.SourceColumnName = dc.ColumnName;
                            cci.DestinationColumnName = validName;
                            switch(dc.DataType.ToString()) 
                            {
                                case "System.String":
                                    cci.SourceColumnType = DbType.String;
                                    cci.FieldType = MetaFieldType.Text;
                                    break;
                                case "System.Byte":
                                    cci.SourceColumnType = DbType.Byte;
                                    cci.FieldType = MetaFieldType.YesNo;
                                    break;
                                case "System.Boolean":
                                    cci.SourceColumnType = DbType.Boolean;
                                    cci.FieldType = MetaFieldType.Checkbox;
                                    break;
                                case "System.Int16":
                                    cci.SourceColumnType = DbType.Int16;
                                    cci.FieldType = MetaFieldType.Number;
                                    break;
                                case "System.Int32":
                                    cci.SourceColumnType = DbType.Int32;
                                    cci.FieldType = MetaFieldType.Number;
                                    break;
                                case "System.Int64":
                                    cci.SourceColumnType = DbType.Int64;
                                    cci.FieldType = MetaFieldType.Number;
                                    break;
                                case "System.Double":
                                    cci.SourceColumnType = DbType.Double;
                                    cci.FieldType = MetaFieldType.Number;
                                    break;
                                case "System.Single":
                                    cci.SourceColumnType = DbType.Single;
                                    cci.FieldType = MetaFieldType.Number;
                                    break;
                                case "System.Decimal":
                                    cci.SourceColumnType = DbType.Decimal;
                                    cci.FieldType = MetaFieldType.Number;
                                    break;
                                case "System.DateTime":
                                    cci.SourceColumnType = DbType.DateTime;
                                    cci.FieldType = MetaFieldType.DateTime;
                                    break;
                            }

                            if (columnMapping.Count > 0 && columnMapping.Count % 24 == 0)
                            {
                                runningPageNumber++;
                                runningTabIndex = 1;                                
                            }

                            cci.ControlLeftPosition = 10;
                            cci.PromptLeftPosition = 10;
                            cci.ControlTopPosition = runningTopPosition + 1;
                            cci.PromptTopPosition = runningTopPosition;

                            cci.Prompt = cci.DestinationColumnName;
                            cci.PageNumber = runningPageNumber;
                            cci.TabIndex = runningTabIndex;
                            cci.IsTabStop = true;
                            columnMapping.Add(cci);

                            runningTabIndex++;
                            runningTopPosition = runningTopPosition + 3;
                        }

                        Epi.ImportExport.Dialogs.TableToViewDialog tableToViewDialog = new Epi.ImportExport.Dialogs.TableToViewDialog(dialog.SelectedTable, dialog.NewViewName, DB, columnMapping);
                        DialogResult result = tableToViewDialog.ShowDialog();

                        // only proceed if columns are selected
                        if (result == System.Windows.Forms.DialogResult.OK)
                        {
                            List<Epi.ImportExport.ColumnConversionInfo> columnMappings = tableToViewDialog.ColumnMappings;
                            Epi.Windows.Dialogs.TableToFormProgressDialog progressDialog = new TableToFormProgressDialog(this.mediator.Project, dialog.NewViewName, DB, dialog.SelectedTable, columnMappings);

                            progressDialog.StartPosition = FormStartPosition.CenterScreen;
                            progressDialog.ShowDialog();

                            Project project = this.projectExplorer.currentPage.view.Project;
                            View view = project.views[dialog.NewViewName];
                            this.projectExplorer.AddView(view);
                        }
                    }
                }
                else
                {
                    MsgBox.ShowInformation(SharedStrings.SELECT_PROJECT);
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the Page toolstrip menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void groupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mediator.OnNewGroupFieldClick();
        }

        /// <summary>
        /// Handles the Click event of the Check Code toolstrip menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void checkCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mediator.OnCheckCodeButtonClick();
        }

        /// <summary>
        /// Handles the Click event of the Make View toolstrip menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void exitMakeViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mediator.OkayToClearClipboard())
            {
                if (projectExplorer.IsProjectLoaded)
                {
                    if (CloseCurrentProject() == false)
                    {
                        return;
                    }
                }
                CloseCurrentMakeView();
            }
        }

        /// <summary>
        /// Handles the Click event of the About Epi Info menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void aboutEpiInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutEpiInfoDialog dialog = new AboutEpiInfoDialog(this);
            dialog.ShowDialog();
        }

        /// <summary>
        /// Handles the Click event of the Option toolstrip menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsDialog dialog = new OptionsDialog(this);
            dialog.ShowDialog();
            Configuration config = Configuration.GetNewInstance();

            if (config.Settings.Republish_IsRepbulishable && (!string.IsNullOrEmpty(config.Settings.WebServiceEndpointAddress)))
                {
                QuickPublishtoolStripButton.Visible = true;
                ChangeModetoolStripDropDownButton.Visible = true;
                toolStripSeparator10.Visible = true;               
                }
            if (config.Settings.Republish_IsRepbulishable && (!string.IsNullOrEmpty(config.Settings.EWEServiceEndpointAddress)))
            {
                QuickPublishtoolStripButton.Visible = true;
                ChangeModetoolStripDropDownButton.Visible = true;                
                toolStripSeparator11.Visible = true;
            }
            else
                {
                QuickPublishtoolStripButton.Visible = false;
                ChangeModetoolStripDropDownButton.Visible = false;
                toolStripSeparator10.Visible = false;
                toolStripSeparator11.Visible = false;

                }
        }

        BackgroundWorker worker1 = null;
        SplashScreenForm sf1 = null;
        /// <summary>
        /// Handles the Click event of the Enter Data button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void btnEnterData_Click(object sender, EventArgs e)
        {
            if (mediator.OkayToClearClipboard())
            {
                if (this.mediator.ProjectExplorer.SelectedPage != null)
                {
                    if (this.mediator.ProjectExplorer.SelectedPage.GetView().IsRelatedView == false)
                    {
                        if (CreateViewDataTable())
                        {
                            try
                            {
                                UpdateWindowTitle();
                                this.Hide();
                                worker1 = new BackgroundWorker();
                                worker1.WorkerSupportsCancellation = true;
                                worker1.DoWork += new DoWorkEventHandler(worker1_DoWork);
                                worker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker1_RunWorkerCompleted);
                                worker1.RunWorkerAsync();
                                Application.DoEvents();
                                View currentView = this.mediator.ProjectExplorer.SelectedPage.GetView();
                                if (this.mediator.Project.CollectedData.TableExists(currentView.TableName))
                                {
                                    mediator.LoadEnter();
                                }
                                else
                                {

                                    IModule module = this.ModuleManager.CreateModuleInstance("Enter");
                                    module.Load(this.ModuleManager, null);
                                    mediator.CloseAll();
                                }
                            }
                            finally
                            {
                            }
                            this.Dispose();
                        }

                    }
                    else
                    {
                        View relatedView = this.mediator.ProjectExplorer.SelectedPage.GetView();
                        View parentView = relatedView.ParentView;
                        if (parentView != null)
                        {
                            MsgBox.ShowInformation(string.Format(SharedStrings.CANNOT_OPEN_VIEW_RELATED_NAMES, parentView.Name));
                        }
                        else
                        {
                            MsgBox.ShowInformation(string.Format(SharedStrings.CANNOT_OPEN_VIEW_RELATED));
                        }
                    }
                }
            }
        }           

        private void worker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (sf1 == null)
            {
                sf1 = new SplashScreenForm();
                sf1.ShowDialog();
            }

            if (worker1.CancellationPending == true)
                e.Cancel = true;
        }

        private void worker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (sf1 != null)
            {
                sf1.Close();
                sf1.Dispose();
                worker1.DoWork -= new DoWorkEventHandler(worker1_DoWork);
                worker1.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(worker1_RunWorkerCompleted);
            }
        }

        /// <summary>
        /// Handles the Click event of the Check Code button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void btnCheckCode_Click(object sender, EventArgs e)
        {
            mediator.OnCheckCodeButtonClick();
        }

        /// <summary>
        /// Handles the Activate event of the About Epi Info menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mniAboutEpiInfo_Activate(object sender, EventArgs e)
        {
            OnAboutClicked();
        }

        /// <summary>
        /// Handles the Activiate event of the Options menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mbiOptions_Activate(object sender, EventArgs e)
        {
            OnOptionsClicked();
        }

        /// <summary>
        /// Handles the Activate event of the Exit menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuExit_Activate(object sender, EventArgs e)
        {
            OnExit();
        }

        /// <summary>
        /// Handles the Click event of the Close Project toolstrip menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void closeProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mediator.OkayToClearClipboard())
            {
                if (CloseCurrentProject() == false)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the insert page menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void insertPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            projectExplorer.InsertPage();
        }

        /// <summary>
        /// Handles the Click event of menu items on the status bar
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void statusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnViewStatusBarClicked(sender as ToolStripMenuItem);
        }

        /// <summary>
        /// Handles the Click event of the Show Log menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void epiInfoLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WinUtil.OpenTextFile(Logger.GetLogFilePath());
        }

        /// <summary>
        /// Handles the Click event of the Set Default Font menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void setDefaultFontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fontName = string.Empty;
            decimal fontSize = 0;
            bool fontBold = false;
            bool fontItalic = false;
            FontStyle style = FontStyle.Regular;
            Configuration config = Configuration.GetNewInstance();
            fontName = config.Settings.EditorFontName;
            fontSize = config.Settings.EditorFontSize;
            fontBold = config.Settings.EditorFontBold;
            fontItalic = config.Settings.EditorFontItalics;
            if (fontBold)
            {
                style |= FontStyle.Bold;
            }
            if (fontItalic)
            {
                style |= FontStyle.Italic;
            }
            currentFont = new Font(fontName, (float)fontSize, style);
            FontDialog fnt = new FontDialog();
            fnt.Font = currentFont;
            fnt.ShowEffects = false;
            DialogResult result = fnt.ShowDialog();
            if (result == DialogResult.OK)
            {
                fontName = fnt.Font.Name;
                try
                {
                    fontSize = (decimal)fnt.Font.Size;
                }
                catch (Exception ex)
                {
                    fontSize = config.Settings.EditorFontSize;
                    //Logger.Log(ex.Message);
                }
                config.Settings.EditorFontSize = fontSize;
                config.Settings.EditorFontName = fontName;
                if (fnt.Font.Bold)
                {
                    config.Settings.EditorFontBold = true;
                }
                else
                {
                    config.Settings.EditorFontBold = false;
                }
                if (fnt.Font.Italic)
                {
                    config.Settings.EditorFontItalics = true;
                }
                else
                {
                    config.Settings.EditorFontItalics = false;
                }
                Configuration.Save(config);
            }
        }

        /// <summary>
        /// Handles the Click event of the Set Default Control Font menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void setDefaultControlFontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fontName = string.Empty;
            decimal fontSize = 0;
            bool fontBold = false;
            bool fontItalic = false;
            FontStyle style = FontStyle.Regular;
            Configuration config = Configuration.GetNewInstance();
            fontName = config.Settings.ControlFontName;
            fontSize = config.Settings.ControlFontSize;
            fontBold = config.Settings.ControlFontBold;
            fontItalic = config.Settings.ControlFontItalics;
            if (fontBold)
            {
                style |= FontStyle.Bold;
            }
            if (fontItalic)
            {
                style |= FontStyle.Italic;
            }
            currentFont = new Font(fontName, (float)fontSize, style);
            FontDialog fnt = new FontDialog();
            fnt.Font = currentFont;
            fnt.ShowEffects = false;
            DialogResult result = fnt.ShowDialog();
            if (result == DialogResult.OK)
            {
                fontName = fnt.Font.Name;
                try
                {
                    fontSize = (decimal)fnt.Font.Size;
                }
                catch (Exception ex)
                {
                    fontSize = config.Settings.ControlFontSize;
                    //Logger.Log(ex.Message);
                }
                config.Settings.ControlFontSize = fontSize;
                config.Settings.ControlFontName = fontName;
                if (fnt.Font.Bold)
                {
                    config.Settings.ControlFontBold = true;
                }
                else
                {
                    config.Settings.ControlFontBold = false;
                }
                if (fnt.Font.Italic)
                {
                    config.Settings.ControlFontItalics = true;
                }
                else
                {
                    config.Settings.ControlFontItalics = false;
                }
                Configuration.Save(config);
            }
        }

        /// <summary>
        /// Handles the click event for the Vertical Alignment tool strip menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void verticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mediator.OnSelectedControlsAlignAsTable(1);
        }

        /// <summary>
        /// Handles the click event for the Horizontal Alignment tool strip menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void horizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mediator.OnSelectedControlsAlignAsTable(2);
        }

        /// <summary>
        /// Handles the Click event of the 'Delete Data Table' menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void deleteDataTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mediator.Project != null)
            {
                View currentView = projectExplorer.SelectedPage.GetView();
                DialogResult rt = MessageBox.Show(this, SharedStrings.DELETE_DATA_TABLE_WARNING, SharedStrings.DELETE_DATA_TABLE, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (rt == DialogResult.Yes)
                {
                    try
                    {
                        this.Cursor = Cursors.WaitCursor;

                        currentView.DeleteDataTables();                        

                        foreach (View view in currentView.GetDescendantViews())
                        {
                            if (this.mediator.Project.CollectedData.TableExists(view.TableName))
                            {
                                view.DeleteDataTables();
                            }
                        }

                        this.Cursor = Cursors.Default;
                    }
                    catch
                    {
                        this.Cursor = Cursors.Default;
                        MessageBox.Show(this, string.Format(SharedStrings.DATA_TABLE_NOT_FOUND, currentView.Name), SharedStrings.MAKE_VIEW, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                }
            }
            SetDataTableMenuItems();
        }

        /// <summary>
        /// Handles the Click event of the 'Data Dictionary' menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void displayDataDictionaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            View currentView = projectExplorer.SelectedPage.GetView();
            DataDictionary dataDictionaryDialog = new DataDictionary(currentView, this);
            DialogResult dialogResult = dataDictionaryDialog.ShowDialog();
        }

        /// <summary>
        /// Handles the click event for 'Cut'
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void editCut_Click(object sender, EventArgs e)
        {
            mediator.OnClipboardCutSelection();
        }

        /// <summary>
        /// Handles the click event for 'Copy'
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void editCopy_Click(object sender, EventArgs e)
        {
            mediator.OnClipboardCopySelection();
        }

        /// <summary>
        /// Handles the click event for 'Paste'
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void editPaste_Click(object sender, EventArgs e)
        {
            mediator.OnClipboardPaste(new Point(12, 12));
        }

        /// <summary>
        /// Handles the click event for items on the Edit menu
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openForViewingOnly)
            {
                mnuEditCut.Enabled = false;
                mnuEditCopy.Enabled = false;
                mnuEditPaste.Enabled = false;
            }
            else
            {
                mnuEditCut.Enabled = mediator.IsSelectedControlEmpty ? false : true;
                mnuEditCopy.Enabled = mediator.IsSelectedControlEmpty ? false : true;
                mnuEditPaste.Enabled = mediator.IsControlClipboardEmpty ? false : true;
            }
        }

        /// <summary>
        /// Handles the click event for the 'Create Data Table' menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void createDataTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateViewDataTable();
            SetDataTableMenuItems();
        }

        /// <summary>
        /// Handles the click event for for the Epi Info 3.5.x importer
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void Import35xToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = SharedStrings.EI3X_PROJECT_FILES + " (*.mdb)|*.mdb";

            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;
            Configuration config = Configuration.GetNewInstance();
            openFileDialog.InitialDirectory = "c:\\epi_info\\";// config.Directories.Project;
            DialogResult res = openFileDialog.ShowDialog();
            if (res == DialogResult.OK)
            {
                if (projectExplorer.IsProjectLoaded)
                {
                    if (CloseCurrentProject() == false)
                    {
                        return;
                    }
                }

                string filePath = openFileDialog.FileName.Trim();
                if (filePath.ToLower(System.Globalization.CultureInfo.CurrentCulture).EndsWith(FileExtensions.MDB))
                {
                    Type UpgradeAssistant = Type.GetType("Epi.Windows.ImportExport.UpgradeAssistant, Epi.Windows.ImportExport", true, true);
                    Project project = null;
                    try
                    {
                        project = UpgradeAssistant.InvokeMember("UpgradeEpi2000Project", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new object[] { filePath, this }) as Project;
                        if (project != null) OpenProject(project);
                    }
                    catch (Exception ex)
                    {
                        MsgBox.ShowException(ex);
                    }
                    finally
                    {
                        EndBusy();
                    }

                }
            }
        }

        /// <summary>
        /// Handles the click event for the main tool strip
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void MainToolStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
        }

        /// <summary>
        /// Handles the click event for vocabulary fields
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void VocabularyFieldsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (vocabularyFieldsToolStripMenuItem.Checked)
            {
                this.projectExplorer.SetIsLinkedFieldVisibile(true);
            }
            else
            {
                this.projectExplorer.SetIsLinkedFieldVisibile(false);
            }
        }

        private void NewProjectMenuItem_Click(object sender, EventArgs e)
        {
            if (projectExplorer.IsProjectLoaded)
            {
                if (mediator.OkayToClearClipboard())
                {
                    if (CloseCurrentProject() == false)
                    {
                        return;
                    }
                    else
                    {
                        mediator.Reset();
                        RunWizardForNewProject();
                    }
                }
            }
            else
            {
                mediator.Reset();
                RunWizardForNewProject();
            }
        }

        private void NewProjectFromTemplateMenuItem_Click(object sender, EventArgs e)
        {
            NewProjectFromTemplate();
        }

        public void NewProjectFromTemplate(string path = "" )
        {
            RunWizardForNewProjectFromTemplate(path);
        }

        private void NewViewMenuItem_Click(object sender, EventArgs e)
        {
            projectExplorer.AddView();
        }

        private void NewPageMenuItem_Click(object sender, EventArgs e)
        {
            projectExplorer.AddPageToCurrentView();
        }

        #endregion // Event Handlers

        private void mnuCopyToPhone_Click(object sender, EventArgs e)
        {
            Dialogs.CopyToAndroid dialog = new CopyToAndroid(CurrentView, this.mediator);
            dialog.ShowDialog();
        }

        private void mnuImportTemplate_Click(object sender, EventArgs e)
        {
            GetTemplate();
        }

        public void GetTemplate()
        {
            GetTemplate(string.Empty);
        }

        public void GetTemplate(string initialTemplatePath)
        {
            string templatePath = string.Empty;
            string saveTo = string.Empty;
            FileStream fileStream = null;
            FileStream readFileStream = null;
            FileStream writeFileStream = null;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = SharedStrings.PE_TEMPLATES + " (*.eit;*.xml)|*.eit;*.xml|All files (*.*)|*.*";

            if (string.IsNullOrEmpty(initialTemplatePath) == false)
            {
                dialog.InitialDirectory = Path.GetDirectoryName(initialTemplatePath);
                dialog.FileName = Path.GetFileName(initialTemplatePath);
            }

            string safeFileName = dialog.SafeFileName;
            string readFilePath = string.Empty;

            if (string.IsNullOrEmpty(initialTemplatePath) )
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    readFilePath = dialog.FileName;
                    safeFileName = dialog.SafeFileName;
                }
            }
            else
            {
                readFilePath = initialTemplatePath;
            }

            if (string.IsNullOrEmpty(readFilePath) == false)
            {
                try
                {
                    fileStream = new FileStream(readFilePath, FileMode.Open);

                    if (true)
                    {
                        using (fileStream)
                        {
                            XmlTextReader reader = new XmlTextReader(fileStream);

                            string targetFolderName = string.Empty;
                            string newNameCandidate = string.Empty;

                            while (reader.ReadToFollowing("Template"))
                            {
                                if (reader.MoveToAttribute("Level"))
                                {
                                    switch (reader.Value)
                                    {
                                        case "Project":
                                            targetFolderName = "Projects";
                                            break;
                                        case "Form":
                                        case "View":
                                            targetFolderName = "Forms";
                                            break;
                                        case "Page":
                                            targetFolderName = "Pages";
                                            break;
                                        case "Field":
                                            targetFolderName = "Fields";
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }

                            if (string.IsNullOrEmpty(targetFolderName))
                            {
                                MessageBox.Show(SharedStrings.INVALID_TEMPLATE_FILE);
                            }
                            else
                            {
                                string programFilesDirectoryName = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles).ToLower();
                                string installFolder = AppDomain.CurrentDomain.BaseDirectory.ToLower();

                                Configuration config = Configuration.GetNewInstance();
                                string configPath = config.Directories.Templates;

                                if (configPath != string.Empty)
                                {
                                    templatePath = Path.Combine(configPath, targetFolderName);
                                }
                                else if (installFolder.ToLower().StartsWith(programFilesDirectoryName))
                                {
                                    templatePath = Path.Combine(installFolder, "Templates\\" + targetFolderName);
                                }
                                else
                                {
                                    string asmPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                                    templatePath = asmPath + targetFolderName;
                                }

                                if (Directory.Exists(templatePath) == false)
                                {
                                    Directory.CreateDirectory(templatePath);
                                }

                                saveTo = Path.Combine(templatePath, safeFileName);

                                if (File.Exists(saveTo))
                                {
                                    newNameCandidate = string.Format("{0}_{1}.xml", safeFileName.Replace(".xml", "").Replace(".eit", ""), DateTime.Now.ToString("yyyyMMddhmmsstt"));
                                    string text = SharedStrings.TEMPLATE_GET_ALREADY_EXISTS
                                        + Environment.NewLine
                                        + string.Format("[{0}]", safeFileName)
                                        + Environment.NewLine
                                        + Environment.NewLine
                                        + SharedStrings.TEMPLATE_GET_RENAME_TEMPLATE
                                        + Environment.NewLine
                                        + Environment.NewLine
                                        + SharedStrings.TEMPLATE_GET_YES_RENAME
                                        + Environment.NewLine
                                        + string.Format("[{0}]", newNameCandidate)
                                        + Environment.NewLine
                                        + Environment.NewLine
                                        + SharedStrings.TEMPLATE_GET_NO_OVERWRITE
                                        + Environment.NewLine
                                        + Environment.NewLine
                                        + SharedStrings.TEMPLATE_GET_CANCEL;

                                    DialogResult overwrite = MessageBox.Show(text, SharedStrings.TEMPLATE_GET_TITLE, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                                    if (overwrite == System.Windows.Forms.DialogResult.Yes)
                                    {
                                        saveTo = Path.Combine(templatePath, newNameCandidate);
                                    }
                                    else if (overwrite == System.Windows.Forms.DialogResult.Cancel)
                                    {
                                        return;
                                    }
                                }

                                fileStream.Close();

                                try
                                {
                                    readFileStream = new FileStream(readFilePath, FileMode.Open);
                                    writeFileStream = new FileStream(saveTo, FileMode.Create, FileAccess.Write);
                                    int Length = 256;
                                    Byte[] buffer = new Byte[Length];
                                    int bytesRead = readFileStream.Read(buffer, 0, Length);

                                    while (bytesRead > 0)
                                    {
                                        writeFileStream.Write(buffer, 0, bytesRead);
                                        bytesRead = readFileStream.Read(buffer, 0, Length);
                                    }
                                }
                                finally
                                {
                                    readFileStream.Close();
                                    writeFileStream.Close();
                                }
                            }

                            if (string.IsNullOrEmpty(targetFolderName) == false)
                            {
                                if (targetFolderName.Equals("Projects"))
                                {
                                    RunWizardForNewProjectFromTemplate(saveTo);
                                }
                                else
                                {
                                    projectExplorer.UpdateTemplates();


                                    string message = string.Format("{0}", safeFileName);

                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(SharedStrings.ERROR + ": " + SharedStrings.TEMPLATE_CANNOT_READ_FILE + Environment.NewLine + ex.Message);
                }
                finally
                {
                    fileStream.Close();
                }
            }
        }

        private void makeAccessProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.CheckFileExists = true;
            openFileDialog.Filter = SharedStrings.MS_ACCESS_FILES + " (*.mdb)|*.mdb";

            DialogResult result = openFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                FileInfo fi = new FileInfo(openFileDialog.FileName);
                string projectName = fi.Directory.ToString() + "\\";
                projectName = projectName + (fi.Name.Replace(fi.Extension, string.Empty)) + ".prj";

                //project.Name = fi.Name.Substring(0, fi.Name.Length - 4);
                //project.Location = directory; 

                if (File.Exists(projectName))
                {
                    DialogResult overwriteResult = Epi.Windows.MsgBox.Show(string.Format(SharedStrings.PROJECT_ALREADY_EXISTS, projectName), SharedStrings.PROJECT_ALREADY_EXISTS_TITLE, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (overwriteResult != System.Windows.Forms.DialogResult.Yes)
                    {
                        return;
                    }
                }

                if (Util.CreateProjectFileFromDatabase(openFileDialog.FileName, true) != null)
                {
                    Epi.Windows.MsgBox.ShowInformation(SharedStrings.PROJECT_FILE_CREATED);
                }
                else
                {
                    Epi.Windows.MsgBox.ShowInformation(SharedStrings.PROJECT_FILE_NOT_CREATED);
                }
            }
        }

        private void makeSQLProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Epi.Data.IDbDriverFactory dbFactory = Epi.Data.DbDriverFactoryCreator.GetDbDriverFactory(Configuration.SqlDriver);
            string connectionString = string.Empty;
            System.Data.Common.DbConnectionStringBuilder dbCnnStringBuilder = new System.Data.Common.DbConnectionStringBuilder();
            Epi.Data.IDbDriver db = dbFactory.CreateDatabaseObject(dbCnnStringBuilder);
                //IDbDriver db = DatabaseFactoryCreator.CreateDatabaseInstance(selectedPlugIn.Key);
                //IConnectionStringGui dialog = db.GetConnectionStringGuiForExistingDb();
                ////IConnectionStringGui dialog = this.selectedProject.Metadata.DBFactory.GetConnectionStringGuiForExistingDb();   
            Epi.Data.IConnectionStringGui dialog = dbFactory.GetConnectionStringGuiForExistingDb();      

            DialogResult result = ((Form)dialog).ShowDialog();
            
            bool success = false;

            if (result == DialogResult.OK)
            {
                
                db.ConnectionString = dialog.DbConnectionStringBuilder.ToString();
                connectionString = db.ConnectionString;

                try
                {
                    success = db.TestConnection();
                }
                catch
                {
                    success = false;
                    MessageBox.Show(SharedStrings.ERROR_CONNECT_DATA_SOURCE);
                }   
            }

            if (success)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.CheckFileExists = false;
                openFileDialog.Filter = "Epi Info " + SharedStrings.PROJECT_FILE + " (*.prj)|*.prj";

                result = openFileDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string projectName = openFileDialog.FileName;
                    if (File.Exists(projectName))
                    {
                        DialogResult overwriteResult = Epi.Windows.MsgBox.Show(string.Format(SharedStrings.PROJECT_ALREADY_EXISTS, projectName), SharedStrings.PROJECT_ALREADY_EXISTS_TITLE, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (overwriteResult != System.Windows.Forms.DialogResult.Yes)
                        {
                            return;
                        }
                    }

                    FileInfo fi = new FileInfo(openFileDialog.FileName);

                    if (Util.CreateProjectFileFromDatabase(connectionString, true, fi.DirectoryName, fi.Name) != null)
                    {
                        Epi.Windows.MsgBox.ShowInformation(SharedStrings.PROJECT_FILE_CREATED);
                    }
                    else
                    {
                        Epi.Windows.MsgBox.ShowError(SharedStrings.PROJECT_FILE_NOT_CREATED);
                    }
                }
            }      
        }

        private void contentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://wwwn.cdc.gov/epiinfo/user-guide/form-designer/Introduction.html");
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void publishNewSurveyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Template template = new Template(this.mediator);
            string InvalidForPublishing = ListFieldsNotSupportedForWeb();
            View view;

            if (projectExplorer.CurrentView != null)
            {
                view = projectExplorer.CurrentView;
            }
            else
            {
                view = projectExplorer.SelectedPage.view;
            }
            
            if (InvalidForPublishing.Length > 0)
            {
                SupportedFieldTypeDialog dialog = new SupportedFieldTypeDialog(InvalidForPublishing);
                dialog.ShowDialog();
            }
            else
            {
                Configuration config = Configuration.GetNewInstance();
                    
                try
                {
                    if (config.Settings.Republish_IsRepbulishable == true)
                    {
                        if (string.IsNullOrWhiteSpace(this.OrganizationKey) && !string.IsNullOrWhiteSpace(view.WebSurveyId))
                        {
                            Epi.Core.ServiceClient.ServiceClient.IsValidOrganizationKeyEnum IsValidOKey = Epi.Core.ServiceClient.ServiceClient.IsValidOrganizationKeyEnum.No;

                            if (string.IsNullOrWhiteSpace(view.WebSurveyId))
                            {
                                IsValidOKey = Epi.Core.ServiceClient.ServiceClient.IsValidOrgKey(this.OrganizationKey);
                            }
                            else
                            {
                                IsValidOKey = Epi.Core.ServiceClient.ServiceClient.IsValidOrgKey(this.OrganizationKey, view.WebSurveyId);
                            }

                            WebPublishDialog dialog = null;
                            WebSurveyOptions wso = null;

                            switch (IsValidOKey)
                            {
                                case Epi.Core.ServiceClient.ServiceClient.IsValidOrganizationKeyEnum.No:
                                    OrgKey NewDialog = new OrgKey(this.CurrentView.WebSurveyId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can republish this survey.");
                                    DialogResult result = NewDialog.ShowDialog();

                                    if (result == System.Windows.Forms.DialogResult.OK)
                                    {
                                        this.OrganizationKey = NewDialog.OrganizationKey;
                                        if (!string.IsNullOrWhiteSpace(OrganizationKey))
                                        {
                                            SetSurveyInfo();
                                            dialog = new WebPublishDialog(this.OrganizationKey, this.mediator, view, template.CreateWebSurveyTemplate());
                                            dialog.ShowDialog();
                                        }
                                    }
                                    break;

                                case Epi.Core.ServiceClient.ServiceClient.IsValidOrganizationKeyEnum.EndPointNotFound:
                                    WebSurveyOptions dialog1 = new WebSurveyOptions();
                                    DialogResult result1 = dialog1.ShowDialog();

                                    if (result1 == System.Windows.Forms.DialogResult.OK)
                                    {
                                        OrgKey OrgKeyDialog = new OrgKey(this.CurrentView.WebSurveyId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can republish this survey.");
                                        DialogResult result2 = OrgKeyDialog.ShowDialog();
                                        if (result2 == System.Windows.Forms.DialogResult.OK)
                                        {
                                            this.OrganizationKey = OrgKeyDialog.OrganizationKey;
                                            if (!string.IsNullOrWhiteSpace(OrganizationKey))
                                            {
                                                SetSurveyInfo();
                                                dialog = new WebPublishDialog(this.OrganizationKey, this.mediator, view, template.CreateWebSurveyTemplate());
                                                dialog.ShowDialog();
                                            }
                                        }
                                    }
                                    break;

                                case Epi.Core.ServiceClient.ServiceClient.IsValidOrganizationKeyEnum.GeneralException:
                                    WebSurveyOptions dialog2 = new WebSurveyOptions();
                                    DialogResult result3 = dialog2.ShowDialog();
                                    if (result3 == System.Windows.Forms.DialogResult.OK)
                                    {
                                        OrgKey OrgKeyDialog = new OrgKey(this.CurrentView.WebSurveyId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can republish this survey.");
                                        DialogResult result4 = OrgKeyDialog.ShowDialog();
                                        if (result4 == System.Windows.Forms.DialogResult.OK)
                                        {
                                            this.OrganizationKey = OrgKeyDialog.OrganizationKey;
                                            if (!string.IsNullOrWhiteSpace(OrganizationKey))
                                            {
                                                SetSurveyInfo();
                                                dialog = new WebPublishDialog(this.OrganizationKey, this.mediator, view, template.CreateWebSurveyTemplate());
                                                dialog.ShowDialog();
                                            }
                                        }
                                    }
                                    break;

                                case Epi.Core.ServiceClient.ServiceClient.IsValidOrganizationKeyEnum.Yes:
                                    SetSurveyInfo();
                                    dialog = new WebPublishDialog(this.OrganizationKey, this.mediator, view, template.CreateWebSurveyTemplate());
                                    dialog.ShowDialog();
                                    break;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(this.OrganizationKey))
                            {
                            WebPublishDialog dialog = new WebPublishDialog(this.OrganizationKey, this.mediator, view, template.CreateWebSurveyTemplate());
                                dialog.ShowDialog();
                            }
                            else
                            {
                                try
                                {
                                SurveyManagerService.ManagerServiceV3Client client = Epi.Core.ServiceClient.ServiceClient.GetClient();
                                    SurveyManagerService.OrganizationRequest Request = new SurveyManagerService.OrganizationRequest();
                                    SurveyManagerService.OrganizationDTO orgDTO = new SurveyManagerService.OrganizationDTO();
                                    Request.Organization = orgDTO;
                                    var TestService = client.GetOrganization(Request);

                                    WebPublishDialog dialog = new WebPublishDialog(null, this.mediator, view, template.CreateWebSurveyTemplate());
                                    DialogResult result = dialog.ShowDialog();
                                    
                                    if (result == System.Windows.Forms.DialogResult.Cancel)
                                    {
                                        this.OrganizationKey = dialog.GetOrgKey;
                                    }
                                }
                                catch
                                {
                                    WebSurveyOptions dialog2 = new WebSurveyOptions();
                                    DialogResult result3 = dialog2.ShowDialog();
                                    if (result3 == System.Windows.Forms.DialogResult.OK)
                                    {
                                    WebPublishDialog dialog = new WebPublishDialog(this.OrganizationKey, this.mediator, view, template.CreateWebSurveyTemplate());
                                        DialogResult result = dialog.ShowDialog();
                                        if (result == System.Windows.Forms.DialogResult.Cancel)
                                        {
                                            this.OrganizationKey = dialog.GetOrgKey;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                        SurveyManagerService.ManagerServiceV3Client client = Epi.Core.ServiceClient.ServiceClient.GetClient();
                            SurveyManagerService.OrganizationRequest Request = new SurveyManagerService.OrganizationRequest();
                            SurveyManagerService.OrganizationDTO orgDTO = new SurveyManagerService.OrganizationDTO();
                            Request.Organization = orgDTO;
                            var TestService = client.GetOrganization(Request);

                            WebPublishDialog dialog = new WebPublishDialog(null, this.mediator, view, template.CreateWebSurveyTemplate());
                            DialogResult result = dialog.ShowDialog();
                            if (result == System.Windows.Forms.DialogResult.Cancel)
                            {
                                this.OrganizationKey = dialog.GetOrgKey;
                            }

                        }
                        catch
                        {
                            if (config.Settings.Republish_IsRepbulishable == true)
                            {
                                WebSurveyOptions dialog2 = new WebSurveyOptions();
                                DialogResult result3 = dialog2.ShowDialog();
                                if (result3 == System.Windows.Forms.DialogResult.OK)
                                {
                                WebPublishDialog dialog = new WebPublishDialog(this.OrganizationKey, this.mediator, view, template.CreateWebSurveyTemplate());
                                    DialogResult result = dialog.ShowDialog();
                                    if (result == System.Windows.Forms.DialogResult.Cancel)
                                    {
                                        this.OrganizationKey = dialog.GetOrgKey;
                                    }
                                }
                            }
                            else
                            {
                            WebPublishDialog dialog = new WebPublishDialog(null, this.mediator, view, template.CreateWebSurveyTemplate());
                                dialog.ShowDialog();
                            }
                        }
                    }
                }
                catch
                {
                WebPublishDialog dialog = new WebPublishDialog(null, this.mediator, view, template.CreateWebSurveyTemplate());
                    dialog.ShowDialog();
                }
            }

            this.SetPublishMenuItems(view);
        }

        private void changePublishModeToolStripMenuItem_Click(object sender, EventArgs e)
        {

            PublishMode dialog = new PublishMode(this.projectExplorer.CurrentView.WebSurveyId, this.OrganizationKey, this.UserPublishKey);
                dialog.ShowDialog();
           
        }

        private void updateSurveyInformationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Template template = new Template(this.mediator);
            string InvalidForPublishing = ListFieldsNotSupportedForWeb();
            if (InvalidForPublishing.Length > 0)
            {
                SupportedFieldTypeDialog dialog = new SupportedFieldTypeDialog(InvalidForPublishing);
                dialog.ShowDialog();
            }
            else
            {
            WebPublishDialog dialog = new WebPublishDialog(this.OrganizationKey, this.mediator, this.projectExplorer.CurrentView, template.CreateWebSurveyTemplate(), true);
                dialog.ShowDialog();
            }

            this.OrganizationKey = this.projectExplorer.CurrentView.CheckCodeBefore;
            this.projectExplorer.CurrentView.CheckCodeBefore = "";

        }

        private void updateSurveyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Template template = new Template(this.mediator);
            string InvalidForPublishing = ListFieldsNotSupportedForWeb();
            if (InvalidForPublishing.Length > 0)
            {
                SupportedFieldTypeDialog dialog = new SupportedFieldTypeDialog(InvalidForPublishing);
                dialog.ShowDialog();
            }
            else
            {
                RepublishSurveyFields dialog = new RepublishSurveyFields(this.OrganizationKey, this.projectExplorer.CurrentView, template.CreateWebSurveyTemplate());
                dialog.ShowDialog();
            }
        }

        private void getSurveyLinkKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetSurveyLink dialog = new GetSurveyLink();
            dialog.ShowDialog();
        }

        public void SetPublishMenuItems(View pView)
        {
            Configuration config = Configuration.GetNewInstance();
            DataTable table = mediator.Project.Metadata.GetPublishedViewKeys(pView.Id);
            DataRow viewRow = null;
            RepublishOrgKey = null;
            if (table != null)
                viewRow = table.Rows[0];
            else
            {
                toWebEnterToolStripMenuItem.Enabled = false;
                EWEToolStripMenuItem.Enabled = false;
                mnuPublishToWeb.Text = "Publish Form to Web";
                mnuPublishToWeb.Enabled = false;
            }
           
            try
            {                
                if (config.Settings.Republish_IsRepbulishable == true)
                {
                    QuickPublishtoolStripButton.Enabled = true;
                    ChangeModetoolStripDropDownButton.Enabled = true;
                    if (!string.IsNullOrWhiteSpace(pView.WebSurveyId))
                    {
                        mnuPublishToWeb.Text = "Republish Form to Web";
                        mnuPublishToWeb.Enabled = true; 

                        toWebSurveyToolStripMenuItem.Visible = true;
                        EIWSToolStripMenuItem.Visible = true;
                        toolStripSeparator10.Visible = true;
                        toolStripSeparator11.Visible = true; 
                        toWebSurveyToolStripMenuItem.Enabled = true;
                        EIWSToolStripMenuItem.Enabled = true;

                    }
                    else
                    {
                        mnuPublishToWeb.Text = "Publish Form to Web";
                        mnuPublishToWeb.Enabled = true;

                        toWebSurveyToolStripMenuItem.Enabled = false;
                        EIWSToolStripMenuItem.Enabled = false;
                    }
                    if (viewRow != null)
                    {
                        if (CheckforRepublishWebEnterMenuItem())
                        {

                            toolStripPublishToWebEnter.Text = "Republish Form to Web Enter";
                            // toolStripPublishToWebEnter.Enabled = true;
                            if (pView.IsRelatedView == true)
                            {
                                toWebEnterToolStripMenuItem.Enabled = false;
                                EWEToolStripMenuItem.Enabled = false;
                            }
                            else
                            {
                                toWebEnterToolStripMenuItem.Enabled = true;
                                EWEToolStripMenuItem.Enabled = true;
                            }
                            // toWebEnterToolStripMenuItem.Enabled = true;
                            //EWEToolStripMenuItem.Enabled = true;
                        }
                        else
                        {

                            toolStripPublishToWebEnter.Text = "Publish Form to Web Enter";
                            // toolStripPublishToWebEnter.Enabled = true;
                            toWebEnterToolStripMenuItem.Enabled = false;
                            EWEToolStripMenuItem.Enabled = false;
                        }

                        if (pView.IsRelatedView == true)
                            toolStripPublishToWebEnter.Enabled = false;
                        else
                            toolStripPublishToWebEnter.Enabled = true;
                    }
                }
                else
                {
                    mnuPublishToWeb.Text = "Publish Form to Web";
                    mnuPublishToWeb.Enabled = true;
                    toolStripPublishToWebEnter.Text = "Publish Form to Web Enter";
                    if (pView.IsRelatedView == true)
                        toolStripPublishToWebEnter.Enabled = false;
                    else
                        toolStripPublishToWebEnter.Enabled = true;
                    QuickPublishtoolStripButton.Enabled = false;
                    ChangeModetoolStripDropDownButton.Enabled = false;
                }
            }
            catch(Exception ex)
            {
                    mnuPublishToWeb.Text = "Publish Form to Web";
                    mnuPublishToWeb.Enabled = true;
                    toolStripPublishToWebEnter.Text = "Publish Form to Web Enter";
                    toolStripPublishToWebEnter.Enabled = true;
                    QuickPublishtoolStripButton.Enabled = false;
                    ChangeModetoolStripDropDownButton.Enabled = false;

            }

        }

        private bool CheckforRepublishWebEnterMenuItem()
        {
            foreach (View view in this.mediator.Project.Views)
            {
                DataTable table = mediator.Project.Metadata.GetPublishedViewKeys(view.Id);
               
                    DataRow ViewRow = table.Rows[0];
                   
                        if (!string.IsNullOrWhiteSpace(ViewRow.ItemArray[2].ToString()))
                        {
                            RepublishOrgKey = ViewRow.ItemArray[2].ToString();
                            return true;
                        }                                    
            }
            return false;
        }

        //private void QuickPublishtoolStripButton_Click(object sender, EventArgs e)
        //{
        //Template template = new Template(this.mediator);
        //try
        //    {
             
        //        Epi.Windows.MakeView.Utils.ServiceClient.IsValidOrganizationKeyEnum IsValidOKey = Epi.Windows.MakeView.Utils.ServiceClient.IsValidOrganizationKeyEnum.No;
        //        if (string.IsNullOrWhiteSpace(this.projectExplorer.CurrentView.WebSurveyId))
        //            {
        //            IsValidOKey = Epi.Windows.MakeView.Utils.ServiceClient.IsValidOrgKey(this.OrganizationKey);
        //            }
        //        else
        //            {
        //            IsValidOKey = Epi.Windows.MakeView.Utils.ServiceClient.IsValidOrgKey(this.OrganizationKey, this.projectExplorer.CurrentView.WebSurveyId);
        //            }
        //        WebPublishDialog dialog = null;
                
        //        switch (IsValidOKey)
        //            {
        //            case Epi.Windows.MakeView.Utils.ServiceClient.IsValidOrganizationKeyEnum.No:
        //                OrgKey NewDialog = new OrgKey(this.CurrentView.WebSurveyId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can republish this survey.");
        //                DialogResult result = NewDialog.ShowDialog();
        //                if (result == System.Windows.Forms.DialogResult.OK)
        //                    {
        //                    this.OrganizationKey = NewDialog.OrganizationKey;
        //                    if (!string.IsNullOrWhiteSpace(OrganizationKey))
        //                        {
        //                        SetSurveyInfo();
        //                        QuickSurveyInfoUpdate();
        //                        }
        //                    }
        //                break;
        //            case Epi.Windows.MakeView.Utils.ServiceClient.IsValidOrganizationKeyEnum.EndPointNotFound:
        //                WebSurveyOptions dialog1 = new WebSurveyOptions();
        //                DialogResult result1 = dialog1.ShowDialog();
        //                if (result1 == System.Windows.Forms.DialogResult.OK)
        //                    {
        //                    OrgKey OrgKeyDialog = new OrgKey(this.CurrentView.WebSurveyId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can republish this survey.");
        //                    DialogResult result2 = OrgKeyDialog.ShowDialog();
        //                    if (result2 == System.Windows.Forms.DialogResult.OK)
        //                        {
        //                        this.OrganizationKey = OrgKeyDialog.OrganizationKey;
        //                        if (!string.IsNullOrWhiteSpace(OrganizationKey))
        //                            {
        //                            SetSurveyInfo();
        //                            QuickSurveyInfoUpdate();
        //                            }
        //                        }
        //                    }
        //                break;
        //            case Epi.Windows.MakeView.Utils.ServiceClient.IsValidOrganizationKeyEnum.GeneralException:
        //                WebSurveyOptions dialog2 = new WebSurveyOptions();
        //                DialogResult result3 = dialog2.ShowDialog();
        //                if (result3 == System.Windows.Forms.DialogResult.OK)
        //                    {
        //                    OrgKey OrgKeyDialog = new OrgKey(this.CurrentView.WebSurveyId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can republish this survey.");
        //                    DialogResult result4 = OrgKeyDialog.ShowDialog();
        //                    if (result4 == System.Windows.Forms.DialogResult.OK)
        //                        {
        //                        this.OrganizationKey = OrgKeyDialog.OrganizationKey;
        //                        if (!string.IsNullOrWhiteSpace(OrganizationKey))
        //                            {
        //                            SetSurveyInfo();
        //                            QuickSurveyInfoUpdate();
        //                            }
        //                        }
        //                    }
        //                break;
        //            case Epi.Windows.MakeView.Utils.ServiceClient.IsValidOrganizationKeyEnum.Yes:
        //               SetSurveyInfo();
        //               QuickSurveyInfoUpdate();
        //                break;
        //            }
                
        //    }
        //catch (Exception ex)// not republishable
        //    {
           
        //    }



        //}
        public bool ValidateServiceProperties()
        {
            bool IsValid = false;
            if (!string.IsNullOrEmpty(WebServiceBindingMode) && !string.IsNullOrEmpty(WebServiceEndpointAddress))
            {
                IsValid = true;
            }
            else {
                 
                return false;
            }

            if (WebServiceAuthMode != null )
            {
                IsValid = true;
            }
            else
            {
               return false;

            }
            return IsValid;
        }
        private void draftToolStripMenuItem_Click(object sender, EventArgs e)
            {
                
            EIWSToolStripMenuItem.CheckState = CheckState.Unchecked;
           
            }
        private void finalToolStripMenuItem_Click(object sender, EventArgs e)
            {

            
            EWEToolStripMenuItem.CheckState = CheckState.Unchecked;
             
            }
        public void UpdateMode(bool IsDraftMode)
         {
         this.IsDraftMode = IsDraftMode;
        Template template = new Template(this.mediator);
        try
            {

                Epi.Core.ServiceClient.ServiceClient.IsValidOrganizationKeyEnum IsValidOKey = Epi.Core.ServiceClient.ServiceClient.IsValidOrganizationKeyEnum.No;
                if (string.IsNullOrWhiteSpace(this.projectExplorer.CurrentView.WebSurveyId))
                    {
                        IsValidOKey = Epi.Core.ServiceClient.ServiceClient.IsValidOrgKey(this.OrganizationKey);
                    }
                else
                    {
                        IsValidOKey = Epi.Core.ServiceClient.ServiceClient.IsValidOrgKey(this.OrganizationKey, this.projectExplorer.CurrentView.WebSurveyId);
                    }
                WebPublishDialog dialog = null;
                
                switch (IsValidOKey)
                    {
                        case Epi.Core.ServiceClient.ServiceClient.IsValidOrganizationKeyEnum.No:
                        OrgKey NewDialog = new OrgKey(this.CurrentView.WebSurveyId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can republish this survey.");
                        DialogResult result = NewDialog.ShowDialog();
                        if (result == System.Windows.Forms.DialogResult.OK)
                            {
                            this.OrganizationKey = NewDialog.OrganizationKey;
                            if (!string.IsNullOrWhiteSpace(OrganizationKey))
                                {
                                SetSurveyInfo();
                             UpdateSurveyMode(IsDraftMode);
                                }
                            }
                        break;
                        case Epi.Core.ServiceClient.ServiceClient.IsValidOrganizationKeyEnum.EndPointNotFound:
                        WebSurveyOptions dialog1 = new WebSurveyOptions();
                        DialogResult result1 = dialog1.ShowDialog();
                        if (result1 == System.Windows.Forms.DialogResult.OK)
                            {
                            OrgKey OrgKeyDialog = new OrgKey(this.CurrentView.WebSurveyId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can republish this survey.");
                            DialogResult result2 = OrgKeyDialog.ShowDialog();
                            if (result2 == System.Windows.Forms.DialogResult.OK)
                                {
                                this.OrganizationKey = OrgKeyDialog.OrganizationKey;
                                if (!string.IsNullOrWhiteSpace(OrganizationKey))
                                    {
                                    SetSurveyInfo();
                                 UpdateSurveyMode(IsDraftMode);
                                    }
                                }
                            }
                        break;
                        case Epi.Core.ServiceClient.ServiceClient.IsValidOrganizationKeyEnum.GeneralException:
                        WebSurveyOptions dialog2 = new WebSurveyOptions();
                        DialogResult result3 = dialog2.ShowDialog();
                        if (result3 == System.Windows.Forms.DialogResult.OK)
                            {
                            OrgKey OrgKeyDialog = new OrgKey(this.CurrentView.WebSurveyId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can republish this survey.");
                            DialogResult result4 = OrgKeyDialog.ShowDialog();
                            if (result4 == System.Windows.Forms.DialogResult.OK)
                                {
                                this.OrganizationKey = OrgKeyDialog.OrganizationKey;
                                if (!string.IsNullOrWhiteSpace(OrganizationKey))
                                    {
                                    SetSurveyInfo();

                                 UpdateSurveyMode(IsDraftMode);
                                    }
                                }
                            }
                        break;
                        case Epi.Core.ServiceClient.ServiceClient.IsValidOrganizationKeyEnum.Yes:
                       SetSurveyInfo();
                     UpdateSurveyMode(IsDraftMode);
                        break;
                    }
                
            }
        catch (Exception ex)// not republishable
            {
           
            }


        }
        public void UpdateWebEnterMode(bool IsDraftMode)
        {
            this.IsEWEDraftMode = IsDraftMode;
            Template template = new Template(this.mediator);


            View RootView = mediator.Project.Metadata.GetParentView(this.projectExplorer.CurrentView.Id);

            // DataTable table = mediator.Project.Metadata.GetPublishedViewKeys(this.projectExplorer.CurrentView.Id);
            DataTable table;
           
            if (RootView == null)
            {
                table = this.mediator.Project.Metadata.GetPublishedViewKeys(this.mediator.ProjectExplorer.CurrentView.Id);
            }
            else
                {
                table = this.mediator.Project.Metadata.GetPublishedViewKeys(RootView.Id);
            }

            DataRow ViewRow = table.Rows[0];
            string OrganizationKey = ViewRow.ItemArray[2].ToString(); ;
            string WebSurveyId = ViewRow.ItemArray[3].ToString();

          if(ValidateUser())
            {
              DoUpDateMode(IsDraftMode, OrganizationKey, WebSurveyId);
            }

            else
    
            {
            Configuration config = Configuration.GetNewInstance(); 
            int ISWindowAuthMode = config.Settings.EWEServiceAuthMode;

            if (ISWindowAuthMode == 0)
                {
                if (LoginInfo.UserID == -1)
                    {
                    UserAuthentication dialog = new UserAuthentication();
                    DialogResult result = dialog.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                        {
                        DoUpDateMode(IsDraftMode, OrganizationKey, WebSurveyId);
                        }
                    }
                  else 
                    {
                    DoUpDateMode(IsDraftMode, OrganizationKey, WebSurveyId);
                    }
                 }
            else {
          
                MessageBox.Show("You are not authorized to Change mode for this form. Please contact system admin for more info.");
            
                }

             }
            }

        private void DoUpDateMode(bool IsDraftMode, string OrganizationKey, string WebSurveyId)
         {
         try
             {

                 Epi.Core.ServiceClient.EWEServiceClient.IsValidOrganizationKeyEnum IsValidOKey = Epi.Core.ServiceClient.EWEServiceClient.IsValidOrganizationKeyEnum.No;
                if (string.IsNullOrWhiteSpace(WebSurveyId))
                 {
                     IsValidOKey = Epi.Core.ServiceClient.EWEServiceClient.IsValidOrgKey(OrganizationKey);
                 }
             else
                 {
                     IsValidOKey = Epi.Core.ServiceClient.EWEServiceClient.IsValidOrgKey(OrganizationKey, WebSurveyId);
                 }
                WebEnterPublishDialog dialog = null;

             switch (IsValidOKey)
                 {
                     case Epi.Core.ServiceClient.EWEServiceClient.IsValidOrganizationKeyEnum.No:
                        EWEOrgKey NewDialog = new EWEOrgKey(WebSurveyId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can republish this survey.");
                     DialogResult result = NewDialog.ShowDialog();
                     if (result == System.Windows.Forms.DialogResult.OK)
                         {
                            OrganizationKey = NewDialog.OrganizationKey;
                         if (!string.IsNullOrWhiteSpace(OrganizationKey))
                             {
                                SetFormInfo();
                                UpdateFormMode(IsDraftMode);
                             }
                         }
                     break;
                     case Epi.Core.ServiceClient.EWEServiceClient.IsValidOrganizationKeyEnum.EndPointNotFound:
                        WebEnterOptions dialog1 = new WebEnterOptions();
                     DialogResult result1 = dialog1.ShowDialog();
                     if (result1 == System.Windows.Forms.DialogResult.OK)
                         {
                            EWEOrgKey OrgKeyDialog = new EWEOrgKey(WebSurveyId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can republish this survey.");
                         DialogResult result2 = OrgKeyDialog.ShowDialog();
                         if (result2 == System.Windows.Forms.DialogResult.OK)
                             {
                                OrganizationKey = OrgKeyDialog.OrganizationKey;
                             if (!string.IsNullOrWhiteSpace(OrganizationKey))
                                 {
                                    SetFormInfo();
                                    UpdateFormMode(IsDraftMode);
                                 }
                             }
                         }
                     break;
                     case Epi.Core.ServiceClient.EWEServiceClient.IsValidOrganizationKeyEnum.GeneralException:
                        WebEnterOptions dialog2 = new WebEnterOptions();
                     DialogResult result3 = dialog2.ShowDialog();
                     if (result3 == System.Windows.Forms.DialogResult.OK)
                         {
                            EWEOrgKey OrgKeyDialog = new EWEOrgKey(WebSurveyId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can republish this survey.");
                         DialogResult result4 = OrgKeyDialog.ShowDialog();
                         if (result4 == System.Windows.Forms.DialogResult.OK)
                             {
                                OrganizationKey = OrgKeyDialog.OrganizationKey;
                             if (!string.IsNullOrWhiteSpace(OrganizationKey))
                                 {
                                    SetFormInfo();

                                    UpdateFormMode(IsDraftMode);
                                 }
                             }
                         }
                     break;
                     case Epi.Core.ServiceClient.EWEServiceClient.IsValidOrganizationKeyEnum.Yes:
                        SetFormInfo();
                        UpdateFormMode(IsDraftMode);
                     break;
                 }

             }
         catch (Exception ex)// not republishable
             {

             }

         }
        public void SetModetoolStripDropDown(bool IsDraftMode){

            if (IsDraftMode)
                {
                draftToolStripMenuItem1.Checked = true;
                draftToolStripMenuItem1.CheckState = CheckState.Checked;
                finalToolStripMenuItem1.CheckState = CheckState.Unchecked;
                }
            else
                {
                finalToolStripMenuItem1.Checked = true;
                finalToolStripMenuItem1.CheckState = CheckState.Checked;
                draftToolStripMenuItem1.CheckState = CheckState.Unchecked;
                }
        }
        public void SetEWEModetoolStripDropDown(bool IsDraftMode)
            {

            if (IsDraftMode)
                {
                draftToolStripMenuItem2.Checked = true;
                draftToolStripMenuItem2.CheckState = CheckState.Checked;
                finalToolStripMenuItem2.CheckState = CheckState.Unchecked;
                }
            else
                {
                finalToolStripMenuItem2.Checked = true;
                finalToolStripMenuItem2.CheckState = CheckState.Checked;
                draftToolStripMenuItem2.CheckState = CheckState.Unchecked;
                }
            }
      
        private void UpdateSurveyMode(bool IsDraftMode)
        {
            try
            {
            SurveyManagerService.ManagerServiceV3Client client = Epi.Core.ServiceClient.ServiceClient.GetClient();
                Configuration config = Configuration.GetNewInstance();

                SurveyManagerService.SurveyInfoRequest Request = new SurveyManagerService.SurveyInfoRequest();//(Epi.Web.Common.Message.SurveyInfoRequest)((object[])e.Argument)[0];
                SurveyManagerService.SurveyInfoResponse Result = new SurveyManagerService.SurveyInfoResponse();//(Epi.Web.Common.Message.SurveyInfoResponse)((object[])e.Argument)[1];

                Request.Criteria.ClosingDate = this.CloseDate;
                Request.Criteria.OrganizationKey = new Guid(this.OrganizationKey);
                Request.Criteria.UserPublishKey = new Guid(this.UserPublishKey);
                Request.Criteria.SurveyIdList = new string[]{this.SurveyId};

                SurveyManagerService.SurveyInfoDTO SurveyInfoDTO = new SurveyManagerService.SurveyInfoDTO();

                SurveyInfoDTO.ClosingDate = this.CloseDate;
                SurveyInfoDTO.StartDate = this.StartDate;
                SurveyInfoDTO.SurveyId = new Guid(this.CurrentView.WebSurveyId).ToString();
                SurveyInfoDTO.SurveyType = this.SurveyType;
                SurveyInfoDTO.SurveyNumber = this.SurveyNumber;
                SurveyInfoDTO.SurveyName = this.SurveyName;
                SurveyInfoDTO.OrganizationKey = new Guid(OrganizationKey);
                SurveyInfoDTO.UserPublishKey = new Guid(this.UserPublishKey);
                SurveyInfoDTO.OrganizationName = this.OrganizationName;
                SurveyInfoDTO.XML = this.TemplateXML;
                SurveyInfoDTO.ExitText = this.ExitText;
                SurveyInfoDTO.IntroductionText = this.IntroductionText;
                SurveyInfoDTO.DepartmentName = this.DepartmentName;                
                Request.Criteria.SurveyType = this.SurveyType;

                if (IsDraftMode)
                {
                    Request.Action = "Update";
                    Request.Criteria.IsDraftMode = true;
                    SurveyInfoDTO.IsDraftMode = true;
                }
                else
                {
                    Request.Action = "UpdateMode";
                    Request.Criteria.IsDraftMode = false;
                    SurveyInfoDTO.IsDraftMode = false;
                }

                Request.SurveyInfoList = new SurveyManagerService.SurveyInfoDTO[]{SurveyInfoDTO};
                Result = client.SetSurveyInfo(Request);

                if (Result != null && Result.SurveyInfoList.Length > 0)
                {
                    if (IsDraftMode)
                    {
                        MessageBox.Show("Survey mode was successfully changed to DRAFT.", "", MessageBoxButtons.OK);
                    }
                    else
                    {
                        MessageBox.Show("Survey mode was successfully changed to FINAL.", "", MessageBoxButtons.OK);
                    }
                }
            }
            catch
            {
            }
        }
        
        private void UpdateFormMode(bool IsDraftMode)
        {
            try
            {
                Template template = new Template(this.mediator);

               var client = Epi.Core.ServiceClient.EWEServiceClient.GetClient();
                DataTable table;
                View RootView = this.mediator.Project.Metadata.GetParentView(this.mediator.ProjectExplorer.CurrentView.Id);
                if (RootView == null)
                {
                    table = this.mediator.Project.Metadata.GetPublishedViewKeys(this.mediator.ProjectExplorer.CurrentView.Id);
                }
                else
                {
                    table = this.mediator.Project.Metadata.GetPublishedViewKeys(RootView.Id);
                }
          
                DataRow ViewRow = table.Rows[0];

                string WebSurveyId = ViewRow.ItemArray[3].ToString();
                string OrganizationKey = ViewRow.ItemArray[2].ToString();


                Configuration config = Configuration.GetNewInstance();
                Epi.EWEManagerService.SurveyInfoRequest Request = new Epi.EWEManagerService.SurveyInfoRequest();
                Epi.EWEManagerService.SurveyInfoResponse Result = new Epi.EWEManagerService.SurveyInfoResponse();
                Epi.EWEManagerService.SurveyInfoCriteria Criteria = new EWEManagerService.SurveyInfoCriteria();

                //   Request.Criteria.ClosingDate = this.CloseDate;
                Criteria.OrganizationKey = new Guid(OrganizationKey);
                //Criteria.UserPublishKey = new Guid(this.UserPublishKey);
                List<string> List = new List<string>();
                List.Add(WebSurveyId);
                Criteria.SurveyIdList = List.ToArray();
                Request.Criteria = Criteria;
                

                Epi.EWEManagerService.SurveyInfoDTO SurveyInfoDTO = new Epi.EWEManagerService.SurveyInfoDTO();


                SurveyInfoDTO.StartDate = DateTime.Now;
                SurveyInfoDTO.SurveyId = new Guid(WebSurveyId).ToString();
                SurveyInfoDTO.SurveyType = 2;
                SurveyInfoDTO.SurveyName = mediator.Project.Name;
                SurveyInfoDTO.OrganizationKey = new Guid(OrganizationKey);
                if (this.mediator.Project.CollectedData.GetDbDriver().ConnectionDescription.ToString().Contains("Microsoft SQL Server:"))
                {
                    SurveyInfoDTO.IsSqlProject= true;
                }// Update IsSqlProject to true on change survey mode for Sql project.

                //SurveyInfoDTO.UserPublishKey = new Guid(this.UserPublishKey);
                SurveyInfoDTO.XML = template.CreateWebEnterTemplate();

                Request.Criteria.SurveyType = 2;
               
               

                if (IsDraftMode)
                    {
                    Request.Action = "Update";
                    Request.Criteria.IsDraftMode = true;
                    SurveyInfoDTO.IsDraftMode = true;
                    }
                else
                    {
                    Request.Action = "UpdateMode";
                    Request.Criteria.IsDraftMode = false;
                    SurveyInfoDTO.IsDraftMode = false;
                    }
                List<Epi.EWEManagerService.SurveyInfoDTO> DTOList = new List<Epi.EWEManagerService.SurveyInfoDTO>();
                DTOList.Add(SurveyInfoDTO);
                Request.SurveyInfoList = DTOList.ToArray();

                Result = client.SetSurveyInfo(Request);
                if (Result != null && Result.SurveyInfoList.Count() > 0)
                    {
                    //this.UpdateStatus("Survey mode was successfully updated!");
                    if (IsDraftMode)
                        {
                        MessageBox.Show("Form mode was successfully changed to Staging.", "", MessageBoxButtons.OK);
                        }
                    else
                        {
                        MessageBox.Show("Form mode was successfully changed to Production.", "", MessageBoxButtons.OK);
                        }
            }
        }

            catch (Exception ex)
                {
                //this.BeginInvoke(new FinishWithExceptionDelegate(FinishWithException), ex);

                }
            }
      //  private void GetSurveyInfo (object sender, System.ComponentModel.DoWorkEventArgs e)
        public void SetSurveyInfo()
        {
            try
            {
               SurveyManagerService.ManagerServiceV3Client client = Epi.Core.ServiceClient.ServiceClient.GetClient();
                Configuration config = Configuration.GetNewInstance();

                SurveyManagerService.SurveyInfoRequest Request = new SurveyManagerService.SurveyInfoRequest();//(Epi.Web.Common.Message.SurveyInfoRequest)((object[])e.Argument)[0];
                SurveyManagerService.SurveyInfoResponse Result = new SurveyManagerService.SurveyInfoResponse();//(Epi.Web.Common.Message.SurveyInfoResponse)((object[])e.Argument)[1];

                if (!string.IsNullOrWhiteSpace(this.CurrentView.WebSurveyId))
                {
                    Request.Criteria.OrganizationKey =  new Guid(this.OrganizationKey); //new Guid(this.OrganizationKey);
                    Request.Criteria.ReturnSizeInfoOnly = false;
                    Request.Criteria.SurveyIdList = new string[]{this.CurrentView.WebSurveyId};
                    Result = client.GetSurveyInfo(Request);
                    
                }

                if (Result != null && Result.SurveyInfoList.Length > 0)
                {

                    SurveyName = Result.SurveyInfoList[0].SurveyName;
                    DepartmentName = Result.SurveyInfoList[0].DepartmentName;
                    SurveyNumber = Result.SurveyInfoList[0].SurveyNumber;
                    OrganizationName = Result.SurveyInfoList[0].OrganizationName;
                    StartDate = Result.SurveyInfoList[0].StartDate;
                    CloseDate = Result.SurveyInfoList[0].ClosingDate;
                    IsDraftMode = Result.SurveyInfoList[0].IsDraftMode;
                    IntroductionText = Result.SurveyInfoList[0].IntroductionText;
                    ExitText = Result.SurveyInfoList[0].ExitText;
                    TemplateXML = Result.SurveyInfoList[0].XML;
                    //this.OrganizationKey = Result.SurveyInfoList[0].OrganizationKey.ToString();
                    SurveyType = Result.SurveyInfoList[0].SurveyType;
                    this.UserPublishKey = Result.SurveyInfoList[0].UserPublishKey.ToString();
                    //SurveyId = Result.SurveyInfoList[0].SurveyId;
                    //ChangeModetoolStripDropDownButton
                    SetModetoolStripDropDown(IsDraftMode);
                }

            }
            catch (FaultException<CustomFaultException> cfe)
            {
                // this.BeginInvoke(new FinishWithCustomFaultExceptionDelegate(FinishWithCustomFaultException), cfe);

            }
            catch (FaultException fe)
            {
                //this.BeginInvoke(new FinishWithFaultExceptionDelegate(FinishWithFaultException), fe);

            }
            catch (SecurityNegotiationException sne)
            {
                //this.BeginInvoke(new FinishWithSecurityNegotiationExceptionDelegate(FinishWithSecurityNegotiationException), sne);

            }
            catch (CommunicationException ce)
            {
                //this.BeginInvoke(new FinishWithCommunicationExceptionDelegate(FinishWithCommunicationException), ce);

            }
            catch (TimeoutException te)
            {
                // this.BeginInvoke(new FinishWithTimeoutExceptionDelegate(FinishWithTimeoutException), te);

            }
            catch (Exception ex)
            {
                //this.BeginInvoke(new FinishWithExceptionDelegate(FinishWithException), ex);

            }
        }
       
        public void SetFormInfo()
        {
            try
            {
                var client = Epi.Core.ServiceClient.EWEServiceClient.GetClient();
                Configuration config = Configuration.GetNewInstance();
                DataTable table = mediator.Project.Metadata.GetPublishedViewKeys(this.projectExplorer.CurrentView.Id);
                DataRow ViewRow = table.Rows[0];

                string WebSurveyId = ViewRow.ItemArray[3].ToString();

                Epi.EWEManagerService.SurveyInfoRequest infoRequest = new Epi.EWEManagerService.SurveyInfoRequest();
                Epi.EWEManagerService.SurveyInfoResponse infoResult = new Epi.EWEManagerService.SurveyInfoResponse();
                infoRequest.Criteria = new EWEManagerService.SurveyInfoCriteria();
                
                if (!string.IsNullOrWhiteSpace(WebSurveyId))
                {
                    infoRequest.Criteria.OrganizationKey = new Guid(this.EWEOrganizationKey);
                    infoRequest.Criteria.ReturnSizeInfoOnly = false;
                    infoRequest.Criteria.SurveyIdList = new string[]{WebSurveyId};
                    infoResult = client.GetSurveyInfo(infoRequest);
                }

                if (infoResult != null && infoResult.SurveyInfoList.Count() > 0)
                {
                    SurveyName = infoResult.SurveyInfoList[0].SurveyName;
                    DepartmentName = infoResult.SurveyInfoList[0].DepartmentName;
                    SurveyNumber = infoResult.SurveyInfoList[0].SurveyNumber;
                    OrganizationName = infoResult.SurveyInfoList[0].OrganizationName;
                    StartDate = infoResult.SurveyInfoList[0].StartDate;
                    CloseDate = infoResult.SurveyInfoList[0].ClosingDate;
                    IsDraftMode = infoResult.SurveyInfoList[0].IsDraftMode;
                    IntroductionText = infoResult.SurveyInfoList[0].IntroductionText;
                    ExitText = infoResult.SurveyInfoList[0].ExitText;
                    TemplateXML = infoResult.SurveyInfoList[0].XML;
                    SurveyType = infoResult.SurveyInfoList[0].SurveyType;
                    this.UserPublishKey = infoResult.SurveyInfoList[0].UserPublishKey.ToString();
                    SetEWEModetoolStripDropDown(IsDraftMode);
                }
            }
            catch (FaultException<CustomFaultException> cfe)
            {
            }
            catch (FaultException fe)
            {
            }
            catch (SecurityNegotiationException sne)
            {
            }
            catch (CommunicationException ce)
            {
            }
            catch (TimeoutException te)
            {
            }
            catch (Exception ex)
            {
            }
        }

        private void ChangeModetoolStripDropDownButton_Click(object sender, EventArgs e)
        {
            /*
        Template template = new Template(this.mediator);
        try
            {

            Epi.Windows.MakeView.Utils.ServiceClient.IsValidOrganizationKeyEnum IsValidOKey = Epi.Windows.MakeView.Utils.ServiceClient.IsValidOrganizationKeyEnum.No;
            if (string.IsNullOrWhiteSpace(this.projectExplorer.CurrentView.WebSurveyId))
                {
                IsValidOKey = Epi.Windows.MakeView.Utils.ServiceClient.IsValidOrgKey(this.OrganizationKey);
                }
            else
                {
                IsValidOKey = Epi.Windows.MakeView.Utils.ServiceClient.IsValidOrgKey(this.OrganizationKey, this.projectExplorer.CurrentView.WebSurveyId);
                }
            WebPublishDialog dialog = null;

            switch (IsValidOKey)
                {
                case Epi.Windows.MakeView.Utils.ServiceClient.IsValidOrganizationKeyEnum.No:
                    OrgKey NewDialog = new OrgKey(this.CurrentView.WebSurveyId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can republish this survey.");
                    DialogResult result = NewDialog.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                        {
                        this.OrganizationKey = NewDialog.OrganizationKey;
                        if (!string.IsNullOrWhiteSpace(OrganizationKey))
                            {
                            SetSurveyInfo();
                            
                            }
                        }
                    break;
                case Epi.Windows.MakeView.Utils.ServiceClient.IsValidOrganizationKeyEnum.EndPointNotFound:
                    WebSurveyOptions dialog1 = new WebSurveyOptions();
                    DialogResult result1 = dialog1.ShowDialog();
                    if (result1 == System.Windows.Forms.DialogResult.OK)
                        {
                        OrgKey OrgKeyDialog = new OrgKey(this.CurrentView.WebSurveyId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can republish this survey.");
                        DialogResult result2 = OrgKeyDialog.ShowDialog();
                        if (result2 == System.Windows.Forms.DialogResult.OK)
                            {
                            this.OrganizationKey = OrgKeyDialog.OrganizationKey;
                            if (!string.IsNullOrWhiteSpace(OrganizationKey))
                                {
                                SetSurveyInfo();
                                
                                }
                            }
                        }
                    break;
                case Epi.Windows.MakeView.Utils.ServiceClient.IsValidOrganizationKeyEnum.GeneralException:
                    WebSurveyOptions dialog2 = new WebSurveyOptions();
                    DialogResult result3 = dialog2.ShowDialog();
                    if (result3 == System.Windows.Forms.DialogResult.OK)
                        {
                        OrgKey OrgKeyDialog = new OrgKey(this.CurrentView.WebSurveyId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can republish this survey.");
                        DialogResult result4 = OrgKeyDialog.ShowDialog();
                        if (result4 == System.Windows.Forms.DialogResult.OK)
                            {
                            this.OrganizationKey = OrgKeyDialog.OrganizationKey;
                            if (!string.IsNullOrWhiteSpace(OrganizationKey))
                                {
                                SetSurveyInfo();
                              
                                }
                            }
                        }
                    break;
                case Epi.Windows.MakeView.Utils.ServiceClient.IsValidOrganizationKeyEnum.Yes:
                    SetSurveyInfo();
                     
                    break;
                }

            }
        catch (Exception ex)// not republishable
            {

            }
            */

        }
        private void QuickSurveyInfoUpdate()
        {
            try
            {
                Template template = new Template(this.mediator);
                SurveyManagerService.ManagerServiceV3Client client = Epi.Core.ServiceClient.ServiceClient.GetClient();
                Configuration config = Configuration.GetNewInstance();

                SurveyManagerService.SurveyInfoRequest Request = new SurveyManagerService.SurveyInfoRequest();//(Epi.Web.Common.Message.SurveyInfoRequest)((object[])e.Argument)[0];
                SurveyManagerService.SurveyInfoResponse Result = new SurveyManagerService.SurveyInfoResponse();//(Epi.Web.Common.Message.SurveyInfoResponse)((object[])e.Argument)[1];

                Request.Criteria.ClosingDate = this.CloseDate;
                Request.Criteria.OrganizationKey = new Guid(this.OrganizationKey);
                Request.Criteria.UserPublishKey = new Guid(this.UserPublishKey);
                Request.Criteria.SurveyIdList = new string[]{this.SurveyId};
                Request.Action = "Update";

                SurveyManagerService.SurveyInfoDTO SurveyInfoDTO = new SurveyManagerService.SurveyInfoDTO();

                SurveyInfoDTO.ClosingDate = this.CloseDate;
                SurveyInfoDTO.StartDate = this.StartDate;
                SurveyInfoDTO.SurveyId = new Guid(this.CurrentView.WebSurveyId).ToString();
                SurveyInfoDTO.SurveyType = this.SurveyType;
                SurveyInfoDTO.SurveyNumber = this.SurveyNumber;
                SurveyInfoDTO.SurveyName = this.SurveyName;
                SurveyInfoDTO.OrganizationKey = new Guid(OrganizationKey);
                SurveyInfoDTO.OrganizationName = this.OrganizationName;
                SurveyInfoDTO.UserPublishKey = new Guid(this.UserPublishKey);
                SurveyInfoDTO.XML = template.CreateWebSurveyTemplate();
                SurveyInfoDTO.ExitText = this.ExitText;
                SurveyInfoDTO.IntroductionText = this.IntroductionText;
                SurveyInfoDTO.DepartmentName = this.DepartmentName;

                Request.Criteria.SurveyType = this.SurveyType;
                Request.Criteria.IsDraftMode = true;
                SurveyInfoDTO.IsDraftMode = this.IsDraftMode;
                Request.SurveyInfoList = new SurveyManagerService.SurveyInfoDTO[] { SurveyInfoDTO };

                Result = client.SetSurveyInfo(Request);

                if (Result != null && Result.SurveyInfoList.Length > 0)
                {
                    //this.UpdateStatus("Survey was successfully updated!");

                MessageBox.Show("Survey was successfully updated.","",MessageBoxButtons.OK);
                }
            }
            catch (FaultException<CustomFaultException> cfe)
            {
                // this.BeginInvoke(new FinishWithCustomFaultExceptionDelegate(FinishWithCustomFaultException), cfe);

            }
            catch (FaultException fe)
            {
                //this.BeginInvoke(new FinishWithFaultExceptionDelegate(FinishWithFaultException), fe);

            }
            catch (SecurityNegotiationException sne)
            {
                //this.BeginInvoke(new FinishWithSecurityNegotiationExceptionDelegate(FinishWithSecurityNegotiationException), sne);

            }
            catch (CommunicationException ce)
            {
                //this.BeginInvoke(new FinishWithCommunicationExceptionDelegate(FinishWithCommunicationException), ce);

            }
            catch (TimeoutException te)
            {
                // this.BeginInvoke(new FinishWithTimeoutExceptionDelegate(FinishWithTimeoutException), te);

            }
            catch (Exception ex)
            {
                //this.BeginInvoke(new FinishWithExceptionDelegate(FinishWithException), ex);

            }
        }
        private void QuickFormInfoUpdate()
            {
            try
                {
                Template template = new Template(this.mediator);
                if (ValidateUser()) // Validate User
                    {

                    DoQuickPublish(template);
                    }
                else // Validate User
                    {

                         Configuration config = Configuration.GetNewInstance();
                         int ISWindowAuthMode = config.Settings.EWEServiceAuthMode;
                         if (ISWindowAuthMode == 0 )
                            {
                            if (LoginInfo.UserID == -1)
                                {
                                    UserAuthentication dialog = new UserAuthentication();
                                    DialogResult result = dialog.ShowDialog();
                                    if (result == System.Windows.Forms.DialogResult.OK)
                                        {
                                        dialog.Close();
                                        DoQuickPublish(template);
                                        }
                                }
                            else 
                                {
                                       DoQuickPublish(template);
                                 }
                            }
                        else 
                            {
                               MessageBox.Show("You are not authorized to quick publish this form to Epi Web Enter system. Please contact system admin for more info.");
                            
                            }
                                   
                    }
                }
            catch (FaultException<CustomFaultException> cfe)
                {
                // this.BeginInvoke(new FinishWithCustomFaultExceptionDelegate(FinishWithCustomFaultException), cfe);

                }
            catch (FaultException fe)
                {
                //this.BeginInvoke(new FinishWithFaultExceptionDelegate(FinishWithFaultException), fe);

                }
            catch (SecurityNegotiationException sne)
                {
                //this.BeginInvoke(new FinishWithSecurityNegotiationExceptionDelegate(FinishWithSecurityNegotiationException), sne);

                }
            catch (CommunicationException ce)
                {
                //this.BeginInvoke(new FinishWithCommunicationExceptionDelegate(FinishWithCommunicationException), ce);

        }
            catch (TimeoutException te)
                {
                // this.BeginInvoke(new FinishWithTimeoutExceptionDelegate(FinishWithTimeoutException), te);

                }
            catch (Exception ex)
                {
                //this.BeginInvoke(new FinishWithExceptionDelegate(FinishWithException), ex);

                }
            }

        private void DoQuickPublish(Template template)
            {




                var client = Epi.Core.ServiceClient.EWEServiceClient.GetClient();

            DataTable table;
            View RootView = this.mediator.Project.Metadata.GetParentView(this.mediator.ProjectExplorer.CurrentView.Id);
            if (RootView == null)
                {
                table = this.mediator.Project.Metadata.GetPublishedViewKeys(this.mediator.ProjectExplorer.CurrentView.Id);
                }
            else
                {
                table = this.mediator.Project.Metadata.GetPublishedViewKeys(RootView.Id);
                }
            DataRow ViewRow = table.Rows[0];

            string WebSurveyId = ViewRow.ItemArray[3].ToString();
            string OrganizationKey = ViewRow.ItemArray[2].ToString();


            Configuration config = Configuration.GetNewInstance();
            Epi.EWEManagerService.SurveyInfoRequest Request = new Epi.EWEManagerService.SurveyInfoRequest();
            Epi.EWEManagerService.SurveyInfoResponse Result = new Epi.EWEManagerService.SurveyInfoResponse();
            Epi.EWEManagerService.SurveyInfoCriteria Criteria = new EWEManagerService.SurveyInfoCriteria();

            //   Request.Criteria.ClosingDate = this.CloseDate;
            Criteria.OrganizationKey = new Guid(OrganizationKey);
            //Criteria.UserPublishKey = new Guid(this.UserPublishKey);
            List<string> List = new List<string>();
            List.Add(WebSurveyId);
            Criteria.SurveyIdList = List.ToArray();
            Request.Criteria = Criteria;
            Request.Action = "Update";

            Epi.EWEManagerService.SurveyInfoDTO SurveyInfoDTO = new Epi.EWEManagerService.SurveyInfoDTO();


            SurveyInfoDTO.StartDate = DateTime.Now;
            SurveyInfoDTO.SurveyId = new Guid(WebSurveyId).ToString();
            SurveyInfoDTO.SurveyType = 2;
            SurveyInfoDTO.SurveyName = mediator.Project.Name;
            SurveyInfoDTO.OrganizationKey = new Guid(OrganizationKey);

            if (this.mediator.Project.CollectedData.GetDbDriver().ConnectionDescription.ToString().Contains("Microsoft SQL Server:"))
            {
                SurveyInfoDTO.IsSqlProject = true;
            }//changed IsSqlProject to true on quick publishing a SQL project

            //SurveyInfoDTO.UserPublishKey = new Guid(this.UserPublishKey);
            SurveyInfoDTO.XML = template.CreateWebEnterTemplate();



            Request.Criteria.SurveyType = 2;
            Request.Criteria.IsDraftMode = true;
            //SurveyInfoDTO.IsDraftMode = this.IsDraftMode;
            List<Epi.EWEManagerService.SurveyInfoDTO> DTOList = new List<Epi.EWEManagerService.SurveyInfoDTO>();
            DTOList.Add(SurveyInfoDTO);
            Request.SurveyInfoList = DTOList.ToArray();
            Result = client.SetSurveyInfo(Request);
            if (Result != null && Result.SurveyInfoList.Count() > 0)
                {
                //this.UpdateStatus("Survey was successfully updated!");

                MessageBox.Show("Form was successfully updated.", "", MessageBoxButtons.OK);
                }
            }
        private void toolStripPublishToWebEnter_Click(object sender, EventArgs e)
            {
            DataTable table = mediator.Project.Metadata.GetPublishedViewKeys(this.projectExplorer.CurrentView.Id);
            DataRow ViewRow = table.Rows[0];
        // this.OrganizationKey = this.projectExplorer.CurrentView.EWEOrganizationKey;
            this.EWEOrganizationKey = ViewRow.ItemArray[2].ToString();
            if (string.IsNullOrEmpty(EWEOrganizationKey))
                if (RepublishOrgKey != null)
                    EWEOrganizationKey = RepublishOrgKey;

            Template template = new Template(this.mediator);
            string InvalidForPublishing = ListFieldsNotSupportedForWeb();
            View view;
            
            if (projectExplorer.CurrentView != null)
                {
                view = projectExplorer.CurrentView;
                }
            else
                {
                view = projectExplorer.SelectedPage.view;
                }
            if (InvalidForPublishing.Length > 0)
                {
                SupportedFieldTypeDialog dialog = new SupportedFieldTypeDialog(InvalidForPublishing);
                dialog.ShowDialog();
                }
            else
                {
                Configuration config = Configuration.GetNewInstance();               

                try
                    {
                       
                        if (view.Project.CollectedData.TableExists(view.TableName) == false)//checking if no table is created in Epi7
                        {                            
                            CreateViewDataTable(view);
                        }
                        if (ValidateUser()&& ! iscancel) // Validate User
                        {
                    if (config.Settings.Republish_IsRepbulishable == true) //IsRepbulishable
                        {

                        if (string.IsNullOrWhiteSpace(this.EWEOrganizationKey) && !string.IsNullOrWhiteSpace(view.EWEFormId))//valitate OrgId and FormId
                            {
                                Epi.Core.ServiceClient.EWEServiceClient.IsValidOrganizationKeyEnum IsValidOKey = Epi.Core.ServiceClient.EWEServiceClient.IsValidOrganizationKeyEnum.No;
                            if (string.IsNullOrWhiteSpace(view.EWEFormId))
                                {
                                    IsValidOKey = Epi.Core.ServiceClient.EWEServiceClient.IsValidOrgKey(this.EWEOrganizationKey);
                                }
                            else
                                {
                                    IsValidOKey = Epi.Core.ServiceClient.EWEServiceClient.IsValidOrgKey(this.EWEOrganizationKey, view.EWEFormId);
                                }
                            WebEnterPublishDialog dialog = null;
                            WebEnterOptions wso = null;
                            switch (IsValidOKey)
                                {
                                    case Epi.Core.ServiceClient.EWEServiceClient.IsValidOrganizationKeyEnum.No:
                                    EWEOrgKey NewDialog = new EWEOrgKey(this.CurrentView.EWEFormId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can republish this survey.");
                                    DialogResult result = NewDialog.ShowDialog();
                                    if (result == System.Windows.Forms.DialogResult.OK)
                                        {
                                        this.EWEOrganizationKey = NewDialog.OrganizationKey;
                                        if (!string.IsNullOrWhiteSpace(EWEOrganizationKey))
                                            {
                                            SetFormInfo();
                                            dialog = new WebEnterPublishDialog(this.EWEOrganizationKey, this.mediator, template.CreateWebEnterTemplate());
                                            dialog.ShowDialog();
                                            }
                                        }
                                    break;
                                    case Epi.Core.ServiceClient.EWEServiceClient.IsValidOrganizationKeyEnum.EndPointNotFound:
                                    WebEnterOptions dialog1 = new WebEnterOptions();
                                    DialogResult result1 = dialog1.ShowDialog();
                                    if (result1 == System.Windows.Forms.DialogResult.OK)
                                        {
                                        EWEOrgKey OrgKeyDialog = new EWEOrgKey(this.CurrentView.EWEFormId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can republish this survey.");
                                        DialogResult result2 = OrgKeyDialog.ShowDialog();
                                        if (result2 == System.Windows.Forms.DialogResult.OK)
                                            {
                                            this.EWEOrganizationKey = OrgKeyDialog.OrganizationKey;
                                            if (!string.IsNullOrWhiteSpace(EWEOrganizationKey))
                                                {
                                                SetFormInfo();
                                                dialog = new WebEnterPublishDialog(this.EWEOrganizationKey, this.mediator, template.CreateWebEnterTemplate());
                                                dialog.ShowDialog();
                                                }
                                            }
                                        }
                                    break;
                                    case Epi.Core.ServiceClient.EWEServiceClient.IsValidOrganizationKeyEnum.GeneralException:
                                    WebEnterOptions dialog2 = new WebEnterOptions();
                                    DialogResult result3 = dialog2.ShowDialog();
                                    if (result3 == System.Windows.Forms.DialogResult.OK)
                                        {
                                        EWEOrgKey OrgKeyDialog = new EWEOrgKey(this.CurrentView.EWEFormId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can republish this survey.");
                                        DialogResult result4 = OrgKeyDialog.ShowDialog();
                                        if (result4 == System.Windows.Forms.DialogResult.OK)
                                            {
                                            this.EWEOrganizationKey = OrgKeyDialog.OrganizationKey;
                                            if (!string.IsNullOrWhiteSpace(EWEOrganizationKey))
                                                {
                                                SetFormInfo();
                                                dialog = new WebEnterPublishDialog(this.EWEOrganizationKey, this.mediator, template.CreateWebEnterTemplate());
                                                dialog.ShowDialog();
                                                }
                                            }
                                        }
                                    break;
                                    case Epi.Core.ServiceClient.EWEServiceClient.IsValidOrganizationKeyEnum.Yes:
                                    SetFormInfo();
                                    dialog = new WebEnterPublishDialog(this.EWEOrganizationKey, this.mediator, template.CreateWebEnterTemplate());
                                    dialog.ShowDialog();
                                    break;
                                }

                            }
                        else  //valitate OrgId and FormId
                            {
                            if (!string.IsNullOrEmpty(this.EWEOrganizationKey))
                                {
                                WebEnterPublishDialog dialog = new WebEnterPublishDialog(this.EWEOrganizationKey, this.mediator, template.CreateWebEnterTemplate());
                                dialog.ShowDialog();
                                }
                            else
                                {
                                try
                                    {

                                    var client = Epi.Core.ServiceClient.EWEServiceClient.GetClient();
                                    EWEManagerService.OrganizationRequest Request = new Epi.EWEManagerService.OrganizationRequest();
                                    //Epi.Web.Common.Message.OrganizationRequest Request = new Epi.Web.Common.Message.OrganizationRequest();
                                     Request.Organization = new EWEManagerService.OrganizationDTO();
                                    var TestService = client.GetOrganization(Request);

                                    WebEnterPublishDialog dialog = new WebEnterPublishDialog(null, this.mediator, template.CreateWebEnterTemplate());
                                    DialogResult result = dialog.ShowDialog();
                                    if (result == System.Windows.Forms.DialogResult.Cancel)
                                        {
                                        this.EWEOrganizationKey = dialog.GetOrgKey;
                                        }

                                    }
                                catch (Exception ex)
                                    {
                                    WebEnterOptions dialog2 = new WebEnterOptions();
                                    DialogResult result3 = dialog2.ShowDialog();
                                    if (result3 == System.Windows.Forms.DialogResult.OK)
                                        {

                                        WebEnterPublishDialog dialog = new WebEnterPublishDialog(this.EWEOrganizationKey, this.mediator, template.CreateWebEnterTemplate());
                                        DialogResult result = dialog.ShowDialog();
                                        if (result == System.Windows.Forms.DialogResult.Cancel)
                                            {
                                            this.EWEOrganizationKey = dialog.GetOrgKey;
                                            }


                                        }
                                    }
                                }
                            }
                        }
                    else // IsRepbulishable
                        {
                        try
                            {

                            SurveyManagerService.ManagerServiceV3Client client = Epi.Core.ServiceClient.ServiceClient.GetClient();
                                SurveyManagerService.OrganizationRequest Request = new SurveyManagerService.OrganizationRequest();
                                SurveyManagerService.OrganizationDTO orgDTO = new SurveyManagerService.OrganizationDTO();
                                Request.Organization = orgDTO;
                            var TestService = client.GetOrganization(Request);

                            WebEnterPublishDialog dialog = new WebEnterPublishDialog(null, this.mediator, template.CreateWebEnterTemplate());
                            DialogResult result = dialog.ShowDialog();
                            if (result == System.Windows.Forms.DialogResult.Cancel)
                                {
                                this.EWEOrganizationKey = dialog.GetOrgKey;
                                }

                            }
                        catch (Exception ex)
                            {


                            if (config.Settings.Republish_IsRepbulishable == true)
                                {
                                WebSurveyOptions dialog2 = new WebSurveyOptions();
                                DialogResult result3 = dialog2.ShowDialog();
                                if (result3 == System.Windows.Forms.DialogResult.OK)
                                    {

                                    WebEnterPublishDialog dialog = new WebEnterPublishDialog(this.EWEOrganizationKey, this.mediator, template.CreateWebEnterTemplate());
                                    DialogResult result = dialog.ShowDialog();
                                    if (result == System.Windows.Forms.DialogResult.Cancel)
                                        {
                                        this.EWEOrganizationKey = dialog.GetOrgKey;
                                        }

                                    }
                                }
                            else
                                {

                                WebEnterPublishDialog dialog = new WebEnterPublishDialog(null, this.mediator, template.CreateWebEnterTemplate());
                                 dialog.ShowDialog();
                                
                                }
                            }
                        }
                        }
                       
                        else if(!iscancel)// Validate User
                        {
                            int ISWindowAuthMode = config.Settings.EWEServiceAuthMode;

                            if (ISWindowAuthMode == 0)
                            {
                                if (LoginInfo.UserID == -1)
                                {
                                    UserAuthentication dialog = new UserAuthentication();
                                    DialogResult result = dialog.ShowDialog();
                                    if (result == System.Windows.Forms.DialogResult.OK)
                                    {
                                        dialog.Close();
                                        if (string.IsNullOrEmpty(this.EWEOrganizationKey))
                                        {
                                            WebEnterPublishDialog dialog1 = new WebEnterPublishDialog(null, this.mediator, template.CreateWebEnterTemplate());
                                            dialog1.ShowDialog();
                                        }
                                        else
                                        {
                                            WebEnterPublishDialog dialog1 = new WebEnterPublishDialog(this.EWEOrganizationKey, this.mediator, template.CreateWebEnterTemplate());
                                            dialog1.ShowDialog();

                                        }
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(this.EWEOrganizationKey))
                                    {
                                        WebEnterPublishDialog dialog1 = new WebEnterPublishDialog(null, this.mediator, template.CreateWebEnterTemplate());
                                        dialog1.ShowDialog();
                                    }
                                    else
                                    {
                                        WebEnterPublishDialog dialog1 = new WebEnterPublishDialog(this.EWEOrganizationKey, this.mediator, template.CreateWebEnterTemplate());
                                        dialog1.ShowDialog();

                                    }

                                }
                            }

                            else
                            {
                                MessageBox.Show("You are not authorized to publish this form to Epi Web Enter system. Please contact system admin for more info.");



                            }
                        }
                }
                catch (Exception ex)// not republishable
                    {
                    WebEnterPublishDialog dialog = new WebEnterPublishDialog(null, this.mediator, template.CreateWebEnterTemplate());
                    dialog.ShowDialog();
                    }
                }

            this.SetPublishMenuItems(view);
            }

        private void CreateViewDataTable(View view)
        {
            view.SetTableName(view.Name);
            this.mediator.Project.CollectedData.CreateDataTableForView(view, 1);
            this.createDataTableToolStripMenuItem.Enabled = false;
            this.DeleteDataTableItemEnabled = true;
            foreach (View v in view.GetDescendantViews())
            {
                if (!this.mediator.Project.CollectedData.TableExists(v.TableName))
                {
                    view.SetTableName(v.Name);
                    this.mediator.Project.CollectedData.CreateDataTableForView(v, 1);
                }
            }
            view.TableName = view.Name;                 
            this.mediator.Project.CollectedData.SynchronizeDataTable(view);
        }
       
        private bool ValidateUser()
            {
            iscancel = false;
            bool IsValidUser = false;            
            string UserName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString();
            Configuration config = Configuration.GetNewInstance();
            int ISWindowAuthMode = config.Settings.EWEServiceAuthMode;


            UserPrincipal User = GetUser(UserName);

            EWEManagerService.UserAuthenticationRequest Request = new EWEManagerService.UserAuthenticationRequest();
            var rUser = new EWEManagerService.UserDTO();
            rUser.EmailAddress = User.EmailAddress;
            Request.User = rUser;
            Request.User.Operation = EWEManagerService.ConstantOperationMode.NoChange;
            try
                {
           var client = Epi.Core.ServiceClient.EWEServiceClient.GetClient();
            var Result = client.GetUser(Request);
            if (Result != null && ISWindowAuthMode == 1)
             {
              IsValidUser = true;
              LoginInfo.UserID = Result.User.UserId;
              }

            return IsValidUser;
            }
             catch (Exception ex) 
                 {
                 Template template = new Template(this.mediator);
                 WebEnterOptions dialog2 = new WebEnterOptions();
                 DialogResult result3 = dialog2.ShowDialog();
                 if (result3 == DialogResult.Cancel)
                 {
                     iscancel = true;
                     dialog2.Close();
                 }

                 //if (result3 == System.Windows.Forms.DialogResult.OK)
                 //    {

                 //    WebEnterPublishDialog dialog = new WebEnterPublishDialog(this.EWEOrganizationKey, this.mediator, template.CreateWebEnterTemplate());
                 //    DialogResult result = dialog.ShowDialog();
                 //    if (result == System.Windows.Forms.DialogResult.Cancel)
                 //        {
                 //        this.EWEOrganizationKey = dialog.GetOrgKey;
                 //        }


                 //    }
                 return IsValidUser;
                 }
            }
        public  UserPrincipal GetUser(string UserName)
            {
            PrincipalContext PrincipalContext = new PrincipalContext(ContextType.Domain);
            UserPrincipal UserPrincipal =
               UserPrincipal.FindByIdentity(PrincipalContext, UserName);
            return UserPrincipal;
            }
        

        private void toWebSurveyToolStripMenuItem_Click(object sender, EventArgs e)
            {
            Template template = new Template(this.mediator);
            try
                {

                    Epi.Core.ServiceClient.ServiceClient.IsValidOrganizationKeyEnum IsValidOKey = Epi.Core.ServiceClient.ServiceClient.IsValidOrganizationKeyEnum.No;
                if (string.IsNullOrWhiteSpace(this.projectExplorer.CurrentView.WebSurveyId))
                    {
                        IsValidOKey = Epi.Core.ServiceClient.ServiceClient.IsValidOrgKey(this.OrganizationKey);
                    }
                else
                    {
                        IsValidOKey = Epi.Core.ServiceClient.ServiceClient.IsValidOrgKey(this.OrganizationKey, this.projectExplorer.CurrentView.WebSurveyId);
                    }
                WebPublishDialog dialog = null;

                switch (IsValidOKey)
                    {
                        case Epi.Core.ServiceClient.ServiceClient.IsValidOrganizationKeyEnum.No:
                        OrgKey NewDialog = new OrgKey(this.CurrentView.WebSurveyId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can republish this survey.");
                        DialogResult result = NewDialog.ShowDialog();
                        if (result == System.Windows.Forms.DialogResult.OK)
                            {
                            this.OrganizationKey = NewDialog.OrganizationKey;
                            if (!string.IsNullOrWhiteSpace(OrganizationKey))
                                {
                                SetSurveyInfo();
                                QuickSurveyInfoUpdate();
                                }
                            }
                        break;
                        case Epi.Core.ServiceClient.ServiceClient.IsValidOrganizationKeyEnum.EndPointNotFound:
                        WebSurveyOptions dialog1 = new WebSurveyOptions();
                        DialogResult result1 = dialog1.ShowDialog();
                        if (result1 == System.Windows.Forms.DialogResult.OK)
                            {
                            OrgKey OrgKeyDialog = new OrgKey(this.CurrentView.WebSurveyId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can republish this survey.");
                            DialogResult result2 = OrgKeyDialog.ShowDialog();
                            if (result2 == System.Windows.Forms.DialogResult.OK)
                                {
                                this.OrganizationKey = OrgKeyDialog.OrganizationKey;
                                if (!string.IsNullOrWhiteSpace(OrganizationKey))
                                    {
                                    SetSurveyInfo();
                                    QuickSurveyInfoUpdate();
                                    }
                                }
                            }
                        break;
                        case Epi.Core.ServiceClient.ServiceClient.IsValidOrganizationKeyEnum.GeneralException:
                        WebSurveyOptions dialog2 = new WebSurveyOptions();
                        DialogResult result3 = dialog2.ShowDialog();
                        if (result3 == System.Windows.Forms.DialogResult.OK)
                            {
                            OrgKey OrgKeyDialog = new OrgKey(this.CurrentView.WebSurveyId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can republish this survey.");
                            DialogResult result4 = OrgKeyDialog.ShowDialog();
                            if (result4 == System.Windows.Forms.DialogResult.OK)
                                {
                                this.OrganizationKey = OrgKeyDialog.OrganizationKey;
                                if (!string.IsNullOrWhiteSpace(OrganizationKey))
                                    {
                                    SetSurveyInfo();
                                    QuickSurveyInfoUpdate();
                                    }
                                }
                            }
                        break;
                        case Epi.Core.ServiceClient.ServiceClient.IsValidOrganizationKeyEnum.Yes:
                        SetSurveyInfo();
                        QuickSurveyInfoUpdate();
                        break;
                    }

                }
            catch (Exception ex)// not republishable
                {

                }


            }

        private void toWebEnterToolStripMenuItem_Click(object sender, EventArgs e)
            {
            DataTable table;
            View RootView = this.mediator.Project.Metadata.GetParentView(this.mediator.ProjectExplorer.CurrentView.Id);
            if (RootView == null)
                {
                table = this.mediator.Project.Metadata.GetPublishedViewKeys(this.mediator.ProjectExplorer.CurrentView.Id);
                }
            else
                {
                table = this.mediator.Project.Metadata.GetPublishedViewKeys(RootView.Id);
                }
            DataRow ViewRow = table.Rows[0];

            string WebSurveyId = ViewRow.ItemArray[2].ToString();
            this.EWEOrganizationKey = ViewRow.ItemArray[3].ToString();

            if (!string.IsNullOrWhiteSpace(EWEOrganizationKey))
            {
                SetFormInfo();
                QuickFormInfoUpdate();
            }
            else
            {
                EWEOrgKey NewDialog = new EWEOrgKey(WebSurveyId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can republish this survey.");
                DialogResult result = NewDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    this.EWEOrganizationKey = NewDialog.OrganizationKey;
                    if (!string.IsNullOrWhiteSpace(EWEOrganizationKey))
                    {
                        SetFormInfo();
                        QuickFormInfoUpdate();
                    }
                }
            }
                                
            //Template template = new Template(this.mediator);
            //try
            //    {

            //        Epi.Core.ServiceClient.EWEServiceClient.IsValidOrganizationKeyEnum IsValidOKey = Epi.Core.ServiceClient.EWEServiceClient.IsValidOrganizationKeyEnum.No;
            //    if (string.IsNullOrWhiteSpace(WebSurveyId))
            //        {
            //            IsValidOKey = Epi.Core.ServiceClient.EWEServiceClient.IsValidOrgKey(this.EWEOrganizationKey);
            //        }
            //    else
            //        {
            //            IsValidOKey = Epi.Core.ServiceClient.EWEServiceClient.IsValidOrgKey(WebSurveyId,this.projectExplorer.CurrentView.EWEFormId);
            //        }
            //    WebEnterPublishDialog dialog = null;

            //    switch (IsValidOKey)
            //        {
            //            case Epi.Core.ServiceClient.EWEServiceClient.IsValidOrganizationKeyEnum.No:
            //            EWEOrgKey NewDialog = new EWEOrgKey(WebSurveyId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can republish this survey.");
            //            DialogResult result = NewDialog.ShowDialog();
            //            if (result == System.Windows.Forms.DialogResult.OK)
            //                {
            //                this.EWEOrganizationKey = NewDialog.OrganizationKey;
            //                if (!string.IsNullOrWhiteSpace(EWEOrganizationKey))
            //                    {
            //                    SetFormInfo();
            //                    QuickFormInfoUpdate();
            //                    }
            //                }
            //            break;
            //            case Epi.Core.ServiceClient.EWEServiceClient.IsValidOrganizationKeyEnum.EndPointNotFound:
            //            WebEnterOptions dialog1 = new WebEnterOptions();
            //            DialogResult result1 = dialog1.ShowDialog();
            //            if (result1 == System.Windows.Forms.DialogResult.OK)
            //                {
            //                EWEOrgKey OrgKeyDialog = new EWEOrgKey(WebSurveyId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can republish this survey.");
            //                DialogResult result2 = OrgKeyDialog.ShowDialog();
            //                if (result2 == System.Windows.Forms.DialogResult.OK)
            //                    {
            //                    this.EWEOrganizationKey = OrgKeyDialog.OrganizationKey;
            //                    if (!string.IsNullOrWhiteSpace(EWEOrganizationKey))
            //                        {
            //                        SetFormInfo();
            //                        QuickFormInfoUpdate();
            //                        }
            //                    }
            //                }
            //            break;
            //            case Epi.Core.ServiceClient.EWEServiceClient.IsValidOrganizationKeyEnum.GeneralException:
            //            WebEnterOptions dialog2 = new WebEnterOptions();
            //            DialogResult result3 = dialog2.ShowDialog();
            //            if (result3 == System.Windows.Forms.DialogResult.OK)
            //                {
            //                EWEOrgKey OrgKeyDialog = new EWEOrgKey(WebSurveyId, false, "The organization key has been successfully submitted!", "The organization key is required for security purposes before you can republish this survey.");
            //                DialogResult result4 = OrgKeyDialog.ShowDialog();
            //                if (result4 == System.Windows.Forms.DialogResult.OK)
            //                    {
            //                    this.EWEOrganizationKey = OrgKeyDialog.OrganizationKey;
            //                    if (!string.IsNullOrWhiteSpace(EWEOrganizationKey))
            //                        {
            //                        SetFormInfo();
            //                        QuickFormInfoUpdate();
            //                        }
            //                    }
            //                }
            //            break;
            //            case Epi.Core.ServiceClient.EWEServiceClient.IsValidOrganizationKeyEnum.Yes:
            //            SetFormInfo();
            //            QuickFormInfoUpdate();
            //            break;
            //        }

            //    }
            //catch (Exception ex)// not republishable
            //    {

            //    }
            }

        private void draftToolStripMenuItem1_Click(object sender, EventArgs e)
            {
            UpdateMode(true);
            finalToolStripMenuItem1.CheckState = CheckState.Unchecked;
            draftToolStripMenuItem1.CheckState = CheckState.Checked;
            }

        private void draftToolStripMenuItem2_Click(object sender, EventArgs e)
            {
            UpdateWebEnterMode(true);
            finalToolStripMenuItem2.CheckState = CheckState.Unchecked;
            draftToolStripMenuItem2.CheckState = CheckState.Checked;
            }

        private void finalToolStripMenuItem1_Click(object sender, EventArgs e)
            {

            UpdateMode(false);
            draftToolStripMenuItem1.CheckState = CheckState.Unchecked;
            finalToolStripMenuItem1.CheckState = CheckState.Checked;
            }

        private void finalToolStripMenuItem2_Click(object sender, EventArgs e)
            {

            UpdateWebEnterMode(false);
            draftToolStripMenuItem2.CheckState = CheckState.Unchecked;
            finalToolStripMenuItem2.CheckState = CheckState.Checked;
            }
        
       
    }
}
