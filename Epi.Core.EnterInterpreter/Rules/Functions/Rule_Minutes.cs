using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_Minutes reduction.
    /// </summary>
    public partial class Rule_Minutes : Rule_DateDiff
    {
        private List<EnterRule> ParameterList = new List<EnterRule>();

        public Rule_Minutes(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext, pToken, FunctionUtils.DateInterval.Minute)
        {
            this.ParameterList = EnterRule.GetFunctionParameters(pContext, pToken);
            if (ParameterList.Count > 0)
            {
                foreach (var item in ParameterList)
                {
                    var id = ((Epi.Core.EnterInterpreter.Rules.Rule_Value)(item)).Id;
                    if (!this.Context.CommandVariableCheck.ContainsKey(id.ToLower()))
                    {
                        this.Context.CommandVariableCheck.Add(id.ToLower(), "minutes");
                    }
                }
            }
        }

        /// <summary>
        /// Executes the reduction
        /// </summary>
        /// <returns>returns the date difference in minutes between two dates.</returns>
        public override object Execute()
        {
            object result = null;

            object p1 = this.ParameterList[0].Execute();
            object p2 = this.ParameterList[1].Execute();

            if (p1 is DateTime && p2 is DateTime)
            {
                DateTime param1 = (DateTime)p1;
                DateTime param2 = (DateTime)p2;

                TimeSpan timeSpan = param2 - param1;
                result = timeSpan.TotalMinutes;

            }

            return result;
        }
    }

}
