using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Epi;
using Epi.Fields;

namespace EpiDashboard
{
    public class RatesParameters : GadgetParametersBase
    {
        public bool SortColumnsByTabOrder { get; set; }
        public bool UsePromptsForColumnNames { get; set; }
        public bool ShowColumnHeadings { get; set; }
        public bool ShowLineColumn { get; set; }
        public bool ShowNullLabels { get; set; }
        public bool NumerDistinct { get; set; }
        public bool DenomDistinct { get; set; }
        public bool ShowAsPercent { get; set; }
        public string PrimaryGroupField { get; set; }
        public string SecondaryGroupField { get; set; }
        public int MaxColumnLength { get; set; }
        public int MaxRows { get; set; }
        public DataFilters NumerFilter { get; set; }
        public DataFilters DenomFilter { get; set; }

        public RatesParameters()
            : base()
        {
            SortColumnsByTabOrder = false;
            UsePromptsForColumnNames = false;
            ShowColumnHeadings = true;
            ShowLineColumn = true;
            NumerDistinct = false;
            DenomDistinct = false;
            ShowNullLabels = true;
            ShowAsPercent = true;
            PrimaryGroupField = String.Empty;
            SecondaryGroupField = String.Empty;
            GadgetTitle = "Rates";
        }
    }
}
