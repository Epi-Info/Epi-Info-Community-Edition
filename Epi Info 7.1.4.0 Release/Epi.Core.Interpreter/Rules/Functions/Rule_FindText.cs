using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public partial class Rule_FindText : AnalysisRule
    {
        
        private List<AnalysisRule> ParameterList = new List<AnalysisRule>();


        public Rule_FindText(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            //SUBSTRING(fullString,startingIndex,length)
            this.ParameterList = AnalysisRule.GetFunctionParameters(pContext, pToken);
            
        }
        /// <summary>
        /// returns a substring index is 1 based ie 1 = first character
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;
            object p1 = this.ParameterList[0].Execute();
            object p2 = this.ParameterList[1].Execute();

            if (p1 != null && p2 != null)
            {
                result = p2.ToString().IndexOf(p1.ToString(), StringComparison.OrdinalIgnoreCase) + 1;
            }

            return result;
        }
    }
}
