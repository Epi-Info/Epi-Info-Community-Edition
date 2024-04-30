using System;
using System.Collections.Generic;
using System.Text;
using com.calitha.goldparser;
using Epi.Data;
using System.Data;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using System.IO.MemoryMappedFiles;
using Epi.Analysis.Statistics;

namespace Epi.Core.AnalysisInterpreter.Rules
{
    public partial class Rule_RCode : AnalysisRule
    {
        bool HasRun = false;
        List<string> IdentifierList = null;

        ProcessStartInfo processStartInfo = new ProcessStartInfo();

        bool isExceptionList = false;
        string commandText = string.Empty;
        string rPath = null;
        string dictListName = null;
        string rStatements = "";
        bool replaceData = false;

        public Rule_RCode(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            this.IdentifierList = new List<string>();

            /*Programming_Chars             = ('>>> '{Valid Chars}*)
            

<R_Statements> ::= R RPATH '=' String DATASET '=' String <R_Code> END-R
       | R RPATH '=' String DATASET '=' String <R_Code> END-R <R_End_Options>

<R_Code> ::= <R_Code> <R_Code_Line>
       | <R_Code_Line>

<R_Code_Line> ::= Programming_Chars
       | '\n'

<R_End_Options> ::= REPLACEDATA '=' YES
       | REPLACEDATA '=' NO
*/

            commandText = this.ExtractTokens(pToken.Tokens);
            foreach (Token T in pToken.Tokens)
            {
                if (T is NonterminalToken)
                {
                    NonterminalToken NT = (NonterminalToken)T;
                    switch (NT.Symbol.ToString())
                    {
                        case "<R_Statements>":
                            break;
                        case "<R_Code>":
                            this.SetRStatements(NT);
                            break;
                        case "<R_Code_Line>":
                            //this.SetFrequencyOption(NT);
                            break;
                        case "<R_End_Options>":
                            this.SetEndOption(NT);
                            break;
                    }
                }
                else
                {
                    TerminalToken TT = (TerminalToken)T;
                    switch (TT.Symbol.ToString())
                    {
                        case "R":
                            break;
                        case "RPATH":
                            this.rPath = this.GetCommandElement(pToken.Tokens, 3).Trim(new char[] { '[', ']' }).Trim(new char[] { '"' });
                            break;
                        case "DATASET":
                            this.dictListName = this.GetCommandElement(pToken.Tokens, 6).Trim(new char[] { '[', ']' }).Trim(new char[] { '"' });
                            break;
                        case "END-R":
                            // Execute here
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void SetEndOption(NonterminalToken pToken)
        {
            foreach (Token T in pToken.Tokens)
            {
                if (T is NonterminalToken)
                {
                }
                else
                {
                    TerminalToken TT = (TerminalToken)T;
                    switch (TT.Symbol.ToString())
                    {
                        case "YES":
                            this.replaceData = true;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void SetRStatements(NonterminalToken pToken)
        {
            foreach (Token T in pToken.Tokens)
            {
                if (T is NonterminalToken)
                {
                    NonterminalToken NT = (NonterminalToken)T;
                    switch (NT.Symbol.ToString())
                    {
                        case "<R_Code>":
                            this.SetRStatements(NT);
                            break;
                        case "<R_Code_Line>":
                            this.SetRStatements(NT);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    // strip ">>> " and append to this.rStatements
                    // (don't forget line feeds)
                    if (!String.IsNullOrEmpty(this.rStatements))
                        this.rStatements += "\n";
                    this.rStatements += T.ToString().Replace(">>> ", String.Empty);
                }
            }
        }

        /// <summary>
        /// performs execution of R code
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;


            // Write working data to JSON file in TEMP folder and then read with R
            DataTable dt = this.Context.DataSet.Tables[2];
            string dtjson = JsonConvert.SerializeObject(dt, Newtonsoft.Json.Formatting.Indented);

            // This isn't used but keeping it for potentially making a data.frame
            Dictionary<String, String> eicolumns = new Dictionary<String, String>();
            foreach (DataColumn col in dt.Columns)
            {
                eicolumns[col.ColumnName] = col.DataType.ToString();
            }

            string tempPath = System.IO.Path.GetTempPath();
            System.IO.File.WriteAllText(tempPath + "WorkingEIDataJSON.json", dtjson);
            string consumejson = "library(\"rjson\")\n";
            consumejson += this.dictListName + " <- fromJSON(file = \"";
            // R needs the slashes escaped
            consumejson += tempPath.Replace("\\", "\\\\") + "WorkingEIDataJSON.json\")\n";

            // Write the R data to JSON file in TEMP folder
            string writejson = "\n";
            if (this.replaceData)
            {
                writejson += "write(toJSON(" + this.dictListName + "), \"" + tempPath.Replace("\\", "\\\\") + "WrittenFromREIDataJSON.json\")" + "\n";
            }
            else
            {
                writejson = "";
            }

            System.IO.File.WriteAllText(tempPath + "EpiInfoRCode.R", consumejson + this.rStatements + writejson);

            this.processStartInfo.FileName = this.rPath;
            this.processStartInfo.Arguments = "\"" +
                tempPath + "EpiInfoRCode.R" +
                "\"";
            this.processStartInfo.UseShellExecute = false;
            this.processStartInfo.CreateNoWindow = true;
            this.processStartInfo.RedirectStandardOutput = true;
            this.processStartInfo.RedirectStandardError = true;
            this.processStartInfo.RedirectStandardInput = true;

            using (Process proc = Process.Start(this.processStartInfo))
            {
                using (StreamReader reader = proc.StandardOutput)
                {
                    string stderr = proc.StandardError.ReadToEnd();
                    string res = reader.ReadToEnd();
                    if (!String.IsNullOrEmpty(stderr))
                    {
                        Console.WriteLine(stderr);
                        throw new GeneralException(stderr);
                    }
                    if (!String.IsNullOrEmpty(res))
                    {
                        result = res;
                    }
                }
            }

            // More Memory Mapped File stuff to comment out
            // mmf.Dispose();
            // mmf0.Dispose();

            if (this.replaceData)
            {
                dtjson = System.IO.File.ReadAllText(tempPath + "WrittenFromREIDataJSON.json");
                dt = (DataTable)JsonConvert.DeserializeObject<DataTable>(dtjson.Replace("{}", "null"));
                dt.TableName = "Output";
                //this.Context.DataSet.Tables[2].Rows.Clear();
                //this.Context.DataSet.Tables[2].Columns.Clear();
                //foreach (DataColumn col in dt.Columns)
                //    this.Context.DataSet.Tables[2].Columns.Add(col);
                //foreach (DataRow row in dt.Rows)
                //    this.Context.DataSet.Tables[2].Rows.Add(row);
                this.Context.DataSet.Tables.Remove(this.Context.DataSet.Tables[2]);
                this.Context.DataSet.Tables.Add(dt);
            }

            if (result != null)
            {
                Dictionary<string, string> args = new Dictionary<string, string>();
                args.Add("COMMANDNAME", "R");
                if (this.replaceData)
                    args.Add("COMMANDTEXT", "R REPLACEDATA=YES");
                else
                    args.Add("COMMANDTEXT", "R REPLACEDATA=NO");
                args.Add("RRESULTS", result.ToString());
                this.Context.AnalysisCheckCodeInterface.Display(args);
            }
            else if (this.replaceData)
            {
                Dictionary<string, string> args = new Dictionary<string, string>();
                args.Add("COMMANDNAME", "R");
                args.Add("COMMANDTEXT", "R REPLACEDATA=YES");
                args.Add("RRESULTS", "<br>");
                this.Context.AnalysisCheckCodeInterface.Display(args);
            }
            else
            {
                Dictionary<string, string> args = new Dictionary<string, string>();
                args.Add("COMMANDNAME", "R");
                args.Add("COMMANDTEXT", "R REPLACEDATA=NO");
                args.Add("RRESULTS", "<br>");
                this.Context.AnalysisCheckCodeInterface.Display(args);
            }

            return result; // No need to return anything. Nothing is expected.
        }
    }
}
