using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace EpiMenu.CommandPlugin
{
   public class Rule_Statements: Rule
   {
        //<Statements> ::= <Statements> NewLine <Statement> | <Statement>
       private Rule_Statement statement = null;
       private Rule_Statements statements = null;

       public Rule_Statements(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
       {
          //<Statements> ::= <Statements> NewLine <Statement> | <Statement>

           if (pToken.Tokens.Length > 1)
           {
               NonterminalToken T;
               T = (NonterminalToken)pToken.Tokens[0];
               this.statements = new Rule_Statements(pContext, T);

               T = ((NonterminalToken)pToken.Tokens[2]);
               this.statement = new Rule_Statement(pContext, T);
           }
           else
           {
               NonterminalToken T;
               T = (NonterminalToken)pToken.Tokens[0];
               this.statement = new Rule_Statement(pContext, T);
           }
       }

       /// <summary>
       /// connects the execution of Rule_Statments
       /// </summary>
       /// <returns>object</returns>
      public override object Execute()
      {
        object result = null;




        if (this.statements != null)
        {
          result = this.statements.Execute();
            
        }

        if (!this.Context.RunOneCommand)
        {
            result = this.statement.Execute();
        }
        else
        {
            if (!this.Context.OneCommandHasRun)
            {
                result = this.statement.Execute();
                this.Context.OneCommandHasRun = true;
            }
        }
        return result;
       }
    }
}
