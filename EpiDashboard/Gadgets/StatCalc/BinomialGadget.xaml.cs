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
    /// Interaction logic for BinomialGadget.xaml
    /// </summary>
    public partial class BinomialGadget : StatCalcGadgetBase, IStatCalcControl
    {
        #region Private Variables
        private StatisticsRepository.Strat2x2 strat2x2;
        #endregion

        #region Delegates
        #endregion

        #region Constructors

        public BinomialGadget()
        {
            InitializeComponent();
            Construct();
        }

        protected override void Construct()
        {
            this.IsProcessing = false;

            strat2x2 = new StatisticsRepository.Strat2x2();
            txtNumerator.TextChanged += new TextChangedEventHandler(txtInput_TextChanged);
            txtObserved.TextChanged += new TextChangedEventHandler(txtInput_TextChanged);
            txtExpected.TextChanged += new TextChangedEventHandler(txtInput_TextChanged);

            txtNumerator.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtObserved.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtExpected.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);

            txtNumerator.Text = "8";
            txtObserved.Text = "16";
            txtExpected.Text = "23.5";

            base.Construct();
        }
        
        #endregion

        #region Public Properties

        public int PreferredUIHeight 
        {
            get
            {
                return 235;
            }
        }
        public int PreferredUIWidth
        {
            get
            {
                return 435;
            }
        }
        #endregion // Public Properties

        #region Public Methods
        #endregion

        #region Event Handlers
        protected override void txtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtNumerator.Text) && !string.IsNullOrEmpty(txtObserved.Text) && !string.IsNullOrEmpty(txtExpected.Text))
            {
                int numerator = 0;
                int observed = 0;
                double expected = 0;
                double lessThan = 0;
                double lessThanEqual = 0;
                double equal = 0;
                double greaterThanEqual = 0;
                double greaterThan = 0;
                double pValue = 0;
                int lcl = 0;
                int ucl = 0;
                bool parseResult1 = int.TryParse(txtNumerator.Text, out numerator);
                bool parseResult2 = int.TryParse(txtObserved.Text, out observed);
                bool parseResult3 = double.TryParse(txtExpected.Text, out expected);

                if (parseResult1 && parseResult2 && parseResult3 && numerator < observed)
                {
                    lblLessThan.Text = "< " + numerator;
                    lblLessThanEqual.Text = "<= " + numerator;
                    lblEqual.Text = "= " + numerator;
                    lblGreaterThanEqual.Text = ">= " + numerator;
                    lblGreaterThan.Text = "> " + numerator;

                    strat2x2.binpdf(observed, numerator, (expected / 100.0), ref lessThan, ref lessThanEqual, ref equal, ref greaterThanEqual, ref greaterThan, ref pValue, ref lcl, ref ucl);

                    txtLessThan.Text = lessThan.ToString("F7");
                    txtLessThanEqual.Text = lessThanEqual.ToString("F7");
                    txtEqual.Text = equal.ToString("F7");
                    txtGreaterThanEqual.Text = greaterThanEqual.ToString("F7");
                    txtGreaterThan.Text = greaterThan.ToString("F7");
                    txtPValue.Text = pValue.ToString("F7");
                    txt95Ci.Text = lcl + " - " + ucl;
                    if (lcl == -1 || ucl == -1)
                        txt95Ci.Text = "Cannot compute";
                }
                else
                {
                    txtLessThan.Text = "";
                    txtLessThanEqual.Text = "";
                    txtEqual.Text = "";
                    txtGreaterThanEqual.Text = "";
                    txtGreaterThan.Text = "";
                    txtPValue.Text = "";
                    txt95Ci.Text = "";
                }
            }
        }
        
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
            return "Binomial";
        }

        public int[] CalculateSampleSizes(int pop, double freq, double worst)
        {
            double[] percentiles = new double[] { 0.80, 0.90, 0.95, 0.97, 0.99, 0.999, 0.9999 };
            int[] sizes = new int[7];
            //double d = Math.abs(freq - worst);
            double d = Math.Abs(worst);
            double factor = freq * (100 - freq) / (d * d);
            for (int i = 0; i < percentiles.Length; i++)
            {
                double twoTail = ANorm(1 - percentiles[i]);
                double n = twoTail * twoTail * factor;
                double sampleSize = n / (1 + (n / pop));
                sizes[i] = (int)Math.Round(sampleSize);
            }
            return sizes;
        }

        public void UnmatchedCaseControl(double a, double b, double vr, double v2, double vor, double v1)
        {
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

        private double OddsToPercentCases(double oddsRatio, double percentControls)
        {
            double rawVal = 0;
            if (oddsRatio != 0)
            {
                rawVal = 100 * percentControls * oddsRatio / (1 + percentControls * (oddsRatio - 1));
            }
            return Math.Round(100000 * rawVal) / 100000;
        }

        private double PercentCasesToOdds(double percentCases, double percentControls)
        {
            return Math.Round(100000 * (percentCases * (1 - percentControls)) / (percentControls * (1 - percentCases))) / 100000;
        }
    }
}
