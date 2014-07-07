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
    public partial class ExpressionAssignDialog : DialogBase
    {
        #region Private Members
        private DashboardHelper dashboardHelper;
        private Rule_ExpressionAssign assignRule;        
        private bool editMode;
        #endregion // Private Members

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper to attach</param>
        public ExpressionAssignDialog(DashboardHelper dashboardHelper)
        {
            InEditMode = false;
            this.dashboardHelper = dashboardHelper;
            InitializeComponent();
            FillSelectionComboboxes();

            this.txtDestinationField.Text = string.Empty;
            this.txtExpression.Text = string.Empty;
            cbxDataType.SelectedIndex = 0;
        }

        /// <summary>
        /// Constructor used for editing an existing format rule
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper to attach</param>
        public ExpressionAssignDialog(DashboardHelper dashboardHelper, Rule_ExpressionAssign assignRule)
        {
            InEditMode = true;
            this.dashboardHelper = dashboardHelper;
            this.AssignRule = assignRule;
            InitializeComponent();
            FillSelectionComboboxes();

            this.txtDestinationField.Text = AssignRule.DestinationColumnName;
            this.txtExpression.Text = AssignRule.Expression;
            this.txtDestinationField.Enabled = false;

            if (assignRule.DestinationColumnType.Equals("System.String"))
            {
                cbxDataType.SelectedIndex = 1;
            }
            else
            {
                cbxDataType.SelectedIndex = 0;
            }
        }
        #endregion Constructors

        #region Public Properties
        /// <summary>
        /// Gets the format rule
        /// </summary>
        public Rule_ExpressionAssign AssignRule
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
        #endregion // Private Properties

        #region Private Methods
        /// <summary>
        /// Fills in the Field Names combo box
        /// </summary>
        private void FillSelectionComboboxes()
        {
        }

        /// <summary>
        /// Method used to test if the expression works
        /// </summary>
        /// <returns>Bool representing whether or not the expression is valid</returns>
        private bool TestExpression()
        {
            return false;
        }
        #endregion // Private Methods

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtDestinationField.Text))
            {
                MsgBox.ShowError(DashboardSharedStrings.EXPRESSION_ASSIGN_DESTINATION_FIELD_BLANK);
                this.DialogResult = DialogResult.None;
                return;
            }

            if (!editMode)
            {
                foreach (string s in dashboardHelper.GetFieldsAsList())
                {
                    if (txtDestinationField.Text.ToLower().Equals(s.ToLower()))
                    {
                        MsgBox.ShowError(DashboardSharedStrings.EXPRESSION_ASSIGN_DESTINATION_FIELD_ALREADY_EXISTS);
                        this.DialogResult = DialogResult.None;
                        return;
                    }
                }

                foreach (IDashboardRule rule in dashboardHelper.Rules)
                {
                    if (rule is DataAssignmentRule)
                    {
                        DataAssignmentRule assignmentRule = rule as DataAssignmentRule;
                        if (txtDestinationField.Text.ToLower().Equals(assignmentRule.DestinationColumnName.ToLower()))
                        {
                            MsgBox.ShowError(DashboardSharedStrings.EXPRESSION_ASSIGN_DESTINATION_FIELD_ALREADY_EXISTS_AS_RECODED);
                            this.DialogResult = DialogResult.None;
                            return;
                        }
                    }
                }
            }

            if (cbxDataType.Text.Equals("Text"))
            {
                AssignRule = new Rule_ExpressionAssign(this.dashboardHelper, "Assign " + txtDestinationField.Text + " the expression: " + txtExpression.Text, txtDestinationField.Text, "System.String", txtExpression.Text);
            }
            else if (cbxDataType.Text.Equals("Date/Time"))
            {
                AssignRule = new Rule_ExpressionAssign(this.dashboardHelper, "Assign " + txtDestinationField.Text + " the expression: " + txtExpression.Text, txtDestinationField.Text, "System.DateTime", txtExpression.Text);
            }   
            else
            {
                AssignRule = new Rule_ExpressionAssign(this.dashboardHelper, "Assign " + txtDestinationField.Text + " the expression: " + txtExpression.Text, txtDestinationField.Text, txtExpression.Text);
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
