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
    /// Interaction logic for HistogramChartGadget.xaml
    /// </summary>
    public partial class HistogramChartGadget : ChartGadgetBase
    {
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
            FillComboboxes();
        }

        DateTime? StartDate { get; set; }
        DateTime? EndDate { get; set; }
        System.Drawing.Font AxisLabelFont { get; set; }
        System.Drawing.Font ChartTitleFont { get; set; }
        System.Drawing.Font ChartSubtitleFont { get; set; }

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
            string prevCrosstabField = string.Empty;
            List<string> prevStrataFields = new List<string>();

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
                if (cmbFieldCrosstab.SelectedIndex >= 0)
                {
                    prevCrosstabField = cmbFieldCrosstab.SelectedItem.ToString();
                }
                foreach (string s in listboxFieldStrata.SelectedItems)
                {
                    prevStrataFields.Add(s);
                }
            }

            if (cmbLegendDock.Items.Count == 0)
            {
                cmbLegendDock.Items.Add(ChartingSharedStrings.LEGEND_DOCK_VALUE_LEFT);
                cmbLegendDock.Items.Add(ChartingSharedStrings.LEGEND_DOCK_VALUE_RIGHT);
                cmbLegendDock.Items.Add(ChartingSharedStrings.LEGEND_DOCK_VALUE_TOP);
                cmbLegendDock.Items.Add(ChartingSharedStrings.LEGEND_DOCK_VALUE_BOTTOM);
                cmbLegendDock.SelectedIndex = 1;
            }

            cmbField.ItemsSource = null;
            cmbField.Items.Clear();

            cmbFieldWeight.ItemsSource = null;
            cmbFieldWeight.Items.Clear();

            cmbFieldCrosstab.ItemsSource = null;
            cmbFieldCrosstab.Items.Clear();

            listboxFieldStrata.ItemsSource = null;
            listboxFieldStrata.Items.Clear();

            List<string> fieldNames = new List<string>();
            List<string> weightFieldNames = new List<string>();
            List<string> strataFieldNames = new List<string>();
            List<string> crosstabFieldNames = new List<string>();

            weightFieldNames.Add(string.Empty);
            crosstabFieldNames.Add(string.Empty);

            ColumnDataType columnDataType = ColumnDataType.DateTime | ColumnDataType.UserDefined;
            fieldNames = DashboardHelper.GetFieldsAsList(columnDataType);

            columnDataType = ColumnDataType.Numeric | ColumnDataType.UserDefined;
            weightFieldNames.AddRange(DashboardHelper.GetFieldsAsList(columnDataType));

            columnDataType = ColumnDataType.Numeric | ColumnDataType.Boolean | ColumnDataType.Text | ColumnDataType.UserDefined;
            strataFieldNames.AddRange(DashboardHelper.GetFieldsAsList(columnDataType));

            columnDataType = ColumnDataType.Numeric | ColumnDataType.Boolean | ColumnDataType.Text | ColumnDataType.UserDefined;
            crosstabFieldNames.AddRange(DashboardHelper.GetFieldsAsList(columnDataType));

            fieldNames.Sort();
            weightFieldNames.Sort();
            strataFieldNames.Sort();
            crosstabFieldNames.Sort();

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

                if (crosstabFieldNames.Contains("RecStatus")) crosstabFieldNames.Remove("RecStatus");
                if (crosstabFieldNames.Contains("FKEY")) crosstabFieldNames.Remove("FKEY");
                if (crosstabFieldNames.Contains("GlobalRecordId")) crosstabFieldNames.Remove("GlobalRecordId");
            }

            cmbField.ItemsSource = fieldNames;
            cmbFieldWeight.ItemsSource = weightFieldNames;
            cmbFieldCrosstab.ItemsSource = crosstabFieldNames;
            listboxFieldStrata.ItemsSource = strataFieldNames;

            if (cmbField.Items.Count > 0)
            {
                cmbField.SelectedIndex = -1;
            }
            if (cmbFieldWeight.Items.Count > 0)
            {
                cmbFieldWeight.SelectedIndex = -1;
            }
            if (cmbFieldCrosstab.Items.Count > 0)
            {
                cmbFieldCrosstab.SelectedIndex = -1;
            }

            if (update)
            {
                cmbField.SelectedItem = prevField;
                cmbFieldWeight.SelectedItem = prevWeightField;
                cmbFieldCrosstab.SelectedItem = prevCrosstabField;

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
            this.panelAdvanced1.IsEnabled = false;
            this.panelDisplay1.IsEnabled = false;
            this.panelDisplay2.IsEnabled = false;
            this.panelDisplay3.IsEnabled = false;
            this.btnRun.IsEnabled = false;
        }

        public override void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
            this.panelAdvanced1.IsEnabled = true;
            this.panelDisplay1.IsEnabled = true;
            this.panelDisplay2.IsEnabled = true;
            this.panelDisplay3.IsEnabled = true;
            this.btnRun.IsEnabled = true;

            base.SetGadgetToFinishedState();
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

            GadgetOptions.ShouldIgnoreRowLimits = true;

            StartDate = null;
            EndDate = null;

            if (!string.IsNullOrEmpty(txtStartValue.Text))
            {
                DateTime stDt = DateTime.Now;
                double stInt = -1;
                if (DateTime.TryParse(txtStartValue.Text, out stDt))
                {
                    StartDate = stDt;
                }
                else if (double.TryParse(txtStartValue.Text, out stInt))
                {
                    stDt = DateTime.Now;
                    stDt = stDt.AddDays(stInt);
                    StartDate = stDt;
                }
            }

            if (!string.IsNullOrEmpty(txtEndValue.Text))
            {
                DateTime edDt = DateTime.Now;
                double edInt = -1;
                if (DateTime.TryParse(txtEndValue.Text, out edDt))
                {
                    EndDate = edDt;
                }
                else if (double.TryParse(txtEndValue.Text, out edInt))
                {
                    edDt = DateTime.Now;
                    edDt = edDt.AddDays(edInt);
                    EndDate = edDt;
                }
                else if (txtEndValue.Text.ToUpper() == "SYSTEMDATE" || txtEndValue.Text == "SYSTEMDATE")
                {
                    EndDate = DateTime.Now;
                }
            }

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

            if (cmbFieldCrosstab.SelectedIndex > -1 && !string.IsNullOrEmpty(cmbFieldCrosstab.SelectedItem.ToString()))
            {
                inputVariableList.Add("crosstabvar", cmbFieldCrosstab.SelectedItem.ToString());
                GadgetOptions.CrosstabVariableName = cmbFieldCrosstab.SelectedItem.ToString();
            }

            if (listboxFieldStrata.SelectedItems.Count > 0)
            {
                GadgetOptions.StrataVariableNames = new List<string>();
                foreach (string s in listboxFieldStrata.SelectedItems)
                {
                    GadgetOptions.StrataVariableNames.Add(s);
                }
            }

            inputVariableList.Add("step", txtStep.Text);
            switch (cmbInterval.SelectedIndex)
            {
                case 0:
                    inputVariableList.Add("interval", "minute");
                    break;
                case 1:
                    inputVariableList.Add("interval", "hour");
                    break;
                case 2:
                    inputVariableList.Add("interval", "day");
                    break;
                case 3:
                    inputVariableList.Add("interval", "epiweek_sun");
                    break;
                case 4:
                    inputVariableList.Add("interval", "epiweek_mon");
                    break;
                case 5:
                    inputVariableList.Add("interval", "month");
                    break;
                case 6:
                    inputVariableList.Add("interval", "year");
                    break;
            }

            GadgetOptions.ShouldIncludeFullSummaryStatistics = false;
            GadgetOptions.InputVariableList = inputVariableList;
        }

        protected override void Construct()
        {
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

            #region Translation

            tblockWidth.Text = DashboardSharedStrings.GADGET_WIDTH;
            tblockHeight.Text = DashboardSharedStrings.GADGET_HEIGHT;
            tblockMainVariable.Text = DashboardSharedStrings.GADGET_MAIN_VARIABLE;
            tblockStrataVariable.Text = DashboardSharedStrings.GADGET_STRATA_VARIABLE;
            tblockWeightVariable.Text = DashboardSharedStrings.GADGET_WEIGHT_VARIABLE;
            tblockCrosstabVariable.Text = ChartingSharedStrings.GRAPH_FOR_EACH_VALUE_OF_VARIABLE;

            expanderAdvancedOptions.Header = DashboardSharedStrings.GADGET_ADVANCED_OPTIONS;
            expanderDisplayOptions.Header = DashboardSharedStrings.GADGET_DISPLAY_OPTIONS;

            lblColorsStyles.Content = ChartingSharedStrings.PANEL_COLORS_AND_STYLES;
            lblLabels.Content = ChartingSharedStrings.PANEL_LABELS;
            lblLegend.Content = ChartingSharedStrings.PANEL_LEGEND;

            checkboxUseRefValues.Content = ChartingSharedStrings.USE_REFERENCE_VALUES;
            checkboxAnnotations.Content = ChartingSharedStrings.SHOW_ANNOTATIONS;
            checkboxGridLines.Content = ChartingSharedStrings.SHOW_GRID_LINES;
            tblockBarSpacing.Text = ChartingSharedStrings.SPACE_BETWEEN_BARS;
            tblockPalette.Text = ChartingSharedStrings.COLOR_PALETTE;
            tblockBarType.Text = ChartingSharedStrings.BAR_TYPE;
            tblockStep.Text = ChartingSharedStrings.STEP;
            tblockInterval.Text = ChartingSharedStrings.INTERVAL;
            tblockStartValue.Text = ChartingSharedStrings.START_VALUE;
            tblockEndValue.Text = ChartingSharedStrings.END_VALUE;

            tblockYAxisLabelValue.Text = ChartingSharedStrings.Y_AXIS_LABEL;
            tblockXAxisLabelType.Text = ChartingSharedStrings.X_AXIS_LABEL_TYPE;
            tblockXAxisLabelValue.Text = ChartingSharedStrings.X_AXIS_LABEL;
            tblockXAxisAngle.Text = ChartingSharedStrings.X_AXIS_ANGLE;
            tblockChartTitleValue.Text = ChartingSharedStrings.CHART_TITLE;
            tblockChartSubTitleValue.Text = ChartingSharedStrings.CHART_SUBTITLE;

            checkboxShowLegend.Content = ChartingSharedStrings.SHOW_LEGEND;
            checkboxShowLegendBorder.Content = ChartingSharedStrings.SHOW_LEGEND_BORDER;
            checkboxShowVarName.Content = ChartingSharedStrings.SHOW_VAR_NAMES;
            tblockLegendFontSize.Text = ChartingSharedStrings.LEGEND_FONT_SIZE;
            tblockLegendDock.Text = ChartingSharedStrings.LEGEND_PLACEMENT;

            btnRun.Content = DashboardSharedStrings.GADGET_RUN_BUTTON;

            #endregion // Translation

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

        private void RenderFinish()
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.StatusPanel;
            messagePanel.Text = string.Empty;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        private void RenderFinishWithWarning(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed; //waitCursor.Visibility = Visibility.Hidden;

            messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.WarningPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

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

            HideConfigPanel();
            CheckAndSetPosition();
        }

        public void CreateFromLegacyXml(XmlElement element)
        {
            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLower())
                {
                    case "casestatusvariable":
                        listboxFieldStrata.SelectedItems.Add(child.InnerText.Replace("&lt;", "<"));
                        break;
                    //case "yaxisvariable":
                    //    cbxColumnYAxisField.Text = child.InnerText.Replace("&lt;", "<");
                    //    break;
                    //case "xaxisvariable":
                    //    cbxColumnXAxisField.Text = child.InnerText.Replace("&lt;", "<");
                    //    break;
                    //case "yaxisscattervariable":
                    //    cbxScatterYAxisField.Text = child.InnerText.Replace("&lt;", "<");
                    //    break;
                    //case "xaxisscattervariable":
                    //    cbxScatterXAxisField.Text = child.InnerText.Replace("&lt;", "<");
                    //    break;
                    //case "allvalues":
                    //    if (child.InnerText.ToLower().Equals("true"))
                    //    {
                    //        checkboxAllValues.IsChecked = true;
                    //    }
                    //    else
                    //    {
                    //        checkboxAllValues.IsChecked = false;
                    //    }
                    //    break;
                    //case "horizontalgridlines":
                    //    if (child.InnerText.ToLower().Equals("true"))
                    //    {
                    //        checkboxShowHorizontalGridLines.IsChecked = true;
                    //    }
                    //    else
                    //    {
                    //        checkboxShowHorizontalGridLines.IsChecked = false;
                    //    }
                    //    break;
                    case "datevariable":
                        cmbField.Text = child.InnerText.Replace("&lt;", "<");
                        cmbField.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "weightvariable":
                        cmbFieldWeight.Text = child.InnerText.Replace("&lt;", "<");
                        cmbFieldWeight.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "dateinterval":
                        switch (child.InnerText)
                        {
                            case "Hour":
                                cmbInterval.Text = "Hour";
                                break;
                            case "Day":
                                cmbInterval.Text = "Day";
                                break;
                            case "Month":
                                cmbInterval.Text = "Month";
                                break;
                            case "Year":
                                cmbInterval.Text = "Year";
                                break;
                        }
                        break;
                    case "stratavariable":
                        listboxFieldStrata.SelectedItems.Add(child.InnerText.Replace("&lt;", "<"));
                        break;
                    case "columnaggregatefunction":
                        //cbxColumnAggregateFunc.Text = child.InnerText;
                        break;
                    case "charttype":
                        //cbxChartType.Text = child.InnerText;
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
                        //cbxChartSize.SelectedItem = child.InnerText;
                        break;
                    case "chartwidth":
                        txtWidth.Text = child.InnerText;
                        break;
                    case "chartheight":
                        txtHeight.Text = child.InnerText;
                        break;
                    case "charttitle":
                        txtChartTitle.Text = child.InnerText;
                        break;
                    case "chartlegendtitle":
                        //LegendTitle = child.InnerText;
                        break;
                    case "xaxislabel":
                        txtXAxisLabelValue.Text = child.InnerText;
                        cmbXAxisLabelType.SelectedIndex = 3;
                        break;
                    case "yaxislabel":
                        txtYAxisLabelValue.Text = child.InnerText;
                        break;
                    case "xaxisstartvalue":
                        txtStartValue.Text = child.InnerText;
                        break;
                    case "xaxisendvalue":
                        txtEndValue.Text = child.InnerText;
                        break;
                    case "xaxisrotation":
                        switch (child.InnerText)
                        {
                            case "0":
                                txtXAxisAngle.Text = "0";
                                break;
                            case "45":
                                txtXAxisAngle.Text = "-45";
                                break;
                            case "90":
                                txtXAxisAngle.Text = "-90";
                                break;
                            default:
                                txtXAxisAngle.Text = "0";
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

            if (element.Name.Equals("chartGadget") || element.Name.Equals("ChartControl"))
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
                        case "crosstabvariable":
                            cmbFieldCrosstab.Text = child.InnerText.Replace("&lt;", "<");
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
                        case "userefvalues":
                            if (child.InnerText.ToLower().Equals("true")) { checkboxUseRefValues.IsChecked = true; }
                            else { checkboxUseRefValues.IsChecked = false; }
                            break;
                        case "showannotations":
                            if (child.InnerText.ToLower().Equals("true")) { checkboxAnnotations.IsChecked = true; }
                            else { checkboxAnnotations.IsChecked = false; }
                            break;
                        case "showgridlines":
                            if (child.InnerText.ToLower().Equals("true")) { checkboxGridLines.IsChecked = true; }
                            else { checkboxGridLines.IsChecked = false; }
                            break;
                        case "step":
                            txtStep.Text = child.InnerText;
                            break;
                        case "interval":
                            cmbInterval.Text = child.InnerText;
                            break;
                        case "barspace":
                            cmbBarSpacing.SelectedIndex = int.Parse(child.InnerText);
                            break;
                        case "palette":
                            cmbPalette.SelectedIndex = int.Parse(child.InnerText);
                            break;
                        case "bartype":
                            cmbBarType.SelectedIndex = int.Parse(child.InnerText);
                            break;
                        case "legenddock":
                            cmbLegendDock.SelectedIndex = int.Parse(child.InnerText);
                            break;
                        case "yaxislabel":
                            txtYAxisLabelValue.Text = child.InnerText;
                            break;
                        case "xaxislabeltype":
                            cmbXAxisLabelType.SelectedIndex = int.Parse(child.InnerText);
                            break;
                        case "xaxislabel":
                            txtXAxisLabelValue.Text = child.InnerText;
                            break;
                        case "xaxisangle":
                            txtXAxisAngle.Text = child.InnerText;
                            break;
                        case "charttitle":
                            txtChartTitle.Text = child.InnerText;
                            break;
                        case "chartsubtitle":
                            txtChartSubTitle.Text = child.InnerText;
                            break;
                        case "showlegend":
                            if (child.InnerText.ToLower().Equals("true")) { checkboxShowLegend.IsChecked = true; }
                            else { checkboxShowLegend.IsChecked = false; }
                            break;
                        case "showlegendborder":
                            if (child.InnerText.ToLower().Equals("true")) { checkboxShowLegendBorder.IsChecked = true; }
                            else { checkboxShowLegendBorder.IsChecked = false; }
                            break;
                        case "showlegendvarnames":
                            if (child.InnerText.ToLower().Equals("true")) { checkboxShowVarName.IsChecked = true; }
                            else { checkboxShowVarName.IsChecked = false; }
                            break;
                        case "legendfontsize":
                            txtLegendFontSize.Text = child.InnerText;
                            break;
                        case "height":
                            txtHeight.Text = child.InnerText;
                            break;
                        case "width":
                            txtWidth.Text = child.InnerText;
                            break;
                        case "yaxisfrom":
                            txtFromValue.Text = child.InnerText;
                            break;
                        case "yaxisto":
                            txtToValue.Text = child.InnerText;
                            break;
                        case "yaxisstep":
                            txtStepValue.Text = child.InnerText;
                            break;
                        case "xaxisstartvalue":
                            string startText = child.InnerText;

                            if (startText.ToUpper().Equals("SYSTEMDATE") || startText.Equals("SYSTEMDATE"))
                            {
                                txtStartValue.Text = startText;
                            }
                            else if (startText.Length < 5)
                            {
                                double stValueNumeric;
                                bool numericSuccess = Double.TryParse(startText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out stValueNumeric);
                                if (numericSuccess)
                                {
                                    txtStartValue.Text = startText;
                                }
                            }
                            else
                            {
                                long ticks;
                                bool longSuccess = long.TryParse(startText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out ticks);
                                if (longSuccess)
                                {
                                    DateTime dt = new DateTime(ticks);
                                    txtStartValue.Text = dt.ToShortDateString();
                                }
                            }
                            break;
                        case "xaxisendvalue":
                            string endText = child.InnerText;

                            if (endText.Equals("SYSTEMDATE") || endText.Equals("SYSTEMDATE"))
                            {
                                txtEndValue.Text = endText;
                            }
                            else if (endText.Length < 5)
                            {
                                double stValueNumeric;
                                bool numericSuccess = Double.TryParse(endText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out stValueNumeric);
                                if (numericSuccess)
                                {
                                    txtEndValue.Text = endText;
                                }
                            }
                            else
                            {
                                long ticks;
                                bool longSuccess = long.TryParse(endText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out ticks);
                                if (longSuccess)
                                {
                                    DateTime dt = new DateTime(ticks);
                                    txtEndValue.Text = dt.ToShortDateString();
                                }
                            }
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
            string crosstabVar = string.Empty;
            string weightVar = string.Empty;
            string sort = string.Empty;
            bool allValues = false;
            bool showConfLimits = true;
            bool showCumulativePercent = true;
            bool includeMissing = false;

            int step = 1;
            int.TryParse(txtStep.Text, out step);

            string interval = "Day";

            if (cmbInterval.SelectedIndex >= 0)
            {
                interval = cmbInterval.Text;
            }

            int height = 600;
            int width = 800;

            crosstabVar = GadgetOptions.CrosstabVariableName.Replace("<", "&lt;");

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
                "<crosstabVariable>" + crosstabVar + "</crosstabVariable>" +
                "<height>" + height + "</height>" +
                "<width>" + width + "</width>" +

            "<sort>" + sort + "</sort>" +
            "<allValues>" + allValues + "</allValues>" +
            "<includeMissing>" + includeMissing + "</includeMissing>" +
            "<customHeading>" + CustomOutputHeading.Replace("<", "&lt;") + "</customHeading>" +
            "<customDescription>" + CustomOutputDescription.Replace("<", "&lt;") + "</customDescription>" +
            "<customCaption>" + CustomOutputCaption + "</customCaption>";

            xmlString += "<useRefValues>" + checkboxUseRefValues.IsChecked.Value + "</useRefValues>";
            xmlString += "<showAnnotations>" + checkboxAnnotations.IsChecked.Value + "</showAnnotations>";
            xmlString += "<showGridLines>" + checkboxGridLines.IsChecked.Value + "</showGridLines>";

            xmlString += "<step>" + step.ToString() + "</step>";
            xmlString += "<interval>" + interval + "</interval>";

            xmlString += "<barSpace>" + cmbBarSpacing.SelectedIndex + "</barSpace>";
            xmlString += "<palette>" + cmbPalette.SelectedIndex + "</palette>";
            xmlString += "<barType>" + cmbBarType.SelectedIndex + "</barType>";

            xmlString += "<yAxisLabel>" + txtYAxisLabelValue.Text + "</yAxisLabel>";
            xmlString += "<xAxisLabelType>" + cmbXAxisLabelType.SelectedIndex + "</xAxisLabelType>";
            xmlString += "<xAxisLabel>" + txtXAxisLabelValue.Text + "</xAxisLabel>";
            xmlString += "<xAxisAngle>" + txtXAxisAngle.Text + "</xAxisAngle>";
            xmlString += "<chartTitle>" + txtChartTitle.Text + "</chartTitle>";
            xmlString += "<chartSubTitle>" + txtChartSubTitle.Text + "</chartSubTitle>";

            xmlString += "<showLegend>" + checkboxShowLegend.IsChecked.Value + "</showLegend>";
            xmlString += "<showLegendBorder>" + checkboxShowLegendBorder.IsChecked.Value + "</showLegendBorder>";
            xmlString += "<showLegendVarNames>" + checkboxShowVarName.IsChecked.Value + "</showLegendVarNames>";
            xmlString += "<legendFontSize>" + txtLegendFontSize.Text + "</legendFontSize>";

            xmlString += "<legendDock>" + cmbLegendDock.SelectedIndex + "</legendDock>";

            xmlString += "<yAxisFrom>" + txtFromValue.Text + "</yAxisFrom>";
            xmlString += "<yAxisTo>" + txtToValue.Text + "</yAxisTo>";
            xmlString += "<yAxisStep>" + txtStepValue.Text + "</yAxisStep>";

            if (!string.IsNullOrEmpty(txtEndValue.Text))
            {
                DateTime edDt = DateTime.Now;
                double edInt = -1;
                if (DateTime.TryParse(txtEndValue.Text, out edDt))
                {
                    xmlString += "<xAxisEndValue>" + edDt.Ticks.ToString() + "</xAxisEndValue>";
                }
                else if (double.TryParse(txtEndValue.Text, out edInt))
                {
                    xmlString += "<xAxisEndValue>" + edInt.ToString() + "</xAxisEndValue>";
                }
                else if (txtEndValue.Text.ToUpper() == "SYSTEMDATE" || txtEndValue.Text == "SYSTEMDATE")
                {
                    xmlString += "<xAxisEndValue>SYSTEMDATE</xAxisEndValue>";
                }
            }

            if (!string.IsNullOrEmpty(txtStartValue.Text))
            {
                DateTime edDt = DateTime.Now;
                double edInt = -1;
                if (DateTime.TryParse(txtStartValue.Text, out edDt))
                {
                    xmlString += "<xAxisStartValue>" + edDt.Ticks + "</xAxisStartValue>";
                }
                else if (double.TryParse(txtStartValue.Text, out edInt))
                {
                    xmlString += "<xAxisStartValue>" + edInt.ToString() + "</xAxisStartValue>";
                }
                else if (txtStartValue.Text.ToUpper() == "SYSTEMDATE" || txtStartValue.Text == "SYSTEMDATE")
                {
                    xmlString += "<xAxisStartValue>SYSTEMDATE</xAxisStartValue>";
                }
            }

            xmlString = xmlString + SerializeAnchors();

            System.Xml.XmlElement element = doc.CreateElement("histogramChartGadget");
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
            type.Value = "EpiDashboard.Gadgets.Charting.HistogramChartGadget";

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);
            element.Attributes.Append(id);

            return element;
        }

        protected override void SetChartData(List<XYColumnChartData> dataList, Strata strata)
        {

            HistogramChartSettings chartSettings = new HistogramChartSettings();
            chartSettings.ChartTitle = txtChartTitle.Text;
            chartSettings.ChartSubTitle = txtChartSubTitle.Text;
            if (strata != null)
            {
                chartSettings.ChartStrataTitle = strata.Filter;
                if (dataList.Count <= 0)
                {
                    chartSettings.ChartStrataTitle = chartSettings.ChartStrataTitle + StringLiterals.SPACE + StringLiterals.PARANTHESES_OPEN +
                        DashboardSharedStrings.ERROR_NO_DATA_FOR_STRATA +
                        StringLiterals.PARANTHESES_CLOSE;
                }
            }
            chartSettings.ChartWidth = int.Parse(txtWidth.Text);
            chartSettings.ChartHeight = int.Parse(txtHeight.Text);

            double from;
            bool success = Double.TryParse(txtFromValue.Text, out from);
            if (success) { chartSettings.YAxisFrom = from; }

            double to;
            success = Double.TryParse(txtToValue.Text, out to);
            if (success) { chartSettings.YAxisTo = to; }

            double step;
            success = Double.TryParse(txtStepValue.Text, out step);
            if (success) { chartSettings.YAxisStep = step; }

            chartSettings.ShowDefaultGridLines = (bool)checkboxGridLines.IsChecked;
            chartSettings.LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Right;

            if (cmbLegendDock.SelectedItem.ToString().Equals(ChartingSharedStrings.LEGEND_DOCK_VALUE_LEFT))
            {
                chartSettings.LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Left;
            }
            else if (cmbLegendDock.SelectedItem.ToString().Equals(ChartingSharedStrings.LEGEND_DOCK_VALUE_RIGHT))
            {
                chartSettings.LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Right;
            }
            else if (cmbLegendDock.SelectedItem.ToString().Equals(ChartingSharedStrings.LEGEND_DOCK_VALUE_TOP))
            {
                chartSettings.LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Top;
            }
            else if (cmbLegendDock.SelectedItem.ToString().Equals(ChartingSharedStrings.LEGEND_DOCK_VALUE_BOTTOM))
            {
                chartSettings.LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Bottom;
            }

            switch (cmbBarType.SelectedIndex)
            {
                case 1:
                    chartSettings.BarKind = BarKind.Cylinder;
                    break;
                case 2:
                    chartSettings.BarKind = BarKind.Rectangle;
                    break;
                case 3:
                    chartSettings.BarKind = BarKind.RoundedBlock;
                    break;
                default:
                case 0:
                    chartSettings.BarKind = BarKind.Block;
                    break;
            }

            switch (cmbBarSpacing.SelectedIndex)
            {
                case 1:
                    chartSettings.BarSpacing = BarSpacing.None;
                    break;
                case 2:
                    chartSettings.BarSpacing = BarSpacing.Small;
                    break;
                case 3:
                    chartSettings.BarSpacing = BarSpacing.Medium;
                    break;
                case 4:
                    chartSettings.BarSpacing = BarSpacing.Large;
                    break;
                case 0:
                default:
                    chartSettings.BarSpacing = BarSpacing.Default;
                    break;
            }

            chartSettings.LegendFontSize = double.Parse(txtLegendFontSize.Text);

            ComboBoxItem cbi = cmbPalette.SelectedItem as ComboBoxItem;
            ComponentArt.Win.DataVisualization.Palette palette = ComponentArt.Win.DataVisualization.Palette.GetPalette(cbi.Content.ToString());
            chartSettings.Palette = palette;

            chartSettings.ShowAnnotations = (bool)checkboxAnnotations.IsChecked;
            chartSettings.ShowAnnotationsY2 = false;
            chartSettings.ShowLegend = (bool)checkboxShowLegend.IsChecked;
            chartSettings.ShowLegendBorder = (bool)checkboxShowLegendBorder.IsChecked;
            chartSettings.ShowLegendVarNames = (bool)checkboxShowVarName.IsChecked;
            chartSettings.UseRefValues = (bool)checkboxUseRefValues.IsChecked;
            chartSettings.XAxisLabel = txtXAxisLabelValue.Text;
            chartSettings.XAxisLabelRotation = int.Parse(txtXAxisAngle.Text);
            chartSettings.YAxisLabel = txtYAxisLabelValue.Text;
            chartSettings.Y2AxisLabel = string.Empty;
            chartSettings.Y2AxisLegendTitle = string.Empty;

            switch (cmbXAxisLabelType.SelectedIndex)
            {
                case 3:
                    chartSettings.XAxisLabelType = XAxisLabelType.Custom;
                    break;
                case 1:
                    chartSettings.XAxisLabelType = XAxisLabelType.FieldPrompt;
                    Field field = DashboardHelper.GetAssociatedField(GadgetOptions.MainVariableName);
                    if (field == null)
                    {
                        chartSettings.XAxisLabel = GadgetOptions.MainVariableName;
                    }
                    else
                    {
                        chartSettings.XAxisLabel = ((IDataField)field).PromptText;
                    }
                    break;
                case 2:
                    chartSettings.XAxisLabelType = XAxisLabelType.None;
                    break;
                default:
                    chartSettings.XAxisLabelType = XAxisLabelType.Automatic;
                    chartSettings.XAxisLabel = GadgetOptions.MainVariableName;
                    break;
            }

            Controls.Charting.HistogramChart columnChart = new Controls.Charting.HistogramChart(DashboardHelper, GadgetOptions, chartSettings, dataList);
            columnChart.Margin = new Thickness(0, 0, 0, 16);
            columnChart.MouseEnter += new MouseEventHandler(chart_MouseEnter);
            columnChart.MouseLeave += new MouseEventHandler(chart_MouseLeave);
            panelMain.Children.Add(columnChart);
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

        protected override void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (DashboardHelper.IsAutoClosing)
            {
                System.Threading.Thread.Sleep(3000);
            }
            else
            {
                System.Threading.Thread.Sleep(1000);
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

                string freqVar = GadgetOptions.MainVariableName;
                string weightVar = GadgetOptions.WeightVariableName;
                string strataVar = string.Empty;
                string crosstabVar = GadgetOptions.CrosstabVariableName;
                bool includeMissing = GadgetOptions.ShouldIncludeMissing;

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

                    if (!string.IsNullOrEmpty(crosstabVar.Trim()))
                    {
                        List<string> crosstabVarList = new List<string>();
                        crosstabVarList.Add(crosstabVar);

                        foreach (Strata strata in DashboardHelper.GetStrataValuesAsDictionary(crosstabVarList, false, false))
                        {
                            GadgetParameters parameters = new GadgetParameters(GadgetOptions);

                            if (!string.IsNullOrEmpty(GadgetOptions.CustomFilter))
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
                            System.Threading.Thread.Sleep(200);
                        }
                    }
                    else
                    {
                        Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables = DashboardHelper.GenerateFrequencyTable(GadgetOptions);
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
                    Debug.Print("Column chart gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete.");
                    Debug.Print(DashboardHelper.DataFilters.GenerateDataFilterString());
                }
            }
        }

        protected override void GenerateChartData(Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables, Strata strata = null)
        {
            lock (syncLockData)
            {
                string second_y_var = string.Empty;

                if (GadgetOptions.InputVariableList.ContainsKey("second_y_var"))
                {
                    second_y_var = GadgetOptions.InputVariableList["second_y_var"];
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
                    DataTable epiCurveTable = new DataTable("epiCurveTable");
                    epiCurveTable.Columns.Add(new DataColumn(table.Columns[0].ColumnName, typeof(string)));
                    epiCurveTable.Columns.Add(new DataColumn(table.Columns[1].ColumnName, typeof(double)));

                    DataColumn[] parentPrimaryKeyColumns = new DataColumn[1];
                    parentPrimaryKeyColumns[0] = epiCurveTable.Columns[0];
                    epiCurveTable.PrimaryKey = parentPrimaryKeyColumns;

                    int step = 1;
                    int.TryParse(GadgetOptions.InputVariableList["step"], out step);

                    string interval = "day";
                    if (GadgetOptions.InputVariableList.ContainsKey("interval"))
                    {
                        interval = GadgetOptions.InputVariableList["interval"];
                    }

                    if (table.Rows.Count > 0) 
                    {
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

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<h2>" + this.txtChartTitle.Text + "</h2>");
            sb.AppendLine("<h3>" + this.txtChartSubTitle.Text + "</h3>");

            foreach (UIElement element in panelMain.Children)
            {
                if (element is Controls.Charting.HistogramChart)
                {
                    sb.AppendLine(((Controls.Charting.HistogramChart)element).ToHTML(htmlFileName, count, true, false));
                    count++;
                }
            }

            return sb.ToString();
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
    }
}
