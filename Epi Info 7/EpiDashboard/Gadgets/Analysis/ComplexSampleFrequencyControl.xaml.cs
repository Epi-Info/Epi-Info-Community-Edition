using System;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for ComplexSampleFrequencyControl.xaml
    /// </summary>
    public partial class ComplexSampleFrequencyControl : GadgetBase
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
        private delegate void CreateComplexSampleFrequencyGridDelegate(StatisticsRepository.ComplexSampleTables.CSFrequencyResults results);
        #endregion // Delegates

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public ComplexSampleFrequencyControl()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper object to attach</param>
        public ComplexSampleFrequencyControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            Construct();
            FillComboboxes();
        }

        #endregion // Constructors

        #region Private Methods
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
        /// Handles the filling of the gadget's combo boxes
        /// </summary>
        private void FillComboboxes(bool update = false)
        {
            //LoadingCombos = true;

            //string prevField = string.Empty;
            //string prevWeightField = string.Empty;
            //string prevStrataField = string.Empty;
            //string prevPSUField = string.Empty;

            //if (update)
            //{
            //    if (cbxField.SelectedIndex >= 0)
            //    {
            //        prevField = cbxField.SelectedItem.ToString();
            //    }
            //    if (cbxFieldWeight.SelectedIndex >= 0)
            //    {
            //        prevWeightField = cbxFieldWeight.SelectedItem.ToString();
            //    }
            //    if (cbxFieldStrata.SelectedIndex >= 0)
            //    {
            //        prevStrataField = cbxFieldStrata.SelectedItem.ToString();
            //    }
            //    if (cbxFieldPSU.SelectedIndex >= 0)
            //    {
            //        prevPSUField = cbxFieldPSU.SelectedItem.ToString();
            //    }
            //}

            //cbxField.ItemsSource = null;
            //cbxField.Items.Clear();

            //cbxFieldWeight.ItemsSource = null;
            //cbxFieldWeight.Items.Clear();

            //cbxFieldStrata.ItemsSource = null;
            //cbxFieldStrata.Items.Clear();

            //cbxFieldPSU.ItemsSource = null;
            //cbxFieldPSU.Items.Clear();

            //List<string> fieldNames = new List<string>();
            //List<string> weightFieldNames = new List<string>();
            //List<string> strataFieldNames = new List<string>();

            //weightFieldNames.Add(string.Empty);
            //strataFieldNames.Add(string.Empty);

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

            //cbxField.ItemsSource = fieldNames;
            //cbxFieldWeight.ItemsSource = weightFieldNames;
            //cbxFieldStrata.ItemsSource = strataFieldNames;
            //cbxFieldPSU.ItemsSource = strataFieldNames;

            //if (cbxField.Items.Count > 0)
            //{
            //    cbxField.SelectedIndex = -1;
            //}
            //if (cbxFieldWeight.Items.Count > 0)
            //{
            //    cbxFieldWeight.SelectedIndex = -1;
            //}
            //if (cbxFieldStrata.Items.Count > 0)
            //{
            //    cbxFieldStrata.SelectedIndex = -1;
            //}

            //if (update)
            //{
            //    cbxField.SelectedItem = prevField;
            //    cbxFieldWeight.SelectedItem = prevWeightField;
            //    cbxFieldStrata.SelectedItem = prevStrataField;
            //    cbxFieldPSU.SelectedItem = prevPSUField;
            //}

            //LoadingCombos = false;
        }

        /// <summary>
        /// Sets the gadget's state to 'finished' mode
        /// </summary>
        protected override void RenderFinish()
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed; //waitCursor.Visibility = Visibility.Hidden;

            foreach (Grid freqGrid in StrataGridList)
            {
                freqGrid.Visibility = Visibility.Visible;
            }

            messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
            messagePanel.Text = string.Empty;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            HideConfigPanel();
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
            //Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

            //GadgetOptions.MainVariableName = string.Empty;
            //GadgetOptions.WeightVariableName = string.Empty;
            //GadgetOptions.StrataVariableNames = new List<string>();
            //GadgetOptions.CrosstabVariableName = string.Empty;
            //GadgetOptions.PSUVariableName = string.Empty;
            //GadgetOptions.ColumnNames = new List<string>();

            //if (cbxField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxField.SelectedItem.ToString())
            //    &&
            //    cbxFieldPSU.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldPSU.SelectedItem.ToString()))
            //{
            //    inputVariableList.Add("IdentifierList", cbxField.SelectedItem.ToString());
            //    GadgetOptions.MainVariableName = cbxField.SelectedItem.ToString();
            //    inputVariableList.Add("PSUVar", cbxFieldPSU.SelectedItem.ToString());
            //    GadgetOptions.PSUVariableName = cbxFieldPSU.SelectedItem.ToString();
            //}
            //else
            //{
            //    return;
            //}
            //if (cbxFieldWeight.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldWeight.SelectedItem.ToString()))
            //{
            //    inputVariableList.Add("WeightVar", cbxFieldWeight.SelectedItem.ToString());
            //    GadgetOptions.WeightVariableName = cbxFieldWeight.SelectedItem.ToString();
            //}
            //if (cbxFieldStrata.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldStrata.SelectedItem.ToString()))
            //{
            //    inputVariableList.Add("StratvarList", cbxFieldStrata.SelectedItem.ToString());
            //    GadgetOptions.StrataVariableNames = new List<string>();
            //    GadgetOptions.StrataVariableNames.Add(cbxFieldStrata.SelectedItem.ToString());
            //}
            //GadgetOptions.ShouldIncludeFullSummaryStatistics = false;
            //GadgetOptions.InputVariableList = inputVariableList;
        }

        /// <summary>
        /// Used to construct the gadget and assign events
        /// </summary>        
        protected override void Construct()
        {
            this.Parameters = new ComplexSampleFrequencyParameters();
            if (!string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)"))
            {
                headerPanel.Text = CustomOutputHeading;
            }

            StrataGridList = new List<Grid>();
            StrataExpanderList = new List<Expander>();            

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

            this.IsProcessing = false;

            this.GadgetStatusUpdate += new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(IsCancelled);

            base.Construct();
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

        public override void CollapseOutput()
        {
            foreach (Expander expander in this.StrataExpanderList)
            {
                expander.Visibility = System.Windows.Visibility.Collapsed;
            }

            foreach (Grid grid in this.StrataGridList)
            {
                grid.Visibility = System.Windows.Visibility.Collapsed;
                Border border = new Border();
                if (grid.Parent is Border)
                {
                    border = (grid.Parent) as Border;
                    border.Visibility = System.Windows.Visibility.Collapsed;
                }
            }

            if (!string.IsNullOrEmpty(this.txtFilterString.Text))
            {
                this.txtFilterString.Visibility = System.Windows.Visibility.Collapsed;
            }

            this.messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            this.txtFilterString.Visibility = System.Windows.Visibility.Collapsed;
            //descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed; //EI-24
            IsCollapsed = true;
        }

        public override void ExpandOutput()
        {
            foreach (Expander expander in this.StrataExpanderList)
            {
                expander.Visibility = System.Windows.Visibility.Visible;
            }

            foreach (Grid grid in this.StrataGridList)
            {
                grid.Visibility = System.Windows.Visibility.Visible;
                Border border = new Border();
                if (grid.Parent is Border)
                {
                    border = (grid.Parent) as Border;
                    border.Visibility = System.Windows.Visibility.Visible;
                }
            }

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
        }

        /// <summary>
        /// Shows the Properties Panel for the gadget to set its options and parameters.
        /// </summary>
        public override void ShowHideConfigPanel()
        {
            Popup = new DashboardPopup();
            Popup.Parent = ((this.Parent as DragCanvas).Parent as ScrollViewer).Parent as Grid;
            Controls.GadgetProperties.ComplexSampleFrequencyProperties properties = new Controls.GadgetProperties.ComplexSampleFrequencyProperties(this.DashboardHelper, this, (ComplexSampleFrequencyParameters)Parameters, StrataGridList);

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

        /// <summary>
        /// Accepts changes to the Properties panel and refreshes the results.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void properties_ChangesAccepted(object sender, EventArgs e)
        {
            Controls.GadgetProperties.ComplexSampleFrequencyProperties properties = Popup.Content as Controls.GadgetProperties.ComplexSampleFrequencyProperties;
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

        #endregion // Private Methods        

        #region Public Methods
        /// <summary>
        /// Returns the gadget's description as a string.
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return "Complex Sample Frequency Gadget";
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the click event for the Run button
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            RefreshResults();
        }

        private void AddComplexSampleFrequencyGrid(StatisticsRepository.ComplexSampleTables.CSFrequencyResults results) 
        {
            ComplexSampleFrequencyParameters csfreqParameters = (ComplexSampleFrequencyParameters)Parameters;
            Grid grid = new Grid();
            string mainVarName = String.Empty;
            if (csfreqParameters.ColumnNames.Count > 0)
            {
                mainVarName = csfreqParameters.ColumnNames[0];
            }
            grid.Tag = mainVarName;
            grid.Style = this.Resources["genericOutputGrid"] as Style;
            grid.Margin = (Thickness)this.Resources["genericElementMargin"];
            grid.Visibility = System.Windows.Visibility.Collapsed;
            StrataGridList.Add(grid);

            // Setup grid header
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

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
            txtValHeader.Text = mainVarName;
            txtValHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtValHeader, 0);
            Grid.SetColumn(txtValHeader, 0);
            grid.Children.Add(txtValHeader);

            TextBlock txtTotalHeader = new TextBlock();
            txtTotalHeader.Text = SharedStrings.TOTAL;
            txtTotalHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtTotalHeader, 0);
            Grid.SetColumn(txtTotalHeader, 1);
            grid.Children.Add(txtTotalHeader);

            double runningTotal = 0;

            // Grid rows
            foreach (StatisticsRepository.ComplexSampleTables.CSRow fRow in results.Rows)
            {
                // Value row
                grid.RowDefinitions.Add(new RowDefinition());

                TextBlock txtValueLabel = new TextBlock();
                txtValueLabel.Text = fRow.Value;
                txtValueLabel.FontWeight = FontWeights.Bold;
                txtValueLabel.Margin = (Thickness)this.Resources["genericTextMargin"];
                Grid.SetRow(txtValueLabel, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(txtValueLabel, 0);
                grid.Children.Add(txtValueLabel);

                TextBlock txtValue = new TextBlock();
                txtValue.Text = fRow.Count.ToString();
                txtValue.Margin = (Thickness)this.Resources["genericTextMargin"];
                txtValue.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                Grid.SetRow(txtValue, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(txtValue, 1);
                grid.Children.Add(txtValue);

                // Row % row
                grid.RowDefinitions.Add(new RowDefinition());

                TextBlock txtRowPercentLabel = new TextBlock();
                txtRowPercentLabel.Text = "Row %";
                txtRowPercentLabel.Margin = (Thickness)this.Resources["genericTextMargin"];
                Grid.SetRow(txtRowPercentLabel, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(txtRowPercentLabel, 0);
                grid.Children.Add(txtRowPercentLabel);

                TextBlock txtRowPercent = new TextBlock();
                txtRowPercent.Text = fRow.RowPercent.ToString("F3");
                txtRowPercent.Margin = (Thickness)this.Resources["genericTextMargin"];
                txtRowPercent.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                Grid.SetRow(txtRowPercent, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(txtRowPercent, 1);
                grid.Children.Add(txtRowPercent);

                // Col % row
                grid.RowDefinitions.Add(new RowDefinition());

                TextBlock txtColPercentLabel = new TextBlock();
                txtColPercentLabel.Text = "Col %";
                txtColPercentLabel.Margin = (Thickness)this.Resources["genericTextMargin"];
                Grid.SetRow(txtColPercentLabel, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(txtColPercentLabel, 0);
                grid.Children.Add(txtColPercentLabel);

                TextBlock txtColPercent = new TextBlock();
                txtColPercent.Text = fRow.ColPercent.ToString("F3");
                txtColPercent.Margin = (Thickness)this.Resources["genericTextMargin"];
                txtColPercent.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                Grid.SetRow(txtColPercent, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(txtColPercent, 1);
                grid.Children.Add(txtColPercent);

                // SE % row
                grid.RowDefinitions.Add(new RowDefinition());

                TextBlock txtSEPercentLabel = new TextBlock();
                txtSEPercentLabel.Text = "SE %";
                txtSEPercentLabel.Margin = (Thickness)this.Resources["genericTextMargin"];
                Grid.SetRow(txtSEPercentLabel, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(txtSEPercentLabel, 0);
                grid.Children.Add(txtSEPercentLabel);

                TextBlock txtSEPercent = new TextBlock();
                txtSEPercent.Text = fRow.SE.ToString("F3");
                txtSEPercent.Margin = (Thickness)this.Resources["genericTextMargin"];
                txtSEPercent.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                Grid.SetRow(txtSEPercent, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(txtSEPercent, 1);
                grid.Children.Add(txtSEPercent);

                // LCL % row
                grid.RowDefinitions.Add(new RowDefinition());

                TextBlock txtLCLPercentLabel = new TextBlock();
                txtLCLPercentLabel.Text = "LCL %";
                txtLCLPercentLabel.Margin = (Thickness)this.Resources["genericTextMargin"];
                Grid.SetRow(txtLCLPercentLabel, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(txtLCLPercentLabel, 0);
                grid.Children.Add(txtLCLPercentLabel);

                TextBlock txtLCLPercent = new TextBlock();
                txtLCLPercent.Text = fRow.LCL.ToString("F3");
                txtLCLPercent.Margin = (Thickness)this.Resources["genericTextMargin"];
                txtLCLPercent.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                Grid.SetRow(txtLCLPercent, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(txtLCLPercent, 1);
                grid.Children.Add(txtLCLPercent);

                // UCL % row
                grid.RowDefinitions.Add(new RowDefinition());

                TextBlock txtUCLPercentLabel = new TextBlock();
                txtUCLPercentLabel.Text = "UCL %";
                txtUCLPercentLabel.Margin = (Thickness)this.Resources["genericTextMargin"];
                Grid.SetRow(txtUCLPercentLabel, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(txtUCLPercentLabel, 0);
                grid.Children.Add(txtUCLPercentLabel);

                TextBlock txtUCLPercent = new TextBlock();
                txtUCLPercent.Text = fRow.UCL.ToString("F3");
                txtUCLPercent.Margin = (Thickness)this.Resources["genericTextMargin"];
                txtUCLPercent.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                Grid.SetRow(txtUCLPercent, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(txtUCLPercent, 1);
                grid.Children.Add(txtUCLPercent);

                runningTotal = runningTotal + fRow.Count;
            }

            // Setup grid footer
            RowDefinition rowDefTotal = new RowDefinition();
            RowDefinition rowDefDE = new RowDefinition();

            grid.RowDefinitions.Add(rowDefTotal);
            grid.RowDefinitions.Add(rowDefDE);            

            TextBlock txtTotalFooter = new TextBlock();
            txtTotalFooter.Text = SharedStrings.TOTAL;
            txtTotalFooter.FontWeight = FontWeights.Bold;
            txtTotalFooter.Margin = (Thickness)this.Resources["genericTextMargin"];
            Grid.SetRow(txtTotalFooter, grid.RowDefinitions.Count - 2);
            Grid.SetColumn(txtTotalFooter, 0);
            grid.Children.Add(txtTotalFooter);

            TextBlock txtTotalFooterValue = new TextBlock();
            txtTotalFooterValue.Text = runningTotal.ToString();
            txtTotalFooterValue.Margin = (Thickness)this.Resources["genericTextMargin"];
            txtTotalFooterValue.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            Grid.SetRow(txtTotalFooterValue, grid.RowDefinitions.Count - 2);
            Grid.SetColumn(txtTotalFooterValue, 1);
            grid.Children.Add(txtTotalFooterValue);

            TextBlock txtDEFooter = new TextBlock();
            txtDEFooter.Text = "Design effect";
            txtDEFooter.Margin = (Thickness)this.Resources["genericTextMargin"];            
            Grid.SetRow(txtDEFooter, grid.RowDefinitions.Count - 1);
            Grid.SetColumn(txtDEFooter, 0);
            grid.Children.Add(txtDEFooter);

            TextBlock txtDEFooterValue = new TextBlock();
            txtDEFooterValue.Text = results.Rows[0].DesignEffect.ToString("F4");
            txtDEFooterValue.Margin = (Thickness)this.Resources["genericTextMargin"];
            txtDEFooterValue.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            Grid.SetRow(txtDEFooterValue, grid.RowDefinitions.Count - 1);
            Grid.SetColumn(txtDEFooterValue, 1);
            grid.Children.Add(txtDEFooterValue);

            // Draw grid borders
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

            panelMain.Children.Add(grid);
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
                //Dictionary<string, string> inputVariableList = ((GadgetParameters)e.Argument).InputVariableList;
                ComplexSampleFrequencyParameters csfreqParameters = (ComplexSampleFrequencyParameters)Parameters;
                Dictionary<string, string> inputVariableList = csfreqParameters.InputVariableList;
                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));                
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));
                CreateComplexSampleFrequencyGridDelegate createGrid = new CreateComplexSampleFrequencyGridDelegate(AddComplexSampleFrequencyGrid);

                //string freqVar = GadgetOptions.MainVariableName;
                //string weightVar = GadgetOptions.WeightVariableName;
                //string strataVar = string.Empty;                
                //bool includeMissing = GadgetOptions.ShouldIncludeMissing;
                string freqVar = string.Empty;
                if (!String.IsNullOrEmpty(csfreqParameters.ColumnNames[0]))
                {
                    freqVar = csfreqParameters.ColumnNames[0];
                }
                string weightVar = csfreqParameters.WeightVariableName;
                string strataVar = string.Empty;
                bool includeMissing = false;
                
                if (csfreqParameters.StrataVariableNames.Count > 0 && !String.IsNullOrEmpty(csfreqParameters.StrataVariableNames[0]))
                {
                    strataVar = csfreqParameters.StrataVariableNames[0];
                }
                
                List<string> stratas = new List<string>();
                if (!string.IsNullOrEmpty(strataVar))
                {
                    stratas.Add(strataVar);
                }

                try
                {
                    Configuration config = DashboardHelper.Config;
                    string yesValue = config.Settings.RepresentationOfYes;
                    string noValue = config.Settings.RepresentationOfNo;

                    RequestUpdateStatusDelegate requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
                    CheckForCancellationDelegate checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

                    //GadgetOptions.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                    //GadgetOptions.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);
                    csfreqParameters.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                    csfreqParameters.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);

                    if (this.DataFilters != null && this.DataFilters.Count > 0)
                    {
                        csfreqParameters.CustomFilter = this.DataFilters.GenerateDataFilterString(false);
                    }
                    else
                    {
                        csfreqParameters.CustomFilter = string.Empty;
                    }

                    List<string> columnNames = new List<string>();
                    //columnNames.Add(GadgetOptions.MainVariableName);
                    columnNames.Add(csfreqParameters.ColumnNames[0]); 
                    //columnNames.Add(GadgetOptions.PSUVariableName);
                    columnNames.Add(csfreqParameters.PSUVariableName);
                    //if(GadgetOptions.StrataVariableNames != null && GadgetOptions.StrataVariableNames.Count == 1 && !string.IsNullOrEmpty(GadgetOptions.StrataVariableNames[0])) 
                    if (csfreqParameters.StrataVariableNames != null && csfreqParameters.StrataVariableNames.Count == 1 && !string.IsNullOrEmpty(csfreqParameters.StrataVariableNames[0]))
                    {
                        columnNames.Add(csfreqParameters.StrataVariableNames[0]);
                    }
                    if (!string.IsNullOrEmpty(csfreqParameters.WeightVariableName)) 
                    {
                        columnNames.Add(csfreqParameters.WeightVariableName);
                    }

                    //DataTable csTable = DashboardHelper.GenerateTable(columnNames, GadgetOptions.CustomFilter);
                    DataTable csTable = DashboardHelper.GenerateTable(columnNames, csfreqParameters.CustomFilter);

                    if (csTable == null || csTable.Rows.Count == 0)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.NO_RECORDS_SELECTED);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));                        
                        return;
                    }
                    else if (worker.CancellationPending)
                    {
                        this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), SharedStrings.DASHBOARD_GADGET_STATUS_OPERATION_CANCELLED);
                        this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                        return;                        
                    }
                    else
                    {
                        lock (staticSyncLock)
                        {
                            // Complex sample frequency actually uses the CS Tables class.
                            StatisticsRepository.ComplexSampleTables csFrequency = new StatisticsRepository.ComplexSampleTables();
                            //StatisticsRepository.ComplexSampleTables.CSFrequencyResults results = csFrequency.ComplexSampleFrequencies(GadgetOptions.InputVariableList, csTable);
                            StatisticsRepository.ComplexSampleTables.CSFrequencyResults results = csFrequency.ComplexSampleFrequencies(csfreqParameters.InputVariableList, csTable);
                            if (!string.IsNullOrEmpty(results.ErrorMessage))
                            {
                                throw new ApplicationException(results.ErrorMessage);
                            }

                            if (results.Rows == null)
                            {
                                throw new ApplicationException("No data was returned from the statistics repository.");
                            }
                            this.Dispatcher.BeginInvoke(createGrid, results);
                        }                        
                    }
                    this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
                    this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
                }
                finally
                {
                    //stopwatch.Stop();
                    //Debug.Print("CS Frequency gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete with " + dashboardHelper.RecordCount.ToString() + " records and the following filters:");
                    //Debug.Print(dashboardHelper.DataFilters.GenerateDataFilterString());
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

        #region IGadget Members
        /// <summary>
        /// Sets the gadget to its 'processing' state
        /// </summary>
        public override void SetGadgetToProcessingState()
        {
            this.IsProcessing = true;
            //this.cbxField.IsEnabled = false;
            //this.cbxFieldStrata.IsEnabled = false;
            //this.cbxFieldWeight.IsEnabled = false;
            //this.cbxFieldPSU.IsEnabled = false;
            //this.btnRun.IsEnabled = false;
        }

        /// <summary>
        /// Sets the gadget to its 'finished' state
        /// </summary>
        public override void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
            //this.cbxField.IsEnabled = true;
            //this.cbxFieldStrata.IsEnabled = true;
            //this.cbxFieldWeight.IsEnabled = true;
            //this.cbxFieldPSU.IsEnabled = true;
            //this.btnRun.IsEnabled = true;

            base.SetGadgetToFinishedState();
        }

        /// <summary>
        /// Initiates a refresh of the gadget's output
        /// </summary>
        public override void RefreshResults()
        {
            ComplexSampleFrequencyParameters csfreqParameters = (ComplexSampleFrequencyParameters)Parameters;

            if (!LoadingCombos)
            {
                if (csfreqParameters != null && csfreqParameters.ColumnNames.Count >0  && !String.IsNullOrEmpty(csfreqParameters.ColumnNames[0]) && !String.IsNullOrEmpty(csfreqParameters.PSUVariableName))
                {
                    CreateInputVariableList();
                    txtFilterString.Visibility = System.Windows.Visibility.Collapsed;
                    waitPanel.Visibility = System.Windows.Visibility.Visible;
                    messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
                    descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
                    baseWorker = new BackgroundWorker();
                    baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
                    baseWorker.RunWorkerAsync();
                    base.RefreshResults();
                }
                else if (!LoadingCombos && csfreqParameters.ColumnNames.Count > 0  && String.IsNullOrEmpty(csfreqParameters.ColumnNames[0]))
                {
                    ClearResults();
                    waitPanel.Visibility = System.Windows.Visibility.Collapsed;
                }
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
            //CreateInputVariableList();
            ComplexSampleFrequencyParameters csfreqParameters = (ComplexSampleFrequencyParameters)Parameters;

            Dictionary<string, string> inputVariableList = csfreqParameters.InputVariableList;

            string freqVar = string.Empty;
            string strataVar = string.Empty;
            string weightVar = string.Empty;
            string sort = string.Empty;

            WordBuilder wb = new WordBuilder(",");
            System.Xml.XmlElement element = doc.CreateElement("complexSampleFrequencyGadget");
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
            collapsed.Value = IsCollapsed.ToString();
            type.Value = "EpiDashboard.ComplexSampleFrequencyControl";

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);
            element.Attributes.Append(id);

            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            XmlElement freqVarElement = doc.CreateElement("mainVariable");
            if (csfreqParameters.ColumnNames.Count > 0)
            {
                if (!String.IsNullOrEmpty(csfreqParameters.ColumnNames[0].ToString()))
                {
                    freqVarElement.InnerText = csfreqParameters.ColumnNames[0].ToString();
                    element.AppendChild(freqVarElement);
                }
            }
            XmlElement StrataVariableNameElement = doc.CreateElement("strataVariable");
            XmlElement StrataVariableNamesElement = doc.CreateElement("strataVariables");
            if (csfreqParameters.StrataVariableNames.Count == 1)
            {
                StrataVariableNameElement.InnerText = csfreqParameters.StrataVariableNames[0].ToString();
                element.AppendChild(StrataVariableNameElement);
            }
            else if (csfreqParameters.StrataVariableNames.Count > 1)
            {
                //string xmlstrataVarString = string.Empty;
                //foreach (string strataColumn in freqParameters.StrataVariableNames)
                //{
                //    xmlstrataVarString = xmlstrataVarString + "<strataVariable>" + strataColumn.Replace("<", "&lt;") + "</strataVariable>";
                //}
                //StrataVariableNamesElement.InnerXml = xmlstrataVarString;
                //element.AppendChild(StrataVariableNamesElement); 

                foreach (string strataColumn in csfreqParameters.StrataVariableNames)
                {
                    XmlElement strataElement = doc.CreateElement("strataVariable");
                    strataElement.InnerText = strataColumn.Replace("<", "&lt;");
                    StrataVariableNamesElement.AppendChild(strataElement);
                }

                element.AppendChild(StrataVariableNamesElement);
            }
            // =========  Former Advanced Options section  ============
            //weightVariable
            XmlElement weightVariableElement = doc.CreateElement("weightVariable");
            if (!String.IsNullOrEmpty(csfreqParameters.WeightVariableName))
            {
                weightVariableElement.InnerText = csfreqParameters.WeightVariableName;
                element.AppendChild(weightVariableElement);
            }

            XmlElement psuVariableElement = doc.CreateElement("psuVariable");
            if (!String.IsNullOrEmpty(csfreqParameters.PSUVariableName))
            {
                psuVariableElement.InnerText = csfreqParameters.PSUVariableName;
                element.AppendChild(psuVariableElement);
            }

            //if (GadgetOptions.StrataVariableNames != null && GadgetOptions.StrataVariableNames.Count == 1)
            //{
            //    strataVar = GadgetOptions.StrataVariableNames[0];
            //}

            //customHeading
            XmlElement customHeadingElement = doc.CreateElement("customHeading");
            customHeadingElement.InnerText = csfreqParameters.GadgetTitle; //CustomOutputHeading.Replace("<", "&lt;");
            element.AppendChild(customHeadingElement);

            //customDescription
            XmlElement customDescriptionElement = doc.CreateElement("customDescription");
            customDescriptionElement.InnerText = csfreqParameters.GadgetDescription; //CustomOutputDescription.Replace("<", "&lt;");
            element.AppendChild(customDescriptionElement);

            //customCaption
            XmlElement customCaptionElement = doc.CreateElement("customCaption");
            customCaptionElement.InnerText = CustomOutputCaption;
            element.AppendChild(customCaptionElement);

            //xmlString =
            //"<mainVariable>" + GadgetOptions.MainVariableName + "</mainVariable>" +
            //"<strataVariable>" + strataVar + "</strataVariable>" +
            //"<weightVariable>" + GadgetOptions.WeightVariableName + "</weightVariable>" +
            //"<psuVariable>" + GadgetOptions.PSUVariableName + "</psuVariable>" +
            //"<customHeading>" + CustomOutputHeading.Replace("<", "&lt;") + "</customHeading>" +
            //"<customDescription>" + CustomOutputDescription.Replace("<", "&lt;") + "</customDescription>" +
            //"<customCaption>" + CustomOutputCaption + "</customCaption>";

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
            this.Parameters = new ComplexSampleFrequencyParameters();
            Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

            HideConfigPanel();

            txtFilterString.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLower())
                {
                    case "mainvariable":
                        {
                            //cbxField.Text = child.InnerText.Replace("&lt;", "<");
                            ((ComplexSampleFrequencyParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                            inputVariableList.Add("IdentifierList", child.InnerText.Replace("&lt;", "<"));
                        }
                        break;
                    case "weightvariable":
                        //cbxFieldWeight.Text = child.InnerText.Replace("&lt;", "<");
                        ((ComplexSampleFrequencyParameters)Parameters).WeightVariableName = child.InnerText.Replace("&lt;", "<");
                        inputVariableList.Add("WeightVar", child.InnerText.Replace("&lt;", "<"));
                        break;
                    case "psuvariable":
                        //cbxFieldPSU.Text = child.InnerText.Replace("&lt;", "<");
                        ((ComplexSampleFrequencyParameters)Parameters).PSUVariableName = child.InnerText.Replace("&lt;", "<");
                        inputVariableList.Add("PSUVar", child.InnerText.Replace("&lt;", "<"));
                        break;
                    case "stratavariable":
                        //lbxFieldStrata.SelectedItems.Add(child.InnerText.Replace("&lt;", "<"));
                        if (string.IsNullOrEmpty(child.InnerText))
                        {
                            ((ComplexSampleFrequencyParameters)Parameters).StrataVariableNames[0] = child.InnerText.Replace("&lt;", "<");
                            inputVariableList.Add("StratvarList", child.InnerText.Replace("&lt;", "<"));
                        }
                        break;
                    case "stratavariables":
                        foreach (XmlElement field in child.ChildNodes)
                        {
                            List<string> fields = new List<string>();
                            if (field.Name.ToLower().Equals("stratavariable"))
                            {
                                //lbxFieldStrata.SelectedItems.Add(field.InnerText.Replace("&lt;", "<"));
                                ((ComplexSampleFrequencyParameters)Parameters).StrataVariableNames.Add(field.InnerText.Replace("&lt;", "<"));
                                inputVariableList.Add("StratvarList", child.InnerText.Replace("&lt;", "<"));
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

            base.CreateFromXml(element);
            this.LoadingCombos = false;

            Parameters.InputVariableList = inputVariableList;

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
            ComplexSampleFrequencyParameters csfreqParameters = (ComplexSampleFrequencyParameters)Parameters;

            StringBuilder htmlBuilder = new StringBuilder();

            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            if (CustomOutputHeading == null || (string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)")))
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">Complex Sample Frequency</h2>");
            }
            else if(CustomOutputHeading != "(none)")
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">" + CustomOutputHeading + "</h2>");
            }

            htmlBuilder.AppendLine("<p class=\"gadgetOptions\"><small>");
            htmlBuilder.AppendLine("<em>Frequency variable:</em> <strong>" + csfreqParameters.ColumnNames[0] + "</strong>");
            htmlBuilder.AppendLine("<br />");
            htmlBuilder.AppendLine("<em>PSU variable:</em> <strong>" + csfreqParameters.PSUVariableName + "</strong>");
            htmlBuilder.AppendLine("<br />");

            if (!String.IsNullOrEmpty(csfreqParameters.WeightVariableName))
            {
                htmlBuilder.AppendLine("<em>Weight variable:</em> <strong>" + csfreqParameters.WeightVariableName + "</strong>");
                htmlBuilder.AppendLine("<br />");
            }
            if (csfreqParameters.StrataVariableNames.Count > 0)
            {
                WordBuilder wb = new WordBuilder(", ");
                foreach (string s in csfreqParameters.StrataVariableNames)
                {
                    wb.Add(s);
                }
                htmlBuilder.AppendLine("<em>Strata variable:</em> <strong>" + wb.ToString() + "</strong>");
                htmlBuilder.AppendLine("<br />");
            }
            //htmlBuilder.AppendLine("<em>Include missing:</em> <strong>" + checkboxIncludeMissing.IsChecked.ToString() + "</strong>");
            //htmlBuilder.AppendLine("<br />");
            htmlBuilder.AppendLine("</small></p>");

            if (!string.IsNullOrEmpty(CustomOutputDescription))
            {
                htmlBuilder.AppendLine("<p class=\"gadgetsummary\">" + CustomOutputDescription + "</p>");
            }

            if (!string.IsNullOrEmpty(messagePanel.Text) && messagePanel.Visibility == Visibility.Visible)
            {
                htmlBuilder.AppendLine("<p><small><strong>" + messagePanel.Text + "</strong></small></p>");
            }

            if (!string.IsNullOrEmpty(txtFilterString.Text) && txtFilterString.Visibility == Visibility.Visible)
            {
                htmlBuilder.AppendLine("<p><small><strong>" + txtFilterString.Text + "</strong></small></p>");
            }

            foreach (Grid grid in this.StrataGridList)
            {
                string gridName = grid.Tag.ToString();               

                htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");

                foreach (UIElement control in grid.Children)
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

                        if (columnNumber >= grid.ColumnDefinitions.Count - 1)
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
        #endregion  
    }
}
