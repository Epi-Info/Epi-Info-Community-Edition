using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_PowExp: EnterRule 
    {
        EnterRule NegateExp1 = null;
        EnterRule NegateExp2 = null;

        public Rule_PowExp(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            /* 	::= <Negate Exp> '^' <Negate Exp>  | <Negate Exp> */

            this.NegateExp1 = EnterRule.BuildStatments(pContext, pToken.Tokens[0]);
            if (pToken.Tokens.Length > 1)
            {
                this.NegateExp2 = EnterRule.BuildStatments(pContext, pToken.Tokens[2]);
            }
            
        }


        /// <summary>
        /// raises a number to a power and returns the resulting number
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            if (NegateExp2 == null)
            {
                result = this.NegateExp1.Execute();
            }
            else
            {

                string LHS = this.NegateExp1.Execute().ToString();
                string RHS = this.NegateExp2.Execute().ToString();

                result = System.Math.Pow(double.Parse(LHS),double.Parse(RHS));
            }

            return result;
        }
    }
}
