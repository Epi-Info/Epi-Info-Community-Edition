using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;


namespace Epi.Core.AnalysisInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_Hours reduction.
    /// </summary>
    public partial class Rule_Hours : Rule_DateDiff
    {
        private List<AnalysisRule> ParameterList = new List<AnalysisRule>();

        public Rule_Hours(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext, pToken, FunctionUtils.DateInterval.Minute)
        {
            this.ParameterList = AnalysisRule.GetFunctionParameters(pContext, pToken);
        }

        /// <summary>
        /// Executes the reduction
        /// </summary>
        /// <returns>returns the date difference in minutes between two dates.</returns>
        public override object Execute()
        {
            object result = null;

            object p1 = this.ParameterList[0].Execute();
            object p2 = this.ParameterList[1].Execute();

            if (p1 is DateTime && p2 is DateTime)
            {
                DateTime param1 = (DateTime)p1;
                DateTime param2 = (DateTime)p2;

                TimeSpan timeSpan = param2 - param1;
                result = Math.Round(timeSpan.TotalHours);
            }

            return result;
        }
    }
}
