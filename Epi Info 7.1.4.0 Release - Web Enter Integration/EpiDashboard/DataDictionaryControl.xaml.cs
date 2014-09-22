using System;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml;
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
using Epi;
using Epi.Data;
using Epi.Fields;
using EpiDashboard.Rules;

namespace EpiDashboard
{
    /// <summary>
    /// Interaction logic for DataDictionaryControl.xaml
    /// </summary>
    public partial class DataDictionaryControl : UserControl
    {
        #region Private Members
        /// <summary>
        /// The dashboard helper that is attached to this gadget; provides database access and many 
        /// method calls that are needed to populate variable lists and get variable types
        /// </summary>
        private DashboardHelper dashboardHelper;

        /// <summary>
        /// The worker thread that the data processing is intended to run on; by using a separate thread,
        /// the user can continue working in the dashboard UI as the gadget is processing the line list.
        /// </summary>
        private BackgroundWorker worker;

        /// <summary>
        /// The base worker thread, which calls the 'worker' thread. Required for gadget cancellation.
        /// </summary>
        private BackgroundWorker baseWorker;

        /// <summary>
        /// Required to 'lock' the worker thread so that you can't spawn more than one worker thread
        /// in the gadget at a single time.
        /// </summary>
        private object syncLock = new object();

        /// <summary>
        /// Bool used to determine if the gadget is currently processing results/calculating statistics
        /// </summary>
        private bool isProcessing;

        /// <summary>
        /// Bool used to determine whether the gadget config panel has been collapsed or not
        /// </summary>
        private bool triangleCollapsed;

        /// <summary>
        /// A data structure used for managing the configuration of text blocks within the output grid
        /// </summary>
        private struct TextBlockConfig
        {
            public string Text;
            public Thickness Margin;
            public VerticalAlignment VerticalAlignment;
            public HorizontalAlignment HorizontalAlignment;
            public int ColumnNumber;
            public int RowNumber;
            public Visibility ControlVisibility;

            public TextBlockConfig(string text, Thickness margin, VerticalAlignment verticalAlignment, HorizontalAlignment horizontalAlignment, int rowNumber, int columnNumber, Visibility controlVisibility)
            {
                this.Text = text;
                this.Margin = margin;
                this.VerticalAlignment = verticalAlignment;
                this.HorizontalAlignment = horizontalAlignment;
                this.RowNumber = rowNumber;
                this.ColumnNumber = columnNumber;
                this.ControlVisibility = controlVisibility;
            }
        }

        #endregion // Private Members

        #region Delegates

        private delegate void SetGridTextDelegate(string strataValue, TextBlockConfig textBlockConfig, FontWeight fontWeight);
        private delegate void SetGridImageDelegate(string strataValue, byte[] imageBlob, TextBlockConfig textBlockConfig, FontWeight fontWeight);
        private delegate void AddLineListGridDelegate(string strataVar, string value, int columnCount);
        private delegate void RenderFrequencyHeaderDelegate(string strataValue, string freqVar, DataColumnCollection columns);
        private delegate void SetGridBarDelegate(string strataValue, int rowNumber, double pct);
        private delegate void AddGridRowDelegate(string strataValue, int height);
        private delegate void AddGridFooterDelegate(string strataValue, int rowNumber, int[] totalRows);
        private delegate void DrawFrequencyBordersDelegate(string strataValue);
        private delegate void SetStatusDelegate(string statusMessage);
        private delegate void RequestUpdateStatusDelegate(string statusMessage);
        private delegate bool CheckForCancellationDelegate();

        private delegate void RenderFinishWithErrorDelegate(string errorMessage);
        private delegate void RenderFinishWithWarningDelegate(string errorMessage);
        private delegate void SimpleCallback();

        #endregion

        #region Events
        public event GadgetClosingHandler GadgetClosing;
        public event GadgetProcessingFinishedHandler GadgetProcessingFinished;
        public event GadgetStatusUpdateHandler GadgetStatusUpdate;
        public event GadgetCheckForCancellationHandler GadgetCheckForCancellation;
        #endregion // Events

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public DataDictionaryControl()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper object to attach</param>
        /// <param name="hostedByEnter">Whether the control is hosted by Enter</param>
        public DataDictionaryControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.dashboardHelper = dashboardHelper;
            Construct();
            FillComboboxes();
        }

        #endregion // Constructors

        #region Public Methods
        /// <summary>
        /// Copies a grid's output to the clipboard
        /// </summary>
        private void CopyToClipboard()
        {
            StringBuilder sb = new StringBuilder();

            //foreach (Grid grid in this.groupGridList)
            //{

            foreach (UIElement control in grdDictionary.Children)
            {
                if (control is TextBlock)
                {
                    int columnNumber = Grid.GetColumn(control);
                    string value = ((TextBlock)control).Text;

                    sb.Append(value + "\t");

                    if (columnNumber >= grdDictionary.ColumnDefinitions.Count - 1)
                    {
                        sb.AppendLine();
                    }
                }
            }

            sb.AppendLine();
            //}
            sb.Replace("\n", "");
            Clipboard.Clear();
            Clipboard.SetText(sb.ToString());
        }

        /// <summary>
        /// Clears the gadget's output
        /// </summary>
        public void ClearResults()
        {
            txtStatus.Text = string.Empty;
            pnlStatus.Visibility = Visibility.Collapsed;
            waitCursor.Visibility = Visibility.Visible;
            
            grdDictionary.Children.Clear();
            grdDictionary.RowDefinitions.Clear();
        }
        #endregion // Public Methods

        #region Event Handlers

        /// <summary>
        /// Fires the 'Close' button event that will remove the gadget from the dashboard
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void imgClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseGadget();
        }

        /// <summary>
        /// Fired when the user removes the mouse cursor from inside of the close button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void imgClose_MouseLeave(object sender, MouseEventArgs e)
        {
            Uri uriSource = new Uri("Images/x.png", UriKind.Relative);
            imgClose.Source = new BitmapImage(uriSource);
        }

        /// <summary>
        /// Fired when the user places the mouse cursor inside of the close button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void imgClose_MouseEnter(object sender, MouseEventArgs e)
        {
            Uri uriSource = new Uri("Images/x_over.png", UriKind.Relative);
            imgClose.Source = new BitmapImage(uriSource);
        }

        /// <summary>
        /// Event used to handle the gadget's configuration collapse/expand arrow
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void Triangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement source = sender as FrameworkElement;
            CollapseExpandConfigPanel();
        }

        /// <summary>
        /// Tells the gadget to begin processing output on a new worker thread
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void Execute(object sender, System.ComponentModel.DoWorkEventArgs e)
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
                worker.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Handles the WorkerCompleted event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Debug.Print("Background worker thread for line list gadget was cancelled or ran to completion.");
        }

        /// <summary>
        /// Handles the DoWorker event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (syncLock)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));     
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));

                AddLineListGridDelegate addGrid = new AddLineListGridDelegate(AddLineListGrid);
                SetGridTextDelegate setText = new SetGridTextDelegate(SetGridText);
                SetGridImageDelegate setImage = new SetGridImageDelegate(SetGridImage);
                AddGridRowDelegate addRow = new AddGridRowDelegate(AddGridRow);
                RenderFrequencyHeaderDelegate renderHeader = new RenderFrequencyHeaderDelegate(RenderFrequencyHeader);
                DrawFrequencyBordersDelegate drawBorders = new DrawFrequencyBordersDelegate(DrawFrequencyBorders);

                Configuration config = dashboardHelper.Config;
                string yesValue = config.Settings.RepresentationOfYes;
                string noValue = config.Settings.RepresentationOfNo;

                try
                {
                    DataTable dictionaryTable = dashboardHelper.FieldTable.Copy();

                    foreach (KeyValuePair<string, string> kvp in dashboardHelper.TableColumnNames)
                    {
                        DataRow row = dictionaryTable.Rows.Find(kvp.Key);
                        if(row == null)
                        {
                            dictionaryTable.Rows.Add(kvp.Key, kvp.Value);
                        }
                    }

                    if (dashboardHelper.IsUsingEpiProject)
                    {
                        dictionaryTable.Columns.Add("Page", typeof(int));
                        dictionaryTable.Columns.Add("Tab", typeof(int));
                        dictionaryTable.Columns.Add("Prompt", typeof(string));
                        dictionaryTable.Columns.Add("Items", typeof(string));

                        foreach (DataRow fieldRow in dictionaryTable.Rows)
                        {
                            if (fieldRow["epifieldtype"] is RenderableField)
                            {
                                RenderableField renderableField = fieldRow["epifieldtype"] as RenderableField;
                                fieldRow["Page"] = renderableField.Page.Position + 1;
                                fieldRow["Tab"] = renderableField.TabIndex;
                                fieldRow["Prompt"] = renderableField.PromptText;
                                if (renderableField is GroupField)
                                {
                                    GroupField groupField = renderableField as GroupField;
                                    fieldRow["Items"] = groupField.ChildFieldNames;
                                }
                                else if (renderableField is OptionField)
                                {
                                    OptionField optionField = renderableField as OptionField;
                                    fieldRow["Items"] = optionField.GetOptionsString();
                                }
                            }
                        }

                        dictionaryTable.Columns["columnname"].SetOrdinal(0);
                        dictionaryTable.Columns["Prompt"].SetOrdinal(1);
                        dictionaryTable.Columns["formname"].SetOrdinal(2);
                        dictionaryTable.Columns["Page"].SetOrdinal(3);
                        dictionaryTable.Columns["Tab"].SetOrdinal(4);
                        dictionaryTable.Columns["datatype"].SetOrdinal(5);
                        dictionaryTable.Columns["epifieldtype"].SetOrdinal(6);
                        dictionaryTable.Columns["tablename"].SetOrdinal(7);
                        dictionaryTable.Columns["Items"].SetOrdinal(8);
                    }

                    if (dictionaryTable == null || dictionaryTable.Rows.Count == 0)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), "There are no valid fields to display."); 
                    }
                    else if (worker.CancellationPending)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.DASHBOARD_GADGET_STATUS_OPERATION_CANCELLED);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        Debug.Print("Data dictionary thread cancelled");
                        return;
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(addGrid, "", "", dictionaryTable.Columns.Count);
                        string formatString = string.Empty;
                        this.Dispatcher.BeginInvoke(renderHeader, "", "", dictionaryTable.Columns);

                        int rowCount = 1;
                        int columnCount = 1;

                        foreach (System.Data.DataRow row in dictionaryTable.Rows)
                        {
                            bool isGroup = false;

                            this.Dispatcher.Invoke(addRow, "", 30);
                            this.Dispatcher.BeginInvoke(setText, "", new TextBlockConfig(StringLiterals.SPACE + rowCount.ToString() + StringLiterals.SPACE, new Thickness(2, 0, 2, 0), VerticalAlignment.Center, HorizontalAlignment.Center, rowCount, 0, Visibility.Visible), FontWeights.Normal);

                            columnCount = 1;
                            foreach (DataColumn column in dictionaryTable.Columns)
                            {                                
                                string displayValue = row[column.ColumnName].ToString();
                                if (column.ColumnName.Equals("epifieldtype"))
                                {
                                    displayValue = displayValue.Replace("Epi.Fields.", "");
                                    if (isGroup)
                                    {
                                        displayValue = "GroupField";
                                    }
                                }
                                else if (column.ColumnName.Equals("columnname"))
                                {
                                    isGroup = dashboardHelper.GetGroupFieldsAsList().Contains(displayValue);
                                }
                                this.Dispatcher.BeginInvoke(setText, "", new TextBlockConfig(displayValue, new Thickness(8, 8, 8, 8), VerticalAlignment.Center, HorizontalAlignment.Left, rowCount, columnCount, Visibility.Visible), FontWeights.Normal);
                                columnCount++;
                            }

                            rowCount++;
                        }

                        this.Dispatcher.BeginInvoke(drawBorders, "");
                    }

                    this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));                    
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                }
                finally
                {   
                    stopwatch.Stop();
                    Debug.Print("Data dictionary gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete with " + dashboardHelper.RecordCount.ToString() + " records and the following filters:");
                    Debug.Print(dashboardHelper.DataFilters.GenerateDataFilterString());
                }
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Used to construct the gadget and assign events
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper to attach to this gadget</param>
        private void Construct()
        {            
            ConfigCollapsedTriangle.MouseLeftButtonUp += new MouseButtonEventHandler(Triangle_MouseLeftButtonUp);
            ConfigExpandedTriangle.MouseLeftButtonUp += new MouseButtonEventHandler(Triangle_MouseLeftButtonUp);            

            imgClose.MouseEnter += new MouseEventHandler(imgClose_MouseEnter);
            imgClose.MouseLeave += new MouseEventHandler(imgClose_MouseLeave);
            imgClose.MouseDown += new MouseButtonEventHandler(imgClose_MouseDown);

            //requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
            //checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

            mnuCopy.Click += new RoutedEventHandler(mnuCopy_Click);
            mnuSendDataToHTML.Click += new RoutedEventHandler(mnuSendDataToHTML_Click);
            
            mnuSendToBack.Click += new RoutedEventHandler(mnuSendToBack_Click);
            mnuRefresh.Click += new RoutedEventHandler(mnuRefresh_Click);
            mnuClose.Click += new RoutedEventHandler(mnuClose_Click);
            mnuSendDataToHTML.Visibility = System.Windows.Visibility.Visible;
            mnuClose.Visibility = System.Windows.Visibility.Visible;            

            this.IsProcessing = false;            

            this.GadgetStatusUpdate += new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(IsCancelled);

            RefreshResults();
        }

        /// <summary>
        /// Closes the gadget
        /// </summary>
        private void CloseGadget()
        {
            if (worker != null && worker.WorkerSupportsCancellation)
            {
                worker.CancelAsync();
            }

            if (worker != null)
            {
                worker.DoWork -= new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
                worker.RunWorkerCompleted -= new System.ComponentModel.RunWorkerCompletedEventHandler(worker_WorkerCompleted);
            }
            if (baseWorker != null)
            {
                baseWorker.DoWork -= new System.ComponentModel.DoWorkEventHandler(Execute);
            }

            //if (gadgetOptions != null)
            //{
            //    gadgetOptions.GadgetStatusUpdate -= new GadgetStatusUpdateHandler(requestUpdateStatus);
            //    gadgetOptions.GadgetCheckForCancellation -= new GadgetCheckForCancellationHandler(checkForCancellation);
            //    gadgetOptions = null;
            //}

            this.GadgetStatusUpdate -= new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation -= new GadgetCheckForCancellationHandler(IsCancelled);
            

            ConfigCollapsedTriangle.MouseLeftButtonUp -= new MouseButtonEventHandler(Triangle_MouseLeftButtonUp);
            ConfigExpandedTriangle.MouseLeftButtonUp -= new MouseButtonEventHandler(Triangle_MouseLeftButtonUp);

            imgClose.MouseEnter -= new MouseEventHandler(imgClose_MouseEnter);
            imgClose.MouseLeave -= new MouseEventHandler(imgClose_MouseLeave);
            imgClose.MouseDown -= new MouseButtonEventHandler(imgClose_MouseDown);
            
            mnuSendToBack.Click -= new RoutedEventHandler(mnuSendToBack_Click);
            mnuRefresh.Click -= new RoutedEventHandler(mnuRefresh_Click);
            mnuClose.Click -= new RoutedEventHandler(mnuClose_Click);

            if (GadgetClosing != null)
                GadgetClosing(this);

            GadgetClosing = null;
            GadgetCheckForCancellation = null;
            GadgetProcessingFinished = null;
            GadgetStatusUpdate = null;
        }

        /// <summary>
        /// Handles the collapsing and expanding of the gadget's configuration panel.
        /// </summary>
        private void CollapseExpandConfigPanel()
        {
            if (triangleCollapsed)
            {
                ExpandConfigPanel();
            }
            else
            {
                CollapseConfigPanel();
            }
        }

        /// <summary>
        /// Forces a collapse of the config panel
        /// </summary>
        private void CollapseConfigPanel()
        {
            ConfigCollapsedTriangle.Visibility = Visibility.Visible;
            ConfigExpandedTriangle.Visibility = Visibility.Collapsed;
            ConfigCollapsedTitle.Visibility = Visibility.Visible;
            ConfigGrid.Height = 50;
            triangleCollapsed = true;
        }

        /// <summary>
        /// Forces an expansion of the config panel
        /// </summary>
        private void ExpandConfigPanel()
        {
            ConfigExpandedTriangle.Visibility = Visibility.Visible;
            ConfigCollapsedTriangle.Visibility = Visibility.Collapsed;
            ConfigCollapsedTitle.Visibility = Visibility.Collapsed;
            ConfigGrid.Height = Double.NaN;
            ConfigGrid.UpdateLayout();
            triangleCollapsed = false;
        }

        /// <summary>
        /// Sends the gadget to the back of the canvas
        /// </summary>
        private void SendToBack()
        {
            Canvas.SetZIndex(this, -1);
        }

        /// <summary>
        /// Handles the filling of the gadget's combo boxes
        /// </summary>
        private void FillComboboxes(bool update = false)
        {            
            //loadingCombos = true;

            //cbxGroupField.ItemsSource = null;
            //cbxGroupField.Items.Clear();            

            //if (!update)
            //{
            //    lbxFields.ItemsSource = null;
            //    lbxFields.Items.Clear();
            //    lbxSortFields.Items.Clear();

            //    List<string> fieldNames = new List<string>();
            //    List<string> strataFieldNames = new List<string>();
            //    ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.DateTime | ColumnDataType.UserDefined;
            //    strataFieldNames = dashboardHelper.GetFieldsAsList(columnDataType);
            //    strataFieldNames.Add(string.Empty);
            //    strataFieldNames.Sort();

            //    fieldNames = dashboardHelper.GetFieldsAsList();
            //    fieldNames.AddRange(dashboardHelper.GetGroupFieldsAsList());

            //    if (dashboardHelper.IsUsingEpiProject)
            //    {
            //        for (int i = 0; i < this.View.Pages.Count; i++)
            //        {
            //            fieldNames.Add("Page " + (i + 1).ToString());
            //        }
            //    }

            //    cbxGroupField.ItemsSource = strataFieldNames;
            //    cbxSortField.ItemsSource = strataFieldNames;
            //    lbxFields.ItemsSource = fieldNames;

            //    if (cbxGroupField.Items.Count > 0)
            //    {
            //        cbxGroupField.SelectedIndex = -1;
            //    }
            //}
            //else
            //{
            //    List<string> selectedListFields = new List<string>();

            //    foreach (string s in lbxFields.SelectedItems)
            //    {
            //        selectedListFields.Add(s);
            //    }

            //    lbxFields.ItemsSource = null;
            //    lbxFields.Items.Clear();                

            //    List<string> fieldNames = new List<string>();
            //    List<string> strataFieldNames = new List<string>();
            //    ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.DateTime | ColumnDataType.UserDefined;
            //    strataFieldNames = dashboardHelper.GetFieldsAsList(columnDataType);
            //    strataFieldNames.Add(string.Empty);
            //    strataFieldNames.Sort();

            //    fieldNames = dashboardHelper.GetFieldsAsList();
            //    fieldNames.AddRange(dashboardHelper.GetGroupFieldsAsList());

            //    if (dashboardHelper.IsUsingEpiProject)
            //    {
            //        for (int i = 0; i < this.View.Pages.Count; i++)
            //        {
            //            fieldNames.Add("Page " + (i + 1).ToString());
            //        }
            //    }

            //    cbxGroupField.ItemsSource = strataFieldNames;
            //    cbxSortField.ItemsSource = strataFieldNames;
            //    lbxFields.ItemsSource = fieldNames;

            //    if (cbxGroupField.Items.Count > 0)
            //    {
            //        cbxGroupField.SelectedIndex = -1;
            //    }

            //    for (int i = lbxSortFields.Items.Count - 1; i == 0; i--)
            //    {
            //        string s = lbxSortFields.Items[i].ToString();

            //        if (!strataFieldNames.Contains(s))
            //        {
            //            lbxSortFields.Items.Remove(s);
            //        }
            //    }

            //    foreach (string s in selectedListFields)
            //    {
            //        if (fieldNames.Contains(s))
            //        {
            //            lbxFields.SelectedItems.Add(s);
            //        }
            //    }
            //}
            //loadingCombos = false;
        }

        /// <summary>
        /// Used to add a new Line List grid to the gadget's output
        /// </summary>
        /// <param name="groupVar">The name of the group variable selected, if any</param>
        /// <param name="value">The value by which this grid has been grouped by</param>
        private void AddLineListGrid(string groupVar, string value, int columnCount)
        {
            ScrollViewer sv = svMain;
            sv.MaxHeight = System.Windows.SystemParameters.PrimaryScreenHeight - 300;
            sv.MaxWidth = System.Windows.SystemParameters.PrimaryScreenWidth - 250;
            sv.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            //Grid grid = new Grid();
            //grid.Tag = value;

            grdDictionary.Children.Clear();
            grdDictionary.ColumnDefinitions.Clear();
            grdDictionary.RowDefinitions.Clear();

            grdDictionary.Width = grdFreq.Width;
            grdDictionary.HorizontalAlignment = HorizontalAlignment.Center;
            grdDictionary.Margin = new Thickness(0, 0, 0, 0);
            grdDictionary.Visibility = System.Windows.Visibility.Collapsed;

            for (int i = 0; i < columnCount; i++)
            {
                ColumnDefinition column = new ColumnDefinition();
                column.Width = GridLength.Auto;
                grdDictionary.ColumnDefinitions.Add(column);
            }

            ColumnDefinition totalColumn = new ColumnDefinition();
            totalColumn.Width = GridLength.Auto;
            grdDictionary.ColumnDefinitions.Add(totalColumn);

            //panelMain.Children.Add(grid);
            //panelMain.Children.Add(sv);
            //sv.Content = grid;

            //groupGridList.Add(grid);
        }

        private Grid GetGroupGrid(string groupValue)
        {
            //Grid grid = new Grid();

            //foreach (Grid g in groupGridList)
            //{
            //    if (g.Tag.Equals(groupValue))
            //    {
            //        grid = g;
            //        break;
            //    }
            //}

            //return grid;
            return grdDictionary;
        }

        private void SetGridText(string strataValue, TextBlockConfig textBlockConfig, FontWeight fontWeight)
        {
            Grid grid = new Grid();

            grid = GetGroupGrid(strataValue);

            TextBlock txt = new TextBlock();            
            txt.FontWeight = fontWeight;
            txt.Text = textBlockConfig.Text;
            txt.Margin = textBlockConfig.Margin;
            txt.VerticalAlignment = textBlockConfig.VerticalAlignment;
            txt.HorizontalAlignment = textBlockConfig.HorizontalAlignment;
            txt.TextWrapping = TextWrapping.Wrap;
            txt.MaxWidth = 300;
            Grid.SetZIndex(txt, 1000);
            Grid.SetRow(txt, textBlockConfig.RowNumber);
            Grid.SetColumn(txt, textBlockConfig.ColumnNumber);
            grid.Children.Add(txt);
        }

        private void SetGridImage(string strataValue, byte[] imageBlob, TextBlockConfig textBlockConfig, FontWeight fontWeight)
        {
            Grid grid = new Grid();

            grid = GetGroupGrid(strataValue);

            Image image = new Image();
            System.IO.MemoryStream stream = new System.IO.MemoryStream(imageBlob);
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            image.Source = bitmap;
            image.Height = bitmap.Height;
            image.Width = bitmap.Width;

            Grid.SetZIndex(image, 1000);
            Grid.SetRow(image, textBlockConfig.RowNumber);
            Grid.SetColumn(image, textBlockConfig.ColumnNumber);
            grid.Children.Add(image);
        }

        private void AddGridRow(string strataValue, int height)
        {
            Grid grid = GetGroupGrid(strataValue);

            waitCursor.Visibility = Visibility.Collapsed;
            grid.Visibility = Visibility.Visible; //grdFreq.Visibility = Visibility.Visible;
            RowDefinition rowDef = new RowDefinition();
            rowDef.Height = GridLength.Auto;
            //rowDef.Height = new GridLength(height);
            grid.RowDefinitions.Add(rowDef);
        }

        private void RenderFrequencyHeader(string strataValue, string freqVar, DataColumnCollection columns)
        {
            Grid grid = GetGroupGrid(strataValue);

            RowDefinition rowDefHeader = new RowDefinition();
            rowDefHeader.Height = new GridLength(30);
            grid.RowDefinitions.Add(rowDefHeader);

            for (int y = 0; y < grid.ColumnDefinitions.Count; y++)
            {                
                Rectangle rctHeader = new Rectangle();
                rctHeader.Fill = SystemColors.MenuHighlightBrush;
                Grid.SetRow(rctHeader, 0);
                Grid.SetColumn(rctHeader, y);
                grid.Children.Add(rctHeader);
            }

            TextBlock txtRowTotalHeader = new TextBlock();
            txtRowTotalHeader.Text = "Line";
            txtRowTotalHeader.VerticalAlignment = VerticalAlignment.Center;
            txtRowTotalHeader.HorizontalAlignment = HorizontalAlignment.Center;
            txtRowTotalHeader.Margin = new Thickness(4, 0, 4, 0);
            txtRowTotalHeader.FontWeight = FontWeights.Bold;
            txtRowTotalHeader.Foreground = Brushes.White;
            Grid.SetRow(txtRowTotalHeader, 0);
            Grid.SetColumn(txtRowTotalHeader, 0);
            grid.Children.Add(txtRowTotalHeader);

            for (int i = 0; i < columns.Count; i++)
            {
                TextBlock txtColHeader = new TextBlock();
                string columnName = columns[i].ColumnName.Trim();

                switch (columnName)
                {
                    case "columnname":
                        columnName = "Field name";
                        break;
                    case "epifieldtype":
                        columnName = "Epi Info field type";
                        break;
                    case "datatype":
                        columnName = "Data type";
                        break;
                    case "tablename":
                        columnName = "Table";
                        break;
                    case "formname":
                        columnName = "Form";
                        break;
                }

                txtColHeader.Text = columnName;
                txtColHeader.VerticalAlignment = VerticalAlignment.Center;
                txtColHeader.HorizontalAlignment = HorizontalAlignment.Center;
                txtColHeader.Margin = new Thickness(4, 0, 4, 0);
                txtColHeader.FontWeight = FontWeights.Bold;
                txtColHeader.Foreground = Brushes.White;

                Grid.SetRow(txtColHeader, 0);
                Grid.SetColumn(txtColHeader, i + 1);
                grid.Children.Add(txtColHeader);
            }

            
        }

        private void RenderFrequencyFooter(string strataValue, int footerRowIndex, int[] totalRows)
        {
            Grid grid = GetGroupGrid(strataValue);

            //RowDefinition rowDefTotals = new RowDefinition();
            //rowDefTotals.Height = new GridLength(30);
            //grid.RowDefinitions.Add(rowDefTotals);

            //TextBlock txtValTotals = new TextBlock();
            //txtValTotals.Text = StringLiterals.SPACE + SharedStrings.TOTAL + StringLiterals.SPACE;
            //txtValTotals.Margin = new Thickness(2, 0, 2, 0);
            //txtValTotals.VerticalAlignment = VerticalAlignment.Center;
            //txtValTotals.FontWeight = FontWeights.Bold;
            //Grid.SetRow(txtValTotals, footerRowIndex);
            //Grid.SetColumn(txtValTotals, 0);
            //grid.Children.Add(txtValTotals);

            //for (int i = 0; i < totalRows.Length; i++)
            //{
            //    TextBlock txtFreqTotals = new TextBlock();
            //    txtFreqTotals.Text = StringLiterals.SPACE + totalRows[i].ToString() + StringLiterals.SPACE;
            //    txtFreqTotals.Margin = new Thickness(2, 0, 2, 0);
            //    txtFreqTotals.VerticalAlignment = VerticalAlignment.Center;
            //    txtFreqTotals.HorizontalAlignment = HorizontalAlignment.Right;
            //    txtFreqTotals.FontWeight = FontWeights.Bold;
            //    Grid.SetRow(txtFreqTotals, footerRowIndex);
            //    Grid.SetColumn(txtFreqTotals, i + 1);
            //    grid.Children.Add(txtFreqTotals);
            //}

            //int sumTotal = 0;
            //foreach (int n in totalRows)
            //{
            //    sumTotal = sumTotal + n;
            //}

            //TextBlock txtOverallTotal = new TextBlock();
            //txtOverallTotal.Text = StringLiterals.SPACE + sumTotal.ToString() + StringLiterals.SPACE;
            //txtOverallTotal.Margin = new Thickness(2, 0, 2, 0);
            //txtOverallTotal.VerticalAlignment = VerticalAlignment.Center;
            //txtOverallTotal.HorizontalAlignment = HorizontalAlignment.Right;
            //txtOverallTotal.FontWeight = FontWeights.Bold;
            //Grid.SetRow(txtOverallTotal, footerRowIndex);
            //Grid.SetColumn(txtOverallTotal, MaxColumns + 1);
            //grid.Children.Add(txtOverallTotal);
        }

        private void DrawFrequencyBorders(string groupValue)
        {
            Grid grid = GetGroupGrid(groupValue);

            waitCursor.Visibility = Visibility.Collapsed;
            int rdcount = 0;
            Brush brush = Brushes.Black;            

            foreach (RowDefinition rd in grid.RowDefinitions)
            {
                //if (rdcount > 0)
                //{
                //    brush = Brushes.DarkSlateGray;
                //}
                int cdcount = 0;
                foreach (ColumnDefinition cd in grid.ColumnDefinitions)
                {  
                    Rectangle rctBorder = new Rectangle();
                    rctBorder.Stroke = brush;
                    rctBorder.StrokeThickness = 0.5;
                    Grid.SetRow(rctBorder, rdcount);
                    Grid.SetColumn(rctBorder, cdcount);
                    grid.Children.Add(rctBorder);
                    if (rdcount > 0)
                    {
                        Grid.SetZIndex(rctBorder, 0);

                        rctBorder.Fill = Brushes.White;
                    }
                    cdcount++;
                }
                rdcount++;
            }
        }

        /// <summary>
        /// Checks the gadget's position on the screen and, if necessary, re-sets it so that it is visible. This is used in 
        /// scenarios where the user has moved the gadget to a specific position where its top and/or left positions are outside 
        /// of the canvas boundaries, adds a filter, and the subsequent gadget output is too short to roll onto the visible 
        /// canvas. The gadget is then hidden entirely from the user's view. This will re-set the gadget so that it is visible.
        /// This should only be called from the RenderFinish series of methods.
        /// </summary>
        private void CheckAndSetPosition()
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

        /// <summary>
        /// Sets the gadget's state to 'finished' mode
        /// </summary>
        private void RenderFinish()
        {   
            waitCursor.Visibility = Visibility.Collapsed;
            grdDictionary.Visibility = Visibility.Visible;            

            pnlStatus.Visibility = System.Windows.Visibility.Hidden;
            txtStatus.Text = string.Empty;

            CheckAndSetPosition();
        }

        private void RenderFinishWithWarning(string errorMessage)
        {
            waitCursor.Visibility = Visibility.Collapsed;
            grdDictionary.Visibility = Visibility.Visible;

            pnlStatus.Background = Brushes.Gold;
            pnlStatusTop.Background = Brushes.Goldenrod;

            pnlStatus.Visibility = System.Windows.Visibility.Visible;
            txtStatus.Text = errorMessage;
            CollapseConfigPanel();
            CheckAndSetPosition();
        }

        /// <summary>
        /// Sets the gadget's state to 'finished with error' mode
        /// </summary>
        /// <param name="errorMessage">The error message to display</param>
        private void RenderFinishWithError(string errorMessage)
        {
            waitCursor.Visibility = Visibility.Collapsed;

            pnlStatus.Background = Brushes.Tomato;
            pnlStatusTop.Background = Brushes.Red;

            pnlStatus.Visibility = System.Windows.Visibility.Visible;
            txtStatus.Text = errorMessage;
            CollapseConfigPanel();
            CheckAndSetPosition();
        }

        /// <summary>
        /// Used to push a status message to the gadget's status panel
        /// </summary>
        /// <param name="statusMessage">The status message to display</param>
        private void RequestUpdateStatusMessage(string statusMessage)
        {
            this.Dispatcher.BeginInvoke(new SetStatusDelegate(SetStatusMessage), statusMessage);
        }

        /// <summary>
        /// Used to sets the gadget's current status, e.g. "Processing results..." or "Displaying output..."
        /// </summary>
        /// <param name="statusMessage">The status message to display</param>
        private void SetStatusMessage(string statusMessage)
        {
            pnlStatus.Visibility = System.Windows.Visibility.Visible;
            txtStatus.Text = statusMessage;
        }

        /// <summary>
        /// Used to check if the user took an action that effectively cancels the currently-running worker
        /// thread. This could include: Closing the gadget, refreshing the dashboard, or selecting another
        /// means variable from the list of variables.
        /// </summary>
        /// <returns>Bool indicating whether or not the gadget's processing thread has been cancelled by the user</returns>
        private bool IsCancelled()
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
        /// Gets the gradient brush to use in drawing the output grid
        /// </summary>
        /// <returns>Brush</returns>
        private Brush GetGradientBrush()
        {
            LinearGradientBrush gradient = new LinearGradientBrush();
            gradient.StartPoint = new Point(0.5, 0);
            gradient.EndPoint = new Point(0.5, 1);

            GradientStop color1 = new GradientStop();
            //color1.Color = SystemColors.ControlLightLightColor;
            color1.Color = SystemColors.GradientActiveCaptionColor;
            color1.Offset = 0;
            gradient.GradientStops.Add(color1);

            GradientStop color4 = new GradientStop();
            //color4.Color = SystemColors.ControlLightColor;
            color4.Color = SystemColors.HighlightColor;
            color4.Offset = 0.1;
            gradient.GradientStops.Add(color4);

            GradientStop color2 = new GradientStop();
            //color2.Color = SystemColors.ControlDarkDarkColor;
            color2.Color = SystemColors.WindowColor;
            color2.Offset = 0.5;
            gradient.GradientStops.Add(color2);

            GradientStop color3 = new GradientStop();
            //color3.Color = SystemColors.ControlDarkColor;
            color3.Color = SystemColors.GradientInactiveCaptionColor;
            color3.Offset = 0.75;
            gradient.GradientStops.Add(color3);


            return gradient;
        }

        /// <summary>
        /// Used to generate the list of variables and options for the GadgetParameters object
        /// </summary> 
        private void CreateInputVariableList()
        {
            //Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

            //gadgetOptions.MainVariableName = string.Empty;
            //gadgetOptions.WeightVariableName = string.Empty;
            //gadgetOptions.StrataVariableNames = new List<string>();
            //gadgetOptions.CrosstabVariableName = string.Empty;

            //List<string> listFields = new List<string>();

            //if (lbxFields.SelectedItems.Count > 0)
            //{
            //    foreach (string item in lbxFields.SelectedItems)
            //    {
            //        if (!string.IsNullOrEmpty(item))
            //        {
            //            listFields.Add(item);
            //        }
            //    }
            //}

            //listFields.Sort();
            //if (IsHostedByEnter)
            //{
            //    if (listFields.Contains("UniqueKey"))
            //        listFields.Remove("UniqueKey");
            //    listFields.Add("UniqueKey");
            //}

            //foreach (string field in listFields)
            //{
            //    inputVariableList.Add(field, "listfield");
            //}

            //if (!inputVariableList.ContainsKey("sortcolumnsbytaborder"))
            //{
            //    if (checkboxTabOrder.IsChecked == true)
            //    {
            //        inputVariableList.Add("sortcolumnsbytaborder", "true");
            //    }
            //    else
            //    {
            //        inputVariableList.Add("sortcolumnsbytaborder", "false");
            //    }
            //}

            //if (!inputVariableList.ContainsKey("usepromptsforcolumnnames"))
            //{
            //    if (checkboxUsePrompts.IsChecked == true)
            //    {
            //        inputVariableList.Add("usepromptsforcolumnnames", "true");
            //    }
            //    else
            //    {
            //        inputVariableList.Add("usepromptsforcolumnnames", "false");
            //    }
            //}

            //if (checkboxListLabels.IsChecked == true)
            //{
            //    gadgetOptions.ShouldShowCommentLegalLabels = true;
            //}
            //else
            //{
            //    gadgetOptions.ShouldShowCommentLegalLabels = false;
            //}

            //if (lbxSortFields.Items.Count > 0)
            //{
            //    foreach (string item in lbxSortFields.Items)
            //    {
            //        if (!string.IsNullOrEmpty(item))
            //        {
            //            string baseStr = item;

            //            if (baseStr.EndsWith("(ascending)"))
            //            {
            //                baseStr = "[" + baseStr.Remove(baseStr.Length - 12) + "] ASC";
            //            }
            //            if (baseStr.EndsWith("(descending)"))
            //            {
            //                baseStr = "[" + baseStr.Remove(baseStr.Length - 13) + "] DESC";
            //            }
            //            inputVariableList.Add(baseStr, "sortfield");
            //        }
            //    }
            //}

            //if (cbxGroupField.SelectedIndex >= 0)
            //{
            //    if (!string.IsNullOrEmpty(cbxGroupField.SelectedItem.ToString()))
            //    {
            //        gadgetOptions.StrataVariableNames.Add(cbxGroupField.SelectedItem.ToString());
            //    }
            //}

            //if(groupGridList.Count >= 1) 
            //{
            //    Grid grid = groupGridList[0];
            //    SortedDictionary<int, string> sortColumnDictionary = new SortedDictionary<int, string>();

            //    foreach (UIElement element in grid.Children)
            //    {
            //        if (Grid.GetRow(element) == 0 && element is TextBlock)
            //        {
            //            TextBlock txtColumnName = element as TextBlock;
            //            //columnOrder.Add(txtColumnName.Text);
            //            sortColumnDictionary.Add(Grid.GetColumn(element), txtColumnName.Text);
            //        }
            //    }

            //    foreach (KeyValuePair<int, string> kvp in sortColumnDictionary)
            //    {
            //        columnOrder.Add(kvp.Value);
            //    }

            //    if (columnOrder.Count == listFields.Count || columnOrder.Count == (listFields.Count + 1))
            //    {
            //        bool same = true;
            //        foreach (string s in listFields)
            //        {
            //            if (!columnOrder.Contains(s))
            //            {
            //                same = false;
            //            }
            //        }

            //        if (same)
            //        {
            //            WordBuilder wb = new WordBuilder("^");
            //            foreach (string s in columnOrder)
            //            {
            //                wb.Add(s);
            //            }

            //            inputVariableList.Add("customusercolumnsort", wb.ToString());
            //        }
            //        else
            //        {
            //            columnOrder = new List<string>();
            //        }
            //    }
            //    else
            //    {
            //        columnOrder = new List<string>();
            //    }
            //}

            //inputVariableList.Add("maxcolumns", MaxColumns.ToString());
            //inputVariableList.Add("maxrows", MaxRows.ToString());

            //gadgetOptions.ShouldIncludeFullSummaryStatistics = false;
            //gadgetOptions.InputVariableList = inputVariableList;
        }

        #endregion

        #region Private Properties
        private View View
        {
            get
            {
                return this.dashboardHelper.View;
            }
        }

        private IDbDriver Database
        {
            get
            {
                return this.dashboardHelper.Database;
            }
        }
        #endregion // Private Properties

        #region IGadget Members

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

        /// <summary>
        /// Initiates a refresh of the gadget's output
        /// </summary>
        public void RefreshResults()
        {
            //if (!loadingCombos && gadgetOptions != null && lbxFields.SelectedItems.Count > 0)
            //{
                //CreateInputVariableList();
                //CollapseConfigPanel();
                waitCursor.Visibility = Visibility.Visible;

                pnlStatus.Background = Brushes.PaleGreen;
                pnlStatusTop.Background = Brushes.Green;

                baseWorker = new BackgroundWorker();
                baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
                baseWorker.RunWorkerAsync();
            //}
        }

        /// <summary>
        /// Updates the variable names available in the gadget's properties
        /// </summary>
        public void UpdateVariableNames()
        {
            //FillComboboxes(true);
            RefreshResults();
        }

        ///// <summary>
        ///// Generates Xml representation of this gadget
        ///// </summary>
        ///// <param name="doc">The Xml docment</param>
        ///// <returns>XmlNode</returns>
        //public XmlNode Serialize(XmlDocument doc)
        //{
        //    Dictionary<string, string> inputVariableList = gadgetOptions.InputVariableList;

        //    string groupVar = string.Empty;            

        //    if (cbxGroupField.SelectedItem != null)
        //    {
        //        groupVar = cbxGroupField.SelectedItem.ToString();
        //    }

        //    string xmlString =
        //    "<groupVariable>" + groupVar + "</groupVariable>" +
        //    "<maxRows>" + MaxRows.ToString() + "</maxRows>" +
        //    "<maxColumnNameLength>" + MaxColumnLength.ToString() + "</maxColumnNameLength>" +
        //    "<sortColumnsByTabOrder>" + checkboxTabOrder.IsChecked.ToString() + "</sortColumnsByTabOrder>" +
        //    "<useFieldPrompts>" + checkboxUsePrompts.IsChecked.ToString() + "</useFieldPrompts>" +
        //    "<alternatingRowColors>" + checkboxAltRowColors.IsChecked.ToString() + "</alternatingRowColors>" +
        //    "<customHeading>" + CustomOutputHeading + "</customHeading>" +
        //    "<customDescription>" + CustomOutputDescription + "</customDescription>" +
        //    "<customCaption>" + CustomOutputCaption + "</customCaption>";

        //    if (inputVariableList.ContainsKey("customusercolumnsort"))
        //    {
        //        string columns = inputVariableList["customusercolumnsort"];
        //        xmlString = xmlString + "<customusercolumnsort>" + columns + "</customusercolumnsort>";                
        //    }
        //    else if (columnOrder != null && columnOrder.Count > 0) // when user has re-ordered columns but not refreshed
        //    {
        //        WordBuilder wb = new WordBuilder("^");
        //        for (int i = 0; i < columnOrder.Count; i++)
        //        {
        //            wb.Add(columnOrder[i]);
        //        }
        //        xmlString = xmlString + "<customusercolumnsort>" + wb.ToString() + "</customusercolumnsort>";
        //    }

        //    System.Xml.XmlElement element = doc.CreateElement("lineListGadget");
        //    element.InnerXml = xmlString;

        //    System.Xml.XmlAttribute locationY = doc.CreateAttribute("top");
        //    System.Xml.XmlAttribute locationX = doc.CreateAttribute("left");
        //    System.Xml.XmlAttribute collapsed = doc.CreateAttribute("collapsed");
        //    System.Xml.XmlAttribute type = doc.CreateAttribute("gadgetType");

        //    locationY.Value = Canvas.GetTop(this).ToString("F2");
        //    locationX.Value = Canvas.GetLeft(this).ToString("F2");
        //    collapsed.Value = "false"; // currently no way to collapse the gadget, so leave this 'false' for now            
        //    type.Value = "EpiDashboard.LineListControl";

        //    element.Attributes.Append(locationY);
        //    element.Attributes.Append(locationX);
        //    element.Attributes.Append(collapsed);
        //    element.Attributes.Append(type);

        //    if (lbxFields.Items.Count > 0 && lbxFields.SelectedItems.Count > 0)
        //    {
        //        string xmlListItemString = string.Empty;
        //        XmlElement listItemElement = doc.CreateElement("listFields");

        //        foreach (string s in lbxFields.SelectedItems)
        //        {
        //            xmlListItemString = xmlListItemString + "<listField>" + s + "</listField>";
        //        }

        //        listItemElement.InnerXml = xmlListItemString;
        //        element.AppendChild(listItemElement);
        //    }

        //    if (lbxSortFields.Items.Count > 0)
        //    {
        //        string xmlSortString = string.Empty;
        //        XmlElement sortElement = doc.CreateElement("sortFields");

        //        foreach (string s in lbxSortFields.Items)
        //        {
        //            xmlSortString = xmlSortString + "<sortField>" + s + "</sortField>";
        //        }

        //        sortElement.InnerXml = xmlSortString;
        //        element.AppendChild(sortElement);
        //    }

        //    return element;
        //}

        ///// <summary>
        ///// Creates the frequency gadget from an Xml element
        ///// </summary>
        ///// <param name="element">The element from which to create the gadget</param>
        //public void CreateFromXml(XmlElement element)
        //{
        //    this.loadingCombos = true;
        //    this.ColumnWarningShown = true;

        //    foreach (XmlElement child in element.ChildNodes)
        //    {
        //        switch (child.Name.ToLower())
        //        {
        //            case "groupvariable":
        //                cbxGroupField.Text = child.InnerText;
        //                break;
        //            case "maxcolumnnamelength":
        //                int maxColumnLength = 24;
        //                int.TryParse(child.InnerText, out maxColumnLength);
        //                txtMaxColumnLength.Text = maxColumnLength.ToString();
        //                break;
        //            case "maxrows":
        //                int maxRows = 200;
        //                int.TryParse(child.InnerText, out maxRows);
        //                txtMaxRows.Text = maxRows.ToString();
        //                break;
        //            case "sortcolumnsbytaborder":                        
        //                bool sortByTabs = false;
        //                bool.TryParse(child.InnerText, out sortByTabs);
        //                checkboxTabOrder.IsChecked = sortByTabs;
        //                break;
        //            case "usefieldprompts":
        //                bool usePrompts = false;
        //                bool.TryParse(child.InnerText, out usePrompts);
        //                checkboxUsePrompts.IsChecked = usePrompts;
        //                break;
        //            case "alternatingrowcolors":
        //                bool altColors = false;
        //                bool.TryParse(child.InnerText, out altColors);
        //                checkboxAltRowColors.IsChecked = altColors;
        //                break;
        //            case "customheading":
        //                this.CustomOutputHeading = child.InnerText;
        //                break;
        //            case "customdescription":
        //                this.CustomOutputDescription = child.InnerText;
        //                break;
        //            case "customcaption":
        //                this.CustomOutputCaption = child.InnerText;
        //                break;
        //            case "listfields":
        //                foreach (XmlElement field in child.ChildNodes)
        //                {
        //                    List<string> fields = new List<string>();
        //                    if (field.Name.ToLower().Equals("listfield"))
        //                    {
        //                     //   fields.Add(field.InnerText);
        //                        lbxFields.SelectedItems.Add(field.InnerText);
        //                    }
        //                }
        //                break;
        //            case "sortfields":
        //                foreach (XmlElement field in child.ChildNodes)
        //                {
        //                    List<string> fields = new List<string>();
        //                    if (field.Name.ToLower().Equals("sortfield"))
        //                    {
        //                        lbxSortFields.Items.Add(field.InnerText);
        //                    }
        //                }
        //                break;
        //            case "customusercolumnsort":                        
        //                string[] cols = child.InnerText.Split('^');
        //                columnOrder = cols.ToList();
        //                break;
        //        }
        //    }

        //    foreach (XmlAttribute attribute in element.Attributes)
        //    {
        //        switch (attribute.Name.ToLower())
        //        {
        //            case "top":
        //                Canvas.SetTop(this, double.Parse(attribute.Value));
        //                break;
        //            case "left":
        //                Canvas.SetLeft(this, double.Parse(attribute.Value));
        //                break;
        //        }
        //    }

        //    this.loadingCombos = false;            

        //    RefreshResults();
        //    CollapseExpandConfigPanel();
        //}

        /// <summary>
        /// Checks to see whether a valid numeric digit was pressed for numeric-only conditions
        /// </summary>
        /// <param name="keyChar">The key that was pressed</param>
        /// <returns>Whether the input was a valid number character</returns>
        private bool ValidNumberChar(string keyChar)
        {
            System.Globalization.NumberFormatInfo numberFormatInfo = System.Globalization.CultureInfo.CurrentCulture.NumberFormat;

            for (int i = 0; i < keyChar.Length; i++)
            {
                char ch = keyChar[i];
                if (!Char.IsDigit(ch))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        
        /// <summary>
        /// Sets the gadget to its 'processing' state
        /// </summary>
        public void SetGadgetToProcessingState()
        {
            this.IsProcessing = true;
            //this.cbxGroupField.IsEnabled = false;
            //this.lbxFields.IsEnabled = false;
            //this.lbxSortFields.IsEnabled = false;
            //this.txtMaxRows.IsEnabled = false;
            //this.txtMaxColumnLength.IsEnabled = false;
            //this.btnRun.IsEnabled = false;
            //this.cbxSortField.IsEnabled = false;
            //this.checkboxTabOrder.IsEnabled = false;
            //this.checkboxUsePrompts.IsEnabled = false;
            //this.checkboxAltRowColors.IsEnabled = false;
            //this.checkboxListLabels.IsEnabled = false;
        }

        /// <summary>
        /// Sets the gadget to its 'finished' state
        /// </summary>
        public void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
            //this.cbxGroupField.IsEnabled = true;
            //this.lbxFields.IsEnabled = true;
            //this.lbxSortFields.IsEnabled = true;
            //this.txtMaxRows.IsEnabled = true;
            //this.txtMaxColumnLength.IsEnabled = true;
            //this.btnRun.IsEnabled = true;
            //this.cbxSortField.IsEnabled = true;
            //this.checkboxTabOrder.IsEnabled = true;
            //this.checkboxUsePrompts.IsEnabled = true;
            //this.checkboxAltRowColors.IsEnabled = true;
            //this.checkboxListLabels.IsEnabled = true;

            if (GadgetProcessingFinished != null)
                GadgetProcessingFinished(this);
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public string ToHTML(string htmlFileName = "", int count = 0)
        {
            StringBuilder htmlBuilder = new StringBuilder();

            htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">Data Dictionary</h2>");

            if (!string.IsNullOrEmpty(txtStatus.Text) && pnlStatus.Visibility == Visibility.Visible)
            {
                htmlBuilder.AppendLine("<p><small><strong>" + txtStatus.Text + "</strong></small></p>");
            }

            Grid grid = grdDictionary;

            //string gridName = grid.Tag.ToString();

            htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
            htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
            //if (string.IsNullOrEmpty(CustomOutputCaption) && this.groupGridList.Count > 1)
            //{
            //    htmlBuilder.AppendLine("<caption>" + grid.Tag + "</caption>");
            //}
            //else if(!string.IsNullOrEmpty(CustomOutputCaption))
            //{
            //    htmlBuilder.AppendLine("<caption>" + CustomOutputCaption + "</caption>");
            //}

            foreach (UIElement control in grid.Children)
            {
                if (control is TextBlock)
                {
                    int rowNumber = Grid.GetRow(control);
                    int columnNumber = Grid.GetColumn(control);

                    string tableDataTagOpen = "<td class=\"value\" style=\"max-width: 300px; vertical-align: top;\">";
                    string tableDataTagClose = "</td>";

                    if (rowNumber == 0)
                    {
                        tableDataTagOpen = "<th style=\"width: auto;\">";
                        tableDataTagClose = "</th>";
                    }

                    if (columnNumber == 0)
                    {
                        if (((double)rowNumber) % 2.0 == 1)
                        {
                            htmlBuilder.AppendLine("<tr class=\"altcolor\">");
                        }
                        else
                        {
                            htmlBuilder.AppendLine("<tr>");
                        }
                    }
                    if (columnNumber == 0 && rowNumber > 0)
                    {
                        tableDataTagOpen = "<td class=\"value\">";
                    }

                    string value = ((TextBlock)control).Text;
                    string formattedValue = value;

                    if (string.IsNullOrEmpty(formattedValue))
                    {
                        formattedValue = "&nbsp;";
                    }

                    //if ((rowNumber == grid.RowDefinitions.Count - 1) || (columnNumber == grid.ColumnDefinitions.Count - 1))
                    //{
                    //    formattedValue = "<span class=\"total\">" + value + "</span>";
                    //}

                    htmlBuilder.AppendLine(tableDataTagOpen + formattedValue + tableDataTagClose);

                    if (columnNumber >= grid.ColumnDefinitions.Count - 1)
                    {
                        htmlBuilder.AppendLine("</tr>");
                    }
                }
            }

            htmlBuilder.AppendLine("</table>");
            

            return htmlBuilder.ToString();
        }

        private string customOutputHeading;
        private string customOutputDescription;
        private string customOutputCaption;

        public string CustomOutputHeading
        {
            get
            {
                return this.customOutputHeading;
            }
            set
            {
                this.customOutputHeading = value;
            }
        }

        public string CustomOutputDescription
        {
            get
            {
                return this.customOutputDescription;
            }
            set
            {
                this.customOutputDescription = value;
            }
        }

        public string CustomOutputCaption
        {
            get
            {
                return this.customOutputCaption;
            }
            set
            {
                this.customOutputCaption = value;
            }
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            RefreshResults();
        }

        void mnuSendToBack_Click(object sender, RoutedEventArgs e)
        {
            SendToBack();
        }

        void mnuRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshResults();
        }

        void mnuClose_Click(object sender, RoutedEventArgs e)
        {
            CloseGadget();
        }

        /// <summary>
        /// Handles the click event for the 'Copy to clipboard' context menu option
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuCopy_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard();
        }

        /// <summary>
        /// Handles the click event for the 'Send data to web browser' context menu option
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuSendDataToHTML_Click(object sender, RoutedEventArgs e)
        {
            if (grdDictionary == null || grdDictionary.RowDefinitions.Count == 0 || grdDictionary.ColumnDefinitions.Count == 0)
            {
                return;
            }

            try
            {
                string fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString("N") + ".html";//GetHTMLLineListing();

                System.IO.FileStream stream = System.IO.File.OpenWrite(fileName);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(stream);
                sw.WriteLine("<html><head><title>Data Dictionary</title>");
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
    }
}
