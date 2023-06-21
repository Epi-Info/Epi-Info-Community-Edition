using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_Abs reduction.
    /// </summary>
    public partial class Rule_GroupRowIndex : AnalysisRule
    {
        private List<AnalysisRule> ParameterList = new List<AnalysisRule>();

        public Rule_GroupRowIndex(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.ParameterList = AnalysisRule.GetFunctionParameters(pContext, pToken);
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns the absolute value of two numbers.</returns>
        public override object Execute()
        {
            object result = 1;
            
            object p1 = this.ParameterList[0].Execute().ToString();
            object p2 = ((Rule_Value)(this.ParameterList[0])).ExecuteOnPreceeding();
            if (p2 != null) {
                p2 = p2.ToString();
                if (p1.Equals(p2))
                {
                    string assigningvariable = this.Context.VariableValueList.Keys.First();
                    string p2result = this.Context.PreceedingDataRow[assigningvariable].ToString();
                    int p2intresult;
                    if (int.TryParse(p2result, out p2intresult))
                    {
                        result = p2intresult + 1;
                    }
                }
            }
            return result;
        }
        
    }
}
