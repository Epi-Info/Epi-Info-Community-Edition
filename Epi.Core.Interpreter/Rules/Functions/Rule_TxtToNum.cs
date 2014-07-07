using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public partial class Rule_TxtToNum : AnalysisRule
    {
        private List<AnalysisRule> ParameterList = new List<AnalysisRule>();

        public Rule_TxtToNum(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.ParameterList = AnalysisRule.GetFunctionParameters(pContext, pToken);
        }

        /// <summary>
        /// returns a number based on a text string
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            result = ParameterList[0].Execute();

            double doubleValue;
            if (!Util.IsEmpty(result))
            {
                if (result.ToString().ToLower().Equals("true") || result.ToString().ToLower().Equals("false"))
                {
                    result = (result.ToString().ToLower() == "true" ? 1 : 0);
                }

                if (Double.TryParse(result.ToString(), out doubleValue))
                {
                    result = doubleValue;
                }
                else
                {
                    result = 0.0;
                }
            }

            return result;
        }
    }
}
