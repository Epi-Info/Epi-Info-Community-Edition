using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EpiDashboard.StatCalc
{
    /// <summary>
    /// Interaction logic for TwoByTwo.xaml
    /// </summary>
    public partial class TwoByTwo : UserControl, IGadget, IStatCalcControl
    {

        private delegate void SimpleDelegate();
        public event GadgetRefreshedHandler GadgetRefreshed;
        public event GadgetClosingHandler GadgetClosing;
        public event GadgetProcessingFinishedHandler GadgetProcessingFinished;
        public event GadgetRepositionEventHandler GadgetReposition;
        public event GadgetCheckForCancellationHandler GadgetCheckForCancellation;
        public event GadgetEventHandler GadgetDragStart;
        public event GadgetEventHandler GadgetDragStop;
        public event GadgetEventHandler GadgetDrag;
        public event GadgetAnchorSetEventHandler GadgetAnchorSetFromXml;

        private StatisticsRepository.cTable.SingleTableResults currentSingleTableResults;
        private Dictionary<int, double[]> strataVals;
        private Dictionary<int, Boolean[]> strataActive;
        private bool isProcessing;

        public UserControl AnchorLeft { get; set; }
        public UserControl AnchorTop { get; set; }
        public UserControl AnchorBottom { get; set; }
        public UserControl AnchorRight { get; set; }
        public Guid UniqueIdentifier { get; private set; }

        public TwoByTwo()
        {
            InitializeComponent();
            RotateTransform rotate = new RotateTransform(270);

            strataVals = new Dictionary<int, double[]>();
            strataVals.Add(1, new double[4]);
            strataVals.Add(2, new double[4]);
            strataVals.Add(3, new double[4]);
            strataVals.Add(4, new double[4]);
            strataVals.Add(5, new double[4]);
            strataVals.Add(6, new double[4]);
            strataVals.Add(7, new double[4]);
            strataVals.Add(8, new double[4]);
            strataVals.Add(9, new double[4]);

            strataActive = new Dictionary<int, bool[]>();
            strataActive.Add(1, new Boolean[4]);
            strataActive.Add(2, new Boolean[4]);
            strataActive.Add(3, new Boolean[4]);
            strataActive.Add(4, new Boolean[4]);
            strataActive.Add(5, new Boolean[4]);
            strataActive.Add(6, new Boolean[4]);
            strataActive.Add(7, new Boolean[4]);
            strataActive.Add(8, new Boolean[4]);
            strataActive.Add(9, new Boolean[4]);

            txtExposure.RenderTransform = rotate;
            txtYesYesVal1.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtYesNoVal1.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtNoYesVal1.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtNoNoVal1.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
			//EI-14
            txtYesYesVal1.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtYesNoVal1.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtNoYesVal1.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtNoNoVal1.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);

            txtExposure2.RenderTransform = rotate;
            txtYesYesVal2.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtYesNoVal2.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtNoYesVal2.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtNoNoVal2.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
			//EI-14
            txtYesYesVal2.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtYesNoVal2.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtNoYesVal2.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtNoNoVal2.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);

            txtExposure3.RenderTransform = rotate;
            txtYesYesVal3.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtYesNoVal3.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtNoYesVal3.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtNoNoVal3.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
			//EI-14
            txtYesYesVal3.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtYesNoVal3.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtNoYesVal3.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtNoNoVal3.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);

            txtExposure4.RenderTransform = rotate;
            txtYesYesVal4.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtYesNoVal4.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtNoYesVal4.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtNoNoVal4.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
			//EI-14
            txtYesYesVal4.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtYesNoVal4.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtNoYesVal4.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtNoNoVal4.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);

            txtExposure5.RenderTransform = rotate;
            txtYesYesVal5.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtYesNoVal5.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtNoYesVal5.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtNoNoVal5.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
			//EI-14
            txtYesYesVal5.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtYesNoVal5.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtNoYesVal5.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtNoNoVal5.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);

            txtExposure6.RenderTransform = rotate;
            txtYesYesVal6.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtYesNoVal6.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtNoYesVal6.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtNoNoVal6.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
			//EI-14
            txtYesYesVal6.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtYesNoVal6.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtNoYesVal6.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtNoNoVal6.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);

            txtExposure7.RenderTransform = rotate;
            txtYesYesVal7.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtYesNoVal7.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtNoYesVal7.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtNoNoVal7.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
			//EI-14
            txtYesYesVal7.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtYesNoVal7.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtNoYesVal7.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtNoNoVal7.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);

            txtExposure8.RenderTransform = rotate;
            txtYesYesVal8.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtYesNoVal8.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtNoYesVal8.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtNoNoVal8.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
			//EI-14
            txtYesYesVal8.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtYesNoVal8.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtNoYesVal8.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtNoNoVal8.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);

            txtExposure9.RenderTransform = rotate;
            txtYesYesVal9.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtYesNoVal9.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtNoYesVal9.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
            txtNoNoVal9.TextChanged += new TextChangedEventHandler(txtInputs_TextChanged);
			//EI-14
            txtYesYesVal9.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtYesNoVal9.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtNoYesVal9.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtNoNoVal9.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);

            imgClose.MouseEnter += new MouseEventHandler(imgClose_MouseEnter);
            imgClose.MouseLeave += new MouseEventHandler(imgClose_MouseLeave);
            imgClose.MouseDown += new MouseButtonEventHandler(imgClose_MouseDown);
            mnuPrint.Click += new RoutedEventHandler(mnuPrint_Click);
            mnuSave.Click += new RoutedEventHandler(mnuSave_Click);

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new SimpleDelegate(DoFocus));
        }

        void mnuSave_Click(object sender, RoutedEventArgs e)
        {
            Common.SaveAsImage(grdAll);
        }


        void mnuPrint_Click(object sender, RoutedEventArgs e)
        {
            Common.Print(pnlMainContent);
        }

        void imgClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (GadgetClosing != null)
                GadgetClosing(this);
        }

        void imgClose_MouseLeave(object sender, MouseEventArgs e)
        {
            Uri uriSource = new Uri("../Images/x.png", UriKind.Relative);
            imgClose.Source = new BitmapImage(uriSource);
        }

        void imgClose_MouseEnter(object sender, MouseEventArgs e)
        {
            Uri uriSource = new Uri("../Images/x_over.png", UriKind.Relative);
            imgClose.Source = new BitmapImage(uriSource);
        }

        private void DoFocus()
        {
            txtYesYesVal1.Focus();
        }

        private childItem FindChild<childItem>(DependencyObject obj, string name)
    where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem && ((FrameworkElement)child).Name.Equals(name))
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindChild<childItem>(child, name);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }


        private void CalculateStrata(int strata)
        {
            try
            {
                int yyVal = 0;
                int ynVal = 0;
                int nyVal = 0;
                int nnVal = 0;

                TextBox txtYesYesVal = FindChild<TextBox>(MainTab, "txtYesYesVal" + strata);
                TextBox txtYesNoVal = FindChild<TextBox>(MainTab, "txtYesNoVal" + strata);
                TextBox txtNoYesVal = FindChild<TextBox>(MainTab, "txtNoYesVal" + strata);
                TextBox txtNoNoVal = FindChild<TextBox>(MainTab, "txtNoNoVal" + strata);

                TextBlock txtYesTotalVal = FindChild<TextBlock>(MainTab, "txtYesTotalVal" + strata);
                TextBlock txtNoTotalVal = FindChild<TextBlock>(MainTab, "txtNoTotalVal" + strata);
                TextBlock txtYesYesRow = FindChild<TextBlock>(MainTab, "txtYesYesRow" + strata);
                TextBlock txtYesNoRow = FindChild<TextBlock>(MainTab, "txtYesNoRow" + strata);
                TextBlock txtNoYesRow = FindChild<TextBlock>(MainTab, "txtNoYesRow" + strata);
                TextBlock txtNoNoRow = FindChild<TextBlock>(MainTab, "txtNoNoRow" + strata);
                TextBlock txtTotalTotalVal = FindChild<TextBlock>(MainTab, "txtTotalTotalVal" + strata);
                TextBlock txtTotalYesVal = FindChild<TextBlock>(MainTab, "txtTotalYesVal" + strata);
                TextBlock txtYesYesCol = FindChild<TextBlock>(MainTab, "txtYesYesCol" + strata);
                TextBlock txtNoYesCol = FindChild<TextBlock>(MainTab, "txtNoYesCol" + strata);
                TextBlock txtTotalNoVal = FindChild<TextBlock>(MainTab, "txtTotalNoVal" + strata);
                TextBlock txtYesNoCol = FindChild<TextBlock>(MainTab, "txtYesNoCol" + strata);
                TextBlock txtNoNoCol = FindChild<TextBlock>(MainTab, "txtNoNoCol" + strata);
                TextBlock txtTotalYesRow = FindChild<TextBlock>(MainTab, "txtTotalYesRow" + strata);
                TextBlock txtTotalNoRow = FindChild<TextBlock>(MainTab, "txtTotalNoRow" + strata);
                TextBlock txtYesTotalCol = FindChild<TextBlock>(MainTab, "txtYesTotalCol" + strata);
                TextBlock txtNoTotalCol = FindChild<TextBlock>(MainTab, "txtNoTotalCol" + strata);
                TextBlock txtTotalYesCol = FindChild<TextBlock>(MainTab, "txtTotalYesCol" + strata);
                TextBlock txtTotalNoCol = FindChild<TextBlock>(MainTab, "txtTotalNoCol" + strata);
                TextBlock txtYesTotalRow = FindChild<TextBlock>(MainTab, "txtYesTotalRow" + strata);
                TextBlock txtNoTotalRow = FindChild<TextBlock>(MainTab, "txtNoTotalRow" + strata);
                TextBlock txtTotalTotalCol = FindChild<TextBlock>(MainTab, "txtTotalTotalCol" + strata);
                TextBlock txtTotalTotalRow = FindChild<TextBlock>(MainTab, "txtTotalTotalRow" + strata);

                TextBlock txtChiSqCorP = FindChild<TextBlock>(MainTab, "txtChiSqCorP" + strata);
                TextBlock txtChiSqCorVal = FindChild<TextBlock>(MainTab, "txtChiSqCorVal" + strata);
                TextBlock txtChiSqManP = FindChild<TextBlock>(MainTab, "txtChiSqManP" + strata);
                TextBlock txtChiSqManVal = FindChild<TextBlock>(MainTab, "txtChiSqManVal" + strata);
                TextBlock txtChiSqUncP = FindChild<TextBlock>(MainTab, "txtChiSqUncP" + strata);
                TextBlock txtChiSqUncVal = FindChild<TextBlock>(MainTab, "txtChiSqUncVal" + strata);
                TextBlock txtOddsRatioEstimate = FindChild<TextBlock>(MainTab, "txtOddsRatioEstimate" + strata);
                TextBlock txtOddsRatioLower = FindChild<TextBlock>(MainTab, "txtOddsRatioLower" + strata);
                TextBlock txtOddsRatioUpper = FindChild<TextBlock>(MainTab, "txtOddsRatioUpper" + strata);
                TextBlock txtMidPEstimate = FindChild<TextBlock>(MainTab, "txtMidPEstimate" + strata);
                TextBlock txtMidPLower = FindChild<TextBlock>(MainTab, "txtMidPLower" + strata);
                TextBlock txtMidPUpper = FindChild<TextBlock>(MainTab, "txtMidPUpper" + strata);
                TextBlock txtFisherLower = FindChild<TextBlock>(MainTab, "txtFisherLower" + strata);
                TextBlock txtFisherUpper = FindChild<TextBlock>(MainTab, "txtFisherUpper" + strata);
                TextBlock txtRiskDifferenceEstimate = FindChild<TextBlock>(MainTab, "txtRiskDifferenceEstimate" + strata);
                TextBlock txtRiskDifferenceLower = FindChild<TextBlock>(MainTab, "txtRiskDifferenceLower" + strata);
                TextBlock txtRiskDifferenceUpper = FindChild<TextBlock>(MainTab, "txtRiskDifferenceUpper" + strata);
                TextBlock txtRiskRatioEstimate = FindChild<TextBlock>(MainTab, "txtRiskRatioEstimate" + strata);
                TextBlock txtRiskRatioLower = FindChild<TextBlock>(MainTab, "txtRiskRatioLower" + strata);
                TextBlock txtRiskRatioUpper = FindChild<TextBlock>(MainTab, "txtRiskRatioUpper" + strata);
                TextBlock txtFisherExact = FindChild<TextBlock>(MainTab, "txtFisherExact" + strata);
                TextBlock txtMidPExact = FindChild<TextBlock>(MainTab, "txtMidPExact" + strata);
                TextBlock txtFisherExact2P = FindChild<TextBlock>(MainTab, "txtFisherExact2P" + strata);

				TextBlock txtWarningMessage = FindChild<TextBlock>(MainTab, "txtWarningMessage" + strata);

				int.TryParse(txtYesYesVal.Text, out yyVal);
                int.TryParse(txtYesNoVal.Text, out ynVal);
                int.TryParse(txtNoYesVal.Text, out nyVal);
                int.TryParse(txtNoNoVal.Text, out nnVal);

                strataVals[strata][0] = yyVal;
                strataVals[strata][1] = ynVal;
                strataVals[strata][2] = nyVal;
                strataVals[strata][3] = nnVal;
                strataActive[strata][0] = (txtYesYesVal.Text.Length > 0);
                strataActive[strata][1] = (txtYesNoVal.Text.Length > 0);
                strataActive[strata][2] = (txtNoYesVal.Text.Length > 0);
                strataActive[strata][3] = (txtNoNoVal.Text.Length > 0);

                int ytVal = yyVal + ynVal;
                int ntVal = nyVal + nnVal;
                int ttVal = ytVal + ntVal;
                int tyVal = yyVal + nyVal;
                int tnVal = ynVal + nnVal;
                double yyRowPct = 0;
                double ynRowPct = 0;
                double nyRowPct = 0;
                double nnRowPct = 0;
                double yyColPct = 0;
                double nyColPct = 0;
                double ynColPct = 0;
                double nnColPct = 0;
                double tyRowPct = 0;
                double tnRowPct = 0;
                double ytColPct = 0;
                double ntColPct = 0;
                if (ytVal != 0)
                {
                    yyRowPct = 1.0 * yyVal / ytVal;
                    ynRowPct = 1.0 * ynVal / ytVal;
                }
                if (ntVal != 0)
                {
                    nyRowPct = 1.0 * nyVal / ntVal;
                    nnRowPct = 1.0 * nnVal / ntVal;
                }
                if (tyVal != 0)
                {
                    yyColPct = 1.0 * yyVal / tyVal;
                    nyColPct = 1.0 * nyVal / tyVal;
                }
                if (tnVal != 0)
                {
                    ynColPct = 1.0 * ynVal / tnVal;
                    nnColPct = 1.0 * nnVal / tnVal;
                }
                if (ttVal != 0)
                {
                    tyRowPct = 1.0 * tyVal / ttVal;
                    tnRowPct = 1.0 * tnVal / ttVal;
                    ytColPct = 1.0 * ytVal / ttVal;
                    ntColPct = 1.0 * ntVal / ttVal;
                }

                txtYesTotalVal.Text = ytVal.ToString();
                txtNoTotalVal.Text = ntVal.ToString();
                txtYesYesRow.Text = yyRowPct.ToString("P");
                txtYesNoRow.Text = ynRowPct.ToString("P");
                txtNoYesRow.Text = nyRowPct.ToString("P");
                txtNoNoRow.Text = nnRowPct.ToString("P");
                txtTotalTotalVal.Text = ttVal.ToString();
                txtTotalYesVal.Text = tyVal.ToString();
                txtYesYesCol.Text = yyColPct.ToString("P");
                txtNoYesCol.Text = nyColPct.ToString("P");
                txtTotalNoVal.Text = tnVal.ToString();
                txtYesNoCol.Text = ynColPct.ToString("P");
                txtNoNoCol.Text = nnColPct.ToString("P");
                txtTotalYesRow.Text = tyRowPct.ToString("P");
                txtTotalNoRow.Text = tnRowPct.ToString("P");
                txtYesTotalCol.Text = ytColPct.ToString("P");
                txtNoTotalCol.Text = ntColPct.ToString("P");
                txtTotalYesCol.Text = (1).ToString("P");
                txtTotalNoCol.Text = (1).ToString("P");
                txtYesTotalRow.Text = (1).ToString("P");
                txtNoTotalRow.Text = (1).ToString("P");
                txtTotalTotalCol.Text = (1).ToString("P");
                txtTotalTotalRow.Text = (1).ToString("P");


                if ((ytVal > 0) && (ntVal > 0) && (tyVal > 0) && (tnVal > 0))
                {
                    StatisticsRepository.cTable.SingleTableResults singleTableResults = new StatisticsRepository.cTable().SigTable((double)yyVal, (double)ynVal, (double)nyVal, (double)nnVal, 0.95);

                    string oddsRatioEstimate = Epi.SharedStrings.UNDEFINED;
                    string oddsRatioLower = Epi.SharedStrings.UNDEFINED;
                    string oddsRatioUpper = Epi.SharedStrings.UNDEFINED;
                    string riskRatioEstimate = Epi.SharedStrings.UNDEFINED;
                    string riskRatioLower = Epi.SharedStrings.UNDEFINED;
                    string riskRatioUpper = Epi.SharedStrings.UNDEFINED;
                    string fisherExact2P = Epi.SharedStrings.UNDEFINED;

                    if (singleTableResults.FisherExact2P != -1)
                    {
                        fisherExact2P = ((double)singleTableResults.FisherExact2P).ToString("F8");
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

					if (singleTableResults.LowestExpectedCellCount < 5)
						txtWarningMessage.Text = "An expected cell count is < 5. X" + '\u00B2' + " may not be valid.";
					else
						txtWarningMessage.Text = String.Empty;

					txtChiSqCorP.Text = singleTableResults.ChiSquareYates2P.ToString("F8");
                    txtChiSqCorVal.Text = singleTableResults.ChiSquareYatesVal.ToString("F4");
                    txtChiSqManP.Text = singleTableResults.ChiSquareMantel2P.ToString("F8");
                    txtChiSqManVal.Text = singleTableResults.ChiSquareMantelVal.ToString("F4");
                    txtChiSqUncP.Text = singleTableResults.ChiSquareUncorrected2P.ToString("F8");
                    txtChiSqUncVal.Text = singleTableResults.ChiSquareUncorrectedVal.ToString("F4");
                    txtOddsRatioEstimate.Text = oddsRatioEstimate; // singleTableResults.OddsRatioEstimate.ToString("F4");
                    txtOddsRatioLower.Text = oddsRatioLower; //singleTableResults.OddsRatioLower.ToString("F4");
                    txtOddsRatioUpper.Text = oddsRatioUpper; // singleTableResults.OddsRatioUpper.ToString("F4");
                    txtMidPEstimate.Text = singleTableResults.OddsRatioMLEEstimate.ToString("F4");
                    txtMidPLower.Text = singleTableResults.OddsRatioMLEMidPLower.ToString("F4");
                    txtMidPUpper.Text = singleTableResults.OddsRatioMLEMidPUpper.ToString("F4");
                    txtFisherLower.Text = singleTableResults.OddsRatioMLEFisherLower.ToString("F4");
                    txtFisherUpper.Text = singleTableResults.OddsRatioMLEFisherUpper.ToString("F4");
                    txtRiskDifferenceEstimate.Text = singleTableResults.RiskDifferenceEstimate.ToString("F4");
                    txtRiskDifferenceLower.Text = singleTableResults.RiskDifferenceLower.ToString("F4");
                    txtRiskDifferenceUpper.Text = singleTableResults.RiskDifferenceUpper.ToString("F4");
                    txtRiskRatioEstimate.Text = riskRatioEstimate; // singleTableResults.RiskRatioEstimate.ToString("F4");
                    txtRiskRatioLower.Text = riskRatioLower; //singleTableResults.RiskRatioLower.ToString("F4");
                    txtRiskRatioUpper.Text = riskRatioUpper; //singleTableResults.RiskRatioUpper.ToString("F4");
                    txtFisherExact.Text = singleTableResults.FisherExactP.ToString("F8");
                    txtMidPExact.Text = singleTableResults.MidP.ToString("F8");
                    txtFisherExact2P.Text = fisherExact2P;

                    this.currentSingleTableResults = singleTableResults;
                }
            }
            catch (Exception ex)
            {
                //
            }
        }

        void txtInputs_TextChanged(object sender, TextChangedEventArgs e)
        {
            int strata = int.Parse(((FrameworkElement)sender).Name.Substring(((FrameworkElement)sender).Name.Length - 1));
            CalculateStrata(strata);
            CalculateSummary();
        }
			//EI-14
        protected virtual void txtInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool isNumPadNumeric = (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || e.Key == Key.Decimal;
            bool isNumeric = (e.Key >= Key.D0 && e.Key <= Key.D9) || e.Key == Key.OemPeriod;

            if (System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ",")
            {
                isNumeric = (e.Key >= Key.D0 && e.Key <= Key.D9) || e.Key == Key.OemComma;
            }

            if ((isNumeric || isNumPadNumeric) && Keyboard.Modifiers != ModifierKeys.None)
            {
                e.Handled = true;
                return;
            }
            bool isControl = ((Keyboard.Modifiers != ModifierKeys.None && Keyboard.Modifiers != ModifierKeys.Shift) || e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Insert || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Tab || e.Key == Key.PageDown || e.Key == Key.PageUp || e.Key == Key.Enter || e.Key == Key.Return || e.Key == Key.Escape || e.Key == Key.Home || e.Key == Key.End);
            e.Handled = !isControl && !isNumeric && !isNumPadNumeric;
        }

        private void CalculateSummary()
        {
            System.ComponentModel.BackgroundWorker worker = new System.ComponentModel.BackgroundWorker();
            worker.DoWork += new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.RunWorkerAsync();
        }

        void worker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                string[] results = (string[])e.Result;
                double tempA;
                double tempB;
                double tempC;
                bool tempABool = double.TryParse(results[0], out tempA);
                bool tempBBool = double.TryParse(results[1], out tempB);
                bool tempCBool = double.TryParse(results[2], out tempC);
                if (tempABool && tempBBool && tempCBool && (tempA > 0.0 || tempB > 0.0 || tempC > 0.0))
                {
                    tblockStratAdjustedMle.Visibility = System.Windows.Visibility.Visible;
                    txtStratAdjustedMle.Visibility = System.Windows.Visibility.Visible;
                    txtStratAdjustedMleLower.Visibility = System.Windows.Visibility.Visible;
                    txtStratAdjustedMleUpper.Visibility = System.Windows.Visibility.Visible;
                    tblockStratAdjustedMleRectangle.Visibility = System.Windows.Visibility.Visible;
                    txtStratAdjustedMleRectangle.Visibility = System.Windows.Visibility.Visible;
                    txtStratAdjustedMleLowerRectangle.Visibility = System.Windows.Visibility.Visible;
                    txtStratAdjustedMleUpperRectangle.Visibility = System.Windows.Visibility.Visible;
                    txtStratAdjustedMle.Text = results[0];
                    txtStratAdjustedMleLower.Text = results[1];
                    txtStratAdjustedMleUpper.Text = results[2];
                }
                else
                {
                    tblockStratAdjustedMle.Visibility = System.Windows.Visibility.Collapsed;
                    txtStratAdjustedMle.Visibility = System.Windows.Visibility.Collapsed;
                    txtStratAdjustedMleLower.Visibility = System.Windows.Visibility.Collapsed;
                    txtStratAdjustedMleUpper.Visibility = System.Windows.Visibility.Collapsed;
                    tblockStratAdjustedMleRectangle.Visibility = System.Windows.Visibility.Collapsed;
                    txtStratAdjustedMleRectangle.Visibility = System.Windows.Visibility.Collapsed;
                    txtStratAdjustedMleLowerRectangle.Visibility = System.Windows.Visibility.Collapsed;
                    txtStratAdjustedMleUpperRectangle.Visibility = System.Windows.Visibility.Collapsed;
                }


                txtStratAdjustedRr.Text = results[3];
                txtStratAdjustedRrLower.Text = results[4];
                txtStratAdjustedRrUpper.Text = results[5];

                txtStratCrudeOr.Text = results[6];
                txtStratCrudeOrLower.Text = results[7];
                txtStratCrudeOrUpper.Text = results[8];

                tempABool = double.TryParse(results[9], out tempA);
                tempBBool = double.TryParse(results[10], out tempB);
                tempCBool = double.TryParse(results[11], out tempC);
                if (tempABool && tempBBool && tempCBool && (tempA > 0.0 || tempB > 0.0 || tempC > 0.0))
                {
                    System.Windows.GridLength grl = new System.Windows.GridLength(30);
                    crudeMLERow.Height = grl;
                    txtStratCrudeMle.Text = results[9];
                    txtStratCrudeMleLower.Text = results[10];
                    txtStratCrudeMleUpper.Text = results[11];
                }
                else
                {
                    System.Windows.GridLength grl = new System.Windows.GridLength(0);
                    crudeMLERow.Height = grl;
                }

                tempBBool = double.TryParse(results[12], out tempB);
                tempCBool = double.TryParse(results[13], out tempC);
                if (tempBBool && tempCBool && (tempB > 0.0 || tempC > 0.0))
                {
                    System.Windows.GridLength grl = new System.Windows.GridLength(30);
                    crudeFisherExactRow.Height = grl;
                    txtStratFisherLower.Text = results[12];
                    txtStratFisherUpper.Text = results[13];
                }
                else
                {
                    System.Windows.GridLength grl = new System.Windows.GridLength(0);
                    crudeFisherExactRow.Height = grl;
                }

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
            }
        }

        void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                List<double> yyList = new List<double>();
                List<double> ynList = new List<double>();
                List<double> nyList = new List<double>();
                List<double> nnList = new List<double>();

                for (int x = 1; x <= 9; x++)
                {
                    if (!strataActive[x].Contains(false))
                    {
                        yyList.Add(strataVals[x][0]);
                        ynList.Add(strataVals[x][1]);
                        nyList.Add(strataVals[x][2]);
                        nnList.Add(strataVals[x][3]);
                    }
                }
                double[] yyArr = yyList.ToArray();
                double[] ynArr = ynList.ToArray();
                double[] nyArr = nyList.ToArray();
                double[] nnArr = nnList.ToArray();

                double yySum = yyList.Sum();
                double ynSum = ynList.Sum();
                double nySum = nyList.Sum();
                double nnSum = nnList.Sum();

                if (yyList.Count > 1)
                {
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
                double corrChisqP = (double)strat2x2.pForChisq(corrChisq); string[] results = new string[24];
                results[0] = mleOR.ToString("F4");
                results[1] = ExactORLL.ToString("F4");
                results[2] = ExactORUL.ToString("F4");

                results[3] = computedRR.ToString("F4");
                results[4] = computedRRMHLL.ToString("F4");
                results[5] = computedRRMHUL.ToString("F4");

                try
                {
                    results[6] = singleTableResults.OddsRatioEstimate.Value.ToString("F4");
                }
                catch (Exception ex)
                {
                    //
                }
                try
                {
                    results[7] = singleTableResults.OddsRatioLower.Value.ToString("F4");
                }
                catch (Exception ex)
                {
                    //
                }
                try
                {
                    results[8] = singleTableResults.OddsRatioUpper.Value.ToString("F4");
                }
                catch (Exception ex)
                {
                    //
                }

                results[9] = singleTableResults.OddsRatioMLEEstimate.ToString("F4");
                results[10] = singleTableResults.OddsRatioMLEMidPLower.ToString("F4");
                results[11] = singleTableResults.OddsRatioMLEMidPUpper.ToString("F4");

                results[12] = singleTableResults.OddsRatioMLEFisherLower.ToString("F4");
                results[13] = singleTableResults.OddsRatioMLEFisherUpper.ToString("F4");

                results[14] = singleTableResults.RiskRatioEstimate.Value.ToString("F4");
                results[15] = singleTableResults.RiskRatioLower.Value.ToString("F4");
                results[16] = singleTableResults.RiskRatioUpper.Value.ToString("F4");

                results[17] = computedOddsRatio.ToString("F4");
                results[18] = computedOddsRatioMHLL.ToString("F4");
                results[19] = computedOddsRatioMHUL.ToString("F4");

                results[20] = uncorrectedChiSquare.ToString("F4");
                results[21] = uncorrectedChiSquareP.ToString("F10");

                results[22] = corrChisq.ToString("F4");
                results[23] = corrChisqP.ToString("F10");

                e.Result = results;
                }
            }
            catch (Exception ex)
            {
                //
            }
        }

        /// <summary>
        /// Updates the variable names available in the gadget's properties
        /// </summary>
        public void UpdateVariableNames()
        {
            //FillComboboxes(true);
        }

        public void RefreshResults()
        {
            
        }

        public XmlNode Serialize(XmlDocument doc)
        {
            throw new NotImplementedException();
        }

        public void CreateFromXml(XmlElement element)
        {
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public string ToHTML(string htmlFileName = "", int count = 0, bool useAlternatingColors = false, bool ForWeb = false)
        {
            if (string.IsNullOrEmpty(txtYesYesVal1.Text) || string.IsNullOrEmpty(txtYesNoVal1.Text) || string.IsNullOrEmpty(txtNoYesVal1.Text) || string.IsNullOrEmpty(txtNoNoVal1.Text))
            {
                return string.Empty;
            }

            StringBuilder htmlBuilder = new StringBuilder();
            Epi.Configuration config = Epi.Configuration.GetNewInstance();

            htmlBuilder.AppendLine("<h2>StatCalc 2x2 Table</h2>");

            htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
            htmlBuilder.AppendLine("<table align=\"left\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");            

            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <td></td>");
            htmlBuilder.AppendLine("  <th colspan=\"2\">Outcome</th>");
            htmlBuilder.AppendLine("  <td></td>");
            htmlBuilder.AppendLine(" </tr>");

            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <th>Exposure</th>");
            htmlBuilder.AppendLine("  <th>" + config.Settings.RepresentationOfYes + "</th>");
            htmlBuilder.AppendLine("  <th>" + config.Settings.RepresentationOfNo + "</th>");
            htmlBuilder.AppendLine("  <th>Total</th>");
            htmlBuilder.AppendLine(" </tr>");

            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <th>" + config.Settings.RepresentationOfYes + "<br/><small>Row %<br/>Col %</small></th>");
            htmlBuilder.AppendLine("  <td>" + txtYesYesVal1.Text + "<br/><small>" + txtYesYesRow1.Text + "<br/>" + txtYesYesCol1.Text + "</small></td>");
            htmlBuilder.AppendLine("  <td>" + txtYesNoVal1.Text + "<br/><small>" + txtYesNoRow1.Text + "<br/>" + txtYesNoCol1.Text + "</small></td>");
            htmlBuilder.AppendLine("  <td>" + txtYesTotalVal1.Text + "<br/><small>" + 1.ToString("P") + "<br/>" + txtYesTotalCol1.Text + "</small></td>");
            htmlBuilder.AppendLine(" </tr>");

            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <th>" + config.Settings.RepresentationOfNo + "<br/><small>Row %<br/>Col %</small></th>");
            htmlBuilder.AppendLine("  <td>" + txtNoYesVal1.Text + "<br/><small>" + txtNoYesRow1.Text + "<br/>" + txtNoYesCol1.Text + "</small></td>");
            htmlBuilder.AppendLine("  <td>" + txtNoNoVal1.Text + "<br/><small>" + txtNoNoRow1.Text + "<br/>" + txtNoNoCol1.Text + "</small></td>");
            htmlBuilder.AppendLine("  <td>" + txtNoTotalVal1.Text + "<br/><small>" + 1.ToString("P") + "<br/>" + txtNoTotalCol1.Text + "</small></td>");
            htmlBuilder.AppendLine(" </tr>");

            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <th>Total<br/><small>Row %<br/>Col %</small></th>");
            htmlBuilder.AppendLine("  <td>" + txtTotalYesVal1.Text + "<br/><small>" + txtTotalYesRow1.Text + "<br/>" + 1.ToString("P") + "</small></td>");
            htmlBuilder.AppendLine("  <td>" + txtTotalNoVal1.Text + "<br/><small>" + txtTotalNoRow1.Text + "<br/>" + 1.ToString("P") + "</small></td>");
            htmlBuilder.AppendLine("  <td>" + txtTotalTotalVal1.Text + "<br/><small>" + 1.ToString("P") + "<br/>" + 1.ToString("P") + "</small></td>");
            htmlBuilder.AppendLine(" </tr>");

            htmlBuilder.AppendLine("</table>");

            int redValue = (int)((double.Parse(txtYesYesVal1.Text) / double.Parse(txtTotalTotalVal1.Text)) * 200);
            int orangeValue = (int)((double.Parse(txtYesNoVal1.Text) / double.Parse(txtTotalTotalVal1.Text)) * 200);
            int yellowValue = (int)((double.Parse(txtNoYesVal1.Text) / double.Parse(txtTotalTotalVal1.Text)) * 200);
            int greenValue = (int)((double.Parse(txtNoNoVal1.Text) / double.Parse(txtTotalTotalVal1.Text)) * 200);

            htmlBuilder.AppendLine("<table class=\"twoByTwoColoredSquares\" align=\"left\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <td class=\"twobyTwoColoredSquareCell\" style=\"vertical-align: bottom; text-align: right;\">");
            htmlBuilder.AppendLine("   <div style=\"float: right; vertical-align: bottom; background-color: red; height: " + redValue.ToString() + "px; width: " + redValue.ToString() + "px;\"></div>");
            htmlBuilder.AppendLine("  </td>");
            htmlBuilder.AppendLine("  <td class=\"twobyTwoColoredSquareCell\" style=\"vertical-align: bottom; text-align: left;\">");
            htmlBuilder.AppendLine("   <div style=\"vertical-align: bottom; background-color: orange; height: " + orangeValue.ToString() + "px; width: " + orangeValue.ToString() + "px;\"></div>");
            htmlBuilder.AppendLine("  </td>");
            htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <td class=\"twobyTwoColoredSquareCell\" style=\"vertical-align: top; text-align: right;\">");
            htmlBuilder.AppendLine("   <div style=\"float: right; vertical-align: top; background-color: yellow; height: " + yellowValue.ToString() + "px; width: " + yellowValue.ToString() + "px;\"></div>");
            htmlBuilder.AppendLine("  </td>");
            htmlBuilder.AppendLine("  <td class=\"twobyTwoColoredSquareCell\" style=\"vertical-align: top; text-align: left;\">");
            htmlBuilder.AppendLine("   <div style=\"vertical-align: top; background-color: green; height: " + greenValue.ToString() + "px; width: " + greenValue.ToString() + "px;\"></div>");
            htmlBuilder.AppendLine("  </td>");
            htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine("</table>");

            htmlBuilder.AppendLine("<br style=\"clear: all\">");

            htmlBuilder.AppendLine("<br clear=\"all\" />");

            string fisherExact = Epi.SharedStrings.UNDEFINED;
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

            if (currentSingleTableResults.FisherExactP != -1)
            {
                fisherExact = ((double)currentSingleTableResults.FisherExactP).ToString("F10");
            }

            if (currentSingleTableResults.OddsRatioMLEFisherLower != -1)
            {
                fisherLower = ((double)currentSingleTableResults.OddsRatioMLEFisherLower).ToString("F4");
            }

            if (currentSingleTableResults.OddsRatioMLEFisherUpper != -1)
            {
                fisherUpper = ((double)currentSingleTableResults.OddsRatioMLEFisherUpper).ToString("F4");
            }

            if (currentSingleTableResults.OddsRatioMLEEstimate != -1)
            {
                oddsRatioMLEEstimate = ((double)currentSingleTableResults.OddsRatioMLEEstimate).ToString("F4");
            }

            if (currentSingleTableResults.OddsRatioMLEMidPLower != -1)
            {
                oddsRatioMLEMidPLower = ((double)currentSingleTableResults.OddsRatioMLEMidPLower).ToString("F4");
            }

            if (currentSingleTableResults.OddsRatioMLEMidPUpper != -1)
            {
                oddsRatioMLEMidPUpper = ((double)currentSingleTableResults.OddsRatioMLEMidPUpper).ToString("F4");
            }

            if (currentSingleTableResults.OddsRatioEstimate != null)
            {
                oddsRatioEstimate = ((double)currentSingleTableResults.OddsRatioEstimate).ToString("F4");
            }

            if (currentSingleTableResults.OddsRatioLower != null)
            {
                oddsRatioLower = ((double)currentSingleTableResults.OddsRatioLower).ToString("F4");
            }

            if (currentSingleTableResults.OddsRatioUpper != null)
            {
                oddsRatioUpper = ((double)currentSingleTableResults.OddsRatioUpper).ToString("F4");
            }

            if (currentSingleTableResults.RiskRatioEstimate != null)
            {
                riskRatioEstimate = ((double)currentSingleTableResults.RiskRatioEstimate).ToString("F4");
            }

            if (currentSingleTableResults.RiskRatioLower != null)
            {
                riskRatioLower = ((double)currentSingleTableResults.RiskRatioLower).ToString("F4");
            }

            if (currentSingleTableResults.RiskRatioUpper != null)
            {
                riskRatioUpper = ((double)currentSingleTableResults.RiskRatioUpper).ToString("F4");
            }


            string chiSqCorP = currentSingleTableResults.ChiSquareYates2P.ToString();
            string chiSqCorVal = currentSingleTableResults.ChiSquareYatesVal.ToString("F4");
            string chiSqManP = currentSingleTableResults.ChiSquareMantel2P.ToString();
            string chiSqManVal = currentSingleTableResults.ChiSquareMantelVal.ToString("F4");
            string chiSqUncP = currentSingleTableResults.ChiSquareUncorrected2P.ToString();
            string chiSqUncVal = currentSingleTableResults.ChiSquareUncorrectedVal.ToString("F4");
            string riskDifferenceEstimate = currentSingleTableResults.RiskDifferenceEstimate.ToString("F4");
            string riskDifferenceLower = currentSingleTableResults.RiskDifferenceLower.ToString("F4");
            string riskDifferenceUpper = currentSingleTableResults.RiskDifferenceUpper.ToString("F4");
            //string riskRatioEstimate = currentSingleTableResults.RiskRatioEstimate.ToString("F4");
            string midPExact = currentSingleTableResults.MidP.ToString("F10");

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
            htmlBuilder.AppendLine("  <td class=\"stats\">Fisher exact</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            htmlBuilder.AppendLine("  <td class=\"stats\" align=\"right\">" + fisherExact + "</td>");
            htmlBuilder.AppendLine("  <td class=\"stats\"></td>");
            htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine("</table>");
            htmlBuilder.AppendLine("<br clear=\"all\" /><br clear=\"all\" />");

            return htmlBuilder.ToString();
        }

        public string CustomOutputHeading { get { return string.Empty; } set { } }
        public string CustomOutputDescription { get { return string.Empty; } set { } }
        public string CustomOutputCaption { get { return string.Empty; } set { } }

        public void HideCloseIcon()
        {
            imgClose.Visibility = Visibility.Collapsed;
        }

        public int PreferredUIHeight
        {
            get { return 620; }
        }

        public int PreferredUIWidth
        {
            get { return 945; }
        }

        public bool IsProcessing
        {
            get
            {
                return false;
            }
            set
            {
                isProcessing = value;
            }
        }

        /// <summary>
        /// Sets the gadget to its 'processing' state
        /// </summary>
        public void SetGadgetToProcessingState()
        {
        }

        /// <summary>
        /// Sets the gadget to its 'finished' state
        /// </summary>
        public void SetGadgetToFinishedState()
        {
            if (GadgetProcessingFinished != null)
                GadgetProcessingFinished(this);
        }

        public bool DrawBorders { get; set; } 
    }
}
