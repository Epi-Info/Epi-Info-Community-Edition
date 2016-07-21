using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using EpiInfo.Plugin;
using Epi.Data.Services;
using Epi.Fields;
using Epi.Windows.Controls;
using Epi.Windows.MakeView.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Drawing.Printing;


namespace Epi.Windows.MakeView.PresentationLogic
{
    /// <summary>
    /// Mediator for Make View's GUI
    /// </summary>
    public partial class GuiMediator : IEnterInterpreterHost
    {
        #region Private Data Members
        private IServiceProvider serviceProvider;
        private static Object classLock = typeof(GuiMediator);
        private Epi.Windows.MakeView.Forms.ProjectExplorer projectExplorer;
        private Epi.Windows.MakeView.Forms.Canvas canvas;
        private Project project;
        private IFieldControl rightClickedControl;
        private SortedDictionary<IFieldControl, Point> selectedFieldControls;
        private Dictionary<IFieldControl, Point> initialSelectedControlLocations;
        private Dictionary<Field, Point> fieldClipboard;
        private SortedDictionary<Int64, BackupField> backupFields;
        private Int64 backupFieldIndex;
        private bool isSelectedControlEmpty = true;
        private bool isCutPasteNotCopy = false;
        private int runningTabIndex = 1;
        private bool awaitingFirstTabClick = true;
        private InitialCollectedData initialCollectedData = null;
        private Field _fieldBefore = null;
        private List<Control> _pastedFields = new List<Control>();
        private List<int> _newFieldIds = new List<int>();
        private int _arrowKeyDownCount = 0;
        private System.Drawing.Printing.PrintDocument printDocument;
        private ArrayList pageImageList;
        private Bitmap memoryImage;
        private int currentPrintPage;
        #endregion Private Data Members

        #region Public Events

        /// <summary>
        /// Raised when a long operation is about to start
        /// </summary>        /// 
        public event BeginBusyEventHandler BeginBusyEvent;

        /// <summary>
        /// Raised at the end of a long operation.
        /// </summary>
        public event EndBusyEventHandler EndBusyEvent;

        /// <summary>
        /// Declaration of Progress Report Begin evebt handler
        /// </summary>
        public event ProgressReportBeginEventHandler ProgressReportBeginEvent;

        private void RaiseProgressReportBeginEvent(int min, int max, int step)
        {
            if (this.ProgressReportBeginEvent != null)
            {
                this.ProgressReportBeginEvent(min, max, step);
            }
        }

        /// <summary>
        /// Declaration of Progress Report Update event handler
        /// </summary>
        public event ProgressReportUpdateEventHandler ProgressReportUpdateEvent;

        private void RaiseProgressReportUpdateEvent()
        {
            if (this.ProgressReportUpdateEvent != null)
            {
                this.ProgressReportUpdateEvent();
            }
        }

        /// <summary>
        /// Declaration of Progress Report End event handler
        /// </summary>
        public event SimpleEventHandler ProgressReportEndEvent;

        private void RaiseProgressReportEndEvent()
        {
            if (this.ProgressReportEndEvent != null)
            {
                this.ProgressReportEndEvent();
            }
        }


        #endregion Events

        #region Constructors

        private GuiMediator(IServiceProvider serviceProvider)
        {
            selectedFieldControls = new SortedDictionary<IFieldControl, Point>(new ControlTopComparer());
            InitialSelectedControlLocations = new Dictionary<IFieldControl, Point>();
            fieldClipboard = new Dictionary<Field, Point>(); // the point is the offset
            backupFields = new SortedDictionary<Int64, BackupField>(new BackupFieldComparer());
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates instance of Mediator
        /// </summary>
        /// <param name="serviceProvider">The service provider</param>
        /// <returns>An instance of the mediator</returns>
        public static GuiMediator CreateInstance(IServiceProvider serviceProvider)
        {
            return new GuiMediator(serviceProvider);
        }

        #endregion

        #region Public Properties

        public bool isShowTabOrder { get; set; }
        public bool isShowFieldName { get; set; }

        public List<int> NewFieldIds
        {
            get { return _newFieldIds;  }
            set { _newFieldIds = value; }
        }

        public InitialCollectedData InitialData
        {
            get { return initialCollectedData;  }
            set { initialCollectedData = value; }
        }

        public Int64 BackupFieldIndex
        {
            get 
            { 
                return backupFieldIndex; 
            }
            set
            {
                backupFieldIndex = value;

                if (backupFieldIndex == 0)
                {
                    ((MakeViewMainForm)canvas.MainForm).UndoButtonEnabled = false;
                }
                else
                {
                    ((MakeViewMainForm)canvas.MainForm).UndoButtonEnabled = true;
                }
            }
        }

        public IFieldControl RightClickedControl
        {
            get { return rightClickedControl; }
            set { rightClickedControl = value; }
        }

        /// <summary>
        /// IsSelectedControlEmpty        
        /// </summary>
        // << dpb - change this to model IsControlClipboardEmpty >>
        public bool IsSelectedControlEmpty
        {
            get { return isSelectedControlEmpty; }
            set
            {
                isSelectedControlEmpty = value;
                Canvas.EnableCutCopyMenuItem = isSelectedControlEmpty ? false : true;
            }
        }

        /// <summary>
        /// IsControlClipboardEmpty
        /// </summary>
        public bool IsControlClipboardEmpty
        {
            get
            {
                if (fieldClipboard == null || fieldClipboard.Count == 0)
                {
                    return true;
                }
                else { return false; }
            }
        }

        /// <summary>
        /// IsTableFull
        /// </summary>
        public Boolean IsTableFull
        {
            get
            {
                bool full = false;
                View currentView = this.ProjectExplorer.SelectedPage.GetView();
                int actual = this.ProjectExplorer.SelectedPage.Fields.Count;
                int max = currentView.Project.CollectedData.TableColumnMax;
                if ((actual >= max))
                {
                    MessageBox.Show(SharedStrings.CANNOT_ADD_FIELD_VIEW_MAX, SharedStrings.MAKE_VIEW, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    full = true;
                }
                return full;
            }
        }

        /// <summary>
        /// Gets or sets the current project explorer
        /// </summary>
        public Epi.Windows.MakeView.Forms.ProjectExplorer ProjectExplorer
        {
            get
            {
                return projectExplorer;
            }
            set
            {
                projectExplorer = value;
                projectExplorer.PageSelected += new Epi.Windows.MakeView.Forms.ProjectExplorerPageSelectedEventHandler(projectExplorer_PageSelected);
            }
        }

        /// <summary>
        /// Gets or sets the current Make View canvas
        /// </summary>
        public Epi.Windows.MakeView.Forms.Canvas Canvas
        {
            get
            {
                return canvas;
            }
            set
            {
                canvas = value;
                if (canvas != null)
                {
                    canvas.ControlResized += new ControlMovementEventHandler(canvas_ControlResized);
                    canvas.ControlMoved += new ControlMovementEventHandler(canvas_ControlMoved);
                    canvas.PromptMoved += new ControlMovementEventHandler(canvas_PromptMoved);
                    canvas.FieldCreationRequested += new FieldCreationRequestEventHandler(canvas_FieldCreationRequested);
                    canvas.GroupCreationRequested += new GroupCreationRequestEventHandler(canvas_GroupCreationRequested);
                    canvas.TreeNodeDropped += new TreeNodeDragDropEventHandler(canvas_TreeNodeDropped);
                    canvas.PageCheckCodeRequested += new PageCheckCodeRequestEventHandler(canvas_PageCheckCodeRequested);
                    canvas.ShowTabOrderSelected += new ShowTabOrderEventHandler(OnShowTabOrder);
                    canvas.StartNewTabOrderSelected += new StartNewTabOrderEventHandler(OnStartNewTabOrder);
                    canvas.ContinueTabOrderSelected += new ContinueTabOrderEventHandler(OnContinueTabOrder);
                    canvas.CutFieldControl += new CutFieldControlEventHandler(OnClipboardCutSelection);
                    canvas.CopyFieldControl += new CopyFieldControlEventHandler(OnClipboardCopySelection);
                    canvas.PasteFieldControl += new PasteFieldControlEventHandler(OnClipboardPaste);
                    canvas.DeleteFieldControl += new DeleteFieldControlEventHandler(OnSelectedControlsDelete);
                    canvas.TableAlign += new TableAlignEventHandler(OnSelectedControlsAlignAsTable);
                    canvas.AlignAsRow += new AlignAsRowEventHandler(OnSelectedControlsAsRow);
                    canvas.MakeSame += new MakeSameEventHandler(OnSelectedControlsMakeSame);
                    canvas.CreateTemplate += new CreateTemplateEventHandler(canvas_CreateTemplate);
                    canvas.ApplyDefaultFonts += new ApplyDefaultFontsEventHandler(canvas_ApplyDefaultFonts);
                }
            }
        }

        void canvas_CreateTemplate(string templateName)
        {
            Template template = new Template(this);
            template.CreateTemplate(templateName, selectedFieldControls, projectExplorer.SelectedPage);
        }

        /// <summary>
        /// Gets or sets the current project
        /// </summary>
        public Project Project
        {
            get
            {
                return project;
            }
            set
            {
                project = value;
            }
        }

        /// <summary>
        /// Gets the selectedFieldControls field
        /// </summary>
        public SortedDictionary<IFieldControl, Point> SelectedFieldControls
        {
            get
            {
                return selectedFieldControls;
            }
        }

        /// <summary>
        /// Gets the initialSelectedControlLocations field
        /// </summary>
        public Dictionary<IFieldControl, Point> InitialSelectedControlLocations
        {
            get
            {
                return initialSelectedControlLocations;
            }
            set
            {
                initialSelectedControlLocations = value;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resets the mediator to its initial state
        /// </summary>
        public void Reset()
        {
            selectedFieldControls = new SortedDictionary<IFieldControl, Point>(new ControlTopComparer());
            fieldClipboard = new Dictionary<Field, Point>(); 
            backupFields = new SortedDictionary<Int64, BackupField>(new BackupFieldComparer());
            
            this.project = null;
            projectExplorer.Reset();
            canvas.Reset();
        }

        /// <summary>
        /// Handles the click event of the Check Code button
        /// </summary>
        public void OnCheckCodeButtonClick()
        {
            Epi.Windows.MakeView.Forms.CheckCode chk = new Epi.Windows.MakeView.Forms.CheckCode(projectExplorer.CurrentView, (MakeViewMainForm)projectExplorer.MainForm);
            chk.ShowDialog();
            //chk.Show();
        }

        /// <summary>
        /// Handles the click event of the Check Code button
        /// </summary>
        public void OnNewGroupFieldClick()
        {
            System.Drawing.Rectangle outline;

            if (this.Canvas.GroupFieldOutline == System.Drawing.Rectangle.Empty)
            {
                outline = new System.Drawing.Rectangle(this.Canvas.RightClickLocation, new System.Drawing.Size(150, 60));
            }
            else
            {
                outline = this.Canvas.GroupFieldOutline;
            }

            canvas_GroupCreationRequested(this.Canvas.PagePanel, outline);
        }

        /// <summary>
        /// OnSelectedControlsDelete
        /// </summary>
        public void OnSelectedControlsDelete()
        {
            if (SelectedFieldControls.Count == 0 || AskCancelDeleteFieldCollectedData())
            {
                return;
            }

            canvas.HideUpdateStart("Start delete...");
            List<Field> fields = new List<Field>();
            canvas.DisposeControlTrackers();

            foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
            {
                if (fields.Contains(kvp.Key.Field) == false)
                {
                    fields.Add(kvp.Key.Field);
                }
            }

            foreach (Field field in fields)
            {
                canvas.UpdateHidePanel(string.Format("Deleting {0} field.", field.Name));
                _fieldBefore = CloneField(field);
                PersistFieldChange(field, BackupFieldAction.Delete);
            }

            selectedFieldControls.Clear();
            canvas.HideUpdateEnd();
        }

        public void OnSelectedControlsAlignAsTable()
        {
            OnSelectedControlsAlignAsTable(-1);
        }

        public void OnSelectedControlsAsRow()
        {
            if(canvas == null) return;
            
            canvas.HideUpdateStart("Start order as row...");

            int refTop = canvas.PagePanel.Size.Height;
            int refLeft = canvas.PagePanel.Size.Width;
            int topAccumulator = 0;
            int tabStopAccumulator = 0;
            int combinedWidth = 0;
            int combinedHeight = 0;
            int columnIndex = 0;
            int maxControlHeightOnRow = 0;
            const int minVerticalMarginBetweenControls = 5;
            const int minHorzMarginBetweenLabelAndControls = 10;
            const int minHorzMarginBetweenControls = 15;
            const int pageMargin = 10;

            canvas.DisposeControlTrackers();

            bool isHorizontalAlign = projectExplorer.SelectedPage.GetView().PageLabelAlign.ToLowerInvariant().Equals("horizontal") ? true : false;

            List<String> childFields = new List<string>();

            SortedDictionary<IFieldControl, Point> leftSortControls = new SortedDictionary<IFieldControl, Point>(selectedFieldControls, new ControlLeftComparer());

            foreach (KeyValuePair<IFieldControl, Point> kvp in leftSortControls)
            {
                if (kvp.Key.Field is GroupField)
                {
                    String[] names = ((GroupField)kvp.Key.Field).ChildFieldNames.Split((Constants.LIST_SEPARATOR));
                    childFields.AddRange(names);
                }
            }

            foreach (KeyValuePair<IFieldControl, Point> kvp in leftSortControls)
            {
                if (!(kvp.Key.Field is GroupField || childFields.Contains(kvp.Key.Field.Name))) // < skip groups >
                {
                    refLeft = ((Control)kvp.Key).Left;
                    refTop = ((Control)kvp.Key).Top;
                    break;
                }
            }

            tabStopAccumulator = refLeft = canvas.Snap(refLeft);
            topAccumulator = refTop = canvas.Snap(refTop);

            foreach (KeyValuePair<IFieldControl, Point> kvp in leftSortControls)
            {
                Control control = ((Control)kvp.Key);
                
                canvas.UpdateHidePanel(string.Format("Moving {0}.", ((Control)kvp.Key).Name));
                
                if (!(kvp.Key.Field is GroupField || childFields.Contains(kvp.Key.Field.Name)))// < skip groups >
                {
                    if (kvp.Key.Field is FieldWithSeparatePrompt)
                    {
                        if (kvp.Key is DragableLabel)
                        {
                            if (isHorizontalAlign)
                            {
                                combinedWidth = 0;
                                combinedHeight = 0;
                            }
                            else
                            {
                                combinedWidth = control.Width > ((DragableLabel)kvp.Key).LabelFor.Width ? control.Width : ((DragableLabel)kvp.Key).LabelFor.Width;
                                combinedWidth += minHorzMarginBetweenLabelAndControls;
                                combinedHeight = control.Height + minVerticalMarginBetweenControls + ((DragableLabel)kvp.Key).LabelFor.Height;
                            }

                            maxControlHeightOnRow = combinedHeight > maxControlHeightOnRow ? combinedHeight : maxControlHeightOnRow;

                            if ((tabStopAccumulator + combinedWidth > canvas.PagePanel.Size.Width + pageMargin))
                            {
                                tabStopAccumulator = refLeft;
                                topAccumulator += minVerticalMarginBetweenControls + maxControlHeightOnRow;

                                if (topAccumulator > canvas.PagePanel.Size.Height - pageMargin)
                                {
                                    break;
                                }

                                topAccumulator = canvas.Snap(topAccumulator, true);
                                maxControlHeightOnRow = 0;
                                columnIndex = 0;
                            }

                            _fieldBefore = CloneField(kvp.Key.Field);

                            ((Control)kvp.Key).Top = topAccumulator;
                            ((Control)kvp.Key).Left = tabStopAccumulator;

                            if (((Control)kvp.Key).Height > maxControlHeightOnRow)
                            {
                                maxControlHeightOnRow = ((Control)kvp.Key).Height;
                            }

                            if (((DragableLabel)kvp.Key).LabelFor != null)
                            {
                                ((DragableLabel)kvp.Key).LabelFor.Top = topAccumulator;

                                if (isHorizontalAlign)
                                {
                                    tabStopAccumulator += control.Width + minHorzMarginBetweenLabelAndControls;
                                    tabStopAccumulator = canvas.Snap(tabStopAccumulator, true);
                                    ((DragableLabel)kvp.Key).LabelFor.Left = tabStopAccumulator;

                                    tabStopAccumulator += ((DragableLabel)kvp.Key).LabelFor.Width + minHorzMarginBetweenLabelAndControls;
                                    tabStopAccumulator = canvas.Snap(tabStopAccumulator, true);

                                    if (((DragableLabel)kvp.Key).LabelFor.Height > maxControlHeightOnRow)
                                    {
                                        maxControlHeightOnRow = ((DragableLabel)kvp.Key).LabelFor.Height;
                                    }
                                    
                                    ConvertAndSaveControlPositionToField(kvp.Key);
                                }
                                else
                                {
                                    int promptLeft = ((Epi.Windows.Controls.DragableLabel)((Epi.Windows.Controls.PairedLabel)(kvp.Key))).Left;
                                    int promptTop = ((Epi.Windows.Controls.DragableLabel)((Epi.Windows.Controls.PairedLabel)(kvp.Key))).Top;
                                    int promptHeight = ((Epi.Windows.Controls.DragableLabel)((Epi.Windows.Controls.PairedLabel)(kvp.Key))).Height;

                                    int controlLeft = ((Control)((Epi.Windows.Controls.PairedLabel)(kvp.Key)).LabelFor).Left;
                                    int controlTop = ((Control)((Epi.Windows.Controls.PairedLabel)(kvp.Key)).LabelFor).Top;
                                    int controlHeight = ((Control)((Epi.Windows.Controls.PairedLabel)(kvp.Key)).LabelFor).Height;

                                    controlLeft = tabStopAccumulator;

                                    int vertPadding = -3;

                                    controlTop = promptTop + promptHeight + vertPadding;
                                    controlTop = canvas.Snap(controlTop, true);

                                    combinedHeight = controlHeight + controlTop - promptTop;

                                    if (combinedHeight > maxControlHeightOnRow)
                                    {
                                        maxControlHeightOnRow = combinedHeight;
                                    }

                                    ((Control)((Epi.Windows.Controls.PairedLabel)(kvp.Key)).LabelFor).Left = controlLeft;
                                    ((Control)((Epi.Windows.Controls.PairedLabel)(kvp.Key)).LabelFor).Top = controlTop;
                                    ((Control)((Epi.Windows.Controls.PairedLabel)(kvp.Key)).LabelFor).Height = controlHeight;

                                    ConvertAndSaveControlPositionToField(kvp.Key);
                                    tabStopAccumulator += combinedWidth;
                                    tabStopAccumulator = canvas.Snap(tabStopAccumulator, true);
                                }
                            }

                            columnIndex++;

                            MovePrompt(kvp.Key, false);
                        }
                    }
                    else if (kvp.Key.Field is FieldWithoutSeparatePrompt)
                    {
                        if (childFields.Contains(kvp.Key.Field.Name) == false)
                        {
                            if (tabStopAccumulator + control.Width > canvas.PagePanel.Size.Width + pageMargin)
                            {
                                tabStopAccumulator = refLeft;
                                topAccumulator += minVerticalMarginBetweenControls + maxControlHeightOnRow;

                                if (topAccumulator > canvas.PagePanel.Size.Height - pageMargin)
                                {
                                    break;
                                }

                                topAccumulator = canvas.Snap(topAccumulator, true);
                                maxControlHeightOnRow = 0;
                                columnIndex = 0;
                            }

                            _fieldBefore = CloneField(kvp.Key.Field);

                            ((Control)kvp.Key).Top = topAccumulator;
                            ((Control)kvp.Key).Left = tabStopAccumulator;

                            ConvertAndSaveControlPositionToField(kvp.Key);

                            tabStopAccumulator += control.Width + minHorzMarginBetweenControls;
                            tabStopAccumulator = canvas.Snap(tabStopAccumulator, true);

                            if (((Control)kvp.Key).Height > maxControlHeightOnRow)
                            {
                                maxControlHeightOnRow = ((Control)kvp.Key).Height;
                            }
                            columnIndex++;

                            MoveField(kvp.Key, false);
                        }
                    }
                }
            }

            InitialSelectedControlLocations.Clear();
            selectedFieldControls.Clear();

            SetZeeOrderOfGroups();
            SetBackColorForGroupCheckBoxes(true);
            PersistChildFieldNames();
            canvas.HideUpdateEnd();

            if (((Epi.Windows.Docking.DockPanel)canvas.PagePanel.Parent).AutoScrollPosition != canvas.ScrollLocationAtDrop)
            {
                ((Epi.Windows.Docking.DockPanel)canvas.PagePanel.Parent).AutoScrollPosition = canvas.ScrollLocationAtDrop;
            }
        }



        public void OnSelectedControlsAlignAsTable(int numColumns)
        {
            canvas.HideUpdateStart("Start align...");
           
            int refTop = canvas.PagePanel.Size.Height;
            int refLeft = canvas.PagePanel.Size.Width;
            int topAccumulator = 0;
            int tabStopAccumulator = 0;
            int maxPromptWidth = 0;
            int maxControlWidth = 0;
            int maxControlHeightOnRow = 0;
            int combinedWidth = 0;
            int combinedHeight = 0;
            int columnIndex = 0;
            const int minVerticalMarginBetweenControls = 5;
            const int minHorzMarginBetweenLabelAndControls = 10;
            const int minHorzMarginBetweenControls = 15;
            const int pageMargin = 10;

            canvas.DisposeControlTrackers();

            bool isHorizontalAlign = projectExplorer.SelectedPage.GetView().PageLabelAlign.ToLowerInvariant().Equals("horizontal") ? true : false;

            List<String> childFields = new List<string>();
            // < build a list of controls that are child controls of groups >
            foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
            {
                if (kvp.Key.Field is GroupField)
                {
                    String[] names = ((GroupField)kvp.Key.Field).ChildFieldNames.Split((Constants.LIST_SEPARATOR));
                    childFields.AddRange(names);
                }
            }

            foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
            {
                canvas.UpdateHidePanel(string.Format("Saving reference location of {0}.", ((Control)kvp.Key).Name));
                if (!(kvp.Key.Field is GroupField || childFields.Contains(kvp.Key.Field.Name))) // < skip groups >
                {
                    if (((Control)kvp.Key).Left < refLeft)
                    {
                        refLeft = ((Control)kvp.Key).Left;
                    }
                    if (((Control)kvp.Key).Top < refTop)
                    {
                        refTop = ((Control)kvp.Key).Top;
                    }
                }
            }

            tabStopAccumulator = refLeft = canvas.Snap(refLeft);
            topAccumulator = refTop = canvas.Snap(refTop);

            // < scrape selectedFieldControls for values used in the layout of the controls >
            foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
            {
                canvas.UpdateHidePanel(string.Format("Saving control dimensions {0}.", ((Control)kvp.Key).Name));
                if (kvp.Key.Field is FieldWithSeparatePrompt)
                {
                    if (kvp.Key is DragableLabel)
                    {
                        if (((Control)kvp.Key).Width > maxPromptWidth)
                        {
                            maxPromptWidth = ((Control)kvp.Key).Width;
                        }

                        if (((DragableLabel)kvp.Key).LabelFor != null)
                        {
                            if ((((DragableLabel)kvp.Key).LabelFor).Width > maxControlWidth) maxControlWidth = (((DragableLabel)kvp.Key).LabelFor).Width;
                        }
                    }
                }
                else if (kvp.Key.Field is FieldWithoutSeparatePrompt)
                {
                    if (((Control)kvp.Key).Width > maxControlWidth) maxControlWidth = ((Control)kvp.Key).Width;
                }
            }

            if (isHorizontalAlign)
            {
                combinedWidth = maxPromptWidth + minHorzMarginBetweenLabelAndControls + maxControlWidth;
            }
            else
            {
                combinedWidth = maxControlWidth > maxPromptWidth ? maxControlWidth : maxPromptWidth;
            }

            // < move the controls >
            foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
            {
                canvas.UpdateHidePanel(string.Format("Moving {0}.", ((Control)kvp.Key).Name));
                if (!(kvp.Key.Field is GroupField || childFields.Contains(kvp.Key.Field.Name)))// < skip groups >
                {
                    if ((tabStopAccumulator + combinedWidth > canvas.PagePanel.Size.Width + pageMargin)
                    || (columnIndex == numColumns))
                    {
                        tabStopAccumulator = refLeft;
                        topAccumulator += minVerticalMarginBetweenControls + maxControlHeightOnRow;

                        if (topAccumulator > canvas.PagePanel.Size.Height - pageMargin)
                        {
                            break;
                        }

                        topAccumulator = canvas.Snap(topAccumulator, true);
                        maxControlHeightOnRow = 0;
                        columnIndex = 0;
                    }

                    if (kvp.Key.Field is FieldWithSeparatePrompt)
                    {
                        if (kvp.Key is DragableLabel)
                        {
                            _fieldBefore = CloneField(kvp.Key.Field);
                            
                            ((Control)kvp.Key).Top = topAccumulator;
                            ((Control)kvp.Key).Left = tabStopAccumulator;

                            if (((Control)kvp.Key).Height > maxControlHeightOnRow)
                            {
                                maxControlHeightOnRow = ((Control)kvp.Key).Height;
                            }

                            if (((DragableLabel)kvp.Key).LabelFor != null)
                            {
                                ((DragableLabel)kvp.Key).LabelFor.Top = topAccumulator;

                                if (isHorizontalAlign)
                                {
                                    tabStopAccumulator += maxPromptWidth + minHorzMarginBetweenLabelAndControls;
                                    tabStopAccumulator = canvas.Snap(tabStopAccumulator, true);
                                    ((DragableLabel)kvp.Key).LabelFor.Left = tabStopAccumulator;

                                    if (((DragableLabel)kvp.Key).LabelFor.Height > maxControlHeightOnRow)
                                    {
                                        maxControlHeightOnRow = ((DragableLabel)kvp.Key).LabelFor.Height;
                                    }
                                }
                                else
                                {
                                    int promptLeft = ((Epi.Windows.Controls.DragableLabel)((Epi.Windows.Controls.PairedLabel)(kvp.Key))).Left;
                                    int promptTop = ((Epi.Windows.Controls.DragableLabel)((Epi.Windows.Controls.PairedLabel)(kvp.Key))).Top;
                                    int promptHeight = ((Epi.Windows.Controls.DragableLabel)((Epi.Windows.Controls.PairedLabel)(kvp.Key))).Height;

                                    int controlLeft = ((Control)((Epi.Windows.Controls.PairedLabel)(kvp.Key)).LabelFor).Left;
                                    int controlTop = ((Control)((Epi.Windows.Controls.PairedLabel)(kvp.Key)).LabelFor).Top;
                                    int controlHeight = ((Control)((Epi.Windows.Controls.PairedLabel)(kvp.Key)).LabelFor).Height;

                                    controlLeft = tabStopAccumulator;

                                    int vertPadding = -3;

                                    controlTop = promptTop + promptHeight + vertPadding;
                                    controlTop = canvas.Snap(controlTop, true);

                                    combinedHeight = controlHeight + controlTop - promptTop;

                                    if (combinedHeight > maxControlHeightOnRow)
                                    {
                                        maxControlHeightOnRow = combinedHeight;
                                    }

                                    ((Control)((Epi.Windows.Controls.PairedLabel)(kvp.Key)).LabelFor).Left = controlLeft;
                                    ((Control)((Epi.Windows.Controls.PairedLabel)(kvp.Key)).LabelFor).Top = controlTop;
                                    ((Control)((Epi.Windows.Controls.PairedLabel)(kvp.Key)).LabelFor).Height = controlHeight;
                                }
                            }

                            ConvertAndSaveControlPositionToField(kvp.Key);
                            tabStopAccumulator += maxControlWidth + minHorzMarginBetweenControls;
                            tabStopAccumulator = canvas.Snap(tabStopAccumulator, true);
                            columnIndex++;

                            MovePrompt(kvp.Key, false);
                        }
                    }
                    else if (kvp.Key.Field is FieldWithoutSeparatePrompt)
                    {
                        if (childFields.Contains(kvp.Key.Field.Name) == false)
                        {
                            _fieldBefore = CloneField(kvp.Key.Field);

                            ((Control)kvp.Key).Top = topAccumulator;
 
                            if (isHorizontalAlign)
                            {
                                tabStopAccumulator += maxPromptWidth + minHorzMarginBetweenLabelAndControls;
                                tabStopAccumulator = canvas.Snap(tabStopAccumulator, true);
                            }
                            
                            ((Control)kvp.Key).Left = tabStopAccumulator;

                            ConvertAndSaveControlPositionToField(kvp.Key);
                            
                            tabStopAccumulator += maxControlWidth + minHorzMarginBetweenControls;
                            tabStopAccumulator = canvas.Snap(tabStopAccumulator, true);

                            if (((Control)kvp.Key).Height > maxControlHeightOnRow)
                            {
                                maxControlHeightOnRow = ((Control)kvp.Key).Height;
                            }
                            columnIndex++;

                            MoveField(kvp.Key, false);
                        }
                    }
                }
            }

            InitialSelectedControlLocations.Clear();
            selectedFieldControls.Clear();

            SetZeeOrderOfGroups();
            PersistChildFieldNames();
            canvas.HideUpdateEnd();

            if (((Epi.Windows.Docking.DockPanel)canvas.PagePanel.Parent).AutoScrollPosition != canvas.ScrollLocationAtDrop)
            {
                ((Epi.Windows.Docking.DockPanel)canvas.PagePanel.Parent).AutoScrollPosition = canvas.ScrollLocationAtDrop;
            }
        }

        public void OnSelectedControlsMakeSame(Enums.MakeSame makeSame)
        {
            canvas.HideUpdateStart("Start make same...");

            int maxControlWidth = int.MinValue;
            int minControlWidth = int.MaxValue;
            int maxControlHeight = int.MinValue;
            int minControlHeight = int.MaxValue;

            canvas.DisposeControlTrackers();

            List<String> childFields = new List<string>();
            // < build a list of controls that are child controls of groups >
            foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
            {
                if (kvp.Key.Field is GroupField)
                {
                    String[] names = ((GroupField)kvp.Key.Field).ChildFieldNames.Split((Constants.LIST_SEPARATOR));
                    childFields.AddRange(names);
                }
            }

            // < scrape selectedFieldControls for values used in the layout of the controls >
            foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
            {
                canvas.UpdateHidePanel(string.Format("Saving control dimensions {0}.", ((Control)kvp.Key).Name));
                if (kvp.Key.Field is FieldWithSeparatePrompt)
                {
                    if (kvp.Key is DragableLabel)
                    {
                        if (((DragableLabel)kvp.Key).LabelFor != null)
                        {
                            if ((((DragableLabel)kvp.Key).LabelFor).Width > maxControlWidth)
                            {
                                maxControlWidth = (((DragableLabel)kvp.Key).LabelFor).Width;
                            }

                            if ((((DragableLabel)kvp.Key).LabelFor).Width < minControlWidth)
                            {
                                minControlWidth = (((DragableLabel)kvp.Key).LabelFor).Width;
                            }

                            if ((((DragableLabel)kvp.Key).LabelFor).Height > maxControlHeight)
                            {
                                maxControlHeight = (((DragableLabel)kvp.Key).LabelFor).Height;
                            }

                            if ((((DragableLabel)kvp.Key).LabelFor).Height < minControlHeight)
                            {
                                minControlHeight = (((DragableLabel)kvp.Key).LabelFor).Height;
                            }
                        }
                    }
                }
                else
                {
                    if (((Control)kvp.Key).Width > maxControlWidth)
                    {
                        maxControlWidth = ((Control)kvp.Key).Width;
                    }

                    if (((Control)kvp.Key).Width < minControlWidth)
                    {
                        minControlWidth = ((Control)kvp.Key).Width;
                    }

                    if (((Control)kvp.Key).Height > maxControlHeight)
                    {
                        maxControlHeight = ((Control)kvp.Key).Height;
                    }

                    if (((Control)kvp.Key).Height < minControlHeight)
                    {
                        minControlHeight = ((Control)kvp.Key).Height;
                    }
                }
            }

            // < resize the controls >
            foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
            {
                canvas.UpdateHidePanel(string.Format("Resizing {0}.", ((Control)kvp.Key).Name));
                if (!(kvp.Key.Field is GroupField || childFields.Contains(kvp.Key.Field.Name)))// < skip groups >
                {
                    if (kvp.Key.Field is FieldWithSeparatePrompt)
                    {
                        if (kvp.Key is DragableLabel)
                        {
                            _fieldBefore = CloneField(kvp.Key.Field);

                            if (((DragableLabel)kvp.Key).LabelFor != null)
                            {
                                switch (makeSame)
                                {
                                    case Enums.MakeSame.Width_Use_Maximum:
                                        ((Control)((Epi.Windows.Controls.PairedLabel)(kvp.Key)).LabelFor).Width = maxControlWidth;
                                        break;
                                    case Enums.MakeSame.Width_Use_Minimum:
                                        ((Control)((Epi.Windows.Controls.PairedLabel)(kvp.Key)).LabelFor).Width = minControlWidth;
                                        break;
                                    case Enums.MakeSame.Height_Use_Maximum:
                                        ((Control)((Epi.Windows.Controls.PairedLabel)(kvp.Key)).LabelFor).Height = maxControlHeight;
                                        break;
                                    case Enums.MakeSame.Height_Use_Minimum:
                                        ((Control)((Epi.Windows.Controls.PairedLabel)(kvp.Key)).LabelFor).Height = minControlHeight;
                                        break;
                                    case Enums.MakeSame.Size_Use_Maximum:
                                        ((Control)((Epi.Windows.Controls.PairedLabel)(kvp.Key)).LabelFor).Width = maxControlWidth;
                                        ((Control)((Epi.Windows.Controls.PairedLabel)(kvp.Key)).LabelFor).Height = maxControlHeight;
                                        break;
                                    case Enums.MakeSame.Size_Use_Minimum:
                                        ((Control)((Epi.Windows.Controls.PairedLabel)(kvp.Key)).LabelFor).Width = minControlWidth;
                                        ((Control)((Epi.Windows.Controls.PairedLabel)(kvp.Key)).LabelFor).Height = minControlHeight;
                                        break;
                                }
                            }

                            ConvertAndSaveControlPositionToField(kvp.Key);
                            MovePrompt(kvp.Key, false);
                        }
                    }
                    else
                    {
                        _fieldBefore = CloneField(kvp.Key.Field);

                        switch (makeSame)
                        {
                            case Enums.MakeSame.Width_Use_Maximum:
                                ((Control)kvp.Key).Width = maxControlWidth;
                                break;
                            case Enums.MakeSame.Width_Use_Minimum:
                                ((Control)kvp.Key).Width = minControlWidth;
                                break;
                            case Enums.MakeSame.Height_Use_Maximum:
                                ((Control)kvp.Key).Height = maxControlHeight;
                                break;
                            case Enums.MakeSame.Height_Use_Minimum:
                                ((Control)kvp.Key).Height = minControlHeight;
                                break;
                            case Enums.MakeSame.Size_Use_Maximum:
                                ((Control)kvp.Key).Width = maxControlWidth;
                                ((Control)kvp.Key).Height = maxControlHeight;
                                break;
                            case Enums.MakeSame.Size_Use_Minimum:
                                ((Control)kvp.Key).Width = minControlWidth;
                                ((Control)kvp.Key).Height = minControlHeight;
                                break;
                        }

                        ConvertAndSaveControlPositionToField(kvp.Key);
                        MoveField(kvp.Key, false);
                    }
                }
            }

            InitialSelectedControlLocations.Clear();
            selectedFieldControls.Clear();

            SetZeeOrderOfGroups();
            SetBackColorForGroupCheckBoxes(true);
            PersistChildFieldNames();
            canvas.HideUpdateEnd();

            if (((Epi.Windows.Docking.DockPanel)canvas.PagePanel.Parent).AutoScrollPosition != canvas.ScrollLocationAtDrop)
            {
                ((Epi.Windows.Docking.DockPanel)canvas.PagePanel.Parent).AutoScrollPosition = canvas.ScrollLocationAtDrop;
            }
        }

        private void ConvertAndSaveControlPositionToField(IFieldControl control)
        {
            if (control.Field is FieldWithSeparatePrompt)
            {
                if (control is DragableLabel)
                {
                    ((FieldWithSeparatePrompt)control.Field).PromptLeftPositionPercentage = AsPercentage(((Control)control).Left, canvas.PagePanel.Size.Width);
                    ((FieldWithSeparatePrompt)control.Field).PromptTopPositionPercentage = AsPercentage(((Control)control).Top, canvas.PagePanel.Size.Height);
                    ((FieldWithSeparatePrompt)control.Field).ControlLeftPositionPercentage = AsPercentage(((DragableLabel)control).LabelFor.Left, canvas.PagePanel.Size.Width);
                    ((FieldWithSeparatePrompt)control.Field).ControlTopPositionPercentage = AsPercentage(((DragableLabel)control).LabelFor.Top, canvas.PagePanel.Size.Height);

                    ((FieldWithSeparatePrompt)control.Field).ControlWidthPercentage = AsPercentage(((DragableLabel)control).LabelFor.Width, canvas.PagePanel.Size.Width);
                    ((FieldWithSeparatePrompt)control.Field).ControlHeightPercentage = AsPercentage(((DragableLabel)control).LabelFor.Height, canvas.PagePanel.Size.Height);
                }
                else
                {
                    // dpb todo
                }
            }
            else if (control.Field is FieldWithoutSeparatePrompt)
            {
                ((FieldWithoutSeparatePrompt)control.Field).ControlLeftPositionPercentage = AsPercentage(((Control)control).Left, canvas.PagePanel.Size.Width);
                ((FieldWithoutSeparatePrompt)control.Field).ControlTopPositionPercentage = AsPercentage(((Control)control).Top, canvas.PagePanel.Size.Height);

                ((FieldWithoutSeparatePrompt)control.Field).ControlWidthPercentage = AsPercentage(((Control)control).Width, canvas.PagePanel.Size.Width);
                ((FieldWithoutSeparatePrompt)control.Field).ControlHeightPercentage = AsPercentage(((Control)control).Height, canvas.PagePanel.Size.Height);
            }
        }

        private void canvas_ApplyDefaultFonts()
        {
            canvas.HideUpdateStart("Apply default font setting...");
            canvas.DisposeControlTrackers();

            Epi.DataSets.Config.SettingsRow settings = Configuration.GetNewInstance().Settings;

            foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
            {
                canvas.UpdateHidePanel(string.Format("Updating {0}.", ((Control)kvp.Key).Name));

                if (kvp.Key.Field is FieldWithSeparatePrompt)
                {
                    if (kvp.Key is DragableLabel)
                    {
                        if (((DragableLabel)kvp.Key).LabelFor != null)
                        {
                            _fieldBefore = CloneField(kvp.Key.Field);
                            kvp.Key.Field = ApplyDefaultFont(settings, kvp.Key.Field);
                            PersistFieldChange(kvp.Key.Field, BackupFieldAction.Change);
                            PanelFieldUpdate(kvp.Key.Field);
                        }
                    }
                }
                else
                {
                    _fieldBefore = CloneField(kvp.Key.Field);
                    kvp.Key.Field = ApplyDefaultFont(settings, kvp.Key.Field);
                    PersistFieldChange(kvp.Key.Field, BackupFieldAction.Change);
                    PanelFieldUpdate(kvp.Key.Field);
                }
            }

            InitialSelectedControlLocations.Clear();
            selectedFieldControls.Clear();

            SetZeeOrderOfGroups();
            PersistChildFieldNames();
            canvas.HideUpdateEnd();

            if (((Epi.Windows.Docking.DockPanel)canvas.PagePanel.Parent).AutoScrollPosition != canvas.ScrollLocationAtDrop)
            {
                ((Epi.Windows.Docking.DockPanel)canvas.PagePanel.Parent).AutoScrollPosition = canvas.ScrollLocationAtDrop;
            }
        }

        private Field ApplyDefaultFont(Epi.DataSets.Config.SettingsRow settings, Field field)
        {
            Font promptFont;
            Font controlFont;
            GetDefaultFonts(out promptFont, out controlFont, settings);
            ((RenderableField)field).PromptFont = promptFont;
            ((RenderableField)field).ControlFont = controlFont;

            return field;
        }

        public void GetDefaultFonts(out Font promptFont, out Font controlFont, Epi.DataSets.Config.SettingsRow settings = null)
        {
            if (settings == null)
            {
                settings = Configuration.GetNewInstance().Settings;
            }

            string name = settings.ControlFontName;
            decimal size = settings.ControlFontSize;
            FontStyle style = FontStyle.Regular;
            if (settings.ControlFontBold)
            {
                style |= FontStyle.Bold;
            }
            if (settings.ControlFontItalics)
            {
                style |= FontStyle.Italic;
            }

            controlFont = new Font(name, (float)size, style);

            name = settings.EditorFontName;
            size = settings.EditorFontSize;
            style = FontStyle.Regular;
            if (settings.EditorFontBold)
            {
                style |= FontStyle.Bold;
            }
            if (settings.EditorFontItalics)
            {
                style |= FontStyle.Italic;
            }

            promptFont = new Font(name, (float)size, style);
        }

        /// <summary>
        /// Re-sets the canvas position and display properties
        /// </summary>
        public void SetCanvasDisplayProperties()
        {
            if (canvas != null)
            {
                canvas.SetCanvasDisplayProperties();
            }
        }

        /// <summary>
        /// Checks with the user to see if it's okay to discard fields cut in a previous 
        /// action.
        /// </summary>
        /// <returns></returns>
        public bool OkayToClearClipboard()
        {
            bool okay = true;

            if (isCutPasteNotCopy)
            {
                if (fieldClipboard.Count > 0)
                {
                    if (Project.CollectedData.TableExists(this.ProjectExplorer.SelectedPage.GetView().Name))
                    {
                        DialogResult response = MessageBox.Show
                        (
                            SharedStrings.WARNING_OKAY_TO_CLEAR_CLIPBOARD_MESSAGE,
                            SharedStrings.WARNING_OKAY_TO_CLEAR_CLIPBOARD_CAPTION,
                            MessageBoxButtons.YesNo
                        );

                        if (response == DialogResult.No)
                        {
                            okay = false;
                        }
                        else { isCutPasteNotCopy = false; }
                    }
                }
            }
            return okay;
        }

        public bool AskCancelDeleteFieldCollectedData()
        {
            bool cancel = false;
            
            if (project.CollectedData.TableExists(ProjectExplorer.SelectedPage.GetView().TableName))
            {
                cancel = true;
                
                DialogResult result = MessageBox.Show
                    (
                        SharedStrings.DELETE_DATA_FIELD_WARNING,
                        SharedStrings.WARNING,
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Warning
                    );

                if (DialogResult.Yes == result)
                {
                    cancel = false;
                }
            }

            return cancel;
        }

        private void AddSelectedControlsToClipboard()
        {
            canvas.LassoFootprint = null;
            fieldClipboard.Clear();

            if (selectedFieldControls.Count > 0)
            {
                foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
                {
                    if (fieldClipboard.ContainsKey(kvp.Key.Field) == false)
                    {
                        if (((IFieldControl)kvp.Key).Field is FieldWithoutSeparatePrompt)
                        {
                            fieldClipboard.Add(CloneField(kvp.Key.Field), kvp.Value);
                        }
                        if (((IFieldControl)kvp.Key).Field is FieldWithSeparatePrompt)
                        {
                            if (((IFieldControl)kvp.Key) is DragableLabel == false)
                            {
                                fieldClipboard.Add(CloneField(kvp.Key.Field), kvp.Value);
                            }
                        }
                    }
                }
            }

            isCutPasteNotCopy = false;

            if (selectedFieldControls.Count == 0)
            {
                MessageBox.Show(SharedStrings.NO_SELECTION_TO_COPY, SharedStrings.MAKE_VIEW, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Adds a single control to the clipboard
        /// </summary>
        /// <param name="control"></param>
        private void AddControlToClipboard(IFieldControl control)
        {
            bool isGridSelected = false;
            canvas.LassoFootprint = null;
            fieldClipboard.Clear();

            if (control is Epi.Windows.Controls.DragableGrid)
            {
                isGridSelected = true;
            }
            else
            {
                if (fieldClipboard.ContainsKey(control.Field) == false)
                {                                          
                    fieldClipboard.Add(CloneField(control.Field), new Point(0, 0)); 
                }
            }

            isCutPasteNotCopy = false;

            if (isGridSelected)
            {
                MessageBox.Show(SharedStrings.GRID_CANNOT_BE_COPIED, SharedStrings.MAKE_VIEW, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }           
        }

        /// <summary>
        /// Detects when a shortcut key has been called
        /// </summary>
        /// <param name="keyCode">The key that has been pressed</param>
        public void OnShortcutKeyPressed(KeyEventArgs e)
        {            
            Keys keyCode = e.KeyCode;
            
            if (keyCode == Keys.Delete)
            {
                OnSelectedControlsDelete();
            }

            else if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                Point currentScrollLocation = ((Epi.Windows.Docking.DockPanel)canvas.PagePanel.Parent).AutoScrollPosition;
                canvas.ScrollLocationAtDrop = new Point(currentScrollLocation.X * -1, currentScrollLocation.Y * -1);

                switch (keyCode)
                {

                    case Keys.Right:
                        OnSelectedControlsAsRow();
                        break;
                    case Keys.D1:
                        OnSelectedControlsAlignAsTable(1);
                        break;
                    case Keys.D2:
                        OnSelectedControlsAlignAsTable(2);
                        break;
                    case Keys.D3:
                        OnSelectedControlsAlignAsTable(3);
                        break;
                    case Keys.D4:
                        OnSelectedControlsAlignAsTable(4);
                        break;
                    case Keys.D5:
                        OnSelectedControlsAlignAsTable(5);
                        break;
                    case Keys.D6:
                        OnSelectedControlsAlignAsTable(6);
                        break;
                    case Keys.D7:
                        OnSelectedControlsAlignAsTable(7);
                        break;
                    case Keys.D8:
                        OnSelectedControlsAlignAsTable(8);
                        break;
                    case Keys.D9:
                        OnSelectedControlsAlignAsTable(9);
                        break;
                    case Keys.A:
                        OnSelectAllControlsOnPage();
                        break;
                    case Keys.B:
                        OnSelectedControlsBold();
                        break;
                    case Keys.I:
                        OnSelectedControlsItalic();
                        break;
                    case Keys.W:
                        if (e.Shift)
                        {
                            OnSelectedControlsMakeSame(Enums.MakeSame.Width_Use_Minimum);
                        }
                        else
                        {
                            OnSelectedControlsMakeSame(Enums.MakeSame.Width_Use_Maximum);
                        }
                        break;
                    case Keys.H:
                        if (e.Shift)
                        {
                            OnSelectedControlsMakeSame(Enums.MakeSame.Height_Use_Minimum);
                        }
                        else
                        {
                            OnSelectedControlsMakeSame(Enums.MakeSame.Height_Use_Maximum);
                        }
                        break;
                    case Keys.S:
                        if (e.Shift)
                        {
                            OnSelectedControlsMakeSame(Enums.MakeSame.Size_Use_Minimum);
                        }
                        else
                        {
                            OnSelectedControlsMakeSame(Enums.MakeSame.Size_Use_Maximum);
                        }
                        break;
                }
            }
            else 
            {
                switch (keyCode)
                {
                    case Keys.Up:
                    case Keys.Down:
                    case Keys.Left:
                    case Keys.Right:
                        _arrowKeyDownCount += 1;  
                        break;
                    case Keys.Enter:
                        RepeatNewOpenControl();
                        break;
                }
            }
        }

        public void OnShortcutKeyUp(KeyEventArgs e)
        {
            Keys keyCode = e.KeyCode;
            
            switch (keyCode)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    OnSelectedControlsArrow(keyCode, _arrowKeyDownCount);
                    _arrowKeyDownCount = 0;
                    break;
            }
        }

        private void RepeatNewOpenControl()
        {
            if (backupFields.Count > 0)
            {
                SortedDictionary<Int64, BackupField> backupFieldsReversed = new SortedDictionary<Int64, BackupField>(backupFields, new BackupFieldReverseComparer());
                SortedDictionary<Int64, BackupField>.Enumerator enumerator = backupFieldsReversed.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    BackupField backupField = enumerator.Current.Value;

                    if (backupField.BackupAction == BackupFieldAction.New)
                    {
                        ((MakeViewMainForm)canvas.MainForm).projectExplorer_CreateNewOpenField((enumerator.Current.Value.fieldAfter).FieldType);
                        break;
                    }

                    if (backupField.BackupAction != BackupFieldAction.Change)
                    {
                        break;
                    }
                }
            }
        }

        private void OnSelectedControlsArrow(Keys keyCode, int count)
        {
            int verticalIncrement = 0;
            int horizontalIncrement = 0;

            switch (keyCode)
            {
                case Keys.Up:
                    verticalIncrement = -count;
                    break;
                case Keys.Down:
                    verticalIncrement = count;
                    break;
                case Keys.Left:
                    horizontalIncrement = -count;
                    break;
                case Keys.Right:
                    horizontalIncrement = count;
                    break;
            }
            
            List<String> childFields = new List<string>();

            foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
            {
                if (kvp.Key.Field is GroupField)
                {
                    String[] names = ((GroupField)kvp.Key.Field).ChildFieldNames.Split((Constants.LIST_SEPARATOR));
                    childFields.AddRange(names);
                }
            }

            try
            {
                foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
                {
                    if (!(kvp.Key.Field is GroupField || childFields.Contains(kvp.Key.Field.Name)))
                    {
                        if (kvp.Key.Field is FieldWithSeparatePrompt)
                        {
                            if (kvp.Key is DragableLabel)
                            {
                                _fieldBefore = CloneField(kvp.Key.Field);

                                ((Control)kvp.Key).Top += verticalIncrement;
                                ((Control)kvp.Key).Left += horizontalIncrement;

                                if (((DragableLabel)kvp.Key).LabelFor != null)
                                {
                                    ((DragableLabel)kvp.Key).LabelFor.Top += verticalIncrement;
                                    ((DragableLabel)kvp.Key).LabelFor.Left += horizontalIncrement;
                                }

                                ConvertAndSaveControlPositionToField(kvp.Key);
                                MovePrompt(kvp.Key, false);
                            }
                        }
                        else if (kvp.Key.Field is FieldWithoutSeparatePrompt)
                        {
                            if (childFields.Contains(kvp.Key.Field.Name) == false)
                            {
                                _fieldBefore = CloneField(kvp.Key.Field);

                                ((Control)kvp.Key).Top += verticalIncrement;
                                ((Control)kvp.Key).Left += horizontalIncrement;

                                ConvertAndSaveControlPositionToField(kvp.Key);
                                MoveField(kvp.Key, false);
                            }
                        }
                    }
                }
            }
            catch(System.InvalidOperationException)
            {
                return;
            }

            SetZeeOrderOfGroups();
            SetBackColorForGroupCheckBoxes(true);
            PersistChildFieldNames();
 
            if (((Epi.Windows.Docking.DockPanel)canvas.PagePanel.Parent).AutoScrollPosition != canvas.ScrollLocationAtDrop)
            {
                ((Epi.Windows.Docking.DockPanel)canvas.PagePanel.Parent).AutoScrollPosition = canvas.ScrollLocationAtDrop;
            }
        }

        /// <summary>
        /// OnSelectAllControlsOnPage
        /// </summary>
        public void OnSelectAllControlsOnPage()
        {
            HandleNewSelection(canvas.PagePanel.ClientRectangle);
        }

        /// <summary>
        /// Bolds the selected controls
        /// </summary>
        private void OnSelectedControlsBold() 
        {
            bool allSelectedFieldsAreBold = true;
            // First go through each field to see if we should be bolding or de-bolding
            foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
            {
                if (kvp.Key.Field is FieldWithSeparatePrompt && (kvp.Key is Label || kvp.Key is Button))
                {
                    if (((FieldWithSeparatePrompt)kvp.Key.Field).PromptFont.Bold == false)
                    {
                        allSelectedFieldsAreBold = false;
                    }
                }
                if (kvp.Key.Field is FieldWithoutSeparatePrompt && (kvp.Key is Label || kvp.Key is Button))
                {
                    if (((FieldWithoutSeparatePrompt)kvp.Key.Field).PromptFont.Bold == false ||
                        ((FieldWithoutSeparatePrompt)kvp.Key.Field).ControlFont.Bold == false)
                    {
                        allSelectedFieldsAreBold = false;
                    }
                }
            }

            foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
            {
                if (kvp.Key.Field is RenderableField && (kvp.Key is Label || kvp.Key is Button))
                {
                    FontStyle style = FontStyle.Regular;
                    RenderableField renderableField = ((RenderableField)kvp.Key.Field);

                    if (!allSelectedFieldsAreBold)
                    {
                        style |= FontStyle.Bold;
                    }
                    if (renderableField.ControlFont.Italic == true)
                    {
                        style |= FontStyle.Italic;
                    }
                    if (renderableField.ControlFont.Underline == true)
                    {
                        style |= FontStyle.Underline;
                    }
                    
                    renderableField.PromptFont = new Font(((Control)kvp.Key).Font, style);
                    if (renderableField is FieldWithoutSeparatePrompt)
                    {
                        renderableField.ControlFont = new Font(((Control)kvp.Key).Font, style);
                    }
                    PersistFieldChange(kvp.Key.Field, BackupFieldAction.Change);
                    PanelFieldUpdate(kvp.Key.Field);
                }
            }
        }


        /// <summary>
        /// Makes all selected controls italicised
        /// </summary>
        private void OnSelectedControlsItalic()
        {
            bool allSelectedFieldsAreItalic = true;
            // First go through each field to see if we should be making italic or de-italicising
            foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
            {
                if (kvp.Key.Field is FieldWithSeparatePrompt && (kvp.Key is Label || kvp.Key is Button))
                {
                    if (((FieldWithSeparatePrompt)kvp.Key.Field).PromptFont.Italic == false)
                    {
                        allSelectedFieldsAreItalic = false;
                    }
                }
                if (kvp.Key.Field is FieldWithoutSeparatePrompt && (kvp.Key is Label || kvp.Key is Button))
                {
                    if (((FieldWithoutSeparatePrompt)kvp.Key.Field).PromptFont.Italic == false ||
                        ((FieldWithoutSeparatePrompt)kvp.Key.Field).ControlFont.Italic == false)
                    {
                        allSelectedFieldsAreItalic = false;
                    }
                }
            }

            foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
            {
                if (kvp.Key.Field is RenderableField && (kvp.Key is Label || kvp.Key is Button))
                {
                    FontStyle style = FontStyle.Regular;
                    RenderableField renderableField = ((RenderableField)kvp.Key.Field);
                    _fieldBefore = CloneField(renderableField);

                    if (!allSelectedFieldsAreItalic)
                    {
                        style |= FontStyle.Italic;
                    }
                    if (renderableField.ControlFont.Bold == true)
                    {
                        style |= FontStyle.Bold;
                    }
                    if (renderableField.ControlFont.Underline == true)
                    {
                        style |= FontStyle.Underline;
                    }
                    
                    renderableField.PromptFont = new Font(((Control)kvp.Key).Font, style);
                    if (renderableField is FieldWithoutSeparatePrompt)
                    {
                        renderableField.ControlFont = new Font(((Control)kvp.Key).Font, style);
                    }
                    PersistFieldChange(kvp.Key.Field, BackupFieldAction.Change);
                    PanelFieldUpdate(kvp.Key.Field);
                }
            }
        }

        /// <summary>
        /// On Clipboard Cut
        /// </summary>
        public void OnClipboardCutSelection()
        {
            if (OkayToClearClipboard())
            {
                if (AskCancelDeleteFieldCollectedData())
                {
                    return;
                }
                
                AddSelectedControlsToClipboard();
                canvas.HideUpdateStart("Start cutting selected fields");

                selectedFieldControls.Clear();
                canvas.DisposeControlTrackers();
                isCutPasteNotCopy = true;

                foreach (KeyValuePair<Field, Point> kvp in fieldClipboard)
                {
                    canvas.UpdateHidePanel(string.Format("Deleting {0}", kvp.Key.Name));
                    _fieldBefore = CloneField(kvp.Key);
                    PersistFieldChange(kvp.Key, BackupFieldAction.Delete);
                }

                canvas.HideUpdateEnd();
            }
        }

        /// <summary>
        /// On Clipboard Copy
        /// </summary>
        public void OnClipboardCopySelection()
        {
            AddSelectedControlsToClipboard();
        }

        /// <summary>
        /// On Clipboard Cut
        /// </summary>
        public void OnClipboardCut(IFieldControl control)
        {
            if (OkayToClearClipboard())
            {
                AddControlToClipboard(control);
                isCutPasteNotCopy = true;
                _fieldBefore = CloneField(control.Field);
                PersistFieldChange(control.Field, BackupFieldAction.Delete);
            }
        }

        /// <summary>
        /// On Clipboard Copy
        /// </summary>
        public void OnClipboardCopy(IFieldControl control)
        {
            AddControlToClipboard(control);
        }

        public void FocusOnCanvasDesigner()
        {
            canvas.DesignerFocus();
        }

        /// <summary>
        /// On Clipboard Paste
        /// </summary>
        /// <param name="point">point</param>
        public void OnClipboardPaste(System.Drawing.Point point)
        {
            if (point == null)
            {
                point = new System.Drawing.Point(10, 10);
            }
            
            SelectedFieldControls.Clear();
            canvas.HideUpdateStart("Start pasting fields...");

            Point newLocation = new Point();
            Point maxOffsetPoint = new Point();
            int gripBuffer = 40;
            int selectedFieldsTabIndexFloor = int.MaxValue;

            foreach (KeyValuePair<Field, Point> kvp in fieldClipboard)
            {
                maxOffsetPoint.X = kvp.Value.X > maxOffsetPoint.X ? kvp.Value.X : maxOffsetPoint.X;
                maxOffsetPoint.Y = kvp.Value.Y > maxOffsetPoint.Y ? kvp.Value.Y : maxOffsetPoint.Y;

                int newFloorCandidate = (int)Math.Floor(((RenderableField)kvp.Key).TabIndex);

                if (selectedFieldsTabIndexFloor > newFloorCandidate)
                {
                    selectedFieldsTabIndexFloor = newFloorCandidate;
                }
            }

            if (selectedFieldsTabIndexFloor == int.MaxValue)
            {
                selectedFieldsTabIndexFloor = 0; 
            }

            //  canvas.PagePanel.Size.Width<min>
            //  |                                        canvas.PagePanel.Size.Width<max>
            //  |                                        |                                              
            //  ------------------------------------------
            //  |                                     |  |
            //  |                                 --->|  |<---'Grip Buffer'<gripBuffer>
            //  |             |<----moved----|        |  |     
            //  |             o-------------(o)-------A[ |  ]         
            //  |             |              |        |  |            
            //  |             |              'Click'Paste<point>            
            //  |             |                       |  |            
            //  |             |                    B[ |  | ]          
            //  |             |                       |  |            
            //  |             |                       |  |            
            //  |             [  ]B------------------(x)offsetPoint   
            //  |                                     |  |             
            //  |                    [     ]D--------(x)<maxOffsetPoint>         
            //  |                                        |   
            //
            // Note: offsetPoint is actualy a distance.

            if (point.X + maxOffsetPoint.X > canvas.PagePanel.Size.Width)
            {
                point.X = canvas.PagePanel.Size.Width - maxOffsetPoint.X - gripBuffer;
            }

            if (point.Y + maxOffsetPoint.Y > canvas.PagePanel.Size.Height)
            {
                point.Y = canvas.PagePanel.Size.Height - maxOffsetPoint.Y - gripBuffer;
            }

            List<Control> pastedControlsPerFieldPaste = new List<Control>();
            ArrayList pastedControlsAllThisMethod = new ArrayList();

            double startTabIndex = Math.Ceiling(projectExplorer.currentPage.MaxTabIndex + 0.01);

            foreach (KeyValuePair<Field, Point> kvp in fieldClipboard)
            {
                newLocation.X = point.X + kvp.Value.X;
                newLocation.Y = point.Y + kvp.Value.Y;

                if (kvp.Key is RenderableField)
                {
                    canvas.UpdateHidePanel(string.Format("Paste {0}", kvp.Key.Name));
                    Field fieldClone = CloneField(kvp.Key);
                    pastedControlsPerFieldPaste = PasteField(fieldClone as RenderableField, this.Canvas.PagePanel, newLocation, isCutPasteNotCopy, startTabIndex, selectedFieldsTabIndexFloor);
                }
                
                foreach (Control control in pastedControlsPerFieldPaste)
                {
                    pastedControlsAllThisMethod.Add(control);
                }
            }

            SelectedControlsChanged(pastedControlsAllThisMethod);

            Dictionary<string, int> names = project.Metadata.DuplicateFieldNames();

            if (names.Count > 0)
            {
                string errorMessage = "The following fields were found as duplicates and deleted:" + Environment.NewLine;

                foreach(KeyValuePair<string, int> kvp in names)
                {
                    errorMessage = string.Format("{0}Name:{1} View:{2}{3}", errorMessage, kvp.Key, kvp.Value, Environment.NewLine);
                    project.Metadata.DeleteField(kvp.Key, kvp.Value);
                }
                
                MessageBox.Show(errorMessage);
            }

            PersistChildFieldNames();
            canvas.HideUpdateEnd();
        }

        private String GetFieldsNamesInRectangle(Panel panel, Rectangle selectedPanelArea)
        {
            String fieldNames = string.Empty;
            foreach (Control control in ((Panel)panel).Controls)
            {
                if (control is IFieldControl && ((IFieldControl)control).Field != null)
                {
                    if (selectedPanelArea.Contains(((Control)control).Location))
                    {
                        fieldNames = fieldNames + ((IFieldControl)control).Field.Name + ",";
                    }
                }
            }
            fieldNames = fieldNames.TrimEnd(new char[] { ',' });
            return fieldNames;
        }

        /// <summary>
        /// Loads the Enter module
        /// </summary>        
        public void LoadEnter()
        {
            if (OkayToClearClipboard())
            {
                View currentView = projectExplorer.SelectedPage.view;

                this.Project.CollectedData.SynchronizeDataTable(currentView);

                string projectFilePath = projectExplorer.SelectedPage.GetProject().FilePath;
                string viewName = currentView.Name;

                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                string commandText = System.IO.Path.GetDirectoryName(assembly.Location) + "\\Enter.exe";

                if (!string.IsNullOrEmpty(commandText))
                {
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.FileName = commandText;
                    proc.StartInfo.UseShellExecute = true;
                    proc.StartInfo.Arguments = string.Format("/project:\"{0}\" /view:\"{1}\"", projectFilePath, viewName);
                    proc.Start();
                }

                CloseAll();
            }
        }

        /// <summary>
        /// Closes all MakeView components
        /// </summary>
        public void CloseAll()
        {
            projectExplorer.Close();
            canvas.Close();
            this.Reset();
        }

        #endregion

        #region Protected Methods
        /// <summary>
        /// Raises BeginBusy event
        /// </summary>
        /// <param name="message">The message to be displayed</param>
        protected void BeginBusy(string message)
        {
            if (this.BeginBusyEvent != null)
            {
                BeginBusyEvent(message);
            }
        }

        /// <summary>
        /// Raises EndBusyEvent
        /// </summary>
        protected void EndBusy()
        {
            if (this.EndBusyEvent != null)
            {
                EndBusyEvent();
            }
        }
        #endregion Protected Methods

        #region Event Handlers

        public void projectExplorer_PageSelected(Page page)
        {
            canvas.HideUpdateStart(SharedStrings.OPENING_PAGE);
            selectedFieldControls.Clear();
            LoadPage(page);
            canvas.Text = page.GetView().Name + System.IO.Path.DirectorySeparatorChar + page.Name.Trim();
            canvas.HideUpdateEnd();
            OnViewFieldTabsChanged();
            if (canvas.OpenForViewingOnly)
            {
                canvas.EnablePagePanel(false);
            }
        }

        private void AutoSetTabOrder()
        {
            canvas.HideUpdateStart(SharedStrings.TAB_START_SETTING_ORDER);
            
            if (selectedFieldControls.Count == 0)
            {
                HandleNewSelection(new Rectangle(0,0,canvas.PagePanel.Width, canvas.PagePanel.Height));
            }

            if (selectedFieldControls.Count > 0)
            {
                foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
                {
                    bool isLabelField = kvp.Key.Field.FieldType == MetaFieldType.LabelTitle;
                    
                    if ((kvp.Key is PairedLabel == false && kvp.Key.Field != null) || isLabelField )
                    {
                        _fieldBefore = CloneField(kvp.Key.Field);
                        canvas.UpdateHidePanel(string.Format(SharedStrings.TAB_ORDER_SET_FIELD_TO_TAB,((RenderableField)kvp.Key.Field).Name, runningTabIndex));
                        ((RenderableField)kvp.Key.Field).TabIndex = runningTabIndex++;
                        PersistFieldChange(kvp.Key.Field, BackupFieldAction.Change);
                        PanelFieldUpdate(kvp.Key.Field);
                    }
                }
            }
            canvas.HideUpdateEnd();
            OnShowTabOrder();
        }

        public void OnShowTabOrder()
        {
            canvas.RemoveTabIndexFieldIndicators();
            canvas.RemoveTabIndexIndicators();
            canvas.TabIndexIndicators = new List<Control>();
            awaitingFirstTabClick = true;

            foreach (Control control in canvas.PagePanel.Controls)
            {
                if (control is IFieldControl)
                {
                    bool isInputField = control is PairedLabel == false && ((IFieldControl)control).Field.FieldType != MetaFieldType.Group;
                    bool isLabelField = ((IFieldControl)control).Field.FieldType == MetaFieldType.LabelTitle;

                    if (isInputField || isLabelField)
                    {
                        Label tabSquare = new Label();
                        tabSquare.BackColor = control.TabStop ? Color.Black : Color.Firebrick;
                        tabSquare.Padding = new Padding(2);
                        tabSquare.ForeColor = Color.White;
                        tabSquare.BorderStyle = BorderStyle.None;
                        tabSquare.Font = new Font(FontFamily.GenericSansSerif, 7, FontStyle.Bold);
                        tabSquare.Text = control.TabIndex.ToString();
                        tabSquare.Location = new Point(control.Location.X - 4, control.Location.Y - 4);
                        tabSquare.Size = TextRenderer.MeasureText("000", tabSquare.Font);
                        tabSquare.Size = new Size(tabSquare.Size.Width + tabSquare.Padding.Size.Width, tabSquare.Size.Height + tabSquare.Padding.Size.Height);
                        tabSquare.MouseClick += new MouseEventHandler(TabSquare_MouseClick);
                        ToolTip toolTip = new ToolTip();
                        toolTip.InitialDelay = 1;
                        toolTip.ReshowDelay = 1;
                        toolTip.AutoPopDelay = 30000;
                        toolTip.ShowAlways = true;
                        toolTip.UseFading = false;
                        String tip = SharedStrings.TAB_CLICK_TO_SET_INDEX + "\r\n" + SharedStrings.RIGHT_CLICK_FOR_OPTIONS;
                        toolTip.SetToolTip(tabSquare, tip);
                        tabSquare.Tag = control;
                        canvas.TabIndexIndicators.Add(tabSquare as Control);
                        tabSquare.BringToFront();
                    }
                }
            }
            canvas.PagePanel.Controls.AddRange(canvas.TabIndexIndicators.ToArray());
            foreach (Control ind in canvas.TabIndexIndicators)
            {
                ind.BringToFront();
            }
        }

        public void OnViewFieldTabsChanged()
        {
            canvas.RemoveTabIndexIndicators();
            canvas.RemoveTabIndexFieldIndicators();
             if (isShowFieldName)
                canvas.TabIndexFieldIndicators = new List<Control>();
             else
                canvas.TabIndexIndicators = new List<Control>();
            awaitingFirstTabClick = true;

            foreach (Control control in canvas.PagePanel.Controls)
            {
                if (control is IFieldControl)
                {
                    bool isInputField = control is PairedLabel == false;// ((IFieldControl)control).Field.FieldType != MetaFieldType.Group;
                    bool isLabelField = ((IFieldControl)control).Field.FieldType == MetaFieldType.LabelTitle;

                    if (isInputField || isLabelField)
                    {
                        Label tabSquare = new Label();
                        tabSquare.BackColor = control.TabStop ? Color.Black : Color.Firebrick;
                        tabSquare.Padding = new Padding(2);
                        tabSquare.ForeColor = Color.White;
                        tabSquare.BorderStyle = BorderStyle.None;
                        tabSquare.Font = new Font(FontFamily.GenericSansSerif, 7, FontStyle.Bold);
                      //  tabSquare.Text = control.TabIndex.ToString();
                        if (isShowFieldName && isShowTabOrder)
                            tabSquare.Text = control.TabIndex.ToString() + "  " + ((IFieldControl)control).Field.Name;
                        else if (isShowFieldName && !isShowTabOrder)
                            tabSquare.Text = ((IFieldControl)control).Field.Name;
                        else if (!isShowFieldName && isShowTabOrder)
                            tabSquare.Text = control.TabIndex.ToString();
                        else return;
                        tabSquare.Location = new Point(control.Location.X - 4, control.Location.Y - 4);
                       // tabSquare.Size = TextRenderer.MeasureText("000", tabSquare.Font);
                        tabSquare.Size = TextRenderer.MeasureText(tabSquare.Text, tabSquare.Font);
                        tabSquare.Size = new Size(tabSquare.Size.Width + tabSquare.Padding.Size.Width, tabSquare.Size.Height + tabSquare.Padding.Size.Height);

                        if (control is GroupBox)
                        {
                            tabSquare.Location = new Point(control.Location.X, control.Location.Y + tabSquare.Size.Height);
                        }
                        else
                        {
                            tabSquare.Location = new Point(control.Location.X, control.Location.Y);
                        }

                        ToolTip toolTip = new ToolTip();
                        toolTip.InitialDelay = 1;
                        toolTip.ReshowDelay = 1;
                        toolTip.AutoPopDelay = 30000;
                        toolTip.ShowAlways = true;
                        toolTip.UseFading = false;
                        String tip = SharedStrings.TAB_CLICK_TO_SET_INDEX + "\r\n" + SharedStrings.RIGHT_CLICK_FOR_OPTIONS;
                        toolTip.SetToolTip(tabSquare, tip);
                        tabSquare.Tag = control;
                        if (isShowFieldName)
                        canvas.TabIndexFieldIndicators.Add(tabSquare as Control);
                        else
                            canvas.TabIndexIndicators.Add(tabSquare as Control);
                        tabSquare.BringToFront();
                    }
                }
            }
            if (isShowFieldName)
            {
                canvas.PagePanel.Controls.AddRange(canvas.TabIndexFieldIndicators.ToArray());
                foreach (Control ind in canvas.TabIndexFieldIndicators)
                {
                    ind.BringToFront();
                }
            }
            else
            {
                canvas.PagePanel.Controls.AddRange(canvas.TabIndexIndicators.ToArray());
                foreach (Control ind in canvas.TabIndexIndicators)
                {
                    ind.BringToFront();
                }
            }
        }
        void TabSquare_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (awaitingFirstTabClick)
                {
                    runningTabIndex = int.Parse(((Label)sender).Text) + 1;
                    awaitingFirstTabClick = false;
                }
                else
                {                   
                    RenderableField field = (RenderableField)((IFieldControl)((Control)sender).Tag).Field;
                    ((Label)sender).Text = runningTabIndex.ToString();
                    _fieldBefore = CloneField(field);
                    field.TabIndex = runningTabIndex;
                    runningTabIndex += 1;
                    PersistFieldChange(field, BackupFieldAction.Change);
                    PanelFieldUpdate(field);
                    ((Control)sender).BringToFront();
                };
            }
            else
            {
                RenderableField field = (RenderableField)((IFieldControl)((Control)sender).Tag).Field;
                ContextMenu rightMouseClickMenu = new ContextMenu();

                MenuItem setAsFirstTab = new MenuItem(SharedStrings.TAB_SET_FIRST);
                setAsFirstTab.Click += new EventHandler(setAsFirstTab_Click);
                setAsFirstTab.Tag = sender;
                rightMouseClickMenu.MenuItems.Add(setAsFirstTab);

                string enabledisableText = field.HasTabStop ? SharedStrings.TAB_DISABLE : SharedStrings.TAB_ENABLE;

                MenuItem enableDisableTab = new MenuItem(enabledisableText);
                enableDisableTab.Click += new EventHandler(enableDisableTab_Click);
                enableDisableTab.Tag = sender;
                rightMouseClickMenu.MenuItems.Add(enableDisableTab);

                rightMouseClickMenu.Show(sender as Control, e.Location);
            } 
        }

        void enableDisableTab_Click(object sender, EventArgs e)
        {
            Label label = (Label)(((MenuItem)sender).Tag);
            IFieldControl control = (IFieldControl)((Control)label).Tag;
            RenderableField field = (RenderableField)(((IFieldControl)control).Field);

            _fieldBefore = CloneField(field);
            if ((field is LabelField))
            {
                if (field.HasTabStop)
                    field.HasTabStop = !field.HasTabStop;
            }
            else
            field.HasTabStop = !field.HasTabStop;
            PersistFieldChange(field, BackupFieldAction.Change);
            label.BackColor = field.HasTabStop ? Color.Black : Color.Firebrick;
        }

        void setAsFirstTab_Click(object sender, EventArgs e)
        {
            Label label = (Label)(((MenuItem)sender).Tag);
            IFieldControl control = (IFieldControl)((Control)label).Tag;
            RenderableField field = (RenderableField)(((IFieldControl)control).Field);

            _fieldBefore = CloneField(field);
            field.TabIndex = 1;
            ((Control)control).TabIndex = 1;
            PersistFieldChange(field, BackupFieldAction.Change);
            runningTabIndex = 1;
            label.Text = "1";
            awaitingFirstTabClick = false;
            runningTabIndex++;
        }

        public void OnStartNewTabOrder()
        {
            runningTabIndex = 1;
            AutoSetTabOrder();
        }

        public void OnContinueTabOrder()
        {
            AutoSetTabOrder();
        }

        private void canvas_GroupCreationRequested(Panel panel, System.Drawing.Rectangle outline)
        {
            GroupField group = (GroupField)projectExplorer.currentPage.CreateField(MetaFieldType.Group);
            Dialogs.FieldDefinitionDialogs.GroupFieldDefinition dialog = new Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs.GroupFieldDefinition(projectExplorer.MainForm, group);
            group.BackgroundColor = Color.White;
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                PositionGroupOnPanel(group, panel, outline);
                group.ChildFieldNames = GetFieldsNamesInRectangle(panel, outline);
                SaveField(group, panel, new Point(outline.X, outline.Y));
            }
            PersistChildFieldNames();
        }

        private void canvas_PageCheckCodeRequested(Panel panel)
        {
            Epi.Windows.MakeView.Forms.CheckCode checkCode = new Epi.Windows.MakeView.Forms.CheckCode(projectExplorer.currentPage, (MakeViewMainForm)projectExplorer.MainForm,ProjectExplorer.CurrentView);
            checkCode.ShowDialog();
        }

        private void canvas_TreeNodeDropped(TreeValueNode node, Panel panel, System.Drawing.Point location)
        {
            bool cancelTemplateDrop = false;
            
            try
            {
                if (node is FieldGroupNode)
                {
                    System.Drawing.Rectangle outline = new System.Drawing.Rectangle(location, new System.Drawing.Size());
                    canvas_GroupCreationRequested(panel, outline);
                }
                if (node is OpenFieldNode)
                {
                    CreateNewOpenField((MetaFieldType)node.Value, panel, location, true);
                    panel.BringToFront();
                }
                if (node is LinkedFieldNode)
                {
                    CreateNewLinkedModeField(location, panel, node.Value.ToString(), node.Text);
                }
                if (node is TemplateNode)
                {
                    Enums.TemplateLevel templateLevel = Template.GetTemplateLevel((TemplateNode)node);

                    if (templateLevel == Enums.TemplateLevel.Field)
                    {
                        foreach (Control control in panel.Controls)
                        {
                            if (control is IFieldControl && (control is TemplateFootprint == false || cancelTemplateDrop == true))
                            {
                                if (control.Bounds.IntersectsWith(canvas.SelectedFieldsFootprint.Bounds))
                                {
                                    DialogResult answer = MessageBox.Show(SharedStrings.TEMPLATE_FIELDS_OVERLAP_CONFIRM,
                                        SharedStrings.TEMPLATE_FIELDS_OVERLAP,
                                        MessageBoxButtons.YesNoCancel);

                                    if (answer != DialogResult.Yes) { cancelTemplateDrop = true; }
                                    break;
                                }
                            }
                        }

                        if (cancelTemplateDrop == false)
                        {
                            new Template(this).CreateFromTemplate((TemplateNode)node, location);
                            PersistChildFieldNames();
                        }
                    }
                    else if (templateLevel == Enums.TemplateLevel.Page || templateLevel == Enums.TemplateLevel.Form)
                    {
                        new Template(this).CreateFromTemplate((TemplateNode)node, new Point(0, 0));
                        PersistChildFieldNames();
                    }
                    else if (templateLevel == Enums.TemplateLevel.Project)
                    {
                        string path = node.FullPath;
                        ((MakeViewMainForm)canvas.MainForm).NewProjectFromTemplate(path);
                    }
                }

                canvas.PagePanel.Controls.Remove(canvas.SelectedFieldsFootprint);
                canvas.SelectedFieldsFootprint = null; 
                canvas.BringToFront();
                canvas.Focus();
                canvas.Refresh();
                panel.BringToFront();
                panel.Show();

                panel.Focus();
            }
            finally
            {
                EndBusy();
            }
        }

        private void canvas_FieldCreationRequested(object sender, MetaFieldType fieldType, Panel panel, System.Drawing.Point location)
        {
            CreateNewOpenField(fieldType, panel, location, false);
        }

        private void canvas_PromptMoved(object sender, FieldControlEventArgs e)
        {
            OnPromptMove(e.Control);
        }

        private void canvas_ControlMoved(object sender, FieldControlEventArgs e)
        {
            OnFieldMove(e.Control);
        }

        private void canvas_ControlResized(object sender, FieldControlEventArgs e)
        {
            OnFieldResize(e.Control);
        }

        /// <summary>
        /// Note: offsetPoint
        /// |---------------------------------------------|
        /// |        offsetPoint                          | 
        /// |     (o)----------------------A[    ]        | 
        /// |      |                                      | 
        /// |      |                                      | 
        /// |      |                                      | 
        /// |      |                    B[     ]          | 
        /// |      |                                      | 
        /// |      |                                      | 
        /// |      [  ]B                                  | 
        /// |                                             | 
        /// |             [     ]D                        | 
        /// |                                             | 
        /// |---------------------------------------------|
        /// </summary>
        /// <param name="controls"></param>
        public void SelectedControlsChanged(ArrayList controls, Boolean showTrackers = true)
        {
            this.selectedFieldControls.Clear();
            canvas.DisposeControlTrackers();

            Point offsetPoint = new Point();
            int px, py;

            // < find the offsetPoint() >
            foreach (Control control in controls)
            {
                if (control is IFieldControl)
                {
                    if ((((IFieldControl)control).Field is FieldWithSeparatePrompt))
                    {
                        px = (int)(((FieldWithSeparatePrompt)((IFieldControl)control).Field).PromptLeftPositionPercentage * canvas.PagePanel.Size.Width);
                        py = (int)(((FieldWithSeparatePrompt)((IFieldControl)control).Field).PromptTopPositionPercentage * canvas.PagePanel.Size.Height);

                        if (control.Location.X < px) px = control.Location.X;
                        if (control.Location.Y < py) py = control.Location.Y;
                    }
                    else
                    {
                        px = control.Location.X;
                        py = control.Location.Y;
                    }

                    if (offsetPoint.IsEmpty)
                    {
                        offsetPoint.X = px;
                        offsetPoint.Y = py;
                    }
                    if (px < offsetPoint.X) offsetPoint.X = px;
                    if (py < offsetPoint.Y) offsetPoint.Y = py;
                }
            }

            Point promptOffset = new Point();
            Point controlOffset = new Point();
            Boolean alreadyAdded = false;

            // < add IFieldControls and offsets to the selectedFieldControls SortedDictionary object >
            foreach (Control control in controls)
            {
                if (control is IFieldControl)
                {
                    // < check to see if the control(s) have already been added to the collection >
                    foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
                    {
                        string selectedFieldName = ((IFieldControl)control).Field.Name;
                        string panelFieldControl = ((IFieldControl)kvp.Key).Field.Name;

                        if (selectedFieldName == panelFieldControl)
                        {
                            alreadyAdded = true;
                            break;
                        }
                    }

                    if (alreadyAdded == false)
                    {
                        if ((((IFieldControl)control).Field is FieldWithSeparatePrompt))
                        {
                            if ((IFieldControl)control is DragableLabel)
                            {
                                promptOffset.X = control.Left - offsetPoint.X;
                                promptOffset.Y = control.Top - offsetPoint.Y;
                                controlOffset.X = ((PairedLabel)control).LabelFor.Left - offsetPoint.X;
                                controlOffset.Y = ((PairedLabel)control).LabelFor.Top - offsetPoint.Y;

                                selectedFieldControls.Add((IFieldControl)control, promptOffset);
                                selectedFieldControls.Add(((IFieldControl)((PairedLabel)control).LabelFor), controlOffset);
                            }
                            else
                            {
                                Control promptControl = GetPromptControl(((IFieldControl)control).Field.Name);
                                promptOffset.X = promptControl.Left - offsetPoint.X;
                                promptOffset.Y = promptControl.Top - offsetPoint.Y;
                                controlOffset.X = control.Left - offsetPoint.X;
                                controlOffset.Y = control.Top - offsetPoint.Y;

                                selectedFieldControls.Add((IFieldControl)promptControl, promptOffset);
                                selectedFieldControls.Add((IFieldControl)control, controlOffset);
                            }
                        }
                        else
                        {
                            controlOffset.X = control.Left - offsetPoint.X;
                            controlOffset.Y = control.Top - offsetPoint.Y;
                            selectedFieldControls.Add((IFieldControl)control, controlOffset);
                        }
                    }
                    alreadyAdded = false;
                }
            }

            InitialSelectedControlLocations = new Dictionary<IFieldControl, Point>();
            foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
            {
                initialSelectedControlLocations.Add(kvp.Key, new Point(kvp.Value.X + offsetPoint.X, kvp.Value.Y + offsetPoint.Y));
            }

            if (showTrackers)
            { 
                canvas.RenderControlTrackers(selectedFieldControls);
            }

            IsSelectedControlEmpty = this.selectedFieldControls.Count == 0 ? true : false;
        }

        public void HandleNewSelection(System.Drawing.Rectangle selectedPanelArea)
        {
            if (selectedPanelArea.IsEmpty)
            {
                selectedFieldControls.Clear();
                canvas.DisposeControlTrackers();
                return;
            }

            ArrayList controlArrayList = new ArrayList();

            foreach (Control control in canvas.PagePanel.Controls)
            {
                if (control is IFieldControl)
                {
                    if (selectedPanelArea.Contains(control.Location.X, control.Location.Y))
                    {
                        controlArrayList.Add(control);
                    }
                }
            }
            SelectedControlsChanged(controlArrayList);
        }

        public void mnuAutoSize_Click(object sender, EventArgs e)
        {
            DefaultAutoSize(rightClickedControl);
        }

        void DefaultAutoSize(IFieldControl fieldControl)
        {
            Field field = fieldControl.Field;
            _fieldBefore = CloneField(field);
            canvas.DisposeControlTrackers();
            AutoSizeLabelField(field, ((Control)rightClickedControl).Location);
            PersistFieldChange(field, BackupFieldAction.Change);
            PanelFieldUpdate(field);
        }

        public void mnuPromptAlign_Click(object sender, EventArgs e)
        {
            DefaultPromptAlign();
        }

        void DefaultPromptAlign()
        {
            _fieldBefore = CloneField(rightClickedControl.Field);
            canvas.DisposeControlTrackers();
            AutoAlignFieldGivenPromptLocation(rightClickedControl.Field, ((Control)rightClickedControl).Location);
            PersistFieldChange(rightClickedControl.Field, BackupFieldAction.Change);
            PanelFieldUpdate(rightClickedControl.Field);
        }

        public void mnuDelete_Click(object sender, EventArgs e)
        {
            Panel panel = (Panel)((Control)rightClickedControl).Parent;
            _fieldBefore = CloneField(rightClickedControl.Field);
            PersistFieldChange(rightClickedControl.Field, BackupFieldAction.Delete);
        }

        public void mnuCopy_Click(object sender, EventArgs e)
        {
            if (SelectedFieldControls.Count == 0) 
            {
                OnClipboardCopy(((IFieldControl)rightClickedControl));
            }
            else
            {
                OnClipboardCopySelection();
            }
        }

        public void mnuCut_Click(object sender, EventArgs e)
        {
            if (SelectedFieldControls.Count == 0)
            {
                OnClipboardCut(((IFieldControl)rightClickedControl));
            }
            else
            {
                OnClipboardCutSelection();
            }
            
        }

        public void mnuGoTo_Click(object sender, EventArgs e)
        {
            if (((RelatedViewField)rightClickedControl.Field).ChildView != null)
            {
                View childView = ((RelatedViewField)rightClickedControl.Field).ChildView;
                Page pageOne = childView.Pages[0];
                projectExplorer.SelectPage(pageOne);
            }
        }

        public void mnuProperties_Click(object sender, EventArgs e)
        {
            string nameBeforeEdit = rightClickedControl.Field.Name;
            bool isAdvancedUser = false;

            if (sender is ToolStripDropDownItem && ((ToolStripDropDownItem)sender).Tag is bool)
            {
                isAdvancedUser = (bool)((ToolStripDropDownItem)sender).Tag;
            }

            RenderableField field = ((RenderableField)(rightClickedControl.Field));

            string promptTextBeforeEdit = field.PromptText;
            Font promptFontBeforeEdit = field.PromptFont;
            Font controlFontBeforeEdit = field.ControlFont;

            FieldDefinitionDialogFactory dialogFactory = FieldDefinitionDialogFactory.GetInstance(this.serviceProvider);
            Dialogs.FieldDefinitionDialogs.FieldDefinition dialog = dialogFactory.GetFieldDefinitionDialog(rightClickedControl.Field);
            
            bool hasCollectedDataColumn = (Project.CollectedData.TableExists(field.GetView().TableName));
            if (isAdvancedUser || (hasCollectedDataColumn == false) || (hasCollectedDataColumn && _newFieldIds.Contains(field.Id)))
            {
                ((Dialogs.FieldDefinitionDialogs.GenericFieldDefinition)dialog).FieldNameEnabled = true;
            }
            else
            {
                ((Dialogs.FieldDefinitionDialogs.GenericFieldDefinition)dialog).FieldNameEnabled = false;
            }
            
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                _fieldBefore = CloneField(rightClickedControl.Field);
                Panel panel = (Panel)((Control)rightClickedControl).Parent;

                if (!((dialog.Field is MirrorField) || (dialog.Field is LabelField) || (dialog.Field is GridField)))
                {
                    dialog.Field.HasTabStop = true;
                }

                if (isAdvancedUser && (nameBeforeEdit != dialog.Field.Name))
                {
                    if (project.CollectedData.TableExists(field.Page.TableName))
                    {
                        if (project.CollectedData.ColumnExists(field.Page.TableName, nameBeforeEdit))
                        {
                            DialogResult response = MessageBox.Show
                            (
                                "================ " + SharedStrings.IMPORT_PREFIX_WARNING + " ================\n\r" +
                                "\n\r" + SharedStrings.CHANGE_NAME_CONSEQUENCE +
                                SharedStrings.CHANGE_NAME_CONFIRM +
                                "\n\r" +
                                SharedStrings.CHANGE_NAME_DECLINE,
                                SharedStrings.IMPORT_PREFIX_WARNING,
                                MessageBoxButtons.YesNo
                            );

                            if (response == DialogResult.No)
                            {
                                dialog.Field.Name = nameBeforeEdit;
                            }
                        }
                    }
                }
                
                if (nameBeforeEdit != dialog.Field.Name)
                {
                    project.CollectedData.SuggestRenameFieldInCollectedData(dialog.Field, nameBeforeEdit);
                }

                // to make sure changes to the pattern when changing proprerties auto-resize the control's width
                // Only for those have a pattern selected.
                if (dialog.Field is NumberField )
                {
                    NumberField numberField = ((NumberField)dialog.Field);
                   
                    if (numberField.Pattern != "None" && numberField.Pattern != "")
                    {
                        Size size = TextRenderer.MeasureText(numberField.Pattern, numberField.ControlFont);
                        numberField.ControlWidthPercentage = (float)size.Width / (float)canvas.PagePanel.Size.Width;
                    }
                }

                bool changePersisted = PersistFieldChange(dialog.Field, BackupFieldAction.Change);

                if (changePersisted && (nameBeforeEdit != dialog.Field.Name))
                {
                    ((MakeViewMainForm)this.Canvas.MainForm).EpiInterpreter.Context.UndefineVariable(nameBeforeEdit);
                }

                if (promptFontBeforeEdit != dialog.Field.PromptFont)
                {
                    if (dialog.Field is LabelField)
                    {
                        DefaultAutoSize(rightClickedControl);
                    }
                    else
                    {
                        DefaultPromptAlign();
                    }
                }

                PanelFieldUpdate(dialog.Field);

                if (dialog.Field is RelatedViewField)
                {
                    RelatedViewField relatedViewField = (RelatedViewField)dialog.Field;                 
                    View childView = relatedViewField.GetProject().GetViewById(relatedViewField.RelatedViewID);
                    childView.IsRelatedView = true;
                    childView.ParentView = this.ProjectExplorer.SelectedPage.GetView(); // added to fix a problem where the parentview property is null after creating a relate button where existing is true.
                    ProjectExplorer.SetView(childView);                                      
                }
            }
        }

        public void canvas_ChangeToFieldType_Click(object sender, EventArgs e)
        {
            Panel panel = canvas.PagePanel;

            MetaFieldType newFieldType = ((MetaFieldType)(((System.Windows.Forms.ToolStripItem)(((System.Windows.Forms.ToolStripDropDownItem)(sender)))).Tag));

            Field newField = (RenderableField)projectExplorer.currentPage.CreateField(newFieldType);
            _fieldBefore = CloneField(rightClickedControl.Field);

            // <assign common properties>
            newField.Name = rightClickedControl.Field.Name;
            newField.UniqueId = rightClickedControl.Field.UniqueId;
            Boolean isReadOnly = false;
            Boolean isRequired = false;
            
            if (rightClickedControl.Field is InputFieldWithSeparatePrompt)
            {
                isReadOnly = ((InputFieldWithSeparatePrompt)rightClickedControl.Field).IsReadOnly;
                isRequired = ((InputFieldWithSeparatePrompt)rightClickedControl.Field).IsRequired;
            }
            if (rightClickedControl.Field is InputFieldWithoutSeparatePrompt)
            {
                isReadOnly = ((InputFieldWithoutSeparatePrompt)rightClickedControl.Field).IsReadOnly;
                isRequired = ((InputFieldWithoutSeparatePrompt)rightClickedControl.Field).IsRequired;
            }
            
            if (newField is InputFieldWithSeparatePrompt)
            {
                ((InputFieldWithSeparatePrompt)newField).IsReadOnly = isReadOnly;
                ((InputFieldWithSeparatePrompt)newField).IsRequired = isRequired;
            }
            if (newField is InputFieldWithoutSeparatePrompt)
            {
                ((InputFieldWithoutSeparatePrompt)newField).IsReadOnly = isReadOnly;
                ((InputFieldWithoutSeparatePrompt)newField).IsRequired = isRequired;
            }

            ((RenderableField)newField).PromptText = ((RenderableField)rightClickedControl.Field).PromptText;
            ((RenderableField)newField).TabIndex = ((RenderableField)rightClickedControl.Field).TabIndex;
            ((RenderableField)newField).PromptFont = ((RenderableField)rightClickedControl.Field).PromptFont;
            ((RenderableField)newField).Page = ((RenderableField)rightClickedControl.Field).Page;
            ((RenderableField)newField).IsControlResizable = ((RenderableField)rightClickedControl.Field).IsControlResizable;
            ((RenderableField)newField).HasTabStop = ((RenderableField)rightClickedControl.Field).HasTabStop;
            ((RenderableField)newField).ControlFont = ((RenderableField)rightClickedControl.Field).ControlFont;
            if(newField is Epi.Fields.CommandButtonField)
                ((RenderableField)newField).ControlFont = ((RenderableField)rightClickedControl.Field).PromptFont;

            FieldDefinitionDialogFactory dialogFactory = FieldDefinitionDialogFactory.GetInstance(this.serviceProvider);
            Dialogs.FieldDefinitionDialogs.FieldDefinition dialog = dialogFactory.GetFieldDefinitionDialog(newField);
            DialogResult result = dialog.ShowDialog();

            newField.Id = rightClickedControl.Field.Id;
            
            ((RenderableField)newField).ControlLeftPositionPercentage = ((RenderableField)rightClickedControl.Field).ControlLeftPositionPercentage;
            ((RenderableField)newField).ControlTopPositionPercentage = ((RenderableField)rightClickedControl.Field).ControlTopPositionPercentage;

            if (newField is FieldWithSeparatePrompt)
            {
                if (rightClickedControl.Field is FieldWithSeparatePrompt)
                {
                    ((RenderableField)newField).ControlLeftPositionPercentage = ((RenderableField)rightClickedControl.Field).ControlLeftPositionPercentage;
                    ((RenderableField)newField).ControlTopPositionPercentage = ((RenderableField)rightClickedControl.Field).ControlTopPositionPercentage;
                    ((FieldWithSeparatePrompt)newField).PromptLeftPositionPercentage = ((FieldWithSeparatePrompt)rightClickedControl.Field).PromptLeftPositionPercentage;
                    ((FieldWithSeparatePrompt)newField).PromptTopPositionPercentage = ((FieldWithSeparatePrompt)rightClickedControl.Field).PromptTopPositionPercentage;
                }
                else
                {
                    ((FieldWithSeparatePrompt)newField).PromptLeftPositionPercentage = ((RenderableField)rightClickedControl.Field).ControlLeftPositionPercentage;
                    ((FieldWithSeparatePrompt)newField).PromptTopPositionPercentage = ((RenderableField)rightClickedControl.Field).ControlTopPositionPercentage;
                    
                    Point location = new Point
                        (
                            (int)(((FieldWithSeparatePrompt)newField).PromptLeftPositionPercentage * canvas.PagePanel.Size.Width),
                            (int)(((FieldWithSeparatePrompt)newField).PromptTopPositionPercentage * canvas.PagePanel.Size.Height)
                        );
                    
                    AutoAlignFieldGivenPromptLocation(newField, location);
                }
            }
            else
            {
                ((RenderableField)newField).ControlLeftPositionPercentage = ((RenderableField)rightClickedControl.Field).ControlLeftPositionPercentage;
                ((RenderableField)newField).ControlTopPositionPercentage = ((RenderableField)rightClickedControl.Field).ControlTopPositionPercentage;
            }

            if (result == DialogResult.OK)
            {
                PersistFieldChange(newField, BackupFieldAction.Change);
                PanelFieldUpdate(newField);

                if (newField.FieldType == MetaFieldType.Relate)
                {
                    RelatedViewField RVF = (RelatedViewField)newField;
                    View V = RVF.GetProject().GetViewById(RVF.RelatedViewID);
                    V.ParentView = this.ProjectExplorer.SelectedPage.GetView();

                    ProjectExplorer.AddViewWithOutChangingSelection(V);
                }
            }
        }

        public void mnuCheckCode_Click(object sender, EventArgs e)
        {
            Epi.Windows.MakeView.Forms.CheckCode checkCode = new Epi.Windows.MakeView.Forms.CheckCode(rightClickedControl.Field, (MakeViewMainForm)projectExplorer.MainForm, ProjectExplorer.CurrentView);
            checkCode.ShowDialog();
        }

        private void control_MouseDown(object sender, MouseEventArgs e)
        {
            // If [ctrl] key down when clicking a control, add it to the selected controls collection
            // else mouse down on a selected control, replace the controls with a footprint
            
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                ArrayList controlArrayList = new ArrayList();

                IFieldControl[] selControls = new IFieldControl[selectedFieldControls.Count];
                selectedFieldControls.Keys.CopyTo(selControls, 0);
                
                bool containsKey = false;

                for (int i = 0; i < selControls.Length; i++)
                {
                    if (selControls[i] == ((IFieldControl)sender))
                    {
                        containsKey = true;
                        break;
                    }
                }

                if (containsKey)
                {
                    for (int i = 0; i < selControls.Length; i++)
                    {
                        if (((IFieldControl)selControls[i]).Field.Id != ((IFieldControl)sender).Field.Id)
                        {
                            controlArrayList.Add(selControls[i]);
                        }
                    }
                }
                else
                {
                    controlArrayList.Add((IFieldControl)sender);
                    for (int i = 0; i < selControls.Length; i++)
                    {
                        controlArrayList.Add(selControls[i]);
                    }
                }
              
                SelectedControlsChanged(controlArrayList);
            }
            else
            {
                canvas.DisposeControlTrackers();
                canvas.RemoveTabIndexIndicators();

                bool isClickOnSelected = false;
                int minOver = int.MaxValue, minDown = int.MaxValue;

                foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
                {
                    if (((IFieldControl)kvp.Key).Field.UniqueId == ((IFieldControl)sender).Field.UniqueId)
                    {
                        isClickOnSelected = true;
                    }

                    int dim = ((Control)kvp.Key).Location.X;
                    if (dim <= minOver) minOver = dim;

                    dim = ((Control)kvp.Key).Location.Y;
                    if (dim <= minDown) minDown = dim;
                }

                if (isClickOnSelected == false)
                {
                    selectedFieldControls.Clear();
                }

                if (sender is DragableGroupBox)
                {
                    canvas.Focus();
                }
            }
        }

        private void control_MouseEnter(object sender, EventArgs e)
        {
            string status = string.Empty;
            
            ((Control)sender).Cursor = Cursors.SizeAll;

            if (canvas.TabIndexIndicators == null || canvas.TabIndexIndicators.Count == 0)
            {
                if (selectedFieldControls.Count > 0)
                {
                    return;
                }

                selectedFieldControls.Clear();
                canvas.DisposeControlTrackers();

                if (((Control)sender) is PairedLabel && ((PairedLabel)sender).LabelFor != null)
                {
                    canvas.RenderControlTracker((PairedLabel)sender);

                    Field field = ((IFieldControl)(((Epi.Windows.Controls.PairedLabel)(sender)).LabelFor)).Field;
                    
                    status = field.Name;

                    if (field is DDLFieldOfCodes)
                    {
                        status += string.Format(" :: {0} :: {1}", ((DDLFieldOfCodes)field).CodeTable, ((DDLFieldOfCodes)field).RelateConditionString);
                    }
                }
                else if (sender is DragableLabel && (((Control)sender) is PairedLabel == false))
                {
                    canvas.RenderControlTracker((Control)sender, true);
                }
                else
                {
                    canvas.RenderControlTracker((Control)sender, true);
                }
            }

            if (status.Length > 0)
            {
                ((MakeViewMainForm)canvas.MainForm).UpdateStatus(status, false);
            }
        }

        #endregion

        #region Private Methods

        private void CreateNewLinkedModeField(System.Drawing.Point location, Panel panel, string setValueName, string promptText)
        {
            PHINVSProvider phin = PHINVSProvider.Instance;
            Page page = projectExplorer.currentPage;
            IMetadataProvider metadata = page.GetMetadata();
            string codeTableName = CleanCodeTableName(setValueName);
            string fieldName = page.GetView().ComposeFieldNameFromPromptText(promptText);

            if (fieldName.Length > 64)
            {
                fieldName = fieldName.Substring(0, 64);
            }

            string[] columnNames = { fieldName };            
            metadata.CreateCodeTable(codeTableName, columnNames[0]);

            if (metadata.TableExists(codeTableName))
            {
                // Subscribe to progress events
                ProgressReportBeginEventHandler beginHandler = new ProgressReportBeginEventHandler(this.RaiseProgressReportBeginEvent);
                ProgressReportUpdateEventHandler updateHandler = new ProgressReportUpdateEventHandler(this.RaiseProgressReportUpdateEvent);
                SimpleEventHandler endHandler = new SimpleEventHandler(this.RaiseProgressReportEndEvent);
                metadata.ProgressReportBeginEvent += beginHandler;
                metadata.ProgressReportUpdateEvent += updateHandler;
                metadata.ProgressReportEndEvent += endHandler;

                // Copy code data
                DataTable concepts = phin.GetConcepts(setValueName, fieldName);
                concepts.Columns[0].MaxLength = 255;
                if (concepts.Columns.Count >= 2)
                {
                    foreach (DataRow row in concepts.Rows)
                    {
                        string value = row[0].ToString() + "-" + row[1].ToString();
                        row[0] = value;
                    }
                }
                metadata.InsertCodeTableData(concepts, codeTableName, columnNames);
                
                metadata.ProgressReportBeginEvent -= beginHandler;
                metadata.ProgressReportUpdateEvent -= updateHandler;
                metadata.ProgressReportEndEvent -= endHandler;
            }

            DDLFieldOfCommentLegal field = (DDLFieldOfCommentLegal)page.CreateField(MetaFieldType.CommentLegal);            
            field.PromptText = promptText;
            field.Name = fieldName; // page.GetView().ComposeFieldNameFromPromptText(promptText);
            AutoAlignFieldGivenPromptLocation(field, location);
            field.SourceTableName = CleanCodeTableName(setValueName);
            field.TextColumnName = fieldName;            
            field.IsExclusiveTable = false;
            field.ShouldSort = false;
            field.HasTabStop = true;
            field.TabIndex = this.runningTabIndex + 1;
            _fieldBefore = null;
            PersistFieldChange(field, BackupFieldAction.Change);
            PanelFieldUpdate(field);
        }

        private string CleanCodeTableName(string fieldName)
        {
            //prefix the name with "code" - later will need to revisit and allow this to be variable
            fieldName = "code" + fieldName;

            //fieldName needs to be in all lowercase and remove hyphens for MySQL
            fieldName = fieldName.Replace("-", string.Empty);
            fieldName = fieldName.ToLowerInvariant();
            return fieldName;
        }

        public void CreateFieldFootprintControl(Size size)
        {
            ControlTracker footprint = new ControlTracker();
            footprint.Size = size;
            footprint.Location = new Point(0, 0);
            canvas.PagePanel.Controls.Add(footprint);
        }

        public Field CreateNewOpenField(MetaFieldType fieldType, Panel panel, System.Drawing.Point location, bool isFromDragDrop)
        {
            Field field = (RenderableField)projectExplorer.SelectedPage.CreateField(fieldType);
            ((RenderableField)field).TabIndex = 1;
            FieldDefinitionDialogFactory dialogFactory = FieldDefinitionDialogFactory.GetInstance(this.serviceProvider);
            Dialogs.FieldDefinitionDialogs.FieldDefinition dialog = dialogFactory.GetFieldDefinitionDialog(field);
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                SaveField(field, panel, location);

                if (field.FieldType == MetaFieldType.Relate)
                {
                    RelatedViewField relatedViewField = (RelatedViewField)field;                    
                    View childView = relatedViewField.GetProject().GetViewById(relatedViewField.RelatedViewID);
                    childView.IsRelatedView = true;
                    
                    /* 
                    * the code in this block is supposed to add a new view to the tree without changing the 
                    * currently-selected view. We don't want to do this if we used an existing view. Fixes a 
                    * crash that occurred when creating a related view button and specifying an existing view.
                    * EK 1/20/2010
                    */

                    if (!((Epi.Windows.MakeView.Dialogs.FieldDefinitionDialogs.RelateFieldDefinition)dialog).UseExistingView)
                    {
                        childView.ParentView = this.ProjectExplorer.SelectedPage.GetView();
                        ProjectExplorer.AddViewWithOutChangingSelection(childView);
                    }
                    else
                    {
                        childView.ParentView = this.ProjectExplorer.SelectedPage.GetView(); // added to fix a problem where the parentview property is null after creating a relate button where existing is true. EK 3/30/2012
                        ProjectExplorer.SetView(childView);
                    }
                }
                else if (field.FieldType == MetaFieldType.Codes)
                {
                    string associatedFields = ((DDLFieldOfCodes)(field)).AssociatedFieldInformation;
                    string [] fieldNames = associatedFields.Split(',');
                    foreach (string fieldName in fieldNames)
                    {
                        if (fieldName == string.Empty) break;
                        
                        string name = fieldName.Substring(0, fieldName.IndexOf(":"));
                        int fieldId = int.Parse(fieldName.Substring(fieldName.IndexOf(":") + 1));
                        Page currentPage = ProjectExplorer.currentPage;
                        Field associatedField = null;

                        if (currentPage.Fields.Contains(name) == true)
                        {
                            associatedField = ((Field)currentPage.Fields[name]);
                        }
                        else
                        {
                            DataTable fieldData = project.Metadata.GetFieldsOnPageAsDataTable(currentPage.Id);
                            DataRow[] rows = fieldData.Select(string.Format("FieldId = {0}", fieldId));
                            name = rows[0]["Name"].ToString();
                            associatedField = ((Field)currentPage.Fields[name]);
                        }

                        if (associatedField is IInputField && (false == (associatedField is TableBasedDropDownField)))
                        { 
                            ((IInputField)associatedField).IsReadOnly = true;
                            associatedField.SaveToDb();
                        }
                        PanelFieldUpdate(associatedField);
                    }
                }
                else if (field.FieldType == MetaFieldType.List)
                {
                    string associatedFields = ((DDListField)(field)).AssociatedFieldInformation;
                    string[] fieldNames = associatedFields.Split(',');
                    foreach (string fieldName in fieldNames)
                    {
                        string name = fieldName.Substring(0, fieldName.IndexOf(":"));
                        Field associatedField = ((Field)this.ProjectExplorer.currentPage.Fields[name]);
                        ((IInputField)associatedField).IsReadOnly = true;
                        associatedField.SaveToDb();
                        PanelFieldUpdate(associatedField);
                    }
                }
            }

            return field;
        }

        public void PasteFromTemplate(RenderableField selectedField)
        {
            selectedField.Name = CreateUniqueFieldName(selectedField.Name);
            _fieldBefore = null;
            PersistFieldChange(selectedField, BackupFieldAction.New);
            List<Control> controls = ControlFactory.Instance.GetFieldControls(selectedField, canvas.PagePanel.Size);
            AddControlsToPanel(controls);
            foreach (Control control in controls)
            {
                SelectedFieldControls.Add((IFieldControl)control, new Point());
            }
        }

        private List<Control> PasteField(RenderableField selectedField, Panel panel, Point inputControlLocation, bool isCutNotCopy, double startingTabIndex, int selectedFieldsTabIndexFloor = 0)
        {
            List<Control> controls = new List<Control>();
            RenderableField newField = selectedField;
            newField.Id = 0;

            Page page = projectExplorer.currentPage;

            if (page != ((RenderableField)newField).Page)
            {
                ((RenderableField)newField).Page = projectExplorer.currentPage;
            }

            newField.TabIndex = newField.TabIndex + startingTabIndex - selectedFieldsTabIndexFloor;

            newField.Name = CreateUniqueFieldName(newField.Name);
            newField.UniqueId = Guid.NewGuid();

            double inputControlLocationLeftPercentage = AsPercentage(inputControlLocation.X, canvas.PagePanel.Size.Width);
            double inputControlLocationTopPercentage = AsPercentage(inputControlLocation.Y, canvas.PagePanel.Size.Height);

            if (newField is FieldWithSeparatePrompt)
            {
                PointF deltaInputPrompt = new PointF
                (
                    (float)(newField.ControlLeftPositionPercentage - ((FieldWithSeparatePrompt)newField).PromptLeftPositionPercentage),
                    (float)(newField.ControlTopPositionPercentage - ((FieldWithSeparatePrompt)newField).PromptTopPositionPercentage)
                );

                ((FieldWithSeparatePrompt)newField).PromptLeftPositionPercentage = inputControlLocationLeftPercentage - deltaInputPrompt.X;
                ((FieldWithSeparatePrompt)newField).PromptTopPositionPercentage = inputControlLocationTopPercentage - deltaInputPrompt.Y;
            }

            newField.ControlLeftPositionPercentage = inputControlLocationLeftPercentage;
            newField.ControlTopPositionPercentage = inputControlLocationTopPercentage;

            newField.View = projectExplorer.CurrentView;

            if (newField is InputFieldWithoutSeparatePrompt)
            {
                ((InputFieldWithoutSeparatePrompt)newField).TableName = projectExplorer.CurrentView.TableName;
            }
            else if (newField is InputFieldWithSeparatePrompt)
            {
                ((InputFieldWithSeparatePrompt)newField).TableName = projectExplorer.CurrentView.TableName;
            }

            _fieldBefore = null;
            
            if(PersistFieldChange(newField, BackupFieldAction.Paste))
            {
                controls = ControlFactory.Instance.GetFieldControls(newField, canvas.PagePanel.Size);
                AddControlsToPanel(controls);
            }
            
            SetZeeOrderOfGroups();
            SetBackColorForGroupCheckBoxes(true);
            return controls;
        }

        protected string CreateUniqueFieldName(string name)
        {
            string newName;
            int i = 1;
            int maxFieldNameLength = 64;

            View currentView = this.ProjectExplorer.SelectedPage.GetView();
            
            if (currentView.Fields.Names.Contains(name) == false)
            {
                return name;
            }

            List<string> names = new List<string>();

            foreach (Field field in currentView.Fields)
            {
                names.Add(field.Name);
            }
            
            do
            {
                string postfix = i.ToString();
                int postfixLength = postfix.Length;
                if (name.Length + postfixLength > maxFieldNameLength)
                {
                    name = name.Substring(0, maxFieldNameLength - postfixLength);
                }
                newName = string.Format("{0}{1:D}", name, i);
                i++;
            }
            while (names.Contains(newName));

            return newName;
        }

        /// <summary>
        /// Save Field
        /// </summary>
        /// <param name="field">field</param>
        /// <param name="panel">panel</param>
        /// <param name="location">location</param>
        private void SaveField(Field field, Panel panel, System.Drawing.Point location)
        {
            View currentView = this.ProjectExplorer.SelectedPage.GetView();

            if (IsTableFull) return;

            if (!string.IsNullOrEmpty(currentView.TableName))
            {
                if (field is IInputField)
                {
                    ((IInputField)field).TableName = currentView.TableName;
                }
            }

            if (field is NumberField && ((NumberField)field).Pattern.Contains("#"))
            {                
                NumberField numberField = ((NumberField)field);
                Size size = TextRenderer.MeasureText(numberField.Pattern, numberField.ControlFont);
                numberField.ControlWidthPercentage = (float)size.Width / (float)canvas.PagePanel.Size.Width;
            }

            if (field is LabelField)
            {
                location = AutoSizeLabelField(field, location);
            }

            if (field is TextField && ((TextField)field).MaxLength >= 1)
            {
                TextField textField = ((TextField)field);
                string length = string.Empty;
                for (int i = 0; i < textField.MaxLength; i++)
                {
                    length = length + "#";
                }

                Size size = TextRenderer.MeasureText(length, textField.ControlFont);
                float proposedWidth = (float)size.Width / (float)canvas.PagePanel.Size.Width;
                textField.ControlWidthPercentage = proposedWidth > 0.95 ? 0.95 : proposedWidth;
            }
           
            if (!((field is MirrorField) || (field is LabelField)))
            {
                ((RenderableField)field).HasTabStop = true;
            }

            if (field.FieldType == MetaFieldType.Relate)
            {
                location = AutoSizeRelate(field, location);
                
                Epi.Fields.RelatedViewField relatedViewField = (Epi.Fields.RelatedViewField)field;

                foreach (View childViewCandidate in currentView.Project.Views)
                {
                    if (relatedViewField.RelatedViewID == childViewCandidate.Id)
                    {
                        if (childViewCandidate.ForeignKeyFieldExists == false)
                        {
                            ForeignKeyField foreignKeyField = null;

                            foreignKeyField = new ForeignKeyField(childViewCandidate);
                            foreignKeyField.SaveToDb();
                            childViewCandidate.MustRefreshFieldCollection = true;
                            childViewCandidate.IsRelatedView = true;
                            childViewCandidate.GetMetadata().UpdateView(childViewCandidate);
                        }
                        break;
                    }
                }
            }
            if (field is IDataField)
            {
                this.projectExplorer.EpiInterpreter.Context.DefineVariable((IDataField)field);
            }

            AutoAlignFieldGivenPromptLocation(field, location);
            AssignTabIndex(field, panel, projectExplorer.currentPage);
            _fieldBefore = null;
            PersistFieldChange(field, BackupFieldAction.New);
            PanelFieldUpdate(field);           
        }

        private System.Drawing.Point AutoSizeLabelField(Field field, System.Drawing.Point location)
        {
            LabelField labelField = (LabelField)field;
            string text = labelField.PromptText;

            int maxWidth = Math.Abs(canvas.PagePanel.Size.Width - location.X - (canvas.PagePanel.Size.Width/20));
            Size proposedSize = new Size(maxWidth, int.MaxValue);

            Bitmap tempImage = new Bitmap(1, 1);
            Graphics graphics = Graphics.FromImage(tempImage);
            SizeF size = graphics.MeasureString(text, labelField.ControlFont, proposedSize.Width);

            if (text == string.Empty)
            {
                size.Width = 24;
            }

            labelField.ControlWidthPercentage = ((float)size.Width + 1.7724) / (float)canvas.PagePanel.Size.Width;
            labelField.ControlHeightPercentage = (float)size.Height / (float)canvas.PagePanel.Size.Height;
            return location;
        }

        private System.Drawing.Point AutoSizeRelate(Field field, System.Drawing.Point location)
        {
            RenderableField renderableField = (RenderableField)field;
            string text = renderableField.PromptText;

            int maxWidth = Math.Abs(canvas.PagePanel.Size.Width - location.X - (canvas.PagePanel.Size.Width / 20));
            Size proposedSize = new Size(maxWidth, int.MaxValue);

            Bitmap tempImage = new Bitmap(1, 1);
            Graphics graphics = Graphics.FromImage(tempImage);
            SizeF size = graphics.MeasureString(text, renderableField.ControlFont, proposedSize.Width);

            float width = size.Width + 22;
            width = width < 200 ? 200 : width;

            renderableField.ControlWidthPercentage = width / (float)canvas.PagePanel.Size.Width;
            renderableField.ControlHeightPercentage = ((float)size.Height + 16) / (float)canvas.PagePanel.Size.Height;
            
            return location;
        }
        
        /// <summary>
        /// Performs a UI and metadata level delete of the controls. 
        /// </summary>
        /// <param name="list"></param>
        private void DeleteFieldControls(List<IFieldControl> list)
        {
            foreach (IFieldControl fieldControl in list)
            {
                if (fieldControl.Field != null)
                {
                    //--147
                    if (fieldControl.Field is GroupField) {  SetBackColorForGroupCheckBoxes(false); }
                    //--
                    fieldControl.Field.Delete();
                    ((Control)fieldControl).Dispose();
                }
             }
            canvas.DisposeControlTrackers();
        }

        /// <summary>
        /// Set the position percentage values in the fields based on the given location.
        /// 
        /// If the field is a FieldWithSeparatePrompt, the input control location is set to the default 
        /// position to the right of the prompt.
        /// </summary>
        /// <param name="field">The field being updated.</param>
        /// <param name="location">The top left corner of the prompt or control.</param>
        private void AutoAlignFieldGivenPromptLocation(Field field, System.Drawing.Point location)
        {
            Panel panel = canvas.PagePanel;
            bool isHorizontalAlign = field.GetView().PageLabelAlign.ToLowerInvariant().Equals("horizontal") ? true : false;
            
            if (field is FieldWithSeparatePrompt)
            {
                FieldWithSeparatePrompt positionedField = (FieldWithSeparatePrompt)field;
                positionedField.PromptLeftPositionPercentage = 1.0 * location.X / canvas.PagePanel.Size.Width;
                positionedField.PromptTopPositionPercentage = 1.0 * location.Y / canvas.PagePanel.Size.Height;

                int promptWidth = TextRenderer.MeasureText(positionedField.PromptText, positionedField.PromptFont).Width;
                int promptHeight = TextRenderer.MeasureText(positionedField.PromptText, positionedField.PromptFont).Height;
                
                if (isHorizontalAlign)
                {
                    int padWidth = 6;
                    double padHeight = 1.0 * location.Y / canvas.PagePanel.Size.Height;

                    ((FieldWithSeparatePrompt)field).ControlLeftPositionPercentage = 1.0 * (location.X + promptWidth + padWidth) / canvas.PagePanel.Size.Width;
                    ((FieldWithSeparatePrompt)field).ControlTopPositionPercentage = padHeight;
                }
                else
                {
                    int padHeight = -1;
                    int controlTop = location.Y + promptHeight + padHeight;

                    controlTop = canvas.Snap(controlTop, true);

                    ((FieldWithSeparatePrompt)field).ControlLeftPositionPercentage = positionedField.PromptLeftPositionPercentage;
                    ((FieldWithSeparatePrompt)field).ControlTopPositionPercentage = 1.0 * controlTop / canvas.PagePanel.Size.Height;
                }
            }
            else
            {
                ((FieldWithoutSeparatePrompt)field).ControlLeftPositionPercentage = 1.0 * location.X / canvas.PagePanel.Size.Width;
                ((FieldWithoutSeparatePrompt)field).ControlTopPositionPercentage = 1.0 * location.Y / canvas.PagePanel.Size.Height;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>This method should be merged with AutoAlignFieldGivenPromptLocation()</remarks>
        /// <param name="field"></param>
        /// <param name="panel"></param>
        /// <param name="location"></param>
        /// <param name="deltaLeft"></param>
        /// <param name="deltaTop"></param>
        private void PositionPastedFieldOnPanel(Field field, Panel panel, Point location, Double deltaLeft, Double deltaTop)
        {
            if (field is FieldWithSeparatePrompt)
            {
                FieldWithSeparatePrompt positionedField = (FieldWithSeparatePrompt)field;
                positionedField.PromptLeftPositionPercentage = 1.0 * location.X / canvas.PagePanel.Size.Width;
                positionedField.PromptTopPositionPercentage = 1.0 * location.Y / canvas.PagePanel.Size.Height;

                ((FieldWithSeparatePrompt)field).ControlLeftPositionPercentage = positionedField.PromptLeftPositionPercentage + deltaLeft;
                ((FieldWithSeparatePrompt)field).ControlTopPositionPercentage = positionedField.PromptTopPositionPercentage + deltaTop;
            }
            else
            {
                ((FieldWithoutSeparatePrompt)field).ControlLeftPositionPercentage = 1.0 * location.X / canvas.PagePanel.Size.Width;
                ((FieldWithoutSeparatePrompt)field).ControlTopPositionPercentage = 1.0 * location.Y / canvas.PagePanel.Size.Height;
            }
            if (field is MultilineTextField)
            {
                ((MultilineTextField)field).ControlHeightPercentage = 40.0 / canvas.PagePanel.Size.Height;
                ((MultilineTextField)field).ControlWidthPercentage = 150.0 / canvas.PagePanel.Size.Width;
            }
        }

        private void PositionGroupOnPanel(GroupField field, Panel panel, System.Drawing.Rectangle outline)
        {
            ((FieldWithoutSeparatePrompt)field).ControlLeftPositionPercentage = 1.0 * outline.X / canvas.PagePanel.Size.Width;
            ((FieldWithoutSeparatePrompt)field).ControlTopPositionPercentage = 1.0 * outline.Y / canvas.PagePanel.Size.Height;
            ((FieldWithoutSeparatePrompt)field).ControlHeightPercentage = 1.0 * outline.Height / canvas.PagePanel.Size.Height;
            ((FieldWithoutSeparatePrompt)field).ControlWidthPercentage = 1.0 * outline.Width / canvas.PagePanel.Size.Width;
        }

        private void MovePrompt(IFieldControl control)
        {
            MovePrompt(control, true);
        }

        public Control GetInputControl(String fieldName)
        {
            foreach (Control pageControl in canvas.PagePanel.Controls)
            {
                IFieldControl fc = pageControl as IFieldControl;
                if (fc != null && fc.Field != null)
                {
                    if (!(pageControl is DragableLabel))
                    {
                        if ((fc.Field.Name) == fieldName)
                        {
                            return pageControl;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the prompt controls for the given field.
        /// Returns null if there is no prompt for the given field.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public Control GetPromptControl(String fieldName)
        {
            foreach (Control pageControl in canvas.PagePanel.Controls)
            {
                IFieldControl fc = pageControl as IFieldControl;
                if (fc != null && fc.Field != null)
                {
                    if (pageControl is DragableLabel)
                    {
                        if ((fc.Field.Name) == fieldName)
                        {
                            return pageControl;
                        }
                    }
                }
            }
            return null;
        }

        public void MovePrompt(IFieldControl promptControl, bool allowSnapToGrid)
        {
            FieldWithSeparatePrompt field = (FieldWithSeparatePrompt)promptControl.Field;
            Point snapSize = new Point();

            Control inputControl = GetInputControl(promptControl.Field.Name);

            if (_fieldBefore == null)
            {
                _fieldBefore = CloneField(field);
            }
            
            // < check the configuration to see if the object snapped should be control or prompt >
            Configuration config = Configuration.GetNewInstance();

            if (allowSnapToGrid && inputControl != null)
            {
                if (config.Settings.SnapPromptToGrid)
                {
                    snapSize = SnapControlToGridRequest((Control)promptControl);
                    if (config.Settings.SnapInputControlToGrid == false)
                    {
                        inputControl.Left = inputControl.Left - snapSize.X;
                        inputControl.Top = inputControl.Top - snapSize.Y;
                    }
                }

                if (config.Settings.SnapInputControlToGrid)
                {
                    snapSize = SnapControlToGridRequest((Control)inputControl);
                    if (config.Settings.SnapPromptToGrid == false)
                    {
                        ((Control)promptControl).Left = ((Control)promptControl).Left - snapSize.X;
                        ((Control)promptControl).Top = ((Control)promptControl).Top - snapSize.Y;
                    }
                }
            }

            if ((ControlTracker)((IFieldControl)inputControl).Tracker != null)
            {
                ((ControlTracker)((IFieldControl)inputControl).Tracker).WrapControl();
            }

            if ((ControlTracker)promptControl.Tracker != null)
            {
                ((ControlTracker)promptControl.Tracker).WrapControl();
            }

            // < convert into percentage and save >
            field.PromptLeftPositionPercentage = AsPercentage((promptControl as Control).Left, canvas.PagePanel.Size.Width);
            field.PromptTopPositionPercentage = AsPercentage((promptControl as Control).Top, canvas.PagePanel.Size.Height);

            if (inputControl != null)
            {
                field.ControlLeftPositionPercentage = AsPercentage((inputControl as Control).Left, canvas.PagePanel.Size.Width);
                field.ControlTopPositionPercentage = AsPercentage((inputControl as Control).Top, canvas.PagePanel.Size.Height);
            }

            PersistFieldChange(field, BackupFieldAction.Change);
        }

        public double AsPercentage(int pixels, int dimension)
        {
            Double percentage = ((Double)(pixels)) / ((Double)dimension);
            return percentage;
        }

        private void OnPromptMove(IFieldControl control)
        {
            MovePrompt(control, true);
            PersistChildFieldNames();
            canvas.RenderControlTrackers(selectedFieldControls);
        }

        private void MoveGroup(IFieldControl groupControl)
        {
            canvas.HideUpdateStart("Moving group control.");
            _fieldBefore = CloneField(groupControl.Field);
            Configuration config = Configuration.GetNewInstance();
            SnapControlToGridRequest((Control)groupControl);

            ((GroupField)groupControl.Field).ControlLeftPositionPercentage = AsPercentage(((Control)groupControl).Left, canvas.PagePanel.Size.Width);
            ((GroupField)groupControl.Field).ControlTopPositionPercentage = AsPercentage(((Control)groupControl).Top, canvas.PagePanel.Size.Height);
            PersistFieldChange(groupControl.Field, BackupFieldAction.Change);
            PanelFieldUpdate(groupControl.Field);
            SetZeeOrderOfGroups();

            List<Control> childControls = new List<Control>();

            String[] childNames = ((GroupField)groupControl.Field).ChildFieldNames.Split(new char[] { ',' });

            canvas.UpdateHidePanel("Finding group child controls.");
            foreach (Control pageControl in canvas.PagePanel.Controls)
            {
                if (pageControl is IFieldControl)
                {
                    IFieldControl fieldControl = pageControl as IFieldControl;

                    foreach (String name in childNames)
                    {
                        if (fieldControl != null && fieldControl.Field.Name == name)
                        {
                            childControls.Add(pageControl);
                        }
                    }
                }
            }

            foreach (Control childControl in childControls)
            {
                childControl.Left = ((Control)groupControl).Left + ((DragableGroupBox)groupControl).GetHorizontalDistanceTo(childControl);
                childControl.Top = ((Control)groupControl).Top + ((DragableGroupBox)groupControl).GetVerticalDistanceTo(childControl);

                canvas.UpdateHidePanel(((IFieldControl)childControl).Field.Name);
                if (((IFieldControl)childControl).Field is FieldWithSeparatePrompt)
                {
                    if (childControl is DragableLabel)
                    {
                        MovePrompt((IFieldControl)childControl, false);
                    }
                }
                else
                {
                    MoveField((IFieldControl)childControl, false);
                }
            }
            //----147
              SetBackColorForGroupCheckBoxes(true);
            //---
            canvas.RenderControlTracker((Control)groupControl, true);
            canvas.HideUpdateEnd();
        }

        public void PersistSelectedFieldControlCollection()
        {
            foreach (KeyValuePair<IFieldControl, Point> kvp in SelectedFieldControls)
            {
                IFieldControl control = kvp.Key;

                if (((IFieldControl)control).Field is FieldWithSeparatePrompt)
                {
                    if (control is DragableLabel)
                    {
                        MovePrompt(control, false);
                    }
                }
                else
                {
                    MoveField(control, false);
                }
            }
        }

        /// <summary>
        /// SnapToGridRequest
        /// </summary>
        /// <param name="movedControl"></param>
        /// <returns>a point representing the distance the control was moved during the snap</returns>
        private Point SnapToGridRequest(Control movedControl)
        {
            Point delta = new Point();
            Configuration config = Configuration.GetNewInstance();

            // < exit now if snap to grid is not selected >
            if (config.Settings.SnapToGrid == false)
            {
                return delta;
            }

            int gridSize = config.Settings.GridSize;

            // < if snap prompt to grid is selected but 
            if (config.Settings.SnapPromptToGrid)
            {
                int gridSizePixels = (gridSize + 1) * Epi.Constants.GRID_FACTOR;

                int leftmod = movedControl.Left % gridSizePixels;
                if (leftmod < (gridSizePixels / 2))
                {
                    delta.X = -leftmod;
                    movedControl.Left = movedControl.Left + delta.X;
                }
                else
                {
                    delta.X = gridSizePixels - leftmod;
                    movedControl.Left = movedControl.Left + delta.X;
                }

                int topMod = movedControl.Top % gridSizePixels;
                if (topMod < (gridSizePixels / 2))
                {
                    delta.Y = -topMod;
                    movedControl.Top = movedControl.Top + delta.Y;
                }
                else
                {
                    delta.Y = gridSizePixels - topMod;
                    movedControl.Top = movedControl.Top + delta.Y;
                }
            }
            return delta;
        }

        /// <summary>
        /// SnapControlToGridRequest
        /// </summary>
        /// <param name="movedControl"></param>
        /// <returns>a point representing the distance the control was moved during the snap</returns>
        private Point SnapControlToGridRequest(Control movedControl)
        {
            Point originalLocation = new Point(movedControl.Left, movedControl.Top);
            Configuration config = Configuration.GetNewInstance();
            if (config.Settings.SnapToGrid == false)
            {
                return new Point();
            }

            int gridSize = config.Settings.GridSize;
            int gridSizePixels = (gridSize + 1) * Epi.Constants.GRID_FACTOR;

            int leftmod = movedControl.Left % gridSizePixels;
            if (leftmod < (gridSizePixels / 2))
            {
                movedControl.Left = movedControl.Left - leftmod;
            }
            else
            {
                movedControl.Left = movedControl.Left + gridSizePixels - leftmod;
            }

            int topMod = movedControl.Top % gridSizePixels;
            if (topMod < (gridSizePixels / 2))
            {
                movedControl.Top = movedControl.Top - topMod;
            }
            else
            {
                movedControl.Top = movedControl.Top + gridSizePixels - topMod;
            }

            return new Point(originalLocation.X - movedControl.Left, originalLocation.Y - movedControl.Top);
        }

        private void OnFieldMove(IFieldControl control)
        {
            if (control.Field is GroupField)
            {
                MoveGroup(control);
            }
            else
            {
                MoveField(control);
            }

            PersistChildFieldNames();
            canvas.RenderControlTrackers(selectedFieldControls);
        }

        private void MoveField(IFieldControl control)
        {
            MoveField(control, true);
        }

        private void MoveField(IFieldControl control, bool allowSnapToGrid)
        {
            RenderableField field = (RenderableField)control.Field;

            if (_fieldBefore == null)
            {
                _fieldBefore = CloneField(field);
            }

            Configuration config = Configuration.GetNewInstance();
            if (allowSnapToGrid)
            {
                if (config.Settings.SnapInputControlToGrid == true)
                {
                    SnapControlToGridRequest((Control)control);
                }
            }

            if ((ControlTracker)control.Tracker != null)
            {
                ((ControlTracker)control.Tracker).WrapControl();
            }

            field.ControlLeftPositionPercentage = AsPercentage(((Control)control).Left, canvas.PagePanel.Size.Width);
            field.ControlTopPositionPercentage = AsPercentage(((Control)control).Top, canvas.PagePanel.Size.Height);
            
            PersistFieldChange(field, BackupFieldAction.Change);
            PanelFieldUpdate(field);
        }
        
        public void PersistChildFieldNames()
        {
            ArrayList newList = new ArrayList();
            
            foreach (Control outerGroupCandidate in canvas.PagePanel.Controls)
            {
                IFieldControl outerGroupCandidateField = outerGroupCandidate as IFieldControl;
                newList.Clear();

                if (outerGroupCandidateField != null && outerGroupCandidateField.Field is GroupField)
                {
                    Rectangle outerGroupRectangle = new Rectangle(outerGroupCandidate.Location, outerGroupCandidate.Size);

                    foreach (Control innerControlCandidate in canvas.PagePanel.Controls)
                    {
                        if (innerControlCandidate != null 
                            && outerGroupRectangle.Contains(innerControlCandidate.Location) 
                            && outerGroupCandidate != innerControlCandidate
                            && innerControlCandidate is ControlTracker == false
                            && innerControlCandidate is IFieldControl)
                        {
                            string name = ((IFieldControl)innerControlCandidate).Field.Name;
                            if (newList.Contains(name) == false)
                            {
                                newList.Add(name);
                            }
                        }
                    }
                    
                    ((GroupField)outerGroupCandidateField.Field).ChildFieldNames = string.Join(",", (string[])(newList.ToArray(typeof(String))));
                    outerGroupCandidateField.Field.SaveToDb();

                    ((DragableGroupBox)outerGroupCandidate).CaptureDistances(canvas.PagePanel);        
                }
            }
        }

        private void OnFieldResize(IFieldControl control)
        {
            RenderableField field = (RenderableField)control.Field;
            _fieldBefore = CloneField(field);

            if (field != null)
            {
                Configuration config = Configuration.GetNewInstance();

                if (config.Settings.SnapToGrid)
                {
                    if (config.Settings.SnapInputControlToGrid == true)
                    {
                        SnapControlToGridRequest((Control)control);
                    }
                }
                
                field.ControlLeftPositionPercentage = AsPercentage(((Control)control).Left, canvas.PagePanel.Size.Width);
                field.ControlTopPositionPercentage = AsPercentage(((Control)control).Top, canvas.PagePanel.Size.Height);
                
                if (config.Settings.SnapToGrid)
                {
                    ((Control)control).Width = canvas.Snap(((Control)control).Width);
                }

                if (((Control)control).Width < 16) { ((Control)control).Width = 16; }
                if (((Control)control).Height < 16) { ((Control)control).Height = 16; }

                field.ControlWidthPercentage = 1.0 * canvas.Snap(((Control)control).Width) / canvas.PagePanel.Size.Width;
                field.ControlHeightPercentage = 1.0 * ((Control)control).Height / canvas.PagePanel.Size.Height;

                PersistFieldChange(field, BackupFieldAction.Change);
            }
            
            PersistChildFieldNames();
            PanelFieldUpdate(field);
        }

        public void LoadPage(Page page)
        {
            ControlFactory factory = ControlFactory.Instance;

            RemovePagePanelControls();
            canvas.ChangePagePanelProperitesForPage();
            page.view.MustRefreshFieldCollection = true;

            List<Control> controls = factory.GetPageControls(page, canvas.PagePanel.Size);
            AddControlsToPanel(controls);

            PersistChildFieldNames();
            SetZeeOrderOfGroups();
            SetBackColorForGroupCheckBoxes(true);
        }

        public void AddControlsToPanel(List<Control> controls)
        {
            Boolean hasGroup = false;
            
            foreach (Control control in controls)
            {
                control.MouseEnter += new EventHandler(control_MouseEnter);
                control.MouseDown += new MouseEventHandler(control_MouseDown);

                if (control is IFieldControl)
                {
                    if (((IFieldControl)control).Field is FieldWithSeparatePrompt)
                    {
                        control.MouseDown += new MouseEventHandler(canvas.FieldDefinition_MouseDown);
                    }
                    else if (((IFieldControl)control).Field is FieldWithoutSeparatePrompt)
                    {
                        control.MouseDown += new MouseEventHandler(canvas.FieldDefinition_MouseDown);
                    }
                }

                if (control is DragableGrid)
                {
                    ((DataGridView)control).ColumnWidthChanged += new DataGridViewColumnEventHandler(DragableGrid_ColumnWidthChanged);
                }

                canvas.PagePanel.Controls.Add(control);

                if (control is GroupBox == false)
                {
                    control.BringToFront();
                }
            }

            if (hasGroup)
            {
                SetZeeOrderOfGroups();
                SetBackColorForGroupCheckBoxes(true);
            }
        }

        public void DragableGrid_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            GridField gridField = ((GridField)((DragableGrid)sender).Field);

            foreach (GridColumnBase fieldColumn in gridField.Columns)
            {
                if (fieldColumn is PredefinedColumn == false)
                {
                    if (fieldColumn.Name == e.Column.Name)
                    {
                        fieldColumn.Width = e.Column.Width;
                        fieldColumn.SaveToDb();
                    }
                }
            }
        }

        public void RemovePagePanelControls()
        {
            foreach (Control control in canvas.PagePanel.Controls)
            {
                if (control is Label)
                {
                    control.MouseEnter -= new EventHandler(control_MouseEnter);
                }

                if (!(control is Label))
                {
                    control.MouseEnter -= new EventHandler(control_MouseEnter);
                    control.MouseDown -= new MouseEventHandler(control_MouseDown);
                }

                if (control is IFieldControl)
                {
                    if ((((IFieldControl)control).Field is FieldWithSeparatePrompt))
                    {
                        control.MouseDown -= new MouseEventHandler(canvas.FieldDefinition_MouseDown);
                    }
                    else if (((IFieldControl)control).Field is FieldWithoutSeparatePrompt)
                    {
                        control.MouseDown -= new MouseEventHandler(canvas.FieldDefinition_MouseDown);
                    }
                }

                canvas.PagePanel.Controls.Remove(control);
                control.Dispose();
            }

            GC.Collect();
            canvas.PagePanel.Controls.Clear();
        }

        public void SetZeeOrderOfGroups()
        {
           System.Collections.Generic.SortedList<IFieldControl, Point> pointSortedGroups = new SortedList<IFieldControl, Point>(new GroupZeeOrderComparer());

           foreach (Control possibleGroupControl in canvas.PagePanel.Controls)
           {
               IFieldControl fieldControl = possibleGroupControl as IFieldControl;
               if (fieldControl != null && fieldControl.Field is GroupField)
               {
                   pointSortedGroups.Add(fieldControl, ((Control)fieldControl).Location);
               }
               foreach (KeyValuePair<IFieldControl, Point> kvp in pointSortedGroups)
               {
                   ((Control)kvp.Key).SendToBack();
               }
           }
        }
        
        ///<summary>
        ///Assign Backcolor for Checkboxes
        ///</summary>
        /// <param name="field">SetcolorofGroup</param>
        /// <param name="panel">The panel the field belongs to</param>
        /// <param name="currentPage">The current page</param>
        private void SetBackColorForGroupCheckBoxes(bool SetcolorofGroup)
        {
            //---147
               foreach (Control GroupControl in canvas.PagePanel.Controls)
                {
                    IFieldControl PfieldControl = GroupControl as IFieldControl;
                    if (PfieldControl != null && PfieldControl.Field is GroupField)
                    {
                        ArrayList groupChildren = new ArrayList();
                        String[] names = ((GroupField)PfieldControl.Field).ChildFieldNames.Split((Constants.LIST_SEPARATOR));
                        foreach (string gfc in names)
                        {
                            if (gfc.Length > 0) { groupChildren.Add(gfc);}

                        }
                        if (groupChildren.Count == 0) { return; }
                        foreach (Control pcontrol in canvas.PagePanel.Controls)
                        {
                            IFieldControl PossfieldControl = pcontrol as IFieldControl;
                            if (PossfieldControl != null && PossfieldControl.Field is CheckBoxField)
                            {
                                if (groupChildren.Contains(PossfieldControl.Field.Name.ToString()))
                                {
                                    if (SetcolorofGroup)
                                    {
                                        ((CheckBox)PossfieldControl).BackColor = ((GroupField)PfieldControl.Field).BackgroundColor;
                                    }
                                    else
                                    {
                                        ((CheckBox)PossfieldControl).BackColor = System.Drawing.Color.Transparent;
                                    }
                                }
                            }
                        }
                    }
                }
              }
      //----
      
        /// <summary>
        /// Assign the tab index for controls
        /// </summary>
        /// <param name="field">The field to be assigned a tab index</param>
        /// <param name="panel">The panel the field belongs to</param>
        /// <param name="currentPage">The current page</param>
        private void AssignTabIndex(Field field, Panel panel, Page currentPage)
        {
            double maxTabOrder = currentPage.GetMetadata().GetMaxTabIndex(currentPage.Id, currentPage.GetView().Id);
            ((RenderableField)field).TabIndex = maxTabOrder + 1;
        }

        #endregion  //Private Methods

        #region IEnterInterpreterHost Members

        public bool Register(IEnterInterpreter enterInterpreter)
        {
            enterInterpreter.Host = this;
            return true;
        }

        /// <summary>
        /// Assign
        /// </summary>
        /// <param name="pName">name</param>
        /// <param name="pValue">value</param>
        /// <returns>bool</returns>
        public bool Assign(string pName, object pValue)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool AssignGrid(string pName, object pValue, int pIndex0, object pIndex1)
        {
            throw new Exception("The method or operation is not implemented.");
        }


        public EpiInfo.Plugin.IVariable GetGridValue(string pName, int pIndex0, object pIndex1)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool Geocode(string address, string latName, string longName)
        {
            return false;
        }

        /// <summary>
        /// AutoSearch
        /// </summary>
        /// <param name="pIdentifierList">Identifier list</param>
        /// <param name="pDisplayList">Display List</param>
        /// <param name="pAlwaysShow">Always</param>
        public void AutoSearch(string[] pIdentifierList, string[] pDisplayList, bool pAlwaysShow)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// AutoSearch Function
        /// </summary>
        /// <param name="pIdentifierList">Identifier list</param>
        /// <param name="pDisplayList">Display List</param>
        /// <param name="pAlwaysShow">Always</param>
        public int AutosearchFunction(string[] pIdentifierList, string[] pDisplayList, bool pAlwaysShow)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// IsDistinct
        /// </summary>
        /// <param name="pIdentifierList">Identifier list</param>
        public bool IsUnique(string[] pIdentifierList)
        {
            return false;
        }

        /// <summary>
        /// Clear
        /// </summary>
        /// <param name="pIdentifierList">Identifier List</param>
        public void Clear(string[] pIdentifierList)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Dialog
        /// </summary>
        /// <param name="pTextPrompt">Text prompt</param>
        /// <param name="pTitleText">Title Text</param>
        public void Dialog(string pTextPrompt, string pTitleText)
        {
            MsgBox.Show(pTextPrompt, pTitleText); 
        }

        /// <summary>
        /// Dialog
        /// </summary>
        /// <param name="pTextPrompt">Text prompt</param>
        /// <param name="pVariable">Variable</param>
        /// <param name="pListType">List Type</param>
        /// <param name="pTitleText">Title text</param>
        public void Dialog(string pTextPrompt, ref object pVariable, string pListType, string pTitleText)
        {
            throw new Exception("The method or operation is not implemented.");
        }


        public int RecordCount()
        {
            return 0;
        }


        public void Highlight(string[] pNameList, bool pIsAnExceptList)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void UnHighlight(string[] pNameList, bool pIsAnExceptList)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Enable(string[] pNameList, bool pIsAnExceptList)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Disable(string[] pNameList, bool pIsAnExceptList)
        {
            throw new Exception("The method or operation is not implemented.");
        }


        public void SetRequired(string[] pNameList)
        {

        }
        public void SetNotRequired(string[] pNameList)
        {

        }

        public List<string> GetDbListValues(string ptablename, string pvariablename)
        {
            List<string>list=new List<string>();
            return list;
        }

        /// <summary>
        /// Dialog
        /// </summary>
        /// <param name="text">text</param>
        /// <param name="caption">caption</param>
        /// <param name="mask">mask</param>
        /// <param name="modifier">modifier</param>
        /// <param name="input">input</param>
        /// <returns>bool</returns>
        public bool Dialog(string text, string caption, string mask, string modifier, ref object input, EpiInfo.Plugin.DataType dataType)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// GetValue
        /// </summary>
        /// <param name="pName">name</param>
        /// <returns>string</returns>
        public string GetValue(string pName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// GoTo
        /// </summary>
        /// <param name="pDestination">destination</param>
        public void GoTo(string pDestination, string targetPage = "", string targetForm = "")
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Hide
        /// </summary>
        /// <param name="pNameList">name list</param>
        /// <param name="pIsAnExceptList">is an except list?</param>
        public void Hide(string[] pNameList, bool pIsAnExceptList)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Unhide
        /// </summary>
        /// <param name="pNameList">name list</param>
        /// <param name="pIsAnExceptList">is an except list?</param>
        public void UnHide(string[] pNameList, bool pIsAnExceptList)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Try to get Field Info
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="dataType">data type</param>
        /// <param name="value">value</param>
        /// <returns>bool</returns>
        public bool TryGetFieldInfo(string name, out EpiInfo.Plugin.DataType dataType, out string value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Quit
        /// </summary>
        public void Quit()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// New Record
        /// </summary>
        public void NewRecord()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// New Record
        /// </summary>
        public bool SaveRecord()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// is ExecutionEnabled
        /// </summary>
        public bool IsExecutionEnabled
        { get { return true; } }

        /// <summary>
        /// is SupressErrorsEnabled
        /// </summary>
        public bool IsSuppressErrorsEnabled
        { get { return true; } }

        #endregion
        
        public void DeleteField(Field field)
        {
            List<IFieldControl> list = new List<IFieldControl>();

            foreach (Control control in canvas.PagePanel.Controls)
            {
                IFieldControl fieldControl = control as IFieldControl;
                if (fieldControl != null)
                {
                    if (fieldControl.Field.UniqueId == field.UniqueId)
                    {
                        list.Add(fieldControl);
                    }
                }
            }
            
            DeleteFieldControls(list);
        }

        public void Undo()
        {
            canvas.DisposeControlTrackers();
            if (BackupFieldIndex == 0) return;
            
            Int64 executeRestorePointer = BackupFieldIndex;
            SortedDictionary<Int64, BackupField>.Enumerator enumerator = backupFields.GetEnumerator();
            if (enumerator.MoveNext())
            {
                if (executeRestorePointer == enumerator.Current.Key)
                {
                    BackupFieldIndex = 0;
                }
                else
                {
                    do
                    {
                        if (enumerator.Current.Key == executeRestorePointer)
                        {
                            break;
                        }

                        BackupFieldIndex = enumerator.Current.Key;
                    }
                    while (enumerator.MoveNext());
                }
            }

            ((MakeViewMainForm)canvas.MainForm).RedoButtonEnabled = true;

            if (backupFields.ContainsKey(executeRestorePointer))
            {
                if (backupFields[executeRestorePointer].BackupAction == BackupFieldAction.New)
                {
                    Field field = backupFields[executeRestorePointer].Field(BackupFieldAction.UndoNew);
                    _fieldBefore = CloneField(field);
                    PersistFieldChange(field, BackupFieldAction.UndoNew);
                }
                else if (backupFields[executeRestorePointer].BackupAction == BackupFieldAction.Paste)
                {
                    Field field = backupFields[executeRestorePointer].Field(BackupFieldAction.UndoPaste);
                    _fieldBefore = CloneField(field);
                    PersistFieldChange(field, BackupFieldAction.UndoPaste);
                }
                else if (backupFields[executeRestorePointer].BackupAction == BackupFieldAction.Delete)
                {
                    Field field = backupFields[executeRestorePointer].Field(BackupFieldAction.UndoDelete);

                    Page page = ((RenderableField)field).Page;
                    IMetadataProvider metadata = page.GetMetadata();
                    DataTable table = metadata.GetPagesForView(page.view.Id);
                    
                    string where = string.Format("PageId = {0}", page.Id);

                    if (table.Select(where).Length > 0)
                    {
                        _fieldBefore = CloneField(field);
                        PersistFieldChange(field, BackupFieldAction.UndoDelete);
                        PanelFieldUpdate(field);
                    }
                }
                else if (backupFields[executeRestorePointer].BackupAction == BackupFieldAction.Change)
                {
                    Field field = backupFields[executeRestorePointer].Field(BackupFieldAction.UndoChange);
                    _fieldBefore = CloneField(field);
                    PersistFieldChange(field, BackupFieldAction.UndoChange);
                    PanelFieldUpdate(field);
                }
            }
        }

        public void Redo()
        {
            canvas.DisposeControlTrackers();
            SortedDictionary<Int64, BackupField>.Enumerator enumerator = backupFields.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Key == BackupFieldIndex || BackupFieldIndex == 0)
                {
                    if (BackupFieldIndex == 0)
                    {
                        BackupFieldIndex = enumerator.Current.Key;
                        ((MakeViewMainForm)canvas.MainForm).RedoButtonEnabled = enumerator.MoveNext() ? true : false;
                    }
                    else
                    {
                        if (enumerator.MoveNext())
                        {
                            BackupFieldIndex = enumerator.Current.Key;
                            ((MakeViewMainForm)canvas.MainForm).RedoButtonEnabled = enumerator.MoveNext() ? true : false;
                        }
                    }
                    break;
                }
            }

            if (backupFields.ContainsKey(BackupFieldIndex))
            {
                if (backupFields[BackupFieldIndex].BackupAction == BackupFieldAction.Delete)
                {
                    Field field = backupFields[BackupFieldIndex].Field(BackupFieldAction.RedoDelete);
                    _fieldBefore = CloneField(field);
                    PersistFieldChange(field, BackupFieldAction.RedoDelete);
                }
                else if (backupFields[BackupFieldIndex].BackupAction == BackupFieldAction.New)
                {
                    Field field = backupFields[BackupFieldIndex].Field(BackupFieldAction.RedoNew);

                    Page page = ((RenderableField)field).Page;
                    IMetadataProvider metadata = page.GetMetadata();
                    DataTable table = metadata.GetPagesForView(page.view.Id);

                    string where = string.Format("PageId = {0}", page.Id);

                    if (table.Select(where).Length > 0)
                    {
                        _fieldBefore = CloneField(field);
                        PersistFieldChange(field, BackupFieldAction.RedoNew);
                        PanelFieldUpdate(field);
                    }
                }
                else if (backupFields[BackupFieldIndex].BackupAction == BackupFieldAction.Paste)
                {
                    Field field = backupFields[BackupFieldIndex].Field(BackupFieldAction.RedoPaste);

                    Page page = ((RenderableField)field).Page;
                    IMetadataProvider metadata = page.GetMetadata();
                    DataTable table = metadata.GetPagesForView(page.view.Id);

                    string where = string.Format("PageId = {0}", page.Id);

                    if (table.Select(where).Length > 0)
                    {
                        _fieldBefore = CloneField(field);
                        PersistFieldChange(field, BackupFieldAction.RedoPaste);
                        PanelFieldUpdate(field);
                    }
                }
                else if (backupFields[BackupFieldIndex].BackupAction == BackupFieldAction.Change)
                {
                    Field field = backupFields[BackupFieldIndex].Field(BackupFieldAction.RedoChange);
                    _fieldBefore = CloneField(field);
                    PersistFieldChange(field, BackupFieldAction.RedoChange);
                    PanelFieldUpdate(field);
                }
            }
        }

        public bool PersistFieldChange(Field field, BackupFieldAction backupFieldAction)
        {
            if (field == null) return false;

            if (canvas.TabIndexIndicators == null || canvas.TabIndexIndicators.Count == 0)
            {
                canvas.RemoveTabIndexIndicators();
            }

            if (((Epi.Fields.RenderableField)(field)).Page.Id != projectExplorer.currentPage.Id)
            {
                ProjectExplorer.SelectPage(((Epi.Fields.RenderableField)(field)).Page);
            }

            if (field.View != projectExplorer.SelectedPage.view)
            {
                field.View = projectExplorer.SelectedPage.view;
            }

            if (backupFieldAction == BackupFieldAction.UndoDelete
                || backupFieldAction == BackupFieldAction.New
                || backupFieldAction == BackupFieldAction.RedoNew
                || backupFieldAction == BackupFieldAction.Paste
                || backupFieldAction == BackupFieldAction.RedoPaste
                )
            {
                if (IsTableFull) return false;
                field.Id = 0;
            }

            if (backupFieldAction == BackupFieldAction.Change
                || backupFieldAction == BackupFieldAction.New
                || backupFieldAction == BackupFieldAction.RedoNew
                || backupFieldAction == BackupFieldAction.Paste
                || backupFieldAction == BackupFieldAction.RedoPaste
                || backupFieldAction == BackupFieldAction.RedoChange
                || backupFieldAction == BackupFieldAction.UndoChange
                || backupFieldAction == BackupFieldAction.UndoDelete
                )
            {
               
                field.SaveToDb();

                if (backupFieldAction == BackupFieldAction.New   ||
                    backupFieldAction == BackupFieldAction.RedoNew)
                {
                    _newFieldIds.Add(field.Id);
                }
            }

            if (backupFieldAction == BackupFieldAction.UndoNew
                || backupFieldAction == BackupFieldAction.Delete
                || backupFieldAction == BackupFieldAction.RedoDelete)
            {
                DeleteField(field);
                _newFieldIds.Remove(field.Id);
            }

            Field fieldAfter = CloneField(field);

            if (backupFieldAction == BackupFieldAction.New
                || backupFieldAction == BackupFieldAction.Paste
                || backupFieldAction == BackupFieldAction.Change
                || backupFieldAction == BackupFieldAction.Delete)
            {
                AddBackupField(_fieldBefore, fieldAfter, backupFieldAction);
            }

            MarshalCollectedDataPerPageTable(_fieldBefore, fieldAfter, backupFieldAction);
            _fieldBefore = fieldAfter = null;

            return true;
        }

        void MarshalCollectedDataPerPageTable(Field fieldBefore, Field fieldAfter, BackupFieldAction backupFieldAction)
        {
            ((MakeViewMainForm)canvas.MainForm).UpdateStatus("Updating data tables.");
            
            if (InitialData != null)
            {
                if (backupFieldAction == BackupFieldAction.UndoNew
                    || backupFieldAction == BackupFieldAction.Delete
                    || backupFieldAction == BackupFieldAction.RedoDelete)
                {
                    if (fieldBefore != null)
                    {
                        InitialData.Add(((RenderableField)fieldBefore).GetView().Name, fieldBefore.Name);

                        string sourceTableName = ((RenderableField)fieldBefore).Page.TableName;

                        if (project.CollectedData.ColumnExists(sourceTableName, fieldBefore.Name))
                        {
                            project.CollectedData.DeleteColumn(sourceTableName, fieldBefore.Name);
                        }
                    }
                }
                
                if (backupFieldAction == BackupFieldAction.New
                    || backupFieldAction == BackupFieldAction.RedoNew
                    || backupFieldAction == BackupFieldAction.Paste
                    || backupFieldAction == BackupFieldAction.RedoPaste
                    || backupFieldAction == BackupFieldAction.UndoDelete)
                {
                    if (fieldAfter is Epi.Fields.IInputField)
                    {
                        string pageTableName = ((RenderableField)fieldAfter).Page.TableName;
                        string viewName = ((RenderableField)fieldAfter).GetView().Name;

                        if (project.CollectedData.TableExists(viewName) && project.CollectedData.TableExists(pageTableName) == false)
                        {
                            List<Epi.Data.TableColumn> columns = new List<Epi.Data.TableColumn>();
                            columns.Add(new Epi.Data.TableColumn("GlobalRecordId", Epi.Data.GenericDbColumnType.String, 255, false));
                                
                            // Create a new collected data page table.
                            project.CollectedData.CreateTable(pageTableName, columns);

                            // Insert a new row for each record that exists the the master page table.
                            foreach (DataRow row in project.CollectedData.GetTableData(viewName).Rows)
                            {
                                string id = row["GlobalRecordId"].ToString();
                                string queryString = string.Format("GlobalRecordId = '{0}'", id);
                                DataRow[] rows = project.CollectedData.GetTableData(viewName).Select(queryString);

                                queryString = string.Format("insert into [{0}] (GlobalRecordId) values ('{1}')",
                                    pageTableName,
                                    id);

                                Epi.Data.Query updateQuery = project.CollectedData.CreateQuery(queryString);
                                project.CollectedData.ExecuteNonQuery(updateQuery);
                            }
                        }

                        if (project.CollectedData.TableExists(pageTableName))
                        {
                            if (project.CollectedData.ColumnExists(pageTableName, fieldAfter.Name) == false)
                            {
                                project.CollectedData.CreateTableColumn((Epi.Fields.IInputField)fieldAfter, pageTableName);
                            }

                            if (backupFieldAction == BackupFieldAction.UndoDelete
                                || backupFieldAction == BackupFieldAction.Paste
                                || backupFieldAction == BackupFieldAction.RedoPaste)
                            {
                                DataRow fieldMetadata = project.Metadata.GetFieldAsDataRow((Field)fieldAfter);
                                string viewNamePageIdTableName = viewName + fieldMetadata["PageId"].ToString();
                                string fieldName = fieldMetadata["Name"] as string;

                                InitialData.MergeWithPageTable(((RenderableField)fieldAfter).GetView().Name, fieldName, viewNamePageIdTableName);
                            }
                        }
                    }
                }
            }
        }

        void DeleteCollectedDataPerPageTable(Field fieldBefore, BackupFieldAction backupFieldAction)
        {
            if (backupFieldAction == BackupFieldAction.UndoNew
                || backupFieldAction == BackupFieldAction.Delete
                || backupFieldAction == BackupFieldAction.RedoDelete)
            {
                string sourceTableName = ((RenderableField)fieldBefore).Page.TableName;

                if (project.CollectedData.ColumnExists(sourceTableName, fieldBefore.Name))
                {
                    project.CollectedData.DeleteColumn(sourceTableName, fieldBefore.Name);
                }
            }
        }

        /// <summary>
        /// Gets a new field object from the database and adds it to the backupFields dictionary.
        /// </summary>
        /// <param name="fieldBefore"></param>
        /// <param name="fieldAfter"></param>
        /// <param name="backupFieldAction"></param>
        private void AddBackupField(Field fieldBefore, Field fieldAfter, BackupFieldAction backupFieldAction)
        {
            List<long> removeList = new List<long>();
            
            foreach (KeyValuePair<Int64, BackupField> kvp in backupFields)
            {
                if (kvp.Key > BackupFieldIndex)
                {
                    removeList.Add(kvp.Key);
                };
            }

            foreach (long remove in removeList)
            {
                backupFields.Remove(remove);
            }

            BackupField backupField = null;

            do
            {
                backupField = new BackupField(fieldBefore, fieldAfter, backupFieldAction);
            }
            while (backupFields.ContainsKey(backupField.Ticks));

            backupFields.Add(backupField.Ticks, backupField);
            BackupFieldIndex = backupField.Ticks;

            ((MakeViewMainForm)canvas.MainForm).RedoButtonEnabled = false;
        }

        public void OnPageDeleteUpdateFieldQueue(Page page)
        {
            #region InputValidation
            if (page == null) return;
            if (backupFields == null) return;
            #endregion
            
            List<long> removeList = new List<long>();
            
            foreach (KeyValuePair<Int64, BackupField> kvp in backupFields)
            {
                if (kvp.Value == null)
                {
                    continue;
                }

                RenderableField fieldUndo = (RenderableField)((BackupField)kvp.Value).fieldBefore;
                RenderableField fieldRedo = (RenderableField)((BackupField)kvp.Value).fieldAfter;

                if (fieldUndo != null && fieldUndo.Page != null && fieldUndo.Page.Id == page.Id ||
                    fieldRedo != null && fieldRedo.Page != null && fieldRedo.Page.Id == page.Id)
                {
                    removeList.Add(kvp.Key);
                    continue;
                };
            }

            foreach (long remove in removeList)
            {
                backupFields.Remove(remove);
            }

            MakeViewMainForm mainForm = (MakeViewMainForm)projectExplorer.MainForm;

            if (backupFields.ContainsKey(backupFieldIndex) == false)
            {
                long tempBackupFieldIndex = backupFieldIndex;

                foreach (KeyValuePair<Int64, BackupField> kvp in backupFields)
                {
                    if (kvp.Key < backupFieldIndex)
                    {
                        tempBackupFieldIndex = kvp.Key;
                    }
                }

                backupFieldIndex = tempBackupFieldIndex;

                foreach (KeyValuePair<Int64, BackupField> kvp in backupFields)
                {
                    if (kvp.Key <= backupFieldIndex) mainForm.UndoButtonEnabled = true;
                    if (kvp.Key >= backupFieldIndex) mainForm.RedoButtonEnabled = true;
                }
            }
        }

        /// <summary>
        /// Deprecate after ICloneable is implemented for IFieldControls.
        /// </summary>
        /// <param name="field"></param>
        private Field CloneField(Field field)
        {
            try
            {
                Field clone = null;
                if (field is SingleLineTextField)
                {
                    clone = ((SingleLineTextField)field).Clone();
                }
                else if (field is MultilineTextField)
                {
                    clone = ((MultilineTextField)field).Clone();
                }
                else if (field is YesNoField)
                {
                    clone = ((YesNoField)field).Clone();
                }
                else if (field is GUIDField)
                {
                    clone = ((GUIDField)field).Clone();
                }
                else if (field is DDLFieldOfCodes)
                {
                    clone = ((DDLFieldOfCodes)field).Clone();
                }
                else if (field is TableBasedDropDownField)
                {
                    clone = ((TableBasedDropDownField)field).Clone();
                }
                else if (field is MirrorField)
                {
                    clone = ((MirrorField)field).Clone();
                }
                else if (field is ImageField)
                {
                    clone = ((ImageField)field).Clone();
                }
                else if (field is GridField)
                {
                    clone = ((GridField)field).Clone();
                }
                else if (field is NumberField)
                {
                    clone = ((NumberField)field).Clone();
                }
                else if (field is DateField)
                {
                    clone = ((DateField)field).Clone();
                }
                else if (field is DateTimeField)
                {
                    clone = ((DateTimeField)field).Clone();
                }
                else if (field is PhoneNumberField)
                {
                    clone = ((PhoneNumberField)field).Clone();
                }
                else if (field is CheckBoxField)
                {
                    clone = ((CheckBoxField)field).Clone();
                }
                else if (field is CommandButtonField)
                {
                    clone = ((CommandButtonField)field).Clone();
                }
                else if (field is LabelField)
                {
                    clone = ((LabelField)field).Clone();
                }
                else if (field is OptionField)
                {
                    clone = ((OptionField)field).Clone();
                }
                else if (field is RelatedViewField)
                {
                    clone = ((RelatedViewField)field).Clone();
                }
                else if (field is GroupField)
                {
                    clone = ((GroupField)field).Clone();
                }
                else if( field == null)
                {
                    clone = null;
                }
                
                return clone;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void PanelFieldUpdate(Field field)
        {
            #region InputValidation
            if (field == null)
            {
                throw new ArgumentNullException("PanelFieldUpdate");
            }
            #endregion
            
            RemoveFieldControlFromPanel(field);
            AddControlsToPanel(ControlFactory.Instance.GetFieldControls(field, canvas.PagePanel.Size));
            PersistChildFieldNames();
            //--147
             SetBackColorForGroupCheckBoxes(true);
            //---
        }

        private void RemoveFieldControlFromPanel(Field field)
        {
            #region InputValidation
            if (field == null)
            {
                throw new ArgumentNullException("RemoveFieldControlFromPanel");
            }
            #endregion
            
            Panel panel = canvas.PagePanel;
            IFieldControl fieldControl = null;

            foreach (Control control in panel.Controls)
            {
                if (control is IFieldControl)
                {
                    if (((IFieldControl)control).Field.UniqueId == field.UniqueId)
                    {
                        fieldControl = control as IFieldControl;
                        break;
                    }
                }
            }

            if (fieldControl != null)
            {
                if (fieldControl is PairedLabel)
                {
                    if (((PairedLabel)fieldControl).LabelFor != null)
                    {
                        panel.Controls.Remove(((PairedLabel)fieldControl).LabelFor);
                        ((PairedLabel)fieldControl).LabelFor.Dispose();
                    }
                    panel.Controls.Remove((Control)fieldControl);
                    ((Control)fieldControl).Dispose();
                }
                else if (fieldControl.Field.FieldType == MetaFieldType.Option)
                {
                    panel.Controls.Remove(((Control)fieldControl));
                    ((Control)fieldControl).Dispose();
                }
                else
                {
                    Control prompt = GetPromptControl(fieldControl.Field.Name);
                    if (prompt != null)
                    {
                        panel.Controls.Remove(prompt);
                        prompt.Dispose();
                    }
                    panel.Controls.Remove(((Control)fieldControl));
                    ((Control)fieldControl).Dispose();
                }
            }
            canvas.DisposeControlTrackers();
        }

        # region Print

        public void Print(int pageNumberStart, int pageNumberEnd)
        {
            if (ProcessPrintRequest( pageNumberStart,  pageNumberEnd))
            {
                printDocument.Print();
            }
            else
            {
                MessageBox.Show("The selected range and number of pages is too much to print at one time.");
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
            }
        }

           /// <summary>
        /// Handles the Print Page event of the document
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void printDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(((Bitmap)pageImageList[currentPrintPage++]), 0, 0);

            if (currentPrintPage < pageImageList.Count)
            {
                e.HasMorePages = true;
            }
            else
            {
                e.HasMorePages = false;
                currentPrintPage = 0;
            }
        }

        private bool ProcessPrintRequest(int pageNumberStart, int pageNumberEnd)
        {
            bool isLandscape = false;           
            currentPrintPage = 0;
            pageImageList = new ArrayList();
            View view = this.ProjectExplorer.SelectedPage.GetView();
            Page currentPage=this.ProjectExplorer.currentPage;
               try
               {
                   List<Page> allPages = view.GetMetadata().GetViewPages(view);
                   List<Page> pages = new List<Page>();
                   pages = allPages.GetRange(pageNumberStart - 1, 1 + pageNumberEnd - pageNumberStart);

                   foreach (Page page in pages)
                   {
                       printDocument = new System.Drawing.Printing.PrintDocument();
                       printDocument.PrintPage += new PrintPageEventHandler(printDocument_PrintPage);

                       DataRow row = page.GetMetadata().GetPageSetupData(view);                      
                       isLandscape = row["Orientation"].ToString().ToLowerInvariant() == "landscape";
                       Panel printPanel = new Panel();

                       float dpiX;
                       Graphics graphics = printPanel.CreateGraphics();
                       dpiX = graphics.DpiX;

                       int height = (int)row["Height"];
                       int width = (int)row["Width"];

                       if (dpiX != 96)
                       {
                           float scaleFactor = (dpiX * 1.041666666f) / 100;
                           height = Convert.ToInt32(((float)height) * (float)scaleFactor);
                           width = Convert.ToInt32(((float)width) * (float)scaleFactor);
                       }

                       if (isLandscape)
                       {
                           printPanel.Size = new System.Drawing.Size(height, width);
                           printDocument.DefaultPageSettings.Landscape = true;
                       }
                       else
                       {
                           printPanel.Size = new System.Drawing.Size(width, height);
                       }

                       ControlFactory factory = ControlFactory.Instance;
                       List<Control> pageControls = factory.GetPageControls(page, printPanel.Size);

                       SortedList<IFieldControl, System.Drawing.Point> groupsOnPage = new SortedList<IFieldControl, System.Drawing.Point>(new GroupZeeOrderComparer());
                       List<Control> childrenOnPage = new List<Control>();
                       List<Control> orphansOnPage = new List<Control>();
                       ArrayList namesOfChildenOnPage = new ArrayList();

                       Panel panel = canvas.PagePanel;
                       
                       foreach (Control control in pageControls)
                       {
                           if (control is GroupBox)
                           {
                               control.SendToBack();
                               printPanel.Controls.Add(control);
                           }
                       }

                       foreach (Control control in pageControls)
                       {
                           if (control is GroupBox) continue;
                           
                           Control printControl = control;
                           Field field = factory.GetAssociatedField(control);
                           field = view.Fields[field.Name];

                            try
                            {
                                if (control is RichTextBox || control is System.Windows.Forms.PictureBox)
                                {
                                    printControl = new TextBox();

                                    if (control is TextBoxBase)
                                    {
                                        ((TextBoxBase)printControl).BorderStyle = ((TextBoxBase)control).BorderStyle;
                                    }
                                    
                                    ((TextBoxBase)printControl).Multiline = true;

                                    printControl.Left = control.Left;
                                    printControl.Top = control.Top;
                                    printControl.Height = control.Height;
                                    printControl.Width = control.Width;
                                    printControl.Font = control.Font;

                                }

                                printPanel.Controls.Add(printControl);
                            }
                            catch
                            {
                                return false;
                            }
 
                           if (printControl is Label) continue;
                          
                           if (printControl is TextBox || printControl is ComboBox)
                           {
                               if (field is IDataField || printControl is DataGridView)
                               {
                                   if (printControl is ComboBox)
                                   {
                                       ((ComboBox)printControl).Text = "";
                                   }
                               }

                               if (printControl is TextBox)
                               {
                                   ((TextBox)printControl).Text = "";
                                   ((TextBox)printControl).Enabled = true;
                               }
                           }                           
                       }

                       int colorValue;
                       DataTable backgroundTable = page.GetMetadata().GetPageBackgroundData(page);
                       DataRow[] rows = backgroundTable.Select("BackgroundId=" + page.BackgroundId);
                       string imageLayout = string.Empty;
                       if (rows.Length > 0)
                       {
                           imageLayout = Convert.ToString(rows[0]["ImageLayout"]);
                       }
                       Color color;
                       Image image;
                       Bitmap bufferBitmap = null;

                       if (page != null)
                       {
                           if (rows.Length > 0)
                           {
                               colorValue = Convert.ToInt32(rows[0]["Color"]);

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

                                           image = Image.FromStream(((Stream)memStream));
                                       }
                                   }
                                   catch
                                   {
                                       image = null;
                                   }
                               }
                               else
                               {
                                   image = null;
                               }

                               if (colorValue == 0)
                               {
                                   color = SystemColors.Window;
                               }
                               else
                               {
                                   color = Color.FromArgb(colorValue);
                               }
                           }
                           else
                           {
                               image = null;
                               imageLayout = "None";
                               color = SystemColors.Window;
                           }
                       }
                       else
                       {
                           image = null;
                           imageLayout = "None";
                           color = SystemColors.Window;
                       }

                       if ((image == null) && (color.Equals(Color.Empty)))
                       {
                           printPanel.BackColor = Color.White;
                       }
                       else
                       {
                           if (!isShowFieldName && !isShowTabOrder)
                           {
                           }
                           else
                           {
                               foreach (Control control in pageControls)
                               {
                                   Field field = factory.GetAssociatedField(control);
                                   if (control.TabStop == true)
                                   {
                                       bool isInputField = ((Control)control) is PairedLabel == false ;
                                       bool isLabelField = field.FieldType == MetaFieldType.LabelTitle;
                                       if (isInputField || isLabelField)
                                       {
                                           Label lbTabSquare = new Label();
                                           lbTabSquare.BackColor = control.TabStop ? Color.Black : Color.Firebrick;
                                           lbTabSquare.Padding = new Padding(2);
                                           lbTabSquare.ForeColor = Color.White;
                                           lbTabSquare.BorderStyle = BorderStyle.None;
                                           lbTabSquare.Font = new Font(FontFamily.GenericSansSerif, 7, FontStyle.Bold);
                                           if (isShowFieldName && isShowTabOrder)
                                               lbTabSquare.Text = control.TabIndex.ToString() + "  " + field.Name;
                                           else if (isShowFieldName && !isShowTabOrder)
                                               lbTabSquare.Text = field.Name;
                                           else if (!isShowFieldName && isShowTabOrder)
                                               lbTabSquare.Text = control.TabIndex.ToString();                                         
                                           lbTabSquare.Size = TextRenderer.MeasureText(lbTabSquare.Text, lbTabSquare.Font);
                                           lbTabSquare.Size = new Size(lbTabSquare.Size.Width + lbTabSquare.Padding.Size.Width, lbTabSquare.Size.Height + lbTabSquare.Padding.Size.Height);
                                           
                                           if(control is GroupBox)
                                           {
                                               lbTabSquare.Location = new Point(control.Location.X, control.Location.Y + lbTabSquare.Size.Height);
                                           }
                                           else
                                           {
                                               lbTabSquare.Location = new Point(control.Location.X, control.Location.Y);
                                           }

                                           lbTabSquare.Tag = "showtab";
                                           lbTabSquare.BringToFront();
                                           printPanel.Controls.Add(lbTabSquare);
                                       }
                                   }
                                   else
                                   {
                                       bool isInputField = ((Control)control) is PairedLabel == false ;
                                       bool isLabelField = field.FieldType == MetaFieldType.LabelTitle;
                                       
                                       if (isInputField || isLabelField)
                                       {
                                           Label lbTabSquare = new Label();
                                           //lbTabSquare.Paint+=new PaintEventHandler(lbTabSquare_Paint);
                                           lbTabSquare.BackColor = Color.Firebrick;// control.TabStop ? Color.Firebrick : Color.Black;
                                           lbTabSquare.Padding = new Padding(2);
                                           lbTabSquare.ForeColor = Color.White;
                                           lbTabSquare.BorderStyle = BorderStyle.None;
                                           lbTabSquare.Font = new Font(FontFamily.GenericSansSerif, 7, FontStyle.Bold);
                                           if (isShowFieldName && isShowTabOrder)
                                               lbTabSquare.Text = control.TabIndex.ToString() + "  " + field.Name;
                                           else if (isShowFieldName && !isShowTabOrder)
                                               lbTabSquare.Text = field.Name;
                                           else if (!isShowFieldName && isShowTabOrder)
                                               lbTabSquare.Text = control.TabIndex.ToString();                                          

                                           lbTabSquare.Size = TextRenderer.MeasureText(lbTabSquare.Text, lbTabSquare.Font);
                                           lbTabSquare.Size = new Size(lbTabSquare.Size.Width + lbTabSquare.Padding.Size.Width, lbTabSquare.Size.Height + lbTabSquare.Padding.Size.Height);

                                           if (control is GroupBox)
                                           {
                                               lbTabSquare.Location = new Point(control.Location.X, control.Location.Y + lbTabSquare.Size.Height);
                                           }
                                           else
                                           {
                                               lbTabSquare.Location = new Point(control.Location.X, control.Location.Y);
                                           }

                                           lbTabSquare.BringToFront();
                                           printPanel.Controls.Add(lbTabSquare);
                                       }
                                   }
                               }
                           }                          

                           printPanel.BackgroundImageLayout = ImageLayout.None;
                           
                           if (printPanel.Size.Width > 0 && printPanel.Size.Height > 0)
                           {
                               try
                               {                                   
                                   Bitmap b = new Bitmap(printPanel.Size.Width, printPanel.Size.Height);                                   
                                   Graphics bufferGraphics = Graphics.FromImage(b);

                                   if (!(color.Equals(Color.Empty)))
                                   {
                                       printPanel.BackColor = color;
                                   }
                                   
                                   bufferGraphics.Clear(printPanel.BackColor);

                                   if (image != null)
                                   {
                                       Image img = image;
                                       switch (imageLayout.ToUpperInvariant())
                                       {
                                           case "TILE":
                                               TextureBrush tileBrush = new TextureBrush(img, System.Drawing.Drawing2D.WrapMode.Tile);
                                               bufferGraphics.FillRectangle(tileBrush, 0, 0, printPanel.Size.Width, printPanel.Size.Height);
                                               tileBrush.Dispose();
                                               break;

                                           case "STRETCH":
                                               bufferGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                               bufferGraphics.DrawImage(img, 0, 0, printPanel.Size.Width, printPanel.Size.Height);
                                               bufferGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
                                               break;

                                           case "CENTER":
                                               int centerX = (printPanel.Size.Width / 2) - (img.Size.Width / 2);
                                               int centerY = (printPanel.Size.Height / 2) - (img.Size.Height / 2);
                                               bufferGraphics.DrawImage(img, centerX, centerY);
                                               break;

                                           default:
                                               bufferGraphics.DrawImage(img, 0, 0);
                                               break;
                                       }
                                   }
                                  
                                   bufferGraphics.DrawImage(b, 0, 0);
                                   bufferBitmap = b;
                                   printPanel.BackgroundImage = b;
                               }
                               catch
                               {
                                   bufferBitmap.Dispose();
                                   graphics.Dispose();
                                   printPanel.Dispose();
                                   pageImageList = null;
                                   return false;
                               }
                           }
                       }

                       if (bufferBitmap != null)
                       {
                           graphics.DrawImage(bufferBitmap, 0, 0);
                       }

                       try
                       {
                            Bitmap b1 = new Bitmap(printPanel.Size.Width, printPanel.Size.Height);
                           DrawGrid(b1);
                           printPanel.BackgroundImage = b1;
                           memoryImage = new Bitmap(printPanel.Width, printPanel.Height, graphics);
                           printPanel.DrawToBitmap(memoryImage, new Rectangle(0, 0, printPanel.Width, printPanel.Height));
                           printPanel.Dispose();
                           pageImageList.Add(memoryImage.Clone());
                       }
                       catch
                       {
                           bufferBitmap.Dispose();
                           graphics.Dispose();
                           printPanel.Dispose();
                           pageImageList = null;
                           memoryImage.Dispose();
                           return false;
                       }
                   }
               }

               catch
               {
                   memoryImage.Dispose();
                   pageImageList = null;
                   return false;
               };

            return true;
        }

        private Configuration Config = Configuration.GetNewInstance();

        private void DrawGrid(Bitmap b)
        {           
            for (int y = 0; y < b.Height; y += (Config.Settings.GridSize + 1) * Epi.Constants.GRID_FACTOR)
            {
                for (int x = 0; x < b.Width; x += (Config.Settings.GridSize + 1) * Epi.Constants.GRID_FACTOR)
                {
                    b.SetPixel(x, y, Color.Silver);
                }
            }
        }

        void lbTabSquare_Paint(object sender, PaintEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    #region ControlTopComparer
    class ControlTopComparer : IComparer<IFieldControl>
    {
        public int Compare(IFieldControl x, IFieldControl y)
        {
            if (((Control)x).Top > ((Control)y).Top + 2)
            {
                return 1;
            }
            else if (((Control)x).Top >= ((Control)y).Top) 
            {
                if ((((Control)x).Left < ((Control)y).Left))
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                return -1;
            } 
        }
    }

    class ControlLeftComparer : IComparer<IFieldControl>
    {
        public int Compare(IFieldControl x, IFieldControl y)
        {
            if (((Control)x).Left > ((Control)y).Left + 2)
            {
                return 1;
            }
            else if (((Control)x).Left >= ((Control)y).Left)
            {
                if ((((Control)x).Top < ((Control)y).Top))
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                return -1;
            }
        }
    }

    #endregion

    #region GroupZeeOrderComparer
    class GroupZeeOrderComparer : IComparer<IFieldControl>
    {
        public int Compare(IFieldControl x, IFieldControl y)
        {
            if (((Control)x).Top < ((Control)y).Top + 2)
            {
                return 1;
            }
            else if (((Control)x).Top <= ((Control)y).Top)
            {
                if ((((Control)x).Left > ((Control)y).Left))
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                return -1;
            }
        }
    }
    #endregion
    
    #region BackupField
    class BackupField
    {
        public Field fieldBefore;
        public Field fieldAfter;
        private String actionDescription;
        private Int64 ticks;
        private BackupFieldAction backupFieldAction; 

        public BackupField(Field fieldBefore, Field fieldAfter, BackupFieldAction action)
        {
            this.fieldBefore = fieldBefore;
            this.fieldAfter = fieldAfter;
            switch (action)
            {
                case BackupFieldAction.Change:
                    this.actionDescription = "Edit";
                    break;
                case BackupFieldAction.Delete:
                    this.actionDescription = SharedStrings.DELETE;
                    break;
                case BackupFieldAction.New:
                    this.actionDescription = "New";
                    break;
                case BackupFieldAction.Paste:
                    this.actionDescription = "Paste";
                    break;
            }
            
            this.backupFieldAction = action;
            this.ticks = DateTime.Now.Ticks;
        }

        public Field Field(BackupFieldAction backupFieldAction)
        {
            switch (backupFieldAction)
            {
                case BackupFieldAction.UndoChange:
                case BackupFieldAction.UndoDelete:
                case BackupFieldAction.RedoDelete:
                    return fieldBefore;

                case BackupFieldAction.UndoNew:
                case BackupFieldAction.RedoNew:
                case BackupFieldAction.UndoPaste:
                case BackupFieldAction.RedoPaste:
                case BackupFieldAction.RedoChange:
                default:
                    return fieldAfter;
            }
        }
        public BackupFieldAction BackupAction { get { return backupFieldAction; } }
        public String ActionDescription { get { return actionDescription; } }
        public Int64 Ticks { get { return ticks; } }
    }

    class BackupFieldComparer : IComparer<Int64>
    {
        public int Compare(Int64 x, Int64 y)
        {
            return x.CompareTo(y);
        }
    }

    class BackupFieldReverseComparer : IComparer<Int64>
    {
        public int Compare(Int64 x, Int64 y)
        {
            return y.CompareTo(x);
        }
    }

    public enum BackupFieldAction
    { 
        New,
        Paste,
        Delete,
        Change,
        UndoNew,
        UndoPaste,
        UndoDelete,
        UndoChange,
        RedoNew,
        RedoPaste,
        RedoDelete,
        RedoChange
    }
    #endregion

  
}
