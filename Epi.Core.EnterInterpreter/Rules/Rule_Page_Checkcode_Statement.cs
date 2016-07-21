using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_Page_Checkcode_Statement :EnterRule
    {
        private EnterRule BeginBefore = null;
        private EnterRule BeginAfter = null;
        private string Identifier = null;

        private string TextField = null;

        public Rule_Page_Checkcode_Statement(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.TextField = this.ExtractTokensWithFormat(pToken.Tokens);

            //<Page_Checkcode_Statement> ::= Page Identifier <Begin_Before_statement> <Begin_After_statement> End
            this.Identifier = this.GetCommandElement(pToken.Tokens, 1).Trim(new char[] { '[', ']' });
            for (int i = 2; i < pToken.Tokens.Length; i++)
            {
                if (pToken.Tokens[i] is NonterminalToken)
                {
                    NonterminalToken T = (NonterminalToken)pToken.Tokens[i];
                    switch (T.Symbol.ToString())
                    {
                        case "<Begin_Before_statement>":
                            this.BeginBefore = EnterRule.BuildStatments(pContext, T);
                            break;
                        case "<Begin_After_statement>":
                            this.BeginAfter = EnterRule.BuildStatments(pContext, T);
                            break;
                    }
                }
            }
        }


        
       /// <summary>
        /// performs execute command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            this.Identifier = this.Identifier.ToLowerInvariant();


            if (this.Context.Page_Checkcode.ContainsKey(this.Identifier))
            {
                this.Context.Page_Checkcode.Remove(this.Identifier);
            }
            this.Context.Page_Checkcode.Add(this.Identifier, this);

            //if (this.BeginBefore.Statements != null)
            //{
                if (this.Context.PageBeforeCheckCode.ContainsKey(this.Identifier))
                {
                    this.Context.PageBeforeCheckCode.Remove(this.Identifier);
                }
                this.Context.PageBeforeCheckCode.Add(this.Identifier, this.BeginBefore);
            //}


            //if (this.BeginAfter.Statements != null)
            //{
                if (this.Context.PageAfterCheckCode.ContainsKey(this.Identifier))
                {
                    this.Context.PageAfterCheckCode.Remove(this.Identifier);
                }
                this.Context.PageAfterCheckCode.Add(this.Identifier, this.BeginAfter);
            //}
            return null;
        }

        public override string ToString()
        {
            return this.TextField;
        }

        public override bool IsNull() { return BeginBefore == null && BeginAfter == null; } 
    }
}
