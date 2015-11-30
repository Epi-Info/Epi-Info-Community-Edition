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
using System.Collections;
namespace EpiDashboard.Controls.GadgetProperties
{
    /// <summary>
    /// Interaction logic for ParetoChartProperties.xaml
    /// </summary>
    public partial class ParetoChartProperties : GadgetPropertiesPanelBase
    {

        private byte _opacity = 240;

        public byte Opacity
        {
            get { return _opacity; }
            set { _opacity = value; }
        }
        public ParetoChartProperties(
            DashboardHelper dashboardHelper, 
            IGadget gadget, 
            ParetoChartParameters parameters, 
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

            cmbBarSpacing.SelectedIndex = 0;
            txtYAxisLabelValue.Text = "Count";
            txtXAxisLabelValue.Text = String.Empty;
            cmbLegendDock.SelectedIndex = 1;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(cmbField.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("VariableCategory");
            view.GroupDescriptions.Add(groupDescription);

            RowFilterControl = new RowFilterControl(this.DashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, (gadget as ParetoChartGadget).DataFilters, true);
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
            txtXAxisAngle.PreviewKeyDown += new KeyEventHandler(txtInput_IntegerOnly_PreviewKeyDown);

            #region Translation

            lblConfigExpandedTitle.Content = DashboardSharedStrings.GADGET_CONFIG_TITLE_PARETO_CHART;
            tbtnVariables.Title = DashboardSharedStrings.GADGET_TABBUTTON_VARIABLES;
            tbtnVariables.Description = DashboardSharedStrings.GADGET_TABDESC_PARETO_CHART;
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
            tblockPanelVariables.Content = DashboardSharedStrings.GADGET_PANELHEADER_VARIABLES;
            tblockMainVariable.Text = DashboardSharedStrings.GADGET_MAIN_VARIABLE;
            tblockWeightVariable.Text = DashboardSharedStrings.GADGET_WEIGHT_VARIABLE;
            tblockPanelDisplay.Content = DashboardSharedStrings.GADGET_PANELHEADER_DISPLAY;
            tblockTitleNDescSubheader.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_TITLENDESC;
            tblockTitle.Content = DashboardSharedStrings.GADGET_GADET_TITLE;
            tblockDesc.Content = DashboardSharedStrings.GADGET_DESCRIPTION;
            tblockDimensions.Content = DashboardSharedStrings.GADGET_DIMENSIONS;
            tblockWidth.Text = DashboardSharedStrings.GADGET_WIDTH;
            tblockHeight.Text = DashboardSharedStrings.GADGET_HEIGHT;
            tblockPanelOutputOpt.Content = DashboardSharedStrings.GADGET_OUTPUT_OPTIONS;
            checkboxCommentLegalLabels.Content = DashboardSharedStrings.GADGET_DISPLAY_LIST_LABELS;
            checkboxIncludeMissing.Content = DashboardSharedStrings.GADGET_INCLUDE_MISSING;
            tblockPanelColorsNStyles.Content = DashboardSharedStrings.GADGET_PANEL_COLORS_STYLES;
            tblockPalette.Text = DashboardSharedStrings.GADGET_COLOR_PALETTE;
            tblockStylesSubheader.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_STYLES;
            checkboxUseRefValues.Content = DashboardSharedStrings.GADGET_USE_REFERENCE_VALUE;
            checkboxAnnotations.Content = DashboardSharedStrings.GADGET_SHOW_ANNOTATIONS;
            checkboxGridLines.Content = DashboardSharedStrings.GADGET_SHOW_GRID_LINES;
            tblockBarSpacing.Text = DashboardSharedStrings.GADGET_SPACE_BETWEEN_BARS;
            tblockBarType.Text = DashboardSharedStrings.GADGET_BARTYPE;
            tblockYAxisSubheader.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_YAXIS;
            tblockYAxisLabelValue.Text = DashboardSharedStrings.GADGET_YAXIS_LABEL;
            tblockYAxisLabelFontSize.Text = DashboardSharedStrings.GADGET_YAXIS_LABEL_FONTSIZE;
            tblockYAxisFontSize.Text = DashboardSharedStrings.GADGET_YAXIS_DATA_FONTSIZE;
            tblockXAxisSubheader.Content = DashboardSharedStrings.GADGET_XAXIS;
            tblockXAxisLabelType.Text = DashboardSharedStrings.GADGET_XAXIS_LABEL_TYPE;
            tblockXAxisLabelValue.Text = DashboardSharedStrings.GADGET_XAXIS_LABEL;
            tblockXAxisAngle.Text = DashboardSharedStrings.GADGET_XAXIS_ANGLE;
            tblockXAxisLabelFontSize.Text = DashboardSharedStrings.GADGET_XAXIS_LABEL_FONTSIZE;
            tblockXAxisFontSize.Text = DashboardSharedStrings.GADGET_XAXIS_DATA_FONTSIZE;
            tblockTitleSubTitle.Content = DashboardSharedStrings.GADGET_SUBHEADER_TITLESUBTITLE;
            tblockChartTitleValue.Text = DashboardSharedStrings.GADGET_CHART_TITLE;
            tblockChartSubTitleValue.Text = DashboardSharedStrings.GADGET_CHART_SUBTITLE;
            tblockPanelLegend.Content = DashboardSharedStrings.GADGET_PANEL_LEGEND;
            checkboxShowLegend.Content = DashboardSharedStrings.GADGET_SHOW_LEGEND;
            checkboxShowLegendBorder.Content = DashboardSharedStrings.GADGET_SHOW_LEGEND_BORDER;
            checkboxShowVarName.Content = DashboardSharedStrings.GADGET_SHOW_VARIABLE_NAME;
            tblockLegendFontSize.Text = DashboardSharedStrings.GADGET_LEGEND_FONTSIZE;
            tblockLegendDock.Text = DashboardSharedStrings.GADGET_LEGEND_PLACEMENT;
            tblockPanelDataFilter.Content = DashboardSharedStrings.GADGET_PANELHEADER_DATA_FILTER;
            tblockAnyFilterGadgetOnly.Content = DashboardSharedStrings.GADGET_FILTER_GADGET_ONLY;
            tblockColorsSubheader.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_COLORS;
            tblockPanelLabels.Content = DashboardSharedStrings.GADGET_PANELSHEADER_LABELS;
            btnOK.Content = DashboardSharedStrings.BUTTON_OK;
            btnCancel.Content = DashboardSharedStrings.BUTTON_CANCEL;

























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

        public ParetoChartParameters Parameters { get; private set; }
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
            //Parameters.ShowAllListValues = (bool)checkboxAllValues.IsChecked;
            Parameters.ShowCommentLegalLabels = (bool)checkboxCommentLegalLabels.IsChecked;
            Parameters.IncludeMissing = (bool)checkboxIncludeMissing.IsChecked;

            //Colors and Styles settings /////////////////////////////////
            //Parameters.UseDiffColors = (bool)checkboxUseDiffColors.IsChecked;
            Parameters.UseRefValues = (bool)checkboxUseRefValues.IsChecked;
            Parameters.ShowAnnotations = (bool)checkboxAnnotations.IsChecked;
            Parameters.ShowGridLines = (bool)checkboxGridLines.IsChecked;

            if (cmbBarSpacing.SelectedIndex >= 0)
            {
                switch (cmbBarSpacing.SelectedIndex)
                {
                    case 1:
                        Parameters.BarSpace = BarSpacing.None;
                        break;
                    case 2:
                        Parameters.BarSpace = BarSpacing.Small;
                        break;
                    case 3:
                        Parameters.BarSpace = BarSpacing.Medium;
                        break;
                    case 4:
                        Parameters.BarSpace = BarSpacing.Large;
                        break;
                    case 0:
                    default:
                        Parameters.BarSpace = BarSpacing.Default;
                        break;
                }
            }

            if (cmbPalette.SelectedIndex >= 0)
            {
                Parameters.Palette = cmbPalette.SelectedIndex;
                Parameters.PaletteColors = GetPaletteColores();

            }

            if (cmbBarType.SelectedIndex >= 0)
            {
                switch (cmbBarType.SelectedIndex)
                {
                    case 0:
                        Parameters.BarKind = BarKind.Block;
                        break;
                    case 1:
                        Parameters.BarKind = BarKind.Cylinder;
                        break;
                    case 2:
                        Parameters.BarKind = BarKind.Rectangle;
                        break;
                    case 3:
                        Parameters.BarKind = BarKind.RoundedBlock;
                        break;
                }
            }

            //Labels settings /////////////////////////////////
            if (!String.IsNullOrEmpty(txtYAxisLabelValue.Text))
            {
                Parameters.YAxisLabel = txtYAxisLabelValue.Text;
            }

            if (cmbXAxisLabelType.SelectedIndex >= 0)
            {
                Parameters.XAxisLabelType = cmbXAxisLabelType.SelectedIndex;
            }

            if (String.IsNullOrEmpty(txtXAxisLabelValue.Text))
            {
                if (cmbXAxisLabelType.SelectedIndex == 0)
                {
                    Parameters.XAxisLabel = Parameters.ColumnNames[0];
                }
            }
            else
            {
                Parameters.XAxisLabel = txtXAxisLabelValue.Text;
            }

            if (!String.IsNullOrEmpty(txtXAxisAngle.Text))
            {
                Parameters.XAxisAngle = int.Parse(txtXAxisAngle.Text);
            }

            Parameters.ChartTitle = txtChartTitle.Text;
            Parameters.ChartSubTitle = txtChartSubTitle.Text;

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
            Parameters.IncludeFullSummaryStatistics = false;
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

            if (Parameters.ColumnNames.Count > 0)
            {
                cmbField.SelectedItem = Parameters.ColumnNames[0];
            }

            cmbFieldWeight.SelectedItem = Parameters.WeightVariableName;

            //Display settings
            scrollViewerProperties.MaxHeight = scrollViewerProperties.MaxHeight + (System.Windows.SystemParameters.PrimaryScreenHeight - 768.0);
            txtTitle.Text = Parameters.GadgetTitle;
            txtDesc.Text = Parameters.GadgetDescription;
            txtWidth.Text = Parameters.ChartWidth.ToString();
            txtHeight.Text = Parameters.ChartHeight.ToString();

            //checkboxAllValues.IsChecked = Parameters.ShowAllListValues;
            checkboxCommentLegalLabels.IsChecked = Parameters.ShowCommentLegalLabels;
            checkboxIncludeMissing.IsChecked = Parameters.IncludeMissing;

            //Display Colors and Styles settings
            scrollViewerPropertiesColors.Height = scrollViewerPropertiesColors.Height + (System.Windows.SystemParameters.PrimaryScreenHeight - 768.0);
            //checkboxUseDiffColors.IsChecked = Parameters.UseDiffColors;
            checkboxUseRefValues.IsChecked = Parameters.UseRefValues;
            checkboxAnnotations.IsChecked = Parameters.ShowAnnotations;
            checkboxGridLines.IsChecked = Parameters.ShowGridLines;

            switch (Parameters.BarSpace)
            {
                case BarSpacing.Default:
                    cmbBarSpacing.SelectedIndex = 0;
                    break;
                case BarSpacing.None:
                    cmbBarSpacing.SelectedIndex = 1;
                    break;
                case BarSpacing.Small:
                    cmbBarSpacing.SelectedIndex = 2;
                    break;
                case BarSpacing.Medium:
                    cmbBarSpacing.SelectedIndex = 3;
                    break;
                case BarSpacing.Large:
                    cmbBarSpacing.SelectedIndex = 4;
                    break;
            }

            cmbPalette.SelectedIndex = Parameters.Palette;
            switch (Parameters.BarKind)
            {
                case BarKind.Block:
                    cmbBarType.SelectedIndex = 0;
                    break;
                case BarKind.Cylinder:
                    cmbBarType.SelectedIndex = 1;
                    break;
                case BarKind.Rectangle:
                    cmbBarType.SelectedIndex = 2;
                    break;
                case BarKind.RoundedBlock:
                    cmbBarType.SelectedIndex = 3;
                    break;
            }

            //Display Labels settings
            scrollViewerPropertiesLabels.MaxHeight = scrollViewerPropertiesLabels.MaxHeight + (System.Windows.SystemParameters.PrimaryScreenHeight - 768.0);

            txtYAxisLabelValue.Text = Parameters.YAxisLabel;
            cmbXAxisLabelType.SelectedIndex = Parameters.XAxisLabelType;
            txtXAxisLabelValue.Text = Parameters.XAxisLabel;
            txtXAxisAngle.Text = Parameters.XAxisAngle.ToString();

            txtChartTitle.Text = Parameters.ChartTitle;
            txtChartSubTitle.Text = Parameters.ChartSubTitle;
            cmbPalette.SelectedIndex = Parameters.Palette;
            if (Parameters.PaletteColors.Count() > 0)
            {
                FillColorSquare(null, 0, Parameters.PaletteColors);
            }
            else
            {
                FillColorSquare(null, cmbPalette.SelectedIndex, null);
            }
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

            //if (isDropDownList || isRecoded)
            //{
            //    checkboxAllValues.IsEnabled = true;
            //}
            //else
            //{
            //    checkboxAllValues.IsEnabled = false;
            //    checkboxAllValues.IsChecked = false;
            //}

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

        private void tbtnVariables_Checked(object sender, RoutedEventArgs e)
        {
            if (panelVariables == null) return;

            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Visible;
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
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplayColors.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplayLabels.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplayLegend.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Visible;
        }

        private void cmbFieldWeight_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

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
                    if (field != null && field is RenderableField)
                    {
                        FieldFlags flags = SetFieldFlags(field as RenderableField);
                        UpdateXAxisLabelValue(cmbField.SelectedItem.ToString());
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
                txtWidth.Text = (System.Windows.SystemParameters.PrimaryScreenWidth * 2 ).ToString();
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
                    UpdateXAxisLabelValue(cmbField.Text);
                }
                else
                {
                    txtXAxisLabelValue.Text = String.Empty;
                }
                txtXAxisLabelValue.IsEnabled = false;
            }
        }

        private void UpdateXAxisLabelValue(string FieldText)
        {
            Field f = DashboardHelper.GetAssociatedField(FieldText);
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
