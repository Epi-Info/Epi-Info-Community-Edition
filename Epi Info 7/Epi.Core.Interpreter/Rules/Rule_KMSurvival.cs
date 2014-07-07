using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;
using EpiInfo.Plugin;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_KMSurvival : AnalysisRule
    {
        bool HasRun = false;
        //<Simple_KM_Statement> ::= 
        //Identifier '=' 
        //<KMTermList> 
        //        '*' 
        //Identifier '(' DecLiteral ')' 
        //<KMOptList>
        /*
<KM_Time_Variable> ::= Identifier
<KM_Group_Variable> ::= Identifier
<KM_Censor_Variable> ::= Identifier

<KM_Survival_Statement> ::= KMSURVIVAL <KM_Time_Variable> '=' <KM_Group_Variable> '*' <KM_Censor_Variable> '(' <Literal>  ')' 
                            |KMSURVIVAL <KM_Time_Variable> '=' <KM_Group_Variable> '*' <KM_Censor_Variable> '(' Boolean  ')' 
                            | KMSURVIVAL <KM_Time_Variable> '=' <KM_Group_Variable> '*' <KM_Censor_Variable> '(' <Literal>  ')' <KMSurvOpts>
                            |KMSURVIVAL <KM_Time_Variable> '=' <KM_Group_Variable> '*' <KM_Censor_Variable> '(' Boolean  ')' <KMSurvOpts>

<KMSurvOpts>                            ::= <KMSurvOpt> | <KMSurvOpt> <KMSurvOpts>

<KMSurvOpt>                             ::= <KMSurvTimeOpt>
                                | <KMSurvGraphOpt>
                                | <KMSurvOutOpt>
                                | <KMSurvWeightOpt>
                                | <KMSurvTitleOpt>
<KMSurvTimeOpt>                     ::= TIMEUNIT '=' String
<KMSurvGraphOpt>                        ::= GRAPHTYPE '=' String
<KMSurvOutOpt>                      ::= OUTTABLE '=' Identifier
<KMSurvWeightOpt>                       ::= WEIGHTVAR '=' Identifier
<KMSurvTitleOpt>                        ::= TITLETEXT '=' String
        */
        string commandText = null;

        string time_variable = null;
        string group_variable = null;
        string censor_variable = null;
        string uncensored_value = null;
        string time_unit = null;

        string out_table = null;
        string graph_type = null;
        string weight_variable = null;

        public Rule_KMSurvival(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            /*  
            !***            Kaplan-Meier Survival Statement ***!
            <Simple_KM_Survival_Statement>     ::= KMSURVIVAL Identifier '=' Identifier '*' Identifier '(' Literal ')' <KMSurvOpts>
            <KM_Survival_Boolean_Statement>    ::= KMSURVIVAL Identifier '=' Identifier '*' Identifier '(' Boolean ')' <KMSurvOpts>
             */

            this.commandText = this.ExtractTokens(pToken.Tokens);
            this.time_variable = this.GetCommandElement(pToken.Tokens, 1).TrimStart('[').TrimEnd(']');
            this.group_variable = this.GetCommandElement(pToken.Tokens, 3).TrimStart('[').TrimEnd(']');

            this.censor_variable = this.GetCommandElement(pToken.Tokens, 5).TrimStart('[').TrimEnd(']');
            this.uncensored_value = this.GetCommandElement(pToken.Tokens, 7);

            if (pToken.Tokens.Length > 9)
            {
                this.SetOptionList((NonterminalToken)pToken.Tokens[9]);
            }
        }

        private void SetTermList(NonterminalToken pToken)
        {
            //<KMTermList> ::= <KMTerm> | <KMTerm> <KMTermList>

            this.SetTerm((NonterminalToken)pToken.Tokens[0]);

            if (pToken.Tokens.Length > 1)
            {
                this.SetTermList((NonterminalToken)pToken.Tokens[1]);
            }

        }
        private void SetTerm(NonterminalToken pToken)
        {
            /*
            <KMTerm> ::= Identifier
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
                    value = this.GetCommandElement(pToken.Tokens, 0);
                    break;

                case "(":
                    //"(' Identifier ')":
                    //"'(' Identifier ')' ':' Identifier ':'":
                    value = this.GetCommandElement(pToken.Tokens, 1);
                    break;
                default:
                    // do nothing
                    break;
            }
        }



        private void SetOptionList(NonterminalToken pToken)
        {
            //<KMSurvOpts>  ::= <KMSurvOpt> | <KMSurvOpt> <KMSurvOpts>
            switch (pToken.Symbol.ToString())
            {
                case "<KMSurvOpt>":
                    this.SetOptionList(pToken);
                    break;
                case "<KMSurvOpts>":
                    foreach (Token T in pToken.Tokens)
                    {
                        if (T is NonterminalToken)
                        {
                            NonterminalToken NT = (NonterminalToken)T;
                            switch (NT.Symbol.ToString())
                            {
                                case "<KMSurvOpt>":
                                    this.SetOptionList(NT);
                                    break;
                                case "<KMSurvOpts>":
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
            <KMSurvOpt>                             ::= <KMSurvTimeOpt>
            | <KMSurvGraphOpt>
            | <KMSurvOutOpt>
            | <KMSurvWeightOpt>
            | <KMSurvTitleOpt>

             <KMSurvTimeOpt>   ::= TIMEUNIT '=' String
             <KMSurvGraphOpt>  ::= GRAPHTYPE '=' String
             <KMSurvOutOpt>    ::= OUTTABLE '=' Identifier
             <KMSurvWeightOpt> ::= WEIGHTVAR '=' Identifier
             <KMSurvTitleOpt>  ::= TITLETEXT '=' String
            */
            switch (pToken.Rule.Rhs[0].ToString())
            {
                case "<KMSurvWeightOpt>":
                    this.weight_variable = this.GetCommandElement(pToken.Tokens, 2).Trim('"');
                    break;
                case "<KMSurvTimeOpt>":
                    this.time_unit = this.GetCommandElement(pToken.Tokens, 2).Trim('"');
                    break;
                case "<KMSurvGraphOpt>":
                    this.graph_type = this.GetCommandElement(pToken.Tokens, 2).Trim('"');
                    break;
                case "<KMSurvOutOpt>":
                    this.out_table = this.GetCommandElement(pToken.Tokens, 2).Trim('"');
                    break;
                case "DIALOG":
                case "SHOWOBSERVED":
                case "PVALUE":
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
                // KMSurvival
                IAnalysisStatistic KMSurvival = null;

                // connection string
                // 1 term / column name
                // 1 dependent variable /column name
                // tablname 

                Dictionary<string, string> setProperties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
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

                setProperties.Add("BLabels", "Yes;No;Missing"); // TODO: Replace Yes, No, Missing with global vars

                //ToDo: Move calls to inputVariableList.Add up into applicable function above: Rule_KMSurv or SetOption
                //
                inputVariableList.Add("time_variable", time_variable);
                inputVariableList.Add("group_variable", group_variable);

                //inputVariableList.Add("time_function", time_function);
                inputVariableList.Add("censor_variable", censor_variable);
                inputVariableList.Add("uncensored_value", uncensored_value);
                inputVariableList.Add("time_unit", time_unit);

                inputVariableList.Add("out_table", out_table);
                inputVariableList.Add("graph_type", graph_type);
                inputVariableList.Add("weight_variable", weight_variable);


                IDataSource DataSource = this.Context.GetDefaultIDataSource();

                AnalysisStatisticExecuteHost statisticHost = new AnalysisStatisticExecuteHost(this.Context, setProperties, DataSource, inputVariableList, this.Context.CurrentSelect.ToString(), this.Context.AnalysisInterpreterHost);

                KMSurvival = this.Context.GetStatistic("KaplanMeierSurvival", statisticHost);
                KMSurvival.Execute();
                this.HasRun = true;
            }

            return result;
        }


    }

    public class Rule_KMTermList : AnalysisRule
    {
        public Rule_KMTermList(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            /*
             <KMTermList> ::= <KMTerm> | <KMTerm> <KMTermList>

            <KMTerm> ::= Identifier
                   | '(' Identifier ')'
                   | Identifier ':' <Expression>
                   | '(' Identifier ')' ':' <Expression>
                   | Identifier ':'  Identifier ':'
                   | '(' Identifier ')' ':' Identifier ':'
            */

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            return result;
        }
    }


    public class Rule_KMOptList : AnalysisRule
    {
        public Rule_KMOptList(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {    

            /*
                        <KMOptList> ::= <KMOpt> | <KMOpt> <KMOptList>

            <KMOpt> ::= WEIGHTVAR '=' Identifier
                   | TIMEUNIT '=' String
                   | GRAPHTYPE '=' String
                   | OUTTABLE '=' Identifier
                   | STRATAVAR '=' <IdentifierList>
                   | GRAPH '=' <IdentifierList>
                   | DIALOG
                   | SHOWOBSERVED
                   | PVALUE '=' Percent
                   | PVALUE '=' RealLiteral
             */
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            return result;
        }
    }
}

