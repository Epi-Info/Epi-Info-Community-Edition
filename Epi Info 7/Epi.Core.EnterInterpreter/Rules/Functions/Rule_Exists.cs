using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_Exists reduction.
    /// </summary>
    public partial class Rule_Exists : EnterRule
    {
        private List<EnterRule> ParameterList = new List<EnterRule>();

        public Rule_Exists(Rule_Context pContext, NonterminalToken pToken)
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
                            this.Context.CommandVariableCheck.Add(id.ToLower(), "exists");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the absolute value of two numbers.</returns>
        public override object Execute()
        {
            object result  = System.IO.File.Exists(this.ParameterList[0].Execute().ToString());

            return result;
        }
        
    }
}
