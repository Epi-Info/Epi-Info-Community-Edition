using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using Epi.WPF;
using Epi.WPF.Dashboard;
//using Epi.WPF.Dashboard.NutStat;
using Epi.WPF.Dashboard.Controls;
using Epi.WPF.Dashboard.Dialogs;
using Epi.WPF.Dashboard.Gadgets;
using Epi.WPF.Dashboard.Gadgets.Analysis;
//using Epi.WPF.Dashboard.Gadgets.Reporting;

namespace Epi.WPF.Dashboard
{
    /// <summary>
    /// Interaction logic for DashboardControl.xaml
    /// </summary>
    public partial class DashboardControl : UserControl
    {
        #region Private Members
        List<UserControl> gadgets;
        private BackgroundWorker worker;
        private DashboardHelper dashboardHelper;
        private DataFilterControl dataFilteringControl;
        //private DataRecodingControl dataRecodingControl;
        private VariablesControl variablesControl;
        //private ExportControl dataExportingControl;
        //private PropertiesControl propertiesControl;

        private delegate void SimpleCallback();
        private delegate void SetStatusDelegate(string statusMessage);
        private delegate void RequestUpdateStatusDelegate(string statusMessage);
        private delegate bool CheckForCancellationDelegate();

        //public event DashboardHelperRequestedHandler DashboardHelperRequested;
        //public event RelatedDataRequestedHandler RelatedDataRequested;
        public event HTMLGeneratedHandler HTMLGenerated;
        public event RecordCountChangedHandler RecordCountChanged;
        public event CanvasChangedHandler CanvasChanged;
        private BackgroundWorker recordCountWorker;
        private bool isCanvasDirty;
        private bool isGeneratingHTMLFromCommandLine;
        private bool isPopulatingDataSet;
        private string htmlFilePath;
        private Point mousePoint;
        private bool showCanvasSummaryInfoInOutput;
        private bool showGadgetHeadingsInOutput;
        private bool showGadgetSettingsInOutput;
        private bool useAlternatingColorsInOutput;
        private string customOutputHeading;
        private string customOutputSummaryText;
        private string customOutputConclusionText;
        private string customOutputTableFontFamily;
        private int tableFontSize;
        private string lastSavedOutputFileName;
        private string lastSavedCanvasFileName;
        private bool sortGadgetsTopToBottom;
        private string currentCanvas = string.Empty;
        private bool isWordInstalled = false;
        private bool isExcelInstalled = false;
        private const int DEFAULT_TABLE_FONT_SIZE = 13;
        private const int DEFAULT_CANVAS_HEIGHT = 8000;

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
            this.dashboardHelper = dashboardHelper;
            Construct();
            dashboardHelper.GenerateRecordCount(false);
            //txtRecordCount.Text = dashboardHelper.RecordCount.ToString();
            UpdateRecordCount();
            mnuDataSource.Visibility = System.Windows.Visibility.Collapsed;

            //if (DashboardHelperRequested == null)
            //{
            //    grdMenuBarInner.ColumnDefinitions[3].Width = new GridLength(0);
            //    grdMenuBarInner.ColumnDefinitions[3].MinWidth = 0;
            //}

            AddFilterGadget();
            AddDefinedVariablesGadget();
        }
        #endregion // Constructors

        private void Construct()
        {
            gadgets = new List<UserControl>();
            this.isPopulatingDataSet = false;
            this.SizeChanged += new SizeChangedEventHandler(DashboardControl_SizeChanged);
            scrollViewerAnalysis.ScrollChanged += new ScrollChangedEventHandler(scrollViewerAnalysis_ScrollChanged);
            statusStripButtonShowBorders.IsSelected = true;
            statusStripButtonShowBorders.Click += new StatusStripButtonHandler(statusStripButtonShowBorders_Click);

            mnuDataSource.Click += new RoutedEventHandler(mnuDataSource_Click);
            mnuExport.Click += new RoutedEventHandler(mnuExport_Click);
            mnuMxN.Click += new RoutedEventHandler(mnuMxN_Click);
            mnu2x2.Click += new RoutedEventHandler(mnu2x2_Click);
            mnuFrequency.Click += new RoutedEventHandler(mnuFrequency_Click);
            mnuCombinedFrequency.Click += new RoutedEventHandler(mnuCombinedFrequency_Click);
            mnuChart.Click += new RoutedEventHandler(mnuChart_Click);
            mnuReset.Click += new RoutedEventHandler(mnuReset_Click);
            mnuRefresh.Click += new RoutedEventHandler(mnuRefresh_Click);
            mnuLinear.Click += new RoutedEventHandler(mnuLinear_Click);
            mnuLogistic.Click += new RoutedEventHandler(mnuLogistic_Click);
            mnuCSFrequency.Click += new RoutedEventHandler(mnuCSFrequency_Click);
            mnuCSMeans.Click += new RoutedEventHandler(mnuCSMeans_Click);
            mnuCSTables.Click += new RoutedEventHandler(mnuCSTables_Click);
            mnuPopSurvey.Click += new RoutedEventHandler(mnuPopSurvey_Click);
            mnuUnmatched.Click += new RoutedEventHandler(mnuUnmatched_Click);
            mnuStatCalc2x2.Click += new RoutedEventHandler(mnuStatCalc2x2_Click);
            mnuPoisson.Click += new RoutedEventHandler(mnuPoisson_Click);
            mnuBinomial.Click += new RoutedEventHandler(mnuBinomial_Click);
            mnuMatchedPairCC.Click += new RoutedEventHandler(mnuMatchedPairCC_Click);
            mnuChiSquare.Click += new RoutedEventHandler(mnuChiSquare_Click);
            mnuCohort.Click += new RoutedEventHandler(mnuCohort_Click);
            mnuMeans.Click += new RoutedEventHandler(mnuMeans_Click);
            mnuAberration.Click += new RoutedEventHandler(mnuAberration_Click);
            mnuLineList.Click += new RoutedEventHandler(mnuLineList_Click);
            mnuSaveOutput.Click += new RoutedEventHandler(mnuSaveOutput_Click);
            mnuProperties.Click += new RoutedEventHandler(mnuProperties_Click);

            mnuAddRelatedData.Click += new RoutedEventHandler(mnuAddRelatedData_Click);
            mnuOpen.Click += new RoutedEventHandler(mnuOpen_Click);
            mnuSave.Click += new RoutedEventHandler(mnuSave_Click);
            mnuSaveAs.Click += new RoutedEventHandler(mnuSaveAs_Click);

            // Reports
            mnuStandardTextBox.Click += new RoutedEventHandler(mnuStandardTextBox_Click);
            mnuStandardImageBox.Click += new RoutedEventHandler(mnuStandardImageBox_Click);

            //mainTabControl.AnalysisButtonClicked += new MouseButtonEventHandler(menu_AnalysisButtonClicked);
            //mainTabControl.DataButtonClicked += new MouseButtonEventHandler(menu_DataButtonClicked);
            //mainTabControl.VariablesButtonClicked += new MouseButtonEventHandler(menu_VariablesButtonClicked);
            //mainTabControl.ReportButtonClicked += new MouseButtonEventHandler(menu_ReportButtonClicked);

            menuControl.AnalysisButtonClicked += new MouseButtonEventHandler(menu_AnalysisButtonClicked);
            menuControl.DataButtonClicked += new MouseButtonEventHandler(menu_DataButtonClicked);
            menuControl.VariablesButtonClicked += new MouseButtonEventHandler(menu_VariablesButtonClicked);
            menuControl.ReportButtonClicked += new MouseButtonEventHandler(menu_ReportButtonClicked);

            #region Growth Chart Add Events

            // CDC 2000 Reference menu options
            //mnuCDC2000BMI.Click += new RoutedEventHandler(mnuCDC2000BMI_Click);
            //mnuCDC2000HA.Click += new RoutedEventHandler(mnuCDC2000HA_Click);
            //mnuCDC2000HCA.Click += new RoutedEventHandler(mnuCDC2000HCA_Click);
            //mnuCDC2000LA.Click += new RoutedEventHandler(mnuCDC2000LA_Click);
            //mnuCDC2000WA.Click += new RoutedEventHandler(mnuCDC2000WA_Click);
            //mnuCDC2000WH.Click += new RoutedEventHandler(mnuCDC2000WH_Click);
            //mnuCDC2000WL.Click += new RoutedEventHandler(mnuCDC2000WL_Click);

            //mnuWHO1978HA.Click += new RoutedEventHandler(mnuWHO1978HA_Click);
            //mnuWHO1978LA.Click += new RoutedEventHandler(mnuWHO1978LA_Click);
            //mnuWHO1978WA.Click += new RoutedEventHandler(mnuWHO1978WA_Click);
            //mnuWHO1978WH.Click += new RoutedEventHandler(mnuWHO1978WH_Click);
            //mnuWHO1978WL.Click += new RoutedEventHandler(mnuWHO1978WL_Click);

            //mnuWHOCGSBMI.Click += new RoutedEventHandler(mnuWHOCGSBMI_Click);
            //mnuWHOCGSHA.Click += new RoutedEventHandler(mnuWHOCGSHA_Click);
            //mnuWHOCGSHCA.Click += new RoutedEventHandler(mnuWHOCGSHCA_Click);
            //mnuWHOCGSLA.Click += new RoutedEventHandler(mnuWHOCGSLA_Click);
            //mnuWHOCGSWA.Click += new RoutedEventHandler(mnuWHOCGSWA_Click);
            //mnuWHOCGSWH.Click += new RoutedEventHandler(mnuWHOCGSWH_Click);
            //mnuWHOCGSWL.Click += new RoutedEventHandler(mnuWHOCGSWL_Click);
            //mnuWHOCGSMUACA.Click += new RoutedEventHandler(mnuWHOCGSMUACA_Click);

            //mnuWHO2007BMI.Click += new RoutedEventHandler(mnuWHO2007BMI_Click);
            //mnuWHO2007HA.Click += new RoutedEventHandler(mnuWHO2007HA_Click);
            //mnuWHO2007WA.Click += new RoutedEventHandler(mnuWHO2007WA_Click);

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

            //tooltipSave.Text = DashboardSharedStrings.TOOLTIP_MENU_SAVE;
            //tooltipSaveAs.Text = DashboardSharedStrings.TOOLTIP_MENU_SAVE_AS;
            //tooltipOpen.Text = DashboardSharedStrings.TOOLTIP_MENU_OPEN;
            //tooltipSetDataSource.Text = DashboardSharedStrings.TOOLTIP_MENU_SET_DATA_SOURCE;
            //tooltipRefresh.Text = DashboardSharedStrings.TOOLTIP_MENU_REFRESH;

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
            mnuProperties.Header = DashboardSharedStrings.CMENU_PROPERTIES;
            mnuRefresh.Header = DashboardSharedStrings.CMENU_REFRESH;
            mnuReset.Header = DashboardSharedStrings.CMENU_RESET;

            mnuLineList.Header = DashboardSharedStrings.CMENU_ADD_LINE_LIST;
            mnuFrequency.Header = DashboardSharedStrings.CMENU_ADD_FREQUENCY;
            mnuCombinedFrequency.Header = DashboardSharedStrings.CMENU_ADD_COMBINED_FREQUENCY;
            mnuAberration.Header = DashboardSharedStrings.CMENU_ADD_EARS;
            mnuMxN.Header = DashboardSharedStrings.CMENU_ADD_MXN;
            mnuMeans.Header = DashboardSharedStrings.CMENU_ADD_MEANS;
            mnuChart.Header = DashboardSharedStrings.CMENU_ADD_CHART;
            mnuAdvStats.Header = DashboardSharedStrings.CMENU_ADV_STATS;
            mnuLinear.Header = DashboardSharedStrings.CMENU_ADD_LINEAR;
            mnuLogistic.Header = DashboardSharedStrings.CMENU_ADD_LOGISTIC;
            mnuCSFrequency.Header = DashboardSharedStrings.CMENU_ADD_CSFREQ;
            mnuCSMeans.Header = DashboardSharedStrings.CMENU_ADD_CSMEANS;
            mnuCSTables.Header = DashboardSharedStrings.CMENU_ADD_CSTABLES;

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

        void menu_ReportButtonClicked(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        void menu_VariablesButtonClicked(object sender, MouseButtonEventArgs e)
        {
            if (dashboardHelper != null)
            {
                scrollViewerAnalysis.Visibility = Visibility.Collapsed;
                dataDictionary.Visibility = Visibility.Visible;
                dataDisplay.Visibility = Visibility.Collapsed;
            }
        }

        void menu_DataButtonClicked(object sender, MouseButtonEventArgs e)
        {
            if (dashboardHelper != null)
            {
                scrollViewerAnalysis.Visibility = Visibility.Collapsed;
                dataDictionary.Visibility = Visibility.Collapsed;
                dataDisplay.Visibility = Visibility.Visible;
            }
        }

        void menu_AnalysisButtonClicked(object sender, MouseButtonEventArgs e)
        {
            scrollViewerAnalysis.Visibility = Visibility.Visible;
            dataDictionary.Visibility = Visibility.Collapsed;
            dataDisplay.Visibility = Visibility.Collapsed;
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
            foreach (UserControl element in this.gadgets)
            {
                //if (element is IReportingGadget)
                //{
                //    IReportingGadget reportGadget = element as IReportingGadget;
                //    reportGadget.Select();
                //}
                if (element is GadgetBase)
                {
                    GadgetBase gadget = element as GadgetBase;
                    gadget.DrawBorders = true;
                }
                //if (element is ChartControl)
                //{
                //    ChartControl chart = element as ChartControl;
                //    chart.TurnOnBorders();
                //}
            }
        }

        private void HideGadgetBorders()
        {
            foreach (UserControl element in this.gadgets)
            {
                //if (element is IReportingGadget)
                //{
                //    IReportingGadget reportGadget = element as IReportingGadget;
                //    reportGadget.Deselect();
                //}
                if (element is GadgetBase)
                {
                    GadgetBase gadget = element as GadgetBase;
                    gadget.DrawBorders = false;
                    gadget.HideConfigPanel();
                }
                //if (element is ChartControl)
                //{
                //    ChartControl chart = element as ChartControl;
                //    chart.TurnOffBorders();
                //}
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
            if (this.dashboardHelper == null)
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
            if (this.dashboardHelper == null)
            {
                return;
            }

            try
            {
                string fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString("N") + ".html";//GetHTMLLineListing();

                System.IO.FileStream stream = System.IO.File.OpenWrite(fileName);
                System.IO.StreamWriter sw = new System.IO.StreamWriter(stream);
                sw.WriteLine(this.ToHTML());
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

        //void mnuWHO2007BMI_Click(object sender, RoutedEventArgs e)
        //{
        //    NutritionChartControl chartControl = new NutritionChartControl(dashboardHelper, GrowthMeasurement.BodyMassIndex, GrowthReference.WHO2007);
        //    canvasMain.Children.Add(chartControl);
        //    Canvas.SetLeft(chartControl, mousePoint.X - 10);
        //    Canvas.SetTop(chartControl, mousePoint.Y - 10);
        //    chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
        //    chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
        //    gadgets.Add(chartControl);
        //    EnableDisableOptions();
        //}

        //void mnuWHO2007HA_Click(object sender, RoutedEventArgs e)
        //{
        //    NutritionChartControl chartControl = new NutritionChartControl(dashboardHelper, GrowthMeasurement.HeightAge, GrowthReference.WHO2007);
        //    canvasMain.Children.Add(chartControl);
        //    Canvas.SetLeft(chartControl, mousePoint.X - 10);
        //    Canvas.SetTop(chartControl, mousePoint.Y - 10);
        //    chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
        //    chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
        //    gadgets.Add(chartControl);
        //    EnableDisableOptions();
        //}

        //void mnuWHO2007WA_Click(object sender, RoutedEventArgs e)
        //{
        //    NutritionChartControl chartControl = new NutritionChartControl(dashboardHelper, GrowthMeasurement.WeightAge, GrowthReference.WHO2007);
        //    canvasMain.Children.Add(chartControl);
        //    Canvas.SetLeft(chartControl, mousePoint.X - 10);
        //    Canvas.SetTop(chartControl, mousePoint.Y - 10);
        //    chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
        //    chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
        //    gadgets.Add(chartControl);
        //    EnableDisableOptions();
        //}

        //void mnuWHOCGSBMI_Click(object sender, RoutedEventArgs e)
        //{
        //    NutritionChartControl chartControl = new NutritionChartControl(dashboardHelper, GrowthMeasurement.BodyMassIndex, GrowthReference.WHO2006);
        //    canvasMain.Children.Add(chartControl);
        //    Canvas.SetLeft(chartControl, mousePoint.X - 10);
        //    Canvas.SetTop(chartControl, mousePoint.Y - 10);
        //    chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
        //    chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
        //    gadgets.Add(chartControl);
        //    EnableDisableOptions();
        //}

        //void mnuWHOCGSHA_Click(object sender, RoutedEventArgs e)
        //{
        //    NutritionChartControl chartControl = new NutritionChartControl(dashboardHelper, GrowthMeasurement.HeightAge, GrowthReference.WHO2006);
        //    canvasMain.Children.Add(chartControl);
        //    Canvas.SetLeft(chartControl, mousePoint.X - 10);
        //    Canvas.SetTop(chartControl, mousePoint.Y - 10);
        //    chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
        //    chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
        //    gadgets.Add(chartControl);
        //    EnableDisableOptions();
        //}

        //void mnuWHOCGSHCA_Click(object sender, RoutedEventArgs e)
        //{
        //    NutritionChartControl chartControl = new NutritionChartControl(dashboardHelper, GrowthMeasurement.HeadCircumferenceAge, GrowthReference.WHO2006);
        //    canvasMain.Children.Add(chartControl);
        //    Canvas.SetLeft(chartControl, mousePoint.X - 10);
        //    Canvas.SetTop(chartControl, mousePoint.Y - 10);
        //    chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
        //    chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
        //    gadgets.Add(chartControl);
        //    EnableDisableOptions();
        //}

        //void mnuWHOCGSLA_Click(object sender, RoutedEventArgs e)
        //{
        //    NutritionChartControl chartControl = new NutritionChartControl(dashboardHelper, GrowthMeasurement.LengthAge, GrowthReference.WHO2006);
        //    canvasMain.Children.Add(chartControl);
        //    Canvas.SetLeft(chartControl, mousePoint.X - 10);
        //    Canvas.SetTop(chartControl, mousePoint.Y - 10);
        //    chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
        //    chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
        //    gadgets.Add(chartControl);
        //    EnableDisableOptions();
        //}

        //void mnuWHOCGSWA_Click(object sender, RoutedEventArgs e)
        //{
        //    NutritionChartControl chartControl = new NutritionChartControl(dashboardHelper, GrowthMeasurement.WeightAge, GrowthReference.WHO2006);
        //    canvasMain.Children.Add(chartControl);
        //    Canvas.SetLeft(chartControl, mousePoint.X - 10);
        //    Canvas.SetTop(chartControl, mousePoint.Y - 10);
        //    chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
        //    chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
        //    gadgets.Add(chartControl);
        //    EnableDisableOptions();
        //}

        //void mnuWHOCGSWH_Click(object sender, RoutedEventArgs e)
        //{
        //    NutritionChartControl chartControl = new NutritionChartControl(dashboardHelper, GrowthMeasurement.WeightHeight, GrowthReference.WHO2006);
        //    canvasMain.Children.Add(chartControl);
        //    Canvas.SetLeft(chartControl, mousePoint.X - 10);
        //    Canvas.SetTop(chartControl, mousePoint.Y - 10);
        //    chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
        //    chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
        //    gadgets.Add(chartControl);
        //    EnableDisableOptions();
        //}

        //void mnuWHOCGSWL_Click(object sender, RoutedEventArgs e)
        //{
        //    NutritionChartControl chartControl = new NutritionChartControl(dashboardHelper, GrowthMeasurement.WeightLength, GrowthReference.WHO2006);
        //    canvasMain.Children.Add(chartControl);
        //    Canvas.SetLeft(chartControl, mousePoint.X - 10);
        //    Canvas.SetTop(chartControl, mousePoint.Y - 10);
        //    chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
        //    chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
        //    gadgets.Add(chartControl);
        //    EnableDisableOptions();
        //}

        //void mnuWHOCGSMUACA_Click(object sender, RoutedEventArgs e)
        //{
        //    NutritionChartControl chartControl = new NutritionChartControl(dashboardHelper, GrowthMeasurement.ArmCircumferenceAge, GrowthReference.WHO2006);
        //    canvasMain.Children.Add(chartControl);
        //    Canvas.SetLeft(chartControl, mousePoint.X - 10);
        //    Canvas.SetTop(chartControl, mousePoint.Y - 10);
        //    chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
        //    chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
        //    gadgets.Add(chartControl);
        //    EnableDisableOptions();
        //}


        //void mnuWHO1978HA_Click(object sender, RoutedEventArgs e)
        //{
        //    NutritionChartControl chartControl = new NutritionChartControl(dashboardHelper, GrowthMeasurement.HeightAge, GrowthReference.WHO1978);
        //    canvasMain.Children.Add(chartControl);
        //    Canvas.SetLeft(chartControl, mousePoint.X - 10);
        //    Canvas.SetTop(chartControl, mousePoint.Y - 10);
        //    chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
        //    chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
        //    gadgets.Add(chartControl);
        //    EnableDisableOptions();
        //}

        //void mnuWHO1978LA_Click(object sender, RoutedEventArgs e)
        //{
        //    NutritionChartControl chartControl = new NutritionChartControl(dashboardHelper, GrowthMeasurement.LengthAge, GrowthReference.WHO1978);
        //    canvasMain.Children.Add(chartControl);
        //    Canvas.SetLeft(chartControl, mousePoint.X - 10);
        //    Canvas.SetTop(chartControl, mousePoint.Y - 10);
        //    chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
        //    chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
        //    gadgets.Add(chartControl);
        //    EnableDisableOptions();
        //}

        //void mnuWHO1978WA_Click(object sender, RoutedEventArgs e)
        //{
        //    NutritionChartControl chartControl = new NutritionChartControl(dashboardHelper, GrowthMeasurement.WeightAge, GrowthReference.WHO1978);
        //    canvasMain.Children.Add(chartControl);
        //    Canvas.SetLeft(chartControl, mousePoint.X - 10);
        //    Canvas.SetTop(chartControl, mousePoint.Y - 10);
        //    chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
        //    chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
        //    gadgets.Add(chartControl);
        //    EnableDisableOptions();
        //}

        //void mnuWHO1978WH_Click(object sender, RoutedEventArgs e)
        //{
        //    NutritionChartControl chartControl = new NutritionChartControl(dashboardHelper, GrowthMeasurement.WeightHeight, GrowthReference.WHO1978);
        //    canvasMain.Children.Add(chartControl);
        //    Canvas.SetLeft(chartControl, mousePoint.X - 10);
        //    Canvas.SetTop(chartControl, mousePoint.Y - 10);
        //    chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
        //    chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
        //    gadgets.Add(chartControl);
        //    EnableDisableOptions();
        //}

        //void mnuWHO1978WL_Click(object sender, RoutedEventArgs e)
        //{
        //    NutritionChartControl chartControl = new NutritionChartControl(dashboardHelper, GrowthMeasurement.WeightLength, GrowthReference.WHO1978);
        //    canvasMain.Children.Add(chartControl);
        //    Canvas.SetLeft(chartControl, mousePoint.X - 10);
        //    Canvas.SetTop(chartControl, mousePoint.Y - 10);
        //    chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
        //    chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
        //    gadgets.Add(chartControl);
        //    EnableDisableOptions();
        //}


        //void mnuCDC2000BMI_Click(object sender, RoutedEventArgs e)
        //{
        //    NutritionChartControl chartControl = new NutritionChartControl(dashboardHelper, GrowthMeasurement.BodyMassIndex, GrowthReference.CDC2000);
        //    canvasMain.Children.Add(chartControl);
        //    Canvas.SetLeft(chartControl, mousePoint.X - 10);
        //    Canvas.SetTop(chartControl, mousePoint.Y - 10);
        //    chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
        //    chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
        //    gadgets.Add(chartControl);
        //    EnableDisableOptions();
        //}

        //void mnuCDC2000HA_Click(object sender, RoutedEventArgs e)
        //{
        //    NutritionChartControl chartControl = new NutritionChartControl(dashboardHelper, GrowthMeasurement.HeightAge, GrowthReference.CDC2000);
        //    canvasMain.Children.Add(chartControl);
        //    Canvas.SetLeft(chartControl, mousePoint.X - 10);
        //    Canvas.SetTop(chartControl, mousePoint.Y - 10);
        //    chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
        //    chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
        //    gadgets.Add(chartControl);
        //    EnableDisableOptions();
        //}

        //void mnuCDC2000HCA_Click(object sender, RoutedEventArgs e)
        //{
        //    NutritionChartControl chartControl = new NutritionChartControl(dashboardHelper, GrowthMeasurement.HeadCircumferenceAge, GrowthReference.CDC2000);
        //    canvasMain.Children.Add(chartControl);
        //    Canvas.SetLeft(chartControl, mousePoint.X - 10);
        //    Canvas.SetTop(chartControl, mousePoint.Y - 10);
        //    chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
        //    chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
        //    gadgets.Add(chartControl);
        //    EnableDisableOptions();
        //}

        //void mnuCDC2000LA_Click(object sender, RoutedEventArgs e)
        //{
        //    NutritionChartControl chartControl = new NutritionChartControl(dashboardHelper, GrowthMeasurement.LengthAge, GrowthReference.CDC2000);
        //    canvasMain.Children.Add(chartControl);
        //    Canvas.SetLeft(chartControl, mousePoint.X - 10);
        //    Canvas.SetTop(chartControl, mousePoint.Y - 10);
        //    chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
        //    chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
        //    gadgets.Add(chartControl);
        //    EnableDisableOptions();
        //}

        //void mnuCDC2000WA_Click(object sender, RoutedEventArgs e)
        //{
        //    NutritionChartControl chartControl = new NutritionChartControl(dashboardHelper, GrowthMeasurement.WeightAge, GrowthReference.CDC2000);
        //    canvasMain.Children.Add(chartControl);
        //    Canvas.SetLeft(chartControl, mousePoint.X - 10);
        //    Canvas.SetTop(chartControl, mousePoint.Y - 10);
        //    chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
        //    chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
        //    gadgets.Add(chartControl);
        //    EnableDisableOptions();
        //}

        //void mnuCDC2000WH_Click(object sender, RoutedEventArgs e)
        //{
        //    NutritionChartControl chartControl = new NutritionChartControl(dashboardHelper, GrowthMeasurement.WeightHeight, GrowthReference.CDC2000);
        //    canvasMain.Children.Add(chartControl);
        //    Canvas.SetLeft(chartControl, mousePoint.X - 10);
        //    Canvas.SetTop(chartControl, mousePoint.Y - 10);
        //    chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
        //    chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
        //    gadgets.Add(chartControl);
        //    EnableDisableOptions();
        //}

        //void mnuCDC2000WL_Click(object sender, RoutedEventArgs e)
        //{
        //    NutritionChartControl chartControl = new NutritionChartControl(dashboardHelper, GrowthMeasurement.WeightLength, GrowthReference.CDC2000);
        //    canvasMain.Children.Add(chartControl);
        //    Canvas.SetLeft(chartControl, mousePoint.X - 10);
        //    Canvas.SetTop(chartControl, mousePoint.Y - 10);
        //    chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
        //    chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
        //    gadgets.Add(chartControl);
        //    EnableDisableOptions();
        //}

        void mnuBinomial_Click(object sender, RoutedEventArgs e)
        {
            //StatCalc.Binomial binomial = new StatCalc.Binomial();
            //canvasMain.Children.Add(binomial);
            //Canvas.SetLeft(binomial, mousePoint.X - 10);
            //Canvas.SetTop(binomial, mousePoint.Y - 10);
            //binomial.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            //binomial.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            //gadgets.Add(binomial);
            //EnableDisableOptions();
        }

        void mnuPoisson_Click(object sender, RoutedEventArgs e)
        {
            //StatCalc.Poisson poisson = new StatCalc.Poisson();
            //canvasMain.Children.Add(poisson);
            //Canvas.SetLeft(poisson, mousePoint.X - 10);
            //Canvas.SetTop(poisson, mousePoint.Y - 10);
            //poisson.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            //poisson.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            //gadgets.Add(poisson);
            //EnableDisableOptions();
        }

        void mnuChiSquare_Click(object sender, RoutedEventArgs e)
        {
            //StatCalc.ChiSquareControl chiSquare = new StatCalc.ChiSquareControl();
            //canvasMain.Children.Add(chiSquare);
            //Canvas.SetLeft(chiSquare, mousePoint.X - 10);
            //Canvas.SetTop(chiSquare, mousePoint.Y - 10);
            //chiSquare.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            //chiSquare.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            //gadgets.Add(chiSquare);
            //EnableDisableOptions();
        }

        void DashboardControl_ContextMenuOpening(object sender, System.Windows.Controls.ContextMenuEventArgs e)
        {
            mousePoint = Mouse.GetPosition(canvasMain);
            introAvailableData.Visibility = System.Windows.Visibility.Collapsed;
        }

        void scrollViewerAnalysis_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.Source == this.scrollViewerAnalysis)
            {
                if (dataFilteringControl != null)
                {
                    if (sliderZoom.Value == 100)
                    {
                        Canvas.SetTop(dataFilteringControl, e.VerticalOffset + (scrollViewerAnalysis.ActualHeight / 2.0) - (dataFilteringControl.ActualHeight / 2.0));
                    }
                    else
                    {
                        double factor = sliderZoom.Value / 100.0;
                        Canvas.SetTop(dataFilteringControl, (e.VerticalOffset / factor) + (scrollViewerAnalysis.ActualHeight / factor / 2.0) - (dataFilteringControl.ActualHeight / factor / 2.0));
                    }
                }
                if (variablesControl != null)
                {
                    if (sliderZoom.Value == 100)
                    {
                        Canvas.SetTop(variablesControl, e.VerticalOffset + (scrollViewerAnalysis.ActualHeight / 2.0) - (variablesControl.ActualHeight / 2.0));
                    }
                    else
                    {
                        double factor = sliderZoom.Value / 100.0;
                        Canvas.SetTop(variablesControl, (e.VerticalOffset / factor) + (scrollViewerAnalysis.ActualHeight / factor / 2.0) - (variablesControl.ActualHeight / factor / 2.0));
                    }
                }

                if (e.VerticalOffset > 0.0)
                {
                    introAvailableData.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        void mnuMatchedPairCC_Click(object sender, RoutedEventArgs e)
        {
            //EpiDashboard.Gadgets.StatCalc.MatchedPairCaseControlGadget matchedCaseControlControl = new EpiDashboard.Gadgets.StatCalc.MatchedPairCaseControlGadget();
            //AddGadgetToCanvasFromContextMenu(matchedCaseControlControl);
        }

        void mnuAberration_Click(object sender, RoutedEventArgs e)
        {
            AberrationControl aberrationControl = new AberrationControl(dashboardHelper);
            AddGadgetToCanvasFromContextMenu(aberrationControl);
        }

        public void AddGadgetToCanvasFromContextMenu(UserControl control)
        {
            if (control is IGadget)
            {
                IGadget gadget = control as IGadget;

                canvasMain.Children.Add(control);
                Canvas.SetLeft(control, mousePoint.X - 10);
                Canvas.SetTop(control, mousePoint.Y - 10);

                gadgets.Add(control);
                gadget.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
                gadget.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
                gadget.GadgetRefreshed += new GadgetRefreshedHandler(gadget_GadgetRefreshed);
                gadget.GadgetReposition += new GadgetRepositionEventHandler(gadgetBase_GadgetReposition);

                if (IsShowingGadgetBorders)
                {
                    gadget.DrawBorders = true;
                }
                else
                {
                    gadget.DrawBorders = false;
                }

                IsCanvasDirty = true;
                if (CanvasChanged != null)
                {
                    CanvasChanged(this.CurrentCanvas);
                }
            }
            EnableDisableOptions();

            introAvailableData.Visibility = System.Windows.Visibility.Collapsed;
        }

        public void AddGadgetToCanvasFromFile(IGadget gadget)
        {
            gadget.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            gadget.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            gadget.GadgetRefreshed += new GadgetRefreshedHandler(gadget_GadgetRefreshed);
            gadget.GadgetReposition += new GadgetRepositionEventHandler(gadgetBase_GadgetReposition);

            if (IsShowingGadgetBorders)
            {
                gadget.DrawBorders = true;
            }
            else
            {
                gadget.DrawBorders = false;
            }

            this.gadgets.Add((UserControl)gadget);
            canvasMain.Children.Add((UserControl)gadget);

            EnableDisableOptions();
        }

        void gadgetBase_GadgetReposition(object sender, GadgetRepositionEventArgs e)
        {
            if (sender is UserControl)
            {
                CenterControlHorizontally(sender as UserControl);
            }
        }

        public void AddRelatedData()
        {
            //if (RelatedDataRequested != null)
            //{
            //    RelatedConnection rConn = RelatedDataRequested();

            //    if (rConn != null)
            //    {
            //        dashboardHelper.AddRelatedDataSource(rConn);
            //        dataFilteringControl.FillSelectionComboboxes();
            //    }
            //}
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

        private DashboardHelper DashboardHelperRequested()
        {
            Epi.WPF.Dashboard.Dialogs.NewCanvasWindow newCanvasWindow = new Epi.WPF.Dashboard.Dialogs.NewCanvasWindow();
            
            //if (dashboardHelper != null && dashboardHelper.Database != null && !dashboardHelper.IsUsingEpiProject)
            //{
            //    newCanvasWindow = new Epi.WPF.Dashboard.Dialogs.NewCanvasWindow(dashboardHelper.Database);
            //}
            //else 
            if (dashboardHelper != null && dashboardHelper.Database != null && dashboardHelper.IsUsingEpiProject)
            {
                newCanvasWindow = new Epi.WPF.Dashboard.Dialogs.NewCanvasWindow(dashboardHelper.View.Project);
            }

            if (newCanvasWindow.ShowDialog() == true)
            {
                if (newCanvasWindow.SelectedDataSource is Project)
                {
                    Project project = (Project)newCanvasWindow.SelectedDataSource;
                    View view = project.GetViewByName(newCanvasWindow.SelectedDataMember);                    

                    if (!File.Exists(project.FilePath))
                    {
                        Epi.WPF.Dashboard.Dialogs.MsgBox.ShowInformation(string.Format(SharedStrings.DASHBOARD_ERROR_PROJECT_NOT_FOUND, project.FilePath));
                        return null;
                    }
                    IDbDriver dbDriver = DBReadExecute.GetDataDriver(project.FilePath);
                    if (!dbDriver.TableExists(view.TableName))
                    {
                        Epi.WPF.Dashboard.Dialogs.MsgBox.ShowInformation(string.Format(SharedStrings.DATA_TABLE_NOT_FOUND, view.Name));
                        return null;
                    }
                    else
                    {
                        Configuration.OnDataSourceAccessed(project.Name, Configuration.Encrypt(project.FilePath), newCanvasWindow.SelectedDataProvider);
                        dashboardHelper = new Epi.WPF.Dashboard.DashboardHelper(view, dbDriver);
                    }
                }
                else
                {                    
                    IDbDriver dbDriver = (IDbDriver)newCanvasWindow.SelectedDataSource;
                    string dbName = dbDriver.DbName;
                    //if (string.IsNullOrEmpty(newCanvasWindow.SQLQuery))
                    //{
                    Configuration.OnDataSourceAccessed(dbName, Configuration.Encrypt(dbDriver.ConnectionString), newCanvasWindow.SelectedDataProvider);
                    dashboardHelper = new Epi.WPF.Dashboard.DashboardHelper(newCanvasWindow.SelectedDataMember, dbDriver);
                    //}
                    //else
                    //{
                    //    dashboardHelper = new Epi.WPF.Dashboard.DashboardHelper(newCanvasWindow.SelectedDataMember, newCanvasWindow.SQLQuery, dbDriver);
                    //}
                }

                return dashboardHelper;
            }
            return null;
        }

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
            
            DashboardHelper temp = DashboardHelperRequested();

            if (temp != null)
            {
                temp.NotificationEvent += new NotificationEventHandler(dashboardHelper_NotificationEvent);
                instructionsPanel.Visibility = Visibility.Collapsed;
                loadingPanel.Visibility = System.Windows.Visibility.Visible;
                CenterControlHorizontally(loadingPanel);

                dashboardHelper = temp;
                dashboardHelper.SetDashboardControl(this);

                AddFilterGadget();
                AddDefinedVariablesGadget();
                DisablePermanentGadgets();

                worker = new System.ComponentModel.BackgroundWorker();
                worker.WorkerSupportsCancellation = true;
                worker.DoWork += new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
                worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(worker_WorkerCompleted);
                worker.RunWorkerAsync();

                RemoveAllGadgets();
                ResetCanvasProperties();
                EnableDisableMenus(true);
                this.currentCanvas = string.Empty;
                this.lastSavedCanvasFileName = string.Empty;

                if (CanvasChanged != null)
                {
                    CanvasChanged(string.Empty);
                }

                UpdateRecordCount();

                this.mnuCombinedFrequency.Visibility = Visibility.Visible;
                this.scrollViewerAnalysis.ScrollToTop();

                dataDictionary.SetDataView(this.dashboardHelper.GenerateDataDictionaryTable().DefaultView);
                dataDictionary.Refresh();

                dataDisplay.SetDataView(this.dashboardHelper.GenerateView());
                dataDisplay.Refresh();
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

            //tblockDataSource.Text = string.IsNullOrEmpty(dashboardHelper.TableName) ? dashboardHelper.View.Project.Name + "\\" + dashboardHelper.View.Name : dashboardHelper.Database.DbName + "\\" + dashboardHelper.TableName;
            //tblockDataSourceLabel.Visibility = System.Windows.Visibility.Visible;
            //tblockDataSource.Visibility = System.Windows.Visibility.Visible;
            string recordCounter = string.Format(DashboardSharedStrings.RECORD_COUNT, dashboardHelper.RecordCount.ToString("N0"));
            //tblockRecordCount.Text = recordCounter;

            //if (dashboardHelper.Database != null)
            //{
            //    tooltipDataSourceLabel.Text = dashboardHelper.Database.ConnectionDescription;
            //}

            //tblockRecordCount.Visibility = System.Windows.Visibility.Visible;
            //borderDataSource.Visibility = System.Windows.Visibility.Visible;

            string fieldCounter = string.Format(DashboardSharedStrings.FIELD_COUNT, dashboardHelper.TableColumnNames.Count.ToString());
            tblockFieldCount.Text = fieldCounter;
            tblockFieldCount.Visibility = System.Windows.Visibility.Visible;

            //tblockCacheTime.Text = dashboardHelper.TimeToCache;
            //tblockCacheTime.Visibility = System.Windows.Visibility.Visible;

            if (RecordCountChanged != null)
            {
                RecordCountChanged(dashboardHelper.RecordCount, string.IsNullOrEmpty(dashboardHelper.TableName) ? dashboardHelper.View.Project.Name + "\\" + dashboardHelper.View.Name : dashboardHelper.Database.DbName + "\\" + dashboardHelper.TableName);
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

            dashboardHelper.DataSet.Tables.Clear();
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
                RemoveAllGadgets();

            EnableDisableMenus(false);
            mnuExport.IsEnabled = false;
            mnuProperties.IsEnabled = false;

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
            isPopulatingDataSet = false;
            mnuExport.IsEnabled = true;
            mnuProperties.IsEnabled = true;
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
            isPopulatingDataSet = true;
            RequestUpdateStatusDelegate requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
            CheckForCancellationDelegate checkForCancellation = new CheckForCancellationDelegate(IsCancelled);
            SetGadgetStatusHandler setGadgetProgress = new SetGadgetStatusHandler(RequestProgressUpdate);
            GadgetParameters inputs = new GadgetParameters();
            inputs.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
            inputs.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);
            inputs.GadgetCheckForProgress += new SetGadgetStatusHandler(setGadgetProgress);

            try
            {
                dashboardHelper.UserVarsNeedUpdating = true;
                dashboardHelper.PopulateDataSet(inputs);
            }
            catch (System.Data.EvaluateException)
            {
                this.Dispatcher.BeginInvoke(new SendNotificationMessageHandler(SendNotificationMessage), "The specified data filter is no longer valid.", true, NotificationButtonType.OK, true, 10);
            }
            catch (Exception ex)
            {
                Epi.WPF.Dashboard.Dialogs.MsgBox.ShowException(ex);
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
                Epi.WPF.Dashboard.Dialogs.MsgBox.ShowError(DashboardSharedStrings.ERROR_DATA_SOURCE_GENERIC); // todo: Move to a non-modal dialog
            }
        }

        void mnuExport_Click(object sender, RoutedEventArgs e)
        {
            if (DashboardHelper != null)
            {
                ExportWindow exportWindow = new ExportWindow(this.dashboardHelper);
                exportWindow.ShowDialog();
                //if (dataExportingControl != null)
                //{
                //    canvasMain.Children.Remove(dataExportingControl);
                //}
                //dataExportingControl = new ExportControl(dashboardHelper);
                //dataExportingControl.Loaded += new RoutedEventHandler(dataExportControl_Loaded);
                //dataExportingControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
                //DragCanvas.SetZIndex(dataExportingControl, 10000);
                //canvasMain.Children.Add(dataExportingControl);

                //DragCanvas.SetCanBeDragged(dataExportingControl, true);
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
            if (dashboardHelper != null)
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

            if (dashboardHelper != null)
            {
                config = dashboardHelper.Config;
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
                    //fileInfoList.Add(canvasFi.LastWriteTime, canvasFi);
                    //LoadQuickDataSource(canvasFi);
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
                        //fileInfoList.Add(canvasFi.LastWriteTime, canvasFi);
                        //LoadQuickDataSource(canvasFi);
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
                if (border.Tag is FileInfo)
                {
                    if (CanCloseDashboard())
                    {
                        FileInfo fi = (border.Tag as FileInfo);
                        OpenCanvas(fi.FullName);
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
            //EpiDashboard.Gadgets.Reporting.StandardImageControl ic = new EpiDashboard.Gadgets.Reporting.StandardImageControl(this.dashboardHelper, this);
            //canvasMain.Children.Add(ic);
            //Canvas.SetLeft(ic, mousePoint.X - 10);
            //Canvas.SetTop(ic, mousePoint.Y - 10);
            //ic.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            //ic.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            //gadgets.Add(ic);
            //EnableDisableOptions();
        }

        void mnuStandardTextBox_Click(object sender, RoutedEventArgs e)
        {
            //EpiDashboard.Gadgets.Reporting.StandardTextControl stc = new EpiDashboard.Gadgets.Reporting.StandardTextControl();
            //canvasMain.Children.Add(stc);
            //Canvas.SetLeft(stc, mousePoint.X - 10);
            //Canvas.SetTop(stc, mousePoint.Y - 10);
            //stc.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            //stc.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            //gadgets.Add(stc);
            //EnableDisableOptions();

            //AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(stc);
            //adornerLayer.Add(new ResizingAdorner(stc));
        }

        void mnuCohort_Click(object sender, RoutedEventArgs e)
        {
            //StatCalc.Cohort cohort = new StatCalc.Cohort();
            //canvasMain.Children.Add(cohort);
            //Canvas.SetLeft(cohort, mousePoint.X - 10);
            //Canvas.SetTop(cohort, mousePoint.Y - 10);
            //cohort.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            //cohort.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            //gadgets.Add(cohort);
            //EnableDisableOptions();
        }

        void mnuStatCalc2x2_Click(object sender, RoutedEventArgs e)
        {
            //StatCalc.TwoByTwo twoByTwo = new StatCalc.TwoByTwo();
            //canvasMain.Children.Add(twoByTwo);
            //Canvas.SetLeft(twoByTwo, mousePoint.X - 10);
            //Canvas.SetTop(twoByTwo, mousePoint.Y - 10);
            //twoByTwo.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            //twoByTwo.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            //gadgets.Add(twoByTwo);
            //EnableDisableOptions();
        }

        void mnuUnmatched_Click(object sender, RoutedEventArgs e)
        {
            //StatCalc.UnmatchedCaseControl unmatchedCC = new StatCalc.UnmatchedCaseControl();
            //canvasMain.Children.Add(unmatchedCC);
            //Canvas.SetLeft(unmatchedCC, mousePoint.X - 10);
            //Canvas.SetTop(unmatchedCC, mousePoint.Y - 10);
            //unmatchedCC.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            //unmatchedCC.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            //gadgets.Add(unmatchedCC);
            //EnableDisableOptions();
        }

        void mnuPopSurvey_Click(object sender, RoutedEventArgs e)
        {
            //StatCalc.PopulationSurvey popSurvey = new StatCalc.PopulationSurvey();
            //canvasMain.Children.Add(popSurvey);
            //Canvas.SetLeft(popSurvey, mousePoint.X - 10);
            //Canvas.SetTop(popSurvey, mousePoint.Y - 10);
            //popSurvey.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            //popSurvey.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            //gadgets.Add(popSurvey);
            //EnableDisableOptions();
        }

        void mnuRefresh_Click(object sender, RoutedEventArgs e)
        {
            //RefreshResults();
            dashboardHelper.UserVarsNeedUpdating = true;
            ReCacheDataSource(false);
        }

        void mnuReset_Click(object sender, RoutedEventArgs e)
        {
            RemoveAllGadgets();
            //this.progressBar.Visibility = System.Windows.Visibility.Collapsed;
            CenterControlHorizontally(spTitle);
            CenterControlHorizontally(instructionsPanel);
        }

        private void RemoveAllGadgets()
        {
            foreach (UserControl gadget in gadgets)
            {
                canvasMain.Children.Remove(gadget);
            }
            gadgets.Clear();
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
            //spTitle.Width = e.NewSize.Width;
            CenterControlHorizontally(spTitle);
            CenterControlHorizontally(instructionsPanel);
            CenterControlHorizontally(loadingPanel);
            if (introPanel != null)
            {
                CenterControlHorizontally(introPanel);
            }
            CenterControlHorizontally(introAvailableData);

            //if (introAvailableData.Visibility == System.Windows.Visibility.Visible && pathDataTriangle.Visibility == System.Windows.Visibility.Visible)
            //{
            //    int column = Grid.GetColumn(tblockOpen);
            //    double width = 0;
            //    for (int i = 0; i < column; i++)
            //    {
            //        width = width + grdMenuBarInner.ColumnDefinitions[i].ActualWidth;
            //    }

            //    Canvas.SetLeft(introAvailableData, width);
            //    Canvas.SetTop(introAvailableData, 24);
            //}
        }

        void mnuChart_Click(object sender, RoutedEventArgs e)
        {
            //ChartControl chartControl = new ChartControl(dashboardHelper);
            //canvasMain.Children.Add(chartControl);
            //Canvas.SetLeft(chartControl, mousePoint.X - 10);
            //Canvas.SetTop(chartControl, mousePoint.Y - 10);
            //chartControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            //chartControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            //gadgets.Add(chartControl);
            //EnableDisableOptions();
        }

        void mnuFrequency_Click(object sender, RoutedEventArgs e)
        {
            FrequencyControl freqControl = new FrequencyControl(dashboardHelper);
            AddGadgetToCanvasFromContextMenu(freqControl);
        }

        void mnuLineList_Click(object sender, RoutedEventArgs e)
        {
            LineListControl listControl = new LineListControl(dashboardHelper);
            AddGadgetToCanvasFromContextMenu(listControl);
        }

        void mnuCombinedFrequency_Click(object sender, RoutedEventArgs e)
        {
            CombinedFrequencyControl combinedFreqControl = new CombinedFrequencyControl(dashboardHelper);
            AddGadgetToCanvasFromContextMenu(combinedFreqControl);
        }

        void mnuMeans_Click(object sender, RoutedEventArgs e)
        {
            MeansControl meansControl = new MeansControl(dashboardHelper);
            AddGadgetToCanvasFromContextMenu(meansControl);
        }

        void mnuSaveOutput_Click(object sender, RoutedEventArgs e)
        {
            SaveOutputAsHTMLAndDisplay();
        }

        void mnuProperties_Click(object sender, RoutedEventArgs e)
        {
            //if (DashboardHelper != null)
            //{
            //    if (propertiesControl != null)
            //    {
            //        canvasMain.Children.Remove(propertiesControl);
            //    }
            //    propertiesControl = new PropertiesControl(DashboardHelper, this);
            //    propertiesControl.Loaded += new RoutedEventHandler(propertiesControl_Loaded);
            //    propertiesControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            //    DragCanvas.SetZIndex(propertiesControl, 10000);
            //    canvasMain.Children.Add(propertiesControl);

            //    DragCanvas.SetCanBeDragged(propertiesControl, true);
            //}
        }

        void gadget_GadgetClosing(UserControl gadget)
        {
            if (gadget is IGadget)
            {
                ((IGadget)gadget).GadgetClosing -= new GadgetClosingHandler(gadget_GadgetClosing);
                ((IGadget)gadget).GadgetProcessingFinished -= new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
                ((IGadget)gadget).GadgetRefreshed -= new GadgetRefreshedHandler(gadget_GadgetRefreshed);
                ((IGadget)gadget).GadgetReposition -= new GadgetRepositionEventHandler(gadgetBase_GadgetReposition);
                gadgets.Remove(gadget);
            }
            canvasMain.Children.Remove(gadget);
            gadget = null;
            EnableDisableOptions();

            //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced); // was necessary when doing on-demand to reduce mem footprint but will try disabling this now that we're using cached data.
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

            foreach (UserControl control in gadgets)
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
                mnuExport.IsEnabled = true;
            }

            if (allFinished && IsGeneratingHTMLFromCommandLine)
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

                    //MessageBox.Show("Output has been saved.", "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);                    

                    if (this.HTMLGenerated != null)
                    {
                        HTMLGenerated();
                    }
                }
                catch (Exception ex)
                {
                    Epi.WPF.Dashboard.Dialogs.MsgBox.ShowException(ex);
                }
            }
        }

        private void EnableDisableOptions()
        {
            tblockGadgetsDescription.Text = string.Empty;

            if (gadgets.Count > 0)
            {
                mnuRefresh.IsEnabled = true;
                mnuReset.IsEnabled = true;
                if (!this.IsPopulatingDataSet)
                {
                    //spTitle.Visibility = Visibility.Hidden;
                    instructionsPanel.Visibility = System.Windows.Visibility.Hidden;
                }
                tooltipGadgets.IsEnabled = true;
                tooltipGadgets.Visibility = System.Windows.Visibility.Visible;

                Dictionary<string, int> gadgetCounts = new Dictionary<string, int>();

                //foreach (UserControl control in this.Gadgets)
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
                //spTitle.Visibility = Visibility.Visible;
                if (dashboardHelper != null)
                {
                    instructionsPanel.Visibility = Visibility.Visible;
                }
                tooltipGadgets.IsEnabled = false;
                tooltipGadgets.Visibility = Visibility.Collapsed;
            }

            string gadgetCounter = string.Format(DashboardSharedStrings.GADGET_COUNT, gadgets.Count.ToString());
            tblockGadgetCount.Text = gadgetCounter; // gadgets.Count + " " + "Gadgets";
        }

        public DashboardHelper DashboardHelper
        {
            get
            {
                return this.dashboardHelper;
            }
        }

        public bool IsCanvasDirty
        {
            get
            {
                return isCanvasDirty;
            }
            set
            {
                isCanvasDirty = value;
            }
        }

        public bool IsPopulatingDataSet
        {
            get
            {
                return this.isPopulatingDataSet;
            }
        }

        public bool IsGeneratingHTMLFromCommandLine
        {
            get
            {
                return isGeneratingHTMLFromCommandLine;
            }
            set
            {
                isGeneratingHTMLFromCommandLine = value;
            }
        }

        public string HTMLFilePath
        {
            get
            {
                return htmlFilePath;
            }
            set
            {
                htmlFilePath = value;
            }
        }

        public bool ShowGadgetHeadingsInOutput
        {
            get
            {
                return showGadgetHeadingsInOutput;
            }
            set
            {
                showGadgetHeadingsInOutput = value;
            }
        }

        public bool ShowGadgetSettingsInOutput
        {
            get
            {
                return showGadgetSettingsInOutput;
            }
            set
            {
                showGadgetSettingsInOutput = value;
            }
        }

        public bool ShowCanvasSummaryInfoInOutput
        {
            get
            {
                return showCanvasSummaryInfoInOutput;
            }
            set
            {
                this.showCanvasSummaryInfoInOutput = value;
            }
        }

        public bool SortGadgetsTopToBottom
        {
            get
            {
                return this.sortGadgetsTopToBottom;
            }
            set
            {
                this.sortGadgetsTopToBottom = value;
            }
        }

        public bool UseAlternatingColorsInOutput
        {
            get
            {
                return this.useAlternatingColorsInOutput;
            }
            set
            {
                this.useAlternatingColorsInOutput = value;
            }
        }

        internal string LastSavedCanvasFileName
        {
            get
            {
                return this.lastSavedCanvasFileName;
            }
        }

        public string CustomOutputHeading
        {
            get
            {
                return customOutputHeading;
            }
            set
            {
                customOutputHeading = value;
            }
        }

        public string CustomOutputSummaryText
        {
            get
            {
                return customOutputSummaryText;
            }
            set
            {
                customOutputSummaryText = value;
            }
        }

        public string CustomOutputConclusionText
        {
            get
            {
                return customOutputConclusionText;
            }
            set
            {
                customOutputConclusionText = value;
            }
        }

        public string CustomOutputTableFontFamily
        {
            get
            {
                return customOutputTableFontFamily;
            }
            set
            {
                customOutputTableFontFamily = value;
            }
        }

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

        public string CurrentCanvas
        {
            get
            {
                return this.currentCanvas;
            }
        }

        public List<UserControl> Gadgets
        {
            get
            {
                return this.gadgets;
            }
        }

        /// <summary>
        /// Gets whether or not the dashboard control has any user-created content
        /// </summary>
        public bool HasGadgets
        {
            get
            {
                if (dashboardHelper == null)
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
                else if (this.dashboardHelper.Rules.Count > 0)
                {
                    return true;
                }
                else if (this.dashboardHelper.DataFilters.Count > 0)
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
            foreach (UserControl gadget in gadgets)
            {
                ((IGadget)gadget).RefreshResults();
            }

            UpdateRecordCount();
            
            dataDictionary.SetDataView(this.dashboardHelper.GenerateDataDictionaryTable().DefaultView);
            dataDictionary.Refresh();

            dataDisplay.SetDataView(this.dashboardHelper.GenerateView());
            dataDisplay.Refresh();
        }

        void mnuMxN_Click(object sender, RoutedEventArgs e)
        {
            CrosstabControl crsControl = new CrosstabControl(dashboardHelper);
            AddGadgetToCanvasFromContextMenu(crsControl);
        }

        void mnu2x2_Click(object sender, RoutedEventArgs e)
        {
            //TwoByTwoTableControl crsControl = new TwoByTwoTableControl(dashboardHelper);
            //canvasMain.Children.Add(crsControl);
            //Canvas.SetLeft(crsControl, mousePoint.X - 10);
            //Canvas.SetTop(crsControl, mousePoint.Y - 10);
            //gadgets.Add(crsControl);
            //crsControl.GadgetClosing += new GadgetClosingHandler(gadget_GadgetClosing);
            //crsControl.GadgetProcessingFinished += new GadgetProcessingFinishedHandler(gadget_GadgetFinished);
            //EnableDisableOptions();
        }

        void mnuLinear_Click(object sender, RoutedEventArgs e)
        {
            LinearRegressionControl linearControl = new LinearRegressionControl(dashboardHelper);
            AddGadgetToCanvasFromContextMenu(linearControl);
        }

        void mnuCSMeans_Click(object sender, RoutedEventArgs e)
        {
            ComplexSampleMeansControl csMeansControl = new ComplexSampleMeansControl(dashboardHelper);
            AddGadgetToCanvasFromContextMenu(csMeansControl);
        }

        void mnuCSFrequency_Click(object sender, RoutedEventArgs e)
        {
            ComplexSampleFrequencyControl csFrequencyControl = new ComplexSampleFrequencyControl(dashboardHelper);
            AddGadgetToCanvasFromContextMenu(csFrequencyControl);
        }

        void mnuCSTables_Click(object sender, RoutedEventArgs e)
        {
            ComplexSampleTablesControl csTablesControl = new ComplexSampleTablesControl(dashboardHelper);
            AddGadgetToCanvasFromContextMenu(csTablesControl);
        }

        void mnuLogistic_Click(object sender, RoutedEventArgs e)
        {
            LogisticRegressionControl logisticControl = new LogisticRegressionControl(dashboardHelper);
            AddGadgetToCanvasFromContextMenu(logisticControl);
        }

        void AddFilterGadget()
        {
            if (dataFilteringControl != null)
            {
                canvasMain.Children.Remove(dataFilteringControl);
            }
            dataFilteringControl = new DataFilterControl(dashboardHelper);
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
            Canvas.SetTop(dataFilteringControl, scrollViewerAnalysis.VerticalOffset + (scrollViewerAnalysis.ActualHeight / 2.0) - (dataFilteringControl.ActualHeight / 2.0));
        }

        void dataExportControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Canvas.SetLeft(dataExportingControl, mousePoint.X - 10);
            //Canvas.SetTop(dataExportingControl, mousePoint.Y - 10);
            //Canvas.SetRight(dataExportingControl, scrollViewer.HorizontalOffset + (scrollViewer.ActualWidth / 2.0) - (dataExportingControl.ActualWidth / 2.0));
            //Canvas.SetTop(dataExportingControl, scrollViewer.VerticalOffset + (scrollViewer.ActualHeight / 2.0) - (dataExportingControl.ActualHeight / 2.0));
        }

        void propertiesControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Canvas.SetLeft(dataExportingControl, mousePoint.X - 10);
            //Canvas.SetTop(dataExportingControl, mousePoint.Y - 10);
            //Canvas.SetRight(propertiesControl, scrollViewer.HorizontalOffset + (scrollViewer.ActualWidth / 2.0) - (propertiesControl.ActualWidth / 2.0));
            //Canvas.SetTop(propertiesControl, scrollViewer.VerticalOffset + (scrollViewer.ActualHeight / 2.0) - (propertiesControl.ActualHeight / 2.0));
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
        }

        void AddDefinedVariablesGadget()
        {
            if (variablesControl != null)
            {
                canvasMain.Children.Remove(variablesControl);
            }
            variablesControl = new VariablesControl(dashboardHelper);
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
            Canvas.SetTop(variablesControl, scrollViewerAnalysis.VerticalOffset + (scrollViewerAnalysis.ActualHeight / 2.0) - (variablesControl.ActualHeight / 2.0));
        }

        void ExpandRecodingGadget()
        {
            DoubleAnimation anim = new DoubleAnimation();
            anim.From = Canvas.GetLeft(variablesControl);
            anim.To = -20;
            anim.AccelerationRatio = 0.8;
            anim.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            variablesControl.BeginAnimation(Canvas.LeftProperty, anim);
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
        }

        void dataRecodingControl_MouseLeave(object sender, MouseEventArgs e)
        {
            CollapseRecodingGadget();
        }

        void dataRecodingControl_MouseEnter(object sender, MouseEventArgs e)
        {
            ExpandRecodingGadget();
        }

        void selectionCriteriaControl_SelectionCriteriaChanged(object sender, EventArgs e)
        {
            //dashboardHelper.UserVarsNeedUpdating = true;
            RefreshResults();
            //txtRecordCount.Text = "...";
            UpdateRecordCount();

            recordCountWorker = new System.ComponentModel.BackgroundWorker();
            recordCountWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(recordCountWorker_DoWork);
            recordCountWorker.RunWorkerAsync();

            IsCanvasDirty = true;
            if (CanvasChanged != null)
            {
                CanvasChanged(this.CurrentCanvas);
            }
        }

        void dataRecodingControl_UserVariableAdded(object sender, EventArgs e)
        {
            dashboardHelper.UserVarsNeedUpdating = true;
            foreach (UserControl uc in this.Gadgets)
            {
                if (uc is IGadget)
                {
                    ((IGadget)uc).UpdateVariableNames();
                }
            }

            //foreach (UIElement uc in this.canvasMain.Children)
            //{
            //    if (uc is DataDictionaryControl)
            //    {
            //        ((DataDictionaryControl)uc).UpdateVariableNames();
            //    }
            //    else if (uc is ExportControl)
            //    {
            //        ((ExportControl)uc).UpdateVariableNames();
            //    }
            //}

            dataFilteringControl.FillSelectionComboboxes(true);

            RefreshResults(); // normally wouldn't need this here, but now that we can filter on user-defined variables, it's necessary

            string fieldCounter = string.Format(DashboardSharedStrings.FIELD_COUNT, dashboardHelper.TableColumnNames.Count.ToString());
            tblockFieldCount.Text = fieldCounter;

            IsCanvasDirty = true;
            if (CanvasChanged != null)
            {
                CanvasChanged(this.CurrentCanvas);
            }
        }

        void dataRecodingControl_UserVariableRemoved(object sender, EventArgs e)
        {
            dashboardHelper.UserVarsNeedUpdating = true;
            foreach (UserControl uc in this.Gadgets)
            {
                if (uc is IGadget)
                {
                    ((IGadget)uc).UpdateVariableNames();
                }
            }

            dataFilteringControl.FillSelectionComboboxes(true);

            RefreshResults();

            string fieldCounter = string.Format(DashboardSharedStrings.FIELD_COUNT, dashboardHelper.TableColumnNames.Count.ToString());
            tblockFieldCount.Text = fieldCounter;

            IsCanvasDirty = true;
            if (CanvasChanged != null)
            {
                CanvasChanged(this.CurrentCanvas);
            }
        }

        void dataRecodingControl_UserVariableChanged(object sender, EventArgs e)
        {
            dashboardHelper.UserVarsNeedUpdating = true;
            foreach (UserControl uc in this.Gadgets)
            {
                if (uc is IGadget)
                {
                    ((IGadget)uc).UpdateVariableNames();
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
            dashboardHelper.GenerateRecordCount(true);
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
            if (this.dashboardHelper == null)
            {
                Epi.WPF.Dashboard.Dialogs.MsgBox.ShowInformation(DashboardSharedStrings.CANNOT_SAVE_CANVAS_NO_DATA_SOURCE);
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
                if (dashboardHelper.IsUsingEpiProject)
                {
                    dlg.FileName = dashboardHelper.View.Name + "Output.html";
                }
                else
                {
                    dlg.FileName = dashboardHelper.TableName + "Output.html";
                }

                if (Directory.Exists(dashboardHelper.Config.Directories.Output))
                {
                    dlg.InitialDirectory = dashboardHelper.Config.Directories.Output;
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

                    textWriter.Write(this.ToHTML(dlg.SafeFileName));

                    textWriter.Dispose();
                    textWriter.Close();

                    if (showSaveConfirmation)
                    {
                        Epi.WPF.Dashboard.Dialogs.MsgBox.ShowInformation(DashboardSharedStrings.OUTPUT_SAVED);
                    }
                    lastSavedOutputFileName = dlg.FileName;
                    return true;
                }
                catch (Exception ex)
                {
                    Epi.WPF.Dashboard.Dialogs.MsgBox.ShowException(ex);
                }
            }

            return false;
        }

        public void SaveOutputAsHTMLAndDisplay()
        {
            if (this.dashboardHelper == null)
            {
                Epi.WPF.Dashboard.Dialogs.MsgBox.ShowInformation(DashboardSharedStrings.CANNOT_SAVE_CANVAS_NO_DATA_SOURCE);
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
            if (this.dashboardHelper == null)
            {
                Epi.WPF.Dashboard.Dialogs.MsgBox.ShowInformation(DashboardSharedStrings.CANNOT_SAVE_CANVAS_NO_DATA_SOURCE);
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
            else if (dashboardHelper.IsUsingEpiProject && File.Exists(dashboardHelper.View.Project.FilePath))
            {
                FileInfo fileInfo = new FileInfo(dashboardHelper.View.Project.FilePath);
                dlg.InitialDirectory = fileInfo.DirectoryName;
            }

            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            //if (dlg.ShowDialog().Value)
            {
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                System.Xml.XmlElement root = doc.CreateElement("DashboardCanvas");

                root.AppendChild(dashboardHelper.Serialize(doc));
                root.AppendChild(this.SerializeGadgets(doc));
                root.AppendChild(this.SerializeOutputSettings(doc));

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
                    this.currentCanvas = dlg.FileName;
                    IsCanvasDirty = false;

                    if (CanvasChanged != null)
                    {
                        CanvasChanged(lastSavedCanvasFileName);
                    }

                    return true;
                }
                catch (IOException)
                {
                    Epi.WPF.Dashboard.Dialogs.MsgBox.ShowInformation(DashboardSharedStrings.ERROR_CANVAS_SAVE_IO_EXCEPTION);
                }
            }

            return false;
        }

        public bool SaveCanvas()
        {
            if (this.dashboardHelper == null)
            {
                Epi.WPF.Dashboard.Dialogs.MsgBox.ShowInformation(DashboardSharedStrings.CANNOT_SAVE_CANVAS_NO_DATA_SOURCE);
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

            root.AppendChild(dashboardHelper.Serialize(doc));
            root.AppendChild(this.SerializeGadgets(doc));
            root.AppendChild(this.SerializeOutputSettings(doc));

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
                this.currentCanvas = fileToSave;
                IsCanvasDirty = false;

                if (CanvasChanged != null)
                {
                    CanvasChanged(lastSavedCanvasFileName);
                }

                return true;
            }
            catch (IOException)
            {
                Epi.WPF.Dashboard.Dialogs.MsgBox.ShowInformation(DashboardSharedStrings.ERROR_CANVAS_SAVE_IO_EXCEPTION);
            }

            return false;
        }

        private void CreateFromXml(string fileName)
        {            
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.Load(fileName);

            bool isLegacyCanvas = false;
            if (doc.InnerXml.Contains("EpiDashboard."))
            {
                isLegacyCanvas = true;
                doc.InnerXml = doc.InnerXml.Replace("EpiDashboard.", "Epi.WPF.Dashboard.");
            }

            foreach (System.Xml.XmlElement element in doc.DocumentElement.ChildNodes)
            {
                if (element.Name.ToLower().Equals("dashboardhelper"))
                {
                    DashboardHelper helper = new DashboardHelper();
                    helper.NotificationEvent += new NotificationEventHandler(dashboardHelper_NotificationEvent);

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
                                Epi.WPF.Dashboard.Dialogs.MsgBox.ShowError(message);

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
                                Epi.WPF.Dashboard.Dialogs.MsgBox.ShowError(DashboardSharedStrings.ERROR_CANNOT_DECRYPT);
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
                                    else if (!System.IO.File.Exists(dsPath))
                                    {
                                        string message = string.Format(DashboardSharedStrings.ERROR_CANVAS_DATA_SOURCE_NOT_FOUND, dsPath);
                                        Epi.WPF.Dashboard.Dialogs.MsgBox.ShowError(message);
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
                        Epi.WPF.Dashboard.Dialogs.MsgBox.ShowError(ex.Message);
                        return;
                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        Epi.WPF.Dashboard.Dialogs.MsgBox.ShowError(ex.Message);
                        return;
                    }
                    catch (ApplicationException ex)
                    {
                        string message = ex.Message;
                        if (message.ToLower().Equals("error executing select query against the database."))
                        {
                            message = DashboardSharedStrings.ERROR_DATA_SOURCE_PERMISSIONS;
                        }
                        Epi.WPF.Dashboard.Dialogs.MsgBox.ShowError(message);
                        return;
                    }
                    catch (System.Security.Cryptography.CryptographicException ex)
                    {
                        Epi.WPF.Dashboard.Dialogs.MsgBox.ShowError(string.Format(SharedStrings.ERROR_CRYPTO_KEYS, ex.Message));
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
                            Epi.WPF.Dashboard.Dialogs.MsgBox.ShowError(ex.Message);
                            return;
                        }
                    }

                    this.dashboardHelper = helper;
                    this.dashboardHelper.SetDashboardControl(this);

                    canvasMain.Height = DEFAULT_CANVAS_HEIGHT;

                    RemoveAllGadgets();
                    EnableDisableMenus(true);
                    AddFilterGadget();
                    AddDefinedVariablesGadget();
                    ResetCanvasProperties();

                    this.currentCanvas = fileName;

                    //dashboardHelper.GenerateRecordCount(true);
                    UpdateRecordCount();
                    ReCacheDataSource();
                }
                if (element.Name.ToLower().Equals("gadgets"))
                {
                    foreach (XmlElement child in element.ChildNodes)
                    {
                        try
                        {                            
                            string value = child.Attributes["gadgetType"].Value;

                            if (isLegacyCanvas)
                            {
                                if (value.Contains("Standard"))
                                {
                                    value = value.Replace("Epi.WPF.Dashboard.", "Epi.WPF.Dashboard.Gadgets.Reporting.");
                                }
                                else
                                {
                                    value = value.Replace("Epi.WPF.Dashboard.", "Epi.WPF.Dashboard.Gadgets.Analysis.");
                                }
                            }

                            Type gadgetType = Type.GetType(value);

                            // The 2x2 gadget was removed and its functionality absorbed into the MxN (crosstab) gadget.
                            // This code, along with updates to the CreateFromXml routine in the Crosstab gadget, ensures
                            // that 2x2 gadgets created in canvas files from versions 7.0.9.61 and prior will still be
                            // loaded without issue. They will now be loaded as Crosstab gadgets instead.
                            if (child.Attributes["gadgetType"].Value.Equals("Epi.WPF.Dashboard.Gadgets.Analysis.TwoByTwoTableControl"))
                            {
                                gadgetType = Type.GetType("Epi.WPF.Dashboard.Gadgets.Analysis.CrosstabControl");
                            }

                            IGadget gadget = (IGadget)Activator.CreateInstance(gadgetType, new object[] { dashboardHelper });
                            gadget.CreateFromXml(child);
                            AddGadgetToCanvasFromFile(gadget);
                        }
                        catch (NullReferenceException)
                        {
                            Epi.WPF.Dashboard.Dialogs.MsgBox.ShowError(DashboardSharedStrings.GADGET_LOAD_ERROR);
                            return;
                        }
                    }
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

                        //mainTabControl.Visibility = Visibility.Visible;
                        //mainTabRow.Height = new GridLength(27);

                        dataDictionary.SetDataView(this.dashboardHelper.GenerateDataDictionaryTable().DefaultView);
                        dataDictionary.Refresh();

                        dataDisplay.SetDataView(this.dashboardHelper.GenerateView());
                        dataDisplay.Refresh();
                    }
                    catch (Exception ex)
                    {
                        Epi.WPF.Dashboard.Dialogs.MsgBox.ShowException(ex);
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
                CanvasChanged(currentCanvas);
            }

            this.scrollViewerAnalysis.ScrollToTop();            
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
            //foreach (UserControl control in this.Gadgets)
            //{
            //    if (control is EpiDashboard.Gadgets.Reporting.StandardTextControl)
            //    {
            //        BackgroundWorker adornerWorker = new BackgroundWorker();
            //        adornerWorker.DoWork += new DoWorkEventHandler(adornerWorker_DoWork);
            //        adornerWorker.RunWorkerAsync(control);
            //    }
            //}
        }

        void adornerWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //if (e.Argument != null && e.Argument is StandardTextControl)
            //{
            //    this.Dispatcher.BeginInvoke(new GadgetSetAdornerHandler(SetAdorner), e.Argument as Control);
            //}
        }

        private void SetAdorner(Control control)
        {
            //if (control != null && control is StandardTextControl)
            //{
            //    System.Threading.Thread.Sleep(100);
            //    EpiDashboard.Gadgets.Reporting.StandardTextControl stc = (control as EpiDashboard.Gadgets.Reporting.StandardTextControl);
            //    AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(stc);
            //    adornerLayer.Add(new ResizingAdorner(stc));
            //}
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
                Epi.WPF.Dashboard.Dialogs.MsgBox.ShowException(ex);
                return;
            }

            CreateFromXml(filePath);
        }

        public XmlNode SerializeGadgets(XmlDocument doc)
        {
            System.Xml.XmlElement root = doc.CreateElement("Gadgets");
            foreach (IGadget gadget in this.gadgets)
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

        public XmlNode SerializeOutputSettings(XmlDocument doc)
        {
            System.Xml.XmlElement root = doc.CreateElement("OutputSettings");

            string xmlString =
           "<showCanvasSummaryInfo>" + ShowCanvasSummaryInfoInOutput + "</showCanvasSummaryInfo>" +
           "<showGadgetHeadings>" + ShowGadgetHeadingsInOutput + "</showGadgetHeadings>" +
           "<showGadgetSettings>" + ShowGadgetSettingsInOutput + "</showGadgetSettings>" +
           "<useAlternatingColors>" + UseAlternatingColorsInOutput + "</useAlternatingColors>" +
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

                foreach (UserControl control in this.gadgets)
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
                foreach (UserControl control in this.gadgets)
                {
                    if (control is IGadget && ((IGadget)control).IsProcessing == false)
                    {
                        IGadget gadget = control as IGadget;
                        htmlBuilder.Append(gadget.ToHTML(htmlFileName, count));
                        htmlBuilder.AppendLine("");
                        htmlBuilder.AppendLine("<p>&nbsp;</p>");
                        count++;
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

        //private void iconRefresh_MouseEnter(object sender, MouseEventArgs e)
        //{
        //    whiteUnderlineRefresh.Visibility = System.Windows.Visibility.Visible;
        //    this.Cursor = Cursors.Hand;
        //}

        //private void iconRefresh_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    whiteUnderlineRefresh.Visibility = System.Windows.Visibility.Collapsed;
        //    this.Cursor = Cursors.Arrow;
        //}

        private void iconRefresh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (dashboardHelper != null)
            {
                dashboardHelper.UserVarsNeedUpdating = true;
                ReCacheDataSource(false);
            }
        }

        private void iconRefresh_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        //private void tblockSetDataSource_MouseEnter(object sender, MouseEventArgs e)
        //{
        //    whiteUnderlineSetData.Visibility = System.Windows.Visibility.Visible;
        //    this.Cursor = Cursors.Hand;
        //}

        //private void tblockSetDataSource_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    whiteUnderlineSetData.Visibility = System.Windows.Visibility.Collapsed;
        //    this.Cursor = Cursors.Arrow;
        //}

        private void tblockSetDataSource_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void tblockSetDataSource_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsPopulatingDataSet)
            {
                SetDataSource();
            }
        }


        //private void tblockOpen_MouseEnter(object sender, MouseEventArgs e)
        //{
        //    whiteUnderlineOpen.Visibility = System.Windows.Visibility.Visible;
        //    this.Cursor = Cursors.Hand;
        //}

        //private void tblockOpen_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    whiteUnderlineOpen.Visibility = System.Windows.Visibility.Collapsed;
        //    this.Cursor = Cursors.Arrow;
        //}

        private void tblockOpen_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void tblockOpen_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //svAvailableCanvasFiles.ScrollToTop();

            //int column = Grid.GetColumn(tblockOpen);
            //double width = 0;
            //for (int i = 0; i < column; i++)
            //{
            //    width = width + grdMenuBarInner.ColumnDefinitions[i].ActualWidth;
            //}

            //if (pathDataTriangle.Visibility == System.Windows.Visibility.Visible &&
            //    introAvailableData.Visibility == System.Windows.Visibility.Visible)
            //{
            //    introAvailableData.Visibility = Visibility.Collapsed;
            //}
            //else
            //{
            //    LoadQuickDataSources();

            //    Canvas.SetLeft(introAvailableData, width);
            //    Canvas.SetTop(introAvailableData, 24 + scrollViewerAnalysis.VerticalOffset);
            //    DragCanvas.SetCanBeDragged(introAvailableData, false);
            //    pathDataTriangle.Visibility = System.Windows.Visibility.Visible;
            //    introAvailableData.Visibility = System.Windows.Visibility.Visible;
            //    Canvas.SetZIndex(introAvailableData, 4000);

            //    if (this.ActualHeight <= 600)
            //    {
            //        introAvailableData.MaxHeight = 550;
            //    }
            //    else if (this.ActualHeight <= 770)
            //    {
            //        introAvailableData.MaxHeight = 700;
            //    }
            //}
        }



        //private void tblockSave_MouseEnter(object sender, MouseEventArgs e)
        //{
        //    whiteUnderlineSave.Visibility = System.Windows.Visibility.Visible;
        //    this.Cursor = Cursors.Hand;
        //}

        //private void tblockSave_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    whiteUnderlineSave.Visibility = System.Windows.Visibility.Collapsed;
        //    this.Cursor = Cursors.Arrow;
        //}

        private void tblockSave_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void tblockSave_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (dashboardHelper != null)
            {
                SaveCanvas();
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

        private void tblockSaveAs_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void tblockSaveAs_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (dashboardHelper != null)
            {
                SaveCanvasAs();
            }
        }

        public bool CanCloseDashboard()
        {
            if (dashboardHelper != null && this.IsCanvasDirty)
            {
                System.Windows.Forms.DialogResult result = System.Windows.Forms.DialogResult.No;

                if (string.IsNullOrEmpty(lastSavedCanvasFileName))
                {
                    result = Epi.WPF.Dashboard.Dialogs.MsgBox.ShowQuestion(DashboardSharedStrings.SAVE_CHANGES, System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                }
                else
                {
                    string message = string.Format(DashboardSharedStrings.SAVE_CHANGES_TO_FILE, lastSavedCanvasFileName);
                    result = Epi.WPF.Dashboard.Dialogs.MsgBox.ShowQuestion(message, System.Windows.Forms.MessageBoxButtons.YesNoCancel);
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

        private void panelAddSpace_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void panelAddSpace_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.canvasMain.Height = this.canvasMain.Height + 2000;
        }
    }
}