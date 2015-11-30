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
    public class ScatterChartParametersBase : ChartParametersBase
    {
        #region Events

        #endregion //Events

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public ScatterChartParametersBase()
            : base()
        {

        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public ScatterChartParametersBase(ScatterChartParametersBase parameters)
            : base(parameters)
        {

        }
        
        #endregion     // Constructors

        #region Properties
        //IChartGadgetParameters Properties
        public List<string> PaletteColors { get; set; }
        #endregion  // Properties

    }
}
