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
using EpiDashboard.Controls.Charting;
using ComponentArt.Win.DataVisualization.Charting;


namespace EpiDashboard.Gadgets.Charting
{
    /// <summary>
    /// Interaction logic for ParetoChartGadget.xaml
    /// </summary>
    public partial class ParetoChartGadget : ParetoChartGadgetBase
    {
        #region Constructors
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
            //FillComboboxes();
        }

        #endregion //Constructors

        #region Delegates
        private delegate void SetChartDataDelegate(List<XYParetoChartData> dataList);

        #endregion //Delegates

        #region Events

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
                ParetoChartParameters chtParameters = (ParetoChartParameters)Parameters;

                string freqVar = chtParameters.ColumnNames[0];
                string weightVar = chtParameters.WeightVariableName;
                string crosstabVar = chtParameters.CrosstabVariableName;
                bool includeMissing = chtParameters.IncludeMissing;

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

                    Dictionary<DataTable, List<DescriptiveStatistics>> stratifiedFrequencyTables = DashboardHelper.GenerateFrequencyTable(chtParameters);
                    List<XYParetoChartData> dataList = new List<XYParetoChartData>();

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
                            XYParetoChartData chartData = new XYParetoChartData();
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
                            foreach (XYParetoChartData chartData in dataList)
                            {
                                if (chartData.X.ToString() == row[0].ToString())
                                {
                                    chartData.Z = ((chartData.Y / max)) + runningPercent;
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
                    Debug.Print("Pareto chart gadget took " + stopwatch.Elapsed.ToString() + " seconds to complete.");
                    Debug.Print(DashboardHelper.DataFilters.GenerateDataFilterString());
                }
            }
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

        //private void checkboxUseDiffColors_Checked(object sender, RoutedEventArgs e)
        //{
        //    xyChart.UseDifferentBarColors = true;
        //}

        //private void checkboxUseDiffColors_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    xyChart.UseDifferentBarColors = false;
        //}

        //private void checkboxShowLegend_Checked(object sender, RoutedEventArgs e)
        //{
        //    xyChart.LegendVisible = true;
        //}

        //private void checkboxShowLegend_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    xyChart.LegendVisible = false;
        //}

        //private void checkboxUseRefValues_Checked(object sender, RoutedEventArgs e)
        //{
        //    yAxis.UseReferenceValue = true;
        //}

        //private void checkboxUseRefValues_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    yAxis.UseReferenceValue = false;
        //}

        //private void checkboxShowLegendBorder_Checked(object sender, RoutedEventArgs e)
        //{
        //    checkboxShowLegend.IsChecked = true;
        //    xyChart.Legend.BorderThickness = new Thickness(1);
        //}

        //private void checkboxShowLegendBorder_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    xyChart.Legend.BorderThickness = new Thickness(0);
        //}

        private void properties_ChangesAccepted(object sender, EventArgs e)
        {
            Controls.GadgetProperties.ParetoChartProperties properties = Popup.Content as Controls.GadgetProperties.ParetoChartProperties;
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

        #endregion //Events

        #region Properties

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
        /// A custom heading to use for this gadget's output
        /// </summary>
        private string customOutputHeading;

        /// <summary>
        /// A custom description to use for this gadget's output
        /// </summary>
        private string customOutputDescription;

        #endregion //Properties

        #region Methods
        public void CreateFromLegacyXml(XmlElement element)
        {
            this.Parameters = new ParetoChartParameters();

            foreach (XmlElement child in element.ChildNodes)
            {
                if (!String.IsNullOrEmpty(child.InnerText))
                {
                    switch (child.Name.ToLower())
                    {
                        case "singlevariable":
                            //cmbField.Text = child.InnerText.Replace("&lt;", "<");
                                if (this.Parameters.ColumnNames.Count > 0)
                                {
                                    ((ParetoChartParameters)Parameters).ColumnNames[0] = (child.InnerText.Replace("&lt;", "<"));
                                }
                                else
                                {
                                    ((ParetoChartParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                                }
                            break;
                        case "weightvariable":
                            //cmbFieldWeight.Text = child.InnerText.Replace("&lt;", "<");
                            ((ParetoChartParameters)Parameters).WeightVariableName = child.InnerText.Replace("&lt;", "<");
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
                        case "chartsize":
                            //cbxChartSize.SelectedItem = child.InnerText;
                            break;
                        case "chartwidth":
                            ((ParetoChartParameters)Parameters).ChartWidth = double.Parse(child.InnerText.Replace("&lt;", "<"));
                            break;
                        case "chartheight":
                            ((ParetoChartParameters)Parameters).ChartHeight = double.Parse(child.InnerText.Replace("&lt;", "<"));
                            break;
                        case "charttitle":
                            ((ParetoChartParameters)Parameters).ChartTitle = child.InnerText.Replace("&lt;", "<");
                            break;
                        case "chartlegendtitle":
                            //LegendTitle = child.InnerText;
                            break;
                        case "xaxislabel":
                            ((ParetoChartParameters)Parameters).XAxisLabel = child.InnerText.Replace("&lt;", "<");
                            break;
                        case "yaxislabel":
                            ((ParetoChartParameters)Parameters).YAxisLabel = child.InnerText.Replace("&lt;", "<");
                            break;
                        case "xaxisrotation":
                            switch (child.InnerText)
                            {
                                case "0":
                                    ((ParetoChartParameters)Parameters).XAxisAngle = 0;
                                    break;
                                case "45":
                                    ((ParetoChartParameters)Parameters).XAxisAngle = -45;
                                    break;
                                case "90":
                                    ((ParetoChartParameters)Parameters).XAxisAngle = -90;
                                    break;
                                default:
                                    ((ColumnChartParameters)Parameters).XAxisAngle = 0;
                                    break;
                            }
                            break;
                    }
                }
            }
        }

        public override void CreateFromXml(XmlElement element)
        {
            this.LoadingCombos = true;
            this.Parameters = new ParetoChartParameters();
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
                    if (!String.IsNullOrEmpty(child.InnerText))
                    {
                        switch (child.Name.ToLower())
                        {
                            case "mainvariable":
                                if (this.Parameters.ColumnNames.Count > 0)
                                {
                                    ((ParetoChartParameters)Parameters).ColumnNames[0] = (child.InnerText.Replace("&lt;", "<"));
                                }
                                else
                                {
                                    ((ParetoChartParameters)Parameters).ColumnNames.Add(child.InnerText.Replace("&lt;", "<"));
                                }
                                break;
                            case "weightvariable":
                                ((ParetoChartParameters)Parameters).WeightVariableName = child.InnerText.Replace("&lt;", "<");
                                break;
                            case "showlistlabels":
                                if (child.InnerText.ToLower().Equals("true"))
                                {
                                    ((ParetoChartParameters)Parameters).ShowCommentLegalLabels = true;
                                }
                                else { ((ParetoChartParameters)Parameters).ShowCommentLegalLabels = false; }
                                break;
                            case "includemissing":
                                if (child.InnerText.ToLower().Equals("true")) { ((ParetoChartParameters)Parameters).IncludeMissing = true; }
                                else { ((ParetoChartParameters)Parameters).IncludeMissing = false; }
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
                                if (child.InnerText.ToLower().Equals("true")) { ((ParetoChartParameters)Parameters).UseRefValues = true; }
                                else { ((ParetoChartParameters)Parameters).UseRefValues = false; }
                                break;
                            case "showannotations":
                                if (child.InnerText.ToLower().Equals("true")) { ((ParetoChartParameters)Parameters).ShowAnnotations = true; }
                                else { ((ParetoChartParameters)Parameters).ShowAnnotations = false; }
                                break;
                            case "barspace":
                                {
                                    switch (child.InnerText)
                                    {
                                        case "0":
                                            ((ParetoChartParameters)Parameters).BarSpace = BarSpacing.Default;
                                            break;
                                        case "1":
                                            ((ParetoChartParameters)Parameters).BarSpace = BarSpacing.None;
                                            break;
                                        case "2":
                                            ((ParetoChartParameters)Parameters).BarSpace = BarSpacing.Small;
                                            break;
                                        case "3":
                                            ((ParetoChartParameters)Parameters).BarSpace = BarSpacing.Medium;
                                            break;
                                        case "4":
                                            ((ParetoChartParameters)Parameters).BarSpace = BarSpacing.Large;
                                            break;
                                    }
                                }                                break;
                            case "palette":
                                ((ParetoChartParameters)Parameters).Palette = int.Parse(child.InnerText);
                                break;
                            case "bartype":
                                switch (child.InnerText.ToString())
                                    {
                                        default:
                                        case "0":
                                            ((ParetoChartParameters)Parameters).BarKind = BarKind.Block;
                                            break;
                                        case "1":
                                            ((ParetoChartParameters)Parameters).BarKind = BarKind.Cylinder;
                                            break;
                                        case "2":
                                            ((ParetoChartParameters)Parameters).BarKind = BarKind.Rectangle;
                                            break;
                                        case "3":
                                            ((ParetoChartParameters)Parameters).BarKind = BarKind.RoundedBlock;
                                            break;
                                    }
                                break;
                            case "yaxislabel":
                                ((ParetoChartParameters)Parameters).YAxisLabel = child.InnerText;
                                break;
                            case "xaxislabeltype":
                                ((ParetoChartParameters)Parameters).XAxisLabelType = int.Parse(child.InnerText);
                                break;
                            case "xaxislabel":
                                ((ParetoChartParameters)Parameters).XAxisLabel = child.InnerText;
                                break;
                            case "xaxisangle":
                                ((ParetoChartParameters)Parameters).XAxisAngle = int.Parse(child.InnerText);
                                break;
                            case "charttitle":
                                ((ParetoChartParameters)Parameters).ChartTitle = child.InnerText;
                                break;
                            case "showlegend":
                                if (child.InnerText.ToLower().Equals("true")) { ((ParetoChartParameters)Parameters).ShowLegend = true; }
                                else { ((ParetoChartParameters)Parameters).ShowLegend = false; }
                                break;
                            case "showlegendborder":
                                if (child.InnerText.ToLower().Equals("true")) { ((ParetoChartParameters)Parameters).ShowLegendBorder = true; }
                                else { ((ParetoChartParameters)Parameters).ShowLegendBorder = false; }
                                break;
                            case "showlegendvarnames":
                                if (child.InnerText.ToLower().Equals("true")) { ((ParetoChartParameters)Parameters).ShowLegendVarNames = true; }
                                else { ((ParetoChartParameters)Parameters).ShowLegendVarNames = false; }
                                break;
                            case "legendfontsize":
                                ((ParetoChartParameters)Parameters).LegendFontSize = int.Parse(child.InnerText);
                                break;
                            case "height":
                                ((ParetoChartParameters)Parameters).ChartHeight = double.Parse(child.InnerText);
                                break;
                            case "width":
                                ((ParetoChartParameters)Parameters).ChartWidth = double.Parse(child.InnerText);
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
            ParetoChartParameters chtParameters = (ParetoChartParameters)Parameters;

            System.Xml.XmlElement element = doc.CreateElement("paretoChartGadget");
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
            collapsed.Value = "false"; // currently no way to collapse the gadget, so leave this 'false' for now
            type.Value = "EpiDashboard.Gadgets.Charting.ParetoChartGadget";

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);
            element.Attributes.Append(id);

            //this.CustomOutputHeading = chtParameters.GadgetTitle;
            //this.CustomOutputDescription = chtParameters.GadgetDescription;
            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            string mainVar = string.Empty;
            string strataVar = string.Empty;
            string weightVar = string.Empty;
            string sort = string.Empty;
            bool allValues = false;
            bool showConfLimits = true;
            bool showCumulativePercent = true;
            bool includeMissing = false;

            double height = 600;
            double width = 800;

            double.TryParse(chtParameters.ChartHeight.ToString(), out height);
            double.TryParse(chtParameters.ChartWidth.ToString(), out width);

            #region PreProperties Code
            //if (inputVariableList.ContainsKey("freqvar"))
            //{
            //    mainVar = inputVariableList["freqvar"].Replace("<", "&lt;");
            //}
            //if (inputVariableList.ContainsKey("weightvar"))
            //{
            //    weightVar = inputVariableList["weightvar"].Replace("<", "&lt;");
            //}
            //if (inputVariableList.ContainsKey("sort"))
            //{
            //    sort = inputVariableList["sort"];
            //}
            //if (inputVariableList.ContainsKey("allvalues"))
            //{
            //    allValues = bool.Parse(inputVariableList["allvalues"]);
            //}
            //if (inputVariableList.ContainsKey("showconflimits"))
            //{
            //    showConfLimits = bool.Parse(inputVariableList["showconflimits"]);
            //}
            //if (inputVariableList.ContainsKey("showcumulativepercent"))
            //{
            //    showCumulativePercent = bool.Parse(inputVariableList["showcumulativepercent"]);
            //}
            //if (inputVariableList.ContainsKey("includemissing"))
            //{
            //    includeMissing = bool.Parse(inputVariableList["includemissing"]);
            //}

            //CustomOutputHeading = headerPanel.Text;
            //CustomOutputDescription = descriptionPanel.Text;

            //string xmlString =
            //"<mainVariable>" + mainVar + "</mainVariable>";

            //if (GadgetOptions.StrataVariableNames.Count == 1)
            //{
            //    xmlString = xmlString + "<strataVariable>" + GadgetOptions.StrataVariableNames[0].Replace("<", "&lt;") + "</strataVariable>";
            //}
            //else if (GadgetOptions.StrataVariableNames.Count > 1)
            //{
            //    xmlString = xmlString + "<strataVariables>";

            //    foreach (string strataVariable in this.GadgetOptions.StrataVariableNames)
            //    {
            //        xmlString = xmlString + "<strataVariable>" + strataVariable.Replace("<", "&lt;") + "</strataVariable>";
            //    }

            //    xmlString = xmlString + "</strataVariables>";
            //}

            //xmlString = xmlString + "<weightVariable>" + weightVar + "</weightVariable>" +

            //    "<height>" + height + "</height>" +
            //    "<width>" + width + "</width>" +

            //"<sort>" + sort + "</sort>" +
            //"<allValues>" + allValues + "</allValues>" +
            //"<showListLabels>" + checkboxCommentLegalLabels.IsChecked + "</showListLabels>" +
            //"<includeMissing>" + includeMissing + "</includeMissing>" +
            //"<customHeading>" + CustomOutputHeading.Replace("<", "&lt;") + "</customHeading>" +
            //"<customDescription>" + CustomOutputDescription.Replace("<", "&lt;") + "</customDescription>" +
            //"<customCaption>" + CustomOutputCaption + "</customCaption>";

            //xmlString += "<useRefValues>" + checkboxUseRefValues.IsChecked.Value + "</useRefValues>";
            //xmlString += "<showAnnotations>" + checkboxAnnotations.IsChecked.Value + "</showAnnotations>";

            //xmlString += "<barSpace>" + cmbBarSpacing.SelectedIndex + "</barSpace>";
            //xmlString += "<palette>" + cmbPalette.SelectedIndex + "</palette>";
            //xmlString += "<barType>" + cmbBarType.SelectedIndex + "</barType>";

            //xmlString += "<yAxisLabel>" + txtYAxisLabelValue.Text + "</yAxisLabel>";
            //xmlString += "<xAxisLabelType>" + cmbXAxisLabelType.SelectedIndex + "</xAxisLabelType>";
            //xmlString += "<xAxisLabel>" + txtXAxisLabelValue.Text + "</xAxisLabel>";
            //xmlString += "<xAxisAngle>" + txtXAxisAngle.Text + "</xAxisAngle>";
            //xmlString += "<chartTitle>" + txtChartTitle.Text + "</chartTitle>";

            //xmlString += "<showLegend>" + checkboxShowLegend.IsChecked.Value + "</showLegend>";
            //xmlString += "<showLegendBorder>" + checkboxShowLegendBorder.IsChecked.Value + "</showLegendBorder>";
            //xmlString += "<showLegendVarNames>" + checkboxShowVarName.IsChecked.Value + "</showLegendVarNames>";
            //xmlString += "<legendFontSize>" + txtLegendFontSize.Text + "</legendFontSize>";

            //xmlString = xmlString + SerializeAnchors();

            //System.Xml.XmlElement element = doc.CreateElement("paretoChartGadget");
            //element.InnerXml = xmlString;
            //element.AppendChild(SerializeFilters(doc));

            //System.Xml.XmlAttribute id = doc.CreateAttribute("id");
            //System.Xml.XmlAttribute locationY = doc.CreateAttribute("top");
            //System.Xml.XmlAttribute locationX = doc.CreateAttribute("left");
            //System.Xml.XmlAttribute collapsed = doc.CreateAttribute("collapsed");
            //System.Xml.XmlAttribute type = doc.CreateAttribute("gadgetType");

            //id.Value = this.UniqueIdentifier.ToString();
            //locationY.Value = Canvas.GetTop(this).ToString("F0");
            //locationX.Value = Canvas.GetLeft(this).ToString("F0");
            //collapsed.Value = "false"; // currently no way to collapse the gadget, so leave this 'false' for now
            //type.Value = "EpiDashboard.Gadgets.Charting.ParetoChartGadget";

            //element.Attributes.Append(locationY);
            //element.Attributes.Append(locationX);
            //element.Attributes.Append(collapsed);
            //element.Attributes.Append(type);
            //element.Attributes.Append(id);
            #endregion //PreProperties Code

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


            //useDiffBarColors 
            //XmlElement useDiffBarColorsElement = doc.CreateElement("useDiffBarColors");
            //useDiffBarColorsElement.InnerText = chtParameters.UseDiffColors.ToString();
            //element.AppendChild(useDiffBarColorsElement);

            //useRefValues 
            XmlElement useRefValuesElement = doc.CreateElement("useRefValues");
            useRefValuesElement.InnerText = chtParameters.UseRefValues.ToString();
            element.AppendChild(useRefValuesElement);

            //showAnnotations 
            XmlElement showAnnotationsElement = doc.CreateElement("showAnnotations");
            showAnnotationsElement.InnerText = chtParameters.ShowAnnotations.ToString();
            element.AppendChild(showAnnotationsElement);

            //showGridLines 
            XmlElement showGridLinesElement = doc.CreateElement("showGridLines");
            showGridLinesElement.InnerText = chtParameters.ShowGridLines.ToString();
            element.AppendChild(showGridLinesElement);

            //barSpace 
            XmlElement barSpaceElement = doc.CreateElement("barSpace");
            switch (chtParameters.BarSpace.ToString())
            {
                default:
                case "Default":
                    barSpaceElement.InnerText = "0";
                    break;
                case "None":
                    barSpaceElement.InnerText = "1";
                    break;
                case "Small":
                    barSpaceElement.InnerText = "2";
                    break;
                case "Medium":
                    barSpaceElement.InnerText = "3";
                    break;
                case "Large":
                    barSpaceElement.InnerText = "4";
                    break;
            }
            element.AppendChild(barSpaceElement);

            //palette 
            XmlElement paletteElement = doc.CreateElement("palette");
            paletteElement.InnerText = chtParameters.Palette.ToString();
            element.AppendChild(paletteElement);

            //barType 
            XmlElement barTypeElement = doc.CreateElement("barType");
            switch (chtParameters.BarKind.ToString())
            {
                case "Block":
                    barTypeElement.InnerText = "0";
                    break;
                case "Cylinder":
                    barTypeElement.InnerText = "1";
                    break;
                case "Rectangle":
                    barTypeElement.InnerText = "2";
                    break;
                case "RoundedBlock":
                    barTypeElement.InnerText = "3";
                    break;
            }
            element.AppendChild(barTypeElement);

            //yAxisLabel 
            XmlElement yAxisLabelElement = doc.CreateElement("yAxisLabel");
            yAxisLabelElement.InnerText = chtParameters.YAxisLabel.ToString().Replace("<", "&lt;");
            element.AppendChild(yAxisLabelElement);

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
            //XmlElement chartSubTitleElement = doc.CreateElement("chartSubTitle");
            //chartSubTitleElement.InnerText = chtParameters.ChartSubTitle.ToString().Replace("<", "&lt;");
            //element.AppendChild(chartSubTitleElement);

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

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public override string ToHTML(string htmlFileName = "", int count = 0)
        {
            ParetoChartParameters chtParameters = (ParetoChartParameters)Parameters;

            StringBuilder htmlBuilder = new StringBuilder();
            CustomOutputHeading = headerPanel.Text;
            CustomOutputDescription = descriptionPanel.Text;

            htmlBuilder.AppendLine("<h2>" + chtParameters.ChartTitle + "</h2>");
            if (!String.IsNullOrEmpty(chtParameters.ChartSubTitle))
            {
                htmlBuilder.AppendLine("<h3>" + chtParameters.ChartSubTitle + "</h3>");
            }

            foreach (UIElement element in panelMain.Children)
            {
                if (element is EpiDashboard.Controls.Charting.ParetoChart)
                {
                    htmlBuilder.AppendLine(((EpiDashboard.Controls.Charting.ParetoChart)element).ToHTML(htmlFileName, count, true, false));
                }
            }

//HANDLED BY CHART BASE
            //imageFileName = imageFileName + "_" + count.ToString() + ".png";

            //BitmapSource img = (BitmapSource)Common.ToImageSource(xyChart);
            //System.IO.FileStream stream = new System.IO.FileStream(imageFileName, System.IO.FileMode.Create);
            ////JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            //PngBitmapEncoder encoder = new PngBitmapEncoder();
            //encoder.Frames.Add(BitmapFrame.Create(img));
            //encoder.Save(stream);
            //stream.Close();

            //htmlBuilder.AppendLine("<img src=\"" + imageFileName + "\" />");

            return htmlBuilder.ToString();
        }

        public override void ShowHideConfigPanel()
        {
            Popup = new DashboardPopup();
            Popup.Parent = ((this.Parent as DragCanvas).Parent as ScrollViewer).Parent as Grid;
            Controls.GadgetProperties.ParetoChartProperties properties = new Controls.GadgetProperties.ParetoChartProperties(this.DashboardHelper, this, (ParetoChartParameters)Parameters, StrataGridList);

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

        public override void RefreshResults()
        {
            ParetoChartParameters chtParameters = (ParetoChartParameters)Parameters;
            if (!LoadingCombos)
            {
                if (chtParameters != null)
                {
                    if (chtParameters.ColumnNames.Count > 0 && !String.IsNullOrEmpty(chtParameters.ColumnNames[0]))
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
                List<XYParetoChartData> dataList = xyChart.DataSource as List<XYParetoChartData>;

                sb.Append("X" + "\t");
                sb.Append("Y1" + "\t");
                sb.Append("Y2" + "\t");
                sb.AppendLine();

                foreach (XYParetoChartData chartData in dataList)
                {
                    sb.Append(chartData.X + "\t");
                    sb.Append(chartData.Y + "\t");
                    sb.Append(chartData.Z + "\t");
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        protected override void Construct()
        {
            this.Parameters = new ParetoChartParameters();

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

            //tblockWidth.Text = DashboardSharedStrings.GADGET_WIDTH;
            //tblockHeight.Text = DashboardSharedStrings.GADGET_HEIGHT;
            //tblockMainVariable.Text = DashboardSharedStrings.GADGET_MAIN_VARIABLE;
            //tblockWeightVariable.Text = DashboardSharedStrings.GADGET_WEIGHT_VARIABLE;
            //checkboxCommentLegalLabels.Content = DashboardSharedStrings.GADGET_LIST_LABELS;
            //checkboxIncludeMissing.Content = DashboardSharedStrings.GADGET_INCLUDE_MISSING;

            //expanderAdvancedOptions.Header = DashboardSharedStrings.GADGET_ADVANCED_OPTIONS;
            //expanderDisplayOptions.Header = DashboardSharedStrings.GADGET_DISPLAY_OPTIONS;

            //lblColorsStyles.Content = ChartingSharedStrings.PANEL_COLORS_AND_STYLES;
            //lblLabels.Content = ChartingSharedStrings.PANEL_LABELS;
            //lblLegend.Content = ChartingSharedStrings.PANEL_LEGEND;

            //checkboxUseRefValues.Content = ChartingSharedStrings.USE_REFERENCE_VALUES;
            //checkboxAnnotations.Content = ChartingSharedStrings.SHOW_ANNOTATIONS;
            //tblockPalette.Text = ChartingSharedStrings.COLOR_PALETTE;

            //tblockYAxisLabelValue.Text = ChartingSharedStrings.Y_AXIS_LABEL;

            //tblockXAxisLabelType.Text = ChartingSharedStrings.X_AXIS_LABEL_TYPE;
            //tblockXAxisLabelValue.Text = ChartingSharedStrings.X_AXIS_LABEL;
            //tblockXAxisAngle.Text = ChartingSharedStrings.X_AXIS_ANGLE;
            //tblockChartTitleValue.Text = ChartingSharedStrings.CHART_TITLE;

            //checkboxShowLegend.Content = ChartingSharedStrings.SHOW_LEGEND;
            //checkboxShowLegendBorder.Content = ChartingSharedStrings.SHOW_LEGEND_BORDER;
            //checkboxShowVarName.Content = ChartingSharedStrings.SHOW_VAR_NAMES;
            //tblockLegendFontSize.Text = ChartingSharedStrings.LEGEND_FONT_SIZE;

            //btnRun.Content = DashboardSharedStrings.GADGET_RUN_BUTTON;

            #endregion // Translation

            //xyChart.Legend.BorderBrush = Brushes.Gray;

            //double legendFontSize = 13;
            //double.TryParse(txtLegendFontSize.Text, out legendFontSize);
            //xyChart.Legend.FontSize = legendFontSize;

            base.Construct();
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

        private void CreateInputVariableList()
        {
            //MOVED TO PARETO CHART PROPERTIES

            //Dictionary<string, string> inputVariableList = new Dictionary<string, string>();

            //GadgetOptions.MainVariableName = string.Empty;
            //GadgetOptions.WeightVariableName = string.Empty;
            //GadgetOptions.StrataVariableNames = new List<string>();
            //GadgetOptions.CrosstabVariableName = string.Empty;
            //GadgetOptions.ColumnNames = new List<string>();

            //GadgetOptions.ShouldSortHighToLow = true;
            //GadgetOptions.InputVariableList.Add("charttype", "pareto");

            //if (cmbField.SelectedIndex > -1 && !string.IsNullOrEmpty(cmbField.SelectedItem.ToString()))
            //{
            //    inputVariableList.Add("freqvar", cmbField.SelectedItem.ToString());
            //    GadgetOptions.MainVariableName = cmbField.SelectedItem.ToString();
            //}
            //else
            //{
            //    return;
            //}

            //if (cmbFieldWeight.SelectedIndex > -1 && !string.IsNullOrEmpty(cmbFieldWeight.SelectedItem.ToString()))
            //{
            //    inputVariableList.Add("weightvar", cmbFieldWeight.SelectedItem.ToString());
            //    GadgetOptions.WeightVariableName = cmbFieldWeight.SelectedItem.ToString();
            //}

            //if (checkboxCommentLegalLabels.IsChecked == true)
            //{
            //    GadgetOptions.ShouldShowCommentLegalLabels = true;
            //}
            //else
            //{
            //    GadgetOptions.ShouldShowCommentLegalLabels = false;
            //}

            //if (checkboxIncludeMissing.IsChecked == true)
            //{
            //    inputVariableList.Add("includemissing", "true");
            //    GadgetOptions.ShouldIncludeMissing = true;
            //}
            //else
            //{
            //    inputVariableList.Add("includemissing", "false");
            //    GadgetOptions.ShouldIncludeMissing = false;
            //}

            //GadgetOptions.ShouldIncludeFullSummaryStatistics = false;
            //GadgetOptions.InputVariableList = inputVariableList;
        }

        private void FillComboboxes(bool update = false)
        {
            #region PreProperties
            //LoadingCombos = true;

            //string prevField = string.Empty;
            //string prevWeightField = string.Empty;
            //List<string> prevStrataFields = new List<string>();

            ////cmbFontSelector.ItemsSource = Fonts.SystemFontFamilies;

            //if (update)
            //{
            //    if (cmbField.SelectedIndex >= 0)
            //    {
            //        prevField = cmbField.SelectedItem.ToString();
            //    }
            //    if (cmbFieldWeight.SelectedIndex >= 0)
            //    {
            //        prevWeightField = cmbFieldWeight.SelectedItem.ToString();
            //    }
            //}

            //cmbField.ItemsSource = null;
            //cmbField.Items.Clear();

            //cmbFieldWeight.ItemsSource = null;
            //cmbFieldWeight.Items.Clear();

            //List<string> fieldNames = new List<string>();
            //List<string> weightFieldNames = new List<string>();
            //List<string> strataFieldNames = new List<string>();

            //weightFieldNames.Add(string.Empty);

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

            //cmbField.ItemsSource = fieldNames;
            //cmbFieldWeight.ItemsSource = weightFieldNames;

            //if (cmbField.Items.Count > 0)
            //{
            //    cmbField.SelectedIndex = -1;
            //}
            //if (cmbFieldWeight.Items.Count > 0)
            //{
            //    cmbFieldWeight.SelectedIndex = -1;
            //}

            //if (update)
            //{
            //    cmbField.SelectedItem = prevField;
            //    cmbFieldWeight.SelectedItem = prevWeightField;
            //}

            //LoadingCombos = false;
            #endregion //PreProperties
        }

        private void ClearResults()
        {
            messagePanel.Visibility = System.Windows.Visibility.Collapsed;
            messagePanel.Text = string.Empty;
            descriptionPanel.PanelMode = EpiDashboard.Controls.GadgetDescriptionPanel.DescriptionPanelMode.Collapsed;

            tblockChartTitle.Visibility = System.Windows.Visibility.Collapsed;

            panelMain.Children.Clear();
            StrataGridList.Clear();
            StrataExpanderList.Clear();
        }

        //private void ShowLabels()
        //{
        //    ParetoChartParameters chtParameters = (ParetoChartParameters)Parameters;
        //    if (string.IsNullOrEmpty(chtParameters.XAxisLabel))
        //    {
        //        tblockXAxisLabel.Visibility = System.Windows.Visibility.Collapsed;
        //    }
        //    else
        //    {
        //        tblockXAxisLabel.Visibility = System.Windows.Visibility.Visible;
        //    }

        //    if (string.IsNullOrEmpty(chtParameters.YAxisLabel))
        //    {
        //        tblockYAxisLabel.Visibility = System.Windows.Visibility.Collapsed;
        //    }
        //    else
        //    {
        //        tblockYAxisLabel.Visibility = System.Windows.Visibility.Visible;
        //    }

        //    if (string.IsNullOrEmpty(chtParameters.ChartTitle))
        //    {
        //        tblockChartTitle.Visibility = System.Windows.Visibility.Collapsed;
        //    }
        //    else
        //    {
        //        tblockChartTitle.Visibility = System.Windows.Visibility.Visible;
        //    }
        //}

        protected void SetChartData(List<XYParetoChartData> dataList)
        {
            #region Input Validation
            if (dataList == null)
            {
                throw new ArgumentNullException("dataList");
            }
            #endregion // Input Validation

            ParetoChartParameters chtParameters = (ParetoChartParameters)Parameters;

            if (dataList.Count > 0)
            {
                chtParameters.ChartStrataTitle = String.Empty;

                EpiDashboard.Controls.Charting.ParetoChart paretoChart = new EpiDashboard.Controls.Charting.ParetoChart(DashboardHelper, chtParameters, dataList);

                paretoChart.Margin = new Thickness(0, 0, 0, 16);
                panelMain.Children.Add(paretoChart);
            }
        }

        //private void SetPalette()
        //{
        //    if (LoadingCombos) { return; }

        //    ComboBoxItem cbi = cmbPalette.SelectedItem as ComboBoxItem;
        //    ComponentArt.Win.DataVisualization.Palette palette = ComponentArt.Win.DataVisualization.Palette.GetPalette(cbi.Content.ToString());

        //    xyChart.Palette = palette;
        //}


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

        #endregion //Methods

        #region Classes
        //public override class XYChartData
        //{
        //    public object X { get; set; }
        //    public double Y { get; set; }
        //    public object Z { get; set; }
        //    //public double Lo { get; set; }
        //    //public double Hi { get; set; }
        //    //public double Open { get; set; }
        //    //public double Close { get; set; }
        //}

        #endregion //Classes
    }
}
