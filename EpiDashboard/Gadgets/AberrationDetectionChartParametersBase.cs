using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComponentArt.Win.DataVisualization.Charting;

namespace EpiDashboard
{
    public class AberrationDetectionChartParametersBase : ChartParametersBase
    {
        #region Constants
        private const int DEFAULT_DEVIATIONS = 3;
        private const int DEFAULT_LAG_TIME = 7;
        private const int DEFAULT_TIME_PERIOD = 365;
        #endregion //Constants

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public AberrationDetectionChartParametersBase()
            : base()
        {
            YAxisLabel = "Count";
            LineType = string.Empty;
            LineKind = LineKind.Auto;
            Y2LineKind = LineKind.Auto;

            LagTime = DEFAULT_LAG_TIME.ToString();
            Deviations = DEFAULT_DEVIATIONS.ToString();
            TimePeriod = DEFAULT_TIME_PERIOD.ToString();
            Aberration = true;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public AberrationDetectionChartParametersBase(AberrationDetectionChartParameters parameters)
            : base(parameters)
        {
            LineType = parameters.LineType;
            LineKind = parameters.LineKind;
            Y2LineKind = parameters.Y2LineKind;

            LagTime = parameters.LagTime;
            Deviations = parameters.Deviations;
            TimePeriod = parameters.TimePeriod;
            Aberration = parameters.Aberration;

        }

        #endregion //Constructors

        #region Properties

        /// <summary>
        /// Gets/sets the type of line as auto, polygon, smooth, step, etc.
        /// </summary>
        public string LineType { get; set; }

        public LineKind LineKind { get; set; }
        public LineKind Y2LineKind { get; set; }
        public double LineThickness { get; set; }

        public string LagTime { get; set; }
        public string Deviations { get; set; }
        public string TimePeriod { get; set; }
        public bool Aberration { get; set; }


        #endregion //Properties

    }
}
