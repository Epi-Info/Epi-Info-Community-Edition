using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_UnHide : EnterRule
    {
        bool IsExceptList = false;
        string[] IdentifierList = null;

        public Rule_UnHide(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            if (pToken.Tokens.Length > 2)
            {
                //<Unhide_Except_Statement> ::= UNHIDE '*' EXCEPT <IdentifierList>
                this.IsExceptList = true;
                this.IdentifierList = this.GetCommandElement(pToken.Tokens, 3).ToString().Split(' ');
            }
            else
            {
                //<Unhide_Some_Statement> ::= UNHIDE <IdentifierList>
                this.IdentifierList = this.GetCommandElement(pToken.Tokens,1).ToString().Split(' ');
            }
        }
        /// <summary>
        /// performs execution of the UNHIDE command via the EnterCheckCodeInterface.UnHide method
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            this.Context.EnterCheckCodeInterface.UnHide(this.IdentifierList, this.IsExceptList);

            return null;

        }
    }
}
