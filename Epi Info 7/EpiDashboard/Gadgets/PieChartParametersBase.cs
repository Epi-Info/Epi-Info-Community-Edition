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
    public class PieChartParametersBase : ChartParametersBase
    {
        #region Events

        #endregion //Events

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public PieChartParametersBase()
            : base()
        {
            ChartStrataTitle = String.Empty;
            ShowAnnotations = true;
            ShowAnnotationLabel = true;
            ShowAnnotationValue = true;
            ShowAnnotationPercent = true;
            AnnotationPercent = 20;
            PieChartKind = ComponentArt.Win.DataVisualization.Charting.PieChartKind.Pie2D;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public PieChartParametersBase(PieChartParametersBase parameters)
            : base(parameters)
        {
            ChartStrataTitle = parameters.ChartStrataTitle;
            ShowAnnotations = parameters.ShowAnnotations;
            ShowAnnotationLabel = parameters.ShowAnnotationLabel;
            ShowAnnotationValue = parameters.ShowAnnotationValue;
            ShowAnnotationPercent = parameters.ShowAnnotationPercent;
            AnnotationPercent = parameters.AnnotationPercent;
            PieChartKind = parameters.PieChartKind;
        }
        
        #endregion     // Constructors

        #region Properties
        //IChartGadgetParameters Properties

        public bool ShowAnnotations { get; set; }
        public bool ShowAnnotationLabel { get; set; }
        public bool ShowAnnotationValue { get; set; }
        public bool ShowAnnotationPercent { get; set; }
        public PieChartKind PieChartKind { get; set; }
        public double AnnotationPercent { get; set; }
        public List<string> PaletteColors { get; set; }
        #endregion  // Properties

    }
}
