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
    public class AreaChartParametersBase : ChartParametersBase
    {
        #region Events

        #endregion //Events

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public AreaChartParametersBase()
            : base()
        {
            Composition = CompositionKind.Stacked;
            ChartStrataTitle = String.Empty;
            ShowSeriesLine = true;
            AreaKind = 0;
            TransTop = 90;
            TransBottom = 30;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public AreaChartParametersBase(AreaChartParametersBase parameters)
            : base(parameters)
        {
            Composition = parameters.Composition;
            ChartStrataTitle = parameters.ChartStrataTitle;
            ShowSeriesLine = parameters.ShowSeriesLine;
            AreaKind = parameters.AreaKind;
            TransTop = parameters.TransTop;
            TransBottom = parameters.TransBottom;
        }
        
        #endregion     // Constructors

        #region Properties
        //IChartGadgetParameters Properties

        public CompositionKind Composition { get; set; }
        public bool ShowSeriesLine { get; set; }
        public LineKind AreaKind { get; set; }
        public double TransTop { get; set; }
        public double TransBottom { get; set; }

        #endregion  // Properties

    }
}
