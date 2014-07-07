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
using Epi.WPF.Dashboard;

namespace Epi.WPF.Dashboard.Controls
{
    /// <summary>
    /// Interaction logic for SingleTableAnalysisPanel.xaml
    /// </summary>
    public partial class SingleTableAnalysisPanel : UserControl
    {
        decimal? yyVal;
        decimal? ynVal;
        decimal? nyVal;
        decimal? nnVal;

        public SingleTableAnalysisPanel()
        {
            InitializeComponent();
            Construct();
        }

        private void Construct()
        {
            #region Translation
            tblockSingleTableAnalysis.Text = DashboardSharedStrings.TABLES_SINGLE_TABLE_ANALYSIS_HEADER;
            tblockOddsRiskParams.Text = DashboardSharedStrings.TABLES_ODDS_RISK_PARAMS;
            tblockStatisticalTests.Text = DashboardSharedStrings.TABLES_STATISTICAL_TESTS;
            tblockOddsRatioLabel.Text = DashboardSharedStrings.TABLES_ODDS_RATIO;
            tblockMLELabel.Text = DashboardSharedStrings.TABLES_MLE_ODDS_RATIO;
            tblockFisherExactLabel.Text = DashboardSharedStrings.TABLES_FISHER_EXACT;            
            tblockRiskRatioLabel.Text = DashboardSharedStrings.TABLES_RISK_RATIO;
            tblockRiskDiffLabel.Text = DashboardSharedStrings.TABLES_RISK_DIFFERENCE;

            tblockCorLabel.Text = DashboardSharedStrings.TABLES_CORRECTED;
            tblockUncLabel.Text = DashboardSharedStrings.TABLES_UNCORRECTED;
            tblockMHLabel.Text = DashboardSharedStrings.TABLES_MH;

            tblockEstimate.Text = DashboardSharedStrings.TABLES_ESTIMATE;
            tblockLower.Text = DashboardSharedStrings.TABLES_LOWER;
            tblockUpper.Text = DashboardSharedStrings.TABLES_UPPER;

            tblockFisherExactPLabel.Text = DashboardSharedStrings.TABLES_FISHER_EXACT;
            tblockMidPLabel.Text = DashboardSharedStrings.TABLES_MIDP_EXACT;
            #endregion // Translation
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
            if (YesYesValue.HasValue && YesNoValue.HasValue && NoYesValue.HasValue && NoNoValue.HasValue)
            {
                StatisticsRepository.cTable cTable = new StatisticsRepository.cTable();
                StatisticsRepository.cTable.SingleTableResults singleTableResults = cTable.SigTable((double)yyVal.Value, (double)ynVal.Value, (double)nyVal.Value, (double)nnVal.Value, 0.95);
                cTable = null;

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

                txtChiSqCorP.Text = singleTableResults.ChiSquareYates2P.ToString("F10");
                txtChiSqCorVal.Text = singleTableResults.ChiSquareYatesVal.ToString("F4");
                txtChiSqManP.Text = singleTableResults.ChiSquareMantel2P.ToString("F10");
                txtChiSqManVal.Text = singleTableResults.ChiSquareMantelVal.ToString("F4");
                txtChiSqUncP.Text = singleTableResults.ChiSquareUncorrected2P.ToString("F10");
                txtChiSqUncVal.Text = singleTableResults.ChiSquareUncorrectedVal.ToString("F4");
                txtOddsRatioEstimate.Text = oddsRatioEstimate;
                txtOddsRatioLower.Text = oddsRatioLower;
                txtOddsRatioUpper.Text = oddsRatioUpper;
                txtMidPEstimate.Text = oddsRatioMLEEstimate;
                txtMidPLower.Text = oddsRatioMLEMidPLower;
                txtMidPUpper.Text = oddsRatioMLEMidPUpper;
                txtRiskDifferenceEstimate.Text = singleTableResults.RiskDifferenceEstimate.ToString("F4");
                txtRiskDifferenceLower.Text = singleTableResults.RiskDifferenceLower.ToString("F4");
                txtRiskDifferenceUpper.Text = singleTableResults.RiskDifferenceUpper.ToString("F4");
                txtRiskRatioEstimate.Text = riskRatioEstimate;// singleTableResults.RiskRatioEstimate.ToString("F4");
                txtRiskRatioLower.Text = riskRatioLower;
                txtRiskRatioUpper.Text = riskRatioUpper;
                txtFisherLower.Text = fisherLower;
                txtFisherUpper.Text = fisherUpper;
                txtFisherExact.Text = fisherExact;
                txtFisherExact2P.Text = fisherExact2P;
                txtMidPExact.Text = singleTableResults.MidP.ToString("F10");                
            }
        }

        public string ToHTML()
        {
            StringBuilder htmlBuilder = new StringBuilder();

            string fisherExact = txtFisherExact.Text;
            string fisherExact2 = txtFisherExact2P.Text;
            string fisherLower = txtFisherLower.Text;
            string fisherUpper = txtFisherUpper.Text;

            string oddsRatioEstimate = txtOddsRatioEstimate.Text;
            string oddsRatioLower = txtOddsRatioLower.Text;
            string oddsRatioUpper = txtOddsRatioUpper.Text;

            string oddsRatioMLEEstimate = txtMidPEstimate.Text;
            string oddsRatioMLEMidPLower = txtMidPLower.Text;
            string oddsRatioMLEMidPUpper = txtMidPUpper.Text;

            string riskRatioLower = txtRiskRatioLower.Text;
            string riskRatioUpper = txtRiskRatioUpper.Text;

            string chiSqCorP = txtChiSqCorP.Text;
            string chiSqCorVal = txtChiSqCorVal.Text;
            string chiSqManP = txtChiSqManP.Text;
            string chiSqManVal = txtChiSqManVal.Text;
            string chiSqUncP = txtChiSqUncP.Text;
            string chiSqUncVal = txtChiSqUncVal.Text;
            string riskDifferenceEstimate = txtRiskDifferenceEstimate.Text;
            string riskDifferenceLower = txtRiskDifferenceLower.Text;
            string riskDifferenceUpper = txtRiskDifferenceUpper.Text;
            string riskRatioEstimate = txtRiskRatioEstimate.Text;
            string midPExact = txtMidPExact.Text;

            htmlBuilder.AppendLine("<h4 align=\"Left\"> Single Table Analysis </h4>");

            htmlBuilder.AppendLine("<table align=\"left\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <th></th>");
            htmlBuilder.AppendLine("  <th align=\"center\">Point</th>");
            htmlBuilder.AppendLine("  <th colspan=\"2\" align=\"center\">95% Confidence Interval</th>");
            htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <th></th>");
            htmlBuilder.AppendLine("  <th align=\"center\">Estimate</th>");
            htmlBuilder.AppendLine("  <th align=\"right\">Lower</th>");
            htmlBuilder.AppendLine("  <th align=\"right\">Upper</th>");
            htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <td>PARAMETERS: Odds-based</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <td class=\"stats\">Odds Ratio (cross product)</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + oddsRatioEstimate + "</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + oddsRatioLower + "</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + oddsRatioUpper + "<tt> (T)</tt></td>");
            htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <td class=\"stats\">Odds Ratio (MLE)</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + oddsRatioMLEEstimate + "</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + oddsRatioMLEMidPLower + "</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + oddsRatioMLEMidPUpper + "<tt> (M)</tt></td>");
            htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + fisherLower + "</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + fisherUpper + "<tt> (F)</tt></td>");
            htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <td class=\"stats\">PARAMETERS: Risk-based</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <td class=\"stats\">Risk Ratio (RR)</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + riskRatioEstimate + "</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + riskRatioLower + "</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + riskRatioUpper + "<tt> (T)</tt></td>");
            htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <td class=\"stats\">Risk Difference (RD%)</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + riskDifferenceEstimate + "</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + riskDifferenceLower + "</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + riskDifferenceUpper + "<tt> (T)</tt></td>");
            htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine(" <tr />");
            htmlBuilder.AppendLine(" <tr />");
            htmlBuilder.AppendLine(" <tr />");
            htmlBuilder.AppendLine(" <tr />");
            htmlBuilder.AppendLine(" <tr />");
            htmlBuilder.AppendLine(" <tr> ");
            htmlBuilder.AppendLine("  <td class=\"stats\" colspan=\"4\"><p align=\"center\"><tt> (T=Taylor series; C=Cornfield; M=Mid-P; F=Fisher Exact)</tt></p>");
            htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine(" <tr />");
            htmlBuilder.AppendLine(" <tr />");
            htmlBuilder.AppendLine(" <tr />");
            htmlBuilder.AppendLine(" <tr />");
            htmlBuilder.AppendLine(" <tr />");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <th>STATISTICAL TESTS</th>");
            htmlBuilder.AppendLine("  <th>Chi-square</th>");
            htmlBuilder.AppendLine("  <th>1-tailed p</th>");
            htmlBuilder.AppendLine("  <th>2-tailed p</th>");
            htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <td class=\"stats\">Chi-square - uncorrected</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + chiSqUncVal + "</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + chiSqManP + "</td>");
            htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <td class=\"stats\">Chi-square - Mantel-Haenszel</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + chiSqManVal + "</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + chiSqManP + "</td>");
            htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <td class=\"stats\">Chi-square - corrected (Yates)</td> ");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + chiSqCorVal + "</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + chiSqCorP + "</td>");
            htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <td class=\"stats\">Mid-p exact</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + midPExact + "</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <td class=\"stats\">Fisher exact 1-tailed</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + fisherExact + "</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + fisherExact2 + "</td>");
            htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine("</table>");

            htmlBuilder.AppendLine("<br clear=\"all\" /><br clear=\"all\" />");

            return htmlBuilder.ToString();
        }
    }
}
