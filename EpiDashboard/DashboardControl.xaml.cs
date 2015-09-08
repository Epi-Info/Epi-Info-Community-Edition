using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
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
using EpiDashboard;
using EpiDashboard.NutStat;
using EpiDashboard.Controls;
using EpiDashboard.Gadgets.Reporting;


namespace EpiDashboard 
{
    public delegate void NotificationButtonHandler();

    public delegate void StatusStripButtonHandler();

    public delegate void GadgetCloseButtonHandler();
    public delegate void GadgetConfigButtonHandler();
    public delegate void GadgetDescriptionButtonHandler();
    public delegate void GadgetOutputExpandButtonHandler();
    public delegate void GadgetOutputCollapseButtonHandler();
    public delegate void GadgetFilterButtonHandler();

    public delegate void StrataGridRowClickedHandler(UIElement control, string strataValue);
    public delegate void StrataGridExpandAllClickedHandler();

    public delegate void TwoByTwoValuesUpdatedHandler(UserControl control);
    public delegate void GadgetSetAdornerHandler(UserControl gadget);
    public delegate void GadgetRefreshedHandler(UserControl gadget);
    public delegate void GadgetClosingHandler(UserControl gadget);
    public delegate void GadgetProcessingFinishedHandler(UserControl gadget);

    public delegate void NotificationEventHandler(object sender, string message);
    public delegate DashboardHelper DashboardHelperRequestedHandler();
    public delegate RelatedConnection RelatedDataRequestedHandler();

    public delegate void SendNotificationMessageHandler(string message, bool showButtons, NotificationButtonType buttonType, bool requiresInteraction, int duration);

    public delegate void HTMLGeneratedHandler();
    public delegate void RecordCountChangedHandler(int recordCount, string dataSourceName);
    public delegate void CanvasChangedHandler(string canvasFilePath);
    public delegate void GadgetStatusUpdateHandler(string statusMessage);
    public delegate void GadgetProgressUpdateHandler(string statusMessage, double progress);
    public delegate bool GadgetCheckForCancellationHandler();

    public delegate void GadgetRepositionEventHandler(object sender, GadgetRepositionEventArgs e);
    public delegate void GadgetAnchorSetEventHandler(object sender, GadgetAnchorSetEventArgs e);
    public delegate void GadgetEventHandler(object sender);
    public delegate void GadgetPropertiesPopupRequestedHandler(object sender, GadgetPropertiesPopupRequestedEventArgs e);

    /// <summary>
    /// Interaction logic for DashboardControl.xaml
    /// </summary>
    public partial class DashboardControl : UserControl
    {
        #region Private Members
        private BackgroundWorker worker;
        private DataFilterControl dataFilteringControl;
        private VariablesControl variablesControl;
        private ExportControl dataExportingControl;
        private PropertiesControl propertiesControl;
        private DataDictionaryControl dataDictionaryControl;
        private DashboardPopup popup;

        private delegate void SimpleCallback();
        private delegate void SetStatusDelegate(string statusMessage);
        private delegate void RequestUpdateStatusDelegate(string statusMessage);
        private delegate bool CheckForCancellationDelegate();

        public event DashboardHelperRequestedHandler DashboardHelperRequested;
        public event RelatedDataRequestedHandler RelatedDataRequested;
        public event HTMLGeneratedHandler HTMLGenerated;
        public event RecordCountChangedHandler RecordCountChanged;
        public event CanvasChangedHandler CanvasChanged;
        private BackgroundWorker recordCountWorker;
        private Point mousePoint;
        private int tableFontSize;
        private string lastSavedOutputFileName;
        private string lastSavedCanvasFileName;
        private bool isWordInstalled = false;
        private bool isExcelInstalled = false;
        private bool isCreatingNewVariable = false;
        private bool isCreatingFilter = false;
        private const int DEFAULT_TABLE_FONT_SIZE = 13;
        private const int DEFAULT_CANVAS_HEIGHT = 8000;
        private const int DEFAULT_ANCHOR_TOP_SPACE = 10;
        private const int DEFAULT_ANCHOR_LEFT_SPACE = 10;
        private const double DEFAULT_CHART_WIDTH = 800;
        private const double DEFAULT_CHART_HEIGHT = 500;
        private XmlElement gadgetXmlElement = null;
        #endregion // Private Members

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public DashboardControl()
        {
            InitializeComponent();
            Construct();
            EnableDisableMenus(false);
            mnuDataSource.IsEnabled = true;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dashboardHelper">The dashboard helper to attach to this dashboard</param>
        public DashboardControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            this.DashboardHelper.IsAutoClosing = this.IsGeneratingHTMLFromCommandLine;
            Construct();
            dashboardHelper.GenerateRecordCount(false);
            //txtRecordCount.Text = dashboardHelper.RecordCount.ToString();
            UpdateRecordCount();
            mnuDataSource.Visibility = System.Windows.Visibility.Collapsed;

            if (DashboardHelperRequested == null)
            {
                grdMenuBarInner.ColumnDefinitions[3].Width = new GridLength(0);
                grdMenuBarInner.ColumnDefinitions[3].MinWidth = 0;
            }

            AddFilterGadget();
            AddDefinedVariablesGadget();
        }
        #endregion // Constructors

        private void Construct()
        {
            DefaultChartHeight = DEFAULT_CHART_HEIGHT;
            DefaultChartWidth = DEFAULT_CHART_WIDTH;

            Gadgets = new List<UserControl>();
            ZTop = 1;
            LoadInEditMode = true;
            this.IsPopulatingDataSet = false;
            this.SizeChanged += new SizeChangedEventHandler(DashboardControl_SizeChanged);
            scrollViewer.ScrollChanged += new ScrollChangedEventHandler(scrollViewer_ScrollChanged);
            statusStripButtonShowBorders.IsSelected = true;
            statusStripButtonShowBorders.Click += new StatusStripButtonHandler(statusStripButtonShowBorders_Click);

            statusStripButtonVerticalArrange.IsSelected = false;
            statusStripButtonVerticalArrange.Click += new StatusStripButtonHandler(statusStripButtonVerticalArrange_Click);

            mnuDataSource.Click += new RoutedEventHandler(mnuDataSource_Click);
            mnuExport.Click += new RoutedEventHandler(mnuExport_Click);
            mnuReset.Click += new RoutedEventHandler(mnuReset_Click);
            mnuRefresh.Click += new RoutedEventHandler(mnuRefresh_Click);

            mnuStatCalc2x2.Click += new RoutedEventHandler(mnuStatCalc2x2_Click);
            mnuChiSquare.Click += new RoutedEventHandler(mnuChiSquare_Click);
            mnuSaveOutput.Click += new RoutedEventHandler(mnuSaveOutput_Click);
            mnuProperties.Click += new RoutedEventHandler(mnuProperties_Click);
            mnuAutoArrange.Click += new RoutedEventHandler(mnuAutoArrange_Click);
            mnuDataDictionary.Click += new RoutedEventHandler(mnuDataDictionary_Click);

            mnuAddRelatedData.Click += new RoutedEventHandler(mnuAddRelatedData_Click);
            mnuOpen.Click += new RoutedEventHandler(mnuOpen_Click);
            mnuSave.Click += new RoutedEventHandler(mnuSave_Click);
            mnuSaveAs.Click += new RoutedEventHandler(mnuSaveAs_Click);

            foreach (Epi.DataSets.Config.GadgetRow row in Configuration.GetNewInstance().Gadget.Rows)
            {
                string type = row.Type;
                string displayName = row.DisplayName;

                GadgetMenuItem gmi = new GadgetMenuItem();
                gmi.GadgetType = type;
                gmi.Header = displayName;
                gmi.Click += new RoutedEventHandler(mnuGadgetMenuItem_Click);
                mnuCustomGadgets.Items.Add(gmi);
            }

            if (mnuCustomGadgets.Items.Count > 0)
            {
                mnuCustomGadgets.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                mnuCustomGadgets.Visibility = System.Windows.Visibility.Collapsed;
            }

            // Reports
            mnuStandardTextBox.Click += new RoutedEventHandler(mnuStandardTextBox_Click);
            mnuStandardImageBox.Click += new RoutedEventHandler(mnuStandardImageBox_Click);

            #region Growth Chart Add Events

            // CDC 2000 Reference menu options
            mnuCDC2000BMI.Click += new RoutedEventHandler(mnuCDC2000BMI_Click);
            mnuCDC2000HA.Click += new RoutedEventHandler(mnuCDC2000HA_Click);
            mnuCDC2000HCA.Click += new RoutedEventHandler(mnuCDC2000HCA_Click);
            mnuCDC2000LA.Click += new RoutedEventHandler(mnuCDC2000LA_Click);
            mnuCDC2000WA.Click += new RoutedEventHandler(mnuCDC2000WA_Click);
            mnuCDC2000WH.Click += new RoutedEventHandler(mnuCDC2000WH_Click);
            mnuCDC2000WL.Click += new RoutedEventHandler(mnuCDC2000WL_Click);

            mnuWHO1978HA.Click += new RoutedEventHandler(mnuWHO1978HA_Click);
            mnuWHO1978LA.Click += new RoutedEventHandler(mnuWHO1978LA_Click);
            mnuWHO1978WA.Click += new RoutedEventHandler(mnuWHO1978WA_Click);
            mnuWHO1978WH.Click += new RoutedEventHandler(mnuWHO1978WH_Click);
            mnuWHO1978WL.Click += new RoutedEventHandler(mnuWHO1978WL_Click);

            mnuWHOCGSBMI.Click += new RoutedEventHandler(mnuWHOCGSBMI_Click);
            mnuWHOCGSHA.Click += new RoutedEventHandler(mnuWHOCGSHA_Click);
            mnuWHOCGSHCA.Click += new RoutedEventHandler(mnuWHOCGSHCA_Click);
            mnuWHOCGSLA.Click += new RoutedEventHandler(mnuWHOCGSLA_Click);
            mnuWHOCGSWA.Click += new RoutedEventHandler(mnuWHOCGSWA_Click);
            mnuWHOCGSWH.Click += new RoutedEventHandler(mnuWHOCGSWH_Click);
            mnuWHOCGSWL.Click += new RoutedEventHandler(mnuWHOCGSWL_Click);
            mnuWHOCGSMUACA.Click += new RoutedEventHandler(mnuWHOCGSMUACA_Click);

            mnuWHO2007BMI.Click += new RoutedEventHandler(mnuWHO2007BMI_Click);
            mnuWHO2007HA.Click += new RoutedEventHandler(mnuWHO2007HA_Click);
            mnuWHO2007WA.Click += new RoutedEventHandler(mnuWHO2007WA_Click);

            #endregion // Growth Chart Add Events

            this.Loaded += new RoutedEventHandler(DashboardControl_Loaded);
            this.ContextMenuOpening += new ContextMenuEventHandler(DashboardControl_ContextMenuOpening);

            lastSavedOutputFileName = string.Empty;
            IsCanvasDirty = false;
            ResetCanvasProperties();
#if LINUX_BUILD
            
            mnuSendOutputTo.Visibility = Visibility.Collapsed;
#else
            mnuSendOutputTo.Visibility = Visibility.Collapsed;
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.ClassesRoot;
            Microsoft.Win32.RegistryKey excelKey = key.OpenSubKey("Excel.Application");
            isExcelInstalled = excelKey == null ? false : true;

            Microsoft.Win32.RegistryKey wordKey = key.OpenSubKey("Word.Application");
            isWordInstalled = wordKey == null ? false : true;

            if (isExcelInstalled)
            {
                mnuSendOutputToExcel.Click += new RoutedEventHandler(mnuSendOutputToExcel_Click);
                mnuSendOutputToExcel.Visibility = Visibility.Visible;
                mnuSendOutputTo.Visibility = Visibility.Visible;
            }
            if (isWordInstalled)
            {
                mnuSendOutputToWord.Click += new RoutedEventHandler(mnuSendOutputToWord_Click);
                mnuSendOutputToWord.Visibility = Visibility.Visible;
                mnuSendOutputTo.Visibility = Visibility.Visible;
            }
#endif

            CenterControlHorizontally(spTitle);
            CenterControlHorizontally(instructionsPanel);

            #region Translation
            // WPF has its own translation capabilities that would normally be used. However, Epi Info 7 has a translation 'tool' built in, and this
            // tool does not work with the WPF translation mechanism. To make this as simple as possible and play nice with the rest of EI7
            // we're just going to use a RESX file.
            tblockIntroPanelTitle.Text = DashboardSharedStrings.DASHBOARD_TITLE;
            tblockIntroText.Text = DashboardSharedStrings.DASHBOARD_INTRO_TEXT;
            tblockSetDataSourceNow.Text = DashboardSharedStrings.DASHBOARD_INTRO_SET_DATA_SOURCE;
            tblockCanvasFiles.Text = DashboardSharedStrings.CANVAS_FILES_HEADER;
            tblockFindMoreCanvasFiles.Text = DashboardSharedStrings.CANVAS_FILE_BROWSE;

            //tblockSetDataSource.Text = DashboardSharedStrings.MENU_SET_DATA_SOURCE;
            //tblockOpen.Text = DashboardSharedStrings.MENU_OPEN;
            //tblockSave.Text = DashboardSharedStrings.MENU_SAVE;
            //tblockSaveAs.Text = DashboardSharedStrings.MENU_SAVE_AS;

            tooltipSave.Text = DashboardSharedStrings.TOOLTIP_MENU_SAVE;
            //tooltipSaveAs.Text = DashboardSharedStrings.TOOLTIP_MENU_SAVE_AS;
            tooltipOpen.Text = DashboardSharedStrings.TOOLTIP_MENU_OPEN;
            tooltipSetDataSource.Text = DashboardSharedStrings.TOOLTIP_MENU_SET_DATA_SOURCE;
            tooltipRefresh.Text = DashboardSharedStrings.TOOLTIP_MENU_REFRESH;

            tooltipSetDataSourceHeader.Content = DashboardSharedStrings.TOOLTIP_MENU_SET_DATA_SOURCE_HEADER;
            tooltipOpenHeader.Content = DashboardSharedStrings.TOOLTIP_MENU_OPEN_HEADER;
            tooltipSaveHeader.Content = DashboardSharedStrings.TOOLTIP_MENU_SAVE_HEADER;
            //tooltipSaveAsHeader.Content = DashboardSharedStrings.TOOLTIP_MENU_SAVE_AS_HEADER;
            tooltipRefreshHeader.Content = DashboardSharedStrings.TOOLTIP_MENU_REFRESH_HEADER;

            instructionsPanel.DescriptionText = DashboardSharedStrings.INSTRUCTIONS;

            mnuDataSource.Header = DashboardSharedStrings.CMENU_SET_DATA_SOURCE;
            mnuAddRelatedData.Header = DashboardSharedStrings.CMENU_ADD_RELATED_DATA;
            mnuOpen.Header = DashboardSharedStrings.CMENU_OPEN_CANVAS;
            mnuSave.Header = DashboardSharedStrings.CMENU_SAVE_CANVAS;
            mnuSaveAs.Header = DashboardSharedStrings.CMENU_SAVE_CANVAS_AS;
            mnuSaveOutput.Header = DashboardSharedStrings.CMENU_SAVE_AS_HTML;
            mnuSendOutputTo.Header = DashboardSharedStrings.CMENU_SEND_OUTPUT_TO;
            mnuSendOutputToExcel.Header = DashboardSharedStrings.CMENU_MS_EXCEL;
            mnuSendOutputToWord.Header = DashboardSharedStrings.CMENU_MS_WORD;
            mnuExport.Header = DashboardSharedStrings.CMENU_EXPORT_DATA;
            mnuAddGadget.Header = DashboardSharedStrings.CMENU_ADD_ANALYSIS_GADGET;
            tblockAddStatCalcGadget.Text = DashboardSharedStrings.CMENU_ADD_STATCALC;
            tblockAddNutStatChart.Text = DashboardSharedStrings.CMENU_ADD_NUTSTAT;
            tblockAddReportGadget.Text = DashboardSharedStrings.CMENU_ADD_REPORT_GADGET;
            mnuStandardTextBox.Header = DashboardSharedStrings.CMENU_ADD_SIMPLE_TEXT_GADGET;
            mnuStandardImageBox.Header = DashboardSharedStrings.CMENU_ADD_IMAGE_GADGET;
            mnuDataDictionary.Header = DashboardSharedStrings.CMENU_SHOW_DATA_DICTIONARY;
            mnuProperties.Header = DashboardSharedStrings.CMENU_PROPERTIES;
            mnuAutoArrange.Header = DashboardSharedStrings.CMENU_AUTO_ARRANGE;
            mnuRefresh.Header = DashboardSharedStrings.CMENU_REFRESH;
            mnuReset.Header = DashboardSharedStrings.CMENU_RESET;

            mnuLineList.Header = DashboardSharedStrings.CMENU_ADD_LINE_LIST;
            mnuFrequency.Header = DashboardSharedStrings.CMENU_ADD_FREQUENCY;
            mnuCombinedFrequency.Header = DashboardSharedStrings.CMENU_ADD_COMBINED_FREQUENCY;
            mnuWordCloud.Header = DashboardSharedStrings.CMENU_ADD_WORD_CLOUD;
            mnuMxN.Header = DashboardSharedStrings.CMENU_ADD_MXN;
            mnuMeans.Header = DashboardSharedStrings.CMENU_ADD_MEANS;
            mnuPMCC.Header = DashboardSharedStrings.CMENU_ADD_MATCHED_PAIR_CASE_CONTROL;
            mnuAdvStats.Header = DashboardSharedStrings.CMENU_ADV_STATS;
            mnuLinear.Header = DashboardSharedStrings.CMENU_ADD_LINEAR;
            mnuLogistic.Header = DashboardSharedStrings.CMENU_ADD_LOGISTIC;
            mnuCSFrequency.Header = DashboardSharedStrings.CMENU_ADD_CSFREQ;
            mnuCSMeans.Header = DashboardSharedStrings.CMENU_ADD_CSMEANS;
            mnuCSTables.Header = DashboardSharedStrings.CMENU_ADD_CSTABLES;
            mnuCharts.Header = DashboardSharedStrings.CMENU_ADD_CHARTS;

            mnuColumnChart.Header = DashboardSharedStrings.CMENU_ADD_COLUMN_CHART;
            mnuLineChart.Header = DashboardSharedStrings.CMENU_ADD_LINE_CHART;
            mnuAreaChart.Header = DashboardSharedStrings.CMENU_ADD_AREA_CHART;
            mnuPieChart.Header = DashboardSharedStrings.CMENU_ADD_PIE_CHART;
            mnuAberrationChart.Header = DashboardSharedStrings.CMENU_ADD_ABERRATION_CHART;
            mnuScatterChart.Header = DashboardSharedStrings.CMENU_ADD_SCATTER_CHART;
            mnuParetoChart.Header = DashboardSharedStrings.CMENU_ADD_PARETO_CHART;
            mnuHistogramChart.Header = DashboardSharedStrings.CMENU_ADD_EPICURVE_CHART;

            mnuCustomGadgets.Header = DashboardSharedStrings.CMENU_CUSTOM_GADGETS;

            this.mnuChiSquare.Header = SharedStrings.DASHBOARD_CONTEXTMENU_ADD_STATCALC_CHI_GADGET;
            this.mnuCohort.Header = SharedStrings.DASHBOARD_CONTEXTMENU_ADD_STATCALC_COHORT_GADGET;
            this.mnuCox.Header = SharedStrings.DASHBOARD_CONTEXTMENU_ADD_COX_GADGET;
            this.mnuKM.Header = SharedStrings.DASHBOARD_CONTEXTMENU_ADD_KMSURVIVAL_GADGET;
            this.mnuPopSurvey.Header = SharedStrings.DASHBOARD_CONTEXTMENU_ADD_STATCALC_POPULATION_GADGET;
            this.mnuSampleSizePower.Header = SharedStrings.DASHBOARD_CONTEXTMENU_ADD_STATCALC_SAMPLE_SIZE_POWER_GADGET;
            this.mnuStatCalc2x2.Header = SharedStrings.DASHBOARD_CONTEXTMENU_ADD_STATCALC_2X2_GADGET;
            this.mnuUnmatched.Header = SharedStrings.DASHBOARD_CONTEXTMENU_ADD_STATCALC_UCC_GADGET;
            #endregion // Translation
        }

        void mnuAutoArrange_Click(object sender, RoutedEventArgs e)
        {
            AutoArrangeGadgets();
        }

        void statusStripButtonVerticalArrange_Click()
        {
            if (statusStripButtonVerticalArrange.IsSelected)
            {
                canvasMain.AllowDragging = false;
                mnuAutoArrange.IsEnabled = false;
                ArrangeGadgets();
            }
            else
            {
                canvasMain.AllowDragging = true;
                mnuAutoArrange.IsEnabled = true;
            }
        }

        void statusStripButtonShowBorders_Click()
        {
            if (statusStripButtonShowBorders.IsSelected)
            {
                ShowGadgetBorders();
            }
            else
            {
                HideGadgetBorders();
            }
        }

        private bool IsShowingGadgetBorders
        {
            get
            {
                return this.statusStripButtonShowBorders.IsSelected;
            }
        }

        private void ShowGadgetBorders()
        {
            foreach (UserControl element in this.Gadgets)
            {
                if (element is IReportingGadget)
                {
                    IReportingGadget reportGadget = element as IReportingGadget;
                    if (reportGadget != null)
                    {
                        reportGadget.Select();
                    }
                }
                if (element is GadgetBase)
                {
                    GadgetBase gadget = element as GadgetBase;
                    if (gadget != null)
                    {
                        gadget.DrawBorders = true;
                    }
                }
            }
        }

        private void HideGadgetBorders()
        {
            foreach (UserControl element in this.Gadgets)
            {
                if (element is IReportingGadget)
                {
                    IReportingGadget reportGadget = element as IReportingGadget;
                    if (reportGadget != null)
                    {
                        reportGadget.Deselect();
                    }
                }
                if (element is GadgetBase)
                {
                    GadgetBase gadget = element as GadgetBase;
                    if (gadget != null)
                    {
                        gadget.DrawBorders = false;
                        gadget.HideConfigPanel();
                    }
                }
            }
        }

        void mnuShowBorders_Click(object sender, RoutedEventArgs e)
        {
            ShowGadgetBorders();
        }

        void mnuHideBorders_Click(object sender, RoutedEventArgs e)
        {
            HideGadgetBorders();
        }

        void mnuAddRelatedData_Click(object sender, RoutedEventArgs e)
        {
            AddRelatedData();
        }

        void mnuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveCanvasAs();
        }

        void mnuSave_Click(object sender, RoutedEventArgs e)
        {
            SaveCanvas();
        }

        void mnuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenCanvas();
        }

        private void mnuSendOutputToWord_Click(object sender, RoutedEventArgs e)
        {
            if (this.DashboardHelper == null)
            {
                return;
            }

            try
            {
                string fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString("N") + ".html";//GetHTMLLineListing();

                System.IO.FileStream stream = System.IO.File.OpenWrite(fileName);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(stream);
                sw.WriteLine(this.ToHTML(fileName));
                sw.Close();
                sw.Dispose();

                if (!string.IsNullOrEmpty(fileName))
                {
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.FileName = "WINWORD.EXE";
                    proc.StartInfo.Arguments = "\"" + fileName + "\"";
                    proc.StartInfo.UseShellExecute = true;
                    proc.Start();
                }
            }
            finally
            {
            }
        }

        private void mnuSendOutputToExcel_Click(object sender, RoutedEventArgs e)
        {
            if (this.DashboardHelper == null)
            {
                return;
            }

            try
            {
                string fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString("N") + ".html";//GetHTMLLineListing();

                System.IO.FileStream stream = System.IO.File.OpenWrite(fileName);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(stream);
                sw.WriteLine(this.ToHTML(fileName)); //ei-279
                sw.Close();
                sw.Dispose();

                if (!string.IsNullOrEmpty(fileName))
                {
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.FileName = "excel";
                    proc.StartInfo.Arguments = "\"" + fileName + "\"";
                    proc.StartInfo.UseShellExecute = true;
                    proc.Start();
                }
            }
            finally
            {
            }
        }

        void mnuWHO2007BMI_Click(object sender, RoutedEventArgs e)
        {
            NutritionChartControl chartControl = new NutritionChartControl(DashboardHelper, GrowthMeasurement.BodyMassIndex, GrowthReference.WHO2007);
            canvasMain.Children.Add(chartControl);
            Canvas.SetLeft(chartControl, mousePoint.X - 10);
            Canvas.SetTop(chartControl, mousePoint.Y - 10);
            chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(chartControl);
            EnableDisableOptions();
        }

        void mnuWHO2007HA_Click(object sender, RoutedEventArgs e)
        {
            NutritionChartControl chartControl = new NutritionChartControl(DashboardHelper, GrowthMeasurement.HeightAge, GrowthReference.WHO2007);
            canvasMain.Children.Add(chartControl);
            Canvas.SetLeft(chartControl, mousePoint.X - 10);
            Canvas.SetTop(chartControl, mousePoint.Y - 10);
            chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(chartControl);
            EnableDisableOptions();
        }

        void mnuWHO2007WA_Click(object sender, RoutedEventArgs e)
        {
            NutritionChartControl chartControl = new NutritionChartControl(DashboardHelper, GrowthMeasurement.WeightAge, GrowthReference.WHO2007);
            canvasMain.Children.Add(chartControl);
            Canvas.SetLeft(chartControl, mousePoint.X - 10);
            Canvas.SetTop(chartControl, mousePoint.Y - 10);
            chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(chartControl);
            EnableDisableOptions();
        }

        void mnuWHOCGSBMI_Click(object sender, RoutedEventArgs e)
        {
            NutritionChartControl chartControl = new NutritionChartControl(DashboardHelper, GrowthMeasurement.BodyMassIndex, GrowthReference.WHO2006);
            canvasMain.Children.Add(chartControl);
            Canvas.SetLeft(chartControl, mousePoint.X - 10);
            Canvas.SetTop(chartControl, mousePoint.Y - 10);
            chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(chartControl);
            EnableDisableOptions();
        }

        void mnuWHOCGSHA_Click(object sender, RoutedEventArgs e)
        {
            NutritionChartControl chartControl = new NutritionChartControl(DashboardHelper, GrowthMeasurement.HeightAge, GrowthReference.WHO2006);
            canvasMain.Children.Add(chartControl);
            Canvas.SetLeft(chartControl, mousePoint.X - 10);
            Canvas.SetTop(chartControl, mousePoint.Y - 10);
            chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(chartControl);
            EnableDisableOptions();
        }

        void mnuWHOCGSHCA_Click(object sender, RoutedEventArgs e)
        {
            NutritionChartControl chartControl = new NutritionChartControl(DashboardHelper, GrowthMeasurement.HeadCircumferenceAge, GrowthReference.WHO2006);
            canvasMain.Children.Add(chartControl);
            Canvas.SetLeft(chartControl, mousePoint.X - 10);
            Canvas.SetTop(chartControl, mousePoint.Y - 10);
            chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(chartControl);
            EnableDisableOptions();
        }

        void mnuWHOCGSLA_Click(object sender, RoutedEventArgs e)
        {
            NutritionChartControl chartControl = new NutritionChartControl(DashboardHelper, GrowthMeasurement.LengthAge, GrowthReference.WHO2006);
            canvasMain.Children.Add(chartControl);
            Canvas.SetLeft(chartControl, mousePoint.X - 10);
            Canvas.SetTop(chartControl, mousePoint.Y - 10);
            chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(chartControl);
            EnableDisableOptions();
        }

        void mnuWHOCGSWA_Click(object sender, RoutedEventArgs e)
        {
            NutritionChartControl chartControl = new NutritionChartControl(DashboardHelper, GrowthMeasurement.WeightAge, GrowthReference.WHO2006);
            canvasMain.Children.Add(chartControl);
            Canvas.SetLeft(chartControl, mousePoint.X - 10);
            Canvas.SetTop(chartControl, mousePoint.Y - 10);
            chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(chartControl);
            EnableDisableOptions();
        }

        void mnuWHOCGSWH_Click(object sender, RoutedEventArgs e)
        {
            NutritionChartControl chartControl = new NutritionChartControl(DashboardHelper, GrowthMeasurement.WeightHeight, GrowthReference.WHO2006);
            canvasMain.Children.Add(chartControl);
            Canvas.SetLeft(chartControl, mousePoint.X - 10);
            Canvas.SetTop(chartControl, mousePoint.Y - 10);
            chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(chartControl);
            EnableDisableOptions();
        }

        void mnuWHOCGSWL_Click(object sender, RoutedEventArgs e)
        {
            NutritionChartControl chartControl = new NutritionChartControl(DashboardHelper, GrowthMeasurement.WeightLength, GrowthReference.WHO2006);
            canvasMain.Children.Add(chartControl);
            Canvas.SetLeft(chartControl, mousePoint.X - 10);
            Canvas.SetTop(chartControl, mousePoint.Y - 10);
            chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(chartControl);
            EnableDisableOptions();
        }

        void mnuWHOCGSMUACA_Click(object sender, RoutedEventArgs e)
        {
            NutritionChartControl chartControl = new NutritionChartControl(DashboardHelper, GrowthMeasurement.ArmCircumferenceAge, GrowthReference.WHO2006);
            canvasMain.Children.Add(chartControl);
            Canvas.SetLeft(chartControl, mousePoint.X - 10);
            Canvas.SetTop(chartControl, mousePoint.Y - 10);
            chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(chartControl);
            EnableDisableOptions();
        }


        void mnuWHO1978HA_Click(object sender, RoutedEventArgs e)
        {
            NutritionChartControl chartControl = new NutritionChartControl(DashboardHelper, GrowthMeasurement.HeightAge, GrowthReference.WHO1978);
            canvasMain.Children.Add(chartControl);
            Canvas.SetLeft(chartControl, mousePoint.X - 10);
            Canvas.SetTop(chartControl, mousePoint.Y - 10);
            chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(chartControl);
            EnableDisableOptions();
        }

        void mnuWHO1978LA_Click(object sender, RoutedEventArgs e)
        {
            NutritionChartControl chartControl = new NutritionChartControl(DashboardHelper, GrowthMeasurement.LengthAge, GrowthReference.WHO1978);
            canvasMain.Children.Add(chartControl);
            Canvas.SetLeft(chartControl, mousePoint.X - 10);
            Canvas.SetTop(chartControl, mousePoint.Y - 10);
            chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(chartControl);
            EnableDisableOptions();
        }

        void mnuWHO1978WA_Click(object sender, RoutedEventArgs e)
        {
            NutritionChartControl chartControl = new NutritionChartControl(DashboardHelper, GrowthMeasurement.WeightAge, GrowthReference.WHO1978);
            canvasMain.Children.Add(chartControl);
            Canvas.SetLeft(chartControl, mousePoint.X - 10);
            Canvas.SetTop(chartControl, mousePoint.Y - 10);
            chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(chartControl);
            EnableDisableOptions();
        }

        void mnuWHO1978WH_Click(object sender, RoutedEventArgs e)
        {
            NutritionChartControl chartControl = new NutritionChartControl(DashboardHelper, GrowthMeasurement.WeightHeight, GrowthReference.WHO1978);
            canvasMain.Children.Add(chartControl);
            Canvas.SetLeft(chartControl, mousePoint.X - 10);
            Canvas.SetTop(chartControl, mousePoint.Y - 10);
            chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(chartControl);
            EnableDisableOptions();
        }

        void mnuWHO1978WL_Click(object sender, RoutedEventArgs e)
        {
            NutritionChartControl chartControl = new NutritionChartControl(DashboardHelper, GrowthMeasurement.WeightLength, GrowthReference.WHO1978);
            canvasMain.Children.Add(chartControl);
            Canvas.SetLeft(chartControl, mousePoint.X - 10);
            Canvas.SetTop(chartControl, mousePoint.Y - 10);
            chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(chartControl);
            EnableDisableOptions();
        }


        void mnuCDC2000BMI_Click(object sender, RoutedEventArgs e)
        {
            NutritionChartControl chartControl = new NutritionChartControl(DashboardHelper, GrowthMeasurement.BodyMassIndex, GrowthReference.CDC2000);
            canvasMain.Children.Add(chartControl);
            Canvas.SetLeft(chartControl, mousePoint.X - 10);
            Canvas.SetTop(chartControl, mousePoint.Y - 10);
            chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(chartControl);
            EnableDisableOptions();
        }

        void mnuCDC2000HA_Click(object sender, RoutedEventArgs e)
        {
            NutritionChartControl chartControl = new NutritionChartControl(DashboardHelper, GrowthMeasurement.HeightAge, GrowthReference.CDC2000);
            canvasMain.Children.Add(chartControl);
            Canvas.SetLeft(chartControl, mousePoint.X - 10);
            Canvas.SetTop(chartControl, mousePoint.Y - 10);
            chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(chartControl);
            EnableDisableOptions();
        }

        void mnuCDC2000HCA_Click(object sender, RoutedEventArgs e)
        {
            NutritionChartControl chartControl = new NutritionChartControl(DashboardHelper, GrowthMeasurement.HeadCircumferenceAge, GrowthReference.CDC2000);
            canvasMain.Children.Add(chartControl);
            Canvas.SetLeft(chartControl, mousePoint.X - 10);
            Canvas.SetTop(chartControl, mousePoint.Y - 10);
            chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(chartControl);
            EnableDisableOptions();
        }

        void mnuCDC2000LA_Click(object sender, RoutedEventArgs e)
        {
            NutritionChartControl chartControl = new NutritionChartControl(DashboardHelper, GrowthMeasurement.LengthAge, GrowthReference.CDC2000);
            canvasMain.Children.Add(chartControl);
            Canvas.SetLeft(chartControl, mousePoint.X - 10);
            Canvas.SetTop(chartControl, mousePoint.Y - 10);
            chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(chartControl);
            EnableDisableOptions();
        }

        void mnuCDC2000WA_Click(object sender, RoutedEventArgs e)
        {
            NutritionChartControl chartControl = new NutritionChartControl(DashboardHelper, GrowthMeasurement.WeightAge, GrowthReference.CDC2000);
            canvasMain.Children.Add(chartControl);
            Canvas.SetLeft(chartControl, mousePoint.X - 10);
            Canvas.SetTop(chartControl, mousePoint.Y - 10);
            chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(chartControl);
            EnableDisableOptions();
        }

        void mnuCDC2000WH_Click(object sender, RoutedEventArgs e)
        {
            NutritionChartControl chartControl = new NutritionChartControl(DashboardHelper, GrowthMeasurement.WeightHeight, GrowthReference.CDC2000);
            canvasMain.Children.Add(chartControl);
            Canvas.SetLeft(chartControl, mousePoint.X - 10);
            Canvas.SetTop(chartControl, mousePoint.Y - 10);
            chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(chartControl);
            EnableDisableOptions();
        }

        void mnuCDC2000WL_Click(object sender, RoutedEventArgs e)
        {
            NutritionChartControl chartControl = new NutritionChartControl(DashboardHelper, GrowthMeasurement.WeightLength, GrowthReference.CDC2000);
            canvasMain.Children.Add(chartControl);
            Canvas.SetLeft(chartControl, mousePoint.X - 10);
            Canvas.SetTop(chartControl, mousePoint.Y - 10);
            chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(chartControl);
            EnableDisableOptions();
        }

        void mnuChiSquare_Click(object sender, RoutedEventArgs e)
        {
            StatCalc.ChiSquareControl chiSquare = new StatCalc.ChiSquareControl();
            canvasMain.Children.Add(chiSquare);
            Canvas.SetLeft(chiSquare, mousePoint.X - 10);
            Canvas.SetTop(chiSquare, mousePoint.Y - 10);
            chiSquare.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            chiSquare.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(chiSquare);
            EnableDisableOptions();
        }

        void DashboardControl_ContextMenuOpening(object sender, System.Windows.Controls.ContextMenuEventArgs e)
        {
            mousePoint = Mouse.GetPosition(canvasMain);
            introAvailableData.Visibility = System.Windows.Visibility.Collapsed;
        }

        void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.Source == this.scrollViewer)
            {
                if (dataFilteringControl != null)
                {
                    if (sliderZoom.Value == 100)
                    {
                        Canvas.SetTop(dataFilteringControl, e.VerticalOffset + (scrollViewer.ActualHeight / 2.0) - (dataFilteringControl.ActualHeight / 2.0));
                    }
                    else
                    {
                        double factor = sliderZoom.Value / 100.0;
                        Canvas.SetTop(dataFilteringControl, (e.VerticalOffset / factor) + (scrollViewer.ActualHeight / factor / 2.0) - (dataFilteringControl.ActualHeight / factor / 2.0));
                    }
                }
                if (variablesControl != null)
                {
                    if (sliderZoom.Value == 100)
                    {
                        Canvas.SetTop(variablesControl, e.VerticalOffset + (scrollViewer.ActualHeight / 2.0) - (variablesControl.ActualHeight / 2.0));
                    }
                    else
                    {
                        double factor = sliderZoom.Value / 100.0;
                        Canvas.SetTop(variablesControl, (e.VerticalOffset / factor) + (scrollViewer.ActualHeight / factor / 2.0) - (variablesControl.ActualHeight / factor / 2.0));
                    }
                }

                if (e.VerticalOffset > 0.0)
                {
                    introAvailableData.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        private void AddGadgetToCanvas(IGadget gadget)
        {
            gadget.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            gadget.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            gadget.GadgetRefreshed += new GadgetRefreshedHandler(gadget_GadgetRefreshed);
            gadget.GadgetReposition += new GadgetRepositionEventHandler(gadgetBase_GadgetReposition);
            gadget.GadgetDrag += new GadgetEventHandler(gadget_GadgetDrag);
            gadget.GadgetDragStart += new GadgetEventHandler(gadget_GadgetDragStart);
            gadget.GadgetDragStop += new GadgetEventHandler(gadget_GadgetDragStop);

            if (IsShowingGadgetBorders)
            {
                gadget.DrawBorders = true;
            }
            else
            {
                gadget.DrawBorders = false;
            }

            UserControl uc = gadget as UserControl;

            if (uc != null)
            {
                this.Gadgets.Add(uc);
                Canvas.SetZIndex(uc, ZTop);
            }
            ZTop++;
        }

        void gadget_GadgetAnchorSetFromXml(object sender, GadgetAnchorSetEventArgs e)
        {
            IGadget gadget = sender as IGadget;
            UserControl control = sender as UserControl;

            if (gadget != null && control != null)
            {
                Guid guid = e.Guid;

                var filteredGadgets = from g in Gadgets
                                      where ((IGadget)g).UniqueIdentifier == guid
                                      select g;

                foreach (UserControl targetControl in filteredGadgets)
                {
                    if (e.AnchorType == GadgetAnchorSetEventArgs.GadgetAnchorType.Left)
                    {
                        gadget.AnchorLeft = targetControl;
                        break;
                    }
                    if (e.AnchorType == GadgetAnchorSetEventArgs.GadgetAnchorType.Top)
                    {
                        gadget.AnchorTop = targetControl;
                        break;
                    }
                }
            }
        }

        void gadget_GadgetDragStop(object sender)
        {
            anchorLine.Visibility = Visibility.Collapsed;
        }

        void gadget_GadgetDragStart(object sender)
        {
        }

        private void gadget_GadgetDrag(object sender)
        {
            IGadget senderGadget = sender as IGadget;

            if (senderGadget != null)
            {
                foreach (IGadget gadget in this.Gadgets)
                {
                    if (gadget == senderGadget)
                    {
                        continue;
                    }

                    UserControl draggedControl = sender as UserControl;
                    UserControl control = gadget as UserControl;

                    if (draggedControl != null && control != null)
                    {
                        double targetLeft = Canvas.GetLeft(control);
                        double targetRight = Canvas.GetLeft(control) + control.ActualWidth;
                        double targetTop = Canvas.GetTop(control);
                        double targetBottom = Canvas.GetTop(control) + control.ActualHeight;

                        double draggedLeft = Canvas.GetLeft(draggedControl);
                        double draggedTop = Canvas.GetTop(draggedControl);

                        double yCoordCenter = 0;
                        double xCoordCenter = 0;

                        // condition for left anchoring
                        if (Math.Abs(draggedTop - targetTop) < 15 && draggedLeft >= targetRight && draggedLeft < (targetRight + 15))
                        {
                            anchorLine.Visibility = System.Windows.Visibility.Visible;

                            Canvas.SetLeft(draggedControl, Canvas.GetLeft(control) + control.ActualWidth + DEFAULT_ANCHOR_LEFT_SPACE);
                            Canvas.SetTop(draggedControl, Canvas.GetTop(control));

                            if (control.ActualHeight > draggedControl.ActualHeight)
                            {
                                yCoordCenter = (draggedControl.ActualHeight / 2) + Canvas.GetTop(control);
                            }
                            else
                            {
                                yCoordCenter = (control.ActualHeight / 2) + Canvas.GetTop(control);
                            }

                            anchorLine.X1 = Canvas.GetLeft(control) + control.ActualWidth;
                            anchorLine.Y1 = yCoordCenter;

                            anchorLine.X2 = Canvas.GetLeft(draggedControl);
                            anchorLine.Y2 = yCoordCenter;

                            ((IGadget)draggedControl).AnchorLeft = control;
                            ((IGadget)draggedControl).AnchorTop = null;

                            break;
                        }
                        // condition for top anchoring                
                        else if (Math.Abs(draggedLeft - targetLeft) < 15 && draggedTop >= targetBottom && draggedTop < (targetBottom + 15))
                        {
                            anchorLine.Visibility = System.Windows.Visibility.Visible;

                            Canvas.SetLeft(draggedControl, Canvas.GetLeft(control));
                            Canvas.SetTop(draggedControl, targetBottom + DEFAULT_ANCHOR_TOP_SPACE);

                            if (control.ActualWidth > draggedControl.ActualWidth)
                            {
                                xCoordCenter = (draggedControl.ActualWidth / 2) + Canvas.GetLeft(control);
                            }
                            else
                            {
                                xCoordCenter = (control.ActualWidth / 2) + Canvas.GetLeft(control);
                            }

                            anchorLine.X1 = xCoordCenter;
                            anchorLine.Y1 = Canvas.GetTop(control) + control.ActualHeight;

                            anchorLine.X2 = xCoordCenter;
                            anchorLine.Y2 = Canvas.GetTop(draggedControl);

                            ((IGadget)draggedControl).AnchorLeft = null;
                            ((IGadget)draggedControl).AnchorTop = control;

                            break;
                        }
                        else
                        {
                            anchorLine.Visibility = System.Windows.Visibility.Collapsed;
                            ((IGadget)draggedControl).AnchorLeft = null;
                            ((IGadget)draggedControl).AnchorTop = null;
                        }
                    }
                }

                MoveAnchoredGadgets(senderGadget);
            }
        }

        private void MoveAnchoredGadgets(IGadget source)
        {
            foreach (IGadget gadget in this.Gadgets)
            {
                if (gadget.AnchorLeft == source)
                {
                    UserControl control = source as UserControl;
                    UserControl anchoredControl = gadget as UserControl;
                    IGadget anchoredGadget = anchoredControl as IGadget;
                    if (control != null && anchoredControl != null && anchoredGadget != null)
                    {
                        Canvas.SetTop(anchoredControl, Canvas.GetTop(control));
                        Canvas.SetLeft(anchoredControl, Canvas.GetLeft(control) + control.ActualWidth + DEFAULT_ANCHOR_LEFT_SPACE);
                        MoveAnchoredGadgets(anchoredGadget);
                    }
                }
                else if (gadget.AnchorTop == source)
                {
                    UserControl control = source as UserControl;
                    UserControl anchoredControl = gadget as UserControl;
                    IGadget anchoredGadget = anchoredControl as IGadget;
                    if (control != null && anchoredControl != null && anchoredGadget != null)
                    {
                        Canvas.SetTop(anchoredControl, Canvas.GetTop(control) + control.ActualHeight + DEFAULT_ANCHOR_TOP_SPACE);
                        Canvas.SetLeft(anchoredControl, Canvas.GetLeft(control));
                        MoveAnchoredGadgets(anchoredGadget);
                    }
                }
            }
        }

        public void AddGadgetToCanvasFromContextMenu(UserControl control)
        {
            if (control != null && control is IGadget)
            {
                IGadget gadget = control as IGadget;

                if (gadget != null)
                {
                    canvasMain.Children.Add(control);
                    Canvas.SetLeft(control, mousePoint.X - 10);
                    Canvas.SetTop(control, mousePoint.Y - 10);

                    AddGadgetToCanvas(gadget);
                    IsCanvasDirty = true;
                    if (CanvasChanged != null)
                    {
                        CanvasChanged(this.CurrentCanvas);
                    }
                }
            }
            EnableDisableOptions();
            ArrangeGadgets();
            introAvailableData.Visibility = System.Windows.Visibility.Collapsed;
        }

        public void AddGadgetToCanvasFromFile(IGadget gadget)
        {
            AddGadgetToCanvas(gadget);
            canvasMain.Children.Add((UserControl)gadget);
            ArrangeGadgets();
            EnableDisableOptions();
        }

        void gadgetBase_GadgetReposition(object sender, GadgetRepositionEventArgs e)
        {
            if (sender != null && sender is UserControl)
            {
                UserControl uc = sender as UserControl;
                if (uc != null)
                {
                    CenterControlHorizontally(uc);
                }
            }
        }

        public void AddRelatedData()
        {
            if (RelatedDataRequested != null)
            {
                RelatedConnection rConn = RelatedDataRequested();

                if (rConn != null)
                {
                    DashboardHelper.AddRelatedDataSource(rConn);
                    dataFilteringControl.FillSelectionComboboxes();
                }
            }
        }

        /// <summary>
        /// Used to push a status message to the gadget's status panel
        /// </summary>
        /// <param name="statusMessage">The status message to display</param>
        private void RequestUpdateStatusMessage(string statusMessage)
        {
            this.Dispatcher.BeginInvoke(new SetStatusDelegate(SetStatusMessage), statusMessage);
        }

        /// <summary>
        /// Used to sets the gadget's current status, e.g. "Processing results..." or "Displaying output..."
        /// </summary>
        /// <param name="statusMessage">The status message to display</param>
        private void SetStatusMessage(string statusMessage)
        {
            tblockInstructions.Text = statusMessage;
            loadingPanel.DescriptionText = statusMessage;
            CenterControlHorizontally(spTitle);
        }

        /// <summary>
        /// Used to check if the user took an action that effectively cancels the currently-running worker
        /// thread. This could include: Closing the gadget, refreshing the dashboard, or selecting another
        /// means variable from the list of variables.
        /// </summary>
        /// <returns>Bool indicating whether or not the gadget's processing thread has been cancelled by the user</returns>
        private bool IsCancelled()
        {
            if (worker != null && worker.WorkerSupportsCancellation && worker.CancellationPending)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void DisablePermanentGadgets()
        {
            dataFilteringControl.IsEnabled = false;
            variablesControl.IsEnabled = false;
        }

        private void EnablePermanentGadgets()
        {
            dataFilteringControl.IsEnabled = true;
            variablesControl.IsEnabled = true;
        }

        private void AppendChildNodes(XmlElement root, XmlDocument doc)
        {
            #region Input Validation
            if (root == null) { throw new ArgumentNullException("root"); }
            if (doc == null) { throw new ArgumentNullException("doc"); }
            #endregion // Input Validation

            root.AppendChild(DashboardHelper.Serialize(doc));
            root.AppendChild(this.SerializeGadgets(doc));
            root.AppendChild(this.SerializeOutputSettings(doc));
            root.AppendChild(this.SerializeCanvasSettings(doc));
        }

        //private DashboardHelper DashboardHelperRequested()
        //{
        //    popup = new DashboardPopup();
        //    popup.Parent = grdMain;
        //    SetDataSource setDataSource = new SetDataSource();

        //    if (DashboardHelper != null && DashboardHelper.Database != null && DashboardHelper.IsUsingEpiProject)
        //    {
        //        setDataSource = new SetDataSource(DashboardHelper.View.Project);
        //    }

        //    setDataSource.Width = 800;
        //    setDataSource.Height = 600;

        //    setDataSource.Cancelled += new EventHandler(setDataSource_Cancelled);
        //    setDataSource.ChangesAccepted += new EventHandler(setDataSource_ChangesAccepted);
        //    popup.Content = setDataSource;
        //    popup.Show();

        //    if (dashboardHelper != null && dashboardHelper.Database != null && !dashboardHelper.IsUsingEpiProject)
        //    {
        //        newCanvasWindow = new Epi.WPF.Dashboard.Dialogs.NewCanvasWindow(dashboardHelper.Database);
        //    }
        //    else
        //        return null;
        //    if (newCanvasWindow.ShowDialog() == true)
        //    {
        //        if (newCanvasWindow.SelectedDataSource is Project)
        //        {
        //            Project project = (Project)newCanvasWindow.SelectedDataSource;
        //            View view = project.GetViewByName(newCanvasWindow.SelectedDataMember);

        //            if (!File.Exists(project.FilePath))
        //            {
        //                Epi.WPF.Dashboard.Dialogs.MsgBox.ShowInformation(string.Format(SharedStrings.DASHBOARD_ERROR_PROJECT_NOT_FOUND, project.FilePath));
        //                return null;
        //            }
        //            IDbDriver dbDriver = DBReadExecute.GetDataDriver(project.FilePath);
        //            if (!dbDriver.TableExists(view.TableName))
        //            {
        //                Epi.WPF.Dashboard.Dialogs.MsgBox.ShowInformation(string.Format(SharedStrings.DATA_TABLE_NOT_FOUND, view.Name));
        //                return null;
        //            }
        //            else
        //            {
        //                Configuration.OnDataSourceAccessed(project.Name, Configuration.Encrypt(project.FilePath), newCanvasWindow.SelectedDataProvider);
        //                dashboardHelper = new Epi.WPF.Dashboard.DashboardHelper(view, dbDriver);
        //            }
        //        }
        //        else
        //        {
        //            IDbDriver dbDriver = (IDbDriver)newCanvasWindow.SelectedDataSource;
        //            string dbName = dbDriver.DbName;
        //            //if (string.IsNullOrEmpty(newCanvasWindow.SQLQuery))
        //            //{
        //            Configuration.OnDataSourceAccessed(dbName, Configuration.Encrypt(dbDriver.ConnectionString), newCanvasWindow.SelectedDataProvider);
        //            dashboardHelper = new Epi.WPF.Dashboard.DashboardHelper(newCanvasWindow.SelectedDataMember, dbDriver);
        //            //}
        //            //else
        //            //{
        //            //    dashboardHelper = new Epi.WPF.Dashboard.DashboardHelper(newCanvasWindow.SelectedDataMember, newCanvasWindow.SQLQuery, dbDriver);
        //            //}
        //        }

        //        return dashboardHelper;
        //    }
        //    return null;
        //}

        public void SetDataSource()
        {
            if (!CanCloseDashboard())
            {
                return;
            }

            if (worker != null && worker.WorkerSupportsCancellation == true)
            {
                worker.CancelAsync();
            }

            if (DashboardHelperRequested != null)
            {
                gadgetXmlElement = null;
                DashboardHelper temp = DashboardHelperRequested();

                if (temp != null)
                {
                    DefaultChartWidth = DEFAULT_CHART_WIDTH;
                    DefaultChartHeight = DEFAULT_CHART_HEIGHT;

                    ZTop = 1;
                    statusStripButtonVerticalArrange.IsSelected = false;
                    canvasMain.AllowDragging = true;
                    temp.NotificationEvent += new NotificationEventHandler(dashboardHelper_NotificationEvent);
                    instructionsPanel.Visibility = Visibility.Collapsed;
                    loadingPanel.Visibility = System.Windows.Visibility.Visible;
                    CenterControlHorizontally(loadingPanel);

                    DashboardHelper = temp;
                    DashboardHelper.IsAutoClosing = IsGeneratingHTMLFromCommandLine;
                    DashboardHelper.SetDashboardControl(this);

                    AddFilterGadget();
                    AddDefinedVariablesGadget();
                    DisablePermanentGadgets();

                    infoPanel.Visibility = Visibility.Collapsed;
                    infoPanel.Text = string.Empty;

                    worker = new System.ComponentModel.BackgroundWorker();
                    worker.WorkerSupportsCancellation = true;
                    worker.DoWork += new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
                    worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(worker_WorkerCompleted);
                    worker.RunWorkerAsync();

                    RemoveAllGadgets();
                    ResetCanvasProperties();
                    EnableDisableMenus(true);
                    this.CurrentCanvas = string.Empty;
                    this.lastSavedCanvasFileName = string.Empty;

                    if (CanvasChanged != null)
                    {
                        CanvasChanged(string.Empty);
                    }

                    UpdateRecordCount();

                    this.mnuCombinedFrequency.Visibility = Visibility.Visible;
                    this.scrollViewer.ScrollToTop();
                }
            }
        }

        public void UpdateRecordCount()
        {
            if (introPanel != null)
            {
                introPanel.Visibility = System.Windows.Visibility.Collapsed;
                canvasMain.Children.Remove(introPanel);
                introPanel = null;
            }

            if (introAvailableData.Visibility == System.Windows.Visibility.Visible)
            {
                introAvailableData.Visibility = System.Windows.Visibility.Collapsed;
            }

            panelAddSpace.Visibility = System.Windows.Visibility.Visible;

            if (!string.IsNullOrEmpty(DashboardHelper.CustomQuery))
            {
                tblockDataSource.Text = DashboardSharedStrings.CUSTOM_QUERY;
            }
            else if (string.IsNullOrEmpty(DashboardHelper.TableName))
            {
                try
                {
                    tblockDataSource.Text = DashboardHelper.View.Project.Name + "\\" + DashboardHelper.View.Name;
                }
                catch (Exception ex)
                {
                    Epi.Windows.MsgBox.ShowError(SharedStrings.ERROR_CONNECT_DATA_SOURCE);
                    worker.CancelAsync();
                    return;
                }
            }
            else if (string.IsNullOrEmpty(DashboardHelper.CustomQuery))
            {
                tblockDataSource.Text = DashboardHelper.Database.DbName + "\\" + DashboardHelper.TableName;
            }
            else
            {
                tblockDataSource.Text = DashboardHelper.Database.DbName;
            }


            string recordCounter = string.Format(DashboardSharedStrings.RECORD_COUNT, DashboardHelper.RecordCount.ToString("N0"));
            tblockRecordCount.Text = recordCounter;
            UpdateFieldCounter();

            if (DashboardHelper.Database != null)
            {
                tooltipDataSourceLabel.Text = DashboardHelper.Database.ConnectionDescription;
            }

            grdDataSource.Visibility = System.Windows.Visibility.Visible;

            UpdateFieldCounter();
            tblockFieldCount.Visibility = System.Windows.Visibility.Visible;

            if (RecordCountChanged != null)
            {
                if (!string.IsNullOrEmpty(DashboardHelper.CustomQuery))
                {
                    RecordCountChanged(DashboardHelper.RecordCount, DashboardSharedStrings.CUSTOM_QUERY);
                }
                else if (DashboardHelper.IsUsingEpiProject)
                {
                    RecordCountChanged(DashboardHelper.RecordCount, DashboardHelper.View.Project.Name + "\\" + DashboardHelper.View.Name);
                }
                else
                {
                    RecordCountChanged(DashboardHelper.RecordCount, DashboardHelper.Database.DbName + "\\" + DashboardHelper.TableName);
                }
            }
        }

        public void ReCacheDataSource(bool clearEverything = true)
        {
            progressBar.Visibility = System.Windows.Visibility.Collapsed;

            if (worker != null && worker.WorkerSupportsCancellation == true)
            {
                worker.CancelAsync();
            }

            if (variablesControl != null && variablesControl != null)
            {
                DisablePermanentGadgets();
            }

            DashboardHelper.DataSet.Tables.Clear();
            waitCursor.Visibility = System.Windows.Visibility.Visible;
            CenterControlHorizontally(spTitle);
            CenterControlHorizontally(instructionsPanel);
            instructionsPanel.Visibility = Visibility.Collapsed;

            loadingPanel.Visibility = System.Windows.Visibility.Visible;
            CenterControlHorizontally(loadingPanel);

            worker = new System.ComponentModel.BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(worker_WorkerCompleted);
            worker.RunWorkerAsync(clearEverything);

            if (clearEverything)
            {
                RemoveAllGadgets();
            }

            EnableDisableMenus(false);
            mnuExport.IsEnabled = false;
            mnuProperties.IsEnabled = false;
            mnuAutoArrange.IsEnabled = false;
            mnuDataDictionary.IsEnabled = false;

            UpdateRecordCount();
            this.mnuCombinedFrequency.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handles the WorkerCompleted event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void worker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            instructionsPanel.DescriptionText = SharedStrings.DASHBOARD_INSTRUCTIONS_FINISHED_CACHING;
            instructionsPanel.Visibility = System.Windows.Visibility.Visible;
            CenterControlHorizontally(instructionsPanel);

            this.tblockInstructions.Text = SharedStrings.DASHBOARD_INSTRUCTIONS_FINISHED_CACHING;
            waitCursor.Visibility = System.Windows.Visibility.Hidden;
            IsPopulatingDataSet = false;
            mnuExport.IsEnabled = true;
            mnuProperties.IsEnabled = true;
            if (canvasMain.AllowDragging == true)
            {
                mnuAutoArrange.IsEnabled = true;
            }
            else
            {
                mnuAutoArrange.IsEnabled = false;
            }
            mnuDataDictionary.IsEnabled = true;
            EnableDisableMenus(true);
            EnableDisableOptions();

            UpdateRecordCount();

            EnablePermanentGadgets();

            progressBar.Value = 0;
            progressBar.Visibility = System.Windows.Visibility.Collapsed;
            CenterControlHorizontally(spTitle);

            CenterControlHorizontally(instructionsPanel);

            CenterControlHorizontally(loadingPanel);
            loadingPanel.Visibility = System.Windows.Visibility.Collapsed;

            if (e != null && e.Result != null && e.Result is bool && (bool)e.Result == true)
            {
                RefreshResults();
            }

            dataFilteringControl.FillSelectionComboboxes(true);

            AddGadgetsFromXml();
            AddResizeAdorners();

            UpdateAllGadgetVariableNames();

            dataDisplay.SetDataView(DashboardHelper.GenerateView());
            dataDisplay.Refresh();

            dataDictionary.AttachDashboardHelper(this.DashboardHelper);
            dataDictionary.SetDataView(this.DashboardHelper.GenerateDataDictionaryTable().DefaultView);
            dataDictionary.Refresh();

            cmbView.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// Handles the DoWorker event for the worker
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            bool needsRefresh = false;

            if (e != null && e.Argument is bool)
            {
                needsRefresh = !((bool)e.Argument);
            }
            IsPopulatingDataSet = true;
            RequestUpdateStatusDelegate requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
            CheckForCancellationDelegate checkForCancellation = new CheckForCancellationDelegate(IsCancelled);
            SetGadgetStatusHandler setGadgetProgress = new SetGadgetStatusHandler(RequestProgressUpdate);
            GadgetParameters inputs = new GadgetParameters();
            inputs.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
            inputs.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);
            inputs.GadgetCheckForProgress += new SetGadgetStatusHandler(setGadgetProgress);

            try
            {
                DashboardHelper.UserVarsNeedUpdating = true;
                DashboardHelper.PopulateDataSet(inputs);
            }
            catch (System.Data.EvaluateException)
            {
                this.Dispatcher.BeginInvoke(new SendNotificationMessageHandler(SendNotificationMessage), DashboardSharedStrings.ERROR_FILTER_NO_LONGER_VALID, true, NotificationButtonType.OK, true, 10);
            }
            catch (Exception ex)
            {
                Epi.Windows.MsgBox.ShowException(ex);
            }

            e.Result = needsRefresh;
        }

        /// <summary>
        /// Used to push a status message to the gadget's status panel
        /// </summary>
        /// <param name="statusMessage">The status message to display</param>
        private void RequestProgressUpdate(string statusMessage, double progress)
        {
            if (progress > 0)
            {
                this.Dispatcher.BeginInvoke(new SetProgressBarDelegate(SetProgressBarValue), progress);
            }
        }

        /// <summary>
        /// Used to set the value of the progress bar.
        /// </summary>
        /// <param name="progress">The progress value to use</param>
        private void SetProgressBarValue(double progress)
        {
            progressBar.Value = progress;

            if (progressBar.Value == 0)
            {
                progressBar.Visibility = System.Windows.Visibility.Collapsed;
                CenterControlHorizontally(spTitle);
            }
            else
            {
                progressBar.Visibility = System.Windows.Visibility.Visible;
                CenterControlHorizontally(spTitle);
            }
        }

        void mnuDataSource_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetDataSource();
            }
            catch
            {
                Epi.Windows.MsgBox.ShowError(DashboardSharedStrings.ERROR_DATA_SOURCE_GENERIC); // todo: Move to a non-modal dialog
            }
        }

        void mnuExport_Click(object sender, RoutedEventArgs e)
        {
            if (DashboardHelper != null)
            {
                ExportWindow exportWindow = new ExportWindow(this.DashboardHelper);
                exportWindow.ShowDialog();
            }
        }

        private void EnableDisableMenus(bool enabled)
        {
            mnuDataSource.IsEnabled = enabled;
            mnuAddGadget.IsEnabled = enabled;
            mnuGrowthCharts.IsEnabled = enabled;
            mnuAddRelatedData.IsEnabled = enabled;
            mnuSave.IsEnabled = enabled;
            mnuSaveAs.IsEnabled = enabled;
            mnuSaveOutput.IsEnabled = enabled;
            mnuSendOutputTo.IsEnabled = enabled;
            mnuSendOutputToExcel.IsEnabled = enabled;
            mnuSendOutputToWord.IsEnabled = enabled;
        }

        void DashboardControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DashboardHelper != null)
            {
                AddFilterGadget();
                AddDefinedVariablesGadget();
            }
            panelNotification.Visibility = System.Windows.Visibility.Collapsed;
            panelNotification.Width = System.Windows.SystemParameters.PrimaryScreenWidth / 1.5;
            Grid.SetZIndex(panelNotification, 2000);

            CenterControlHorizontally(introPanel);
            CenterControlHorizontally(introAvailableData);
            CenterControlHorizontally(spTitle);
            CenterControlHorizontally(instructionsPanel);

            if (System.Windows.SystemParameters.PrimaryScreenHeight < 1024)
            {
                svAvailableCanvasFiles.MaxHeight = System.Windows.SystemParameters.PrimaryScreenHeight - Canvas.GetTop(introAvailableData) - 250;
            }

            if ((System.Windows.SystemParameters.PrimaryScreenWidth - 45) > canvasMain.ActualWidth)
            {
                canvasMain.Width = System.Windows.SystemParameters.PrimaryScreenWidth - 45;
            }
            else
            {
                canvasMain.Width = canvasMain.ActualWidth;
            }
            LoadQuickDataSources();
            canvasMain.DragStarted += new DragEventHandler(canvasMain_DragStarted);
        }

        private void LoadQuickDataSources()
        {
            Configuration config = null;

            if (DashboardHelper != null)
            {
                config = DashboardHelper.Config;
            }
            else
            {
                config = Configuration.GetNewInstance();
            }

            if (grdRecentDataSources.RowDefinitions.Count > 0)
            {
                grdRecentDataSources.RowDefinitions.Clear();
                grdRecentDataSources.Children.Clear();
            }

            DirectoryInfo di = new DirectoryInfo(config.Directories.Project);

            int count = 0;

            List<string> fileNames = new List<string>();

            foreach (DirectoryInfo projectDi in di.GetDirectories())
            {
                FileInfo[] fi = projectDi.GetFiles("*.cvs7");
                foreach (FileInfo canvasFi in fi)
                {
                    fileNames.Add(canvasFi.FullName);
                    count++;
                }
            }

            if (Directory.Exists(config.Directories.Working))
            {
                di = new DirectoryInfo(config.Directories.Working);
                foreach (DirectoryInfo projectDi in di.GetDirectories())
                {
                    FileInfo[] fi = projectDi.GetFiles("*.cvs7");

                    foreach (FileInfo canvasFi in fi)
                    {
                        fileNames.Add(canvasFi.FullName);
                        count++;
                    }
                }
            }

            var sort = from fn in fileNames.ToArray()
                       orderby new FileInfo(fn).LastWriteTime descending
                       select fn;

            foreach (string n in sort)
            {
                FileInfo canvasFi = new FileInfo(n);
                LoadQuickDataSource(canvasFi);
            }
        }

        private void LoadQuickDataSource(FileInfo fi)
        {
            int rowDefStart = grdRecentDataSources.RowDefinitions.Count;

            RowDefinition rowDef1 = new RowDefinition();
            rowDef1.Height = GridLength.Auto;

            RowDefinition rowDef2 = new RowDefinition();
            rowDef2.Height = GridLength.Auto;

            RowDefinition rowDef3 = new RowDefinition();
            rowDef3.Height = new GridLength(8);

            grdRecentDataSources.RowDefinitions.Add(rowDef1);
            grdRecentDataSources.RowDefinitions.Add(rowDef2);
            grdRecentDataSources.RowDefinitions.Add(rowDef3);

            Border highlightBorder = new Border();
            highlightBorder.Tag = fi;
            highlightBorder.Style = this.Resources["recentDataHighlightBorderStyle"] as Style;
            highlightBorder.MouseLeftButtonUp += new MouseButtonEventHandler(highlightBorder_MouseLeftButtonUp);
            Grid.SetRow(highlightBorder, rowDefStart);
            Grid.SetColumn(highlightBorder, 0);
            Grid.SetRowSpan(highlightBorder, 2);
            Grid.SetColumnSpan(highlightBorder, 2);
            grdRecentDataSources.Children.Add(highlightBorder);

            TextBlock txt = new TextBlock();
            txt.Text = fi.Name;
            txt.IsHitTestVisible = false;
            txt.FontSize = 13;
            Grid.SetRow(txt, rowDefStart);
            Grid.SetColumn(txt, 1);
            grdRecentDataSources.Children.Add(txt);

            txt = new TextBlock();
            txt.Text = fi.DirectoryName;
            txt.IsHitTestVisible = false;
            Grid.SetRow(txt, rowDefStart + 1);
            Grid.SetColumn(txt, 1);
            txt.Foreground = Brushes.Gray;
            grdRecentDataSources.Children.Add(txt);

            System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();
            path.Data = this.Resources["pathGeometryDocumentIcon"] as PathGeometry;
            path.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            path.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            path.IsHitTestVisible = false;

            ScaleTransform transform = new ScaleTransform(0.30, 0.30);
            PathGeometry geometryTransformed = Geometry.Combine(Geometry.Empty, (path.Data as PathGeometry), GeometryCombineMode.Union, transform);
            PathGeometry newGeometry = geometryTransformed;
            path.Data = newGeometry;

            Grid.SetRow(path, rowDefStart);
            Grid.SetRowSpan(path, 2);
            Grid.SetColumn(path, 0);

            path.Fill = Brushes.Gray;

            grdRecentDataSources.Children.Add(path);
        }

        void highlightBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border)
            {
                Border border = sender as Border;
                if (border != null && border.Tag != null && border.Tag is FileInfo)
                {
                    if (CanCloseDashboard())
                    {
                        FileInfo fi = border.Tag as FileInfo;
                        if (fi != null)
                        {
                            OpenCanvas(fi.FullName);
                        }
                    }
                }
            }
        }

        void CenterControlHorizontally(FrameworkElement control)
        {
            if (control != null)
            {
                double actualCanvasWidth = canvasMain.ActualWidth;
                double actualControlWidth = control.ActualWidth;
                if (actualCanvasWidth > actualControlWidth)
                {
                    double diff = actualCanvasWidth - actualControlWidth;
                    double left = diff / 2;
                    Canvas.SetLeft(control, left);
                }
            }
        }

        void canvasMain_DragStarted(object sender, DragEventArgs e)
        {
            this.IsCanvasDirty = true;
            if (CanvasChanged != null)
            {
                CanvasChanged(this.CurrentCanvas);
            }
        }

        void mnuStandardImageBox_Click(object sender, RoutedEventArgs e)
        {
            EpiDashboard.Gadgets.Reporting.StandardImageControl ic = new EpiDashboard.Gadgets.Reporting.StandardImageControl(this.DashboardHelper, this);
            canvasMain.Children.Add(ic);
            Canvas.SetLeft(ic, mousePoint.X - 10);
            Canvas.SetTop(ic, mousePoint.Y - 10);
            ic.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            ic.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(ic);
            ArrangeGadgets();
            EnableDisableOptions();
        }

        void mnuStandardTextBox_Click(object sender, RoutedEventArgs e)
        {
            EpiDashboard.Gadgets.Reporting.StandardTextControl stc = new EpiDashboard.Gadgets.Reporting.StandardTextControl();
            canvasMain.Children.Add(stc);
            Canvas.SetLeft(stc, mousePoint.X - 10);
            Canvas.SetTop(stc, mousePoint.Y - 10);
            stc.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            stc.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(stc);
            ArrangeGadgets();
            EnableDisableOptions();

            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(stc);
            adornerLayer.Add(new ResizingAdorner(stc));
        }

        void mnuStatCalc2x2_Click(object sender, RoutedEventArgs e)
        {
            StatCalc.TwoByTwo twoByTwo = new StatCalc.TwoByTwo();
            canvasMain.Children.Add(twoByTwo);
            Canvas.SetLeft(twoByTwo, mousePoint.X - 10);
            Canvas.SetTop(twoByTwo, mousePoint.Y - 10);
            twoByTwo.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            twoByTwo.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            Gadgets.Add(twoByTwo);
            EnableDisableOptions();
        }

        void mnuRefresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        void mnuReset_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.DialogResult result = Epi.Windows.MsgBox.ShowQuestion(DashboardSharedStrings.GADGET_ERASE_ALL_FROM_CANVAS);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                RemoveAllGadgets();
                this.progressBar.Visibility = System.Windows.Visibility.Collapsed;
                CenterControlHorizontally(spTitle);
                CenterControlHorizontally(instructionsPanel);
            }
        }

        private void RemoveGadget(UserControl gadget)
        {
            if (gadget is IGadget)
            {
                ((IGadget)gadget).GadgetClosing -= new GadgetClosingHandler(gadget_GadgetClosing);
                ((IGadget)gadget).GadgetProcessingFinished -= new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
                ((IGadget)gadget).GadgetRefreshed -= new GadgetRefreshedHandler(gadget_GadgetRefreshed);
                ((IGadget)gadget).GadgetReposition -= new GadgetRepositionEventHandler(gadgetBase_GadgetReposition);
                Gadgets.Remove(gadget);
            }
            canvasMain.Children.Remove(gadget);
        }

        private void RemoveAllGadgets()
        {
            while (Gadgets.Count > 0)
            {
                RemoveGadget(Gadgets[Gadgets.Count - 1]);
            }

            if (dataDictionaryControl != null)
            {
                canvasMain.Children.Remove(dataDictionaryControl);
                dataDictionaryControl = null;
            }

            if (propertiesControl != null)
            {
                canvasMain.Children.Remove(propertiesControl);
                propertiesControl = null;
            }

            Gadgets.Clear();
            EnableDisableOptions();
        }

        private void ResetCanvasProperties()
        {
            ShowCanvasSummaryInfoInOutput = true;
            ShowGadgetHeadingsInOutput = true;
            ShowGadgetSettingsInOutput = true;
            CustomOutputHeading = string.Empty;
            CustomOutputSummaryText = string.Empty;
            CustomOutputConclusionText = string.Empty;
            CustomOutputTableFontFamily = string.Empty;
            UseAlternatingColorsInOutput = false;
            SortGadgetsTopToBottom = true;
            TableFontSize = DEFAULT_TABLE_FONT_SIZE;
        }

        void DashboardControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CenterControlHorizontally(spTitle);
            CenterControlHorizontally(instructionsPanel);
            CenterControlHorizontally(loadingPanel);
            if (introPanel != null)
            {
                CenterControlHorizontally(introPanel);
            }
            CenterControlHorizontally(introAvailableData);

            if (introAvailableData.Visibility == System.Windows.Visibility.Visible && pathDataTriangle.Visibility == System.Windows.Visibility.Visible)
            {
                int column = Grid.GetColumn(iconOpen);
                double width = 0;
                for (int i = 0; i < column; i++)
                {
                    width = width + grdMenuBarInner.ColumnDefinitions[i].ActualWidth;
                }

                Canvas.SetLeft(introAvailableData, width);
                Canvas.SetTop(introAvailableData, 24);
            }
        }

        void mnuGadgetMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is GadgetMenuItem)
            {
                GadgetMenuItem gadgetMenuItem = sender as GadgetMenuItem;
                if (gadgetMenuItem != null)
                {
                    CreateGadget(gadgetMenuItem.GadgetType);
                }
            }
        }

        internal void CreateClone(Object control)
        {
            UserControl gadget = (UserControl)control;

            Type typeFactory = Type.GetType(gadget.GetType().ToString());

            if (typeFactory == null)
            {
                typeFactory = GetGadgetType(gadget.GetType().ToString());
            }

            UserControl newgadget = null;
            // If StatCalc, create without Dashboard Helper since data source is not needed for calculators
            if (typeFactory.ToString().Contains("Gadgets.StatCalc"))
            {
                newgadget = Activator.CreateInstance(typeFactory) as UserControl;
            }
            else
            {
                newgadget = Activator.CreateInstance(typeFactory, DashboardHelper) as UserControl;
            }

            ((GadgetBase)newgadget).CopyGadgetParameters(gadget);

            AddGadgetToCanvasFromContextMenu(newgadget);

            ((GadgetBase)newgadget).RefreshResults();
        }

        internal void CreateGadget(string gadgetType)
        {
            //string gadgetType = gadgetMenuItem.GadgetType;
            Type typeFactory = Type.GetType(gadgetType);

            if (typeFactory == null)
            {
                typeFactory = GetGadgetType(gadgetType);
            }

            try
            {
                UserControl gadget = null;
                // If StatCalc, create without Dashboard Helper since data source is not needed for calculators
                if (typeFactory.ToString().Contains("Gadgets.StatCalc"))
                {
                    gadget = Activator.CreateInstance(typeFactory) as UserControl;
                }
                else
                {
                    gadget = Activator.CreateInstance(typeFactory, DashboardHelper) as UserControl;
                }

                if (gadget != null)
                {
                    AddGadgetToCanvasFromContextMenu(gadget);
                    GadgetBase gBase = gadget as GadgetBase;
                    if (gBase != null)
                    {
                        gBase.ShowHideConfigPanel();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Cannot create instance of dbFactory" + ex.StackTrace);
            }
        }

        void mnuSaveOutput_Click(object sender, RoutedEventArgs e)
        {
            SaveOutputAsHTMLAndDisplay();
        }

        void mnuProperties_Click(object sender, RoutedEventArgs e)
        {
            if (DashboardHelper != null)
            {
                if (this.DashboardHelper.View != null
                    && !Util.DoesDataSourceExistForProject(this.DashboardHelper.View.Project))
                {
                    Epi.Windows.MsgBox.ShowError(String.Format(DashboardSharedStrings.ERROR_CANVAS_DATA_SOURCE_NOT_FOUND, this.DashboardHelper.View.Project.FullName));
                }
                else
                {
                    ShowCanvasProperties();
                }
            }
        }

        void mnuDataDictionary_Click(object sender, RoutedEventArgs e)
        {
            if (DashboardHelper != null)
            {
                if (dataDictionaryControl != null)
                {
                    canvasMain.Children.Remove(dataDictionaryControl);
                }
                dataDictionaryControl = new DataDictionaryControl(DashboardHelper);
                Canvas.SetLeft(dataDictionaryControl, mousePoint.X - 10);
                Canvas.SetTop(dataDictionaryControl, mousePoint.Y - 10);
                dataDictionaryControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
                DragCanvas.SetZIndex(dataDictionaryControl, 10000);
                canvasMain.Children.Add(dataDictionaryControl);

                DragCanvas.SetCanBeDragged(dataDictionaryControl, true);
            }
        }

        void gadget_GadgetClosing(UserControl gadget)
        {
            RemoveGadget(gadget);
            ArrangeGadgets();
            gadget = null;
            EnableDisableOptions();
        }

        void gadget_GadgetRefreshed(UserControl gadget)
        {
            IsCanvasDirty = true;

            if (CanvasChanged != null)
            {
                CanvasChanged(this.CurrentCanvas);
            }
        }

        void gadget_GadgetFinished(UserControl gadget)
        {
            if (CanvasChanged != null)
            {
                CanvasChanged(this.CurrentCanvas);
            }

            bool allFinished = true;

            foreach (UserControl control in Gadgets)
            {
                if (control is IGadget && ((IGadget)control).IsProcessing == true)
                {
                    allFinished = false;
                    break;
                }
            }

            // special case; must handle for times when gadgets call the data cache routine instead of the dashboard control
            if (worker == null || (!worker.IsBusy && allFinished))
            {
                mnuProperties.IsEnabled = true;
                if (canvasMain.AllowDragging == true)
                {
                    mnuAutoArrange.IsEnabled = true;
                }
                else
                {
                    mnuAutoArrange.IsEnabled = false;
                }
                mnuDataDictionary.IsEnabled = true;
                mnuExport.IsEnabled = true;
            }

            if (allFinished)
            {
                statusStripButtonShowBorders.IsSelected = LoadInEditMode;
                statusStripButtonShowBorders_Click();

                if (IsGeneratingHTMLFromCommandLine)
                {
                    try
                    {
                        if (System.IO.File.Exists(this.HTMLFilePath))
                        {
                            System.IO.File.Delete(this.HTMLFilePath);
                        }

                        System.IO.TextWriter textWriter = System.IO.File.CreateText(this.HTMLFilePath);

                        textWriter.Write(this.ToHTML(this.HTMLFilePath));

                        textWriter.Dispose();
                        textWriter.Close();

                        if (this.HTMLGenerated != null)
                        {
                            HTMLGenerated();
                        }
                    }
                    catch (Exception ex)
                    {
                        Epi.Windows.MsgBox.ShowException(ex);
                    }
                }

                BackgroundWorker arrangeWorker = new BackgroundWorker();
                arrangeWorker.DoWork += new DoWorkEventHandler(arrangeWorker_DoWork);
                arrangeWorker.RunWorkerAsync();
            }
        }

        void arrangeWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(75);
            this.Dispatcher.BeginInvoke(new SimpleCallback(ArrangeGadgets));
        }

        private void AutoArrangeGadgets()
        {
            if (canvasMain.AllowDragging == false) return; // if we turned off dragging, assume the user wants top-to-bottom auto-arrangement of canvas items. In this case we don't want to auto-arrange.

            var gadgetsByHeight = from g in Gadgets where ((UserControl)g).ActualHeight > 0 orderby ((UserControl)g).ActualHeight select g;

            List<IGadget> placedGadgets = new List<IGadget>();
            List<IGadget> nonPlacedGadgets = new List<IGadget>();

            foreach (IGadget gadget in gadgetsByHeight)
            {
                nonPlacedGadgets.Add(gadget);
            }

            int x = 0;
            int y = 0;
            int rowHeight = 0;

            while (nonPlacedGadgets.Count > 0)
            {
                int nextGadget = -1;

                for (int i = 0; i < nonPlacedGadgets.Count - 1; i++)
                {
                    if (x + ((UserControl)nonPlacedGadgets[i]).ActualWidth <= canvasMain.Width)
                    {
                        nextGadget = i;
                        break;
                    }
                }

                if (nextGadget < 0)
                {
                    y = y + rowHeight;
                    x = 0;
                    rowHeight = 0;
                    nextGadget = 0;
                }

                UserControl control = nonPlacedGadgets[nextGadget] as UserControl;

                if (control != null)
                {
                    IGadget placedGadget = nonPlacedGadgets[nextGadget];
                    Canvas.SetLeft(control, x);
                    Canvas.SetTop(control, y);
                    x = x + (int)control.ActualWidth;

                    if (rowHeight < control.ActualHeight)
                    {
                        rowHeight = (int)control.ActualHeight;
                    }

                    placedGadgets.Add(placedGadget);
                    nonPlacedGadgets.Remove(placedGadget);
                }
                else
                {
                    throw new InvalidOperationException("control cannot be null in AutoArrangeGadgets()");
                }
            }

            if (Gadgets.Count > 0)
            {
                IsCanvasDirty = true;
                if (CanvasChanged != null)
                {
                    CanvasChanged(this.CurrentCanvas);
                }
            }
        }

        /// <summary>
        /// Auto-arranges all gadgets on the canvas in top-to-bottom order
        /// </summary>
        private void ArrangeGadgets()
        {
            if (canvasMain.AllowDragging == false) // if we turned off dragging, assume the user wants top-to-bottom auto-arrangement of canvas items
            {
                var filtered = from g in Gadgets
                               where ((IGadget)g).AnchorLeft == null
                               orderby Canvas.GetTop(g)
                               select g;

                double runningTopPosition = 30; // starts this many units down from the top of the canvas

                foreach (IGadget gadget in filtered)
                {
                    gadget.AnchorTop = null;
                    Canvas.SetLeft((UserControl)gadget, 40); // ensure left position is consistent
                    Canvas.SetTop((UserControl)gadget, runningTopPosition);
                    runningTopPosition = runningTopPosition + ((UserControl)gadget).ActualHeight + 20;
                }
            }

            var unanchored = from g in Gadgets
                             where ((IGadget)g).AnchorLeft == null && ((IGadget)g).AnchorTop == null
                             orderby Canvas.GetTop(g), Canvas.GetLeft(g)
                             select g;

            foreach (IGadget gadget in unanchored)
            {
                MoveAnchoredGadgets(gadget);
            }
        }

        private void EnableDisableOptions()
        {
            tblockGadgetsDescription.Text = string.Empty;

            if (Gadgets.Count > 0)
            {
                mnuRefresh.IsEnabled = true;
                mnuReset.IsEnabled = true;
                if (!this.IsPopulatingDataSet)
                {
                    instructionsPanel.Visibility = System.Windows.Visibility.Hidden;
                }
                tooltipGadgets.IsEnabled = true;
                tooltipGadgets.Visibility = System.Windows.Visibility.Visible;

                Dictionary<string, int> gadgetCounts = new Dictionary<string, int>();

                for (int i = 0; i < Gadgets.Count; i++)
                {
                    UserControl control = Gadgets[i];

                    if (gadgetCounts.ContainsKey(control.ToString()))
                    {
                        gadgetCounts[control.ToString()]++;
                    }
                    else
                    {
                        gadgetCounts.Add(control.ToString(), 1);
                    }
                }

                int j = 0;
                foreach (KeyValuePair<string, int> kvp in gadgetCounts)
                {
                    tblockGadgetsDescription.Text = tblockGadgetsDescription.Text + kvp.Value + "x " + kvp.Key;
                    if (j < gadgetCounts.Count - 1)
                    {
                        tblockGadgetsDescription.Text = tblockGadgetsDescription.Text + Environment.NewLine;
                    }
                    j++;
                }
            }
            else
            {
                mnuRefresh.IsEnabled = false;
                mnuReset.IsEnabled = false;
                if (DashboardHelper != null)
                {
                    instructionsPanel.Visibility = Visibility.Visible;
                }
                tooltipGadgets.IsEnabled = false;
                tooltipGadgets.Visibility = Visibility.Collapsed;
            }

            string gadgetCounter = string.Format(DashboardSharedStrings.GADGET_COUNT, Gadgets.Count.ToString());
            tblockGadgetCount.Text = gadgetCounter;
        }

        public DashboardHelper DashboardHelper { get; private set; }
        private int ZTop { get; set; }

        public bool IsCanvasDirty { get; set; }
        public bool IsPopulatingDataSet { get; private set; }
        public bool IsGeneratingHTMLFromCommandLine { get; set; }
        public string HTMLFilePath { get; set; }
        public bool ShowGadgetHeadingsInOutput { get; set; }
        public bool ShowGadgetSettingsInOutput { get; set; }
        public bool ShowCanvasSummaryInfoInOutput { get; set; }
        public bool SortGadgetsTopToBottom { get; set; }
        public bool UseAlternatingColorsInOutput { get; set; }
        public double DefaultChartHeight { get; set; }
        public double DefaultChartWidth { get; set; }

        internal string LastSavedCanvasFileName
        {
            get
            {
                return this.lastSavedCanvasFileName;
            }
        }

        public string CustomOutputHeading { get; set; }
        public string CustomOutputSummaryText { get; set; }
        public string CustomOutputConclusionText { get; set; }
        public string CustomOutputTableFontFamily { get; set; }

        public int TableFontSize
        {
            get
            {
                return this.tableFontSize;
            }
            set
            {
                if (value < 6)
                {
                    this.tableFontSize = 6;
                }
                else if (value > 20)
                {
                    this.tableFontSize = 20;
                }
                else
                {
                    this.tableFontSize = value;
                }
            }
        }

        public bool LoadInEditMode { get; private set; }
        public string CurrentCanvas { get; private set; }
        public List<UserControl> Gadgets { get; private set; }

        /// <summary>
        /// Gets whether or not the dashboard control has any user-created content
        /// </summary>
        public bool HasGadgets
        {
            get
            {
                if (DashboardHelper == null)
                {
                    return false;
                }
                if (Gadgets == null)
                {
                    return false;
                }
                if (this.Gadgets.Count > 0)
                {
                    return true;
                }
                else if (this.DashboardHelper.Rules.Count > 0)
                {
                    return true;
                }
                else if (this.DashboardHelper.DataFilters.Count > 0)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Refreshes all gadgets on the dashboard
        /// </summary>
        private void RefreshResults()
        {
            foreach (UserControl gadget in Gadgets)
            {
                ((IGadget)gadget).RefreshResults();
            }
            if (dataDictionaryControl != null)
            {
                dataDictionaryControl.RefreshResults();
            }
            if (dataExportingControl != null)
            {
                canvasMain.Children.Remove(dataExportingControl);
                dataExportingControl = null;
            }
            UpdateRecordCount();
        }

        void mnuMxN_Click(object sender, RoutedEventArgs e)
        {
            CrosstabControl crsControl = new CrosstabControl(DashboardHelper);
            AddGadgetToCanvasFromContextMenu(crsControl);
        }

        void mnuPMCC_Click(object sender, RoutedEventArgs e)
        {
            PMCCControl pmccControl = new PMCCControl(DashboardHelper);
            AddGadgetToCanvasFromContextMenu(pmccControl);
        }

        void mnu2x2_Click(object sender, RoutedEventArgs e)
        {
            TwoByTwoTableControl crsControl = new TwoByTwoTableControl(DashboardHelper);
            canvasMain.Children.Add(crsControl);
            Canvas.SetLeft(crsControl, mousePoint.X - 10);
            Canvas.SetTop(crsControl, mousePoint.Y - 10);
            Gadgets.Add(crsControl);
            crsControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            crsControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            EnableDisableOptions();
        }

        void AddFilterGadget()
        {
            if (dataFilteringControl != null)
            {
                canvasMain.Children.Remove(dataFilteringControl);
            }
            dataFilteringControl = new DataFilterControl(DashboardHelper);
            dataFilteringControl.btnNewCondition.ContextMenu.MouseEnter += new MouseEventHandler(selectionCriteriaControl_MouseEnter);
            dataFilteringControl.btnNewCondition.ContextMenu.MouseLeave += new MouseEventHandler(selectionCriteriaControl_MouseLeave);
            dataFilteringControl.MouseEnter += new MouseEventHandler(selectionCriteriaControl_MouseEnter);
            dataFilteringControl.MouseLeave += new MouseEventHandler(selectionCriteriaControl_MouseLeave);
            dataFilteringControl.SelectionCriteriaChanged += new EventHandler(selectionCriteriaControl_SelectionCriteriaChanged);
            dataFilteringControl.Loaded += new RoutedEventHandler(dataFilteringControl_Loaded);
            DragCanvas.SetZIndex(dataFilteringControl, 10000);
            canvasMain.Children.Add(dataFilteringControl);

            DragCanvas.SetCanBeDragged(dataFilteringControl, false);
        }

        void dataFilteringControl_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas.SetRight(dataFilteringControl, -537);
            Canvas.SetTop(dataFilteringControl, scrollViewer.VerticalOffset + (scrollViewer.ActualHeight / 2.0) - (dataFilteringControl.ActualHeight / 2.0));
        }

        void dataExportControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Canvas.SetLeft(dataExportingControl, mousePoint.X - 10);
            //Canvas.SetTop(dataExportingControl, mousePoint.Y - 10);
            Canvas.SetRight(dataExportingControl, scrollViewer.HorizontalOffset + (scrollViewer.ActualWidth / 2.0) - (dataExportingControl.ActualWidth / 2.0));
            Canvas.SetTop(dataExportingControl, scrollViewer.VerticalOffset + (scrollViewer.ActualHeight / 2.0) - (dataExportingControl.ActualHeight / 2.0));
        }

        void propertiesControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Canvas.SetLeft(dataExportingControl, mousePoint.X - 10);
            //Canvas.SetTop(dataExportingControl, mousePoint.Y - 10);
            Canvas.SetRight(propertiesControl, scrollViewer.HorizontalOffset + (scrollViewer.ActualWidth / 2.0) - (propertiesControl.ActualWidth / 2.0));
            Canvas.SetTop(propertiesControl, scrollViewer.VerticalOffset + (scrollViewer.ActualHeight / 2.0) - (propertiesControl.ActualHeight / 2.0));
        }

        void selectionCriteriaControl_MouseLeave(object sender, MouseEventArgs e)
        {
            CollapseFilterGadget();
        }

        void selectionCriteriaControl_MouseEnter(object sender, MouseEventArgs e)
        {
            ExpandFilterGadget();
        }

        void ExpandFilterGadget()
        {
            DoubleAnimation anim = new DoubleAnimation();
            anim.From = Canvas.GetRight(dataFilteringControl);
            anim.To = -20;
            anim.AccelerationRatio = 0.8;
            anim.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            dataFilteringControl.BeginAnimation(Canvas.RightProperty, anim);
            dataFilteringControl.SetExpanded();
            this.isCreatingFilter = true;
        }

        void CollapseFilterGadget()
        {
            DoubleAnimation anim = new DoubleAnimation();
            anim.BeginTime = new TimeSpan(0, 0, 0, 0, 350);
            anim.From = Canvas.GetRight(dataFilteringControl);
            anim.To = -540;
            anim.DecelerationRatio = 0.8;
            anim.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            dataFilteringControl.BeginAnimation(Canvas.RightProperty, anim);
            dataFilteringControl.SetCollapsed();
            this.isCreatingFilter = false;
        }

        void AddDefinedVariablesGadget()
        {
            if (variablesControl != null)
            {
                canvasMain.Children.Remove(variablesControl);
            }
            variablesControl = new VariablesControl(DashboardHelper, this);
            variablesControl.btnNewRule.ContextMenu.MouseEnter += new MouseEventHandler(dataRecodingControl_MouseEnter);
            variablesControl.btnNewRule.ContextMenu.MouseLeave += new MouseEventHandler(dataRecodingControl_MouseLeave);
            variablesControl.MouseEnter += new MouseEventHandler(dataRecodingControl_MouseEnter);
            variablesControl.MouseLeave += new MouseEventHandler(dataRecodingControl_MouseLeave);
            variablesControl.Loaded += new RoutedEventHandler(dataRecodingControl_Loaded);
            variablesControl.UserVariableChanged += new EventHandler(dataRecodingControl_UserVariableChanged);
            variablesControl.UserVariableAdded += new EventHandler(dataRecodingControl_UserVariableAdded);
            variablesControl.UserVariableRemoved += new EventHandler(dataRecodingControl_UserVariableRemoved);
            DragCanvas.SetZIndex(variablesControl, 10000);
            canvasMain.Children.Add(variablesControl);

            DragCanvas.SetCanBeDragged(variablesControl, false);
        }

        void dataRecodingControl_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas.SetLeft(variablesControl, -425);
            Canvas.SetTop(variablesControl, scrollViewer.VerticalOffset + (scrollViewer.ActualHeight / 2.0) - (variablesControl.ActualHeight / 2.0));
        }

        void ExpandRecodingGadget()
        {
            DoubleAnimation anim = new DoubleAnimation();
            anim.From = Canvas.GetLeft(variablesControl);
            anim.To = -20;
            anim.AccelerationRatio = 0.8;
            anim.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            variablesControl.BeginAnimation(Canvas.LeftProperty, anim);
            this.isCreatingNewVariable = true;
        }

        void CollapseRecodingGadget()
        {
            DoubleAnimation anim = new DoubleAnimation();
            anim.BeginTime = new TimeSpan(0, 0, 0, 0, 250);
            anim.From = Canvas.GetLeft(variablesControl);
            anim.To = -425;
            anim.DecelerationRatio = 0.8;
            anim.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            variablesControl.BeginAnimation(Canvas.LeftProperty, anim);
            this.isCreatingNewVariable = false;
        }

        void dataRecodingControl_MouseLeave(object sender, MouseEventArgs e)
        {
            CollapseRecodingGadget();
        }

        void dataRecodingControl_MouseEnter(object sender, MouseEventArgs e)
        {
            ExpandRecodingGadget();
        }

        public bool AnyGadgetReliesOnVariable(string friendlyRule)
        {
            foreach (IGadget gadget in this.Gadgets)
            {
                if (GadgetReliesOnVariable(gadget, friendlyRule) == true)
                {
                    return true;
                }
            }
            return false;
        }

        private bool GadgetReliesOnVariable(IGadget gadget, string friendlyRule)
        {
            if (gadget != null)
            {
                GadgetBase gadgetBase = gadget as GadgetBase;

                if (gadgetBase != null)
                {
                    EpiDashboard.Rules.IDashboardRule rule = DashboardHelper.Rules.GetRule(friendlyRule);

                    if (rule is EpiDashboard.Rules.DataAssignmentRule)
                    {
                        EpiDashboard.Rules.DataAssignmentRule assignRule = rule as EpiDashboard.Rules.DataAssignmentRule;
                        if (assignRule != null)
                        {
                            string destinationField = assignRule.DestinationColumnName;

                            if (gadgetBase.DataFilters != null) //EI-362
                            {
                                foreach (FilterCondition fc in gadgetBase.DataFilters)
                                {
                                    if (fc.RawColumnName.Equals(destinationField))
                                    {
                                        return true;
                                    }
                                }
                            }

                        }
                    }
                }
            }

            return false;
        }

        void selectionCriteriaControl_SelectionCriteriaChanged(object sender, EventArgs e)
        {
            RefreshResults();
            UpdateRecordCount();

            recordCountWorker = new System.ComponentModel.BackgroundWorker();
            recordCountWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(recordCountWorker_DoWork);
            recordCountWorker.RunWorkerAsync();

            IsCanvasDirty = true;
            if (CanvasChanged != null)
            {
                CanvasChanged(this.CurrentCanvas);
            }

            if (DashboardHelper.UseAdvancedUserDataFilter && DashboardHelper.AdvancedUserDataFilter.Length > 0)
            {
                infoPanel.Visibility = Visibility.Visible;
                infoPanel.Text = string.Format(DashboardSharedStrings.CANVAS_FILTERS_IN_EFFECT, DashboardHelper.AdvancedUserDataFilter);
            }
            else if (!DashboardHelper.UseAdvancedUserDataFilter && DashboardHelper.DataFilters.Count > 0)
            {
                infoPanel.Visibility = Visibility.Visible;
                infoPanel.Text = string.Format(DashboardSharedStrings.CANVAS_FILTERS_IN_EFFECT, DashboardHelper.DataFilters.GenerateReadableDataFilterString());
            }
            else
            {
                infoPanel.Visibility = Visibility.Collapsed;
                infoPanel.Text = string.Empty;
            }
        }

        void dataRecodingControl_UserVariableAdded(object sender, EventArgs e)
        {
            UserVariableAddedRemoved();
        }

        void dataRecodingControl_UserVariableRemoved(object sender, EventArgs e)
        {
            UserVariableAddedRemoved();
        }

        private void UserVariableAddedRemoved()
        {
            DashboardHelper.UserVarsNeedUpdating = true;
            UpdateAllGadgetVariableNames();

            foreach (UIElement uc in this.canvasMain.Children)
            {
                if (uc is DataDictionaryControl)
                {
                    ((DataDictionaryControl)uc).UpdateVariableNames();
                }
                else if (uc is ExportControl)
                {
                    ((ExportControl)uc).UpdateVariableNames();
                }
            }

            dataFilteringControl.FillSelectionComboboxes(true);

            RefreshResults();

            UpdateFieldCounter();

            IsCanvasDirty = true;
            if (CanvasChanged != null)
            {
                CanvasChanged(this.CurrentCanvas);
            }
        }

        private void UpdateFieldCounter()
        {
            string fieldCounter = string.Format(DashboardSharedStrings.FIELD_AND_RECORD_COUNT, DashboardHelper.TableColumnNames.Count.ToString(), DashboardHelper.RecordCount);
            tblockFieldCount.Text = fieldCounter;
        }

        private void UpdateAllGadgetVariableNames()
        {
            foreach (UserControl uc in this.Gadgets)
            {
                if (uc is IGadget)
                {
                    ((IGadget)uc).UpdateVariableNames();
                }
            }
        }

        void dataRecodingControl_UserVariableChanged(object sender, EventArgs e)
        {
            DashboardHelper.UserVarsNeedUpdating = true;
            UpdateAllGadgetVariableNames();

            foreach (UIElement uc in this.canvasMain.Children)
            {
                if (uc is DataDictionaryControl)
                {
                    ((DataDictionaryControl)uc).UpdateVariableNames();
                }
                else if (uc is ExportControl)
                {
                    ((ExportControl)uc).UpdateVariableNames();
                }
            }

            RefreshResults();

            IsCanvasDirty = true;
            if (CanvasChanged != null)
            {
                CanvasChanged(this.CurrentCanvas);
            }
        }

        private void recordCountWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            DashboardHelper.GenerateRecordCount(true);
            this.Dispatcher.BeginInvoke(new SimpleCallback(UpdateRecordCount));
        }

        private void sliderZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_canvasTransform.ScaleX = sliderZoom.Value / 100;
            m_canvasTransform.ScaleY = sliderZoom.Value / 100;
            tblockZoom.Text = sliderZoom.Value.ToString() + " %";

            if (dataFilteringControl != null && variablesControl != null)
            {
                if (m_canvasTransform.ScaleX < 1)
                {
                    dataFilteringControl.Visibility = System.Windows.Visibility.Hidden;
                    variablesControl.Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    dataFilteringControl.Visibility = System.Windows.Visibility.Visible;
                    variablesControl.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }


        public bool SaveOutputAsHTML(bool showSaveConfirmation = true)
        {
            if (this.DashboardHelper == null)
            {
                Epi.Windows.MsgBox.ShowInformation(DashboardSharedStrings.CANNOT_SAVE_CANVAS_NO_DATA_SOURCE);
                return false;
            }

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".html";
            dlg.Filter = "HTML File|*.html";
            if (!string.IsNullOrEmpty(lastSavedOutputFileName))
            {
                dlg.FileName = lastSavedOutputFileName;
            }
            else
            {
                if (DashboardHelper.IsUsingEpiProject)
                {
                    dlg.FileName = DashboardHelper.View.Name + "Output.html";
                }
                else
                {
                    dlg.FileName = DashboardHelper.TableName + "Output.html";
                }

                if (Directory.Exists(DashboardHelper.Config.Directories.Output))
                {
                    dlg.InitialDirectory = DashboardHelper.Config.Directories.Output;
                }
            }

            if (dlg.ShowDialog().Value)
            {
                try
                {
                    if (System.IO.File.Exists(dlg.FileName))
                    {
                        System.IO.File.Delete(dlg.FileName);
                    }

                    System.IO.TextWriter textWriter = System.IO.File.CreateText(dlg.FileName);

                    textWriter.Write(this.ToHTML(dlg.FileName));

                    textWriter.Dispose();
                    textWriter.Close();

                    if (showSaveConfirmation)
                    {
                        Epi.Windows.MsgBox.ShowInformation(DashboardSharedStrings.OUTPUT_SAVED);
                    }
                    lastSavedOutputFileName = dlg.FileName;
                    return true;
                }
                catch (Exception ex)
                {
                    Epi.Windows.MsgBox.ShowException(ex);
                }
            }

            return false;
        }

        public void SaveOutputAsHTMLAndDisplay()
        {
            if (this.DashboardHelper == null)
            {
                Epi.Windows.MsgBox.ShowInformation(DashboardSharedStrings.CANNOT_SAVE_CANVAS_NO_DATA_SOURCE);
                return;
            }

            bool success = SaveOutputAsHTML(false);

            if (success)
            {
                if (!string.IsNullOrEmpty(lastSavedOutputFileName))
                {
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.FileName = lastSavedOutputFileName;
                    proc.StartInfo.UseShellExecute = true;
                    proc.Start();
                }
            }
        }

        public bool SaveCanvasAs()
        {
            if (this.DashboardHelper == null)
            {
                Epi.Windows.MsgBox.ShowInformation(DashboardSharedStrings.CANNOT_SAVE_CANVAS_NO_DATA_SOURCE);
                return false;
            }

            System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
            //Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".cvs7";
            dlg.Filter = "Epi Info 7 Dashboard Canvas File|*.cvs7";
            if (!string.IsNullOrEmpty(lastSavedCanvasFileName))
            {
                dlg.FileName = lastSavedCanvasFileName;
            }
            else if (DashboardHelper.IsUsingEpiProject && File.Exists(DashboardHelper.View.Project.FilePath))
            {
                FileInfo fileInfo = new FileInfo(DashboardHelper.View.Project.FilePath);
                dlg.InitialDirectory = fileInfo.DirectoryName;
            }

            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            //if (dlg.ShowDialog().Value)
            {
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                System.Xml.XmlElement root = doc.CreateElement("DashboardCanvas");

                AppendChildNodes(root, doc);

                try
                {
                    System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(dlg.FileName, Encoding.UTF8);
                    writer.Formatting = Formatting.Indented;

                    root.WriteTo(writer);
                    writer.Close();

                    string message = string.Format(DashboardSharedStrings.NOTIFICATION_CANVAS_SAVE, dlg.FileName);
                    NotificationMessage savedMessage = new NotificationMessage(message, false, NotificationButtonType.None, false, 7);
                    panelNotification.AddMessage(savedMessage);

                    lastSavedCanvasFileName = dlg.FileName;
                    this.CurrentCanvas = dlg.FileName;
                    IsCanvasDirty = false;

                    if (CanvasChanged != null)
                    {
                        CanvasChanged(lastSavedCanvasFileName);
                    }

                    return true;
                }
                catch (IOException)
                {
                    Epi.Windows.MsgBox.ShowInformation(DashboardSharedStrings.ERROR_CANVAS_SAVE_IO_EXCEPTION);
                }
            }

            return false;
        }

        public bool SaveCanvas()
        {
            if (this.DashboardHelper == null)
            {
                Epi.Windows.MsgBox.ShowInformation(DashboardSharedStrings.CANNOT_SAVE_CANVAS_NO_DATA_SOURCE);
                return false;
            }

            string fileToSave = string.Empty;

            if (!string.IsNullOrEmpty(this.lastSavedCanvasFileName))
            {
                fileToSave = lastSavedCanvasFileName;
            }
            else if (!string.IsNullOrEmpty(this.CurrentCanvas))
            {
                fileToSave = this.CurrentCanvas;
            }
            else
            {
                return this.SaveCanvasAs();
            }

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            System.Xml.XmlElement root = doc.CreateElement("DashboardCanvas");

            AppendChildNodes(root, doc);

            try
            {
                System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(fileToSave, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;

                root.WriteTo(writer);
                writer.Close();

                string message = string.Format(DashboardSharedStrings.NOTIFICATION_CANVAS_SAVE, fileToSave);
                NotificationMessage savedMessage = new NotificationMessage(message, false, NotificationButtonType.None, false, 7);
                panelNotification.AddMessage(savedMessage);

                lastSavedCanvasFileName = fileToSave;
                this.CurrentCanvas = fileToSave;
                IsCanvasDirty = false;

                if (CanvasChanged != null)
                {
                    CanvasChanged(lastSavedCanvasFileName);
                }

                return true;
            }
            catch (IOException)
            {
                Epi.Windows.MsgBox.ShowInformation(DashboardSharedStrings.ERROR_CANVAS_SAVE_IO_EXCEPTION);
            }

            return false;
        }

        private Type GetGadgetType(string gadgetType)
        {
            Type typeFactory = null;
            Configuration config = Configuration.GetNewInstance();
            foreach (Epi.DataSets.Config.GadgetRow row in config.Gadget.Rows)
            {
                string type = row.Type;

                if (type == gadgetType && row.FileName != null && !string.IsNullOrEmpty(row.FileName))
                {
                    string dllFileName = row.FileName;
                    Assembly assemblyInstance = Assembly.LoadFrom(dllFileName);
                    Type[] types = assemblyInstance.GetTypes();

                    foreach (Type t in types)
                    {
                        if (t.FullName == type)
                        {
                            typeFactory = t;
                        }
                    }
                }
            }
            return typeFactory;
        }

        private Type ConvertLegacyChart(XmlElement element)
        {
            if (element.Name.Equals("frequencyGadget"))
            {
                return Type.GetType("EpiDashboard.Gadgets.Charting.AberrationChartGadget");
            }
            else
            {
                foreach (XmlElement child in element.ChildNodes)
                {
                    if (child.Name.Equals("chartType"))
                    {
                        switch (child.InnerText)
                        {
                            case "Scatter":
                                return Type.GetType("EpiDashboard.Gadgets.Charting.ScatterChartGadget");
                            case "Stacked Column":
                            case "Column":
                            case "Bar":
                                return Type.GetType("EpiDashboard.Gadgets.Charting.ColumnChartGadget");
                            case "Pareto":
                                return Type.GetType("EpiDashboard.Gadgets.Charting.ParetoChartGadget");
                            case "Epi Curve":
                                return Type.GetType("EpiDashboard.Gadgets.Charting.HistogramChartGadget");
                            case "Pie":
                                return Type.GetType("EpiDashboard.Gadgets.Charting.PieChartGadget");
                            case "Line":
                                return Type.GetType("EpiDashboard.Gadgets.Charting.LineChartGadget");
                        }
                    }
                }
            }

            throw new ApplicationException();
        }

        private void AddGadgetsFromXml()
        {
            if (gadgetXmlElement != null)
            {
                foreach (XmlElement child in gadgetXmlElement.ChildNodes)
                {
                    try
                    {
                        Type gadgetType = Type.GetType(child.Attributes["gadgetType"].Value);

                        // The 2x2 gadget was removed and its functionality absorbed into the MxN (crosstab) gadget.
                        // This code, along with updates to the CreateFromXml routine in the Crosstab gadget, ensures
                        // that 2x2 gadgets created in canvas files from versions 7.0.9.61 and prior will still be
                        // loaded without issue. They will now be loaded as Crosstab gadgets instead.
                        if (child.Attributes["gadgetType"].Value.Equals("EpiDashboard.TwoByTwoTableControl"))
                        {
                            gadgetType = Type.GetType("EpiDashboard.CrosstabControl");
                        }

                        if (child.Attributes["gadgetType"].Value.Equals("EpiDashboard.ChartControl") || child.Attributes["gadgetType"].Value.Equals("EpiDashboard.AberrationControl"))
                        {
                            gadgetType = ConvertLegacyChart(child);
                        }

                        if (gadgetType == null)
                        {
                            gadgetType = GetGadgetType(child.Attributes["gadgetType"].Value);
                        }

                        IGadget gadget = (IGadget)Activator.CreateInstance(gadgetType, new object[] { DashboardHelper });
                        gadget.GadgetAnchorSetFromXml += new GadgetAnchorSetEventHandler(gadget_GadgetAnchorSetFromXml);
                        gadget.CreateFromXml(child);
                        AddGadgetToCanvasFromFile(gadget);
                    }
                    catch (NullReferenceException)
                    {
                        Epi.Windows.MsgBox.ShowError(DashboardSharedStrings.GADGET_LOAD_ERROR);
                        return;
                    }
                    catch (ArgumentNullException)
                    {
                        Epi.Windows.MsgBox.ShowError(DashboardSharedStrings.GADGET_LOAD_ERROR);
                        return;
                    }
                }
                gadgetXmlElement = null;
            }
        }

        private void CreateFromXml(string fileName)
        {
            gadgetXmlElement = null;
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.Load(fileName);

            DefaultChartWidth = DEFAULT_CHART_WIDTH;
            DefaultChartHeight = DEFAULT_CHART_HEIGHT;

            foreach (System.Xml.XmlElement element in doc.DocumentElement.ChildNodes)
            {
                if (element.Name.ToLower().Equals("dashboardhelper"))
                {
                    DashboardHelper helper = new DashboardHelper();
                    helper.NotificationEvent += new NotificationEventHandler(dashboardHelper_NotificationEvent);
                    helper.IsAutoClosing = this.IsGeneratingHTMLFromCommandLine;

                    foreach (System.Xml.XmlElement child in element.ChildNodes)
                    {
                        // Check to see if PRJ file exists and if not, prompt the user that this is the case; then offer to select a new PRJ file.
                        if (child.Name.Equals("projectPath") && !string.IsNullOrEmpty(child.InnerText))
                        {
                            FileInfo fiProject = new FileInfo(child.InnerText);
                            FileInfo fiCanvas = new FileInfo(fileName);
                            string projectPath = child.InnerText;

                            if (File.Exists(fiCanvas.Directory + "\\" + fiProject.Name))
                            {
                                projectPath = fiCanvas.Directory + "\\" + fiProject.Name;
                                child.InnerText = projectPath;
                            }
                            else if (!System.IO.File.Exists(child.InnerText))
                            {
                                string message = string.Format(DashboardSharedStrings.ERROR_PROJECT_NOT_FOUND_FOR_CANVAS, child.InnerText);
                                Epi.Windows.MsgBox.ShowError(message);

                                System.Windows.Forms.OpenFileDialog projectDialog = new System.Windows.Forms.OpenFileDialog();
                                projectDialog.Filter = "Epi Info 7 Project File|*.prj";

                                System.Windows.Forms.DialogResult projectDialogResult = projectDialog.ShowDialog();

                                if (projectDialogResult == System.Windows.Forms.DialogResult.OK)
                                {
                                    child.InnerText = projectDialog.FileName;
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                        else if (child.Name.Equals("connectionString") && !string.IsNullOrEmpty(child.InnerText))
                        {
                            string connStr;

                            try
                            {
                                connStr = Configuration.Decrypt(child.InnerText);
                            }
                            catch (System.Security.Cryptography.CryptographicException)
                            {
                                Epi.Windows.MsgBox.ShowError(DashboardSharedStrings.ERROR_CANNOT_DECRYPT);
                                return;
                            }

                            if (connStr.ToLower().StartsWith("provider=microsoft.ace") || connStr.ToLower().StartsWith("provider=microsoft.jet.oledb"))
                            {
                                string filePath = string.Empty;

                                int indexOf = connStr.IndexOf("Data Source=");

                                if (indexOf >= 0)
                                {
                                    indexOf += 12;
                                    filePath = connStr.Substring(indexOf, connStr.Length - indexOf);

                                    bool isTextFile = false;

                                    if (filePath.ToLower().Contains("text;"))
                                    {
                                        isTextFile = true;
                                    }

                                    if (filePath.ToLower().Contains("extended properties"))
                                    {
                                        indexOf = filePath.IndexOf(';');
                                        if (indexOf >= 0)
                                        {
                                            filePath = filePath.Substring(0, indexOf);
                                        }
                                    }
                                    filePath = filePath.TrimStart('\"');
                                    filePath = filePath.TrimEnd('\"');

                                    FileInfo fiDataSource = new FileInfo(filePath);
                                    FileInfo fiCanvas = new FileInfo(fileName);
                                    string dsPath = filePath;

                                    if (File.Exists(fiCanvas.Directory + "\\" + fiDataSource.Name))
                                    {
                                        dsPath = fiCanvas.Directory + "\\" + fiDataSource.Name;
                                        child.InnerText = Configuration.Encrypt(dsPath);
                                    }
                                    else if (isTextFile)
                                    {
                                    }
                                    else if (!System.IO.File.Exists(dsPath))
                                    {
                                        string message = string.Format(DashboardSharedStrings.ERROR_CANVAS_DATA_SOURCE_NOT_FOUND, dsPath);
                                        Epi.Windows.MsgBox.ShowError(message);
                                        return;
                                    }
                                }
                            }
                        }
                    }

                    try
                    {
                        helper.CreateFromXml(element);
                    }
                    catch (ViewNotFoundException ex)
                    {
                        Epi.Windows.MsgBox.ShowError(ex.Message);
                        return;
                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        Epi.Windows.MsgBox.ShowError(ex.Message);
                        return;
                    }
                    catch (System.Data.OleDb.OleDbException ex)
                    {
                        Epi.Windows.MsgBox.ShowError(ex.Message);
                        return;
                    }
                    catch (System.IO.FileNotFoundException ex)
                    {
                        Epi.Windows.MsgBox.ShowError(ex.Message);
                        return;
                    }
                    catch (GeneralException ex)
                    {
                        Epi.Windows.MsgBox.ShowError(ex.Message);
                        return;
                    }
                    catch (ApplicationException ex)
                    {
                        string message = ex.Message;
                        if (message.ToLower().Equals("error executing select query against the database."))
                        {
                            message = DashboardSharedStrings.ERROR_DATA_SOURCE_PERMISSIONS;
                        }
                        Epi.Windows.MsgBox.ShowError(message);
                        return;
                    }
                    catch (System.Security.Cryptography.CryptographicException ex)
                    {
                        Epi.Windows.MsgBox.ShowError(string.Format(SharedStrings.ERROR_CRYPTO_KEYS, ex.Message));
                        return;
                    }

                    // If its a non-Epi 7 data source, make sure the database exists
                    if (helper.Database != null)
                    {
                        try
                        {
                            bool success = helper.Database.TestConnection();
                        }
                        catch (Exception ex)
                        {
                            Epi.Windows.MsgBox.ShowError(ex.Message);
                            return;
                        }
                    }

                    this.DashboardHelper = helper;
                    this.DashboardHelper.SetDashboardControl(this);

                    canvasMain.Height = DEFAULT_CANVAS_HEIGHT;

                    RemoveAllGadgets();
                    EnableDisableMenus(true);
                    AddFilterGadget();
                    AddDefinedVariablesGadget();
                    ResetCanvasProperties();

                    this.CurrentCanvas = fileName;

                    //dashboardHelper.GenerateRecordCount(true);
                    try
                    {
                        UpdateRecordCount();
                        ReCacheDataSource();
                    }
                    catch (Exception ex)
                    {
                        Epi.Windows.MsgBox.ShowError(ex.Message);
                        return;
                    }
                }

                if (element.Name.ToLower().Equals("gadgets"))
                {
                    gadgetXmlElement = element;
                }
                if (element.Name.ToLower().Equals("outputsettings"))
                {
                    try
                    {
                        foreach (XmlElement child in element.ChildNodes)
                        {
                            switch (child.Name.ToLower())
                            {
                                case "showcanvassummaryinfo":
                                    ShowCanvasSummaryInfoInOutput = bool.Parse(child.InnerText);
                                    break;
                                case "showgadgetheadings":
                                    ShowGadgetHeadingsInOutput = bool.Parse(child.InnerText);
                                    break;
                                case "showgadgetsettings":
                                    ShowGadgetSettingsInOutput = bool.Parse(child.InnerText);
                                    break;
                                case "usealternatingcolors":
                                    UseAlternatingColorsInOutput = bool.Parse(child.InnerText);
                                    break;
                                case "defaultchartwidth":
                                    double defaultWidth = DEFAULT_CHART_WIDTH;
                                    double.TryParse(child.InnerText, out defaultWidth);
                                    DefaultChartWidth = defaultWidth;
                                    break;
                                case "defaultchartheight":
                                    double defaultHeight = DEFAULT_CHART_HEIGHT;
                                    double.TryParse(child.InnerText, out defaultHeight);
                                    DefaultChartHeight = defaultHeight;
                                    break;
                                case "sortgadgets":
                                    SortGadgetsTopToBottom = bool.Parse(child.InnerText);
                                    break;
                                case "canvasheight":
                                    int height;
                                    bool success = false;
                                    success = int.TryParse(child.InnerText, out height);
                                    if (success && height > 8000)
                                    {
                                        canvasMain.Height = height;
                                    }
                                    else
                                    {
                                        canvasMain.Height = DEFAULT_CANVAS_HEIGHT;
                                    }
                                    break;
                                case "customheading":
                                    CustomOutputHeading = child.InnerText;
                                    break;
                                case "customsummary":
                                    CustomOutputSummaryText = child.InnerText;
                                    break;
                                case "customconclusion":
                                    CustomOutputConclusionText = child.InnerText;
                                    break;
                                case "customtablefontfamily":
                                    CustomOutputTableFontFamily = child.InnerText;
                                    break;
                                case "tablefontsize":
                                    TableFontSize = int.Parse(child.InnerText);
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Epi.Windows.MsgBox.ShowException(ex);
                    }
                }
                if (element.Name.ToLower().Equals("canvassettings"))
                {
                    try
                    {
                        foreach (XmlElement child in element.ChildNodes)
                        {
                            switch (child.Name.ToLower())
                            {
                                case "editmode":
                                    LoadInEditMode = bool.Parse(child.InnerText);
                                    break;
                                case "layoutmode":
                                    if (child.InnerText.ToLower().Equals("free"))
                                    {
                                        canvasMain.AllowDragging = true;
                                        statusStripButtonVerticalArrange.IsSelected = false;
                                    }
                                    else
                                    {
                                        canvasMain.AllowDragging = false;
                                        statusStripButtonVerticalArrange.IsSelected = true;
                                    }
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Epi.Windows.MsgBox.ShowException(ex);
                    }
                }
            }

            string ldmTxt = string.Format(DashboardSharedStrings.CANVAS_LOADED, fileName);
            NotificationMessage loadedMessage = new NotificationMessage(ldmTxt, false, NotificationButtonType.None, false, 7);
            panelNotification.AddMessage(loadedMessage);

            lastSavedCanvasFileName = string.Empty;

            AddResizeAdorners();

            this.IsCanvasDirty = false;

            if (CanvasChanged != null)
            {
                CanvasChanged(CurrentCanvas);
            }

            if (DashboardHelper.DataFilters.Count > 0)
            {
                infoPanel.Visibility = Visibility.Visible;
                infoPanel.Text = string.Format(DashboardSharedStrings.CANVAS_FILTERS_IN_EFFECT, DashboardHelper.DataFilters.GenerateReadableDataFilterString());
            }
            else
            {
                infoPanel.Visibility = Visibility.Collapsed;
                infoPanel.Text = string.Empty;
            }

            this.scrollViewer.ScrollToTop();
        }

        private void dashboardHelper_NotificationEvent(object sender, string message)
        {
            this.Dispatcher.BeginInvoke(new SendNotificationMessageHandler(SendNotificationMessage), message, true, NotificationButtonType.OK, true, 8);
        }

        private void SendNotificationMessage(string message, bool showButtons, NotificationButtonType buttonType, bool requiresInteraction, int duration)
        {
            NotificationMessage msg = new NotificationMessage(message, showButtons, buttonType, requiresInteraction, duration);
            panelNotification.AddMessage(msg);
        }

        private void AddResizeAdorners()
        {
            foreach (UserControl control in this.Gadgets)
            {
                if (control is EpiDashboard.Gadgets.Reporting.StandardTextControl)
                {
                    BackgroundWorker adornerWorker = new BackgroundWorker();
                    adornerWorker.DoWork += new DoWorkEventHandler(adornerWorker_DoWork);
                    adornerWorker.RunWorkerAsync(control);
                }
            }
        }

        void adornerWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (e.Argument != null && e.Argument is StandardTextControl)
            {
                this.Dispatcher.BeginInvoke(new GadgetSetAdornerHandler(SetAdorner), e.Argument as Control);
            }
        }

        private void SetAdorner(Control control)
        {
            if (control != null && control is StandardTextControl)
            {
                System.Threading.Thread.Sleep(100);
                EpiDashboard.Gadgets.Reporting.StandardTextControl stc = (control as EpiDashboard.Gadgets.Reporting.StandardTextControl);
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(stc);
                adornerLayer.Add(new ResizingAdorner(stc));
            }
        }

        public void OpenCanvas()
        {
            if (!CanCloseDashboard())
            {
                return;
            }

            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Filter = "Epi Info 7 Dashboard Canvas File|*.cvs7";
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                CreateFromXml(dlg.FileName);
            }
        }

        public void OpenCanvas(string filePath)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            try
            {
                doc.Load(filePath);
            }
            catch (FileNotFoundException ex)
            {
                Epi.Windows.MsgBox.ShowException(ex);
                return;
            }

            CreateFromXml(filePath);
        }

        public XmlNode SerializeGadgets(XmlDocument doc)
        {
            System.Xml.XmlElement root = doc.CreateElement("Gadgets");

            var sortedGadgets = from g in Gadgets
                                orderby Canvas.GetTop(g), Canvas.GetLeft(g)
                                select g;

            foreach (IGadget gadget in sortedGadgets)
            {
                try
                {
                    root.AppendChild(gadget.Serialize(doc));
                }
                catch (NotImplementedException)
                {
                    // to avoid problems with the StatCalc gadgets
                }
            }
            return root;
        }

        public XmlNode SerializeCanvasSettings(XmlDocument doc)
        {
            System.Xml.XmlElement root = doc.CreateElement("CanvasSettings");

            bool editMode = statusStripButtonShowBorders.IsSelected;
            string layoutMode = "free";

            if (canvasMain.AllowDragging == false)
            {
                layoutMode = "autoArrangeVertical";
            }

            string xmlString =
           "<editMode>" + editMode + "</editMode>" +
           "<layoutMode>" + layoutMode + "</layoutMode>";

            root.InnerXml = xmlString;

            return root;
        }

        public XmlNode SerializeOutputSettings(XmlDocument doc)
        {
            System.Xml.XmlElement root = doc.CreateElement("OutputSettings");

            string xmlString =
           "<showCanvasSummaryInfo>" + ShowCanvasSummaryInfoInOutput + "</showCanvasSummaryInfo>" +
           "<showGadgetHeadings>" + ShowGadgetHeadingsInOutput + "</showGadgetHeadings>" +
           "<showGadgetSettings>" + ShowGadgetSettingsInOutput + "</showGadgetSettings>" +
           "<useAlternatingColors>" + UseAlternatingColorsInOutput + "</useAlternatingColors>" +
           "<defaultChartWidth>" + DefaultChartWidth + "</defaultChartWidth>" +
           "<defaultChartHeight>" + DefaultChartHeight + "</defaultChartHeight>" +
           "<tableFontSize>" + TableFontSize + "</tableFontSize>" +
           "<customHeading>" + CustomOutputHeading + "</customHeading>" +
           "<customSummary>" + CustomOutputSummaryText + "</customSummary>" +
           "<customConclusion>" + CustomOutputConclusionText + "</customConclusion>" +
           "<customTableFontFamily>" + CustomOutputTableFontFamily + "</customTableFontFamily>" +
           "<canvasHeight>" + canvasMain.Height.ToString("F0") + "</canvasHeight>" +
           "<sortGadgets>" + SortGadgetsTopToBottom + "</sortGadgets>";

            root.InnerXml = xmlString;

            return root;
        }

        public void GenerateStandardHTMLStyle(StringBuilder htmlBuilder)
        {
            htmlBuilder.AppendLine("  <style type=\"text/css\">");
            htmlBuilder.AppendLine("            ");
            htmlBuilder.AppendLine("body ");
            htmlBuilder.AppendLine("{");
            htmlBuilder.AppendLine("	background-color: white;");
            htmlBuilder.AppendLine("	font-family: Calibri, Arial, sans-serif;");
            htmlBuilder.AppendLine("	font-size: 11pt;");
            htmlBuilder.AppendLine("}");
            htmlBuilder.AppendLine("    ");
            htmlBuilder.AppendLine("td ");
            htmlBuilder.AppendLine("{");
            if (string.IsNullOrEmpty(CustomOutputTableFontFamily))
            {
                //htmlBuilder.AppendLine("	font-family: Consolas, 'Courier New', monospace;");
                htmlBuilder.AppendLine("	font-family: Calibri, Arial, sans-serif;");
            }
            else
            {
                htmlBuilder.AppendLine("	font-family: " + CustomOutputTableFontFamily + ", 'Courier New', monospace;");
            }
            htmlBuilder.AppendLine("	font-size: " + TableFontSize.ToString() + "px;");
            //htmlBuilder.AppendLine("	font-family: Arial, sans-serif;");
            htmlBuilder.AppendLine("	border-right: 1px solid rgb(23, 54, 93);");
            htmlBuilder.AppendLine("	border-bottom: 1px solid rgb(23, 54, 93);");
            htmlBuilder.AppendLine("	text-align: right;");
            htmlBuilder.AppendLine("	padding-left: 3px;");
            htmlBuilder.AppendLine("	padding-right: 3px;");
            htmlBuilder.AppendLine("	padding-top: 2px;");
            htmlBuilder.AppendLine("	padding-bottom: 2px;");
            htmlBuilder.AppendLine("}");
            htmlBuilder.AppendLine("    ");
            htmlBuilder.AppendLine("td.blank ");
            htmlBuilder.AppendLine("{");
            htmlBuilder.AppendLine("	border-right: 0px solid rgb(23, 54, 93);");
            htmlBuilder.AppendLine("	border-bottom: 0px solid rgb(23, 54, 93);");
            htmlBuilder.AppendLine("}");
            htmlBuilder.AppendLine("    ");
            htmlBuilder.AppendLine("td.noborder ");
            htmlBuilder.AppendLine("{");
            htmlBuilder.AppendLine("	border-right: 0px solid rgb(23, 54, 93);");
            htmlBuilder.AppendLine("	border-bottom: 0px solid rgb(23, 54, 93);");
            htmlBuilder.AppendLine("}");
            htmlBuilder.AppendLine("    ");
            htmlBuilder.AppendLine("td.twobyTwoColoredSquareCell ");
            htmlBuilder.AppendLine("{");
            htmlBuilder.AppendLine("	border-right: 0px solid rgb(23, 54, 93);");
            htmlBuilder.AppendLine("	border-bottom: 0px solid rgb(23, 54, 93);");
            htmlBuilder.AppendLine("	padding-left: 0px;");
            htmlBuilder.AppendLine("	padding-right: 0px;");
            htmlBuilder.AppendLine("	padding-top: 0px;");
            htmlBuilder.AppendLine("	padding-bottom: 0px;");
            htmlBuilder.AppendLine("}");
            htmlBuilder.AppendLine("    ");
            htmlBuilder.AppendLine("td.value ");
            htmlBuilder.AppendLine("{");
            htmlBuilder.AppendLine("	text-align: left;");
            htmlBuilder.AppendLine("}");
            htmlBuilder.AppendLine("    ");

            htmlBuilder.AppendLine("td.highlightedRow ");
            htmlBuilder.AppendLine("{");
            htmlBuilder.AppendLine("	font-weight: bold;");
            htmlBuilder.AppendLine("	background-color: rgb(217, 150, 148);");
            htmlBuilder.AppendLine("}");
            htmlBuilder.AppendLine("    ");

            htmlBuilder.AppendLine("tr.altcolor"); // if alt color is other than white, rows will alternate between white and this color; off by default
            htmlBuilder.AppendLine("{");
            if (UseAlternatingColorsInOutput)
            {
                htmlBuilder.AppendLine("	background-color: #EEEEEE;");
            }
            else
            {
                htmlBuilder.AppendLine("	background-color: rgb(255, 255, 255);");
            }
            htmlBuilder.AppendLine("}");
            htmlBuilder.AppendLine("    ");
            htmlBuilder.AppendLine("th");
            htmlBuilder.AppendLine("{");
            htmlBuilder.AppendLine("	font-family: Calibri, sans-serif;");
            htmlBuilder.AppendLine("	background-color: #4a7ac9;");
            htmlBuilder.AppendLine("	font-weight: bold;");
            htmlBuilder.AppendLine("	color: white;");
            htmlBuilder.AppendLine("	padding-left: 5px;");
            htmlBuilder.AppendLine("	padding-right: 5px;");
            htmlBuilder.AppendLine("	padding-top: 3px;");
            htmlBuilder.AppendLine("	padding-bottom: 3px;");
            htmlBuilder.AppendLine("	border-right: 1px solid rgb(23, 54, 93);");
            htmlBuilder.AppendLine("	border-bottom: 1px solid rgb(23, 54, 93);");
            htmlBuilder.AppendLine("	min-width: 50px;");
            htmlBuilder.AppendLine("}");
            htmlBuilder.AppendLine("    ");
            htmlBuilder.AppendLine("h1");
            htmlBuilder.AppendLine("{");
            htmlBuilder.AppendLine("	color: #4a7ac9;");
            htmlBuilder.AppendLine("	font-family: Cambria, 'Times New Roman', serif;");
            htmlBuilder.AppendLine("}");
            htmlBuilder.AppendLine("    ");
            htmlBuilder.AppendLine("h2");
            htmlBuilder.AppendLine("{");
            htmlBuilder.AppendLine("	font-family: Cambria, 'Times New Roman', serif;");
            htmlBuilder.AppendLine("}");
            htmlBuilder.AppendLine("h2.gadgetHeading");
            htmlBuilder.AppendLine("{");
            if (!ShowGadgetHeadingsInOutput)
            {
                htmlBuilder.AppendLine("	visibility: hidden;");
                htmlBuilder.AppendLine("	height: 1px;");
            }
            htmlBuilder.AppendLine("}");

            htmlBuilder.AppendLine("p.gadgetOptions");
            htmlBuilder.AppendLine("{");
            if (!ShowGadgetSettingsInOutput)
            {
                htmlBuilder.AppendLine("	visibility: hidden;");
                htmlBuilder.AppendLine("	height: 1px;");
            }
            htmlBuilder.AppendLine("}");
            htmlBuilder.AppendLine("p.summary");
            htmlBuilder.AppendLine("{");
            htmlBuilder.AppendLine("	margin-left: 20px;");
            htmlBuilder.AppendLine("	margin-right: 20px;");
            htmlBuilder.AppendLine("}");
            htmlBuilder.AppendLine("    ");
            htmlBuilder.AppendLine("p.gadgetSummary");
            htmlBuilder.AppendLine("{");
            htmlBuilder.AppendLine("	font-size: 11pt;");
            htmlBuilder.AppendLine("	font-family: Calibri, Arial, Verdana, sans-serif;");
            htmlBuilder.AppendLine("}");
            htmlBuilder.AppendLine("    ");
            htmlBuilder.AppendLine("table ");
            htmlBuilder.AppendLine("{");
            htmlBuilder.AppendLine("	border-left: 1px solid rgb(23, 54, 93);");
            htmlBuilder.AppendLine("	border-top: 1px solid rgb(23, 54, 93);");
            htmlBuilder.AppendLine("}");
            htmlBuilder.AppendLine("    ");
            htmlBuilder.AppendLine("table.noborder ");
            htmlBuilder.AppendLine("{");
            htmlBuilder.AppendLine("	border-left: 0px solid rgb(23, 54, 93);");
            htmlBuilder.AppendLine("	border-top: 0px solid rgb(23, 54, 93);");
            htmlBuilder.AppendLine("}");
            htmlBuilder.AppendLine("    ");
            htmlBuilder.AppendLine("table.twoByTwoColoredSquares ");
            htmlBuilder.AppendLine("{");
            htmlBuilder.AppendLine("	border-left: 0px solid rgb(23, 54, 93);");
            htmlBuilder.AppendLine("	border-top: 0px solid rgb(23, 54, 93);");
            htmlBuilder.AppendLine("	width: 200px;");
            htmlBuilder.AppendLine("	height: 200px;");
            htmlBuilder.AppendLine("}");
            htmlBuilder.AppendLine("    ");
            htmlBuilder.AppendLine("div.percentBar");
            htmlBuilder.AppendLine("{");
            htmlBuilder.AppendLine("	height: 8px;");
            htmlBuilder.AppendLine("	background-color: rgb(34, 177, 76);");
            htmlBuilder.AppendLine("}");
            htmlBuilder.AppendLine("    ");
            htmlBuilder.AppendLine(".total");
            htmlBuilder.AppendLine("{");
            htmlBuilder.AppendLine("	font-weight: bold;");
            htmlBuilder.AppendLine("}");
            htmlBuilder.AppendLine("    ");
            htmlBuilder.AppendLine(".warning");
            htmlBuilder.AppendLine("{");
            htmlBuilder.AppendLine("	font-weight: bold;");
            htmlBuilder.AppendLine("	color: red;");
            htmlBuilder.AppendLine("}");
            htmlBuilder.AppendLine("    ");
            htmlBuilder.AppendLine(".bold");
            htmlBuilder.AppendLine("{");
            htmlBuilder.AppendLine("	font-weight: bold;");
            htmlBuilder.AppendLine("}");
            htmlBuilder.AppendLine("    ");
            htmlBuilder.AppendLine("caption");
            htmlBuilder.AppendLine("{");
            htmlBuilder.AppendLine("	font-weight: bold;");
            htmlBuilder.AppendLine("}");
            htmlBuilder.AppendLine("    ");
            htmlBuilder.AppendLine("  </style>");
        }

        public string ToHTML(string htmlFileName = "")
        {
            StringBuilder htmlBuilder = new StringBuilder();

            htmlBuilder.AppendLine("<!DOCTYPE html>");
            htmlBuilder.AppendLine("<html>");
            htmlBuilder.AppendLine(" <head>");
            htmlBuilder.AppendLine("  <meta name=\"description\" content=\"Dashboard Output\" />");
            htmlBuilder.AppendLine("  <meta name=\"keywords\" content=\"Epi, Info, Dashboard\" />");
            htmlBuilder.AppendLine("  <meta name=\"author\" content=\"Epi Info 7\" />");
            htmlBuilder.AppendLine("  <meta http-equiv=\"content-type\" content=\"text/html;charset=UTF-8\" />");
            GenerateStandardHTMLStyle(htmlBuilder);
            htmlBuilder.AppendLine("    ");

            htmlBuilder.AppendLine("  <title>" + SharedStrings.DASHBOARD_HTML_TITLE + "</title>");
            htmlBuilder.AppendLine(" </head>");
            htmlBuilder.AppendLine("<body>");
            if (string.IsNullOrEmpty(CustomOutputHeading))
            {
                htmlBuilder.AppendLine("<h1>" + SharedStrings.DASHBOARD_HTML_HEADING + "</h1>");
            }
            else
            {
                htmlBuilder.AppendLine("<h1>" + CustomOutputHeading + "</h1>");
            }
            htmlBuilder.AppendLine("<hr>");
            htmlBuilder.AppendLine(" ");

            if (!string.IsNullOrEmpty(CustomOutputSummaryText))
            {
                htmlBuilder.AppendLine("<p class=\"summary\">");
                htmlBuilder.AppendLine(CustomOutputSummaryText);
                htmlBuilder.AppendLine("</p>");
                htmlBuilder.AppendLine(" ");
                htmlBuilder.AppendLine("<hr>");
                htmlBuilder.AppendLine(" ");
                htmlBuilder.AppendLine("<p>&nbsp;</p>");
                htmlBuilder.AppendLine(" ");
            }

            if (ShowCanvasSummaryInfoInOutput)
            {
                htmlBuilder.AppendLine(this.DashboardHelper.ToHTML());
            }

            //gadgets
            if (SortGadgetsTopToBottom)
            {
                SortedDictionary<double, IGadget> sortedGadgets = new SortedDictionary<double, IGadget>();

                foreach (UserControl control in this.Gadgets)
                {
                    if (control is IGadget && ((IGadget)control).IsProcessing == false)
                    {
                        double top = Canvas.GetTop(control);
                        if (!sortedGadgets.ContainsKey(top))
                        {
                            sortedGadgets.Add(top, control as IGadget);
                        }
                        else
                        {
                            double d = 0;
                            while (sortedGadgets.ContainsKey(top - d))
                            {
                                d = d + 0.0001;
                            }
                            sortedGadgets.Add(top - d, control as IGadget);
                        }
                    }
                }

                int count = 0;
                foreach (KeyValuePair<double, IGadget> kvp in sortedGadgets)
                {
                    IGadget gadget = kvp.Value;
                    if (gadget.IsProcessing == false)
                    {
                        htmlBuilder.Append(gadget.ToHTML(htmlFileName, count));
                        htmlBuilder.AppendLine("");
                        htmlBuilder.AppendLine("<p>&nbsp;</p>");
                        count++;
                    }
                }
            }
            else
            {
                int count = 0;
                foreach (UserControl control in this.Gadgets)
                {
                    if (control is IGadget && ((IGadget)control).IsProcessing == false)
                    {
                        IGadget gadget = control as IGadget;
                        if (gadget != null)
                        {
                            htmlBuilder.Append(gadget.ToHTML(htmlFileName, count));
                            htmlBuilder.AppendLine("");
                            htmlBuilder.AppendLine("<p>&nbsp;</p>");
                            count++;
                        }
                    }
                }
            }

            htmlBuilder.AppendLine(" ");

            if (!string.IsNullOrEmpty(CustomOutputConclusionText))
            {
                htmlBuilder.AppendLine("<hr>");
                htmlBuilder.AppendLine(" ");
                htmlBuilder.AppendLine("<p class=\"summary\">");
                htmlBuilder.AppendLine(CustomOutputConclusionText);
                htmlBuilder.AppendLine("</p>");
                htmlBuilder.AppendLine(" ");
                htmlBuilder.AppendLine("<hr>");
                htmlBuilder.AppendLine(" ");
            }

            htmlBuilder.AppendLine("</body>");
            htmlBuilder.AppendLine("</html>");

            return htmlBuilder.ToString();
        }

        private void buttonZoomOut_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            sliderZoom.Value = sliderZoom.Value - 5;
        }

        private void buttonZoomIn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            sliderZoom.Value = sliderZoom.Value + 5;
        }

        private void ShowCanvasProperties()
        {
            popup = new DashboardPopup();
            popup.Parent = grdMain;
            DashboardProperties properties = new DashboardProperties(this.DashboardHelper);

            properties.Conclusion = this.CustomOutputConclusionText;
            properties.Title = this.CustomOutputHeading;
            properties.Summary = this.CustomOutputSummaryText;
            properties.UseAlternatingColors = this.UseAlternatingColorsInOutput;
            properties.ShowCanvasSummary = this.ShowCanvasSummaryInfoInOutput;
            properties.ShowGadgetHeadings = this.ShowGadgetHeadingsInOutput;
            properties.ShowGadgetSettings = this.ShowGadgetSettingsInOutput;
            properties.UseTopToBottomGadgetOrdering = this.SortGadgetsTopToBottom;
            properties.DefaultChartHeight = this.DefaultChartHeight;
            properties.DefaultChartWidth = this.DefaultChartWidth;

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
            popup.Content = properties;
            popup.Show();
        }

        private void iconFile_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (scrollViewer.ContextMenu != null)
            {
                scrollViewer.ContextMenu.PlacementTarget = iconFile;
                scrollViewer.ContextMenu.IsOpen = true;
            }

            e.Handled = true;
            return;
        }

        void properties_ChangesAccepted(object sender, EventArgs e)
        {
            DashboardProperties properties = popup.Content as DashboardProperties;

            if (properties == null)
            {
                throw new InvalidOperationException("properties cannot be null in properties_ChangesAccepted");
            }

            if (DashboardHelper.IsUsingEpiProject)
            {
                bool projectPathIsValid = true;
                Project tempProject = new Project();
                try
                {
                    tempProject = new Project(properties.ProjectFileInfo.FullName);
                }
                catch
                {
                    projectPathIsValid = false;
                }

                if (!projectPathIsValid)
                {
                    //pnlError.Visibility = System.Windows.Visibility.Visible;
                    //txtError.Text = "Error: The specified project path is not valid. Settings were not saved.";
                    return;
                }

                if (!tempProject.Views.Contains(properties.FormName))
                {
                    //pnlError.Visibility = System.Windows.Visibility.Visible;
                    //txtError.Text = "Error: Form not found in project. Settings were not saved.";
                    return;
                }

                if (DashboardHelper.IsUsingEpiProject)
                {
                    //if (cmbTableName.Text.ToLower() == dashboardHelper.View.Name.ToLower() && dashboardHelper.View.Project.FilePath == txtProjectPath.Text)
                    //{
                    //    pnlWarning.Visibility = System.Windows.Visibility.Collapsed;
                    //    txtWarning.Text = string.Empty;
                    //}
                }

                DashboardHelper.ResetView(tempProject.Views[properties.FormName], false);
            }
            else if (DashboardHelper.Database.ToString().Contains("SqlDatabase"))
            {
                Epi.Data.IDbDriver dbDriver = Epi.Data.DBReadExecute.GetDataDriver(properties.txtSQLConnectionString.Text);
                DashboardHelper.SetDataSource(dbDriver, properties.TableName, properties.CustomQuery);
            }
            else
            {
                Epi.Data.IDbDriver dbDriver = Epi.Data.DBReadExecute.GetDataDriver(properties.txtStandalonePath.Text);
                DashboardHelper.SetDataSource(dbDriver, properties.TableName, properties.CustomQuery);
            }

            this.CustomOutputConclusionText = properties.Conclusion;
            this.CustomOutputHeading = properties.Title;
            this.CustomOutputSummaryText = properties.Summary;
            this.UseAlternatingColorsInOutput = properties.UseAlternatingColors;
            this.ShowCanvasSummaryInfoInOutput = properties.ShowCanvasSummary;
            this.ShowGadgetHeadingsInOutput = properties.ShowGadgetHeadings;
            this.ShowGadgetSettingsInOutput = properties.ShowGadgetSettings;
            this.SortGadgetsTopToBottom = properties.UseTopToBottomGadgetOrdering;

            if (properties.DefaultChartWidth.HasValue)
            {
                DefaultChartWidth = properties.DefaultChartWidth.Value;
            }

            if (properties.DefaultChartHeight.HasValue)
            {
                DefaultChartHeight = properties.DefaultChartHeight.Value;
            }

            popup.Close();
        }

        void properties_Cancelled(object sender, EventArgs e)
        {
            popup.Close();
        }

        private void iconRefresh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (DashboardHelper != null)
            {
                if (DashboardHelper.View != null && !Util.DoesDataSourceExistForProject(this.DashboardHelper.View.Project))
                {
                    Epi.Windows.MsgBox.ShowError(String.Format(DashboardSharedStrings.ERROR_CANVAS_DATA_SOURCE_NOT_FOUND, this.DashboardHelper.View.Project.FullName));
                }
                else
                {
                    DashboardHelper.UserVarsNeedUpdating = true;
                    ReCacheDataSource(false);
                }
            }
        }

        private void iconDb_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsPopulatingDataSet)
            {
                SetDataSource();
            }
        }

        private void iconOpen_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            svAvailableCanvasFiles.ScrollToTop();

            int column = Grid.GetColumn(iconOpen);
            double width = 0;
            for (int i = 0; i < column; i++)
            {
                width = width + grdMenuBarInner.ColumnDefinitions[i].ActualWidth;
            }

            if (pathDataTriangle.Visibility == System.Windows.Visibility.Visible &&
                introAvailableData.Visibility == System.Windows.Visibility.Visible)
            {
                introAvailableData.Visibility = Visibility.Collapsed;
            }
            else
            {
                LoadQuickDataSources();

                Canvas.SetLeft(introAvailableData, width);
                Canvas.SetTop(introAvailableData, 24 + scrollViewer.VerticalOffset);
                DragCanvas.SetCanBeDragged(introAvailableData, false);
                pathDataTriangle.Visibility = System.Windows.Visibility.Visible;
                introAvailableData.Visibility = System.Windows.Visibility.Visible;
                Canvas.SetZIndex(introAvailableData, 4000);

                if (this.ActualHeight <= 600)
                {
                    introAvailableData.MaxHeight = 550;
                }
                else if (this.ActualHeight <= 770)
                {
                    introAvailableData.MaxHeight = 700;
                }
            }
        }

        private void iconSave_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DashboardHelper != null)
            {
                SaveCanvas();
            }
        }

        private void iconSaveAs_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DashboardHelper != null)
            {
                SaveCanvasAs();
            }
        }

        private void iconCreateNewVariable_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DashboardHelper != null)
            {
                if (isCreatingNewVariable)
                    CollapseRecodingGadget();
                else
                    ExpandRecodingGadget();
            }
        }

        private void iconFilter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DashboardHelper != null)
            {
                if (isCreatingFilter)
                    CollapseFilterGadget();
                else
                    ExpandFilterGadget();
            }
        }

        //private void tblockSaveAs_MouseEnter(object sender, MouseEventArgs e)
        //{
        //    whiteUnderlineSaveAs.Visibility = System.Windows.Visibility.Visible;
        //    this.Cursor = Cursors.Hand;
        //}

        //private void tblockSaveAs_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    whiteUnderlineSaveAs.Visibility = System.Windows.Visibility.Collapsed;
        //    this.Cursor = Cursors.Arrow;
        //}

        //private void tblockSaveAs_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{            
        //}

        private void tblockSaveAs_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DashboardHelper != null)
            {
                SaveCanvasAs();
            }
        }

        public bool CanCloseDashboard()
        {
            if (DashboardHelper != null && this.IsCanvasDirty)
            {
                System.Windows.Forms.DialogResult result = System.Windows.Forms.DialogResult.No;

                if (string.IsNullOrEmpty(lastSavedCanvasFileName))
                {
                    result = Epi.Windows.MsgBox.ShowQuestion(DashboardSharedStrings.SAVE_CHANGES, System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                }
                else
                {
                    string message = string.Format(DashboardSharedStrings.SAVE_CHANGES_TO_FILE, lastSavedCanvasFileName);
                    result = Epi.Windows.MsgBox.ShowQuestion(message, System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                }

                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    SaveCanvas();
                }
                else if (result == System.Windows.Forms.DialogResult.Cancel)
                {
                    return false;
                }
            }

            return true;
        }

        private void introArrowSetDataSource_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void introArrowSetDataSource_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        private void introArrowSetDataSource_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetDataSource();
        }

        private void introArrowFindMore_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void introArrowFindMore_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        private void introArrowFindMore_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenCanvas();
        }

        private void panelAddSpace_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void panelAddSpace_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        private void panelAddSpace_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.canvasMain.Height = this.canvasMain.Height + 2000;
        }

        private void cmbView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataDisplay != null && dataDictionary != null)
            {
                if (cmbView.SelectedIndex == 0)
                {
                    dataDisplay.Visibility = System.Windows.Visibility.Collapsed;
                    dataDictionary.Visibility = System.Windows.Visibility.Collapsed;
                    duplicatesDisplay.Visibility = Visibility.Collapsed;
                }
                else if (cmbView.SelectedIndex == 1)
                {
                    dataDisplay.SetDataView(DashboardHelper.GenerateView());
                    dataDisplay.Refresh();
                    dataDisplay.Visibility = System.Windows.Visibility.Visible;
                    dataDictionary.Visibility = System.Windows.Visibility.Collapsed;
                    duplicatesDisplay.Visibility = Visibility.Collapsed;
                }
                else if (cmbView.SelectedIndex == 2)
                {
                    dataDictionary.AttachDashboardHelper(this.DashboardHelper);
                    dataDictionary.SetDataView(this.DashboardHelper.GenerateDataDictionaryTable().DefaultView);
                    dataDictionary.Refresh();
                    dataDictionary.Visibility = System.Windows.Visibility.Visible;
                    dataDisplay.Visibility = System.Windows.Visibility.Collapsed;
                    duplicatesDisplay.Visibility = Visibility.Collapsed;
                }
                //else if (cmbView.SelectedIndex == 3)
                //{
                //    string[] columnNames = new string[1];
                //    columnNames[0] = "State";

                //    duplicatesDisplay.SetDataView(DashboardHelper.GenerateDuplicatesTable(DashboardHelper.DataSet.Tables[0].DefaultView, columnNames).DefaultView);
                //    duplicatesDisplay.Refresh();

                //    dataDictionary.Visibility = System.Windows.Visibility.Collapsed;
                //    dataDisplay.Visibility = System.Windows.Visibility.Collapsed;
                //    duplicatesDisplay.Visibility = Visibility.Visible;
                //}
            }
        }
    }
}