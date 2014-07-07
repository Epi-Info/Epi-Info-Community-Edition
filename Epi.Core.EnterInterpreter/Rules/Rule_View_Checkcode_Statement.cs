using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_View_Checkcode_Statement : EnterRule
    {
        private EnterRule BeginBefore = null;
        private EnterRule BeginAfter = null;

        private string TextField = null;

        public Rule_View_Checkcode_Statement(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {

            this.TextField = this.ExtractTokensWithFormat(pToken.Tokens);
            // <View_Checkcode_Statement> ::= View <Begin_Before_statement> <Begin_After_statement> End
            for (int i = 1; i < pToken.Tokens.Length; i++)
            {
                if (pToken.Tokens[i] is NonterminalToken)
                {
                    NonterminalToken T = (NonterminalToken)pToken.Tokens[i];
                    switch (T.Symbol.ToString())
                    {
                        case "<Begin_Before_statement>":
                            this.BeginBefore = EnterRule.BuildStatments(pContext, pToken.Tokens[i]);
                            break;
                        case "<Begin_After_statement>":
                            this.BeginAfter = EnterRule.BuildStatments(pContext, pToken.Tokens[i]);
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
            this.Context.View_Checkcode = this;

            //if (this.BeginBefore.Statements != null)
            //{

                if (this.Context.BeforeCheckCode.ContainsKey("view"))
                {
                    this.Context.BeforeCheckCode.Remove("view");
                }
                this.Context.BeforeCheckCode.Add("view", this.BeginBefore);
            //}

            //if (this.BeginAfter.Statements != null)
            //{
                if (this.Context.AfterCheckCode.ContainsKey("view"))
                {
                    this.Context.AfterCheckCode.Remove("view");
                }
                this.Context.AfterCheckCode.Add("view", this.BeginAfter);
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
