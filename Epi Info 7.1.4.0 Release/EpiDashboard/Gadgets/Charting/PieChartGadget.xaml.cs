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
using EpiDashboard.Controls;
using EpiDashboard.Gadgets;
using EpiDashboard.Gadgets.Charting;
using ComponentArt.Win.DataVisualization.Charting;

namespace EpiDashboard.Gadgets.Charting
{
    /// <summary>
    /// Interaction logic for PieChartGadget.xaml
    /// </summary>
    public partial class PieChartGadget : ChartGadgetBase
    {
        public PieChartGadget()
        {
            InitializeComponent();
            Construct();
        }

        public PieChartGadget(DashboardHelper dashboardHelper)
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
            string prevCrosstabField = string.Empty;
            List<string> prevStrataFields = new List<string>();

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
                if (cmbFieldCrosstab.SelectedIndex >= 0)
                {
                    prevCrosstabField = cmbFieldCrosstab.SelectedItem.ToString();
                }
                foreach (string s in listboxFieldStrata.SelectedItems)
                {
                    prevStrataFields.Add(s);
                }
            }

            cmbField.ItemsSource = null;
            cmbField.Items.Clear();

            cmbFieldWeight.ItemsSource = null;
            cmbFieldWeight.Items.Clear();

            cmbFieldCrosstab.ItemsSource = null;
            cmbFieldCrosstab.Items.Clear();

            listboxFieldStrata.ItemsSource = null;
            listboxFieldStrata.Items.Clear();

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
            cmbFieldCrosstab.ItemsSource = strataFieldNames;
            listboxFieldStrata.ItemsSource = strataFieldNames;

            if (cmbField.Items.Count > 0)
            {
                cmbField.SelectedIndex = -1;
            }
            if (cmbFieldWeight.Items.Count > 0)
            {
                cmbFieldWeight.SelectedIndex = -1;
            }
            if (cmbFieldCrosstab.Items.Count > 0)
            {
                cmbFieldCrosstab.SelectedIndex = -1;
            }

            if (update)
            {
                cmbField.SelectedItem = prevField;
                cmbFieldWeight.SelectedItem = prevWeightField;
                cmbFieldCrosstab.SelectedItem = prevCrosstabField;

                foreach (string s in prevStrataFields)
                {
                    listboxFieldStrata.SelectedItems.Add(s);
                }
            }

            LoadingCombos = false;
        }

        public override void SetGadgetToProcessingState()
        {
            this.IsProcessing = true;
            this.txtChartTitle.IsEnabled = false;
            this.txtChartSubTitle.IsEnabled = false;
            this.cmbPalette.IsEnabled = false;
            this.cmbChartKind.IsEnabled = false;
            this.cmbFieldCrosstab.IsEnabled = false;
            this.cmbField.IsEnabled = false;
            this.listboxFieldStrata.IsEnabled = false;
            this.cmbFieldWeight.IsEnabled = false;
            this.checkboxIncludeMissing.IsEnabled = false;
            this.checkboxSortHighLow.IsEnabled = false;
            this.checkboxCommentLegalLabels.IsEnabled = false;
            this.checkboxAllValues.IsEnabled = false;
            this.btnRun.IsEnabled = false;
        }

        public override void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
            this.txtChartTitle.IsEnabled = true;
            this.txtChartSubTitle.IsEnabled = true;
            this.cmbPalette.IsEnabled = true;
            this.cmbChartKind.IsEnabled = true;
            this.cmbFieldCrosstab.IsEnabled = true;
            this.cmbField.IsEnabled = true;
            this.listboxFieldStrata.IsEnabled = true;
            this.cmbFieldWeight.IsEnabled = true;
            this.checkboxIncludeMissing.IsEnabled = true;
            this.checkboxSortHighLow.IsEnabled = true;
            this.btnRun.IsEnabled = true;
            this.checkboxAllValues.IsEnabled = true;
            this.checkboxCommentLegalLabels.IsEnabled = true;

            base.SetGadgetToFinishedState();
        }

        public override void RefreshResults()
        {
            if (!LoadingCombos && GadgetOptions != null && cmbField.SelectedIndex > -1)
            {
                CreateInputVariableList();
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

            if (cmbFieldCrosstab.SelectedIndex > -1 && !string.IsNullOrEmpty(cmbFieldCrosstab.SelectedItem.ToString()))
            {
                inputVariableList.Add("crosstabvar", cmbFieldCrosstab.SelectedItem.ToString());
                GadgetOptions.CrosstabVariableName = cmbFieldCrosstab.SelectedItem.ToString();
            }

            if (listboxFieldStrata.SelectedItems.Count > 0)
            {
                GadgetOptions.StrataVariableNames = new List<string>();
                foreach (string s in listboxFieldStrata.SelectedItems)
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

            mnuCopy.Click += new RoutedEventHandler(mnuCopy_Click);
            mnuSendDataToHTML.Click += new RoutedEventHandler(mnuSendDataToHTML_Click);
            txtAnnotationPercent.PreviewKeyDown += new KeyEventHandler(txtInput_PositiveIntegerOnly_PreviewKeyDown);
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
            tblockStrataVariable.Text = DashboardSharedStrings.GADGET_STRATA_VARIABLE;
            tblockWeightVariable.Text = DashboardSharedStrings.GADGET_WEIGHT_VARIABLE;
            tblockCrosstabVariable.Text = ChartingSharedStrings.GRAPH_FOR_EACH_VALUE_OF_VARIABLE;

            checkboxAllValues.Content = DashboardSharedStrings.GADGET_ALL_LIST_VALUES;
            checkboxCommentLegalLabels.Content = DashboardSharedStrings.GADGET_LIST_LABELS;
            checkboxIncludeMissing.Content = DashboardSharedStrings.GADGET_INCLUDE_MISSING;
            checkboxSortHighLow.Content = DashboardSharedStrings.GADGET_SORT_HI_LOW;

            expanderAdvancedOptions.Header = DashboardSharedStrings.GADGET_ADVANCED_OPTIONS;
            expanderDisplayOptions.Header = DashboardSharedStrings.GADGET_DISPLAY_OPTIONS;

            lblColorsStyles.Content = ChartingSharedStrings.PANEL_COLORS_AND_STYLES;
            lblLabels.Content = ChartingSharedStrings.PANEL_LABELS;
            lblLegend.Content = ChartingSharedStrings.PANEL_LEGEND;

            checkboxAnnotations.Content = ChartingSharedStrings.SHOW_ANNOTATIONS;
            tblockPalette.Text = ChartingSharedStrings.COLOR_PALETTE;

            tblockChartTitleValue.Text = ChartingSharedStrings.CHART_TITLE;
            tblockChartSubTitleValue.Text = ChartingSharedStrings.CHART_SUBTITLE;

            checkboxShowLegend.Content = ChartingSharedStrings.SHOW_LEGEND;
            checkboxShowLegendBorder.Content = ChartingSharedStrings.SHOW_LEGEND_BORDER;
            checkboxShowVarName.Content = ChartingSharedStrings.SHOW_VAR_NAMES;
            tblockLegendFontSize.Text = ChartingSharedStrings.LEGEND_FONT_SIZE;
            //tblockLegendDock.Text = ChartingSharedStrings.LEGEND_PLACEMENT;

            btnRun.Content = DashboardSharedStrings.GADGET_RUN_BUTTON;

            #endregion // Translation

            base.Construct();
        }

        private void ClearResults()
        {
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Text = string.Empty;
            descriptionPanel.PanelMode = EpiDashboard.Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;

            panelMain.Children.Clear();
            //dvChart.DataSource = null;
            //dvChart.Width = 0;
            //dvChart.Height = 0;

            StrataGridList.Clear();
            StrataExpanderList.Clear();
        }

        private void RenderFinish()
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.StatusPanel;
            messagePanel.Text = string.Empty;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        private void RenderFinishWithWarning(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed; //waitCursor.Visibility = Visibility.Hidden;

            messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.WarningPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        private void RenderFinishWithError(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.ErrorPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            panelMain.Children.Clear();
            //dvChart.DataSource = null;
            //dvChart.Visibility = System.Windows.Visibility.Collapsed;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        public void CreateFromLegacyXml(XmlElement element)
        {
            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLower())
                {
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
                    case "singlevariable":
                        cmbField.Text = child.InnerText.Replace("&lt;", "<");
                        cmbField.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "weightvariable":
                        cmbFieldWeight.Text = child.InnerText.Replace("&lt;", "<");
                        cmbFieldWeight.Text = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "stratavariable":
                        listboxFieldStrata.SelectedItems.Add(child.InnerText.Replace("&lt;", "<"));
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
                    case "chartwidth":
                        if (!string.IsNullOrEmpty(child.InnerText))
                        {
                            txtWidth.Text = child.InnerText;
                        }
                        break;
                    case "chartheight":
                        if (!string.IsNullOrEmpty(child.InnerText))
                        {
                            txtHeight.Text = child.InnerText;
                        }
                        break;
                    case "charttitle":
                        txtChartTitle.Text = child.InnerText;                        
                        break;
                    case "chartsize":
                        switch (child.InnerText)
                        {
                            case "Small":
                                txtWidth.Text = "400";
                                txtHeight.Text = "250";
                                break;
                            case "Medium":
                                txtWidth.Text = "533";
                                txtHeight.Text = "333";
                                break;
                            case "Large":
                                txtWidth.Text = "800";
                                txtHeight.Text = "500";
                                break;
                            case "Custom":
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
                        case "stratavariable":
                            listboxFieldStrata.SelectedItems.Add(child.InnerText.Replace("&lt;", "<"));
                            break;
                        case "stratavariables":
                            foreach (XmlElement field in child.ChildNodes)
                            {
                                List<string> fields = new List<string>();
                                if (field.Name.ToLower().Equals("stratavariable"))
                                {
                                    listboxFieldStrata.SelectedItems.Add(field.InnerText.Replace("&lt;", "<"));
                                }
                            }
                            break;
                        case "weightvariable":
                            cmbFieldWeight.Text = child.InnerText.Replace("&lt;", "<");
                            break;
                        case "crosstabvariable":
                            cmbFieldCrosstab.Text = child.InnerText.Replace("&lt;", "<");
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
                        //case "usediffbarcolors":
                        //    if (child.InnerText.ToLower().Equals("true")) { checkboxUseDiffColors.IsChecked = true; }
                        //    else { checkboxUseDiffColors.IsChecked = false; }
                        //    break;
                        case "showannotations":
                            if (child.InnerText.ToLower().Equals("true")) { checkboxAnnotations.IsChecked = true; }
                            else { checkboxAnnotations.IsChecked = false; }
                            break;
                        case "showannotationlabel":
                            if (child.InnerText.ToLower().Equals("true")) { checkboxAnnotationLabel.IsChecked = true; }
                            else { checkboxAnnotationLabel.IsChecked = false; }
                            break;
                        case "showannotationvalue":
                            if (child.InnerText.ToLower().Equals("true")) { checkboxAnnotationValue.IsChecked = true; }
                            else { checkboxAnnotationValue.IsChecked = false; }
                            break;
                        case "showannotationpercent":
                            if (child.InnerText.ToLower().Equals("true")) { checkboxAnnotationPercent.IsChecked = true; }
                            else { checkboxAnnotationPercent.IsChecked = false; }
                            break;
                        case "annotationpercent":
                            if (string.IsNullOrEmpty(child.InnerText))
                            {
                                txtAnnotationPercent.Text = "20";
                            }
                            else
                            {
                                txtAnnotationPercent.Text = child.InnerText;
                            }
                            break;
                        case "chartkind":
                            cmbChartKind.SelectedIndex = int.Parse(child.InnerText);
                            break;
                        case "palette":
                            cmbPalette.SelectedIndex = int.Parse(child.InnerText);
                            break;
                        case "charttitle":
                            txtChartTitle.Text = child.InnerText;
                            break;
                        case "chartsubtitle":
                            txtChartSubTitle.Text = child.InnerText;
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
                            if (!string.IsNullOrEmpty(child.InnerText))
                            {
                                txtHeight.Text = child.InnerText;
                            }
                            break;
                        case "width":
                            if (!string.IsNullOrEmpty(child.InnerText))
                            {
                                txtWidth.Text = child.InnerText;
                            }
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
            string crosstabVar = string.Empty;
            string sort = string.Empty;
            bool allValues = false;
            bool showConfLimits = true;
            bool showCumulativePercent = true;
            bool includeMissing = false;

            int height = 600;
            int width = 800;

            int.TryParse(txtHeight.Text, out height);
            int.TryParse(txtWidth.Text, out width);

            crosstabVar = GadgetOptions.CrosstabVariableName.Replace("<", "&lt;");

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
                "<crosstabVariable>" + crosstabVar + "</crosstabVariable>" +
                "<height>" + height + "</height>" +
                "<width>" + width + "</width>" +

            "<sort>" + sort + "</sort>" +
            "<allValues>" + allValues + "</allValues>" +
            "<showListLabels>" + checkboxCommentLegalLabels.IsChecked + "</showListLabels>" +
            "<includeMissing>" + includeMissing + "</includeMissing>" +
            "<customHeading>" + CustomOutputHeading.Replace("<", "&lt;") + "</customHeading>" +
            "<customDescription>" + CustomOutputDescription.Replace("<", "&lt;") + "</customDescription>" +
            "<customCaption>" + CustomOutputCaption + "</customCaption>";

            xmlString += "<showAnnotations>" + checkboxAnnotations.IsChecked.Value + "</showAnnotations>";
            xmlString += "<showAnnotationLabel>" + checkboxAnnotationLabel.IsChecked.Value + "</showAnnotationLabel>";
            xmlString += "<showAnnotationValue>" + checkboxAnnotationValue.IsChecked.Value + "</showAnnotationValue>";
            xmlString += "<showAnnotationPercent>" + checkboxAnnotationPercent.IsChecked.Value + "</showAnnotationPercent>";
            xmlString += "<annotationPercent>" + txtAnnotationPercent.Text + "</annotationPercent>";

            xmlString += "<chartKind>" + cmbChartKind.SelectedIndex + "</chartKind>";
            xmlString += "<palette>" + cmbPalette.SelectedIndex + "</palette>";

            xmlString += "<chartTitle>" + txtChartTitle.Text + "</chartTitle>";
            xmlString += "<chartSubTitle>" + txtChartSubTitle.Text + "</chartSubTitle>";

            xmlString += "<showLegend>" + checkboxShowLegend.IsChecked.Value + "</showLegend>";
            xmlString += "<showLegendBorder>" + checkboxShowLegendBorder.IsChecked.Value + "</showLegendBorder>";
            xmlString += "<showLegendVarNames>" + checkboxShowVarName.IsChecked.Value + "</showLegendVarNames>";
            xmlString += "<legendFontSize>" + txtLegendFontSize.Text + "</legendFontSize>";

            xmlString = xmlString + SerializeAnchors();

            System.Xml.XmlElement element = doc.CreateElement("pieChartGadget");
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
            type.Value = "EpiDashboard.Gadgets.Charting.PieChartGadget";

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
            public object S { get; set; }
        }

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
                string crosstabVar = GadgetOptions.CrosstabVariableName;
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

                    if (!string.IsNullOrEmpty(crosstabVar.Trim()))
                    {
                        List<string> crosstabVarList = new List<string>();
                        crosstabVarList.Add(crosstabVar);

                        foreach (Strata strata in DashboardHelper.GetStrataValuesAsDictionary(crosstabVarList, false, false))
                        {
                            GadgetParameters parameters = new GadgetParameters(GadgetOptions);

                            if (!string.IsNullOrEmpty(GadgetOptions.CustomFilter))
                            {
                                parameters.CustomFilter = "(" + parameters.CustomFilter + ") AND " + strata.SafeFilter;
                            }
                            else
                            {
                                parameters.CustomFilter = strata.SafeFilter;
                            }
                            parameters.CrosstabVariableName = string.Empty;
                            Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables = DashboardHelper.GenerateFrequencyTable(parameters);
                            GenerateChartData(stratifiedFrequencyTables, strata);
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables = DashboardHelper.GenerateFrequencyTable(GadgetOptions);
                        GenerateChartData(stratifiedFrequencyTables);
                    }
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

        protected override void GenerateChartData(Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables, Strata strata = null)
        {
            lock (syncLockData)
            {
                List<XYColumnChartData> dataList = new List<XYColumnChartData>();

                foreach (KeyValuePair<DataTable, List<DescriptiveStatistics>> tableKvp in stratifiedFrequencyTables)
                {
                    double count = 0;
                    foreach (DescriptiveStatistics ds in tableKvp.Value)
                    {
                        count = count + ds.observations;
                    }

                    string strataValue = tableKvp.Key.TableName;
                    DataTable table = tableKvp.Key;

                    foreach (DataRow row in table.Rows)
                    {
                        XYColumnChartData chartData = new XYColumnChartData();
                        chartData.X = strataValue;
                        chartData.Y = (double)row[1];
                        chartData.S = row[0];

                        if (chartData.S == null || string.IsNullOrEmpty(chartData.S.ToString().Trim()))
                        {
                            chartData.S = Config.Settings.RepresentationOfMissing;
                        }

                        dataList.Add(chartData);
                    }
                }

                this.Dispatcher.BeginInvoke(new SetChartDataDelegate(SetChartData), dataList, strata);
            }
        }

        protected override void SetChartData(List<XYColumnChartData> dataList, Strata strata)
        {
            PieChartSettings chartSettings = new PieChartSettings();
            chartSettings.ChartTitle = txtChartTitle.Text;
            chartSettings.ChartSubTitle = txtChartSubTitle.Text;
            if (strata != null)
            {
                chartSettings.ChartStrataTitle = strata.Filter;
            }
            chartSettings.ChartWidth = int.Parse(txtWidth.Text);
            chartSettings.ChartHeight = int.Parse(txtHeight.Text);

            double pct = 20;
            double.TryParse(txtAnnotationPercent.Text, out pct);

            chartSettings.AnnotationPercent = pct;

            switch (cmbChartKind.SelectedIndex)
            {
                case 0:
                    chartSettings.PieChartKind = PieChartKind.Pie2D;
                    break;
                case 1:
                    chartSettings.PieChartKind = PieChartKind.Pie;
                    break;
                case 2:
                    chartSettings.PieChartKind = PieChartKind.Donut2D;
                    break;
                case 3:
                    chartSettings.PieChartKind = PieChartKind.Donut;
                    break;
            }

            chartSettings.LegendFontSize = double.Parse(txtLegendFontSize.Text);

            ComboBoxItem cbi = cmbPalette.SelectedItem as ComboBoxItem;
            ComponentArt.Win.DataVisualization.Palette palette = ComponentArt.Win.DataVisualization.Palette.GetPalette(cbi.Content.ToString());
            chartSettings.Palette = palette;

            chartSettings.ShowAnnotations = (bool)checkboxAnnotations.IsChecked;
            chartSettings.ShowAnnotationValue = (bool)checkboxAnnotationValue.IsChecked;
            chartSettings.ShowAnnotationLabel = (bool)checkboxAnnotationLabel.IsChecked;
            chartSettings.ShowAnnotationPercent = (bool)checkboxAnnotationPercent.IsChecked;

            chartSettings.ShowLegend = (bool)checkboxShowLegend.IsChecked;
            chartSettings.ShowLegendBorder = (bool)checkboxShowLegendBorder.IsChecked;
            chartSettings.ShowLegendVarNames = (bool)checkboxShowVarName.IsChecked;

            Controls.Charting.PieChart pieChart = new Controls.Charting.PieChart(DashboardHelper, GadgetOptions, chartSettings, dataList);
            pieChart.Margin = new Thickness(0, 0, 0, 16);
            //Grid.SetColumn(columnChart, 1);
            pieChart.MouseEnter += new MouseEventHandler(chart_MouseEnter);
            pieChart.MouseLeave += new MouseEventHandler(chart_MouseLeave);
            panelMain.Children.Add(pieChart);
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<h2>" + this.txtChartTitle.Text + "</h2>");
            sb.AppendLine("<h3>" + this.txtChartSubTitle.Text + "</h3>");

            foreach (UIElement element in panelMain.Children)
            {
                if (element is Controls.Charting.PieChart)
                {
                    sb.AppendLine(((Controls.Charting.PieChart)element).ToHTML(htmlFileName, count, true, false));
                    count++;
                }
            }

            return sb.ToString();
        }
    }
}
