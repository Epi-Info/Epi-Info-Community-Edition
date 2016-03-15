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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EpiDashboard.Controls
{
    /// <summary>
    /// Interaction logic for StatCalcMatchedCaseControlPanel.xaml
    /// </summary>
    public partial class StatCalcMatchedCaseControlPanel : UserControl, IDisposable
    {
        private bool showRowColumnPercents;
        private Orientation orientation;
        private bool loadingData = false;

        private decimal totalValue;
        private decimal totalYesRow;
        private decimal totalNoRow;
        private decimal totalYesCol;
        private decimal totalNoCol;
        
        public event TwoByTwoValuesUpdatedHandler ValuesUpdated;

        public StatCalcMatchedCaseControlPanel()
        {
            InitializeComponent();
            ShowRowColumnPercents = true;
            Construct();
        }

        public StatCalcMatchedCaseControlPanel(decimal yyVal, decimal ynVal, decimal nyVal, decimal nnVal)
        {
            InitializeComponent();
            ShowRowColumnPercents = true;
            Construct();
            this.YesYesValue = yyVal;
            this.YesNoValue = ynVal;
            this.NoYesValue = nyVal;
            this.NoNoValue = nnVal;
        }

        private void Construct()
        {
            toolTipSwapOutcome.Content = DashboardSharedStrings.TOOLTIP_SWAP_OUTCOME;
            toolTipSwapExposure.Content = DashboardSharedStrings.TOOLTIP_SWAP_EXPOSURE;

            txtYYValue.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtYNValue.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtNYValue.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
            txtNNValue.PreviewKeyDown += new KeyEventHandler(txtInput_PreviewKeyDown);
        }

        //public decimal? OutcomeRateExposure
        //{
        //    get
        //    {
        //        if (totalYesRow != null && totalYesRow > 0 && YesYesValue.HasValue)
        //        {
        //            return YesYesValue.Value / totalYesRow;
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //}

        //public decimal? OutcomeRateNoExposure
        //{
        //    get
        //    {
        //        if (totalNoRow != null && totalNoRow > 0 && NoYesValue.HasValue)
        //        {
        //            return NoYesValue.Value / totalNoRow;
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //}

        //public decimal? OddsRatio
        //{
        //    get
        //    {
        //        decimal result;
        //        bool success = decimal.TryParse(this.panelSingleTableResults.txtOddsRatioEstimate.Text, out result);
        //        if (success)
        //        {
        //            return result;
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //}

        //public decimal? RiskRatio
        //{
        //    get
        //    {
        //        decimal result;
        //        bool success = decimal.TryParse(this.panelSingleTableResults.txtRiskRatioEstimate.Text, out result);
        //        if (success)
        //        {
        //            return result;
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //}

        //public decimal? OddsRatioUpper
        //{
        //    get
        //    {
        //        decimal result;
        //        bool success = decimal.TryParse(this.panelSingleTableResults.txtOddsRatioUpper.Text, out result);
        //        if (success)
        //        {
        //            return result;
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //}

        //public decimal? OddsRatioLower
        //{
        //    get
        //    {
        //        decimal result;
        //        bool success = decimal.TryParse(this.panelSingleTableResults.txtOddsRatioLower.Text, out result);
        //        if (success)
        //        {
        //            return result;
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //}

        public bool ShowRowColumnPercents 
        {
            get
            {
                return this.showRowColumnPercents;
            }
            set
            {
                this.showRowColumnPercents = value;
                if (ShowRowColumnPercents)
                {
                    TurnOnRowColumnPercents();
                }
                else
                {
                    TurnOffRowColumnPercents();
                }
            }
        }

        private void TurnOnRowColumnPercents()
        {
            tblockYYColP.Visibility = System.Windows.Visibility.Visible;
            tblockYNColP.Visibility = System.Windows.Visibility.Visible;
            tblockNYColP.Visibility = System.Windows.Visibility.Visible;
            tblockNNColP.Visibility = System.Windows.Visibility.Visible;

            tblockYYRowP.Visibility = System.Windows.Visibility.Visible;
            tblockYNRowP.Visibility = System.Windows.Visibility.Visible;
            tblockNYRowP.Visibility = System.Windows.Visibility.Visible;
            tblockNNRowP.Visibility = System.Windows.Visibility.Visible;

            tblockYTColP.Visibility = System.Windows.Visibility.Visible;
            tblockNTColP.Visibility = System.Windows.Visibility.Visible;

            tblockYTRowP.Visibility = System.Windows.Visibility.Visible;
            tblockNTRowP.Visibility = System.Windows.Visibility.Visible;

            tblockTotalPercent.Visibility = System.Windows.Visibility.Visible;

            tblockTT.Visibility = System.Windows.Visibility.Visible;
            tblockCC1.Visibility = System.Windows.Visibility.Visible;
            tblockCC2.Visibility = System.Windows.Visibility.Visible;
            tblockRR1.Visibility = System.Windows.Visibility.Visible;
            tblockRR2.Visibility = System.Windows.Visibility.Visible;            

            panel1.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            panel2.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
        }

        private void TurnOffRowColumnPercents()
        {
            tblockYYColP.Visibility = System.Windows.Visibility.Collapsed;
            tblockYNColP.Visibility = System.Windows.Visibility.Collapsed;
            tblockNYColP.Visibility = System.Windows.Visibility.Collapsed;
            tblockNNColP.Visibility = System.Windows.Visibility.Collapsed;

            tblockYYRowP.Visibility = System.Windows.Visibility.Collapsed;
            tblockYNRowP.Visibility = System.Windows.Visibility.Collapsed;
            tblockNYRowP.Visibility = System.Windows.Visibility.Collapsed;
            tblockNNRowP.Visibility = System.Windows.Visibility.Collapsed;

            tblockYTColP.Visibility = System.Windows.Visibility.Collapsed;
            tblockNTColP.Visibility = System.Windows.Visibility.Collapsed;

            tblockYTRowP.Visibility = System.Windows.Visibility.Collapsed;
            tblockNTRowP.Visibility = System.Windows.Visibility.Collapsed;

            tblockTotalPercent.Visibility = System.Windows.Visibility.Collapsed;

            tblockTT.Visibility = System.Windows.Visibility.Collapsed;
            tblockCC1.Visibility = System.Windows.Visibility.Collapsed;
            tblockCC2.Visibility = System.Windows.Visibility.Collapsed;
            tblockRR1.Visibility = System.Windows.Visibility.Collapsed;
            tblockRR2.Visibility = System.Windows.Visibility.Collapsed;

            panel1.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            panel2.VerticalAlignment = System.Windows.VerticalAlignment.Center;
        }

        public decimal? YesYesValue
        {
            get
            {
                decimal yyVal;
                bool success = decimal.TryParse(txtYYValue.Text, out yyVal);
                if (success)
                {
                    return yyVal;
                }
                else
                {
                    return null;
                }                
            }
            set
            {
                txtYYValue.Text = value.ToString();
                Compute();
            }
        }

        public decimal? YesNoValue
        {
            get
            {
                decimal ynVal;
                bool success = decimal.TryParse(txtYNValue.Text, out ynVal);
                if (success)
                {
                    return ynVal;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                txtYNValue.Text = value.ToString();
                Compute();
            }
        }

        public decimal? NoYesValue
        {
            get
            {
                decimal nyVal;
                bool success = decimal.TryParse(txtNYValue.Text, out nyVal);
                if (success)
                {
                    return nyVal;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                txtNYValue.Text = value.ToString();
                Compute();
            }
        }

        public decimal? NoNoValue
        {
            get
            {
                decimal nnVal;
                bool success = decimal.TryParse(txtNNValue.Text, out nnVal);
                if (success)
                {
                    return nnVal;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                txtNNValue.Text = value.ToString();
                Compute();
            }
        }

        public string ExposureYesLabel
        {
            get
            {
                return this.tblockExposureYesLabel.Text;
            }
            set
            {
                this.tblockExposureYesLabel.Text = value;
            }
        }

        public string ExposureNoLabel
        {
            get
            {
                return this.tblockExposureNoLabel.Text;
            }
            set
            {
                this.tblockExposureNoLabel.Text = value;
            }
        }

        public decimal? TotalValue
        {
            get
            {
                return totalValue;
            }
        }

        public decimal? TotalYesRow
        {
            get
            {
                return totalYesRow;
            }
        }

        public decimal? TotalNoRow
        {
            get
            {
                return totalNoRow;
            }
        }

        public decimal? TotalYesCol
        {
            get
            {
                return totalYesCol;
            }
        }

        public decimal? TotalNoCol
        {
            get
            {
                return totalNoCol;
            }
        }

        public string OutcomeYesLabel
        {
            get
            {
                return this.tblockOutcomeYesLabel.Text;
            }
            set
            {
                this.tblockOutcomeYesLabel.Text = value;
            }
        }

        public string OutcomeNoLabel
        {
            get
            {
                return this.tblockOutcomeNoLabel.Text;
            }
            set
            {
                this.tblockOutcomeNoLabel.Text = value;
            }
        }

        public string OutcomeVariable
        {
            get
            {
                return this.tblockOutcome.Text;
            }
            set
            {
                this.tblockOutcome.Text = value;
            }
        }

        public string ExposureVariable
        {
            get
            {
                return this.tblockExposure.Text;
            }
            set
            {
                this.tblockExposure.Text = value;

                //rotate.CenterX = tblockExposure.ActualWidth;
                //rotate.CenterY = tblockExposure.ActualHeight;
                //Canvas.SetBottom(tblockExposure, tblockExposure.
                 //CenterX="{Binding ElementName=tblockExposure, Path=ActualWidth}" CenterY="{Binding ElementName=tblockExposure, Path=ActualHeight}"

                TextBlock tblock = new TextBlock();
                tblock.FontSize = tblockExposure.FontSize;
                tblock.FontWeight = tblockExposure.FontWeight;
                tblock.Text = tblockExposure.Text;

                Typeface typeFace = new Typeface(new FontFamily("Global User Interface"), tblockExposure.FontStyle, tblockExposure.FontWeight, tblockExposure.FontStretch);
                FormattedText ftxt = new FormattedText(tblockExposure.Text, System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, typeFace, tblockExposure.FontSize, Brushes.Black);

                rotate.Angle = 0;
                double actualCanvasHeight = exposureCanvas.Height;
                double actualControlWidth = ftxt.Width;//tblockExposure.ActualWidth;

                if (actualCanvasHeight > actualControlWidth)
                {                    
                    double diff = actualCanvasHeight - actualControlWidth;
                    double bottom = diff / 2;
                    Canvas.SetBottom(tblockExposure, bottom);
                    
                }

                rotate.Angle = 270;
            }
        }

        public void Compute()
        {
            if (loadingData)
            {
                return;
            }

            decimal yyVal;
            decimal ynVal;
            decimal nyVal;
            decimal nnVal;

            bool success1 = decimal.TryParse(txtYYValue.Text, out yyVal);
            bool success2 = decimal.TryParse(txtYNValue.Text, out ynVal);
            bool success3 = decimal.TryParse(txtNYValue.Text, out nyVal);
            bool success4 = decimal.TryParse(txtNNValue.Text, out nnVal);

            if ((success1 && success2 && success3 && success4) && (yyVal >= 0 && ynVal >= 0 && nyVal >= 0 && nnVal >= 0))
            {
                totalYesRow = yyVal + ynVal;
                totalNoRow = nyVal + nnVal;

                totalYesCol = yyVal + nyVal;
                totalNoCol = ynVal + nnVal;

                totalValue = yyVal + ynVal + nyVal + nnVal;

                tblockYTColCount.Text = totalYesCol.ToString();
                tblockNTColCount.Text = totalNoCol.ToString();
                tblockYTRowCount.Text = totalYesRow.ToString();
                tblockNTRowCount.Text = totalNoRow.ToString();

                tblockTotalCount.Text = totalValue.ToString();

                decimal yesRowPercent = totalYesCol / totalValue;
                decimal noRowPercent = totalNoCol / totalValue;

                decimal yesColPercent = totalYesRow / totalValue;
                decimal noColPercent = totalNoRow / totalValue;

                decimal hundred = 1;

                if (totalYesCol == 0)
                {
                    tblockYYColP.Text = Epi.SharedStrings.UNDEFINED;
                    tblockNYColP.Text = Epi.SharedStrings.UNDEFINED;
                }
                else
                {
                    tblockYYColP.Text = (yyVal / totalYesCol).ToString("P");
                    tblockNYColP.Text = (nyVal / totalYesCol).ToString("P");
                }

                if (totalNoCol == 0)
                {
                    tblockYNColP.Text = Epi.SharedStrings.UNDEFINED;
                    tblockNNColP.Text = Epi.SharedStrings.UNDEFINED;
                }
                else
                {
                    tblockYNColP.Text = (ynVal / totalNoCol).ToString("P");
                    tblockNNColP.Text = (nnVal / totalNoCol).ToString("P");
                }


                if (totalYesRow == 0)
                {
                    tblockYYRowP.Text = Epi.SharedStrings.UNDEFINED;
                    tblockYNRowP.Text = Epi.SharedStrings.UNDEFINED;
                }
                else
                {
                    tblockYYRowP.Text = (yyVal / totalYesRow).ToString("P");
                    tblockYNRowP.Text = (ynVal / totalYesRow).ToString("P");
                }
                
                if (totalNoRow == 0)
                {
                    tblockNYRowP.Text = Epi.SharedStrings.UNDEFINED;
                    tblockNNRowP.Text = Epi.SharedStrings.UNDEFINED;
                }
                else
                {
                    tblockNYRowP.Text = (nyVal / totalNoRow).ToString("P");
                    tblockNNRowP.Text = (nnVal / totalNoRow).ToString("P");
                }

                //tblockYYRowP.Text = (yyVal / totalYesRow).ToString("P");
                //tblockYNRowP.Text = (ynVal / totalYesRow).ToString("P");
                //tblockNYRowP.Text = (nyVal / totalNoRow).ToString("P");
                //tblockNNRowP.Text = (nnVal / totalNoRow).ToString("P");

                tblockYTColP.Text = yesColPercent.ToString("P");
                tblockNTColP.Text = noColPercent.ToString("P");

                tblockYTRowP.Text = yesRowPercent.ToString("P");
                tblockNTRowP.Text = noRowPercent.ToString("P");

                tblockTotalPercent.Text = hundred.ToString("P");

                DoubleAnimation daRed = new DoubleAnimation();
                DoubleAnimation daYellow = new DoubleAnimation();
                DoubleAnimation daOrange = new DoubleAnimation();
                DoubleAnimation daGreen = new DoubleAnimation();

                daRed.From = 1;
                daYellow.From = 1;
                daOrange.From = 1;
                daGreen.From = 1;

                if (totalValue > 0)
                {
                    daRed.To = 75.0 * (double)yyVal / (double)totalValue;
                    daOrange.To = 75.0 * (double)ynVal / (double)totalValue;
                    daYellow.To = 75.0 * (double)nyVal / (double)totalValue;
                    daGreen.To = 75.0 * (double)nnVal / (double)totalValue;

                    daRed.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                    rctRed.BeginAnimation(Rectangle.HeightProperty, daRed);
                    rctRed.BeginAnimation(Rectangle.WidthProperty, daRed);

                    daYellow.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                    rctYellow.BeginAnimation(Rectangle.HeightProperty, daYellow);
                    rctYellow.BeginAnimation(Rectangle.WidthProperty, daYellow);

                    daOrange.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                    rctOrange.BeginAnimation(Rectangle.HeightProperty, daOrange);
                    rctOrange.BeginAnimation(Rectangle.WidthProperty, daOrange);

                    daGreen.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                    rctGreen.BeginAnimation(Rectangle.HeightProperty, daGreen);
                    rctGreen.BeginAnimation(Rectangle.WidthProperty, daGreen);

                    grdGraph.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    rctRed.Width = 0;
                    rctOrange.Width = 0;
                    rctYellow.Width = 0;
                    rctGreen.Width = 0;

                    rctRed.Height = 0;
                    rctOrange.Height = 0;
                    rctYellow.Height = 0;
                    rctGreen.Height = 0;
                }

                panelSingleTableResults.YesYesValue = this.YesYesValue;
                panelSingleTableResults.YesNoValue = this.YesNoValue;
                panelSingleTableResults.NoYesValue = this.NoYesValue;
                panelSingleTableResults.NoNoValue = this.NoNoValue;
                panelSingleTableResults.tooManyCases = 0;
                panelSingleTableResults.tooFewCases = 0;
                panelSingleTableResults.tooManyControls = 0;
                panelSingleTableResults.tooFewControls = 0;
            }
            else
            {
                tblockYTColCount.Text = "n/a";
                tblockNTColCount.Text = "n/a";
                tblockYTRowCount.Text = "n/a";
                tblockNTRowCount.Text = "n/a";
                tblockTotalCount.Text = "n/a";

                tblockYYColP.Text = "n/a";
                tblockYNColP.Text = "n/a";
                tblockNYColP.Text = "n/a";
                tblockNNColP.Text = "n/a";

                tblockYYRowP.Text = "n/a";
                tblockYNRowP.Text = "n/a";
                tblockNYRowP.Text = "n/a";
                tblockNNRowP.Text = "n/a";

                tblockYTColP.Text = "n/a";
                tblockNTColP.Text = "n/a";

                tblockYTRowP.Text = "n/a";
                tblockNTRowP.Text = "n/a";

                tblockTotalPercent.Text = "n/a";

                grdGraph.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        void txtInput_PreviewKeyDown(object sender, KeyEventArgs e)
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

        private void txtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            Compute();
        }

        private void PathShowPercents_MouseEnter(object sender, MouseEventArgs e)
        {
            showPercentsRectangle.Style = this.Resources["percentRectangleHover"] as Style;
            this.Cursor = Cursors.Hand;
        }

        private void PathShowPercents_MouseLeave(object sender, MouseEventArgs e)
        {
            showPercentsRectangle.Style = this.Resources["percentRectangle"] as Style;
            this.Cursor = Cursors.Arrow;
        }

        private void PathShowPercents_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.ShowRowColumnPercents = !this.ShowRowColumnPercents;
        }

        private void PathSwapOutcome_MouseEnter(object sender, MouseEventArgs e)
        {
            swapOutcomeRectangle.Style = this.Resources["swapValuesRectangleHover"] as Style;
            this.Cursor = Cursors.Hand;
        }

        private void PathSwapOutcome_MouseLeave(object sender, MouseEventArgs e)
        {
            swapOutcomeRectangle.Style = this.Resources["swapValuesRectangle"] as Style;
            this.Cursor = Cursors.Arrow;
        }

        private void PathSwapOutcome_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            loadingData = true;
            
            decimal temp = YesYesValue.Value;
            YesYesValue = YesNoValue;
            YesNoValue = temp;

            temp = NoYesValue.Value;
            NoYesValue = NoNoValue;
            NoNoValue = temp;

            string tempStr = tblockOutcomeYesLabel.Text;
            tblockOutcomeYesLabel.Text = tblockOutcomeNoLabel.Text;
            tblockOutcomeNoLabel.Text = tempStr;

            loadingData = false;

            Compute();

            if (ValuesUpdated != null)
            {
                ValuesUpdated(this);
            }
        }

        private void PathSwapExposure_MouseEnter(object sender, MouseEventArgs e)
        {
            swapExposureRectangle.Style = this.Resources["swapValuesRectangleHover"] as Style;
            this.Cursor = Cursors.Hand;
        }

        private void PathSwapExposure_MouseLeave(object sender, MouseEventArgs e)
        {
            swapExposureRectangle.Style = this.Resources["swapValuesRectangle"] as Style;
            this.Cursor = Cursors.Arrow;
        }

        private void PathSwapExposure_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            loadingData = true;

            decimal temp = YesYesValue.Value;
            YesYesValue = NoYesValue;
            NoYesValue = temp;

            temp = YesNoValue.Value;
            YesNoValue = NoNoValue;
            NoNoValue = temp;

            string tempStr = tblockExposureYesLabel.Text;
            tblockExposureYesLabel.Text = tblockExposureNoLabel.Text;
            tblockExposureNoLabel.Text = tempStr;

            loadingData = false;

            Compute();

            if (ValuesUpdated != null)
            {
                ValuesUpdated(this);
            }
        }

        public Orientation Orientation
        {
            get
            {
                return this.orientation;                
            }
            set
            {
                this.orientation = value;

                if (this.Orientation == System.Windows.Controls.Orientation.Vertical)
                {
                    Grid.SetRow(panelSingleTableResults, 0);
                    Grid.SetColumn(panelSingleTableResults, 7);
                    Grid.SetRowSpan(panelSingleTableResults, 3);
                    Grid.SetColumnSpan(panelSingleTableResults, 1);
                    panelSingleTableResults.Margin = new Thickness(0, 0, 0, 0);
                }
                else if (this.Orientation == System.Windows.Controls.Orientation.Horizontal)
                {
                    Grid.SetRow(panelSingleTableResults, 4);
                    Grid.SetColumn(panelSingleTableResults, 0);
                    Grid.SetRowSpan(panelSingleTableResults, 1);
                    Grid.SetColumnSpan(panelSingleTableResults, 6);
                    panelSingleTableResults.Margin = new Thickness(0, 10, 0, 0);
                }
            }
        }

        public string ToHTML(string htmlFileName = "", int count = 0)
        {
            StringBuilder htmlBuilder = new StringBuilder();

            htmlBuilder.AppendLine("<table align=\"left\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
            //htmlBuilder.AppendLine("<caption>" + gridName + "</caption>");

            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <td></td>");
            htmlBuilder.AppendLine("  <th colspan=\"2\">" + tblockOutcome.Text + "</th>");
            htmlBuilder.AppendLine("  <td></td>");
            htmlBuilder.AppendLine(" </tr>");

            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <th>" + tblockExposure.Text + "</th>");
            htmlBuilder.AppendLine("  <th>" + tblockOutcomeYesLabel.Text + "</th>");
            htmlBuilder.AppendLine("  <th>" + tblockOutcomeNoLabel.Text + "</th>");
            htmlBuilder.AppendLine("  <th>Total</th>");
            htmlBuilder.AppendLine(" </tr>");

            if (ShowRowColumnPercents == false)
            {
                htmlBuilder.AppendLine(" <tr>");
                htmlBuilder.AppendLine("  <th>" + tblockExposureYesLabel.Text + "</th>");
                htmlBuilder.AppendLine("  <td>" + txtYYValue.Text + "<br/><br/></td>");
                htmlBuilder.AppendLine("  <td>" + txtYNValue.Text + "<br/><br/></td>");
                htmlBuilder.AppendLine("  <td>" + tblockYTRowCount.Text + "<br/><br/></td>");
                htmlBuilder.AppendLine(" </tr>");

                htmlBuilder.AppendLine(" <tr>");
                htmlBuilder.AppendLine("  <th>" + tblockExposureNoLabel.Text + "</th>");
                htmlBuilder.AppendLine("  <td>" + txtNYValue.Text + "<br/><br/></td>");
                htmlBuilder.AppendLine("  <td>" + txtNNValue.Text + "<br/><br/></td>");
                htmlBuilder.AppendLine("  <td>" + tblockNTRowCount.Text + "<br/><br/></td>");
                htmlBuilder.AppendLine(" </tr>");

                htmlBuilder.AppendLine(" <tr>");
                htmlBuilder.AppendLine("  <th>Total</th>");
                htmlBuilder.AppendLine("  <td>" + tblockYTColCount.Text + "<br/><br/></td>");
                htmlBuilder.AppendLine("  <td>" + tblockNTColCount.Text + "<br/><br/></td>");
                htmlBuilder.AppendLine("  <td>" + tblockTotalCount.Text + "<br/><br/></td>");
                htmlBuilder.AppendLine(" </tr>");
            }
            else
            {
                htmlBuilder.AppendLine(" <tr>");
                htmlBuilder.AppendLine("  <th>" + tblockExposureYesLabel.Text + "<br/><small>Row %<br/>Col %</small></th>");
                htmlBuilder.AppendLine("  <td>" + txtYYValue.Text + "<br/><small>" + tblockYYRowP.Text + "<br/>" + tblockYYColP.Text + "</small></td>");
                htmlBuilder.AppendLine("  <td>" + txtYNValue.Text + "<br/><small>" + tblockYNRowP.Text + "<br/>" + tblockYNColP.Text + "</small></td>");
                htmlBuilder.AppendLine("  <td>" + tblockYTRowCount.Text + "<br/><small>" + 1.ToString("P") + "<br/>" + tblockYTColP.Text + "</small></td>");
                htmlBuilder.AppendLine(" </tr>");

                htmlBuilder.AppendLine(" <tr>");
                htmlBuilder.AppendLine("  <th>" + tblockExposureNoLabel.Text + "<br/><small>Row %<br/>Col %</small></th>");
                htmlBuilder.AppendLine("  <td>" + txtNYValue.Text + "<br/><small>" + tblockNYRowP.Text + "<br/>" + tblockNYColP.Text + "</small></td>");
                htmlBuilder.AppendLine("  <td>" + txtNNValue.Text + "<br/><small>" + tblockNNRowP.Text + "<br/>" + tblockNNColP.Text + "</small></td>");
                htmlBuilder.AppendLine("  <td>" + tblockNTRowCount.Text + "<br/><small>" + 1.ToString("P") + "<br/>" + tblockNTColP.Text + "</small></td>");
                htmlBuilder.AppendLine(" </tr>");

                htmlBuilder.AppendLine(" <tr>");
                htmlBuilder.AppendLine("  <th>Total<br/><small>Row %<br/>Col %</small></th>");
                htmlBuilder.AppendLine("  <td>" + tblockYTColCount.Text + "<br/><small>" + tblockYTRowP.Text + "<br/>" + 1.ToString("P") + "</small></td>");
                htmlBuilder.AppendLine("  <td>" + tblockNTColCount.Text + "<br/><small>" + tblockNTRowP.Text + "<br/>" + 1.ToString("P") + "</small></td>");
                htmlBuilder.AppendLine("  <td>" + tblockTotalCount.Text + "<br/><small>" + 1.ToString("P") + "<br/>" + 1.ToString("P") + "</small></td>");
                htmlBuilder.AppendLine(" </tr>");
            }

            htmlBuilder.AppendLine("</table>");

            double Total = (double)(YesYesValue.Value + YesNoValue.Value + NoYesValue.Value + NoNoValue.Value);

            if (Total > 0)
            {
                int redValue = (int)(((double)YesYesValue.Value / Total * 200));
                int orangeValue = (int)(((double)YesNoValue.Value / Total * 200));
                int yellowValue = (int)(((double)NoYesValue.Value / Total * 200));
                int greenValue = (int)(((double)NoNoValue.Value / Total * 200));

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
            }

            htmlBuilder.AppendLine("<br style=\"clear: all\">");
            htmlBuilder.AppendLine("<br clear=\"all\" />");

            htmlBuilder.AppendLine(this.panelSingleTableResults.ToHTML());

            return htmlBuilder.ToString();
        }

        public void Dispose()
        {            
            ValuesUpdated = null;
        }

        
    }
}
