using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_If_Then_Else_End : AnalysisRule 
    {
        AnalysisRule IfClause;
        AnalysisRule ThenClause;
        AnalysisRule ElseClause;

        public Rule_If_Then_Else_End(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {

            /*
             <If_Statement> ::=      IF <Expression> THEN <Statements> END
             <If_Else_Statement> ::= IF <Expression> THEN <Statements> ELSE <Statements> END
            */


            IfClause = AnalysisRule.BuildStatments(pContext, (NonterminalToken)pToken.Tokens[1]);
            ThenClause = AnalysisRule.BuildStatments(pContext, (NonterminalToken)pToken.Tokens[3]);
            if (pToken.Tokens.Length > 5)
            {
                ElseClause = AnalysisRule.BuildStatments(pContext, (NonterminalToken)pToken.Tokens[5]);
            }
        }


        /// <summary>
        /// performs execution of the If...Then...Else command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            if (this.Context.CurrentDataRow == null)
            {
                List<System.Data.DataRow> DT = this.Context.GetOutput(IfClause, this.ExecuteThenClause, this.ExecuteElseClause);
            }
            else
            {
                if (IfClause.Execute().ToString().Equals("true", StringComparison.CurrentCultureIgnoreCase))
                {
                    result = ThenClause.Execute();
                }
                else
                {
                    if (ElseClause != null)
                    {
                        result = ElseClause.Execute();
                    }
                }
            }

            return result;
        }

        
        private void ExecuteThenClause()
        {
            this.ThenClause.Execute();
        }
        private void ExecuteElseClause()
        {
            if (this.ElseClause != null)
            {
                this.ElseClause.Execute();
            }
        }
    }
}
