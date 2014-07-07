using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_CheckCodeBlock : EnterRule
    {
        EnterRule CheckCodeBlock = null;

        public Rule_CheckCodeBlock(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            /* <CheckCodeBlock> ::=   <DefineVariables_Statement>
                                | <View_Checkcode_Statement>
                                | <Record_Checkcode_Statement>
                    | <Page_Checkcode_Statement>  
                    | <Field_Checkcode_Statement>
                    | <Subroutine_Statement>  */
            switch (pToken.Rule.Rhs[0].ToString())
            {
                case "<DefineVariables_Statement>":
                    this.CheckCodeBlock = EnterRule.BuildStatments(pContext, pToken.Tokens[0]);
                    break;
                case "<View_Checkcode_Statement>":
                    this.CheckCodeBlock = EnterRule.BuildStatments(pContext, pToken.Tokens[0]);
                    break;
                case "<Record_Checkcode_Statement>":
                    this.CheckCodeBlock = EnterRule.BuildStatments(pContext, pToken.Tokens[0]);
                    break;
                case "<Page_Checkcode_Statement>":
                    this.CheckCodeBlock = EnterRule.BuildStatments(pContext, pToken.Tokens[0]);
                    break;
                case "<Field_Checkcode_Statement>":
                    this.CheckCodeBlock = EnterRule.BuildStatments(pContext, pToken.Tokens[0]);
                    break;
                case "<Subroutine_Statement>":
                    this.CheckCodeBlock = EnterRule.BuildStatments(pContext, pToken.Tokens[0]);
                    break;
            }
        }



        /// <summary>
        ///executes CheckCodeBlock statement
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            CheckCodeBlock.Execute();

            return null;
        }

        public override bool IsNull() { return CheckCodeBlock == null; } 
    }
}
