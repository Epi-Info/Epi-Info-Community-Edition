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
    public class PieChartParameters : PieChartParametersBase
    {
        //Pie Chart parameters
        #region Properties

        #endregion  // Properties

        /// <summary>
        /// Default Constructor
        /// </summary>
        public PieChartParameters()
            : base()
        {
            ColumnNames = new List<string>();
            WeightVariableName = string.Empty;
            CrosstabVariableName = string.Empty;
            //  Display
            GadgetTitle = "Pie Chart";
            GadgetDescription = String.Empty;
            Width = 800;
            Height = 500;
            ShowAllListValues = false;
            ShowCommentLegalLabels = false;
            SortHighToLow = false;
            IncludeMissing = false;
            //  Colors/Styles
            PieChartKind = PieChartKind.Pie2D;
            ShowAnnotations = true;
            ShowAnnotationValue = true;
            ShowAnnotationLabel = true;
            ShowAnnotationPercent = true;
            AnnotationPercent = 20;
            Palette = 12;
            //  Labels
            ChartTitle = String.Empty;
            ChartSubTitle = String.Empty;
            //  Legend
            ShowLegend = true;
            ShowLegendBorder = false;
            ShowLegendVarNames = false;
            LegendFontSize = 12;
            LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Right;
            ChartStrataTitle = String.Empty;
        }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        public PieChartParameters(PieChartParameters parameters)
            : base (parameters)
        {
            ColumnNames = parameters.ColumnNames;
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
            IgnoreRowLimits = parameters.IgnoreRowLimits;
            SortVariables = parameters.SortVariables;

            //from ChartGadgetParametersBase
            //Advanced options
            ChartWidth = parameters.ChartWidth;
            ChartHeight = parameters.ChartHeight;

            //Display options
            //  Color and styles
            PieChartKind = parameters.PieChartKind;
            UseRefValues = parameters.UseRefValues;
            ShowAnnotations = parameters.ShowAnnotations;
            ShowAnnotationLabel = parameters.ShowAnnotationLabel;
            ShowAnnotationValue = parameters.ShowAnnotationValue;
            ShowAnnotationPercent = parameters.ShowAnnotationPercent;
            AnnotationPercent = parameters.AnnotationPercent;
            Palette = parameters.Palette;
            //  Labels
            ChartTitle = parameters.ChartTitle;
            ChartSubTitle = parameters.ChartSubTitle;
            ChartStrataTitle = parameters.ChartStrataTitle;
            //  Legend
            ShowLegend = parameters.ShowLegend;
            ShowLegendBorder = parameters.ShowLegendBorder;
            ShowLegendVarNames = parameters.ShowLegendVarNames;
            LegendFontSize = parameters.LegendFontSize;
            LegendDock = parameters.LegendDock;
        }
    }
}
