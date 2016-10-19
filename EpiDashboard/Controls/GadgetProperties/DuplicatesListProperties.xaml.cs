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
    /// Interaction logic for DuplicatesListProperties.xaml
    /// </summary>
    public partial class DuplicatesListProperties : GadgetPropertiesPanelBase
    {
        public DuplicatesListProperties(
            DashboardHelper dashboardHelper, 
            IGadget gadget, 
            DuplicatesListParameters parameters, 
            List<Grid> strataGridList,
            List<string> columnOrder
            )
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            this.Gadget = gadget;
            this.Parameters = parameters;
            this.StrataGridList = strataGridList;
            this.ColumnOrder = columnOrder;

            List<FieldInfo> items = new List<FieldInfo>();
            List<string> fields = new List<string>();

            foreach (string fieldName in DashboardHelper.GetFieldsAsList())
            {
                items.Add(new FieldInfo()
                {
                    Name = fieldName,
                    DataType = DashboardHelper.GetColumnDbType(fieldName).ToString(),
                    VariableCategory = VariableCategory.Field
                });

                fields.Add(fieldName);
            }

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

            /// USED ONLY FOR PAGES
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

            lvVariables.ItemsSource = items;
            lvDisplayVariables.ItemsSource = items;

            cmbGroupField.ItemsSource = fields;
            cmbSecondaryGroupField.ItemsSource = fields;

            foreach (string fieldName in fields)
            {
                lbxAvailableVariables.Items.Add(fieldName);
            }

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvVariables.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("VariableCategory");
            view.GroupDescriptions.Add(groupDescription);

            RowFilterControl = new RowFilterControl(this.DashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, (gadget as EpiDashboard.Gadgets.Analysis.DuplicatesListControl).DataFilters, true);
            RowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            panelFilters.Children.Add(RowFilterControl);



            #region Translation

            lblConfigExpandedTitleTxt.Text = DashboardSharedStrings.GADGET_CONFIG_TITLE_DUPLICATESLIST;
            tbtnVariables.Title = DashboardSharedStrings.GADGET_TABBUTTON_VARIABLES;
            tbtnVariables.Description = DashboardSharedStrings.GADGET_TABDESC_MEANS;
            tbtnDisplay.Title = DashboardSharedStrings.GADGET_TABBUTTON_DISPLAY;
            tbtnDisplay.Description = DashboardSharedStrings.GADGET_TABDESC_DISPLAY;
            tbtnFilters.Title = DashboardSharedStrings.GADGET_TABBUTTON_FILTERS;
            tbtnFilters.Description = DashboardSharedStrings.GADGET_TABDESC_FILTERS;

            tblockPanelVariablesTxt.Text = DashboardSharedStrings.GADGET_PANELHEADER_VARIABLES;
            tblockPanelDisplayTxt.Text = DashboardSharedStrings.GADGET_PANELHEADER_DISPLAY;
            tblockPanelDataFilterTxt.Text = DashboardSharedStrings.GADGET_PANELHEADER_DATA_FILTER;
            tblockAnyFilterGadgetOnlyTxt.Text = DashboardSharedStrings.GADGET_FILTER_GADGET_ONLY;
            tblockTitleNDescSubheaderTxt.Text = DashboardSharedStrings.GADGET_PANELSUBHEADER_TITLENDESC;
            tblockTitleTxt.Text = DashboardSharedStrings.GADGET_GADET_TITLE;
            tblockDescTxt.Text = DashboardSharedStrings.GADGET_DESCRIPTION;
            tblockDimensionsTxt.Text = DashboardSharedStrings.GADGET_DIMENSIONS;
            tblockPanelOutputOptTxt.Text = DashboardSharedStrings.GADGET_OUTPUT_OPTIONS;
            checkboxUsePromptsTxt.Text = DashboardSharedStrings.GADGET_USE_FIELD_PROMPTS;
            checkboxListLabelsTxt.Text = DashboardSharedStrings.GADGET_DISPLAY_LIST_LABELS;
            checkboxColumnHeadersTxt.Text = DashboardSharedStrings.GADGET_SHOW_COLUMN_HEADINGS;
            checkboxShowNullsTxt.Text = DashboardSharedStrings.GADGET_SHOW_MISSING_REP;
            checkboxTabOrderTxt.Text = DashboardSharedStrings.EXPORT_SORT_BY_TAB_ORDER;
            tblockKeyDuplicateCheckingTxt.Text = DashboardSharedStrings.GADGET_KEY_DUPCHECKING;
            tblockAdditionalFieldsDisplayTxt.Text = DashboardSharedStrings.GADGET_ADDITIONAL_FIELDS_DISPLAY;
            checkboxLineColumnTxt.Text = DashboardSharedStrings.GADGET_SHOW_ROW_NUM;
            tblockMaxWidthTxt.Text = DashboardSharedStrings.GADGET_MAX_WIDTH;
            tblockMaxHeightTxt.Text = DashboardSharedStrings.GADGET_MAX_HEIGHT;
            btnOKTxt.Text = DashboardSharedStrings.BUTTON_OK;
            btnCancelTxt.Text = DashboardSharedStrings.BUTTON_CANCEL;

            #endregion // Translation

        }

        public bool HasSelectedFields
        {
            get
            {
                if (lvVariables.SelectedItems.Count > 0)
                {
                    return true;
                }
                return false;
            }
        }

        public DuplicatesListParameters Parameters { get; private set; }
        private List<Grid> StrataGridList { get; set; }
        private List<string> ColumnOrder { get; set; }

        /// <summary>
        /// Used to generate the list of variables and options for the GadgetParameters object
        /// </summary> 
        protected override void CreateInputVariableList()
        {
            // Set data filters!
            this.DataFilters = RowFilterControl.DataFilters;

            //Parameters.ColumnNames used for the "Additional Variables to Display"
            Parameters.ColumnNames = new List<string>();
            Parameters.KeyColumnNames = new List<string>();

            Parameters.SortVariables = new Dictionary<string, SortOrder>();

            Parameters.GadgetTitle = txtTitle.Text;
            Parameters.GadgetDescription = txtDesc.Text;

            double height = 0;
            double width = 0;

            bool success = double.TryParse(txtMaxHeight.Text, out height);
            if (success)
            {
                Parameters.Height = height;
            }

            success = double.TryParse(txtMaxWidth.Text, out width);
            if (success)
            {
                Parameters.Width = width;
            }

            List<string> listFields = new List<string>();

            if (lvVariables.SelectedItems.Count > 0)
            {
                foreach (FieldInfo fieldInfo in lvVariables.SelectedItems)
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
                if (!string.IsNullOrEmpty(field) && !Parameters.KeyColumnNames.Contains(field))
                {
                    Parameters.KeyColumnNames.Add(field);
                }
            }

            List<string> displayListFields = new List<string>();

            if (lvDisplayVariables.SelectedItems.Count > 0)
            {
                foreach (FieldInfo fieldInfo in lvDisplayVariables.SelectedItems)
                {
                    if (!string.IsNullOrEmpty(fieldInfo.Name))
                    {
                        displayListFields.Add(fieldInfo.Name);
                    }
                }
            }

            displayListFields.Sort();

            foreach (string field in displayListFields)
            {
                Parameters.ColumnNames.Add(field);
            }

            Parameters.SortColumnsByTabOrder = checkboxTabOrder.IsChecked.Value;
            Parameters.UseFieldPrompts = checkboxUsePrompts.IsChecked.Value;
            Parameters.ShowColumnHeadings = checkboxColumnHeaders.IsChecked.Value;
            Parameters.ShowLineColumn = checkboxLineColumn.IsChecked.Value;
            Parameters.ShowNullLabels = checkboxShowNulls.IsChecked.Value;
            Parameters.ShowCommentLegalLabels = checkboxListLabels.IsChecked.Value;

            if (lbxSortOrder.Items.Count > 0)
            {
                foreach (string item in lbxSortOrder.Items)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        //string baseStr = item;

                        //if (baseStr.EndsWith("(ascending)"))
                        //{
                        //    baseStr = "[" + baseStr.Remove(baseStr.Length - 12) + "] ASC";
                        //}
                        //if (baseStr.EndsWith("(descending)"))
                        //{
                        //    baseStr = "[" + baseStr.Remove(baseStr.Length - 13) + "] DESC";
                        //}
                        //inputVariableList.Add(baseStr, "sortfield");

                        string baseStr = item;

                        if (baseStr.EndsWith("(ascending)"))
                        {
                            baseStr = baseStr.Remove(baseStr.Length - 12);
                            Parameters.SortVariables.Add(baseStr, SortOrder.Ascending);
                        }
                        if (baseStr.EndsWith("(descending)"))
                        {
                            baseStr = baseStr.Remove(baseStr.Length - 13);
                            Parameters.SortVariables.Add(baseStr, SortOrder.Descending);
                        }
                    }
                }
            }

            if (cmbGroupField.SelectedIndex >= 0)
            {
                if (!string.IsNullOrEmpty(cmbGroupField.SelectedItem.ToString()))
                {
                    Parameters.PrimaryGroupField = cmbGroupField.SelectedItem.ToString();
                }
            }

            if (cmbSecondaryGroupField.SelectedIndex >= 0)
            {
                if (!string.IsNullOrEmpty(cmbSecondaryGroupField.SelectedItem.ToString()))
                {
                    Parameters.SecondaryGroupField = cmbSecondaryGroupField.SelectedItem.ToString();
                }
            }

            if (StrataGridList.Count >= 1) 
            {
                Grid grid = StrataGridList[0];
                SortedDictionary<int, string> sortColumnDictionary = new SortedDictionary<int, string>();

                foreach (UIElement element in grid.Children)
                {
                    if (Grid.GetRow(element) == 0 && element is TextBlock)
                    {
                        TextBlock txtColumnName = element as TextBlock;
                        //columnOrder.Add(txtColumnName.Text);
                        sortColumnDictionary.Add(Grid.GetColumn(element), txtColumnName.Text);
                    }
                }

                ColumnOrder = new List<string>();
                foreach (KeyValuePair<int, string> kvp in sortColumnDictionary)
                {
                    ColumnOrder.Add(kvp.Value);
                }

                if (ColumnOrder.Count == listFields.Count || ColumnOrder.Count == (listFields.Count + 1))
                {
                    bool same = true;
                    foreach (string s in listFields)
                    {
                        if (!ColumnOrder.Contains(s))
                        {
                            same = false;
                        }
                    }

                    if (same)
                    {
                        WordBuilder wb = new WordBuilder("^");
                        foreach (string s in ColumnOrder)
                        {
                            wb.Add(s);
                        }

                        //inputVariableList.Add("customusercolumnsort", wb.ToString());
                        Parameters.CustomUserColumnSort.Add(wb.ToString());
                    }
                    else
                    {
                        ColumnOrder = new List<string>();
                    }
                }
                else
                {
                    ColumnOrder = new List<string>();
                }
            }
            
            //Parameters.InputVariableList = inputVariableList;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            lvVariables.MaxHeight = this.ActualHeight - 200;
            lvDisplayVariables.MaxHeight = this.ActualHeight - 200;

            //Dictionary<string, string> inputVariableList = Parameters.InputVariableList;

            foreach (string s in Parameters.KeyColumnNames)
            {
                foreach (FieldInfo info in lvVariables.Items)
                {
                    if (info.Name == s)
                    {
                        lvVariables.SelectedItems.Add(info);
                        continue;
                    }
                }
            }

            foreach (string s in Parameters.ColumnNames)
            {
                foreach (FieldInfo info in lvVariables.Items)
                {
                    if (info.Name == s)
                    {
                        lvDisplayVariables.SelectedItems.Add(info);
                        continue;
                    }
                }
            }

            if (!String.IsNullOrEmpty(Parameters.Height.ToString()))
            {
                txtMaxHeight.Text = Parameters.Height.ToString();
            }
            if (!String.IsNullOrEmpty(Parameters.Width.ToString()))
            {
                txtMaxWidth.Text = Parameters.Width.ToString();
            }

            svVariables.Height = svVariables.Height + (Math.Max(0, System.Windows.SystemParameters.PrimaryScreenHeight - 768.0));

            lbxAvailableVariables.Height = lbxAvailableVariables.Height + (System.Windows.SystemParameters.PrimaryScreenHeight - 868.0);
            lbxSortOrder.Height = lbxSortOrder.Height + (System.Windows.SystemParameters.PrimaryScreenHeight - 868.0);
            
            checkboxTabOrder.IsChecked = Parameters.SortColumnsByTabOrder;
            checkboxUsePrompts.IsChecked = Parameters.UseFieldPrompts;
            checkboxColumnHeaders.IsChecked = Parameters.ShowColumnHeadings;
            checkboxLineColumn.IsChecked = Parameters.ShowLineColumn;
            checkboxShowNulls.IsChecked = Parameters.ShowNullLabels;
            checkboxListLabels.IsChecked = Parameters.ShowCommentLegalLabels;

            cmbGroupField.SelectedItem = Parameters.PrimaryGroupField;
            cmbSecondaryGroupField.SelectedItem = Parameters.SecondaryGroupField;

            foreach (KeyValuePair<string, SortOrder> kvp in Parameters.SortVariables)
            {
                string suffix = String.Empty;
                switch (kvp.Value)
                {
                    case SortOrder.Descending:
                        suffix = " (descending)";
                        break;
                    case SortOrder.Ascending:
                        suffix = " (ascending)";
                        break;
                }

                lbxAvailableVariables.Items.Remove(kvp.Key);
                lbxSortOrder.Items.Add(kvp.Key + suffix);
            }

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

        private void lbxAvailableVariables_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lbxAvailableVariables.SelectedItems.Count == 1)
            {
                string fieldName = lbxAvailableVariables.SelectedItem.ToString();

                lbxAvailableVariables.Items.Remove(fieldName);

                string method = " (ascending)";
                if (cmbSortMethod.SelectedIndex == 1)
                {
                    method = " (descending)";
                }
                lbxSortOrder.Items.Add(fieldName + method);
            }
        }

        private void lbxSortOrder_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lbxSortOrder.SelectedItems.Count == 1)
            {
                string fieldName = lbxSortOrder.SelectedItem.ToString();

                lbxSortOrder.Items.Remove(fieldName);

                string originalFieldName = fieldName.Replace(" (ascending)", String.Empty).Replace(" (descending)", String.Empty);

                lbxAvailableVariables.Items.Add(originalFieldName);

                List<string> items = new List<string>();

                foreach (string item in lbxAvailableVariables.Items)
                {
                    items.Add(item);
                }

                items.Sort();

                lbxAvailableVariables.Items.Clear();

                foreach (string item in items)
                {
                    lbxAvailableVariables.Items.Add(item);
                }
            }
        }

        private void lvVariables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
 
        }
    }
}
