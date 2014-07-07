using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using com.calitha.goldparser;
using Epi.Data;
using Epi.Web;
using Epi.Analysis.Statistics;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_Graph : AnalysisRule
    {
        bool HasRun = false;

        Dictionary<string, string> inputVariableList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        string _commandText = string.Empty;
        string _strataVar = string.Empty;
        string _independentVariableList = string.Empty;
        List<string> _optionList = new List<string>();
        string _graphType = string.Empty;
        string _graphCrossTab = string.Empty;
        string _graphTitle = string.Empty;
        string _graphXAxisLabel = string.Empty;
        string _graphYAxisLabel = string.Empty;
        string _weightVar = string.Empty;
        string _aggregateFunction = string.Empty;
        string _graphDateFormat = string.Empty;
        string _graphInterval = string.Empty;
        string _graphIntervalUnits = string.Empty;
        string _graphStartFrom = string.Empty;

        EpiInfo.Plugin.IAnalysisStatistic GraphStatistic;

        public Rule_Graph(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            _commandText = this.ExtractTokens(pToken.Tokens);

            string caseSymbol = string.Empty;

            foreach (Token token in pToken.Tokens)
            {
                if (token is NonterminalToken)
                {
                    NonterminalToken nonterminalToken = (NonterminalToken)token;

                    caseSymbol = nonterminalToken.Symbol.ToString();

                    switch (caseSymbol)
                    {
                        case "<Graph_Type>":
                            _graphType = nonterminalToken.Tokens[0].ToString();
                            break;
                        case "<Graph_Interval>":
                            string intervalText = (((NonterminalToken)nonterminalToken).Tokens[2]).ToString();
                            intervalText = intervalText.Trim(new char[] { '"' });
                            string[] intervalArray = intervalText.Split(' ');
                            _graphInterval = intervalArray[0];
                            _graphIntervalUnits = intervalArray[1];
                            break;
                        case "<Graph_Variable>":
                            _independentVariableList = nonterminalToken.Tokens[0].ToString().Trim(new char[] {'[',']'});
                            break;
                        case "<Graph_Variable_List>":
                            this.SetIdentifierList(nonterminalToken);
                            break;
                        case "<Graph_CrossTab>":
                            _graphCrossTab = nonterminalToken.Tokens[0].ToString().Trim(new char[] { '[', ']' });
                            break;
                        case "<Graph_Option_List>":
                            this.SetOptionList(nonterminalToken);
                            break;
                        case "<Graph_Title>":
                            _graphTitle = this.GetCommandElement(nonterminalToken.Tokens, 2);
                            break;
                        case "<Graph_XAxisLabel>":
                            _graphXAxisLabel = this.GetCommandElement(nonterminalToken.Tokens, 2);
                            break;
                        case "<Graph_YAxisLabel>":
                            _graphYAxisLabel = this.GetCommandElement(nonterminalToken.Tokens, 2);
                            break;
                        case "<Graph_StrataVar>":
                            _strataVar = (((NonterminalToken)nonterminalToken).Tokens[2]).ToString();
                            break;
                        case "<Graph_DateFormat>":
                            _graphDateFormat = (((NonterminalToken)nonterminalToken).Tokens[2]).ToString();
                            break;
                        case "<Graph_WeightVar>":
                            object tokenObject = (((NonterminalToken)nonterminalToken).Tokens[2]);

                            if (tokenObject is TerminalToken)
                            {
                                _weightVar = ((TerminalToken)tokenObject).Text.Trim(new char[] { '[', ']' });
                            }
                            else
                            {
                                _aggregateFunction = this.GetCommandElement(((NonterminalToken)tokenObject).Tokens, 0);

                                Token tok = null;

                                if (((NonterminalToken)tokenObject).Tokens.Length == 1)
                                {
                                    tok = ((NonterminalToken)tokenObject).Tokens[0];

                                    if (_aggregateFunction.ToUpper() == "PERCENT()")
                                    {
                                        _aggregateFunction = _aggregateFunction.TrimEnd(new char[] { '(', ')' });
                                        _weightVar = _independentVariableList.Trim(new char[] { ',' });
                                        break;
                                    }
                                }
                                else
                                {
                                    tok = ((NonterminalToken)tokenObject).Tokens[2];
                                    tok = ((NonterminalToken)(tok)).Tokens[0];
                                }
                                
                                _weightVar = ((TerminalToken)(tok)).Text;
                                _weightVar = _weightVar.Trim(new char[] { '[', ']' });

                            }
                            break;
                    }
                }
                else
                {
                    TerminalToken terminalToken = (TerminalToken)token;
                    switch (terminalToken.Symbol.ToString())
                    {
                        case "Identifier":
                            _independentVariableList = terminalToken.ToString().Trim(new char[] { '[', ']' });
                            break;
                    }
                }
            }
        }

        private void SetIdentifierList(NonterminalToken nonTerm)
        {
            _independentVariableList = _independentVariableList + "," + this.GetCommandElement(nonTerm.Tokens, 0).ToUpper().Trim(new char[] { '[', ']' });
            if (nonTerm.Tokens.Length > 1)
            {
                this.SetIdentifierList((NonterminalToken)nonTerm.Tokens[1]);
            }
        }

        private void SetOptionList(NonterminalToken nonTerm)
        {
            this._optionList.Add(this.GetCommandElement(nonTerm.Tokens, 0));
            
            if (nonTerm.Tokens.Length > 0)
            {
                foreach (Token optionToken in nonTerm.Tokens)
                {
                    if (optionToken is NonterminalToken)
                    {
                        NonterminalToken nonterminalOptionToken = (NonterminalToken) optionToken;

                        string symbol = nonterminalOptionToken.Symbol.ToString();
                        switch (symbol)
                        {
                            case "<Graph_Option>":
                                break;
                            case "<Graph_Option_List>":
                                this.SetOptionList(nonterminalOptionToken);
                                break;
                            case "<Graph_Title>":
                                _graphTitle = this.GetCommandElement(nonterminalOptionToken.Tokens, 2);
                                break;
                            case "<Graph_XAxisLabel>":
                                _graphXAxisLabel = this.GetCommandElement(nonterminalOptionToken.Tokens, 2);
                                break;
                            case "<Graph_YAxisLabel>":
                                _graphYAxisLabel = this.GetCommandElement(nonterminalOptionToken.Tokens, 2);
                                break;
                            case "<Graph_StrataVar>":
                                _strataVar = (((NonterminalToken)nonterminalOptionToken).Tokens[2]).ToString();
                                break;
                            case "<Graph_DateFormat>":
                                _graphDateFormat = (((NonterminalToken)nonterminalOptionToken).Tokens[2]).ToString();
                                break;
                            case "<Graph_WeightVar>":
                                object tokenObject = (((NonterminalToken)nonterminalOptionToken).Tokens[2]);

                                if (tokenObject is TerminalToken)
                                {
                                    _weightVar = ((TerminalToken)tokenObject).Text.Trim(new char[] { '[', ']' });
                                }
                                else
                                {
                                    if (((NonterminalToken)tokenObject).Tokens.Length >= 3)
                                    {
                                        Token ntt = (NonterminalToken)((NonterminalToken)tokenObject).Tokens[2];
                                        string weightTok = ((com.calitha.goldparser.TerminalToken)(((com.calitha.goldparser.NonterminalToken)(ntt)).Tokens[0])).Text;
                                        _weightVar = weightTok.Trim(new char[] { '[', ']' });
                                        _aggregateFunction = this.GetCommandElement(((NonterminalToken)tokenObject).Tokens, 0);
                                    }
                                    else
                                    {
                                        _weightVar = this.GetCommandElement(((NonterminalToken)tokenObject).Tokens, 0);
                                    }
                                }
                                break;

                            case "<Graph_Interval>":
                                string intervalText = (((NonterminalToken)nonterminalOptionToken).Tokens[2]).ToString();
                                intervalText = intervalText.Trim(new char[] { '"' });
                                string[] intervalArray = intervalText.Split(' ');

                                _graphInterval = intervalArray[0];
                                _graphIntervalUnits = intervalArray[1];

                                break;
                            case "<Graph_StartFrom>":
                                _graphStartFrom = (((NonterminalToken)nonterminalOptionToken).Tokens[2]).ToString();
                                _graphStartFrom = _graphStartFrom.Trim(new char[] { '"' });
                                break;
                        }
                    }
                }
            }
        }

        public override object Execute()
        {
            object result = null;

            if (!this.HasRun)
            {

                Dictionary<string, string> setProperties = this.Context.GetGlobalSettingProperties();

                if (!string.IsNullOrEmpty(_commandText)) inputVariableList.Add("Command_Text", _commandText);
                if (!string.IsNullOrEmpty(_independentVariableList)) inputVariableList.Add("Independent_Variable_List", _independentVariableList.Trim(','));
                if (!string.IsNullOrEmpty(_graphCrossTab)) inputVariableList.Add("Cross_Tabulation_Variable", _graphCrossTab);
                if (!string.IsNullOrEmpty(_strataVar)) inputVariableList.Add("StrataVar", _strataVar);
                if (!string.IsNullOrEmpty(_graphType)) inputVariableList.Add("Graph_Type", _graphType);
                if (!string.IsNullOrEmpty(_graphTitle)) inputVariableList.Add("Graph_Title", _graphTitle);
                if (!string.IsNullOrEmpty(_graphXAxisLabel)) inputVariableList.Add("Graph_Independent_Value_Axis_Label", _graphXAxisLabel);
                if (!string.IsNullOrEmpty(_graphYAxisLabel)) inputVariableList.Add("Graph_Dependent_Value_Axis_Label", _graphYAxisLabel);
                if (!string.IsNullOrEmpty(_weightVar)) inputVariableList.Add("Weight_Variable", _weightVar);
                if (!string.IsNullOrEmpty(_aggregateFunction)) inputVariableList.Add("Aggregate_Function", _aggregateFunction);
                if (!string.IsNullOrEmpty(_graphDateFormat)) inputVariableList.Add("Date_Format", _graphDateFormat);

                if (!string.IsNullOrEmpty(_graphInterval)) inputVariableList.Add("Interval", _graphInterval);
                if (!string.IsNullOrEmpty(_graphIntervalUnits)) inputVariableList.Add("Interval_Units", _graphIntervalUnits);
                if (!string.IsNullOrEmpty(_graphStartFrom)) inputVariableList.Add("Start_From", _graphStartFrom);

                EpiInfo.Plugin.IDataSource DataSource = this.Context.GetDefaultIDataSource();

                AnalysisStatisticExecuteHost statisticHost = new AnalysisStatisticExecuteHost(this.Context, setProperties, DataSource, inputVariableList, this.Context.CurrentSelect.ToString(), this.Context.AnalysisInterpreterHost);

                this.GraphStatistic = this.Context.GetStatistic("Graph", statisticHost);
                this.GraphStatistic.Execute();
                this.GraphStatistic = null;
                this.HasRun = true;
            }
            return result;
        }

        private System.Data.DataTable BuildTempTableFromContext()
        {
            System.Data.DataTable tempTable = new DataTable();

            foreach (DataColumn column in this.Context.DataSet.Tables["output"].Columns)
            {
                DataColumn newColumn = new DataColumn(column.ColumnName);
                newColumn.DataType = column.DataType;
                tempTable.Columns.Add(newColumn);
            }

            foreach (DataRow row in this.Context.GetOutput())
            {
                tempTable.ImportRow(row);
            }
            return tempTable;
        }

        private static void AggregateCount(List<DataRow> rows, string seriesName, Dictionary<object, double> indDep_PreSORT_Values)
        {
            foreach (DataRow row in rows)
            {
                object item = row[seriesName];

                if (item != System.DBNull.Value)
                {
                    if (indDep_PreSORT_Values.ContainsKey(item))
                    {
                        indDep_PreSORT_Values[item]++;
                    }
                    else
                    {
                        indDep_PreSORT_Values.Add(item, 1);
                    }
                }
            }
        }
    }
}
