using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using EpiDashboard;
using Epi;
using Epi.Core;
using Epi.Fields;
using Epi.Windows;
using Epi.Windows.Dialogs;
using EpiDashboard.Rules;

namespace EpiDashboard.Dialogs
{
    public partial class SimpleAssignDialog : DialogBase 
    {
        #region Private Members
        private DashboardHelper dashboardHelper;
        private Rule_SimpleAssign assignRule;
        private bool editMode;        
        #endregion // Private Members

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper to attach</param>
        public SimpleAssignDialog(DashboardHelper dashboardHelper)
        {
            InEditMode = false;
            this.dashboardHelper = dashboardHelper;
            InitializeComponent();

            cbxAssignmentType.SelectedIndex = 0;

            FillSelectionComboBoxes();

            this.txtDestinationField.Text = string.Empty;
        }

        /// <summary>
        /// Constructor used for editing an existing format rule
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper to attach</param>
        public SimpleAssignDialog(DashboardHelper dashboardHelper, Rule_SimpleAssign assignRule)
        {
            InEditMode = true;
            this.dashboardHelper = dashboardHelper;
            this.AssignRule = assignRule;
            InitializeComponent();

            SimpleAssignType assignType = assignRule.AssignmentType;

            switch (assignType)
            {
                case SimpleAssignType.YearsElapsed:
                    cbxAssignmentType.SelectedIndex = 0;
                    break;
                case SimpleAssignType.MonthsElapsed:
                    cbxAssignmentType.SelectedIndex = 1;
                    break;
                case SimpleAssignType.DaysElapsed:
                    cbxAssignmentType.SelectedIndex = 2;
                    break;
                case SimpleAssignType.HoursElapsed:
                    cbxAssignmentType.SelectedIndex = 3;
                    break;
                case SimpleAssignType.MinutesElapsed:
                    cbxAssignmentType.SelectedIndex = 4;
                    break;
                case SimpleAssignType.Round:
                    cbxAssignmentType.SelectedIndex = 5;
                    break;
                case SimpleAssignType.TextToNumber:
                    cbxAssignmentType.SelectedIndex = 6;
                    break;
                case SimpleAssignType.StringLength:
                    cbxAssignmentType.SelectedIndex = 7;
                    break;
                case SimpleAssignType.FindText:
                    cbxAssignmentType.SelectedIndex = 8;
                    break;
                case SimpleAssignType.Substring:
                    cbxAssignmentType.SelectedIndex = 9;
                    break;
                case SimpleAssignType.Uppercase:
                    cbxAssignmentType.SelectedIndex = 10;
                    break;
                case SimpleAssignType.Lowercase:
                    cbxAssignmentType.SelectedIndex = 11;
                    break;
                case SimpleAssignType.AddDays:
                    cbxAssignmentType.SelectedIndex = 12;
                    break;
                case SimpleAssignType.DetermineNonExistantListValues:
                    cbxAssignmentType.SelectedIndex = 13;
                    break;
                case SimpleAssignType.CountCheckedCheckboxesInGroup:
                    cbxAssignmentType.SelectedIndex = 14;
                    break;
                case SimpleAssignType.CountYesMarkedYesNoFieldsInGroup:
                    cbxAssignmentType.SelectedIndex = 15;
                    break;
                case SimpleAssignType.DetermineCheckboxesCheckedInGroup:
                    cbxAssignmentType.SelectedIndex = 16;
                    break;
                case SimpleAssignType.DetermineYesMarkedYesNoFieldsInGroup:
                    cbxAssignmentType.SelectedIndex = 17;
                    break;
                case SimpleAssignType.CountNumericFieldsBetweenValuesInGroup:
                    cbxAssignmentType.SelectedIndex = 18;
                    break;
                case SimpleAssignType.CountNumericFieldsOutsideValuesInGroup:
                    cbxAssignmentType.SelectedIndex = 19;
                    break;
                case SimpleAssignType.FindSumNumericFieldsInGroup:
                    cbxAssignmentType.SelectedIndex = 20;
                    break;
                case SimpleAssignType.FindMeanNumericFieldsInGroup:
                    cbxAssignmentType.SelectedIndex = 21;
                    break;
                case SimpleAssignType.FindMaxNumericFieldsInGroup:
                    cbxAssignmentType.SelectedIndex = 22;
                    break;
                case SimpleAssignType.FindMinNumericFieldsInGroup:
                    cbxAssignmentType.SelectedIndex = 23;
                    break;
                case SimpleAssignType.CountFieldsWithMissingInGroup:
                    cbxAssignmentType.SelectedIndex = 24;
                    break;
                case SimpleAssignType.CountFieldsWithoutMissingInGroup:
                    cbxAssignmentType.SelectedIndex = 25;
                    break;
                case SimpleAssignType.DetermineFieldsWithMissingInGroup:
                    cbxAssignmentType.SelectedIndex = 26;
                    break;
                case SimpleAssignType.NumberToText:
                    cbxAssignmentType.SelectedIndex = 27;
                    break;
                case SimpleAssignType.StripDate:
                    cbxAssignmentType.SelectedIndex = 28;
                    break;
                case SimpleAssignType.TextToDate:
                    cbxAssignmentType.SelectedIndex = 29;
                    break;
                case SimpleAssignType.NumberToDate:
                    cbxAssignmentType.SelectedIndex = 30;
                    break;
            }

            FillSelectionComboBoxes();

            this.txtDestinationField.Text = AssignRule.DestinationColumnName;            
            this.txtDestinationField.Enabled = false;
        }
        #endregion Constructors

        #region Public Properties
        /// <summary>
        /// Gets the format rule
        /// </summary>
        public Rule_SimpleAssign AssignRule
        {
            get
            {
                return this.assignRule;
            }
            private set
            {
                this.assignRule = value;
            }
        }
        #endregion Public Properties

        #region Private Properties
        /// <summary>
        /// Gets the View associated with the attached dashboard helper
        /// </summary>
        private Epi.View View
        {
            get
            {
                return this.dashboardHelper.View;
            }
        }

        /// <summary>
        /// Gets whether or not the rule is being edited
        /// </summary>
        private bool InEditMode
        {
            get
            {
                return this.editMode;
            }
            set
            {
                this.editMode = value;
            }
        }

        private bool IsDateRange
        {
            get
            {
                if (cbxAssignmentType.SelectedIndex >= 0 && cbxAssignmentType.SelectedIndex <= 4)
                {
                    return true;
                }
                return false;
            }
        }
        #endregion // Private Properties

        #region Private Methods
        /// <summary>
        /// Fills the combo boxes on this dialog
        /// </summary>
        private void FillSelectionComboBoxes()
        {          
            cbxParam1.DataSource = null;
            cbxParam2.DataSource = null;
            cbxParam3.DataSource = null;

            cbxParam2.Items.Clear();
            cbxParam3.Items.Clear();

            ColumnDataType columnDataType = ColumnDataType.Numeric | ColumnDataType.UserDefined;
            List<string> fieldNames1 = dashboardHelper.GetFieldsAsList(columnDataType);
            List<string> fieldNames2 = dashboardHelper.GetFieldsAsList(columnDataType);

            if (cbxAssignmentType.SelectedIndex >= 0)
            {
                switch (cbxAssignmentType.SelectedIndex)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                        columnDataType = ColumnDataType.DateTime | ColumnDataType.UserDefined;                
                        fieldNames1 = dashboardHelper.GetFieldsAsList(columnDataType);
                        fieldNames2 = dashboardHelper.GetFieldsAsList(columnDataType);
                        cbxParam1.DataSource = fieldNames1;
                        cbxParam2.DataSource = fieldNames2;
                        break;
                    case 5:                    
                        columnDataType = ColumnDataType.Numeric | ColumnDataType.UserDefined;
                        fieldNames1 = dashboardHelper.GetFieldsAsList(columnDataType);
                        cbxParam1.DataSource = fieldNames1;
                        break;
                    case 6:
                    case 7:                    
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                        columnDataType = ColumnDataType.Text | ColumnDataType.UserDefined;
                        fieldNames1 = dashboardHelper.GetFieldsAsList(columnDataType);
                        cbxParam1.DataSource = fieldNames1;
                        break;
                    case 12:
                        columnDataType = ColumnDataType.DateTime | ColumnDataType.UserDefined;
                        fieldNames1 = dashboardHelper.GetFieldsAsList(columnDataType);
                        cbxParam1.DataSource = fieldNames1;
                        break;
                    case 13:
                    case 29:
                        columnDataType = ColumnDataType.Text | ColumnDataType.UserDefined;
                        fieldNames1 = dashboardHelper.GetFieldsAsList(columnDataType);
                        cbxParam1.DataSource = fieldNames1;
                        break;
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                    case 18:
                    case 19:                    
                    case 22:
                    case 23:
                    case 24:
                    case 25:
                    case 26:
                        fieldNames1 = dashboardHelper.GetAllGroupsAsList();
                        cbxParam1.DataSource = fieldNames1;
                        break;
                    case 20:
                    case 21:
                        fieldNames1 = dashboardHelper.GetAllGroupsAsList();
                        cbxParam1.DataSource = fieldNames1;
                        cbxParam2.Items.Add(dashboardHelper.Config.Settings.RepresentationOfYes);
                        cbxParam2.Items.Add(dashboardHelper.Config.Settings.RepresentationOfNo);
                        cbxParam3.Items.Add(dashboardHelper.Config.Settings.RepresentationOfYes);
                        cbxParam3.Items.Add(dashboardHelper.Config.Settings.RepresentationOfNo);
                        break;
                    case 27:
                        columnDataType = ColumnDataType.Numeric | ColumnDataType.UserDefined;
                        fieldNames1 = dashboardHelper.GetFieldsAsList(columnDataType);
                        cbxParam1.DataSource = fieldNames1;
                        break;
                    case 28:
                        columnDataType = ColumnDataType.DateTime | ColumnDataType.UserDefined;
                        fieldNames1 = dashboardHelper.GetFieldsAsList(columnDataType);
                        cbxParam1.DataSource = fieldNames1;
                        break;
                    case 30:
                        columnDataType = ColumnDataType.Numeric | ColumnDataType.UserDefined;
                        cbxParam1.DataSource = dashboardHelper.GetFieldsAsList(columnDataType);
                        cbxParam2.DataSource = dashboardHelper.GetFieldsAsList(columnDataType);
                        cbxParam3.DataSource = dashboardHelper.GetFieldsAsList(columnDataType);
                        break; 
                }
            }

            if (editMode)
            {
                txtDestinationField.Enabled = false;
            }
            else
            {
                txtDestinationField.Enabled = true;
            }

            cbxParam1.SelectedIndex = -1;
            cbxParam2.SelectedIndex = -1;
            cbxParam3.SelectedIndex = -1;

            if (AssignRule != null && AssignRule.AssignmentParameters != null)
            {
                if (AssignRule.AssignmentParameters.Count > 0)
                {
                    cbxParam1.Text = AssignRule.AssignmentParameters[0];
                }
                if (AssignRule.AssignmentParameters.Count > 1)
                {
                    cbxParam2.Text = AssignRule.AssignmentParameters[1];
                }
                if (AssignRule.AssignmentParameters.Count > 2)
                {
                    cbxParam3.Text = AssignRule.AssignmentParameters[2];
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtDestinationField.Text))
            {
                MsgBox.ShowError(SimpleAssignmentStrings.ERROR_DESTINATION_FIELD_MISSING);
                this.DialogResult = DialogResult.None;
                return;
            }

            if (cbxAssignmentType.SelectedIndex < 0)
            {
                MsgBox.ShowError(SimpleAssignmentStrings.ERROR_TYPE_MISSING);
                this.DialogResult = DialogResult.None;
                return;
            }

            if (
                (cbxParam1.Visible == true && string.IsNullOrEmpty(cbxParam1.Text))
                || (cbxParam2.Visible == true && string.IsNullOrEmpty(cbxParam2.Text))
                || (cbxParam3.Visible == true && string.IsNullOrEmpty(cbxParam3.Text))
                )
            {
                MsgBox.ShowError(SimpleAssignmentStrings.ERROR_PARAMS_BLANK);
                this.DialogResult = DialogResult.None;
                return;
            }

            bool overwritesPermanentField = false;
            if (this.AssignRule != null)
            {
                overwritesPermanentField = this.AssignRule.OverwritesPermanentField;
            }

            if (!editMode)
            {
                ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.Numeric | ColumnDataType.Text;                

                foreach (IDashboardRule rule in dashboardHelper.Rules)
                {
                    if (rule is DataAssignmentRule)
                    {
                        DataAssignmentRule assignmentRule = rule as DataAssignmentRule;
                        if (txtDestinationField.Text.ToLower().Equals(assignmentRule.DestinationColumnName.ToLower()))
                        {
                            MsgBox.ShowError(SimpleAssignmentStrings.ERROR_FIELD_ALREADY_EXISTS_WITH_RECODED_DATA);
                            this.DialogResult = DialogResult.None;
                            return;
                        }
                    }
                }

                foreach (string s in dashboardHelper.GetFieldsAsList(columnDataType))
                {
                    if (txtDestinationField.Text.ToLower().Equals(s.ToLower()))
                    {
                        System.Windows.Forms.DialogResult result = MsgBox.ShowQuestion(SimpleAssignmentStrings.OVERWRITE_FIELD_DATA);
                        if (result != System.Windows.Forms.DialogResult.Yes && result != System.Windows.Forms.DialogResult.OK)
                        {
                            this.DialogResult = DialogResult.None;
                            return;
                        }
                        else
                        {
                            overwritesPermanentField = true;
                        }
                    }
                }
            }

            string friendlyLabel = "Assign " + txtDestinationField.Text;

            string param1 = cbxParam1.Text.ToString();
            string param2 = cbxParam2.Text.ToString();
            string param3 = cbxParam3.Text.ToString();

            List<string> parameters = new List<string>();
            parameters.Add(param1);
            if (!string.IsNullOrEmpty(param2)) { parameters.Add(param2); }
            if (!string.IsNullOrEmpty(param3)) { parameters.Add(param3); }
            SimpleAssignType assignmentType = SimpleAssignType.YearsElapsed;

            //switch (cbxAssignmentType.SelectedItem.ToString())
            switch (cbxAssignmentType.SelectedIndex)
            {
                case 0: //"Difference in years":
                    friendlyLabel = friendlyLabel + " the difference in years between " + param1 + " and " + param2;
                    assignmentType = SimpleAssignType.YearsElapsed;
                    break;
                case 1: //"Difference in months":
                    friendlyLabel = friendlyLabel + " the difference in months between " + param1 + " and " + param2;
                    assignmentType = SimpleAssignType.MonthsElapsed;
                    break;
                case 2: //"Difference in days":
                    friendlyLabel = friendlyLabel + " the difference in days between " + param1 + " and " + param2;
                    assignmentType = SimpleAssignType.DaysElapsed;
                    break;
                case 3: //"Difference in hours":
                    friendlyLabel = friendlyLabel + " the difference in hours between " + param1 + " and " + param2;
                    assignmentType = SimpleAssignType.HoursElapsed;
                    break;
                case 4: //"Difference in minutes":
                    friendlyLabel = friendlyLabel + " the difference in minutes between " + param1 + " and " + param2;
                    assignmentType = SimpleAssignType.MinutesElapsed;
                    break;
                case 5: //"Round a number":
                    friendlyLabel = friendlyLabel + " the rounded value of " + param1;
                    if (!string.IsNullOrEmpty(param2))
                    {
                        friendlyLabel = friendlyLabel + " to " + param2 + " decimal place(s)";
                    }
                    assignmentType = SimpleAssignType.Round;
                    break;
                case 6: //"Convert text data to numeric data":
                    friendlyLabel = friendlyLabel + " the numeric representation of " + param1;
                    assignmentType = SimpleAssignType.TextToNumber;
                    break;
                case 7: //"Find the length of text data":
                    friendlyLabel = friendlyLabel + " the length of the text contained in " + param1;
                    assignmentType = SimpleAssignType.StringLength;
                    break;
                case 8: //"Find the location of text data":
                    friendlyLabel = friendlyLabel + " the starting location of the text " + param2 + " contained in " + param1;
                    assignmentType = SimpleAssignType.FindText;
                    break;
                case 9: //"Substring":                    
                    friendlyLabel = friendlyLabel + " the portion of the text contained in " + param1 + " starting at position " + param2 + " and continuing for " + param3 + " characters";
                    assignmentType = SimpleAssignType.Substring;
                    break;

                // New ones added after 7.0.9.48
                case 10: //"Convert text characters to uppercase":
                    friendlyLabel = friendlyLabel + " the upper case equivalent of " + param1;
                    assignmentType = SimpleAssignType.Uppercase;
                    break;
                case 11: //"Convert text characters to lower":
                    friendlyLabel = friendlyLabel + " the lower case equivalent of " + param1;
                    assignmentType = SimpleAssignType.Lowercase;
                    break;

                // New ones added after 7.0.9.51
                case 12: //"Add days to a date field":
                    friendlyLabel = friendlyLabel + " the date value in " + param1 + " and add " + param2 + " days";
                    assignmentType = SimpleAssignType.AddDays;
                    break;
                case 13: //"Determine if a drop-down list field contains a value not present in its code table":
                    friendlyLabel = friendlyLabel + " a Yes if the value in " + param1 + " appears in its corresponding code table";
                    assignmentType = SimpleAssignType.DetermineNonExistantListValues;
                    break;
                case 14: //"Count the number of checked checkboxes in a group":
                    friendlyLabel = friendlyLabel + " the number of checked checkboxes in " + param1 + " (group field)";
                    assignmentType = SimpleAssignType.CountCheckedCheckboxesInGroup;
                    break;
                case 15: //"Count the number of Yes-marked Yes/No fields in a group":
                    friendlyLabel = friendlyLabel + " the number of Yes-marked Yes/No fields in " + param1 + " (group field)";
                    assignmentType = SimpleAssignType.CountYesMarkedYesNoFieldsInGroup;
                    break;
                case 16: //"Determine if more than N checkboxes are checked in a group":
                    friendlyLabel = friendlyLabel + " a Yes if more than " + param2 + " checkboxes are checked in " + param1 + " (group field)";
                    assignmentType = SimpleAssignType.DetermineCheckboxesCheckedInGroup;
                    break;
                case 17: //"Determine if more than N Yes/No fields are marked Yes in a group":
                    friendlyLabel = friendlyLabel + " a Yes if more than " + param2 + " Yes/No fields are marked Yes in " + param1 + " (group field)";
                    assignmentType = SimpleAssignType.DetermineYesMarkedYesNoFieldsInGroup;
                    break;
                case 18: //"Count the number of numeric fields with values between X and Y in a group":
                    friendlyLabel = friendlyLabel + " the number of numeric fields with values between (inclusive) " + param2 + " and " + param3 + " in " + param1 + " (group field)";
                    assignmentType = SimpleAssignType.CountNumericFieldsBetweenValuesInGroup;
                    break;
                case 19: //"Count the number of numeric fields with values outside X and Y in a group":
                    friendlyLabel = friendlyLabel + " the number of numeric fields with values outside " + param2 + " and " + param3 + " in " + param1 + " (group field)";
                    assignmentType = SimpleAssignType.CountNumericFieldsOutsideValuesInGroup;
                    break;
                case 20: //"Find the sum of all numeric fields in a group":
                    friendlyLabel = friendlyLabel + " the sum of all numeric fields in " + param1 + " (group field).";
                    if (param2 == dashboardHelper.Config.Settings.RepresentationOfYes) friendlyLabel = friendlyLabel + " Include Yes/No fields.";
                    else friendlyLabel = friendlyLabel + " Do not include Yes/No fields.";
                    if (param3 == dashboardHelper.Config.Settings.RepresentationOfYes) friendlyLabel = friendlyLabel + " Include Comment Legal fields.";
                    else friendlyLabel = friendlyLabel + " Do not include Comment Legal fields.";
                    assignmentType = SimpleAssignType.FindSumNumericFieldsInGroup;
                    break;
                case 21: //"Find the mean of all numeric fields in a group":
                    friendlyLabel = friendlyLabel + " the mean of all numeric fields in " + param1 + " (group field).";
                    if (param2 == dashboardHelper.Config.Settings.RepresentationOfYes) friendlyLabel = friendlyLabel + " Include Yes/No fields.";
                    else friendlyLabel = friendlyLabel + " Do not include Yes/No fields.";
                    if (param3 == dashboardHelper.Config.Settings.RepresentationOfYes) friendlyLabel = friendlyLabel + " Include Comment Legal fields.";
                    else friendlyLabel = friendlyLabel + " Do not include Comment Legal fields.";
                    assignmentType = SimpleAssignType.FindMeanNumericFieldsInGroup;
                    break;
                case 22: //"Find the maximum value of all numeric fields in a group":
                    friendlyLabel = friendlyLabel + " the maximum numeric value in " + param1 + " (group field)";
                    assignmentType = SimpleAssignType.FindMaxNumericFieldsInGroup;
                    break;
                case 23: //"Find the minimum value of all numeric fields in a group":
                    friendlyLabel = friendlyLabel + " the minimum numeric value in " + param1 + " (group field)";
                    assignmentType = SimpleAssignType.FindMinNumericFieldsInGroup;
                    break;
                case 24: //"Count the number of fields with missing values in a group":
                    friendlyLabel = friendlyLabel + " the number of fields with missing values in " + param1 + " (group field)";
                    assignmentType = SimpleAssignType.CountFieldsWithMissingInGroup;
                    break;
                case 25: //"Count the number of fields without missing values in a group":
                    friendlyLabel = friendlyLabel + " the number of fields without missing values in " + param1 + " (group field)";
                    assignmentType = SimpleAssignType.CountFieldsWithoutMissingInGroup;
                    break;
                case 26: //"Determine if more than N fields have missing values in a group":
                    friendlyLabel = friendlyLabel + " a Yes if more than " + param2 + " fields are missing in " + param1 + " (group field)";
                    assignmentType = SimpleAssignType.DetermineFieldsWithMissingInGroup;
                    break;
                case 27: //"Convert numeric data to text data":
                    friendlyLabel = friendlyLabel + " the text representation of " + param1;
                    assignmentType = SimpleAssignType.NumberToText;
                    break;
                case 28: //"Strip date":
                    friendlyLabel = friendlyLabel + " the date component of " + param1;
                    assignmentType = SimpleAssignType.StripDate;
                    break;
                case 29: //"Convert text data to date data":
                    friendlyLabel = friendlyLabel + " the date representation of " + param1;
                    assignmentType = SimpleAssignType.TextToDate;
                    break;
                case 30: //"Convert numeric data to date data":
                    friendlyLabel = friendlyLabel + " the date representation of " + param1 + "(day) and " + param2 + "(month) and " + param3 + "(year)";
                    assignmentType = SimpleAssignType.NumberToDate;
                    break;
            }
            
            AssignRule = new Rule_SimpleAssign(this.dashboardHelper, friendlyLabel, txtDestinationField.Text, assignmentType, parameters);
            AssignRule.OverwritesPermanentField = overwritesPermanentField;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Adds descriptive text to the dialog box
        /// </summary>
        private void AddDescription()
        {
            switch (cbxAssignmentType.SelectedIndex)
            {
                case 0:
                    txtDescription.Text = SimpleAssignmentStrings.YEARS_DESCRIPTION;
                    break;
                case 1:
                    txtDescription.Text = SimpleAssignmentStrings.MONTHS_DESCRIPTION;
                    break;
                case 2:
                    txtDescription.Text = SimpleAssignmentStrings.DAYS_DESCRIPTION;
                    break;
                case 3:
                    txtDescription.Text = SimpleAssignmentStrings.HOURS_DESCRIPTION;
                    break;
                case 4:
                    txtDescription.Text = SimpleAssignmentStrings.MINUTES_DESCRIPTION;
                    break;
                case 5:
                    txtDescription.Text = SimpleAssignmentStrings.ROUND_DESCRIPTION;
                    break;
                case 6: // "Convert text data to numeric data":
                    txtDescription.Text = SimpleAssignmentStrings.TEXT_TO_NUMBER_DESCRIPTION;
                    break;
                case 7: // "Find the length of text data":
                    txtDescription.Text = SimpleAssignmentStrings.STRLEN_DESCRIPTION;
                    break;
                case 8:
                    txtDescription.Text = SimpleAssignmentStrings.FINDTEXT_DESCRIPTION;
                    break;
                case 9:
                    txtDescription.Text = SimpleAssignmentStrings.SUBSTRING_DESCRIPTION;
                    break;
                case 10: // "Convert text characters to uppercase":
                    txtDescription.Text = SimpleAssignmentStrings.UPPERCASE_DESCRIPTION;
                    break;
                case 11: // "Convert text characters to lowercase":
                    txtDescription.Text = SimpleAssignmentStrings.LOWERCASE_DESCRIPTION;
                    break;
                case 12:
                    txtDescription.Text = SimpleAssignmentStrings.ADD_DAYS_DESCRIPTION;
                    break;
                case 13:
                    txtDescription.Text = SimpleAssignmentStrings.DDL_CHECK_DESCRIPTION;
                    break;
                case 14:
                    txtDescription.Text = SimpleAssignmentStrings.COUNT_CHECKED_CHECKBOXES_DESCRIPTION;
                    break;
                case 15:
                    txtDescription.Text = SimpleAssignmentStrings.COUNT_YES_YESNOFIELDS_DESCRIPTION;
                    break;
                case 16:
                    txtDescription.Text = SimpleAssignmentStrings.DETERMINE_IF_N_CHECKBOXES_CHECKED_DESCRIPTION;
                    break;
                case 17:
                    txtDescription.Text = SimpleAssignmentStrings.DETERMINE_IF_N_YESNOFIELDS_DESCRIPTION;
                    break;
                case 18:
                    txtDescription.Text = SimpleAssignmentStrings.COUNT_NUMERIC_BETWEEN_X_Y_DESCRIPTION;
                    break;
                case 19:
                    txtDescription.Text = SimpleAssignmentStrings.COUNT_NUMERIC_OUTSIDE_X_Y_DESCRIPTION;
                    break;
                case 20:
                    txtDescription.Text = SimpleAssignmentStrings.SUM_NUMERIC_FIELDS_DESCRIPTION;
                    break;
                case 21:
                    txtDescription.Text = SimpleAssignmentStrings.AVERAGE_NUMERIC_FIELDS_DESCRIPTION;
                    break;
                case 22:
                    txtDescription.Text = SimpleAssignmentStrings.MAX_NUMERIC_FIELDS_DESCRIPTION;
                    break;
                case 23:
                    txtDescription.Text = SimpleAssignmentStrings.MIN_NUMERIC_FIELDS_DESCRIPTION;
                    break;
                case 24:
                    txtDescription.Text = SimpleAssignmentStrings.COUNT_MISSING_DESCRIPTION;
                    break;
                case 25:
                    txtDescription.Text = SimpleAssignmentStrings.COUNT_NOT_MISSING_DESCRIPTION;
                    break;
                case 26:
                    txtDescription.Text = SimpleAssignmentStrings.DETERMINE_IF_N_MISSING_DESCRIPTION;
                    break;
                case 27:
                    txtDescription.Text = SimpleAssignmentStrings.NUMBER_TO_TEXT_DESCRIPTION;
                    break;
                case 28:
                    txtDescription.Text = SimpleAssignmentStrings.STRIP_DATE_DESCRIPTION;
                    break;
                case 29: //"Convert text data to date data":
                    txtDescription.Text = SimpleAssignmentStrings.TEXT_TO_DATE_DESCRIPTION;
                    break;
                case 30: //"Convert text data to date data":
                    txtDescription.Text = SimpleAssignmentStrings.NUMBER_TO_DATE_DESCRIPTION;
                    break;
            }
        }
        #endregion // Private Methods

        #region Event Handlers
        /// <summary>
        /// Handles the SelectedIndexChanged event for the assign type drop-down list
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void cbxAssignmentType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cbxAssignmentType.SelectedIndex == -1) 
            {
                return;
            }

            //switch (cbxAssignmentType.SelectedItem.ToString())
            switch (cbxAssignmentType.SelectedIndex)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                    cbxParam1.Visible = true;
                    cbxParam2.Visible = true;
                    cbxParam3.Visible = false;

                    lblParam1.Visible = true;
                    lblParam2.Visible = true;
                    lblParam3.Visible = false;

                    cbxParam3.Text = string.Empty;
                    lblParam1.Text = SimpleAssignmentStrings.PARAM_START_DATE;
                    lblParam2.Text = SimpleAssignmentStrings.PARAM_END_DATE;
                    break;
                case 8: // "Find the location of text data":
                    cbxParam1.Visible = true;
                    cbxParam2.Visible = true;
                    cbxParam3.Visible = false;

                    lblParam1.Visible = true;
                    lblParam2.Visible = true;
                    lblParam3.Visible = false;

                    cbxParam3.Text = string.Empty;
                    lblParam1.Text = SimpleAssignmentStrings.PARAM_TEXT_FIELD_TO_SEARCH;
                    lblParam2.Text = SimpleAssignmentStrings.PARAM_TEXT_TO_SEARCH_FOR;
                    break;
                case 5: // "Round a number":
                    cbxParam1.Visible = true;
                    cbxParam2.Visible = true;
                    cbxParam3.Visible = false;

                    lblParam1.Visible = true;
                    lblParam2.Visible = true;
                    lblParam3.Visible = false;

                    cbxParam3.Text = string.Empty;
                    lblParam1.Text = SimpleAssignmentStrings.PARAM_NUMERIC_FIELD_TO_ROUND;
                    lblParam2.Text = SimpleAssignmentStrings.PARAM_ROUND_TO;
                    break;
                case 9: // "Substring":
                    cbxParam1.Visible = true;
                    cbxParam2.Visible = true;
                    cbxParam3.Visible = true;

                    lblParam1.Visible = true;
                    lblParam2.Visible = true;
                    lblParam3.Visible = true;

                    lblParam1.Text = SimpleAssignmentStrings.PARAM_TEXT_FIELD;
                    lblParam2.Text = SimpleAssignmentStrings.PARAM_FIRST_CHARACTER;
                    lblParam3.Text = SimpleAssignmentStrings.PARAM_NUMBER_OF_CHARACTERS;
                    break;
                case 6: // "Convert text data to numeric data":
                case 7: // "Find the length of text data":                
                case 29: // "Convert text data to date data":   
                    cbxParam1.Visible = true;
                    cbxParam2.Visible = false;
                    cbxParam3.Visible = false;

                    lblParam1.Visible = true;
                    lblParam2.Visible = false;
                    lblParam3.Visible = false;

                    cbxParam2.Text = string.Empty;
                    cbxParam3.Text = string.Empty;
                    lblParam1.Text = SimpleAssignmentStrings.PARAM_TEXT_FIELD;
                    break;
                case 10: // "Convert text characters to uppercase":
                case 11: // "Convert text characters to lowercase":
                    cbxParam1.Visible = true;
                    cbxParam2.Visible = false;
                    cbxParam3.Visible = false;

                    lblParam1.Visible = true;
                    lblParam2.Visible = false;
                    lblParam3.Visible = false;

                    lblParam1.Text = SimpleAssignmentStrings.PARAM_TEXT_FIELD;
                    break;

                // New ones added
                case 12:                 
                    cbxParam1.Visible = true;
                    cbxParam2.Visible = true;
                    cbxParam3.Visible = false;

                    lblParam1.Visible = true;
                    lblParam2.Visible = true;
                    lblParam3.Visible = false;

                    lblParam1.Text = SimpleAssignmentStrings.PARAM_DATE_FIELD;
                    lblParam2.Text = SimpleAssignmentStrings.PARAM_DAYS_TO_ADD;
                    break;

                case 13:
                    cbxParam1.Visible = true;
                    cbxParam2.Visible = false;
                    cbxParam3.Visible = false;

                    lblParam1.Visible = true;
                    lblParam2.Visible = false;
                    lblParam3.Visible = false;

                    lblParam1.Text = SimpleAssignmentStrings.PARAM_DDL_FIELD;
                    break;

                case 14:
                case 15:                
                case 22:
                case 23:
                case 24:
                case 25:
                    cbxParam1.Visible = true;
                    cbxParam2.Visible = false;
                    cbxParam3.Visible = false;

                    lblParam1.Visible = true;
                    lblParam2.Visible = false;
                    lblParam3.Visible = false;

                    lblParam1.Text = SimpleAssignmentStrings.PARAM_GROUP_FIELD;
                    break;
                case 20:
                case 21:
                    cbxParam1.Visible = true;
                    cbxParam2.Visible = true;
                    cbxParam3.Visible = true;

                    lblParam1.Visible = true;
                    lblParam2.Visible = true;
                    lblParam3.Visible = true;

                    lblParam1.Text = SimpleAssignmentStrings.PARAM_GROUP_FIELD;
                    lblParam2.Text = SimpleAssignmentStrings.PARAM_INCLUDE_YESNO;
                    lblParam3.Text = SimpleAssignmentStrings.PARAM_INCLUDE_COMMENTLEGAL;
                    break;
                case 16:
                case 17:
                case 26:
                    cbxParam1.Visible = true;
                    cbxParam2.Visible = true;
                    cbxParam3.Visible = false;

                    lblParam1.Visible = true;
                    lblParam2.Visible = true;
                    lblParam3.Visible = false;

                    lblParam1.Text = SimpleAssignmentStrings.PARAM_GROUP_FIELD;
                    lblParam2.Text = SimpleAssignmentStrings.PARAM_COUNT;
                    break;

                case 18:
                case 19:
                    cbxParam1.Visible = true;
                    cbxParam2.Visible = true;
                    cbxParam3.Visible = true;

                    lblParam1.Visible = true;
                    lblParam2.Visible = true;
                    lblParam3.Visible = true;

                    lblParam1.Text = SimpleAssignmentStrings.PARAM_GROUP_FIELD;
                    lblParam2.Text = SimpleAssignmentStrings.PARAM_LOWER_BOUND;
                    lblParam3.Text = SimpleAssignmentStrings.PARAM_UPPER_BOUND;
                    break;

                case 27: // "Convert numeric data to text data":
                    cbxParam1.Visible = true;
                    cbxParam2.Visible = false;
                    cbxParam3.Visible = false;

                    lblParam1.Visible = true;
                    lblParam2.Visible = false;
                    lblParam3.Visible = false;

                    lblParam1.Text = SimpleAssignmentStrings.PARAM_NUMBER_FIELD;
                    break;

                case 28: // Strip a date
                    cbxParam1.Visible = true;
                    cbxParam2.Visible = false;
                    cbxParam3.Visible = false;

                    lblParam1.Visible = true;
                    lblParam2.Visible = false;
                    lblParam3.Visible = false;

                    lblParam1.Text = SimpleAssignmentStrings.PARAM_DATE_FIELD;
                    break;

                case 30:
                    cbxParam1.Visible = true;
                    cbxParam2.Visible = true;
                    cbxParam3.Visible = true;

                    lblParam1.Visible = true;
                    lblParam2.Visible = true;
                    lblParam3.Visible = true;

                    lblParam1.Text = SimpleAssignmentStrings.DAY_NUMERIC;
                    lblParam2.Text = SimpleAssignmentStrings.MONTH_NUMERIC;
                    lblParam3.Text = SimpleAssignmentStrings.YEAR_NUMERIC;
                    break;

                //case "Difference in years":
                //case "Difference in months":
                //case "Difference in days":
                //case "Difference in hours":
                //case "Difference in minutes":                
                //    cbxParam1.Visible = true;
                //    cbxParam2.Visible = true;
                //    cbxParam3.Visible = false;

                //    lblParam1.Visible = true;
                //    lblParam2.Visible = true;
                //    lblParam3.Visible = false;

                //    cbxParam3.Text = string.Empty;
                //    lblParam1.Text = "Start date:";
                //    lblParam2.Text = "End date:";
                //    break;
                //case "Find the location of text data":                
                //    cbxParam1.Visible = true;
                //    cbxParam2.Visible = true;
                //    cbxParam3.Visible = false;

                //    lblParam1.Visible = true;
                //    lblParam2.Visible = true;
                //    lblParam3.Visible = false;

                //    cbxParam3.Text = string.Empty;
                //    lblParam1.Text = "Text field to search:";
                //    lblParam2.Text = "The text string to search for:";
                //    break;
                //case "Round a number":
                //    cbxParam1.Visible = true;
                //    cbxParam2.Visible = true;
                //    cbxParam3.Visible = false;

                //    lblParam1.Visible = true;
                //    lblParam2.Visible = true;
                //    lblParam3.Visible = false;

                //    cbxParam3.Text = string.Empty;
                //    lblParam1.Text = "The numeric field to round:";
                //    lblParam2.Text = "To number of decimal places to round to:";
                //    break;
                //case "Substring":
                //    cbxParam1.Visible = true;
                //    cbxParam2.Visible = true;
                //    cbxParam3.Visible = true;

                //    lblParam1.Visible = true;
                //    lblParam2.Visible = true;
                //    lblParam3.Visible = true;
                    
                //    lblParam1.Text = "Text field:";
                //    lblParam2.Text = "Position of the first character to extract:";
                //    lblParam3.Text = "Number of characters to extract:";
                //    break;
                //case "Convert text data to numeric data":
                //case "Find the length of text data":
                //    cbxParam1.Visible = true;
                //    cbxParam2.Visible = false;
                //    cbxParam3.Visible = false;

                //    lblParam1.Visible = true;
                //    lblParam2.Visible = false;
                //    lblParam3.Visible = false;

                //    cbxParam2.Text = string.Empty;
                //    cbxParam3.Text = string.Empty;
                //    lblParam1.Text = "Text field:";                    
                //    break;
                //case "Convert text characters to uppercase":
                //case "Convert text characters to lowercase":
                //    cbxParam1.Visible = true;
                //    cbxParam2.Visible = false;
                //    cbxParam3.Visible = false;

                //    lblParam1.Visible = true;
                //    lblParam2.Visible = false;
                //    lblParam3.Visible = false;

                //    lblParam1.Text = "Text field:";
                //    break;
            }

            AddDescription();

            FillSelectionComboBoxes();
        }
        #endregion // Event Handlers
    }
}
