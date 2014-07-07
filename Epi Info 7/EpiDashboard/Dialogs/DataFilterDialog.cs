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
    public partial class DataFilterDialog : DialogBase
    {
        #region Private Members
        private DashboardHelper dashboardHelper;
        private FilterCondition filterCondition;
        private bool editMode;
        #endregion // Private Members

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper to attach</param>
        public DataFilterDialog(DashboardHelper dashboardHelper)
        {
            InEditMode = false;
            this.dashboardHelper = dashboardHelper;
            InitializeComponent();

            cbxOperand.SelectedIndex = 0;

            FillSelectionComboBoxes();

            //this.txtDestinationField.Text = string.Empty;            
        }

        /// <summary>
        /// Constructor used for editing an existing format rule
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper to attach</param>
        public DataFilterDialog(DashboardHelper dashboardHelper, FilterCondition filterCondition)
        {
            InEditMode = true;
            this.dashboardHelper = dashboardHelper;
            this.FilterCondition = filterCondition;
            InitializeComponent();

            FillSelectionComboBoxes();

            //this.txtDestinationField.Text = AssignRule.DestinationColumnName;            
            //this.txtDestinationField.Enabled = false;
        }
        #endregion Constructors

        #region Public Properties
        /// <summary>
        /// Gets the format rule
        /// </summary>
        public FilterCondition FilterCondition
        {
            get
            {
                return this.filterCondition;
            }
            private set
            {
                this.filterCondition = value;
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
        #endregion // Private Properties

        #region Private Methods
        /// <summary>
        /// Fills the combo boxes on this dialog
        /// </summary>
        private void FillSelectionComboBoxes()
        {            
            //txtDestinationField.Text = string.Empty;            
            cbxParam1.Items.Clear();
            cbxParam2.Items.Clear();

            if (cbxOperand.SelectedIndex >= 0 && cbxOperand.SelectedIndex <= 3)
            {
                ColumnDataType columnDataType = ColumnDataType.DateTime | ColumnDataType.UserDefined;                
                
                List<string> fieldNames1 = dashboardHelper.GetFieldsAsList(columnDataType);
                List<string> fieldNames2 = dashboardHelper.GetFieldsAsList(columnDataType);
                cbxParam1.DataSource = fieldNames1;
                cbxParam2.DataSource = fieldNames2;
            }

            //if (editMode)
            //{
            //    txtDestinationField.Enabled = false;
            //}
            //else
            //{
            //    txtDestinationField.Enabled = true;
            //}

            //cbxParam1.SelectedIndex = -1;
            //cbxParam2.SelectedIndex = -1;

            //if (AssignRule != null && AssignRule.AssignmentParameters != null)
            //{
            //    if (AssignRule.AssignmentParameters.Count > 0)
            //    {
            //        cbxParam1.Text = AssignRule.AssignmentParameters[0];
            //    }
            //    if (AssignRule.AssignmentParameters.Count > 1)
            //    {
            //        cbxParam2.Text = AssignRule.AssignmentParameters[1];
            //    }
            //}
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            //if (string.IsNullOrEmpty(txtDestinationField.Text))
            //{
            //    MsgBox.ShowError("Destination field is blank.");
            //    this.DialogResult = DialogResult.None;
            //    return;
            //}

            //if (cbxOperand.SelectedIndex < 0)
            //{
            //    MsgBox.ShowError("Assignment type is blank.");
            //    this.DialogResult = DialogResult.None;
            //    return;
            //}

            //if (cbxParam1.SelectedIndex < 0 || cbxParam2.SelectedIndex < 0)
            //{
            //    MsgBox.ShowError("One or more required parameters are blank.");
            //    this.DialogResult = DialogResult.None;
            //    return;
            //}

            //if (!editMode)
            //{
            //    ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.Numeric | ColumnDataType.Text;
            //    foreach (string s in dashboardHelper.GetFieldsAsList(columnDataType))
            //    {
            //        if (txtDestinationField.Text.ToLower().Equals(s.ToLower()))
            //        {
            //            MsgBox.ShowError("Destination field name already exists as a column in this data set. Please use another name.");
            //            this.DialogResult = DialogResult.None;
            //            return;
            //        }
            //    }

            //    foreach (IDashboardRule rule in dashboardHelper.Rules)
            //    {
            //        if (rule is DataAssignmentRule)
            //        {
            //            DataAssignmentRule assignmentRule = rule as DataAssignmentRule;
            //            if (txtDestinationField.Text.ToLower().Equals(assignmentRule.DestinationColumnName.ToLower()))
            //            {
            //                MsgBox.ShowError("Destination field name already exists as a defined field with recoded values. Please use another field name.");
            //                this.DialogResult = DialogResult.None;
            //                return;
            //            }
            //        }
            //    }
            //}

            //string friendlyLabel = "Assign to " + txtDestinationField.Text;

            //string param1 = cbxParam1.SelectedItem.ToString();
            //string param2 = cbxParam2.SelectedItem.ToString();

            //List<string> parameters = new List<string>();
            //parameters.Add(param1);
            //parameters.Add(param2);
            //SimpleAssignType assignmentType = SimpleAssignType.YearsElapsed;

            //switch (cbxOperand.SelectedItem.ToString())
            //{
            //    case "Difference in years":
            //        friendlyLabel = friendlyLabel + " the difference in years between " + param1 + " and " + param2;
            //        assignmentType = SimpleAssignType.YearsElapsed;
            //        break;
            //    case "Difference in months":
            //        friendlyLabel = friendlyLabel + " the difference in months between " + param1 + " and " + param2;
            //        assignmentType = SimpleAssignType.MonthsElapsed;
            //        break;
            //    case "Difference in days":
            //        friendlyLabel = friendlyLabel + " the difference in days between " + param1 + " and " + param2;
            //        assignmentType = SimpleAssignType.DaysElapsed;
            //        break;
            //    case "Difference in hours":
            //        friendlyLabel = friendlyLabel + " the difference in hours between " + param1 + " and " + param2;
            //        assignmentType = SimpleAssignType.HoursElapsed;
            //        break;
            //}
            
            //AssignRule = new Rule_SimpleAssign(this.dashboardHelper, friendlyLabel, txtDestinationField.Text, assignmentType, parameters);
            //this.DialogResult = DialogResult.OK;
            //this.Close();
        }
        #endregion // Private Methods
    }
}
