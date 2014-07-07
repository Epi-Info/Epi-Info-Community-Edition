using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_AutoSearch: EnterRule 
    {
        string[] IdentifierList = null;
        bool AlwaysShow = false;
        string[] DisplayList = null;

        public Rule_AutoSearch(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            /*<Auto_Search_Statement> ::= AUTOSEARCH <IdentifierList> 
                                          |AUTOSEARCH <IdentifierList> Always
                                          |AUTOSEARCH <IdentifierList> DisplayList <IdentifierList> 
                                          |AUTOSEARCH <IdentifierList> DisplayList <IdentifierList> Always*/


            this.IdentifierList = this.GetCommandElement(pToken.Tokens, 1).Split(' ');
            if (pToken.Tokens.Length > 2)
            {

                if (pToken.Tokens[2].ToString().Equals("Always",StringComparison.OrdinalIgnoreCase))
                {
                    this.AlwaysShow = true;
                }
                else
                {
                    this.DisplayList = this.GetCommandElement(pToken.Tokens, 3).Split(' ');
                }
            }


            if (pToken.Tokens.Length == 5)
            {
                this.AlwaysShow = true;
            }
        }


        /// <summary>
        /// uses the EnterCheckCodeInterface to perform an AutoSearch
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            this.Context.EnterCheckCodeInterface.AutoSearch(this.IdentifierList, this.DisplayList, this.AlwaysShow);
            return null;
        }
    }
}
