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
    public partial class Rule_Exp_Func : AnalysisRule
    {
        private List<AnalysisRule> ParameterList = new List<AnalysisRule>();

        public Rule_Exp_Func(Rule_Context pContext, NonterminalToken pToken)
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
            object result = null;
            
            object p1 = this.ParameterList[0].Execute().ToString();
            double param1;

            if(double.TryParse(p1.ToString(), out param1))
            {
                result = Math.Pow(Math.E, param1);
            }

            return result;
        }
        
    }
}
