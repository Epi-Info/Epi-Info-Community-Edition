using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Epi;
using Epi.Fields;

namespace EpiDashboard
{
    public class ComplexSampleMeansParameters : FrequencyParametersBase
    {
        public bool ShowANOVA { get; set; }
        public List<int> columnsToHide;

        public ComplexSampleMeansParameters()
            : base()
        {
            ShowANOVA = true;
            columnsToHide = new List<int>();
        }
    }
}
