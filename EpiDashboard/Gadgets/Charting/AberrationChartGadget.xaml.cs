using System;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using Epi;
using Epi.Data;
using Epi.Fields;
using EpiDashboard;
using ComponentArt.Win.DataVisualization.Charting;

namespace EpiDashboard.Gadgets.Charting
{
    /// <summary>
    /// Interaction logic for AberrationChartGadget.xaml
    /// </summary>
    public partial class AberrationChartGadget : GadgetBase
    {
        private const int DEFAULT_DEVIATIONS = 3;
        private const int DEFAULT_LAG_TIME = 7;
        private const int DEFAULT_TIME_PERIOD = 365;

        private delegate void AddChartDelegate(List<AberrationChartData> dataList, DataTable aberrationDetails, string strata);

        #region Classes

        private class XYChartData
        {
            public DateTime Date { get; set; }
            public double Actual { get; set; }
            public double? Expected { get; set; }
            public double? Aberration { get; set; }
            //public object S { get; set; }
        }

        //public class SimpleDataValue
        //{
        //    public double DependentValue { get; set; }
        //    public DateTime IndependentValue { get; set; }
        //}

        //public class DataGridRow
        //{
        //    public DateTime Date { get; set; }
        //    public double Frequency { get; set; }
        //    public double RunningAverage { get; set; }
        //    public double StandardDeviation { get; set; }
        //    public double Delta
        //    {
        //        get
        //        {
        //            return (Frequency - RunningAverage) / StandardDeviation;
        //        }
        //    }
        //}
        #endregion // Classes

        private string customOutputHeading;
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
        private bool IsDropDownList { get; set; }
        private bool IsCommentLegal { get; set; }
        private bool IsRecoded { get; set; }
        private bool IsOptionField { get; set; }

        public AberrationChartGadget()
        {
            InitializeComponent();
            Construct();
        }

        public AberrationChartGadget(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            Construct();
            FillComboboxes();
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            if (LoadingCombos)
            {
                return;
            }

            RefreshResults();
        }

        private void FillComboboxes(bool update = false)
        {
            LoadingCombos = true;

            string prevField = string.Empty;
            string prevWeightField = string.Empty;
            List<string> prevStrataFields = new List<string>();

            //cmbFontSelector.ItemsSource = Fonts.SystemFontFamilies;

            if (update)
            {
                if (cmbField.SelectedIndex >= 0)
                {
                    prevField = cmbField.SelectedItem.ToString();
                }
                if (cmbFieldWeight.SelectedIndex >= 0)
                {
                    prevWeightField = cmbFieldWeight.SelectedItem.ToString();
                }
                foreach (string s in listboxFieldStrata.SelectedItems)
                {
                    prevStrataFields.Add(s);
                }
            }

            cmbField.ItemsSource = null;
            cmbField.Items.Clear();

            cmbFieldWeight.ItemsSource = null;
            cmbFieldWeight.Items.Clear();

            listboxFieldStrata.ItemsSource = null;
            listboxFieldStrata.Items.Clear();

            List<string> fieldNames = new List<string>();
            List<string> weightFieldNames = new List<string>();
            List<string> strataFieldNames = new List<string>();

            weightFieldNames.Add(string.Empty);

            ColumnDataType columnDataType = ColumnDataType.DateTime | ColumnDataType.UserDefined;
            fieldNames = DashboardHelper.GetFieldsAsList(columnDataType);

            columnDataType = ColumnDataType.Numeric | ColumnDataType.UserDefined;
            weightFieldNames.AddRange(DashboardHelper.GetFieldsAsList(columnDataType));

            columnDataType = ColumnDataType.Numeric | ColumnDataType.Boolean | ColumnDataType.Text | ColumnDataType.UserDefined;
            strataFieldNames.AddRange(DashboardHelper.GetFieldsAsList(columnDataType));

            fieldNames.Sort();
            weightFieldNames.Sort();
            strataFieldNames.Sort();

            if (fieldNames.Contains("SYSTEMDATE"))
            {
                fieldNames.Remove("SYSTEMDATE");
            }

            if (DashboardHelper.IsUsingEpiProject)
            {
                if (fieldNames.Contains("RecStatus")) fieldNames.Remove("RecStatus");
                if (weightFieldNames.Contains("RecStatus")) weightFieldNames.Remove("RecStatus");

                if (strataFieldNames.Contains("RecStatus")) strataFieldNames.Remove("RecStatus");
                if (strataFieldNames.Contains("FKEY")) strataFieldNames.Remove("FKEY");
                if (strataFieldNames.Contains("GlobalRecordId")) strataFieldNames.Remove("GlobalRecordId");
            }

            cmbField.ItemsSource = fieldNames;
            cmbFieldWeight.ItemsSource = weightFieldNames;
            listboxFieldStrata.ItemsSource = strataFieldNames;

            if (cmbField.Items.Count > 0)
            {
                cmbField.SelectedIndex = -1;
            }
            if (cmbFieldWeight.Items.Count > 0)
            {
                cmbFieldWeight.SelectedIndex = -1;
            }

            if (update)
            {
                cmbField.SelectedItem = prevField;
                cmbFieldWeight.SelectedItem = prevWeightField;

                foreach (string s in prevStrataFields)
                {
                    listboxFieldStrata.SelectedItems.Add(s);
                }
            }

            LoadingCombos = false;
        }

        public override void SetGadgetToProcessingState()
        {
            this.IsProcessing = true;
            //this.cmbComposition.IsEnabled = false;
            //this.cmbBarSpacing.IsEnabled = false;
            //this.cmbPalette.IsEnabled = false;
            this.cmbField.IsEnabled = false;
            this.listboxFieldStrata.IsEnabled = false;
            this.cmbFieldWeight.IsEnabled = false;
            this.btnRun.IsEnabled = false;
        }

        public override void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
            //this.cmbComposition.IsEnabled = true;
            //this.cmbBarSpacing.IsEnabled = true;
            //this.cmbPalette.IsEnabled = true;
            this.cmbField.IsEnabled = true;
            this.listboxFieldStrata.IsEnabled = true;
            this.cmbFieldWeight.IsEnabled = true;
            this.btnRun.IsEnabled = true;

            base.SetGadgetToFinishedState();
        }

        private void SetVisuals(Controls.Charting.AberrationChart aberrationChart)
        {
            aberrationChart.Chart.InnerMargins = new Thickness(20, 30, 30, 50);

            aberrationChart.YAxisLabel = txtYAxisLabelValue.Text;

            switch (cmbXAxisLabelType.SelectedIndex)
            {
                case 3:
                    aberrationChart.XAxisLabel = txtXAxisLabelValue.Text;
                    break;
                case 1:
                    Field field = DashboardHelper.GetAssociatedField(GadgetOptions.MainVariableName);
                    if (field != null)
                    {
                        RenderableField rField = field as RenderableField;
                        aberrationChart.XAxisLabel = rField.PromptText;
                    }
                    else
                    {
                        aberrationChart.XAxisLabel = GadgetOptions.MainVariableName;
                    }
                    break;
                case 2:
                    aberrationChart.XAxisLabel = string.Empty;
                    break;
                default:
                    aberrationChart.XAxisLabel = GadgetOptions.MainVariableName;
                    break;
            }

            aberrationChart.ChartTitle = txtChartTitle.Text;
        }

        public override void RefreshResults()
        {
            if (!LoadingCombos && GadgetOptions != null && cmbField.SelectedIndex > -1)
            {
                CreateInputVariableList();
                infoPanel.Visibility = System.Windows.Visibility.Collapsed;
                waitPanel.Visibility = System.Windows.Visibility.Visible;
                messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.StatusPanel;
                descriptionPanel.PanelMode = EpiDashboard.Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
                baseWorker = new BackgroundWorker();
                baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
                baseWorker.RunWorkerAsync();
                base.RefreshResults();
            }
            else if (!LoadingCombos && cmbField.SelectedIndex == -1)
            {
                ClearResults();
                waitPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void CreateInputVariableList()
        {
            Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

            GadgetOptions.MainVariableName = string.Empty;
            GadgetOptions.WeightVariableName = string.Empty;
            GadgetOptions.StrataVariableNames = new List<string>();
            GadgetOptions.CrosstabVariableName = string.Empty;
            GadgetOptions.ColumnNames = new List<string>();

            if (cmbField.SelectedIndex > -1 && !string.IsNullOrEmpty(cmbField.SelectedItem.ToString()))
            {
                inputVariableList.Add("freqvar", cmbField.SelectedItem.ToString());
                GadgetOptions.MainVariableName = cmbField.SelectedItem.ToString();
            }
            else
            {
                return;
            }

            if (cmbFieldWeight.SelectedIndex > -1 && !string.IsNullOrEmpty(cmbFieldWeight.SelectedItem.ToString()))
            {
                inputVariableList.Add("weightvar", cmbFieldWeight.SelectedItem.ToString());
                GadgetOptions.WeightVariableName = cmbFieldWeight.SelectedItem.ToString();
            }

            if (listboxFieldStrata.SelectedItems.Count > 0)
            {
                GadgetOptions.StrataVariableNames = new List<string>();
                foreach (string s in listboxFieldStrata.SelectedItems)
                {
                    GadgetOptions.StrataVariableNames.Add(s);
                }
            }

            if (!string.IsNullOrEmpty(txtLagTime.Text))
            {
                inputVariableList.Add("lagtime", txtLagTime.Text);
            }
            else
            {
                inputVariableList.Add("lagtime", DEFAULT_LAG_TIME.ToString());
                txtLagTime.Text = DEFAULT_LAG_TIME.ToString();
            }

            if (!string.IsNullOrEmpty(txtDeviations.Text))
            {
                inputVariableList.Add("deviations", txtDeviations.Text);
            }
            else
            {
                inputVariableList.Add("deviations", DEFAULT_DEVIATIONS.ToString());
                txtDeviations.Text = DEFAULT_DEVIATIONS.ToString();
            }

            if (!string.IsNullOrEmpty(txtTimePeriod.Text))
            {
                inputVariableList.Add("timeperiod", txtTimePeriod.Text);
            }
            else
            {
                inputVariableList.Add("timeperiod", int.MaxValue.ToString());
            }

            inputVariableList.Add("aberration", "true");

            GadgetOptions.ShouldIncludeFullSummaryStatistics = false;
            GadgetOptions.InputVariableList = inputVariableList;
        }

        protected override void Construct()
        {
            if (!string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)"))
            {
                headerPanel.Text = CustomOutputHeading;
            }

            //xyChart.ProcessDataSourceInBackgroundThread = true;
            //xyChart.RenderSeriesInSpecifiedOrder = true;

            //xAxisScrollBar.ManipulationCompleted += new EventHandler<ManipulationCompletedEventArgs>(xAxisScrollBar_ManipulationCompleted);
            //xAxisScrollBar.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(xAxisScrollBar_ManipulationDelta);
            //xAxisScrollBar.MouseLeftButtonUp += new MouseButtonEventHandler(xAxisScrollBar_MouseLeftButtonUp);

            StrataGridList = new List<Grid>();
            StrataExpanderList = new List<Expander>();

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

            mnuSendToBack.Click += new RoutedEventHandler(mnuSendToBack_Click);
            mnuClose.Click += new RoutedEventHandler(mnuClose_Click);
            this.IsProcessing = false;
            this.GadgetStatusUpdate += new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(IsCancelled);

            #region Translation
            //tblockConfigTitle.Text = DashboardSharedStrings.GADGET_CONFIG_TITLE_ABERRATION_DETECTION;
            tblockStrataVariable.Text = DashboardSharedStrings.GADGET_INDICATOR_VARIABLES_OPT;
            tblockWeightVariable.Text = DashboardSharedStrings.GADGET_COUNT_VARIABLE_OPT;
            tblockMainVariable.Text = DashboardSharedStrings.GADGET_DATE_VARIABLE;
            tblockLagTimeDays.Text = DashboardSharedStrings.GADGET_LAG_TIME_DAYS;
            tblockThresh.Text = DashboardSharedStrings.GADGET_THRESHOLD;
            tblockTimePeriod.Text = DashboardSharedStrings.GADGET_DAYS_PRIOR;
            //expanderAdvancedOptions.Header = DashboardSharedStrings.GADGET_ADVANCED_OPTIONS;
            expanderDisplayOptions.Header = DashboardSharedStrings.GADGET_DISPLAY_OPTIONS;

            tblockWidth.Text = DashboardSharedStrings.GADGET_WIDTH;
            tblockHeight.Text = DashboardSharedStrings.GADGET_HEIGHT;

            lblLabels.Content = ChartingSharedStrings.PANEL_LABELS;

            tblockYAxisLabelValue.Text = ChartingSharedStrings.Y_AXIS_LABEL;
            tblockXAxisLabelType.Text = ChartingSharedStrings.X_AXIS_LABEL_TYPE;
            tblockXAxisLabelValue.Text = ChartingSharedStrings.X_AXIS_LABEL;
            tblockChartTitleValue.Text = ChartingSharedStrings.CHART_TITLE;

            btnRun.Content = DashboardSharedStrings.GADGET_RUN_BUTTON;
            #endregion // Translation

            txtWidth.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            txtHeight.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);

            base.Construct();
        }

        private void ClearResults()
        {
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Text = string.Empty;
            descriptionPanel.PanelMode = EpiDashboard.Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;

            //tblockXAxisLabel.Visibility = System.Windows.Visibility.Collapsed;
            //tblockYAxisLabel.Visibility = System.Windows.Visibility.Collapsed;
            //tblockChartTitle.Visibility = System.Windows.Visibility.Collapsed;

            panelMain.Children.Clear();
            //xyChart.DataSource = null;

            //series0.DataSource = null;
            //series1.DataSource = null;
            //series2.DataSource = null;

            //dataGridMain.DataContext = null;

            //xyChart.Visibility = System.Windows.Visibility.Collapsed;
            //panelData.Visibility = System.Windows.Visibility.Collapsed;
            //xAxisScrollBar.Visibility = System.Windows.Visibility.Collapsed;

            //xyChart.Width = 0;
            //xyChart.Height = 0;

            StrataGridList.Clear();
            StrataExpanderList.Clear();
        }

        private void ShowLabels()
        {
            //if (string.IsNullOrEmpty(tblockXAxisLabel.Text.Trim()))
            //{
            //    tblockXAxisLabel.Visibility = System.Windows.Visibility.Collapsed;
            //}
            //else
            //{
            //    tblockXAxisLabel.Visibility = System.Windows.Visibility.Visible;
            //}

            //if (string.IsNullOrEmpty(tblockYAxisLabel.Text.Trim()))
            //{
            //    tblockYAxisLabel.Visibility = System.Windows.Visibility.Collapsed;
            //}
            //else
            //{
            //    tblockYAxisLabel.Visibility = System.Windows.Visibility.Visible;
            //}

            //if (string.IsNullOrEmpty(tblockChartTitle.Text.Trim()))
            //{
            //    tblockChartTitle.Visibility = System.Windows.Visibility.Collapsed;
            //}
            //else
            //{
            //    tblockChartTitle.Visibility = System.Windows.Visibility.Visible;
            //}
        }

        private void RenderFinish()
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.StatusPanel;
            messagePanel.Text = string.Empty;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            ShowLabels();
            HideConfigPanel();
            CheckAndSetPosition();
        }

        private void RenderFinishWithWarning(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed; //waitCursor.Visibility = Visibility.Hidden;

            messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.WarningPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            ShowLabels();
            HideConfigPanel();
            CheckAndSetPosition();
        }

        private void RenderFinishWithError(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.ErrorPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            panelMain.Children.Clear();
            //xyChart.DataSource = null;
            //xyChart.Visibility = System.Windows.Visibility.Collapsed;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        /// <summary>
        /// Creates the frequency gadget from an Xml element loaded from a canvas saved in 7.1.1.14 or earlier.
        /// </summary>
        /// <param name="element">The element from which to create the gadget</param>
        private void CreateFromLegacyXml(XmlElement element)
        {
            this.LoadingCombos = true;

            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLower())
                {
                    case "mainvariable":
                        cmbField.Text = child.InnerText;
                        cmbField.Text = child.InnerText;
                        //cbxDate.Text = child.InnerText;
                        break;
                    case "stratavariable":
                        listboxFieldStrata.SelectedItems.Add(child.InnerText);
                        //cbxSyndrome.Text = child.InnerText;
                        break;
                    case "weightvariable":
                        cmbFieldWeight.Text = child.InnerText;
                        cmbFieldWeight.Text = child.InnerText;
                        //cbxFieldWeight.Text = child.InnerText;
                        break;
                    case "lagtime":                        
                        txtLagTime.Text = child.InnerText;
                        break;
                    case "deviations":
                        txtDeviations.Text = child.InnerText;
                        break;
                    case "timeperiod":
                        txtTimePeriod.Text = child.InnerText;
                        break;
                    case "datafilters":
                        this.DataFilters = new DataFilters(this.DashboardHelper);
                        this.DataFilters.CreateFromXml(child);
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
                }
            }

            base.CreateFromXml(element);

            this.LoadingCombos = false;

            RefreshResults();
            HideConfigPanel();
        }

        public override void CreateFromXml(XmlElement element)
        {
            this.LoadingCombos = true;
            HideConfigPanel();
            infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            if (element.Name.Equals("aberrationGadget") || element.Name.Equals("AberrationControl"))
            {
                CreateFromLegacyXml(element);
            }
            else
            {
                foreach (XmlElement child in element.ChildNodes)
                {
                    switch (child.Name.ToLower())
                    {
                        case "mainvariable":
                            cmbField.Text = child.InnerText.Replace("&lt;", "<");
                            break;
                        case "stratavariable":
                            listboxFieldStrata.SelectedItems.Add(child.InnerText.Replace("&lt;", "<"));
                            break;
                        case "stratavariables":
                            foreach (XmlElement field in child.ChildNodes)
                            {
                                List<string> fields = new List<string>();
                                if (field.Name.ToLower().Equals("stratavariable"))
                                {
                                    listboxFieldStrata.SelectedItems.Add(field.InnerText.Replace("&lt;", "<"));
                                }
                            }
                            break;
                        case "weightvariable":
                            cmbFieldWeight.Text = child.InnerText.Replace("&lt;", "<");
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
                        //case "userefvalues":
                        //    if (child.InnerText.ToLower().Equals("true")) { checkboxUseRefValues.IsChecked = true; }
                        //    else { checkboxUseRefValues.IsChecked = false; }
                        //    break;
                        //case "showannotations":
                        //    if (child.InnerText.ToLower().Equals("true")) { checkboxAnnotations.IsChecked = true; }
                        //    else { checkboxAnnotations.IsChecked = false; }
                        //    break;
                        //case "palette":
                        //    cmbPalette.SelectedIndex = int.Parse(child.InnerText);
                        //    break;
                        //case "linetype":
                        //    cmbLineType.SelectedIndex = int.Parse(child.InnerText);
                        //    break;
                        case "yaxislabel":
                            txtYAxisLabelValue.Text = child.InnerText;
                            break;
                        case "xaxislabeltype":
                            cmbXAxisLabelType.SelectedIndex = int.Parse(child.InnerText);
                            break;
                        case "xaxislabel":
                            txtXAxisLabelValue.Text = child.InnerText;
                            break;
                        //case "xaxisangle":
                        //    txtXAxisAngle.Text = child.InnerText;
                        //    break;
                        case "charttitle":
                            txtChartTitle.Text = child.InnerText;
                            break;
                        //case "showlegend":
                        //    if (child.InnerText.ToLower().Equals("true")) { checkboxShowLegend.IsChecked = true; }
                        //    else { checkboxShowLegend.IsChecked = false; }
                        //    break;
                        //case "showlegendborder":
                        //    if (child.InnerText.ToLower().Equals("true")) { checkboxShowLegendBorder.IsChecked = true; }
                        //    else { checkboxShowLegendBorder.IsChecked = false; }
                        //    break;
                        //case "showlegendvarnames":
                        //    if (child.InnerText.ToLower().Equals("true")) { checkboxShowVarName.IsChecked = true; }
                        //    else { checkboxShowVarName.IsChecked = false; }
                        //    break;
                        //case "legendfontsize":
                        //    txtLegendFontSize.Text = child.InnerText;
                        //    break;
                        case "height":
                            txtHeight.Text = child.InnerText;
                            break;
                        case "width":
                            txtWidth.Text = child.InnerText;
                            break;
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
            CreateInputVariableList();

            Dictionary<string, string> inputVariableList = GadgetOptions.InputVariableList;

            string mainVar = string.Empty;
            string strataVar = string.Empty;
            string weightVar = string.Empty;
            string sort = string.Empty;
            bool allValues = false;
            bool showConfLimits = true;
            bool showCumulativePercent = true;
            bool includeMissing = false;

            int height = 600;
            int width = 800;

            int.TryParse(txtHeight.Text, out height);
            int.TryParse(txtWidth.Text, out width);

            if (inputVariableList.ContainsKey("freqvar"))
            {
                mainVar = inputVariableList["freqvar"].Replace("<", "&lt;");
            }
            if (inputVariableList.ContainsKey("weightvar"))
            {
                weightVar = inputVariableList["weightvar"].Replace("<", "&lt;");
            }
            if (inputVariableList.ContainsKey("sort"))
            {
                sort = inputVariableList["sort"];
            }
            if (inputVariableList.ContainsKey("allvalues"))
            {
                allValues = bool.Parse(inputVariableList["allvalues"]);
            }
            if (inputVariableList.ContainsKey("showconflimits"))
            {
                showConfLimits = bool.Parse(inputVariableList["showconflimits"]);
            }
            if (inputVariableList.ContainsKey("showcumulativepercent"))
            {
                showCumulativePercent = bool.Parse(inputVariableList["showcumulativepercent"]);
            }
            if (inputVariableList.ContainsKey("includemissing"))
            {
                includeMissing = bool.Parse(inputVariableList["includemissing"]);
            }

            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            string xmlString =
            "<mainVariable>" + mainVar + "</mainVariable>";

            if (GadgetOptions.StrataVariableNames.Count == 1)
            {
                xmlString = xmlString + "<strataVariable>" + GadgetOptions.StrataVariableNames[0].Replace("<", "&lt;") + "</strataVariable>";
            }
            else if (GadgetOptions.StrataVariableNames.Count > 1)
            {
                xmlString = xmlString + "<strataVariables>";

                foreach (string strataVariable in this.GadgetOptions.StrataVariableNames)
                {
                    xmlString = xmlString + "<strataVariable>" + strataVariable.Replace("<", "&lt;") + "</strataVariable>";
                }

                xmlString = xmlString + "</strataVariables>";
            }

            xmlString = xmlString + "<weightVariable>" + weightVar + "</weightVariable>" +

                "<height>" + height + "</height>" +
                "<width>" + width + "</width>" +

            "<sort>" + sort + "</sort>" +
            "<allValues>" + allValues + "</allValues>" +
            "<includeMissing>" + includeMissing + "</includeMissing>" +
            "<customHeading>" + CustomOutputHeading.Replace("<", "&lt;") + "</customHeading>" +
            "<customDescription>" + CustomOutputDescription.Replace("<", "&lt;") + "</customDescription>" +
            "<customCaption>" + CustomOutputCaption + "</customCaption>";

            //xmlString += "<showAnnotations>" + checkboxAnnotations.IsChecked.Value + "</showAnnotations>";

            xmlString += "<lagTime>" + txtLagTime.Text + "</lagTime>";
            xmlString += "<deviations>" + txtDeviations.Text + "</deviations>";
            xmlString += "<timePeriod>" + txtTimePeriod.Text + "</timePeriod>";

            //xmlString += "<palette>" + cmbPalette.SelectedIndex + "</palette>";
            //xmlString += "<lineType>" + cmbLineType.SelectedIndex + "</lineType>";

            xmlString += "<yAxisLabel>" + txtYAxisLabelValue.Text + "</yAxisLabel>";
            xmlString += "<xAxisLabelType>" + cmbXAxisLabelType.SelectedIndex + "</xAxisLabelType>";
            xmlString += "<xAxisLabel>" + txtXAxisLabelValue.Text + "</xAxisLabel>";
            xmlString += "<chartTitle>" + txtChartTitle.Text + "</chartTitle>";

            xmlString = xmlString + SerializeAnchors();

            System.Xml.XmlElement element = doc.CreateElement("aberrationChartGadget");
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
            collapsed.Value = "false"; // currently no way to collapse the gadget, so leave this 'false' for now
            type.Value = "EpiDashboard.Gadgets.Charting.AberrationChartGadget";

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);
            element.Attributes.Append(id);

            return element;
        }

        private void NumberBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //e.ControlText
            e.Handled = !Util.IsWholeNumber(e.Text);
            base.OnPreviewTextInput(e);
        }

        private void txtLagTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtLagTime.Text))
            {
                int lagTime = 7;

                int.TryParse(txtLagTime.Text, out lagTime);

                if (lagTime > 365)
                {
                    txtLagTime.Text = "365";
                }
                else if (lagTime <= 1)
                {
                    txtLagTime.Text = "1";
                }
            }
        }

        private void txtDeviations_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtDeviations.Text))
            {
                int dev = 0;

                int.TryParse(txtDeviations.Text, out dev);

                if (dev > 7)
                {
                    txtDeviations.Text = "7";
                }
                else if (dev <= 1)
                {
                    txtDeviations.Text = "1";
                }
            }
        }

        private void txtTimePeriod_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtTimePeriod.Text))
            {
                int period = 365;

                int.TryParse(txtTimePeriod.Text, out period);

                if (period > 366)
                {
                    txtTimePeriod.Text = "366";
                }
            }
        }

        /// <summary>
        /// TODO: Rename this method
        /// </summary>
        /// <param name="dataList"></param>
        /// <param name="aberrationDetails"></param>
        private void AddChart(List<AberrationChartData> dataList, DataTable aberrationDetails, string strata)
        {
            Controls.Charting.AberrationChart abChart = new Controls.Charting.AberrationChart();
            abChart.DashboardHelper = this.DashboardHelper;
            abChart.SetChartSize(double.Parse(txtWidth.Text), double.Parse(txtHeight.Text));
            abChart.SetChartData(dataList, aberrationDetails);
            panelMain.Children.Add(abChart);
            SetVisuals(abChart);

            if (GadgetOptions.StrataVariableNames.Count > 0)
            {
                abChart.IndicatorTitle = strata;
            }

            abChart.Margin = new Thickness(0, 0, 0, 48);
        }

        private double CalculateStdDev(IEnumerable<double> values)
        {
            double ret = 0;
            if (values.Count() > 0)
            {
                double avg = values.Average();
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                ret = Math.Sqrt((sum) / (values.Count() - 1));
            }
            return ret;
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

        private void AddFillerRows(DataTable table)
        {
            //bool recreateTable = false;
            string columnType = table.Columns[0].DataType.ToString();
            if (columnType == "System.DateTime")
            {
                DateTime minDate = ((DateTime)table.Rows[0][0]);
                DateTime maxDate = ((DateTime)table.Rows[table.Rows.Count - 1][0]);
                DataColumn[] pkColumns = new DataColumn[1];
                pkColumns[0] = table.Columns[0];
                table.PrimaryKey = pkColumns;
                DateTime curDate = minDate;

                while (curDate < maxDate)
                {
                    curDate = curDate.AddDays(1);
                    DataRow row = table.Rows.Find(curDate);
                    if (row == null)
                    {
                        table.Rows.Add(curDate, 0);
                        //recreateTable = true;
                    }
                }
            }
        }

        protected override void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (syncLock)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));

                string freqVar = GadgetOptions.MainVariableName;
                string weightVar = GadgetOptions.WeightVariableName;
                string strataVar = string.Empty;
                bool includeMissing = GadgetOptions.ShouldIncludeMissing;

                int lagTime = DEFAULT_LAG_TIME;
                int timePeriod = DEFAULT_TIME_PERIOD;
                double deviations = DEFAULT_DEVIATIONS;

                if (GadgetOptions.InputVariableList.ContainsKey("lagtime"))
                {
                    int.TryParse(GadgetOptions.InputVariableList["lagtime"], out lagTime);
                }
                if (GadgetOptions.InputVariableList.ContainsKey("timeperiod"))
                {
                    int.TryParse(GadgetOptions.InputVariableList["timeperiod"], out timePeriod);
                }
                if (GadgetOptions.InputVariableList.ContainsKey("deviations"))
                {
                    double.TryParse(GadgetOptions.InputVariableList["deviations"], out deviations);
                }

                List<string> stratas = new List<string>();
                if (!string.IsNullOrEmpty(strataVar))
                {
                    stratas.Add(strataVar);
                }

                try
                {
                    RequestUpdateStatusDelegate requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
                    CheckForCancellationDelegate checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

                    GadgetOptions.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                    GadgetOptions.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);

                    if (this.DataFilters != null && this.DataFilters.Count > 0)
                    {
                        GadgetOptions.CustomFilter = this.DataFilters.GenerateDataFilterString(false);
                    }
                    else
                    {
                        GadgetOptions.CustomFilter = string.Empty;
                    }

                    Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables = DashboardHelper.GenerateFrequencyTable(GadgetOptions);
                    List<AberrationChartData> dataList = new List<AberrationChartData>();
                    DataTable aberrationDetails = new DataTable();

                    aberrationDetails.Columns.Add(new DataColumn("Date", typeof(string)));
                    aberrationDetails.Columns.Add(new DataColumn("Count", typeof(double)));
                    aberrationDetails.Columns.Add(new DataColumn("Expected", typeof(double)));
                    aberrationDetails.Columns.Add(new DataColumn("Difference", typeof(string)));

                    foreach (KeyValuePair<DataTable, List<DescriptiveStatistics>> tableKvp in stratifiedFrequencyTables)
                    {
                        dataList = new List<AberrationChartData>();
                        aberrationDetails = new DataTable();

                        aberrationDetails.Columns.Add(new DataColumn("Date", typeof(string)));
                        aberrationDetails.Columns.Add(new DataColumn("Count", typeof(double)));
                        aberrationDetails.Columns.Add(new DataColumn("Expected", typeof(double)));
                        aberrationDetails.Columns.Add(new DataColumn("Difference", typeof(string)));

                        double count = 0;
                        foreach (DescriptiveStatistics ds in tableKvp.Value)
                        {
                            count = count + ds.observations;
                        }

                        string strataValue = tableKvp.Key.TableName;
                        DataTable table = tableKvp.Key;

                        AddFillerRows(table);

                        double lastAvg = double.NegativeInfinity;
                        double lastStdDev = double.NegativeInfinity;
                        Queue<double> frame = new Queue<double>();
                        List<AberrationChartData> actualValues = new List<AberrationChartData>();
                        int rowCount = 1;

                        var rows = from DataRow row in table.Rows
                                   orderby row[0]
                                   select row;

                        foreach (System.Data.DataRow row in rows /*table.Rows*/)
                        {
                            if (!row[freqVar].Equals(DBNull.Value) || (row[freqVar].Equals(DBNull.Value) && includeMissing == true))
                            {
                                DateTime displayValue = DateTime.Parse(row[freqVar].ToString());

                                frame.Enqueue((double)row["freq"]);
                                AberrationChartData actualValue = new AberrationChartData();
                                actualValue.Date = displayValue; // strataValue;
                                actualValue.Actual = (double)row[1];
                                //actualValue.S = row[0];
                                //dataList.Add(actualValue);
                                if (frame.Count > lagTime - 1 /*6*/)
                                {
                                    double[] frameArray = frame.ToArray();
                                    double frameAvg = frameArray.Average();
                                    frame.Dequeue();
                                    double stdDev = CalculateStdDev(frameArray);
                                    if (lastAvg != double.NegativeInfinity)
                                    {
                                        actualValue.Expected = lastAvg;

                                        //dataList.Add(trendValue);
                                        if ((double)row["freq"] > lastAvg + (/*2.99*/deviations * lastStdDev))
                                        {
                                            double freq = (double)row["freq"];

                                            actualValue.Aberration = freq;
                                            double dvs = (freq - lastAvg) / lastStdDev;
                                            aberrationDetails.Rows.Add(displayValue.ToShortDateString(), (double)row["freq"], Math.Round(lastAvg, 2), Math.Round(dvs, 2).ToString() + " standard deviations");
                                        }
                                    }
                                    lastAvg = frameAvg;
                                    lastStdDev = stdDev;
                                }


                                dataList.Add(actualValue);

                                rowCount++;
                            }
                        }

                        this.Dispatcher.BeginInvoke(new AddChartDelegate(AddChart), dataList, aberrationDetails, strataValue);
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
                    Debug.Print("Column chart gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete.");
                    Debug.Print(DashboardHelper.DataFilters.GenerateDataFilterString());
                }
            }
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0)
        {

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

            int subCount = 0;
            string imageFileName = string.Empty;

            foreach (UIElement element in panelMain.Children)
            {
                if (element is Controls.Charting.AberrationChart)
                {
                    imageFileName = htmlFileName + "_" + count.ToString() + "_" + subCount.ToString() + ".png";

                    Controls.Charting.AberrationChart abChart = element as Controls.Charting.AberrationChart;

                    htmlBuilder.Append(abChart.ToHTML(htmlFileName, subCount));
                    subCount++;
                }
            }
            return htmlBuilder.ToString();
        }

        public static ImageSource ToImageSource(FrameworkElement obj)
        {
            Transform transform = obj.LayoutTransform;
            Thickness margin = obj.Margin;
            obj.Margin = new Thickness(0, 0, margin.Right - margin.Left, margin.Bottom - margin.Top);
            Size size = new Size(obj.ActualWidth, obj.ActualHeight);
            obj.Measure(size);
            obj.Arrange(new Rect(size));
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)obj.ActualWidth, (int)obj.ActualHeight, 96, 96, PixelFormats.Pbgra32);

            bmp.Render(obj);
            obj.LayoutTransform = transform;
            obj.Margin = margin;
            return bmp;
        }

        private void cmbXAxisLabelType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LoadingCombos || txtXAxisLabelValue == null) return;
            switch (cmbXAxisLabelType.SelectedIndex)
            {
                case 3:
                    txtXAxisLabelValue.IsEnabled = true;
                    break;
                case 0:
                case 1:
                case 2:
                    txtXAxisLabelValue.IsEnabled = false;
                    txtXAxisLabelValue.Text = string.Empty;
                    break;
            }
        }

        private void xyChart_DataStructureCreated(object sender, EventArgs e)
        {
            string sName = "";
        }
    }
}
