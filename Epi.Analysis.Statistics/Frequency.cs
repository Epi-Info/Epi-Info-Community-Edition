using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using EpiInfo.Plugin;
using Epi.Data;

namespace Epi.Analysis.Statistics
{
    public class Frequency : IAnalysisStatistic
    {
        string[] IdentifierList = null;
        string[] StratvarList = null;
        string OutTable = null;
        string WeightVar = null;
        string commandText = string.Empty;
        string CurrentRead_Identifier = string.Empty;
        IAnalysisStatisticContext Context;
        private Configuration config = null;

        static private System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<object>> PermutationList;
        static private System.Collections.Generic.List<string> SelectClauses;

        public Frequency(IAnalysisStatisticContext AnalysisStatisticContext) 
        {
            this.Construct(AnalysisStatisticContext);
        }

        public string Name { get { return "Epi.Analysis.Statistics.Frequency"; } }
        public void Construct(IAnalysisStatisticContext AnalysisStatisticContext)
        {
            this.Context = AnalysisStatisticContext;
            this.config = Configuration.GetNewInstance();
            

            this.IdentifierList = this.Context.InputVariableList["IdentifierList"].Split(',');
            for (int i = 0; i < this.IdentifierList.Length; i++)
            {
                this.IdentifierList[i] = this.IdentifierList[i].Trim(new char[] { '[', ']' });
            }

            if (this.Context.InputVariableList.ContainsKey("StratvarList"))
            {
                this.StratvarList = this.Context.InputVariableList["StratvarList"].Split(',');
                for (int i = 0; i < this.StratvarList.Length; i++)
                {
                    this.StratvarList[i] = this.StratvarList[i].Trim(new char[] { '[', ']' });
                }
            }
            this.OutTable = this.Context.InputVariableList["OutTable"];
            this.WeightVar = this.Context.InputVariableList["WeightVar"];
            this.commandText = this.Context.InputVariableList["commandText"];
            this.CurrentRead_Identifier = this.Context.InputVariableList["TableName"];
            this.commandText = this.Context.InputVariableList["CommandText"];

        }

        public void Execute()
        {
            Dictionary<string,string> config = this.Context.SetProperties;
            System.Collections.Generic.Dictionary<string, System.Data.DataTable> Freq_ListSet = new Dictionary<string, System.Data.DataTable>();
            
            DataTable DT = new DataTable();
            DT.CaseSensitive = true;
            DataTable OutDataTable = new DataTable();

            foreach (DataColumn column in this.Context.Columns)
            {
                DataColumn newColumn = new DataColumn(column.ColumnName);
                newColumn.DataType = column.DataType;
                DT.Columns.Add(newColumn);
            }

            // **** Get Participating Variables Start
            List<string> ParticipatingVariables = new List<string>();
            /*
            if (this.IdentifierList[0] == "*")
            {
                foreach (System.Data.DataColumn C in this.Context.Columns)
                {
                    //ParticipatingVariables.Add(C.ColumnName);
                }
            }
            else
            {
                foreach (string Key in IdentifierList)
                {
                    //ParticipatingVariables.Add(Key);
                }
            }*/

            if(!string.IsNullOrEmpty(this.WeightVar) && this.WeightVar != "")
            {
                ParticipatingVariables.Add(this.WeightVar);
            }

            if (this.StratvarList != null) // PATCH; improve later
            {
                foreach (string stratavar in this.StratvarList)
                {
                    ParticipatingVariables.Add(stratavar);
                }
            }

            // **** Get Participating Variables End
            foreach (DataRow row in this.Context.GetDataRows(ParticipatingVariables))
            {
                DT.ImportRow(row); 
            }

            Frequency.PermutationList = new Dictionary<string, List<object>>();
            CreatePermutaionList(this.StratvarList);


            List<string> RemoveList = new List<string>();
            if (!this.config.Settings.IncludeMissingValues)
            {
                foreach (string s in Frequency.SelectClauses)
                {
                    if (s.EndsWith(" is Null", StringComparison.OrdinalIgnoreCase))
                    {
                        RemoveList.Add(s);
                    }
                }
            }

            foreach (string s in RemoveList)
            {
                Frequency.SelectClauses.Remove(s);
            }

            StringBuilder HTMLString = new StringBuilder();
            System.Collections.Generic.Dictionary<string, string> KeyList = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(this.OutTable))
            {
                OutDataTable.TableName = this.OutTable;
                if (this.StratvarList != null)
                {
                    foreach (string stratavar in this.StratvarList)
                    {
                        DataColumn newColumn = new DataColumn(stratavar);
                        newColumn.DataType = this.Context.Columns[stratavar].DataType;
                        OutDataTable.Columns.Add(newColumn);
                    }
                }
                foreach (string freqitem in this.IdentifierList)
                {
                    DataColumn newColumn = new DataColumn(freqitem);
                    newColumn.DataType = this.Context.Columns[freqitem].DataType;
                    OutDataTable.Columns.Add(newColumn);
                }
                DataColumn varCol = new DataColumn("VARNAME");
                varCol.DataType = typeof(string);
                OutDataTable.Columns.Add(varCol);

                DataColumn percentCol = new DataColumn("PERCENT");
                percentCol.DataType = typeof(double);
                OutDataTable.Columns.Add(percentCol);

                DataColumn countCol = new DataColumn("COUNT");
                countCol.DataType = typeof(int);
                OutDataTable.Columns.Add(countCol);
            }

            foreach (string SelectClause in Frequency.SelectClauses)
            {
                Freq_ListSet.Clear();
                KeyList.Clear();

                if (this.IdentifierList[0] == "*")
                {
                    foreach (System.Data.DataColumn C in this.Context.Columns)
                    {
                        string Key = C.ColumnName;
                        if (!Freq_ListSet.ContainsKey(Key))
                        {
                            Freq_ListSet.Add(Key, CreateDataTable(Key));
                            KeyList.Add(Key, Key);
                        }
                    }
                }
                else
                {
                    foreach (string Key in IdentifierList)
                    {
                        if (!Freq_ListSet.ContainsKey(Key))
                        {
                            if (GetColumnDataType(Key) != null)
                            {
                            Freq_ListSet.Add(Key, CreateDataTable(Key));
                            KeyList.Add(Key, Key);
                            }
                        }
                    }
                }

                foreach (System.Data.DataRow R in DT.Select(SelectClause))
                {
                    foreach (System.Collections.Generic.KeyValuePair<string, System.Data.DataTable> KeyP in Freq_ListSet)
                    {
                        string Key = KeyP.Key;
                        foreach (System.Data.DataColumn C in DT.Columns)
                        {
                            string ColumnKey = C.ColumnName;
                            if (ColumnKey.ToUpperInvariant() == Key.ToUpperInvariant())
                            {
                                bool RowIsFound = false;

                                foreach (System.Data.DataRow R2 in KeyP.Value.Rows)
                                {
                                    if (R[C.ColumnName].ToString() == R2["value"].ToString())
                                    {
                                        RowIsFound = true;
                                        if (string.IsNullOrEmpty(this.WeightVar))
                                        {
                                            R2["count"] = int.Parse(R2["count"].ToString()) + 1;
                                        }
                                        else
                                        {
                                            double temp = 0.0;

                                            if (double.TryParse(R[this.WeightVar].ToString(), out temp))
                                            {
                                                R2["count"] = double.Parse(R2["count"].ToString()) + temp;
                                            }
                                        }
                                        break;
                                    }
                                }

                                if (!RowIsFound)
                                {
                                    System.Data.DataRow R3;
                                    if (string.IsNullOrEmpty(this.WeightVar))
                                    {
                                        R3 = Freq_ListSet[Key].NewRow();
                                        R3["value"] = R[C.ColumnName];
                                        R3["count"] = 1;
                                        if (this.StratvarList != null)
                                        {
                                            foreach (string strata in this.StratvarList)
                                            {
                                                R3[strata] = R[strata];
                                            }
                                        }
                                        KeyP.Value.Rows.Add(R3);
                                    }
                                    else
                                    {
                                        double temp = 0;
                                        if (double.TryParse(R[this.WeightVar].ToString(), out temp))
                                        {
                                            R3 = Freq_ListSet[Key].NewRow();
                                            R3["value"] = R[C.ColumnName];
                                            R3["count"] = temp;
                                            if (this.StratvarList != null)
                                            {
                                                foreach (string strata in this.StratvarList)
                                                {
                                                    R3[strata] = R[strata];
                                                }
                                            }
                                            KeyP.Value.Rows.Add(R3);
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                }

                string[] tmpString2 = null;
                tmpString2 = SelectClause.ToString().Split(new string[] { " AND " }, StringSplitOptions.None);
                bool appendWithAND = false;
                HTMLString.Append("<p><b>");
                if (!(tmpString2.Length == 1 && tmpString2[0] == ""))
                {
                    foreach (string tempString in tmpString2)
                    {
                        string[] tmpString = null;

                        tmpString = tempString.ToString().Split('=');
                        if (tmpString.Length == 1)
                        {
                            tmpString = tempString.ToString().Split(new string[] { " is " }, StringSplitOptions.None);
                        }
                        string newSelectClause = string.Empty;

                        bool variableExists = this.Context.EpiViewVariableList.ContainsKey(tmpString[0].Trim());
                        string dataType = DT.Columns[tmpString[0].Trim()].DataType.ToString();
                        bool isByte = dataType.Equals("System.Byte");
                        bool isBool = dataType.Equals("System.Boolean");

                        if (variableExists && (isByte || isBool))
                        {
                            if (tmpString[1].ToUpperInvariant().Contains("FALSE"))
                            {
                                tmpString[1] = this.config.Settings.RepresentationOfNo;
                            }
                            else if (tmpString[1].ToUpperInvariant().Contains("TRUE"))
                            {
                                tmpString[1] = this.config.Settings.RepresentationOfYes;
                            }

                            if (tmpString[1].Contains("1"))
                            {
                                tmpString[1] = this.config.Settings.RepresentationOfYes;
                            }
                            else if (tmpString[1].Contains("0"))
                            {
                                tmpString[1] = this.config.Settings.RepresentationOfNo;
                            }
                            else if (tmpString[1].ToLowerInvariant().Contains("null"))
                            {
                                tmpString[1] = this.config.Settings.RepresentationOfMissing;
                            }
                            newSelectClause = tmpString[0] + "=" + tmpString[1].ToString();
                        }
                        else
                        {
                            newSelectClause = tempString;
                        }

                        if (appendWithAND)
                        {
                            HTMLString.Append(" AND " + newSelectClause.Replace("''", "'"));
                        }
                        else
                        {
                            HTMLString.Append(newSelectClause.Replace("''", "'"));
                        }

                        appendWithAND = true;
                    }
                }
                HTMLString.Append("</b></p>");    

                foreach (System.Collections.Generic.KeyValuePair<string, System.Data.DataTable> Key in Freq_ListSet)
                {
                    double Mode = 0.0;
                    double ModeCount = 0.0;
                    double mean = 0.0;
                    double variance = 0.0;

                    System.Data.DataRow[] tempRows = Key.Value.Select("", "value");

                    if (config["include-missing"].ToUpperInvariant() == "FALSE")
                    {
                        if (Key.Value.Rows[0][0].GetType() == typeof(string))
                            tempRows = Key.Value.Select(string.Format(" varname='{0}' and [value] is NOT NULL and [value] <> '' ", Key.Key), "value");
                        else
                            tempRows = Key.Value.Select(string.Format(" varname='{0}' and [value] is NOT NULL ", Key.Key), "value");
                    }
                    else
                    {
                        tempRows = Key.Value.Select("", "value");
                    }

                    //var Rows = tempRows.OrderByDescending(item => item["count"]);

                    int n = 0;
                    double std_dev = 0.0;
                    double Sum = 0.0;
                    double Sum_Sqr = 0.0;

                    double Total = 0;
                    double Min = 0.0;
                    double Max = 0.0;

                    foreach (System.Data.DataRow R in tempRows)
                    {
                        double temp;
                        double.TryParse(R["count"].ToString(), out temp);
                        Total += temp;
                    }

                    foreach (System.Data.DataRow R in tempRows)
                    {
                        if (!string.IsNullOrEmpty(this.OutTable))
                        {
                            DataRow newRow = OutDataTable.NewRow();
                            if (this.StratvarList != null)
                            {
                                foreach (string stratavar in this.StratvarList)
                                {
                                    newRow[stratavar] = R[stratavar];
                                }
                            }
                            newRow[Key.Key] = R["value"];
                            newRow["VARNAME"] = Key.Key ;
                            newRow["PERCENT"] = (((double)R["count"]) / Total) * 100.0;
                            newRow["COUNT"] = R["count"];
                            OutDataTable.Rows.Add(newRow);
                        }
                    }

                    HTMLString.Append("<table cellpadding=\"2\">");
                    HTMLString.Append("<tr><th>");
                    if (this.Context.EpiViewVariableList.ContainsKey(Key.Key) && this.config.Settings.ShowCompletePrompt)
                        HTMLString.Append(this.Context.EpiViewVariableList[Key.Key].Prompt);
                    else
                        HTMLString.Append(Key.Key);
                    HTMLString.Append("</th><th>Frequency</th><th>Percent</th><th>Cum. Percent</th><th style=\"width:100px\">&nbsp;</th></tr>");
                    double AccumulatedTotal = 0;
                    List<ConfLimit> confLimits = new List<ConfLimit>();
                    int obs = 0;
                    foreach (System.Data.DataRow R in tempRows)
                    {
                        double x;

                        Double.TryParse(R["value"].ToString(), out x);

                        if (obs == 0)
                        {
                            Max = Min = x;
                        }
                        else
                        {
                            Max = x;
                        }

                        obs++;
                        n++;
                        Sum += x;
                        Sum_Sqr += x * x;

                        double currrentCount;
                        double.TryParse(R["count"].ToString(), out currrentCount);

                        if (currrentCount > ModeCount)
                        {
                            ModeCount = currrentCount;
                            Mode = x;
                        }

                        AccumulatedTotal += currrentCount;

                        HTMLString.Append("<tr>");
                        HTMLString.Append("<td><strong>");

                        if (Context.EpiViewVariableList.ContainsKey(Key.Key))
                        {
                            int dataTypeCode = Context.EpiViewVariableList[Key.Key].DataType.GetHashCode();
                            DataType dataType = (DataType)dataTypeCode;
                            GetPrintValue(Key.Key, R["value"], config, HTMLString, dataType);
                        }
                        else
                        { 
                            GetPrintValue(Key.Key, R["value"], config, HTMLString);
                        }

                        HTMLString.Append("</strong></td>");
                        HTMLString.Append("<td align=\"right\">");
                        HTMLString.Append(currrentCount.ToString());
                        HTMLString.Append("</td>");
                        HTMLString.Append("<td align=\"right\">");
                        HTMLString.Append(ConvertToPercent(currrentCount / Total));
                        HTMLString.Append("</td>");
                        HTMLString.Append("<td align=\"right\">");
                        HTMLString.Append(ConvertToPercent(AccumulatedTotal / Total));
                        HTMLString.Append("</td><td><div class=PercentBar_Summary style=\"width:" + ConvertToPixelLength(currrentCount / Total) + "\">&nbsp;</div></td>");
                        HTMLString.Append("</tr>");
                        confLimits.Add(GetConfLimit(GetPrintValue(Key.Key,R["value"], config), (float)currrentCount, (float)Total));
                    }

                    mean = Sum / n;
                    variance = (Sum_Sqr - Sum * mean) / (n - 1);
                    std_dev = calcStd_Dev(tempRows, mean);

                    HTMLString.Append("<tr>");
                    HTMLString.Append("<td><strong>Total</strong></td><td align=\"right\">");
                    HTMLString.Append(Total.ToString());
                    HTMLString.Append("</td><td align=\"right\">" + ConvertToPercent(1) + "</td><td align=\"right\">" + ConvertToPercent(1) + "</td><td><div class=PercentBar_Totals style=\"width:100%\">&nbsp;</div></td><tr>");

                    HTMLString.Append("</table>");

                    HTMLString.Append("<BR CLEAR=ALL/>");
                    if (Total < 300)
                        HTMLString.Append("<TABLE> <TD Class='Stats' ColSpan=\"3\"><B>Exact 95% Conf Limits</B></TD>");
                    else
                        HTMLString.Append("<TABLE> <TD Class='Stats' ColSpan=\"3\"><B>Wilson 95% Conf Limits</B></TD>");
                    foreach (ConfLimit cl in confLimits)
                    {
                        HTMLString.Append("<TR><TD Class='Stats'>" + cl.Value + "</TD><TD Class='Stats'>" + ConvertToPercent(cl.Lower) + "</TD><TD Class='Stats'>" + ConvertToPercent(cl.Upper) + "</TD></TR>");
                    }
                    HTMLString.Append("</TABLE>");
                }
            }
            if (!string.IsNullOrEmpty(this.OutTable))
            {
                this.Context.OutTable(OutDataTable);
            }

            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("COMMANDNAME", "FREQ");
            args.Add("COMMANDTEXT", commandText.Trim());
            args.Add("HTMLRESULTS", HTMLString.ToString());

            this.Context.Display(args);
        }

        private ConfLimit GetConfLimit(string value, int frequency, int count)
        {
            StatisticsRepository.cFreq freq = new StatisticsRepository.cFreq();
            double lower = 0;
            double upper = 0;
            if (frequency == count)
            {
                lower = 1;
                upper = 1;
                if (count < 300)
                {
                    lower = 0;
                    freq.ExactCI(frequency, (double)count, 95.0, ref lower, ref upper);
                    upper = 1;
                }
            }
            else
            {
                if (count >= 300)
                {
                    freq.WILSON(frequency, (double)count, 1.96, ref lower, ref upper);
                }
                else
                {
                    freq.ExactCI(frequency, (double)count, 95.0, ref lower, ref upper);
                }
            }
            ConfLimit cl = new ConfLimit();
            cl.Lower = lower;
            cl.Upper = upper;
            cl.Value = value;
            return cl;
        }

        private ConfLimit GetConfLimit(string value, float frequency, float count)
        {
            StatisticsRepository.cFreq freq = new StatisticsRepository.cFreq();
            double lower = 0;
            double upper = 0;
            if (frequency == count)
            {
                lower = 1;
                upper = 1;
                if (count < 300)
                {
                    lower = 0;
                    freq.ExactCI(frequency, (double)count, 95.0, ref lower, ref upper);
                    upper = 1;
                }
            }
            else
            {
                if (count >= 300)
                {
                    freq.WILSON(frequency, (double)count, 1.96, ref lower, ref upper);
                }
                else
                {
                    freq.ExactCI(frequency, (double)count, 95.0, ref lower, ref upper);
                }
            }
            ConfLimit cl = new ConfLimit();
            cl.Lower = lower;
            cl.Upper = upper;
            cl.Value = value;
            return cl;
        }

        private struct ConfLimit
        {
            public string Value;
            public double Upper;
            public double Lower;
        }

        private double calcStd_Dev(System.Data.DataRow[] pRows, double pMean)
        {
            int n = 0;
            double Sum = 0.0;

            foreach (System.Data.DataRow R in pRows)
            {
                n++;
                double x = 0.0;
                Double.TryParse(R["value"].ToString(), out x);
                Sum += Math.Pow(x - pMean, 2);
            }

            return Math.Sqrt(Sum / n);
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
            string format = "{0: ##0";

            if (config.Settings.PrecisionForStatistics == 0)
            {
                format = format + "}";
            }
            else
            {
                format = format + ".";
                for (int i = 0; i < config.Settings.PrecisionForStatistics; i++)
                {
                    format = format + "0";
                }
                format = format + "}";
            }
            if (config.Settings.ShowPercents)
            {
                format = format + "%";
            }

            return string.Format(format, (100.0 * pValue));
        }

        private string ConvertToPixelLength(double pValue)
        {
            return string.Format("{0: ##0}px", 1 * Math.Round((100.0 * pValue), MidpointRounding.AwayFromZero));
        }

        private System.Data.DataTable CreateDataTable(string pVariable)
        {
            System.Data.DataTable result = new System.Data.DataTable();
            System.Data.DataColumn NC = null;

            
            if (this.StratvarList != null)
            {
                foreach (string addcol in this.StratvarList)
                {
                    NC = new System.Data.DataColumn(addcol);
                    NC.DataType = GetColumnDataType(addcol);
                    result.Columns.Add(NC);
                }
            }

            NC = new System.Data.DataColumn("value");
            NC.DataType = GetColumnDataType(pVariable);
            result.Columns.Add(NC);
            NC = new System.Data.DataColumn("count");
            
            NC.DataType = typeof(double);
            result.Columns.Add(NC);
            NC = new System.Data.DataColumn("varname");
            NC.DataType = typeof(string);
            NC.DefaultValue = pVariable;
            result.Columns.Add(NC);

            return result;
        }

        private System.Type GetColumnDataType(string pColumnName)
        {
            System.Type result = null;

            foreach (System.Data.DataColumn column in this.Context.Columns)
            {
                if (column.ColumnName.Trim().ToUpperInvariant() == pColumnName.Trim().ToUpperInvariant())
                {
                    result = column.DataType;
                    break;
                }
            }

            return result;
        }

        private void CreatePermutaionList(string[] pStratvarList)
        {
            int i = 0;
            Frequency.SelectClauses = new List<string>();

            if (pStratvarList != null)
            {
                List<DataRow> Rows = this.Context.GetDataRows(null);

                foreach (string StratVar in pStratvarList)
                {
                    Frequency.PermutationList.Add(StratVar, new List<object>());
                }

                foreach (DataRow R in Rows)
                {
                    foreach (string StratVar in pStratvarList)
                    {
                        bool isFound = false;
                        for (i = 0; i < Frequency.PermutationList[StratVar].Count; i++)
                        {
                            if (CompareEqual(Frequency.PermutationList[StratVar][i], R[StratVar]))
                            {
                                isFound = true;
                                break;
                            }
                        }

                        if (!isFound)
                        {
                            Frequency.PermutationList[StratVar].Add(R[StratVar]);
                        }
                    }
                }

                EnumerablePermuter_Freq EP = new EnumerablePermuter_Freq(this.Context.Columns, EnumerablePermuter_Freq.RunModeEnum.SelectClauses, ref this.StratvarList, ref Frequency.SelectClauses);

                System.Collections.IEnumerable[] PL = new System.Collections.IEnumerable[Frequency.PermutationList.Count];
                i = 0;
                foreach (KeyValuePair<string, List<object>> Key in Frequency.PermutationList)
                {
                    PL[i++] = Key.Value;
                }
                
                EP.VisitAll(PL);
            }

            if (Frequency.SelectClauses.Count == 0)
            {
                Frequency.SelectClauses.Add("");
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
                result = result && pR[Name[i]] == Frequency.PermutationList[Name[i]][Value[i]];
            }

            return result;
        }

        private void GetPrintValue(string pFieldName, object pValue, Dictionary<string, string> pConfig, StringBuilder pBuilder, DataType? epiDataType = null)
        {
            string format = string.Empty;

            if (pValue == DBNull.Value)
            {
                pBuilder.Append(pConfig["RepresentationOfMissing"]);
            }
            else switch (Type.GetTypeCode(pValue.GetType()))
                {
                    case TypeCode.Byte:
                        if (this.Context.EpiViewVariableList.ContainsKey(pFieldName))
                        {
                            int val = 0;
                            if (int.TryParse(pValue.ToString(), out val))
                            {
                                pBuilder.Append((val != 0 ? pConfig["RepresentationOfYes"] : pConfig["RepresentationOfNo"]));
                            }
                            else
                            {
                                pBuilder.Append(pValue);
                            }
                        }
                        else
                        {
                            pBuilder.Append(pValue);
                        }
                        break;
                    case TypeCode.Boolean:
                        pBuilder.Append((Convert.ToBoolean(pValue) ? pConfig["RepresentationOfYes"] : pConfig["RepresentationOfNo"]));
                        break;
                    case TypeCode.Double:
                        pBuilder.Append(string.Format("{0:0.##########}", pValue));
                        break;
                    case TypeCode.DateTime:
                        switch (epiDataType)
                        { 
                            case DataType.Date:
                                string appendDate = ((DateTime)pValue).ToShortDateString();
                                pBuilder.Append(appendDate);
                                break;
                            case DataType.Time:
                                string appendTime = ((DateTime)pValue).ToShortTimeString();
                                pBuilder.Append(appendTime);
                                break;
                            case DataType.DateTime:
                            default:
                                string appendDateTime = ((DateTime)pValue).ToString();
                                pBuilder.Append(appendDateTime);
                                break;
                        }
                        break;
                    
                    default:
                        pBuilder.Append(pValue);
                        break;
                }
        }


        private string GetPrintValue(string pFieldName, object pValue, Dictionary<string, string> pConfig)
        {
            string result = null;

            if (pValue == DBNull.Value)
            {
                result = pConfig["RepresentationOfMissing"];
            }
            else switch (pValue.GetType().Name)
                {
                    case "Byte":
                        if (this.Context.EpiViewVariableList.ContainsKey(pFieldName))
                        {
                            int val = 0;
                            if (int.TryParse(pValue.ToString(), out val))
                            {
                                result = (val != 0 ? pConfig["RepresentationOfYes"] : pConfig["RepresentationOfNo"]);
                            }
                            else
                            {
                                result = pValue.ToString();
                            }
                        }
                        else
                        {
                            result = pValue.ToString();
                        }
                        break;
                    case "Boolean":
                        result = (Convert.ToBoolean(pValue) ? pConfig["RepresentationOfYes"] : pConfig["RepresentationOfNo"]);
                        break;
                    case "Double":
                        result = string.Format("{0:0.##########}", pValue);
                        break;
                    default:
                        result = pValue.ToString();
                        break;
                }

            return result;
        }
    }

    public class EnumerablePermuter_Freq
    {

        public EnumerablePermuter_Freq(DataColumnCollection pContext, RunModeEnum pRunMode, ref string[] pStratavarList, ref List<string> pSelectClauses)
        {
            this.RunMode = pRunMode;
            this.StrataVarList = pStratavarList;
            this.SelectClaues = pSelectClauses;
            this.Columns = pContext;
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
                        
                        val = a[j].Current;
                        if (val == DBNull.Value)
                        {
                            columnName.Append(" is ");
                            columnName.Append("Null");
                        }
                        else
                        {
                            columnName.Append("=");
                            columnName.Append(val);
                        }
                        columnName.Append(" ");
                        break;
                    case "System.String":
                        val = a[j].Current;
                        if (val == DBNull.Value)
                        {
                            columnName.Append(" is ");
                            columnName.Append("Null");
                        }
                        else
                        {
                            columnName.Append("=");
                            columnName.Append("'");
                            columnName.Append(((string)val).Replace("'","''"));
                            columnName.Append("'");
                        }
                        break;
                    default:
                        val = a[j].Current;
                        if (val == DBNull.Value)
                        {
                            columnName.Append(" is ");
                            columnName.Append("Null");
                        }
                        else
                        {
                            columnName.Append("=");
                            columnName.Append("'");
                            columnName.Append(val);
                            columnName.Append("'");
                        }
                        
                        break;
                }
            }

            bool isFound = false;
            string TestValue = columnName.ToString();
            for (int k = 0; k < this.SelectClaues.Count; k++)
            {
                if (this.SelectClaues[k] == TestValue)
                {
                    isFound = true;
                    break;
                }
            }
            if (!isFound)
            {
                this.SelectClaues.Add(TestValue);
            }
        }

        private void VisitCurrentRow(System.Collections.IEnumerator[] a)
        {
            string head = "c";
            for (int j = 1; j < a.Length; j++)
            {
                System.Console.Write("{0}{1}_", head, a[j].Current);
            }
            System.Console.WriteLine();
        }

        private void Visit(System.Collections.IEnumerator[] a)
        {
            string head = "c";
            for (int j = 1; j < a.Length; j++)
            {
                System.Console.Write("{0}{1}_", head, a[j].Current);
            }
            System.Console.WriteLine();
        }

        public void VisitAll(params System.Collections.IEnumerable[] m)
        {
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

            for (;;)
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

                j = n;

                // carry if necessary
                while (j >= 0 && !a[j].MoveNext())
                {
                    a[j].Reset();
                    a[j].MoveNext();
                    j -= 1;
                }

                // increase unless done
                if (j <= 0)
                {
                    break; // Terminate the algorithm
                }
            }
        }
    }
}
