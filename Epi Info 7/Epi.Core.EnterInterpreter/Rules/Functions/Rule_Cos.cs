using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_Cos reduction
    /// </summary>
    public partial class Rule_Cos : EnterRule
    {

        private List<EnterRule> ParameterList = new List<EnterRule>();

        public Rule_Cos(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {

            this.ParameterList = EnterRule.GetFunctionParameters(pContext, pToken);
            if (ParameterList.Count > 0)
            {
                foreach (var item in ParameterList)
                {
                    if (item is Rule_Value)
                    {
                        var id = ((Epi.Core.EnterInterpreter.Rules.Rule_Value)(item)).Id;
                        if (!this.Context.CommandVariableCheck.ContainsKey(id.ToLower()))
                        {
                            this.Context.CommandVariableCheck.Add(id.ToLower(), "cos");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the COS of a number.</returns>
        public override object Execute()
        {
            double result = 0.0;
            if (Double.TryParse(this.ParameterList[0].Execute().ToString(), out result))
            {

                return Math.Cos(result);
            }
            else
            {
                return null;
            }
            
        }
    }
}
