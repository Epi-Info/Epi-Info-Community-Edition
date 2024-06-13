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
using Epi.Core;
using Epi.Data;
using Epi.Fields;

namespace EpiDashboard
{
    /// <summary>
    /// Interaction logic for RowFilterControl.xaml
    /// </summary>
    public partial class RowFilterControl : UserControl
    {
        #region Private Members
        private DashboardHelper dashboardHelper;
        private double selectionGridHeight;
        private double guidedButtonsGridHeight;
        public DataFilters DataFilters;
        private Configuration config;
        private bool includeUserDefinedVars = true;
        private EpiDashboard.Dialogs.FilterDialogMode mode = EpiDashboard.Dialogs.FilterDialogMode.RowFilterMode;
        #endregion // Private Members

        public event EventHandler SelectionCriteriaChanged;

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view to attach</param>
        /// <param name="db">The database to attach</param>
        /// <param name="dashboardHelper">The dashboard helper to attach</param>
        public RowFilterControl(DashboardHelper dashboardHelper, EpiDashboard.Dialogs.FilterDialogMode pMode, DataFilters filters = null, bool includeUserDefinedVars = true)
        {
            InitializeComponent();
            this.dashboardHelper = dashboardHelper;
            if (dashboardHelper != null)
            {
                this.config = dashboardHelper.Config;
                this.includeUserDefinedVars = includeUserDefinedVars;
                this.Mode = pMode;

                if (filters == null)
                {
                    DataFilters = new DataFilters(this.dashboardHelper);
                }
                else
                {
                    DataFilters = filters;
                }

                //txtTitle.RenderTransform = new RotateTransform(270);
                FillSelectionComboboxes();

                selectionGridHeight = grdSelectionProperties.Height;
                guidedButtonsGridHeight = grdGuidedModeButtons.Height;

                //if (dashboardHelper.UseAdvancedUserDataFilter)
                //{
                //    pnlAdvancedMode.Visibility = Visibility.Visible;
                //    txtAdvancedFilter.Text = dashboardHelper.AdvancedUserDataFilter;
                //    SetAdvancedFilterMode();
                //    ApplyAdvancedModeFilter();
                //}
                //else
                //{
                pnlAdvancedMode.Visibility = Visibility.Collapsed;
                txtAdvancedFilter.Text = string.Empty;
                SetGuidedFilterMode();
                //}

                UpdateFilterConditions();

                if (!dashboardHelper.IsUsingEpiProject)
                {
                    //panelAdvanced.Visibility = Visibility.Collapsed;
                }
                config = Configuration.GetNewInstance();
            }

            #region Translation

            tblockValueFieldName.Text = DashboardSharedStrings.GADGET_VALUE_FIELD_NAME;
            tblockOperator.Text = DashboardSharedStrings.GADGET_OPERATOR;
            txtValue.Text = DashboardSharedStrings.GADGET_VALUE_TEXT;
            txtLoValue.Text = DashboardSharedStrings.GADGET_LOW_VALUE;
            txtHiValue.Text = DashboardSharedStrings.GADGET_HIGH_VALUE;
            tblockAddThisCondition.Content = DashboardSharedStrings.GADGET_ADD_CONDITION;
            mnuAddWithAnd.Header = DashboardSharedStrings.GADGET_ADD_CONDITION_AND;
            mnuAddWithOr.Header = DashboardSharedStrings.GADGET_ADD_CONDITION_OR;
            tblockDataFilterListHeading.Text = DashboardSharedStrings.GADGET_DATAFILTER;
            tblockRemoveSelected.Content = DashboardSharedStrings.GADGET_REMOVE_SELECTED;
            tblockClearAllConditions.Content = DashboardSharedStrings.GADGET_CLEAR_ALL_CONDITIONS;
            tblockAdvancedMode.Content = DashboardSharedStrings.GADGET_ADVANCED_MODE;
            tblockAdvanceFilterMode.Text = DashboardSharedStrings.GADGET_ADVANCED_FILTER_MODE;
            txtDesiredDataFilter.Text = DashboardSharedStrings.GADGET_DESIRED_DATA_FILTER;
            txtNumericData.Text = DashboardSharedStrings.GADGET_NUMERIC_DATA;
            txtTextData.Text = DashboardSharedStrings.GADGET_TEXT_DATA;
            txtBooleanData.Text = DashboardSharedStrings.GADGET_BOOLEAN_DATA;
            btnApplyAdvancedFilter.Content = DashboardSharedStrings.GADGET_APPLY_ADVANCED_FILTER;
            btnGuidedMode.Content = DashboardSharedStrings.GADGET_GUIDED_MODE;

            #endregion // Translation

        }
        #endregion // Constructors

        #region Private Properties
        /// <summary>
        /// Gets/sets the dialog mode
        /// </summary>
        private EpiDashboard.Dialogs.FilterDialogMode Mode
        {
            get
            {
                return this.mode;
            }
            set
            {
                this.mode = value;
                if (Mode == EpiDashboard.Dialogs.FilterDialogMode.RowFilterMode)
                {
                    this.btnNewCondition.Content = SharedStrings.ADD_FILTER;
                    this.tblockDataFilterListHeading.Text = SharedStrings.DATA_FILTERS;
                }
                else if (Mode == EpiDashboard.Dialogs.FilterDialogMode.ConditionalMode)
                {
                    this.btnNewCondition.Content = SharedStrings.ADD_CONDITION;
                    this.tblockDataFilterListHeading.Text = SharedStrings.CONDITIONS;
                }
            }
        }

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
        #endregion // Public Methods

        #region Event Handlers
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
            txtAdvancedFilterStatus.Text = SharedStrings.ADVANCED_FILTER_CHANGED;
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
        private void cmbFieldName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmbOperator.Items.Clear();
            cmbValue.Items.Clear();

            if (cmbFieldName.SelectedIndex <= -1)
            {
                return;
            }

            string columnName = cmbFieldName.SelectedItem.ToString();
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
                cmbValue.IsEnabled = true;
                cmbOperator.IsEnabled = true;

                if (field is IDataField)
                {
                    FillOperatorValues(field);
                    ShowValueSelector(field);
                }

                if (field is TableBasedDropDownField)
                {
                    //DataTable dataTable = db.Select(db.CreateQuery("SELECT " + field.Name + " " + view.FromViewSQL));
                    DataTable dataTable = ((TableBasedDropDownField)field).GetSourceData();
                   if (dataTable != null)
                    { 
                    Dictionary<string, string> fieldValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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
									if (dash < 0)
										continue;
                                Key = Key.Substring(0, dash);
                                if (!fieldValues.ContainsKey(Key))
                                {
                                    fieldValues.Add(Key, Key);
                                }
                            }
                        }
                    }

                    cmbValue.Items.Clear();

                    int i = 0;
                    foreach (KeyValuePair<string, string> kvp in fieldValues)
                    {
                        if (kvp.Key.Length > 0)
                        {
                            cmbValue.Items.Add(kvp.Key);
                        }

                        i++;
                    }
                    cmbValue.Items.Add(config.Settings.RepresentationOfMissing);
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
                
                cmbValue.IsEnabled = true;
                cmbOperator.IsEnabled = true;

                FillOperatorValues(columnDataType);
                ShowValueSelector(columnDataType);
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event for the operator combo box
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void cmbOperator_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbOperator.SelectedItem != null)
            {
                btnNewCondition.IsEnabled = true;

                string columnName = cmbFieldName.SelectedItem.ToString();
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
                cmbValue.IsEnabled = true;
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
                DataFilters.RemoveFilterCondition(friendlyCondition);
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
            if (pnlAdvancedMode.Visibility != System.Windows.Visibility.Visible && dgFilters.Items.Count > 0 && this.DataFilters.Count > 0)
            {
                this.DataFilters.ClearFilterConditions();
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
        private void cmbRecordProcessScope_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (cbxRecordProcessScope.SelectedIndex == 0)
            //{
            //    dashboardHelper.RecordProcessScope = RecordProcessingScope.Undeleted;
            //}
            //else if (cbxRecordProcessScope.SelectedIndex == 1)
            //{
            //    dashboardHelper.RecordProcessScope = RecordProcessingScope.Deleted;
            //}
            //else if (cbxRecordProcessScope.SelectedIndex == 2)
            //{
            //    dashboardHelper.RecordProcessScope = RecordProcessingScope.Both;
            //}
        }
        #endregion // Event Handlers

        #region Private Methods
        /// <summary>
        /// Adds a new condition to the list of selection criteria
        /// </summary>
        /// <param name="conditionOperand">The operand to add the condition with</param>
        private void AddCondition(ConditionJoinType joinType)
        {
            string fieldName = cmbFieldName.SelectedItem.ToString();
            string friendlyOperand = cmbOperator.SelectedItem.ToString();
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

                AddDataFilterCondition(friendlyOperand, friendlyValue, fieldName, joinType);
            }
            else if (friendlyOperand.Equals(SharedStrings.FRIENDLY_OPERATOR_EQUAL_TO) && cmbFieldName.SelectedIndex > -1 && dashboardHelper.GetColumnType(cmbFieldName.SelectedItem.ToString()).Equals("System.DateTime"))
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

                AddDataFilterCondition(friendlyOperand, friendlyLowValue, friendlyHighValue, fieldName, joinType);
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

                AddDataFilterCondition(friendlyOperand, friendlyLowValue, friendlyHighValue, fieldName, joinType);
            }

            UpdateFilterConditions();
        }

        /// <summary>
        /// Adds a new data filtering condition
        /// </summary>
        /// <param name="friendlyCondition">The human-readable condition</param>
        /// <param name="friendlyOperand">The human-readable operand</param>
        /// <param name="friendlyValue">The human-readable value</param>
        /// <param name="columnName">The name of the column</param>
        /// <param name="joinType">The type of join to use in including this condition in the list of conditions</param>
        /// <returns>Whether the addition was successful</returns>
        public bool AddDataFilterCondition(string friendlyOperand, string friendlyValue, string columnName, ConditionJoinType joinType)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(friendlyOperand.Trim()))
            {
                throw new ArgumentNullException("friendlyOperand");
            }
            else if (string.IsNullOrEmpty(columnName.Trim()))
            {
                throw new ArgumentNullException("columnName");
            }
            #endregion // Input Validation

            bool added = false;
            string columnType = dashboardHelper.GetColumnType(columnName);
            string rawColumnName = columnName;

            columnName = Util.InsertInSquareBrackets(columnName);

            if (columnType.Equals("System.DateTime") && friendlyOperand.Equals(SharedStrings.FRIENDLY_OPERATOR_EQUAL_TO))
            {
                DateTime lowVal = DateTime.Parse(friendlyValue, System.Globalization.CultureInfo.CurrentCulture);
                string friendlyLowValue = lowVal.ToString("M/d/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                DateTime highVal = DateTime.Parse(friendlyValue, System.Globalization.CultureInfo.CurrentCulture);
                string friendlyHighValue = highVal.ToString("M/d/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                FilterCondition newCondition = this.DataFilters.GenerateFilterCondition(columnName, rawColumnName, columnType, friendlyOperand, friendlyLowValue, friendlyHighValue);
                this.DataFilters.AddFilterCondition(newCondition, joinType);
            }
            else if (columnType.Equals("System.DateTime") && friendlyOperand.Equals(SharedStrings.FRIENDLY_OPERATOR_LESS_THAN_OR_EQUAL))
            {
                DateTime dateVal = DateTime.Parse(friendlyValue, System.Globalization.CultureInfo.CurrentCulture);

                if (dateVal.Hour == 0)
                    dateVal = dateVal.AddHours(23);
                if (dateVal.Minute == 0)
                    dateVal = dateVal.AddMinutes(59);
                if (dateVal.Second == 0)
                    dateVal = dateVal.AddSeconds(59);
                if (dateVal.Millisecond == 0)
                    dateVal = dateVal.AddMilliseconds(999);

                friendlyValue = dateVal.ToString("M/d/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                FilterCondition newCondition = this.DataFilters.GenerateFilterCondition(columnName, rawColumnName, columnType, friendlyOperand, friendlyValue);
                this.DataFilters.AddFilterCondition(newCondition, joinType);
            }
            else if (columnType.Equals("System.DateTime") && friendlyOperand.Equals(SharedStrings.FRIENDLY_OPERATOR_GREATER_THAN))
            {
                DateTime dateVal = DateTime.Parse(friendlyValue, System.Globalization.CultureInfo.CurrentCulture);

                if (dateVal.Hour == 0)
                    dateVal = dateVal.AddHours(23);
                if (dateVal.Minute == 0)
                    dateVal = dateVal.AddMinutes(59);
                if (dateVal.Second == 0)
                    dateVal = dateVal.AddSeconds(59);
                if (dateVal.Millisecond == 0)
                    dateVal = dateVal.AddMilliseconds(999);

                friendlyValue = dateVal.ToString("M/d/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                FilterCondition newCondition = this.DataFilters.GenerateFilterCondition(columnName, rawColumnName, columnType, friendlyOperand, friendlyValue);
                this.DataFilters.AddFilterCondition(newCondition, joinType);
            }
            else if (columnType.Equals("System.DateTime"))
            {
                if (string.IsNullOrEmpty(friendlyValue))
                {
                    friendlyValue = config.Settings.RepresentationOfMissing;
                }
                else
                {
                    DateTime dateVal = DateTime.Parse(friendlyValue, System.Globalization.CultureInfo.CurrentCulture);
                    friendlyValue = dateVal.ToString("M/d/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                }

                FilterCondition newCondition = this.DataFilters.GenerateFilterCondition(columnName, rawColumnName, columnType, friendlyOperand, friendlyValue);
                this.DataFilters.AddFilterCondition(newCondition, joinType);
            }
            else
            {
                FilterCondition newCondition = this.DataFilters.GenerateFilterCondition(columnName, rawColumnName, columnType, friendlyOperand, friendlyValue);
                this.DataFilters.AddFilterCondition(newCondition, joinType);
            }

            return added;
        }

        /// <summary>
        /// Adds a new data filtering condition using the between operator
        /// </summary>
        /// <param name="friendlyCondition">The human-readable condition</param>
        /// <param name="condition">The condition to add</param>
        /// <returns>Whether the addition was successful</returns>
        public bool AddDataFilterCondition(string friendlyOperand, string friendlyLowValue, string friendlyHighValue, string columnName, ConditionJoinType joinType)
        {
            #region Input Validation
            if (string.IsNullOrEmpty(friendlyOperand.Trim()))
            {
                throw new ArgumentNullException("friendlyOperand");
            }
            else if (string.IsNullOrEmpty(columnName.Trim()))
            {
                throw new ArgumentNullException("columnName");
            }
            else if (string.IsNullOrEmpty(friendlyLowValue.Trim()))
            {
                throw new ArgumentNullException("friendlyLowValue");
            }
            else if (string.IsNullOrEmpty(friendlyHighValue.Trim()))
            {
                throw new ArgumentNullException("friendlyHighValue");
            }
            #endregion // Input Validation

            string columnType = dashboardHelper.GetColumnType(columnName);
            string rawColumnName = columnName;

            if (!dashboardHelper.IsUsingEpiProject)
            {
                columnName = Util.InsertInSquareBrackets(columnName);
            }

            if (columnType.Equals("System.DateTime"))
            {
                DateTime lowVal = DateTime.Parse(friendlyLowValue, System.Globalization.CultureInfo.CurrentCulture);
                friendlyLowValue = lowVal.ToString("M/d/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                DateTime highVal = DateTime.Parse(friendlyHighValue, System.Globalization.CultureInfo.CurrentCulture);
                friendlyHighValue = highVal.ToString("M/d/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            }

            bool added = false;
            FilterCondition newCondition = this.DataFilters.GenerateFilterCondition(columnName, rawColumnName, columnType, friendlyOperand, friendlyLowValue, friendlyHighValue);
            this.DataFilters.AddFilterCondition(newCondition, joinType);

            return added;
        }

        /// <summary>
        /// Sets the control to 'advanced' mode
        /// </summary>
        private void SetAdvancedFilterMode()
        {
            //pnlAdvancedMode.Visibility = Visibility.Visible;
            //grdSelectionProperties.Height = 0;
            //grdGuidedModeButtons.Height = 0;

            //grdSelectionProperties.IsEnabled = false;
            //dashboardHelper.UseAdvancedUserDataFilter = true;
            //txtAdvancedFilter.Foreground = this.Resources["normalColor"] as Brush;
            //btnApplyAdvancedFilter.IsEnabled = true;
            //txtAdvancedFilterStatus.Text = "No advanced filters are in effect.";
            //txtTitle.Margin = new Thickness(-3, 0, 0, -270);

            //if (dashboardHelper.DataFilters.Count > 0 && string.IsNullOrEmpty(txtAdvancedFilter.Text))
            //{
            //    txtAdvancedFilter.Text = dashboardHelper.DataFilters.GenerateDataFilterString();
            //}
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
            //dashboardHelper.UseAdvancedUserDataFilter = false;
            UpdateFilterConditions();
            txtAdvancedFilterStatus.Text = string.Empty;
            //txtTitle.Margin = new Thickness(-3, 0, 0, -80);
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
            //lbxConditions.Items.Clear();
            DataTable filterConditions = DataFilters.GetFilterConditionsAsTable();

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

            if (dgFilters.Items.Count == 0 || this.DataFilters.Count == 0)
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
            cmbFieldName.ItemsSource = null;
            cmbOperator.Items.Clear();
            //cbxRecordProcessScope.Items.Clear();

            List<string> fieldNames = new List<string>();            
            ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.DateTime | ColumnDataType.Numeric | ColumnDataType.Text;

            if (includeUserDefinedVars)
            {
                columnDataType = ColumnDataType.Boolean | ColumnDataType.DateTime | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.UserDefined;
            }

            fieldNames = dashboardHelper.GetFieldsAsList(columnDataType);

            if (fieldNames.Contains(ColumnNames.REC_STATUS))
            {
                fieldNames.Remove(ColumnNames.REC_STATUS);
            }

            if (fieldNames.Contains("SYSTEMDATE"))
            {
                fieldNames.Remove("SYSTEMDATE");
            }

            if (fieldNames.Contains("GlobalRecordId"))
            {
                fieldNames.Remove("GlobalRecordId");
            }

            cmbFieldName.ItemsSource = fieldNames;

            //cbxRecordProcessScope.Items.Add("Normal records only");
            //cbxRecordProcessScope.Items.Add("Deleted records only");
            //cbxRecordProcessScope.Items.Add("Both normal and deleted records");

            //cbxRecordProcessScope.SelectedIndex = 0;
        }

        /// <summary>
        /// Fills in the 'operator' drop-down list based upon the type of field being used in the selection condition
        /// </summary>
        /// <param name="field">The field that the condition is based upon</param>
        private void FillOperatorValues(Field field) 
        {
            if (field is IDataField)
            {
                cmbOperator.Items.Clear();

                // Set operator drop-down values
                if (field is NumberField || field is DateField || field is DateTimeField || field is TimeField)
                {
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_EQUAL_TO);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_NOT_EQUAL_TO);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_BETWEEN);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_LESS_THAN);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_GREATER_THAN);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_LESS_THAN_OR_EQUAL);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_GREATER_THAN_OR_EQUAL);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_MISSING);
                }
                else if (field is CheckBoxField)
                {
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_EQUAL_TO);
                    cmbOperator.SelectedIndex = 0;
                    cmbOperator.IsEnabled = false;

                    cmbValue.Items.Add(config.Settings.RepresentationOfYes);
                    cmbValue.Items.Add(config.Settings.RepresentationOfNo);
                    cmbValue.IsEnabled = true;
                }
                else if (field is YesNoField)
                {
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_EQUAL_TO);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_MISSING);

                    cmbValue.Items.Add(config.Settings.RepresentationOfYes);
                    cmbValue.Items.Add(config.Settings.RepresentationOfNo);
                    cmbValue.Items.Add(config.Settings.RepresentationOfMissing);
                    cmbValue.IsEnabled = true;
                }
                else if (field is TextField || field is UpperCaseTextField || field is TableBasedDropDownField || field is PhoneNumberField)
                {
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_EQUAL_TO);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_NOT_EQUAL_TO);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_LIKE);

                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_IS_ANY_OF);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_IS_NOT_ANY_OF);

                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_MISSING);
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
            cmbOperator.Items.Clear();

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
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_EQUAL_TO);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_NOT_EQUAL_TO);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_BETWEEN);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_LESS_THAN);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_GREATER_THAN);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_LESS_THAN_OR_EQUAL);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_GREATER_THAN_OR_EQUAL);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_MISSING);
                    break;
                case "System.Boolean":
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_EQUAL_TO);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_MISSING);
                    cmbOperator.SelectedIndex = 0;
                    cmbValue.Items.Add(config.Settings.RepresentationOfYes);
                    cmbValue.Items.Add(config.Settings.RepresentationOfNo);                    
                    cmbValue.IsEnabled = true;
                    break;
                case "System.String":
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_EQUAL_TO);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_NOT_EQUAL_TO);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_LIKE);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING);
                    cmbOperator.Items.Add(SharedStrings.FRIENDLY_OPERATOR_MISSING);
                    break;
            }
        }

        /// <summary>
        /// Shows the appropriate value selection control to the user depending on the type of field that is being selected for
        /// </summary>
        /// <param name="field">The field that the condition is based upon</param>
        private void ShowValueSelector(Field field)
        {
            HideValueSelectors();
            if (field is IDataField)
            {
                if (cmbOperator.SelectedValue != null)
                {
                    dpValue.IsEnabled = true;
                    tbxNumericValue.IsEnabled = true;
                    cmbValue.IsEnabled = true;
                    dpLowValue.IsEnabled = true;
                    dpHighValue.IsEnabled = true;
                    tbxLowValue.IsEnabled = true;
                    tbxHighValue.IsEnabled = true;
                    tbxValue.IsEnabled = true;

                    if (cmbOperator.SelectedValue.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_BETWEEN))
                    {
                        //Show the Lo and Hi value labels
                        txtLoValue.Visibility = Visibility.Visible;
                        txtAnd.Visibility = Visibility.Visible;
                        txtHiValue.Visibility = Visibility.Visible;
                        if (field is DateField || (field is DateTimeField && !(field is TimeField)))
                        {
                            ShowDateLoHiSelector();
                        }
                        else
                        {
                            ShowNumericLoHiSelector();
                        }
                    }
                    else if (!cmbOperator.SelectedItem.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING) && !cmbOperator.SelectedItem.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_MISSING))
                    {
                        if (field is DateField || (field is DateTimeField && !(field is TimeField)))
                        {
                            ShowDateValueSelector();
                        }
                        else if (field is NumberField)
                        {
                            ShowNumericValueSelector();
                        }
                        else if (field is TableBasedDropDownField || field is CheckBoxField || field is YesNoField)
                        {
                            ShowDDLValueSelector();
                        }
                        else if (field is TextField || field is UpperCaseTextField || field is MultilineTextField)
                        {
                            ShowTextValueSelector();
                        }
                    }
                }
                else  //cmbOperator.SelectedValue == null
                {
                    dpValue.IsEnabled = false;
                    tbxNumericValue.IsEnabled = false;
                    cmbValue.IsEnabled = false;
                    dpLowValue.IsEnabled = false;
                    dpHighValue.IsEnabled = false;
                    tbxLowValue.IsEnabled = false;
                    tbxHighValue.IsEnabled = false;
                    tbxValue.IsEnabled = false;

                    tbxNumericValue.Clear();
                    cmbValue.SelectedIndex = -1;
                    tbxLowValue.Clear();
                    tbxHighValue.Clear();
                    tbxValue.Clear();
                    return;
                }
            }
        }

        /// <summary>
        /// Shows the appropriate value selection control to the user depending on the type of column that is being selected for
        /// </summary>
        /// <param name="column">The column that the condition is based upon</param>
        private void ShowValueSelector(string columnType)
        {
            HideValueSelectors();
            if (cmbOperator.SelectedValue != null)
            {
                tbxNumericValue.IsEnabled = true;
                cmbValue.IsEnabled = true;
                dpLowValue.IsEnabled = true;
                dpHighValue.IsEnabled = true;
                tbxLowValue.IsEnabled = true;
                tbxHighValue.IsEnabled = true;
                tbxValue.IsEnabled = true;

                if (cmbOperator.SelectedValue.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_BETWEEN))
                {
                    //Show the Lo and Hi value labels
                    txtLoValue.Visibility = Visibility.Visible;
                    txtAnd.Visibility = Visibility.Visible;
                    txtHiValue.Visibility = Visibility.Visible;
                    if (columnType.Equals("System.DateTime"))
                    {
                        ShowDateLoHiSelector();
                    }
                    else
                    {
                        ShowNumericLoHiSelector();
                    }
                }
                else if (!cmbOperator.SelectedItem.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING) && !cmbOperator.SelectedItem.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_MISSING))
                {
                    if (columnType.Equals("System.DateTime"))
                    {
                        ShowDateValueSelector();
                    }
                    else if (columnType.Equals("System.Int16") || columnType.Equals("System.Int32") || columnType.Equals("System.Int64") || columnType.Equals("System.Single") || columnType.Equals("System.Double") || columnType.Equals("System.Decimal") || columnType.Equals("System.Byte") || columnType.Equals("System.SByte"))
                    {
                        ShowNumericValueSelector();
                    }
                    else if (columnType.Equals("System.Boolean"))
                    {
                        ShowDDLValueSelector();
                    }
                    else if (columnType.Equals("System.String"))
                    {
                        ShowTextValueSelector();
                    }
                }  // friendlyOperator == Missing or Not Missing, then don't show any of the value selectors
            }
            else  //cmbOperator.SelectedValue == null
            {
                dpValue.IsEnabled = false;
                tbxNumericValue.IsEnabled = false;
                cmbValue.IsEnabled = false;
                dpLowValue.IsEnabled = false;
                dpHighValue.IsEnabled = false;
                tbxLowValue.IsEnabled = false;
                tbxHighValue.IsEnabled = false;
                tbxValue.IsEnabled = false;

                tbxNumericValue.Clear();
                cmbValue.SelectedIndex = -1;
                tbxLowValue.Clear();
                tbxHighValue.Clear();
                tbxValue.Clear();
                return;
            }
        }



        /// <summary>
        /// Hides the value selection controls in order for only the correct controls to be shown.
        /// </summary>
        private void HideValueSelectors()
        {
            //Hiding single value controls
            txtValue.Visibility = Visibility.Hidden;
            dpValue.Visibility = Visibility.Hidden;
            tbxNumericValue.Visibility = Visibility.Hidden;
            cmbValue.Visibility = Visibility.Hidden;
            tbxValue.Visibility = Visibility.Hidden;

            //Hiding Lo and Hi value controls
            txtAnd.Visibility = Visibility.Hidden;
            txtHiValue.Visibility = Visibility.Hidden;
            txtLoValue.Visibility = Visibility.Hidden;
            dpLowValue.Visibility = Visibility.Hidden;
            dpHighValue.Visibility = Visibility.Hidden;
            tbxLowValue.Visibility = Visibility.Hidden;
            tbxHighValue.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Shows the appropriate value selection control for date type fields or columns
        /// </summary>
        private void ShowDateValueSelector()
        {
            txtValue.Visibility = Visibility.Visible;
            dpValue.IsEnabled = true;
            dpValue.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Shows the appropriate value selection control for date type fields or columns that are between a low and high value
        /// </summary>
        private void ShowDateLoHiSelector()
        {
            txtLoValue.Visibility = Visibility.Visible;
            txtAnd.Visibility = Visibility.Visible;
            txtHiValue.Visibility = Visibility.Visible;
            dpLowValue.IsEnabled = true;
            dpLowValue.Visibility = Visibility.Visible;
            dpHighValue.Visibility = Visibility.Visible;
            dpHighValue.IsEnabled = true;
        }

        /// <summary>
        /// Shows the appropriate value selection control for text type fields or columns
        /// </summary>
        private void ShowTextValueSelector()
        {
            txtValue.Visibility = Visibility.Visible;
            tbxValue.Visibility = Visibility.Visible;
            tbxValue.IsEnabled = true;
        }

        /// <summary>
        /// Shows the appropriate value selection control for text type fields or columns
        /// </summary>
        private void ShowNumericValueSelector()
        {
            txtValue.Visibility = Visibility.Visible;
            tbxNumericValue.Visibility = Visibility.Visible;
            tbxNumericValue.IsEnabled = true;
        }

        /// <summary>
        /// Shows the appropriate value selection controls for text type fields or columns that are between a low and high value
        /// </summary>
        private void ShowNumericLoHiSelector()
        {
            txtLoValue.Visibility = Visibility.Visible;
            txtAnd.Visibility = Visibility.Visible;
            txtHiValue.Visibility = Visibility.Visible;
            tbxLowValue.IsEnabled = true;
            tbxLowValue.Visibility = Visibility.Visible;
            tbxHighValue.Visibility = Visibility.Visible;
            tbxHighValue.IsEnabled = true;
        }

        /// <summary>
        /// Shows the appropriate value selection control for text type fields or columns
        /// </summary>
        private void ShowDDLValueSelector()
        {
            txtValue.Visibility = Visibility.Visible;
            cmbValue.IsEnabled = true;
            cmbValue.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Gets the proper value to use for the condition
        /// </summary>
        /// <returns>The value as a string</returns>
        private string GetValue()
        {
            string value = string.Empty;

            if (cmbOperator.SelectedValue.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING) || cmbOperator.SelectedValue.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_MISSING))
            {
                value = string.Empty;
            }
            else if (!cmbOperator.SelectedValue.ToString().Equals(SharedStrings.FRIENDLY_OPERATOR_BETWEEN))
            {
                if (dpValue.Visibility == Visibility.Visible)
                {
                    value = dpValue.Text;
                }
                else if (tbxNumericValue.Visibility == Visibility.Visible)
                {
                    value = tbxNumericValue.Text;
                }
                else if (cmbValue.Visibility == Visibility.Visible)
                {
                    if (cmbValue.SelectedItem != null)
                    {
                        value = cmbValue.SelectedItem.ToString();
                    }
                    else if (!string.IsNullOrEmpty(cmbValue.Text))
                    {
                        value = cmbValue.Text;
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
                if (dpLowValue.Visibility == Visibility.Visible)
                {
                    value = dpLowValue.Text + ";" + dpHighValue.Text;
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
            //if (pnlAdvancedMode.Visibility == System.Windows.Visibility.Visible)
            //{
            //    this.txtTitle.Text = SharedStrings.DATA_FILTERS;
            //}
            //else
            //{
            //    this.txtTitle.Text = SharedStrings.DATA_FILTERS + StringLiterals.SPACE + StringLiterals.PARANTHESES_OPEN +
            //        dgFilters.Items.Count.ToString() + StringLiterals.PARANTHESES_CLOSE;
            //}
        }

        /// <summary>
        /// Applies a filter from 'advanced' mode
        /// </summary>
        private void ApplyAdvancedModeFilter()
        {
            //DataTable dt = dashboardHelper.GenerateTopTwoTable();

            //dashboardHelper.AddSystemVariablesToTable(dt);
            //dashboardHelper.AddPermanentVariablesToTable(dt);            

            //try
            //{
            //    DataRow[] rows = dt.Select(txtAdvancedFilter.Text);
            //}
            //catch (Exception)
            //{
            //    txtAdvancedFilterStatus.Text = "The filter is not valid and has not been applied. Check the filter's syntax and try again.";
            //    txtAdvancedFilter.Foreground = this.Resources["invalidColor"] as Brush;
            //    return;
            //}

            //if (SelectionCriteriaChanged != null)
            //{
            //    dashboardHelper.AdvancedUserDataFilter = txtAdvancedFilter.Text;
            //    btnApplyAdvancedFilter.IsEnabled = false;
            //    SelectionCriteriaChanged(this, new EventArgs());
            //}

            //txtAdvancedFilter.Foreground = this.Resources["validColor"] as Brush;
            //txtAdvancedFilterStatus.Text = "The filter is valid and is now in effect.";

            //SetTitle();
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
    }    
}
