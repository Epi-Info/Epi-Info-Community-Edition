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
    /// Interaction logic for ColumnChartGadget.xaml
    /// </summary>
    public partial class LineChartGadget : ChartGadgetBase
    {
        #region Constructors

        private bool IsDropDownList { get; set; }
        private bool IsCommentLegal { get; set; }
        private bool IsRecoded { get; set; }
        private bool IsOptionField { get; set; }

        /// A custom heading to use for this gadget's output
        /// </summary>
        private string customOutputHeading;

        /// <summary>
        /// A custom description to use for this gadget's output
        /// </summary>
        private string customOutputDescription;

        public LineChartGadget()
        {
            InitializeComponent();
            Construct();
        }

        public LineChartGadget(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            Construct();
        }
        #endregion //Constructors

        #region Private and Protected Methods

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            if (LoadingCombos)
            {
                return;
            }

            RefreshResults();
        }

        protected override void Construct()
        {
            this.Parameters = new LineChartParameters();

            if (!string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)"))
            {
                headerPanel.Text = CustomOutputHeading;
            }

            StrataGridList = new List<Grid>();
            StrataExpanderList = new List<Expander>();

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

        protected override void SetChartData(List<XYColumnChartData> dataList, Strata strata)
        {
            #region Input Validation
            if (dataList == null)
            {
                throw new ArgumentNullException("dataList");
            }
            #endregion // Input Validation

            LineChartParameters LCParameters = (LineChartParameters)Parameters;

            if (dataList.Count > 0)
            {
                if (strata != null)
                {
                    LCParameters.ChartStrataTitle = strata.Filter;
                }
                else
                {
                    LCParameters.ChartStrataTitle = String.Empty;
                }
                EpiDashboard.Controls.Charting.LineChart lineChart = new EpiDashboard.Controls.Charting.LineChart(DashboardHelper, LCParameters, dataList);
                lineChart.Margin = new Thickness(0, 0, 0, 16);
                lineChart.MouseEnter += new MouseEventHandler(chart_MouseEnter);
                lineChart.MouseLeave += new MouseEventHandler(chart_MouseLeave);
                panelMain.Children.Add(lineChart);
            }
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

        private void properties_ChangesAccepted(object sender, EventArgs e)
        {
            Controls.GadgetProperties.LineChartProperties properties = Popup.Content as Controls.GadgetProperties.LineChartProperties;
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
                LineChartParameters LineChartParameters = (LineChartParameters)Parameters;

                string freqVar = LineChartParameters.ColumnNames[0];
                string weightVar = LineChartParameters.WeightVariableName;
                string crosstabVar = LineChartParameters.CrosstabVariableName;

                bool includeMissing = LineChartParameters.IncludeMissing;

                List<string> stratas = new List<string>();

                try
                {
                    RequestUpdateStatusDelegate requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
                    CheckForCancellationDelegate checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

                    LineChartParameters.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                    LineChartParameters.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);

                    if (this.DataFilters != null && this.DataFilters.Count > 0)
                    {
                        LineChartParameters.CustomFilter = this.DataFilters.GenerateDataFilterString(false);
                    }
                    else
                    {
                        LineChartParameters.CustomFilter = string.Empty;
                    }

                    if (!string.IsNullOrEmpty(crosstabVar.Trim()))
                    {
                        List<string> crosstabVarList = new List<string>();
                        crosstabVarList.Add(crosstabVar);

                        foreach (Strata strata in DashboardHelper.GetStrataValuesAsDictionary(crosstabVarList, false, false))
                        {
                            LineChartParameters parameters = new LineChartParameters(LineChartParameters);

                            if (!string.IsNullOrEmpty(parameters.CustomFilter))
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
                        Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables = DashboardHelper.GenerateFrequencyTable(LineChartParameters);
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
                    Debug.Print("Line chart gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete.");
                    Debug.Print(DashboardHelper.DataFilters.GenerateDataFilterString());
                }
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
            LineChartParameters LineChartParameters = (LineChartParameters)Parameters;
            if (!LoadingCombos)
            {
                if (LineChartParameters != null)
                {
                    if (LineChartParameters.ColumnNames.Count > 0 && !String.IsNullOrEmpty(LineChartParameters.ColumnNames[0]))
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
                switch (child.Name.ToLowerInvariant())
                {
                    case "allvalues":
                        if (child.InnerText.ToLowerInvariant().Equals("true"))
                        {
                            ((LineChartParameters)Parameters).ShowAllListValues = true;
                        }
                        else
                        {
                            ((LineChartParameters)Parameters).ShowAllListValues = false;
                        }
                        break;
                    //case "horizontalgridlines":
                    //    if (child.InnerText.ToLowerInvariant().Equals("true"))
                    //    {
                    //        checkboxShowHorizontalGridLines.IsChecked = true;
                    //    }
                    //    else
                    //    {
                    //        checkboxShowHorizontalGridLines.IsChecked = false;
                    //    }
                    //    break;
                    
                    case "singlevariable":
                        if ((this.Parameters.ColumnNames.Count > 0) && (!String.IsNullOrEmpty(child.InnerText.Trim())))
                        {
                            ((LineChartParameters)Parameters).ColumnNames[0] = child.InnerText.Replace("&lt;", "<");
                        }
                        break;
                    case "weightvariable":
                        if (!String.IsNullOrEmpty(child.InnerText.Trim()))
                        {
                            ((LineChartParameters)Parameters).WeightVariableName = child.InnerText.Replace("&lt;", "<");
                        }
                        break;
                    case "stratavariable":
                        if (((LineChartParameters)Parameters).StrataVariableNames.Count > 0)
                        {
                            ((LineChartParameters)Parameters).StrataVariableNames[0] = child.InnerText.Replace("&lt;", "<");
                        }
                        else
                        {
                            ((LineChartParameters)Parameters).StrataVariableNames.Add(child.InnerText.Replace("&lt;", "<"));
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
                                ((LineChartParameters)Parameters).ChartWidth = 400;
                                ((LineChartParameters)Parameters).ChartHeight = 250;
                                break;
                            case "Medium":
                                ((LineChartParameters)Parameters).ChartWidth = 533;
                                ((LineChartParameters)Parameters).ChartHeight = 333;
                                break;
                            case "Large":
                                ((LineChartParameters)Parameters).ChartWidth = 800;
                                ((LineChartParameters)Parameters).ChartHeight = 500;
                                break;
                            case "Custom":
                                break;
                        }
                        break;
                    case "chartwidth":
                        if (!string.IsNullOrEmpty(child.InnerText))
                        {
                            ((LineChartParameters)Parameters).ChartWidth = int.Parse(child.InnerText.Replace("&lt;", "<"));
                        }
                        break;
                    case "chartheight":
                        if (!string.IsNullOrEmpty(child.InnerText))
                        {
                            ((LineChartParameters)Parameters).ChartHeight = int.Parse(child.InnerText.Replace("&lt;", "<"));
                        }
                        break;
                    case "charttitle":
                        if (!string.IsNullOrEmpty(child.InnerText))
                        {
                            ((LineChartParameters)Parameters).ChartTitle = child.InnerText.Replace("&lt;", "<");
                        }
                        break;
                    case "xaxislabel":
                        if (!string.IsNullOrEmpty(child.InnerText))
                        {
                            ((LineChartParameters)Parameters).XAxisLabel = child.InnerText.Replace("&lt;", "<");
                        }
                        break;
                    case "yaxislabel":
                        if (!string.IsNullOrEmpty(child.InnerText))
                        {
                            ((LineChartParameters)Parameters).YAxisLabel = child.InnerText.Replace("&lt;", "<");
                        }
                        break;
                    case "xaxisrotation":
                        switch (child.InnerText)
                        {
                            case "0":
                                ((LineChartParameters)Parameters).XAxisAngle = 0;
                                break;
                            case "45":
                                ((LineChartParameters)Parameters).XAxisAngle = -45;
                                break;
                            case "90":
                                ((LineChartParameters)Parameters).XAxisAngle = -90;
                                break;
                            default:
                                ((LineChartParameters)Parameters).XAxisAngle = 0;
                                break;
                        }
                        break;
                }
            }
        }

        public override void CreateFromXml(XmlElement element)
        {
            this.LoadingCombos = true;
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
                    if (!string.IsNullOrEmpty(child.InnerText))
                    {
                        switch (child.Name.ToLowerInvariant())
                        {
                            case "mainvariable":
                                if (this.Parameters.ColumnNames.Count > 0)
                                {
                                    ((LineChartParameters)Parameters).ColumnNames[0] = child.InnerText.Replace("&lt;", "<");
                                }
                                else
                                {
                                    ((LineChartParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                                }
                                break;
                            case "stratavariable":
                                if (((LineChartParameters)Parameters).StrataVariableNames.Count > 0)
                                {
                                    ((LineChartParameters)Parameters).StrataVariableNames[0] = child.InnerText.Replace("&lt;", "<");
                                }
                                else
                                {
                                    ((LineChartParameters)Parameters).StrataVariableNames.Add(child.InnerText.Replace("&lt;", "<"));
                                }
                                break;
                            case "stratavariables":
                                foreach (XmlElement field in child.ChildNodes)
                                {
                                    List<string> fields = new List<string>();
                                    if (field.Name.ToLowerInvariant().Equals("stratavariable"))
                                    {
                                        ((LineChartParameters)Parameters).StrataVariableNames.Add(field.InnerText.Replace("&lt;", "<"));
                                    }
                                }
                                break;
                            case "weightvariable":
                                ((LineChartParameters)Parameters).WeightVariableName = child.InnerText.Replace("&lt;", "<");
                                break;
                            case "crosstabvariable":
                                ((LineChartParameters)Parameters).CrosstabVariableName = child.InnerText.Replace("&lt;", "<");
                                break;
                            case "secondyvar":
                                if (this.Parameters.ColumnNames.Count > 1)
                                {
                                    ((LineChartParameters)Parameters).ColumnNames[1] = (child.InnerText.Replace("&lt;", "<"));
                                }
                                else
                                {
                                    ((LineChartParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                                }
                                break;
                            case "secondyvartype":
//                                Y2Type y2Type = ((Y2Type)Int32.Parse(child.InnerText));
                                Y2Type y2Type = ((Y2Type)Int32.Parse("0"));
//                                ((LineChartParameters)Parameters).Y2AxisType = int.Parse(child.InnerText.Replace("&lt;", "<"));
                                ((LineChartParameters)Parameters).Y2AxisType = int.Parse("0");
                                break;
                            case "sort":
                                if (child.InnerText.ToLowerInvariant().Equals("highlow") || child.InnerText.ToLowerInvariant().Equals("hightolow"))
                                {
                                    ((LineChartParameters)Parameters).SortHighToLow = true;
                                }

                                break;
                            case "allvalues":
                                if (child.InnerText.ToLowerInvariant().Equals("true"))
                                {
                                    ((LineChartParameters)Parameters).ShowAllListValues = true;
                                }
                                else { ((LineChartParameters)Parameters).ShowAllListValues = false; }
                                break;
                            case "showlistlabels":
                                if (child.InnerText.ToLowerInvariant().Equals("true"))
                                {
                                    ((LineChartParameters)Parameters).ShowCommentLegalLabels = true;
                                }
                                else { ((LineChartParameters)Parameters).ShowCommentLegalLabels = false; }
                                break;
                            case "includemissing":
                                if (child.InnerText.ToLowerInvariant().Equals("true")) { ((LineChartParameters)Parameters).IncludeMissing = true; }
                                else { ((LineChartParameters)Parameters).IncludeMissing = false; }
                                break;
                            case "customheading":
                                if (!child.InnerText.Equals("(none)"))
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
                                if (!child.InnerText.Equals("(none)"))
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
                            case "userefvalues":
                                if (child.InnerText.ToLowerInvariant().Equals("true")) { ((LineChartParameters)Parameters).UseRefValues = true; }
                                else { ((LineChartParameters)Parameters).UseRefValues = false; }
                                break;
                            case "showannotations":
                                if (child.InnerText.ToLowerInvariant().Equals("true")) { ((LineChartParameters)Parameters).ShowAnnotations = true; }
                                else { ((LineChartParameters)Parameters).ShowAnnotations = false; }
                                break;
                            case "y2showannotations":
                                if (child.InnerText.ToLowerInvariant().Equals("true")) { ((LineChartParameters)Parameters).Y2ShowAnnotations = true; }
                                else { ((LineChartParameters)Parameters).Y2ShowAnnotations = false; }
                                break;
                            case "showgridlines":
                                if (child.InnerText.ToLowerInvariant().Equals("true")) { ((LineChartParameters)Parameters).ShowGridLines = true; }
                                else { ((LineChartParameters)Parameters).ShowGridLines = false; }
                                break;
                            case "palettecolor1":
                                ((LineChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                                break;
                            case "palettecolor2":
                                ((LineChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                                break;
                            case "palettecolor3":
                                ((LineChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                                break;
                            case "palettecolor4":
                                ((LineChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                                break;
                            case "palettecolor5":
                                ((LineChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                                break;
                            case "palettecolor6":
                                ((LineChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                                break;
                            case "palettecolor7":
                                ((LineChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                                break;
                            case "palettecolor8":
                                ((LineChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                                break;
                            case "palettecolor9":
                                ((LineChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                                break;
                            case "palettecolor10":
                                ((LineChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                                break;
                            case "palettecolor11":
                                ((LineChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                                break;
                            case "palettecolor12":
                                ((LineChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                                break;
                            case "palette":
                                ((LineChartParameters)Parameters).Palette = int.Parse(child.InnerText);
                                break;
                            case "linetype":
                                string linetypeText = getEnumText(child.InnerText);
                                switch (linetypeText)
                                {
                                    case "Polygon":
                                        ((LineChartParameters)Parameters).LineKind = LineKind.Polygon;
                                        break;
                                    case "Smooth":
                                        ((LineChartParameters)Parameters).LineKind = LineKind.Smooth;
                                        break;
                                    case "Step":
                                        ((LineChartParameters)Parameters).LineKind = LineKind.Step;
                                        break;
                                    case "Auto":
                                        ((LineChartParameters)Parameters).LineKind = LineKind.Auto;
                                        break;
                                    default:
                                        ((LineChartParameters)Parameters).LineKind = LineKind.Auto;
                                        break;
                                }
                                break;
                            case "linethickness":
                                ((LineChartParameters)Parameters).LineThickness = double.Parse(child.InnerText);
                                break;
                            case "legenddock":
                                {
                                    switch (int.Parse(child.InnerText))
                                    {
                                        case 0:
                                            ((LineChartParameters)Parameters).LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Left;
                                            break;
                                        default:
                                        case 1:
                                            ((LineChartParameters)Parameters).LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Right;
                                            break;
                                        case 2:
                                            ((LineChartParameters)Parameters).LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Top;
                                            break;
                                        case 3:
                                            ((LineChartParameters)Parameters).LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Bottom;
                                            break;
                                    }
                                }
                                break;
                            case "y2linetype":
                                switch (child.InnerText)
                                {
                                    case "Polygon":
                                        ((LineChartParameters)Parameters).Y2LineKind = LineKind.Polygon;
                                        break;
                                    case "Smooth":
                                        ((LineChartParameters)Parameters).Y2LineKind = LineKind.Smooth;
                                        break;
                                    case "Step":
                                        ((LineChartParameters)Parameters).Y2LineKind = LineKind.Step;
                                        break;
                                    case "Auto":
                                        ((LineChartParameters)Parameters).Y2LineKind = LineKind.Auto;
                                        break;
                                }
                                break;
                            case "y2linedashstyle":
                                {
                                    switch (child.InnerText)
                                    {
                                        case "Solid":
                                            ((LineChartParameters)Parameters).Y2LineDashStyle = LineDashStyle.Solid;
                                            break;
                                        case "Dash":
                                            ((LineChartParameters)Parameters).Y2LineDashStyle = LineDashStyle.Dash;
                                            break;
                                        case "Dot":
                                            ((LineChartParameters)Parameters).Y2LineDashStyle = LineDashStyle.Dot;
                                            break;
                                        case "DashDot":
                                            ((LineChartParameters)Parameters).Y2LineDashStyle = LineDashStyle.DashDot;
                                            break;
                                        case "DashDotDot":
                                            ((LineChartParameters)Parameters).Y2LineDashStyle = LineDashStyle.DashDotDot;
                                            break;
                                    }
                                }
                                break;
                            case "y2linethickness":
                                ((LineChartParameters)Parameters).Y2LineThickness = double.Parse(child.InnerText);
                                break;
                            case "yaxislabel":
                                ((LineChartParameters)Parameters).YAxisLabel = child.InnerText;
                                break;
                            case "yaxisformatstring":
                                ((LineChartParameters)Parameters).YAxisFormat = child.InnerText;
                                break;
                            case "y2axisformatstring":
                                ((LineChartParameters)Parameters).Y2AxisFormat = child.InnerText;
                                break;
                            case "xaxislabeltype":
                                ((LineChartParameters)Parameters).XAxisLabelType = int.Parse(child.InnerText);
                                break;
                            case "xaxislabel":
                                ((LineChartParameters)Parameters).XAxisLabel = child.InnerText;
                                break;
                            case "xaxisangle":
                                ((LineChartParameters)Parameters).XAxisAngle = int.Parse(child.InnerText);
                                break;
                            case "charttitle":
                                ((LineChartParameters)Parameters).ChartTitle = child.InnerText;
                                break;
                            case "chartsubtitle":
                                ((LineChartParameters)Parameters).ChartSubTitle = child.InnerText;
                                break;
                            case "showlegend":
                                if (child.InnerText.ToLowerInvariant().Equals("true")) { ((LineChartParameters)Parameters).ShowLegend = true; }
                                else { ((LineChartParameters)Parameters).ShowLegend = false; }
                                break;
                            case "showlegendborder":
                                if (child.InnerText.ToLowerInvariant().Equals("true")) { ((LineChartParameters)Parameters).ShowLegendBorder = true; }
                                else { ((LineChartParameters)Parameters).ShowLegendBorder = false; }
                                break;
                            case "showlegendvarnames":
                                if (child.InnerText.ToLowerInvariant().Equals("true")) { ((LineChartParameters)Parameters).ShowLegendVarNames = true; }
                                else { ((LineChartParameters)Parameters).ShowLegendVarNames = false; }
                                break;
                            case "legendfontsize":
                                ((LineChartParameters)Parameters).LegendFontSize = int.Parse(child.InnerText);
                                break;
                            case "height":
                                ((LineChartParameters)Parameters).ChartHeight = double.Parse(child.InnerText);
                                break;
                            case "width":
                                ((LineChartParameters)Parameters).ChartWidth = double.Parse(child.InnerText);
                                break;
                            //EI-98
                            case "yaxislabelfontsize":
                                ((LineChartParameters)Parameters).YAxisLabelFontSize = double.Parse(child.InnerText);
                                break;
                            case "xaxislabelfontsize":
                                ((LineChartParameters)Parameters).XAxisLabelFontSize = double.Parse(child.InnerText);
                                break;
                            case "yaxisfontsize":
                                ((LineChartParameters)Parameters).YAxisFontSize = double.Parse(child.InnerText);
                                break;
                            case "xaxisfontsize":
                                ((LineChartParameters)Parameters).XAxisFontSize = double.Parse(child.InnerText);
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
        /// JIRA EI-429 - to maintain compatibility with canvases created with 7.1.5 and earlier 
        /// the enum for LineKind from the 2011 release of the Component Art library is 
        /// re-created here.  
        /// 
        /// Reference -- 
        ///   http://docs.componentart.com/default.aspx?content=ComponentArt.Web.Visualization.Charting.server/ComponentArt.Web.Visualization.Charting~ComponentArt.Web.Visualization.Charting.LineKind.html#WinDataVisualization/2011/ComponentArt.Win.DataVisualization.API/ComponentArt.Win.DataVisualization.Charting~ComponentArt.Win.DataVisualization.Charting.LineKind.html
        /// </summary>
        private enum LineKind2011
        {
            Auto = 0,
            Polygon = 1,
            Smooth = 2,
            Step = 3
        }

        /// <summary>
        /// If the  testTextFromXml can parse into ah integer we assume that the canvas file was create 
        /// with Component Art 2011.  So use the LineKind2011 enum to determine the line type     
        /// Otherwise just return the text            
        /// </summary>
        /// <param name="testTextFromXml"></param>
        /// <returns></returns>
        private string getEnumText(string testTextFromXml)
        {
            int enumAsNum;
            string enumAsText;

            bool result = Int32.TryParse(testTextFromXml, out enumAsNum);
            // if true cast the  int as appropriate enum      
            if (result)
            {
                LineKind2011 lk = (LineKind2011)enumAsNum;
                enumAsText = lk.ToString();
                return enumAsText;
            }
            //  else just return orig text       
            else
            {
                enumAsText = testTextFromXml;
                return enumAsText;
            }

        }

        /// <summary>
        /// Serializes the gadget into Xml
        /// </summary>
        /// <param name="doc">The Xml docment</param>
        /// <returns>XmlNode</returns>
        public override XmlNode Serialize(XmlDocument doc)
        {
            LineChartParameters LineChartParameters = (LineChartParameters)Parameters;

            System.Xml.XmlElement element = doc.CreateElement("LineChartGadget");
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
            type.Value = "EpiDashboard.Gadgets.Charting.LineChartGadget";
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
            string crosstabVar = string.Empty;
            string weightVar = string.Empty;
            string second_y_var = string.Empty;
            string sort = string.Empty;

            double height = 600;
            double width = 800;

            double.TryParse(LineChartParameters.ChartHeight.ToString(), out height);
            double.TryParse(LineChartParameters.ChartWidth.ToString(), out width);

            crosstabVar = LineChartParameters.CrosstabVariableName.Replace("<", "&lt;");

            //mainVariable
            XmlElement freqVarElement = doc.CreateElement("mainVariable");
            if (LineChartParameters.ColumnNames.Count > 0)
            {
                if (!String.IsNullOrEmpty(LineChartParameters.ColumnNames[0].ToString()))
                {
                    freqVarElement.InnerText = LineChartParameters.ColumnNames[0].ToString().Replace("<", "&lt;");
                    element.AppendChild(freqVarElement);
                }
            }
            // =========  Former Advanced Options section  ============
            //weightVariable
            XmlElement weightVariableElement = doc.CreateElement("weightVariable");
            if (!String.IsNullOrEmpty(LineChartParameters.WeightVariableName))
            {
                weightVariableElement.InnerText = LineChartParameters.WeightVariableName.Replace("<", "&lt;");
                element.AppendChild(weightVariableElement);
            }

            //Cross Tab Variable
            XmlElement crossTabVarElement = doc.CreateElement("crosstabVariable");
            if (!String.IsNullOrEmpty(LineChartParameters.CrosstabVariableName))
            {
                crossTabVarElement.InnerText = LineChartParameters.CrosstabVariableName.Replace("<", "&lt;");
                element.AppendChild(crossTabVarElement);
            }

            //strataVariables
            XmlElement StrataVariableNameElement = doc.CreateElement("strataVariable");
            XmlElement StrataVariableNamesElement = doc.CreateElement("strataVariables");
            if (LineChartParameters.StrataVariableNames.Count == 1)
            {
                StrataVariableNameElement.InnerText = LineChartParameters.StrataVariableNames[0].ToString().Replace("<", "&lt;");
                element.AppendChild(StrataVariableNameElement);
            }
            else if (LineChartParameters.StrataVariableNames.Count > 1)
            {
                foreach (string strataColumn in LineChartParameters.StrataVariableNames)
                {
                    XmlElement strataElement = doc.CreateElement("strataVariable");
                    strataElement.InnerText = strataColumn.Replace("<", "&lt;");
                    StrataVariableNamesElement.AppendChild(strataElement);
                }

                element.AppendChild(StrataVariableNamesElement);
            }

            //secondYVarType
            XmlElement secondYVarTypeElement = doc.CreateElement("secondYVarType");
            secondYVarTypeElement.InnerText = LineChartParameters.Y2AxisType.ToString().Replace("<", "&lt;");
            element.AppendChild(secondYVarTypeElement);

            //secondYVar
            XmlElement secondYVarElement = doc.CreateElement("secondYVar");
            if (LineChartParameters.ColumnNames.Count > 1)
            {
                if (!String.IsNullOrEmpty(LineChartParameters.ColumnNames[1].ToString()))
                {
                    secondYVarElement.InnerText = LineChartParameters.ColumnNames[1].ToString().Replace("<", "&lt;");
                    element.AppendChild(secondYVarElement);
                }
            }

            //height 
            XmlElement heightElement = doc.CreateElement("height");
            heightElement.InnerText = LineChartParameters.ChartHeight.ToString().Replace("<", "&lt;");
            element.AppendChild(heightElement);

            //width 
            XmlElement widthElement = doc.CreateElement("width");
            widthElement.InnerText = LineChartParameters.ChartWidth.ToString().Replace("<", "&lt;");
            element.AppendChild(widthElement);

            //showAllListValues
            XmlElement allValuesElement = doc.CreateElement("allValues");
            allValuesElement.InnerText = LineChartParameters.ShowAllListValues.ToString().Replace("<", "&lt;");
            element.AppendChild(allValuesElement);

            //showListLabels
            XmlElement showListLabelsElement = doc.CreateElement("showListLabels");
            showListLabelsElement.InnerText = LineChartParameters.ShowCommentLegalLabels.ToString().Replace("<", "&lt;");
            element.AppendChild(showListLabelsElement);

            //sort
            XmlElement sortElement = doc.CreateElement("sort");
            if (LineChartParameters.SortHighToLow) sortElement.InnerText = "hightolow";
            element.AppendChild(sortElement);

            //includeMissing
            XmlElement includeMissingElement = doc.CreateElement("includeMissing");
            includeMissingElement.InnerText = LineChartParameters.IncludeMissing.ToString();
            element.AppendChild(includeMissingElement);

            //customHeading
            XmlElement customHeadingElement = doc.CreateElement("customHeading");
            customHeadingElement.InnerText = LineChartParameters.GadgetTitle.Replace("<", "&lt;");
            element.AppendChild(customHeadingElement);

            //customDescription
            XmlElement customDescriptionElement = doc.CreateElement("customDescription");
            customDescriptionElement.InnerText = LineChartParameters.GadgetDescription.Replace("<", "&lt;");
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

            //useRefValues 
            XmlElement useRefValuesElement = doc.CreateElement("useRefValues");
            useRefValuesElement.InnerText = LineChartParameters.UseRefValues.ToString();
            element.AppendChild(useRefValuesElement);

            //showAnnotations 
            XmlElement showAnnotationsElement = doc.CreateElement("showAnnotations");
            showAnnotationsElement.InnerText = LineChartParameters.ShowAnnotations.ToString();
            element.AppendChild(showAnnotationsElement);

            //y2showAnnotations 
            XmlElement y2showAnnotationsElement = doc.CreateElement("y2showAnnotations");
            y2showAnnotationsElement.InnerText = LineChartParameters.Y2ShowAnnotations.ToString();
            element.AppendChild(y2showAnnotationsElement);

            //showGridLines 
            XmlElement showGridLinesElement = doc.CreateElement("showGridLines");
            showGridLinesElement.InnerText = LineChartParameters.ShowGridLines.ToString();
            element.AppendChild(showGridLinesElement);

            //palette 
            XmlElement paletteElement = doc.CreateElement("palette");
            paletteElement.InnerText = LineChartParameters.Palette.ToString();
            element.AppendChild(paletteElement);
            //Palette Colors
            for (int i = 0; i < LineChartParameters.PaletteColors.Count(); i++)
            {
                XmlElement palettecolorsElement = doc.CreateElement("paletteColor" + (i + 1));
                palettecolorsElement.InnerText = LineChartParameters.PaletteColors[i].ToString();
                element.AppendChild(palettecolorsElement);
            }
            //lineType
            XmlElement lineTypeElement = doc.CreateElement("lineType");
            lineTypeElement.InnerText = LineChartParameters.LineKind.ToString();
            element.AppendChild(lineTypeElement);

            //lineThickness
            XmlElement lineThicknessElement = doc.CreateElement("lineThickness");
            lineThicknessElement.InnerText = LineChartParameters.LineThickness.ToString();
            element.AppendChild(lineThicknessElement);

            //y2LineType 
            XmlElement y2LineTypeElement = doc.CreateElement("y2LineType");
            y2LineTypeElement.InnerText = LineChartParameters.Y2LineKind.ToString();
            element.AppendChild(y2LineTypeElement);

            //y2LineDashStyle 
            XmlElement y2LineDashStyleElement = doc.CreateElement("y2LineDashStyle");
            y2LineDashStyleElement.InnerText = LineChartParameters.Y2LineDashStyle.ToString();
            element.AppendChild(y2LineDashStyleElement);

            //y2LineThickness 
            XmlElement y2LineThicknessElement = doc.CreateElement("y2LineThickness");
            y2LineThicknessElement.InnerText = LineChartParameters.Y2LineThickness.ToString();
            element.AppendChild(y2LineThicknessElement);

            //yAxisLabel 
            XmlElement yAxisLabelElement = doc.CreateElement("yAxisLabel");
            yAxisLabelElement.InnerText = LineChartParameters.YAxisLabel.ToString().Replace("<", "&lt;");
            element.AppendChild(yAxisLabelElement);

            //yAxisFormatString 
            XmlElement yAxisFormatStringElement = doc.CreateElement("yAxisFormatString");
            yAxisFormatStringElement.InnerText = LineChartParameters.YAxisFormat.ToString().Replace("<", "&lt;");
            element.AppendChild(yAxisFormatStringElement);

            //y2AxisLabel 
            XmlElement y2AxisLabelElement = doc.CreateElement("y2AxisLabel");
            y2AxisLabelElement.InnerText = LineChartParameters.Y2AxisLabel.ToString().Replace("<", "&lt;");
            element.AppendChild(y2AxisLabelElement);

            //y2AxisLegendTitle 
            XmlElement y2AxisLegendTitleElement = doc.CreateElement("y2AxisLegendTitle");
            y2AxisLegendTitleElement.InnerText = LineChartParameters.Y2AxisLegendTitle.ToString().Replace("<", "&lt;");
            element.AppendChild(y2AxisLegendTitleElement);

            //y2AxisFormatString 
            XmlElement y2AxisFormatStringElement = doc.CreateElement("y2AxisFormatString");
            y2AxisFormatStringElement.InnerText = LineChartParameters.Y2AxisFormat.ToString().Replace("<", "&lt;");
            element.AppendChild(y2AxisFormatStringElement);

            //xAxisLabelType 
            XmlElement xAxisLabelTypeElement = doc.CreateElement("xAxisLabelType");
            xAxisLabelTypeElement.InnerText = LineChartParameters.XAxisLabelType.ToString().Replace("<", "&lt;");
            element.AppendChild(xAxisLabelTypeElement);

            //xAxisLabel 
            XmlElement xAxisLabelElement = doc.CreateElement("xAxisLabel");
            xAxisLabelElement.InnerText = LineChartParameters.XAxisLabel.ToString().Replace("<", "&lt;");
            element.AppendChild(xAxisLabelElement);

            //xAxisAngle 
            XmlElement xAxisAngleElement = doc.CreateElement("xAxisAngle");
            xAxisAngleElement.InnerText = LineChartParameters.XAxisAngle.ToString().Replace("<", "&lt;");
            element.AppendChild(xAxisAngleElement);

            //chartTitle 
            XmlElement chartTitleElement = doc.CreateElement("chartTitle");
            chartTitleElement.InnerText = LineChartParameters.ChartTitle.ToString().Replace("<", "&lt;");
            element.AppendChild(chartTitleElement);

            //chartSubTitle 
            XmlElement chartSubTitleElement = doc.CreateElement("chartSubTitle");
            chartSubTitleElement.InnerText = LineChartParameters.ChartSubTitle.ToString().Replace("<", "&lt;");
            element.AppendChild(chartSubTitleElement);

            //showLegend 
            XmlElement showLegendElement = doc.CreateElement("showLegend");
            showLegendElement.InnerText = LineChartParameters.ShowLegend.ToString().Replace("<", "&lt;");
            element.AppendChild(showLegendElement);

            //showLegendBorder 
            XmlElement showLegendBorderElement = doc.CreateElement("showLegendBorder");
            showLegendBorderElement.InnerText = LineChartParameters.ShowLegendBorder.ToString();
            element.AppendChild(showLegendBorderElement);

            //showLegendVarNames 
            XmlElement showLegendVarNamesElement = doc.CreateElement("showLegendVarNames");
            showLegendVarNamesElement.InnerText = LineChartParameters.ShowLegendVarNames.ToString();
            element.AppendChild(showLegendVarNamesElement);

            //legendFontSize 
            XmlElement legendFontSizeElement = doc.CreateElement("legendFontSize");
            legendFontSizeElement.InnerText = LineChartParameters.LegendFontSize.ToString();
            element.AppendChild(legendFontSizeElement);

            //EI-98
            //yAxisLabelFontSize 
            XmlElement yAxisLabelFontSizeElement = doc.CreateElement("yAxisLabelFontSize");
            yAxisLabelFontSizeElement.InnerText = LineChartParameters.YAxisLabelFontSize.ToString().Replace("<", "&lt;");
            element.AppendChild(yAxisLabelFontSizeElement);

            //xAxisLabelFontSize 
            XmlElement xAxisLabelFontSize = doc.CreateElement("xAxisLabelFontSize");
            xAxisLabelFontSize.InnerText = LineChartParameters.XAxisLabelFontSize.ToString().Replace("<", "&lt;");
            element.AppendChild(xAxisLabelFontSize);

            //yAxisFontSize 
            XmlElement yAxisFontSizeElement = doc.CreateElement("yAxisFontSize");
            yAxisFontSizeElement.InnerText = LineChartParameters.YAxisFontSize.ToString().Replace("<", "&lt;");
            element.AppendChild(yAxisFontSizeElement);

            //xAxisFontSize 
            XmlElement xAxisFontSize = doc.CreateElement("xAxisFontSize");
            xAxisFontSize.InnerText = LineChartParameters.XAxisFontSize.ToString().Replace("<", "&lt;");
            element.AppendChild(xAxisFontSize);

            //legendDock 
            XmlElement legendDockElement = doc.CreateElement("legendDock");
            switch (LineChartParameters.LegendDock)
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

        Controls.GadgetProperties.LineChartProperties properties = null;
        public override void ShowHideConfigPanel()
        {
            Popup = new DashboardPopup();
            Popup.Parent = ((this.Parent as DragCanvas).Parent as ScrollViewer).Parent as Grid;
            properties = new Controls.GadgetProperties.LineChartProperties(this.DashboardHelper, this, (LineChartParameters)Parameters, StrataGridList);

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
                properties = new Controls.GadgetProperties.LineChartProperties(this.DashboardHelper, this, (LineChartParameters)Parameters, StrataGridList);
            properties.Height = (Convert.ToInt32(i_StandardHeight * f_HeightRatio)) / 1.07;
            properties.Width = (Convert.ToInt32(i_StandardWidth * f_WidthRatio)) / 1.07;

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

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0, bool useAlternatingColors = false)
        {
            LineChartParameters LineChartParameters = (LineChartParameters)Parameters;
            if (LineChartParameters.ColumnNames.Count < 1) return string.Empty;	

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<h2>" + LineChartParameters.ChartTitle + "</h2>");
            sb.AppendLine("<h3>" + LineChartParameters.ChartSubTitle + "</h3>");

            foreach (UIElement element in panelMain.Children)
            {
                if (element is EpiDashboard.Controls.Charting.LineChart)
                {
                    sb.AppendLine(((EpiDashboard.Controls.Charting.LineChart)element).ToHTML(htmlFileName, count, true, false));
                    count++;
                }
            }

            return sb.ToString();
        }

        #endregion  //Public Methods
    }
}
