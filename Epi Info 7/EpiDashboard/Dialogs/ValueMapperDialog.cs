using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EpiDashboard.Dialogs
{
    public partial class ValueMapperDialog : Form
    {
        public DashboardHelper DashboardHelper { get; private set; }

        public List<string> YesValues { get; set; }
        public List<string> NoValues { get; set; }

        public ValueMapperDialog(DashboardHelper dashboardHelper, DataColumn column)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;

            YesValues = new List<string>();
            NoValues = new List<string>();

            foreach (string s in DashboardHelper.GetDistinctValuesAsList(column.ColumnName))
            {
                lbxAllValues.Items.Add(s);
            }
        }

        public ValueMapperDialog(DashboardHelper dashboardHelper, DataColumn exposure, DataColumn outcome, List<string> yesValues, List<string> noValues)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;

            YesValues = new List<string>();
            NoValues = new List<string>();

            foreach (string s in DashboardHelper.GetDistinctValuesAsList(exposure.ColumnName))
            {
                lbxAllValues.Items.Add(s);
            }
            foreach (string s in DashboardHelper.GetDistinctValuesAsList(outcome.ColumnName))
            {
                if (!lbxAllValues.Items.Contains(s))
                {
                    lbxAllValues.Items.Add(s);
                }
            }

            foreach (string s in yesValues)
            {
                lbxYesValues.Items.Add(s);
                lbxAllValues.Items.Remove(s);
            }

            foreach (string s in noValues)
            {
                lbxNoValues.Items.Add(s);
                lbxAllValues.Items.Remove(s);
            }
        }

        public ValueMapperDialog(DashboardHelper dashboardHelper, List<string> exposureFields, DataColumn outcome, List<string> yesValues, List<string> noValues)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;

            YesValues = new List<string>();
            NoValues = new List<string>();

            foreach (string field in exposureFields)                 
            {
                foreach (string s in DashboardHelper.GetDistinctValuesAsList(field))
                {
                    if (!lbxAllValues.Items.Contains(s))
                    {
                        lbxAllValues.Items.Add(s);
                    }
                }
            }
            foreach (string s in DashboardHelper.GetDistinctValuesAsList(outcome.ColumnName))
            {
                if (!lbxAllValues.Items.Contains(s))
                {
                    lbxAllValues.Items.Add(s);
                }
            }

            foreach (string s in yesValues)
            {
                lbxYesValues.Items.Add(s);
                lbxAllValues.Items.Remove(s);
            }

            foreach (string s in noValues)
            {
                lbxNoValues.Items.Add(s);
                lbxAllValues.Items.Remove(s);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            foreach (string s in lbxYesValues.Items)
            {
                YesValues.Add(s);
            }

            foreach (string s in lbxNoValues.Items)
            {
                if (!YesValues.Contains(s))
                {
                    NoValues.Add(s);
                }
                else
                {
                    MessageBox.Show("A value has been selected in more than one list.");
                    this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                    break;
                }
            }

            //foreach (string s in lbxMissingValues.SelectedItems)
            //{
            //    if (!YesValues.Contains(s) && !(NoValues.Contains(s)))
            //    {
            //        MissingValues.Add(s);
            //    }
            //    else
            //    {
            //        MessageBox.Show("A value has been selected in more than one list.");
            //        this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            //        break;
            //    }
            //}
        }

        private void btnAddYes_Click(object sender, EventArgs e)
        {
            if (lbxAllValues.SelectedItems.Count > 0)
            {
                string selectedItem = lbxAllValues.SelectedItem.ToString();

                lbxAllValues.Items.Remove(selectedItem);
                lbxYesValues.Items.Add(selectedItem);
            }
        }

        private void btnRemoveYes_Click(object sender, EventArgs e)
        {
            if (lbxYesValues.SelectedItems.Count > 0)
            {
                string selectedItem = lbxYesValues.SelectedItem.ToString();

                lbxYesValues.Items.Remove(selectedItem);
                lbxAllValues.Items.Add(selectedItem);
            }
        }

        private void btnAddNo_Click(object sender, EventArgs e)
        {
            if (lbxAllValues.SelectedItems.Count > 0)
            {
                string selectedItem = lbxAllValues.SelectedItem.ToString();

                lbxAllValues.Items.Remove(selectedItem);
                lbxNoValues.Items.Add(selectedItem);
            }
        }

        private void btnRemoveNo_Click(object sender, EventArgs e)
        {
            if (lbxNoValues.SelectedItems.Count > 0)
            {
                string selectedItem = lbxNoValues.SelectedItem.ToString();

                lbxNoValues.Items.Remove(selectedItem);
                lbxAllValues.Items.Add(selectedItem);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            foreach (string s in lbxYesValues.Items)
            {
                lbxAllValues.Items.Add(s);
            }

            foreach (string s in lbxNoValues.Items)
            {
                lbxAllValues.Items.Add(s);
            }

            lbxYesValues.Items.Clear();
            lbxNoValues.Items.Clear();
        }
    }
}
