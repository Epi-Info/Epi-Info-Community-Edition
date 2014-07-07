using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_NegateExp: EnterRule
    {
        string op = null;
        EnterRule Value = null;

        public Rule_NegateExp(Rule_Context pContext, NonterminalToken pTokens) : base(pContext)
        {
            //<Negate Exp> ::= '-' <Value>
            // or
            //<Negate Exp> ::= <Value>

            if(pTokens.Tokens.Length > 1)
            {
                this.op = this.GetCommandElement(pTokens.Tokens,0);
                this.Value = EnterRule.BuildStatments(pContext, pTokens.Tokens[1]);
            }
            else
            {
                this.Value = EnterRule.BuildStatments(pContext, pTokens.Tokens[0]);
            }

        }
        /// <summary>
        /// performs the negation or pass-thru operation
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;
            if (this.op != null)
            {
                if (this.op == "-")
                {
                    result = -1.0 * double.Parse(Value.Execute().ToString());
                }
                else
                {
                    result = double.Parse(Value.Execute().ToString());
                }
            }
            else
            {
                result = Value.Execute();
            }

            return result;
        }
    }
}
