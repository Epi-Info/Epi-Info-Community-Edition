using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EpiDashboard.Mapping
{
    public partial class TimeConfigDialog : Form
    {
        public TimeConfigDialog(Epi.View view)
        {
            InitializeComponent();
            FillComboBox(view);
        }

        public TimeConfigDialog(List<DashboardHelper> dashboardHelpers)
        {
            InitializeComponent();
            FillComboBox(dashboardHelpers);
        }

        private void FillComboBox(List<DashboardHelper> dashboardHelpers)
        {
            cbxTime.Items.Clear();
            foreach (DashboardHelper dashboardHelper in dashboardHelpers)
            {
                ColumnDataType columnDataType = ColumnDataType.DateTime;
                List<string> dateFields = dashboardHelper.GetFieldsAsList(columnDataType);
                foreach (string dateField in dateFields)
                {
                    if (!cbxTime.Items.Contains(dateField))
                    {
                        cbxTime.Items.Add(dateField);
                    }
                }
            }
        }

        private void FillComboBox(Epi.View view)
        {
            cbxTime.Items.Clear();
            foreach (Epi.Fields.Field f in view.Fields)
            {
                if ((f is Epi.Fields.DateField) || (f is Epi.Fields.DateTimeField) || (f is Epi.Fields.TimeField))
                {
                    cbxTime.Items.Add(f.Name);
                }
            }
        }

        public string TimeVariable
        {
            get
            {
                if (cbxTime.SelectedIndex != -1)
                    return cbxTime.SelectedItem.ToString();
                else
                    return null;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void cbxTime_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOk.Enabled = (cbxTime.SelectedIndex != -1);
        }
    }
}
