using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_ExprList :EnterRule
    {

        EnterRule Expression = null;
        EnterRule ExprList = null;

        public Rule_ExprList(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            /*::= <Expression> ',' <Expr List> | <Expression> */

            this.Expression = EnterRule.BuildStatments(pContext, pToken.Tokens[0]);

            if (pToken.Tokens.Length > 1)
            {
                this.ExprList = EnterRule.BuildStatments(pContext, pToken.Tokens[2]);
            }

        }


        /// <summary>
        /// performs execution of a list of expressions
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            result = this.Expression.Execute();

            if (this.ExprList != null)
            {
                result = this.ExprList.Execute();
            }


            return result;
        }
    }
}
