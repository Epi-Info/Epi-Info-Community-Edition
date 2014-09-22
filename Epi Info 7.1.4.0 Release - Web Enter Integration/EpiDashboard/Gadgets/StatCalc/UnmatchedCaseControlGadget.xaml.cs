using System;
using System.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Epi;
using Epi.Data;
using Epi.Fields;
using EpiDashboard.Rules;

namespace EpiDashboard.Gadgets.StatCalc
{
    /// <summary>
    /// Interaction logic for UnmatchedCaseControlGadget.xaml
    /// </summary>
    public partial class UnmatchedCaseControlGadget : StatCalcGadgetBase, IStatCalcControl
    {
        #region Private Variables
        #endregion

        #region Delegates
        #endregion

        #region Constructors

        public UnmatchedCaseControlGadget()
        {
            InitializeComponent();
            Construct();
        }

        protected override void Construct()
        {
            this.IsProcessing = false;

            txtPower.TextChanged += new TextChangedEventHandler(txtInput_TextChanged);
            txtRatioControlsExposed.TextChanged += new TextChangedEventHandler(txtInput_TextChanged);
            txtPctControlsExposed.TextChanged += new TextChangedEventHandler(txtInput_TextChanged);
            txtOddsRatio.TextChanged += new TextChangedEventHandler(txtInput_TextChanged);            
            txtPctCasesWithExposure.TextChanged += new TextChangedEventHandler(txtInput_TextChanged);
            cbxConfidenceLevel.SelectionChanged += new SelectionChangedEventHandler(cbxConfidenceLevel_SelectionChanged);

            txtPower.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtRatioControlsExposed.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtPctControlsExposed.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtOddsRatio.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);            
            txtPctCasesWithExposure.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new SimpleCallback(DoFocus));

            base.Construct();
        }
        
        #endregion

        #region Public Properties
        public int PreferredUIHeight 
        {
            get
            {
                return 350;
            }
        }
        public int PreferredUIWidth
        {
            get
            {
                return 530;
            }
        }
        #endregion // Public Properties

        #region Public Methods
        #endregion

        #region Event Handlers
        #endregion

        #region Private Methods
        /// <summary>
        /// Closes the gadget
        /// </summary>
        protected override void CloseGadget()
        {
            base.CloseGadget();
            GadgetOptions = null;
        }

        #endregion

        #region Private Properties
        #endregion // Private Properties

        /// <summary>
        /// Returns the gadget's description as a string.
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return "Unmatched Case Control";
        }

        private void DoFocus()
        {
            txtPower.Focus();
        }

        private void Calculate()
        {
            string confidenceRaw = ((ComboBoxItem)cbxConfidenceLevel.SelectedItem).Content.ToString().Split('%')[0];
            double confidence = (100.0 - double.Parse(confidenceRaw, System.Globalization.CultureInfo.InvariantCulture)) / 100.0;
            double power = 0;
            double.TryParse(txtPower.Text, out power);

            double controlRatio = 0;
            if (txtRatioControlsExposed.Text.Length > 0)
            {
                double.TryParse(txtRatioControlsExposed.Text, out controlRatio);
            }
            double percentExposed = 0;
            double.TryParse(txtPctControlsExposed.Text, out percentExposed);
            percentExposed = percentExposed / 100.0;

            double oddsRatio = 0;
            if (txtOddsRatio.Text.Length > 0)
            {
                double.TryParse(txtOddsRatio.Text, out oddsRatio);
            }
            double percentCasesExposure = 0;
            double.TryParse(txtPctCasesWithExposure.Text, out percentCasesExposure);
            percentCasesExposure = percentCasesExposure / 100.0;
            Calculate(confidence, power, controlRatio, percentExposed, oddsRatio, percentCasesExposure);
        }

        void cbxConfidenceLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Calculate();
        }

        protected override void txtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            double percentExposed = 0;
            double.TryParse(txtPctControlsExposed.Text, out percentExposed);
            percentExposed = percentExposed / 100.0;

            if ((sender == txtOddsRatio) || (sender == txtPctControlsExposed))
            {
                if (!txtPctCasesWithExposure.IsFocused)
                {
                    double oddsRatio = 0;
                    double.TryParse(txtOddsRatio.Text, out oddsRatio);
                    txtPctCasesWithExposure.Text = OddsToPercentCases(oddsRatio, percentExposed).ToString("N1");
                }
            }
            else if (sender == txtPctCasesWithExposure)
            {
                if (!txtOddsRatio.IsFocused)
                {
                    double percentCasesExposure = 0;
                    double.TryParse(txtPctCasesWithExposure.Text, out percentCasesExposure);
                    percentCasesExposure = percentCasesExposure / 100.0;
                    txtOddsRatio.Text = PercentCasesToOdds(percentCasesExposure, percentExposed).ToString();
                }
            }
            Calculate();
        }

        private void Calculate(double a, double b, double vr, double v2, double vor, double v1)
        {
            //95,80,1.0,40,10,86.96
            //UnmatchedCaseControl(0.05, 80.0, 1.0, 0.4, 10.0, 0.8696);

            double Za = ANorm(a);
            double Zb = 0;

            if (b >= 1)
            {
                b = b / 100.0;
            }
            if (b < 0.5)
            {
                Zb = -ANorm(2.0 * b);
            }
            else
            {
                Zb = ANorm(2.0 - 2.0 * b);
            }
            if (vor != 0)
            {
                v1 = v2 * vor / (1.0 + v2 * (vor - 1.0));
            }
            double pbar = (v1 + vr * v2) / (1.0 + vr);
            double qbar = 1.0 - pbar;
            double vn = ((Math.Pow((Za + Zb), 2.0)) * pbar * qbar * (vr + 1.0)) / ((Math.Pow((v1 - v2), 2.0)) * vr);
            double vn1 = Math.Pow(((Za * Math.Sqrt((vr + 1.0) * pbar * qbar)) + (Zb * Math.Sqrt((vr * v1 * (1.0 - v1)) + (v2 * (1.0 - v2))))), 2.0) / (vr * Math.Pow((v2 - v1), 2.0));
            double vn2 = Math.Pow(Za * Math.Sqrt((vr + 1.0) * pbar * qbar) + Zb * Math.Sqrt(vr * v1 * (1.0 - v1) + v2 * (1.0 - v2)), 2.0) / (vr * Math.Pow(Math.Abs(v1 - v2), 2.0));
            vn2 = vn2 * Math.Pow((1.0 + Math.Sqrt(1.0 + 2.0 * (vr + 1.0) / (vn2 * vr * Math.Abs(v2 - v1)))), 2.0) / 4.0;

            txtKelseyCases.Text = Math.Ceiling(vn).ToString();
            txtKelseyControls.Text = Math.Ceiling(vn * vr).ToString();
            txtKelseyTotal.Text = (Math.Ceiling(vn) + Math.Ceiling(vn * vr)).ToString();

            txtFleissCases.Text = Math.Ceiling(vn1).ToString();
            txtFleissControls.Text = Math.Ceiling(vn1 * vr).ToString();
            txtFleissTotal.Text = (Math.Ceiling(vn1) + Math.Ceiling(vn1 * vr)).ToString();

            txtFleissCCCases.Text = Math.Ceiling(vn2).ToString();
            txtFleissCCControls.Text = Math.Ceiling(vn2 * vr).ToString();
            txtFleissCCTotal.Text = (Math.Ceiling(vn2) + Math.Ceiling(vn2 * vr)).ToString();
        }

        public void Cohort(double a, double b, double vr, double v2, double vor, double v1, double rr, double dd)
        {
            double Za = ANorm(a);
            if (b >= 1)
            {
                b = b / 100.0;
            }
            double Zb;
            if (b < 0.5)
            {
                Zb = -ANorm(2.0 * b);
            }
            else
            {
                Zb = ANorm(2.0 - (2.0 * b));
            }
            if (vor != 0)
            {
                v1 = v2 * vor / (1.0 + v2 * (vor - 1.0));
            }
            double pbar = (v1 + vr * v2) / (1.0 + vr);
            double qbar = 1.0 - pbar;
            double vn = ((Math.Pow((Za + Zb), 2.0)) * pbar * qbar * (vr + 1.0)) / ((Math.Pow((v1 - v2), 2.0)) * vr);
            double vn1 = Math.Pow(((Za * Math.Sqrt((vr + 1.0) * pbar * qbar)) + (Zb * Math.Sqrt((vr * v1 * (1.0 - v1)) + (v2 * (1.0 - v2))))), 2.0) / (vr * Math.Pow((v2 - v1), 2.0));
            double vn2 = Math.Pow(Za * Math.Sqrt((vr + 1.0) * pbar * qbar) + Zb * Math.Sqrt(vr * v1 * (1.0 - v1) + v2 * (1.0 - v2)), 2.0) / (vr * Math.Pow(Math.Abs(v1 - v2), 2.0));
            vn2 = vn2 * Math.Pow((1.0 + Math.Sqrt(1.0 + 2.0 * (vr + 1.0) / (vn2 * vr * Math.Abs(v2 - v1)))), 2.0) / 4.0;
        }

        private double ANorm(double p)
        {
            double v = 0.5;
            double dv = 0.5;
            double z = 0;

            while (dv > 1e-6)
            {
                z = 1.0 / v - 1.0;
                dv = dv / 2.0;
                if (Norm(z) > p)
                {
                    v = v - dv;
                }
                else
                {
                    v = v + dv;
                }
            }

            return z;
        }

        private double Norm(double z)
        {
            z = Math.Sqrt(z * z);
            double p = 1.0 + z * (0.04986735 + z * (0.02114101 + z * (0.00327763 + z * (0.0000380036 + z * (0.0000488906 + z * 0.000005383)))));
            p = p * p; p = p * p; p = p * p;
            return 1.0 / (p * p);
        }

        public double OddsToPercentCases(double oddsRatio, double percentControls)
        {
            double rawVal = 0;
            if (oddsRatio != 0)
            {
                rawVal = 100.0 * percentControls * oddsRatio / (1.0 + percentControls * (oddsRatio - 1.0));
            }
            return Math.Round(100000.0 * rawVal) / 100000.0;
        }

        public double PercentCasesToOdds(double percentCases, double percentControls)
        {
            return Math.Round(100000.0 * (percentCases * (1.0 - percentControls)) / (percentControls * (1.0 - percentCases))) / 100000.0;
        }
    }
}
