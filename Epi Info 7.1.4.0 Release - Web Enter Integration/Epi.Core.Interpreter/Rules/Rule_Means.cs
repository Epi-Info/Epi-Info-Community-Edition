using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;
using Epi.Data;
using System.Data;
using System.Reflection;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_Means : AnalysisRule
    {
        bool HasRun = false;

        string Numeric_Variable = null;
        string Cross_Tabulation_Variable = null;
        string OutTable = null;
        List<string> StratvarList = null;
        string WeightVar = null;
        string PSUVar = null;
        string NoWrap = null;
        string ColumnSize = null;
        string commandText = string.Empty;

        Dictionary<string, string> inputVariableList;

        EpiInfo.Plugin.IAnalysisStatistic MeansStatistic;



        //MEANS AGE CABBAGESAL STRATAVAR= BROWNBREAD CAKES CODE_RW  SEX WEIGHTVAR=ILL
        //MEANS AGE CABBAGESAL STRATAVAR= BAKEDHAM CHOCOLATE COFFEE WEIGHTVAR=ILL
        //MEANS NumCigar Sex STRATAVAR=Strata WEIGHTVAR=SampW PSUVAR=PSUID 
        //MEANS CHL CHD 

        public Rule_Means(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            StratvarList = new List<string>();
            /*
             <Numeric_Variable> ::= '*' | Identifier
            <Cross_Tabulation_Variable> ::= '*' | Identifier
            <Means_Statement> ::= MEANS <Numeric_Variable> <FreqOpts>
                                | MEANS <Numeric_Variable> <Cross_Tabulation_Variable>
                                | MEANS <Numeric_Variable> <Cross_Tabulation_Variable> <FreqOpts>
             */

            commandText = this.ExtractTokens(pToken.Tokens);

            foreach (Token T in pToken.Tokens)
            {
                if (T is NonterminalToken)
                {
                    NonterminalToken NT = (NonterminalToken)T;
                    switch (NT.Symbol.ToString())
                    {

                        case "<Numeric_Variable>":
                            this.SetNumeric_Variable(NT); 
                            break;
                        case "<Cross_Tabulation_Variable>":
                            this.SetCross_Tabulation_Variable(NT);
                            break;
                        case "<FreqOpts>":
                            this.SetFrequencyOptions(NT);
                            break;
                        case "<FreqOpt>":
                            this.SetFrequencyOption(NT);
                            break;
                    }

                }
            }
        }


        private void SetNumeric_Variable(NonterminalToken pToken)
        {
            this.Numeric_Variable = this.GetCommandElement(pToken.Tokens, 0).Trim(new char[] { '[', ']' });
        }

        private void SetCross_Tabulation_Variable(NonterminalToken pToken)
        {
            this.Cross_Tabulation_Variable = this.GetCommandElement(pToken.Tokens, 0).Trim(new char[] { '[', ']'});
        }


        private void SetFrequencyOptions(NonterminalToken pToken)
        {
            //<FreqOpts> <FreqOpt> | <FreqOpt> 
            foreach (Token T in pToken.Tokens)
            {
                if (T is NonterminalToken)
                {
                    NonterminalToken NT = (NonterminalToken)T;
                    switch (NT.Symbol.ToString())
                    {
                        case "<FreqOpt>":
                            this.SetFrequencyOption(NT);
                            break;
                        case "<FreqOpts>":
                            this.SetFrequencyOptions(NT);
                            break;
                    }
                }
            }

        }

        private void SetFrequencyOption(NonterminalToken pToken)
        {
            switch (pToken.Rule.Rhs[0].ToString())
            {
                case "STRATAVAR":
                    this.StratvarList.AddRange(AnalysisRule.SpliIdentifierList(this.GetCommandElement(pToken.Tokens, 2).Trim())); 
                    break;

                case "WEIGHTVAR":
                    this.WeightVar = this.GetCommandElement(pToken.Tokens, 2).Trim(new char[] { '[', ']' });
                    break;
                case "OUTTABLE":
                    this.OutTable = this.GetCommandElement(pToken.Tokens, 2).Trim(new char[] { '[', ']' });
                    break;
                case "PSUVAR":
                    this.PSUVar = this.GetCommandElement(pToken.Tokens, 2).Trim(new char[] { '[', ']' });
                    break;
                case "NOWRAP":
                    this.NoWrap = "NOWRAP";
                    break;
                case "COLUMNSIZE":
                    this.ColumnSize  = this.GetCommandElement(pToken.Tokens, 2);
                    break;
            }
            
        }


        /// <summary>
        /// performs execution of the MEANS command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            if (!this.HasRun)
            {

                Dictionary<string, string> setProperties = this.Context.GetGlobalSettingProperties();

                List<string> NumericVariableList = new List<string>();
                NumericVariableList.Add(this.Numeric_Variable);

                bool isExceptionList = false;
                this.Context.ExpandGroupVariables(NumericVariableList, ref isExceptionList);


                foreach (string Numeric_Variable in NumericVariableList)
                {
                    inputVariableList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    inputVariableList.Add("Numeric_Variable", Numeric_Variable);
                    inputVariableList.Add("Cross_Tabulation_Variable", this.Cross_Tabulation_Variable);


                    this.Context.ExpandGroupVariables(this.StratvarList, ref isExceptionList);

                    if (this.StratvarList != null && this.StratvarList.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();

                        foreach (string s in this.StratvarList)
                        {
                            sb.Append(s);
                            sb.Append(",");
                        }
                        sb.Length = sb.Length - 1;

                        inputVariableList.Add("StratvarList", sb.ToString());
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

                    if (this.ColumnSize != null)
                    {
                        inputVariableList.Add("ColumnSize", this.ColumnSize);
                    }

                    if (this.NoWrap != null)
                    {
                        inputVariableList.Add("NoWrap", this.NoWrap);
                    }

                    EpiInfo.Plugin.IDataSource DataSource = this.Context.GetDefaultIDataSource();

                    AnalysisStatisticExecuteHost statisticHost = new AnalysisStatisticExecuteHost(this.Context, setProperties, DataSource, inputVariableList, this.Context.CurrentSelect.ToString(), this.Context.AnalysisInterpreterHost);

                    if (string.IsNullOrEmpty(this.PSUVar))
                    {
                        this.MeansStatistic = this.Context.GetStatistic("Means", statisticHost);
                    }
                    else
                    {
                        this.MeansStatistic = this.Context.GetStatistic("ComplexSampleMeans", statisticHost);
                    }

                    this.MeansStatistic.Execute();

                    this.MeansStatistic = null;
                }
                this.HasRun = true;
            }

            return result;
        }

    }

}
