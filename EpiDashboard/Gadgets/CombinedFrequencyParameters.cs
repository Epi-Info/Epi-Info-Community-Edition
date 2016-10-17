using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Epi;
using Epi.Fields;


namespace EpiDashboard
{
    public class CombinedFrequencyParameters : GadgetParametersBase
    {
        public CombineModeTypes CombineMode { get; set; }
        public bool SortHighToLow { get; set; }
        public bool ShowDenominator { get; set; }
        public string TrueValue { get; set; }

        public CombinedFrequencyParameters()
            : base()
        {
            CombineMode = CombineModeTypes.Automatic;
            SortHighToLow = true;
            ShowDenominator = true;
            TrueValue = String.Empty;
            GadgetTitle = DashboardSharedStrings.GADGET_CONFIG_TITLE_COMBINEDFEQ;
        }       
    }
}
