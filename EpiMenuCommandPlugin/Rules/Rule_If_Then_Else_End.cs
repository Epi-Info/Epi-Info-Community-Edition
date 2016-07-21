using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace EpiMenu.CommandPlugin
{
    public class Rule_If_Then_Else_End : Rule
    {
        //Rule_Expression IfClause;
        Rule IfClause;
        Rule ThenClause;
        Rule ElseClause;

        public Rule_If_Then_Else_End(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {

            /*
             <If_Statement> ::=      IF <Expression> THEN NewLine <Statements> NewLine END
             <If_Else_Statement> ::= IF <Expression> THEN NewLine <Statements> NewLine ELSE NewLine <Statements> NewLine END
            */


            //IfClause = new Rule_Expression(pContext, (NonterminalToken)pToken.Tokens[1]);
            ThenClause = new Rule_Statements(pContext, (NonterminalToken)pToken.Tokens[4]);
            if (pToken.Tokens.Length > 7)
            {
                ElseClause = new Rule_Statements(pContext, (NonterminalToken)pToken.Tokens[8]);
            }
        }


        /// <summary>
        /// performs execution of the If...Then...Else command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            if (IfClause.Execute().ToString().ToLowerInvariant() == "true")
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
