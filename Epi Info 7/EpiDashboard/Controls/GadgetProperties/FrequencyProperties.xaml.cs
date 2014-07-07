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

namespace EpiDashboard.Controls.GadgetProperties
{
    /// <summary>
    /// Interaction logic for FrequencyProperties.xaml
    /// </summary>
    public partial class FrequencyProperties : GadgetPropertiesPanelBase
    {
        public FrequencyProperties(
            DashboardHelper dashboardHelper, 
            IGadget gadget, 
            FrequencyParameters parameters, 
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
            cbxField.ItemsSource = fields;

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
            cbxFieldWeight.ItemsSource = weightFields;

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
            lbxFieldStrata.ItemsSource = strataItems;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(cbxField.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("VariableCategory");
            view.GroupDescriptions.Add(groupDescription);

            RowFilterControl = new RowFilterControl(this.DashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, (gadget as FrequencyControl).DataFilters, true);
            RowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            panelFilters.Children.Add(RowFilterControl);

            txtRows.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            txtBarWidth.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);

            #region Translation

            lblConfigExpandedTitle.Content = DashboardSharedStrings.GADGET_CONFIG_TITLE_FREQUENCY;
            //expanderAdvancedOptions.Header = DashboardSharedStrings.GADGET_ADVANCED_OPTIONS;
            //expanderDisplayOptions.Header = DashboardSharedStrings.GADGET_DISPLAY_OPTIONS;
            tblockMainVariable.Text = DashboardSharedStrings.GADGET_FREQUENCY_VARIABLE;
            tblockStrataVariable.Text = DashboardSharedStrings.GADGET_STRATA_VARIABLE;
            tblockWeightVariable.Text = DashboardSharedStrings.GADGET_WEIGHT_VARIABLE;

            //checkboxAllValues.Content = DashboardSharedStrings.GADGET_ALL_LIST_VALUES;
            //checkboxCommentLegalLabels.Content = DashboardSharedStrings.GADGET_LIST_LABELS;
            checkboxIncludeMissing.Content = DashboardSharedStrings.GADGET_INCLUDE_MISSING;

            checkboxSortHighLow.Content = DashboardSharedStrings.GADGET_SORT_HI_LOW;
            checkboxUsePrompts.Content = DashboardSharedStrings.GADGET_USE_FIELD_PROMPT;
            //tblockOutputColumns.Text = DashboardSharedStrings.GADGET_OUTPUT_COLUMNS_DISPLAY;
            //tblockPrecision.Text = DashboardSharedStrings.GADGET_DECIMALS_TO_DISPLAY;

            tblockRows.Text = DashboardSharedStrings.GADGET_MAX_ROWS_TO_DISPLAY;
            tblockBarWidth.Text = DashboardSharedStrings.GADGET_MAX_PERCENT_BAR_WIDTH;

            //btnRun.Content = DashboardSharedStrings.GADGET_RUN_BUTTON;
            #endregion // Translation

        }

        public bool HasSelectedFields
        {
            get
            {
                if (cbxField.SelectedIndex > -1)
                {
                    return true;
                }
                return false;
            }
        }

        public FrequencyParameters Parameters { get; private set; }
        private List<Grid> StrataGridList { get; set; }

        /// <summary>
        /// Used to generate the list of variables and options for the GadgetParameters object
        /// </summary> 
        protected override void CreateInputVariableList()
        {
            // Set data filters!
            this.DataFilters = RowFilterControl.DataFilters;

            Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

            //Variables settings
            Parameters.ColumnNames = new List<string>();
            Parameters.StrataVariableNames = new List<string>();

            if (cbxField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxField.SelectedItem.ToString()))
            {
                if (Parameters.ColumnNames.Count > 0)
                {
                    Parameters.ColumnNames[0] = cbxField.SelectedItem.ToString();
                }
                else
                {
                    Parameters.ColumnNames.Add(cbxField.SelectedItem.ToString());
                }
            }
            else
            {
                return;
            }

            if (cbxFieldWeight.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldWeight.SelectedItem.ToString()))
            {
                Parameters.WeightVariableName = cbxFieldWeight.SelectedItem.ToString();
            }
            else
            {
                Parameters.WeightVariableName = String.Empty;
            }

            Parameters.SortHighToLow = (bool)checkboxSortHighLow.IsChecked;

            if (lbxFieldStrata.SelectedItems.Count > 0)
            {
                Parameters.StrataVariableNames = new List<string>();
                foreach (string s in lbxFieldStrata.SelectedItems)
                {
                    Parameters.StrataVariableNames.Add(s.ToString());
                }
            }

            //Display settings
            Parameters.GadgetTitle = txtTitle.Text;
            Parameters.GadgetDescription = txtDesc.Text;

            Parameters.ShowAllListValues = (bool)checkboxAllValues.IsChecked;
            Parameters.ShowCommentLegalLabels = (bool)checkboxCommentLegalLabels.IsChecked;
            Parameters.IncludeMissing = (bool) checkboxIncludeMissing.IsChecked;

            Parameters.UseFieldPrompts = (bool) checkboxUsePrompts.IsChecked;

            Parameters.DrawBorders = (bool) checkboxDrawBorders.IsChecked;
            Parameters.DrawHeaderRow = (bool)checkboxDrawHeader.IsChecked;
            Parameters.DrawTotalRow = (bool)checkboxDrawTotal.IsChecked;
            //Parameters.ShowNullRepresentation = (bool)checkboxShowNulls.IsChecked;

            Parameters.Precision = cbxFieldPrecision.Text;

            Parameters.PercentBarMode = cmbPercentBarMode.SelectedItem.ToString();

            //Max rows to display
            if (string.IsNullOrEmpty(txtRows.Text))
            {
                Parameters.RowsToDisplay = null;
            }
            else
            {
                int rows;
                bool success = int.TryParse(txtRows.Text, out rows);
                if (success)
                {
                    Parameters.RowsToDisplay = rows;
                }
                else
                {
                    Parameters.RowsToDisplay = null;
                    txtRows.Text = string.Empty;
                }
            }

            if (String.IsNullOrEmpty(txtBarWidth.Text))
            {
                Parameters.PercentBarWidth = 100;
                txtBarWidth.Text = "100";
            }
            else
            {
                int barWidth;
                bool bar_success = int.TryParse(txtBarWidth.Text, out barWidth);
                if(bar_success) Parameters.PercentBarWidth = barWidth;
                else Parameters.PercentBarWidth = 100;
            }

            //Columns to Display
            Parameters.ShowFrequencyCol = (bool)checkboxColumnFrequency.IsChecked;
            Parameters.ShowPercentCol = (bool)checkboxColumnPercent.IsChecked;
            Parameters.ShowCumPercentCol = (bool)checkboxColumnCumulativePercent.IsChecked;
            Parameters.Show95CILowerCol = (bool)checkboxColumn95CILower.IsChecked;
            Parameters.Show95CIUpperCol = (bool)checkboxColumn95CIUpper.IsChecked;
            Parameters.ShowPercentBarsCol = (bool)checkboxColumnPercentBars.IsChecked;

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Variables settings
            //Just one column for Frequency, .ColumnNames should have only one item
            if (Parameters.ColumnNames.Count > 0)
            {
                cbxField.SelectedItem = Parameters.ColumnNames[0];
            }
            cbxFieldWeight.SelectedItem = Parameters.WeightVariableName;
            checkboxSortHighLow.IsChecked = Parameters.SortHighToLow;
            if (Parameters.StrataVariableNames.Count > 0)
            {
                foreach (string s in Parameters.StrataVariableNames)
                {
                    //lbxFieldStrata.SelectedItem = s;
                    lbxFieldStrata.SelectedItems.Add(s.ToString());
                }
            }

            //Display settings
            txtTitle.Text = Parameters.GadgetTitle;
            txtDesc.Text = Parameters.GadgetDescription;
            checkboxAllValues.IsChecked = Parameters.ShowAllListValues;
            checkboxCommentLegalLabels.IsChecked = Parameters.ShowCommentLegalLabels;
            checkboxIncludeMissing.IsChecked = Parameters.IncludeMissing;

            checkboxUsePrompts.IsChecked = Parameters.UseFieldPrompts;

            checkboxDrawBorders.IsChecked = Parameters.DrawBorders;
            checkboxDrawHeader.IsChecked = Parameters.DrawHeaderRow;
            checkboxDrawTotal.IsChecked = Parameters.DrawTotalRow;
            //checkboxShowNulls.IsChecked = Parameters.ShowNullRepresentation;

            int precision = 2;
            bool precise_parse = int.TryParse(Parameters.Precision.ToString(), out precision);
            if (precise_parse) cbxFieldPrecision.SelectedIndex = precision;

            cmbPercentBarMode.SelectedItem = Parameters.PercentBarMode;

            txtRows.Text = Parameters.RowsToDisplay.ToString();
            txtBarWidth.Text = Parameters.PercentBarWidth.ToString();
            

            //Output columns to display
            checkboxColumnFrequency.IsChecked = Parameters.ShowFrequencyCol;
            checkboxColumnPercent.IsChecked = Parameters.ShowPercentCol;
            checkboxColumnCumulativePercent.IsChecked = Parameters.ShowCumPercentCol;
            checkboxColumn95CILower.IsChecked = Parameters.Show95CILowerCol;
            checkboxColumn95CIUpper.IsChecked = Parameters.Show95CIUpperCol;
            checkboxColumnPercentBars.IsChecked = Parameters.ShowPercentBarsCol;

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

            if (cbxField.SelectedItem != null && !string.IsNullOrEmpty(cbxField.SelectedItem.ToString()))
            {
                foreach (DataRow fieldRow in DashboardHelper.FieldTable.Rows)
                {
                    if (fieldRow["columnname"].Equals(cbxField.SelectedItem.ToString()))
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

                if (DashboardHelper.IsUserDefinedColumn(cbxField.SelectedItem.ToString()))
                {
                    List<IDashboardRule> associatedRules = DashboardHelper.Rules.GetRules(cbxField.SelectedItem.ToString());
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
            //panelSorting.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnSorting_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            //panelSorting.Visibility = System.Windows.Visibility.Visible;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnDisplay_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            //panelSorting.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Visible;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnFilters_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            //panelSorting.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Visible;
        }

        private void lbxFieldStrata_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool clearLbx = false;
            foreach (string s in lbxFieldStrata.SelectedItems)
            {
                if (s == String.Empty)
                {
                    clearLbx = true;
                }
            }
            if (clearLbx) lbxFieldStrata.SelectedItems.Clear();
        }

        private void cbxFieldWeight_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (cbxFieldWeight.SelectedValue == String.Empty)
            //{
            //    cbxFieldWeight.Items.Clear();
            //}
        }

        private void cbxField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckVariables();
        }
    }
}
