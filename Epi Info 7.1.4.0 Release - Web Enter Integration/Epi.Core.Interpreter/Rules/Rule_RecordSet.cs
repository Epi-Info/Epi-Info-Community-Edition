using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_RecordSet : AnalysisRule
    {
        string Identifier = null;
        string SQL = null;
        public Rule_RecordSet(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            //<RecordSet_Command> ::= RecordSet BracketString
            //// example RecordSet [Select * from table]
            
            if (pToken.Tokens.Length == 4)
            {
                ///Variable.RecordSet BraceString
                this.Identifier = pToken.Tokens[0].ToString();
                this.SQL = this.GetCommandElement(pToken.Tokens, 3).Trim(new char[] { '{', '}' });
            }
            else
            {
                //RecordSet BraceString
                this.Identifier = "_DB";
                this.SQL = this.GetCommandElement(pToken.Tokens, 1).Trim(new char[] { '{', '}' });
            }
        }

        public override object Execute()
        {
            this.Context.isReadMode = false;
            this.Context.CurrentSelect.Length = 0;
            this.Context.CurrentSelect.Append(this.SQL);


            
            this.Context.DataTableRefreshNeeded = true;
            if (this.Context.CurrentRead != null)
            {
                this.Context.CurrentRead.IsEpi7ProjectRead = false;
            }

            this.Context.SyncVariableAndOutputTable();

            List<System.Data.DataRow> recordCount = this.Context.GetOutput();

            //string result = string.Format("number of records read {0}", recordCount.Count);
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("COMMANDNAME", "RecordSet");
            args.Add("SQL", this.SQL);
            args.Add("ROWCOUNT", recordCount.Count.ToString());


            this.Context.AnalysisCheckCodeInterface.Display(args);

            return null;
        }
    }
}
