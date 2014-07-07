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
            GadgetTitle = "Line Chart";
            LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Right;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public LineChartParameters(LineChartParameters parameters)
            : base(parameters)
        {
            LineType = parameters.LineType;
        }

    }
}
