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
using Epi.WPF.Dashboard.Rules;

namespace Epi.WPF.Dashboard.Gadgets.Analysis
{
    /// <summary>
    /// Interaction logic for CombinedFrequencyControl.xaml
    /// </summary>
    public partial class CombinedFrequencyControl : GadgetBase
    {
        #region Private Variables
        
        private List<TextBlock> gridLabelsList;
        private bool loadingCombos;

        #endregion

        #region Delegates

        private new delegate void AddGridFooterDelegate(string strataValue, int denominator, int fields = -1, bool isBoolean = false);
        private new delegate void SetGridTextDelegate(string strataValue, TextBlockConfig textBlockConfig);
        private delegate void AddFreqGridDelegate(string strataVar, string value);
        private delegate void RenderFrequencyHeaderDelegate(string strataValue, string freqVar);
        private delegate void RenderConfidenceLimitsHeaderDelegate(string strataValue, string freqVar);
        private delegate void DrawConfidenceLimitBordersDelegate(string strataValue);
        
        private delegate void SetConfidenceLimitsGridTextDelegate(string strataValue, TextBlockConfig textBlockConfig);
        private delegate void AddConfidenceLimitsGridRowDelegate(string strataValue, int height);
        
        #endregion

        #region Constructors

        public CombinedFrequencyControl()
        {
            InitializeComponent();
            Construct();
        }

        public CombinedFrequencyControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.dashboardHelper = dashboardHelper;
            Construct();
        }

        protected override void Construct()
        {
            if (!string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)"))
            {
                headerPanel.Text = CustomOutputHeading;
            }

            strataGridList = new List<Grid>();
            gridLabelsList = new List<TextBlock>();

            FillComboboxes();

            mnuCopyData.Click += new RoutedEventHandler(mnuCopyData_Click);
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

            mnuSendToBack.Click += new RoutedEventHandler(mnuSendToBack_Click);
            mnuClose.Click += new RoutedEventHandler(mnuClose_Click);

            this.IsProcessing = false;

            base.Construct();

            #region Translation
            ConfigExpandedTitle.Text = DashboardSharedStrings.GADGET_CONFIG_TITLE_COMBINED_FREQUENCY;
            tblockGroupField.Text = DashboardSharedStrings.GADGET_GROUP_VARIABLE;
            expanderAdvancedOptions.Header = DashboardSharedStrings.GADGET_ADVANCED_OPTIONS;
            checkboxSortHighLow.Content = DashboardSharedStrings.GADGET_SORT_HI_LOW;
            checkboxShowDenominator.Content = DashboardSharedStrings.GADGET_SHOW_DENOMINATOR;
            #endregion // Translation
        }

        /// <summary>
        /// Handles the click event for the 'Copy data to clipboard' context menu option
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuCopyData_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard();
        }

        /// <summary>
        /// Copies a grid's output to the clipboard
        /// </summary>
        protected override void CopyToClipboard()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Grid grid in strataGridList)
            {
                if (strataGridList.Count > 1)
                {
                    sb.AppendLine(grid.Tag.ToString());
                }

                foreach (UIElement control in grid.Children)
                {
                    if (control is TextBlock)
                    {
                        int columnNumber = Grid.GetColumn(control);
                        string value = ((TextBlock)control).Text;

                        sb.Append(value + "\t");

                        if (columnNumber >= grid.ColumnDefinitions.Count - 1)
                        {
                            sb.AppendLine();
                        }
                    }
                }

                sb.AppendLine();
            }

            Clipboard.Clear();
            Clipboard.SetText(sb.ToString());
        }

        //private void CollapseExpandConfigPanel()
        //{
        //    if (triangleCollapsed)
        //    {
        //        ConfigExpandedTriangle.Visibility = Visibility.Visible;
        //        ConfigCollapsedTriangle.Visibility = Visibility.Collapsed;
        //        ConfigCollapsedTitle.Visibility = Visibility.Collapsed;
        //        ConfigGrid.Height = Double.NaN;
        //        ConfigGrid.UpdateLayout();
        //    }
        //    else
        //    {
        //        ConfigCollapsedTriangle.Visibility = Visibility.Visible;
        //        ConfigExpandedTriangle.Visibility = Visibility.Collapsed;
        //        ConfigCollapsedTitle.Visibility = Visibility.Visible;
        //        ConfigGrid.Height = 50;
        //    }
        //    triangleCollapsed = !triangleCollapsed;
        //}

        #endregion

        #region Public Methods
        /// <summary>
        /// Clears all of the gadget's output and sets it to its beginning state
        /// </summary>
        public void ClearResults()
        {
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Text = string.Empty;
            descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;

            foreach (Grid grid in strataGridList)
            {
                grid.Children.Clear();
                grid.RowDefinitions.Clear();
                if (grid.Parent is Border)
                {
                    Border border = grid.Parent as Border;
                    panelMain.Children.Remove(border);
                }
            }

            foreach (TextBlock textBlock in gridLabelsList)
            {
                panelMain.Children.Remove(textBlock);
            }

            strataGridList.Clear();

            panelMain.Children.Clear();
        }

        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the click event for the Run button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            if (LoadingCombos)
            {
                return;
            }
            RefreshResults();
        }

        /// <summary>
        /// Handles the selection changed event for the combine mode combo box
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void cmbCombineMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (txtTrueValue != null && tblockTrueValue != null)
            {
                if (cmbCombineMode.SelectedIndex == 1)
                {
                    txtTrueValue.Visibility = System.Windows.Visibility.Visible;
                    tblockTrueValue.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    txtTrueValue.Visibility = System.Windows.Visibility.Collapsed;
                    tblockTrueValue.Visibility = System.Windows.Visibility.Collapsed;
                    txtTrueValue.Text = string.Empty;
                }
            }
        }

        protected override void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Debug.Print("Background worker thread for combined frequency gadget was cancelled or ran to completion.");
        }

        protected override void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            Dictionary<string, string> inputVariableList = GadgetOptions.InputVariableList;

            lock (syncLock)
            {
                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));

                AddFreqGridDelegate addGrid = new AddFreqGridDelegate(AddFreqGrid);
                SetGridTextDelegate setText = new SetGridTextDelegate(SetGridText);
                AddGridRowDelegate addRow = new AddGridRowDelegate(AddGridRow);
                RenderFrequencyHeaderDelegate renderHeader = new RenderFrequencyHeaderDelegate(RenderFrequencyHeader);
                DrawFrequencyBordersDelegate drawBorders = new DrawFrequencyBordersDelegate(DrawOutputGridBorders);

                System.Collections.Generic.Dictionary<string, System.Data.DataTable> Freq_ListSet = new Dictionary<string, System.Data.DataTable>();

                string freqVar = GadgetOptions.MainVariableName;
                string weightVar = GadgetOptions.WeightVariableName;
                string strataVar = string.Empty;

                if (GadgetOptions.StrataVariableNames != null && GadgetOptions.StrataVariableNames.Count > 0)
                {
                    strataVar = GadgetOptions.StrataVariableNames[0];
                }

                try
                {
                    List<string> stratas = new List<string>();
                    if (!string.IsNullOrEmpty(strataVar))
                    {
                        stratas.Add(strataVar);
                    }

                    DataTable dt = new DataTable();
                    bool booleanResults = false;
                    int fields = -1;

                    if (dashboardHelper.GetAllGroupsAsList().Contains(freqVar))
                    {
                        dt = dashboardHelper.GenerateCombinedFrequencyTable(GadgetOptions, ref booleanResults);
                        fields = dashboardHelper.GetVariablesInGroup(freqVar).Count;
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), "Something that should never fail has failed.");
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        return;
                    }

                    if (dt == null || dt.Rows.Count == 0)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.NO_RECORDS_SELECTED);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        return;
                    }
                    else if (worker.CancellationPending)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.DASHBOARD_GADGET_STATUS_OPERATION_CANCELLED);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        Debug.Print("Combined frequency thread cancelled");
                        return;
                    }
                    else
                    {
                        string formatString = string.Empty;
                        double count = Convert.ToDouble(dt.Compute("sum([count])", string.Empty));  //0;
                        this.Dispatcher.BeginInvoke(addGrid, strataVar, string.Empty);
                        string strataValue = dt.TableName;
                        this.Dispatcher.BeginInvoke(renderHeader, strataValue, freqVar);
                        //double AccumulatedTotal = 0;
                        int rowCount = 1;
                        int denominator = dashboardHelper.RecordCount;

                        if (!booleanResults)
                        {
                            denominator = denominator * fields;
                        }

                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            if (!row["value"].Equals(DBNull.Value))
                            {
                                this.Dispatcher.Invoke(addRow, strataValue, 26);
                                string displayValue = row["value"].ToString();

                                this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(displayValue, new Thickness(6, 0, 6, 0), VerticalAlignment.Center, HorizontalAlignment.Left, TextAlignment.Left, rowCount, 0, Visibility.Visible));
                                this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(row["count"].ToString(), new Thickness(6, 0, 6, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 1, Visibility.Visible));

                                double pct = 0;
                                if (count > 0)
                                {
                                    pct = Convert.ToDouble(row["count"]) / (double)denominator;
                                }
                                //AccumulatedTotal += pct;

                                this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(pct.ToString("P"), new Thickness(6, 0, 6, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 2, Visibility.Visible));

                                rowCount++;
                            }
                        }

                        this.Dispatcher.BeginInvoke(new AddGridFooterDelegate(RenderFrequencyFooter), strataValue, denominator, fields, booleanResults);
                        this.Dispatcher.BeginInvoke(drawBorders, strataValue);

                        this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                    }
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                }
                finally
                {
                    //stopwatch.Stop();
                    //Debug.Print("Combined Frequency gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete with " + dashboardHelper.RecordCount.ToString() + " records and the following filters:");
                    //Debug.Print(dashboardHelper.DataFilters.GenerateDataFilterString());
                }
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Fill the gadget's comboboxes
        /// </summary>
        private void FillComboboxes(bool update = false)
        {
            loadingCombos = true;

            string prevField = string.Empty;

            if (update)
            {
                if (cbxField.SelectedIndex >= 0)
                {
                    prevField = cbxField.SelectedItem.ToString();
                }
            }

            cbxField.ItemsSource = null;
            cbxField.Items.Clear();
            //cbxFieldWeight.Items.Clear();

            cmbFieldStrata.ItemsSource = null;
            cmbFieldStrata.Items.Clear();

            List<string> fieldNames = new List<string>();
            List<string> weightFieldNames = new List<string>();
            List<string> strataFieldNames = new List<string>();

            weightFieldNames.Add(string.Empty);
            strataFieldNames.Add(string.Empty);

            if (dashboardHelper.IsUsingEpiProject)
            {
                Field field = null;
                foreach (DataRow fieldRow in dashboardHelper.FieldTable.Rows)
                {
                    if (fieldRow["epifieldtype"] is Field)
                    {
                        field = fieldRow["epifieldtype"] as Field;
                        if (field is GroupField)
                        {
                            fieldNames.Add(field.Name);
                        }
                    }
                }

                //foreach (IDashboardRule rule in dashboardHelper.Rules)
                //{
                //    if (rule is DataAssignmentRule)
                //    {
                //        DataAssignmentRule assignmentRule = rule as DataAssignmentRule;
                //        fieldNames.Add(assignmentRule.DestinationColumnName);
                //        strataFieldNames.Add(assignmentRule.DestinationColumnName);
                //        if (assignmentRule.VariableType.Equals(DashboardVariableType.Numeric))
                //        {
                //            weightFieldNames.Add(assignmentRule.DestinationColumnName);
                //        }
                //    }
                //}
            }
            else
            {
                //fieldNames = dashboardHelper.GetFieldsAsList();
                //fieldNames.AddRange(dashboardHelper.GetGroupFieldsAsList());

                //weightFieldNames = dashboardHelper.GetFieldsAsList();
                //strataFieldNames = dashboardHelper.GetFieldsAsList();
                //strataFieldNames.AddRange(dashboardHelper.GetFieldsAsList());
                //strataFieldNames.Sort();
            }

            fieldNames.AddRange(dashboardHelper.GetGroupVariablesAsList());

            fieldNames.Sort();
            weightFieldNames.Sort();
            strataFieldNames.Sort();

            cbxField.ItemsSource = fieldNames;
            //cbxFieldWeight.ItemsSource = weightFieldNames;
            cmbFieldStrata.ItemsSource = strataFieldNames;

            if (cbxField.Items.Count > 0)
            {
                cbxField.SelectedIndex = -1;
            }
            //if (cbxFieldWeight.Items.Count > 0)
            //{
            //    cbxFieldWeight.SelectedIndex = -1;
            //}
            if (cmbFieldStrata.Items.Count > 0)
            {
                cmbFieldStrata.SelectedIndex = -1;
            }

            if (update)
            {
                cbxField.SelectedItem = prevField;
            }

            loadingCombos = false;
        }

        private void AddFreqGrid(string strataVar, string value)
        {
            Grid grid = new Grid();
            grid.Tag = value;
            grid.Style = this.Resources["genericOutputGrid"] as Style;
            grid.Visibility = System.Windows.Visibility.Collapsed;

            ColumnDefinition column1 = new ColumnDefinition();
            ColumnDefinition column2 = new ColumnDefinition();
            ColumnDefinition column3 = new ColumnDefinition();

            column1.Width = GridLength.Auto;
            column2.Width = GridLength.Auto;
            column3.Width = GridLength.Auto;

            grid.ColumnDefinitions.Add(column1);
            grid.ColumnDefinitions.Add(column2);
            grid.ColumnDefinitions.Add(column3);

            //TextBlock txtGridLabel = new TextBlock();
            //txtGridLabel.Text = value;
            //txtGridLabel.HorizontalAlignment = HorizontalAlignment.Left;
            //txtGridLabel.VerticalAlignment = VerticalAlignment.Bottom;
            //txtGridLabel.Margin = new Thickness(2, 42, 2, 2);
            //txtGridLabel.FontWeight = FontWeights.Bold;
            //if (string.IsNullOrEmpty(strataVar))
            //{
            //    txtGridLabel.Margin = new Thickness(2, 36, 2, 2);
            //    txtGridLabel.Visibility = System.Windows.Visibility.Hidden;
            //}
            //else
            //{
            //    txtGridLabel.Margin = new Thickness(2, 42, 2, 2);
            //}
            //gridLabelsList.Add(txtGridLabel);
            //panelMain.Children.Add(txtGridLabel);

            Border border = new Border();
            border.Style = this.Resources["genericOutputGridBorderWithMargins"] as Style;

            border.Child = grid;
            panelMain.Children.Add(border);
            strataGridList.Add(grid);
        }

        private void SetGridText(string strataValue, TextBlockConfig textBlockConfig)
        {
            Grid grid = new Grid();

            grid = GetStrataGrid(strataValue);

            TextBlock txt = new TextBlock();
            txt.Text = textBlockConfig.Text;
            txt.Margin = textBlockConfig.Margin;
            txt.VerticalAlignment = textBlockConfig.VerticalAlignment;
            txt.HorizontalAlignment = textBlockConfig.HorizontalAlignment;
            Grid.SetRow(txt, textBlockConfig.RowNumber);
            Grid.SetColumn(txt, textBlockConfig.ColumnNumber);
            grid.Children.Add(txt);
        }

        protected override void AddGridRow(string strataValue, int height)
        {
            Grid grid = GetStrataGrid(strataValue);
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;
            //waitCursor.Visibility = Visibility.Collapsed;            
            grid.Visibility = Visibility.Visible;
            RowDefinition rowDef = new RowDefinition();
            rowDef.Height = new GridLength(height);
            grid.RowDefinitions.Add(rowDef);
        }

        /// <summary>
        /// Returns the gadget's description as a string.
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return "Combined Frequency Gadget";
        }

        public override void CollapseOutput()
        {
            foreach (Grid grid in this.strataGridList)
            {
                grid.Visibility = System.Windows.Visibility.Collapsed;
                Border border = new Border();
                if (grid.Parent is Border)
                {
                    border = (grid.Parent) as Border;
                    border.Visibility = System.Windows.Visibility.Collapsed;
                }
            }

            this.messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            this.infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            //this.panelDescription.Visibility = System.Windows.Visibility.Collapsed;
            descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
        }

        public override void ExpandOutput()
        {
            foreach (Grid grid in this.strataGridList)
            {
                grid.Visibility = System.Windows.Visibility.Visible;
                Border border = new Border();
                if (grid.Parent is Border)
                {
                    border = (grid.Parent) as Border;
                    border.Visibility = System.Windows.Visibility.Visible;
                }
            }

            if (this.messagePanel.MessagePanelType != Controls.MessagePanelType.StatusPanel)
            {
                this.messagePanel.Visibility = System.Windows.Visibility.Visible;
            }

            if (!string.IsNullOrEmpty(this.infoPanel.Text))
            {
                this.infoPanel.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void RenderFrequencyHeader(string strataValue, string freqVar)
        {
            Grid grid = GetStrataGrid(strataValue);

            RowDefinition rowDefHeader = new RowDefinition();
            rowDefHeader.Height = new GridLength(30);
            grid.RowDefinitions.Add(rowDefHeader);

            for (int y = 0; y < grid.ColumnDefinitions.Count; y++)
            {
                Rectangle rctHeader = new Rectangle();
                rctHeader.Style = this.Resources["gridHeaderCellRectangle"] as Style;
                Grid.SetRow(rctHeader, 0);
                Grid.SetColumn(rctHeader, y);
                grid.Children.Add(rctHeader);
            }

            TextBlock txtValHeader = new TextBlock();
            txtValHeader.Text = freqVar;
            txtValHeader.VerticalAlignment = VerticalAlignment.Center;
            txtValHeader.HorizontalAlignment = HorizontalAlignment.Center;
            txtValHeader.Margin = new Thickness(6, 0, 6, 0);
            txtValHeader.FontWeight = FontWeights.Bold;
            txtValHeader.Foreground = Brushes.White;
            Grid.SetRow(txtValHeader, 0);
            Grid.SetColumn(txtValHeader, 0);
            grid.Children.Add(txtValHeader);

            TextBlock txtFreqHeader = new TextBlock();
            txtFreqHeader.Text = DashboardSharedStrings.COL_HEADER_FREQUENCY;
            txtFreqHeader.VerticalAlignment = VerticalAlignment.Center;
            txtFreqHeader.HorizontalAlignment = HorizontalAlignment.Center;
            txtFreqHeader.Margin = new Thickness(6, 0, 6, 0);
            txtFreqHeader.FontWeight = FontWeights.Bold;
            txtFreqHeader.Foreground = Brushes.White;
            Grid.SetRow(txtFreqHeader, 0);
            Grid.SetColumn(txtFreqHeader, 1);
            grid.Children.Add(txtFreqHeader);

            TextBlock txtPctHeader = new TextBlock();
            txtPctHeader.Text = DashboardSharedStrings.COL_HEADER_PERCENT;
            txtPctHeader.VerticalAlignment = VerticalAlignment.Center;
            txtPctHeader.HorizontalAlignment = HorizontalAlignment.Center;
            txtPctHeader.Margin = new Thickness(6, 0, 6, 0);
            txtPctHeader.FontWeight = FontWeights.Bold;
            txtPctHeader.Foreground = Brushes.White;
            Grid.SetRow(txtPctHeader, 0);
            Grid.SetColumn(txtPctHeader, 2);
            grid.Children.Add(txtPctHeader);
        }

        private void RenderFrequencyFooter(string strataValue, int denominator, int fields = -1, bool isBoolean = false)
        {
            //Grid grid = GetStrataGrid(strataValue);
            if (checkboxShowDenominator.IsChecked == false)
            {
                return;
            }

            TextBlock txtDenom = new TextBlock();
            txtDenom.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            txtDenom.FontWeight = FontWeights.Bold;
            txtDenom.Margin = (Thickness)this.Resources["genericTextMargin"];

            if (isBoolean == true)
            {
                txtDenom.Text = string.Format(DashboardSharedStrings.DENOMINATOR_DESCRIPTION_BOOLEAN, denominator);
            }
            else
            {
                txtDenom.Text = string.Format(DashboardSharedStrings.DENOMINATOR_DESCRIPTION_NON_BOOLEAN, denominator);
            }

            panelMain.Children.Add(txtDenom);

            //RowDefinition rowDefTotals = new RowDefinition();
            //rowDefTotals.Height = new GridLength(26);
            //grid.RowDefinitions.Add(rowDefTotals); 

            //TextBlock txtValTotals = new TextBlock();
            //txtValTotals.Text = SharedStrings.TOTAL;
            //txtValTotals.Margin = new Thickness(4, 0, 4, 0);
            //txtValTotals.VerticalAlignment = VerticalAlignment.Center;
            //txtValTotals.FontWeight = FontWeights.Bold;
            //Grid.SetRow(txtValTotals, footerRowIndex);
            //Grid.SetColumn(txtValTotals, 0);
            //grid.Children.Add(txtValTotals); 

            //TextBlock txtFreqTotals = new TextBlock();
            //txtFreqTotals.Text = totalRows.ToString();
            //txtFreqTotals.Margin = new Thickness(4, 0, 4, 0);
            //txtFreqTotals.VerticalAlignment = VerticalAlignment.Center;
            //txtFreqTotals.HorizontalAlignment = HorizontalAlignment.Right;
            //txtFreqTotals.FontWeight = FontWeights.Bold;
            //Grid.SetRow(txtFreqTotals, footerRowIndex);
            //Grid.SetColumn(txtFreqTotals, 1);
            //grid.Children.Add(txtFreqTotals); 

            //TextBlock txtPctTotals = new TextBlock();
            //txtPctTotals.Text = (1).ToString("P");
            //txtPctTotals.Margin = new Thickness(4, 0, 4, 0);
            //txtPctTotals.VerticalAlignment = VerticalAlignment.Center;
            //txtPctTotals.HorizontalAlignment = HorizontalAlignment.Right;
            //txtPctTotals.FontWeight = FontWeights.Bold;
            //Grid.SetRow(txtPctTotals, footerRowIndex);
            //Grid.SetColumn(txtPctTotals, 2);
            //grid.Children.Add(txtPctTotals); 
        }

        private void RenderFinish()
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            foreach (Grid freqGrid in strataGridList)
            {
                freqGrid.Visibility = Visibility.Visible;
            }

            messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
            messagePanel.Text = string.Empty;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            CheckAndSetPosition();
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
            }
            if (baseWorker != null)
            {
                baseWorker.DoWork -= new System.ComponentModel.DoWorkEventHandler(Execute);
            }

            this.GadgetStatusUpdate -= new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation -= new GadgetCheckForCancellationHandler(IsCancelled);

            base.CloseGadget();

            GadgetOptions = null;
        }

        /// <summary>
        /// Sends the gadget to the back of the canvas
        /// </summary>
        //private void SendToBack()
        //{
        //    Canvas.SetZIndex(this, -1);
        //}

        private void RenderFinishWithWarning(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed; //waitCursor.Visibility = Visibility.Hidden;

            foreach (Grid freqGrid in strataGridList)
            {
                freqGrid.Visibility = Visibility.Visible;
            }

            messagePanel.MessagePanelType = Controls.MessagePanelType.WarningPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        private void RenderFinishWithError(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = Controls.MessagePanelType.ErrorPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        private void CreateInputVariableList()
        {
            Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

            if (cbxField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxField.SelectedItem.ToString()))
            {
                GadgetOptions.MainVariableName = cbxField.SelectedItem.ToString();
            }
            else
            {
                return;
            }

            //if (cbxFieldStrata.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldStrata.SelectedItem.ToString()))
            //{
            //    inputVariableList.Add("stratavar", cbxFieldStrata.SelectedItem.ToString());
            //}

            if (checkboxSortHighLow.IsChecked == true)
            {
                inputVariableList.Add("sort", "highlow");
                GadgetOptions.ShouldSortHighToLow = true;
            }
            else
            {
                GadgetOptions.ShouldSortHighToLow = false;
            }

            if (checkboxShowDenominator.IsChecked == true)
            {
                inputVariableList.Add("denom", "true");
            }
            else
            {
                inputVariableList.Add("denom", "false");
            }

            GadgetOptions.ShouldIncludeMissing = false;

            if (DataFilters != null && DataFilters.Count > 0)
            {
                GadgetOptions.CustomFilter = DataFilters.GenerateDataFilterString(false);
            }
            else
            {
                GadgetOptions.CustomFilter = string.Empty;
            }

            if (cmbCombineMode.SelectedIndex >= 0)
            {
                inputVariableList.Add("combinemode", cmbCombineMode.SelectedIndex.ToString());
            }

            inputVariableList.Add("truevalue", txtTrueValue.Text);

            GadgetOptions.InputVariableList = inputVariableList;
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
        /// Sets the gadget to its 'processing' state
        /// </summary>
        public override void SetGadgetToProcessingState()
        {
            this.cbxField.IsEnabled = false;
            this.IsProcessing = true;
        }

        /// <summary>
        /// Sets the gadget to its 'finished' state
        /// </summary>
        public override void SetGadgetToFinishedState()
        {
            this.cbxField.IsEnabled = true;
            this.IsProcessing = false;

            base.SetGadgetToFinishedState();
        }

        /// <summary>
        /// Updates the variable names available in the gadget's properties
        /// </summary>
        public override void UpdateVariableNames()
        {
            FillComboboxes(true);
        }

        /// <summary>
        /// Forces a refresh
        /// </summary>
        public override void RefreshResults()
        {
            Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

            if (!loadingCombos && cbxField.SelectedIndex >= 0)
            {
                CreateInputVariableList();
                infoPanel.Visibility = System.Windows.Visibility.Collapsed;
                waitPanel.Visibility = System.Windows.Visibility.Visible;
                messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
                descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;

                baseWorker = new BackgroundWorker();
                baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
                baseWorker.RunWorkerAsync();

                base.RefreshResults();
            }
        }

        /// <summary>
        /// Generates Xml representation of this gadget
        /// </summary>
        /// <param name="doc">The Xml docment</param>
        /// <returns>XmlNode</returns>
        public override XmlNode Serialize(XmlDocument doc)
        {
            CreateInputVariableList();

            Dictionary<string, string> inputVariableList = GadgetOptions.InputVariableList;

            string freqVar = GadgetOptions.MainVariableName;
            string strataVar = string.Empty;
            string crosstabVar = GadgetOptions.CrosstabVariableName;
            string sort = string.Empty;
            bool showDenominator = true;
            if (checkboxShowDenominator.IsChecked == false)
            {
                showDenominator = false;
            }

            if (inputVariableList.ContainsKey("sort"))
            {
                sort = inputVariableList["sort"];
            }

            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            string combineMode = "automatic";

            switch (cmbCombineMode.SelectedIndex)
            {
                case 1:
                    combineMode = "boolean";
                    break;
                case 2:
                    combineMode = "categorical";
                    break;
                case 0:
                default:
                    combineMode = "automatic";
                    break;
            }

            string xmlString =
            "<mainVariable>" + freqVar + "</mainVariable>" +
            "<sort>" + sort + "</sort>" +
            "<combineMode>" + combineMode + "</combineMode>" +
            "<trueValue>" + txtTrueValue.Text + "</trueValue>" +
            "<showDenominator>" + showDenominator + "</showDenominator>" +
            "<customHeading>" + CustomOutputHeading.Replace("<", "&lt;") + "</customHeading>" +
            "<customDescription>" + CustomOutputDescription.Replace("<", "&lt;") + "</customDescription>";

            System.Xml.XmlElement element = doc.CreateElement("combinedFrequencyGadget");
            element.InnerXml = xmlString;
            element.AppendChild(SerializeFilters(doc));

            System.Xml.XmlAttribute locationY = doc.CreateAttribute("top");
            System.Xml.XmlAttribute locationX = doc.CreateAttribute("left");
            System.Xml.XmlAttribute collapsed = doc.CreateAttribute("collapsed");
            System.Xml.XmlAttribute type = doc.CreateAttribute("gadgetType");

            locationY.Value = Canvas.GetTop(this).ToString("F0");
            locationX.Value = Canvas.GetLeft(this).ToString("F0");
            collapsed.Value = "false"; // currently no way to collapse the gadget, so leave this 'false' for now
            type.Value = "EpiDashboard.CombinedFrequencyControl";

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);

            return element;
        }

        /// <summary>
        /// Creates the frequency gadget from an Xml element
        /// </summary>
        /// <param name="element">The element from which to create the gadget</param>
        public override void CreateFromXml(XmlElement element)
        {
            this.loadingCombos = true;

            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLower())
                {
                    case "mainvariable":
                        cbxField.Text = child.InnerText;
                        break;
                    case "stratavariable":
                        cmbFieldStrata.Text = child.InnerText;
                        break;
                    case "truevalue":
                        txtTrueValue.Text = child.InnerText;
                        break;
                    case "combinemode":
                        switch (child.InnerText.ToLower())
                        {
                            case "boolean":
                            case "bool":
                                cmbCombineMode.SelectedIndex = 1;
                                break;
                            case "categorical":
                                cmbCombineMode.SelectedIndex = 2;
                                break;
                            case "automatic":
                            case "auto":
                            default:
                                cmbCombineMode.SelectedIndex = 0;
                                break;
                        }
                        break;
                    case "sort":
                        if (child.InnerText.ToLower().Equals("hightolow") || child.InnerText.ToLower().Equals("highlow"))
                        {
                            checkboxSortHighLow.IsChecked = true;
                        }
                        else
                        {
                            checkboxSortHighLow.IsChecked = false;
                        }
                        break;
                    case "customheading":
                        if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
                        {
                            this.CustomOutputHeading = child.InnerText.Replace("&lt;", "<"); ;
                        }
                        break;
                    case "customdescription":
                        if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
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
                    case "showdenominator":
                        bool showDenom = false;
                        bool.TryParse(child.InnerText, out showDenom);
                        checkboxShowDenominator.IsChecked = showDenom;
                        break;
                    case "datafilters":
                        this.dataFilters = new DataFilters(this.dashboardHelper);
                        this.DataFilters.CreateFromXml(child);
                        break;
                }
            }

            SetPositionFromXml(element);

            this.loadingCombos = false;

            RefreshResults();
            HideConfigPanel();
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0)
        {
            StringBuilder htmlBuilder = new StringBuilder();

            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            if (CustomOutputHeading == null || (string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)")))
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">Combined Frequency</h2>");
            }
            else if (CustomOutputHeading != "(none)")
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">" + CustomOutputHeading + "</h2>");
            }

            htmlBuilder.AppendLine("<p class=\"gadgetOptions\"><small>");
            htmlBuilder.AppendLine("<em>Group variable:</em> <strong>" + cbxField.Text + "</strong>");
            htmlBuilder.AppendLine("<br />");

            if (cmbFieldStrata.SelectedIndex >= 0)
            {
                htmlBuilder.AppendLine("<em>Strata variable:</em> <strong>" + cmbFieldStrata.Text + "</strong>");
                htmlBuilder.AppendLine("<br />");
            }

            htmlBuilder.AppendLine("<em>Combine mode:</em> <strong>" + cmbCombineMode.Text + "</strong>");
            htmlBuilder.AppendLine("<br />");

            //htmlBuilder.AppendLine("<em>Include missing:</em> <strong>" + checkboxIncludeMissing.IsChecked.ToString() + "</strong>");
            //htmlBuilder.AppendLine("<br />");
            htmlBuilder.AppendLine("</small></p>");

            if (!string.IsNullOrEmpty(CustomOutputDescription))
            {
                htmlBuilder.AppendLine("<p class=\"gadgetsummary\">" + CustomOutputDescription + "</p>");
            }

            if (!string.IsNullOrEmpty(messagePanel.Text) && messagePanel.Visibility == System.Windows.Visibility.Visible)
            {
                htmlBuilder.AppendLine("<p><small><strong>" + messagePanel.Text + "</strong></small></p>");
            }

            if (!string.IsNullOrEmpty(infoPanel.Text) && infoPanel.Visibility == Visibility.Visible)
            {
                htmlBuilder.AppendLine("<p><small><strong>" + infoPanel.Text + "</strong></small></p>");
            }

            foreach (Grid grid in this.strataGridList)
            {
                string gridName = grid.Tag.ToString();

                htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
                htmlBuilder.AppendLine("<caption>" + gridName + "</caption>");

                foreach (UIElement control in grid.Children)
                {
                    if (control is TextBlock)
                    {
                        int rowNumber = Grid.GetRow(control);
                        int columnNumber = Grid.GetColumn(control);

                        string tableDataTagOpen = "<td>";
                        string tableDataTagClose = "</td>";

                        if (rowNumber == 0)
                        {
                            tableDataTagOpen = "<th>";
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

                        htmlBuilder.AppendLine(tableDataTagOpen + formattedValue + tableDataTagClose);

                        if (columnNumber >= grid.ColumnDefinitions.Count - 1)
                        {
                            htmlBuilder.AppendLine("</tr>");
                        }
                    }
                }

                htmlBuilder.AppendLine("</table>");
            }

            // TODO: Update if this gadget ever implements stratas
            foreach (UIElement element in panelMain.Children)
            {
                if (element is TextBlock)
                {
                    TextBlock tblock = element as TextBlock;
                    htmlBuilder.AppendLine("<p>");
                    htmlBuilder.AppendLine(tblock.Text);
                    htmlBuilder.AppendLine("</p>");
                }
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
        #endregion
    }
}
