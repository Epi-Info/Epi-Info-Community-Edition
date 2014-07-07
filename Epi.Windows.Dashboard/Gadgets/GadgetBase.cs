using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Epi.WPF.Dashboard;
using Epi.WPF.Dashboard.Controls;

namespace Epi.WPF.Dashboard.Gadgets
{
    /// <summary>
    /// A base class for Dashboard gadgets.
    /// </summary>
    public class GadgetBase : UserControl, IGadget
    {
        #region Protected Members
        /// <summary>
        /// The data filters object handles any filters specific to the gadget.
        /// </summary>
        protected DataFilters dataFilters;

        protected DashboardHelper dashboardHelper;        

        /// <summary>
        /// For stratifications, each strata value will result in one output grid (typically). This list
        /// maintains those strata grids.
        /// </summary>
        protected List<Grid> strataGridList;

        /// <summary>
        /// List of panels in stratifications
        /// </summary>
        protected List<StackPanel> strataPanelList;

        private bool drawBorders = true;

        /// <summary>
        /// The list of expanders for each group value.
        /// </summary>
        protected List<Expander> strataExpanderList; 

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

        /// <summary>
        /// Bool used to determine whether or not to run events on the combo boxes
        /// </summary>
        private bool loadingCombos; 

        /// <summary>
        /// The object that is passed into the Dashboard Helper and contains the options the user selected.
        /// For example, this will contain the columns for the list variables, sort variables, group variable,
        /// and any other parameters set in the gadget UI.
        /// </summary>
        private GadgetParameters gadgetOptions;

        //protected Border borderAll = new Border();

        /// <summary>
        /// Bool used to determine if the gadget is currently processing results/calculating statistics
        /// </summary>
        private bool isProcessing;

        #endregion // Private Members

        #region Events

        public event GadgetCheckForCancellationHandler GadgetCheckForCancellation;
        public event GadgetClosingHandler GadgetClosing;
        public event GadgetProcessingFinishedHandler GadgetProcessingFinished;
        public event GadgetRefreshedHandler GadgetRefreshed;
        public event GadgetRepositionEventHandler GadgetReposition;
        public event GadgetStatusUpdateHandler GadgetStatusUpdate;

        #endregion // Events

        #region Delegates

        protected delegate void SetGridTextDelegate(string strataValue, TextBlockConfig textBlockConfig, FontWeight fontWeight);        
        protected delegate void AddOutputGridDelegate(string strataVar, string value, int columnCount);        
        protected delegate void RenderOutputGridHeaderDelegate(string strataValue, string freqVar);
        protected delegate void SetGridBarDelegate(string strataValue, int rowNumber, double pct);
        protected delegate void AddGridRowDelegate(string strataValue, int height);
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

        /// <summary>
        /// Gets the data filters associated with this gadget
        /// </summary>
        public DataFilters DataFilters
        {
            get
            {
                return this.dataFilters;
            }
        }

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
        public bool IsProcessing
        {
            get
            {
                return isProcessing;
            }
            set
            {
                isProcessing = value;
            }
        }
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
        protected DashboardHelper DashboardHelper
        {
            get
            {
                return this.dashboardHelper;
            }
        }

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
        /// Gets/sets the gadget options
        /// </summary>
        protected GadgetParameters GadgetOptions
        {
            get
            {
                return this.gadgetOptions;
            }
            set
            {
                this.gadgetOptions = value;
            }
        }

        /// <summary>
        /// Gets/sets whether the gadget is loading combo boxes.
        /// </summary>
        protected bool LoadingCombos
        {
            get
            {
                return this.loadingCombos;
            }
            set
            {
                this.loadingCombos = value;
            }
        }
        #endregion // Protected Properties

        #region Constructors
        public GadgetBase()
        {
            this.Loaded += new RoutedEventHandler(GadgetBase_Loaded);
        }
        #endregion // Constructors

        #region Public Methods
        public virtual XmlNode Serialize(XmlDocument doc) { return null; }
        public virtual void CreateFromXml(XmlElement element) { }
        public virtual string ToHTML(string htmlFileName = "", int count = 0) { return string.Empty; }
        public virtual void UpdateVariableNames() { }
        public virtual void CollapseOutput() { }
        public virtual void ExpandOutput() { }

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
            object el = this.FindName("descriptionPanel");
            if (el != null && el is GadgetDescriptionPanel)
            {
                GadgetDescriptionPanel descPanel = el as GadgetDescriptionPanel;
                if (descPanel.PanelMode == GadgetDescriptionPanel.DescriptionPanelMode.Collapsed)
                {
                    if (string.IsNullOrEmpty(descPanel.Text))
                    {
                        descPanel.PanelMode = GadgetDescriptionPanel.DescriptionPanelMode.EditMode;
                    }
                    else
                    {
                        descPanel.PanelMode = GadgetDescriptionPanel.DescriptionPanelMode.DisplayMode;
                    }
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
            strataPanelList.Clear();
            strataGridList.Clear();
            strataExpanderList.Clear();

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
            //Dialogs.RowFilterDialog rfd = new Dialogs.RowFilterDialog(this.dashboardHelper, Dialogs.FilterDialogMode.RowFilterMode, dataFilters, false);
            //if (rfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    this.dataFilters = rfd.DataFilters;
            //    CheckForCustomFilters();
            //    RefreshResults();
            //}
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

        protected void SetPositionFromXml(XmlElement element)
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

            foreach (Grid g in strataGridList)
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

            foreach (Expander e in strataExpanderList)
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

        /// <summary>
        /// Constructs the gadget
        /// </summary>
        protected virtual void Construct()
        {
            strataGridList = new List<Grid>();
            strataPanelList = new List<StackPanel>();
            strataExpanderList = new List<Expander>();
            strataCount = 0;

            GadgetOptions = new GadgetParameters();
            GadgetOptions.InputVariableList = new Dictionary<string, string>();
            GadgetOptions.ShouldIncludeFullSummaryStatistics = false;
            GadgetOptions.ShouldIncludeMissing = false;
            GadgetOptions.ShouldSortHighToLow = false;
            GadgetOptions.ShouldUseAllPossibleValues = false;
            GadgetOptions.StrataVariableNames = new List<string>();
            GadgetOptions.PSUVariableName = string.Empty;
            GadgetOptions.CustomFilter = string.Empty;

            if (this.DashboardHelper != null)
            {
                this.dataFilters = new Epi.WPF.Dashboard.DataFilters(this.DashboardHelper);
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
                header.Visibility = System.Windows.Visibility.Visible;
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
                header.Visibility = System.Windows.Visibility.Hidden;
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
        /// Handles the click event for the 'Copy to clipboard' context menu option
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected void mnuCopy_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard();
        }

        /// <summary>
        /// Handles the click event for the 'Send data to web browser' context menu option
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected void mnuSendDataToExcel_Click(object sender, RoutedEventArgs e)
        {
            //if (this.strataGridList.Count == 0)
            //{
            //    return;
            //}

            try
            {
                string fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString("N") + ".html";//GetHTMLLineListing();

                System.IO.FileStream stream = System.IO.File.OpenWrite(fileName);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(stream);
                sw.WriteLine("<html><head>");
                sw.WriteLine("<meta http-equiv=\"content-type\" content=\"text/html;charset=UTF-8\" />");
                sw.WriteLine("<meta name=\"author\" content=\"Epi Info 7\" />");
                sw.WriteLine(dashboardHelper.GenerateStandardHTMLStyle().ToString());

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
            //if (this.strataGridList.Count == 0)
            //{
            //    return;
            //}

            try
            {
                string fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString("N") + ".html";//GetHTMLLineListing();

                System.IO.FileStream stream = System.IO.File.OpenWrite(fileName);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(stream);
                sw.WriteLine("<html><head><title>Line List</title>");
                sw.WriteLine("<meta http-equiv=\"content-type\" content=\"text/html;charset=UTF-8\" />");
                sw.WriteLine("<meta name=\"author\" content=\"Epi Info 7\" />");
                sw.WriteLine(dashboardHelper.GenerateStandardHTMLStyle().ToString());

                sw.WriteLine(this.ToHTML());
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
            CloseGadget();
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
                worker.RunWorkerAsync(gadgetOptions);
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
    }
}
