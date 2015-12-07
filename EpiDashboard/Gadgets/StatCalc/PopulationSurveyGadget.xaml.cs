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
    /// Interaction logic for PopulationSurveyGadget.xaml
    /// </summary>
    public partial class PopulationSurveyGadget : StatCalcGadgetBase, IStatCalcControl
    {
        #region Private Variables
        #endregion

        #region Delegates
        #endregion

        #region Constructors

        public PopulationSurveyGadget()
        {
            InitializeComponent();
            Construct();
        }

        protected override void Construct()
        {
            this.IsProcessing = false;

            txtPopulationSize.TextChanged += new TextChangedEventHandler(txtInput_TextChanged);
            txtExpectedFreq.TextChanged += new TextChangedEventHandler(txtInput_TextChanged);
            txtConfidenceLimits.TextChanged += new TextChangedEventHandler(txtInput_TextChanged);
            txtDesignEffect.TextChanged += new TextChangedEventHandler(txtInput_TextChanged);
            txtNumberOfClusters.TextChanged += new TextChangedEventHandler(txtInput_TextChanged);

            txtPopulationSize.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtExpectedFreq.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtConfidenceLimits.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtDesignEffect.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtNumberOfClusters.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);

            txtPopulationSize.Text = "999999";
            txtExpectedFreq.Text = "50";
            txtConfidenceLimits.Text = "5";
            string separator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            txtDesignEffect.Text = "1" + separator + "0";
            txtNumberOfClusters.Text = "1";

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
            return "Population Survey";
        }

        protected override void txtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPopulationSize.Text) && !string.IsNullOrEmpty(txtExpectedFreq.Text) && !string.IsNullOrEmpty(txtConfidenceLimits.Text) && !string.IsNullOrEmpty(txtDesignEffect.Text) && !string.IsNullOrEmpty(txtNumberOfClusters.Text))
            {
                int populationSize = 0;
                double expectedFreq = 0;
                double confidenceLimits = 0;
                double de = 1.0;
                int clusters = 1;
                bool parseResult1 = int.TryParse(txtPopulationSize.Text, out populationSize);
                bool parseResult2 = double.TryParse(txtExpectedFreq.Text, out expectedFreq);
                bool parseResult3 = double.TryParse(txtConfidenceLimits.Text, out confidenceLimits);
                bool parseResult4 = double.TryParse(txtDesignEffect.Text, out de);
                bool parseResult5 = int.TryParse(txtNumberOfClusters.Text, out clusters);

                if (parseResult1 && parseResult2 && parseResult3 && parseResult4 && parseResult5)
                {
                    int[] res = CalculateSampleSizes(populationSize, expectedFreq, confidenceLimits, de, clusters);
                    txt80.Text = res[0].ToString();
                    txt90.Text = res[1].ToString();
                    txt95.Text = res[2].ToString();
                    txt97.Text = res[3].ToString();
                    txt99.Text = res[4].ToString();
                    txt999.Text = res[5].ToString();
                    txt9999.Text = res[6].ToString();
                    ttxt80.Text = (clusters * res[0]).ToString();
                    ttxt90.Text = (clusters * res[1]).ToString();
                    ttxt95.Text = (clusters * res[2]).ToString();
                    ttxt97.Text = (clusters * res[3]).ToString();
                    ttxt99.Text = (clusters * res[4]).ToString();
                    ttxt999.Text = (clusters * res[5]).ToString();
                    ttxt9999.Text = (clusters * res[6]).ToString();
                }
            }
        }

        public int[] CalculateSampleSizes(int pop, double freq, double worst, double de, int clusters)
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
                sizes[i] = (int)Math.Ceiling(de * (double)sizes[i] / (double)clusters);
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
