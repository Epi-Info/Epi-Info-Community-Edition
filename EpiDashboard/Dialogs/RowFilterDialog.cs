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
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

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

            System.Windows.Size elementSize = GetElementPixelSize(host.Child);

            this.Width = (int)elementSize.Width + 90;
            this.Height = (int)elementSize.Height + 90;
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

            System.Windows.Size elementSize = GetElementPixelSize(host.Child);

            this.Width = (int)elementSize.Width + 90;
            this.Height = (int)elementSize.Height + 90;
        }

        public System.Windows.Size GetElementPixelSize(System.Windows.UIElement element)
        {
            Matrix transformToDevice;
            var source = PresentationSource.FromVisual(element);
            if (source != null)
            { 
                transformToDevice = source.CompositionTarget.TransformToDevice;
            }
            else
            {
                using (var Hwndsource = new HwndSource(new HwndSourceParameters()))
                {
                    transformToDevice = Hwndsource.CompositionTarget.TransformToDevice;
                }
            }

            if (element.DesiredSize == new System.Windows.Size())
            {
                element.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
            }

            return (System.Windows.Size)transformToDevice.Transform((Vector)element.DesiredSize);
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
                    this.Text = DashboardSharedStrings.GADGET_SPECIFY_ROW_FILTER;  //"Specify row filter";
                }
                else if (Mode == FilterDialogMode.ConditionalMode)
                {
                    this.Text = DashboardSharedStrings.GADGET_SPECIFY_ASSIGN_CONDITION;  //"Specify assign condition";
                }
            }
        }

        private void Construct()
        {
            host = new ElementHost();
            host.Dock = DockStyle.Fill;
            this.Controls.Add(host);
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
