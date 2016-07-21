using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_AndExp : EnterRule 
    {
        EnterRule NotExp = null;
        EnterRule AndExp = null;

        public Rule_AndExp(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            // <Not Exp> AND <And Exp> 
            // <Not Exp> 

            this.NotExp = EnterRule.BuildStatments(pContext, pToken.Tokens[0]);
            if (pToken.Tokens.Length > 1)
            {
                this.AndExp = EnterRule.BuildStatments(pContext, pToken.Tokens[2]);
            }
        }

        /// <summary>
        /// performs an "and" operation on boolean expresssions
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            if (this.AndExp == null)
            {
                result = this.NotExp.Execute();
            }
            else
            {
                // dpb: this needs to be fixed to work with more then just strings
                
                string LHS = this.NotExp.Execute().ToString().ToLowerInvariant();
                string RHS = this.AndExp.Execute().ToString().ToLowerInvariant();

                result = "true" == LHS && LHS == RHS;
            }

            return result;
        }
    }
}
