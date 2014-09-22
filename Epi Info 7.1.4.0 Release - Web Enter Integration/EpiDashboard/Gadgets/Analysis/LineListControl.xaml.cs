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
using EpiDashboard.Controls;

namespace EpiDashboard
{
    /// <summary>
    /// Interaction logic for LineListControl.xaml
    /// </summary>
    /// /// <remarks>
    /// This gadget is used to generate a line listing for the fields in the current data source, to include
    /// any related data columns. Columns are not sorted by default but, if using an Epi Info 7 project, they
    /// may also be sorted by their tab order (also known as the order of entry).
    /// </remarks>
    public partial class LineListControl : GadgetBase
    {
        #region Private Members
        /// <summary>
        /// The list of labels for each group value. E.g. if grouping the line list by SEX, there would
        /// be (likely) two values in this list: One for Male and one for Female. These show up as the table
        /// headings when displaying both of the line lists.
        /// </summary>
        private List<TextBlock> gridLabelsList;

        /// <summary>
        /// The list of scrollviewers for each group value.
        /// </summary>
        private List<ScrollViewer> groupSvList;

        private List<string> columnOrder = new List<string>();
        private int draggedColumnIndex = -1;
        private string clickedColumnName = string.Empty;
        private bool columnWarningShown;
        private int rowCount = 1;
        private int columnCount = 1;
        private int maxColumns;

        private const int MAX_ROW_LIMIT = 2000;

        /// <summary>
        /// Bool used to determine if the gadget is being called directly from the Enter module (e.g. 'Interactive Line List' option)
        /// </summary>
        private bool isHostedByEnter;

        /// <summary>
        /// Used for maintaining the width when collapsing output
        /// </summary>
        private double currentWidth;

        private Rectangle highlightRowRectangle;
        private FrameworkElement allowUpdateBox;
        private RequestUpdateStatusDelegate requestUpdateStatus;
        private CheckForCancellationDelegate checkForCancellation;

        #endregion // Private Members

        #region Delegates
        private delegate void SetGridImageDelegate(string strataValue, byte[] imageBlob, TextBlockConfig textBlockConfig, FontWeight fontWeight);
        private delegate void RenderFrequencyHeaderDelegate(string strataValue, string freqVar, DataColumnCollection columns);
        #endregion

        #region Events
        public event Mapping.RecordSelectedHandler RecordSelected;
        #endregion // Events

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public LineListControl()
        {
            InitializeComponent();            
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper object to attach</param>
        public LineListControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            this.IsHostedByEnter = false;
            Construct();
            FillComboboxes();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper object to attach</param>
        /// <param name="hostedByEnter">Whether the control is hosted by Enter</param>
        public LineListControl(DashboardHelper dashboardHelper, bool hostedByEnter)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            this.IsHostedByEnter = hostedByEnter;
            Construct();
            FillComboboxes();
        }

        #endregion // Constructors

        #region Public Methods

        /// <summary>
        /// Returns the gadget's description as a string.
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return "Line List Gadget";
        }

        /// <summary>
        /// Auto-selects a given page number from the list of pages
        /// </summary>
        /// <param name="pageNumber">The page number</param>
        public void SelectPageNumber(int pageNumber)
        {
            string pageNumberStr = "Page " + pageNumber.ToString();
            List<string> fieldNames = new List<string>();
            Epi.Page pageOne = this.View.GetPageByPosition(pageNumber - 1);

            foreach (Field field in pageOne.Fields)
            {
                if (field is IDataField && !(field is GridField || field is GroupField || field is UniqueKeyField || field is ImageField ||
                    field is RecStatusField || field is ForeignKeyField || field is GlobalRecordIdField))
                {
                    if (lbxFields.Items.Contains(field.Name))
                    {
                        fieldNames.Add(field.Name);
                    }
                }
            }

            foreach (string s in fieldNames)
            {
                lbxFields.SelectedItems.Add(s);
            }
        }

        /// <summary>
        /// Copies a grid's output to the clipboard
        /// </summary>
        protected override void CopyToClipboard()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Grid grid in this.StrataGridList)
            {
                string gridName = grid.Tag.ToString();
                if (StrataGridList.Count > 1)
                {
                    sb.AppendLine(grid.Tag.ToString());
                }

                SortedList<int, TextBlock> textBlocks = new SortedList<int, TextBlock>();
                int currentColumnCount = 0;
                int currentColumnNumber = 0;
                foreach (UIElement control in grid.Children)
                {
                    if (control is TextBlock)
                    {                        
                        int columnNumber = Grid.GetColumn(control);
                        textBlocks.Add(columnNumber + currentColumnCount, control as TextBlock);
                        currentColumnNumber++;
                    }
                    if (currentColumnNumber >= grid.ColumnDefinitions.Count)
                    {
                        currentColumnCount = currentColumnCount + grid.ColumnDefinitions.Count;
                        currentColumnNumber = 0;
                    }
                }

                foreach (KeyValuePair<int, TextBlock> kvp in textBlocks)
                {
                    TextBlock control = kvp.Value;
                    int rowNumber = Grid.GetRow(control);
                    int columnNumber = Grid.GetColumn(control);
                        
                    string value = ((TextBlock)control).Text;

                    sb.Append(value + "\t");

                    if (columnNumber >= grid.ColumnDefinitions.Count - 1)
                    {
                        sb.AppendLine();
                    }                    
                }

                sb.AppendLine();
            }
            sb.Replace("\n", "");
            Clipboard.Clear();
            Clipboard.SetText(sb.ToString());
        }

        /// <summary>
        /// Clears the gadget's output
        /// </summary>
        public void ClearResults()
        {
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Text = string.Empty;
            descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;

            foreach (Grid grid in StrataGridList)
            {
                grid.Children.Clear();
                grid.RowDefinitions.Clear();
                panelMain.Children.Remove(grid);                
            }

            foreach (ScrollViewer sv in groupSvList)
            {
                if (panelMain.Children.Contains(sv))
                {
                    panelMain.Children.Remove(sv);
                }
            }

            foreach (Expander expander in StrataExpanderList)
            {
                if (panelMain.Children.Contains(expander))
                {
                    panelMain.Children.Remove(expander);
                }
            }

            foreach (TextBlock textBlock in gridLabelsList)
            {
                panelMain.Children.Remove(textBlock);
            }

            StrataExpanderList.Clear();
            StrataGridList.Clear();
            groupSvList.Clear();            
        }
        #endregion // Public Methods

        #region Event Handlers

        /// <summary>
        /// Handles the WorkerCompleted event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected override void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Debug.Print("Background worker thread for line list gadget was cancelled or ran to completion.");
        }

        /// <summary>
        /// Handles the DoWorker event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected override void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (syncLock)
            {
                Dictionary<string, string> inputVariableList = ((GadgetParameters)e.Argument).InputVariableList;

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));     
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));

                AddOutputGridDelegate addGrid = new AddOutputGridDelegate(AddLineListGrid);
                SetGridTextDelegate setText = new SetGridTextDelegate(SetGridText);
                SetGridImageDelegate setImage = new SetGridImageDelegate(SetGridImage);
                AddGridRowDelegate addRow = new AddGridRowDelegate(AddGridRow);
                RenderFrequencyHeaderDelegate renderHeader = new RenderFrequencyHeaderDelegate(RenderFrequencyHeader);
                DrawFrequencyBordersDelegate drawBorders = new DrawFrequencyBordersDelegate(DrawFrequencyBorders);

                Configuration config = DashboardHelper.Config;
                string yesValue = config.Settings.RepresentationOfYes;
                string noValue = config.Settings.RepresentationOfNo;

                string groupVar = string.Empty;                
                int maxRows = 50;
                bool exceededMaxRows = false;
                bool exceededMaxColumns = false;
                bool showLineColumn = true;
                bool showColumnHeadings = true;
                bool showNullLabels = true;

                if (GadgetOptions.StrataVariableNames.Count > 0)
                {
                    groupVar = GadgetOptions.StrataVariableNames[0];
                }                

                if (inputVariableList.ContainsKey("maxcolumns"))
                {
                    maxColumns = int.Parse(inputVariableList["maxcolumns"]);
                }

                if (inputVariableList.ContainsKey("maxrows"))
                {
                    maxRows = int.Parse(inputVariableList["maxrows"]);
                }

                if (inputVariableList.ContainsKey("showcolumnheadings")) 
                {
                    showColumnHeadings = bool.Parse(inputVariableList["showcolumnheadings"]);
                }

                if (inputVariableList.ContainsKey("showlinecolumn"))
                {
                    showLineColumn = bool.Parse(inputVariableList["showlinecolumn"]);
                }

                if (inputVariableList.ContainsKey("shownulllabels"))
                {
                    showNullLabels = bool.Parse(inputVariableList["shownulllabels"]);
                }                

                //System.Threading.Thread.Sleep(4000); // Artifically inflating process time to see how the 'loading' screen looks. TODO: REMOVE LATER

                if (IsHostedByEnter)
                {
                    if (System.Windows.SystemParameters.IsSlowMachine == true)
                    {
                        maxColumns = 512;
                    }
                    else
                    {
                        maxColumns = 1024;
                    }
                }
                else
                {   
                    int renderingTier = (RenderCapability.Tier >> 16);
                    if (renderingTier >= 2)
                    {
                        maxColumns = 128;
                    }
                    else if (renderingTier >= 1)
                    {
                        maxColumns = 64;
                    }
                    else
                    {
                        maxColumns = 24;
                    }
                    
                    if (System.Windows.SystemParameters.IsSlowMachine == true)
                    {
                        maxColumns = 24;
                    }
                }

                List<string> stratas = new List<string>();
                if (!string.IsNullOrEmpty(groupVar))
                {
                    stratas.Add(groupVar);
                }

                try
                {
                    GadgetOptions.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                    GadgetOptions.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);
                    if (this.DataFilters != null)
                    {
                        GadgetOptions.CustomFilter = this.DataFilters.GenerateDataFilterString(false);
                    }
                    else
                    {
                        GadgetOptions.CustomFilter = string.Empty;
                    }

                    List<DataTable> lineListTables = DashboardHelper.GenerateLineList(GadgetOptions);

                    if (lineListTables == null || lineListTables.Count == 0)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.NO_RECORDS_SELECTED);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        Debug.Print("Line list thread cancelled");
                        return;
                    }
                    else if (lineListTables.Count == 1 && lineListTables[0].Rows.Count == 0)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.NO_RECORDS_SELECTED);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        Debug.Print("Line list thread cancelled");
                        return;
                    }
                    else if (worker.CancellationPending)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.DASHBOARD_GADGET_STATUS_OPERATION_CANCELLED);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        Debug.Print("Line list thread cancelled");
                        return;
                    }
                    else
                    {
                        string formatString = string.Empty;

                        foreach (DataTable listTable in lineListTables)
                        {
                            string strataValue = listTable.TableName;
                            if (listTable.Rows.Count == 0)
                            {
                                continue;
                            }
                            this.Dispatcher.BeginInvoke(addGrid, groupVar, listTable.TableName, listTable.Columns.Count);
                        }

                        SetGadgetStatusMessage(SharedStrings.DASHBOARD_GADGET_STATUS_DISPLAYING_OUTPUT);

                        foreach (DataTable listTable in lineListTables)
                        {
                            string strataValue = listTable.TableName;
                            if (listTable.Rows.Count == 0)
                            {
                                continue;
                            }
                            string tableHeading = listTable.TableName;

                            if (lineListTables.Count > 1)
                            {
                                //tableHeading = freqVar; ???
                            }

                            SetCustomColumnSort(listTable);

                            this.Dispatcher.BeginInvoke(renderHeader, strataValue, tableHeading, listTable.Columns);
                            
                            rowCount = 1;

                            if (listTable.Columns.Count == 0)
                            {
                                throw new ApplicationException("There are no columns to display in this list. If specifying a group variable, ensure the group variable contains data fields.");
                            }

                            int[] totals = new int[listTable.Columns.Count - 1];
                            columnCount = 1;

                            foreach (System.Data.DataRow row in listTable.Rows)
                            {
                                this.Dispatcher.Invoke(addRow, strataValue, -1);
                                this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(rowCount.ToString(), new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Stretch, TextAlignment.Center, rowCount, 0, Visibility.Visible), FontWeights.Normal);
                                columnCount = 1;

                                foreach (DataColumn column in listTable.Columns)
                                {
                                    if (columnCount > maxColumns + 1)
                                    {
                                        exceededMaxColumns = true;
                                        break;
                                    }

                                    string displayValue = row[column.ColumnName].ToString();

                                    if (DashboardHelper.IsUserDefinedColumn(column.ColumnName))
                                    {
                                        displayValue = DashboardHelper.GetFormattedOutput(column.ColumnName, row[column.ColumnName]);
                                    }
                                    else
                                    {
                                        Field field = null;
                                        string columnType = string.Empty;

                                        foreach (DataRow fieldRow in DashboardHelper.FieldTable.Rows)
                                        {
                                            if (fieldRow["columnname"].Equals(column.ColumnName))
                                            {
                                                columnType = fieldRow["datatype"].ToString();
                                                if (fieldRow["epifieldtype"] is Field)
                                                {
                                                    field = fieldRow["epifieldtype"] as Field;
                                                }
                                                break;
                                            }
                                        }

                                        if ((field != null && (field is YesNoField || field is CheckBoxField)) || column.DataType.ToString().Equals("System.Boolean"))
                                        {
                                            if (row[column.ColumnName].ToString().Equals("1") || row[column.ColumnName].ToString().ToLower().Equals("true"))
                                                displayValue = yesValue;
                                            else if (row[column.ColumnName].ToString().Equals("0") || row[column.ColumnName].ToString().ToLower().Equals("false"))
                                                displayValue = noValue;
                                        }
                                        else if ((field != null && field is DateField) || (!DashboardHelper.DateColumnRequiresTime(listTable, listTable.Columns[column.ColumnName].ColumnName)))
                                        {
                                            displayValue = string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:d}", row[column.ColumnName]);
                                        }
                                        else if (field != null && field is TimeField)
                                        {
                                            displayValue = string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:T}", row[column.ColumnName]);
                                        }
                                        else
                                        {
                                            displayValue = DashboardHelper.GetFormattedOutput(column.ColumnName, row[column.ColumnName]);
                                        }
                                    }

                                    if (string.IsNullOrEmpty(displayValue))
                                    {
                                        if (showNullLabels)
                                        {
                                            displayValue = config.Settings.RepresentationOfMissing;
                                        }
                                        else
                                        {
                                            displayValue = string.Empty;
                                        }
                                    }

                                    if (column.DataType.ToString().Equals("System.DateTime") || column.DataType.ToString().Equals("System.Int32") || column.DataType.ToString().Equals("System.Double") || column.DataType.ToString().Equals("System.Single"))
                                    {
                                        if (column.ColumnName.Equals("UniqueKey"))
                                        {
                                            this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(displayValue, new Thickness(8, 6, 8, 6), VerticalAlignment.Stretch, HorizontalAlignment.Stretch, TextAlignment.Right, rowCount, columnCount, Visibility.Collapsed), FontWeights.Normal);
                                        }
                                        else
                                        {
                                            this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(displayValue, new Thickness(8, 6, 8, 6), VerticalAlignment.Stretch, HorizontalAlignment.Stretch, TextAlignment.Right, rowCount, columnCount, Visibility.Visible), FontWeights.Normal);
                                        }
                                    }
                                    else if (column.DataType == typeof(byte[]) && displayValue != config.Settings.RepresentationOfMissing && IsHostedByEnter)
                                    {
                                        this.Dispatcher.BeginInvoke(setImage, strataValue, (byte[])row[column.ColumnName], new TextBlockConfig(displayValue, new Thickness(8, 6, 8, 6), VerticalAlignment.Stretch, HorizontalAlignment.Stretch, TextAlignment.Left, rowCount, columnCount, Visibility.Visible), FontWeights.Normal);
                                    }
                                    else
                                    {
                                        this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(displayValue, new Thickness(8, 6, 8, 6), VerticalAlignment.Stretch, HorizontalAlignment.Stretch, TextAlignment.Left, rowCount, columnCount, Visibility.Visible), FontWeights.Normal);
                                    }
                                    columnCount++;
                                }

                                rowCount++;

                                if (rowCount > maxRows)
                                {
                                    break;
                                }
                            }
                            
                            this.Dispatcher.BeginInvoke(drawBorders, strataValue);

                            if (rowCount > maxRows)
                            {
                                exceededMaxRows = true;
                            }
                        }

                        for(int i = 0; i < lineListTables.Count; i++)
                        {
                            lineListTables[i].Dispose();
                        }
                        lineListTables.Clear();
                    }

                    if (exceededMaxRows && exceededMaxColumns)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithWarningDelegate(RenderFinishWithWarning), "Warning: Some rows and columns were not displayed due to gadget settings. Showing top " + maxRows.ToString() + " rows and top " + maxColumns.ToString() + " columns only.");
                    }
                    else if (exceededMaxColumns)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithWarningDelegate(RenderFinishWithWarning), "Warning: Some columns were not displayed due to gadget settings. Showing top " + maxColumns.ToString() + " columns only.");
                    }
                    else if (exceededMaxRows)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithWarningDelegate(RenderFinishWithWarning), string.Format(SharedStrings.DASHBOARD_GADGET_STATUS_ROW_LIMIT, maxRows.ToString()));
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));
                    }
                    // Now called after drawing all the borders.
                    // this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState)); 
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                }
                finally
                {   
                    stopwatch.Stop();
                    Debug.Print("Line list gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete with " + DashboardHelper.RecordCount.ToString() + " records and the following filters:");
                    Debug.Print(DashboardHelper.DataFilters.GenerateDataFilterString());
                }
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Used to construct the gadget and assign events
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper to attach to this gadget</param>
        protected override void Construct()
        {
            currentWidth = borderAll.ActualWidth;

            if (!string.IsNullOrEmpty(CustomOutputHeading))
            {
                headerPanel.Text = CustomOutputHeading;
            }

            StrataGridList = new List<Grid>();
            groupSvList = new List<ScrollViewer>();
            StrataExpanderList = new List<Expander>();
            gridLabelsList = new List<TextBlock>();

            cbxSortField.SelectionChanged += new SelectionChangedEventHandler(cbxSortField_SelectionChanged);
            requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
            checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

            mnuCopy.Click += new RoutedEventHandler(mnuCopy_Click);
            mnuSendDataToHTML.Click += new RoutedEventHandler(mnuSendDataToHTML_Click);
            mnuRemoveSorts.Click += new RoutedEventHandler(mnuRemoveSorts_Click);

#if LINUX_BUILD
            mnuSendDataToExcel.Visibility = Visibility.Collapsed;
#else
            mnuSendDataToExcel.Visibility = Visibility.Visible;
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.ClassesRoot; Microsoft.Win32.RegistryKey excelKey = key.OpenSubKey("Excel.Application"); bool excelInstalled = excelKey == null ? false : true; key = Microsoft.Win32.Registry.ClassesRoot;
            excelKey = key.OpenSubKey("Excel.Application");
            excelInstalled = excelKey == null ? false : true;

            if (!excelInstalled)
            {
                mnuSendDataToExcel.Visibility = Visibility.Collapsed;
            }
            else
            {
                mnuSendDataToExcel.Click += new RoutedEventHandler(mnuSendDataToExcel_Click);
            }
#endif

            mnuRemove.Click += new RoutedEventHandler(mnuRemove_Click);
            mnuSwapSortType.Click += new RoutedEventHandler(mnuSwapSortType_Click);
            if (!IsHostedByEnter)
            {
                mnuSendToBack.Click += new RoutedEventHandler(mnuSendToBack_Click);
                //mnuRefresh.Click += new RoutedEventHandler(mnuRefresh_Click);
                mnuClose.Click += new RoutedEventHandler(mnuClose_Click);
                mnuCenter.Click += new RoutedEventHandler(mnuCenter_Click);
                mnuSendDataToHTML.Visibility = System.Windows.Visibility.Visible;
                mnuClose.Visibility = System.Windows.Visibility.Visible;
                //mnuCenter.Visibility = System.Windows.Visibility.Collapsed;//Visible; // TODO: Re-enable.
            }
            else
            {
                mnuSendDataToHTML.Visibility = System.Windows.Visibility.Collapsed;
                mnuClose.Visibility = System.Windows.Visibility.Collapsed;
                mnuCenter.Visibility = System.Windows.Visibility.Collapsed;
                borderAll.ContextMenu = null;
                headerPanel.IsCloseButtonAvailable = false;
                headerPanel.IsDescriptionButtonAvailable = false;
                headerPanel.IsFilterButtonAvailable = false;
                headerPanel.IsOutputCollapseButtonAvailable = false;
                pathTriangle.Margin = new Thickness(pathTriangle.Margin.Left, pathTriangle.Margin.Top, 8, pathTriangle.Margin.Bottom);
                grdDimensions.Visibility = System.Windows.Visibility.Collapsed;
            }

            columnWarningShown = false;

            this.IsProcessing = false;
            this.MaxColumns = 25;

            this.GadgetStatusUpdate += new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(IsCancelled);

            base.Construct();

            lbxFields.SelectedIndex = -1;

            #region Translation
            mnuCenter.Header = DashboardSharedStrings.GADGET_CENTER;
            mnuClose.Header = LineListSharedStrings.CMENU_CLOSE;
            mnuCopy.Header = LineListSharedStrings.CMENU_COPY;
            mnuRemoveSorts.Header = LineListSharedStrings.CMENU_REMOVE_SORTING;
            mnuSendDataToExcel.Header = LineListSharedStrings.CMENU_EXCEL;
            mnuSendDataToHTML.Header = LineListSharedStrings.CMENU_HTML;
            mnuSendToBack.Header = LineListSharedStrings.CMENU_SEND_TO_BACK;

            mnuSwapSortType.Header = LineListSharedStrings.CMENU_SWAP_SORT;
            mnuRemove.Header = LineListSharedStrings.CMENU_REMOVE_FIELD;

            tblockListVariablesToDisplay.Text = LineListSharedStrings.LIST_VARIABLES_TO_DISPLAY;
            tblockSortVariables.Text = LineListSharedStrings.SORT_VARIABLES;
            tblockSortOrder.Text = LineListSharedStrings.SORT_ORDER;
            tblockLineListProperties.Text = LineListSharedStrings.LIST_CONFIG_TITLE;
            tblockGroupResultsBy.Text = LineListSharedStrings.GROUP_RESULTS_BY;
            tblockMaxVarNameLength.Text = LineListSharedStrings.MAX_VAR_NAME_LENGTH;
            tblockMaxRows.Text = LineListSharedStrings.MAX_ROWS_TO_DISPLAY;

            tblockMaxWidth.Text = DashboardSharedStrings.GADGET_MAX_WIDTH;
            tblockMaxHeight.Text = DashboardSharedStrings.GADGET_MAX_HEIGHT;

            tblockInst1.Text = LineListSharedStrings.INSTRUCTIONS_1;
            tblockInst2.Text = LineListSharedStrings.INSTRUCTIONS_2;

            checkboxTabOrder.Content = LineListSharedStrings.SORT_VARS_TAB_ORDER;
            checkboxUsePrompts.Content = LineListSharedStrings.USE_FIELD_PROMPTS;
            checkboxListLabels.Content = DashboardSharedStrings.GADGET_LIST_LABELS;
            checkboxAllowUpdates.Content = LineListSharedStrings.ALLOW_UPDATES;
            checkboxLineColumn.Content = LineListSharedStrings.SHOW_LINE_COLUMN;
            checkboxColumnHeaders.Content = LineListSharedStrings.SHOW_COLUMN_HEADINGS;
            checkboxShowNulls.Content = LineListSharedStrings.SHOW_MISSING_REPRESENTATION;

            btnRun.Content = LineListSharedStrings.GENERATE_LINE_LIST;
            #endregion // Translation
        }

        void mnuCenter_Click(object sender, RoutedEventArgs e)
        {
            CenterGadget();
        }

        /// <summary>
        /// Closes the gadget
        /// </summary>
        protected override void CloseGadget()
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

            if (GadgetOptions != null)
            {
                GadgetOptions.GadgetStatusUpdate -= new GadgetStatusUpdateHandler(requestUpdateStatus);
                GadgetOptions.GadgetCheckForCancellation -= new GadgetCheckForCancellationHandler(checkForCancellation);
                GadgetOptions = null;
            }

            this.GadgetStatusUpdate -= new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation -= new GadgetCheckForCancellationHandler(IsCancelled);

            for (int i = 0; i < StrataGridList.Count; i++)
            {
                StrataGridList[i].Children.Clear();
            }
            for (int i = 0; i < groupSvList.Count; i++)
            {
                groupSvList[i].Content = null;
            }
            for (int i = 0; i < StrataExpanderList.Count; i++)
            {
                StrataExpanderList[i].Content = null;
            }
            this.StrataGridList.Clear();
            this.groupSvList.Clear();
            this.StrataExpanderList.Clear();

            cbxSortField.SelectionChanged -= new SelectionChangedEventHandler(cbxSortField_SelectionChanged);

            requestUpdateStatus = null;
            checkForCancellation = null;

            mnuRemove.Click -= new RoutedEventHandler(mnuRemove_Click);
            mnuSwapSortType.Click -= new RoutedEventHandler(mnuSwapSortType_Click);
            if (!IsHostedByEnter)
            {
                mnuSendToBack.Click -= new RoutedEventHandler(mnuSendToBack_Click);
                //mnuRefresh.Click -= new RoutedEventHandler(mnuRefresh_Click);
                mnuClose.Click -= new RoutedEventHandler(mnuClose_Click);
            }

            base.CloseGadget();

            GadgetOptions = null;
        }

        /// <summary>
        /// Handles the filling of the gadget's combo boxes
        /// </summary>
        private void FillComboboxes(bool update = false)
        {
            LoadingCombos = true;

            string prevGroup = string.Empty;

            if (update)
            {
                if (cbxGroupField.SelectedIndex >= 0)
                {
                    prevGroup = cbxGroupField.SelectedItem.ToString();
                }
            }

            cbxGroupField.ItemsSource = null;
            cbxGroupField.Items.Clear();            

            if (!update)
            {
                lbxFields.ItemsSource = null;
                lbxFields.Items.Clear();
                lbxSortFields.Items.Clear();

                List<string> fieldNames = new List<string>();
                List<string> strataFieldNames = new List<string>();
                ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.DateTime | ColumnDataType.UserDefined;
                strataFieldNames = DashboardHelper.GetFieldsAsList(columnDataType);
                strataFieldNames.Add(string.Empty);
                strataFieldNames.Sort();

                fieldNames = DashboardHelper.GetFieldsAsList();

                if (fieldNames.Contains("SYSTEMDATE"))
                {
                    fieldNames.Remove("SYSTEMDATE");
                }

                if (strataFieldNames.Contains("SYSTEMDATE"))
                {
                    strataFieldNames.Remove("SYSTEMDATE");
                }

                //fieldNames.AddRange(DashboardHelper.GetGroupFieldsAsList());
                fieldNames.AddRange(DashboardHelper.GetAllGroupsAsList());

                if (DashboardHelper.IsUsingEpiProject)
                {
                    for (int i = 0; i < this.View.Pages.Count; i++)
                    {
                        fieldNames.Add("Page " + (i + 1).ToString());
                    }
                }                

                cbxGroupField.ItemsSource = strataFieldNames;
                cbxSortField.ItemsSource = strataFieldNames;
                lbxFields.ItemsSource = fieldNames;

                if (cbxGroupField.Items.Count > 0)
                {
                    cbxGroupField.SelectedIndex = -1;
                }
            }
            else
            {
                List<string> selectedListFields = new List<string>();

                foreach (string s in lbxFields.SelectedItems)
                {
                    selectedListFields.Add(s);
                }

                lbxFields.ItemsSource = null;
                lbxFields.Items.Clear();                

                List<string> fieldNames = new List<string>();
                List<string> strataFieldNames = new List<string>();
                ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.DateTime | ColumnDataType.UserDefined;
                strataFieldNames = DashboardHelper.GetFieldsAsList(columnDataType);
                strataFieldNames.Add(string.Empty);
                strataFieldNames.Sort();

                fieldNames = DashboardHelper.GetFieldsAsList();
                
                if (fieldNames.Contains("SYSTEMDATE"))
                {
                    fieldNames.Remove("SYSTEMDATE");
                }

                fieldNames.AddRange(DashboardHelper.GetAllGroupsAsList());

                if (DashboardHelper.IsUsingEpiProject)
                {
                    for (int i = 0; i < this.View.Pages.Count; i++)
                    {
                        fieldNames.Add("Page " + (i + 1).ToString());
                    }
                }

                cbxGroupField.ItemsSource = strataFieldNames;
                cbxSortField.ItemsSource = strataFieldNames;
                lbxFields.ItemsSource = fieldNames;

                if (cbxGroupField.Items.Count > 0)
                {
                    cbxGroupField.SelectedIndex = -1;
                }

                for (int i = lbxSortFields.Items.Count - 1; i == 0; i--)
                {
                    string s = lbxSortFields.Items[i].ToString();

                    if (!strataFieldNames.Contains(s))
                    {
                        lbxSortFields.Items.Remove(s);
                    }
                }

                foreach (string s in selectedListFields)
                {
                    if (fieldNames.Contains(s))
                    {
                        lbxFields.SelectedItems.Add(s);
                    }
                }
            }

            if (update)
            {
                cbxGroupField.SelectedItem = prevGroup;
            }

            LoadingCombos = false;
        }

        /// <summary>
        /// Used to add a new Line List grid to the gadget's output
        /// </summary>
        /// <param name="groupVar">The name of the group variable selected, if any</param>
        /// <param name="value">The value by which this grid has been grouped by</param>
        private void AddLineListGrid(string groupVar, string value, int columnCount)
        {
            ScrollViewer sv = new ScrollViewer();
            sv.Style = this.Resources["gadgetScrollViewer"] as Style;
            sv.Tag = value;
            if (IsHostedByEnter)
            {
                sv.MaxHeight = System.Windows.SystemParameters.PrimaryScreenHeight - 180;
                sv.MaxWidth = System.Windows.SystemParameters.PrimaryScreenWidth - 150;
                sv.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            }
            else
            {
                //sv.MaxHeight = System.Windows.SystemParameters.PrimaryScreenHeight - 300;
                //sv.MaxWidth = System.Windows.SystemParameters.PrimaryScreenWidth - 250;

                int width = 800;
                int height = 600;

                if (int.TryParse(txtMaxWidth.Text, out width))
                {
                    sv.MaxWidth = width;
                }
                else
                {
                    sv.MaxWidth = double.NaN;
                }

                if (int.TryParse(txtMaxHeight.Text, out height))
                {
                    sv.MaxHeight = height;
                }
                else
                {
                    sv.MaxHeight = double.NaN;
                }
            }
            
            sv.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            Grid grid = new Grid();
            grid.Tag = value;
            grid.Style = this.Resources["genericOutputGrid"] as Style;
            grid.Visibility = System.Windows.Visibility.Collapsed;

            Border border = new Border();
            border.Style = this.Resources["genericOutputGridBorder"] as Style;
            border.Child = grid;

            for (int i = 0; i < columnCount; i++)
            {
                if (i > MaxColumns)
                {
                    break;
                }

                ColumnDefinition column = new ColumnDefinition();
                column.Width = GridLength.Auto;
                grid.ColumnDefinitions.Add(column);
            }

            if (checkboxLineColumn.IsChecked == false)
            {
                grid.ColumnDefinitions[0].Width = new GridLength(1);
            }

            ColumnDefinition totalColumn = new ColumnDefinition();
            totalColumn.Width = GridLength.Auto;
            grid.ColumnDefinitions.Add(totalColumn);

            TextBlock txtGridLabel = new TextBlock();
            txtGridLabel.Text = value;
            txtGridLabel.HorizontalAlignment = HorizontalAlignment.Left;
            txtGridLabel.VerticalAlignment = VerticalAlignment.Bottom;
            txtGridLabel.Margin = new Thickness(2, 2, 2, 2);
            txtGridLabel.FontWeight = FontWeights.Bold;

            sv.Content = border;

            Expander expander = new Expander();

            TextBlock txtExpanderHeader = new TextBlock();
            txtExpanderHeader.Text = value;
            txtExpanderHeader.Style = this.Resources["genericOutputExpanderText"] as Style;
            expander.Header = txtExpanderHeader;

            if (string.IsNullOrEmpty(groupVar))
            {
                txtGridLabel.Visibility = System.Windows.Visibility.Collapsed;
                panelMain.Children.Add(sv);
            }
            else
            {
                expander.Margin = (Thickness)this.Resources["expanderMargin"]; //new Thickness(6, 2, 6, 6);                
                sv.Margin = new Thickness(0, 4, 0, 4);
                expander.Content = sv;
                expander.IsExpanded = true;
                panelMain.Children.Add(expander);
                StrataExpanderList.Add(expander);
            }

            StrataGridList.Add(grid);
            groupSvList.Add(sv);          
        }

        private void SetGridText(string strataValue, TextBlockConfig textBlockConfig, FontWeight fontWeight)
        {
            Grid grid = new Grid();

            grid = GetStrataGrid(strataValue);

            TextBlock txt = new TextBlock();
            txt.Foreground = this.Resources["cellForeground"] as SolidColorBrush;
            txt.FontWeight = fontWeight;
            txt.Text = textBlockConfig.Text;
            txt.Margin = textBlockConfig.Margin;
            txt.VerticalAlignment = textBlockConfig.VerticalAlignment;
            txt.HorizontalAlignment = textBlockConfig.HorizontalAlignment;
            txt.TextAlignment = textBlockConfig.TextAlignment;
            txt.TextWrapping = TextWrapping.Wrap;
            txt.MaxWidth = 300;
            txt.Visibility = textBlockConfig.ControlVisibility;
            if (IsHostedByEnter)
            {                
                txt.MouseEnter += new MouseEventHandler(rowDef_MouseEnter);
                txt.MouseLeave += new MouseEventHandler(rowDef_MouseLeave);
                txt.MouseUp += new MouseButtonEventHandler(rowDef_MouseUp);
            }
            Grid.SetZIndex(txt, 1000);
            Grid.SetRow(txt, textBlockConfig.RowNumber);
            Grid.SetColumn(txt, textBlockConfig.ColumnNumber);
            grid.Children.Add(txt);

            if (checkboxAllowUpdates.IsChecked == true)
            {
                //txt.MouseLeftButtonDown += new MouseButtonEventHandler(gridCell_MouseLeftButtonDown); 
            }
        }

        private struct AllowUpdateMetaData
        {
            public UIElement GridCellTextBlock;
            public MetaFieldType FieldType;
            public object Value;
            int Column;
            int Row;
            string Key;
            string FieldName;

            public AllowUpdateMetaData(UIElement gridCellTextBlock, MetaFieldType fieldType, string fieldName, object value, int column, int row, string key)
            {
                GridCellTextBlock = gridCellTextBlock;
                FieldType = fieldType;
                FieldName = fieldName;
                Value = value;
                Column = column;
                Row = row;
                Key = key;
            }
        }

        private void gridCell_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (sender is TextBlock)
            {
                int row = Grid.GetRow((UIElement)sender);
                int column = Grid.GetColumn((UIElement)sender);
                Grid grid = (sender as TextBlock).Parent as Grid;

                if (allowUpdateBox == null)
                {
                    //UIElement element = ((sender as TextBlock).Parent as Grid).Children.Cast<UIElement>().First(x => Grid.GetRow(x) == 0 && Grid.GetColumn(x) == column);
                    IEnumerable<UIElement> elements = grid.Children.Cast<UIElement>().Where(x => Grid.GetRow(x) == 0 && Grid.GetColumn(x) == column);
                    UIElement element = new UIElement();

                    foreach (UIElement uie in elements)
                    {
                        if (uie is TextBlock)
                        {
                            element = uie;
                            break;
                        }
                    }

                    if (element is TextBlock)
                    {
                        TextBlock columnTextBlock = element as TextBlock;
                        Field field = DashboardHelper.GetAssociatedField(columnTextBlock.Text);
                        if (field != null)
                        {
                            TextBlock gridCellTextBlock = sender as TextBlock;

                            switch (field.FieldType)
                            {
                                case MetaFieldType.Checkbox:
                                    allowUpdateBox = new ComboBox();
                                    (allowUpdateBox as ComboBox).Items.Add(Config.Settings.RepresentationOfYes);
                                    (allowUpdateBox as ComboBox).Items.Add(Config.Settings.RepresentationOfNo);
                                    (allowUpdateBox as ComboBox).SelectedItem = gridCellTextBlock.Text;
                                    break;
                                case MetaFieldType.YesNo:
                                    allowUpdateBox = new ComboBox();
                                    (allowUpdateBox as ComboBox).Items.Add(Config.Settings.RepresentationOfYes);
                                    (allowUpdateBox as ComboBox).Items.Add(Config.Settings.RepresentationOfNo);
                                    (allowUpdateBox as ComboBox).Items.Add(Config.Settings.RepresentationOfMissing);
                                    (allowUpdateBox as ComboBox).SelectedItem = gridCellTextBlock.Text;
                                    break;
                                case MetaFieldType.Text:
                                    allowUpdateBox = new TextBox();
                                    (allowUpdateBox as TextBox).Text = gridCellTextBlock.Text;
                                    break;
                                default:
                                    return;
                            }

                            int keyColumn = grid.ColumnDefinitions.Count - 1;
                            elements = grid.Children.Cast<UIElement>().Where(x => Grid.GetRow(x) == row && Grid.GetColumn(x) == keyColumn);
                            string key = string.Empty;

                            foreach (UIElement uie in elements)
                            {
                                if (uie is TextBlock)
                                {
                                    key = (uie as TextBlock).Text;
                                }
                            }

                            //allowUpdateBox = new TextBox();
                            allowUpdateBox.Margin = new Thickness(2);
                            //allowUpdateBox.Tag = new AllowUpdateMetaData(gridCellTextBlock, field.FieldType, gridCellTextBlock.Text, column, row, key);  //gridCellTextBlock;
                            Grid.SetZIndex(allowUpdateBox, 2000);
                            Grid.SetRow(allowUpdateBox, row);
                            Grid.SetColumn(allowUpdateBox, column);
                            ((Grid)((FrameworkElement)sender).Parent).Children.Add(allowUpdateBox);
                            allowUpdateBox.KeyDown += new KeyEventHandler(cellTextBox_KeyDown);
                        }
                    }
                }
                else
                {
                    allowUpdateBox.Visibility = System.Windows.Visibility.Collapsed;
                    allowUpdateBox.KeyDown -= new KeyEventHandler(cellTextBox_KeyDown);
                    allowUpdateBox = null;
                }
            }
        }

        private bool UpdateDatabase(UIElement inputControl, object value, int column)
        {
            Grid grid = (inputControl as FrameworkElement).Parent as Grid;
            string columnName = string.Empty;
            string key = string.Empty;

            int row = Grid.GetRow((FrameworkElement)inputControl);
            int keyColumn = grid.ColumnDefinitions.Count - 1;

            IEnumerable<UIElement> elements = grid.Children.Cast<UIElement>().Where(x => Grid.GetRow(x) == row && Grid.GetColumn(x) == keyColumn);

            foreach (UIElement uie in elements)
            {
                if (uie is TextBlock)
                {
                    key = (uie as TextBlock).Text;
                }
            }

            elements = grid.Children.Cast<UIElement>().Where(x => Grid.GetRow(x) == 0 && Grid.GetColumn(x) == column);
            UIElement element = new UIElement();

            foreach (UIElement uie in elements)
            {
                if (uie is TextBlock)
                {
                    columnName = (uie as TextBlock).Text;                    
                }
            }

            Field field = DashboardHelper.GetAssociatedField(columnName);
            if (field != null)
            {
                // Todo: Move this to a separate class. This should not be in the UI.
                DataView dv = new DataView(DashboardHelper.DataSet.Tables[0]);
                dv.RowFilter = "[UniqueKey] = " + key;
                if (dv.Count == 1)
                {
                    dv[0][columnName] = value;
                }
            }

            return false;
        }

        private void cellTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox)
            {
                int row = Grid.GetRow((UIElement)sender);
                int column = Grid.GetColumn((UIElement)sender);
                TextBox textBox = (sender as TextBox);
                TextBlock textBlock = textBox.Tag as TextBlock; 

                if (e.Key == Key.Enter)
                {
                    allowUpdateBox.Visibility = System.Windows.Visibility.Collapsed;
                    textBlock.Text = textBox.Text;
                    allowUpdateBox.KeyDown -= new KeyEventHandler(cellTextBox_KeyDown);
                    allowUpdateBox = null;
                }
                else if (e.Key == Key.Escape)
                {
                    allowUpdateBox.Visibility = System.Windows.Visibility.Collapsed;
                    allowUpdateBox.KeyDown -= new KeyEventHandler(cellTextBox_KeyDown);
                    allowUpdateBox = null;
                }
            }
            else if (sender is ComboBox)
            {
                int row = Grid.GetRow((UIElement)sender);
                int column = Grid.GetColumn((UIElement)sender);
                ComboBox textBox = (sender as ComboBox);
                TextBlock textBlock = textBox.Tag as TextBlock;

                if (e.Key == Key.Enter)
                {
                    allowUpdateBox.Visibility = System.Windows.Visibility.Collapsed;
                    textBlock.Text = textBox.SelectedItem.ToString();
                    allowUpdateBox.KeyDown -= new KeyEventHandler(cellTextBox_KeyDown);
                    allowUpdateBox = null;
                }
                else if (e.Key == Key.Escape)
                {
                    allowUpdateBox.Visibility = System.Windows.Visibility.Collapsed;
                    allowUpdateBox.KeyDown -= new KeyEventHandler(cellTextBox_KeyDown);
                    allowUpdateBox = null;
                }
            }
        }


        private void SetGridImage(string strataValue, byte[] imageBlob, TextBlockConfig textBlockConfig, FontWeight fontWeight)
        {
            Grid grid = new Grid();

            grid = GetStrataGrid(strataValue);

            Image image = new Image();
            System.IO.MemoryStream stream = new System.IO.MemoryStream(imageBlob);
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            image.Source = bitmap;
            image.Height = bitmap.Height;
            image.Width = bitmap.Width;

            if (IsHostedByEnter)
            {
                image.MouseEnter += new MouseEventHandler(rowDef_MouseEnter);
                image.MouseLeave += new MouseEventHandler(rowDef_MouseLeave);
                image.MouseUp += new MouseButtonEventHandler(rowDef_MouseUp);
            }
            Grid.SetZIndex(image, 1000);
            Grid.SetRow(image, textBlockConfig.RowNumber);
            Grid.SetColumn(image, textBlockConfig.ColumnNumber);
            grid.Children.Add(image);
        }

        void colDef_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (draggedColumnIndex == -1 || IsProcessing)
            {
                return;
            }

            columnOrder = new List<string>();
        
            Grid grid = (Grid)((FrameworkElement)sender).Parent;            
            int destinationColumn = Grid.GetColumn((FrameworkElement)sender);

            if (destinationColumn < draggedColumnIndex)
            {
                foreach (UIElement element in grid.Children)
                {
                    int currentColumn = Grid.GetColumn(element);
                    if (currentColumn == draggedColumnIndex)
                    {
                        Grid.SetColumn(element, destinationColumn);
                    }
                    else if (currentColumn == destinationColumn)
                    {
                        Grid.SetColumn(element, destinationColumn + 1);
                    }
                    else if (currentColumn >= destinationColumn && currentColumn < draggedColumnIndex)
                    {
                        Grid.SetColumn(element, currentColumn + 1);
                    }
                }
            }
            else if (destinationColumn > draggedColumnIndex)
            {
                foreach (UIElement element in grid.Children)
                {
                    int currentColumn = Grid.GetColumn(element);
                    if (currentColumn == draggedColumnIndex)
                    {
                        Grid.SetColumn(element, destinationColumn);
                    }
                    else if (currentColumn == destinationColumn)
                    {
                        Grid.SetColumn(element, destinationColumn - 1);
                    }
                    else if (currentColumn < destinationColumn && currentColumn > draggedColumnIndex)
                    {
                        Grid.SetColumn(element, currentColumn - 1);
                    }
                }
            }

            draggedColumnIndex = -1;
        }

        void colDef_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsProcessing)
            {
                draggedColumnIndex = -1;
            }
            else
            {
                Grid grid = (Grid)((FrameworkElement)sender).Parent;
                int column = Grid.GetColumn((FrameworkElement)sender);
                draggedColumnIndex = column;
            }
        }

        void rowDef_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Grid grid = StrataGridList[0]; //new Grid();

            //if (sender is Rectangle)
            //{
            //    grid = (((sender as Rectangle).Parent) as Border).Parent as Grid;
            //}
            int row = Grid.GetRow((FrameworkElement)sender);
            int column = grid.ColumnDefinitions.Count - 1;
            //UIElement element = grid.Children.Cast<UIElement>().First(x => Grid.GetRow(x) == row && Grid.GetColumn(x) == column);
            //if (element is TextBlock)
            //{
            //    if (RecordSelected != null)
            //    {
            //        RecordSelected(int.Parse(((TextBlock)element).Text));
            //    }
            //}
            //else if (element is Rectangle)
            //{
                IEnumerable<UIElement> elements = grid.Children.Cast<UIElement>().Where(x => Grid.GetRow(x) == row && Grid.GetColumn(x) == column);

                foreach (UIElement element in elements)
                {
                    if (element is TextBlock)
                    {
                        if (RecordSelected != null)
                        {
                            RecordSelected(int.Parse(((TextBlock)element).Text));
                        }
                    }
                }
            //}
        }

        //private void AddGridRow(string strataValue, int height)
        //{
        //    Grid grid = GetStrataGrid(strataValue);
        //    waitPanel.Visibility = System.Windows.Visibility.Collapsed;
        //    grid.Visibility = Visibility.Visible;
        //    RowDefinition rowDef = new RowDefinition();
        //    rowDef.Height = GridLength.Auto;
        //    grid.RowDefinitions.Add(rowDef);
        //}

        void rowDef_MouseLeave(object sender, MouseEventArgs e)
        {
            if (highlightRowRectangle != null && StrataGridList.Count > 0 && StrataGridList[0].Children.Contains(highlightRowRectangle))
            {
                StrataGridList[0].Children.Remove(highlightRowRectangle);
            }
            this.Cursor = Cursors.Arrow;
        }

        void rowDef_MouseEnter(object sender, MouseEventArgs e)
        {
            int row = Grid.GetRow((UIElement)sender);

            if(sender is Rectangle) 
            {
                Rectangle rectangle = sender as Rectangle;
                if (rectangle.Parent is Border)
                {
                    Border border = rectangle.Parent as Border;
                    row = Grid.GetRow(border);
                }
            }
            
            if (highlightRowRectangle == null)
            {
                highlightRowRectangle = new Rectangle();
                highlightRowRectangle.IsHitTestVisible = false;
                highlightRowRectangle.Fill = Brushes.LightBlue;
                highlightRowRectangle.Opacity = 0.25;
                highlightRowRectangle.Margin = new Thickness(0);
                //highlightRowRectangle.StrokeThickness = 3;
                Grid.SetZIndex(highlightRowRectangle, 2000);
            }
            Grid.SetRow(highlightRowRectangle, row);
            Grid.SetColumn(highlightRowRectangle, 0);
            Grid.SetColumnSpan(highlightRowRectangle, StrataGridList[0].ColumnDefinitions.Count); //((Grid)((FrameworkElement)sender).Parent);
            //Grid.SetColumnSpan(highlightRowRectangle, (((Border)((FrameworkElement)sender).Parent).Child as Grid).ColumnDefinitions.Count); //((Grid)((FrameworkElement)sender).Parent);
            StrataGridList[0].Children.Add(highlightRowRectangle);
            
            
            //.ColumnDefinitions.Count);
            this.Cursor = Cursors.Hand;
        }

        void rctHeader_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Rectangle)
            {
                Rectangle rect = sender as Rectangle;
                rect.Fill = this.Resources["lineListRowHighlightBrush"] as SolidColorBrush;
                rect.Opacity = 0.4;
                this.Cursor = Cursors.Hand;
            }
        }

        void rctHeader_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Rectangle)
            {
                Rectangle rect = sender as Rectangle;
                rect.Fill = this.Resources["lineListRowHighlightBrush"] as SolidColorBrush;
                rect.Opacity = 1.0;
                this.Cursor = Cursors.Arrow;
            }
        }

        void rctHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle && (sender as Rectangle).Tag != null)
            {
                clickedColumnName = (sender as Rectangle).Tag.ToString();
            }
        }

        void rctHeader_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Rectangle)
            {
                Rectangle rect = sender as Rectangle;

                if (rect.Tag != null && rect.Tag.ToString() == clickedColumnName)
                {
                    string columnName = rect.Tag.ToString();
                    if (!lbxSortFields.Items.Contains(columnName + " (ascending)") && !lbxSortFields.Items.Contains(columnName + " (descending)"))
                    {
                        InsertColumnInSortList(columnName, 0);
                        this.RefreshResults();
                    }
                    else if (lbxSortFields.Items.Contains(columnName + " (ascending)") || lbxSortFields.Items.Contains(columnName + " (descending)"))
                    {
                        SwapColumnOrderInSortList(columnName);
                        this.RefreshResults();
                    }
                }
            }
        }

        bool IsSortingBy(string columnName, out SortOrder sortOrder)
        {
            sortOrder = SortOrder.Ascending;

            foreach (KeyValuePair<string, string> kvp in this.GadgetOptions.InputVariableList)
            {
                if (kvp.Value.Equals("sortfield"))
                {
                    string key = kvp.Key.Substring(0, kvp.Key.Length - 4);
                    string order = kvp.Key.Substring(kvp.Key.Length - 5, 4);

                    if (order.Contains("DES"))
                    {
                        sortOrder = SortOrder.Descending;
                    }

                    key = key.Trim().TrimEnd(']').TrimStart('[');
                    if (key == columnName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void RenderFrequencyHeader(string strataValue, string freqVar, DataColumnCollection columns)
        {
            Grid grid = GetStrataGrid(strataValue);

            RowDefinition rowDefHeader = new RowDefinition();
            rowDefHeader.Height = new GridLength(30);
            grid.RowDefinitions.Add(rowDefHeader);

            for (int y = 0; y < grid.ColumnDefinitions.Count; y++)
            {
                Rectangle rctHeader = new Rectangle();
                rctHeader.Style = this.Resources["gridHeaderCellRectangle"] as Style;

                if (y >= 1)
                {
                    rctHeader.Tag = columns[y - 1].ColumnName.Trim();                    
                }
                
                //rctHeader.Fill = headerBackgroundBrush; // new SolidColorBrush(Color.FromRgb(112, 146, 190)); // SystemColors.MenuHighlightBrush;
                Grid.SetRow(rctHeader, 0);
                Grid.SetColumn(rctHeader, y);
                grid.Children.Add(rctHeader);

                rctHeader.MouseUp += new MouseButtonEventHandler(colDef_MouseUp);
                rctHeader.MouseDown += new MouseButtonEventHandler(colDef_MouseDown);

                rctHeader.MouseEnter += new MouseEventHandler(rctHeader_MouseEnter);
                rctHeader.MouseLeave += new MouseEventHandler(rctHeader_MouseLeave);
                rctHeader.MouseLeftButtonDown += new MouseButtonEventHandler(rctHeader_MouseLeftButtonDown);                
                rctHeader.MouseLeftButtonUp += new MouseButtonEventHandler(rctHeader_MouseLeftButtonUp);                
            }

            int maxColumnLength = MaxColumnLength;

            TextBlock txtRowTotalHeader = new TextBlock();
            txtRowTotalHeader.Text = "Line";
            txtRowTotalHeader.Style = this.Resources["columnHeadingText"] as Style;
            
            Grid.SetRow(txtRowTotalHeader, 0);
            Grid.SetColumn(txtRowTotalHeader, 0);
            grid.Children.Add(txtRowTotalHeader);

            if (checkboxColumnHeaders.IsChecked == false)
            {
                grid.RowDefinitions[0].Height = new GridLength(1);
            }

            for (int i = 0; i < columns.Count; i++)
            {
                if (i > MaxColumns)
                {
                    break;
                }
                TextBlock txtColHeader = new TextBlock();
                string columnName = columns[i].ColumnName.Trim();

                SortOrder order = SortOrder.Ascending;
                if (IsSortingBy(columnName, out order))
                {
                    if (order == SortOrder.Ascending)
                    {
                        Controls.AscendingSortArrow arrowAsc = new Controls.AscendingSortArrow();
                        arrowAsc.IsHitTestVisible = false;
                        arrowAsc.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        arrowAsc.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                        arrowAsc.Opacity = 0.5;
                        arrowAsc.Margin = new Thickness(2);
                        Grid.SetRow(arrowAsc, 0);
                        Grid.SetColumn(arrowAsc, i + 1);
                        grid.Children.Add(arrowAsc);
                    }
                    else
                    {
                        Controls.DescendingSortArrow arrowDesc = new Controls.DescendingSortArrow();
                        arrowDesc.IsHitTestVisible = false;
                        arrowDesc.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        arrowDesc.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                        arrowDesc.Opacity = 0.5;
                        arrowDesc.Margin = new Thickness(2);
                        Grid.SetRow(arrowDesc, 0);
                        Grid.SetColumn(arrowDesc, i + 1);
                        grid.Children.Add(arrowDesc);
                    }
                }

                if (GadgetOptions.InputVariableList.ContainsKey("usepromptsforcolumnnames") && GadgetOptions.InputVariableList["usepromptsforcolumnnames"].ToLower().Equals("true"))
                {
                    Field field = DashboardHelper.GetAssociatedField(columns[i].ColumnName);
                    if (field != null && field is RenderableField)
                    {
                        columnName = ((RenderableField)field).PromptText;
                    }
                }

                if (columnName.Length > maxColumnLength)
                {
                    columnName = columnName.Substring(0, maxColumnLength) + StringLiterals.ELLIPSIS;
                }

                txtColHeader.Text = columnName;
                txtColHeader.IsHitTestVisible = false;
                txtColHeader.Style = this.Resources["columnHeadingText"] as Style;

                txtColHeader.MouseUp += new MouseButtonEventHandler(colDef_MouseUp);
                txtColHeader.MouseDown += new MouseButtonEventHandler(colDef_MouseDown);

                Grid.SetRow(txtColHeader, 0);
                Grid.SetColumn(txtColHeader, i + 1);
                if (IsHostedByEnter || checkboxAllowUpdates.IsChecked == true)
                {
                    if (i < columns.Count - 1)
                    {
                        grid.Children.Add(txtColHeader);
                    }
                }
                else
                {
                    grid.Children.Add(txtColHeader);
                }
            }
        }

        private void RenderFrequencyFooter(string strataValue, int footerRowIndex, int[] totalRows)
        {
            Grid grid = GetStrataGrid(strataValue);

            RowDefinition rowDefTotals = new RowDefinition();
            rowDefTotals.Height = new GridLength(30);
            grid.RowDefinitions.Add(rowDefTotals);

            TextBlock txtValTotals = new TextBlock();
            txtValTotals.Text = StringLiterals.SPACE + SharedStrings.TOTAL + StringLiterals.SPACE;
            txtValTotals.Margin = new Thickness(2, 0, 2, 0);
            txtValTotals.VerticalAlignment = VerticalAlignment.Center;
            txtValTotals.FontWeight = FontWeights.Bold;
            Grid.SetRow(txtValTotals, footerRowIndex);
            Grid.SetColumn(txtValTotals, 0);
            grid.Children.Add(txtValTotals);

            for (int i = 0; i < totalRows.Length; i++)
            {
                if (i >= MaxColumns)
                {
                    break;
                }
                TextBlock txtFreqTotals = new TextBlock();
                txtFreqTotals.Text = StringLiterals.SPACE + totalRows[i].ToString() + StringLiterals.SPACE;
                txtFreqTotals.Margin = new Thickness(2, 0, 2, 0);
                txtFreqTotals.VerticalAlignment = VerticalAlignment.Center;
                txtFreqTotals.HorizontalAlignment = HorizontalAlignment.Right;
                txtFreqTotals.FontWeight = FontWeights.Bold;
                Grid.SetRow(txtFreqTotals, footerRowIndex);
                Grid.SetColumn(txtFreqTotals, i + 1);
                grid.Children.Add(txtFreqTotals);
            }

            int sumTotal = 0;
            foreach (int n in totalRows)
            {
                sumTotal = sumTotal + n;
            }

            TextBlock txtOverallTotal = new TextBlock();
            txtOverallTotal.Text = StringLiterals.SPACE + sumTotal.ToString() + StringLiterals.SPACE;
            txtOverallTotal.Margin = new Thickness(2, 0, 2, 0);
            txtOverallTotal.VerticalAlignment = VerticalAlignment.Center;
            txtOverallTotal.HorizontalAlignment = HorizontalAlignment.Right;
            txtOverallTotal.FontWeight = FontWeights.Bold;
            Grid.SetRow(txtOverallTotal, footerRowIndex);
            Grid.SetColumn(txtOverallTotal, MaxColumns + 1);
            grid.Children.Add(txtOverallTotal);
            grid.Visibility = System.Windows.Visibility.Visible;
        }

        private void DrawFrequencyBorders(string groupValue)
        {
            Grid grid = GetStrataGrid(groupValue);

            waitPanel.Visibility = System.Windows.Visibility.Collapsed;
            int rdcount = 0;

            foreach (RowDefinition rd in grid.RowDefinitions)
            {
                int cdcount = 0;
                foreach (ColumnDefinition cd in grid.ColumnDefinitions)
                {  
                    Rectangle rctBorder = new Rectangle();                    
                    Border border = new Border();
                    if (rdcount > 0)
                    {
                        border.Style = this.Resources["gridCellBorder"] as Style;
                    }
                    else
                    {
                        border.Style = this.Resources["gridHeaderCellBorder"] as Style;
                    }

                    if (rdcount == 0)
                    {
                        border.BorderThickness = new Thickness(border.BorderThickness.Left, border.BorderThickness.Bottom, border.BorderThickness.Right, border.BorderThickness.Bottom);
                    }
                    if (cdcount == 0)
                    {
                        border.BorderThickness = new Thickness(border.BorderThickness.Right, border.BorderThickness.Top, border.BorderThickness.Right, border.BorderThickness.Bottom);
                    }

                    border.Child = rctBorder;                    

                    Grid.SetRow(border, rdcount);
                    Grid.SetColumn(border, cdcount);
                    grid.Children.Add(border);
                    if (rdcount > 0)
                    {
                        Grid.SetZIndex(rctBorder, 0);

                        rctBorder.Style = this.Resources["gridCellRectangle"] as Style;// .Fill = Brushes.White;
                        
                        if (IsHostedByEnter)
                        {
                            //rctBorder.MouseEnter += new MouseEventHandler(rowDef_MouseEnter);
                            //rctBorder.MouseLeave += new MouseEventHandler(rowDef_MouseLeave);
                            //rctBorder.MouseUp += new MouseButtonEventHandler(rowDef_MouseUp);
                            rctBorder.IsHitTestVisible = false;
                        }
                    }
                    cdcount++;
                }
                rdcount++;
            }

            SetGadgetToFinishedState();
        }

        private void SetCustomColumnSort(DataTable table)
        {
            #region Input Validation
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            #endregion // Input Validation

            if (columnOrder == null || columnOrder.Count == 0)
            {
                return;
            }

            if (columnOrder.Contains("Line"))
            {
                columnOrder.Remove("Line");
            }

            for (int i = 0; i < columnOrder.Count; i++)
            {
                string s = columnOrder[i];
                if (table.Columns.Contains(s))
                {
                    table.Columns[s].SetOrdinal(i);
                }
            }
        }

        /// <summary>
        /// Sets the gadget's state to 'finished' mode
        /// </summary>
        private void RenderFinish()
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            foreach (Grid grid in StrataGridList)
            {
                grid.Visibility = Visibility.Visible;
            }

            messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
            messagePanel.Text = string.Empty;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        private void RenderFinishWithWarning(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            foreach (Grid grid in StrataGridList)
            {
                grid.Visibility = Visibility.Visible;
            }

            messagePanel.MessagePanelType = Controls.MessagePanelType.WarningPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        /// <summary>
        /// Sets the gadget's state to 'finished with error' mode
        /// </summary>
        /// <param name="errorMessage">The error message to display</param>
        private void RenderFinishWithError(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = Controls.MessagePanelType.ErrorPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        /// <summary>
        /// Used to generate the list of variables and options for the GadgetParameters object
        /// </summary> 
        private void CreateInputVariableList()
        {
            Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

            GadgetOptions.MainVariableName = string.Empty;
            GadgetOptions.WeightVariableName = string.Empty;
            GadgetOptions.StrataVariableNames = new List<string>();
            GadgetOptions.CrosstabVariableName = string.Empty;

            List<string> listFields = new List<string>();

            if (lbxFields.SelectedItems.Count > 0)
            {
                foreach (string item in lbxFields.SelectedItems)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        listFields.Add(item);
                    }
                }
            }

            listFields.Sort();
            if (IsHostedByEnter)
            {
                //if (dashboardHelper.IsUsingEpiProject && checkboxAllowUpdates.IsChecked == true)
                //{
                    if (listFields.Contains("UniqueKey"))
                        listFields.Remove("UniqueKey");
                    listFields.Add("UniqueKey");
                //}
            }

            foreach (string field in listFields)
            {
                inputVariableList.Add(field, "listfield");
            }

            if (!inputVariableList.ContainsKey("sortcolumnsbytaborder"))
            {
                if (checkboxTabOrder.IsChecked == true)
                {
                    inputVariableList.Add("sortcolumnsbytaborder", "true");
                }
                else
                {
                    inputVariableList.Add("sortcolumnsbytaborder", "false");
                }
            }

            if (!inputVariableList.ContainsKey("usepromptsforcolumnnames"))
            {
                if (checkboxUsePrompts.IsChecked == true)
                {
                    inputVariableList.Add("usepromptsforcolumnnames", "true");
                }
                else
                {
                    inputVariableList.Add("usepromptsforcolumnnames", "false");
                }
            }

            if (!inputVariableList.ContainsKey("showcolumnheadings"))
            {
                if (checkboxColumnHeaders.IsChecked == true)
                {
                    inputVariableList.Add("showcolumnheadings", "true");
                }
                else
                {
                    inputVariableList.Add("showcolumnheadings", "false");
                }
            }

            if (!inputVariableList.ContainsKey("showlinecolumn"))
            {
                if (checkboxLineColumn.IsChecked == true)
                {
                    inputVariableList.Add("showlinecolumn", "true");
                }
                else
                {
                    inputVariableList.Add("showlinecolumn", "false");
                }
            }

            if (!inputVariableList.ContainsKey("shownulllabels"))
            {
                if (checkboxShowNulls.IsChecked == true)
                {
                    inputVariableList.Add("shownulllabels", "true");
                }
                else
                {
                    inputVariableList.Add("shownulllabels", "false");
                }
            }

            if (checkboxListLabels.IsChecked == true)
            {
                GadgetOptions.ShouldShowCommentLegalLabels = true;
            }
            else
            {
                GadgetOptions.ShouldShowCommentLegalLabels = false;
            }

            if (lbxSortFields.Items.Count > 0)
            {
                foreach (string item in lbxSortFields.Items)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        string baseStr = item;

                        if (baseStr.EndsWith("(ascending)"))
                        {
                            baseStr = "[" + baseStr.Remove(baseStr.Length - 12) + "] ASC";
                        }
                        if (baseStr.EndsWith("(descending)"))
                        {
                            baseStr = "[" + baseStr.Remove(baseStr.Length - 13) + "] DESC";
                        }
                        inputVariableList.Add(baseStr, "sortfield");
                    }
                }
            }

            if (cbxGroupField.SelectedIndex >= 0)
            {
                if (!string.IsNullOrEmpty(cbxGroupField.SelectedItem.ToString()))
                {
                    GadgetOptions.StrataVariableNames.Add(cbxGroupField.SelectedItem.ToString());
                }
            }

            if (StrataGridList.Count >= 1) 
            {
                Grid grid = StrataGridList[0];
                SortedDictionary<int, string> sortColumnDictionary = new SortedDictionary<int, string>();

                foreach (UIElement element in grid.Children)
                {
                    if (Grid.GetRow(element) == 0 && element is TextBlock)
                    {
                        TextBlock txtColumnName = element as TextBlock;
                        //columnOrder.Add(txtColumnName.Text);
                        sortColumnDictionary.Add(Grid.GetColumn(element), txtColumnName.Text);
                    }
                }

                columnOrder = new List<string>();
                foreach (KeyValuePair<int, string> kvp in sortColumnDictionary)
                {
                    columnOrder.Add(kvp.Value);
                }

                if (columnOrder.Count == listFields.Count || columnOrder.Count == (listFields.Count + 1))
                {
                    bool same = true;
                    foreach (string s in listFields)
                    {
                        if (!columnOrder.Contains(s))
                        {
                            same = false;
                        }
                    }

                    if (same)
                    {
                        WordBuilder wb = new WordBuilder("^");
                        foreach (string s in columnOrder)
                        {
                            wb.Add(s);
                        }

                        inputVariableList.Add("customusercolumnsort", wb.ToString());
                    }
                    else
                    {
                        columnOrder = new List<string>();
                    }
                }
                else
                {
                    columnOrder = new List<string>();
                }
            }

            inputVariableList.Add("maxcolumns", MaxColumns.ToString());
            inputVariableList.Add("maxrows", MaxRows.ToString());

            GadgetOptions.ShouldIncludeFullSummaryStatistics = false;
            GadgetOptions.InputVariableList = inputVariableList;
        }

        #endregion

        #region Private Properties
        private View View
        {
            get
            {
                return this.DashboardHelper.View;
            }
        }

        private IDbDriver Database
        {
            get
            {
                return this.DashboardHelper.Database;
            }
        }
        #endregion // Private Properties

        #region IGadget Members

        public override void CollapseOutput()
        {
            borderAll.MinWidth = currentWidth;
            foreach (ScrollViewer sv in this.groupSvList)
            {
                sv.Visibility = System.Windows.Visibility.Collapsed;
            }

            foreach (TextBlock tb in this.gridLabelsList)
            {
                tb.Visibility = System.Windows.Visibility.Collapsed;
            }

            foreach (Expander expander in this.StrataExpanderList)
            {
                expander.Visibility = System.Windows.Visibility.Collapsed;
            }

            panelMain.Visibility = System.Windows.Visibility.Collapsed;

            this.messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            this.infoPanel.Visibility = System.Windows.Visibility.Collapsed;            
            descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
            IsCollapsed = true;
        }

        public override void ExpandOutput()
        {
            foreach (ScrollViewer sv in this.groupSvList)
            {
                sv.Visibility = System.Windows.Visibility.Visible;
            }

            foreach (TextBlock tb in this.gridLabelsList)
            {
                tb.Visibility = System.Windows.Visibility.Visible;
            }

            foreach (Expander expander in this.StrataExpanderList)
            {
                expander.Visibility = System.Windows.Visibility.Visible;
            }

            if (this.messagePanel.MessagePanelType != Controls.MessagePanelType.StatusPanel)
            {
                this.messagePanel.Visibility = System.Windows.Visibility.Visible;
            }

            if (!string.IsNullOrEmpty(this.infoPanel.Text))
            {
                this.infoPanel.Visibility = System.Windows.Visibility.Visible;
            }

            if (!string.IsNullOrEmpty(this.descriptionPanel.Text))
            {
                descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.DisplayMode;
            }

            panelMain.Visibility = System.Windows.Visibility.Visible;

            IsCollapsed = false;
        }

        protected override void ShowCustomFilterDialog()
        {
            base.ShowCustomFilterDialog();
        }

        protected override void SetCustomFilterDisplay()
        {
            if (this.DataFilters != null && this.DataFilters.Count > 0)
            {
                if (this.groupSvList.Count > 0 && groupSvList[0].ActualWidth > 30)
                {
                    infoPanel.MaxWidth = Math.Truncate(groupSvList[0].ActualWidth - 10);
                }
                else
                {
                    infoPanel.MaxWidth = this.ActualWidth;
                }

                infoPanel.Text = "Only showing records matching the following criteria (in addition to canvas filters): " + this.DataFilters.GenerateReadableDataFilterString();
                infoPanel.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                infoPanel.Text = string.Empty;
                infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Initiates a refresh of the gadget's output
        /// </summary>
        public override void RefreshResults()
        {
            if (!LoadingCombos && GadgetOptions != null && lbxFields.SelectedItems.Count > 0)
            {
                CreateInputVariableList();
                if (IsHostedByEnter)
                {
                    HideConfigPanel(); 
                }
                waitPanel.Visibility = System.Windows.Visibility.Visible;

                messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
                descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;

                baseWorker = new BackgroundWorker();
                baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
                baseWorker.RunWorkerAsync();

                base.RefreshResults();
            }
            else
            {
                return;
            }

            base.RefreshResults();
        }

        /// <summary>
        /// Updates the variable names available in the gadget's properties
        /// </summary>
        public override void UpdateVariableNames()
        {
            FillComboboxes(true);
        }

        /// <summary>
        /// Generates Xml representation of this gadget
        /// </summary>
        /// <param name="doc">The Xml docment</param>
        /// <returns>XmlNode</returns>
        public override XmlNode Serialize(XmlDocument doc)
        {
            Dictionary<string, string> inputVariableList = GadgetOptions.InputVariableList;

            string groupVar = string.Empty;            

            if (cbxGroupField.SelectedItem != null)
            {
                groupVar = cbxGroupField.SelectedItem.ToString().Replace("<", "&lt;");
            }

            CustomOutputHeading = headerPanel.Text;            
            //CustomOutputDescription = txtOutputDescription.Text.Replace("<", "&lt;");
            CustomOutputDescription = descriptionPanel.Text; //txtOutputDescription.Text.Replace("<", "&lt;");

            string xmlString =
            "<groupVariable>" + groupVar + "</groupVariable>" +
            "<maxRows>" + MaxRows.ToString() + "</maxRows>" +
            "<maxColumnNameLength>" + MaxColumnLength.ToString() + "</maxColumnNameLength>" +
            "<sortColumnsByTabOrder>" + checkboxTabOrder.IsChecked.ToString() + "</sortColumnsByTabOrder>" +
            "<useFieldPrompts>" + checkboxUsePrompts.IsChecked.ToString() + "</useFieldPrompts>" +
            "<showListLabels>" + checkboxListLabels.IsChecked + "</showListLabels>" +
            "<showLineColumn>" + checkboxLineColumn.IsChecked.ToString() + "</showLineColumn>" +
            "<showColumnHeadings>" + checkboxColumnHeaders.IsChecked.ToString() + "</showColumnHeadings>" +
            "<showNullLabels>" + checkboxShowNulls.IsChecked.ToString() + "</showNullLabels>" + 
            //"<alternatingRowColors>" + checkboxAltRowColors.IsChecked.ToString() + "</alternatingRowColors>" +
            //"<allowUpdates>" + checkboxAllowUpdates.IsChecked.ToString() + "</allowUpdates>" +
            "<customHeading>" + CustomOutputHeading.Replace("<", "&lt;") + "</customHeading>" +
            "<customDescription>" + CustomOutputDescription.Replace("<", "&lt;") + "</customDescription>" +
            "<customCaption>" + CustomOutputCaption + "</customCaption>";

            xmlString = xmlString + SerializeAnchors();

            if (inputVariableList.ContainsKey("customusercolumnsort"))
            {
                string columns = inputVariableList["customusercolumnsort"];
                xmlString = xmlString + "<customusercolumnsort>" + columns + "</customusercolumnsort>";                
            }
            else if (columnOrder != null && columnOrder.Count > 0) // when user has re-ordered columns but not refreshed
            {
                WordBuilder wb = new WordBuilder("^");
                for (int i = 0; i < columnOrder.Count; i++)
                {
                    wb.Add(columnOrder[i]);
                }
                xmlString = xmlString + "<customusercolumnsort>" + wb.ToString() + "</customusercolumnsort>";
            }

            System.Xml.XmlElement element = doc.CreateElement("lineListGadget");
            element.InnerXml = xmlString;
            element.AppendChild(SerializeFilters(doc));

            System.Xml.XmlAttribute id = doc.CreateAttribute("id");
            System.Xml.XmlAttribute locationY = doc.CreateAttribute("top");
            System.Xml.XmlAttribute locationX = doc.CreateAttribute("left");
            System.Xml.XmlAttribute collapsed = doc.CreateAttribute("collapsed");
            System.Xml.XmlAttribute type = doc.CreateAttribute("gadgetType");

            id.Value = this.UniqueIdentifier.ToString();
            locationY.Value = Canvas.GetTop(this).ToString("F0");
            locationX.Value = Canvas.GetLeft(this).ToString("F0");
            collapsed.Value = IsCollapsed.ToString();     
            type.Value = "EpiDashboard.LineListControl";

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);
            element.Attributes.Append(id);

            if (lbxFields.Items.Count > 0 && lbxFields.SelectedItems.Count > 0)
            {
                string xmlListItemString = string.Empty;
                XmlElement listItemElement = doc.CreateElement("listFields");

                foreach (string s in lbxFields.SelectedItems)
                {
                    xmlListItemString = xmlListItemString + "<listField>" + s.Replace("<", "&lt;") + "</listField>";
                }

                listItemElement.InnerXml = xmlListItemString;
                element.AppendChild(listItemElement);
            }

            if (lbxSortFields.Items.Count > 0)
            {
                string xmlSortString = string.Empty;
                XmlElement sortElement = doc.CreateElement("sortFields");

                foreach (string s in lbxSortFields.Items)
                {
                    xmlSortString = xmlSortString + "<sortField>" + s.Replace("<", "&lt;") + "</sortField>";
                }

                sortElement.InnerXml = xmlSortString;
                element.AppendChild(sortElement);
            }

            return element;
        }

        /// <summary>
        /// Creates the line list gadget from an Xml element
        /// </summary>
        /// <param name="element">The element from which to create the gadget</param>
        public override void CreateFromXml(XmlElement element)
        {
            this.LoadingCombos = true;
            this.ColumnWarningShown = true;

            HideConfigPanel();
            
            infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLower())
                {
                    case "groupvariable":
                        cbxGroupField.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "maxcolumnnamelength":
                        int maxColumnLength = 24;
                        int.TryParse(child.InnerText, out maxColumnLength);
                        txtMaxColumnLength.Text = maxColumnLength.ToString();
                        break;
                    case "maxrows":
                        int maxRows = 50;
                        int.TryParse(child.InnerText, out maxRows);
                        txtMaxRows.Text = maxRows.ToString();
                        break;
                    case "sortcolumnsbytaborder":                        
                        bool sortByTabs = false;
                        bool.TryParse(child.InnerText, out sortByTabs);
                        checkboxTabOrder.IsChecked = sortByTabs;
                        break;
                    case "usefieldprompts":
                        bool usePrompts = false;
                        bool.TryParse(child.InnerText, out usePrompts);
                        checkboxUsePrompts.IsChecked = usePrompts;
                        break;
                    case "allowupdates":
                        bool allowUpdates = false;
                        bool.TryParse(child.InnerText, out allowUpdates);
                        checkboxAllowUpdates.IsChecked = allowUpdates;
                        break;
                    case "showlinecolumn":
                        bool showLineColumn = true;
                        bool.TryParse(child.InnerText, out showLineColumn);
                        checkboxLineColumn.IsChecked = showLineColumn;
                        break;
                    case "showcolumnheadings":
                        bool showColumnHeadings = true;
                        bool.TryParse(child.InnerText, out showColumnHeadings);
                        checkboxColumnHeaders.IsChecked = showColumnHeadings;
                        break;
                    case "showlistlabels":
                        bool showLabels = false;
                        bool.TryParse(child.InnerText, out showLabels);
                        checkboxListLabels.IsChecked = showLabels;
                        break;
                    case "shownulllabels":
                        bool showNullLabels = true;
                        bool.TryParse(child.InnerText, out showNullLabels);
                        checkboxShowNulls.IsChecked = showNullLabels;
                        break;
                    case "customheading":
                        if (!string.IsNullOrEmpty(child.InnerText))
                        {
                            this.CustomOutputHeading = child.InnerText.Replace("&lt;", "<"); ;
                        }
                        break;
                    case "customdescription":
                        if (!string.IsNullOrEmpty(child.InnerText))
                        {
                            this.CustomOutputDescription = child.InnerText.Replace("&lt;", "<");

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
                        break;
                    case "customcaption":
                        this.CustomOutputCaption = child.InnerText;
                        break;
                    case "listfields":
                        foreach (XmlElement field in child.ChildNodes)
                        {
                            List<string> fields = new List<string>();
                            if (field.Name.ToLower().Equals("listfield"))
                            {
                             //   fields.Add(field.InnerText);
                                lbxFields.SelectedItems.Add(field.InnerText.Replace("&lt;", "<"));
                            }
                        }
                        break;
                    case "sortfields":
                        foreach (XmlElement field in child.ChildNodes)
                        {
                            List<string> fields = new List<string>();
                            if (field.Name.ToLower().Equals("sortfield"))
                            {
                                lbxSortFields.Items.Add(field.InnerText.Replace("&lt;", "<"));
                            }
                        }
                        break;
                    case "customusercolumnsort":                        
                        string[] cols = child.InnerText.Split('^');
                        columnOrder = cols.ToList();
                        break;
                    case "datafilters":
                        this.DataFilters = new DataFilters(this.DashboardHelper);
                        this.DataFilters.CreateFromXml(child);
                        break;                
                }
            }

            base.CreateFromXml(element);

            this.LoadingCombos = false;            

            RefreshResults();
            HideConfigPanel();
        }

        #endregion

        private void txtMaxColumns_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Util.IsWholeNumber(e.Text);
            base.OnPreviewTextInput(e);
        }

        private int MaxRows
        {
            get
            {
                int maxRows = 50;
                bool success = int.TryParse(txtMaxRows.Text, out maxRows);
                if (!success)
                {
                    return 50;
                }
                else
                {
                    if (maxRows >= MAX_ROW_LIMIT)
                    {
                        return MAX_ROW_LIMIT;
                    }
                    else
                    {
                        return maxRows;
                    }
                }
            }
        }

        private int MaxColumns
        {
            get
            {
                return this.maxColumns;                
            }
            set
            {
                if (value <= 1)
                {
                    this.maxColumns = 1;
                }
                else
                {
                    this.maxColumns = value;
                }
            }
        }

        private int MaxColumnLength
        {
            get
            {
                int maxColumnLength = 24;
                bool success = int.TryParse(txtMaxColumnLength.Text, out maxColumnLength);
                if (!success)
                {
                    return 24;
                }
                else
                {
                    return maxColumnLength;
                }
            }
        }

        private bool ColumnWarningShown
        {
            get
            {
                return this.columnWarningShown;
            }
            set
            {
                this.columnWarningShown = value;
            }
        }

        /// <summary>
        /// Sets the gadget to its 'processing' state
        /// </summary>
        public override void SetGadgetToProcessingState()
        {
            this.IsProcessing = true;
            this.cbxGroupField.IsEnabled = false;
            this.lbxFields.IsEnabled = false;
            this.lbxSortFields.IsEnabled = false;
            this.txtMaxRows.IsEnabled = false;
            this.txtMaxColumnLength.IsEnabled = false;
            this.btnRun.IsEnabled = false;
            this.cbxSortField.IsEnabled = false;
            this.checkboxTabOrder.IsEnabled = false;
            this.checkboxUsePrompts.IsEnabled = false;
            this.checkboxAllowUpdates.IsEnabled = false;
            this.checkboxListLabels.IsEnabled = false;
            this.checkboxLineColumn.IsEnabled = false;
            this.checkboxColumnHeaders.IsEnabled = false;
            this.checkboxShowNulls.IsEnabled = false;

            base.SetGadgetToProcessingState();   
        }

        /// <summary>
        /// Sets the gadget to its 'finished' state
        /// </summary>
        public override void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
            this.cbxGroupField.IsEnabled = true;
            this.lbxFields.IsEnabled = true;
            this.lbxSortFields.IsEnabled = true;
            this.txtMaxRows.IsEnabled = true;
            this.txtMaxColumnLength.IsEnabled = true;
            this.btnRun.IsEnabled = true;
            this.cbxSortField.IsEnabled = true;
            this.checkboxTabOrder.IsEnabled = true;
            this.checkboxUsePrompts.IsEnabled = true;
            this.checkboxAllowUpdates.IsEnabled = true;
            this.checkboxListLabels.IsEnabled = true;
            this.checkboxLineColumn.IsEnabled = true;
            this.checkboxColumnHeaders.IsEnabled = true;
            this.checkboxShowNulls.IsEnabled = true;

            currentWidth = borderAll.ActualWidth;

            base.SetGadgetToFinishedState();
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0)
        {
            if (IsCollapsed) return string.Empty;

            StringBuilder htmlBuilder = new StringBuilder();
            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            if (CustomOutputHeading == null || (string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)")))
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">Line List</h2>");
            }
            else if (CustomOutputHeading != "(none)")
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">" + CustomOutputHeading + "</h2>");
            }

            if (!string.IsNullOrEmpty(CustomOutputDescription) && CustomOutputDescription != "(none)")
            {
                htmlBuilder.AppendLine("<p class=\"gadgetsummary\">" + CustomOutputDescription + "</p>");
            }

            if (!string.IsNullOrEmpty(messagePanel.Text) && messagePanel.Visibility == Visibility.Visible)
            {
                htmlBuilder.AppendLine("<p><small><strong>" + messagePanel.Text + "</strong></small></p>");
            }                   

            foreach (Grid grid in this.StrataGridList)
            {
                string gridName = grid.Tag.ToString();

                htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
                if (string.IsNullOrEmpty(CustomOutputCaption) && this.StrataGridList.Count > 1)
                {
                    htmlBuilder.AppendLine("<caption>" + grid.Tag + "</caption>");
                }
                else if(!string.IsNullOrEmpty(CustomOutputCaption))
                {
                    htmlBuilder.AppendLine("<caption>" + CustomOutputCaption + "</caption>");
                }
                
                SortedList<int, TextBlock> textBlocks = new SortedList<int, TextBlock>();
                int currentColumnCount = 0;
                int currentColumnNumber = 0;
                foreach (UIElement control in grid.Children)
                {
                    if (control is TextBlock)
                    {                        
                        int columnNumber = Grid.GetColumn(control);
                        textBlocks.Add(columnNumber + currentColumnCount, control as TextBlock);
                        currentColumnNumber++;
                    }
                    if (currentColumnNumber >= grid.ColumnDefinitions.Count)
                    {
                        currentColumnCount = currentColumnCount + grid.ColumnDefinitions.Count;
                        currentColumnNumber = 0;
                    }
                }

                foreach (KeyValuePair<int, TextBlock> kvp in textBlocks)
                {
                    TextBlock control = kvp.Value;
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
                    string formattedValue = value.Replace("<", "&lt;").Replace(">", "&gt;");
                    formattedValue = formattedValue.Replace("\n", "<br/>");

                    htmlBuilder.AppendLine(tableDataTagOpen + formattedValue + tableDataTagClose);

                    if (columnNumber >= grid.ColumnDefinitions.Count - 1)
                    {
                        htmlBuilder.AppendLine("</tr>");
                    }
                }

                htmlBuilder.AppendLine("</table>");
            }

            return htmlBuilder.ToString();
        }

        private string customOutputHeading;
        private string customOutputDescription;
        private string customOutputCaption;

        public override string CustomOutputHeading
        {
            get
            {
                return this.customOutputHeading;
            }
            set
            {
                this.customOutputHeading = value;
                headerPanel.Text = CustomOutputHeading;
            }
        }

        public override string CustomOutputDescription
        {
            get
            {
                return this.customOutputDescription;
            }
            set
            {
                this.customOutputDescription = value;
                //txtOutputDescription.Text = CustomOutputDescription;
                descriptionPanel.Text = CustomOutputDescription;
            }
        }

        public override string CustomOutputCaption
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

        public bool IsHostedByEnter
        {
            get
            {
                return isHostedByEnter;
            }
            set
            {
                isHostedByEnter = value;
                if (value)
                {
                    //imgClose.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    //imgClose.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            RefreshResults();
        }

        private void cbxSortField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!LoadingCombos && cbxSortField.SelectedIndex > 0 && !lbxSortFields.Items.Contains(cbxSortField.SelectedItem.ToString()))
            {
                AddColumnToSortList(cbxSortField.SelectedItem.ToString());                
            }
        }

        private void AddColumnToSortList(string columnName)
        {
            if (!lbxSortFields.Items.Contains(columnName) && !lbxSortFields.Items.Contains(columnName + " (ascending)"))
            {
                lbxSortFields.Items.Add(columnName + " (ascending)");
            }
        }

        private void InsertColumnInSortList(string columnName, int insertAt)
        {
            if (!lbxSortFields.Items.Contains(columnName) && !lbxSortFields.Items.Contains(columnName + " (ascending)"))
            {
                lbxSortFields.Items.Insert(insertAt, columnName + " (ascending)");
            }
        }

        private void SwapColumnOrderInSortList(string columnName)
        {
            if (lbxSortFields.Items.Contains(columnName + " (ascending)"))
            {
                string sortColumn = columnName + " (descending)";
                lbxSortFields.Items.Remove(columnName + " (ascending)");
                lbxSortFields.Items.Add(sortColumn);
            }
            else if (lbxSortFields.Items.Contains(columnName + " (descending)"))
            {
                string sortColumn = columnName + " (ascending)";
                lbxSortFields.Items.Remove(columnName + " (descending)");
                lbxSortFields.Items.Add(sortColumn);
            }
        }

        void mnuRemoveSorts_Click(object sender, RoutedEventArgs e)
        {
            lbxSortFields.Items.Clear();
            RefreshResults();
        }

        void mnuSwapSortType_Click(object sender, RoutedEventArgs e)
        {
            if (lbxSortFields.SelectedItems.Count == 1)
            {
                string selectedItem = lbxSortFields.SelectedItem.ToString();
                int index = lbxSortFields.SelectedIndex;

                if (selectedItem.ToLower().EndsWith("(ascending)"))
                {
                    selectedItem = selectedItem.Remove(selectedItem.Length - 11) + "(descending)";
                }
                else
                {
                    selectedItem = selectedItem.Remove(selectedItem.Length - 12) + "(ascending)";
                }

                lbxSortFields.Items.Remove(lbxSortFields.SelectedItem.ToString());
                lbxSortFields.Items.Insert(index, selectedItem);
            }
        }

        void mnuRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lbxSortFields.SelectedItems.Count == 1)
            {
                lbxSortFields.Items.Remove(lbxSortFields.SelectedItem.ToString());
            }
        }

        private void txtMaxRows_TextChanged(object sender, TextChangedEventArgs e)
        {
            int rows = 0;

            int.TryParse(txtMaxRows.Text, out rows);

            if (rows > MAX_ROW_LIMIT)
            {
                rows = MAX_ROW_LIMIT;
                txtMaxRows.Text = rows.ToString();
            }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            //#region Fade in
            //// Create a storyboard to contain the animations.
            //Storyboard storyboard = new Storyboard();
            //TimeSpan duration = new TimeSpan(0, 0, 0, 0, 250);

            //// Create a DoubleAnimation to fade the not selected option control
            //DoubleAnimation animation = new DoubleAnimation();

            //animation.From = 0.0;
            //animation.To = 1.0;
            //animation.Duration = new Duration(duration);
            //// Configure the animation to target de property Opacity
            //Storyboard.SetTargetName(animation, ConfigGrid.Name);
            //Storyboard.SetTargetProperty(animation, new PropertyPath(Control.OpacityProperty));
            //// Add the animation to the storyboard
            //storyboard.Children.Add(animation);

            //// Begin the storyboard
            //storyboard.Begin(this);
            //#endregion
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            //#region Fade out
            //// Create a storyboard to contain the animations.
            //Storyboard storyboard = new Storyboard();
            //TimeSpan duration = new TimeSpan(0, 0, 0, 0, 250);

            //// Create a DoubleAnimation to fade the not selected option control
            //DoubleAnimation animation = new DoubleAnimation();

            //animation.From = 1.0;
            //animation.To = 0.0;
            //animation.Duration = new Duration(duration);
            //// Configure the animation to target de property Opacity
            //Storyboard.SetTargetName(animation, ConfigGrid.Name);
            //Storyboard.SetTargetProperty(animation, new PropertyPath(Control.OpacityProperty));
            //// Add the animation to the storyboard
            //storyboard.Children.Add(animation);

            //// Begin the storyboard
            //storyboard.Begin(this);
            //#endregion
        }
    }
}
