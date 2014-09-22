using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpiDashboard
{
    public class AberrationDetectionChartParameters : AberrationDetectionChartParametersBase
    {
        //Aberration Detection Chart parameters
        #region Properties

        #endregion  // Properties

        public AberrationDetectionChartParameters()
            : base()
        {
            LineType = string.Empty;
            GadgetTitle = "Aberration Detection Chart";
            LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Right;

        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public AberrationDetectionChartParameters(AberrationDetectionChartParameters parameters)
            : base(parameters)
        {
            LineType = parameters.LineType;
        }

    }
}
