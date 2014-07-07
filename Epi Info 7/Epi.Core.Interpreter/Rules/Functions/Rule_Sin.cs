using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_Sin reduction
    /// </summary>
    public partial class Rule_Sin : AnalysisRule
    {

        private List<AnalysisRule> ParameterList = new List<AnalysisRule>();

        public Rule_Sin(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {

            this.ParameterList = AnalysisRule.GetFunctionParameters(pContext, pToken);
        }
        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the Sin of a number.</returns>
        public override object Execute()
        {
            double result = 0.0;
            if (Double.TryParse(this.ParameterList[0].Execute().ToString(), out result))
            {

                return Math.Sin(result);
            }
            else
            {
                return null;
            }

        }
    }
}
