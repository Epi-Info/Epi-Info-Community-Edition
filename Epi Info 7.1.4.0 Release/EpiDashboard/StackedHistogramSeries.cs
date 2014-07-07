using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Controls.DataVisualization;
using System.Windows.Controls;

namespace EpiDashboard
{
    public class StackedHistogramSeries : StackedColumnSeries
    {
        protected override void UpdateDataItemPlacement(IEnumerable<DefinitionSeries.DataItem> dataItems)
        {
            base.UpdateDataItemPlacement(dataItems);
            double width = this.SeriesArea.ActualWidth / IndependentValueGroups.Count();

            double min = 32767;
            double max = 0;

            List<string> series = new List<string>();

            foreach (DefinitionSeries.DataItem dataItem in dataItems)
            {
                if (dataItem.DataPoint.Width < min)
                {
                    min = dataItem.DataPoint.Width;
                }
                if (dataItem.DataPoint.Width > max)
                {
                    max = dataItem.DataPoint.Width;
                }
                if (!series.Contains(dataItem.SeriesDefinition.ActualTitle.ToString()))
                {
                    series.Add(dataItem.SeriesDefinition.ActualTitle.ToString());
                }
            }

            foreach (DefinitionSeries.DataItem dataItem in dataItems)
            {
                double orgWidth = dataItem.DataPoint.Width;
                double delta = width - dataItem.DataPoint.Width - 1;
                dataItem.DataPoint.Width = width;                

                Canvas.SetLeft(dataItem.Container, Canvas.GetLeft(dataItem.Container) - delta);

                if (orgWidth == max)
                {
                    Canvas.SetLeft(dataItem.Container, Canvas.GetLeft(dataItem.Container) - 1);
                    if (series.Count > 1)
                    {
                        dataItem.DataPoint.Width = dataItem.DataPoint.Width + 1;
                    }
                }

                dataItem.DataPoint.Width = dataItem.DataPoint.Width + 1;
            }

            if (series.Count > 1)
            {
                int z = dataItems.Count();
                foreach (DefinitionSeries.DataItem dataItem in dataItems)
                {
                    Canvas.SetZIndex(dataItem.Container, z);
                    z--;
                    dataItem.DataPoint.Width = dataItem.DataPoint.Width - 1;
                }                
            }
        }

        public int IndependentValueCount
        {
            get
            {
                return IndependentValueGroups.Count();
            }
        }
    }
}
