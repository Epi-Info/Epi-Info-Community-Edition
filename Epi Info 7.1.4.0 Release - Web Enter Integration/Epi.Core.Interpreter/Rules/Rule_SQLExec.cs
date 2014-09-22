using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_SQLExec : AnalysisRule
    {
        bool HasRun = false;

        string Identifier = null;
        string SQL = null;

        public Rule_SQLExec(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            //<SQL_Execute_Command> ::= SQLExecute BracketString 
            // example SQLExec [Select * from tablename]

            if (pToken.Tokens.Length == 4)
            {
                ///Variable.SQLExec BraceString
                this.Identifier = pToken.Tokens[0].ToString();
                this.SQL = this.GetCommandElement(pToken.Tokens, 3).Trim(new char[] { '{', '}' });
            }
            else
            {
                //SQLExec BraceString
                this.Identifier = "_DB";
                this.SQL = this.GetCommandElement(pToken.Tokens, 1).Trim(new char[] { '{', '}' });
            }
        }


        public override object Execute()
        {

            if (!this.HasRun)
            {
                return this.Context.ConnectionList[this.Identifier].ExecuteSQL(this.SQL);
                this.HasRun = true;
            }

            return null;


        }
    }
}
