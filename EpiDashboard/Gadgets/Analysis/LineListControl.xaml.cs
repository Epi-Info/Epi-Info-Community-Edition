using System;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        private List<string> columnOrder = new List<string>();

        /// <summary>
        /// Bool used to determine if the gadget is being called directly from the Enter module (e.g. 'Interactive Line List' option)
        /// </summary>
        private bool isHostedByEnter;

        /// <summary>
        /// Used for maintaining the width when collapsing output
        /// </summary>
        private double currentWidth;

        private RequestUpdateStatusDelegate requestUpdateStatus;
        private CheckForCancellationDelegate checkForCancellation;


        #endregion // Private Members

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
            Parameters.ColumnNames.Clear();
            Parameters.ColumnNames.Add("Page " + pageNumber);
            if (IsHostedByEnter)
            {
                //if (dashboardHelper.IsUsingEpiProject && checkboxAllowUpdates.IsChecked == true)
                //{
                if (Parameters.ColumnNames.Contains("UniqueKey"))
                    Parameters.ColumnNames.Remove("UniqueKey");
                Parameters.ColumnNames.Add("UniqueKey");
                //}
            }
        }

        private DataGrid GetDataGrid()
        {
            DataGrid dg = null;
            foreach (UIElement element in panelMain.Children)
            {
                if (element is DataGrid)
                {
                    dg = element as DataGrid;
                }
            }

            return dg;
        }

        /// <summary>
        /// Copies a grid's output to the clipboard
        /// </summary>
        protected override void CopyToClipboard()
        {
            DataGrid dg = GetDataGrid();

            if (dg != null)
            {
                if (dg.ItemsSource is DataView)
                {
                    Common.CopyDataViewToClipboard(dg.ItemsSource as DataView);
                }
                else if (dg.ItemsSource is ListCollectionView)
                {
                    ListCollectionView lcv = dg.ItemsSource as ListCollectionView;
                    if (lcv.SourceCollection is DataView)
                    {
                        Common.CopyDataViewToClipboard(lcv.SourceCollection as DataView);
                    }
                }
            }
        }

        public override void ShowHideConfigPanel()
        {
            Popup = new DashboardPopup();
            if (this.Parent is DragCanvas)
            {
                Popup.Parent = ((this.Parent as DragCanvas).Parent as ScrollViewer).Parent as Grid;
            }
            else
            {
                Popup.Parent = this.Parent as Grid;
            }

            Controls.GadgetProperties.LineListProperties properties = new Controls.GadgetProperties.LineListProperties(this.DashboardHelper, this, (LineListParameters)Parameters, StrataGridList, columnOrder);

            properties.Width = 800;
            properties.Height = 600;

            if ((System.Windows.SystemParameters.PrimaryScreenWidth / 1.2) > properties.Width)
            {
                properties.Width = (System.Windows.SystemParameters.PrimaryScreenWidth / 1.2);
            }

            if ((System.Windows.SystemParameters.PrimaryScreenHeight / 1.2) > properties.Height)
            {
                properties.Height = (System.Windows.SystemParameters.PrimaryScreenHeight / 1.2);
            }

            properties.Cancelled += new EventHandler(properties_Cancelled);
            properties.ChangesAccepted += new EventHandler(properties_ChangesAccepted);
            Popup.Content = properties;
            Popup.Show();
        }

        private void properties_ChangesAccepted(object sender, EventArgs e)
        {
            Controls.GadgetProperties.LineListProperties properties = Popup.Content as Controls.GadgetProperties.LineListProperties;
            this.Parameters = properties.Parameters;
            this.DataFilters = properties.DataFilters;
            this.CustomOutputHeading = Parameters.GadgetTitle;
            this.CustomOutputDescription = Parameters.GadgetDescription;
            Popup.Close();
            if (properties.HasSelectedFields)
            {
                RefreshResults();
            }
        }

        private void properties_Cancelled(object sender, EventArgs e)
        {
            Popup.Close();
        }

        /// <summary>
        /// Clears the gadget's output
        /// </summary>
        public void ClearResults()
        {
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Text = string.Empty;
            descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;

            panelMain.Children.Clear();
        }
        #endregion // Public Methods

        private void AddDataGrid(DataView dv, string strataValue)
        {
            DataGrid dg = new DataGrid();
            dg.Style = this.Resources["LineListDataGridStyle"] as Style;

            LineListParameters ListParameters = (this.Parameters) as LineListParameters;

            FrameworkElementFactory datagridRowsPresenter = new FrameworkElementFactory(typeof(DataGridRowsPresenter));
            ItemsPanelTemplate itemsPanelTemplate = new ItemsPanelTemplate();
            itemsPanelTemplate.VisualTree = datagridRowsPresenter;
            GroupStyle groupStyle = new GroupStyle();
            groupStyle.ContainerStyle = this.Resources["DefaultGroupItemStyle"] as Style;
            groupStyle.Panel = itemsPanelTemplate;
            dg.GroupStyle.Add(groupStyle);

            GroupStyle groupStyle2 = new GroupStyle();
            groupStyle2.HeaderTemplate = this.Resources["GroupDataTemplate"] as DataTemplate;
            //groupStyle.Panel = itemsPanelTemplate;
            dg.GroupStyle.Add(groupStyle2);

            string groupVar = String.Empty;
          DataTable  dataTable = dv.ToTable();
          if (dataTable.Rows.Count > ListParameters.MaxRows)
            {
                dataTable = dataTable.AsEnumerable().Skip(0).Take(ListParameters.MaxRows).CopyToDataTable();
            }
          if (ListParameters.ShowLineColumn)
          {
              DataColumn dc = new DataColumn();
              dc.ColumnName = "Line";
              dc.DataType = typeof(System.Int32);            
              dataTable.Columns.Add(dc);
              dc.SetOrdinal(0);
              int rowid = 1;
              foreach (DataRow dr in dataTable.Rows)
              {
                  dr["Line"] = rowid++;
              }
          }            
                      
            DataView dataView = new DataView(dataTable);
            if (!String.IsNullOrEmpty(ListParameters.PrimaryGroupField.Trim()))
            {
                groupVar = ListParameters.PrimaryGroupField.Trim();
                ListCollectionView lcv = new ListCollectionView(dataView);
                lcv.GroupDescriptions.Add(new PropertyGroupDescription(groupVar));
                if (!String.IsNullOrEmpty(ListParameters.SecondaryGroupField.Trim()) && !ListParameters.SecondaryGroupField.Trim().Equals(groupVar))
                {
                    lcv.GroupDescriptions.Add(new PropertyGroupDescription(ListParameters.SecondaryGroupField.Trim())); // for second category
                }
                dg.ItemsSource = lcv;

            }
            else
            {
                dg.ItemsSource = dataView;
            }
            
            if (Parameters.Height.HasValue)
            {
                dg.MaxHeight = Parameters.Height.Value;
            }
            else
            {
                dg.MaxHeight = 700;
            }

            if (Parameters.Width.HasValue)
            {
                dg.MaxWidth = Parameters.Width.Value;
            }
            else
            {
                dg.MaxWidth = 900;
            }

            dg.AutoGeneratedColumns += new EventHandler(dg_AutoGeneratedColumns);
            dg.AutoGeneratingColumn += new EventHandler<DataGridAutoGeneratingColumnEventArgs>(dg_AutoGeneratingColumn);

            panelMain.Children.Add(dg);
        }

        void dg_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.IsReadOnly = true;

            DataGridTextColumn dataGridTextColumn = e.Column as DataGridTextColumn;
            if (dataGridTextColumn != null)
            {
                if (dataGridTextColumn.Header.ToString().Length > ((LineListParameters)Parameters).MaxColumnLength)
                {
                   dataGridTextColumn.Header = dataGridTextColumn.Header.ToString().Substring(0, ((LineListParameters)Parameters).MaxColumnLength) + StringLiterals.ELLIPSIS;
                }
                                
                if (e.PropertyType == typeof(DateTime))
                {
                    dataGridTextColumn.CellStyle = this.Resources["RightAlignDataGridCellStyle"] as Style;
                    Field field = DashboardHelper.GetAssociatedField(e.Column.Header.ToString());
                    if (field != null && field is DateField)
                    {
                        dataGridTextColumn.Binding.StringFormat = "{0:d}";
                    }
                    else if (field != null && field is TimeField)
                    {
                        dataGridTextColumn.Binding.StringFormat = "{0:t}";
                    }
                }
                else if (e.PropertyType == typeof(int) ||
                    e.PropertyType == typeof(byte) ||
                    e.PropertyType == typeof(decimal) ||
                    e.PropertyType == typeof(double) ||
                    e.PropertyType == typeof(float))
                {
                    dataGridTextColumn.CellStyle = this.Resources["RightAlignDataGridCellStyle"] as Style;
                }
                if(((LineListParameters)Parameters).ShowNullLabels)
                    dataGridTextColumn.Binding.TargetNullValue = DashboardHelper.Config.Settings.RepresentationOfMissing;
            }
        }

        void dg_AutoGeneratedColumns(object sender, EventArgs e)
        {
            //DataGrid dg = (sender as DataGrid);
            //DataView dv = dg.ItemsSource as DataView;

            //int columnCount = 0;
            //foreach (DataColumn column in dv.Table.Columns)
            //{
            //    Field field = DashboardHelper.GetAssociatedField(column.ColumnName);
            //    if (field != null && field is RenderableField)
            //    {
            //        if (GadgetOptions.InputVariableList.ContainsKey("usepromptsforcolumnnames") &&
            //            GadgetOptions.InputVariableList["usepromptsforcolumnnames"] == "true")
            //        {
            //            dg.Columns[columnCount].Header = (((RenderableField)field).PromptText);
            //        }
            //    }
            //    columnCount++;
            //}
        }

        #region Event Handlers

        /// <summary>
        /// Handles the WorkerCompleted event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected override void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Debug.Print("Background worker thread for line list gadget was cancelled or ran to completion.");
            this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
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
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));
                AddDataGridDelegate addDataGrid = new AddDataGridDelegate(AddDataGrid);

                try
                {
                    int maxRows =( (LineListParameters)Parameters).MaxRows;
                    bool exceededMaxRows = false;
                    Parameters.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                    Parameters.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);
                    if (this.DataFilters != null)
                    {
                        Parameters.CustomFilter = this.DataFilters.GenerateDataFilterString(false);
                    }
                    else
                    {
                        Parameters.CustomFilter = string.Empty;
                    }

                    List<DataTable> lineListTables = DashboardHelper.GenerateLineList(Parameters as LineListParameters);

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
                        }

                   //    SetGadgetStatusMessage(SharedStrings.DASHBOARD_GADGET_STATUS_DISPLAYING_OUTPUT);

                        foreach (DataTable listTable in lineListTables)
                        {
                            string strataValue = listTable.TableName;
                            if (listTable.Rows.Count == 0)
                            {
                                continue;
                            }
                            string tableHeading = listTable.TableName;
                            if (Parameters.ColumnNames.Count != columnOrder.Count)
                                columnOrder = new List<string>();
                            SetCustomColumnSort(listTable);

                            if (listTable.Columns.Count == 0)
                            {
                                throw new ApplicationException("There are no columns to display in this list. If specifying a group variable, ensure the group variable contains data fields.");
                            }

                            int[] totals = new int[listTable.Columns.Count - 1];
                            if (listTable.Rows.Count > maxRows)
                            {
                                exceededMaxRows = true;
                            } 
                            this.Dispatcher.BeginInvoke(addDataGrid, listTable.AsDataView(), listTable.TableName);                            
                        }                        
                    }
                    if (exceededMaxRows)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithWarningDelegate(RenderFinishWithWarning), string.Format(SharedStrings.DASHBOARD_GADGET_STATUS_ROW_LIMIT, maxRows.ToString()));                       
                    }
                    else

                    this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
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
            this.Parameters = new LineListParameters();

            if (!string.IsNullOrEmpty(CustomOutputHeading))
            {
                headerPanel.Text = CustomOutputHeading;
            }

            requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
            checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

            mnuCopy.Click += new RoutedEventHandler(mnuCopy_Click);
            mnuSendDataToHTML.Click += new RoutedEventHandler(mnuSendDataToHTML_Click);

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
            if (!IsHostedByEnter)
            {
                mnuSendToBack.Click += new RoutedEventHandler(mnuSendToBack_Click);
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
            }

            this.IsProcessing = false;

            this.GadgetStatusUpdate += new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(IsCancelled);

            base.Construct();

            #region Translation
            mnuCenter.Header = DashboardSharedStrings.GADGET_CENTER;
            mnuClose.Header = LineListSharedStrings.CMENU_CLOSE;
            mnuCopy.Header = LineListSharedStrings.CMENU_COPY;
            mnuRemoveSorts.Header = LineListSharedStrings.CMENU_REMOVE_SORTING;
            mnuSendDataToExcel.Header = LineListSharedStrings.CMENU_EXCEL;
            mnuSendDataToHTML.Header = LineListSharedStrings.CMENU_HTML;
            mnuSendToBack.Header = LineListSharedStrings.CMENU_SEND_TO_BACK;

            //tblockListVariablesToDisplay.Text = LineListSharedStrings.LIST_VARIABLES_TO_DISPLAY;
            //tblockSortVariables.Text = LineListSharedStrings.SORT_VARIABLES;
            //tblockSortOrder.Text = LineListSharedStrings.SORT_ORDER;
            //tblockLineListProperties.Text = LineListSharedStrings.LIST_CONFIG_TITLE;
            //tblockGroupResultsBy.Text = LineListSharedStrings.GROUP_RESULTS_BY;
            //tblockMaxVarNameLength.Text = LineListSharedStrings.MAX_VAR_NAME_LENGTH;
            //tblockMaxRows.Text = LineListSharedStrings.MAX_ROWS_TO_DISPLAY;

            //tblockMaxWidth.Text = DashboardSharedStrings.GADGET_MAX_WIDTH;
            //tblockMaxHeight.Text = DashboardSharedStrings.GADGET_MAX_HEIGHT;

            //tblockInst1.Text = LineListSharedStrings.INSTRUCTIONS_1;
            //tblockInst2.Text = LineListSharedStrings.INSTRUCTIONS_2;

            //checkboxTabOrder.Content = LineListSharedStrings.SORT_VARS_TAB_ORDER;
            //checkboxUsePrompts.Content = LineListSharedStrings.USE_FIELD_PROMPTS;
            //checkboxListLabels.Content = DashboardSharedStrings.GADGET_LIST_LABELS;
            //checkboxAllowUpdates.Content = LineListSharedStrings.ALLOW_UPDATES;
            //checkboxLineColumn.Content = LineListSharedStrings.SHOW_LINE_COLUMN;
            //checkboxColumnHeaders.Content = LineListSharedStrings.SHOW_COLUMN_HEADINGS;
            //checkboxShowNulls.Content = LineListSharedStrings.SHOW_MISSING_REPRESENTATION;

            //btnRun.Content = LineListSharedStrings.GENERATE_LINE_LIST;
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

            if (Parameters != null)
            {
                Parameters.GadgetStatusUpdate -= new GadgetStatusUpdateHandler(requestUpdateStatus);
                Parameters.GadgetCheckForCancellation -= new GadgetCheckForCancellationHandler(checkForCancellation);
                Parameters = null;
            }

            this.GadgetStatusUpdate -= new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation -= new GadgetCheckForCancellationHandler(IsCancelled);

            panelMain.Children.Clear();

            requestUpdateStatus = null;
            checkForCancellation = null;

            if (!IsHostedByEnter)
            {
                mnuSendToBack.Click -= new RoutedEventHandler(mnuSendToBack_Click);
                mnuClose.Click -= new RoutedEventHandler(mnuClose_Click);
            }

            base.CloseGadget();

            Parameters = null;
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
        protected override void RenderFinish()
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
            messagePanel.Text = string.Empty;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        protected override void RenderFinishWithWarning(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = Controls.MessagePanelType.WarningPanel;
           // messagePanel.Text = string.Empty;
            //messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        /// <summary>
        /// Sets the gadget's state to 'finished with error' mode
        /// </summary>
        /// <param name="errorMessage">The error message to display</param>
        protected override void RenderFinishWithError(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = Controls.MessagePanelType.ErrorPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            HideConfigPanel();
            CheckAndSetPosition();
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
            panelMain.Visibility = System.Windows.Visibility.Collapsed;

            this.messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            this.infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            //descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed; //EI-24
            IsCollapsed = true;
        }

        public override void ExpandOutput()
        {
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
                infoPanel.MaxWidth = this.ActualWidth;
                infoPanel.Text = String.Format(DashboardSharedStrings.GADGET_CUSTOM_FILTER, this.DataFilters.GenerateReadableDataFilterString());
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
            if (!LoadingCombos && Parameters != null && Parameters.ColumnNames.Count > 0)
            {
                if (IsHostedByEnter)
                {
                    HideConfigPanel();
                }
                waitPanel.Visibility = System.Windows.Visibility.Visible;

                messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
                descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;

                baseWorker = new BackgroundWorker();
                baseWorker.WorkerSupportsCancellation = true;
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
            //FillComboboxes(true);
        }

        /// <summary>
        /// Generates Xml representation of this gadget
        /// </summary>
        /// <param name="doc">The Xml docment</param>
        /// <returns>XmlNode</returns>
        public override XmlNode Serialize(XmlDocument doc)
        {
            LineListParameters listParameters = (LineListParameters)Parameters;

            System.Xml.XmlElement element = doc.CreateElement("lineListGadget");
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

            string groupVar1 = String.Empty;
            string groupVar2 = String.Empty;

            if (!String.IsNullOrEmpty(listParameters.PrimaryGroupField))
            {
                groupVar1 = listParameters.PrimaryGroupField;
            }
            if (!String.IsNullOrEmpty(listParameters.SecondaryGroupField))
            {
                groupVar2 = listParameters.SecondaryGroupField;
            }

            XmlElement groupElement = doc.CreateElement("groupVariable");
            groupElement.InnerText = groupVar1;
            element.AppendChild(groupElement);

            XmlElement groupElement2 = doc.CreateElement("groupVariableSecondary");
            groupElement2.InnerText = groupVar2;
            element.AppendChild(groupElement2);

            XmlElement tabOrderElement = doc.CreateElement("sortColumnsByTabOrder");
            tabOrderElement.InnerText = listParameters.SortColumnsByTabOrder.ToString();
            element.AppendChild(tabOrderElement);

            XmlElement usePromptsElement = doc.CreateElement("usepromptsforcolumnnames");
            usePromptsElement.InnerText = listParameters.UsePromptsForColumnNames.ToString();
            element.AppendChild(usePromptsElement);

            XmlElement showListLabelsElement = doc.CreateElement("showListLabels");
            showListLabelsElement.InnerText = listParameters.ShowCommentLegalLabels.ToString();
            element.AppendChild(showListLabelsElement);

            XmlElement showLineColumnElement = doc.CreateElement("showLineColumn");
            showLineColumnElement.InnerText = listParameters.ShowLineColumn.ToString();
            element.AppendChild(showLineColumnElement);

            XmlElement showColumnHeadersElement = doc.CreateElement("showColumnHeadings");
            showColumnHeadersElement.InnerText = listParameters.ShowColumnHeadings.ToString();
            element.AppendChild(showColumnHeadersElement);

            XmlElement showNullLabelsElement = doc.CreateElement("showNullLabels");
            showNullLabelsElement.InnerText = listParameters.ShowNullLabels.ToString();
            element.AppendChild(showNullLabelsElement);

            XmlElement customHeadingElement = doc.CreateElement("customHeading");
            customHeadingElement.InnerText = listParameters.GadgetTitle; // CustomOutputHeading.Replace("<", "&lt;");
            element.AppendChild(customHeadingElement);

            XmlElement customDescElement = doc.CreateElement("customDescription");
            customDescElement.InnerText = listParameters.GadgetDescription; // CustomOutputDescription.Replace("<", "&lt;");
            element.AppendChild(customDescElement);

            XmlElement customMaxRowElement = doc.CreateElement("maxRows");
            customMaxRowElement.InnerText = listParameters.MaxRows.ToString(); // CustomOutputHeading.Replace("<", "&lt;");
            element.AppendChild(customMaxRowElement);

            XmlElement customMaxColumnElement = doc.CreateElement("maxColumnNameLength");
            customMaxColumnElement.InnerText = listParameters.MaxColumnLength.ToString(); // CustomOutputHeading.Replace("<", "&lt;");
            element.AppendChild(customMaxColumnElement);

            SerializeAnchors(element);

            // when user has re-ordered columns but not refreshed            
            if (!string.IsNullOrEmpty(listParameters.CustomSortColumnName))
            {
                XmlElement customusercolumnsort = doc.CreateElement("customusercolumnsort");
                customusercolumnsort.InnerText = listParameters.CustomSortColumnName;
                element.AppendChild(customusercolumnsort);
            }
            else if (columnOrder != null && columnOrder.Count > 0) // when user has re-ordered columns but not refreshed
            {
                XmlElement customusercolumnsort = doc.CreateElement("customusercolumnsort");
                WordBuilder wb = new WordBuilder("^");
                for (int i = 0; i < columnOrder.Count; i++)
                {
                    wb.Add(columnOrder[i]);
                }
                customusercolumnsort.InnerText = wb.ToString();
                element.AppendChild(customusercolumnsort);
            }

            XmlElement listItemElement = doc.CreateElement("listFields");

            string xmlListItemString = string.Empty;

            foreach (string columnName in listParameters.ColumnNames)
            {
                xmlListItemString = xmlListItemString + "<listField>" + columnName.Replace("<", "&lt;") + "</listField>";
            }

            listItemElement.InnerXml = xmlListItemString;

            if (!String.IsNullOrEmpty(xmlListItemString))
            {
                element.AppendChild(listItemElement);
            }

            XmlElement sortItemElement = doc.CreateElement("sortFields");

            string xmlSortItemString = string.Empty;

            foreach (KeyValuePair<string, SortOrder> kvp in Parameters.SortVariables)
            {
                if (kvp.Value == SortOrder.Descending)
                {
                    xmlSortItemString = xmlSortItemString + "<sortField order=\"DESC\">" + kvp.Key.Replace("<", "&lt;") + "</sortField>";
                }
                else
                {
                    xmlSortItemString = xmlSortItemString + "<sortField order=\"ASC\">" + kvp.Key.Replace("<", "&lt;") + "</sortField>";
                }
            }

            sortItemElement.InnerXml = xmlSortItemString;

            if (!String.IsNullOrEmpty(xmlSortItemString))
            {
                element.AppendChild(sortItemElement);
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
            this.Parameters = new LineListParameters();

            HideConfigPanel();

            infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLower())
                {
                    case "groupvariable":
                    case "groupvariableprimary":
                        if (!String.IsNullOrEmpty(child.InnerText.Trim()))
                        {
                            ((LineListParameters)Parameters).PrimaryGroupField = child.InnerText.Trim();
                        }
                        break;
                    case "groupvariablesecondary":
                        if (!String.IsNullOrEmpty(child.InnerText.Trim()))
                        {
                            ((LineListParameters)Parameters).SecondaryGroupField = child.InnerText.Trim();
                        }
                        break;
                    case "maxcolumnnamelength":
                        int maxColumnLength = 24;
                        int.TryParse(child.InnerText, out maxColumnLength);
                        ((LineListParameters)Parameters).MaxColumnLength = maxColumnLength;
                        break;
                    case "maxrows":
                        int maxRows = 50;
                        int.TryParse(child.InnerText, out maxRows);
                        ((LineListParameters)Parameters).MaxRows = maxRows;
                        break;
                    case "sortcolumnsbytaborder":
                        bool sortByTabs = false;
                        bool.TryParse(child.InnerText, out sortByTabs);
                        ((LineListParameters)Parameters).SortColumnsByTabOrder = sortByTabs;
                        break;
                    case "usefieldprompts":
                        bool usePrompts = false;
                        bool.TryParse(child.InnerText, out usePrompts);
                        ((LineListParameters)Parameters).UsePromptsForColumnNames = usePrompts;
                        break;
                    case "showlinecolumn":
                        bool showLineColumn = true;
                        bool.TryParse(child.InnerText, out showLineColumn);
                        ((LineListParameters)Parameters).ShowLineColumn = showLineColumn;
                        break;
                    case "showcolumnheadings":
                        bool showColumnHeadings = true;
                        bool.TryParse(child.InnerText, out showColumnHeadings);
                        ((LineListParameters)Parameters).ShowColumnHeadings = showColumnHeadings;
                        break;
                    case "showlistlabels":
                        bool showLabels = false;
                        bool.TryParse(child.InnerText, out showLabels);
                        Parameters.ShowCommentLegalLabels = showLabels;
                        break;
                    case "shownulllabels":
                        bool showNullLabels = true;
                        bool.TryParse(child.InnerText, out showNullLabels);
                        ((LineListParameters)Parameters).ShowNullLabels = showNullLabels;
                        break;
                    case "customheading":
                        if (!string.IsNullOrEmpty(child.InnerText))
                        {
                            this.CustomOutputHeading = child.InnerText.Replace("&lt;", "<");
                            Parameters.GadgetTitle = CustomOutputHeading;
                        }
                        break;
                    case "customdescription":
                        if (!string.IsNullOrEmpty(child.InnerText))
                        {
                            this.CustomOutputDescription = child.InnerText.Replace("&lt;", "<");
                            Parameters.GadgetDescription = CustomOutputDescription;
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
                                //GadgetOptions.InputVariableList.Add(field.InnerText.Replace("&lt;", "<"), "listfield");
                                Parameters.ColumnNames.Add(field.InnerText.Replace("&lt;", "<"));
                            }
                        }
                        break;
                    case "sortfields":
                        foreach (XmlElement field in child.ChildNodes)
                        {
                            List<string> fields = new List<string>();

                            SortOrder order = SortOrder.Ascending;

                            if (field.Attributes.Count >= 1 && field.Attributes["order"].Value == "DESC")
                            {
                                order = SortOrder.Descending;
                            }

                            if (field.Name.ToLower().Equals("sortfield"))
                            {
                                if (field.Attributes["order"]==null)
                                {
                                    if (field.InnerText.Contains("(ascending)"))
                                        Parameters.SortVariables.Add(field.InnerText.Replace("&lt;", "<").Substring(0, field.InnerText.Replace("&lt;", "<").IndexOf("(") - 1), SortOrder.Ascending);
                                    else Parameters.SortVariables.Add(field.InnerText.Replace("&lt;", "<").Substring(0, field.InnerText.Replace("&lt;", "<").IndexOf("(") - 1), SortOrder.Descending);
                                }
                                else
                                    Parameters.SortVariables.Add(field.InnerText.Replace("&lt;", "<"), order);
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

        /// <summary>
        /// Sets the gadget to its 'processing' state
        /// </summary>
        public override void SetGadgetToProcessingState()
        {
            this.IsProcessing = true;
            base.SetGadgetToProcessingState();
        }

        /// <summary>
        /// Sets the gadget to its 'finished' state
        /// </summary>
        public override void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
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
            String groupField = (Parameters as EpiDashboard.LineListParameters).PrimaryGroupField;
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

            DataGrid dg = GetDataGrid();


            //if (dg.ItemsSource is ListCollectionView)
            //{
            //    StrataCount = cvGroup.Count;
            //}
            //for (int i = 0; i < StrataCount; i++)
            //{
            Dictionary<String, bool> groupValues = new Dictionary<String, bool>();
            if (dg != null && dg.ItemsSource != null)
            {
                if (!String.IsNullOrEmpty(groupField))
                {
                    if (dg.ItemsSource is DataView)
                    {
                        DataView idv = dg.ItemsSource as DataView;
                        foreach (DataRow dr in idv.Table.Rows)
                        {
                            if (!String.IsNullOrEmpty(dr[groupField].ToString() as String) && !groupValues.Keys.Contains(dr[groupField].ToString() as String))
                            {
                                groupValues.Add(dr[groupField].ToString() as String, true);
                            }
                            else if (String.IsNullOrEmpty(dr[groupField].ToString() as String) && !groupValues.Keys.Contains(""))
                            {
                                groupValues.Add("", true);
                            }
                        }
                    }
                    else if (dg.ItemsSource is ListCollectionView)
                    {
                        ListCollectionView lcv = dg.ItemsSource as ListCollectionView;
                        if (lcv.SourceCollection is DataView)
                        {
                            DataView idv = lcv.SourceCollection as DataView;
                            foreach (DataRow dr in idv.Table.Rows)
                            {
                                if (!String.IsNullOrEmpty(dr[groupField].ToString() as String) && !groupValues.Keys.Contains(dr[groupField].ToString() as String))
                                {
                                    groupValues.Add(dr[groupField].ToString() as String, true);
                                }
                                else if (String.IsNullOrEmpty(dr[groupField].ToString() as String) && !groupValues.Keys.Contains(""))
                                {
                                    groupValues.Add("", true);
                                }
                            }
                        }
                    }
                }
                if (groupValues.Count == 0)
                {
                    groupValues.Add("", true);
                }
                foreach (String groupValue in groupValues.Keys)
                {
                    htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                    if (!String.IsNullOrEmpty(groupField))
                    {
                        htmlBuilder.AppendLine("<h3>" + groupField + " = " + groupValue + "</h3>");
                    }
                    htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
                    if (string.IsNullOrEmpty(CustomOutputCaption) && this.StrataGridList.Count > 1)
                    {
                        //htmlBuilder.AppendLine("<caption>" + grid.Tag + "</caption>");
                    }
                    else if (!string.IsNullOrEmpty(CustomOutputCaption))
                    {
                        htmlBuilder.AppendLine("<caption>" + CustomOutputCaption + "</caption>");
                    }

                    if (dg.ItemsSource is DataView)
                    {
                        if (!String.IsNullOrEmpty(groupField))
                        {
                            DataView sdv = dg.ItemsSource as DataView;
                            if (groupValue.Equals(""))
                                sdv.RowFilter = groupField + " is null";
                            else
                                sdv.RowFilter = groupField + " = '" + groupValue + "'";
                            DataTable sdvTable = sdv.ToTable();
                            if (!Parameters.ColumnNames.Contains(groupField))
                            {
                                sdvTable.Columns.Remove(groupField);
                            }
                            htmlBuilder.AppendLine(Common.ConvertDataViewToHtmlString(sdvTable.DefaultView));
                        }
                        else
                        {
                            htmlBuilder.AppendLine(Common.ConvertDataViewToHtmlString(dg.ItemsSource as DataView));
                        }
                    }
                    else if (dg.ItemsSource is ListCollectionView)
                    {
                        ListCollectionView lcv = dg.ItemsSource as ListCollectionView;
                        if (lcv.SourceCollection is DataView)
                        {
                            if (!String.IsNullOrEmpty(groupField))
                            {
                                DataView sdv = lcv.SourceCollection as DataView;
                                if (groupValue.Equals(""))
                                    sdv.RowFilter = groupField + " is null";
                                else
                                    sdv.RowFilter = groupField + " = '" + groupValue + "'";
                                DataTable sdvTable = sdv.ToTable();
                                if (!Parameters.ColumnNames.Contains(groupField))
                                {
                                    sdvTable.Columns.Remove(groupField);
                                }
                                htmlBuilder.AppendLine(Common.ConvertDataViewToHtmlString(sdvTable.DefaultView));
                            }
                            else
                            {
                                htmlBuilder.AppendLine(Common.ConvertDataViewToHtmlString(lcv.SourceCollection as DataView));
                            }
                        }

                        //htmlBuilder.AppendLine(ConvertGroupToHtmlString(cvGroup[0]));

                        //htmlBuilder.AppendLine(Common.ConvertDataViewToHtmlString(cvGroup[0] as DataView));
                    }

                    htmlBuilder.AppendLine("</table>");
                }
            }
            //}



            return htmlBuilder.ToString();
        }

        //public string ConvertGroupToHtmlString(CollectionViewGroup group)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    DataGrid dg = GetDataGrid();
        //    ListCollectionView lcv = dg.ItemsSource as ListCollectionView;
        //    sb.AppendLine(" <tr>");
        //    foreach (DataColumn dc in group.Items)
        //    {
        //        sb.AppendLine("  <th style=\"width: auto;\">");
        //        sb.AppendLine("  " + dc.ColumnName);
        //        sb.AppendLine("  </th>");
        //    }
        //    sb.AppendLine("</tr>");

        //    foreach (var row in group.Items)
        //    {
        //        sb.AppendLine(" <tr>");
        //        for (int i = 0; i < lcv.ItemProperties.Count ; i++)
        //        {
        //            sb.AppendLine("  <td class=\"value\">");
        //            object value = row.[i];
        //            string strValue = value.ToString().Trim();

        //            if (String.IsNullOrEmpty(strValue))
        //            {
        //                strValue = "&nbsp;";
        //            }

        //            sb.Append(strValue);
        //            sb.AppendLine("  </td>");
        //        }
        //        //foreach (DataColumn dc in dt.Columns)
        //        //{
        //        //    sb.AppendLine("  <td class=\"value\">");
        //        //    object value = row[dc];
        //        //    string strValue = value.ToString().Trim();

        //        //    if (String.IsNullOrEmpty(strValue))
        //        //    {
        //        //        strValue = "&nbsp;";
        //        //    }

        //        //    sb.Append(strValue);
        //        //    sb.AppendLine("  </td>");
        //        //}
        //        sb.AppendLine(" </tr>");
        //    }

        //    sb.Replace("\n", "");

        //    return sb.ToString();
        //}

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
                // REVISIT THIS
                //if (value)
                //{
                //    imgClose.Visibility = System.Windows.Visibility.Collapsed;
                //}
                //else
                //{
                //    imgClose.Visibility = System.Windows.Visibility.Visible;
                //}
            }
        }
    }
}
