using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EpiDashboard.Rules;

namespace EpiDashboard.Dialogs
{
    public partial class CreateGroupDialog : Form
    {
        private DashboardHelper dashboardHelper;
        private Rule_VariableGroup group;
        private bool editMode = false;

        public CreateGroupDialog(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.dashboardHelper = dashboardHelper;
            editMode = false;
        }

        public CreateGroupDialog(DashboardHelper dashboardHelper, Rule_VariableGroup group)
        {
            InitializeComponent();
            this.dashboardHelper = dashboardHelper;
            this.group = group;
            this.txtGroupField.Text = group.GroupName;
            editMode = true;
        }

        public Rule_VariableGroup Group
        {
            get
            {
                return this.group;
            }
        }

        private void CreateGroupDialog_Load(object sender, EventArgs e)
        {
            this.listBoxItems.DataSource = null;
            this.listBoxItems.Items.Clear();

            ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.DateTime | ColumnDataType.UserDefined;
            List<string> fieldNames = dashboardHelper.GetFieldsAsList(columnDataType);

            if (fieldNames.Contains("SYSTEMDATE"))
            {
                fieldNames.Remove("SYSTEMDATE");
            }
            if (fieldNames.Contains("RecStatus"))
            {
                fieldNames.Remove("RecStatus");
            }
            if (fieldNames.Contains("FKEY"))
            {
                fieldNames.Remove("FKEY");
            }
            if (fieldNames.Contains("UniqueKey"))
            {
                fieldNames.Remove("UniqueKey");
            }

            foreach (string fieldName in fieldNames)
            {
                listBoxItems.Items.Add(fieldName);
            }

            if (editMode)
            {
                int notFoundCount = 0;
                foreach (string fieldName in group.Variables)
                {
                    if (listBoxItems.Items.Contains(fieldName))
                    {
                        listBoxItems.SelectedItems.Add(fieldName);
                    }
                    else
                    {
                        notFoundCount++;
                    }
                }

                if (notFoundCount > 0)
                {
                    Epi.Windows.MsgBox.ShowWarning(string.Format(DashboardSharedStrings.CREATE_VAR_GROUP_FIELDS_NOT_FOUND, notFoundCount.ToString()));
                }

                txtGroupField.Enabled = false;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (listBoxItems.SelectedItems.Count <= 0)
            {
                Epi.Windows.MsgBox.ShowError(DashboardSharedStrings.CREATE_VAR_GROUP_NO_FIELDS_SELECTED);
                this.DialogResult = System.Windows.Forms.DialogResult.None;
                return;
            }

            if (!editMode && (dashboardHelper.GetFieldsAsList().Contains(txtGroupField.Text) || dashboardHelper.GetAllGroupsAsList().Contains(txtGroupField.Text)))
            {
                Epi.Windows.MsgBox.ShowError(DashboardSharedStrings.CREATE_VAR_GROUP_FIELD_ALREADY_EXISTS);
                this.DialogResult = System.Windows.Forms.DialogResult.None;
                return;
            }

            if (string.IsNullOrEmpty(txtGroupField.Text))
            {
                Epi.Windows.MsgBox.ShowError(DashboardSharedStrings.CREATE_VAR_GROUP_NO_NAME_SPECIFIED);
                this.DialogResult = System.Windows.Forms.DialogResult.None;
                return;
            }

            List<string> items = new List<string>();

            foreach (string item in listBoxItems.SelectedItems)
            {
                items.Add(item);
            }

            group = new Rule_VariableGroup(this.dashboardHelper, txtGroupField.Text, items);            
        }
    }
}
