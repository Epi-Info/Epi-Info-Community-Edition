using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_Step reduction.
    /// </summary>
    public partial class Rule_Step : AnalysisRule
    {
        private List<AnalysisRule> ParameterList = new List<AnalysisRule>();

        public Rule_Step(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.ParameterList = AnalysisRule.GetFunctionParameters(pContext, pToken);
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>This function returns 0 for all values of one expression less than a second, 1 for all values of the first expression greater than or equal to the second.</returns>
        public override object Execute()
        {
            object result = null;
            
            object p1 = this.ParameterList[0].Execute().ToString();
            object p2 = this.ParameterList[1].Execute().ToString();

            double param1;
            double param2;


            if (double.TryParse(p1.ToString(), out param1) && double.TryParse(p2.ToString(), out param2))
            {
                if (param1 < param2)
                {
                    result = 0.0;
                }
                else
                {
                    result = 1.0;
                }

            }

            return result;
        }
        
    }
}
