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
    public class ParetoChartParameters : ParetoChartParametersBase
    {
        //Pareto Chart parameters
        #region Properties

        public bool UseDiffColors { get; set; }
        public CompositionKind Composition { get; set; }
        public Orientation Orientation { get; set; }
        public BarKind BarKind { get; set; }
        public string ChartStrataTitle { get; set; }

        #endregion  // Properties

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ParetoChartParameters()
            : base()
        {
            ColumnNames = new List<string>();
            WeightVariableName = string.Empty;
            CrosstabVariableName = string.Empty;
            Y2AxisType = 0;
            //Parameters.Y2AxisVariable ==> ColumnNames[1]
            //  Sorting/Grouping
            StrataVariableNames = new List<string>();
            SortHighToLow = true;
            //  Display
            GadgetTitle = DashboardSharedStrings.GADGET_CONFIG_TITLE_PARETO_CHART;
            GadgetDescription = String.Empty;
            Width = 800;
            Height = 500;
            ShowAllListValues = false;
            ShowCommentLegalLabels = false;
            IncludeMissing = false;
            //  Colors/Styles
            UseDiffColors = false;
            UseRefValues = true;
            ShowAnnotations = false;
            Y2ShowAnnotations = false;
            ShowGridLines = true;
            Composition = CompositionKind.SideBySide;
            BarSpace = BarSpacing.Default;
            Orientation = Orientation.Vertical;
            Palette = 12;
            BarKind = BarKind.Rectangle;
            Y2LineDashStyle = LineDashStyle.Solid;
            Y2LineThickness = 2;
            PaletteColors = new List<string>();
            //  Labels
            YAxisLabel = "Count";
            YAxisFormat = String.Empty;
            Y2AxisLabel = String.Empty;
            Y2AxisLegendTitle = String.Empty;
            Y2AxisFormat = String.Empty;
            XAxisLabelType = 0;
            XAxisLabel = String.Empty;
            XAxisAngle = 0;
            ChartTitle = String.Empty;
            ChartSubTitle = String.Empty;
            //  Legend
            ShowLegend = false;
            ShowLegendBorder = false;
            ShowLegendVarNames = false;
            LegendFontSize = 12;
            LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Right;
            ChartStrataTitle = String.Empty;
            YAxisFontSize = 12;
            YAxisLabelFontSize = 12;
            XAxisFontSize = 12;
            XAxisLabelFontSize = 12;
        }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        public ParetoChartParameters(ParetoChartParameters parameters)
            : base (parameters)
        {
            //MainVariableNames = parameters.MainVariableNames;
            ColumnNames = parameters.ColumnNames;
            //GadgetStatusUpdate = parameters.GadgetStatusUpdate;
            InputVariableList = parameters.InputVariableList;
            CustomFilter = parameters.CustomFilter;
            CustomSortColumnName = parameters.CustomSortColumnName;
            IncludeFullSummaryStatistics = parameters.IncludeFullSummaryStatistics;
            ShowAllListValues = parameters.ShowAllListValues;
            ShowCommentLegalLabels = parameters.ShowCommentLegalLabels;
            SortHighToLow = parameters.SortHighToLow;
            IncludeMissing = parameters.IncludeMissing;
            CrosstabVariableName = parameters.CrosstabVariableName;
            WeightVariableName = parameters.WeightVariableName;
            StrataVariableNames = parameters.StrataVariableNames;
            IgnoreRowLimits = parameters.IgnoreRowLimits;
            SortVariables = parameters.SortVariables;

            //from ColumnChartGadgetParametersBase
            UseDiffColors = parameters.UseDiffColors ;
            Composition = parameters.Composition ;
            //BarSpacing = parameters.BarSpacing;
            Orientation = parameters.Orientation ;
            BarKind = parameters.BarKind;

            //from ChartGadgetParametersBase
            //Advanced options
            ChartWidth = parameters.ChartWidth;
            ChartHeight = parameters.ChartHeight;
            Y2AxisType = parameters.Y2AxisType;

            //Display options
            //  Color and styles
            UseRefValues = parameters.UseRefValues;
            ShowAnnotations = parameters.ShowAnnotations;
            Y2ShowAnnotations = parameters.Y2ShowAnnotations;
            ShowGridLines = parameters.ShowGridLines;
            Palette = parameters.Palette;
            //Y2LineType = parameters.Y2LineType;
            Y2LineDashStyle = parameters.Y2LineDashStyle;
            Y2LineThickness = parameters.Y2LineThickness;
            //  Labels
            YAxisLabel = parameters.YAxisLabel;
            YAxisFormat = parameters.YAxisFormat;
            Y2AxisLabel = parameters.Y2AxisLabel;
            Y2AxisLegendTitle = parameters.Y2AxisLegendTitle;
            Y2AxisFormat = parameters.Y2AxisFormat;
            XAxisLabelType = parameters.XAxisLabelType;
            XAxisLabel = parameters.XAxisLabel;
            XAxisAngle = parameters.XAxisAngle;
            ChartTitle = parameters.ChartTitle;
            ChartSubTitle = parameters.ChartSubTitle;
            ChartStrataTitle = parameters.ChartStrataTitle;
            //  Legend
            ShowLegend = parameters.ShowLegend;
            ShowLegendBorder = parameters.ShowLegendBorder;
            ShowLegendVarNames = parameters.ShowLegendVarNames;
            //LegendFontSize in ChartGadgetParametersBase.cs
            LegendDock = parameters.LegendDock;
            YAxisFontSize = parameters.YAxisFontSize;
            YAxisLabelFontSize = parameters.YAxisLabelFontSize;
            XAxisFontSize = parameters.XAxisFontSize;
            XAxisLabelFontSize = parameters.XAxisLabelFontSize;
            PaletteColors = parameters.PaletteColors;
        }
    }
}
