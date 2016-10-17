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
using ComponentArt.Win.DataVisualization.Charting;

namespace EpiDashboard.Gadgets.Charting
{
    /// <summary>
    /// Interaction logic for PieChartGadget.xaml
    /// </summary>
    public partial class PieChartGadget : ChartGadgetBase
    {
        #region Constructors
        public PieChartGadget()
        {
            InitializeComponent();
            Construct();
        }

        public PieChartGadget(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            Construct();
        }
        #endregion //Constructors

        #region Private and Protected Methods
        /// <summary>
        /// A custom heading to use for this gadget's output
        /// </summary>
        private string customOutputHeading;

        /// <summary>
        /// A custom description to use for this gadget's output
        /// </summary>
        private string customOutputDescription;

        protected override void Construct()
        {
            this.Parameters = new PieChartParameters();

            if (!string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)"))
            {
                headerPanel.Text = CustomOutputHeading;
            }

            mnuCopy.Click += new RoutedEventHandler(mnuCopy_Click);
            mnuSendDataToHTML.Click += new RoutedEventHandler(mnuSendDataToHTML_Click);

            //#if LINUX_BUILD
            //            mnuSendDataToExcel.Visibility = Visibility.Collapsed;
            //#else
            //            mnuSendDataToExcel.Visibility = Visibility.Visible;
            //            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.ClassesRoot; Microsoft.Win32.RegistryKey excelKey = key.OpenSubKey("Excel.Application"); bool excelInstalled = excelKey == null ? false : true; key = Microsoft.Win32.Registry.ClassesRoot;
            //            excelKey = key.OpenSubKey("Excel.Application");
            //            excelInstalled = excelKey == null ? false : true;

            //            if (!excelInstalled)
            //            {
            //                mnuSendDataToExcel.Visibility = Visibility.Collapsed;
            //            }
            //            else
            //            {
            //                mnuSendDataToExcel.Click += new RoutedEventHandler(mnuSendDataToExcel_Click);
            //            }
            //#endif

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
            //dvChart.DataSource = null;
            //dvChart.Width = 0;
            //dvChart.Height = 0;

            StrataGridList.Clear();
            StrataExpanderList.Clear();
        }

        private void properties_ChangesAccepted(object sender, EventArgs e)
        {
            Controls.GadgetProperties.PieChartProperties properties = Popup.Content as Controls.GadgetProperties.PieChartProperties;
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
            //dvChart.DataSource = null;
            //dvChart.Visibility = System.Windows.Visibility.Collapsed;

            HideConfigPanel();
            CheckAndSetPosition();
        }
        protected override void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (DashboardHelper.IsAutoClosing)
            {
                System.Threading.Thread.Sleep(2200);
            }
            else
            {
                System.Threading.Thread.Sleep(300);
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
                PieChartParameters chtParameters = (PieChartParameters)Parameters;

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
                            PieChartParameters parameters = new PieChartParameters(chtParameters);

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
                    Debug.Print("Pie chart gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete.");
                    Debug.Print(DashboardHelper.DataFilters.GenerateDataFilterString());
                }
            }
        }

        protected override void GenerateChartData(Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables, Strata strata = null)
        {
            lock (syncLockData)
            {
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

                    foreach (DataRow row in table.Rows)
                    {
                        XYColumnChartData chartData = new XYColumnChartData();
                        chartData.X = strataValue;
                        chartData.Y = (double)row[1];
                        chartData.S = row[0];

                        if (chartData.S == null || string.IsNullOrEmpty(chartData.S.ToString().Trim()))
                        {
                            chartData.S = Config.Settings.RepresentationOfMissing;
                        }

                        dataList.Add(chartData);
                    }
                }

                this.Dispatcher.BeginInvoke(new SetChartDataDelegate(SetChartData), dataList, strata);
            }
        }

        protected override void SetChartData(List<XYColumnChartData> dataList, Strata strata)
        {
            PieChartParameters chtParameters = (PieChartParameters)Parameters;
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

                Controls.Charting.PieChart pieChart = new Controls.Charting.PieChart(DashboardHelper, chtParameters, dataList);
                pieChart.Margin = new Thickness(0, 0, 0, 16);
                //Grid.SetColumn(columnChart, 1);
                pieChart.MouseEnter += new MouseEventHandler(chart_MouseEnter);
                pieChart.MouseLeave += new MouseEventHandler(chart_MouseLeave);
                panelMain.Children.Add(pieChart);
            }
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
            PieChartParameters chtParameters = (PieChartParameters)Parameters;
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
            foreach (XmlElement child in element.ChildNodes)
            {
                if(!String.IsNullOrEmpty(child.InnerText.Trim()))
                {
                    switch (child.Name.ToLowerInvariant())
                    {
                        case "allvalues":
                            if (child.InnerText.ToLowerInvariant().Equals("true"))
                            {
                                ((PieChartParameters)Parameters).ShowAllListValues = true;
                            }
                            else
                            {
                                ((PieChartParameters)Parameters).ShowAllListValues = false;
                            }
                            break;
                        case "singlevariable":
                            if (this.Parameters.ColumnNames.Count > 0)
                            {
                                ((PieChartParameters)Parameters).ColumnNames[0] = (child.InnerText.Replace("&lt;", "<"));
                            }
                            else
                            {
                                ((PieChartParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                            }
                            break;
                        case "weightvariable":
                            ((PieChartParameters)Parameters).WeightVariableName = child.InnerText.Replace("&lt;", "<");
                            break;
                        case "stratavariable":
                            if(this.Parameters.StrataVariableNames.Count>0)
                            {
                                ((PieChartParameters)Parameters).StrataVariableNames[0] = child.InnerText.Replace("&lt;", "<");
                            }
                            else
                            {
                                ((PieChartParameters)Parameters).StrataVariableNames.Add(child.InnerText.Replace("&lt;", "<"));
                            }
                            break;
                        case "customheading":
                            if (!child.InnerText.Equals("none"))
                            {
                                this.Parameters.GadgetTitle = child.InnerText.Replace("&lt;", "<");
                                this.CustomOutputHeading = child.InnerText.Replace("&lt;", "<");
                            }
                            break;
                        case "customdescription":
                            if (!child.InnerText.Equals("none"))
                            {
                                this.Parameters.GadgetDescription = child.InnerText.Replace("&lt;", "<");
                                this.CustomOutputDescription = child.InnerText.Replace("&lt;", "<");
                            }
                            break;
                        case "customcaption":
                            this.CustomOutputCaption = child.InnerText.Replace("&lt;", "<");
                            break;
                        case "chartwidth":
                            if (!string.IsNullOrEmpty(child.InnerText))
                            {
                                ((PieChartParameters)Parameters).ChartWidth = double.Parse(child.InnerText.Replace("&lt;", "<"));
                            }
                            break;
                        case "chartheight":
                            if (!string.IsNullOrEmpty(child.InnerText))
                            {
                                ((PieChartParameters)Parameters).ChartHeight = double.Parse(child.InnerText.Replace("&lt;", "<"));
                            }
                            break;
                        case "charttitle":
                            ((PieChartParameters)Parameters).ChartTitle = child.InnerText.Replace("&lt;", "<");
                            break;
                        case "chartsize":
                            switch (child.InnerText)
                            {
                                case "Small":
                                    ((PieChartParameters)Parameters).ChartWidth = 400;
                                    ((PieChartParameters)Parameters).ChartHeight = 250;
                                    break;
                                case "Medium":
                                    ((PieChartParameters)Parameters).ChartWidth = 533;
                                    ((PieChartParameters)Parameters).ChartHeight = 333;
                                    break;
                                case "Large":
                                    ((PieChartParameters)Parameters).ChartWidth = 800;
                                    ((PieChartParameters)Parameters).ChartHeight = 500;
                                    break;
                                case "Custom":
                                    break;
                            }
                            break;
                    }
                }
            }
        }

        public override void CreateFromXml(XmlElement element)
        {
            this.LoadingCombos = true;
            this.Parameters = new PieChartParameters();

            HideConfigPanel();
            infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            if (element.Name.Equals("chartGadget") || element.Name.Equals("ChartControl"))
            {
                CreateFromLegacyXml(element);
            }
            else
            {
                foreach (XmlElement child in element.ChildNodes)
                {
                    if (!String.IsNullOrEmpty(child.InnerText.Trim()))
                    {

                        switch (child.Name.ToLowerInvariant())
                        {
                            case "mainvariable":
                                if (this.Parameters.ColumnNames.Count > 0)
                                {
                                    ((PieChartParameters)Parameters).ColumnNames[0] = (child.InnerText.Replace("&lt;", "<"));
                                }
                                else
                                {
                                    ((PieChartParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                                }
                            break;
                            case "stratavariable":
                                if(this.Parameters.StrataVariableNames.Count>0)
                                {
                                    ((PieChartParameters)Parameters).StrataVariableNames[0] = child.InnerText.Replace("&lt;", "<");
                                }
                                else
                                {
                                    ((PieChartParameters)Parameters).StrataVariableNames.Add(child.InnerText.Replace("&lt;", "<"));
                                }
                                break;
                            case "stratavariables":
                                foreach (XmlElement field in child.ChildNodes)
                                {
                                    List<string> fields = new List<string>();
                                    if (field.Name.ToLowerInvariant().Equals("stratavariable"))
                                    {
                                        ((PieChartParameters)Parameters).StrataVariableNames.Add(field.InnerText.Replace("&lt;", "<"));
                                    }
                                }
                                break;
                            case "weightvariable":
                            ((PieChartParameters)Parameters).WeightVariableName = child.InnerText.Replace("&lt;", "<");
                                break;
                            case "crosstabvariable":
                                ((PieChartParameters)Parameters).CrosstabVariableName = child.InnerText.Replace("&lt;", "<");
                                break;
                            case "sort":
                                if (child.InnerText.ToLowerInvariant().Equals("highlow") || child.InnerText.ToLowerInvariant().Equals("hightolow"))
                                {
                                    ((PieChartParameters)Parameters).SortHighToLow = true;
                                }
                                break;
                            case "allvalues":
                                if (child.InnerText.ToLowerInvariant().Equals("true")) { ((PieChartParameters)Parameters).ShowAllListValues = true; }
                                else { ((PieChartParameters)Parameters).ShowAllListValues = false; }
                                break;
                            case "showlistlabels":
                                if (child.InnerText.ToLowerInvariant().Equals("true")) { ((PieChartParameters)Parameters).ShowCommentLegalLabels = true; }
                                else { ((PieChartParameters)Parameters).ShowCommentLegalLabels = false; }
                                break;
                            case "includemissing":
                                if (child.InnerText.ToLowerInvariant().Equals("true")) { ((PieChartParameters)Parameters).IncludeMissing = true; }
                                else { ((PieChartParameters)Parameters).IncludeMissing = false; }
                                break;
                            case "customheading":
                            if (!child.InnerText.Equals("none"))
                            {
                                this.Parameters.GadgetTitle = child.InnerText.Replace("&lt;", "<");
                                this.CustomOutputHeading = child.InnerText.Replace("&lt;", "<");
                            }
                            break;
                            case "customdescription":
                                if (!child.InnerText.Equals("(none)"))
                                {
                                    this.Parameters.GadgetDescription = child.InnerText.Replace("&lt;", "<");
                                    this.CustomOutputDescription = child.InnerText.Replace("&lt;", "<");
                                    if (!CustomOutputHeading.Equals("(none)"))
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
                            case "showannotations":
                                if (child.InnerText.ToLowerInvariant().Equals("true")) { ((PieChartParameters)Parameters).ShowAnnotations = true; }
                                else { ((PieChartParameters)Parameters).ShowAnnotations = false; }
                                break;
                            case "showannotationlabel":
                                if (child.InnerText.ToLowerInvariant().Equals("true")) { ((PieChartParameters)Parameters).ShowAnnotationLabel = true; }
                                else { ((PieChartParameters)Parameters).ShowAnnotationLabel = false; }
                                break;
                            case "showannotationvalue":
                                if (child.InnerText.ToLowerInvariant().Equals("true")) { ((PieChartParameters)Parameters).ShowAnnotationValue = true; }
                                else { ((PieChartParameters)Parameters).ShowAnnotationValue = false; }
                                break;
                            case "showannotationpercent":
                                if (child.InnerText.ToLowerInvariant().Equals("true")) { ((PieChartParameters)Parameters).ShowAnnotationPercent = true; }
                                else { ((PieChartParameters)Parameters).ShowAnnotationPercent = false; }
                                break;
                            case "annotationpercent":
                                ((PieChartParameters)Parameters).AnnotationPercent = double.Parse(child.InnerText.Replace("&lt;", "<"));
                                break;
                            case "chartkind":
                                {
                                    switch (child.InnerText)
                                    {
                                        case "0":
                                            ((PieChartParameters)Parameters).PieChartKind = PieChartKind.Pie2D;
                                            break;
                                        case "1":
                                            ((PieChartParameters)Parameters).PieChartKind = PieChartKind.Pie;
                                            break;
                                        case "2":
                                            ((PieChartParameters)Parameters).PieChartKind = PieChartKind.Donut2D;
                                            break;
                                        case "3":
                                            ((PieChartParameters)Parameters).PieChartKind = PieChartKind.Donut;
                                            break;
                                    }
                                }
                                break;
                            case "palette":
                               ((PieChartParameters)Parameters).Palette = int.Parse(child.InnerText);
                                break;
                            case "palettecolor1":
                                ((PieChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                                break;
                            case "palettecolor2":
                                ((PieChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                                break;
                            case "palettecolor3":
                                ((PieChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                                break;
                            case "palettecolor4":
                                ((PieChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                                break;
                            case "palettecolor5":
                                ((PieChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                                break;
                            case "palettecolor6":
                                ((PieChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                                break;
                            case "palettecolor7":
                                ((PieChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                                break;
                            case "palettecolor8":
                                ((PieChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                                break;
                            case "palettecolor9":
                                ((PieChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                                break;
                            case "palettecolor10":
                                ((PieChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                                break;
                            case "palettecolor11":
                                ((PieChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                                break;
                            case "palettecolor12":
                                ((PieChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                                break;
                            case "charttitle":
                                ((PieChartParameters)Parameters).ChartTitle = child.InnerText;
                                break;
                            case "chartsubtitle":
                                ((PieChartParameters)Parameters).ChartSubTitle = child.InnerText;
                                break;
                            case "showlegend":
                                if (child.InnerText.ToLowerInvariant().Equals("true")) { ((PieChartParameters)Parameters).ShowLegend = true; }
                                else { ((PieChartParameters)Parameters).ShowLegend = false; }
                                break;
                            case "showlegendborder":
                                if (child.InnerText.ToLowerInvariant().Equals("true")) { ((PieChartParameters)Parameters).ShowLegendBorder = true; }
                                else { ((PieChartParameters)Parameters).ShowLegendBorder = false; }
                                break;
                            case "showlegendvarnames":
                                if (child.InnerText.ToLowerInvariant().Equals("true")) { ((PieChartParameters)Parameters).ShowLegendVarNames = true; }
                                else { ((PieChartParameters)Parameters).ShowLegendVarNames = false; }
                                break;
                            case "legendfontsize":
                                ((PieChartParameters)Parameters).LegendFontSize = double.Parse(child.InnerText);
                                break;
                            case "legenddock":
                                {
                                    switch (int.Parse(child.InnerText))
                                    {
                                        case 0:
                                            ((PieChartParameters)Parameters).LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Left;
                                            break;
                                        default:
                                        case 1:
                                            ((PieChartParameters)Parameters).LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Right;
                                            break;
                                        case 2:
                                            ((PieChartParameters)Parameters).LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Top;
                                            break;
                                        case 3:
                                            ((PieChartParameters)Parameters).LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Bottom;
                                            break;
                                    }
                                }
                                break;
                            case "height":
                                ((PieChartParameters)Parameters).ChartHeight = double.Parse(child.InnerText);
                                break;
                            case "width":
                                ((PieChartParameters)Parameters).ChartWidth = double.Parse(child.InnerText);   
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
            PieChartParameters chtParameters = (PieChartParameters)Parameters;

            System.Xml.XmlElement element = doc.CreateElement("pieChartGadget");
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
            type.Value = "EpiDashboard.Gadgets.Charting.PieChartGadget";
            actualHeight.Value = this.ActualHeight.ToString();

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);
            element.Attributes.Append(id);
            if (IsCollapsed == false)
            {
                element.Attributes.Append(actualHeight);
            }

            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text; 
            
            string mainVar = string.Empty;
            string strataVar = string.Empty;
            string weightVar = string.Empty;
            string crosstabVar = string.Empty;
            string sort = string.Empty;
            bool allValues = false;
            bool showConfLimits = true;
            bool showCumulativePercent = true;
            bool includeMissing = false;

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

            //showAnnotations 
            XmlElement showAnnotationsElement = doc.CreateElement("showAnnotations");
            showAnnotationsElement.InnerText = chtParameters.ShowAnnotations.ToString();
            element.AppendChild(showAnnotationsElement);

            //showAnnotations 
            XmlElement showAnnotationLabelElement = doc.CreateElement("showAnnotationLabel");
            showAnnotationLabelElement.InnerText = chtParameters.ShowAnnotationLabel.ToString();
            element.AppendChild(showAnnotationLabelElement);

            //showAnnotations 
            XmlElement showAnnotationValueElement = doc.CreateElement("showAnnotationValue");
            showAnnotationValueElement.InnerText = chtParameters.ShowAnnotationValue.ToString();
            element.AppendChild(showAnnotationValueElement);

            //showAnnotations 
            XmlElement showAnnotationPercentElement = doc.CreateElement("showAnnotationPercent");
            showAnnotationPercentElement.InnerText = chtParameters.ShowAnnotationPercent.ToString();
            element.AppendChild(showAnnotationPercentElement);

            //chartKind
            XmlElement chartKindElement = doc.CreateElement("chartKind");
            switch (chtParameters.PieChartKind)
            {
                case PieChartKind.Pie2D:
                    chartKindElement.InnerText = "0";
                    break;
                case PieChartKind.Pie:
                    chartKindElement.InnerText = "1";
                    break;
                case PieChartKind.Donut2D:
                    chartKindElement.InnerText = "2";
                    break;
                case PieChartKind.Donut:
                    chartKindElement.InnerText = "3";
                    break;
            }
            element.AppendChild(chartKindElement);

            //palette 
            XmlElement paletteElement = doc.CreateElement("palette");
            paletteElement.InnerText = chtParameters.Palette.ToString();
            element.AppendChild(paletteElement);
            //Palette Colors
            for (int i = 0; i < chtParameters.PaletteColors.Count(); i++)
            {
                XmlElement palettecolorsElement = doc.CreateElement("paletteColor" + (i + 1));
                palettecolorsElement.InnerText = chtParameters.PaletteColors[i].ToString();
                element.AppendChild(palettecolorsElement);
            }
            //annotation Percent
            XmlElement annotationPercentElement = doc.CreateElement("annotationPercent");
            annotationPercentElement.InnerText = chtParameters.AnnotationPercent.ToString();
            element.AppendChild(annotationPercentElement);

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

            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            xmlString = xmlString + SerializeAnchors();

            return element;
        }

        private class XYChartData
        {
            public object X { get; set; }
            public double Y { get; set; }
            public object S { get; set; }
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

        #endregion //Public Methods

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0, bool useAlternatingColors = false)
        {
            PieChartParameters chtParameters = (PieChartParameters)Parameters;
            if (chtParameters.ColumnNames.Count < 1) return string.Empty;	

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<h2>" + chtParameters.ChartTitle + "</h2>");
            sb.AppendLine("<h3>" + chtParameters.ChartSubTitle + "</h3>");

            foreach (UIElement element in panelMain.Children)
            {
                if (element is Controls.Charting.PieChart)
                {
                    sb.AppendLine(((Controls.Charting.PieChart)element).ToHTML(htmlFileName, count, true, false));
                    count++;
                }
            }
            return sb.ToString();
        }
        Controls.GadgetProperties.PieChartProperties properties = null;
        public override void ShowHideConfigPanel()
        {
            Popup = new DashboardPopup();
            Popup.Parent = ((this.Parent as DragCanvas).Parent as ScrollViewer).Parent as Grid;
            properties = new Controls.GadgetProperties.PieChartProperties(this.DashboardHelper, this, (PieChartParameters)Parameters, StrataGridList);

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

            if (properties == null)
                properties = new Controls.GadgetProperties.PieChartProperties(this.DashboardHelper, this, (PieChartParameters)Parameters, StrataGridList);
            properties.Height = (Convert.ToInt32(i_StandardHeight * f_HeightRatio)) / 1.07;
            properties.Width = (Convert.ToInt32(i_StandardWidth * f_WidthRatio)) / 1.07;

        }
    }
}
