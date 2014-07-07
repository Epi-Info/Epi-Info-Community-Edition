using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public partial class Rule_TxtToDate : AnalysisRule
    {
        private List<AnalysisRule> ParameterList = new List<AnalysisRule>();

        public Rule_TxtToDate(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.ParameterList = AnalysisRule.GetFunctionParameters(pContext, pToken);
        }
        /// <summary>
        /// returns a date based on a text string
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            result = ParameterList[0].Execute();

            DateTime dateValue;

            if (!Util.IsEmpty(result))
            {
                if (DateTime.TryParse(result.ToString(), out dateValue))
                {
                    result = dateValue;
                }
            }

            return result;
        }
    }
}
