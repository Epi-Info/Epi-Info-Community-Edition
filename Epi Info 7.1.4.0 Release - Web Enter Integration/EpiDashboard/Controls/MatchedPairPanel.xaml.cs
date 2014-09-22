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
using EpiDashboard;

namespace EpiDashboard.Controls
{
    /// <summary>
    /// Interaction logic for MatchedPairPanel.xaml
    /// </summary>
    public partial class MatchedPairPanel : UserControl
    {
        decimal? yyVal;
        decimal? ynVal;
        decimal? nyVal;
        decimal? nnVal;
        decimal? tMCa;
        decimal? tFCa;
        decimal? tMCo;
        decimal? tFCo;

        public MatchedPairPanel()
        {
            InitializeComponent();
            Construct();
        }

        private void Construct()
        {
            #region Translation
            //tblockSingleTableAnalysis.Text = DashboardSharedStrings.TABLES_SINGLE_TABLE_ANALYSIS_HEADER;
            //tblockOddsRiskParams.Text = DashboardSharedStrings.TABLES_ODDS_RISK_PARAMS;
            //tblockStatisticalTests.Text = DashboardSharedStrings.TABLES_STATISTICAL_TESTS;
            //tblockOddsRatioLabel.Text = DashboardSharedStrings.TABLES_ODDS_RATIO;
            //tblockMLELabel.Text = DashboardSharedStrings.TABLES_MLE_ODDS_RATIO;
            //tblockFisherExactLabel.Text = DashboardSharedStrings.TABLES_FISHER_EXACT;            
            //tblockRiskRatioLabel.Text = DashboardSharedStrings.TABLES_RISK_RATIO;
            //tblockRiskDiffLabel.Text = DashboardSharedStrings.TABLES_RISK_DIFFERENCE;

            //tblockCorLabel.Text = DashboardSharedStrings.TABLES_CORRECTED;
            //tblockUncLabel.Text = DashboardSharedStrings.TABLES_UNCORRECTED;
            //tblockMHLabel.Text = DashboardSharedStrings.TABLES_MH;

            //tblockEstimate.Text = DashboardSharedStrings.TABLES_ESTIMATE;
            //tblockLower.Text = DashboardSharedStrings.TABLES_LOWER;
            //tblockUpper.Text = DashboardSharedStrings.TABLES_UPPER;

            //tblockFisherExactPLabel.Text = DashboardSharedStrings.TABLES_FISHER_EXACT;
            //tblockMidPLabel.Text = DashboardSharedStrings.TABLES_MIDP_EXACT;
            #endregion // Translation
        }

        public decimal? tooManyCases
        {
            get
            {
                return this.tMCa;
            }
            set
            {
                tMCa = value;
                Compute();
            }
        }

        public decimal? tooFewCases
        {
            get
            {
                return this.tFCa;
            }
            set
            {
                tFCa = value;
                Compute();
            }
        }

        public decimal? tooManyControls
        {
            get
            {
                return this.tMCo;
            }
            set
            {
                tMCo = value;
                Compute();
            }
        }

        public decimal? tooFewControls
        {
            get
            {
                return this.tFCo;
            }
            set
            {
                tFCo = value;
                Compute();
            }
        }

        public decimal? YesYesValue
        {
            get
            {
                return this.yyVal;
            }
            set
            {
                yyVal = value;
                Compute();
            }
        }

        public decimal? YesNoValue
        {
            get
            {
                return ynVal;
            }
            set
            {
                ynVal = value;
                Compute();
            }
        }

        public decimal? NoYesValue
        {
            get
            {
                return nyVal;
            }
            set
            {
                nyVal = value;
                Compute();
            }
        }

        public decimal? NoNoValue
        {
            get
            {
                return nnVal;
            }
            set
            {
                nnVal = value;
                Compute();
            }
        }

        public void Compute()
        {
            if (YesYesValue.HasValue && YesNoValue.HasValue && NoYesValue.HasValue && NoNoValue.HasValue &&
                tooManyCases.HasValue && tooFewCases.HasValue && tooManyControls.HasValue && tooFewControls.HasValue)
            {
                Epi.Statistics.pmccTable pmccTable = new Epi.Statistics.pmccTable();
                Epi.Statistics.pmccTable.SingleTableResults statTable = pmccTable.statTable((double)ynVal.Value, (double)nyVal.Value);
                pmccTable = null;

                string fisherExact = Epi.SharedStrings.UNDEFINED;
                string fisherExact2P = Epi.SharedStrings.UNDEFINED;
                string fisherLower = Epi.SharedStrings.UNDEFINED;
                string fisherUpper = Epi.SharedStrings.UNDEFINED;

                string oddsRatioEstimate = Epi.SharedStrings.UNDEFINED;
                string oddsRatioLower = Epi.SharedStrings.UNDEFINED;
                string oddsRatioUpper = Epi.SharedStrings.UNDEFINED;

                if (!Double.IsNaN(statTable.FisherExactP) && statTable.FisherExactP != -1)
                {
                    fisherExact = ((double)statTable.FisherExactP).ToString("F10");
                    clOneTailP.Text = "1 Tailed P";
                    clTwoTailP.Text = "2 Tailed P";
                    tblockFisherExactPLabel.Text = "Fisher Exact";
                    fisherBorderLine.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    fisherExact = "";
                    fisherExact2P = "";
                    tblockFisherExactPLabel.Text = "";
                    clOneTailP.Text = "";
                    clTwoTailP.Text = "";
                    fisherBorderLine.Visibility = System.Windows.Visibility.Hidden;
                }

                if (!Double.IsNaN(statTable.FisherExact2P) && statTable.FisherExact2P != -1)
                {
                    fisherExact2P = ((double)statTable.FisherExact2P).ToString("F10");
                }

                if (!Double.IsNaN(statTable.OddsRatioFisherLower) && statTable.OddsRatioFisherLower != -1)
                {
                    fisherLower = ((double)statTable.OddsRatioFisherLower).ToString("F4");
                    tblockFisherExactLabel.Text = "Exact";
                }
                else
                {
                    fisherLower = "";
                    fisherUpper = "";
                    tblockFisherExactLabel.Text = "";
                }

                if (!Double.IsNaN(statTable.OddsRatioFisherUpper) && statTable.OddsRatioFisherUpper != -1)
                {
                    fisherUpper = ((double)statTable.OddsRatioFisherUpper).ToString("F4");
                }

                if (statTable.OddsRatioEstimate != null)
                {
                    oddsRatioEstimate = ((double)statTable.OddsRatioEstimate).ToString("F4");
                }

                if (statTable.OddsRatioLower != null)
                {
                    oddsRatioLower = ((double)statTable.OddsRatioLower).ToString("F4");
                }

                if (statTable.OddsRatioUpper != null)
                {
                    oddsRatioUpper = ((double)statTable.OddsRatioUpper).ToString("F4");
                }

                int discordantPairs = (int) ynVal.Value + (int) nyVal.Value;
                int totalPairs = (int)yyVal.Value + (int)ynVal.Value + (int)nyVal.Value + (int)nnVal.Value;

                txtChiSqCorP.Text = statTable.McNemarCor2P.ToString("F10");
                txtChiSqCorVal.Text = statTable.McNemarCorVal.ToString("F4");
                txtChiSqUncP.Text = statTable.McNemarUncorrected2P.ToString("F10");
                txtChiSqUncVal.Text = statTable.McNemarUncorrectedVal.ToString("F4");
                txtOddsRatioEstimate.Text = oddsRatioEstimate;
                txtOddsRatioLower.Text = oddsRatioLower;
                txtOddsRatioUpper.Text = oddsRatioUpper;
                txtFisherLower.Text = fisherLower;
                txtFisherUpper.Text = fisherUpper;
                txtFisherExact.Text = fisherExact;
                txtFisherExact2P.Text = fisherExact2P;
                testDisclaimer0.Visibility = System.Windows.Visibility.Visible;
                testDisclaimer0.Text = "There are " + discordantPairs + " discordant pairs.  Because this number";
                if (discordantPairs < 20)
                {
                    testDisclaimer1.Visibility = System.Windows.Visibility.Visible;
                    testDisclaimer1.Text = "is fewer than 20, it is recommended that";
                    testDisclaimer2.Visibility = System.Windows.Visibility.Visible;
                    testDisclaimer2.Text = "only the exact results be used.";
                }
                else
                {
                    testDisclaimer1.Visibility = System.Windows.Visibility.Visible;
                    testDisclaimer1.Text = "is  >= 20, the McNemar test can be used.";
                    testDisclaimer2.Text = "";
                    testDisclaimer2.Visibility = System.Windows.Visibility.Collapsed;
                }
                if (totalPairs == 0)
                {
                    testDisclaimer0.Text = "No pairs were counted.  Confirm that case/control and ";
                    testDisclaimer1.Visibility = System.Windows.Visibility.Visible;
                    testDisclaimer1.Text = "exposure variables have 2 possible values.";
                    testDisclaimer2.Text = "";
                    testDisclaimer2.Visibility = System.Windows.Visibility.Collapsed;
                }
                if ((int) ynVal.Value == 0 || (int) nyVal.Value == 0)
                {
                    zeroDisclaimer0.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    zeroDisclaimer0.Visibility = System.Windows.Visibility.Collapsed;
                }
                if (tMCa == 1)
                {
                    testDisclaimer3.Visibility = System.Windows.Visibility.Visible;
                    testDisclaimer3.Text = "1 group has more than one case.";
                }
                else if (tMCa > 1)
                {
                    testDisclaimer3.Visibility = System.Windows.Visibility.Visible;
                    testDisclaimer3.Text = tMCa + " groups have more than one case.";
                }
                else
                    testDisclaimer3.Visibility = System.Windows.Visibility.Collapsed;
                if (tFCa == 1)
                {
                    testDisclaimer4.Visibility = System.Windows.Visibility.Visible;
                    testDisclaimer4.Text = "1 group has no case.";
                }
                else if (tFCa > 1)
                {
                    testDisclaimer4.Visibility = System.Windows.Visibility.Visible;
                    testDisclaimer4.Text = tFCa + " groups have no case.";
                }
                else
                    testDisclaimer4.Visibility = System.Windows.Visibility.Collapsed;
                if (tMCo == 1)
                {
                    testDisclaimer5.Visibility = System.Windows.Visibility.Visible;
                    testDisclaimer5.Text = "1 group has more than one control.";
                }
                else if (tMCo > 1)
                {
                    testDisclaimer5.Visibility = System.Windows.Visibility.Visible;
                    testDisclaimer5.Text = tMCo + " groups have more than one control.";
                }
                else
                    testDisclaimer5.Visibility = System.Windows.Visibility.Collapsed;
                if (tFCo == 1)
                {
                    testDisclaimer6.Visibility = System.Windows.Visibility.Visible;
                    testDisclaimer6.Text = "1 group has no control.";
                }
                else if (tFCo > 1)
                {
                    testDisclaimer6.Visibility = System.Windows.Visibility.Visible;
                    testDisclaimer6.Text = tFCo + " groups have no control.";
                }
                else
                    testDisclaimer6.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        public string ToHTML()
        {
            //return string.Empty;
            StringBuilder htmlBuilder = new StringBuilder();

            string fisherExact = txtFisherExact.Text;
            string fisherExact2 = txtFisherExact2P.Text;
            string fisherLower = txtFisherLower.Text;
            string fisherUpper = txtFisherUpper.Text;

            string oddsRatioEstimate = txtOddsRatioEstimate.Text;
            string oddsRatioLower = txtOddsRatioLower.Text;
            string oddsRatioUpper = txtOddsRatioUpper.Text;

            //string oddsRatioMLEEstimate = txtMidPEstimate.Text;
            //string oddsRatioMLEMidPLower = txtMidPLower.Text;
            //string oddsRatioMLEMidPUpper = txtMidPUpper.Text;

            //string riskRatioLower = txtRiskRatioLower.Text;
            //string riskRatioUpper = txtRiskRatioUpper.Text;

            string chiSqCorP = txtChiSqCorP.Text;
            string chiSqCorVal = txtChiSqCorVal.Text;
            //string chiSqManP = txtChiSqManP.Text;
            //string chiSqManVal = txtChiSqManVal.Text;
            string chiSqUncP = txtChiSqUncP.Text;
            string chiSqUncVal = txtChiSqUncVal.Text;
            //string riskDifferenceEstimate = txtRiskDifferenceEstimate.Text;
            //string riskDifferenceLower = txtRiskDifferenceLower.Text;
            //string riskDifferenceUpper = txtRiskDifferenceUpper.Text;
            //string riskRatioEstimate = txtRiskRatioEstimate.Text;
            //string midPExact = txtMidPExact.Text;

            htmlBuilder.AppendLine("<h4 align=\"Left\">Matched Pair Case-Control Study</h4>");

            htmlBuilder.AppendLine("<table align=\"left\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
            //htmlBuilder.AppendLine(" <tr>");
            //htmlBuilder.AppendLine("  <th></th>");
            //htmlBuilder.AppendLine("  <th align=\"center\">Point</th>");
            //htmlBuilder.AppendLine("  <th colspan=\"2\" align=\"center\">95% Confidence Interval</th>");
            //htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <th align=\"left\">Odds-based parameters</th>");
            htmlBuilder.AppendLine("  <th align=\"center\">Estimate</th>");
            htmlBuilder.AppendLine("  <th align=\"right\">Lower</th>");
            htmlBuilder.AppendLine("  <th align=\"right\">Upper</th>");
            htmlBuilder.AppendLine(" </tr>");
            //htmlBuilder.AppendLine(" <tr>");
            //htmlBuilder.AppendLine("  <td></td>");
            //htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            //htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            //htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            //htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <td class=\"stats\">Odds Ratio</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + oddsRatioEstimate + "</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + oddsRatioLower + "</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + oddsRatioUpper + "</td>");
            htmlBuilder.AppendLine(" </tr>");
            //htmlBuilder.AppendLine(" <tr>");
            //htmlBuilder.AppendLine("  <td class=\"stats\">Odds Ratio (MLE)</td>");
            //htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + oddsRatioMLEEstimate + "</td>");
            //htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + oddsRatioMLEMidPLower + "</td>");
            //htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + oddsRatioMLEMidPUpper + "<tt> (M)</tt></td>");
            //htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <td class=\"stats\">Exact</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + fisherLower + "</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + fisherUpper + "</td>");
            htmlBuilder.AppendLine(" </tr>");
            //htmlBuilder.AppendLine(" <tr>");
            //htmlBuilder.AppendLine("  <td class=\"stats\">PARAMETERS: Risk-based</td>");
            //htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            //htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            //htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            //htmlBuilder.AppendLine(" </tr>");
            //htmlBuilder.AppendLine(" <tr>");
            //htmlBuilder.AppendLine("  <td class=\"stats\">Risk Ratio (RR)</td>");
            //htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + riskRatioEstimate + "</td>");
            //htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + riskRatioLower + "</td>");
            //htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + riskRatioUpper + "<tt> (T)</tt></td>");
            //htmlBuilder.AppendLine(" </tr>");
            //htmlBuilder.AppendLine(" <tr>");
            //htmlBuilder.AppendLine("  <td class=\"stats\">Risk Difference (RD%)</td>");
            //htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + riskDifferenceEstimate + "</td>");
            //htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + riskDifferenceLower + "</td>");
            //htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + riskDifferenceUpper + "<tt> (T)</tt></td>");
            //htmlBuilder.AppendLine(" </tr>");
            //htmlBuilder.AppendLine(" <tr />");
            //htmlBuilder.AppendLine(" <tr />");
            //htmlBuilder.AppendLine(" <tr />");
            //htmlBuilder.AppendLine(" <tr />");
            //htmlBuilder.AppendLine(" <tr />");
            //htmlBuilder.AppendLine(" <tr> ");
            //htmlBuilder.AppendLine("  <td class=\"stats\" colspan=\"4\"><p align=\"center\"><tt> (T=Taylor series; C=Cornfield; M=Mid-P; F=Fisher Exact)</tt></p>");
            //htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine(" <tr />");
            htmlBuilder.AppendLine(" <tr />");
            htmlBuilder.AppendLine(" <tr />");
            htmlBuilder.AppendLine(" <tr />");
            htmlBuilder.AppendLine(" <tr />");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <th>Statistical tests</th>");
            htmlBuilder.AppendLine("  <th>Chi-square</th>");
            htmlBuilder.AppendLine("  <th>1-tailed p</th>");
            htmlBuilder.AppendLine("  <th>2-tailed p</th>");
            htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <td class=\"stats\">McNemar</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + chiSqUncVal + "</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + chiSqUncP + "</td>");
            htmlBuilder.AppendLine(" </tr>");
            //htmlBuilder.AppendLine(" <tr>");
            //htmlBuilder.AppendLine("  <td class=\"stats\">Chi-square - Mantel-Haenszel</td>");
            //htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + chiSqManVal + "</td>");
            //htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            //htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + chiSqManP + "</td>");
            //htmlBuilder.AppendLine(" </tr>");
            //htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <td class=\"stats\">Corrected</td> ");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + chiSqCorVal + "</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + chiSqCorP + "</td>");
            htmlBuilder.AppendLine(" </tr>");
            //htmlBuilder.AppendLine(" <tr>");
            //htmlBuilder.AppendLine("  <td class=\"stats\">Mid-p exact</td>");
            //htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            //htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + midPExact + "</td>");
            //htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            //htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <td class=\"stats\">Fisher Exact</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + fisherExact + "</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + fisherExact2 + "</td>");
            htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine("</table>");

            htmlBuilder.AppendLine("<br clear=\"all\" /><br clear=\"all\" />");

            if (zeroDisclaimer0.Visibility == System.Windows.Visibility.Visible)
                htmlBuilder.AppendLine("" + zeroDisclaimer0.Text + "<br>");
            if (testDisclaimer0.Visibility == System.Windows.Visibility.Visible)
                htmlBuilder.AppendLine("" + testDisclaimer0.Text + "");
            if (testDisclaimer1.Visibility == System.Windows.Visibility.Visible)
                htmlBuilder.AppendLine(" " + testDisclaimer1.Text + "");
            if (testDisclaimer2.Visibility == System.Windows.Visibility.Visible)
                htmlBuilder.AppendLine(" " + testDisclaimer2.Text + "<br>");
            else if (testDisclaimer1.Visibility == System.Windows.Visibility.Visible)
                htmlBuilder.AppendLine("<br>");
            if (testDisclaimer3.Visibility == System.Windows.Visibility.Visible)
                htmlBuilder.AppendLine("" + testDisclaimer3.Text + "<br>");
            if (testDisclaimer4.Visibility == System.Windows.Visibility.Visible)
                htmlBuilder.AppendLine("" + testDisclaimer4.Text + "<br>");
            if (testDisclaimer5.Visibility == System.Windows.Visibility.Visible)
                htmlBuilder.AppendLine("" + testDisclaimer5.Text + "<br>");
            if (testDisclaimer6.Visibility == System.Windows.Visibility.Visible)
                htmlBuilder.AppendLine("" + testDisclaimer6.Text + "<br>");

            return htmlBuilder.ToString();
        }
    }
}
