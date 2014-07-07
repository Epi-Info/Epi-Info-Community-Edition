using System;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Windows.Controls;
using Epi;
using Epi.Data;
using Epi.Fields;
using EpiDashboard.Rules;

namespace EpiDashboard
{
    /// <summary>
    /// Interaction logic for TwoByTwoTableControl.xaml
    /// </summary>
    public partial class TwoByTwoTableControl : UserControl, IGadget
    {
        private DashboardHelper dashboardHelper;
        private bool loadingCombos;
        private bool triangleCollapsed;
        private Dictionary<string, GridCells> currentData;
        private GridCells currentSingleTableData;
        private bool isProcessing;
        private bool advTriangleCollapsed;
        private GadgetParameters gadgetOptions;        
        private BackgroundWorker baseWorker;
        private BackgroundWorker worker;
        private bool is2x2 = false;
        private bool isGrouped = false;
        private static object syncLock = new object();

        public UserControl AnchorLeft { get; set; }
        public UserControl AnchorTop { get; set; }
        public UserControl AnchorBottom { get; set; }
        public UserControl AnchorRight { get; set; }
        public Guid UniqueIdentifier { get; private set; }

        public event GadgetRefreshedHandler GadgetRefreshed;
        public event GadgetClosingHandler GadgetClosing;
        public event GadgetProcessingFinishedHandler GadgetProcessingFinished;
        public event GadgetStatusUpdateHandler GadgetStatusUpdate;
        public event GadgetCheckForCancellationHandler GadgetCheckForCancellation;
        public event GadgetRepositionEventHandler GadgetReposition;
        public event GadgetEventHandler GadgetDragStart;
        public event GadgetEventHandler GadgetDragStop;
        public event GadgetEventHandler GadgetDrag;
        public event GadgetAnchorSetEventHandler GadgetAnchorSetFromXml;

        private struct GridCells
        {
            public int ytVal;
            public int yyVal;
            public int ynVal;
            public int ntVal;
            public int nyVal;
            public int nnVal;
            public int ttVal;
            public int tyVal;
            public int tnVal;
            public double yyRowPct;
            public double ynRowPct;
            public double nyRowPct;
            public double nnRowPct;
            public double tyRowPct;
            public double tnRowPct;
            public double yyColPct;
            public double nyColPct;
            public double ynColPct;
            public double nnColPct;
            public double ytColPct;
            public double ntColPct;
            public StatisticsRepository.cTable.SingleTableResults singleTableResults;
        }

        private struct TextBlockConfig
        {
            public string Text;
            public Thickness Margin;
            public VerticalAlignment VerticalAlignment;
            public HorizontalAlignment HorizontalAlignment;
            public int ColumnNumber;
            public int RowNumber;

            public TextBlockConfig(string text, Thickness margin, VerticalAlignment verticalAlignment, HorizontalAlignment horizontalAlignment, int rowNumber, int columnNumber)
            {
                this.Text = text;
                this.Margin = margin;
                this.VerticalAlignment = verticalAlignment;
                this.HorizontalAlignment = horizontalAlignment;
                this.RowNumber = rowNumber;
                this.ColumnNumber = columnNumber;
            }
        }

        private delegate void SetGridTextDelegate(string strataValue, TextBlockConfig textBlockConfig, FontWeight fontWeight);
        private delegate void AddFreqGridDelegate(string strataVar, string value, int columnCount);
        private delegate void RenderFrequencyHeaderDelegate(string strataValue, string freqVar, DataColumnCollection columns);
        private delegate void SetGridBarDelegate(string strataValue, int rowNumber, double pct);
        private delegate void AddGridRowDelegate(string strataValue, int height);
        private delegate void AddGridFooterDelegate(string strataValue, int rowNumber, int[] totalRows);
        private delegate void DrawFrequencyBordersDelegate(string strataValue);

        private delegate void SetOutcomeHeadingsDelegate(string yesValue, string noValue);
        private delegate void SetExposureHeadingsDelegate(string yesValue, string noValue);

        private delegate void RenderGridDelegate(GridCells gridCells, DataTable twoByTwoTable);
        private delegate void RenderMultiGridDelegate(Dictionary<string,GridCells> gridCellCollection);
        private delegate void ShowErrorMessage(string errorMessage);
        private delegate void SimpleCallback();

        private delegate void SetStatusDelegate(string statusMessage);
        private delegate void RequestUpdateStatusDelegate(string statusMessage);
        private delegate bool CheckForCancellationDelegate();

        private delegate void RenderFinishWithErrorDelegate(string errorMessage);
        private delegate void RenderFinishWithWarningDelegate(string errorMessage);

        public TwoByTwoTableControl()
        {
            InitializeComponent();
            Construct();
        }

        public TwoByTwoTableControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.dashboardHelper = dashboardHelper;
            Construct();
        }

        private void Construct()
        {
            FillComboboxes();

            dgResults.CanUserAddRows = false;
            dgResults.CanUserDeleteRows = false;
            dgResults.IsReadOnly = true;
            dgResults.SelectedCellsChanged += new Microsoft.Windows.Controls.SelectedCellsChangedEventHandler(dgResults_SelectedCellsChanged);
            cbxExposureField.SelectionChanged += new SelectionChangedEventHandler(Field_SelectionChanged);
            cbxOutcomeField.SelectionChanged += new SelectionChangedEventHandler(Field_SelectionChanged);

            checkboxShowPercents.Checked += new RoutedEventHandler(checkboxShowPercents_CheckChanged);
            checkboxShowPercents.Unchecked += new RoutedEventHandler(checkboxShowPercents_CheckChanged);

            checkboxSmartTable.Checked += new RoutedEventHandler(checkboxCheckChanged);
            checkboxSmartTable.Unchecked += new RoutedEventHandler(checkboxCheckChanged);

            ConfigCollapsedTriangle.MouseLeftButtonUp += new MouseButtonEventHandler(Triangle_MouseLeftButtonUp);
            ConfigExpandedTriangle.MouseLeftButtonUp += new MouseButtonEventHandler(Triangle_MouseLeftButtonUp);
            grdAdvancedOptionsHeading.MouseLeftButtonUp += new MouseButtonEventHandler(AdvTriangle_MouseLeftButtonUp);

            mnuSave.Click += new RoutedEventHandler(mnuSave_Click);
            mnuCopyImage.Click += new RoutedEventHandler(mnuCopyImage_Click);
            mnuCopyData.Click += new RoutedEventHandler(mnuCopyData_Click);
            mnuSendDataToHTML.Click += new RoutedEventHandler(mnuSendDataToHTML_Click);
            mnuPrint.Click += new RoutedEventHandler(mnuPrint_Click);

            mnuSendToBack.Click += new RoutedEventHandler(mnuSendToBack_Click);
            mnuRefresh.Click += new RoutedEventHandler(mnuRefresh_Click);
            mnuClose.Click += new RoutedEventHandler(mnuClose_Click);

            txtExposure.RenderTransform = new RotateTransform(270);
            imgClose.MouseEnter += new MouseEventHandler(imgClose_MouseEnter);
            imgClose.MouseLeave += new MouseEventHandler(imgClose_MouseLeave);
            imgClose.MouseDown += new MouseButtonEventHandler(imgClose_MouseDown);            

            this.advTriangleCollapsed = true;

            this.IsProcessing = false;
            this.txtStatus.Text = string.Empty;

            this.GadgetStatusUpdate += new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(IsCancelled);

            gadgetOptions = new GadgetParameters();
            gadgetOptions.ShouldIncludeFullSummaryStatistics = false;
            gadgetOptions.ShouldIncludeMissing = false;
            gadgetOptions.ShouldSortHighToLow = false;
            gadgetOptions.ShouldUseAllPossibleValues = false;
            gadgetOptions.StrataVariableNames = new List<string>();
        }

        private bool Is2x2
        {
            get
            {
                return is2x2;
            }
            set
            {
                is2x2 = value;
            }
        }

        /// <summary>
        /// Handles the check / unchecked events
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void checkboxShowPercents_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (checkboxShowPercents.IsChecked == true)
            {
                TurnOnPercents();
            }
            else
            {
                TurnOffPercents();
            }
        }

        void mnuPrint_Click(object sender, RoutedEventArgs e)
        {
            Common.Print(pnlMainContent);
        }

        void imgClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseGadget();   
        }

        void imgClose_MouseLeave(object sender, MouseEventArgs e)
        {
            Uri uriSource = new Uri("Images/x.png", UriKind.Relative);
            imgClose.Source = new BitmapImage(uriSource);
        }

        void imgClose_MouseEnter(object sender, MouseEventArgs e)
        {
            Uri uriSource = new Uri("Images/x_over.png", UriKind.Relative);
            imgClose.Source = new BitmapImage(uriSource);
        }

        void mnuSave_Click(object sender, RoutedEventArgs e)
        {
            Common.SaveAsImage(pnlMainContent);
        }

        /// <summary>
        /// Handles the click event for the 'Copy data to clipboard' context menu option
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuCopyData_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard();
        }

        void mnuCopyImage_Click(object sender, RoutedEventArgs e)
        {
            if (pnlMainContent.Children.Count > 0)
            {                
                BitmapSource img = (BitmapSource)Common.ToImageSource(pnlMainContent);
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(img));
                Clipboard.SetImage(encoder.Frames[0]);
            }
        }

        void mnuSendToBack_Click(object sender, RoutedEventArgs e)
        {
            SendToBack();
        }

        void mnuRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshResults();
        }

        void mnuClose_Click(object sender, RoutedEventArgs e)
        {
            CloseGadget();
        }

        public ImageSource ToImageSource(FrameworkElement obj)
        {
            // Save current canvas transform
            Transform transform = obj.LayoutTransform;

            // fix margin offset as well
            Thickness margin = obj.Margin;
            obj.Margin = new Thickness(0, 0,
                 margin.Right - margin.Left, margin.Bottom - margin.Top);

            // Get the size of canvas
            Size size = new Size(obj.ActualWidth, obj.ActualHeight);

            // force control to Update
            obj.Measure(size);
            obj.Arrange(new Rect(size));

            RenderTargetBitmap bmp = new RenderTargetBitmap(
                (int)obj.ActualWidth, (int)obj.ActualHeight, 96, 96, PixelFormats.Pbgra32);

            bmp.Render(obj);

            // return values as they were before
            obj.LayoutTransform = transform;
            obj.Margin = margin;
            return bmp;
        }

        void dgResults_SelectedCellsChanged(object sender, Microsoft.Windows.Controls.SelectedCellsChangedEventArgs e)
        {
            //if (!this.IsProcessing)
            //{                
                if (e.AddedCells.Count > 0)
                {
                    if (e.AddedCells[0].Item is DataRowView)
                    {
                        try
                        {
                            RenderDetails((DataRowView)e.AddedCells[0].Item);
                        }
                        catch (Exception)
                        {
                            // do nothing
                        }
                    }
                }
            //}
        }

        private void RenderDetails(DataRowView data)
        {
            if (currentData.ContainsKey(data.Row[0].ToString()))
            {
                GridCells gridCells = (GridCells)currentData[data.Row[0].ToString()];

                currentSingleTableData = gridCells;

                txtExposure.Visibility = Visibility.Visible;
                txtExposure.Text = data.Row[0].ToString();
                txtExposure.Margin = new Thickness(0, (txtExposure.Text.Length * 5) - 20, 0, 0);

                ResetGrid();
                grdGraph.Visibility = Visibility.Visible;
                grdTables.Visibility = Visibility.Visible;
                pnlContent.Visibility = Visibility.Visible;

                txtYesTotalVal.Text = gridCells.ytVal.ToString();
                txtYesYesVal.Text = gridCells.yyVal.ToString();
                txtYesNoVal.Text = gridCells.ynVal.ToString();
                txtNoYesVal.Text = gridCells.nyVal.ToString();
                txtNoNoVal.Text = gridCells.nnVal.ToString();
                txtNoTotalVal.Text = gridCells.ntVal.ToString();
                txtYesYesRow.Text = gridCells.yyRowPct.ToString("P");
                txtYesNoRow.Text = gridCells.ynRowPct.ToString("P");
                txtNoYesRow.Text = gridCells.nyRowPct.ToString("P");
                txtNoNoRow.Text = gridCells.nnRowPct.ToString("P");
                txtTotalTotalVal.Text = gridCells.ttVal.ToString();
                txtTotalYesVal.Text = gridCells.tyVal.ToString();
                txtYesYesCol.Text = gridCells.yyColPct.ToString("P");
                txtNoYesCol.Text = gridCells.nyColPct.ToString("P");
                txtTotalNoVal.Text = gridCells.tnVal.ToString();
                txtYesNoCol.Text = gridCells.ynColPct.ToString("P");
                txtNoNoCol.Text = gridCells.nnColPct.ToString("P");
                txtTotalYesRow.Text = gridCells.tyRowPct.ToString("P");
                txtTotalNoRow.Text = gridCells.tnRowPct.ToString("P");
                txtYesTotalCol.Text = gridCells.ytColPct.ToString("P");
                txtNoTotalCol.Text = gridCells.ntColPct.ToString("P");

                if (gridCells.tyVal > 0 && gridCells.tnVal > 0 && gridCells.ytVal > 0 && gridCells.ntVal > 0)
                {
                    pnlAdvanced.Visibility = Visibility.Visible;
                    grdTables.Visibility = Visibility.Visible;
                    dgResults.Visibility = Visibility.Visible;

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

                    if (gridCells.singleTableResults.FisherExactP != -1)
                    {
                        fisherExact = ((double)gridCells.singleTableResults.FisherExactP).ToString("F10");
                    }

                    if (gridCells.singleTableResults.FisherExact2P != -1)
                    {
                        fisherExact2P = ((double)gridCells.singleTableResults.FisherExact2P).ToString("F10");
                    }

                    if (gridCells.singleTableResults.OddsRatioMLEFisherLower != -1)
                    {
                        fisherLower = ((double)gridCells.singleTableResults.OddsRatioMLEFisherLower).ToString("F4");
                    }

                    if (gridCells.singleTableResults.OddsRatioMLEFisherUpper != -1)
                    {
                        fisherUpper = ((double)gridCells.singleTableResults.OddsRatioMLEFisherUpper).ToString("F4");
                    }

                    if (gridCells.singleTableResults.OddsRatioMLEEstimate != -1)
                    {
                        oddsRatioMLEEstimate = ((double)gridCells.singleTableResults.OddsRatioMLEEstimate).ToString("F4");
                    }

                    if (gridCells.singleTableResults.OddsRatioMLEMidPLower != -1)
                    {
                        oddsRatioMLEMidPLower = ((double)gridCells.singleTableResults.OddsRatioMLEMidPLower).ToString("F4");
                    }

                    if (gridCells.singleTableResults.OddsRatioMLEMidPUpper != -1)
                    {
                        oddsRatioMLEMidPUpper = ((double)gridCells.singleTableResults.OddsRatioMLEMidPUpper).ToString("F4");
                    }

                    if (gridCells.singleTableResults.OddsRatioEstimate != null)
                    {
                        oddsRatioEstimate = ((double)gridCells.singleTableResults.OddsRatioEstimate).ToString("F4");
                    }

                    if (gridCells.singleTableResults.OddsRatioLower != null)
                    {
                        oddsRatioLower = ((double)gridCells.singleTableResults.OddsRatioLower).ToString("F4");
                    }

                    if (gridCells.singleTableResults.OddsRatioUpper != null)
                    {
                        oddsRatioUpper = ((double)gridCells.singleTableResults.OddsRatioUpper).ToString("F4");
                    }

                    if (gridCells.singleTableResults.RiskRatioEstimate != null)
                    {
                        riskRatioEstimate = ((double)gridCells.singleTableResults.RiskRatioEstimate).ToString("F4");
                    }

                    if (gridCells.singleTableResults.RiskRatioLower != null)
                    {
                        riskRatioLower = ((double)gridCells.singleTableResults.RiskRatioLower).ToString("F4");
                    }

                    if (gridCells.singleTableResults.RiskRatioUpper != null)
                    {
                        riskRatioUpper = ((double)gridCells.singleTableResults.RiskRatioUpper).ToString("F4");
                    }


                    txtChiSqCorP.Text = gridCells.singleTableResults.ChiSquareYates2P.ToString();
                    txtChiSqCorVal.Text = gridCells.singleTableResults.ChiSquareYatesVal.ToString("F4");
                    txtChiSqManP.Text = gridCells.singleTableResults.ChiSquareMantel2P.ToString();
                    txtChiSqManVal.Text = gridCells.singleTableResults.ChiSquareMantelVal.ToString("F4");
                    txtChiSqUncP.Text = gridCells.singleTableResults.ChiSquareUncorrected2P.ToString();
                    txtChiSqUncVal.Text = gridCells.singleTableResults.ChiSquareUncorrectedVal.ToString("F4");
                    txtOddsRatioEstimate.Text = oddsRatioEstimate;
                    txtOddsRatioLower.Text = oddsRatioLower;
                    txtOddsRatioUpper.Text = oddsRatioUpper;
                    txtMidPEstimate.Text = oddsRatioMLEEstimate;
                    txtMidPLower.Text = oddsRatioMLEMidPLower;
                    txtMidPUpper.Text = oddsRatioMLEMidPUpper;
                    txtRiskDifferenceEstimate.Text = gridCells.singleTableResults.RiskDifferenceEstimate.ToString("F4");
                    txtRiskDifferenceLower.Text = gridCells.singleTableResults.RiskDifferenceLower.ToString("F4");
                    txtRiskDifferenceUpper.Text = gridCells.singleTableResults.RiskDifferenceUpper.ToString("F4");
                    txtRiskRatioEstimate.Text = riskRatioEstimate; // gridCells.singleTableResults.RiskRatioEstimate.ToString("F4");
                    txtRiskRatioLower.Text = riskRatioLower;
                    txtRiskRatioUpper.Text = riskRatioUpper;
                    txtFisherLower.Text = fisherLower;
                    txtFisherUpper.Text = fisherUpper;
                    txtFisherExact.Text = fisherExact;
                    txtFisherExact2P.Text = fisherExact2P;
                    txtMidPExact.Text = gridCells.singleTableResults.MidP.ToString("F10");
                }
                else
                {
                    pnlAdvanced.Visibility = Visibility.Collapsed;
                }
                rctRed.Height = 1;
                rctRed.Width = 1;
                rctYellow.Height = 1;
                rctYellow.Width = 1;
                rctOrange.Height = 1;
                rctOrange.Width = 1;
                rctGreen.Height = 1;
                rctGreen.Width = 1;

                DoubleAnimation daRed = new DoubleAnimation();
                DoubleAnimation daYellow = new DoubleAnimation();
                DoubleAnimation daOrange = new DoubleAnimation();
                DoubleAnimation daGreen = new DoubleAnimation();

                daRed.From = 1;
                daYellow.From = 1;
                daOrange.From = 1;
                daGreen.From = 1;

                if (gridCells.ttVal != 0)
                {
                    daRed.To = 149.0 * gridCells.yyVal / gridCells.ttVal;
                    daYellow.To = 149.0 * gridCells.nyVal / gridCells.ttVal;
                    daOrange.To = 149.0 * gridCells.ynVal / gridCells.ttVal;
                    daGreen.To = 149.0 * gridCells.nnVal / gridCells.ttVal;
                }
                else
                {
                    daRed.To = 0;
                    daYellow.To = 0;
                    daOrange.To = 0;
                    daGreen.To = 0;
                }
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
            }
        }

        private void CheckAndSetPosition()
        {
            double top = Canvas.GetTop(this);
            double left = Canvas.GetLeft(this);

            if (top < 0)
            {
                Canvas.SetTop(this, 0);
            }
            if (left < 0)
            {
                Canvas.SetLeft(this, 0);
            }
        }

        /// <summary>
        /// Sends the gadget to the back of the canvas
        /// </summary>
        private void SendToBack()
        {
            Canvas.SetZIndex(this, -1);
        }

        private void RenderFinish()
        {
            waitCursor.Visibility = Visibility.Collapsed;
            pnlStatus.Visibility = System.Windows.Visibility.Hidden;
            txtStatus.Text = string.Empty;

            if (Is2x2)
            {
                pnlMainContent.Visibility = System.Windows.Visibility.Visible;
                pnlCrosstabContent.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                pnlMainContent.Visibility = System.Windows.Visibility.Collapsed;
                pnlCrosstabContent.Visibility = System.Windows.Visibility.Visible;
            }

            //if (isGrouped)
            //{
                
            //}

            CheckAndSetPosition();
        }

        private void ShowGrid()
        {
            dgResults.UnselectAll();
            dgResults.Visibility = System.Windows.Visibility.Visible;
            dgResults.IsEnabled = dgResults.IsEnabled;
            if (dgResults != null && dgResults.Items.Count > 0)
            {
                dgResults.SelectionMode = Microsoft.Windows.Controls.DataGridSelectionMode.Single;
                dgResults.SelectedIndex = 0;
                int x = dgResults.Columns.Count;
            }
        }

        private void RenderFinishWithWarning(string errorMessage)
        {
            waitCursor.Visibility = Visibility.Collapsed;

            if (Is2x2)
            {
                pnlMainContent.Visibility = System.Windows.Visibility.Visible;
                pnlCrosstabContent.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                pnlMainContent.Visibility = System.Windows.Visibility.Collapsed;
                pnlCrosstabContent.Visibility = System.Windows.Visibility.Visible;
            }

            pnlStatus.Background = Brushes.Gold;
            pnlStatusTop.Background = Brushes.Goldenrod;

            pnlStatus.Visibility = System.Windows.Visibility.Visible;
            txtStatus.Text = errorMessage;
            CollapseConfigPanel();
            CheckAndSetPosition();
        }

        private void RenderFinishWithError(string errorMessage)
        {
            waitCursor.Visibility = Visibility.Collapsed;

            pnlStatus.Visibility = System.Windows.Visibility.Visible;
            txtStatus.Text = errorMessage;

            pnlStatus.Background = Brushes.Tomato;
            pnlStatusTop.Background = Brushes.Red;

            pnlMainContent.Visibility = System.Windows.Visibility.Collapsed;
            pnlCrosstabContent.Visibility = System.Windows.Visibility.Collapsed;
            CollapseConfigPanel();
            CheckAndSetPosition();
        }

        private void RequestUpdateStatusMessage(string statusMessage)
        {
            this.Dispatcher.BeginInvoke(new SetStatusDelegate(SetStatusMessage), statusMessage);
        }

        private void SetStatusMessage(string statusMessage)
        {
            pnlStatus.Visibility = System.Windows.Visibility.Visible;
            txtStatus.Text = statusMessage;
        }

        private bool IsCancelled()
        {
            if (worker != null && worker.WorkerSupportsCancellation && worker.CancellationPending)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        void Field_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!loadingCombos)
            {
                RefreshResults();
            }
        }

        private void FillComboboxes(bool update = false)
        {
            loadingCombos = true;

            object prevExposureField = string.Empty;
            string prevOutcomeField = string.Empty;

            if (update)
            {
                if (cbxExposureField.SelectedIndex >= 0)
                {
                    if (cbxExposureField.SelectedItem is GroupField)
                    {
                        prevExposureField = cbxExposureField.SelectedItem;
                    }
                    else
                    {
                        prevExposureField = cbxExposureField.SelectedItem.ToString();
                    }
                }
                if (cbxOutcomeField.SelectedIndex >= 0)
                {
                    prevOutcomeField = cbxOutcomeField.SelectedItem.ToString();
                }
            }

            cbxExposureField.ItemsSource = null;
            cbxExposureField.Items.Clear();

            cbxOutcomeField.ItemsSource = null;
            cbxOutcomeField.Items.Clear();

            ColumnDataType columnDataType = ColumnDataType.Text | ColumnDataType.Numeric | ColumnDataType.Boolean | ColumnDataType.UserDefined;

            if (dashboardHelper.IsUsingEpiProject)
            {
                List<string> fieldNames = dashboardHelper.GetFieldsAsList(columnDataType);

                foreach (string s in fieldNames)
                {
                    cbxExposureField.Items.Add(s);
                    cbxOutcomeField.Items.Add(s);
                }

                foreach (Epi.Page p in View.Pages)
                {
                    foreach (GroupField group in p.GroupFields)
                    {
                        cbxExposureField.Items.Add(group);
                    }
                }

                foreach (string s in dashboardHelper.GetGroupVariablesAsList())
                {
                    cbxExposureField.Items.Add(s);
                }
            }
            else
            {
                List<string> exposureFields = dashboardHelper.GetFieldsAsList(columnDataType);
                List<string> outcomeFields = dashboardHelper.GetFieldsAsList(columnDataType);

                exposureFields.AddRange(dashboardHelper.GetGroupVariablesAsList());

                cbxExposureField.ItemsSource = exposureFields;
                cbxOutcomeField.ItemsSource = outcomeFields;
            }

            if (cbxExposureField.Items.Count > 0)
            {
                cbxExposureField.SelectedIndex = -1;
            }
            if (cbxOutcomeField.Items.Count > 0)
            {
                cbxOutcomeField.SelectedIndex = -1;
            }

            if (update)
            {
                cbxExposureField.SelectedItem = prevExposureField;
                cbxOutcomeField.SelectedItem = prevOutcomeField;
            }

            loadingCombos = false;
        }

        void Triangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement source = sender as FrameworkElement;
            CollapseExpandConfigPanel();
        }

        /// <summary>
        /// Handles the collapsing and expanding of the gadget's configuration panel.
        /// </summary>
        private void CollapseExpandConfigPanel()
        {
            if (triangleCollapsed)
            {
                ExpandConfigPanel();
            }
            else
            {
                CollapseConfigPanel();
            }
        }

        /// <summary>
        /// Forces a collapse of the config panel
        /// </summary>
        private void CollapseConfigPanel()
        {
            ConfigCollapsedTriangle.Visibility = Visibility.Visible;
            ConfigExpandedTriangle.Visibility = Visibility.Collapsed;
            ConfigCollapsedTitle.Visibility = Visibility.Visible;
            ConfigGrid.Height = 50;
            triangleCollapsed = true;
        }

        /// <summary>
        /// Forces an expansion of the config panel
        /// </summary>
        private void ExpandConfigPanel()
        {
            ConfigExpandedTriangle.Visibility = Visibility.Visible;
            ConfigCollapsedTriangle.Visibility = Visibility.Collapsed;
            ConfigCollapsedTitle.Visibility = Visibility.Collapsed;
            ConfigGrid.Height = Double.NaN;
            ConfigGrid.UpdateLayout();
            triangleCollapsed = false;
        }

        /// <summary>
        /// Closes the gadget
        /// </summary>
        private void CloseGadget()
        {
            if (worker != null && worker.WorkerSupportsCancellation)
            {
                worker.CancelAsync();
            }

            if (worker != null)
            {
                worker.DoWork -= new System.ComponentModel.DoWorkEventHandler(worker_DoWork);                
            }
            if (baseWorker != null)
            {
                baseWorker.DoWork -= new System.ComponentModel.DoWorkEventHandler(Execute);
            }

            this.GadgetStatusUpdate -= new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation -= new GadgetCheckForCancellationHandler(IsCancelled);

            dgResults.ItemsSource = null;

            if (GadgetClosing != null)
                GadgetClosing(this);

            gadgetOptions = null;

            GadgetClosing = null;
            GadgetCheckForCancellation = null;
            GadgetProcessingFinished = null;
            GadgetStatusUpdate = null;
        }

        private void ClearGroupResults() 
        {
            dgResults.Visibility = Visibility.Collapsed;
            ResetGrid();
        }

        //private void Execute(List<string> exposureVars, string outcomeVar)
        private void ExecuteGroup(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (worker != null && worker.WorkerSupportsCancellation)
            {
                worker.CancelAsync();
            }

            lock (syncLock)
            {
                worker = new System.ComponentModel.BackgroundWorker();
                worker.WorkerSupportsCancellation = true;
                worker.DoWork += new System.ComponentModel.DoWorkEventHandler(groupWorker_DoWork);
                worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(groupWorker_WorkerCompleted);
                worker.RunWorkerAsync(gadgetOptions);
            }

            currentData = new Dictionary<string, GridCells>();
        }

        /// <summary>
        /// Event used to handle the 'Advanced options' collapse/expand arrow
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void AdvTriangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement source = sender as FrameworkElement;
            if (advTriangleCollapsed)
            {
                AdvExpandedTriangle.Visibility = Visibility.Visible;
                AdvCollapsedTriangle.Visibility = Visibility.Collapsed;
                panelAdvanced.Visibility = Visibility.Visible;
            }
            else
            {
                AdvCollapsedTriangle.Visibility = Visibility.Visible;
                AdvExpandedTriangle.Visibility = Visibility.Collapsed;
                panelAdvanced.Visibility = Visibility.Collapsed;
            }
            advTriangleCollapsed = !advTriangleCollapsed;
        }

        /// <summary>
        /// Handles the WorkerCompleted event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void groupWorker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (this.isGrouped)
            {
                this.Dispatcher.BeginInvoke(new SimpleCallback(ShowGrid));
            }
        }

        void groupWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            lock (syncLock)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                try
                {   
                    currentData = new Dictionary<string, GridCells>();

                    Is2x2 = true;

                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));
                    this.Dispatcher.BeginInvoke(new SimpleCallback(ClearGroupResults));

                    List<string> exposureVars = new List<string>();
                    foreach (KeyValuePair<string, string> kvp in gadgetOptions.InputVariableList)
                    {
                        if (kvp.Value.ToLower().Equals("groupfield"))
                        {
                            exposureVars.Add(kvp.Key);
                        }
                    }
                    string outcomeVar = gadgetOptions.CrosstabVariableName;

                    //if(dashboardHelper.GetDistinctValueCount(outcomeVar) > 2) 
                    //{
                    //    throw new ArgumentException(SharedStrings.OUTCOME_VALUES_TOO_MANY);
                    //}

                    Dictionary<string, GridCells> gridCellCollection = new Dictionary<string, GridCells>();
                    foreach (string exposureVar in exposureVars)
                    {
                        if (worker.CancellationPending)
                        {
                            this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.DASHBOARD_GADGET_STATUS_OPERATION_CANCELLED);
                            this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                            Debug.Print("2x2 thread cancelled");
                            return;                        
                        }

                        GridCells cells = CalculateTables(exposureVar, outcomeVar);
                        if (cells.ttVal < 0)
                        {
                            continue;
                        }
                        else if (cells.ynVal == 0 && cells.yyVal == 0)
                        {
                        }
                        else if (cells.nyVal == 0 && cells.nnVal == 0)
                        {
                        }
                        else
                        {
                            gridCellCollection.Add(exposureVar, cells);
                        }
                    }

                    if (gridCellCollection.Count == 0)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), "No data is available in the fields selected for this gadget.");
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));                        
                        return; 
                    }
                    
                    this.Dispatcher.BeginInvoke(new RenderMultiGridDelegate(RenderMultiGrid), gridCellCollection);
                    this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                }
                catch (DuplicateNameException ex)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message + " " + "Check to make sure that the exposure variable is not contained within the group field.");
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                }                    
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                }
                finally
                {
                    stopwatch.Stop();
                    Debug.Print("2x2 table gadget (on group variable) took " + stopwatch.Elapsed.ToString() + " seconds to complete with " + dashboardHelper.RecordCount.ToString() + " records and the following filters:");
                    Debug.Print(dashboardHelper.DataFilters.GenerateDataFilterString());
                }
            }
        }

        public void ClearSingleResults() 
        {
            pnlStatus.Visibility = Visibility.Collapsed;
            txtStatus.Text = string.Empty;

            grdGraph.Visibility = Visibility.Visible;
            grdTables.Visibility = Visibility.Visible;            
            waitCursor.Visibility = Visibility.Visible;
            dgResults.Visibility = Visibility.Collapsed;
            pnlCrosstabContent.Visibility = Visibility.Collapsed;

            ResetGrid();
            
            txtExposure.Visibility = Visibility.Visible;            
        }

        //public void Execute(string exposureVar, string outcomeVar)
        private void Execute(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (worker != null && worker.WorkerSupportsCancellation)
            {
                worker.CancelAsync();
            }

            lock (syncLock)
            {
                worker = new System.ComponentModel.BackgroundWorker();
                worker.WorkerSupportsCancellation = true;
                worker.DoWork += new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
                //worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(worker_WorkerCompleted);
                worker.RunWorkerAsync(gadgetOptions);
            }

            //string[] workerParams = new string[2];
            //workerParams[0] = exposureVar;
            //workerParams[1] = outcomeVar;

            //txtOutcome.Text = outcomeVar;
            //txtExposure.Text = exposureVar;

            //worker = new System.ComponentModel.BackgroundWorker();
            //worker.WorkerSupportsCancellation = true;
            //worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            ////worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(worker_WorkerCompleted);
            //worker.RunWorkerAsync(workerParams);
        }

        private void SetExposureHeadings(string yesHeading, string noHeading)
        {
            //exposureYes.Text = yesHeading;
            //exposureNo.Text = noHeading;
        }

        private void SetOutcomeHeadings(string yesHeading, string noHeading)
        {
            outcomeYes.Text = yesHeading;
            outcomeNo.Text = noHeading;
        }

        private GridCells CalculateTables(string exposureVar, string outcomeVar)
        {
            DataTable twoByTwoTable = dashboardHelper.Generate2x2Table(exposureVar, outcomeVar);
            
            GridCells gridCells = new GridCells();

            if (twoByTwoTable == null || twoByTwoTable.Rows.Count == 0)
            {
                gridCells.ttVal = -999;
                return gridCells;
            }

            if (twoByTwoTable.Rows.Count == 3)
            {
                return gridCells;
            }
            else if (twoByTwoTable.Rows.Count == 1)
            {
                return gridCells;
            }

            this.Dispatcher.BeginInvoke(new SetOutcomeHeadingsDelegate(SetOutcomeHeadings), twoByTwoTable.Columns[1].ColumnName, twoByTwoTable.Columns[2].ColumnName);
            //this.Dispatcher.BeginInvoke(new SetExposureHeadingsDelegate(SetExposureHeadings), twoByTwoTable.Rows[0][0].ToString(), twoByTwoTable.Rows[1][0].ToString());

            if (twoByTwoTable != null && twoByTwoTable.Rows.Count != 0)
            {
                gridCells.yyVal = int.Parse(twoByTwoTable.Rows[0][1].ToString());
                gridCells.ynVal = int.Parse(twoByTwoTable.Rows[0][2].ToString());
                gridCells.nyVal = int.Parse(twoByTwoTable.Rows[1][1].ToString());
                gridCells.nnVal = int.Parse(twoByTwoTable.Rows[1][2].ToString());
                gridCells.ntVal = gridCells.nnVal + gridCells.nyVal;
                gridCells.ytVal = gridCells.yyVal + gridCells.ynVal;
                
                gridCells.ttVal = gridCells.ytVal + gridCells.ntVal;
                gridCells.tyVal = gridCells.yyVal + gridCells.nyVal;
                gridCells.tnVal = gridCells.ynVal + gridCells.nnVal;
                if (gridCells.ytVal != 0)
                {
                    gridCells.yyRowPct = 1.0 * gridCells.yyVal / gridCells.ytVal;
                    gridCells.ynRowPct = 1.0 * gridCells.ynVal / gridCells.ytVal;
                }
                if (gridCells.ntVal != 0)
                {
                    gridCells.nyRowPct = 1.0 * gridCells.nyVal / gridCells.ntVal;
                    gridCells.nnRowPct = 1.0 * gridCells.nnVal / gridCells.ntVal;
                }
                if (gridCells.tyVal != 0)
                {
                    gridCells.yyColPct = 1.0 * gridCells.yyVal / gridCells.tyVal;
                    gridCells.nyColPct = 1.0 * gridCells.nyVal / gridCells.tyVal;
                }
                if (gridCells.tnVal != 0)
                {
                    gridCells.ynColPct = 1.0 * gridCells.ynVal / gridCells.tnVal;
                    gridCells.nnColPct = 1.0 * gridCells.nnVal / gridCells.tnVal;
                }
                if (gridCells.ttVal != 0)
                {
                    gridCells.tyRowPct = 1.0 * gridCells.tyVal / gridCells.ttVal;
                    gridCells.tnRowPct = 1.0 * gridCells.tnVal / gridCells.ttVal;
                    gridCells.ytColPct = 1.0 * gridCells.ytVal / gridCells.ttVal;
                    gridCells.ntColPct = 1.0 * gridCells.ntVal / gridCells.ttVal;
                }
                if (gridCells.tyVal > 0 && gridCells.tnVal > 0 && gridCells.ytVal > 0 && gridCells.ntVal > 0)
                {
                    gridCells.singleTableResults = new StatisticsRepository.cTable().SigTable((double)gridCells.yyVal, (double)gridCells.ynVal, (double)gridCells.nyVal, (double)gridCells.nnVal, 0.95);
                }
            }
            return gridCells;
        }

        void Swap2x2RowValues(DataTable table)
        {
            if (table.Rows.Count > 2 || table.Columns.Count > 3)
            {
                return; // cannot do an invalid 2x2 table
            }

            object row1Col1 = table.Rows[0][1];
            object row1Col2 = table.Rows[0][2];

            object row2Col1 = table.Rows[1][1];
            object row2Col2 = table.Rows[1][2];

            table.Rows[0][1] = row2Col1;
            table.Rows[0][2] = row2Col2;

            table.Rows[1][1] = row1Col1;
            table.Rows[1][2] = row1Col2;

            object firstRowName = table.Rows[0][0];
            table.Rows[0][0] = table.Rows[1][0];
            table.Rows[1][0] = firstRowName;
        }

        void Swap2x2ColValues(DataTable table)
        {
            if (table.Rows.Count > 2 || table.Columns.Count > 3)
            {
                return; // cannot do an invalid 2x2 table
            }

            object row1Col1 = table.Rows[0][1];
            object row1Col2 = table.Rows[0][2];

            object row2Col1 = table.Rows[1][1];
            object row2Col2 = table.Rows[1][2];

            table.Rows[0][1] = row1Col2;
            table.Rows[0][2] = row1Col1;

            table.Rows[1][1] = row2Col2;
            table.Rows[1][2] = row2Col1;

            string firstColumnName = table.Columns[1].ColumnName;
            string secondColumnName = table.Columns[2].ColumnName;

            table.Columns[1].ColumnName = "_COL1_";
            table.Columns[2].ColumnName = "_COL2_";

            table.Columns[1].ColumnName = secondColumnName;
            table.Columns[2].ColumnName = firstColumnName;
        }

        void CheckLabels(DataTable table)
        {
            Dictionary<string, string> booleanValues = new Dictionary<string, string>();
            booleanValues.Add("0", "1");
            booleanValues.Add("false", "true");
            booleanValues.Add("f", "t");
            //booleanValues.Add("no", "yes");
            booleanValues.Add("n", "y");
            if (!booleanValues.ContainsKey(dashboardHelper.Config.Settings.RepresentationOfNo.ToLower()))
            {
                booleanValues.Add(dashboardHelper.Config.Settings.RepresentationOfNo.ToLower(), dashboardHelper.Config.Settings.RepresentationOfYes.ToLower());
            }

            string firstColumnName = table.Columns[1].ColumnName.ToLower();
            string secondColumnName = table.Columns[2].ColumnName.ToLower();

            if (booleanValues.ContainsKey(firstColumnName) && secondColumnName.Equals(booleanValues[firstColumnName]))
            {
                Swap2x2ColValues(table);
            }

            string firstRowName = table.Rows[0][0].ToString();
            string secondRowName = table.Rows[1][0].ToString();

            if (booleanValues.ContainsKey(firstRowName) && secondRowName.Equals(booleanValues[firstRowName]))
            {
                Swap2x2RowValues(table);
            }
        }

        /// <summary>
        /// Handles the check / unchecked events
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void checkboxCheckChanged(object sender, RoutedEventArgs e)
        {
            RefreshResults();
        }

        /// <summary>
        /// Handles the click event for the 'Send data to web browser' context menu option
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void mnuSendDataToHTML_Click(object sender, RoutedEventArgs e)
        {
            //if (this.strataGridList.Count == 0)
            //{
            //    return;
            //}            

            try
            {
                string fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString("N") + ".html";//GetHTMLLineListing();

                System.IO.FileStream stream = System.IO.File.OpenWrite(fileName);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(stream);
                sw.WriteLine("<html><head><title>2x2 Table</title>");
                sw.WriteLine("<meta http-equiv=\"content-type\" content=\"text/html;charset=UTF-8\" />");
                sw.WriteLine("<meta name=\"author\" content=\"Epi Info 7\" />");
                sw.WriteLine(dashboardHelper.GenerateStandardHTMLStyle().ToString());

                sw.WriteLine(this.ToHTML());
                sw.Close();
                sw.Dispose();

                if (!string.IsNullOrEmpty(fileName))
                {
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.FileName = fileName;
                    proc.StartInfo.UseShellExecute = true;
                    proc.Start();
                }
            }
            finally
            {
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            lock (syncLock)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearSingleResults));

                //string[] workerParams = (string[])e.Argument;
                //string exposureVar = workerParams[0];
                //string outcomeVar = workerParams[1];
                int rowCount = 1;
                int columnCount = 1;                
                bool exceededMaxRows = false;
                bool exceededMaxColumns = false;
                bool includeMissing = false;

                try
                {
                    RequestUpdateStatusDelegate requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
                    CheckForCancellationDelegate checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

                    gadgetOptions.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                    gadgetOptions.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);

                    Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables = dashboardHelper.GenerateFrequencyTable(gadgetOptions);

                    if (stratifiedFrequencyTables == null || stratifiedFrequencyTables.Count == 0)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.NO_RECORDS_SELECTED);
                    }
                    else if (worker.CancellationPending)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.DASHBOARD_GADGET_STATUS_OPERATION_CANCELLED);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        Debug.Print("Thread cancelled");
                        return;
                    }
                    else if (stratifiedFrequencyTables.Count > 1)
                    {
                        throw new ApplicationException("Can not have more than one table in 2x2 gadget.");
                    }
                    else
                    {
                        Is2x2 = false;
                        DataTable table = new DataTable();
                        List<DescriptiveStatistics> statsList = new List<DescriptiveStatistics>();

                        foreach (KeyValuePair<DataTable, List<DescriptiveStatistics>> tableKvp in stratifiedFrequencyTables)
                        {
                            statsList = tableKvp.Value;
                            table = tableKvp.Key;

                            double count = 0;
                            foreach (DescriptiveStatistics ds in tableKvp.Value)
                            {
                                count = count + ds.observations;
                            }

                            if (count == 0 && stratifiedFrequencyTables.Count == 1)
                            {
                                // this is the only table and there are no records, so let the user know
                                this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.NO_RECORDS_SELECTED);
                                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                                return;
                            }

                            // we have a 2x2 table
                            if (table.Rows.Count == 2 && table.Columns.Count == 3)
                            {
                                Is2x2 = true;
                            }
                        }

                        //DataTable twoByTwoTable = dashboardHelper.Generate2x2Table(exposureVar, outcomeVar);

                        if (Is2x2)
                        {
                            GridCells gridCells = new GridCells();

                            if (table != null && table.Rows.Count != 0)
                            {
                                if (gadgetOptions.InputVariableList.ContainsKey("smarttable") && gadgetOptions.InputVariableList["smarttable"].Equals("true"))
                                {
                                    CheckLabels(table);
                                }

                                gridCells.yyVal = int.Parse(table.Rows[0][1].ToString());
                                gridCells.ynVal = int.Parse(table.Rows[0][2].ToString());
                                gridCells.nyVal = int.Parse(table.Rows[1][1].ToString());
                                gridCells.nnVal = int.Parse(table.Rows[1][2].ToString());
                                gridCells.ntVal = gridCells.nnVal + gridCells.nyVal;
                                gridCells.ytVal = gridCells.yyVal + gridCells.ynVal;

                                gridCells.ttVal = gridCells.ytVal + gridCells.ntVal;
                                gridCells.tyVal = gridCells.yyVal + gridCells.nyVal;
                                gridCells.tnVal = gridCells.ynVal + gridCells.nnVal;
                                if (gridCells.ytVal != 0)
                                {
                                    gridCells.yyRowPct = 1.0 * gridCells.yyVal / gridCells.ytVal;
                                    gridCells.ynRowPct = 1.0 * gridCells.ynVal / gridCells.ytVal;
                                }
                                if (gridCells.ntVal != 0)
                                {
                                    gridCells.nyRowPct = 1.0 * gridCells.nyVal / gridCells.ntVal;
                                    gridCells.nnRowPct = 1.0 * gridCells.nnVal / gridCells.ntVal;
                                }
                                if (gridCells.tyVal != 0)
                                {
                                    gridCells.yyColPct = 1.0 * gridCells.yyVal / gridCells.tyVal;
                                    gridCells.nyColPct = 1.0 * gridCells.nyVal / gridCells.tyVal;
                                }
                                if (gridCells.tnVal != 0)
                                {
                                    gridCells.ynColPct = 1.0 * gridCells.ynVal / gridCells.tnVal;
                                    gridCells.nnColPct = 1.0 * gridCells.nnVal / gridCells.tnVal;
                                }
                                if (gridCells.ttVal != 0)
                                {
                                    gridCells.tyRowPct = 1.0 * gridCells.tyVal / gridCells.ttVal;
                                    gridCells.tnRowPct = 1.0 * gridCells.tnVal / gridCells.ttVal;
                                    gridCells.ytColPct = 1.0 * gridCells.ytVal / gridCells.ttVal;
                                    gridCells.ntColPct = 1.0 * gridCells.ntVal / gridCells.ttVal;
                                }
                                if (gridCells.tyVal > 0 && gridCells.tnVal > 0 && gridCells.ytVal > 0 && gridCells.ntVal > 0)
                                {
                                    gridCells.singleTableResults = new StatisticsRepository.cTable().SigTable((double)gridCells.yyVal, (double)gridCells.ynVal, (double)gridCells.nyVal, (double)gridCells.nnVal, 0.95);
                                }
                            }
                            
                            if (!string.IsNullOrEmpty(gridCells.singleTableResults.ErrorMessage))
                            {
                                //this.Dispatcher.BeginInvoke(new SimpleCallback(ClearSingleResults));
                                //this.Dispatcher.BeginInvoke(new ShowErrorMessage(ShowError), gridCells.singleTableResults.ErrorMessage);
                                this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), gridCells.singleTableResults.ErrorMessage);

                                stopwatch.Stop();
                                Debug.Print("2x2 table gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete with " + dashboardHelper.RecordCount.ToString() + " records and the following filters:");
                                Debug.Print(dashboardHelper.DataFilters.GenerateDataFilterString());
                            }
                            else
                            {
                                this.Dispatcher.BeginInvoke(new RenderGridDelegate(RenderGridCells), gridCells, table);
                                this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));                                
                            }
                            this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        } // end if do2x2
                        else
                        {
                            AddFreqGridDelegate addGrid = new AddFreqGridDelegate(AddFreqGrid);
                            SetGridTextDelegate setText = new SetGridTextDelegate(SetGridText);
                            AddGridRowDelegate addRow = new AddGridRowDelegate(AddGridRow);
                            RenderFrequencyHeaderDelegate renderHeader = new RenderFrequencyHeaderDelegate(RenderFrequencyHeader);
                            DrawFrequencyBordersDelegate drawBorders = new DrawFrequencyBordersDelegate(DrawFrequencyBorders);

                            string strataValue = table.TableName;
                            string freqVar = gadgetOptions.MainVariableName;

                            this.Dispatcher.BeginInvoke(addGrid, string.Empty, table.TableName, table.Columns.Count);

                            double count = 0;
                            foreach (DescriptiveStatistics ds in statsList)
                            {
                                count = count + ds.observations;
                            }

                            if (count == 0)
                            {
                                this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.NO_RECORDS_SELECTED);
                                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                                return;
                            }

                            if (table.Rows.Count == 0)
                            {
                                this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.NO_RECORDS_SELECTED);
                                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                                return;
                            }

                            string tableHeading = strataValue;

                            this.Dispatcher.BeginInvoke(renderHeader, strataValue, tableHeading, table.Columns);
                            rowCount = 1;

                            int[] totals = new int[table.Columns.Count - 1];
                            columnCount = 1;

                            foreach (System.Data.DataRow row in table.Rows)
                            {
                                if (!row[freqVar].Equals(DBNull.Value) || (row[freqVar].Equals(DBNull.Value) && includeMissing == true))
                                {
                                    Field field = null;
                                    foreach (DataRow fieldRow in dashboardHelper.FieldTable.Rows)
                                    {
                                        if (fieldRow["columnname"].Equals(freqVar))
                                        {
                                            if (fieldRow["epifieldtype"] is Field)
                                            {
                                                field = fieldRow["epifieldtype"] as Field;
                                            }
                                            break;
                                        }
                                    }

                                    this.Dispatcher.Invoke(addRow, strataValue, 30);
                                    string displayValue = row[freqVar].ToString();

                                    if (dashboardHelper.IsUserDefinedColumn(freqVar))
                                    {
                                        displayValue = dashboardHelper.GetFormattedOutput(freqVar, row[freqVar]);
                                    }
                                    else
                                    {
                                        if (field != null && field is YesNoField)
                                        {
                                            if (row[freqVar].ToString().Equals("1"))
                                                displayValue = "Yes";
                                            else if (row[freqVar].ToString().Equals("0"))
                                                displayValue = "No";
                                        }
                                        else if (field != null && field is DateField)
                                        {
                                            displayValue = string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:d}", row[freqVar]);
                                        }
                                        else if (field != null && field is TimeField)
                                        {
                                            displayValue = string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:T}", row[freqVar]);
                                        }
                                        else
                                        {
                                            displayValue = dashboardHelper.GetFormattedOutput(freqVar, row[freqVar]);
                                        }
                                    }

                                    if (string.IsNullOrEmpty(displayValue))
                                    {
                                        Configuration config = dashboardHelper.Config;
                                        displayValue = config.Settings.RepresentationOfMissing;
                                    }

                                    this.Dispatcher.BeginInvoke(setText, strataValue/*grdFreq*/, new TextBlockConfig(StringLiterals.SPACE + displayValue + StringLiterals.SPACE, new Thickness(2, 0, 2, 0), VerticalAlignment.Center, HorizontalAlignment.Left, rowCount, 0), FontWeights.Normal);


                                    int rowTotal = 0;
                                    columnCount = 1;

                                    foreach (DataColumn column in table.Columns)
                                    {
                                        if (columnCount > MaxColumns)
                                        {
                                            //this.Dispatcher.BeginInvoke(new ShowWarningDelegate(ShowWarning), (frequencies.Columns.Count - maxColumns).ToString() + " additional columns were not displayed due to gadget settings.");
                                            exceededMaxColumns = true;
                                            break;
                                        }

                                        if (column.ColumnName.Equals(freqVar))
                                        {
                                            continue;
                                        }

                                        this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(StringLiterals.SPACE + row[column.ColumnName].ToString() + StringLiterals.SPACE, new Thickness(2, 0, 2, 0), VerticalAlignment.Center, HorizontalAlignment.Right, rowCount, columnCount), FontWeights.Normal);
                                        columnCount++;

                                        int rowValue = 0;
                                        bool success = int.TryParse(row[column.ColumnName].ToString(), out rowValue);
                                        if (success)
                                        {
                                            totals[columnCount - 2] = totals[columnCount - 2] + rowValue;
                                            rowTotal = rowTotal + rowValue;
                                        }
                                    }

                                    this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(StringLiterals.SPACE + rowTotal.ToString() + StringLiterals.SPACE, new Thickness(2, 0, 2, 0), VerticalAlignment.Center, HorizontalAlignment.Right, rowCount, columnCount), FontWeights.Bold);

                                    rowCount++;
                                }

                                if (rowCount > MaxRows)
                                {
                                    foreach (DataColumn column in table.Columns)
                                    {
                                        if (columnCount > MaxColumns)
                                        {
                                            exceededMaxColumns = true;
                                            break;
                                        }

                                        if (column.ColumnName.Equals(freqVar))
                                        {
                                            continue;
                                        }

                                        this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(StringLiterals.SPACE + StringLiterals.ELLIPSIS + StringLiterals.SPACE, new Thickness(2, 0, 2, 0), VerticalAlignment.Center, HorizontalAlignment.Right, rowCount, columnCount), FontWeights.Normal);
                                        columnCount++;
                                    }

                                    this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(StringLiterals.SPACE + StringLiterals.ELLIPSIS + StringLiterals.SPACE, new Thickness(2, 0, 2, 0), VerticalAlignment.Center, HorizontalAlignment.Right, rowCount, columnCount), FontWeights.Bold);


                                    rowCount++;
                                    exceededMaxRows = true;
                                    break;
                                }
                            }

                            this.Dispatcher.BeginInvoke(new AddGridFooterDelegate(RenderFrequencyFooter), strataValue, rowCount, totals);
                            this.Dispatcher.BeginInvoke(drawBorders, strataValue);

                            if (exceededMaxRows && exceededMaxColumns)
                            {
                                this.Dispatcher.BeginInvoke(new RenderFinishWithWarningDelegate(RenderFinishWithWarning), "Warning: Some rows and columns were not displayed due to gadget settings. Showing top " + MaxRows.ToString() + " rows and top " + MaxColumns.ToString() + " columns only.");
                            }
                            else if (exceededMaxColumns)
                            {
                                this.Dispatcher.BeginInvoke(new RenderFinishWithWarningDelegate(RenderFinishWithWarning), "Warning: Some columns were not displayed due to gadget settings. Showing top " + MaxColumns.ToString() + " columns only.");
                            }
                            else if (exceededMaxRows)
                            {
                                this.Dispatcher.BeginInvoke(new RenderFinishWithWarningDelegate(RenderFinishWithWarning), string.Format(SharedStrings.DASHBOARD_GADGET_STATUS_ROW_LIMIT, MaxRows.ToString()));
                            }
                            else if(rowCount > 2)
                            {
                                this.Dispatcher.BeginInvoke(new RenderFinishWithWarningDelegate(RenderFinishWithWarning), "Warning: More than two values exist in the exposure field. Displaying an MxN table.");                                
                            }
                            else if (columnCount > 3)
                            {
                                this.Dispatcher.BeginInvoke(new RenderFinishWithWarningDelegate(RenderFinishWithWarning), "Warning: More than two values exist in the outcome field. Displaying an MxN table.");
                            }
                            else if (rowCount < 2)
                            {
                                this.Dispatcher.BeginInvoke(new RenderFinishWithWarningDelegate(RenderFinishWithWarning), "Warning: Less than two values exist in the exposure field. Displaying an MxN table.");
                            }
                            else if (columnCount < 3)
                            {
                                this.Dispatcher.BeginInvoke(new RenderFinishWithWarningDelegate(RenderFinishWithWarning), "Warning: Less than two values exist in the outcome field. Displaying an MxN table.");
                            }
                            else
                            {
                                this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));
                            }
                            this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                }
                finally
                {
                    stopwatch.Stop();
                    Debug.Print("2x2 table gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete with " + dashboardHelper.RecordCount.ToString() + " records and the following filters:");
                    Debug.Print(dashboardHelper.DataFilters.GenerateDataFilterString());
                }
            }
        }

        private void RenderMultiGrid(Dictionary<string,GridCells> gridCellCollection)
        {
            waitCursor.Visibility = Visibility.Collapsed;
            
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("Exposure", typeof(string)));
            dt.Columns.Add(new DataColumn("Outcome Rate" + Environment.NewLine + "Exposure", typeof(string)));
            dt.Columns.Add(new DataColumn("Outcome Rate" + Environment.NewLine + "No Exposure", typeof(string)));
            dt.Columns.Add(new DataColumn("Risk Ratio", typeof(string)));
            dt.Columns.Add(new DataColumn("Odds Ratio", typeof(string)));

            // TODO: Fix this a better way
            dgResults.Columns.Clear();
            Microsoft.Windows.Controls.DataGridTextColumn textColumn = new Microsoft.Windows.Controls.DataGridTextColumn();
            textColumn.Header = "Row";
            textColumn.Visibility = System.Windows.Visibility.Collapsed;
            dgResults.Columns.Add(textColumn);

            foreach (string exposureVar in gridCellCollection.Keys)
            {
                if (gridCellCollection[exposureVar].tyVal > 0 && gridCellCollection[exposureVar].tnVal > 0 && gridCellCollection[exposureVar].ytVal > 0 && gridCellCollection[exposureVar].ntVal > 0)
                {
                    string oddsRatioEstimate = string.Empty;
                    string riskRatioEstimate = string.Empty;
                    if (gridCellCollection[exposureVar].singleTableResults.OddsRatioEstimate != null)
                    {
                        oddsRatioEstimate = ((double)gridCellCollection[exposureVar].singleTableResults.OddsRatioEstimate).ToString("F4");
                    }
                    if (gridCellCollection[exposureVar].singleTableResults.RiskRatioEstimate != null)
                    {
                        riskRatioEstimate = ((double)gridCellCollection[exposureVar].singleTableResults.RiskRatioEstimate).ToString("F4");
                    }
                    dt.Rows.Add(exposureVar, gridCellCollection[exposureVar].yyRowPct.ToString("F4"), gridCellCollection[exposureVar].nyRowPct.ToString("F4"), riskRatioEstimate, oddsRatioEstimate);
                }
                else
                {
                    dt.Rows.Add(exposureVar, gridCellCollection[exposureVar].yyRowPct.ToString("F4"), gridCellCollection[exposureVar].nyRowPct.ToString("F4"), "0.0000", "0.0000");
                }
                currentData.Add(exposureVar, gridCellCollection[exposureVar]);
            }

            DataRow [] rowArray = dt.Select(string.Empty, "[Risk Ratio] DESC");
            dt = rowArray.CopyToDataTable().DefaultView.ToTable();

            if (worker.CancellationPending == false)
            {                
                dgResults.ItemsSource = dt.DefaultView;
                //dgResults.SelectedIndex = -1;
                //dgResults.UnselectAllCells();
                //dgResults.UnselectAll();
                //if (dgResults.Items.Count > 0)
                //    dgResults.SelectedIndex = 0;
            }
        }

        private void RenderGridCells(GridCells gridCells, DataTable twoByTwoTable)
        {
            waitCursor.Visibility = Visibility.Collapsed;

            if (twoByTwoTable.Rows.Count == 3)
            {
                pnlStatus.Visibility = Visibility.Visible;
                //pnlErrorMessages.Margin = new Thickness(0, ConfigGrid.ActualHeight, 0, 0);
                txtStatus.Text = twoByTwoTable.Rows[2][0].ToString();
                return;
            }
            else
            {
                //pnlErrorMessages.Visibility = System.Windows.Visibility.Collapsed;
                pnlStatus.Visibility = Visibility.Hidden;
                txtStatus.Text = string.Empty;
            }

            if (twoByTwoTable != null && twoByTwoTable.Rows.Count > 0)
            {
                pnlContent.Visibility = System.Windows.Visibility.Visible;

                exposureYes.Text = twoByTwoTable.Rows[0][0].ToString();
                exposureNo.Text = twoByTwoTable.Rows[1][0].ToString();

                outcomeYes.Text = twoByTwoTable.Columns[1].ColumnName;
                outcomeNo.Text = twoByTwoTable.Columns[2].ColumnName;

                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn("Exposure", typeof(string)));
                dt.Columns.Add(new DataColumn("Outcome Rate" + Environment.NewLine + "Exposure", typeof(string)));
                dt.Columns.Add(new DataColumn("Outcome Rate" + Environment.NewLine + "No Exposure", typeof(string)));
                dt.Columns.Add(new DataColumn("Risk Ratio", typeof(string)));
                dt.Columns.Add(new DataColumn("Odds Ratio", typeof(string)));

                if (gridCells.tyVal > 0 && gridCells.tnVal > 0 && gridCells.ytVal > 0 && gridCells.ntVal > 0)
                {
                    string oddsRatioEstimate = Epi.SharedStrings.UNDEFINED;
                    string riskRatioEstimate = Epi.SharedStrings.UNDEFINED;
                    if (gridCells.singleTableResults.OddsRatioEstimate != null)
                    {
                        oddsRatioEstimate = ((double)gridCells.singleTableResults.OddsRatioEstimate).ToString("F4");
                    }
                    if (gridCells.singleTableResults.RiskRatioEstimate != null)
                    {
                        riskRatioEstimate = ((double)gridCells.singleTableResults.RiskRatioEstimate).ToString("F4");
                    }
                    dt.Rows.Add("...", gridCells.yyRowPct.ToString("P"), gridCells.nyRowPct.ToString("P"), riskRatioEstimate, oddsRatioEstimate);
                }
                else
                {
                    dt.Rows.Add("...", gridCells.yyRowPct.ToString("P"), gridCells.nyRowPct.ToString("P"), "0", "0");
                }

                currentSingleTableData = gridCells;

                dgResults.ItemsSource = dt.DefaultView;

                dgResults.Visibility = System.Windows.Visibility.Collapsed; // Temp fix only, re-work later

                txtYesTotalVal.Text = gridCells.ytVal.ToString();
                txtYesYesVal.Text = gridCells.yyVal.ToString();
                txtYesNoVal.Text = gridCells.ynVal.ToString();
                txtNoYesVal.Text = gridCells.nyVal.ToString();
                txtNoNoVal.Text = gridCells.nnVal.ToString();
                txtNoTotalVal.Text = gridCells.ntVal.ToString();
                txtYesYesRow.Text = gridCells.yyRowPct.ToString("P");
                txtYesNoRow.Text = gridCells.ynRowPct.ToString("P");
                txtNoYesRow.Text = gridCells.nyRowPct.ToString("P");
                txtNoNoRow.Text = gridCells.nnRowPct.ToString("P");
                txtTotalTotalVal.Text = gridCells.ttVal.ToString();
                txtTotalYesVal.Text = gridCells.tyVal.ToString();
                txtYesYesCol.Text = gridCells.yyColPct.ToString("P");
                txtNoYesCol.Text = gridCells.nyColPct.ToString("P");
                txtTotalNoVal.Text = gridCells.tnVal.ToString();
                txtYesNoCol.Text = gridCells.ynColPct.ToString("P");
                txtNoNoCol.Text = gridCells.nnColPct.ToString("P");
                txtTotalYesRow.Text = gridCells.tyRowPct.ToString("P");
                txtTotalNoRow.Text = gridCells.tnRowPct.ToString("P");
                txtYesTotalCol.Text = gridCells.ytColPct.ToString("P");
                txtNoTotalCol.Text = gridCells.ntColPct.ToString("P");

                if (gridCells.tyVal > 0 && gridCells.tnVal > 0 && gridCells.ytVal > 0 && gridCells.ntVal > 0)
                {
                    pnlAdvanced.Visibility = Visibility.Visible;

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

                    if (gridCells.singleTableResults.FisherExactP != -1)
                    {
                        fisherExact = ((double)gridCells.singleTableResults.FisherExactP).ToString("F10");
                    }

                    if (gridCells.singleTableResults.FisherExact2P != -1)
                    {
                        fisherExact2P = ((double)gridCells.singleTableResults.FisherExact2P).ToString("F10");
                    }

                    if (gridCells.singleTableResults.OddsRatioMLEFisherLower != -1)
                    {
                        fisherLower = ((double)gridCells.singleTableResults.OddsRatioMLEFisherLower).ToString("F4");
                    }

                    if (gridCells.singleTableResults.OddsRatioMLEFisherUpper != -1)
                    {
                        fisherUpper = ((double)gridCells.singleTableResults.OddsRatioMLEFisherUpper).ToString("F4");
                    }

                    if (gridCells.singleTableResults.OddsRatioMLEEstimate != -1)
                    {
                        oddsRatioMLEEstimate = ((double)gridCells.singleTableResults.OddsRatioMLEEstimate).ToString("F4");
                    }

                    if (gridCells.singleTableResults.OddsRatioMLEMidPLower != -1)
                    {
                        oddsRatioMLEMidPLower = ((double)gridCells.singleTableResults.OddsRatioMLEMidPLower).ToString("F4");
                    }

                    if (gridCells.singleTableResults.OddsRatioMLEMidPUpper != -1)
                    {
                        oddsRatioMLEMidPUpper = ((double)gridCells.singleTableResults.OddsRatioMLEMidPUpper).ToString("F4");
                    }

                    if (gridCells.singleTableResults.OddsRatioEstimate != null)
                    {
                        oddsRatioEstimate = ((double)gridCells.singleTableResults.OddsRatioEstimate).ToString("F4");
                    }

                    if (gridCells.singleTableResults.OddsRatioLower != null)
                    {
                        oddsRatioLower = ((double)gridCells.singleTableResults.OddsRatioLower).ToString("F4");
                    }

                    if (gridCells.singleTableResults.OddsRatioUpper != null)
                    {
                        oddsRatioUpper = ((double)gridCells.singleTableResults.OddsRatioUpper).ToString("F4");
                    }

                    if (gridCells.singleTableResults.RiskRatioEstimate != null)
                    {
                        riskRatioEstimate = ((double)gridCells.singleTableResults.RiskRatioEstimate).ToString("F4");
                    }

                    if (gridCells.singleTableResults.RiskRatioLower != null)
                    {
                        riskRatioLower = ((double)gridCells.singleTableResults.RiskRatioLower).ToString("F4");
                    }

                    if (gridCells.singleTableResults.RiskRatioUpper != null)
                    {
                        riskRatioUpper = ((double)gridCells.singleTableResults.RiskRatioUpper).ToString("F4");
                    }

                    txtChiSqCorP.Text = gridCells.singleTableResults.ChiSquareYates2P.ToString("F10");
                    txtChiSqCorVal.Text = gridCells.singleTableResults.ChiSquareYatesVal.ToString("F4");
                    txtChiSqManP.Text = gridCells.singleTableResults.ChiSquareMantel2P.ToString("F10");
                    txtChiSqManVal.Text = gridCells.singleTableResults.ChiSquareMantelVal.ToString("F4");
                    txtChiSqUncP.Text = gridCells.singleTableResults.ChiSquareUncorrected2P.ToString("F10");
                    txtChiSqUncVal.Text = gridCells.singleTableResults.ChiSquareUncorrectedVal.ToString("F4");
                    txtOddsRatioEstimate.Text = oddsRatioEstimate;
                    txtOddsRatioLower.Text = oddsRatioLower;
                    txtOddsRatioUpper.Text = oddsRatioUpper;
                    txtMidPEstimate.Text = oddsRatioMLEEstimate;
                    txtMidPLower.Text = oddsRatioMLEMidPLower;
                    txtMidPUpper.Text = oddsRatioMLEMidPUpper;
                    txtFisherLower.Text = fisherLower;
                    txtFisherUpper.Text = fisherUpper;
                    txtRiskDifferenceEstimate.Text = gridCells.singleTableResults.RiskDifferenceEstimate.ToString("F4");
                    txtRiskDifferenceLower.Text = gridCells.singleTableResults.RiskDifferenceLower.ToString("F4");
                    txtRiskDifferenceUpper.Text = gridCells.singleTableResults.RiskDifferenceUpper.ToString("F4");
                    txtRiskRatioEstimate.Text = riskRatioEstimate; // gridCells.singleTableResults.RiskRatioEstimate.ToString("F4");
                    txtRiskRatioLower.Text = riskRatioLower;
                    txtRiskRatioUpper.Text = riskRatioUpper;
                    txtFisherExact.Text = fisherExact;
                    txtFisherExact2P.Text = fisherExact2P;
                    txtMidPExact.Text = gridCells.singleTableResults.MidP.ToString("F10");
                }
                else
                {
                    pnlAdvanced.Visibility = Visibility.Collapsed;
                }
                rctRed.Height = 1;
                rctRed.Width = 1;
                rctYellow.Height = 1;
                rctYellow.Width = 1;
                rctOrange.Height = 1;
                rctOrange.Width = 1;
                rctGreen.Height = 1;
                rctGreen.Width = 1;

                DoubleAnimation daRed = new DoubleAnimation();
                daRed.From = 1;
                if (gridCells.ttVal != 0)
                    daRed.To = 149.0 * gridCells.yyVal / gridCells.ttVal;
                else
                    daRed.To = 1;
                daRed.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                rctRed.BeginAnimation(Rectangle.HeightProperty, daRed);
                rctRed.BeginAnimation(Rectangle.WidthProperty, daRed);

                DoubleAnimation daYellow = new DoubleAnimation();
                daYellow.From = 1;
                if (gridCells.ttVal != 0)
                    daYellow.To = 149.0 * gridCells.nyVal / gridCells.ttVal;
                else
                    daYellow.To = 1;
                daYellow.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                rctYellow.BeginAnimation(Rectangle.HeightProperty, daYellow);
                rctYellow.BeginAnimation(Rectangle.WidthProperty, daYellow);

                DoubleAnimation daOrange = new DoubleAnimation();
                daOrange.From = 1;
                if (gridCells.ttVal != 0)
                    daOrange.To = 149.0 * gridCells.ynVal / gridCells.ttVal;
                else
                    daOrange.To = 1;
                daOrange.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                rctOrange.BeginAnimation(Rectangle.HeightProperty, daOrange);
                rctOrange.BeginAnimation(Rectangle.WidthProperty, daOrange);

                DoubleAnimation daGreen = new DoubleAnimation();
                daGreen.From = 1;
                if (gridCells.ttVal != 0)
                    daGreen.To = 149.0 * gridCells.nnVal / gridCells.ttVal;
                else
                    daGreen.To = 1;
                daGreen.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                rctGreen.BeginAnimation(Rectangle.HeightProperty, daGreen);
                rctGreen.BeginAnimation(Rectangle.WidthProperty, daGreen);
            }
            else
            {
                ResetGrid();
            }
        }

        /// <summary>
        /// Copies a grid's output to the clipboard
        /// </summary>
        private void CopyToClipboard()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(txtExposure.Text + " by " + txtOutcome.Text);
            sb.AppendLine();
            sb.AppendLine("\t" + outcomeYes.Text + "\t" + outcomeNo.Text + "\tTotal");
            sb.AppendLine(exposureYes.Text + "\t" + txtYesYesVal.Text + "\t" + txtYesNoVal.Text + "\t" + txtYesTotalVal.Text);
            sb.AppendLine(exposureNo.Text + "\t" + txtNoYesVal.Text + "\t" + txtNoNoVal.Text + "\t" + txtNoTotalVal.Text);
            sb.AppendLine("Total\t" + txtTotalYesVal.Text + "\t" + txtTotalNoVal.Text + "\t" + txtTotalTotalVal.Text);
            sb.AppendLine();

            sb.AppendLine("(Exposure = Rows; Outcome = Columns)");

            Clipboard.Clear();
            Clipboard.SetText(sb.ToString());
        }

        private void ResetGrid()
        {
            txtNoNoCol.Text = "0";
            txtNoNoRow.Text = "0";
            txtNoNoVal.Text = "0";

            txtNoTotalCol.Text = "0";
            txtNoTotalRow.Text = (1).ToString("P");
            txtNoTotalVal.Text = "0";

            txtNoYesCol.Text = "0";
            txtNoYesRow.Text = "0";
            txtNoYesVal.Text = "0";

            txtTotalNoCol.Text = (1).ToString("P");
            txtTotalNoRow.Text = "0";
            txtTotalNoVal.Text = "0";

            txtTotalTotalCol.Text = (1).ToString("P");
            txtTotalTotalRow.Text = (1).ToString("P");
            txtTotalTotalVal.Text = "0";

            txtTotalYesCol.Text = (1).ToString("P");
            txtTotalYesRow.Text = "0";
            txtTotalYesVal.Text = "0";

            txtYesNoCol.Text = "0";
            txtYesNoRow.Text = "0";
            txtYesNoVal.Text = "0";

            txtYesTotalCol.Text = "0";
            txtYesTotalRow.Text = (1).ToString("P");
            txtYesTotalVal.Text = "0";

            txtYesYesCol.Text = "0";
            txtYesYesRow.Text = "0";
            txtYesYesVal.Text = "0";

            txtChiSqUncVal.Text = string.Empty;
            txtChiSqUncP.Text = string.Empty;
            txtChiSqManVal.Text = string.Empty;
            txtChiSqManP.Text = string.Empty;
            txtChiSqCorVal.Text = string.Empty;
            txtChiSqCorP.Text = string.Empty;
            
            txtMidPExact.Text = string.Empty;
            txtFisherExact.Text = string.Empty;
            
            txtRiskRatioEstimate.Text = string.Empty;
            txtRiskRatioLower.Text = string.Empty;
            txtRiskRatioUpper.Text = string.Empty;

            txtRiskDifferenceEstimate.Text = string.Empty;
            txtRiskDifferenceLower.Text = string.Empty;
            txtRiskDifferenceUpper.Text = string.Empty;

            txtOddsRatioEstimate.Text = string.Empty;
            txtOddsRatioLower.Text = string.Empty;
            txtOddsRatioUpper.Text = string.Empty;

            txtMidPEstimate.Text = string.Empty;
            txtMidPLower.Text = string.Empty;
            txtMidPUpper.Text = string.Empty;
            txtFisherEstimate.Text = string.Empty;
            txtFisherLower.Text = string.Empty;
            txtFisherUpper.Text = string.Empty;

            pnlAdvanced.Visibility = Visibility.Collapsed;
            pnlContent.Visibility = System.Windows.Visibility.Collapsed;

            dgResults.IsEnabled = true;

            grdTable.Children.Clear();
            grdTable.ColumnDefinitions.Clear();
            grdTable.RowDefinitions.Clear();            
        }

        private void AddFreqGrid(string strataVar, string value, int columnCount)
        {
            Grid grid = grdTable;
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();
            grid.Tag = value;
            //grid.Width = grdFreq.Width;
            grid.HorizontalAlignment = HorizontalAlignment.Center;
            grid.Margin = new Thickness(2, 94, 2, 2);
            grid.Visibility = System.Windows.Visibility.Collapsed;

            for (int i = 0; i < columnCount; i++)
            {
                if (i > MaxColumns)
                {
                    break;
                }

                ColumnDefinition column = new ColumnDefinition();
                column.Width = GridLength.Auto;
                grid.ColumnDefinitions.Add(column);
            }

            ColumnDefinition totalColumn = new ColumnDefinition();
            totalColumn.Width = GridLength.Auto;
            grid.ColumnDefinitions.Add(totalColumn);

            TextBlock txtGridLabel = new TextBlock();
            txtGridLabel.Text = value;
            txtGridLabel.HorizontalAlignment = HorizontalAlignment.Left;
            txtGridLabel.VerticalAlignment = VerticalAlignment.Bottom;
            txtGridLabel.Margin = new Thickness(2, 42, 2, 2);
            txtGridLabel.FontWeight = FontWeights.Bold;
            if (string.IsNullOrEmpty(strataVar))
            {
                txtGridLabel.Margin = new Thickness(2, 36, 2, 2);
                txtGridLabel.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                txtGridLabel.Margin = new Thickness(2, 42, 2, 2);
            }
            //gridLabelsList.Add(txtGridLabel);
            //panelMain.Children.Add(txtGridLabel);

            //panelMain.Children.Add(grid);
            //strataGridList.Add(grid);
        }

        private void SetGridText(string strataValue, TextBlockConfig textBlockConfig, FontWeight fontWeight)
        {
            Grid grid = new Grid();

            grid = grdTable;

            TextBlock txt = new TextBlock();
            txt.FontWeight = fontWeight;
            txt.Text = textBlockConfig.Text;
            txt.Margin = textBlockConfig.Margin;
            txt.VerticalAlignment = textBlockConfig.VerticalAlignment;
            txt.HorizontalAlignment = textBlockConfig.HorizontalAlignment;
            Grid.SetRow(txt, textBlockConfig.RowNumber);
            Grid.SetColumn(txt, textBlockConfig.ColumnNumber);
            grid.Children.Add(txt);
        }

        private void AddGridRow(string strataValue, int height)
        {
            Grid grid = grdTable;

            waitCursor.Visibility = Visibility.Collapsed;            
            grid.Visibility = Visibility.Visible;            
            RowDefinition rowDef = new RowDefinition();
            rowDef.Height = new GridLength(height);
            grid.RowDefinitions.Add(rowDef);
        }

        private void RenderFrequencyHeader(string strataValue, string freqVar, DataColumnCollection columns)
        {
            Grid grid = grdTable;

            RowDefinition rowDefHeader = new RowDefinition();
            rowDefHeader.Height = new GridLength(30);
            grid.RowDefinitions.Add(rowDefHeader);

            for (int y = 0; y < grid.ColumnDefinitions.Count; y++)
            {
                Rectangle rctHeader = new Rectangle();
                rctHeader.Fill = SystemColors.MenuHighlightBrush;
                Grid.SetRow(rctHeader, 0);
                Grid.SetColumn(rctHeader, y);
                grid.Children.Add(rctHeader);
            }

            TextBlock txtValHeader = new TextBlock();
            txtValHeader.Text = StringLiterals.SPACE + freqVar + StringLiterals.SPACE;
            txtValHeader.VerticalAlignment = VerticalAlignment.Center;
            txtValHeader.HorizontalAlignment = HorizontalAlignment.Center;
            txtValHeader.Margin = new Thickness(2, 0, 2, 0);
            txtValHeader.FontWeight = FontWeights.Bold;
            txtValHeader.Foreground = Brushes.White;
            Grid.SetRow(txtValHeader, 0);
            Grid.SetColumn(txtValHeader, 0);
            grid.Children.Add(txtValHeader);

            int maxColumnLength = MaxColumnLength;

            for (int i = 1; i < columns.Count; i++)
            {
                if (i > MaxColumns)
                {
                    break;
                }
                TextBlock txtColHeader = new TextBlock();
                string columnName = columns[i].ColumnName.Trim();

                if (columnName.Length > maxColumnLength)
                {
                    columnName = columnName.Substring(0, maxColumnLength);
                }

                txtColHeader.Text = StringLiterals.SPACE + columnName + StringLiterals.SPACE;
                txtColHeader.VerticalAlignment = VerticalAlignment.Center;
                txtColHeader.HorizontalAlignment = HorizontalAlignment.Center;
                txtColHeader.Margin = new Thickness(2, 0, 2, 0);
                txtColHeader.FontWeight = FontWeights.Bold;
                txtColHeader.Foreground = Brushes.White;
                Grid.SetRow(txtColHeader, 0);
                Grid.SetColumn(txtColHeader, i);
                grid.Children.Add(txtColHeader);
            }

            TextBlock txtRowTotalHeader = new TextBlock();
            txtRowTotalHeader.Text = StringLiterals.SPACE + SharedStrings.TOTAL + StringLiterals.SPACE;
            txtRowTotalHeader.VerticalAlignment = VerticalAlignment.Center;
            txtRowTotalHeader.HorizontalAlignment = HorizontalAlignment.Center;
            txtRowTotalHeader.Margin = new Thickness(2, 0, 2, 0);
            txtRowTotalHeader.FontWeight = FontWeights.Bold;
            txtRowTotalHeader.Foreground = Brushes.White;
            Grid.SetRow(txtRowTotalHeader, 0);
            Grid.SetColumn(txtRowTotalHeader, MaxColumns + 1);
            grid.Children.Add(txtRowTotalHeader);
        }

        private void RenderFrequencyFooter(string strataValue, int footerRowIndex, int[] totalRows)
        {
            Grid grid = grdTable;

            RowDefinition rowDefTotals = new RowDefinition();
            rowDefTotals.Height = new GridLength(30);
            grid.RowDefinitions.Add(rowDefTotals);

            TextBlock txtValTotals = new TextBlock();
            txtValTotals.Text = StringLiterals.SPACE + SharedStrings.TOTAL + StringLiterals.SPACE;
            txtValTotals.Margin = new Thickness(2, 0, 2, 0);
            txtValTotals.VerticalAlignment = VerticalAlignment.Center;
            txtValTotals.FontWeight = FontWeights.Bold;
            Grid.SetRow(txtValTotals, footerRowIndex);
            Grid.SetColumn(txtValTotals, 0);
            grid.Children.Add(txtValTotals);

            for (int i = 0; i < totalRows.Length; i++)
            {
                if (i >= MaxColumns)
                {
                    break;
                }
                TextBlock txtFreqTotals = new TextBlock();
                txtFreqTotals.Text = StringLiterals.SPACE + totalRows[i].ToString() + StringLiterals.SPACE;
                txtFreqTotals.Margin = new Thickness(2, 0, 2, 0);
                txtFreqTotals.VerticalAlignment = VerticalAlignment.Center;
                txtFreqTotals.HorizontalAlignment = HorizontalAlignment.Right;
                txtFreqTotals.FontWeight = FontWeights.Bold;
                Grid.SetRow(txtFreqTotals, footerRowIndex);
                Grid.SetColumn(txtFreqTotals, i + 1);
                grid.Children.Add(txtFreqTotals);
            }

            int sumTotal = 0;
            foreach (int n in totalRows)
            {
                sumTotal = sumTotal + n;
            }

            TextBlock txtOverallTotal = new TextBlock();
            txtOverallTotal.Text = StringLiterals.SPACE + sumTotal.ToString() + StringLiterals.SPACE;
            txtOverallTotal.Margin = new Thickness(2, 0, 2, 0);
            txtOverallTotal.VerticalAlignment = VerticalAlignment.Center;
            txtOverallTotal.HorizontalAlignment = HorizontalAlignment.Right;
            txtOverallTotal.FontWeight = FontWeights.Bold;
            Grid.SetRow(txtOverallTotal, footerRowIndex);
            Grid.SetColumn(txtOverallTotal, MaxColumns + 1);
            grid.Children.Add(txtOverallTotal);
        }

        private void DrawFrequencyBorders(string strataValue)
        {
            Grid grid = grdTable;

            waitCursor.Visibility = Visibility.Collapsed;
            int rdcount = 0;
            foreach (RowDefinition rd in grid.RowDefinitions)
            {
                int cdcount = 0;
                foreach (ColumnDefinition cd in grid.ColumnDefinitions)
                {
                    Rectangle rctBorder = new Rectangle();
                    rctBorder.Stroke = Brushes.Black;
                    Grid.SetRow(rctBorder, rdcount);
                    Grid.SetColumn(rctBorder, cdcount);
                    grid.Children.Add(rctBorder);
                    cdcount++;
                }
                rdcount++;
            }
        }

        #region IGadget Members

        /// <summary>
        /// Sets the gadget to its 'processing' state
        /// </summary>
        public void SetGadgetToProcessingState()
        {
            this.IsProcessing = true;
            this.cbxExposureField.IsEnabled = false;
            this.cbxOutcomeField.IsEnabled = false;
            this.checkboxSmartTable.IsEnabled = false;
            this.dgResults.IsEnabled = false;
            this.dgResults.Visibility = System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// Sets the gadget to its 'finished' state
        /// </summary>
        public void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
            this.cbxExposureField.IsEnabled = true;
            this.cbxOutcomeField.IsEnabled = true;
            this.checkboxSmartTable.IsEnabled = true;
            
            if (isGrouped)
            {
                this.dgResults.IsEnabled = true;
                this.dgResults.Visibility = System.Windows.Visibility.Visible;
            }

            if (GadgetProcessingFinished != null)
                GadgetProcessingFinished(this);
        }

        /// <summary>
        /// Gets/sets whether the gadget is processing
        /// </summary>
        public bool IsProcessing
        {
            get
            {
                return isProcessing;
            }
            set
            {
                isProcessing = value;
            }
        }

        private void CreateInputVariableList()
        {
            Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

            gadgetOptions.MainVariableName = string.Empty;
            gadgetOptions.WeightVariableName = string.Empty;
            gadgetOptions.StrataVariableNames = new List<string>();
            gadgetOptions.CrosstabVariableName = string.Empty;
            gadgetOptions.InputVariableList = new Dictionary<string, string>();

            if (checkboxSmartTable.IsChecked == true)
            {
                inputVariableList.Add("smarttable", "true");
            }
            else
            {
                inputVariableList.Add("smarttable", "false");
            }

            if (!(cbxExposureField.SelectedItem is GroupField || dashboardHelper.GetAllGroupsAsList().Contains(cbxExposureField.SelectedItem.ToString())) && gadgetOptions != null)
            {
                if (cbxExposureField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxExposureField.SelectedItem.ToString()))
                {
                    gadgetOptions.MainVariableName = cbxExposureField.SelectedItem.ToString();
                }
                else
                {
                    return;
                }

                if (cbxOutcomeField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxOutcomeField.SelectedItem.ToString()))
                {
                    gadgetOptions.CrosstabVariableName = cbxOutcomeField.SelectedItem.ToString();
                }
            }
            else if ((cbxExposureField.SelectedItem is GroupField) && gadgetOptions != null)
            {
                if (cbxExposureField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxExposureField.SelectedItem.ToString()))
                {
                    string[] fields = ((GroupField)cbxExposureField.SelectedItem).ChildFieldNames.Split(',');                    
                    foreach (string field in fields)
                    {
                        if ((View.Fields[field] is YesNoField) || (View.Fields[field] is CheckBoxField))
                        {
                            inputVariableList.Add(field, "groupfield");
                        }
                    }
                }
                else
                {
                    return;
                }

                if (cbxOutcomeField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxOutcomeField.SelectedItem.ToString()))
                {
                    gadgetOptions.CrosstabVariableName = cbxOutcomeField.SelectedItem.ToString();
                }                
            }
            else if (dashboardHelper.GetAllGroupsAsList().Contains(cbxExposureField.SelectedItem.ToString()) && gadgetOptions != null)
            {
                if (cbxExposureField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxExposureField.SelectedItem.ToString()))
                {
                    List<string> fields = dashboardHelper.GetGroupVariable(cbxExposureField.SelectedItem.ToString()).Variables;
                    foreach (string field in fields)
                    {
                        if(dashboardHelper.GetFieldsAsList(ColumnDataType.Boolean).Contains(field)) 
                        {
                            inputVariableList.Add(field, "groupfield");
                        }
                    }
                }
                else
                {
                    return;
                }

                if (cbxOutcomeField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxOutcomeField.SelectedItem.ToString()))
                {
                    gadgetOptions.CrosstabVariableName = cbxOutcomeField.SelectedItem.ToString();
                }
            }

            gadgetOptions.ShouldIncludeFullSummaryStatistics = false;
            gadgetOptions.InputVariableList = inputVariableList;
        }

        /// <summary>
        /// Updates the variable names available in the gadget's properties
        /// </summary>
        public void UpdateVariableNames()
        {
            FillComboboxes(true);
        }

        public void RefreshResults()
        {
            if (!loadingCombos && cbxExposureField.SelectedIndex > -1 && cbxOutcomeField.SelectedIndex > -1)
            {
                pnlStatus.Background = Brushes.PaleGreen;
                pnlStatusTop.Background = Brushes.Green;

                if (!(cbxExposureField.SelectedItem is GroupField || dashboardHelper.GetAllGroupsAsList().Contains(cbxExposureField.SelectedItem.ToString())) && gadgetOptions != null)
                {                    
                    CreateInputVariableList();
                    isGrouped = false;

                    txtOutcome.Text = cbxOutcomeField.SelectedItem.ToString();
                    txtExposure.Text = cbxExposureField.SelectedItem.ToString();

                    waitCursor.Visibility = Visibility.Visible;
                    baseWorker = new BackgroundWorker();
                    baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
                    baseWorker.RunWorkerAsync();
                }
                else
                {
                    ResetGrid();
                    CreateInputVariableList();
                    isGrouped = true;
                    txtOutcome.Text = gadgetOptions.CrosstabVariableName;
                    //string[] fields = ((GroupField)cbxExposureField.SelectedItem).ChildFieldNames.Split(',');
                    //List<string> binaryFields = new List<string>();
                    //foreach (string field in fields)
                    //{
                    //    if ((View.Fields[field] is YesNoField) || (View.Fields[field] is CheckBoxField))
                    //    {
                    //        binaryFields.Add(field);
                    //    }
                    //}
                    //if (binaryFields.Count > 0)
                    //{
                        //Execute(binaryFields, cbxOutcomeField.SelectedItem.ToString());
                    //}

                    waitCursor.Visibility = Visibility.Visible;
                    baseWorker = new BackgroundWorker();
                    baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(ExecuteGroup);
                    baseWorker.RunWorkerAsync();
                }
            }
            else if (!loadingCombos && (cbxExposureField.SelectedIndex == -1 || cbxOutcomeField.SelectedIndex == -1))
            {
                ClearGroupResults();
                ClearSingleResults();
                waitCursor.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Generates Xml representation of this gadget
        /// </summary>
        /// <param name="doc">The Xml docment</param>
        /// <returns>XmlNode</returns>
        public XmlNode Serialize(XmlDocument doc)
        {
            string exposure = string.Empty;
            string outcome = string.Empty;

            if (cbxExposureField.SelectedIndex > -1) 
            {
                exposure = cbxExposureField.SelectedItem.ToString().Replace("<", "&lt;");
            }
            
            if(cbxOutcomeField.SelectedIndex > -1) 
            {
                outcome = cbxOutcomeField.SelectedItem.ToString().Replace("<", "&lt;");
            }

            string xmlString =
            "<exposureVariable>" + exposure + "</exposureVariable>" +
            "<outcomeVariable>" + outcome + "</outcomeVariable>" +
            "<customHeading>" + CustomOutputHeading + "</customHeading>" +
            "<customDescription>" + CustomOutputDescription + "</customDescription>" +
            "<customCaption>" + CustomOutputCaption + "</customCaption>" +
            "<smarttable>" + ((bool)checkboxSmartTable.IsChecked) + "</smarttable>";

            System.Xml.XmlElement element = doc.CreateElement("twoByTwoTableGadget");
            element.InnerXml = xmlString;

            System.Xml.XmlAttribute locationY = doc.CreateAttribute("top");
            System.Xml.XmlAttribute locationX = doc.CreateAttribute("left");
            System.Xml.XmlAttribute collapsed = doc.CreateAttribute("collapsed");
            System.Xml.XmlAttribute type = doc.CreateAttribute("gadgetType");

            locationY.Value = Canvas.GetTop(this).ToString("F0");
            locationX.Value = Canvas.GetLeft(this).ToString("F0");
            collapsed.Value = "false"; // currently no way to collapse the gadget, so leave this 'false' for now
            type.Value = "EpiDashboard.TwoByTwoTableControl";

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);

            return element;
        }

        /// <summary>
        /// Creates the 2x2 table gadget from an Xml element
        /// </summary>
        /// <param name="element">The element from which to create the gadget</param>
        public void CreateFromXml(XmlElement element)
        {
            this.loadingCombos = true;

            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLower())
                {
                    case "exposurevariable":
                        cbxExposureField.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "outcomevariable":
                        cbxOutcomeField.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "customheading":
                        this.CustomOutputHeading = child.InnerText;
                        break;
                    case "customdescription":
                        this.CustomOutputDescription = child.InnerText;
                        break;
                    case "customcaption":
                        this.CustomOutputCaption = child.InnerText;
                        break;
                    case "smarttable":
                        if (child.InnerText.ToLower().Equals("true"))
                        {
                            checkboxSmartTable.IsChecked = true;
                        }
                        else
                        {
                            checkboxSmartTable.IsChecked = false;
                        }
                        break;
                }
            }

            foreach (XmlAttribute attribute in element.Attributes)
            {
                switch (attribute.Name.ToLower())
                {
                    case "top":
                        Canvas.SetTop(this, double.Parse(attribute.Value));
                        break;
                    case "left":
                        Canvas.SetLeft(this, double.Parse(attribute.Value));
                        break;
                }
            }

            this.loadingCombos = false;
            
            RefreshResults();
            CollapseExpandConfigPanel();
        }

        public void TurnOffPercents()
        {
            txtYesYesRow.Visibility = System.Windows.Visibility.Collapsed;
            txtYesYesCol.Visibility = System.Windows.Visibility.Collapsed;

            txtYesNoRow.Visibility = System.Windows.Visibility.Collapsed;
            txtYesNoCol.Visibility = System.Windows.Visibility.Collapsed;

            txtNoYesRow.Visibility = System.Windows.Visibility.Collapsed;
            txtNoYesCol.Visibility = System.Windows.Visibility.Collapsed;

            txtNoNoRow.Visibility = System.Windows.Visibility.Collapsed;
            txtNoNoCol.Visibility = System.Windows.Visibility.Collapsed;

            txtTotalYesRow.Visibility = System.Windows.Visibility.Collapsed;
            txtTotalYesCol.Visibility = System.Windows.Visibility.Collapsed;

            txtTotalNoRow.Visibility = System.Windows.Visibility.Collapsed;
            txtTotalNoCol.Visibility = System.Windows.Visibility.Collapsed;

            txtTotalNoRow.Visibility = System.Windows.Visibility.Collapsed;
            txtTotalNoCol.Visibility = System.Windows.Visibility.Collapsed;

            txtNoTotalRow.Visibility = System.Windows.Visibility.Collapsed;
            txtNoTotalCol.Visibility = System.Windows.Visibility.Collapsed;

            txtYesTotalRow.Visibility = System.Windows.Visibility.Collapsed;
            txtYesTotalCol.Visibility = System.Windows.Visibility.Collapsed;

            txtTotalTotalRow.Visibility = System.Windows.Visibility.Collapsed;
            txtTotalTotalCol.Visibility = System.Windows.Visibility.Collapsed;

            tblockYesRowPercent.Visibility = System.Windows.Visibility.Collapsed;
            tblockYesColPercent.Visibility = System.Windows.Visibility.Collapsed;

            tblockNoRowPercent.Visibility = System.Windows.Visibility.Collapsed;
            tblockNoColPercent.Visibility = System.Windows.Visibility.Collapsed;

            tblockTotalRowPercent.Visibility = System.Windows.Visibility.Collapsed;
            tblockTotalColPercent.Visibility = System.Windows.Visibility.Collapsed;
        }

        public void TurnOnPercents()
        {
            txtYesYesRow.Visibility = System.Windows.Visibility.Visible;
            txtYesYesCol.Visibility = System.Windows.Visibility.Visible;

            txtYesNoRow.Visibility = System.Windows.Visibility.Visible;
            txtYesNoCol.Visibility = System.Windows.Visibility.Visible;

            txtNoYesRow.Visibility = System.Windows.Visibility.Visible;
            txtNoYesCol.Visibility = System.Windows.Visibility.Visible;

            txtNoNoRow.Visibility = System.Windows.Visibility.Visible;
            txtNoNoCol.Visibility = System.Windows.Visibility.Visible;

            txtTotalYesRow.Visibility = System.Windows.Visibility.Visible;
            txtTotalYesCol.Visibility = System.Windows.Visibility.Visible;

            txtTotalNoRow.Visibility = System.Windows.Visibility.Visible;
            txtTotalNoCol.Visibility = System.Windows.Visibility.Visible;

            txtTotalNoRow.Visibility = System.Windows.Visibility.Visible;
            txtTotalNoCol.Visibility = System.Windows.Visibility.Visible;

            txtNoTotalRow.Visibility = System.Windows.Visibility.Visible;
            txtNoTotalCol.Visibility = System.Windows.Visibility.Visible;

            txtYesTotalRow.Visibility = System.Windows.Visibility.Visible;
            txtYesTotalCol.Visibility = System.Windows.Visibility.Visible;

            txtTotalTotalRow.Visibility = System.Windows.Visibility.Visible;
            txtTotalTotalCol.Visibility = System.Windows.Visibility.Visible;

            tblockYesRowPercent.Visibility = System.Windows.Visibility.Visible;
            tblockYesColPercent.Visibility = System.Windows.Visibility.Visible;

            tblockNoRowPercent.Visibility = System.Windows.Visibility.Visible;
            tblockNoColPercent.Visibility = System.Windows.Visibility.Visible;

            tblockTotalRowPercent.Visibility = System.Windows.Visibility.Visible;
            tblockTotalColPercent.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public string ToHTML(string htmlFileName = "", int count = 0)
        {
            StringBuilder htmlBuilder = new StringBuilder();

            if (Is2x2)
            {
                if (string.IsNullOrEmpty(CustomOutputHeading))
                {
                    htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">2x2 Table</h2>");
                }
                else if (CustomOutputHeading != "(none)")
                {
                    htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">" + CustomOutputHeading + "</h2>");
                }

                htmlBuilder.AppendLine("<p class=\"gadgetOptions\"><small>");
                htmlBuilder.AppendLine("<em>Exposure variable:</em> <strong>" + cbxExposureField.Text + "</strong>");
                htmlBuilder.AppendLine("<br />");

                if (cbxOutcomeField.SelectedIndex >= 0)
                {
                    htmlBuilder.AppendLine("<em>Crosstab variable:</em> <strong>" + cbxOutcomeField.Text + "</strong>");
                    htmlBuilder.AppendLine("<br />");
                }
                
                htmlBuilder.AppendLine("<em>Show percents:</em> <strong>" + checkboxShowPercents.IsChecked.ToString() + "</strong>");
                htmlBuilder.AppendLine("<br />");

                htmlBuilder.AppendLine("</small></p>");

                if (!string.IsNullOrEmpty(txtStatus.Text) && pnlStatus.Visibility == Visibility.Visible)
                {
                    htmlBuilder.AppendLine("<p><small><strong>" + txtStatus.Text + "</strong></small></p>");
                }

                if (!string.IsNullOrEmpty(CustomOutputDescription))
                {
                    htmlBuilder.AppendLine("<p class=\"gadgetsummary\">" + CustomOutputDescription + "</p>");
                }

                if (dgResults.Visibility == System.Windows.Visibility.Visible)
                {
                    htmlBuilder.AppendLine("<table align=\"left\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
                    // Show Relative Risk Chart

                    htmlBuilder.AppendLine(" <tr>");
                    for (int i = 0; i < ((DataView)dgResults.ItemsSource).Table.Columns.Count; i++)
                    {
                        htmlBuilder.AppendLine("  <th>" + ((DataView)dgResults.ItemsSource).Table.Columns[i].ColumnName + "</th>");
                    }
                    htmlBuilder.AppendLine(" </tr>");

                    foreach (DataRow row in ((DataView)dgResults.ItemsSource).Table.Rows)
                    {
                        htmlBuilder.AppendLine(" <tr>");
                        for (int i = 0; i < ((DataView)dgResults.ItemsSource).Table.Columns.Count; i++)
                        {
                            htmlBuilder.AppendLine("  <td>" + row[i].ToString() + "</td>");
                        }

                        htmlBuilder.AppendLine(" </tr>");
                    }
                    htmlBuilder.AppendLine("</table>");
                    htmlBuilder.AppendLine("<br style=\"clear: both\">");
                }
                else
                {
                    htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                    htmlBuilder.AppendLine("<table align=\"left\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
                    //htmlBuilder.AppendLine("<caption>" + gridName + "</caption>");

                    htmlBuilder.AppendLine(" <tr>");
                    htmlBuilder.AppendLine("  <td></td>");
                    htmlBuilder.AppendLine("  <th colspan=\"2\">" + cbxOutcomeField.Text + "</th>");
                    htmlBuilder.AppendLine("  <td></td>");
                    htmlBuilder.AppendLine(" </tr>");

                    htmlBuilder.AppendLine(" <tr>");
                    htmlBuilder.AppendLine("  <th>" + txtExposure.Text + "</th>");
                    htmlBuilder.AppendLine("  <th>" + outcomeYes.Text + "</th>");
                    htmlBuilder.AppendLine("  <th>" + outcomeNo.Text + "</th>");
                    htmlBuilder.AppendLine("  <th>Total</th>");
                    htmlBuilder.AppendLine(" </tr>");

                    if (checkboxShowPercents.IsChecked == false)
                    {
                        htmlBuilder.AppendLine(" <tr>");
                        htmlBuilder.AppendLine("  <th>" + exposureYes.Text + "</th>");
                        htmlBuilder.AppendLine("  <td>" + currentSingleTableData.yyVal + "<br/><br/></td>");
                        htmlBuilder.AppendLine("  <td>" + currentSingleTableData.ynVal + "<br/><br/></td>");
                        htmlBuilder.AppendLine("  <td>" + currentSingleTableData.ytVal + "<br/><br/></td>");
                        htmlBuilder.AppendLine(" </tr>");

                        htmlBuilder.AppendLine(" <tr>");
                        htmlBuilder.AppendLine("  <th>" + exposureNo.Text + "</th>");
                        htmlBuilder.AppendLine("  <td>" + currentSingleTableData.nyVal + "<br/><br/></td>");
                        htmlBuilder.AppendLine("  <td>" + currentSingleTableData.nnVal + "<br/><br/></td>");
                        htmlBuilder.AppendLine("  <td>" + currentSingleTableData.ntVal + "<br/><br/></td>");
                        htmlBuilder.AppendLine(" </tr>");

                        htmlBuilder.AppendLine(" <tr>");
                        htmlBuilder.AppendLine("  <th>Total</th>");
                        htmlBuilder.AppendLine("  <td>" + currentSingleTableData.tyVal + "<br/><br/></td>");
                        htmlBuilder.AppendLine("  <td>" + currentSingleTableData.tnVal + "<br/><br/></td>");
                        htmlBuilder.AppendLine("  <td>" + currentSingleTableData.ttVal + "<br/><br/></td>");
                        htmlBuilder.AppendLine(" </tr>");
                    }
                    else
                    {
                        htmlBuilder.AppendLine(" <tr>");
                        htmlBuilder.AppendLine("  <th>" + exposureYes.Text + "<br/><small>Row %<br/>Col %</small></th>");
                        htmlBuilder.AppendLine("  <td>" + currentSingleTableData.yyVal + "<br/><small>" + currentSingleTableData.yyRowPct.ToString("P") + "<br/>" + currentSingleTableData.yyColPct.ToString("P") + "</small></td>");
                        htmlBuilder.AppendLine("  <td>" + currentSingleTableData.ynVal + "<br/><small>" + currentSingleTableData.ynRowPct.ToString("P") + "<br/>" + currentSingleTableData.ynColPct.ToString("P") + "</small></td>");
                        htmlBuilder.AppendLine("  <td>" + currentSingleTableData.ytVal + "<br/><small>" + 1.ToString("P") + "<br/>" + currentSingleTableData.ytColPct.ToString("P") + "</small></td>");
                        htmlBuilder.AppendLine(" </tr>");

                        htmlBuilder.AppendLine(" <tr>");
                        htmlBuilder.AppendLine("  <th>" + exposureNo.Text + "<br/><small>Row %<br/>Col %</small></th>");
                        htmlBuilder.AppendLine("  <td>" + currentSingleTableData.nyVal + "<br/><small>" + currentSingleTableData.nyRowPct.ToString("P") + "<br/>" + currentSingleTableData.nyColPct.ToString("P") + "</small></td>");
                        htmlBuilder.AppendLine("  <td>" + currentSingleTableData.nnVal + "<br/><small>" + currentSingleTableData.nnRowPct.ToString("P") + "<br/>" + currentSingleTableData.nnColPct.ToString("P") + "</small></td>");
                        htmlBuilder.AppendLine("  <td>" + currentSingleTableData.ntVal + "<br/><small>" + 1.ToString("P") + "<br/>" + currentSingleTableData.ntColPct.ToString("P") + "</small></td>");
                        htmlBuilder.AppendLine(" </tr>");

                        htmlBuilder.AppendLine(" <tr>");
                        htmlBuilder.AppendLine("  <th>Total<br/><small>Row %<br/>Col %</small></th>");
                        htmlBuilder.AppendLine("  <td>" + currentSingleTableData.tyVal + "<br/><small>" + currentSingleTableData.tyRowPct.ToString("P") + "<br/>" + 1.ToString("P") + "</small></td>");
                        htmlBuilder.AppendLine("  <td>" + currentSingleTableData.tnVal + "<br/><small>" + currentSingleTableData.tnRowPct.ToString("P") + "<br/>" + 1.ToString("P") + "</small></td>");
                        htmlBuilder.AppendLine("  <td>" + currentSingleTableData.ttVal + "<br/><small>" + 1.ToString("P") + "<br/>" + 1.ToString("P") + "</small></td>");
                        htmlBuilder.AppendLine(" </tr>");
                    }

                    htmlBuilder.AppendLine("</table>");

                    int redValue = (int)(((double)currentSingleTableData.yyVal / (double)currentSingleTableData.ttVal) * 200);
                    int orangeValue = (int)(((double)currentSingleTableData.ynVal / (double)currentSingleTableData.ttVal) * 200);
                    int yellowValue = (int)(((double)currentSingleTableData.nyVal / (double)currentSingleTableData.ttVal) * 200);
                    int greenValue = (int)(((double)currentSingleTableData.nnVal / (double)currentSingleTableData.ttVal) * 200);

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
                    string fisherExact2 = Epi.SharedStrings.UNDEFINED;
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

                    if (currentSingleTableData.singleTableResults.FisherExactP != -1)
                    {
                        fisherExact = ((double)currentSingleTableData.singleTableResults.FisherExactP).ToString("F10");
                    }

                    if (currentSingleTableData.singleTableResults.FisherExact2P != -1)
                    {
                        fisherExact2 = ((double)currentSingleTableData.singleTableResults.FisherExact2P).ToString("F10");
                    }

                    if (currentSingleTableData.singleTableResults.OddsRatioMLEFisherLower != -1)
                    {
                        fisherLower = ((double)currentSingleTableData.singleTableResults.OddsRatioMLEFisherLower).ToString("F4");
                    }

                    if (currentSingleTableData.singleTableResults.OddsRatioMLEFisherUpper != -1)
                    {
                        fisherUpper = ((double)currentSingleTableData.singleTableResults.OddsRatioMLEFisherUpper).ToString("F4");
                    }

                    if (currentSingleTableData.singleTableResults.OddsRatioMLEEstimate != -1)
                    {
                        oddsRatioMLEEstimate = ((double)currentSingleTableData.singleTableResults.OddsRatioMLEEstimate).ToString("F4");
                    }

                    if (currentSingleTableData.singleTableResults.OddsRatioMLEMidPLower != -1)
                    {
                        oddsRatioMLEMidPLower = ((double)currentSingleTableData.singleTableResults.OddsRatioMLEMidPLower).ToString("F4");
                    }

                    if (currentSingleTableData.singleTableResults.OddsRatioMLEMidPUpper != -1)
                    {
                        oddsRatioMLEMidPUpper = ((double)currentSingleTableData.singleTableResults.OddsRatioMLEMidPUpper).ToString("F4");
                    }

                    if (currentSingleTableData.singleTableResults.OddsRatioEstimate != null)
                    {
                        oddsRatioEstimate = ((double)currentSingleTableData.singleTableResults.OddsRatioEstimate).ToString("F4");
                    }

                    if (currentSingleTableData.singleTableResults.OddsRatioLower != null)
                    {
                        oddsRatioLower = ((double)currentSingleTableData.singleTableResults.OddsRatioLower).ToString("F4");
                    }

                    if (currentSingleTableData.singleTableResults.OddsRatioUpper != null)
                    {
                        oddsRatioUpper = ((double)currentSingleTableData.singleTableResults.OddsRatioUpper).ToString("F4");
                    }

                    if (currentSingleTableData.singleTableResults.RiskRatioEstimate != null)
                    {
                        riskRatioEstimate = ((double)currentSingleTableData.singleTableResults.RiskRatioEstimate).ToString("F4");
                    }

                    if (currentSingleTableData.singleTableResults.RiskRatioLower != null)
                    {
                        riskRatioLower = ((double)currentSingleTableData.singleTableResults.RiskRatioLower).ToString("F4");
                    }

                    if (currentSingleTableData.singleTableResults.RiskRatioUpper != null)
                    {
                        riskRatioUpper = ((double)currentSingleTableData.singleTableResults.RiskRatioUpper).ToString("F4");
                    }


                    string chiSqCorP = currentSingleTableData.singleTableResults.ChiSquareYates2P.ToString();
                    string chiSqCorVal = currentSingleTableData.singleTableResults.ChiSquareYatesVal.ToString("F4");
                    string chiSqManP = currentSingleTableData.singleTableResults.ChiSquareMantel2P.ToString();
                    string chiSqManVal = currentSingleTableData.singleTableResults.ChiSquareMantelVal.ToString("F4");
                    string chiSqUncP = currentSingleTableData.singleTableResults.ChiSquareUncorrected2P.ToString();
                    string chiSqUncVal = currentSingleTableData.singleTableResults.ChiSquareUncorrectedVal.ToString("F4");
                    string riskDifferenceEstimate = currentSingleTableData.singleTableResults.RiskDifferenceEstimate.ToString("F4");
                    string riskDifferenceLower = currentSingleTableData.singleTableResults.RiskDifferenceLower.ToString("F4");
                    string riskDifferenceUpper = currentSingleTableData.singleTableResults.RiskDifferenceUpper.ToString("F4");
                    //string riskRatioEstimate = currentSingleTableData.singleTableResults.RiskRatioEstimate.ToString("F4");
                    string midPExact = currentSingleTableData.singleTableResults.MidP.ToString("F10");

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
                }
                htmlBuilder.AppendLine("<br clear=\"all\" /><br clear=\"all\" />");
            }
            else
            {
                if (string.IsNullOrEmpty(CustomOutputHeading))
                {
                    htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">2x2 Table</h2>");
                }
                else if (CustomOutputHeading != "(none)")
                {
                    htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">" + CustomOutputHeading + "</h2>");
                }

                htmlBuilder.AppendLine("<p class=\"gadgetOptions\"><small>");
                htmlBuilder.AppendLine("<em>Exposure variable:</em> <strong>" + cbxExposureField.Text + "</strong>");
                htmlBuilder.AppendLine("<br />");

                if (cbxOutcomeField.SelectedIndex >= 0)
                {
                    htmlBuilder.AppendLine("<em>Crosstab variable:</em> <strong>" + cbxOutcomeField.Text + "</strong>");
                    htmlBuilder.AppendLine("<br />");
                }

                htmlBuilder.AppendLine("</small></p>");

                if (!string.IsNullOrEmpty(txtStatus.Text) && pnlStatus.Visibility == Visibility.Visible)
                {
                    htmlBuilder.AppendLine("<p><small><strong>" + txtStatus.Text + "</strong></small></p>");
                }

                if (!string.IsNullOrEmpty(CustomOutputDescription))
                {
                    htmlBuilder.AppendLine("<p class=\"gadgetsummary\">" + CustomOutputDescription + "</p>");
                }

                htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
                htmlBuilder.AppendLine("<caption>" + cbxExposureField.Text + " * " + cbxOutcomeField.Text + "</caption>");

                foreach (UIElement control in grdTable.Children)
                {
                    if (control is TextBlock)
                    {
                        int rowNumber = Grid.GetRow(control);
                        int columnNumber = Grid.GetColumn(control);

                        string tableDataTagOpen = "<td>";
                        string tableDataTagClose = "</td>";

                        if (rowNumber == 0)
                        {
                            tableDataTagOpen = "<th>";
                            tableDataTagClose = "</th>";
                        }

                        if (columnNumber == 0)
                        {
                            htmlBuilder.AppendLine("<tr>");
                        }
                        if (columnNumber == 0 && rowNumber > 0)
                        {
                            tableDataTagOpen = "<td class=\"value\">";
                        }

                        string value = ((TextBlock)control).Text;
                        string formattedValue = value;

                        if ((rowNumber == grdTable.RowDefinitions.Count - 1) || (columnNumber == grdTable.ColumnDefinitions.Count - 1))
                        {
                            formattedValue = "<span class=\"total\">" + value + "</span>";
                        }

                        htmlBuilder.AppendLine(tableDataTagOpen + formattedValue + tableDataTagClose);

                        if (columnNumber >= grdTable.ColumnDefinitions.Count - 1)
                        {
                            htmlBuilder.AppendLine("</tr>");
                        }
                    }
                }
            }

            return htmlBuilder.ToString();
        }

        private string customOutputHeading;
        private string customOutputDescription;
        private string customOutputCaption;

        public string CustomOutputHeading
        {
            get
            {
                return this.customOutputHeading;
            }
            set
            {
                this.customOutputHeading = value;
            }
        }

        public string CustomOutputDescription
        {
            get
            {
                return this.customOutputDescription;
            }
            set
            {
                this.customOutputDescription = value;
            }
        }

        public string CustomOutputCaption
        {
            get
            {
                return this.customOutputCaption;
            }
            set
            {
                this.customOutputCaption = value;
            }
        }
        #endregion

        private View View
        {
            get
            {
                return this.dashboardHelper.View;
            }
        }

        private IDbDriver Database
        {
            get
            {
                return this.dashboardHelper.Database;
            }
        }

        private int MaxRows
        {
            get
            {
                return 200;
                //int maxRows = 200;
                //bool success = int.TryParse(txtMaxRows.Text, out maxRows);
                //if (!success)
                //{
                //    return 200;
                //}
                //else
                //{
                //    return maxRows;
                //}
            }
        }

        private int MaxColumns
        {
            get
            {
                return 100;
                //int maxColumns = 50;
                //bool success = int.TryParse(txtMaxColumns.Text, out maxColumns);
                //if (!success)
                //{
                //    return 50;
                //}
                //else
                //{
                //    return maxColumns;
                //}
            }
        }

        private int MaxColumnLength
        {
            get
            {
                return 32;
                //int maxColumnLength = 24;
                //bool success = int.TryParse(txtMaxColumnLength.Text, out maxColumnLength);
                //if (!success)
                //{
                //    return 24;
                //}
                //else
                //{
                //    return maxColumnLength;
                //}
            }
        }

        public bool DrawBorders { get; set; } 
    }
}
