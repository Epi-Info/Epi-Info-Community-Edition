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
            //FillComboboxes();
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

        private void FillComboboxes(bool update = false)
        {
            //LoadingCombos = true;

            //string prevField = string.Empty;
            //string prevWeightField = string.Empty;
            //string prevCrosstabField = string.Empty;
            //string prev2ndYAxisField = string.Empty;
            //List<string> prevStrataFields = new List<string>();

            //if (update)
            //{
            //    if (cmbField.SelectedIndex >= 0)
            //    {
            //        prevField = cmbField.SelectedItem.ToString();
            //    }
            //    if (cmbFieldWeight.SelectedIndex >= 0)
            //    {
            //        prevWeightField = cmbFieldWeight.SelectedItem.ToString();
            //    }
            //    if (cmbFieldCrosstab.SelectedIndex >= 0)
            //    {
            //        prevCrosstabField = cmbFieldCrosstab.SelectedItem.ToString();
            //    }
            //    if (cmbSecondYAxisVariable.SelectedIndex >= 0)
            //    {
            //        prev2ndYAxisField = cmbSecondYAxisVariable.SelectedItem.ToString();
            //    }
            //    foreach (string s in listboxFieldStrata.SelectedItems)
            //    {
            //        prevStrataFields.Add(s);
            //    }
            //}

            //if (cmbLegendDock.Items.Count == 0)
            //{
            //    cmbLegendDock.Items.Add(ChartingSharedStrings.LEGEND_DOCK_VALUE_LEFT);
            //    cmbLegendDock.Items.Add(ChartingSharedStrings.LEGEND_DOCK_VALUE_RIGHT);
            //    cmbLegendDock.Items.Add(ChartingSharedStrings.LEGEND_DOCK_VALUE_TOP);
            //    cmbLegendDock.Items.Add(ChartingSharedStrings.LEGEND_DOCK_VALUE_BOTTOM);
            //    cmbLegendDock.SelectedIndex = 1;
            //}

            //cmbField.ItemsSource = null;
            //cmbField.Items.Clear();

            //cmbFieldWeight.ItemsSource = null;
            //cmbFieldWeight.Items.Clear();

            //cmbFieldCrosstab.ItemsSource = null;
            //cmbFieldCrosstab.Items.Clear();

            //cmbSecondYAxisVariable.ItemsSource = null;
            //cmbSecondYAxisVariable.Items.Clear();

            //listboxFieldStrata.ItemsSource = null;
            //listboxFieldStrata.Items.Clear();

            //List<string> fieldNames = new List<string>();
            //List<string> weightFieldNames = new List<string>();
            //List<string> strataFieldNames = new List<string>();
            //List<string> crosstabFieldNames = new List<string>();

            //weightFieldNames.Add(string.Empty);
            //crosstabFieldNames.Add(string.Empty);

            //ColumnDataType columnDataType = ColumnDataType.Text | ColumnDataType.DateTime | ColumnDataType.Numeric | ColumnDataType.UserDefined;
            //fieldNames = DashboardHelper.GetFieldsAsList(columnDataType);

            //columnDataType = ColumnDataType.Numeric | ColumnDataType.UserDefined;
            //weightFieldNames.AddRange(DashboardHelper.GetFieldsAsList(columnDataType));

            //columnDataType = ColumnDataType.Numeric | ColumnDataType.Boolean | ColumnDataType.Text | ColumnDataType.UserDefined;
            //strataFieldNames.AddRange(DashboardHelper.GetFieldsAsList(columnDataType));

            //columnDataType = ColumnDataType.Numeric | ColumnDataType.Boolean | ColumnDataType.Text | ColumnDataType.UserDefined;
            //crosstabFieldNames.AddRange(DashboardHelper.GetFieldsAsList(columnDataType));

            //fieldNames.Sort();
            //weightFieldNames.Sort();
            //strataFieldNames.Sort();
            //crosstabFieldNames.Sort();

            //if (fieldNames.Contains("SYSTEMDATE"))
            //{
            //    fieldNames.Remove("SYSTEMDATE");
            //}

            //if (DashboardHelper.IsUsingEpiProject)
            //{
            //    if (fieldNames.Contains("RecStatus")) fieldNames.Remove("RecStatus");
            //    if (weightFieldNames.Contains("RecStatus")) weightFieldNames.Remove("RecStatus");

            //    if (strataFieldNames.Contains("RecStatus")) strataFieldNames.Remove("RecStatus");
            //    if (strataFieldNames.Contains("FKEY")) strataFieldNames.Remove("FKEY");
            //    if (strataFieldNames.Contains("GlobalRecordId")) strataFieldNames.Remove("GlobalRecordId");

            //    if (crosstabFieldNames.Contains("RecStatus")) crosstabFieldNames.Remove("RecStatus");
            //    if (crosstabFieldNames.Contains("FKEY")) crosstabFieldNames.Remove("FKEY");
            //    if (crosstabFieldNames.Contains("GlobalRecordId")) crosstabFieldNames.Remove("GlobalRecordId");
            //}

            //cmbField.ItemsSource = fieldNames;
            //cmbFieldWeight.ItemsSource = weightFieldNames;
            //cmbFieldCrosstab.ItemsSource = crosstabFieldNames;
            //cmbSecondYAxisVariable.ItemsSource = weightFieldNames;
            //listboxFieldStrata.ItemsSource = strataFieldNames;

            //if (cmbField.Items.Count > 0)
            //{
            //    cmbField.SelectedIndex = -1;
            //}
            //if (cmbFieldWeight.Items.Count > 0)
            //{
            //    cmbFieldWeight.SelectedIndex = -1;
            //}
            //if (cmbFieldCrosstab.Items.Count > 0)
            //{
            //    cmbFieldCrosstab.SelectedIndex = -1;
            //}
            //if (cmbSecondYAxisVariable.Items.Count > 0)
            //{
            //    cmbSecondYAxisVariable.SelectedIndex = -1;
            //}

            //if (update)
            //{
            //    cmbField.SelectedItem = prevField;
            //    cmbFieldWeight.SelectedItem = prevWeightField;
            //    cmbFieldCrosstab.SelectedItem = prevCrosstabField;
            //    cmbSecondYAxisVariable.SelectedItem = prev2ndYAxisField;

            //    foreach (string s in prevStrataFields)
            //    {
            //        listboxFieldStrata.SelectedItems.Add(s);
            //    }
            //}

            //LoadingCombos = false;
        }

        private void CreateInputVariableList()
        {
            //Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

            //GadgetOptions.MainVariableName = string.Empty;
            //GadgetOptions.WeightVariableName = string.Empty;
            //GadgetOptions.StrataVariableNames = new List<string>();
            //GadgetOptions.CrosstabVariableName = string.Empty;
            //GadgetOptions.ColumnNames = new List<string>();

            //if (cmbField.SelectedIndex > -1 && !string.IsNullOrEmpty(cmbField.SelectedItem.ToString()))
            //{
            //    inputVariableList.Add("freqvar", cmbField.SelectedItem.ToString());
            //    GadgetOptions.MainVariableName = cmbField.SelectedItem.ToString();
            //}
            //else
            //{
            //    return;
            //}

            //if (cmbFieldWeight.SelectedIndex > -1 && !string.IsNullOrEmpty(cmbFieldWeight.SelectedItem.ToString()))
            //{
            //    inputVariableList.Add("weightvar", cmbFieldWeight.SelectedItem.ToString());
            //    GadgetOptions.WeightVariableName = cmbFieldWeight.SelectedItem.ToString();
            //}

            //if (cmbFieldCrosstab.SelectedIndex > -1 && !string.IsNullOrEmpty(cmbFieldCrosstab.SelectedItem.ToString()))
            //{
            //    inputVariableList.Add("crosstabvar", cmbFieldCrosstab.SelectedItem.ToString());
            //    GadgetOptions.CrosstabVariableName = cmbFieldCrosstab.SelectedItem.ToString();
            //}

            //if (cmbSecondYAxisVariable.SelectedIndex > -1 && !string.IsNullOrEmpty(cmbSecondYAxisVariable.SelectedItem.ToString()))
            //{
            //    inputVariableList.Add("second_y_var", cmbSecondYAxisVariable.SelectedItem.ToString());
            //}

            //if (cmbSecondYAxis.SelectedIndex == 1)
            //{
            //    inputVariableList.Add("second_y_var_type", "single");
            //}
            //else if (cmbSecondYAxis.SelectedIndex == 2)
            //{
            //    inputVariableList.Add("second_y_var_type", "rate_per_100k");
            //}

            //if (listboxFieldStrata.SelectedItems.Count > 0)
            //{
            //    GadgetOptions.StrataVariableNames = new List<string>();
            //    foreach (string s in listboxFieldStrata.SelectedItems)
            //    {
            //        GadgetOptions.StrataVariableNames.Add(s);
            //    }
            //}

            //if (checkboxAllValues.IsChecked == true)
            //{
            //    inputVariableList.Add("allvalues", "true");
            //    GadgetOptions.ShouldUseAllPossibleValues = true;
            //}
            //else
            //{
            //    inputVariableList.Add("allvalues", "false");
            //    GadgetOptions.ShouldUseAllPossibleValues = false;
            //}

            //if (checkboxCommentLegalLabels.IsChecked == true)
            //{
            //    GadgetOptions.ShouldShowCommentLegalLabels = true;
            //}
            //else
            //{
            //    GadgetOptions.ShouldShowCommentLegalLabels = false;
            //}

            //if (checkboxSortHighLow.IsChecked == true)
            //{
            //    inputVariableList.Add("sort", "highlow");
            //    GadgetOptions.ShouldSortHighToLow = true;
            //}
            //else
            //{
            //    GadgetOptions.ShouldSortHighToLow = false;
            //}

            //if (checkboxIncludeMissing.IsChecked == true)
            //{
            //    inputVariableList.Add("includemissing", "true");
            //    GadgetOptions.ShouldIncludeMissing = true;
            //}
            //else
            //{
            //    inputVariableList.Add("includemissing", "false");
            //    GadgetOptions.ShouldIncludeMissing = false;
            //}

            //GadgetOptions.ShouldIncludeFullSummaryStatistics = false;
            //GadgetOptions.InputVariableList = inputVariableList;            
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

            #region Translation

            //tblockWidth.Text = DashboardSharedStrings.GADGET_WIDTH;
            //tblockHeight.Text = DashboardSharedStrings.GADGET_HEIGHT;
            //tblockMainVariable.Text = DashboardSharedStrings.GADGET_MAIN_VARIABLE;
            //tblockStrataVariable.Text = DashboardSharedStrings.GADGET_STRATA_VARIABLE;
            //tblockWeightVariable.Text = DashboardSharedStrings.GADGET_WEIGHT_VARIABLE;
            //tblockCrosstabVariable.Text = ChartingSharedStrings.GRAPH_FOR_EACH_VALUE_OF_VARIABLE;

            //checkboxAllValues.Content = DashboardSharedStrings.GADGET_ALL_LIST_VALUES;
            //checkboxCommentLegalLabels.Content = DashboardSharedStrings.GADGET_LIST_LABELS;
            //checkboxIncludeMissing.Content = DashboardSharedStrings.GADGET_INCLUDE_MISSING;
            //checkboxSortHighLow.Content = DashboardSharedStrings.GADGET_SORT_HI_LOW;

            //tblockSecondYAxis.Text = ChartingSharedStrings.SECOND_Y_AXIS_TYPE;
            //tblockSecondYAxisVariable.Text = ChartingSharedStrings.SECOND_Y_AXIS_VARIABLE;

            //expanderAdvancedOptions.Header = DashboardSharedStrings.GADGET_ADVANCED_OPTIONS;
            //expanderDisplayOptions.Header = DashboardSharedStrings.GADGET_DISPLAY_OPTIONS;

            //lblColorsStyles.Content = ChartingSharedStrings.PANEL_COLORS_AND_STYLES;
            //lblLabels.Content = ChartingSharedStrings.PANEL_LABELS;
            //lblLegend.Content = ChartingSharedStrings.PANEL_LEGEND;

            //checkboxUseRefValues.Content = ChartingSharedStrings.USE_REFERENCE_VALUES;            
            //checkboxAnnotations.Content = ChartingSharedStrings.SHOW_ANNOTATIONS;
            //checkboxGridLines.Content = ChartingSharedStrings.SHOW_GRID_LINES;            
            //tblockPalette.Text = ChartingSharedStrings.COLOR_PALETTE;

            //tblockLineType.Text = ChartingSharedStrings.LINE_TYPE;
            //tblockLineTypeY2.Text = ChartingSharedStrings.LINE_TYPE_Y2;
            //tblockLineDashTypeY2.Text = ChartingSharedStrings.LINE_DASH_STYLE_Y2;
            //tblockLineThicknessY2.Text = ChartingSharedStrings.LINE_THICKNESS_Y2;

            //tblockYAxisLabelValue.Text = ChartingSharedStrings.Y_AXIS_LABEL;
            //tblockYAxisFormatString.Text = ChartingSharedStrings.Y_AXIS_FORMAT;

            //tblockY2AxisLabelValue.Text = ChartingSharedStrings.Y2_AXIS_LABEL;
            //tblockY2AxisLegendTitle.Text = ChartingSharedStrings.Y2_AXIS_LEGEND_TITLE;
            //tblockY2AxisFormatString.Text = ChartingSharedStrings.Y2_AXIS_FORMAT;

            //tblockXAxisLabelType.Text = ChartingSharedStrings.X_AXIS_LABEL_TYPE;
            //tblockXAxisLabelValue.Text = ChartingSharedStrings.X_AXIS_LABEL;
            //tblockXAxisAngle.Text = ChartingSharedStrings.X_AXIS_ANGLE;
            //tblockChartTitleValue.Text = ChartingSharedStrings.CHART_TITLE;
            //tblockChartSubTitleValue.Text = ChartingSharedStrings.CHART_SUBTITLE;

            //checkboxShowLegend.Content = ChartingSharedStrings.SHOW_LEGEND;
            //checkboxShowLegendBorder.Content = ChartingSharedStrings.SHOW_LEGEND_BORDER;
            //checkboxShowVarName.Content = ChartingSharedStrings.SHOW_VAR_NAMES;
            //tblockLegendFontSize.Text = ChartingSharedStrings.LEGEND_FONT_SIZE;
            //tblockLegendDock.Text = ChartingSharedStrings.LEGEND_PLACEMENT;

            //btnRun.Content = DashboardSharedStrings.GADGET_RUN_BUTTON;

            #endregion // Translation

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
                //LineChartSettings chartSettings = new LineChartSettings();
                //chartSettings.ChartTitle = txtChartTitle.Text;
                //chartSettings.ChartSubTitle = txtChartSubTitle.Text;
                if (strata != null)
                {
                    LCParameters.ChartStrataTitle = strata.Filter;
                }
                else
                {
                    LCParameters.ChartStrataTitle = String.Empty;
                }
                //chartSettings.ChartWidth = int.Parse(txtWidth.Text);
                //chartSettings.ChartHeight = int.Parse(txtHeight.Text);
                //chartSettings.ShowDefaultGridLines = (bool)checkboxGridLines.IsChecked;

                //chartSettings.LegendFontSize = double.Parse(txtLegendFontSize.Text);

                //ComboBoxItem cbi = cmbPalette.SelectedItem as ComboBoxItem;
                //ComponentArt.Win.DataVisualization.Palette palette = ComponentArt.Win.DataVisualization.Palette.GetPalette(cbi.Content.ToString());
                //chartSettings.Palette = palette;

                //chartSettings.ShowAnnotations = (bool)checkboxAnnotations.IsChecked;
                //chartSettings.ShowAnnotationsY2 = (bool)checkboxAnnotationsY2.IsChecked;
                //chartSettings.ShowLegend = (bool)checkboxShowLegend.IsChecked;
                //chartSettings.ShowLegendBorder = (bool)checkboxShowLegendBorder.IsChecked;
                //chartSettings.ShowLegendVarNames = (bool)checkboxShowVarName.IsChecked;
                //chartSettings.UseRefValues = (bool)checkboxUseRefValues.IsChecked;
                //chartSettings.XAxisLabel = txtXAxisLabelValue.Text;
                //chartSettings.XAxisLabelRotation = int.Parse(txtXAxisAngle.Text);
                //chartSettings.YAxisLabel = txtYAxisLabelValue.Text;
                //chartSettings.YAxisFormattingString = txtYAxisFormatString.Text;
                //chartSettings.Y2AxisFormattingString = txtY2AxisFormatString.Text;
                //chartSettings.Y2AxisLabel = txtY2AxisLabelValue.Text;
                //chartSettings.Y2AxisLegendTitle = txtY2AxisLegendTitle.Text;

                //switch (cmbXAxisLabelType.SelectedIndex)
                //{
                //    case 3:
                //        chartSettings.XAxisLabelType = XAxisLabelType.Custom;
                //        break;
                //    case 1:
                //        chartSettings.XAxisLabelType = XAxisLabelType.FieldPrompt;
                //        Field field = DashboardHelper.GetAssociatedField(GadgetOptions.MainVariableName);
                //        if (field == null)
                //        {
                //            chartSettings.XAxisLabel = GadgetOptions.MainVariableName;
                //        }
                //        else
                //        {
                //            chartSettings.XAxisLabel = ((IDataField)field).PromptText;
                //        }
                //        break;
                //    case 2:
                //        chartSettings.XAxisLabelType = XAxisLabelType.None;
                //        break;
                //    default:
                //        chartSettings.XAxisLabelType = XAxisLabelType.Automatic;
                //        chartSettings.XAxisLabel = GadgetOptions.MainVariableName;
                //        break;
                //}

                //switch (cmbLineType.SelectedIndex)
                //{
                //    case 1:
                //        chartSettings.LineKind = LineKind.Polygon;
                //        break;
                //    case 2:
                //        chartSettings.LineKind = LineKind.Smooth;
                //        break;
                //    case 3:
                //        chartSettings.LineKind = LineKind.Step;
                //        break;
                //    default:
                //    case 0:
                //        chartSettings.LineKind = LineKind.Auto;
                //        break;
                //}

                //switch (cmbLineTypeY2.SelectedIndex)
                //{
                //    case 1:
                //        chartSettings.LineKindY2 = LineKind.Polygon;
                //        break;
                //    case 2:
                //        chartSettings.LineKindY2 = LineKind.Smooth;
                //        break;
                //    case 3:
                //        chartSettings.LineKindY2 = LineKind.Step;
                //        break;
                //    default:
                //    case 0:
                //        chartSettings.LineKindY2 = LineKind.Auto;
                //        break;
                //}

                //switch (cmbLineDashTypeY2.SelectedIndex)
                //{
                //    case 0:
                //        chartSettings.LineDashStyleY2 = LineDashStyle.Dash;
                //        break;
                //    case 1:
                //        chartSettings.LineDashStyleY2 = LineDashStyle.DashDot;
                //        break;
                //    case 2:
                //        chartSettings.LineDashStyleY2 = LineDashStyle.DashDotDot;
                //        break;
                //    case 3:
                //        chartSettings.LineDashStyleY2 = LineDashStyle.Dot;
                //        break;
                //    default:
                //        chartSettings.LineDashStyleY2 = LineDashStyle.Solid;
                //        break;
                //}

                //chartSettings.LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Right;

                //if (cmbLegendDock.SelectedItem.ToString().Equals(ChartingSharedStrings.LEGEND_DOCK_VALUE_LEFT))
                //{
                //    chartSettings.LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Left;
                //}
                //else if (cmbLegendDock.SelectedItem.ToString().Equals(ChartingSharedStrings.LEGEND_DOCK_VALUE_RIGHT))
                //{
                //    chartSettings.LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Right;
                //}
                //else if (cmbLegendDock.SelectedItem.ToString().Equals(ChartingSharedStrings.LEGEND_DOCK_VALUE_TOP))
                //{
                //    chartSettings.LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Top;
                //}
                //else if (cmbLegendDock.SelectedItem.ToString().Equals(ChartingSharedStrings.LEGEND_DOCK_VALUE_BOTTOM))
                //{
                //    chartSettings.LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Bottom;
                //}

                //int lineThicknessY2 = cmbLineThicknessY2.SelectedIndex + 1;
                //chartSettings.Y2LineThickness = (double)lineThicknessY2;

                //int lineThicknessY1 = cmbLineThickness.SelectedIndex + 1;
                //chartSettings.LineThickness = (double)lineThicknessY1;

                //EpiDashboard.Controls.Charting.LineChart lineChart = new EpiDashboard.Controls.Charting.LineChart(DashboardHelper, GadgetOptions, chartSettings, dataList);
                EpiDashboard.Controls.Charting.LineChart lineChart = new EpiDashboard.Controls.Charting.LineChart(DashboardHelper, LCParameters, dataList);
                lineChart.Margin = new Thickness(0, 0, 0, 16);
                lineChart.MouseEnter += new MouseEventHandler(chart_MouseEnter);
                lineChart.MouseLeave += new MouseEventHandler(chart_MouseLeave);
                panelMain.Children.Add(lineChart);
            }
        }

        private void EnableDisableY2Fields()
        {
            //if (cmbSecondYAxis.SelectedIndex == 0)
            //{
            //    tblockSecondYAxisVariable.Visibility = System.Windows.Visibility.Collapsed;
            //    cmbSecondYAxisVariable.Visibility = System.Windows.Visibility.Collapsed;
            //    cmbSecondYAxisVariable.SelectedIndex = -1;
            //    checkboxAnnotationsY2.IsEnabled = false;
            //    checkboxAnnotationsY2.IsChecked = false;
            //    cmbLineDashTypeY2.IsEnabled = false;
            //    cmbLineThicknessY2.IsEnabled = false;
            //    cmbLineTypeY2.IsEnabled = false;
            //    txtY2AxisLabelValue.IsEnabled = false;
            //}
            //else
            //{
            //    tblockSecondYAxisVariable.Visibility = System.Windows.Visibility.Visible;
            //    cmbSecondYAxisVariable.Visibility = System.Windows.Visibility.Visible;
            //    checkboxAnnotationsY2.IsEnabled = true;
            //    cmbLineDashTypeY2.IsEnabled = true;
            //    cmbLineThicknessY2.IsEnabled = true;
            //    cmbLineTypeY2.IsEnabled = true;
            //    txtY2AxisLabelValue.IsEnabled = true;

            //    if (cmbSecondYAxis.SelectedIndex == 1)
            //    {
            //        tblockSecondYAxisVariable.Text = "Second y-axis variable:";
            //    }
            //    else if (cmbSecondYAxis.SelectedIndex == 2)
            //    {
            //        tblockSecondYAxisVariable.Text = "Population variable:";
            //    }
            //}
        }

        //private void cmbSecondYAxis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (LoadingCombos || tblockSecondYAxisVariable == null || cmbSecondYAxisVariable == null) return;

        //    EnableDisableY2Fields();
        //}

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

                //string freqVar = GadgetOptions.MainVariableName;
                //string weightVar = GadgetOptions.WeightVariableName;
                //string strataVar = string.Empty;
                //bool includeMissing = GadgetOptions.ShouldIncludeMissing;
                //string crosstabVar = GadgetOptions.CrosstabVariableName;
                string freqVar = LineChartParameters.ColumnNames[0];
                string weightVar = LineChartParameters.WeightVariableName;
                string crosstabVar = LineChartParameters.CrosstabVariableName;

                bool includeMissing = LineChartParameters.IncludeMissing;
                
                List<string> stratas = new List<string>();

                try
                {
                    RequestUpdateStatusDelegate requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
                    CheckForCancellationDelegate checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

                    //GadgetOptions.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                    //GadgetOptions.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);
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
                            //GadgetParameters parameters = new GadgetParameters(GadgetOptions);
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
                        //Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables = DashboardHelper.GenerateFrequencyTable(GadgetOptions);
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
                        //CreateInputVariableList();
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
                switch (child.Name.ToLower())
                {
                    case "allvalues":
                        if (child.InnerText.ToLower().Equals("true"))
                        {
                            //checkboxAllValues.IsChecked = true;
                            ((LineChartParameters)Parameters).ShowAllListValues = true;
                        }
                        else
                        {
                            ((LineChartParameters)Parameters).ShowAllListValues = false;
                        }
                        break;
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
                    case "singlevariable":
                        //cmbField.Text = child.InnerText.Replace("&lt;", "<");
                        if ((this.Parameters.ColumnNames.Count > 0) && (!String.IsNullOrEmpty(child.InnerText.Trim())))
                        {
                            ((LineChartParameters)Parameters).ColumnNames[0] = child.InnerText.Replace("&lt;", "<");
                        }
                        break;
                    case "weightvariable":
                        //cmbFieldWeight.Text = child.InnerText.Replace("&lt;", "<");
                        if (!String.IsNullOrEmpty(child.InnerText.Trim()))
                        {
                            ((LineChartParameters)Parameters).WeightVariableName = child.InnerText.Replace("&lt;", "<");
                        }
                        break;
                    case "stratavariable":
                        if (!String.IsNullOrEmpty(child.InnerText.Trim()))
                        {
                            //listboxFieldStrata.SelectedItems.Add(child.InnerText.Replace("&lt;", "<"));
                            ((LineChartParameters)Parameters).StrataVariableNames[0] = child.InnerText.Replace("&lt;", "<");
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
                        //this.CustomOutputDescription = child.InnerText;
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
                            //case "Small":
                            //    txtWidth.Text = "400";
                            //    txtHeight.Text = "250";
                            //    break;
                            //case "Medium":
                            //    txtWidth.Text = "533";
                            //    txtHeight.Text = "333";
                            //    break;
                            //case "Large":
                            //    txtWidth.Text = "800";
                            //    txtHeight.Text = "500";
                            //    break;
                            //case "Custom":
                            //    break;
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
                            //txtWidth.Text = child.InnerText;
                            ((LineChartParameters)Parameters).ChartWidth = int.Parse(child.InnerText.Replace("&lt;", "<"));
                        }
                        break;
                    case "chartheight":
                        if (!string.IsNullOrEmpty(child.InnerText))
                        {
                            //txtHeight.Text = child.InnerText;
                            ((LineChartParameters)Parameters).ChartHeight = int.Parse(child.InnerText.Replace("&lt;", "<"));
                        }
                        break;
                    case "charttitle":
                        //txtChartTitle.Text = child.InnerText;
                        if (!string.IsNullOrEmpty(child.InnerText))
                        {
                            ((LineChartParameters)Parameters).ChartTitle = child.InnerText.Replace("&lt;", "<");
                        }
                        break;
                    case "xaxislabel":
                        //txtXAxisLabelValue.Text = child.InnerText;
                        //cmbXAxisLabelType.SelectedIndex = 3;
                        if (!string.IsNullOrEmpty(child.InnerText))
                        {
                            ((LineChartParameters)Parameters).XAxisLabel = child.InnerText.Replace("&lt;", "<");
                        }
                        break;
                    case "yaxislabel":
                        //txtYAxisLabelValue.Text = child.InnerText;
                        if (!string.IsNullOrEmpty(child.InnerText))
                        {
                            ((LineChartParameters)Parameters).YAxisLabel = child.InnerText.Replace("&lt;", "<");
                        }
                        break;
                    case "xaxisrotation":
                        switch (child.InnerText)
                        {
                            //case "0":
                            //    txtXAxisAngle.Text = "0";
                            //    break;
                            //case "45":
                            //    txtXAxisAngle.Text = "-45";
                            //    break;
                            //case "90":
                            //    txtXAxisAngle.Text = "-90";
                            //    break;
                            //default:
                            //    txtXAxisAngle.Text = "0";
                            //    break;
                            case "0":
                                //txtXAxisAngle.Text = "0";
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
            //HideConfigPanel();
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
                        switch (child.Name.ToLower())
                        {
                            case "mainvariable":
                                //cmbField.Text = child.InnerText.Replace("&lt;", "<");
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
                                //listboxFieldStrata.SelectedItems.Add(child.InnerText.Replace("&lt;", "<"));
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
                                //foreach (XmlElement field in child.ChildNodes)
                                //{
                                //    List<string> fields = new List<string>();
                                //    if (field.Name.ToLower().Equals("stratavariable"))
                                //    {
                                //        listboxFieldStrata.SelectedItems.Add(field.InnerText.Replace("&lt;", "<"));
                                //    }
                                //}
                                foreach (XmlElement field in child.ChildNodes)
                                {
                                    List<string> fields = new List<string>();
                                    if (field.Name.ToLower().Equals("stratavariable"))
                                    {
                                        ((LineChartParameters)Parameters).StrataVariableNames.Add(field.InnerText.Replace("&lt;", "<"));
                                    }
                                }
                                break;
                            case "weightvariable":
                                //cmbFieldWeight.Text = child.InnerText.Replace("&lt;", "<");
                                    ((LineChartParameters)Parameters).WeightVariableName = child.InnerText.Replace("&lt;", "<");
                                break;
                            case "crosstabvariable":
                                //cmbFieldCrosstab.Text = child.InnerText.Replace("&lt;", "<");
                                    ((LineChartParameters)Parameters).CrosstabVariableName = child.InnerText.Replace("&lt;", "<");
                                break;
                            case "secondyvar":
                                    //cmbSecondYAxisVariable.Text = child.InnerText.Replace("&lt;", "<");
                                    //cmbSecondYAxisVariable.Visibility = System.Windows.Visibility.Visible;
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
                                    Y2Type y2Type = ((Y2Type)Int32.Parse(child.InnerText));
                                    //switch (y2Type)
                                    //{
                                    //    case Y2Type.None:
                                    //        cmbSecondYAxis.SelectedIndex = 0;
                                    //        tblockSecondYAxisVariable.Visibility = System.Windows.Visibility.Collapsed;
                                    //        break;
                                    //    case Y2Type.SingleField:
                                    //        cmbSecondYAxis.SelectedIndex = 1;
                                    //        tblockSecondYAxisVariable.Visibility = System.Windows.Visibility.Visible;
                                    //        break;
                                    //    case Y2Type.RatePer100kPop:
                                    //        cmbSecondYAxis.SelectedIndex = 2;
                                    //        tblockSecondYAxisVariable.Visibility = System.Windows.Visibility.Visible;
                                    //        break;
                                    //}
                                    ((LineChartParameters)Parameters).Y2AxisType = int.Parse(child.InnerText.Replace("&lt;", "<"));
                                break;
                            case "sort":
                                //if (child.InnerText.ToLower().Equals("highlow"))
                                //{
                                //    checkboxSortHighLow.IsChecked = true;
                                //}
                                if (child.InnerText.ToLower().Equals("highlow") || child.InnerText.ToLower().Equals("hightolow"))
                                {
                                    ((LineChartParameters)Parameters).SortHighToLow = true;
                                }

                                break;
                            case "allvalues":
                                //if (child.InnerText.ToLower().Equals("true")) { checkboxAllValues.IsChecked = true; }
                                //else { checkboxAllValues.IsChecked = false; }
                                if (child.InnerText.ToLower().Equals("true"))
                                {
                                    ((LineChartParameters)Parameters).ShowAllListValues = true;
                                }
                                else { ((LineChartParameters)Parameters).ShowAllListValues = false; }
                                break;
                            case "showlistlabels":
                                //if (child.InnerText.ToLower().Equals("true")) { checkboxCommentLegalLabels.IsChecked = true; }
                                //else { checkboxCommentLegalLabels.IsChecked = false; }
                                if (child.InnerText.ToLower().Equals("true"))
                                {
                                    ((LineChartParameters)Parameters).ShowCommentLegalLabels = true;
                                }
                                else { ((LineChartParameters)Parameters).ShowCommentLegalLabels = false; }
                                break;
                            case "includemissing":
                                //if (child.InnerText.ToLower().Equals("true")) { checkboxIncludeMissing.IsChecked = true; }
                                //else { checkboxIncludeMissing.IsChecked = false; }
                                if (child.InnerText.ToLower().Equals("true")) { ((LineChartParameters)Parameters).IncludeMissing = true; }
                                else { ((LineChartParameters)Parameters).IncludeMissing = false; }
                                break;
                            case "customheading":
                                //if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
                                //{
                                //    this.CustomOutputHeading = child.InnerText.Replace("&lt;", "<"); ;
                                //}
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
                                //if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
                                //{
                                //    this.CustomOutputDescription = child.InnerText.Replace("&lt;", "<");
                                //    if (!string.IsNullOrEmpty(CustomOutputDescription) && !CustomOutputHeading.Equals("(none)"))
                                //    {
                                //        descriptionPanel.Text = CustomOutputDescription;
                                //        descriptionPanel.PanelMode = EpiDashboard.Controls.GadgetDescriptionPanel.DescriptionPanelMode.DisplayMode;
                                //    }
                                //    else
                                //    {
                                //        descriptionPanel.PanelMode = EpiDashboard.Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
                                //    }
                                //}
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
                                //if (child.InnerText.ToLower().Equals("true")) { checkboxUseRefValues.IsChecked = true; }
                                //else { checkboxUseRefValues.IsChecked = false; }
                                if (child.InnerText.ToLower().Equals("true")) { ((LineChartParameters)Parameters).UseRefValues = true; }
                                else { ((LineChartParameters)Parameters).UseRefValues = false; }
                                break;
                            case "showannotations":
                                //if (child.InnerText.ToLower().Equals("true")) { checkboxAnnotations.IsChecked = true; }
                                //else { checkboxAnnotations.IsChecked = false; }
                                if (child.InnerText.ToLower().Equals("true")) { ((LineChartParameters)Parameters).ShowAnnotations = true; }
                                else { ((LineChartParameters)Parameters).ShowAnnotations = false; }
                                break;
                            case "y2showannotations":
                                //if (child.InnerText.ToLower().Equals("true")) { checkboxAnnotationsY2.IsChecked = true; }
                                //else { checkboxAnnotationsY2.IsChecked = false; }
                                if (child.InnerText.ToLower().Equals("true")) { ((LineChartParameters)Parameters).Y2ShowAnnotations = true; }
                                else { ((LineChartParameters)Parameters).Y2ShowAnnotations = false; }
                                break;
                            case "showgridlines":
                                //if (child.InnerText.ToLower().Equals("true")) { checkboxGridLines.IsChecked = true; }
                                //else { checkboxGridLines.IsChecked = false; }
                                if (child.InnerText.ToLower().Equals("true")) { ((LineChartParameters)Parameters).ShowGridLines = true; }
                                else { ((LineChartParameters)Parameters).ShowGridLines = false; }
                                break;
                            case "palette":
                                //cmbPalette.SelectedIndex = int.Parse(child.InnerText);
                                ((LineChartParameters)Parameters).Palette = int.Parse(child.InnerText);
                                break;
                            case "linetype":
                                //cmbLineType.SelectedIndex = int.Parse(child.InnerText);
                                switch (child.InnerText)
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
                                    default:
                                    case "Auto":
                                        ((LineChartParameters)Parameters).LineKind = LineKind.Auto;
                                        break;
                                }
                                break;
                            case "linethickness":
                                //cmbLineThickness.SelectedIndex = int.Parse(child.InnerText);
                                    ((LineChartParameters)Parameters).LineThickness = double.Parse(child.InnerText);
                                break;
                            case "legenddock":
                                //cmbLegendDock.SelectedIndex = int.Parse(child.InnerText);
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
                                //cmbLineTypeY2.SelectedIndex = int.Parse(child.InnerText);
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
                                    //cmbLineDashTypeY2.SelectedIndex = int.Parse(child.InnerText);
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
                                //cmbLineThicknessY2.SelectedIndex = int.Parse(child.InnerText);
                                    ((LineChartParameters)Parameters).Y2LineThickness = double.Parse(child.InnerText);
                                break;
                            case "yaxislabel":
                                //txtYAxisLabelValue.Text = child.InnerText;
                                    ((LineChartParameters)Parameters).YAxisLabel = child.InnerText;
                                break;
                            case "yaxisformatstring":
                                //txtYAxisFormatString.Text = child.InnerText;
                                    ((LineChartParameters)Parameters).YAxisFormat = child.InnerText;
                                break;
                            case "y2axisformatstring":
                                //txtY2AxisFormatString.Text = child.InnerText;
                                    ((LineChartParameters)Parameters).Y2AxisFormat = child.InnerText;
                                break;
                            case "xaxislabeltype":
                                //cmbXAxisLabelType.SelectedIndex = int.Parse(child.InnerText);
                                    ((LineChartParameters)Parameters).XAxisLabelType = int.Parse(child.InnerText);
                                break;
                            case "xaxislabel":
                                //txtXAxisLabelValue.Text = child.InnerText;
                                    ((LineChartParameters)Parameters).XAxisLabel = child.InnerText;
                                break;
                            case "xaxisangle":
                                //txtXAxisAngle.Text = child.InnerText;
                                    ((LineChartParameters)Parameters).XAxisAngle = int.Parse(child.InnerText);
                                break;
                            case "charttitle":
                                //txtChartTitle.Text = child.InnerText;
                                    ((LineChartParameters)Parameters).ChartTitle = child.InnerText;
                                break;
                            case "chartsubtitle":
                                //txtChartSubTitle.Text = child.InnerText;
                                    ((LineChartParameters)Parameters).ChartSubTitle = child.InnerText;
                                break;
                            case "showlegend":
                                //if (child.InnerText.ToLower().Equals("true")) { checkboxShowLegend.IsChecked = true; }
                                //else { checkboxShowLegend.IsChecked = false; }
                                    if (child.InnerText.ToLower().Equals("true")) { ((LineChartParameters)Parameters).ShowLegend = true; }
                                    else { ((LineChartParameters)Parameters).ShowLegend = false; }
                                break;
                            case "showlegendborder":
                                //if (child.InnerText.ToLower().Equals("true")) { checkboxShowLegendBorder.IsChecked = true; }
                                //else { checkboxShowLegendBorder.IsChecked = false; }
                                    if (child.InnerText.ToLower().Equals("true")) { ((LineChartParameters)Parameters).ShowLegendBorder = true; }
                                    else { ((LineChartParameters)Parameters).ShowLegendBorder = false; }
                                break;
                            case "showlegendvarnames":
                                //if (child.InnerText.ToLower().Equals("true")) { checkboxShowVarName.IsChecked = true; }
                                //else { checkboxShowVarName.IsChecked = false; }
                                    if (child.InnerText.ToLower().Equals("true")) { ((LineChartParameters)Parameters).ShowLegendVarNames = true; }
                                    else { ((LineChartParameters)Parameters).ShowLegendVarNames = false; }
                                break;
                            case "legendfontsize":
                                //txtLegendFontSize.Text = child.InnerText;
                                    ((LineChartParameters)Parameters).LegendFontSize = int.Parse(child.InnerText);
                                break;
                            case "height":
                                //txtHeight.Text = child.InnerText;
                                    ((LineChartParameters)Parameters).ChartHeight = double.Parse(child.InnerText);
                                break;
                            case "width":
                                //txtWidth.Text = child.InnerText;
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
            //SetXAxisLabelControls();
        }

        /// <summary>
        /// Serializes the gadget into Xml
        /// </summary>
        /// <param name="doc">The Xml docment</param>
        /// <returns>XmlNode</returns>
        public override XmlNode Serialize(XmlDocument doc)
        {
            #region PreProperties Code
            //CreateInputVariableList();

            //Dictionary<string, string> inputVariableList = GadgetOptions.InputVariableList;

            //string mainVar = string.Empty;
            //string strataVar = string.Empty;
            //string weightVar = string.Empty;
            //string second_y_var = string.Empty;
            //string crosstabVar = string.Empty;
            //string sort = string.Empty;
            //bool allValues = false;
            //bool showConfLimits = true;
            //bool showCumulativePercent = true;
            //bool includeMissing = false;

            //int height = 600;
            //int width = 800;

            //int.TryParse(txtHeight.Text, out height);
            //int.TryParse(txtWidth.Text, out width);

            //crosstabVar = GadgetOptions.CrosstabVariableName.Replace("<", "&lt;");

            //if (inputVariableList.ContainsKey("freqvar"))
            //{
            //    mainVar = inputVariableList["freqvar"].Replace("<", "&lt;");
            //}
            //if (inputVariableList.ContainsKey("weightvar"))
            //{
            //    weightVar = inputVariableList["weightvar"].Replace("<", "&lt;");
            //}
            //if (inputVariableList.ContainsKey("second_y_var"))
            //{
            //    second_y_var = inputVariableList["second_y_var"].Replace("<", "&lt;");
            //}

            //Y2Type second_y_var_type = Y2Type.None;

            //if (inputVariableList.ContainsKey("second_y_var_type"))
            //{
            //    switch(inputVariableList["second_y_var_type"]) 
            //    {
            //        case "rate_per_100k":
            //            second_y_var_type = Y2Type.RatePer100kPop;
            //            break;
            //        case "single":
            //            second_y_var_type = Y2Type.SingleField;
            //            break;
            //    }                
            //}

            //if (inputVariableList.ContainsKey("sort"))
            //{
            //    sort = inputVariableList["sort"];
            //}
            //if (inputVariableList.ContainsKey("allvalues"))
            //{
            //    allValues = bool.Parse(inputVariableList["allvalues"]);
            //}
            //if (inputVariableList.ContainsKey("showconflimits"))
            //{
            //    showConfLimits = bool.Parse(inputVariableList["showconflimits"]);
            //}
            //if (inputVariableList.ContainsKey("showcumulativepercent"))
            //{
            //    showCumulativePercent = bool.Parse(inputVariableList["showcumulativepercent"]);
            //}
            //if (inputVariableList.ContainsKey("includemissing"))
            //{
            //    includeMissing = bool.Parse(inputVariableList["includemissing"]);
            //}

            //CustomOutputHeading = headerPanel.Text;
            //CustomOutputDescription = descriptionPanel.Text;

            //string xmlString =
            //"<mainVariable>" + mainVar + "</mainVariable>";

            //if (GadgetOptions.StrataVariableNames.Count == 1)
            //{
            //    xmlString = xmlString + "<strataVariable>" + GadgetOptions.StrataVariableNames[0].Replace("<", "&lt;") + "</strataVariable>";
            //}
            //else if (GadgetOptions.StrataVariableNames.Count > 1)
            //{
            //    xmlString = xmlString + "<strataVariables>";

            //    foreach (string strataVariable in this.GadgetOptions.StrataVariableNames)
            //    {
            //        xmlString = xmlString + "<strataVariable>" + strataVariable.Replace("<", "&lt;") + "</strataVariable>";
            //    }

            //    xmlString = xmlString + "</strataVariables>";
            //}

            //xmlString = xmlString + "<weightVariable>" + weightVar + "</weightVariable>" +
            //    "<crosstabVariable>" + crosstabVar + "</crosstabVariable>" +
            //    "<secondYVarType>" + ((int)second_y_var_type).ToString() + "</secondYVarType>" +
            //    "<secondYVar>" + second_y_var + "</secondYVar>" +
            //    "<height>" + height + "</height>" +
            //    "<width>" + width + "</width>" +

            //"<sort>" + sort + "</sort>" +
            //"<allValues>" + allValues + "</allValues>" +            
            //"<showListLabels>" + checkboxCommentLegalLabels.IsChecked + "</showListLabels>" +         
            //"<includeMissing>" + includeMissing + "</includeMissing>" +
            //"<customHeading>" + CustomOutputHeading.Replace("<", "&lt;") + "</customHeading>" +
            //"<customDescription>" + CustomOutputDescription.Replace("<", "&lt;") + "</customDescription>" +
            //"<customCaption>" + CustomOutputCaption + "</customCaption>";
            
            //xmlString += "<useRefValues>" + checkboxUseRefValues.IsChecked.Value + "</useRefValues>";
            //xmlString += "<showAnnotations>" + checkboxAnnotations.IsChecked.Value + "</showAnnotations>";
            //xmlString += "<y2showAnnotations>" + checkboxAnnotationsY2.IsChecked.Value + "</y2showAnnotations>";
            //xmlString += "<showGridLines>" + checkboxGridLines.IsChecked.Value + "</showGridLines>";
            
            //xmlString += "<palette>" + cmbPalette.SelectedIndex + "</palette>";
            //xmlString += "<lineType>" + cmbLineType.SelectedIndex + "</lineType>";
            //xmlString += "<lineThickness>" + cmbLineThickness.SelectedIndex + "</lineThickness>";

            //xmlString += "<y2LineType>" + cmbLineTypeY2.SelectedIndex + "</y2LineType>";
            //xmlString += "<y2LineDashStyle>" + cmbLineDashTypeY2.SelectedIndex + "</y2LineDashStyle>";
            //xmlString += "<y2LineThickness>" + cmbLineThicknessY2.SelectedIndex + "</y2LineThickness>";

            //xmlString += "<yAxisLabel>" + txtYAxisLabelValue.Text + "</yAxisLabel>";
            //xmlString += "<yAxisFormatString>" + txtYAxisFormatString.Text + "</yAxisFormatString>";
            //xmlString += "<y2AxisLabel>" + txtY2AxisLabelValue.Text + "</y2AxisLabel>";
            //xmlString += "<y2AxisLegendTitle>" + txtY2AxisLegendTitle.Text + "</y2AxisLegendTitle>";
            //xmlString += "<y2AxisFormatString>" + txtY2AxisFormatString.Text + "</y2AxisFormatString>";
            //xmlString += "<xAxisLabelType>" + cmbXAxisLabelType.SelectedIndex + "</xAxisLabelType>";
            //xmlString += "<xAxisLabel>" + txtXAxisLabelValue.Text + "</xAxisLabel>";
            //xmlString += "<xAxisAngle>" + txtXAxisAngle.Text + "</xAxisAngle>";
            //xmlString += "<chartTitle>" + txtChartTitle.Text + "</chartTitle>";
            //xmlString += "<chartSubTitle>" + txtChartSubTitle.Text + "</chartSubTitle>";

            //xmlString += "<showLegend>" + checkboxShowLegend.IsChecked.Value + "</showLegend>";
            //xmlString += "<showLegendBorder>" + checkboxShowLegendBorder.IsChecked.Value + "</showLegendBorder>";
            //xmlString += "<showLegendVarNames>" + checkboxShowVarName.IsChecked.Value + "</showLegendVarNames>";
            //xmlString += "<legendFontSize>" + txtLegendFontSize.Text + "</legendFontSize>";

            //xmlString += "<legendDock>" + cmbLegendDock.SelectedIndex + "</legendDock>";

            //xmlString = xmlString + SerializeAnchors();

            //System.Xml.XmlElement element = doc.CreateElement("lineChartGadget");
            //element.InnerXml = xmlString;
            //element.AppendChild(SerializeFilters(doc));

            //System.Xml.XmlAttribute id = doc.CreateAttribute("id");
            //System.Xml.XmlAttribute locationY = doc.CreateAttribute("top");
            //System.Xml.XmlAttribute locationX = doc.CreateAttribute("left");
            //System.Xml.XmlAttribute collapsed = doc.CreateAttribute("collapsed");
            //System.Xml.XmlAttribute type = doc.CreateAttribute("gadgetType");

            //id.Value = this.UniqueIdentifier.ToString();
            //locationY.Value = Canvas.GetTop(this).ToString("F0");
            //locationX.Value = Canvas.GetLeft(this).ToString("F0");
            //collapsed.Value = "false"; // currently no way to collapse the gadget, so leave this 'false' for now
            //type.Value = "EpiDashboard.Gadgets.Charting.LineChartGadget";

            //element.Attributes.Append(locationY);
            //element.Attributes.Append(locationX);
            //element.Attributes.Append(collapsed);
            //element.Attributes.Append(type);
            //element.Attributes.Append(id);
            #endregion //PreProperties Code

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

            id.Value = this.UniqueIdentifier.ToString();
            locationY.Value = Canvas.GetTop(this).ToString("F0");
            locationX.Value = Canvas.GetLeft(this).ToString("F0");
            collapsed.Value = "false"; // currently no way to collapse the gadget, so leave this 'false' for now
            type.Value = "EpiDashboard.Gadgets.Charting.LineChartGadget";

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);
            element.Attributes.Append(id);

            //this.CustomOutputHeading = LineChartParameters.GadgetTitle;
            //this.CustomOutputDescription = LineChartParameters.GadgetDescription;
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

        public override void ShowHideConfigPanel()
        {
            Popup = new DashboardPopup();
            Popup.Parent = ((this.Parent as DragCanvas).Parent as ScrollViewer).Parent as Grid;
            Controls.GadgetProperties.LineChartProperties properties = new Controls.GadgetProperties.LineChartProperties(this.DashboardHelper, this, (LineChartParameters)Parameters, StrataGridList);

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

        /// <summary>
        /// Updates the variable names available in the gadget's properties
        /// </summary>
        public override void UpdateVariableNames()
        {
            FillComboboxes(true);
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0)
        {
            LineChartParameters LineChartParameters = (LineChartParameters)Parameters;
            StringBuilder sb = new StringBuilder();

            //sb.AppendLine("<h2>" + this.txtChartTitle.Text + "</h2>");
            //sb.AppendLine("<h3>" + this.txtChartSubTitle.Text + "</h3>");
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
