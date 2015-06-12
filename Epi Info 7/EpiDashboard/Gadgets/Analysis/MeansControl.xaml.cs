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
using Epi.Fields;
using EpiDashboard.Rules;

namespace EpiDashboard
{
    /// <summary>
    /// Interaction logic for MeansControl.xaml
    /// </summary>
    /// <remarks>
    /// This gadget is used to generate descriptive statistics for a numeric variable in the database. It will return the mean, median, mode, standard
    /// deviation, variance, 25% value, 75% value, min value and max value. It also supports weights and can be both stratified and cross-tabulated.
    /// It is essentially a mirror of the MEANS command in the 'Classic' Analysis module.
    /// </remarks>
    public partial class MeansControl : GadgetBase
    {
        #region Private Members
        /// <summary>
        /// The list of labels for each strata value. E.g. if stratifying by SEX, there would be (likely)
        /// two values in this list: One for Male and one for Female. These show up as the table
        /// headings when displaying output.
        /// </summary>
        private List<TextBlock> gridLabelsList;

        /// <summary>
        /// A list of panels, one for each of the stratifcation values, and containing the ANOVA statistics
        /// </summary>
        private List<StackPanel> anovaBlocks;

        /// <summary>
        /// A custom heading to use for this gadget's output
        /// </summary>
        private string customOutputHeading;

        /// <summary>
        /// A custom description to use for this gadget's output
        /// </summary>
        private string customOutputDescription;

        /// <summary>
        /// A custom caption to use for this gadget's table/image output, if applicable
        /// </summary>
        private string customOutputCaption;
        #endregion // Private Members

        #region Delegates

        private new delegate void SetGridTextDelegate(string strataValue, TextBlockConfig textBlockConfig);
        private delegate void AddMeansGridDelegate(string strataVar, string value);
        private delegate void AddAnovaDelegate(string strataValue, DescriptiveStatistics stats);
        private delegate void RenderMeansHeaderDelegate(string strataValue, string meansVar, string crosstabVar); 
        private delegate void DrawMeansBordersDelegate(string strataValue); 
        #endregion // Delegates

        #region Events

        #endregion // Events

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public MeansControl()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper object to attach</param>
        public MeansControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            Construct();
            FillComboboxes();
        }

        #endregion // Constructors
        
        #region Event Handlers
        /// <summary>
        /// Handles the check / unchecked events
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void checkboxCheckChanged(object sender, RoutedEventArgs e)
        {
            //RefreshResults();
        }

        /// <summary>
        /// Fired when the user changes a field selection
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void cbxField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (!LoadingCombos)
            //{
            //    RefreshResults();
            //}
        }

        /// <summary>
        /// Fired when the user clicks the Run button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            if (LoadingCombos)
            {
                return;
            }

            //CheckVariables();
            RefreshResults();
        }

        /// <summary>
        /// Fired when the user changes a column selection
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void lbxColumns_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowHideOutputColumns();
        }

        /// <summary>
        /// Handles the WorkerCompleted event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected override void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            //Debug.Print("Background worker thread for means gadget was cancelled or ran to completion.");     
            System.Threading.Thread.Sleep(100);
            this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
        }

        /// <summary>
        /// Handles the DoWorker event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected override void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (syncLock)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));

                AddAnovaDelegate addAnova = new AddAnovaDelegate(AddAnova);
                AddMeansGridDelegate addGrid = new AddMeansGridDelegate(AddMeansGrid);
                SetGridTextDelegate setText = new SetGridTextDelegate(SetGridText);
                AddGridRowDelegate addRow = new AddGridRowDelegate(AddGridRow);
                RenderMeansHeaderDelegate renderHeader = new RenderMeansHeaderDelegate(RenderMeansHeader);
                DrawMeansBordersDelegate drawBorders = new DrawMeansBordersDelegate(DrawOutputGridBorders);

                //GadgetOptions.ShouldIgnoreRowLimits = true;
                //GadgetOptions.ShouldIncludeFullSummaryStatistics = true;                

                string precisionFormat = "F4";
                (Parameters as MeansParameters).IncludeFullSummaryStatistics = true;

                //if (GadgetOptions.InputVariableList.ContainsKey("precision"))
                //{
                //    precisionFormat = GadgetOptions.InputVariableList["precision"];
                //    precisionFormat = "F" + precisionFormat;
                //}

                string meansVar = Parameters.ColumnNames[0];
                string weightVar = (Parameters as MeansParameters).WeightVariableName;
                string strataVar = string.Empty;
                bool hasData = false;
                if ((Parameters as MeansParameters).StrataVariableNames != null && (Parameters as MeansParameters).StrataVariableNames.Count > 0)
                {
                    strataVar = (Parameters as MeansParameters).StrataVariableNames[0];
                }
                
                bool showAnova = true;
                //if (GadgetOptions.InputVariableList.ContainsKey("showanova"))
                //{
                //    bool.TryParse(GadgetOptions.InputVariableList["showanova"], out showAnova);
                //}

                string crosstabVar = (Parameters as MeansParameters).CrosstabVariableName;

                //if (string.IsNullOrEmpty(crosstabVar))
                //{
                //    GadgetOptions.InputVariableList.Add("NeedsOutputGrid", "false");
                //}
                //else
                //{
                //    GadgetOptions.InputVariableList.Add("NeedsOutputGrid", "true");
                //}

                List<string> stratas = new List<string>();
                if (!string.IsNullOrEmpty(strataVar))
                {
                    stratas.Add(strataVar);
                }
                try
                {
                    RequestUpdateStatusDelegate requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
                    CheckForCancellationDelegate checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

                    //GadgetOptions.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                    //GadgetOptions.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);

                    //if (this.DataFilters != null && this.DataFilters.Count > 0)
                    //{
                    //    GadgetOptions.CustomFilter = this.DataFilters.GenerateDataFilterString(false);
                    //}
                    //else
                    //{
                    //    GadgetOptions.CustomFilter = string.Empty;
                    //}

                    Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables = DashboardHelper.GenerateFrequencyTable(Parameters);
                    if (stratifiedFrequencyTables == null || stratifiedFrequencyTables.Count == 0)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), DashboardSharedStrings.GADGET_MSG_NO_DATA);
                        //this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        return;
                    }
                    else if (worker.CancellationPending)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.DASHBOARD_GADGET_STATUS_OPERATION_CANCELLED);
                        //this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        Debug.Print("Thread cancelled");
                        return;
                    }
                    else
                    {
                        //bool useSpecialFormatting = false;
                        string formatString = string.Empty;
                        StrataCount = stratifiedFrequencyTables.Count;

                        foreach (KeyValuePair<DataTable, List<DescriptiveStatistics>> tableKvp in stratifiedFrequencyTables)
                        {
                            string strataValue = tableKvp.Key.TableName;

                            double count = 0;
                            foreach (DescriptiveStatistics ds in tableKvp.Value)
                            {
                                count = count + ds.observations;
                            }

                            if (count == 0)
                            {
                                continue;
                            }
                            DataTable frequencies = tableKvp.Key; //dashboardHelper.GenerateFrequencyTable(freqVar, weightVar, out count);

                            if (frequencies.Rows.Count == 0)
                            {
                                continue;
                            }

                            this.Dispatcher.BeginInvoke(addGrid, strataVar, frequencies.TableName);
                            if (showAnova && tableKvp.Value[0].anovaPValue.HasValue)
                            {
                                this.Dispatcher.BeginInvoke(addAnova, strataValue, tableKvp.Value[0]);
                            }
                        }

                        //if (GadgetStatusUpdate != null)
                        //{
                        //    this.Dispatcher.BeginInvoke(GadgetStatusUpdate, SharedStrings.DASHBOARD_GADGET_STATUS_DISPLAYING_OUTPUT);
                        //    System.Threading.Thread.Sleep(50); // necessary to prevent this message from overriding the 'no records selected'
                        //}
                        
                        foreach (KeyValuePair<DataTable, List<DescriptiveStatistics>> tableKvp in stratifiedFrequencyTables)
                        {
                            string strataValue = tableKvp.Key.TableName;

                            double count = 0;
                            foreach (DescriptiveStatistics ds in tableKvp.Value)
                            {
                                count = count + ds.observations;
                            }

                            if (count == 0)
                            {
                                continue;
                            }
                            else
                            {
                                hasData = true;
                            }

                            DataTable frequencies = tableKvp.Key; //dashboardHelper.GenerateFrequencyTable(freqVar, weightVar, out count);                    

                            if (frequencies.Rows.Count == 0)
                            {
                                continue;
                            }

                            string tableHeading = tableKvp.Key.TableName;

                            if (!string.IsNullOrEmpty(crosstabVar))
                            {
                                tableHeading = meansVar + " * " + crosstabVar;// +": " + frequencies.TableName;
                            }

                            this.Dispatcher.BeginInvoke(renderHeader, strataValue, tableHeading, crosstabVar);
                            int rowCount = 1;

                            foreach (System.Data.DataColumn column in frequencies.Columns)
                            {
                                if (worker.CancellationPending)
                                {
                                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.DASHBOARD_GADGET_STATUS_OPERATION_CANCELLED);
                                    //this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                                    Debug.Print("Thread cancelled");
                                    return;
                                }
                                string columnName = column.ColumnName;
                                if (columnName.Equals("___sortvalue___"))
                                {
                                    continue;
                                }
                                
                                DescriptiveStatistics means = tableKvp.Value[rowCount - 1];

                                if (!columnName.ToLower().Equals(meansVar.ToLower()))
                                {
                                    this.Dispatcher.Invoke(addRow, strataValue, 30);
                                    string displayValue = meansVar;
                                    if (tableKvp.Value.Count > 1)
                                    {
                                        displayValue = columnName;
                                    }

                                    string statsObs = means.observations.ToString(precisionFormat);
                                    string statsSum = Epi.SharedStrings.UNDEFINED;
                                    string statsMean = Epi.SharedStrings.UNDEFINED;
                                    string statsVar = Epi.SharedStrings.UNDEFINED;
                                    string statsStdDev = Epi.SharedStrings.UNDEFINED;
                                    string statsMin = Epi.SharedStrings.UNDEFINED;
                                    string statsMax = Epi.SharedStrings.UNDEFINED;
                                    string statsMedian = Epi.SharedStrings.UNDEFINED;
                                    string statsMode = Epi.SharedStrings.UNDEFINED;
                                    string statsQ1 = Epi.SharedStrings.UNDEFINED;
                                    string statsQ3 = Epi.SharedStrings.UNDEFINED;

                                    if (Convert.ToInt32(((MeansParameters)Parameters).Precision) >= 0)
                                    {
                                        precisionFormat = "F" + ((MeansParameters)Parameters).Precision;
                                    }

                                    if (means.sum != null)
                                        statsSum = ((double)means.sum).ToString(precisionFormat);
                                    if (means.mean != null)
                                        statsMean = ((double)means.mean).ToString(precisionFormat);
                                    if (means.variance != null)
                                        statsVar = ((double)means.variance).ToString(precisionFormat);
                                    if (means.stdDev != null)
                                        statsStdDev = ((double)means.stdDev).ToString(precisionFormat);
                                    if (means.min != null)
                                        statsMin = ((double)means.min).ToString(precisionFormat);
                                    if (means.q1 != null)
                                        statsQ1 = ((double)means.q1).ToString(precisionFormat);
                                    if (means.median != null)
                                        statsMedian = ((double)means.median).ToString(precisionFormat);
                                    if (means.q3 != null)
                                        statsQ3 = ((double)means.q3).ToString(precisionFormat);
                                    if (means.max != null)
                                        statsMax = ((double)means.max).ToString(precisionFormat);
                                    if (means.mode != null)
                                        statsMode = ((double)means.mode).ToString(precisionFormat);

                                    if (statsObs.EndsWith("0") && (statsObs.Contains(".") || statsObs.Contains(",")))
                                    {
                                        statsObs = statsObs.TrimEnd('0').TrimEnd('.').TrimEnd(',');
                                    }

                                    if (statsSum.EndsWith("0") && (statsSum.Contains(".") || statsSum.Contains(",")))
                                    {
                                        statsSum = statsSum.TrimEnd('0').TrimEnd('.').TrimEnd(',');
                                    }

                                    this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(displayValue, new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Left, TextAlignment.Left, rowCount, 0, Visibility.Visible));
                                    this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(statsObs, new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 1, Visibility.Visible));
                                    this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(statsSum, new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 2, Visibility.Visible));
                                    this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(statsMean, new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 3, Visibility.Visible));
                                    this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(statsVar, new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 4, Visibility.Visible));
                                    this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(statsStdDev, new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 5, Visibility.Visible));
                                    this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(statsMin, new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 6, Visibility.Visible));
                                    this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(statsQ1, new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 7, Visibility.Visible));
                                    this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(statsMedian, new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 8, Visibility.Visible));
                                    this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(statsQ3, new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 9, Visibility.Visible));
                                    this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(statsMax, new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 10, Visibility.Visible));
                                    this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(statsMode, new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 11, Visibility.Visible));

                                    rowCount++;
                                }
                            }
                            this.Dispatcher.BeginInvoke(drawBorders, strataValue);                           
                        }
                    }

                    if (!hasData)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), DashboardSharedStrings.GADGET_MSG_NO_DATA);
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));
                    }
                    //this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
                    //this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                }

                stopwatch.Stop();
                Debug.Print("Means gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete with " + DashboardHelper.RecordCount.ToString() + " records and the following filters:");
                Debug.Print(DashboardHelper.DataFilters.GenerateDataFilterString());
            }
        }

        #endregion
               
        #region Public Methods
        /// <summary>
        /// Returns the gadget's description as a string.
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return "Means Gadget";
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Enables and disables output columns based on user selection.
        /// </summary>
        private void ShowHideOutputColumns()
        {
            if (!LoadingCombos)
            {
                if (this.StrataGridList != null && this.StrataGridList.Count > 0)
                {
                    List<int> columnsToShow = new List<int>();
                    if (!lbxColumns.SelectedItems.Contains("Observations")) columnsToShow.Add(1);
                    if (!lbxColumns.SelectedItems.Contains("Total")) columnsToShow.Add(2);
                    if (!lbxColumns.SelectedItems.Contains("Mean")) columnsToShow.Add(3);
                    if (!lbxColumns.SelectedItems.Contains("Variance")) columnsToShow.Add(4);
                    if (!lbxColumns.SelectedItems.Contains("Std. Dev.")) columnsToShow.Add(5);
                    if (!lbxColumns.SelectedItems.Contains("Minimum")) columnsToShow.Add(6);
                    if (!lbxColumns.SelectedItems.Contains("25%")) columnsToShow.Add(7);
                    if (!lbxColumns.SelectedItems.Contains("Median")) columnsToShow.Add(8);
                    if (!lbxColumns.SelectedItems.Contains("75%")) columnsToShow.Add(9);
                    if (!lbxColumns.SelectedItems.Contains("Maximum")) columnsToShow.Add(10);
                    if (!lbxColumns.SelectedItems.Contains("Mode")) columnsToShow.Add(11);

                    foreach (Grid grid in this.StrataGridList)
                    {
                        for (int i = 0; i < grid.ColumnDefinitions.Count; i++)
                        {
                            if (((MeansParameters)Parameters).columnsToHide.Contains(i))
                            {
                                grid.ColumnDefinitions[i].Width = new GridLength(0);
                            }
                            else
                            {
                                //if (i == 6)
                                //{
                                //    grid.ColumnDefinitions[i].Width = new GridLength(100);
                                //}
                                //else
                                //{
                                    grid.ColumnDefinitions[i].Width = new GridLength(1, GridUnitType.Auto);
                                //}
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Copies a grid's output to the clipboard
        /// </summary>
        protected override void CopyToClipboard()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Grid grid in this.StrataGridList)
            {
                string gridName = grid.Tag.ToString();

                if (StrataGridList.Count > 1)
                {
                    sb.AppendLine(grid.Tag.ToString());
                }

                foreach (UIElement control in grid.Children)
                {
                    if (control is TextBlock)
                    {
                        int columnNumber = Grid.GetColumn(control);
                        string value = ((TextBlock)control).Text;

                        sb.Append(value + "\t");

                        if (columnNumber >= grid.ColumnDefinitions.Count - 1)
                        {
                            sb.AppendLine();
                        }
                    }
                }

                sb.AppendLine();
            }

            Clipboard.Clear();
            Clipboard.SetText(sb.ToString());
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

            for (int i = 0; i < StrataGridList.Count; i++)
            {
                StrataGridList[i].Children.Clear();
            }
            for (int i = 0; i < anovaBlocks.Count; i++)
            {
                anovaBlocks[i].Children.Clear();
            }
            
            this.StrataGridList.Clear();
            this.anovaBlocks.Clear();
            this.panelMain.Children.Clear();

            base.CloseGadget();

            GadgetOptions = null;
        }

        /// <summary>
        /// Clears the gadget's output
        /// </summary>
        private void ClearResults()
        {
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Text = string.Empty;
            descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;

            foreach (Grid grid in StrataGridList)
            {
                grid.Children.Clear();
                grid.RowDefinitions.Clear();                
            }

            foreach (Expander expander in StrataExpanderList)
            {
                expander.Content = null;
                if (panelMain.Children.Contains(expander))
                {
                    panelMain.Children.Remove(expander);
                }
            }

            //foreach (StackPanel anovaBlock in anovaBlocks)
            //{
            //    panelMain.Children.Remove(anovaBlock);
            //}

            panelMain.Children.Clear();
            StrataCount = 0;

            StrataGridList.Clear();
            StrataExpanderList.Clear();
            anovaBlocks.Clear();
        }

        /// <summary>
        /// Handles the filling of the gadget's combo boxes
        /// </summary>
        private void FillComboboxes(bool update = false)
        {
            LoadingCombos = true;

            string prevField = string.Empty;
            string prevWeightField = string.Empty;
            //string prevStrataField = string.Empty;
            List<string> prevStrataFields = new List<string>();
            string prevCrosstabField = string.Empty;

            if (lbxColumns.Items.Count == 0)
            {
                lbxColumns.Items.Add("Observations");
                lbxColumns.Items.Add("Total");
                lbxColumns.Items.Add("Mean");
                lbxColumns.Items.Add("Variance");
                lbxColumns.Items.Add("Std. Dev.");
                lbxColumns.Items.Add("Minimum");
                lbxColumns.Items.Add("25%");
                lbxColumns.Items.Add("Median");
                lbxColumns.Items.Add("75%");
                lbxColumns.Items.Add("Maximum");
                lbxColumns.Items.Add("Mode");
                lbxColumns.SelectAll();
            }

            if (update)
            {
                if (cbxField.SelectedIndex >= 0)
                {
                    prevField = cbxField.SelectedItem.ToString();
                }
                if (cbxFieldWeight.SelectedIndex >= 0)
                {
                    prevWeightField = cbxFieldWeight.SelectedItem.ToString();
                }
                //if (cbxFieldStrata.SelectedIndex >= 0)
                //{
                //    prevStrataField = cbxFieldStrata.SelectedItem.ToString();
                //}
                foreach (string s in lbxFieldStrata.SelectedItems)
                {
                    prevStrataFields.Add(s);
                }
                if (cbxFieldCrosstab.SelectedIndex >= 0)
                {
                    prevCrosstabField = cbxFieldCrosstab.SelectedItem.ToString();
                }
            }

            cbxField.ItemsSource = null;
            cbxField.Items.Clear();

            cbxFieldWeight.ItemsSource = null;
            cbxFieldWeight.Items.Clear();

            //cbxFieldStrata.ItemsSource = null;
            //cbxFieldStrata.Items.Clear();

            lbxFieldStrata.ItemsSource = null;
            lbxFieldStrata.Items.Clear();

            cbxFieldCrosstab.ItemsSource = null;
            cbxFieldCrosstab.Items.Clear();

            List<string> fieldNames = new List<string>();
            List<string> weightFieldNames = new List<string>();
            List<string> strataFieldNames = new List<string>();
            List<string> crosstabFieldNames = new List<string>();

            weightFieldNames.Add(string.Empty);
            //strataFieldNames.Add(string.Empty);
            crosstabFieldNames.Add(string.Empty);
            
            ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.Numeric | ColumnDataType.UserDefined;
            fieldNames = DashboardHelper.GetFieldsAsList(columnDataType);

            columnDataType = ColumnDataType.Numeric | ColumnDataType.UserDefined;
            weightFieldNames.AddRange(DashboardHelper.GetFieldsAsList(columnDataType));

            columnDataType = ColumnDataType.Numeric | ColumnDataType.Boolean | ColumnDataType.Text | ColumnDataType.UserDefined;
            strataFieldNames.AddRange(DashboardHelper.GetFieldsAsList(columnDataType));

            columnDataType = ColumnDataType.Numeric | ColumnDataType.Boolean | ColumnDataType.Text | ColumnDataType.UserDefined;
            crosstabFieldNames.AddRange(DashboardHelper.GetFieldsAsList(columnDataType));  

            fieldNames.Sort();
            weightFieldNames.Sort();
            strataFieldNames.Sort();
            crosstabFieldNames.Sort();

            if (DashboardHelper.IsUsingEpiProject)
            {
                if (fieldNames.Contains("RecStatus")) fieldNames.Remove("RecStatus");
                if (fieldNames.Contains("FKEY")) fieldNames.Remove("FKEY");
                if (fieldNames.Contains("GlobalRecordId")) fieldNames.Remove("GlobalRecordId");
                if (fieldNames.Contains("FirstSaveTime")) fieldNames.Remove("FirstSaveTime");
                if (fieldNames.Contains("LastSaveTime")) fieldNames.Remove("LastSaveTime");

                if (weightFieldNames.Contains("RecStatus")) weightFieldNames.Remove("RecStatus");
                if (weightFieldNames.Contains("FKEY")) weightFieldNames.Remove("FKEY");
                if (weightFieldNames.Contains("GlobalRecordId")) weightFieldNames.Remove("GlobalRecordId");
                if (weightFieldNames.Contains("FirstSaveTime")) weightFieldNames.Remove("FirstSaveTime");
                if (weightFieldNames.Contains("LastSaveTime")) weightFieldNames.Remove("LastSaveTime");

                if (strataFieldNames.Contains("RecStatus")) strataFieldNames.Remove("RecStatus");
                if (strataFieldNames.Contains("FKEY")) strataFieldNames.Remove("FKEY");
                if (strataFieldNames.Contains("GlobalRecordId")) strataFieldNames.Remove("GlobalRecordId");
                if (strataFieldNames.Contains("FirstSaveTime")) strataFieldNames.Remove("FirstSaveTime");
                if (strataFieldNames.Contains("LastSaveTime")) strataFieldNames.Remove("LastSaveTime");

                if (crosstabFieldNames.Contains("RecStatus")) crosstabFieldNames.Remove("RecStatus");
                if (crosstabFieldNames.Contains("FKEY")) crosstabFieldNames.Remove("FKEY");
                if (crosstabFieldNames.Contains("GlobalRecordId")) crosstabFieldNames.Remove("GlobalRecordId");
                if (crosstabFieldNames.Contains("FirstSaveTime")) crosstabFieldNames.Remove("FirstSaveTime");
                if (crosstabFieldNames.Contains("LastSaveTime")) crosstabFieldNames.Remove("LastSaveTime");
            }

            cbxField.ItemsSource = fieldNames;
            cbxFieldWeight.ItemsSource = weightFieldNames;
            //cbxFieldStrata.ItemsSource = strataFieldNames;
            lbxFieldStrata.ItemsSource = strataFieldNames;
            cbxFieldCrosstab.ItemsSource = crosstabFieldNames;

            if (cbxField.Items.Count > 0)
            {
                cbxField.SelectedIndex = -1;
            }
            if (cbxFieldWeight.Items.Count > 0)
            {
                cbxFieldWeight.SelectedIndex = -1;
            }
            //if (cbxFieldStrata.Items.Count > 0)
            //{
            //    cbxFieldStrata.SelectedIndex = -1;
            //}
            if (cbxFieldCrosstab.Items.Count > 0)
            {
                cbxFieldCrosstab.SelectedIndex = -1;
            }

            if (update)
            {
                cbxField.SelectedItem = prevField;
                cbxFieldWeight.SelectedItem = prevWeightField;
                //cbxFieldStrata.SelectedItem = prevStrataField;

                foreach (string s in prevStrataFields)
                {
                    lbxFieldStrata.SelectedItems.Add(s);
                }

                cbxFieldCrosstab.SelectedItem = prevCrosstabField;
            }

            LoadingCombos = false;
        }

        /// <summary>
        /// Adds anova statistics to the gadget
        /// </summary>
        /// <param name="strataValue">The strata value associated with the results</param>
        /// <param name="stats">The descriptive statistics to process</param>
        private void AddAnova(string strataValue, DescriptiveStatistics stats)
        {
            if (stats.crosstabs == 2)
            {
                Expander tTestExpander = new Expander();
                tTestExpander.Margin = (Thickness)this.Resources["expanderMargin"];
                tTestExpander.IsExpanded = true;

                StackPanel pnlT = new StackPanel();

                tTestExpander.Content = pnlT;

                if (this.StrataCount > 1)
                {
                    Expander expander = GetStrataExpander(strataValue);
                    if (expander.Content is StackPanel)
                    {
                        StackPanel sPanel = expander.Content as StackPanel;
                        sPanel.Children.Add(tTestExpander);
                    }
                }
                else
                {
                    panelMain.Children.Add(tTestExpander);
                }

                    KeyValuePair<string, DescriptiveStatistics> TTestStatistics = new KeyValuePair<string, DescriptiveStatistics>(strataValue, stats);
                    pnlT.Tag = TTestStatistics;
                    pnlT.HorizontalAlignment = HorizontalAlignment.Center;
                    pnlT.VerticalAlignment = VerticalAlignment.Center;
                    pnlT.Margin = new Thickness(5);
                    anovaBlocks.Add(pnlT);

                    TextBlock txtT1 = new TextBlock();
                    txtT1.Text = "T-Test";
                    txtT1.HorizontalAlignment = HorizontalAlignment.Center;
                    txtT1.Margin = new Thickness(5);
                    txtT1.FontWeight = FontWeights.Bold;
                    tTestExpander.Header = txtT1;

                    Grid tGrid1 = new Grid();
                    tGrid1.SnapsToDevicePixels = true;
                    tGrid1.HorizontalAlignment = HorizontalAlignment.Center;
                    tGrid1.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
                    tGrid1.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
                    tGrid1.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
                    tGrid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    tGrid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    tGrid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    tGrid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    tGrid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    tGrid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                    pnlT.Children.Add(tGrid1);

                    for (int y = 0; y < tGrid1.ColumnDefinitions.Count; y++)
                    {
                        Rectangle rctHeader = new Rectangle();
                        rctHeader.Style = this.Resources["gridHeaderCellRectangle"] as Style;
                        Grid.SetRow(rctHeader, 0);
                        Grid.SetColumn(rctHeader, y);
                        tGrid1.Children.Add(rctHeader);
                    }

                    TextBlock tDiffPooled = new TextBlock();
                    tDiffPooled.Text = "Diff (Group 1 - Group 2)";
                    tDiffPooled.Margin = new Thickness(4, 0, 4, 0);
                    tDiffPooled.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    tDiffPooled.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid1.Children.Add(tDiffPooled);
                    Grid.SetRow(tDiffPooled, 1);
                    Grid.SetColumn(tDiffPooled, 0);

                    TextBlock tDiffSatterthwaite = new TextBlock();
                    tDiffSatterthwaite.Text = "Diff (Group 1 - Group 2)";
                    tDiffSatterthwaite.Margin = new Thickness(4, 0, 4, 0);
                    tDiffSatterthwaite.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    tDiffSatterthwaite.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid1.Children.Add(tDiffSatterthwaite);
                    Grid.SetRow(tDiffSatterthwaite, 2);
                    Grid.SetColumn(tDiffSatterthwaite, 0);

                    TextBlock methodHeader = new TextBlock();
                    methodHeader.Text = "Method";
                    methodHeader.Margin = new Thickness(4, 0, 4, 0);
                    methodHeader.FontWeight = FontWeights.Bold;
                    methodHeader.Foreground = Brushes.White;
                    methodHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    methodHeader.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid1.Children.Add(methodHeader);
                    Grid.SetRow(methodHeader, 0);
                    Grid.SetColumn(methodHeader, 1);

                    TextBlock meanHeader = new TextBlock();
                    meanHeader.Text = "Mean";
                    meanHeader.Margin = new Thickness(4, 0, 4, 0);
                    meanHeader.FontWeight = FontWeights.Bold;
                    meanHeader.Foreground = Brushes.White;
                    meanHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    meanHeader.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid1.Children.Add(meanHeader);
                    Grid.SetRow(meanHeader, 0);
                    Grid.SetColumn(meanHeader, 2);

                    TextBlock clHeader = new TextBlock();
                    clHeader.Text = "95%";
                    clHeader.Margin = new Thickness(4, 0, 4, 0);
                    clHeader.FontWeight = FontWeights.Bold;
                    clHeader.Foreground = Brushes.White;
                    clHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    clHeader.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid1.Children.Add(clHeader);
                    Grid.SetRow(clHeader, 0);
                    Grid.SetColumn(clHeader, 3);

                    TextBlock uclHeader = new TextBlock();
                    uclHeader.Text = "CL";
                    uclHeader.Margin = new Thickness(4, 0, 4, 0);
                    uclHeader.FontWeight = FontWeights.Bold;
                    uclHeader.Foreground = Brushes.White;
                    uclHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    uclHeader.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid1.Children.Add(uclHeader);
                    Grid.SetRow(uclHeader, 0);
                    Grid.SetColumn(uclHeader, 4);

                    TextBlock stdDevHeader = new TextBlock();
                    stdDevHeader.Text = "StdDev";
                    stdDevHeader.Margin = new Thickness(4, 0, 4, 0);
                    stdDevHeader.FontWeight = FontWeights.Bold;
                    stdDevHeader.Foreground = Brushes.White;
                    stdDevHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    stdDevHeader.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid1.Children.Add(stdDevHeader);
                    Grid.SetRow(stdDevHeader, 0);
                    Grid.SetColumn(stdDevHeader, 5);

                    TextBlock txtMethod = new TextBlock();
                    txtMethod.Text = "Pooled";
                    txtMethod.Margin = new Thickness(4, 0, 4, 0);
                    txtMethod.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    txtMethod.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid1.Children.Add(txtMethod);
                    Grid.SetRow(txtMethod, 1);
                    Grid.SetColumn(txtMethod, 1);

                    String precisionFormat = "N4";

                    if (Convert.ToInt32(((MeansParameters)Parameters).Precision) >= 0)
                    {
                        precisionFormat = "N" + ((MeansParameters)Parameters).Precision;
                    }


                    TextBlock txtMeanP = new TextBlock();
                    txtMeanP.Text = stats.meansDiff.ToString(precisionFormat);
                    txtMeanP.Margin = new Thickness(4, 0, 4, 0);
                    txtMeanP.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    txtMeanP.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid1.Children.Add(txtMeanP);
                    Grid.SetRow(txtMeanP, 1);
                    Grid.SetColumn(txtMeanP, 2);

                    TextBlock txtLCLP = new TextBlock();
                    txtLCLP.Text = stats.equalLCLMean.ToString(precisionFormat);
                    txtLCLP.Margin = new Thickness(4, 0, 4, 0);
                    txtLCLP.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    txtLCLP.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid1.Children.Add(txtLCLP);
                    Grid.SetRow(txtLCLP, 1);
                    Grid.SetColumn(txtLCLP, 3);

                    TextBlock txtUCLP = new TextBlock();
                    txtUCLP.Text = stats.equalUCLMean.ToString(precisionFormat);
                    txtUCLP.Margin = new Thickness(4, 0, 4, 0);
                    txtUCLP.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    txtUCLP.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid1.Children.Add(txtUCLP);
                    Grid.SetRow(txtUCLP, 1);
                    Grid.SetColumn(txtUCLP, 4);

                    TextBlock txtStdDev = new TextBlock();
                    txtStdDev.Text = stats.stdDevDiff.ToString(precisionFormat);
                    txtStdDev.Margin = new Thickness(4, 0, 4, 0);
                    txtStdDev.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    txtStdDev.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid1.Children.Add(txtStdDev);
                    Grid.SetRow(txtStdDev, 1);
                    Grid.SetColumn(txtStdDev, 5);

                    TextBlock txtMethodS = new TextBlock();
                    txtMethodS.Text = "Satterthwaite";
                    txtMethodS.Margin = new Thickness(4, 0, 4, 0);
                    txtMethodS.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    txtMethodS.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid1.Children.Add(txtMethodS);
                    Grid.SetRow(txtMethodS, 21);
                    Grid.SetColumn(txtMethodS, 1);

                    TextBlock txtMeanS = new TextBlock();
                    txtMeanS.Text = stats.meansDiff.ToString(precisionFormat);
                    txtMeanS.Margin = new Thickness(4, 0, 4, 0);
                    txtMeanS.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    txtMeanS.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid1.Children.Add(txtMeanS);
                    Grid.SetRow(txtMeanS, 2);
                    Grid.SetColumn(txtMeanS, 2);

                    TextBlock txtLCLS = new TextBlock();
                    txtLCLS.Text = stats.unequalLCLMean.ToString(precisionFormat);
                    txtLCLS.Margin = new Thickness(4, 0, 4, 0);
                    txtLCLS.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    txtLCLS.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid1.Children.Add(txtLCLS);
                    Grid.SetRow(txtLCLS, 2);
                    Grid.SetColumn(txtLCLS, 3);

                    TextBlock txtUCLS = new TextBlock();
                    txtUCLS.Text = stats.unequalUCLMean.ToString(precisionFormat);
                    txtUCLS.Margin = new Thickness(4, 0, 4, 0);
                    txtUCLS.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    txtUCLS.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid1.Children.Add(txtUCLS);
                    Grid.SetRow(txtUCLS, 2);
                    Grid.SetColumn(txtUCLS, 4);

                    int rdcountT = 0;
                    foreach (RowDefinition rd in tGrid1.RowDefinitions)
                    {
                        int cdcount = 0;
                        foreach (ColumnDefinition cd in tGrid1.ColumnDefinitions)
                        {
                            Border b = new Border();
                            b.Style = this.Resources["gridCellBorder"] as Style;

                            if (rdcountT == 0)
                            {
                                b.BorderThickness = new Thickness(b.BorderThickness.Left, 1, b.BorderThickness.Right, b.BorderThickness.Bottom);
                            }
                            if (cdcount == 0)
                            {
                                b.BorderThickness = new Thickness(1, b.BorderThickness.Top, b.BorderThickness.Right, b.BorderThickness.Bottom);
                            }
                            if (cdcount == 3 && rdcountT == 0)
                            {
                                b.BorderThickness = new Thickness(b.BorderThickness.Left, 1, 0, b.BorderThickness.Bottom);
                            }

                            Grid.SetRow(b, rdcountT);
                            Grid.SetColumn(b, cdcount);
                            tGrid1.Children.Add(b);
                            cdcount++;
                        }
                        rdcountT++;
                    }

                    Grid tGrid2 = new Grid();
                    tGrid2.Margin = new Thickness(5);
                    tGrid2.SnapsToDevicePixels = true;
                    tGrid2.HorizontalAlignment = HorizontalAlignment.Center;
                    tGrid2.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
                    tGrid2.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
                    tGrid2.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
                    tGrid2.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    tGrid2.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    tGrid2.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    tGrid2.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    tGrid2.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                    pnlT.Children.Add(tGrid2);

                    for (int y = 0; y < tGrid2.ColumnDefinitions.Count; y++)
                    {
                        Rectangle rctHeader = new Rectangle();
                        rctHeader.Style = this.Resources["gridHeaderCellRectangle"] as Style;
                        Grid.SetRow(rctHeader, 0);
                        Grid.SetColumn(rctHeader, y);
                        tGrid2.Children.Add(rctHeader);
                    }

                    TextBlock methodHeaderG2 = new TextBlock();
                    methodHeaderG2.Text = "Method";
                    methodHeaderG2.Margin = new Thickness(4, 0, 4, 0);
                    methodHeaderG2.FontWeight = FontWeights.Bold;
                    methodHeaderG2.Foreground = Brushes.White;
                    methodHeaderG2.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    methodHeaderG2.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid2.Children.Add(methodHeaderG2);
                    Grid.SetRow(methodHeaderG2, 0);
                    Grid.SetColumn(methodHeaderG2, 0);

                    TextBlock variancesHeader = new TextBlock();
                    variancesHeader.Text = "Variances";
                    variancesHeader.Margin = new Thickness(4, 0, 4, 0);
                    variancesHeader.FontWeight = FontWeights.Bold;
                    variancesHeader.Foreground = Brushes.White;
                    variancesHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    variancesHeader.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid2.Children.Add(variancesHeader);
                    Grid.SetRow(variancesHeader, 0);
                    Grid.SetColumn(variancesHeader, 1);

                    TextBlock dfHeader = new TextBlock();
                    dfHeader.Text = "DF";
                    dfHeader.Margin = new Thickness(4, 0, 4, 0);
                    dfHeader.FontWeight = FontWeights.Bold;
                    dfHeader.Foreground = Brushes.White;
                    dfHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    dfHeader.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid2.Children.Add(dfHeader);
                    Grid.SetRow(dfHeader, 0);
                    Grid.SetColumn(dfHeader, 2);

                    TextBlock tValueHeader = new TextBlock();
                    tValueHeader.Text = "t Value";
                    tValueHeader.Margin = new Thickness(4, 0, 4, 0);
                    tValueHeader.FontWeight = FontWeights.Bold;
                    tValueHeader.Foreground = Brushes.White;
                    tValueHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    tValueHeader.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid2.Children.Add(tValueHeader);
                    Grid.SetRow(tValueHeader, 0);
                    Grid.SetColumn(tValueHeader, 3);

                    TextBlock prHeader = new TextBlock();
                    prHeader.Text = "Pr > |t|";
                    prHeader.Margin = new Thickness(4, 0, 4, 0);
                    prHeader.FontWeight = FontWeights.Bold;
                    prHeader.Foreground = Brushes.White;
                    prHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    prHeader.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid2.Children.Add(prHeader);
                    Grid.SetRow(prHeader, 0);
                    Grid.SetColumn(prHeader, 4);

                    TextBlock txtPooled = new TextBlock();
                    txtPooled.Text = "Pooled";
                    txtPooled.Margin = new Thickness(4, 0, 4, 0);
                    txtPooled.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    txtPooled.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid2.Children.Add(txtPooled);
                    Grid.SetRow(txtPooled, 1);
                    Grid.SetColumn(txtPooled, 0);

                    TextBlock txtSatterthwaite = new TextBlock();
                    txtSatterthwaite.Text = "Satterthwaite";
                    txtSatterthwaite.Margin = new Thickness(4, 0, 4, 0);
                    txtSatterthwaite.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    txtSatterthwaite.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid2.Children.Add(txtSatterthwaite);
                    Grid.SetRow(txtSatterthwaite, 2);
                    Grid.SetColumn(txtSatterthwaite, 0);

                    TextBlock txtEqual = new TextBlock();
                    txtEqual.Text = "Equal";
                    txtEqual.Margin = new Thickness(4, 0, 4, 0);
                    txtEqual.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    txtEqual.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid2.Children.Add(txtEqual);
                    Grid.SetRow(txtEqual, 1);
                    Grid.SetColumn(txtEqual, 1);

                    TextBlock txtUnequal = new TextBlock();
                    txtUnequal.Text = "Unequal";
                    txtUnequal.Margin = new Thickness(4, 0, 4, 0);
                    txtUnequal.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    txtUnequal.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid2.Children.Add(txtUnequal);
                    Grid.SetRow(txtUnequal, 2);
                    Grid.SetColumn(txtUnequal, 1);

                    TextBlock txtDFP = new TextBlock();
                    txtDFP.Text = stats.df.ToString();
                    txtDFP.Margin = new Thickness(4, 0, 4, 0);
                    txtDFP.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    txtDFP.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid2.Children.Add(txtDFP);
                    Grid.SetRow(txtDFP, 1);
                    Grid.SetColumn(txtDFP, 2);

                    TextBlock txtDFS = new TextBlock();
                    txtDFS.Text = stats.SatterthwaiteDF.ToString(precisionFormat);
                    txtDFS.Margin = new Thickness(4, 0, 4, 0);
                    txtDFS.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    txtDFS.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid2.Children.Add(txtDFS);
                    Grid.SetRow(txtDFS, 2);
                    Grid.SetColumn(txtDFS, 2);

                    TextBlock txtTStatP = new TextBlock();
                    txtTStatP.Text = stats.tStatistic.ToString(precisionFormat);
                    txtTStatP.Margin = new Thickness(4, 0, 4, 0);
                    txtTStatP.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    txtTStatP.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid2.Children.Add(txtTStatP);
                    Grid.SetRow(txtTStatP, 1);
                    Grid.SetColumn(txtTStatP, 3);

                    TextBlock txtTStatS = new TextBlock();
                    txtTStatS.Text = stats.tStatisticUnequal.ToString(precisionFormat);
                    txtTStatS.Margin = new Thickness(4, 0, 4, 0);
                    txtTStatS.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    txtTStatS.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid2.Children.Add(txtTStatS);
                    Grid.SetRow(txtTStatS, 2);
                    Grid.SetColumn(txtTStatS, 3);

                    TextBlock txtPValueP = new TextBlock();
                    txtPValueP.Text = stats.pEqual.ToString(precisionFormat);
                    txtPValueP.Margin = new Thickness(4, 0, 4, 0);
                    txtPValueP.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    txtPValueP.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid2.Children.Add(txtPValueP);
                    Grid.SetRow(txtPValueP, 1);
                    Grid.SetColumn(txtPValueP, 4);

                    TextBlock txtPValueS = new TextBlock();
                    txtPValueS.Text = stats.pUneqal.ToString(precisionFormat);
                    txtPValueS.Margin = new Thickness(4, 0, 4, 0);
                    txtPValueS.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    txtPValueS.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    tGrid2.Children.Add(txtPValueS);
                    Grid.SetRow(txtPValueS, 2);
                    Grid.SetColumn(txtPValueS, 4);

                    rdcountT = 0;
                    foreach (RowDefinition rd in tGrid2.RowDefinitions)
                    {
                        int cdcount = 0;
                        foreach (ColumnDefinition cd in tGrid2.ColumnDefinitions)
                        {
                            Border b = new Border();
                            b.Style = this.Resources["gridCellBorder"] as Style;

                            if (rdcountT == 0)
                            {
                                b.BorderThickness = new Thickness(b.BorderThickness.Left, 1, b.BorderThickness.Right, b.BorderThickness.Bottom);
                            }
                            if (cdcount == 0)
                            {
                                b.BorderThickness = new Thickness(1, b.BorderThickness.Top, b.BorderThickness.Right, b.BorderThickness.Bottom);
                            }

                            Grid.SetRow(b, rdcountT);
                            Grid.SetColumn(b, cdcount);
                            tGrid2.Children.Add(b);
                            cdcount++;
                        }
                        rdcountT++;
                    }
            }

            Expander anovaExpander = new Expander();
            anovaExpander.Margin = (Thickness)this.Resources["expanderMargin"];
            anovaExpander.IsExpanded = true;

            StackPanel pnl = new StackPanel();

            anovaExpander.Content = pnl;

            if (this.StrataCount > 1)
            {
                Expander expander = GetStrataExpander(strataValue);
                if (expander.Content is StackPanel)
                {
                    StackPanel sPanel = expander.Content as StackPanel;
                    sPanel.Children.Add(anovaExpander);
                }
            }
            else
            {
                panelMain.Children.Add(anovaExpander);
            }

            KeyValuePair<string, DescriptiveStatistics> ANOVAStatistics = new KeyValuePair<string, DescriptiveStatistics>(strataValue, stats);
            pnl.Tag = ANOVAStatistics;
            pnl.HorizontalAlignment = HorizontalAlignment.Center;
            pnl.VerticalAlignment = VerticalAlignment.Center;
            pnl.Margin = new Thickness(5);
            anovaBlocks.Add(pnl);

            TextBlock txt1 = new TextBlock();
            txt1.Text = SharedStrings.ANOVA_DESCRIPTION_FOR_MEANS;
            txt1.HorizontalAlignment = HorizontalAlignment.Center;
            txt1.Margin = new Thickness(5);
            txt1.FontWeight = FontWeights.Bold;
            //pnl.Children.Add(txt1);
            anovaExpander.Header = txt1;

            #region Grid1
            TextBlock txt2 = new TextBlock();
            txt2.Text = SharedStrings.ANOVA_DESCRIPTION_FOR_MEANS_SUBTITLE;
            txt2.HorizontalAlignment = HorizontalAlignment.Center;
            pnl.Children.Add(txt2);

            Grid grid1 = new Grid();
            grid1.SnapsToDevicePixels = true;
            grid1.HorizontalAlignment = HorizontalAlignment.Center;
            grid1.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
            grid1.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
            grid1.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
            grid1.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
            grid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            pnl.Children.Add(grid1);

            for (int y = 0; y < grid1.ColumnDefinitions.Count; y++)
            {
                Rectangle rctHeader = new Rectangle();
                rctHeader.Style = this.Resources["gridHeaderCellRectangle"] as Style;
                Grid.SetRow(rctHeader, 0);
                Grid.SetColumn(rctHeader, y);
                grid1.Children.Add(rctHeader);
            }

            for (int y = 1; y < grid1.RowDefinitions.Count; y++)
            {
                Rectangle rctHeader = new Rectangle();
                rctHeader.Style = this.Resources["gridHeaderCellRectangle"] as Style;
                Grid.SetRow(rctHeader, y);
                Grid.SetColumn(rctHeader, 0);
                grid1.Children.Add(rctHeader);
            }

            TextBlock lblVariation = new TextBlock();
            lblVariation.Text = SharedStrings.ANOVA_VARIATION;
            lblVariation.Margin = new Thickness(4, 0, 4, 0);
            lblVariation.FontWeight = FontWeights.Bold;
            lblVariation.Foreground = Brushes.White;
            lblVariation.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            lblVariation.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            grid1.Children.Add(lblVariation);
            Grid.SetRow(lblVariation, 0);
            Grid.SetColumn(lblVariation, 0);

            TextBlock lblBetween = new TextBlock();
            lblBetween.Text = SharedStrings.ANOVA_BETWEEN;
            lblBetween.Margin = new Thickness(4, 0, 4, 0);
            lblBetween.FontWeight = FontWeights.Bold;
            lblBetween.Foreground = Brushes.White;
            lblBetween.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            lblBetween.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            grid1.Children.Add(lblBetween);
            Grid.SetRow(lblBetween, 1);
            Grid.SetColumn(lblBetween, 0);

            TextBlock lblWithin = new TextBlock();
            lblWithin.Text = SharedStrings.ANOVA_WITHIN;
            lblWithin.Margin = new Thickness(4, 0, 4, 0);
            lblWithin.FontWeight = FontWeights.Bold;
            lblWithin.Foreground = Brushes.White;
            lblWithin.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            lblWithin.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            grid1.Children.Add(lblWithin);
            Grid.SetRow(lblWithin, 2);
            Grid.SetColumn(lblWithin, 0);

            TextBlock lblTotal = new TextBlock();
            lblTotal.Text = SharedStrings.ANOVA_TOTAL;
            lblTotal.Margin = new Thickness(4, 0, 4, 0);
            lblTotal.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            lblTotal.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            lblTotal.FontWeight = FontWeights.Bold;
            lblTotal.Foreground = Brushes.White;
            grid1.Children.Add(lblTotal);
            Grid.SetRow(lblTotal, 3);
            Grid.SetColumn(lblTotal, 0);

            TextBlock lblSS = new TextBlock();
            lblSS.Text = SharedStrings.ANOVA_SS;
            lblSS.Margin = new Thickness(4, 0, 4, 0);
            lblSS.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            lblSS.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            lblSS.FontWeight = FontWeights.Bold;
            lblSS.Foreground = Brushes.White;
            grid1.Children.Add(lblSS);
            Grid.SetRow(lblSS, 0);
            Grid.SetColumn(lblSS, 1);

            TextBlock lblDf = new TextBlock();
            lblDf.Text = SharedStrings.ANOVA_DF_SHORT;
            lblDf.Margin = new Thickness(4, 0, 4, 0);
            lblDf.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            lblDf.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            lblDf.FontWeight = FontWeights.Bold;
            lblDf.Foreground = Brushes.White;
            grid1.Children.Add(lblDf);
            Grid.SetRow(lblDf, 0);
            Grid.SetColumn(lblDf, 2);

            TextBlock lblMS = new TextBlock();
            lblMS.Text = SharedStrings.ANOVA_MS;
            lblMS.Margin = new Thickness(4, 0, 4, 0);
            lblMS.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            lblMS.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            lblMS.FontWeight = FontWeights.Bold;
            lblMS.Foreground = Brushes.White;
            grid1.Children.Add(lblMS);
            Grid.SetRow(lblMS, 0);
            Grid.SetColumn(lblMS, 3);

            TextBlock lblF = new TextBlock();
            lblF.Text = SharedStrings.ANOVA_F;
            lblF.Margin = new Thickness(4, 0, 4, 0);
            lblF.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            lblF.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            lblF.FontWeight = FontWeights.Bold;
            lblF.Foreground = Brushes.White;
            grid1.Children.Add(lblF);
            Grid.SetRow(lblF, 0);
            Grid.SetColumn(lblF, 4);

            String roundingFormat = "N4";

            if (Convert.ToInt32(((MeansParameters)Parameters).Precision) >= 0)
            {
                roundingFormat = "N" + ((MeansParameters)Parameters).Precision;
            }

            TextBlock txtSSBetween = new TextBlock();
            txtSSBetween.Text = stats.ssBetween.Value.ToString(roundingFormat);
            txtSSBetween.Margin = new Thickness(4, 0, 4, 0);
            txtSSBetween.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            txtSSBetween.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            grid1.Children.Add(txtSSBetween);
            Grid.SetRow(txtSSBetween, 1);
            Grid.SetColumn(txtSSBetween, 1);

            TextBlock txtSSWithin = new TextBlock();
            txtSSWithin.Text = stats.ssWithin.Value.ToString(roundingFormat);
            txtSSWithin.Margin = new Thickness(4, 0, 4, 0);
            txtSSWithin.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            txtSSWithin.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            grid1.Children.Add(txtSSWithin);
            Grid.SetRow(txtSSWithin, 2);
            Grid.SetColumn(txtSSWithin, 1);

            TextBlock txtSSTotal = new TextBlock();
            txtSSTotal.Text = (stats.ssWithin.Value + stats.ssBetween.Value).ToString(roundingFormat);
            txtSSTotal.Margin = new Thickness(4, 0, 4, 0);
            txtSSTotal.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            txtSSTotal.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            grid1.Children.Add(txtSSTotal);
            Grid.SetRow(txtSSTotal, 3);
            Grid.SetColumn(txtSSTotal, 1);

            TextBlock txtDFBetween = new TextBlock();
            txtDFBetween.Text = stats.dfBetween.Value.ToString();
            txtDFBetween.Margin = new Thickness(4, 0, 4, 0);
            txtDFBetween.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            txtDFBetween.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            grid1.Children.Add(txtDFBetween);
            Grid.SetRow(txtDFBetween, 1);
            Grid.SetColumn(txtDFBetween, 2);

            TextBlock txtDFWithin = new TextBlock();
            txtDFWithin.Text = stats.dfWithin.Value.ToString();
            txtDFWithin.Margin = new Thickness(4, 0, 4, 0);
            txtDFWithin.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            txtDFWithin.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            grid1.Children.Add(txtDFWithin);
            Grid.SetRow(txtDFWithin, 2);
            Grid.SetColumn(txtDFWithin, 2);

            TextBlock txtDFTotal = new TextBlock();
            txtDFTotal.Text = (stats.dfWithin.Value + stats.dfBetween.Value).ToString();
            txtDFTotal.Margin = new Thickness(4, 0, 4, 0);
            txtDFTotal.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            txtDFTotal.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            grid1.Children.Add(txtDFTotal);
            Grid.SetRow(txtDFTotal, 3);
            Grid.SetColumn(txtDFTotal, 2);

            TextBlock txtMSBetween = new TextBlock();
            txtMSBetween.Text = stats.msBetween.Value.ToString(roundingFormat);
            txtMSBetween.Margin = new Thickness(4, 0, 4, 0);
            txtMSBetween.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            txtMSBetween.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            grid1.Children.Add(txtMSBetween);
            Grid.SetRow(txtMSBetween, 1);
            Grid.SetColumn(txtMSBetween, 3);

            TextBlock txtMSWithin = new TextBlock();
            txtMSWithin.Text = stats.msWithin.Value.ToString(roundingFormat);
            txtMSWithin.Margin = new Thickness(4, 0, 4, 0);
            txtMSWithin.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            txtMSWithin.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            grid1.Children.Add(txtMSWithin);
            Grid.SetRow(txtMSWithin, 2);
            Grid.SetColumn(txtMSWithin, 3);

            TextBlock txtFStat = new TextBlock();
            txtFStat.Text = stats.fStatistic.Value.ToString(roundingFormat);
            txtFStat.Margin = new Thickness(4, 0, 4, 0);
            txtFStat.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            txtFStat.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            grid1.Children.Add(txtFStat);
            Grid.SetRow(txtFStat, 1);
            Grid.SetColumn(txtFStat, 4);

            int rdcount = 0;
            foreach (RowDefinition rd in grid1.RowDefinitions)
            {
                int cdcount = 0;
                foreach (ColumnDefinition cd in grid1.ColumnDefinitions)
                {
                    Border b = new Border();
                    b.Style = this.Resources["gridCellBorder"] as Style;

                    if (rdcount == 0)
                    {
                        b.BorderThickness = new Thickness(b.BorderThickness.Left, 1, b.BorderThickness.Right, b.BorderThickness.Bottom);
                    }
                    if (cdcount == 0)
                    {
                        b.BorderThickness = new Thickness(1, b.BorderThickness.Top, b.BorderThickness.Right, b.BorderThickness.Bottom);
                    }

                    Grid.SetRow(b, rdcount);
                    Grid.SetColumn(b, cdcount);
                    grid1.Children.Add(b);
                    cdcount++;
                }
                rdcount++;
            }

            #endregion // Grid1

            #region Grid2
            Grid grid2 = new Grid();
            grid2.SnapsToDevicePixels = true;
            grid2.Margin = new Thickness(5);
            grid2.HorizontalAlignment = HorizontalAlignment.Center;
            grid2.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
            grid2.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(110) });
            grid2.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(95) });

            pnl.Children.Add(grid2);

            for (int y = 0; y < grid2.RowDefinitions.Count; y++)
            {
                Rectangle rctHeader = new Rectangle();
                rctHeader.Style = this.Resources["gridHeaderCellRectangle"] as Style;
                Grid.SetRow(rctHeader, y);
                Grid.SetColumn(rctHeader, 0);
                grid2.Children.Add(rctHeader);
            }

            TextBlock lblPValue = new TextBlock();
            lblPValue.Text = SharedStrings.ANOVA_P_VAL;
            lblPValue.HorizontalAlignment = HorizontalAlignment.Center;
            lblPValue.VerticalAlignment = VerticalAlignment.Center;
            lblPValue.FontWeight = FontWeights.Bold;
            lblPValue.Foreground = Brushes.White;
            Grid.SetRow(lblPValue, 0);
            Grid.SetColumn(lblPValue, 0);
            grid2.Children.Add(lblPValue);

            TextBlock txtPValue = new TextBlock();
            txtPValue.Text = stats.anovaPValue.Value.ToString(roundingFormat);
            txtPValue.HorizontalAlignment = HorizontalAlignment.Center;
            txtPValue.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(txtPValue, 0);
            Grid.SetColumn(txtPValue, 1);
            grid2.Children.Add(txtPValue);

            rdcount = 0;
            foreach (RowDefinition rd in grid2.RowDefinitions)
            {
                int cdcount = 0;
                foreach (ColumnDefinition cd in grid2.ColumnDefinitions)
                {
                    Border b = new Border();
                    b.Style = this.Resources["gridCellBorder"] as Style;

                    if (rdcount == 0)
                    {
                        b.BorderThickness = new Thickness(b.BorderThickness.Left, 1, b.BorderThickness.Right, b.BorderThickness.Bottom);
                    }
                    if (cdcount == 0)
                    {
                        b.BorderThickness = new Thickness(1, b.BorderThickness.Top, b.BorderThickness.Right, b.BorderThickness.Bottom);
                    }

                    Grid.SetRow(b, rdcount);
                    Grid.SetColumn(b, cdcount);
                    grid2.Children.Add(b);
                    cdcount++;
                }
                rdcount++;
            }
            #endregion // Grid2

            #region Grid3
            TextBlock txt3 = new TextBlock();
            txt3.Text = SharedStrings.ANOVA_BARTLETT;
            txt3.HorizontalAlignment = HorizontalAlignment.Center;
            txt3.Margin = new Thickness(5);
            txt3.FontWeight = FontWeights.Bold;
            pnl.Children.Add(txt3);

            Grid grid3 = new Grid();
            grid3.SnapsToDevicePixels = true;
            grid3.Margin = new Thickness(5);
            grid3.HorizontalAlignment = HorizontalAlignment.Center;
            grid3.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
            grid3.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
            grid3.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
            grid3.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(110) });
            grid3.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(95) });

            pnl.Children.Add(grid3);

            for (int y = 0; y < grid3.RowDefinitions.Count; y++)
            {
                Rectangle rctHeader = new Rectangle();
                rctHeader.Style = this.Resources["gridHeaderCellRectangle"] as Style;
                Grid.SetRow(rctHeader, y);
                Grid.SetColumn(rctHeader, 0);
                grid3.Children.Add(rctHeader);
            }

            TextBlock lblBartlettChi = new TextBlock();
            lblBartlettChi.Text = SharedStrings.ANOVA_CHI;
            lblBartlettChi.HorizontalAlignment = HorizontalAlignment.Center;
            lblBartlettChi.VerticalAlignment = VerticalAlignment.Center;
            lblBartlettChi.FontWeight = FontWeights.Bold;
            lblBartlettChi.Foreground = Brushes.White;
            Grid.SetRow(lblBartlettChi, 0);
            Grid.SetColumn(lblBartlettChi, 0);
            grid3.Children.Add(lblBartlettChi);

            TextBlock lblBartlettDf = new TextBlock();
            lblBartlettDf.Text = SharedStrings.ANOVA_DF_LONG;
            lblBartlettDf.HorizontalAlignment = HorizontalAlignment.Center;
            lblBartlettDf.VerticalAlignment = VerticalAlignment.Center;
            lblBartlettDf.FontWeight = FontWeights.Bold;
            lblBartlettDf.Foreground = Brushes.White;
            Grid.SetRow(lblBartlettDf, 1);
            Grid.SetColumn(lblBartlettDf, 0);
            grid3.Children.Add(lblBartlettDf);

            TextBlock lblBartlettP = new TextBlock();
            lblBartlettP.Text = SharedStrings.ANOVA_P_VAL;
            lblBartlettP.HorizontalAlignment = HorizontalAlignment.Center;
            lblBartlettP.VerticalAlignment = VerticalAlignment.Center;
            lblBartlettP.FontWeight = FontWeights.Bold;
            lblBartlettP.Foreground = Brushes.White;
            Grid.SetRow(lblBartlettP, 2);
            Grid.SetColumn(lblBartlettP, 0);
            grid3.Children.Add(lblBartlettP);

            TextBlock txtBartlettChi = new TextBlock();
            txtBartlettChi.Text = stats.chiSquare.Value.ToString(roundingFormat);
            txtBartlettChi.HorizontalAlignment = HorizontalAlignment.Center;
            txtBartlettChi.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(txtBartlettChi, 0);
            Grid.SetColumn(txtBartlettChi, 1);
            grid3.Children.Add(txtBartlettChi);

            TextBlock txtBartlettDf = new TextBlock();
            txtBartlettDf.Text = stats.dfBetween.Value.ToString();
            txtBartlettDf.HorizontalAlignment = HorizontalAlignment.Center;
            txtBartlettDf.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(txtBartlettDf, 1);
            Grid.SetColumn(txtBartlettDf, 1);
            grid3.Children.Add(txtBartlettDf);

            TextBlock txtBartlettP = new TextBlock();
            txtBartlettP.Text = stats.bartlettPValue.Value.ToString(roundingFormat);
            txtBartlettP.HorizontalAlignment = HorizontalAlignment.Center;
            txtBartlettP.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(txtBartlettP, 2);
            Grid.SetColumn(txtBartlettP, 1);
            grid3.Children.Add(txtBartlettP);

            rdcount = 0;
            foreach (RowDefinition rd in grid3.RowDefinitions)
            {
                int cdcount = 0;
                foreach (ColumnDefinition cd in grid3.ColumnDefinitions)
                {
                    Border b = new Border();
                    b.Style = this.Resources["gridCellBorder"] as Style;

                    if (rdcount == 0)
                    {
                        b.BorderThickness = new Thickness(b.BorderThickness.Left, 1, b.BorderThickness.Right, b.BorderThickness.Bottom);
                    }
                    if (cdcount == 0)
                    {
                        b.BorderThickness = new Thickness(1, b.BorderThickness.Top, b.BorderThickness.Right, b.BorderThickness.Bottom);
                    }

                    Grid.SetRow(b, rdcount);
                    Grid.SetColumn(b, cdcount);
                    grid3.Children.Add(b);
                    cdcount++;
                }
                rdcount++;
            }

            TextBlock txt4 = new TextBlock();
            txt4.Text = SharedStrings.ANOVA_SMALL_P;
            txt4.HorizontalAlignment = HorizontalAlignment.Center;
            txt4.Margin = new Thickness(5);
            pnl.Children.Add(txt4);

            TextBlock txt5 = new TextBlock();
            txt5.Text = SharedStrings.ANOVA_MWWTST;
            txt5.HorizontalAlignment = HorizontalAlignment.Center;
            txt5.Margin = new Thickness(5);
            txt5.FontWeight = FontWeights.Bold;
            pnl.Children.Add(txt5);

            #endregion // Grid3

            #region Grid4
            Grid grid4 = new Grid();
            grid4.SnapsToDevicePixels = true;
            grid4.Margin = new Thickness(5);
            grid4.HorizontalAlignment = HorizontalAlignment.Center;
            grid4.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
            grid4.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
            grid4.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
            grid4.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(110) });
            grid4.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(95) });

            pnl.Children.Add(grid4);

            for (int y = 0; y < grid4.RowDefinitions.Count; y++)
            {
                Rectangle rctHeader = new Rectangle();
                rctHeader.Style = this.Resources["gridHeaderCellRectangle"] as Style;
                Grid.SetRow(rctHeader, y);
                Grid.SetColumn(rctHeader, 0);
                grid4.Children.Add(rctHeader);
            }

            TextBlock lblKWChi = new TextBlock();
            lblKWChi.Text = SharedStrings.ANOVA_KWH;
            lblKWChi.HorizontalAlignment = HorizontalAlignment.Center;
            lblKWChi.VerticalAlignment = VerticalAlignment.Center;
            lblKWChi.FontWeight = FontWeights.Bold;
            lblKWChi.Foreground = Brushes.White;
            Grid.SetRow(lblKWChi, 0);
            Grid.SetColumn(lblKWChi, 0);
            grid4.Children.Add(lblKWChi);

            TextBlock lblKWDf = new TextBlock();
            lblKWDf.Text = SharedStrings.ANOVA_DF_LONG;
            lblKWDf.HorizontalAlignment = HorizontalAlignment.Center;
            lblKWDf.VerticalAlignment = VerticalAlignment.Center;
            lblKWDf.FontWeight = FontWeights.Bold;
            lblKWDf.Foreground = Brushes.White;
            Grid.SetRow(lblKWDf, 1);
            Grid.SetColumn(lblKWDf, 0);
            grid4.Children.Add(lblKWDf);

            TextBlock lblKWP = new TextBlock();
            lblKWP.Text = SharedStrings.ANOVA_P_VAL;
            lblKWP.HorizontalAlignment = HorizontalAlignment.Center;
            lblKWP.VerticalAlignment = VerticalAlignment.Center;
            lblKWP.FontWeight = FontWeights.Bold;
            lblKWP.Foreground = Brushes.White;
            Grid.SetRow(lblKWP, 2);
            Grid.SetColumn(lblKWP, 0);
            grid4.Children.Add(lblKWP);

            TextBlock txtKWChi = new TextBlock();
            txtKWChi.Text = stats.kruskalWallisH.Value.ToString(roundingFormat);
            txtKWChi.HorizontalAlignment = HorizontalAlignment.Center;
            txtKWChi.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(txtKWChi, 0);
            Grid.SetColumn(txtKWChi, 1);
            grid4.Children.Add(txtKWChi);

            TextBlock txtWKDf = new TextBlock();
            txtWKDf.Text = stats.dfBetween.Value.ToString();
            txtWKDf.HorizontalAlignment = HorizontalAlignment.Center;
            txtWKDf.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(txtWKDf, 1);
            Grid.SetColumn(txtWKDf, 1);
            grid4.Children.Add(txtWKDf);

            TextBlock txtKWP = new TextBlock();
            txtKWP.Text = stats.kruskalPValue.Value.ToString(roundingFormat);
            txtKWP.HorizontalAlignment = HorizontalAlignment.Center;
            txtKWP.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetRow(txtKWP, 2);
            Grid.SetColumn(txtKWP, 1);
            grid4.Children.Add(txtKWP);

            rdcount = 0;
            foreach (RowDefinition rd in grid4.RowDefinitions)
            {
                int cdcount = 0;
                foreach (ColumnDefinition cd in grid4.ColumnDefinitions)
                {
                    Border b = new Border();
                    b.Style = this.Resources["gridCellBorder"] as Style;

                    if (rdcount == 0)
                    {
                        b.BorderThickness = new Thickness(b.BorderThickness.Left, 1, b.BorderThickness.Right, b.BorderThickness.Bottom);
                    }
                    if (cdcount == 0)
                    {
                        b.BorderThickness = new Thickness(1, b.BorderThickness.Top, b.BorderThickness.Right, b.BorderThickness.Bottom);
                    }

                    Grid.SetRow(b, rdcount);
                    Grid.SetColumn(b, cdcount);
                    grid4.Children.Add(b);
                    cdcount++;
                }
                rdcount++;
            }

            #endregion // Grid4

            //panelMain.Children.Add(pnl);
        }

        /// <summary>
        /// Used to add a new MEANS grid to the gadget's output
        /// </summary>
        /// <param name="strataVar">The name of the stratification variable selected, if any</param>
        /// <param name="value">The value by which this grid has been stratified by</param>
        private void AddMeansGrid(string strataVar, string value)
        {
            Grid grid = new Grid();
            grid.Tag = value;
            grid.Style = this.Resources["genericOutputGrid"] as Style;
            grid.MinWidth = 250;
            grid.Visibility = System.Windows.Visibility.Collapsed;

            Border border = new Border();
            border.Style = this.Resources["genericOutputGridBorderWithMargins"] as Style;
            border.Child = grid;

            Expander expander = new Expander();
            expander.Tag = value;

            TextBlock txtExpanderHeader = new TextBlock();
            txtExpanderHeader.Text = value;
            txtExpanderHeader.Style = this.Resources["genericOutputExpanderText"] as Style;
            expander.Header = txtExpanderHeader;
            
            ColumnDefinition column1 = new ColumnDefinition();
            ColumnDefinition column2 = new ColumnDefinition();
            ColumnDefinition column3 = new ColumnDefinition();
            ColumnDefinition column4 = new ColumnDefinition();
            ColumnDefinition column5 = new ColumnDefinition();
            ColumnDefinition column6 = new ColumnDefinition();
            ColumnDefinition column7 = new ColumnDefinition();
            ColumnDefinition column8 = new ColumnDefinition();
            ColumnDefinition column9 = new ColumnDefinition();
            ColumnDefinition column10 = new ColumnDefinition();
            ColumnDefinition column11 = new ColumnDefinition();
            ColumnDefinition column12 = new ColumnDefinition();

            //column1.Width = new GridLength(1, GridUnitType.Star);
            column1.Width = new GridLength(1, GridUnitType.Star);
            column1.MinWidth = 150; // GridLength.Auto;
            
            column2.Width = GridLength.Auto;
            column3.Width = GridLength.Auto;
            column4.Width = GridLength.Auto;
            column5.Width = GridLength.Auto;
            column6.Width = GridLength.Auto;
            column7.Width = GridLength.Auto;
            column8.Width = GridLength.Auto;
            column9.Width = GridLength.Auto;
            column10.Width = GridLength.Auto;
            column11.Width = GridLength.Auto;
            column12.Width = GridLength.Auto;            

            grid.ColumnDefinitions.Add(column1);
            grid.ColumnDefinitions.Add(column2);
            grid.ColumnDefinitions.Add(column3);
            grid.ColumnDefinitions.Add(column4);
            grid.ColumnDefinitions.Add(column5);
            grid.ColumnDefinitions.Add(column6);
            grid.ColumnDefinitions.Add(column7);
            grid.ColumnDefinitions.Add(column8);
            grid.ColumnDefinitions.Add(column9);
            grid.ColumnDefinitions.Add(column10);
            grid.ColumnDefinitions.Add(column11);
            grid.ColumnDefinitions.Add(column12);

            if (string.IsNullOrEmpty(strataVar))
            {
                panelMain.Children.Add(border);
            }
            else
            {
                expander.Margin = (Thickness)this.Resources["expanderMargin"];

                StackPanel panel = new StackPanel();
                panel.Children.Add(border);
                expander.Content = panel;
                expander.IsExpanded = true;
                panelMain.Children.Add(expander);
                StrataExpanderList.Add(expander);
            }

            StrataGridList.Add(grid);
        }

        /// <summary>
        /// Sets the text value of a grid cell in a given output grid
        /// </summary>
        /// <param name="strataValue">
        /// The strata value to which this grid cell belongs; used to search the list of grids and 
        /// return the proper System.Windows.Controls.Grid for text insertion.
        /// </param>
        /// <param name="textBlockConfig">The configuration options for this block of text</param>
        private void SetGridText(string strataValue, TextBlockConfig textBlockConfig)
        {
            Grid grid = new Grid();

            grid = GetStrataGrid(strataValue);

            TextBlock txt = new TextBlock();
            txt.Text = textBlockConfig.Text;
            txt.Margin = textBlockConfig.Margin;
            txt.VerticalAlignment = textBlockConfig.VerticalAlignment;
            txt.HorizontalAlignment = textBlockConfig.HorizontalAlignment;
            Grid.SetRow(txt, textBlockConfig.RowNumber);
            Grid.SetColumn(txt, textBlockConfig.ColumnNumber);
            grid.Children.Add(txt);
        }

        /// <summary>
        /// Used to render the header (first row) of a given MEANS output grid
        /// </summary>
        /// <param name="strataValue">
        /// The strata value to which this grid cell belongs; used to search the list of grids and 
        /// return the proper System.Windows.Controls.Grid for text insertion.
        /// </param>
        /// <param name="meansVar">The variable that the statistics were run on</param>
        /// <param name="crosstabVar">The variable used to cross-tabulate by, if any</param>
        private void RenderMeansHeader(string strataValue, string meansVar, string crosstabVar)
        {
            Grid grid = GetStrataGrid(strataValue);

            RowDefinition rowDefHeader = new RowDefinition();
            rowDefHeader.Height = new GridLength(30);
            grid.RowDefinitions.Add(rowDefHeader); 

            for (int y = 0; y < grid.ColumnDefinitions.Count; y++)
            {
                Rectangle rctHeader = new Rectangle();
                rctHeader.Style = this.Resources["gridHeaderCellRectangle"] as Style;
                Grid.SetRow(rctHeader, 0);
                Grid.SetColumn(rctHeader, y);
                grid.Children.Add(rctHeader); 
            }

            TextBlock txtValHeader = new TextBlock();
            if (string.IsNullOrEmpty(crosstabVar))
            {
                txtValHeader.Text = string.Empty;
            }
            else
            {
                txtValHeader.Text = meansVar;
            }
            txtValHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtValHeader, 0);
            Grid.SetColumn(txtValHeader, 0);
            grid.Children.Add(txtValHeader);

            TextBlock txtObservationsHeader = new TextBlock();
            txtObservationsHeader.Text = StringLiterals.SPACE + SharedStrings.DASHBOARD_MEANS_OBSERVATIONS + StringLiterals.SPACE;
            txtObservationsHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtObservationsHeader, 0);
            Grid.SetColumn(txtObservationsHeader, 1);
            grid.Children.Add(txtObservationsHeader);

            TextBlock txtTotalHeader = new TextBlock();
            txtTotalHeader.Text = SharedStrings.DASHBOARD_MEANS_TOTAL;
            txtTotalHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtTotalHeader, 0);
            Grid.SetColumn(txtTotalHeader, 2);
            grid.Children.Add(txtTotalHeader);

            TextBlock txtMeanHeader = new TextBlock();
            txtMeanHeader.Text = SharedStrings.DASHBOARD_MEANS_MEAN;
            txtMeanHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtMeanHeader, 0);
            Grid.SetColumn(txtMeanHeader, 3);
            grid.Children.Add(txtMeanHeader);

            TextBlock txtVarianceHeader = new TextBlock();
            txtVarianceHeader.Text = SharedStrings.DASHBOARD_MEANS_VARIANCE;
            txtVarianceHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtVarianceHeader, 0);
            Grid.SetColumn(txtVarianceHeader, 4);
            grid.Children.Add(txtVarianceHeader);

            TextBlock txtStdDevHeader = new TextBlock();
            txtStdDevHeader.Text = SharedStrings.DASHBOARD_MEANS_STANDARD_DEVIATION;
            txtStdDevHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtStdDevHeader, 0);
            Grid.SetColumn(txtStdDevHeader, 5);
            grid.Children.Add(txtStdDevHeader);

            TextBlock txtMinHeader = new TextBlock();
            txtMinHeader.Text = SharedStrings.DASHBOARD_MEANS_MIN;
            txtMinHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtMinHeader, 0);
            Grid.SetColumn(txtMinHeader, 6);
            grid.Children.Add(txtMinHeader);

            TextBlock txt25Header = new TextBlock();
            txt25Header.Text = SharedStrings.DASHBOARD_MEANS_25;
            txt25Header.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txt25Header, 0);
            Grid.SetColumn(txt25Header, 7);
            grid.Children.Add(txt25Header);

            TextBlock txtMedianHeader = new TextBlock();
            txtMedianHeader.Text = SharedStrings.DASHBOARD_MEANS_MEDIAN;
            txtMedianHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtMedianHeader, 0);
            Grid.SetColumn(txtMedianHeader, 8);
            grid.Children.Add(txtMedianHeader);

            TextBlock txt75Header = new TextBlock();
            txt75Header.Text = SharedStrings.DASHBOARD_MEANS_75;
            txt75Header.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txt75Header, 0);
            Grid.SetColumn(txt75Header, 9);
            grid.Children.Add(txt75Header);

            TextBlock txtMaxHeader = new TextBlock();
            txtMaxHeader.Text = SharedStrings.DASHBOARD_MEANS_MAX;
            txtMaxHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtMaxHeader, 0);
            Grid.SetColumn(txtMaxHeader, 10);
            grid.Children.Add(txtMaxHeader);

            TextBlock txtModeHeader = new TextBlock();
            txtModeHeader.Text = SharedStrings.DASHBOARD_MEANS_MODE;
            txtModeHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtModeHeader, 0);
            Grid.SetColumn(txtModeHeader, 11);
            grid.Children.Add(txtModeHeader);
        }

        /// <summary>
        /// Sets the gadget's state to 'finished' mode
        /// </summary>
        protected override void RenderFinish()
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            foreach (Grid freqGrid in StrataGridList)
            {
                freqGrid.Visibility = Visibility.Visible;
            }

            messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
            messagePanel.Text = string.Empty;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            HideConfigPanel();
            ShowHideOutputColumns();
            CheckAndSetPosition();
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

            panelMain.Children.Clear();

            HideConfigPanel();
            CheckAndSetPosition();
        }

        /// <summary>
        /// Used to generate the list of variables and options for the GadgetParameters object
        /// </summary>        
        private void CreateInputVariableList()
        {
            //MOVED TO MEANS PROPERTIES
            //Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

            //GadgetOptions.MainVariableName = string.Empty;
            //GadgetOptions.WeightVariableName = string.Empty;
            //GadgetOptions.StrataVariableNames = new List<string>();
            //GadgetOptions.CrosstabVariableName = string.Empty;
            //GadgetOptions.ColumnNames = new List<string>();

            //if (cbxField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxField.SelectedItem.ToString()))
            //{
            //    inputVariableList.Add("meansvar", cbxField.SelectedItem.ToString());
            //    GadgetOptions.MainVariableName = cbxField.SelectedItem.ToString();
            //}
            //else
            //{
            //    return;
            //}

            //if (cbxFieldWeight.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldWeight.SelectedItem.ToString()))
            //{
            //    inputVariableList.Add("weightvar", cbxFieldWeight.SelectedItem.ToString());
            //    GadgetOptions.WeightVariableName = cbxFieldWeight.SelectedItem.ToString();
            //}
            ////if (cbxFieldStrata.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldStrata.SelectedItem.ToString()))
            ////{
            ////    inputVariableList.Add("stratavar", cbxFieldStrata.SelectedItem.ToString());
            ////    GadgetOptions.StrataVariableNames = new List<string>();
            ////    GadgetOptions.StrataVariableNames.Add(cbxFieldStrata.SelectedItem.ToString());
            ////}

            //if (lbxFieldStrata.SelectedItems.Count > 0)
            //{
            //    //inputVariableList.Add("stratavar", cbxFieldStrata.SelectedItem.ToString());
            //    GadgetOptions.StrataVariableNames = new List<string>();
            //    foreach (string s in lbxFieldStrata.SelectedItems)
            //    {
            //        GadgetOptions.StrataVariableNames.Add(s);
            //    }
            //}

            //if (cbxFieldCrosstab.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldCrosstab.SelectedItem.ToString()))
            //{
            //    inputVariableList.Add("crosstabvar", cbxFieldCrosstab.SelectedItem.ToString());
            //    GadgetOptions.CrosstabVariableName = cbxFieldCrosstab.SelectedItem.ToString();
            //}

            //if (checkboxShowANOVA.IsChecked == true)
            //{
            //    inputVariableList.Add("showanova", "true");
            //}
            //else
            //{
            //    inputVariableList.Add("showanova", "false");
            //}

            //if (cbxFieldPrecision.SelectedIndex >= 0)
            //{
            //    inputVariableList.Add("precision", cbxFieldPrecision.SelectedIndex.ToString());
            //}

            //GadgetOptions.InputVariableList = inputVariableList;            
        }

        /// <summary>
        /// Used to construct the gadget and assign events
        /// </summary>
        protected override void Construct()
        {
            if (!string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)"))
            {
                headerPanel.Text = CustomOutputHeading;
            }

            Parameters = new MeansParameters();

            StrataGridList = new List<Grid>();
            anovaBlocks = new List<StackPanel>();
            gridLabelsList = new List<TextBlock>();
            StrataExpanderList = new List<Expander>();

            cbxField.SelectionChanged += new SelectionChangedEventHandler(cbxField_SelectionChanged);
            cbxFieldWeight.SelectionChanged += new SelectionChangedEventHandler(cbxField_SelectionChanged);
            //cbxFieldStrata.SelectionChanged += new SelectionChangedEventHandler(cbxField_SelectionChanged);
            cbxFieldCrosstab.SelectionChanged += new SelectionChangedEventHandler(cbxField_SelectionChanged);
            cbxFieldPrecision.SelectionChanged += new SelectionChangedEventHandler(cbxField_SelectionChanged);

            mnuCopy.Click += new RoutedEventHandler(mnuCopy_Click);
            mnuSendDataToHTML.Click += new RoutedEventHandler(mnuSendDataToHTML_Click);

#if LINUX_BUILD
            mnuSendDataToExcel.Visibility = Visibility.Collapsed;
#else
            mnuSendDataToExcel.Visibility = Visibility.Visible;
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.ClassesRoot; Microsoft.Win32.RegistryKey excelKey = key.OpenSubKey("Excel.Application"); bool excelInstalled = excelKey == null ? false : true; key = Microsoft.Win32.Registry.ClassesRoot;
            excelKey = key.OpenSubKey("Excel.Application");
            excelInstalled = excelKey == null ? false : true;

            if (!excelInstalled)
            {
                mnuSendDataToExcel.Visibility = Visibility.Collapsed;
            }
            else
            {
                mnuSendDataToExcel.Click += new RoutedEventHandler(mnuSendDataToExcel_Click);
            }
#endif

            mnuSendToBack.Click += new RoutedEventHandler(mnuSendToBack_Click);
            mnuClose.Click += new RoutedEventHandler(mnuClose_Click);

            checkboxShowANOVA.Checked += new RoutedEventHandler(checkboxCheckChanged);
            checkboxShowANOVA.Unchecked += new RoutedEventHandler(checkboxCheckChanged);
    
            tblockCrosstabVariable.Text = SharedStrings.DASHBOARD_CROSSTAB_FIELD_LABEL;
            tblockMainVariable.Text = SharedStrings.DASHBOARD_FIELD_LABEL;
            tblockStrataVariable.Text = SharedStrings.DASHBOARD_STRATA_FIELD_SINGLE_LABEL;
            tblockWeightVariable.Text = SharedStrings.DASHBOARD_WEIGHT_FIELD_LABEL;
            btnRun.Content = DashboardSharedStrings.GADGET_RUN_BUTTON;

            this.IsProcessing = false;

            this.GadgetStatusUpdate += new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(IsCancelled);

            base.Construct();

            #region Translation
            ConfigExpandedTitle.Text = DashboardSharedStrings.GADGET_CONFIG_TITLE_MEANS;
            tblockMainVariable.Text = DashboardSharedStrings.GADGET_MEANS_VARIABLE;
            expanderAdvancedOptions.Header = DashboardSharedStrings.GADGET_ADVANCED_OPTIONS;
            expanderDisplayOptions.Header = DashboardSharedStrings.GADGET_DISPLAY_OPTIONS;
            tblockStrataVariable.Text = DashboardSharedStrings.GADGET_STRATA_VARIABLE;
            tblockWeightVariable.Text = DashboardSharedStrings.GADGET_WEIGHT_VARIABLE;
            tblockCrosstabVariable.Text = DashboardSharedStrings.GADGET_CROSSTAB_VARIABLE;
            checkboxShowANOVA.Content = DashboardSharedStrings.GADGET_DISPLAY_ANOVA;
            tblockOutputColumns.Text = DashboardSharedStrings.GADGET_OUTPUT_COLUMNS_DISPLAY;
            tblockPrecision.Text = DashboardSharedStrings.GADGET_DECIMALS_TO_DISPLAY;
            btnRun.Content = DashboardSharedStrings.GADGET_RUN_BUTTON;
            #endregion // Translation
        }

        #endregion // Private Methods

        #region IGadget Members

        /// <summary>
        /// Sets the gadget to its 'processing' state
        /// </summary>
        public override void SetGadgetToProcessingState()
        {
            this.IsProcessing = true;
            this.cbxField.IsEnabled = false;
            this.cbxFieldCrosstab.IsEnabled = false;
            this.lbxFieldStrata.IsEnabled = false;
            this.cbxFieldWeight.IsEnabled = false;
            this.cbxFieldPrecision.IsEnabled = false;
            this.checkboxShowANOVA.IsEnabled = false;
            this.lbxColumns.IsEnabled = false;
            this.btnRun.IsEnabled = false;
        }

        /// <summary>
        /// Sets the gadget to its 'finished' state
        /// </summary>
        public override void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
            this.cbxField.IsEnabled = true;
            this.cbxFieldCrosstab.IsEnabled = true;
            this.lbxFieldStrata.IsEnabled = true;
            this.cbxFieldWeight.IsEnabled = true;
            this.cbxFieldPrecision.IsEnabled = true;
            this.checkboxShowANOVA.IsEnabled = true;
            this.lbxColumns.IsEnabled = true;
            this.btnRun.IsEnabled = true;

            base.SetGadgetToFinishedState();
        }

        /// <summary>
        /// Updates the variable names available in the gadget's properties
        /// </summary>
        public override void UpdateVariableNames()
        {
            FillComboboxes(true);
        }

        /// <summary>
        /// Initiates a refresh of the gadget's output
        /// </summary>
        public override void RefreshResults()
        {
            if (!LoadingCombos && Parameters != null && Parameters.ColumnNames.Count > 0) // && cbxField.SelectedIndex > -1)
            {
//                CreateInputVariableList();
                StrataCount = 0;

                infoPanel.Visibility = System.Windows.Visibility.Collapsed;
                waitPanel.Visibility = System.Windows.Visibility.Visible;
                messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
                descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;

                baseWorker = new BackgroundWorker();
                baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
                baseWorker.RunWorkerAsync();

                base.RefreshResults();
            }
            else if (!LoadingCombos) // && cbxField.SelectedIndex == -1)
            {
                ClearResults();
                waitPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        public override void ShowHideConfigPanel()
        {
            Popup = new DashboardPopup();
            Popup.Parent = ((this.Parent as DragCanvas).Parent as ScrollViewer).Parent as Grid;
            Controls.GadgetProperties.MeansProperties properties = new Controls.GadgetProperties.MeansProperties(this.DashboardHelper, this, (MeansParameters)Parameters);

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
            Controls.GadgetProperties.MeansProperties properties = Popup.Content as Controls.GadgetProperties.MeansProperties;
            this.Parameters = properties.Parameters;
            this.DataFilters = properties.DataFilters;
            this.Parameters.CustomFilter = this.DataFilters.GenerateDataFilterString(false);
            if (!String.IsNullOrEmpty(properties.txtTitle.Text))
                this.headerPanel.Text = properties.txtTitle.Text;
            else
                this.headerPanel.Text = "Means";
            this.descriptionPanel.Text = properties.txtDesc.Text;
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

        /// <summary>
        /// Generates Xml representation of this gadget
        /// </summary>
        /// <param name="doc">The Xml docment</param>
        /// <returns>XmlNode</returns>
        public override XmlNode Serialize(XmlDocument doc)
        {
            CreateInputVariableList();

            //Dictionary<string, string> inputVariableList = GadgetOptions.InputVariableList;
            MeansParameters meansParameters = (MeansParameters)Parameters;

            System.Xml.XmlElement element = doc.CreateElement("meansGadget");
            string xmlString = string.Empty;
            element.InnerXml = xmlString;
            element.AppendChild(SerializeFilters(doc));

            System.Xml.XmlAttribute id = doc.CreateAttribute("id");
            System.Xml.XmlAttribute locationY = doc.CreateAttribute("top");
            System.Xml.XmlAttribute locationX = doc.CreateAttribute("left");
            System.Xml.XmlAttribute collapsed = doc.CreateAttribute("collapsed");
            System.Xml.XmlAttribute type = doc.CreateAttribute("gadgetType");

            id.Value = this.UniqueIdentifier.ToString();
            locationY.Value = Canvas.GetTop(this).ToString("F0");
            locationX.Value = Canvas.GetLeft(this).ToString("F0");
            collapsed.Value = IsCollapsed.ToString(); //
            type.Value = "EpiDashboard.MeansControl";

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);
            element.Attributes.Append(id);

            //string meansVar = string.Empty;
            //string strataVar = string.Empty;
            //string weightVar = string.Empty;
            //string crosstabVar = string.Empty;

            this.CustomOutputHeading = meansParameters.GadgetTitle;
            this.CustomOutputDescription = meansParameters.GadgetDescription;

            //if (inputVariableList.ContainsKey("meansvar"))
            //{
            //    meansVar = inputVariableList["meansvar"].Replace("<", "&lt;");
            //}
            //if (inputVariableList.ContainsKey("stratavar"))
            //{
            //    strataVar = inputVariableList["stratavar"].Replace("<", "&lt;");
            //}
            //if (inputVariableList.ContainsKey("weightvar"))
            //{
            //    weightVar = inputVariableList["weightvar"].Replace("<", "&lt;");
            //}
            //if (inputVariableList.ContainsKey("crosstabvar"))
            //{
            //    crosstabVar = inputVariableList["crosstabvar"].Replace("<", "&lt;");
            //}

            //CustomOutputHeading = headerPanel.Text;            
            //CustomOutputDescription = descriptionPanel.Text;

            //int precision = 4;
            //if (cbxFieldPrecision.SelectedIndex >= 0)
            //{
            //    precision = cbxFieldPrecision.SelectedIndex;
            //}

            //xmlString =
            //"<mainVariable>" + meansVar + "</mainVariable>";
            XmlElement meansVarElement = doc.CreateElement("mainVariable");
            if (meansParameters.ColumnNames.Count > 0)
            {
                if (!String.IsNullOrEmpty(meansParameters.ColumnNames[0].ToString()))
                {
                    meansVarElement.InnerText = meansParameters.ColumnNames[0].ToString();
                    element.AppendChild(meansVarElement);
                }
            }

            //weightVariable
            XmlElement weightVariableElement = doc.CreateElement("weightVariable");
            if (!String.IsNullOrEmpty(meansParameters.WeightVariableName))
            {
                weightVariableElement.InnerText = meansParameters.WeightVariableName;
                element.AppendChild(weightVariableElement);
            }

            //if(GadgetOptions.StrataVariableNames.Count == 1) 
            //{
            //    xmlString = xmlString + "<strataVariable>" + GadgetOptions.StrataVariableNames[0].Replace("<", "&lt;") + "</strataVariable>";
            //}
            //else if (GadgetOptions.StrataVariableNames.Count > 1)
            //{
            //    xmlString = xmlString + "<strataVariables>";

            //    foreach (string strataVariable in this.GadgetOptions.StrataVariableNames)
            //    {
            //        xmlString = xmlString + "<strataVariable>" + strataVariable.Replace("<", "&lt;") + "</strataVariable>";
            //    }

            //    xmlString = xmlString + "</strataVariables>";
            //}
            XmlElement StrataVariableNameElement = doc.CreateElement("strataVariable");
            XmlElement StrataVariableNamesElement = doc.CreateElement("strataVariables");
            if (meansParameters.StrataVariableNames.Count == 1)
            {
                StrataVariableNameElement.InnerText = meansParameters.StrataVariableNames[0].ToString();
                element.AppendChild (StrataVariableNameElement );
            }
            else if (meansParameters.StrataVariableNames.Count > 1)
            {
                foreach (string strataColumn in meansParameters.StrataVariableNames)
                {
                    XmlElement strataElement = doc.CreateElement("strataVariable");
                    strataElement.InnerText = strataColumn.Replace("<", "&lt;");
                    StrataVariableNamesElement.AppendChild(strataElement);
                }

                element.AppendChild(StrataVariableNamesElement); 
            }

            //"<crosstabVariable>" + crosstabVar + "</crosstabVariable>" +
            XmlElement crosstabElement = doc.CreateElement("crosstabVariable");
            crosstabElement.InnerText = meansParameters.CrosstabVariableName.ToString();
            element.AppendChild(crosstabElement);

            //"<columnsToShow>" + wb.ToString() + "</columnsToShow>" +
            //columnsToShow


            WordBuilder wb = new WordBuilder(",");
            if (meansParameters.columnsToHide.Count > 0)
            {
                foreach (int x in meansParameters.columnsToHide)
                {
                    wb.Add(x.ToString());
                }
            }
            XmlElement columnsToShowElement = doc.CreateElement("columnsToShow");
            columnsToShowElement.InnerText = wb.ToString();
            element.AppendChild(columnsToShowElement);

            //"<precision>" + precision.ToString() + "</precision>" +
            //precision
            int precision = 4;
            bool precision_success = int.TryParse(meansParameters.Precision.ToString(), out precision);
            XmlElement precisionElement = doc.CreateElement("precision");
            if (precision_success)
            {
                precisionElement.InnerText = precision.ToString();
            }
            else
            {
                precisionElement.InnerText = "4";
                meansParameters.Precision = "4";
            }
            element.AppendChild(precisionElement);

            //"<showANOVA>" + (bool)checkboxShowANOVA.IsChecked + "</showANOVA>" +
            XmlElement showAnovaElement = doc.CreateElement("showANOVA");
            showAnovaElement.InnerText = meansParameters.ShowANOVA.ToString();
            element.AppendChild(showAnovaElement);


            //"<customHeading>" + CustomOutputHeading.Replace("<", "&lt;") + "</customHeading>" +
            //"<customDescription>" + CustomOutputDescription.Replace("<", "&lt;") + "</customDescription>" +
            //"<customCaption>" + CustomOutputCaption + "</customCaption>";
            //customHeading
            XmlElement customHeadingElement = doc.CreateElement("customHeading");
            customHeadingElement.InnerText = meansParameters.GadgetTitle; //CustomOutputHeading.Replace("<", "&lt;");
            element.AppendChild(customHeadingElement);

            //customDescription
            XmlElement customDescriptionElement = doc.CreateElement("customDescription");
            customDescriptionElement.InnerText = meansParameters.GadgetDescription; //CustomOutputDescription.Replace("<", "&lt;");
            element.AppendChild(customDescriptionElement);

            //customCaption
            XmlElement customCaptionElement = doc.CreateElement("customCaption");
            customCaptionElement.InnerText = CustomOutputCaption;
            element.AppendChild(customCaptionElement);

            SerializeAnchors(element);

            return element;
        }

        /// <summary>
        /// Creates the frequency gadget from an Xml element
        /// </summary>
        /// <param name="element">The element from which to create the gadget</param>
        public override void CreateFromXml(XmlElement element)
        {
            this.LoadingCombos = true;
            this.Parameters = new MeansParameters();

            HideConfigPanel();

            infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            foreach (XmlElement child in element.ChildNodes)
            {
                if (!String.IsNullOrEmpty(child.InnerText.Trim()))
                {
                    switch (child.Name.ToLower())
                    {
                        case "mainvariable":
                            //cbxField.Text = child.InnerText.Replace("&lt;", "<");
                            ((MeansParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                            break;
                        case "stratavariable":
                            //cbxFieldStrata.Text = child.InnerText.Replace("&lt;", "<");
                            //lbxFieldStrata.SelectedItems.Add(child.InnerText.Replace("&lt;", "<"));
                            if (string.IsNullOrEmpty(child.InnerText))
                            {
                                ((MeansParameters)Parameters).StrataVariableNames[0] = child.InnerText.Replace("&lt;", "<");
                            }

                            break;
                        case "stratavariables":
                            foreach (XmlElement field in child.ChildNodes)
                            {
                                List<string> fields = new List<string>();
                                if (field.Name.ToLower().Equals("stratavariable"))
                                {
                                    //lbxFieldStrata.SelectedItems.Add(field.InnerText.Replace("&lt;", "<"));
                                    ((MeansParameters)Parameters).StrataVariableNames.Add(field.InnerText.Replace("&lt;", "<"));
                                }
                            }
                            break;
                        case "weightvariable":
                            //cbxFieldWeight.Text = child.InnerText.Replace("&lt;", "<");
                            ((MeansParameters)Parameters).WeightVariableName = child.InnerText.Replace("&lt;", "<");
                            break;
                        case "crosstabvariable":
                            //cbxFieldCrosstab.Text = child.InnerText.Replace("&lt;", "<");
                            ((MeansParameters)Parameters).CrosstabVariableName = child.InnerText.Replace("&lt;", "<");
                            break;
                        case "precision":
                            int precision = 4;
                            int.TryParse(child.InnerText, out precision);
                            //cbxFieldPrecision.SelectedIndex = precision;
                            ((MeansParameters)Parameters).Precision = child.InnerText.Replace("&lt;", "<");
                            break;
                        case "showanova":
                            bool anova = false;
                            bool.TryParse(child.InnerText, out anova);
                            //checkboxShowANOVA.IsChecked = anova;
                            ((MeansParameters)Parameters).ShowANOVA = anova;
                            break;
                        case "customheading":
                            if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
                            {
                                this.CustomOutputHeading = child.InnerText.Replace("&lt;", "<"); ;
                                Parameters.GadgetTitle = CustomOutputHeading;
                            }
                            break;
                        case "customdescription":
                            if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
                            {
                                this.CustomOutputDescription = child.InnerText.Replace("&lt;", "<");
                                Parameters.GadgetDescription = CustomOutputDescription;

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
                        case "customcaption":
                            this.CustomOutputCaption = child.InnerText;
                            break;
                        case "columnstoshow":
                            string[] columnsToShow = child.InnerText.Split(',');
                            foreach (string s in columnsToShow)
                            {
                                int columnNumber = -1;
                                bool success = int.TryParse(s, out columnNumber);
                                if (success)
                                {
                                    ((MeansParameters)Parameters).columnsToHide.Add(columnNumber);
                                }
                            }
                            break;
                        case "datafilters":
                            this.DataFilters = new DataFilters(this.DashboardHelper);
                            this.DataFilters.CreateFromXml(child);
                            break;
                    }
                }
            }

            base.CreateFromXml(element);

            this.LoadingCombos = false;

            RefreshResults();            
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
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">Means</h2>");
            }
            else if (CustomOutputHeading != "(none)")
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">" + CustomOutputHeading + "</h2>");
            }

            htmlBuilder.AppendLine("<p class=\"gadgetOptions\"><small>");
            htmlBuilder.AppendLine("<em>Main variable:</em> <strong>" + cbxField.Text + "</strong>");
            htmlBuilder.AppendLine("<br />");

            if (cbxFieldCrosstab.SelectedIndex >= 0)
            {
                htmlBuilder.AppendLine("<em>Crosstab variable:</em> <strong>" + cbxFieldCrosstab.Text + "</strong>");
                htmlBuilder.AppendLine("<br />");
            }

            if (cbxFieldWeight.SelectedIndex >= 0)
            {
                htmlBuilder.AppendLine("<em>Weight variable:</em> <strong>" + cbxFieldWeight.Text + "</strong>");
                htmlBuilder.AppendLine("<br />");
            }
            //if (cbxFieldStrata.SelectedIndex >= 0)
            //{
            //    htmlBuilder.AppendLine("<em>Strata variable:</em> <strong>" + cbxFieldStrata.Text + "</strong>");
            //    htmlBuilder.AppendLine("<br />");
            //}
            if (lbxFieldStrata.SelectedItems.Count > 0)
            {
                WordBuilder wb = new WordBuilder(", ");
                foreach (string s in lbxFieldStrata.SelectedItems)
                {
                    wb.Add(s);
                }
                htmlBuilder.AppendLine("<em>Strata variable(s):</em> <strong>" + wb.ToString() + "</strong>");
                htmlBuilder.AppendLine("<br />");
            }
            
            htmlBuilder.AppendLine("</small></p>");

            if (!string.IsNullOrEmpty(CustomOutputDescription))
            {
                htmlBuilder.AppendLine("<p class=\"gadgetsummary\">" + CustomOutputDescription + "</p>");
            }

            if (!string.IsNullOrEmpty(messagePanel.Text) && messagePanel.Visibility == Visibility.Visible)
            {
                htmlBuilder.AppendLine("<p><small><strong>" + messagePanel.Text + "</strong></small></p>");
            }

            if (!string.IsNullOrEmpty(infoPanel.Text) && infoPanel.Visibility == Visibility.Visible)
            {
                htmlBuilder.AppendLine("<p><small><strong>" + infoPanel.Text + "</strong></small></p>");
            }

            // Each grid in the 'strataGridList' has a tag associated with it which tells us which strata 
            // that grid is for. For example, if we stratify by 'Sex' in the Oswego table (in Sample.prj)
            // we will have two grids in strataGridList, one for males and one for females. The tags will
            // be 'Sex = Male' and 'Sex = Female', respectively. 
            foreach (Grid grid in this.StrataGridList)
            {
                string gridName = grid.Tag.ToString();

                string summaryText = "This tables contains several descriptive statistics for the field " + cbxField.Text + ". ";
                if (!string.IsNullOrEmpty(cbxFieldWeight.Text)) { summaryText += "The field " + cbxFieldWeight.Text + " has been specified as a weight. "; }
                if (lbxFieldStrata.SelectedItems.Count > 0) { summaryText += "The data has been stratified. The data in this table is for the strata value " + grid.Tag.ToString() + ". "; }
                if (!string.IsNullOrEmpty(cbxFieldCrosstab.Text)) { summaryText += "The data has been cross-tabulated; there will be one data row for each value of " + cbxFieldCrosstab.Text + ". "; }
                summaryText += "The column headings are: The description of the data, the total number of observations, the sum of the observations, the mean, the variance, the standard deviation, the minimum observed value, the 25% value, the median value, the 75% value, the maximum, and the mode.";                

                htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" summary=\"" + summaryText + "\">");
                htmlBuilder.AppendLine("<caption>" + gridName + "</caption>");

                for (int i = 0; i < grid.RowDefinitions.Count; i++)
                {
                    for (int j = 0; j < grid.ColumnDefinitions.Count; j++)
                    {
                        string tableDataTagOpen = "<td>";
                        string tableDataTagClose = "</td>";

                        if (i == 0)
                        {
                            tableDataTagOpen = "<th>";
                            tableDataTagClose = "</th>";
                        }

                        if (j == 0)
                        {
                            htmlBuilder.AppendLine("<tr>");
                        }
                        if (j == 0 && i > 0)
                        {
                            tableDataTagOpen = "<td class=\"value\">";
                        }

                        ColumnDefinition colDef = grid.ColumnDefinitions[j];
                        if (colDef.Width.Value > 0)
                        {
                            IEnumerable<UIElement> elements = grid.Children.Cast<UIElement>().Where(x => Grid.GetRow(x) == i && Grid.GetColumn(x) == j);
                            TextBlock txt = null;
                            foreach (UIElement element in elements)
                            {
                                if (element is TextBlock)
                                {
                                    txt = element as TextBlock;
                                }
                            }
                            string value = "&nbsp;";

                            if (txt != null)
                            {
                                value = txt.Text;
                            }
                            
                            htmlBuilder.AppendLine(tableDataTagOpen + value + tableDataTagClose);                            
                        }

                        if (j >= grid.ColumnDefinitions.Count - 1)
                        {
                            htmlBuilder.AppendLine("</tr>");
                        }
                    }
                }

                htmlBuilder.AppendLine("</table>");

                // We must find the specific stack panel that corresponds to this strata value from the list of 
                // ANOVA panels in the gadget. The 'tag' property of the panel is a key value pair that contains
                // the strata value we need, plus the set of descriptive statistics that we must display in the
                // output.
                StackPanel anovaPanel = null;
                DescriptiveStatistics statistics = new DescriptiveStatistics();
                foreach (StackPanel panel in anovaBlocks)
                {
                    if(panel.Tag is KeyValuePair<string, DescriptiveStatistics>) 
                    {
                        KeyValuePair<string, DescriptiveStatistics> kvp = ((KeyValuePair<string, DescriptiveStatistics>)panel.Tag);
                        if (kvp.Key.Equals(gridName))
                        {
                            anovaPanel = panel;
                            statistics = kvp.Value;
                            break; // no sense in continuning
                        }
                    }
                }

                // check to make sure we actually found one
                if (!(anovaPanel == null))
                {
                    string precision = "4";
                    Parameters.InputVariableList.TryGetValue("precision", out precision);
                    // check for t-test
                    if (grid.RowDefinitions.Count == 3)
                    {
                        htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                        htmlBuilder.AppendLine("<p><strong>T-Test</strong><br />");
                        htmlBuilder.AppendLine("<small>" + SharedStrings.ANOVA_DESCRIPTION_FOR_MEANS_SUBTITLE + "</small></p>");
                        htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" summary=\"" + summaryText + "\">");
                        htmlBuilder.AppendLine(" <tr>");
                        htmlBuilder.AppendLine("   <th></th>");
                        htmlBuilder.AppendLine("   <th>Method</th>");
                        htmlBuilder.AppendLine("   <th>Mean</th>");
                        htmlBuilder.AppendLine("   <th colspan=2>95% CL</th>");
                        htmlBuilder.AppendLine("   <th>Std. Dev.</th>");
                        htmlBuilder.AppendLine(" </tr>");

                        htmlBuilder.AppendLine(" <tr>");
                        htmlBuilder.AppendLine("   <th>Diff (Group 1 - Group 2)</th>");
                        htmlBuilder.AppendLine("   <td class=\"value\">Pooled</td>");
                        htmlBuilder.AppendLine("   <td>" + statistics.meansDiff.ToString("F" + precision) + "</td>");
                        htmlBuilder.AppendLine("   <td>" + statistics.equalLCLMean.ToString("F" + precision) + "</td>");
                        htmlBuilder.AppendLine("   <td>" + statistics.equalUCLMean.ToString("F" + precision) + "</td>");
                        htmlBuilder.AppendLine("   <td>" + statistics.stdDevDiff.ToString("F" + precision) + "</td>");
                        htmlBuilder.AppendLine(" </tr>");

                        htmlBuilder.AppendLine(" <tr>");
                        htmlBuilder.AppendLine("   <th>Diff (Group 1 - Group 2)</th>");
                        htmlBuilder.AppendLine("   <td class=\"value\">Satterthwaite</td>");
                        htmlBuilder.AppendLine("   <td>" + statistics.meansDiff.ToString("F" + precision) + "</td>");
                        htmlBuilder.AppendLine("   <td>" + statistics.unequalLCLMean.ToString("F" + precision) + "</td>");
                        htmlBuilder.AppendLine("   <td>" + statistics.unequalUCLMean.ToString("F" + precision) + "</td>");
                        htmlBuilder.AppendLine("   <td></td>");
                        htmlBuilder.AppendLine(" </tr>");
                        htmlBuilder.AppendLine("</table>");

                        htmlBuilder.AppendLine("<br><table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" summary=\"" + summaryText + "\">");
                        htmlBuilder.AppendLine(" <tr>");
                        htmlBuilder.AppendLine("   <th>Method</th>");
                        htmlBuilder.AppendLine("   <th>Variances</th>");
                        htmlBuilder.AppendLine("   <th>DF</th>");
                        htmlBuilder.AppendLine("   <th>t value</th>");
                        htmlBuilder.AppendLine("   <th>Pr > |t|</th>");
                        htmlBuilder.AppendLine(" </tr>");

                        htmlBuilder.AppendLine(" <tr>");
                        htmlBuilder.AppendLine("   <th style=\"text-align: left;\">Pooled</th>");
                        htmlBuilder.AppendLine("   <td class=\"value\">Equal</td>");
                        htmlBuilder.AppendLine("   <td>" + statistics.df.ToString() + "</td>");
                        htmlBuilder.AppendLine("   <td>" + statistics.tStatistic.ToString("F" + precision) + "</td>");
                        htmlBuilder.AppendLine("   <td>" + statistics.pEqual.ToString("F" + precision) + "</td>");
                        htmlBuilder.AppendLine(" </tr>");

                        htmlBuilder.AppendLine(" <tr>");
                        htmlBuilder.AppendLine("   <th style=\"text-align: left;\">Satterthwaite</th>");
                        htmlBuilder.AppendLine("   <td class=\"value\">Unequal</td>");
                        htmlBuilder.AppendLine("   <td>" + statistics.SatterthwaiteDF.ToString("F" + precision) + "</td>");
                        htmlBuilder.AppendLine("   <td>" + statistics.tStatisticUnequal.ToString("F" + precision) + "</td>");
                        htmlBuilder.AppendLine("   <td>" + statistics.pUneqal.ToString("F" + precision) + "</td>");
                        htmlBuilder.AppendLine(" </tr>");
                        htmlBuilder.AppendLine("</table>");
                    }

                    string strssBetweenValue = SharedStrings.UNDEFINED;
                    string strdfBetweenValue = SharedStrings.UNDEFINED;
                    string strmsBetweenValue = SharedStrings.UNDEFINED;
                    string strssWithinValue = SharedStrings.UNDEFINED;
                    string strdfWithinValue = SharedStrings.UNDEFINED;
                    string strmsWithinValue = SharedStrings.UNDEFINED;
                    string strfStatisticValue = SharedStrings.UNDEFINED;
                    string stranovaPValueValue = SharedStrings.UNDEFINED;
                    string stranovaTValueValue = SharedStrings.UNDEFINED;
                    string strchiSquareValue = SharedStrings.UNDEFINED;
                    string strbartlettPValue = SharedStrings.UNDEFINED;
                    string strTotalSSValue = SharedStrings.UNDEFINED;
                    string strTotalDFValue = SharedStrings.UNDEFINED;
                    string strKruskalWallisH = SharedStrings.UNDEFINED;
                    string strKruskalPValue = SharedStrings.UNDEFINED;

                    if (statistics.ssBetween.HasValue) { strssBetweenValue = statistics.ssBetween.Value.ToString("F" + precision); }
                    if (statistics.dfBetween.HasValue) { strdfBetweenValue = statistics.dfBetween.Value.ToString("F0"); }
                    if (statistics.msBetween.HasValue) { strmsBetweenValue = statistics.msBetween.Value.ToString("F" + precision); }
                    if (statistics.ssWithin.HasValue) { strssWithinValue = statistics.ssWithin.Value.ToString("F" + precision); }
                    if (statistics.dfWithin.HasValue) { strdfWithinValue = statistics.dfWithin.Value.ToString("F0"); }
                    if (statistics.msWithin.HasValue) { strmsWithinValue = statistics.msWithin.Value.ToString("F" + precision); }
                    if (statistics.fStatistic.HasValue) { strfStatisticValue = statistics.fStatistic.Value.ToString("F" + precision); }
                    if (statistics.anovaPValue.HasValue) { stranovaPValueValue = statistics.anovaPValue.Value.ToString("F" + precision); }
                    if (statistics.chiSquare.HasValue) { strchiSquareValue = statistics.chiSquare.Value.ToString("F" + precision); }
                    if (statistics.bartlettPValue.HasValue) { strbartlettPValue = statistics.bartlettPValue.Value.ToString("F" + precision); }

                    if (statistics.ssBetween.HasValue && statistics.ssWithin.HasValue) { strTotalSSValue = (statistics.ssBetween.Value + statistics.ssWithin.Value).ToString("F" + precision); }
                    if (statistics.dfBetween.HasValue && statistics.dfWithin.HasValue) { strTotalDFValue = (statistics.dfBetween.Value + statistics.dfWithin.Value).ToString("F0"); }
                    if (statistics.kruskalWallisH.HasValue) { strKruskalWallisH = statistics.kruskalWallisH.Value.ToString("F" + precision); }
                    if (statistics.kruskalPValue.HasValue) { strKruskalPValue = statistics.kruskalPValue.Value.ToString("F" + precision); }

                    summaryText = "This table contains analysis of variance (ANOVA) statistics for the field " + cbxField.Text + ", cross-tabulated by " + cbxFieldCrosstab.Text + ". ";
                    summaryText += "The column headings for this table are: The variation, the SS value, the degrees of freedom, the MS value, and the F-statistic. There are three rows: The between, the within, and the total.";


                    // ANOVA
                    htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                    htmlBuilder.AppendLine("<p><strong>" + SharedStrings.ANOVA_DESCRIPTION_FOR_MEANS + "</strong><br />");
                    htmlBuilder.AppendLine("<small>" + SharedStrings.ANOVA_DESCRIPTION_FOR_MEANS_SUBTITLE + "</small></p>");
                    htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" summary=\"" + summaryText + "\">");
                    htmlBuilder.AppendLine(" <tr>");
                    htmlBuilder.AppendLine("   <th>Variation</th>");
                    htmlBuilder.AppendLine("   <th>SS</th>");
                    htmlBuilder.AppendLine("   <th>df</th>");
                    htmlBuilder.AppendLine("   <th>MS</th>");
                    htmlBuilder.AppendLine("   <th>F statistic</th>");
                    htmlBuilder.AppendLine(" </tr>");

                    htmlBuilder.AppendLine(" <tr>");
                    htmlBuilder.AppendLine("   <th>Between</th>");
                    htmlBuilder.AppendLine("   <td>" + strssBetweenValue + "</td>");
                    htmlBuilder.AppendLine("   <td>" + strdfBetweenValue + "</td>");
                    htmlBuilder.AppendLine("   <td>" + strmsBetweenValue + "</td>");
                    htmlBuilder.AppendLine("   <td>" + strfStatisticValue + "</td>");
                    htmlBuilder.AppendLine(" </tr>");

                    htmlBuilder.AppendLine(" <tr>");
                    htmlBuilder.AppendLine("   <th>Within</th>");
                    htmlBuilder.AppendLine("   <td>" + strssWithinValue + "</td>");
                    htmlBuilder.AppendLine("   <td>" + strdfWithinValue + "</td>");
                    htmlBuilder.AppendLine("   <td>" + strmsWithinValue + "</td>");
                    htmlBuilder.AppendLine("   <td>&nbsp;</td>");
                    htmlBuilder.AppendLine(" </tr>");

                    htmlBuilder.AppendLine(" <tr>");
                    htmlBuilder.AppendLine("   <th>Total</th>");
                    htmlBuilder.AppendLine("   <td>" + strTotalSSValue + "</td>");
                    htmlBuilder.AppendLine("   <td>" + strTotalDFValue + "</td>");
                    htmlBuilder.AppendLine("   <td>&nbsp;</td>");
                    htmlBuilder.AppendLine("   <td>&nbsp;</td>");
                    htmlBuilder.AppendLine(" </tr>");
                    htmlBuilder.AppendLine("</table>");

                    htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");

                    htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" summary=\"This table contains the ANOVA p-value.\">");
                    htmlBuilder.AppendLine(" <tr>");
                    htmlBuilder.AppendLine("   <th>P Value</th>");
                    htmlBuilder.AppendLine("   <td>" + stranovaPValueValue + "</td>");
                    htmlBuilder.AppendLine(" </tr>");
                    htmlBuilder.AppendLine("</table>");

                    // Bartlett
                    htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                    htmlBuilder.AppendLine("<p><strong>Bartlett's Test for Inequality of Population Variances</strong></p>");
                    htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
                    htmlBuilder.AppendLine(" <tr>");
                    htmlBuilder.AppendLine("   <th>Chi Square</th>");
                    htmlBuilder.AppendLine("   <td>" + strchiSquareValue + "</td>");
                    htmlBuilder.AppendLine(" </tr>");

                    htmlBuilder.AppendLine(" <tr>");
                    htmlBuilder.AppendLine("   <th>Degrees of freedom</th>");
                    htmlBuilder.AppendLine("   <td>" + strdfBetweenValue + "</td>");
                    htmlBuilder.AppendLine(" </tr>");

                    htmlBuilder.AppendLine(" <tr>");
                    htmlBuilder.AppendLine("   <th>P Value</th>");
                    htmlBuilder.AppendLine("   <td>" + strbartlettPValue + "</td>");
                    htmlBuilder.AppendLine("</table>");

                    htmlBuilder.AppendLine("<p><small>A small p-value (e.g., less than 0.05) suggests that the variances are not homogeneous and that the ANOVA may not be appropriate.</small></p>");

                    // Kruskal-Wallis
                    htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                    htmlBuilder.AppendLine("<p><strong>Mann-Whitney/Wilcoxon Two-Sample Test (Kruskal-Wallis test for two groups)</strong></p>");
                    htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" >");
                    htmlBuilder.AppendLine(" <tr>");
                    htmlBuilder.AppendLine("   <th>Kruskal-Wallis H</th>");
                    htmlBuilder.AppendLine("   <td>" + strKruskalWallisH + "</td>");
                    htmlBuilder.AppendLine(" </tr>");

                    htmlBuilder.AppendLine(" <tr>");
                    htmlBuilder.AppendLine("   <th>Degrees of freedom</th>");
                    htmlBuilder.AppendLine("   <td>" + strdfBetweenValue + "</td>");
                    htmlBuilder.AppendLine(" </tr>");

                    htmlBuilder.AppendLine(" <tr>");
                    htmlBuilder.AppendLine("   <th>P Value</th>");
                    htmlBuilder.AppendLine("   <td>" + strKruskalPValue + "</td>");
                    htmlBuilder.AppendLine("</table>");
                }
            }

            return htmlBuilder.ToString();
        }

        /// <summary>
        /// Gets/sets the gadget's custom output heading
        /// </summary>
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

        /// <summary>
        /// Gets/sets the gadget's custom output description
        /// </summary>
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

        /// <summary>
        /// Gets/sets the gadget's custom output caption for its table or image output components, if applicable
        /// </summary>
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

        #endregion        

        private void expanderAdvancedOptions_Expanded(object sender, RoutedEventArgs e)
        {

        }
    }
}
