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
using Epi.WPF.Dashboard.Rules;
using Epi.WPF.Dashboard.Controls;

namespace Epi.WPF.Dashboard.Gadgets.Analysis
{
    /// <summary>
    /// Interaction logic for CrosstabControl.xaml
    /// </summary>
    public partial class CrosstabControl : GadgetBase
    {
        #region Private Variables
        private RequestUpdateStatusDelegate requestUpdateStatus;
        private CheckForCancellationDelegate checkForCancellation;
        private List<TextBlock> gridDisclaimerList;        
        private List<Grid> strataChiSquareGridList;
        private List<GadgetTwoByTwoPanel> strata2x2GridList;
        private List<StackPanel> groupList;
        
        private List<GadgetStrataListPanel> strataListPanels;
        private GadgetStrataListPanel strataGroupPanel;
        
        private bool columnWarningShown;
        private int rowCount = 1;
        private int columnCount = 1;
        private Dictionary<string, string> inputVariableList;
        private bool exposureIsDropDownList = false;
        private bool exposureIsCommentLegal = false;
        private bool exposureIsOptionField = false;
        private bool outcomeIsNumeric = false;
        private bool smartTable = true;
        private bool showPercents = true;
        private bool? isRunningGrouped2x2 = null;

        #endregion

        #region Delegates
        
        private delegate void AddFreqGridDelegate(string strataVar, string value, string crosstabVar, string groupReference, DataTable table);
        private delegate void RenderFrequencyHeaderDelegate(string strataValue, string freqVar, DataColumnCollection columns);
        //private delegate void AddGridRowDelegate(string strataValue, int height);
        private delegate void FillPercentsDelegate(Grid grid);
        private delegate void AddChiSquareDelegate(double tableChiSq, double tableChiSqDf, double tableChiSqP, string disclaimer, string value);
        //private delegate void DrawFrequencyBordersDelegate(string strataValue);        

        #endregion

        #region Constructors

        public CrosstabControl()
        {
            InitializeComponent();
            Construct();
        }

        public CrosstabControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.dashboardHelper = dashboardHelper;
            Construct();
            FillComboboxes();
        }

        protected override void Construct()
        {
            if (!string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)"))
            {
                headerPanel.Text = CustomOutputHeading;
            }

            smartTable = (bool)checkboxSmartTable.IsChecked;
            showPercents = (bool)checkboxRowColPercents.IsChecked;

            inputVariableList = new Dictionary<string, string>();
            strataGridList = new List<Grid>();
            strataChiSquareGridList = new List<Grid>();
            strata2x2GridList = new List<Controls.GadgetTwoByTwoPanel>();
            groupList = new List<StackPanel>();
            gridDisclaimerList = new List<TextBlock>();
            strataListPanels = new List<GadgetStrataListPanel>();

            cbxExposureField.SelectionChanged += new SelectionChangedEventHandler(cbxField_SelectionChanged);
            cbxOutcomeField.SelectionChanged += new SelectionChangedEventHandler(cbxField_SelectionChanged);
            cbxFieldWeight.SelectionChanged += new SelectionChangedEventHandler(cbxField_SelectionChanged);
            //cbxFieldStrata.SelectionChanged += new SelectionChangedEventHandler(cbxField_SelectionChanged);            
            lbxFieldStrata.SelectionChanged += new SelectionChangedEventHandler(cbxField_SelectionChanged);

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

            checkboxAllValues.Checked += new RoutedEventHandler(checkboxCheckChanged);
            checkboxAllValues.Unchecked += new RoutedEventHandler(checkboxCheckChanged);

            checkboxIncludeMissing.Checked += new RoutedEventHandler(checkboxCheckChanged);
            checkboxIncludeMissing.Unchecked += new RoutedEventHandler(checkboxCheckChanged);

            checkboxOutcomeContinuous.Checked += new RoutedEventHandler(checkboxCheckChanged);
            checkboxOutcomeContinuous.Unchecked += new RoutedEventHandler(checkboxCheckChanged);

            checkboxSmartTable.Checked += new RoutedEventHandler(checkboxCheckChanged);
            checkboxSmartTable.Unchecked += new RoutedEventHandler(checkboxCheckChanged);

            checkboxRowColPercents.Checked += new RoutedEventHandler(checkboxRowColPercents_Checked);
            checkboxRowColPercents.Unchecked += new RoutedEventHandler(checkboxRowColPercents_Checked);

            checkboxHorizontal.Checked += new RoutedEventHandler(checkboxHorizontal_Checked);
            checkboxHorizontal.Unchecked += new RoutedEventHandler(checkboxHorizontal_Unchecked);

            checkboxStrataSummaryOnly.Checked += new RoutedEventHandler(checkboxStrataSummaryOnly_CheckChanged);
            checkboxStrataSummaryOnly.Unchecked += new RoutedEventHandler(checkboxStrataSummaryOnly_CheckChanged);
            
            columnWarningShown = false;

            this.IsProcessing = false;

            this.GadgetStatusUpdate += new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(IsCancelled);

            base.Construct();

            requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
            checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

            #region Translation
            ConfigExpandedTitle.Text = DashboardSharedStrings.GADGET_CONFIG_TITLE_MXN;
            tblockExposureField.Text = DashboardSharedStrings.GADGET_EXPOSURE_VARIABLE;
            tblockOutcomeField.Text = DashboardSharedStrings.GADGET_OUTCOME_VARIABLE;
            tblockWeightVariable.Text = DashboardSharedStrings.GADGET_WEIGHT_VARIABLE;
            tblockStratifyBy.Text = DashboardSharedStrings.GADGET_STRATA_VARIABLE;

            expanderAdvancedOptions.Header = DashboardSharedStrings.GADGET_ADVANCED_OPTIONS;
            checkboxStrataSummaryOnly.Content = DashboardSharedStrings.GADGET_STRATA_SUMMARY_ONLY;
            checkboxIncludeMissing.Content = DashboardSharedStrings.GADGET_INCLUDE_MISSING;
            checkboxAllValues.Content = DashboardSharedStrings.GADGET_ALL_LIST_VALUES;
            checkboxCommentLegalLabels.Content = DashboardSharedStrings.GADGET_LIST_LABELS;
            checkboxOutcomeContinuous.Content = DashboardSharedStrings.GADGET_OUTCOME_CONTINUOUS;
            checkboxRowColPercents.Content = DashboardSharedStrings.GADGET_ROW_COL_PERCENTS;
            checkboxSmartTable.Content = DashboardSharedStrings.GADGET_SMART_2X2;
            checkboxHorizontal.Content = DashboardSharedStrings.GADGET_DISPLAY_STRATAS_HORIZONTALLY;

            tblockMaxColumnLength.Text = DashboardSharedStrings.GADGET_MAX_COL_NAME_LENGTH;

            btnRun.Content = DashboardSharedStrings.GADGET_RUN_BUTTON;
            #endregion // Translation
        }

        void checkboxStrataSummaryOnly_CheckChanged(object sender, RoutedEventArgs e)
        {
        }

        void checkboxHorizontal_Unchecked(object sender, RoutedEventArgs e)
        {
            //panelMain.Orientation = Orientation.Vertical;
            foreach (StackPanel panel in this.groupList)
            {
                panel.Orientation = Orientation.Vertical;
            }

            foreach (Controls.GadgetTwoByTwoPanel panel in this.strata2x2GridList)
            {
                panel.Orientation = Orientation.Vertical;
            }
            CheckAndSetPosition();
        }

        void checkboxHorizontal_Checked(object sender, RoutedEventArgs e)
        {
            //panelMain.Orientation = Orientation.Horizontal;
            foreach (StackPanel panel in this.groupList)
            {
                panel.Orientation = Orientation.Horizontal;
            }

            foreach (Controls.GadgetTwoByTwoPanel panel in this.strata2x2GridList)
            {
                panel.Orientation = Orientation.Horizontal;
            }
            CheckAndSetPosition();
        }

        void lbxFieldStrata_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void checkboxRowColPercents_Checked(object sender, RoutedEventArgs e)
        {
            //RefreshResults();
        }

        #region Public Methods
        /// <summary>
        /// Returns the gadget's description as a string.
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return "Crosstabulation Gadget";
        }

        public override void CollapseOutput()
        {
            panelMain.Visibility = System.Windows.Visibility.Collapsed;

            if (!string.IsNullOrEmpty(this.infoPanel.Text))
            {
                this.infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            }

            this.messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            this.infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
        }

        public override void ExpandOutput()
        {
            panelMain.Visibility = System.Windows.Visibility.Visible;

            if (this.messagePanel.MessagePanelType != Controls.MessagePanelType.StatusPanel)
            {
                this.messagePanel.Visibility = System.Windows.Visibility.Visible;
            }

            if (!string.IsNullOrEmpty(this.infoPanel.Text))
            {
                this.infoPanel.Visibility = System.Windows.Visibility.Visible;
            }
        }
        #endregion

        /// <summary>
        /// Copies a grid's output to the clipboard
        /// </summary>
        protected override void CopyToClipboard()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Grid grid in this.strataGridList)
            {
                if (strataGridList.Count > 1)
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
                    else if (control is StackPanel)
                    {                        
                        int columnNumber = Grid.GetColumn(control);
                        string value = (((control as StackPanel).Children[0]) as TextBlock).Text;

                        sb.Append(value + "\t");

                        if (columnNumber >= grid.ColumnDefinitions.Count - 1)
                        {
                            sb.AppendLine();
                        }
                    }
                }

                sb.AppendLine();
            }

            foreach (GadgetTwoByTwoPanel grid2x2 in this.strata2x2GridList)
            {
                sb.AppendLine(grid2x2.ExposureVariable + " by " + grid2x2.OutcomeVariable);
                sb.AppendLine();
                sb.AppendLine("\t" + grid2x2.OutcomeYesLabel + "\t" + grid2x2.OutcomeNoLabel + "\tTotal");
                sb.AppendLine(grid2x2.ExposureYesLabel + "\t" + grid2x2.YesYesValue + "\t" + grid2x2.YesNoValue + "\t" + grid2x2.TotalYesRow.Value.ToString());
                sb.AppendLine(grid2x2.ExposureNoLabel + "\t" + grid2x2.NoYesValue + "\t" + grid2x2.NoNoValue + "\t" + grid2x2.TotalNoRow.Value.ToString());
                sb.AppendLine("Total\t" + grid2x2.TotalYesCol.Value.ToString() + "\t" + grid2x2.TotalNoCol.Value.ToString() + "\t" + grid2x2.TotalValue.Value.ToString());
                sb.AppendLine();                

                sb.AppendLine("(Exposure = Rows; Outcome = Columns)");

                sb.AppendLine();
            }

            Clipboard.Clear();
            Clipboard.SetText(sb.ToString());
        }

        private void ClearResults()
        {
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Text = string.Empty;
            descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;

            foreach (Grid grid in strataGridList)
            {
                grid.Children.Clear();
                grid.RowDefinitions.Clear();
            }

            foreach (Grid grid in strataChiSquareGridList)
            {
                grid.Children.Clear();
                grid.RowDefinitions.Clear();
            }

            foreach (TextBlock textBlock in gridDisclaimerList)
            {
                panelMain.Children.Remove(textBlock);
            }

            foreach (GadgetStrataListPanel strataPanel in strataListPanels)
            {
                strataPanel.RowClicked -= new StrataGridRowClickedHandler(strataListPanel_RowClicked);
                strataPanel.Clear();
            }

            strataGroupPanel = null;
            strataListPanels.Clear();
            strataGridList.Clear();
            strataChiSquareGridList.Clear();
            strata2x2GridList.Clear();
            groupList.Clear();
            strataExpanderList.Clear();

            panelMain.Children.Clear();
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

            for (int i = 0; i < strataGridList.Count; i++)
            {
                strataGridList[i].Children.Clear();
            }
            this.strataGridList.Clear();

            for (int i = 0; i < strataChiSquareGridList.Count; i++)
            {
                strataChiSquareGridList[i].Children.Clear();
            }

            foreach (GadgetTwoByTwoPanel grid2x2 in this.strata2x2GridList)
            {
                grid2x2.Dispose();
                grid2x2.ValuesUpdated -= new TwoByTwoValuesUpdatedHandler(twoByTwoPanel_ValuesUpdated);
            }

            foreach (GadgetStrataListPanel strataPanel in strataListPanels)
            {
                strataPanel.RowClicked -= new StrataGridRowClickedHandler(strataListPanel_RowClicked);
                strataPanel.Clear();
            }

            strataListPanels.Clear();
            strataGroupPanel = null;
            this.strataChiSquareGridList.Clear();
            this.strata2x2GridList.Clear();
            this.groupList.Clear();
            this.strataExpanderList.Clear();
            this.panelMain.Children.Clear();

            base.CloseGadget();

            GadgetOptions = null;
        }

        /// <summary>
        /// Handles the check / unchecked events
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void checkboxCheckChanged(object sender, RoutedEventArgs e)
        {
            if(sender == checkboxOutcomeContinuous && checkboxOutcomeContinuous.IsChecked == true) 
            {
                LoadingCombos = true;
                checkboxIncludeMissing.IsChecked = false;
                LoadingCombos = false;
            }
            else if (sender == checkboxIncludeMissing && checkboxIncludeMissing.IsChecked == true)
            {
                LoadingCombos = true;
                checkboxOutcomeContinuous.IsChecked = false;
                LoadingCombos = false;
            }

            //RefreshResults();
        }
        
        void cbxField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckVariables();
        }
        #endregion        

        #region Event Handlers

        private void SmartTable(DataTable table)
        {
            if (smartTable && table.Columns.Count == 3 && table.Rows.Count == 2)
            {
                Dictionary<string, string> booleanValues = new Dictionary<string, string>();
                booleanValues.Add("0", "1");
                booleanValues.Add("false", "true");
                booleanValues.Add("f", "t");
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

        private void AddStrata2x2Statistics()
        {
            if (this.strataGridList.Count == 0 && this.strata2x2GridList.Count >= 2 && this.GadgetOptions.StrataVariableNames.Count >= 1 && !dashboardHelper.GetAllGroupsAsList().Contains(GadgetOptions.MainVariableName))
            {
                // We have two or more 2x2 tables and stratification (non-grouped). Get 2x2 strata summary results.

                List<double> yyList = new List<double>();
                List<double> ynList = new List<double>();
                List<double> nyList = new List<double>();
                List<double> nnList = new List<double>();

                foreach (Controls.GadgetTwoByTwoPanel twoByTwoPanel in this.strata2x2GridList)
                {
                    if (twoByTwoPanel.YesYesValue.HasValue && twoByTwoPanel.YesNoValue.HasValue && twoByTwoPanel.NoYesValue.HasValue && twoByTwoPanel.NoNoValue.HasValue)
                    {
                        yyList.Add((double)twoByTwoPanel.YesYesValue.Value);
                        ynList.Add((double)twoByTwoPanel.YesNoValue.Value);
                        nyList.Add((double)twoByTwoPanel.NoYesValue.Value);
                        nnList.Add((double)twoByTwoPanel.NoNoValue.Value);
                    }
                }

                //double yySum = yyList.Sum();
                //double ynSum = yyList.Sum();
                //double nySum = yyList.Sum();
                //double nnSum = yyList.Sum();

                //List<double>[] lists = new List<double>[4];
                //lists[0] = yyList;
                //lists[1] = ynList;
                //lists[2] = nyList;
                //lists[3] = nnList;

                Controls.StratifiedTableAnalysisPanel stap = new StratifiedTableAnalysisPanel();                
                stap.YesYesList = yyList;
                stap.YesNoList = ynList;
                stap.NoYesList = nyList;
                stap.NoNoList = nnList;

                stap.Margin = new Thickness(10);
                stap.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                stap.Compute();

                panelMain.Children.Add(stap);

                //System.ComponentModel.BackgroundWorker worker = new System.ComponentModel.BackgroundWorker();
                //worker.DoWork += new System.ComponentModel.DoWorkEventHandler(strataTwoByTwoWorker_DoWork);
                //worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(strataTwoByTwoWorker_RunWorkerCompleted);
                //worker.RunWorkerAsync(lists);
            }
        }

        private void UpdateStrata2x2Statistics()
        {
            foreach(UIElement element in panelMain.Children)
            {
                if (element is StratifiedTableAnalysisPanel)
                {
                    StratifiedTableAnalysisPanel stap = element as StratifiedTableAnalysisPanel;

                    List<double> yyList = new List<double>();
                    List<double> ynList = new List<double>();
                    List<double> nyList = new List<double>();
                    List<double> nnList = new List<double>();

                    foreach (Controls.GadgetTwoByTwoPanel twoByTwoPanel in this.strata2x2GridList)
                    {
                        if (twoByTwoPanel.YesYesValue.HasValue && twoByTwoPanel.YesNoValue.HasValue && twoByTwoPanel.NoYesValue.HasValue && twoByTwoPanel.NoNoValue.HasValue)
                        {
                            yyList.Add((double)twoByTwoPanel.YesYesValue.Value);
                            ynList.Add((double)twoByTwoPanel.YesNoValue.Value);
                            nyList.Add((double)twoByTwoPanel.NoYesValue.Value);
                            nnList.Add((double)twoByTwoPanel.NoNoValue.Value);
                        }
                    }

                    // necessary
                    stap.YesYesList = new List<double>();
                    stap.YesNoList = new List<double>();
                    stap.NoYesList = new List<double>();
                    stap.NoNoList = new List<double>();

                    stap.YesYesList = yyList;
                    stap.YesNoList = ynList;
                    stap.NoYesList = nyList;
                    stap.NoNoList = nnList;

                    stap.Compute();
                }
            }
        }

        protected override void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Debug.Print("Background worker thread for frequency gadget was cancelled or ran to completion.");
            this.Dispatcher.BeginInvoke(new SimpleCallback(AddStrata2x2Statistics));
        }

        protected override void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (syncLock)
            {
                Dictionary<string, string> inputVariableList = ((GadgetParameters)e.Argument).InputVariableList;

                //Stopwatch stopwatch = new Stopwatch();
                //stopwatch.Start();

                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));     
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));

                AddFreqGridDelegate addGrid = new AddFreqGridDelegate(AddFreqGrid);
                SetGridTextDelegate setText = new SetGridTextDelegate(SetGridText);
                AddGridRowDelegate addRow = new AddGridRowDelegate(AddGridRow);
                RenderFrequencyHeaderDelegate renderHeader = new RenderFrequencyHeaderDelegate(RenderFrequencyHeader);
                DrawFrequencyBordersDelegate drawBorders = new DrawFrequencyBordersDelegate(DrawOutputGridBorders);

                string freqVar = string.Empty;
                string weightVar = string.Empty;
                string strataVar = string.Empty;
                string crosstabVar = string.Empty;
                bool includeMissing = false;
                bool outcomeContinuous = false;
                bool showStrataSummaryOnly = false;

                if (inputVariableList.ContainsKey("freqvar"))
                {
                    freqVar = inputVariableList["freqvar"];
                }

                if (inputVariableList.ContainsKey("crosstabvar"))
                {
                    crosstabVar = inputVariableList["crosstabvar"];
                }

                if (inputVariableList.ContainsKey("weightvar"))
                {
                    weightVar = inputVariableList["weightvar"];
                }

                if (inputVariableList.ContainsKey("stratavar"))
                {
                    strataVar = inputVariableList["stratavar"];
                }

                if (inputVariableList.ContainsKey("includemissing"))
                {
                    if (inputVariableList["includemissing"].Equals("true"))
                    {
                        includeMissing = true;
                    }
                }

                if (inputVariableList.ContainsKey("stratasummaryonly"))
                {
                    if (inputVariableList["stratasummaryonly"].Equals("true"))
                    {
                        showStrataSummaryOnly = true;
                    }
                }                

                if (inputVariableList.ContainsKey("treatoutcomeascontinuous"))
                {
                    if (inputVariableList["treatoutcomeascontinuous"].Equals("true"))
                    {
                        outcomeContinuous = true;
                    }
                }

                List<string> stratas = new List<string>();
                if (!string.IsNullOrEmpty(strataVar))
                {
                    stratas.Add(strataVar);
                }

                try
                {
                    GadgetOptions.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                    GadgetOptions.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);

                    if (this.DataFilters != null && this.DataFilters.Count > 0)
                    {
                        GadgetOptions.CustomFilter = this.DataFilters.GenerateDataFilterString(false);
                    }
                    else
                    {
                        GadgetOptions.CustomFilter = string.Empty;
                    }

                    bool runGroup = false;
                    
                    Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables = new Dictionary<DataTable, List<DescriptiveStatistics>>();

                    List<string> allGroupFields = dashboardHelper.GetAllGroupsAsList();

                    if (GadgetOptions.MainVariableNames != null && GadgetOptions.MainVariableNames.Count > 0)
                    {
                        Dictionary<DataTable, List<DescriptiveStatistics>> grpTables = new Dictionary<DataTable, List<DescriptiveStatistics>>();

                        foreach (string mainVariableName in GadgetOptions.MainVariableNames)
                        {
                            GadgetParameters newOptions = new GadgetParameters(GadgetOptions);
                            newOptions.MainVariableNames = null;
                            newOptions.MainVariableName = mainVariableName;

                            grpTables = dashboardHelper.GenerateFrequencyTable(newOptions);

                            foreach (KeyValuePair<DataTable, List<DescriptiveStatistics>> kvp in grpTables)
                            {
                                stratifiedFrequencyTables.Add(kvp.Key, kvp.Value);
                            }
                        }                        
                        runGroup = true;
                    }
                    else if (allGroupFields.Contains(GadgetOptions.MainVariableName))
                    {
                        Dictionary<DataTable, List<DescriptiveStatistics>> grpTables = new Dictionary<DataTable, List<DescriptiveStatistics>>();

                        foreach (string variableName in dashboardHelper.GetVariablesInGroup(GadgetOptions.MainVariableName))
                        {
                            if (!allGroupFields.Contains(variableName) && dashboardHelper.TableColumnNames.ContainsKey(variableName))
                            {
                                GadgetParameters newOptions = new GadgetParameters(GadgetOptions);
                                newOptions.MainVariableNames = null;
                                newOptions.MainVariableName = variableName;

                                grpTables = dashboardHelper.GenerateFrequencyTable(newOptions);

                                foreach (KeyValuePair<DataTable, List<DescriptiveStatistics>> kvp in grpTables)
                                {
                                    stratifiedFrequencyTables.Add(kvp.Key, kvp.Value);
                                }
                            }
                        }
                        runGroup = true;
                    }
                    else
                    {
                        stratifiedFrequencyTables = dashboardHelper.GenerateFrequencyTable(GadgetOptions/*, freqVar, weightVar, stratas, crosstabVar, useAllPossibleValues, sortHighLow, includeMissing, false*/);
                        runGroup = false;
                    }

                    if (runGroup) showStrataSummaryOnly = false;

                    if (stratifiedFrequencyTables == null || stratifiedFrequencyTables.Count == 0)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), DashboardSharedStrings.GADGET_MSG_NO_DATA);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        return;
                    }
                    else if (worker.CancellationPending)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.DASHBOARD_GADGET_STATUS_OPERATION_CANCELLED);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        //Debug.Print("Thread cancelled");
                        return;
                    }
                    else
                    {
                        string formatString = string.Empty;

                        foreach (KeyValuePair<DataTable, List<DescriptiveStatistics>> tableKvp in stratifiedFrequencyTables)
                        {
                            SmartTable(tableKvp.Key);

                            string strataValue = tableKvp.Key.TableName;

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
                            else if (count == 0)
                            {
                                continue;
                            }
                            DataTable frequencies = tableKvp.Key;

                            if (frequencies.Rows.Count == 0)
                            {
                                continue;
                            }

                            if (outcomeContinuous)
                            {
                                if (frequencies.Columns.Count <= 128)
                                {
                                    int min;
                                    int max;

                                    if (int.TryParse(frequencies.Columns[1].ColumnName, out min) && int.TryParse(frequencies.Columns[frequencies.Columns.Count - 1].ColumnName, out max))
                                    {
                                        bool addedColumns = false;

                                        for (int i = min; i <= max; i++)
                                        {
                                            if (!frequencies.Columns.Contains(i.ToString()))
                                            {
                                                DataColumn newColumn = new DataColumn(i.ToString(), typeof(double));
                                                newColumn.DefaultValue = 0;
                                                frequencies.Columns.Add(newColumn);
                                                addedColumns = true;
                                            }
                                        }

                                        if (addedColumns)
                                        {
                                            int ordinal = 1;
                                            for (int i = min; i <= max; i++)
                                            {
                                                if (frequencies.Columns.Contains(i.ToString()))
                                                {
                                                    frequencies.Columns[i.ToString()].SetOrdinal(ordinal);
                                                    ordinal++;
                                                }
                                            }
                                        }
                                    }
                                }
                            }                            
                            
                            this.Dispatcher.BeginInvoke(addGrid, strataVar, frequencies.TableName, crosstabVar, frequencies.Columns[0].ToString(), frequencies);
                        }

                        foreach (KeyValuePair<DataTable, List<DescriptiveStatistics>> tableKvp in stratifiedFrequencyTables)
                        {
                            if (runGroup)
                            {
                                freqVar = tableKvp.Key.Columns[0].ColumnName;
                            }

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
                            DataTable frequencies = tableKvp.Key;

                            if (frequencies.Rows.Count == 0)
                            {
                                continue;
                            }

                            string tableHeading = tableKvp.Key.TableName;

                            if (stratifiedFrequencyTables.Count > 1)
                            {
                                tableHeading = freqVar;
                            }

                            this.Dispatcher.BeginInvoke(renderHeader, strataValue, tableHeading, frequencies.Columns);
                            
                            rowCount = 1;

                            int[] totals = new int[frequencies.Columns.Count - 1];
                            columnCount = 1;

                            DataRow[] SortedRows = new DataRow[frequencies.Rows.Count];
                            int rowcounter = 0;
                            foreach (System.Data.DataRow row in frequencies.Rows)
                            {
                                SortedRows[rowcounter++] = row;
                                if (!row[freqVar].Equals(DBNull.Value) || (row[freqVar].Equals(DBNull.Value) && includeMissing == true))
                                {
                                    this.Dispatcher.Invoke(addRow, strataValue, -1);
                                    string displayValue = row[freqVar].ToString();

                                    if (dashboardHelper.IsUserDefinedColumn(freqVar))
                                    {
                                        displayValue = dashboardHelper.GetFormattedOutput(freqVar, row[freqVar]);
                                    }
                                    else
                                    {
                                        if (dashboardHelper.IsUsingEpiProject && View.Fields[freqVar] is YesNoField)
                                        {
                                            if (row[freqVar].ToString().Equals("1"))
                                                displayValue = "Yes";
                                            else if (row[freqVar].ToString().Equals("0"))
                                                displayValue = "No";
                                        }
                                        else if (dashboardHelper.IsUsingEpiProject && View.Fields[freqVar] is DateField)
                                        {
                                            displayValue = string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:d}", row[freqVar]);
                                        }
                                        else if (dashboardHelper.IsUsingEpiProject && View.Fields[freqVar] is TimeField)
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

                                    this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(displayValue, new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Left, TextAlignment.Left, rowCount, 0, System.Windows.Visibility.Visible), FontWeights.Normal);

                                    
                                    int rowTotal = 0;
                                    columnCount = 1;

                                    foreach (DataColumn column in frequencies.Columns)
                                    {
                                        if (column.ColumnName.Equals(freqVar))
                                        {
                                            continue;
                                        }

                                        this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(row[column.ColumnName].ToString(), new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, columnCount, System.Windows.Visibility.Visible), FontWeights.Normal);
                                        columnCount++;

                                        int rowValue = 0;
                                        bool success = int.TryParse(row[column.ColumnName].ToString(), out rowValue);
                                        if (success)
                                        {
                                            totals[columnCount - 2] = totals[columnCount - 2] + rowValue;
                                            rowTotal = rowTotal + rowValue;
                                        }
                                    }
                                    this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(rowTotal.ToString(), new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, columnCount, System.Windows.Visibility.Visible), FontWeights.Bold);
                                    rowCount++;
                                }                                
                            }

                            double[] tableChiSq = Epi.Statistics.SingleMxN.CalcChiSq(SortedRows, false);
                            double tableChiSqDF = (double)(SortedRows.Length - 1) * (SortedRows[0].ItemArray.Length - 2);
                            double tableChiSqP = Epi.Statistics.SharedResources.PValFromChiSq(tableChiSq[0], tableChiSqDF);
                            String disclaimer = "";
                            if (tableChiSq[1] == 1.0)
                                disclaimer = SharedStrings.TABLES_CHI_SQUARE_NOT_VALID;

                            this.Dispatcher.BeginInvoke(new AddChiSquareDelegate(RenderChiSquare), tableChiSq[0], tableChiSqDF, tableChiSqP, disclaimer, strataValue);
                            this.Dispatcher.BeginInvoke(new AddGridFooterDelegate(RenderFrequencyFooter), strataValue, rowCount, totals);
                            this.Dispatcher.BeginInvoke(drawBorders, strataValue);
                        }
                    }

                    
                    this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));

                    // check for existence of 2x2 table...
                    if (rowCount == 3 && columnCount == 3)
                    {
                    }

                    stratifiedFrequencyTables.Clear();                    
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                }
                finally
                {
                    //stopwatch.Stop();
                    //Debug.Print("Crosstab gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete with " + dashboardHelper.RecordCount.ToString() + " records and the following filters:");
                    //Debug.Print(dashboardHelper.DataFilters.GenerateDataFilterString());
                }
            }
        }

        #endregion

        #region Private Methods

        private void FillComboboxes(bool update = false)
        {
            LoadingCombos = true;

            string prevExposureField = string.Empty;
            string prevOutcomeField = string.Empty;
            string prevWeightField = string.Empty;
            //string prevStrataField = string.Empty;
            List<string> prevStrataFields = new List<string>();

            if (update)
            {
                if (cbxExposureField.SelectedIndex >= 0)
                {
                    prevExposureField = cbxExposureField.SelectedItem.ToString();
                }
                if (cbxOutcomeField.SelectedIndex >= 0)
                {
                    prevOutcomeField = cbxOutcomeField.SelectedItem.ToString();
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
            }            

            cbxExposureField.ItemsSource = null;
            cbxExposureField.Items.Clear();

            cbxOutcomeField.ItemsSource = null;
            cbxOutcomeField.Items.Clear();

            cbxFieldWeight.ItemsSource = null;
            cbxFieldWeight.Items.Clear();

            //cbxFieldStrata.ItemsSource = null;
            //cbxFieldStrata.Items.Clear();

            lbxFieldStrata.ItemsSource = null;
            lbxFieldStrata.Items.Clear();

            List<string> fieldNames = new List<string>();
            List<string> weightFieldNames = new List<string>();
            List<string> strataFieldNames = new List<string>();

            weightFieldNames.Add(string.Empty);
            strataFieldNames.Add(string.Empty);
            
            ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.DateTime | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.UserDefined;
            fieldNames = dashboardHelper.GetFieldsAsList(columnDataType);

            columnDataType = ColumnDataType.Numeric | ColumnDataType.UserDefined;
            weightFieldNames.AddRange(dashboardHelper.GetFieldsAsList(columnDataType));

            columnDataType = ColumnDataType.Numeric | ColumnDataType.Boolean | ColumnDataType.Text | ColumnDataType.UserDefined;
            strataFieldNames.AddRange(dashboardHelper.GetFieldsAsList(columnDataType));            

            fieldNames.Sort();
            weightFieldNames.Sort();
            strataFieldNames.Sort();

            if (fieldNames.Contains("SYSTEMDATE"))
            {
                fieldNames.Remove("SYSTEMDATE");
            }

            if (DashboardHelper.IsUsingEpiProject)
            {
                if (fieldNames.Contains("RecStatus")) fieldNames.Remove("RecStatus");
                if (weightFieldNames.Contains("RecStatus")) weightFieldNames.Remove("RecStatus");

                if (strataFieldNames.Contains("RecStatus")) strataFieldNames.Remove("RecStatus");
                if (strataFieldNames.Contains("FKEY")) strataFieldNames.Remove("FKEY");
                if (strataFieldNames.Contains("GlobalRecordId")) strataFieldNames.Remove("GlobalRecordId");
            }

            fieldNames.AddRange(dashboardHelper.GetAllGroupsAsList());

            cbxExposureField.ItemsSource = fieldNames;
            cbxOutcomeField.ItemsSource = fieldNames;
            cbxFieldWeight.ItemsSource = weightFieldNames;
            //cbxFieldStrata.ItemsSource = strataFieldNames;
            lbxFieldStrata.ItemsSource = strataFieldNames;

            if (cbxExposureField.Items.Count > 0)
            {
                cbxExposureField.SelectedIndex = -1;
                cbxOutcomeField.SelectedIndex = -1;
            }
            if (cbxFieldWeight.Items.Count > 0)
            {
                cbxFieldWeight.SelectedIndex = -1;
            }
            //if (cbxFieldStrata.Items.Count > 0)
            //{
            //    cbxFieldStrata.SelectedIndex = -1;
            //}            

            if (update)
            {
                cbxExposureField.SelectedItem = prevExposureField;
                cbxOutcomeField.SelectedItem = prevOutcomeField;
                cbxFieldWeight.SelectedItem = prevWeightField;
                //cbxFieldStrata.SelectedItem = prevStrataField;

                foreach (string s in prevStrataFields)
                {
                    lbxFieldStrata.SelectedItems.Add(s);
                }
            }

            LoadingCombos = false;
        }

        private void AddFreqGrid(string strataVar, string value, string crosstabVar, string groupReference, DataTable table)
        {
            StackPanel crosstabPanel = new StackPanel();
            StackPanel groupPanel = new StackPanel();

            bool strataSummaryOnly = (bool)checkboxStrataSummaryOnly.IsChecked;
            bool isGrouped = this.DashboardHelper.GetAllGroupsAsList().Contains(GadgetOptions.MainVariableName);
            if (isGrouped && strataGroupPanel == null)
            {
                strataGroupPanel = new GadgetStrataListPanel(StrataListPanelMode.GroupMode);
                strataGroupPanel.SnapsToDevicePixels = true;
                panelMain.Children.Add(strataGroupPanel);
            }

            groupPanel.Tag = groupReference;
            bool inList = false;

            foreach (StackPanel panel in groupList)
            {
                if (panel.Tag == groupPanel.Tag)
                {
                    groupPanel = panel;
                    inList = true;
                }
            }

            if (!inList)
            {
                groupList.Add(groupPanel);                
            }

            int columnCount = table.Columns.Count;

            Expander expander = new Expander();

            TextBlock txtExpanderHeader = new TextBlock();
            txtExpanderHeader.Text = value;
            txtExpanderHeader.Style = this.Resources["genericOutputExpanderText"] as Style;

            string formattedValue = value;

            if (string.IsNullOrEmpty(strataVar) && GadgetOptions.StrataVariableNames.Count == 0)
            {
                formattedValue = crosstabVar;
            }
            else
            {
                if (value.EndsWith(" = "))
                {
                    formattedValue = value + dashboardHelper.Config.Settings.RepresentationOfMissing;
                }
            }

            if (isGrouped)
            {
                formattedValue = groupReference + ", " + formattedValue;
            }

            txtExpanderHeader.Text = formattedValue;
            expander.Header = txtExpanderHeader;

            if (columnCount == 3 && table.Rows.Count == 2 && (isRunningGrouped2x2 == null || isRunningGrouped2x2 == true)) // is 2x2
            {
                isRunningGrouped2x2 = true;

                decimal yyVal = decimal.Parse(table.Rows[0][1].ToString());
                decimal ynVal = decimal.Parse(table.Rows[0][2].ToString());
                decimal nyVal = decimal.Parse(table.Rows[1][1].ToString());
                decimal nnVal = decimal.Parse(table.Rows[1][2].ToString());

                Controls.GadgetTwoByTwoPanel twoByTwoPanel = new Controls.GadgetTwoByTwoPanel(yyVal, ynVal, nyVal, nnVal);
                twoByTwoPanel.ValuesUpdated += new TwoByTwoValuesUpdatedHandler(twoByTwoPanel_ValuesUpdated);

                if (panelMain.Orientation == Orientation.Horizontal)
                {
                    twoByTwoPanel.Orientation = Orientation.Horizontal;
                }
                else
                {
                    twoByTwoPanel.Orientation = Orientation.Vertical;
                }

                twoByTwoPanel.Margin = new Thickness(5);

                if (isGrouped)
                {
                    twoByTwoPanel.ExposureVariable = groupReference;
                }
                else
                {
                    twoByTwoPanel.ExposureVariable = this.GadgetOptions.MainVariableName;
                    if (strataSummaryOnly)
                    {
                        twoByTwoPanel.Visibility = System.Windows.Visibility.Collapsed;
                        expander.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
                twoByTwoPanel.OutcomeVariable = this.GadgetOptions.CrosstabVariableName;

                twoByTwoPanel.OutcomeYesLabel = table.Columns[1].ColumnName;
                twoByTwoPanel.OutcomeNoLabel = table.Columns[2].ColumnName;

                twoByTwoPanel.ExposureYesLabel = table.Rows[0][0].ToString();
                twoByTwoPanel.ExposureNoLabel = table.Rows[1][0].ToString();

                if (showPercents)
                {
                    twoByTwoPanel.ShowRowColumnPercents = true;
                }
                else
                {
                    twoByTwoPanel.ShowRowColumnPercents = false;
                }

                if (string.IsNullOrEmpty(strataVar) && GadgetOptions.StrataVariableNames.Count == 0)
                {                    
                    crosstabPanel.Margin = (Thickness)this.Resources["genericElementMargin"];
                    crosstabPanel.Children.Add(twoByTwoPanel);

                    if (isGrouped && strataGroupPanel != null)
                    {
                        StrataGridListRow sRow = new StrataGridListRow();
                        sRow.StrataLabel = groupReference;
                        sRow.OutcomeRateExposure = twoByTwoPanel.OutcomeRateExposure;
                        sRow.OutcomeRateNoExposure = twoByTwoPanel.OutcomeRateNoExposure;
                        sRow.RiskRatio = twoByTwoPanel.RiskRatio;
                        sRow.OddsRatio = twoByTwoPanel.OddsRatio;
                        sRow.OddsLower = twoByTwoPanel.OddsRatioLower;
                        sRow.OddsUpper = twoByTwoPanel.OddsRatioUpper;
                        strataGroupPanel.AddRow(sRow);
                        strataGroupPanel.RowClicked += new StrataGridRowClickedHandler(strataGroupPanel_NoStrata_RowClicked);
                        strataGroupPanel.ExpandAllClicked += new StrataGridExpandAllClickedHandler(strataGroupPanel_NoStrata_ExpandAllClicked);
                    }

                    groupPanel.Children.Add(crosstabPanel);
                    //panelMain.Children.Add(crosstabPanel);
                    if (!panelMain.Children.Contains(groupPanel))
                    {
                        panelMain.Children.Add(groupPanel);
                    }
                }
                else
                {
                    GadgetStrataListPanel strataListPanel = new GadgetStrataListPanel();

                    bool found = false;
                    foreach (UIElement element in groupPanel.Children)
                    {
                        if (element is GadgetStrataListPanel)
                        {
                            found = true;
                            strataListPanel = (element as GadgetStrataListPanel);
                            break;
                        }
                    }                    

                    if (!found)
                    {                        
                        strataListPanel.RowClicked += new StrataGridRowClickedHandler(strataListPanel_RowClicked);
                        strataListPanel.ExpandAllClicked += new StrataGridExpandAllClickedHandler(strataListPanel_ExpandAllClicked);
                        strataListPanel.SnapsToDevicePixels = true;
                        strataListPanels.Add(strataListPanel);
                        groupPanel.Children.Add(strataListPanel);

                        if (!isGrouped && strataSummaryOnly)
                        {
                            strataListPanel.Visibility = System.Windows.Visibility.Collapsed;
                        }
                        //panelMain.Children.Add(strataListPanel);                        
                    }

                    StrataGridListRow sRow = new StrataGridListRow();
                    sRow.StrataLabel = formattedValue;
                    sRow.OutcomeRateExposure = twoByTwoPanel.OutcomeRateExposure;
                    sRow.OutcomeRateNoExposure = twoByTwoPanel.OutcomeRateNoExposure;
                    sRow.RiskRatio = twoByTwoPanel.RiskRatio;
                    sRow.OddsRatio = twoByTwoPanel.OddsRatio;
                    sRow.OddsLower = twoByTwoPanel.OddsRatioLower;
                    sRow.OddsUpper = twoByTwoPanel.OddsRatioUpper;

                    strataListPanel.AddRow(sRow);
                    if (isGrouped && strataGroupPanel != null)
                    {
                        strataListPanel.ExpanderHeader = groupReference;
                        sRow = new StrataGridListRow();
                        sRow.StrataLabel = groupReference;
                        sRow.OutcomeRateExposure = null;
                        sRow.OutcomeRateNoExposure = null;
                        sRow.RiskRatio = null;
                        sRow.OddsRatio = null;
                        sRow.OddsLower = null;
                        sRow.OddsUpper = null;                        
                        strataGroupPanel.AddRow(sRow);
                        strataGroupPanel.RowClicked += new StrataGridRowClickedHandler(strataGroupPanel_RowClicked);
                        strataGroupPanel.ExpandAllClicked += new StrataGridExpandAllClickedHandler(strataGroupPanel_ExpandAllClicked);
                    }

                    twoByTwoPanel.Tag = formattedValue;
                    crosstabPanel.Margin = (Thickness)this.Resources["genericElementMargin"];
                    crosstabPanel.Children.Add(twoByTwoPanel);
                    expander.Margin = (Thickness)this.Resources["expanderMargin"];
                    expander.IsExpanded = true;
                    expander.Content = crosstabPanel;
                    groupPanel.Children.Add(expander);
                    //panelMain.Children.Add(expander);
                    strataExpanderList.Add(expander);

                    if (groupList.Count == 0)
                    {
                        groupList.Add(groupPanel);
                    }

                    if (!panelMain.Children.Contains(groupPanel))
                    {
                        panelMain.Children.Add(groupPanel);
                    }
                }

                strata2x2GridList.Add(twoByTwoPanel);
                
                return;
            }
            else if ((columnCount != 3 || table.Rows.Count != 2) && (isRunningGrouped2x2 == true))
            {
                return;
            }

            isRunningGrouped2x2 = false;

            Grid chiSquaregrid = new Grid();
            chiSquaregrid.Tag = value;

            chiSquaregrid.HorizontalAlignment = HorizontalAlignment.Center;
            chiSquaregrid.Margin = new Thickness(0, 5, 0, 10);
            chiSquaregrid.Visibility = System.Windows.Visibility.Collapsed;
            
            chiSquaregrid.ColumnDefinitions.Add(new ColumnDefinition());
            chiSquaregrid.ColumnDefinitions.Add(new ColumnDefinition());
            chiSquaregrid.ColumnDefinitions.Add(new ColumnDefinition());

            chiSquaregrid.RowDefinitions.Add(new RowDefinition());
            chiSquaregrid.RowDefinitions.Add(new RowDefinition());

            Grid grid = new Grid();
            grid.Tag = value;
            grid.Style = this.Resources["genericOutputGrid"] as Style;                        
            grid.Visibility = System.Windows.Visibility.Collapsed;
            grid.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            for (int i = 0; i < columnCount; i++)
            {
                ColumnDefinition column = new ColumnDefinition();
                column.Width = GridLength.Auto;
                grid.ColumnDefinitions.Add(column);
            }

            ColumnDefinition totalColumn = new ColumnDefinition();
            totalColumn.Width = GridLength.Auto;
            grid.ColumnDefinitions.Add(totalColumn);

            ScrollViewer sv = new ScrollViewer();
            sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            sv.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            sv.MaxHeight = System.Windows.SystemParameters.PrimaryScreenHeight - 320;
            sv.MaxWidth = System.Windows.SystemParameters.PrimaryScreenWidth - 300;
            sv.Margin = (Thickness)this.Resources["expanderMargin"];

            Grid gridOuter = new Grid();            
            gridOuter.ColumnDefinitions.Add(new ColumnDefinition());
            gridOuter.ColumnDefinitions.Add(new ColumnDefinition());
            gridOuter.RowDefinitions.Add(new RowDefinition());
            gridOuter.RowDefinitions.Add(new RowDefinition());            

            Grid.SetColumn(grid, 1);
            Grid.SetRow(grid, 1);
            gridOuter.Children.Add(grid);

            TextBlock tblock1 = new TextBlock();

            RotateTransform rotate = new RotateTransform(270);
            tblock1.RenderTransform = rotate;

            tblock1.Margin = new Thickness(0, 0, -5, 0);
            tblock1.FontWeight = FontWeights.Bold;
            tblock1.FontSize = tblock1.FontSize + 2;
            if (dashboardHelper.GetAllGroupsAsList().Contains(cbxExposureField.SelectedItem.ToString()))
            {
                tblock1.Text = value;
            }
            else
            {
                tblock1.Text = cbxExposureField.SelectedItem.ToString();
            }
            tblock1.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            tblock1.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            
            Grid.SetColumn(tblock1, 0);
            Grid.SetRow(tblock1, 1);
            gridOuter.Children.Add(tblock1);

            TextBlock tblock2 = new TextBlock();
            tblock2.Name = "tblockOutcomeGridHeader";
            tblock2.FontWeight = FontWeights.Bold;
            tblock2.FontSize = tblock1.FontSize + 2;
            tblock2.Text = cbxOutcomeField.SelectedItem.ToString();
            tblock2.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            tblock2.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            Grid.SetColumn(tblock2, 1);
            Grid.SetRow(tblock2, 0);
            gridOuter.Children.Add(tblock2);

            sv.Content = gridOuter;     

            TextBlock txtDisclaimerLabel = new TextBlock();
            txtDisclaimerLabel.Text = string.Empty;
            txtDisclaimerLabel.Margin = new Thickness(2, 8, 2, 10);
            txtDisclaimerLabel.HorizontalAlignment = HorizontalAlignment.Center;
            txtDisclaimerLabel.Tag = value;
            
            gridDisclaimerList.Add(txtDisclaimerLabel);

            crosstabPanel.Children.Add(sv);
            crosstabPanel.Children.Add(chiSquaregrid);
            crosstabPanel.Children.Add(txtDisclaimerLabel);

            if (string.IsNullOrEmpty(strataVar) && GadgetOptions.StrataVariableNames.Count == 0)
            {                
                panelMain.Children.Add(crosstabPanel);
            }
            else
            {
                crosstabPanel.Margin = (Thickness)this.Resources["genericElementMargin"];
                expander.Margin = (Thickness)this.Resources["expanderMargin"];
                expander.IsExpanded = true;
                expander.Content = crosstabPanel;
                panelMain.Children.Add(expander);
            }

            strataChiSquareGridList.Add(chiSquaregrid);
            strataGridList.Add(grid);
        }

        void strataGroupPanel_ExpandAllClicked()
        {
            foreach (StackPanel panel in this.groupList)
            {                
                panel.Visibility = System.Windows.Visibility.Visible;                
            }
        }

        void strataGroupPanel_RowClicked(UIElement control, string strataValue)
        {
            foreach (StackPanel panel in this.groupList)
            {
                if (panel.Tag.ToString() == strataValue)
                {
                    panel.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    panel.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        void strataGroupPanel_NoStrata_ExpandAllClicked()
        {
            foreach (GadgetTwoByTwoPanel grid2x2 in this.strata2x2GridList)
            {                
                grid2x2.Visibility = System.Windows.Visibility.Visible;
                FrameworkElement element = grid2x2.Parent as FrameworkElement;
                element.Visibility = System.Windows.Visibility.Visible;
            }
        }

        void strataGroupPanel_NoStrata_RowClicked(UIElement control, string strataValue)
        {
            foreach (GadgetTwoByTwoPanel grid2x2 in this.strata2x2GridList)
            {
                if (grid2x2.ExposureVariable.Equals(strataValue))
                {
                    grid2x2.Visibility = System.Windows.Visibility.Visible;
                    FrameworkElement element = grid2x2.Parent as FrameworkElement;
                    element.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    grid2x2.Visibility = System.Windows.Visibility.Collapsed;
                    FrameworkElement element = grid2x2.Parent as FrameworkElement;
                    element.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        void strataListPanel_ExpandAllClicked()
        {
            foreach(Expander expander in this.strataExpanderList) 
            {
                expander.IsExpanded = true;
                expander.Visibility = System.Windows.Visibility.Visible;
            }
        }

        void strataListPanel_RowClicked(UIElement control, string strataValue)
        {
            foreach (Expander expander in this.strataExpanderList)
            {
                if ((expander.Header as TextBlock).Text == strataValue)
                {
                    expander.IsExpanded = true;
                    expander.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    expander.IsExpanded = false;
                    expander.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        void twoByTwoPanel_ValuesUpdated(UserControl control)
        {
            UpdateStrata2x2Statistics();
        }

        private Grid GetStrataChiSquareGrid(string strataValue)
        {
            Grid grid = new Grid();

            foreach (Grid g in strataChiSquareGridList)
            {
                if (g.Tag.Equals(strataValue))
                {
                    grid = g;
                    break;
                }
            }

            return grid;
        }

        private TextBlock GetStrataChiSquareDisclaimer(string strataValue)
        {
            TextBlock textBlock = new TextBlock();

            foreach (TextBlock t in gridDisclaimerList)
            {
                if (t.Tag.Equals(strataValue))
                {
                    textBlock = t;
                    break;
                }
            }

            return textBlock;
        }

        private void SetGridText(string strataValue, TextBlockConfig textBlockConfig, FontWeight fontWeight)
        {
            Grid grid = new Grid();

            grid = GetStrataGrid(strataValue);

            TextBlock txt = new TextBlock();                        
            txt.Text = textBlockConfig.Text;
            txt.Margin = textBlockConfig.Margin;
            txt.VerticalAlignment = textBlockConfig.VerticalAlignment;
            txt.HorizontalAlignment = textBlockConfig.HorizontalAlignment;
            txt.FontWeight = fontWeight;

            int rowNum = textBlockConfig.RowNumber;
            int colNum = textBlockConfig.ColumnNumber;

            if (colNum == 0 && rowNum > 0)
            {
                Rectangle rctHeader = new Rectangle();
                rctHeader.Style = this.Resources["gridHeaderCellRectangle"] as Style;
                Grid.SetRow(rctHeader, rowNum);
                Grid.SetColumn(rctHeader, 0);
                grid.Children.Add(rctHeader);
                txt.Style = this.Resources["columnHeadingText"] as Style;
            }

            if (colNum > 0)
            {
                StackPanel panel = new StackPanel();
                panel.Margin = new Thickness(2, 4, 2, 4);
                panel.Children.Add(txt);

                Grid.SetRow(panel, rowNum);
                Grid.SetColumn(panel, colNum);
                grid.Children.Add(panel);
            }
            else
            {
                Grid.SetRow(txt, rowNum);
                Grid.SetColumn(txt, colNum);
                grid.Children.Add(txt);
            }
        }

        /// <summary>
        /// Draws the borders around a given output grid
        /// </summary>
        /// <param name="strataValue">
        /// The strata value to which this grid cell belongs; used to search the list of grids and 
        /// return the proper System.Windows.Controls.Grid for text insertion.
        /// </param>
        protected override void DrawOutputGridBorders(string strataValue)
        {
            Grid grid = GetStrataGrid(strataValue);
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            int rdcount = 0;
            foreach (RowDefinition rd in grid.RowDefinitions)
            {
                int cdcount = 0;
                foreach (ColumnDefinition cd in grid.ColumnDefinitions)
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
                    grid.Children.Add(border);
                    cdcount++;
                }
                rdcount++;
            }
        }

        private void RenderFrequencyHeader(string strataValue, string freqVar, DataColumnCollection columns)
        {
            Grid grid = GetStrataGrid(strataValue);

            if (grid.ColumnDefinitions.Count == 0)
            {
                return;
            }

            RowDefinition rowDefHeader = new RowDefinition();
            rowDefHeader.Height = new GridLength(30);
            grid.RowDefinitions.Add(rowDefHeader);

            for (int y = 1; y < grid.ColumnDefinitions.Count; y++)
            {
                Rectangle rctHeader = new Rectangle();
                rctHeader.Style = this.Resources["gridHeaderCellRectangle"] as Style;
                Grid.SetRow(rctHeader, 0);
                Grid.SetColumn(rctHeader, y);
                grid.Children.Add(rctHeader);
            }

            TextBlock txtValHeader = new TextBlock();
            txtValHeader.Text = freqVar;
            txtValHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtValHeader, 0);
            Grid.SetColumn(txtValHeader, 0);
            grid.Children.Add(txtValHeader);

            int maxColumnLength = MaxColumnLength;

            for (int i = 1; i < columns.Count; i++)
            {
                TextBlock txtColHeader = new TextBlock();
                string columnName = columns[i].ColumnName.Trim();

                if (columnName.Length > maxColumnLength)
                {
                    columnName = columnName.Substring(0, maxColumnLength);
                }

                txtColHeader.Text = columnName;
                txtColHeader.Style = this.Resources["columnHeadingText"] as Style;
                Grid.SetRow(txtColHeader, 0);
                Grid.SetColumn(txtColHeader, i);
                grid.Children.Add(txtColHeader);
            }

            TextBlock txtRowTotalHeader = new TextBlock();
            txtRowTotalHeader.Text = SharedStrings.TOTAL;
            txtRowTotalHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtRowTotalHeader, 0);
            Grid.SetColumn(txtRowTotalHeader, grid.ColumnDefinitions.Count - 1);
            grid.Children.Add(txtRowTotalHeader);
        }

        private void RenderChiSquare(double tableChiSq, double tableChiSqDF, double tableChiSqP, string disclaimer, string value)
        {
            TextBlock textBlock = new TextBlock();

            foreach (TextBlock tblock in gridDisclaimerList)
            {
                if (tblock.Tag.Equals(value))
                {
                    textBlock = tblock;
                    break;
                }
            }           

            if (!string.IsNullOrEmpty(disclaimer) && !Double.IsNaN(tableChiSq))
            {
                textBlock.Text = disclaimer;
                textBlock.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                textBlock.Text = string.Empty;
                textBlock.Visibility = System.Windows.Visibility.Collapsed;
            }

            Grid grid = GetStrataChiSquareGrid(value);
            grid.Visibility = System.Windows.Visibility.Visible;

            Thickness margin = new Thickness(4);

            TextBlock txt1 = new TextBlock();
            txt1.Text = "Chi-square";
            txt1.FontWeight = FontWeights.Bold;
            txt1.Margin = margin;
            txt1.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            Grid.SetRow(txt1, 0);
            Grid.SetColumn(txt1, 0);
            grid.Children.Add(txt1);

            TextBlock txt2 = new TextBlock();
            txt2.Text = "df";
            txt2.FontWeight = FontWeights.Bold;
            txt2.Margin = margin;
            txt2.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            Grid.SetRow(txt2, 0);
            Grid.SetColumn(txt2, 1);
            grid.Children.Add(txt2);

            TextBlock txt3 = new TextBlock();
            txt3.Text = "Probability";
            txt3.FontWeight = FontWeights.Bold;
            txt3.Margin = margin;
            txt3.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            Grid.SetRow(txt3, 0);
            Grid.SetColumn(txt3, 2);
            grid.Children.Add(txt3);

            TextBlock txt4 = new TextBlock();
            txt4.Text = tableChiSq.ToString("F4");
            if (txt4.Text.Equals("NaN"))
                txt4.Text = "N/A";
            txt4.Margin = margin;
            txt4.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            Grid.SetRow(txt4, 1);
            Grid.SetColumn(txt4, 0);
            grid.Children.Add(txt4);

            TextBlock txt5 = new TextBlock();
            txt5.Text = tableChiSqDF.ToString();
            if (txt4.Text.Equals("N/A"))
                txt5.Text = "N/A";
            txt5.Margin = margin;
            txt5.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            Grid.SetRow(txt5, 1);
            Grid.SetColumn(txt5, 1);
            grid.Children.Add(txt5);

            TextBlock txt6 = new TextBlock();
            txt6.Text = tableChiSqP.ToString("F4"); //tableChiSqP.ToString();            
            if (txt4.Text.Equals("N/A"))
                txt6.Text = "N/A";
            txt6.Margin = margin;
            txt6.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            Grid.SetRow(txt6, 1);
            Grid.SetColumn(txt6, 2);
            grid.Children.Add(txt6);
        }

        private void RenderFrequencyFooter(string strataValue, int footerRowIndex, int[] totalRows)
        {
            Grid grid = GetStrataGrid(strataValue);

            if (grid.ColumnDefinitions.Count == 0)
            {
                return;
            }

            RowDefinition rowDefTotals = new RowDefinition();
            rowDefTotals.Height = GridLength.Auto; //new GridLength(30);
            grid.RowDefinitions.Add(rowDefTotals);

            Rectangle rctHeader = new Rectangle();
            rctHeader.Style = this.Resources["gridHeaderCellRectangle"] as Style;
            Grid.SetRow(rctHeader, grid.RowDefinitions.Count - 1);
            Grid.SetColumn(rctHeader, 0);
            grid.Children.Add(rctHeader);

            TextBlock txtValTotals = new TextBlock();
            txtValTotals.Text = SharedStrings.TOTAL;
            txtValTotals.Style = this.Resources["columnHeadingText"] as Style;
            txtValTotals.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            Grid.SetRow(txtValTotals, footerRowIndex);
            Grid.SetColumn(txtValTotals, 0);
            grid.Children.Add(txtValTotals);

            for (int i = 0; i < totalRows.Length; i++)
            {
                //if (i >= MaxColumns)
                //{
                //    break;
                //}
                TextBlock txtFreqTotals = new TextBlock();
                txtFreqTotals.Text = totalRows[i].ToString();
                txtFreqTotals.Margin = new Thickness(4, 0, 4, 0);
                txtFreqTotals.VerticalAlignment = VerticalAlignment.Center;
                txtFreqTotals.HorizontalAlignment = HorizontalAlignment.Right;
                txtFreqTotals.FontWeight = FontWeights.Bold;

                StackPanel panel = new StackPanel();
                panel.Margin = new Thickness(2, 4, 2, 4);
                panel.Children.Add(txtFreqTotals);

                Grid.SetRow(panel, footerRowIndex);
                Grid.SetColumn(panel, i + 1);
                grid.Children.Add(panel);
                //Grid.SetRow(txtFreqTotals, footerRowIndex);
                //Grid.SetColumn(txtFreqTotals, i + 1);
                //grid.Children.Add(txtFreqTotals);
            }

            int sumTotal = 0;
            foreach (int n in totalRows)
            {
                sumTotal = sumTotal + n;
            }

            TextBlock txtOverallTotal = new TextBlock();
            txtOverallTotal.Text = sumTotal.ToString();
            txtOverallTotal.Margin = new Thickness(4, 0, 4, 0);
            txtOverallTotal.VerticalAlignment = VerticalAlignment.Center;
            txtOverallTotal.HorizontalAlignment = HorizontalAlignment.Right;
            txtOverallTotal.FontWeight = FontWeights.Bold;

            StackPanel panel2 = new StackPanel();
            panel2.Margin = new Thickness(2, 4, 2, 4);
            panel2.Children.Add(txtOverallTotal);

            Grid.SetRow(panel2, footerRowIndex);
            Grid.SetColumn(panel2, grid.ColumnDefinitions.Count - 1);
            grid.Children.Add(panel2);

            //Grid.SetRow(txtOverallTotal, footerRowIndex);
            //Grid.SetColumn(txtOverallTotal, grid.ColumnDefinitions.Count - 1);
            //grid.Children.Add(txtOverallTotal);            

            //this.Dispatcher.BeginInvoke(new FillPercentsDelegate(FillInRowColumnPercents), grid);
            FillInRowColumnPercents(grid);

            if (grid.Parent is Grid)
            {
                Grid gridOuter = grid.Parent as Grid;
                UIElement element = gridOuter.Children.Cast<UIElement>().First(x => Grid.GetRow(x) == 0 && Grid.GetColumn(x) == 1);                

                if (element != null && element is TextBlock)
                {
                    double width = grid.ColumnDefinitions[0].ActualWidth;                    
                    TextBlock tblockHeader = element as TextBlock;
                    tblockHeader.Margin = new Thickness(width, 0, 0, 0);
                }
            }
        }

        private void FillInRowColumnPercents(Grid grid)
        {
            int cdCount = 0;
            foreach (ColumnDefinition cd in grid.ColumnDefinitions)
            {
                double columnTotal = 0;

                UIElement cElement = grid.Children.Cast<UIElement>().First(x => Grid.GetRow(x) == grid.RowDefinitions.Count - 1 && Grid.GetColumn(x) == cdCount);
                if (cElement is TextBlock)
                {
                    columnTotal = double.Parse((cElement as TextBlock).Text);
                }
                else if (cElement is StackPanel)
                {
                    columnTotal = double.Parse(((cElement as StackPanel).Children[0] as TextBlock).Text);
                }

                int rdCount = 0;
                foreach (RowDefinition rd in grid.RowDefinitions)
                {
                    double rowTotal = 0;
                    double currentCount = 0;

                    UIElement rElement = grid.Children.Cast<UIElement>().First(x => Grid.GetRow(x) == rdCount && Grid.GetColumn(x) == grid.ColumnDefinitions.Count - 1);
                    if (rElement is StackPanel)
                    {
                        StackPanel panel = rElement as StackPanel;
                        TextBlock tblockRowTotal = panel.Children[0] as TextBlock;
                        rowTotal = double.Parse(tblockRowTotal.Text);                        
                    }

                    UIElement element = grid.Children.Cast<UIElement>().First(x => Grid.GetRow(x) == rdCount && Grid.GetColumn(x) == cdCount);
                    if (element is StackPanel)
                    {
                        StackPanel panel = element as StackPanel;
                        TextBlock tblockCurrentCount = panel.Children[0] as TextBlock;
                        currentCount = double.Parse(tblockCurrentCount.Text);

                        TextBlock tblockRowPercent = new TextBlock();
                        tblockRowPercent.Foreground = Brushes.Gray;
                        tblockRowPercent.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                        tblockRowPercent.Text = (currentCount / rowTotal).ToString("P");

                        TextBlock tblockColPercent = new TextBlock();
                        tblockColPercent.Foreground = Brushes.Gray;
                        tblockColPercent.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                        tblockColPercent.Text = (currentCount / columnTotal).ToString("P");

                        if (checkboxRowColPercents.IsChecked == false)
                        {
                            tblockRowPercent.Visibility = System.Windows.Visibility.Collapsed;
                            tblockColPercent.Visibility = System.Windows.Visibility.Collapsed;
                        }

                        panel.Children.Add(tblockRowPercent);
                        panel.Children.Add(tblockColPercent);
                    }

                    rdCount++;
                }
                cdCount++;
            }
        }

        private void RenderFinish()
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed; 

            foreach (Grid freqGrid in strataGridList)
            {
                freqGrid.Visibility = Visibility.Visible;
            }

            foreach (Grid chiSquareGrid in strataChiSquareGridList)
            {
                chiSquareGrid.Visibility = Visibility.Visible;
            }

            messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
            messagePanel.Text = string.Empty;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            
            CheckAndSetPosition();
        }

        private void RenderFinishWithWarning(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            foreach (Grid freqGrid in strataGridList)
            {
                freqGrid.Visibility = Visibility.Visible;
            }

            foreach (Grid chiSquareGrid in strataChiSquareGridList)
            {
                chiSquareGrid.Visibility = Visibility.Visible;
            }

            messagePanel.MessagePanelType = Controls.MessagePanelType.WarningPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        private void RenderFinishWithError(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = Controls.MessagePanelType.ErrorPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        private void CheckVariables()
        {
            //bool outcomeIsNumeric = false;
            //string exposure = string.Empty;
            //string outcome = string.Empty;

            //if (cbxExposureField.SelectedIndex >= 0)
            //{
            //    exposure = cbxExposureField.SelectedItem.ToString();
            //}
            //else
            //{
            //    checkboxAllValues.IsEnabled = false;
            //    checkboxAllValues.IsChecked = false;
            //}


            //if (cbxOutcomeField.SelectedIndex >= 0)
            //{
            //    outcome = cbxOutcomeField.SelectedItem.ToString();
            //}

            //Field exposureField = dashboardHelper.GetAssociatedField(exposure);
            //Field outcomeField = dashboardHelper.GetAssociatedField(outcome);

            ////if ((exposureField != null && exposureField is DDLFieldOfCommentLegal) || (outcomeField != null && outcomeField is DDLFieldOfCommentLegal))
            ////{
            ////    checkboxCommentLegalLabels.IsEnabled = true;
            ////}
            ////else
            ////{
            ////    checkboxCommentLegalLabels.IsEnabled = false;
            ////}

            //if (!string.IsNullOrEmpty(exposure))
            //{
            //    bool isRecoded = false;
            //    if (dashboardHelper.IsUserDefinedColumn(exposure))
            //    {
            //        List<IDashboardRule> associatedRules = dashboardHelper.Rules.GetRules(exposure);
            //        foreach (IDashboardRule rule in associatedRules)
            //        {
            //            if (rule is Rule_Recode)
            //            {
            //                isRecoded = true;
            //            }
            //        }
            //    }

            //    if ((exposureField != null && exposureField is TableBasedDropDownField) || isRecoded)
            //    {
            //        checkboxAllValues.IsEnabled = true;
            //    }
            //    else
            //    {
            //        checkboxAllValues.IsEnabled = false;
            //    }
            //}

            //if (cbxOutcomeField.SelectedItem != null && dashboardHelper.IsColumnNumeric(cbxOutcomeField.SelectedItem.ToString()))
            //{
            //    outcomeIsNumeric = true;
            //}

            //if (outcomeIsNumeric)
            //{
            //    checkboxOutcomeContinuous.IsEnabled = true;
            //}
            //else
            //{
            //    checkboxOutcomeContinuous.IsEnabled = false;
            //    checkboxOutcomeContinuous.IsChecked = false;
            //}
            lbxFieldStrata.IsEnabled = true;
            if (cbxExposureField.SelectedIndex >= 0)
            {
                if (dashboardHelper.GetAllGroupsAsList().Contains(cbxExposureField.SelectedItem.ToString()))
                {
                    lbxFieldStrata.IsEnabled = false;
                    lbxFieldStrata.SelectedItems.Clear();
                }
            }
        }

        private void CreateInputVariableList()
        {
            Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

            GadgetOptions.MainVariableName = string.Empty;
            GadgetOptions.WeightVariableName = string.Empty;
            GadgetOptions.StrataVariableNames = new List<string>();
            GadgetOptions.CrosstabVariableName = string.Empty;

            smartTable = (bool)checkboxSmartTable.IsChecked;
            showPercents = (bool)checkboxRowColPercents.IsChecked;

            if (cbxExposureField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxExposureField.SelectedItem.ToString()))
            {
                inputVariableList.Add("freqvar", cbxExposureField.SelectedItem.ToString());
                GadgetOptions.MainVariableName = cbxExposureField.SelectedItem.ToString();
            }
            else
            {
                return;
            }

            if (cbxOutcomeField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxOutcomeField.SelectedItem.ToString()))
            {
                inputVariableList.Add("crosstabvar", cbxOutcomeField.SelectedItem.ToString());
                GadgetOptions.CrosstabVariableName = cbxOutcomeField.SelectedItem.ToString();
            }

            if (cbxFieldWeight.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldWeight.SelectedItem.ToString()))
            {
                inputVariableList.Add("weightvar", cbxFieldWeight.SelectedItem.ToString());
                GadgetOptions.WeightVariableName = cbxFieldWeight.SelectedItem.ToString();
            }
            //if (cbxFieldStrata.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldStrata.SelectedItem.ToString()))
            //{
            //    inputVariableList.Add("stratavar", cbxFieldStrata.SelectedItem.ToString());
            //    GadgetOptions.StrataVariableNames = new List<string>();
            //    GadgetOptions.StrataVariableNames.Add(cbxFieldStrata.SelectedItem.ToString());
            //}

            if (lbxFieldStrata.SelectedItems.Count > 0)
            {
                //inputVariableList.Add("stratavar", cbxFieldStrata.SelectedItem.ToString());
                GadgetOptions.StrataVariableNames = new List<string>();
                foreach (string s in lbxFieldStrata.SelectedItems)
                {
                    GadgetOptions.StrataVariableNames.Add(s);
                }
            }

            if (checkboxAllValues.IsChecked == true)
            {
                inputVariableList.Add("allvalues", "true");
                GadgetOptions.ShouldUseAllPossibleValues = true;
            }
            else
            {
                inputVariableList.Add("allvalues", "false");
                GadgetOptions.ShouldUseAllPossibleValues = false;
            }

            if (checkboxCommentLegalLabels.IsChecked == true)
            {
                GadgetOptions.ShouldShowCommentLegalLabels = true;
            }
            else
            {
                GadgetOptions.ShouldShowCommentLegalLabels = false;
            }

            if (checkboxIncludeMissing.IsChecked == true)
            {
                inputVariableList.Add("includemissing", "true");
                GadgetOptions.ShouldIncludeMissing = true;
            }
            else
            {
                inputVariableList.Add("includemissing", "false");
                GadgetOptions.ShouldIncludeMissing = false;
            }

            if (checkboxOutcomeContinuous.IsChecked == true)
            {
                inputVariableList.Add("treatoutcomeascontinuous", "true");                
            }
            else
            {
                inputVariableList.Add("treatoutcomeascontinuous", "false");                
            }

            if (checkboxStrataSummaryOnly.IsChecked == true)
            {
                inputVariableList.Add("stratasummaryonly", "true");
            }
            else
            {
                inputVariableList.Add("stratasummaryonly", "false");
            }

            //inputVariableList.Add("maxcolumns", MaxColumns.ToString());
            //inputVariableList.Add("maxrows", MaxRows.ToString());

            GadgetOptions.ShouldIncludeFullSummaryStatistics = false;
            GadgetOptions.InputVariableList = inputVariableList;
        }
        #endregion

        #region Private Properties
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
        #endregion // Private Properties

        #region IGadget Members
        /// <summary>
        /// Updates the variable names available in the gadget's properties
        /// </summary>
        public override void UpdateVariableNames()
        {
            FillComboboxes(true);
        }

        public override void RefreshResults()
        {
            isRunningGrouped2x2 = null;

            if (!LoadingCombos && GadgetOptions != null && cbxExposureField.SelectedIndex > -1 && cbxOutcomeField.SelectedIndex > -1)
            {
                CreateInputVariableList();
                infoPanel.Visibility = System.Windows.Visibility.Collapsed;
                waitPanel.Visibility = System.Windows.Visibility.Visible;
                messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
                descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
                baseWorker = new BackgroundWorker();
                baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
                baseWorker.RunWorkerAsync();
                base.RefreshResults();
            }
            else if (!LoadingCombos && (cbxExposureField.SelectedIndex == -1 && cbxOutcomeField.SelectedIndex == -1))
            {
                ClearResults();
                waitPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Generates Xml representation of this gadget
        /// </summary>
        /// <param name="doc">The Xml docment</param>
        /// <returns>XmlNode</returns>
        public override XmlNode Serialize(XmlDocument doc)
        {
            Dictionary<string, string> inputVariableList = GadgetOptions.InputVariableList;

            string freqVar = string.Empty;
            string crosstabVar = string.Empty;
            string strataVar = string.Empty;
            string weightVar = string.Empty;
            string sort = string.Empty;
            bool allValues = false;
            bool showConfLimits = true;
            bool showCumulativePercent = true;
            bool includeMissing = false;
            bool outcomeContinuous = false;
            bool rowColPercents = (bool)checkboxRowColPercents.IsChecked;
            string layoutMode = "vertical";
            bool showStrataSummaryOnly = false;

            if (checkboxHorizontal.IsChecked == true)
            {
                layoutMode = "horizontal";
            }

            if (inputVariableList.ContainsKey("freqvar"))
            {
                freqVar = inputVariableList["freqvar"].Replace("<", "&lt;");
            }
            if (inputVariableList.ContainsKey("stratavar"))
            {
                strataVar = inputVariableList["stratavar"].Replace("<", "&lt;");
            }
            if (inputVariableList.ContainsKey("weightvar"))
            {
                weightVar = inputVariableList["weightvar"].Replace("<", "&lt;");
            }
            if (inputVariableList.ContainsKey("crosstabvar"))
            {
                crosstabVar = inputVariableList["crosstabvar"].Replace("<", "&lt;");
            }
            if (inputVariableList.ContainsKey("sort"))
            {
                sort = inputVariableList["sort"];
            }
            if (inputVariableList.ContainsKey("allvalues"))
            {
                allValues = bool.Parse(inputVariableList["allvalues"]);
            }
            if (inputVariableList.ContainsKey("showconflimits"))
            {
                showConfLimits = bool.Parse(inputVariableList["showconflimits"]);
            }
            if (inputVariableList.ContainsKey("showcumulativepercent"))
            {
                showCumulativePercent = bool.Parse(inputVariableList["showcumulativepercent"]);
            }
            if (inputVariableList.ContainsKey("includemissing"))
            {
                includeMissing = bool.Parse(inputVariableList["includemissing"]);
            }
            if (inputVariableList.ContainsKey("treatoutcomeascontinuous"))
            {
                outcomeContinuous = bool.Parse(inputVariableList["treatoutcomeascontinuous"]);
            }
            if (inputVariableList.ContainsKey("stratasummaryonly"))
            {
                showStrataSummaryOnly = bool.Parse(inputVariableList["stratasummaryonly"]);                
            }   

            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            string xmlString =
            "<mainVariable>" + freqVar + "</mainVariable>" +
            "<crosstabVariable>" + crosstabVar + "</crosstabVariable>";
            
            if(GadgetOptions.StrataVariableNames.Count == 1) 
            {
                xmlString = xmlString + "<strataVariable>" + GadgetOptions.StrataVariableNames[0].Replace("<", "&lt;") + "</strataVariable>";
            }
            else if (GadgetOptions.StrataVariableNames.Count > 1)
            {
                xmlString = xmlString + "<strataVariables>";

                foreach (string strataVariable in this.GadgetOptions.StrataVariableNames)
                {
                    xmlString = xmlString + "<strataVariable>" + strataVariable.Replace("<", "&lt;") + "</strataVariable>";
                }

                xmlString = xmlString + "</strataVariables>";
            }
            
            xmlString = xmlString + "<weightVariable>" + weightVar + "</weightVariable>" +
            //"<maxColumns>" + MaxColumns.ToString() + "</maxColumns>" +
            "<maxColumnNameLength>" + MaxColumnLength.ToString() + "</maxColumnNameLength>" +
            "<allValues>" + allValues + "</allValues>" +
            "<showListLabels>" + checkboxCommentLegalLabels.IsChecked + "</showListLabels>" +
            "<showConfLimits>" + showConfLimits + "</showConfLimits>" +
            "<showCumulativePercent>" + showCumulativePercent + "</showCumulativePercent>" + 
            "<includeMissing>" + includeMissing + "</includeMissing>" +
            "<showStrataSummaryOnly>" + showStrataSummaryOnly + "</showStrataSummaryOnly>" +
            "<treatOutcomeAsContinuous>" + outcomeContinuous + "</treatOutcomeAsContinuous>" +
            "<customHeading>" + CustomOutputHeading.Replace("<", "&lt;") + "</customHeading>" +
            "<customDescription>" + CustomOutputDescription.Replace("<", "&lt;") + "</customDescription>" +
            "<customCaption>" + CustomOutputCaption + "</customCaption>" +
            "<layoutMode>" + layoutMode + "</layoutMode>" +
            "<rowColPercents>" + rowColPercents + "</rowColPercents>";

            System.Xml.XmlElement element = doc.CreateElement("frequencyGadget");
            element.InnerXml = xmlString;
            element.AppendChild(SerializeFilters(doc));

            System.Xml.XmlAttribute locationY = doc.CreateAttribute("top");
            System.Xml.XmlAttribute locationX = doc.CreateAttribute("left");
            System.Xml.XmlAttribute collapsed = doc.CreateAttribute("collapsed");
            System.Xml.XmlAttribute type = doc.CreateAttribute("gadgetType");

            locationY.Value = Canvas.GetTop(this).ToString("F0");
            locationX.Value = Canvas.GetLeft(this).ToString("F0");
            collapsed.Value = "false"; // currently no way to collapse the gadget, so leave this 'false' for now
            type.Value = "Epi.WPF.Dashboard.Gadgets.Analysis.CrosstabControl";

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);

            return element;
        }

        /// <summary>
        /// Creates the frequency gadget from an Xml element
        /// </summary>
        /// <param name="element">The element from which to create the gadget</param>
        public override void CreateFromXml(XmlElement element)
        {
            this.LoadingCombos = true;
            this.ColumnWarningShown = true;

            HideConfigPanel();

            infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLower())
                {
                    case "mainvariable":
                    case "exposurevariable": // added to work with the old 2x2 gadget
                        cbxExposureField.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "crosstabvariable":
                    case "outcomevariable": // added to work with the old 2x2 gadget
                        cbxOutcomeField.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "stratavariable":
                        //cbxFieldStrata.Text = child.InnerText.Replace("&lt;", "<");
                        if (!string.IsNullOrEmpty(child.InnerText))
                        {
                            lbxFieldStrata.SelectedItems.Add(child.InnerText.Replace("&lt;", "<"));
                        }
                        break;                    
                    case "weightvariable":
                        cbxFieldWeight.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "maxcolumnnamelength":
                        int maxColumnLength = 24;
                        int.TryParse(child.InnerText, out maxColumnLength);
                        txtMaxColumnLength.Text = maxColumnLength.ToString();
                        break;
                    case "allvalues":
                        if (child.InnerText.ToLower().Equals("true"))
                        {
                            checkboxAllValues.IsChecked = true;
                        }
                        else
                        {
                            checkboxAllValues.IsChecked = false;
                        }
                        break;
                    case "showlistlabels":
                        if (child.InnerText.ToLower().Equals("true"))
                        {
                            checkboxCommentLegalLabels.IsChecked = true;
                        }
                        else
                        {
                            checkboxCommentLegalLabels.IsChecked = false;
                        }
                        break;
                    case "includemissing":
                        if (child.InnerText.ToLower().Equals("true"))
                        {
                            checkboxIncludeMissing.IsChecked = true;
                        }
                        else
                        {
                            checkboxIncludeMissing.IsChecked = false;
                        }       
                        break;
                    case "showstratasummaryonly":                        
                        if (child.InnerText.ToLower().Equals("true"))
                        {
                            checkboxStrataSummaryOnly.IsChecked = true;
                        }
                        else
                        {
                            checkboxStrataSummaryOnly.IsChecked = false;
                        }
                        break;
                    case "treatoutcomeascontinuous":
                        if (child.InnerText.ToLower().Equals("true"))
                        {
                            checkboxOutcomeContinuous.IsChecked = true;
                        }
                        else
                        {
                            checkboxOutcomeContinuous.IsChecked = false;
                        }
                        break;
                    case "rowcolpercents":
                        if (child.InnerText.ToLower().Equals("true"))
                        {
                            checkboxRowColPercents.IsChecked = true;
                        }
                        else
                        {
                            checkboxRowColPercents.IsChecked = false;
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
                    case "customcaption":
                        this.CustomOutputCaption = child.InnerText;
                        break;
                    case "datafilters":
                        this.dataFilters = new DataFilters(this.dashboardHelper);
                        this.DataFilters.CreateFromXml(child);
                        break;
                    case "stratavariables":
                        foreach (XmlElement field in child.ChildNodes)
                        {
                            List<string> fields = new List<string>();
                            if (field.Name.ToLower().Equals("stratavariable"))
                            {
                                lbxFieldStrata.SelectedItems.Add(field.InnerText.Replace("&lt;", "<"));
                            }
                        }
                        break;
                    case "layoutmode":
                        if (child.InnerText.ToLower().Equals("horizontal"))
                        {
                            checkboxHorizontal.IsChecked = true;
                            panelMain.Orientation = Orientation.Horizontal;
                        }
                        else
                        {
                            checkboxHorizontal.IsChecked = false;
                            panelMain.Orientation = Orientation.Vertical;
                        }                        
                        break;
                }
            }

            SetPositionFromXml(element);

            this.LoadingCombos = false;

            CheckVariables();

            RefreshResults();
            HideConfigPanel();
        }

        /// <summary>
        /// Checks to see whether a valid numeric digit was pressed for numeric-only conditions
        /// </summary>
        /// <param name="keyChar">The key that was pressed</param>
        /// <returns>Whether the input was a valid number character</returns>
        private bool ValidNumberChar(string keyChar)
        {
            System.Globalization.NumberFormatInfo numberFormatInfo = System.Globalization.CultureInfo.CurrentCulture.NumberFormat;

            for (int i = 0; i < keyChar.Length; i++)
            {
                char ch = keyChar[i];
                if (!Char.IsDigit(ch))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        private void txtMaxColumns_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !ValidNumberChar(e.Text);

            base.OnPreviewTextInput(e);
        }        

        private int MaxColumnLength
        {
            get
            {
                int maxColumnLength = 24;
                bool success = int.TryParse(txtMaxColumnLength.Text, out maxColumnLength);
                if (!success)
                {
                    return 24;
                }
                else
                {
                    return maxColumnLength;
                }
            }
        }

        private bool ColumnWarningShown
        {
            get
            {
                return this.columnWarningShown;
            }
            set
            {
                this.columnWarningShown = value;
            }
        }
        
        /// <summary>
        /// Sets the gadget to its 'processing' state
        /// </summary>
        public override void SetGadgetToProcessingState()
        {
            this.IsProcessing = true;
            this.cbxExposureField.IsEnabled = false;
            this.cbxOutcomeField.IsEnabled = false;
            //this.cbxFieldStrata.IsEnabled = false;
            this.lbxFieldStrata.IsEnabled = false;
            this.cbxFieldWeight.IsEnabled = false;
            this.txtMaxColumnLength.IsEnabled = false;
            //this.txtMaxColumns.IsEnabled = false;
            //this.txtMaxRows.IsEnabled = false;
            this.checkboxAllValues.IsEnabled = false;
            this.checkboxIncludeMissing.IsEnabled = false;
            this.checkboxCommentLegalLabels.IsEnabled = false;
            this.checkboxOutcomeContinuous.IsEnabled = false;
            this.checkboxRowColPercents.IsEnabled = false;
            this.checkboxSmartTable.IsEnabled = false;
            this.checkboxHorizontal.IsEnabled = false;
            this.checkboxStrataSummaryOnly.IsEnabled = false;
            this.btnRun.IsEnabled = false;
        }

        /// <summary>
        /// Sets the gadget to its 'finished' state
        /// </summary>
        public override void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
            this.cbxExposureField.IsEnabled = true;
            this.cbxOutcomeField.IsEnabled = true;
            //this.cbxFieldStrata.IsEnabled = true;
            this.lbxFieldStrata.IsEnabled = true;
            this.cbxFieldWeight.IsEnabled = true;
            this.txtMaxColumnLength.IsEnabled = true;
            //this.txtMaxColumns.IsEnabled = true;
            //this.txtMaxRows.IsEnabled = true;
            this.checkboxIncludeMissing.IsEnabled = true;
            this.checkboxRowColPercents.IsEnabled = true;
            this.checkboxSmartTable.IsEnabled = true;
            this.checkboxHorizontal.IsEnabled = true;
            this.checkboxStrataSummaryOnly.IsEnabled = true;
            this.btnRun.IsEnabled = true;

            //if (exposureIsDropDownList)
            //{
                this.checkboxAllValues.IsEnabled = true;
            //}

            //if (exposureIsCommentLegal)
            //{
                this.checkboxCommentLegalLabels.IsEnabled = true;
            //}

            //if (outcomeIsNumeric)
            //{
                this.checkboxOutcomeContinuous.IsEnabled = true;
            //}

                CheckVariables();

            base.SetGadgetToFinishedState();
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0)
        {
            StringBuilder htmlBuilder = new StringBuilder();

            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            if (CustomOutputHeading == null || (string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)")))
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">Crosstabulation</h2>");
            }
            else if (CustomOutputHeading != "(none)")
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">" + CustomOutputHeading + "</h2>");
            }

            htmlBuilder.AppendLine("<p class=\"gadgetOptions\"><small>");
            htmlBuilder.AppendLine("<em>Main variable:</em> <strong>" + cbxExposureField.Text + "</strong>");
            htmlBuilder.AppendLine("<br />");

            htmlBuilder.AppendLine("<em>Crosstab variable:</em> <strong>" + cbxOutcomeField.Text + "</strong>");
            htmlBuilder.AppendLine("<br />");

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
                htmlBuilder.AppendLine("<em>Strata variable:</em> <strong>" + wb.ToString() + "</strong>");
                htmlBuilder.AppendLine("<br />");
            }

            htmlBuilder.AppendLine("<em>Include missing:</em> <strong>" + checkboxIncludeMissing.IsChecked.ToString() + "</strong>");
            htmlBuilder.AppendLine("<br />");
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

            foreach (Grid grid in this.strataGridList)
            {
                string gridName = grid.Tag.ToString();

                htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
                htmlBuilder.AppendLine("<caption>" + cbxExposureField.Text + " * " + cbxOutcomeField.Text + "</caption>");

                foreach (UIElement control in grid.Children)
                {
                    string value = string.Empty;
                    int rowNumber = -1;
                    int columnNumber = -1;
                    if (control is TextBlock || control is StackPanel)
                    {
                        if (control is TextBlock)
                        {
                            rowNumber = Grid.GetRow(control);
                            columnNumber = Grid.GetColumn(control);
                            value = ((TextBlock)control).Text;
                        }
                        else if (control is StackPanel)
                        {
                            rowNumber = Grid.GetRow(control);
                            columnNumber = Grid.GetColumn(control);
                            value = (((control as StackPanel).Children[0]) as TextBlock).Text;
                        }

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

                        string formattedValue = value;

                        if ((rowNumber == grid.RowDefinitions.Count - 1) || (columnNumber == grid.ColumnDefinitions.Count - 1))
                        {
                            formattedValue = "<span class=\"total\">" + value + "</span>";
                        }

                        htmlBuilder.AppendLine(tableDataTagOpen + formattedValue + tableDataTagClose);

                        if (columnNumber >= grid.ColumnDefinitions.Count - 1)
                        {
                            htmlBuilder.AppendLine("</tr>");
                        }
                    }
                }

                htmlBuilder.AppendLine("</table>");

                // Chi Square

                Grid chiSquareGrid = GetStrataChiSquareGrid(grid.Tag.ToString());
                htmlBuilder.AppendLine("<p></p>");
                htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");

                foreach (UIElement control in chiSquareGrid.Children)
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

                        string value = ((TextBlock)control).Text;
                        string formattedValue = value;

                        htmlBuilder.AppendLine(tableDataTagOpen + formattedValue + tableDataTagClose);

                        if (columnNumber >= grid.ColumnDefinitions.Count - 1)
                        {
                            htmlBuilder.AppendLine("</tr>");
                        }
                    }
                }

                htmlBuilder.AppendLine("</table>");

                string disclaimer = GetStrataChiSquareDisclaimer(grid.Tag.ToString()).Text;
                if (!string.IsNullOrEmpty(disclaimer))
                {
                    htmlBuilder.AppendLine("<p></p>");
                    htmlBuilder.AppendLine("<p>" + disclaimer.Replace("<", "&lt;") + "</p>");
                }

                // End Chi Square
            }

            foreach (GadgetTwoByTwoPanel grid2x2 in this.strata2x2GridList)
            {
                //string gridName = grid.Tag.ToString();
                string gridName = string.Empty;
                if (grid2x2.Tag != null)
                {
                    gridName = grid2x2.Tag.ToString();
                }
                if (!string.IsNullOrEmpty(gridName))
                {
                    htmlBuilder.AppendLine("<h3>" + gridName + "</h3>");
                }
                htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                htmlBuilder.AppendLine(grid2x2.ToHTML());
                htmlBuilder.AppendLine("<p></p>");
            }

            foreach (UIElement element in panelMain.Children)
            {
                if (element is StratifiedTableAnalysisPanel)
                {
                    StratifiedTableAnalysisPanel stap = element as StratifiedTableAnalysisPanel;
                    htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                    htmlBuilder.AppendLine(stap.ToHTML());
                    htmlBuilder.AppendLine("<p></p>");
                }
            }

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

        private void checkboxIncludeMissing_Checked(object sender, RoutedEventArgs e)
        {
            checkboxOutcomeContinuous.IsChecked = false;
        }

        private void checkboxOutcomeContinuous_Checked(object sender, RoutedEventArgs e)
        {
            checkboxIncludeMissing.IsChecked = false;
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            if (LoadingCombos)
            {
                return;
            }

            CheckVariables();
            RefreshResults();
        }
    }
}
