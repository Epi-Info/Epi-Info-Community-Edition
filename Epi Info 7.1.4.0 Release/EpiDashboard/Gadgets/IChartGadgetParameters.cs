using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using ComponentArt.Win.DataVisualization.Charting;
using ComponentArt.Win.DataVisualization.Common;
using ComponentArt.Win.DataVisualization;

namespace EpiDashboard
{
    //public enum XAxisLabelType
    //{
    //    Automatic,
    //    FieldPrompt,
    //    None,
    //    Custom
    //}

    public enum DateType
    {
        Minute,
        Hour,
        Day,
        Month,
        Year
    }

    public interface IChartGadgetParameters
    {
        bool UseRefValues { get; set; }
        bool ShowGridLines { get; set; }

        string XAxisLabel { get; set; }
        int XAxisLabelType { get; set; }
        int XAxisAngle { get; set; }

        string YAxisFormat { get; set; }
        string YAxisLabel { get; set; }

        string Y2AxisFormat { get; set; }
        string Y2AxisLabel { get; set; }
        string Y2AxisLegendTitle { get; set; }
        LineKind Y2LineKind { get; set; }
        //int Y2LineType { get; set; }
        int Y2AxisType { get; set; }
        LineDashStyle Y2LineDashStyle { get; set; }
        bool Y2ShowAnnotations { get; set; }
        double Y2LineThickness { get; set; }

        string ChartTitle { get; set; }
        string ChartStrataTitle { get; set; }
        string ChartSubTitle { get; set; }

        bool ShowAnnotations { get; set; }
        bool ShowLegend { get; set; }
        bool ShowLegendBorder { get; set; }
        bool ShowLegendVarNames { get; set; }

        double LegendFontSize { get; set; }
        double ChartWidth { get; set; }
        double ChartHeight { get; set; }
        int Palette { get; set; }

        ComponentArt.Win.DataVisualization.Charting.Dock LegendDock { get; set; }
    }
}
