using System;
using System.Collections.Generic;
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
using EpiDashboard;
using Epi;

namespace EpiDashboard.Controls.GadgetProperties
{
    /// <summary>
    /// Interaction logic for MeansProperties.xaml
    /// </summary>
    public partial class MeansProperties : GadgetPropertiesPanelBase
    {
        public MeansProperties(
            DashboardHelper dashboardHelper, 
            IGadget gadget,
            MeansParameters parameters
            )
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            this.Gadget = gadget;
            this.Parameters = parameters;

            List<string> fields = new List<string>();
            List<FieldInfo> items = new List<FieldInfo>();
            List<string> crosstabFields = new List<string>();
            List<string> strataFields = new List<string>();

            crosstabFields.Add(string.Empty);
            items.Add(new FieldInfo()
            {
                Name = "",
                DataType = "",
                VariableCategory = VariableCategory.Field
            });

            foreach (string fieldName in DashboardHelper.GetFieldsAsList())
            {
                items.Add(new FieldInfo()
                {
                    Name = fieldName,
                    DataType = DashboardHelper.GetColumnDbType(fieldName).ToString(),
                    VariableCategory = VariableCategory.Field
                });

                fields.Add(fieldName);
                crosstabFields.Add(fieldName);
                strataFields.Add(fieldName);
            }
            fields.Sort();
            crosstabFields.Sort();

            cbxField.ItemsSource = fields;
            cbxFieldWeight.ItemsSource = fields;
            cbxFieldCrosstab.ItemsSource = crosstabFields;
            lvFieldStrata.ItemsSource = strataFields;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(cbxField.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("VariableCategory");
            view.GroupDescriptions.Add(groupDescription);

            RowFilterControl = new RowFilterControl(this.DashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, (gadget as MeansControl).DataFilters, true);
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

        private void lbxColumns_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        public MeansParameters Parameters { get; private set; }

        /// <summary>
        /// Used to generate the list of variables and options for the GadgetParameters object
        /// </summary> 
        protected override void CreateInputVariableList()
        {
            // Set data filters!
            this.DataFilters = RowFilterControl.DataFilters;

            Dictionary<string, string> inputVariableList = new Dictionary<string, string>();
            Parameters.ColumnNames = new List<string>();

            Parameters.SortVariables = new Dictionary<string, SortOrder>();

            Parameters.GadgetTitle = txtTitle.Text;
            Parameters.GadgetDescription = txtDesc.Text;

            List<string> listFields = new List<string>();

            Parameters.ShowCommentLegalLabels = checkboxListLabels.IsChecked.Value;
            Parameters.InputVariableList = inputVariableList;

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

            if (lvFieldStrata.SelectedItems.Count > 0)
            {
                Parameters.StrataVariableNames = new List<string>();
                foreach (String s in lvFieldStrata.SelectedItems)
                {
                    if (!string.IsNullOrEmpty(s))
                        Parameters.StrataVariableNames.Add(s);
                }
            }
            else
                Parameters.StrataVariableNames.Clear();

            if (cbxFieldCrosstab.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldCrosstab.SelectedItem.ToString()))
            {
                Parameters.CrosstabVariableName = cbxFieldCrosstab.SelectedItem.ToString();
            }
            else
            {
                Parameters.CrosstabVariableName = String.Empty;
            }

            if (checkboxShowANOVA.IsChecked == true)
            {
                inputVariableList.Add("showanova", "true");
                Parameters.ShowANOVA = true;
            }
            else
            {
                inputVariableList.Add("showanova", "false");
                Parameters.ShowANOVA = false;
            }

            if (cbxFieldPrecision.SelectedIndex >= 0)
            {
                inputVariableList.Add("precision", cbxFieldPrecision.SelectedIndex.ToString());
                Parameters.Precision = cbxFieldPrecision.SelectedIndex.ToString();
            }
            Parameters.columnsToHide.Clear();
            if (lbxColumns.SelectedItems.Count > 0)
            {
                if (!lbxColumns.SelectedItems.Contains("Observations")) Parameters.columnsToHide.Add(1);
                if (!lbxColumns.SelectedItems.Contains("Total")) Parameters.columnsToHide.Add(2);
                if (!lbxColumns.SelectedItems.Contains("Mean")) Parameters.columnsToHide.Add(3);
                if (!lbxColumns.SelectedItems.Contains("Variance")) Parameters.columnsToHide.Add(4);
                if (!lbxColumns.SelectedItems.Contains("Std. Dev.")) Parameters.columnsToHide.Add(5);
                if (!lbxColumns.SelectedItems.Contains("Minimum")) Parameters.columnsToHide.Add(6);
                if (!lbxColumns.SelectedItems.Contains("25%")) Parameters.columnsToHide.Add(7);
                if (!lbxColumns.SelectedItems.Contains("Median")) Parameters.columnsToHide.Add(8);
                if (!lbxColumns.SelectedItems.Contains("75%")) Parameters.columnsToHide.Add(9);
                if (!lbxColumns.SelectedItems.Contains("Maximum")) Parameters.columnsToHide.Add(10);
                if (!lbxColumns.SelectedItems.Contains("Mode")) Parameters.columnsToHide.Add(11);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Dictionary<string, string> inputVariableList = Parameters.InputVariableList;

            //Just one column for Frequency, .ColumnNames should have only one item
            txtTitle.Text = Parameters.GadgetTitle;
            txtDesc.Text = Parameters.GadgetDescription;
            if (Parameters.ColumnNames.Count > 0)
            {
                cbxField.SelectedItem = Parameters.ColumnNames[0];
            }
            cbxFieldWeight.SelectedItem = Parameters.WeightVariableName;
            cbxFieldCrosstab.SelectedItem = Parameters.CrosstabVariableName;
            checkboxShowANOVA.IsChecked = Parameters.ShowANOVA;
            cbxFieldPrecision.SelectedIndex = Convert.ToInt32(Parameters.Precision);
            lvFieldStrata.MaxHeight = lvFieldStrata.MaxHeight + (System.Windows.SystemParameters.PrimaryScreenHeight - 768.0);
            scrollViewerDisplay.Height = scrollViewerDisplay.Height + (System.Windows.SystemParameters.PrimaryScreenHeight - 768.0);
            if (Parameters.StrataVariableNames.Count > 0)
            {
                foreach (string s in Parameters.StrataVariableNames)
                    lvFieldStrata.SelectedItems.Add(s.ToString());
            }

            if (lbxColumns.Items.Count == 0)
            {
                lbxColumns.Items.Add("Observations");
                lbxColumns.Items.Add("Total");
                lbxColumns.Items.Add("Mean");
                lbxColumns.Items.Add("Variance");
                lbxColumns.Items.Add("Std. Dev.");
                lbxColumns.Items.Add("Minimum");
                lbxColumns.Items.Add("25%");
                lbxColumns.Items.Add("Median");
                lbxColumns.Items.Add("75%");
                lbxColumns.Items.Add("Maximum");
                lbxColumns.Items.Add("Mode");
                lbxColumns.SelectAll();
            }
            //checkboxShowAllListValues.IsChecked = Parameters.ShowAllListValues;
            //checkboxShowListLabels.IsChecked = Parameters.ShowListLabels;
            //checkboxSortHighLow.IsChecked = Parameters.SortHighToLow;
            //checkboxIncludeMissing.IsChecked = Parameters.IncludeMissing;

            //checkboxUsePrompts.IsChecked = Parameters.UseFieldPrompts;
            //checkboxDrawBorders.IsChecked = Parameters.DrawBorders;
            //checkboxDrawHeader.IsChecked = Parameters.DrawHeaderRow;
            //checkboxDrawTotal.IsChecked = Parameters.DrawTotalRow;

            //cbxFieldPrecision.SelectedItem = Parameters.Precision;

            //tblockBarWidth.Text = Parameters.PercentBarWidth.ToString();

            //checkboxColumnFrequency.IsChecked = Parameters.ShowFrequencyCol;
            //checkboxColumnPercent.IsChecked = Parameters.ShowPercentCol;
            //checkboxColumnCumulativePercent.IsChecked = Parameters.ShowCumPercentCol;
            //checkboxColumn95CILower.IsChecked = Parameters.Show95CILowerCol;
            //checkboxColumn95CIUpper.IsChecked = Parameters.Show95CIUpperCol;
            //checkboxColumnPercentBars.IsChecked = Parameters.ShowPercentBarsCol;
        }

        public class FieldInfo { public string Name { get; set; } public string DataType { get; set; } public VariableCategory VariableCategory { get; set; } }

        private void tbtnVariables_Checked(object sender, RoutedEventArgs e)
        {
            if (panelVariables == null) return;

            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Visible;
            panelSorting.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnSorting_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelSorting.Visibility = System.Windows.Visibility.Visible;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnDisplay_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelSorting.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Visible;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnFilters_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as SettingsToggleButton);
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelSorting.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Visible;
        }

        private void lbxAvailableVariables_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (lbxAvailableVariables.SelectedItems.Count == 1)
            //{
            //    string fieldName = lbxAvailableVariables.SelectedItem.ToString();

            //    lbxAvailableVariables.Items.Remove(fieldName);

            //    string method = " (ascending)";
            //    if (cmbSortMethod.SelectedIndex == 1)
            //    {
            //        method = " (descending)";
            //    }
            //    lbxSortOrder.Items.Add(fieldName + method);
            //}
        }

        private void lbxSortOrder_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (lbxSortOrder.SelectedItems.Count == 1)
            //{
            //    string fieldName = lbxSortOrder.SelectedItem.ToString();

            //    lbxSortOrder.Items.Remove(fieldName);

            //    string originalFieldName = fieldName.Replace(" (ascending)", String.Empty).Replace(" (descending)", String.Empty);

            //    lbxAvailableVariables.Items.Add(originalFieldName);

            //    List<string> items = new List<string>();

            //    foreach (string item in lbxAvailableVariables.Items)
            //    {
            //        items.Add(item);
            //    }

            //    items.Sort();

            //    lbxAvailableVariables.Items.Clear();

            //    foreach (string item in items)
            //    {
            //        lbxAvailableVariables.Items.Add(item);
            //    }
            //}
        }
    }
}
