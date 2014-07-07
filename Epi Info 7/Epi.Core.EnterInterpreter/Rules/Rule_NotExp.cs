using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_NotExp: EnterRule
    {
        EnterRule CompareExp = null;
        bool isNotStatement = false;

        public Rule_NotExp(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            //<Not Exp> ::= NOT <Compare Exp> | <Compare Exp>
            if (pToken.Tokens.Length == 1)
            {
                CompareExp = EnterRule.BuildStatments(pContext, pToken.Tokens[0]);
            }
            else
            {
                CompareExp = EnterRule.BuildStatments(pContext, pToken.Tokens[1]);
                this.isNotStatement = true;
            }

        }

        /// <summary>
        /// performs a NOT or pass-thru operation
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            if (this.isNotStatement)
            {
                //result = this.CompareExp.Execute();
                
                if (this.CompareExp.Execute().ToString().Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    result = "false";
                }
                else
                {
                    result = "true";
                }
            }
            else
            {
                result = this.CompareExp.Execute();
            }

            /*
            if (this.CompareExp.Execute().ToString() == "true")
            {
                result = "false";
            }
            else
            {
                result = "true";
            }*/

            return result;

        }

    }
}
