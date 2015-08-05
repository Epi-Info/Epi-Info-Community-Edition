using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using EpiInfo.Plugin;
using Epi.Data;

namespace Epi.Analysis.Statistics
{
    public class Means : IAnalysisStatistic
    {
        string Numeric_Variable = null;
        string Cross_Tabulation_Variable = null;
        string OutTable = null;
        string[] StratvarList = null;
        string WeightVar = null;
        string commandText = string.Empty;
        private double unweightedObservations = 0;
        private double grandObservations = 0;
        private double grandSum = 0;
        private double grandRecordCount = 0;
        private List<double> observationsList = new List<double>();
        private List<double> unweightedObservationsList = new List<double>();
        private List<double> averagesList = new List<double>();
        private List<double> variancesList = new List<double>();
        private int crosstabs = 0;
        private Configuration config = null;

        private List<string> NumericTypeList = new List<string>(new string[] { "INT", "FLOAT", "INT16", "INT32", "INT64", "SINGLE", "DOUBLE", "BYTE", "DECIMAL" });

        IAnalysisStatisticContext Context;

        static private System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<object>> PermutationList;
        static private System.Collections.Generic.List<string> SelectClauses;


        public Means(IAnalysisStatisticContext AnalysisStatisticContext) 
        {
            this.Construct(AnalysisStatisticContext);
        
        }

        public string Name { get { return "Epi.Analysis.Statistics.Means"; } }
        public void Construct(IAnalysisStatisticContext AnalysisStatisticContext)
        {
            this.Context = AnalysisStatisticContext;
            this.config = Configuration.GetNewInstance();

            this.Numeric_Variable = this.Context.InputVariableList["Numeric_Variable"].Trim(new char[] { '[', ']' });

            if (this.Context.InputVariableList["Cross_Tabulation_Variable"] != null)
            {
                this.Cross_Tabulation_Variable = this.Context.InputVariableList["Cross_Tabulation_Variable"].Trim(new char[] { '[', ']' });
            }

            if (this.Context.InputVariableList.ContainsKey("StratvarList"))
            {
                this.StratvarList = this.Context.InputVariableList["StratvarList"].Split(',');
                for (int i = 0; i < this.StratvarList.Length; i++)
                {
                    this.StratvarList[i] = this.StratvarList[i].Trim(new char[] { '[', ']' });
                }
            }
            if (this.Context.InputVariableList.ContainsKey("OutTable"))
            {
                this.OutTable = this.Context.InputVariableList["OutTable"];
            }
            if (this.Context.InputVariableList.ContainsKey("WeightVar"))
            {
                this.WeightVar = this.Context.InputVariableList["WeightVar"];
            }
            this.commandText = this.Context.InputVariableList["commandText"];

            this.commandText = this.Context.InputVariableList["CommandText"];
        }


        //MEANS AGE CABBAGESAL STRATAVAR= BROWNBREAD CAKES CODE_RW  SEX WEIGHTVAR=ILL
        //MEANS AGE CABBAGESAL STRATAVAR= BAKEDHAM CHOCOLATE COFFEE WEIGHTVAR=ILL


        //private double CalculateKruskalWallisH(DataTable freqHorizontal, DataTable freqVertical, List<List<int>> allLocalFreqs, int recordCount)
        //{
        //    double cf = 0;
        //    double avgr = 0;
        //    int greaterSize;
        //    if (freqHorizontal.Rows.Count > freqVertical.Rows.Count)
        //        greaterSize = freqHorizontal.Rows.Count;
        //    else
        //        greaterSize = freqVertical.Rows.Count;
        //    double[] sr = new double[greaterSize];
        //    double adj = 0;
        //    double H = 0;

        //    for (int i = 0; i < allLocalFreqs.Count; i++)
        //    {
        //        int totalHFreq = (int)freqHorizontal.Rows[i]["freq"];
        //        cf += totalHFreq;
        //        avgr = cf - (totalHFreq - 1) / 2.0;
        //        for (int l = 0; l < allLocalFreqs[0].Count; l++)
        //        {
        //            sr[l] += allLocalFreqs[i][l] * avgr;
        //        }
        //        adj += totalHFreq * (Math.Pow(totalHFreq, 2) - 1);
        //    }

        //    for (int i = 0; i < freqVertical.Rows.Count; i++)
        //    {
        //        int totalVFreq = (int)freqVertical.Rows[i]["freq"];
        //        if (totalVFreq != 0)
        //        {
        //            H += sr[i] * sr[i] / totalVFreq;
        //        }
        //    }

        //    H = H * 12 / (recordCount * (recordCount + 1)) - 3 * (recordCount + 1);
        //    H = H / (1 - adj / (Math.Pow(recordCount, 3) - recordCount));
            
        //    return H;
        //}

        /// <summary>
        /// CalculateKruskalWallisH
        /// </summary>
        /// <param name="freqHorizontal"></param>
        /// <param name="freqVertical"></param>
        /// <param name="allLocalFreqs"></param>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        private double CalculateKruskalWallisH(DataTable freqHorizontal, DataTable freqVertical, List<List<object>> allLocalFreqs, double recordCount)
        {
            double cf = 0;
            double avgr = 0;
            int greaterSize;
            if (freqHorizontal.Rows.Count > freqVertical.Rows.Count)
                greaterSize = freqHorizontal.Rows.Count;
            else
                greaterSize = freqVertical.Rows.Count;
            double[] sr = new double[greaterSize];
            double adj = 0;
            double H = 0;

            for (int i = 0; i < allLocalFreqs.Count; i++)
            {
                double totalHFreq = (double)freqHorizontal.Rows[i]["freq"];
                cf += totalHFreq;
                avgr = cf - (totalHFreq - 1) / 2.0;
                for (int l = 0; l < allLocalFreqs[0].Count; l++)
                {
                    sr[l] += (double)allLocalFreqs[i][l] * avgr;
                }
                adj += totalHFreq * (Math.Pow(totalHFreq, 2) - 1);
            }

            for (int i = 0; i < freqVertical.Rows.Count; i++)
            {
                double totalVFreq = (double)freqVertical.Rows[i]["freq"];
                if (totalVFreq != 0)
                {
                    H += sr[i] * sr[i] / totalVFreq;
                }
            }

            H = H * 12 / (recordCount * (recordCount + 1)) - 3 * (recordCount + 1);
            H = H / (1 - adj / (Math.Pow(recordCount, 3) - recordCount));

            return H;
        }

        private double CalculateChiSquare(double dfWithin, double pooledVariance, List<double> freqs, List<double> vars)
        {
            double denominator = 0;
            double result = 0;

            for (int j = 0; j < freqs.Count; j++)
            {
                if ((freqs[j] - 1 != 0) && (vars[j] != 0))
                {
                    denominator += 1.0 / (freqs[j] - 1);
                    result += (freqs[j] - 1) * Math.Log(vars[j]);
                }
                else
                {
                    denominator = 0;
                    result = 0;
                }
            }

            denominator = 1.0 + (1.0 / (3.0 * (freqs.Count - 1))) * (denominator - 1.0 / dfWithin);

            if ((denominator != 0) && (pooledVariance != 0))
            {
                result = (1.0 / denominator) * (dfWithin * Math.Log(pooledVariance) - result);
            }

            return result;
        }

        private double CalculateSSBetween(double grandMean, List<double> freqs, List<double> avgs)
        {
            double retval = 0;
            for (int x = 0; x < freqs.Count; x++)
            {
                retval += freqs[x] * (Math.Pow(avgs[x] - grandMean, 2));
            }
            return retval;
        }

        private double CalculateSSWithin(List<double> freqs, List<double> vars)
        {
            double retval = 0;
            for (int x = 0; x < freqs.Count; x++)
            {
                retval += (freqs[x] - 1) * vars[x];
            }
            return retval;
        }

        /// <summary>
        /// performs execution of the MEANS command
        /// </summary>
        /// <returns>object</returns>
        public void Execute()
        {
            /*
            Produce Table with rows = number of distinct values in main_variable
	            if no cross _tab_variable then columns = 2
	            else columns = 2 + number of values in cross_tab_variable

            For each strata 
	            get distinct values of the strata
	            for each distinct value
		            produce the following table
            */


            

            Dictionary<string, string> config = this.Context.SetProperties;
            StringBuilder HTMLString = new StringBuilder();

            System.Data.DataTable DT = new DataTable();
            foreach (DataColumn column in this.Context.Columns)
            {
                DataColumn newColumn = new DataColumn(column.ColumnName);
                newColumn.DataType = column.DataType;
                DT.Columns.Add(newColumn);
            }
            List<string> VariableList = new List<string>();
            VariableList.Add(this.Numeric_Variable);
            foreach (DataRow row in this.Context.GetDataRows(VariableList))
            {
                DT.ImportRow(row);
            }

            string StrataVarName = null;
            string StrataVar_Value = null;
            List<string> StrataVarNameList = new List<string>();
            List<string> StrataVarValueList = new List<string>();

            cDataSetHelper ds = new cDataSetHelper();
            if(this.StratvarList != null && this.StratvarList.Length > 0)
            {
                DataTable[] Strata_ValueList = new DataTable[this.StratvarList.Length];


                foreach (string s in this.StratvarList)
                {
                    StrataVarNameList.Add(s);
                    StrataVarName = s;
                }


                for (int ItemIndex = 0; ItemIndex < this.StratvarList.Length; ItemIndex++)
                {
                    Strata_ValueList[ItemIndex] = ds.SelectDistinct("Stratavalue", DT, StrataVarNameList[ItemIndex]);
                }


                
                for (int[] CurrentIndexes = new int[Strata_ValueList.Length]; isMoreLeft(Strata_ValueList, CurrentIndexes); IncrementArray(Strata_ValueList, CurrentIndexes))
                {
                    StrataVarValueList.Clear();
                    for (int CurrentIndex2 = 0; CurrentIndex2 < CurrentIndexes.Length; CurrentIndex2++)
                    {

                        if (Strata_ValueList[CurrentIndex2].Rows[CurrentIndexes[CurrentIndex2]][StrataVarNameList[CurrentIndex2]] == DBNull.Value)
                            {
                                StrataVar_Value = "NULL";
                            }
                            else
                            {
                                StrataVar_Value = Strata_ValueList[CurrentIndex2].Rows[CurrentIndexes[CurrentIndex2]][StrataVarNameList[CurrentIndex2]].ToString();
                            }
                            StrataVarValueList.Add(StrataVar_Value);
                    }

                         
                    DataTable workingTable = cWorkingTable.CreateWorkingTable(this.Numeric_Variable, this.Cross_Tabulation_Variable, config, DT, StrataVarNameList, StrataVarValueList, this.WeightVar);

                    if (string.IsNullOrEmpty(this.Cross_Tabulation_Variable))
                    {
                        if (!(string.IsNullOrEmpty(StrataVarValueList[0]) || StrataVarValueList[0].Equals("NULL")))
                            this.Execute_Means(HTMLString, this.Numeric_Variable, this.Cross_Tabulation_Variable, StrataVarNameList, StrataVarValueList, config, workingTable, DT.Select(StrataVarNameList[0] + " = '" + StrataVarValueList[0] + "'").CopyToDataTable());
                    }
                    else
                    {
                        string stratSelectClause = null;
                        if (!(string.IsNullOrEmpty(StrataVarValueList[0]) || StrataVarValueList[0].Equals("NULL")))
                            stratSelectClause = stratSelectClause + StrataVarNameList[0] + " = '" + StrataVarValueList[0] + "'";
                        for (int i = 1; i < StrataVarValueList.Count; i++)
                        {
                            if (!(string.IsNullOrEmpty(StrataVarValueList[i]) || StrataVarValueList[i].Equals("NULL")))
                                stratSelectClause = stratSelectClause + " AND " + StrataVarNameList[i] + " = '" + StrataVarValueList[i] + "'";
                        }
                        if (!string.IsNullOrEmpty(stratSelectClause))
                            this.Execute_CrossTab(HTMLString, this.Numeric_Variable, this.Cross_Tabulation_Variable, StrataVarNameList, StrataVarValueList, config, workingTable, DT.Select(stratSelectClause).CopyToDataTable());
                    }


                    if (!string.IsNullOrEmpty(this.OutTable))
                    {
                        workingTable.TableName = this.OutTable;
                        this.Context.OutTable(workingTable);
                    }


                }

            }
            else
            {
                if (!string.IsNullOrEmpty(StrataVarName))
                { 
                    StrataVarNameList.Add(StrataVarName);
                    StrataVarValueList.Add(StrataVar_Value);
                }

                DataTable workingTable = cWorkingTable.CreateWorkingTable(this.Numeric_Variable, this.Cross_Tabulation_Variable, config, DT, StrataVarNameList, StrataVarValueList, this.WeightVar);

                //HTMLString.Append("<table><tr>");
                //foreach (DataColumn c in workingTable.Columns)
                //{
                //    //System.Console.WriteLine("{0}\t{1}\t{2}", r[0], r[1], r[2]);
                //    //System.Console.WriteLine("{0}\t{1}\t{2}\t{3}", r[0], r[1], r[2], r[3]);
                //    System.Console.Write("{0}\t", c.ColumnName);
                //    HTMLString.Append("<th>");
                //    HTMLString.Append(c.ColumnName);
                //    HTMLString.Append("</th>");
                //}
                //System.Console.Write("\n");
                //HTMLString.Append("</tr>");
                //foreach (DataRow r in workingTable.Rows)
                //{
                //    HTMLString.Append("<tr>");
                //    foreach (DataColumn c in workingTable.Columns)
                //    {
                //        //System.Console.WriteLine("{0}\t{1}\t{2}", r[0], r[1], r[2]);
                //        //System.Console.WriteLine("{0}\t{1}\t{2}\t{3}", r[0], r[1], r[2], r[3]);
                //        System.Console.Write("{0}\t", r[c.ColumnName]);
                //        HTMLString.Append("<td>");
                //        HTMLString.Append(r[c.ColumnName]);
                //        HTMLString.Append("</td>");
                //    }
                //    System.Console.Write("\n");
                //    HTMLString.Append("</tr>");
                //}

                //HTMLString.Append("</table>");

                if (string.IsNullOrEmpty(this.Cross_Tabulation_Variable))
                {
                    this.Execute_Means(HTMLString, this.Numeric_Variable, this.Cross_Tabulation_Variable, StrataVarNameList, StrataVarValueList, config, workingTable, DT);
                }
                else
                {
                    this.Execute_CrossTab(HTMLString, this.Numeric_Variable, this.Cross_Tabulation_Variable, StrataVarNameList, StrataVarValueList, config, workingTable, DT);
                }

                if (!string.IsNullOrEmpty(this.OutTable))
                {
                    workingTable.TableName = this.OutTable;
                    this.Context.OutTable(workingTable);
                }

            }


            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("COMMANDNAME", "MEANS");
            args.Add("COMMANDTEXT", commandText.Trim());
            args.Add("MEANSVARIABLE", this.Numeric_Variable);
            args.Add("HTMLRESULTS", HTMLString.ToString());

            this.Context.Display(args);
        }



        private void Execute_Means(StringBuilder pHTMLString, string pNumericVariable, string pCrossTabVariable, List<string> pStratavar, List<string> pStrataValue, Dictionary<string, string> config, DataTable DT)
        {
            DataRow[] ROWS = DT.Select();

            double obs = 0;
            object Mode = 0.0;

            double Total = 0;
            object Min = 0.0;
            object Max = 0.0;

            System.Collections.Generic.Dictionary<string, double> ColumnTotalSet = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            //IEnumerable<DataColumn> SortedColumns = null;


            double AccumulatedTotal = 0;

            int i = 0;
            foreach (System.Data.DataRow R in ROWS)
            {

                double currrentCount;

                double.TryParse(R["__Count__"].ToString(), out currrentCount);


                AccumulatedTotal += currrentCount;
                obs += currrentCount;


                foreach (DataColumn C in DT.Columns)
                {
                    if (C.ColumnName != "__Values__" && C.ColumnName != "__Count__")
                    {
                        double ColVal = 0;
                        double.TryParse(R[C.ColumnName.ToUpper()].ToString(), out ColVal);

                        if (ColumnTotalSet.ContainsKey(C.ColumnName))
                        {
                            ColumnTotalSet[C.ColumnName] += ColVal;
                        }
                        else
                        {
                            ColumnTotalSet.Add(C.ColumnName, ColVal);
                        }

                    }
                }

            }


            if (pStratavar != null && pStratavar.Count > 0)
            {
                pHTMLString.Append("<b>");
                for (int StratIndex = 0; StratIndex < pStratavar.Count; StratIndex++)
                {
                    pHTMLString.Append(pStratavar[StratIndex]);
                    pHTMLString.Append("=");

                    object PrintValue = DBNull.Value;
                    double TestValue;
                    byte ByteValue;


                    switch (this.Context.Columns[pStratavar[StratIndex]].DataType.ToString())
                    {
                        case "System.Byte":
                        case "System.Boolean":
                            if (byte.TryParse(pStrataValue[StratIndex], out ByteValue))
                            {
                                PrintValue = ByteValue;
                            }
                            break;
                        default:
                            if (double.TryParse(pStrataValue[StratIndex], out TestValue))
                            {
                                PrintValue = TestValue;
                            }
                            else if (!string.IsNullOrEmpty(pStrataValue[StratIndex]))
                            {
                                PrintValue = pStrataValue[StratIndex];
                            }
                            break;
                    }


                    pHTMLString.Append(GetPrintValue(pStratavar[StratIndex], PrintValue, config));
                }
                pHTMLString.Append("</b><br/>");
            }

//            pHTMLString.Append("<table cellpadding=\"2\">");
//            pHTMLString.Append("<tr><th>");
//            pHTMLString.Append(this.Numeric_Variable);//GetPrintValue(Key.Key, this.Context.SetProperties));
//            pHTMLString.Append("</th><th>Frequency</th><th>Percent</th><th>Cum. Percent</th><th style=\"width:100px\">&nbsp;</th></tr>");

            Total = obs;
            AccumulatedTotal = 0;
            foreach (System.Data.DataRow R in DT.Rows)
            {
                //double temp;
                //double.TryParse(R["__Value__"].ToString(), out temp);
                //Total += temp;
                double currrentCount;
                double.TryParse(R["__Count__"].ToString(), out currrentCount);

                AccumulatedTotal += currrentCount;

//                pHTMLString.Append("<tr>");
//                pHTMLString.Append("<td>");
                //HTMLString.Append(R["__Value__"].ToString());
//                GetPrintValue(pNumericVariable, R["__Values__"], config, pHTMLString);
//                pHTMLString.Append("</td>");
//                pHTMLString.Append("<td align=\"right\">");
//                pHTMLString.Append(currrentCount.ToString());
//                pHTMLString.Append("</td>");
//                pHTMLString.Append("<td align=\"right\">");
//                pHTMLString.Append(ConvertToPercent(currrentCount / Total));
//                pHTMLString.Append("</td>");
//                pHTMLString.Append("<td align=\"right\">");
//                pHTMLString.Append(ConvertToPercent(AccumulatedTotal / Total));
//                pHTMLString.Append("</td><td><div class=PercentBar_Summary style=\"width:" + ConvertToPixelLength(currrentCount / Total) + "\">&nbsp;</div></td>");
//                pHTMLString.Append("</tr>");
            }

//            pHTMLString.Append("<tr>");
//            pHTMLString.Append("<td><b>TOTAL<b></td><td align=\"right\">");
//            pHTMLString.Append(Total.ToString());
//            pHTMLString.Append("</td><td align=\"right\">" + ConvertToPercent(1) + "</td><td align=\"right\">" + ConvertToPercent(1) + "</td><td><div class=PercentBar_Totals style=\"width:100%\">&nbsp;</div></td><tr>");


//            pHTMLString.Append("</table>");

//            pHTMLString.Append("<BR CLEAR=ALL/>");


            pHTMLString.Append("<TABLE ALIGN=CENTER>");
            pHTMLString.Append("<TR class='Stats'><TD  class='Stats' ALIGN=CENTER>&nbsp;</TD><TD  class='Stats' ALIGN=CENTER>Obs</TD><TD class='Stats' ALIGN=CENTER>Total</TD> <TD class='Stats' ALIGN=CENTER>Mean</TD><TD class='Stats'>Variance</TD><TD class='Stats' ALIGN=CENTER>Std Dev</TD></TR>");
            //pHTMLString.Append(string.Format("<TR><TD class='Stats' ALIGN=RIGHT>{0:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{1:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{2:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{3:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{4:#0.0000}</TD></TR>", obs, Sum, mean, variance, std_dev));
            AddStats1(pHTMLString, DT, "__Count__");
            pHTMLString.Append("</TABLE>");
            pHTMLString.Append("<TABLE ALIGN=CENTER>");
            pHTMLString.Append("<TR><TD class='Stats' ALIGN=CENTER>&nbsp;</TD><TD class='Stats' ALIGN=CENTER>Minimum</TD><TD class='Stats' ALIGN=CENTER>25%</TD><TD class='Stats' ALIGN=CENTER>Median</TD><TD class='Stats' ALIGN=CENTER>75%</TD><TD class='Stats' ALIGN=CENTER>Maximum</TD><TD class='Stats' ALIGN=CENTER>Mode</TD></TR>");
            //pHTMLString.Append(string.Format("<TR><TD class='Stats' ALIGN=RIGHT>{0:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{1:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{2:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{3:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{4:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{5:#0.0000}</TD></TR>", GetPrintValue(Min, this.Context.SetProperties), Percent_25, Median, Percent_75, GetPrintValue(Max, this.Context.SetProperties), GetPrintValue(Mode, this.Context.SetProperties)));
            AddStats2(pHTMLString, DT, obs, "__Count__");
            pHTMLString.Append("</TABLE>");
            pHTMLString.Append("<BR CLEAR=ALL/>");
            /*this.PrintDescriptiveStats(HTMLString);
            this.PrintANOVA(HTMLString);
            this.PrintBartlett(HTMLString);
            this.PrintKruskalWallis(HTMLString);*/

            if (!string.IsNullOrEmpty(this.OutTable))
            {
                DT.TableName = this.OutTable;
                this.Context.OutTable(DT);
            }
        }

        private void Execute_Means(StringBuilder pHTMLString, string pNumericVariable, string pCrossTabVariable, List<string> pStratavar, List<string> pStrataValue, Dictionary<string, string> config, DataTable DT, DataTable FullTable)
        {
            DataRow[] ROWS = DT.Select();

            double obs = 0;
            object Mode = 0.0;

            double Total = 0;
            object Min = 0.0;
            object Max = 0.0;

            System.Collections.Generic.Dictionary<string, double> ColumnTotalSet = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            //IEnumerable<DataColumn> SortedColumns = null;


            double AccumulatedTotal = 0;

            int i = 0;
            foreach (System.Data.DataRow R in ROWS)
            {

                double currrentCount;

                double.TryParse(R["__Count__"].ToString(), out currrentCount);


                AccumulatedTotal += currrentCount;
                obs += currrentCount;


                foreach (DataColumn C in DT.Columns)
                {
                    if (C.ColumnName != "__Values__" && C.ColumnName != "__Count__")
                    {
                        double ColVal = 0;
                        double.TryParse(R[C.ColumnName.ToUpper()].ToString(), out ColVal);

                        if (ColumnTotalSet.ContainsKey(C.ColumnName))
                        {
                            ColumnTotalSet[C.ColumnName] += ColVal;
                        }
                        else
                        {
                            ColumnTotalSet.Add(C.ColumnName, ColVal);
                        }

                    }
                }

            }


            if (pStratavar != null && pStratavar.Count > 0)
            {
                pHTMLString.Append("<b>");
                for (int StratIndex = 0; StratIndex < pStratavar.Count; StratIndex++)
                {
                    pHTMLString.Append(pStratavar[StratIndex]);
                    pHTMLString.Append("=");

                    object PrintValue = DBNull.Value;
                    double TestValue;
                    byte ByteValue;


                    switch (this.Context.Columns[pStratavar[StratIndex]].DataType.ToString())
                    {
                        case "System.Byte":
                        case "System.Boolean":
                            if (byte.TryParse(pStrataValue[StratIndex], out ByteValue))
                            {
                                PrintValue = ByteValue;
                            }
                            break;
                        default:
                            if (double.TryParse(pStrataValue[StratIndex], out TestValue))
                            {
                                PrintValue = TestValue;
                            }
                            else if (!string.IsNullOrEmpty(pStrataValue[StratIndex]))
                            {
                                PrintValue = pStrataValue[StratIndex];
                            }
                            break;
                    }


                    pHTMLString.Append(GetPrintValue(pStratavar[StratIndex], PrintValue, config));
                }
                pHTMLString.Append("</b><br/>");
            }

            //            pHTMLString.Append("<table cellpadding=\"2\">");
            //            pHTMLString.Append("<tr><th>");
            //            pHTMLString.Append(this.Numeric_Variable);//GetPrintValue(Key.Key, this.Context.SetProperties));
            //            pHTMLString.Append("</th><th>Frequency</th><th>Percent</th><th>Cum. Percent</th><th style=\"width:100px\">&nbsp;</th></tr>");

            Total = obs;
            AccumulatedTotal = 0;
            foreach (System.Data.DataRow R in DT.Rows)
            {
                //double temp;
                //double.TryParse(R["__Value__"].ToString(), out temp);
                //Total += temp;
                double currrentCount;
                double.TryParse(R["__Count__"].ToString(), out currrentCount);

                AccumulatedTotal += currrentCount;

                //                pHTMLString.Append("<tr>");
                //                pHTMLString.Append("<td>");
                //HTMLString.Append(R["__Value__"].ToString());
                //                GetPrintValue(pNumericVariable, R["__Values__"], config, pHTMLString);
                //                pHTMLString.Append("</td>");
                //                pHTMLString.Append("<td align=\"right\">");
                //                pHTMLString.Append(currrentCount.ToString());
                //                pHTMLString.Append("</td>");
                //                pHTMLString.Append("<td align=\"right\">");
                //                pHTMLString.Append(ConvertToPercent(currrentCount / Total));
                //                pHTMLString.Append("</td>");
                //                pHTMLString.Append("<td align=\"right\">");
                //                pHTMLString.Append(ConvertToPercent(AccumulatedTotal / Total));
                //                pHTMLString.Append("</td><td><div class=PercentBar_Summary style=\"width:" + ConvertToPixelLength(currrentCount / Total) + "\">&nbsp;</div></td>");
                //                pHTMLString.Append("</tr>");
            }

            //            pHTMLString.Append("<tr>");
            //            pHTMLString.Append("<td><b>TOTAL<b></td><td align=\"right\">");
            //            pHTMLString.Append(Total.ToString());
            //            pHTMLString.Append("</td><td align=\"right\">" + ConvertToPercent(1) + "</td><td align=\"right\">" + ConvertToPercent(1) + "</td><td><div class=PercentBar_Totals style=\"width:100%\">&nbsp;</div></td><tr>");


            //            pHTMLString.Append("</table>");

            //            pHTMLString.Append("<BR CLEAR=ALL/>");


            pHTMLString.Append("<TABLE ALIGN=CENTER>");
            pHTMLString.Append("<TR class='Stats'><TD  class='Stats' ALIGN=CENTER>&nbsp;</TD><TD  class='Stats' ALIGN=CENTER>Obs</TD><TD class='Stats' ALIGN=CENTER>Total</TD> <TD class='Stats' ALIGN=CENTER>Mean</TD><TD class='Stats'>Variance</TD><TD class='Stats' ALIGN=CENTER>Std Dev</TD></TR>");
            //pHTMLString.Append(string.Format("<TR><TD class='Stats' ALIGN=RIGHT>{0:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{1:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{2:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{3:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{4:#0.0000}</TD></TR>", obs, Sum, mean, variance, std_dev));
            AddStats1(pHTMLString, DT, "__Count__", FullTable);
            pHTMLString.Append("</TABLE>");
            pHTMLString.Append("<TABLE ALIGN=CENTER>");
            pHTMLString.Append("<TR><TD class='Stats' ALIGN=CENTER>&nbsp;</TD><TD class='Stats' ALIGN=CENTER>Minimum</TD><TD class='Stats' ALIGN=CENTER>25%</TD><TD class='Stats' ALIGN=CENTER>Median</TD><TD class='Stats' ALIGN=CENTER>75%</TD><TD class='Stats' ALIGN=CENTER>Maximum</TD><TD class='Stats' ALIGN=CENTER>Mode</TD></TR>");
            //pHTMLString.Append(string.Format("<TR><TD class='Stats' ALIGN=RIGHT>{0:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{1:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{2:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{3:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{4:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{5:#0.0000}</TD></TR>", GetPrintValue(Min, this.Context.SetProperties), Percent_25, Median, Percent_75, GetPrintValue(Max, this.Context.SetProperties), GetPrintValue(Mode, this.Context.SetProperties)));
            AddStats2(pHTMLString, DT, obs, "__Count__");
            pHTMLString.Append("</TABLE>");
            pHTMLString.Append("<BR CLEAR=ALL/>");
            /*this.PrintDescriptiveStats(HTMLString);
            this.PrintANOVA(HTMLString);
            this.PrintBartlett(HTMLString);
            this.PrintKruskalWallis(HTMLString);*/

            if (!string.IsNullOrEmpty(this.OutTable))
            {
                DT.TableName = this.OutTable;
                this.Context.OutTable(DT);
            }
        }

        private void Execute_CrossTab(StringBuilder pHTMLString, string pNumericVariable, string pCrossTabVariable, List<string> pStratavar, List<string> pStrataValue, Dictionary<string, string> config, DataTable DT, DataTable FullTable)
        {
            DataRow[] ROWS;
            string SelectedStatement;

            if (config["include-missing"].ToUpper() == "FALSE")
            {
                if (DT.Columns.Contains(config["RepresentationOfMissing"]))
                {
                    DT.Columns.Remove(DT.Columns[config["RepresentationOfMissing"]]);
                }
                //tempRows = Key.Value.Select(string.Format(" varname='{0}' and [value] is NOT NULL ", Key.Key), "value");
                SelectedStatement = " [__values__] is NOT NULL ";
            }
            else
            {
                SelectedStatement = "";
            }


            ROWS = DT.Select(SelectedStatement);


            double obs = 0;


            object Mode = 0.0;

            double Total = 0;
            object Min = 0.0;
            object Max = 0.0;

            System.Collections.Generic.Dictionary<string, double> ColumnTotalSet = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            IEnumerable<DataColumn> SortedColumns = null;
            StringBuilder RowPercent = new StringBuilder();
            StringBuilder ColPercent = new StringBuilder();

            foreach (System.Data.DataRow R in ROWS)
            {

                foreach (DataColumn C in DT.Columns)
                {
                    if (C.ColumnName != "__Values__" && C.ColumnName != "__Count__")
                    {
                        double ColVal = 0;
                        double.TryParse(R[C.ColumnName.ToUpper()].ToString(), out ColVal);

                        if (ColumnTotalSet.ContainsKey(C.ColumnName))
                        {
                            ColumnTotalSet[C.ColumnName] += ColVal;
                        }
                        else
                        {
                            ColumnTotalSet.Add(C.ColumnName, ColVal);
                        }

                    }
                }

            }

            string SelectOrder = "";
            if (
                (DT.Columns.Count == 4 && ROWS.Length == 2 && (this.Context.Columns[this.Cross_Tabulation_Variable].DataType.ToString() == "System.Byte" || this.Context.Columns[this.Cross_Tabulation_Variable].DataType.ToString() == "System.Boolean"))

                )
            {
                SortedColumns = DT.Columns.Cast<DataColumn>().OrderByDescending(x => { return x.ColumnName; });
                SelectOrder = "__Values__ desc";
            }
            else if (((DT.Columns.Count == 4 || DT.Columns.Count == 5) && (ROWS.Length == 2 || ROWS.Length == 3) && (this.Context.Columns[this.Cross_Tabulation_Variable].DataType.ToString() == "System.Byte" || this.Context.Columns[this.Cross_Tabulation_Variable].DataType.ToString() == "System.Boolean")) && (config["include-missing"].ToUpper() == "TRUE" && this.Context.EpiViewVariableList.Count > 0))
            {
                SortedColumns = DT.Columns.Cast<DataColumn>().OrderByDescending(x => { return GetPrintValue(this.Cross_Tabulation_Variable, this.Context.Columns[this.Cross_Tabulation_Variable].DataType.ToString(), x.ColumnName, config); });
                SelectOrder = "__Values__ desc";
            }
            else if ((DT.Columns.Count == 4 && ((this.Context.Columns[this.Cross_Tabulation_Variable].DataType.ToString() == "System.Byte" && DT.Columns[2].ColumnName == "0" && DT.Columns[3].ColumnName == "1") || this.Context.Columns[this.Cross_Tabulation_Variable].DataType.ToString() == "System.Boolean")))
            {
                SortedColumns = DT.Columns.Cast<DataColumn>().OrderByDescending(x => { return x.ColumnName; });
                SelectOrder = "__Values__ desc";
            }
            else
            {
                SortedColumns = DT.Columns.Cast<DataColumn>().OrderBy(dataColumn => { return dataColumn.ExtendedProperties["Value"]; });
            }

            if (pStratavar != null && pStratavar.Count > 0)
            {
                pHTMLString.Append("<b>");
                for (int StratIndex = 0; StratIndex < pStratavar.Count; StratIndex++)
                {
                    pHTMLString.Append(pStratavar[StratIndex]);
                    pHTMLString.Append("=");

                    object PrintValue = DBNull.Value;
                    double TestValue;
                    byte ByteValue;


                    switch (this.Context.Columns[pStratavar[StratIndex]].DataType.ToString())
                    {
                        case "System.Byte":
                        case "System.Boolean":
                            if (byte.TryParse(pStrataValue[StratIndex], out ByteValue))
                            {
                                PrintValue = ByteValue;
                            }
                            break;
                        default:
                            if (double.TryParse(pStrataValue[StratIndex], out TestValue))
                            {
                                PrintValue = TestValue;
                            }
                            else if (!string.IsNullOrEmpty(pStrataValue[StratIndex]))
                            {
                                PrintValue = pStrataValue[StratIndex];
                            }
                            break;
                    }
                    pHTMLString.Append(GetPrintValue(pStratavar[StratIndex], PrintValue, config));
                }
                pHTMLString.Append("</b><br/>");
            }

            //            pHTMLString.Append("<table>");
            //            pHTMLString.Append("<tr><th>&nbsp;</th><th colspan=");
            //            pHTMLString.Append(DT.Columns.Count - 2);
            //            pHTMLString.Append(">");
            //            pHTMLString.Append(this.Cross_Tabulation_Variable);
            //            pHTMLString.Append("</th><th>&nbsp;</th></tr>");
            //            pHTMLString.Append("<tr><th>");
            //            pHTMLString.Append(this.Numeric_Variable);//GetPrintValue(Key.Key, this.Context.SetProperties));
            //            pHTMLString.Append("</th>");
            foreach (DataColumn C in SortedColumns)
            {
                if (C.ColumnName != "__Values__" && C.ColumnName != "__Count__")
                {
                    //                    pHTMLString.Append("<th>");
                    //                    pHTMLString.Append(GetPrintValue(this.Cross_Tabulation_Variable, this.Context.Columns[this.Cross_Tabulation_Variable].DataType.Name, C.ColumnName, this.Context.SetProperties));
                    //                    pHTMLString.Append("</th>");
                    //ColumnTotalSet.Add(C.ColumnName, 0);
                }
            }

            //            pHTMLString.Append("<th>Total</th></tr>");

            DataTable horizontalFrequencies = new DataTable("horizontal");
            DataColumn horizontalCount = new DataColumn("freq", typeof(double));
            horizontalFrequencies.Columns.Add(horizontalCount);

            DataTable verticalFrequencies = new DataTable("vertical");
            DataColumn verticalCount = new DataColumn("freq", typeof(double));
            verticalFrequencies.Columns.Add(verticalCount);

            List<List<object>> allLocalFrequencies = new List<List<object>>();


            foreach (System.Data.DataRow R in DT.Select("", SelectOrder))
            {
                RowPercent.Length = 0;
                ColPercent.Length = 0;

                double RowTotal = 0;
                //                pHTMLString.Append("<tr><td><b>");
                //HTMLString.Append(R["__Values__"].ToString());
                //                GetPrintValue(pNumericVariable, R["__Values__"], config, pHTMLString);
                //                pHTMLString.Append("</b></td>");


                RowPercent.Append("<tr><td>Row%</td>");
                ColPercent.Append("<tr><td>Col%</td>");

                List<object> values = new List<object>();

                // value row
                foreach (DataColumn C in SortedColumns)
                {
                    if (C.ColumnName != "__Values__" && C.ColumnName != "__Count__")
                    {
                        double ColVal = 0;
                        double.TryParse(R[C.ColumnName.ToUpper()].ToString(), out ColVal);
                        values.Add(ColVal);
                        RowTotal += ColVal;
                        //                        pHTMLString.Append("<td align=\"right\">");
                        //                        pHTMLString.Append(GetPrintValue(pCrossTabVariable, ColVal.ToString(), this.Context.SetProperties));
                        //                        pHTMLString.Append("</td>");
                    }
                }
                allLocalFrequencies.Add(values);
                // percent row
                foreach (DataColumn C in SortedColumns)
                {
                    if (C.ColumnName != "__Values__" && C.ColumnName != "__Count__")
                    {
                        double ColVal = 0;
                        double.TryParse(R[C.ColumnName.ToUpper()].ToString(), out ColVal);
                        RowPercent.Append("<td align=\"right\">");
                        if (RowTotal == 0)
                        {
                            RowPercent.Append(GetPrintValue(pCrossTabVariable, ConvertToPercent(0 / 1).ToString(), this.Context.SetProperties));
                        }
                        else
                        {
                            RowPercent.Append(GetPrintValue(pCrossTabVariable, ConvertToPercent(ColVal / RowTotal).ToString(), this.Context.SetProperties));
                        }
                        RowPercent.Append("</td>");


                        ColPercent.Append("<td align=\"right\">");
                        ColPercent.Append(GetPrintValue(pCrossTabVariable, ConvertToPercent(ColVal / ColumnTotalSet[C.ColumnName]).ToString(), this.Context.SetProperties));
                        ColPercent.Append("</td>");
                    }
                }
                Total += RowTotal;
                //                pHTMLString.Append("<td align=\"right\">");
                //                pHTMLString.Append(RowTotal.ToString());
                //                pHTMLString.Append("</td></tr>");
                horizontalFrequencies.Rows.Add(RowTotal);

                RowPercent.Append("<td align=\"right\">" + ConvertToPercent(1) + "</td></tr>");
                ColPercent.Append("<td align=\"right\">");
                ColPercent.Append(GetPrintValue(pNumericVariable, ConvertToPercent(RowTotal / ColumnTotalSet.Sum(x => x.Value)).ToString(), this.Context.SetProperties));
                ColPercent.Append("</td></tr>");


                //                pHTMLString.Append(RowPercent);
                //                pHTMLString.Append(ColPercent);

            }

            RowPercent.Length = 0;
            ColPercent.Length = 0;

            //            pHTMLString.Append("<tr><td><b>TOTAL</b></td>");

            RowPercent.Append("<tr><td>Row%</td>");
            ColPercent.Append("<tr><td>Col%</td>");



            foreach (DataColumn C in SortedColumns)
            {
                if (C.ColumnName != "__Values__" && C.ColumnName != "__Count__")
                {
                    //                    pHTMLString.Append("<td align=\"right\">");
                    //                    pHTMLString.Append(GetPrintValue(pCrossTabVariable, ColumnTotalSet[C.ColumnName], this.Context.SetProperties));
                    //                    pHTMLString.Append("</td>");
                    RowPercent.Append("<td align=\"right\">");
                    RowPercent.Append(GetPrintValue(pCrossTabVariable, ConvertToPercent(ColumnTotalSet[C.ColumnName] / Total).ToString(), this.Context.SetProperties));
                    RowPercent.Append("</td>");

                    ColPercent.Append("<td align=\"right\">" + ConvertToPercent(1) + "</td>");

                    verticalFrequencies.Rows.Add(GetPrintValue(pCrossTabVariable, ColumnTotalSet[C.ColumnName], this.Context.SetProperties));
                }
            }

            //            pHTMLString.Append("<td align=\"right\">");
            //            pHTMLString.Append(Total);
            grandRecordCount = Total;
            //            pHTMLString.Append("</td></tr>");
            RowPercent.Append("<td align=\"right\">" + ConvertToPercent(1) + "</td></tr>");
            ColPercent.Append("<td align=\"right\">" + ConvertToPercent(1) + "</td></tr>");

            //            pHTMLString.Append(RowPercent);
            //            pHTMLString.Append(ColPercent);


            //           pHTMLString.Append("</table>");

            //            pHTMLString.Append("<BR CLEAR=ALL/>");

            crosstabs = 0;
            grandObservations = 0;
            grandSum = 0;
            observationsList = new List<double>();
            unweightedObservationsList = new List<double>();
            averagesList = new List<double>();
            variancesList = new List<double>();

            pHTMLString.Append("<TABLE ALIGN=CENTER>");
            pHTMLString.Append("<TR class='Stats'><TH  class='Stats' ALIGN=CENTER colspan=6>Descriptive Statistics for Each Value of Crosstab Variable</TH></TR>");
            pHTMLString.Append("<TR class='Stats'><TD  class='Stats' ALIGN=CENTER>&nbsp;</TD><TD  class='Stats' ALIGN=CENTER>Obs</TD><TD class='Stats' ALIGN=CENTER>Total</TD> <TD class='Stats' ALIGN=CENTER>Mean</TD><TD class='Stats'>Variance</TD><TD class='Stats' ALIGN=CENTER>Std Dev</TD></TR>");
            foreach (DataColumn C in SortedColumns)
            {
                if (C.ColumnName != "__Values__" && C.ColumnName != "__Count__")
                {
                    AddStats1(pHTMLString, DT, C.ColumnName, FullTable);
                    crosstabs++;
                }
            }

            pHTMLString.Append("</TABLE>");
            pHTMLString.Append("<TABLE ALIGN=CENTER>");
            pHTMLString.Append("<TR><TD  class='Stats' ALIGN=CENTER>&nbsp;</TD><TD class='Stats' ALIGN=CENTER>Minimum</TD><TD class='Stats' ALIGN=CENTER>25%</TD><TD class='Stats' ALIGN=CENTER>Median</TD><TD class='Stats' ALIGN=CENTER>75%</TD><TD class='Stats' ALIGN=CENTER>Maximum</TD><TD class='Stats' ALIGN=CENTER>Mode</TD></TR>");

            foreach (DataColumn C in SortedColumns)
            {
                if (C.ColumnName != "__Values__" && C.ColumnName != "__Count__")
                {
                    AddStats2(pHTMLString, DT, obs, C.ColumnName);
                }
            }
            pHTMLString.Append("</TABLE>");
            pHTMLString.Append("<BR CLEAR=ALL/>");

            this.PrintANOVA(pHTMLString, horizontalFrequencies, verticalFrequencies, allLocalFrequencies);
            //this.PrintKruskalWallis(pHTMLString, horizontalFrequencies, verticalFrequencies, allLocalFrequencies);

            if (!string.IsNullOrEmpty(this.OutTable))
            {
                DT.TableName = this.OutTable;
                this.Context.OutTable(DT);
            }
        }

        private void AddStats1(StringBuilder pHTMLString, DataTable pWorkingTable, string pColumnName)
        {
            try
            {
                double obs = 0;
                double Sum = 0;
                double Sum_Sqr = 0;
                double mean = 0;
                double variance = 0;
                double std_dev = 0;


                double AccumulatedTotal = 0;

                DataRow[] ROWS = pWorkingTable.Select(string.Format("[{0}] > 0 ", pColumnName));
                double i = 0;
                foreach (System.Data.DataRow R in ROWS)
                {
                    double temp;
                    //object temp = R["__Values__"];
                    double.TryParse(R["__Values__"].ToString(), out temp);
                    double currentCount;

                    double.TryParse(R[pColumnName].ToString(), out currentCount);
                    obs += currentCount;

                    Sum += (temp * currentCount);

                    Sum_Sqr += Math.Pow(temp, 2) * currentCount;
                }

                mean = Sum / obs;

                averagesList.Add(mean);
                observationsList.Add(obs);

                grandSum = grandSum + Sum;
                grandObservations = grandObservations + obs;

                variance = (Sum_Sqr - Sum * mean) / (obs - 1);

                variance = 0.0;
                Sum_Sqr = 0.0;
                foreach (System.Data.DataRow R in ROWS)
                {
                    double temp;
                    double.TryParse(R["__Values__"].ToString(), out temp);
                    double currentCount;

                    double.TryParse(R[pColumnName].ToString(), out currentCount);

                    Sum_Sqr += Math.Pow(temp - mean, 2) * currentCount;
                    i += currentCount;
                }

                variance = Sum_Sqr / (i - 1);

                unweightedObservationsList.Add(i);
                variancesList.Add(variance);

                std_dev = Math.Sqrt(variance);
            }
            catch (Exception ex)
            {
                //
            }

            object PrintValue = DBNull.Value;
            double TestValue;
            byte ByteValue;
            bool BooleanValue;

            if (pColumnName == "__Count__")
            {
                PrintValue = "";
            }
            else
            {
                string dataType = this.Context.Columns[this.Cross_Tabulation_Variable].DataType.ToString();
                switch (dataType)
                {
                    case "System.Boolean":
                        if (bool.TryParse(pColumnName, out BooleanValue))
                        {
                            PrintValue = BooleanValue;
                        }
                        break;
                    case "System.Byte":
                        if (byte.TryParse(pColumnName, out ByteValue))
                        {
                            PrintValue = ByteValue;
                        }
                        break;
                    default:
                        if (pColumnName == "__Count__")
                        {
                            PrintValue = "";
                        }
                        else
                        { 
                            if (double.TryParse(pColumnName, out TestValue))
                            {
                                PrintValue = TestValue;
                            }
                            else if (!string.IsNullOrEmpty(pColumnName))
                            {
                                PrintValue = pColumnName;
                            }
                        }
                        break;
                }
            }

            Statistic_Struct ss = cWorkingTable.GetStatistics(pWorkingTable, pColumnName);

            pHTMLString.Append(
                string.Format(
                "<TR><TD  class='Stats' ALIGN=CENTER>{0}</TD><TD class='Stats' ALIGN=RIGHT>{1:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{2:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{3:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{4:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{5:#0.0000}</TD></TR>", 
                GetPrintValue(this.Numeric_Variable, PrintValue, this.Context.SetProperties), 
                ss.Obs, 
                ss.Sum, 
                ss.AVG, 
                ss.Variance, 
                ss.Std_Dev));
        }

        private void AddStats1(StringBuilder pHTMLString, DataTable pWorkingTable, string pColumnName, DataTable FullTable)
        {
            try
            {
                double obs = 0;
                double Sum = 0;
                double Sum_Sqr = 0;
                double mean = 0;
                double variance = 0;
                double std_dev = 0;


                double AccumulatedTotal = 0;

                DataRow[] ROWS = pWorkingTable.Select(string.Format("[{0}] > 0 ", pColumnName));
                double i = 0;
                foreach (System.Data.DataRow R in ROWS)
                {
                    double temp;
                    //object temp = R["__Values__"];
                    double.TryParse(R["__Values__"].ToString(), out temp);
                    double currentCount;

                    double.TryParse(R[pColumnName].ToString(), out currentCount);
                    obs += currentCount;

                    Sum += (temp * currentCount);

                    Sum_Sqr += Math.Pow(temp, 2) * currentCount;
                }

                mean = Sum / obs;

                averagesList.Add(mean);
                observationsList.Add(obs);

                grandSum = grandSum + Sum;
                grandObservations = grandObservations + obs;

                variance = (Sum_Sqr - Sum * mean) / (obs - 1);

                variance = 0.0;
                Sum_Sqr = 0.0;
                foreach (System.Data.DataRow R in ROWS)
                {
                    double temp;
                    double.TryParse(R["__Values__"].ToString(), out temp);
                    double currentCount;

                    double.TryParse(R[pColumnName].ToString(), out currentCount);

                    Sum_Sqr += Math.Pow(temp - mean, 2) * currentCount;
                    i += currentCount;
                }
                DataRow[] ALLROWS = FullTable.Select();
                i = 0.0;
                foreach (System.Data.DataRow R in ALLROWS)
                {
                    if ((!string.IsNullOrEmpty(this.Cross_Tabulation_Variable) && R[this.Cross_Tabulation_Variable].ToString().Equals(pColumnName)) || pColumnName.Equals("__Count__"))
                        i++;
                }

                variance = Sum_Sqr / (i - 1);
                unweightedObservations += i;

                unweightedObservationsList.Add(i);
                variancesList.Add(variance);

                std_dev = Math.Sqrt(variance);
            }
            catch (Exception ex)
            {
                //
            }

            object PrintValue = DBNull.Value;
            double TestValue;
            byte ByteValue;
            bool BooleanValue;

            if (pColumnName == "__Count__")
            {
                PrintValue = "";
            }
            else
            {
                string dataType = this.Context.Columns[this.Cross_Tabulation_Variable].DataType.ToString();
                switch (dataType)
                {
                    case "System.Boolean":
                        if (bool.TryParse(pColumnName, out BooleanValue))
                        {
                            PrintValue = BooleanValue;
                        }
                        break;
                    case "System.Byte":
                        if (byte.TryParse(pColumnName, out ByteValue))
                        {
                            PrintValue = ByteValue;
                        }
                        break;
                    default:
                        if (pColumnName == "__Count__")
                        {
                            PrintValue = "";
                        }
                        else
                        {
                            if (double.TryParse(pColumnName, out TestValue))
                            {
                                PrintValue = TestValue;
                            }
                            else if (!string.IsNullOrEmpty(pColumnName))
                            {
                                PrintValue = pColumnName;
                            }
                        }
                        break;
                }
            }

            Statistic_Struct ss = cWorkingTable.GetStatistics(pWorkingTable, pColumnName, FullTable);
            ss.Variance = variancesList.Last();
            ss.Std_Dev = Math.Sqrt(ss.Variance);
            ss.Std_Error = ss.Std_Dev / Math.Sqrt(ss.Obs);
            ss.tZero = ss.AVG / ss.Std_Error;
            ss.tZeroP = 2.0 * Epi.Statistics.SharedResources.PFromT(ss.tZero, (int)ss.Obs - 1);

            pHTMLString.Append(
                string.Format(
                "<TR><TD  class='Stats' ALIGN=CENTER>{0}</TD><TD class='Stats' ALIGN=RIGHT>{1:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{2:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{3:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{4:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{5:#0.0000}</TD></TR>",
                GetPrintValue(this.Numeric_Variable, PrintValue, this.Context.SetProperties),
                ss.Obs,
                ss.Sum,
                ss.AVG,
                ss.Variance,
                ss.Std_Dev));
        }

        private void AddStats2(StringBuilder pHTMLString, DataTable pWorkingTable, double pNumberOfObservations, string pColumnName)
        {
            object PrintValue = DBNull.Value;
            double TestValue;
            byte ByteValue;
            bool BooleanValue;

            if (pColumnName == "__Count__")
            {
                PrintValue = "";
            }
            else
            {
                switch (this.Context.Columns[this.Cross_Tabulation_Variable].DataType.ToString())
                {
                    case "System.Boolean":
                        if (bool.TryParse(pColumnName, out BooleanValue))
                        {
                            PrintValue = BooleanValue;
                        }
                        break;
                    case "System.Byte":
                        if (byte.TryParse(pColumnName, out ByteValue))
                        {
                            PrintValue = ByteValue;
                        }
                        break;
                    default:
                        if (pColumnName == "__Count__")
                        {
                            PrintValue = "";
                        }
                        else
                        {
                            if (double.TryParse(pColumnName, out TestValue))
                            {
                                PrintValue = TestValue;
                            }
                            else if (!string.IsNullOrEmpty(pColumnName))
                            {
                                PrintValue = pColumnName;
                            }
                        }
                        break;
                }
            }
            
            Statistic_Struct ss = cWorkingTable.GetStatistics(pWorkingTable, pColumnName);

            bool isNumeric = false;

            if(this.NumericTypeList.Contains(this.Context.Columns[this.Numeric_Variable].DataType.ToString().Replace("System.","").ToUpper()))
            {
                isNumeric = true;
            }

            if (isNumeric)
            {
                pHTMLString.Append(string.Format("<TR><TD  class='Stats' ALIGN=CENTER>{0}</TD><TD class='Stats' ALIGN=RIGHT>{1:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{2:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{3:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{4:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{5:#0.0000}</TD><TD class='Stats' ALIGN=RIGHT>{6:#0.0000}</TD></TR>", GetPrintValue(this.Numeric_Variable, PrintValue, this.Context.SetProperties), ss.Min, ss.Median_25, ss.Median, ss.Median_75, ss.Max, ss.Mode));
            }
            else
            {
                pHTMLString.Append(string.Format("<TR><TD  class='Stats' ALIGN=CENTER>{0}</TD><TD class='Stats' ALIGN=RIGHT>{1}</TD><TD class='Stats' ALIGN=RIGHT>{2}</TD><TD class='Stats' ALIGN=RIGHT>{3}</TD><TD class='Stats' ALIGN=RIGHT>{4}</TD><TD class='Stats' ALIGN=RIGHT>{5}</TD><TD class='Stats' ALIGN=RIGHT>{6}</TD></TR>", GetPrintValue(this.Numeric_Variable, PrintValue, this.Context.SetProperties), ss.Min, ss.Median_25, ss.Median, ss.Median_75, ss.Max, ss.Mode));
            }
        }

        private string ConvertToPercent(double pValue)
        {
            //"{0: ##0.0}%"
            string format = "{0: ##0";

            if (config.Settings.PrecisionForStatistics == 0)
            {
                format = format + "}%";
            }
            else
            {
                format = format + ".";
                for (int i = 0; i < config.Settings.PrecisionForStatistics; i++)
                {
                    format = format + "0";
                }
                format = format + "}%";
            }

            return string.Format(format, (100.0 * pValue));
            //return //string.Format("{0: ##0.0}%", (100.0 * pValue));
        }

        private string ConvertToPixelLength(double pValue)
        {
            return string.Format("{0: ##0}px", 1 * Math.Round((100.0 * pValue), MidpointRounding.AwayFromZero));
        }

        private System.Type GetColumnDataType(string pColumnName)
        {
            System.Type result = null;

            foreach (System.Data.DataColumn C in this.Context.Columns)
            {
                if (C.ColumnName.ToUpper() == pColumnName.ToUpper())
                {
                    result = C.DataType;
                    break;
                }
            }

            return result;
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

        private void PrintANOVA(StringBuilder pBuilder, DataTable horizontalFrequencies, DataTable verticalFrequencies, List<List<object>> allLocalFrequencies)
        {
            if (crosstabs <= 1)
            {
                return;
            }
            for (int i = 0; i < observationsList.Count; i++)
                if (observationsList[i] == 0.0)
                    return;

            double grandMean = grandSum / grandObservations;
            double? ssBetween = CalculateSSBetween(grandMean, observationsList, averagesList);
            double? dfBetween = crosstabs - 1;
            double? msBetween = ssBetween / dfBetween;
            double? ssWithin = CalculateSSWithin(observationsList, variancesList);
            double? dfWithin = /*grandTotal*/grandObservations - crosstabs;
            double? dfError = unweightedObservations - crosstabs;
            double? msWithin = ssWithin / dfWithin;
            double? fStatistic = msBetween / msWithin;
            double? anovaPValue = new StatisticsRepository.statlib().PfromF(fStatistic.Value, dfBetween.Value, dfError.Value);
            double? anovaTValue = null;            
            double? chiSquare = CalculateChiSquare(dfWithin.Value, msWithin.Value, observationsList, variancesList);
            double? bartlettPValue = new StatisticsRepository.statlib().PfromX2(chiSquare.Value, dfBetween.Value);

            if (crosstabs == 2 && dfBetween.HasValue && anovaPValue.HasValue)
            {
                // do T-Statistics
                double SatterthwaiteDF = Math.Pow(variancesList[0] / observationsList[0] + variancesList[1] / observationsList[1], 2.0) / (1.0 / (unweightedObservationsList[0] - 1.0) * Math.Pow(variancesList[0] / observationsList[0], 2.0) + 1.0 / (unweightedObservationsList[1] - 1.0) * Math.Pow(variancesList[1] / observationsList[1], 2.0));
                double SEu = Math.Sqrt(variancesList[0] / observationsList[0] + variancesList[1] / observationsList[1]);
                SatterthwaiteDF = Math.Pow(SEu, 4.0) / (1.0 / (unweightedObservationsList[0] - 1.0) * Math.Pow(variancesList[0] / observationsList[0], 2.0) + 1.0 / (unweightedObservationsList[1] - 1.0) * Math.Pow(variancesList[1] / observationsList[1], 2.0));
                double meansDiff = averagesList[0] - averagesList[1];
                double stdDevDiff = Math.Sqrt(((unweightedObservationsList[0] - 1) * variancesList[0] + (unweightedObservationsList[1] - 1) * variancesList[1]) / (unweightedObservationsList[0] + unweightedObservationsList[1] - 2));
                int df = (int)unweightedObservationsList[0] + (int)unweightedObservationsList[1] - 2;
                short shortDF = (short)df;
                double tProbability = 0.05;
                double? intervalLength = new StatisticsRepository.statlib().TfromP(ref tProbability, ref shortDF) * stdDevDiff * Math.Sqrt(1 / observationsList[0] + 1 / observationsList[1]);
                double tStatistic = meansDiff / (stdDevDiff * Math.Sqrt(1.0 / observationsList[0] + 1.0 / observationsList[1]));
                double pEqual = 2.0 * Epi.Statistics.SharedResources.PFromT(tStatistic, (int)df);
                double tStatisticUnequal = meansDiff / SEu;
                double pUnequalLower = 2.0 * Epi.Statistics.SharedResources.PFromT(tStatisticUnequal, (int)Math.Ceiling(SatterthwaiteDF));
                double pUnequalUpper = 2.0 * Epi.Statistics.SharedResources.PFromT(tStatisticUnequal, (int)Math.Floor(SatterthwaiteDF));
                double pUneqal = pUnequalLower + (SatterthwaiteDF - Math.Floor(SatterthwaiteDF)) * (pUnequalUpper - pUnequalLower);
                short shortDFCeiling = (short)(int)Math.Ceiling(SatterthwaiteDF);
                short shortDFFloor = (short)(int)Math.Floor(SatterthwaiteDF);
                double? unEqualIntervalTLower = new StatisticsRepository.statlib().TfromP(ref tProbability, ref shortDFCeiling);
                double? unEqualIntervalTUpper = new StatisticsRepository.statlib().TfromP(ref tProbability, ref shortDFFloor);
                double unEqualIntervalT = (double)unEqualIntervalTLower + (SatterthwaiteDF - Math.Floor(SatterthwaiteDF)) * (double)(unEqualIntervalTUpper - unEqualIntervalTLower);

                double equalLCLMean = meansDiff - (double)intervalLength;
                double equalUCLMean = meansDiff + (double)intervalLength;
                double unequalLCLMean = meansDiff - SEu * unEqualIntervalT;
                double unequalUCLMean = meansDiff + SEu * unEqualIntervalT;

                pBuilder.AppendLine("<BR><H4 ALIGN=CENTER>T-Test</H4>");
                pBuilder.AppendLine("<TABLE ALIGN=CENTER><TR><TD Class='Stats'></TD><TD ALIGN=LEFT Class='Stats'>Method</TD><TD ALIGN=CENTER Class='Stats'>Mean</TD><TD ALIGN=CENTER Class='Stats' Colspan='2'>95% CL Mean</TD><TD Class='Stats'>Std Dev</TD></TR>");
                pBuilder.AppendLine("<TR><TD Class='Stats'>Diff (Group 1 - Group 2)</TD><TD ALIGN=LEFT Class='Stats'>Pooled</TD><TD ALIGN=RIGHT Class='Stats'>" + meansDiff.ToString("F4") + "</TD><TD ALIGN=RIGHT Class='Stats'>" + equalLCLMean.ToString("F4") + "</TD><TD ALIGN=RIGHT Class='Stats'>" + equalUCLMean.ToString("F4") + "</TD><TD ALIGN=RIGHT Class='Stats'>" + stdDevDiff.ToString("F4") + "</TD></TR>");
                pBuilder.AppendLine("<TR><TD Class='Stats'>Diff (Group 1 - Group 2)</TD><TD ALIGN=LEFT Class='Stats'>Satterthwaite</TD><TD ALIGN=RIGHT Class='Stats'>" + meansDiff.ToString("F4") + "</TD><TD ALIGN=RIGHT Class='Stats'>" + unequalLCLMean.ToString("F4") + "</TD><TD ALIGN=RIGHT Class='Stats'>" + unequalUCLMean.ToString("F4") + "</TD><TD ALIGN=RIGHT Class='Stats'></TD></TR>");
                pBuilder.AppendLine("</TABLE><BR/>");

                pBuilder.AppendLine("<TABLE ALIGN=CENTER><TR><TD Class='Stats'>Method</TD><TD ALIGN=CENTER Class='Stats'>Variances</TD><TD ALIGN=CENTER Class='Stats'>DF</TD><TD ALIGN=CENTER Class='Stats'>t Value</TD><TD Class='Stats'>Pr > |t|</TD></TR>");
                pBuilder.AppendLine("<TR><TD Class='Stats'>Pooled</TD><TD ALIGN=LEFT Class='Stats'>Equal</TD><TD ALIGN=RIGHT Class='Stats'>" + df + "</TD><TD ALIGN=RIGHT Class='Stats'>" + tStatistic.ToString("F2") + "</TD><TD ALIGN=RIGHT Class='Stats'>" + pEqual.ToString("F4") + "</TD></TR>");
                pBuilder.AppendLine("<TR><TD Class='Stats'>Satterthwaite</TD><TD ALIGN=LEFT Class='Stats'>Unequal</TD><TD ALIGN=RIGHT Class='Stats'>" + SatterthwaiteDF.ToString("F2") + "</TD><TD ALIGN=RIGHT Class='Stats'>" + tStatisticUnequal.ToString("F2") + "</TD><TD ALIGN=RIGHT Class='Stats'>" + pUneqal.ToString("F4") + "</TD></TR>");
                pBuilder.AppendLine("</TABLE><BR/>");
            }

            string strssBetweenValue = SharedStrings.UNDEFINED;
            string strdfBetweenValue = SharedStrings.UNDEFINED;
            string strmsBetweenValue = SharedStrings.UNDEFINED;
            string strssWithinValue = SharedStrings.UNDEFINED;
            string strdfWithinValue = SharedStrings.UNDEFINED;
            string strmsWithinValue = SharedStrings.UNDEFINED;
            string strfStatisticValue = SharedStrings.UNDEFINED;
            string stranovaPValueValue = SharedStrings.UNDEFINED;
            string stranovaTValueValue = SharedStrings.UNDEFINED;
            string strchiSquareValue = SharedStrings.UNDEFINED;
            string strbartlettPValue = SharedStrings.UNDEFINED;
            string strTotalSSValue = SharedStrings.UNDEFINED;
            string strTotalDFValue = SharedStrings.UNDEFINED;

            if (ssBetween.HasValue) { strssBetweenValue = ssBetween.Value.ToString("F5"); }
            if (dfBetween.HasValue) { strdfBetweenValue = dfBetween.Value.ToString("F0"); }
            if (msBetween.HasValue) { strmsBetweenValue = msBetween.Value.ToString("F5"); }
            if (ssWithin.HasValue) { strssWithinValue = ssWithin.Value.ToString("F5"); }
            if (dfWithin.HasValue) { strdfWithinValue = dfWithin.Value.ToString("F0"); }
            if (msWithin.HasValue) { strmsWithinValue = msWithin.Value.ToString("F5"); }
            if (fStatistic.HasValue) { strfStatisticValue = fStatistic.Value.ToString("F5"); }
            if (anovaPValue.HasValue) { stranovaPValueValue = anovaPValue.Value.ToString("F5"); }
            if (anovaTValue.HasValue) { stranovaTValueValue = anovaTValue.Value.ToString("F5"); }
            if (chiSquare.HasValue) { strchiSquareValue = chiSquare.Value.ToString("F5"); }
            if (bartlettPValue.HasValue) { strbartlettPValue = bartlettPValue.Value.ToString("F5"); }

            if (ssBetween.HasValue && ssWithin.HasValue) { strTotalSSValue = (ssBetween.Value + ssWithin.Value).ToString("F5"); }
            if (dfBetween.HasValue && dfWithin.HasValue) { strTotalDFValue = (dfBetween.Value + dfWithin.Value).ToString("F0"); }

            pBuilder.AppendLine("<BR><H4 ALIGN=CENTER>ANOVA, a Parametric Test for Inequality of Population Means</H4>");
            pBuilder.AppendLine("<P ALIGN=CENTER>(For normally distributed data only)</P>");
            pBuilder.AppendLine("<TABLE ALIGN=CENTER><TR><TD Class='Stats'>Variation</TD><TD ALIGN=CENTER Class='Stats'>SS</TD><TD ALIGN=CENTER Class='Stats'>df</TD><TD ALIGN=CENTER Class='Stats'>MS</TD><TD Class='Stats'>F statistic</TD></TR>");
            pBuilder.AppendLine("<TR><TD Class='Stats'>Between</TD><TD ALIGN=RIGHT Class='Stats'>" + strssBetweenValue + "</TD><TD ALIGN=RIGHT Class='Stats'>" + strdfBetweenValue + "</TD><TD ALIGN=RIGHT Class='Stats'>" + strmsBetweenValue + "</TD><TD ALIGN=RIGHT Class='Stats'>" + strfStatisticValue + "</TD></TR>");
            pBuilder.AppendLine("<TR><TD Class='Stats'>Within</TD><TD ALIGN=RIGHT Class='Stats'>" + strssWithinValue + "</TD><TD ALIGN=RIGHT Class='Stats'>" + strdfWithinValue + "</TD><TD ALIGN=RIGHT Class='Stats'>" + strmsWithinValue + "</TD><TD Class='Stats'>&nbsp;</TD></TR>");
            pBuilder.AppendLine("<TR><TD Class='Stats'>Total</TD><TD ALIGN=RIGHT Class='Stats'>" + strTotalSSValue + "</TD><TD ALIGN=RIGHT Class='Stats'>" + strTotalDFValue + "</TD><TD Class='Stats'>&nbsp;</TD><TD Class='Stats'>&nbsp;</TD></TR>");
            pBuilder.AppendLine("</TABLE><BR/>");
            if (crosstabs == 2)
            {
                // Not implemented at this time
                //pBuilder.AppendLine("<P ALIGN=CENTER>T Statistic = " + stranovaTValueValue + "</P>"); // no T-Statistic available in current build
            }
            pBuilder.AppendLine("<P ALIGN=CENTER>P-value = " + stranovaPValueValue + "</P><BR>");
            
            pBuilder.AppendLine("<H4 ALIGN=CENTER> Bartlett's Test for Inequality of Population Variances</H4>");
            pBuilder.AppendLine("<TABLE ALIGN=CENTER>");
            pBuilder.AppendLine("<TR><TD Class='Stats'>Bartlett's chi square=</TD><TD Class='Stats'>" + strchiSquareValue + "</TD><TD Class='Stats'>df=" + strdfBetweenValue + "</TD><TD Class='Stats'>P value=" + strbartlettPValue + "</TD></TR>");
            pBuilder.AppendLine("</TABLE><P ALIGN=CENTER>A small p-value (e.g., less than 0.05 suggests that the variances are not homogeneous and that the ANOVA may not be appropriate.</p>");

            double? kruskalWallisH = CalculateKruskalWallisH(horizontalFrequencies, verticalFrequencies, allLocalFrequencies, /*grandTotal*/ grandRecordCount );
            double? kruskalPValue = new StatisticsRepository.statlib().PfromX2(kruskalWallisH.Value, dfBetween.Value);

            string strKruskalWallisH = SharedStrings.UNDEFINED;
            string strKruskalPValue = SharedStrings.UNDEFINED;

            if (kruskalWallisH.HasValue) { strKruskalWallisH = kruskalWallisH.Value.ToString("F4"); }
            if (kruskalPValue.HasValue) { strKruskalPValue = kruskalPValue.Value.ToString("F4"); }

            pBuilder.AppendLine("<H4 ALIGN=CENTER>Mann-Whitney/Wilcoxon Two-Sample Test (Kruskal-Wallis test for two groups)</H4>");
            pBuilder.AppendLine("<TABLE ALIGN=CENTER>");
            pBuilder.AppendLine("<TR><TD ALIGN=RIGHT Class='Stats'>Kruskal-Wallis H (equivalent to Chi square) = </TD><TD ALIGN=RIGHT Class='Stats'>" + strKruskalWallisH + "</TD></TR>");
            pBuilder.AppendLine("<TR><TD ALIGN=RIGHT Class='Stats'>  Degrees of freedom = </TD><TD ALIGN=RIGHT Class='Stats'>" + strdfBetweenValue + "</TD><TR><TD ALIGN=RIGHT Class='Stats'> P value = </TD><TD ALIGN=RIGHT Class='Stats'> " + strKruskalPValue + "</TD></TR></TABLE>");

        }

        private void GetPrintValue(string pFieldName, object pValue, Dictionary<string,string> pConfig, StringBuilder pBuilder)
        {
            if (pValue == DBNull.Value)
            {
                pBuilder.Append(pConfig["RepresentationOfMissing"]);
            }
            else switch (pValue.GetType().Name.Replace("System.", ""))
                {
                    case "Byte":
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
                    case "Boolean":
                        pBuilder.Append((Convert.ToBoolean(pValue) ? pConfig["RepresentationOfYes"] : pConfig["RepresentationOfNo"]));
                        break;
                    case "Double":
                        pBuilder.Append(string.Format("{0:#.##}", pValue));
                        break;
                    default:
                        pBuilder.Append(pValue);
                        break;
                }
        }


        public double GetMedian(string ColumnName, DataRow[] pRows, int pStart, int pEnd)
        {
            double result = 0;

            int Length = pEnd - pStart;

            double MiddleNumber = Length / 2.0;
            if (Length <= pRows.Length && Length > 0)
            {
                if (Length == 1)
                {
                    result = Convert.ToDouble(pRows[0][ColumnName]);
                }
                else if (Length % 2 == 0)
                {
                    result = (Convert.ToDouble(pRows[Convert.ToInt32(pStart + MiddleNumber - 1)][ColumnName]) + Convert.ToDouble(pRows[Convert.ToInt32(pStart + MiddleNumber)][ColumnName])) / 2.0;
                }
                else
                {
                    result = Convert.ToDouble(pRows[Convert.ToInt32(pStart + MiddleNumber)][ColumnName]);
                }
            }

            return result;
        }


        private string GetPrintValue(string pFieldName, string pDataType, object pValue, Dictionary<string, string> pConfig)
        {
            string result = null;

            if (pValue == DBNull.Value)
            {
                result = pConfig["RepresentationOfMissing"];
            }
            else switch (pDataType.Replace("System.", ""))
                {
                    case "Byte":
                        if (this.Context.EpiViewVariableList.ContainsKey(pFieldName))
                        {
                            int val = 0;
                            if(int.TryParse(pValue.ToString(),out val))
                            {
                                result = (val != 0  ? pConfig["RepresentationOfYes"] : pConfig["RepresentationOfNo"]);
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
                        result = string.Format("{0:0.##}", pValue);
                        break;
                    default:
                        result = pValue.ToString();
                        break;
                }

            return result;
        }

        private string GetPrintValue(string pFieldName, object pValue, Dictionary<string, string> pConfig)
        {
            string result = null;

            if (pValue == DBNull.Value)
            {
                result = pConfig["RepresentationOfMissing"];
            }
            else switch (pValue.GetType().Name.Replace("System.", ""))
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


        private void PrintTable(List<DataRow> pRows, StringBuilder pHTML)
        {
            Dictionary<string, long> ColHeaders = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, long> RowHeaders = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, Dictionary<string, long>> CellDetail = new Dictionary<string, Dictionary<string, long>>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, DataRow> RowList = new Dictionary<string, DataRow>(StringComparer.OrdinalIgnoreCase);


            //RowTotal = Foreach ColumnValue : Sum(RowValue)
            //ColumnTotal == Foreach Rowvalue : Sum(ColumnValue)

            bool twobytwo = false;



            long ColTotal = 0;

            foreach (DataRow R in pRows)
            {
                string ColKey = GetPrintValue(this.Numeric_Variable,R[this.Numeric_Variable], this.Context.SetProperties);
                string RowKey = GetPrintValue(this.Cross_Tabulation_Variable,R[this.Cross_Tabulation_Variable], this.Context.SetProperties);

                if (ColHeaders.ContainsKey(ColKey))
                {
                    ColHeaders[ColKey] += Convert.ToInt32(R["Outcome"]);
                    ColTotal += Convert.ToInt32(R["Outcome"]);
                }
                else
                {
                    ColHeaders.Add(ColKey, Convert.ToInt32(R["Outcome"]));
                    ColTotal += Convert.ToInt32(R["Outcome"]);
                }

                if (RowHeaders.ContainsKey(RowKey))
                {
                    RowHeaders[RowKey] += Convert.ToInt32(R["Outcome"]);
                }
                else
                {
                    RowHeaders.Add(RowKey, Convert.ToInt32(R["Outcome"]));
                }


                if (CellDetail.ContainsKey(ColKey))
                {
                    if (CellDetail[ColKey].ContainsKey(RowKey))
                    {
                        //System.Diagnostics.Debug.Assert(false);
                        CellDetail[ColKey][RowKey] += Convert.ToInt32(R["Outcome"]);
                    }
                    else
                    {
                        CellDetail[ColKey].Add(RowKey, Convert.ToInt32(R["Outcome"]));
                    }
                }
                else
                {
                    CellDetail.Add(ColKey, new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase));
                    CellDetail[ColKey].Add(RowKey, Convert.ToInt32(R["Outcome"]));
                }
            }

            if (RowHeaders.Count == 2 && ColHeaders.Count == 2 && pRows.Count == 4)
            {
                twobytwo = true;
            }

            pHTML.Append("<table align=\"left\"><tr><th colspan='");
            pHTML.Append(RowHeaders.Count + 2);
            pHTML.Append("'>");
            pHTML.Append(this.Cross_Tabulation_Variable);
            pHTML.Append("</th></tr><tr><th>");
            pHTML.Append(this.Numeric_Variable);
            pHTML.Append("</th>");



            long yy = 0; // yes/yes cell
            long yn = 0; // yes/no cell
            long ny = 0; // no/yes cell
            long nn = 0; // no/no cell
            long rnt = 0; // right no total
            long ryt = 0; // right yes total

            long bnt = 0; // bottom no total
            long byt = 0; // bottom yes total

            long tt = 0; // total





            IEnumerable<KeyValuePair<string, long>> RHE = null;
            IEnumerable<KeyValuePair<string, long>> CHE = null;
            if (twobytwo)
            {
                CHE = ColHeaders.OrderByDescending(x => { return x.Key; });
                RHE = RowHeaders.OrderByDescending(x => { return x.Key; });
            }
            else
            {
                CHE = ColHeaders;
                RHE = RowHeaders;
            }


            foreach (KeyValuePair<string, long> RH in RHE)
            {
                pHTML.Append("<th>");
                pHTML.Append(RH.Key);
                pHTML.Append("</th>");
            }
            pHTML.Append("<th>Total</th></tr>");

            int counter = 0;


            foreach (KeyValuePair<string, long> CH in CHE)
            {
                pHTML.Append("<tr><td>");
                pHTML.Append("<strong>" + CH.Key + "</strong>");
                pHTML.Append("<br/>Row%<br/>Col%</td>");

                foreach (KeyValuePair<string, long> RH in RHE)
                {
                    pHTML.Append("<td>");
                    if (CellDetail.ContainsKey(CH.Key) && CellDetail[CH.Key].ContainsKey(RH.Key))
                    {
                        GetPrintValue(CH.Key, CellDetail[CH.Key][RH.Key], this.Context.SetProperties, pHTML);

                        if (twobytwo)
                        {
                            switch (counter)
                            {
                                case 0:
                                    yy = CellDetail[CH.Key][RH.Key];
                                    break;
                                case 1:
                                    yn = CellDetail[CH.Key][RH.Key];
                                    break;
                                case 3:
                                    ny = CellDetail[CH.Key][RH.Key];
                                    break;
                                case 4:
                                    nn = CellDetail[CH.Key][RH.Key];
                                    break;
                            }
                            counter++;
                        }

                        pHTML.Append("<br/>");
                        GetPrintValue(CH.Key, (Convert.ToDouble(CellDetail[CH.Key][RH.Key]) / Convert.ToDouble(CH.Value)) * 100.0, this.Context.SetProperties, pHTML);
                        pHTML.Append("<br/>");
                        GetPrintValue(CH.Key, (Convert.ToDouble(CellDetail[CH.Key][RH.Key]) / Convert.ToDouble(RH.Value)) * 100.0, this.Context.SetProperties, pHTML);

                    }
                    else
                    {
                        pHTML.Append("0<br/>0<br/>0");
                    }
                    pHTML.Append("</td>");
                }

                pHTML.Append("<td>");
                pHTML.Append(CH.Value.ToString());

                if (twobytwo)
                {
                    switch (counter)
                    {
                        case 2:
                            ryt = CH.Value;
                            break;
                        case 5:
                            rnt = CH.Value;
                            break;
                    }
                    counter++;
                }

                pHTML.Append("<br/>100%<br/>");
                GetPrintValue(CH.Key, Convert.ToDouble(CH.Value) / Convert.ToDouble(ColTotal) * 100.0, this.Context.SetProperties, pHTML);
                pHTML.Append("</td>");

                pHTML.Append("</tr>");
            }



            pHTML.Append("<tr><td>");
            pHTML.Append("<strong>Total</strong>");
            pHTML.Append("</td>");

            long TotalRows = 0;
            counter = 0;

            foreach (KeyValuePair<string, long> RH in RHE)
            {
                pHTML.Append("<td>");
                TotalRows += RH.Value;
                pHTML.Append(RH.Value.ToString());

                if (twobytwo)
                {
                    switch (counter)
                    {
                        case 0:
                            byt = RH.Value;
                            break;
                        case 1:
                            bnt = RH.Value;
                            break;
                    }
                    counter++;
                }

                pHTML.Append("</td>");
            }
            pHTML.Append("<td>");
            pHTML.Append(TotalRows.ToString());
            if (twobytwo)
            {
                tt = TotalRows;
            }
            pHTML.Append("</td>");
            pHTML.Append("</table>");

            if (twobytwo)
            {
                StatisticsRepository.cTable.SingleTableResults singleTableResults;
                singleTableResults = new StatisticsRepository.cTable().SigTable((double)yy, (double)yn, (double)ny, (double)nn, 0.95);

                double yyPct = ((double)yy / (double)tt) * 160;
                double ynPct = ((double)yn / (double)tt) * 160;
                double nyPct = ((double)ny / (double)tt) * 160;
                double nnPct = ((double)nn / (double)tt) * 160;

                string oddsRatioEstimate = Epi.SharedStrings.UNDEFINED;
                string oddsRatioLower = SharedStrings.UNDEFINED;
                string oddsRatioUpper = SharedStrings.UNDEFINED;
                string riskRatioEstimate = SharedStrings.UNDEFINED;
                string riskRatioLower = SharedStrings.UNDEFINED;
                string riskRatioUpper = SharedStrings.UNDEFINED;

                if (singleTableResults.OddsRatioLower != null)
                {
                    oddsRatioLower = ((double)singleTableResults.OddsRatioLower).ToString("F4");
                }

                if (singleTableResults.OddsRatioUpper != null)
                {
                    oddsRatioUpper = ((double)singleTableResults.OddsRatioUpper).ToString("F4");
                }

                if (singleTableResults.RiskRatioEstimate != null)
                {
                    riskRatioEstimate = ((double)singleTableResults.RiskRatioEstimate).ToString("F4");
                }

                if (singleTableResults.RiskRatioLower != null)
                {
                    riskRatioLower = ((double)singleTableResults.RiskRatioLower).ToString("F4");
                }

                if (singleTableResults.RiskRatioUpper != null)
                {
                    riskRatioUpper = ((double)singleTableResults.RiskRatioUpper).ToString("F4");
                }

                pHTML.Append("<table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"padding-left: 50px;\">");
                pHTML.Append("<tr>");
                pHTML.Append("<td class=\"TwoByTwo\" valign=\"bottom\" align=\"right\"><div style=\"background-color: red; width:" + yyPct.ToString("F0") + "; height:" + yyPct.ToString("F0") + ";\"><!-- --></td>");
                pHTML.Append("<td class=\"TwoByTwo\" valign=\"bottom\" align=\"left\"><div style=\"background-color: orange; width:" + ynPct.ToString("F0") + "; height:" + ynPct.ToString("F0") + ";\"><!-- --></td>");
                pHTML.Append("</tr><tr>");
                pHTML.Append("<td class=\"TwoByTwo\" valign=\"top\" align=\"right\"><div style=\"background-color: yellow; width:" + nyPct.ToString("F0") + "; height:" + nyPct.ToString("F0") + ";\"><!-- --></td>");
                pHTML.Append("<td class=\"TwoByTwo\" valign=\"top\" align=\"left\"><div style=\"background-color: green; width:" + nnPct.ToString("F0") + "; height:" + nnPct.ToString("F0") + ";\"><!-- --></td>");
                pHTML.Append("</tr></table>");
                pHTML.Append("<br clear=\"all\" />");

                pHTML.Append("<br clear=\"all\" /><h4 align=\"center\"> Single Table Analysis </h4>");
                pHTML.Append("<table align=\"center\">");
                pHTML.Append("<tr><td class=\"stats\"></td><td class=\"stats\" align=\"center\">Point</td>");
                pHTML.Append("<td class=\"stats\" colspan=\"2\" align=\"center\">95% Confidence Interval</td>");
                pHTML.Append("<tr><td class=\"stats\"></td><td class=\"stats\" align=\"center\">Estimate<td class=\"stats\" align=\"right\">Lower<td class=\"stats\" align=\"right\">Upper</td></tr>");
                pHTML.Append("<tr><td class=\"stats\">PARAMETERS: Odds-based</td><td class=\"stats\"></td><td class=\"stats\"></td><td class=\"stats\"></td></tr>");
                pHTML.Append("<tr><td class=\"stats\">Odds Ratio (cross product)</td><td class=\"stats\" align=\"right\">" + /*singleTableResults.OddsRatioEstimate.ToString("F4")*/oddsRatioEstimate + "</td><td class=\"stats\" align=\"right\">" + /*singleTableResults.OddsRatioLower.ToString("F4")*/oddsRatioLower + "</td><td class=\"stats\" align=\"right\">" + /*singleTableResults.OddsRatioUpper.ToString("F4")*/oddsRatioUpper + "<tt> (T)</tt></td></tr>");
                pHTML.Append("<tr><td class=\"stats\">Odds Ratio (MLE)</td><td class=\"stats\" align=\"right\">" + singleTableResults.OddsRatioMLEEstimate.ToString("F4") + "</td><td class=\"stats\" align=\"right\">" + singleTableResults.OddsRatioMLEMidPLower.ToString("F4") + "</td><td class=\"stats\" align=\"right\">" + singleTableResults.OddsRatioMLEMidPUpper.ToString("F4") + "<tt> (M)</tt></td></tr>");
                pHTML.Append("<tr><td class=\"stats\"></td><td class=\"stats\"></td><td class=\"stats\" align=\"right\">" + singleTableResults.OddsRatioMLEFisherLower.ToString("F4") + "</td><td class=\"stats\" align=\"right\">" + singleTableResults.OddsRatioMLEFisherUpper.ToString("F4") + "<tt> (F)</tt></td></tr>");
                pHTML.Append("<tr><td class=\"stats\">PARAMETERS: Risk-based</td><td class=\"stats\"></td><td class=\"stats\"></td><td class=\"stats\"></td></tr>");
                pHTML.Append("<tr><td class=\"stats\">Risk Ratio (RR)</td><td class=\"stats\" align=\"right\">" + /*singleTableResults.RiskRatioEstimate.ToString("F4")*/riskRatioEstimate + "</td><td class=\"stats\" align=\"right\">" + /*singleTableResults.RiskRatioLower.ToString("F4")*/riskRatioLower + "</td><td class=\"stats\" align=\"right\">" + /*singleTableResults.RiskRatioUpper.ToString("F4")*/ riskRatioUpper + "<tt> (T)</tt></td></tr>");
                pHTML.Append("<tr><td class=\"stats\">Risk Difference (RD%)</td><td class=\"stats\" align=\"right\">" + singleTableResults.RiskDifferenceEstimate.ToString("F4") + "</td><td class=\"stats\" align=\"right\">" + singleTableResults.RiskDifferenceLower.ToString("F4") + "</td><td class=\"stats\" align=\"right\">" + singleTableResults.RiskDifferenceUpper.ToString("F4") + "<tt> (T)</tt></td></tr>");
                pHTML.Append("<tr /><tr /><tr /><tr /><tr />");
                pHTML.Append("<tr> <td class=\"stats\" colspan=\"4\"><p align=\"center\"><tt> (T=Taylor series; C=Cornfield; M=Mid-P; F=Fisher Exact)</tt></p></tr>");
                pHTML.Append("<tr /><tr /><tr /><tr /><tr />");
                pHTML.Append("<tr><td class=\"stats\">STATISTICAL TESTS</td> <td class=\"stats\">Chi-square<td class=\"stats\">1-tailed p<td class=\"stats\">2-tailed p<tr><td class=\"stats\">Chi-square - uncorrected<td class=\"stats\" align=\"right\">" + singleTableResults.ChiSquareUncorrectedVal.ToString("F4") + "<td class=\"stats\"><td class=\"stats\" align=\"right\">" + singleTableResults.ChiSquareUncorrected2P.ToString("F10") + "</td></tr>");
                pHTML.Append("<tr><td class=\"stats\">Chi-square - Mantel-Haenszel</td> <td class=\"stats\" align=\"right\">" + singleTableResults.ChiSquareMantelVal.ToString("F4") + "<td class=\"stats\"><td class=\"stats\" align=\"right\">" + singleTableResults.ChiSquareMantel2P.ToString("F10") + "</td></tr>");
                pHTML.Append("<tr><td class=\"stats\">Chi-square - corrected (Yates)</td> <td class=\"stats\" align=\"right\">" + singleTableResults.ChiSquareYatesVal.ToString("F4") + "<td class=\"stats\"></td><td class=\"stats\" align=\"right\">" + singleTableResults.ChiSquareYates2P.ToString("F10") + "</td></tr>");
                pHTML.Append("<tr><td class=\"stats\">Mid-p exact</td>  <td class=\"stats\"></td> <td class=\"stats\" align=\"right\">" + singleTableResults.MidP.ToString("F10") + "</td><td class=\"stats\"></td></tr>");
                pHTML.Append("<tr><td class=\"stats\">Fisher exact</td> <td class=\"stats\"></td> <td class=\"stats\" align=\"right\">" + singleTableResults.FisherExactP.ToString("F10") + "</td><td class=\"stats\"></td></tr>");
                pHTML.Append("</table>");

            }
            pHTML.Append("<br clear=\"all\" />");
            pHTML.Append("<br clear=\"all\" />");
        }

        private bool isMoreLeft(DataTable[] pDataTable, int[] pCurrentIndexes)
        {
            bool result = true;

            if(pCurrentIndexes[0] > pDataTable[0].Rows.Count - 1)
            {
                result = false;
            }

            return result;
        }


        private void IncrementArray(DataTable[] pDataTable, int[] pCurrentIndexes)
        {
            bool ContinueProcessing = true;

            for (int i = pCurrentIndexes.Length -1; i > -1 && ContinueProcessing; i--)
            {
                if (pCurrentIndexes[i] + 1 > pDataTable[i].Rows.Count - 1)
                {
                    if (i > 0)
                    {
                        pCurrentIndexes[i] = 0;
                    }
                    else
                    {
                        pCurrentIndexes[i]++;
                        ContinueProcessing = false;
                    }
                }
                else
                {
                    pCurrentIndexes[i]++;
                
                    ContinueProcessing = false;
                }
            }

        }
    }

}
