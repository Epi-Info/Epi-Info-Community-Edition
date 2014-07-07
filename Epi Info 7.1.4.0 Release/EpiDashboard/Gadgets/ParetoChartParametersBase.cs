using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using ComponentArt.Win.DataVisualization;
using ComponentArt.Win.DataVisualization.Common;
using ComponentArt.Win.DataVisualization.Charting;

namespace EpiDashboard
{
    public class ParetoChartParametersBase : ChartParametersBase
    {
        #region Events

        #endregion //Events

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public ParetoChartParametersBase()
            : base()
        {
            BarSpace = BarSpacing.Default;
            BarType = BarKind.Rectangle;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public ParetoChartParametersBase(ParetoChartParametersBase parameters)
            : base(parameters)
        {
            BarSpace = parameters.BarSpace;
            BarType = parameters.BarType;
        }
        
        #endregion     // Constructors

        #region Properties
        //IChartGadgetParameters Properties
        
        public BarSpacing BarSpace { get; set; }
        public BarKind BarType { get; set; }
        
        #endregion  // Properties

    }
}
