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

namespace EpiDashboard.Controls.GadgetProperties
{
    /// <summary>
    /// Interaction logic for AberrationDetectionChartProperties.xaml
    /// </summary>
    public partial class AberrationDetectionChartProperties : GadgetPropertiesPanelBase
    {
        public AberrationDetectionChartProperties(
            DashboardHelper dashboardHelper,
            IGadget gadget,
            AberrationDetectionChartParameters parameters,
            List<Grid> strataGridList
            )
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            this.Gadget = gadget;
            this.Parameters = parameters;
            this.StrataGridList = strataGridList;

            //Variable fields

            List<string> fieldNames = new List<string>();
            List<string> weightFieldNames = new List<string>();
            List<string> strataFieldNames = new List<string>();

            ColumnDataType columnDataType = ColumnDataType.DateTime | ColumnDataType.UserDefined;
            fieldNames = DashboardHelper.GetFieldsAsList(columnDataType);

            weightFieldNames.Add(string.Empty);
            columnDataType = ColumnDataType.Numeric | ColumnDataType.UserDefined;
            weightFieldNames.AddRange(DashboardHelper.GetFieldsAsList(columnDataType));

            strataFieldNames.Add(string.Empty); 
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

            txtYAxisLabelValue.Text = "Count";
            txtXAxisLabelValue.Text = String.Empty;
            
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(cmbField.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("VariableCategory");
            view.GroupDescriptions.Add(groupDescription);

            RowFilterControl = new RowFilterControl(this.DashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, (gadget as AberrationChartGadget).DataFilters, true);
            RowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            panelFilters.Children.Add(RowFilterControl);

            //EI-98
            txtXAxisFontSize.Text = parameters.XAxisFontSize.ToString();
            txtYAxisFontSize.Text = parameters.YAxisFontSize.ToString();

            txtXAxisLabelFontSize.Text = parameters.XAxisLabelFontSize.ToString();
            txtYAxisLabelFontSize.Text = parameters.YAxisLabelFontSize.ToString();

            txtWidth.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            txtHeight.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            txtLagTime.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            txtDeviations.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            txtTimePeriod.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);

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

            //EI-98
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

        public AberrationDetectionChartParameters Parameters { get; private set; }
        private List<Grid> StrataGridList { get; set; }

        /// <summary>
        /// Used to generate the list of variables and options for the GadgetParameters object
        /// </summary> 
        protected override void CreateInputVariableList()
        {
            // Set data filters!
            this.DataFilters = RowFilterControl.DataFilters;

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

            if (!String.IsNullOrEmpty(txtLagTime.Text))
            {
                Parameters.LagTime = txtLagTime.Text;
            }
            if (!String.IsNullOrEmpty(txtDeviations.Text))
            {
                Parameters.Deviations = txtDeviations.Text;
            }
            if (!String.IsNullOrEmpty(txtTimePeriod.Text))
            {
                Parameters.TimePeriod = txtTimePeriod.Text;
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

            //Display settings ///////////////////////////////////////////

            Parameters.GadgetTitle = txtTitle.Text;
            Parameters.GadgetDescription = txtDesc.Text;
            Parameters.ChartWidth = double.Parse(txtWidth.Text);
            Parameters.ChartHeight = double.Parse(txtHeight.Text);

            //Labels settings /////////////////////////////////
            if (!String.IsNullOrEmpty(txtYAxisLabelValue.Text))
            {
                Parameters.YAxisLabel = txtYAxisLabelValue.Text;
            }

            if (cmbXAxisLabelType.SelectedIndex >= 0)
            {
                Parameters.XAxisLabelType = cmbXAxisLabelType.SelectedIndex;
            }

            if (!String.IsNullOrEmpty(txtXAxisLabelValue.Text))
            {
                Parameters.XAxisLabel = txtXAxisLabelValue.Text;
            }

            Parameters.ChartTitle = txtChartTitle.Text;

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
            //Potentially two columns for Line Chart, 
            //.ColumnNames[0] = Main Variable
            //.ColumnNames[1] = Second Y-axis Variable

            if (Parameters.ColumnNames.Count > 0)
            {
                cmbField.SelectedItem = Parameters.ColumnNames[0];
            }

            cmbFieldWeight.SelectedItem = Parameters.WeightVariableName;

            if (!String.IsNullOrEmpty(Parameters.LagTime))
            {
                txtLagTime.Text = Parameters.LagTime;
            }
            else
            {
                txtLagTime.Text = "7";
            }

            if (!String.IsNullOrEmpty(Parameters.Deviations))
            {
                txtDeviations.Text = Parameters.Deviations;
            }
            else
            {
                txtDeviations.Text = "3";
            }

            if (!String.IsNullOrEmpty(Parameters.TimePeriod))
            {
                txtTimePeriod.Text = Parameters.TimePeriod;
            }
            else
            {
                txtTimePeriod.Text = "365";
            }

            //Sorting and Grouping settings
            if (Parameters.StrataVariableNames.Count > 0)
            {
                foreach (string s in Parameters.StrataVariableNames)
                {
                    listboxFieldStrata.SelectedItems.Add(s.ToString());
                }
            }

            //Display settings
            scrollViewerProperties.MaxHeight = scrollViewerProperties.MaxHeight + (System.Windows.SystemParameters.PrimaryScreenHeight - 768.0);
            txtTitle.Text = Parameters.GadgetTitle;
            txtDesc.Text = Parameters.GadgetDescription;
            txtWidth.Text = Parameters.ChartWidth.ToString();
            txtHeight.Text = Parameters.ChartHeight.ToString();


            //Display Labels settings
            scrollViewerPropertiesLabels.MaxHeight = scrollViewerPropertiesLabels.MaxHeight + (System.Windows.SystemParameters.PrimaryScreenHeight - 768.0);

            txtYAxisLabelValue.Text = Parameters.YAxisLabel;
            cmbXAxisLabelType.SelectedIndex = Parameters.XAxisLabelType;
            txtXAxisLabelValue.Text = Parameters.XAxisLabel;

            txtChartTitle.Text = Parameters.ChartTitle;

            CheckVariables();
        }

        public class FieldInfo { public string Name { get; set; } public string DataType { get; set; } public VariableCategory VariableCategory { get; set; } }

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
        }


        private void tbtnVariables_Checked(object sender, RoutedEventArgs e)
        {
            if (panelVariables == null) return;

            SettingsToggleButton stb = sender as SettingsToggleButton;
            if (stb != null)
            {
                CheckButtonStates(stb);
                panelVariables.Visibility = System.Windows.Visibility.Visible;
                panelSorting.Visibility = System.Windows.Visibility.Collapsed;
                panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
                panelDisplayLabels.Visibility = System.Windows.Visibility.Collapsed;
                panelFilters.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void tbtnSorting_Checked(object sender, RoutedEventArgs e)
        {
            SettingsToggleButton stb = sender as SettingsToggleButton;
            if (stb != null)
            {
                CheckButtonStates(stb);
                panelVariables.Visibility = System.Windows.Visibility.Collapsed;
                panelSorting.Visibility = System.Windows.Visibility.Visible;
                panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
                panelDisplayLabels.Visibility = System.Windows.Visibility.Collapsed;
                panelFilters.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void tbtnDisplay_Checked(object sender, RoutedEventArgs e)
        {
            SettingsToggleButton stb = sender as SettingsToggleButton;
            if (stb != null)
            {
                CheckButtonStates(stb);
                panelVariables.Visibility = System.Windows.Visibility.Collapsed;
                panelSorting.Visibility = System.Windows.Visibility.Collapsed;
                panelDisplay.Visibility = System.Windows.Visibility.Visible;
                panelDisplayLabels.Visibility = System.Windows.Visibility.Collapsed;
                panelFilters.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void tbtnDisplayLabels_Checked(object sender, RoutedEventArgs e)
        {
            SettingsToggleButton stb = sender as SettingsToggleButton;
            if (stb != null)
            {
                CheckButtonStates(stb);
                panelVariables.Visibility = System.Windows.Visibility.Collapsed;
                panelSorting.Visibility = System.Windows.Visibility.Collapsed;
                panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
                panelDisplayLabels.Visibility = System.Windows.Visibility.Visible;
                panelFilters.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void tbtnFilters_Checked(object sender, RoutedEventArgs e)
        {
            SettingsToggleButton stb = sender as SettingsToggleButton;
            if (stb != null)
            {
                CheckButtonStates(stb);
                panelVariables.Visibility = System.Windows.Visibility.Collapsed;
                panelSorting.Visibility = System.Windows.Visibility.Collapsed;
                panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
                panelDisplayLabels.Visibility = System.Windows.Visibility.Collapsed;
                panelFilters.Visibility = System.Windows.Visibility.Visible;
            }
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

        protected virtual void cmbField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null)
            {
                ComboBox cmbField = sender as ComboBox;

                if (cmbField != null)
                {

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
                    if (f != null)
                    {
                        Epi.Fields.IDataField dataField = f as IDataField;
                        if (dataField != null)
                        {
                            txtXAxisLabelValue.Text = dataField.PromptText;
                        }
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
    }
}
