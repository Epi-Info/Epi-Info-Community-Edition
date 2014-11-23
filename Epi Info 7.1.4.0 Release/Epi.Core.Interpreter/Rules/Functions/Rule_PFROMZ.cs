using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_Abs reduction.
    /// </summary>
    public partial class Rule_PFROMZ : AnalysisRule
    {
        private List<AnalysisRule> ParameterList = new List<AnalysisRule>();

        public Rule_PFROMZ(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.ParameterList = AnalysisRule.GetFunctionParameters(pContext, pToken);
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the absolute value of two numbers.</returns>
        public override object Execute()
        {
            double result = 0.0;
            object p1 = this.ParameterList[0].Execute();
            if (p1 != null)
            {
                if (Double.TryParse(p1.ToString(), out result))
                {
                    result = Math.Round(AnthStat.NutriDataCalc.GetPercentile(result), 3);
                    if (result >= 99.9999)
                    {
                        result = 99.999;
                    }
                    else if (result <= -99.9999)
                    {
                        result = -99.999;
                    }
                    return result;
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
