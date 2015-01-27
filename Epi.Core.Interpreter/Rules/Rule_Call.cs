using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_Call : AnalysisRule
    {

        bool HasRun = false;

        private AnalysisRule Statements = null;
        private string Identifier = null;

        private string TextField = null;

        public Rule_Call(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {

            //<Subroutine_Statement> ::= Sub Identifier <Statements> End | Sub Identifier End
            this.Identifier = this.GetCommandElement(pToken.Tokens, 1);          
        }

        
       /// <summary>
        /// performs execute command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            if (!this.HasRun)
            {
                this.Identifier = this.Identifier.ToLower();

                if (this.Context.SubroutineList.ContainsKey(this.Identifier))
                {
                    ((Epi.Core.AnalysisInterpreter.Rules.Rule_Statements)(this.Context.Subroutine[this.Identifier])).Execute();                   
                }            
                this.HasRun = true;
            }
            return null;
        }

    }
}
