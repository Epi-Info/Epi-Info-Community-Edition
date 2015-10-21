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
using EpiDashboard;
using EpiDashboard.Controls;
using EpiDashboard.Gadgets;
using EpiDashboard.Gadgets.Charting;
using ComponentArt.Win.DataVisualization;
using ComponentArt.Win.DataVisualization.Common;
using ComponentArt.Win.DataVisualization.Charting;


namespace EpiDashboard.Gadgets.Charting
{
    /// <summary>
    /// Interaction logic for HistogramChartGadget.xaml
    /// </summary>
    public partial class HistogramChartGadget : HistogramChartGadgetBase
    {
        #region Constructors
        public HistogramChartGadget()
        {
            InitializeComponent();
            Construct();
        }

        public HistogramChartGadget(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            Construct();
        }

        #endregion //Constructors

        #region Properties

        //MOVED TO CHART PROPERTIES
        DateTime? StartDate { get; set; }
        DateTime? EndDate { get; set; }
        System.Drawing.Font AxisLabelFont { get; set; }
        System.Drawing.Font ChartTitleFont { get; set; }
        System.Drawing.Font ChartSubtitleFont { get; set; }

        /// <summary>
        /// A custom heading to use for this gadget's output
        /// </summary>
        private string customOutputHeading;

        /// <summary>
        /// A custom description to use for this gadget's output
        /// </summary>
        private string customOutputDescription;

        #endregion //Properties

        #region Private and Protected Methods

        protected override void Construct()
        {
            this.Parameters = new HistogramChartParameters();

            if (!string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)"))
            {
                headerPanel.Text = CustomOutputHeading;
            }

            StrataGridList = new List<Grid>();
            StrataExpanderList = new List<Expander>();

            mnuCopy.Click += new RoutedEventHandler(mnuCopy_Click);
            mnuSendDataToHTML.Click += new RoutedEventHandler(mnuSendDataToHTML_Click);

            mnuSendToBack.Click += new RoutedEventHandler(mnuSendToBack_Click);
            mnuClose.Click += new RoutedEventHandler(mnuClose_Click);
            this.IsProcessing = false;
            this.GadgetStatusUpdate += new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(IsCancelled);

            base.Construct();
        }

        private void ClearResults()
        {
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Text = string.Empty;
            descriptionPanel.PanelMode = EpiDashboard.Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;

            panelMain.Children.Clear();

            StrataGridList.Clear();
            StrataExpanderList.Clear();
        }

        protected override void RenderFinish()
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.StatusPanel;
            messagePanel.Text = string.Empty;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        protected override void RenderFinishWithWarning(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed; //waitCursor.Visibility = Visibility.Hidden;

            messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.WarningPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        protected override void RenderFinishWithError(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.ErrorPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            panelMain.Children.Clear();

            HideConfigPanel();
            CheckAndSetPosition();
        }

        protected override void SetChartData(List<XYColumnChartData> dataList, Strata strata)
        {
            #region Input Validation
            if (dataList == null)
            {
                throw new ArgumentNullException("dataList");
            }
            #endregion // Input Validation
            HistogramChartParameters chtParameters = (HistogramChartParameters)Parameters;

            if (dataList.Count > 0)
            {
                if (strata != null)
                {
                    chtParameters.ChartStrataTitle = strata.Filter;
                }
                else
                {
                    chtParameters.ChartStrataTitle = String.Empty;
                }
                EpiDashboard.Controls.Charting.HistogramChart columnChart = new EpiDashboard.Controls.Charting.HistogramChart(DashboardHelper, chtParameters, dataList);
                columnChart.Margin = new Thickness(0, 0, 0, 16);
                columnChart.MouseEnter += new MouseEventHandler(chart_MouseEnter);
                columnChart.MouseLeave += new MouseEventHandler(chart_MouseLeave);
                panelMain.Children.Add(columnChart);
            }
        }

        private void CalculateByMinute(DataTable sourceDt, DataTable curveDt, int step)
        {
            DateTime startDt = (DateTime)(sourceDt.Rows[0][0]);
            startDt = new DateTime(startDt.Year, startDt.Month, startDt.Day, startDt.Hour, startDt.Minute, 0);

            DateTime endDt = (DateTime)(sourceDt.Rows[sourceDt.Rows.Count - 1][0]);
            endDt = new DateTime(endDt.Year, endDt.Month, endDt.Day, endDt.Hour, endDt.Minute, 59);

            if (StartDate.HasValue)
            {
                startDt = StartDate.Value;
            }

            if (EndDate.HasValue)
            {
                endDt = EndDate.Value;
            }

            DateTime incDt = new DateTime(startDt.Ticks);

            while (incDt < endDt)
            {
                DateTime nextDt = incDt.AddMinutes(step);

                string label = incDt.ToString("d MMM HH:mm:ss");

                int numRowsAdded = 0;

                foreach (DataRow row in sourceDt.Rows)
                {
                    DateTime dt = (DateTime)row[0];
                    if (dt >= incDt && dt < nextDt)
                    {
                        DataRow ecRow = curveDt.Rows.Find(label);
                        if (ecRow != null)
                        {
                            ecRow[1] = (double)ecRow[1] + (double)row[1];
                        }
                        else
                        {
                            curveDt.Rows.Add(label, (double)row[1]);
                            numRowsAdded++;
                        }
                    }
                }

                if (numRowsAdded == 0)
                {
                    curveDt.Rows.Add(label, 0);
                }

                incDt = incDt.AddMinutes(step);
            }
        }

        private void CalculateByHour(DataTable sourceDt, DataTable curveDt, int step)
        {
            DateTime startDt = (DateTime)(sourceDt.Rows[0][0]);
            startDt = new DateTime(startDt.Year, startDt.Month, startDt.Day, startDt.Hour, 0, 0);

            DateTime endDt = (DateTime)(sourceDt.Rows[sourceDt.Rows.Count - 1][0]);
            endDt = new DateTime(endDt.Year, endDt.Month, endDt.Day, endDt.Hour, 59, 59);

            if (StartDate.HasValue)
            {
                startDt = StartDate.Value;
            }

            if (EndDate.HasValue)
            {
                endDt = EndDate.Value;
            }

            DateTime incDt = new DateTime(startDt.Ticks);

            while (incDt < endDt)
            {
                DateTime nextDt = incDt.AddHours(step);

                string label = incDt.ToString("d MMM HH:mm");

                int numRowsAdded = 0;

                foreach (DataRow row in sourceDt.Rows)
                {
                    DateTime dt = (DateTime)row[0];
                    if (dt >= incDt && dt < nextDt)
                    {
                        DataRow ecRow = curveDt.Rows.Find(label);
                        if (ecRow != null)
                        {
                            ecRow[1] = (double)ecRow[1] + (double)row[1];
                        }
                        else
                        {
                            curveDt.Rows.Add(label, (double)row[1]);
                            numRowsAdded++;
                        }
                    }
                }

                if (numRowsAdded == 0)
                {
                    curveDt.Rows.Add(label, 0);
                }

                incDt = incDt.AddHours(step);
            }
        }

        private void CalculateByDay(DataTable sourceDt, DataTable curveDt, int step)
        {
            DateTime startDt = (DateTime)(sourceDt.Rows[0][0]);
            startDt = new DateTime(startDt.Year, startDt.Month, startDt.Day, 0, 0, 0);

            DateTime endDt = (DateTime)(sourceDt.Rows[sourceDt.Rows.Count - 1][0]);
            endDt = new DateTime(endDt.Year, endDt.Month, endDt.Day, 23, 59, 59);

            if (StartDate.HasValue)
            {
                startDt = StartDate.Value;
            }

            if (EndDate.HasValue)
            {
                endDt = EndDate.Value;
            }

            DateTime incDt = new DateTime(startDt.Ticks);

            while (incDt < endDt)
            {
                DateTime nextDt = incDt.AddDays(step);

                string label = incDt.ToString("d MMM yy");

                int numRowsAdded = 0;

                foreach (DataRow row in sourceDt.Rows)
                {
                    DateTime dt = (DateTime)row[0];
                    if (dt >= incDt && dt < nextDt)
                    {
                        DataRow ecRow = curveDt.Rows.Find(label);
                        if (ecRow != null)
                        {
                            ecRow[1] = (double)ecRow[1] + (double)row[1];
                        }
                        else
                        {
                            curveDt.Rows.Add(label, (double)row[1]);
                            numRowsAdded++;
                        }
                    }
                }

                if (numRowsAdded == 0)
                {
                    curveDt.Rows.Add(label, 0);
                }

                incDt = incDt.AddDays(step);
            }
        }

        private void CalculateByMonth(DataTable sourceDt, DataTable curveDt, int step)
        {
            DateTime startDt = (DateTime)(sourceDt.Rows[0][0]);
            startDt = new DateTime(startDt.Year, startDt.Month, 1, 0, 0, 0);

            DateTime endDt = (DateTime)(sourceDt.Rows[sourceDt.Rows.Count - 1][0]);
            endDt = new DateTime(endDt.Year, endDt.Month, endDt.Day, 23, 59, 59);

            if (StartDate.HasValue)
            {
                startDt = StartDate.Value;
            }

            if (EndDate.HasValue)
            {
                endDt = EndDate.Value;
            }

            DateTime incDt = new DateTime(startDt.Ticks);

            if (StartDate.HasValue)
            {
                startDt = StartDate.Value;
            }

            if (EndDate.HasValue)
            {
                endDt = EndDate.Value;
            }

            while (incDt < endDt)
            {
                DateTime nextDt = incDt.AddMonths(step);

                string label = incDt.ToString("MMM yy");

                int numRowsAdded = 0;

                foreach (DataRow row in sourceDt.Rows)
                {
                    DateTime dt = (DateTime)row[0];
                    if (dt >= incDt && dt < nextDt)
                    {
                        DataRow ecRow = curveDt.Rows.Find(label);
                        if (ecRow != null)
                        {
                            ecRow[1] = (double)ecRow[1] + (double)row[1];
                        }
                        else
                        {
                            curveDt.Rows.Add(label, (double)row[1]);
                            numRowsAdded++;
                        }
                    }
                }

                if (numRowsAdded == 0)
                {
                    curveDt.Rows.Add(label, 0);
                }

                incDt = incDt.AddMonths(step);
            }
        }

        private void CalculateByYear(DataTable sourceDt, DataTable curveDt, int step)
        {
            DateTime startDt = (DateTime)(sourceDt.Rows[0][0]);
            startDt = new DateTime(startDt.Year, 1, 1, 0, 0, 0);

            DateTime endDt = (DateTime)(sourceDt.Rows[sourceDt.Rows.Count - 1][0]);
            endDt = new DateTime(endDt.Year, endDt.Month, endDt.Day, 23, 59, 59);

            if (StartDate.HasValue)
            {
                startDt = StartDate.Value;
            }

            if (EndDate.HasValue)
            {
                endDt = EndDate.Value;
            }

            DateTime incDt = new DateTime(startDt.Ticks);

            if (StartDate.HasValue)
            {
                startDt = StartDate.Value;
            }

            if (EndDate.HasValue)
            {
                endDt = EndDate.Value;
            }

            while (incDt < endDt)
            {
                DateTime nextDt = incDt.AddYears(step);

                string label = incDt.ToString("yyyy");

                int numRowsAdded = 0;

                foreach (DataRow row in sourceDt.Rows)
                {
                    DateTime dt = (DateTime)row[0];
                    if (dt >= incDt && dt < nextDt)
                    {
                        DataRow ecRow = curveDt.Rows.Find(label);
                        if (ecRow != null)
                        {
                            ecRow[1] = (double)ecRow[1] + (double)row[1];
                        }
                        else
                        {
                            curveDt.Rows.Add(label, (double)row[1]);
                            numRowsAdded++;
                        }
                    }
                }

                if (numRowsAdded == 0)
                {
                    curveDt.Rows.Add(label, 0);
                }

                incDt = incDt.AddYears(step);
            }
        }

        protected override void GenerateChartData(Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables, Strata strata = null)
        {
            lock (syncLockData)
            {
                HistogramChartParameters chtParameters = (HistogramChartParameters)Parameters;
                string second_y_var = string.Empty;

                if (chtParameters.ColumnNames.Count > 2)
                {
                    second_y_var = chtParameters.ColumnNames[1];
                }

                List<XYColumnChartData> dataList = new List<XYColumnChartData>();

                foreach (KeyValuePair<DataTable, List<DescriptiveStatistics>> tableKvp in stratifiedFrequencyTables)
                {
                    double count = 0;
                    foreach (DescriptiveStatistics ds in tableKvp.Value)
                    {
                        count = count + ds.observations;
                    }

                    string strataValue = tableKvp.Key.TableName;
                    DataTable table = tableKvp.Key;

                    string startDate = (chtParameters.StartDate != null) ? Convert.ToString(chtParameters.StartDate) : string.Empty;
                    string endDate = (chtParameters.EndDate != null) ? Convert.ToString(chtParameters.EndDate) : string.Empty;

                    if(!string.IsNullOrEmpty(startDate))
                    {
                        string filterExp = table.Columns[0].ColumnName + " > '" + startDate + "'";
                        table = table.Select(filterExp).CopyToDataTable();
                    }

                    if (!string.IsNullOrEmpty(endDate))
                    {
                        string filterExp = table.Columns[0].ColumnName + " < '" + endDate + "'";
                        table = table.Select(filterExp).CopyToDataTable();
                    }

                    DataTable epiCurveTable = new DataTable("epiCurveTable");
                    epiCurveTable.Columns.Add(new DataColumn(table.Columns[0].ColumnName, typeof(string)));
                    epiCurveTable.Columns.Add(new DataColumn(table.Columns[1].ColumnName, typeof(double)));

                    DataColumn[] parentPrimaryKeyColumns = new DataColumn[1];
                    parentPrimaryKeyColumns[0] = epiCurveTable.Columns[0];
                    epiCurveTable.PrimaryKey = parentPrimaryKeyColumns;

                    int step = 1;
                    int.TryParse(chtParameters.Step.ToString(), out step);

                    string interval = "day";

                    if (!String.IsNullOrEmpty(chtParameters.Interval))
                    {
                        interval = chtParameters.Interval.ToLower();//EI-479
                    }

                    switch (interval)
                    {
                        case "year":
                            CalculateByYear(table, epiCurveTable, step);
                            break;
                        case "minute":
                            CalculateByMinute(table, epiCurveTable, step);
                            break;
                        case "hour":
                            CalculateByHour(table, epiCurveTable, step);
                            break;
                        case "month":
                            CalculateByMonth(table, epiCurveTable, step);
                            break;
                        default:
                        case "day":
                            CalculateByDay(table, epiCurveTable, step);
                            break;
                    }

                    foreach (DataRow row in epiCurveTable.Rows)
                    {
                        XYColumnChartData chartData = new XYColumnChartData();
                        chartData.X = strataValue;
                        chartData.Y = (double)row[1];

                        chartData.S = row[0].ToString();

                        //Debug.Print(chartData.S + " : " + chartData.Y);

                        dataList.Add(chartData);
                    }
                }

                this.Dispatcher.BeginInvoke(new SetChartDataDelegate(SetChartData), dataList, strata);
            }
        }

        private void properties_ChangesAccepted(object sender, EventArgs e)
        {
            Controls.GadgetProperties.HistogramChartProperties properties = Popup.Content as Controls.GadgetProperties.HistogramChartProperties;
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


        #endregion //Private and Protected Methods

        #region Public Methods

        public override void SetGadgetToProcessingState()
        {
            this.IsProcessing = true;
            base.SetGadgetToProcessingState();
        }

        public override void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
            base.SetGadgetToFinishedState();
        }

        public override void RefreshResults()
        {
            HistogramChartParameters chtParameters = (HistogramChartParameters)Parameters;

            if (!LoadingCombos)
            {
                if (chtParameters != null)
                {
                    if (chtParameters.ColumnNames.Count > 0 && !String.IsNullOrEmpty(chtParameters.ColumnNames[0]))
                    {
                        infoPanel.Visibility = System.Windows.Visibility.Collapsed;
                        waitPanel.Visibility = System.Windows.Visibility.Visible;
                        messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.StatusPanel;
                        descriptionPanel.PanelMode = EpiDashboard.Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
                        baseWorker = new BackgroundWorker();
                        baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
                        baseWorker.RunWorkerAsync();
                        base.RefreshResults();
                    }
                    else
                    {
                        ClearResults();
                        waitPanel.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
            }
        }

        public void CreateFromLegacyXml(XmlElement element)
        {
            bool isStacked = false;
            this.Parameters = new HistogramChartParameters();

            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLower())
                {
                    case "casestatusvariable":
                        if (!isStacked) ((HistogramChartParameters)Parameters).StrataVariableNames.Add(child.InnerText.Replace("&lt;", "<"));
                        break;
                    case "datevariable":
                        if (!isStacked && !string.IsNullOrEmpty(child.InnerText.Trim()))
                        {
                            if (this.Parameters.ColumnNames.Count > 0)
                            {
                                ((HistogramChartParameters)Parameters).ColumnNames[0] = (child.InnerText.Replace("&lt;", "<"));
                            }
                            else
                            {
                                ((HistogramChartParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                            }
                        }
                        break;
                    case "yaxisvariable":
                        if (string.IsNullOrEmpty(child.InnerText))
                        {
                            ((HistogramChartParameters)Parameters).StrataVariableNames.Add(child.InnerText.Replace("&lt;", "<"));
                        }
                        break;
                    case "xaxisvariable":
                        if (!string.IsNullOrEmpty(child.InnerText.Trim()))
                        {
                            if (this.Parameters.ColumnNames.Count > 0)
                            {
                                ((HistogramChartParameters)Parameters).ColumnNames[0] = (child.InnerText.Replace("&lt;", "<"));
                            }
                            else
                            {
                                ((HistogramChartParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                            }
                        }
                        break;
                    case "allvalues":
                        if (child.InnerText.ToLower().Equals("true"))
                        {
                            ((HistogramChartParameters)Parameters).ShowAllListValues = true;
                        }
                        else { ((HistogramChartParameters)Parameters).ShowAllListValues = false; }
                        break;
                    case "singlevariable":
                        if (!isStacked)
                        {
                            //cmbField.Text = child.InnerText.Replace("&lt;", "<");
                            if (this.Parameters.ColumnNames.Count > 0)
                            {
                                ((HistogramChartParameters)Parameters).ColumnNames[0] = (child.InnerText.Replace("&lt;", "<"));
                            }
                            else
                            {
                                ((HistogramChartParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                            }
                        }
                        break;
                    case "dateinterval":
                        switch (child.InnerText)
                        {
                            case "Hour":
                                ((HistogramChartParameters)Parameters).Interval = "Hour";
                                break;
                            case "Day":
                                ((HistogramChartParameters)Parameters).Interval = "Day";
                                break;
                            case "Month":
                                ((HistogramChartParameters)Parameters).Interval = "Month";
                                break;
                            case "Year":
                                ((HistogramChartParameters)Parameters).Interval = "Year";
                                break;
                        }
                        break;

                    case "weightvariable":
                        if (!string.IsNullOrEmpty(child.InnerText.Trim()))
                        {
                            ((HistogramChartParameters)Parameters).WeightVariableName = child.InnerText.Replace("&lt;", "<");
                        }
                        break;
                    case "stratavariable":
                        if (!isStacked) ((HistogramChartParameters)Parameters).StrataVariableNames[0] = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "columnaggregatefunction":
                        if (child.InnerText == "Count")
                        {
                            ((HistogramChartParameters)Parameters).Composition = CompositionKind.Stacked;
                        }
                        else
                        {
                            ((HistogramChartParameters)Parameters).Composition = CompositionKind.Stacked100;
                        }
                        break;
                    case "charttype":
                        if (child.InnerText == "Stacked Column")
                        {
                            isStacked = true;
                        }
                        else if (child.InnerText == "Bar")
                        {
                            ((HistogramChartParameters)Parameters).Orientation = Orientation.Horizontal;
                        }
                        break;
                    case "customheading":
                        if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
                        {
                            this.Parameters.GadgetTitle = child.InnerText.Replace("&lt;", "<");
                            this.CustomOutputHeading = child.InnerText.Replace("&lt;", "<");
                        }
                        else
                        {
                            this.CustomOutputHeading = string.Empty;
                            this.Parameters.GadgetTitle = string.Empty;
                        }
                        break;
                    case "customdescription":
                        if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
                        {
                            this.CustomOutputDescription = child.InnerText.Replace("&lt;", "<");
                            this.Parameters.GadgetDescription = child.InnerText.Replace("&lt;", "<");
                            if (!string.IsNullOrEmpty(CustomOutputDescription) && !CustomOutputHeading.Equals("(none)"))
                            {
                                descriptionPanel.Text = CustomOutputDescription;
                                descriptionPanel.PanelMode = EpiDashboard.Controls.GadgetDescriptionPanel.DescriptionPanelMode.DisplayMode;
                            }
                            else
                            {
                                descriptionPanel.PanelMode = EpiDashboard.Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
                            }
                        }
                        break;
                    case "customcaption":
                        this.CustomOutputCaption = child.InnerText;
                        break;
                    case "chartsize":
                        switch (child.InnerText)
                        {
                            case "Small":
                                ((HistogramChartParameters)Parameters).ChartWidth = 400;
                                ((HistogramChartParameters)Parameters).ChartHeight = 250;
                                break;
                            case "Medium":
                                ((HistogramChartParameters)Parameters).ChartWidth = 533;
                                ((HistogramChartParameters)Parameters).ChartHeight = 333;
                                break;
                            case "Large":
                                ((HistogramChartParameters)Parameters).ChartWidth = 800;
                                ((HistogramChartParameters)Parameters).ChartHeight = 500;
                                break;
                            case "Custom":
                                break;
                        }
                        break;
                    case "chartwidth":
                        if (!string.IsNullOrEmpty(child.InnerText))
                        {
                            ((HistogramChartParameters)Parameters).ChartWidth = double.Parse(child.InnerText.Replace("&lt;", "<"));
                        }
                        break;
                    case "chartheight":
                        if (!string.IsNullOrEmpty(child.InnerText))
                        {
                            ((HistogramChartParameters)Parameters).ChartHeight = double.Parse(child.InnerText.Replace("&lt;", "<"));
                        }
                        break;
                    case "charttitle":
                        ((HistogramChartParameters)Parameters).ChartTitle = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "chartlegendtitle":
                        break;
                    case "xaxislabel":
                        ((HistogramChartParameters)Parameters).XAxisLabel = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "yaxislabel":
                        ((HistogramChartParameters)Parameters).YAxisLabel = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "xaxisstartvalue":
                        ((HistogramChartParameters)Parameters).StartDate = Convert.ToDateTime(child.InnerText);
                        break;
                    case "xaxisendvalue":
                        ((HistogramChartParameters)Parameters).EndDate = Convert.ToDateTime(child.InnerText);
                        break;
                    case "xaxisrotation":
                        switch (child.InnerText)
                        {
                            case "0":
                                ((HistogramChartParameters)Parameters).XAxisAngle = 0;
                                break;
                            case "45":
                                ((HistogramChartParameters)Parameters).XAxisAngle = -45;
                                break;
                            case "90":
                                ((HistogramChartParameters)Parameters).XAxisAngle = -90;
                                break;
                            default:
                                ((HistogramChartParameters)Parameters).XAxisAngle = 0;
                                break;
                        }
                        break;
                }
            }
        }

        public override void CreateFromXml(XmlElement element)
        {
            this.LoadingCombos = true;
            HideConfigPanel();
            infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            foreach (XmlAttribute attribute in element.Attributes)
            {
                switch (attribute.Name.ToLower())
                {
                    case "actualheight":
                        string actualHeight = attribute.Value.Replace(',', '.');
                        double controlheight = 0.0;
                        double.TryParse(actualHeight, out controlheight);
                        this.Height = controlheight;
                        break;
                }
            }

            if (element.Name.Equals("chartGadget") || element.Name.Equals("ChartControl"))
            {
                CreateFromLegacyXml(element);
            }
            else
            {
                foreach (XmlElement child in element.ChildNodes)
                {
                    if (!String.IsNullOrEmpty(child.InnerText))
                    {
                        switch (child.Name.ToLower())
                        {
                            case "mainvariable":
                                if (this.Parameters.ColumnNames.Count > 0)
                                {
                                    ((HistogramChartParameters)Parameters).ColumnNames[0] = (child.InnerText.Replace("&lt;", "<"));
                                }
                                else
                                {
                                    ((HistogramChartParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                                }
                                break;
                            case "stratavariable":
                                if (!string.IsNullOrEmpty(child.InnerText))
                                {
                                    if (((HistogramChartParameters)Parameters).StrataVariableNames.Count > 0)
                                    {
                                        ((HistogramChartParameters)Parameters).StrataVariableNames[0] = child.InnerText.Replace("&lt;", "<");
                                    }
                                    else
                                    {
                                        ((HistogramChartParameters)Parameters).StrataVariableNames.Add(child.InnerText.Replace("&lt;", "<"));
                                    }
                                }
                                break;
                            case "stratavariables":
                                foreach (XmlElement field in child.ChildNodes)
                                {
                                    List<string> fields = new List<string>();
                                    if (field.Name.ToLower().Equals("stratavariable"))
                                    {
                                        ((HistogramChartParameters)Parameters).StrataVariableNames.Add(field.InnerText.Replace("&lt;", "<"));
                                    }
                                }
                                break;
                            case "weightvariable":
                                ((HistogramChartParameters)Parameters).WeightVariableName = child.InnerText.Replace("&lt;", "<");
                                break;
                            case "startdate":
                            case "xaxisstartvalue":
                                long ticks; 
                                if ( long.TryParse(child.InnerText.Replace("&lt;", "<") , out ticks))
                                {
                                    ((HistogramChartParameters)Parameters).StartDate = new DateTime(ticks);
                                }
                                else
                                {
                                    ((HistogramChartParameters)Parameters).StartDate = Convert.ToDateTime(child.InnerText.Replace("&lt;", "<"));
                                }
                                
                                break;
                            case "enddate":
                            case "xaxisendvalue":
                                 long ticks2; 
                                if ( long.TryParse(child.InnerText.Replace("&lt;", "<") , out ticks2))
                                {
                                    ((HistogramChartParameters)Parameters).EndDate = new DateTime(ticks2);
                                }
                                else
                                {
                                    ((HistogramChartParameters)Parameters).EndDate = Convert.ToDateTime(child.InnerText.Replace("&lt;", "<"));
                                }
                                //((HistogramChartParameters)Parameters).EndDate = new DateTime(Convert.ToInt64( child.InnerText.Replace("&lt;", "<")));
                                break;
                            case "crosstabvariable":
                                ((HistogramChartParameters)Parameters).CrosstabVariableName = child.InnerText.Replace("&lt;", "<");
                                break;
                            case "secondyvar":
                                if (this.Parameters.ColumnNames.Count > 1)
                                {
                                    ((HistogramChartParameters)Parameters).ColumnNames[1] = (child.InnerText.Replace("&lt;", "<"));
                                }
                                else
                                {
                                    ((HistogramChartParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                                }
                                break;
                            case "secondyvartype":
                                Y2Type y2Type = ((Y2Type)Int32.Parse(child.InnerText));
                                ((HistogramChartParameters)Parameters).Y2AxisType = int.Parse(child.InnerText.Replace("&lt;", "<"));
                                break;
                            case "sort":
                                if (child.InnerText.ToLower().Equals("highlow") || child.InnerText.ToLower().Equals("hightolow"))
                                {
                                    ((HistogramChartParameters)Parameters).SortHighToLow = true;
                                }
                                break;
                            case "allvalues":
                                if (child.InnerText.ToLower().Equals("true"))
                                {
                                    ((HistogramChartParameters)Parameters).ShowAllListValues = true;
                                }
                                else { ((HistogramChartParameters)Parameters).ShowAllListValues = false; }
                                break;
                            case "showlistlabels":
                                if (child.InnerText.ToLower().Equals("true"))
                                {
                                    ((HistogramChartParameters)Parameters).ShowCommentLegalLabels = true;
                                }
                                else { ((HistogramChartParameters)Parameters).ShowCommentLegalLabels = false; }
                                break;
                            case "includemissing":
                                if (child.InnerText.ToLower().Equals("true")) { ((HistogramChartParameters)Parameters).IncludeMissing = true; }
                                else { ((HistogramChartParameters)Parameters).IncludeMissing = false; }
                                break;
                            case "customheading":
                                if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
                                {
                                    this.Parameters.GadgetTitle = child.InnerText.Replace("&lt;", "<");
                                    this.CustomOutputHeading = child.InnerText.Replace("&lt;", "<");
                                }
                                else
                                {
                                    this.CustomOutputHeading = string.Empty;
                                    this.Parameters.GadgetTitle = string.Empty;
                                }
                                break;
                            case "customdescription":
                                if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
                                {
                                    this.CustomOutputDescription = child.InnerText.Replace("&lt;", "<");
                                    this.Parameters.GadgetDescription = child.InnerText.Replace("&lt;", "<");
                                    if (!string.IsNullOrEmpty(CustomOutputDescription) && !CustomOutputHeading.Equals("(none)"))
                                    {
                                        descriptionPanel.Text = CustomOutputDescription;
                                        descriptionPanel.PanelMode = EpiDashboard.Controls.GadgetDescriptionPanel.DescriptionPanelMode.DisplayMode;
                                    }
                                    else
                                    {
                                        descriptionPanel.PanelMode = EpiDashboard.Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
                                    }
                                }
                                break;
                            case "customcaption":
                                this.CustomOutputCaption = child.InnerText;
                                break;
                            case "datafilters":
                                this.DataFilters = new DataFilters(this.DashboardHelper);
                                this.DataFilters.CreateFromXml(child);
                                break;
                            case "usediffbarcolors":
                                if (child.InnerText.ToLower().Equals("true")) { ((HistogramChartParameters)Parameters).UseDiffColors = true; }
                                else { ((HistogramChartParameters)Parameters).UseDiffColors = false; }
                                break;
                            case "userefvalues":
                                if (child.InnerText.ToLower().Equals("true")) { ((HistogramChartParameters)Parameters).UseRefValues = true; }
                                else { ((HistogramChartParameters)Parameters).UseRefValues = false; }
                                break;
                            case "showannotations":
                                if (child.InnerText.ToLower().Equals("true")) { ((HistogramChartParameters)Parameters).ShowAnnotations = true; }
                                else { ((HistogramChartParameters)Parameters).ShowAnnotations = false; }
                                break;
                            case "y2showannotations":
                                if (child.InnerText.ToLower().Equals("true")) { ((HistogramChartParameters)Parameters).Y2ShowAnnotations = true; }
                                else { ((HistogramChartParameters)Parameters).Y2ShowAnnotations = false; }
                                break;
                            case "showgridlines":
                                if (child.InnerText.ToLower().Equals("true")) { ((HistogramChartParameters)Parameters).ShowGridLines = true; }
                                else { ((HistogramChartParameters)Parameters).ShowGridLines = false; }
                                break;
                            case "composition":
                                {
                                    switch (child.InnerText)
                                    {
                                        case "0":
                                            ((HistogramChartParameters)Parameters).Composition = CompositionKind.SideBySide;
                                            break;
                                        case "1":
                                            ((HistogramChartParameters)Parameters).Composition = CompositionKind.Stacked;
                                            break;
                                        case "2":
                                            ((HistogramChartParameters)Parameters).Composition = CompositionKind.Stacked100;
                                            break;
                                    }
                                }
                                break;
                            case "step":
                                ((HistogramChartParameters)Parameters).Step = int.Parse(child.InnerText.Replace("&lt;", "<"));
                                break;
                            case "interval":
                                ((HistogramChartParameters)Parameters).Interval = child.InnerText.Replace("&lt;", "<");
                                break;
                            case "barspace":
                                {
                                    switch (child.InnerText)
                                    {
                                        case "0":
                                            ((HistogramChartParameters)Parameters).BarSpace = BarSpacing.Default;
                                            break;
                                        case "1":
                                            ((HistogramChartParameters)Parameters).BarSpace = BarSpacing.None;
                                            break;
                                        case "2":
                                            ((HistogramChartParameters)Parameters).BarSpace = BarSpacing.Small;
                                            break;
                                        case "3":
                                            ((HistogramChartParameters)Parameters).BarSpace = BarSpacing.Medium;
                                            break;
                                        case "4":
                                            ((HistogramChartParameters)Parameters).BarSpace = BarSpacing.Large;
                                            break;
                                    }
                                }
                                break;
                            case "orientation":
                                if (child.InnerText == "0")
                                {
                                    ((HistogramChartParameters)Parameters).Orientation = Orientation.Vertical;
                                }
                                else
                                {
                                    ((HistogramChartParameters)Parameters).Orientation = Orientation.Horizontal;
                                }
                                break;
                            case "palette":
                                ((HistogramChartParameters)Parameters).Palette = int.Parse(child.InnerText);
                                break;
                            case "bartype":
                                switch (child.InnerText.ToString())
                                {
                                    default:
                                    case "0":
                                        ((HistogramChartParameters)Parameters).BarKind = BarKind.Block;
                                        break;
                                    case "1":
                                        ((HistogramChartParameters)Parameters).BarKind = BarKind.Cylinder;
                                        break;
                                    case "2":
                                        ((HistogramChartParameters)Parameters).BarKind = BarKind.Rectangle;
                                        break;
                                    case "3":
                                        ((HistogramChartParameters)Parameters).BarKind = BarKind.RoundedBlock;
                                        break;
                                }
                                break;
                            case "legenddock":
                                {
                                    switch (int.Parse(child.InnerText))
                                    {
                                        case 0:
                                            ((HistogramChartParameters)Parameters).LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Left;
                                            break;
                                        default:
                                        case 1:
                                            ((HistogramChartParameters)Parameters).LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Right;
                                            break;
                                        case 2:
                                            ((HistogramChartParameters)Parameters).LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Top;
                                            break;
                                        case 3:
                                            ((HistogramChartParameters)Parameters).LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Bottom;
                                            break;
                                    }
                                }
                                break;
                            case "y2linetype":
                                switch (child.InnerText)
                                {
                                    case "Polygon":
                                        ((HistogramChartParameters)Parameters).Y2LineKind = LineKind.Polygon;
                                        break;
                                    case "Smooth":
                                        ((HistogramChartParameters)Parameters).Y2LineKind = LineKind.Smooth;
                                        break;
                                    case "Step":
                                        ((HistogramChartParameters)Parameters).Y2LineKind = LineKind.Step;
                                        break;
                                    case "Auto":
                                        ((HistogramChartParameters)Parameters).Y2LineKind = LineKind.Auto;
                                        break;
                                }
                                break;
                            case "y2linedashstyle":
                                {
                                    switch (child.InnerText)
                                    {
                                        case "Solid":
                                            ((HistogramChartParameters)Parameters).Y2LineDashStyle = LineDashStyle.Solid;
                                            break;
                                        case "Dash":
                                            ((HistogramChartParameters)Parameters).Y2LineDashStyle = LineDashStyle.Dash;
                                            break;
                                        case "Dot":
                                            ((HistogramChartParameters)Parameters).Y2LineDashStyle = LineDashStyle.Dot;
                                            break;
                                        case "DashDot":
                                            ((HistogramChartParameters)Parameters).Y2LineDashStyle = LineDashStyle.DashDot;
                                            break;
                                        case "DashDotDot":
                                            ((HistogramChartParameters)Parameters).Y2LineDashStyle = LineDashStyle.DashDotDot;
                                            break;
                                    }
                                    break;
                                }
                            case "y2linethickness":
                                ((HistogramChartParameters)Parameters).Y2LineThickness = int.Parse(child.InnerText);
                                break;
                            case "yaxislabel":
                                ((HistogramChartParameters)Parameters).YAxisLabel = child.InnerText;
                                break;
								//EI-98
                            case "yaxislabelfontsize":
                                ((HistogramChartParameters)Parameters).YAxisLabelFontSize = double.Parse(child.InnerText);
                                break;
                            case "xaxislabelfontsize":
                                ((HistogramChartParameters)Parameters).XAxisLabelFontSize = double.Parse(child.InnerText);
                                break;
                            case "yaxisfontsize":
                                ((HistogramChartParameters)Parameters).YAxisFontSize = double.Parse(child.InnerText);
                                break;
                            case "xaxisfontsize":
                                ((HistogramChartParameters)Parameters).XAxisFontSize = double.Parse(child.InnerText);
                                break;
                            case "yaxisformatstring":
                                ((HistogramChartParameters)Parameters).YAxisFormat = child.InnerText;
                                break;
                            case "y2axisformatstring":
                                ((HistogramChartParameters)Parameters).Y2AxisFormat = child.InnerText;
                                break;
                            case "y2axislabel":
                                ((HistogramChartParameters)Parameters).Y2AxisLabel = child.InnerText;
                                break;
                            case "y2axislegendtitle":
                                ((HistogramChartParameters)Parameters).Y2AxisLegendTitle = child.InnerText;
                                break;
                            case "xaxislabeltype":
                                {
                                    ((HistogramChartParameters)Parameters).XAxisLabelType = int.Parse(child.InnerText);
                                    break;
                                }
                            case "xaxislabel":
                                ((HistogramChartParameters)Parameters).XAxisLabel = child.InnerText;
                                break;
                            case "xaxisangle":
                                ((HistogramChartParameters)Parameters).XAxisAngle = int.Parse(child.InnerText);
                                break;
                            case "charttitle":
                                ((HistogramChartParameters)Parameters).ChartTitle = child.InnerText;
                                break;
                            case "chartsubtitle":
                                ((HistogramChartParameters)Parameters).ChartSubTitle = child.InnerText;
                                break;
                            case "showlegend":
                                if (child.InnerText.ToLower().Equals("true")) { ((HistogramChartParameters)Parameters).ShowLegend = true; }
                                else { ((HistogramChartParameters)Parameters).ShowLegend = false; }
                                break;
                            case "showlegendborder":
                                if (child.InnerText.ToLower().Equals("true")) { ((HistogramChartParameters)Parameters).ShowLegendBorder = true; }
                                else { ((HistogramChartParameters)Parameters).ShowLegendBorder = false; }
                                break;
                            case "showlegendvarnames":
                                if (child.InnerText.ToLower().Equals("true")) { ((HistogramChartParameters)Parameters).ShowLegendVarNames = true; }
                                else { ((HistogramChartParameters)Parameters).ShowLegendVarNames = false; }
                                break;
                            case "legendfontsize":
                                ((HistogramChartParameters)Parameters).LegendFontSize = int.Parse(child.InnerText);
                                break;
                            case "height":
                                ((HistogramChartParameters)Parameters).ChartHeight = double.Parse(child.InnerText);
                                break;
                            case "width":
                                ((HistogramChartParameters)Parameters).ChartWidth = double.Parse(child.InnerText);
                                break;
                        }
                    }
                }
            }

            base.CreateFromXml(element);

            this.LoadingCombos = false;
            RefreshResults();
            HideConfigPanel();
        }

        /// <summary>
        /// Serializes the gadget into Xml
        /// </summary>
        /// <param name="doc">The Xml docment</param>
        /// <returns>XmlNode</returns>
        public override XmlNode Serialize(XmlDocument doc)
        {
            HistogramChartParameters chtParameters = (HistogramChartParameters)Parameters;

            System.Xml.XmlElement element = doc.CreateElement("histogramChartGadget");
            string xmlString = string.Empty;
            element.InnerXml = xmlString;
            element.AppendChild(SerializeFilters(doc));

            System.Xml.XmlAttribute id = doc.CreateAttribute("id");
            System.Xml.XmlAttribute locationY = doc.CreateAttribute("top");
            System.Xml.XmlAttribute locationX = doc.CreateAttribute("left");
            System.Xml.XmlAttribute collapsed = doc.CreateAttribute("collapsed");
            System.Xml.XmlAttribute type = doc.CreateAttribute("gadgetType");
            System.Xml.XmlAttribute actualHeight = doc.CreateAttribute("actualHeight");

            id.Value = this.UniqueIdentifier.ToString();
            locationY.Value = Canvas.GetTop(this).ToString("F0");
            locationX.Value = Canvas.GetLeft(this).ToString("F0");
            collapsed.Value = "false"; // currently no way to collapse the gadget, so leave this 'false' for now
            type.Value = "EpiDashboard.Gadgets.Charting.HistogramChartGadget";
            actualHeight.Value = this.ActualHeight.ToString();

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);
            element.Attributes.Append(id);
            element.Attributes.Append(actualHeight);

            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            string mainVar = string.Empty;
            string strataVar = string.Empty;
            string crosstabVar = string.Empty;
            string weightVar = string.Empty;
            string second_y_var = string.Empty;
            string sort = string.Empty;
            bool allValues = false;
            bool showConfLimits = true;
            bool showCumulativePercent = true;
            bool includeMissing = false;

            double step = 1;
            double.TryParse(chtParameters.Step.ToString(), out step);

            string interval = "Day";
            interval = chtParameters.Interval;

            double height = 600;
            double width = 800;

            double.TryParse(chtParameters.ChartHeight.ToString(), out height);
            double.TryParse(chtParameters.ChartWidth.ToString(), out width);

            //mainVariable
            XmlElement freqVarElement = doc.CreateElement("mainVariable");
            if (chtParameters.ColumnNames.Count > 0)
            {
                if (!String.IsNullOrEmpty(chtParameters.ColumnNames[0].ToString()))
                {
                    freqVarElement.InnerText = chtParameters.ColumnNames[0].ToString().Replace("<", "&lt;");
                    element.AppendChild(freqVarElement);
                }
            }
            // =========  Former Advanced Options section  ============
            //weightVariable
            XmlElement weightVariableElement = doc.CreateElement("weightVariable");
            if (!String.IsNullOrEmpty(chtParameters.WeightVariableName))
            {
                weightVariableElement.InnerText = chtParameters.WeightVariableName.Replace("<", "&lt;");
                element.AppendChild(weightVariableElement);
            }

            //Cross Tab Variable
            XmlElement crossTabVarElement = doc.CreateElement("crosstabVariable");
            if (!String.IsNullOrEmpty(chtParameters.CrosstabVariableName))
            {
                crossTabVarElement.InnerText = chtParameters.CrosstabVariableName.Replace("<", "&lt;");
                element.AppendChild(crossTabVarElement);
            }

            //strataVariables
            XmlElement StrataVariableNameElement = doc.CreateElement("strataVariable");
            XmlElement StrataVariableNamesElement = doc.CreateElement("strataVariables");
            if (chtParameters.StrataVariableNames.Count == 1)
            {
                StrataVariableNameElement.InnerText = chtParameters.StrataVariableNames[0].ToString().Replace("<", "&lt;");
                element.AppendChild(StrataVariableNameElement);
            }
            else if (chtParameters.StrataVariableNames.Count > 1)
            {
                foreach (string strataColumn in chtParameters.StrataVariableNames)
                {
                    XmlElement strataElement = doc.CreateElement("strataVariable");
                    strataElement.InnerText = strataColumn.Replace("<", "&lt;");
                    StrataVariableNamesElement.AppendChild(strataElement);
                }

                element.AppendChild(StrataVariableNamesElement);
            }

            //secondYVarType
            XmlElement secondYVarTypeElement = doc.CreateElement("secondYVarType");
            secondYVarTypeElement.InnerText = chtParameters.Y2AxisType.ToString().Replace("<", "&lt;");
            element.AppendChild(secondYVarTypeElement);

            //secondYVar
            XmlElement secondYVarElement = doc.CreateElement("secondYVar");
            if (chtParameters.ColumnNames.Count > 1)
            {
                if (!String.IsNullOrEmpty(chtParameters.ColumnNames[1].ToString()))
                {
                    secondYVarElement.InnerText = chtParameters.ColumnNames[1].ToString().Replace("<", "&lt;");
                    element.AppendChild(secondYVarElement);
                }
            }

            //height 
            XmlElement heightElement = doc.CreateElement("height");
            heightElement.InnerText = chtParameters.ChartHeight.ToString().Replace("<", "&lt;");
            element.AppendChild(heightElement);

            //width 
            XmlElement widthElement = doc.CreateElement("width");
            widthElement.InnerText = chtParameters.ChartWidth.ToString().Replace("<", "&lt;");
            element.AppendChild(widthElement);

            //showAllListValues
            XmlElement allValuesElement = doc.CreateElement("allValues");
            allValuesElement.InnerText = chtParameters.ShowAllListValues.ToString().Replace("<", "&lt;");
            element.AppendChild(allValuesElement);

            //showListLabels
            XmlElement showListLabelsElement = doc.CreateElement("showListLabels");
            showListLabelsElement.InnerText = chtParameters.ShowCommentLegalLabels.ToString().Replace("<", "&lt;");
            element.AppendChild(showListLabelsElement);

            //sort
            XmlElement sortElement = doc.CreateElement("sort");
            if (chtParameters.SortHighToLow) sortElement.InnerText = "hightolow";
            element.AppendChild(sortElement);

            //GADGET DOES NOT CURRENTLY ALLOW FOR ShowConfLimits or ShowCumPercent
            //if (inputVariableList.ContainsKey("showconflimits"))
            //{
            //    showConfLimits = bool.Parse(inputVariableList["showconflimits"]);
            //}
            //if (inputVariableList.ContainsKey("showcumulativepercent"))
            //{
            //    showCumulativePercent = bool.Parse(inputVariableList["showcumulativepercent"]);
            //}

            //includeMissing
            XmlElement includeMissingElement = doc.CreateElement("includeMissing");
            includeMissingElement.InnerText = chtParameters.IncludeMissing.ToString();
            element.AppendChild(includeMissingElement);

            //customHeading
            XmlElement customHeadingElement = doc.CreateElement("customHeading");
            customHeadingElement.InnerText = chtParameters.GadgetTitle.Replace("<", "&lt;");
            element.AppendChild(customHeadingElement);

            //customDescription
            XmlElement customDescriptionElement = doc.CreateElement("customDescription");
            customDescriptionElement.InnerText = chtParameters.GadgetDescription.Replace("<", "&lt;");
            element.AppendChild(customDescriptionElement);

            //customCaption
            XmlElement customCaptionElement = doc.CreateElement("customCaption");
            if (!String.IsNullOrEmpty(CustomOutputCaption))
            {
                customCaptionElement.InnerText = CustomOutputCaption.Replace("<", "&lt;");
            }
            else
            {
                customCaptionElement.InnerText = string.Empty;
            }
            element.AppendChild(customCaptionElement);


            //useDiffBarColors 
            XmlElement useDiffBarColorsElement = doc.CreateElement("useDiffBarColors");
            useDiffBarColorsElement.InnerText = chtParameters.UseDiffColors.ToString();
            element.AppendChild(useDiffBarColorsElement);

            //useRefValues 
            XmlElement useRefValuesElement = doc.CreateElement("useRefValues");
            useRefValuesElement.InnerText = chtParameters.UseRefValues.ToString();
            element.AppendChild(useRefValuesElement);

            //showAnnotations 
            XmlElement showAnnotationsElement = doc.CreateElement("showAnnotations");
            showAnnotationsElement.InnerText = chtParameters.ShowAnnotations.ToString();
            element.AppendChild(showAnnotationsElement);

            //y2showAnnotations 
            XmlElement y2showAnnotationsElement = doc.CreateElement("y2showAnnotations");
            y2showAnnotationsElement.InnerText = chtParameters.Y2ShowAnnotations.ToString();
            element.AppendChild(y2showAnnotationsElement);

            //showGridLines 
            XmlElement showGridLinesElement = doc.CreateElement("showGridLines");
            showGridLinesElement.InnerText = chtParameters.ShowGridLines.ToString();
            element.AppendChild(showGridLinesElement);

            //composition 
            XmlElement compositionElement = doc.CreateElement("composition");
            switch (chtParameters.Composition.ToString())
            {
                case "SideBySide":
                    compositionElement.InnerText = "0";
                    break;
                case "Stacked":
                    compositionElement.InnerText = "1";
                    break;
                case "Stacked100":
                    compositionElement.InnerText = "2";
                    break;
            }
            element.AppendChild(compositionElement);

            //barSpace 
            XmlElement barSpaceElement = doc.CreateElement("barSpace");
            switch (chtParameters.BarSpace.ToString())
            {
                default:
                case "Default":
                    barSpaceElement.InnerText = "0";
                    break;
                case "None":
                    barSpaceElement.InnerText = "1";
                    break;
                case "Small":
                    barSpaceElement.InnerText = "2";
                    break;
                case "Medium":
                    barSpaceElement.InnerText = "3";
                    break;
                case "Large":
                    barSpaceElement.InnerText = "4";
                    break;
            }
            element.AppendChild(barSpaceElement);

            //orientation 
            XmlElement orientationElement = doc.CreateElement("orientation");
            switch (chtParameters.Orientation.ToString())
            {
                case "Vertical":
                    orientationElement.InnerText = "0";
                    break;
                case "Horizontal":
                    orientationElement.InnerText = "1";
                    break;
            }
            element.AppendChild(orientationElement);

            //palette 
            XmlElement paletteElement = doc.CreateElement("palette");
            paletteElement.InnerText = chtParameters.Palette.ToString();
            element.AppendChild(paletteElement);

            //barType 
            XmlElement barTypeElement = doc.CreateElement("barType");
            switch (chtParameters.BarKind.ToString())
            {
                case "Block":
                    barTypeElement.InnerText = "0";
                    break;
                case "Cylinder":
                    barTypeElement.InnerText = "1";
                    break;
                case "Rectangle":
                    barTypeElement.InnerText = "2";
                    break;
                case "RoundedBlock":
                    barTypeElement.InnerText = "3";
                    break;
            }
            element.AppendChild(barTypeElement);

            //step
            XmlElement stepElement = doc.CreateElement("step");
            stepElement.InnerText = chtParameters.Step.ToString();
            element.AppendChild(stepElement);

            //interval
            XmlElement intervalElement = doc.CreateElement("interval");
            intervalElement.InnerText = chtParameters.Interval.ToString();
            element.AppendChild(intervalElement);

            //y2LineType 
            XmlElement y2LineTypeElement = doc.CreateElement("y2LineType");
            y2LineTypeElement.InnerText = chtParameters.Y2LineKind.ToString();
            element.AppendChild(y2LineTypeElement);

            //y2LineDashStyle 
            XmlElement y2LineDashStyleElement = doc.CreateElement("y2LineDashStyle");
            y2LineDashStyleElement.InnerText = chtParameters.Y2LineDashStyle.ToString();
            element.AppendChild(y2LineDashStyleElement);

            //y2LineThickness 
            XmlElement y2LineThicknessElement = doc.CreateElement("y2LineThickness");
            y2LineThicknessElement.InnerText = chtParameters.Y2LineThickness.ToString();
            element.AppendChild(y2LineThicknessElement);

            //yAxisLabel 
            XmlElement yAxisLabelElement = doc.CreateElement("yAxisLabel");
            yAxisLabelElement.InnerText = chtParameters.YAxisLabel.ToString().Replace("<", "&lt;");
            element.AppendChild(yAxisLabelElement);

            //yAxisLabelFontSize 
            XmlElement yAxisLabelFontSizeElement = doc.CreateElement("yAxisLabelFontSize");
            yAxisLabelFontSizeElement.InnerText = chtParameters.YAxisLabelFontSize.ToString().Replace("<", "&lt;");
            element.AppendChild(yAxisLabelFontSizeElement);

            //xAxisLabelFontSize 
            XmlElement xAxisLabelFontSize = doc.CreateElement("xAxisLabelFontSize");
            xAxisLabelFontSize.InnerText = chtParameters.XAxisLabelFontSize.ToString().Replace("<", "&lt;");
            element.AppendChild(xAxisLabelFontSize);

            //yAxisFontSize 
            XmlElement yAxisFontSizeElement = doc.CreateElement("yAxisFontSize");
            yAxisFontSizeElement.InnerText = chtParameters.YAxisFontSize.ToString().Replace("<", "&lt;");
            element.AppendChild(yAxisFontSizeElement);

            //xAxisFontSize 
            XmlElement xAxisFontSize = doc.CreateElement("xAxisFontSize");
            xAxisFontSize.InnerText = chtParameters.XAxisFontSize.ToString().Replace("<", "&lt;");
            element.AppendChild(xAxisFontSize);

            //yAxisFormatString 
            XmlElement yAxisFormatStringElement = doc.CreateElement("yAxisFormatString");
            yAxisFormatStringElement.InnerText = chtParameters.YAxisFormat.ToString().Replace("<", "&lt;");
            element.AppendChild(yAxisFormatStringElement);

            //y2AxisLabel 
            XmlElement y2AxisLabelElement = doc.CreateElement("y2AxisLabel");
            y2AxisLabelElement.InnerText = chtParameters.Y2AxisLabel.ToString().Replace("<", "&lt;");
            element.AppendChild(y2AxisLabelElement);

            //y2AxisLegendTitle 
            XmlElement y2AxisLegendTitleElement = doc.CreateElement("y2AxisLegendTitle");
            y2AxisLegendTitleElement.InnerText = chtParameters.Y2AxisLegendTitle.ToString().Replace("<", "&lt;");
            element.AppendChild(y2AxisLegendTitleElement);

            //y2AxisFormatString 
            XmlElement y2AxisFormatStringElement = doc.CreateElement("y2AxisFormatString");
            y2AxisFormatStringElement.InnerText = chtParameters.Y2AxisFormat.ToString().Replace("<", "&lt;");
            element.AppendChild(y2AxisFormatStringElement);

            //xAxisLabelType 
            XmlElement xAxisLabelTypeElement = doc.CreateElement("xAxisLabelType");
            xAxisLabelTypeElement.InnerText = chtParameters.XAxisLabelType.ToString().Replace("<", "&lt;");
            element.AppendChild(xAxisLabelTypeElement);

            //xAxisLabel 
            XmlElement xAxisLabelElement = doc.CreateElement("xAxisLabel");
            xAxisLabelElement.InnerText = chtParameters.XAxisLabel.ToString().Replace("<", "&lt;");
            element.AppendChild(xAxisLabelElement);

            //xAxisAngle 
            XmlElement xAxisAngleElement = doc.CreateElement("xAxisAngle");
            xAxisAngleElement.InnerText = chtParameters.XAxisAngle.ToString().Replace("<", "&lt;");
            element.AppendChild(xAxisAngleElement);

            //chartTitle 
            XmlElement chartTitleElement = doc.CreateElement("chartTitle");
            chartTitleElement.InnerText = chtParameters.ChartTitle.ToString().Replace("<", "&lt;");
            element.AppendChild(chartTitleElement);

            //chartSubTitle 
            XmlElement chartSubTitleElement = doc.CreateElement("chartSubTitle");
            chartSubTitleElement.InnerText = chtParameters.ChartSubTitle.ToString().Replace("<", "&lt;");
            element.AppendChild(chartSubTitleElement);

            //showLegend 
            XmlElement showLegendElement = doc.CreateElement("showLegend");
            showLegendElement.InnerText = chtParameters.ShowLegend.ToString().Replace("<", "&lt;");
            element.AppendChild(showLegendElement);

            //showLegendBorder 
            XmlElement showLegendBorderElement = doc.CreateElement("showLegendBorder");
            showLegendBorderElement.InnerText = chtParameters.ShowLegendBorder.ToString();
            element.AppendChild(showLegendBorderElement);

            //showLegendVarNames 
            XmlElement showLegendVarNamesElement = doc.CreateElement("showLegendVarNames");
            showLegendVarNamesElement.InnerText = chtParameters.ShowLegendVarNames.ToString();
            element.AppendChild(showLegendVarNamesElement);

            //legendFontSize 
            XmlElement legendFontSizeElement = doc.CreateElement("legendFontSize");
            legendFontSizeElement.InnerText = chtParameters.LegendFontSize.ToString();
            element.AppendChild(legendFontSizeElement);

            ////startdate 
            //XmlElement startDateElement = doc.CreateElement("startdate");
            //startDateElement.InnerText = chtParameters.StartDate.ToString();
            //element.AppendChild(startDateElement);

            ////enddate 
            //XmlElement endDateElement = doc.CreateElement("enddate");
            //endDateElement.InnerText = chtParameters.EndDate.ToString();
            //element.AppendChild(endDateElement);
            if (!string.IsNullOrEmpty(chtParameters.EndDate.ToString()))
            {
                DateTime edDt = DateTime.Now;
                if (DateTime.TryParse(chtParameters.EndDate.ToString(), out edDt))
                {
                    XmlElement endDateElement = doc.CreateElement("enddate");
                    endDateElement.InnerText = edDt.Ticks.ToString();
                    element.AppendChild(endDateElement);
                }
                else if (chtParameters.EndDate.ToString().ToUpper() == "SYSTEMDATE" || chtParameters.EndDate.ToString() == "SYSTEMDATE")
                {
                    XmlElement endDateElement = doc.CreateElement("SYSTEMDATE");
                    endDateElement.InnerText = edDt.Ticks.ToString();
                    element.AppendChild(endDateElement);
                }
            }

            if (!string.IsNullOrEmpty(chtParameters.StartDate.ToString()))
            {
                DateTime stDt = DateTime.Now;
                if (DateTime.TryParse(chtParameters.StartDate.ToString(), out stDt))
                {
                    XmlElement stDateElement = doc.CreateElement("startdate");
                    stDateElement.InnerText = stDt.Ticks.ToString();
                    element.AppendChild(stDateElement);
                }
                else if (chtParameters.EndDate.ToString().ToUpper() == "SYSTEMDATE" || chtParameters.EndDate.ToString() == "SYSTEMDATE")
                {
                    XmlElement stDateElement = doc.CreateElement("SYSTEMDATE");
                    stDateElement.InnerText = stDt.Ticks.ToString();
                    element.AppendChild(stDateElement);
                }
            }

            //legendDock 
            XmlElement legendDockElement = doc.CreateElement("legendDock");
            switch (chtParameters.LegendDock)
            {
                case ComponentArt.Win.DataVisualization.Charting.Dock.Left:
                    legendDockElement.InnerText = "0";
                    break;
                default:
                case ComponentArt.Win.DataVisualization.Charting.Dock.Right:
                    legendDockElement.InnerText = "1";
                    break;
                case ComponentArt.Win.DataVisualization.Charting.Dock.Top:
                    legendDockElement.InnerText = "2";
                    break;
                case ComponentArt.Win.DataVisualization.Charting.Dock.Bottom:
                    legendDockElement.InnerText = "3";
                    break;
            }

            element.AppendChild(legendDockElement);

            SerializeAnchors(element);

            return element;
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0)
        {
            HistogramChartParameters chtParameters = (HistogramChartParameters)Parameters;
            StringBuilder sb = new StringBuilder();
            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            sb.AppendLine("<h2>" + chtParameters.ChartTitle + "</h2>");
            sb.AppendLine("<h3>" + chtParameters.ChartSubTitle + "</h3>");

            foreach (UIElement element in panelMain.Children)
            {
                if (element is EpiDashboard.Controls.Charting.HistogramChart)
                {
                    sb.AppendLine(((EpiDashboard.Controls.Charting.HistogramChart)element).ToHTML(htmlFileName, count, true, false));
                    count++;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets/sets the gadget's custom output heading
        /// </summary>
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

        /// <summary>
        /// Gets/sets the gadget's custom output description
        /// </summary>
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

        Controls.GadgetProperties.HistogramChartProperties properties = null;
        public override void ShowHideConfigPanel()
        {
            Popup = new DashboardPopup();
            Popup.Parent = ((this.Parent as DragCanvas).Parent as ScrollViewer).Parent as Grid;
            properties = new Controls.GadgetProperties.HistogramChartProperties(this.DashboardHelper, this, (HistogramChartParameters)Parameters, StrataGridList);

            if (DashboardHelper.ResizedWidth != 0 & DashboardHelper.ResizedHeight != 0)
            {
                double i_StandardHeight = System.Windows.SystemParameters.PrimaryScreenHeight;//Developer Desktop Width Where the Form is Designed
                double i_StandardWidth = System.Windows.SystemParameters.PrimaryScreenWidth; ////Developer Desktop Height Where the Form is Designed
                float f_HeightRatio = new float();
                float f_WidthRatio = new float();
                f_HeightRatio = (float)((float)DashboardHelper.ResizedHeight / (float)i_StandardHeight);
                f_WidthRatio = (float)((float)DashboardHelper.ResizedWidth / (float)i_StandardWidth);

                properties.Height = (Convert.ToInt32(i_StandardHeight * f_HeightRatio)) / 1.07;
                properties.Width = (Convert.ToInt32(i_StandardWidth * f_WidthRatio)) / 1.07;

            }
            else
            {
                properties.Width = (System.Windows.SystemParameters.PrimaryScreenWidth / 1.07);
                properties.Height = (System.Windows.SystemParameters.PrimaryScreenHeight / 1.15);
            }

            properties.Cancelled += new EventHandler(properties_Cancelled);
            properties.ChangesAccepted += new EventHandler(properties_ChangesAccepted);
            Popup.Content = properties;
            Popup.Show();
        }

        public override void GadgetBase_SizeChanged(double width, double height)
        {
            double i_StandardHeight = System.Windows.SystemParameters.PrimaryScreenHeight;//Developer Desktop Width Where the Form is Designed
            double i_StandardWidth = System.Windows.SystemParameters.PrimaryScreenWidth; ////Developer Desktop Height Where the Form is Designed
            float f_HeightRatio = new float();
            float f_WidthRatio = new float();
            f_HeightRatio = (float)((float)height / (float)i_StandardHeight);
            f_WidthRatio = (float)((float)width / (float)i_StandardWidth);

            properties.Height = (Convert.ToInt32(i_StandardHeight * f_HeightRatio)) / 1.07;
            properties.Width = (Convert.ToInt32(i_StandardWidth * f_WidthRatio)) / 1.07;

        }

        #endregion //Public Methods

        #region Event Handlers
        protected override void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (DashboardHelper.IsAutoClosing)
            {
                System.Threading.Thread.Sleep(3000);
            }
            else
            {
                System.Threading.Thread.Sleep(400);
            }
            this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
        }

        protected override void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (syncLock)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));
                HistogramChartParameters chtParameters = (HistogramChartParameters)Parameters;

                string freqVar = chtParameters.ColumnNames[0];
                string weightVar = chtParameters.WeightVariableName;
                string strataVar = string.Empty;
                string crosstabVar = chtParameters.CrosstabVariableName;
                bool includeMissing = chtParameters.IncludeMissing;

                List<string> stratas = new List<string>();
                if (!string.IsNullOrEmpty(strataVar))
                {
                    stratas.Add(strataVar);
                }

                try
                {
                    RequestUpdateStatusDelegate requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
                    CheckForCancellationDelegate checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

                    chtParameters.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                    chtParameters.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);

                    if (this.DataFilters != null && this.DataFilters.Count > 0)
                    {
                        chtParameters.CustomFilter = this.DataFilters.GenerateDataFilterString(false);
                    }
                    else
                    {
                        chtParameters.CustomFilter = string.Empty;
                    }

                    
                    if (!string.IsNullOrEmpty(crosstabVar.Trim()))
                    {
                        List<string> crosstabVarList = new List<string>();
                        crosstabVarList.Add(crosstabVar);

                        foreach (Strata strata in DashboardHelper.GetStrataValuesAsDictionary(crosstabVarList, false, false))
                        {
                            HistogramChartParameters parameters = new HistogramChartParameters(chtParameters);

                            if (!string.IsNullOrEmpty(chtParameters.CustomFilter))
                            {
                                parameters.CustomFilter = "(" + parameters.CustomFilter + ") AND " + strata.SafeFilter;
                            }
                            else
                            {
                                parameters.CustomFilter = strata.SafeFilter;
                            }
                            parameters.CrosstabVariableName = string.Empty;
                            Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables = DashboardHelper.GenerateFrequencyTable(parameters);
                            GenerateChartData(stratifiedFrequencyTables, strata);
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables = DashboardHelper.GenerateFrequencyTable(chtParameters);
                        GenerateChartData(stratifiedFrequencyTables);
                    }

                    this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
                }
                finally
                {
                    stopwatch.Stop();
                    Debug.Print("Histogram (Epi Curve) chart gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete.");
                    Debug.Print(DashboardHelper.DataFilters.GenerateDataFilterString());
                }
            }
        }
        #endregion //Event Handlers
    }
}
