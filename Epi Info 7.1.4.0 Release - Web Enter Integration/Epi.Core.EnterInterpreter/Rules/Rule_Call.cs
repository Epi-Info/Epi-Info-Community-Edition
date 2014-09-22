﻿using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;


namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_Call : EnterRule
    {
        string Identifier = null;

        public Rule_Call(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            //<Call_Statement> ::= CALL Identifier
            this.Identifier = this.GetCommandElement(pToken.Tokens, 1);
        }

        /// <summary>
        /// Call Statment rule
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            if (this.Context.Subroutine.ContainsKey(this.Identifier.ToLower()))
            {
                EnterRule Sub = this.Context.Subroutine[this.Identifier];
                return Sub.Execute();
            }

            return null;
        }
    }
}
