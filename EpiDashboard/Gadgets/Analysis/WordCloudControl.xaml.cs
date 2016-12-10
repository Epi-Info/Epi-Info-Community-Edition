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
using EpiDashboard;
using EpiDashboard.Rules;

namespace EpiDashboard
{
    /// <summary>
    /// Interaction logic for WordCloudControl.xaml
    /// </summary>
    /// <remarks>
    /// This gadget is used to generate a basic frequency for most types of data in the database. It will return the count, percent of the total, 
    /// cumulative percent, and upper/lower 95% confidence intervals. It also displays a 'percent bar' as the final column in the output table
    /// that visually represents the percent of the total. It is more or less a mirror of the FREQ command in the 'Classic' Analysis module.
    /// </remarks>
    public partial class WordCloudControl : GadgetBase
    {
        #region Private Members

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

        /// <summary>
        /// Whether the main variable is a drop-down list field
        /// </summary>
        private bool isDropDownList = false;

        /// <summary>
        /// Whether the main variable is a comment legal field
        /// </summary>
        private bool isCommentLegal = false;

        /// <summary>
        /// Whether the main variable is an option field
        /// </summary>
        private bool isOptionField = false;

        /// <summary>
        /// Whether the main variable is a recoded variable
        /// </summary>
        private bool isRecoded = false;

        /// <summary>
        /// Used for calculating the 95% confidence intervals
        /// </summary>
        private StatisticsRepository.cFreq freq = new StatisticsRepository.cFreq();

        /// <summary>
        /// A data structure used for storing the 95% confidence intervals
        /// </summary>
        private struct ConfLimit
        {
            public string Value;
            public double Upper;
            public double Lower;
        }

        private double currentWidth;

        #endregion // Private Members

        #region Delegates
        private new delegate void SetGridTextDelegate(string strataValue, TextBlockConfig textBlockConfig);
        private delegate void AddFreqGridDelegate(string strataVar, string value);
        private delegate void RenderFrequencyHeaderDelegate(string strataValue, string freqVar);
        private new delegate void AddGridFooterDelegate(string strataValue, int rowNumber, int totalRows);
        private delegate void AddDictionaryOutputDelegate(Dictionary<string, double> wordCloudDictionary, double totalWordCounts);
        #endregion // Delegates

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public WordCloudControl()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper object to attach</param>
        public WordCloudControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            Construct();
            FillComboboxes();
        }

        #endregion // Constructors

        #region Private and Protected Methods
        /// <summary>
        /// Enables and disables output columns based on user selection.
        /// </summary>
        private void ShowHideOutputColumns()
        {
            if (!LoadingCombos)
            {
                if (this.StrataGridList != null && this.StrataGridList.Count > 0)
                {
                    List<int> columnsToHide = new List<int>();

                    foreach (Grid grid in this.StrataGridList)
                    {
                        for (int i = 0; i < grid.ColumnDefinitions.Count; i++)
                        {
                            if (columnsToHide.Contains(i))
                            {
                                grid.ColumnDefinitions[i].Width = new GridLength(0);
                            }
                            else
                            {
                                if (i == 6)
                                {
                                    grid.ColumnDefinitions[i].Width = new GridLength(100);
                                }
                                else
                                {
                                    grid.ColumnDefinitions[i].Width = new GridLength(1, GridUnitType.Auto);
                                }
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
            if (wordCloudDictionary!=null)
            foreach(var key in wordCloudDictionary)
            {
                sb.Append(key.Key.ToString());
                sb.Append("  ");
            }
           /* foreach (Grid grid in this.StrataGridList)
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
                        if (columnNumber >= grid.ColumnDefinitions.Count - 2)
                        {
                            sb.AppendLine();
                        }
                    }
                }

                sb.AppendLine();
            }*/
            Clipboard.Clear();
           Clipboard.SetText(sb.ToString());
            //Clipboard.SetImage(ToBitmapSource(true));
        }

        /// <summary>
        /// Handles the filling of the gadget's combo boxes
        /// </summary>
        private void FillComboboxes(bool update = false)
        {
            //LoadingCombos = true;

            //string prevField = string.Empty;
            //string prevWeightField = string.Empty;
            ////string prevStrataField = string.Empty;
            //List<string> prevStrataFields = new List<string>();

            //if (update)
            //{
            //    if (cbxField.SelectedIndex >= 0)
            //    {
            //        prevField = cbxField.SelectedItem.ToString();
            //    }
            //}

            //cbxField.ItemsSource = null;
            //cbxField.Items.Clear();

            ////cbxFieldStrata.ItemsSource = null;
            ////cbxFieldStrata.Items.Clear();

            //List<string> fieldNames = new List<string>();
            //List<string> weightFieldNames = new List<string>();
            //List<string> strataFieldNames = new List<string>();

            //weightFieldNames.Add(string.Empty);
            ////strataFieldNames.Add(string.Empty);

            //List<string> fieldNames = new List<string>();
            //ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.DateTime | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.UserDefined | ColumnDataType.UserDefined;
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

            //cbxField.ItemsSource = fieldNames;

            //if (cbxField.Items.Count > 0)
            //{
            //    cbxField.SelectedIndex = -1;
            //}

            //if (update)
            //{
            //    cbxField.SelectedItem = prevField;

            //    foreach (string s in prevStrataFields)
            //    {
            //    }
            //}

            //LoadingCombos = false;
        }

        /// <summary>
        /// Used to add a new FREQ grid to the gadget's output
        /// </summary>
        /// <param name="strataVar">The name of the stratification variable selected, if any</param>
        /// <param name="value">The value by which this grid has been stratified by</param>
        /// <summary>
        /// Sets the grid's percentage bar
        /// </summary>
        /// <param name="strataValue">
        /// The strata value to which this grid cell belongs; used to search the list of grids and 
        /// return the proper System.Windows.Controls.Grid for text insertion.
        /// </param>
        /// <param name="rowNumber">The row number of the grid to add the bar to</param>
        /// <param name="pct">The percentage width of the bar</param>
        private void SetGridBar(string strataValue, int rowNumber, double pct)
        {
            Grid grid = GetStrataGrid(strataValue);

            Rectangle rctBar = new Rectangle();
            rctBar.Width = 0.1;// pct * 100.0;
            rctBar.Fill = this.Resources["frequencyPercentBarBrush"] as SolidColorBrush;
            rctBar.HorizontalAlignment = HorizontalAlignment.Left;
            rctBar.Margin = new Thickness(2,6,2,6); //.HorizontalAlignment = HorizontalAlignment.Left;

            Grid.SetRow(rctBar, rowNumber);
            Grid.SetColumn(rctBar, /*4*/6);
            grid.Children.Add(rctBar);

            DoubleAnimation daBar = new DoubleAnimation();
            daBar.From = 1;
            daBar.To = pct * 100.0;
            daBar.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            rctBar.BeginAnimation(Rectangle.WidthProperty, daBar);
        }

        /// <summary>
        /// Hides the cumulative percentage column
        /// </summary>
        private void HideCumulativePercent()
        {
            foreach (Grid grid in StrataGridList)
            {
                grid.ColumnDefinitions[3].Width = new GridLength(0);
            }
        }

        /// <summary>
        /// Hides the 95% CI column
        /// </summary>
        private void HideConfidenceIntervals()
        {
            foreach (Grid grid in StrataGridList)
            {
                grid.ColumnDefinitions[4].Width = new GridLength(0);
                grid.ColumnDefinitions[5].Width = new GridLength(0);
            }
        }

        /// <summary>
        /// Used to render the header (first row) of a given frequency output grid
        /// </summary>
        /// <param name="strataValue">
        /// The strata value to which this grid cell belongs; used to search the list of grids and 
        /// return the proper System.Windows.Controls.Grid for text insertion.
        /// </param>
        /// <param name="freqVar">The variable that the statistics were run on</param>        
        private void RenderFrequencyHeader(string strataValue, string freqVar)
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
            txtValHeader.Text = freqVar;
            txtValHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtValHeader, 0);
            Grid.SetColumn(txtValHeader, 0);
            grid.Children.Add(txtValHeader);

            txtValHeader.MouseLeftButtonUp += new MouseButtonEventHandler(txtValHeader_MouseLeftButtonUp);
            txtValHeader.MouseEnter += new MouseEventHandler(txtValHeader_MouseEnter);
            txtValHeader.MouseLeave += new MouseEventHandler(txtValHeader_MouseLeave);

            TextBlock txtFreqHeader = new TextBlock();
            txtFreqHeader.Text = DashboardSharedStrings.COL_HEADER_FREQUENCY;
            txtFreqHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtFreqHeader, 0);
            Grid.SetColumn(txtFreqHeader, 1);
            grid.Children.Add(txtFreqHeader);

            TextBlock txtPctHeader = new TextBlock();
            txtPctHeader.Text = DashboardSharedStrings.COL_HEADER_PERCENT;
            txtPctHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtPctHeader, 0);
            Grid.SetColumn(txtPctHeader, 2);
            grid.Children.Add(txtPctHeader);

            TextBlock txtAccuHeader = new TextBlock();
            txtAccuHeader.Text = DashboardSharedStrings.COL_HEADER_CUMULATIVE_PERCENT;
            txtAccuHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtAccuHeader, 0);
            Grid.SetColumn(txtAccuHeader, 3);
            grid.Children.Add(txtAccuHeader);

            TextBlock txtCILowHeader = new TextBlock();
            txtCILowHeader.Text = DashboardSharedStrings.COL_HEADER_CI_LOWER;
            txtCILowHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtCILowHeader, 0);
            Grid.SetColumn(txtCILowHeader, 4);
            grid.Children.Add(txtCILowHeader);

            TextBlock txtCIUpperHeader = new TextBlock();
            txtCIUpperHeader.Text = DashboardSharedStrings.COL_HEADER_CI_UPPER;
            txtCIUpperHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtCIUpperHeader, 0);
            Grid.SetColumn(txtCIUpperHeader, 5);
            grid.Children.Add(txtCIUpperHeader);
        }

        /// <summary>
        /// Used to render the footer (last row) of a given frequency output grid
        /// </summary>
        /// <param name="strataValue">
        /// The strata value to which this grid cell belongs; used to search the list of grids and 
        /// return the proper System.Windows.Controls.Grid for text insertion.
        /// </param>
        /// <param name="footerRowIndex">The row index of the footer</param>
        /// <param name="totalRows">The total number of rows in this grid</param>
        private void RenderFrequencyFooter(string strataValue, int footerRowIndex, int totalRows)
        {
            Grid grid = GetStrataGrid(strataValue);

            RowDefinition rowDefTotals = new RowDefinition();
            rowDefTotals.Height = new GridLength(26);
            grid.RowDefinitions.Add(rowDefTotals); 

            TextBlock txtValTotals = new TextBlock();
            txtValTotals.Text = SharedStrings.TOTAL;
            txtValTotals.Margin = new Thickness(4, 0, 4, 0);
            txtValTotals.VerticalAlignment = VerticalAlignment.Center;
            txtValTotals.FontWeight = FontWeights.Bold;
            Grid.SetRow(txtValTotals, footerRowIndex);
            Grid.SetColumn(txtValTotals, 0);
            grid.Children.Add(txtValTotals); 

            TextBlock txtFreqTotals = new TextBlock();
            txtFreqTotals.Text = totalRows.ToString();
            txtFreqTotals.Margin = new Thickness(4, 0, 4, 0);
            txtFreqTotals.VerticalAlignment = VerticalAlignment.Center;
            txtFreqTotals.HorizontalAlignment = HorizontalAlignment.Right;
            txtFreqTotals.FontWeight = FontWeights.Bold;
            Grid.SetRow(txtFreqTotals, footerRowIndex);
            Grid.SetColumn(txtFreqTotals, 1);
            grid.Children.Add(txtFreqTotals);

            TextBlock txtPctTotals = new TextBlock();
            txtPctTotals.Text = (1).ToString("P");//SharedStrings.DASHBOARD_100_PERCENT_LABEL;
            txtPctTotals.Margin = new Thickness(4, 0, 4, 0);
            txtPctTotals.VerticalAlignment = VerticalAlignment.Center;
            txtPctTotals.HorizontalAlignment = HorizontalAlignment.Right;
            txtPctTotals.FontWeight = FontWeights.Bold;
            Grid.SetRow(txtPctTotals, footerRowIndex);
            Grid.SetColumn(txtPctTotals, 2);
            grid.Children.Add(txtPctTotals);

            TextBlock txtAccuTotals = new TextBlock();
            txtAccuTotals.Text = (1).ToString("P");
            txtAccuTotals.Margin = new Thickness(4, 0, 4, 0);
            txtAccuTotals.VerticalAlignment = VerticalAlignment.Center;
            txtAccuTotals.HorizontalAlignment = HorizontalAlignment.Right;
            txtAccuTotals.FontWeight = FontWeights.Bold;
            Grid.SetRow(txtAccuTotals, footerRowIndex);
            Grid.SetColumn(txtAccuTotals, 3);
            grid.Children.Add(txtAccuTotals);

            TextBlock txtCILowerTotals = new TextBlock();
            txtCILowerTotals.Text = StringLiterals.SPACE + StringLiterals.SPACE + StringLiterals.SPACE;
            txtCILowerTotals.Margin = new Thickness(4, 0, 4, 0);
            txtCILowerTotals.VerticalAlignment = VerticalAlignment.Center;
            txtCILowerTotals.HorizontalAlignment = HorizontalAlignment.Right;
            txtCILowerTotals.FontWeight = FontWeights.Bold;
            Grid.SetRow(txtCILowerTotals, footerRowIndex);
            Grid.SetColumn(txtCILowerTotals, 4);
            grid.Children.Add(txtCILowerTotals);

            TextBlock txtUpperTotals = new TextBlock();
            txtUpperTotals.Text = StringLiterals.SPACE + StringLiterals.SPACE + StringLiterals.SPACE;
            txtUpperTotals.Margin = new Thickness(4, 0, 4, 0);
            txtUpperTotals.VerticalAlignment = VerticalAlignment.Center;
            txtUpperTotals.HorizontalAlignment = HorizontalAlignment.Right;
            txtUpperTotals.FontWeight = FontWeights.Bold;
            Grid.SetRow(txtUpperTotals, footerRowIndex);
            Grid.SetColumn(txtUpperTotals, 5);
            grid.Children.Add(txtUpperTotals);

            Rectangle rctTotalsBar = new Rectangle();
            rctTotalsBar.Width = 0.1;// 100;
            rctTotalsBar.Fill = this.Resources["frequencyPercentBarBrush"] as SolidColorBrush;
            rctTotalsBar.Margin = new Thickness(2, 6, 2, 6);
            rctTotalsBar.HorizontalAlignment = HorizontalAlignment.Left;
            Grid.SetRow(rctTotalsBar, footerRowIndex);
            Grid.SetColumn(rctTotalsBar, 6);
            grid.Children.Add(rctTotalsBar);

            DoubleAnimation daBar = new DoubleAnimation();
            daBar.From = 1;
            daBar.To = 100;
            daBar.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            rctTotalsBar.BeginAnimation(Rectangle.WidthProperty, daBar);
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
        /// Sets the gadget's state to 'finished with warning' mode
        /// </summary>
        /// <remarks>
        /// Common scenario for the usage of this method is when the distinct list of frequency values
        /// exceeds some built-in row limit. The output is limited to prevent the UI from locking up,
        /// and we want to let the user know that the output is limited while still showing them something.
        /// Thus we finish the rendering, but still show a message.
        /// </remarks>
        /// <param name="errorMessage">The warning message to display</param>
        protected override void RenderFinishWithWarning(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed; //waitCursor.Visibility = Visibility.Hidden;

            foreach (Grid freqGrid in StrataGridList)
            {
                freqGrid.Visibility = Visibility.Visible;
            }

            messagePanel.MessagePanelType = Controls.MessagePanelType.WarningPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

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
        /// Special method used to get the 95% confidence limit
        /// </summary>
        /// <param name="value">The value to run the CI on</param>
        /// <param name="frequency">The frequency of the value</param>
        /// <param name="count">The count</param>
        /// <returns>ConfLimit object containing the upper and lower limits</returns>
        private ConfLimit GetConfLimit(string value, double frequency, double count)
        {
            double lower = 0;
            double upper = 0;

            if (frequency == count)
            {
                lower = 1;
                upper = 1;
            }
            else
            {
                if (count > 300)
                {
                    freq.FLEISS(frequency, (double)count, 1.96, ref lower, ref upper);
                }
                else
                {
                    freq.ExactCI(frequency, (double)count, 95.0, ref lower, ref upper);
                }
            }

            ConfLimit cl = new ConfLimit();
            cl.Lower = lower;
            cl.Upper = upper;
            cl.Value = value;
            return cl;
        }

        /// <summary>
        /// Used to generate the list of variables and options for the GadgetParameters object
        /// </summary> 
        private void CreateInputVariableList()
        {
            //Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

            //GadgetOptions.MainVariableName = string.Empty;
            //GadgetOptions.WeightVariableName = string.Empty;
            //GadgetOptions.StrataVariableNames = new List<string>();
            //GadgetOptions.CrosstabVariableName = string.Empty;
            //GadgetOptions.ColumnNames = new List<string>();

            //if (cbxField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxField.SelectedItem.ToString()))
            //{
            //    inputVariableList.Add("freqvar", cbxField.SelectedItem.ToString());
            //    GadgetOptions.MainVariableName = cbxField.SelectedItem.ToString();
            //}
            //else
            //{
            //    return;
            //}

            //{
            //    inputVariableList.Add("allvalues", "false");
            //    GadgetOptions.ShouldUseAllPossibleValues = false;
            //}

            //{
            //    GadgetOptions.ShouldShowCommentLegalLabels = false;
            //}

            //{
            //    GadgetOptions.ShouldSortHighToLow = false;
            //}

            //{
            //    inputVariableList.Add("includemissing", "false");
            //    GadgetOptions.ShouldIncludeMissing = false;
            //}

            //if (!inputVariableList.ContainsKey("usepromptforfield"))
            //{
            //    {
            //        inputVariableList.Add("usepromptforfield", "false");
            //    }
            //}

            //GadgetOptions.ShouldIncludeFullSummaryStatistics = false;
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
            currentWidth = borderAll.ActualWidth;
            StrataGridList = new List<Grid>();
            StrataExpanderList = new List<Expander>();
            //cbxField.SelectionChanged += new SelectionChangedEventHandler(cbxField_SelectionChanged);
            //cbxFieldWeight.SelectionChanged += new SelectionChangedEventHandler(cbxField_SelectionChanged);
            //cbxFieldStrata.SelectionChanged += new SelectionChangedEventHandler(cbxField_SelectionChanged);

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

            // HIDE SEND DATA TO EXCEL MENU ITEM BECAUSE WORD CLOUDS ARE IMAGES NOT TABULAR INFORMATION
            mnuSendDataToExcel.Visibility = Visibility.Collapsed;

            mnuSendToBack.Click += new RoutedEventHandler(mnuSendToBack_Click);
            mnuClose.Click += new RoutedEventHandler(mnuClose_Click);

            //checkboxAllValues.Checked += new RoutedEventHandler(checkboxCheckChanged);
            //checkboxAllValues.Unchecked += new RoutedEventHandler(checkboxCheckChanged);

            //checkboxCommentLegalLabels.Checked += new RoutedEventHandler(checkboxCheckChanged);
            //checkboxCommentLegalLabels.Unchecked += new RoutedEventHandler(checkboxCheckChanged);

            //checkboxIncludeMissing.Checked += new RoutedEventHandler(checkboxCheckChanged);
            //checkboxIncludeMissing.Unchecked += new RoutedEventHandler(checkboxCheckChanged);

            //checkboxSortHighLow.Checked += new RoutedEventHandler(checkboxCheckChanged);
            //checkboxSortHighLow.Unchecked += new RoutedEventHandler(checkboxCheckChanged);

            this.IsProcessing = false;

            this.GadgetStatusUpdate += new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(IsCancelled);

            #region Translation
            //ConfigExpandedTitle.Text = DashboardSharedStrings.GADGET_CONFIG_TITLE_WORD_CLOUD;
            //tblockMainVariable.Text = DashboardSharedStrings.GADGET_PARSE_FIELD;
            //btnRun.Content = DashboardSharedStrings.GADGET_RUN_BUTTON;
            #endregion // Translation

            base.Construct();
            this.Parameters = new WordCloudParameters();
        }

        /// <summary>
        /// Shows the field's column name in the first column heading for the output grid. 
        /// </summary>
        private void ShowFieldName()
        {
            foreach (Grid grid in this.StrataGridList)
            {
                IEnumerable<UIElement> elements = grid.Children.Cast<UIElement>().Where(x => Grid.GetRow(x) == 0 && Grid.GetColumn(x) == 0);
                TextBlock txt = null;
                foreach (UIElement element in elements)
                {
                    if (element is TextBlock)
                    {
                        txt = element as TextBlock;
                        break;
                    }
                }

                if (txt != null)
                {
                    txt.Text = GadgetOptions.MainVariableName;
                }
            }
        }

        /// <summary>
        /// Shows the field's prompt value (if applicable) in the first column heading for the output grid.
        /// </summary>
        private void ShowFieldPrompt()
        {
            foreach (Grid grid in this.StrataGridList)
            {
                IEnumerable<UIElement> elements = grid.Children.Cast<UIElement>().Where(x => Grid.GetRow(x) == 0 && Grid.GetColumn(x) == 0);
                TextBlock txt = null;
                foreach (UIElement element in elements)
                {
                    if (element is TextBlock)
                    {
                        txt = element as TextBlock;
                        break;
                    }
                }

                if (txt != null)
                {
                    Field field = DashboardHelper.GetAssociatedField(GadgetOptions.MainVariableName);
                    if (field != null && field is IDataField)
                    {
                        txt.Text = (field as IDataField).PromptText;
                    }
                }
            }
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

        /// <summary>
        /// Collapses the output for the gadget
        /// </summary>
        public override void CollapseOutput()
        {                     
            borderAll.MinWidth = currentWidth;
            panelMain.Visibility = System.Windows.Visibility.Collapsed;

            this.messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            this.infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            //descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed; //EI-24
            IsCollapsed = true;
        }

        /// <summary>
        /// Expands the output for the gadget
        /// </summary>
        public override void ExpandOutput()
        {            
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

            panelMain.Visibility = System.Windows.Visibility.Visible;

            IsCollapsed = false;
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
            for (int i = 0; i < StrataExpanderList.Count; i++)
            {
                StrataExpanderList[i].Content = null;
            }
            this.StrataExpanderList.Clear();
            this.StrataGridList.Clear();
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
                if (grid.Parent is Border)
                {
                    Border border = (grid.Parent) as Border;                    
                    panelMain.Children.Remove(border);
                }                
            }

            foreach (Expander expander in StrataExpanderList)
            {
                if (panelMain.Children.Contains(expander))
                {
                    panelMain.Children.Remove(expander);
                }
            }

            panelMain.Children.Clear();

            StrataGridList.Clear();
            StrataExpanderList.Clear();
        }

        Controls.GadgetProperties.WordCloudProperties properties = null;
        public override void ShowHideConfigPanel()
        {
            Popup = new DashboardPopup();
            Popup.Parent = ((this.Parent as DragCanvas).Parent as ScrollViewer).Parent as Grid;
            properties = new Controls.GadgetProperties.WordCloudProperties(this.DashboardHelper, this, (WordCloudParameters)Parameters);

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
                properties = new Controls.GadgetProperties.WordCloudProperties(this.DashboardHelper, this, (WordCloudParameters)Parameters);
            properties.Height = (Convert.ToInt32(i_StandardHeight * f_HeightRatio)) / 1.07;
            properties.Width = (Convert.ToInt32(i_StandardWidth * f_WidthRatio)) / 1.07;

        }

        private void properties_ChangesAccepted(object sender, EventArgs e)
        {
            Controls.GadgetProperties.WordCloudProperties properties = Popup.Content as Controls.GadgetProperties.WordCloudProperties;
            this.Parameters = properties.Parameters;
            this.DataFilters = properties.DataFilters;
            this.CustomOutputHeading = this.Parameters.GadgetTitle;
            this.CustomOutputDescription = this.Parameters.GadgetDescription;
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
        /// Checks the selected variables and enables/disables checkboxes as appropriate
        /// </summary>

        #endregion // Private and Protected Methods

        #region Public Methods
        #region IGadget Members
        /// <summary>
        /// Sets the gadget to its 'processing' state
        /// </summary>
        public override void SetGadgetToProcessingState()
        {
            this.IsProcessing = true;
            //this.cbxField.IsEnabled = false;
            //this.btnRun.IsEnabled = false;
        }

        /// <summary>
        /// Sets the gadget to its 'finished' state
        /// </summary>
        public override void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
            //this.cbxField.IsEnabled = true;
            //this.btnRun.IsEnabled = true;

            if (IsDropDownList || IsRecoded)
            {
            }

            if (IsCommentLegal || IsOptionField)
            {
            }
            currentWidth = borderAll.ActualWidth;
            base.SetGadgetToFinishedState();
        }

        /// <summary>
        /// Initiates a refresh of the gadget's output
        /// </summary>
        public override void RefreshResults()
        {
            //if (!LoadingCombos && GadgetOptions != null && cbxField.SelectedIndex > -1)
            if (!LoadingCombos && Parameters != null && Parameters.ColumnNames.Count > 0)
            {
                //CreateInputVariableList();
                infoPanel.Visibility = System.Windows.Visibility.Collapsed;
                waitPanel.Visibility = System.Windows.Visibility.Visible;
                messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
                descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
                baseWorker = new BackgroundWorker();
                baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
                baseWorker.RunWorkerAsync();
                base.RefreshResults();
            }
            //else if (!LoadingCombos && cbxField.SelectedIndex == -1)
            else
            {
                ClearResults();
                waitPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Updates the variable names available in the gadget's properties
        /// </summary>
        public override void UpdateVariableNames()
        {
            FillComboboxes(true);
        }

        /// <summary>
        /// Generates Xml representation of this gadget
        /// </summary>
        /// <param name="doc">The Xml docment</param>
        /// <returns>XmlNode</returns>
        public override XmlNode Serialize(XmlDocument doc)
        {
            WordCloudParameters WordCloudParameters = (WordCloudParameters)Parameters;

            System.Xml.XmlElement element = doc.CreateElement("frequencyGadget");
            //element.InnerXml = xmlString;
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
            type.Value = "EpiDashboard.WordCloudControl";
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

            string freqVar = string.Empty;

            WordBuilder wb = new WordBuilder(",");

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

            int precision = 4;
            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            XmlElement mainVariableElement = doc.CreateElement("mainVariable");
            if (WordCloudParameters.ColumnNames.Count > 0)
            {
                mainVariableElement.InnerText = WordCloudParameters.ColumnNames[0];
            }
            element.AppendChild(mainVariableElement);


            XmlElement precisionElement = doc.CreateElement("precision");
            precisionElement.InnerText = precision.ToString();
            element.AppendChild(precisionElement);

            XmlElement commonWordsElement = doc.CreateElement("commonWords");
            commonWordsElement.InnerText = WordCloudParameters.CommonWords;
            element.AppendChild(commonWordsElement);

            //string xmlString =
            //"<mainVariable>" + freqVar + "</mainVariable>";
            
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

            //xmlString = xmlString + "<weightVariable>" + weightVar + "</weightVariable>" +
            //"<sort>" + sort + "</sort>" +
            //"<allValues>" + allValues + "</allValues>" +
            //"<precision>" + precision.ToString() + "</precision>" +
            //"<showListLabels> </showListLabels>" +
            //"<useFieldPrompts> </useFieldPrompts>" +
            //"<columnsToShow>" + wb.ToString() + "</columnsToShow>" +
            //"<includeMissing>" + includeMissing + "</includeMissing>" +
            //"<customHeading>" + CustomOutputHeading.Replace("<", "&lt;") + "</customHeading>" +
            //"<customDescription>" + CustomOutputDescription.Replace("<", "&lt;") + "</customDescription>" +
            //"<customCaption>" + CustomOutputCaption + "</customCaption>";

            XmlElement customHeadingElement = doc.CreateElement("customHeading");
            customHeadingElement.InnerText = CustomOutputHeading.Replace("<", "&lt;");
            element.AppendChild(customHeadingElement);

            XmlElement customDescriptionElement = doc.CreateElement("customDescription");
            customDescriptionElement.InnerText = CustomOutputDescription.Replace("<", "&lt;");
            element.AppendChild(customDescriptionElement);

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
            this.Parameters = new WordCloudParameters();
            HideConfigPanel();
            infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLowerInvariant())
                {
                    case "mainvariable":
                        //cbxField.Text = child.InnerText.Replace("&lt;", "<");
                        ((WordCloudParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                        break;
                    case "stratavariable":
                        break;
                    case "stratavariables":
                        foreach (XmlElement field in child.ChildNodes)
                        {
                            List<string> fields = new List<string>();
                            if (field.Name.ToLowerInvariant().Equals("stratavariable"))
                            {
                            }
                        }
                        break;
                    case "weightvariable":
                        break;
                    case "precision":
                        int precision = 4;
                        int.TryParse(child.InnerText, out precision);
                        break;
                    case "commonwords":
                        ((WordCloudParameters)Parameters).CommonWords = (child.InnerText.Replace("&lt;", "<"));
                        break;
                    case "sort":
                        if (child.InnerText.ToLowerInvariant().Equals("highlow"))
                        {
                        }
                        break;
                    case "allvalues":
                        if (child.InnerText.ToLowerInvariant().Equals("true")) {  }
                        else {  }
                        break;
                    case "showlistlabels":
                        break;
                    case "columnstoshow":
                        break;
                    case "includemissing":
                        break;
                    case "usefieldprompts":
                        bool usePrompts = false;
                        bool.TryParse(child.InnerText, out usePrompts);
                        break;
                    case "customheading":
                        if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
                        {
                            this.CustomOutputHeading = child.InnerText.Replace("&lt;", "<");
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
                }
            }

            base.CreateFromXml(element);
            this.LoadingCombos = false;
            RefreshResults();
            HideConfigPanel();
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0, bool useAlternatingColors = false)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            if (CustomOutputHeading == null || (string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)")))
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">WordCloud</h2>");
            }
            else if (CustomOutputHeading != "(none)")
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">" + CustomOutputHeading + "</h2>");
            }

            htmlBuilder.AppendLine("<p class=\"gadgetOptions\"><small>");
            htmlBuilder.AppendLine("<em>WordCloud variable:</em> <strong>" + Parameters.ColumnNames[0] + "</strong>");
            htmlBuilder.AppendLine("<br />");

            //if (cbxFieldStrata.SelectedIndex >= 0)
            //{
            //    htmlBuilder.AppendLine("<em>Strata variable:</em> <strong>" + cbxFieldStrata.Text + "</strong>");
            //    htmlBuilder.AppendLine("<br />");
            //}
            htmlBuilder.AppendLine("<em>Include missing:</em> <strong> </strong>");
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

            string imageFileName = string.Empty;

            if (htmlFileName.EndsWith(".html"))
            {
                imageFileName = htmlFileName.Remove(htmlFileName.Length - 5, 5);
            }
            else if (htmlFileName.EndsWith(".htm"))
            {
                imageFileName = htmlFileName.Remove(htmlFileName.Length - 4, 4);
            }

            imageFileName = imageFileName + "_" + count.ToString() + ".png";

            System.IO.FileInfo fi = new System.IO.FileInfo(imageFileName);

            ToImageFile(imageFileName, false);

            htmlBuilder.AppendLine("<img src=\"" + fi.Name + "\" />");

            return htmlBuilder.ToString();
        }

        public virtual void ToImageFile(string fileName, bool includeGrid = true)
        {
            BitmapSource img = ToBitmapSource(false);

            System.IO.FileStream stream = new System.IO.FileStream(fileName, System.IO.FileMode.Create);
            BitmapEncoder encoder = null; // new BitmapEncoder();

            if (fileName.EndsWith(".png"))
            {
                encoder = new PngBitmapEncoder();
            }
            else
            {
                encoder = new JpegBitmapEncoder();
            }

            if (img != null)
            {
                encoder.Frames.Add(BitmapFrame.Create(img));
                encoder.Save(stream);
            }
            
            stream.Close();
        }

        public virtual BitmapSource ToBitmapSource(bool includeGrid = true)        
        {
            if (panelMain.Children.Count > 0 && panelMain.Children[0] is Canvas)
            {
                Canvas canvas = panelMain.Children[0] as Canvas;

                Transform transform = canvas.LayoutTransform;

                // reset current transform (in case it is scaled or rotated)
                canvas.LayoutTransform = null;

                Size size = new Size(canvas.Width, canvas.Height);
                // Measure and arrange the surface
                // VERY IMPORTANT
                canvas.Measure(size);
                canvas.Arrange(new Rect(size));

                RenderTargetBitmap renderBitmap =
                  new RenderTargetBitmap(
                    (int)size.Width,
                    (int)size.Height,
                    96d,
                    96d,
                    PixelFormats.Pbgra32);
                renderBitmap.Render(canvas);

                canvas.LayoutTransform = transform;

                canvas.Measure(size);
                canvas.Arrange(new Rect(size));

                return renderBitmap;
            }
            else
            {
                return null;
            }
            
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

        /// <summary>
        /// Returns the gadget's description as a string.
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return "WordCloud Gadget";
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the Checked event for checkboxUsePrompts
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void checkboxUsePrompts_Checked(object sender, RoutedEventArgs e)
        {
            ShowFieldPrompt();
        }

        /// <summary>
        /// Handles the Unchecked event for checkboxUsePrompts
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void checkboxUsePrompts_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowFieldName();
        }

        /// <summary>
        /// Handles the MouseLeave event for the value header in the output grid
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void txtValHeader_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Handles the MouseEnter event for the value header in the output grid
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void txtValHeader_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        /// <summary>
        /// Handles the MouseLeftButtonUp event for the value header in the output grid
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        void txtValHeader_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
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
        /// Fired when the user changes a field selection
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void cbxField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (LoadingCombos)
            //{
            //    return;
            //}

            //CheckVariables();
            //RefreshResults();
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
            //Debug.Print("Background worker thread for frequency gadget was cancelled or ran to completion.");
            System.Threading.Thread.Sleep(100);
            this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
            //System.Threading.Thread.Sleep(10000);
        }
        Dictionary<string, double> wordCloudDictionary = null;
        /// <summary>
        /// Handles the DoWorker event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        protected override void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (syncLock)
            {
                //Dictionary<string, string> inputVariableList = ((GadgetParameters)e.Argument).InputVariableList;
                WordCloudParameters WordCloudParameters = (WordCloudParameters)Parameters;

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));                
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));                

                //string freqVar = GadgetOptions.MainVariableName;
                //string weightVar = GadgetOptions.WeightVariableName;
                //string strataVar = string.Empty;                
                //bool includeMissing = GadgetOptions.ShouldIncludeMissing;

                if (WordCloudParameters.ColumnNames.Count > 0)
                {
                    string freqVar = WordCloudParameters.ColumnNames[0];
                }

                //if (inputVariableList.ContainsKey("stratavar"))
                //{
                //    strataVar = inputVariableList["stratavar"];
                //}
                
                //List<string> stratas = new List<string>();
                //if (!string.IsNullOrEmpty(strataVar))
                //{
                //    stratas.Add(strataVar);
                //}

                try
                {                    
                    RequestUpdateStatusDelegate requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
                    CheckForCancellationDelegate checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

                    //GadgetOptions.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                    //GadgetOptions.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);
                    WordCloudParameters.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                    WordCloudParameters.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);
                    if (this.DataFilters != null && this.DataFilters.Count > 0)
                    {
                        WordCloudParameters.CustomFilter = this.DataFilters.GenerateDataFilterString(false);
                    }
                    else
                    {
                        WordCloudParameters.CustomFilter = string.Empty;
                    }

                    //GadgetOptions.ShouldIgnoreRowLimits = true;
//                    Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables = DashboardHelper.GenerateFrequencyTable(GadgetOptions/*, freqVar, weightVar, stratas, string.Empty, useAllPossibleValues, sortHighLow, includeMissing, false*/);
//                    DataTable dt = DashboardHelper.ExtractFirstFrequencyTable(stratifiedFrequencyTables);
                    //DataView dv = DashboardHelper.GenerateView(GadgetOptions);
                    DataView dv = DashboardHelper.GenerateView(WordCloudParameters);

                    Dictionary<char, Dictionary<string, double>> wordCloudDictionary1 = new Dictionary<char, Dictionary<string, double>>();
                    wordCloudDictionary1.Add('0', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('1', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('2', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('3', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('4', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('5', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('6', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('7', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('8', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('9', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('a', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('b', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('c', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('d', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('e', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('f', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('g', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('h', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('i', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('j', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('k', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('l', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('m', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('n', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('o', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('p', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('q', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('r', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('s', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('t', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('u', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('v', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('w', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('x', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('y', new Dictionary<string, double>());
                    wordCloudDictionary1.Add('z', new Dictionary<string, double>());
                   wordCloudDictionary = new Dictionary<string, double>();

                    if (WordCloudParameters.ColumnNames.Count > 0 && !String.IsNullOrEmpty(WordCloudParameters.ColumnNames[0]))
                    {
                        foreach (DataRowView dr in dv)
                        {
                            DataRow row = dr.Row;
                            string[] strings = row[WordCloudParameters.ColumnNames[0]].ToString().Replace(", ", " ").Replace(". ", " ").ToLowerInvariant().Split(' ');

                            foreach (string s in strings)
                            {
                                string s0 = s;
                                if (s.Length > 0 && (s.Substring(s.Length - 1).Equals(",") || s.Substring(s.Length - 1).Equals(".")))
                                    s0 = s.Remove(s.Length - 1);
                                char[] firstChars = s0.ToCharArray();
                                if (firstChars.Length > 0)
                                {
                                    char firstChar = firstChars[0];
                                    if (wordCloudDictionary1.ContainsKey(firstChar))
                                    {
                                        if (wordCloudDictionary1[firstChar].ContainsKey(s0))
                                        {
                                            wordCloudDictionary1[firstChar][s0]++;
                                        }
                                        else
                                        {
                                            //, . a am an and are as at but by for her his i if in is it me my not of on or our pm that the their this to us was we were will with would you your
                                            //string txtCommonWords = ", . a am an and are as at but by for her his i if in is it me my not of on or our pm that the their this to us was we were will with would you your";

                                            //if (!s0.Equals(",") && !s0.Equals(".") && !s0.Equals("and") && !s0.Equals("the") && !s0.Equals("for") &&
                                            //    !s0.Equals("in") && !s0.Equals("on") && !s0.Equals("with") && !s0.Equals("of") && !s0.Equals("a") && !s0.Equals("an")
                                            //     && !s0.Equals("to") && !s0.Equals("as") && !s0.Equals("we") && !s0.Equals("i") && !s0.Equals("me") && !s0.Equals("us")
                                            //     && !s0.Equals("you") && !s0.Equals("this") && !s0.Equals("it") && !s0.Equals("is") && !s0.Equals("are") && !s0.Equals("or")
                                            //     && !s0.Equals("my") && !s0.Equals("that") && !s0.Equals("if") && !s0.Equals("not") && !s0.Equals("were") && !s0.Equals("was")
                                            //     && !s0.Equals("by") && !s0.Equals("at") && !s0.Equals("our") && !s0.Equals("your") && !s0.Equals("their") && !s0.Equals("will")
                                            //     && !s0.Equals("would") && !s0.Equals("but") && !s0.Equals("his") && !s0.Equals("her") && !s0.Equals("am") && !s0.Equals("pm")
                                            //     && s0.Length > 0)
                                            if(s0.Length > 0 && !WordCloudParameters.CommonWords.Contains(s0))
                                            {
                                                wordCloudDictionary1[firstChar].Add(s0, 1.0);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    Dictionary<string, double> wordCloudDictionary0 = new Dictionary<string, double>();

                    foreach (KeyValuePair<char, Dictionary<string, double>> kvp0 in wordCloudDictionary1)
                    {
                        foreach (KeyValuePair<string, double> kvp in kvp0.Value)
                        {
                            if (kvp.Value > 1.0)
                            {
                                wordCloudDictionary0.Add(kvp.Key, kvp.Value);
                            }
                        }
                    }

                    wordCloudDictionary = wordCloudDictionary0.OrderByDescending(g => g.Value).ToDictionary(g => g.Key, g => g.Value);

                    wordCloudDictionary0.Clear();

                    int entryCount = 0;
                    double totalWordCounts = 0;
                    foreach (KeyValuePair<string, double> kvp in wordCloudDictionary)
                    {
                        wordCloudDictionary0.Add(kvp.Key, kvp.Value);
                        totalWordCounts += kvp.Value;
                        if (++entryCount > 49)
                            break;
                    }

//                    wordCloudDictionary = wordCloudDictionary0.OrderBy(g => g.Key).ToDictionary(g => g.Key, g => g.Value);
                    wordCloudDictionary = wordCloudDictionary0;
/*
                    // Sort so more frequent words are in the middle
                    Dictionary<string, double> wordCloudDictionary1 = wordCloudDictionary0.OrderBy(g => g.Value).ToDictionary(g => g.Key, g => g.Value);
                    wordCloudDictionary.Clear();
                    bool isEven = (wordCloudDictionary0.Count() % 2 == 0);
                    int wcdItem = 0;
                    foreach (KeyValuePair<string, double> kvp in wordCloudDictionary1)
                    {
                        wcdItem++;
                        if (wcdItem % 2 == 1)
                            wordCloudDictionary.Add(kvp.Key, kvp.Value);
                    }
                    wcdItem = 0;
                    if (isEven)
                    {
                        foreach (KeyValuePair<string, double> kvp in wordCloudDictionary0)
                        {
                            wcdItem++;
                            if (wcdItem % 2 == 1)
                                wordCloudDictionary.Add(kvp.Key, kvp.Value);
                        }
                    }
                    else
                    {
                        foreach (KeyValuePair<string, double> kvp in wordCloudDictionary0)
                        {
                            wcdItem++;
                            if (wcdItem % 2 == 0)
                                wordCloudDictionary.Add(kvp.Key, kvp.Value);
                        }
                    }
                    // End altered sorting
*/
                    if (wordCloudDictionary == null || wordCloudDictionary.Count == 0)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), DashboardSharedStrings.GADGET_MSG_NO_DATA);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));                        
                        return;
                    }

                    this.Dispatcher.BeginInvoke(new AddDictionaryOutputDelegate(RenderWordCloudImage), wordCloudDictionary, totalWordCounts);
                    this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                }
                finally
                {
                    stopwatch.Stop();
                    Debug.Print("Word cloud gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete with " + DashboardHelper.RecordCount.ToString() + " records and the following filters:");
                    Debug.Print(DashboardHelper.DataFilters.GenerateDataFilterString());
                }
            }
        }

        private void RenderWordCloudImage(Dictionary<string, double> wordCloudDictionary, double totalWordCounts)
        {
            int words = wordCloudDictionary.Count();
            DrawingGroup drawingGroup = new DrawingGroup();
            foreach (KeyValuePair<string, double> kvp in wordCloudDictionary)
            {
                double fontSize = 10.0;
                if (words <= 5)
                    fontSize = 10.0 + Math.Min(Math.Max((100 * kvp.Value) / totalWordCounts, 0.0), 100.0);
                else
                    fontSize = 10.0 + Math.Min(Math.Max((500 * kvp.Value) / totalWordCounts, 0.0), 100.0);

                string word = kvp.Key;

                FormattedText atoz = new FormattedText(word, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Georgia"), fontSize, Brushes.Black);
                Random rnd = new Random();
                bool intersects = true;
                GeometryDrawing geoDrawing = new GeometryDrawing();
                
                int iterations = 0;
                bool gaveUp = false;
                while (intersects)
                {
                    if (iterations > 50)
                    {
                        gaveUp = true;
                        break;
                    }
                    iterations++;
                    int maxY = 225;
                    int maxX = 325;
                    if (drawingGroup.Children.Count < 1)
                    {
                        maxY = 75;
                        maxX = 175;
                    }
                    Geometry geo = atoz.BuildGeometry(new Point(rnd.Next(0, maxX), rnd.Next(0, maxY)));

                    int colorIndex = rnd.Next(0, 6);
                    Brush color;
                    if (colorIndex == 0)
                    {
                        color = new SolidColorBrush(Color.FromRgb(44, 59, 0));
                    }
                    else if (colorIndex == 1)
                    {
                        color = new SolidColorBrush(Color.FromRgb(247, 125, 0));
                    }
                    else if (colorIndex == 2)
                    {
                        color = new SolidColorBrush(Color.FromRgb(107, 0, 21));
                    }
                    else if (colorIndex == 3)
                    {
                        color = new SolidColorBrush(Color.FromRgb(102, 110, 0));
                    }
                    else if (colorIndex == 4)
                    {
                        color = new SolidColorBrush(Color.FromRgb(247, 170, 0));
                    }
                    else
                    {
                        color = new SolidColorBrush(Color.FromRgb(235, 14, 0));
                    }

                    geoDrawing = new GeometryDrawing(color, new Pen(color, 1.0), geo);
                    intersects = false;
                    foreach (GeometryDrawing d in drawingGroup.Children)
                    {
                        if (geo.FillContainsWithDetail(d.Geometry) != IntersectionDetail.Empty)
                        {
                            intersects = true;
                            break;
                        }
                    }
                }

                if (!gaveUp)
                {
                    drawingGroup.Children.Add(geoDrawing);
                }
                else
                {
                    break;
                }
            }

            DrawingImage drawingImage = new DrawingImage(drawingGroup);
            Canvas canvas = new Canvas();
            canvas.Height = 400;
            canvas.Width = 600;
            Image img = new Image();
            img.Height = 400;
            img.Width = 600;
            img.Source = drawingImage;
            canvas.Children.Add(img);
            panelMain.Children.Add(canvas);

        }
        #endregion // Event Handlers        
        
        #region Private Properties
        /// <summary>
        /// Gets whether or not the main variable is a drop-down list
        /// </summary>
        private bool IsDropDownList
        {
            get
            {
                return this.isDropDownList;
            }
        }

        /// <summary>
        /// Gets whether the main variable is a comment legal field
        /// </summary>
        private bool IsCommentLegal
        {
            get
            {
                return this.isCommentLegal;
            }
        }

        /// <summary>
        /// Gets whether the main variable is an option field
        /// </summary>
        private bool IsOptionField
        {
            get
            {
                return this.isOptionField;
            }
        }

        /// <summary>
        /// Gets whether the main variable is a recoded variable
        /// </summary>
        private bool IsRecoded
        {
            get
            {
                return this.isRecoded;
            }
        }
        #endregion // Private Properties
    }
}
