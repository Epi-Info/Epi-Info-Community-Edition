using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EpiDashboard.Controls
{
    /// <summary>
    /// Interaction logic for StratifiedTableAnalysisPanel.xaml
    /// </summary>
    public partial class StratifiedTableAnalysisPanel : UserControl
    {
        List<double> yyList = new List<double>();
        List<double> ynList = new List<double>();
        List<double> nyList = new List<double>();
        List<double> nnList = new List<double>();

        public StratifiedTableAnalysisPanel()
        {
            InitializeComponent();
        }

        public List<double> YesYesList
        {
            get
            {
                return this.yyList;
            }
            set
            {
                this.yyList = value;
            }
        }

        public List<double> YesNoList
        {
            get
            {
                return this.ynList;
            }
            set
            {
                this.ynList = value;
            }
        }

        public List<double> NoYesList
        {
            get
            {
                return this.nyList;
            }
            set
            {
                this.nyList = value;
            }
        }

        public List<double> NoNoList
        {
            get
            {
                return this.nnList;
            }
            set
            {
                this.nnList = value;
            }
        }

        public void Compute()
        {
            if (YesYesList.Count >= 2 && YesNoList.Count == YesYesList.Count && NoYesList.Count == YesYesList.Count && NoNoList.Count == YesYesList.Count)
            {
                System.ComponentModel.BackgroundWorker worker = new System.ComponentModel.BackgroundWorker();
                worker.DoWork += new System.ComponentModel.DoWorkEventHandler(strataTwoByTwoWorker_DoWork);
                worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(strataTwoByTwoWorker_RunWorkerCompleted);
                worker.RunWorkerAsync();
            }
        }

        public string ToHTML()
        {
            StringBuilder htmlBuilder = new StringBuilder();

            htmlBuilder.AppendLine("<center><strong>SUMMARY INFORMATION</strong></center>"); // TODO: Remove horrible <center> tag, replace with proper HTML

            htmlBuilder.AppendLine("<table class=\"noborder\" cellspacing=10 align=center>");
            htmlBuilder.AppendLine("    <tr>");
            htmlBuilder.AppendLine("        <td class=\"noborder\"></td>");
            htmlBuilder.AppendLine("        <td class=\"noborder\">Point</td>");
            htmlBuilder.AppendLine("        <td class=\"noborder\" colspan=\"2\">95%Confidence Interval</td>");
            htmlBuilder.AppendLine("    </tr>");
            htmlBuilder.AppendLine("    <tr>");
            htmlBuilder.AppendLine("        <td class=\"noborder\">Parameters</td>");
            htmlBuilder.AppendLine("        <td class=\"noborder\" align=\"right\">Estimate</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>Lower</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>Upper</td>");
            htmlBuilder.AppendLine("    <TR>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\">Odds Ratio Estimates</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\"></td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\"></td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\"></td>");
            htmlBuilder.AppendLine("    <TR>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\">Crude OR (cross product)</td>");          
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratCrudeOr.Text + "</td>"); // 23.4545
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratCrudeOrLower.Text + ",</td>"); // 5.8410
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratCrudeOrUpper.Text + "<TT> (T)</TT></td>"); //94.1811
            htmlBuilder.AppendLine("    </tr>");
            htmlBuilder.AppendLine("    <TR>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\">Crude (MLE)</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratCrudeMle.Text + "</td>"); //22.1490
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratCrudeMleLower.Text + ",</td>"); // 5.9280
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratCrudeMleUpper.Text + "<TT> (M)</TT></td>"); // 109.1473
            htmlBuilder.AppendLine("    </tr>");
            htmlBuilder.AppendLine("    <TR>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\"></td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT></td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratFisherLower.Text + ",</td>"); // 5.2153
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratFisherUpper.Text + "<TT> (F)</TT></td>"); // 138.3935
            htmlBuilder.AppendLine("    </tr>");
            htmlBuilder.AppendLine("    <TR>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\">Adjusted OR (MH)</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratAdjustedOr.Text + "</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratAdjustedOrLower.Text + ",</td>"); // 7.1914
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratAdjustedOrUpper.Text + "<TT> (R)</TT></td>"); // 197.0062
            htmlBuilder.AppendLine("    </tr>");
            htmlBuilder.AppendLine("    <TR>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\">Adjusted OR (MLE)</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratAdjustedMle.Text + "</td>"); // 28.5753
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratAdjustedMleLower.Text + ",</td>"); // 6.9155
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratAdjustedMleUpper.Text + "<TT> (F)</TT></td>"); // 160.2216
            htmlBuilder.AppendLine("    </tr>");
            htmlBuilder.AppendLine("    <TR>");
            //htmlBuilder.AppendLine("        <TD class=\"noborder\"></td>");
            //htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT></td>");
            //htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT> N.I.,</td>"); // 6.0084
            //htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>N.I.<TT> (F)</TT></td>"); // 207.5014
            //htmlBuilder.AppendLine("    </tr>");
            //htmlBuilder.AppendLine("    <TR>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\">Risk Ratios (RR)</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\"></td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\"></td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\"></td>");
            htmlBuilder.AppendLine("    </tr>");
            htmlBuilder.AppendLine("    <TR>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\">Crude Risk Ratio (RR)</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratCrudeRr.Text + "</td>"); // 5.5741
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratCrudeRrLower.Text + ",</td>"); // 1.9383
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratCrudeRrUpper.Text + "</td>"); // 16.0296
            htmlBuilder.AppendLine("    </tr>");
            htmlBuilder.AppendLine("    <TR>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\">Adjusted RR (MH)</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratAdjustedRr.Text + "</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratAdjustedRrLower.Text + ",</td>"); // 1.9868
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratAdjustedRrUpper.Text + "</td>"); // 16.5127
            htmlBuilder.AppendLine("    </tr>");
            htmlBuilder.AppendLine("</TABLE>");
            htmlBuilder.AppendLine("");
            htmlBuilder.AppendLine("<p align=center><tt> (T=Taylor series; R=RGB; M=Exact mid-P; F=Fisher exact)</tt></P>");
            htmlBuilder.AppendLine("");
            htmlBuilder.AppendLine("<table class=\"noborder\" cellspacing=10 align=center>");
            htmlBuilder.AppendLine("    <TR>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\"> STATISTICAL TESTS (overall association)</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\">Chi-square</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\">1-tailed p</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\">2-tailed p</td>");
            htmlBuilder.AppendLine("    </tr>");
            htmlBuilder.AppendLine("    <TR>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\">MH Chi-square - uncorrected</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratChiUnc.Text + "</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\"></td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratChiUnc2Tail.Text + "</td>"); // 0.0000
            htmlBuilder.AppendLine("    </tr>");
            htmlBuilder.AppendLine("    <TR>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\">MH Chi-square - corrected</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratChiCor.Text + "</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\"></td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratChiCor2Tail.Text + "</td>"); // 0.0000
            htmlBuilder.AppendLine("    </tr>");
            htmlBuilder.AppendLine("</TABLE>");
            htmlBuilder.AppendLine("<br>");
            htmlBuilder.AppendLine("<table class=\"noborder\" cellspacing=10 align=center>");
            htmlBuilder.AppendLine("    <TR>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\"> Homogeneity Tests</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\">Chi-square</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\">1-tailed p</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\">2-tailed p</td>");
            htmlBuilder.AppendLine("    </tr>");
            htmlBuilder.AppendLine("    <TR>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\">Breslow-Day-Tarone test for Odds Ratios</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratBDT.Text + "</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\"></td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratBDT2Tail.Text + "</td>"); // 0.0000
            htmlBuilder.AppendLine("    </tr>");
            htmlBuilder.AppendLine("    <TR>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\">Breslow-Day test for Odds Ratios</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratBDOR.Text + "</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\"></td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratBDOR2Tail.Text + "</td>"); // 0.0000
            htmlBuilder.AppendLine("    </tr>");
            htmlBuilder.AppendLine("    <TR>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\">Breslow-Day test for Risk Ratios</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratBDRR.Text + "</td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\"></td>");
            htmlBuilder.AppendLine("        <TD class=\"noborder\" ALIGN=RIGHT>" + txtStratBDRR2Tail.Text + "</td>"); // 0.0000
            htmlBuilder.AppendLine("    </tr>");
            htmlBuilder.AppendLine("</TABLE>");

            return htmlBuilder.ToString();
        }

        private void strataTwoByTwoWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            //List<double>[] lists = (List<double>[])e.Argument;

            //List<double> yyList = lists[0];
            //List<double> ynList = lists[1];
            //List<double> nyList = lists[2];
            //List<double> nnList = lists[3];

            double[] yyArr = yyList.ToArray();
            double[] ynArr = ynList.ToArray();
            double[] nyArr = nyList.ToArray();
            double[] nnArr = nnList.ToArray();

            double yySum = yyList.Sum();
            double ynSum = ynList.Sum();
            double nySum = nyList.Sum();
            double nnSum = nnList.Sum();

            StatisticsRepository.Strat2x2 strat2x2 = new StatisticsRepository.Strat2x2();
            StatisticsRepository.cTable.SingleTableResults singleTableResults = new StatisticsRepository.cTable().SigTable(yySum, ynSum, nySum, nnSum, 0.95);

            double computedOddsRatio = (double)strat2x2.ComputeOddsRatio(yyArr, ynArr, nyArr, nnArr);
            double computedOddsRatioMHLL = computedOddsRatio * Math.Exp(-(double)strat2x2.ZSElnOR(yyArr, ynArr, nyArr, nnArr));
            double computedOddsRatioMHUL = computedOddsRatio * Math.Exp((double)strat2x2.ZSElnOR(yyArr, ynArr, nyArr, nnArr));
            double computedRR = (double)strat2x2.ComputedRR(yyArr, ynArr, nyArr, nnArr);
            double computedRRMHLL = computedRR * Math.Exp(-(double)strat2x2.ZSElnRR(yyArr, ynArr, nyArr, nnArr));
            double computedRRMHUL = computedRR * Math.Exp((double)strat2x2.ZSElnRR(yyArr, ynArr, nyArr, nnArr));
            //                double mleOR = (double)strat2x2.ucestimaten(yyArr, ynArr, nyArr, nnArr);
            //                double ExactORLL = (double)strat2x2.exactorln(yyArr, ynArr, nyArr, nnArr);
            //                double ExactORUL = (double)strat2x2.exactorun(yyArr, ynArr, nyArr, nnArr);
            double mleOR = double.NaN;
            double ExactORLL = double.NaN;
            double ExactORUL = double.NaN;
            if (ynSum == 0.0 || nySum == 0.0)
            {
                mleOR = double.PositiveInfinity;
                ExactORLL = (double)strat2x2.exactorln(yyArr, ynArr, nyArr, nnArr);
                ExactORUL = double.PositiveInfinity;
            }
            else if (yySum == 0.0 || nnSum == 0.0)
            {
                mleOR = 0.0;
                ExactORLL = 0.0;
                ExactORUL = (double)strat2x2.exactorun(yyArr, ynArr, nyArr, nnArr, ref mleOR);
            }
            else
            {
                mleOR = (double)strat2x2.ucestimaten(yyArr, ynArr, nyArr, nnArr);
                ExactORLL = (double)strat2x2.exactorln(yyArr, ynArr, nyArr, nnArr);
                ExactORUL = (double)strat2x2.exactorun(yyArr, ynArr, nyArr, nnArr, ref mleOR);
            }
            double uncorrectedChiSquare = strat2x2.ComputeUnChisq(yyArr, ynArr, nyArr, nnArr);
            double corrChisq = strat2x2.ComputeCorrChisq(yyArr, ynArr, nyArr, nnArr);
            double uncorrectedChiSquareP = (double)strat2x2.pForChisq(uncorrectedChiSquare);
            double corrChisqP = (double)strat2x2.pForChisq(corrChisq); string[] results = new string[30];
            double bdtOR = Epi.Statistics.Single2x2.bdtOR(yyList, ynList, nyList, nnList, computedOddsRatio);
            double bdtORP = (double)strat2x2.pForChisq(bdtOR, (double)yyList.Count - 1.0);
            double bdOR = Epi.Statistics.Single2x2.bdOR(yyList, ynList, nyList, nnList);
            double bdORP = (double)strat2x2.pForChisq(bdOR, (double)yyList.Count - 1.0);
            double bdRR = Epi.Statistics.Single2x2.bdRR(yyList, ynList, nyList, nnList);
            double bdRRP = (double)strat2x2.pForChisq(bdRR, (double)yyList.Count - 1.0);
            results[0] = mleOR.ToString("F4");
            results[1] = ExactORLL.ToString("F4");
            results[2] = ExactORUL.ToString("F4");

            results[3] = computedRR.ToString("F4");
            results[4] = computedRRMHLL.ToString("F4");
            results[5] = computedRRMHUL.ToString("F4");

            if (singleTableResults.OddsRatioEstimate.HasValue)
            {
                results[6] = singleTableResults.OddsRatioEstimate.Value.ToString("F4");
            }
            if (singleTableResults.OddsRatioLower.HasValue)
            {
                results[7] = singleTableResults.OddsRatioLower.Value.ToString("F4");
            }
            if (singleTableResults.OddsRatioUpper.HasValue)
            {
                results[8] = singleTableResults.OddsRatioUpper.Value.ToString("F4");
            }            

            results[9] = singleTableResults.OddsRatioMLEEstimate.ToString("F4");
            results[10] = singleTableResults.OddsRatioMLEMidPLower.ToString("F4");
            results[11] = singleTableResults.OddsRatioMLEMidPUpper.ToString("F4");

            results[12] = singleTableResults.OddsRatioMLEFisherLower.ToString("F4");
            results[13] = singleTableResults.OddsRatioMLEFisherUpper.ToString("F4");

            if (singleTableResults.RiskRatioEstimate.HasValue)
            {
                results[14] = singleTableResults.RiskRatioEstimate.Value.ToString("F4");
            }
            if (singleTableResults.RiskRatioLower.HasValue)
            {
                results[15] = singleTableResults.RiskRatioLower.Value.ToString("F4");
            }
            if (singleTableResults.RiskRatioUpper.HasValue)
            {
                results[16] = singleTableResults.RiskRatioUpper.Value.ToString("F4");
            }

            results[17] = computedOddsRatio.ToString("F4");
            results[18] = computedOddsRatioMHLL.ToString("F4");
            results[19] = computedOddsRatioMHUL.ToString("F4");

            results[20] = uncorrectedChiSquare.ToString("F4");
            results[21] = uncorrectedChiSquareP.ToString("F10");

            results[22] = corrChisq.ToString("F4");
            results[23] = corrChisqP.ToString("F10");

            results[24] = bdtOR.ToString("F4");
            results[25] = bdtORP.ToString("F10");

            results[26] = bdOR.ToString("F4");
            results[27] = bdORP.ToString("F10");

            results[28] = bdRR.ToString("F4");
            results[29] = bdRRP.ToString("F10");

            e.Result = results;
        }

        private void strataTwoByTwoWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                string[] results = (string[])e.Result;
                txtStratAdjustedMle.Text = results[0];
                txtStratAdjustedMleLower.Text = results[1];
                txtStratAdjustedMleUpper.Text = results[2];

                txtStratAdjustedRr.Text = results[3];
                txtStratAdjustedRrLower.Text = results[4];
                txtStratAdjustedRrUpper.Text = results[5];

                txtStratCrudeOr.Text = results[6];
                txtStratCrudeOrLower.Text = results[7];
                txtStratCrudeOrUpper.Text = results[8];

                txtStratCrudeMle.Text = results[9];
                txtStratCrudeMleLower.Text = results[10];
                txtStratCrudeMleUpper.Text = results[11];

                txtStratFisherLower.Text = results[12];
                txtStratFisherUpper.Text = results[13];

                txtStratCrudeRr.Text = results[14];
                txtStratCrudeRrLower.Text = results[15];
                txtStratCrudeRrUpper.Text = results[16];

                txtStratAdjustedOr.Text = results[17];
                txtStratAdjustedOrLower.Text = results[18];
                txtStratAdjustedOrUpper.Text = results[19];

                txtStratChiUnc.Text = results[20];
                txtStratChiUnc2Tail.Text = results[21];

                txtStratChiCor.Text = results[22];
                txtStratChiCor2Tail.Text = results[23];

                txtStratBDT.Text = results[24];
                txtStratBDT2Tail.Text = results[25];

                txtStratBDOR.Text = results[26];
                txtStratBDOR2Tail.Text = results[27];

                txtStratBDRR.Text = results[28];
                txtStratBDRR2Tail.Text = results[29];
            }
        }
    }
}
