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

        public MeansParameters()
            : base()
        {
            ShowANOVA = true;
            columnsToHide = new List<int>();
            Precision = "4";
        }
    }
}
