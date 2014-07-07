using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Reduction to convert number values to a time.
    /// </summary>
    public partial class Rule_NumToTime : EnterRule
    {

        private List<EnterRule> ParameterList = new List<EnterRule>();


        public Rule_NumToTime(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {

            this.ParameterList = EnterRule.GetFunctionParameters(pContext, pToken);
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the value converted to a time.</returns>
        public override object Execute()
        {
            object result = null;

            int hour;
            int minute;
            int second;

            if (Int32.TryParse(ParameterList[0].Execute().ToString(), out hour))
            {
                if (Int32.TryParse(ParameterList[1].Execute().ToString(), out minute))
                {
                    if (Int32.TryParse(ParameterList[2].Execute().ToString(), out second))
                    {
                        result = new TimeSpan(hour, minute, second);
                    }
                }
            }

            return result;
        }
    }
}
