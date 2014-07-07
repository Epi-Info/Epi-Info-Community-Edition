using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpiDashboard
{
    public class Crosstab
    {
        public string VariableName { get; set; }
        public object Value { get; set; }
        public string DisplayValue { get; set; }
        public string Filter { get; set; }
        public string SafeFilter { get; set; }

        public Crosstab(string varName, object value, string displayValue, string filter, string safeFilter)
        {
            VariableName = varName;
            Value = value;
            DisplayValue = displayValue;
            Filter = filter;
            SafeFilter = safeFilter;
        }
    }
}
