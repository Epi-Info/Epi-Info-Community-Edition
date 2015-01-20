using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using EpiDashboard.Gadgets.Charting;
using ComponentArt.Win.DataVisualization;
using ComponentArt.Win.DataVisualization.Common;
using ComponentArt.Win.DataVisualization.Charting;


namespace EpiDashboard
{
    public class ChartParametersBase : FrequencyParametersBase, IChartGadgetParameters
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public ChartParametersBase()
            : base()
        {
            UseRefValues = true;
            ShowGridLines = true;

            XAxisLabel = String.Empty;
            XAxisLabelType = 0;
            XAxisAngle = 0;

            ShowAnnotations = false;
            YAxisFormat = String.Empty;
            YAxisLabel = String.Empty;

            Y2AxisFormat = String.Empty;
            Y2AxisLabel = String.Empty;
            Y2AxisLegendTitle = String.Empty;
            Y2LineKind = 0;
            Y2LineDashStyle = 0;
            Y2ShowAnnotations = false;
            Y2LineThickness = 0;
            
            ChartTitle = String.Empty;
            ChartStrataTitle = String.Empty;
            ChartSubTitle = String.Empty;

            ShowLegend = false;
            ShowLegendBorder = false;
            ShowLegendVarNames = false;
            LegendFontSize = 12;
            ChartWidth = 800;
            ChartHeight = 500;
            Palette = 12;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public ChartParametersBase(ChartParametersBase parameters)
            : base(parameters)
        {
            UseRefValues = parameters.UseRefValues;
            ShowGridLines = parameters.ShowGridLines;

            XAxisLabel = parameters.XAxisLabel;
            XAxisLabelType = parameters.XAxisLabelType;
            XAxisAngle = parameters.XAxisAngle;

            YAxisFormat = parameters.YAxisFormat;
            YAxisLabel = parameters.YAxisLabel;

            Y2AxisFormat = parameters.Y2AxisFormat;
            Y2AxisLabel = parameters.Y2AxisLabel;
            Y2AxisLegendTitle = parameters.Y2AxisLegendTitle;
            Y2LineKind =parameters.Y2LineKind;
            Y2LineDashStyle = parameters.Y2LineDashStyle;
            Y2ShowAnnotations = parameters.Y2ShowAnnotations;
            Y2LineThickness = parameters.Y2LineThickness;
                
            ChartTitle = parameters.ChartTitle;
            ChartStrataTitle = parameters.ChartStrataTitle;
            ChartSubTitle = parameters.ChartSubTitle;

            ShowAnnotations = parameters.ShowAnnotations;
            ShowLegend = parameters.ShowLegend;
            ShowLegendBorder = parameters.ShowLegendBorder;
            ShowLegendVarNames = parameters.ShowLegendVarNames;
            LegendFontSize = parameters.LegendFontSize;
            ChartWidth = parameters.ChartWidth;
            ChartHeight = parameters.ChartHeight;
            Palette = parameters.Palette;
        }
        #endregion     // Constructors

        #region Properties

        //IChartGadgetParameters Properties
        public bool UseRefValues { get; set; }
        public bool ShowGridLines { get; set; }

        public string XAxisLabel { get; set; }
        public double XAxisLabelFontSize { get; set; }//EI-98
        public double XAxisFontSize { get; set; }//EI-98
        public int XAxisLabelType { get; set; }
        public int XAxisAngle { get; set; }

        public string YAxisFormat { get; set; }
        public string YAxisLabel { get; set; }
        public double YAxisLabelFontSize { get; set; }//EI-98
        public double YAxisFontSize { get; set; }//EI-98
        public string Y2AxisFormat { get; set; }
        public string Y2AxisLabel { get; set; }
        public string Y2AxisLegendTitle { get; set; }
        public LineKind Y2LineKind { get; set; }
        //public int Y2LineType { get; set; }
        public int Y2AxisType { get; set; }
        public LineDashStyle Y2LineDashStyle { get; set; }
        public bool Y2ShowAnnotations { get; set; }
        public double Y2LineThickness { get; set; }

        public string ChartTitle { get; set; }
        public string ChartStrataTitle { get; set; }
        public string ChartSubTitle { get; set; }

        public bool ShowAnnotations { get; set; }
        public bool ShowLegend { get; set; }
        public bool ShowLegendBorder { get; set; }
        public bool ShowLegendVarNames { get; set; }

        public double LegendFontSize { get; set; }
        public double ChartWidth { get; set; }
        public double ChartHeight { get; set; }
        public int Palette { get; set; }

        public ComponentArt.Win.DataVisualization.Charting.Dock LegendDock { get; set; }
        #endregion  // Properties
    }
}
