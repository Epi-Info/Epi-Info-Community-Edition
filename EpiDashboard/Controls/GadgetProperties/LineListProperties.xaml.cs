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
    /// Interaction logic for LineListProperties.xaml
    /// </summary>
    public partial class LineListProperties : GadgetPropertiesPanelBase
    {
        private const int MAX_ROW_LIMIT = 2000;
        public LineListProperties(
            DashboardHelper dashboardHelper,
            IGadget gadget,
            LineListParameters parameters,
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
            fields.Add(string.Empty);

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

            if (DashboardHelper.IsUsingEpiProject)
            {
                for (int i = 0; i < DashboardHelper.View.Pages.Count; i++)
                {
                    items.Add(new FieldInfo()
                    {
                        Name = "Page " + (i + 1).ToString(),
                        DataType = String.Empty,
                        VariableCategory = VariableCategory.Page
                    });
                }
            }

            fields.Sort();

            lvVariables.ItemsSource = items;
            cmbGroupField.ItemsSource = fields;
            cmbSecondaryGroupField.ItemsSource = fields;

            foreach (string fieldName in fields)
            {
                lbxAvailableVariables.Items.Add(fieldName);
            }

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvVariables.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("VariableCategory");
            view.GroupDescriptions.Add(groupDescription);

            RowFilterControl = new RowFilterControl(this.DashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, (gadget as LineListControl).DataFilters, true);
            RowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            panelFilters.Children.Add(RowFilterControl);

            #region Translation

            lblConfigExpandedTitle.Content = DashboardSharedStrings.GADGET_CONFIG_TITLE_LINE_LIST;
            tbtnVariables.Title = DashboardSharedStrings.GADGET_TABBUTTON_VARIABLES;
            tbtnVariables.Description = DashboardSharedStrings.GADGET_TABDESC_LINELIST;
            tbtnSorting.Title = DashboardSharedStrings.GADGET_TABBUTTON_SORTING;
            tbtnSorting.Description = DashboardSharedStrings.GADGET_TABDESC_SORTING;
            tbtnDisplay.Title = DashboardSharedStrings.GADGET_TABBUTTON_DISPLAY;
            tbtnDisplay.Description = DashboardSharedStrings.GADGET_TABDESC_DISPLAY;
            tbtnFilters.Title = DashboardSharedStrings.GADGET_TABBUTTON_FILTERS;
            tbtnFilters.Description = DashboardSharedStrings.GADGET_TABDESC_FILTERS;
            tblockPanelVariables.Content = DashboardSharedStrings.GADGET_PANELHEADER_VARIABLES;
            tblockPanelVariables.Content = DashboardSharedStrings.GADGET_PANELHEADER_VARIABLES;
            tblockPanelDisplay.Content = DashboardSharedStrings.GADGET_PANELHEADER_DISPLAY;
            tblockPanelSorting.Content = DashboardSharedStrings.GADGET_PANELHEADER_SORTING;
            tblockPanelDataFilter.Content = DashboardSharedStrings.GADGET_PANELHEADER_DATA_FILTER;
            tblockGroupingSubheader.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_GROUPING;
            tblockSortingSubheader.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_SORTING;
            tblockSortMethod.Content = DashboardSharedStrings.GADGET_SORT_METHOD;
            tblockTitle.Content = DashboardSharedStrings.GADGET_GADET_TITLE;
            tblockDesc.Content = DashboardSharedStrings.GADGET_DESCRIPTION;
            tblockPanelOutputOpt.Content = DashboardSharedStrings.GADGET_OUTPUT_OPTIONS;
            tblockAnyFilterGadgetOnly.Content = DashboardSharedStrings.GADGET_FILTER_GADGET_ONLY;
            tblockVariableToDisplay.Content = DashboardSharedStrings.GADGET_VARIABLES_TO_DISPLAY;
            tblockGroupby.Content = DashboardSharedStrings.GADGET_GROUP_BY;
            tblockSubGroupBy.Content = DashboardSharedStrings.GADGET_SUBGROUP_BY;
            tblockAvailableVariables.Content = DashboardSharedStrings.GADGET_AVAILABLE_VARIABLES;
            tblockSortOrder.Content = DashboardSharedStrings.GADGET_SORT_ORDER;
            tblockDimensions.Content = DashboardSharedStrings.GADGET_DIMENSIONS;
            checkboxTabOrderTxt.Text = DashboardSharedStrings.EXPORT_SORT_BY_TAB_ORDER;
            checkboxUsePromptsTxt.Text = DashboardSharedStrings.GADGET_USE_FIELD_PROMPTS;
            checkboxLineColumnTxt.Text = DashboardSharedStrings.GADGET_SHOW_LINE_COLUMN;
            checkboxColumnHeadersTxt.Text = DashboardSharedStrings.GADGET_SHOW_COLUMN_HEADINGS;
            checkboxShowNullsTxt.Text = DashboardSharedStrings.GADGET_SHOW_MISSING_REP;
            tblockTitleNDescSubheader.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_TITLENDESC;
            checkboxListLabelsTxt.Text = DashboardSharedStrings.GADGET_DISPLAY_LIST_LABELS;
            tblockMaxWidth.Text = DashboardSharedStrings.GADGET_MAX_WIDTH;
            tblockMaxHeight.Text = DashboardSharedStrings.GADGET_MAX_HEIGHT;
            btnOK.Content = DashboardSharedStrings.BUTTON_OK;
            btnCancel.Content = DashboardSharedStrings.BUTTON_CANCEL;

            tblockMaxVarNameLength.Text = SharedStrings.DASHBOARD_MAX_LENGTH;
            tblockMaxRows.Text = SharedStrings.DASHBOARD_OPTION_MAX_ROWS;
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

        public LineListParameters Parameters { get; private set; }
        private List<Grid> StrataGridList { get; set; }
        private List<string> ColumnOrder { get; set; }

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

            double height = 0;
            double width = 0;
            int maxrows = 0;
            int maxColumnLength = 0;

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

            success = int.TryParse(txtMaxVarNameLength.Text, out maxColumnLength);
            if (success)
            {
                if (maxColumnLength > 64)
                    maxColumnLength = 64;
                Parameters.MaxColumnLength = maxColumnLength;
            }

            success = int.TryParse(txtMaxRows.Text, out maxrows);
            if (success)
            {
                Parameters.MaxRows = maxrows;
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
            if ((Gadget as LineListControl).IsHostedByEnter)
            {
                //if (dashboardHelper.IsUsingEpiProject && checkboxAllowUpdates.IsChecked == true)
                //{
                if (listFields.Contains("UniqueKey"))
                    listFields.Remove("UniqueKey");
                listFields.Add("UniqueKey");
                //}
            }

            foreach (string field in listFields)
            {
                Parameters.ColumnNames.Add(field);
            }

            Parameters.SortColumnsByTabOrder = checkboxTabOrder.IsChecked.Value;
            Parameters.UsePromptsForColumnNames = checkboxUsePrompts.IsChecked.Value;
            Parameters.ShowColumnHeadings = checkboxColumnHeaders.IsChecked.Value;
            Parameters.ShowLineColumn = checkboxLineColumn.IsChecked.Value;
            Parameters.ShowNullLabels = checkboxShowNulls.IsChecked.Value;
            Parameters.ShowCommentLegalLabels = checkboxListLabels.IsChecked.Value;

            if (checkboxUsePrompts.IsChecked == true)
            {
                inputVariableList.Add("usepromptsforcolumnnames", "true");
            }
            else
            {
                inputVariableList.Add("usepromptsforcolumnnames", "false");
            }

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
                Parameters.PrimaryGroupField = cmbGroupField.SelectedItem.ToString();
            }

            if (cmbSecondaryGroupField.SelectedIndex >= 0)
            {
                Parameters.SecondaryGroupField = cmbSecondaryGroupField.SelectedItem.ToString();
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

                        inputVariableList.Add("customusercolumnsort", wb.ToString());
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

            Parameters.InputVariableList = inputVariableList;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            lvVariables.MaxHeight = this.ActualHeight - 200;

            //Dictionary<string, string> inputVariableList = Parameters.InputVariableList;

            foreach (string s in Parameters.ColumnNames)
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

            LineListControl llc = (LineListControl)this.Gadget;
            StackPanel sp = llc.panelMain;
            UIElementCollection uiec = sp.Children;
            DataGrid uie = (DataGrid)uiec[0];
            Dictionary<int, string> columnswithindexes = new Dictionary<int, string>();
            foreach (System.Windows.Controls.DataGridTextColumn dgtc in uie.Columns)
            {
                if (dgtc.DisplayIndex == 0)
                    continue;
                columnswithindexes.Add(dgtc.DisplayIndex, dgtc.Header.ToString());
            }
            for (int i = 1; i < columnswithindexes.Count + 1; i++)
            {
                ColumnOrder.Add(columnswithindexes[i]);
            }

            if (!String.IsNullOrEmpty(Parameters.Height.ToString()))
            {
                txtMaxHeight.Text = Parameters.Height.ToString();
            }
            if (!String.IsNullOrEmpty(Parameters.Width.ToString()))
            {
                txtMaxWidth.Text = Parameters.Width.ToString();
            }
            if (!String.IsNullOrEmpty(Parameters.MaxColumnLength.ToString()) & Parameters.MaxColumnLength != 0)
            {
                txtMaxVarNameLength.Text = Parameters.MaxColumnLength.ToString();
            }
            if (!String.IsNullOrEmpty(Parameters.MaxRows.ToString()) & Parameters.MaxRows != 0)
            {
                txtMaxRows.Text = Parameters.MaxRows.ToString();
            }

            //lbxAvailableVariables.Height = lbxAvailableVariables.Height + (System.Windows.SystemParameters.PrimaryScreenHeight - 868.0);
            //lbxSortOrder.Height = lbxSortOrder.Height + (System.Windows.SystemParameters.PrimaryScreenHeight - 868.0);

            checkboxTabOrder.IsChecked = Parameters.SortColumnsByTabOrder;
            checkboxUsePrompts.IsChecked = Parameters.UsePromptsForColumnNames;
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

        private void txtMaxVarNameLength_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Util.IsWholeNumber(e.Text);
            base.OnPreviewTextInput(e);
        }

        private void txtMaxRows_TextChanged(object sender, TextChangedEventArgs e)
        {
            int rows = 0;

            int.TryParse(txtMaxRows.Text, out rows);

            if (rows > MAX_ROW_LIMIT)
            {
                rows = MAX_ROW_LIMIT;
                txtMaxRows.Text = rows.ToString();
            }

        }
        private void txtWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            double thisWidth = 0;
            double.TryParse(txtMaxWidth.Text, out thisWidth);
            if (thisWidth > System.Windows.SystemParameters.PrimaryScreenWidth * 2)
            {
                txtMaxWidth.Text = (System.Windows.SystemParameters.PrimaryScreenWidth * 2).ToString();
            }
        }

        private void txtHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            double thisHeight = 0;
            double.TryParse(txtMaxHeight.Text, out thisHeight);
            if (thisHeight > System.Windows.SystemParameters.PrimaryScreenHeight * 2)
            {
                txtMaxHeight.Text = (System.Windows.SystemParameters.PrimaryScreenHeight * 2).ToString();
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

        }

    }
}
