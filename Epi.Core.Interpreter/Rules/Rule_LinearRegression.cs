using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;
using EpiInfo.Plugin;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_LinearRegression : AnalysisRule
    {
        bool HasRun = false;

        string Identifier;
        string Terms;
        string commandText;
        List<string> TermList;
        
        bool nointercept;
        string WeightVar;
        string OutTable;
        string pvalue;
        //'=' <TermList> <LogisticOpts>


        public Rule_LinearRegression(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.nointercept = false;
            this.Identifier = this.GetCommandElement(pToken.Tokens, 1).TrimStart('[').TrimEnd(']');
            this.Terms = this.GetCommandElement(pToken.Tokens, 3);
            this.commandText = this.ExtractTokens(pToken.Tokens);

            if (pToken.Tokens.Length >= 4)
            {
                //this.IdentifierList = this.GetCommandElement(pToken.Tokens, 4).Split(' ');
                NonterminalToken T = (NonterminalToken)pToken.Tokens[4];

                this.SetRegressionOptions(T);
            }

            if (this.WeightVar != null)
            {
                this.WeightVar = this.WeightVar.TrimStart('[').TrimEnd(']');
            }

            // Make sure dummy vars aren't split up either!
            if (this.Terms.Contains("(") || this.Terms.Contains(")"))
            {
                this.Terms = this.Terms.Replace("( ", "(");
                this.Terms = this.Terms.Replace(" )", ")");                
            }

            TermList = new List<string>();
            string[] termArray;
            // Make sure each interaction term isn't split up!
            this.Terms = this.Terms.Replace(" *", "*");
            this.Terms = this.Terms.Replace("* ", "*");
            termArray = this.Terms.Split(' ');

            StringBuilder sb = new StringBuilder();
            bool isMultiWorded = false;

            foreach (string s in termArray)
            {                
                sb.Append(s.TrimStart('[').TrimEnd(']'));
                
                bool isLastWord = false;

                if (s.StartsWith("[") && !(s.EndsWith("]")))
                {
                    isMultiWorded = true;
                }
                else if (!(s.StartsWith("[")) && s.EndsWith("]"))
                {
                    isLastWord = true;
                    isMultiWorded = false;
                }

                if (isMultiWorded)
                {
                    // do nothing, keep building
                    sb.Append(" ");
                }
                else
                {
                    TermList.Add(sb.ToString());
                    sb = new StringBuilder();
                }
            }
     
            
/*  Regress BirthWeight = AgeInDays 
            REGRESS Identifier '=' <TermList> <LogisticOpts>
            <BaseTerm>                          ::= Identifier | '(' Identifier ')'
<Term>                              ::= <BaseTerm> | <Term> '*' <BaseTerm>
<TermList>                          ::= <Term> | <TermList> <Term>
<LogisticOpts>                      ::= <LogisticOpts> <LogisticOpt>
                                | <LogisticOpt>
                                

<LogisticOpt>                       ::= <LogisticTitleOpt>
                                | <WeightOpt>
                                | <LogisticMatchOpt>
                                | <LogisticPValPercentOpt>
                                | <LogisticPValRealOpt>
                                | <OutTableOpt>
                                | <LogisticNoInterceptOpt>

<LogisticTitleOpt>                      ::= TITLETEXT '=' String
<LogisticMatchOpt>                      ::= MATCHVAR '=' Identifier
<LogisticPValPercentOpt>                    ::= PVALUE '=' Percent
<LogisticPValRealOpt>                   ::= PVALUE '=' RealLiteral
<OutTableOpt>                           ::= OUTTABLE '=' Identifier <KeyVarList>
<LogisticNoInterceptOpt>                    ::= NOINTERCEPT             
             
*/
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
                // Create the AnalysisStatisticExecuteHost analysisStatisticExecuteHost = null;
                // Create dynamic reference to the Linear Regression assembly
                // call the LinearRegress.ProcessData(IAnalysisStatisticExecuteHost AnalysisStatisticExecuteHost);
                // method
                IAnalysisStatistic LinearRegress = null;

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
                inputVariableList.Add(this.Identifier, "DependVar");
                bool containsDummyVariables = false;

                if (this.nointercept == false)
                {
                    setProperties.Add("Intercept", "true");
                }
                else
                {
                    setProperties.Add("Intercept", "false");
                }

                if (!string.IsNullOrEmpty(this.pvalue))
                {
                    double p = 0.95;
                    bool success = Double.TryParse(this.pvalue.Replace("%", string.Empty), out p);
                    if (success)
                    {
                        setProperties.Add("P", p.ToString());
                    }
                }

                Configuration config = Configuration.GetNewInstance();

                setProperties.Add("BLabels", config.Settings.RepresentationOfYes + ";" + config.Settings.RepresentationOfNo + ";" + config.Settings.RepresentationOfMissing); // TODO: Replace Yes, No, Missing with global vars

                //// Make sure dummy vars aren't split up either!
                //if(this.TermList.Contains("(") || this.TermList.Contains(")"))
                //{
                //    this.TermList = this.TermList.Replace("( ", "(");
                //    this.TermList = this.TermList.Replace(" )", ")");
                //    containsDummyVariables = true;
                //}

                //string[] terms;
                //// Make sure each interaction term isn't split up!
                //this.TermList = this.TermList.Replace(" *", "*");
                //this.TermList = this.TermList.Replace("* ", "*");
                //terms = this.TermList.Split(' ');            

                //foreach (string s in terms)
                //{


                //    if (!s.Contains("*") && s.Contains("(") && s.Contains(")"))
                //    {
                //        // dummy variables (discrete terms) get added to the terms list, but this is handled in the regression code
                //        inputVariableList.Add(s.Replace("(", string.Empty).Replace(")", string.Empty), "Discrete");
                //    }
                //    else if (s.Contains("*"))
                //    {
                //        inputVariableList.Add(s, "Term");
                //    }
                //    else
                //    {
                //        inputVariableList.Add(s, "Unsorted");
                //    }
                //}

                foreach (string s in TermList)
                {
                    if (!s.Contains("*") && s.Contains("(") && s.Contains(")"))
                    {
                        // dummy variables (discrete terms) get added to the terms list, but this is handled in the regression code
                        inputVariableList.Add(s.Replace("(", string.Empty).Replace(")", string.Empty), "Discrete");
                    }
                    else if (s.Contains("*"))
                    {
                        inputVariableList.Add(s.Replace("[", "").Replace("]", ""), "Term");
                    }
                    else
                    {
                        inputVariableList.Add(s, "Unsorted");
                    }
                }

                if (!string.IsNullOrEmpty(this.WeightVar))
                {
                    // Make sure our weight variable isn't a term or the dependent variable. This shouldn't occur through the UI, but the user can still type something like this in manually
                    if (inputVariableList.ContainsKey(this.WeightVar))
                    {
                        Dictionary<string, string> args = new Dictionary<string, string>();
                        args.Add("COMMANDNAME", CommandNames.REGRESS);
                        args.Add("COMMANDTEXT", commandText.Trim());
                        args.Add("HTMLRESULTS", "<br clear=\"all\" /><p align=\"left\"><b><tlt>Weight variable cannot be a term or dependent variable</tlt></b></p>");

                        this.Context.AnalysisCheckCodeInterface.Display(args);

                        return result;
                    }
                    inputVariableList.Add(this.WeightVar, "WeightVar");
                }

                IDataSource DataSource = this.Context.GetDefaultIDataSource();

                AnalysisStatisticExecuteHost statisticHost = new AnalysisStatisticExecuteHost(this.Context, setProperties, DataSource, inputVariableList, this.Context.CurrentSelect.ToString(), this.Context.AnalysisInterpreterHost);

                LinearRegress = this.Context.GetStatistic("LinearRegression", statisticHost);
                LinearRegress.Execute();
                this.HasRun = true;
            }

            return result;
        }

        private void SetRegressionOptions(NonterminalToken pToken)
        {
            for (int i = 0; i < pToken.Tokens.Length; i++)
            {
                if (pToken.Tokens[i].ToString().Equals("NOINTERCEPT"))
                {
                    this.nointercept = true;
                    continue;
                }
                if (pToken.Tokens[i] is TerminalToken)
                {
                    continue;
                }

                NonterminalToken T = (NonterminalToken)pToken.Tokens[i];

                switch (T.Rule.Rhs[0].ToString())
                {
                    case "WEIGHTVAR":
                        this.WeightVar = this.GetCommandElement(pToken.Tokens, 2);
                        break;
                    case "NOINTERCEPT":
                        this.nointercept = true;
                        break;
                    case "PVALUE":
                        // Still not activating this -zfj4 20240830
                        break;
                    case "<LogisticOpts>":                    
                    case "<LogisticOpt>":
                        for (int j = 0; j < T.Tokens.Length; j++)
                        {
                            string temp = this.ExtractTokens(((NonterminalToken)T.Tokens[j]).Tokens);
                            string[] tmp = temp.Split('=');

                            if (tmp[0].ToUpperInvariant().Contains("NOINTERCEPT"))
                            {
                                this.nointercept = true;
                                tmp[0] = tmp[0].ToUpperInvariant().Replace("NOINTERCEPT", string.Empty);
                            }

                            switch (tmp[0].ToUpperInvariant().Trim())
                            {                                
                                case "WEIGHTVAR":
                                    this.WeightVar = tmp[1].Trim();
                                    break;
                            }
                        }
                        break;
                    case "<WeightOpt>":
                        this.WeightVar = this.GetCommandElement(T.Tokens, 2);                                                   
                        break;
                    case "<LogisticPValPercentOpt>":
                        this.pvalue = this.GetCommandElement(T.Tokens, 2);                                                   
                        break;
                    case "<LogisticNoInterceptOpt>":
                        this.nointercept = true;
                        break;
                }
            }
        }
    }
}

