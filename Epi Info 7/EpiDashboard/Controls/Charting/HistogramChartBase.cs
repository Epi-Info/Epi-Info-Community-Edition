using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ComponentArt.Win.DataVisualization.Charting;
using EpiDashboard;
using EpiDashboard.Gadgets.Charting;

namespace EpiDashboard.Controls.Charting
{
    public class HistogramChartBase : ChartBase
    {
        public IChartSettings Settings { get; set; }
        public HistogramChartParameters HistogramChartParameters { get; set; }
        public DashboardHelper DashboardHelper { get; set; }

        new protected virtual void SetLegendItems()
        {
            XYChart xyChart = null;
            object el = FindName("xyChart");
            if (el is XYChart)
            {
                xyChart = el as XYChart;
            }

            if (xyChart == null) throw new ApplicationException();

            string sName = "";

            if (HistogramChartParameters.StrataVariableNames.Count > 0)
            {
                foreach (Series s0 in xyChart.DataSeries)
                {
                    if (s0.Label != null)
                    {
                        sName = s0.Label.Split('.')[1];
                        if (HistogramChartParameters.ShowLegendVarNames == false)
                        {
                            int index = sName.IndexOf(" = ");
                            s0.Label = sName.Substring(index + 3);
                        }
                        else
                        {
                            s0.Label = sName;
                        }
                    }
                }
            }
            else
            {
                foreach (Series series in xyChart.DataSeries)
                {
                    if(series.Name == "series0")
                    {
                        series.Label = HistogramChartParameters.YAxisLabel;
                    }
                    else if(series.Name == "series1")
                    {
                        if (!string.IsNullOrEmpty(Y2AxisLegendTitle))
                        {
                            series.Label = HistogramChartParameters.Y2AxisLegendTitle;
                        }
                        else if (!string.IsNullOrEmpty(Y2AxisLabel))
                        {
                            series.Label = HistogramChartParameters.Y2AxisLabel;
                        }
                    }
                }
            }
        }

    }
}
