using System;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
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
using Epi;
using Epi.Data;
using EpiInfo.Plugin;
using Epi.Fields;
using Epi.Core;
using EpiDashboard.Rules;

namespace EpiDashboard
{
    /// <summary>
    /// Interaction logic for LinearRegressionControl.xaml
    /// </summary>
    public partial class LinearRegressionControl : GadgetBase
    {
        #region Private Variables

        private struct VariableRow 
        {
            public string variableName;
            public double coefficient;
            public double stdError;
            public double Ftest;
            public double P;
            public double coefficientLower;
            public double coefficientUpper;
        }

        private struct RegressionResults
        {
            public List<VariableRow> variables;
            public double correlationCoefficient;
            public int regressionDf;
            public double regressionSumOfSquares;
            public double regressionMeanSquare;
            public double regressionF;
            public double regressionFp;
            public int residualsDf;
            public double residualsSumOfSquares;
            public double residualsMeanSquare;            
            public int totalDf;
            public double totalSumOfSquares;
            public string errorMessage;
            public StatisticsRepository.LinearRegression.LinearRegressionResults regressionResults;            
        }        

        //private struct TextBlockConfig
        //{
        //    public string Text;
        //    public Thickness Margin;
        //    public VerticalAlignment VerticalAlignment;
        //    public HorizontalAlignment HorizontalAlignment;
        //    public int ColumnNumber;
        //    public int RowNumber;

        //    public TextBlockConfig(string text, Thickness margin, VerticalAlignment verticalAlignment, HorizontalAlignment horizontalAlignment, int rowNumber, int columnNumber)
        //    {
        //        this.Text = text;
        //        this.Margin = margin;
        //        this.VerticalAlignment = verticalAlignment;
        //        this.HorizontalAlignment = horizontalAlignment;
        //        this.RowNumber = rowNumber;
        //        this.ColumnNumber = columnNumber;
        //    }
        //}

        #endregion // Private Variables

        #region Delegates
        private new delegate void SetGridTextDelegate(Grid grid, TextBlockConfig textBlockConfig);
        //private delegate void SetGridBarDelegate(int rowNumber, double pct);
        private new delegate void AddGridRowDelegate(Grid grid, int height);
        private new delegate void AddGridFooterDelegate(int rowNumber, int totalRows);
        private delegate void RenderGridDelegate(RegressionResults regressionResults);

        //private delegate void SetStatusDelegate(string statusMessage);
        //private delegate void RequestUpdateStatusDelegate(string statusMessage);
        //private delegate bool CheckForCancellationDelegate();
        //private delegate void RenderFinishWithErrorDelegate(string errorMessage);
        #endregion // Delegates

        #region Events

        //public event GadgetRefreshedHandler GadgetRefreshed;
        //public event GadgetClosingHandler GadgetClosing;
        //public event GadgetProcessingFinishedHandler GadgetProcessingFinished;
        //public event GadgetStatusUpdateHandler GadgetStatusUpdate;
        //public event GadgetCheckForCancellationHandler GadgetCheckForCancellation;

        #endregion // Events

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public LinearRegressionControl()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">The database to attach</param>
        /// <param name="view">The view to attach</param>
        public LinearRegressionControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            Construct();
            FillComboboxes();
        }

        protected override void Construct()
        {
            if (!string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)"))
            {
                headerPanel.Text = CustomOutputHeading;
            }

            Parameters = new LogisticParameters();

            this.IsProcessing = false;

            this.GadgetStatusUpdate += new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(IsCancelled);
            
            cbxFields.SelectionChanged += new SelectionChangedEventHandler(cbxFields_SelectionChanged);

            mnuSendToBack.Click += new RoutedEventHandler(mnuSendToBack_Click);
            mnuClose.Click += new RoutedEventHandler(mnuClose_Click);

            // Init gadget parameters with default values
            GadgetOptions = new GadgetParameters();
            GadgetOptions.ShouldIncludeFullSummaryStatistics = true;
            GadgetOptions.ShouldIncludeMissing = false;
            GadgetOptions.ShouldSortHighToLow = false;
            GadgetOptions.ShouldUseAllPossibleValues = false;
            GadgetOptions.StrataVariableNames = new List<string>();

            base.Construct();
        }

        void cbxFields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LoadingCombos)
            {
                return;
            }

            if (cbxFields.SelectedIndex > -1)
            {
                if(!lbxOtherFields.Items.Contains(cbxFields.SelectedItem.ToString()))
                {
                    lbxOtherFields.Items.Add(cbxFields.SelectedItem.ToString());
                }
            }
        }

        #endregion

        #region Public Methods

        public override void CollapseOutput()
        {
            grdRegress.Visibility = System.Windows.Visibility.Collapsed;
            txtCorrelation.Visibility = System.Windows.Visibility.Collapsed;
            grdParameters.Visibility = System.Windows.Visibility.Collapsed;

            if (!string.IsNullOrEmpty(this.txtFilterString.Text))
            {
                this.txtFilterString.Visibility = System.Windows.Visibility.Collapsed;
            }

            this.messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            this.txtFilterString.Visibility = System.Windows.Visibility.Collapsed;
            descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
            IsCollapsed = true;
        }

        public override void ExpandOutput()
        {
            grdRegress.Visibility = System.Windows.Visibility.Visible;
            txtCorrelation.Visibility = System.Windows.Visibility.Visible;
            grdParameters.Visibility = System.Windows.Visibility.Visible;

            if (this.messagePanel.MessagePanelType != Controls.MessagePanelType.StatusPanel)
            {
                this.messagePanel.Visibility = System.Windows.Visibility.Visible;
            }

            if (!string.IsNullOrEmpty(this.txtFilterString.Text))
            {
                this.txtFilterString.Visibility = System.Windows.Visibility.Visible;
            }

            if (!string.IsNullOrEmpty(this.descriptionPanel.Text))
            {
                descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.DisplayMode;
            }
            IsCollapsed = false;
        }

        public void ClearResults() 
        {
            grdRegress.Children.Clear();
            grdRegress.RowDefinitions.Clear();

            grdParameters.Visibility = Visibility.Collapsed;
            txtCorrelation.Visibility = Visibility.Collapsed;

            waitPanel.Visibility = System.Windows.Visibility.Visible;
            messagePanel.Text = string.Empty;
            messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;
        }

        #endregion

        #region Event Handlers

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            RefreshResults();
        }

        private void btnMakeDummy_Click(object sender, RoutedEventArgs e)
        {
            if (lbxOtherFields.SelectedItems.Count == 1)
            {
                if (!string.IsNullOrEmpty(lbxOtherFields.SelectedItem.ToString()))
                {
                    string columnName = lbxOtherFields.SelectedItem.ToString();
                    lbxOtherFields.Items.Remove(columnName);
                    lbxDummy.Items.Add(columnName);
                }
            }
            else if (lbxOtherFields.SelectedItems.Count == 2)
            {
                string term = lbxOtherFields.SelectedItems[0] + "*" + lbxOtherFields.SelectedItems[1];
                if (!lbxInteractionTerms.Items.Contains(term))
                {
                    lbxInteractionTerms.Items.Add(term);
                }
            }
        }

        private void lbxOtherFields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbxOtherFields.SelectedItems.Count == 1 || lbxOtherFields.SelectedItems.Count == 2)
            {
                btnMakeDummy.IsEnabled = true;
            }
            else
            {
                btnMakeDummy.IsEnabled = false;
            }

            if (lbxOtherFields.SelectedItems.Count == 1 || lbxOtherFields.SelectedItems.Count > 2)
            {
                btnMakeDummy.Content = "Make Dummy";                
            }
            else if(lbxOtherFields.SelectedItems.Count == 2)
            {
                btnMakeDummy.Content = "Make Interaction";
            }
        }

        protected override void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (syncLock)
            {
                Configuration config = DashboardHelper.Config;
                Dictionary<string, string> setProperties = new Dictionary<string, string>();
                setProperties.Add("Intercept", "true");
                setProperties.Add("P", 0.95.ToString());
                setProperties.Add("BLabels", config.Settings.RepresentationOfYes + ";" + config.Settings.RepresentationOfNo + ";" + config.Settings.RepresentationOfMissing); // TODO: Replace Yes, No, Missing with global vars

                //Dictionary<string, string> inputVariableList = (Dictionary<string, string>)e.Argument;

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));

                SetGridTextDelegate setText = new SetGridTextDelegate(SetGridText);
                AddGridRowDelegate addRow = new AddGridRowDelegate(AddGridRow);
                //SetGridBarDelegate setBar = new SetGridBarDelegate(SetGridBar);

                System.Collections.Generic.Dictionary<string, System.Data.DataTable> Freq_ListSet = new Dictionary<string, System.Data.DataTable>();

                //string customFilter = string.Empty;
                List<string> columnNames = new List<string>();

                foreach (KeyValuePair<string, string> kvp in GadgetOptions.InputVariableList)
                {
                    if (kvp.Value.ToLower().Equals("unsorted") || kvp.Value.ToLower().Equals("dependvar") || kvp.Value.ToLower().Equals("weightvar") || kvp.Value.ToLower().Equals("matchvar"))
                    {
                        columnNames.Add(kvp.Key);
                    }
                    else if (kvp.Value.ToLower().Equals("discrete"))
                    {
                        columnNames.Add(kvp.Key);
                    }
                }

                try
                {
                    lock (staticSyncLock)
                    {
                        RequestUpdateStatusDelegate requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
                        CheckForCancellationDelegate checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

                        GadgetOptions.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                        GadgetOptions.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);

                        DataTable regressTable = DashboardHelper.GenerateTable(columnNames);
                        RegressionResults results = new RegressionResults();

                        if (regressTable == null || regressTable.Rows.Count <= 0)
                        {
                            this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.NO_RECORDS_SELECTED);
                            this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                            Debug.Print("Thread stopped due to errors.");
                            return;
                        }
                        else if (worker.CancellationPending)
                        {
                            this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.DASHBOARD_GADGET_STATUS_OPERATION_CANCELLED);
                            this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                            Debug.Print("Thread cancelled");
                            return;
                        }
                        else
                        {
                            StatisticsRepository.LinearRegression linearRegression = new StatisticsRepository.LinearRegression();

                            results.regressionResults = linearRegression.LinearRegression(GadgetOptions.InputVariableList, regressTable);

                            results.correlationCoefficient = results.regressionResults.correlationCoefficient;
                            results.regressionDf = results.regressionResults.regressionDf;
                            results.regressionF = results.regressionResults.regressionF;
                            results.regressionMeanSquare = results.regressionResults.regressionMeanSquare;
                            results.regressionSumOfSquares = results.regressionResults.regressionSumOfSquares;
                            results.residualsDf = results.regressionResults.residualsDf;
                            results.residualsMeanSquare = results.regressionResults.residualsMeanSquare;
                            results.residualsSumOfSquares = results.regressionResults.residualsSumOfSquares;
                            results.totalDf = results.regressionResults.totalDf;
                            results.totalSumOfSquares = results.regressionResults.totalSumOfSquares;
                            results.errorMessage = results.regressionResults.errorMessage.Replace("<tlt>", string.Empty).Replace("</tlt>", string.Empty);

                            if (!string.IsNullOrEmpty(results.errorMessage))
                            {
                                throw new ApplicationException(results.errorMessage);
                            }

                            results.variables = new List<VariableRow>();

                            if (results.regressionResults.variables != null)
                            {
                                double tScore = 1.9;
                                while (Epi.Statistics.SharedResources.PFromT(tScore, results.residualsDf) > 0.025)
                                {
                                    tScore += 0.000001;
                                }
                                foreach (StatisticsRepository.LinearRegression.VariableRow vrow in results.regressionResults.variables)
                                {
                                    VariableRow nrow = new VariableRow();
                                    nrow.coefficient = vrow.coefficient;
                                    nrow.Ftest = vrow.Ftest;
                                    nrow.P = vrow.P;
                                    nrow.stdError = vrow.stdError;
                                    nrow.variableName = vrow.variableName;
                                    nrow.coefficientLower = nrow.coefficient - tScore * nrow.stdError;
                                    nrow.coefficientUpper = nrow.coefficient + tScore * nrow.stdError;
                                    results.variables.Add(nrow);
                                }

                                results.regressionFp = Epi.Statistics.SharedResources.PFromF(results.regressionF, results.regressionDf, results.residualsDf);

                                this.Dispatcher.BeginInvoke(new SimpleCallback(RenderRegressionHeader));

                                int rowCount = 1;
                                foreach (VariableRow row in results.variables)
                                {
                                    this.Dispatcher.Invoke(addRow, grdRegress, 30);

                                    string displayValue = row.variableName;

                                    this.Dispatcher.BeginInvoke(setText, grdRegress, new TextBlockConfig(StringLiterals.SPACE + displayValue + StringLiterals.SPACE, new Thickness(2, 0, 2, 0), VerticalAlignment.Center, HorizontalAlignment.Left, TextAlignment.Left, rowCount, 0, Visibility.Visible));
                                    this.Dispatcher.BeginInvoke(setText, grdRegress, new TextBlockConfig(StringLiterals.SPACE + row.coefficient.ToString("F3") + StringLiterals.SPACE, new Thickness(2, 0, 2, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 1, Visibility.Visible));
                                    this.Dispatcher.BeginInvoke(setText, grdRegress, new TextBlockConfig(StringLiterals.SPACE + row.coefficientLower.ToString("F3") + StringLiterals.SPACE, new Thickness(2, 0, 2, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 2, Visibility.Visible));
                                    this.Dispatcher.BeginInvoke(setText, grdRegress, new TextBlockConfig(StringLiterals.SPACE + row.coefficientUpper.ToString("F3") + StringLiterals.SPACE, new Thickness(2, 0, 2, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 3, Visibility.Visible));

                                    this.Dispatcher.BeginInvoke(setText, grdRegress, new TextBlockConfig(StringLiterals.SPACE + row.stdError.ToString("F3") + StringLiterals.SPACE, new Thickness(2, 0, 2, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 4, Visibility.Visible));
                                    this.Dispatcher.BeginInvoke(setText, grdRegress, new TextBlockConfig(StringLiterals.SPACE + row.Ftest.ToString("F4") + StringLiterals.SPACE, new Thickness(2, 0, 2, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 5, Visibility.Visible));
                                    this.Dispatcher.BeginInvoke(setText, grdRegress, new TextBlockConfig(StringLiterals.SPACE + row.P.ToString("F6") + StringLiterals.SPACE, new Thickness(2, 0, 2, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 6, Visibility.Visible));

                                    rowCount++;
                                }

                                //this.Dispatcher.BeginInvoke(new AddGridFooterDelegate(RenderFrequencyFooter), rowCount, count);
                                this.Dispatcher.BeginInvoke(new SimpleCallback(DrawRegressionBorders));
                            }
                        }

                        this.Dispatcher.BeginInvoke(new RenderGridDelegate(RenderRegressionResults), results);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
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
                    Debug.Print("Linear regression gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete with " + DashboardHelper.RecordCount.ToString() + " records and the following filters:");
                    Debug.Print(DashboardHelper.DataFilters.GenerateDataFilterString());
                }
            }
            //gridCells.singleTableResults = new StatisticsRepository.cTable().SigTable((double)gridCells.yyVal, (double)gridCells.ynVal, (double)gridCells.nyVal, (double)gridCells.nnVal, 0.95);
        }

        /// <summary>
        /// Handles the WorkerCompleted event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected override void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Debug.Print("Background worker thread for linear regression gadget was cancelled or ran to completion.");
        }

        private void RenderRegressionResults(RegressionResults results)
        {
            txtCorrelation.Visibility = Visibility.Visible;
            //waitCursor.Visibility = Visibility.Collapsed;
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;
            grdParameters.Visibility = Visibility.Visible;
            grdRegress.Visibility = System.Windows.Visibility.Visible;
            btnRun.IsEnabled = true;

            if (!string.IsNullOrEmpty(results.errorMessage))
            {
                txtCorrelation.Text = results.errorMessage;
                Thickness margin = txtCorrelation.Margin;
                margin.Top = 52;
                txtCorrelation.Margin = margin;
                grdParameters.Visibility = System.Windows.Visibility.Hidden;
            }
            else if (results.variables == null)
            {
                txtCorrelation.Text = string.Empty;
                Thickness margin = txtCorrelation.Margin;
                margin.Top = 8;
                txtCorrelation.Margin = margin;
                grdParameters.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                Thickness margin = txtCorrelation.Margin;
                margin.Top = 8;
                txtCorrelation.Margin = margin;
                grdParameters.Visibility = System.Windows.Visibility.Visible;

                txtRegressionDf.Text = StringLiterals.SPACE + results.regressionDf.ToString() + StringLiterals.SPACE;
                txtRegressionSumOfSquares.Text = StringLiterals.SPACE + results.regressionSumOfSquares.ToString("F4") + StringLiterals.SPACE;
                txtRegressionMeanSquare.Text = StringLiterals.SPACE + results.regressionMeanSquare.ToString("F4") + StringLiterals.SPACE;
                txtRegressionFstatistic.Text = StringLiterals.SPACE + results.regressionF.ToString("F4") + StringLiterals.SPACE;
                txtRegressionFstatisticP.Text = StringLiterals.SPACE + results.regressionFp.ToString("F4") + StringLiterals.SPACE;

                txtResidualsDf.Text = StringLiterals.SPACE + results.residualsDf.ToString() + StringLiterals.SPACE;
                txtResidualsSumOfSquares.Text = StringLiterals.SPACE + results.residualsSumOfSquares.ToString("F4") + StringLiterals.SPACE;
                txtResidualsMeanSquare.Text = StringLiterals.SPACE + results.residualsMeanSquare.ToString("F4") + StringLiterals.SPACE;

                txtTotalDf.Text = StringLiterals.SPACE + results.totalDf.ToString() + StringLiterals.SPACE;
                txtTotalSumOfSquares.Text = StringLiterals.SPACE + results.totalSumOfSquares.ToString("F4") + StringLiterals.SPACE;

                txtCorrelation.Text = "Correlation Coefficient: r^2 = " + results.correlationCoefficient.ToString("F2");

                if (results.regressionResults.pearsonCoefficient <= 1.0)
                {
                    grdPearson.Visibility = System.Windows.Visibility.Visible;
                    txtPearsonAnalysis.Visibility = System.Windows.Visibility.Visible;
                    txtPearsonCoefficient.Text = results.regressionResults.pearsonCoefficient.ToString("F4");
                    txtPearsonCoefficientT.Text = results.regressionResults.pearsonCoefficientT.ToString("F4");
                    txtPearsonCoefficientTP.Text = results.regressionResults.pearsonCoefficientTP.ToString("F4");
                    if (results.regressionResults.pearsonCoefficientTP < 0.0001)
                        txtPearsonCoefficientTP.Text = "<0.0001";
                    if (results.regressionResults.spearmanCoefficient <= 1.0)
                    {
                        grdSpearman.Visibility = System.Windows.Visibility.Visible;
                        txtSpearmanAnalysis.Visibility = System.Windows.Visibility.Visible;
                        txtSpearmanCoefficient.Text = results.regressionResults.spearmanCoefficient.ToString("F4");
                        txtSpearmanCoefficientT.Text = results.regressionResults.spearmanCoefficientT.ToString("F4");
                        txtSpearmannCoefficientTP.Text = results.regressionResults.spearmanCoefficientTP.ToString("F4");
                        if (results.regressionResults.spearmanCoefficientTP < 0.0001)
                            txtSpearmannCoefficientTP.Text = "<0.0001";
                    }
                    else
                    {
                        grdSpearman.Visibility = System.Windows.Visibility.Collapsed;
                        txtSpearmanAnalysis.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
                else
                {
                    grdPearson.Visibility = System.Windows.Visibility.Collapsed;
                    txtPearsonAnalysis.Visibility = System.Windows.Visibility.Collapsed;
                    grdSpearman.Visibility = System.Windows.Visibility.Collapsed;
                    txtSpearmanAnalysis.Visibility = System.Windows.Visibility.Collapsed;
                }
            }

            HideConfigPanel();
        }

        private void RenderRegressionHeader()
        {
            RowDefinition rowDefHeader = new RowDefinition();
            rowDefHeader.Height = new GridLength(30);
            grdRegress.RowDefinitions.Add(rowDefHeader);

            for (int y = 0; y < grdRegress.ColumnDefinitions.Count + 2; y++)
            {
                Rectangle rctHeader = new Rectangle();
                rctHeader.Style = this.Resources["gridHeaderCellRectangle"] as Style;
                Grid.SetRow(rctHeader, 0);
                Grid.SetColumn(rctHeader, y);
                grdRegress.Children.Add(rctHeader);
            }

            TextBlock txtVarHeader = new TextBlock();
            txtVarHeader.Text = "Variable";
            txtVarHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtVarHeader, 0);
            Grid.SetColumn(txtVarHeader, 0);
            grdRegress.Children.Add(txtVarHeader);

            TextBlock txtCoefHeader = new TextBlock();
            txtCoefHeader.Text = "Coefficient";
            txtCoefHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtCoefHeader, 0);
            Grid.SetColumn(txtCoefHeader, 1);
            grdRegress.Children.Add(txtCoefHeader);

            TextBlock txtCoefHeaderLL = new TextBlock();
            txtCoefHeaderLL.Text = "95% Confidence";
            txtCoefHeaderLL.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtCoefHeaderLL, 0);
            Grid.SetColumn(txtCoefHeaderLL, 2);
            grdRegress.Children.Add(txtCoefHeaderLL);

            TextBlock txtCoefHeaderUL = new TextBlock();
            txtCoefHeaderUL.Text = "Limits";
            txtCoefHeaderUL.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtCoefHeaderUL, 0);
            Grid.SetColumn(txtCoefHeaderUL, 3);
            grdRegress.Children.Add(txtCoefHeaderUL);

            TextBlock txtStdErrorHeader = new TextBlock();
            txtStdErrorHeader.Text = "Std Error";
            txtStdErrorHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtStdErrorHeader, 0);
            Grid.SetColumn(txtStdErrorHeader, 4);
            grdRegress.Children.Add(txtStdErrorHeader);

            TextBlock txtFHeader = new TextBlock();
            txtFHeader.Text = "F-test";
            txtFHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtFHeader, 0);
            Grid.SetColumn(txtFHeader, 5);
            grdRegress.Children.Add(txtFHeader);

            TextBlock txtPHeader = new TextBlock();
            txtPHeader.Text = "P-value";
            txtPHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtPHeader, 0);
            Grid.SetColumn(txtPHeader, 6);
            grdRegress.Children.Add(txtPHeader);
        }

        #endregion        

        /// <summary>
        /// Closes the gadget
        /// </summary>
        protected override void CloseGadget()
        {
            if (worker != null && worker.WorkerSupportsCancellation)
            {
                worker.CancelAsync();
            }

            if (worker != null)
            {
                worker.DoWork -= new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
                worker.RunWorkerCompleted -= new System.ComponentModel.RunWorkerCompletedEventHandler(worker_WorkerCompleted);
            }
            if (baseWorker != null)
            {
                baseWorker.DoWork -= new System.ComponentModel.DoWorkEventHandler(Execute);
            }

            this.GadgetStatusUpdate -= new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation -= new GadgetCheckForCancellationHandler(IsCancelled);

            base.CloseGadget();

            GadgetOptions = null;
        }

        /// <summary>
        /// Sets the gadget's state to 'finished with error' mode
        /// </summary>
        /// <param name="errorMessage">The error message to display</param>
        protected override void RenderFinishWithError(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = Controls.MessagePanelType.ErrorPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            grdRegress.Visibility = System.Windows.Visibility.Collapsed;
            grdParameters.Visibility = System.Windows.Visibility.Collapsed;
            txtCorrelation.Visibility = System.Windows.Visibility.Collapsed;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        #region Private Methods

        private void DrawRegressionBorders()
        {
            //waitCursor.Visibility = Visibility.Collapsed;
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;
            int rdcount = 0;
            foreach (RowDefinition rd in grdRegress.RowDefinitions)
            {
                int cdcount = 0;
                foreach (ColumnDefinition cd in grdRegress.ColumnDefinitions)
                {
                    Border border = new Border();
                    border.Style = this.Resources["gridCellBorder"] as Style;

                    if (rdcount == 0 && cdcount > 0)
                    {
                        border.BorderThickness = new Thickness(border.BorderThickness.Left, border.BorderThickness.Bottom, border.BorderThickness.Right, border.BorderThickness.Bottom);
                    }
                    if (cdcount == 0 && rdcount > 0)
                    {
                        border.BorderThickness = new Thickness(border.BorderThickness.Right, border.BorderThickness.Top, border.BorderThickness.Right, border.BorderThickness.Bottom);
                    }

                    Grid.SetRow(border, rdcount);
                    Grid.SetColumn(border, cdcount);
                    grdRegress.Children.Add(border);
                    cdcount++;
                }
                rdcount++;
            }
        }

        private void FillComboboxes(bool update = false)
        {
            LoadingCombos = true;

            string prevOutcome = string.Empty;
            string prevWeight = string.Empty;

            if (update)
            {
                if (cbxFieldOutcome.SelectedIndex >= 0)
                {
                    prevOutcome = cbxFieldOutcome.SelectedItem.ToString();
                }
                if (cbxFieldWeight.SelectedIndex >= 0)
                {
                    prevWeight = cbxFieldWeight.SelectedItem.ToString();
                }
            }

            cbxFieldOutcome.ItemsSource = null;
            cbxFieldOutcome.Items.Clear();

            cbxFieldWeight.ItemsSource = null;
            cbxFieldWeight.Items.Clear();

            if (!update)
            {
                cbxConf.ItemsSource = null;
                cbxConf.Items.Clear();

                cbxConf.Items.Add(string.Empty);
                cbxConf.Items.Add("90%");
                cbxConf.Items.Add("95%");
                cbxConf.Items.Add("99%");

                cbxFields.ItemsSource = null;
                cbxFields.Items.Clear();

                lbxOtherFields.ItemsSource = null;
                lbxOtherFields.Items.Clear();

                lbxInteractionTerms.ItemsSource = null;
                lbxInteractionTerms.Items.Clear();
            }

            List<string> fieldNames = new List<string>();
            List<string> weightFieldNames = new List<string>();
            List<string> otherFieldNames = new List<string>();

            weightFieldNames.Add(string.Empty);
            otherFieldNames.Add(string.Empty);
            
            ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.UserDefined;
            fieldNames = DashboardHelper.GetFieldsAsList(columnDataType);

            columnDataType = ColumnDataType.Boolean | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.UserDefined;
            otherFieldNames = DashboardHelper.GetFieldsAsList(columnDataType);

            columnDataType = ColumnDataType.Numeric | ColumnDataType.UserDefined;
            weightFieldNames.AddRange(DashboardHelper.GetFieldsAsList(columnDataType));

            fieldNames.Sort();
            weightFieldNames.Sort();
            otherFieldNames.Sort();

            cbxFieldOutcome.ItemsSource = fieldNames;
            cbxFieldWeight.ItemsSource = weightFieldNames;
            cbxFields.ItemsSource = otherFieldNames;

            if (cbxFieldOutcome.Items.Count > 0)
            {
                cbxFieldOutcome.SelectedIndex = -1;
                cbxFieldWeight.SelectedIndex = -1;
                cbxFields.SelectedIndex = -1;
                if (!update)
                {
                    lbxOtherFields.SelectedItems.Clear();
                }
            }

            if (lbxOtherFields.Items.Contains(string.Empty))
            {
                lbxOtherFields.Items.Remove(string.Empty);
            }

            if (update)
            {
                cbxFieldOutcome.SelectedItem = prevOutcome;
                cbxFieldWeight.SelectedItem = prevWeight;
                cbxFields.SelectedIndex = -1;
            }

            LoadingCombos = false;
        }

        private void SetGridText(Grid grid, TextBlockConfig textBlockConfig)
        {
            TextBlock txt = new TextBlock();
            txt.Text = textBlockConfig.Text;
            txt.Margin = textBlockConfig.Margin;
            txt.VerticalAlignment = textBlockConfig.VerticalAlignment;
            txt.HorizontalAlignment = textBlockConfig.HorizontalAlignment;
            Grid.SetRow(txt, textBlockConfig.RowNumber);
            Grid.SetColumn(txt, textBlockConfig.ColumnNumber);
            grid.Children.Add(txt);
        }        

        private void AddGridRow(Grid grid, int height)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;
            grdRegress.Visibility = Visibility.Visible;
            RowDefinition rowDef = new RowDefinition();
            rowDef.Height = new GridLength(height);
            grid.RowDefinitions.Add(rowDef);
        }
        #endregion

        #region IGadget Members

        /// <summary>
        /// Sets the gadget to its 'processing' state
        /// </summary>
        public override void SetGadgetToProcessingState()
        {
            this.IsProcessing = true;
        }

        /// <summary>
        /// Sets the gadget to its 'finished' state
        /// </summary>
        public override void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
            base.SetGadgetToFinishedState();
        }

        /// <summary>
        /// Updates the variable names available in the gadget's properties
        /// </summary>
        public override void UpdateVariableNames()
        {
            FillComboboxes(true);
        }

        public override void RefreshResults()
        {
            if (!LoadingCombos && cbxFieldOutcome.SelectedIndex > -1 && (lbxOtherFields.Items.Count > 0 || lbxDummy.Items.Count > 0))
            {
                CreateInputVariableList();

                if (GadgetOptions.InputVariableList == null || GadgetOptions.InputVariableList.Count == 0)
                {
                    RenderFinishWithError("No inputs selected.");
                    return;
                }

//                txtFilterString.Visibility = System.Windows.Visibility.Collapsed;
                waitPanel.Visibility = System.Windows.Visibility.Visible;
                messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
                descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;                

                baseWorker = new BackgroundWorker();
                baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
                baseWorker.RunWorkerAsync();

                base.RefreshResults();
            }
        }

        public override void ShowHideConfigPanel()
        {
            Popup = new DashboardPopup();
            Popup.Parent = ((this.Parent as DragCanvas).Parent as ScrollViewer).Parent as Grid;
            Controls.GadgetProperties.LinearRegressionProperties properties = new Controls.GadgetProperties.LinearRegressionProperties(this.DashboardHelper, this, (LogisticParameters)Parameters);

            properties.Width = 800;
            properties.Height = 600;

            if ((System.Windows.SystemParameters.PrimaryScreenWidth / 1.2) > properties.Width)
            {
                properties.Width = (System.Windows.SystemParameters.PrimaryScreenWidth / 1.2);
            }

            if ((System.Windows.SystemParameters.PrimaryScreenHeight / 1.2) > properties.Height)
            {
                properties.Height = (System.Windows.SystemParameters.PrimaryScreenHeight / 1.2);
            }

            properties.Cancelled += new EventHandler(properties_Cancelled);
            properties.ChangesAccepted += new EventHandler(properties_ChangesAccepted);
            Popup.Content = properties;
            Popup.Show();
        }

        private void properties_ChangesAccepted(object sender, EventArgs e)
        {
            Controls.GadgetProperties.LinearRegressionProperties properties = Popup.Content as Controls.GadgetProperties.LinearRegressionProperties;
            this.Parameters = properties.Parameters;
            this.DataFilters = properties.DataFilters;
            Popup.Close();
            if (properties.HasSelectedFields)
            {
                RefreshResults();
            }
        }

        private void properties_Cancelled(object sender, EventArgs e)
        {
            Popup.Close();
        }

        private void CreateInputVariableList()
        {
            Dictionary<string, string> inputVariableList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            GadgetOptions.MainVariableName = string.Empty;
            GadgetOptions.WeightVariableName = string.Empty;
            GadgetOptions.StrataVariableNames = new List<string>();
            GadgetOptions.CrosstabVariableName = string.Empty;
            GadgetOptions.InputVariableList = new Dictionary<string, string>();

            try
            {
                inputVariableList.Add(cbxFieldOutcome.SelectedItem.ToString(), "dependvar");

                if (cbxFieldWeight.SelectedIndex >= 0 && !string.IsNullOrEmpty(cbxFieldWeight.SelectedItem.ToString()))
                {
                    inputVariableList.Add(cbxFieldWeight.SelectedItem.ToString(), "weightvar");
                }

                if (checkboxNoIntercept.IsChecked == true)
                {
                    inputVariableList.Add("intercept", "false");
                }
                else
                {
                    inputVariableList.Add("intercept", "true");
                }

                if (checkboxIncludeMissing.IsChecked == true)
                {
                    inputVariableList.Add("includemissing", "true");
                }
                else
                {
                    inputVariableList.Add("includemissing", "false");
                }

                double p = 0.95;
                if (!string.IsNullOrEmpty(cbxConf.SelectedItem.ToString()))
                {
                    bool success = Double.TryParse(cbxConf.SelectedItem.ToString().Replace("%", string.Empty), out p);
                    if (!success)
                    {
                        p = 0.95;
                    }
                    else
                    {
                        p = p / 100;
                    }
                }
                inputVariableList.Add("p", p.ToString());

                foreach (string s in lbxOtherFields.Items)
                {
                    inputVariableList.Add(s, "unsorted");
                }

                foreach (string s in lbxInteractionTerms.Items)
                {
                    inputVariableList.Add(s, "term");
                }

                foreach (string s in lbxDummy.Items)
                {
                    inputVariableList.Add(s, "discrete");
                }
            }
            catch (ArgumentException)
            {
                txtCorrelation.Visibility = System.Windows.Visibility.Collapsed;
                grdParameters.Visibility = System.Windows.Visibility.Collapsed;
                grdRegress.Visibility = Visibility.Collapsed;
                grdRegress.RowDefinitions.Clear();          
                return;
            }

            GadgetOptions.InputVariableList = inputVariableList;
        }

        /// <summary>
        /// Generates Xml representation of this gadget
        /// </summary>
        /// <param name="doc">The Xml docment</param>
        /// <returns>XmlNode</returns>
        public override XmlNode Serialize(XmlDocument doc)
        {
            string dependVar = string.Empty;
            string weightVar = string.Empty;
            string pvalue = string.Empty;
            bool intercept = true;

            if (cbxFieldOutcome.SelectedItem != null)
            {
                dependVar = cbxFieldOutcome.SelectedItem.ToString();
            }

            if (cbxFieldWeight.SelectedItem != null)
            {
                weightVar = cbxFieldWeight.SelectedItem.ToString();
            }

            if (checkboxNoIntercept.IsChecked == true)
            {
                intercept = false;
            }
            else
            {
                intercept = true;
            }

            double p = 0.95;
            if (!string.IsNullOrEmpty(cbxConf.SelectedItem.ToString()))
            {
                bool success = Double.TryParse(cbxConf.SelectedItem.ToString().Replace("%", string.Empty), out p);
                if (!success)
                {
                    pvalue = 95.ToString();
                }
                else
                {
                    pvalue = p.ToString();
                }
            }

            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            string xmlString =
            "<mainVariable>" + dependVar.Replace("<", "&lt;") + "</mainVariable>" +
            "<weightVariable>" + weightVar.Replace("<", "&lt;") + "</weightVariable>" +
            "<pvalue>" + pvalue + "</pvalue>" +
            "<intercept>" + intercept.ToString() + "</intercept>" +
            "<customHeading>" + CustomOutputHeading.Replace("<", "&lt;") + "</customHeading>" +
            "<customDescription>" + CustomOutputDescription.Replace("<", "&lt;") + "</customDescription>";

            xmlString = xmlString + SerializeAnchors();

            System.Xml.XmlElement element = doc.CreateElement("linearRegressionGadget");
            element.InnerXml = xmlString;

            System.Xml.XmlAttribute id = doc.CreateAttribute("id");
            System.Xml.XmlAttribute locationY = doc.CreateAttribute("top");
            System.Xml.XmlAttribute locationX = doc.CreateAttribute("left");
            System.Xml.XmlAttribute collapsed = doc.CreateAttribute("collapsed");
            System.Xml.XmlAttribute type = doc.CreateAttribute("gadgetType");

            id.Value = this.UniqueIdentifier.ToString();
            locationY.Value = Canvas.GetTop(this).ToString("F0");
            locationX.Value = Canvas.GetLeft(this).ToString("F0");
            collapsed.Value = IsCollapsed.ToString();      
            type.Value = "EpiDashboard.LinearRegressionControl";

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);
            element.Attributes.Append(id);

            if ((lbxDummy.Items.Count + lbxOtherFields.Items.Count) > 0)
            {
                string xmlCovariateString = string.Empty;
                XmlElement covariateElement = doc.CreateElement("covariates");

                foreach (string s in lbxOtherFields.Items)
                {
                    xmlCovariateString = xmlCovariateString + "<covariate>" + s + "</covariate>";
                }

                foreach (string s in lbxDummy.Items)
                {
                    xmlCovariateString = xmlCovariateString + "<dummy>" + s + "</dummy>";
                }

                foreach (string s in lbxInteractionTerms.Items)
                {
                    xmlCovariateString = xmlCovariateString + "<interactionTerm>" + s + "</interactionTerm>";
                }

                covariateElement.InnerXml = xmlCovariateString;
                element.AppendChild(covariateElement);
            }

            return element;
        }

        /// <summary>
        /// Creates the gadget from an Xml element
        /// </summary>
        /// <param name="element">The element from which to create the gadget</param>
        public override void CreateFromXml(XmlElement element)
        {
            this.LoadingCombos = true;

            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLower())
                {
                    case "mainvariable":
                        cbxFieldOutcome.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "weightvariable":
                        cbxFieldWeight.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "pvalue":
                        if (child.InnerText.Equals("90")) { cbxConf.SelectedIndex = 1; }
                        if (child.InnerText.Equals("95")) { cbxConf.SelectedIndex = 2; }
                        if (child.InnerText.Equals("99")) { cbxConf.SelectedIndex = 3; }
                        break;
                    case "customheading":
                        if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
                        {
                            this.CustomOutputHeading = child.InnerText.Replace("&lt;", "<"); ;
                        }
                        break;
                    case "customdescription":
                        if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
                        {
                            this.CustomOutputDescription = child.InnerText.Replace("&lt;", "<");

                            if (!string.IsNullOrEmpty(CustomOutputDescription) && !CustomOutputHeading.Equals("(none)"))
                            {
                                descriptionPanel.Text = CustomOutputDescription;
                                descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.DisplayMode;
                            }
                            else
                            {
                                descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
                            }
                        }
                        break;
                    case "intercept":
                        if (child.InnerText.ToLower().Equals("false")) { checkboxNoIntercept.IsChecked = true; }
                        break;
                    case "covariates":
                        foreach (XmlElement covariate in child.ChildNodes)
                        {   
                            if (covariate.Name.ToLower().Equals("covariate"))
                            {
                                lbxOtherFields.Items.Add(covariate.InnerText);
                            }
                            if (covariate.Name.ToLower().Equals("dummy"))
                            {
                                lbxDummy.Items.Add(covariate.InnerText);
                            }
                            if (covariate.Name.ToLower().Equals("interactionterm"))
                            {
                                lbxInteractionTerms.Items.Add(covariate.InnerText);
                            }
                        }
                        break;
                }
            }

            base.CreateFromXml(element);

            this.LoadingCombos = false;

            RefreshResults();
            HideConfigPanel();
        }
        #endregion        

        #region Public Methods
        /// <summary>
        /// Returns the gadget's description as a string.
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return "Linear Regression Gadget";
        }
        #endregion

        private void lbxOtherFields_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lbxOtherFields.SelectedItems.Count == 1)
            {
                lbxOtherFields.Items.Remove(lbxOtherFields.SelectedItem);
            }
        }

        private void lbxDummy_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lbxDummy.SelectedItems.Count == 1)
            {
                lbxDummy.Items.Remove(lbxDummy.SelectedItem);
                lbxOtherFields.Items.Add(lbxDummy.SelectedItem);
            }
        }

        private void lbxInteractionTerms_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lbxInteractionTerms.SelectedItems.Count == 1)
            {
                lbxInteractionTerms.Items.Remove(lbxInteractionTerms.SelectedItem);
            }
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0)
        {
            if (IsCollapsed) return string.Empty;

            StringBuilder htmlBuilder = new StringBuilder();
            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            if (CustomOutputHeading == null || (string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)")))
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">Linear Regression</h2>");
            }
            else if (CustomOutputHeading != "(none)")
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">" + CustomOutputHeading + "</h2>");
            }

            htmlBuilder.AppendLine("<p class=\"gadgetSettings\"><small>");
            htmlBuilder.AppendLine("<em>Outcome variable:</em> <strong>" + cbxFieldOutcome.Text + "</strong>");
            htmlBuilder.AppendLine("<br />");

            if (cbxFieldWeight.SelectedIndex >= 0)
            {
                htmlBuilder.AppendLine("<em>Weight variable:</em> <strong>" + cbxFieldWeight.Text + "</strong>");
                htmlBuilder.AppendLine("<br />");
            }

            string varList = string.Empty;
            foreach (string s in this.lbxOtherFields.Items)
            {
                varList = varList + s + ",";
            }
            varList = varList.TrimEnd(',');

            htmlBuilder.AppendLine("<em>Co-variates:</em> <strong>" + varList + "</strong>");
            htmlBuilder.AppendLine("<br />");

            varList = string.Empty;
            foreach (string s in this.lbxDummy.Items)
            {
                varList = varList + s + ",";
            }
            varList = varList.TrimEnd(',');

            htmlBuilder.AppendLine("<em>Dummy variables:</em> <strong>" + varList + "</strong>");
            htmlBuilder.AppendLine("<br />");

            varList = string.Empty;
            foreach (string s in this.lbxInteractionTerms.Items)
            {
                varList = varList + s + ",";
            }
            varList = varList.TrimEnd(',');

            htmlBuilder.AppendLine("<em>Interaction terms:</em> <strong>" + varList + "</strong>");
            htmlBuilder.AppendLine("<br />");

            htmlBuilder.AppendLine("<em>Include missing:</em> <strong>" + checkboxIncludeMissing.IsChecked.ToString() + "</strong>");
            htmlBuilder.AppendLine("<br />");
            htmlBuilder.AppendLine("</small></p>");

            if (!string.IsNullOrEmpty(CustomOutputDescription))
            {
                htmlBuilder.AppendLine("<p class=\"gadgetsummary\">" + CustomOutputDescription + "</p>");
            }

            if (!string.IsNullOrEmpty(messagePanel.Text) && messagePanel.Visibility == System.Windows.Visibility.Visible)
            {
                htmlBuilder.AppendLine("<p><small><strong>" + messagePanel.Text + "</strong></small></p>");
            }

            if (!string.IsNullOrEmpty(txtFilterString.Text) && txtFilterString.Visibility == Visibility.Visible)
            {
                htmlBuilder.AppendLine("<p><small><strong>" + txtFilterString.Text + "</strong></small></p>");
            }

            htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
            htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
            //htmlBuilder.AppendLine("<caption>" + grid.Tag + "</caption>");

            foreach (UIElement control in grdRegress.Children)
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

                    htmlBuilder.AppendLine(tableDataTagOpen + formattedValue + tableDataTagClose);

                    if (columnNumber >= grdRegress.ColumnDefinitions.Count - 1)
                    {
                        htmlBuilder.AppendLine("</tr>");
                    }
                }
            }

            htmlBuilder.AppendLine("</table>");

            htmlBuilder.AppendLine("<p><strong>" + txtCorrelation.Text + "</strong></p>");

            htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
            htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <th>Source</th>");
            htmlBuilder.AppendLine("  <th>df</th>");
            htmlBuilder.AppendLine("  <th>Sum of Squares</th>");
            htmlBuilder.AppendLine("  <th>Mean Square</th>");
            htmlBuilder.AppendLine("  <th>F-statistic</th>");
            htmlBuilder.AppendLine("  <th>p-value</th>");
            htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <th>Regression</th>");
            htmlBuilder.AppendLine("  <td>" + txtRegressionDf.Text + "</td>");
            htmlBuilder.AppendLine("  <td>" + txtRegressionSumOfSquares.Text + "</td>");
            htmlBuilder.AppendLine("  <td>" + txtRegressionMeanSquare.Text + "</td>");
            htmlBuilder.AppendLine("  <td>" + txtRegressionFstatistic.Text + "</td>");
            htmlBuilder.AppendLine("  <td>" + txtRegressionFstatisticP.Text + "</td>");
            htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine(" <tr>");
            htmlBuilder.AppendLine("  <th>Residuals</th>");
            htmlBuilder.AppendLine("  <td>" + txtResidualsDf.Text + "</td>");
            htmlBuilder.AppendLine("  <td>" + txtResidualsSumOfSquares.Text + "</td>");
            htmlBuilder.AppendLine("  <td>" + txtResidualsMeanSquare.Text + "</td>");
            htmlBuilder.AppendLine("  <td></td>");
            htmlBuilder.AppendLine("  <td></td>");
            htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine("  <th>Total</th>");
            htmlBuilder.AppendLine("  <td>" + txtTotalDf.Text + "</td>");
            htmlBuilder.AppendLine("  <td>" + txtTotalSumOfSquares.Text + "</td>");
            htmlBuilder.AppendLine("  <td></td>");
            htmlBuilder.AppendLine("  <td></td>");
            htmlBuilder.AppendLine("  <td></td>");
            htmlBuilder.AppendLine(" </tr>");
            htmlBuilder.AppendLine("</table>");

            if (txtPearsonAnalysis.IsVisible)
            {
                htmlBuilder.AppendLine("<p><strong>" + txtPearsonAnalysis.Text + "</strong></p>");
                htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
                htmlBuilder.AppendLine(" <tr>");
                htmlBuilder.AppendLine("  <th>Coefficient</th>");
                htmlBuilder.AppendLine("  <th>T Statistic</th>");
                htmlBuilder.AppendLine("  <th>P-Value</th>");
                htmlBuilder.AppendLine(" </tr>");
                htmlBuilder.AppendLine(" <tr>");
                htmlBuilder.AppendLine("  <td>" + txtPearsonCoefficient.Text + "</td>");
                htmlBuilder.AppendLine("  <td>" + txtPearsonCoefficientT.Text + "</td>");
                htmlBuilder.AppendLine("  <td>" + txtPearsonCoefficientTP.Text + "</td>");
                htmlBuilder.AppendLine(" </tr>");
                htmlBuilder.AppendLine("</table>");
            }
            if (txtSpearmanAnalysis.IsVisible)
            {
                htmlBuilder.AppendLine("<p><strong>" + txtSpearmanAnalysis.Text + "</strong></p>");
                htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
                htmlBuilder.AppendLine(" <tr>");
                htmlBuilder.AppendLine("  <th>Coefficient</th>");
                htmlBuilder.AppendLine("  <th>T Statistic</th>");
                htmlBuilder.AppendLine("  <th>P-Value</th>");
                htmlBuilder.AppendLine(" </tr>");
                htmlBuilder.AppendLine(" <tr>");
                htmlBuilder.AppendLine("  <td>" + txtSpearmanCoefficient.Text + "</td>");
                htmlBuilder.AppendLine("  <td>" + txtSpearmanCoefficientT.Text + "</td>");
                htmlBuilder.AppendLine("  <td>" + txtSpearmannCoefficientTP.Text + "</td>");
                htmlBuilder.AppendLine(" </tr>");
                htmlBuilder.AppendLine("</table>");
            }

            htmlBuilder.AppendLine("</table>");

            return htmlBuilder.ToString();
        }

        private string customOutputHeading;
        private string customOutputDescription;
        private string customOutputCaption;

        public override string CustomOutputHeading
        {
            get
            {
                return this.customOutputHeading;
            }
            set
            {
                this.customOutputHeading = value;
                headerPanel.Text = CustomOutputHeading;
            }
        }

        public override string CustomOutputDescription
        {
            get
            {
                return this.customOutputDescription;
            }
            set
            {
                this.customOutputDescription = value;
                descriptionPanel.Text = CustomOutputDescription;
            }
        }

        public override string CustomOutputCaption
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
    }
}
