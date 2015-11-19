using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Epi;
using Epi.Core;
using Epi.Fields;
using EpiDashboard.Controls;

namespace EpiDashboard
{
    /// <summary>
    /// A base class for Dashboard gadgets.
    /// </summary>
    public class GadgetBase : UserControl, IGadget
    {
        #region Protected Members
        /// <summary>
        /// The worker thread that the data processing is intended to run on; by using a separate thread,
        /// the user can continue working in the dashboard UI as the gadget is processing the line list.
        /// </summary>
        protected BackgroundWorker worker;

        /// <summary>
        /// The base worker thread, which calls the 'worker' thread. Required for gadget cancellation.
        /// </summary>
        protected BackgroundWorker baseWorker;

        /// <summary>
        /// Required to 'lock' the worker thread so that you can't spawn more than one worker thread
        /// in the gadget at a single time.
        /// </summary>
        protected object syncLock = new object();

        /// <summary>
        /// A lock that can apply to all gadgets.
        /// </summary>
        public static object staticSyncLock = new object();

        private int strataCount = 0;

        #endregion // Protected Members

        #region Private Members
        private bool drawBorders = true;
        #endregion // Private Members

        #region Events
        public event GadgetCheckForCancellationHandler GadgetCheckForCancellation;
        public event GadgetClosingHandler GadgetClosing;
        public event GadgetProcessingFinishedHandler GadgetProcessingFinished;
        public event GadgetRefreshedHandler GadgetRefreshed;
        public event GadgetRepositionEventHandler GadgetReposition;
        public event GadgetStatusUpdateHandler GadgetStatusUpdate;
        public event GadgetEventHandler GadgetDragStart;
        public event GadgetEventHandler GadgetDragStop;
        public event GadgetEventHandler GadgetDrag;
        public event GadgetAnchorSetEventHandler GadgetAnchorSetFromXml;        
        #endregion // Events

        #region Delegates

        protected delegate void SetGridTextDelegate(string strataValue, TextBlockConfig textBlockConfig, FontWeight fontWeight);        
        protected delegate void AddOutputGridDelegate(string strataVar, string value, int columnCount);        
        protected delegate void RenderOutputGridHeaderDelegate(string strataValue, string freqVar);
        protected delegate void SetGridBarDelegate(string strataValue, int rowNumber, double pct);
        protected delegate void AddGridRowDelegate(string strataValue, int height);
        protected delegate void AddDataGridDelegate(DataView dv, string strataValue);
        protected delegate void AddGridFooterDelegate(string strataValue, int rowNumber, int[] totalRows);
        protected delegate void DrawFrequencyBordersDelegate(string strataValue);
        protected delegate void SetStatusDelegate(string statusMessage);
        protected delegate void RequestUpdateStatusDelegate(string statusMessage);
        protected delegate bool CheckForCancellationDelegate();

        protected delegate void RenderFinishWithErrorDelegate(string errorMessage);
        protected delegate void RenderFinishWithWarningDelegate(string errorMessage);
        protected delegate void SimpleCallback();        
        
        #endregion // Delegates

        #region Public Properties
        public virtual string CustomOutputHeading { get; set; }
        public virtual string CustomOutputDescription { get; set; }
        public virtual string CustomOutputCaption { get; set; }
        public UserControl AnchorLeft { get; set; }
        public UserControl AnchorTop { get; set; }
        public UserControl AnchorBottom { get; set; }
        public UserControl AnchorRight { get; set; }
        public bool IsBeingDragged { get; set; }
        public Guid UniqueIdentifier { get; private set; }
        public bool IsCollapsed { get; protected set; }
        public DashboardPopup Popup { get; protected set; }

        /// <summary>
        /// Gets the data filters associated with this gadget
        /// </summary>
        public DataFilters DataFilters { get; set; }

        /// <summary>
        /// Gets/sets whether the gadget's visual borders should be on or off.
        /// </summary>
        public bool DrawBorders
        {
            get
            {
                return this.drawBorders;
            }
            set
            {
                this.drawBorders = value;
                if (DrawBorders)
                {
                    TurnOnBorders();
                }
                else
                {
                    TurnOffBorders();
                }
            }
        }

        /// <summary>
        /// Gets/sets whether the gadget is processing
        /// </summary>
        public bool IsProcessing { get; set; }        
        #endregion // Public Properties

        #region Protected Properties
        protected int StrataCount
        {
            get
            {
                return this.strataCount;
            }
            set
            {
                this.strataCount = value;
            }
        }

        /// <summary>
        /// Gets the dashboard helper associated with this gadget
        /// </summary>
        protected DashboardHelper DashboardHelper { get; set; }

        /// <summary>
        /// Gets the configuration associated with this gadget's dashboard helper
        /// </summary>
        protected Configuration Config
        {
            get
            {
                return this.DashboardHelper.Config;
            }
        }

        /// <summary>
        /// Gets the list of strata grids for this gadget's output. For stratifications, each strata value will result in one output grid (typically). This list
        /// maintains those strata grids.
        /// </summary>
        protected List<Grid> StrataGridList { get; set; }

        /// <summary>
        /// List of panels in stratifications
        /// </summary>
        protected List<StackPanel> StrataPanelList { get; set; }

        /// <summary>
        /// The list of expanders for each group value.
        /// </summary>
        protected List<Expander> StrataExpanderList { get; set; }

        /// <summary>
        /// Gets/sets the gadget options
        /// </summary>
        /// <remarks>
        /// The object that is passed into the Dashboard Helper and contains the options the user selected.
        /// For example, this will contain the columns for the list variables, sort variables, group variable,
        /// and any other parameters set in the gadget UI.
        /// </remarks>
        protected GadgetParameters GadgetOptions { get; set; }

        /// <summary>
        /// Gets/sets the parameters for the gadget
        /// </summary>
        /// <remarks>
        /// The object that is passed into the data manager and contains the options the user selected.
        /// For example, this will contain the columns for the list variables, sort variables, group variable,
        /// and any other parameters set in the gadget UI.
        /// </remarks>
        protected IGadgetParameters Parameters { get; set; }

        /// <summary>
        /// Gets/sets whether the gadget is loading combo boxes.
        /// </summary>
        protected bool LoadingCombos { get; set; }
        #endregion // Protected Properties

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        public GadgetBase()
        {
            IsBeingDragged = false;
            this.Loaded += new RoutedEventHandler(GadgetBase_Loaded);
            this.PreviewMouseDown += new MouseButtonEventHandler(GadgetBase_PreviewMouseDown);
            this.PreviewMouseUp += new MouseButtonEventHandler(GadgetBase_PreviewMouseUp);
            this.PreviewMouseMove += new MouseEventHandler(GadgetBase_PreviewMouseMove);
            this.UniqueIdentifier = Guid.NewGuid();
        }
        #endregion // Constructors

        #region Public Methods
        public virtual XmlNode Serialize(XmlDocument doc) { return null; }
        public virtual void CreateFromXml(XmlElement element) 
        {
            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLower())
                {
                    case "anchorleft":
                        Guid guidL = new System.Guid(child.InnerText);
                        if (GadgetAnchorSetFromXml != null)
                        {
                            GadgetAnchorSetFromXml(this, new GadgetAnchorSetEventArgs(GadgetAnchorSetEventArgs.GadgetAnchorType.Left, guidL));
                        }
                        break;
                    case "anchortop":
                        Guid guidT = new System.Guid(child.InnerText);
                        if (GadgetAnchorSetFromXml != null)
                        {
                            GadgetAnchorSetFromXml(this, new GadgetAnchorSetEventArgs(GadgetAnchorSetEventArgs.GadgetAnchorType.Top, guidT));
                        }
                        break;
                }
            }

            SetAttributesFromXml(element);
        }
        public virtual string ToHTML(string htmlFileName = "", int count = 0) { return string.Empty; }
        public virtual void UpdateVariableNames() { }

        public virtual void CopyGadgetParameters(UserControl Gadget) 
        {
            this.Parameters = ((GadgetBase)Gadget).Parameters;
        }

        public virtual void CollapseOutput() 
        {
            object element = this.FindName("panelMain");
            if (element is StackPanel)
            {
                StackPanel panelMain = element as StackPanel;
                panelMain.Visibility = System.Windows.Visibility.Collapsed;
            }
            element = this.FindName("infoPanel");

            if (element is Controls.GadgetInfoPanel)
            {
                Controls.GadgetInfoPanel infoPanel = element as Controls.GadgetInfoPanel;                
                infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            }

            element = this.FindName("messagePanel");

            if (element is Controls.GadgetMessagePanel)
            {
                Controls.GadgetMessagePanel messagePanel = element as Controls.GadgetMessagePanel;
                messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            }
            //EI-24
            //element = this.FindName("descriptionPanel");

            //if (element is Controls.GadgetDescriptionPanel)
            //{
            //    Controls.GadgetDescriptionPanel descriptionPanel = element as Controls.GadgetDescriptionPanel;
            //    descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
            //}
            IsCollapsed = true;
        }

        public virtual void ExpandOutput() 
        {
            object element = this.FindName("panelMain");
            if (element is StackPanel)
            {
                StackPanel panelMain = element as StackPanel;
                panelMain.Visibility = System.Windows.Visibility.Visible;
            }

            element = this.FindName("messagePanel");

            if (element is Controls.GadgetMessagePanel)
            {
                Controls.GadgetMessagePanel messagePanel = element as Controls.GadgetMessagePanel;
                if (messagePanel.MessagePanelType != Controls.MessagePanelType.StatusPanel)
                {
                    messagePanel.Visibility = System.Windows.Visibility.Visible;
                }
            }

            element = this.FindName("infoPanel");

            if (element is Controls.GadgetInfoPanel)
            {
                Controls.GadgetInfoPanel infoPanel = element as Controls.GadgetInfoPanel;
                if (!string.IsNullOrEmpty(infoPanel.Text))
                {
                    infoPanel.Visibility = System.Windows.Visibility.Visible;
                }
            }

            element = this.FindName("descriptionPanel");

            if (element is Controls.GadgetDescriptionPanel)
            {
                Controls.GadgetDescriptionPanel descriptionPanel = element as Controls.GadgetDescriptionPanel;
                if (!string.IsNullOrEmpty(descriptionPanel.Text))
                {
                    descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.DisplayMode;
                }
            }
            IsCollapsed = false;
        }

        public virtual void SetGadgetToProcessingState()
        {
            IsProcessing = true;
        }

        public virtual void SetGadgetToFinishedState()
        {
            object el = FindName("descriptionPanel");
            if (el != null && el is Controls.GadgetDescriptionPanel)
            {
                Controls.GadgetDescriptionPanel descriptionPanel = el as Controls.GadgetDescriptionPanel;

                if (!string.IsNullOrEmpty(descriptionPanel.Text))
                {
                    descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.DisplayMode;
                }
                else
                {
                    descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
                }
            }

            IsProcessing = false;

            if (GadgetProcessingFinished != null)
                GadgetProcessingFinished(this);

            this.Dispatcher.BeginInvoke(new SimpleCallback(CheckForCustomFilters));
        }

        public virtual void ShowConfigPanel()
        {
            object el = this.FindName("ConfigGrid");
            if (el != null && el is Grid)
            {
                Grid configGrid = el as Grid;
                configGrid.Visibility = System.Windows.Visibility.Visible;
            }
        }

        public virtual void HideConfigPanel()
        {
            object el = this.FindName("ConfigGrid");
            if (el != null && el is Grid)
            {
                Grid configGrid = el as Grid;
                configGrid.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        public virtual void ShowHideConfigPanel()
        {
            object el = this.FindName("ConfigGrid");
            if (el != null && el is Grid)
            {
                Grid configGrid = el as Grid;
                if (configGrid.Visibility == System.Windows.Visibility.Collapsed)
                {
                    ShowConfigPanel();
                }
                else
                {
                    HideConfigPanel();
                }
            }
        }

        public virtual void ShowDescriptionPanel()
        {
            object el = this.FindName("panelDescription");
            if (el != null && el is StackPanel)
            {
                StackPanel descPanel = el as StackPanel;
                descPanel.Visibility = System.Windows.Visibility.Visible;
            }
        }

        public virtual void HideDescriptionPanel()
        {
            object el = this.FindName("panelDescription");
            if (el != null && el is StackPanel)
            {
                StackPanel descPanel = el as StackPanel;
                descPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        public virtual void ShowHideDescriptionPanel()
        {
            string gadgetTitle = string.Empty;
            string gadgetDescription = string.Empty;

            GadgetDescriptionPanel descPanel = null;
            Controls.GadgetHeaderPanel headerPanel = null;

            object el1 = this.FindName("descriptionPanel");
            if (el1 != null && el1 is GadgetDescriptionPanel)
            {
                descPanel = el1 as GadgetDescriptionPanel;
                gadgetDescription = descPanel.Text;
            }
            
            object el2 = this.FindName("headerPanel");
            if (el2 != null)
            {
                headerPanel = el2 as Controls.GadgetHeaderPanel;
                gadgetTitle = headerPanel.Text;
            }

            if (descPanel == null || headerPanel == null) return;

            GadgetDescriptionWindow descWindow = new GadgetDescriptionWindow();
            descWindow.GadgetTitle = gadgetTitle;
            descWindow.GadgetDescription = gadgetDescription;
            bool? ok = descWindow.ShowDialog();
            if (ok.HasValue && ok.Value == true)
            {
                descPanel.Text = descWindow.GadgetDescription;
                headerPanel.Text = descWindow.GadgetTitle;

                if (descPanel.Text.Trim().Length > 0)
                {
                    descPanel.PanelMode = GadgetDescriptionPanel.DescriptionPanelMode.DisplayMode;
                }
                else
                {
                    descPanel.PanelMode = GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
                }
            }
        }

        /// <summary>
        /// Forces a refresh of the gadget's output
        /// </summary>
        public virtual void RefreshResults() 
        {
            if (GadgetRefreshed != null)
                GadgetRefreshed(this);
        }
        #endregion Public Methods

        #region Protected Methods
        /// <summary>
        /// Finishes rendering
        /// </summary>
        protected virtual void RenderFinish() { }

        /// <summary>
        /// Finishes rendering with a warning
        /// </summary>
        protected virtual void RenderFinishWithWarning(string errorMessage) { }

        /// <summary>
        /// Finishes rendering with an error
        /// </summary>
        protected virtual void RenderFinishWithError(string errorMessage) { }

        /// <summary>
        /// Adds anchoring information to the Xml used to serialize the gadget
        /// </summary>
        /// <returns>string</returns>
        protected string SerializeAnchors()
        {
            string str = string.Empty;

            string anchorLeft = string.Empty;
            string anchorTop = string.Empty;
            string anchorRight = string.Empty; // unused
            string anchorBottom = string.Empty; // unused

            if (this.AnchorLeft != null)
            {
                anchorLeft = ((IGadget)AnchorLeft).UniqueIdentifier.ToString();
                str = str + "<anchorLeft>" + anchorLeft + "</anchorLeft>";
            }
            if (this.AnchorTop != null)
            {
                anchorTop = ((IGadget)AnchorTop).UniqueIdentifier.ToString();
                str = str + "<anchorTop>" + anchorTop + "</anchorTop>";
            }
            if (this.AnchorRight != null)
            {
                anchorRight = ((IGadget)AnchorRight).UniqueIdentifier.ToString();
                str = str + "<anchorRight>" + anchorRight + "</anchorRight>";
            }
            if (this.AnchorBottom != null)
            {
                anchorBottom = ((IGadget)AnchorBottom).UniqueIdentifier.ToString();
                str = str + "<anchorBottom>" + anchorBottom + "</anchorBottom>";
            }

            return str;
        }

        /// <summary>
        /// Adds anchoring information to the Xml used to serialize the gadget
        /// </summary>
        /// <returns>string</returns>
        protected void SerializeAnchors(XmlElement gadgetElement)
        {
            XmlDocument doc = gadgetElement.OwnerDocument;

            if (this.AnchorLeft != null)
            {
                XmlElement anchorLeft = doc.CreateElement("anchorLeft");
                anchorLeft.InnerText = ((IGadget)AnchorLeft).UniqueIdentifier.ToString();
                gadgetElement.AppendChild(anchorLeft);
            }

            if (this.AnchorTop != null)
            {
                XmlElement anchorTop = doc.CreateElement("anchorTop");
                anchorTop.InnerText = ((IGadget)AnchorTop).UniqueIdentifier.ToString();
                gadgetElement.AppendChild(anchorTop);
            }
        }

        /// <summary>
        /// Adds a new row to a given output grid
        /// </summary>
        /// <param name="strataValue">
        /// The strata value to which this grid cell belongs; used to search the list of grids and 
        /// return the proper System.Windows.Controls.Grid for text insertion.
        /// </param>
        /// <param name="height">The desired height of the grid row</param>
        protected virtual void AddGridRow(string strataValue, int height)
        {
            Grid grid = GetStrataGrid(strataValue);

            object el = FindName("waitPanel");
            if (el is GadgetWaitPanel)
            {
                GadgetWaitPanel waitPanel = el as GadgetWaitPanel;
                waitPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
            grid.Visibility = Visibility.Visible;
            RowDefinition rowDef = new RowDefinition();
            if (height == -1)
            {
                rowDef.Height = GridLength.Auto;
            }
            else
            {
                rowDef.Height = new GridLength(height);
            }
            grid.RowDefinitions.Add(rowDef);
        }

        /// <summary>
        /// Copies a grid's output to the clipboard
        /// </summary>
        protected virtual void CopyToClipboard() { }

        /// <summary>
        /// Used to push a status message to the gadget's status panel
        /// </summary>
        /// <param name="statusMessage">The status message to display</param>
        protected void RequestUpdateStatusMessage(string statusMessage)
        {
            this.Dispatcher.BeginInvoke(new SetStatusDelegate(SetStatusMessage), statusMessage);
        }

        /// <summary>
        /// Draws the borders around a given output grid
        /// </summary>
        /// <param name="strataValue">
        /// The strata value to which this grid cell belongs; used to search the list of grids and 
        /// return the proper System.Windows.Controls.Grid for text insertion.
        /// </param>
        protected virtual void DrawOutputGridBorders(string strataValue)
        {
            Grid grid = GetStrataGrid(strataValue);

            object el = FindName("waitPanel");
            if (el is GadgetWaitPanel)
            {
                GadgetWaitPanel waitPanel = el as GadgetWaitPanel;
                waitPanel.Visibility = System.Windows.Visibility.Collapsed;
            }            

            int rdcount = 0;
            foreach (RowDefinition rd in grid.RowDefinitions)
            {
                int cdcount = 0;
                foreach (ColumnDefinition cd in grid.ColumnDefinitions)
                {
                    Border border = new Border();
                    border.Style = this.Resources["gridCellBorder"] as Style;                                        

                    if (rdcount == 0)
                    {
                        border.BorderThickness = new Thickness(border.BorderThickness.Left, border.BorderThickness.Bottom, border.BorderThickness.Right, border.BorderThickness.Bottom);
                    }
                    if (cdcount == 0)
                    {
                        border.BorderThickness = new Thickness(border.BorderThickness.Right, border.BorderThickness.Top, border.BorderThickness.Right, border.BorderThickness.Bottom);
                    }

                    Grid.SetRow(border, rdcount);
                    Grid.SetColumn(border, cdcount);
                    grid.Children.Add(border);
                    cdcount++;
                }
                rdcount++;
            }
        }

        /// <summary>
        /// Used to sets the gadget's current status, e.g. "Processing results..." or "Displaying output..."
        /// </summary>
        /// <param name="statusMessage">The status message to display</param>
        protected virtual void SetStatusMessage(string statusMessage)
        {
            object el = FindName("messagePanel");
            if (el is GadgetMessagePanel)
            {
                GadgetMessagePanel messagePanel = el as GadgetMessagePanel;
                messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
                messagePanel.Text = statusMessage;
            }
        }

        /// <summary>
        /// Used to check if the user took an action that effectively cancels the currently-running worker
        /// thread. This could include: Closing the gadget, refreshing the dashboard, or selecting another
        /// means variable from the list of variables.
        /// </summary>
        /// <returns>Bool indicating whether or not the gadget's processing thread has been cancelled by the user</returns>
        protected bool IsCancelled()
        {
            if (worker != null && worker.WorkerSupportsCancellation && worker.CancellationPending)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Converts the data filter conditions into XML
        /// </summary>
        /// <param name="doc">The Xml document to modify</param>
        /// <returns>XmlElement</returns>
        protected XmlElement SerializeFilters(XmlDocument doc)
        {
            return this.DataFilters.Serialize(doc);
        }

        protected virtual void SetGadgetStatusMessage(string message)
        {
            if (GadgetStatusUpdate != null)
            {
                this.Dispatcher.BeginInvoke(GadgetStatusUpdate, message);
            }
        }

        protected virtual void CloseGadget()
        {
            if (StrataPanelList != null) StrataPanelList.Clear();
            if (StrataGridList != null) StrataGridList.Clear();
            if (StrataExpanderList != null) StrataExpanderList.Clear();

            object el = this.FindName("headerPanel");
            if (el != null)
            {
                Controls.GadgetHeaderPanel headerPanel = el as Controls.GadgetHeaderPanel;
                headerPanel.GadgetCloseButtonClicked -= new GadgetCloseButtonHandler(CloseGadget);
                headerPanel.GadgetOutputCollapseButtonClicked -= new GadgetOutputCollapseButtonHandler(CollapseOutput);
                headerPanel.GadgetOutputExpandButtonClicked -= new GadgetOutputExpandButtonHandler(ExpandOutput);
                headerPanel.GadgetConfigButtonClicked -= new GadgetConfigButtonHandler(ShowHideConfigPanel);
                headerPanel.GadgetDescriptionButtonClicked -= new GadgetDescriptionButtonHandler(ShowHideDescriptionPanel);
                headerPanel.GadgetFilterButtonClicked -= new GadgetFilterButtonHandler(ShowCustomFilterDialog);
            }

            if (worker != null && worker.WorkerSupportsCancellation)
            {
                worker.CancelAsync();
            }
            if (baseWorker != null && baseWorker.WorkerSupportsCancellation)
            {
                baseWorker.CancelAsync();
            }

            if (GadgetClosing != null)
                GadgetClosing(this);

            GadgetClosing = null;
            GadgetCheckForCancellation = null;
            GadgetProcessingFinished = null;
            GadgetStatusUpdate = null;
            GadgetRefreshed = null;
        }

        protected virtual void ShowCustomFilterDialog()
        {
            Dialogs.RowFilterDialog rfd = new Dialogs.RowFilterDialog(DashboardHelper, Dialogs.FilterDialogMode.RowFilterMode, DataFilters, true);
            if (rfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.DataFilters = rfd.DataFilters;
                CheckForCustomFilters();
                RefreshResults();
            }
        }

        protected void CenterGadget()
        {
            if (this.GadgetReposition != null)
            {
                this.GadgetReposition(this, new GadgetRepositionEventArgs(GadgetRepositionEventArgs.GadgetRepositionType.Center));
            }
        }

        /// <summary>
        /// Checks the gadget's position on the screen and, if necessary, re-sets it so that it is visible. This is used in 
        /// scenarios where the user has moved the gadget to a specific position where its top and/or left positions are outside 
        /// of the canvas boundaries, adds a filter, and the subsequent gadget output is too short to roll onto the visible 
        /// canvas. The gadget is then hidden entirely from the user's view. This will re-set the gadget so that it is visible.
        /// This should only be called from the RenderFinish series of methods.
        /// </summary>
        protected void CheckAndSetPosition()
        {
            double top = Canvas.GetTop(this);
            double left = Canvas.GetLeft(this);

            if (top < 0)
            {
                Canvas.SetTop(this, 0);
            }
            if (left < 0)
            {
                Canvas.SetLeft(this, 0);
            }
        }

        protected void SetAttributesFromXml(XmlElement element)
        {
            System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;

            foreach (XmlAttribute attribute in element.Attributes)
            {
                switch (attribute.Name.ToLower())
                {
                    case "top":
                        string top = attribute.Value.Replace(',', '.');
                        int topP = top.IndexOf('.');
                        if (topP >= 0)
                        {
                            top = top.Substring(0, topP);
                        }
                        Canvas.SetTop(this, double.Parse(top, culture));
                        break;
                    case "left":
                        string left = attribute.Value.Replace(',', '.');
                        int leftP = left.IndexOf('.');
                        if (leftP >= 0)
                        {
                            left = left.Substring(0, leftP);
                        }
                        Canvas.SetLeft(this, double.Parse(left, culture));
                        break;
                    case "id":
                        if (attribute.Value == "00000000-0000-0000-0000-000000000000")
                        {
                            this.UniqueIdentifier = new Guid();
                        }
                        else
                        {
                            this.UniqueIdentifier = new Guid(attribute.Value);
                        }
                        break;
                    case "collapsed":
                        if (attribute.Value.ToLower().Equals("true"))
                        {
                            IsCollapsed = true;
                            CollapseOutput();
                        }                        
                        break;
                }
            }
        }

        protected virtual void SetCustomFilterDisplay()
        {
            object el = this.FindName("txtFilterString");
            if (el != null && el is TextBlock)
            {
                TextBlock txtFilter = el as TextBlock;
                if (this.DataFilters != null && this.DataFilters.Count > 0)
                {
                    txtFilter.MaxWidth = this.ActualWidth;
                    txtFilter.Text = string.Format(DashboardSharedStrings.GADGET_CUSTOM_FILTER, this.DataFilters.GenerateReadableDataFilterString());
                    txtFilter.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    txtFilter.Text = string.Empty;
                    txtFilter.Visibility = System.Windows.Visibility.Collapsed;
                }
            }

            el = this.FindName("infoPanel");
            if (el != null && el is GadgetInfoPanel)
            {
                GadgetInfoPanel panel = el as GadgetInfoPanel;
                if (this.DataFilters != null && this.DataFilters.Count > 0)
                {
                    panel.MaxWidth = this.ActualWidth;
                    panel.Text = string.Format(DashboardSharedStrings.GADGET_CUSTOM_FILTER, this.DataFilters.GenerateReadableDataFilterString());
                    panel.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    panel.Text = string.Empty;
                    panel.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        private void FilterTimerElapsed(object source, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new SimpleCallback(SetCustomFilterDisplay));
            if (source is System.Timers.Timer)
            {
                System.Timers.Timer timer = source as System.Timers.Timer;
                timer.Stop();
                timer = null;
            }
        }

        private void CheckForCustomFilters()
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(FilterTimerElapsed);

            timer.Interval = 300;
            timer.Start();
        }

        protected virtual Grid GetStrataGrid(string strataValue)
        {
            Grid grid = new Grid();

            foreach (Grid g in StrataGridList)
            {
                if (g.Tag.Equals(strataValue))
                {
                    grid = g;
                    break;
                }
            }

            return grid;
        }

        protected virtual Expander GetStrataExpander(string strataValue)
        {
            Expander expander = new Expander();

            foreach (Expander e in StrataExpanderList)
            {
                if (e.Tag.Equals(strataValue))
                {
                    expander = e;
                    break;
                }
            }

            return expander;
        }

        /// <summary>
        /// Sends the gadget to the back of the canvas
        /// </summary>
        protected void SendToBack()
        {
            Canvas.SetZIndex(this, -1);
        }

        protected virtual FieldFlags SetFieldFlags(Epi.Fields.RenderableField field)
        {
            FieldFlags flags = new FieldFlags(false, false, false, false);

            if (field is TableBasedDropDownField || field is YesNoField || field is CheckBoxField)
            {
                flags.IsDropDownListField = true;
                if (field is DDLFieldOfCommentLegal)
                {
                    flags.IsCommentLegalField = true;
                }
            }
            else if (field is OptionField)
            {
                flags.IsOptionField = true;
            }

            return flags;
            //    if (DashboardHelper.IsUserDefinedColumn(cbxField.SelectedItem.ToString()))
            //    {
            //        List<IDashboardRule> associatedRules = DashboardHelper.Rules.GetRules(cbxField.SelectedItem.ToString());
            //        foreach (IDashboardRule rule in associatedRules)
            //        {
            //            if (rule is Rule_Recode)
            //            {
            //                isRecoded = true;
            //            }
            //        }
            //    }
            //}
        }

        /// <summary>
        /// Constructs the gadget
        /// </summary>
        protected virtual void Construct()
        {
            IsCollapsed = false;
            StrataGridList = new List<Grid>();
            StrataPanelList = new List<StackPanel>();
            StrataExpanderList = new List<Expander>();
            strataCount = 0;

            

            this.MouseEnter += new MouseEventHandler(GadgetBase_MouseEnter);
            this.MouseLeave += new MouseEventHandler(GadgetBase_MouseLeave);

            if (this.DashboardHelper != null)
            {
                this.DataFilters = new EpiDashboard.DataFilters(this.DashboardHelper);
            }

            object el = this.FindName("headerPanel");
            if (el != null)
            {
                Controls.GadgetHeaderPanel headerPanel = el as Controls.GadgetHeaderPanel;
                headerPanel.GadgetCloseButtonClicked += new GadgetCloseButtonHandler(CloseGadget);
                headerPanel.GadgetOutputCollapseButtonClicked += new GadgetOutputCollapseButtonHandler(CollapseOutput);
                headerPanel.GadgetOutputExpandButtonClicked += new GadgetOutputExpandButtonHandler(ExpandOutput);
                headerPanel.GadgetConfigButtonClicked += new GadgetConfigButtonHandler(ShowHideConfigPanel);
                headerPanel.GadgetDescriptionButtonClicked += new GadgetDescriptionButtonHandler(ShowHideDescriptionPanel);
                headerPanel.GadgetFilterButtonClicked += new GadgetFilterButtonHandler(ShowCustomFilterDialog);
            }

            el = this.FindName("messagePanel");
            if (el != null)
            {
                Controls.GadgetMessagePanel messagePanel = el as Controls.GadgetMessagePanel;
                messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
                messagePanel.Text = string.Empty;
                messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            }

            el = this.FindName("descriptionPanel");
            if (el != null)
            {
                Controls.GadgetDescriptionPanel descriptionPanel = el as Controls.GadgetDescriptionPanel;
                if (!string.IsNullOrEmpty(CustomOutputDescription) && !CustomOutputHeading.Equals("(none)"))
                {
                    descriptionPanel.Text = CustomOutputDescription;
                    descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.DisplayMode;
                }
                else
                {
                    descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
                }
            }
        }

        void GadgetBase_MouseLeave(object sender, MouseEventArgs e)
        {
            //object el1 = FindName("headerPanel");
            //object el2 = FindName("borderAll");
            //object el3 = FindName("rectangleShadow");

            //if (el1 != null && el2 != null && el3 != null)
            //{
            //    GadgetHeaderPanel headerPanel = el1 as GadgetHeaderPanel;
            //    Border border = el2 as Border;
            //    ShadowRectangle rectangle = el3 as ShadowRectangle;

            //    Storyboard storyboard = new Storyboard();
            //    TimeSpan duration = new TimeSpan(0, 0, 0, 0, 100);

            //    DoubleAnimation animation = new DoubleAnimation();
            //    animation.From = 1.0;
            //    animation.To = 0.0;
            //    animation.Duration = new Duration(duration);
            //    Storyboard.SetTargetName(animation, headerPanel.Name);
            //    Storyboard.SetTargetProperty(animation, new PropertyPath(Control.OpacityProperty));
            //    storyboard.Children.Add(animation);
            //    storyboard.Begin(this);

            //    animation = new DoubleAnimation();
            //    animation.From = 1.0;
            //    animation.To = 0.0;
            //    animation.Duration = new Duration(duration);
            //    Storyboard.SetTargetName(animation, rectangle.Name);
            //    Storyboard.SetTargetProperty(animation, new PropertyPath(Control.OpacityProperty));
            //    storyboard.Children.Add(animation);
            //    storyboard.Begin(this);
            //}
        }

        void GadgetBase_MouseEnter(object sender, MouseEventArgs e)
        {
            //object el1 = FindName("headerPanel");
            //object el2 = FindName("borderAll");
            //object el3 = FindName("rectangleShadow");            

            //if (el1 != null && el2 != null && el3 != null)
            //{
            //    GadgetHeaderPanel headerPanel = el1 as GadgetHeaderPanel;
            //    Border border = el2 as Border;
            //    ShadowRectangle rectangle = el3 as ShadowRectangle;                

            //    if (headerPanel.Opacity < 1.0)
            //    {
            //        TimeSpan duration = new TimeSpan(0, 0, 0, 0, 200);

            //        Storyboard storyboard1 = new Storyboard();
            //        DoubleAnimation animation1 = new DoubleAnimation();
            //        animation1.From = 0.0;
            //        animation1.To = 1.0;
            //        animation1.Duration = new Duration(duration);
            //        Storyboard.SetTargetName(animation1, headerPanel.Name);
            //        Storyboard.SetTargetProperty(animation1, new PropertyPath(Control.OpacityProperty));
            //        storyboard1.Children.Add(animation1);
            //        storyboard1.Begin(this);

            //        Storyboard storyboard2 = new Storyboard();
            //        DoubleAnimation animation2 = new DoubleAnimation();
            //        animation2.BeginTime = new TimeSpan(0, 0, 0, 0, 200);
            //        animation2.From = 0.0;
            //        animation2.To = 1.0;
            //        animation2.Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200));
            //        Storyboard.SetTargetName(animation2, rectangle.Name);
            //        Storyboard.SetTargetProperty(animation2, new PropertyPath(Control.OpacityProperty));
            //        storyboard2.Children.Add(animation2);
            //        storyboard2.Begin(this);

            //        //Storyboard storyboard3 = new Storyboard();
            //        //ColorAnimation animation3 = new ColorAnimation(backgroundBrush.Color, Colors.Transparent, new Duration(duration), FillBehavior.HoldEnd);                    
            //        //storyboard3.Children.Add(animation3);
            //        //storyboard3.Begin(this);
            //    }
            //}
        }

        protected void TurnOnBorders()
        {
            object borderElement = FindName("borderAll");
            if (borderElement != null)
            {
                Border border = borderElement as Border;
                border.Style = this.Resources["mainGadgetBorder"] as Style;
            }

            object headerPanelElement = FindName("headerPanel");
            if (headerPanelElement != null)
            {
                Controls.GadgetHeaderPanel header = headerPanelElement as Controls.GadgetHeaderPanel;
                //header.Visibility = System.Windows.Visibility.Visible;
                header.ShowButtons();
            }

            object shadowRectangleElement = FindName("rectangleShadow");
            if (shadowRectangleElement != null)
            {
                ShadowRectangle rectangle = shadowRectangleElement as ShadowRectangle;
                rectangle.Visibility = System.Windows.Visibility.Visible;
            }

            object infoPanel = FindName("infoPanel");
            if (infoPanel != null)
            {
                GadgetInfoPanel panel = infoPanel as GadgetInfoPanel;
                if (!string.IsNullOrEmpty(panel.Text))
                {
                    panel.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        protected void TurnOffBorders()
        {
            object borderElement = FindName("borderAll");
            if (borderElement != null)
            {
                Border border = borderElement as Border;
                border.Style = null;
            }

            object headerPanelElement = FindName("headerPanel");
            if (headerPanelElement != null)
            {
                Controls.GadgetHeaderPanel header = headerPanelElement as Controls.GadgetHeaderPanel;
                //header.Visibility = System.Windows.Visibility.Hidden;
                header.HideButtons();
            }

            object shadowRectangleElement = FindName("rectangleShadow");
            if (shadowRectangleElement != null)
            {
                ShadowRectangle rectangle = shadowRectangleElement as ShadowRectangle;
                rectangle.Visibility = System.Windows.Visibility.Hidden;
            }

            object infoPanel = FindName("infoPanel");
            if (infoPanel != null)
            {
                GadgetInfoPanel panel = infoPanel as GadgetInfoPanel;
                if (panel.Visibility == System.Windows.Visibility.Visible)
                {
                    panel.Visibility = System.Windows.Visibility.Hidden;
                }
            }
        }

        #endregion // Protected Methods

        #region Event Handlers
        /// <summary>
        /// Handles the preview mouse move event for the gadget
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void GadgetBase_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (IsBeingDragged)
            {
                if (this.GadgetDrag != null)
                {
                    GadgetDrag(this);
                }
            }
        }

        /// <summary>
        /// Handles the preview mouse up event for the gadget
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void GadgetBase_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            IsBeingDragged = false;
            if (this.GadgetDragStop != null)
            {
                GadgetDragStop(this);
            }
        }

        /// <summary>
        /// Handles the preview mouse down event for the gadget
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void GadgetBase_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            IsBeingDragged = true;
            if (this.GadgetDragStart != null)
            {
                GadgetDragStart(this);
            }
        }

        /// <summary>
        /// Handles the click event for the 'Copy to clipboard' context menu option
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected void mnuCopy_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard();
        }

        /// <summary>
        /// Handles the click event for the 'Clone Gadget' context menu option
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected void mnuClone_Click(object sender, RoutedEventArgs e)
        {
            GadgetBase gadget = (GadgetBase)this.MemberwiseClone();
            gadget.UniqueIdentifier = Guid.NewGuid();

            ((((this.Parent as DragCanvas).Parent as ScrollViewer).Parent as Grid).Parent as DashboardControl).CreateClone(gadget);
        }

        /// <summary>
        /// Handles the click event for the 'Send data to web browser' context menu option
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected void mnuSendDataToExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString("N") + ".html";//GetHTMLLineListing();

                System.IO.FileStream stream = System.IO.File.OpenWrite(fileName);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(stream);
                sw.WriteLine("<html><head>");
                sw.WriteLine("<meta http-equiv=\"content-type\" content=\"text/html;charset=UTF-8\" />");
                sw.WriteLine("<meta name=\"author\" content=\"Epi Info 7\" />");
                sw.WriteLine(DashboardHelper.GenerateStandardHTMLStyle().ToString());

                sw.WriteLine(this.ToHTML());
                sw.Close();
                sw.Dispose();

                if (!string.IsNullOrEmpty(fileName))
                {
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.FileName = "excel";
                    proc.StartInfo.Arguments = "\"" + fileName + "\"";
                    proc.StartInfo.UseShellExecute = true;
                    proc.Start();
                }
            }
            finally
            {
            }
        }

        /// <summary>
        /// Handles the click event for the 'Send data to web browser' context menu option
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected void mnuSendDataToHTML_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString("N") + ".html";//GetHTMLLineListing();

                System.IO.FileStream stream = System.IO.File.OpenWrite(fileName);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(stream);
                sw.WriteLine("<html><head><title>" + this.ToString() + "</title>");
                sw.WriteLine("<meta http-equiv=\"content-type\" content=\"text/html;charset=UTF-8\" />");
                sw.WriteLine("<meta name=\"author\" content=\"Epi Info 7\" />");
                sw.WriteLine(DashboardHelper.GenerateStandardHTMLStyle().ToString());

                sw.WriteLine(this.ToHTML(fileName));
                sw.Close();
                sw.Dispose();

                if (!string.IsNullOrEmpty(fileName))
                {
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.FileName = fileName;
                    proc.StartInfo.UseShellExecute = true;
                    proc.Start();
                }
            }
            finally
            {
            }
        }

        protected void mnuSendToBack_Click(object sender, RoutedEventArgs e)
        {
            SendToBack();
        }

        protected void mnuRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshResults();
        }

        protected void mnuClose_Click(object sender, RoutedEventArgs e)
        {
           // CloseGadget();
            
          MessageBoxResult result = MessageBox.Show(DashboardSharedStrings.WARNING_CONFIRM_CLOSE, DashboardSharedStrings.WARNING, MessageBoxButton.YesNo, MessageBoxImage.Warning);
        
           if (result == MessageBoxResult.Yes)
            {
                CloseGadget();
            }
        }

        protected virtual void txtInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool isNumPadNumeric = (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || e.Key == Key.Decimal;
            bool isNumeric = (e.Key >= Key.D0 && e.Key <= Key.D9) || e.Key == Key.OemPeriod;

            if (System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ",")
            {
                isNumeric = (e.Key >= Key.D0 && e.Key <= Key.D9) || e.Key == Key.OemComma;
            }

            if ((isNumeric || isNumPadNumeric) && Keyboard.Modifiers != ModifierKeys.None)
            {
                e.Handled = true;
                return;
            }
            bool isControl = ((Keyboard.Modifiers != ModifierKeys.None && Keyboard.Modifiers != ModifierKeys.Shift) || e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Insert || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Tab || e.Key == Key.PageDown || e.Key == Key.PageUp || e.Key == Key.Enter || e.Key == Key.Return || e.Key == Key.Escape || e.Key == Key.Home || e.Key == Key.End);
            e.Handled = !isControl && !isNumeric && !isNumPadNumeric;
        }

        protected virtual void txtInput_PositiveIntegerOnly_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool isNumPadNumeric = (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9);
            bool isNumeric = (e.Key >= Key.D0 && e.Key <= Key.D9);

            if ((isNumeric || isNumPadNumeric) && Keyboard.Modifiers != ModifierKeys.None)
            {
                e.Handled = true;
                return;
            }
            bool isControl = ((Keyboard.Modifiers != ModifierKeys.None && Keyboard.Modifiers != ModifierKeys.Shift) || e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Insert || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Tab || e.Key == Key.PageDown || e.Key == Key.PageUp || e.Key == Key.Enter || e.Key == Key.Return || e.Key == Key.Escape || e.Key == Key.Home || e.Key == Key.End);
            e.Handled = !isControl && !isNumeric && !isNumPadNumeric;
        }

        protected virtual void txtInput_IntegerOnly_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool isNumPadNumeric = (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || e.Key == Key.OemMinus;
            bool isNumeric = (e.Key >= Key.D0 && e.Key <= Key.D9);

            if ((isNumeric || isNumPadNumeric) && Keyboard.Modifiers != ModifierKeys.None)
            {
                e.Handled = true;
                return;
            }
            bool isControl = ((Keyboard.Modifiers != ModifierKeys.None && Keyboard.Modifiers != ModifierKeys.Shift) || e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Insert || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Tab || e.Key == Key.PageDown || e.Key == Key.PageUp || e.Key == Key.Enter || e.Key == Key.Return || e.Key == Key.Escape || e.Key == Key.Home || e.Key == Key.End);
            e.Handled = !isControl && !isNumeric && !isNumPadNumeric;
        }

        /// <summary>
        /// Handles the Loaded event for the gadget.
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void GadgetBase_Loaded(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Tells the gadget to begin processing output on a new worker thread
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected void Execute(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (worker != null && worker.WorkerSupportsCancellation)
            {
                worker.CancelAsync();
            }

            lock (syncLock)
            {
                worker = new System.ComponentModel.BackgroundWorker();
                worker.WorkerSupportsCancellation = true;
                worker.DoWork += new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
                worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(worker_WorkerCompleted);
                worker.RunWorkerAsync(this.GadgetOptions);
            }
        }

        /// <summary>
        /// Handles the WorkerCompleted event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected virtual void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {            
        }

        /// <summary>
        /// Handles the DoWorker event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected virtual void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {            
        }
        #endregion // Event Handlers

        public virtual void GadgetBase_SizeChanged(double width, double height)
        {
        }




    }
}
