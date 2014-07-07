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
    public enum FilterDialogMode
    {
        RowFilterMode = 1,
        ConditionalMode = 2
    }

    public partial class RowFilterDialog : Form
    {
        private ElementHost host;
        private EpiDashboard.RowFilterControl rfc;
        private EpiDashboard.DashboardHelper dashboardHelper;
        private bool includeUserDefinedVars = true;
        private FilterDialogMode mode = FilterDialogMode.RowFilterMode;

        public RowFilterDialog(DashboardHelper dashboardHelper, FilterDialogMode pMode, bool includeUserDefinedVars)
        {
            InitializeComponent();
            this.includeUserDefinedVars = includeUserDefinedVars;
            this.dashboardHelper = dashboardHelper;
            this.Mode = pMode;
            Construct();

            rfc = new EpiDashboard.RowFilterControl(dashboardHelper, Mode, null, includeUserDefinedVars);
            host.Child = rfc;            
        }

        public RowFilterDialog(DashboardHelper dashboardHelper, FilterDialogMode pMode, DataFilters filters, bool includeUserDefinedVars)
        {
            InitializeComponent();
            this.includeUserDefinedVars = includeUserDefinedVars;
            this.dashboardHelper = dashboardHelper;
            this.Mode = pMode;
            Construct();

            rfc = new EpiDashboard.RowFilterControl(dashboardHelper, Mode, filters, includeUserDefinedVars);
            host.Child = rfc;            
        }

        private FilterDialogMode Mode
        {
            get
            {
                return this.mode;
            }
            set
            {
                this.mode = value;
                if (Mode == FilterDialogMode.RowFilterMode)
                {
                    this.Text = "Specify row filter";
                }
                else if (Mode == FilterDialogMode.ConditionalMode)
                {
                    this.Text = "Specify assign condition";
                }
            }
        }

        private void Construct()
        {
            host = new ElementHost();
            host.Dock = DockStyle.Fill;
            this.Controls.Add(host);

            this.Width = 587;
            this.Height = 560;
        }

        public DataFilters DataFilters
        {
            get
            {
                return this.rfc.DataFilters;
            }
        }
    }
}
