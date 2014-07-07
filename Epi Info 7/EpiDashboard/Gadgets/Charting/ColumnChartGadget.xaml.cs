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
using EpiDashboard.Gadgets.Charting;
using ComponentArt.Win.DataVisualization;
using ComponentArt.Win.DataVisualization.Common;
using ComponentArt.Win.DataVisualization.Charting;

namespace EpiDashboard.Gadgets.Charting
{
    /// <summary>
    /// Interaction logic for ColumnChartGadget.xaml
    /// </summary>
    public partial class ColumnChartGadget : ColumnChartGadgetBase
    {
        #region Constructors

            public ColumnChartGadget()
            {
                InitializeComponent();
                Construct();
            }

            public ColumnChartGadget(DashboardHelper dashboardHelper)
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

            protected override void Construct()
            {
                this.Parameters = new ColumnChartParameters();

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

                //ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.DateTime | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.UserDefined;
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
                //MOVED TO COLUMN CHART PROPERTIES
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
                //if (cmbSecondYAxis.SelectedIndex == 2)
                //{
                //    inputVariableList.Add("second_y_var_type", "rate_per_100k");
                //}
                //if (cmbSecondYAxis.SelectedIndex == 3)
                //{
                //    inputVariableList.Add("second_y_var_type", "cumulative_percent");
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

            private void ClearResults()
            {
                messagePanel.Visibility = System.Windows.Visibility.Collapsed;
                messagePanel.Text = string.Empty;
                descriptionPanel.PanelMode = EpiDashboard.Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;

                tblockXAxisLabel.Visibility = System.Windows.Visibility.Collapsed;
                tblockYAxisLabel.Visibility = System.Windows.Visibility.Collapsed;
                tblockChartTitle.Visibility = System.Windows.Visibility.Collapsed;

                panelMain.Children.Clear();

                StrataGridList.Clear();
                StrataExpanderList.Clear();
            }

            private void ShowLabels()
            {
                ColumnChartParameters chtParameters = (ColumnChartParameters)Parameters;
                //if (string.IsNullOrEmpty(tblockXAxisLabel.Text.Trim()))  
                if(string.IsNullOrEmpty(chtParameters.XAxisLabel))
                {
                    tblockXAxisLabel.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    tblockXAxisLabel.Visibility = System.Windows.Visibility.Visible;
                }

                //if (string.IsNullOrEmpty(tblockYAxisLabel.Text.Trim()))
                if (string.IsNullOrEmpty(chtParameters.YAxisLabel))
                {
                    tblockYAxisLabel.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    tblockYAxisLabel.Visibility = System.Windows.Visibility.Visible;
                }

                if (string.IsNullOrEmpty(chtParameters.ChartTitle))
                {
                    tblockChartTitle.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    tblockChartTitle.Visibility = System.Windows.Visibility.Visible;
                }
            }

            protected override void RenderFinish()
            {
                waitPanel.Visibility = System.Windows.Visibility.Collapsed;

                messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.StatusPanel;
                messagePanel.Text = string.Empty;
                messagePanel.Visibility = System.Windows.Visibility.Collapsed;

                //ShowLabels();
                HideConfigPanel();
                CheckAndSetPosition();
            }

            protected override void RenderFinishWithWarning(string errorMessage)
            {
                waitPanel.Visibility = System.Windows.Visibility.Collapsed; //waitCursor.Visibility = Visibility.Hidden;

                messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.WarningPanel;
                messagePanel.Text = errorMessage;
                messagePanel.Visibility = System.Windows.Visibility.Visible;

                //ShowLabels();
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
                Controls.GadgetProperties.ColumnChartProperties properties = Popup.Content as Controls.GadgetProperties.ColumnChartProperties;
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

            //private new void ChartGadgetBase_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
            //{               
            //    if (SelectedChart != null)
            //    {
            //        separatorCurrentChart.Visibility = System.Windows.Visibility.Visible;
            //        mnuCurrentChart.Visibility = System.Windows.Visibility.Visible;
            //        mnuSaveSelectedChartAsImage.Visibility = System.Windows.Visibility.Visible;
            //        mnuCopySelectedChartImage.Visibility = System.Windows.Visibility.Visible;
            //        mnuCopySelectedChartData.Visibility = System.Windows.Visibility.Visible;                
            //    }
            //    else
            //    {
            //        separatorCurrentChart.Visibility = System.Windows.Visibility.Collapsed;
            //        mnuCurrentChart.Visibility = System.Windows.Visibility.Collapsed;
            //        mnuSaveSelectedChartAsImage.Visibility = System.Windows.Visibility.Collapsed;
            //        mnuCopySelectedChartImage.Visibility = System.Windows.Visibility.Collapsed;
            //        mnuCopySelectedChartData.Visibility = System.Windows.Visibility.Collapsed;                
            //    }
            //}

            //protected virtual void mnuSaveSelectedChartAsImage_Click(object sender, RoutedEventArgs e)
            //{
            //    if (SelectedChart != null)
            //    {
            //        SelectedChart.SaveImageToFile();
            //    }
            //}

            //protected virtual void mnuCopySelectedChartImage_Click(object sender, RoutedEventArgs e)
            //{
            //    if (SelectedChart != null)
            //    {
            //        SelectedChart.CopyImageToClipboard();
            //    } 
            //}

            //protected virtual void mnuCopySelectedChartData_Click(object sender, RoutedEventArgs e)
            //{
            //    if (SelectedChart != null)
            //    {
            //        SelectedChart.CopyAllDataToClipboard();
            //    }
            //}

//  SetChartData(dataList, strata) does not appear to be used, ever.
            protected override void SetChartData(List<XYColumnChartData> dataList, Strata strata)
            {
                #region Input Validation
                if (dataList == null)
                {
                    throw new ArgumentNullException("dataList");
                }
                #endregion // Input Validation
                ColumnChartParameters chtParameters = (ColumnChartParameters)Parameters;

                if (dataList.Count > 0)
                {
                    //ColumnChartSettings chartSettings = new ColumnChartSettings();
                    //chartSettings.ChartTitle = chtParameters.ChartTitle;
                    //chartSettings.ChartSubTitle = chtParameters.ChartSubTitle;
                    if (strata != null)
                    {
                        //chartSettings.ChartStrataTitle = strata.Filter;
                        chtParameters.ChartStrataTitle = strata.Filter;
                    }
                    else
                    {
                        chtParameters.ChartStrataTitle = String.Empty;
                    }

                    //chartSettings.ChartWidth = int.Parse(chtParameters.ChartWidth.ToString());
                    //chartSettings.ChartHeight = int.Parse(chtParameters.ChartHeight);
                    //chartSettings.ShowDefaultGridLines = chtParameters.ShowGridLines;

                    //switch (chtParameters.Orientation)
                    //{
                    //    case 1:
                    //        chartSettings.Orientation = Orientation.Horizontal;
                    //        break;
                    //    case 0:
                    //    default:
                    //        chartSettings.Orientation = Orientation.Vertical;
                    //        break;
                    //}

                    //switch (chtParameters.BarType)
                    //{
                    //    case 1:
                    //        chartSettings.BarKind = BarKind.Cylinder;
                    //        break;
                    //    case 2:
                    //        chartSettings.BarKind = BarKind.Rectangle;
                    //        break;
                    //    case 3:
                    //        chartSettings.BarKind = BarKind.RoundedBlock;
                    //        break;
                    //    default:
                    //    case 0:
                    //        chartSettings.BarKind = BarKind.Block;
                    //        break;
                    //}

                    //switch (chtParameters.BarSpace)
                    //{
                    //    case 1:
                    //        chartSettings.BarSpacing = BarSpacing.None;
                    //        break;
                    //    case 2:
                    //        chartSettings.BarSpacing = BarSpacing.Small;
                    //        break;
                    //    case 3:
                    //        chartSettings.BarSpacing = BarSpacing.Medium;
                    //        break;
                    //    case 4:
                    //        chartSettings.BarSpacing = BarSpacing.Large;
                    //        break;
                    //    case 0:
                    //    default:
                    //        chartSettings.BarSpacing = BarSpacing.Default;
                    //        break;
                    //}

                    //switch (chtParameters.Composition)
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

                    //if (chtParameters.LegendDock.ToString().Equals(ChartingSharedStrings.LEGEND_DOCK_VALUE_LEFT))
                    //{
                    //    chartSettings.LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Left;
                    //}
                    //else if (chtParameters.LegendDock.ToString().Equals(ChartingSharedStrings.LEGEND_DOCK_VALUE_RIGHT))
                    //{
                    //    chartSettings.LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Right;
                    //}
                    //else if (chtParameters.LegendDock.ToString().Equals(ChartingSharedStrings.LEGEND_DOCK_VALUE_TOP))
                    //{
                    //    chartSettings.LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Top;
                    //}
                    //else if (chtParameters.LegendDock.ToString().Equals(ChartingSharedStrings.LEGEND_DOCK_VALUE_BOTTOM))
                    //{
                    //    chartSettings.LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Bottom;
                    //}

                    //chartSettings.LegendFontSize = double.Parse(chtParameters.LegendFontSize.ToString());

                    ////ComboBoxItem cbi = cmbPalette.SelectedItem as ComboBoxItem;
                    //ComponentArt.Win.DataVisualization.Palette palette = ComponentArt.Win.DataVisualization.Palette.GetPalette(chtParameters.PaletteName);
                    //chartSettings.Palette = palette;

                    //chartSettings.ShowAnnotations = chtParameters.ShowAnnotations;
                    //chartSettings.ShowAnnotationsY2 = chtParameters.Y2ShowAnnotations;
                    //chartSettings.ShowLegend = chtParameters.ShowLegend;
                    //chartSettings.ShowLegendBorder = chtParameters.ShowLegendBorder;
                    //chartSettings.ShowLegendVarNames = chtParameters.ShowLegendVarNames;
                    //chartSettings.UseDiffColors = chtParameters.UseDiffBarColors;
                    //chartSettings.UseRefValues = chtParameters.UseReferenceValues;
                    //chartSettings.XAxisLabel = chtParameters.XAxisLabel;
                    //chartSettings.XAxisLabelRotation = int.Parse(chtParameters.XAxisAngle);
                    //chartSettings.YAxisLabel = chtParameters.YAxisLabel;
                    //chartSettings.YAxisFormattingString = chtParameters.YAxisFormat;
                    //chartSettings.Y2AxisFormattingString = chtParameters.Y2AxisFormat;
                    //chartSettings.Y2AxisLabel = chtParameters.Y2AxisLabel;
                    //chartSettings.Y2AxisLegendTitle = chtParameters.Y2AxisLegendTitle;

                    ////if (cmbSecondYAxis.SelectedIndex == 3)
                    //if (chtParameters.Y2AxisType == 3)
                    //{
                    //    chartSettings.Y2AxisFormattingString = "P0";
                    //    chartSettings.Y2IsCumulativePercent = true;
                    //}

                    ////switch (cmbXAxisLabelType.SelectedIndex)
                    //switch (chtParameters.XAxisLabelType)
                    //{
                    //    case 3:
                    //        chartSettings.XAxisLabelType = XAxisLabelType.Custom;
                    //        break;
                    //    case 1:
                    //        chartSettings.XAxisLabelType = XAxisLabelType.FieldPrompt;
                    //        //Field field = DashboardHelper.GetAssociatedField(GadgetOptions.MainVariableName);
                    //        Field field = DashboardHelper.GetAssociatedField(chtParameters.ColumnNames[0].ToString());
                    //        if (field == null)
                    //        {
                    //            chartSettings.XAxisLabel = chtParameters.ColumnNames[0].ToString();
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
                    //        chartSettings.XAxisLabel = chtParameters.ColumnNames[0].ToString();
                    //        break;
                    //}

                    ////switch (cmbLineTypeY2.SelectedIndex)
                    //switch (chtParameters.Y2LineType)
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

                    ////switch (cmbLineDashTypeY2.SelectedIndex)
                    //switch (chtParameters.Y2LineDashStyle)
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

                    //int lineThickness = (int)chtParameters.Y2LineThickness + 1;
                    //chartSettings.Y2LineThickness = (double)lineThickness;


                    //EpiDashboard.Controls.Charting.ColumnChart columnChart = new EpiDashboard.Controls.Charting.ColumnChart(DashboardHelper, GadgetOptions, chartSettings, dataList);
                    EpiDashboard.Controls.Charting.ColumnChart columnChart = new EpiDashboard.Controls.Charting.ColumnChart(DashboardHelper, chtParameters, dataList);

                    columnChart.Margin = new Thickness(0, 0, 0, 16);
                    columnChart.MouseEnter += new MouseEventHandler(chart_MouseEnter);
                    columnChart.MouseLeave += new MouseEventHandler(chart_MouseLeave);
                    panelMain.Children.Add(columnChart);
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
            //        txtY2AxisFormatString.IsEnabled = false;
            //    }
            //    else if (cmbSecondYAxis.SelectedIndex == 3)
            //    {
            //        tblockSecondYAxisVariable.Visibility = System.Windows.Visibility.Collapsed;
            //        cmbSecondYAxisVariable.Visibility = System.Windows.Visibility.Collapsed;
            //        cmbSecondYAxisVariable.SelectedIndex = -1;
            //        checkboxAnnotationsY2.IsEnabled = true;
            //        cmbLineDashTypeY2.IsEnabled = true;
            //        cmbLineThicknessY2.IsEnabled = true;
            //        cmbLineTypeY2.IsEnabled = true;
            //        txtY2AxisLabelValue.IsEnabled = true;
            //        txtY2AxisFormatString.IsEnabled = false;
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
            //        txtY2AxisFormatString.IsEnabled = true;

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

            private void cmbSecondYAxis_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
            //    if (LoadingCombos || tblockSecondYAxisVariable == null || cmbSecondYAxisVariable == null) return;

            //    EnableDisableY2Fields();
            }

        #endregion // Private and Protected Methods

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
                ColumnChartParameters chtParameters = (ColumnChartParameters)Parameters;
                //if (!LoadingCombos && GadgetOptions != null && cmbField.SelectedIndex > -1)
                if (!LoadingCombos)
                {
                    if (chtParameters != null)
                    {
                        if (chtParameters.ColumnNames.Count>0 && !String.IsNullOrEmpty(chtParameters.ColumnNames[0]))
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
                bool isStacked = false;
                this.Parameters = new ColumnChartParameters();

                foreach (XmlElement child in element.ChildNodes)
                {
                    switch (child.Name.ToLower())
                    {
                        case "casestatusvariable":
                             if (!isStacked) ((ColumnChartParameters)Parameters).StrataVariableNames.Add(child.InnerText.Replace("&lt;", "<"));
                            break;
                        case "datevariable":
                            if (!isStacked && !string.IsNullOrEmpty(child.InnerText.Trim()))
                            {
                                if (this.Parameters.ColumnNames.Count > 0)
                                {
                                    ((ColumnChartParameters)Parameters).ColumnNames[0] = (child.InnerText.Replace("&lt;", "<"));
                                }
                                else
                                {
                                    ((ColumnChartParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                                }
                            }
                            break;
                        case "yaxisvariable":
                            //listboxFieldStrata.SelectedItems.Add(child.InnerText.Replace("&lt;", "<"));
                            if (string.IsNullOrEmpty(child.InnerText))
                            {
                                ((ColumnChartParameters)Parameters).StrataVariableNames.Add(child.InnerText.Replace("&lt;", "<"));
                            }
                            break;
                        case "xaxisvariable":
                            if (!string.IsNullOrEmpty(child.InnerText.Trim()))
                            {
                                //cmbField.Text = child.InnerText.Replace("&lt;", "<");
                                if (this.Parameters.ColumnNames.Count > 0)
                                {
                                    ((ColumnChartParameters)Parameters).ColumnNames[0] = (child.InnerText.Replace("&lt;", "<"));
                                }
                                else
                                {
                                    ((ColumnChartParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                                }

                            }
                            break;
                        //case "yaxisscattervariable":
                        //    cbxScatterYAxisField.Text = child.InnerText.Replace("&lt;", "<");
                        //    break;
                        //case "xaxisscattervariable":
                        //    cbxScatterXAxisField.Text = child.InnerText.Replace("&lt;", "<");
                        //    break;
                        case "allvalues":
                            if (child.InnerText.ToLower().Equals("true")) 
                            { 
                                ((ColumnChartParameters)Parameters).ShowAllListValues = true;
                            }
                            else { ((ColumnChartParameters)Parameters).ShowAllListValues = false; }
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
                            if (!isStacked)
                            {
                                //cmbField.Text = child.InnerText.Replace("&lt;", "<");
                                if (this.Parameters.ColumnNames.Count > 0)
                                {
                                    ((ColumnChartParameters)Parameters).ColumnNames[0] = (child.InnerText.Replace("&lt;", "<"));
                                }
                                else
                                {
                                    ((ColumnChartParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                                }
                            }
                            break;
                        case "weightvariable":
                            if (!string.IsNullOrEmpty(child.InnerText.Trim()))
                            {
                                //cmbFieldWeight.Text = child.InnerText.Replace("&lt;", "<");
                                ((ColumnChartParameters)Parameters).WeightVariableName = child.InnerText.Replace("&lt;", "<");
                            }
                            break;
                        case "stratavariable":
                            if (!isStacked) ((ColumnChartParameters)Parameters).StrataVariableNames[0] = child.InnerText.Replace("&lt;", "<");
                                //listboxFieldStrata.SelectedItems.Add(child.InnerText.Replace("&lt;", "<"));
                            break;
                        case "columnaggregatefunction":
                            if (child.InnerText == "Count")
                            {
                                //cmbComposition.SelectedIndex = 1;
                                ((ColumnChartParameters)Parameters).Composition = CompositionKind.Stacked;
                            }
                            else
                            {
                                ((ColumnChartParameters)Parameters).Composition = CompositionKind.Stacked100;
                            }
                            break;
                        case "charttype":
                            if (child.InnerText == "Stacked Column")
                            {
                                isStacked = true;
                            }
                            else if (child.InnerText == "Bar")
                            {
                                //cmbOrientation.SelectedIndex = 1;
                                ((ColumnChartParameters)Parameters).Orientation = Orientation.Horizontal;
                            }
                            //cbxChartType.Text = child.InnerText;
                            break;
                        case "customheading":
                            //this.CustomOutputHeading = child.InnerText;
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
                                case "Small":
                                    ((ColumnChartParameters)Parameters).ChartWidth = 400;
                                    ((ColumnChartParameters)Parameters).ChartHeight = 250;
                                    break;
                                case "Medium":
                                    ((ColumnChartParameters)Parameters).ChartWidth = 533;
                                    ((ColumnChartParameters)Parameters).ChartHeight = 333;
                                    break;
                                case "Large":
                                    ((ColumnChartParameters)Parameters).ChartWidth = 800;
                                    ((ColumnChartParameters)Parameters).ChartHeight = 500;
                                    break;
                                case "Custom":
                                    break;
                            }
                            break;
                        case "chartwidth":
                            if (!string.IsNullOrEmpty(child.InnerText))
                            {
                                ((ColumnChartParameters)Parameters).ChartWidth = double.Parse(child.InnerText.Replace("&lt;", "<"));
                            }
                            break;
                        case "chartheight":
                            if (!string.IsNullOrEmpty(child.InnerText))
                            {
                                ((ColumnChartParameters)Parameters).ChartHeight = double.Parse(child.InnerText.Replace("&lt;", "<"));
                            }
                            break;
                        case "charttitle":
                            //txtChartTitle.Text = child.InnerText;
                            ((ColumnChartParameters)Parameters).ChartTitle = child.InnerText.Replace("&lt;", "<");
                            break;
                        case "chartlegendtitle":
                            //LegendTitle = child.InnerText;
                            break;
                        case "xaxislabel":
                            //txtXAxisLabelValue.Text = child.InnerText;
                            //cmbXAxisLabelType.SelectedIndex = 3;
                            ((ColumnChartParameters)Parameters).XAxisLabel = child.InnerText.Replace("&lt;", "<");
                            break;
                        case "yaxislabel":
                            //txtYAxisLabelValue.Text = child.InnerText;
                            ((ColumnChartParameters)Parameters).YAxisLabel = child.InnerText.Replace("&lt;", "<");
                            break;
                        //case "xaxisstartvalue":
                        //    txtXAxisStartValue.Text = child.InnerText;
                        //    break;
                        //case "xaxisendvalue":
                        //    txtXAxisEndValue.Text = child.InnerText;
                        //    break;
                        case "xaxisrotation":
                            switch (child.InnerText)
                            {
                                case "0":
                                    //txtXAxisAngle.Text = "0";
                                    ((ColumnChartParameters)Parameters).XAxisAngle = 0;
                                    break;
                                case "45":
                                    ((ColumnChartParameters)Parameters).XAxisAngle = -45;
                                    break;
                                case "90":
                                    ((ColumnChartParameters)Parameters).XAxisAngle = -90;
                                    break;
                                default:
                                    ((ColumnChartParameters)Parameters).XAxisAngle = 0;
                                    break;
                            }
                            break;                    
                    }
                }
            }

            public override void CreateFromXml(XmlElement element)
            {
                this.LoadingCombos = true;
                this.Parameters = new ColumnChartParameters();
            
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
                        if (!String.IsNullOrEmpty(child.InnerText))
                        {
                            switch (child.Name.ToLower())
                            {
                                case "mainvariable":
                                    if (this.Parameters.ColumnNames.Count > 0)
                                    {
                                        ((ColumnChartParameters)Parameters).ColumnNames[0] = (child.InnerText.Replace("&lt;", "<"));
                                    }
                                    else
                                    {
                                        ((ColumnChartParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                                    }
                                    break;
                                case "stratavariable":
                                    if (!string.IsNullOrEmpty(child.InnerText))
                                    {
                                        if (((ColumnChartParameters)Parameters).StrataVariableNames.Count > 0)
                                        {
                                            ((ColumnChartParameters)Parameters).StrataVariableNames[0] = child.InnerText.Replace("&lt;", "<");
                                        }
                                        else
                                        {
                                            ((ColumnChartParameters)Parameters).StrataVariableNames.Add(child.InnerText.Replace("&lt;", "<"));
                                        }
                                    }
                                    break;
                                case "stratavariables":
                                    foreach (XmlElement field in child.ChildNodes)
                                    {
                                        List<string> fields = new List<string>();
                                        if (field.Name.ToLower().Equals("stratavariable"))
                                        {
                                            ((ColumnChartParameters)Parameters).StrataVariableNames.Add(field.InnerText.Replace("&lt;", "<"));
                                        }
                                    }
                                    break;
                                case "weightvariable":
                                    ((ColumnChartParameters)Parameters).WeightVariableName = child.InnerText.Replace("&lt;", "<");
                                    break;
                                case "crosstabvariable":
                                    ((ColumnChartParameters)Parameters).CrosstabVariableName = child.InnerText.Replace("&lt;", "<");
                                    break;
                                case "secondyvar":
                                    if (this.Parameters.ColumnNames.Count > 1)
                                    {
                                        ((ColumnChartParameters)Parameters).ColumnNames[1] = (child.InnerText.Replace("&lt;", "<"));
                                    }
                                    else
                                    {
                                        ((ColumnChartParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                                    }
                                    break;
                                case "secondyvartype":
                                    Y2Type y2Type = ((Y2Type)Int32.Parse(child.InnerText));
                                    ((ColumnChartParameters)Parameters).Y2AxisType = int.Parse(child.InnerText.Replace("&lt;", "<"));
                                    break;
                                case "sort":
                                    if (child.InnerText.ToLower().Equals("highlow") || child.InnerText.ToLower().Equals("hightolow"))
                                    {
                                        ((ColumnChartParameters)Parameters).SortHighToLow = true;
                                    }
                                    break;
                                case "allvalues":
                                    if (child.InnerText.ToLower().Equals("true"))
                                    {
                                        ((ColumnChartParameters)Parameters).ShowAllListValues = true;
                                    }
                                    else { ((ColumnChartParameters)Parameters).ShowAllListValues = false; }
                                    break;
                                case "showlistlabels":
                                    if (child.InnerText.ToLower().Equals("true"))
                                    {
                                        ((ColumnChartParameters)Parameters).ShowCommentLegalLabels = true;
                                    }
                                    else { ((ColumnChartParameters)Parameters).ShowCommentLegalLabels = false; }
                                    break;
                                case "includemissing":
                                    if (child.InnerText.ToLower().Equals("true")) { ((ColumnChartParameters)Parameters).IncludeMissing = true; }
                                    else { ((ColumnChartParameters)Parameters).IncludeMissing = false; }
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
                                    if (child.InnerText.ToLower().Equals("true")) { ((ColumnChartParameters)Parameters).UseDiffColors = true; }
                                    else { ((ColumnChartParameters)Parameters).UseDiffColors = false; }
                                    break;
                                case "userefvalues":
                                    if (child.InnerText.ToLower().Equals("true")) { ((ColumnChartParameters)Parameters).UseRefValues = true; }
                                    else { ((ColumnChartParameters)Parameters).UseRefValues = false; }
                                    break;
                                case "showannotations":
                                    if (child.InnerText.ToLower().Equals("true")) { ((ColumnChartParameters)Parameters).ShowAnnotations = true; }
                                    else { ((ColumnChartParameters)Parameters).ShowAnnotations = false; }
                                    break;
                                case "y2showannotations":
                                    if (child.InnerText.ToLower().Equals("true")) { ((ColumnChartParameters)Parameters).Y2ShowAnnotations = true; }
                                    else { ((ColumnChartParameters)Parameters).Y2ShowAnnotations = false; }
                                    break;
                                case "showgridlines":
                                    if (child.InnerText.ToLower().Equals("true")) { ((ColumnChartParameters)Parameters).ShowGridLines = true; }
                                    else { ((ColumnChartParameters)Parameters).ShowGridLines = false; }
                                    break;
                                case "composition":
                                    {
                                        switch (child.InnerText)
                                        {
                                            case "0":
                                                ((ColumnChartParameters)Parameters).Composition = CompositionKind.SideBySide;
                                                break;
                                            case "1":
                                                ((ColumnChartParameters)Parameters).Composition = CompositionKind.Stacked;
                                                break;
                                            case "2":
                                                ((ColumnChartParameters)Parameters).Composition = CompositionKind.Stacked100;
                                                break;
                                        }
                                    }
                                    break;
                                case "barspace":
                                    {
                                        switch (child.InnerText)
                                        {
                                            case "0":
                                                ((ColumnChartParameters)Parameters).BarSpace = BarSpacing.Default;
                                                break;
                                            case "1":
                                                ((ColumnChartParameters)Parameters).BarSpace = BarSpacing.None;
                                                break;
                                            case "2":
                                                ((ColumnChartParameters)Parameters).BarSpace = BarSpacing.Small;
                                                break;
                                            case "3":
                                                ((ColumnChartParameters)Parameters).BarSpace = BarSpacing.Medium;
                                                break;
                                            case "4":
                                                ((ColumnChartParameters)Parameters).BarSpace = BarSpacing.Large;
                                                break;
                                        }
                                    }
                                    break;
                                case "orientation":
                                    if (child.InnerText == "0")
                                    {
                                        ((ColumnChartParameters)Parameters).Orientation = Orientation.Vertical;
                                    }
                                    else
                                    {
                                        ((ColumnChartParameters)Parameters).Orientation = Orientation.Horizontal;
                                    }
                                    break;
                                case "palette":
                                    ((ColumnChartParameters)Parameters).Palette = int.Parse(child.InnerText);
                                    break;
                                case "bartype":
                                    switch (child.InnerText.ToString())
                                    {
                                        default:
                                        case "0":
                                            ((ColumnChartParameters)Parameters).BarKind = BarKind.Block;
                                            break;
                                        case "1":
                                            ((ColumnChartParameters)Parameters).BarKind = BarKind.Cylinder;
                                            break;
                                        case "2":
                                            ((ColumnChartParameters)Parameters).BarKind = BarKind.Rectangle;
                                            break;
                                        case "3":
                                            ((ColumnChartParameters)Parameters).BarKind = BarKind.RoundedBlock;
                                            break;
                                    }
                                    break;
                                case "legenddock":
                                    {
                                        switch (int.Parse(child.InnerText))
                                        {
                                            case 0:
                                                ((ColumnChartParameters)Parameters).LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Left;
                                                break;
                                            default:
                                            case 1:
                                                ((ColumnChartParameters)Parameters).LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Right;
                                                break;
                                            case 2:
                                                ((ColumnChartParameters)Parameters).LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Top;
                                                break;
                                            case 3:
                                                ((ColumnChartParameters)Parameters).LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Bottom;
                                                break;
                                        }
                                    }
                                    break;
                                case "y2linetype":
                                    switch (child.InnerText)
                                    {
                                        case "Polygon":
                                            ((ColumnChartParameters)Parameters).Y2LineKind = LineKind.Polygon;
                                            break;
                                        case "Smooth":
                                            ((ColumnChartParameters)Parameters).Y2LineKind = LineKind.Smooth;
                                            break;
                                        case "Step":
                                            ((ColumnChartParameters)Parameters).Y2LineKind = LineKind.Step;
                                            break;
                                        case "Auto":
                                            ((ColumnChartParameters)Parameters).Y2LineKind = LineKind.Auto;
                                            break;
                                    }
                                    break;
                                case "y2linedashstyle":
                                    {
                                        switch (child.InnerText)
                                        {
                                            case "Solid":
                                                ((ColumnChartParameters)Parameters).Y2LineDashStyle = LineDashStyle.Solid;
                                                break;
                                            case "Dash":
                                                ((ColumnChartParameters)Parameters).Y2LineDashStyle = LineDashStyle.Dash;
                                                break;
                                            case "Dot":
                                                ((ColumnChartParameters)Parameters).Y2LineDashStyle = LineDashStyle.Dot;
                                                break;
                                            case "DashDot":
                                                ((ColumnChartParameters)Parameters).Y2LineDashStyle = LineDashStyle.DashDot;
                                                break;
                                            case "DashDotDot":
                                                ((ColumnChartParameters)Parameters).Y2LineDashStyle = LineDashStyle.DashDotDot;
                                                break;
                                        }
                                        break;
                                    }
                                case "y2linethickness":
                                    ((ColumnChartParameters)Parameters).Y2LineThickness = int.Parse(child.InnerText);
                                    break;
                                case "yaxislabel":
                                    ((ColumnChartParameters)Parameters).YAxisLabel = child.InnerText;
                                    break;
                                case "yaxisformatstring":
                                    ((ColumnChartParameters)Parameters).YAxisFormat = child.InnerText;
                                    break;
                                case "y2axisformatstring":
                                    ((ColumnChartParameters)Parameters).Y2AxisFormat = child.InnerText;
                                    break;
                                case "y2axislabel":
                                    ((ColumnChartParameters)Parameters).Y2AxisLabel = child.InnerText;
                                    break;
                                case "y2axislegendtitle":
                                    ((ColumnChartParameters)Parameters).Y2AxisLegendTitle = child.InnerText;
                                    break;
                                case "xaxislabeltype":
                                    {
                                        //switch (child.InnerText)
                                        //{
                                        //    case "Automatic":
                                        //        ((ColumnChartParameters)Parameters).XAxisLabelType = EpiDashboard.XAxisLabelType.Automatic;
                                        //        break;
                                        //    case "FieldPrompt":
                                        //        ((ColumnChartParameters)Parameters).XAxisLabelType = EpiDashboard.XAxisLabelType.FieldPrompt;
                                        //        break;
                                        //    case "None":
                                        //        ((ColumnChartParameters)Parameters).XAxisLabelType = EpiDashboard.XAxisLabelType.None;
                                        //        break;
                                        //    case "Custom":
                                        //        ((ColumnChartParameters)Parameters).XAxisLabelType = EpiDashboard.XAxisLabelType.Custom;
                                        //        break;
                                        //}
                                        ((ColumnChartParameters)Parameters).XAxisLabelType = int.Parse(child.InnerText);
                                        break;
                                    }
                                case "xaxislabel":
                                    ((ColumnChartParameters)Parameters).XAxisLabel = child.InnerText;
                                    break;
                                case "xaxisangle":
                                    ((ColumnChartParameters)Parameters).XAxisAngle = int.Parse(child.InnerText);
                                    break;
                                case "charttitle":
                                    ((ColumnChartParameters)Parameters).ChartTitle = child.InnerText;
                                    break;
                                case "chartsubtitle":
                                    ((ColumnChartParameters)Parameters).ChartSubTitle = child.InnerText;
                                    break;
                                case "showlegend":
                                    if (child.InnerText.ToLower().Equals("true")) { ((ColumnChartParameters)Parameters).ShowLegend = true; }
                                    else { ((ColumnChartParameters)Parameters).ShowLegend = false; }
                                    break;
                                case "showlegendborder":
                                    if (child.InnerText.ToLower().Equals("true")) { ((ColumnChartParameters)Parameters).ShowLegendBorder = true; }
                                    else { ((ColumnChartParameters)Parameters).ShowLegendBorder = false; }
                                    break;
                                case "showlegendvarnames":
                                    if (child.InnerText.ToLower().Equals("true")) { ((ColumnChartParameters)Parameters).ShowLegendVarNames = true; }
                                    else { ((ColumnChartParameters)Parameters).ShowLegendVarNames = false; }
                                    break;
                                case "legendfontsize":
                                    ((ColumnChartParameters)Parameters).LegendFontSize = int.Parse(child.InnerText);
                                    break;
                                case "height":
                                    ((ColumnChartParameters)Parameters).ChartHeight = double.Parse(child.InnerText);
                                    break;
                                case "width":
                                    ((ColumnChartParameters)Parameters).ChartWidth = double.Parse(child.InnerText);
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
                //CreateInputVariableList();
                ColumnChartParameters chtParameters = (ColumnChartParameters)Parameters;

                System.Xml.XmlElement element = doc.CreateElement("columnChartGadget");
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
                type.Value = "EpiDashboard.Gadgets.Charting.ColumnChartGadget";

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
                switch(chtParameters.Composition.ToString())
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
                ColumnChartParameters chtParameters = (ColumnChartParameters)Parameters;
                StringBuilder sb = new StringBuilder();
                CustomOutputHeading = headerPanel.Text;
                CustomOutputDescription = descriptionPanel.Text;

                //sb.AppendLine("<h2>" + this.txtChartTitle.Text + "</h2>");
                //sb.AppendLine("<h3>" + this.txtChartSubTitle.Text + "</h3>");

                sb.AppendLine("<h2>" + chtParameters.ChartTitle + "</h2>");
                sb.AppendLine("<h3>" + chtParameters.ChartSubTitle + "</h3>");

                foreach (UIElement element in panelMain.Children)
                {
                    if (element is EpiDashboard.Controls.Charting.ColumnChart)
                    {
                        sb.AppendLine(((EpiDashboard.Controls.Charting.ColumnChart)element).ToHTML(htmlFileName, count, true, false));
                    }
                }

                return sb.ToString();
            }

            public override void ShowHideConfigPanel()
            {
                Popup = new DashboardPopup();
                Popup.Parent = ((this.Parent as DragCanvas).Parent as ScrollViewer).Parent as Grid;
                Controls.GadgetProperties.ColumnChartProperties properties = new Controls.GadgetProperties.ColumnChartProperties(this.DashboardHelper, this, (ColumnChartParameters)Parameters, StrataGridList);

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
                    ColumnChartParameters chtParameters = (ColumnChartParameters)Parameters;

                    //string freqVar = GadgetOptions.MainVariableName;
                    //string weightVar = GadgetOptions.WeightVariableName;
                    //string crosstabVar = GadgetOptions.CrosstabVariableName;
                    string freqVar = chtParameters.ColumnNames[0];
                    string weightVar = chtParameters.WeightVariableName;
                    string crosstabVar = chtParameters.CrosstabVariableName;
                    bool includeMissing = chtParameters.IncludeMissing;
                    List<string> stratas = new List<string>();

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
                                ColumnChartParameters parameters = new ColumnChartParameters(chtParameters);
                                
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
                                GenerateColumnChartData(stratifiedFrequencyTables, strata);
                                System.Threading.Thread.Sleep(100);
                            }
                        }
                        else
                        {
                            Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables = DashboardHelper.GenerateFrequencyTable(chtParameters);
                            GenerateColumnChartData(stratifiedFrequencyTables);
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

        #endregion //Event Handlers

        #region Private Properties
            /// <summary>
            /// Gets whether or not the main variable is a drop-down list
            /// </summary>
            private bool IsDropDownList { get; set; }
        
            /// <summary>
            /// Gets whether the main variable is a comment legal field
            /// </summary>
            private bool IsCommentLegal { get; set; }

            /// <summary>
            /// Gets whether the main variable is a recoded variable
            /// </summary>
            private bool IsRecoded { get; set; }

            /// <summary>
            /// Gets whether the main variable is an option field
            /// </summary>
            private bool IsOptionField { get; set; }

        #endregion //Private Properties
    }
}
