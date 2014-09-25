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
    /// Interaction logic for AreaChartGadget.xaml
    /// </summary>
    public partial class AreaChartGadget : ChartGadgetBase
    {
        #region Constructors

        public AreaChartGadget()
        {
            InitializeComponent();
            Construct();
        }

        public AreaChartGadget(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            Construct();
            //FillComboboxes();
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
                //MOVED TO AREA CHART PROPERTIES
                //Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

                //GadgetOptions.MainVariableName = string.Empty;
                //GadgetOptions.WeightVariableName = string.Empty;
                //GadgetOptions.StrataVariableNames = new List<string>();
                //GadgetOptions.CrosstabVariableName = string.Empty;
                //GadgetOptions.ColumnNames = new List<string>();
                //GadgetOptions.ShouldIgnoreRowLimits = true;

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
                this.Parameters = new AreaChartParameters();

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
                //tblockComposition.Text = ChartingSharedStrings.COMPOSITION;
                //tblockAreaType.Text = ChartingSharedStrings.AREA_TYPE;

                //tblockTransTop.Text = ChartingSharedStrings.TRANS_PCT_TOP;
                //tblockTransBottom.Text = ChartingSharedStrings.TRANS_PCT_BOTTOM;

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

                //double legendFontSize = 13;
                //double.TryParse(txtLegendFontSize.Text, out legendFontSize);

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
                AreaChartParameters areaParameters = (AreaChartParameters)Parameters;

                if (dataList.Count > 0)
                {
                    //AreaChartSettings chartSettings = new AreaChartSettings();
                    //chartSettings.ChartTitle = txtChartTitle.Text;
                    //chartSettings.ChartSubTitle = txtChartSubTitle.Text;
                    if (strata != null)
                    {
                        //chartSettings.ChartStrataTitle = strata.Filter;
                        areaParameters.ChartStrataTitle = strata.Filter;
                    }
                    else
                    {
                        areaParameters.ChartStrataTitle = String.Empty;
                    }
                    //chartSettings.ChartWidth = int.Parse(txtWidth.Text);
                    //chartSettings.ChartHeight = int.Parse(txtHeight.Text);
                    //chartSettings.ShowDefaultGridLines = (bool)checkboxGridLines.IsChecked;

                    //switch (cmbAreaType.SelectedIndex)
                    //{
                    //    case 1:
                    //        chartSettings.AreaType = LineKind.Polygon;
                    //        break;
                    //    case 2:
                    //        chartSettings.AreaType = LineKind.Smooth;
                    //        break;
                    //    case 3:
                    //        chartSettings.AreaType = LineKind.Step;
                    //        break;
                    //    default:
                    //    case 0:
                    //        chartSettings.AreaType = LineKind.Auto;
                    //        break;
                    //}

                    //switch (cmbComposition.SelectedIndex)
                    //{
                    //    case 0:
                    //        chartSettings.Composition = CompositionKind.SideBySide;
                    //        break;
                    //    case 1:
                    //        chartSettings.Composition = CompositionKind.Stacked;
                    //        break;
                    //    case 2:
                    //        chartSettings.Composition = CompositionKind.Stacked100;
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

                    //chartSettings.LegendFontSize = double.Parse(txtLegendFontSize.Text);

                    //ComboBoxItem cbi = cmbPalette.SelectedItem as ComboBoxItem;
                    //ComponentArt.Win.DataVisualization.Palette palette = ComponentArt.Win.DataVisualization.Palette.GetPalette(cbi.Content.ToString());
                    //chartSettings.Palette = palette;

                    //chartSettings.ShowSeriesLine = (bool)checkboxShowSeriesLine.IsChecked;
                    //chartSettings.ShowLegend = (bool)checkboxShowLegend.IsChecked;
                    //chartSettings.ShowLegendBorder = (bool)checkboxShowLegendBorder.IsChecked;
                    //chartSettings.ShowLegendVarNames = (bool)checkboxShowVarName.IsChecked;
                    //chartSettings.UseRefValues = (bool)checkboxUseRefValues.IsChecked;
                    //chartSettings.XAxisLabel = txtXAxisLabelValue.Text;
                    //chartSettings.XAxisLabelRotation = int.Parse(txtXAxisAngle.Text);
                    //chartSettings.YAxisLabel = txtYAxisLabelValue.Text;
                    //chartSettings.YAxisFormattingString = txtYAxisFormatString.Text;
                    //chartSettings.Y2AxisLabel = txtY2AxisLabelValue.Text;
                    //chartSettings.Y2AxisLegendTitle = txtY2AxisLegendTitle.Text;
                    //chartSettings.Y2AxisFormattingString = txtY2AxisFormatString.Text;

                    //int pctTop = 100;
                    //int pctBottom = 100;

                    //int.TryParse(txtTransTop.Text, out pctTop);
                    //int.TryParse(txtTransBottom.Text, out pctBottom);

                    //chartSettings.TransparencyTop = pctTop;
                    //chartSettings.TransparencyBottom = pctBottom;

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

                    //int lineThicknessY2 = cmbLineThicknessY2.SelectedIndex + 1;
                    //chartSettings.Y2LineThickness = (double)lineThicknessY2;

                    //Controls.Charting.AreaChart areaChart = new Controls.Charting.AreaChart(DashboardHelper, GadgetOptions, chartSettings, dataList);
                    EpiDashboard.Controls.Charting.AreaChart areaChart = new EpiDashboard.Controls.Charting.AreaChart(DashboardHelper, areaParameters, dataList);
                    areaChart.Margin = new Thickness(0, 0, 0, 16);
                    areaChart.MouseEnter += new MouseEventHandler(chart_MouseEnter);
                    areaChart.MouseLeave += new MouseEventHandler(chart_MouseLeave);
                    panelMain.Children.Add(areaChart);
                }
            }

            //private void EnableDisableY2Fields()
            //{
            //    if (cmbSecondYAxis.SelectedIndex == 0)
            //    {
            //        tblockSecondYAxisVariable.Visibility = System.Windows.Visibility.Collapsed;
            //        cmbSecondYAxisVariable.Visibility = System.Windows.Visibility.Collapsed;
            //        cmbSecondYAxisVariable.SelectedIndex = -1;
            //        checkboxAnnotationsY2.IsEnabled = false;
            //        checkboxAnnotationsY2.IsChecked = false;
            //        cmbLineDashTypeY2.IsEnabled = false;
            //        cmbLineThicknessY2.IsEnabled = false;
            //        cmbLineTypeY2.IsEnabled = false;
            //        txtY2AxisLabelValue.IsEnabled = false;
            //    }
            //    else
            //    {
            //        tblockSecondYAxisVariable.Visibility = System.Windows.Visibility.Visible;
            //        cmbSecondYAxisVariable.Visibility = System.Windows.Visibility.Visible;
            //        checkboxAnnotationsY2.IsEnabled = true;
            //        cmbLineDashTypeY2.IsEnabled = true;
            //        cmbLineThicknessY2.IsEnabled = true;
            //        cmbLineTypeY2.IsEnabled = true;
            //        txtY2AxisLabelValue.IsEnabled = true;

            //        if (cmbSecondYAxis.SelectedIndex == 1)
            //        {
            //            tblockSecondYAxisVariable.Text = "Second y-axis variable:";
            //        }
            //        else if (cmbSecondYAxis.SelectedIndex == 2)
            //        {
            //            tblockSecondYAxisVariable.Text = "Population variable:";
            //        }
            //    }
            //}

        #endregion // Private and Protected Methods

        #region Public Methods

            public override void SetGadgetToProcessingState()
            {
                this.IsProcessing = true;
                base.SetGadgetToProcessingState();
                //this.panelAdvanced1.IsEnabled = false;
                //this.panelAdvanced2.IsEnabled = false;
                //this.panelAdvanced3.IsEnabled = false;
                //this.panelDisplay1.IsEnabled = false;
                //this.panelDisplay2.IsEnabled = false;
                //this.panelDisplay3.IsEnabled = false;

                //this.btnRun.IsEnabled = false;
            }

            public override void SetGadgetToFinishedState()
            {
                this.IsProcessing = false;
                base.SetGadgetToFinishedState();

                //this.panelAdvanced1.IsEnabled = true;
                //this.panelAdvanced2.IsEnabled = true;
                //this.panelAdvanced3.IsEnabled = true;
                //this.panelDisplay1.IsEnabled = true;
                //this.panelDisplay2.IsEnabled = true;
                //this.panelDisplay3.IsEnabled = true;

                //this.btnRun.IsEnabled = true;

                //EnableDisableY2Fields();

            }

            public override void RefreshResults()
            {
                AreaChartParameters areaParameters = (AreaChartParameters)Parameters;
                //if (!LoadingCombos && GadgetOptions != null && cmbField.SelectedIndex > -1)
                if (!LoadingCombos)
                {
                    if (areaParameters != null)
                    {
                        if (areaParameters.ColumnNames.Count > 0 && !String.IsNullOrEmpty(areaParameters.ColumnNames[0]))
                        {
                            //CreateInputVariableList();
                            //SetVisuals();
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

            public override void CreateFromXml(XmlElement element)
            {
                this.LoadingCombos = true;
                this.Parameters = new AreaChartParameters();

                HideConfigPanel();
                infoPanel.Visibility = System.Windows.Visibility.Collapsed;
                messagePanel.Visibility = System.Windows.Visibility.Collapsed;

                foreach (XmlElement child in element.ChildNodes)
                {
                    #region Original code
                    //switch (child.Name.ToLower())
                    //{
                        //case "mainvariable":
                        //    cmbField.Text = child.InnerText.Replace("&lt;", "<");
                        //    break;
                        //case "stratavariable":
                        //    listboxFieldStrata.SelectedItems.Add(child.InnerText.Replace("&lt;", "<"));
                        //    break;
                        //case "stratavariables":
                        //    foreach (XmlElement field in child.ChildNodes)
                        //    {
                        //        List<string> fields = new List<string>();
                        //        if (field.Name.ToLower().Equals("stratavariable"))
                        //        {
                        //            listboxFieldStrata.SelectedItems.Add(field.InnerText.Replace("&lt;", "<"));
                        //        }
                        //    }
                        //    break;
                        //case "weightvariable":
                        //    cmbFieldWeight.Text = child.InnerText.Replace("&lt;", "<");
                        //    break;
                        //case "crosstabvariable":
                        //    cmbFieldCrosstab.Text = child.InnerText.Replace("&lt;", "<");
                        //    break;
                        //case "secondyvar":
                        //    cmbSecondYAxisVariable.Text = child.InnerText.Replace("&lt;", "<");
                        //    cmbSecondYAxisVariable.Visibility = System.Windows.Visibility.Visible;
                        //    break;
                        //case "secondyvartype":
                        //    Y2Type y2Type = ((Y2Type)Int32.Parse(child.InnerText));
                        //    switch (y2Type)
                        //    {
                        //        case Y2Type.None:
                        //            cmbSecondYAxis.SelectedIndex = 0;
                        //            tblockSecondYAxisVariable.Visibility = System.Windows.Visibility.Collapsed;
                        //            break;
                        //        case Y2Type.SingleField:
                        //            cmbSecondYAxis.SelectedIndex = 1;
                        //            tblockSecondYAxisVariable.Visibility = System.Windows.Visibility.Visible;
                        //            break;
                        //        case Y2Type.RatePer100kPop:
                        //            cmbSecondYAxis.SelectedIndex = 2;
                        //            tblockSecondYAxisVariable.Visibility = System.Windows.Visibility.Visible;
                        //            break;
                        //    }
                        //    break;
                        //case "sort":
                        //    if (child.InnerText.ToLower().Equals("highlow"))
                        //    {
                        //        checkboxSortHighLow.IsChecked = true;
                        //    }
                        //    break;
                        //case "allvalues":
                        //    if (child.InnerText.ToLower().Equals("true")) { checkboxAllValues.IsChecked = true; }
                        //    else { checkboxAllValues.IsChecked = false; }
                        //    break;
                        //case "showlistlabels":
                        //    if (child.InnerText.ToLower().Equals("true")) { checkboxCommentLegalLabels.IsChecked = true; }
                        //    else { checkboxCommentLegalLabels.IsChecked = false; }
                        //    break;
                        //case "includemissing":
                        //    if (child.InnerText.ToLower().Equals("true")) { checkboxIncludeMissing.IsChecked = true; }
                        //    else { checkboxIncludeMissing.IsChecked = false; }
                        //    break;
                        //case "customheading":
                        //    if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
                        //    {
                        //        this.CustomOutputHeading = child.InnerText.Replace("&lt;", "<"); ;
                        //    }
                        //    break;
                        //case "customdescription":
                        //    if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
                        //    {
                        //        this.CustomOutputDescription = child.InnerText.Replace("&lt;", "<");
                        //        if (!string.IsNullOrEmpty(CustomOutputDescription) && !CustomOutputHeading.Equals("(none)"))
                        //        {
                        //            descriptionPanel.Text = CustomOutputDescription;
                        //            descriptionPanel.PanelMode = EpiDashboard.Controls.GadgetDescriptionPanel.DescriptionPanelMode.DisplayMode;
                        //        }
                        //        else
                        //        {
                        //            descriptionPanel.PanelMode = EpiDashboard.Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
                        //        }
                        //    }
                        //    break;
                        //case "customcaption":
                        //    this.CustomOutputCaption = child.InnerText;
                        //    break;
                        //case "datafilters":
                        //    this.DataFilters = new DataFilters(this.DashboardHelper);
                        //    this.DataFilters.CreateFromXml(child);
                        //    break;
                        //case "usediffbarcolors":
                        //    if (child.InnerText.ToLower().Equals("true")) { checkboxAllValues.IsChecked = true; }
                        //    else { checkboxAllValues.IsChecked = false; }
                        //    break;
                        //case "userefvalues":
                        //    if (child.InnerText.ToLower().Equals("true")) { checkboxUseRefValues.IsChecked = true; }
                        //    else { checkboxUseRefValues.IsChecked = false; }
                        //    break;
                        //case "showannotations":
                        //    if (child.InnerText.ToLower().Equals("true")) { checkboxAnnotations.IsChecked = true; }
                        //    else { checkboxAnnotations.IsChecked = false; }
                        //    break;
                        //case "y2showannotations":
                        //    if (child.InnerText.ToLower().Equals("true")) { checkboxAnnotationsY2.IsChecked = true; }
                        //    else { checkboxAnnotationsY2.IsChecked = false; }
                        //    break;
                        //case "composition":
                        //    cmbComposition.SelectedIndex = int.Parse(child.InnerText);
                        //    break;
                        //case "showseriesline":
                        //    if (child.InnerText.ToLower().Equals("true")) { checkboxShowSeriesLine.IsChecked = true; }
                        //    else { checkboxShowSeriesLine.IsChecked = false; }
                        //    break;
                        //case "showgridlines":
                        //    if (child.InnerText.ToLower().Equals("true")) { checkboxGridLines.IsChecked = true; }
                        //    else { checkboxGridLines.IsChecked = false; }
                        //    break;
                        //case "palette":
                        //    cmbPalette.SelectedIndex = int.Parse(child.InnerText);
                        //    break;
                        //case "areatype":
                        //    cmbAreaType.SelectedIndex = int.Parse(child.InnerText);
                        //    break;
                        //case "y2linetype":
                        //    cmbLineTypeY2.SelectedIndex = int.Parse(child.InnerText);
                        //    break;
                        //case "y2linedashstyle":
                        //    cmbLineDashTypeY2.SelectedIndex = int.Parse(child.InnerText);
                        //    break;
                        //case "y2linethickness":
                        //    cmbLineThicknessY2.SelectedIndex = int.Parse(child.InnerText);
                        //    break;
                        //case "transtop":
                        //    txtTransTop.Text = child.InnerText;
                        //    break;
                        //case "transbottom":
                        //    txtTransBottom.Text = child.InnerText;
                        //    break;
                        //case "legenddock":
                        //    cmbLegendDock.SelectedIndex = int.Parse(child.InnerText);
                        //    break;
                        //case "yaxislabel":
                        //    txtYAxisLabelValue.Text = child.InnerText;
                        //    break;
                        //case "yaxisformatstring":
                        //    txtYAxisFormatString.Text = child.InnerText;
                        //    break;
                        //case "y2axisformatstring":
                        //    txtY2AxisFormatString.Text = child.InnerText;
                        //    break;
                        //case "xaxislabeltype":
                        //    cmbXAxisLabelType.SelectedIndex = int.Parse(child.InnerText);
                        //    break;
                        //case "xaxislabel":
                        //    txtXAxisLabelValue.Text = child.InnerText;
                        //    break;
                        //case "xaxisangle":
                        //    txtXAxisAngle.Text = child.InnerText;
                        //    break;
                        //case "charttitle":
                        //    txtChartTitle.Text = child.InnerText;
                        //    break;
                        //case "chartsubtitle":
                        //    txtChartSubTitle.Text = child.InnerText;
                        //    break;
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
                        //case "height":
                        //    txtHeight.Text = child.InnerText;
                        //    break;
                        //case "width":
                        //    txtWidth.Text = child.InnerText;
                        //    break;
                        #endregion //Original code

                    if (!String.IsNullOrEmpty(child.InnerText))
                    {
                        switch (child.Name.ToLower())
                        {
                            case "mainvariable":
                                if (this.Parameters.ColumnNames.Count > 0)
                                {
                                    ((AreaChartParameters)Parameters).ColumnNames[0] = (child.InnerText.Replace("&lt;", "<"));
                                }
                                else
                                {
                                    ((AreaChartParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                                }
                                break;
                            case "stratavariable":
                                if (!string.IsNullOrEmpty(child.InnerText))
                                {
                                    if (((AreaChartParameters)Parameters).StrataVariableNames.Count > 0)
                                    {
                                        ((AreaChartParameters)Parameters).StrataVariableNames[0] = child.InnerText.Replace("&lt;", "<");
                                    }
                                    else
                                    {
                                        ((AreaChartParameters)Parameters).StrataVariableNames.Add(child.InnerText.Replace("&lt;", "<"));
                                    }
                                }
                                break;
                            case "stratavariables":
                                foreach (XmlElement field in child.ChildNodes)
                                {
                                    List<string> fields = new List<string>();
                                    if (field.Name.ToLower().Equals("stratavariable"))
                                    {
                                        ((AreaChartParameters)Parameters).StrataVariableNames.Add(field.InnerText.Replace("&lt;", "<"));
                                    }
                                }
                                break;
                            case "weightvariable":
                                ((AreaChartParameters)Parameters).WeightVariableName = child.InnerText.Replace("&lt;", "<");
                                break;
                            case "crosstabvariable":
                                ((AreaChartParameters)Parameters).CrosstabVariableName = child.InnerText.Replace("&lt;", "<");
                                break;
                            case "secondyvar":
                                if (this.Parameters.ColumnNames.Count > 1)
                                {
                                    ((AreaChartParameters)Parameters).ColumnNames[1] = (child.InnerText.Replace("&lt;", "<"));
                                }
                                else
                                {
                                    ((AreaChartParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                                }
                                break;
                            case "secondyvartype":
                                Y2Type y2Type = ((Y2Type)Int32.Parse(child.InnerText));
                                ((AreaChartParameters)Parameters).Y2AxisType = int.Parse(child.InnerText.Replace("&lt;", "<"));
                                break;
                            case "sort":
                                if (child.InnerText.ToLower().Equals("highlow") || child.InnerText.ToLower().Equals("hightolow"))
                                {
                                    ((AreaChartParameters)Parameters).SortHighToLow = true;
                                }
                                break;
                            case "allvalues":
                                if (child.InnerText.ToLower().Equals("true"))
                                {
                                    ((AreaChartParameters)Parameters).ShowAllListValues = true;
                                }
                                else { ((AreaChartParameters)Parameters).ShowAllListValues = false; }
                                break;
                            case "showlistlabels":
                                if (child.InnerText.ToLower().Equals("true"))
                                {
                                    ((AreaChartParameters)Parameters).ShowCommentLegalLabels = true;
                                }
                                else { ((AreaChartParameters)Parameters).ShowCommentLegalLabels = false; }
                                break;
                            case "includemissing":
                                if (child.InnerText.ToLower().Equals("true")) { ((AreaChartParameters)Parameters).IncludeMissing = true; }
                                else { ((AreaChartParameters)Parameters).IncludeMissing = false; }
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
                            case "userefvalues":
                                if (child.InnerText.ToLower().Equals("true")) { ((AreaChartParameters)Parameters).UseRefValues = true; }
                                else { ((AreaChartParameters)Parameters).UseRefValues = false; }
                                break;
                            case "showannotations":
                                if (child.InnerText.ToLower().Equals("true")) { ((AreaChartParameters)Parameters).ShowAnnotations = true; }
                                else { ((AreaChartParameters)Parameters).ShowAnnotations = false; }
                                break;
                            case "y2showannotations":
                                if (child.InnerText.ToLower().Equals("true")) { ((AreaChartParameters)Parameters).Y2ShowAnnotations = true; }
                                else { ((AreaChartParameters)Parameters).Y2ShowAnnotations = false; }
                                break;
                            case "composition":
                                {
                                    switch (child.InnerText)
                                    {
                                        case "0":
                                            ((AreaChartParameters)Parameters).Composition = CompositionKind.SideBySide;
                                            break;
                                        case "1":
                                            ((AreaChartParameters)Parameters).Composition = CompositionKind.Stacked;
                                            break;
                                        case "2":
                                            ((AreaChartParameters)Parameters).Composition = CompositionKind.Stacked100;
                                            break;
                                    }
                                }
                                break;
                            case "showseriesline":
                                if (child.InnerText.ToLower().Equals("true")) { ((AreaChartParameters)Parameters).ShowSeriesLine = true; }
                                else { ((AreaChartParameters)Parameters).ShowSeriesLine = false; }
                                break;
                            case "showgridlines":
                                if (child.InnerText.ToLower().Equals("true")) { ((AreaChartParameters)Parameters).ShowGridLines = true; }
                                else { ((AreaChartParameters)Parameters).ShowGridLines = false; }
                                break;
                            case "palette":
                                ((AreaChartParameters)Parameters).Palette = int.Parse(child.InnerText);
                                break;
                            case "areatype":
                                {
                                    switch (child.InnerText)
                                    {
                                        case "Polygon":
                                            ((AreaChartParameters)Parameters).AreaKind = LineKind.Polygon;
                                            break;
                                        case "Smooth":
                                            ((AreaChartParameters)Parameters).AreaKind = LineKind.Smooth;
                                            break;
                                        case "Step":
                                            ((AreaChartParameters)Parameters).AreaKind = LineKind.Step;
                                            break;
                                        case "Auto":
                                            ((AreaChartParameters)Parameters).AreaKind = LineKind.Auto;
                                            break;
                                    }
                                }
                                break;
                            case "y2linetype":
                                switch (child.InnerText)
                                {
                                    case "Polygon":
                                        ((AreaChartParameters)Parameters).Y2LineKind = LineKind.Polygon;
                                        break;
                                    case "Smooth":
                                        ((AreaChartParameters)Parameters).Y2LineKind = LineKind.Smooth;
                                        break;
                                    case "Step":
                                        ((AreaChartParameters)Parameters).Y2LineKind = LineKind.Step;
                                        break;
                                    case "Auto":
                                        ((AreaChartParameters)Parameters).Y2LineKind = LineKind.Auto;
                                        break;
                                }
                                break;
                            case "y2linedashstyle":
                                {
                                    switch (child.InnerText)
                                    {
                                        case "Solid":
                                            ((AreaChartParameters)Parameters).Y2LineDashStyle = LineDashStyle.Solid;
                                            break;
                                        case "Dash":
                                            ((AreaChartParameters)Parameters).Y2LineDashStyle = LineDashStyle.Dash;
                                            break;
                                        case "Dot":
                                            ((AreaChartParameters)Parameters).Y2LineDashStyle = LineDashStyle.Dot;
                                            break;
                                        case "DashDot":
                                            ((AreaChartParameters)Parameters).Y2LineDashStyle = LineDashStyle.DashDot;
                                            break;
                                        case "DashDotDot":
                                            ((AreaChartParameters)Parameters).Y2LineDashStyle = LineDashStyle.DashDotDot;
                                            break;
                                    }
                                    break;
                                }
                            case "y2linethickness":
                                ((AreaChartParameters)Parameters).Y2LineThickness = int.Parse(child.InnerText);
                                break;
                            case "transtop":
                                ((AreaChartParameters)Parameters).TransTop = double.Parse(child.InnerText);
                                break;
                            case "transbottom":
                                ((AreaChartParameters)Parameters).TransBottom = double.Parse(child.InnerText);
                                break;
                            case "legenddock":
                                {
                                    switch (int.Parse(child.InnerText))
                                    {
                                        case 0:
                                            ((AreaChartParameters)Parameters).LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Left;
                                            break;
                                        default:
                                        case 1:
                                            ((AreaChartParameters)Parameters).LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Right;
                                            break;
                                        case 2:
                                            ((AreaChartParameters)Parameters).LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Top;
                                            break;
                                        case 3:
                                            ((AreaChartParameters)Parameters).LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Bottom;
                                            break;
                                    }
                                }
                                break;
                            case "yaxislabel":
                                ((AreaChartParameters)Parameters).YAxisLabel = child.InnerText;
                                break;
                            case "yaxisformatstring":
                                ((AreaChartParameters)Parameters).YAxisFormat = child.InnerText;
                                break;
                            case "y2axisformatstring":
                                ((AreaChartParameters)Parameters).Y2AxisFormat = child.InnerText;
                                break;
                            case "y2axislabel":
                                ((AreaChartParameters)Parameters).Y2AxisLabel = child.InnerText;
                                break;
                            case "y2axislegendtitle":
                                ((AreaChartParameters)Parameters).Y2AxisLegendTitle = child.InnerText;
                                break;
                            case "xaxislabeltype":
                                {
                                    ((AreaChartParameters)Parameters).XAxisLabelType = int.Parse(child.InnerText);
                                    break;
                                }
                            case "xaxislabel":
                                ((AreaChartParameters)Parameters).XAxisLabel = child.InnerText;
                                break;
                            case "xaxisangle":
                                ((AreaChartParameters)Parameters).XAxisAngle = int.Parse(child.InnerText);
                                break;
                            case "charttitle":
                                ((AreaChartParameters)Parameters).ChartTitle = child.InnerText;
                                break;
                            case "chartsubtitle":
                                ((AreaChartParameters)Parameters).ChartSubTitle = child.InnerText;
                                break;
                            case "showlegend":
                                if (child.InnerText.ToLower().Equals("true")) { ((AreaChartParameters)Parameters).ShowLegend = true; }
                                else { ((AreaChartParameters)Parameters).ShowLegend = false; }
                                break;
                            case "showlegendborder":
                                if (child.InnerText.ToLower().Equals("true")) { ((AreaChartParameters)Parameters).ShowLegendBorder = true; }
                                else { ((AreaChartParameters)Parameters).ShowLegendBorder = false; }
                                break;
                            case "showlegendvarnames":
                                if (child.InnerText.ToLower().Equals("true")) { ((AreaChartParameters)Parameters).ShowLegendVarNames = true; }
                                else { ((AreaChartParameters)Parameters).ShowLegendVarNames = false; }
                                break;
                            case "legendfontsize":
                                ((AreaChartParameters)Parameters).LegendFontSize = int.Parse(child.InnerText);
                                break;
                            case "height":
                                ((AreaChartParameters)Parameters).ChartHeight = double.Parse(child.InnerText);
                                break;
                            case "width":
                                ((AreaChartParameters)Parameters).ChartWidth = double.Parse(child.InnerText);
                                break;
                        }
                    }
                }

                base.CreateFromXml(element);

                this.LoadingCombos = false;
                RefreshResults();
                HideConfigPanel();
                //SetXAxisLabelControls();
            }

            public override void ShowHideConfigPanel()
            {
                Popup = new DashboardPopup();
                Popup.Parent = ((this.Parent as DragCanvas).Parent as ScrollViewer).Parent as Grid;
                Controls.GadgetProperties.AreaChartProperties properties = new Controls.GadgetProperties.AreaChartProperties(this.DashboardHelper, this, (AreaChartParameters)Parameters, StrataGridList);

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
            /// Serializes the gadget into Xml
            /// </summary>
            /// <param name="doc">The Xml docment</param>
            /// <returns>XmlNode</returns>
            public override XmlNode Serialize(XmlDocument doc)
            {
                //CreateInputVariableList();
                AreaChartParameters chtParameters = (AreaChartParameters)Parameters;
                //Dictionary<string, string> inputVariableList = GadgetOptions.InputVariableList;

                #region Original Serialize code
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
                //    switch (inputVariableList["second_y_var_type"])
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

                //xmlString += "<showSeriesLine>" + checkboxShowSeriesLine.IsChecked.Value + "</showSeriesLine>";
                //xmlString += "<useRefValues>" + checkboxUseRefValues.IsChecked.Value + "</useRefValues>";
                //xmlString += "<showAnnotations>" + checkboxAnnotations.IsChecked.Value + "</showAnnotations>";
                //xmlString += "<y2showAnnotations>" + checkboxAnnotationsY2.IsChecked.Value + "</y2showAnnotations>";
                //xmlString += "<showGridLines>" + checkboxGridLines.IsChecked.Value + "</showGridLines>";

                //xmlString += "<composition>" + cmbComposition.SelectedIndex + "</composition>";
                //xmlString += "<palette>" + cmbPalette.SelectedIndex + "</palette>";
                //xmlString += "<areaType>" + cmbAreaType.SelectedIndex + "</areaType>";

                //xmlString += "<transTop>" + txtTransTop.Text + "</transTop>";
                //xmlString += "<transBottom>" + txtTransBottom.Text + "</transBottom>";

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

                //System.Xml.XmlElement element = doc.CreateElement("areaChartGadget");
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
                //type.Value = "EpiDashboard.Gadgets.Charting.AreaChartGadget";

                //element.Attributes.Append(locationY);
                //element.Attributes.Append(locationX);
                //element.Attributes.Append(collapsed);
                //element.Attributes.Append(type);
                //element.Attributes.Append(id);

                //return element;
                #endregion // Origional Serialize code

                System.Xml.XmlElement element = doc.CreateElement("areaChartGadget");
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
                type.Value = "EpiDashboard.Gadgets.Charting.AreaChartGadget";

                element.Attributes.Append(locationY);
                element.Attributes.Append(locationX);
                element.Attributes.Append(collapsed);
                element.Attributes.Append(type);
                element.Attributes.Append(id);

                //this.CustomOutputHeading = chtParameters.GadgetTitle;
                //this.CustomOutputDescription = chtParameters.GadgetDescription;
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

                double.TryParse(chtParameters.ChartHeight.ToString(), out height);
                double.TryParse(chtParameters.ChartWidth.ToString(), out width);

                crosstabVar = chtParameters.CrosstabVariableName.Replace("<", "&lt;");

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

                //showSeriesLine
                //xmlString += "<showSeriesLine>" + checkboxShowSeriesLine.IsChecked.Value + "</showSeriesLine>";
                XmlElement showSeriesLineElement = doc.CreateElement("showSeriesLine");
                showSeriesLineElement.InnerText = chtParameters.ShowSeriesLine.ToString();
                element.AppendChild(showSeriesLineElement);

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

                //palette 
                XmlElement paletteElement = doc.CreateElement("palette");
                paletteElement.InnerText = chtParameters.Palette.ToString();
                element.AppendChild(paletteElement);

                //areaType
                //xmlString += "<areaType>" + cmbAreaType.SelectedIndex + "</areaType>";
                XmlElement areaTypeElement = doc.CreateElement("areaType");
                areaTypeElement.InnerText = chtParameters.AreaKind.ToString();
                element.AppendChild(areaTypeElement);

                //transTop
                //xmlString += "<transTop>" + txtTransTop.Text + "</transTop>";
                XmlElement transTopElement = doc.CreateElement("transTop");
                transTopElement.InnerText = chtParameters.TransTop.ToString();
                element.AppendChild(transTopElement);

                //transBottom
                //xmlString += "<transBottom>" + txtTransBottom.Text + "</transBottom>";
                XmlElement transBottomElement = doc.CreateElement("transBottom");
                transBottomElement.InnerText = chtParameters.TransBottom.ToString();
                element.AppendChild(transBottomElement);

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

        #region Event Handlers
            private void properties_ChangesAccepted(object sender, EventArgs e)
            {
                Controls.GadgetProperties.AreaChartProperties properties = Popup.Content as Controls.GadgetProperties.AreaChartProperties;
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

                    AreaChartParameters chtParameters = (AreaChartParameters)Parameters;

                    //string freqVar = GadgetOptions.MainVariableName;
                    //string weightVar = GadgetOptions.WeightVariableName;
                    //string strataVar = string.Empty;
                    //string crosstabVar = GadgetOptions.CrosstabVariableName;
                    //bool includeMissing = GadgetOptions.ShouldIncludeMissing;
                    string freqVar = chtParameters.ColumnNames[0];
                    string weightVar = chtParameters.WeightVariableName;
                    string crosstabVar = chtParameters.CrosstabVariableName;
                    bool includeMissing = chtParameters.IncludeMissing;

                    List<string> stratas = new List<string>();
                    //if (!string.IsNullOrEmpty(strataVar))
                    //{
                    //    stratas.Add(strataVar);
                    //}

                    try
                    {
                        RequestUpdateStatusDelegate requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
                        CheckForCancellationDelegate checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

                        //GadgetOptions.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                        //GadgetOptions.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);
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
                                //GadgetParameters parameters = new GadgetParameters(GadgetOptions);
                                AreaChartParameters parameters = new AreaChartParameters(chtParameters);

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
                        Debug.Print("Area chart gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete.");
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
                AreaChartParameters chtParameters = (AreaChartParameters)Parameters;
                StringBuilder sb = new StringBuilder();

                //sb.AppendLine("<h2>" + this.txtChartTitle.Text + "</h2>");
                //sb.AppendLine("<h3>" + this.txtChartSubTitle.Text + "</h3>");
                sb.AppendLine("<h2>" + chtParameters.ChartTitle + "</h2>");
                sb.AppendLine("<h3>" + chtParameters.ChartSubTitle + "</h3>");

                foreach (UIElement element in panelMain.Children)
                {
                    if (element is Controls.Charting.AreaChart)
                    {
                        sb.AppendLine(((Controls.Charting.AreaChart)element).ToHTML(htmlFileName, count, true, false));
                        count++;
                    }
                }

                return sb.ToString();
            }

        #endregion //Event Handlers
    }
}
