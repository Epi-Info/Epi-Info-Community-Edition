using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_Highlight :EnterRule
    {
        bool IsExceptList = false;
        string[] IdentifierList = null;

        public Rule_Highlight(Rule_Context pContext, NonterminalToken pToken)
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
            if (IdentifierList.Length > 0)
            {
                foreach (var item in IdentifierList)
                {
                    if (!this.Context.CommandVariableCheck.ContainsKey(item.ToLower()))
                    {
                        this.Context.CommandVariableCheck.Add(item, "highlight");
                    }
                }
            }
        }


        /// <summary>
        /// performs execution of the HIDE command via the EnterCheckCodeInterface.Hide method
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            this.Context.EnterCheckCodeInterface.Highlight(this.IdentifierList, this.IsExceptList);
            return null;
        }
    }
}
