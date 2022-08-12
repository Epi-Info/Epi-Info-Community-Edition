using System;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Web.UI;
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
using EpiDashboard.Controls;
using System.Net;

namespace EpiDashboard
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
       // private bool showPercents = true;
        private bool? isRunningGrouped2x2 = null;

        private List<string> YesValues;
        private List<string> NoValues;

        #endregion

        #region Delegates

        private delegate void AddFreqGridDelegate(string strataVar, string value, string crosstabVar, string groupReference, DataTable table);
        private delegate void RenderFrequencyHeaderDelegate(string strataValue, string freqVar, DataColumnCollection columns);
        //private delegate void AddGridRowDelegate(string strataValue, int height);
        private delegate void FillPercentsDelegate(Grid grid);
        private delegate void AddChiSquareDelegate(double tableChiSq, double tableChiSqDf, double tableChiSqP, double tableFisherP, string disclaimer, string value);
        private delegate void AddFisherDelegate(DataRow[] SortedRows, double tableFisherP, string value);
        //private delegate void DrawFrequencyBordersDelegate(string strataValue);  
        protected new delegate void SetGridTextDelegate(string strataValue, TextBlockConfig textBlockConfig, FontWeight fontWeight, double total);

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
            this.DashboardHelper = dashboardHelper;
            Construct();
            //FillComboboxes();
        }

        protected override void Construct()
        {
            this.Parameters = new CrosstabParameters();
            if (!string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)"))
            {
                headerPanel.Text = CustomOutputHeading;
            }

            YesValues = new List<string>();
            NoValues = new List<string>();

            //smartTable = (bool)checkboxSmartTable.IsChecked;
            //showPercents = (bool)checkboxRowColPercents.IsChecked;

            inputVariableList = new Dictionary<string, string>();
            StrataGridList = new List<Grid>();
            strataChiSquareGridList = new List<Grid>();
            strata2x2GridList = new List<Controls.GadgetTwoByTwoPanel>();
            groupList = new List<StackPanel>();
            gridDisclaimerList = new List<TextBlock>();
            strataListPanels = new List<GadgetStrataListPanel>();

            //cbxExposureField.SelectionChanged += new SelectionChangedEventHandler(cbxField_SelectionChanged);
            //cbxOutcomeField.SelectionChanged += new SelectionChangedEventHandler(cbxField_SelectionChanged);
            //cbxFieldWeight.SelectionChanged += new SelectionChangedEventHandler(cbxField_SelectionChanged);
            ////cbxFieldStrata.SelectionChanged += new SelectionChangedEventHandler(cbxField_SelectionChanged);            
            //lbxFieldStrata.SelectionChanged += new SelectionChangedEventHandler(cbxField_SelectionChanged);

            //rctHighColor.MouseUp += new MouseButtonEventHandler(rctHighColor_MouseUp);
            //rctLowColor.MouseUp += new MouseButtonEventHandler(rctLowColor_MouseUp);
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

            //checkboxAllValues.Checked += new RoutedEventHandler(checkboxCheckChanged);
            //checkboxAllValues.Unchecked += new RoutedEventHandler(checkboxCheckChanged);

            //checkboxIncludeMissing.Checked += new RoutedEventHandler(checkboxCheckChanged);
            //checkboxIncludeMissing.Unchecked += new RoutedEventHandler(checkboxCheckChanged);

            //checkboxOutcomeContinuous.Checked += new RoutedEventHandler(checkboxCheckChanged);
            //checkboxOutcomeContinuous.Unchecked += new RoutedEventHandler(checkboxCheckChanged);

            //checkboxSmartTable.Checked += new RoutedEventHandler(checkboxCheckChanged);
            //checkboxSmartTable.Unchecked += new RoutedEventHandler(checkboxCheckChanged);

            //checkboxRowColPercents.Checked += new RoutedEventHandler(checkboxRowColPercents_Checked);
            //checkboxRowColPercents.Unchecked += new RoutedEventHandler(checkboxRowColPercents_Checked);

            //checkboxHorizontal.Checked += new RoutedEventHandler(checkboxHorizontal_Checked);
            //checkboxHorizontal.Unchecked += new RoutedEventHandler(checkboxHorizontal_Unchecked);

            //checkboxStrataSummaryOnly.Checked += new RoutedEventHandler(checkboxStrataSummaryOnly_CheckChanged);
            //checkboxStrataSummaryOnly.Unchecked += new RoutedEventHandler(checkboxStrataSummaryOnly_CheckChanged);

            columnWarningShown = false;

            this.IsProcessing = false;

            this.GadgetStatusUpdate += new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(IsCancelled);

            base.Construct();

            requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
            checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

            #region Translation
            //ConfigExpandedTitle.Text = DashboardSharedStrings.GADGET_CONFIG_TITLE_MXN;
            //tblockExposureField.Text = DashboardSharedStrings.GADGET_EXPOSURE_VARIABLE;
            //tblockOutcomeField.Text = DashboardSharedStrings.GADGET_OUTCOME_VARIABLE;
            //tblockWeightVariable.Text = DashboardSharedStrings.GADGET_WEIGHT_VARIABLE;
            //tblockStratifyBy.Text = DashboardSharedStrings.GADGET_STRATA_VARIABLE;

            //expanderAdvancedOptions.Header = DashboardSharedStrings.GADGET_ADVANCED_OPTIONS;
            //expanderDisplayOptions.Header = DashboardSharedStrings.GADGET_DISPLAY_OPTIONS;
            //checkboxStrataSummaryOnly.Content = DashboardSharedStrings.GADGET_STRATA_SUMMARY_ONLY;
            //checkboxIncludeMissing.Content = DashboardSharedStrings.GADGET_INCLUDE_MISSING;
            //checkboxAllValues.Content = DashboardSharedStrings.GADGET_ALL_LIST_VALUES;
            //checkboxCommentLegalLabels.Content = DashboardSharedStrings.GADGET_LIST_LABELS;
            //checkboxOutcomeContinuous.Content = DashboardSharedStrings.GADGET_OUTCOME_CONTINUOUS;
            //checkboxRowColPercents.Content = DashboardSharedStrings.GADGET_ROW_COL_PERCENTS;
            //checkboxSmartTable.Content = DashboardSharedStrings.GADGET_SMART_2X2;
            //checkboxHorizontal.Content = DashboardSharedStrings.GADGET_DISPLAY_2X2_SUMMARY_HORIZONTALLY;
            //checkboxConditionalShading.Content = DashboardSharedStrings.GADGET_USE_CONDITIONAL_CELL_SHADING;

            //tblockColorGradient.Text = DashboardSharedStrings.GADGET_COLOR_GRADIENT;
            //tblockBreakType.Text = DashboardSharedStrings.GADGET_BREAK_TYPE;

            //tblockBreak1.Text = DashboardSharedStrings.GADGET_BREAK1;
            //tblockBreak2.Text = DashboardSharedStrings.GADGET_BREAK2;
            //tblockBreak3.Text = DashboardSharedStrings.GADGET_BREAK3;
            //tblockBreak4.Text = DashboardSharedStrings.GADGET_BREAK4;
            //tblockBreak5.Text = DashboardSharedStrings.GADGET_BREAK5;

            //tblockMaxColumnLength.Text = DashboardSharedStrings.GADGET_MAX_COL_NAME_LENGTH;

            //btnRun.Content = DashboardSharedStrings.GADGET_RUN_BUTTON;
            //btnValueMappings.Content = DashboardSharedStrings.GADGET_2X2_VALUE_REMAPPING_BUTTON;
            #endregion // Translation
        }

        //void checkboxStrataSummaryOnly_CheckChanged(object sender, RoutedEventArgs e)
        //{
        //}

        //void rctLowColor_MouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
        //    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        rctLowColor.Fill = new SolidColorBrush(Color.FromArgb(0xF0, dialog.Color.R, dialog.Color.G, dialog.Color.B));                
        //    }
        //}

        //void rctHighColor_MouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
        //    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        rctHighColor.Fill = new SolidColorBrush(Color.FromArgb(0xF0, dialog.Color.R, dialog.Color.G, dialog.Color.B));                
        //    }
        //}

        //void checkboxHorizontal_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    //panelMain.Orientation = Orientation.Vertical;
        //    foreach (StackPanel panel in this.groupList)
        //    {
        //        panel.Orientation = Orientation.Vertical;
        //    }

        //    foreach (Controls.GadgetTwoByTwoPanel panel in this.strata2x2GridList)
        //    {
        //        panel.Orientation = Orientation.Vertical;
        //    }
        //    CheckAndSetPosition();
        //}

        //void checkboxHorizontal_Checked(object sender, RoutedEventArgs e)
        //{
        //    //panelMain.Orientation = Orientation.Horizontal;
        //    foreach (StackPanel panel in this.groupList)
        //    {
        //        panel.Orientation = Orientation.Horizontal;
        //    }

        //    foreach (Controls.GadgetTwoByTwoPanel panel in this.strata2x2GridList)
        //    {
        //        panel.Orientation = Orientation.Horizontal;
        //    }
        //    CheckAndSetPosition();
        //}

        //void lbxFieldStrata_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        //void checkboxRowColPercents_Checked(object sender, RoutedEventArgs e)
        //{
        //    //RefreshResults();
        //}

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
            //descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed; //EI-24
            IsCollapsed = true;
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

            if (!string.IsNullOrEmpty(this.descriptionPanel.Text))
            {
                descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.DisplayMode;
            }
            IsCollapsed = false;
        }
        #endregion

        /// <summary>
        /// Copies a grid's output to the clipboard
        /// </summary>
        protected override void CopyToClipboard()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Grid grid in this.StrataGridList)
            {
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

            foreach (Grid grid in StrataGridList)
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
            StrataGridList.Clear();
            strataChiSquareGridList.Clear();
            strata2x2GridList.Clear();
            groupList.Clear();
            StrataExpanderList.Clear();

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

            for (int i = 0; i < StrataGridList.Count; i++)
            {
                StrataGridList[i].Children.Clear();
            }
            this.StrataGridList.Clear();

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
            this.StrataExpanderList.Clear();
            this.panelMain.Children.Clear();

            base.CloseGadget();

            //GadgetOptions = null;
        }

        ///// <summary>
        ///// Handles the check / unchecked events
        ///// </summary>
        ///// <param name="sender">Object that fired the event</param>
        ///// <param name="e">.NET supplied event parameters</param>
        //private void checkboxCheckChanged(object sender, RoutedEventArgs e)
        //{
        //    if(sender == checkboxOutcomeContinuous && checkboxOutcomeContinuous.IsChecked == true) 
        //    {
        //        LoadingCombos = true;
        //        checkboxIncludeMissing.IsChecked = false;
        //        LoadingCombos = false;
        //    }
        //    else if (sender == checkboxIncludeMissing && checkboxIncludeMissing.IsChecked == true)
        //    {
        //        LoadingCombos = true;
        //        checkboxOutcomeContinuous.IsChecked = false;
        //        LoadingCombos = false;
        //    }
        //}

        //void cbxField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    CheckVariables();
        //}
        #endregion

        #region Event Handlers

        private DataTable ApplyColumnValueMappings(DataTable table)
        {
            CrosstabParameters crosstabParameters = (CrosstabParameters)Parameters;
            if (YesValues.Count > 0 && NoValues.Count > 0)
            {
                DataTable remappedTable = new DataTable();
                remappedTable.TableName = table.TableName;

                switch (table.Columns[0].DataType.ToString())
                {
                    case "System.String":
                        remappedTable.Columns.Add(new DataColumn(table.Columns[0].ColumnName, typeof(string)));
                        remappedTable.Columns.Add(new DataColumn(Config.Settings.RepresentationOfYes, typeof(double)));
                        remappedTable.Columns.Add(new DataColumn(Config.Settings.RepresentationOfNo, typeof(double)));
                        remappedTable.Rows.Add(Config.Settings.RepresentationOfYes, 0, 0);
                        remappedTable.Rows.Add(Config.Settings.RepresentationOfNo, 0, 0);
                        break;
                    case "System.Single":
                    case "System.Double":
                    case "System.Byte":
                    case "System.Int16":
                    case "System.Int32":
                    case "System.Int64":
                        remappedTable.Columns.Add(new DataColumn(table.Columns[0].ColumnName, typeof(double)));
                        remappedTable.Columns.Add(new DataColumn(Config.Settings.RepresentationOfYes, typeof(double)));
                        remappedTable.Columns.Add(new DataColumn(Config.Settings.RepresentationOfNo, typeof(double)));
                        remappedTable.Rows.Add(1, 0, 0);
                        remappedTable.Rows.Add(2, 0, 0);
                        break;
                }

                for (int i = 1; i < table.Columns.Count; i++)
                {
                    string columnName = table.Columns[i].ToString();

                    //                    if (YesValues.Contains(columnName) || ((DashboardHelper.IsColumnBoolean(GadgetOptions.CrosstabVariableName) || DashboardHelper.IsColumnYesNo(GadgetOptions.CrosstabVariableName)) && columnName.Equals(Config.Settings.RepresentationOfYes)))
                    if (YesValues.Contains(columnName) || ((DashboardHelper.IsColumnBoolean(crosstabParameters.CrosstabVariableName) || DashboardHelper.IsColumnYesNo(crosstabParameters.CrosstabVariableName)) && columnName.Equals(Config.Settings.RepresentationOfYes)))
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            remappedTable.Rows[j][1] = Convert.ToDouble(remappedTable.Rows[j][1]) + Convert.ToDouble(table.Rows[j][i]);
                        }
                    }

                    //if (NoValues.Contains(columnName) || ((DashboardHelper.IsColumnBoolean(GadgetOptions.CrosstabVariableName) || DashboardHelper.IsColumnYesNo(GadgetOptions.CrosstabVariableName)) && columnName.Equals(Config.Settings.RepresentationOfNo)))
                    if (NoValues.Contains(columnName) || ((DashboardHelper.IsColumnBoolean(crosstabParameters.CrosstabVariableName) || DashboardHelper.IsColumnYesNo(crosstabParameters.CrosstabVariableName)) && columnName.Equals(Config.Settings.RepresentationOfNo)))
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            remappedTable.Rows[j][2] = Convert.ToDouble(remappedTable.Rows[j][2]) + Convert.ToDouble(table.Rows[j][i]);
                        }
                    }
                }

                return remappedTable;
            }

            return table;
        }

        private DataTable ApplyRowValueMappings(DataTable table)
        {
            CrosstabParameters crosstabParameters = (CrosstabParameters)Parameters;

            if (YesValues.Count > 0 && NoValues.Count > 0)
            {
                DataTable remappedTable = table.Clone();

                switch (remappedTable.Columns[0].DataType.ToString())
                {
                    case "System.String":
                        remappedTable.Rows.Add(Config.Settings.RepresentationOfYes);
                        remappedTable.Rows.Add(Config.Settings.RepresentationOfNo);
                        break;
                    case "System.Single":
                    case "System.Double":
                    case "System.Byte":
                    case "System.Int16":
                    case "System.Int32":
                    case "System.Int64":
                        remappedTable.Rows.Add(1);
                        remappedTable.Rows.Add(2);
                        break;
                }

                for (int i = 1; i < remappedTable.Columns.Count; i++)
                {
                    remappedTable.Rows[0][i] = 0;
                    remappedTable.Rows[1][i] = 0;
                }

                foreach (DataRow row in table.Rows)
                {
                    string value = row[0].ToString();

                    //if (YesValues.Contains(value) || ((DashboardHelper.IsColumnBoolean(GadgetOptions.MainVariableName) || DashboardHelper.IsColumnYesNo(GadgetOptions.MainVariableName)) && value.Equals(Config.Settings.RepresentationOfYes)))
                    if (YesValues.Contains(value) || ((DashboardHelper.IsColumnBoolean(crosstabParameters.ColumnNames[0]) || DashboardHelper.IsColumnYesNo(crosstabParameters.ColumnNames[0])) && value.Equals(Config.Settings.RepresentationOfYes)))
                    {
                        for (int i = 1; i < remappedTable.Columns.Count; i++)
                        {
                            remappedTable.Rows[0][i] = Convert.ToDouble(remappedTable.Rows[0][i]) + Convert.ToDouble(row[i]);
                        }
                    }

                    //if (NoValues.Contains(value) || ( (DashboardHelper.IsColumnBoolean(GadgetOptions.MainVariableName) || DashboardHelper.IsColumnYesNo(GadgetOptions.MainVariableName)) && value.Equals(Config.Settings.RepresentationOfNo)))
                    if (NoValues.Contains(value) || ((DashboardHelper.IsColumnBoolean(crosstabParameters.ColumnNames[0]) || DashboardHelper.IsColumnYesNo(crosstabParameters.ColumnNames[0])) && value.Equals(Config.Settings.RepresentationOfNo)))
                    {
                        for (int i = 1; i < remappedTable.Columns.Count; i++)
                        {
                            remappedTable.Rows[1][i] = Convert.ToDouble(remappedTable.Rows[1][i]) + Convert.ToDouble(row[i]);
                        }
                    }
                }
                return remappedTable;
            }
            else
            {
                return table;
            }
        }

        private void CreateSmartTable(DataTable table)
        {
            if (smartTable && table.Columns.Count == 3 && table.Rows.Count == 2)
            {
                //Dictionary<string, string> booleanValues = new Dictionary<string, string>();
                //booleanValues.Add("0", "1");
                //booleanValues.Add("false", "true");
                //booleanValues.Add("f", "t");
                //booleanValues.Add("n", "y");
                CrosstabParameters crosstabParameters = (CrosstabParameters)Parameters;
                // if (!booleanValues.ContainsKey(DashboardHelper.Config.Settings.RepresentationOfNo.ToLowerInvariant()))
                // {
                //  booleanValues.Add(DashboardHelper.Config.Settings.RepresentationOfNo.ToLowerInvariant(), DashboardHelper.Config.Settings.RepresentationOfYes.ToLowerInvariant());
                //}

                string firstColumnName = table.Columns[1].ColumnName.ToLowerInvariant();
                string secondColumnName = table.Columns[2].ColumnName.ToLowerInvariant();

                //  if (booleanValues.ContainsKey(firstColumnName) && secondColumnName.Equals(booleanValues[firstColumnName]))
                if (crosstabParameters.OutcomeSwap)
                {
                    Swap2x2ColValues(table);
                }

                string firstRowName = table.Rows[0][0].ToString();
                string secondRowName = table.Rows[1][0].ToString();

                // if (booleanValues.ContainsKey(firstRowName) && secondRowName.Equals(booleanValues[firstRowName]))
                if (crosstabParameters.ExposureSwap)
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
            CrosstabParameters crosstabParameters = (CrosstabParameters)Parameters;

            //if (this.StrataGridList.Count == 0 && this.strata2x2GridList.Count >= 2 && this.GadgetOptions.StrataVariableNames.Count >= 1 && !DashboardHelper.GetAllGroupsAsList().Contains(GadgetOptions.MainVariableName))
            if (this.StrataGridList.Count == 0 && this.strata2x2GridList.Count >= 2 && crosstabParameters.StrataVariableNames.Count >= 1 && !DashboardHelper.GetAllGroupsAsList().Contains(crosstabParameters.ColumnNames[0]))
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
            foreach (UIElement element in panelMain.Children)
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
                //Dictionary<string, string> inputVariableList = ((GadgetParameters)e.Argument).InputVariableList;

                //Stopwatch stopwatch = new Stopwatch();
                //stopwatch.Start();

                CrosstabParameters crosstabParameters = (CrosstabParameters)Parameters;

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
                List<string> stratas = new List<string>();
                string crosstabVar = string.Empty;
                bool includeMissing = false;
                bool outcomeContinuous = false;
                bool showStrataSummaryOnly = false;

                //if (inputVariableList.ContainsKey("freqvar"))
                //{
                //    freqVar = inputVariableList["freqvar"];
                //}
                if (!String.IsNullOrEmpty(crosstabParameters.ColumnNames[0]))
                    freqVar = crosstabParameters.ColumnNames[0];

                //if (inputVariableList.ContainsKey("crosstabvar"))
                //{
                //    crosstabVar = inputVariableList["crosstabvar"];
                //}
                if (!String.IsNullOrEmpty(crosstabParameters.CrosstabVariableName))
                    crosstabVar = crosstabParameters.CrosstabVariableName;

                //if (inputVariableList.ContainsKey("weightvar"))
                //{
                //    weightVar = inputVariableList["weightvar"];
                //}
                if (!String.IsNullOrEmpty(crosstabParameters.WeightVariableName))
                    weightVar = crosstabParameters.WeightVariableName;

                //if (inputVariableList.ContainsKey("stratavar"))
                //{
                //    strataVar = inputVariableList["stratavar"];
                //}
                if (crosstabParameters.StrataVariableNames.Count > 0)
                {
                    stratas = crosstabParameters.StrataVariableNames;
                }

                //if (inputVariableList.ContainsKey("includemissing"))
                //{
                //    if (inputVariableList["includemissing"].Equals("true"))
                //    {
                //        includeMissing = true;
                //    }
                //}
                includeMissing = crosstabParameters.IncludeMissing;

                //if (inputVariableList.ContainsKey("stratasummaryonly"))
                //{
                //    if (inputVariableList["stratasummaryonly"].Equals("true"))
                //    {
                //        showStrataSummaryOnly = true;
                //    }
                //}                
                showStrataSummaryOnly = crosstabParameters.StrataSummaryOnly;

                //if (inputVariableList.ContainsKey("treatoutcomeascontinuous"))
                //{
                //    if (inputVariableList["treatoutcomeascontinuous"].Equals("true"))
                //    {
                //        outcomeContinuous = true;
                //    }
                //}
                outcomeContinuous = crosstabParameters.TreatOutcomeAsContinuous;

                //List<string> stratas = new List<string>();
                //if (!string.IsNullOrEmpty(strataVar))
                //{
                //    stratas.Add(strataVar);
                //}

                try
                {
                    crosstabParameters.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                    crosstabParameters.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);

                    if (this.DataFilters != null && this.DataFilters.Count > 0)
                    {
                        //GadgetOptions.CustomFilter = this.DataFilters.GenerateDataFilterString(false);
                        crosstabParameters.CustomFilter = this.DataFilters.GenerateDataFilterString(false);
                    }
                    else
                    {
                        crosstabParameters.CustomFilter = string.Empty;
                    }

                    bool runGroup = false;

                    Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables = new Dictionary<DataTable, List<DescriptiveStatistics>>();

                    List<string> allGroupFields = DashboardHelper.GetAllGroupsAsList();
					List<string> CRGFields = new List<string>();

					bool useCRG = true;
					if (useCRG)
					{
						CRGFields.Add(crosstabParameters.ColumnNames[0]);
					}

                    //if (GadgetOptions.MainVariableNames != null && GadgetOptions.MainVariableNames.Count > 0)
                    if (crosstabParameters.ColumnNames[0] != null && crosstabParameters.ColumnNames.Count > 0 &&
                        !(allGroupFields.Contains(crosstabParameters.ColumnNames[0]) || CRGFields.Contains(crosstabParameters.ColumnNames[0])))
                    {
                        Dictionary<DataTable, List<DescriptiveStatistics>> grpTables = new Dictionary<DataTable, List<DescriptiveStatistics>>();
                        CrosstabParameters newOptions = new CrosstabParameters(crosstabParameters);

                        foreach (string mainVariableName in crosstabParameters.ColumnNames)
                        {
                            //GadgetParameters newOptions = new GadgetParameters(GadgetOptions);
                            //CrosstabParameters newOptions = new CrosstabParameters(crosstabParameters);
                            //newOptions.MainVariableNames = null;
                            //newOptions.MainVariableName = mainVariableName;
                            //newOptions.ColumnNames[0] = null;
                            //newOptions.ColumnNames[0] = mainVariableName;

                            grpTables = DashboardHelper.GenerateFrequencyTable(newOptions);

                            foreach (KeyValuePair<DataTable, List<DescriptiveStatistics>> kvp in grpTables)
                            {
                                stratifiedFrequencyTables.Add(kvp.Key, kvp.Value);
                            }
                        }
                        //grpTables = DashboardHelper.GenerateFrequencyTable(newOptions);

                        //foreach (KeyValuePair<DataTable, List<DescriptiveStatistics>> kvp in grpTables)
                        //{
                        //    stratifiedFrequencyTables.Add(kvp.Key, kvp.Value);
                        //}
                        runGroup = true;
                    }
                    //                  else if (allGroupFields.Contains(GadgetOptions.MainVariableName))
                    else if (allGroupFields.Contains(crosstabParameters.ColumnNames[0]))
                    {
                        Dictionary<DataTable, List<DescriptiveStatistics>> grpTables = new Dictionary<DataTable, List<DescriptiveStatistics>>();

                        //foreach (string variableName in DashboardHelper.GetVariablesInGroup(GadgetOptions.MainVariableName))
                        foreach (string variableName in DashboardHelper.GetVariablesInGroup(crosstabParameters.ColumnNames[0]))
                        {
                            if (!allGroupFields.Contains(variableName) && DashboardHelper.TableColumnNames.ContainsKey(variableName))
                            {
                                //                              GadgetParameters newOptions = new GadgetParameters(GadgetOptions);
                                CrosstabParameters newOptions = new CrosstabParameters(crosstabParameters);
                                //newOptions.MainVariableNames = null;
                                //newOptions.MainVariableName = variableName;
                                newOptions.ColumnNames[0] = null;
                                newOptions.ColumnNames[0] = variableName;

                                grpTables = DashboardHelper.GenerateFrequencyTable(newOptions);

                                foreach (KeyValuePair<DataTable, List<DescriptiveStatistics>> kvp in grpTables)
                                {
                                    stratifiedFrequencyTables.Add(kvp.Key, kvp.Value);
                                }
                            }
                        }
                        runGroup = true;
					}
					else if (CRGFields.Contains(crosstabParameters.ColumnNames[0]))
					{
						Dictionary<DataTable, List<DescriptiveStatistics>> grpTables = new Dictionary<DataTable, List<DescriptiveStatistics>>();

						//foreach (string variableName in DashboardHelper.GetVariablesInGroup(GadgetOptions.MainVariableName))
						foreach (string variableName in CRGFields)
						{
							if (!allGroupFields.Contains(variableName) && DashboardHelper.TableColumnNames.ContainsKey(variableName))
							{
								//                              GadgetParameters newOptions = new GadgetParameters(GadgetOptions);
								CrosstabParameters newOptions = new CrosstabParameters(crosstabParameters);
								//newOptions.MainVariableNames = null;
								//newOptions.MainVariableName = variableName;
								newOptions.ColumnNames[0] = null;
								newOptions.ColumnNames[0] = variableName;

								grpTables = DashboardHelper.GenerateFrequencyTable(newOptions);

								foreach (KeyValuePair<DataTable, List<DescriptiveStatistics>> kvp in grpTables)
								{
									for (int rv = 1; rv < kvp.Key.Rows.Count; rv++)
									{
										DataTable kvpKey = kvp.Key.Copy();
										DataRow[] keydrs = kvpKey.Select();
										DataRow dr0 = keydrs[0];
										DataRow dri = keydrs[rv];
										kvpKey.Rows.Clear();
										kvpKey.Rows.Add(dr0);
										kvpKey.Rows.Add(dri);
										stratifiedFrequencyTables.Add(kvp.Key, kvp.Value);
									}
									stratifiedFrequencyTables.Add(kvp.Key, kvp.Value);
								}
							}
						}
						runGroup = true;
					}
					else
                    {
                        //                      stratifiedFrequencyTables = DashboardHelper.GenerateFrequencyTable(GadgetOptions/*, freqVar, weightVar, stratas, crosstabVar, useAllPossibleValues, sortHighLow, includeMissing, false*/);
                        stratifiedFrequencyTables = DashboardHelper.GenerateFrequencyTable(crosstabParameters/*, freqVar, weightVar, stratas, crosstabVar, useAllPossibleValues, sortHighLow, includeMissing, false*/);
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
                            CreateSmartTable(tableKvp.Key);

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

                            if (YesValues.Count > 0 && NoValues.Count > 0)
                            {
                                frequencies = ApplyRowValueMappings(frequencies);
                                frequencies = ApplyColumnValueMappings(frequencies);
                            }

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

                            if (YesValues.Count > 0 && NoValues.Count > 0)
                            {
                                frequencies = ApplyRowValueMappings(frequencies);
                                frequencies = ApplyColumnValueMappings(frequencies);
                            }

                            if (frequencies.Rows.Count == 0)
                            {
                                continue;
                            }

                            string tableHeading = frequencies.TableName;

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

                                    if (DashboardHelper.IsUserDefinedColumn(freqVar))
                                    {
                                        displayValue = DashboardHelper.GetFormattedOutput(freqVar, row[freqVar]);
                                    }
                                    else
                                    {
                                        if (DashboardHelper.IsUsingEpiProject && View.Fields[freqVar] is YesNoField)
                                        {
                                            if (row[freqVar].ToString().Equals("1"))
                                                displayValue = "Yes";
                                            else if (row[freqVar].ToString().Equals("0"))
                                                displayValue = "No";
                                        }
                                        else if (DashboardHelper.IsUsingEpiProject && View.Fields[freqVar] is DateField)
                                        {
                                            displayValue = string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:d}", row[freqVar]);
                                        }
                                        else if (DashboardHelper.IsUsingEpiProject && View.Fields[freqVar] is TimeField)
                                        {
                                            displayValue = string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:T}", row[freqVar]);
                                        }
                                        else
                                        {
                                            displayValue = DashboardHelper.GetFormattedOutput(freqVar, row[freqVar]);
                                        }
                                    }

                                    if (string.IsNullOrEmpty(displayValue))
                                    {
                                        Configuration config = DashboardHelper.Config;
                                        displayValue = config.Settings.RepresentationOfMissing;
                                    }

                                    this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(displayValue, new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Left, TextAlignment.Left, rowCount, 0, System.Windows.Visibility.Visible), FontWeights.Normal, count);


                                    int rowTotal = 0;
                                    columnCount = 1;

                                    foreach (DataColumn column in frequencies.Columns)
                                    {
                                        if (column.ColumnName.Equals(freqVar))
                                        {
                                            continue;
                                        }

                                        this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(row[column.ColumnName].ToString(), new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, columnCount, System.Windows.Visibility.Visible), FontWeights.Normal, count);
                                        columnCount++;

                                        int rowValue = 0;
                                        bool success = int.TryParse(row[column.ColumnName].ToString(), out rowValue);
                                        if (success)
                                        {
                                            totals[columnCount - 2] = totals[columnCount - 2] + rowValue;
                                            rowTotal = rowTotal + rowValue;
                                        }
                                    }
                                    this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(rowTotal.ToString(), new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, columnCount, System.Windows.Visibility.Visible), FontWeights.Bold, count);
                                    rowCount++;
                                }
                            }
                          
                            double[] tableChiSq = Epi.Statistics.SingleMxN.CalcChiSq(SortedRows, false);
                            double tableChiSqDF = (double)(SortedRows.Length - 1) * (SortedRows[0].ItemArray.Length - 2);
                            double tableChiSqP = Epi.Statistics.SharedResources.PValFromChiSq(tableChiSq[0], tableChiSqDF);
//                            double /*fisherTe*/stResult = Epi.Statistics.SingleMxN.CalcFisher(SortedRows, false);
                            double fisherTestResult = 0.0;
//                            fisherTestResult = Epi.Statistics.SingleMxN.FisherTest(SortedRows, false);
                            String disclaimer = "";
                            if (tableChiSq[1] == 5.0)
                                disclaimer = "An expected cell value is <5. X" + '\u00B2' + " may not be valid.";
                            if (tableChiSq[1] == 1.0)
                                disclaimer = "An expected cell value is <1. X" + '\u00B2' + " may not be valid.";

                            this.Dispatcher.BeginInvoke(new AddChiSquareDelegate(RenderChiSquare), tableChiSq[0], tableChiSqDF, tableChiSqP, fisherTestResult, disclaimer, strataValue);
                            //                            this.Dispatcher.BeginInvoke(new AddFisherDelegate(RenderFisherResults), SortedRows, tableChiSq[0], tableChiSqDF, tableChiSqP, fisherTestResult, disclaimer, strataValue);
                            System.Threading.Thread fisherThread = new System.Threading.Thread(()=>ComputeFisherResults(SortedRows, fisherTestResult, strataValue));
                            fisherThread.Start();
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
            //LoadingCombos = true;

            //string prevExposureField = string.Empty;
            //string prevOutcomeField = string.Empty;
            //string prevWeightField = string.Empty;
            //List<string> prevStrataFields = new List<string>();

            //if (update)
            //{
            //    if (cbxExposureField.SelectedIndex >= 0)
            //    {
            //        prevExposureField = cbxExposureField.SelectedItem.ToString();
            //    }
            //    if (cbxOutcomeField.SelectedIndex >= 0)
            //    {
            //        prevOutcomeField = cbxOutcomeField.SelectedItem.ToString();
            //    }
            //    if (cbxFieldWeight.SelectedIndex >= 0)
            //    {
            //        prevWeightField = cbxFieldWeight.SelectedItem.ToString();
            //    }
            //    foreach (string s in lbxFieldStrata.SelectedItems)
            //    {
            //        prevStrataFields.Add(s);
            //    }
            //}            

            //cbxExposureField.ItemsSource = null;
            //cbxExposureField.Items.Clear();

            //cbxOutcomeField.ItemsSource = null;
            //cbxOutcomeField.Items.Clear();

            //cbxFieldWeight.ItemsSource = null;
            //cbxFieldWeight.Items.Clear();

            //lbxFieldStrata.ItemsSource = null;
            //lbxFieldStrata.Items.Clear();

            //List<string> fieldNames = new List<string>();
            //List<string> weightFieldNames = new List<string>();
            //List<string> strataFieldNames = new List<string>();

            //weightFieldNames.Add(string.Empty);

            //ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.DateTime | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.UserDefined;
            //fieldNames = DashboardHelper.GetFieldsAsList(columnDataType);

            //columnDataType = ColumnDataType.Numeric | ColumnDataType.UserDefined;
            //weightFieldNames.AddRange(DashboardHelper.GetFieldsAsList(columnDataType));

            //columnDataType = ColumnDataType.Numeric | ColumnDataType.Boolean | ColumnDataType.Text | ColumnDataType.UserDefined;
            //strataFieldNames.AddRange(DashboardHelper.GetFieldsAsList(columnDataType));            

            //fieldNames.Sort();
            //weightFieldNames.Sort();
            //strataFieldNames.Sort();

            //if (fieldNames.Contains("SYSTEMDATE"))
            //{
            //    fieldNames.Remove("SYSTEMDATE");
            //}

            //if (DashboardHelper.IsUsingEpiProject)
            //{
            //    if (fieldNames.Contains("RecStatus")) fieldNames.Remove("RecStatus");
            //    if (weightFieldNames.Contains("RecStatus")) weightFieldNames.Remove("RecStatus");

            //    if (strataFieldNames.Contains("RecStatus")) strataFieldNames.Remove("RecStatus");
            //    if (strataFieldNames.Contains("FKEY")) strataFieldNames.Remove("FKEY");
            //    if (strataFieldNames.Contains("GlobalRecordId")) strataFieldNames.Remove("GlobalRecordId");
            //}

            //List<string> allFieldNames = new List<string>();
            //allFieldNames.AddRange(fieldNames);
            //allFieldNames.AddRange(DashboardHelper.GetAllGroupsAsList());

            //cbxExposureField.ItemsSource = allFieldNames;
            //cbxOutcomeField.ItemsSource = fieldNames;
            //cbxFieldWeight.ItemsSource = weightFieldNames;
            //lbxFieldStrata.ItemsSource = strataFieldNames;

            //if (cbxExposureField.Items.Count > 0)
            //{
            //    cbxExposureField.SelectedIndex = -1;
            //    cbxOutcomeField.SelectedIndex = -1;
            //}
            //if (cbxFieldWeight.Items.Count > 0)
            //{
            //    cbxFieldWeight.SelectedIndex = -1;
            //}    

            //if (update)
            //{
            //    cbxExposureField.SelectedItem = prevExposureField;
            //    cbxOutcomeField.SelectedItem = prevOutcomeField;
            //    cbxFieldWeight.SelectedItem = prevWeightField;

            //    foreach (string s in prevStrataFields)
            //    {
            //        lbxFieldStrata.SelectedItems.Add(s);
            //    }
            //}

            //LoadingCombos = false;
        }

        private void AddFreqGrid(string strataVar, string value, string crosstabVar, string groupReference, DataTable table)
        {
            CrosstabParameters crosstabParameters = (CrosstabParameters)Parameters;
            StackPanel crosstabPanel = new StackPanel();
            StackPanel groupPanel = new StackPanel();

            //bool strataSummaryOnly = (bool)checkboxStrataSummaryOnly.IsChecked;
            bool strataSummaryOnly = crosstabParameters.StrataSummaryOnly;
            //bool isGrouped = this.DashboardHelper.GetAllGroupsAsList().Contains(GadgetOptions.MainVariableName);
            bool isGrouped = this.DashboardHelper.GetAllGroupsAsList().Contains(crosstabParameters.ColumnNames[0]);
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
            txtExpanderHeader.Text =  value;
           
            txtExpanderHeader.Style = this.Resources["genericOutputExpanderText"] as Style;

            string formattedValue = value;

            //if (string.IsNullOrEmpty(strataVar) && GadgetOptions.StrataVariableNames.Count == 0)
            if (string.IsNullOrEmpty(strataVar) && crosstabParameters.StrataVariableNames.Count == 0)
            {
                formattedValue = crosstabVar;
            }
            else
            {
                if (value.EndsWith(" = "))
                {
                    formattedValue = value + DashboardHelper.Config.Settings.RepresentationOfMissing;
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

                Controls.GadgetTwoByTwoPanel twoByTwoPanel = null;

                try
                {
                    twoByTwoPanel = new Controls.GadgetTwoByTwoPanel(yyVal, ynVal, nyVal, nnVal);
                }
                catch (DivideByZeroException ex)
                {
                    return;
                }

                twoByTwoPanel.ValuesUpdated += new TwoByTwoValuesUpdatedHandler(twoByTwoPanel_ValuesUpdated);

                //if (checkboxHorizontal.IsChecked == true)
                if (crosstabParameters.HorizontalDisplayMode == true)
                {
                    twoByTwoPanel.Orientation = Orientation.Vertical;
                }
                else
                {
                    twoByTwoPanel.Orientation = Orientation.Horizontal;
                }

                twoByTwoPanel.Margin = new Thickness(5);

                if (isGrouped)
                {
                    twoByTwoPanel.ExposureVariable = groupReference;
                    //--Ei-83
                    if (crosstabParameters.UsePromptsForColumnNames == true) { twoByTwoPanel.ExposureVariable = ShowFieldPrompt(groupReference); }
                    //--
                }
                else
                {
                    //twoByTwoPanel.ExposureVariable = this.GadgetOptions.MainVariableName;
                    twoByTwoPanel.ExposureVariable = crosstabParameters.ColumnNames[0];
                    //EI-403
                    if (twoByTwoPanel.ExposureVariable.Length > MaxColumnLength)
                    {
                        twoByTwoPanel.ExposureVariable = twoByTwoPanel.ExposureVariable.Substring(0, MaxColumnLength) + "...";
                    }
                    //--Ei-83
                    if (crosstabParameters.UsePromptsForColumnNames == true) { twoByTwoPanel.ExposureVariable = ShowFieldPrompt(crosstabParameters.ColumnNames[0]); }
                    //--
                    if (strataSummaryOnly)
                    {
                        twoByTwoPanel.Visibility = System.Windows.Visibility.Collapsed;
                        expander.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
               //EI-403
                twoByTwoPanel.OutcomeVariable = crosstabParameters.CrosstabVariableName;
                if (twoByTwoPanel.OutcomeVariable.Length > MaxColumnLength)
                {
                    twoByTwoPanel.OutcomeVariable = twoByTwoPanel.OutcomeVariable.Substring(0, MaxColumnLength) + "...";
                }
                //Ei-83
                 if (crosstabParameters.UsePromptsForColumnNames == true)
                    twoByTwoPanel.OutcomeVariable = ShowFieldPrompt(crosstabParameters.CrosstabVariableName);
                //--

                twoByTwoPanel.OutcomeYesLabel = table.Columns[1].ColumnName;
                twoByTwoPanel.OutcomeNoLabel = table.Columns[2].ColumnName;

                twoByTwoPanel.ExposureYesLabel = table.Rows[0][0].ToString();
                twoByTwoPanel.ExposureNoLabel = table.Rows[1][0].ToString();

                if (YesValues != null && YesValues.Count > 0 && NoValues != null && NoValues.Count > 0)
                {
                    twoByTwoPanel.OutcomeYesLabel = Config.Settings.RepresentationOfYes;
                    twoByTwoPanel.OutcomeNoLabel = Config.Settings.RepresentationOfNo;

                    twoByTwoPanel.ExposureYesLabel = Config.Settings.RepresentationOfYes;
                    twoByTwoPanel.ExposureNoLabel = Config.Settings.RepresentationOfNo;
                }

                if (crosstabParameters.ShowPercents)
                {
                    twoByTwoPanel.ShowRowColumnPercents = true;
                }
                else
                {
                    twoByTwoPanel.ShowRowColumnPercents = false;
                }

                if (string.IsNullOrEmpty(strataVar) && crosstabParameters.StrataVariableNames.Count == 0)
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
                        sRow.RiskLower = twoByTwoPanel.RiskRatioLower;
                        sRow.RiskUpper = twoByTwoPanel.RiskRatioUpper;
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
                    sRow.RiskLower = twoByTwoPanel.RiskRatioLower;
                    sRow.RiskUpper = twoByTwoPanel.RiskRatioUpper;
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
                        sRow.RiskLower = null;
                        sRow.RiskUpper = null;
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
                    StrataExpanderList.Add(expander);

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

            Canvas exposureCanvas = new Canvas();
            Grid.SetColumn(exposureCanvas, 0);
            Grid.SetRow(exposureCanvas, 1);

            exposureCanvas.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            exposureCanvas.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            exposureCanvas.Width = 25;
            Binding canvasHeightBinding = new Binding("Height");
            canvasHeightBinding.Source = grid;
            exposureCanvas.SetBinding(Grid.HeightProperty, canvasHeightBinding);

            TextBlock tblock1 = new TextBlock();

            tblock1.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            tblock1.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            tblock1.Margin = new Thickness(0, 0, -5, 0);
            tblock1.FontWeight = FontWeights.Bold;
            tblock1.FontSize = tblock1.FontSize + 2;
            //if (DashboardHelper.GetAllGroupsAsList().Contains(cbxExposureField.SelectedItem.ToString()))
            if (DashboardHelper.GetAllGroupsAsList().Contains(crosstabParameters.ColumnNames[0]))
            {
                tblock1.Text = value;
            }
            else
            {
                //tblock1.Text = cbxExposureField.SelectedItem.ToString();
                tblock1.Text = crosstabParameters.ColumnNames[0];
            }
            //--EI-83
            if (crosstabParameters.UsePromptsForColumnNames == true)
            {
                string txtcolumnName = tblock1.Text;
                txtcolumnName = ShowFieldPrompt(txtcolumnName);
                tblock1.Text = txtcolumnName;
            }
            //

            Typeface typeFace = new Typeface(new FontFamily("Global User Interface"), tblock1.FontStyle, tblock1.FontWeight, tblock1.FontStretch);
            FormattedText ftxt = new FormattedText(tblock1.Text, System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, typeFace, tblock1.FontSize, Brushes.Black);

            RotateTransform rotate = new RotateTransform(270);

            // This code works, but only if grid height is known at this point.
            //double actualCanvasHeight = exposureCanvas.Height;
            //double actualControlWidth = ftxt.Width;//tblockExposure.ActualWidth;

            //double x = Canvas.GetLeft(tblock1);

            //if (actualCanvasHeight > actualControlWidth)
            //{
            //    double diff = actualCanvasHeight - actualControlWidth;
            //    double bottom = diff / 2;
            //    Canvas.SetBottom(tblock1, bottom);
            //}

            tblock1.RenderTransform = rotate;

            exposureCanvas.Children.Add(tblock1);

            //Grid.SetColumn(tblock1, 0);
            //Grid.SetRow(tblock1, 1);
            //gridOuter.Children.Add(tblock1);

            gridOuter.Children.Add(exposureCanvas);

            TextBlock tblock2 = new TextBlock();
            tblock2.Name = "tblockOutcomeGridHeader";
            tblock2.FontWeight = FontWeights.Bold;
            tblock2.FontSize = tblock1.FontSize + 2;
            //tblock2.Text = cbxOutcomeField.SelectedItem.ToString();
            tblock2.Text = crosstabParameters.CrosstabVariableName;
            //--EI-83
            if (crosstabParameters.UsePromptsForColumnNames == true)
            {
                string txtcolumnName = tblock2.Text;
                txtcolumnName = ShowFieldPrompt(txtcolumnName);
                tblock2.Text = txtcolumnName;
            }
            //
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

            //if (string.IsNullOrEmpty(strataVar) && GadgetOptions.StrataVariableNames.Count == 0)
            if (string.IsNullOrEmpty(strataVar) && crosstabParameters.StrataVariableNames.Count == 0)
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
            StrataGridList.Add(grid);
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
            foreach (Expander expander in this.StrataExpanderList)
            {
                expander.IsExpanded = true;
                expander.Visibility = System.Windows.Visibility.Visible;
            }
        }

        void strataListPanel_RowClicked(UIElement control, string strataValue)
        {
            foreach (Expander expander in this.StrataExpanderList)
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

        void twoByTwoPanel_ValuesUpdated(System.Windows.Controls.UserControl control)
        {
            CrosstabParameters crosstabParameters = (CrosstabParameters)Parameters;
            if (((EpiDashboard.Controls.GadgetTwoByTwoPanel)(control)).ExposureSwap)
                crosstabParameters.ExposureSwap = !crosstabParameters.ExposureSwap;          
            if (((EpiDashboard.Controls.GadgetTwoByTwoPanel)(control)).OutComeSwap)
                crosstabParameters.OutcomeSwap = !crosstabParameters.OutcomeSwap;   
        
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

        private void SetGridText(string strataValue, TextBlockConfig textBlockConfig, FontWeight fontWeight, double total)
        {
            CrosstabParameters crosstabParameters = (CrosstabParameters)Parameters;
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
            else if (colNum > 0 && rowNum > 0 && colNum < grid.ColumnDefinitions.Count - 1)
            {
                double value = double.Parse(txt.Text);
                double pct = value / total;

                double break1 = 0;
                double break2 = 20;
                double break3 = 40;
                double break4 = 60;
                double break5 = 80;

                //double.TryParse(txtPct1.Text, out break1);
                //double.TryParse(txtPct2.Text, out break2);
                //double.TryParse(txtPct3.Text, out break3);
                //double.TryParse(txtPct4.Text, out break4);
                //double.TryParse(txtPct5.Text, out break5);
                double.TryParse(crosstabParameters.Break1, out break1);
                double.TryParse(crosstabParameters.Break2, out break2);
                double.TryParse(crosstabParameters.Break3, out break3);
                double.TryParse(crosstabParameters.Break4, out break4);
                double.TryParse(crosstabParameters.Break5, out break5);


                //if (cmbBreakType.SelectedIndex == 1) { pct = value; }
                if (crosstabParameters.BreakType == 1) { pct = value; }
                else
                {
                    break1 = break1 / 100;
                    break2 = break2 / 100;
                    break3 = break3 / 100;
                    break4 = break4 / 100;
                    break5 = break5 / 100;
                }

                //if (checkboxConditionalShading.IsChecked == true && (break1 < break2 && break2 < break3 && break3 < break4 && break4 < break5))
                if (crosstabParameters.ConditionalShading == true && (break1 < break2 && break2 < break3 && break3 < break4 && break4 < break5))
                {
                    if (pct >= break1 && pct < break2)
                    {
                        Rectangle rctHeat = new Rectangle();
                        rctHeat.Fill = ColorList[4];
                        Grid.SetRow(rctHeat, rowNum);
                        Grid.SetColumn(rctHeat, colNum);
                        grid.Children.Add(rctHeat);
                    }
                    else if (pct >= break2 && pct < break3)
                    {
                        Rectangle rctHeat = new Rectangle();
                        rctHeat.Fill = ColorList[3];
                        Grid.SetRow(rctHeat, rowNum);
                        Grid.SetColumn(rctHeat, colNum);
                        grid.Children.Add(rctHeat);
                    }
                    else if (pct >= break3 && pct < break4)
                    {
                        Rectangle rctHeat = new Rectangle();
                        rctHeat.Fill = ColorList[2];
                        Grid.SetRow(rctHeat, rowNum);
                        Grid.SetColumn(rctHeat, colNum);
                        grid.Children.Add(rctHeat);
                    }
                    else if (pct >= break4 && pct < break5)
                    {
                        Rectangle rctHeat = new Rectangle();
                        rctHeat.Fill = ColorList[1];
                        Grid.SetRow(rctHeat, rowNum);
                        Grid.SetColumn(rctHeat, colNum);
                        grid.Children.Add(rctHeat);
                    }
                    else if (pct >= break5 && pct <= double.MaxValue)
                    {
                        Rectangle rctHeat = new Rectangle();
                        rctHeat.Fill = ColorList[0];
                        Grid.SetRow(rctHeat, rowNum);
                        Grid.SetColumn(rctHeat, colNum);
                        grid.Children.Add(rctHeat);
                    }
                }
                else
                {
                    Rectangle rctNormal = new Rectangle();
                    rctNormal.Fill = Brushes.White;
                    Grid.SetRow(rctNormal, rowNum);
                    Grid.SetColumn(rctNormal, colNum);
                    grid.Children.Add(rctNormal);
                }
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

        private Color GetColorAtPoint(Rectangle theRec, Point thePoint)
        {
            LinearGradientBrush br = (LinearGradientBrush)theRec.Fill;
            double y3 = thePoint.Y; double x3 = thePoint.X;
            double x1 = br.StartPoint.X * theRec.Width;
            double y1 = br.StartPoint.Y * theRec.Height;
            Point p1 = new Point(x1, y1);
            double x2 = br.EndPoint.X * theRec.Width;
            double y2 = br.EndPoint.Y * theRec.Height;
            Point p2 = new Point(x2, y2);
            Point p4 = new Point();
            if (y1 == y2)
            { p4 = new Point(x3, y1); }
            else if (x1 == x2)
            { p4 = new Point(x1, y3); }
            else
            {
                double m = (y2 - y1) / (x2 - x1);
                double m2 = -1 / m;
                double b = y1 - m * x1;
                double c = y3 - m2 * x3;
                double x4 = (c - b) / (m - m2);
                double y4 = m * x4 + b; p4 = new Point(x4, y4);
            }
            double d4 = dist(p4, p1, p2);
            double d2 = dist(p2, p1, p2);
            double x = d4 / d2;
            double max = br.GradientStops.Max(n => n.Offset);
            if (x > max) { x = max; }
            double min = br.GradientStops.Min(n => n.Offset);
            if (x < min) { x = min; }
            GradientStop gs0 = br.GradientStops.Where(n => n.Offset <= x).OrderBy(n => n.Offset).Last();
            GradientStop gs1 = br.GradientStops.Where(n => n.Offset >= x).OrderBy(n => n.Offset).First();
            float y = 0f;
            if (gs0.Offset != gs1.Offset)
            { y = (float)((x - gs0.Offset) / (gs1.Offset - gs0.Offset)); }
            Color cx = new Color();
            if (br.ColorInterpolationMode == ColorInterpolationMode.ScRgbLinearInterpolation)
            {
                float aVal = (gs1.Color.ScA - gs0.Color.ScA) * y + gs0.Color.ScA;
                float rVal = (gs1.Color.ScR - gs0.Color.ScR) * y + gs0.Color.ScR;
                float gVal = (gs1.Color.ScG - gs0.Color.ScG) * y + gs0.Color.ScG;
                float bVal = (gs1.Color.ScB - gs0.Color.ScB) * y + gs0.Color.ScB;
                cx = Color.FromScRgb(aVal, rVal, gVal, bVal);
            }
            else
            {
                byte aVal = (byte)((gs1.Color.A - gs0.Color.A) * y + gs0.Color.A);
                byte rVal = (byte)((gs1.Color.R - gs0.Color.R) * y + gs0.Color.R);
                byte gVal = (byte)((gs1.Color.G - gs0.Color.G) * y + gs0.Color.G);
                byte bVal = (byte)((gs1.Color.B - gs0.Color.B) * y + gs0.Color.B);
                cx = Color.FromArgb(aVal, rVal, gVal, bVal);
            }
            return cx;
        }

        private double dist(Point px, Point po, Point pf)
        {
            double d = Math.Sqrt((px.Y - po.Y) * (px.Y - po.Y) + (px.X - po.X) * (px.X - po.X));
            if (((px.Y < po.Y) && (pf.Y > po.Y)) || ((px.Y > po.Y) && (pf.Y < po.Y)) || ((px.Y == po.Y) && (px.X < po.X) && (pf.X > po.X)) || ((px.Y == po.Y) && (px.X > po.X) && (pf.X < po.X)))
            {
                d = -d;
            }
            return d;
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

        private void RenderChiSquare(double tableChiSq, double tableChiSqDF, double tableChiSqP, double tableFisherP, string disclaimer, string value)
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
            //EI-146
            if (!((CrosstabParameters)Parameters).DisplayChiSq)
            {
                return;
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

            // Commenting out Fisher's Exact rendering for now.

            grid.RowDefinitions.Add(new RowDefinition());
            TextBlock txt1b = new TextBlock();
            txt1b.Text = "Fisher's Exact";
            txt1b.FontWeight = FontWeights.Bold;
            txt1b.Margin = margin;
            txt1b.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            Grid.SetRow(txt1b, 2);
            Grid.SetColumn(txt1b, 0);
            grid.Children.Add(txt1b);

            TextBlock txt15b = new TextBlock();
            txt15b.Text = "";
            txt15b.Margin = margin;
            txt15b.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            Grid.SetRow(txt15b, 2);
            Grid.SetColumn(txt15b, 1);
            grid.Children.Add(txt15b);

            TextBlock txt2b = new TextBlock();
            txt2b.Text = "computing...";
            txt2b.Margin = margin;
            txt2b.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            Grid.SetRow(txt2b, 2);
            Grid.SetColumn(txt2b, 2);
            grid.Children.Add(txt2b);
            /*
                        grid.RowDefinitions.Add(new RowDefinition());
                        TextBlock txt4b = new TextBlock();
                        txt4b.Text = "(Computed by R)";
                        txt4b.Margin = margin;
                        txt4b.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                        Grid.SetRow(txt4b, 3);
                        Grid.SetColumn(txt4b, 2);
                        grid.Children.Add(txt4b);
            */
            if (!string.IsNullOrEmpty(disclaimer) && !Double.IsNaN(tableChiSq))
            {
                //                grid.RowDefinitions.Add(new RowDefinition());
//                foreach (TextBlock t in gridDisclaimerList)
//                {
//                    t.Text = disclaimer;
//                }

                    StackPanel g = (StackPanel)grid.Parent;

                TextBlock txt7 = new TextBlock();
                txt7.Text = disclaimer;
                txt7.Margin = margin;
                txt7.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                txt7.FontWeight = FontWeights.DemiBold;
//                Grid.SetRow(txt7, 2);
//                Grid.SetColumn(txt7, 0);
//                Grid.SetColumnSpan(txt7, 3);
//                grid.Children.Add(txt7);
                if (g == null) { }
                else
                {
//                    textBlock.Text = string.Empty;
                    textBlock.Visibility = System.Windows.Visibility.Collapsed;
                    g.Children.Add(txt7);
                }
            }
        }
        private void ComputeFisherResults(DataRow[] SortedRows, double tableFisherP, string value)
        {
            try
            {
                tableFisherP = Epi.Statistics.SingleMxN.CalcFisher(SortedRows, false);
            }
            catch (Exception except)
            {
                tableFisherP = Double.NegativeInfinity;
            }
            this.Dispatcher.BeginInvoke(new AddFisherDelegate(RenderFisherResults), SortedRows, tableFisherP, value);
        }

        private void RenderFisherResults(DataRow[] SortedRows, double tableFisherP, string value)
        {
            //EI-146
            if (!((CrosstabParameters)Parameters).DisplayChiSq)
            {
                return;
            }

            Grid grid = GetStrataChiSquareGrid(value);
            grid.Visibility = System.Windows.Visibility.Visible;

            Thickness margin = new Thickness(4);

            // Commenting out Fisher's Exact rendering for now.

            foreach (TextBlock text_b in grid.Children)
            {
                if (Grid.GetRow(text_b) == 2 && Grid.GetColumn(text_b) == 2)
                {
                    if (Double.IsNegativeInfinity(tableFisherP))
                        text_b.Text = "Cannot compute. Too many iterations.";
                    else
                        text_b.Text = tableFisherP.ToString("F4");
                }
            }
/*
            grid.RowDefinitions.Add(new RowDefinition());
            TextBlock txt1b = new TextBlock();
            txt1b.Text = "Fisher's Exact";
            txt1b.FontWeight = FontWeights.Bold;
            txt1b.Margin = margin;
            txt1b.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            Grid.SetRow(txt1b, 2);
            Grid.SetColumn(txt1b, 0);
            grid.Children.Add(txt1b);

            TextBlock txt2b = new TextBlock();
            txt2b.Text = tableFisherP.ToString("F4");
            txt2b.Margin = margin;
            txt2b.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            Grid.SetRow(txt2b, 2);
            Grid.SetColumn(txt2b, 2);
            grid.Children.Add(txt2b);
*/
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
            CrosstabParameters crosstabParameters = (CrosstabParameters)Parameters;
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

                    UIElement element = grid.Children.Cast<UIElement>().Last(x => Grid.GetRow(x) == rdCount && Grid.GetColumn(x) == cdCount);
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

                        //if (checkboxRowColPercents.IsChecked == false)
                        if (crosstabParameters.ShowPercents == false)
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

        protected override void RenderFinish()
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            foreach (Grid freqGrid in StrataGridList)
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

            HideConfigPanel();
            CheckAndSetPosition();
            
        }

        protected override void RenderFinishWithWarning(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            foreach (Grid freqGrid in StrataGridList)
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

        protected override void RenderFinishWithError(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = Controls.MessagePanelType.ErrorPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        //private void CheckVariables()
        //{            
        //lbxFieldStrata.IsEnabled = true;
        //if (cbxExposureField.SelectedIndex >= 0)
        //{
        //    string exposureFieldName = cbxExposureField.SelectedItem.ToString();

        //    if (DashboardHelper.GetAllGroupsAsList().Contains(exposureFieldName))
        //    {
        //        lbxFieldStrata.IsEnabled = false;
        //        lbxFieldStrata.SelectedItems.Clear();

        //        btnValueMappings.IsEnabled = true;
        //    }
        //    else if (DashboardHelper.IsColumnText(exposureFieldName) || DashboardHelper.IsColumnNumeric(exposureFieldName))
        //    {
        //        btnValueMappings.IsEnabled = true;
        //    }

        //}
        //else
        //{
        //    if (!LoadingCombos)
        //    {
        //        btnValueMappings.IsEnabled = false;
        //        YesValues = new List<string>();
        //        NoValues = new List<string>();
        //    }
        //    }
        //}

        //private void CreateInputVariableList()
        //{
        //MOVED TO CROSSTAB PROPERTIES
        //Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

        //GadgetOptions.MainVariableName = string.Empty;
        //GadgetOptions.WeightVariableName = string.Empty;
        //GadgetOptions.StrataVariableNames = new List<string>();
        //GadgetOptions.CrosstabVariableName = string.Empty;

        //smartTable = (bool)checkboxSmartTable.IsChecked;
        //showPercents = (bool)checkboxRowColPercents.IsChecked;

        //if (cbxExposureField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxExposureField.SelectedItem.ToString()))
        //{
        //    inputVariableList.Add("freqvar", cbxExposureField.SelectedItem.ToString());
        //    GadgetOptions.MainVariableName = cbxExposureField.SelectedItem.ToString();
        //}
        //else
        //{
        //    return;
        //}

        //if (cbxOutcomeField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxOutcomeField.SelectedItem.ToString()))
        //{
        //    inputVariableList.Add("crosstabvar", cbxOutcomeField.SelectedItem.ToString());
        //    GadgetOptions.CrosstabVariableName = cbxOutcomeField.SelectedItem.ToString();
        //}

        //if (cbxFieldWeight.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldWeight.SelectedItem.ToString()))
        //{
        //    inputVariableList.Add("weightvar", cbxFieldWeight.SelectedItem.ToString());
        //    GadgetOptions.WeightVariableName = cbxFieldWeight.SelectedItem.ToString();
        //}

        //if (lbxFieldStrata.SelectedItems.Count > 0)
        //{
        //    GadgetOptions.StrataVariableNames = new List<string>();
        //    foreach (string s in lbxFieldStrata.SelectedItems)
        //    {
        //        GadgetOptions.StrataVariableNames.Add(s);
        //    }
        //}

        //if (checkboxAllValues.IsChecked == true)
        //{
        //    inputVariableList.Add("allvalues", "true");
        //    GadgetOptions.ShouldUseAllPossibleValues = true;
        //}
        //else
        //{
        //    inputVariableList.Add("allvalues", "false");
        //    GadgetOptions.ShouldUseAllPossibleValues = false;
        //}

        //if (checkboxCommentLegalLabels.IsChecked == true)
        //{
        //    GadgetOptions.ShouldShowCommentLegalLabels = true;
        //}
        //else
        //{
        //    GadgetOptions.ShouldShowCommentLegalLabels = false;
        //}

        //if (checkboxIncludeMissing.IsChecked == true)
        //{
        //    inputVariableList.Add("includemissing", "true");
        //    GadgetOptions.ShouldIncludeMissing = true;
        //}
        //else
        //{
        //    inputVariableList.Add("includemissing", "false");
        //    GadgetOptions.ShouldIncludeMissing = false;
        //}

        //if (checkboxOutcomeContinuous.IsChecked == true)
        //{
        //    inputVariableList.Add("treatoutcomeascontinuous", "true");                
        //}
        //else
        //{
        //    inputVariableList.Add("treatoutcomeascontinuous", "false");                
        //}

        //if (checkboxStrataSummaryOnly.IsChecked == true)
        //{
        //    inputVariableList.Add("stratasummaryonly", "true");
        //}
        //else
        //{
        //    inputVariableList.Add("stratasummaryonly", "false");
        //}

        //GadgetOptions.ShouldIncludeFullSummaryStatistics = false;
        //GadgetOptions.InputVariableList = inputVariableList;
        //}
        #endregion

        #region Private Properties
        private List<SolidColorBrush> ColorList { get; set; }
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
        #endregion // Private Properties

        #region IGadget Members
        /// <summary>
        /// Updates the variable names available in the gadget's properties
        /// </summary>
        public override void UpdateVariableNames()
        {
            FillComboboxes(true);
        }

        private void GenerateHeatMap()
        {
            CrosstabParameters crosstabParameters = (CrosstabParameters)Parameters;

            //LinearGradientBrush gradientBrush = new LinearGradientBrush((rctHighColor.Fill as SolidColorBrush).Color, (rctLowColor.Fill as SolidColorBrush).Color, 0);
            LinearGradientBrush gradientBrush = new LinearGradientBrush(crosstabParameters.HiColorFill.Color, crosstabParameters.LoColorFill.Color, 0);
            Rectangle temp = new Rectangle();
            temp.Width = 256;
            temp.Height = 256;
            temp.Fill = gradientBrush;

            ColorList = new List<SolidColorBrush>();

            List<SolidColorBrush> BlueShades = new List<SolidColorBrush>();

            int rgbFactor = 255 / 4; // classCount;

            for (int j = 0; j < 256; j = j + rgbFactor)
            {
                Color color = GetColorAtPoint(temp, new Point(j, j));
                color.A = 0xF0;
                BlueShades.Add(new SolidColorBrush(color));
            }

            //BlueShades[BlueShades.Count-1] = (rctLowColor.Fill as SolidColorBrush);
            BlueShades[BlueShades.Count - 1] = (crosstabParameters.LoColorFill as SolidColorBrush);

            ColorList = BlueShades;

            //foreach (List<SolidColorBrush> brushList in ColorList)
            //{
            //    brushList.Reverse();
            //}
        }

        public override void RefreshResults()
        {
            CrosstabParameters crosstabParameters = (CrosstabParameters)Parameters;
            isRunningGrouped2x2 = null;

            //if (!LoadingCombos && GadgetOptions != null && cbxExposureField.SelectedIndex > -1 && cbxOutcomeField.SelectedIndex > -1)
            if (Parameters != null && crosstabParameters.ColumnNames.Count > 0)
            {
                if (!String.IsNullOrEmpty(crosstabParameters.ColumnNames[0]) && (!String.IsNullOrEmpty(crosstabParameters.CrosstabVariableName)))
                {
                    //CreateInputVariableList();

                    //if (checkboxConditionalShading.IsChecked == true)
                    if (crosstabParameters.ConditionalShading == true)
                    {
                        GenerateHeatMap();
                    }
                    infoPanel.Visibility = System.Windows.Visibility.Collapsed;
                    waitPanel.Visibility = System.Windows.Visibility.Visible;
                    messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
                    descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
                    baseWorker = new BackgroundWorker();
                    baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
                    baseWorker.RunWorkerAsync();
                    base.RefreshResults();
                }
                //else if (!LoadingCombos && (cbxExposureField.SelectedIndex == -1 && cbxOutcomeField.SelectedIndex == -1))
                else if (String.IsNullOrEmpty(crosstabParameters.ColumnNames[0]) || (String.IsNullOrEmpty(crosstabParameters.CrosstabVariableName)))
                {
                    ClearResults();
                    waitPanel.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Shows the Properties Panel for the gadget to set its options and parameters.
        /// </summary>
        /// 
        Controls.GadgetProperties.CrosstabProperties properties = null;
        public override void ShowHideConfigPanel()
        {
            Popup = new DashboardPopup();
            Popup.Parent = ((this.Parent as DragCanvas).Parent as ScrollViewer).Parent as Grid;
            properties = new Controls.GadgetProperties.CrosstabProperties(this.DashboardHelper, this, (CrosstabParameters)Parameters, StrataGridList);

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
                properties = new Controls.GadgetProperties.CrosstabProperties(this.DashboardHelper, this, (CrosstabParameters)Parameters, StrataGridList);
            properties.Height = (Convert.ToInt32(i_StandardHeight * f_HeightRatio)) / 1.07;
            properties.Width = (Convert.ToInt32(i_StandardWidth * f_WidthRatio)) / 1.07;

        }
        /// <summary>
        /// Accepts changes to the Properties panel and refreshes the results.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void properties_ChangesAccepted(object sender, EventArgs e)
        {
            Controls.GadgetProperties.CrosstabProperties properties = Popup.Content as Controls.GadgetProperties.CrosstabProperties;
            this.Parameters = properties.Parameters;
            this.DataFilters = properties.DataFilters;
            this.CustomOutputHeading = Parameters.GadgetTitle;
            this.CustomOutputDescription = Parameters.GadgetDescription;
            this.YesValues = (properties.Parameters as CrosstabParameters).YesValues;
            this.NoValues = (properties.Parameters as CrosstabParameters).NoValues;
            Popup.Close();
            if (properties.HasSelectedFields)
            {
                RefreshResults();
            }
        }

        /// <summary>
        /// Closes the Properties panel when cancel is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            //Dictionary<string, string> inputVariableList = GadgetOptions.InputVariableList;
            CrosstabParameters crosstabParameters = (CrosstabParameters)Parameters;

            System.Xml.XmlElement element = doc.CreateElement("crosstabGadget");  //Note: 7.1.3.0 has this as "frequencyGadget"
            string xmlString = string.Empty;
            element.InnerXml = xmlString;
            element.AppendChild(SerializeFilters(doc));

            System.Xml.XmlAttribute id = doc.CreateAttribute("id");
            System.Xml.XmlAttribute locationY = doc.CreateAttribute("top");
            System.Xml.XmlAttribute locationX = doc.CreateAttribute("left");
            System.Xml.XmlAttribute collapsed = doc.CreateAttribute("collapsed");
            System.Xml.XmlAttribute type = doc.CreateAttribute("gadgetType");
            System.Xml.XmlAttribute actualHeight = doc.CreateAttribute("actualHeight");

            id.Value = this.UniqueIdentifier.ToString();
            locationY.Value = Canvas.GetTop(this).ToString("F0");
            locationX.Value = Canvas.GetLeft(this).ToString("F0");
            collapsed.Value = IsCollapsed.ToString();
            type.Value = "EpiDashboard.CrosstabControl";
            actualHeight.Value = this.ActualHeight.ToString();

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);
            element.Attributes.Append(id);
            if (IsCollapsed == false)
            {
                element.Attributes.Append(actualHeight);
            }

            //string freqVar = string.Empty;
            //string crosstabVar = string.Empty;
            //string strataVar = string.Empty;
            //string weightVar = string.Empty;
            //string sort = string.Empty;
            //bool allValues = false;
            //bool showConfLimits = true;
            //bool showCumulativePercent = true;
            //bool includeMissing = false;
            //bool outcomeContinuous = false;
            //bool rowColPercents = (bool)checkboxRowColPercents.IsChecked;
            //string layoutMode = "horizontal";
            //bool showStrataSummaryOnly = false;


            //if (inputVariableList.ContainsKey("freqvar"))
            //{
            //    freqVar = inputVariableList["freqvar"].Replace("<", "&lt;");
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
            //if (inputVariableList.ContainsKey("sort"))
            //{
            //    sort = inputVariableList["sort"];
            //}
            //if (inputVariableList.ContainsKey("allvalues"))
            //{
            //    allValues = bool.Parse(inputVariableList["allvalues"]);
            //}
            //if (inputVariableList.ContainsKey("showconflimits"))
            //{
            //    showConfLimits = bool.Parse(inputVariableList["showconflimits"]);
            //}
            //if (inputVariableList.ContainsKey("showcumulativepercent"))
            //{
            //    showCumulativePercent = bool.Parse(inputVariableList["showcumulativepercent"]);
            //}
            //if (inputVariableList.ContainsKey("includemissing"))
            //{
            //    includeMissing = bool.Parse(inputVariableList["includemissing"]);
            //}
            //if (inputVariableList.ContainsKey("treatoutcomeascontinuous"))
            //{
            //    outcomeContinuous = bool.Parse(inputVariableList["treatoutcomeascontinuous"]);
            //}
            //if (inputVariableList.ContainsKey("stratasummaryonly"))
            //{
            //    showStrataSummaryOnly = bool.Parse(inputVariableList["stratasummaryonly"]);                
            //}
            //if (checkboxHorizontal.IsChecked == true)
            //{
            //    layoutMode = "vertical";
            //}

            //CustomOutputHeading = headerPanel.Text;
            //CustomOutputDescription = descriptionPanel.Text;
            CustomOutputHeading = crosstabParameters.GadgetTitle;
            CustomOutputDescription = crosstabParameters.GadgetDescription;

            //string xmlString =
            //"<mainVariable>" + freqVar + "</mainVariable>" +

            XmlElement mainVarElement = doc.CreateElement("mainVariable");
            if (crosstabParameters.ColumnNames.Count > 0)
            {
                if (!String.IsNullOrEmpty(crosstabParameters.ColumnNames[0].ToString()))
                {
                    mainVarElement.InnerText = crosstabParameters.ColumnNames[0].ToString();
                    element.AppendChild(mainVarElement);
                }
            }

            //"<crosstabVariable>" + crosstabVar + "</crosstabVariable>";
            XmlElement crosstabVarElement = doc.CreateElement("crosstabVariable");
            if (!String.IsNullOrEmpty(crosstabParameters.CrosstabVariableName.ToString()))
            {
                crosstabVarElement.InnerText = crosstabParameters.CrosstabVariableName.ToString();
                element.AppendChild(crosstabVarElement);
            }

            //if(GadgetOptions.StrataVariableNames.Count == 1) 
            //{
            //    //xmlString = xmlString + "<strataVariable>" + GadgetOptions.StrataVariableNames[0].Replace("<", "&lt;") + "</strataVariable>";
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
            if (crosstabParameters.StrataVariableNames.Count == 1)
            {
                StrataVariableNameElement.InnerText = crosstabParameters.StrataVariableNames[0].ToString();
                element.AppendChild(StrataVariableNameElement);
            }
            else if (crosstabParameters.StrataVariableNames.Count > 1)
            {
                //string xmlstrataVarString = string.Empty;
                //foreach (string strataColumn in freqParameters.StrataVariableNames)
                //{
                //    xmlstrataVarString = xmlstrataVarString + "<strataVariable>" + strataColumn.Replace("<", "&lt;") + "</strataVariable>";
                //}
                //StrataVariableNamesElement.InnerXml = xmlstrataVarString;
                //element.AppendChild(StrataVariableNamesElement); 

                foreach (string strataColumn in crosstabParameters.StrataVariableNames)
                {
                    XmlElement strataElement = doc.CreateElement("strataVariable");
                    strataElement.InnerText = strataColumn.Replace("<", "&lt;");
                    StrataVariableNamesElement.AppendChild(strataElement);
                }

                element.AppendChild(StrataVariableNamesElement);
            }

            //if (YesValues.Count > 0 && NoValues.Count > 0)
            if (crosstabParameters.YesValues.Count > 0 && crosstabParameters.NoValues.Count > 0)
            {
                //xmlString = xmlString + "<valueRemappings>";
                XmlElement valueRemapElement = doc.CreateElement("valueRemappings");
                //xmlString = xmlString + "<yesValues>";
                XmlElement yesValuesElement = doc.CreateElement("yesValues");
                //foreach (string s in YesValues)
                foreach (string s in crosstabParameters.YesValues)
                {
                    //xmlString = xmlString + "<value>";
                    //xmlString = xmlString + s;
                    //xmlString = xmlString + "</value>";
                    XmlElement valueElement = doc.CreateElement("value");
                    valueElement.InnerText = s;
                    yesValuesElement.AppendChild(valueElement);
                }
                valueRemapElement.AppendChild(yesValuesElement);
                //xmlString = xmlString + "</yesValues>";
                //xmlString = xmlString + "<noValues>";
                XmlElement noValuesElement = doc.CreateElement("noValues");
                foreach (string s in crosstabParameters.NoValues)
                {
                    //xmlString = xmlString + "<value>";
                    //xmlString = xmlString + s;
                    //xmlString = xmlString + "</value>";
                    XmlElement valueElement = doc.CreateElement("value");
                    valueElement.InnerText = s;
                    noValuesElement.AppendChild(valueElement);
                }
                //xmlString = xmlString + "</noValues>";
                //xmlString = xmlString + "</valueRemappings>";
                valueRemapElement.AppendChild(noValuesElement);
                element.AppendChild(valueRemapElement);
            }

            //xmlString = xmlString + "<weightVariable>" + weightVar + "</weightVariable>" +
            XmlElement weightVarElement = doc.CreateElement("weightVar");
            if (!String.IsNullOrEmpty(crosstabParameters.WeightVariableName.ToString()))
            {
                weightVarElement.InnerText = crosstabParameters.WeightVariableName.ToString();
                element.AppendChild(weightVarElement);
            }

            //"<maxColumns>" + MaxColumns.ToString() + "</maxColumns>" +
            //"<maxColumnNameLength>" + MaxColumnLength.ToString() + "</maxColumnNameLength>" +
            XmlElement maxColumnNameLengthElement = doc.CreateElement("maxColumnNameLength");
            if (!String.IsNullOrEmpty(crosstabParameters.MaxColumnNameLength.ToString()))
            {
                maxColumnNameLengthElement.InnerText = crosstabParameters.MaxColumnNameLength.ToString();
                element.AppendChild(maxColumnNameLengthElement);
            }

            //"<allValues>" + allValues + "</allValues>" +
            XmlElement allValuesElement = doc.CreateElement("allValues");
            if (!String.IsNullOrEmpty(crosstabParameters.ShowAllListValues.ToString()))
            {
                allValuesElement.InnerText = crosstabParameters.ShowAllListValues.ToString();
                element.AppendChild(allValuesElement);
            }
            //--EI-83
            //<usepromptsforcolumnnames> + usepromptsforcolumnnames + "<usepromptsforcolumnames>
            XmlElement usePromptsElement = doc.CreateElement("usepromptsforcolumnnames");
            usePromptsElement.InnerText = crosstabParameters.UsePromptsForColumnNames.ToString();
            element.AppendChild(usePromptsElement);
            //--
            //"<showListLabels>" + checkboxCommentLegalLabels.IsChecked + "</showListLabels>" +
            XmlElement showListLabelsElement = doc.CreateElement("showListLabels");
            if (!String.IsNullOrEmpty(crosstabParameters.ShowCommentLegalLabels.ToString()))
            {
                showListLabelsElement.InnerText = crosstabParameters.ShowCommentLegalLabels.ToString();
                element.AppendChild(showListLabelsElement);
            }

            //"<showConfLimits>" + showConfLimits + "</showConfLimits>" +
            //"<showCumulativePercent>" + showCumulativePercent + "</showCumulativePercent>" +

            //"<includeMissing>" + includeMissing + "</includeMissing>" +
            XmlElement includeMissingElement = doc.CreateElement("includeMissing");
            if (!String.IsNullOrEmpty(crosstabParameters.IncludeMissing.ToString()))
            {
                includeMissingElement.InnerText = crosstabParameters.IncludeMissing.ToString();
                element.AppendChild(includeMissingElement);
            }

            //"<showStrataSummaryOnly>" + showStrataSummaryOnly + "</showStrataSummaryOnly>" +
            XmlElement showStrataSummaryOnlyElement = doc.CreateElement("showStrataSummaryOnly");
            if (!String.IsNullOrEmpty(crosstabParameters.StrataSummaryOnly.ToString()))
            {
                showStrataSummaryOnlyElement.InnerText = crosstabParameters.StrataSummaryOnly.ToString();
                element.AppendChild(showStrataSummaryOnlyElement);
            }
            //EI-146
            XmlElement displayChiSqElement = doc.CreateElement("displayChiSquare");
            if (!String.IsNullOrEmpty(crosstabParameters.DisplayChiSq.ToString()))
            {
                displayChiSqElement.InnerText = crosstabParameters.DisplayChiSq.ToString();
                element.AppendChild(displayChiSqElement);
            }

            //"<treatOutcomeAsContinuous>" + outcomeContinuous + "</treatOutcomeAsContinuous>" +
            XmlElement treatOutcomeAsContinuousElement = doc.CreateElement("treatOutcomeAsContinuous");
            if (!String.IsNullOrEmpty(crosstabParameters.TreatOutcomeAsContinuous.ToString()))
            {
                treatOutcomeAsContinuousElement.InnerText = crosstabParameters.TreatOutcomeAsContinuous.ToString();
                element.AppendChild(treatOutcomeAsContinuousElement);
            }

            XmlElement smart2x2Element = doc.CreateElement("smartTable");
            if (!String.IsNullOrEmpty(crosstabParameters.SmartTable.ToString()))
            {
                smart2x2Element.InnerText = crosstabParameters.SmartTable.ToString();
                element.AppendChild(smart2x2Element);
            }

            //"<customHeading>" + CustomOutputHeading.Replace("<", "&lt;") + "</customHeading>" +
            //"<customDescription>" + CustomOutputDescription.Replace("<", "&lt;") + "</customDescription>" +
            //"<customCaption>" + CustomOutputCaption + "</customCaption>" +

            //customHeading
            XmlElement customHeadingElement = doc.CreateElement("customHeading");
            customHeadingElement.InnerText = crosstabParameters.GadgetTitle; //CustomOutputHeading.Replace("<", "&lt;");
            element.AppendChild(customHeadingElement);

            //customDescription
            XmlElement customDescriptionElement = doc.CreateElement("customDescription");
            customDescriptionElement.InnerText = crosstabParameters.GadgetDescription; //CustomOutputDescription.Replace("<", "&lt;");
            element.AppendChild(customDescriptionElement);

            //customCaption
            XmlElement customCaptionElement = doc.CreateElement("customCaption");
            customCaptionElement.InnerText = CustomOutputCaption;
            element.AppendChild(customCaptionElement);

            //"<layoutMode>" + layoutMode + "</layoutMode>" +
            XmlElement layoutModeElement = doc.CreateElement("layoutMode");
            if (!String.IsNullOrEmpty(crosstabParameters.HorizontalDisplayMode.ToString()))
            {
                if (crosstabParameters.HorizontalDisplayMode == true)
                {
                    layoutModeElement.InnerText = "vertical";
                }
                else
                {
                    layoutModeElement.InnerText = "horizontal";
                }
                element.AppendChild(layoutModeElement);
            }

            //"<rowColPercents>" + rowColPercents + "</rowColPercents>" +
            XmlElement rowColPercentsElement = doc.CreateElement("rowColPercents");
            if (!String.IsNullOrEmpty(crosstabParameters.ShowPercents.ToString()))
            {
                rowColPercentsElement.InnerText = crosstabParameters.ShowPercents.ToString();
                element.AppendChild(rowColPercentsElement);
            }

            //"<showHeatMap>" + checkboxConditionalShading.IsChecked + "</showHeatMap>" +
            XmlElement showHeatMapElement = doc.CreateElement("showHeatMap");
            if (!String.IsNullOrEmpty(crosstabParameters.ConditionalShading.ToString()))
            {
                showHeatMapElement.InnerText = crosstabParameters.ConditionalShading.ToString();
                element.AppendChild(showHeatMapElement);
            }

            //"<breakType>" + cmbBreakType.SelectedIndex + "</breakType>" +
            XmlElement breakTypeElement = doc.CreateElement("breakType");
            if (!String.IsNullOrEmpty(crosstabParameters.BreakType.ToString()))
            {
                breakTypeElement.InnerText = crosstabParameters.BreakType.ToString();
                element.AppendChild(breakTypeElement);
            }

            //"<break1>" + txtPct1.Text + "</break1>" +
            //"<break2>" + txtPct2.Text + "</break2>" +
            //"<break3>" + txtPct3.Text + "</break3>" +
            //"<break4>" + txtPct4.Text + "</break4>" +
            //"<break5>" + txtPct5.Text + "</break5>";

            XmlElement break1Element = doc.CreateElement("break1");
            if (!String.IsNullOrEmpty(crosstabParameters.Break1.ToString()))
            {
                break1Element.InnerText = crosstabParameters.Break1.ToString();
                element.AppendChild(break1Element);
            }

            XmlElement break2Element = doc.CreateElement("break2");
            if (!String.IsNullOrEmpty(crosstabParameters.Break2.ToString()))
            {
                break2Element.InnerText = crosstabParameters.Break2.ToString();
                element.AppendChild(break2Element);
            }

            XmlElement break3Element = doc.CreateElement("break3");
            if (!String.IsNullOrEmpty(crosstabParameters.Break3.ToString()))
            {
                break3Element.InnerText = crosstabParameters.Break3.ToString();
                element.AppendChild(break3Element);
            }

            XmlElement break4Element = doc.CreateElement("break4");
            if (!String.IsNullOrEmpty(crosstabParameters.Break4.ToString()))
            {
                break4Element.InnerText = crosstabParameters.Break4.ToString();
                element.AppendChild(break4Element);
            }

            XmlElement break5Element = doc.CreateElement("break5");
            if (!String.IsNullOrEmpty(crosstabParameters.Break5.ToString()))
            {
                break5Element.InnerText = crosstabParameters.Break5.ToString();
                element.AppendChild(break5Element);
            }

            //xmlString += "<startColor>";
            //xmlString += "<red>";
            //xmlString += ((SolidColorBrush)rctLowColor.Fill).Color.R;
            //xmlString += "</red>";
            XmlElement startColorElement = doc.CreateElement("startColor");
            XmlElement startColorRedElement = doc.CreateElement("red");
            startColorRedElement.InnerText = crosstabParameters.LoColorFill.Color.R.ToString();
            startColorElement.AppendChild(startColorRedElement);

            XmlElement startColorGreenElement = doc.CreateElement("green");
            startColorGreenElement.InnerText = crosstabParameters.LoColorFill.Color.G.ToString();
            startColorElement.AppendChild(startColorGreenElement);

            XmlElement startColorBlueElement = doc.CreateElement("blue");
            startColorBlueElement.InnerText = crosstabParameters.LoColorFill.Color.B.ToString();
            startColorElement.AppendChild(startColorBlueElement);
            element.AppendChild(startColorElement);

            XmlElement endColorElement = doc.CreateElement("endColor");
            XmlElement endColorRedElement = doc.CreateElement("red");
            endColorRedElement.InnerText = crosstabParameters.HiColorFill.Color.R.ToString();
            endColorElement.AppendChild(endColorRedElement);

            XmlElement endColorGreenElement = doc.CreateElement("green");
            endColorGreenElement.InnerText = crosstabParameters.HiColorFill.Color.G.ToString();
            endColorElement.AppendChild(endColorGreenElement);

            XmlElement endColorBlueElement = doc.CreateElement("blue");
            endColorBlueElement.InnerText = crosstabParameters.HiColorFill.Color.B.ToString();
            endColorElement.AppendChild(endColorBlueElement);
            element.AppendChild(endColorElement);

            XmlElement OutcomeSwap = doc.CreateElement("OutcomeSwap");
            if (!String.IsNullOrEmpty(crosstabParameters.OutcomeSwap.ToString()))
            {
                OutcomeSwap.InnerText = crosstabParameters.OutcomeSwap.ToString();
                element.AppendChild(OutcomeSwap);
            }

            XmlElement ExposureSwap = doc.CreateElement("ExposureSwap");
            if (!String.IsNullOrEmpty(crosstabParameters.ExposureSwap.ToString()))
            {
                ExposureSwap.InnerText = crosstabParameters.ExposureSwap.ToString();
                element.AppendChild(ExposureSwap);
            }

            //xmlString = xmlString + SerializeAnchors();
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
            this.ColumnWarningShown = true;
            this.Parameters = new CrosstabParameters();

            HideConfigPanel();

            infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            byte red = 0;
            byte green = 0;
            byte blue = 0;

            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLowerInvariant())
                {
                    case "mainvariable":
                    case "exposurevariable": // added to work with the old 2x2 gadget
                        //cbxExposureField.Text = child.InnerText.Replace("&lt;", "<");
                        ((CrosstabParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                        break;
                    case "crosstabvariable":
                    case "outcomevariable": // added to work with the old 2x2 gadget
                        //cbxOutcomeField.Text = child.InnerText.Replace("&lt;", "<");
                        ((CrosstabParameters)Parameters).CrosstabVariableName = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "stratavariable":
                        //cbxFieldStrata.Text = child.InnerText.Replace("&lt;", "<");
                        //if (!string.IsNullOrEmpty(child.InnerText))
                        //{
                        //    lbxFieldStrata.SelectedItems.Add(child.InnerText.Replace("&lt;", "<"));
                        //}
                        if (!string.IsNullOrEmpty(child.InnerText))
                        {
                            if (((CrosstabParameters)Parameters).StrataVariableNames.Count == 0)
                            {
                                ((CrosstabParameters)Parameters).StrataVariableNames.Add(child.InnerText.Replace("&lt;", "<"));
                            }
                            else
                            {
                                ((CrosstabParameters)Parameters).StrataVariableNames[0] = child.InnerText.Replace("&lt;", "<");
                            }
                        }
                        break;
                    //case "stratavariables":
                    //    foreach (XmlElement field in child.ChildNodes)
                    //    {
                    //        List<string> fields = new List<string>();
                    //        if (field.Name.ToLowerInvariant().Equals("stratavariable"))
                    //        {
                    //            lbxFieldStrata.SelectedItems.Add(field.InnerText.Replace("&lt;", "<"));
                    //        }
                    //    }
                    //    break;
                    case "stratavariables":
                        foreach (XmlElement field in child.ChildNodes)
                        {
                            List<string> fields = new List<string>();
                            if (field.Name.ToLowerInvariant().Equals("stratavariable"))
                            {
                                ((CrosstabParameters)Parameters).StrataVariableNames.Add(field.InnerText.Replace("&lt;", "<"));
                            }
                        }
                        break;
                    case "weightvariable":
                        //cbxFieldWeight.Text = child.InnerText.Replace("&lt;", "<");
                        ((CrosstabParameters)Parameters).WeightVariableName = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "weightvar":
                        //cbxFieldWeight.Text = child.InnerText.Replace("&lt;", "<");
                        ((CrosstabParameters)Parameters).WeightVariableName = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "maxcolumnnamelength":
                        int maxColumnLength = 24;
                        int.TryParse(child.InnerText, out maxColumnLength);
                        //txtMaxColumnLength.Text = maxColumnLength.ToString();
                        ((CrosstabParameters)Parameters).MaxColumnNameLength = maxColumnLength.ToString();
                        break;
                    case "allvalues":
                        if (child.InnerText.ToLowerInvariant().Equals("true"))
                        {
                            //checkboxAllValues.IsChecked = true;
                            ((CrosstabParameters)Parameters).ShowAllListValues = true;
                        }
                        else
                        {
                            //checkboxAllValues.IsChecked = false;
                            ((CrosstabParameters)Parameters).ShowAllListValues = false;
                        }
                        break;
                    case "showlistlabels":
                        if (child.InnerText.ToLowerInvariant().Equals("true"))
                        {
                            //checkboxCommentLegalLabels.IsChecked = true;
                            ((CrosstabParameters)Parameters).ShowCommentLegalLabels = true;
                        }
                        else
                        {
                            //checkboxCommentLegalLabels.IsChecked = false;
                            ((CrosstabParameters)Parameters).ShowCommentLegalLabels = false;
                        }
                        break;
                    //---Ei-83
                    case "usepromptsforcolumnnames":
                        {
                            bool usePrompts = false;
                            bool.TryParse(child.InnerText, out usePrompts);
                            ((CrosstabParameters)Parameters).UsePromptsForColumnNames = usePrompts;
                        }
                        break;
                    //--
                    case "includemissing":
                        if (child.InnerText.ToLowerInvariant().Equals("true"))
                        {
                            //checkboxIncludeMissing.IsChecked = true;
                            ((CrosstabParameters)Parameters).IncludeMissing = true;
                        }
                        else
                        {
                            //checkboxIncludeMissing.IsChecked = false;
                            ((CrosstabParameters)Parameters).IncludeMissing = false;
                        }
                        break;
                    case "treatoutcomeascontinuous":
                        if (child.InnerText.ToLowerInvariant().Equals("true"))
                        {
                            //checkboxOutcomeContinuous.IsCheckead = true;
                            ((CrosstabParameters)Parameters).TreatOutcomeAsContinuous = true;
                        }
                        else
                        {
                            //checkboxOutcomeContinuous.IsChecked = false;
                            ((CrosstabParameters)Parameters).TreatOutcomeAsContinuous = false;
                        }
                        break;
                    case "smarttable":
                        if (child.InnerText.ToLowerInvariant().Equals("true"))
                        {
                            ((CrosstabParameters)Parameters).SmartTable = true;
                        }
                        else
                        {
                            ((CrosstabParameters)Parameters).SmartTable = false;
                        }
                        break;
                    case "showstratasummaryonly":
                        if (child.InnerText.ToLowerInvariant().Equals("true"))
                        {
                            //checkboxStrataSummaryOnly.IsChecked = true;
                            ((CrosstabParameters)Parameters).StrataSummaryOnly = true;
                        }
                        else
                        {
                            //checkboxStrataSummaryOnly.IsChecked = false;
                            ((CrosstabParameters)Parameters).StrataSummaryOnly = false;
                        }
                        break;
                    //EI-146
                    case "displaychisquare":
                        if (child.InnerText.ToLowerInvariant().Equals("true"))
                        {
                            ((CrosstabParameters)Parameters).DisplayChiSq = true;
                        }
                        else
                        {
                            ((CrosstabParameters)Parameters).DisplayChiSq = false;
                        }
                        break;
                    case "rowcolpercents":
                        if (child.InnerText.ToLowerInvariant().Equals("true"))
                        {
                            //checkboxRowColPercents.IsChecked = true;
                            ((CrosstabParameters)Parameters).ShowPercents = true;
                        }
                        else
                        {
                            //checkboxRowColPercents.IsChecked = false;
                            ((CrosstabParameters)Parameters).ShowPercents = false;
                        }
                        break;
                    case "customheading":
                        if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
                        {
                            //this.CustomOutputHeading = child.InnerText.Replace("&lt;", "<");
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
                    case "datafilters":
                        this.DataFilters = new DataFilters(this.DashboardHelper);
                        this.DataFilters.CreateFromXml(child);
                        break;
                    case "layoutmode":
                        if (child.InnerText.ToLowerInvariant().Equals("horizontal"))
                        {
                            //checkboxHorizontal.IsChecked = false;
                            ((CrosstabParameters)Parameters).HorizontalDisplayMode = false;
                        }
                        else
                        {
                            //checkboxHorizontal.IsChecked = true;
                            ((CrosstabParameters)Parameters).HorizontalDisplayMode = true;
                        }
                        break;
                    case "showheatmap":
                        if (child.InnerText.ToLowerInvariant().Equals("true"))
                        {
                            //checkboxConditionalShading.IsChecked = true;
                            ((CrosstabParameters)Parameters).ConditionalShading = true;
                        }
                        else
                        {
                            //checkboxConditionalShading.IsChecked = false;
                            ((CrosstabParameters)Parameters).ConditionalShading = false;
                        }
                        break;
                    case "breaktype":
                        //cmbBreakType.SelectedIndex = int.Parse(child.InnerText);
                        ((CrosstabParameters)Parameters).BreakType = int.Parse(child.InnerText);
                        break;
                    case "break1":
                        //txtPct1.Text = child.InnerText;
                        ((CrosstabParameters)Parameters).Break1 = child.InnerText;
                        break;
                    case "break2":
                        //txtPct2.Text = child.InnerText;
                        ((CrosstabParameters)Parameters).Break2 = child.InnerText;
                        break;
                    case "break3":
                        //txtPct3.Text = child.InnerText;
                        ((CrosstabParameters)Parameters).Break3 = child.InnerText;
                        break;
                    case "break4":
                        //txtPct4.Text = child.InnerText;
                        ((CrosstabParameters)Parameters).Break4 = child.InnerText;
                        break;
                    case "break5":
                        //txtPct5.Text = child.InnerText;
                        ((CrosstabParameters)Parameters).Break5 = child.InnerText;
                        break;
                    case "startcolor":
                        foreach (XmlNode tdNode in child.ChildNodes)
                        {
                            switch (tdNode.Name)
                            {
                                case "red":
                                    red = byte.Parse(tdNode.InnerText);
                                    break;
                                case "green":
                                    green = byte.Parse(tdNode.InnerText);
                                    break;
                                case "blue":
                                    blue = byte.Parse(tdNode.InnerText);
                                    break;
                            }
                        }
                        ((CrosstabParameters)Parameters).LoColorFill = new SolidColorBrush(Color.FromRgb(red, green, blue));
                        break;
                    case "endcolor":
                        foreach (XmlNode tdNode in child.ChildNodes)
                        {
                            switch (tdNode.Name)
                            {
                                case "red":
                                    red = byte.Parse(tdNode.InnerText);
                                    break;
                                case "green":
                                    green = byte.Parse(tdNode.InnerText);
                                    break;
                                case "blue":
                                    blue = byte.Parse(tdNode.InnerText);
                                    break;
                            }
                        }
                        ((CrosstabParameters)Parameters).HiColorFill = new SolidColorBrush(Color.FromRgb(red, green, blue));
                        //MOVE TO CONTROL
                        //SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(red, green, blue));
                        //if (child.Name.ToLowerInvariant() == "startcolor")
                        //{
                        //    rctLowColor.Fill = brush;
                        //}
                        //else if (child.Name.ToLowerInvariant() == "endcolor")
                        //{
                        //    rctHighColor.Fill = brush;
                        //}
                        break;
                    case "outcomeswap":
                        if (child.InnerText.ToLowerInvariant().Equals("true"))
                        {
                            //checkboxConditionalShading.IsChecked = true;
                            ((CrosstabParameters)Parameters).OutcomeSwap = true;
                        }
                        else
                        {
                            //checkboxConditionalShading.IsChecked = false;
                            ((CrosstabParameters)Parameters).OutcomeSwap = false;
                        }
                        break;
                    case "exposureswap":
                        if (child.InnerText.ToLowerInvariant().Equals("true"))
                        {
                            //checkboxConditionalShading.IsChecked = true;
                            ((CrosstabParameters)Parameters).ExposureSwap = true;
                        }
                        else
                        {
                            //checkboxConditionalShading.IsChecked = false;
                            ((CrosstabParameters)Parameters).ExposureSwap = false;
                        }
                        break;
                    case "valueremappings":
                        foreach (XmlNode node in child.ChildNodes)
                        {
                            switch (node.Name.ToLowerInvariant())
                            {
                                case "yesvalues":
                                    foreach (XmlNode valueNode in node.ChildNodes)
                                    {
                                        //YesValues.Add(valueNode.InnerText);
                                        ((CrosstabParameters)Parameters).YesValues.Add(valueNode.InnerText);
                                    }
                                    break;
                                case "novalues":
                                    foreach (XmlNode valueNode in node.ChildNodes)
                                    {
                                        //NoValues.Add(valueNode.InnerText);
                                        ((CrosstabParameters)Parameters).NoValues.Add(valueNode.InnerText);
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }

            base.CreateFromXml(element);

            this.LoadingCombos = false;

            //CheckVariables();
            this.YesValues = ((CrosstabParameters)Parameters).YesValues;
            this.NoValues = ((CrosstabParameters)Parameters).NoValues;
            RefreshResults();
            //HideConfigPanel();
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
                CrosstabParameters crosstabParameters = (CrosstabParameters)Parameters;
                int maxColumnLength = 24;
                //bool success = int.TryParse(txtMaxColumnLength.Text, out maxColumnLength);
                bool success = int.TryParse(crosstabParameters.MaxColumnNameLength, out maxColumnLength);
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
            //this.cbxExposureField.IsEnabled = false;
            //this.cbxOutcomeField.IsEnabled = false;
            //this.panelAdvanced.IsEnabled = false;
            //this.panelAdvanced2.IsEnabled = false;

            //this.panelHeatMap1.IsEnabled = false;
            //this.panelHeatMap2.IsEnabled = false;

            //this.panelDisplay1.IsEnabled = false;
            //this.panelDisplay2.IsEnabled = false;

            //this.btnRun.IsEnabled = false;
        }

        /// <summary>
        /// Sets the gadget to its 'finished' state
        /// </summary>
        public override void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
            //this.cbxExposureField.IsEnabled = true;
            //this.cbxOutcomeField.IsEnabled = true;

            //this.panelAdvanced.IsEnabled = true;
            //this.panelAdvanced2.IsEnabled = true;

            //this.panelHeatMap1.IsEnabled = true;
            //this.panelHeatMap2.IsEnabled = true;

            //this.panelDisplay1.IsEnabled = true;
            //this.panelDisplay2.IsEnabled = true;

            //this.btnRun.IsEnabled = true;

            //CheckVariables();

            base.SetGadgetToFinishedState();
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0, bool useAlternatingColors = false, bool ForWeb = false)
        {
             
            if (IsCollapsed) return string.Empty;
            CrosstabParameters crosstabParameters = (CrosstabParameters)Parameters;
            if (crosstabParameters.ColumnNames.Count < 1) return string.Empty;

            StringWriter stringWriter = new StringWriter();
            //using (HTMTextWriter writer = new HTMTextWriter(stringWriter))

            using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
            {

                //StringBuilder htmlBuilder = new StringBuilder();

                //CustomOutputHeading = headerPanel.Text;
                //CustomOutputDescription = descriptionPanel.Text;
                CustomOutputHeading = headerPanel.Text;
                CustomOutputDescription = descriptionPanel.Text;
                if (ForWeb) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "row col-lg-12");
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                    //writer.AddAttribute(HtmlTextWriterAttribute.Class, "col-md-4");
                    //writer.RenderBeginTag(HtmlTextWriterTag.Div);
                }
                if (CustomOutputHeading == null || (string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)")))
                {
                    //htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">Crosstabulation</h2>");
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "gadgetHeading");
                    writer.RenderBeginTag(HtmlTextWriterTag.H2);   // Begin #1
                    writer.Write("Crosstabulation");
                    writer.RenderEndTag(); // End #1
                }
                else if (CustomOutputHeading != "(none)")
                {
                    //htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">" + CustomOutputHeading + "</h2>");
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "gadgetHeading");
                    writer.RenderBeginTag(HtmlTextWriterTag.H2);   // Begin #1
                    writer.Write(CustomOutputHeading);
                    writer.RenderEndTag(); // End #1
                }
                if (!ForWeb)
                {
                    //htmlBuilder.AppendLine("<p class=\"gadgetOptions\"><small>");
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "gadgetOptions");
                    writer.RenderBeginTag(HtmlTextWriterTag.P);  //Begin (B) #1
                    writer.RenderBeginTag(HtmlTextWriterTag.Small);  //B 2

                    //htmlBuilder.AppendLine("<em>Main variable:</em> <strong>" + cbxExposureField.Text + "</strong>");
                    writer.RenderBeginTag(HtmlTextWriterTag.Em);  //B 3
                    writer.Write("Main variable:");
                    writer.RenderEndTag();  //E 3
                    writer.RenderBeginTag(HtmlTextWriterTag.Strong); //B 3
                    writer.Write(crosstabParameters.ColumnNames[0].ToString());
                    writer.RenderEndTag();  //E 3

                    //htmlBuilder.AppendLine("<br />");
                    writer.WriteBreak();

                    //htmlBuilder.AppendLine("<em>Crosstab variable:</em> <strong>" + cbxOutcomeField.Text + "</strong>");
                    //htmlBuilder.AppendLine("<br />");
                    writer.RenderBeginTag(HtmlTextWriterTag.Em);  //B 3
                    writer.Write("Main variable:");
                    writer.RenderEndTag();  //E 3
                    writer.RenderBeginTag(HtmlTextWriterTag.Strong); //B 3
                    writer.Write(crosstabParameters.CrosstabVariableName.ToString());
                    writer.RenderEndTag();  //E 3
                    writer.WriteBreak();

                    //if (cbxFieldWeight.SelectedIndex >= 0)
                    if (!String.IsNullOrEmpty(crosstabParameters.WeightVariableName))
                    {
                        //htmlBuilder.AppendLine("<em>Weight variable:</em> <strong>" + cbxFieldWeight.Text + "</strong>");
                        //htmlBuilder.AppendLine("<br />");
                        writer.RenderBeginTag(HtmlTextWriterTag.Em);  //B 3
                        writer.Write("Weight variable:");
                        writer.RenderEndTag();  //E 3
                        writer.RenderBeginTag(HtmlTextWriterTag.Strong); //B 3
                        writer.Write(crosstabParameters.WeightVariableName.ToString());
                        writer.RenderEndTag();  //E 3
                        writer.WriteBreak();
                    }

                    //if (lbxFieldStrata.SelectedItems.Count > 0)
                    if (crosstabParameters.StrataVariableNames.Count > 0)
                    {
                        WordBuilder wb = new WordBuilder(", ");
                        foreach (string s in crosstabParameters.StrataVariableNames)
                        {
                            wb.Add(s);
                        }
                        //htmlBuilder.AppendLine("<em>Strata variable(s):</em> <strong>" + wb.ToString() + "</strong>");
                        //htmlBuilder.AppendLine("<br />");
                        writer.RenderBeginTag(HtmlTextWriterTag.Em);  //B 3
                        writer.Write("Strata variable(s):");
                        writer.RenderEndTag();  //E 3
                        writer.RenderBeginTag(HtmlTextWriterTag.Strong); //B 3
                        writer.Write(wb.ToString());
                        writer.RenderEndTag();  //E 3
                        writer.WriteBreak();
                    }

                    //htmlBuilder.AppendLine("<em>Include missing:</em> <strong>" + checkboxIncludeMissing.IsChecked.ToString() + "</strong>");
                    //htmlBuilder.AppendLine("<br />");
                    //htmlBuilder.AppendLine("</small></p>");
                    writer.RenderBeginTag(HtmlTextWriterTag.Em);  //B 3
                    writer.Write("Include missing:");
                    writer.RenderEndTag();  //E 3
                    writer.RenderBeginTag(HtmlTextWriterTag.Strong); //B 3
                    writer.Write(crosstabParameters.IncludeMissing.ToString());
                    writer.RenderEndTag();  //E 3
                    writer.WriteBreak();
                    writer.RenderEndTag();  //E 2
                    writer.RenderEndTag();  //E 1
                }
                if (!string.IsNullOrEmpty(CustomOutputDescription))
                {
                    if (!ForWeb) {
                        //htmlBuilder.AppendLine("<p class=\"gadgetsummary\">" + CustomOutputDescription + "</p>");
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "gadgetsummary");
                        writer.RenderBeginTag(HtmlTextWriterTag.P);  // B1
                        writer.Write(CustomOutputDescription);
                        writer.RenderEndTag();  //E1
                    }
                    else {
                        //<div class="row col-lg-12" style="color:gray">
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "row col-lg-12");
                        writer.RenderBeginTag(HtmlTextWriterTag.Div);
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "gadgetsummary");
                        writer.RenderBeginTag(HtmlTextWriterTag.P);  // B1
                        writer.Write(CustomOutputDescription);
                        writer.RenderEndTag();  //E1
                        writer.RenderEndTag();

                    }
                }

                if (!string.IsNullOrEmpty(messagePanel.Text) && messagePanel.Visibility == Visibility.Visible)
                {
                    //htmlBuilder.AppendLine("<p><small><strong>" + messagePanel.Text + "</strong></small></p>");
                    writer.RenderBeginTag(HtmlTextWriterTag.P); // B1
                    writer.RenderBeginTag(HtmlTextWriterTag.Small);  //B2
                    writer.RenderBeginTag(HtmlTextWriterTag.Strong);  //B3
                    writer.Write(messagePanel.Text);
                    writer.RenderEndTag();  //E3
                    writer.RenderEndTag();  //E2
                    writer.RenderEndTag(); // E1
                }

                if (!string.IsNullOrEmpty(infoPanel.Text) && infoPanel.Visibility == Visibility.Visible)
                {
                    //htmlBuilder.AppendLine("<p><small><strong>" + infoPanel.Text + "</strong></small></p>");
                    writer.RenderBeginTag(HtmlTextWriterTag.P); // B1
                    writer.RenderBeginTag(HtmlTextWriterTag.Small);  //B2
                    writer.RenderBeginTag(HtmlTextWriterTag.Strong);  //B3
                    writer.Write(infoPanel.Text);
                    writer.RenderEndTag();  //E3
                    writer.RenderEndTag();  //E2
                    writer.RenderEndTag(); // E1
                }
                if (ForWeb)
                {
                    
                   // writer.RenderEndTag();  //col-xs-8 
                    //writer.AddAttribute(HtmlTextWriterAttribute.Class, "col-md-8 CenterText");
                    //writer.RenderBeginTag(HtmlTextWriterTag.Div);

                    //writer.WriteBreak();
                    //writer.WriteBreak();
                    //writer.WriteBreak();
                    //writer.Write("<hr>");
                    //writer.RenderBeginTag(HtmlTextWriterTag.Strong);  //B 3
                    //writer.Write("Single Table Analysis");
                    //writer.Write("<hr>");
                    //writer.RenderEndTag();  //E 3


                   /* writer.RenderEndTag();*/ // col-xs-4
                   
                    writer.RenderEndTag();  //row 


                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "row col-lg-12");
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                }
                foreach (Grid grid in this.StrataGridList)
                {
                    string gridName = grid.Tag.ToString();
                   
                    //htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                    if (!string.IsNullOrEmpty(gridName))
                    {
                        //htmlBuilder.AppendLine("<h3>" + gridName + "</h3>");
                        //rotate-text
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "col-sm-1");
                        writer.RenderBeginTag(HtmlTextWriterTag.Div);
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "rotate-text");
                        writer.RenderBeginTag(HtmlTextWriterTag.H3);
                        writer.Write(gridName);
                        writer.RenderEndTag();
                        writer.RenderEndTag();

                    }
                    if (!ForWeb)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Style, "height: 7px;");


                        writer.RenderBeginTag(HtmlTextWriterTag.Div);
                        writer.RenderEndTag();
                    }

                    //htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
                    //                    writer.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "col-lg-12");
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "CenterText");
                    writer.AddAttribute(HtmlTextWriterAttribute.Style, "font-weight: bold;");
                    writer.RenderBeginTag(HtmlTextWriterTag.H6);
                    writer.Write(crosstabParameters.CrosstabVariableName.ToString());
                    writer.RenderEndTag();
                    writer.RenderEndTag();


                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "col-sm-11");
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                    writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                    writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
                    writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                    writer.RenderBeginTag(HtmlTextWriterTag.Table);

                    //htmlBuilder.AppendLine("<caption>" + cbxExposureField.Text + " * " + cbxOutcomeField.Text + "</caption>");
                    //writer.RenderBeginTag(HtmlTextWriterTag.Caption);
                    //writer.Write(crosstabParameters.ColumnNames[0].ToString() + " * " + crosstabParameters.CrosstabVariableName.ToString());
                    //writer.RenderEndTag();

                    SolidColorBrush backColor = Brushes.White;

                    foreach (UIElement control in grid.Children)
                    {
                         
                        string value = string.Empty;
                        int rowNumber = -1;
                        int columnNumber = -1;

                        if (control is TextBlock || control is StackPanel || control is Rectangle)
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
                                int numchildren = (control as StackPanel).Children.Count;
                                value = (((control as StackPanel).Children[0]) as TextBlock).Text;
                                for (int i = 1; i < numchildren; i++)
                                {
                                    value = value + "<br>" + (((control as StackPanel).Children[i]) as TextBlock).Text;
                                }
                                StackPanel panel = control as StackPanel;
                                foreach (UIElement element in panel.Children)
                                {
                                    if (element is Rectangle)
                                    {

                                    }
                                }
                            }
                            else if (control is Rectangle)
                            {
                                Rectangle rectangle = control as Rectangle;
                                backColor = rectangle.Fill as SolidColorBrush;
                                continue;
                            }

                            string tableDataTagOpen = "<td>";
                            string tableDataTagClose = "</td>";

                            if (columnNumber == 0)
                            {
                                //htmlBuilder.AppendLine("<tr>");
                                //writer.AddAttribute(HtmlTextWriterAttribute.Class, "TwoByTwoTr1 CenterText");
                                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                            }

                            //tableDataTagOpen
                            if (rowNumber == 0 && columnNumber < grid.ColumnDefinitions.Count - 1)
                          
                            {
                                tableDataTagOpen = "<th>";
                                tableDataTagClose = "</th>";
                                if (columnNumber == 0)
                                {
                                    writer.AddAttribute(HtmlTextWriterAttribute.Class,"NoBorders");
                                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                                    writer.Write("");
                                    writer.RenderEndTag();
                                }
                                else {
                                    writer.RenderBeginTag(HtmlTextWriterTag.Th);
                                    writer.Write(value);
                                    writer.RenderEndTag();
                                }
                            }
                            else if (rowNumber == 0 && columnNumber >= grid.ColumnDefinitions.Count - 1)
                            {
                                writer.RenderBeginTag(HtmlTextWriterTag.Th);
                            }
                            else if (rowNumber > 0 && rowNumber < grid.RowDefinitions.Count - 1 && columnNumber == 0)
                            {
                                //tableDataTagOpen = "<td class=\"value\">";
                                writer.AddAttribute(HtmlTextWriterAttribute.Class, "value");
                                if (columnNumber == 0) {
                                    writer.RenderBeginTag(HtmlTextWriterTag.Th);
                                }
                                else {
                                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                                }

                                writer.Write(WebUtility.HtmlEncode(value));
                                writer.RenderEndTag();
                            }
                            else if (rowNumber > 0 && rowNumber < grid.RowDefinitions.Count - 1 && columnNumber < grid.ColumnDefinitions.Count - 1)
                            {
                                //tableDataTagOpen = "<td style=\"background-color: rgb(" + backColor.Color.R.ToString() + ", " + backColor.Color.G.ToString() + ", " + backColor.Color.B.ToString() + ");\">";
                                writer.AddAttribute(HtmlTextWriterAttribute.Style, "background-color: rgb(" + backColor.Color.R.ToString() + ", " + backColor.Color.G.ToString() + ", " + backColor.Color.B.ToString() + ");");
                                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                                writer.Write(value);
                                writer.RenderEndTag();
                            }
                            else if (rowNumber > 0 && rowNumber < grid.RowDefinitions.Count - 1 && columnNumber >= grid.ColumnDefinitions.Count - 1)
                            {
                                //tableDataTagOpen = "<td style=\"background-color: rgb(" + backColor.Color.R.ToString() + ", " + backColor.Color.G.ToString() + ", " + backColor.Color.B.ToString() + ");\">";
                                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                            }
                            else if (rowNumber >= grid.RowDefinitions.Count - 1)
                            {
                                //tableDataTagOpen = "<td style=\"background-color: rgb(" + backColor.Color.R.ToString() + ", " + backColor.Color.G.ToString() + ", " + backColor.Color.B.ToString() + ");\">";
                                if (value.ToLower() == "total")
                                {
                                    writer.RenderBeginTag(HtmlTextWriterTag.Th);
                                }
                                else {

                                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                                }
                            }

                            string formattedValue = value;

                            if ((rowNumber == grid.RowDefinitions.Count - 1) || (columnNumber == grid.ColumnDefinitions.Count - 1))
                            {
                                //formattedValue = "<span class=\"total\">" + value + "</span>";
                                writer.AddAttribute(HtmlTextWriterAttribute.Class, "total");
                                writer.RenderBeginTag(HtmlTextWriterTag.Span);
                                writer.Write(value);
                                try
                                {
                                    writer.RenderEndTag();
                                    writer.RenderEndTag();
                                }
                                catch (Exception ex)
                                {
                                    Exception eexx = ex;
                                }
                            }

                            //htmlBuilder.AppendLine(tableDataTagOpen + formattedValue + tableDataTagClose);
//                            writer.RenderEndTag();

                            if (columnNumber >= grid.ColumnDefinitions.Count - 1)
                            {
                                //htmlBuilder.AppendLine("</tr>");
                                try
                                {
                                    writer.RenderEndTag();
                                }
                                catch (Exception ex)
                                {
                                    Exception eexx = ex;
                                }
                            }
                        }
                    }

                    //htmlBuilder.AppendLine("</table>");
                    try
                    {
                        writer.RenderEndTag();
                        writer.RenderEndTag();// Div outer table
                    }
                    catch (Exception ex)
                    {
                        Exception eexx = ex;
                    }

                    // Chi Square

                    Grid chiSquareGrid = GetStrataChiSquareGrid(grid.Tag.ToString());
                    //htmlBuilder.AppendLine("<p></p>");
                    //htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");
                    writer.AddAttribute(HtmlTextWriterAttribute.Style, "clear: both");
                    writer.RenderBeginTag(HtmlTextWriterTag.P);
                    // </p>
                    writer.RenderEndTag();
                    //writer.RenderBeginTag(HtmlTextWriterTag.Br);
                    //writer.RenderBeginTag(HtmlTextWriterTag.Br);
                    //writer.RenderBeginTag(HtmlTextWriterTag.Br);
                    
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "row col-md-9 offset-md-2 ");
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                    
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "ChiSquareTable ");
                    writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                    writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
                    writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                    writer.RenderBeginTag(HtmlTextWriterTag.Table);

                    foreach (UIElement control in chiSquareGrid.Children)
                    {
                         
                        if (control is TextBlock)
                        {
                            int rowNumber = Grid.GetRow(control);
                            int columnNumber = Grid.GetColumn(control);

                            string tableDataTagOpen = "<td>";
                            string tableDataTagClose = "</td>";

                            if (columnNumber == 0)
                            {
                                //htmlBuilder.AppendLine("<tr>");
                               
                                writer.AddAttribute(HtmlTextWriterAttribute.Class, "ChiSquare");
                                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                            }

                            if (rowNumber == 0)
                            {
                                //tableDataTagOpen = "<th>";
                                //tableDataTagClose = "</th>";
                                writer.RenderBeginTag(HtmlTextWriterTag.Th);
                            }
                            else
                            {
                                writer.AddAttribute(HtmlTextWriterAttribute.Class, "CenterText");
                                if (((TextBlock)control).Text == "Fisher's Exact")
                                { 
                                writer.RenderBeginTag(HtmlTextWriterTag.Th);
                                }
                                else {
                                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                                }
                            }

                            string value = ((TextBlock)control).Text;
                            string formattedValue = value;

                            //htmlBuilder.AppendLine(tableDataTagOpen + formattedValue + tableDataTagClose);
                            writer.Write(formattedValue);
                            writer.RenderEndTag();

                            if (columnNumber >= chiSquareGrid.ColumnDefinitions.Count - 1)
                            {
                                //htmlBuilder.AppendLine("</tr>");
                                writer.RenderEndTag();
                            }
                        }
                    }
                    
                    //htmlBuilder.AppendLine("</table>");
                    try
                    {
                        writer.RenderEndTag();
                        writer.RenderEndTag(); // outer Table Div
                    }
                    catch (Exception ex)
                    {
                        Exception eexx = ex;
                    }


                    string disclaimer = GetStrataChiSquareDisclaimer(grid.Tag.ToString()).Text;
                    if (!string.IsNullOrEmpty(disclaimer))
                    {
                        //htmlBuilder.AppendLine("<p></p>");
                        //htmlBuilder.AppendLine("<p>" + disclaimer.Replace("<", "&lt;") + "</p>");
                        writer.RenderBeginTag(HtmlTextWriterTag.P);
                        writer.RenderEndTag();
                        writer.RenderBeginTag(HtmlTextWriterTag.P);
                        writer.Write(disclaimer);
                        writer.RenderEndTag();
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
                        //htmlBuilder.AppendLine("<h3>" + gridName + "</h3>");
                        writer.RenderBeginTag(HtmlTextWriterTag.H3);
                        writer.Write(gridName);
                        writer.RenderEndTag();

                    }
                    //htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                    if (!ForWeb) {
                        writer.AddAttribute(HtmlTextWriterAttribute.Style, "height: 7px;");
                        writer.RenderBeginTag(HtmlTextWriterTag.Div);
                        writer.RenderEndTag();
                    }
                  //  writer.AddAttribute(HtmlTextWriterAttribute.Class, " col-md-4");
                   // writer.RenderBeginTag(HtmlTextWriterTag.Div);
                    //htmlBuilder.AppendLine(grid2x2.ToHTML());
                    //htmlBuilder.AppendLine("<p></p>");
                    writer.Write(grid2x2.ToHTML("",0,ForWeb));

                    //writer.RenderBeginTag(HtmlTextWriterTag.P);
                    //writer.RenderEndTag();
                   // writer.RenderEndTag();
                }

                foreach (UIElement element in panelMain.Children)
                {
                     
                    if (element is StratifiedTableAnalysisPanel)
                    {
                        StratifiedTableAnalysisPanel stap = element as StratifiedTableAnalysisPanel;
                        //htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                        writer.AddAttribute(HtmlTextWriterAttribute.Style, "height: 7px;");
                        writer.RenderBeginTag(HtmlTextWriterTag.Div);
                        writer.RenderEndTag();

                        //htmlBuilder.AppendLine(stap.ToHTML());
                        //htmlBuilder.AppendLine("<p></p>");
                        writer.Write(stap.ToHTML());
                        writer.RenderBeginTag(HtmlTextWriterTag.P);
                        writer.RenderEndTag();
                    }
                }
                if (ForWeb  ) {
                    writer.RenderEndTag(); // row div

                }
                //return htmlBuilder.ToString();
                return stringWriter.ToString();
            }
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

        private string ShowFieldPrompt(string columnName)
        {
             Field field = DashboardHelper.GetAssociatedField(columnName);
             if (field != null && field is IDataField)
                 {
                    return (field as IDataField).PromptText;
                  }
              else
                {
                  return columnName;
                }
                
        }

        //private void checkboxIncludeMissing_Checked(object sender, RoutedEventArgs e)
        //{
        //    checkboxOutcomeContinuous.IsChecked = false;
        //}

        //private void checkboxOutcomeContinuous_Checked(object sender, RoutedEventArgs e)
        //{
        //    checkboxIncludeMissing.IsChecked = false;
        //}

        //private void btnRun_Click(object sender, RoutedEventArgs e)
        //{
        //    if (LoadingCombos)
        //    {
        //        return;
        //    }

        //    CheckVariables();
        //    RefreshResults();
        //}

        //private void btnValueMappings_Click(object sender, RoutedEventArgs e)
        //{
        //    if (cbxExposureField.SelectedIndex >= 0 && cbxOutcomeField.SelectedIndex >= 0)
        //    {
        //        string exposureFieldName = cbxExposureField.SelectedItem.ToString();
        //        string outcomeFieldName = cbxOutcomeField.SelectedItem.ToString();

        //        DataColumn exposure = DashboardHelper.DataSet.Tables[0].Columns[exposureFieldName];
        //        DataColumn outcome = DashboardHelper.DataSet.Tables[0].Columns[outcomeFieldName];

        //        Dialogs.ValueMapperDialog vmd = null;

        //        if (DashboardHelper.GetAllGroupsAsList().Contains(exposureFieldName))
        //        {
        //            vmd = new Dialogs.ValueMapperDialog(DashboardHelper, DashboardHelper.GetVariablesInGroup(exposureFieldName), outcome, YesValues, NoValues);
        //        }
        //        else if (
        //            (DashboardHelper.IsColumnText(exposureFieldName) || DashboardHelper.IsColumnNumeric(exposureFieldName))
        //            ||
        //            (DashboardHelper.IsColumnText(outcomeFieldName) || DashboardHelper.IsColumnNumeric(outcomeFieldName))
        //            )
        //        {
        //            vmd = new Dialogs.ValueMapperDialog(DashboardHelper, exposure, outcome, YesValues, NoValues);
        //        }

        //        if (vmd != null)
        //        {
        //            System.Windows.Forms.DialogResult result = vmd.ShowDialog();
        //            if (result == System.Windows.Forms.DialogResult.OK)
        //            {
        //                YesValues = vmd.YesValues;
        //                NoValues = vmd.NoValues;
        //            }
        //        }
        //        else
        //        {
        //            Epi.Windows.MsgBox.ShowInformation(DashboardSharedStrings.REMAPPER_MESSAGE_ALREADY_BOOLEAN);
        //        }
        //    }
        //}
    }
   
   
}
