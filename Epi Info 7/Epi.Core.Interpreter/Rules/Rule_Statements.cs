using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
   public partial class Rule_Statements: AnalysisRule 
   {
        //<Statements> ::= <Statements> <Statement> | <Statement>
       private AnalysisRule statement = null;
       private AnalysisRule statements = null;

       public Rule_Statements(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
       {
          //<Statements> ::= <Statements> <Statement> | <Statement>

           if (pToken.Tokens.Length > 1)
           {
               //NonterminalToken T;
               //T = (NonterminalToken)pToken.Tokens[0];
               //this.statements = new Rule_Statements(pContext, T);
               statements = AnalysisRule.BuildStatments(pContext, pToken.Tokens[0]);

               //T = ((NonterminalToken)pToken.Tokens[1]);
               //this.statement = new Rule_Statement(pContext, T);
               statement = AnalysisRule.BuildStatments(pContext, pToken.Tokens[1]);

           }
           else
           {
               //NonterminalToken T;
               //T = (NonterminalToken)pToken.Tokens[0];
               //this.statement = new Rule_Statement(pContext, T);
               statement = AnalysisRule.BuildStatments(pContext, pToken.Tokens[0]);
           }


       }

       /// <summary>
       /// connects the execution of Rule_Statments
       /// </summary>
       /// <returns>object</returns>
      public override object Execute()
      {
        object result = null;

        if (statements != null && !Context.isCancelRequest)
        {
            Context.NextRule = statement;
            result = statements.Execute();
        }

        if (!Context.RunOneCommand && !Context.isCancelRequest)
        {
            if (statement != null)
            result = statement.Execute();
        }
        else
        {
            if (Context.CurrentDataRow == null)
            {
                if (!Context.OneCommandHasRun && !Context.isCancelRequest)
                {
                    if (statement != null)
                    {
                        result = statement.Execute();
                        Context.OneCommandHasRun = true;
                    }
                }
            }
            else
            {
                if (!Context.isCancelRequest)
                {
                    if (statement != null)
                    result = statement.Execute();
                }
            }
        }

        return result;
       }
    }
}
