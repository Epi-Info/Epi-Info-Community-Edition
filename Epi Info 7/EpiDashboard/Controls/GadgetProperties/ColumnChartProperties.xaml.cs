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

namespace EpiDashboard.Controls.GadgetProperties
{
    /// <summary>
    /// Interaction logic for ColumnChartProperties.xaml
    /// </summary>
    public partial class ColumnChartProperties : GadgetPropertiesPanelBase
    {
        public ColumnChartProperties(
            DashboardHelper dashboardHelper, 
            IGadget gadget, 
            ColumnChartParameters parameters, 
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

            cmbBarSpacing.SelectedIndex = 0;
            txtYAxisLabelValue.Text = "Count";
            txtXAxisLabelValue.Text = String.Empty;
            cmbLegendDock.SelectedIndex = 1;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(cmbField.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("VariableCategory");
            view.GroupDescriptions.Add(groupDescription);

            RowFilterControl = new RowFilterControl(this.DashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, (gadget as ColumnChartGadget).DataFilters, true);
            RowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            panelFilters.Children.Add(RowFilterControl);

            //EI-98
            txtXAxisFontSize.Text = parameters.XAxisFontSize.ToString();
            txtYAxisFontSize.Text = parameters.YAxisFontSize.ToString();

            txtXAxisLabelFontSize.Text = parameters.XAxisLabelFontSize.ToString();
            txtYAxisLabelFontSize.Text = parameters.YAxisLabelFontSize.ToString();

            //Ei-418
            txtYAxismaxValue.Text = parameters.YAxisTo.ToString();
            txtYAxisminValue.Text = parameters.YAxisFrom.ToString();
            txtYAxisstepValue.Text = parameters.YAxisStep.ToString();
          
            txtWidth.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            txtHeight.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            txtLegendFontSize.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);

            #region Translation


            #endregion // Translation

        }


        protected override bool ValidateInput()
        {
            bool isValid = true;

            if (cmbField.SelectedIndex == -1)
            {
                isValid = false;
                MessageBox.Show(DashboardSharedStrings.PROPERTIES_MAIN_VARIABLE_REQ);
            }
            else if ((cmbSecondYAxis.SelectedIndex == 1 || cmbSecondYAxis.SelectedIndex == 2) && (cmbSecondYAxisVariable.SelectedIndex == -1 || cmbSecondYAxisVariable.SelectedItem.ToString() == String.Empty ))
            {
                isValid = false;
                MessageBox.Show(DashboardSharedStrings.PROPERTIES_Y2_AXIS_VARIABLE_REQ);
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

        public ColumnChartParameters Parameters 
        { 
            get; 
            private set; 
        }
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
            if (Parameters.Y2AxisType == 3)
            {
                Parameters.Y2AxisFormat = "P0";
            }

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
          //  Parameters.ChartWidth = double.Parse(txtWidth.Text);
           // Parameters.ChartHeight = double.Parse(txtHeight.Text);
            Parameters.ShowAllListValues = (bool)checkboxAllValues.IsChecked;
            Parameters.ShowCommentLegalLabels = (bool)checkboxCommentLegalLabels.IsChecked;
            Parameters.IncludeMissing = (bool)checkboxIncludeMissing.IsChecked;

            //Colors and Styles settings /////////////////////////////////
            Parameters.UseDiffColors = (bool)checkboxUseDiffColors.IsChecked;
            Parameters.UseRefValues = (bool)checkboxUseRefValues.IsChecked;
            Parameters.ShowAnnotations = (bool)checkboxAnnotations.IsChecked;
            Parameters.Y2ShowAnnotations = (bool)checkboxAnnotationsY2.IsChecked;
            Parameters.ShowGridLines = (bool)checkboxGridLines.IsChecked;
            if (cmbComposition.SelectedIndex >= 0)
            {
                switch (cmbComposition.SelectedIndex)
                {
                    default:
                    case 0:
                        Parameters.Composition = CompositionKind.SideBySide;
                        break;
                    case 1:
                        Parameters.Composition = CompositionKind.Stacked;
                        break;
                    case 2:
                        Parameters.Composition = CompositionKind.Stacked100;
                        break;
                }
            }

            if (cmbBarSpacing.SelectedIndex >= 0)
            {
                switch (cmbBarSpacing.SelectedIndex)
                {
                    default:
                    case 0:
                        Parameters.BarSpace = BarSpacing.Default;
                        break;
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
                }
            }

            if (cmbOrientation.SelectedIndex >= 0)
            {
                switch (cmbOrientation.SelectedIndex)
                {
                    default:
                    case 0:
                        Parameters.Orientation = Orientation.Vertical;
                        break;
                    case 1:
                        Parameters.Orientation = Orientation.Horizontal;
                        break;
                }
            }

            if (cmbPalette.SelectedIndex >= 0)
            {
                Parameters.Palette = cmbPalette.SelectedIndex;
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

            if (!String.IsNullOrEmpty(txtXAxisFontSize.Text))
            {
                Parameters.XAxisFontSize = double.Parse( txtXAxisFontSize.Text);
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
            //--Ei-418
            double yaxisto;
            success = double.TryParse(txtYAxismaxValue.Text, out yaxisto);
            if (success)
            {
                Parameters.YAxisTo = yaxisto;
            }
            double yaxisfrom;
            success = double.TryParse(txtYAxisminValue.Text, out yaxisfrom);
            if (success)
            {
                Parameters.YAxisFrom = yaxisfrom;
            }
            double yaxisstep;
            success = double.TryParse(txtYAxisstepValue.Text, out yaxisstep);
            if (success)
            {
                Parameters.YAxisStep = yaxisstep;
            }
          }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Variables settings
            //Potentially two columns for Column Chart, 
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
            txtWidth.Text = Parameters.ChartWidth.ToString();
            txtHeight.Text = Parameters.ChartHeight.ToString();

            checkboxAllValues.IsChecked = Parameters.ShowAllListValues;
            checkboxCommentLegalLabels.IsChecked = Parameters.ShowCommentLegalLabels;
            checkboxIncludeMissing.IsChecked = Parameters.IncludeMissing;

            //Display Colors and Styles settings
            scrollViewerPropertiesColors.Height = scrollViewerPropertiesColors.Height + (System.Windows.SystemParameters.PrimaryScreenHeight - 768.0);
            checkboxUseDiffColors.IsChecked = Parameters.UseDiffColors;
            checkboxUseRefValues.IsChecked = Parameters.UseRefValues;
            checkboxAnnotations.IsChecked = Parameters.ShowAnnotations;
            checkboxAnnotationsY2.IsChecked = Parameters.Y2ShowAnnotations;
            checkboxGridLines.IsChecked = Parameters.ShowGridLines;

            cmbComposition.SelectedItem = Parameters.Composition;
            switch (Parameters.Composition)
            {
                case CompositionKind.SideBySide:
                    cmbComposition.SelectedIndex = 0;
                    break;
                case CompositionKind.Stacked:
                    cmbComposition.SelectedIndex = 1;
                    break;
                case CompositionKind.Stacked100:
                    cmbComposition.SelectedIndex = 2;
                    break;
            }

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
            switch (Parameters.Orientation)
            {
                case Orientation.Vertical:
                    cmbOrientation.SelectedIndex = 0;
                    break;
                case Orientation.Horizontal:
                    cmbOrientation.SelectedIndex = 1;
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

            switch (Parameters.Y2LineKind)
            {
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

            txtChartTitle.Text = Parameters.ChartTitle;
            txtChartSubTitle.Text = Parameters.ChartSubTitle;

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
                    if (field != null && (field is NumberField  || field is DateField || field is TimeField))
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
                tblockSecondYAxisVariable.Text = "Second y-axis variable:";
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
                //tblockSecondYAxisVariable.Visibility = System.Windows.Visibility.Collapsed;
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
            }
            else
            {
                checkboxShowLegendBorder.IsEnabled = false;
                checkboxShowVarName.IsEnabled = false;
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
                    Field f = DashboardHelper.GetAssociatedField(cmbField.Text);
                    if(f != null && f is IDataField) 
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
    }
}
