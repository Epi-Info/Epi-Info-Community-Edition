using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Epi;
using Epi.Fields;

namespace EpiDashboard
{
    public class MeansParameters : FrequencyParametersBase
    {
        public bool ShowANOVA { get; set; }
        public List<int> columnsToHide;
        public string PairIDVariableName { get; set; }

        public MeansParameters()
            : base()
        {
            ShowANOVA = true;
            columnsToHide = new List<int>();
            Precision = "4";
            GadgetTitle = DashboardSharedStrings.GADGET_CONFIG_TITLE_MEANS;
        }
    }
}
