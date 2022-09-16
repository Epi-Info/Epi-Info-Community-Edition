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
    /// Interaction logic for KaplanMeierControl.xaml
    /// </summary>
    public partial class KaplanMeierControl : GadgetBase
    {
        #region Private Variables

        private struct VariableRow 
        {
            public string variableName;
            public double oddsRatio;
            public double ninetyFivePercent;
            public double ci;
            public double coefficient;
            public double se;
            public double Z;
            public double P;
        }

        private struct RegressionResults
        {
            public List<VariableRow> variables;
            public string convergence;
            public int iterations;
            public double finalLikelihood;
            public int casesIncluded;
            public double scoreStatistic;
            public double scoreDF;
            public double scoreP;
            public double LRStatistic;
            public double LRDF;
            public double LRP;
            public string errorMessage;
            public StatisticsRepository.LogisticRegression.LogisticRegressionResults regressionResults;            
        }
        #endregion // Private Variables

        #region Delegates
        private new delegate void SetGridTextDelegate(Grid grid, TextBlockConfig textBlockConfig);
        private new delegate void AddGridRowDelegate(Grid grid, int height);
        private new delegate void AddGridFooterDelegate(int rowNumber, int totalRows);
        private delegate void RenderGridDelegate(RegressionResults regressionResults);
        #endregion // Delegates
        
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public KaplanMeierControl()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">The database to attach</param>
        /// <param name="view">The view to attach</param>
        public KaplanMeierControl(DashboardHelper dashboardHelper)
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

            Parameters = new KaplanMeierParameters();

            this.IsProcessing = false;

            this.GadgetStatusUpdate += new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(IsCancelled);
            this.headerPanel.IsDescriptionButtonAvailable = false;
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

        private View View
        {
            get
            {
                return this.DashboardHelper.View;
            }
        }

        private IDbDriver Database
        {
            get
            {
                return this.DashboardHelper.Database;
            }
        }

        void cbxFields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LoadingCombos)
            {
                return;
            }

            if (cbxFields.SelectedIndex > -1)
            {
                if (!lbxOtherFields.Items.Contains(cbxFields.SelectedItem.ToString()))
                {
                    lbxOtherFields.Items.Add(cbxFields.SelectedItem.ToString());
                }
            }
        }

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
		#endregion

		private class XYChartData
		{
			public object X { get; set; }
			public double? Y { get; set; }
		}

		private class RegressionChartData
		{
			public object X { get; set; }
			public double Z { get; set; }
		}

		public class NumericDataValue
		{
			public decimal DependentValue { get; set; }
			public decimal IndependentValue { get; set; }
		}

		#region Public Methods

		public void ClearResults() 
        {
            grdParameters.Visibility = Visibility.Collapsed;

            waitPanel.Visibility = System.Windows.Visibility.Visible;
            messagePanel.Text = string.Empty;
            messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;
        }

        public override void CollapseOutput()
        {
            grdParameters.Visibility = Visibility.Collapsed;

            if (this.txtFilterString != null && !string.IsNullOrEmpty(this.txtFilterString.Text))
            {
                this.txtFilterString.Visibility = System.Windows.Visibility.Collapsed;
            }

            this.messagePanel.Visibility = System.Windows.Visibility.Collapsed;
//            this.txtFilterString.Visibility = System.Windows.Visibility.Collapsed;
            //descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed; //EI-24
            IsCollapsed = true;
        }

        public override void ExpandOutput()
        {
            grdParameters.Visibility = Visibility.Visible;
            
            if (this.messagePanel.MessagePanelType != Controls.MessagePanelType.StatusPanel)
            {
                this.messagePanel.Visibility = System.Windows.Visibility.Visible;
            }

            if (this.txtFilterString != null && !string.IsNullOrEmpty(this.txtFilterString.Text))
            {
                this.txtFilterString.Visibility = System.Windows.Visibility.Visible;
            }
            if (!string.IsNullOrEmpty(this.descriptionPanel.Text))
            {
                descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.DisplayMode;
            }
            IsCollapsed = false;
        }
        
        #endregion

        #region Event Handlers

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            RefreshResults();
        }

        private void btnClearInteractionTerms_Click(object sender, RoutedEventArgs e)
        {
            lbxInteractionTerms.Items.Clear();
        }

        private void btnMakeDummy_Click(object sender, RoutedEventArgs e)
        {
            if (lbxOtherFields.SelectedItems.Count == 1)
            {
                if (!string.IsNullOrEmpty(lbxOtherFields.SelectedItem.ToString()))
                {
                    string columnName = lbxOtherFields.SelectedItem.ToString();
                    lbxOtherFields.Items.Remove(columnName);
                    lbxDummyTerms.Items.Add(columnName);
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
            else if (lbxOtherFields.SelectedItems.Count == 2)
            {
                btnMakeDummy.Content = "Make Interaction";
            }
		}

		private void SetChartData(List<XYChartData> dataList, List<int> treatmentStartIndexes, List<string> treatmentNames)
		{
			int serieses = 0;
			for (int s = xyChart.DataSeries.Count - 1; s > 0; s--)
			{
				xyChart.DataSeries.Remove(xyChart.DataSeries[s]);
			}
			if (treatmentStartIndexes != null && treatmentStartIndexes.Count > 1)
			{
				try
				{
					series0.DataSource = dataList.GetRange(0, treatmentStartIndexes[1]);
					series0.Label = treatmentNames[0];
					int seriesend = dataList.Count;
					for (int t = 1; t < treatmentStartIndexes.Count; t++)
					{
						seriesend = dataList.Count;
						if (treatmentStartIndexes.Count > t + 1)
							seriesend = treatmentStartIndexes[t + 1];
						ComponentArt.Win.DataVisualization.Charting.LineSeries series = new ComponentArt.Win.DataVisualization.Charting.LineSeries();
						series.XPath = "X";
						series.YPath = "Y";
						series.ShowPointAnnotations = false;
						series.LineKind = ComponentArt.Win.DataVisualization.Charting.LineKind.Step;
						series.Label = treatmentNames[t];
						series.DataSource = dataList.GetRange(treatmentStartIndexes[t], seriesend - treatmentStartIndexes[t]);
						xyChart.DataSeries.Add(series);
					}
				}
				catch (Exception e)
				{

				}
			}
			else
			{
				series0.DataSource = dataList;
				//series1.DataSource = dataList;
			}
			xyChart.LegendVisible = true;
			xyChart.Width = 600;
			xyChart.Height = 400;
			xyChart.Visibility = Visibility.Visible;
		}

		private void SetXandYCoordinates(List<XYChartData> dataList, int newMinXvalue, int newMaxXvalue)
		{

			//---Ei-196
			//adds one step before
			int remvalue = 2;
			int value = newMinXvalue % 2;
			if (value == 1) { remvalue = 1; };
			xyChart.XRangeStart = newMinXvalue - remvalue;
			remvalue = 2;
			xyChart.XRangeEnd = newMaxXvalue + remvalue;

			int Yminvalue = Int32.MaxValue;
			int Ymaxvalue = Int32.MinValue;
			remvalue = 0;
			foreach (XYChartData xyc in dataList)
			{
				if (xyc.Y.HasValue)
				{
					int Tminvalue = (int)xyc.Y;
					Yminvalue = Math.Min(Yminvalue, Tminvalue);
					int Tmaxvalue = (int)xyc.Y;
					Ymaxvalue = Math.Max(Ymaxvalue, Tmaxvalue);
				}
			}
			value = Yminvalue % 10;
			if (value > 0) { remvalue = value; }
			ComponentArt.Win.DataVisualization.Charting.NumericCoordinates numy = new ComponentArt.Win.DataVisualization.Charting.NumericCoordinates();
			numy.From = (int)Yminvalue - remvalue;
			ComponentArt.Win.DataVisualization.Charting.AxisCoordinates ay = new ComponentArt.Win.DataVisualization.Charting.AxisCoordinates();
			ay.Coordinates = numy;
			ay.Visibility = System.Windows.Visibility.Visible;
			xyChart.YAxisArea.Clear();
			xyChart.YAxisArea.Add(ay);
			//-- 
		}

		private delegate void SetChartDataDelegate(List<XYChartData> dataList, List<int> treatmentStartIndexe, List<string> treatmentNames);

		protected override void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Configuration config = DashboardHelper.Config;
            Dictionary<string, string> setProperties = new Dictionary<string, string>();
            setProperties.Add("BLabels", config.Settings.RepresentationOfYes + ";" + config.Settings.RepresentationOfNo + ";" + config.Settings.RepresentationOfMissing); // TODO: Replace Yes, No, Missing with global vars

            bool includeMissing = true;

            Dictionary<string, string> inputVariableList = GadgetOptions.InputVariableList;

            lock (syncLock)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));

                SetGridTextDelegate setText = new SetGridTextDelegate(SetGridText);
                AddGridRowDelegate addRow = new AddGridRowDelegate(AddGridRow);

                System.Collections.Generic.Dictionary<string, System.Data.DataTable> Freq_ListSet = new Dictionary<string, System.Data.DataTable>();

                string customFilter = string.Empty;
                List<string> columnNames = new List<string>();

                foreach (KeyValuePair<string, string> kvp in inputVariableList)
                {   
                    if (kvp.Key.ToLowerInvariant().Equals("includemissing"))
                    {
                        includeMissing = bool.Parse(kvp.Value);
                    }
                }

                foreach (KeyValuePair<string, string> kvp in inputVariableList)
                {
					if (kvp.Value == null) continue;
					if (kvp.Value.ToLowerInvariant().Equals("unsorted") || kvp.Value.ToLowerInvariant().Equals("dependvar") || kvp.Value.ToLowerInvariant().Equals("weightvar") || kvp.Value.ToLowerInvariant().Equals("matchvar"))
					{
						columnNames.Add(kvp.Key);
						if (!kvp.Value.ToLowerInvariant().Equals("dependvar"))
						{
							customFilter = customFilter + StringLiterals.PARANTHESES_OPEN + StringLiterals.LEFT_SQUARE_BRACKET + kvp.Key + StringLiterals.RIGHT_SQUARE_BRACKET + StringLiterals.SPACE + "is not null" + StringLiterals.PARANTHESES_CLOSE + " AND ";
						}
					}
					else if (kvp.Value.ToLowerInvariant().Equals("discrete"))
					{
						columnNames.Add(kvp.Key);
						customFilter = customFilter + StringLiterals.PARANTHESES_OPEN + StringLiterals.LEFT_SQUARE_BRACKET + kvp.Key + StringLiterals.RIGHT_SQUARE_BRACKET + StringLiterals.SPACE + "is not null" + StringLiterals.PARANTHESES_CLOSE + " AND ";
					}
					else if (kvp.Key.ToLowerInvariant().Equals("time_variable"))
					{
						columnNames.Add(kvp.Value);
						customFilter = customFilter + StringLiterals.PARANTHESES_OPEN + StringLiterals.LEFT_SQUARE_BRACKET + kvp.Value + StringLiterals.RIGHT_SQUARE_BRACKET + StringLiterals.SPACE + "is not null" + StringLiterals.PARANTHESES_CLOSE + " AND ";
					}
                }

                if (includeMissing)
                {
                    customFilter = string.Empty;
                }
                else
                {
                    customFilter = customFilter.Remove(customFilter.Length - 4, 4);
                }

                try
                {
                    lock (staticSyncLock)
                    {
                        string GadgetFilter = String.Empty;
                        GadgetFilter = DataFilters.GenerateDataFilterString(false);
                        if (!String.IsNullOrEmpty(GadgetFilter))
                            customFilter = customFilter + " AND (" + GadgetFilter + ")";

                        DataTable regressTable = DashboardHelper.GenerateTable(columnNames, customFilter);

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
                            StatisticsRepository.LogisticRegression logisticRegression = new StatisticsRepository.LogisticRegression();
							StatisticsRepository.EIKaplanMeierSurvival kmSurvival = new StatisticsRepository.EIKaplanMeierSurvival();
							IAnalysisStatisticContext contexxt = new Object() as IAnalysisStatisticContext;
							kmSurvival.contextInputVariableList = inputVariableList;
							Dictionary<string, string> contextSetProperties = new Dictionary<string, string>();
							contextSetProperties.Add("TableName", "LEUKEM210");
							contextSetProperties.Add("CommandText", "KMSURVIVAL TIMEVAR = TESTVAR * CENSOREDVAR (0) GRAPHTYPE = \"Survival Probability\"");
							contextSetProperties.Add("BLabels", "Yes;No;Missing");
							kmSurvival.contextSetProperties = contextSetProperties;
							kmSurvival.contextColumns = regressTable.Columns;
							kmSurvival.contextDataTable = regressTable;
							kmSurvival.Execute();

							Array KMResultsArray = (Array)kmSurvival.ResultArray;
							Array KMRA1 = (Array)KMResultsArray.GetValue(1,1);
							Double.TryParse(KMRA1.GetValue(0, 2).ToString(), out results.scoreDF);
							Double.TryParse(KMRA1.GetValue(0, 3).ToString(), out results.scoreP);
							Double.TryParse(KMRA1.GetValue(0, 1).ToString(), out results.scoreStatistic);
							Double.TryParse(KMRA1.GetValue(1, 2).ToString(), out results.LRDF);
							Double.TryParse(KMRA1.GetValue(1, 3).ToString(), out results.LRP);
							Double.TryParse(KMRA1.GetValue(1, 1).ToString(), out results.LRStatistic);
							
							DataTable dattab = new DataTable();
							dattab.Columns.Add("time");
							dattab.Columns.Add("percent");
							dattab.Columns.Add("treatment");
							Array graphdata = (Array)KMResultsArray.GetValue(1, 0);
							string currentTreatment = graphdata.GetValue(1, 0).ToString();
							List<int> treatmentStartIndexes = new List<int>();
							List<string> treatmentNames = new List<string>();
							treatmentStartIndexes.Add(0);
							treatmentNames.Add(currentTreatment.Trim());
							for (int pct = 0; pct < graphdata.Length / 8; pct++)
							{
								DataRow dr = dattab.NewRow();
								dr[0] = graphdata.GetValue(0, pct);
								dr[1] = graphdata.GetValue(5, pct);
								dr[2] = graphdata.GetValue(1, pct);
								dattab.Rows.Add(dr);
								if (!dr[2].ToString().Equals(currentTreatment))
								{
									treatmentStartIndexes.Add(pct);
									currentTreatment = dr[2].ToString();
									treatmentNames.Add(currentTreatment.Trim());
								}
							}
							DataView dv = new DataView(dattab);

							List<XYChartData> dataList = new List<XYChartData>();

							NumericDataValue minValue = null;
							NumericDataValue maxValue = null;

							foreach (DataRowView drv in dv)
							{
								DataRow row = drv.Row;

								if (row["time"] != DBNull.Value && row["time"] != null && row["percent"] != DBNull.Value && row["percent"] != null)
								{
									XYChartData chartData = new XYChartData();
									chartData.X = Convert.ToDouble(row["time"]);
									chartData.Y = Convert.ToDouble(row["percent"]);
									dataList.Add(chartData);
								}
							}

							DataRow[] drs = dv.Table.Select(dv.RowFilter);
							DataTable cloneTable = dv.Table.Clone();
							foreach (DataRow dr in drs)
							{
								cloneTable.ImportRow(dr);
							}
							this.Dispatcher.BeginInvoke(new SetChartDataDelegate(SetChartData), dataList, treatmentStartIndexes, treatmentNames);
							//                            Array logRegressionResults = Epi.Statistics.SharedResources.LogRegressionWithR(regressTable);

							results.casesIncluded = 0;
                            results.convergence = results.regressionResults.convergence;
                            results.finalLikelihood = 0;
                            results.iterations = 0;
                            results.errorMessage = "";
                            results.variables = new List<VariableRow>();

                            if (!string.IsNullOrEmpty(results.errorMessage))
                            {
                                throw new ApplicationException(results.errorMessage);

                            }
                            if (results.regressionResults.variables != null)
                            {
                                foreach (StatisticsRepository.LogisticRegression.VariableRow vrow in results.regressionResults.variables)
                                {
                                    VariableRow nrow = new VariableRow();
                                    nrow.coefficient = vrow.coefficient;
                                    nrow.ci = vrow.ci;
                                    nrow.P = vrow.P;
                                    nrow.ninetyFivePercent = vrow.ninetyFivePercent;
                                    nrow.oddsRatio = vrow.oddsRatio;
                                    nrow.se = vrow.se;
                                    nrow.variableName = vrow.variableName;
                                    nrow.Z = vrow.Z;
                                    results.variables.Add(nrow);
                                }

                                
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
                    Debug.Print("Logistic regression gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete with " + DashboardHelper.RecordCount.ToString() + " records and the following filters:");
                    Debug.Print(DashboardHelper.DataFilters.GenerateDataFilterString());
                    //gridCells.singleTableResults = new StatisticsRepository.cTable().SigTable((double)gridCells.yyVal, (double)gridCells.ynVal, (double)gridCells.nyVal, (double)gridCells.nnVal, 0.95);
                }
            }
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

            grdParameters.Visibility = System.Windows.Visibility.Collapsed;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        private void RenderRegressionResults(RegressionResults results)
        {
            grdParameters.Visibility = System.Windows.Visibility.Visible;

            waitPanel.Visibility = System.Windows.Visibility.Collapsed;
            btnRun.IsEnabled = true;

            if (!string.IsNullOrEmpty(results.errorMessage) || results.variables == null)
            {                
                grdParameters.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {                
                grdParameters.Visibility = System.Windows.Visibility.Visible;

                txtScoreStatistic.Text = StringLiterals.SPACE + results.scoreStatistic.ToString("F4") + StringLiterals.SPACE;
                txtScoreDF.Text = StringLiterals.SPACE + results.scoreDF.ToString() + StringLiterals.SPACE;
                txtScoreP.Text = StringLiterals.SPACE + results.scoreP.ToString("F4") + StringLiterals.SPACE;

                txtLStatistic.Text = StringLiterals.SPACE + results.LRStatistic.ToString("F4") + StringLiterals.SPACE;
                txtLDF.Text = StringLiterals.SPACE + results.LRDF.ToString() + StringLiterals.SPACE;
                txtLP.Text = StringLiterals.SPACE + results.LRP.ToString("F4") + StringLiterals.SPACE;

			}

            HideConfigPanel();
        }

        private void RenderRegressionHeader()
        {
            RowDefinition rowDefHeader = new RowDefinition();
            rowDefHeader.Height = new GridLength(30);

            TextBlock txtVarHeader = new TextBlock();
            txtVarHeader.Text = "Term";
            txtVarHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtVarHeader, 0);
            Grid.SetColumn(txtVarHeader, 0);

            TextBlock txtOddsHeader = new TextBlock();
			headerPanel.Text = "Kaplan-Meier";
            txtOddsHeader.Text = "Odds Ratio";
			if (properties != null && properties.cbxFieldLink.SelectedValue.Equals("Log (For Evaluation)"))
			{
				txtOddsHeader.Text = "Risk Ratio";
				headerPanel.Text = "Log-Binomial Regression (For Evaluation Only)";
			}
            txtOddsHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtOddsHeader, 0);
            Grid.SetColumn(txtOddsHeader, 1);

            TextBlock txt95Header = new TextBlock();
            string percentText = "95%";

            if (cbxConf.SelectedItem != null && !(string.IsNullOrEmpty(cbxConf.SelectedItem.ToString())))
            {
                percentText = cbxConf.SelectedItem.ToString();
            }
            
            txt95Header.Text = percentText;
            txt95Header.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txt95Header, 0);
            Grid.SetColumn(txt95Header, 2);

            TextBlock txtCIHeader = new TextBlock();
            txtCIHeader.Text = "C.I.";
            txtCIHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtCIHeader, 0);
            Grid.SetColumn(txtCIHeader, 3);

            TextBlock txtCoefficientHeader = new TextBlock();
            txtCoefficientHeader.Text = "Coefficient";
            txtCoefficientHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtCoefficientHeader, 0);
            Grid.SetColumn(txtCoefficientHeader, 4);

            TextBlock txtSEHeader = new TextBlock();
            txtSEHeader.Text = "S.E.";
            txtSEHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtSEHeader, 0);
            Grid.SetColumn(txtSEHeader, 5);

            TextBlock txtZHeader = new TextBlock();
            txtZHeader.Text = "Z-Statistic";
            txtZHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtZHeader, 0);
            Grid.SetColumn(txtZHeader, 6);

            TextBlock txtPHeader = new TextBlock();
            txtPHeader.Text = "P-Value";
            txtPHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtPHeader, 0);
            Grid.SetColumn(txtPHeader, 7);
        }

        private void RenderIORHeader()
        {
                RowDefinition rowDefHeaderIOR = new RowDefinition();
                rowDefHeaderIOR.Height = new GridLength(30);

                TextBlock txtVarHeaderIOR = new TextBlock();
                txtVarHeaderIOR.Text = "Interaction Terms";
                txtVarHeaderIOR.Style = this.Resources["columnHeadingText"] as Style;
                Grid.SetRow(txtVarHeaderIOR, 0);
                Grid.SetColumn(txtVarHeaderIOR, 0);

                TextBlock txtOddsHeaderIOR = new TextBlock();
                txtOddsHeaderIOR.Text = "Odds Ratio";
                txtOddsHeaderIOR.Style = this.Resources["columnHeadingText"] as Style;
                Grid.SetRow(txtOddsHeaderIOR, 0);
                Grid.SetColumn(txtOddsHeaderIOR, 1);

                TextBlock txt95HeaderIOR = new TextBlock();
                string percentText = "95%";

                //            if (cbxConf.SelectedItem != null && !(string.IsNullOrEmpty(cbxConf.SelectedItem.ToString())))
                //            {
                //                percentText = cbxConf.SelectedItem.ToString();
                //            }

                txt95HeaderIOR.Text = percentText;
                txt95HeaderIOR.Style = this.Resources["columnHeadingText"] as Style;
                Grid.SetRow(txt95HeaderIOR, 0);
                Grid.SetColumn(txt95HeaderIOR, 2);

                TextBlock txtCIHeaderIOR = new TextBlock();
                txtCIHeaderIOR.Text = "C.I.";
                txtCIHeaderIOR.Style = this.Resources["columnHeadingText"] as Style;
                Grid.SetRow(txtCIHeaderIOR, 0);
                Grid.SetColumn(txtCIHeaderIOR, 3);
        }

        #endregion

        #region Private Methods

        private void DrawRegressionBorders()
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;
            
        }

        private void FillComboboxes(bool update = false)
        {
            LoadingCombos = true;

            string prevOutcome = string.Empty;
            string prevWeight = string.Empty;
            string prevMatch = string.Empty;

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
                if (cbxFieldMatch.SelectedIndex >= 0)
                {
                    prevMatch = cbxFieldMatch.SelectedItem.ToString();
                }
            }

            cbxFieldOutcome.ItemsSource = null;
            cbxFieldOutcome.Items.Clear();

            cbxFieldWeight.ItemsSource = null;
            cbxFieldWeight.Items.Clear();

            cbxFieldMatch.ItemsSource = null;
            cbxFieldMatch.Items.Clear();

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
            List<string> matchFieldNames = new List<string>();
            List<string> otherFieldNames = new List<string>();

            weightFieldNames.Add(string.Empty);
            matchFieldNames.Add(string.Empty);

            ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.UserDefined;
            fieldNames = DashboardHelper.GetFieldsAsList(columnDataType);

            columnDataType = ColumnDataType.Boolean | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.UserDefined;
            otherFieldNames = DashboardHelper.GetFieldsAsList(columnDataType);

            columnDataType = ColumnDataType.Numeric | ColumnDataType.UserDefined;
            weightFieldNames.AddRange(DashboardHelper.GetFieldsAsList(columnDataType));

            columnDataType = ColumnDataType.Numeric | ColumnDataType.Boolean | ColumnDataType.UserDefined;
            matchFieldNames.AddRange(DashboardHelper.GetFieldsAsList(columnDataType));

            fieldNames.Sort();
            weightFieldNames.Sort();
            otherFieldNames.Sort();
            matchFieldNames.Sort();

            cbxFieldOutcome.ItemsSource = fieldNames;
            cbxFieldWeight.ItemsSource = weightFieldNames;
            cbxFieldMatch.ItemsSource = matchFieldNames;
            cbxFields.ItemsSource = otherFieldNames;

            if (cbxFieldOutcome.Items.Count > 0)
            {
                cbxFieldOutcome.SelectedIndex = -1;
                cbxFieldWeight.SelectedIndex = -1;
                cbxFieldMatch.SelectedIndex = -1;
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
                cbxFieldMatch.SelectedItem = prevMatch;
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
            if (!LoadingCombos && cbxFieldOutcome.SelectedIndex > -1 && (lbxOtherFields.Items.Count > 0 || lbxDummyTerms.Items.Count > 0))
            {
                Dictionary<string, string> inputVariableList = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                CreateInputVariableList();

        //        txtFilterString.Visibility = System.Windows.Visibility.Collapsed;
                waitPanel.Visibility = System.Windows.Visibility.Visible;
                messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
                descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;                

                baseWorker = new BackgroundWorker();
                baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
                baseWorker.RunWorkerAsync();

                base.RefreshResults();
            }
        }

        Controls.GadgetProperties.KaplanMeierProperties properties = null;
        public override void ShowHideConfigPanel()
        {
            Popup = new DashboardPopup();
            Popup.Parent = ((this.Parent as DragCanvas).Parent as ScrollViewer).Parent as Grid;
//            Controls.GadgetProperties.MeansProperties properties = new Controls.GadgetProperties.MeansProperties(this.DashboardHelper, this, (MeansParameters)Parameters);
            properties = new Controls.GadgetProperties.KaplanMeierProperties(this.DashboardHelper, this, (KaplanMeierParameters)Parameters);

            if (DashboardHelper.ResizedWidth != 0 & DashboardHelper.ResizedHeight != 0)
            {
                double i_StandardHeight = System.Windows.SystemParameters.PrimaryScreenHeight;//Developer Desktop Width Where the Form is Designed
                double i_StandardWidth = System.Windows.SystemParameters.PrimaryScreenWidth; ////Developer Desktop Height Where the Form is Designed
                float f_HeightRatio = new float();
                float f_WidthRatio = new float();
                f_HeightRatio = (float)((float)DashboardHelper.ResizedHeight / (float)i_StandardHeight);
                f_WidthRatio = (float)((float)DashboardHelper.ResizedWidth / (float)i_StandardWidth);

                properties.Height = (Convert.ToInt32(i_StandardHeight * f_HeightRatio)) / 1.07;
                properties.Width = (Convert.ToInt32(i_StandardWidth * f_WidthRatio)) / 1.07;

            }
            else
            {
                properties.Width = (System.Windows.SystemParameters.PrimaryScreenWidth / 1.07);
                properties.Height = (System.Windows.SystemParameters.PrimaryScreenHeight / 1.15);
            }

            properties.Cancelled += new EventHandler(properties_Cancelled);
            properties.ChangesAccepted += new EventHandler(properties_ChangesAccepted);
            Popup.Content = properties;
            Popup.Show();
        }

        public override void GadgetBase_SizeChanged(double width, double height)
        {
            double i_StandardHeight = System.Windows.SystemParameters.PrimaryScreenHeight;//Developer Desktop Width Where the Form is Designed
            double i_StandardWidth = System.Windows.SystemParameters.PrimaryScreenWidth; ////Developer Desktop Height Where the Form is Designed
            float f_HeightRatio = new float();
            float f_WidthRatio = new float();
            f_HeightRatio = (float)((float)height / (float)i_StandardHeight);
            f_WidthRatio = (float)((float)width / (float)i_StandardWidth);

            if (properties == null)
                properties = new Controls.GadgetProperties.KaplanMeierProperties(this.DashboardHelper, this, (KaplanMeierParameters)Parameters);
            properties.Height = (Convert.ToInt32(i_StandardHeight * f_HeightRatio)) / 1.07;
            properties.Width = (Convert.ToInt32(i_StandardWidth * f_WidthRatio)) / 1.07;

        }

        private void properties_ChangesAccepted(object sender, EventArgs e)
        {
            Controls.GadgetProperties.KaplanMeierProperties properties = Popup.Content as Controls.GadgetProperties.KaplanMeierProperties;
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

            try
            {
                inputVariableList.Add("censor_variable", cbxFieldOutcome.SelectedItem.ToString());
				inputVariableList.Add("uncensored_value", cbxFieldUncensored.SelectedItem.ToString());
				inputVariableList.Add("time_unit", null);
				inputVariableList.Add("out_table", null);
				inputVariableList.Add("graph_type", null);
				inputVariableList.Add(cbxFieldOutcome.SelectedItem.ToString(), "dependvar");

				if (cbxFieldWeight.SelectedIndex >= 0 && !string.IsNullOrEmpty(cbxFieldWeight.SelectedItem.ToString()))
                {
                    inputVariableList.Add("weight_variable", cbxFieldWeight.SelectedItem.ToString());
                }
				else
				{
					inputVariableList.Add("weight_variable", null);
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

                if (cbxFieldMatch.SelectedIndex >= 0 && !string.IsNullOrEmpty(cbxFieldMatch.SelectedItem.ToString()))
                {
                    inputVariableList.Add("time_variable", cbxFieldMatch.SelectedItem.ToString());
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
                inputVariableList.Add("P", p.ToString());

				if (cbxFieldLink.SelectedItem != null)
				{
					inputVariableList.Add(cbxFieldLink.SelectedItem.ToString(), "linkfunction");
				}
				else
				{
					inputVariableList.Add("Logit", "linkfunction");
				}

				foreach (string s in lbxOtherFields.Items)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        inputVariableList.Add(s, "unsorted");
						inputVariableList.Add("group_variable", s);
					}
                }

                foreach (string s in lbxInteractionTerms.Items)
                {
                    inputVariableList.Add(s, "term");
                }

                foreach (string s in lbxDummyTerms.Items)
                {
                    inputVariableList.Add(s, "discrete");
                }
            }
            catch (ArgumentException)
            {
                grdParameters.Visibility = System.Windows.Visibility.Hidden;

                Epi.Windows.MsgBox.ShowError("The same variable cannot be used more than once.");
            }

            //return inputVariableList;
            GadgetOptions.InputVariableList = inputVariableList;
        }

        public override XmlNode Serialize(XmlDocument doc)
        {
            string dependVar = string.Empty;
			string uncensoredValue = string.Empty;
            string weightVar = string.Empty;
            string matchVar = string.Empty;
            string pvalue = string.Empty;
            bool intercept = true;

            if (cbxFieldOutcome.SelectedItem != null)
            {
                dependVar = cbxFieldOutcome.SelectedItem.ToString();
            }

			if (cbxFieldUncensored.SelectedItem != null)
				uncensoredValue = cbxFieldUncensored.SelectedItem.ToString();

            if (cbxFieldWeight.SelectedItem != null)
            {
                weightVar = cbxFieldWeight.SelectedItem.ToString();
            }

            if (cbxFieldMatch.SelectedItem != null)
            {
                matchVar = cbxFieldMatch.SelectedItem.ToString();
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
            "<mainVariable>" + dependVar + "</mainVariable>" +
            "<weightVariable>" + weightVar + "</weightVariable>" +
            "<matchVariable>" + matchVar + "</matchVariable>" +
			"<uncensoredValue>" + uncensoredValue + "</uncensoredValue>" +
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
            type.Value = "EpiDashboard.KaplanMeierControl";

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);
            element.Attributes.Append(id);

            if ((lbxDummyTerms.Items.Count + lbxOtherFields.Items.Count) > 0)
            {
                string xmlCovariateString = string.Empty;
                XmlElement covariateElement = doc.CreateElement("covariates");

                foreach (string s in lbxOtherFields.Items)
                {
                    xmlCovariateString = xmlCovariateString + "<covariate>" + s + "</covariate>";
                }

                foreach (string s in lbxDummyTerms.Items)
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

        public override void CreateFromXml(XmlElement element)
        {
            this.LoadingCombos = true;

			string uncensoredValue = String.Empty;
            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLowerInvariant())
                {
                    case "mainvariable":
                        cbxFieldOutcome.Text = child.InnerText;
                        break;
                    case "weightvariable":
                        cbxFieldWeight.Text = child.InnerText;
                        break;
                    case "matchvariable":
                        cbxFieldMatch.Text = child.InnerText;
                        break;
					case "uncensoredvalue":
						uncensoredValue = child.InnerText;
						break;
                    case "pvalue":
                        if (child.InnerText.Equals("90")) { cbxConf.SelectedIndex = 1; }
                        if (child.InnerText.Equals("95")) { cbxConf.SelectedIndex = 2; }
                        if (child.InnerText.Equals("99")) { cbxConf.SelectedIndex = 3; }
                        break;
                    case "intercept":
                        if (child.InnerText.ToLowerInvariant().Equals("false")) { checkboxNoIntercept.IsChecked = true; }
                        break;
                    case "covariates":
                        foreach (XmlElement covariate in child.ChildNodes)
                        {
                            if (covariate.Name.ToLowerInvariant().Equals("covariate"))
                            {
                                lbxOtherFields.Items.Add(covariate.InnerText);
								cbxFields.Text = covariate.InnerText;
								cbxFieldUncensored.Items.Add("");
								cbxFieldUncensored.Items.Add("0");
								cbxFieldUncensored.Items.Add("1");
								break;
                            }
                            if (covariate.Name.ToLowerInvariant().Equals("dummy"))
                            {
                                lbxDummyTerms.Items.Add(covariate.InnerText);
                            }
                            if (covariate.Name.ToLowerInvariant().Equals("interactionterm"))
                            {
                                lbxInteractionTerms.Items.Add(covariate.InnerText);
                            }
                        }
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
                }
			}
			cbxFieldUncensored.Text = uncensoredValue;

			foreach (XmlAttribute attribute in element.Attributes)
            {
                switch (attribute.Name.ToLowerInvariant())
                {
                    case "top":
                        Canvas.SetTop(this, double.Parse(attribute.Value));
                        break;
                    case "left":
                        Canvas.SetLeft(this, double.Parse(attribute.Value));
                        break;
                }
            }

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
            return "Kaplan-Meier Gadget";
        }
        #endregion

        private void lbxOtherFields_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lbxOtherFields.SelectedItems.Count == 1)
            {
                lbxOtherFields.Items.Remove(lbxOtherFields.SelectedItem);
            }
        }

        private void lbxDummyTerms_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lbxDummyTerms.SelectedItems.Count == 1)
            {
                lbxDummyTerms.Items.Remove(lbxDummyTerms.SelectedItem);
                lbxOtherFields.Items.Add(lbxDummyTerms.SelectedItem);
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
        public override string ToHTML(string htmlFileName = "", int count = 0, bool useAlternatingColors = false, bool ForWeb = false)
        {
            if (IsCollapsed) return string.Empty;

            StringBuilder htmlBuilder = new StringBuilder();
            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            if (CustomOutputHeading == null || (string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)")))
            {
                if (this.cbxFieldMatch.Text.Length <= 0)
                {
                    htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">Unconditional Logistic Regression</h2>");
                }
                else
                {
                    htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">Conditional Logistic Regression</h2>");
                }
            }
            else
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">" + CustomOutputHeading + "</h2>");
            }

            htmlBuilder.AppendLine("<p class=\"gadgetOptions\"><small>");
            htmlBuilder.AppendLine("<em>Censored variable:</em> <strong>" + cbxFieldOutcome.Text + "</strong>");
            htmlBuilder.AppendLine("<br />");

            if (cbxFieldWeight.SelectedIndex >= 0)
            {
                htmlBuilder.AppendLine("<em>Weight variable:</em> <strong>" + cbxFieldWeight.Text + "</strong>");
                htmlBuilder.AppendLine("<br />");
            }
            if (cbxFieldMatch.SelectedIndex >= 0)
            {
                htmlBuilder.AppendLine("<em>Time variable:</em> <strong>" + cbxFieldMatch.Text + "</strong>");
                htmlBuilder.AppendLine("<br />");
            }

            string varList = string.Empty;
            foreach(string s in this.lbxOtherFields.Items)
            {
                varList = varList + s + ",";
            }
            varList = varList.TrimEnd(',');

            htmlBuilder.AppendLine("<em>Test variable:</em> <strong>" + varList + "</strong>");
            htmlBuilder.AppendLine("<br />");

            varList = string.Empty;
            foreach (string s in this.lbxDummyTerms.Items)
            {
                varList = varList + s + ",";
            }
            varList = varList.TrimEnd(',');

            varList = string.Empty;
            foreach (string s in this.lbxInteractionTerms.Items)
            {
                varList = varList + s + ",";
            }
            varList = varList.TrimEnd(',');

            htmlBuilder.AppendLine("</small></p>");

            if (!string.IsNullOrEmpty(CustomOutputDescription))
            {
                htmlBuilder.AppendLine("<p class=\"gadgetsummary\">" + CustomOutputDescription + "</p>");
            }

            if (!string.IsNullOrEmpty(messagePanel.Text) && messagePanel.Visibility == System.Windows.Visibility.Visible)
            {
                htmlBuilder.AppendLine("<p><small><strong>" + messagePanel.Text + "</strong></small></p>");
            }

            if (txtFilterString != null && !string.IsNullOrEmpty(txtFilterString.Text) && txtFilterString.Visibility == Visibility.Visible)
            {
                htmlBuilder.AppendLine("<p><small><strong>" + txtFilterString.Text + "</strong></small></p> ");
            }

                htmlBuilder.AppendLine("<div style=\"height: 17px;\"></div>");
                htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
                htmlBuilder.AppendLine(" <tr>");
                htmlBuilder.AppendLine("  <th>Test</th>");
                htmlBuilder.AppendLine("  <th>Statistic</th>");
                htmlBuilder.AppendLine("  <th>D.F.</th>");
                htmlBuilder.AppendLine("  <th>P-Value</th>");
                htmlBuilder.AppendLine(" </tr>");
                htmlBuilder.AppendLine(" <tr>");
                htmlBuilder.AppendLine("  <th>Log-Rank Statistic</th>");
                htmlBuilder.AppendLine("  <td>" + txtScoreStatistic.Text + "</td>");
                htmlBuilder.AppendLine("  <td>" + txtScoreDF.Text + "</td>");
                htmlBuilder.AppendLine("  <td>" + txtScoreP.Text + "</td>");
                htmlBuilder.AppendLine(" </tr>");
                htmlBuilder.AppendLine(" <tr>");
                htmlBuilder.AppendLine("  <th>Wilcoxon</th>");
                htmlBuilder.AppendLine("  <td>" + txtLStatistic.Text + "</td>");
                htmlBuilder.AppendLine("  <td>" + txtLDF.Text + "</td>");
                htmlBuilder.AppendLine("  <td>" + txtLP.Text + "</td>");
                htmlBuilder.AppendLine(" </tr>");                
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

		private void xyChart_DataStructureCreated(object sender, EventArgs e)
		{
			return;
			ScatterChartParameters chtParameters = (ScatterChartParameters)Parameters;
			string sName = "";

			if (chtParameters.StrataVariableNames.Count > 0)
			{
				foreach (ComponentArt.Win.DataVisualization.Charting.Series s0 in xyChart.DataSeries)
				{
					sName = s0.Label.Split('.')[1];
					if (chtParameters.ShowLegendVarNames == false)
					{
						int index = sName.IndexOf(" = ");
						s0.Label = sName.Substring(index + 3);
					}
					else
					{
						s0.Label = sName;
					}
				}
			}
		}
	}
}
