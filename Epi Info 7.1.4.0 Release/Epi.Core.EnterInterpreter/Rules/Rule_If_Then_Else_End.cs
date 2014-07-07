using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    public class Rule_If_Then_Else_End : EnterRule 
    {
        EnterRule IfClause;
        EnterRule ThenClause;
        EnterRule ElseClause;

        public Rule_If_Then_Else_End(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            /*
              
            <If_Statement>                  ::=   IF <Expression> THEN  <Statements>  END-IF 
                                                | IF <Expression> THEN  <Statements>  END
            <If_Else_Statement>              ::=  IF <Expression> THEN  <Statements> <Else_If_Statement>  END-IF 
                                                | IF <Expression> THEN  <Statements>  <Else_If_Statement>  END
                                                    IF <Expression> THEN <Statements> ELSE  <Statements>  END-IF 
                                                | IF <Expression> THEN <Statements> ELSE  <Statements>  END
             */

            IfClause = EnterRule.BuildStatments(pContext, pToken.Tokens[1]);
            ThenClause = EnterRule.BuildStatments(pContext, pToken.Tokens[3]);
            if (this.GetCommandElement(pToken.Tokens, 4).Equals("Else", StringComparison.OrdinalIgnoreCase))
            {
                ElseClause = EnterRule.BuildStatments(pContext, pToken.Tokens[5]);
            }
                /*
            else
            {
                ElseClause = EnterRule.BuildStatments(pContext, pToken.Tokens[4]);
            }*/
        }

        /// <summary>
        /// performs execution of the If...Then...Else command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            if (IfClause.Execute().ToString().ToLower() == "true")
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

    }
}
