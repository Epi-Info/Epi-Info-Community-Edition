using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using EpiInfo.Plugin;
using Epi.Data;


namespace Epi.Analysis.Statistics
{ 
    public class Tables : IAnalysisStatistic
    {
        List<double> yyList = new List<double>();
        List<double> ynList = new List<double>();
        List<double> nyList = new List<double>();
        List<double> nnList = new List<double>();

        string Exposure = null;
        string Outcome = null;
        string[] AggregateList = null;
        Dictionary<string, string> AggregateElement = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string[] StratvarList = null;
        string OutTable = null;
        string WeightVar = null;
        string commandText = string.Empty;
        string CurrentRead_Identifier = string.Empty;
        IAnalysisStatisticContext Context;
        public StatisticsRepository.cTable.SingleTableResults singleTableResults;
        private Configuration config = null;

        bool tablesShowStatistics = true;

        static private System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<object>> PermutationList;
        static private System.Collections.Generic.List<string> SelectClauses;

        public Tables(IAnalysisStatisticContext AnalysisStatisticContext) 
        {
            this.Construct(AnalysisStatisticContext);
        }

        public string Name { get { return "Epi.Analysis.Statistics.Tables"; } }
        public void Construct(IAnalysisStatisticContext AnalysisStatisticContext)
        {
            this.Context = AnalysisStatisticContext;
            this.config = Configuration.GetNewInstance();
            //AggregateElementList
            this.Exposure = this.Context.InputVariableList["Exposure_Variable"].Trim(new char[] { '[', ']' });
            this.Outcome = this.Context.InputVariableList["Outcome_Variable"].Trim(new char[] { '[', ']' });

            if (this.Context.InputVariableList.ContainsKey("Stratavar"))
            {
                this.StratvarList = this.Context.InputVariableList["Stratavar"].Split(',');
                for (int i = 0; i < this.StratvarList.Length; i++)
                {
                    this.StratvarList[i] = this.StratvarList[i].Trim(new char[] { '[', ']' });
                }
            }
            /*
            string[]table = this.Context.InputVariableList["AggregateElementList"].Split(',');
            for(int i = 0; i < table.Length; i++)
            {
                string[] row = table[i].Split('=');
                this.AggregateElement.Add(row[0].Trim(), row[1].Replace(" ","").Trim());
            }*/

            if (this.Context.InputVariableList.ContainsKey("OutTable"))
            {
                this.OutTable = this.Context.InputVariableList["OutTable"];
            }

            if (this.Context.InputVariableList.ContainsKey("WeightVar"))
            {
                this.WeightVar = this.Context.InputVariableList["WeightVar"];
            }

            if (this.Context.InputVariableList.ContainsKey("STATISTICS"))
            {
                if (this.Context.InputVariableList["STATISTICS"].Equals("NONE"))
                {
                    tablesShowStatistics = false;
                }
            }

            this.commandText = this.Context.InputVariableList["commandText"];
            this.CurrentRead_Identifier = this.Context.InputVariableList["TableName"];
            this.commandText = this.Context.InputVariableList["CommandText"];

        }


        /// <summary>
        /// performs execution of the MEANS command
        /// </summary>
        /// <returns>object</returns>
        public void Execute()
        {
            yyList = new List<double>();
            ynList = new List<double>();
            nyList = new List<double>();
            nnList = new List<double>();

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
            System.Data.DataTable OutDataTable = new DataTable();

            foreach (DataColumn column in this.Context.Columns)
            {
                DataColumn newColumn = new DataColumn(column.ColumnName);
                newColumn.DataType = column.DataType;
                DT.Columns.Add(newColumn);
            }

            foreach (DataRow row in this.Context.GetDataRows(null))
            {
                DT.ImportRow(row);
            }

            string StrataVarName = null;
            string StrataVar_Value = null;
            List<string> StrataVarNameList = new List<string>();
            List<string> StrataVarValueList = new List<string>();

            if (!string.IsNullOrEmpty(this.OutTable))
            {
                InitOutTable(OutDataTable);
            }
            cDataSetHelper ds = new cDataSetHelper();
            if (this.StratvarList != null && this.StratvarList.Length > 0)
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
                    //loops for each value of the strata variable
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

                    //creates a workingTable for each value of the strata variable; previous workingTables lost; only last strata value's workingTable exists when done
                    //OutDataTable used for when an Output to Table is requested.
                    DataTable workingTable = cWorkingTable.CreateWorkingTable(this.Exposure, this.Outcome, config, DT, StrataVarNameList, StrataVarValueList, this.WeightVar);

                    if (!string.IsNullOrEmpty(this.OutTable))
                    {
                        BuildOutTable(workingTable, StrataVarNameList, StrataVarValueList, OutDataTable);
                    }

                    this.Execute_CrossTab(HTMLString, this.Exposure, this.Outcome, StrataVarNameList, StrataVarValueList, config, workingTable);
                }

//                double cochranQ = executeCochran(this.Exposure, this.Outcome, "ColumnValues", DT, this.Exposure, yyList, StrataVarNameList, StrataVarValueList, this.WeightVar);
//                double cochranQP = PValFromChiSq(cochranQ, 1 + StrataVarNameList.Count);

                if (tablesShowStatistics)
                try
                {
                    StatisticsRepository.Strat2x2 strat2x2 = new StatisticsRepository.Strat2x2();

                    double[] yyArray = yyList.ToArray();
                    double[] ynArray = ynList.ToArray();
                    double[] nyArray = nyList.ToArray();
                    double[] nnArray = nnList.ToArray();
                    double cumulativeYY = yyList.Sum();
                    double cumulativeYN = ynList.Sum();
                    double cumulativeNY = nyList.Sum();
                    double cumulativeNN = nnList.Sum();

                    double computedOddsRatio = (double)strat2x2.ComputeOddsRatio(yyArray, ynArray, nyArray, nnArray);
                    double computedOddsRatioMHLL = computedOddsRatio * Math.Exp(-(double)strat2x2.ZSElnOR(yyList.ToArray(), ynList.ToArray(), nyList.ToArray(), nnList.ToArray()));
                    double computedOddsRatioMHUL = computedOddsRatio * Math.Exp((double)strat2x2.ZSElnOR(yyList.ToArray(), ynList.ToArray(), nyList.ToArray(), nnList.ToArray()));
                    double computedRR = (double)strat2x2.ComputedRR(yyArray, ynArray, nyArray, nnArray);
                    double computedRRMHLL = computedRR * Math.Exp(-(double)strat2x2.ZSElnRR(yyList.ToArray(), ynList.ToArray(), nyList.ToArray(), nnList.ToArray()));
                    double computedRRMHUL = computedRR * Math.Exp((double)strat2x2.ZSElnRR(yyList.ToArray(), ynList.ToArray(), nyList.ToArray(), nnList.ToArray()));
                    double mleOR = double.NaN;
                    double ExactORLL = double.NaN;
                    double ExactORUL = double.NaN;
                    if (cumulativeYN == 0.0 || cumulativeNY == 0.0)
                    {
                        mleOR = double.PositiveInfinity;
                        ExactORLL = (double)strat2x2.exactorln(yyArray, ynArray, nyArray, nnArray);
                        ExactORUL = double.PositiveInfinity;
                    }
                    else if (cumulativeYY == 0.0 || cumulativeNN == 0.0)
                    {
                        mleOR = 0.0;
                        ExactORLL = 0.0;
                        ExactORUL = (double)strat2x2.exactorun(yyArray, ynArray, nyArray, nnArray, ref mleOR);
                    }
                    else
                    {
                        mleOR = (double)strat2x2.ucestimaten(yyArray, ynArray, nyArray, nnArray);
                        ExactORLL = (double)strat2x2.exactorln(yyArray, ynArray, nyArray, nnArray);
                        ExactORUL = (double)strat2x2.exactorun(yyArray, ynArray, nyArray, nnArray, ref mleOR);
                    }
                    double uncorrectedChiSquare = strat2x2.ComputeUnChisq(yyArray, ynArray, nyArray, nnArray);
                    double corrChisq = strat2x2.ComputeCorrChisq(yyArray, ynArray, nyArray, nnArray);
                    double uncorrectedChiSquareP = (double)strat2x2.pForChisq(uncorrectedChiSquare);
                    double corrChisqP = (double)strat2x2.pForChisq(corrChisq);
                    double bdtOR = Epi.Statistics.Single2x2.bdtOR(yyList, ynList, nyList, nnList, computedOddsRatio);
                    double bdtORP = (double)strat2x2.pForChisq(bdtOR, (double)yyList.Count - 1.0);
                    double bdOR = Epi.Statistics.Single2x2.bdOR(yyList, ynList, nyList, nnList);
                    double bdORP = (double)strat2x2.pForChisq(bdOR, (double)yyList.Count - 1.0);
                    double bdRR = Epi.Statistics.Single2x2.bdRR(yyList, ynList, nyList, nnList);
                    double bdRRP = (double)strat2x2.pForChisq(bdRR, (double)yyList.Count - 1.0);

                    double yy = 0.0;
                    double yn = 0.0;
                    double ny = 0.0;
                    double nn = 0.0;
                    for (int i = 0; i < yyList.Count; i++)
                    {
                        yy = yy + yyList[i];
                        yn = yn + ynList[i];
                        ny = ny + nyList[i];
                        nn = nn + nnList[i];
                    }
                    StatisticsRepository.cTable.SingleTableResults singleTableResults;
                    singleTableResults = new StatisticsRepository.cTable().SigTable((double)yy, (double)yn, (double)ny, (double)nn, 0.95);
                    // ???
                    //double fisherExactRight = strat2x2.ComputeFisherExactRightTail(yyList.ToArray(), ynList.ToArray(), nyList.ToArray(), nnList.ToArray());


                    HTMLString.AppendLine("<center><strong>SUMMARY INFORMATION</strong></center>"); // TODO: Remove horrible <center> tag, replace with proper HTML

                    HTMLString.AppendLine("<table cellspacing=10 align=center>");
                    HTMLString.AppendLine("    <tr>");
                    HTMLString.AppendLine("        <td class=\"stats\"></td>");
                    HTMLString.AppendLine("        <td class=\"stats\">Point</td>");
                    HTMLString.AppendLine("        <td class=\"stats\" colspan=\"2\">95%Confidence Interval</td>");
                    HTMLString.AppendLine("    </tr>");
                    HTMLString.AppendLine("    <tr>");
                    HTMLString.AppendLine("        <td class=\"stats\">Parameters</td>");
                    HTMLString.AppendLine("        <td class=\"stats\" align=\"right\">Estimate</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>Lower</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>Upper</td>");
                    HTMLString.AppendLine("    <TR>");
                    HTMLString.AppendLine("        <TD class=\"stats\">Odds Ratio Estimates</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\"></td>");
                    HTMLString.AppendLine("        <TD class=\"stats\"></td>");
                    HTMLString.AppendLine("        <TD class=\"stats\"></td>");
                    HTMLString.AppendLine("    <TR>");
                    HTMLString.AppendLine("        <TD class=\"stats\">Crude OR (cross product)</td>");
                    try
                    {
                        HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + Math.Round((decimal)singleTableResults.OddsRatioEstimate, 4).ToString() + "</td>"); // 23.4545
                    }
                    catch (Exception ex)
                    {
                        // do nothing for now
                        HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT> </td>"); // 23.4545
                    }
                    try
                    {
                        HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + Math.Round((decimal)singleTableResults.OddsRatioLower, 4).ToString() + ",</td>"); // 5.8410
                    }
                    catch (Exception ex)
                    {
                        // do nothing for now
                        HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT> </td>"); // 23.4545
                    }
                    try
                    {
                        HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + Math.Round((decimal)singleTableResults.OddsRatioUpper, 4).ToString() + "<TT> (T)</TT></td>"); //94.1811
                    }
                    catch (Exception ex)
                    {
                        // do nothing for now
                        HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT> </td>"); // 23.4545
                    }
                    HTMLString.AppendLine("    </tr>");
                    HTMLString.AppendLine("    <TR>");
                    HTMLString.AppendLine("        <TD class=\"stats\">Crude OR (MLE)</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + Math.Round((decimal)singleTableResults.OddsRatioMLEEstimate, 4).ToString() + "</td>"); //22.1490
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + Math.Round((decimal)singleTableResults.OddsRatioMLEMidPLower, 4).ToString() + ",</td>"); // 5.9280
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + Math.Round((decimal)singleTableResults.OddsRatioMLEMidPUpper, 4).ToString() + "<TT> (M)</TT></td>"); // 109.1473
                    HTMLString.AppendLine("    </tr>");
                    HTMLString.AppendLine("    <TR>");
                    HTMLString.AppendLine("        <TD class=\"stats\"></td>");
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT></td>");
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + Math.Round((decimal)singleTableResults.OddsRatioMLEFisherLower, 4).ToString() + ",</td>"); // 5.2153
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + Math.Round((decimal)singleTableResults.OddsRatioMLEFisherUpper, 4).ToString() + "<TT> (F)</TT></td>"); // 138.3935
                    HTMLString.AppendLine("    </tr>");
                    HTMLString.AppendLine("    <TR>");
                    HTMLString.AppendLine("        <TD class=\"stats\">Adjusted OR (MH)</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + computedOddsRatio.ToString("N4") + "</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + computedOddsRatioMHLL.ToString("N4") + ",</td>"); // 7.1914
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + computedOddsRatioMHUL.ToString("N4") + "<TT> (R)</TT></td>"); // 197.0062
                    HTMLString.AppendLine("    </tr>");
                    HTMLString.AppendLine("    <TR>");
                    HTMLString.AppendLine("        <TD class=\"stats\">Adjusted OR (MLE)</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + mleOR.ToString("N4") + "</td>"); // 28.5753
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + ExactORLL.ToString("N4") + ",</td>"); // 6.9155
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + ExactORUL.ToString("N4") + "<TT> (F)</TT></td>"); // 160.2216
                    HTMLString.AppendLine("    </tr>");
                    HTMLString.AppendLine("    <TR>");
                    //HTMLString.AppendLine("        <TD class=\"stats\"></td>");
                    //HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT></td>");
                    //HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT> N.I.,</td>"); // 6.0084
                    //HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>N.I.<TT> (F)</TT></td>"); // 207.5014
                    //HTMLString.AppendLine("    </tr>");
                    //HTMLString.AppendLine("    <TR>");
                    HTMLString.AppendLine("        <TD class=\"stats\">Risk Ratios (RR)</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\"></td>");
                    HTMLString.AppendLine("        <TD class=\"stats\"></td>");
                    HTMLString.AppendLine("        <TD class=\"stats\"></td>");
                    HTMLString.AppendLine("    </tr>");
                    HTMLString.AppendLine("    <TR>");
                    HTMLString.AppendLine("        <TD class=\"stats\">Crude Risk Ratio (RR)</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + Math.Round((decimal)singleTableResults.RiskRatioEstimate, 4).ToString() + "</td>"); // 5.5741
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + Math.Round((decimal)singleTableResults.RiskRatioLower, 4).ToString() + ",</td>"); // 1.9383
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + Math.Round((decimal)singleTableResults.RiskRatioUpper, 4).ToString() + "<TT> (T)</TT></td>"); // 16.0296
                    HTMLString.AppendLine("    </tr>");
                    HTMLString.AppendLine("    <TR>");
                    HTMLString.AppendLine("        <TD class=\"stats\">Adjusted RR (MH)</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + computedRR.ToString("N4") + "</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + computedRRMHLL.ToString("N4") + ",</td>"); // 1.9868
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + computedRRMHUL.ToString("N4") + "<TT> (T)</TT></td>"); // 16.5127
                    HTMLString.AppendLine("    </tr>");
                    HTMLString.AppendLine("</TABLE>");
                    HTMLString.AppendLine("");
                    HTMLString.AppendLine("<p align=center><tt> (T=Taylor series; R=RGB; M=Exact mid-P; F=Fisher exact)</tt></P>");
                    HTMLString.AppendLine("");
                    HTMLString.AppendLine("<table cellspacing=10 align=center>");
                    HTMLString.AppendLine("    <TR>");
                    HTMLString.AppendLine("        <TD class=\"stats\"> STATISTICAL TESTS (overall association)</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\">Chi-square</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\">1-tailed p</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\">2-tailed p</td>");
                    HTMLString.AppendLine("    </tr>");
                    HTMLString.AppendLine("    <TR>");
                    HTMLString.AppendLine("        <TD class=\"stats\">MH Chi-square - uncorrected</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + uncorrectedChiSquare.ToString("N4") + "</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\"></td>");
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + uncorrectedChiSquareP.ToString("N4") + "</td>"); // 0.0000
                    HTMLString.AppendLine("    </tr>");
                    HTMLString.AppendLine("    <TR>");
                    HTMLString.AppendLine("        <TD class=\"stats\">MH Chi-square - corrected</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + corrChisq.ToString("N4") + "</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\"></td>");
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + corrChisqP.ToString("N4") + "</td>"); // 0.0000
                    HTMLString.AppendLine("    </tr>");
                    //HTMLString.AppendLine("    <TR>");
                    //HTMLString.AppendLine("        <TD class=\"stats\">Mid-p exact</td>");
                    //HTMLString.AppendLine("        <TD class=\"stats\"></td>");
                    //HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>N.I.</td>"); // 0.0000
                    //HTMLString.AppendLine("        <TD class=\"stats\"></td>");
                    //HTMLString.AppendLine("    </tr>");
                    //HTMLString.AppendLine("    <TR>");
                    //HTMLString.AppendLine("        <TD>Fisher exact</td><TD></td><TD ALIGN=RIGHT>0.0000</td><TD></td>");
                    //HTMLString.AppendLine("    </tr>");
                    HTMLString.AppendLine("</TABLE>");
                    HTMLString.AppendLine("<br>");
                    HTMLString.AppendLine("<table cellspacing=10 align=center>");
                    HTMLString.AppendLine("    <TR>");
                    HTMLString.AppendLine("        <TD class=\"stats\"> Homogeneity Tests</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\">Chi-square</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\">1-tailed p</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\">2-tailed p</td>");
                    HTMLString.AppendLine("    </tr>");
                    HTMLString.AppendLine("    <TR>");
                    HTMLString.AppendLine("        <TD class=\"stats\">Breslow-Day-Tarone test for Odds Ratio</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + bdtOR.ToString("N4") + "</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\"></td>");
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + bdtORP.ToString("N4") + "</td>"); // 0.0000
                    HTMLString.AppendLine("    </tr>");
                    HTMLString.AppendLine("    <TR>");
                    HTMLString.AppendLine("        <TD class=\"stats\">Breslow-Day test for Odds Ratio</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + bdOR.ToString("N4") + "</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\"></td>");
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + bdORP.ToString("N4") + "</td>"); // 0.0000
                    HTMLString.AppendLine("    </tr>");
                    HTMLString.AppendLine("    <TR>");
                    HTMLString.AppendLine("        <TD class=\"stats\">Breslow-Day test for Risk Ratio</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + bdRR.ToString("N4") + "</td>");
                    HTMLString.AppendLine("        <TD class=\"stats\"></td>");
                    HTMLString.AppendLine("        <TD class=\"stats\" ALIGN=RIGHT>" + bdRRP.ToString("N4") + "</td>"); // 0.0000
                    HTMLString.AppendLine("    </tr>");
                    HTMLString.AppendLine("</TABLE>");
                }
                catch (Exception ex)
                {
                    // do nothing for now
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(StrataVarName))
                {
                    StrataVarNameList.Add(StrataVarName);
                    StrataVarValueList.Add(StrataVar_Value);
                }

                DataTable workingTable = cWorkingTable.CreateWorkingTable(this.Exposure, this.Outcome, config, DT, StrataVarNameList, StrataVarValueList, this.WeightVar);

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

                if (!string.IsNullOrEmpty(this.OutTable))
                {
                    BuildOutTable(workingTable, StrataVarNameList, StrataVarValueList, OutDataTable);
                }
                this.Execute_CrossTab(HTMLString, this.Exposure, this.Outcome, StrataVarNameList, StrataVarValueList, config, workingTable);
            }

            if (!string.IsNullOrEmpty(this.OutTable))
            {
                OutDataTable.TableName = this.OutTable;
                this.Context.OutTable(OutDataTable);
            }

            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("COMMANDNAME", "SUMMARIZE");
            args.Add("COMMANDTEXT", commandText.Trim());
            args.Add("MEANSVARIABLE", this.Exposure);
            args.Add("HTMLRESULTS", HTMLString.ToString());

            this.Context.Display(args);
        }

        /// <summary>
        /// performs execution of the MEANS command
        /// </summary>
        /// <returns>object</returns>
        private void BuildOutTable(DataTable workingTable, List<string> StrataVarNameList, List<string> StrataVarValueList, DataTable OutDataTable)
        {
            //for each data row in workingTable
            //    for each column that is not __Values__ or __Count__ 
            //        add a new row to the OutDataTable which has a column for:
            //          * each strata variable
            //          * exposure
            //          * outcome
            //          * VARNAME = "ExposureVar:OutcomeVar"
            //          * COUNT = the total # of cases that match the strata, exposure, and outcome for that row

            foreach (DataRow wtRow in workingTable.Rows)
            {
                foreach (DataColumn wtColumn in workingTable.Columns)
                {
                    if (!(wtColumn.ColumnName.Equals("__Values__") || wtColumn.ColumnName.Equals("__Count__")))
                    {
                        DataRow thisOutRow = OutDataTable.NewRow();
                        foreach (string thisStrata in StrataVarNameList)
                        {
                            thisOutRow[thisStrata] = StrataVarValueList[StrataVarNameList.IndexOf(thisStrata)];
                        }
                        thisOutRow[this.Exposure] = wtRow["__Values__"];
                        thisOutRow[this.Outcome] = wtColumn.ColumnName.ToString();
                        thisOutRow["VARNAME"] = this.Exposure.ToString() + ":" + this.Outcome.ToString();
                        thisOutRow["COUNT"] = wtRow[wtColumn.ColumnName];
                        OutDataTable.Rows.Add(thisOutRow);
                    }
                }
            }
        }

        private void Execute_CrossTab(StringBuilder pHTMLString, string pNumericVariable, string pCrossTabVariable, List<string> pStratavar, List<string> pStrataValue, Dictionary<string, string> config, DataTable DT)
        {
            DataRow[] ROWS;
            string SelectedStatement;

            if (config["include-missing"].ToUpperInvariant() == "FALSE")
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

            double Sum = 0.0;
            double Sum_Sqr = 0.0;

            object Mode = 0.0;
            double ModeCount = 0.0;
            double mean = 0.0;
            double variance = 0.0;
            //int n = 0;
            double std_dev = 0.0;
            double Total = 0;
            object Min = null;
            object Max = null;


            bool twobytwo = false;
            double yy = 0; // yes/yes cell
            double yn = 0; // yes/no cell
            double ny = 0; // no/yes cell
            double nn = 0; // no/no cell
            double rnt = 0; // right no total
            double ryt = 0; // right yes total

            double bnt = 0; // bottom no total
            double byt = 0; // bottom yes total

            double tt = 0; // total


            System.Collections.Generic.Dictionary<string, double> ColumnTotalSet = new Dictionary<string, double>();
            IEnumerable<DataColumn> SortedColumns = null;
            StringBuilder RowPercent = new StringBuilder();
            StringBuilder ColPercent = new StringBuilder();

            double AccumulatedTotal = 0;

            int i = 0;
            foreach (System.Data.DataRow R in ROWS)
            {
                object temp = R["__Values__"];
                //double.TryParse(R["__Value__"].ToString(), out temp);
                double currrentCount;

                double.TryParse(R["__Count__"].ToString(), out currrentCount);

                if (currrentCount > ModeCount)
                {
                    ModeCount = currrentCount;
                    Mode = temp;
                }

                AccumulatedTotal += currrentCount;
                obs += currrentCount;
                //Total += currrentCount;
                if (i == 0 && temp != System.DBNull.Value)
                {
                    Max = Min = temp;
                }
                else
                {
                    if (temp != System.DBNull.Value)
                    {
                        int compare = ((IComparable)temp).CompareTo(Max);
                        if (compare > 0)
                        {
                            Max = temp;
                        }

                        compare = ((IComparable)temp).CompareTo(Min);
                        if (compare < 0)
                        {
                            Min = temp;
                        }
                    }
                }
                i++;

                foreach (DataColumn C in DT.Columns)
                {
                    if (C.ColumnName != "__Values__" && C.ColumnName != "__Count__")
                    {
                        double ColVal = 0;
                      //  double.TryParse(R[C.ColumnName.ToUpperInvariant()].ToString(), out ColVal);
                        double.TryParse(R[C.ColumnName].ToString(), out ColVal);

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

            mean = Sum / obs;
            variance = (Sum_Sqr - Sum * mean) / (obs - 1);
            std_dev = Math.Sqrt(variance);

            /*if (
                DT.Columns.Count == 4 && ROWS.Length == 2 ||
                (DT.Columns.Count == 4 && ROWS.Length == 2 && (this.Context.Columns[this.Outcome].DataType.ToString() == "System.Byte" || this.Context.Columns[this.Outcome].DataType.ToString() == "System.Boolean")) ||
                ((DT.Columns.Count == 4 || DT.Columns.Count == 5) && (ROWS.Length == 2 || ROWS.Length == 3) && (this.Context.Columns[this.Outcome].DataType.ToString() == "System.Byte" || this.Context.Columns[this.Outcome].DataType.ToString() == "System.Boolean")) && (config["include-missing"].ToUpperInvariant() == "TRUE" && this.Context.EpiViewVariableList.Count > 0)
                )*/
            if (DT.Columns.Count == 4 && ROWS.Length == 2 )
            {
                twobytwo = true;
            }

            string SelectOrder = "";
            
            SortedColumns = DT.Columns.Cast<DataColumn>().OrderBy(x => { return x.ColumnName; });

            if (DT.Columns.Count == 4 && ROWS.Length == 2)
            {
                if ((this.Context.Columns[this.Outcome].DataType.ToString() == "System.Byte" && DT.Columns[2].ColumnName == "0" && DT.Columns[3].ColumnName == "1") || this.Context.Columns[this.Outcome].DataType.ToString() == "System.Boolean")
                {
                    SortedColumns = DT.Columns.Cast<DataColumn>().OrderByDescending(x => { return x.ColumnName; });                    
                }

                if ((this.Context.Columns[this.Exposure].DataType.ToString() == "System.Byte") || this.Context.Columns[this.Exposure].DataType.ToString() == "System.Boolean")
                {
                    SelectOrder = "__Values__ desc";
                }
            }
            else if ((DT.Columns.Count == 4 || DT.Columns.Count == 5) && (ROWS.Length == 2 || ROWS.Length == 3) && (config["include-missing"].ToUpperInvariant() == "TRUE" && this.Context.EpiViewVariableList.Count > 0))
            {
                int colOffset = DT.Columns.Count;

                if ((this.Context.Columns[this.Outcome].DataType.ToString() == "System.Byte" && DT.Columns[colOffset - 2].ColumnName == "0" && DT.Columns[colOffset - 1].ColumnName == "1") || this.Context.Columns[this.Outcome].DataType.ToString() == "System.Boolean")
                {
                    SortedColumns = DT.Columns.Cast<DataColumn>().OrderByDescending(x => { return x.ColumnName; });

                    DataColumn missingColumn = null;

                    foreach (DataColumn dc in SortedColumns)
                    {
                        if (dc.ColumnName.Equals(this.config.Settings.RepresentationOfMissing))
                        {
                            missingColumn = dc;
                            break;
                        }
                    }

                    if (missingColumn != null)
                    {
                        var columns = from dc in SortedColumns where dc.ColumnName != this.config.Settings.RepresentationOfMissing select dc;
                        columns = columns.ToList();
                        (columns as List<DataColumn>).Add(missingColumn);
                        SortedColumns = columns;
                    }
                }

                int rowOffset = ROWS.Length;

                if ((this.Context.Columns[this.Exposure].DataType.ToString() == "System.Byte" && DT.Rows[rowOffset - 2][0].ToString() == "0" && DT.Rows[rowOffset - 1][0].ToString() == "1") || this.Context.Columns[this.Exposure].DataType.ToString() == "System.Boolean")
                {
                    SelectOrder = "__Values__ desc";
                }
            }

            if (pStratavar != null && pStratavar.Count > 0)
            {
                pHTMLString.Append("<b>");
                for (int StratIndex = 0; StratIndex < pStratavar.Count; StratIndex++)
                {
                    pHTMLString.Append("&nbsp;");
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
                            else
                            {
                                PrintValue = pStrataValue[StratIndex];
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
                    pHTMLString.Append(GetPrintValue(pStratavar[StratIndex], this.Context.Columns[pStratavar[StratIndex]].DataType.ToString(), PrintValue, config));
                }
                pHTMLString.Append("</b><br/>");
            }

            pHTMLString.Append("<table align=\"left\">");
            pHTMLString.Append("<tr><th>&nbsp;</th><th colspan=");
            pHTMLString.Append(DT.Columns.Count - 2);
            pHTMLString.Append(">");
            if (this.Context.EpiViewVariableList.ContainsKey(this.Outcome) && config["Show-Prompts"].Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                pHTMLString.Append(this.Context.EpiViewVariableList[this.Outcome].Prompt);
            }
            else
            {
                pHTMLString.Append(this.Outcome);
            }
            pHTMLString.Append("</th><th>&nbsp;</th></tr>");
            pHTMLString.Append("<tr><th>");
            if (this.Context.EpiViewVariableList.ContainsKey(this.Exposure) && config["Show-Prompts"].Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                pHTMLString.Append(this.Context.EpiViewVariableList[this.Exposure].Prompt);
            }
            else
            {
                pHTMLString.Append(this.Exposure);
            }
            pHTMLString.Append("</th>");
            foreach (DataColumn C in SortedColumns)
            {
                if (C.ColumnName != "__Values__" && C.ColumnName != "__Count__")
                {
                    pHTMLString.AppendLine("<th>");
                    pHTMLString.Append(GetPrintValue(this.Outcome, this.Context.Columns[this.Outcome].DataType.Name, C.ColumnName, this.Context.SetProperties));
                    pHTMLString.Append("</th>");
                    //ColumnTotalSet.Add(C.ColumnName, 0);
                }
            }

            pHTMLString.AppendLine("<th>Total</th></tr>");

            
            AccumulatedTotal = 0;
            foreach (System.Data.DataRow R in DT.Select(SelectedStatement, SelectOrder))
            {
                RowPercent.Length = 0;
                ColPercent.Length = 0;
                double RowTotal = 0;// = (double)R["__Count__"];
                pHTMLString.Append("<tr><td><b>");
                //HTMLString.Append(R["__Values__"].ToString());
                GetPrintValue(pNumericVariable, R["__Values__"], config, pHTMLString);
                pHTMLString.Append("</b></td>");
               
                if (this.config.Settings.ShowPercents)
                {
                    RowPercent.Append("<tr><td>Row");
                    RowPercent.Append("%");
                    RowPercent.Append("</td>");
                }
                             
                if (this.config.Settings.ShowPercents)
                {
                    ColPercent.Append("<tr><td>Col");
                    ColPercent.Append("%");
                    ColPercent.Append("</td>");
                }
                if (this.config.Settings.ShowPercents)
                {
                }
                // value row
                foreach (DataColumn C in SortedColumns)
                {
                    if (C.ColumnName != "__Values__" && C.ColumnName != "__Count__")
                    {
                        double ColVal = 0;
                       // double.TryParse(R[C.ColumnName.ToUpperInvariant()].ToString(), out ColVal);
                        double.TryParse(R[C.ColumnName].ToString(), out ColVal);
                        RowTotal += ColVal;
                        pHTMLString.Append("<td align=\"right\">");
                        pHTMLString.Append(GetPrintValue(pCrossTabVariable, ColVal.ToString(), this.Context.SetProperties));
                        //add COUNT to newOutRow
                        pHTMLString.Append("</td>");
                    }
                }

                // percent row
                foreach (DataColumn C in SortedColumns)
                {
                    if (C.ColumnName != "__Values__" && C.ColumnName != "__Count__")
                    {
                        double ColVal = 0;
                       // double.TryParse(R[C.ColumnName.ToUpperInvariant()].ToString(), out ColVal);
                        double.TryParse(R[C.ColumnName].ToString(), out ColVal);
                        if (this.config.Settings.ShowPercents)
                        {
                            RowPercent.Append("<td align=\"right\">");
                            RowPercent.Append(GetPrintValue(pCrossTabVariable, ConvertToPercent(ColVal / RowTotal).ToString(), this.Context.SetProperties));
                            RowPercent.Append("</td>");


                            ColPercent.Append("<td align=\"right\">");
                            ColPercent.Append(GetPrintValue(pCrossTabVariable, ConvertToPercent(ColVal / ColumnTotalSet[C.ColumnName]).ToString(), this.Context.SetProperties));
                            ColPercent.Append("</td>");
                        }
                    }
                }

                Total += RowTotal;
                pHTMLString.Append("<td align=\"right\">");
                pHTMLString.Append(RowTotal.ToString());
                pHTMLString.Append("</td></tr>");
                if (this.config.Settings.ShowPercents)
                {
                    RowPercent.Append("<td align=\"right\">" + ConvertToPercent(1) + "</td></tr>");
                    ColPercent.Append("<td align=\"right\">");
                    ColPercent.Append(GetPrintValue(pNumericVariable, ConvertToPercent(RowTotal / ColumnTotalSet.Sum(x => x.Value)).ToString(), this.Context.SetProperties));
                    ColPercent.AppendLine("</td></tr>");
                }


                pHTMLString.Append(RowPercent);
                pHTMLString.Append(ColPercent);
                
            }

            RowPercent.Length = 0;
            ColPercent.Length = 0;

            pHTMLString.AppendLine("<tr><td><b>TOTAL</b></td>");

          
            if (this.config.Settings.ShowPercents)
            {
                RowPercent.Append("<tr><td>Row");
                RowPercent.Append("%");
                RowPercent.Append("</td>");
            }
          

          
            if (this.config.Settings.ShowPercents)
            {
                ColPercent.Append("<tr><td>Col");
                ColPercent.Append("%");
                ColPercent.Append("</td>");
            }
          
            foreach (DataColumn C in SortedColumns)
            {
                if (C.ColumnName != "__Values__" && C.ColumnName != "__Count__")
                {
                    pHTMLString.Append("<td align=\"right\">");
                    if (ColumnTotalSet.ContainsKey(C.ColumnName))
                    {
                        pHTMLString.Append(GetPrintValue(pCrossTabVariable, ColumnTotalSet[C.ColumnName], this.Context.SetProperties));
                        pHTMLString.Append("</td>");
                        if (this.config.Settings.ShowPercents)
                        {
                            RowPercent.Append("<td align=\"right\">");
                            RowPercent.Append(GetPrintValue(pCrossTabVariable, ConvertToPercent(ColumnTotalSet[C.ColumnName] / Total).ToString(), this.Context.SetProperties));
                        }
                    }
                    else
                    {
                        if (this.config.Settings.ShowPercents)
                        {
                            pHTMLString.Append("0</td>");
                            RowPercent.Append("<td align=\"right\">0");
                        }
                    }
                    if (this.config.Settings.ShowPercents)
                    {
                        RowPercent.Append("</td>");

                        ColPercent.Append("<td align=\"right\">" + ConvertToPercent(1) + "</td>");
                    }
                }
            }

            pHTMLString.AppendLine("<td align=\"right\">");
            pHTMLString.Append(Total);
            pHTMLString.AppendLine("</td></tr>");
            if (this.config.Settings.ShowPercents)
            {
                RowPercent.AppendLine("<td align=\"right\">" + ConvertToPercent(1) + "</td></tr>");
                ColPercent.AppendLine("<td align=\"right\">" + ConvertToPercent(1) + "</td></tr>");
            }

            pHTMLString.Append(RowPercent);
            pHTMLString.Append(ColPercent);


            pHTMLString.Append("</table>");

            if (twobytwo && tablesShowStatistics)
            {
                DataRow[] SortedRows = DT.Select(SelectedStatement, SelectOrder);
                if (this.Context.Columns[this.Outcome].DataType == typeof(bool) || this.Context.Columns[this.Outcome].DataType == typeof(byte))
                {
                    yy = double.Parse(SortedRows[0][3].ToString());
                    yn = double.Parse(SortedRows[0][2].ToString());
                    ny = double.Parse(SortedRows[1][3].ToString());
                    nn = double.Parse(SortedRows[1][2].ToString());
                }
                else
                {
                    yy = double.Parse(SortedRows[0][2].ToString());
                    yn = double.Parse(SortedRows[0][3].ToString());
                    ny = double.Parse(SortedRows[1][2].ToString());
                    nn = double.Parse(SortedRows[1][3].ToString());
                }

                double expectedYY = ((yy + yn) * (yy + ny)) / (yy + yn + ny + nn);
                double expectedYN = ((yy + yn) * (yn + nn)) / (yy + yn + ny + nn);
                double expectedNY = ((ny + nn) * (yy + ny)) / (yy + yn + ny + nn);
                double expectedNN = ((ny + nn) * (yn + nn)) / (yy + yn + ny + nn);

                bool hasAnExpectedCountLessThanFive = expectedYY < 5 || expectedYN < 5 || expectedNY < 5 || expectedNN < 5;

                tt = yy + yn + ny + nn;

                yyList.Add(yy);
                ynList.Add(yn);
                nyList.Add(ny);
                nnList.Add(nn);

                StatisticsRepository.cTable.SingleTableResults singleTableResults;
                singleTableResults = new StatisticsRepository.cTable().SigTable((double)yy, (double)yn, (double)ny, (double)nn, 0.95);

                bool hasNPQLessThanFive = singleTableResults.LowNPQ < 5;

                if (string.IsNullOrEmpty(singleTableResults.ErrorMessage))
                {
                    double yyPct = ((double)yy / (double)tt) * 160;
                    double ynPct = ((double)yn / (double)tt) * 160;
                    double nyPct = ((double)ny / (double)tt) * 160;
                    double nnPct = ((double)nn / (double)tt) * 160;

                    string fisherExact = Epi.SharedStrings.UNDEFINED;
                    string fisherExact2P = Epi.SharedStrings.UNDEFINED;
                    string fisherLower = Epi.SharedStrings.UNDEFINED;
                    string fisherUpper = Epi.SharedStrings.UNDEFINED;

                    string oddsRatioEstimate = Epi.SharedStrings.UNDEFINED;
                    string oddsRatioLower = Epi.SharedStrings.UNDEFINED;
                    string oddsRatioUpper = Epi.SharedStrings.UNDEFINED;

                    string oddsRatioMLEEstimate = Epi.SharedStrings.UNDEFINED;
                    string oddsRatioMLEMidPLower = Epi.SharedStrings.UNDEFINED;
                    string oddsRatioMLEMidPUpper = Epi.SharedStrings.UNDEFINED;

                    string riskRatioEstimate = Epi.SharedStrings.UNDEFINED;
                    string riskRatioLower = Epi.SharedStrings.UNDEFINED;
                    string riskRatioUpper = Epi.SharedStrings.UNDEFINED;

                    if (singleTableResults.FisherExactP != -1)
                    {
                        fisherExact = ((double)singleTableResults.FisherExactP).ToString("F10");
                    }

                    if (singleTableResults.FisherExact2P != -1)
                    {
                        fisherExact2P = ((double)singleTableResults.FisherExact2P).ToString("F10");
                    }

                    if (singleTableResults.OddsRatioMLEFisherLower != -1)
                    {
                        fisherLower = ((double)singleTableResults.OddsRatioMLEFisherLower).ToString("F4");
                    }

                    if (singleTableResults.OddsRatioMLEFisherUpper != -1)
                    {
                        fisherUpper = ((double)singleTableResults.OddsRatioMLEFisherUpper).ToString("F4");
                    }

                    if (singleTableResults.OddsRatioMLEEstimate != -1)
                    {
                        oddsRatioMLEEstimate = ((double)singleTableResults.OddsRatioMLEEstimate).ToString("F4");
                    }

                    if (singleTableResults.OddsRatioMLEMidPLower != -1)
                    {
                        oddsRatioMLEMidPLower = ((double)singleTableResults.OddsRatioMLEMidPLower).ToString("F4");
                    }

                    if (singleTableResults.OddsRatioMLEMidPUpper != -1)
                    {
                        oddsRatioMLEMidPUpper = ((double)singleTableResults.OddsRatioMLEMidPUpper).ToString("F4");
                    }

                    if (singleTableResults.OddsRatioEstimate != null)
                    {
                        oddsRatioEstimate = ((double)singleTableResults.OddsRatioEstimate).ToString("F4");
                    }

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

                    pHTMLString.AppendLine("<table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"padding-left: 50px;\">");
                    pHTMLString.AppendLine(" <tr>");
                    pHTMLString.AppendLine("  <td class=\"TwoByTwo\" valign=\"bottom\" align=\"right\"><div style=\"background-color: red; width:" + yyPct.ToString("F0") + "; height:" + yyPct.ToString("F0") + ";\"><!-- --></td>");
                    pHTMLString.AppendLine("  <td class=\"TwoByTwo\" valign=\"bottom\" align=\"left\"><div style=\"background-color: orange; width:" + ynPct.ToString("F0") + "; height:" + ynPct.ToString("F0") + ";\"><!-- --></td>");
                    pHTMLString.AppendLine(" </tr><tr>");
                    pHTMLString.AppendLine("  <td class=\"TwoByTwo\" valign=\"top\" align=\"right\"><div style=\"background-color: yellow; width:" + nyPct.ToString("F0") + "; height:" + nyPct.ToString("F0") + ";\"><!-- --></td>");
                    pHTMLString.AppendLine("  <td class=\"TwoByTwo\" valign=\"top\" align=\"left\"><div style=\"background-color: green; width:" + nnPct.ToString("F0") + "; height:" + nnPct.ToString("F0") + ";\"><!-- --></td>");
                    pHTMLString.AppendLine(" </tr></table>");
                    pHTMLString.AppendLine("<br clear=\"all\" />");

                    pHTMLString.AppendLine("<br clear=\"all\" /><h4 align=\"center\"> Single Table Analysis </h4>");
                    pHTMLString.AppendLine("<table align=\"center\">");
                    pHTMLString.AppendLine(" <tr><td class=\"stats\"></td><td class=\"stats\" align=\"center\">Point</td>");
                    pHTMLString.AppendLine("  <td class=\"stats\" colspan=\"2\" align=\"center\">95% Confidence Interval</td>");
                    pHTMLString.AppendLine(" <tr><td class=\"stats\"></td><td class=\"stats\" align=\"center\">Estimate<td class=\"stats\" align=\"right\">Lower<td class=\"stats\" align=\"right\">Upper</td></tr>");
                    pHTMLString.AppendLine(" <tr><td class=\"stats\">PARAMETERS: Odds-based</td><td class=\"stats\"></td><td class=\"stats\"></td><td class=\"stats\"></td></tr>");
                    pHTMLString.AppendLine(" <tr><td class=\"stats\">Odds Ratio (cross product)</td><td class=\"stats\" align=\"right\">" + oddsRatioEstimate + "</td><td class=\"stats\" align=\"right\">" + oddsRatioLower + "</td><td class=\"stats\" align=\"right\">" + oddsRatioUpper + "<tt> (T)</tt></td></tr>");
                    pHTMLString.AppendLine(" <tr><td class=\"stats\">Odds Ratio (MLE)</td><td class=\"stats\" align=\"right\">" + oddsRatioMLEEstimate + "</td><td class=\"stats\" align=\"right\">" + oddsRatioMLEMidPLower + "</td><td class=\"stats\" align=\"right\">" + oddsRatioMLEMidPUpper + "<tt> (M)</tt></td></tr>");
                    pHTMLString.AppendLine(" <tr><td class=\"stats\"></td><td class=\"stats\"></td><td class=\"stats\" align=\"right\">" + fisherLower + "</td><td class=\"stats\" align=\"right\">" + fisherUpper + "<tt> (F)</tt></td></tr>");
                    pHTMLString.AppendLine(" <tr><td class=\"stats\">PARAMETERS: Risk-based</td><td class=\"stats\"></td><td class=\"stats\"></td><td class=\"stats\"></td></tr>");
                    pHTMLString.AppendLine(" <tr><td class=\"stats\">Risk Ratio (RR)</td><td class=\"stats\" align=\"right\">" + riskRatioEstimate /*singleTableResults.RiskRatioEstimate.ToString("F4")*/ + "</td><td class=\"stats\" align=\"right\">" + /*singleTableResults.RiskRatioLower.ToString("F4")*/riskRatioLower + "</td><td class=\"stats\" align=\"right\">" + /*singleTableResults.RiskRatioUpper.ToString("F4")*/ riskRatioUpper + "<tt> (T)</tt></td></tr>");
                    pHTMLString.AppendLine(" <tr><td class=\"stats\">Risk Difference (RD%)</td><td class=\"stats\" align=\"right\">" + singleTableResults.RiskDifferenceEstimate.ToString("F4") + "</td><td class=\"stats\" align=\"right\">" + singleTableResults.RiskDifferenceLower.ToString("F4") + "</td><td class=\"stats\" align=\"right\">" + singleTableResults.RiskDifferenceUpper.ToString("F4") + "<tt> (T)</tt></td></tr>");
                    pHTMLString.AppendLine(" <tr /><tr /><tr /><tr /><tr />");
                    pHTMLString.AppendLine(" <tr> <td class=\"stats\" colspan=\"4\"><p align=\"center\"><tt> (T=Taylor series; C=Cornfield; M=Mid-P; F=Fisher Exact)</tt></p></tr>");
                    if (hasNPQLessThanFive)
                        pHTMLString.AppendLine(" <tr> <td class=\"stats\" colspan=\"4\"><p align=\"center\"><tt><b>Sparse data. Use exact confidence limits.</b></tt></p></tr>");
                    pHTMLString.AppendLine(" <tr /><tr /><tr /><tr /><tr />");
                    pHTMLString.AppendLine(" <tr><td class=\"stats\">STATISTICAL TESTS</td> <td class=\"stats\">Chi-square<td class=\"stats\">1-tailed p<td class=\"stats\">2-tailed p<tr><td class=\"stats\">Chi-square - uncorrected<td class=\"stats\" align=\"right\">" + singleTableResults.ChiSquareUncorrectedVal.ToString("F4") + "<td class=\"stats\"><td class=\"stats\" align=\"right\">" + singleTableResults.ChiSquareUncorrected2P.ToString("F10") + "</td></tr>");
                    pHTMLString.AppendLine(" <tr><td class=\"stats\">Chi-square - Mantel-Haenszel</td> <td class=\"stats\" align=\"right\">" + singleTableResults.ChiSquareMantelVal.ToString("F4") + "<td class=\"stats\"><td class=\"stats\" align=\"right\">" + singleTableResults.ChiSquareMantel2P.ToString("F10") + "</td></tr>");
                    pHTMLString.AppendLine(" <tr><td class=\"stats\">Chi-square - corrected (Yates)</td> <td class=\"stats\" align=\"right\">" + singleTableResults.ChiSquareYatesVal.ToString("F4") + "<td class=\"stats\"></td><td class=\"stats\" align=\"right\">" + singleTableResults.ChiSquareYates2P.ToString("F10") + "</td></tr>");
                    pHTMLString.AppendLine(" <tr><td class=\"stats\">Mid-p exact</td>  <td class=\"stats\"></td> <td class=\"stats\" align=\"right\">" + singleTableResults.MidP.ToString("F10") + "</td><td class=\"stats\"></td></tr>");
                    pHTMLString.AppendLine(" <tr><td class=\"stats\">Fisher exact</td> <td class=\"stats\"></td> <td class=\"stats\" align=\"right\">" + singleTableResults.FisherExactP.ToString("F10") + "</td><td class=\"stats\">" + singleTableResults.FisherExact2P.ToString("F10") + "</td></tr>");
                    if (hasAnExpectedCountLessThanFive)
                        pHTMLString.AppendLine(" <tr> <td class=\"stats\" colspan=\"4\"><p align=\"center\"><tt><b>At least one cell has expected size <5. Chi-square may not be a valid test.</b></tt></p></tr>");
                    pHTMLString.AppendLine("</table>");
                }

            }
            else if (twobytwo)
            {
                DataRow[] SortedRows = DT.Select(SelectedStatement, SelectOrder);
                if (this.Context.Columns[this.Outcome].DataType == typeof(bool) || this.Context.Columns[this.Outcome].DataType == typeof(byte))
                {
                    yy = double.Parse(SortedRows[0][3].ToString());
                    yn = double.Parse(SortedRows[0][2].ToString());
                    ny = double.Parse(SortedRows[1][3].ToString());
                    nn = double.Parse(SortedRows[1][2].ToString());
                }
                else
                {
                    yy = double.Parse(SortedRows[0][2].ToString());
                    yn = double.Parse(SortedRows[0][3].ToString());
                    ny = double.Parse(SortedRows[1][2].ToString());
                    nn = double.Parse(SortedRows[1][3].ToString());
                }

                tt = yy + yn + ny + nn;

                if (string.IsNullOrEmpty(singleTableResults.ErrorMessage))
                {
                    double yyPct = ((double)yy / (double)tt) * 160;
                    double ynPct = ((double)yn / (double)tt) * 160;
                    double nyPct = ((double)ny / (double)tt) * 160;
                    double nnPct = ((double)nn / (double)tt) * 160;

                    pHTMLString.AppendLine("<table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style=\"padding-left: 50px;\">");
                    pHTMLString.AppendLine(" <tr>");
                    pHTMLString.AppendLine("  <td class=\"TwoByTwo\" valign=\"bottom\" align=\"right\"><div style=\"background-color: red; width:" + yyPct.ToString("F0") + "; height:" + yyPct.ToString("F0") + ";\"><!-- --></td>");
                    pHTMLString.AppendLine("  <td class=\"TwoByTwo\" valign=\"bottom\" align=\"left\"><div style=\"background-color: orange; width:" + ynPct.ToString("F0") + "; height:" + ynPct.ToString("F0") + ";\"><!-- --></td>");
                    pHTMLString.AppendLine(" </tr><tr>");
                    pHTMLString.AppendLine("  <td class=\"TwoByTwo\" valign=\"top\" align=\"right\"><div style=\"background-color: yellow; width:" + nyPct.ToString("F0") + "; height:" + nyPct.ToString("F0") + ";\"><!-- --></td>");
                    pHTMLString.AppendLine("  <td class=\"TwoByTwo\" valign=\"top\" align=\"left\"><div style=\"background-color: green; width:" + nnPct.ToString("F0") + "; height:" + nnPct.ToString("F0") + ";\"><!-- --></td>");
                    pHTMLString.AppendLine(" </tr></table>");
                    pHTMLString.AppendLine("<br clear=\"all\" />");
                }

            }
            else if (tablesShowStatistics)
            {
                DataRow[] SortedRows = DT.Select(SelectedStatement, SelectOrder);
                double[] tableChiSq = Epi.Statistics.SingleMxN.CalcChiSq(SortedRows, true);
                double tableChiSqDF = (double)(SortedRows.Length - 1) * (SortedRows[0].ItemArray.Length - 3);
                double tableChiSqP = Epi.Statistics.SharedResources.PValFromChiSq(tableChiSq[0], tableChiSqDF);
                String disclaimer = "";
                if (tableChiSq[1] == 1.0)
                    disclaimer = "An expected value is < 1. Chi-squared may not be a valid test.";
                if (tableChiSq[1] == 5.0)
                    disclaimer = "An expected value is < 5. Chi-squared may not be a valid test.";
                pHTMLString.Append("<br clear=\"all\" />");
                pHTMLString.AppendLine("<br clear=\"all\" /><h4 align=\"center\"> Single Table Analysis </h4>");
                pHTMLString.AppendLine("<table align=\"center\">");
                pHTMLString.AppendLine(" <tr><td class=\"stats\" align=\"center\">Chi-Squared<td class=\"stats\" align=\"center\">df</td><td class=\"stats\" align=\"center\">Probability</td></tr>");
                if (!Double.IsNaN(tableChiSq[0]))
                    pHTMLString.AppendLine(" <tr><td class=\"stats\" align=\"center\">" + Math.Round(tableChiSq[0] * 10000) / 10000 + "</td><td class=\"stats\" align=\"center\">" + Math.Round(tableChiSqDF) + "</td><td class=\"stats\" align=\"center\">" + Math.Round(tableChiSqP * 10000) / 10000 + "</td></tr>");
                else
                    pHTMLString.AppendLine(" <tr><td class=\"stats\" align=\"center\">N/A</td><td class=\"stats\" align=\"center\">N/A</td><td class=\"stats\" align=\"center\">N/A</td></tr>");
                pHTMLString.AppendLine("</table>");
                pHTMLString.AppendLine("<h4 align=\"center\">" + disclaimer + "</h4>");
            }
            pHTMLString.Append("<br clear=\"all\" />");
            pHTMLString.Append("<br clear=\"all\" />");
            //pHTMLString.Append("<TABLE ALIGN=CENTER>");
            //pHTMLString.Append("<TR class='Stats'><TH  class='Stats' ALIGN=CENTER colspan=6>Descriptive Statistics for Each Value of Crosstab Variable</TH></TR>");
            //pHTMLString.Append("<TR class='Stats'><TD  class='Stats' ALIGN=CENTER>&nbsp;</TD><TD  class='Stats' ALIGN=CENTER>Obs</TD><TD class='Stats' ALIGN=CENTER>Total</TD> <TD class='Stats' ALIGN=CENTER>Mean</TD><TD class='Stats'>Variance</TD><TD class='Stats' ALIGN=CENTER>Std Dev</TD></TR>");
            //foreach (DataColumn C in SortedColumns)
            //{
            //    if (C.ColumnName != "__Values__" && C.ColumnName != "__Count__")
            //    {
            //        AddStats1(pHTMLString, DT, C.ColumnName);
            //    }
            //}

            //pHTMLString.Append("</TABLE>");
            //pHTMLString.Append("<TABLE ALIGN=CENTER>");
            //pHTMLString.Append("<TR><TD  class='Stats' ALIGN=CENTER>&nbsp;</TD><TD class='Stats' ALIGN=CENTER>Minimum</TD><TD class='Stats' ALIGN=CENTER>25%</TD><TD class='Stats' ALIGN=CENTER>Median</TD><TD class='Stats' ALIGN=CENTER>75%</TD><TD class='Stats' ALIGN=CENTER>Maximum</TD><TD class='Stats' ALIGN=CENTER>Mode</TD></TR>");
            //foreach (DataColumn C in SortedColumns)
            //{
            //    if (C.ColumnName != "__Values__" && C.ColumnName != "__Count__")
            //    {
            //        AddStats2(pHTMLString, DT, obs, C.ColumnName);
            //    }
            //}
            //pHTMLString.Append("</TABLE>");
            //pHTMLString.Append("<BR CLEAR=ALL/>");
            /*this.PrintDescriptiveStats(HTMLString);
            this.PrintANOVA(HTMLString);
            this.PrintBartlett(HTMLString);
            this.PrintKruskalWallis(HTMLString);*/
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


        private string GetPrintValue(string pFieldName, string pDataType, object pValue, Dictionary<string, string> pConfig)
        {
            string result = null;

            if (pValue == DBNull.Value)
            {
                result = pConfig["RepresentationOfMissing"];
            }
            else switch (pDataType.Replace("System.",""))
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
                        result = string.Format("{0:0.##}", pValue);
                        break;
                    default:
                        result = pValue.ToString();
                        break;
                }

            return result;
        }
        public void Execute_Old()
        {
            // READ 'C:\temp\Sample.mdb':Oswego
            // SUMMARIZE FirstVisit::MIN(VisitDate), LastVisit::MAX(VisitDate), NVisit::COUNT() TO PatSumm STRATAVAR=PatientID

            //dt = dsHelper.CreateGroupByTable("OrderSummary", ds.Tables["Orders"], "EmployeeID,sum(Amount) Total,min(Amount) Min,max(Amount) Max");

            //READ {Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\temp\Sample.mdb}:Addicts 
            //SUMMARIZE MinTime::MIN(Survival_Time_Days), MaxTime::MAX(Survival_Time_Days) TO PatSumm STRATAVAR=clinic
            // OutTable Structure Clinic, MinTime, MaxTime
            StringBuilder columnList = new StringBuilder();
            StringBuilder groupByList = new StringBuilder();
            cDataSetHelper dsHelper = new cDataSetHelper();
            System.Data.DataTable dt = null;


            columnList.Append(this.Exposure);
            columnList.Append(", ");

            columnList.Append(this.Outcome);
            columnList.Append(", ");


            groupByList.Append(this.Exposure);
            groupByList.Append(", ");

            groupByList.Append(this.Outcome);
            groupByList.Append(", ");

            if (this.StratvarList != null)
            {
                foreach (string s in this.StratvarList)
                {
                    groupByList.Append(s);
                    groupByList.Append(", ");
                    columnList.Append(s);
                    columnList.Append(", ");
                }
            }


            groupByList.Length = groupByList.Length - 2;

            columnList.Append("Count(");
            columnList.Append(this.Outcome);
            columnList.Append(") Outcome, ");

            columnList.Length = columnList.Length - 2;



            dt = dsHelper.CreateGroupByTable(this.OutTable, this.Context.Columns, this.Context.GetDataRows(null), columnList.ToString());

            dsHelper.InsertGroupByInto(dt, this.Context.Columns, this.Context.GetDataRows(null), columnList.ToString(), null, groupByList.ToString());

            if ((!string.IsNullOrEmpty(this.OutTable)) && (!string.IsNullOrEmpty(dt.TableName)))
            {
                this.Context.OutTable(dt);
            }

            StringBuilder HTMLString = new StringBuilder();
            Dictionary<string, string> config = this.Context.SetProperties;

            PrintTable(dt, HTMLString);

            
            //HTMLString.Append("<table><tr>");
            //foreach (DataColumn c in dt.Columns)
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
            //foreach (DataRow r in dt.Rows)
            //{
            //    HTMLString.Append("<tr>");
            //    foreach (DataColumn c in dt.Columns)
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
            /**/
            

            

            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("COMMANDNAME", "SUMMARIZE");
            args.Add("COMMANDTEXT", commandText.Trim());
            //args.Add("FREQVARIABLE", this.Identifier);
            args.Add("HTMLRESULTS", HTMLString.ToString());


            this.Context.Display(args);

        }

        private void PrintTable(DataTable pDT, StringBuilder pHTML)
        {
            Dictionary<string, long> ColHeaders = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, long> RowHeaders = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, Dictionary<string, long>> CellDetail = new Dictionary<string, Dictionary<string, long>>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, DataRow> RowList = new Dictionary<string, DataRow>(StringComparer.OrdinalIgnoreCase);


            //RowTotal = Foreach ColumnValue : Sum(RowValue)
            //ColumnTotal == Foreach Rowvalue : Sum(ColumnValue)
            
            bool twobytwo = false;
            


            long ColTotal = 0;

            foreach (DataRow R in pDT.Rows)
            {
                string ColKey = GetPrintValue(this.Exposure, R[this.Exposure], this.Context.SetProperties);
                string RowKey = GetPrintValue(this.Outcome, R[this.Outcome], this.Context.SetProperties);

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

            if (RowHeaders.Count == 2 && ColHeaders.Count == 2 && pDT.Rows.Count == 4)
            {
                twobytwo = true;
            }

            pHTML.Append("<table align=\"left\"><tr><th colspan='");
            pHTML.Append(RowHeaders.Count + 2);
            pHTML.Append("'>");
            pHTML.Append(this.Outcome);
            pHTML.Append("</th></tr><tr><th>");
            pHTML.Append(this.Exposure);
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
                if (config.Settings.ShowPercents)
                {
                    pHTML.Append("<br/>Row");
                    pHTML.Append("%");
                }               
                if (config.Settings.ShowPercents)
                {
                    pHTML.Append("<br/>Col");
                    pHTML.Append("%");
                  
                }
                pHTML.Append("</td>");
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

                pHTML.Append("<br/>" + ConvertToPercent(1) + "<br/>");
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

                string oddsRatioEstimate = SharedStrings.UNDEFINED;
                string oddsRatioLower = SharedStrings.UNDEFINED;
                string oddsRatioUpper = SharedStrings.UNDEFINED;
                string riskRatioEstimate = SharedStrings.UNDEFINED;
                string riskRatioLower = SharedStrings.UNDEFINED;
                string riskRatioUpper = SharedStrings.UNDEFINED;

                if (singleTableResults.OddsRatioEstimate != null)
                {
                    oddsRatioEstimate = ((double)singleTableResults.OddsRatioEstimate).ToString("F4");
                }

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
                pHTML.Append("<tr><td class=\"stats\">Risk Ratio (RR)</td><td class=\"stats\" align=\"right\">" + riskRatioEstimate /*singleTableResults.RiskRatioEstimate.ToString("F4")*/ + "</td><td class=\"stats\" align=\"right\">" + /*singleTableResults.RiskRatioLower.ToString("F4")*/riskRatioLower + "</td><td class=\"stats\" align=\"right\">" + /*singleTableResults.RiskRatioUpper.ToString("F4")*/riskRatioUpper + "<tt> (T)</tt></td></tr>");
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


        private void PrintTable(string StrataVariable, DataTable pDT, StringBuilder pHTML)
        {
            Dictionary<string, long> ColHeaders = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, long> RowHeaders = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, Dictionary<string, string>> StratValues = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, Dictionary<string, long>> CellDetail = new Dictionary<string, Dictionary<string, long>>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, DataRow> RowList = new Dictionary<string, DataRow>(StringComparer.OrdinalIgnoreCase);


            //RowTotal = Foreach ColumnValue : Sum(RowValue)
            //ColumnTotal == Foreach Rowvalue : Sum(ColumnValue)
            

            long ColTotal = 0;
            foreach (DataRow R in pDT.Rows)
            {
                string ColKey = GetPrintValue(this.Exposure, R[this.Exposure], this.Context.SetProperties);
                string RowKey = GetPrintValue(this.Outcome, R[this.Outcome], this.Context.SetProperties);

                if (ColHeaders.ContainsKey(ColKey))
                {
                    ColHeaders[ColKey] += Convert.ToInt32(R["Outcome"]);
                    ColTotal+= Convert.ToInt32(R["Outcome"]);
                }
                else
                {
                    ColHeaders.Add(ColKey, Convert.ToInt32(R["Outcome"]));
                    ColTotal+= Convert.ToInt32(R["Outcome"]);
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
                        CellDetail[ColKey][RowKey]+= Convert.ToInt32(R["Outcome"]); 
                    }
                    else
                    {
                        CellDetail[ColKey].Add(RowKey, Convert.ToInt32(R["Outcome"])); 
                    }
                }
                else
                {
                    CellDetail.Add(ColKey, new Dictionary<string,long>(StringComparer.OrdinalIgnoreCase));
                    CellDetail[ColKey].Add(RowKey, Convert.ToInt32(R["Outcome"])); 
                }
            }

            
            pHTML.Append("<table><tr><th colspan='");
            pHTML.Append(RowHeaders.Count + 2);
            pHTML.Append("'>");
            if (this.Context.EpiViewVariableList.ContainsKey(this.Outcome) && config.Settings.ShowCompletePrompt)
            {
                pHTML.Append(this.Context.EpiViewVariableList[this.Outcome].Prompt);
            }
            else
            {
                pHTML.Append(this.Outcome);
            }
            pHTML.Append("</th></tr><tr><th>");
            if (this.Context.EpiViewVariableList.ContainsKey(this.Exposure) && config.Settings.ShowCompletePrompt)
            {
                pHTML.Append(this.Context.EpiViewVariableList[this.Exposure].Prompt);
            }
            else
            {
                pHTML.Append(this.Exposure);
            }
            pHTML.Append("</th>");
            
            foreach (KeyValuePair<string, long> RH in RowHeaders)
            {
                pHTML.Append("<th>");
                pHTML.Append(RH.Key);
                pHTML.Append("</th>");
            }
            pHTML.Append("<th>total</th></tr>");

            foreach (KeyValuePair<string, long> CH in ColHeaders)
            {
                pHTML.Append("<tr><td>");
                pHTML.Append(CH.Key);
                pHTML.Append("<br/>Row%<br/>Col%</td>");

                foreach (KeyValuePair<string, long> RH in RowHeaders)
                {
                    pHTML.Append("<td>");
                    if (CellDetail.ContainsKey(CH.Key) && CellDetail[CH.Key].ContainsKey(RH.Key))
                    {
                        GetPrintValue(CH.Key, CellDetail[CH.Key][RH.Key], this.Context.SetProperties, pHTML);
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
                pHTML.Append("<br/>" + ConvertToPercent(1) + "<br/>");
                GetPrintValue(CH.Key, Convert.ToDouble(CH.Value) / Convert.ToDouble(ColTotal) * 100.0, this.Context.SetProperties, pHTML);
                pHTML.Append("</td>");

                pHTML.Append("</tr>");
            }


            pHTML.Append("<tr><td>");
            pHTML.Append("total");
            pHTML.Append("</td>");

            long TotalRows = 0;
            foreach (KeyValuePair<string, long> RH in RowHeaders)
            {
                pHTML.Append("<td>");
                TotalRows += RH.Value;
                pHTML.Append(RH.Value.ToString());
                
                pHTML.Append("</td>");
            }
            pHTML.Append("<td>");
            pHTML.Append(TotalRows.ToString());
            pHTML.Append("</td>");
            pHTML.Append("</table>");


        }


        private void GetPrintValue(string pFieldName, object pValue, Dictionary<string, string> pConfig, StringBuilder pBuilder)
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
                        pBuilder.Append(string.Format("{0:0.##########}", pValue));
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
                        result = string.Format("{0:0.##}", pValue);
                        break;
                    default:
                        result = pValue.ToString();
                        break;
                }

            return result;
        }

        private bool isMoreLeft(DataTable[] pDataTable, int[] pCurrentIndexes)
        {
            bool result = true;

            if (pCurrentIndexes[0] > pDataTable[0].Rows.Count - 1)
            {
                result = false;
            }

            return result;
        }

        private double PValFromChiSq(double _x, double _df)
        {
            double _j;
            double _k;
            double _l;
            double _m;
            double _pi = 3.1416;

            if (_x < 0.000000001 || _df < 1.0)
                return 1.0;

            double _rr = 1.0;
            int _ii = (int)_df;

            while (_ii >= 2)
            {
                _rr = _rr * (double)_ii;
                _ii = _ii - 2;
            }

            _k = Math.Exp(Math.Floor((_df + 1.0) * 0.5) * Math.Log(Math.Abs(_x)) - _x * 0.5) / _rr;

            if (_k < 0.00001)
                return 0.0;

            if (Math.Floor(_df * 0.5) == _df * 0.5)
                _j = 1.0;
            else
                _j = Math.Sqrt(2.0 / _x / _pi);

            _l = 1.0;
            _m = 1.0;

            if (!double.IsNaN(_x) && !double.IsInfinity(_x))
            {
                while (_m >= 0.00000001)
                {
                    _df = _df + 2.0;
                    _m = _m * _x / _df;
                    _l = _l + _m;
                }
            }
            double PfX2 = 1 - _j * _k * _l;
            return PfX2;
        }

        private double executeCochran(string expVar, string outcVar, string TableName, DataTable SourceTable, string FieldName, List<double> yyL, List<string> StratVarList, List<string> StratValueList, string wgtvar)
        {
            double A = 0.0;
            double T = 0.0;
            double S = 0.0;
            int wgtIndex = 0;
            List<string> allVariables = new List<string>();
            allVariables.Add(expVar);
            allVariables.Add(outcVar);
            for (int i = 0; i < StratVarList.Count; i++)
            {
                allVariables.Add(StratVarList[i]);
            }

            int columns = (int)(Math.Log10((double)yyL.Count) / Math.Log10(2.0)) + 2;

            DataTable dt = new DataTable(TableName);
            dt.Columns.Add(FieldName, SourceTable.Columns[FieldName].DataType);

            List<double> totals = new List<double>();
            List<double> successes = new List<double>();

            for (int i = 0; i < columns; i++)
            {
                totals.Add(0.0);
                successes.Add(0.0);
            }

            foreach (DataRow dr in SourceTable.Select("", FieldName))
            {
                if (wgtvar != null)
                {
                    for (int i = 0; i < dr.Table.Columns.Count; i++)
                    {
                        if ((dr.Table.Columns[i].ToString()).Equals(wgtvar))
                        {
                            wgtIndex = i;
                        }
                    }
                }
                int rowsuccesses = 0;
                int totalsindex = 0;
                for (int i = 0; i < dr.Table.Columns.Count; i++)
                {
                    for (int j = 0; j < allVariables.Count; j++)
                    {
                        if ((dr.Table.Columns[i].ToString().ToUpperInvariant()).Equals(allVariables[j].ToUpperInvariant()))
                        {
                            if (!dr[i].Equals(StratValueList[0]))
                            {
                                if (wgtvar != null)
                                {
                                    totals[totalsindex] = totals[totalsindex] + (double)dr[wgtIndex];
                                }
                                else totals[totalsindex]++;
                                rowsuccesses++;
                            }
                            totalsindex++;
                        }
                    }
                }
                if (rowsuccesses != 0)
                {
                    if (wgtvar != null)
                    {
                        successes[columns - rowsuccesses] += (double)dr[wgtIndex];
                    }
                    else successes[columns - rowsuccesses]++;
                }
            }
            for (int i = 0; i < totals.Count; i++)
            {
                A = A + Math.Pow(totals[i], 2);
                T = T + totals[i];
                S = S + successes[i] * Math.Pow((totals.Count - i), 2);
            }
            double cochranQ = (totals.Count * (totals.Count - 1) * A - (totals.Count - 1) * Math.Pow(T, 2)) / (totals.Count * T - S);
            return cochranQ;
        }

        private void AddOutColumn (string ColToAdd, DataTable OutputTable)
        {
            DataColumn newOutColumn = new DataColumn(ColToAdd);
            switch (ColToAdd)
            {
                case "VARNAME":
                    newOutColumn.DataType = typeof(string);
                    break;
                case "COUNT":
                    newOutColumn.DataType = typeof(double);
                    break;
                default:
                    newOutColumn.DataType = this.Context.Columns[ColToAdd].DataType;
                    break;
            }
            OutputTable.Columns.Add(newOutColumn);
        }

        private void InitOutTable(DataTable ODT)
        {
            if (this.StratvarList != null && this.StratvarList.Length > 0)
            {
                foreach (string s in this.StratvarList)
                {
                    AddOutColumn(s, ODT);
                }
            }
            //needs to accomodate an Exposure list, *, or group field
            AddOutColumn(this.Exposure, ODT);
            AddOutColumn(this.Outcome, ODT);
            AddOutColumn("VARNAME", ODT);
            AddOutColumn("COUNT", ODT);
        }

        private void IncrementArray(DataTable[] pDataTable, int[] pCurrentIndexes)
        {
            bool ContinueProcessing = true;

            for (int i = pCurrentIndexes.Length - 1; i > -1 && ContinueProcessing; i--)
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

    public class CaseInsensitiveComparer : IComparer<KeyValuePair<string, long>>
    {
        public int Compare(KeyValuePair<string, long> x, KeyValuePair<string, long> y)
        {
            return string.Compare(x.Key, y.Key, true);
        }
    }

}
