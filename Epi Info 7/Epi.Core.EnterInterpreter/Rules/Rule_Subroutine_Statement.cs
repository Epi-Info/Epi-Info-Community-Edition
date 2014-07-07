using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_Subroutine_Statement : EnterRule
    {
        private EnterRule Statements = null;
        private string Identifier = null;

        private string TextField = null;

        public Rule_Subroutine_Statement(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {

            this.TextField = this.ExtractTokensWithFormat(pToken.Tokens);

            //<Subroutine_Statement> ::= Sub Identifier <Statements> End | Sub Identifier End
            this.Identifier = this.GetCommandElement(pToken.Tokens, 1);
            if (pToken.Tokens.Length > 3)
            {
                //NonterminalToken T = (NonterminalToken)pToken.Tokens[2];
                //this.Statements = new Rule_Statements(pContext, T);
                this.Statements = EnterRule.BuildStatments(pContext, pToken.Tokens[2]);
            }
        }

        
       /// <summary>
        /// performs execute command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            this.Identifier = this.Identifier.ToLower();

            if (this.Context.Subroutine.ContainsKey(this.Identifier))
            {
                this.Context.Subroutine.Remove(this.Identifier);
            }
            this.Context.Subroutine.Add(this.Identifier, this.Statements);

            return null;
        }

        public override string ToString()
        {
            return this.TextField;
        }
    }
}
