using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Epi.Fields;
using EpiDashboard;
using EpiDashboard.Controls;
using EpiDashboard.Gadgets;
using EpiDashboard.Gadgets.Charting;

namespace EpiDashboard.Gadgets.Charting
{
    public class HistogramChartGadgetBase : ChartGadgetBase
    {
        public Controls.Charting.IChart SelectedChart { get; set; }

        protected object syncLockData = new object();

        protected delegate void SetChartDataDelegate(List<XYColumnChartData> dataList, Strata strata);

        protected virtual void SetChartData(List<XYColumnChartData> dataList, Strata strata)
        {
        }

        protected override void Construct()
        {
            object element = this.FindName("txtLegendFontSize");
            if (element != null && element is TextBox)
            {
                TextBox textBox = element as TextBox;
                textBox.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            }

            element = this.FindName("txtHeight");
            if (element != null && element is TextBox)
            {
                TextBox textBox = element as TextBox;
                textBox.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            }

            element = this.FindName("txtWidth");
            if (element != null && element is TextBox)
            {
                TextBox textBox = element as TextBox;
                textBox.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            }

            element = this.FindName("txtTransTop");
            if (element != null && element is TextBox)
            {
                TextBox textBox = element as TextBox;
                textBox.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            }

            element = this.FindName("txtTransBottom");
            if (element != null && element is TextBox)
            {
                TextBox textBox = element as TextBox;
                textBox.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            }

            element = this.FindName("txtXAxisAngle");
            if (element != null && element is TextBox)
            {
                TextBox textBox = element as TextBox;
                textBox.PreviewKeyDown += new KeyEventHandler(txtInput_IntegerOnly_PreviewKeyDown);
            }

            element = this.FindName("expanderAdvancedOptions");
            if (element != null && element is Expander)
            {
                Expander expanderAdvancedOptions = element as Expander;
                expanderAdvancedOptions.Header = DashboardSharedStrings.GADGET_ADVANCED_OPTIONS;
            }

            element = this.FindName("expanderDisplayOptions");
            if (element != null && element is Expander)
            {
                Expander expanderDisplayOptions = element as Expander;
                expanderDisplayOptions.Header = DashboardSharedStrings.GADGET_DISPLAY_OPTIONS;
            }

            base.Construct();
        }
    }
}
