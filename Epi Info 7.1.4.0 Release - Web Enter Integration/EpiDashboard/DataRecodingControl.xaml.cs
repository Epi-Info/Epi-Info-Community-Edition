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
using EpiDashboard.Rules;

namespace EpiDashboard
{    
    /// <summary>
    /// Interaction logic for DataRecodingControl.xaml
    /// </summary>
    public partial class DataRecodingControl : UserControl
    {
        #region Private Members
        private DashboardHelper dashboardHelper;
        #endregion // Private Members

        #region Public Events
        public event EventHandler UserVariableAdded;
        public event EventHandler UserVariableRemoved;
        public event EventHandler UserVariableChanged;
        public event GadgetClosingHandler GadgetClosing;
        #endregion // Public Events

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view">The view to attach</param>
        /// <param name="db">The database to attach</param>
        /// <param name="dashboardHelper">The dashboard helper to attach</param>
        public DataRecodingControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.dashboardHelper = dashboardHelper;
            txtTitle.RenderTransform = new RotateTransform(90);
            UpdateRules();
        }
        #endregion // Constructors
        
        public void MinimizeGadget()
        {
            ConfigGrid.Height = 50;
            //triangleCollapsed = true;
        }

        #region Event Handlers
        void imgClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (GadgetClosing != null)
                GadgetClosing(this);
        }

        private void btnRemoveRule_Click(object sender, RoutedEventArgs e)
        {
            if (lbxRules.SelectedItem != null)
            {                
                dashboardHelper.RemoveRule(lbxRules.SelectedItem.ToString());
            }
            UpdateRules();
            if (UserVariableRemoved != null)
            {
                UserVariableRemoved(this, new EventArgs());
            }
        }

        private void btnClearConditions_Click(object sender, RoutedEventArgs e)
        {
            dashboardHelper.ClearRules();
            UpdateRules();
        }
        #endregion // Event Handlers

        #region Private Methods
        private void UpdateRules()
        {
            lbxRules.Items.Clear();
            foreach (IDashboardRule rule in dashboardHelper.Rules)
            {
                lbxRules.Items.Add(rule.ToString());
            }

            //this.txtTitle.Text = SharedStrings.DATA_FORMAT_RECODE + " (" + lbxRules.Items.Count.ToString() + ")";
            this.txtTitle.Text = string.Format(DashboardSharedStrings.VARIABLE_CONTROL_TITLE, lbxRules.Items.Count.ToString());
        }
        #endregion // Private Methods

        #region Private Properties
        private View View
        {
            get
            {
                return this.dashboardHelper.View;
            }
        }

        private IDbDriver Database
        {
            get
            {
                return this.dashboardHelper.Database;
            }
        }
        #endregion // Private Properties

        private void btnNewRule_Click(object sender, RoutedEventArgs e)
        {
            if (btnNewRule.ContextMenu != null)
            {
                btnNewRule.ContextMenu.PlacementTarget = btnNewRule;
                btnNewRule.ContextMenu.IsOpen = true;
            }

            e.Handled = true;
            return;

            //EpiDashboard.Dialogs.RecodeDialog recodeDialog = new EpiDashboard.Dialogs.RecodeDialog(this.dashboardHelper);
            //recodeDialog.ShowDialog();
        }

        private void mnuNewRecodeRule_Click(object sender, RoutedEventArgs e)
        {
            EpiDashboard.Dialogs.RecodeDialog recodeDialog = new EpiDashboard.Dialogs.RecodeDialog(this.dashboardHelper);
            System.Windows.Forms.DialogResult result = recodeDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // Add to the list of recode rules
                dashboardHelper.AddRule(recodeDialog.RecodeRule);

                // Update the UI listbox
                UpdateRules();
                if (UserVariableAdded != null)
                {
                    UserVariableAdded(this, new EventArgs());
                }
            }
        }

        private void mnuNewFormatRule_Click(object sender, RoutedEventArgs e)
        {
            EpiDashboard.Dialogs.FormatDialog formatDialog = new EpiDashboard.Dialogs.FormatDialog(this.dashboardHelper);
            System.Windows.Forms.DialogResult result = formatDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // Add to the list of rules
                dashboardHelper.AddRule(formatDialog.FormatRule);

                // Update the UI listbox
                UpdateRules();
                if (UserVariableAdded != null)
                {
                    UserVariableAdded(this, new EventArgs());
                }
            }
        }

        private void mnuNewExpressionAssignRule_Click(object sender, RoutedEventArgs e)
        {
            EpiDashboard.Dialogs.ExpressionAssignDialog assignDialog = new EpiDashboard.Dialogs.ExpressionAssignDialog(this.dashboardHelper);
            System.Windows.Forms.DialogResult result = assignDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // Add to the list of rules
                dashboardHelper.AddRule(assignDialog.AssignRule);

                // Update the UI listbox
                UpdateRules();
                if (UserVariableAdded != null)
                {
                    UserVariableAdded(this, new EventArgs());
                }
            }
        }

        private void mnuNewSimpleAssignRule_Click(object sender, RoutedEventArgs e)
        {
            EpiDashboard.Dialogs.SimpleAssignDialog assignDialog = new EpiDashboard.Dialogs.SimpleAssignDialog(this.dashboardHelper);
            System.Windows.Forms.DialogResult result = assignDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // Add to the list of rules
                dashboardHelper.AddRule(assignDialog.AssignRule);

                // Update the UI listbox
                UpdateRules();
                if (UserVariableAdded != null)
                {
                    UserVariableAdded(this, new EventArgs());
                }
            }
        }

        private void mnuNewConditionalAssignRule_Click(object sender, RoutedEventArgs e)
        {
            EpiDashboard.Dialogs.ConditionalAssignDialog assignDialog = new EpiDashboard.Dialogs.ConditionalAssignDialog(this.dashboardHelper);
            System.Windows.Forms.DialogResult result = assignDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // Add to the list of rules
                dashboardHelper.AddRule(assignDialog.AssignRule);

                // Update the UI listbox
                UpdateRules();
                if (UserVariableChanged != null)
                {
                    UserVariableChanged(this, new EventArgs());
                }
            }
        }

        private void mnuNewVariableGroupRule_Click(object sender, RoutedEventArgs e)
        {
            EpiDashboard.Dialogs.CreateGroupDialog groupDialog = new EpiDashboard.Dialogs.CreateGroupDialog(this.dashboardHelper);
            System.Windows.Forms.DialogResult result = groupDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // Add to the list of format rules
                dashboardHelper.AddRule(groupDialog.Group);

                // Update the UI listbox
                UpdateRules();
                if (UserVariableChanged != null)
                {
                    UserVariableChanged(this, new EventArgs());
                }
            }
        }

        private void btnEditRule_Click(object sender, RoutedEventArgs e)
        {
            if (lbxRules.SelectedItems != null && lbxRules.SelectedItems.Count == 1)
            {
                Rule_Recode recodeRule = null;
                Rule_Format formatRule = null;
                Rule_ExpressionAssign expressionAssignRule = null;
                Rule_SimpleAssign simpleAssignRule = null;
                Rule_ConditionalAssign conditionalAssignRule = null;
                Rule_VariableGroup variableGroupRule = null;

                foreach (IDashboardRule rule in dashboardHelper.Rules)
                {
                    if (rule.FriendlyRule.Equals(lbxRules.SelectedItem.ToString()))
                    {
                        if (rule is Rule_Recode)
                        {
                            recodeRule = rule as Rule_Recode;
                            break;
                        }
                        else if (rule is Rule_Format)
                        {
                            formatRule = rule as Rule_Format;
                            break;
                        }
                        else if (rule is Rule_ExpressionAssign)
                        {
                            expressionAssignRule = rule as Rule_ExpressionAssign;
                            break;
                        }
                        else if (rule is Rule_SimpleAssign)
                        {
                            simpleAssignRule = rule as Rule_SimpleAssign;
                            break;
                        }
                        else if (rule is Rule_ConditionalAssign)
                        {
                            conditionalAssignRule = rule as Rule_ConditionalAssign;
                            break;
                        }
                        else if (rule is Rule_VariableGroup)
                        {
                            variableGroupRule = rule as Rule_VariableGroup;
                            break;
                        }
                    }
                }

                System.Windows.Forms.DialogResult result = System.Windows.Forms.DialogResult.None;

                if (recodeRule != null)
                {
                    EpiDashboard.Dialogs.RecodeDialog recodeDialog = new EpiDashboard.Dialogs.RecodeDialog(this.dashboardHelper, recodeRule);
                    result = recodeDialog.ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        dashboardHelper.UpdateRule(recodeRule, recodeDialog.RecodeRule);
                        UpdateRules();
                        if (UserVariableChanged != null)
                        {
                            UserVariableChanged(this, new EventArgs());
                        }
                    }
                }
                else if(formatRule != null) 
                {
                    EpiDashboard.Dialogs.FormatDialog formatDialog = new EpiDashboard.Dialogs.FormatDialog(this.dashboardHelper, formatRule);
                    result = formatDialog.ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        dashboardHelper.UpdateRule(formatRule, formatDialog.FormatRule);
                        UpdateRules();
                        if (UserVariableChanged != null)
                        {
                            UserVariableChanged(this, new EventArgs());
                        }
                    }
                }
                else if (expressionAssignRule != null)
                {
                    EpiDashboard.Dialogs.ExpressionAssignDialog assignDialog = new EpiDashboard.Dialogs.ExpressionAssignDialog(this.dashboardHelper, expressionAssignRule);
                    result = assignDialog.ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        dashboardHelper.UpdateRule(expressionAssignRule, assignDialog.AssignRule);
                        UpdateRules();
                        if (UserVariableChanged != null)
                        {
                            UserVariableChanged(this, new EventArgs());
                        }
                    }
                }
                else if (simpleAssignRule != null)
                {
                    EpiDashboard.Dialogs.SimpleAssignDialog assignDialog = new EpiDashboard.Dialogs.SimpleAssignDialog(this.dashboardHelper, simpleAssignRule);
                    result = assignDialog.ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        dashboardHelper.UpdateRule(simpleAssignRule, assignDialog.AssignRule);
                        UpdateRules();
                        if (UserVariableChanged != null)
                        {
                            UserVariableChanged(this, new EventArgs());
                        }
                    }
                }
                else if (conditionalAssignRule != null)
                {
                    EpiDashboard.Dialogs.ConditionalAssignDialog assignDialog = new EpiDashboard.Dialogs.ConditionalAssignDialog(this.dashboardHelper, conditionalAssignRule);
                    result = assignDialog.ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        dashboardHelper.UpdateRule(conditionalAssignRule, assignDialog.AssignRule);
                        UpdateRules();
                        if (UserVariableChanged != null)
                        {
                            UserVariableChanged(this, new EventArgs());
                        }
                    }
                }
                else if (variableGroupRule != null)
                {
                    EpiDashboard.Dialogs.CreateGroupDialog groupDialog = new EpiDashboard.Dialogs.CreateGroupDialog(this.dashboardHelper, variableGroupRule);
                    result = groupDialog.ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        dashboardHelper.UpdateRule(variableGroupRule, groupDialog.Group);
                        UpdateRules();
                        if (UserVariableChanged != null)
                        {
                            UserVariableChanged(this, new EventArgs());
                        }
                    }
                }
            }
        }
    }
}
