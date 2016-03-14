using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using Epi;
using Epi.Data;
using Epi.Fields;
using Epi.DataVisualization.Toolkit.PieExtension;
using EpiDashboard.Rules;
using EpiDashboard.Controls;

namespace EpiDashboard
{
    /// <summary>
    /// Interaction logic for ChartControl.xaml
    /// </summary>
    public partial class ChartControl : GadgetBase
    {
        private readonly char[] SplitTokens = " \t;".ToCharArray();        
        private bool loadingCombos;        
        private Chart chart;        
        private int axisLabelMaxLength;
        private bool isBooleanWithNoStratas = false;        
        private object syncLock = new object();
        private string chartTitle;
        private string legendTitle;
        private string xAxisLabel;
        private string yAxisLabel;
        //private object objXAxisStart;
        //private object objXAxisEnd;

        //private delegate void SetStatusDelegate(string statusMessage);        
        //private delegate void RequestUpdateStatusDelegate(string statusMessage);
        //private delegate bool CheckForCancellationDelegate();
        //private delegate void RenderFinishWithErrorDelegate(string errorMessage);
        //private delegate void RenderFinishWithWarningDelegate(string errorMessage);
        private delegate void RenderFinishEpiCurveDelegate(DataTable data, List<List<StringDataValue>> dataValues);
        private delegate void RenderFinishSingleChartDelegate(List<List<StringDataValue>> stratifiedValues);
        private delegate void RenderFinishScatterChartDelegate(List<NumericDataValue> dataValues, StatisticsRepository.LinearRegression.LinearRegressionResults results, NumericDataValue maxValue, NumericDataValue minValue);
        private delegate void RenderFinishStackedChartDelegate(List<List<StringDataValue>> dataValues, DataTable data);        

        public class StringDataValue
        {
            public double DependentValue { get; set; }
            public string IndependentValue { get; set; }
            public string StratificationValue { get; set; }
            public double CurrentMeanValue { get; set; }
        }

        public class NumericDataValue
        {
            public decimal DependentValue { get; set; }
            public decimal IndependentValue { get; set; }
        }

        public class TypeStringTuple
        {
            public Type Item1 { get; set; }
            public string Item2 { get; set; }
            public TypeStringTuple(Type item1, string item2)
            {
                Item1 = item1;
                Item2 = item2;
            }
        }

        public ChartControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;            
            cbxDateField.SelectionChanged += new SelectionChangedEventHandler(ConfigField_SelectionChanged);
            cbxCaseStatusField.SelectionChanged += new SelectionChangedEventHandler(ConfigField_SelectionChanged);
            cbxChartType.SelectionChanged += new SelectionChangedEventHandler(cbxChartType_SelectionChanged);
            cbxColumnXAxisField.SelectionChanged += new SelectionChangedEventHandler(ConfigField_SelectionChanged);
            cbxColumnYAxisField.SelectionChanged += new SelectionChangedEventHandler(ConfigField_SelectionChanged);
            cbxScatterXAxisField.SelectionChanged += new SelectionChangedEventHandler(ConfigField_SelectionChanged);
            cbxScatterYAxisField.SelectionChanged += new SelectionChangedEventHandler(ConfigField_SelectionChanged);
            cbxColumnAggregateFunc.SelectionChanged += new SelectionChangedEventHandler(ConfigField_SelectionChanged);
            cbxSingleField.SelectionChanged += new SelectionChangedEventHandler(ConfigField_SelectionChanged);
            cbxWeightField.SelectionChanged += new SelectionChangedEventHandler(ConfigField_SelectionChanged);
            cbxStrataField.SelectionChanged += new SelectionChangedEventHandler(ConfigField_SelectionChanged);
            cbxChartSize.SelectionChanged += new SelectionChangedEventHandler(ConfigField_SelectionChanged);
            txtCustomHeight.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            txtCustomWidth.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);

            mnuSave.Click += new RoutedEventHandler(mnuSave_Click);
            mnuCopy.Click += new RoutedEventHandler(mnuCopy_Click);
            mnuLabel.Click += new RoutedEventHandler(mnuLabel_Click);
            mnuPrint.Click += new RoutedEventHandler(mnuPrint_Click);
            mnuSendToBack.Click += new RoutedEventHandler(mnuSendToBack_Click);
            mnuRefresh.Click += new RoutedEventHandler(mnuRefresh_Click);
            mnuClose.Click += new RoutedEventHandler(mnuClose_Click);

            FillComboboxes();

            this.IsProcessing = false;

            this.GadgetStatusUpdate += new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(IsCancelled);

            #region Translation

            //tblockChartType.Text = DashboardSharedStrings.GADGET_CHART_TYPE;
            //tblockChartSize.Text = DashboardSharedStrings.GADGET_CHART_SIZE;
            //tblockDateInterval.Text = DashboardSharedStrings.GADGET_DATE_INTERVAL;
            //tblockXAxisLabelRotation.Text = DashboardSharedStrings.GADGET_X_LABEL_ROTATION;
            //textXAxisStartValue.Text = DashboardSharedStrings.GADGET_X_START_VALUE;
            //textXAxisEndValue.Text = DashboardSharedStrings.GADGET_X_END_VALUE;
            //tblockStackedXField.Text = DashboardSharedStrings.GADGET_X_FIELD;
            //tblockScatterXField.Text = DashboardSharedStrings.GADGET_X_FIELD;

            //tblockStackedOutcomeField.Text = DashboardSharedStrings.GADGET_CHART_OUTCOME_VARIABLE;
            //tblockScatterOutcomeField.Text = DashboardSharedStrings.GADGET_CHART_OUTCOME_VARIABLE;

            //tblockAggregate.Text = DashboardSharedStrings.GADGET_AGG_FUNC;
            //tblockPrimaryField.Text = DashboardSharedStrings.GADGET_CHART_VARIABLE;
            //tblockStrataField.Text = DashboardSharedStrings.GADGET_STRATA_VARIABLE;
            //tblockWeightField.Text = DashboardSharedStrings.GADGET_WEIGHT_VARIABLE;

            //tblockOnsetDateField.Text = DashboardSharedStrings.GADGET_ONSET_DATE_FIELD;
            //tblockCaseStatusField.Text = DashboardSharedStrings.GADGET_CASE_STATUS_FIELD;

            //checkboxAllValues.Content = DashboardSharedStrings.GADGET_ALL_LIST_VALUES;
            //checkboxShowHorizontalGridLines.Content = DashboardSharedStrings.GADGET_CHART_GRID_LINES;

            //tblockProperties.Text = DashboardSharedStrings.GADGET_CONFIG_TITLE_CHART;

            //btnRun.Content = DashboardSharedStrings.GADGET_CHART_GENERATE;
            #endregion // Translation

            base.Construct();
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

            this.GadgetStatusUpdate -= new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation -= new GadgetCheckForCancellationHandler(IsCancelled);

            base.CloseGadget();

            GadgetOptions = null;
            chart = null;            
        }

        void mnuPrint_Click(object sender, RoutedEventArgs e)
        {
            Common.Print(pnlMain);
        }

        /// <summary>
        /// Collapses the output for the gadget
        /// </summary>
        public override void CollapseOutput()
        {
            this.messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            this.pnlMain.Visibility = System.Windows.Visibility.Collapsed;
            //descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed; //EI-24
            IsCollapsed = true;
        }

        /// <summary>
        /// Expands the output for the gadget
        /// </summary>
        public override void ExpandOutput()
        {
            this.pnlMain.Visibility = System.Windows.Visibility.Visible;

            if (this.messagePanel.MessagePanelType != Controls.MessagePanelType.StatusPanel)
            {
                this.messagePanel.Visibility = System.Windows.Visibility.Visible;
            }

            IsCollapsed = false;
        }

        void mnuLabel_Click(object sender, RoutedEventArgs e)
        {
            if (pnlMain.Children.Count > 0 && pnlMain.Children[0] is Chart)
            {
                Chart currentChart = (Chart)pnlMain.Children[0];

                ChartLabelDialog dlg = new ChartLabelDialog();
                if (currentChart.Title != null)
                {
                    dlg.Title = currentChart.Title.ToString();
                }

                if (currentChart.LegendTitle != null)
                {
                    dlg.LegendTitle = currentChart.LegendTitle.ToString();
                }

                if (currentChart.ActualAxes.Count > 0)
                {
                    if (((DisplayAxis)currentChart.ActualAxes[0]).Orientation == AxisOrientation.Y)
                    {
                        if (((DisplayAxis)currentChart.ActualAxes[0]).Title != null)
                        {
                            dlg.YAxisLabel = ((DisplayAxis)currentChart.ActualAxes[0]).Title.ToString();
                        }
                        if (((DisplayAxis)currentChart.ActualAxes[1]).Title != null)
                        {
                            dlg.XAxisLabel = ((DisplayAxis)currentChart.ActualAxes[1]).Title.ToString();
                        }
                    }
                    else
                    {
                        if (((DisplayAxis)currentChart.ActualAxes[0]).Title != null)
                        {
                            dlg.XAxisLabel = ((DisplayAxis)currentChart.ActualAxes[0]).Title.ToString();
                        }
                        if (((DisplayAxis)currentChart.ActualAxes[1]).Title != null)
                        {
                            dlg.YAxisLabel = ((DisplayAxis)currentChart.ActualAxes[1]).Title.ToString();
                        }
                    }
                }

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SetChartLabels(dlg.Title, dlg.LegendTitle, dlg.XAxisLabel, dlg.YAxisLabel);
                    //if (currentChart.ActualAxes.Count > 0)
                    //{
                    //    ((DisplayAxis)currentChart.ActualAxes[0]).Title = dlg.YAxisLabel;
                    //    ((DisplayAxis)currentChart.ActualAxes[1]).Title = dlg.XAxisLabel;
                    //}
                    //currentChart.Title = dlg.Title;
                }
            }
        }

        private void SetChartLabels(string chartTitle, string legendTitle, string xAxisLabel, string yAxisLabel)
        {
            if (pnlMain.Children.Count > 0 && pnlMain.Children[0] is Chart)
            {
                Chart currentChart = (Chart)pnlMain.Children[0];                

                if (currentChart.ActualAxes.Count > 0)
                {
                    if (((DisplayAxis)currentChart.ActualAxes[0]).Orientation == AxisOrientation.Y)
                    {
                        ((DisplayAxis)currentChart.ActualAxes[0]).Title = yAxisLabel;
                        ((DisplayAxis)currentChart.ActualAxes[1]).Title = xAxisLabel;

                        ((DisplayAxis)currentChart.ActualAxes[0]).TitleStyle = this.Resources["axisLabelStyle"] as Style;
                        ((DisplayAxis)currentChart.ActualAxes[1]).TitleStyle = this.Resources["axisLabelStyle"] as Style;
                    }
                    else
                    {
                        ((DisplayAxis)currentChart.ActualAxes[0]).Title = xAxisLabel;
                        ((DisplayAxis)currentChart.ActualAxes[1]).Title = yAxisLabel;

                        ((DisplayAxis)currentChart.ActualAxes[0]).TitleStyle = this.Resources["axisLabelStyle"] as Style;
                        ((DisplayAxis)currentChart.ActualAxes[1]).TitleStyle = this.Resources["axisLabelStyle"] as Style;
                    }
                    YAxisLabel = yAxisLabel;
                    XAxisLabel = xAxisLabel;
                }
                currentChart.Title = chartTitle;
                ChartTitle = chartTitle;
                if (!string.IsNullOrEmpty(legendTitle))
                {
                    currentChart.LegendTitle = legendTitle;
                    LegendTitle = legendTitle;
                }
                else
                {
                    currentChart.LegendTitle = null;
                    LegendTitle = string.Empty;
                }
            }
        }

        void mnuSave_Click(object sender, RoutedEventArgs e)
        {
            Common.SaveAsImage(pnlMain);
        }

        void mnuCopy_Click(object sender, RoutedEventArgs e)
        {
            if (pnlMain.Children.Count > 0)
            {
                Chart currentChart = (Chart)pnlMain.Children[0];
                BitmapSource img = (BitmapSource)Common.ToImageSource(pnlMain);
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(img));
                Clipboard.SetImage(encoder.Frames[0]);
            }
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

        void cbxChartType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                string chartType = ((ComboBoxItem)e.AddedItems[0]).Content.ToString();
                switch (chartType)
                {
                    case "Stacked Column":
                        pnlScatterConfig.Visibility = Visibility.Collapsed;
                        pnlStackedColumnConfig.Visibility = Visibility.Visible;
                        pnlEpiCurveConfig.Visibility = Visibility.Collapsed;
                        pnlSingleConfig.Visibility = Visibility.Collapsed;
                        break;
                    case "Epi Curve":
                        pnlScatterConfig.Visibility = Visibility.Collapsed;
                        pnlStackedColumnConfig.Visibility = Visibility.Collapsed;
                        pnlEpiCurveConfig.Visibility = Visibility.Visible;
                        pnlSingleConfig.Visibility = Visibility.Collapsed;
                        break;
                    case "Scatter":
                        pnlScatterConfig.Visibility = Visibility.Visible;
                        pnlStackedColumnConfig.Visibility = Visibility.Collapsed;
                        pnlEpiCurveConfig.Visibility = Visibility.Collapsed;
                        pnlSingleConfig.Visibility = Visibility.Collapsed; 
                        break;
                    default:
                        pnlScatterConfig.Visibility = Visibility.Collapsed;
                        pnlStackedColumnConfig.Visibility = Visibility.Collapsed;
                        pnlEpiCurveConfig.Visibility = Visibility.Collapsed;
                        pnlSingleConfig.Visibility = Visibility.Visible;
                        if (chartType.ToLower().Equals("pie") || chartType.ToLower().Equals("pareto"))
                        {
                            cbxStrataField.Visibility = System.Windows.Visibility.Collapsed;
                            tblockStrataField.Visibility = System.Windows.Visibility.Collapsed;
                        }
                        else
                        {
                            cbxStrataField.Visibility = System.Windows.Visibility.Visible;
                            tblockStrataField.Visibility = System.Windows.Visibility.Visible;
                        }
                        break;
                }
                ResetComboboxes();
            }
        }

        private void ResetComboboxes()
        {
            loadingCombos = true;

            if (cbxColumnXAxisField.Items.Count > 0)
                cbxColumnXAxisField.SelectedIndex = -1;
            if (cbxColumnYAxisField.Items.Count > 0)
                cbxColumnYAxisField.SelectedIndex = -1;
            if (cbxCaseStatusField.Items.Count > 0)
                cbxCaseStatusField.SelectedIndex = -1;
            if (cbxDateField.Items.Count > 0)
                cbxDateField.SelectedIndex = -1;
            if (cbxSingleField.Items.Count > 0)
                cbxSingleField.SelectedIndex = -1;
            if (cbxWeightField.Items.Count > 0)
                cbxWeightField.SelectedIndex = -1;
            if (cbxStrataField.Items.Count > 0)
                cbxStrataField.SelectedIndex = -1;  
            if (cbxScatterXAxisField.Items.Count > 0)
                cbxScatterXAxisField.SelectedIndex = -1;
            if (cbxScatterYAxisField.Items.Count > 0)
                cbxScatterYAxisField.SelectedIndex = -1;

            loadingCombos = false;
        }

        void ConfigField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!loadingCombos)
            {
                bool isDropDownList = false;
                bool isRecoded = false;

                if (cbxChartSize.SelectedIndex == 3)
                {
                    tblockCustomHeight.Visibility = System.Windows.Visibility.Visible;
                    tblockCustomWidth.Visibility = System.Windows.Visibility.Visible;
                    txtCustomHeight.Visibility = System.Windows.Visibility.Visible;
                    txtCustomWidth.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    tblockCustomHeight.Visibility = System.Windows.Visibility.Collapsed;
                    tblockCustomWidth.Visibility = System.Windows.Visibility.Collapsed;
                    txtCustomHeight.Visibility = System.Windows.Visibility.Collapsed;
                    txtCustomWidth.Visibility = System.Windows.Visibility.Collapsed;

                    txtCustomWidth.Text = string.Empty;
                    txtCustomHeight.Text = string.Empty;
                }

                if (cbxChartType.Text.ToLower().Equals("column") || cbxChartType.Text.ToLower().Equals("bar"))
                {
                    if (cbxSingleField.SelectedItem != null && !string.IsNullOrEmpty(cbxSingleField.SelectedItem.ToString()))
                    {
                        foreach (DataRow fieldRow in DashboardHelper.FieldTable.Rows)
                        {
                            if (fieldRow["columnname"].Equals(cbxSingleField.SelectedItem.ToString()))
                            {
                                if (fieldRow["epifieldtype"] is TableBasedDropDownField)
                                {
                                    isDropDownList = true;
                                }
                                break;
                            }
                        }
                    }

                    if (cbxSingleField.SelectedIndex >= 0 && DashboardHelper.IsUserDefinedColumn(cbxSingleField.SelectedItem.ToString()))
                    {
                        List<IDashboardRule> associatedRules = DashboardHelper.Rules.GetRules(cbxSingleField.SelectedItem.ToString());
                        foreach (IDashboardRule rule in associatedRules)
                        {
                            if (rule is Rule_Recode)
                            {
                                isRecoded = true;
                            }
                        }
                    }

                    if (isDropDownList || isRecoded)
                    {
                        checkboxAllValues.IsEnabled = true;
                    }
                    else
                    {
                        checkboxAllValues.IsEnabled = false;
                        checkboxAllValues.IsChecked = false;
                    }
                }
                else if (cbxChartType.Text.ToLower().Equals("epi curve"))
                {
                    string columnName = e.AddedItems[0].ToString();
                    if (cbxDateField.SelectedIndex >= 0)
                    {
                        columnName = cbxDateField.SelectedItem.ToString();
                    }

                    if (e.AddedItems.Count == 1 && e.AddedItems[0] is string && !string.IsNullOrEmpty(columnName))
                    {
                        if (!DashboardHelper.IsColumnDateTime(columnName))
                        {
                            tblockDateInterval.Visibility = System.Windows.Visibility.Collapsed;
                            cbxDateInterval.Visibility = System.Windows.Visibility.Collapsed;
                            tblockXAxisLabelRotation.Visibility = System.Windows.Visibility.Collapsed;
                            cbxXAxisLabelRotation.Visibility = System.Windows.Visibility.Collapsed;
                        }
                        else
                        {
                            tblockDateInterval.Visibility = System.Windows.Visibility.Visible;
                            cbxDateInterval.Visibility = System.Windows.Visibility.Visible;
                            tblockXAxisLabelRotation.Visibility = System.Windows.Visibility.Visible;
                            cbxXAxisLabelRotation.Visibility = System.Windows.Visibility.Visible;

                            DateTime? minDate = null;
                            DateTime? maxDate = null;
                            DashboardHelper.FindUpperLowerDateValues(columnName, ref minDate, ref maxDate);

                            if (minDate.HasValue && maxDate.HasValue)
                            {
                                TimeSpan timeSpan = (DateTime)maxDate - (DateTime)minDate;

                                if (timeSpan.TotalHours <= 24)
                                {
                                    cbxDateInterval.SelectedIndex = 1; // Hours
                                }
                                else if (timeSpan.TotalDays < 150)
                                {
                                    cbxDateInterval.SelectedIndex = 0; // Days
                                }
                                else if (timeSpan.TotalDays <= 366)
                                {
                                    cbxDateInterval.SelectedIndex = 2; // Months
                                }
                                else
                                {
                                    cbxDateInterval.SelectedIndex = 3; // Years
                                }
                            }
                        }
                    }
                }
                else
                {
                    checkboxAllValues.IsChecked = false;
                }

                //RefreshResults();
            }
        }

        private void GenerateStackedColumnChart(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (worker != null && worker.WorkerSupportsCancellation)
            {
                worker.CancelAsync();
            }

            lock (syncLock)
            {
                worker = new System.ComponentModel.BackgroundWorker();
                worker.DoWork += new System.ComponentModel.DoWorkEventHandler(stackedChartWorker_DoWork);
                worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(stackedChartWorker_RunWorkerCompleted);
                worker.RunWorkerAsync();
            }
        }

        void stackedChartWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                List<List<StringDataValue>> dataValues = (List<List<StringDataValue>>)((object[])e.Result)[0];
                DataTable data = (DataTable)((object[])e.Result)[1];
                this.Dispatcher.BeginInvoke(new RenderFinishStackedChartDelegate(RenderFinishStackedChart), dataValues, data);
                System.Threading.Thread.Sleep(2500);
                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
            }
        }

        private void stackedChartWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));

                RequestUpdateStatusDelegate requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
                CheckForCancellationDelegate checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

                GadgetOptions.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                GadgetOptions.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);

                DataTable data = GetStackedColumnData();
                if (data == null || data.Rows.Count == 0)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.NO_RECORDS_SELECTED);
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                    return;
                }
                else if (worker.CancellationPending)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.DASHBOARD_GADGET_STATUS_OPERATION_CANCELLED);
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                    System.Diagnostics.Debug.Print("Stacked chart thread was cancelled");
                    return;
                }
                else if (data != null && data.Rows.Count > 0)
                {
                    //DataRow[] allRows;

                    //if (dashboardHelper.IsUsingEpiProject && data.Rows[0][0].ToString().Equals(dashboardHelper.Config.Settings.RepresentationOfYes) && (dashboardHelper.IsColumnYesNo(data.Columns[0].ColumnName) || dashboardHelper.IsColumnBoolean(data.Columns[0].ColumnName)))
                    //{
                    //    allRows = data.Select(string.Empty, "[" + data.Columns[0].ColumnName + "] DESC");
                    //}
                    //else
                    //{
                    //    allRows = data.Select(string.Empty, "[" + data.Columns[0].ColumnName + "]");
                    //}                    
                    
                    List<List<StringDataValue>> dataValues = new List<List<StringDataValue>>();
                    for (int i = 1; i < data.Columns.Count; i++)
                    {
                        List<StringDataValue> values = new List<StringDataValue>();
                        foreach (DataRow row in data.Rows)
                        {
                            StringDataValue val = new StringDataValue();
                            if (row[i] != DBNull.Value)
                            {
                                val.DependentValue = (int)row[i];
                            }
                            else
                            {
                                val.DependentValue = 0;
                            }
                            val.IndependentValue = row[0].ToString();
                            values.Add(val);
                        }
                        dataValues.Add(values);
                    }

                    object[] result = new object[] { dataValues, data };
                    e.Result = result;

                    this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));
                }                
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
            }
        }

        private void RenderFinishStackedChart(List<List<StringDataValue>> dataValues, DataTable data)
        {
            StackedColumnSeries stackedSeries;
            if (((ComboBoxItem)cbxColumnAggregateFunc.SelectedItem).Content.ToString().Equals("Count %"))
                stackedSeries = new Stacked100ColumnSeries();
            else
                stackedSeries = new StackedColumnSeries();

            int counter = 1;
            foreach (var values in dataValues)
            {
                SeriesDefinition definition = new SeriesDefinition();
                definition.DependentValuePath = "DependentValue";
                definition.IndependentValuePath = "IndependentValue";
                if (DashboardHelper.IsUsingEpiProject && (View.Fields[cbxColumnYAxisField.SelectedItem.ToString()] is YesNoField || View.Fields[cbxColumnYAxisField.SelectedItem.ToString()] is CheckBoxField))
                {
                    if (data.Columns[counter].ColumnName.Equals(DashboardHelper.Config.Settings.RepresentationOfYes))
                        definition.Title = cbxColumnYAxisField.SelectedItem.ToString();
                    else
                        definition.Title = "Not " + cbxColumnYAxisField.SelectedItem.ToString();
                }
                else
                    definition.Title = data.Columns[counter].ColumnName;
                definition.ItemsSource = values;
                stackedSeries.SeriesDefinitions.Add(definition);
                counter++;
            }

            chart = new Chart();
            chart.Loaded += new RoutedEventHandler(chart_Loaded);
            chart.BorderThickness = new Thickness(0); 
            chart.Series.Add(stackedSeries);
            chart.Height = 600;

            chart.PlotAreaStyle = new Style();
            chart.PlotAreaStyle.Setters.Add(new Setter(Chart.BackgroundProperty, Brushes.White));

            chart.Palette = (System.Collections.ObjectModel.Collection<ResourceDictionary>)this.Resources["defaultColumnPalette"];

            if (data.Rows.Count > 20)
                chart.Width = data.Rows.Count * 40;
            else
                chart.Width = 800;
            pnlMain.Children.Clear();
            pnlMain.Children.Add(chart);
        }

        private DataTable GetStackedColumnData()
        {
            DataTable data = new DataTable();
            //string query = "select count(" + cbxColumnYAxisField.SelectedItem.ToString() + ") as totals, " + cbxColumnXAxisField.SelectedItem.ToString() + ", " + cbxColumnYAxisField.SelectedItem.ToString() + " " + view.FromViewSQL + " group by " + cbxColumnYAxisField.SelectedItem.ToString() + ", " + cbxColumnXAxisField.SelectedItem.ToString() + " order by " + cbxColumnYAxisField.SelectedItem.ToString();
            //data.Merge(db.Select(db.CreateQuery(query)), true, MissingSchemaAction.Add);
            string xAxisColumnName = GadgetOptions.MainVariableName;
            string yAxisColumnName = GadgetOptions.CrosstabVariableName;
            data = DashboardHelper.GenerateStackedBarTable(xAxisColumnName, yAxisColumnName);
            if (data != null && data.Rows.Count > 0)
            {
                data = Pivot(data, xAxisColumnName, yAxisColumnName, "totals");
            }
            return data;
        }

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
        /// Sends the gadget to the back of the canvas
        /// </summary>
        private void SendToBack()
        {
            Canvas.SetZIndex(this, -1);
        }

        protected override void RenderFinish()
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;                        

            pnlMain.Visibility = System.Windows.Visibility.Visible;

            messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
            messagePanel.Text = string.Empty;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            SetChartLabels(ChartTitle, LegendTitle, XAxisLabel, YAxisLabel);
            HideConfigPanel();
            CheckAndSetPosition();
        }

        protected override void RenderFinishWithWarning(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            pnlMain.Visibility = System.Windows.Visibility.Visible;

            messagePanel.MessagePanelType = Controls.MessagePanelType.WarningPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            SetChartLabels(ChartTitle, LegendTitle, XAxisLabel, YAxisLabel);
            HideConfigPanel();
            CheckAndSetPosition();
        }

        protected override void RenderFinishWithError(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;            

            pnlMain.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = Controls.MessagePanelType.ErrorPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        private void FillComboboxes(bool update = false)
        {
            loadingCombos = true;

            string prevDateField = string.Empty;
            string prevCaseStatusField = string.Empty;
            string prevColumnXAxisField = string.Empty;
            string prevColumnYAxisField = string.Empty;
            string prevScatterXAxisField = string.Empty;
            string prevScatterYAxisField = string.Empty;
            string prevSingleField = string.Empty;
            string prevWeightField = string.Empty;
            string prevStrataField = string.Empty;

            if (update)
            {
                if (cbxDateField.SelectedIndex >= 0)
                {
                    prevDateField = cbxDateField.SelectedItem.ToString();
                }
                if (cbxCaseStatusField.SelectedIndex >= 0)
                {
                    prevCaseStatusField = cbxCaseStatusField.SelectedItem.ToString();
                }
                if (cbxColumnXAxisField.SelectedIndex >= 0)
                {
                    prevColumnXAxisField = cbxColumnXAxisField.SelectedItem.ToString();
                }
                if (cbxColumnYAxisField.SelectedIndex >= 0)
                {
                    prevColumnYAxisField = cbxColumnYAxisField.SelectedItem.ToString();
                }
                if (cbxScatterXAxisField.SelectedIndex >= 0)
                {
                    prevScatterXAxisField = cbxScatterXAxisField.SelectedItem.ToString();
                }
                if (cbxScatterYAxisField.SelectedIndex >= 0)
                {
                    prevScatterYAxisField = cbxScatterYAxisField.SelectedItem.ToString();
                }
                if (cbxSingleField.SelectedIndex >= 0)
                {
                    prevSingleField = cbxSingleField.SelectedItem.ToString();
                }
                if (cbxWeightField.SelectedIndex >= 0)
                {
                    prevWeightField = cbxWeightField.SelectedItem.ToString();
                }
                if (cbxStrataField.SelectedIndex >= 0)
                {
                    prevStrataField = cbxStrataField.SelectedItem.ToString();
                }
            }

            cbxCaseStatusField.ItemsSource = null;
            cbxCaseStatusField.Items.Clear();

            cbxDateField.ItemsSource = null;
            cbxDateField.Items.Clear();

            List<string> caseStatusFields = new List<string>();
            List<string> dateFields = new List<string>();
            List<string> numericFields = new List<string>();
            List<string> allFields = new List<string>();

            caseStatusFields.Add(string.Empty);

            if (DashboardHelper.IsUsingEpiProject)
            {
                ColumnDataType columnDataType = ColumnDataType.Numeric | ColumnDataType.UserDefined;
                numericFields = DashboardHelper.GetFieldsAsList(columnDataType);

                columnDataType = ColumnDataType.DateTime | ColumnDataType.Numeric | ColumnDataType.Boolean | ColumnDataType.Text | ColumnDataType.UserDefined;
                allFields = DashboardHelper.GetFieldsAsList(columnDataType);

                foreach (Field f in View.Fields)
                {
                    if ((f is TableBasedDropDownField) || (f is YesNoField))
                    {
                        caseStatusFields.Add(f.Name);
                    }
                    if ((f is DateField) || (f is DateTimeField) || (f is NumberField))
                    {
                        dateFields.Add(f.Name);
                    }                    
                }

                foreach (EpiDashboard.Rules.IDashboardRule rule in DashboardHelper.Rules)
                {
                    if (rule is EpiDashboard.Rules.DataAssignmentRule)
                    {
                        EpiDashboard.Rules.DataAssignmentRule assignmentRule = rule as EpiDashboard.Rules.DataAssignmentRule;
                        if (!dateFields.Contains(assignmentRule.DestinationColumnName) && assignmentRule.VariableType.Equals(EpiDashboard.Rules.DashboardVariableType.Numeric))
                        {
                            dateFields.Add(assignmentRule.DestinationColumnName);
                        }
                    }
                }

                if (allFields.Contains("RecStatus"))
                {
                    allFields.Remove("RecStatus");
                }

                if (allFields.Contains("GlobalRecordId"))
                {
                    allFields.Remove("GlobalRecordId");
                }

                if (allFields.Contains("FKEY"))
                {
                    allFields.Remove("FKEY");
                }

                if (allFields.Contains("UniqueKey"))
                {
                    allFields.Remove("UniqueKey");
                }

                if (dateFields.Contains("SYSTEMDATE"))
                {
                    allFields.Remove("SYSTEMDATE");
                }

                dateFields.Sort();
                numericFields.Sort();
                caseStatusFields.Sort();
                allFields.Sort();
            }
            else
            {
                ColumnDataType columnDataType = ColumnDataType.DateTime | ColumnDataType.Numeric | ColumnDataType.UserDefined;
                dateFields = DashboardHelper.GetFieldsAsList(columnDataType);

                //columnDataType = ColumnDataType.Numeric;
                //weightFieldNames.AddRange(dashboardHelper.GetFieldsAsList(columnDataType));

                columnDataType = ColumnDataType.Numeric | ColumnDataType.Boolean | ColumnDataType.Text | ColumnDataType.UserDefined;
                caseStatusFields.AddRange(DashboardHelper.GetFieldsAsList(columnDataType));

                columnDataType = ColumnDataType.Numeric | ColumnDataType.UserDefined;
                numericFields = DashboardHelper.GetFieldsAsList(columnDataType);

                columnDataType = ColumnDataType.DateTime | ColumnDataType.Numeric | ColumnDataType.Boolean | ColumnDataType.Text | ColumnDataType.UserDefined;
                allFields = DashboardHelper.GetFieldsAsList(columnDataType);
            }

            foreach (EpiDashboard.Rules.IDashboardRule rule in DashboardHelper.Rules)
            {
                if (rule is EpiDashboard.Rules.Rule_Format)
                {
                    EpiDashboard.Rules.Rule_Format formatRule = rule as EpiDashboard.Rules.Rule_Format;
                    if (!dateFields.Contains(formatRule.DestinationColumnName) && (formatRule.FormatType == Rules.FormatTypes.Hours || formatRule.FormatType == Rules.FormatTypes.EpiWeek))
                    {
                        dateFields.Add(formatRule.DestinationColumnName);
                    }
                }
            }

            List<string> allFieldsWithSpace = new List<string>();
            allFieldsWithSpace.Add(string.Empty);
            allFieldsWithSpace.AddRange(allFields);

            List<string> numericFieldsWithSpace = new List<string>();
            numericFieldsWithSpace.Add(string.Empty);
            numericFieldsWithSpace.AddRange(numericFields);

            if (dateFields.Contains("SYSTEMDATE"))
            {
                dateFields.Remove("SYSTEMDATE");
            }

            if (allFields.Contains("SYSTEMDATE"))
            {
                allFields.Remove("SYSTEMDATE");
            }

            cbxDateField.ItemsSource = dateFields;
            cbxCaseStatusField.ItemsSource = caseStatusFields;
            cbxColumnXAxisField.ItemsSource = allFields;
            cbxColumnYAxisField.ItemsSource = allFields;
            cbxScatterXAxisField.ItemsSource = numericFields;
            cbxScatterYAxisField.ItemsSource = numericFields;
            cbxSingleField.ItemsSource = allFields;
            cbxWeightField.ItemsSource = numericFieldsWithSpace;
            cbxStrataField.ItemsSource = allFieldsWithSpace;

            if (cbxCaseStatusField.Items.Count > 0)
            {
                cbxCaseStatusField.SelectedIndex = -1;
            }
            if (cbxDateField.Items.Count > 0)
            {
                cbxDateField.SelectedIndex = -1;
            }
            if (cbxScatterXAxisField.Items.Count > 0)
            {
                cbxScatterXAxisField.SelectedIndex = -1;
                cbxScatterYAxisField.SelectedIndex = -1;
                cbxWeightField.SelectedIndex = -1;
            }
            if (cbxSingleField.Items.Count > 0)
            {
                cbxSingleField.SelectedIndex = -1;
                cbxStrataField.SelectedIndex = -1;
                cbxColumnXAxisField.SelectedIndex = -1;
                cbxColumnYAxisField.SelectedIndex = -1;
            }

            if (cbxChartSize.Items.Count == 0)
            {
                cbxChartSize.Items.Add("Small");
                cbxChartSize.Items.Add("Medium");
                cbxChartSize.Items.Add("Large");
                cbxChartSize.Items.Add("Custom");
                cbxChartSize.SelectedIndex = 1;
            }

            if (update)
            {
                cbxDateField.SelectedItem = prevDateField;
                cbxCaseStatusField.SelectedItem = prevCaseStatusField;
                cbxColumnXAxisField.SelectedItem = prevColumnXAxisField;
                cbxColumnYAxisField.SelectedItem = prevColumnYAxisField;
                cbxScatterXAxisField.SelectedItem = prevScatterXAxisField;
                cbxScatterYAxisField.SelectedItem = prevScatterYAxisField;
                cbxSingleField.SelectedItem = prevSingleField;
                cbxWeightField.SelectedItem = prevWeightField;
                cbxStrataField.SelectedItem = prevStrataField;
            }
            
            loadingCombos = false;
        }

        private void GenerateEpiCurve(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (worker != null && worker.WorkerSupportsCancellation)
            {
                worker.CancelAsync();
            }

            //pnlMain.Children.Clear();
            //waitCursor.Visibility = System.Windows.Visibility.Visible;
            lock (syncLock)
            {
                bool byEpiWeek = false;
                if (GadgetOptions.InputVariableList.ContainsKey("isdatecolumnnumeric"))
                {
                    byEpiWeek = bool.Parse(GadgetOptions.InputVariableList["isdatecolumnnumeric"]);
                }
                //System.ComponentModel.BackgroundWorker epiCurveWorker = new System.ComponentModel.BackgroundWorker();
                worker = new BackgroundWorker();
                worker.DoWork += new System.ComponentModel.DoWorkEventHandler(epiCurveWorker_DoWork);
                worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(epiCurveWorker_RunWorkerCompleted);
                object[] args = new object[4];
                args[0] = byEpiWeek;
                args[1] = GadgetOptions.MainVariableName;// cbxDateField.SelectedItem.ToString();
                args[2] = GadgetOptions.CrosstabVariableName;
                args[3] = GadgetOptions.InputVariableList["dateinterval"];
                //if (cbxCaseStatusField.SelectedIndex > -1)
                //{
                //    args[2] = cbxCaseStatusField.SelectedItem.ToString();
                //}
                //else
                //{
                //    args[2] = string.Empty;
                //}
                worker.RunWorkerAsync(args);
            }
        }

        void epiCurveWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {                
                DataTable data = (DataTable)((object[])e.Result)[1];
                List<List<StringDataValue>> dataValues = (List<List<StringDataValue>>)((object[])e.Result)[0];
                this.Dispatcher.BeginInvoke(new RenderFinishEpiCurveDelegate(RenderFinishEpiCurve), data, dataValues);
                System.Threading.Thread.Sleep(2500);
                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
            }
        }

        private void RenderFinishEpiCurve(DataTable data, List<List<StringDataValue>> dataValues)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            StackedHistogramSeries stackedSeries = new StackedHistogramSeries();
            LinearAxis axis = new LinearAxis();
            axis.Orientation = AxisOrientation.Y;
            axis.Minimum = 0;
            axis.ShowGridLines = (bool)checkboxShowHorizontalGridLines.IsChecked; //true;
            stackedSeries.DependentAxis = axis;
            CategoryAxis independentAxis = new CategoryAxis();
            independentAxis.Orientation = AxisOrientation.X;

            double extraRightPadding = 0;

            if (!DashboardHelper.IsColumnNumeric(cbxDateField.SelectedItem.ToString()))
            {
                if (cbxXAxisLabelRotation.SelectedIndex == -1 || ((ComboBoxItem)cbxXAxisLabelRotation.SelectedItem).Content.ToString().Equals("90"))
                {
                    independentAxis.AxisLabelStyle = Resources["RotateAxisStyle90"] as Style;
                }
                else if (cbxXAxisLabelRotation.SelectedIndex >= 0 && ((ComboBoxItem)cbxXAxisLabelRotation.SelectedItem).Content.ToString().Equals("45"))
                {
                    independentAxis.AxisLabelStyle = Resources["RotateAxisStyle45"] as Style;
                    extraRightPadding = 50;
                }
            }

            stackedSeries.IndependentAxis = independentAxis;

            int counter = 1;
            foreach (var values in dataValues)
            {
                SeriesDefinition definition = new SeriesDefinition();
                definition.DependentValuePath = "DependentValue";
                definition.IndependentValuePath = "IndependentValue";
                if (cbxCaseStatusField.SelectedIndex >= 0 && !string.IsNullOrEmpty(cbxCaseStatusField.SelectedItem.ToString()))
                {
                    Field field = null;
                    foreach (DataRow fieldRow in DashboardHelper.FieldTable.Rows)
                    {
                        if (fieldRow["columnname"].Equals(cbxCaseStatusField.SelectedItem.ToString()))
                        {
                            if (fieldRow["epifieldtype"] is Field)
                            {
                                field = fieldRow["epifieldtype"] as Field;
                            }
                            break;
                        }
                    }

                    if (field != null && (field is YesNoField || field is CheckBoxField))
                    {
                        string value = data.Columns[counter].ColumnName;

                        if (value.ToLower().Equals("true") || value.Equals("1"))
                        {
                            definition.Title = DashboardHelper.Config.Settings.RepresentationOfYes;
                        }
                        else if (value.ToLower().Equals("false") || value.Equals("0"))
                        {
                            definition.Title = DashboardHelper.Config.Settings.RepresentationOfNo;
                        }
                        else
                        {
                            definition.Title = cbxCaseStatusField.SelectedItem.ToString();
                        }
                    }
                    else
                    {
                        definition.Title = data.Columns[counter].ColumnName;
                    }
                }
                else
                {
                    definition.Title = SharedStrings.DEFAULT_CHART_LEGEND_ITEM;
                }
                definition.ItemsSource = values;
                //definition.DataPointStyle = this.Resources["epiCurveDataPointStyle"] as Style;

                stackedSeries.SeriesDefinitions.Add(definition);
                counter++;
            }

            Chart chart = new Chart();

            if (dataValues.Count == 1)
            {
                chart.LegendStyle = this.Resources["noLegendStyle"] as Style;
            }

            chart.PlotAreaStyle = new Style();
            chart.PlotAreaStyle.Setters.Add(new Setter(Chart.BackgroundProperty, Brushes.White));

            axis.GridLineStyle = new Style();
            axis.GridLineStyle.Setters.Add(new Setter(Line.StrokeProperty, Brushes.LightGray));

            chart.Palette = (System.Collections.ObjectModel.Collection<ResourceDictionary>)this.Resources["defaultColumnPalette"];

            //stackedSeries
            

            chart.BorderThickness = new Thickness(0); 
            chart.Loaded += new RoutedEventHandler(chart_Loaded);
            chart.Series.Add(stackedSeries);
            chart.Height = 500;
            chart.Width = (stackedSeries.IndependentValueCount * 25) + 150;

            chart.Margin = new Thickness(chart.Margin.Left, chart.Margin.Top, chart.Margin.Right + extraRightPadding, chart.Margin.Bottom);

            pnlMain.Children.Clear();
            pnlMain.Children.Add(chart);

            SetChartLabels(ChartTitle, LegendTitle, XAxisLabel, YAxisLabel);
        }

        void epiCurveWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                object[] args = (object[])e.Argument;
                bool byEpiWeek = (bool)args[0];
                string dateVar = args[1].ToString();
                string caseStatusVar = args[2].ToString();
                string dateFormat = args[3].ToString();

                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));

                RequestUpdateStatusDelegate requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
                CheckForCancellationDelegate checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

                GadgetOptions.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                GadgetOptions.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);
                
                //System.Threading.Thread.Sleep(100);

                DataTable data = GetEpiCurveData(byEpiWeek, dateVar, caseStatusVar);
                if (data != null && data.Rows.Count > 0)
                {
                    DataRow[] allRows = data.Select(string.Empty, "[" + data.Columns[0].ColumnName + "]");
                    List<List<StringDataValue>> dataValues = new List<List<StringDataValue>>();
                    for (int i = 1; i < data.Columns.Count; i++)
                    {
                        List<StringDataValue> values = new List<StringDataValue>();
                        foreach (DataRow row in allRows)
                        {
                            StringDataValue val = new StringDataValue();
                            if (row[i] != DBNull.Value)
                            {
                                val.DependentValue = (int)row[i];
                            }
                            else
                            {
                                val.DependentValue = 0;
                            }
                            if (byEpiWeek)
                            {
                                val.IndependentValue = row[0].ToString();
                            }
                            else
                            {
                                if (dateFormat.ToLower().Equals("hours"))
                                {
                                    val.IndependentValue = ((DateTime)row[0]).ToString("g");
                                }
                                else if (dateFormat.ToLower().Equals("months"))
                                {
                                    val.IndependentValue = ((DateTime)row[0]).ToString("MMM yyyy");                                    
                                }
                                else if (dateFormat.ToLower().Equals("years"))
                                {
                                    val.IndependentValue = ((DateTime)row[0]).ToString("yyyy");
                                }
                                else
                                {
                                    val.IndependentValue = ((DateTime)row[0]).ToShortDateString();
                                }
                            }
                            values.Add(val);
                        }
                        dataValues.Add(values);
                    }
                    object[] retVals = new object[2];
                    retVals[0] = dataValues;
                    retVals[1] = data;
                    e.Result = retVals;
                    this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));
                }
                else if (data == null || data.Rows.Count == 0)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.NO_RECORDS_SELECTED);
                }
                else
                {
                    this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));
                }
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
            }
        }

        private List<StringDataValue> ConvertToPct(List<StringDataValue> list)
        {
            List<StringDataValue> values = new List<StringDataValue>();
            foreach (StringDataValue value in list)
            {
                values.Add(new StringDataValue() { IndependentValue = value.IndependentValue, DependentValue = value.DependentValue / 31 * 100 });
            }
            return values;
        }

        private DataTable GetEpiCurveData(bool byEpiWeek, string dateVar, string caseStatusVar)
        {
            DataTable data = new DataTable();            

            if (byEpiWeek)
            {
                decimal? minWeek = null;
                decimal? maxWeek = null;

                DashboardHelper.FindUpperLowerNumericValues(dateVar, ref minWeek, ref maxWeek);
                if (minWeek.HasValue && maxWeek.HasValue)                
                {
                    // override min and max values if the user set custom start and end points for the x-axis
                    if (GadgetOptions.InputVariableList.ContainsKey("xaxisstart"))
                    {
                        minWeek = decimal.Parse(GadgetOptions.InputVariableList["xaxisstart"]);
                    }
                    if (GadgetOptions.InputVariableList.ContainsKey("xaxisend"))
                    {
                        maxWeek = decimal.Parse(GadgetOptions.InputVariableList["xaxisend"]);
                    }

                    if (string.IsNullOrEmpty(caseStatusVar))
                    {
                        GadgetOptions.CrosstabVariableName = "_default_";
                        data = DashboardHelper.GenerateEpiCurveTable(GadgetOptions, minWeek.Value, maxWeek.Value);
                    }
                    else
                    {
                        data = DashboardHelper.GenerateEpiCurveTable(GadgetOptions, minWeek.Value, maxWeek.Value);
                    }
                    if (data != null && data.Rows.Count > 0)
                    {
                        if (string.IsNullOrEmpty(caseStatusVar))
                        {
                            data = Pivot(data, dateVar, "_default_", "totals");
                        }
                        else
                        {
                            data = Pivot(data, dateVar, caseStatusVar, "totals");
                        }
                    }
                    decimal currentWeek = minWeek.Value;
                    while (currentWeek <= maxWeek.Value)
                    {
                        if (data.Select("[" + dateVar + "] = '" + currentWeek.ToString() + "'").Count() == 0)
                        {
                            DataRow row = data.NewRow();
                            row[0] = currentWeek;
                            data.Rows.Add(row);
                        }
                        currentWeek = currentWeek + 1;
                    }
                }
            }
            else
            {
                string dateInterval = GadgetOptions.InputVariableList["dateinterval"];

                DateTime? minDate = null;
                DateTime? maxDate = null;

                DashboardHelper.FindUpperLowerDateValues(dateVar, ref minDate, ref maxDate);
                if (minDate.HasValue && maxDate.HasValue)
                {
                    if (GadgetOptions.InputVariableList.ContainsKey("xaxisstart"))
                    {
                        DateTime dt;
                        bool success = DateTime.TryParse(GadgetOptions.InputVariableList["xaxisstart"], out dt);
                        if (success)
                        {
                            minDate = dt;
                        }
                    }
                    if (GadgetOptions.InputVariableList.ContainsKey("xaxisend"))
                    {
                        DateTime dt;
                        bool success = DateTime.TryParse(GadgetOptions.InputVariableList["xaxisend"], out dt);
                        if (success)
                        {
                            maxDate = dt;
                        }                        
                    }

                    if (string.IsNullOrEmpty(caseStatusVar))
                    {
                        GadgetOptions.CrosstabVariableName = "_default_";                        
                    }

                    data = DashboardHelper.GenerateEpiCurveTable(GadgetOptions, (DateTime)minDate, (DateTime)maxDate);

                    if (data != null && data.Rows.Count > 0)
                    {
                        if (string.IsNullOrEmpty(caseStatusVar))
                        {
                            data = Pivot(data, dateVar, "_default_", "totals");
                        }
                        else
                        {
                            data = Pivot(data, dateVar, caseStatusVar, "totals");
                        }
                    }
                    DateTime currentDate = minDate.Value;
                    while (currentDate <= maxDate.Value)
                    {
                        if (IsCancelled())
                        {
                            data = null;
                            return data;
                        }

                        if (data.Select("[" + dateVar + "] = '" + currentDate.ToString() + "'").Count() == 0)
                        {
                            DataRow row = data.NewRow();
                            row[0] = currentDate;
                            data.Rows.Add(row);
                        }

                        //if (dashboardHelper.IsUsingEpiProject && View.Fields.Contains(dateVar) && View.Fields[dateVar] is DateTimeField && !(View.Fields[dateVar] is DateField) && !(View.Fields[dateVar] is TimeField))
                        if (dateInterval.ToLower().Equals("hours"))
                        {
                            currentDate = currentDate.AddHours(1);
                        }
                        else if (dateInterval.ToLower().Equals("days"))
                        {
                            currentDate = currentDate.AddDays(1);
                        }
                        else if (dateInterval.ToLower().Equals("months"))
                        {
                            currentDate = currentDate.AddMonths(1);
                        }
                        else if (dateInterval.ToLower().Equals("years"))
                        {
                            currentDate = currentDate.AddMonths(1);
                        }
                    }
                }
            }
            return data;
        }

        public static DataTable Pivot(DataTable dataValues, string keyColumn, string pivotNameColumn, string pivotValueColumn)
        {
            DataTable tmp = new DataTable();
            DataRow r;
            string LastKey = "//dummy//";
            int i, pValIndex, pNameIndex;
            string s;
            bool FirstRow = true;

            pValIndex = dataValues.Columns[pivotValueColumn].Ordinal;
            pNameIndex = dataValues.Columns[pivotNameColumn].Ordinal;

            for (i = 0; i <= dataValues.Columns.Count - 1; i++)
            {
                if (i != pValIndex && i != pNameIndex)
                    tmp.Columns.Add(dataValues.Columns[i].ColumnName, dataValues.Columns[i].DataType);
            }

            r = tmp.NewRow();

            foreach (DataRow row1 in dataValues.Rows)
            {
                if (row1[keyColumn] != DBNull.Value)
                {
                    if (row1[keyColumn].ToString() != LastKey)
                    {
                        if (!FirstRow)
                            tmp.Rows.Add(r);

                        r = tmp.NewRow();
                        FirstRow = false;

                        //loop thru fields of row1 and populate tmp table
                        for (i = 0; i <= row1.ItemArray.Length - 3; i++)
                            r[i] = row1[tmp.Columns[i].ToString()];

                        LastKey = row1[keyColumn].ToString();
                    }

                    s = row1[pNameIndex].ToString();

                    if (!string.IsNullOrEmpty(s))
                    {
                        if (!tmp.Columns.Contains(s))
                            tmp.Columns.Add(s, typeof(int));// dataValues.Columns[pNameIndex].DataType);
                        r[s] = row1[pValIndex];
                    }
                }
            }

            //add that final row to the datatable:
            tmp.Rows.Add(r);
            return tmp;
        }
       
        private bool IsBooleanWithNoStratas
        {
            get
            {
                return this.isBooleanWithNoStratas;
            }
            set
            {
                this.isBooleanWithNoStratas = value;
            }
        }

        public void ClearResults()
        {
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Text = string.Empty;
            waitPanel.Visibility = Visibility.Collapsed;//Visibility.Visible;

            IsBooleanWithNoStratas = false;

            pnlMain.Children.Clear();
            pnlMain.Visibility = System.Windows.Visibility.Collapsed;
        }

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

        /// <summary>
        /// Updates the variable names available in the gadget's properties
        /// </summary>
        public override void UpdateVariableNames()
        {
            FillComboboxes(true);
        }

        public override void RefreshResults()
        {
            if (!loadingCombos && GadgetOptions != null && ((cbxDateField.SelectedIndex > -1) || (cbxColumnXAxisField.SelectedIndex > -1 && cbxColumnYAxisField.SelectedIndex > -1) || (cbxScatterXAxisField.SelectedIndex > -1 && cbxScatterYAxisField.SelectedIndex > -1) || (cbxSingleField.SelectedIndex > -1)))
            {
                waitPanel.Visibility = System.Windows.Visibility.Visible;
                messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;

                //this.Dispatcher.BeginInvoke(GadgetStatusUpdate, SharedStrings.DASHBOARD_GADGET_STATUS_INITIALIZING);
                
                GadgetOptions.MainVariableName = string.Empty;
                GadgetOptions.WeightVariableName = string.Empty;
                GadgetOptions.StrataVariableNames = new List<string>();
                GadgetOptions.CrosstabVariableName = string.Empty;
                GadgetOptions.InputVariableList = new Dictionary<string, string>();
                GadgetOptions.ShouldSortHighToLow = false;

                baseWorker = new BackgroundWorker();

                if (!string.IsNullOrEmpty(txtCustomHeight.Text))
                {
                    GadgetOptions.InputVariableList.Add("chartheight", txtCustomHeight.Text);
                }

                if (!string.IsNullOrEmpty(txtCustomWidth.Text))
                {
                    GadgetOptions.InputVariableList.Add("chartwidth", txtCustomWidth.Text);
                }

                if (checkboxAllValues.IsChecked == true)
                {
                    GadgetOptions.ShouldUseAllPossibleValues = true;
                }
                else
                {
                    GadgetOptions.ShouldUseAllPossibleValues = false;
                }                

                switch (((ComboBoxItem)cbxChartType.SelectedItem).Content.ToString())
                {
                    case "Stacked Column":
                        //GenerateStackedColumnChart();
                        if (cbxColumnXAxisField.SelectedIndex >= 0 && cbxColumnYAxisField.SelectedIndex >= 0)
                        {
                            GadgetOptions.MainVariableName = cbxColumnXAxisField.SelectedItem.ToString();
                            GadgetOptions.CrosstabVariableName = cbxColumnYAxisField.SelectedItem.ToString();
                            baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(GenerateStackedColumnChart);
                            baseWorker.RunWorkerAsync();
                        }
                        break;
                    case "Epi Curve":
                        if (!string.IsNullOrEmpty(txtXAxisStartValue.Text))
                        {
                            GadgetOptions.InputVariableList.Add("xaxisstart", txtXAxisStartValue.Text);
                        }
                        if (!string.IsNullOrEmpty(txtXAxisEndValue.Text))
                        {
                            GadgetOptions.InputVariableList.Add("xaxisend", txtXAxisEndValue.Text);
                        }
                        GadgetOptions.InputVariableList.Add("isdatecolumnnumeric", DashboardHelper.IsColumnNumeric(cbxDateField.SelectedItem.ToString()).ToString().ToLower());
                        GadgetOptions.InputVariableList.Add("dateinterval", cbxDateInterval.Text);
                        GadgetOptions.MainVariableName = cbxDateField.SelectedItem.ToString();
                        if (cbxCaseStatusField.SelectedIndex >= 0)
                        {
                            GadgetOptions.CrosstabVariableName = cbxCaseStatusField.SelectedItem.ToString();
                        }
                        //GenerateEpiCurve(dashboardHelper.IsColumnNumeric(cbxDateField.SelectedItem.ToString()));
                        baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(GenerateEpiCurve);
                        baseWorker.RunWorkerAsync();
                        break;
                    case "Scatter":
                        //GenerateScatterChart();                        
                        if (cbxScatterXAxisField.SelectedIndex >= 0 && cbxScatterYAxisField.SelectedIndex >= 0)
                        {
                            GadgetOptions.MainVariableName = cbxScatterXAxisField.SelectedItem.ToString();
                            GadgetOptions.CrosstabVariableName = cbxScatterYAxisField.SelectedItem.ToString();
                            baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(GenerateScatterChart);
                            baseWorker.RunWorkerAsync();
                        }
                        break;                        
                    case "Pie":
                        if (cbxChartSize.SelectedIndex > -1)
                        {
                            GadgetOptions.InputVariableList.Add("chartsize", cbxChartSize.SelectedItem.ToString());
                        }
                        GadgetOptions.InputVariableList.Add("charttype", "pie");
                        GadgetOptions.ShouldSortHighToLow = true;
                        GadgetOptions.MainVariableName = cbxSingleField.SelectedItem.ToString();
                        if (cbxWeightField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxWeightField.SelectedItem.ToString()))
                        {
                            GadgetOptions.WeightVariableName = cbxWeightField.SelectedItem.ToString();
                        }
                        if (cbxStrataField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxStrataField.SelectedItem.ToString()))
                        {
                            List<string> strataVars = new List<string>();
                            strataVars.Add(cbxStrataField.SelectedItem.ToString());
                            GadgetOptions.StrataVariableNames = strataVars;
                        }
                        baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(GenerateSingleChart);
                        baseWorker.RunWorkerAsync();
                        //GenerateSingleChart();
                        break;
                    default:
                        if (cbxChartSize.SelectedIndex > -1)
                        {
                            GadgetOptions.InputVariableList.Add("chartsize", cbxChartSize.SelectedItem.ToString());
                        }

                        string chartType = (((ComboBoxItem)cbxChartType.SelectedItem).Content.ToString());
                        if (chartType.ToLower().Equals("pareto"))
                        {
                            GadgetOptions.ShouldSortHighToLow = true;
                            GadgetOptions.InputVariableList.Add("charttype", "pareto");
                        }
                        GadgetOptions.MainVariableName = cbxSingleField.SelectedItem.ToString();

                        if (cbxWeightField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxWeightField.SelectedItem.ToString()))
                        {
                            GadgetOptions.WeightVariableName = cbxWeightField.SelectedItem.ToString();
                        }
                        if (!chartType.ToLower().Equals("pareto") && cbxStrataField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxStrataField.SelectedItem.ToString()))
                        {
                            List<string> strataVars = new List<string>();
                            strataVars.Add(cbxStrataField.SelectedItem.ToString());
                            GadgetOptions.StrataVariableNames = strataVars;
                        }
                        baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(GenerateSingleChart);
                        baseWorker.RunWorkerAsync();
                        //GenerateSingleChart();
                        break;
                }
            }
        }

        private void GenerateScatterChart(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            //pnlMain.Children.Clear();
            //waitCursor.Visibility = System.Windows.Visibility.Visible;

            //System.ComponentModel.BackgroundWorker scatterChartWorker = new System.ComponentModel.BackgroundWorker();
            //scatterChartWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(scatterChartWorker_DoWork);
            //scatterChartWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(scatterChartWorker_RunWorkerCompleted);
            //List<string> columnNames = new List<string>();
            //columnNames.Add(cbxScatterXAxisField.SelectedItem.ToString());
            //columnNames.Add(cbxScatterYAxisField.SelectedItem.ToString());

            //scatterChartWorker.RunWorkerAsync(columnNames);

            if (worker != null && worker.WorkerSupportsCancellation)
            {
                worker.CancelAsync();
            }

            lock (syncLock)
            {
                worker = new System.ComponentModel.BackgroundWorker();
                worker.DoWork += new System.ComponentModel.DoWorkEventHandler(scatterChartWorker_DoWork);
                worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(scatterChartWorker_RunWorkerCompleted);
                //object[] args = new object[2];
                //args[0] = GadgetOptions;                
                worker.RunWorkerAsync(/*args*/);
            }
        }

        void scatterChartWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                List<NumericDataValue> dataValues = (List<NumericDataValue>)((object[])e.Result)[0];
                StatisticsRepository.LinearRegression.LinearRegressionResults results = (StatisticsRepository.LinearRegression.LinearRegressionResults)((object[])e.Result)[1];

                NumericDataValue maxValue = (NumericDataValue)((object[])e.Result)[2];
                NumericDataValue minValue = (NumericDataValue)((object[])e.Result)[3];

                this.Dispatcher.BeginInvoke(new RenderFinishScatterChartDelegate(RenderFinishScatterChart), dataValues, results, maxValue, minValue);

                System.Threading.Thread.Sleep(2500);
                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
            }
        }

        void chart_Loaded(object sender, RoutedEventArgs e)
        {
            SetChartLabels(ChartTitle, LegendTitle, XAxisLabel, YAxisLabel);
        }

        void scatterChartWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {   
                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));

                RequestUpdateStatusDelegate requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
                CheckForCancellationDelegate checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

                GadgetOptions.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                GadgetOptions.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);
                
                string xAxisVar = GadgetOptions.MainVariableName;
                string yAxisVar = GadgetOptions.CrosstabVariableName;

                List<string> columnNames = new List<string>();
                columnNames.Add(xAxisVar);
                columnNames.Add(yAxisVar);

                DataTable regressTable = DashboardHelper.GenerateTable(columnNames);

                if (regressTable == null || regressTable.Rows.Count == 0)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.NO_RECORDS_SELECTED);
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                    return;
                }
                else if (worker.CancellationPending)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.DASHBOARD_GADGET_STATUS_OPERATION_CANCELLED);
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                    System.Diagnostics.Debug.Print("Single chart thread was cancelled");
                    return;
                }
                else
                {
                    List<NumericDataValue> dataValues = new List<NumericDataValue>();
                    NumericDataValue minValue = null;
                    NumericDataValue maxValue = null;
                    foreach (DataRow row in regressTable.Rows)
                    {
                        if (row[yAxisVar].Equals(DBNull.Value) || row[xAxisVar].Equals(DBNull.Value))
                        {
                            continue;
                        }
                        NumericDataValue currentValue = new NumericDataValue() { DependentValue = Convert.ToDecimal(row[yAxisVar]), IndependentValue = Convert.ToDecimal(row[xAxisVar]) };
                        dataValues.Add(currentValue);
                        if (minValue == null)
                        {
                            minValue = currentValue;
                        }
                        else
                        {
                            if (currentValue.IndependentValue < minValue.IndependentValue)
                            {
                                minValue = currentValue;
                            }
                        }
                        if (maxValue == null)
                        {
                            maxValue = currentValue;
                        }
                        else
                        {
                            if (currentValue.IndependentValue > maxValue.IndependentValue)
                            {
                                maxValue = currentValue;
                            }
                        }
                    }

                    StatisticsRepository.LinearRegression linearRegression = new StatisticsRepository.LinearRegression();
                    Dictionary<string, string> inputVariableList = new Dictionary<string, string>();
                    inputVariableList.Add(yAxisVar, "dependvar");
                    inputVariableList.Add("intercept", "true");
                    inputVariableList.Add("includemissing", "false");
                    inputVariableList.Add("p", "0.95");
                    inputVariableList.Add(xAxisVar, "unsorted");

                    StatisticsRepository.LinearRegression.LinearRegressionResults regresResults = linearRegression.LinearRegression(inputVariableList, regressTable);

                    object[] result = new object[] { dataValues, regresResults, maxValue, minValue };
                    e.Result = result;

                    this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));
                }
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
            }
        }

        private void GenerateSingleChart(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (worker != null && worker.WorkerSupportsCancellation)
            {
                worker.CancelAsync();
            }

            //waitCursor.Visibility = System.Windows.Visibility.Visible;

            //if (cbxChartSize.SelectedIndex > -1)
            //{
            //    GadgetOptions.InputVariableList.Add("chartsize", cbxChartSize.SelectedItem.ToString());                
            //}

            //GadgetOptions.MainVariableName = cbxSingleField.SelectedItem.ToString();
            //if (cbxWeightField.SelectedIndex > -1)
            //{
            //    GadgetOptions.WeightVariableName = cbxWeightField.SelectedItem.ToString();
            //}
            //if (cbxStrataField.SelectedIndex > -1)
            //{
            //    List<string> strataVars = new List<string>();
            //    strataVars.Add(cbxStrataField.SelectedItem.ToString());
            //    GadgetOptions.StrataVariableNames = strataVars;
            //}

            lock (syncLock)
            {
                //System.ComponentModel.BackgroundWorker singleChartWorker = new System.ComponentModel.BackgroundWorker();
                worker = new System.ComponentModel.BackgroundWorker();
                worker.DoWork += new System.ComponentModel.DoWorkEventHandler(singleChartWorker_DoWork);
                worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(singleChartWorker_RunWorkerCompleted);
                object[] args = new object[2];
                args[0] = GadgetOptions;
                //args[1] = cbxSingleField.SelectedItem.ToString();
                worker.RunWorkerAsync(args);
            }
        }

        private void RenderFinishScatterChart(List<NumericDataValue> dataValues, StatisticsRepository.LinearRegression.LinearRegressionResults results, NumericDataValue maxValue, NumericDataValue minValue)
        {
            Chart chart = new Chart();
            chart.Loaded += new RoutedEventHandler(chart_Loaded);
            chart.BorderThickness = new Thickness(0); 
            ScatterSeries series = new ScatterSeries();            

            LinearAxis xaxis = new LinearAxis();
            xaxis.Orientation = AxisOrientation.X;
            xaxis.Title = cbxScatterXAxisField.SelectedItem.ToString();
            series.IndependentAxis = xaxis;

            LinearAxis yaxis = new LinearAxis();
            yaxis.Orientation = AxisOrientation.Y;
            yaxis.Title = cbxScatterYAxisField.SelectedItem.ToString();
            series.DependentRangeAxis = yaxis;
            yaxis.ShowGridLines = (bool)checkboxShowHorizontalGridLines.IsChecked;

            series.IndependentValuePath = "IndependentValue";
            series.DependentValuePath = "DependentValue";
            series.ItemsSource = dataValues;
            chart.Series.Add(series);

            if (results.variables != null)
            {
                decimal coefficient = Convert.ToDecimal(results.variables[0].coefficient);
                decimal constant = Convert.ToDecimal(results.variables[1].coefficient);

                NumericDataValue newMaxValue = new NumericDataValue();
                newMaxValue.IndependentValue = maxValue.IndependentValue + 1;
                newMaxValue.DependentValue = (coefficient * maxValue.IndependentValue) + constant;
                NumericDataValue newMinValue = new NumericDataValue();
                newMinValue.IndependentValue = minValue.IndependentValue - 1;
                newMinValue.DependentValue = (coefficient * minValue.IndependentValue) + constant;

                List<NumericDataValue> regresValues = new List<NumericDataValue>();
                regresValues.Add(newMinValue);
                regresValues.Add(newMaxValue);

                LineSeries regression = new LineSeries();
                regression.DependentRangeAxis = yaxis;
                regression.IndependentAxis = xaxis;
                regression.IndependentValuePath = "IndependentValue";
                regression.DependentValuePath = "DependentValue";
                regression.ItemsSource = regresValues;
                chart.Series.Add(regression);

                series.LegendItems.Clear();
                regression.LegendItems.Clear();

                chart.Height = 600;
                if (dataValues.Count > 20)
                    chart.Width = dataValues.Count * 25;
                else
                    chart.Width = 800;
                pnlMain.Children.Clear();
                pnlMain.Children.Add(chart);
            }
            else
            {
                pnlMain.Children.Clear();
                RenderFinishWithWarning("Insufficient data to produce this chart.");
            }
            //RenderFinish();
        }

        private void RenderFinishSingleChart(List<List<StringDataValue>> stratifiedValues)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;
            pnlMain.Children.Clear();

            Chart chart = new Chart();
            chart.Loaded += new RoutedEventHandler(chart_Loaded);
            chart.BorderThickness = new Thickness(0);                        
            LinearAxis dependentAxis = new LinearAxis();            
            dependentAxis.Minimum = 0;
            dependentAxis.ShowGridLines = (bool)checkboxShowHorizontalGridLines.IsChecked;

            CategoryAxis independentAxis = new CategoryAxis();
            independentAxis.Orientation = AxisOrientation.X;
            
            foreach (List<StringDataValue> dataValues in stratifiedValues)
            {
                DataPointSeries series;
                LineSeries avgSeries = null;                

                switch (((ComboBoxItem)cbxChartType.SelectedItem).Content.ToString())
                {
                    case "Bar":
                        series = new BarSeries();
                        dependentAxis.Orientation = AxisOrientation.X;
                        ((BarSeries)series).DependentRangeAxis = dependentAxis;

                        chart.PlotAreaStyle = new Style();
                        chart.PlotAreaStyle.Setters.Add(new Setter(Chart.BackgroundProperty, Brushes.White));

                        dependentAxis.GridLineStyle = new Style();
                        dependentAxis.GridLineStyle.Setters.Add(new Setter(Line.StrokeProperty, Brushes.LightGray));

                        chart.Palette = (System.Collections.ObjectModel.Collection<ResourceDictionary>)this.Resources["defaultBarPalette"];

                        break;
                    case "Column":                    
                        series = new ColumnSeries();
                        if (DashboardHelper.IsColumnText(cbxSingleField.SelectedItem.ToString()) && axisLabelMaxLength > 5)
                            independentAxis.AxisLabelStyle = Resources["RotateAxisStyle90"] as Style;

                        ((ColumnSeries)series).IndependentAxis = independentAxis;
                        dependentAxis.Orientation = AxisOrientation.Y;
                        ((ColumnSeries)series).DependentRangeAxis = dependentAxis;   
                     
                        chart.PlotAreaStyle = new Style();
                        chart.PlotAreaStyle.Setters.Add(new Setter(Chart.BackgroundProperty, Brushes.White));

                        dependentAxis.GridLineStyle = new Style();
                        dependentAxis.GridLineStyle.Setters.Add(new Setter(Line.StrokeProperty, Brushes.LightGray));

                        chart.Palette = (System.Collections.ObjectModel.Collection<ResourceDictionary>)this.Resources["defaultColumnPalette"];

                        //series.Effect = this.Resources["shadowEffect"] as System.Windows.Media.Effects.DropShadowEffect;

                        dependentAxis.GridLineStyle.Setters.Add(new Setter(Line.StrokeProperty, Brushes.LightGray));
                        //<Setter Property="Effect" Value="{StaticResource shadowEffect}"/>-->

                        break;
                    case "Pareto":
                        series = new ColumnSeries();
                        if (DashboardHelper.IsColumnText(cbxSingleField.SelectedItem.ToString()) && axisLabelMaxLength > 5)
                            independentAxis.AxisLabelStyle = Resources["RotateAxisStyle90"] as Style;
                        ((ColumnSeries)series).IndependentAxis = independentAxis;
                        dependentAxis.Orientation = AxisOrientation.Y;
                        ((ColumnSeries)series).DependentRangeAxis = dependentAxis;                        

                        LinearAxis percentAxis = new LinearAxis();
                        percentAxis.Minimum = 0;
                        percentAxis.Maximum = 100;
                        percentAxis.ShowGridLines = (bool)checkboxShowHorizontalGridLines.IsChecked;
                        percentAxis.Orientation = AxisOrientation.Y;
                        percentAxis.Location = AxisLocation.Right;

                        avgSeries = new LineSeries();
                        avgSeries.IndependentValuePath = "IndependentValue";
                        avgSeries.DependentValuePath = "CurrentMeanValue";                        
                        avgSeries.Title = "Accumulated %";
                        avgSeries.DependentRangeAxis = percentAxis;
                        avgSeries.IndependentAxis = independentAxis;
                        avgSeries.PolylineStyle = Resources["GooglePolylineStyle"] as Style;
                        avgSeries.DataPointStyle = Resources["GoogleDataPointStyle"] as Style;

                        chart.PlotAreaStyle = new Style();
                        chart.PlotAreaStyle.Setters.Add(new Setter(Chart.BackgroundProperty, Brushes.White));

                        dependentAxis.GridLineStyle = new Style();
                        dependentAxis.GridLineStyle.Setters.Add(new Setter(Line.StrokeProperty, Brushes.LightGray));

                        //series.Effect = this.Resources["shadowEffect"] as System.Windows.Media.Effects.DropShadowEffect;

                        chart.Palette = (System.Collections.ObjectModel.Collection<ResourceDictionary>)this.Resources["defaultColumnPalette"];

                        break;
                    case "Line":
                        series = new LineSeries();
                        if (DashboardHelper.IsColumnText(cbxSingleField.SelectedItem.ToString()) && axisLabelMaxLength > 5)
                            independentAxis.AxisLabelStyle = Resources["RotateAxisStyle90"] as Style;
                        ((LineSeries)series).IndependentAxis = independentAxis;
                        dependentAxis.Orientation = AxisOrientation.Y;
                        ((LineSeries)series).DependentRangeAxis = dependentAxis;
                        break;
                    case "Pie":
                        if (stratifiedValues.Count > 1)
                        {
                            if (dataValues.Count > 0)
                            {
                                Label lblStrata = new Label();
                                lblStrata.Margin = new Thickness(0, 10, 0, 0);
                                lblStrata.FontWeight = FontWeights.Bold;
                                lblStrata.Content = dataValues[0].StratificationValue;
                                pnlMain.Children.Add(lblStrata);
                            }
                        }
                        chart = new LabeledPieChart();
                        chart.Loaded += new RoutedEventHandler(chart_Loaded);
                        chart.BorderThickness = new Thickness(0);                        
                        series = new LabeledPieSeries();                        
                        series.LegendItemStyle = Resources["PieLegendStyle"] as Style;                        
                        ((LabeledPieSeries)series).PieChartLabelStyle=Resources["pieChartLabelStyle"] as Style;
					    ((LabeledPieSeries)series).PieChartLabelItemTemplate=Resources["pieChartLabelDataTemplate"] as DataTemplate;
                        ((LabeledPieSeries)series).LabelDisplayMode = DisplayMode.Auto;
                        chart.LegendTitle = cbxSingleField.SelectedItem.ToString();
                        chart.Series.Add(series);
                        chart.Height = 600;
                        chart.Width = 920;

                        chart.Margin = new Thickness(50, 0, 0, 0);

                        chart.PlotAreaStyle = new Style();
                        chart.PlotAreaStyle.Setters.Add(new Setter(Chart.BackgroundProperty, Brushes.White));

                        dependentAxis.GridLineStyle = new Style();
                        dependentAxis.GridLineStyle.Setters.Add(new Setter(Line.StrokeProperty, Brushes.LightGray));

                        chart.Palette = (System.Collections.ObjectModel.Collection<ResourceDictionary>)this.Resources["defaultPiePalette"];

                        pnlMain.Children.Add(chart);
                        break;
                    case "Scatter":
                        series = new ScatterSeries();
                        dependentAxis.Orientation = AxisOrientation.Y;
                        ((ScatterSeries)series).DependentRangeAxis = dependentAxis;
                        break;
                    default:
                        series = new LineSeries();
                        dependentAxis.Orientation = AxisOrientation.Y;
                        ((LineSeries)series).DependentRangeAxis = dependentAxis;
                        break;
                }

                series.IndependentValuePath = "IndependentValue";
                series.DependentValuePath = "DependentValue";

                series.ItemsSource = dataValues;
                if (avgSeries != null)
                {
                    List<StringDataValue> paretoDataValues = new List<StringDataValue>(dataValues);

                    double max = 0;

                    foreach (StringDataValue sdv in paretoDataValues)
                    {
                        max = max + sdv.DependentValue;
                    }

                    double runningPercent = 0;
                    foreach (StringDataValue sdv in paretoDataValues)
                    {
                        sdv.CurrentMeanValue = ((sdv.DependentValue / max) * 100) + runningPercent;
                        runningPercent = sdv.CurrentMeanValue;
                    }

                    avgSeries.ItemsSource = paretoDataValues;
                }

                if (stratifiedValues.Count > 1)
                {
                    if (!(series is LabeledPieSeries) && cbxStrataField.SelectedIndex > -1)
                    {
                        chart.LegendTitle = cbxStrataField.SelectedItem.ToString();
                    }

                    if (dataValues.Count > 0 && !IsBooleanWithNoStratas)
                    {
                        series.Title = dataValues[0].StratificationValue.Split('=')[1].Trim();
                    }
                    else if (dataValues.Count > 0 && IsBooleanWithNoStratas)
                    {
                        series.Title = dataValues[0].StratificationValue.Trim();
                        chart.LegendTitle = dataValues[0].IndependentValue;
                    }
                }
                else
                {
                    series.Title = "Count";
                }

                if (series is LabeledPieSeries)
                {
                    //chart.Height = 600;
                    //chart.Width = 800;
                }
                else if (series is BarSeries)
                {
                    if (dataValues.Count > 30)
                        chart.Height = dataValues.Count * 30;
                    else
                        chart.Height = 600;
                    chart.Width = 800;
                }
                else
                {
                    chart.Height = 600;
                    if (dataValues.Count > 20)
                        chart.Width = dataValues.Count * 40;
                    else
                        chart.Width = 800;
                }
                if (!(series is LabeledPieSeries))
                {
                    chart.Series.Add(series);
                    if (avgSeries != null)
                    {
                        chart.Series.Add(avgSeries);
                    }
                }

                ///////////////////////////////////////////////////////
                string chartSize = "Medium";

                if (GadgetOptions == null)
                {
                    return;
                }

                if (GadgetOptions.InputVariableList != null && GadgetOptions.InputVariableList.ContainsKey("chartsize"))
                {
                    chartSize = GadgetOptions.InputVariableList["chartsize"];
                }

                switch (chartSize)
                {
                    case "Small":
                        chart.Height = chart.Height / 2;
                        chart.Width = chart.Width / 2;
                        break;
                    case "Medium":
                        chart.Height = chart.Height / 1.5;
                        chart.Width = chart.Width / 1.5;
                        break;
                    case "Custom":
                        if (GadgetOptions.InputVariableList.ContainsKey("chartheight"))
                        {
                            int height;
                            bool success = int.TryParse(GadgetOptions.InputVariableList["chartheight"], out height);
                            if (success)
                            {
                                chart.Height = height;
                            }
                        }
                        if (GadgetOptions.InputVariableList.ContainsKey("chartwidth"))
                        {
                            int width;
                            bool success = int.TryParse(GadgetOptions.InputVariableList["chartwidth"], out width);
                            if (success)
                            {
                                chart.Width = width;
                            }
                        }
                        break;
                }
                ///////////////////////////////////////////////////////
            }
            if (!((ComboBoxItem)cbxChartType.SelectedItem).Content.ToString().Equals("Pie"))
            {
                pnlMain.Children.Add(chart);
            }
        }

        void singleChartWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                List<List<StringDataValue>> stratifiedValues = (List<List<StringDataValue>>)e.Result;
                this.Dispatcher.BeginInvoke(new RenderFinishSingleChartDelegate(RenderFinishSingleChart), stratifiedValues);
                System.Threading.Thread.Sleep(2500);
                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
            }
        }

        void singleChartWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                GadgetParameters GadgetOptions = (GadgetParameters)((object[])e.Argument)[0];
                string varName = GadgetOptions.MainVariableName;//((object[])e.Argument)[1].ToString();
                string singleChartType = string.Empty;
                if (GadgetOptions.InputVariableList.ContainsKey("charttype"))
                {
                    singleChartType = GadgetOptions.InputVariableList["charttype"];
                }

                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));

                RequestUpdateStatusDelegate requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
                CheckForCancellationDelegate checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

                GadgetOptions.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                GadgetOptions.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);

                Dictionary<DataTable, List<DescriptiveStatistics>> res = DashboardHelper.GenerateFrequencyTable(GadgetOptions);

                if (res == null || res.Count == 0)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.NO_RECORDS_SELECTED);
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                    return;
                }
                else if (worker.CancellationPending)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.DASHBOARD_GADGET_STATUS_OPERATION_CANCELLED);
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                    System.Diagnostics.Debug.Print("Single chart thread was cancelled");
                    return;
                }
                else
                {
                    DataTable frequencies = new DataTable();
                    List<List<StringDataValue>> stratifiedValues = new List<List<StringDataValue>>();
                    axisLabelMaxLength = 0;

                    //if (GadgetStatusUpdate != null)
                    //{
                        //this.Dispatcher.BeginInvoke(GadgetStatusUpdate, SharedStrings.DASHBOARD_GADGET_STATUS_DISPLAYING_OUTPUT);
                        System.Threading.Thread.Sleep(150); // necessary to prevent this message from overriding the 'no records selected'
                    //}

                        if (res.Count == 1 && !singleChartType.Equals("pie") && !singleChartType.Equals("pareto") && DashboardHelper.IsUsingEpiProject && DashboardHelper.View.Fields.Contains(GadgetOptions.MainVariableName) && (DashboardHelper.View.Fields[GadgetOptions.MainVariableName] is YesNoField || DashboardHelper.View.Fields[GadgetOptions.MainVariableName] is CheckBoxField))
                    {
                        IsBooleanWithNoStratas = true;
                    }
                    else
                    {
                        IsBooleanWithNoStratas = false;
                    }

                    foreach (KeyValuePair<DataTable, List<DescriptiveStatistics>> tableKvp in res)
                    {
                        string strataValue = tableKvp.Key.TableName;

                        double count = 0;
                        foreach (DescriptiveStatistics ds in tableKvp.Value)
                        {
                            count = count + ds.observations;
                        }

                        if (count == 0)
                        {
                            continue;
                        }

                        frequencies = tableKvp.Key;

                        List<StringDataValue> dataValues = new List<StringDataValue>();
                        int maxLegendItems = 15;
                        object[] itemArray = null;

                        if (singleChartType.Equals("pie") && !frequencies.Columns[0].DataType.ToString().Equals("System.DateTime") && !frequencies.Columns[0].DataType.ToString().Equals("System.Double") && !frequencies.Columns[0].DataType.ToString().Equals("System.Single") && !frequencies.Columns[0].DataType.ToString().Equals("System.Int32"))
                        {
                            int rowStoppedAt = 0;
                            DataTable condensedTable = new DataTable(frequencies.TableName);
                            condensedTable = frequencies.Clone();

                            for (int i = 0; i < frequencies.Rows.Count; i++)
                            {
                                if (i < maxLegendItems)
                                {
                                    condensedTable.Rows.Add(frequencies.Rows[i].ItemArray);
                                }
                                else if (i == maxLegendItems)
                                {                                    
                                    condensedTable.Rows.Add("All others", frequencies.Rows[i][1]);
                                    rowStoppedAt = i;
                                }
                                else
                                {
                                    condensedTable.Rows[maxLegendItems][1] = (double)condensedTable.Rows[rowStoppedAt][1] + (double)frequencies.Rows[i][1];
                                }                        
                            }

                            if (condensedTable.Rows.Count > maxLegendItems)
                            {
                                itemArray = condensedTable.Rows[maxLegendItems].ItemArray;
                                DataRow rowToRemove = condensedTable.Rows[maxLegendItems];
                                condensedTable.Rows.Remove(rowToRemove);
                            }

                            frequencies = condensedTable.Copy();

                            if (!DashboardHelper.IsColumnYesNo(GadgetOptions.MainVariableName) && !DashboardHelper.IsColumnBoolean(GadgetOptions.MainVariableName))
                            {
                                frequencies = frequencies.Select(string.Empty, frequencies.Columns[0].ColumnName).CopyToDataTable().DefaultView.ToTable(frequencies.TableName);
                            }

                            if (itemArray != null)
                            {
                                frequencies.Rows.Add(itemArray);
                            }
                        }

                        double sum = 0;
                        double runningTotal = 0;
                        double maxValue = 0;

                        foreach (DataRow row in frequencies.Rows)
                        {
                            double value = (double)row["freq"];
                            sum = sum + value;
                            if (value > maxValue)
                            {
                                maxValue = value;
                            }
                        }

                        if (IsBooleanWithNoStratas)
                        {
                            foreach (DataRow row in frequencies.Rows)
                            {
                                runningTotal = runningTotal + (double)row["freq"];
                                double currentTotal = (runningTotal / sum) * maxValue;
                                StringDataValue sdv = new StringDataValue() { DependentValue = (double)row["freq"], IndependentValue = strataValue, StratificationValue = row[varName].ToString(), CurrentMeanValue = currentTotal };
                                List<StringDataValue> listSdv = new List<StringDataValue>();
                                listSdv.Add(sdv);
                                //    dataValues.Add(new StringDataValue() { DependentValue = (double)row["freq"], IndependentValue = row[varName].ToString(), StratificationValue = string.Empty });
                                if (row[varName].ToString().Length > axisLabelMaxLength)
                                {
                                    axisLabelMaxLength = row[varName].ToString().Length;
                                }
                                stratifiedValues.Add(listSdv);                                
                            }                            
                        }
                        else
                        {
                            foreach (DataRow row in frequencies.Rows)
                            {
                                runningTotal = runningTotal + (double)row["freq"];
                                double currentTotal = (runningTotal / sum) * maxValue;
                                dataValues.Add(new StringDataValue() { DependentValue = (double)row["freq"], IndependentValue = row[varName].ToString(), StratificationValue = strataValue, CurrentMeanValue = currentTotal });
                                if (row[varName].ToString().Length > axisLabelMaxLength)
                                {
                                    axisLabelMaxLength = row[varName].ToString().Length;
                                }                                
                            }
                            stratifiedValues.Add(dataValues);                            
                        }
                    }
                    e.Result = stratifiedValues;

                    if (stratifiedValues.Count == 0)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.NO_RECORDS_SELECTED);
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));
                    }                    
                }
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);                
            }
        }

        /// <summary>
        /// Sets the gadget to its 'processing' state
        /// </summary>
        public override void SetGadgetToProcessingState()
        {
            cbxCaseStatusField.IsEnabled = false;
            cbxDateInterval.IsEnabled = false;
            cbxChartSize.IsEnabled = false;
            cbxChartType.IsEnabled = false;
            cbxColumnAggregateFunc.IsEnabled = false;
            cbxColumnXAxisField.IsEnabled = false;
            cbxColumnYAxisField.IsEnabled = false;
            cbxDateField.IsEnabled = false;
            cbxScatterXAxisField.IsEnabled = false;
            cbxScatterYAxisField.IsEnabled = false;
            cbxSingleField.IsEnabled = false;
            cbxStrataField.IsEnabled = false;
            cbxWeightField.IsEnabled = false;
            txtXAxisEndValue.IsEnabled = false;
            txtXAxisStartValue.IsEnabled = false;
            cbxXAxisLabelRotation.IsEnabled = false;
            btnRun.IsEnabled = false;

            this.IsProcessing = true;
        }

        /// <summary>
        /// Sets the gadget to its 'finished' state
        /// </summary>
        public override void SetGadgetToFinishedState()
        {
            cbxCaseStatusField.IsEnabled = true;
            cbxDateInterval.IsEnabled = true;
            cbxChartSize.IsEnabled = true;
            cbxChartType.IsEnabled = true;
            cbxColumnAggregateFunc.IsEnabled = true;
            cbxColumnXAxisField.IsEnabled = true;
            cbxColumnYAxisField.IsEnabled = true;
            cbxDateField.IsEnabled = true;
            cbxScatterXAxisField.IsEnabled = true;
            cbxScatterYAxisField.IsEnabled = true;
            cbxSingleField.IsEnabled = true;
            cbxStrataField.IsEnabled = true;
            cbxWeightField.IsEnabled = true;
            txtXAxisEndValue.IsEnabled = true;
            txtXAxisStartValue.IsEnabled = true;
            cbxXAxisLabelRotation.IsEnabled = true;
            btnRun.IsEnabled = true;

            this.IsProcessing = false;

            base.SetGadgetToFinishedState();
        }

        /// <summary>
        /// Serializes the gadget into Xml
        /// </summary>
        /// <param name="doc">The Xml docment</param>
        /// <returns>XmlNode</returns>
        public override XmlNode Serialize(XmlDocument doc)
        {
            string caseStatusField = string.Empty;
            string dateField = string.Empty;
            string singleField = string.Empty;
            string weightField = string.Empty;
            string strataField = string.Empty;
            string columnAggregateFunction = string.Empty;
            string xAxisField = string.Empty;
            string yAxisField = string.Empty;
            string xAxisFieldScatter = string.Empty;
            string yAxisFieldScatter = string.Empty;
            string chartType = string.Empty;
            string chartSize = "Medium";
            string xAxisRotation = "90";

            if (cbxXAxisLabelRotation.SelectedIndex == 0)
            {
                xAxisRotation = "0";
            }
            else if (cbxXAxisLabelRotation.SelectedIndex == 1)
            {
                xAxisRotation = "45";
            }

            if (cbxChartSize.SelectedIndex >= 0)
            {
                chartSize = cbxChartSize.SelectedItem.ToString();
            }

            // chart type should come first
            if (cbxChartType.SelectedIndex > -1)
            {
                chartType = cbxChartType.Text;
            }            
            if (cbxCaseStatusField.SelectedIndex > -1)
            {
                caseStatusField = cbxCaseStatusField.SelectedItem.ToString().Replace("<", "&lt;");
            }
            if (cbxDateField.SelectedIndex > -1)
            {
                dateField = cbxDateField.SelectedItem.ToString().Replace("<", "&lt;");
            }
            if (cbxColumnXAxisField.SelectedIndex > -1)
            {
                xAxisField = cbxColumnXAxisField.SelectedItem.ToString().Replace("<", "&lt;");
            }
            if (cbxColumnYAxisField.SelectedIndex > -1)
            {
                yAxisField = cbxColumnYAxisField.SelectedItem.ToString().Replace("<", "&lt;");
            }
            if (cbxSingleField.SelectedIndex > -1)
            {
                singleField = cbxSingleField.SelectedItem.ToString().Replace("<", "&lt;");
            }
            if (cbxScatterXAxisField.SelectedIndex > -1)
            {
                xAxisFieldScatter = cbxScatterXAxisField.SelectedItem.ToString().Replace("<", "&lt;");
            }
            if (cbxScatterYAxisField.SelectedIndex > -1)
            {
                yAxisFieldScatter = cbxScatterYAxisField.SelectedItem.ToString().Replace("<", "&lt;");
            }
            if (cbxWeightField.SelectedIndex > -1)
            {
                weightField = cbxWeightField.SelectedItem.ToString().Replace("<", "&lt;");
            }
            if (cbxStrataField.SelectedIndex > -1)
            {
                strataField = cbxStrataField.SelectedItem.ToString().Replace("<", "&lt;");
            }
            if (cbxColumnAggregateFunc.SelectedIndex > -1)
            {
                columnAggregateFunction = cbxColumnAggregateFunc.Text;
            }

            string xmlString =
            "<chartType>" + chartType + "</chartType>" +
            "<chartSize>" + chartSize + "</chartSize>" +
            "<chartHeight>" + txtCustomHeight.Text + "</chartHeight>" +
            "<chartWidth>" + txtCustomWidth.Text + "</chartWidth>" +
            "<chartTitle>" + ChartTitle + "</chartTitle>" +
            "<chartLegendTitle>" + LegendTitle + "</chartLegendTitle>" +
            "<columnAggregateFunction>" + columnAggregateFunction + "</columnAggregateFunction>" +
            "<caseStatusVariable>" + caseStatusField + "</caseStatusVariable>" +
            "<dateVariable>" + dateField + "</dateVariable>" +
            "<dateInterval>" + cbxDateInterval.Text + "</dateInterval>" +
            "<singleVariable>" + singleField + "</singleVariable>" +            
            "<weightVariable>" + weightField + "</weightVariable>" +
            "<strataVariable>" + strataField + "</strataVariable>" +
            "<yAxisVariable>" + yAxisField + "</yAxisVariable>" +
            "<xAxisVariable>" + xAxisField + "</xAxisVariable>" +
            "<yAxisScatterVariable>" + yAxisFieldScatter + "</yAxisScatterVariable>" +
            "<xAxisScatterVariable>" + xAxisFieldScatter + "</xAxisScatterVariable>" +
            "<yAxisLabel>" + YAxisLabel + "</yAxisLabel>" +
            "<xAxisLabel>" + XAxisLabel + "</xAxisLabel>" +
            "<xAxisRotation>" + xAxisRotation + "</xAxisRotation>" +            
            "<xAxisStartValue>" + txtXAxisStartValue.Text + "</xAxisStartValue>" +
            "<xAxisEndValue>" + txtXAxisEndValue.Text + "</xAxisEndValue>" +
            "<allValues>" + checkboxAllValues.IsChecked.ToString() + "</allValues>" +
            "<horizontalGridLines>" + checkboxShowHorizontalGridLines.IsChecked.ToString() + "</horizontalGridLines>" +
            "<customHeading>" + CustomOutputHeading + "</customHeading>" +
            "<customDescription>" + CustomOutputDescription + "</customDescription>" +
            "<customCaption>" + CustomOutputCaption + "</customCaption>";

            System.Xml.XmlElement element = doc.CreateElement("chartGadget");
            element.InnerXml = xmlString;

            System.Xml.XmlAttribute locationY = doc.CreateAttribute("top");
            System.Xml.XmlAttribute locationX = doc.CreateAttribute("left");
            System.Xml.XmlAttribute collapsed = doc.CreateAttribute("collapsed");
            System.Xml.XmlAttribute type = doc.CreateAttribute("gadgetType");

            locationY.Value = Canvas.GetTop(this).ToString("F0");
            locationX.Value = Canvas.GetLeft(this).ToString("F0");
            collapsed.Value = "false"; // currently no way to collapse the gadget, so leave this 'false' for now
            type.Value = "EpiDashboard.ChartControl";

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);

            return element;
        }

        /// <summary>
        /// Creates the charting gadget from an Xml element
        /// </summary>
        /// <param name="element">The element from which to create the gadget</param>
        public override void CreateFromXml(XmlElement element)
        {
            this.loadingCombos = true;

            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLower())
                {
                    case "casestatusvariable":
                        cbxCaseStatusField.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "datevariable":
                        cbxDateField.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "dateinterval":
                        cbxDateInterval.Text = child.InnerText;
                        break;
                    case "yaxisvariable":
                        cbxColumnYAxisField.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "xaxisvariable":
                        cbxColumnXAxisField.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "yaxisscattervariable":
                        cbxScatterYAxisField.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "xaxisscattervariable":
                        cbxScatterXAxisField.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "allvalues":
                        if (child.InnerText.ToLower().Equals("true"))
                        {
                            checkboxAllValues.IsChecked = true;
                        }
                        else
                        {
                            checkboxAllValues.IsChecked = false;
                        }
                        break;
                    case "horizontalgridlines":
                        if (child.InnerText.ToLower().Equals("true"))
                        {
                            checkboxShowHorizontalGridLines.IsChecked = true;
                        }
                        else
                        {
                            checkboxShowHorizontalGridLines.IsChecked = false;
                        }
                        break;
                    case "singlevariable":
                        cbxSingleField.Text = child.InnerText.Replace("&lt;", "<");
                        cbxSingleField.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "weightvariable":
                        cbxWeightField.Text = child.InnerText.Replace("&lt;", "<");
                        cbxWeightField.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "stratavariable":
                        cbxStrataField.Text = child.InnerText.Replace("&lt;", "<");
                        cbxStrataField.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "columnaggregatefunction":
                        cbxColumnAggregateFunc.Text = child.InnerText;
                        break;
                    case "charttype":
                        cbxChartType.Text = child.InnerText;
                        break;
                    case "customheading":
                        this.CustomOutputHeading = child.InnerText;
                        break;
                    case "customdescription":
                        this.CustomOutputDescription = child.InnerText;
                        break;
                    case "customcaption":
                        this.CustomOutputCaption = child.InnerText;
                        break;
                    case "chartsize":
                        cbxChartSize.SelectedItem = child.InnerText;
                        break;
                    case "chartwidth":
                        txtCustomWidth.Text = child.InnerText;
                        break;
                    case "chartheight":
                        txtCustomHeight.Text = child.InnerText;
                        break;
                    case "charttitle":
                        ChartTitle = child.InnerText;
                        break;
                    case "chartlegendtitle":
                        LegendTitle = child.InnerText;
                        break;
                    case "xaxislabel":
                        XAxisLabel = child.InnerText;
                        break;
                    case "yaxislabel":
                        YAxisLabel = child.InnerText;
                        break;
                    case "xaxisstartvalue":
                        txtXAxisStartValue.Text = child.InnerText;
                        break;
                    case "xaxisendvalue":
                        txtXAxisEndValue.Text = child.InnerText;
                        break;
                    case "xaxisrotation":
                        switch (child.InnerText)
                        {
                            case "0":
                                cbxXAxisLabelRotation.SelectedIndex = 0;
                                break;
                            case "45":
                                cbxXAxisLabelRotation.SelectedIndex = 1;
                                break;
                            default:
                                cbxXAxisLabelRotation.SelectedIndex = 2;
                                break;
                        }                        
                        break;
                }
            }

            foreach (XmlAttribute attribute in element.Attributes)
            {
                switch (attribute.Name.ToLower())
                {
                    case "top":
                        Canvas.SetTop(this, double.Parse(attribute.Value));
                        break;
                    case "left":
                        Canvas.SetLeft(this, double.Parse(attribute.Value));
                        break;
                }
            }

            this.loadingCombos = false;

            RefreshResults();
            HideConfigPanel();
        }

        /// <summary>
        /// Returns the gadget's description as a string.
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return "Chart Gadget";
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0, bool useAlternatingColors = false)
        {
            // Check to see if a chart has been created.
            if (pnlMain.ActualHeight == 0 || pnlMain.ActualWidth == 0)
            {
                return string.Empty;
            }

            StringBuilder htmlBuilder = new StringBuilder();

            if (string.IsNullOrEmpty(CustomOutputHeading))
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">Chart</h2>");
            }
            else if (CustomOutputHeading != "(none)")
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">" + CustomOutputHeading + "</h2>");
            }

            if (!string.IsNullOrEmpty(CustomOutputDescription))
            {
                htmlBuilder.AppendLine("<p class=\"gadgetsummary\">" + CustomOutputDescription + "</p>");
            }

            string imageFileName = string.Empty;

            if(htmlFileName.EndsWith(".html")) 
            {
                imageFileName = htmlFileName.Remove(htmlFileName.Length - 5, 5);
            }
            else if (htmlFileName.EndsWith(".htm"))
            {
                imageFileName = htmlFileName.Remove(htmlFileName.Length - 4, 4);
            }

            imageFileName = imageFileName + "_" + count.ToString() + ".png";
            
            BitmapSource img = (BitmapSource)Common.ToImageSource(pnlMain);
            FileStream stream = new FileStream(imageFileName, FileMode.Create);
            //JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(img));
            encoder.Save(stream);
            stream.Close();

            htmlBuilder.AppendLine("<img src=\"" + imageFileName + "\" />");

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

        public string ChartTitle
        {
            get
            {
                return this.chartTitle;
            }
            set
            {
                this.chartTitle = value;
            }
        }

        public string LegendTitle
        {
            get
            {
                return this.legendTitle;
            }
            set
            {
                this.legendTitle = value;
            }
        }

        public string XAxisLabel
        {
            get
            {
                return this.xAxisLabel;
            }
            set
            {
                this.xAxisLabel = value;
            }
        }

        public string YAxisLabel
        {
            get
            {
                return this.yAxisLabel;
            }
            set
            {
                this.yAxisLabel = value;
            }
        }

        #endregion

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            RefreshResults();
        }

        public bool DrawBorders { get; set; } 
    }

    public class PieData
    {
        public object IndependentValue { get; set; }
        public object DependentValue { get; set; }
    }

}
