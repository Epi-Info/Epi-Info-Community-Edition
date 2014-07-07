using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
   public class Rule_Statements: EnterRule 
   {
        //<Statements> ::= <Statements> <Statement> | <Statement>
       public EnterRule statement = null;
       public EnterRule statements = null;

       public Rule_Statements(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
       {
          //<Statements> ::= <Statements> <Statement> | <Statement>

           if (pToken.Tokens.Length > 1)
           {
               //NonterminalToken T;
               //T = (NonterminalToken)pToken.Tokens[0];
               //this.statements = new Rule_Statements(pContext, T);
               this.statements = EnterRule.BuildStatments(pContext, pToken.Tokens[0]);

               //T = ((NonterminalToken)pToken.Tokens[1]);
               //this.statement = new Rule_Statement(pContext, T);
               this.statement = EnterRule.BuildStatments(pContext, pToken.Tokens[1]);
           }
           else
           {
               //NonterminalToken T;
               //T = (NonterminalToken)pToken.Tokens[0];
               //this.statement = new Rule_Statement(pContext, T);
               this.statement = EnterRule.BuildStatments(pContext, pToken.Tokens[0]);
           }
       }

       /// <summary>
       /// connects the execution of Rule_Statments
       /// </summary>
       /// <returns>object</returns>
      public override object Execute()
      {
        object result = null;

        if (! this.statements.IsNull())
        {
          result = this.statements.Execute();
            
        }

        result = this.statement.Execute();

        return result;
       }
    }
}
