using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_GoTo : EnterRule
    {
        string Destination = null;

        public Rule_GoTo(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            //<Go_To_Variable_Statement> ::= GOTO  Identifier
            //<Go_To_Page_Statement> ::= GOTO DecLiteral
            // GOTO '-'DecLiteral
            // GOTO '+'DecLiteral
            if (pToken.Tokens.Length == 2)
            {
                this.Destination = this.GetCommandElement(pToken.Tokens, 1);
            }
            else
            {
                this.Destination = this.GetCommandElement(pToken.Tokens, 1) + this.GetCommandElement(pToken.Tokens, 2);
            }
        }



        /// <summary>
        /// executes the GOTO command via the EnterCheckCodeInterface.GoTo method
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            this.Context.EnterCheckCodeInterface.GoTo(Destination);

            return null;
        }
    }
}
