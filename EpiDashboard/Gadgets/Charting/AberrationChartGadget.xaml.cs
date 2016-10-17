using System;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using Epi;
using Epi.Data;
using Epi.Fields;
using EpiDashboard;
using ComponentArt.Win.DataVisualization.Charting;

namespace EpiDashboard.Gadgets.Charting
{
    /// <summary>
    /// Interaction logic for AberrationChartGadget.xaml
    /// </summary>
    public partial class AberrationChartGadget : GadgetBase
    {
        #region Contstants
        private const int DEFAULT_DEVIATIONS = 3;
        private const int DEFAULT_LAG_TIME = 7;
        private const int DEFAULT_TIME_PERIOD = 365;
        #endregion //Constants

        #region Constructors
        public AberrationChartGadget()
        {
            InitializeComponent();
            Construct();
        }

        public AberrationChartGadget(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.DashboardHelper = dashboardHelper;
            Construct();
        }

        #endregion //Constructors

        #region Delegates
        private delegate void AddChartDelegate(List<AberrationChartData> dataList, DataTable aberrationDetails, string strata);
        #endregion //Delegates

        #region Events

        protected override void worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (syncLock)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                this.Dispatcher.BeginInvoke(new SimpleCallback(SetGadgetToProcessingState));
                this.Dispatcher.BeginInvoke(new SimpleCallback(ClearResults));

                AberrationDetectionChartParameters AbDetChartParameters = (AberrationDetectionChartParameters)Parameters;

                string freqVar = AbDetChartParameters.ColumnNames[0];
                string weightVar = AbDetChartParameters.WeightVariableName;
                string strataVar = string.Empty;
                bool includeMissing = AbDetChartParameters.IncludeMissing;

                int lagTime = DEFAULT_LAG_TIME;
                double deviations = DEFAULT_DEVIATIONS;
                int timePeriod = DEFAULT_TIME_PERIOD;

                if (!String.IsNullOrEmpty(AbDetChartParameters.LagTime))
                {
                    int.TryParse(AbDetChartParameters.LagTime, out lagTime);
                }
                if (!String.IsNullOrEmpty(AbDetChartParameters.Deviations ))
                {
                    double.TryParse(AbDetChartParameters.Deviations, out deviations);
                }
                if (!String.IsNullOrEmpty(AbDetChartParameters.TimePeriod))
                {
                    int.TryParse(AbDetChartParameters.TimePeriod, out timePeriod);
                }

                List<string> stratas = new List<string>();
                if (!string.IsNullOrEmpty(strataVar))
                {
                    stratas.Add(strataVar);
                }

                try
                {
                    RequestUpdateStatusDelegate requestUpdateStatus = new RequestUpdateStatusDelegate(RequestUpdateStatusMessage);
                    CheckForCancellationDelegate checkForCancellation = new CheckForCancellationDelegate(IsCancelled);

                    AbDetChartParameters.GadgetStatusUpdate += new GadgetStatusUpdateHandler(requestUpdateStatus);
                    AbDetChartParameters.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(checkForCancellation);

                    if (this.DataFilters != null && this.DataFilters.Count > 0)
                    {
                        AbDetChartParameters.CustomFilter = this.DataFilters.GenerateDataFilterString(false);
                    }
                    else
                    {
                        AbDetChartParameters.CustomFilter = string.Empty;
                    }

                    Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables = DashboardHelper.GenerateFrequencyTable(AbDetChartParameters);
                    List<AberrationChartData> dataList = new List<AberrationChartData>();
                    DataTable aberrationDetails = new DataTable();

                    aberrationDetails.Columns.Add(new DataColumn("Date", typeof(string)));
                    aberrationDetails.Columns.Add(new DataColumn("Count", typeof(double)));
                    aberrationDetails.Columns.Add(new DataColumn("Expected", typeof(double)));
                    aberrationDetails.Columns.Add(new DataColumn("Difference", typeof(string)));

                    foreach (KeyValuePair<DataTable, List<DescriptiveStatistics>> tableKvp in stratifiedFrequencyTables)
                    {
                        dataList = new List<AberrationChartData>();
                        aberrationDetails = new DataTable();

                        aberrationDetails.Columns.Add(new DataColumn("Date", typeof(string)));
                        aberrationDetails.Columns.Add(new DataColumn("Count", typeof(double)));
                        aberrationDetails.Columns.Add(new DataColumn("Expected", typeof(double)));
                        aberrationDetails.Columns.Add(new DataColumn("Difference", typeof(string)));

                        double count = 0;
                        foreach (DescriptiveStatistics ds in tableKvp.Value)
                        {
                            count = count + ds.observations;
                        }

                        string strataValue = tableKvp.Key.TableName;
                        DataTable table = tableKvp.Key;

                        AddFillerRows(table);

                        double lastAvg = double.NegativeInfinity;
                        double lastStdDev = double.NegativeInfinity;
                        Queue<double> frame = new Queue<double>();
                        List<AberrationChartData> actualValues = new List<AberrationChartData>();
                        int rowCount = 1;

                        var rows = from DataRow row in table.Rows
                                   orderby row[0]
                                   select row;

                        foreach (System.Data.DataRow row in rows /*table.Rows*/)
                        {
                            if (!row[freqVar].Equals(DBNull.Value) || (row[freqVar].Equals(DBNull.Value) && includeMissing == true))
                            {
                                DateTime displayValue = DateTime.Parse(row[freqVar].ToString());

                                frame.Enqueue((double)row["freq"]);
                                AberrationChartData actualValue = new AberrationChartData();
                                actualValue.Date = displayValue; // strataValue;
                                actualValue.Actual = (double)row[1];
                                //actualValue.S = row[0];
                                //dataList.Add(actualValue);
                                if (frame.Count > lagTime - 1 /*6*/)
                                {
                                    double[] frameArray = frame.ToArray();
                                    double frameAvg = frameArray.Average();
                                    frame.Dequeue();
                                    double stdDev = CalculateStdDev(frameArray);
                                    if (lastAvg != double.NegativeInfinity)
                                    {
                                        actualValue.Expected = lastAvg;

                                        //dataList.Add(trendValue);
                                        if ((double)row["freq"] > lastAvg + (/*2.99*/deviations * lastStdDev))
                                        {
                                            double freq = (double)row["freq"];

                                            actualValue.Aberration = freq;
                                            double dvs = (freq - lastAvg) / lastStdDev;
                                            aberrationDetails.Rows.Add(displayValue.ToShortDateString(), (double)row["freq"], Math.Round(lastAvg, 2), Math.Round(dvs, 2).ToString() + " standard deviations");
                                        }
                                    }
                                    lastAvg = frameAvg;
                                    lastStdDev = stdDev;
                                }


                                dataList.Add(actualValue);

                                rowCount++;
                            }
                        }

                        this.Dispatcher.BeginInvoke(new AddChartDelegate(AddChart), dataList, aberrationDetails, strataValue);
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
                    Debug.Print("Aberration Detection chart gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete.");
                    Debug.Print(DashboardHelper.DataFilters.GenerateDataFilterString());
                }
            }
        }

        private void xyChart_DataStructureCreated(object sender, EventArgs e)
        {
            string sName = "";
        }

        #endregion //Events

        #region Properties
        private bool IsDropDownList { get; set; }
        private bool IsCommentLegal { get; set; }
        private bool IsRecoded { get; set; }
        private bool IsOptionField { get; set; }
        #endregion //Properties

        #region Methods
        #region Public Methods
        public override void SetGadgetToProcessingState()
        {
            this.IsProcessing = true;
        }

        public override void SetGadgetToFinishedState()
        {
            this.IsProcessing = false;
            base.SetGadgetToFinishedState();
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0, bool useAlternatingColors = false)
        {
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

            int subCount = 0;
            string imageFileName = string.Empty;

            foreach (UIElement element in panelMain.Children)
            {
                if (element is Controls.Charting.AberrationChart)
                {
                    imageFileName = htmlFileName + "_" + count.ToString() + "_" + subCount.ToString() + ".png";

                    Controls.Charting.AberrationChart abChart = element as Controls.Charting.AberrationChart;

                    htmlBuilder.Append(abChart.ToHTML(htmlFileName, subCount));
                    subCount++;
                }
            }
            return htmlBuilder.ToString();
        }

        public static ImageSource ToImageSource(FrameworkElement obj)
        {
            Transform transform = obj.LayoutTransform;
            Thickness margin = obj.Margin;
            obj.Margin = new Thickness(0, 0, margin.Right - margin.Left, margin.Bottom - margin.Top);
            Size size = new Size(obj.ActualWidth, obj.ActualHeight);
            obj.Measure(size);
            obj.Arrange(new Rect(size));
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)obj.ActualWidth, (int)obj.ActualHeight, 96, 96, PixelFormats.Pbgra32);

            bmp.Render(obj);
            obj.LayoutTransform = transform;
            obj.Margin = margin;
            return bmp;
        }

        public override void RefreshResults()
        {
            AberrationDetectionChartParameters AberrationDetectionChartParameters = (AberrationDetectionChartParameters)Parameters;
            if (!LoadingCombos)
            {
                if (AberrationDetectionChartParameters != null)
                {
                    if (AberrationDetectionChartParameters.ColumnNames.Count > 0 && !String.IsNullOrEmpty(AberrationDetectionChartParameters.ColumnNames[0]))
                    {
                        //CreateInputVariableList();

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
            infoPanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;

            if (element.Name.Equals("aberrationGadget") || element.Name.Equals("AberrationControl"))
            {
                CreateFromLegacyXml(element);
            }
            else
            {
                foreach (XmlElement child in element.ChildNodes)
                {
                    if (!string.IsNullOrEmpty(child.InnerText))
                    {
                        switch (child.Name.ToLowerInvariant())
                        {
                            case "mainvariable":
                                if (this.Parameters.ColumnNames.Count > 0)
                                {
                                    ((AberrationDetectionChartParameters)Parameters).ColumnNames[0] = child.InnerText.Replace("&lt;", "<");
                                }
                                else
                                {
                                    ((AberrationDetectionChartParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                                }
                                break;
                            case "stratavariable":
                                if (((AberrationDetectionChartParameters)Parameters).StrataVariableNames.Count > 0)
                                {
                                    ((AberrationDetectionChartParameters)Parameters).StrataVariableNames[0] = child.InnerText.Replace("&lt;", "<");
                                }
                                else
                                {
                                    ((AberrationDetectionChartParameters)Parameters).StrataVariableNames.Add(child.InnerText.Replace("&lt;", "<"));
                                }
                                break;
                            case "stratavariables":
                                foreach (XmlElement field in child.ChildNodes)
                                {
                                    List<string> fields = new List<string>();
                                    if (field.Name.ToLowerInvariant().Equals("stratavariable"))
                                    {
                                        ((AberrationDetectionChartParameters)Parameters).StrataVariableNames.Add(field.InnerText.Replace("&lt;", "<"));
                                    }
                                }
                                break;
                            case "weightvariable":
                                ((AberrationDetectionChartParameters)Parameters).WeightVariableName = child.InnerText.Replace("&lt;", "<");
                                break;
                            case "customheading":
                                if (!child.InnerText.Equals("(none)"))
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
                                if (!child.InnerText.Equals("(none)"))
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
                            case "width":
                                ((AberrationDetectionChartParameters)Parameters).ChartWidth = int.Parse(child.InnerText.Replace("&lt;", "<"));
                                break;
                            case "height":
                                ((AberrationDetectionChartParameters)Parameters).ChartHeight = int.Parse(child.InnerText.Replace("&lt;", "<"));
                                break;
                            case "charttitle":
                                ((AberrationDetectionChartParameters)Parameters).ChartTitle = child.InnerText.Replace("&lt;", "<");
                                break;
                            case "lagtime":
                                ((AberrationDetectionChartParameters)Parameters).LagTime = child.InnerText.Replace("&lt;", "<");
                                break;
                            case "deviations":
                                ((AberrationDetectionChartParameters)Parameters).Deviations = child.InnerText.Replace("&lt;", "<");
                                break;
                            case "timeperiod":
                                ((AberrationDetectionChartParameters)Parameters).TimePeriod = child.InnerText.Replace("&lt;", "<");
                                break;
                            case "yaxislabel":
                                ((AberrationDetectionChartParameters)Parameters).YAxisLabel = child.InnerText.Replace("&lt;", "<");
                                break;
                            case "xaxislabeltype":
                                ((AberrationDetectionChartParameters)Parameters).XAxisLabelType = int.Parse(child.InnerText);
                                break;
                            case "xaxislabel":
                                ((AberrationDetectionChartParameters)Parameters).XAxisLabel = child.InnerText;
                                break;
                            //EI-98
                            case "yaxislabelfontsize":
                                ((AberrationDetectionChartParameters)Parameters).YAxisLabelFontSize = double.Parse(child.InnerText);
                                break;
                            case "xaxislabelfontsize":
                                ((AberrationDetectionChartParameters)Parameters).XAxisLabelFontSize = double.Parse(child.InnerText);
                                break;
                            case "yaxisfontsize":
                                ((AberrationDetectionChartParameters)Parameters).YAxisFontSize = double.Parse(child.InnerText);
                                break;
                            case "xaxisfontsize":
                                ((AberrationDetectionChartParameters)Parameters).XAxisFontSize = double.Parse(child.InnerText);
                                break;
                        }
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
            AberrationDetectionChartParameters AbDetChartParameters = (AberrationDetectionChartParameters)Parameters;

            System.Xml.XmlElement element = doc.CreateElement("aberrationChartGadget");
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
            type.Value = "EpiDashboard.Gadgets.Charting.AberrationChartGadget";
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

            double height = 600;
            double width = 800;

            double.TryParse(AbDetChartParameters.ChartHeight.ToString(), out height);
            double.TryParse(AbDetChartParameters.ChartWidth.ToString(), out width);

            //mainVariable
            XmlElement freqVarElement = doc.CreateElement("mainVariable");
            if (AbDetChartParameters.ColumnNames.Count > 0)
            {
                if (!String.IsNullOrEmpty(AbDetChartParameters.ColumnNames[0].ToString()))
                {
                    freqVarElement.InnerText = AbDetChartParameters.ColumnNames[0].ToString().Replace("<", "&lt;");
                    element.AppendChild(freqVarElement);
                }
            }

            //strataVariables
            XmlElement StrataVariableNameElement = doc.CreateElement("strataVariable");
            XmlElement StrataVariableNamesElement = doc.CreateElement("strataVariables");
            if (AbDetChartParameters.StrataVariableNames.Count == 1)
            {
                StrataVariableNameElement.InnerText = AbDetChartParameters.StrataVariableNames[0].ToString().Replace("<", "&lt;");
                element.AppendChild(StrataVariableNameElement);
            }
            else if (AbDetChartParameters.StrataVariableNames.Count > 1)
            {
                foreach (string strataColumn in AbDetChartParameters.StrataVariableNames)
                {
                    XmlElement strataElement = doc.CreateElement("strataVariable");
                    strataElement.InnerText = strataColumn.Replace("<", "&lt;");
                    StrataVariableNamesElement.AppendChild(strataElement);
                }
                element.AppendChild(StrataVariableNamesElement);
            }

            //weightVariable
            XmlElement weightVariableElement = doc.CreateElement("weightVariable");
            if (!String.IsNullOrEmpty(AbDetChartParameters.WeightVariableName))
            {
                weightVariableElement.InnerText = AbDetChartParameters.WeightVariableName.Replace("<", "&lt;");
                element.AppendChild(weightVariableElement);
            }

            //height 
            XmlElement heightElement = doc.CreateElement("height");
            heightElement.InnerText = AbDetChartParameters.ChartHeight.ToString().Replace("<", "&lt;");
            element.AppendChild(heightElement);

            //width 
            XmlElement widthElement = doc.CreateElement("width");
            widthElement.InnerText = AbDetChartParameters.ChartWidth.ToString().Replace("<", "&lt;");
            element.AppendChild(widthElement);

            //sort?
            //allValues?
            //includeMissing?

            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            //customHeading
            XmlElement customHeadingElement = doc.CreateElement("customHeading");
            customHeadingElement.InnerText = AbDetChartParameters.GadgetTitle.Replace("<", "&lt;");
            element.AppendChild(customHeadingElement);

            //customDescription
            XmlElement customDescriptionElement = doc.CreateElement("customDescription");
            customDescriptionElement.InnerText = AbDetChartParameters.GadgetDescription.Replace("<", "&lt;");
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

            //lagTime
            XmlElement LagTimeElement = doc.CreateElement("lagTime");
            LagTimeElement.InnerText = AbDetChartParameters.LagTime.ToString().Replace("<", "&lt;");
            element.AppendChild(LagTimeElement);

            //deviations
            XmlElement deviationsElement = doc.CreateElement("deviations");
            deviationsElement.InnerText = AbDetChartParameters.Deviations.ToString().Replace("<", "&lt;");
            element.AppendChild(deviationsElement);

            //timePeriod
            XmlElement timePeriodElement = doc.CreateElement("timePeriod");
            timePeriodElement.InnerText = AbDetChartParameters.TimePeriod.ToString().Replace("<", "&lt;");
            element.AppendChild(timePeriodElement);

            //yAxisLabel 
            XmlElement yAxisLabelElement = doc.CreateElement("yAxisLabel");
            yAxisLabelElement.InnerText = AbDetChartParameters.YAxisLabel.ToString().Replace("<", "&lt;");
            element.AppendChild(yAxisLabelElement);

            ////yAxisFormatString 

            //xAxisLabelType
            XmlElement xAxisLabelTypeElement = doc.CreateElement("xAxisLabelType");
            xAxisLabelTypeElement.InnerText = AbDetChartParameters.XAxisLabelType.ToString().Replace("<", "&lt;");
            element.AppendChild(xAxisLabelTypeElement);

            //xAxisLabel
            XmlElement xAxisLabelElement = doc.CreateElement("xAxisLabel");
            xAxisLabelElement.InnerText = AbDetChartParameters.XAxisLabel.ToString().Replace("<", "&lt;");
            element.AppendChild(xAxisLabelElement);
            
            //chartTitle
            XmlElement chartTitleElement = doc.CreateElement("chartTitle");
            chartTitleElement.InnerText = AbDetChartParameters.ChartTitle.ToString().Replace("<", "&lt;");
            element.AppendChild(chartTitleElement);

            //EI-98

            //yAxisLabelFontSize 
            XmlElement yAxisLabelFontSizeElement = doc.CreateElement("yAxisLabelFontSize");
            yAxisLabelFontSizeElement.InnerText = AbDetChartParameters.YAxisLabelFontSize.ToString().Replace("<", "&lt;");
            element.AppendChild(yAxisLabelFontSizeElement);

            //xAxisLabelFontSize 
            XmlElement xAxisLabelFontSize = doc.CreateElement("xAxisLabelFontSize");
            xAxisLabelFontSize.InnerText = AbDetChartParameters.XAxisLabelFontSize.ToString().Replace("<", "&lt;");
            element.AppendChild(xAxisLabelFontSize);

            //yAxisFontSize 
            XmlElement yAxisFontSizeElement = doc.CreateElement("yAxisFontSize");
            yAxisFontSizeElement.InnerText = AbDetChartParameters.YAxisFontSize.ToString().Replace("<", "&lt;");
            element.AppendChild(yAxisFontSizeElement);

            //xAxisFontSize 
            XmlElement xAxisFontSize = doc.CreateElement("xAxisFontSize");
            xAxisFontSize.InnerText = AbDetChartParameters.XAxisFontSize.ToString().Replace("<", "&lt;");
            element.AppendChild(xAxisFontSize);

            SerializeAnchors(element);
            return element;
        }

        Controls.GadgetProperties.AberrationDetectionChartProperties properties = null;
        public override void ShowHideConfigPanel()
        {
            Popup = new DashboardPopup();
            Popup.Parent = ((this.Parent as DragCanvas).Parent as ScrollViewer).Parent as Grid;
            properties = new Controls.GadgetProperties.AberrationDetectionChartProperties(this.DashboardHelper, this, (AberrationDetectionChartParameters)Parameters, StrataGridList);

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
                properties = new Controls.GadgetProperties.AberrationDetectionChartProperties(this.DashboardHelper, this, (AberrationDetectionChartParameters)Parameters, StrataGridList);
            properties.Height = (Convert.ToInt32(i_StandardHeight * f_HeightRatio)) / 1.07;
            properties.Width = (Convert.ToInt32(i_StandardWidth * f_WidthRatio)) / 1.07;

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

        #region Private and Protected Methods

        /// <summary>
        /// A custom heading to use for this gadget's output
        /// </summary>
        private string customOutputHeading;

        /// <summary>
        /// A custom description to use for this gadget's output
        /// </summary>
        private string customOutputDescription;



        
        private void SetVisuals(Controls.Charting.AberrationChart aberrationChart)
        {
            AberrationDetectionChartParameters AbDetChartParameters = (AberrationDetectionChartParameters)Parameters;

            aberrationChart.Chart.InnerMargins = new Thickness(20, 30, 30, 50);

            aberrationChart.YAxisLabel = AbDetChartParameters.YAxisLabel;

            switch (AbDetChartParameters.XAxisLabelType)
            {
                case 3:
                    aberrationChart.XAxisLabel = AbDetChartParameters.XAxisLabel;
                    break;
                case 1:
                    Field field = DashboardHelper.GetAssociatedField(AbDetChartParameters.ColumnNames[0]);
                    if (field != null && field is IDataField)
                    {
                        RenderableField rField = field as RenderableField;
                        aberrationChart.XAxisLabel = rField.PromptText;
                    }
                    else
                    {
                        aberrationChart.XAxisLabel = AbDetChartParameters.ColumnNames[0];
                    }
                    break;
                case 2:
                    aberrationChart.XAxisLabel = string.Empty;
                    break;
                default:
                    aberrationChart.XAxisLabel = AbDetChartParameters.ColumnNames[0];
                    break;
            }

            aberrationChart.ChartTitle = AbDetChartParameters.ChartTitle;

        }

        protected override void Construct()
        {
            this.Parameters = new AberrationDetectionChartParameters();

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

        /// <summary>
        /// Creates the frequency gadget from an Xml element loaded from a canvas saved in 7.1.1.14 or earlier.
        /// </summary>
        /// <param name="element">The element from which to create the gadget</param>
        private void CreateFromLegacyXml(XmlElement element)
        {
            this.LoadingCombos = true;

            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLowerInvariant())
                {
                    case "mainvariable":
                        if (this.Parameters.ColumnNames.Count > 0)
                        {
                            ((AberrationDetectionChartParameters)Parameters).ColumnNames[0] = child.InnerText.Replace("&lt;", "<");
                        }
                        else
                        {
                            ((AberrationDetectionChartParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                        }
                        break;
                    case "stratavariable":
                        if (((AberrationDetectionChartParameters)Parameters).StrataVariableNames.Count > 0)
                        {
                            ((AberrationDetectionChartParameters)Parameters).StrataVariableNames[0] = child.InnerText.Replace("&lt;", "<");
                        }
                        else
                        {
                            ((AberrationDetectionChartParameters)Parameters).StrataVariableNames.Add(child.InnerText.Replace("&lt;", "<"));
                        }
                        break;
                    case "weightvariable":
                        ((AberrationDetectionChartParameters)Parameters).WeightVariableName = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "lagtime":                        
                        ((AberrationDetectionChartParameters)Parameters).LagTime = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "deviations":
                        ((AberrationDetectionChartParameters)Parameters).Deviations = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "timeperiod":
                        ((AberrationDetectionChartParameters)Parameters).TimePeriod = child.InnerText.Replace("&lt;", "<");
                        break;
                    case "datafilters":
                        this.DataFilters = new DataFilters(this.DashboardHelper);
                        this.DataFilters.CreateFromXml(child);
                        break;
                    case "customheading":
                        if (!child.InnerText.Equals("(none)"))
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
                        if (!child.InnerText.Equals("(none)"))
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
                }
            }

            base.CreateFromXml(element);

            this.LoadingCombos = false;

            RefreshResults();
            HideConfigPanel();
        }

        /// <summary>
        /// TODO: Rename this method
        /// </summary>
        /// <param name="dataList"></param>
        /// <param name="aberrationDetails"></param>
        private void AddChart(List<AberrationChartData> dataList, DataTable aberrationDetails, string strata)
        {
            AberrationDetectionChartParameters AbDetChartParameters = (AberrationDetectionChartParameters)Parameters;
            Controls.Charting.AberrationChart abChart = new Controls.Charting.AberrationChart();
            abChart.DashboardHelper = this.DashboardHelper;
            abChart.SetChartSize(double.Parse(AbDetChartParameters.ChartWidth.ToString()), double.Parse(AbDetChartParameters.ChartHeight.ToString()));
            abChart.SetChartData(dataList, aberrationDetails);
            abChart.SetChartLabels(AbDetChartParameters);
            panelMain.Children.Add(abChart);
            SetVisuals(abChart);

            if (AbDetChartParameters.StrataVariableNames.Count > 0)
            {
                abChart.IndicatorTitle = strata;
            }

            abChart.Margin = new Thickness(0, 0, 0, 48);
        }

        private double CalculateStdDev(IEnumerable<double> values)
        {
            double ret = 0;
            if (values.Count() > 0)
            {
                double avg = values.Average();
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                ret = Math.Sqrt((sum) / (values.Count() - 1));
            }
            return ret;
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

        private void AddFillerRows(DataTable table)
        {
            //bool recreateTable = false;
            string columnType = table.Columns[0].DataType.ToString();
            if (columnType == "System.DateTime")
            {
                DateTime minDate = ((DateTime)table.Rows[0][0]);
                DateTime maxDate = ((DateTime)table.Rows[table.Rows.Count - 1][0]);
                DataColumn[] pkColumns = new DataColumn[1];
                pkColumns[0] = table.Columns[0];
                table.PrimaryKey = pkColumns;
                DateTime curDate = minDate;

                while (curDate < maxDate)
                {
                    curDate = curDate.AddDays(1);
                    DataRow row = table.Rows.Find(curDate);
                    if (row == null)
                    {
                        table.Rows.Add(curDate, 0);
                        //recreateTable = true;
                    }
                }
            }
        }

        private void properties_ChangesAccepted(object sender, EventArgs e)
        {
            Controls.GadgetProperties.AberrationDetectionChartProperties properties = Popup.Content as Controls.GadgetProperties.AberrationDetectionChartProperties;
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

        #endregion //Private and Protected
        #endregion //Methods


        #region Classes

        private class XYChartData
        {
            public DateTime Date { get; set; }
            public double Actual { get; set; }
            public double? Expected { get; set; }
            public double? Aberration { get; set; }
            //public object S { get; set; }
        }

        //public class SimpleDataValue
        //{
        //    public double DependentValue { get; set; }
        //    public DateTime IndependentValue { get; set; }
        //}

        //public class DataGridRow
        //{
        //    public DateTime Date { get; set; }
        //    public double Frequency { get; set; }
        //    public double RunningAverage { get; set; }
        //    public double StandardDeviation { get; set; }
        //    public double Delta
        //    {
        //        get
        //        {
        //            return (Frequency - RunningAverage) / StandardDeviation;
        //        }
        //    }
        //}
        #endregion // Classes    
    }
}
