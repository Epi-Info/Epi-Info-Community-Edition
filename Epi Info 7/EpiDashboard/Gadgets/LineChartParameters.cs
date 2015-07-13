using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpiDashboard
{
    public class LineChartParameters : LineChartParametersBase
    {
        //Line Chart parameters
        #region Properties

        #endregion  // Properties

        public LineChartParameters()
            : base()
        {
            LineType = string.Empty;
            LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Right;
            YAxisFontSize = 12;
            YAxisLabelFontSize = 12;
            XAxisFontSize = 12;
            XAxisLabelFontSize = 12;
            ChartTitle = String.Empty;
            ChartSubTitle = String.Empty;
            GadgetTitle = "Line Chart";
            GadgetDescription = String.Empty;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public LineChartParameters(LineChartParameters parameters)
            : base(parameters)
        {
            LineType = parameters.LineType;
            YAxisFontSize = parameters.YAxisFontSize;
            YAxisLabelFontSize = parameters.YAxisLabelFontSize;
            XAxisFontSize = parameters.XAxisFontSize;
            XAxisLabelFontSize = parameters.XAxisLabelFontSize;
            ChartTitle = parameters.ChartTitle;
            ChartSubTitle = parameters.ChartSubTitle;
            GadgetTitle = parameters.GadgetTitle;
            GadgetDescription = parameters.GadgetDescription;
        }

    }
}
