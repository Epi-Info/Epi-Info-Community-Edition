using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public partial class Rule_Month : Rule_DatePart
    {
        private List<AnalysisRule> ParameterList = new List<AnalysisRule>();

        public Rule_Month(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext, pToken, FunctionUtils.DateInterval.Year)
        {
            this.ParameterList = AnalysisRule.GetFunctionParameters(pContext, pToken);
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the date difference in years between two dates.</returns>
        public override object Execute()
        {
            object result = null;

            object p1 = this.ParameterList[0].Execute();

            if (p1 is DateTime)
            {

                DateTime param1 = (DateTime)p1;
                result = param1.Month;
            }
             
            return result;
        }
    }
}
