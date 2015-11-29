using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComponentArt.Win.DataVisualization.Charting;

namespace EpiDashboard
{
    public class LineChartParametersBase : ChartParametersBase
    {
        #region Events

        #endregion //Events

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public LineChartParametersBase()
            : base()
        {
            YAxisLabel = "Count";
            LineType = string.Empty;
            LineKind = LineKind.Auto;
            Y2LineKind = LineKind.Auto;
            
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public LineChartParametersBase(LineChartParameters parameters)
            : base(parameters)
        {
            LineType = parameters.LineType;
            LineKind = parameters.LineKind;
            Y2LineKind = parameters.Y2LineKind;
          
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
        public List<string> PaletteColors { get; set; }
        #endregion //Properties

    }
}
