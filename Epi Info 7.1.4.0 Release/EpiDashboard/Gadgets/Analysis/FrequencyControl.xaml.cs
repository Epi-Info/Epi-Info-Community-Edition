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
                    if (!lbxColumns.SelectedItems.Contains("Frequency")) columnsToHide.Add(1);
                    if (!lbxColumns.SelectedItems.Contains("Percent")) columnsToHide.Add(2);
                    if (!lbxColumns.SelectedItems.Contains("Cumulative Percent")) columnsToHide.Add(3);
                    if (!lbxColumns.SelectedItems.Contains("95% CI Lower")) columnsToHide.Add(4);
                    if (!lbxColumns.SelectedItems.Contains("95% CI Upper")) columnsToHide.Add(5);
                    if (!lbxColumns.SelectedItems.Contains("Percent bars")) columnsToHide.Add(6);

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
                                    if (int.TryParse(txtBarWidth.Text, out width))
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
        /// Handles the filling of the gadget's combo boxes
        /// </summary>
        private void FillComboboxes(bool update = false)
        {
            LoadingCombos = true;

            string prevField = string.Empty;
            string prevWeightField = string.Empty;
            List<string> prevStrataFields = new List<string>();

            if (lbxColumns.Items.Count == 0)
            {
                lbxColumns.Items.Add("Frequency");
                lbxColumns.Items.Add("Percent");
                lbxColumns.Items.Add("Cumulative Percent");
                lbxColumns.Items.Add("95% CI Lower");
                lbxColumns.Items.Add("95% CI Upper");
                lbxColumns.Items.Add("Percent bars");
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
                foreach (string s in lbxFieldStrata.SelectedItems)
                {
                    prevStrataFields.Add(s);
                }
            }

            cbxField.ItemsSource = null;
            cbxField.Items.Clear();

            cbxFieldWeight.ItemsSource = null;
            cbxFieldWeight.Items.Clear();

            lbxFieldStrata.ItemsSource = null;
            lbxFieldStrata.Items.Clear();

            List<string> fieldNames = new List<string>();
            List<string> weightFieldNames = new List<string>();
            List<string> strataFieldNames = new List<string>();

            weightFieldNames.Add(string.Empty);

            ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.DateTime | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.UserDefined;
            fieldNames = DashboardHelper.GetFieldsAsList(columnDataType);

            columnDataType = ColumnDataType.Numeric | ColumnDataType.UserDefined;
            weightFieldNames.AddRange(DashboardHelper.GetFieldsAsList(columnDataType));

            columnDataType = ColumnDataType.Numeric | ColumnDataType.Boolean | ColumnDataType.Text | ColumnDataType.UserDefined;
            strataFieldNames.AddRange(DashboardHelper.GetFieldsAsList(columnDataType));

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

            cbxField.ItemsSource = fieldNames;
            cbxFieldWeight.ItemsSource = weightFieldNames;
            lbxFieldStrata.ItemsSource = strataFieldNames;

            if (cbxField.Items.Count > 0)
            {
                cbxField.SelectedIndex = -1;
            }
            if (cbxFieldWeight.Items.Count > 0)
            {
                cbxFieldWeight.SelectedIndex = -1;
            }

            if (update)
            {
                cbxField.SelectedItem = prevField;
                cbxFieldWeight.SelectedItem = prevWeightField;

                foreach (string s in prevStrataFields)
                {
                    lbxFieldStrata.SelectedItems.Add(s);
                }
            }

            LoadingCombos = false;
        }

        /// <summary>
        /// Used to add a new FREQ grid to the gadget's output
        /// </summary>
        /// <param name="strataVar">The name of the stratification variable selected, if any</param>
        /// <param name="value">The value by which this grid has been stratified by</param>
        private void AddFreqGrid(string strataVar, string value)
        {
            Grid grid = new Grid();            
            grid.Tag = value;
            grid.Style = this.Resources["genericOutputGrid"] as Style;
            grid.Visibility = System.Windows.Visibility.Collapsed;

            Border border = new Border();
            border.Style = this.Resources["genericOutputGridBorderWithMargins"] as Style;
            border.Child = grid;

            Expander expander = new Expander();

            TextBlock txtExpanderHeader = new TextBlock();
            txtExpanderHeader.Text = value;
            txtExpanderHeader.Style = this.Resources["genericOutputExpanderText"] as Style;
            expander.Header = txtExpanderHeader;

            if (string.IsNullOrEmpty(strataVar) && GadgetOptions.StrataVariableNames.Count == 0)            
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
            if (int.TryParse(txtBarWidth.Text, out width))
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
            Grid grid = GetStrataGrid(strataValue);

            Rectangle rctBar = new Rectangle();
            rctBar.Width = 0.1;// pct * 100.0;
            rctBar.Fill = this.Resources["frequencyPercentBarBrush"] as SolidColorBrush;
            rctBar.HorizontalAlignment = HorizontalAlignment.Left;
            rctBar.Margin = new Thickness(2,6,2,6); //.HorizontalAlignment = HorizontalAlignment.Left;

            Grid.SetRow(rctBar, rowNumber);
            Grid.SetColumn(rctBar, /*4*/6);
            grid.Children.Add(rctBar);

            int maxWidth = 100;
            int.TryParse(txtBarWidth.Text, out maxWidth);

            DoubleAnimation daBar = new DoubleAnimation();
            daBar.From = 1;
            daBar.To = pct * maxWidth;//100.0;
            daBar.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            rctBar.BeginAnimation(Rectangle.WidthProperty, daBar);

            if (cmbPercentBarMode.SelectedIndex > 0)
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
            Grid grid = GetStrataGrid(strataValue);

            RowDefinition rowDefHeader = new RowDefinition();
            rowDefHeader.Height = new GridLength(30);            

            grid.RowDefinitions.Add(rowDefHeader);

            if (checkboxDrawHeader.IsChecked == false)
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

            if (checkboxUsePrompts.IsChecked == true) ShowFieldPrompt();
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
                txtCILowHeader.Text = DashboardSharedStrings.COL_HEADER_FLEISS_CI_LOWER;
            txtCILowHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtCILowHeader, 0);
            Grid.SetColumn(txtCILowHeader, 4);
            grid.Children.Add(txtCILowHeader);

            TextBlock txtCIUpperHeader = new TextBlock();
            if (observationCount < 300)
                txtCIUpperHeader.Text = DashboardSharedStrings.COL_HEADER_EXACT_CI_UPPER;
            else
                txtCIUpperHeader.Text = DashboardSharedStrings.COL_HEADER_FLEISS_CI_UPPER;
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

            if (checkboxDrawFooter.IsChecked == false)
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
            int.TryParse(txtBarWidth.Text, out maxWidth);

            daBar.To = maxWidth;
            daBar.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            rctTotalsBar.BeginAnimation(Rectangle.WidthProperty, daBar);

            if (cmbPercentBarMode.SelectedIndex > 0)
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
        private void RenderFinish()
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
        private void RenderFinishWithWarning(string errorMessage)
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
        private void RenderFinishWithError(string errorMessage)
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
            Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

            GadgetOptions.MainVariableName = string.Empty;
            GadgetOptions.WeightVariableName = string.Empty;
            GadgetOptions.StrataVariableNames = new List<string>();
            GadgetOptions.CrosstabVariableName = string.Empty;
            GadgetOptions.ColumnNames = new List<string>();

            if (cbxField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxField.SelectedItem.ToString()))
            {
                inputVariableList.Add("freqvar", cbxField.SelectedItem.ToString());
                GadgetOptions.MainVariableName = cbxField.SelectedItem.ToString();
            }
            else
            {
                return;
            }

            if (cbxFieldWeight.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldWeight.SelectedItem.ToString()))
            {
                inputVariableList.Add("weightvar", cbxFieldWeight.SelectedItem.ToString());
                GadgetOptions.WeightVariableName = cbxFieldWeight.SelectedItem.ToString();
            }            

            if (lbxFieldStrata.SelectedItems.Count > 0)
            {                
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

            if (checkboxSortHighLow.IsChecked == true)
            {
                inputVariableList.Add("sort", "highlow");
                GadgetOptions.ShouldSortHighToLow = true;
            }
            else
            {
                GadgetOptions.ShouldSortHighToLow = false;
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

            if (!inputVariableList.ContainsKey("usepromptforfield"))
            {
                if (checkboxUsePrompts.IsChecked == true)
                {
                    inputVariableList.Add("usepromptforfield", "true");
                }
                else
                {
                    inputVariableList.Add("usepromptforfield", "false");
                }
            }

            if (cbxFieldPrecision.SelectedIndex >= 0)
            {
                inputVariableList.Add("precision", cbxFieldPrecision.SelectedIndex.ToString());
            }

            if (checkboxDrawBorders.IsChecked == true)
            {
                inputVariableList.Add("drawborders", "true");
            }
            else
            {
                inputVariableList.Add("drawborders", "false");
            }

            GadgetOptions.ShouldIncludeFullSummaryStatistics = false;
            GadgetOptions.InputVariableList = inputVariableList;
            if (string.IsNullOrEmpty(txtRows.Text))
            {
                GadgetOptions.RowsToDisplay = null;
            }
            else
            {
                int rows;
                bool success = int.TryParse(txtRows.Text, out rows);
                if (success)
                {
                    GadgetOptions.RowsToDisplay = rows;
                }
                else
                {
                    GadgetOptions.RowsToDisplay = null;
                    txtRows.Text = string.Empty;
                }
            }
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

            lbxColumns.SelectAll();

            StrataGridList = new List<Grid>();
            StrataExpanderList = new List<Expander>();
            checkboxUsePrompts.Checked += new RoutedEventHandler(checkboxUsePrompts_Checked);
            checkboxUsePrompts.Unchecked += new RoutedEventHandler(checkboxUsePrompts_Unchecked);

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
            txtRows.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            this.IsProcessing = false;
            this.GadgetStatusUpdate += new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(IsCancelled);

            #region Translation
            ConfigExpandedTitle.Text = DashboardSharedStrings.GADGET_CONFIG_TITLE_FREQUENCY;
            expanderAdvancedOptions.Header = DashboardSharedStrings.GADGET_ADVANCED_OPTIONS;
            expanderDisplayOptions.Header = DashboardSharedStrings.GADGET_DISPLAY_OPTIONS;
            tblockMainVariable.Text = DashboardSharedStrings.GADGET_FREQUENCY_VARIABLE;
            tblockStrataVariable.Text = DashboardSharedStrings.GADGET_STRATA_VARIABLE;
            tblockWeightVariable.Text = DashboardSharedStrings.GADGET_WEIGHT_VARIABLE;
            checkboxAllValues.Content = DashboardSharedStrings.GADGET_ALL_LIST_VALUES;
            checkboxCommentLegalLabels.Content = DashboardSharedStrings.GADGET_LIST_LABELS;
            checkboxIncludeMissing.Content = DashboardSharedStrings.GADGET_INCLUDE_MISSING;
            checkboxSortHighLow.Content = DashboardSharedStrings.GADGET_SORT_HI_LOW;
            checkboxUsePrompts.Content = DashboardSharedStrings.GADGET_USE_FIELD_PROMPT;
            tblockOutputColumns.Text = DashboardSharedStrings.GADGET_OUTPUT_COLUMNS_DISPLAY;
            tblockPrecision.Text = DashboardSharedStrings.GADGET_DECIMALS_TO_DISPLAY;

            tblockRows.Text = DashboardSharedStrings.GADGET_MAX_ROWS_TO_DISPLAY;
            tblockBarWidth.Text = DashboardSharedStrings.GADGET_MAX_PERCENT_BAR_WIDTH;

            btnRun.Content = DashboardSharedStrings.GADGET_RUN_BUTTON;
            #endregion // Translation

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

        /// <summary>
        /// Checks the selected variables and enables/disables checkboxes as appropriate
        /// </summary>
        private void CheckVariables()
        {
            isDropDownList = false;
            isCommentLegal = false;
            isOptionField = false;
            isRecoded = false;

            if (cbxField.SelectedItem != null && !string.IsNullOrEmpty(cbxField.SelectedItem.ToString()))
            {
                foreach (DataRow fieldRow in DashboardHelper.FieldTable.Rows)
                {
                    if (fieldRow["columnname"].Equals(cbxField.SelectedItem.ToString()))
                    {
                        if (fieldRow["epifieldtype"] is TableBasedDropDownField || fieldRow["epifieldtype"] is YesNoField || fieldRow["epifieldtype"] is CheckBoxField)
                        {
                            isDropDownList = true;
                            if (fieldRow["epifieldtype"] is DDLFieldOfCommentLegal)
                            {
                                isCommentLegal = true;
                            }
                        }
                        else if (fieldRow["epifieldtype"] is OptionField)
                        {
                            isOptionField = true;
                        }
                        break;
                    }
                }

                if (DashboardHelper.IsUserDefinedColumn(cbxField.SelectedItem.ToString()))
                {
                    List<IDashboardRule> associatedRules = DashboardHelper.Rules.GetRules(cbxField.SelectedItem.ToString());
                    foreach (IDashboardRule rule in associatedRules)
                    {
                        if (rule is Rule_Recode)
                        {
                            isRecoded = true;
                        }
                    }
                }
            }

            if (IsDropDownList || IsRecoded)
            {
                checkboxAllValues.IsEnabled = true;
            }
            else
            {
                checkboxAllValues.IsEnabled = false;
                checkboxAllValues.IsChecked = false;
            }

            if (IsCommentLegal || IsOptionField)
            {
                checkboxCommentLegalLabels.IsEnabled = true;
            }
            else
            {
                checkboxCommentLegalLabels.IsEnabled = false;
            }

            if (!IsCommentLegal && !IsOptionField)
            {
                checkboxCommentLegalLabels.IsChecked = isCommentLegal;
            }   
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
            this.cbxField.IsEnabled = false;
            this.lbxFieldStrata.IsEnabled = false;
            this.cbxFieldWeight.IsEnabled = false;
            this.cbxFieldPrecision.IsEnabled = false;
            this.checkboxIncludeMissing.IsEnabled = false;
            this.checkboxSortHighLow.IsEnabled = false;
            this.checkboxUsePrompts.IsEnabled = false;
            this.lbxColumns.IsEnabled = false;
            this.checkboxCommentLegalLabels.IsEnabled = false;
            this.checkboxAllValues.IsEnabled = false;
            this.btnRun.IsEnabled = false;
        }

        /// <summary>
        /// Sets the gadget to its 'finished' state
        /// </summary>
        public override void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
            this.cbxField.IsEnabled = true;
            this.lbxFieldStrata.IsEnabled = true;
            this.cbxFieldWeight.IsEnabled = true;
            this.cbxFieldPrecision.IsEnabled = true;
            this.checkboxIncludeMissing.IsEnabled = true;
            this.checkboxSortHighLow.IsEnabled = true;
            this.checkboxUsePrompts.IsEnabled = true;
            this.lbxColumns.IsEnabled = true;
            this.btnRun.IsEnabled = true;

            if (IsDropDownList || IsRecoded)
            {
                this.checkboxAllValues.IsEnabled = true;
            }

            if (IsCommentLegal || IsOptionField)
            {
                this.checkboxCommentLegalLabels.IsEnabled = true;
            }

            base.SetGadgetToFinishedState();
        }

        /// <summary>
        /// Initiates a refresh of the gadget's output
        /// </summary>
        public override void RefreshResults()
        {
            if (!LoadingCombos && GadgetOptions != null && cbxField.SelectedIndex > -1)
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
            else if (!LoadingCombos && cbxField.SelectedIndex == -1)
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
            CreateInputVariableList();

            Dictionary<string, string> inputVariableList = GadgetOptions.InputVariableList;

            string freqVar = string.Empty;
            string strataVar = string.Empty;
            string weightVar = string.Empty;
            string sort = string.Empty;
            bool allValues = false;
            bool showConfLimits = true;
            bool showCumulativePercent = true;
            bool includeMissing = false;            

            WordBuilder wb = new WordBuilder(",");
            if (lbxColumns.SelectedItems.Contains("Frequency")) wb.Add("1");
            if (lbxColumns.SelectedItems.Contains("Percent")) wb.Add("2");
            if (lbxColumns.SelectedItems.Contains("Cumulative Percent")) wb.Add("3");
            if (lbxColumns.SelectedItems.Contains("95% CI Lower")) wb.Add("4");
            if (lbxColumns.SelectedItems.Contains("95% CI Upper")) wb.Add("5");
            if (lbxColumns.SelectedItems.Contains("Percent bars")) wb.Add("6");

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

            int precision = 4;
            if (cbxFieldPrecision.SelectedIndex >= 0)
            {
                precision = cbxFieldPrecision.SelectedIndex;
            }

            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            string xmlString =            
            "<mainVariable>" + freqVar + "</mainVariable>";
            
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
            "<sort>" + sort + "</sort>" +
            "<allValues>" + allValues + "</allValues>" +
            "<precision>" + precision.ToString() + "</precision>" +            
            "<showListLabels>" + checkboxCommentLegalLabels.IsChecked + "</showListLabels>" +
            "<useFieldPrompts>" + checkboxUsePrompts.IsChecked.ToString() + "</useFieldPrompts>" +
            "<columnsToShow>" + wb.ToString() + "</columnsToShow>" +
            "<includeMissing>" + includeMissing + "</includeMissing>" +
            "<customHeading>" + CustomOutputHeading.Replace("<", "&lt;") + "</customHeading>" +
            "<customDescription>" + CustomOutputDescription.Replace("<", "&lt;") + "</customDescription>" +
            "<customCaption>" + CustomOutputCaption + "</customCaption>";

            if (!string.IsNullOrEmpty(txtRows.Text))
            {
                xmlString += "<rowsToDisplay>" + txtRows.Text + "</rowsToDisplay>";
            }

            xmlString = xmlString + SerializeAnchors();

            System.Xml.XmlElement element = doc.CreateElement("frequencyGadget");
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
            collapsed.Value = IsCollapsed.ToString();
            type.Value = "EpiDashboard.FrequencyControl";

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);
            element.Attributes.Append(id);

            return element;
        }

        /// <summary>
        /// Creates the frequency gadget from an Xml element
        /// </summary>
        /// <param name="element">The element from which to create the gadget</param>
        public override void CreateFromXml(XmlElement element)
        {
            this.LoadingCombos = true;
            HideConfigPanel();
            infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLower())
                {
                    case "mainvariable":
                        cbxField.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "stratavariable":
                        lbxFieldStrata.SelectedItems.Add(child.InnerText.Replace("&lt;", "<"));
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
                    case "weightvariable":
                        cbxFieldWeight.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "precision":
                        int precision = 4;
                        int.TryParse(child.InnerText, out precision);
                        cbxFieldPrecision.SelectedIndex = precision;
                        break;
                    case "rowstodisplay":
                        int rows;
                        bool success = int.TryParse(child.InnerText, out rows);
                        if (success)
                        {
                            txtRows.Text = rows.ToString();
                        }                        
                        break;
                    case "sort":
                        if (child.InnerText.ToLower().Equals("highlow"))
                        {
                            checkboxSortHighLow.IsChecked = true;
                        }
                        break;
                    case "allvalues":
                        if (child.InnerText.ToLower().Equals("true")) { checkboxAllValues.IsChecked = true; }
                        else { checkboxAllValues.IsChecked = false; }
                        break;
                    case "showlistlabels":
                        if (child.InnerText.ToLower().Equals("true")) { checkboxCommentLegalLabels.IsChecked = true; }
                        else { checkboxCommentLegalLabels.IsChecked = false; }
                        break;
                    case "columnstoshow":
                        if (string.IsNullOrEmpty(child.InnerText)) { lbxColumns.SelectedItems.Clear(); }
                        else
                        {
                            lbxColumns.SelectedItems.Clear();
                            string[] columnsToShow = child.InnerText.Split(',');
                            foreach (string s in columnsToShow)
                            {
                                int columnNumber = -1;
                                success = int.TryParse(s, out columnNumber);
                                if (success)
                                {
                                    lbxColumns.SelectedItems.Add(lbxColumns.Items[columnNumber - 1]);
                                }
                            }
                        }
                        break;
                    case "includemissing":
                        if (child.InnerText.ToLower().Equals("true")) { checkboxIncludeMissing.IsChecked = true; }
                        else { checkboxIncludeMissing.IsChecked = false; }
                        break;
                    case "usefieldprompts":
                        bool usePrompts = false;
                        bool.TryParse(child.InnerText, out usePrompts);
                        checkboxUsePrompts.IsChecked = usePrompts;
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
                        this.DataFilters = new DataFilters(this.DashboardHelper);
                        this.DataFilters.CreateFromXml(child);
                        break;
                }
            }

            base.CreateFromXml(element);
            
            this.LoadingCombos = false;
            CheckVariables();
            RefreshResults();
            HideConfigPanel();
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
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">Frequency</h2>");
            }
            else if (CustomOutputHeading != "(none)")
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">" + CustomOutputHeading + "</h2>");
            }

            htmlBuilder.AppendLine("<p class=\"gadgetOptions\"><small>");
            htmlBuilder.AppendLine("<em>Frequency variable:</em> <strong>" + cbxField.Text + "</strong>");
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
                htmlBuilder.AppendLine("<em>Strata variable(s):</em> <strong>" + wb.ToString() + "</strong>");
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

            foreach (Grid grid in this.StrataGridList)
            {
                string gridName = grid.Tag.ToString();

                string summaryText = "This tables represents the frequency of the field " + cbxField.Text + ". ";
                if (!string.IsNullOrEmpty(cbxFieldWeight.Text)) { summaryText += "The field " + cbxFieldWeight.Text + " has been specified as a weight. "; }
                if (lbxFieldStrata.SelectedItems.Count > 0) { summaryText += "The frequency data has been stratified. The data in this table is for the strata value " + grid.Tag.ToString() + ". "; }
                summaryText += "Each non-heading row in the table represents one of the distinct frequency values. The last row contains the total.";

                htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" summary=\"" + summaryText + "\">");

                if (string.IsNullOrEmpty(CustomOutputCaption))
                {
                    if (lbxFieldStrata.SelectedItems.Count > 0)
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
            if (checkboxUsePrompts.IsChecked == true) checkboxUsePrompts.IsChecked = false;
            else checkboxUsePrompts.IsChecked = true;
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

            CheckVariables();
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
                Dictionary<string, string> inputVariableList = ((GadgetParameters)e.Argument).InputVariableList;
            
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));                
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));

                AddFreqGridDelegate addGrid = new AddFreqGridDelegate(AddFreqGrid);
                SetGridTextDelegate setText = new SetGridTextDelegate(SetGridText);
                AddGridRowDelegate addRow = new AddGridRowDelegate(AddGridRow);
                SetGridBarDelegate setBar = new SetGridBarDelegate(SetGridBar);
                RenderFrequencyHeaderDelegate renderHeader = new RenderFrequencyHeaderDelegate(RenderFrequencyHeader);
                DrawFrequencyBordersDelegate drawBorders = new DrawFrequencyBordersDelegate(DrawOutputGridBorders);

                string freqVar = GadgetOptions.MainVariableName;
                string weightVar = GadgetOptions.WeightVariableName;
                string strataVar = string.Empty;
                bool showConfLimits = false;
                bool showCumulativePercent = false;
                bool includeMissing = GadgetOptions.ShouldIncludeMissing;
                int? rowsToDisplay = GadgetOptions.RowsToDisplay;
                bool showEllipsis = false;
                bool shouldDrawBorders = true;
                
                if (inputVariableList.ContainsKey("stratavar"))
                {
                    strataVar = inputVariableList["stratavar"];
                }
                if (inputVariableList.ContainsKey("showconflimits"))
                {
                    if (inputVariableList["showconflimits"].Equals("true"))
                    {
                        showConfLimits = true;
                    }
                }
                if (inputVariableList.ContainsKey("showcumulativepercent"))
                {
                    if (inputVariableList["showcumulativepercent"].Equals("true"))
                    {
                        showCumulativePercent = true;
                    }
                }
                if (inputVariableList.ContainsKey("drawborders") && inputVariableList["drawborders"].Equals("false")) 
                    shouldDrawBorders = false;
                
                List<string> stratas = new List<string>();
                if (!string.IsNullOrEmpty(strataVar))
                {
                    stratas.Add(strataVar);
                }

                string precisionFormat = "F2";
                string precisionPercentFormat = "P2";

                if (GadgetOptions.InputVariableList.ContainsKey("precision"))
                {
                    precisionFormat = GadgetOptions.InputVariableList["precision"];
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

                    Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables = DashboardHelper.GenerateFrequencyTable(GadgetOptions/*, freqVar, weightVar, stratas, string.Empty, useAllPossibleValues, sortHighLow, includeMissing, false*/);

                    if (stratifiedFrequencyTables == null || stratifiedFrequencyTables.Count == 0)
                    {
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
                                this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), DashboardSharedStrings.GADGET_MSG_NO_DATA);
                                //this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));                                
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
                                        this.Dispatcher.BeginInvoke(setText, strataValue, new TextBlockConfig(row["freq"].ToString(), new Thickness(4, 0, 4, 0), VerticalAlignment.Center, HorizontalAlignment.Right, TextAlignment.Right, rowCount, 1, Visibility.Visible));

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
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
                    //this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                }
                finally
                {
                    stopwatch.Stop();
                    Debug.Print("Frequency gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete with " + DashboardHelper.RecordCount.ToString() + " records and the following filters:");
                    Debug.Print(DashboardHelper.DataFilters.GenerateDataFilterString());
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
