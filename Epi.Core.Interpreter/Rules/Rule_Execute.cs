using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using com.calitha.goldparser;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    /*
		<Execute_Statement> ::= EXECUTE String
                        | EXECUTE File
                        | EXECUTE Url
                        | EXECUTE WAITFOREXIT File
                        | EXECUTE WAITFOREXIT String
                        | EXECUTE WAITFOREXIT Url
                        | EXECUTE NOWAITFOREXIT File
                        | EXECUTE NOWAITFOREXIT String
                        | EXECUTE NOWAITFOREXIT Url
    */
    public class Rule_Execute : AnalysisRule
    {
        bool HasRun = false;

        string commandName = String.Empty;
        string commandlineString = String.Empty;
        string executeOption = String.Empty;
        string firstParam = String.Empty;

        public Rule_Execute(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            //<Simple_Execute_Statement> ::= EXECUTE String
            //<Execute_File_Statement> ::= EXECUTE File
            //<Execute_Url_Statement> ::= EXECUTE Url

            firstParam = this.GetCommandElement(pToken.Tokens, 1);
            if (firstParam.ToUpper() == CommandNames.NOWAITFOREXIT.ToUpper() || firstParam.ToUpper() == CommandNames.WAITFOREXIT.ToUpper())
            {
                executeOption = firstParam;
                commandlineString = this.GetCommandElement(pToken.Tokens, 2);
            }
            else
            {
                commandlineString = firstParam;
            }

            commandlineString = commandlineString.Trim(new char[] { '\"', '\'' });

            if (!commandlineString.EndsWith(".xlx") & !commandlineString.EndsWith(".xls") & !commandlineString.EndsWith(".xlsx") & !commandlineString.EndsWith(".docx") & !commandlineString.EndsWith(".pdf") & !commandlineString.EndsWith(".html") & !commandlineString.EndsWith(".htm"))
            {
                List<string> temp = splitCommandLine(commandlineString);

                firstParam = temp[0];
                if (temp.Count > 1)
                {
                    commandlineString = temp[1];
                }
                else
                {
                    commandlineString = string.Empty;
                }
            }
            else
            {
                commandlineString = string.Empty;
            }
        }


        /// <summary>
        /// performs execute command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            if (!this.HasRun)
            {
                Process process;

                switch (executeOption.ToUpper())
                {

                    case "WAITFOREXIT":
                        process = new Process();
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.FileName = firstParam;
                        process.StartInfo.Arguments = commandlineString;
                        process.WaitForExit();

                        break;
                    case "NOWAITFOREXIT":
                        process = new Process();
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.FileName = firstParam;
                        process.StartInfo.Arguments = commandlineString;
                        process.Start();
                        break;
                    case "":
                    default:
                        if (string.IsNullOrEmpty(commandlineString))
                        {
                            Process.Start(firstParam);
                        }
                        else
                        {
                            process = new Process();
                            process.StartInfo.UseShellExecute = false;
                            process.StartInfo.FileName = firstParam;
                            process.StartInfo.Arguments = commandlineString;
                            process.Start();
                        }
                        break;
                }
                this.HasRun = true;
            }
            return result;
        }



        private List<string> splitCommandLine(string pValue)
        {
            List<string> result = new List<string>();
            StringBuilder value = new StringBuilder();
            bool ignoreSpace = false;
            bool firstSpaceFound = false;

            foreach (char c in pValue)
            {
                if (!firstSpaceFound)
                {
                    if (c == '\"')
                    {
                        ignoreSpace = !ignoreSpace;
                    }
                    else
                    {
                        ignoreSpace = false;
                    }

                    if (c == ' ' && ignoreSpace == false)
                    {
                        firstSpaceFound = true;
                        result.Add(value.ToString());
                        value.Length = 0;
                    }
                    else
                    {
                        value.Append(c);
                    }
                }
                else
                {
                    value.Append(c);
                }
            }
             result.Add(value.ToString());
            return result;

        }

    }


    
}
