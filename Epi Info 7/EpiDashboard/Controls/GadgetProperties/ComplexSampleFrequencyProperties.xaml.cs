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
    /// Interaction logic for ComplexSampleFrequencyProperties.xaml
    /// </summary>
    public partial class ComplexSampleFrequencyProperties : GadgetPropertiesPanelBase
    {
        public ComplexSampleFrequencyProperties(
            DashboardHelper dashboardHelper, 
            IGadget gadget, 
            ComplexSampleFrequencyParameters parameters, 
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
            cbxFieldStrata.ItemsSource = strataItems;
            cbxFieldPSU.ItemsSource = strataItems;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(cbxField.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("VariableCategory");
            view.GroupDescriptions.Add(groupDescription);

            RowFilterControl = new RowFilterControl(this.DashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, (gadget as ComplexSampleFrequencyControl).DataFilters, true);
            RowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            panelFilters.Children.Add(RowFilterControl);

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

        public ComplexSampleFrequencyParameters Parameters { get; private set; }
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
            Parameters.WeightVariableName = String.Empty;
            Parameters.StrataVariableNames = new List<string>();
            Parameters.CrosstabVariableName = string.Empty;
            Parameters.PSUVariableName = string.Empty;
            
            if (cbxField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxField.SelectedItem.ToString()) )
            {
                inputVariableList.Add("IdentifierList", cbxField.SelectedItem.ToString());
                if (Parameters.ColumnNames.Count > 0)
                {
                    Parameters.ColumnNames[0] = cbxField.SelectedItem.ToString();
                }
                else
                {
                    Parameters.ColumnNames.Add(cbxField.SelectedItem.ToString());
                }
            }
            if(cbxFieldPSU.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldPSU.SelectedItem.ToString()))
            {
                inputVariableList.Add("PSUVar", cbxFieldPSU.SelectedItem.ToString());
                Parameters.PSUVariableName = cbxFieldPSU.SelectedItem.ToString();            
            }
            else
            {
                return;
            }
            if (cbxFieldWeight.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldWeight.SelectedItem.ToString()))
            {
                inputVariableList.Add("WeightVar", cbxFieldWeight.SelectedItem.ToString());
                Parameters.WeightVariableName = cbxFieldWeight.SelectedItem.ToString();
            }
            else
            {
                inputVariableList.Add("WeightVar", string.Empty); 
                Parameters.WeightVariableName = string.Empty;
            }

            if (cbxFieldStrata.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldStrata.SelectedItem.ToString()))
            {
                inputVariableList.Add("StratvarList", cbxFieldStrata.SelectedItem.ToString());
                Parameters.StrataVariableNames = new List<string>();
                Parameters.StrataVariableNames.Add(cbxFieldStrata.SelectedItem.ToString());
            }
            Parameters.IncludeFullSummaryStatistics = false;
            Parameters.InputVariableList = inputVariableList;

            //Display settings
            Parameters.GadgetTitle = txtTitle.Text;
            Parameters.GadgetDescription = txtDesc.Text;
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
            cbxFieldPSU.SelectedItem = Parameters.PSUVariableName;

            if (Parameters.StrataVariableNames.Count > 0)
            {
                cbxFieldStrata.SelectedItem = Parameters.StrataVariableNames[0].ToString();
            }

            //Display settings
            txtTitle.Text = Parameters.GadgetTitle;
            txtDesc.Text = Parameters.GadgetDescription;

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
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnSorting_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnDisplay_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Visible;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnFilters_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Visible;
        }

        private void cbxField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckVariables();
        }
    }
}
