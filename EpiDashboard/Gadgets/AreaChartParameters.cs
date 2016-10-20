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
    public class AreaChartParameters : AreaChartParametersBase
    {
        //Area Chart parameters
        #region Properties

        #endregion  // Properties

        /// <summary>
        /// Default Constructor
        /// </summary>
        public AreaChartParameters()
            : base()
        {
            ColumnNames = new List<string>();
            WeightVariableName = string.Empty;
            CrosstabVariableName = string.Empty;
            Y2AxisType = 0;
            //  Sorting/Grouping
            StrataVariableNames = new List<string>();
            SortHighToLow = false;
            //  Display
            GadgetTitle = DashboardSharedStrings.GADGET_CONFIG_TITLE_AREACHART;
            GadgetDescription = String.Empty;
            Width = 800;
            Height = 500;
            ShowAllListValues = false;
            ShowCommentLegalLabels = false;
            IncludeMissing = false;
            //  Colors/Styles
            AreaKind = LineKind.Auto;
            UseRefValues = true;
            ShowAnnotations = false;
            Y2ShowAnnotations = false;
            ShowGridLines = true;
            Composition = CompositionKind.Stacked;
            Palette = 12;
            Y2LineDashStyle = LineDashStyle.Solid;
            Y2LineThickness = 2;
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
            PaletteColors = new List<string>();


        }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        public AreaChartParameters(AreaChartParameters parameters)
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
            Composition = parameters.Composition ;

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
