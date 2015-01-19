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
using ComponentArt.Win.DataVisualization.Charting;

namespace EpiDashboard.Gadgets.Charting
{
    /// <summary>
    /// Interaction logic for ParetoChartGadget.xaml
    /// </summary>
    public partial class ParetoChartGadget : GadgetBase
    {
        private string customOutputHeading;
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

        public ParetoChartGadget()
        {
            InitializeComponent();
            Construct();
        }

        public ParetoChartGadget(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            Construct();
            FillComboboxes();
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            if (LoadingCombos)
            {
                return;
            }

            RefreshResults();
        }

        private void FillComboboxes(bool update = false)
        {
            LoadingCombos = true;

            string prevField = string.Empty;
            string prevWeightField = string.Empty;
            List<string> prevStrataFields = new List<string>();

            //cmbFontSelector.ItemsSource = Fonts.SystemFontFamilies;

            if (update)
            {
                if (cmbField.SelectedIndex >= 0)
                {
                    prevField = cmbField.SelectedItem.ToString();
                }
                if (cmbFieldWeight.SelectedIndex >= 0)
                {
                    prevWeightField = cmbFieldWeight.SelectedItem.ToString();
                }
            }

            cmbField.ItemsSource = null;
            cmbField.Items.Clear();

            cmbFieldWeight.ItemsSource = null;
            cmbFieldWeight.Items.Clear();

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

            cmbField.ItemsSource = fieldNames;
            cmbFieldWeight.ItemsSource = weightFieldNames;

            if (cmbField.Items.Count > 0)
            {
                cmbField.SelectedIndex = -1;
            }
            if (cmbFieldWeight.Items.Count > 0)
            {
                cmbFieldWeight.SelectedIndex = -1;
            }

            if (update)
            {
                cmbField.SelectedItem = prevField;
                cmbFieldWeight.SelectedItem = prevWeightField;
            }

            LoadingCombos = false;
        }

        public override void SetGadgetToProcessingState()
        {
            this.IsProcessing = true;
            this.cmbBarSpacing.IsEnabled = false;
            this.cmbPalette.IsEnabled = false;
            this.cmbField.IsEnabled = false;
            this.cmbFieldWeight.IsEnabled = false;
            this.checkboxIncludeMissing.IsEnabled = false;
            this.checkboxCommentLegalLabels.IsEnabled = false;
            this.btnRun.IsEnabled = false;
        }

        public override void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
            this.cmbBarSpacing.IsEnabled = true;
            this.cmbPalette.IsEnabled = true;
            this.cmbField.IsEnabled = true;
            this.cmbFieldWeight.IsEnabled = true;
            this.checkboxIncludeMissing.IsEnabled = true;
            this.btnRun.IsEnabled = true;
            this.checkboxCommentLegalLabels.IsEnabled = true;           

            base.SetGadgetToFinishedState();
        }

        private void SetVisuals()
        {
            SetBarSpacing();
            SetPalette();

            double legendFontSize = 13;
            double.TryParse(txtLegendFontSize.Text, out legendFontSize);
            xyChart.Legend.FontSize = legendFontSize;

            if (checkboxAnnotations.IsChecked == true) series0.ShowPointAnnotations = true;
            else series0.ShowPointAnnotations = false;

            //xyChart.InnerMargins = new Thickness(20, 30, 30, 50);

            double angle = 0;
            if (double.TryParse(txtXAxisAngle.Text, out angle))
            {
                xAxisCoordinates.Angle = angle;
            }

            tblockYAxisLabel.Text = txtYAxisLabelValue.Text;

            switch (cmbXAxisLabelType.SelectedIndex)
            {
                case 3:
                    tblockXAxisLabel.Text = txtXAxisLabelValue.Text;
                    break;
                case 1:
                    Field field = DashboardHelper.GetAssociatedField(GadgetOptions.MainVariableName);
                    if (field != null)
                    {
                        RenderableField rField = field as RenderableField;
                        tblockXAxisLabel.Text = rField.PromptText;
                    }
                    else
                    {
                        tblockXAxisLabel.Text = GadgetOptions.MainVariableName;
                    }
                    break;
                case 2:
                    tblockXAxisLabel.Text = string.Empty;
                    break;
                default:
                    tblockXAxisLabel.Text = GadgetOptions.MainVariableName;
                    break;
            }

            tblockChartTitle.Text = txtChartTitle.Text;
        }

        public override void RefreshResults()
        {
            if (!LoadingCombos && GadgetOptions != null && cmbField.SelectedIndex > -1)
            {
                CreateInputVariableList();
                SetVisuals();
                infoPanel.Visibility = System.Windows.Visibility.Collapsed;
                waitPanel.Visibility = System.Windows.Visibility.Visible;
                messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.StatusPanel;
                descriptionPanel.PanelMode = EpiDashboard.Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
                baseWorker = new BackgroundWorker();
                baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
                baseWorker.RunWorkerAsync();
                base.RefreshResults();
            }
            else if (!LoadingCombos && cmbField.SelectedIndex == -1)
            {
                ClearResults();
                waitPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void CreateInputVariableList()
        {
            Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

            GadgetOptions.MainVariableName = string.Empty;
            GadgetOptions.WeightVariableName = string.Empty;
            GadgetOptions.StrataVariableNames = new List<string>();
            GadgetOptions.CrosstabVariableName = string.Empty;
            GadgetOptions.ColumnNames = new List<string>();

            GadgetOptions.ShouldSortHighToLow = true;
            GadgetOptions.InputVariableList.Add("charttype", "pareto");

            if (cmbField.SelectedIndex > -1 && !string.IsNullOrEmpty(cmbField.SelectedItem.ToString()))
            {
                inputVariableList.Add("freqvar", cmbField.SelectedItem.ToString());
                GadgetOptions.MainVariableName = cmbField.SelectedItem.ToString();
            }
            else
            {
                return;
            }

            if (cmbFieldWeight.SelectedIndex > -1 && !string.IsNullOrEmpty(cmbFieldWeight.SelectedItem.ToString()))
            {
                inputVariableList.Add("weightvar", cmbFieldWeight.SelectedItem.ToString());
                GadgetOptions.WeightVariableName = cmbFieldWeight.SelectedItem.ToString();
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

            GadgetOptions.ShouldIncludeFullSummaryStatistics = false;
            GadgetOptions.InputVariableList = inputVariableList;
        }

        protected override void Construct()
        {
            if (!string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)"))
            {
                headerPanel.Text = CustomOutputHeading;
            }

            StrataGridList = new List<Grid>();
            StrataExpanderList = new List<Expander>();
            
            mnuSendDataToHTML.Click += new RoutedEventHandler(mnuSendDataToHTML_Click);

//#if LINUX_BUILD
//            mnuSendDataToExcel.Visibility = Visibility.Collapsed;
//#else
//            mnuSendDataToExcel.Visibility = Visibility.Visible;
//            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.ClassesRoot; Microsoft.Win32.RegistryKey excelKey = key.OpenSubKey("Excel.Application"); bool excelInstalled = excelKey == null ? false : true; key = Microsoft.Win32.Registry.ClassesRoot;
//            excelKey = key.OpenSubKey("Excel.Application");
//            excelInstalled = excelKey == null ? false : true;

//            if (!excelInstalled)
//            {
//                mnuSendDataToExcel.Visibility = Visibility.Collapsed;
//            }
//            else
//            {
//                mnuSendDataToExcel.Click += new RoutedEventHandler(mnuSendDataToExcel_Click);
//            }
//#endif

            mnuSendToBack.Click += new RoutedEventHandler(mnuSendToBack_Click);
            mnuClose.Click += new RoutedEventHandler(mnuClose_Click);
            this.IsProcessing = false;
            this.GadgetStatusUpdate += new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            this.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(IsCancelled);

            #region Translation

            tblockWidth.Text = DashboardSharedStrings.GADGET_WIDTH;
            tblockHeight.Text = DashboardSharedStrings.GADGET_HEIGHT;
            tblockMainVariable.Text = DashboardSharedStrings.GADGET_MAIN_VARIABLE;
            tblockWeightVariable.Text = DashboardSharedStrings.GADGET_WEIGHT_VARIABLE;
            checkboxCommentLegalLabels.Content = DashboardSharedStrings.GADGET_LIST_LABELS;
            checkboxIncludeMissing.Content = DashboardSharedStrings.GADGET_INCLUDE_MISSING;

            expanderAdvancedOptions.Header = DashboardSharedStrings.GADGET_ADVANCED_OPTIONS;
            expanderDisplayOptions.Header = DashboardSharedStrings.GADGET_DISPLAY_OPTIONS;

            lblColorsStyles.Content = ChartingSharedStrings.PANEL_COLORS_AND_STYLES;
            lblLabels.Content = ChartingSharedStrings.PANEL_LABELS;
            lblLegend.Content = ChartingSharedStrings.PANEL_LEGEND;

            checkboxUseRefValues.Content = ChartingSharedStrings.USE_REFERENCE_VALUES;
            checkboxAnnotations.Content = ChartingSharedStrings.SHOW_ANNOTATIONS;
            tblockPalette.Text = ChartingSharedStrings.COLOR_PALETTE;

            tblockYAxisLabelValue.Text = ChartingSharedStrings.Y_AXIS_LABEL;

            tblockXAxisLabelType.Text = ChartingSharedStrings.X_AXIS_LABEL_TYPE;
            tblockXAxisLabelValue.Text = ChartingSharedStrings.X_AXIS_LABEL;
            tblockXAxisAngle.Text = ChartingSharedStrings.X_AXIS_ANGLE;
            tblockChartTitleValue.Text = ChartingSharedStrings.CHART_TITLE;

            checkboxShowLegend.Content = ChartingSharedStrings.SHOW_LEGEND;
            checkboxShowLegendBorder.Content = ChartingSharedStrings.SHOW_LEGEND_BORDER;
            checkboxShowVarName.Content = ChartingSharedStrings.SHOW_VAR_NAMES;
            tblockLegendFontSize.Text = ChartingSharedStrings.LEGEND_FONT_SIZE;

            btnRun.Content = DashboardSharedStrings.GADGET_RUN_BUTTON;

            #endregion // Translation

            txtWidth.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            txtHeight.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            txtLegendFontSize.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
            txtXAxisAngle.PreviewKeyDown += new KeyEventHandler(txtInput_IntegerOnly_PreviewKeyDown);

            xyChart.Legend.BorderBrush = Brushes.Gray;
            double legendFontSize = 13;
            double.TryParse(txtLegendFontSize.Text, out legendFontSize);
            xyChart.Legend.FontSize = legendFontSize;

            base.Construct();
        }

        private void ClearResults()
        {
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Text = string.Empty;
            descriptionPanel.PanelMode = EpiDashboard.Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;

            tblockXAxisLabel.Visibility = System.Windows.Visibility.Collapsed;
            tblockYAxisLabel.Visibility = System.Windows.Visibility.Collapsed;
            tblockChartTitle.Visibility = System.Windows.Visibility.Collapsed;

            //panelMain.Children.Clear();
            xyChart.DataSource = null;
            xyChart.Width = 0;
            xyChart.Height = 0;

            StrataGridList.Clear();
            StrataExpanderList.Clear();
        }

        private void ShowLabels()
        {
            if (string.IsNullOrEmpty(tblockXAxisLabel.Text.Trim()))
            {
                tblockXAxisLabel.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                tblockXAxisLabel.Visibility = System.Windows.Visibility.Visible;
            }

            if (string.IsNullOrEmpty(tblockYAxisLabel.Text.Trim()))
            {
                tblockYAxisLabel.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                tblockYAxisLabel.Visibility = System.Windows.Visibility.Visible;
            }

            if (string.IsNullOrEmpty(tblockChartTitle.Text.Trim()))
            {
                tblockChartTitle.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                tblockChartTitle.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void RenderFinish()
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.StatusPanel;
            messagePanel.Text = string.Empty;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            ShowLabels();
            HideConfigPanel();
            CheckAndSetPosition();
        }

        private void RenderFinishWithWarning(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed; //waitCursor.Visibility = Visibility.Hidden;

            messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.WarningPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            ShowLabels();
            HideConfigPanel();
            CheckAndSetPosition();
        }

        private void RenderFinishWithError(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.ErrorPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            xyChart.DataSource = null;
            xyChart.Visibility = System.Windows.Visibility.Collapsed;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        public void CreateFromLegacyXml(XmlElement element)
        {
            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLower())
                {
                    case "singlevariable":                        
                        cmbField.Text = child.InnerText.Replace("&lt;", "<");
                        cmbField.Text = child.InnerText.Replace("&lt;", "<");                        
                        break;
                    case "weightvariable":
                        cmbFieldWeight.Text = child.InnerText.Replace("&lt;", "<");
                        cmbFieldWeight.Text = child.InnerText.Replace("&lt;", "<");
                        break;               
                    case "customheading":
                        this.CustomOutputHeading = child.InnerText;
                        break;
                    case "customdescription":
                        this.CustomOutputDescription = child.InnerText;
                        break;
                    case "customcaption":
                        this.CustomOutputCaption = child.InnerText;
                        break;
                    case "chartsize":
                        //cbxChartSize.SelectedItem = child.InnerText;
                        break;
                    case "chartwidth":
                        txtWidth.Text = child.InnerText;
                        break;
                    case "chartheight":
                        txtHeight.Text = child.InnerText;
                        break;
                    case "charttitle":
                        txtChartTitle.Text = child.InnerText;
                        break;
                    case "chartlegendtitle":
                        //LegendTitle = child.InnerText;
                        break;
                    case "xaxislabel":
                        txtXAxisLabelValue.Text = child.InnerText;
                        cmbXAxisLabelType.SelectedIndex = 3;
                        break;
                    case "yaxislabel":
                        txtYAxisLabelValue.Text = child.InnerText;
                        break;
                    case "xaxisrotation":
                        switch (child.InnerText)
                        {
                            case "0":
                                txtXAxisAngle.Text = "0";
                                break;
                            case "45":
                                txtXAxisAngle.Text = "-45";
                                break;
                            case "90":
                                txtXAxisAngle.Text = "-90";
                                break;
                            default:
                                txtXAxisAngle.Text = "0";
                                break;
                        }
                        break;
                }
            }
        }

        public override void CreateFromXml(XmlElement element)
        {
            this.LoadingCombos = true;
            HideConfigPanel();
            infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            if (element.Name.Equals("chartGadget") || element.Name.Equals("ChartControl"))
            {
                CreateFromLegacyXml(element);
            }
            else
            {
                foreach (XmlElement child in element.ChildNodes)
                {
                    switch (child.Name.ToLower())
                    {
                        case "mainvariable":
                            cmbField.Text = child.InnerText.Replace("&lt;", "<");
                            break;
                        case "weightvariable":
                            cmbFieldWeight.Text = child.InnerText.Replace("&lt;", "<");
                            break;
                        case "showlistlabels":
                            if (child.InnerText.ToLower().Equals("true")) { checkboxCommentLegalLabels.IsChecked = true; }
                            else { checkboxCommentLegalLabels.IsChecked = false; }
                            break;
                        case "includemissing":
                            if (child.InnerText.ToLower().Equals("true")) { checkboxIncludeMissing.IsChecked = true; }
                            else { checkboxIncludeMissing.IsChecked = false; }
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
                                    descriptionPanel.PanelMode = EpiDashboard.Controls.GadgetDescriptionPanel.DescriptionPanelMode.DisplayMode;
                                }
                                else
                                {
                                    descriptionPanel.PanelMode = EpiDashboard.Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
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
                        case "userefvalues":
                            if (child.InnerText.ToLower().Equals("true")) { checkboxUseRefValues.IsChecked = true; }
                            else { checkboxUseRefValues.IsChecked = false; }
                            break;
                        case "showannotations":
                            if (child.InnerText.ToLower().Equals("true")) { checkboxAnnotations.IsChecked = true; }
                            else { checkboxAnnotations.IsChecked = false; }
                            break;
                        case "barspace":
                            cmbBarSpacing.SelectedIndex = int.Parse(child.InnerText);
                            break;
                        case "palette":
                            cmbPalette.SelectedIndex = int.Parse(child.InnerText);
                            break;
                        case "bartype":
                            cmbBarType.SelectedIndex = int.Parse(child.InnerText);
                            break;
                        case "yaxislabel":
                            txtYAxisLabelValue.Text = child.InnerText;
                            break;
                        case "xaxislabeltype":
                            cmbXAxisLabelType.SelectedIndex = int.Parse(child.InnerText);
                            break;
                        case "xaxislabel":
                            txtXAxisLabelValue.Text = child.InnerText;
                            break;
                        case "xaxisangle":
                            txtXAxisAngle.Text = child.InnerText;
                            break;
                        case "charttitle":
                            txtChartTitle.Text = child.InnerText;
                            break;
                        case "showlegend":
                            if (child.InnerText.ToLower().Equals("true")) { checkboxShowLegend.IsChecked = true; }
                            else { checkboxShowLegend.IsChecked = false; }
                            break;
                        case "showlegendborder":
                            if (child.InnerText.ToLower().Equals("true")) { checkboxShowLegendBorder.IsChecked = true; }
                            else { checkboxShowLegendBorder.IsChecked = false; }
                            break;
                        case "showlegendvarnames":
                            if (child.InnerText.ToLower().Equals("true")) { checkboxShowVarName.IsChecked = true; }
                            else { checkboxShowVarName.IsChecked = false; }
                            break;
                        case "legendfontsize":
                            txtLegendFontSize.Text = child.InnerText;
                            break;
                        case "height":
                            txtHeight.Text = child.InnerText;
                            break;
                        case "width":
                            txtWidth.Text = child.InnerText;
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
        /// Serializes the gadget into Xml
        /// </summary>
        /// <param name="doc">The Xml docment</param>
        /// <returns>XmlNode</returns>
        public override XmlNode Serialize(XmlDocument doc)
        {
            CreateInputVariableList();

            Dictionary<string, string> inputVariableList = GadgetOptions.InputVariableList;

            string mainVar = string.Empty;
            string strataVar = string.Empty;
            string weightVar = string.Empty;
            string sort = string.Empty;
            bool allValues = false;
            bool showConfLimits = true;
            bool showCumulativePercent = true;
            bool includeMissing = false;

            int height = 600;
            int width = 800;

            int.TryParse(txtHeight.Text, out height);
            int.TryParse(txtWidth.Text, out width);

            if (inputVariableList.ContainsKey("freqvar"))
            {
                mainVar = inputVariableList["freqvar"].Replace("<", "&lt;");
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

            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            string xmlString =
            "<mainVariable>" + mainVar + "</mainVariable>";

            if (GadgetOptions.StrataVariableNames.Count == 1)
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

                "<height>" + height + "</height>" +
                "<width>" + width + "</width>" +

            "<sort>" + sort + "</sort>" +
            "<allValues>" + allValues + "</allValues>" +
            "<showListLabels>" + checkboxCommentLegalLabels.IsChecked + "</showListLabels>" +
            "<includeMissing>" + includeMissing + "</includeMissing>" +
            "<customHeading>" + CustomOutputHeading.Replace("<", "&lt;") + "</customHeading>" +
            "<customDescription>" + CustomOutputDescription.Replace("<", "&lt;") + "</customDescription>" +
            "<customCaption>" + CustomOutputCaption + "</customCaption>";

            xmlString += "<useRefValues>" + checkboxUseRefValues.IsChecked.Value + "</useRefValues>";
            xmlString += "<showAnnotations>" + checkboxAnnotations.IsChecked.Value + "</showAnnotations>";

            xmlString += "<barSpace>" + cmbBarSpacing.SelectedIndex + "</barSpace>";
            xmlString += "<palette>" + cmbPalette.SelectedIndex + "</palette>";
            xmlString += "<barType>" + cmbBarType.SelectedIndex + "</barType>";

            xmlString += "<yAxisLabel>" + txtYAxisLabelValue.Text + "</yAxisLabel>";
            xmlString += "<xAxisLabelType>" + cmbXAxisLabelType.SelectedIndex + "</xAxisLabelType>";
            xmlString += "<xAxisLabel>" + txtXAxisLabelValue.Text + "</xAxisLabel>";
            xmlString += "<xAxisAngle>" + txtXAxisAngle.Text + "</xAxisAngle>";
            xmlString += "<chartTitle>" + txtChartTitle.Text + "</chartTitle>";

            xmlString += "<showLegend>" + checkboxShowLegend.IsChecked.Value + "</showLegend>";
            xmlString += "<showLegendBorder>" + checkboxShowLegendBorder.IsChecked.Value + "</showLegendBorder>";
            xmlString += "<showLegendVarNames>" + checkboxShowVarName.IsChecked.Value + "</showLegendVarNames>";
            xmlString += "<legendFontSize>" + txtLegendFontSize.Text + "</legendFontSize>";

            xmlString = xmlString + SerializeAnchors();

            System.Xml.XmlElement element = doc.CreateElement("paretoChartGadget");
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
            collapsed.Value = "false"; // currently no way to collapse the gadget, so leave this 'false' for now
            type.Value = "EpiDashboard.Gadgets.Charting.ParetoChartGadget";

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);
            element.Attributes.Append(id);

            return element;
        }

        private class XYChartData
        {
            public object X { get; set; }
            public double Y { get; set; }
            public object Z { get; set; }
            //public double Lo { get; set; }
            //public double Hi { get; set; }
            //public double Open { get; set; }
            //public double Close { get; set; }
        }

        private void SetChartData(List<XYChartData> dataList)
        {
            xyChart.DataSource = dataList;
            xyChart.Width = double.Parse(txtWidth.Text);
            xyChart.Height = double.Parse(txtHeight.Text);
        }

        private delegate void SetChartDataDelegate(List<XYChartData> dataList);

        protected override void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (DashboardHelper.IsAutoClosing)
            {
                System.Threading.Thread.Sleep(2200);
            }
            else
            {
                System.Threading.Thread.Sleep(300);
            }
            this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToFinishedState));
        }

        protected override void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (syncLock)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));

                string freqVar = GadgetOptions.MainVariableName;
                string weightVar = GadgetOptions.WeightVariableName;
                string strataVar = string.Empty;
                bool includeMissing = GadgetOptions.ShouldIncludeMissing;

                List<string> stratas = new List<string>();
                if (!string.IsNullOrEmpty(strataVar))
                {
                    stratas.Add(strataVar);
                }

                try
                {
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

                    Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables = DashboardHelper.GenerateFrequencyTable(GadgetOptions);
                    List<XYChartData> dataList = new List<XYChartData>();

                    foreach (KeyValuePair<DataTable, List<DescriptiveStatistics>> tableKvp in stratifiedFrequencyTables)
                    {
                        double count = 0;
                        foreach (DescriptiveStatistics ds in tableKvp.Value)
                        {
                            count = count + ds.observations;
                        }

                        string strataValue = tableKvp.Key.TableName;
                        DataTable table = tableKvp.Key;
                        double max = 0;
                        foreach (DataRow row in table.Rows)
                        {
                            XYChartData chartData = new XYChartData();
                            chartData.X = row[0].ToString();
                            chartData.Y = (double)row[1];
                            //chartData.Z = row[0];
                            //GetConfLimit(ref chartData, count);
                            dataList.Add(chartData);
                            max = max + (double)row[1];
                        }

                        //foreach (DataRow row in table.Rows)
                        //{
                        //    max = max + (double)row[1];
                        //}

                        double runningPercent = 0;
                        foreach (DataRow row in table.Rows)
                        {
                            foreach (XYChartData chartData in dataList)
                            {
                                if (chartData.X.ToString() == row[0].ToString())
                                {
                                    chartData.Z = ((chartData.Y / max) * 100) + runningPercent;
                                    runningPercent = (double)chartData.Z;
                                }
                            }
                        }
                    }

                    this.Dispatcher.BeginInvoke(new SetChartDataDelegate(SetChartData), dataList);
                    this.Dispatcher.BeginInvoke(new SimpleCallback(RenderFinish));

                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
                }
                finally
                {
                    stopwatch.Stop();
                    Debug.Print("Column chart gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete.");
                    Debug.Print(DashboardHelper.DataFilters.GenerateDataFilterString());
                }
            }
        }

        //private void GetConfLimit(ref XYChartData chartData, double count)
        //{
        //    double lower = 0;
        //    double upper = 0;

        //    if (chartData.Y == count)
        //    {
        //        lower = 1;
        //        upper = 1;
        //    }
        //    else
        //    {
        //        if (count > 300)
        //        {
        //            freq.FLEISS(chartData.Y, (double)count, 1.96, ref lower, ref upper);
        //        }
        //        else
        //        {
        //            freq.ExactCI(chartData.Y, (double)count, 95.0, ref lower, ref upper);
        //        }
        //    }

        //    //double pct = chartData.Y / count;

        //    chartData.Lo = lower * count;
        //    chartData.Hi = upper * count;
        //    chartData.Open = lower * count;
        //    chartData.Close = upper * count;
        //}

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0)
        {
            // Check to see if a chart has been created.
            if (xyChart.ActualHeight == 0 || xyChart.ActualWidth == 0)
            {
                return string.Empty;
            }

            StringBuilder htmlBuilder = new StringBuilder();

            if (string.IsNullOrEmpty(CustomOutputHeading))
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">Chart</h2>");
            }
            else if (CustomOutputHeading != "(none)")
            {
                htmlBuilder.AppendLine("<h2 class=\"gadgetHeading\">" + CustomOutputHeading + "</h2>");
            }

            if (!string.IsNullOrEmpty(CustomOutputDescription))
            {
                htmlBuilder.AppendLine("<p class=\"gadgetsummary\">" + CustomOutputDescription + "</p>");
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

            BitmapSource img = (BitmapSource)Common.ToImageSource(xyChart);
            System.IO.FileStream stream = new System.IO.FileStream(imageFileName, System.IO.FileMode.Create);
            //JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(img));
            encoder.Save(stream);
            stream.Close();

            htmlBuilder.AppendLine("<img src=\"" + imageFileName + "\" />");

            return htmlBuilder.ToString();
        }

        private void SetBarType()
        {
            if (LoadingCombos) { return; }

            switch (cmbBarType.SelectedIndex)
            {
                case 1:
                    series0.BarKind = BarKind.Cylinder;
                    break;
                case 2:
                    series0.BarKind = BarKind.Rectangle;
                    break;
                case 3:
                    series0.BarKind = BarKind.RoundedBlock;
                    break;
                default:
                case 0:
                    series0.BarKind = BarKind.Block;
                    break;
            }
        }

        private void SetPalette()
        {
            if (LoadingCombos) { return; }

            ComboBoxItem cbi = cmbPalette.SelectedItem as ComboBoxItem;
            ComponentArt.Win.DataVisualization.Palette palette = ComponentArt.Win.DataVisualization.Palette.GetPalette(cbi.Content.ToString());

            xyChart.Palette = palette;
        }

        private void SetBarSpacing()
        {
            if (LoadingCombos) { return; }

            series0.BarRelativeBegin = double.NaN;
            series0.BarRelativeEnd = double.NaN;

            switch (cmbBarSpacing.SelectedIndex)
            {
                case 1:
                    series0.RelativePointSpace = 0;
                    series0.BarRelativeBegin = 0;
                    series0.BarRelativeEnd = 0;
                    break;
                case 2:
                    series0.RelativePointSpace = 0.1;
                    break;
                case 3:
                    series0.RelativePointSpace = 0.4;
                    break;
                case 4:
                    series0.RelativePointSpace = 0.8;
                    break;
                case 0:
                default:
                    series0.RelativePointSpace = 0.15;
                    break;
            }
        }

        private void cmbBarSpacing_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetBarSpacing();
        }

        private void checkboxUseDiffColors_Checked(object sender, RoutedEventArgs e)
        {
            xyChart.UseDifferentBarColors = true;
        }

        private void checkboxUseDiffColors_Unchecked(object sender, RoutedEventArgs e)
        {
            xyChart.UseDifferentBarColors = false;
        }

        private void checkboxShowLegend_Checked(object sender, RoutedEventArgs e)
        {
            xyChart.LegendVisible = true;
        }

        private void checkboxShowLegend_Unchecked(object sender, RoutedEventArgs e)
        {
            xyChart.LegendVisible = false;
        }

        private void checkboxUseRefValues_Checked(object sender, RoutedEventArgs e)
        {
            yAxis.UseReferenceValue = true;
        }

        private void checkboxUseRefValues_Unchecked(object sender, RoutedEventArgs e)
        {
            yAxis.UseReferenceValue = false;
        }

        private void cmbPalette_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetPalette();
        }

        private void cmbBarType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetBarType();
        }

        private void cmbXAxisLabelType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LoadingCombos || txtXAxisLabelValue == null) return;
            switch (cmbXAxisLabelType.SelectedIndex)
            {
                case 3:
                    txtXAxisLabelValue.IsEnabled = true;
                    break;
                case 0:
                case 1:
                case 2:
                    txtXAxisLabelValue.IsEnabled = false;
                    txtXAxisLabelValue.Text = string.Empty;
                    break;
            }
        }

        private void xyChart_DataStructureCreated(object sender, EventArgs e)
        {
            string sName = "";

            NumericCoordinates nc = new NumericCoordinates() { From = 0, To = 100, Step = 10 };
            //AxisCoordinates ac = new AxisCoordinates();
            //ac.Coordinates = nc;
            y2AxisCoordinates.Coordinates = nc;


            if (GadgetOptions.StrataVariableNames.Count > 0)
            {
                foreach (Series s0 in xyChart.DataSeries)
                {
                    sName = s0.Label.Split('.')[1];
                    if (checkboxShowVarName.IsChecked == false)
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

        private void checkboxShowLegendBorder_Checked(object sender, RoutedEventArgs e)
        {
            checkboxShowLegend.IsChecked = true;
            xyChart.Legend.BorderThickness = new Thickness(1);
        }

        private void checkboxShowLegendBorder_Unchecked(object sender, RoutedEventArgs e)
        {
            xyChart.Legend.BorderThickness = new Thickness(0);
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

            encoder.Frames.Add(BitmapFrame.Create(img));
            encoder.Save(stream);
            stream.Close();
        }

        public virtual BitmapSource ToBitmapSource(bool includeGrid = true)
        {
            ComponentArt.Win.DataVisualization.Charting.ChartBase xyChart = null;
            object el = FindName("xyChart");
            if (el is XYChart)
            {
                xyChart = el as XYChart;
            }
            else if (el is ComponentArt.Win.DataVisualization.Charting.PieChart)
            {
                xyChart = el as ComponentArt.Win.DataVisualization.Charting.PieChart;
            }

            if (xyChart == null) throw new ApplicationException();

            Transform transform = xyChart.LayoutTransform;
            Thickness margin = new Thickness(xyChart.Margin.Left, xyChart.Margin.Top, xyChart.Margin.Right, xyChart.Margin.Bottom);
            // reset current transform (in case it is scaled or rotated)
            xyChart.LayoutTransform = null;
            xyChart.Margin = new Thickness(0);

            Size size = new Size(xyChart.Width, xyChart.Height);
            // Measure and arrange the surface
            // VERY IMPORTANT
            xyChart.Measure(size);
            xyChart.Arrange(new Rect(size));

            RenderTargetBitmap renderBitmap =
              new RenderTargetBitmap(
                (int)size.Width,
                (int)size.Height,
                96d,
                96d,
                PixelFormats.Pbgra32);
            renderBitmap.Render(xyChart);

            xyChart.LayoutTransform = transform;
            xyChart.Margin = margin;

            xyChart.Measure(size);
            xyChart.Arrange(new Rect(size));

            return renderBitmap;
        }

        public void SaveImageToFile()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG Image (.png)|*.png|JPEG Image (.jpg)|*.jpg";

            if (dlg.ShowDialog().Value)
            {
                ToImageFile(dlg.FileName);
                MessageBox.Show("Image saved successfully.", "Save Image", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public virtual void CopyImageToClipboard()
        {
            BitmapSource img = ToBitmapSource(false); // renderBitmap;
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(img));
            Clipboard.SetImage(encoder.Frames[0]);
        }

        public virtual void CopyAllDataToClipboard()
        {
            object el = FindName("xyChart");
            if (el is XYChart)
            {
                Clipboard.Clear();
                Clipboard.SetText(SendDataToString());
            }
        }

        public virtual string SendDataToString()
        {
            StringBuilder sb = new StringBuilder();

            XYChart xyChart = null;
            object el = FindName("xyChart");
            if (el is XYChart)
            {
                xyChart = el as XYChart;
                List<XYChartData> dataList = xyChart.DataSource as List<XYChartData>;

                sb.Append("X" + "\t");
                sb.Append("Y1" + "\t");
                sb.Append("Y2" + "\t");
                sb.AppendLine();

                foreach (XYChartData chartData in dataList)
                {
                    sb.Append(chartData.X + "\t");
                    sb.Append(chartData.Y + "\t");
                    sb.Append(chartData.Z + "\t");
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private void mnuSaveChartAsImage_Click(object sender, RoutedEventArgs e)
        {
            SaveImageToFile();
        }

        private void mnuCopyChartImage_Click(object sender, RoutedEventArgs e)
        {
            CopyImageToClipboard();
        }

        private void mnuCopyChartData_Click(object sender, RoutedEventArgs e)
        {
            CopyAllDataToClipboard();
        }
    }
}
