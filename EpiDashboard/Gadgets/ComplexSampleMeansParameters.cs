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
        public string PSUVariableName { get; set; }

        public ComplexSampleMeansParameters()
            : base()
        {
            GadgetTitle = DashboardSharedStrings.GADGET_CONFIG_TITLE_COMPLEX_MEANS;
            ShowANOVA = true;
            columnsToHide = new List<int>();
            PSUVariableName = string.Empty;
        }

        public ComplexSampleMeansParameters(ComplexSampleMeansParameters parameters)
            : base(parameters)
        {
            GadgetTitle = parameters.GadgetTitle;
            ShowANOVA = parameters.ShowANOVA;
            columnsToHide = parameters.columnsToHide;
            PSUVariableName = parameters.PSUVariableName;
        }
    }
}
