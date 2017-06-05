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
    public partial class RatesProperties : GadgetPropertiesPanelBase
    {
        private const int MAX_ROW_LIMIT = 2000;

        bool _numerDistinct = true;
        bool _denomDistinct = true;
        DataFilters _numerFilter = null;
        DataFilters _denomFilter = null;

        public RatesProperties(
            DashboardHelper dashboardHelper,
            IGadget gadget,
            RatesParameters parameters,
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

            _numerFilter = new DataFilters(DashboardHelper);
            _denomFilter = new DataFilters(DashboardHelper);

            List<FieldInfo> items = new List<FieldInfo>();
            List<string> fields = new List<string>();
            fields.Add(string.Empty);
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

            cmbNumeratorField.ItemsSource = items;
            cmbDenominatorField.ItemsSource = items;

            cmbGroupField.ItemsSource = fields;
            cmbSecondaryGroupField.ItemsSource = fields;

            foreach (string fieldName in fields)
            {
                lbxAvailableVariables.Items.Add(fieldName);
            }

            //nk  CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvNumerator.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("VariableCategory");
            //nk  view.GroupDescriptions.Add(groupDescription);

            RowFilterControl = new RowFilterControl(this.DashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, (gadget as RatesControl).DataFilters, true);
            RowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            panelFilters.Children.Add(RowFilterControl);

            #region Translation

            lblConfigExpandedTitle.Content = DashboardSharedStrings.GADGET_CONFIG_TITLE_RATES;
            tbtnVariables.Title = "Define Rate";
            tbtnVariables.Description = "Create rules to define a rate.";
            tbtnSorting.Title = DashboardSharedStrings.GADGET_TABBUTTON_SORTING;
            tbtnSorting.Description = DashboardSharedStrings.GADGET_TABDESC_SORTING;
            tbtnDisplay.Title = DashboardSharedStrings.GADGET_TABBUTTON_DISPLAY;
            tbtnDisplay.Description = DashboardSharedStrings.GADGET_TABDESC_DISPLAY;
            tbtnFilters.Title = DashboardSharedStrings.GADGET_TABBUTTON_FILTERS;
            tbtnFilters.Description = DashboardSharedStrings.GADGET_TABDESC_FILTERS;

            tblockPanelVariables.Content = "Define Rate";
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
            tblockVariableDenominator.Content = DashboardSharedStrings.GADGET_VARIABLES_DENOMINATOR;
            tblockGroupby.Content = DashboardSharedStrings.GADGET_GROUP_BY;
            tblockSubGroupBy.Content = DashboardSharedStrings.GADGET_SUBGROUP_BY;
            tblockAvailableVariables.Content = DashboardSharedStrings.GADGET_AVAILABLE_VARIABLES;
            tblockSortOrder.Content = DashboardSharedStrings.GADGET_SORT_ORDER;
            tblockDimensions.Content = DashboardSharedStrings.GADGET_DIMENSIONS;
            tblockTitleNDescSubheader.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_TITLENDESC;
            tblockMaxWidth.Text = DashboardSharedStrings.GADGET_MAX_WIDTH;
            btnOK.Content = DashboardSharedStrings.BUTTON_OK;
            btnCancel.Content = DashboardSharedStrings.BUTTON_CANCEL;
            tblockMaxRows.Text = SharedStrings.DASHBOARD_OPTION_MAX_ROWS;
            #endregion // Translation
        }

        public bool HasSelectedFields
        {
            get
            {
                return _numerFilter != null && _denomFilter != null;
            }
        }

        public RatesParameters Parameters { get; private set; }
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

            Parameters.SortVariables = new Dictionary<string, SortOrder>();

            Parameters.GadgetTitle = txtTitle.Text;
            Parameters.GadgetDescription = txtDesc.Text;

            double height = 5000;
            double width = 0;
            int maxrows = 0;
            int maxColumnLength = 0;

            bool success = double.TryParse(txtMaxWidth.Text, out width);
            if (success)
            {
                Parameters.Width = width;
            }

            success = int.TryParse(txtMaxRows.Text, out maxrows);
            if (success)
            {
                Parameters.MaxRows = maxrows;
            }

            List<string> listFields = new List<string>();
            List<string> numerFilterFields = new List<string>();
            List<string> denomFilterFields = new List<string>();

            if (Parameters.NumerFilter != null)
            { 
                foreach (System.Data.DataRow numerRow in Parameters.NumerFilter.ConditionTable.Rows)
                {
                    string[] fragments = (numerRow["filter"]).ToString().Split(new char[] { '[', ']' });
                    if (fragments.Length == 3)
                    {
                        numerFilterFields.Add(fragments[1]);
                    }
                }
            }

            if (Parameters.DenomFilter != null)
            {
                foreach (System.Data.DataRow denomRow in Parameters.DenomFilter.ConditionTable.Rows)
                {
                    string[] fragments = (denomRow["filter"]).ToString().Split(new char[] { '[', ']' });
                    if (fragments.Length == 3)
                    {
                        denomFilterFields.Add(fragments[1]);
                    }
                }
            }

            numerFilterFields.AddRange(denomFilterFields.ToList<string>());
            listFields = numerFilterFields;

            listFields.Sort();
            if ((Gadget as RatesControl).IsHostedByEnter)
            {
                if (listFields.Contains("UniqueKey"))
                {
                    listFields.Remove("UniqueKey");
                }

                listFields.Add("UniqueKey");
            }

            foreach (string field in listFields)
            {
                Parameters.ColumnNames.Add(field);
            }

            Parameters.NumerDistinct = checkBoxNumberatorDistinct.IsChecked.Value;
            Parameters.DenomDistinct = checkBoxDenominatorDistinct.IsChecked.Value;

            if (lbxSortOrder.Items.Count > 0)
            {
                foreach (string item in lbxSortOrder.Items)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
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

            if (string.IsNullOrWhiteSpace(cmbRateMultiplier.Text) == false)
            {
                Parameters.RateMultiplierString = cmbRateMultiplier.Text;
            }
            else
            {
                Parameters.RateMultiplierString = "100";
            }

            if (cmbNumeratorField.SelectedIndex >= 0)
            {
                if (cmbNumeratorField.SelectedItem is RatesProperties.FieldInfo)
                {
                    Parameters.NumeratorField = ((RatesProperties.FieldInfo)cmbNumeratorField.SelectedItem).Name;
                }
            }

            if (cmbDenominatorField.SelectedIndex >= 0)
            {
                if (cmbDenominatorField.SelectedItem is RatesProperties.FieldInfo)
                {
                    Parameters.DenominatorField = ((RatesProperties.FieldInfo)cmbDenominatorField.SelectedItem).Name;
                }
            }

            if(string.IsNullOrWhiteSpace(Parameters.DenominatorField) == false )
            {
                Parameters.ColumnNames.Add(Parameters.DenominatorField);
            }

            if (string.IsNullOrWhiteSpace(Parameters.DenominatorField) == false)
            {
                Parameters.ColumnNames.Add(Parameters.DenominatorField);
            }

            if (cmbSelectNumeratorAggregateFunction.SelectedIndex >= 0)
            {
                Parameters.NumeratorAggregator = (string)((AggFxInfo)(cmbSelectNumeratorAggregateFunction.SelectedItem)).Keyword;
            }

            if (cmbSelectDenominatorAggregateFunction.SelectedIndex >= 0)
            {
                Parameters.DenominatorAggregator = (string)((AggFxInfo)(cmbSelectDenominatorAggregateFunction.SelectedItem)).Keyword;
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

            Parameters.DefaultColor = rctColorDefault.Fill.ToString();
            Parameters.UseDefaultColor = defaultColorOption.IsChecked == true;

            double doubleValue = -1;

            Parameters.Color_L1 = rctColor1.Fill.ToString();
            success = double.TryParse(rampEnd01.Text, out doubleValue);
            if (success) Parameters.HighValue_L1 = doubleValue;

            Parameters.Color_L2 = rctColor2.Fill.ToString();
            success = double.TryParse(rampStart02.Text, out doubleValue);
            if (success) Parameters.LowValue_L2 = doubleValue;
            success = double.TryParse(rampEnd02.Text, out doubleValue);
            if (success) Parameters.HighValue_L2 = doubleValue;

            Parameters.Color_L3 = rctColor3.Fill.ToString();
            success = double.TryParse(rampStart03.Text, out doubleValue);
            if (success) Parameters.LowValue_L3 = doubleValue;
            success = double.TryParse(rampEnd03.Text, out doubleValue);
            if (success) Parameters.HighValue_L3 = doubleValue;

            Parameters.Color_L4 = rctColor4.Fill.ToString();
            success = double.TryParse(rampStart04.Text, out doubleValue);
            if (success) Parameters.LowValue_L4 = doubleValue;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(Parameters.Width.ToString()))
            {
                txtMaxWidth.Text = Parameters.Width.ToString();
            }
            if (!String.IsNullOrEmpty(Parameters.MaxRows.ToString()) & Parameters.MaxRows != 0)
            {
                txtMaxRows.Text = Parameters.MaxRows.ToString();
            }

            checkBoxDenominatorDistinct.IsChecked = Parameters.DenomDistinct;
            checkBoxNumberatorDistinct.IsChecked = Parameters.NumerDistinct;

            if (!String.IsNullOrEmpty(Parameters.RateMultiplierString))
            {
                cmbRateMultiplier.Text = Parameters.RateMultiplierString;
            }

            if(Parameters.NumerFilter != null)
            {
                numeratorRule.Content = Parameters.NumerFilter.GenerateReadableDataFilterString();
            }

            if (Parameters.DenomFilter != null)
            {
                denominatorRule.Content = Parameters.DenomFilter.GenerateReadableDataFilterString();
            }

            cmbGroupField.SelectedItem = Parameters.PrimaryGroupField;
            cmbSecondaryGroupField.SelectedItem = Parameters.SecondaryGroupField;

            var numerFieldInfo = cmbNumeratorField.Items.Cast<FieldInfo>().
                Where(fieldInfo => String.Compare(fieldInfo.Name, Parameters.NumeratorField) == 0).
                ToList<FieldInfo>();
            if(numerFieldInfo.Count > 0 && numerFieldInfo[0] is FieldInfo)
            {
                cmbNumeratorField.SelectedItem = numerFieldInfo[0];
            }

            var denomFieldInfo = cmbDenominatorField.Items.Cast<FieldInfo>().
                Where(fieldInfo => String.Compare(fieldInfo.Name, Parameters.DenominatorField) == 0).
                ToList<FieldInfo>();
            if (denomFieldInfo.Count > 0 && denomFieldInfo[0] is FieldInfo)
            {
                cmbDenominatorField.SelectedItem = denomFieldInfo[0];
            }

            var numerAggregateFunction = cmbSelectNumeratorAggregateFunction.Items.Cast<AggFxInfo>().
                Where(aggFxInfo => String.Compare(aggFxInfo.Keyword, Parameters.NumeratorAggregator) == 0).
                ToList<AggFxInfo>();
            if (numerAggregateFunction.Count > 0 && numerAggregateFunction[0] is AggFxInfo)
            {
                cmbSelectNumeratorAggregateFunction.SelectedItem = numerAggregateFunction[0];
            }

            var denomAggregateFunction = cmbSelectDenominatorAggregateFunction.Items.Cast<AggFxInfo>().
                Where(aggFxInfo => String.Compare(aggFxInfo.Keyword, Parameters.DenominatorAggregator) == 0).
                ToList<AggFxInfo>();
            if (denomAggregateFunction.Count > 0 && denomAggregateFunction[0] is AggFxInfo)
            {
                cmbSelectDenominatorAggregateFunction.SelectedItem = denomAggregateFunction[0];
            }

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

            Color color;
            if (Parameters.DefaultColor != null)
            {
                color = (Color)ColorConverter.ConvertFromString(Parameters.DefaultColor);
                rctColorDefault.Fill = new SolidColorBrush(color);
            }

            if (Parameters.UseDefaultColor == true)
            {
                defaultColorOption.IsChecked = true;
                colorStack.Visibility = Visibility.Collapsed;
            }
            else
            {
                defaultColorOption.IsChecked = false;
                colorStack.Visibility = Visibility.Visible;
            }

            if (Parameters.DefaultColor != null)
            {
                color = (Color)ColorConverter.ConvertFromString(Parameters.Color_L1);
                rctColor1.Fill = new SolidColorBrush(color);
            }
            if (Parameters.DefaultColor != null)
            {
                color = (Color)ColorConverter.ConvertFromString(Parameters.Color_L2);
                rctColor2.Fill = new SolidColorBrush(color);
            }

            if (Parameters.DefaultColor != null)
            {
                color = (Color)ColorConverter.ConvertFromString(Parameters.Color_L3);
                rctColor3.Fill = new SolidColorBrush(color);
            }

            if (Parameters.DefaultColor != null)
            {
                color = (Color)ColorConverter.ConvertFromString(Parameters.Color_L4);
                rctColor4.Fill = new SolidColorBrush(color);
            }

            rampStart02.Text = Parameters.LowValue_L2.ToString().Replace("NaN", "");
            rampStart03.Text = Parameters.LowValue_L3.ToString().Replace("NaN", "");
            rampStart04.Text = Parameters.LowValue_L4.ToString().Replace("NaN", "");

            rampEnd01.Text = Parameters.HighValue_L1.ToString().Replace("NaN", "");
            rampEnd02.Text = Parameters.HighValue_L2.ToString().Replace("NaN", "");
            rampEnd03.Text = Parameters.HighValue_L3.ToString().Replace("NaN", "");
        }

        public class FieldInfo { public string Name { get; set; } public string DataType { get; set; } public VariableCategory VariableCategory { get; set; } }
        public class AggFxInfo {
            public string Name { get; set; }
            public string Keyword { get; set; }
            public string UseAsDefaultForType { get; set; }
            public string Description { get; set; }
            public AggFxInfo(string name, string keyword, string useAsDefaultForType, string description)
            {
                Name = name;
                Keyword = keyword;
                UseAsDefaultForType = useAsDefaultForType;
                Description = description;
            }
        }
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

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
        }

        private void btnNumeratorRule_Click(object sender, RoutedEventArgs e)
        {
            Dialogs.RowFilterDialog rowFilterDialog = new Dialogs.RowFilterDialog(this.DashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, Parameters.NumerFilter, true);
            System.Windows.Forms.DialogResult result = rowFilterDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                _numerFilter = rowFilterDialog.DataFilters;
                Parameters.NumerFilter = rowFilterDialog.DataFilters;
                numeratorRule.Content = _numerFilter.GenerateReadableDataFilterString();
            }
        }

        private void btnDenominatorRule_Click(object sender, RoutedEventArgs e)
        {
            Dialogs.RowFilterDialog rowFilterDialog = new Dialogs.RowFilterDialog(this.DashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, Parameters.DenomFilter, true);
            System.Windows.Forms.DialogResult result = rowFilterDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                _denomFilter = rowFilterDialog.DataFilters;
                Parameters.DenomFilter = rowFilterDialog.DataFilters;
                denominatorRule.Content = _denomFilter.GenerateReadableDataFilterString();
            }
        }
        private static List<AggFxInfo> GetAggFxInfo(FieldInfo fieldInfo)
        {
            List<AggFxInfo> items = new List<AggFxInfo>();

            string dataType = fieldInfo.DataType.ToLowerInvariant();
            if (dataType == "string" || dataType == "boolean" || dataType == "datetime")
            {
                items.Add(new AggFxInfo("Count", "count", "string,boolean,datetime", "Returns the number of all the values in the field. COUNT can be used with numeric, text, and boolean columns. Null values are ignored."));
            }
            else
            {
                items.Add(new AggFxInfo("Count", "count", "int,double,decimal,string,boolean,datetime", "Returns the number of all the values in the field. COUNT can be used with numeric, text, and boolean columns. Null values are ignored."));
                items.Add(new AggFxInfo("Sum", "sum", "int,double,decimal", "Returns the sum of all the values in the field. SUM can be used with numeric columns only. Null values are ignored."));
                items.Add(new AggFxInfo("Average", "avg", "int,double,decimal", ""));
                items.Add(new AggFxInfo("Minimum", "min", "int,double,decimal", ""));
                items.Add(new AggFxInfo("Maximum", "max", "int,double,decimal", ""));
                items.Add(new AggFxInfo("Statistical Standard Eviation", "int,double,decimal", "stdev", ""));
                items.Add(new AggFxInfo("Statistical Variance", "var", "int,double,decimal", ""));
            }

            return items;
        }
        private void cmbNumeratorField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox control = (ComboBox)sender;
            if (control.SelectedItem != null)
            {
                if (control.SelectedItem is FieldInfo)
                {
                    FieldInfo fieldInfo = (FieldInfo)control.SelectedItem;
                    Parameters.NumeratorField = fieldInfo.Name;
                    List<AggFxInfo> items = GetAggFxInfo(fieldInfo);

                    if (cmbSelectNumeratorAggregateFunction.ItemsSource == null)
                    {
                        cmbSelectNumeratorAggregateFunction.ItemsSource = items;
                    }
                    else
                    {
                        if (cmbSelectNumeratorAggregateFunction.Items.Count != items.Count)
                        {
                            object currentNumeratorSelected = cmbSelectNumeratorAggregateFunction.SelectedItem;
                            cmbSelectNumeratorAggregateFunction.ItemsSource = items;

                            if (currentNumeratorSelected is AggFxInfo)
                            {
                                var aggregateFunction = cmbSelectNumeratorAggregateFunction.Items.Cast<AggFxInfo>().
                                    Where(aggFxInfo => ((string)aggFxInfo.UseAsDefaultForType.ToLowerInvariant()).Contains(fieldInfo.DataType.ToLowerInvariant())).
                                    ToList<AggFxInfo>();

                                if (aggregateFunction.Count > 0 && aggregateFunction[0] is AggFxInfo)
                                {
                                    cmbSelectNumeratorAggregateFunction.SelectionChanged -= new System.Windows.Controls.SelectionChangedEventHandler(this.cmbNumeratorField_SelectionChanged);
                                    cmbSelectNumeratorAggregateFunction.SelectedItem = aggregateFunction[0];
                                    cmbSelectNumeratorAggregateFunction.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.cmbNumeratorField_SelectionChanged);
                                }
                            }
                        }
                    }
                }
            }
        }
        private void cmbDenominatorField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox control = (ComboBox)sender;
            if (control.SelectedItem != null)
            {
                if (control.SelectedItem is FieldInfo)
                {
                    FieldInfo fieldInfo = (FieldInfo)control.SelectedItem;
                    Parameters.DenominatorField = fieldInfo.Name;
                    List<AggFxInfo> items = GetAggFxInfo(fieldInfo);

                    if (cmbSelectDenominatorAggregateFunction.ItemsSource == null)
                    {
                        cmbSelectDenominatorAggregateFunction.ItemsSource = items;
                    }
                    else
                    {
                        if (cmbSelectDenominatorAggregateFunction.Items.Count != items.Count)
                        {
                            object currentDenominatorSelected = cmbSelectDenominatorAggregateFunction.SelectedItem;
                            cmbSelectDenominatorAggregateFunction.ItemsSource = items;

                            if (currentDenominatorSelected is AggFxInfo)
                            {
                                var aggregateFunction = cmbSelectDenominatorAggregateFunction.Items.Cast<AggFxInfo>().
                                    Where(aggFxInfo => ((string)aggFxInfo.UseAsDefaultForType.ToLowerInvariant()).Contains(fieldInfo.DataType.ToLowerInvariant())).
                                    ToList<AggFxInfo>();

                                if (aggregateFunction.Count > 0 && aggregateFunction[0] is AggFxInfo)
                                {
                                    cmbSelectDenominatorAggregateFunction.SelectionChanged -= new System.Windows.Controls.SelectionChangedEventHandler(this.cmbDenominatorField_SelectionChanged);
                                    cmbSelectDenominatorAggregateFunction.SelectedItem = aggregateFunction[0];
                                    cmbSelectDenominatorAggregateFunction.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.cmbDenominatorField_SelectionChanged);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void cmbSelectNumeratorAggregateFunction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox control = (ComboBox)sender;
            if (control.SelectedItem != null)
            {
                if (control.SelectedItem is AggFxInfo && cmbSelectNumeratorAggregateFunction.SelectedItem != null)
                {
                    Parameters.NumeratorAggregator = (string)((AggFxInfo)(cmbSelectNumeratorAggregateFunction.SelectedItem)).Keyword;
                    lblAggregateFunctionDefinition.Content = (string)((AggFxInfo)(cmbSelectNumeratorAggregateFunction.SelectedItem)).Description;
                }
            }
        }

        private void cmbSelectDenominatorAggregateFunction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox control = (ComboBox)sender;
            if (control.SelectedItem != null)
            {
                if (control.SelectedItem is AggFxInfo && cmbSelectDenominatorAggregateFunction.SelectedItem != null)
                {
                    Parameters.DenominatorAggregator = (string)((AggFxInfo)(cmbSelectDenominatorAggregateFunction.SelectedItem)).Keyword;
                    lblAggregateFunctionDefinitionDenominator.Content = (string)((AggFxInfo)(cmbSelectDenominatorAggregateFunction.SelectedItem)).Description;
                }
            }
        }

        private void rctSelectColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            System.Windows.Media.Color currentColor = ((SolidColorBrush)((System.Windows.Shapes.Rectangle)sender).Fill).Color;
            System.Drawing.Color initColor = System.Drawing.Color.FromArgb(255, currentColor.R, currentColor.G, currentColor.B);

            List<Int32> baseColors = new List<Int32>()
            {
                -32640,
                -128,
                -8323200,
                -16711808,
                -8323073,
                -16744193,
                -32576,
                -32513,

                -65536,
                -256,
                -8323328,
                -16711872,
                -16711681,
                -16744256,
                -8355648,
                -65281,

                -8372160,
                -32704,
                -16711936,
                -16744320,
                -16760704,
                -8355585,
                -8388544,
                -65408,

                -8388608,
                -32768,
                -16744448,
                -16744384,
                -16776961,
                -16777056,
                -8388480,
                -8388353,

                -12582912,
                -32768,
                -16744448,
                -16744384,
                -16776961,
                -16777056,
                -8388480,
                -8388353,

                -12582912,
                -8372224,
                -16760832,
                -16760768,
                -16777088,
                -16777152,
                -12582848,
                -12582784,

                -16777216,
                -8355840,
                -8355776,
                -8355712,
                -12550016,
                -4144960,
                -12582848,
                -1
            };

            Int32 initColorString = initColor.ToArgb();

            if (baseColors.Contains(initColorString))
            {
                dialog.Color = initColor;
            }

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ((System.Windows.Shapes.Rectangle)sender).Fill = new SolidColorBrush(Color.FromRgb(dialog.Color.R, dialog.Color.G, dialog.Color.B));
                //LayerProvider.UseCustomColors = true;

                Int32 colValue = dialog.Color.ToArgb();

                // LayerProvider.CustomColorsDictionary.Add(((System.Windows.Shapes.Rectangle)sender).Name, Color.FromRgb(dialog.Color.R, dialog.Color.G, dialog.Color.B));

                ((SolidColorBrush)((System.Windows.Shapes.Rectangle)sender).Fill).Color = Color.FromRgb(dialog.Color.R, dialog.Color.G, dialog.Color.B);


                if (((System.Windows.Shapes.Rectangle)sender).Tag is String && ((String)((System.Windows.Shapes.Rectangle)sender).Tag) == "Reset_Legend")
                {
                    //Reset_Legend();
                }
            }
        }
        private void rampValue_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((sender is System.Windows.Controls.TextBox) == false)
            {
                return;
            }

            System.Windows.Controls.TextBox textBox = ((System.Windows.Controls.TextBox)sender);
            string newText = textBox.Text;
            float newValue = float.NaN;

            string name = textBox.Name;

            //int classLevel = LayerProvider.ClassRangesDictionary.GetClassLevelWithKey(name);
            //ClassLimitType limit = LayerProvider.ClassRangesDictionary.GetLimitTypeWithKey(name);

            if (float.TryParse(newText, out newValue) == false)
            {
                System.Windows.Controls.TextBox found = (System.Windows.Controls.TextBox)this.FindName(name);
               // if (LayerProvider.ClassRangesDictionary.RangeDictionary.ContainsKey(name))
                {
                //    found.Text = LayerProvider.ClassRangesDictionary.RangeDictionary[name];
                }
                return;
            }

            //UpdateClassRangesDictionary_FromControls();

            //if (IsMissingLimitValue())
            {
                return;
            }

            //List<ClassLimits> limits = LayerProvider.ClassRangesDictionary.GetLimitValues();

            //AdjustClassBreaks(limits, newValue, classLevel, limit);

            //LayerProvider.ClassRangesDictionary.SetRangesDictionary(limits);

            //foreach (KeyValuePair<string, string> kvp in LayerProvider.ClassRangesDictionary.RangeDictionary)
            {
                //System.Windows.Controls.TextBox found = (System.Windows.Controls.TextBox)this.FindName(kvp.Key);
                //found.Text = kvp.Value;
            }

            //PropertyChanged_EnableDisable();
        }

        private void CheckBox_DefaultColor_Click(object sender, RoutedEventArgs e)
        {
            colorStack.Visibility = ((CheckBox)sender).IsChecked == true ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    [ValueConversion(typeof(double), typeof(String))]
    public class RateToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value is TextBlock)
            {
                try
                {
                    TextBlock textBlock = (TextBlock)value;
                    System.Data.DataRowView rowView = (System.Data.DataRowView)textBlock.BindingGroup.Items[0];
                    return rowView.Row["hexColor"].ToString();
                }
                catch { }
            }

            return "#FFFFFF";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
