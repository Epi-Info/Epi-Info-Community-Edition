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
        string[] StratvarList = null;
        string OutTable = null;
        string WeightVar = null;
        string commandText = string.Empty;
        string PSUVar = null;
        string pythonPath = null;
        string dictListName = null;
        string pythonStatements = "";

        Dictionary<string, string> SetOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        static private System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<object>> PermutationList;
        static private System.Collections.Generic.List<string> SelectClauses;

        EpiInfo.Plugin.IAnalysisStatistic FrequencyStatistic;

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
                }
            }
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
            /*
            WEIGHTVAR '=' Identifier
            | STRATAVAR '=' <Freq_Variable_List> 
            |NOWRAP
            |COLUMNSIZE '=' DecLiteral
            |PSUVAR '=' Identifier
            | OUTTABLE '=' Identifier
            | <SetClause>
            */


            switch (pToken.Rule.Rhs[0].ToString())
            {
                case "STRATAVAR":
                    this.StratvarList = AnalysisRule.SpliIdentifierList(this.GetCommandElement(pToken.Tokens, 2).Trim());
                        break;
                case "WEIGHTVAR":
                        this.WeightVar = this.GetCommandElement(pToken.Tokens, 2).Trim(new char[] { '[',']'});
                        break;
                case "OUTTABLE":
                        this.OutTable = this.GetCommandElement(pToken.Tokens, 2).Trim(new char[] { '[', ']' });
                    break;
                case "PSUVAR":
                    this.PSUVar = this.GetCommandElement(pToken.Tokens, 2).Trim(new char[] { '[', ']' });
                    break;
                case "<SetClause>":
                    this.SetSetClause((NonterminalToken) pToken.Tokens[0]);
                    break;
            }
            
        }

        private void SetSetClause(NonterminalToken pToken)
        {
            /*           <SetClause> ::=  STATISTICS '=' <StatisticsOption>	!These options could be set in FREQ,MATCH, and MEANS commands also
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
                    | USEBROWSER '=' <OnOff>*/
            string Key = this.GetCommandElement(pToken.Tokens, 0);
            string Value = null;
            Token T = pToken.Tokens[2];
            if (T is NonterminalToken)
            {
                Value = this.ExtractTokens(((NonterminalToken)T).Tokens).Trim();
            }
            else
            {
                Value = this.GetCommandElement(pToken.Tokens, 2).Trim();
            }
            if (this.SetOptions.ContainsKey(Key))
            {
                this.SetOptions[Key] = Value.Trim('"');
            }
            else
            {
                this.SetOptions.Add(Key, Value.Trim('"'));
            }
        }

        private struct ConfLimit
        {
            public string Value;
            public double Upper;
            public double Lower;
        }




        /// <summary>
        /// performs execution of the Freq(uency) command
        /// </summary>
        /// <returns>object</returns>
        public override object Execute()
        {
            object result = null;
            return result; // Need to figure out what Execute should be. And also what to do with the tokens.

            if (!this.HasRun)
            {
                this.Context.AddConfigSettings(this.SetOptions);

                Dictionary<string, string> setProperties = this.Context.GetGlobalSettingProperties();
                Dictionary<string, string> inputVariableList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                //bool isExceptionList = false;

                this.Context.ExpandGroupVariables(this.IdentifierList, ref isExceptionList);

                StringBuilder sb = new StringBuilder();
                foreach (string s in IdentifierList)
                {
                    sb.Append(s);
                    sb.Append(",");
                }
                sb.Length = sb.Length - 1;

                inputVariableList.Add("IdentifierList", sb.ToString());

                if (StratvarList != null)
                {
                    sb.Length = 0;

                    foreach (string s in StratvarList)
                    {
                        sb.Append(s);
                        sb.Append(",");
                    }
                    sb.Length = sb.Length - 1;

                    inputVariableList.Add("StratvarList", sb.ToString());
                }

                inputVariableList.Add("OutTable", OutTable);
                inputVariableList.Add("WeightVar", WeightVar);
                inputVariableList.Add("commandText", commandText);
                inputVariableList.Add("PSUVar", PSUVar);
                if (this.Context.CurrentRead == null)
                {
                    inputVariableList.Add("TableName", "");
                }
                else
                {
                    inputVariableList.Add("TableName", this.Context.CurrentRead.Identifier);
                }

                EpiInfo.Plugin.IDataSource DataSource = this.Context.GetDefaultIDataSource();

               // Context.DataTableRefreshNeeded = true;

                AnalysisStatisticExecuteHost statisticHost = new AnalysisStatisticExecuteHost(this.Context, setProperties, DataSource, inputVariableList, this.Context.CurrentSelect.ToString(), this.Context.AnalysisInterpreterHost);

                if (string.IsNullOrEmpty(this.PSUVar))
                {
                    this.FrequencyStatistic = this.Context.GetStatistic("Frequency", statisticHost);
                }
                else
                {
                    this.FrequencyStatistic = this.Context.GetStatistic("ComplexSampleTables", statisticHost);
                }

                this.FrequencyStatistic.Execute();
                this.FrequencyStatistic = null;
                this.HasRun = true;
            }

            return result;
        }



        private double calcStd_Dev(System.Data.DataRow[] pRows, double pMean)
        {
            double result = 0.0;

            //sqrt(sum(((obs.value - mean)^2))/numberOfObs)
            int n = 0;
            double Sum = 0.0;

            foreach (System.Data.DataRow R in pRows)
            {
                n++;
                double x = 0.0;
                Double.TryParse(R["value"].ToString(), out x);

                Sum += Math.Pow(x - pMean, 2);
            }

            result = Math.Sqrt(Sum / n);

            return result;
        }

        private double calcVariance(System.Data.DataTable pDT)
        {
            int n = 0;
            double Sum = 0.0;
            double Sum_Sqr = 0.0;

            foreach (System.Data.DataRow R in pDT.Rows)
            {
                n++;
                double x = 0.0;
                Double.TryParse(R["value"].ToString(), out x);

                Sum += x;
                Sum_Sqr += x * x;
            }

            double mean = Sum / n;
            double variance = (Sum_Sqr - Sum * mean) / (n - 1);
            return variance;
        }

        private string ConvertToPercent(double pValue)
        {
            return string.Format("{0: ##0.0}%", (100.0 * pValue));
        }

        private string ConvertToPixelLength(double pValue)
        {
            return string.Format("{0: ##0}px", 1 * Math.Round((100.0 * pValue), MidpointRounding.AwayFromZero));
        }

        private System.Type GetColumnDataType(string pColumnName)
        {
            System.Type result = null;

            foreach (System.Data.DataColumn C in this.Context.DataSet.Tables["Output"].Columns)
            {
                if (C.ColumnName.ToUpperInvariant() == pColumnName.ToUpperInvariant())
                {
                    result = C.DataType;
                    break;
                }
            }

            return result;
        }

        private System.Data.DataTable CreateDataTable(string pVariable)
        {
            System.Data.DataTable result = new System.Data.DataTable();
            System.Data.DataColumn NC = null;

            NC = new System.Data.DataColumn("value");
            NC.DataType = GetColumnDataType(pVariable);
            result.Columns.Add(NC);
            NC = new System.Data.DataColumn("count");
            NC.DataType = typeof(int);
            result.Columns.Add(NC);
            NC = new System.Data.DataColumn("varname");
            NC.DataType = typeof(string);
            NC.DefaultValue = pVariable;
            result.Columns.Add(NC);

            return result;
        }

        private void CreatePermutaionList(string[] pStratvarList)
        {
            int i = 0;

            Rule_PythonCode.SelectClauses = new List<string>();

            if (pStratvarList != null)
            {
                List<DataRow> Rows = this.Context.GetOutput();

                foreach (string StratVar in pStratvarList)
                {
                    Rule_PythonCode.PermutationList.Add(StratVar, new List<object>());
                }

                foreach (DataRow R in Rows)
                {
                    foreach (string StratVar in pStratvarList)
                    {
                        bool isFound = false;
                        for (i = 0; i < Rule_PythonCode.PermutationList[StratVar].Count; i++)
                        {
                            if (CompareEqual(Rule_PythonCode.PermutationList[StratVar][i], R[StratVar]))
                            {
                                isFound = true;
                                break;
                            }
                        }

                        if (!isFound)
                        {
                            Rule_PythonCode.PermutationList[StratVar].Add(R[StratVar]);
                        }
                    }
                }

                EnumerablePermuter_Freq EP = new EnumerablePermuter_Freq(this.Context.DataSet.Tables["Output"].Columns, EnumerablePermuter_Freq.RunModeEnum.SelectClauses, ref this.StratvarList, ref Rule_PythonCode.SelectClauses);

                System.Collections.IEnumerable[] PL = new System.Collections.IEnumerable[Rule_PythonCode.PermutationList.Count];
                i = 0;
                foreach (KeyValuePair<string, List<object>> Key in Rule_PythonCode.PermutationList)
                {
                    PL[i++] = Key.Value;
                }
                EP.VisitAll(PL);

            }

            if (Rule_PythonCode.SelectClauses.Count == 0)
            {
                Rule_PythonCode.SelectClauses.Add("");
            }

        }

        private bool CompareEqual(object A, object B)
        {
            if (A == DBNull.Value && B == DBNull.Value)
            {
                return true;
            }

            if (A == DBNull.Value || B == DBNull.Value)
            {
                return false;
            }

            return A.Equals(B);
        }

        private bool DelegateFunction(System.Data.DataRow pR, string pFilterString)
        {
            bool result = true;

            // FilterString name1=0; name2=1
            string[] FS = pFilterString.Split(';');
            List<string> Name = new List<string>();
            List<int> Value = new List<int>();

            for (int i = 0; i < FS.Length; i++)
            {
                string[] temp = FS[i].Split('=');
                Name.Add(temp[0]);
                Value.Add(int.Parse(temp[1]));
            }

            for (int i = 0; i < Name.Count; i++)
            {
                result = result && pR[Name[i]] == Rule_PythonCode.PermutationList[Name[i]][Value[i]];
            }

            return result;
        }



        private void GetPrintValue(object pValue, Configuration pConfig, StringBuilder pBuilder)
        {
            if (pValue == DBNull.Value)
            {
                pBuilder.Append(pConfig.Settings.RepresentationOfMissing);
            }
            else switch (pValue.GetType().Name)
            {
                case "Byte":
                    pBuilder.Append((Convert.ToBoolean(pValue) ? pConfig.Settings.RepresentationOfYes : pConfig.Settings.RepresentationOfNo));
                    break;
                case "Double":
                    pBuilder.Append(string.Format("{0:#.##}", pValue));
                    break;
                default:
                    pBuilder.Append(pValue);
                    break;
            }
        }
    }

 

    public class EnumerablePermuter_Python
    {

        public EnumerablePermuter_Python(DataColumnCollection DataColumns, RunModeEnum pRunMode, ref string[] pStratavarList, ref List<string> pSelectClauses)
        {
            this.RunMode = pRunMode;
            this.StrataVarList = pStratavarList;
            this.SelectClaues = pSelectClauses;
            this.Columns = DataColumns;
        }

        public enum RunModeEnum
        {
            Console,
            SelectClauses,
            CurrentRow
        }

        public RunModeEnum RunMode = RunModeEnum.SelectClauses;
        private System.Data.DataColumnCollection Columns = null;
        private string[] StrataVarList = null;
        private List<string> SelectClaues = null;
        private void VisitTableColumns(System.Collections.IEnumerator[] a)
        {
            System.Text.StringBuilder columnName = new StringBuilder();
            for (int j = 1; j < a.Length; j++)
            {
                if (j != 1)
                {
                    columnName.Append(" AND ");
                }
                columnName.Append(this.StrataVarList[j - 1]);
                object val = null;
                switch (this.Columns[this.StrataVarList[j - 1]].DataType.ToString())
                {
                    case "System.Single":
                    case "Sytem.Int32":
                    case "System.Double":
                    case "System.Boolean":
                    case "System.Decimal":
                        columnName.Append("=");
                        val = a[j].Current;
                        if (val == DBNull.Value)
                        {
                            columnName.Append("Null");
                        }
                        else
                        {
                            columnName.Append(val);
                        }
                        columnName.Append(" ");
                        break;
                    default:
                        columnName.Append("='");
                        val = a[j].Current;
                        if (val == DBNull.Value)
                        {
                            columnName.Append("Null");
                        }
                        else
                        {
                            columnName.Append(val);
                        }
                        columnName.Append("' ");
                        break;
                }


            }

            this.SelectClaues.Add(columnName.ToString());

            //System.Console.WriteLine(columnName.ToString());

        }

        private void VisitCurrentRow(System.Collections.IEnumerator[] a)
        {
            string head = "c";
            for (int j = 1; j < a.Length; j++)
            {
                System.Console.Write("{0}{1}_", head, a[j].Current);
                //head = "";
            }
            System.Console.WriteLine();
        }


        private void Visit(System.Collections.IEnumerator[] a)
        {
            string head = "c";
            for (int j = 1; j < a.Length; j++)
            {
                System.Console.Write("{0}{1}_", head, a[j].Current);
                //head = "";
            }
            System.Console.WriteLine();
        }

        public void VisitAll(params System.Collections.IEnumerable[] m)
        {

            // initialize
            int n = m.Length;
            int j;
            System.Collections.IEnumerator[] a = new System.Collections.IEnumerator[n + 1];

            for (j = 1; j <= n; j++)
            {
                a[j] = m[j - 1].GetEnumerator();
                a[j].MoveNext();
            }

            a[0] = m[0].GetEnumerator();
            a[0].MoveNext();

            for (; ; )
            {
                switch (this.RunMode)
                {
                    case RunModeEnum.SelectClauses:
                        VisitTableColumns(a);
                        break;

                    case RunModeEnum.CurrentRow:
                        VisitCurrentRow(a);
                        break;

                    case RunModeEnum.Console:
                    default:
                        Visit(a);
                        break;
                }

                // prepare to add one
                j = n;

                // carry if necessary
                while (!a[j].MoveNext())
                {
                    a[j].Reset();
                    a[j].MoveNext();
                    j -= 1;
                }

                // increase unless done
                if (j == 0)
                {
                    break; // Terminate the algorithm
                }

            }
        }
    }
}
