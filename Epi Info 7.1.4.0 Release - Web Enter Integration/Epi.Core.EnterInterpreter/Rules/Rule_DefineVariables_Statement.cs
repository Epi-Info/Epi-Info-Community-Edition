using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_DefineVariables_Statement : EnterRule
    {
        private EnterRule define_Statements_Group = null;
        private string TextField = null;

        public Rule_DefineVariables_Statement(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.TextField = this.ExtractTokensWithFormat(pToken.Tokens);

            //<DefineVariables_Statement> ::= DefineVariables <Define_Statement_Group> End-DefineVariables
            if (pToken.Tokens.Length > 2)
            {
                //define_Statements_Group = new Rule_Define_Statement_Group(pContext, (NonterminalToken)pToken.Tokens[1]);
                define_Statements_Group = EnterRule.BuildStatments(pContext, pToken.Tokens[1]);
            }
        }

        
       /// <summary>
        /// performs execute command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {

            //if (define_Statements_Group != null && define_Statements_Group.Define_Statement_Type != null && this.Context.EnterCheckCodeInterface.IsExecutionEnabled)
            if (define_Statements_Group != null && this.Context.EnterCheckCodeInterface.IsExecutionEnabled)
            {
                this.Context.DefineVariablesCheckcode = this;
                return define_Statements_Group.Execute();
            }
            else
            {
                this.Context.DefineVariablesCheckcode = null;
                return null;
            }
        }

        public override string ToString()
        {
            return this.TextField;
        }


        public override bool IsNull() { return this.define_Statements_Group == null; } 
    }
}
