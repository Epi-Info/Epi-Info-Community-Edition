using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_Define_Statement_Type : EnterRule
    {
        EnterRule define_command = null;

        public Rule_Define_Statement_Type(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            /*<Define_Statement_Type> ::= <Define_Variable_Statement>
                            | <Define_Dll_Statement>
                            | <Define_Group_Statement> */

            this.define_command = EnterRule.BuildStatments(pContext, pToken.Tokens[0]);
            /*
            if (((NonterminalToken)pToken.Tokens[0]).Symbol.ToString() == "<Define_Dll_Statement>")
            {
                this.define_command = new Rule_DLL_Statement(pContext, (NonterminalToken) pToken.Tokens[0]);
            }
            else
            {
                this.define_command = new Rule_Define(pContext, (NonterminalToken) pToken.Tokens[0]);
            }*/
        }


        
       /// <summary>
        /// performs execute command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            return this.define_command.Execute();
        }
    }
}
