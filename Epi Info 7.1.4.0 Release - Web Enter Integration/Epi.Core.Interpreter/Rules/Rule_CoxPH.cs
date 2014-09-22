using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;
using EpiInfo.Plugin;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_CoxPH : AnalysisRule
    {

        bool HasRun = false;
        //<Simple_Cox_Statement> ::= 
        //Identifier '=' 
        //<CoxTermList> 
        //        '*' 
        //Identifier '(' DecLiteral ')' 
        //<CoxOptList>

        /*
<Cox_Time_Variable> ::= Identifier
<Cox_Censor_Variable> ::= Identifier  '(' <Literal>  ')'
                    | Identifier  '(' Boolean ')'

<CoxPH_Statement> ::= COXPH <Cox_Time_Variable> '=' <Cox_CovariateList>  '*'  <Cox_censor_variable>
                    | COXPH <Cox_Time_Variable> '=' <Cox_CovariateList>  '*'  <Cox_censor_variable> <CoxOptList>

<CoxOptList> ::= <CoxOpt> | <CoxOpt> <CoxOptList>

<CoxOpt> ::= WEIGHTVAR '=' Identifier
       | TIMEUNIT '=' String
       | GRAPHTYPE '=' String
       | OUTTABLE '=' Identifier
       | STRATAVAR '=' <IdentifierList>
       | GRAPH '=' <IdentifierList>
       | DIALOG
       | SHOWOBSERVED
       | PVALUE '=' Percentage
       | PVALUE '=' RealLiteral


<Cox_CovariateList> ::= <Cox_Covariate> | <Cox_Covariate> <Cox_CovariateList>
<Cox_Time_Function> ::= ':' <Expression> ':'
                    | ':' Identifier ':'
<Cox_Covariate> ::= Identifier
       | '(' Identifier ')'
       | Identifier <Cox_Time_Function> orSet
       | '(' Identifier ')' <Cox_Time_Function>
        */
        string commandText = null;

        string time_variable = null;
        List<string> DiscreteList = new List<string>();
        List<string> CovariateList = new List<string>();
        string time_function = null;
        string censor_variable = null;
        string censor_value = null;
        string time_unit = null;
        string out_table = null;
        string graph_type = null;
        string weightvar = null;
        string p_value = null;

        List<string> StrataVarList = new List<string>();
        List<string> GraphVariableList = new List<string>();
        
        public Rule_CoxPH(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            /*  
            <Simple_Cox_Statement> ::= COXPH Identifier '=' <CoxTermList> '*' Identifier '(' DecLiteral ')' <CoxOptList>
            <Boolean_Cox_Statement> ::= COXPH Identifier '=' <CoxTermList> '*' Identifier '(' Boolean ')' <CoxOptList>*/

            //!COXPH <time variable>= <covariate(s)>[: <time function>:]  *  <censor variable> (<value>) [TIMEUNIT="<time unit>"] [OUTTABLE=<tablename>] [GRAPHTYPE="<graph type>"] [WEIGHTVAR=<weight variable>] [STRATAVAR=<strata variable(s)>] [GRAPH=<graph variable(s)>]

            string[] saCensorArray;

            this.commandText = this.ExtractTokens(pToken.Tokens);
            this.time_variable = this.GetCommandElement(pToken.Tokens, 1).Trim(new char[] {'[',']'});
            
            saCensorArray = GetUncensoredVarVal(this.GetCommandElement(pToken.Tokens, 5));
            this.censor_variable = saCensorArray[0].Trim(new char[] { '[', ']' });
            this.censor_value = saCensorArray[1].Trim(new char[] { '[', ']' });

            this.SetTermList((NonterminalToken) pToken.Tokens[3]);

            if (pToken.Tokens.Length > 6)
            {
                this.SetOptionList((NonterminalToken)pToken.Tokens[6]);
            }
        }

        //  den4 4/7/2011, for extracting the value for uncensored from the censor_variable
        /// <summary>
        /// Developer can extract the the value for uncensored from the censor_variable without parsing tokens
        /// </summary>
        /// <param name="censored_input">tokens parameter from the parser</param>
        /// <returns>Returns the contents of the value for uncensored as a string</returns>
        protected string[] GetUncensoredVarVal(string censored_input)
        {
            Int32 iOpenParen;
            Int32 iCloseParen;
            Int32 iCensorLen;
            string[] strUncensoredVarVal = new string[2];
            char[] charsToTrim = { '"', ' ', '\'' };

            iOpenParen = censored_input.IndexOf("(");
            if (iOpenParen > 0)
            {
                //extract the variable
                strUncensoredVarVal[0] = censored_input.Substring(0, iOpenParen).Trim();
                iCloseParen = censored_input.IndexOf(")");
                iCensorLen = iCloseParen - iOpenParen;
                //extract the value enclosed in parentheses
                strUncensoredVarVal[1] = censored_input.Substring(iOpenParen + 1, iCensorLen - 1).Trim(charsToTrim);
            }
            else
            {
                strUncensoredVarVal[0] = censored_input;
                strUncensoredVarVal[1] = censored_input;
            }
            return strUncensoredVarVal;
        }


        private void SetTermList(NonterminalToken pToken)
        {
            //<CoxTermList> ::= <CoxTerm> | <CoxTerm> <CoxTermList>
            if (pToken.Symbol.ToString() == "<Cox_Covariate>")
            {
                this.SetTerm(pToken);
            }
            else if (pToken.Symbol.ToString() == "<Cox_CovariateList>")
            {
                    this.SetTerm((NonterminalToken)pToken.Tokens[0]);
                    this.SetTermList((NonterminalToken)pToken.Tokens[1]);
            }
            else
            {
                foreach (Token T in pToken.Tokens)
                {
                    if (T is NonterminalToken)
                    {
                        NonterminalToken NT = (NonterminalToken)T;
                        switch (NT.Symbol.ToString())
                        {
                            case "<CoxTerm>":
                            case "<Cox_Covariate>":
                                this.SetTerm(NT);
                                break;
                            case "<CoxTermList>":
                                this.SetTermList(NT);
                                break;
                        }
                    }

                }
            }
        }

        private void SetTerm(NonterminalToken pToken)
        {
            /*
            <CoxTerm> ::= Identifier
                   | '(' Identifier ')'
                   | Identifier ':' <Expression>
                   | '(' Identifier ')' ':' <Expression>
                   | Identifier ':'  Identifier ':'
                   | '(' Identifier ')' ':' Identifier ':'
            */
            string value = null;
            switch (pToken.Rule.Rhs[0].ToString())
            {
                case "Identifier":
                    //"Identifier":
                    //"Identifier ':' <Expression>":
                    //"Identifier ':'  Identifier ':'":
                    value = this.GetCommandElement(pToken.Tokens, 0).Trim(new char[] { '[', ']' });
                    if (pToken.Tokens.Length > 2)
                    {
                        value += ":" + this.GetCommandElement(pToken.Tokens, 2).Trim('"');
                    }
                    this.CovariateList.Add(value);
                    break;

                case "(":
                    //"(' Identifier ')":
                    //"'(' Identifier ')' ':' Identifier ':'":
                    value = this.GetCommandElement(pToken.Tokens, 1);
                    if (pToken.Tokens.Length > 3)
                    {
                        value += ":" + this.GetCommandElement(pToken.Tokens, 4).Trim(new char[] { '[', ']' });
                    }
                    this.DiscreteList.Add(value);
                    this.CovariateList.Add(value);
                    break;
                default:
                    // do nothing
                    break;
            }
        }

        private void SetOptionList(NonterminalToken pToken)
        {
            //<CoxOptList> ::= <CoxOpt> | <CoxOpt> <CoxOptList>
            switch (pToken.Symbol.ToString())
            {
                case "<CoxOpt>":
                    this.SetOption(pToken);
                    break;
                case "<CoxOptList>":
                    foreach (Token T in pToken.Tokens)
                    {
                        if (T is NonterminalToken)
                        {
                            NonterminalToken NT = (NonterminalToken)T;
                            switch (NT.Symbol.ToString())
                            {
                                case "<CoxOpt>":
                                    this.SetOption(NT);
                                    break;
                                case "<CoxOptList>":
                                    this.SetOptionList(NT);
                                    break;
                            }

                        }

                    }
                    break;
            }
        }

        private void SetOption(NonterminalToken pToken)
        {
            /*
            <CoxOpt> ::= WEIGHTVAR '=' Identifier
                   | TIMEUNIT '=' String
                   | GRAPHTYPE '=' String
                   | OUTTABLE '=' Identifier
                   | STRATAVAR '=' <IdentifierList>
                   | GRAPH '=' <IdentifierList>
                   | DIALOG
                   | SHOWOBSERVED
                   | PVALUE '=' Percentage
                   | PVALUE '=' RealLiteral
            */
            switch (pToken.Rule.Rhs[0].ToString())
            {
                case "WEIGHTVAR":
                    this.weightvar = this.GetCommandElement(pToken.Tokens, 2).Trim('"').Trim(new char[] { '[', ']' });
                    break;
                case "TIMEUNIT":
                    this.time_unit = this.GetCommandElement(pToken.Tokens, 2).Trim('"').Trim(new char[] { '[', ']' });
                    break;
                case "GRAPHTYPE":
                    this.graph_type = this.GetCommandElement(pToken.Tokens, 2).Trim('"').Trim(new char[] { '[', ']' });
                    break;
                case "OUTTABLE":
                    this.out_table = this.GetCommandElement(pToken.Tokens, 2).Trim('"').Trim(new char[] { '[', ']' });
                    break;
                case "STRATAVAR":
                    this.StrataVarList.AddRange(AnalysisRule.SpliIdentifierList(this.GetCommandElement(pToken.Tokens, 2))); 
                    break;
                case "GRAPH":
                    this.GraphVariableList.AddRange(AnalysisRule.SpliIdentifierList(this.GetCommandElement(pToken.Tokens, 2)));
                    break;
                case "DIALOG":
                case "SHOWOBSERVED":
                case "PVALUE":
                    this.p_value = this.GetCommandElement(pToken.Tokens, 2).Trim(new char[] { '[', ']' });
                    break;
                default:
                    // do nothing
                    break;
            }
        }

        /// <summary>
        /// performs an addition / subtraction or pass thru rule
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            if (!this.HasRun)
            {
                // prepare IAnalysisStatisticContext information to be sent to the 
                // CoxPH
                IAnalysisStatistic CoxPH = null;

                // connection string
                // 1 term / column name
                // 1 dependent variable /column name
                // tablname 

                Dictionary<string, string> setProperties = this.Context.GetGlobalSettingProperties();
                if (this.Context.CurrentRead == null)
                {
                    setProperties.Add("TableName", "");
                }
                else
                {
                    setProperties.Add("TableName", this.Context.CurrentRead.Identifier);
                }
                setProperties.Add("CommandText", this.commandText);
                Dictionary<string, string> inputVariableList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                inputVariableList.Add("time_variable", time_variable);
                StringBuilder sb = new StringBuilder();
                foreach (string s in this.CovariateList)
                {
                    sb.Append(s);
                    sb.Append(",");
                }
                if (sb.Length > 0)
                {
                    sb.Length = sb.Length - 1;
                }
                inputVariableList.Add("CovariateList", sb.ToString());
                sb.Length = 0;
                foreach (string s in this.DiscreteList)
                {
                    sb.Append(s);
                    sb.Append(",");
                }
                if (this.DiscreteList.Count > 0)
                {
                    sb.Length = sb.Length - 1;
                }

                inputVariableList.Add("DiscreteList", sb.ToString());
                inputVariableList.Add("time_function", time_function);
                inputVariableList.Add("censor_variable", censor_variable);
                inputVariableList.Add("censor_value", censor_value);
                inputVariableList.Add("time_unit", time_unit);

                inputVariableList.Add("out_table", out_table);
                inputVariableList.Add("graph_type", graph_type);
                inputVariableList.Add("weightvar", weightvar);

                sb.Length = 0;

                foreach (string s in StrataVarList)
                {
                    sb.Append(s);
                    sb.Append(",");
                }
                if (sb.Length > 0)
                {
                    sb.Length = sb.Length - 1;
                }
                inputVariableList.Add("StrataVarList", sb.ToString());

                sb.Length = 0;
                foreach (string s in GraphVariableList)
                {
                    sb.Append(s);
                    sb.Append(",");
                }
                if (sb.Length > 0)
                {
                    sb.Length = sb.Length - 1;
                }
                inputVariableList.Add("GraphVariableList", sb.ToString());

                IDataSource DataSource = this.Context.GetDefaultIDataSource();

                AnalysisStatisticExecuteHost statisticHost = new AnalysisStatisticExecuteHost(this.Context, setProperties, DataSource, inputVariableList, this.Context.CurrentSelect.ToString(), this.Context.AnalysisInterpreterHost);

                CoxPH = this.Context.GetStatistic("CoxProportionalHazards", statisticHost);
                CoxPH.Execute();
                this.HasRun = true;
            }
            
            return result;
        }
    }
}

