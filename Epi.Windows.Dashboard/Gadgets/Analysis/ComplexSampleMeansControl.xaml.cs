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
using Epi.WPF.Dashboard;
using Epi.WPF.Dashboard.Rules;

namespace Epi.WPF.Dashboard.Gadgets.Analysis
{
    /// <summary>
    /// Interaction logic for ComplexSampleMeansControl.xaml
    /// </summary>
    public partial class ComplexSampleMeansControl : GadgetBase
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
        private delegate void CreateComplexSampleMeansGridDelegate(StatisticsRepository.ComplexSampleMeans.CSMeansResults results);
        #endregion // Delegates

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public ComplexSampleMeansControl()
        {
            InitializeComponent();
            Construct();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper object to attach</param>
        public ComplexSampleMeansControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.dashboardHelper = dashboardHelper;
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
            foreach (Grid grid in this.strataGridList)
            {
                string gridName = grid.Tag.ToString();
                if (strataGridList.Count > 1)
                {
                    sb.AppendLine(grid.Tag.ToString());
                }
                for (int i = 3; i < grid.RowDefinitions.Count; i++)
                {
                    for (int j = 0; j < grid.ColumnDefinitions.Count; j++)
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

                        string value = string.Empty;

                        if (txt != null)
                        {
                            value = txt.Text;
                        }

                        sb.Append(value + "\t");

                        if (j >= grid.ColumnDefinitions.Count - 1)
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
            string prevStrataField = string.Empty;
            string prevPSUField = string.Empty;
            string prevCrosstabField = string.Empty;

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
                if (cbxFieldStrata.SelectedIndex >= 0)
                {
                    prevStrataField = cbxFieldStrata.SelectedItem.ToString();
                }
                if (cbxFieldPSU.SelectedIndex >= 0)
                {
                    prevPSUField = cbxFieldPSU.SelectedItem.ToString();
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

            cbxFieldStrata.ItemsSource = null;
            cbxFieldStrata.Items.Clear();

            cbxFieldPSU.ItemsSource = null;
            cbxFieldPSU.Items.Clear();

            cbxFieldCrosstab.ItemsSource = null;
            cbxFieldCrosstab.Items.Clear();

            List<string> fieldNames = new List<string>();
            List<string> weightFieldNames = new List<string>();
            List<string> strataFieldNames = new List<string>();

            weightFieldNames.Add(string.Empty);
            strataFieldNames.Add(string.Empty);

            ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.DateTime | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.UserDefined | ColumnDataType.UserDefined;
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

            cbxField.ItemsSource = fieldNames;
            cbxFieldWeight.ItemsSource = weightFieldNames;
            cbxFieldStrata.ItemsSource = strataFieldNames;
            cbxFieldPSU.ItemsSource = strataFieldNames;
            cbxFieldCrosstab.ItemsSource = strataFieldNames;

            if (cbxField.Items.Count > 0)
            {
                cbxField.SelectedIndex = -1;
            }
            if (cbxFieldWeight.Items.Count > 0)
            {
                cbxFieldWeight.SelectedIndex = -1;
            }
            if (cbxFieldStrata.Items.Count > 0)
            {
                cbxFieldStrata.SelectedIndex = -1;
            }
            if (cbxFieldCrosstab.Items.Count > 0)
            {
                cbxFieldCrosstab.SelectedIndex = -1;
            }

            if (update)
            {
                cbxField.SelectedItem = prevField;
                cbxFieldWeight.SelectedItem = prevWeightField;
                cbxFieldStrata.SelectedItem = prevStrataField;
                cbxFieldPSU.SelectedItem = prevPSUField;
                cbxFieldCrosstab.SelectedItem = prevCrosstabField;
            }

            LoadingCombos = false;
        }

        /// <summary>
        /// Sets the gadget's state to 'finished' mode
        /// </summary>
        private void RenderFinish()
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed; //waitCursor.Visibility = Visibility.Hidden;

            foreach (Grid freqGrid in strataGridList)
            {
                freqGrid.Visibility = Visibility.Visible;
            }

            messagePanel.MessagePanelType = Controls.MessagePanelType.StatusPanel;
            messagePanel.Text = string.Empty;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

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

            foreach (Grid freqGrid in strataGridList)
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
        /// Used to generate the list of variables and options for the GadgetParameters object
        /// </summary> 
        private void CreateInputVariableList()
        {
            Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

            GadgetOptions.MainVariableName = string.Empty;
            GadgetOptions.WeightVariableName = string.Empty;
            GadgetOptions.StrataVariableNames = new List<string>();
            GadgetOptions.CrosstabVariableName = string.Empty;
            GadgetOptions.PSUVariableName = string.Empty;
            GadgetOptions.ColumnNames = new List<string>();

            if (cbxField.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxField.SelectedItem.ToString())
                &&
                cbxFieldPSU.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldPSU.SelectedItem.ToString()))
            {
                inputVariableList.Add("Numeric_Variable", cbxField.SelectedItem.ToString());
                GadgetOptions.MainVariableName = cbxField.SelectedItem.ToString();
                inputVariableList.Add("PSUVar", cbxFieldPSU.SelectedItem.ToString());
                GadgetOptions.PSUVariableName = cbxFieldPSU.SelectedItem.ToString();                
            }
            else
            {
                return;
            }

            if (cbxFieldCrosstab.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldCrosstab.SelectedItem.ToString()))
            {
                inputVariableList.Add("Cross_Tabulation_Variable", cbxFieldCrosstab.SelectedItem.ToString());
                GadgetOptions.CrosstabVariableName = cbxFieldCrosstab.SelectedItem.ToString();
            }

            if (cbxFieldWeight.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldWeight.SelectedItem.ToString()))
            {
                inputVariableList.Add("WeightVar", cbxFieldWeight.SelectedItem.ToString());
                GadgetOptions.WeightVariableName = cbxFieldWeight.SelectedItem.ToString();
            }
            if (cbxFieldStrata.SelectedIndex > -1 && !string.IsNullOrEmpty(cbxFieldStrata.SelectedItem.ToString()))
            {
                inputVariableList.Add("StratvarList", cbxFieldStrata.SelectedItem.ToString());
                GadgetOptions.StrataVariableNames = new List<string>();
                GadgetOptions.StrataVariableNames.Add(cbxFieldStrata.SelectedItem.ToString());
            }
            GadgetOptions.ShouldIncludeFullSummaryStatistics = false;
            GadgetOptions.InputVariableList = inputVariableList;
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

            strataGridList = new List<Grid>();
            strataExpanderList = new List<Expander>();            

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

            //checkboxCommentLegalLabels.Checked += new RoutedEventHandler(checkboxCheckChanged);
            //checkboxCommentLegalLabels.Unchecked += new RoutedEventHandler(checkboxCheckChanged);

            //checkboxIncludeMissing.Checked += new RoutedEventHandler(checkboxCheckChanged);
            //checkboxIncludeMissing.Unchecked += new RoutedEventHandler(checkboxCheckChanged);

            //checkboxSortHighLow.Checked += new RoutedEventHandler(checkboxCheckChanged);
            //checkboxSortHighLow.Unchecked += new RoutedEventHandler(checkboxCheckChanged);

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
            foreach (Expander expander in this.strataExpanderList)
            {
                expander.Visibility = System.Windows.Visibility.Collapsed;
            }

            foreach (Grid grid in this.strataGridList)
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
            descriptionPanel.PanelMode = Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
        }

        public override void ExpandOutput()
        {
            foreach (Expander expander in this.strataExpanderList)
            {
                expander.Visibility = System.Windows.Visibility.Visible;
            }

            foreach (Grid grid in this.strataGridList)
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
            for (int i = 0; i < strataExpanderList.Count; i++)
            {
                strataExpanderList[i].Content = null;
            }
            this.strataExpanderList.Clear();
            this.strataGridList.Clear();
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

            foreach (Grid grid in strataGridList)
            {
                grid.Children.Clear();
                grid.RowDefinitions.Clear();
                if (grid.Parent is Border)
                {
                    Border border = (grid.Parent) as Border;                    
                    panelMain.Children.Remove(border);
                }                
            }

            foreach (Expander expander in strataExpanderList)
            {
                if (panelMain.Children.Contains(expander))
                {
                    panelMain.Children.Remove(expander);
                }
            }

            panelMain.Children.Clear();

            strataGridList.Clear();
            strataExpanderList.Clear();
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
                foreach (DataRow fieldRow in dashboardHelper.FieldTable.Rows)
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

                if (dashboardHelper.IsUserDefinedColumn(cbxField.SelectedItem.ToString()))
                {
                    List<IDashboardRule> associatedRules = dashboardHelper.Rules.GetRules(cbxField.SelectedItem.ToString());
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

        private void AddComplexSampleMeansGrid(StatisticsRepository.ComplexSampleMeans.CSMeansResults results) 
        {
            Grid grid = new Grid();
            grid.Tag = GadgetOptions.MainVariableName;
            grid.Style = this.Resources["genericOutputGrid"] as Style;
            grid.Margin = (Thickness)this.Resources["genericElementMargin"];
            grid.Visibility = System.Windows.Visibility.Collapsed;
            strataGridList.Add(grid);

            // Setup grid header
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());            

            RowDefinition rowDefHeader = new RowDefinition();
            rowDefHeader.Height = new GridLength(30);
            grid.RowDefinitions.Add(rowDefHeader);

            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());

            Rectangle rctHeader = new Rectangle();
            rctHeader.Style = this.Resources["gridHeaderCellRectangle"] as Style;
            Grid.SetRow(rctHeader, 0);
            Grid.SetRowSpan(rctHeader, 3);
            Grid.SetColumn(rctHeader, 0);
            Grid.SetColumnSpan(rctHeader, 8);
            grid.Children.Add(rctHeader);

            TextBlock txtValHeader = new TextBlock();
            txtValHeader.Text = GadgetOptions.MainVariableName;
            txtValHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtValHeader, 0);
            Grid.SetColumn(txtValHeader, 1);
            Grid.SetColumnSpan(txtValHeader, 7);
            grid.Children.Add(txtValHeader);

            TextBlock txtCrosstabHeader = new TextBlock();
            txtCrosstabHeader.Text = GadgetOptions.CrosstabVariableName;
            txtCrosstabHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtCrosstabHeader, 1);
            Grid.SetRowSpan(txtCrosstabHeader, 2);
            Grid.SetColumn(txtCrosstabHeader, 0);
            grid.Children.Add(txtCrosstabHeader);

            TextBlock txtCountHeader = new TextBlock();
            txtCountHeader.Text = "Count";
            txtCountHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtCountHeader, 1);
            Grid.SetRowSpan(txtCountHeader, 2);
            Grid.SetColumn(txtCountHeader, 1);
            grid.Children.Add(txtCountHeader);

            TextBlock txtMeanHeader = new TextBlock();
            txtMeanHeader.Text = "Mean";
            txtMeanHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtMeanHeader, 1);
            Grid.SetRowSpan(txtMeanHeader, 2);
            Grid.SetColumn(txtMeanHeader, 2);
            grid.Children.Add(txtMeanHeader);

            TextBlock txtStdErrorHeader = new TextBlock();
            txtStdErrorHeader.Text = "Std Error";
            txtStdErrorHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtStdErrorHeader, 1);
            Grid.SetRowSpan(txtStdErrorHeader, 2);
            Grid.SetColumn(txtStdErrorHeader, 3);
            grid.Children.Add(txtStdErrorHeader);

            TextBlock txtCIHeader = new TextBlock();
            txtCIHeader.Text = "Confidence Limits";
            txtCIHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtCIHeader, 1);
            Grid.SetColumn(txtCIHeader, 4);
            Grid.SetColumnSpan(txtCIHeader, 2);
            grid.Children.Add(txtCIHeader);

            TextBlock txtLowerHeader = new TextBlock();
            txtLowerHeader.Text = "Lower";
            txtLowerHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtLowerHeader, 2);            
            Grid.SetColumn(txtLowerHeader, 4);
            grid.Children.Add(txtLowerHeader);

            TextBlock txtUpperHeader = new TextBlock();
            txtUpperHeader.Text = "Upper";
            txtUpperHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtUpperHeader, 2);            
            Grid.SetColumn(txtUpperHeader, 5);
            grid.Children.Add(txtUpperHeader);

            TextBlock txtMinHeader = new TextBlock();
            txtMinHeader.Text = "Minimum";
            txtMinHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtMinHeader, 1);
            Grid.SetRowSpan(txtMinHeader, 2);
            Grid.SetColumn(txtMinHeader, 6);
            grid.Children.Add(txtMinHeader);

            TextBlock txtMaxHeader = new TextBlock();
            txtMaxHeader.Text = "Maximum";
            txtMaxHeader.Style = this.Resources["columnHeadingText"] as Style;
            Grid.SetRow(txtMaxHeader, 1);
            Grid.SetRowSpan(txtMaxHeader, 2);
            Grid.SetColumn(txtMaxHeader, 7);
            grid.Children.Add(txtMaxHeader);

            string unknown = "----";
            string blank = string.Empty;

            foreach (StatisticsRepository.ComplexSampleMeans.MeansRow mRow in results.Rows)
            {
                grid.RowDefinitions.Add(new RowDefinition());

                TextBlock txtRowLabel = new TextBlock();
                txtRowLabel.Text = mRow.Label;
                txtRowLabel.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                //txtRowLabel.Style = this.Resources["columnHeadingText"] as Style;
                Grid.SetRow(txtRowLabel, grid.RowDefinitions.Count - 1);                
                Grid.SetColumn(txtRowLabel, 0);
                grid.Children.Add(txtRowLabel);

                TextBlock txtRowCount = new TextBlock();
                txtRowCount.Text = mRow.Count.HasValue == true ? mRow.Count.Value.ToString("F4") : blank;
                if (txtRowCount.Text.EndsWith(".0000"))
                {
                    txtRowCount.Text = mRow.Count.Value.ToString("F0");
                }
                txtRowCount.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                txtRowCount.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                //txtRowCount.Style = this.Resources["columnHeadingText"] as Style;
                Grid.SetRow(txtRowCount, grid.RowDefinitions.Count - 1);                
                Grid.SetColumn(txtRowCount, 1);
                grid.Children.Add(txtRowCount);

                TextBlock txtRowMean = new TextBlock();
                txtRowMean.Text = mRow.Mean.HasValue == true ? mRow.Mean.Value.ToString("F4") : unknown;
                txtRowMean.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                txtRowMean.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                //txtRowMean.Style = this.Resources["columnHeadingText"] as Style;
                Grid.SetRow(txtRowMean, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(txtRowMean, 2);
                grid.Children.Add(txtRowMean);

                TextBlock txtRowStdError = new TextBlock();
                txtRowStdError.Text = mRow.StdErr.HasValue == true ? mRow.StdErr.Value.ToString("F4") : unknown;
                txtRowStdError.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                txtRowStdError.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                //txtRowStdError.Style = this.Resources["columnHeadingText"] as Style;
                Grid.SetRow(txtRowStdError, grid.RowDefinitions.Count - 1);                
                Grid.SetColumn(txtRowStdError, 3);
                grid.Children.Add(txtRowStdError);                

                TextBlock txtRowLower = new TextBlock();
                txtRowLower.Text = mRow.LCL.HasValue == true ? mRow.LCL.Value.ToString("F4") : unknown;
                txtRowLower.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                txtRowLower.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                //txtRowLower.Style = this.Resources["columnHeadingText"] as Style;
                Grid.SetRow(txtRowLower, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(txtRowLower, 4);
                grid.Children.Add(txtRowLower);

                TextBlock txtRowUpper = new TextBlock();
                txtRowUpper.Text = mRow.UCL.HasValue == true ? mRow.UCL.Value.ToString("F4") : unknown;
                txtRowUpper.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                txtRowUpper.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                //txtRowUpper.Style = this.Resources["columnHeadingText"] as Style;
                Grid.SetRow(txtRowUpper, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(txtRowUpper, 5);
                grid.Children.Add(txtRowUpper);

                TextBlock txtRowMin = new TextBlock();
                txtRowMin.Text = mRow.Min.HasValue == true ? mRow.Min.Value.ToString("F4") : blank;
                txtRowMin.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                txtRowMin.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                //txtRowMin.Style = this.Resources["columnHeadingText"] as Style;
                Grid.SetRow(txtRowMin, grid.RowDefinitions.Count - 1);                
                Grid.SetColumn(txtRowMin, 6);
                grid.Children.Add(txtRowMin);

                TextBlock txtRowMax = new TextBlock();
                txtRowMax.Text = mRow.Max.HasValue == true ? mRow.Max.Value.ToString("F4") : blank;
                txtRowMax.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                txtRowMax.Margin = (Thickness)this.Resources["genericTextMarginAlt"];
                //txtRowMax.Style = this.Resources["columnHeadingText"] as Style;
                Grid.SetRow(txtRowMax, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(txtRowMax, 7);
                grid.Children.Add(txtRowMax);
            }

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

                    if (rdcount == 0 && cdcount > 0 && cdcount < grid.ColumnDefinitions.Count - 1)
                    {
                        border.BorderThickness = new Thickness(0, border.BorderThickness.Bottom, 0, border.BorderThickness.Bottom);
                    }

                    if (rdcount == 1 && (cdcount < 4 || cdcount > 5))
                    {
                        border.BorderThickness = new Thickness(border.BorderThickness.Left, border.BorderThickness.Top, border.BorderThickness.Right, 0);
                    }

                    if (rdcount == 1 && cdcount == 4)
                    {
                        border.BorderThickness = new Thickness(border.BorderThickness.Left, border.BorderThickness.Top, 0, border.BorderThickness.Bottom);
                    }
                    if (rdcount == 0 && cdcount == 0)
                    {
                        border.BorderThickness = new Thickness(1, 1, 1, 1);
                    }

                    //if (rdcount == 0 && cdcount == 0)
                    //{
                    //    border.BorderThickness = new Thickness(1, 1, 1, 0);
                    //}
                    //else if (rdcount == 0 && cdcount == grid.ColumnDefinitions.Count - 1)
                    //{
                    //    border.BorderThickness = new Thickness(border.BorderThickness.Left, border.BorderThickness.Top, border.BorderThickness.Right, 0);
                    //}

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
                Dictionary<string, string> inputVariableList = ((GadgetParameters)e.Argument).InputVariableList;
            
                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));                
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));
                CreateComplexSampleMeansGridDelegate createGrid = new CreateComplexSampleMeansGridDelegate(AddComplexSampleMeansGrid);

                string freqVar = GadgetOptions.MainVariableName;
                string weightVar = GadgetOptions.WeightVariableName;
                string strataVar = string.Empty;                
                bool includeMissing = GadgetOptions.ShouldIncludeMissing;
                
                if (inputVariableList.ContainsKey("stratavar"))
                {
                    strataVar = inputVariableList["stratavar"];
                }
                
                List<string> stratas = new List<string>();
                if (!string.IsNullOrEmpty(strataVar))
                {
                    stratas.Add(strataVar);
                }

                try
                {
                    Configuration config = dashboardHelper.Config;
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

                    List<string> columnNames = new List<string>();
                    columnNames.Add(GadgetOptions.MainVariableName);
                    columnNames.Add(GadgetOptions.PSUVariableName);
                    if(GadgetOptions.StrataVariableNames != null && GadgetOptions.StrataVariableNames.Count == 1 && !string.IsNullOrEmpty(GadgetOptions.StrataVariableNames[0])) 
                    {
                        columnNames.Add(GadgetOptions.StrataVariableNames[0]);
                    }
                    if(!string.IsNullOrEmpty(GadgetOptions.WeightVariableName)) 
                    {
                        columnNames.Add(GadgetOptions.WeightVariableName);
                    }
                    if (!string.IsNullOrEmpty(GadgetOptions.CrosstabVariableName))
                    {
                        columnNames.Add(GadgetOptions.CrosstabVariableName);
                    }

                    DataTable csTable = dashboardHelper.GenerateTable(columnNames, GadgetOptions.CustomFilter);

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
                            StatisticsRepository.ComplexSampleMeans csMeans = new StatisticsRepository.ComplexSampleMeans();
                            StatisticsRepository.ComplexSampleMeans.CSMeansResults results = csMeans.ComplexSampleMeans(GadgetOptions.InputVariableList, csTable);

                            if (!string.IsNullOrEmpty(results.ErrorMessage))
                            {
                                throw new ApplicationException(results.ErrorMessage);
                            }

                            if (results.Rows == null || results.Rows.Count == 0)
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
            this.cbxField.IsEnabled = false;
            this.cbxFieldStrata.IsEnabled = false;
            this.cbxFieldWeight.IsEnabled = false;
            this.cbxFieldPSU.IsEnabled = false;
            this.cbxFieldCrosstab.IsEnabled = false;
            this.btnRun.IsEnabled = false;
        }

        /// <summary>
        /// Sets the gadget to its 'finished' state
        /// </summary>
        public override void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
            this.cbxField.IsEnabled = true;
            this.cbxFieldStrata.IsEnabled = true;
            this.cbxFieldWeight.IsEnabled = true;
            this.cbxFieldPSU.IsEnabled = true;
            this.cbxFieldCrosstab.IsEnabled = true;
            this.btnRun.IsEnabled = true;

            base.SetGadgetToFinishedState();
        }

        /// <summary>
        /// Initiates a refresh of the gadget's output
        /// </summary>
        public override void RefreshResults()
        {
            if (!LoadingCombos && GadgetOptions != null && cbxField.SelectedIndex > -1 && cbxFieldPSU.SelectedIndex > -1)
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

            WordBuilder wb = new WordBuilder(",");

            if (GadgetOptions.StrataVariableNames != null && GadgetOptions.StrataVariableNames.Count == 1)
            {
                strataVar = GadgetOptions.StrataVariableNames[0];
            }

            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            string xmlString =
            "<mainVariable>" + GadgetOptions.MainVariableName.Replace("<", "&lt;") + "</mainVariable>" +
            "<strataVariable>" + strataVar.Replace("<", "&lt;") + "</strataVariable>" +
            "<weightVariable>" + GadgetOptions.WeightVariableName.Replace("<", "&lt;") + "</weightVariable>" +
            "<psuVariable>" + GadgetOptions.PSUVariableName.Replace("<", "&lt;") + "</psuVariable>" +
            "<crosstabVariable>" + GadgetOptions.CrosstabVariableName.Replace("<", "&lt;") + "</crosstabVariable>" +
            "<customHeading>" + CustomOutputHeading.Replace("<", "&lt;") + "</customHeading>" +
            "<customDescription>" + CustomOutputDescription.Replace("<", "&lt;") + "</customDescription>" +
            "<customCaption>" + CustomOutputCaption + "</customCaption>";

            System.Xml.XmlElement element = doc.CreateElement("complexSampleMeansGadget");
            element.InnerXml = xmlString;
            element.AppendChild(SerializeFilters(doc));

            System.Xml.XmlAttribute locationY = doc.CreateAttribute("top");
            System.Xml.XmlAttribute locationX = doc.CreateAttribute("left");
            System.Xml.XmlAttribute collapsed = doc.CreateAttribute("collapsed");
            System.Xml.XmlAttribute type = doc.CreateAttribute("gadgetType");

            locationY.Value = Canvas.GetTop(this).ToString("F0");
            locationX.Value = Canvas.GetLeft(this).ToString("F0");
            collapsed.Value = "false"; // currently no way to collapse the gadget, so leave this 'false' for now
            type.Value = "Epi.WPF.Dashboard.Gadgets.Analysis.ComplexSampleMeansControl";

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

            HideConfigPanel();

            txtFilterString.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLower())
                {
                    case "mainvariable":
                        cbxField.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "stratavariable":
                        cbxFieldStrata.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "weightvariable":
                        cbxFieldWeight.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "psuvariable":
                        cbxFieldPSU.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "crosstabvariable":
                        cbxFieldCrosstab.Text = child.InnerText.Replace("&lt;", "<");
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
                }
            }
            SetPositionFromXml(element);
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
            StringBuilder htmlBuilder = new StringBuilder();

            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            if (CustomOutputHeading == null || (string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)")))
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">Complex Sample Means</h2>");
            }
            else if(CustomOutputHeading != "(none)")
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">" + CustomOutputHeading + "</h2>");
            }

            htmlBuilder.AppendLine("<p class=\"gadgetOptions\"><small>");
            htmlBuilder.AppendLine("<em>Frequency variable:</em> <strong>" + cbxField.Text + "</strong>");
            htmlBuilder.AppendLine("<br />");
            htmlBuilder.AppendLine("<em>PSU variable:</em> <strong>" + cbxFieldPSU.Text + "</strong>");
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
            if (cbxFieldStrata.SelectedIndex >= 0)
            {
                htmlBuilder.AppendLine("<em>Strata variable:</em> <strong>" + cbxFieldStrata.Text + "</strong>");
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

            foreach (Grid grid in this.strataGridList)
            {
                string gridName = grid.Tag.ToString();               

                htmlBuilder.AppendLine("<div style=\"height: 7px;\"></div>");
                htmlBuilder.AppendLine("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">");

                htmlBuilder.AppendLine("<tr>");
                htmlBuilder.AppendLine(" <th>&nbsp;</th>");
                htmlBuilder.AppendLine(" <th colspan=\"7\">" + GadgetOptions.MainVariableName + "</th>");
                htmlBuilder.AppendLine("</tr>");

                htmlBuilder.AppendLine("<tr>");
                htmlBuilder.AppendLine(" <th rowspan=\"2\">" + GadgetOptions.CrosstabVariableName + "</th>");
                htmlBuilder.AppendLine(" <th rowspan=\"2\">Count</th>");
                htmlBuilder.AppendLine(" <th rowspan=\"2\">Mean</th>");
                htmlBuilder.AppendLine(" <th rowspan=\"2\">Std Error</th>");
                htmlBuilder.AppendLine(" <th rowspan=\"1\" colspan=\"2\">Confidence Limits</th>");
                htmlBuilder.AppendLine(" <th rowspan=\"2\">Minimum</th>");
                htmlBuilder.AppendLine(" <th rowspan=\"2\">Maximum</th>");
                htmlBuilder.AppendLine("</tr>");

                htmlBuilder.AppendLine("<tr>");
                htmlBuilder.AppendLine(" <th>Lower</th>");
                htmlBuilder.AppendLine(" <th>Upper</th>");
                htmlBuilder.AppendLine("</tr>");

                for (int i = 3; i < grid.RowDefinitions.Count; i++)
                {
                    for (int j = 0; j < grid.ColumnDefinitions.Count; j++)
                    {
                        string tableDataTagOpen = "<td>";
                        string tableDataTagClose = "</td>";

                        if (j == 0)
                        {
                            htmlBuilder.AppendLine("<tr>");
                        }
                        if (j == 0 && i > 0)
                        {
                            tableDataTagOpen = "<td class=\"value\">";
                        }

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
        #endregion  
    }
}
