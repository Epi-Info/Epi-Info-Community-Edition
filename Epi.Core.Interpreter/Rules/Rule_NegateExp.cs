using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public partial class Rule_NegateExp : AnalysisRule
    {
        string op = null;
        AnalysisRule Value = null;

        public Rule_NegateExp(Rule_Context pContext, NonterminalToken pTokens) : base(pContext)
        {
            //<Negate Exp> ::= '-' <Value>
            // or
            //<Negate Exp> ::= <Value>

            if(pTokens.Tokens.Length > 1)
            {
                this.op = this.GetCommandElement(pTokens.Tokens,0);
                //this.Value = new Rule_Value(pContext, pTokens.Tokens[1]);
                this.Value = AnalysisRule.BuildStatments(pContext, pTokens.Tokens[1]);
            }
            else
            {
                //this.Value = new Rule_Value(pContext, pTokens.Tokens[0]);
                this.Value = AnalysisRule.BuildStatments(pContext, pTokens.Tokens[0]);
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
