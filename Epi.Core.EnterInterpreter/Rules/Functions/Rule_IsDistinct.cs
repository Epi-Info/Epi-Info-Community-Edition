using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_IsDistinct reduction.
    /// </summary>
    public partial class Rule_IsDistinct : EnterRule
    {
        string[] IdentifierList = null;
        private List<EnterRule> ParameterList = new List<EnterRule>();

        public Rule_IsDistinct(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.ParameterList = EnterRule.GetFunctionParameters(pContext, pToken);
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns true when the value is unique.</returns>
        public override object Execute()
        {
            //this.Context.EnterCheckCodeInterface.AutoSearch(this.IdentifierList, this.IdentifierList, true); 
            object result = true;

            return result;
        }
        
    }
}
