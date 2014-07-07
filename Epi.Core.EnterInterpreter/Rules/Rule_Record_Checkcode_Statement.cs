using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_Record_Checkcode_Statement : EnterRule
    {

        private EnterRule BeginBefore = null;
        private EnterRule BeginAfter = null;

        private string TextField = null;

        public Rule_Record_Checkcode_Statement(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.TextField = this.ExtractTokensWithFormat(pToken.Tokens);

            //<Record_Checkcode_Statement> ::= Record <Begin_Before_statement> <Begin_After_statement> End
            for (int i = 1; i < pToken.Tokens.Length; i++)
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

            this.Context.Record_Checkcode = this;

            //if (this.BeginBefore.Statements != null)
            //{
                if (this.Context.BeforeCheckCode.ContainsKey("record"))
                {
                    this.Context.BeforeCheckCode.Remove("record");
                }
                this.Context.BeforeCheckCode.Add("record", this.BeginBefore);
            //}


            //if (this.BeginAfter.Statements != null)
            //{
                if (this.Context.AfterCheckCode.ContainsKey("record"))
                {
                    this.Context.AfterCheckCode.Remove("record");
                }
                this.Context.AfterCheckCode.Add("record", this.BeginAfter);
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
