using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_FileDate reduction.
    /// </summary>
    public partial class Rule_FileDate : AnalysisRule
    {
        private List<AnalysisRule> ParameterList = new List<AnalysisRule>();

        public Rule_FileDate(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.ParameterList = AnalysisRule.GetFunctionParameters(pContext, pToken);
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns></returns>
        public override object Execute()
        {
            object result = null;

            string fileName = this.ParameterList[0].Execute().ToString();

            if (System.IO.File.Exists(fileName))
            {
                result = System.IO.File.GetLastWriteTime(fileName);
            }

            return result;
        }
        
    }
}
