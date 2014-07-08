using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using ComponentArt.Win.DataVisualization.Charting;
using ComponentArt.Win.DataVisualization.Common;
using ComponentArt.Win.DataVisualization;

namespace EpiDashboard.Gadgets.Charting
{
    public enum BarSpacing
    {
        Default,
        None,
        Small,
        Medium,
        Large
    }

    public enum XAxisLabelType
    {
        Automatic,
        FieldPrompt,
        None,
        Custom
    }

    public interface IChartSettings
    {
        bool UseRefValues { get; set; }
        bool ShowAnnotationsY2 { get; set; }
        bool ShowDefaultGridLines { get; set; }
        Palette Palette { get; set; }
        LineKind LineKindY2 { get; set; }
        LineDashStyle LineDashStyleY2 { get; set; }
        string Y2AxisLabel { get; set; }
        string Y2AxisLegendTitle { get; set; }
        string YAxisLabel { get; set; }
        string XAxisLabel { get; set; }
        double Y2LineThickness { get; set; }
        XAxisLabelType XAxisLabelType { get; set; }
        int XAxisLabelRotation { get; set; }
        string ChartTitle { get; set; }
        string ChartStrataTitle { get; set; }
        string ChartSubTitle { get; set; }
        bool ShowLegend { get; set; }
        bool ShowLegendBorder { get; set; }
        bool ShowLegendVarNames { get; set; }
        double LegendFontSize { get; set; }
        int ChartWidth { get; set; }
        int ChartHeight { get; set; }
        string YAxisFormattingString { get; set; }
        string Y2AxisFormattingString { get; set; }
        ComponentArt.Win.DataVisualization.Charting.Dock LegendDock { get; set; }
    }

    public class AreaChartSettings : IChartSettings
    {
        public bool ShowSeriesLine { get; set; }
        public bool UseRefValues { get; set; }
        public bool ShowAnnotationsY2 { get; set; }
        public bool ShowDefaultGridLines { get; set; }
        public CompositionKind Composition { get; set; }
        public Palette Palette { get; set; }
        public LineKind AreaType { get; set; }
        public LineKind LineKindY2 { get; set; }
        public LineDashStyle LineDashStyleY2 { get; set; }
        public string Y2AxisLabel { get; set; }
        public string Y2AxisLegendTitle { get; set; }
        public int TransparencyTop { get; set; }
        public int TransparencyBottom { get; set; }
        public string YAxisLabel { get; set; }
        public string XAxisLabel { get; set; }
        public double Y2LineThickness { get; set; }
        public XAxisLabelType XAxisLabelType { get; set; }
        public int XAxisLabelRotation { get; set; }
        public string ChartTitle { get; set; }
        public string ChartStrataTitle { get; set; }
        public string ChartSubTitle { get; set; }
        public bool ShowLegend { get; set; }
        public bool ShowLegendBorder { get; set; }
        public bool ShowLegendVarNames { get; set; }
        public double LegendFontSize { get; set; }
        public int ChartWidth { get; set; }
        public int ChartHeight { get; set; }
        public string YAxisFormattingString { get; set; }
        public string Y2AxisFormattingString { get; set; }

        public ComponentArt.Win.DataVisualization.Charting.Dock LegendDock { get; set; }
    }

    public class LineChartSettings : IChartSettings
    {
        public bool UseRefValues { get; set; }
        public bool ShowAnnotations { get; set; }
        public bool ShowAnnotationsY2 { get; set; }
        public bool ShowDefaultGridLines { get; set; }
        public Palette Palette { get; set; }
        public LineKind LineKind { get; set; }
        public LineKind LineKindY2 { get; set; }
        public LineDashStyle LineDashStyleY2 { get; set; }
        public string Y2AxisLabel { get; set; }
        public string Y2AxisLegendTitle { get; set; }
        public string YAxisLabel { get; set; }
        public string XAxisLabel { get; set; }
        public double LineThickness { get; set; }
        public double Y2LineThickness { get; set; }
        public XAxisLabelType XAxisLabelType { get; set; }
        public int XAxisLabelRotation { get; set; }
        public string ChartTitle { get; set; }
        public string ChartStrataTitle { get; set; }
        public string ChartSubTitle { get; set; }
        public bool ShowLegend { get; set; }
        public bool ShowLegendBorder { get; set; }
        public bool ShowLegendVarNames { get; set; }
        public double LegendFontSize { get; set; }
        public int ChartWidth { get; set; }
        public int ChartHeight { get; set; }
        public string YAxisFormattingString { get; set; }
        public string Y2AxisFormattingString { get; set; }

        public ComponentArt.Win.DataVisualization.Charting.Dock LegendDock { get; set; }
    }

    public class ColumnChartSettings : IChartSettings
    {
        public bool UseDiffColors { get; set; }
        public bool UseRefValues { get; set; }
        public bool ShowAnnotations { get; set; }
        public bool ShowAnnotationsY2 { get; set; }
        public bool ShowDefaultGridLines { get; set; }
        public CompositionKind Composition { get; set; }
        public Orientation Orientation { get; set; }
        public BarSpacing BarSpacing { get; set; }
        public Palette Palette { get; set; }
        public BarKind BarKind { get; set; }
        public LineKind LineKindY2 { get; set; }
        public LineDashStyle LineDashStyleY2 { get; set; }
        public string Y2AxisLabel { get; set; }
        public string Y2AxisLegendTitle { get; set; }
        public string YAxisLabel { get; set; }
        public string XAxisLabel { get; set; }
        public double Y2LineThickness { get; set; }
        public XAxisLabelType XAxisLabelType { get; set; }
        public int XAxisLabelRotation { get; set; }
        public string ChartTitle { get; set; }
        public string ChartStrataTitle { get; set; }
        public string ChartSubTitle { get; set; }
        public bool ShowLegend { get; set; }
        public bool ShowLegendBorder { get; set; }
        public bool ShowLegendVarNames { get; set; }
        public double LegendFontSize { get; set; }
        public int ChartWidth { get; set; }
        public int ChartHeight { get; set; }
        public string YAxisFormattingString { get; set; }
        public string Y2AxisFormattingString { get; set; }
        public bool Y2IsCumulativePercent { get; set; }

        public double YAxisFrom { get; set; }
        public double YAxisTo { get; set; }
        public double YAxisStep { get; set; }

        public ComponentArt.Win.DataVisualization.Charting.Dock LegendDock { get; set; }
    }

    public class HistogramChartSettings : IChartSettings
    {
        public bool UseRefValues { get; set; }
        public bool ShowAnnotations { get; set; }
        public bool ShowAnnotationsY2 { get; set; }
        public bool ShowDefaultGridLines { get; set; }
        public Palette Palette { get; set; }
        public BarKind BarKind { get; set; }
        public LineKind LineKindY2 { get; set; }
        public LineDashStyle LineDashStyleY2 { get; set; }
        public BarSpacing BarSpacing { get; set; }
        public int Step { get; set; }
        public DateType DateType { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string Y2AxisLabel { get; set; }
        public string Y2AxisLegendTitle { get; set; }

        public string YAxisLabel { get; set; }
        public string XAxisLabel { get; set; }
        public double Y2LineThickness { get; set; }
        public XAxisLabelType XAxisLabelType { get; set; }
        public int XAxisLabelRotation { get; set; }
        public string ChartTitle { get; set; }
        public string ChartStrataTitle { get; set; }
        public string ChartSubTitle { get; set; }
        public bool ShowLegend { get; set; }
        public bool ShowLegendBorder { get; set; }
        public bool ShowLegendVarNames { get; set; }
        public double LegendFontSize { get; set; }
        public int ChartWidth { get; set; }
        public int ChartHeight { get; set; }

        public string YAxisFormattingString { get; set; }
        public string Y2AxisFormattingString { get; set; }

        public double YAxisFrom { get; set; }
        public double YAxisTo { get; set; }
        public double YAxisStep { get; set; }

        public ComponentArt.Win.DataVisualization.Charting.Dock LegendDock { get; set; }
    }

    public enum DateType
    {
        Minute,
        Hour,
        Day,
        Month,
        Year
    }

    public class PieChartSettings : IChartSettings
    {
        public bool UseRefValues { get; set; }
        public bool ShowAnnotations { get; set; }
        public bool ShowAnnotationsY2 { get; set; }
        public bool ShowDefaultGridLines { get; set; }
        public bool ShowAnnotationLabel { get; set; }
        public bool ShowAnnotationValue { get; set; }
        public bool ShowAnnotationPercent { get; set; }
        public Palette Palette { get; set; }
        public LineKind LineKindY2 { get; set; }
        public LineDashStyle LineDashStyleY2 { get; set; }
        public string Y2AxisLabel { get; set; }
        public string Y2AxisLegendTitle { get; set; }
        public double Y2LineThickness { get; set; }
        public string YAxisLabel { get; set; }
        public string XAxisLabel { get; set; }
        public XAxisLabelType XAxisLabelType { get; set; }
        public int XAxisLabelRotation { get; set; }
        public string ChartTitle { get; set; }
        public string ChartStrataTitle { get; set; }
        public string ChartSubTitle { get; set; }
        public bool ShowLegend { get; set; }
        public bool ShowLegendBorder { get; set; }
        public bool ShowLegendVarNames { get; set; }
        public double LegendFontSize { get; set; }
        public PieChartKind PieChartKind { get; set; }
        public int ChartWidth { get; set; }
        public int ChartHeight { get; set; }
        public double AnnotationPercent { get; set; }

        public string YAxisFormattingString { get; set; }
        public string Y2AxisFormattingString { get; set; }

        public ComponentArt.Win.DataVisualization.Charting.Dock LegendDock { get; set; }
    }
}

