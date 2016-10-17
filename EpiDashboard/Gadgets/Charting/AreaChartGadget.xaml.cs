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
    /// Interaction logic for AreaChartGadget.xaml
    /// </summary>
    public partial class AreaChartGadget : ChartGadgetBase
    {
        #region Constructors

        public AreaChartGadget()
        {
            InitializeComponent();
            Construct();
        }

        public AreaChartGadget(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            Construct();
        }

        #endregion //Constructors

        #region Private and Protected Methods

        /// <summary>
        /// A custom heading to use for this gadget's output
        /// </summary>
        private string customOutputHeading;

        /// <summary>
        /// A custom description to use for this gadget's output
        /// </summary>
        private string customOutputDescription;

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            if (LoadingCombos)
            {
                return;
            }

            RefreshResults();
        }

        protected override void Construct()
        {
            this.Parameters = new AreaChartParameters();

            if (!string.IsNullOrEmpty(CustomOutputHeading) && !CustomOutputHeading.Equals("(none)"))
            {
                headerPanel.Text = CustomOutputHeading;
            }

            StrataGridList = new List<Grid>();
            StrataExpanderList = new List<Expander>();

            mnuCopy.Click += new RoutedEventHandler(mnuCopy_Click);
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

            base.Construct();
        }

        private void ClearResults()
        {
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Text = string.Empty;
            descriptionPanel.PanelMode = EpiDashboard.Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;

            panelMain.Children.Clear();

            StrataGridList.Clear();
            StrataExpanderList.Clear();
        }

        protected override void RenderFinish()
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.StatusPanel;
            messagePanel.Text = string.Empty;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        protected override void RenderFinishWithWarning(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed; //waitCursor.Visibility = Visibility.Hidden;

            messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.WarningPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            HideConfigPanel();
            CheckAndSetPosition();
        }

        protected override void RenderFinishWithError(string errorMessage)
        {
            waitPanel.Visibility = System.Windows.Visibility.Collapsed;

            messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.ErrorPanel;
            messagePanel.Text = errorMessage;
            messagePanel.Visibility = System.Windows.Visibility.Visible;

            panelMain.Children.Clear();

            HideConfigPanel();
            CheckAndSetPosition();
        }

        protected override void SetChartData(List<XYColumnChartData> dataList, Strata strata)
        {
            #region Input Validation
            if (dataList == null)
            {
                throw new ArgumentNullException("dataList");
            }
            #endregion // Input Validation
            AreaChartParameters areaParameters = (AreaChartParameters)Parameters;

            foreach (var item in dataList)
            {
                double val = 0.0;
                try
                {
                    val = Convert.ToDouble(item.S);
                }
                catch (Exception)
                {

                }

            }


            if (dataList.Count > 0)
            {
                if (strata != null)
                {
                    areaParameters.ChartStrataTitle = strata.Filter;
                }
                else
                {
                    areaParameters.ChartStrataTitle = String.Empty;
                }
                EpiDashboard.Controls.Charting.AreaChart areaChart = new EpiDashboard.Controls.Charting.AreaChart(DashboardHelper, areaParameters, dataList);
                areaChart.Margin = new Thickness(0, 0, 0, 16);
                areaChart.MouseEnter += new MouseEventHandler(chart_MouseEnter);
                areaChart.MouseLeave += new MouseEventHandler(chart_MouseLeave);
                panelMain.Children.Add(areaChart);
            }
        }

        #endregion // Private and Protected Methods

        #region Public Methods

        public override void SetGadgetToProcessingState()
        {
            this.IsProcessing = true;
            base.SetGadgetToProcessingState();
        }

        public override void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
            base.SetGadgetToFinishedState();
        }

        public override void RefreshResults()
        {
            AreaChartParameters areaParameters = (AreaChartParameters)Parameters;
            if (!LoadingCombos)
            {
                if (areaParameters != null)
                {
                    if (areaParameters.ColumnNames.Count > 0 && !String.IsNullOrEmpty(areaParameters.ColumnNames[0]))
                    {
                        infoPanel.Visibility = System.Windows.Visibility.Collapsed;
                        waitPanel.Visibility = System.Windows.Visibility.Visible;
                        messagePanel.MessagePanelType = EpiDashboard.Controls.MessagePanelType.StatusPanel;
                        descriptionPanel.PanelMode = EpiDashboard.Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;
                        baseWorker = new BackgroundWorker();
                        baseWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
                        baseWorker.RunWorkerAsync();
                        base.RefreshResults();
                    }
                    else
                    {
                        ClearResults();
                        waitPanel.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
            }
        }

        public override void CreateFromXml(XmlElement element)
        {
            this.LoadingCombos = true;
            this.Parameters = new AreaChartParameters();

            HideConfigPanel();
            infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            foreach (XmlElement child in element.ChildNodes)
            {
                if (!String.IsNullOrEmpty(child.InnerText))
                {
                    switch (child.Name.ToLowerInvariant())
                    {
                        case "mainvariable":
                            if (this.Parameters.ColumnNames.Count > 0)
                            {
                                ((AreaChartParameters)Parameters).ColumnNames[0] = (child.InnerText.Replace("&lt;", "<"));
                            }
                            else
                            {
                                ((AreaChartParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                            }
                            break;
                        case "stratavariable":
                            if (!string.IsNullOrEmpty(child.InnerText))
                            {
                                if (((AreaChartParameters)Parameters).StrataVariableNames.Count > 0)
                                {
                                    ((AreaChartParameters)Parameters).StrataVariableNames[0] = child.InnerText.Replace("&lt;", "<");
                                }
                                else
                                {
                                    ((AreaChartParameters)Parameters).StrataVariableNames.Add(child.InnerText.Replace("&lt;", "<"));
                                }
                            }
                            break;
                        case "stratavariables":
                            foreach (XmlElement field in child.ChildNodes)
                            {
                                List<string> fields = new List<string>();
                                if (field.Name.ToLowerInvariant().Equals("stratavariable"))
                                {
                                    ((AreaChartParameters)Parameters).StrataVariableNames.Add(field.InnerText.Replace("&lt;", "<"));
                                }
                            }
                            break;
                        case "weightvariable":
                            ((AreaChartParameters)Parameters).WeightVariableName = child.InnerText.Replace("&lt;", "<");
                            break;
                        case "crosstabvariable":
                            ((AreaChartParameters)Parameters).CrosstabVariableName = child.InnerText.Replace("&lt;", "<");
                            break;
                        case "secondyvar":
                            if (this.Parameters.ColumnNames.Count > 1)
                            {
                                ((AreaChartParameters)Parameters).ColumnNames[1] = (child.InnerText.Replace("&lt;", "<"));
                            }
                            else
                            {
                                ((AreaChartParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                            }
                            break;
                        case "secondyvartype":
//                            Y2Type y2Type = ((Y2Type)Int32.Parse(child.InnerText));
//                            ((AreaChartParameters)Parameters).Y2AxisType = int.Parse(child.InnerText.Replace("&lt;", "<"));
                            Y2Type y2Type = ((Y2Type)Int32.Parse("0"));
                            ((AreaChartParameters)Parameters).Y2AxisType = int.Parse("0");
                            break;
                        case "sort":
                            if (child.InnerText.ToLowerInvariant().Equals("highlow") || child.InnerText.ToLowerInvariant().Equals("hightolow"))
                            {
                                ((AreaChartParameters)Parameters).SortHighToLow = true;
                            }
                            break;
                        case "allvalues":
                            if (child.InnerText.ToLowerInvariant().Equals("true"))
                            {
                                ((AreaChartParameters)Parameters).ShowAllListValues = true;
                            }
                            else { ((AreaChartParameters)Parameters).ShowAllListValues = false; }
                            break;
                        case "showlistlabels":
                            if (child.InnerText.ToLowerInvariant().Equals("true"))
                            {
                                ((AreaChartParameters)Parameters).ShowCommentLegalLabels = true;
                            }
                            else { ((AreaChartParameters)Parameters).ShowCommentLegalLabels = false; }
                            break;
                        case "includemissing":
                            if (child.InnerText.ToLowerInvariant().Equals("true")) { ((AreaChartParameters)Parameters).IncludeMissing = true; }
                            else { ((AreaChartParameters)Parameters).IncludeMissing = false; }
                            break;
                        case "customheading":
                            if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
                            {
                                this.Parameters.GadgetTitle = child.InnerText.Replace("&lt;", "<");
                                this.CustomOutputHeading = child.InnerText.Replace("&lt;", "<");
                            }
                            else
                            {
                                this.CustomOutputHeading = string.Empty;
                                this.Parameters.GadgetTitle = string.Empty;
                            }
                            break;
                        case "customdescription":
                            if (!string.IsNullOrEmpty(child.InnerText) && !child.InnerText.Equals("(none)"))
                            {
                                this.CustomOutputDescription = child.InnerText.Replace("&lt;", "<");
                                this.Parameters.GadgetDescription = child.InnerText.Replace("&lt;", "<");
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
                            if (child.InnerText.ToLowerInvariant().Equals("true")) { ((AreaChartParameters)Parameters).UseRefValues = true; }
                            else { ((AreaChartParameters)Parameters).UseRefValues = false; }
                            break;
                        case "showannotations":
                            if (child.InnerText.ToLowerInvariant().Equals("true")) { ((AreaChartParameters)Parameters).ShowAnnotations = true; }
                            else { ((AreaChartParameters)Parameters).ShowAnnotations = false; }
                            break;
                        case "y2showannotations":
                            if (child.InnerText.ToLowerInvariant().Equals("true")) { ((AreaChartParameters)Parameters).Y2ShowAnnotations = true; }
                            else { ((AreaChartParameters)Parameters).Y2ShowAnnotations = false; }
                            break;
                        case "composition":
                            {
                                switch (child.InnerText)
                                {
                                    case "0":
                                        ((AreaChartParameters)Parameters).Composition = CompositionKind.SideBySide;
                                        break;
                                    case "1":
                                        ((AreaChartParameters)Parameters).Composition = CompositionKind.Stacked;
                                        break;
                                    case "2":
                                        ((AreaChartParameters)Parameters).Composition = CompositionKind.Stacked100;
                                        break;
                                }
                            }
                            break;
                        case "showseriesline":
                            if (child.InnerText.ToLowerInvariant().Equals("true")) { ((AreaChartParameters)Parameters).ShowSeriesLine = true; }
                            else { ((AreaChartParameters)Parameters).ShowSeriesLine = false; }
                            break;
                        case "showgridlines":
                            if (child.InnerText.ToLowerInvariant().Equals("true")) { ((AreaChartParameters)Parameters).ShowGridLines = true; }
                            else { ((AreaChartParameters)Parameters).ShowGridLines = false; }
                            break;
                        case "palette":
                            ((AreaChartParameters)Parameters).Palette = int.Parse(child.InnerText);
                            break;
                        case "palettecolor1":
                            ((AreaChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                            break;
                        case "palettecolor2":
                            ((AreaChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                            break;
                        case "palettecolor3":
                            ((AreaChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                            break;
                        case "palettecolor4":
                            ((AreaChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                            break;
                        case "palettecolor5":
                            ((AreaChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                            break;
                        case "palettecolor6":
                            ((AreaChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                            break;
                        case "palettecolor7":
                            ((AreaChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                            break;
                        case "palettecolor8":
                            ((AreaChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                            break;
                        case "palettecolor9":
                            ((AreaChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                            break;
                        case "palettecolor10":
                            ((AreaChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                            break;
                        case "palettecolor11":
                            ((AreaChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                            break;
                        case "palettecolor12":
                            ((AreaChartParameters)Parameters).PaletteColors.Add(child.InnerText);
                            break;
                        case "areatype":
                            {
                                switch (child.InnerText)
                                {
                                    case "Polygon":
                                    case "1":
                                        ((AreaChartParameters)Parameters).AreaKind = LineKind.Polygon;
                                        break;
                                    case "Smooth":
                                    case "2":
                                        ((AreaChartParameters)Parameters).AreaKind = LineKind.Smooth;
                                        break;
                                    case "Step":
                                    case "3":
                                        ((AreaChartParameters)Parameters).AreaKind = LineKind.Step;
                                        break;
                                    case "Auto":
                                    case "0":
                                        ((AreaChartParameters)Parameters).AreaKind = LineKind.Auto;
                                        break;
                                }
                            }
                            break;
                        case "y2linetype":
                            switch (child.InnerText)
                            {
                                case "Polygon":
                                    ((AreaChartParameters)Parameters).Y2LineKind = LineKind.Polygon;
                                    break;
                                case "Smooth":
                                    ((AreaChartParameters)Parameters).Y2LineKind = LineKind.Smooth;
                                    break;
                                case "Step":
                                    ((AreaChartParameters)Parameters).Y2LineKind = LineKind.Step;
                                    break;
                                case "Auto":
                                    ((AreaChartParameters)Parameters).Y2LineKind = LineKind.Auto;
                                    break;
                            }
                            break;
                        case "y2linedashstyle":
                            {
                                switch (child.InnerText)
                                {
                                    case "Solid":
                                        ((AreaChartParameters)Parameters).Y2LineDashStyle = LineDashStyle.Solid;
                                        break;
                                    case "Dash":
                                        ((AreaChartParameters)Parameters).Y2LineDashStyle = LineDashStyle.Dash;
                                        break;
                                    case "Dot":
                                        ((AreaChartParameters)Parameters).Y2LineDashStyle = LineDashStyle.Dot;
                                        break;
                                    case "DashDot":
                                        ((AreaChartParameters)Parameters).Y2LineDashStyle = LineDashStyle.DashDot;
                                        break;
                                    case "DashDotDot":
                                        ((AreaChartParameters)Parameters).Y2LineDashStyle = LineDashStyle.DashDotDot;
                                        break;
                                }
                                break;
                            }
                        case "y2linethickness":
                            ((AreaChartParameters)Parameters).Y2LineThickness = int.Parse(child.InnerText);
                            break;
                        case "transtop":
                            ((AreaChartParameters)Parameters).TransTop = double.Parse(child.InnerText);
                            break;
                        case "transbottom":
                            ((AreaChartParameters)Parameters).TransBottom = double.Parse(child.InnerText);
                            break;
                        case "legenddock":
                            {
                                switch (int.Parse(child.InnerText))
                                {
                                    case 0:
                                        ((AreaChartParameters)Parameters).LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Left;
                                        break;
                                    default:
                                    case 1:
                                        ((AreaChartParameters)Parameters).LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Right;
                                        break;
                                    case 2:
                                        ((AreaChartParameters)Parameters).LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Top;
                                        break;
                                    case 3:
                                        ((AreaChartParameters)Parameters).LegendDock = ComponentArt.Win.DataVisualization.Charting.Dock.Bottom;
                                        break;
                                }
                            }
                            break;
                        case "yaxislabel":
                            ((AreaChartParameters)Parameters).YAxisLabel = child.InnerText;
                            break;
                        case "yaxisformatstring":
                            ((AreaChartParameters)Parameters).YAxisFormat = child.InnerText;
                            break;
                        case "y2axisformatstring":
                            ((AreaChartParameters)Parameters).Y2AxisFormat = child.InnerText;
                            break;
                        case "y2axislabel":
                            ((AreaChartParameters)Parameters).Y2AxisLabel = child.InnerText;
                            break;
                        case "y2axislegendtitle":
                            ((AreaChartParameters)Parameters).Y2AxisLegendTitle = child.InnerText;
                            break;
                        case "xaxislabeltype":
                            {
                                ((AreaChartParameters)Parameters).XAxisLabelType = int.Parse(child.InnerText);
                                break;
                            }
                        case "xaxislabel":
                            ((AreaChartParameters)Parameters).XAxisLabel = child.InnerText;
                            break;
                        case "xaxisangle":
                            ((AreaChartParameters)Parameters).XAxisAngle = int.Parse(child.InnerText);
                            break;
                        case "charttitle":
                            ((AreaChartParameters)Parameters).ChartTitle = child.InnerText;
                            break;
                        case "chartsubtitle":
                            ((AreaChartParameters)Parameters).ChartSubTitle = child.InnerText;
                            break;
                        case "showlegend":
                            if (child.InnerText.ToLowerInvariant().Equals("true")) { ((AreaChartParameters)Parameters).ShowLegend = true; }
                            else { ((AreaChartParameters)Parameters).ShowLegend = false; }
                            break;
                        case "showlegendborder":
                            if (child.InnerText.ToLowerInvariant().Equals("true")) { ((AreaChartParameters)Parameters).ShowLegendBorder = true; }
                            else { ((AreaChartParameters)Parameters).ShowLegendBorder = false; }
                            break;
                        case "showlegendvarnames":
                            if (child.InnerText.ToLowerInvariant().Equals("true")) { ((AreaChartParameters)Parameters).ShowLegendVarNames = true; }
                            else { ((AreaChartParameters)Parameters).ShowLegendVarNames = false; }
                            break;
                        case "legendfontsize":
                            ((AreaChartParameters)Parameters).LegendFontSize = int.Parse(child.InnerText);
                            break;
                        case "height":
                            ((AreaChartParameters)Parameters).ChartHeight = double.Parse(child.InnerText);
                            break;
                        case "width":
                            ((AreaChartParameters)Parameters).ChartWidth = double.Parse(child.InnerText);
                            break;
                        //EI-98
                        case "yaxislabelfontsize":
                            ((AreaChartParameters)Parameters).YAxisLabelFontSize = double.Parse(child.InnerText);
                            break;
                        case "xaxislabelfontsize":
                            ((AreaChartParameters)Parameters).XAxisLabelFontSize = double.Parse(child.InnerText);
                            break;
                        case "yaxisfontsize":
                            ((AreaChartParameters)Parameters).YAxisFontSize = double.Parse(child.InnerText);
                            break;
                        case "xaxisfontsize":
                            ((AreaChartParameters)Parameters).XAxisFontSize = double.Parse(child.InnerText);
                            break;
                    }
                }
            }

            base.CreateFromXml(element);

            this.LoadingCombos = false;
            RefreshResults();
            HideConfigPanel();
        }

        Controls.GadgetProperties.AreaChartProperties properties = null;
        public override void ShowHideConfigPanel()
        {
            Popup = new DashboardPopup();
            Popup.Parent = ((this.Parent as DragCanvas).Parent as ScrollViewer).Parent as Grid;
            properties = new Controls.GadgetProperties.AreaChartProperties(this.DashboardHelper, this, (AreaChartParameters)Parameters, StrataGridList);

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
                properties = new Controls.GadgetProperties.AreaChartProperties(this.DashboardHelper, this, (AreaChartParameters)Parameters, StrataGridList);
            properties.Height = (Convert.ToInt32(i_StandardHeight * f_HeightRatio)) / 1.07;
            properties.Width = (Convert.ToInt32(i_StandardWidth * f_WidthRatio)) / 1.07;

        }
        /// <summary>
        /// Serializes the gadget into Xml
        /// </summary>
        /// <param name="doc">The Xml docment</param>
        /// <returns>XmlNode</returns>
        public override XmlNode Serialize(XmlDocument doc)
        {
            AreaChartParameters chtParameters = (AreaChartParameters)Parameters;

            System.Xml.XmlElement element = doc.CreateElement("areaChartGadget");
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
            collapsed.Value = "false"; // currently no way to collapse the gadget, so leave this 'false' for now
            type.Value = "EpiDashboard.Gadgets.Charting.AreaChartGadget";
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

            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            string mainVar = string.Empty;
            string strataVar = string.Empty;
            string crosstabVar = string.Empty;
            string weightVar = string.Empty;
            string second_y_var = string.Empty;
            string sort = string.Empty;

            double height = 600;
            double width = 800;

            double.TryParse(chtParameters.ChartHeight.ToString(), out height);
            double.TryParse(chtParameters.ChartWidth.ToString(), out width);

            crosstabVar = chtParameters.CrosstabVariableName.Replace("<", "&lt;");

            //mainVariable
            XmlElement freqVarElement = doc.CreateElement("mainVariable");
            if (chtParameters.ColumnNames.Count > 0)
            {
                if (!String.IsNullOrEmpty(chtParameters.ColumnNames[0].ToString()))
                {
                    freqVarElement.InnerText = chtParameters.ColumnNames[0].ToString().Replace("<", "&lt;");
                    element.AppendChild(freqVarElement);
                }
            }
            // =========  Former Advanced Options section  ============
            //weightVariable
            XmlElement weightVariableElement = doc.CreateElement("weightVariable");
            if (!String.IsNullOrEmpty(chtParameters.WeightVariableName))
            {
                weightVariableElement.InnerText = chtParameters.WeightVariableName.Replace("<", "&lt;");
                element.AppendChild(weightVariableElement);
            }

            //Cross Tab Variable
            XmlElement crossTabVarElement = doc.CreateElement("crosstabVariable");
            if (!String.IsNullOrEmpty(chtParameters.CrosstabVariableName))
            {
                crossTabVarElement.InnerText = chtParameters.CrosstabVariableName.Replace("<", "&lt;");
                element.AppendChild(crossTabVarElement);
            }

            //strataVariables
            XmlElement StrataVariableNameElement = doc.CreateElement("strataVariable");
            XmlElement StrataVariableNamesElement = doc.CreateElement("strataVariables");
            if (chtParameters.StrataVariableNames.Count == 1)
            {
                StrataVariableNameElement.InnerText = chtParameters.StrataVariableNames[0].ToString().Replace("<", "&lt;");
                element.AppendChild(StrataVariableNameElement);
            }
            else if (chtParameters.StrataVariableNames.Count > 1)
            {
                foreach (string strataColumn in chtParameters.StrataVariableNames)
                {
                    XmlElement strataElement = doc.CreateElement("strataVariable");
                    strataElement.InnerText = strataColumn.Replace("<", "&lt;");
                    StrataVariableNamesElement.AppendChild(strataElement);
                }

                element.AppendChild(StrataVariableNamesElement);
            }

            //secondYVarType
            XmlElement secondYVarTypeElement = doc.CreateElement("secondYVarType");
            secondYVarTypeElement.InnerText = chtParameters.Y2AxisType.ToString().Replace("<", "&lt;");
            element.AppendChild(secondYVarTypeElement);

            //secondYVar
            XmlElement secondYVarElement = doc.CreateElement("secondYVar");
            if (chtParameters.ColumnNames.Count > 1)
            {
                if (!String.IsNullOrEmpty(chtParameters.ColumnNames[1].ToString()))
                {
                    secondYVarElement.InnerText = chtParameters.ColumnNames[1].ToString().Replace("<", "&lt;");
                    element.AppendChild(secondYVarElement);
                }
            }

            //height 
            XmlElement heightElement = doc.CreateElement("height");
            heightElement.InnerText = chtParameters.ChartHeight.ToString().Replace("<", "&lt;");
            element.AppendChild(heightElement);

            //width 
            XmlElement widthElement = doc.CreateElement("width");
            widthElement.InnerText = chtParameters.ChartWidth.ToString().Replace("<", "&lt;");
            element.AppendChild(widthElement);

            //showAllListValues
            XmlElement allValuesElement = doc.CreateElement("allValues");
            allValuesElement.InnerText = chtParameters.ShowAllListValues.ToString().Replace("<", "&lt;");
            element.AppendChild(allValuesElement);

            //showListLabels
            XmlElement showListLabelsElement = doc.CreateElement("showListLabels");
            showListLabelsElement.InnerText = chtParameters.ShowCommentLegalLabels.ToString().Replace("<", "&lt;");
            element.AppendChild(showListLabelsElement);

            //sort
            XmlElement sortElement = doc.CreateElement("sort");
            if (chtParameters.SortHighToLow) sortElement.InnerText = "hightolow";
            element.AppendChild(sortElement);

            //includeMissing
            XmlElement includeMissingElement = doc.CreateElement("includeMissing");
            includeMissingElement.InnerText = chtParameters.IncludeMissing.ToString();
            element.AppendChild(includeMissingElement);

            //customHeading
            XmlElement customHeadingElement = doc.CreateElement("customHeading");
            customHeadingElement.InnerText = chtParameters.GadgetTitle.Replace("<", "&lt;");
            element.AppendChild(customHeadingElement);

            //customDescription
            XmlElement customDescriptionElement = doc.CreateElement("customDescription");
            customDescriptionElement.InnerText = chtParameters.GadgetDescription.Replace("<", "&lt;");
            element.AppendChild(customDescriptionElement);

            //customCaption
            XmlElement customCaptionElement = doc.CreateElement("customCaption");
            if (!String.IsNullOrEmpty(CustomOutputCaption))
            {
                customCaptionElement.InnerText = CustomOutputCaption.Replace("<", "&lt;");
            }
            else
            {
                customCaptionElement.InnerText = string.Empty;
            }
            element.AppendChild(customCaptionElement);

            //showSeriesLine
            XmlElement showSeriesLineElement = doc.CreateElement("showSeriesLine");
            showSeriesLineElement.InnerText = chtParameters.ShowSeriesLine.ToString();
            element.AppendChild(showSeriesLineElement);

            //useRefValues 
            XmlElement useRefValuesElement = doc.CreateElement("useRefValues");
            useRefValuesElement.InnerText = chtParameters.UseRefValues.ToString();
            element.AppendChild(useRefValuesElement);

            //showAnnotations 
            XmlElement showAnnotationsElement = doc.CreateElement("showAnnotations");
            showAnnotationsElement.InnerText = chtParameters.ShowAnnotations.ToString();
            element.AppendChild(showAnnotationsElement);

            //y2showAnnotations 
            XmlElement y2showAnnotationsElement = doc.CreateElement("y2showAnnotations");
            y2showAnnotationsElement.InnerText = chtParameters.Y2ShowAnnotations.ToString();
            element.AppendChild(y2showAnnotationsElement);

            //showGridLines 
            XmlElement showGridLinesElement = doc.CreateElement("showGridLines");
            showGridLinesElement.InnerText = chtParameters.ShowGridLines.ToString();
            element.AppendChild(showGridLinesElement);

            //composition 
            XmlElement compositionElement = doc.CreateElement("composition");
            switch (chtParameters.Composition.ToString())
            {
                case "SideBySide":
                    compositionElement.InnerText = "0";
                    break;
                case "Stacked":
                    compositionElement.InnerText = "1";
                    break;
                case "Stacked100":
                    compositionElement.InnerText = "2";
                    break;
            }
            element.AppendChild(compositionElement);

            //palette 
            XmlElement paletteElement = doc.CreateElement("palette");
            paletteElement.InnerText = chtParameters.Palette.ToString();
            element.AppendChild(paletteElement);
            //Palette Colors
            for (int i = 0; i < chtParameters.PaletteColors.Count(); i++)
            {
                XmlElement palettecolorsElement = doc.CreateElement("paletteColor" + (i + 1));
                palettecolorsElement.InnerText = chtParameters.PaletteColors[i].ToString();
                element.AppendChild(palettecolorsElement);
            }
            //areaType
            XmlElement areaTypeElement = doc.CreateElement("areaType");
            areaTypeElement.InnerText = chtParameters.AreaKind.ToString();
            element.AppendChild(areaTypeElement);

            //transTop
            XmlElement transTopElement = doc.CreateElement("transTop");
            transTopElement.InnerText = chtParameters.TransTop.ToString();
            element.AppendChild(transTopElement);

            //transBottom
            XmlElement transBottomElement = doc.CreateElement("transBottom");
            transBottomElement.InnerText = chtParameters.TransBottom.ToString();
            element.AppendChild(transBottomElement);

            //y2LineType 
            XmlElement y2LineTypeElement = doc.CreateElement("y2LineType");
            y2LineTypeElement.InnerText = chtParameters.Y2LineKind.ToString();
            element.AppendChild(y2LineTypeElement);

            //y2LineDashStyle 
            XmlElement y2LineDashStyleElement = doc.CreateElement("y2LineDashStyle");
            y2LineDashStyleElement.InnerText = chtParameters.Y2LineDashStyle.ToString();
            element.AppendChild(y2LineDashStyleElement);

            //y2LineThickness 
            XmlElement y2LineThicknessElement = doc.CreateElement("y2LineThickness");
            y2LineThicknessElement.InnerText = chtParameters.Y2LineThickness.ToString();
            element.AppendChild(y2LineThicknessElement);

            //yAxisLabel 
            XmlElement yAxisLabelElement = doc.CreateElement("yAxisLabel");
            yAxisLabelElement.InnerText = chtParameters.YAxisLabel.ToString().Replace("<", "&lt;");
            element.AppendChild(yAxisLabelElement);

            //yAxisFormatString 
            XmlElement yAxisFormatStringElement = doc.CreateElement("yAxisFormatString");
            yAxisFormatStringElement.InnerText = chtParameters.YAxisFormat.ToString().Replace("<", "&lt;");
            element.AppendChild(yAxisFormatStringElement);

            //y2AxisLabel 
            XmlElement y2AxisLabelElement = doc.CreateElement("y2AxisLabel");
            y2AxisLabelElement.InnerText = chtParameters.Y2AxisLabel.ToString().Replace("<", "&lt;");
            element.AppendChild(y2AxisLabelElement);

            //y2AxisLegendTitle 
            XmlElement y2AxisLegendTitleElement = doc.CreateElement("y2AxisLegendTitle");
            y2AxisLegendTitleElement.InnerText = chtParameters.Y2AxisLegendTitle.ToString().Replace("<", "&lt;");
            element.AppendChild(y2AxisLegendTitleElement);

            //y2AxisFormatString 
            XmlElement y2AxisFormatStringElement = doc.CreateElement("y2AxisFormatString");
            y2AxisFormatStringElement.InnerText = chtParameters.Y2AxisFormat.ToString().Replace("<", "&lt;");
            element.AppendChild(y2AxisFormatStringElement);

            //xAxisLabelType 
            XmlElement xAxisLabelTypeElement = doc.CreateElement("xAxisLabelType");
            xAxisLabelTypeElement.InnerText = chtParameters.XAxisLabelType.ToString().Replace("<", "&lt;");
            element.AppendChild(xAxisLabelTypeElement);

            //xAxisLabel 
            XmlElement xAxisLabelElement = doc.CreateElement("xAxisLabel");
            xAxisLabelElement.InnerText = chtParameters.XAxisLabel.ToString().Replace("<", "&lt;");
            element.AppendChild(xAxisLabelElement);

            //xAxisAngle 
            XmlElement xAxisAngleElement = doc.CreateElement("xAxisAngle");
            xAxisAngleElement.InnerText = chtParameters.XAxisAngle.ToString().Replace("<", "&lt;");
            element.AppendChild(xAxisAngleElement);

            //chartTitle 
            XmlElement chartTitleElement = doc.CreateElement("chartTitle");
            chartTitleElement.InnerText = chtParameters.ChartTitle.ToString().Replace("<", "&lt;");
            element.AppendChild(chartTitleElement);

            //chartSubTitle 
            XmlElement chartSubTitleElement = doc.CreateElement("chartSubTitle");
            chartSubTitleElement.InnerText = chtParameters.ChartSubTitle.ToString().Replace("<", "&lt;");
            element.AppendChild(chartSubTitleElement);

            //showLegend 
            XmlElement showLegendElement = doc.CreateElement("showLegend");
            showLegendElement.InnerText = chtParameters.ShowLegend.ToString().Replace("<", "&lt;");
            element.AppendChild(showLegendElement);

            //showLegendBorder 
            XmlElement showLegendBorderElement = doc.CreateElement("showLegendBorder");
            showLegendBorderElement.InnerText = chtParameters.ShowLegendBorder.ToString();
            element.AppendChild(showLegendBorderElement);

            //showLegendVarNames 
            XmlElement showLegendVarNamesElement = doc.CreateElement("showLegendVarNames");
            showLegendVarNamesElement.InnerText = chtParameters.ShowLegendVarNames.ToString();
            element.AppendChild(showLegendVarNamesElement);

            //legendFontSize 
            XmlElement legendFontSizeElement = doc.CreateElement("legendFontSize");
            legendFontSizeElement.InnerText = chtParameters.LegendFontSize.ToString();
            element.AppendChild(legendFontSizeElement);

            //EI-98
            //yAxisLabelFontSize 
            XmlElement yAxisLabelFontSizeElement = doc.CreateElement("yAxisLabelFontSize");
            yAxisLabelFontSizeElement.InnerText = chtParameters.YAxisLabelFontSize.ToString().Replace("<", "&lt;");
            element.AppendChild(yAxisLabelFontSizeElement);

            //xAxisLabelFontSize 
            XmlElement xAxisLabelFontSize = doc.CreateElement("xAxisLabelFontSize");
            xAxisLabelFontSize.InnerText = chtParameters.XAxisLabelFontSize.ToString().Replace("<", "&lt;");
            element.AppendChild(xAxisLabelFontSize);

            //yAxisFontSize 
            XmlElement yAxisFontSizeElement = doc.CreateElement("yAxisFontSize");
            yAxisFontSizeElement.InnerText = chtParameters.YAxisFontSize.ToString().Replace("<", "&lt;");
            element.AppendChild(yAxisFontSizeElement);

            //xAxisFontSize 
            XmlElement xAxisFontSize = doc.CreateElement("xAxisFontSize");
            xAxisFontSize.InnerText = chtParameters.XAxisFontSize.ToString().Replace("<", "&lt;");
            element.AppendChild(xAxisFontSize);

            //legendDock 
            XmlElement legendDockElement = doc.CreateElement("legendDock");
            switch (chtParameters.LegendDock)
            {
                case ComponentArt.Win.DataVisualization.Charting.Dock.Left:
                    legendDockElement.InnerText = "0";
                    break;
                default:
                case ComponentArt.Win.DataVisualization.Charting.Dock.Right:
                    legendDockElement.InnerText = "1";
                    break;
                case ComponentArt.Win.DataVisualization.Charting.Dock.Top:
                    legendDockElement.InnerText = "2";
                    break;
                case ComponentArt.Win.DataVisualization.Charting.Dock.Bottom:
                    legendDockElement.InnerText = "3";
                    break;
            }
            element.AppendChild(legendDockElement);

            SerializeAnchors(element);

            return element;
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


        #endregion //Public Methods

        #region Event Handlers
        private void properties_ChangesAccepted(object sender, EventArgs e)
        {
            Controls.GadgetProperties.AreaChartProperties properties = Popup.Content as Controls.GadgetProperties.AreaChartProperties;
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

        private void properties_Cancelled(object sender, EventArgs e)
        {
            Popup.Close();
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

                AreaChartParameters chtParameters = (AreaChartParameters)Parameters;
                string freqVar = chtParameters.ColumnNames[0];
                string weightVar = chtParameters.WeightVariableName;
                string crosstabVar = chtParameters.CrosstabVariableName;
                bool includeMissing = chtParameters.IncludeMissing;

                List<string> stratas = new List<string>();
                try
                {
                    RequestUpdateStatusDelegate requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
                    CheckForCancellationDelegate checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

                    chtParameters.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                    chtParameters.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);

                    if (this.DataFilters != null && this.DataFilters.Count > 0)
                    {
                        chtParameters.CustomFilter = this.DataFilters.GenerateDataFilterString(false);
                    }
                    else
                    {
                        chtParameters.CustomFilter = string.Empty;
                    }

                    if (!string.IsNullOrEmpty(crosstabVar.Trim()))
                    {
                        List<string> crosstabVarList = new List<string>();
                        crosstabVarList.Add(crosstabVar);

                        foreach (Strata strata in DashboardHelper.GetStrataValuesAsDictionary(crosstabVarList, false, false))
                        {
                            AreaChartParameters parameters = new AreaChartParameters(chtParameters);

                            if (!string.IsNullOrEmpty(chtParameters.CustomFilter))
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
                        Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables = DashboardHelper.GenerateFrequencyTable(chtParameters);
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
                    Debug.Print("Area chart gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete.");
                    Debug.Print(DashboardHelper.DataFilters.GenerateDataFilterString());
                }
            }
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0, bool useAlternatingColors = false)
        {
            AreaChartParameters chtParameters = (AreaChartParameters)Parameters;
            if (chtParameters.ColumnNames.Count < 1) return string.Empty;	

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<h2>" + chtParameters.ChartTitle + "</h2>");
            sb.AppendLine("<h3>" + chtParameters.ChartSubTitle + "</h3>");

            foreach (UIElement element in panelMain.Children)
            {
                if (element is Controls.Charting.AreaChart)
                {
                    sb.AppendLine(((Controls.Charting.AreaChart)element).ToHTML(htmlFileName, count, true, false));
                    count++;
                }
            }

            return sb.ToString();
        }

        #endregion //Event Handlers
    }
}
