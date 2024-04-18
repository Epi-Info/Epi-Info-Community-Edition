using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;
using Epi.Data;
using System.Data;
using System.Reflection;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public partial class Rule_PythonCode : AnalysisRule
    {
        bool HasRun = false;
        List<string> IdentifierList = null;

        bool isExceptionList = false;
        string commandText = string.Empty;
        string pythonPath = null;
        string dictListName = null;
        string pythonStatements = "";

        public Rule_PythonCode(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            this.IdentifierList = new List<string>();

            /*Programming_Chars             = ('>>> '{Valid Chars}*)

            <Python_Statements> ::= PYTHON PYTHONPATH '=' String DATASET '=' String <Python_Code> END-PYTHON

            <Python_Code> ::= <Python_Code> <Python_Code_Line>
                   | <Python_Code_Line>

            <Python_Code_Line> ::= Programming_Chars
                   | '\n'
*/

            commandText = this.ExtractTokens(pToken.Tokens);
            foreach (Token T in pToken.Tokens)
            {
                if (T is NonterminalToken)
                {
                    NonterminalToken NT = (NonterminalToken)T;
                    switch (NT.Symbol.ToString())
                    {
                        case "<Python_Statements>":
                            break;
                        case "<Python_Code>":
                            this.SetPythonStatements(NT);
                            break;
                        case "<Python_Code_Line>":
                            //this.SetFrequencyOption(NT);
                            break;
                    }
                }
                else
                {
                    TerminalToken TT = (TerminalToken)T;
                    switch (TT.Symbol.ToString())
                    {
                        case "PYTHON":
                            break;
                        case "PYTHONPATH":
                            this.pythonPath = this.GetCommandElement(pToken.Tokens, 3).Trim(new char[] { '[', ']' }).Trim(new char[] { '"' });
                            break;
                        case "DATASET":
                            this.dictListName = this.GetCommandElement(pToken.Tokens, 6).Trim(new char[] { '[', ']' }).Trim(new char[] { '"' });
                            break;
                        case "END-PYTHON":
                            // Execute here
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void SetPythonStatements(NonterminalToken pToken)
        {
            //<FreqOpts> <FreqOpt> | <FreqOpt> 
            foreach (Token T in pToken.Tokens)
            {
                if (T is NonterminalToken)
                {
                    NonterminalToken NT = (NonterminalToken)T;
                    switch (NT.Symbol.ToString())
                    {
                        case "<Python_Code>":
                            this.SetPythonStatements(NT);
                            break;
                        case "<Python_Code_Line>":
                            this.SetPythonStatements(NT);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    // strip ">>> " and append to this.pythonStatements
                    // (don't forget line feeds)
                    if (!String.IsNullOrEmpty(this.pythonStatements))
                        this.pythonStatements += "\n";
                    this.pythonStatements += T.ToString().Replace(">>> ", String.Empty);
                }
            }
        }

        /// <summary>
        /// performs execution of Python code
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;
            return result; // Need to figure out what Execute should be. And also what to do with the tokens.
        }
    }
}
