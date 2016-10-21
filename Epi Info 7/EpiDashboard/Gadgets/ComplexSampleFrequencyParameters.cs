using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Epi;
using Epi.Fields;

namespace EpiDashboard
{
    public class ComplexSampleFrequencyParameters : FrequencyParametersBase
    {
        public bool DrawBorders { get; set; }
        public bool DrawHeaderRow { get; set; }
        public bool DrawTotalRow { get; set; }
        public string PercentBarMode { get; set; }
        public int? RowsToDisplay { get; set; }
        public int? PercentBarWidth { get; set; }
        public bool ShowFrequencyCol { get; set; }
        public bool ShowPercentCol { get; set; }
        public bool ShowCumPercentCol { get; set; }
        public bool Show95CILowerCol { get; set; }
        public bool Show95CIUpperCol { get; set; }
        public bool ShowPercentBarsCol { get; set; }
        public string PSUVariableName { get; set; }

        public ComplexSampleFrequencyParameters()
            : base()
        {
            DrawBorders = true;
            DrawHeaderRow = true;
            DrawTotalRow = true;
            RowsToDisplay = null;
            PercentBarMode = "Bar only";
            PercentBarWidth = 100;
            ShowFrequencyCol = true;
            ShowPercentCol = true;
            ShowCumPercentCol = true;
            Show95CILowerCol = true;
            Show95CIUpperCol = true;
            ShowPercentBarsCol = true;
            IncludeFullSummaryStatistics = false;
            GadgetTitle = DashboardSharedStrings.GADGET_CONFIG_TITLE_COMPLEX_SAMPLE;
            PSUVariableName = string.Empty;
        }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        public ComplexSampleFrequencyParameters(ComplexSampleFrequencyParameters parameters)
            : base()
        {
            GadgetTitle = parameters.GadgetTitle;
            GadgetDescription = parameters.GadgetDescription;

            WeightVariableName = parameters.WeightVariableName;
            StrataVariableNames = parameters.StrataVariableNames;
            CrosstabVariableName = parameters.CrosstabVariableName;
            UseFieldPrompts = parameters.UseFieldPrompts;
            DrawBorders = parameters.DrawBorders;
            DrawHeaderRow = parameters.DrawHeaderRow;
            DrawTotalRow = parameters.DrawTotalRow;
            RowsToDisplay = parameters.RowsToDisplay;
            Precision = parameters.Precision;
            PercentBarMode = parameters.PercentBarMode;
            PercentBarWidth = parameters.PercentBarWidth;
            ShowAllListValues = parameters.ShowAllListValues;
            SortHighToLow = parameters.SortHighToLow;
            IncludeMissing = parameters.IncludeMissing;
            ShowFrequencyCol = parameters.ShowFrequencyCol;
            ShowPercentCol = parameters.ShowPercentCol;
            ShowCumPercentCol = parameters.ShowCumPercentCol;
            Show95CILowerCol = parameters.Show95CILowerCol;
            Show95CIUpperCol = parameters.Show95CIUpperCol;
            ShowPercentBarsCol = parameters.ShowPercentBarsCol;
            IncludeFullSummaryStatistics = parameters.IncludeFullSummaryStatistics;
            PSUVariableName = parameters.PSUVariableName;
        }
    }
}
