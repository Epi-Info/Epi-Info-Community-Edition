using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_Cos reduction
    /// </summary>
    public partial class Rule_Cos : AnalysisRule
    {

        private List<AnalysisRule> ParameterList = new List<AnalysisRule>();

        public Rule_Cos(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {

            this.ParameterList = AnalysisRule.GetFunctionParameters(pContext, pToken);
        }
        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the COS of a number.</returns>
        public override object Execute()
        {
            double result = 0.0;
            object p1 = this.ParameterList[0].Execute();
            if (p1 != null)
            {
                if (Double.TryParse(p1.ToString(), out result))
                {

                    return Math.Cos(result);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

        }
    }
}
