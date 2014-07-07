#region Namespaces

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Epi.Windows.Controls;

#endregion  //Namespaces

namespace Epi.Windows.MakeView.Forms
{
    /// <summary>
    /// The MakeView WYSIWYG canvas
    /// </summary>
    public partial class Canvas : Epi.Windows.Docking.DockWindow
    {
        #region Public Events

        /// <summary>
        /// 
        /// </summary>
        public event ShowTabOrderEventHandler ShowTabOrderSelected;
        /// <summary>
        /// 
        /// </summary>
        public event StartNewTabOrderEventHandler StartNewTabOrderSelected;
        /// <summary>
        /// 
        /// </summary>
        public event ContinueTabOrderEventHandler ContinueTabOrderSelected;
        /// <summary>
        /// Occurs when a control is resized
        /// </summary>
        public event ControlMovementEventHandler ControlResized;
        /// <summary>
        /// Occurs when a control is moved
        /// </summary>
        public event ControlMovementEventHandler ControlMoved;
        /// <summary>
        /// Occurs when a prompt is moved
        /// </summary>
        public event ControlMovementEventHandler PromptMoved;
        /// <summary>
        /// Occurs when a field creation request is made
        /// </summary>
        public event FieldCreationRequestEventHandler FieldCreationRequested;
        /// <summary>
        /// Occurs when a group creation request is made
        /// </summary>
        public event GroupCreationRequestEventHandler GroupCreationRequested;
        /// <summary>
        /// Occurs when a tree node is dropped on the canvas
        /// </summary>
        public event TreeNodeDragDropEventHandler TreeNodeDropped;
        /// <summary>
        /// Occurs when a page's check code is requested
        /// </summary>
        public event PageCheckCodeRequestEventHandler PageCheckCodeRequested;
        /// <summary>
        /// Occurs when the selected panel area changes on mouse up
        /// </summary>
        public event SelectedControlsChangedEventHandler SelectedControlsChanged;
        /// <summary>
        /// Occurs when the Cut menu item is selected
        /// </summary>
        public event CutFieldControlEventHandler CutFieldControl;
        /// <summary>
        /// Occurs when the Delete menu item is selected
        /// </summary>
        public event DeleteFieldControlEventHandler DeleteFieldControl;
        /// <summary>
        /// Occurs when the AsignAsRow menu item is selected
        /// </summary>
        public event TableAlignEventHandler TableAlign;
        /// <summary>
        /// Occurs when the AsignAsRow menu item is selected
        /// </summary>
        public event AlignAsRowEventHandler AlignAsRow;
        /// <summary>
        /// Occurs when a MakeSame... menu item is selected
        /// </summary>
        public event MakeSameEventHandler MakeSame;
        /// <summary>
        /// Occurs when the ...CreateTemplate... is selected
        /// </summary>
        public event CreateTemplateEventHandler CreateTemplate;
        /// <summary>
        /// Occurs when the Copy menu item is selected
        /// </summary>
        public event CopyFieldControlEventHandler CopyFieldControl;
        /// <summary>
        /// Occurs when the Paste menu item is selected
        /// </summary>
        public event PasteFieldControlEventHandler PasteFieldControl;

        public event ApplyDefaultFontsEventHandler ApplyDefaultFonts;

        #endregion  //Public Events

        #region Private Members

        private Epi.Windows.Controls.ControlTracker controlTracker;

        private Point rightClickLocation;
        private Point lastMouseLocation;
        private Point selectionStartLocation;
        private Rectangle groupFieldOutline = Rectangle.Empty;
        private bool updateGroupFieldOutline = false;
        private bool openForViewingOnly = false;
        private List<Control> tabIndexIndicators;
        private Graphics bufferGraphics;
        private Bitmap bufferBitmap;
        private Configuration config;
        private MakeViewMainForm makeViewForm;
        private WaitPanel waitPanel;
        private int hideUpdateCallCount = 0;
        private TemplateFootprint lassoFootprint = null;
        private TemplateFootprint selectedFieldsFootprint = null;
        private Point _footprintMouseOffset = new Point();
        private Point scrollLocationAtDrop;

        public Point ScrollLocationAtDrop
        {
            get { return scrollLocationAtDrop; }
            set { scrollLocationAtDrop = value; }
        }

        public TemplateFootprint LassoFootprint
        {
            get { return lassoFootprint; }
            set 
            {
                if (value == null)
                {
                    this.PagePanel.Controls.Remove(lassoFootprint);
                    lassoFootprint = null;
                }
                else
                {
                    lassoFootprint = value;
                    this.PagePanel.Controls.Add(lassoFootprint);
                }
            }
        }

        public TemplateFootprint SelectedFieldsFootprint
        {
            get { return selectedFieldsFootprint; }
            set
            {
                if (value == null)
                {
                    this.PagePanel.Controls.Remove(selectedFieldsFootprint);
                    selectedFieldsFootprint = null;
                }
                else
                {
                    selectedFieldsFootprint = value;
                    this.PagePanel.Controls.Add(selectedFieldsFootprint);
                }
            }
        }

        public Point FootprintMouseOffset
        {
            get { return _footprintMouseOffset; }
            set { _footprintMouseOffset = value; }
        }
    
        #endregion  //Private Members

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="frm">The main form</param>
        public Canvas(MainForm frm)
            : base(frm)
        {
            InitializeComponent();
            config = Configuration.GetNewInstance();
            
            makeViewForm = frm as MakeViewMainForm;
            makeViewForm.PageChanged += new EventHandler(makeViewForm_PageChanged);
            makeViewForm.GridSettingsChanged += new EventHandler(makeViewForm_GridSettingsChanged);

            waitPanel = new WaitPanel(PagePanel.Size);
            waitPanel.Hide();
            
            this.Controls.Add(waitPanel);
        }

        #endregion  //Constructors

        #region Public Properties
        
        /// <summary>
        /// EnableCutCopyMenuItem
        /// </summary>
        public bool EnableCutCopyMenuItem { get; set; }

        #endregion

        #region Public Methods
        public void ChangePagePanelProperitesForPage()
        {
            PagePanel.Name = makeViewForm.projectExplorer.currentPage.Name;
            PagePanel.Size = GetCurrentPageSize();
            SetCanvasDisplayProperties();
            EnablePagePanel(true);
        }

        public void UpdateHidePanel(string status)
        {
            if (waitPanel != null)
            {
                waitPanel.Update(status);
                waitPanel.BringToFront();
                waitPanel.Refresh();
            }
        }
        
        public void HideUpdateStart(string message)
        {
            hideUpdateCallCount++;

            if (PagePanel != null)
            {
                PagePanel.Hide();
                PagePanel.Refresh();
            }

            waitPanel.ShowPanel(message);
        }

        public void HideUpdateEnd()
        {
            hideUpdateCallCount = hideUpdateCallCount > 0 ? hideUpdateCallCount - 1 : 0 ;

            if (waitPanel != null && hideUpdateCallCount == 0)
            {
                waitPanel.HidePanel();
                waitPanel.Refresh();

                if (makeViewForm.mediator.SelectedFieldControls.Count > 0)
                {
                    RenderControlTrackers(makeViewForm.mediator.SelectedFieldControls);
                }

                pagePanel.Show();
                pagePanel.Refresh();
            }
        }

        public void HideUpdateEnd(bool forceHideUpdateEnd)
        {
            hideUpdateCallCount = 0;
            HideUpdateEnd();
        }

        /// <summary>
        /// Enable or disable panel
        /// </summary>
        /// <param name="enable">Boolean to enable or disable</param>        
        public void EnablePagePanel(bool enable)
        {
            PagePanel.Enabled = enable;
        }

        public void WaitPanelForceHide()
        {
            if (waitPanel != null)
            {
                waitPanel.Hide();
                waitPanel.Refresh();
                hideUpdateCallCount = 0;
            }
        }

        public void ShowPagePanel()
        {
            PagePanel.BringToFront();
            PagePanel.Show();
            PagePanel.Refresh();
            ((Epi.Windows.Docking.DockPanel)PagePanel.Parent).AutoScrollPosition = scrollLocationAtDrop;
        }
        
        public void DesignerFocus()
        {
            PagePanel.Focus();
        }

        private Size GetCurrentPageSize()
        {
            Size pageSize = new Size();
            pageSize.Height = makeViewForm.projectExplorer.currentPage.view.PageHeight;
            pageSize.Width = makeViewForm.projectExplorer.currentPage.view.PageWidth;
            return pageSize;
        }

        /// <summary>
        /// Reset the canvas to its initial state
        /// </summary>
        public void Reset()
        {
            this.Text = string.Empty;
            foreach (Control control in PagePanel.Controls)
            {
                PagePanel.Controls.Remove(control);
                control.Dispose();
            }

            GC.Collect();
            RedrawCanvasBackground();
        }

        /// <summary>
        /// RemoveControls
        /// </summary>
        /// <param name="fieldName">field name</param>
        public void RemoveControls(string fieldName)
        {
            
            foreach (Control control in PagePanel.Controls)
            {
                if (control is IFieldControl)
                {
                    if (((IFieldControl)control).Field.Name.Equals(fieldName))
                    {
                        ((IFieldControl)control).Field.Delete();
                        control.Dispose();
                    }
                }
            }

            GC.Collect();
        }

        public void SetCanvasDisplayProperties()
        {
            SetCanvasDisplayProperties(this.makeViewForm.projectExplorer.SelectedPage);
        }
        
        /// <summary>
        /// Sets the location and sized of the panel designer on the main form canvas.
        /// </summary>
        /// <param name="page"></param>
        /// <returns>The size of the panel designer.</returns>
        public Size SetCanvasDisplayProperties(Page page)
        {
            View view;

            if (page != null)
            {
                view = page.GetView();
            }
            else
            {
                return new Size();
            }

            if (page.GetView().Project == null)
            {
                return new Size();
            }

            DataRow row = view.Project.Metadata.GetPageSetupData(view);
            
            string orientation;
            int height;
            int width;

            if (row["Orientation"] is System.DBNull || row["Width"] is System.DBNull || row["Height"] is System.DBNull)
            {
                orientation = (string)config.Settings["DefaultPageOrientation"];
                height = (int)config.Settings["DefaultPageHeight"];
                width = (int)config.Settings["DefaultPageWidth"];
            }
            else
            {
                orientation = row["Orientation"].ToString();
                width = (int)row["Width"];
                height = (int)row["Height"];
            }

            float dpiX;
            Graphics graphics = this.CreateGraphics();
            dpiX = graphics.DpiX; 

            if (dpiX != 96)
            {
                float scaleFactor = (dpiX * 1.041666666f) / 100;
                height = Convert.ToInt32(((float)height) * (float)scaleFactor);
                width = Convert.ToInt32(((float)width) * (float)scaleFactor);
            }

            if (orientation.ToLower() == "landscape")
            {
                PagePanel.Size = new Size(height, width);
            }
            else
            {
                PagePanel.Size = new Size(width, height);
            }

            int darkControlWidth = makeViewForm.Size.Width - makeViewForm.projectExplorer.Size.Width;
            int leftMargin = (darkControlWidth - PagePanel.Size.Width) / 2;
            leftMargin = leftMargin < 8 ? 8 : leftMargin;
            int topMargin = 0;

            PagePanel.Location = new Point(leftMargin, topMargin);    

            return PagePanel.Size;
        }

        public void RenderControlTracker(Control control, bool showAsResizeable)
        {
            if (((IFieldControl)control).Tracker == null)
            {
                ((IFieldControl)control).Tracker = new ControlTracker(control, true);
                PagePanel.Controls.Add(((IFieldControl)control).Tracker);
                ((IFieldControl)control).Tracker.MouseEnter += controlTracker_MouseEnter;
                ((IFieldControl)control).Tracker.MouseLeave += controlTracker_MouseLeave;
                ((IFieldControl)control).Tracker.Resized += controlTracker_Resized;
                ((IFieldControl)control).Tracker.BringToFront();
            }

            ((IFieldControl)control).TrackerStatus = showAsResizeable ? 
                Epi.Windows.Enums.TrackerStatus.Resize:
                Epi.Windows.Enums.TrackerStatus.Selected;
        }

        public void RenderControlTracker(PairedLabel label)
        {
            if (((IFieldControl)label).Tracker == null)
            {
                ((IFieldControl)label).Tracker = new ControlTracker(label, false);
                PagePanel.Controls.Add(((IFieldControl)label).Tracker);
                ((IFieldControl)label).Tracker.MouseEnter += controlTracker_MouseEnter;
                ((IFieldControl)label).Tracker.MouseLeave += controlTracker_MouseLeave;
                ((IFieldControl)label).Tracker.Resized += controlTracker_Resized;
                ((IFieldControl)label).Tracker.BringToFront();
            }
            
            ((IFieldControl)label).TrackerStatus = Epi.Windows.Enums.TrackerStatus.Selected;

            if (((IFieldControl)label.LabelFor).Tracker == null)
            {
                ((IFieldControl)label.LabelFor).Tracker = new ControlTracker(label.LabelFor, false);
                PagePanel.Controls.Add(((IFieldControl)label.LabelFor).Tracker);
                ((IFieldControl)label.LabelFor).Tracker.MouseEnter += controlTracker_MouseEnter;
                ((IFieldControl)label.LabelFor).Tracker.MouseLeave += controlTracker_MouseLeave;
                ((IFieldControl)label.LabelFor).Tracker.Resized += controlTracker_Resized;
                ((IFieldControl)label.LabelFor).Tracker.BringToFront();
            }
            ((IFieldControl)label.LabelFor).TrackerStatus = Epi.Windows.Enums.TrackerStatus.Selected;
        }
        
        /// <summary>
        /// Renders the list of control trackers.
        /// </summary>
        /// <param name="controls">The control that will get a tracker</param>
        public void RenderControlTrackers(SortedDictionary<IFieldControl,Point> controls)
        {
            foreach (KeyValuePair<IFieldControl, Point> kvp in controls)
            {
                RenderControlTracker((Control)kvp.Key, false);
            }
        }

        void controlTracker_MouseEnter(object sender, EventArgs e)
        {
            //((Control)sender).MouseEnter += new EventHandler(pagePanel_MouseEnter);
        }

        void controlTracker_Resized(object sender, EventArgs e)
        {
            if (ControlResized != null)
            {
                ControlResized(this, new FieldControlEventArgs((IFieldControl)sender));
            }
        }

        void controlTracker_MouseLeave(object sender, EventArgs e)
        {
        }

        void pagePanel_MouseEnter(object sender, System.EventArgs e)
        {
            if (OpenForViewingOnly)
            {
                return;
            }

            if (makeViewForm.mediator.SelectedFieldControls.Count <= 0)
            {
                DisposeControlTrackers();
                RenderControlTrackers(makeViewForm.mediator.SelectedFieldControls);
            }
            mainForm.UpdateStatus(SharedStrings.READY, false);
        }
    
        /// <summary>
        /// Disposes the current control tracker
        /// </summary>
        public void DisposeControlTrackers()
        {
            object[] pagePanelControls = new object[PagePanel.Controls.Count];
            PagePanel.Controls.CopyTo(pagePanelControls, 0);

            for (int i = 0; i < pagePanelControls.Length; i++)
            {
                if (pagePanelControls[i] is ControlTracker)
                {
                    ((ControlTracker)pagePanelControls[i]).TrackerStatus = Epi.Windows.Enums.TrackerStatus.NotSelected;
                }
            }
        }
        
        /// <summary>
        /// On Settings Saved
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        public void OnSettingsSaved(object sender, EventArgs e)
        {
            makeViewForm.ChangeBackgroundData();
            makeViewForm.mediator.LoadPage(makeViewForm.CurrentPage);
            RedrawCanvasBackground();
        }

        private void RedrawCanvasBackground()
        {
            if (makeViewForm != null)
            {
                config = Configuration.GetNewInstance();
                SaveSnapshot();
                DrawPicture(bufferGraphics);
            }
        }
        #endregion  //Public Methods

        #region Event Handlers

        void makeViewForm_PageChanged(object sender, EventArgs e)
        {
            this.RedrawCanvasBackground();
        }

        void makeViewForm_GridSettingsChanged(object sender, EventArgs e)
        {
            this.RedrawCanvasBackground();
        }

        private void PagePanel_Resize(object sender, EventArgs e)
        {
            this.RedrawCanvasBackground();
        }
        
        /// <summary>
        /// Handles the Mouse Down event of the panel
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void panel_MouseDown(object sender, MouseEventArgs e)
        {
            if (OpenForViewingOnly)
            {
                return;
            }

            if (e.Button == MouseButtons.Right)
            {
                rightClickLocation = e.Location;
                BuildFieldDefinitionContextMenu().Show((Control)sender, e.Location);
            }
            else if (e.Button == MouseButtons.Left)
            {
                DisposeControlTrackers();
                groupFieldOutline = Rectangle.Empty;
                selectionStartLocation = e.Location;
                updateGroupFieldOutline = true;
                this.Focus();
            }

            RemoveTabIndexIndicators();
            Refresh();
        }

        void panel_MouseUp(object sender, MouseEventArgs e)
        {
            if (OpenForViewingOnly)
            {
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                LassoFootprint = null;

                makeViewForm.mediator.HandleNewSelection(GroupFieldOutline);

                if (makeViewForm.mediator.SelectedFieldControls.Count != 0)
                {
                    if (updateGroupFieldOutline == false)
                    {
                        makeViewForm.mediator.LoadPage(makeViewForm.projectExplorer.currentPage);
                        SelectedFieldsFootprint = null;
                    }
                }

                updateGroupFieldOutline = false;
            }
        }

        void panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (OpenForViewingOnly)
            {
                return;
            }

            if (lastMouseLocation != e.Location)
            {
                if (updateGroupFieldOutline)
                {
                    if (LassoFootprint == null)
                    {
                        LassoFootprint = new TemplateFootprint();
                    }

                    if (e.X < selectionStartLocation.X)
                    {
                        groupFieldOutline.X = e.X;
                        groupFieldOutline.Width = selectionStartLocation.X - e.X;
                    }
                    else
                    {
                        groupFieldOutline.X = selectionStartLocation.X;
                        groupFieldOutline.Width = e.X - selectionStartLocation.X;
                    }

                    if (e.Y < selectionStartLocation.Y)
                    {
                        groupFieldOutline.Y = e.Y;
                        groupFieldOutline.Height = selectionStartLocation.Y - e.Y;
                    }
                    else
                    {
                        groupFieldOutline.Y = selectionStartLocation.Y;
                        groupFieldOutline.Height = e.Y - selectionStartLocation.Y;
                    }

                    if (LassoFootprint != null && LassoFootprint.IsDisposed == false)
                    {
                        LassoFootprint.SizeTo(groupFieldOutline);
                    }
                }
            }
            
            lastMouseLocation = e.Location;
        }

        private void MoveFieldsOnDrop()
        {
            if (OpenForViewingOnly)
            {
                return;
            }

            HideUpdateStart(SharedStrings.START_FIELD_DROP);

            Point dropOnPoint = CalcAcceptableFootprintLocation(
                PagePanel.PointToClient(MousePosition)
                );

            foreach (KeyValuePair<IFieldControl, Point> kvp in makeViewForm.mediator.SelectedFieldControls)
            {
                IFieldControl control = kvp.Key;

                int left = kvp.Value.X + dropOnPoint.X + scrollLocationAtDrop.X;
                ((Control)control).Left = left;

                int top = kvp.Value.Y + dropOnPoint.Y + scrollLocationAtDrop.Y;
                ((Control)control).Top = top;

                ((Control)control).Visible = true;
            }

            makeViewForm.mediator.PersistSelectedFieldControlCollection();
            makeViewForm.mediator.InitialSelectedControlLocations.Clear();

            DisposeControlTrackers();
            makeViewForm.mediator.SelectedFieldControls.Clear();
        }

        /// <summary>
        /// Handles the Drag Over event of the panel 
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void PagePanel_DragOver(object sender, DragEventArgs e)
        {
            if (OpenForViewingOnly)
            {
                return;
            }

            if (e.Data.GetData("DragControl") != null)
            {
                bool dragControlIsSelected = false;
                e.Effect = DragDropEffects.Move;
                IFieldControl drgControl = (IFieldControl)e.Data.GetData("DragControl");
                Rectangle rec = new Rectangle(e.X - ((IDragable)drgControl).XOffset, e.Y - ((IDragable)drgControl).YOffset, ((Control)drgControl).Width, ((Control)drgControl).Height);
                rec = PagePanel.RectangleToClient(rec);
                ((Control)drgControl).Location = rec.Location;
                ((Control)drgControl).Size = rec.Size;

                SortedDictionary<IFieldControl, Point> selectedFieldControls = makeViewForm.mediator.SelectedFieldControls;
                DisposeControlTrackers();

                if (selectedFieldControls.Count != 0)
                {
                    foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
                    {
                        if (((IFieldControl)kvp.Key).Field.UniqueId == ((IFieldControl)drgControl).Field.UniqueId)
                        {
                            dragControlIsSelected = true;
                            break;
                        }
                    }

                    if (dragControlIsSelected == false)
                    {
                        selectedFieldControls.Clear();
                    }
                }

                if (selectedFieldControls.Count == 0)
                {
                    if (drgControl is DragableLabel)
                    {
                        DragableLabel drgLabelControl = (DragableLabel)drgControl;
                        if (drgLabelControl.LabelFor != null)
                        {
                            Control ctrl = drgLabelControl.LabelFor;
                            ctrl.Top = rec.Top + drgLabelControl.VerticalDistanceToControl;
                            ctrl.Left = rec.Left + drgLabelControl.HorizontalDistanceToControl;
                        }
                    }
                    
                    if (drgControl is DragableGroupBox)
                    {
                        DragableGroupBox drgGroupControl = (DragableGroupBox)drgControl;
                        foreach (Control childControl in (drgGroupControl.ChildControls))
                        {
                            if (childControl is IFieldControl)
                            {
                                childControl.Top = rec.Top + drgGroupControl.GetVerticalDistanceTo(childControl);
                                childControl.Left = rec.Left + drgGroupControl.GetHorizontalDistanceTo(childControl);
                            }
                        }
                    }
                }
                else
                {
                    if (drgControl is IDragable)
                    {
                        if (SelectedFieldsFootprint == null)
                        {
                            int minOver = int.MaxValue, minDown = int.MaxValue, maxOver = int.MinValue, maxDown = int.MinValue;
                                    
                            foreach (KeyValuePair<IFieldControl, Point> kvp in selectedFieldControls)
                            {
                                ((Control)kvp.Key).Visible = false;

                                int dim = ((Control)kvp.Key).Location.X;
                                if (minOver >= dim) minOver = dim;

                                dim = ((Control)kvp.Key).Location.Y;
                                if (minDown >= dim) minDown = dim;

                                dim = ((Control)kvp.Key).Location.X + ((Control)kvp.Key).Width;
                                if (maxOver <= dim) maxOver = dim;

                                dim = ((Control)kvp.Key).Location.Y + ((Control)kvp.Key).Height;
                                if (maxDown <= dim) maxDown = dim;
                            }

                            Size size = new Size(maxOver - minOver, maxDown - minDown);
                            Rectangle rectangle = new Rectangle(MousePosition, size);
                            rectangle = PagePanel.RectangleToClient(rectangle);

                            SelectedFieldsFootprint = new TemplateFootprint(rectangle);
                            _footprintMouseOffset = PagePanel.PointToClient(new Point(e.X - minOver,  e.Y - minDown));
                        }

                        Point point = PagePanel.PointToClient(new Point(e.X, e.Y));
                        point = CalcAcceptableFootprintLocation(point);
                        SelectedFieldsFootprint.Left = point.X;
                        SelectedFieldsFootprint.Top = point.Y;
                    }
                }
            }
            else if (e.Data.GetData("DraggedTemplateNode") != null)
            {
                e.Effect = DragDropEffects.Move;
                object ob = e.Data.GetData("DraggedTemplateNode");

                if (ob is TemplateNode)
                {
                    Template template = new Template(makeViewForm.mediator);
                    if (Template.GetTemplateLevel((TemplateNode)ob) == Enums.TemplateLevel.Field)
                    {
                        Size size = template.GetFieldFootprint((TemplateNode)ob);
                        Rectangle rectangle = new Rectangle(MousePosition, size);
                        rectangle = PagePanel.RectangleToClient(rectangle);
                        if (SelectedFieldsFootprint == null)
                        {
                            SelectedFieldsFootprint = new TemplateFootprint(rectangle);
                        }

                        Point point = PagePanel.PointToClient(new Point(e.X, e.Y));
                        SelectedFieldsFootprint.Left = point.X;
                        SelectedFieldsFootprint.Top = point.Y;
                    }
                }
            }
            else if (e.Data.GetData("DraggedTreeNode") != null)
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private Point CalcAcceptableFootprintLocation(Point mouse)
        {
            Point returnPoint = new Point();
            Point footprintOriginCandidate = new Point();
            footprintOriginCandidate.X = mouse.X - _footprintMouseOffset.X;
            footprintOriginCandidate.Y = mouse.Y - _footprintMouseOffset.Y;

            if (footprintOriginCandidate.X >= this.pagePanel.Width - SelectedFieldsFootprint.Width)
                returnPoint.X = this.pagePanel.Width - SelectedFieldsFootprint.Width;
            else
                returnPoint.X = mouse.X - _footprintMouseOffset.X;

            if (footprintOriginCandidate.Y > this.pagePanel.Height - SelectedFieldsFootprint.Height)
                returnPoint.Y = this.pagePanel.Height - SelectedFieldsFootprint.Height;
            else
                returnPoint.Y = mouse.Y - _footprintMouseOffset.Y;
            return returnPoint;
        }

        /// <summary>
        /// Handles the Drag Drop event of the panel
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void PagePanel_DragDrop(object sender, DragEventArgs e)
        {
            if (OpenForViewingOnly)
            {
                return;
            }

            Point currentScrollLocation = ((Epi.Windows.Docking.DockPanel)PagePanel.Parent).AutoScrollPosition;
            scrollLocationAtDrop = new Point(currentScrollLocation.X * -1, currentScrollLocation.Y * -1);
            DisposeControlTrackers();

            try
            {
                if (e.Data.GetData("DraggedTreeNode") != null)
                {
                    if (TreeNodeDropped != null)
                    {
                        TreeNodeDropped((TreeValueNode)e.Data.GetData("DraggedTreeNode"), PagePanel, PagePanel.PointToClient(new Point(e.X, e.Y)));
                    }
                }
                else if (e.Data.GetData("DraggedTemplateNode") != null)
                {
                    if (TreeNodeDropped != null)
                    {
                        TreeNodeDropped((TreeValueNode)e.Data.GetData("DraggedTemplateNode"), PagePanel, PagePanel.PointToClient(new Point(e.X, e.Y)));
                    }
                }
                else if (e.Data.GetData("DragControl") != null)
                {
                    IFieldControl drgControl = (IFieldControl)e.Data.GetData("DragControl");

                    if (makeViewForm.mediator.SelectedFieldControls.Count == 0)
                    {
                        if (drgControl is DragableLabel)
                        {
                            if (((DragableLabel)drgControl).LabelFor != null)
                            {
                                if (PromptMoved != null)
                                {
                                    PromptMoved(this, new FieldControlEventArgs(drgControl));
                                }
                            }
                            else
                            {
                                if (ControlMoved != null)
                                {
                                    ControlMoved(this, new FieldControlEventArgs(drgControl));
                                }
                            }
                        }
                        else
                        {
                            if (ControlMoved != null)
                            {
                                ControlMoved(this, new FieldControlEventArgs(drgControl));
                            }
                        }
                    }
                    else
                    {
                        MoveFieldsOnDrop();        
                    }

                    makeViewForm.mediator.PersistChildFieldNames();
                    HideUpdateEnd();
                }
            }
            catch (Exception ex)
            {
                HideUpdateEnd(true);
                throw new System.ApplicationException(ex.Message, ex);
            }
            finally
            {
                if(((Epi.Windows.Docking.DockPanel)PagePanel.Parent).AutoScrollPosition != scrollLocationAtDrop)
                {
                    ((Epi.Windows.Docking.DockPanel)PagePanel.Parent).AutoScrollPosition = scrollLocationAtDrop;
                }
                SelectedFieldsFootprint = null;
            }
        }

        /// <summary>
        /// Handles the Click event of the group menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuGroup_Click(object sender, EventArgs e)
        {
            if (GroupCreationRequested != null)
            {
                GroupCreationRequested(PagePanel, groupFieldOutline);
            }
        }

        /// <summary>
        /// Handles the Click event of the GUID menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuGUID_Click(object sender, EventArgs e)
        {
            if (FieldCreationRequested != null)
            {
                FieldCreationRequested(this, MetaFieldType.GUID, PagePanel, rightClickLocation);
            }
        }        

        /// <summary>
        /// Handles the Click event of the Check Code for a page
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuPageCheckCode_Click(object sender, EventArgs e)
        {
            if (PageCheckCodeRequested != null)
            {
                PageCheckCodeRequested(PagePanel);
            }
        }
        /// <summary>
        /// Handles the Click event of the Check Code for a page
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuCut_Click(object sender, EventArgs e)
        {
            if (CutFieldControl != null)
            {
                CutFieldControl();
            }
        }
        /// <summary>
        /// Handles the Click event of the Check Code for a page
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuDelete_Click(object sender, EventArgs e)
        {
            if (CutFieldControl != null)
            {
                DeleteFieldControl();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        void mnuShowTabOrder_Click(object sender, EventArgs e)
        {
            ShowTabOrderSelected();
        }
        /// <summary>
        /// 
        /// </summary>
        void mnuStartNewTabOrder_Click(object sender, EventArgs e)
        {
            StartNewTabOrderSelected();
        }
        /// <summary>
        /// 
        /// </summary>
        void mnuContinueTabOrder_Click(object sender, EventArgs e)
        {
            ContinueTabOrderSelected();
        }

        void mnuAlignAsRow_Click(object sender, EventArgs e)
        {
            if (CutFieldControl != null)
            {
                AlignAsRow();
            }
        }

        void mnuMakeSameWidthMax_Click(object sender, EventArgs e)
        {
            MakeSame(Enums.MakeSame.Width_Use_Maximum);
        }

        void mnuMakeSameWidthMin_Click(object sender, EventArgs e)
        {
            MakeSame(Enums.MakeSame.Width_Use_Minimum);
        }

        void mnuMakeSameHeightMax_Click(object sender, EventArgs e)
        {
            MakeSame(Enums.MakeSame.Height_Use_Maximum);
        }

        void mnuMakeSameHeightMin_Click(object sender, EventArgs e)
        {
            MakeSame(Enums.MakeSame.Height_Use_Minimum);
        }

        void mnuMakeSameSizeMax_Click(object sender, EventArgs e)
        {
            MakeSame(Enums.MakeSame.Size_Use_Maximum);
        }

        void mnuMakeSameSizeMin_Click(object sender, EventArgs e)
        {
            MakeSame(Enums.MakeSame.Size_Use_Minimum);
        }

        void mnuAlignAsTable_Click(object sender, EventArgs e)
        {
            if (CutFieldControl != null)
            {
                int columnCount = (int)(((ToolStripMenuItem)sender).Tag);
                TableAlign(columnCount);
            }
        }

        void mnuCreateTemplate_Click(object sender, EventArgs e)
        {
            PresentationLogic.GuiMediator mediator = makeViewForm.GetMediator();
            mainForm.TemplateNode = "Fields";
            Dialogs.AddTemplateDialog dialog = new Epi.Windows.MakeView.Dialogs.AddTemplateDialog(makeViewForm);
            dialog.ShowDialog();
            if (dialog.DialogResult == DialogResult.OK)
            {
                CreateTemplate(dialog.TemplateName);
                mediator.ProjectExplorer.UpdateTemplates();
            }
        }

        void mnuFont_Click(object sender, EventArgs e)
        {
            ApplyDefaultFonts();
        }

        /// <summary>
        /// Handles the Click event of the Check Code for a page
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuCopy_Click(object sender, EventArgs e)
        {
            if (CopyFieldControl != null)
            {
                CopyFieldControl();
            }
        }
        /// <summary>
        /// Handles the Click event of the Check Code for a page
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuPaste_Click(object sender, EventArgs e)
        {
            if (PasteFieldControl != null)
            {
                PasteFieldControl(rightClickLocation);
            }
        }

        /// <summary>
        /// Handles the Click event of the Relate menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuRelate_Click(object sender, EventArgs e)
        {
            if (FieldCreationRequested != null)
            {
                FieldCreationRequested(this, MetaFieldType.Relate, PagePanel, rightClickLocation);
            }
        }

        /// <summary>
        /// Handles the Click event of the Grid menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuGrid_Click(object sender, EventArgs e)
        {
            if (FieldCreationRequested != null)
            {
                FieldCreationRequested(this, MetaFieldType.Grid, PagePanel, rightClickLocation);
            }
        }

        /// <summary>
        /// Handles the Click event of the Mirror menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuMirror_Click(object sender, EventArgs e)
        {
            if (FieldCreationRequested != null)
            {
                FieldCreationRequested(this, MetaFieldType.Mirror, PagePanel, rightClickLocation);
            }
        }

        /// <summary>
        /// Handles the Click event of the Image menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuImage_Click(object sender, EventArgs e)
        {
            if (FieldCreationRequested != null)
            {
                FieldCreationRequested(this, MetaFieldType.Image, PagePanel, rightClickLocation);
            }
        }

        /// <summary>
        /// Handles the Click event of the Legal Values menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuLegalValues_Click(object sender, EventArgs e)
        {
            if (FieldCreationRequested != null)
            {
                FieldCreationRequested(this, MetaFieldType.LegalValues, PagePanel, rightClickLocation);
            }
        }

        /// <summary>
        /// Handles the Click event of the Codes menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuCodes_Click(object sender, EventArgs e)
        {
            if (FieldCreationRequested != null)
            {
                FieldCreationRequested(this, MetaFieldType.Codes, PagePanel, rightClickLocation);
            }
        }

        /// <summary>
        /// Handles the Click event of the Codes menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuList_Click(object sender, EventArgs e)
        {
            if (FieldCreationRequested != null)
            {
                FieldCreationRequested(this, MetaFieldType.List, PagePanel, rightClickLocation);
            }
        }

        /// <summary>
        /// Handles the Click event of the Comment Legal menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuCommentLegal_Click(object sender, EventArgs e)
        {
            if (FieldCreationRequested != null)
            {
                FieldCreationRequested(this, MetaFieldType.CommentLegal, PagePanel, rightClickLocation);
            }
        }

        /// <summary>
        /// Handles the Click event of the Options menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuOption_Click(object sender, EventArgs e)
        {
            if (FieldCreationRequested != null)
            {
                FieldCreationRequested(this, MetaFieldType.Option, PagePanel, rightClickLocation);
            }
        }

        /// <summary>
        /// Handles the Click event of the Yes No menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuYesNo_Click(object sender, EventArgs e)
        {
            if (FieldCreationRequested != null)
            {
                FieldCreationRequested(this, MetaFieldType.YesNo, PagePanel, rightClickLocation);
            }
        }

        /// <summary>
        /// Handles the Click event of the Upper Case menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuUpperCase_Click(object sender, EventArgs e)
        {
            if (FieldCreationRequested != null)
            {
                FieldCreationRequested(this, MetaFieldType.TextUppercase, PagePanel, rightClickLocation);
            }
        }

        /// <summary>
        /// Handles the Click event of the Time menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuTime_Click(object sender, EventArgs e)
        {
            if (FieldCreationRequested != null)
            {
                FieldCreationRequested(this, MetaFieldType.Time, PagePanel, rightClickLocation);
            }
        }

        /// <summary>
        /// Handles the Click event of the Text menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuText_Click(object sender, EventArgs e)
        {
            if (FieldCreationRequested != null)
            {
                FieldCreationRequested(this, MetaFieldType.Text, PagePanel, rightClickLocation);
            }
        }

        /// <summary>
        /// Handles the Click event of the Phone Number menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuPhoneNumber_Click(object sender, EventArgs e)
        {
            if (FieldCreationRequested != null)
            {
                FieldCreationRequested(this, MetaFieldType.PhoneNumber, PagePanel, rightClickLocation);
            }
        }

        /// <summary>
        /// Handles the Click event of the Number menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuNumber_Click(object sender, EventArgs e)
        {
            if (FieldCreationRequested != null)
            {
                FieldCreationRequested(this, MetaFieldType.Number, PagePanel, rightClickLocation);
            }
        }

        /// <summary>
        /// Handles the Click event of the Multiline menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuMultiline_Click(object sender, EventArgs e)
        {
            if (FieldCreationRequested != null)
            {
                FieldCreationRequested(this, MetaFieldType.Multiline, PagePanel, rightClickLocation);
            }
        }

        /// <summary>
        /// Handles the Click event of the Label menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuLabel_Click(object sender, EventArgs e)
        {
            if (FieldCreationRequested != null)
            {
                FieldCreationRequested(this, MetaFieldType.LabelTitle, PagePanel, rightClickLocation);
            }
        }

        /// <summary>
        /// Handles the Click event of the Date Time menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuDateTime_Click(object sender, EventArgs e)
        {
            if (FieldCreationRequested != null)
            {
                FieldCreationRequested(this, MetaFieldType.DateTime, PagePanel, rightClickLocation);
            }
        }

        /// <summary>
        /// Handles the Click event of the Data menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuDate_Click(object sender, EventArgs e)
        {
            if (FieldCreationRequested != null)
            {
                FieldCreationRequested(this, MetaFieldType.Date, PagePanel, rightClickLocation);
            }
        }

        /// <summary>
        /// Handles the Click event of the Command Button menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuCommandButton_Click(object sender, EventArgs e)
        {
            if (FieldCreationRequested != null)
            {
                FieldCreationRequested(this, MetaFieldType.CommandButton, PagePanel, rightClickLocation);
            }
        }

        /// <summary>
        /// Handles the Click event of the Check Box menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuCheckBox_Click(object sender, EventArgs e)
        {
            if (FieldCreationRequested != null)
            {
                FieldCreationRequested(this, MetaFieldType.Checkbox, PagePanel, rightClickLocation);
            }
        }

        #endregion  //Event Handlers

        #region Public Properties
        /// <summary>
        /// Whether the canvas should be opened for viewing only
        /// </summary>
        public bool OpenForViewingOnly
        {
            get
            {
                return this.openForViewingOnly;
            }
            set
            {
                this.openForViewingOnly = value;
            }
        }

        /// <summary>
        /// Gets the canvas' image list
        /// </summary>
        public ImageList ImageList
        {
            get
            {
                return this.imgCanvas;
            }
        }

        /// <summary>
        /// Gets the Point object representing the last right click on the canvas.
        /// </summary>
        public Point RightClickLocation
        {
            get { return this.rightClickLocation; }
        }

        /// <summary>
        /// Gets the Rectangle defined by the users MouseDown/MouseUp.
        /// </summary>
        public Rectangle GroupFieldOutline
        {
            get { return this.groupFieldOutline; }
        }

        public Panel PagePanel
        {
            get { return this.pagePanel; }
            set { pagePanel = value; }
        }

        /// <summary>
        /// Gets the current Panel
        /// </summary>
        public List<Control> TabIndexIndicators
        {
            get { return this.tabIndexIndicators; }
            set { tabIndexIndicators = value; }
        }

        #endregion  //Public Properties

        #region Private Methods

        public void RemoveTabIndexIndicators()
        {
            if (this.TabIndexIndicators != null)
            {
                if (this.TabIndexIndicators.Count > 0)
                {
                    foreach (Control tabBox in this.TabIndexIndicators)
                    {
                        PagePanel.Controls.Remove(tabBox);
                    }
                    this.TabIndexIndicators.Clear();
                }
            }
        }
        
        private void SaveSnapshot()
        {
            PagePanel.BackgroundImageLayout = ImageLayout.None;
            if (PagePanel.Size.Width > 0 && PagePanel.Size.Height > 0)
            {
                Bitmap b = new Bitmap(PagePanel.Size.Width, PagePanel.Size.Height);
                this.bufferGraphics = Graphics.FromImage(b);
                PagePanel.BackColor = makeViewForm.CurrentBackgroundColor;
                this.bufferGraphics.Clear(PagePanel.BackColor);
                if (makeViewForm.CurrentBackgroundImage != null)
                {
                    Image img = makeViewForm.CurrentBackgroundImage;
                    switch (makeViewForm.CurrentBackgroundImageLayout.ToUpper())
                    {
                        case "TILE":
                            TextureBrush tileBrush = new TextureBrush(img, System.Drawing.Drawing2D.WrapMode.Tile);
                            bufferGraphics.FillRectangle(tileBrush, 0, 0, PagePanel.Size.Width, PagePanel.Size.Height);
                            tileBrush.Dispose();
                            break;

                        case "STRETCH":
                            bufferGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            bufferGraphics.DrawImage(img, 0, 0, PagePanel.Size.Width, PagePanel.Size.Height);
                            bufferGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
                            break;

                        case "CENTER":
                            int centerX = (this.PagePanel.Size.Width - img.Width) / 2;
                            int centerY = (this.PagePanel.Size.Height - img.Height) / 2;
                            bufferGraphics.DrawImage(img, centerX, centerY);
                            break;

                        default:
                            bufferGraphics.DrawImage(img, 0, 0);
                            break;
                    }
                }
                if (config.Settings.ShowGrid)
                {
                    this.DrawGrid(b);
                }
                bufferGraphics.DrawImage(b, 0, 0);
                bufferBitmap = b;
                PagePanel.BackgroundImage = b;
            }
        }

        private void DrawGrid(Bitmap b)
        {
            for (int y = 0; y < b.Height; y += (config.Settings.GridSize + 1) * Epi.Constants.GRID_FACTOR)
            {
                for (int x = 0; x < b.Width; x += (config.Settings.GridSize + 1) * Epi.Constants.GRID_FACTOR)
                {
                    b.SetPixel(x, y, Color.Silver);
                }
            }
        }

        private void DrawPicture(Graphics g)
        {
            if (bufferBitmap != null)
            {
                g.DrawImage(bufferBitmap, 0, 0);
            }
        }

        public void FieldDefinition_MouseDown(object sender, MouseEventArgs e)
        {
            if (OpenForViewingOnly)
            {
                return;
            }

            if (makeViewForm.mediator.SelectedFieldControls.Count == 0)
            {
                if (e.Button == MouseButtons.Right)
                {
                    makeViewForm.mediator.RightClickedControl = (IFieldControl)sender;
                    ContextMenuStrip contextMenuStrip = BuildFieldPropertiesContextMenu((Control)sender);

                    if (((Control)sender).Visible)
                    {
                        contextMenuStrip.Show((Control)sender, e.Location);
                    }
                }
                else
                {
                    RemoveTabIndexIndicators();
                }
            }
        }

        public int Snap(int before)
        {
            return Snap(before, false);
        }

        public int Snap(int before, bool roundUp)
        {
            int delta;
            return Snap(before, roundUp, out delta);
        }

        public int Snap(int before, bool roundUp, out int delta)
        {
            int after;
            Configuration config = Configuration.GetNewInstance();
            int gridSize = config.Settings.GridSize;
            int gridSizePixels = (gridSize + 1) * Epi.Constants.GRID_FACTOR;

            int modulo = before % gridSizePixels;

            if (modulo < (gridSizePixels / 2) && roundUp == false)
            {
                delta = -modulo;
                after = before + delta;
            }
            else
            {
                delta = gridSizePixels - modulo;
                after = before + delta;
            }
            return after;
        }

        public void Snap(Control sender, SortedDictionary<IFieldControl, Point> fieldControls)
        {
            int deltaLeft = 0;
            int deltaTop = 0;
            if( config.Settings.SnapToGrid)
            {
                if (sender is DragableLabel && ((DragableLabel)sender).LabelFor != null && config.Settings.SnapPromptToGrid == false)
                {
                    Snap(((DragableLabel)sender).LabelFor.Left, false, out deltaLeft);
                    Snap(((DragableLabel)sender).LabelFor.Top, false, out deltaTop);
                }
                else
                {
                    Snap(sender.Left, false, out deltaLeft);
                    Snap(sender.Top, false, out deltaTop);
                }

                foreach (KeyValuePair<IFieldControl, Point> kvp in fieldControls)
                {
                    ((Control)kvp.Key).Left += deltaLeft;
                    ((Control)kvp.Key).Top += deltaTop;
                }
            }
        }

        /// <summary>
        /// Builds the context menu for the field definition items
        /// </summary>
        /// <returns>Field Definition context menu</returns>
        public ContextMenuStrip BuildFieldDefinitionContextMenu()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.ImageList = imgCanvas;

            ToolStripMenuItem mnuCheckBox = new ToolStripMenuItem(SharedStrings.PE_CHECKBOX_NODE);
            mnuCheckBox.Click += new EventHandler(mnuCheckBox_Click);
            mnuCheckBox.ImageIndex = 1;

            ToolStripMenuItem mnuCommandButton = new ToolStripMenuItem(SharedStrings.PE_COMMAND_BUTTON_NODE);
            mnuCommandButton.Click += new EventHandler(mnuCommandButton_Click);
            mnuCommandButton.ImageIndex = 0;

            ToolStripMenuItem mnuDate = new ToolStripMenuItem(SharedStrings.PE_DATE_NODE);
            mnuDate.Click += new EventHandler(mnuDate_Click);
            mnuDate.ImageIndex = 2;

            ToolStripMenuItem mnuDateTime = new ToolStripMenuItem(SharedStrings.PE_DATETIME_NODE);
            mnuDateTime.ImageIndex = 2;
            mnuDateTime.Click += new EventHandler(mnuDateTime_Click);

            ToolStripMenuItem mnuLabel = new ToolStripMenuItem(SharedStrings.PE_LABEL_NODE);
            mnuLabel.Click += new EventHandler(mnuLabel_Click);
            mnuLabel.ImageIndex = 7;

            ToolStripMenuItem mnuMultiline = new ToolStripMenuItem(SharedStrings.PE_MULTI_LINE_NODE);
            mnuMultiline.ImageIndex = 12;
            mnuMultiline.Click += new EventHandler(mnuMultiline_Click);

            ToolStripMenuItem mnuNumber = new ToolStripMenuItem(SharedStrings.PE_NUMBER_NODE);
            mnuNumber.ImageIndex = 9;
            mnuNumber.Click += new EventHandler(mnuNumber_Click);

            ToolStripMenuItem mnuPhoneNumber = new ToolStripMenuItem(SharedStrings.PE_PHONE_NODE);
            mnuPhoneNumber.ImageIndex = 9;
            mnuPhoneNumber.Click += new EventHandler(mnuPhoneNumber_Click);

            ToolStripMenuItem mnuText = new ToolStripMenuItem(SharedStrings.PE_SINGLE_LINE_NODE);
            mnuText.Click += new EventHandler(mnuText_Click);
            mnuText.ImageIndex = 12;

            ToolStripMenuItem mnuGUID = new ToolStripMenuItem(SharedStrings.PE_GUID_NODE);
            mnuGUID.Click += new EventHandler(mnuGUID_Click);
            mnuGUID.ImageIndex = 12;
            mnuGUID.Enabled = true;

            ToolStripMenuItem mnuTime = new ToolStripMenuItem(SharedStrings.PE_TIME_NODE);
            mnuTime.ImageIndex = 2;
            mnuTime.Click += new EventHandler(mnuTime_Click);

            ToolStripMenuItem mnuUpperCase = new ToolStripMenuItem(SharedStrings.PE_UPPER_CASE_NODE);
            mnuUpperCase.ImageIndex = 13;
            mnuUpperCase.Click += new EventHandler(mnuUpperCase_Click);

            ToolStripMenuItem mnuYesNo = new ToolStripMenuItem(SharedStrings.PE_YESNO_NODE);
            mnuYesNo.ImageIndex = 3;
            mnuYesNo.Click += new EventHandler(mnuYesNo_Click);

            ToolStripMenuItem mnuOption = new ToolStripMenuItem(SharedStrings.PE_OPTION_NODE);
            mnuOption.ImageIndex = 11;
            mnuOption.Click += new EventHandler(mnuOption_Click);

            ToolStripMenuItem mnuCommentLegal = new ToolStripMenuItem(SharedStrings.PE_COMMENT_LEGAL_NODE);
            mnuCommentLegal.Click += new EventHandler(mnuCommentLegal_Click);
            mnuCommentLegal.ImageIndex = 3;

            ToolStripMenuItem mnuCodes = new ToolStripMenuItem(SharedStrings.PE_CODES_NODE);
            mnuCodes.Click += new EventHandler(mnuCodes_Click);
            mnuCodes.ImageIndex = 3;

            ToolStripMenuItem mnuLegalValues = new ToolStripMenuItem(SharedStrings.PE_LEGAL_VALUE_NODE);
            mnuLegalValues.Click += new EventHandler(mnuLegalValues_Click);
            mnuLegalValues.ImageIndex = 3;

            ToolStripMenuItem mnuImage = new ToolStripMenuItem(SharedStrings.PE_IMAGE_NODE);
            mnuImage.Click += new EventHandler(mnuImage_Click);
            mnuImage.ImageIndex = 6;
            mnuImage.Enabled = true;

            ToolStripMenuItem mnuMirror = new ToolStripMenuItem(SharedStrings.PE_MIRROR_NODE);
            mnuMirror.Click += new EventHandler(mnuMirror_Click);
            mnuMirror.ImageIndex = 8;
            mnuMirror.Enabled = true;

            ToolStripMenuItem mnuGrid = new ToolStripMenuItem(SharedStrings.PE_GRID_NODE);
            mnuGrid.Click += new EventHandler(mnuGrid_Click);
            mnuGrid.ImageIndex = 4;
            mnuGrid.Enabled = true; 

            ToolStripMenuItem mnuRelate = new ToolStripMenuItem(SharedStrings.PE_RELATE_NODE);
            mnuRelate.ImageIndex = 0;
            mnuRelate.Click += new EventHandler(mnuRelate_Click);
            mnuRelate.Enabled = true;
            //ske2 -- Def 1467

            ToolStripMenuItem mnuNew = new ToolStripMenuItem(SharedStrings.NEW_FIELD);
            mnuNew.ImageIndex = 10;
            // Renamed mnuGroup to mnuNewGroup
            ToolStripMenuItem mnuNewGroup = new ToolStripMenuItem(SharedStrings.NEW_GROUP_FIELD);
            mnuNewGroup.ImageIndex = 5;
            mnuNewGroup.Click += new EventHandler(mnuGroup_Click);
            //mnuGroup.Enabled = false;

            ToolStripMenuItem mnuShowTabOrder = new ToolStripMenuItem(SharedStrings.SHOW_TAB_ORDER);
            mnuShowTabOrder.ImageIndex = 17;
            mnuShowTabOrder.Click += new EventHandler(mnuShowTabOrder_Click);

            ToolStripMenuItem mnuAutoSetTabOrder = new ToolStripMenuItem(SharedStrings.START_NEW_TAB_ORDER);
            mnuAutoSetTabOrder.ImageIndex = 17;
            mnuAutoSetTabOrder.Click += new EventHandler(mnuStartNewTabOrder_Click);

            ToolStripMenuItem mnuContinueSetTabOrder = new ToolStripMenuItem(SharedStrings.CONTINUE_TAB_ORDER);
            mnuContinueSetTabOrder.ImageIndex = 17;
            mnuContinueSetTabOrder.Click += new EventHandler(mnuContinueTabOrder_Click);  
         
            ToolStripMenuItem mnuTabs = new ToolStripMenuItem(SharedStrings.TABS);
            mnuTabs.ImageIndex = 17;

            mnuTabs.DropDownItems.Add(mnuShowTabOrder);
            mnuTabs.DropDownItems.Add(new ToolStripSeparator());
            mnuTabs.DropDownItems.Add(mnuAutoSetTabOrder);
            mnuTabs.DropDownItems.Add(mnuContinueSetTabOrder);

            ToolStripMenuItem mnuPageCheckCode = new ToolStripMenuItem(SharedStrings.PAGE_CHECK_CODE);
            mnuPageCheckCode.ImageIndex = 14;
            mnuPageCheckCode.Click += new EventHandler(mnuPageCheckCode_Click);

            ToolStripSeparator separator = new ToolStripSeparator();

            ToolStripMenuItem mnuCut = new ToolStripMenuItem(SharedStrings.CUT);
            mnuCut.Enabled = EnableCutCopyMenuItem;
            mnuCut.Click += new EventHandler(mnuCut_Click);

            ToolStripMenuItem mnuCopy = new ToolStripMenuItem(SharedStrings.COPY);
            mnuCopy.Enabled = EnableCutCopyMenuItem;
            mnuCopy.Click += new EventHandler(mnuCopy_Click);

            ToolStripMenuItem mnuPaste = new ToolStripMenuItem(SharedStrings.PASTE);
            mnuPaste.Enabled = !(makeViewForm.mediator.IsControlClipboardEmpty);
            mnuPaste.Click += new EventHandler(mnuPaste_Click);

            ToolStripMenuItem mnuDelete = new ToolStripMenuItem(SharedStrings.DELETE);
            mnuDelete.Enabled = EnableCutCopyMenuItem;
            mnuDelete.Click += new EventHandler(mnuDelete_Click);

            ToolStripSeparator separatorFont = new ToolStripSeparator(); 
            
            ToolStripMenuItem mnuFont = new ToolStripMenuItem(SharedStrings.APPLY_DEFAULT_FONTS);
            mnuFont.Enabled = EnableCutCopyMenuItem;
            mnuFont.Click += new EventHandler(mnuFont_Click);

            ToolStripSeparator separatorAlign = new ToolStripSeparator();

            ToolStripMenuItem mnuAlignOneColumn = new ToolStripMenuItem(SharedStrings.ALIGN_1_COLUMN);
            mnuAlignOneColumn.Enabled = EnableCutCopyMenuItem;
            mnuAlignOneColumn.ShowShortcutKeys = true;
            mnuAlignOneColumn.ShortcutKeys = Keys.Control | Keys.D1;
            mnuAlignOneColumn.Tag = 1;
            mnuAlignOneColumn.Click += new EventHandler(mnuAlignAsTable_Click);

            ToolStripMenuItem mnuAlignTwoColumns = new ToolStripMenuItem(SharedStrings.ALIGN_2_COLUMN);
            mnuAlignTwoColumns.Enabled = EnableCutCopyMenuItem;
            mnuAlignTwoColumns.ShowShortcutKeys = true;
            mnuAlignTwoColumns.ShortcutKeys = Keys.Control | Keys.D2;
            mnuAlignTwoColumns.Tag = 2;
            mnuAlignTwoColumns.Click += new EventHandler(mnuAlignAsTable_Click);

            ToolStripMenuItem mnuAlignThreeColumns = new ToolStripMenuItem(SharedStrings.ALIGN_3_COLUMN);
            mnuAlignThreeColumns.Enabled = EnableCutCopyMenuItem;
            mnuAlignThreeColumns.ShowShortcutKeys = true;
            mnuAlignThreeColumns.ShortcutKeys = Keys.Control | Keys.D3;
            mnuAlignThreeColumns.Tag = 3;
            mnuAlignThreeColumns.Click += new EventHandler(mnuAlignAsTable_Click);

            ToolStripMenuItem mnuAlignFourColumns = new ToolStripMenuItem(SharedStrings.ALIGN_4_COLUMN);
            mnuAlignFourColumns.Enabled = EnableCutCopyMenuItem;
            mnuAlignFourColumns.ShowShortcutKeys = true;
            mnuAlignFourColumns.ShortcutKeys = Keys.Control | Keys.D4;
            mnuAlignFourColumns.Tag = 4;
            mnuAlignFourColumns.Click += new EventHandler(mnuAlignAsTable_Click);

            ToolStripMenuItem mnuAlignFiveColumns = new ToolStripMenuItem(SharedStrings.ALIGN_5_COLUMN);
            mnuAlignFiveColumns.Enabled = EnableCutCopyMenuItem;
            mnuAlignFiveColumns.ShowShortcutKeys = true;
            mnuAlignFiveColumns.ShortcutKeys = Keys.Control | Keys.D5;
            mnuAlignFiveColumns.Tag = 5;
            mnuAlignFiveColumns.Click += new EventHandler(mnuAlignAsTable_Click);

            ToolStripMenuItem mnuAlignAsRow = new ToolStripMenuItem(SharedStrings.ALIGN_AS_ROW);
            mnuAlignAsRow.Enabled = EnableCutCopyMenuItem;
            mnuAlignAsRow.ShowShortcutKeys = true;
            mnuAlignAsRow.ShortcutKeys = Keys.Control | Keys.Right;
            mnuAlignAsRow.Click += new EventHandler(mnuAlignAsRow_Click);

            ToolStripSeparator separatorMakeSame = new ToolStripSeparator();

            ToolStripMenuItem mnuMakeSameWidthMax = new ToolStripMenuItem(SharedStrings.MAKE_SAME_WIDTH_MAX);
            mnuMakeSameWidthMax.Enabled = EnableCutCopyMenuItem;
            mnuMakeSameWidthMax.ShowShortcutKeys = true;
            mnuMakeSameWidthMax.ShortcutKeys = Keys.Control | Keys.W;
            mnuMakeSameWidthMax.Click += new EventHandler(mnuMakeSameWidthMax_Click);

            ToolStripMenuItem mnuMakeSameWidthMin = new ToolStripMenuItem(SharedStrings.MAKE_SAME_WIDTH_MIN);
            mnuMakeSameWidthMin.Enabled = EnableCutCopyMenuItem;
            mnuMakeSameWidthMin.ShowShortcutKeys = true;
            mnuMakeSameWidthMin.ShortcutKeys = Keys.Control | Keys.Shift | Keys.W;
            mnuMakeSameWidthMin.Click += new EventHandler(mnuMakeSameWidthMin_Click);

            ToolStripMenuItem mnuMakeSameHeightMax = new ToolStripMenuItem(SharedStrings.MAKE_SAME_HEIGHT_MAX);
            mnuMakeSameHeightMax.Enabled = EnableCutCopyMenuItem;
            mnuMakeSameHeightMax.ShowShortcutKeys = true;
            mnuMakeSameHeightMax.ShortcutKeys = Keys.Control | Keys.H;
            mnuMakeSameHeightMax.Click += new EventHandler(mnuMakeSameHeightMax_Click);
            
            ToolStripMenuItem mnuMakeSameHeightMin = new ToolStripMenuItem(SharedStrings.MAKE_SAME_HEIGHT_MIN);
            mnuMakeSameHeightMin.Enabled = EnableCutCopyMenuItem;
            mnuMakeSameHeightMin.ShowShortcutKeys = true;
            mnuMakeSameHeightMin.ShortcutKeys = Keys.Control | Keys.Shift | Keys.H;
            mnuMakeSameHeightMin.Click += new EventHandler(mnuMakeSameHeightMin_Click);

            ToolStripMenuItem mnuMakeSameSizeMax = new ToolStripMenuItem(SharedStrings.MAKE_SAME_SIZE_MAX);
            mnuMakeSameSizeMax.Enabled = EnableCutCopyMenuItem;
            mnuMakeSameSizeMax.ShowShortcutKeys = true;
            mnuMakeSameSizeMax.ShortcutKeys = Keys.Control | Keys.S;
            mnuMakeSameSizeMax.Click += new EventHandler(mnuMakeSameSizeMax_Click);
            
            ToolStripMenuItem mnuMakeSameSizeMin = new ToolStripMenuItem(SharedStrings.MAKE_SAME_SIZE_MIN);
            mnuMakeSameSizeMin.Enabled = EnableCutCopyMenuItem;
            mnuMakeSameSizeMin.ShowShortcutKeys = true;
            mnuMakeSameSizeMin.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
            mnuMakeSameSizeMin.Click += new EventHandler(mnuMakeSameSizeMin_Click);
            
            ToolStripSeparator separatorTemplates = new ToolStripSeparator();

            ToolStripMenuItem mnuCreateTemplate = new ToolStripMenuItem(SharedStrings.SAVE_AS_TEMPLATE);
            mnuCreateTemplate.Enabled = EnableCutCopyMenuItem;
            mnuCreateTemplate.Click += new EventHandler(mnuCreateTemplate_Click);

            mnuNew.DropDown.ImageList = imgCanvas;
            mnuNew.DropDownItems.AddRange(new ToolStripMenuItem[] 
            { 
                mnuLabel, 
                mnuText, 
                mnuUpperCase, 
                mnuMultiline, 
                mnuGUID, 
                mnuNumber, 
                mnuPhoneNumber, 
                mnuDate, 
                mnuTime, 
                mnuDateTime, 
                mnuCheckBox, 
                mnuYesNo, 
                mnuOption, 
                mnuCommandButton, 
                mnuImage, 
                mnuMirror, 
                mnuGrid, 
                mnuLegalValues, 
                mnuCommentLegal, 
                mnuCodes,
                mnuRelate });

            contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] 
            { 
                mnuNew, 
                mnuNewGroup, 
                mnuTabs, 
                mnuPageCheckCode,

                separator, 

                mnuCut,
                mnuCopy,
                mnuPaste,
                mnuDelete,

                separatorFont,

                mnuFont,

                separatorAlign,

                mnuAlignOneColumn,
                mnuAlignTwoColumns,
                mnuAlignThreeColumns,
                mnuAlignFourColumns,
                mnuAlignFiveColumns,
                mnuAlignAsRow,

                separatorMakeSame,

                mnuMakeSameWidthMax,
                mnuMakeSameWidthMin,
                mnuMakeSameHeightMax,
                mnuMakeSameHeightMin,
                mnuMakeSameSizeMax,
                mnuMakeSameSizeMin,

                separatorTemplates,

                mnuCreateTemplate
            });

            return contextMenu;
        }

        private ContextMenuStrip BuildFieldPropertiesContextMenu(object sender)
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.ImageList = imgCanvas;

            Epi.Fields.Field field = makeViewForm.mediator.RightClickedControl.Field;

            ToolStripMenuItem mnuProperties = new ToolStripMenuItem(SharedStrings.PROPERTIES);
            mnuProperties.ImageIndex = 16;
            mnuProperties.Click += new EventHandler(makeViewForm.mediator.mnuProperties_Click);
            contextMenu.Items.Add(mnuProperties);

            if (makeViewForm.mediator.RightClickedControl.Field is Epi.Fields.RelatedViewField)
            {
                ToolStripMenuItem mnuGoTo = new ToolStripMenuItem(SharedStrings.GO_TO_RELATED_FORM);
                mnuGoTo.ImageIndex = 18;
                mnuGoTo.Click += new EventHandler(makeViewForm.mediator.mnuGoTo_Click);
                contextMenu.Items.Add(mnuGoTo);
            }

            ToolStripMenuItem mnuCheckCode = new ToolStripMenuItem(SharedStrings.FIELD_CHECK_CODE);
            mnuCheckCode.ImageIndex = 14;
            mnuCheckCode.Click += new EventHandler(makeViewForm.mediator.mnuCheckCode_Click);
            contextMenu.Items.Add(mnuCheckCode);
            mnuCheckCode.Enabled = (
                    field is Fields.IFieldWithCheckCodeAfter)
                || (field is Fields.IFieldWithCheckCodeBefore)
                || (field is Fields.IFieldWithCheckCodeClick);

            ToolStripMenuItem mnuChangeTo = new ToolStripMenuItem(SharedStrings.CHANGE_TO);
            mnuChangeTo.DropDownItems.AddRange(BuildChangeToList(field));

            bool hasCollectedDataColumn = (makeViewForm.mediator.Project.CollectedData.TableExists(field.GetView().TableName));
            mnuChangeTo.Enabled = (hasCollectedDataColumn == false) || (hasCollectedDataColumn && makeViewForm.mediator.NewFieldIds.Contains(field.Id));

            contextMenu.Items.Add(mnuChangeTo);

            if (sender is DragableLabel && ((DragableLabel)sender).Field is Epi.Fields.LabelField)
            {
                ToolStripMenuItem mnuAutoSize = new ToolStripMenuItem(SharedStrings.DEFAULT_AUTO_SIZE);
                mnuAutoSize.Click += new EventHandler(makeViewForm.mediator.mnuAutoSize_Click);
                contextMenu.Items.Add(mnuAutoSize);
            }
            else
            { 
                ToolStripMenuItem mnuPromptAlign = new ToolStripMenuItem(SharedStrings.DEFAULT_PROMPT_ALIGN);
                mnuPromptAlign.Click += new EventHandler(makeViewForm.mediator.mnuPromptAlign_Click);
                contextMenu.Items.Add(mnuPromptAlign);
            }

            contextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem mnuCut = new ToolStripMenuItem(SharedStrings.CUT);            
            mnuCut.Click += new EventHandler(makeViewForm.mediator.mnuCut_Click);
            contextMenu.Items.Add(mnuCut);

            ToolStripMenuItem mnuCopy = new ToolStripMenuItem(SharedStrings.COPY);            
            mnuCopy.Click += new EventHandler(makeViewForm.mediator.mnuCopy_Click);
            contextMenu.Items.Add(mnuCopy);

            ToolStripMenuItem mnuDelete = new ToolStripMenuItem(SharedStrings.DELETE);
            mnuDelete.ImageIndex = 15;
            mnuDelete.Click += new EventHandler(makeViewForm.mediator.mnuDelete_Click);
            contextMenu.Items.Add(mnuDelete);

            return contextMenu;
        }

        private ToolStripMenuItem[] BuildChangeToList(Epi.Fields.Field field)
        {
            List<ToolStripMenuItem> items = new List<ToolStripMenuItem>();

            if (MetaFieldType.LabelTitle != field.FieldType)
            {
                ToolStripMenuItem mnuLabel = new ToolStripMenuItem(SharedStrings.PE_LABEL_NODE);
                mnuLabel.ImageIndex = 7;
                mnuLabel.Tag = MetaFieldType.LabelTitle;
                mnuLabel.Click += new EventHandler(makeViewForm.mediator.canvas_ChangeToFieldType_Click);
                items.Add(mnuLabel);
            }
            if (MetaFieldType.Text != field.FieldType)
            {
                ToolStripMenuItem mnuText = new ToolStripMenuItem(SharedStrings.PE_SINGLE_LINE_NODE);
                mnuText.ImageIndex = 12;
                mnuText.Tag = MetaFieldType.Text;
                mnuText.Click += new EventHandler(makeViewForm.mediator.canvas_ChangeToFieldType_Click);
                items.Add(mnuText);
            }
            if (MetaFieldType.TextUppercase != field.FieldType)
            {
                ToolStripMenuItem mnuUpperCase = new ToolStripMenuItem(SharedStrings.PE_UPPER_CASE_NODE);
                mnuUpperCase.ImageIndex = 13;
                mnuUpperCase.Tag = MetaFieldType.TextUppercase;
                mnuUpperCase.Click += new EventHandler(makeViewForm.mediator.canvas_ChangeToFieldType_Click);
                items.Add(mnuUpperCase);
            }
            if (MetaFieldType.Multiline != field.FieldType)
            {
                ToolStripMenuItem mnuMultiline = new ToolStripMenuItem(SharedStrings.PE_MULTI_LINE_NODE);
                mnuMultiline.ImageIndex = 12;
                mnuMultiline.Tag = MetaFieldType.Multiline;
                mnuMultiline.Click += new EventHandler(makeViewForm.mediator.canvas_ChangeToFieldType_Click);
                items.Add(mnuMultiline);
            }
            if (MetaFieldType.GUID != field.FieldType)
            {
                ToolStripMenuItem mnuGUID = new ToolStripMenuItem(SharedStrings.PE_GUID_NODE);
                mnuGUID.ImageIndex = 12;
                mnuGUID.Tag = MetaFieldType.GUID;
                mnuGUID.Click += new EventHandler(makeViewForm.mediator.canvas_ChangeToFieldType_Click);
                items.Add(mnuGUID);
            }
            if (MetaFieldType.Number != field.FieldType)
            {
                ToolStripMenuItem mnuNumber = new ToolStripMenuItem(SharedStrings.PE_NUMBER_NODE);
                mnuNumber.ImageIndex = 9;
                mnuNumber.Tag = MetaFieldType.Number;
                mnuNumber.Click += new EventHandler(makeViewForm.mediator.canvas_ChangeToFieldType_Click);
                items.Add(mnuNumber);
            }
            if (MetaFieldType.PhoneNumber != field.FieldType)
            {
                ToolStripMenuItem mnuPhoneNumber = new ToolStripMenuItem(SharedStrings.PE_PHONE_NODE);
                mnuPhoneNumber.ImageIndex = 9;
                mnuPhoneNumber.Tag = MetaFieldType.PhoneNumber;
                mnuPhoneNumber.Click += new EventHandler(makeViewForm.mediator.canvas_ChangeToFieldType_Click);
                items.Add(mnuPhoneNumber);
            }
            if (MetaFieldType.Date != field.FieldType)
            {
                ToolStripMenuItem mnuDate = new ToolStripMenuItem(SharedStrings.PE_DATE_NODE);
                mnuDate.ImageIndex = 2;
                mnuDate.Tag = MetaFieldType.Date;
                mnuDate.Click += new EventHandler(makeViewForm.mediator.canvas_ChangeToFieldType_Click);
                items.Add(mnuDate);
            }
            if (MetaFieldType.Time != field.FieldType)
            {
                ToolStripMenuItem mnuTime = new ToolStripMenuItem(SharedStrings.PE_TIME_NODE);
                mnuTime.ImageIndex = 2;
                mnuTime.Tag = MetaFieldType.Time;
                mnuTime.Click += new EventHandler(makeViewForm.mediator.canvas_ChangeToFieldType_Click);
                items.Add(mnuTime);
            }
            if (MetaFieldType.DateTime != field.FieldType)
            {
                ToolStripMenuItem mnuDateTime = new ToolStripMenuItem(SharedStrings.PE_DATETIME_NODE);
                mnuDateTime.ImageIndex = 2;
                mnuDateTime.Tag = MetaFieldType.DateTime;
                mnuDateTime.Click += new EventHandler(makeViewForm.mediator.canvas_ChangeToFieldType_Click);
                items.Add(mnuDateTime);
            }
            if (MetaFieldType.Checkbox != field.FieldType)
            {
                ToolStripMenuItem mnuCheckBox = new ToolStripMenuItem(SharedStrings.PE_CHECKBOX_NODE);
                mnuCheckBox.ImageIndex = 1;
                mnuCheckBox.Tag = MetaFieldType.Checkbox;
                mnuCheckBox.Click += new EventHandler(makeViewForm.mediator.canvas_ChangeToFieldType_Click);
                items.Add(mnuCheckBox);
            }
            if (MetaFieldType.YesNo != field.FieldType)
            {
                ToolStripMenuItem mnuYesNo = new ToolStripMenuItem(SharedStrings.PE_YESNO_NODE);
                mnuYesNo.ImageIndex = 3;
                mnuYesNo.Tag = MetaFieldType.YesNo;
                mnuYesNo.Click += new EventHandler(makeViewForm.mediator.canvas_ChangeToFieldType_Click);
                items.Add(mnuYesNo);
            }
            if (MetaFieldType.Option != field.FieldType)
            {
                ToolStripMenuItem mnuOption = new ToolStripMenuItem(SharedStrings.PE_OPTION_NODE);
                mnuOption.ImageIndex = 11;
                mnuOption.Tag = MetaFieldType.Option;
                mnuOption.Click += new EventHandler(makeViewForm.mediator.canvas_ChangeToFieldType_Click);
                items.Add(mnuOption);
            }
            if (MetaFieldType.CommandButton != field.FieldType)
            {
                ToolStripMenuItem mnuCommandButton = new ToolStripMenuItem(SharedStrings.PE_COMMAND_BUTTON_NODE);
                mnuCommandButton.ImageIndex = 0;
                mnuCommandButton.Tag = MetaFieldType.CommandButton;
                mnuCommandButton.Click += new EventHandler(makeViewForm.mediator.canvas_ChangeToFieldType_Click);
                items.Add(mnuCommandButton);
            }
            if (MetaFieldType.Image != field.FieldType)
            {
                ToolStripMenuItem mnuImage = new ToolStripMenuItem(SharedStrings.PE_IMAGE_NODE);
                mnuImage.ImageIndex = 6;
                mnuImage.Tag = MetaFieldType.Image;
                mnuImage.Click += new EventHandler(makeViewForm.mediator.canvas_ChangeToFieldType_Click);
                items.Add(mnuImage);
            }
            if (MetaFieldType.Mirror != field.FieldType)
            {
                ToolStripMenuItem mnuMirror = new ToolStripMenuItem(SharedStrings.PE_MIRROR_NODE);
                mnuMirror.ImageIndex = 8;
                mnuMirror.Tag = MetaFieldType.Mirror;
                mnuMirror.Click += new EventHandler(makeViewForm.mediator.canvas_ChangeToFieldType_Click);
                items.Add(mnuMirror);
            }
            if (MetaFieldType.LegalValues != field.FieldType)
            {
                ToolStripMenuItem mnuLegalValues = new ToolStripMenuItem(SharedStrings.PE_LEGAL_VALUE_NODE);
                mnuLegalValues.ImageIndex = 3;
                mnuLegalValues.Tag = MetaFieldType.LegalValues;
                mnuLegalValues.Click += new EventHandler(makeViewForm.mediator.canvas_ChangeToFieldType_Click);
                items.Add(mnuLegalValues);
            }
            if (MetaFieldType.CommentLegal != field.FieldType)
            {
                ToolStripMenuItem mnuCommentLegal = new ToolStripMenuItem(SharedStrings.PE_COMMENT_LEGAL_NODE);
                mnuCommentLegal.ImageIndex = 3;
                mnuCommentLegal.Tag = MetaFieldType.CommentLegal;
                mnuCommentLegal.Click += new EventHandler(makeViewForm.mediator.canvas_ChangeToFieldType_Click);
                items.Add(mnuCommentLegal);
            }
            if (MetaFieldType.Codes != field.FieldType)
            {
                ToolStripMenuItem mnuCodes = new ToolStripMenuItem(SharedStrings.PE_CODES_NODE);
                mnuCodes.ImageIndex = 3;
                mnuCodes.Tag = MetaFieldType.Codes;
                mnuCodes.Click += new EventHandler(makeViewForm.mediator.canvas_ChangeToFieldType_Click);
                items.Add(mnuCodes);
            }
            ToolStripMenuItem[] toolStripItems = items.ToArray();
            return toolStripItems;
        }

        private void PagePanel_Paint(object sender, PaintEventArgs e)
        {            
        }

        #endregion  //Private Methods
    }
}
