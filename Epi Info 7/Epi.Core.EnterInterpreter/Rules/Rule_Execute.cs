using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using com.calitha.goldparser;

namespace Epi.Core.EnterInterpreter.Rules
{
    /*
		| <Simple_Execute_Statement>
		| <Execute_File_Statement>
		| <Execute_Url_Statement>
		| <Execute_Wait_For_Exit_File_Statement>
		| <Execute_Wait_For_Exit_String_Statement>
		| <Execute_Wait_For_Exit_Url_Statement>
		| <Execute_No_Wait_For_Exit_File_Statement>
		| <Execute_No_Wait_For_Exit_String_Statement>
		| <Execute_No_Wait_For_Exit_Url_Statement>
    */
    public class Rule_Execute : EnterRule
    {
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

            string[] temp = commandlineString.Split(new char[] { ' ' });

            firstParam = "";

            for(int i = 0; i < temp.Length; i++)
            {
                firstParam += " " + temp[i];
                firstParam = firstParam.Trim();

                if (File.Exists(firstParam) == true)
                {
                    break;                
                }
            }

            commandlineString = commandlineString.Substring(firstParam.Length).Trim();
        }


        /// <summary>
        /// performs execute command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;

            bool firstParamExecutable = firstParam.ToLower().EndsWith(".exe")
                || firstParam.ToLower().EndsWith(".bat")
                || firstParam.ToLower().EndsWith(".cmd");

            string fileName = firstParam;
            string workingDirectory = "";

            if (firstParamExecutable == true)
            {
                fileName = Path.GetFileName(firstParam);
                workingDirectory = Path.GetDirectoryName(firstParam);
            }

            if (firstParamExecutable == false)
            {
                Process.Start(fileName);
            }
            else
            {
                Process process = new Process();

                process.StartInfo.FileName = fileName;
                process.StartInfo.WorkingDirectory = workingDirectory;
                process.StartInfo.Arguments = commandlineString;
                process.StartInfo.CreateNoWindow = true;

                if (executeOption.ToUpper() == "WAITFOREXIT")
                {
                    process.Start();
                    process.WaitForExit();
                    process.Close();
                }
                else
                {
                    process.Start();
                }
            }

            return result;
        }
    }
}
