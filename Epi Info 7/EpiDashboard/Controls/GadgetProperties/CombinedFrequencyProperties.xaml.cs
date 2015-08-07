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
    /// Interaction logic for CombinedFrequencyProperties.xaml
    /// </summary>
    public partial class CombinedFrequencyProperties : GadgetPropertiesPanelBase
    {
        public CombinedFrequencyProperties(
            DashboardHelper dashboardHelper, 
            IGadget gadget, 
            IGadgetParameters parameters
            )
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            this.Gadget = gadget;
            this.Parameters = (parameters as CombinedFrequencyParameters);

            List<FieldInfo> items = new List<FieldInfo>();
            List<string> fields = new List<string>();

            ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.UserDefined;
            //Commenting for EI-392
            //foreach (string fieldName in DashboardHelper.GetFieldsAsList(columnDataType)) 
            //{
            //    items.Add(new FieldInfo()
            //    {
            //        Name = fieldName,
            //        DataType = DashboardHelper.GetColumnDbType(fieldName).ToString(),
            //        VariableCategory = VariableCategory.Field
            //    });

            //    fields.Add(fieldName);
            //}

            foreach (string fieldName in DashboardHelper.GetAllGroupsAsList())
            {
                FieldInfo fieldInfo = new FieldInfo()
                {
                    Name = fieldName,
                    DataType = String.Empty,
                    VariableCategory = VariableCategory.Group
                };
                items.Add(fieldInfo);
            }
            //Commenting for EI-392
            //if (DashboardHelper.IsUsingEpiProject)
            //{
            //    for (int i = 0; i < DashboardHelper.View.Pages.Count; i++)
            //    {
            //        items.Add(new FieldInfo()
            //        {
            //            Name = "Page " + (i + 1).ToString(),
            //            DataType = String.Empty,
            //            VariableCategory = VariableCategory.Page
            //        });
            //    }
            //}

            fields.Sort();

            lvGroupVariables.ItemsSource = items;
            //cmbGroupField.ItemsSource = fields;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvGroupVariables.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("VariableCategory");
            view.GroupDescriptions.Add(groupDescription);

            RowFilterControl = new RowFilterControl(this.DashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, (gadget as CombinedFrequencyControl).DataFilters, true);
            RowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            panelFilters.Children.Add(RowFilterControl);



            #region Translation


            tbtnVariables.Title = DashboardSharedStrings.GADGET_TABBUTTON_VARIABLES;
            tbtnVariables.Description = DashboardSharedStrings.GADGET_TABDESC_COMBINEDFEQ;
            tbtnSorting.Title = DashboardSharedStrings.GADGET_TABBUTTON_SORTING;
            tbtnSorting.Description = DashboardSharedStrings.GADGET_TABDESC_SORTING;
            tbtnDisplay.Title = DashboardSharedStrings.GADGET_TABBUTTON_DISPLAY;
            tbtnDisplay.Description = DashboardSharedStrings.GADGET_TABDESC_DISPLAY;
            tbtnFilters.Title = DashboardSharedStrings.GADGET_TABBUTTON_FILTERS;
            tbtnFilters.Description = DashboardSharedStrings.GADGET_TABDESC_FILTERS;
            tblockPanelVariables.Content = DashboardSharedStrings.GADGET_PANELHEADER_VARIABLES;
            tblockPanelSorting.Content = DashboardSharedStrings.GADGET_PANELHEADER_SORTING;
            tblockPanelDisplay.Content = DashboardSharedStrings.GADGET_PANELHEADER_DISPLAY;
            tblockSortingSubheader.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_SORTING;
            tblockGroupingSubheader.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_GROUPING;
            tblockTitleNDescSubheader.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_TITLENDESC;
            tblockTitle.Content = DashboardSharedStrings.GADGET_GADET_TITLE;
            tblockDesc.Content = DashboardSharedStrings.GADGET_DESCRIPTION;
            tblockPanelOutputOpt.Content = DashboardSharedStrings.GADGET_OUTPUT_OPTIONS;
            tblockSortMethod.Text = DashboardSharedStrings.GADGET_SORT_METHOD;
            tblockAnyFilterGadgetOnly.Content = DashboardSharedStrings.GADGET_FILTER_GADGET_ONLY;
            tblockVariablesToDisplay.Content = DashboardSharedStrings.GADGET_VARIABLES_TO_DISPLAY;
            tblockCombineMode.Text = DashboardSharedStrings.GADGET_COMBINE_MODE;
            checkboxShowDenominator.Content = DashboardSharedStrings.GADGET_SHOW_DENOMINATOR;
            lblConfigExpandedTitle.Content = DashboardSharedStrings.GADGET_CONFIG_TITLE_COMBINEDFEQ;
            tblockPanelDataFilter.Content = DashboardSharedStrings.GADGET_PANELHEADER_DATA_FILTER;
            checkboxSortHighLow.Content = DashboardSharedStrings.GADGET_SORT_HI_LOW;



            #endregion // Translation

        }

        public bool HasSelectedFields
        {
            get
            {
                if (lvGroupVariables.SelectedItems.Count > 0)
                {
                    return true;
                }
                return false;
            }
        }
        public CombinedFrequencyParameters Parameters { get; private set; }
        private List<Grid> StrataGridList { get; set; }
        private List<string> ColumnOrder { get; set; }

        /// <summary>
        /// Used to generate the list of variables and options for the GadgetParameters object
        /// </summary> 
        protected override void CreateInputVariableList()
        {
            this.DataFilters = RowFilterControl.DataFilters;

            Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

            Parameters.ColumnNames = new List<string>();
            Parameters.GadgetTitle = txtTitle.Text;
            Parameters.GadgetDescription = txtDesc.Text;
            
            List<string> listFields = new List<string>();

            if (lvGroupVariables.SelectedItems.Count > 0)
            {
                foreach (FieldInfo fieldInfo in lvGroupVariables.SelectedItems)
                {
                    if (!string.IsNullOrEmpty(fieldInfo.Name))
                    {
                        listFields.Add(fieldInfo.Name);
                    }
                }
            }

            listFields.Sort();

            foreach (string field in listFields)
            {
                Parameters.ColumnNames.Add(field);
            }

            Parameters.CombineMode = CombineModeTypes.Automatic;
            switch (cmbCombineMode.Text)
            {
                case "Boolean":
                    Parameters.CombineMode = CombineModeTypes.Boolean;                    
                    break;
                case "Categorical":
                    Parameters.CombineMode = CombineModeTypes.Categorical;
                    break;
            }
            ValidateInput();
            Parameters.TrueValue = txtTrueValue.Text;
            Parameters.SortHighToLow = checkboxSortHighLow.IsChecked.Value;
            Parameters.ShowDenominator = checkboxShowDenominator.IsChecked.Value;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            lvGroupVariables.MaxHeight = this.ActualHeight - 200;

            Dictionary<string, string> inputVariableList = Parameters.InputVariableList;

            foreach (string columnName in Parameters.ColumnNames)
            {
                foreach (FieldInfo info in lvGroupVariables.Items)
                {
                    if (info.Name == columnName)
                    {
                        lvGroupVariables.SelectedItem = info;
                        break;
                    }
                }
            }

            switch (Parameters.CombineMode)
            {
                case CombineModeTypes.Automatic:
                    cmbCombineMode.SelectedIndex = 0;
                    break;
                case CombineModeTypes.Boolean:
                    cmbCombineMode.SelectedIndex = 1;
                    break;
                case CombineModeTypes.Categorical:
                    cmbCombineMode.SelectedIndex = 2;
                    break;
            }

            if (String.IsNullOrEmpty(Parameters.TrueValue))
            {
                tblockTrueValue.Visibility = System.Windows.Visibility.Collapsed;
                txtTrueValue.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                tblockTrueValue.Visibility = System.Windows.Visibility.Visible;
                txtTrueValue.Visibility = System.Windows.Visibility.Visible;
            }

            txtTrueValue.Text = Parameters.TrueValue;

            checkboxShowDenominator.IsChecked = Parameters.ShowDenominator;
            checkboxSortHighLow.IsChecked = Parameters.SortHighToLow;

            txtTitle.Text = Parameters.GadgetTitle;
            txtDesc.Text = Parameters.GadgetDescription;
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

        /// <summary>
        /// Handles the selection changed event for the combine mode combo box
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void cmbCombineMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (txtTrueValue != null && tblockTrueValue != null)
            {
                if (cmbCombineMode.SelectedIndex == 1)
                {
                    txtTrueValue.Visibility = System.Windows.Visibility.Visible;
                    tblockTrueValue.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    txtTrueValue.Visibility = System.Windows.Visibility.Collapsed;
                    tblockTrueValue.Visibility = System.Windows.Visibility.Collapsed;
                    txtTrueValue.Text = string.Empty;
                }
            }
        }

        protected override bool ValidateInput()
        {
            bool isValid = true;

            if (cmbCombineMode.SelectedIndex == 1)
            {
                if (String.IsNullOrEmpty(txtTrueValue.Text.ToString().Trim()))
                { 
                    isValid = false;
                    MessageBox.Show(DashboardSharedStrings.GADGET_TRUE_VALUE_REQ);
                    panelVariables.Visibility = System.Windows.Visibility.Collapsed;
                    panelSorting.Visibility = System.Windows.Visibility.Visible;
                    panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
                    panelFilters.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
            return isValid;
        }
    }
}
