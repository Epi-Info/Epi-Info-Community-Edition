using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.calitha.goldparser;
using Epi.Analysis.Statistics;
using EpiInfo.Plugin;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_LogisticRegression : AnalysisRule
    {
        bool HasRun = false;

        string Identifier;
        string TermList;
        string commandText;                
        
        bool nointercept;
        string WeightVar;
        bool LinkFunctionLog;
        string MatchVar;
        string OutTable;
        string pvalue;
        //'=' <TermList> <LogisticOpts>


        public Rule_LogisticRegression(Rule_Context pContext, NonterminalToken pToken)
            : base(pContext)
        {
            this.nointercept = false;
            this.LinkFunctionLog = false;
            this.Identifier = this.GetCommandElement(pToken.Tokens, 1).Trim(new char[] {'[',']' });
            this.TermList = this.GetCommandElement(pToken.Tokens, 3);
            this.commandText = this.ExtractTokens(pToken.Tokens);

            if (pToken.Tokens.Length >= 4)
            {
                //this.IdentifierList = this.GetCommandElement(pToken.Tokens, 4).Split(' ');
                NonterminalToken T = (NonterminalToken)pToken.Tokens[4];

                this.SetRegressionOptions(T);
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
                IAnalysisStatistic LogisticRegress = null;

                //View view = null;

                //if (Context.CurrentProject != null)
                //{
                //    view = Context.CurrentProject.GetViewByName(Context.CurrentRead.Identifier);
                //}

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

                if (this.nointercept == false)
                {
                    setProperties.Add("Intercept", "true");
                    inputVariableList.Add("true", "Intercept");
                }
                else
                {
                    setProperties.Add("Intercept", "false");
                    inputVariableList.Add("false", "Intercept");
                }

                if (!string.IsNullOrEmpty(this.pvalue))
                {
                    double p = 0.95;
                    bool success = Double.TryParse(this.pvalue.Replace("%", string.Empty), out p);
                    if (success)
                    {
                        setProperties.Add("P", p.ToString());
                        inputVariableList.Add(p.ToString(), "P");
                    }
                }

                // Make sure dummy vars aren't split up either!
                if (this.TermList.Contains("(") || this.TermList.Contains(")"))
                {
                    this.TermList = this.TermList.Replace("( ", "(");
                    this.TermList = this.TermList.Replace(" )", ")");
                }

                string[] terms;
                // Make sure each interaction term isn't split up!
                this.TermList = this.TermList.Replace(" *", "*");
                this.TermList = this.TermList.Replace("* ", "*");
                terms = this.TermList.Split(' ');

                foreach (string s in terms)
                {
                    if (!s.Contains("*"))
                    {
                        if ((s.Contains("(") && s.Contains(")")))// || (view != null && view.Fields.Contains(s) && view.Fields[s] is Epi.Fields.YesNoField))
                        {
                            inputVariableList.Add(s.Replace("(", string.Empty).Replace(")", string.Empty), "Discrete");
                        }
                        else
                        {
                            inputVariableList.Add(s.TrimStart('[').TrimEnd(']'), "Unsorted");
                        }
                    }
                    else
                    {
                        inputVariableList.Add(s.Replace("[", "").Replace("]", ""), "Term");
                    }
                }

                if (!string.IsNullOrEmpty(this.WeightVar))
                {
                    // Make sure our weight variable isn't a term or the dependent variable. This shouldn't occur through the UI, but the user can still type something like this in manually
                    if (inputVariableList.ContainsKey(this.WeightVar))
                    {
                        Dictionary<string, string> args = new Dictionary<string, string>();
                        args.Add("COMMANDNAME", CommandNames.LOGISTIC);
                        args.Add("COMMANDTEXT", commandText.Trim());
                        args.Add("HTMLRESULTS", "<br clear=\"all\" /><p align=\"left\"><b><tlt>Weight variable cannot be a term or dependent variable</tlt></b></p>");

                        this.Context.AnalysisCheckCodeInterface.Display(args);

                        return result;
                    }
                    inputVariableList.Add(this.WeightVar, "WeightVar");
                }

                if (!string.IsNullOrEmpty(this.MatchVar))
                {
                    // Make sure our weight variable isn't a term or the dependent variable. This shouldn't occur through the UI, but the user can still type something like this in manually
                    if (inputVariableList.ContainsKey(this.MatchVar))
                    {
                        Dictionary<string, string> args = new Dictionary<string, string>();
                        args.Add("COMMANDNAME", CommandNames.LOGISTIC);
                        args.Add("COMMANDTEXT", commandText.Trim());
                        args.Add("HTMLRESULTS", "<br clear=\"all\" /><p align=\"left\"><b><tlt>Match variable cannot be a term or dependent variable</tlt></b></p>");

                        this.Context.AnalysisCheckCodeInterface.Display(args);

                        return result;
                    }
                    inputVariableList.Add(this.MatchVar, "MatchVar");
                }

                IDataSource DataSource = this.Context.GetDefaultIDataSource();

                AnalysisStatisticExecuteHost statisticHost = new AnalysisStatisticExecuteHost(this.Context, setProperties, DataSource, inputVariableList, this.Context.CurrentSelect.ToString(), this.Context.AnalysisInterpreterHost);

                LogisticRegress = this.Context.GetStatistic("LogisticRegression", statisticHost);
                ((LogisticRegression)LogisticRegress).setDoLogBinomial(this.LinkFunctionLog);
                LogisticRegress.Execute();

                this.HasRun = true;
            }

            return result;
        }

        private void SetRegressionOptions(Token pToken)
        {
            if (pToken is NonterminalToken)
            {
                NonterminalToken NT = (NonterminalToken)pToken;

                switch (NT.Symbol.ToString())
                {
                    case "<LogisticOpts>":
                        if (NT.Tokens.Length > 0)
                        {
                            if (NT.Tokens.Length > 1)
                            {
                                this.SetRegressionOptions(NT.Tokens[0]);
                                this.SetLogisticOpt(NT.Tokens[1]);
                            }
                            else
                            {
                                this.SetLogisticOpt(NT.Tokens[0]);
                            }
                        }
                        break;

                    case "<LogisticOpt>":
                        this.SetLogisticOpt(pToken);
                        break;
                    case "<WeightOpt>":
                        if (NT.Tokens.Length > 0)
                        {
                            string OptionString = this.GetCommandElement(NT.Tokens, 0).Trim();
                            if (OptionString.ToUpperInvariant().Contains("NOINTERCEPT"))
                            {

                            }
                            else
                            {
                                switch (OptionString)
                                {
                                    case "WEIGHTVAR":
                                        this.WeightVar = this.GetCommandElement(NT.Tokens, 2).Trim(new char[] { '[', ']' });
                                        break;
                                    case "MATCHVAR":
                                        this.MatchVar = this.GetCommandElement(NT.Tokens, 2).Trim(new char[] { '[', ']' });
                                        break;
                                }
                            }

                            for (int j = 0; j < NT.Tokens.Length; j++)
                            {
                                string temp = this.ExtractTokens(((NonterminalToken)NT.Tokens[j]).Tokens);
                                string[] tmp = temp.Split('=');

                                if (tmp[0].ToUpperInvariant().Contains("NOINTERCEPT"))
                                {
                                    this.nointercept = true;
                                    tmp[0] = tmp[0].ToUpperInvariant().Replace("NOINTERCEPT", string.Empty);
                                }

                                else if (tmp[1].ToUpperInvariant().Contains("NOINTERCEPT"))
                                {
                                    this.nointercept = true;
                                    tmp[1] = tmp[1].ToUpperInvariant().Replace("NOINTERCEPT", string.Empty);
                                }

                                switch (tmp[0].ToUpperInvariant().Trim())
                                {
                                    case "WEIGHTVAR":
                                        this.WeightVar = tmp[1].Trim();
                                        break;
                                    case "MATCHVAR":
                                        this.MatchVar = tmp[1].Trim();
                                        break;
                                }
                            }
                        }
                        break;
                    case "<LogisticPValPercentOpt>":
                        this.pvalue = this.GetCommandElement(NT.Tokens, 2);
                        break;
                    case "<LogisticNoInterceptOpt>":
                        this.nointercept = true;
                        break;
                    case "<LogisticMatchOpt>":
                        this.MatchVar = ((TerminalToken)NT.Tokens[2]).Text.Trim(new char[] { '[', ']' });
                        break;
                    default:
                        if (NT.Tokens.Length > 0)
                        {
                            foreach (Token T in NT.Tokens)
                            {
                                SetRegressionOptions(T);
                            }
                        }
                        break;
                }

                
            }
            else
            {
                TerminalToken TT = (TerminalToken)pToken;

                switch (TT.Symbol.ToString())
                {
                    case "WEIGHTVAR":
                        this.WeightVar = TT.Text.Trim(new char[] { '[', ']' });
                        break;
                    case "MATCHVAR":
                        this.MatchVar = TT.Text.Trim(new char[] { '[', ']' });
                        break;
                }
            }

        }

        private void SetLogisticOpt(Token pToken)
        {

            /*
            <LogisticOpt>                       ::= <LogisticTitleOpt>
                                | WEIGHTVAR '=' Identifier
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
            <LogisticNoInterceptOpt>                    ::= NOINTERCEPT*/

            if (pToken is NonterminalToken)
            {
                NonterminalToken NT = (NonterminalToken)pToken;
                TerminalToken TT = (TerminalToken)NT.Tokens[0];
                TerminalToken TT2;


                switch (TT.Symbol.ToString())
                {
                    case "WEIGHTVAR":
                        TT2 = (TerminalToken)NT.Tokens[2];
                        this.WeightVar =  TT2.Text.Trim(new char[] { '[', ']' });
                        break;
                    case "LINKFUNCTION":
                        TT2 = (TerminalToken)NT.Tokens[2];
                        this.LinkFunctionLog = TT2.Text.Trim(new char[] { '[', ']' }).Equals("LOG");
                        break;

                    case "TITLETEXT":
                        // do nothing for now
                        break;
                    case "MATCHVAR":
                        TT2 = (TerminalToken)NT.Tokens[2];
                        this.MatchVar = TT2.Text.Trim(new char[] { '[', ']' });
                        break;
                    case "PVALUE":
                        TT2 = (TerminalToken)NT.Tokens[2];
                        this.pvalue = TT2.Text.Trim(new char[] { '[', ']' });
                        break;
                    case "OUTTABLE":
                        TT2 = (TerminalToken)NT.Tokens[2];
                        this.OutTable = TT2.Text.Trim(new char[] { '[', ']' });
                        break;
                    case "NOINTERCEPT":
                        this.nointercept = true;
                        break;
                }
            }
            else
            {
                TerminalToken TT = (TerminalToken)pToken;

                switch (TT.Symbol.ToString())
                {
                    case "WEIGHTVAR":
                        this.WeightVar =  TT.Text.Trim(new char[] { '[', ']' });
                        break;

                    case "TITLETEXT":
                        // do nothing for now
                        break;
                    case "MATCHVAR":
                        this.MatchVar = TT.Text.Trim(new char[] { '[', ']' });
                        break;
                    case "PVALUE":
                        this.pvalue = TT.Text.Trim(new char[] { '[', ']' });
                        break;
                    case "OUTTABLE":
                        this.OutTable = TT.Text.Trim(new char[] { '[', ']' });
                        break;
                    case "NOINTERCEPT":
                        this.nointercept = true;
                        break;
                }
            }


        }
    }
}

