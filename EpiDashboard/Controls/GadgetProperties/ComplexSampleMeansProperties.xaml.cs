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
    /// Interaction logic for ComplexSampleMeansProperties.xaml
    /// </summary>
    public partial class ComplexSampleMeansProperties : GadgetPropertiesPanelBase
    {
        public ComplexSampleMeansProperties(
            DashboardHelper dashboardHelper, 
            IGadget gadget,
            ComplexSampleMeansParameters parameters,
            List<Grid> strataGridList
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
            cbxFieldStrata.ItemsSource = strataFields;
            cbxFieldPSU.ItemsSource = strataFields;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(cbxField.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("VariableCategory");
            view.GroupDescriptions.Add(groupDescription);

            RowFilterControl = new RowFilterControl(this.DashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, (gadget as ComplexSampleMeansControl).DataFilters, true);
            RowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            panelFilters.Children.Add(RowFilterControl);



            #region Translation

            lblConfigExpandedTitle.Content = DashboardSharedStrings.GADGET_CONFIG_TITLE_COMPLEX_MEANS;
            tbtnVariables.Title = DashboardSharedStrings.GADGET_TABBUTTON_VARIABLES;
            tbtnVariables.Description = DashboardSharedStrings.GADGET_TABDESC_COMPLEX_MEANS;
            tbtnDisplay.Title = DashboardSharedStrings.GADGET_TABBUTTON_DISPLAY;
            tbtnDisplay.Description = DashboardSharedStrings.GADGET_TABDESC_DISPLAY;
            tbtnFilters.Title = DashboardSharedStrings.GADGET_TABBUTTON_FILTERS;
            tbtnFilters.Description = DashboardSharedStrings.GADGET_TABDESC_FILTERS;
            tblockPanelVariables.Content = DashboardSharedStrings.GADGET_PANELHEADER_VARIABLES;
            tblockWeightVariable.Content = DashboardSharedStrings.GADGET_WEIGHT_VARIABLE;
            tblockPanelDisplay.Content = DashboardSharedStrings.GADGET_PANELHEADER_DISPLAY;
            tblockTitleNDescSubheader.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_TITLENDESC;
            tblockTitle.Content = DashboardSharedStrings.GADGET_GADET_TITLE;
            tblockDesc.Content = DashboardSharedStrings.GADGET_DESCRIPTION;
            tblockPanelDataFilter.Content = DashboardSharedStrings.GADGET_PANELHEADER_DATA_FILTER;
            tblockAnyFilterGadgetOnly.Content = DashboardSharedStrings.GADGET_FILTER_GADGET_ONLY;
            tblockStrataVariable.Text = DashboardSharedStrings.GADGET_STRATA_VARIABLE;
            tblockPSU.Text = DashboardSharedStrings.GADGET_PSU;
            tblockCrosstabVariable.Text = DashboardSharedStrings.GADGET_CROSSTAB_VARIABLE;
            tblockMeansOf.Content = DashboardSharedStrings.GADGET_MEANS_VARIABLE;
            btnOK.Content = DashboardSharedStrings.BUTTON_OK;
            btnCancel.Content = DashboardSharedStrings.BUTTON_CANCEL;
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

        private void lbxColumns_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        public ComplexSampleMeansParameters Parameters { get; private set; }

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

            Parameters.InputVariableList = inputVariableList;

            if (cbxField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxField.SelectedItem.ToString()) )               
            {
                inputVariableList.Add("Identifier", cbxField.SelectedItem.ToString());
                if (Parameters.ColumnNames.Count > 0)
                {
                    Parameters.ColumnNames[0] = cbxField.SelectedItem.ToString();
                }
                else
                {
                    Parameters.ColumnNames.Add(cbxField.SelectedItem.ToString());
                }
            }
            if (cbxFieldPSU.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldPSU.SelectedItem.ToString()))
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

            if (cbxFieldStrata.SelectedIndex > -1)
            {
                Parameters.StrataVariableNames = new List<string>();
                Parameters.StrataVariableNames.Add(cbxFieldStrata.SelectedItem.ToString());
                inputVariableList.Add("stratavar", cbxFieldStrata.SelectedItem.ToString());
            }
            else
                Parameters.StrataVariableNames.Clear();

            if (cbxFieldCrosstab.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldCrosstab.SelectedItem.ToString()))
            {
                inputVariableList.Add("Cross_Tabulation_Variable", cbxFieldCrosstab.SelectedItem.ToString());
                Parameters.CrosstabVariableName = cbxFieldCrosstab.SelectedItem.ToString();
            }
            else
            {
                Parameters.CrosstabVariableName = String.Empty;
                inputVariableList.Add("Cross_Tabulation_Variable", String.Empty);
            }

            Parameters.InputVariableList = inputVariableList;
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
            cbxFieldPSU.SelectedItem = Parameters.PSUVariableName; 
            cbxFieldCrosstab.SelectedItem = Parameters.CrosstabVariableName;
            if (Parameters.StrataVariableNames.Count > 0)
            {
                cbxFieldStrata.SelectedItem = Parameters.StrataVariableNames[0].ToString();
            }

            scrollViewerDisplay.Height = scrollViewerDisplay.Height + (System.Windows.SystemParameters.PrimaryScreenHeight - 768.0);
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
