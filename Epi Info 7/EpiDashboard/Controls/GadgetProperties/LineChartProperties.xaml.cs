using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Epi;
using Epi.Fields;
using EpiDashboard;
using EpiDashboard.Rules;
using EpiDashboard.Gadgets.Charting;
using ComponentArt.Win.DataVisualization.Charting;
using EpiDashboard.Gadgets;
using System.Collections;
namespace EpiDashboard.Controls.GadgetProperties
{
    /// <summary>
    /// Interaction logic for LineChartProperties.xaml
    /// </summary>
    public partial class LineChartProperties : GadgetPropertiesPanelBase
    {
        private byte _opacity = 240;

        public byte Opacity
        {
            get { return _opacity; }
            set { _opacity = value; }
        }

        public LineChartProperties(
            DashboardHelper dashboardHelper,
            IGadget gadget,
            LineChartParameters parameters,
            List<Grid> strataGridList
            )
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            this.Gadget = gadget;
            this.Parameters = parameters;
            this.StrataGridList = strataGridList;

            List<string> fields = new List<string>();
            List<string> weightFields = new List<string>();
            List<string> strataItems = new List<string>();

            //Variable fields
            fields.Add(String.Empty);
            ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.DateTime | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.UserDefined;
            foreach (string fieldName in DashboardHelper.GetFieldsAsList(columnDataType))
            {
                if (DashboardHelper.IsUsingEpiProject)
                {
                    if (!(fieldName == "RecStatus")) fields.Add(fieldName);
                }
                else
                {
                    fields.Add(fieldName);
                }
            }
            cmbField.ItemsSource = fields;

            //Weight Fields
            weightFields.Add(String.Empty);
            columnDataType = ColumnDataType.Numeric | ColumnDataType.UserDefined;
            foreach (string fieldName in DashboardHelper.GetFieldsAsList(columnDataType))
            {
                if (DashboardHelper.IsUsingEpiProject)
                {
                    if (!(fieldName == "RecStatus")) weightFields.Add(fieldName);
                }
                else
                {
                    weightFields.Add(fieldName);
                }
            }
            weightFields.Sort();
            cmbFieldWeight.ItemsSource = weightFields;
            cmbSecondYAxisVariable.ItemsSource = weightFields;

            //Strata Fields 
            strataItems.Add(String.Empty);
            columnDataType = ColumnDataType.Numeric | ColumnDataType.Boolean | ColumnDataType.Text | ColumnDataType.UserDefined;
            foreach (string fieldName in DashboardHelper.GetFieldsAsList(columnDataType))
            {
                if (DashboardHelper.IsUsingEpiProject)
                {
                    if (!(fieldName == "RecStatus" || fieldName == "FKEY" || fieldName == "GlobalRecordId")) strataItems.Add(fieldName);
                }
                else
                {
                    strataItems.Add(fieldName);
                }
            }
            listboxFieldStrata.ItemsSource = strataItems;
            cmbFieldCrosstab.ItemsSource = strataItems;

            txtYAxisLabelValue.Text = "Count";
            txtXAxisLabelValue.Text = String.Empty;
            cmbLegendDock.SelectedIndex = 1;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(cmbField.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("VariableCategory");
            view.GroupDescriptions.Add(groupDescription);

            RowFilterControl = new RowFilterControl(this.DashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, (gadget as LineChartGadget).DataFilters, true);
            RowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            panelFilters.Children.Add(RowFilterControl);

            //EI-98
            txtXAxisFontSize.Text = parameters.XAxisFontSize.ToString();
            txtYAxisFontSize.Text = parameters.YAxisFontSize.ToString();

            txtXAxisLabelFontSize.Text = parameters.XAxisLabelFontSize.ToString();
            txtYAxisLabelFontSize.Text = parameters.YAxisLabelFontSize.ToString();

            txtWidth.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            txtHeight.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            txtLegendFontSize.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);

            #region Translation

            lblConfigExpandedTitleTxt.Text = DashboardSharedStrings.GADGET_CONFIG_TITLE_LINECHART;
            tbtnVariables.Title = DashboardSharedStrings.GADGET_TABBUTTON_VARIABLES;
            tbtnVariables.Description = DashboardSharedStrings.GADGET_TABDESC_LINE_CHART;
            tbtnSorting.Title = DashboardSharedStrings.GADGET_TABBUTTON_SORTING;
            tbtnSorting.Description = DashboardSharedStrings.GADGET_TABDESC_SORTING;
            tbtnDisplay.Title = DashboardSharedStrings.GADGET_TABBUTTON_DISPLAY;
            tbtnDisplay.Description = DashboardSharedStrings.GADGET_TABDESC_DISPLAY;
            tbtnDisplayColors.Title = DashboardSharedStrings.GADGET_TAB_COLORS_STYLES;
            tbtnDisplayColors.Description = DashboardSharedStrings.GADGET_TABDESC_COLORS_STYLES;
            tbtnDisplayLabels.Title = DashboardSharedStrings.GADGET_TABBUTTON_LABELS;
            tbtnDisplayLabels.Description = DashboardSharedStrings.GADGET_TABDESC_LABELS;
            tbtnDisplayLegend.Title = DashboardSharedStrings.GADGET_TABBUTTON_LEGEND;
            tbtnDisplayLegend.Description = DashboardSharedStrings.GADGET_TABDESC_LEGEND;
            tbtnFilters.Title = DashboardSharedStrings.GADGET_TABBUTTON_FILTERS;
            tbtnFilters.Description = DashboardSharedStrings.GADGET_TABDESC_FILTERS;
            tblockPanelVariablesTxt.Text = DashboardSharedStrings.GADGET_PANELHEADER_VARIABLES;
            tblockMainVariable.Text = DashboardSharedStrings.GADGET_MAIN_VARIABLE;
            tblockCrossTabVariable.Text = DashboardSharedStrings.GADGET_ONE_GRAPH_VALUE;
            tblockWeightVariable.Text = DashboardSharedStrings.GADGET_WEIGHT_VARIABLE;
            tblockPanelSortingTxt.Text = DashboardSharedStrings.GADGET_PANELHEADER_SORTING;
            tblockGroupingSubheaderTxt.Text = DashboardSharedStrings.GADGET_PANELSUBHEADER_GROUPING;
            tblockSortingSubheaderTxt.Text = DashboardSharedStrings.GADGET_PANELSUBHEADER_SORTING;
            tblockStrataVariableTxt.Text = DashboardSharedStrings.GADGET_STRATA_VARIABLE;
            tblockSortMethod.Text = DashboardSharedStrings.GADGET_SORT_METHOD;
            checkboxSortHighLowTxt.Text = DashboardSharedStrings.GADGET_SORT_HI_LOW;
            tblockPanelDisplayTxt.Text = DashboardSharedStrings.GADGET_PANELHEADER_DISPLAY;
            tblockTitleNDescSubheaderTxt.Text = DashboardSharedStrings.GADGET_PANELSUBHEADER_TITLENDESC;
            tblockTitleTxt.Text = DashboardSharedStrings.GADGET_GADET_TITLE;
            tblockDescTxt.Text = DashboardSharedStrings.GADGET_DESCRIPTION;
            tblockDimensionsTxt.Text = DashboardSharedStrings.GADGET_DIMENSIONS;
            tblockWidth.Text = DashboardSharedStrings.GADGET_WIDTH;
            tblockHeight.Text = DashboardSharedStrings.GADGET_HEIGHT;
            tblockPanelOutputOptTxt.Text = DashboardSharedStrings.GADGET_OUTPUT_OPTIONS;
            checkboxAllValuesTxt.Text = DashboardSharedStrings.GADGET_DISPLAY_LIST_VALUE;
            checkboxCommentLegalLabelsTxt.Text = DashboardSharedStrings.GADGET_DISPLAY_LIST_LABELS;
            checkboxIncludeMissingTxt.Text = DashboardSharedStrings.GADGET_INCLUDE_MISSING;
            checkboxAnnotationsTxt.Text = DashboardSharedStrings.GADGET_SHOW_ANNOTATIONS;
            checkboxAnnotationsY2Txt.Text = DashboardSharedStrings.GADGET_SHOW_ANNOTATIONSY2;
            tblockPanelColorsNStylesTxt.Text = DashboardSharedStrings.GADGET_PANEL_COLORS_STYLES;
            tblockColorsSubheaderTxt.Text = DashboardSharedStrings.GADGET_PANELSUBHEADER_COLORS;
            tblockPalette.Text = DashboardSharedStrings.GADGET_COLOR_PALETTE;
            tblockStylesSubheaderTxt.Text = DashboardSharedStrings.GADGET_PANELSUBHEADER_STYLES;
            checkboxUseRefValuesTxt.Text = DashboardSharedStrings.GADGET_USE_REFERENCE_VALUE;
            checkboxGridLinesTxt.Text = DashboardSharedStrings.GADGET_SHOW_GRID_LINES;
            tblockLineTypeY2.Text = DashboardSharedStrings.GADGET_LINETYPE_Y2;
            tblockLineDashTypeY2.Text = DashboardSharedStrings.GADGET_LINEDASH_STYLEY2;
            tblockLineThicknessY2.Text = DashboardSharedStrings.GADGET_LINE_THICKNESSY2;
            tblockPanelLabelsTxt.Text = DashboardSharedStrings.GADGET_PANELSHEADER_LABELS;
            tblockYAxisSubheaderTxt.Text = DashboardSharedStrings.GADGET_PANELSUBHEADER_YAXIS;
            tblockYAxisLabelValue.Text = DashboardSharedStrings.GADGET_YAXIS_LABEL;
            tblockYAxisFormatString.Text = DashboardSharedStrings.GADGET_YAXIS_FORMAT;
            tblockYAxisLabelFontSize.Text = DashboardSharedStrings.GADGET_YAXIS_LABEL_FONTSIZE;
            tblockYAxisFontSize.Text = DashboardSharedStrings.GADGET_YAXIS_DATA_FONTSIZE;
            tblockSecondYAxis.Text = DashboardSharedStrings.GADGET_SECOND_YAXIS_TYPE;
            tblockSecondYAxisVariable.Text = DashboardSharedStrings.GADGET_SECOND_YAXIS_VARIABLE;
            tblockY2AxisLabelValue.Text = DashboardSharedStrings.GADGET_Y2_AXISLABEL;
            tblockY2AxisLegendTitle.Text = DashboardSharedStrings.GADGET_Y2_AXISLEGEND_TITLE;
            tblockY2AxisFormatString.Text = DashboardSharedStrings.GADGET_Y2_AXISFORMAT;
            tblockXAxisSubheaderTxt.Text = DashboardSharedStrings.GADGET_XAXIS;
            tblockXAxisLabelType.Text = DashboardSharedStrings.GADGET_XAXIS_LABEL_TYPE;
            tblockXAxisLabelValue.Text = DashboardSharedStrings.GADGET_XAXIS_LABEL;
            tblockXAxisAngle.Text = DashboardSharedStrings.GADGET_XAXIS_ANGLE;
            tblockXAxisLabelFontSize.Text = DashboardSharedStrings.GADGET_XAXIS_LABEL_FONTSIZE;
            tblockXAxisFontSize.Text = DashboardSharedStrings.GADGET_XAXIS_DATA_FONTSIZE;
            tblockTitleSubTitleTxt.Text = DashboardSharedStrings.GADGET_SUBHEADER_TITLESUBTITLE;
            tblockChartTitleValue.Text = DashboardSharedStrings.GADGET_CHART_TITLE;
            tblockChartSubTitleValue.Text = DashboardSharedStrings.GADGET_CHART_SUBTITLE;
            tblockPanelLegendTxt.Text = DashboardSharedStrings.GADGET_PANEL_LEGEND;
            checkboxShowLegendTxt.Text = DashboardSharedStrings.GADGET_SHOW_LEGEND;
            checkboxShowLegendBorderTxt.Text = DashboardSharedStrings.GADGET_SHOW_LEGEND_BORDER;
            checkboxShowVarNameTxt.Text = DashboardSharedStrings.GADGET_SHOW_VARIABLE_NAME;
            tblockLegendFontSize.Text = DashboardSharedStrings.GADGET_LEGEND_FONTSIZE;
            tblockLegendDock.Text = DashboardSharedStrings.GADGET_LEGEND_PLACEMENT;
            tblockPanelDataFilterTxt.Text = DashboardSharedStrings.GADGET_PANELHEADER_DATA_FILTER;
            tblockAnyFilterGadgetOnlyTxt.Text = DashboardSharedStrings.GADGET_FILTER_GADGET_ONLY;
            tblockLineType.Text = DashboardSharedStrings.GADGET_LINE_TYPE;
            tblockLineThickness.Text = DashboardSharedStrings.GADGET_LINE_THICKNESS;
            btnOKTxt.Text = DashboardSharedStrings.BUTTON_OK;
            btnCancelTxt.Text = DashboardSharedStrings.BUTTON_CANCEL;
            tblockY2AxisSubheaderTxt.Text = DashboardSharedStrings.GADGETS_YAXIS_HEADER;
            #endregion // Translation

        }


        protected override bool ValidateInput()
        {
            bool isValid = true;

            if (cmbField.SelectedIndex == -1)
            {
                isValid = false;
                MessageBox.Show(DashboardSharedStrings.PROPERTIES_MAIN_VARIABLE_REQ);
                return isValid;
            }
            else if ((cmbSecondYAxis.SelectedIndex == 1 || cmbSecondYAxis.SelectedIndex == 2) && (cmbSecondYAxisVariable.SelectedIndex == -1 || cmbSecondYAxisVariable.SelectedItem.ToString() == String.Empty))
            {
                isValid = false;
                MessageBox.Show(DashboardSharedStrings.PROPERTIES_Y2_AXIS_VARIABLE_REQ);
                return isValid;
            }
            //EI-98
            ValidateFontSize(txtLegendFontSize, DashboardSharedStrings.PROPERTIES_LEGEND_FONT_SIZE_INVALID, out isValid);
            ValidateFontSize(txtYAxisFontSize, DashboardSharedStrings.PROPERTIES_YAXIS_FONT_SIZE_INVALID, out isValid);
            ValidateFontSize(txtXAxisFontSize, DashboardSharedStrings.PROPERTIES_XAXIS_FONT_SIZE_INVALID, out isValid);


            return isValid;
        }

        //EI-98
        private void ValidateFontSize(TextBox txtFontSize, string errorMessage, out bool isValid)
        {
            if (String.IsNullOrEmpty(txtFontSize.Text))
            {
                txtYAxisFontSize.Text = "12";
            }
            else
            {
                double thisSize = 0;
                double.TryParse(txtYAxisFontSize.Text, out thisSize);
                if (thisSize < 5 || thisSize > 100)
                {
                    isValid = false;
                    MessageBox.Show(errorMessage);
                    return;
                }
            }
            isValid = true;
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
        public bool HasSelectedFields
        {
            get
            {
                if (cmbField.SelectedIndex > -1)
                {
                    return true;
                }
                return false;
            }
        }

        public LineChartParameters Parameters { get; private set; }
        private List<Grid> StrataGridList { get; set; }

        /// <summary>
        /// Used to generate the list of variables and options for the GadgetParameters object
        /// </summary> 
        protected override void CreateInputVariableList()
        {
            // Set data filters!
            this.DataFilters = RowFilterControl.DataFilters;

            //Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

            if (cmbField.SelectedIndex > -1 && !string.IsNullOrEmpty(cmbField.SelectedItem.ToString()))
            {
                if (Parameters.ColumnNames.Count > 0)
                {
                    Parameters.ColumnNames[0] = cmbField.SelectedItem.ToString();
                }
                else
                {
                    Parameters.ColumnNames.Add(cmbField.SelectedItem.ToString());
                }
            }
            else
            {
                return;
            }

            if (cmbFieldWeight.SelectedIndex > -1 && !string.IsNullOrEmpty(cmbFieldWeight.SelectedItem.ToString()))
            {
                Parameters.WeightVariableName = cmbFieldWeight.SelectedItem.ToString();
            }
            else
            {
                Parameters.WeightVariableName = String.Empty;
            }

            //if (cmbFieldCrosstab.SelectedIndex > -1 && !string.IsNullOrEmpty(cmbFieldCrosstab.SelectedItem.ToString()))
            //{
            //    inputVariableList.Add("crosstabvar", cmbFieldCrosstab.SelectedItem.ToString());
            //    GadgetOptions.CrosstabVariableName = cmbFieldCrosstab.SelectedItem.ToString();
            //}

            if (cmbFieldCrosstab.SelectedIndex > -1 && !string.IsNullOrEmpty(cmbFieldCrosstab.SelectedItem.ToString()))
            {
                Parameters.CrosstabVariableName = cmbFieldCrosstab.SelectedItem.ToString();
            }
            else
            {
                Parameters.CrosstabVariableName = String.Empty;
            }

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

            Parameters.Y2AxisType = cmbSecondYAxis.SelectedIndex;

            if (cmbSecondYAxisVariable.SelectedIndex > -1 && !string.IsNullOrEmpty(cmbSecondYAxisVariable.SelectedItem.ToString()))
            {
                //inputVariableList.Add("second_y_var", cmbSecondYAxisVariable.SelectedItem.ToString());
                if (Parameters.ColumnNames.Count > 1)
                {
                    Parameters.ColumnNames[1] = cmbSecondYAxisVariable.SelectedItem.ToString();
                }
                else
                {
                    Parameters.ColumnNames.Add(cmbSecondYAxisVariable.SelectedItem.ToString());
                }
            }
            else
            {
                if (Parameters.ColumnNames.Count > 1)
                {
                    Parameters.ColumnNames[1] = String.Empty;
                }
                ValidateInput();
            }


            //Sorting and Grouping settings //////////////////////////////////

            if (listboxFieldStrata.SelectedItems.Count > 0)
            {
                Parameters.StrataVariableNames = new List<string>();
                foreach (string s in listboxFieldStrata.SelectedItems)
                {
                    Parameters.StrataVariableNames.Add(s.ToString());
                }
            }

            Parameters.SortHighToLow = (bool)checkboxSortHighLow.IsChecked;


            //Display settings ///////////////////////////////////////////

            Parameters.GadgetTitle = txtTitle.Text;
            Parameters.GadgetDescription = txtDesc.Text;
            double height = 0;
            double width = 0;
            bool success = double.TryParse(txtWidth.Text, out width);
            if (success)
            {
                Parameters.ChartWidth = width;
            }
            success = double.TryParse(txtHeight.Text, out height);
            if (success)
            {
                Parameters.ChartHeight = height;
            }
            Parameters.ShowAllListValues = (bool)checkboxAllValues.IsChecked;
            Parameters.ShowCommentLegalLabels = (bool)checkboxCommentLegalLabels.IsChecked;
            Parameters.IncludeMissing = (bool)checkboxIncludeMissing.IsChecked;

            //Colors and Styles settings /////////////////////////////////
            Parameters.UseRefValues = (bool)checkboxUseRefValues.IsChecked;
            Parameters.ShowAnnotations = (bool)checkboxAnnotations.IsChecked;
            Parameters.Y2ShowAnnotations = (bool)checkboxAnnotationsY2.IsChecked;
            Parameters.ShowGridLines = (bool)checkboxGridLines.IsChecked;

            if (cmbPalette.SelectedIndex >= 0)
            {
                Parameters.Palette = cmbPalette.SelectedIndex;
                Parameters.PaletteColors = GetPaletteColores();
            }

            switch (cmbLineTypeY2.SelectedIndex)
            {
                case 1:
                    Parameters.Y2LineKind = LineKind.Polygon;
                    break;
                case 2:
                    Parameters.Y2LineKind = LineKind.Smooth;
                    break;
                case 3:
                    Parameters.Y2LineKind = LineKind.Step;
                    break;
                default:
                case 0:
                    Parameters.Y2LineKind = LineKind.Auto;
                    break;
            }

            switch (cmbLineType.SelectedIndex)
            {
                case 1:
                    Parameters.LineKind = LineKind.Polygon;
                    break;
                case 2:
                    Parameters.LineKind = LineKind.Smooth;
                    break;
                case 3:
                    Parameters.LineKind = LineKind.Step;
                    break;
                default:
                case 0:
                    Parameters.LineKind = LineKind.Auto;
                    break;
            }

            if (cmbLineDashTypeY2.SelectedIndex >= 0)
            {
                switch (cmbLineDashTypeY2.SelectedIndex)
                {
                    case 0:
                        Parameters.Y2LineDashStyle = LineDashStyle.Dash;
                        break;
                    case 1:
                        Parameters.Y2LineDashStyle = LineDashStyle.DashDot;
                        break;
                    case 2:
                        Parameters.Y2LineDashStyle = LineDashStyle.DashDotDot;
                        break;
                    case 3:
                        Parameters.Y2LineDashStyle = LineDashStyle.Dot;
                        break;
                    case 4:
                        Parameters.Y2LineDashStyle = LineDashStyle.Solid;
                        break;
                }
            }
            

            if (cmbLineThicknessY2.SelectedIndex >= 0)
            {
                Parameters.Y2LineThickness = cmbLineThicknessY2.SelectedIndex + 1;
            }

            if (cmbLineThickness.SelectedIndex >= 0)
            {
                Parameters.LineThickness = cmbLineThickness.SelectedIndex + 1;
            }

            //Labels settings /////////////////////////////////
            if (!String.IsNullOrEmpty(txtYAxisLabelValue.Text))
            {
                Parameters.YAxisLabel = txtYAxisLabelValue.Text;
            }

            if (!String.IsNullOrEmpty(txtYAxisFormatString.Text))
            {
                Parameters.YAxisFormat = txtYAxisFormatString.Text;
            }

            if (!String.IsNullOrEmpty(txtY2AxisLabelValue.Text))
            {
                Parameters.Y2AxisLabel = txtY2AxisLabelValue.Text;
            }

            if (!String.IsNullOrEmpty(txtY2AxisLegendTitle.Text))
            {
                Parameters.Y2AxisLegendTitle = txtY2AxisLegendTitle.Text;
            }

            if (!String.IsNullOrEmpty(txtY2AxisFormatString.Text))
            {
                Parameters.Y2AxisFormat = txtY2AxisFormatString.Text;
            }

            if (cmbXAxisLabelType.SelectedIndex >= 0)
            {
                //switch (cmbXAxisLabelType.SelectedIndex)
                //{
                //    default:
                //    case 0:
                //        Parameters.XAxisLabelType = XAxisLabelType.Automatic;
                //        break;
                //    case 1:
                //        Parameters.XAxisLabelType = XAxisLabelType.FieldPrompt;
                //        break;
                //    case 2:
                //        Parameters.XAxisLabelType = XAxisLabelType.None;
                //        break;
                //    case 3:
                //        Parameters.XAxisLabelType = XAxisLabelType.Custom;
                //        break;
                //}
                Parameters.XAxisLabelType = cmbXAxisLabelType.SelectedIndex;
            }

            if (!String.IsNullOrEmpty(txtXAxisLabelValue.Text))
            {
                Parameters.XAxisLabel = txtXAxisLabelValue.Text;
            }

            if (!String.IsNullOrEmpty(txtXAxisAngle.Text))
            {
                Parameters.XAxisAngle = int.Parse(txtXAxisAngle.Text);
            }

            Parameters.ChartTitle = txtChartTitle.Text;           
            Parameters.ChartSubTitle = txtChartSubTitle.Text;
           // Parameters.ChartTitle = txtTitle.Text;
          //  Parameters.ChartSubTitle = txtDesc.Text;

            //Legend settings /////////////////////////////////

            Parameters.ShowLegend = (bool)checkboxShowLegend.IsChecked;
            Parameters.ShowLegendBorder = (bool)checkboxShowLegendBorder.IsChecked;
            Parameters.ShowLegendVarNames = (bool)checkboxShowVarName.IsChecked;

            if (!String.IsNullOrEmpty(txtLegendFontSize.Text))
            {
                Parameters.LegendFontSize = double.Parse(txtLegendFontSize.Text);
            }

            if (cmbLegendDock.SelectedIndex >= 0)
            {
                switch (cmbLegendDock.SelectedIndex)
                {
                    case 0:
                        Parameters.LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Left;
                        break;
                    case 1:
                        Parameters.LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Right;
                        break;
                    case 2:
                        Parameters.LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Top;
                        break;
                    case 3:
                        Parameters.LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Bottom;
                        break;
                }
            }

            //GadgetOptions.ShouldIncludeFullSummaryStatistics = false;
            Parameters.IncludeFullSummaryStatistics = false;
            //GadgetOptions.InputVariableList = inputVariableList;

            //EI-98
            if (!String.IsNullOrEmpty(txtXAxisFontSize.Text))
            {
                Parameters.XAxisFontSize = double.Parse(txtXAxisFontSize.Text);
            }

            if (!String.IsNullOrEmpty(txtXAxisLabelFontSize.Text))
            {
                Parameters.XAxisLabelFontSize = double.Parse(txtXAxisLabelFontSize.Text);
            }
            if (!String.IsNullOrEmpty(txtYAxisFontSize.Text))
            {
                Parameters.YAxisFontSize = double.Parse(txtYAxisFontSize.Text);
            }
            if (!String.IsNullOrEmpty(txtYAxisLabelFontSize.Text))
            {
                Parameters.YAxisLabelFontSize = double.Parse(txtYAxisLabelFontSize.Text);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Variables settings
            //Potentially two columns for Line Chart, 
            //.ColumnNames[0] = Main Variable
            //.ColumnNames[1] = Second Y-axis Variable

            if (Parameters.ColumnNames.Count > 0)
            {
                cmbField.SelectedItem = Parameters.ColumnNames[0];
            }

            cmbFieldWeight.SelectedItem = Parameters.WeightVariableName;
            cmbFieldCrosstab.SelectedItem = Parameters.CrosstabVariableName;

            switch (Parameters.Y2AxisType)
            //switch (Parameters.Y2LineType)
            {
                case 1:
                    cmbSecondYAxis.SelectedIndex = 1;
                    break;
                case 2:
                    cmbSecondYAxis.SelectedIndex = 2;
                    if (Parameters.ColumnNames.Count > 1 && !String.IsNullOrEmpty(Parameters.ColumnNames[1]))
                    {
                        cmbSecondYAxisVariable.SelectedItem = Parameters.ColumnNames[1];
                    }
                    break;
                case 3:
                    cmbSecondYAxis.SelectedIndex = 3;
                    if (Parameters.ColumnNames.Count > 1 && !String.IsNullOrEmpty(Parameters.ColumnNames[1]))
                    {
                        cmbSecondYAxisVariable.SelectedItem = Parameters.ColumnNames[1];
                    }
                    break;
                default:
                case 0:
                    cmbSecondYAxis.SelectedIndex = 0;
                    break;
            }
            EnableDisableY2Fields();

            //Sorting and Grouping settings
            if (Parameters.StrataVariableNames.Count > 0)
            {
                foreach (string s in Parameters.StrataVariableNames)
                {
                    //listboxFieldStrata.SelectedItem = s;
                    listboxFieldStrata.SelectedItems.Add(s.ToString());
                }
            }
            checkboxSortHighLow.IsChecked = Parameters.SortHighToLow;

            //Display settings
            scrollViewerProperties.MaxHeight = scrollViewerProperties.MaxHeight + (System.Windows.SystemParameters.PrimaryScreenHeight - 768.0);
            txtTitle.Text = Parameters.GadgetTitle;
            txtDesc.Text = Parameters.GadgetDescription;
            txtChartTitle.Text = Parameters.ChartTitle;
            txtChartSubTitle.Text = Parameters.ChartSubTitle;
            txtWidth.Text = Parameters.ChartWidth.ToString();
            txtHeight.Text = Parameters.ChartHeight.ToString();

            checkboxAllValues.IsChecked = Parameters.ShowAllListValues;
            checkboxCommentLegalLabels.IsChecked = Parameters.ShowCommentLegalLabels;
            checkboxIncludeMissing.IsChecked = Parameters.IncludeMissing;

            //Display Colors and Styles settings
            scrollViewerPropertiesColors.Height = scrollViewerPropertiesColors.Height + (System.Windows.SystemParameters.PrimaryScreenHeight - 768.0);
            checkboxUseRefValues.IsChecked = Parameters.UseRefValues;
            checkboxAnnotations.IsChecked = Parameters.ShowAnnotations;
            checkboxAnnotationsY2.IsChecked = Parameters.Y2ShowAnnotations;
            checkboxGridLines.IsChecked = Parameters.ShowGridLines;
            cmbPalette.SelectedIndex = Parameters.Palette;
            if (Parameters.PaletteColors.Count() > 0)
            {
                FillColorSquare(null, 0, Parameters.PaletteColors);
            }
            else
            {
                FillColorSquare(null, cmbPalette.SelectedIndex, null);
            }
            switch (Parameters.LineKind)
            {
                default:
                case LineKind.Auto:
                    cmbLineType.SelectedIndex = 0;
                    break;
                case LineKind.Polygon:
                    cmbLineType.SelectedIndex = 1;
                    break;
                case LineKind.Smooth:
                    cmbLineType.SelectedIndex = 2;
                    break;
                case LineKind.Step:
                    cmbLineType.SelectedIndex = 3;
                    break;
            }

            switch (Parameters.Y2LineKind)
            {
                default:
                case LineKind.Auto:
                    cmbLineTypeY2.SelectedIndex = 0;
                    break;
                case LineKind.Polygon:
                    cmbLineTypeY2.SelectedIndex = 1;
                    break;
                case LineKind.Smooth:
                    cmbLineTypeY2.SelectedIndex = 2;
                    break;
                case LineKind.Step:
                    cmbLineTypeY2.SelectedIndex = 3;
                    break;
            }
            switch (Parameters.Y2LineDashStyle)
            {
                case LineDashStyle.Dash:
                    cmbLineDashTypeY2.SelectedIndex = 0;
                    break;
                case LineDashStyle.DashDot:
                    cmbLineDashTypeY2.SelectedIndex = 1;
                    break;
                case LineDashStyle.DashDotDot:
                    cmbLineDashTypeY2.SelectedIndex = 2;
                    break;
                case LineDashStyle.Dot:
                    cmbLineDashTypeY2.SelectedIndex = 3;
                    break;
                case LineDashStyle.Solid:
                    cmbLineDashTypeY2.SelectedIndex = 4;
                    break;
            }

            cmbLineThickness.SelectedIndex = (int)Parameters.LineThickness - 1;
            cmbLineThicknessY2.SelectedIndex = (int)Parameters.Y2LineThickness - 1;

            //Display Labels settings
            scrollViewerPropertiesLabels.MaxHeight = scrollViewerPropertiesLabels.MaxHeight + (System.Windows.SystemParameters.PrimaryScreenHeight - 768.0);

            txtYAxisLabelValue.Text = Parameters.YAxisLabel;
            txtYAxisFormatString.Text = Parameters.YAxisFormat;
            txtY2AxisLabelValue.Text = Parameters.Y2AxisLabel;
            txtY2AxisLegendTitle.Text = Parameters.Y2AxisLegendTitle;
            txtY2AxisFormatString.Text = Parameters.Y2AxisFormat;
            cmbXAxisLabelType.SelectedIndex = Parameters.XAxisLabelType;
            txtXAxisLabelValue.Text = Parameters.XAxisLabel;
            txtXAxisAngle.Text = Parameters.XAxisAngle.ToString();

            //txtChartTitle.Text = Parameters.ChartTitle;
            //txtChartSubTitle.Text = Parameters.ChartSubTitle;
            Parameters.ChartTitle = txtTitle.Text;
            Parameters.ChartSubTitle = txtDesc.Text;

            //Display Legend settings
            checkboxShowLegend.IsChecked = Parameters.ShowLegend;
            checkboxShowLegendBorder.IsChecked = Parameters.ShowLegendBorder;
            checkboxShowVarName.IsChecked = Parameters.ShowLegendVarNames;
            EnableDisableLegendOptions();
            txtLegendFontSize.Text = Parameters.LegendFontSize.ToString();
            switch (Parameters.LegendDock.ToString())
            {
                case "Left":
                    cmbLegendDock.SelectedIndex = 0;
                    break;
                default:
                case "Right":
                    cmbLegendDock.SelectedIndex = 1;
                    break;
                case "Top":
                    cmbLegendDock.SelectedIndex = 2;
                    break;
                case "Bottom":
                    cmbLegendDock.SelectedIndex = 3;
                    break;
            }
            CheckVariables();
        }

        public class FieldInfo { public string Name { get; set; } public string DataType { get; set; } public VariableCategory VariableCategory { get; set; } }

        ///// <summary>
        ///// Fired when the user changes a column selection
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void lbxColumns_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    ShowHideOutputColumns();
        //}

        /// <summary>
        /// Checks the selected variables and enables/disables checkboxes as appropriate
        /// </summary>
        private void CheckVariables()
        {
            bool isDropDownList = false;
            bool isCommentLegal = false;
            bool isOptionField = false;
            bool isRecoded = false;

            if (cmbField.SelectedItem != null && !string.IsNullOrEmpty(cmbField.SelectedItem.ToString()))
            {
                foreach (DataRow fieldRow in DashboardHelper.FieldTable.Rows)
                {
                    if (fieldRow["columnname"].Equals(cmbField.SelectedItem.ToString()))
                    {
                        if (fieldRow["epifieldtype"] is TableBasedDropDownField || fieldRow["epifieldtype"] is YesNoField || fieldRow["epifieldtype"] is CheckBoxField)
                        {
                            isDropDownList = true;
                            if (fieldRow["epifieldtype"] is DDLFieldOfCommentLegal)
                            {
                                isCommentLegal = true;
                            }
                        }
                        else if (fieldRow["epifieldtype"] is OptionField)
                        {
                            isOptionField = true;
                        }
                        break;
                    }
                }

                if (DashboardHelper.IsUserDefinedColumn(cmbField.SelectedItem.ToString()))
                {
                    List<IDashboardRule> associatedRules = DashboardHelper.Rules.GetRules(cmbField.SelectedItem.ToString());
                    foreach (IDashboardRule rule in associatedRules)
                    {
                        if (rule is Rule_Recode)
                        {
                            isRecoded = true;
                        }
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

            if (isCommentLegal || isOptionField)
            {
                checkboxCommentLegalLabels.IsEnabled = true;
            }
            else
            {
                checkboxCommentLegalLabels.IsEnabled = false;
            }

            if (!isCommentLegal && !isOptionField)
            {
                checkboxCommentLegalLabels.IsChecked = isCommentLegal;
            }
        }


        ///// <summary>
        ///// Enables and disables output columns based on user selection.
        ///// </summary>
        //private void ShowHideOutputColumns()
        //{
        //    if (this.StrataGridList != null && this.StrataGridList.Count > 0)
        //    {
        //        List<int> columnsToHide = new List<int>();

        //        if (!(bool)checkboxColumnFrequency.IsChecked) columnsToHide.Add(1);
        //        if (!(bool)checkboxColumnPercent.IsChecked) columnsToHide.Add(2);
        //        if (!(bool)checkboxColumnCumulativePercent.IsChecked) columnsToHide.Add(3);
        //        if (!(bool)checkboxColumn95CILower.IsChecked) columnsToHide.Add(4);
        //        if (!(bool)checkboxColumn95CIUpper.IsChecked) columnsToHide.Add(5);
        //        if (!(bool)checkboxColumnPercentBars.IsChecked) columnsToHide.Add(6);

        //        foreach (Grid grid in this.StrataGridList)
        //        {
        //            for (int i = 0; i < grid.ColumnDefinitions.Count; i++)
        //            {
        //                if (columnsToHide.Contains(i))
        //                {
        //                    grid.ColumnDefinitions[i].Width = new GridLength(0);
        //                }
        //                else
        //                {
        //                    if (i == 6)
        //                    {
        //                        //grid.ColumnDefinitions[i].Width = GridLength.Auto;
        //                        int width = 100;
        //                        if (int.TryParse(txtBarWidth.Text, out width))
        //                        {
        //                            grid.ColumnDefinitions[i].Width = new GridLength(width);
        //                        }
        //                        else
        //                        {
        //                            grid.ColumnDefinitions[i].Width = new GridLength(100);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        grid.ColumnDefinitions[i].Width = new GridLength(1, GridUnitType.Auto);
        //                    }
        //                }
        //            }
        //        }
        //    }
        // }

        private void tbtnVariables_Checked(object sender, RoutedEventArgs e)
        {
            if (panelVariables == null) return;

            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Visible;
            panelSorting.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplayColors.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplayLabels.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplayLegend.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnSorting_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelSorting.Visibility = System.Windows.Visibility.Visible;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplayColors.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplayLabels.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplayLegend.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnDisplay_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelSorting.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Visible;
            panelDisplayColors.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplayLabels.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplayLegend.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnDisplayColors_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelSorting.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplayColors.Visibility = System.Windows.Visibility.Visible;
            panelDisplayLabels.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplayLegend.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnDisplayLabels_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelSorting.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplayColors.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplayLabels.Visibility = System.Windows.Visibility.Visible;
            panelDisplayLegend.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnDisplayLegend_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelSorting.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplayColors.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplayLabels.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplayLegend.Visibility = System.Windows.Visibility.Visible;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnFilters_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelSorting.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplayColors.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplayLabels.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplayLegend.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Visible;
        }

        private void listboxFieldStrata_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool clearLbx = false;
            if (listboxFieldStrata.SelectedItems.Count == 0)
            {
                clearLbx = true;
            }
            else
            {
                foreach (string s in listboxFieldStrata.SelectedItems)
                {
                    if (s == String.Empty)
                    {
                        clearLbx = true;
                    }
                }
            }
            if (clearLbx)
            {
                listboxFieldStrata.SelectedItems.Clear();
                Parameters.StrataVariableNames.Clear();
            }
        }

        private void cmbFieldWeight_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (cmbFieldWeight.SelectedValue == String.Empty)
            //{
            //    cmbFieldWeight.Items.Clear();
            //}
        }

        //private void cmbField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    CheckVariables();
        //}

        protected virtual void cmbField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox)
            {
                ComboBox cmbField = sender as ComboBox;

                CheckBox checkboxAllValues = null;
                CheckBox checkboxCommentLegalLabels = null;

                object element = this.FindName("checkboxAllValues");
                if (element != null && element is CheckBox)
                {
                    checkboxAllValues = element as CheckBox;
                }

                element = this.FindName("checkboxCommentLegalLabels");
                if (element != null && element is CheckBox)
                {
                    checkboxCommentLegalLabels = element as CheckBox;
                }

                if (cmbField.SelectedIndex >= 0)
                {
                    Field field = DashboardHelper.GetAssociatedField(cmbField.SelectedItem.ToString());
                    //Disable Include Missing check box for Number, Date, time fields.
                    if (field != null &&
                        (field is NumberField ||
                        field is DateField ||
                        field is TimeField ||
                        field is DateTimeField ||
                        field is TimeField))
                    {
                        checkboxIncludeMissing.IsChecked = false;
                        checkboxIncludeMissing.IsEnabled = false;
                    }
                    else
                    {
                        checkboxIncludeMissing.IsEnabled = true;
                    }
                    if (field != null && field is RenderableField)
                    {
                        FieldFlags flags = SetFieldFlags(field as RenderableField);

                        if (checkboxAllValues != null)
                        {
                            if (flags.IsDropDownListField || flags.IsRecodedField)
                            {
                                checkboxAllValues.IsEnabled = true;
                            }
                            else
                            {
                                checkboxAllValues.IsEnabled = false;
                                checkboxAllValues.IsChecked = false;
                            }
                        }

                        if (checkboxCommentLegalLabels != null)
                        {
                            if (flags.IsCommentLegalField || flags.IsOptionField)
                            {
                                checkboxCommentLegalLabels.IsEnabled = true;
                            }
                            else
                            {
                                checkboxCommentLegalLabels.IsEnabled = false;
                                checkboxCommentLegalLabels.IsChecked = false;
                            }

                            if (!flags.IsCommentLegalField && !flags.IsOptionField)
                            {
                                checkboxCommentLegalLabels.IsChecked = flags.IsCommentLegalField;
                            }
                        }
                    }
                }
            }
        }

        protected virtual FieldFlags SetFieldFlags(Epi.Fields.RenderableField field)
        {
            FieldFlags flags = new FieldFlags(false, false, false, false);

            if (field is TableBasedDropDownField || field is YesNoField || field is CheckBoxField)
            {
                flags.IsDropDownListField = true;
                if (field is DDLFieldOfCommentLegal)
                {
                    flags.IsCommentLegalField = true;
                }
            }
            else if (field is OptionField)
            {
                flags.IsOptionField = true;
            }

            return flags;
        }

        private void EnableDisableY2Fields()
        {
            if (cmbSecondYAxis.SelectedIndex == 0)  //Second Y-axis type = None
            {
                tblockSecondYAxisVariable.Text = DashboardSharedStrings.GADGET_SECOND_YAXIS_VARIABLE;
                //tblockSecondYAxisVariable.Visibility = System.Windows.Visibility.Collapsed;
                //cmbSecondYAxisVariable.Visibility = System.Windows.Visibility.Collapsed;
                cmbSecondYAxisVariable.IsEnabled = false;
                cmbSecondYAxisVariable.SelectedIndex = -1;
                checkboxAnnotationsY2.IsEnabled = false;
                checkboxAnnotationsY2.IsChecked = false;
                cmbLineDashTypeY2.IsEnabled = false;
                cmbLineThicknessY2.IsEnabled = false;
                cmbLineTypeY2.IsEnabled = false;
                txtY2AxisLabelValue.IsEnabled = false;
                txtY2AxisLegendTitle.IsEnabled = false;
                txtY2AxisFormatString.IsEnabled = false;
                //if (Parameters.ColumnNames.Count > 1) Parameters.ColumnNames[1] = String.Empty;
            }
            else if (cmbSecondYAxis.SelectedIndex == 3)  //Second Y-axis type = Cumulative percent
            {
                tblockSecondYAxisVariable.Text = "Second y-axis variable:";
               // tblockSecondYAxisVariable.Visibility = System.Windows.Visibility.Collapsed;
                //cmbSecondYAxisVariable.Visibility = System.Windows.Visibility.Collapsed;
                cmbSecondYAxisVariable.IsEnabled = false;
                cmbSecondYAxisVariable.SelectedIndex = -1;
                checkboxAnnotationsY2.IsEnabled = true;
                cmbLineDashTypeY2.IsEnabled = true;
                cmbLineThicknessY2.IsEnabled = true;
                cmbLineTypeY2.IsEnabled = true;
                txtY2AxisLabelValue.IsEnabled = true;
                txtY2AxisLegendTitle.IsEnabled = true;
                txtY2AxisFormatString.IsEnabled = true;
                //if (Parameters.ColumnNames.Count > 1) Parameters.ColumnNames[1] = String.Empty;
            }
            else  //Second Y-axis type = Single field or Rate per 100k population
            {
                //tblockSecondYAxisVariable.Visibility = System.Windows.Visibility.Visible;
                //cmbSecondYAxisVariable.Visibility = System.Windows.Visibility.Visible;
                cmbSecondYAxisVariable.IsEnabled = true;
                checkboxAnnotationsY2.IsEnabled = true;
                cmbLineDashTypeY2.IsEnabled = true;
                cmbLineThicknessY2.IsEnabled = true;
                cmbLineTypeY2.IsEnabled = true;
                txtY2AxisLabelValue.IsEnabled = true;
                txtY2AxisLegendTitle.IsEnabled = true;
                txtY2AxisFormatString.IsEnabled = true;

                if (cmbSecondYAxis.SelectedIndex == 1)  //Second y-axis = "Single field"
                {
                    tblockSecondYAxisVariable.Text = "Second y-axis variable:";
                    if (Parameters.ColumnNames.Count > 1 && (!String.IsNullOrEmpty(Parameters.ColumnNames[1])))
                    {
                        cmbSecondYAxisVariable.SelectedItem = Parameters.ColumnNames[1];
                    }

                }
                else if (cmbSecondYAxis.SelectedIndex == 2)  //Second y-axis = "Rate per 100k population"
                {
                    tblockSecondYAxisVariable.Text = "Population variable:";
                    if (Parameters.ColumnNames.Count > 1)
                    {
                        cmbSecondYAxisVariable.SelectedItem = Parameters.ColumnNames[1];
                    }

                }
            }
        }

        private void cmbSecondYAxis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tblockSecondYAxisVariable == null || cmbSecondYAxisVariable == null) return;

            EnableDisableY2Fields();
        }

        private void cmbSecondYAxisVariable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (cmbSecondYAxisVariable == null) return; 

            //if (!String.IsNullOrEmpty(cmbSecondYAxisVariable.SelectedItem.ToString()))
            //{
            //    if (Parameters.ColumnNames.Count > 1)
            //    {
            //        Parameters.ColumnNames[1] = cmbSecondYAxisVariable.SelectedItem.ToString();
            //    }
            //    else
            //    {
            //        Parameters.ColumnNames.Add(cmbSecondYAxisVariable.SelectedItem.ToString());
            //    }
            //}
        }

        private void EnableDisableLegendOptions()
        {
            if (checkboxShowLegend.IsChecked == true)
            {
                checkboxShowLegendBorder.IsEnabled = true;
                checkboxShowVarName.IsEnabled = true;
                txtLegendFontSize.IsEnabled = true;
                cmbLegendDock.IsEnabled = true;
            }
            else
            {
                checkboxShowLegendBorder.IsEnabled = false;
                checkboxShowVarName.IsEnabled = false;
                txtLegendFontSize.IsEnabled = false;
                cmbLegendDock.IsEnabled = false;
            }
        }

        private void checkboxShowLegend_Click(object sender, RoutedEventArgs e)
        {
            EnableDisableLegendOptions();
        }

        private void txtWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            double thisWidth = 0;
            double.TryParse(txtWidth.Text, out thisWidth);
            if (thisWidth > System.Windows.SystemParameters.PrimaryScreenWidth * 2)
            {
                txtWidth.Text = (System.Windows.SystemParameters.PrimaryScreenWidth * 2).ToString();
            }
        }

        private void txtHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            double thisHeight = 0;
            double.TryParse(txtHeight.Text, out thisHeight);
            if (thisHeight > System.Windows.SystemParameters.PrimaryScreenHeight * 2)
            {
                txtHeight.Text = (System.Windows.SystemParameters.PrimaryScreenHeight * 2).ToString();
            }
        }

        private void cmbXAxisLabelType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (txtXAxisLabelValue == null) return;
            if (cmbXAxisLabelType.SelectedIndex == 3)
            {
                txtXAxisLabelValue.IsEnabled = true;
            }
            else
            {
                if (cmbXAxisLabelType.SelectedIndex == 1)
                {
                    Field f = DashboardHelper.GetAssociatedField(cmbField.Text);
                    if (f != null && f is IDataField)
                    {
                        Epi.Fields.IDataField dataField = f as IDataField;
                        txtXAxisLabelValue.Text = dataField.PromptText;
                    }
                    else
                    {
                        txtXAxisLabelValue.Text = String.Empty;
                    }
                }
                else
                {
                    txtXAxisLabelValue.Text = String.Empty;
                }
                txtXAxisLabelValue.IsEnabled = false;
            }
        }

        private void txtLegendFontSize_SelectionChanged(object sender, RoutedEventArgs e)
        {
            double thisSize = 0;
            double.TryParse(txtLegendFontSize.Text, out thisSize);
            if (thisSize > 100)
            {
                MessageBox.Show(DashboardSharedStrings.PROPERTIES_LEGEND_FONT_SIZE_INVALID);
                txtLegendFontSize.Text = "12";
            }
        }
        private List<string> GetPaletteColores()
        {
            List<string> PaletteColors = new List<string>();
            PaletteColors.Add(Color1.Fill.ToString());
            PaletteColors.Add(Color2.Fill.ToString());
            PaletteColors.Add(Color3.Fill.ToString());
            PaletteColors.Add(Color4.Fill.ToString());
            PaletteColors.Add(Color5.Fill.ToString());
            PaletteColors.Add(Color6.Fill.ToString());
            PaletteColors.Add(Color7.Fill.ToString());
            PaletteColors.Add(Color8.Fill.ToString());
            PaletteColors.Add(Color9.Fill.ToString());
            PaletteColors.Add(Color10.Fill.ToString());
            PaletteColors.Add(Color11.Fill.ToString());
            PaletteColors.Add(Color12.Fill.ToString());
            return PaletteColors;
        }
        private void rctColor1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color1.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));

            }


        }
        private void rctColor2_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color2.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));

            }


        }
        private void rctColor3_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color3.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));

            }


        }
        private void rctColor4_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color4.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));

            }

        }
        private void rctColor5_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color5.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));

            }


        }
        private void rctColor6_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color6.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));

            }


        }
        private void rctColor7_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color7.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));

            }


        }
        private void rctColor8_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color8.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));

            }


        }
        private void rctColor9_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color9.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));

            }


        }
        private void rctColor10_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color10.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));

            }


        }
        private void rctColor11_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color11.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));

            }


        }
        private void rctColor12_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color12.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));

            }


        }


        private static Brush ConvertToBrush(string Color)
        {
            var converter = new System.Windows.Media.BrushConverter();
            var brush = (Brush)converter.ConvertFromString(Color);
            return brush;
        }



        private void cmbPalette_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FillColorSquare(sender);
        }

        private void FillColorSquare(object sender = null, int selectedIndex = 0, List<string> PaletteColors = null)
        {
            XYChart xyChart = new XYChart();

            if (PaletteColors == null)
            {
                int Index = 0;
                if (sender != null)
                {
                    Index = ((System.Windows.Controls.Primitives.Selector)(sender)).SelectedIndex;
                }
                else
                {
                    Index = selectedIndex;
                }

                switch (Index)
                {
                    case 0:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Atlantic");
                        break;
                    case 1:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Breeze");
                        break;
                    case 2:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("ComponentArt");
                        break;
                    case 3:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Deep");
                        break;
                    case 4:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Earth");
                        break;
                    case 5:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Evergreen");
                        break;
                    case 6:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Heatwave");
                        break;
                    case 7:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Montreal");
                        break;
                    case 8:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Pastel");
                        break;
                    case 9:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Renaissance");
                        break;
                    case 10:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("SharePoint");
                        break;
                    case 11:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("Study");
                        break;
                    default:
                    case 12:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("VibrantA");
                        break;
                    case 13:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("VibrantB");
                        break;
                    case 14:
                        xyChart.Palette = ComponentArt.Win.DataVisualization.Palette.GetPalette("VibrantC");
                        break;
                }
            }
            try
            {
                IList Colorcollection;
                if (PaletteColors == null)
                {
                    Colorcollection = (IList)xyChart.Palette.ChartingDataPoints12;
                }
                else
                {
                    Colorcollection = (IList)PaletteColors;

                }
                if (Color1 != null && Color12 != null)
                {
                    this.Color1.Fill = ConvertToBrush(Colorcollection[0].ToString());
                    this.Color2.Fill = ConvertToBrush(Colorcollection[1].ToString());
                    this.Color3.Fill = ConvertToBrush(Colorcollection[2].ToString());
                    this.Color4.Fill = ConvertToBrush(Colorcollection[3].ToString());
                    this.Color5.Fill = ConvertToBrush(Colorcollection[4].ToString());
                    this.Color6.Fill = ConvertToBrush(Colorcollection[5].ToString());
                    this.Color7.Fill = ConvertToBrush(Colorcollection[6].ToString());
                    this.Color8.Fill = ConvertToBrush(Colorcollection[7].ToString());
                    this.Color9.Fill = ConvertToBrush(Colorcollection[8].ToString());
                    this.Color10.Fill = ConvertToBrush(Colorcollection[9].ToString());
                    this.Color11.Fill = ConvertToBrush(Colorcollection[10].ToString());
                    this.Color12.Fill = ConvertToBrush(Colorcollection[11].ToString());
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

      
    }
}
