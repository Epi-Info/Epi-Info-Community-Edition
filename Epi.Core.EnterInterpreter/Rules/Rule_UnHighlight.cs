using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_UnHighlight :EnterRule
    {
        bool IsExceptList = false;
        string[] IdentifierList = null;

        public Rule_UnHighlight(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            //<IdentifierList> ::= <IdentifierList> Identifier | Identifier

            if (pToken.Tokens.Length > 2)
            {
                //<Hide_Except_Statement> ::= HIDE '*' EXCEPT <IdentifierList>
                this.IsExceptList = true;
                this.IdentifierList = this.GetCommandElement(pToken.Tokens, 3).ToString().Split(' ');
            }
            else
            {
                //<Hide_Some_Statement> ::= HIDE <IdentifierList>
                this.IdentifierList = this.GetCommandElement(pToken.Tokens, 1).ToString().Split(' ');
            }
        }


        /// <summary>
        /// performs execution of the HIDE command via the EnterCheckCodeInterface.Hide method
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            this.Context.EnterCheckCodeInterface.UnHighlight(this.IdentifierList, this.IsExceptList);
            return null;
        }
    }
}
