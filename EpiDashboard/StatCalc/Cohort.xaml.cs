using System;
using System.Collections.Generic;
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
    /// Interaction logic for UnmatchedCaseControl.xaml
    /// </summary>
    public partial class Cohort : UserControl, IGadget, IStatCalcControl
    {
        public event GadgetRefreshedHandler GadgetRefreshed;
        private delegate void SimpleDelegate();
        public event GadgetClosingHandler GadgetClosing;
        public event GadgetProcessingFinishedHandler GadgetProcessingFinished;
        public event GadgetRepositionEventHandler GadgetReposition;
        public event GadgetCheckForCancellationHandler GadgetCheckForCancellation;
        public event GadgetEventHandler GadgetDragStart;
        public event GadgetEventHandler GadgetDragStop;
        public event GadgetEventHandler GadgetDrag;
        public event GadgetAnchorSetEventHandler GadgetAnchorSetFromXml;

        public UserControl AnchorLeft { get; set; }
        public UserControl AnchorTop { get; set; }
        public UserControl AnchorBottom { get; set; }
        public UserControl AnchorRight { get; set; }
        public Guid UniqueIdentifier { get; private set; }

        private bool isProcessing;

        public Cohort()
        {
            InitializeComponent();

            imgClose.MouseEnter += new MouseEventHandler(imgClose_MouseEnter);
            imgClose.MouseLeave += new MouseEventHandler(imgClose_MouseLeave);
            imgClose.MouseDown += new MouseButtonEventHandler(imgClose_MouseDown);
            txtPower.TextChanged += new TextChangedEventHandler(txtInput_TextChanged);
            txtRatioControlsExposed.TextChanged += new TextChangedEventHandler(txtInput_TextChanged);
            txtPctControlsExposed.TextChanged += new TextChangedEventHandler(txtInput_TextChanged);
            txtOddsRatio.TextChanged += new TextChangedEventHandler(txtInput_TextChanged);
            txtRiskRatio.TextChanged += new TextChangedEventHandler(txtInput_TextChanged);
            txtPctCasesWithExposure.TextChanged += new TextChangedEventHandler(txtInput_TextChanged);
			//EI-14
            txtPower.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtRatioControlsExposed.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtPctControlsExposed.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtOddsRatio.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtRiskRatio.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtPctCasesWithExposure.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);

            cbxConfidenceLevel.SelectionChanged += new SelectionChangedEventHandler(cbxConfidenceLevel_SelectionChanged);
            mnuPrint.Click += new RoutedEventHandler(mnuPrint_Click);
            mnuSave.Click += new RoutedEventHandler(mnuSave_Click);

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new SimpleDelegate(DoFocus));
        }

        void mnuSave_Click(object sender, RoutedEventArgs e)
        {
            Common.SaveAsImage(pnlMainContent);
        }

        void mnuPrint_Click(object sender, RoutedEventArgs e)
        {
            Common.Print(pnlMainContent);
        }

        private void DoFocus()
        {
            txtPower.Focus();
        }

        private void Calculate()
        {
            System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InvariantCulture;

            string confidenceRaw = ((ComboBoxItem)cbxConfidenceLevel.SelectedItem).Content.ToString().Split('%')[0];
            double confidence = (100.0 - double.Parse(confidenceRaw, ci)) / 100.0;
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

        void txtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            double percentExposed = 0;
            double.TryParse(txtPctControlsExposed.Text, out percentExposed);
            percentExposed = percentExposed / 100.0;

            if ((sender == txtOddsRatio) || (sender == txtPctControlsExposed))
            {
                double oddsRatio = 0;
                double.TryParse(txtOddsRatio.Text, out oddsRatio);
                if (!txtPctCasesWithExposure.IsFocused)
                {
                    txtPctCasesWithExposure.Text = OddsToPercentCases(oddsRatio, percentExposed).ToString("N1");
                }
                if (!txtRiskRatio.IsFocused)
                {
                    txtRiskRatio.Text = OddsToRisk(oddsRatio, percentExposed).ToString();
                }
            }
            else if ((sender == txtRiskRatio))
            {
                double riskRatio = 0;
                double.TryParse(txtRiskRatio.Text, out riskRatio);
                if (!txtOddsRatio.IsFocused)
                {
                    txtOddsRatio.Text = RiskToOdds(riskRatio, percentExposed).ToString();
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

        public void CalcCohort(double a, double b, double vr, double v2, double vor, double v1, double rr, double dd)
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

        /*static void Main(string[] args)
        {
            //95,80,1.0,40,10,86.96
            UnmatchedCaseControl(0.05, 80.0, 1.0, 0.4, 10.0, 0.8696);
            //95,80,1.0,5,24,55.81,11.16,50.81
            Cohort(0.05, 80.0, 1.0, 0.05, 24.0, 0.5581, 11.16, 0.5081);
            OddsToPercentCases(13, 0.37);
            PercentCasesToOdds(0.8842, 0.37);
            SampleSize(999999, 50, 11);
        }*/

        public double OddsToRisk(double oddsRatio, double percentControls)
        {
            double rawVal = 0;
            if (oddsRatio != 0)
            {
                rawVal = percentControls * oddsRatio / (1.0 + percentControls * (oddsRatio - 1.0));
            }
            return Math.Round(100000.0 * rawVal / percentControls) / 100000.0;
        }

        public double RiskToOdds(double riskRatio, double percentControls)
        {
            double rawVal = 0;
            if (riskRatio != 0)
            {
                rawVal = percentControls * riskRatio;
            }
            return Math.Round(100000.0 * (rawVal * (1.0 - percentControls)) / (percentControls * (1.0 - rawVal))) / 100000.0;
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

        #region IGadget Members

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
            string xmlString =
            "<ConfidenceLevel>" + cbxConfidenceLevel.Text + "</ConfidenceLevel>" +
            "<Power>" + txtPower.Text + "</Power>" +
            "<RatioControlsExposed>" + txtRatioControlsExposed.Text + "</RatioControlsExposed>" +
            "<PctControlsExposed>" + txtPctControlsExposed.Text + "</PctControlsExposed>" +
            "<OddsRatio>" + txtOddsRatio.Text + "</OddsRatio>" +
            "<PctCasesWithExposure>" + txtPctCasesWithExposure.Text + "</PctCasesWithExposure>";

            System.Xml.XmlElement element = doc.CreateElement("statcalcCohortGadget");
            element.InnerXml = xmlString;

            System.Xml.XmlAttribute locationY = doc.CreateAttribute("top");
            System.Xml.XmlAttribute locationX = doc.CreateAttribute("left");
            System.Xml.XmlAttribute collapsed = doc.CreateAttribute("collapsed");
            System.Xml.XmlAttribute type = doc.CreateAttribute("gadgetType");

            locationY.Value = Canvas.GetTop(this).ToString("F2");
            locationX.Value = Canvas.GetLeft(this).ToString("F2");
            collapsed.Value = "false"; // currently no way to collapse the gadget, so leave this 'false' for now
            type.Value = "EpiDashboard.StatCalc.Cohort";

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);

            return element;
        }

        public void CreateFromXml(XmlElement element)
        {   
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public string ToHTML(string htmlFileName = "", int count = 0, bool useAlternatingColors = false)
        {
            return string.Empty;

            //if (string.IsNullOrEmpty(txtPower.Text) || string.IsNullOrEmpty(txtRatioControlsExposed.Text) || string.IsNullOrEmpty(txtPctControlsExposed.Text) || string.IsNullOrEmpty(txtOddsRatio.Text) || string.IsNullOrEmpty(txtPctCasesWithExposure.Text))
            //{
            //    return string.Empty;
            //}

            //StringBuilder htmlBuilder = new StringBuilder();
            //Epi.Configuration config = Epi.Configuration.GetNewInstance();

            //htmlBuilder.AppendLine("<h2>StatCalc Sample Size and Power</h2>");

            //htmlBuilder.AppendLine("<p><small>");
            //htmlBuilder.AppendLine("<strong>Unmatched Cohort and Cross-Sectional Studies (Exposed and Nonexposed)</strong>");
            //htmlBuilder.AppendLine("<br />");
            //htmlBuilder.AppendLine("</small></p>");

            //htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
            //htmlBuilder.AppendLine("");
            //htmlBuilder.AppendLine("<table class=\"noborder\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
            //htmlBuilder.AppendLine("<tr>");
            //htmlBuilder.AppendLine("<td class=\"noborder\" style=\"padding-right: 20px;\">");
            //htmlBuilder.AppendLine("");
            //htmlBuilder.AppendLine("");
            //htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
            //htmlBuilder.AppendLine(" <tr class=\"altcolor\">");
            //htmlBuilder.AppendLine("  <td class=\"value\">Two-sided confidence level:</td>");
            //htmlBuilder.AppendLine("  <td>" + cbxConfidenceLevel.Text + "</td>");
            //htmlBuilder.AppendLine(" </tr>");
            //htmlBuilder.AppendLine(" <tr>");
            //htmlBuilder.AppendLine("  <td class=\"value\">Power:</td>");
            //htmlBuilder.AppendLine("  <td>" + txtPower.Text + "</td>");
            //htmlBuilder.AppendLine(" </tr>");
            //htmlBuilder.AppendLine(" <tr class=\"altcolor\">");
            //htmlBuilder.AppendLine("  <td class=\"value\">Ratio of controls to cases:</td>");
            //htmlBuilder.AppendLine("  <td>" + txtRatioControlsExposed.Text + "</td>");
            //htmlBuilder.AppendLine(" </tr>");
            //htmlBuilder.AppendLine(" <tr>");
            //htmlBuilder.AppendLine("  <td class=\"value\">Percent of controls exposed:</td>");
            //htmlBuilder.AppendLine("  <td>" + txtPctControlsExposed.Text + "</td>");
            //htmlBuilder.AppendLine(" </tr>");
            //htmlBuilder.AppendLine(" <tr class=\"altcolor\">");
            //htmlBuilder.AppendLine("  <td class=\"value\">Odds ratio:</td>");
            //htmlBuilder.AppendLine("  <td>" + txtOddsRatio.Text + "</td>");
            //htmlBuilder.AppendLine(" </tr>");
            //htmlBuilder.AppendLine(" <tr>");
            //htmlBuilder.AppendLine("  <td class=\"value\">Percent of cases with exposure:</td>");
            //htmlBuilder.AppendLine("  <td>" + txtPctCasesWithExposure.Text + "</td>");
            //htmlBuilder.AppendLine(" </tr>");
            //htmlBuilder.AppendLine("</table>");
            //htmlBuilder.AppendLine("");
            //htmlBuilder.AppendLine("</td>");
            //htmlBuilder.AppendLine("<td class=\"noborder\">");
            //htmlBuilder.AppendLine("");
            //htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
            //htmlBuilder.AppendLine("  <tr>");
            //htmlBuilder.AppendLine("    <td>&nbsp;</td>");
            //htmlBuilder.AppendLine("    <th>Kelsey</th>");
            //htmlBuilder.AppendLine("    <th>Fleiss</th>");
            //htmlBuilder.AppendLine("    <th>Fleiss<br/>w/CC</th>");
            //htmlBuilder.AppendLine("  </tr>");
            //htmlBuilder.AppendLine("  <tr class=\"altcolor\">");
            //htmlBuilder.AppendLine("    <th>Cases</th>");
            //htmlBuilder.AppendLine("    <td style=\"text-align: center;\">" + txtKelseyCases.Text + "</td>");
            //htmlBuilder.AppendLine("    <td style=\"text-align: center;\">" + txtFleissCases.Text + "</td>");
            //htmlBuilder.AppendLine("    <td style=\"text-align: center;\">" + txtFleissCCCases.Text + "</td>");
            //htmlBuilder.AppendLine("  </tr>");
            //htmlBuilder.AppendLine("  <tr>");
            //htmlBuilder.AppendLine("    <th>Controls</th>");
            //htmlBuilder.AppendLine("    <td style=\"text-align: center;\">" + txtKelseyControls.Text + "</td>");
            //htmlBuilder.AppendLine("    <td style=\"text-align: center;\">" + txtFleissControls.Text + "</td>");
            //htmlBuilder.AppendLine("    <td style=\"text-align: center;\">" + txtFleissCCControls.Text + "</td>");
            //htmlBuilder.AppendLine("  </tr>");
            //htmlBuilder.AppendLine("  <tr class=\"altcolor\">");
            //htmlBuilder.AppendLine("    <th>Total</th>");
            //htmlBuilder.AppendLine("    <td style=\"text-align: center;\">" + txtKelseyTotal.Text + "</td>");
            //htmlBuilder.AppendLine("    <td style=\"text-align: center;\">" + txtFleissTotal.Text + "</td>");
            //htmlBuilder.AppendLine("    <td style=\"text-align: center;\">" + txtFleissCCTotal.Text + "</td>");
            //htmlBuilder.AppendLine("  </tr>");
            //htmlBuilder.AppendLine("</table>");
            //htmlBuilder.AppendLine("</td>");
            //htmlBuilder.AppendLine("</tr>");
            //htmlBuilder.AppendLine("</table>");

            //return htmlBuilder.ToString();
        }

        public string CustomOutputHeading { get { return string.Empty; } set { } }
        public string CustomOutputDescription { get { return string.Empty; } set { } }
        public string CustomOutputCaption { get { return string.Empty; } set { } }

        #endregion


        public void HideCloseIcon()
        {
            imgClose.Visibility = Visibility.Collapsed;
        }

        public int PreferredUIHeight
        {
            get { return 370; }
        }

        public int PreferredUIWidth
        {
            get { return 535; }
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

        public bool DrawBorders { get; set; } 
    }
}
