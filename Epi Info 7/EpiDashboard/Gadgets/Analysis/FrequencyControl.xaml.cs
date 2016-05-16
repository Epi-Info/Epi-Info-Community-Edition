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
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for FrequencyControl.xaml
    /// </summary>
    /// <remarks>
    /// This gadget is used to generate a basic frequency for most types of data in the database. It will return the count, percent of the total, 
    /// cumulative percent, and upper/lower 95% confidence intervals. It also displays a 'percent bar' as the final column in the output table
    /// that visually represents the percent of the total. It is more or less a mirror of the FREQ command in the 'Classic' Analysis module.
    /// </remarks>
    public partial class FrequencyControl : GadgetBase
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
        /// Used for maintaining the width when collapsing output
        /// </summary>
        private double currentWidth;

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
        /// Used to hold main variables.
        /// </summary>
        private List<string> fields;

        /// <summary>
        /// A data structure used for storing the 95% confidence intervals
        /// </summary>
        private struct ConfLimit
        {
            public string Value;
            public double Upper;
            public double Lower;
        }

        #endregion // Private Members

        #region Delegates
        private new delegate void SetGridTextDelegate(string strataValue, TextBlockConfig textBlockConfig);
        private delegate void AddFreqGridDelegate(string strataVar, string value);
        private delegate void RenderFrequencyHeaderDelegate(string strataValue, string freqVar, double observationCount);
        private new delegate void AddGridFooterDelegate(string strataValue, int rowNumber, int totalRows);
        #endregion // Delegates

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public FrequencyControl()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper object to attach</param>
        public FrequencyControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            Construct();
        }

        #endregion // Constructors

        #region Private and Protected Methods

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
                        if (columnNumber >= grid.ColumnDefinitions.Count - 2)
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
        /// Used to add a new FREQ grid to the gadget's output
        /// </summary>
        /// <param name="strataVar">The name of the stratification variable selected, if any</param>
        /// <param name="value">The value by which this grid has been stratified by</param>
        private void AddFreqGrid(string strataVar, string value)
        {
            FrequencyParameters freqParameters = (FrequencyParameters)Parameters;

            Grid grid = new Grid();            
            grid.Tag = value;
            grid.Style = this.Resources["genericOutputGrid"] as Style;
            grid.Visibility = System.Windows.Visibility.Collapsed;

            Border border = new Border();
            border.Style = this.Resources["genericOutputGridBorderWithMargins"] as Style;
            border.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            border.Child = grid;

            Expander expander = new Expander();

            TextBlock txtExpanderHeader = new TextBlock();
            txtExpanderHeader.Text = value;
            txtExpanderHeader.Style = this.Resources["genericOutputExpanderText"] as Style;
            expander.Header = txtExpanderHeader;

            if (string.IsNullOrEmpty(strataVar) && this.Parameters.StrataVariableNames.Count == 0)
            {
                panelMain.Children.Add(border);
            }
            else
            {
                expander.Margin = (Thickness)this.Resources["expanderMargin"]; //new Thickness(6, 2, 6, 6);                
                //grid.Margin = new Thickness(0, 4, 0, 4);
                expander.Content = border;
                expander.IsExpanded = true;
                panelMain.Children.Add(expander);
                StrataExpanderList.Add(expander);
            }

            StrataGridList.Add(grid);

            ColumnDefinition column1 = new ColumnDefinition();
            ColumnDefinition column2 = new ColumnDefinition();
            ColumnDefinition column3 = new ColumnDefinition();
            ColumnDefinition column4 = new ColumnDefinition();
            ColumnDefinition column5 = new ColumnDefinition();
            ColumnDefinition column6 = new ColumnDefinition();
            ColumnDefinition column7 = new ColumnDefinition();

            column1.Width = GridLength.Auto;
            column2.Width = GridLength.Auto;
            column3.Width = GridLength.Auto;
            column4.Width = GridLength.Auto;
            column5.Width = GridLength.Auto;
            column6.Width = GridLength.Auto;            

            int width = 100;
            if (int.TryParse(freqParameters.PercentBarWidth.ToString(), out width))
            {
                column7.Width = new GridLength(width);
            }
            else
            {
                column7.Width = new GridLength(100);
            }

            grid.ColumnDefinitions.Add(column1);
            grid.ColumnDefinitions.Add(column2);
            grid.ColumnDefinitions.Add(column3);
            grid.ColumnDefinitions.Add(column4);
            grid.ColumnDefinitions.Add(column5);
            grid.ColumnDefinitions.Add(column6);
            grid.ColumnDefinitions.Add(column7);
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
            txt.Foreground = this.Resources["cellForeground"] as SolidColorBrush;
            txt.Text = textBlockConfig.Text;
            txt.Margin = textBlockConfig.Margin;
            txt.VerticalAlignment = textBlockConfig.VerticalAlignment;
            txt.HorizontalAlignment = textBlockConfig.HorizontalAlignment;
            Grid.SetRow(txt, textBlockConfig.RowNumber);
            Grid.SetColumn(txt, textBlockConfig.ColumnNumber);
            grid.Children.Add(txt);
        }

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
            FrequencyParameters freqParameters = (FrequencyParameters)Parameters;

            Grid grid = GetStrataGrid(strataValue);

            Rectangle rctBar = new Rectangle();
            rctBar.Width = 0.1;// pct * 100.0;
            rctBar.Fill = this.Resources["frequencyPercentBarBrush"] as SolidColorBrush;
            rctBar.HorizontalAlignment = HorizontalAlignment.Left;
            rctBar.Margin = new Thickness(2, 6, 2, 6); //.HorizontalAlignment = HorizontalAlignment.Left;

            Grid.SetRow(rctBar, rowNumber);
            Grid.SetColumn(rctBar, /*4*/6);
            grid.Children.Add(rctBar);

            int maxWidth = 100;
            int.TryParse(freqParameters.PercentBarWidth.ToString(), out maxWidth);

            DoubleAnimation daBar = new DoubleAnimation();
            daBar.From = 1;
            daBar.To = pct * maxWidth;//100.0;
            daBar.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            rctBar.BeginAnimation(Rectangle.WidthProperty, daBar);

            if (freqParameters.PercentBarMode == "Bar and percent")
            {
                TextBlock tb = new TextBlock();
                tb.Margin = new Thickness(pct * maxWidth + 5, 0, 0, 0);
                tb.Foreground = this.Resources["cellForeground"] as SolidColorBrush;
                tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                tb.Text = pct.ToString("P0");

                Grid.SetRow(tb, rowNumber);
                Grid.SetColumn(tb, 6);
                grid.Children.Add(tb);
            }
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
        private void RenderFrequencyHeader(string strataValue, string freqVar, double observationCount)
        {
            FrequencyParameters freqParameters = (FrequencyParameters)Parameters;

            Grid grid = GetStrataGrid(strataValue);

            RowDefinition rowDefHeader = new RowDefinition();
            rowDefHeader.Height = new GridLength(30);            

            grid.RowDefinitions.Add(rowDefHeader);

            if (freqParameters.DrawHeaderRow == false)
            {
                rowDefHeader.Height = new GridLength(0);
                return;
            }

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

            //Dont show prompt if frequency is requested for more than one main variable.
            if (freqParameters.UseFieldPrompts == true && fields.Count > 0) ShowFieldPrompt();
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
            if (observationCount < 300)
                txtCILowHeader.Text = DashboardSharedStrings.COL_HEADER_EXACT_CI_LOWER;
            else
                txtCILowHeader.Text = DashboardSharedStrings.COL_HEADER_WILSON_CI_LOWER;
            txtCILowHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtCILowHeader, 0);
            Grid.SetColumn(txtCILowHeader, 4);
            grid.Children.Add(txtCILowHeader);

            TextBlock txtCIUpperHeader = new TextBlock();
            if (observationCount < 300)
                txtCIUpperHeader.Text = DashboardSharedStrings.COL_HEADER_EXACT_CI_UPPER;
            else
                txtCIUpperHeader.Text = DashboardSharedStrings.COL_HEADER_WILSON_CI_UPPER;
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
            FrequencyParameters freqParameters = (FrequencyParameters)Parameters;

            Grid grid = GetStrataGrid(strataValue);

            RowDefinition rowDefTotals = new RowDefinition();
            rowDefTotals.Height = new GridLength(26);            

            grid.RowDefinitions.Add(rowDefTotals);

            if (freqParameters.DrawTotalRow == false)
            {
                rowDefTotals.Height = new GridLength(0);
                return;
            }

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

            int maxWidth = 100;
            int.TryParse(freqParameters.PercentBarWidth.ToString(), out maxWidth);

            daBar.To = maxWidth;
            daBar.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            rctTotalsBar.BeginAnimation(Rectangle.WidthProperty, daBar);

            if (freqParameters.PercentBarMode == "Bar and percent")
            {
                TextBlock tb = new TextBlock();
                tb.Margin = new Thickness(maxWidth + 5, 0, 0, 0);
                tb.Foreground = this.Resources["cellForeground"] as SolidColorBrush;
                tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                tb.Text = 1.ToString("P0");

                Grid.SetRow(tb, footerRowIndex);
                Grid.SetColumn(tb, 6);
                grid.Children.Add(tb);
            }
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

            messagePanel2.MessagePanelType = Controls.MessagePanelType.WarningPanel;
            messagePanel2.Text += "\n" + errorMessage;
            messagePanel2.Visibility = System.Windows.Visibility.Visible;
            
            HideConfigPanel();
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
                if (count < 300)
                {
                    lower = 0;
                    freq.ExactCI(frequency, (double)count, 95.0, ref lower, ref upper);
                    upper = 1;
                }
            }
            else
            {
                if (count > 300)
                {
                    freq.WILSON(frequency, (double)count, 1.96, ref lower, ref upper);
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
        /// Used to construct the gadget and assign events
        /// </summary>        
        protected override void Construct()
        {
            this.Parameters = new FrequencyParameters();
            if (!string.IsNullOrEmpty(CustomOutputHeading))
            {
                headerPanel.Text = CustomOutputHeading;
            }

            //lbxColumns.SelectAll();

            StrataGridList = new List<Grid>();
            StrataExpanderList = new List<Expander>();
            //checkboxUsePrompts.Checked += new RoutedEventHandler(checkboxUsePrompts_Checked);
            //checkboxUsePrompts.Unchecked += new RoutedEventHandler(checkboxUsePrompts_Unchecked);
           // mnuClone.Click += new RoutedEventHandler(mnuClone_Click);
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
            //txtRows.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            this.IsProcessing = false;
            this.GadgetStatusUpdate += new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(IsCancelled);

            //#region Translation
//Moved to FrequencyProperties.xaml.cs   
            //ConfigExpandedTitle.Text = DashboardSharedStrings.GADGET_CONFIG_TITLE_FREQUENCY;
            //expanderAdvancedOptions.Header = DashboardSharedStrings.GADGET_ADVANCED_OPTIONS;
            //expanderDisplayOptions.Header = DashboardSharedStrings.GADGET_DISPLAY_OPTIONS;
            //tblockMainVariable.Text = DashboardSharedStrings.GADGET_FREQUENCY_VARIABLE;
            //tblockStrataVariable.Text = DashboardSharedStrings.GADGET_STRATA_VARIABLE;
            //tblockWeightVariable.Text = DashboardSharedStrings.GADGET_WEIGHT_VARIABLE;
            //checkboxAllValues.Content = DashboardSharedStrings.GADGET_ALL_LIST_VALUES;
            //checkboxCommentLegalLabels.Content = DashboardSharedStrings.GADGET_LIST_LABELS;
            //checkboxIncludeMissing.Content = DashboardSharedStrings.GADGET_INCLUDE_MISSING;
            //checkboxSortHighLow.Content = DashboardSharedStrings.GADGET_SORT_HI_LOW;
            //checkboxUsePrompts.Content = DashboardSharedStrings.GADGET_USE_FIELD_PROMPT;
            //tblockOutputColumns.Text = DashboardSharedStrings.GADGET_OUTPUT_COLUMNS_DISPLAY;
            //tblockPrecision.Text = DashboardSharedStrings.GADGET_DECIMALS_TO_DISPLAY;

            //tblockRows.Text = DashboardSharedStrings.GADGET_MAX_ROWS_TO_DISPLAY;
            //tblockBarWidth.Text = DashboardSharedStrings.GADGET_MAX_PERCENT_BAR_WIDTH;

            //btnRun.Content = DashboardSharedStrings.GADGET_RUN_BUTTON;
            //#endregion // Translation

            base.Construct();
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
                    //txt.Text = GadgetOptions.MainVariableName;
                    if (Parameters.ColumnNames.Count > 0)
                    {
                        txt.Text = Parameters.ColumnNames[0];
                    }
                }
            }
        }

        /// <summary>
        /// Shows the field's prompt value (if applicable) in the first column heading for the output grid.
        /// </summary>
        private void ShowFieldPrompt()
        {
            int stratagridlistcounter = 1;
            foreach (Grid grid in this.StrataGridList)
            {
                if (stratagridlistcounter++ < this.StrataGridList.Count)
                    continue;
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
                    //Field field = DashboardHelper.GetAssociatedField(GadgetOptions.MainVariableName);
                    Field field = DashboardHelper.GetAssociatedField(Parameters.ColumnNames[0]);
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

        ///// <summary>
        ///// Collapses the output for the gadget
        ///// </summary>
        //public override void CollapseOutput()
        //{
        //    foreach (Expander expander in this.StrataExpanderList)
        //    {
        //        expander.Visibility = System.Windows.Visibility.Collapsed;
        //    }

        //    foreach (Grid grid in this.StrataGridList)
        //    {
        //        grid.Visibility = System.Windows.Visibility.Collapsed;
        //        Border border = new Border();
        //        if (grid.Parent is Border)
        //        {
        //            border = (grid.Parent) as Border;
        //            border.Visibility = System.Windows.Visibility.Collapsed;
        //        }
        //    }

        //    if (!string.IsNullOrEmpty(this.infoPanel.Text))
        //    {
        //        this.infoPanel.Visibility = System.Windows.Visibility.Collapsed;
        //    }

        //    this.messagePanel.Visibility = System.Windows.Visibility.Collapsed;
        //    this.infoPanel.Visibility = System.Windows.Visibility.Collapsed;
        //    descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
        //    IsCollapsed = true;
        //}

        ///// <summary>
        ///// Expands the output for the gadget
        ///// </summary>
        //public override void ExpandOutput()
        //{
        //    foreach (Expander expander in this.StrataExpanderList)
        //    {
        //        expander.Visibility = System.Windows.Visibility.Visible;
        //    }

        //    foreach (Grid grid in this.StrataGridList)
        //    {
        //        grid.Visibility = System.Windows.Visibility.Visible;
        //        Border border = new Border();
        //        if (grid.Parent is Border)
        //        {
        //            border = (grid.Parent) as Border;
        //            border.Visibility = System.Windows.Visibility.Visible;
        //        }
        //    }

        //    if (this.messagePanel.MessagePanelType != Controls.MessagePanelType.StatusPanel)
        //    {
        //        this.messagePanel.Visibility = System.Windows.Visibility.Visible;
        //    }

        //    if (!string.IsNullOrEmpty(this.infoPanel.Text))
        //    {
        //        this.infoPanel.Visibility = System.Windows.Visibility.Visible;
        //    }

        //    if (!string.IsNullOrEmpty(this.descriptionPanel.Text))
        //    {
        //        descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.DisplayMode;
        //    }
        //    IsCollapsed = false;
        //}

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

            //GadgetOptions = null;
            this.Parameters = null;
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

            messagePanel2.Text = "";
             
        }

        #endregion // Private and Protected Methods

        #region Public Methods
        #region IGadget Members
        /// <summary>
        /// Sets the gadget to its 'processing' state
        /// </summary>
        public override void SetGadgetToProcessingState()
        {
            this.IsProcessing = true;
            base.SetGadgetToProcessingState();
        }

        /// <summary>
        /// Sets the gadget to its 'finished' state
        /// </summary>
        public override void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
            currentWidth = borderAll.ActualWidth;
            base.SetGadgetToFinishedState();
        }

        /// <summary>
        /// Initiates a refresh of the gadget's output
        /// </summary>
        public override void RefreshResults()
        {
            FrequencyParameters freqParameters = (FrequencyParameters)Parameters;

            if (!LoadingCombos && freqParameters != null && freqParameters.ColumnNames.Count > 0)
            {
                if (!String.IsNullOrEmpty(freqParameters.ColumnNames[0]))
                {
                    infoPanel.Visibility = System.Windows.Visibility.Collapsed;
                    waitPanel.Visibility = System.Windows.Visibility.Visible;
                    messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
                    descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
                    baseWorker = new BackgroundWorker();
                    baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
                    baseWorker.RunWorkerAsync();
                    base.RefreshResults();
                }
                else if (!LoadingCombos && freqParameters.ColumnNames[0] == String.Empty)
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

        Controls.GadgetProperties.FrequencyProperties properties = null;
        public override void ShowHideConfigPanel()
        {
            Popup = new DashboardPopup();
            Popup.Parent = ((this.Parent as DragCanvas).Parent as ScrollViewer).Parent as Grid;
            properties = new Controls.GadgetProperties.FrequencyProperties(this.DashboardHelper, this, (FrequencyParameters)Parameters, StrataGridList);

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
                properties = new Controls.GadgetProperties.FrequencyProperties(this.DashboardHelper, this, (FrequencyParameters)Parameters, StrataGridList);
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
            Controls.GadgetProperties.FrequencyProperties properties = Popup.Content as Controls.GadgetProperties.FrequencyProperties;
            this.Parameters = properties.Parameters;
            this.DataFilters = properties.DataFilters;
            this.CustomOutputHeading = Parameters.GadgetTitle;
            this.CustomOutputDescription = Parameters.GadgetDescription;
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
            FrequencyParameters freqParameters = (FrequencyParameters)Parameters;

            System.Xml.XmlElement element = doc.CreateElement("frequencyGadget");
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
            type.Value = "EpiDashboard.FrequencyControl";
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

            WordBuilder wb = new WordBuilder(",");          
            if (freqParameters.ShowFrequencyCol) wb.Add("1");
            if (freqParameters.ShowPercentCol) wb.Add("2");
            if (freqParameters.ShowCumPercentCol) wb.Add("3");
            if (freqParameters.Show95CILowerCol) wb.Add("4");
            if (freqParameters.Show95CIUpperCol) wb.Add("5");
            if (freqParameters.ShowPercentBarsCol) wb.Add("6");
           
            this.CustomOutputHeading = freqParameters.GadgetTitle;
            this.CustomOutputDescription = freqParameters.GadgetDescription;

            XmlElement freqVarElements = doc.CreateElement("mainVariables");

            
            foreach (var mainVar in freqParameters.ColumnNames)
            {
            XmlElement freqVarElement = doc.CreateElement("mainVariable");
            if (freqParameters.ColumnNames.Count > 0)
            {
                    if (!String.IsNullOrEmpty(mainVar.ToString()))
                {
                        freqVarElement.InnerText = mainVar.ToString();
                        freqVarElements.AppendChild(freqVarElement);
                    }
                }
            }
            element.AppendChild(freqVarElements);
            // =========  Former Advanced Options section  ============
            //weightVariable
            XmlElement weightVariableElement = doc.CreateElement("weightVariable");
            if (!String.IsNullOrEmpty(freqParameters.WeightVariableName))
            {
                weightVariableElement.InnerText = freqParameters.WeightVariableName;
                element.AppendChild(weightVariableElement);
            }

            XmlElement StrataVariableNameElement = doc.CreateElement("strataVariable");
            XmlElement StrataVariableNamesElement = doc.CreateElement("strataVariables");
            if (freqParameters.StrataVariableNames.Count == 1)
            {
                StrataVariableNameElement.InnerText = freqParameters.StrataVariableNames[0].ToString();
                element.AppendChild(StrataVariableNameElement);
            }
            else if (freqParameters.StrataVariableNames.Count > 1)
            {
                foreach (string strataColumn in freqParameters.StrataVariableNames)
                {
                    XmlElement strataElement = doc.CreateElement("strataVariable");
                    strataElement.InnerText = strataColumn.Replace("<", "&lt;");
                    StrataVariableNamesElement.AppendChild(strataElement);
                }

                element.AppendChild(StrataVariableNamesElement); 
            }

            //showAllListValues
            XmlElement allValuesElement = doc.CreateElement("allValues");
            allValuesElement.InnerText = freqParameters.ShowAllListValues.ToString();
            element.AppendChild(allValuesElement);

            //showListLabels
            XmlElement showListLabelsElement = doc.CreateElement("showListLabels");
            showListLabelsElement.InnerText = freqParameters.ShowCommentLegalLabels.ToString();
            element.AppendChild(showListLabelsElement);

            //sort
            XmlElement sortElement = doc.CreateElement("sort");
            if (freqParameters.SortHighToLow) sortElement.InnerText = "hightolow";
            element.AppendChild(sortElement);

            //includeMissing
            XmlElement includeMissingElement = doc.CreateElement("includeMissing");
            includeMissingElement.InnerText = freqParameters.IncludeMissing.ToString();
            element.AppendChild(includeMissingElement);

            // =========  Former Display Options section  ============

            //useFieldPrompts
            XmlElement useFieldPromptsElement = doc.CreateElement("useFieldPrompts");
            useFieldPromptsElement.InnerText = freqParameters.UseFieldPrompts.ToString();
            element.AppendChild(useFieldPromptsElement);

            //drawBorders
            XmlElement drawBordersElement = doc.CreateElement("drawBorders");
            drawBordersElement.InnerText = freqParameters.DrawBorders.ToString();
            element.AppendChild(drawBordersElement);

            //drawHeaderRow
            XmlElement drawHeaderRowElement = doc.CreateElement("drawHeaderRow");
            drawHeaderRowElement.InnerText = freqParameters.DrawHeaderRow.ToString();
            element.AppendChild(drawHeaderRowElement);

            //drawTotalRow
            XmlElement drawTotalRowElement = doc.CreateElement("drawTotalRow");
            drawTotalRowElement.InnerText = freqParameters.DrawTotalRow.ToString();
            element.AppendChild(drawTotalRowElement);
            
            //precision
            int precision = 2;
            bool precision_success = int.TryParse(freqParameters.Precision.ToString(), out precision);
            XmlElement precisionElement = doc.CreateElement("precision");
            if (precision_success)
            {
                precisionElement.InnerText = precision.ToString();
            }
            else
            {
                precisionElement.InnerText = "2";
                freqParameters.Precision = "2";
            }
            element.AppendChild(precisionElement);

            //rowsToDisplay
            string rowsToDisplay = String.Empty;
            XmlElement rowsToDisplayElement = doc.CreateElement("rowsToDisplay");
            if (freqParameters.RowsToDisplay.HasValue) 
            {
                if (freqParameters.RowsToDisplay > 0)
                {
                    rowsToDisplay = freqParameters.RowsToDisplay.ToString();
                    rowsToDisplayElement.InnerText = rowsToDisplay;
                }
            }
            element.AppendChild(rowsToDisplayElement);

            //percentBarWidth
            XmlElement percentBarWidthElement = doc.CreateElement("percentBarWidth");
            int percentBarWidth = 100;
            bool success = int.TryParse(freqParameters.PercentBarWidth.ToString(), out percentBarWidth);
            if (success)
            {
                percentBarWidthElement.InnerText = freqParameters.PercentBarWidth.ToString();
            }
            else
            {
                percentBarWidthElement.InnerText = "100";
                freqParameters.PercentBarWidth = 100;
            }
            element.AppendChild(percentBarWidthElement);

            //columnsToShow
            XmlElement columnsToShowElement = doc.CreateElement("columnsToShow");
            columnsToShowElement.InnerText = wb.ToString();
            element.AppendChild(columnsToShowElement);

            //customHeading
            XmlElement customHeadingElement = doc.CreateElement("customHeading");
            customHeadingElement.InnerText = freqParameters.GadgetTitle; //CustomOutputHeading.Replace("<", "&lt;");
            element.AppendChild(customHeadingElement);

            //customDescription
            XmlElement customDescriptionElement = doc.CreateElement("customDescription");
            customDescriptionElement.InnerText = freqParameters.GadgetDescription; //CustomOutputDescription.Replace("<", "&lt;");
            element.AppendChild(customDescriptionElement);

            //customCaption
            XmlElement customCaptionElement = doc.CreateElement("customCaption");
            customCaptionElement.InnerText = CustomOutputCaption;
            element.AppendChild(customCaptionElement);

            SerializeAnchors(element);

            return element;
        }

        /// <summary>
        /// Enables and disables output columns based on user selection.
        /// </summary>
        private void ShowHideOutputColumns()
        {
            FrequencyParameters freqParameters = (FrequencyParameters)Parameters;
            if (!LoadingCombos)
            {
                if (this.StrataGridList != null && this.StrataGridList.Count > 0)
                {
                    List<int> columnsToHide = new List<int>();
                    if (!(Parameters as FrequencyParameters).ShowFrequencyCol) columnsToHide.Add(1);
                    if (!(Parameters as FrequencyParameters).ShowPercentCol) columnsToHide.Add(2);
                    if (!(Parameters as FrequencyParameters).ShowCumPercentCol) columnsToHide.Add(3);
                    if (!(Parameters as FrequencyParameters).Show95CILowerCol) columnsToHide.Add(4);
                    if (!(Parameters as FrequencyParameters).Show95CIUpperCol) columnsToHide.Add(5);
                    if (!(Parameters as FrequencyParameters).ShowPercentBarsCol) columnsToHide.Add(6);

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
                                    //grid.ColumnDefinitions[i].Width = GridLength.Auto;
                                    int width = 100;
                                    if (int.TryParse(freqParameters.PercentBarWidth.ToString(), out width))
                                    {
                                        grid.ColumnDefinitions[i].Width = new GridLength(width);
                                    }
                                    else
                                    {
                                        grid.ColumnDefinitions[i].Width = new GridLength(100);
                                    }
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
        /// Creates the frequency gadget from an Xml element
        /// </summary>
        /// <param name="element">The element from which to create the gadget</param>
        public override void CreateFromXml(XmlElement element)
        {
            this.LoadingCombos = true;
            this.Parameters = new FrequencyParameters();
            
            HideConfigPanel();
            
            infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            foreach (XmlAttribute attribute in element.Attributes)
            {
                switch (attribute.Name.ToLower())
                {
                    case "actualheight":
                        string actualHeight = attribute.Value.Replace(',', '.');
                        double controlheight = 0.0;
                        double.TryParse(actualHeight, out controlheight);
                        this.Height = controlheight;
                        break;
                }
            }

            foreach (XmlElement child in element.ChildNodes)
            {
                if (!String.IsNullOrEmpty(child.InnerText.Trim()))
                {

                    switch (child.Name.ToLower())
                    {
                        case "mainvariable":
                            {
                                ((FrequencyParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                            }
                            break;
                        case "mainvariables":
                            foreach (XmlElement field in child.ChildNodes)
                            {
                                List<string> fields = new List<string>();
                                if (field.Name.ToLower().Equals("mainvariable"))
                                {
                                    ((FrequencyParameters)Parameters).ColumnNames.Add(field.InnerText.Replace("&lt;", "<"));
                                }
                            }
                            break;
                        case "weightvariable":
                            ((FrequencyParameters)Parameters).WeightVariableName = child.InnerText.Replace("&lt;", "<");
                            ((FrequencyParameters)Parameters).ColumnNames.Remove(((FrequencyParameters)Parameters).WeightVariableName);
                            break;
                        case "stratavariable":
                            if (Parameters.StrataVariableNames.Count > 0)
                            {
                                ((FrequencyParameters)Parameters).StrataVariableNames[0] = child.InnerText.Replace("&lt;", "<");
                            }
                            else
                            {
                                ((FrequencyParameters)Parameters).StrataVariableNames.Add(child.InnerText.Replace("&lt;", "<"));
                            }
                            break;
                        case "stratavariables":
                            foreach (XmlElement field in child.ChildNodes)
                            {
                                List<string> fields = new List<string>();
                                if (field.Name.ToLower().Equals("stratavariable"))
                                {
                                    ((FrequencyParameters)Parameters).StrataVariableNames.Add(field.InnerText.Replace("&lt;", "<"));
                                }
                            }
                            break;
                        case "allvalues":
                            if (child.InnerText.ToLower().Equals("true"))
                            {
                                ((FrequencyParameters)Parameters).ShowAllListValues = true;
                            }
                            else
                            {
                                ((FrequencyParameters)Parameters).ShowAllListValues = false;
                            }
                            break;
                        case "showlistlabels":
                            if (child.InnerText.ToLower().Equals("true")) 
                            { 
                                ((FrequencyParameters)Parameters).ShowCommentLegalLabels = true; 
                            }
                            else 
                            { 
                                ((FrequencyParameters)Parameters).ShowCommentLegalLabels = false; 
                            }
                            break;
                        case "sort":
                            if (child.InnerText.ToLower().Equals("highlow") || child.InnerText.ToLower().Equals("hightolow"))
                            {
                                ((FrequencyParameters)Parameters).SortHighToLow = true;
                            }
                            else
                            {
                                ((FrequencyParameters)Parameters).SortHighToLow = false;
                            }
                            break;
                        case "includemissing":
                            if (child.InnerText.ToLower().Equals("true"))
                            {
                                ((FrequencyParameters)Parameters).IncludeMissing = true;
                            }
                            else
                            {
                                ((FrequencyParameters)Parameters).IncludeMissing = false;
                            }
                            break;
                        case "usefieldprompts":
                            bool usePrompts = false;
                            bool.TryParse(child.InnerText, out usePrompts);
                            ((FrequencyParameters)Parameters).UseFieldPrompts = usePrompts;
                            break;
                        case "drawborders":
                            bool drawborders = false;
                            bool.TryParse(child.InnerText, out drawborders);
                            ((FrequencyParameters)Parameters).DrawBorders = drawborders;
                            break;
                        case "drawheaderrow":
                            bool drawheaderrow = false;
                            bool.TryParse(child.InnerText, out drawheaderrow);
                            ((FrequencyParameters)Parameters).DrawHeaderRow = drawheaderrow;
                            break;
                        case "drawtotalrow":
                            bool drawtotalrow = false;
                            bool.TryParse(child.InnerText, out drawtotalrow);
                            ((FrequencyParameters)Parameters).DrawTotalRow = drawtotalrow;
                            break;
                        case "precision":
                            int precision = 4;
                            int.TryParse(child.InnerText, out precision);
                            ((FrequencyParameters)Parameters).Precision = precision.ToString();
                            break;
                        case "rowstodisplay":
                            {
                                int rows;
                                bool success = int.TryParse(child.InnerText, out rows);
                                if (success)
                                {
                                    ((FrequencyParameters)Parameters).RowsToDisplay = rows;
                                }
                            }
                            break;
                        case "percentbarwidth":
                            int barwidth;
                            bool parsesuccess = int.TryParse(child.InnerText, out barwidth);
                            if (parsesuccess)
                            {
                                ((FrequencyParameters)Parameters).PercentBarWidth = barwidth;
                            }
                            break;
                        case "columnstoshow":
                            if (string.IsNullOrEmpty(child.InnerText)) 
                            { 
                                ((FrequencyParameters)Parameters).ShowFrequencyCol = true;
                                ((FrequencyParameters)Parameters).ShowPercentCol = true;
                                ((FrequencyParameters)Parameters).ShowCumPercentCol = true;
                                ((FrequencyParameters)Parameters).Show95CILowerCol = true;
                                ((FrequencyParameters)Parameters).Show95CIUpperCol = true;
                                ((FrequencyParameters)Parameters).ShowPercentBarsCol = true;
                            }
                            else
                            {
                                ((FrequencyParameters)Parameters).ShowFrequencyCol = false;
                                ((FrequencyParameters)Parameters).ShowPercentCol = false;
                                ((FrequencyParameters)Parameters).ShowCumPercentCol = false;
                                ((FrequencyParameters)Parameters).Show95CILowerCol = false;
                                ((FrequencyParameters)Parameters).Show95CIUpperCol = false;
                                ((FrequencyParameters)Parameters).ShowPercentBarsCol = false;

                                string[] columnsToShow = child.InnerText.Split(',');
                                foreach (string s in columnsToShow)
                                {
                                    int columnNumber = -1;
                                    bool colsuccess = int.TryParse(s, out columnNumber);
                                    if (colsuccess)
                                    {
                                        switch (columnNumber)
                                        {
                                            case 1:
                                                ((FrequencyParameters)Parameters).ShowFrequencyCol = true;
                                                break;
                                            case 2:
                                                ((FrequencyParameters)Parameters).ShowPercentCol = true;
                                                break;
                                            case 3:
                                                ((FrequencyParameters)Parameters).ShowCumPercentCol = true;
                                                break;
                                            case 4:
                                                ((FrequencyParameters)Parameters).Show95CILowerCol = true;
                                                break;
                                            case 5:
                                                ((FrequencyParameters)Parameters).Show95CIUpperCol = true;
                                                break;
                                            case 6:
                                                ((FrequencyParameters)Parameters).ShowPercentBarsCol = true;
                                                break;
                                        }
                                    }
                                }
                            }
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
            HideConfigPanel();
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0, bool useAlternatingColors = false)
        {
            if (IsCollapsed) return string.Empty;

            FrequencyParameters freqParameters = (FrequencyParameters)Parameters;
            if (freqParameters.ColumnNames.Count() < 1) return string.Empty;

            StringBuilder htmlBuilder = new StringBuilder();
            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            if (CustomOutputHeading == null || (string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)")))
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">Frequency</h2>");
            }
            else if (CustomOutputHeading != "(none)")
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">" + CustomOutputHeading + "</h2>");
            }

            htmlBuilder.AppendLine("<p class=\"gadgetOptions\"><small>");
            htmlBuilder.AppendLine("<em>Frequency variable:</em> <strong>" + freqParameters.ColumnNames[0] + "</strong>");
            htmlBuilder.AppendLine("<br />");

            if (!String.IsNullOrEmpty(freqParameters.WeightVariableName))
            {
                htmlBuilder.AppendLine("<em>Weight variable:</em> <strong>" + freqParameters.WeightVariableName + "</strong>");
                htmlBuilder.AppendLine("<br />");
            }

            if (freqParameters.StrataVariableNames.Count > 0)
            {
                WordBuilder wb = new WordBuilder(", ");
                foreach (string s in freqParameters.StrataVariableNames)
                {
                    wb.Add(s);
                }
                htmlBuilder.AppendLine("<em>Strata variable(s):</em> <strong>" + wb.ToString() + "</strong>");
                htmlBuilder.AppendLine("<br />");
            }

            htmlBuilder.AppendLine("<em>Include missing:</em> <strong>" + freqParameters.IncludeMissing.ToString() + "</strong>");
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

            foreach (Grid grid in this.StrataGridList)
            {
                string gridName = grid.Tag.ToString();

                string summaryText = "This tables represents the frequency of the field " + freqParameters.ColumnNames[0] + ". ";
                if (!string.IsNullOrEmpty(freqParameters.WeightVariableName)) { summaryText += "The field " + freqParameters.WeightVariableName + " has been specified as a weight. "; }
                if (freqParameters.StrataVariableNames.Count > 0) { summaryText += "The frequency data has been stratified. The data in this table is for the strata value " + grid.Tag.ToString() + ". "; }
                summaryText += "Each non-heading row in the table represents one of the distinct frequency values. The last row contains the total.";

                htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" summary=\"" + summaryText + "\">");

                if (string.IsNullOrEmpty(CustomOutputCaption))
                {
                    if (freqParameters.StrataVariableNames.Count > 0)
                    {
                        htmlBuilder.AppendLine("<caption>" + grid.Tag + "</caption>");
                    }
                }
                else
                {
                    htmlBuilder.AppendLine("<caption>" + CustomOutputCaption + "</caption>");
                }

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
                            Rectangle rct = null;
                            foreach (UIElement element in elements)
                            {
                                if (element is TextBlock)
                                {
                                    txt = element as TextBlock;
                                }
                                else if (element is Rectangle)
                                {
                                    rct = element as Rectangle;
                                }
                            }

                            if (rct != null && j > 0 && i > 0)
                            {
                                double barWidth = rct.ActualWidth;
                                htmlBuilder.AppendLine("<td class=\"value\"><div class=\"percentBar\" style=\"width: " + ((int)barWidth * 2).ToString() + "px;\"></td>");
                            }
                            else
                            {
                                string value = "&nbsp;";

                                if (txt != null)
                                {
                                    value = txt.Text;
                                }

                                if (i == grid.RowDefinitions.Count - 1)
                                {
                                    htmlBuilder.AppendLine(tableDataTagOpen + "<b>" + value + "</b>" + tableDataTagClose);
                                }
                                else
                                {
                                    htmlBuilder.AppendLine(tableDataTagOpen + value + tableDataTagClose);
                                }
                            }
                        }

                        if (j >= grid.ColumnDefinitions.Count - 1)
                        {
                            htmlBuilder.AppendLine("</tr>");
                        }
                    }
                }

                htmlBuilder.AppendLine("</table>");
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

        /// <summary>
        /// Returns the gadget's description as a string.
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return "Frequency Gadget";
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
            FrequencyParameters freqParameters = (FrequencyParameters)Parameters;
            if (freqParameters.UseFieldPrompts == true) freqParameters.UseFieldPrompts = false;
            else freqParameters.UseFieldPrompts = true;
            RefreshResults();
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
                DrawFrequencyBordersDelegate drawBorders = new DrawFrequencyBordersDelegate(DrawOutputGridBorders);

                FrequencyParameters freqParameters = (FrequencyParameters)Parameters;
                fields = new List<string>();
                if (freqParameters.StrataVariableNames.Count > 0) //The frequency is stratfied for one variable.
                {
                    fields.Add(freqParameters.ColumnNames[0]);
                }
                else
                {
                    fields.AddRange(freqParameters.ColumnNames);
                }
                

                for (int i = 0; i < fields.Count; i++)
                {
                    freqParameters.ColumnNames.Clear();
                    freqParameters.ColumnNames.Add(fields[i]);


                AddFreqGridDelegate addGrid = new AddFreqGridDelegate(AddFreqGrid);
                SetGridTextDelegate setText = new SetGridTextDelegate(SetGridText);
                AddGridRowDelegate addRow = new AddGridRowDelegate(AddGridRow);
                SetGridBarDelegate setBar = new SetGridBarDelegate(SetGridBar);
                RenderFrequencyHeaderDelegate renderHeader = new RenderFrequencyHeaderDelegate(RenderFrequencyHeader);

                string freqVar = string.Empty;
                string weightVar = freqParameters.WeightVariableName;
                string strataVar = string.Empty;
                List<string> stratas = freqParameters.StrataVariableNames;
                bool showConfLimits = false;
                bool showCumulativePercent = false;
                bool includeMissing = freqParameters.IncludeMissing; 
                int? rowsToDisplay = freqParameters.RowsToDisplay;
                bool showEllipsis = false;
                bool shouldDrawBorders = true;

                if (!String.IsNullOrEmpty(freqParameters.ColumnNames[0]))
                {
                    freqVar = freqParameters.ColumnNames[0];
                }

                if (freqParameters.StrataVariableNames.Count > 0)
                {
                    stratas = freqParameters.StrataVariableNames;
                }

                showCumulativePercent = freqParameters.ShowCumPercentCol;

                shouldDrawBorders = freqParameters.DrawBorders;
               
                string precisionFormat = "F2";
                string precisionPercentFormat = "P2";

                if (!String.IsNullOrEmpty(freqParameters.Precision))
                {
                    precisionFormat = freqParameters.Precision;
                    precisionPercentFormat = "P" + precisionFormat;
                    precisionFormat = "F" + precisionFormat;
                }

                try
                {
                    Configuration config = DashboardHelper.Config;
                    string yesValue = config.Settings.RepresentationOfYes;
                    string noValue = config.Settings.RepresentationOfNo;

                    RequestUpdateStatusDelegate requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
                    CheckForCancellationDelegate checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

                    freqParameters.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                    freqParameters.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);

                    if (this.DataFilters != null && this.DataFilters.Count > 0)
                    {
                        freqParameters.CustomFilter = this.DataFilters.GenerateDataFilterString(false);
                    }
                    else
                    {
                        freqParameters.CustomFilter = string.Empty;
                    }

                    Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables = DashboardHelper.GenerateFrequencyTable(freqParameters /*, freqVar, weightVar, stratas, string.Empty, useAllPossibleValues, sortHighLow, includeMissing, false*/);

                    if (stratifiedFrequencyTables == null || stratifiedFrequencyTables.Count == 0)
                    {
                        //Reset the column names to what were selected by the user
                        freqParameters.ColumnNames = fields.ToList<string>();
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), DashboardSharedStrings.GADGET_MSG_NO_DATA);
                        //this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));                        
                        return;
                    }
                    else if (worker.CancellationPending)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), DashboardSharedStrings.GADGET_MSG_OPERATION_CANCELLED);
                        //this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        return;                        
                    }
                    else
                    {
                        string formatString = string.Empty;

                        foreach (KeyValuePair<DataTable, List<DescriptiveStatistics>> tableKvp in stratifiedFrequencyTables)
                        {
                            string strataValue = tableKvp.Key.TableName;

                            double count = 0;
                            foreach (DescriptiveStatistics ds in tableKvp.Value)
                            {
                                count = count + ds.observations;
                            }

                            if (count == 0 && stratifiedFrequencyTables.Count == 1)
                            {
                                // this is the only table and there are no records, so let the user know
                             //   this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), DashboardSharedStrings.GADGET_MSG_NO_DATA);
                                this.Dispatcher.BeginInvoke(addGrid, DashboardSharedStrings.GADGET_MSG_NO_DATA, freqParameters.ColumnNames[0].ToString() + "  " + DashboardSharedStrings.GADGET_MSG_NO_DATA);

                                continue;
                                //this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));                                
                               // return;
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

                            this.Dispatcher.BeginInvoke(addGrid, strataVar, frequencies.TableName);
                        }

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
                            DataTable frequencies = tableKvp.Key;

                            if (frequencies.Rows.Count == 0)
                            {
                                continue;
                            }

                            string tableHeading = tableKvp.Key.TableName;

                            if (stratifiedFrequencyTables.Count > 1)
                            {
                                tableHeading = freqVar;// +": " + strataVar + " = " + frequencies.TableName;
                                if (freqParameters.UseFieldPrompts)
                                {
                                    Field tmpField = DashboardHelper.GetAssociatedField(freqVar);
                                    if (tmpField != null && tmpField is IDataField)
                                    {
                                        tableHeading = ((IDataField)tmpField).PromptText;
                                    }
                                }
                            }

                            if (!showCumulativePercent)
                            {
                                this.Dispatcher.BeginInvoke(new SimpleCallback(HideCumulativePercent));
                            }

                            if (!showConfLimits)
                            {
                                this.Dispatcher.BeginInvoke(new SimpleCallback(HideConfidenceIntervals));
                            }

                            Field field = null;
                            string columnType = string.Empty;

                            foreach (DataRow fieldRow in DashboardHelper.FieldTable.Rows)
                            {
                                if (fieldRow["columnname"].Equals(freqVar))
                                {
                                    columnType = fieldRow["datatype"].ToString();
                                    if (fieldRow["epifieldtype"] is Field)
                                    {
                                        field = fieldRow["epifieldtype"] as Field;
                                    }
                                    break;
                                }
                            }

                            this.Dispatcher.BeginInvoke(renderHeader, strataValue, tableHeading, count);

                            double AccumulatedTotal = 0;
                            List<ConfLimit> confLimits = new List<ConfLimit>();
                            int rowCount = 1;
                            foreach (System.Data.DataRow row in frequencies.Rows)
                            {
                                if (!row[freqVar].Equals(DBNull.Value) || (row[freqVar].Equals(DBNull.Value) && includeMissing == true))
                                {
                                    if ((rowsToDisplay.HasValue && rowCount <= rowsToDisplay.Value) || rowsToDisplay.HasValue == false)
                                    {
                                        this.Dispatcher.Invoke(addRow, strataValue, 26);
                                    }
                                    string displayValue = row[freqVar].ToString();

                                    if (DashboardHelper.IsUserDefinedColumn(freqVar))
                                    {
                                        displayValue = DashboardHelper.GetFormattedOutput(freqVar, row[freqVar]);
                                    }
                                    else
                                    {
                                        if (field != null && field is YesNoField)
                                        {
                                            if (row[freqVar].ToString().Equals("1"))
                                                displayValue = yesValue;
                                            else if (row[freqVar].ToString().Equals("0"))
                                                displayValue = noValue;
                                        }
                                        else if ((field != null && field is DateField) || (!DashboardHelper.DateColumnRequiresTime(frequencies, frequencies.Columns[0].ColumnName)))
                                        {
                                            displayValue = string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:d}", row[freqVar]);
                                        }
                                        else if (field != null && field is TimeField)
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
                                        displayValue = config.Settings.RepresentationOfMissing;
                                    }

                                    double pct = 0;
                                    if (count > 0)
                                        pct = Convert.ToDouble(row["freq"]) / (count * 1.0);
                                    AccumulatedTotal += pct;

                                    if (count > 0)
                                        confLimits.Add(GetConfLimit(displayValue, Convert.ToDouble(row["freq"]), count));

                                    if ((rowsToDisplay.HasValue && rowCount <= rowsToDisplay.Value) || rowsToDisplay.HasValue == false)
                                    {
                                        this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(displayValue, new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Left, TextAlignment.Left, rowCount, 0, Visibility.Visible));
                                        //this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(row["freq"].ToString(), new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 1, Visibility.Visible));
                                        this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(Math.Round((double)(row["freq"]), int.Parse((Parameters as FrequencyParameters).Precision)).ToString(), new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 1, Visibility.Visible));

                                        this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(pct.ToString(precisionPercentFormat), new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 2, Visibility.Visible));
                                        this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(AccumulatedTotal.ToString(precisionPercentFormat), new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 3, Visibility.Visible));

                                        this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(confLimits[rowCount - 1].Lower.ToString(precisionPercentFormat), new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 4, Visibility.Visible));
                                        this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(confLimits[rowCount - 1].Upper.ToString(precisionPercentFormat), new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 5, Visibility.Visible));
                                        this.Dispatcher.BeginInvoke(setBar, strataValue, rowCount, pct);
                                        rowCount++;
                                    }
                                    else
                                    {
                                        showEllipsis = true;
                                    }                                    
                                }
                            }

                            if (showEllipsis)
                            {
                                this.Dispatcher.Invoke(addRow, strataValue, 26);
                                this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(StringLiterals.ELLIPSIS, new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Left, TextAlignment.Left, rowCount, 0, Visibility.Visible));
                                this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(StringLiterals.ELLIPSIS, new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 1, Visibility.Visible));

                                this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(StringLiterals.ELLIPSIS, new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 2, Visibility.Visible));
                                this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(StringLiterals.ELLIPSIS, new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 3, Visibility.Visible));

                                this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(StringLiterals.ELLIPSIS, new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 4, Visibility.Visible));
                                this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(StringLiterals.ELLIPSIS, new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 5, Visibility.Visible));
                                rowCount++;
                            }

                            this.Dispatcher.BeginInvoke(new AddGridFooterDelegate(RenderFrequencyFooter), strataValue, rowCount, (int)count);
                            if (shouldDrawBorders) this.Dispatcher.BeginInvoke(drawBorders, strataValue);                            
                        }

                        stratifiedFrequencyTables.Clear();
                    }
                    this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));
                    //this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                }
                catch (Exception ex)
                {
                   // if (fields.Count <= 1)
                   // {
                      // // this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
                       
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithWarning), ex.Message);
                        
                  //  }
                   // else
                   // {
                  //      this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                  //  }
                }
                finally
                {
                    stopwatch.Stop();
                    Debug.Print("Frequency gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete with " + DashboardHelper.RecordCount.ToString() + " records and the following filters:");
                    Debug.Print(DashboardHelper.DataFilters.GenerateDataFilterString());
                }
                }

                if (fields.Count > 1)
                {
                    freqParameters.ColumnNames.Clear();
                    freqParameters.ColumnNames.AddRange(fields);
                }


            }
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
