using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /// <summary>
    /// Class for the Rule_IsUnique reduction.
    /// </summary>
    public partial class Rule_IsUnique : EnterRule
    {
        /*<Is_Unique_Statement> ::= ISUNIQUE <IdentifierList> 
                                     |ISUNIQUE <IdentifierList> Always
        */
        string[] IdentifierList = null;
        //private List<EnterRule> ParameterList = new List<EnterRule>();

        public Rule_IsUnique(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.IdentifierList = this.GetCommandElement(pToken.Tokens, 0).Split(' ');
            //this.ParameterList = EnterRule.GetFunctionParameters(pContext, pToken);
        }

        /// <summary>
        /// Executes the reduction.
        /// </summary>
        /// <returns>Returns true when the value is unique.</returns>
        public override object Execute()
        {
            //this.Context.EnterCheckCodeInterface.AutoSearch(this.IdentifierList, this.IdentifierList, true); 

            object result = this.Context.EnterCheckCodeInterface.IsUnique(this.IdentifierList); 

            return result;
        }
        
    }
}
