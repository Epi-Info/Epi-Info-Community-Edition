using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_Abs reduction.
    /// </summary>
    public partial class Rule_Abs : EnterRule
    {
        private List<EnterRule> ParameterList = new List<EnterRule>();

        public Rule_Abs(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.ParameterList = EnterRule.GetFunctionParameters(pContext, pToken);
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the absolute value of two numbers.</returns>
        public override object Execute()
        {
            double result = 0.0;
            if (Double.TryParse(this.ParameterList[0].Execute().ToString(), out result))
            {

                return Math.Abs(result);
            }
            else
            {
                return null;
            }
        }
        
    }
}
