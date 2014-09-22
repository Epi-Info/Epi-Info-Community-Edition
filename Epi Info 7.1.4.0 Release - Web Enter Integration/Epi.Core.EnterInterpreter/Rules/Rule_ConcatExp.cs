using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_ConcatExp: EnterRule
    {
        EnterRule AddExp = null;
        string op = null;
        EnterRule ConcatExp = null;

        public Rule_ConcatExp(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            /*::= <Add Exp> '&' <Concat Exp>
		   						| <Add Exp>*/

            this.AddExp = EnterRule.BuildStatments(pContext, pToken.Tokens[0]);
            if (pToken.Tokens.Length > 1)
            {
                this.op = pToken.Tokens[1].ToString();

                this.ConcatExp = EnterRule.BuildStatments(pContext, pToken.Tokens[2]);
            }

        }
        /// <summary>
        /// performs concatenation of string via the '&' operator
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            if (op == null)
            {
                result = this.AddExp.Execute();
            }
            else
            {
                object LHSO = this.AddExp.Execute(); 
                object RHSO = this.ConcatExp.Execute();

                if (LHSO != null && RHSO != null)
                {
                    result = LHSO.ToString() + RHSO.ToString();
                }
                else if (LHSO != null)
                {
                    if (LHSO.GetType() == typeof(string))
                    {
                        result = LHSO;
                    }
                }
                else if (RHSO.GetType() == typeof(string))
                {
                    result = RHSO;
                }
            }

            return result;
        }
    }
}
