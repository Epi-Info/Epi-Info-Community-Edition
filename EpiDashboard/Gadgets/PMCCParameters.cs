using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Epi;
using Epi.Fields;

namespace EpiDashboard
{
    public class PMCCParameters : FrequencyParametersBase
    {
        public bool TreatOutcomeAsContinuous { get; set; }
        public bool StrataSummaryOnly { get; set; }
        public bool RowColPercents { get; set; }
        public bool ShowConfLimits { get; set; }
        public bool ShowCumulativePercent { get; set; }
        public List<string> YesValues { get; set; }
        public List<string> NoValues { get; set; }
        public string MaxColumnNameLength { get; set; }
        public string LayoutMode { get; set; }

        public PMCCParameters()
            : base()
        {
            GadgetTitle = "Matched Pair Case-Control";
            TreatOutcomeAsContinuous = false;
            StrataSummaryOnly = false;
            RowColPercents = true;
            ShowConfLimits = true;
            ShowCumulativePercent = true;
            YesValues = new List<string>();
            NoValues = new List<string>();
            MaxColumnNameLength = "24";
            LayoutMode = "vertical";
        }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        public PMCCParameters(PMCCParameters parameters)
            : base(parameters)
        {
            GadgetTitle = parameters.GadgetTitle;
            TreatOutcomeAsContinuous = parameters.TreatOutcomeAsContinuous;
            StrataSummaryOnly = parameters.StrataSummaryOnly;
            RowColPercents = parameters.RowColPercents;
            ShowConfLimits = parameters.ShowConfLimits;
            ShowCumulativePercent = parameters.ShowCumulativePercent;
            YesValues = parameters.YesValues;
            NoValues = parameters.NoValues;
            MaxColumnNameLength = parameters.MaxColumnNameLength;
            LayoutMode = parameters.LayoutMode;
        }
    }
}
