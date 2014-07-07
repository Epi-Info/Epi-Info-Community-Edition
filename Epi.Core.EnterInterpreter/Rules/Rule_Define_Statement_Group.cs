using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_Define_Statement_Group : EnterRule
    {
        public EnterRule  Define_Statement_Type = null;
        public EnterRule Define_Statement_Group = null;

        public Rule_Define_Statement_Group(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            //<Define_Statement_Group> ::= <Define_Statement_Type> <Define_Statement_Group> | <Define_Statement_Type>

            Define_Statement_Type = EnterRule.BuildStatments(pContext, pToken.Tokens[0]);

            if (pToken.Tokens.Length > 1)
            {
                Define_Statement_Group = EnterRule.BuildStatments(pContext, pToken.Tokens[1]);
            }
        }

        
       /// <summary>
        /// performs execute command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = Define_Statement_Type.Execute();

            if (Define_Statement_Group != null)
            {
                result = Define_Statement_Group.Execute();
            }

            return result;
        }



    }
}
