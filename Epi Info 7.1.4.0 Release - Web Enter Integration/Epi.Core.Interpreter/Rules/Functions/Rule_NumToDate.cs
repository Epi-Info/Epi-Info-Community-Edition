using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    /// <summary>
    /// Reduction to convert number values to a date.
    /// </summary>
    public partial class Rule_NumToDate : AnalysisRule
    {
        private List<AnalysisRule> ParameterList = new List<AnalysisRule>();

        public Rule_NumToDate(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.ParameterList = AnalysisRule.GetFunctionParameters(pContext, pToken);
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the value converted to a date.</returns>
        public override object Execute()
        {
            object result = null;

            int year;
            int month;
            int day;

            if (Int32.TryParse(ParameterList[0].Execute().ToString(), out year))
            {
                if (Int32.TryParse(ParameterList[1].Execute().ToString(), out month))
                {
                    if (Int32.TryParse(ParameterList[2].Execute().ToString(), out day))
                    {
                        result = new DateTime(year, month, day);
                    }
                }
            }

            return result;
        }
    }
}
