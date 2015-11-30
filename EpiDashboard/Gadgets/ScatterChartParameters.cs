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
    public class ScatterChartParameters : ScatterChartParametersBase
    {
        //Scatter Chart parameters
        #region Properties
        public int MarkerType { get; set; }        

        #endregion  // Properties

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ScatterChartParameters()
            : base()
        {
            ColumnNames = new List<string>();
            CrosstabVariableName = String.Empty;
            //  Sorting/Grouping
            //  Display
            GadgetTitle = "Scatter Chart";
            GadgetDescription = String.Empty;
            Width = 800;
            Height = 500;
            //  Colors/Styles
            Palette = 12;
            MarkerType = 0;
            //  Labels
            //--EI-196
            //YAxisLabel = "Count";
            YAxisLabel = string.Empty;
            //
            XAxisLabelType = 0;
            XAxisLabel = String.Empty;
            XAxisAngle = 0;
            ChartTitle = String.Empty;
            ChartSubTitle = String.Empty;
            //  Legend
            //Ei-196
            //ShowLegend = false;
            //ShowLegendBorder = false;
            //ShowLegendVarNames = false;
            ShowLegend = true;
            ShowLegendBorder = true;
            ShowLegendVarNames = true;
            //--
            LegendFontSize = 12;
            LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Right;
            PaletteColors = new List<string>();
        }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        public ScatterChartParameters(ScatterChartParameters parameters)
            : base (parameters)
        {
            ColumnNames = parameters.ColumnNames;
            CrosstabVariableName = parameters.CrosstabVariableName;
            InputVariableList = parameters.InputVariableList;
            CustomFilter = parameters.CustomFilter;
            CustomSortColumnName = parameters.CustomSortColumnName;

            //from ChartGadgetParametersBase
            //Advanced options
            ChartWidth = parameters.ChartWidth;
            ChartHeight = parameters.ChartHeight;

            //Display options
            //  Color and styles
            Palette = parameters.Palette;
            MarkerType = parameters.MarkerType;
            //  Labels
            YAxisLabel = parameters.YAxisLabel;
            XAxisLabelType = parameters.XAxisLabelType;
            XAxisLabel = parameters.XAxisLabel;
            XAxisAngle = parameters.XAxisAngle;
            ChartTitle = parameters.ChartTitle;
            ChartSubTitle = parameters.ChartSubTitle;
            //  Legend
            ShowLegend = parameters.ShowLegend;
            ShowLegendBorder = parameters.ShowLegendBorder;
            ShowLegendVarNames = parameters.ShowLegendVarNames;
            //LegendFontSize in ChartGadgetParametersBase.cs
            LegendDock = parameters.LegendDock;
            PaletteColors = parameters.PaletteColors;
        }
    }
}
