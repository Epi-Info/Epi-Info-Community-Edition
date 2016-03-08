using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_Field_Checkcode_Statement : EnterRule
    {
        private EnterRule BeginBefore = null;
        private EnterRule BeginAfter = null;
        private EnterRule BeginClick = null;

        private string TextField = null;

        private string Identifier = string.Empty;

        public Rule_Field_Checkcode_Statement(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            //<Field_Checkcode_Statement> ::=  Field Identifier <Begin_Before_statement> <Begin_After_statement> <Begin_Click_statement>  End

            this.TextField = this.ExtractTokensWithFormat(pToken.Tokens);

            this.Identifier = this.GetCommandElement(pToken.Tokens, 1).Trim(new char[] { '[',']'});
            if (pContext.IsVariableValidationEnable)
            {
                if (!string.IsNullOrEmpty(Identifier))
                {

                    if (!this.Context.CommandVariableCheck.ContainsKey(Identifier.ToLower()))
                    {
                        this.Context.CommandVariableCheck.Add(Identifier, "Field");
                    }

                }
            }
            if (Context.ParsedFieldNames.Contains(Identifier.ToLower()) == false)
            {
                Context.ParsedFieldNames.Add(Identifier.ToLower());
                
            }
            
            for (int i = 2; i < pToken.Tokens.Length; i++)
            {
                if (pToken.Tokens[i] is NonterminalToken)
                {
                    NonterminalToken T = (NonterminalToken) pToken.Tokens[i];
                    switch (T.Symbol.ToString())
                    {
                        case "<Begin_Before_statement>":
                            this.BeginBefore = EnterRule.BuildStatments(pContext, T);
                            break;
                        case "<Begin_After_statement>":
                            this.BeginAfter = EnterRule.BuildStatments(pContext, T);
                            break;
                        case "<Begin_Click_statement>":
                            this.BeginClick = EnterRule.BuildStatments(pContext, T);
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
            this.Identifier = this.Identifier.ToLower();

            if (this.Context.Field_Checkcode.ContainsKey(this.Identifier))
            {
                this.Context.Field_Checkcode.Remove(this.Identifier);
            }
            this.Context.Field_Checkcode.Add(this.Identifier, this);

            //if (this.BeginBefore.Statements != null)
            //{
                if (this.Context.FieldBeforeCheckCode.ContainsKey(this.Identifier))
                {
                    this.Context.FieldBeforeCheckCode.Remove(this.Identifier);
                }
                this.Context.FieldBeforeCheckCode.Add(this.Identifier, this.BeginBefore);
            //}

            //if (this.BeginAfter.Statements != null)
            //{
                if (this.Context.FieldAfterCheckCode.ContainsKey(this.Identifier))
                {
                    this.Context.FieldAfterCheckCode.Remove(this.Identifier);
                }
                this.Context.FieldAfterCheckCode.Add(this.Identifier, this.BeginAfter);
            //}


            //if (this.BeginClick.Statements != null)
            //{
                if (this.Context.FieldClickCheckCode.ContainsKey(this.Identifier))
                {
                    this.Context.FieldClickCheckCode.Remove(this.Identifier);
                }
                this.Context.FieldClickCheckCode.Add(this.Identifier, this.BeginClick);
            //}
            return null;
        }

        public override string ToString()
        {
            return this.TextField;
        }


        public override bool IsNull() 
        {

            return BeginBefore == null && BeginAfter == null && BeginClick == null; 
        } 

    }


 
}
