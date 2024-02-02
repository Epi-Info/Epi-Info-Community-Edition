using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using EpiDashboard;
using EpiDashboard.Rules;

namespace EpiDashboard.Dialogs
{
    public partial class ConditionalAssignDialog : Form
    {
        private EpiDashboard.DashboardHelper dashboardHelper;
        private Rule_ConditionalAssign assignRule;
        private DataFilters DataFilters;
        private bool editMode = false;

        private delegate void SetAssignValue(Rule_ConditionalAssign conditionalAssignRule);

        public ConditionalAssignDialog(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.dashboardHelper = dashboardHelper;
            this.DataFilters = new DataFilters(dashboardHelper);
            FillComboBoxes();
        }

        public ConditionalAssignDialog(DashboardHelper dashboardHelper, Rule_ConditionalAssign conditionalAssignRule)
        {
            InitializeComponent();
            this.dashboardHelper = dashboardHelper;
            this.DataFilters = conditionalAssignRule.DataFilters;

            editMode = true;

            FillComboBoxes();

            this.txtDestinationField.Text = conditionalAssignRule.DestinationColumnName;
            this.txtDestinationField.Enabled = false;

            SetAssignValue setAssignValue = new SetAssignValue(SetAssignmentValue);

            switch (conditionalAssignRule.DestinationColumnType)
            {
                case "System.SByte":
                case "System.Byte":
                case "System.Boolean":
                    this.cbxFieldType.SelectedItem = "Yes/No";
                    setAssignValue = new SetAssignValue(SetBooleanAssignmentValue);
                    break;
                case "System.String":
                    this.cbxFieldType.SelectedItem = "Text";
                    break;
                case "System.Single":
                case "System.Double":
                case "System.Decimal":
                case "System.Int32":
                case "System.Int16":
                    this.cbxFieldType.SelectedItem = "Numeric";
                    break;
            }

            cbxFieldType.Enabled = false;

            this.txtAssignCondition.Text = DataFilters.GenerateReadableDataFilterString();

            //foreach (KeyValuePair<string, object> kvp in conditionalAssignRule.Conditions)
            //{
            //    this.txtAssignValue.Text = kvp.Value.ToString();
            //    break;
            //}
            
            setAssignValue(conditionalAssignRule);            
        }

        private void SetAssignmentValue(Rule_ConditionalAssign conditionalAssignRule)
        {
            this.txtAssignValue.Text = conditionalAssignRule.AssignValue.ToString();

            if (conditionalAssignRule.UseElse == true)
            {
                this.checkboxUseElse.Checked = true;
                this.txtElseValue.Text = conditionalAssignRule.ElseValue.ToString();
            }
            else
            {
                this.checkboxUseElse.Checked = false;
                this.txtElseValue.Text = string.Empty;
            }
        }

        private void SetBooleanAssignmentValue(Rule_ConditionalAssign conditionalAssignRule)
        {
            this.txtAssignValue.Text = conditionalAssignRule.AssignValue.ToString();
            if (conditionalAssignRule.AssignValue.ToString().ToLowerInvariant() == "true")
                this.cmbAssignValue.SelectedIndex = 0;
            else if (conditionalAssignRule.AssignValue.ToString().ToLowerInvariant() == "false")
                this.cmbAssignValue.SelectedIndex = 1;

            if (conditionalAssignRule.UseElse == true)
            {
                this.checkboxUseElse.Checked = true;
                this.txtElseValue.Text = conditionalAssignRule.ElseValue.ToString();
                if (conditionalAssignRule.ElseValue.ToString().ToLowerInvariant() == "true")
                    this.cmbElseValue.SelectedIndex = 0;
                else if (conditionalAssignRule.ElseValue.ToString().ToLowerInvariant() == "false")
                    this.cmbElseValue.SelectedIndex = 1;
            }
            else
            {
                this.checkboxUseElse.Checked = false;
                this.txtElseValue.Text = string.Empty;
                this.cmbElseValue.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Gets the format rule
        /// </summary>
        public Rule_ConditionalAssign AssignRule
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

        /// <summary>
        /// Fills the combo boxes on this dialog
        /// </summary>
        private void FillComboBoxes()
        {
            txtDestinationField.Text = string.Empty;            
            cbxFieldType.Items.Clear();

            List<string> fieldNames = new List<string>();

            cbxFieldType.Items.Add("Text");
            cbxFieldType.Items.Add("Numeric");
            cbxFieldType.Items.Add("Yes/No");
            cbxFieldType.SelectedIndex = 0;

            cmbAssignValue.Items.Add(dashboardHelper.Config.Settings.RepresentationOfYes);
            cmbAssignValue.Items.Add(dashboardHelper.Config.Settings.RepresentationOfNo);

            cmbElseValue.Items.Add(dashboardHelper.Config.Settings.RepresentationOfYes);
            cmbElseValue.Items.Add(dashboardHelper.Config.Settings.RepresentationOfNo);
        }

        private void checkboxUseElse_CheckedChanged(object sender, EventArgs e)
        {
            if (checkboxUseElse.Checked)
            {
                txtElseValue.Enabled = true;
                cmbElseValue.Enabled = true;
            }
            else
            {
                txtElseValue.Enabled = false;
                cmbElseValue.Enabled = false;
            }
        }

        private void btnIfCondition_Click(object sender, EventArgs e)
        {
            RowFilterDialog rfd = new RowFilterDialog(this.dashboardHelper, FilterDialogMode.ConditionalMode, DataFilters, true);
            DialogResult result = rfd.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.DataFilters = rfd.DataFilters;
                this.txtAssignCondition.Text = DataFilters.GenerateReadableDataFilterString();//this.DataFilters.GenerateDataFilterString();
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string destinationColumnType = "System.String";

            object elseValue = this.txtElseValue.Text;
            object assignValue = this.txtAssignValue.Text;

            switch (cbxFieldType.SelectedItem.ToString())
            {
                case "Yes/No":
                    destinationColumnType = "System.Boolean";
                    if (cmbAssignValue.SelectedIndex == 0)
                    {
                        assignValue = true;
                    }
                    else if (cmbAssignValue.SelectedIndex == 1)
                    {
                        assignValue = false;
                    }

                    if (cmbElseValue.SelectedIndex == 0)
                    {
                        elseValue = true;
                    }
                    else if (cmbElseValue.SelectedIndex == 1)
                    {
                        elseValue = false;
                    }

                    break;
                case "Text":
                    destinationColumnType = "System.String";
                    elseValue = this.txtElseValue.Text;
                    assignValue = this.txtAssignValue.Text;
                    break;
                case "Numeric":
                    destinationColumnType = "System.Decimal";
                    decimal decElse;
                    decimal decAssign;
                    bool success1 = Decimal.TryParse(this.txtElseValue.Text, out decElse);
                    if (success1) elseValue = decElse;
                    else if (this.txtElseValue.Text.StartsWith("varname(") && this.txtElseValue.Text.IndexOf(')') == this.txtElseValue.Text.Length - 1)
                    {
                        elseValue = this.txtElseValue.Text;
                        success1 = true;
                    }
                    bool success2 = Decimal.TryParse(this.txtAssignValue.Text, out decAssign);
                    if (success2) assignValue = decAssign;
                    else if (this.txtAssignValue.Text.StartsWith("varname(") && this.txtAssignValue.Text.IndexOf(')') == this.txtAssignValue.Text.Length - 1)
                    {
                        assignValue = this.txtAssignValue.Text;
                        success2 = true;
                    }

                    if ((!success1 && checkboxUseElse.Checked) || !success2)
                    {
                        Epi.Windows.MsgBox.ShowError(DashboardSharedStrings.ERROR_CANNOT_CONDITIONAL_ASSIGN_INVALID_INPUT);
                        this.DialogResult = DialogResult.None;
                        return;
                    }
                    break;
            }

            if (!editMode && this.dashboardHelper.TableColumnNames.ContainsKey(txtDestinationField.Text))
            {
                string columnType = dashboardHelper.GetColumnType(txtDestinationField.Text);

                if (columnType != destinationColumnType)
                {
                    Epi.Windows.MsgBox.ShowError(string.Format(DashboardSharedStrings.ERROR_CANNOT_CONDITIONAL_ASSIGN_TYPE_MISMATCH, columnType, destinationColumnType));                        
                    this.DialogResult = DialogResult.None;
                    return;
                }
            }
            
            string sentencePart = txtAssignCondition.Text;

            if (sentencePart.Length > 0)
            {
                sentencePart = " when " + "t" + txtAssignCondition.Text.Remove(0, 1);
            }

            string conditionText = "Assign " + txtDestinationField.Text + " the value " + assignValue + sentencePart;

            if (!checkboxUseElse.Checked)
            {
                elseValue = null;
            }
            else
            {
                conditionText = conditionText + ". Otherwise, assign " + txtDestinationField.Text + " the value " + elseValue + ".";
            }

            assignRule = new Rule_ConditionalAssign(this.dashboardHelper, conditionText, txtDestinationField.Text, destinationColumnType, assignValue, elseValue, DataFilters.GenerateDataFilterString());
            assignRule.DataFilters = this.DataFilters;
        }

        private void cbxFieldType_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cbxFieldType.SelectedIndex)
            {
                case -1:
                    return;
                case 0:
                    txtAssignValue.Visible = true;
                    txtElseValue.Visible = true;
                    cmbAssignValue.Visible = false;
                    cmbElseValue.Visible = false;
                    break;
                case 1:
                    txtAssignValue.Visible = true;
                    txtElseValue.Visible = true;
                    cmbAssignValue.Visible = false;
                    cmbElseValue.Visible = false;
                    break;
                case 2:
                    txtAssignValue.Visible = false;
                    txtElseValue.Visible = false;
                    cmbAssignValue.Visible = true;                    
                    cmbElseValue.Visible = true;
                    break;
            }
        }
    }
}
