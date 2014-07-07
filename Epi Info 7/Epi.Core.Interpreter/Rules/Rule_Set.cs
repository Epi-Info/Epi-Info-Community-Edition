using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public class Rule_Set : AnalysisRule
    {
        /*
        !***  			Set  Statement  		***!
        <Set_Statement> ::= SET <SetList> End-Set

        <SetClause> ::=  STATISTICS '=' <StatisticsOption>	!These options could be set in FREQ,MATCH, and MEANS commands also
            | PROCESS '=' <ProcessOption>
            | PROCESS '=' Identifier
            | Boolean '=' String
            | YN	'=' String ',' String ',' String
            | DELETED '=' <DeletedOption>
            | PERCENTS '=' <OnOff>
            | MISSING '=' <OnOff>
            | IGNORE '=' <OnOff>
            | SELECT '=' <OnOff>
            | FREQGRAPH '=' <OnOff>
            | HYPERLINKS '=' <OnOff>
            | SHOWPROMPTS '=' <OnOff>
            | TABLES '=' <OnOff>
            | USEBROWSER '=' <OnOff>

        <SetList> ::= <SetList> <SetClause>
            | <SetClause>
 						   			   
        <OnOff> ::= ON
            | OFF
            | Boolean

        <DeletedOption> ::= <OnOff> 
            |Identifier

        <StatisticsOption>	::= NONE
            | MINIMAL
            | INTERMEDIATE
            | COMPLETE

        <ProcessOption> ::= UNDELETED
            | DELETED
            | BOTH

        !*** 			End 					***!
        */
        
        Dictionary<string, string> _setOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public Rule_Set(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            foreach (Token T in pToken.Tokens)
            {
                if (T is NonterminalToken)
                {
                    NonterminalToken NT = (NonterminalToken)T;
                    switch (NT.Symbol.ToString())
                    {
                        case "<SetList>":
                            this.SetSetList(NT);
                            break;
                        case "<SetClause>":
                            this.SetSetClause(NT);
                            break;
                    }
                }
            }
        }

        private void SetSetList(NonterminalToken nonTermToken)
        {
            //// <SetList> ::= <SetList> <SetClause> | <SetClause>

            foreach (Token token in nonTermToken.Tokens)
            {
                if (token is NonterminalToken)
                {
                    NonterminalToken asNonTermToken = (NonterminalToken)token;

                    switch (asNonTermToken.Symbol.ToString())
                    {
                        case "<SetClause>":
                            this.SetSetClause(asNonTermToken);
                            break;

                        case "<SetList>":
                            foreach (NonterminalToken tokenInList in asNonTermToken.Tokens)
                            {
                                this.SetSetClause(tokenInList);
                            }
                            break;
                    }
                }
            }
        }

        private void SetSetClause(NonterminalToken nonTermToken)
        {
            ////    <SetClause> ::=  STATISTICS '=' <StatisticsOption>	!These options could be set in FREQ,MATCH, and MEANS commands also
            ////        | PROCESS '=' <ProcessOption>
            ////        | PROCESS '=' Identifier
            ////        | Boolean '=' String
            ////        | YN	'=' String ',' String ',' String
            ////        | DELETED '=' <DeletedOption>
            ////        | PERCENTS '=' <OnOff>
            ////        | MISSING '=' <OnOff>
            ////        | IGNORE '=' <OnOff>
            ////        | SELECT '=' <OnOff>
            ////        | FREQGRAPH '=' <OnOff>
            ////        | HYPERLINKS '=' <OnOff>
            ////        | SHOWPROMPTS '=' <OnOff>
            ////        | TABLES '=' <OnOff>
            ////        | USEBROWSER '=' <OnOff>

            string Key = this.GetCommandElement(nonTermToken.Tokens, 0);
            string Value = null;
            Token token = nonTermToken.Tokens[2];

            if (token is NonterminalToken)
            {
                Value = this.ExtractTokens(((NonterminalToken)token).Tokens).Trim();
            }
            else
            {
                Value = this.GetCommandElement(nonTermToken.Tokens, 2).Trim();
            }

            if (_setOptions.ContainsKey(Key))
            {
                _setOptions[Key] = Value.Trim('"');
            }
            else
            {
                _setOptions.Add(Key, Value.Trim('"'));
            }
        }

        /// <summary>
        /// currently unimplemented - will perform the SET command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            this.Context.AddConfigSettings(_setOptions);
            return null;
        }
    }
}
