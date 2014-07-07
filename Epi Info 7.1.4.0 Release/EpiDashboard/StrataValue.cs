using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpiDashboard
{
    public class Strata
    {
        public List<string> VariableNames { get; set; }
        public List<object> Values { get; set; }
        public string Filter { get; set; }
        public string SafeFilter { get; set; }

        public Strata(List<string> varNames, List<object> values, string filter, string safeFilter)
        {
            VariableNames = varNames;
            Values = values;
            Filter = filter;
            SafeFilter = safeFilter;
        }
    }
}
