using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using com.calitha.goldparser;

using Epi.Data;
using Epi.Web;


namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_Match : AnalysisRule 
    {
        bool HasRun = false;

		string[] IdentifierList = null;
		string[] IdentifierList2 = null;
		Dictionary<string, string> inputVariableList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		string Exposure = null;
		string Outcome = null;
		List<string> MatchVarList = null;
		string OutTable = null;
		string Stratvar = null;
		string WeightVar = null;
		string PSUVar = null;
		//bool isExceptionList = false;
		string[] FrequencyOptions = null;
		
		string parameter1 = null;
		string parameter2 = null;
		string parameter3 = null;
		string parameter4 = null;
		//DataTable dt = null;

        bool IsExceptionList1 = false;
        bool IsExceptionList2 = false;


        string commandText = string.Empty;

        public Rule_Match(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            commandText = this.ExtractTokens(pToken.Tokens);

            /*!***            Match Statement             ***!
            <Match_Row_All_Statement> ::= MATCH '*' Identifier <MatchOpts>
            <Match_Row_Except_Statement> ::= MATCH '*' EXCEPT <IdentifierList> Identifier <MatchOpts>
            <Match_Column_All_Statement> ::= MATCH Identifier '*' <MatchOpts>
            <Match_Column_Except_Statement> ::= MATCH Identifier '*' EXCEPT <IdentifierList> <MatchOpts>
            <Match_Row_Column_Statement> ::= MATCH Identifier Identifier <MatchOpts>

            <MatchOpts> ::= <MatchOpts> <MatchOpt>| <MatchOpt> 

            <MatchOpt> ::= WEIGHTVAR '=' Identifier
                      | MATCHVAR '=' <IdentifierList>
                      | <SetClause>
             */
        }

        /// <summary>
        /// performs execution of the Match command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            if (!this.HasRun)
            {
                //Configuration config = Configuration.GetNewInstance();
                Dictionary<string, string> setProperties = this.Context.GetGlobalSettingProperties();
                if (Stratvar != null)
                {
                    inputVariableList.Add("Stratavar", Stratvar);
                }

                if (!string.IsNullOrEmpty(OutTable))
                {
                    inputVariableList.Add("OutTable", OutTable);
                }

                if (!string.IsNullOrEmpty(WeightVar))
                {
                    inputVariableList.Add("WeightVar", WeightVar);
                }
                inputVariableList.Add("commandText", commandText);
                if (!string.IsNullOrEmpty(PSUVar))
                {
                    inputVariableList.Add("PSUVar", PSUVar);
                }
                if (this.Context.CurrentRead == null)
                {
                    inputVariableList.Add("TableName", "");
                }
                else
                {
                    inputVariableList.Add("TableName", this.Context.CurrentRead.Identifier);
                }

                EpiInfo.Plugin.IDataSource DataSource = this.Context.GetDefaultIDataSource();
                AnalysisStatisticExecuteHost statisticHost = new AnalysisStatisticExecuteHost(this.Context, setProperties, DataSource, inputVariableList, this.Context.CurrentSelect.ToString(), this.Context.AnalysisInterpreterHost);

                string resultcount = "<p>Match command NOT yet implemented</p>";
                Dictionary<string, string> args = new Dictionary<string, string>();
                args.Add("COMMANDNAME", CommandNames.DELETE);
                args.Add("COMMANDTEXT", this.commandText.Trim());
                args.Add("TABLENAME", this.Context.CurrentRead.Identifier);
                args.Add("HTMLRESULTS", resultcount);
                this.Context.AnalysisCheckCodeInterface.Display(args);
                this.HasRun = true;
            }
            return result;
        }
    }
}
