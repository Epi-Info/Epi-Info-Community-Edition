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
    public partial class Rule_PythonCode : AnalysisRule
    {
        bool HasRun = false;
        List<string> IdentifierList = null;

        ProcessStartInfo processStartInfo = new ProcessStartInfo();

        bool isExceptionList = false;
        string commandText = string.Empty;
        string pythonPath = null;
        string dictListName = null;
        string pythonStatements = "";
        bool replaceData = false;

        public Rule_PythonCode(Rule_Context pContext, NonterminalToken pToken) : base(pContext)
        {
            this.IdentifierList = new List<string>();

            /*Programming_Chars             = ('>>> '{Valid Chars}*)
            
<Python_Statements> ::= PYTHON PYTHONPATH '=' String DATASET '=' String <Python_Code> END-PYTHON
       | PYTHON PYTHONPATH '=' String DATASET '=' String <Python_Code> END-PYTHON <Python_End_Options>

<Python_Code> ::= <Python_Code> <Python_Code_Line>
       | <Python_Code_Line>

<Python_Code_Line> ::= Programming_Chars
       | '\n'

<Python_End_Options> ::= REPLACEDATA '=' YES
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
                        case "<Python_Statements>":
                            break;
                        case "<Python_Code>":
                            this.SetPythonStatements(NT);
                            break;
                        case "<Python_Code_Line>":
                            //this.SetFrequencyOption(NT);
                            break;
                        case "<Python_End_Options>":
                            this.SetEndOption(NT);
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

        private void SetPythonStatements(NonterminalToken pToken)
        {
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


            // Write working data to JSON file in TEMP folder and then read with Python
            DataTable dt = this.Context.DataSet.Tables[2];
            string dtjson = JsonConvert.SerializeObject(dt, Newtonsoft.Json.Formatting.Indented);
            string tempPath = System.IO.Path.GetTempPath();
            System.IO.File.WriteAllText(tempPath + "WorkingEIDataJSON.json", dtjson);
            string consumejson = "import json\n";
            consumejson += this.dictListName + " = []\n";
            // Python needs the slashes escaped
            consumejson += "with open(\"" + tempPath.Replace("\\", "\\\\") + "WorkingEIDataJSON.json\", \"r\") as f:\n";
            consumejson += "    fread = f.read()\n";
            consumejson += "    " + this.dictListName + " = json.loads(fread)\n";

            // Write the Python data to JSON file in TEMP folder
            string writejson = "\n";
            if (this.replaceData)
            {
                writejson += "fwrite = json.dumps(" + this.dictListName + ", indent=2)\n";
                writejson += "with open(\"" + tempPath.Replace("\\", "\\\\") + "WrittenFromPythonEIDataJSON.json\", \"w\") as f:\n";
                writejson += "    f.write(fwrite)\n";
            }
            else
            {
                writejson = "";
            }

            // Here is where I tried to move an object from C# to Python environment
            // using a Memory Mapped File. I don't think this is the way but am leaving
            // the code in case I want to return to it.
            // consumejson = "import mmap\n";
            // consumejson += this.dictListName + " = []\n";
            // consumejson += "mmap_object = mmap.mmap(-1, 0, \"jsonfile\")\n";
            // consumejson += this.dictListName + " = mmap_object.read()\n";

            this.processStartInfo.FileName = this.pythonPath;
            this.processStartInfo.Arguments = "-c \"" +
                consumejson.Replace("\"", "\\\"") +
                this.pythonStatements.Replace("\"", "\\\"") +
                writejson.Replace("\"", "\\\"") +
                "\"";
            this.processStartInfo.UseShellExecute = false;
            this.processStartInfo.CreateNoWindow = true;
            this.processStartInfo.RedirectStandardOutput = true;
            this.processStartInfo.RedirectStandardError = true;
            this.processStartInfo.RedirectStandardInput = true;
            
            // Here is where I tried to move an object from C# to Python environment
            // using a Memory Mapped File. I don't think this is the way but am leaving
            // the code in case I want to return to it.
            // byte[] buffer = ASCIIEncoding.ASCII.GetBytes(dtjson);
            // MemoryMappedFile mmf0 = MemoryMappedFile.CreateNew("jsonfile", 10000000);
            // MemoryMappedViewAccessor accessor = mmf0.CreateViewAccessor();
            // accessor.Write(54, (ushort)buffer.Length);
            // accessor.WriteArray(54 + 2, buffer, 0, buffer.Length);
            //
            // MemoryMappedFile mmf = MemoryMappedFile.OpenExisting("jsonfile");
            // MemoryMappedViewAccessor accessor2 = mmf0.CreateViewAccessor();
            // ushort size = accessor2.ReadUInt16(54);
            // byte[] buffer2 = new byte[size];
            // accessor.ReadArray(54 + 2, buffer2, 0, buffer2.Length);
            // string mmfout = ASCIIEncoding.ASCII.GetString(buffer2);

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
                dtjson = System.IO.File.ReadAllText(tempPath + "WrittenFromPythonEIDataJSON.json");
                dt = (DataTable)JsonConvert.DeserializeObject<DataTable>(dtjson);
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
                args.Add("COMMANDNAME", "PYTHON");
                if (this.replaceData)
                    args.Add("COMMANDTEXT", "PYTHON REPLACEDATA=YES");
                else
                    args.Add("COMMANDTEXT", "PYTHON REPLACEDATA=NO");
                args.Add("PYTHONRESULTS", result.ToString());
                this.Context.AnalysisCheckCodeInterface.Display(args);
            }
            else if (this.replaceData)
            {
                Dictionary<string, string> args = new Dictionary<string, string>();
                args.Add("COMMANDNAME", "PYTHON");
                args.Add("COMMANDTEXT", "PYTHON REPLACEDATA=YES");
                args.Add("PYTHONRESULTS", "<br>");
                this.Context.AnalysisCheckCodeInterface.Display(args);
            }
            else
            {
                Dictionary<string, string> args = new Dictionary<string, string>();
                args.Add("COMMANDNAME", "PYTHON");
                args.Add("COMMANDTEXT", "PYTHON REPLACEDATA=NO");
                args.Add("PYTHONRESULTS", "<br>");
                this.Context.AnalysisCheckCodeInterface.Display(args);
            }

            return result; // No need to return anything. Nothing is expected.
        }
    }
}
