using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;
using EpiInfo.Plugin;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_Summarize : AnalysisRule
    {
        bool hasRun = false;

        List<string> aggregateList = new List<string>();
        Dictionary<string, string> aggregateElement = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string outTableName = null;
        List<string> participatingVariableList = new List<string>();
        List<string> strataVariableList = new List<string>();
        string weightVariable = null;
        string commandText = null;

        public Rule_Summarize(Rule_Context context, NonterminalToken token) 
            : base(context)
        {
            commandText = ExtractTokens(token.Tokens);
            SetAggretageList((NonterminalToken)token.Tokens[1]);
            outTableName = GetCommandElement(token.Tokens, 3);
            
            if (token.Tokens.Length > 4)
            {
                SetSummarizeOptList((NonterminalToken)token.Tokens[4]);
            }

            //  <Summarize_Statement>       ::= SUMMARIZE <AggregateList> TO Identifier <SummarizeOptList> | SUMMARIZE <AggregateList> TO Identifier
            //  <SummarizeOptList>          ::= <SummarizeOpt> | <SummarizeOptList> <SummarizeOpt>
            //  <SummarizeOpt>              ::=  STRATAVAR '=' <IdentifierList> |  WEIGHTVAR '=' Identifier
            //  <AggregateVariableElement>  ::= Identifier '::' <AggregateElement>
            //  <AggregateElement>          ::= <Aggregate>  '(' Identifier ')' | 'Count()'
            //  <Aggregate>                 ::= AVG | COUNT  | FIRST | LAST | MAX | MIN | STDEV | STDEVP | SUM | VAR | VARP
            //  <AggregateList>             ::= <AggregateVariableElement> | <AggregateList> <AggregateVariableElement> | <AggregateList> ',' <AggregateVariableElement>
        }

        public override object Execute()
        {
            object result = null;

            if (false == hasRun)
            {
                IAnalysisStatistic summarizePlugin = null;
                Dictionary<string, string> setProperties = Context.GetGlobalSettingProperties();
                Dictionary<string, string> inputVariableList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                StringBuilder stringBuilder = new StringBuilder();

                foreach (string aggregate in aggregateList)
                {
                    stringBuilder.Append(aggregate);
                    stringBuilder.Append(",");
                }

                stringBuilder.Length = stringBuilder.Length - 1;

                inputVariableList.Add("AggregateList", stringBuilder.ToString());
                stringBuilder.Length = 0;
                bool HasAggregateElements = false;
                
                foreach (KeyValuePair<string, string> aggregatePair in aggregateElement)
                {
                    stringBuilder.Append(aggregatePair.Key);
                    stringBuilder.Append("=");
                    stringBuilder.Append(aggregatePair.Value);
                    stringBuilder.Append(",");
                    HasAggregateElements = true;
                }
                
                if (HasAggregateElements)
                {
                    stringBuilder.Length = stringBuilder.Length - 1;
                }

                inputVariableList.Add("AggregateElementList", stringBuilder.ToString());

                if (strataVariableList.Count > 0)
                {
                    stringBuilder.Length = 0;

                    foreach (string strataVariable in strataVariableList)
                    {
                        stringBuilder.Append(strataVariable);
                        stringBuilder.Append(",");
                    }
                    
                    stringBuilder.Length = stringBuilder.Length - 1;
                    inputVariableList.Add("StratvarList", stringBuilder.ToString());
                }

                if (participatingVariableList.Count > 0)
                {
                    stringBuilder.Length = 0;

                    foreach (string participatingVariable in participatingVariableList)
                    {
                        stringBuilder.Append(participatingVariable);
                        stringBuilder.Append(",");
                    }

                    stringBuilder.Length = stringBuilder.Length - 1;
                    inputVariableList.Add("ParticipatingVariableList", stringBuilder.ToString());
                }

                inputVariableList.Add("OutTable", outTableName);
                inputVariableList.Add("WeightVar", weightVariable);
                inputVariableList.Add("commandText", commandText);

                if (Context.CurrentRead == null)
                {
                    inputVariableList.Add("TableName", "");
                }
                else
                {
                    inputVariableList.Add("TableName", Context.CurrentRead.Identifier);
                }

                EpiInfo.Plugin.IDataSource DataSource = Context.GetDefaultIDataSource();
                AnalysisStatisticExecuteHost statisticHost = new AnalysisStatisticExecuteHost(Context, setProperties, DataSource, inputVariableList, Context.CurrentSelect.ToString(), Context.AnalysisInterpreterHost);
                summarizePlugin = Context.GetStatistic("Summarize", statisticHost);
                summarizePlugin.Execute();
                summarizePlugin = null;

                hasRun = true;
            }
            return result;
        }

        private void SetAggretageList(NonterminalToken pToken)
        {
            switch (pToken.Symbol.ToString())
            {
                case "<AggregateVariableElement>":
                    SetAggregateElement(pToken);
                    break;
                case "<AggregateList>":
                    SetAggretageList((NonterminalToken)pToken.Tokens[0]);
                    if (pToken.Tokens.Length > 2)
                    {
                        SetAggregateElement((NonterminalToken)pToken.Tokens[2]);
                    }
                    else
                    {
                        SetAggregateElement((NonterminalToken)pToken.Tokens[1]);
                    }
                    break;
                case "<AggregateElement>":
                    SetAggregateElement(pToken);
                    break;
            }
        }

        private void SetAggregateElement(NonterminalToken pToken)
        {
            string Identifier1;
            string Identifier2;

            switch (pToken.Rule.Rhs[0].ToString())
            {
                case "Identifier":
                case "<Aggregate>":
                    Identifier1 = GetCommandElement(pToken.Tokens, 0);
                    aggregateList.Add(Identifier1);
                    NonterminalToken ParticipatingToken = (NonterminalToken)pToken.Tokens[2];
                    if (ParticipatingToken.Tokens.Length > 2)
                    {
                        participatingVariableList.Add(ParticipatingToken.Tokens[2].ToString().Trim());
                    }
                    Identifier2 = ExtractTokens(((NonterminalToken)pToken.Tokens[2]).Tokens).Trim();
                    if (Identifier2.Equals("Count()", StringComparison.OrdinalIgnoreCase))
                    {
                        aggregateElement.Add(Identifier1, "Count(" + Identifier1 + ")");
                    }
                    else
                    {
                        aggregateElement.Add(Identifier1, Identifier2);
                    }
                    break;
                default:
                    Identifier1 = GetCommandElement(pToken.Tokens, 0);
                    aggregateList.Add(Identifier1);
                    break;
            }

            //    <AggregateVariableElement>  ::= Identifier '::' <AggregateElement>
            //    <AggregateElement>          ::= <Aggregate>  '(' Identifier ')' |   'Count()'
        }

        private void SetSummarizeOptList(NonterminalToken pToken)
        {
            //<SummarizeOptList> ::= <SummarizeOpt> | <SummarizeOptList> <SummarizeOpt>
            switch (pToken.Symbol.ToString())
            {
                case "<SummarizeOptList>":
                    SetSummarizeOptList((NonterminalToken)pToken.Tokens[0]);
                    SetSummarizeOpt((NonterminalToken)pToken.Tokens[1]);
                    break;
                case "<SummarizeOpt>":
                    SetSummarizeOpt(pToken);
                    break;
            }
        }

        private void SetSummarizeOpt(NonterminalToken pToken)
        {
            //<SummarizeOpt> ::=  STRATAVAR '=' <IdentifierList> |  WEIGHTVAR '=' Identifier
            switch (((TerminalToken) pToken.Tokens[0]).Symbol.ToString())
            {
                case "STRATAVAR":
                    strataVariableList.AddRange(GetCommandElement(pToken.Tokens, 2).Split(' '));
                    break;
                case "WEIGHTVAR":
                    weightVariable = GetCommandElement(pToken.Tokens, 2);
                    break;
            }
        }
    }
}
