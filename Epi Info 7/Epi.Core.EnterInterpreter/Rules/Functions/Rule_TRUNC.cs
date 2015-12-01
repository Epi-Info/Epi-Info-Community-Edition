using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_TRUNC reduction.
    /// </summary>
    public partial class Rule_TRUNC : EnterRule
    {
        private List<EnterRule> ParameterList = new List<EnterRule>();

        public Rule_TRUNC(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.ParameterList = EnterRule.GetFunctionParameters(pContext, pToken);
            if (ParameterList.Count > 0)
            {
                foreach (var item in ParameterList)
                {
                    var id = ((Epi.Core.EnterInterpreter.Rules.Rule_Value)(item)).Id;
                    if (!this.Context.CommandVariableCheck.ContainsKey(id.ToLower()))
                    {
                        this.Context.CommandVariableCheck.Add(id.ToLower(), "trunc");
                    }
                }
            }
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the Truncation of a number.</returns>
        public override object Execute()
        {
            object result = null;
            object p1 = this.ParameterList[0].Execute().ToString();
            double param1;

            if (Double.TryParse(p1.ToString(), out param1))
            {

                result = Math.Truncate(param1);
            }
            else
            {

            }


            return result;
        }
        
    }
}
