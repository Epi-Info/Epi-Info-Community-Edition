using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EpiDashboard
{
    public partial class ChartLabelDialog : Form
    {

        public ChartLabelDialog()
        {
            InitializeComponent();
        }

        public string Title 
        {
            get
            {
                return txtChartTitle.Text;
            }
            set
            {
                txtChartTitle.Text = value;
            }
        }

        public string XAxisLabel 
        {
            get
            {
                return txtXAxis.Text;
            }
            set
            {
                txtXAxis.Text = value;
            }
        }

        public string YAxisLabel 
        {
            get
            {
                return txtYAxis.Text;
            }
            set
            {
                txtYAxis.Text = value;
            }
        }

        public string LegendTitle
        {
            get
            {
                return txtLegendTitle.Text;
            }
            set
            {
                txtLegendTitle.Text = value;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

    }
}
