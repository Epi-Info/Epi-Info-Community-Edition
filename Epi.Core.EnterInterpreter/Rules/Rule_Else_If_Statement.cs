using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_Else_If_Statement : EnterRule 
    {
        EnterRule Expression;
        EnterRule Statements1;
        EnterRule Statements2;

        public Rule_Else_If_Statement(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            /*
              
<Else_If_Statement>          ::=  Else-If <Expression> Then <Statements>
                                | Else-If <Expression> Then <Statements> Else <Statements>
                                | Else-If <Expression> Then <Statements> <Else_If_Statement>              
            */

            Expression = EnterRule.BuildStatments(pContext, pToken.Tokens[1]);
            Statements1 = EnterRule.BuildStatments(pContext, pToken.Tokens[3]);
            if(pToken.Tokens.Length > 4)
            {
                if (this.GetCommandElement(pToken.Tokens, 4).Equals("Else", StringComparison.OrdinalIgnoreCase))
                {
                    Statements2 = EnterRule.BuildStatments(pContext, pToken.Tokens[5]);
                }
                else
                {
                    Statements2 = EnterRule.BuildStatments(pContext, pToken.Tokens[4]);
                }
            }
        }

        /// <summary>
        /// performs execution of the If...Then...Else command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            if (Expression.Execute().ToString().ToLower() == "true")
            {
                result = Statements1.Execute();
            }
            else
            {
                if (Statements2 != null)
                {
                    result = Statements2.Execute();
                }
            }
            
            return result;
        }

        
        private void ExecuteThenClause()
        {
            this.Statements1.Execute();
        }
        private void ExecuteElseClause()
        {
            if (this.Statements2 != null)
            {
                this.Statements2.Execute();
            }
        }
    }
}
