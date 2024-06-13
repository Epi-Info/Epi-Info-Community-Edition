using System;
using System.Collections.Generic;
using System.Data;
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
using Microsoft.Windows.Controls;
using Epi;
using Epi.Core;
using Epi.Data;
using Epi.Fields;
using EpiDashboard;

namespace EpiDashboard.Controls
{
    /// <summary>
    /// Interaction logic for DataFilterControl.xaml
    /// </summary>
    public partial class DataFilterControl : UserControl
    {
        #region Private Members
        private DashboardHelper dashboardHelper;
        private double selectionGridHeight;
        private double guidedButtonsGridHeight;
        #endregion // Private Members

        public event EventHandler SelectionCriteriaChanged;

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view to attach</param>
        /// <param name="db">The database to attach</param>
        /// <param name="dashboardHelper">The dashboard helper to attach</param>
        public DataFilterControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.dashboardHelper = dashboardHelper;
            //txtTitle.RenderTransform = new RotateTransform(270);
            FillSelectionComboboxes();

            selectionGridHeight = grdSelectionProperties.Height;
            guidedButtonsGridHeight = grdGuidedModeButtons.Height;

            if (dashboardHelper.UseAdvancedUserDataFilter)
            {
                pnlAdvancedMode.Visibility = Visibility.Visible;
                txtAdvancedFilter.Text = dashboardHelper.AdvancedUserDataFilter;
                SetAdvancedFilterMode();
                ApplyAdvancedModeFilter();
            }
            else
            {
                pnlAdvancedMode.Visibility = Visibility.Collapsed;
                txtAdvancedFilter.Text = string.Empty;
                SetGuidedFilterMode();
            }

            UpdateFilterConditions();
            imgClose.MouseUp += new MouseButtonEventHandler(imgClose_MouseUp);

            if (!dashboardHelper.IsUsingEpiProject)
            {
                panelAdvanced.Visibility = Visibility.Collapsed;
            }
            
            #region Translation

            tbkTheValueOf.Text = DashboardSharedStrings.DASHBOARD_FILTER_VALUEOF;
            lblFieldName.Text = DashboardSharedStrings.GADGET_FIELDNAME;
            lblOperator.Text = DashboardSharedStrings.GADGET_OPERATOR;
            txtValue.Text = DashboardSharedStrings.GADGET_VALUE;
            txtAnd.Text = DashboardSharedStrings.GADGET_AND;
            btnNewCondition.Content = DashboardSharedStrings.GADGET_ADDFILTER;
            lblConditions.Text = DashboardSharedStrings.GADGET_DATA_FILTERS;
            mnuAddWithAnd.Header = DashboardSharedStrings.GADGET_ADD_WITH_AND_OPERATOR;
            mnuAddWithOr.Header = DashboardSharedStrings.GADGET_ADD_WITH_OR_OPERATOR;
            lblRecordProcessScope.Text = DashboardSharedStrings.GADGET_PROCESS_SCOPE;
            btnRemoveCondition.Content=DashboardSharedStrings.GADGET_REMOVE_SELECTED;
            btnClearConditions.Content=DashboardSharedStrings.GADGET_CLEAR_ALL;
            btnAdvancedMode.Content=DashboardSharedStrings.GADGET_ADVANCED_MODE;
            lblAdvancedFilterMode.Text =DashboardSharedStrings.GADGET_ADVANCED_MODE;
            tblockAdvancedInstruct.Text = DashboardSharedStrings.GADGET_DESIRED_DATA_FILTER;
            lblAdvancedNumeric.Text = DashboardSharedStrings.GADGET_ADVANCED_FILTER_NUMERIC +  "  (AGE >= 15) AND (AGE <= 45)";
            lblAdvancedText.Text = DashboardSharedStrings.GADGET_ADVANCED_FILTER_TEXT + "  (LastName LIKE '%sen') OR (LastName = 'Smith')";
            lblAdvancedDate.Text = DashboardSharedStrings.GADGET_ADVANCED_FILTER_DATE + "  (DOB >= #01/01/2000#) AND (DOB <= #12/31/2000 23:59:59#)";
            lblAdvancedBool.Text = DashboardSharedStrings.GADGET_ADVANCED_FILTER_BOOLEAN + "  (ILL = true)";
            txtAnd.Text = DashboardSharedStrings.AND;
            btnApplyAdvancedFilter.Content = DashboardSharedStrings.GADGET_APPLY_ADVANCED_FILTER;
            btnGuidedMode.Content=DashboardSharedStrings.GADGET_GUIDED_MODE;
            #endregion //Translation

        }
        #endregion // Constructors

        #region Private Properties
        /// <summary>
        /// Gets the View associated with this gadget
        /// </summary>
        private View View
        {
            get
            {
                return this.dashboardHelper.View;
            }
        }

        /// <summary>
        /// Gets the database driver associated with this gadget
        /// </summary>
        private IDbDriver Database
        {
            get
            {
                return this.dashboardHelper.Database;
            }
        }
        #endregion // Private Properties

        #region Public Methods

        /// <summary>
        /// Updates the variable names available in the gadget's properties
        /// </summary>
        public void UpdateVariableNames()
        {
            FillSelectionComboboxes(true);
        }

        /// <summary>
        /// Sets the control to 'Expanded' mode
        /// </summary>
        public void SetExpanded()
        {
            if (pnlAdvancedMode.Visibility == System.Windows.Visibility.Visible)
            {
                txtAdvancedFilter.IsEnabled = true;
            }
        }

        /// <summary>
        /// Sets the control to 'Collapsed' mode
        /// </summary>
        public void SetCollapsed()
        {
            if (pnlAdvancedMode.Visibility == System.Windows.Visibility.Visible)
            {
                txtAdvancedFilter.IsEnabled = false;
            }
        }

        public void MinimizeGadget()
        {
            ConfigGrid.Height = 50;
            //triangleCollapsed = true;
        }

        public bool IsClosable
        {
            set
            {
                if (value)
                    imgClose.Visibility = System.Windows.Visibility.Visible;
                else
                    imgClose.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
        #endregion // Public Methods

        #region Event Handlers
        /// <summary>
        /// Handles the MouseUp event for imgClose
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        public void imgClose_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// PreviewTextInput event
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void tbxNumericValue_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !ValidNumberChar(e.Text);

            base.OnPreviewTextInput(e);
        }

        /// <summary>
        /// Handles the Click event for the add new filter button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnNewCondition_Click(object sender, RoutedEventArgs e)
        {
            if (cbxFieldName.SelectedIndex == -1)
            {
                return;
            }

            if (dgFilters.Items.Count == 0)
            {
                AddCondition(ConditionJoinType.And);
                return;
            }

            if (btnNewCondition.ContextMenu != null)
            {
                btnNewCondition.ContextMenu.PlacementTarget = btnNewCondition;
                btnNewCondition.ContextMenu.IsOpen = true;
            }

            e.Handled = true;
            return;
        }

        /// <summary>
        /// Handles the TextChanged event for the advanced filter text box
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void txtAdvancedFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnApplyAdvancedFilter.IsEnabled = true;
            txtAdvancedFilter.Foreground = this.Resources["normalColor"] as Brush;
            txtAdvancedFilterStatus.Text = "Filter text has been changed. The previous filter is in effect until the new filter has been applied.";
        }

        /// <summary>
        /// Handles the Click event for the advanced mode button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnAdvancedMode_Click(object sender, RoutedEventArgs e)
        {
            SwapFilterMode();
        }

        /// <summary>
        /// Handles the Click event for the apply advanced filter button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnApplyAdvancedFilter_Click(object sender, RoutedEventArgs e)
        {
            ApplyAdvancedModeFilter();
        }

        /// <summary>
        /// Handles the SelectionChanged event for the field name combo box
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void cbxFieldName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbxOperator.Items.Clear();
            cbxValue.Items.Clear();

            if (cbxFieldName.SelectedIndex <= -1)
            {
                return;
            }

            string columnName = cbxFieldName.SelectedItem.ToString();
            Field field = null;
            string columnType = string.Empty;

            foreach (DataRow fieldRow in dashboardHelper.FieldTable.Rows)
            {
                if (fieldRow["columnname"].Equals(columnName))
                {
                    columnType = fieldRow["datatype"].ToString();
                    if (fieldRow["epifieldtype"] is Field)
                    {
                        field = fieldRow["epifieldtype"] as Field;
                    }
                    break;
                }
            }

            if (field != null)
            {
                Configuration config = dashboardHelper.Config;
                cbxValue.IsEnabled = true;
                cbxOperator.IsEnabled = true;

                if (field is IDataField)
                {
                    FillOperatorValues(field);
                    ShowValueSelector(field);
                }

                if (field is TableBasedDropDownField)
                {
                    //DataTable dataTable = db.Select(db.CreateQuery("SELECT " + field.Name + " " + view.FromViewSQL));
                    DataTable dataTable = ((TableBasedDropDownField)field).GetSourceData();
                    Dictionary<string, string> fieldValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    if (dataTable != null)
                    {
                        if (field is DDLFieldOfLegalValues)
                        {
                            foreach (System.Data.DataRow row in dataTable.Rows)
                            {
                                string codeColumnName = ((TableBasedDropDownField)field).CodeColumnName.Trim();
                                if (!string.IsNullOrEmpty(codeColumnName))
                                {
                                    string Key = row[0].ToString();
                                    if (!fieldValues.ContainsKey(Key))
                                    {
                                        fieldValues.Add(Key, Key);
                                    }
                                }
                            }
                        }
                        else if (field is DDLFieldOfCodes)
                        {
                            foreach (System.Data.DataRow row in dataTable.Rows)
                            {
                                string codeColumnName = ((DDLFieldOfCodes)field).TextColumnName.Trim();
                                if (!string.IsNullOrEmpty(codeColumnName))
                                {
                                    string Key = row[codeColumnName].ToString();
                                    if (!fieldValues.ContainsKey(Key))
                                    {
                                        fieldValues.Add(Key, Key);
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (System.Data.DataRow row in dataTable.Rows)
                            {
                                string codeColumnName = ((TableBasedDropDownField)field).TextColumnName.Trim();
                                if (!string.IsNullOrEmpty(codeColumnName))
                                {
                                    string Key = row[codeColumnName].ToString();
                                    int dash = Key.IndexOf('-');
                                    Key = Key.Substring(0, dash);
                                    if (!fieldValues.ContainsKey(Key))
                                    {
                                        fieldValues.Add(Key, Key);
                                    }
                                }
                            }
                        }

                        cbxValue.Items.Clear();

                        int i = 0;
                        foreach (KeyValuePair<string, string> kvp in fieldValues)
                        {
                            if (kvp.Key.Length > 0)
                            {
                                cbxValue.Items.Add(kvp.Key);
                            }

                            i++;
                        }
                        cbxValue.Items.Add(config.Settings.RepresentationOfMissing);
                    }
                }
            }
            else
            {
                string columnDataType = string.Empty;
                if (dashboardHelper.TableColumnNames.ContainsKey(columnName))
                {
                    columnDataType = dashboardHelper.TableColumnNames[columnName];
                }

                cbxValue.IsEnabled = true;
                cbxOperator.IsEnabled = true;

                FillOperatorValues(columnDataType);
                ShowValueSelector(columnDataType);
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event for the operator combo box
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void cbxOperator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxOperator.SelectedItem != null)
            {
                btnNewCondition.IsEnabled = true;

                string columnName = cbxFieldName.SelectedItem.ToString();
                Field field = null;
                string columnType = string.Empty;

                foreach (DataRow fieldRow in dashboardHelper.FieldTable.Rows)
                {
                    if (fieldRow["columnname"].Equals(columnName))
                    {
                        columnType = fieldRow["datatype"].ToString();
                        if (fieldRow["epifieldtype"] is Field)
                        {
                            field = fieldRow["epifieldtype"] as Field;
                        }
                        break;
                    }
                }

                if (field != null)
                {
                    ShowValueSelector(field);
                }
                else
                {
                    if (dashboardHelper.TableColumnNames.ContainsKey(columnName))
                    {
                        columnType = dashboardHelper.TableColumnNames[columnName];
                    }
                    ShowValueSelector(columnType);
                }
            }
            else
            {
                btnNewCondition.IsEnabled = false;
                cbxValue.IsEnabled = true;
            }
        }

        /// <summary>
        /// Handles the Click event for the remove button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnRemoveCondition_Click(object sender, RoutedEventArgs e)
        {
            if (dgFilters.Items.Count > 0 && dgFilters.SelectedIndex >= 0)
            {
                string friendlyCondition = ((DataRowView)dgFilters.SelectedItem).Row[DataFilters.COLUMN_FRIENDLY_FILTER].ToString();
                dashboardHelper.RemoveDataFilterCondition(friendlyCondition);
                UpdateFilterConditions();
            }
        }

        /// <summary>
        /// Handles the Click event for the clear conditions button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnClearConditions_Click(object sender, RoutedEventArgs e)
        {
            if (pnlAdvancedMode.Visibility != System.Windows.Visibility.Visible && dgFilters.Items.Count > 0 && dashboardHelper.DataFilters.Count > 0)
            {
                dashboardHelper.ClearDataFilterConditions();
                UpdateFilterConditions();
            }
        }

        /// <summary>
        /// Handles the Click event for the 'Add with and' pop-up menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void mnuAddWithAnd_Click(object sender, RoutedEventArgs e)
        {
            AddCondition(ConditionJoinType.And);
        }

        /// <summary>
        /// Handles the Click event for the 'Add with or' pop-up menu item
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void mnuAddWithOr_Click(object sender, RoutedEventArgs e)
        {
            AddCondition(ConditionJoinType.Or);
        }

        /// <summary>
        /// Handles the SelectionChanged event for the record processing scope combo box
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void cbxRecordProcessScope_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxRecordProcessScope.SelectedIndex == 0)
            {
                dashboardHelper.RecordProcessScope = RecordProcessingScope.Undeleted;
            }
            else if (cbxRecordProcessScope.SelectedIndex == 1)
            {
                dashboardHelper.RecordProcessScope = RecordProcessingScope.Deleted;
            }
            else if (cbxRecordProcessScope.SelectedIndex == 2)
            {
                dashboardHelper.RecordProcessScope = RecordProcessingScope.Both;
            }
        }
        #endregion // Event Handlers

        #region Private Methods
        /// <summary>
        /// Adds a new condition to the list of selection criteria
        /// </summary>
        /// <param name="conditionOperand">The operand to add the condition with</param>
        private void AddCondition(ConditionJoinType joinType)
        {
            Configuration config = Configuration.GetNewInstance();

            string fieldName = cbxFieldName.SelectedItem.ToString();
            string friendlyOperand = cbxOperator.SelectedItem.ToString();
            string friendlyValue = string.Empty;
            string friendlyCondition = string.Empty;

            if (!friendlyOperand.Equals(SharedStrings.FRIENDLY_OPERATOR_BETWEEN))
            {
                friendlyValue = GetValue();

                if (string.IsNullOrEmpty(friendlyValue) && !(friendlyOperand.Equals(SharedStrings.FRIENDLY_OPERATOR_MISSING) || friendlyOperand.Equals(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING)))
                {
                    Epi.Windows.MsgBox.ShowInformation(SharedStrings.COMPLETE_ALL_SELECTION_FIELDS);
                    return;
                }

                dashboardHelper.AddDataFilterCondition(friendlyOperand, friendlyValue, fieldName, joinType);
            }
            else if (friendlyOperand.Equals(SharedStrings.FRIENDLY_OPERATOR_EQUAL_TO) && cbxFieldName.SelectedIndex > -1 && dashboardHelper.GetColumnType(cbxFieldName.SelectedItem.ToString()).Equals("System.DateTime"))
            {
                string loValue = GetValue();
                string hiValue = GetValue();

                string friendlyLowValue = loValue.Trim();
                string friendlyHighValue = hiValue.Trim();

                if (string.IsNullOrEmpty(loValue) || string.IsNullOrEmpty(hiValue))
                {
                    Epi.Windows.MsgBox.ShowInformation(SharedStrings.COMPLETE_ALL_SELECTION_FIELDS);
                    return;
                }

                dashboardHelper.AddDataFilterCondition(friendlyOperand, friendlyLowValue, friendlyHighValue, fieldName, joinType);
            }
            else
            {
                friendlyValue = GetValue();
                string[] values = friendlyValue.Split(';');

                string loValue = values[0];
                string hiValue = values[1];

                string friendlyLowValue = loValue.Trim();
                string friendlyHighValue = hiValue.Trim();

                if (string.IsNullOrEmpty(loValue) || string.IsNullOrEmpty(hiValue))
                {
                    Epi.Windows.MsgBox.ShowInformation(SharedStrings.COMPLETE_ALL_SELECTION_FIELDS);
                    return;
                }

                dashboardHelper.AddDataFilterCondition(friendlyOperand, friendlyLowValue, friendlyHighValue, fieldName, joinType);
            }

            UpdateFilterConditions();
        }

        /// <summary>
        /// Sets the control to 'advanced' mode
        /// </summary>
        private void SetAdvancedFilterMode()
        {
            pnlAdvancedMode.Visibility = Visibility.Visible;
            grdSelectionProperties.Height = 0;
            grdGuidedModeButtons.Height = 0;

            grdSelectionProperties.IsEnabled = false;
            dashboardHelper.UseAdvancedUserDataFilter = true;
            txtAdvancedFilter.Foreground = this.Resources["normalColor"] as Brush;
            btnApplyAdvancedFilter.IsEnabled = true;
            txtAdvancedFilterStatus.Text = DashboardSharedStrings.GADGET_FILTERS_NONE;
            txtTitle.Margin = new Thickness(-3, 0, 0, -270);

            if (dashboardHelper.DataFilters.Count > 0 && string.IsNullOrEmpty(txtAdvancedFilter.Text))
            {
                txtAdvancedFilter.Text = dashboardHelper.DataFilters.GenerateDataFilterString();
            }
        }

        /// <summary>
        /// Sets the control to 'guided' mode
        /// </summary>
        private void SetGuidedFilterMode()
        {
            pnlAdvancedMode.Visibility = Visibility.Collapsed;
            grdSelectionProperties.Height = selectionGridHeight;
            grdGuidedModeButtons.Height = guidedButtonsGridHeight;

            grdSelectionProperties.IsEnabled = true;
            dashboardHelper.UseAdvancedUserDataFilter = false;
            UpdateFilterConditions();
            txtAdvancedFilterStatus.Text = string.Empty;
            txtTitle.Margin = new Thickness(-3, 0, 0, -80);
        }

        /// <summary>
        /// Sets the filtering mode
        /// </summary>
        private void SwapFilterMode()
        {
            if (pnlAdvancedMode.Visibility != Visibility.Visible)
            {
                SetAdvancedFilterMode();
            }
            else
            {
                SetGuidedFilterMode();
            }
        }

        /// <summary>
        /// Updates the list of selection conditions
        /// </summary>
        private void UpdateFilterConditions()
        {
            lbxConditions.Items.Clear();
            DataTable filterConditions = dashboardHelper.DataFilters.GetFilterConditionsAsTable();

            if (filterConditions.Columns.Contains(DataFilters.COLUMN_ACTIVE))
            {
                filterConditions.Columns.Remove(DataFilters.COLUMN_ACTIVE);
            }

            if (filterConditions.Columns.Contains(DataFilters.COLUMN_JOIN))
            {
                filterConditions.Columns[DataFilters.COLUMN_JOIN].ColumnName = DataFilters.COLUMN_FRIENDLY_JOIN;
            }

            if (filterConditions.Columns.Contains(DataFilters.COLUMN_FILTER))
            {
                filterConditions.Columns[DataFilters.COLUMN_FILTER].ColumnName = DataFilters.COLUMN_FRIENDLY_FILTER;
            }

            dgFilters.ItemsSource = filterConditions.DefaultView;

            if (SelectionCriteriaChanged != null)
            {
                SelectionCriteriaChanged(this, new EventArgs());
            }

            SetTitle();

            if (dgFilters.Items.Count == 0 || dashboardHelper.DataFilters.Count == 0)
            {
                btnClearConditions.IsEnabled = false;
                btnRemoveCondition.IsEnabled = false;
            }
            else
            {
                btnClearConditions.IsEnabled = true;
                btnRemoveCondition.IsEnabled = true;
            }
        }

        /// <summary>
        /// Fills in the Field Names combo box
        /// </summary>
        public void FillSelectionComboboxes(bool update = false)
        {
            cbxFieldName.ItemsSource = null;
            cbxOperator.Items.Clear();
            cbxRecordProcessScope.Items.Clear();

            List<string> fieldNames = new List<string>();
            ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.DateTime | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.UserDefined;

            fieldNames = dashboardHelper.GetFieldsAsList(columnDataType);

            if (fieldNames.Contains(ColumnNames.REC_STATUS))
            {
                fieldNames.Remove(ColumnNames.REC_STATUS);
            }

            cbxFieldName.ItemsSource = fieldNames;

            cbxRecordProcessScope.Items.Add("Normal records only");
            cbxRecordProcessScope.Items.Add("Deleted records only");
            cbxRecordProcessScope.Items.Add("Both normal and deleted records");

            cbxRecordProcessScope.SelectedIndex = 0;
        }

        /// <summary>
        /// Fills in the 'operator' drop-down list based upon the type of field being used in the selection condition
        /// </summary>
        /// <param name="field">The field that the condition is based upon</param>
        private void FillOperatorValues(Field field)
        {
            if (field is IDataField)
            {
                Configuration config = Configuration.GetNewInstance();

                cbxOperator.Items.Clear();

                // Set operator drop-down values
                if (field is NumberField || field is DateField || field is DateTimeField || field is TimeField )
                {
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_EQUAL_TO);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_NOT_EQUAL_TO);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_BETWEEN);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_LESS_THAN);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_GREATER_THAN);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_LESS_THAN_OR_EQUAL);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_GREATER_THAN_OR_EQUAL);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_MISSING);
                }
                else if (field is CheckBoxField)
                {
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_EQUAL_TO);
                    cbxOperator.SelectedIndex = 0;
                    cbxOperator.IsEnabled = false;

                    cbxValue.Items.Add(config.Settings.RepresentationOfYes);
                    cbxValue.Items.Add(config.Settings.RepresentationOfNo);
                    cbxValue.IsEnabled = true;
                }
                else if (field is YesNoField)
                {
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_EQUAL_TO);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_MISSING);

                    cbxValue.Items.Add(config.Settings.RepresentationOfYes);
                    cbxValue.Items.Add(config.Settings.RepresentationOfNo);
                    cbxValue.Items.Add(config.Settings.RepresentationOfMissing);
                    cbxValue.IsEnabled = true;
                }
                else if (field is TextField || field is UpperCaseTextField || field is TableBasedDropDownField || field is PhoneNumberField || field is GlobalRecordIdField)
                {
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_EQUAL_TO);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_NOT_EQUAL_TO);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_LIKE);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_MISSING);
                }
                else if (field is OptionField)
                {
                    FillOperatorValues(dashboardHelper.GetColumnType(field.Name));
                }
            }
        }

        /// <summary>
        /// Fills in the 'operator' drop-down list based upon the type of column being used in the selection condition
        /// </summary>
        /// <param name="column">The column that the condition is based upon</param>
        private void FillOperatorValues(string columnType)
        {
            Configuration config = Configuration.GetNewInstance();
            cbxOperator.Items.Clear();

            switch (columnType)
            {
                case "System.Decimal":
                case "System.Double":
                case "System.Single":
                case "System.Int64":
                case "System.DateTime":
                case "System.Int16":
                case "System.Int32":
                case "System.Byte":
                case "System.SByte":
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_EQUAL_TO);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_NOT_EQUAL_TO);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_BETWEEN);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_LESS_THAN);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_GREATER_THAN);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_LESS_THAN_OR_EQUAL);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_GREATER_THAN_OR_EQUAL);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_MISSING);
                    break;
                case "System.Boolean":
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_EQUAL_TO);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_MISSING);
                    cbxOperator.SelectedIndex = 0;
                    cbxValue.Items.Add(config.Settings.RepresentationOfYes);
                    cbxValue.Items.Add(config.Settings.RepresentationOfNo);
                    cbxValue.IsEnabled = true;
                    break;
                case "System.String":
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_EQUAL_TO);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_NOT_EQUAL_TO);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_LIKE);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING);
                    cbxOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_MISSING);
                    break;
            }
        }

        /// <summary>
        /// Shows the appropriate value selection control to the user depending on the type of field that is being selected for
        /// </summary>
        /// <param name="field">The field that the condition is based upon</param>
        private void ShowValueSelector(Field field)
        {
            txtValue.Visibility = System.Windows.Visibility.Visible;
            if (field is IDataField)
            {
                if (cbxOperator.SelectedValue != null)
                {
                    dateValue.IsEnabled = true;
                    tbxNumericValue.IsEnabled = true;
                    cbxValue.IsEnabled = true;
                    dateLowValue.IsEnabled = true;
                    dateHighValue.IsEnabled = true;
                    tbxLowValue.IsEnabled = true;
                    tbxHighValue.IsEnabled = true;
                    tbxValue.IsEnabled = true;
                }
                else
                {
                    dateValue.IsEnabled = false;
                    tbxNumericValue.IsEnabled = false;
                    cbxValue.IsEnabled = false;
                    dateLowValue.IsEnabled = false;
                    dateHighValue.IsEnabled = false;
                    tbxLowValue.IsEnabled = false;
                    tbxHighValue.IsEnabled = false;
                    tbxValue.IsEnabled = false;

                    tbxNumericValue.Clear();
                    cbxValue.SelectedIndex = -1;
                    tbxLowValue.Clear();
                    tbxHighValue.Clear();
                    tbxValue.Clear();
                    return;
                }

                if (cbxOperator.SelectedValue == null || !cbxOperator.SelectedValue.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_BETWEEN))
                {
                    txtAnd.Visibility = Visibility.Hidden;
                    dateLowValue.Visibility = Visibility.Hidden;
                    dateHighValue.Visibility = Visibility.Hidden;
                    tbxLowValue.Visibility = Visibility.Hidden;
                    tbxHighValue.Visibility = Visibility.Hidden;

                    // Set which 'value' control is visible
                    if (field is DateField || (field is DateTimeField && !(field is TimeField)))
                    {
                        dateValue.Visibility = Visibility.Visible;
                        tbxNumericValue.Visibility = Visibility.Hidden;
                        cbxValue.Visibility = Visibility.Hidden;
                        tbxValue.Visibility = Visibility.Hidden;

                        if (cbxOperator.SelectedItem.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING) || cbxOperator.SelectedItem.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_MISSING))
                        {
                            dateValue.IsEnabled = false;
                            dateValue.Visibility = Visibility.Hidden;
                            txtValue.Visibility = Visibility.Hidden;
                        }
                    }
                    else if (field is NumberField)
                    {
                        dateValue.Visibility = Visibility.Hidden;
                        tbxNumericValue.Visibility = Visibility.Visible;
                        cbxValue.Visibility = Visibility.Hidden;
                        tbxValue.Visibility = Visibility.Hidden;

                        if (cbxOperator.SelectedItem.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING) || cbxOperator.SelectedItem.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_MISSING))
                        {
                            tbxNumericValue.IsEnabled = false;
                        }
                    }
                    else if (field is TableBasedDropDownField || field is CheckBoxField || field is YesNoField)
                    {
                        dateValue.Visibility = Visibility.Hidden;
                        tbxNumericValue.Visibility = Visibility.Hidden;
                        cbxValue.Visibility = Visibility.Visible;
                        tbxValue.Visibility = Visibility.Hidden;

                        if (cbxOperator.SelectedItem.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING) || cbxOperator.SelectedItem.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_MISSING))
                        {
                            cbxValue.IsEnabled = false;
                        }
                    }
                    else if (field is TextField || field is UpperCaseTextField || field is MultilineTextField)
                    {
                        dateValue.Visibility = Visibility.Hidden;
                        tbxNumericValue.Visibility = Visibility.Hidden;
                        cbxValue.Visibility = Visibility.Hidden;
                        tbxValue.Visibility = Visibility.Visible;

                        if (cbxOperator.SelectedItem.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING) || cbxOperator.SelectedItem.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_MISSING))
                        {
                            tbxValue.IsEnabled = false;
                        }
                    }
                }
                else
                {
                    txtAnd.Visibility = Visibility.Visible;
                    dateValue.Visibility = Visibility.Hidden;
                    tbxNumericValue.Visibility = Visibility.Hidden;
                    cbxValue.Visibility = Visibility.Hidden;
                    dateLowValue.Visibility = Visibility.Hidden;
                    dateHighValue.Visibility = Visibility.Hidden;
                    tbxLowValue.Visibility = Visibility.Hidden;
                    tbxHighValue.Visibility = Visibility.Hidden;
                    tbxValue.Visibility = Visibility.Hidden;

                    if (field is DateField || (field is DateTimeField && !(field is TimeField)))
                    {
                        dateLowValue.Visibility = Visibility.Visible;
                        dateHighValue.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        tbxLowValue.Visibility = Visibility.Visible;
                        tbxHighValue.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        /// <summary>
        /// Shows the appropriate value selection control to the user depending on the type of column that is being selected for
        /// </summary>
        /// <param name="column">The column that the condition is based upon</param>
        /// 
        

        private void ShowValueSelector(string columnType)
        {
            txtValue.Visibility = System.Windows.Visibility.Visible;

            if (cbxOperator.SelectedValue != null)
            {
                dateValue.IsEnabled = true;
                tbxNumericValue.IsEnabled = true;
                cbxValue.IsEnabled = true;
                dateLowValue.IsEnabled = true;
                dateHighValue.IsEnabled = true;
                tbxLowValue.IsEnabled = true;
                tbxHighValue.IsEnabled = true;
                tbxValue.IsEnabled = true;
            }
            else
            {
                dateValue.IsEnabled = false;
                tbxNumericValue.IsEnabled = false;
                cbxValue.IsEnabled = false;
                dateLowValue.IsEnabled = false;
                dateHighValue.IsEnabled = false;
                tbxLowValue.IsEnabled = false;
                tbxHighValue.IsEnabled = false;
                tbxValue.IsEnabled = false;

                tbxNumericValue.Clear();
                cbxValue.SelectedIndex = -1;
                tbxLowValue.Clear();
                tbxHighValue.Clear();
                tbxValue.Clear();
                return;
            }

            if (cbxOperator.SelectedValue == null || !cbxOperator.SelectedValue.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_BETWEEN))
            {
                txtAnd.Visibility = Visibility.Hidden;
                dateLowValue.Visibility = Visibility.Hidden;
                dateHighValue.Visibility = Visibility.Hidden;
                tbxLowValue.Visibility = Visibility.Hidden;
                tbxHighValue.Visibility = Visibility.Hidden;

                // Set which 'value' control is visible
                if (columnType.Equals("System.DateTime"))
                {
                    dateValue.Visibility = Visibility.Visible;
                    tbxNumericValue.Visibility = Visibility.Hidden;
                    cbxValue.Visibility = Visibility.Hidden;
                    tbxValue.Visibility = Visibility.Hidden;

                    if (cbxOperator.SelectedItem.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING) || cbxOperator.SelectedItem.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_MISSING))
                    {
                        dateValue.IsEnabled = false;
                        dateValue.Visibility = Visibility.Hidden;
                        txtValue.Visibility = Visibility.Hidden;
                    }
                }
                else if (columnType.Equals("System.Int16") || columnType.Equals("System.Int32") || columnType.Equals("System.Int64") || columnType.Equals("System.Single") || columnType.Equals("System.Double") || columnType.Equals("System.Decimal") || columnType.Equals("System.Byte") || columnType.Equals("System.SByte"))
                {
                    dateValue.Visibility = Visibility.Hidden;
                    tbxNumericValue.Visibility = Visibility.Visible;
                    cbxValue.Visibility = Visibility.Hidden;
                    tbxValue.Visibility = Visibility.Hidden;

                    if (cbxOperator.SelectedItem.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING) || cbxOperator.SelectedItem.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_MISSING))
                    {
                        tbxNumericValue.IsEnabled = false;
                    }
                }
                else if (columnType.Equals("System.Boolean"))
                {
                    dateValue.Visibility = Visibility.Hidden;
                    tbxNumericValue.Visibility = Visibility.Hidden;
                    cbxValue.Visibility = Visibility.Visible;
                    tbxValue.Visibility = Visibility.Hidden;

                    if (cbxOperator.SelectedItem.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING) || cbxOperator.SelectedItem.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_MISSING))
                    {
                        cbxValue.IsEnabled = false;
                    }
                }
                else if (columnType.Equals("System.String"))
                {
                    dateValue.Visibility = Visibility.Hidden;
                    tbxNumericValue.Visibility = Visibility.Hidden;
                    cbxValue.Visibility = Visibility.Hidden;
                    tbxValue.Visibility = Visibility.Visible;

                    if (cbxOperator.SelectedItem.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING) || cbxOperator.SelectedItem.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_MISSING))
                    {
                        tbxValue.IsEnabled = false;
                    }
                }
            }
            else
            {
                txtAnd.Visibility = Visibility.Visible;
                dateValue.Visibility = Visibility.Hidden;
                tbxNumericValue.Visibility = Visibility.Hidden;
                cbxValue.Visibility = Visibility.Hidden;
                dateLowValue.Visibility = Visibility.Hidden;
                dateHighValue.Visibility = Visibility.Hidden;
                tbxLowValue.Visibility = Visibility.Hidden;
                tbxHighValue.Visibility = Visibility.Hidden;
                tbxValue.Visibility = Visibility.Hidden;

                if (columnType.Equals("System.DateTime"))
                {
                    dateLowValue.Visibility = Visibility.Visible;
                    dateHighValue.Visibility = Visibility.Visible;
                }
                else
                {
                    tbxLowValue.Visibility = Visibility.Visible;
                    tbxHighValue.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Gets the proper value to use for the condition
        /// </summary>
        /// <returns>The value as a string</returns>
        private string GetValue()
        {
            string value = string.Empty;

            if (cbxOperator.SelectedValue.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING) || cbxOperator.SelectedValue.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_MISSING))
            {
                value = string.Empty;
            }
            else if (!cbxOperator.SelectedValue.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_BETWEEN))
            {
                if (dateValue.Visibility == Visibility.Visible)
                {
                    value = dateValue.Text;
                }
                else if (tbxNumericValue.Visibility == Visibility.Visible)
                {
                    value = tbxNumericValue.Text;
                }
                else if (cbxValue.Visibility == Visibility.Visible)
                {
                    if (cbxValue.SelectedItem != null)
                    {
                        value = cbxValue.SelectedItem.ToString();
                    }
                    else if (!string.IsNullOrEmpty(cbxValue.Text))
                    {
                        value = cbxValue.Text;
                    }
                    else
                    {
                        value = string.Empty;
                    }
                }
                else if (tbxValue.Visibility == Visibility.Visible)
                {
                    value = tbxValue.Text;
                }
            }
            else
            {
                if (dateLowValue.Visibility == Visibility.Visible)
                {
                    value = dateLowValue.Text + ";" + dateHighValue.Text;
                }
                else if (tbxLowValue.Visibility == Visibility.Visible)
                {
                    value = tbxLowValue.Text + ";" + tbxHighValue.Text;
                }
            }

            return value;
        }

        /// <summary>
        /// Sets the title for the data filter control
        /// </summary>
        private void SetTitle()
        {
            if (pnlAdvancedMode.Visibility == System.Windows.Visibility.Visible)
            {
                this.txtTitle.Text = SharedStrings.DATA_FILTERS;
            }
            else
            {
                this.txtTitle.Text = SharedStrings.DATA_FILTERS + StringLiterals.SPACE + StringLiterals.PARANTHESES_OPEN +
                    dgFilters.Items.Count.ToString() + StringLiterals.PARANTHESES_CLOSE;
            }
        }

        /// <summary>
        /// Applies a filter from 'advanced' mode
        /// </summary>
        private void ApplyAdvancedModeFilter()
        {
            DataView dv = dashboardHelper.GenerateView();
            //DataTable dt = dv.Table;//dashboardHelper.GenerateTopTwoTable();

            //dashboardHelper.AddSystemVariablesToTable(dt);
            //dashboardHelper.AddPermanentVariablesToTable(dt);            

            try
            {
                dv.RowFilter = txtAdvancedFilter.Text;
                //DataRow[] rows = dt.Select(txtAdvancedFilter.Text);
            }
            catch (Exception)
            {
                txtAdvancedFilterStatus.Text = DashboardSharedStrings.FILTER_IS_INVALID;
                txtAdvancedFilter.Foreground = this.Resources["invalidColor"] as Brush;
                return;
            }

            if (SelectionCriteriaChanged != null)
            {
                dashboardHelper.AdvancedUserDataFilter = txtAdvancedFilter.Text;
                btnApplyAdvancedFilter.IsEnabled = false;
                SelectionCriteriaChanged(this, new EventArgs());
            }

            txtAdvancedFilter.Foreground = this.Resources["validColor"] as Brush;
            txtAdvancedFilterStatus.Text = DashboardSharedStrings.FILTER_IS_VALID;

            SetTitle();
        }

        /// <summary>
        /// Checks to see whether a valid numeric digit was pressed for numeric-only conditions
        /// </summary>
        /// <param name="keyChar">The key that was pressed</param>
        /// <returns>Whether the input was a valid number character</returns>
        private bool ValidNumberChar(string keyChar)
        {
            System.Globalization.NumberFormatInfo numberFormatInfo = System.Globalization.CultureInfo.CurrentCulture.NumberFormat;

            if (keyChar == numberFormatInfo.NegativeSign | keyChar == numberFormatInfo.NumberDecimalSeparator | keyChar == numberFormatInfo.PercentDecimalSeparator)
            {
                return true;
            }

            for (int i = 0; i < keyChar.Length; i++)
            {
                char ch = keyChar[i];
                if (!Char.IsDigit(ch))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion // Private Methods

        private void dgFilters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }
    }



}
