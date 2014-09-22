using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    /// <summary>
    /// Reduction to convert number values to a time.
    /// </summary>
    public partial class Rule_NumToTime : AnalysisRule
    {

        private List<AnalysisRule> ParameterList = new List<AnalysisRule>();


        public Rule_NumToTime(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {

            this.ParameterList = AnalysisRule.GetFunctionParameters(pContext, pToken);
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
             object temp = ParameterList[0].Execute();


            if (temp != null && Int32.TryParse(temp.ToString(), out hour))
            {
                temp = ParameterList[1].Execute();
                if (temp != null && Int32.TryParse(ParameterList[1].Execute().ToString(), out minute))
                {
                    temp = ParameterList[2].Execute();
                    if (temp != null && Int32.TryParse(ParameterList[2].Execute().ToString(), out second))
                    {
                        result = new TimeSpan(hour, minute, second);
                    }
                }
            }

            return result;
        }
    }
}
