using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_Subroutine_Statement : AnalysisRule
    {
        bool HasRun = false;

        private AnalysisRule Statements = null;
        private string Identifier = null;

        private string TextField = null;

        public Rule_Subroutine_Statement(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {

            //<Subroutine_Statement> ::= Sub Identifier <Statements> End | Sub Identifier End
            this.Identifier = this.GetCommandElement(pToken.Tokens, 1);
            if (pToken.Tokens.Length > 3)
            {              
                this.Statements = AnalysisRule.BuildStatments(pContext, pToken.Tokens[2]);
            }
        }

        
       /// <summary>
        /// performs execute command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            if (!this.HasRun)
            {
                this.Identifier = this.Identifier.ToLowerInvariant();

                if (this.Context.SubroutineList.ContainsKey(this.Identifier))
                {
                    this.Context.Subroutine.Remove(this.Identifier);
                }
                this.Context.Subroutine.Add(this.Identifier, this.Statements);
                this.Context.SubroutineList.Add(this.Identifier,this.Statements);             
                this.HasRun = true;
            }
            return null;
        }


    }
}
