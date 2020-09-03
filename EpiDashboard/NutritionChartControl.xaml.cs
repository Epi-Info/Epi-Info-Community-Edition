using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Research.DynamicDataDisplay;                
using Microsoft.Research.DynamicDataDisplay.DataSources;    
using Microsoft.Research.DynamicDataDisplay.PointMarkers;   
using Microsoft.Research.DynamicDataDisplay.Charts.Navigation;
using Microsoft.Research.DynamicDataDisplay.ViewportRestrictions;
using Epi;
using Epi.Data;
using Epi.Fields;
using EpiDashboard.NutStat;

namespace EpiDashboard
{
    public enum Gender
    {
        Male,
        Female
    }

    /// <summary>
    /// Interaction logic for NutritionChartControl.xaml
    /// </summary>
    public partial class NutritionChartControl : UserControl, IGadget
    {
        #region Private Members
        private readonly char[] SplitTokens = " \t;".ToCharArray();
        private bool triangleCollapsed;
        private bool loadingCombos;
        private DashboardHelper dashboardHelper;        
        private bool isProcessing;
        private GadgetParameters gadgetOptions;
        private object syncLock = new object();
        private BackgroundWorker worker;
        private bool isMetric;
        private Color colorRed = Color.FromRgb(255, 0, 0);
        private Color colorBlue = Color.FromRgb(0, 0, 255);
        private Color colorGreen = Color.FromRgb(0, 255, 0);
        private Color colorOrange = Color.FromRgb(255, 127, 39);
        private Color colorPurple = Color.FromRgb(128, 0, 64);
        private Color colorBrown = Color.FromRgb(128, 64, 0);
        private Color colorBlack = Color.FromRgb(0, 0, 0);
        private Color customCurveColor = Color.FromRgb(0, 0, 0);
        private GrowthReference reference;
        private GrowthMeasurement measurement;
        private Gender gender;
        private string patientLabel;
        private string measurementColumnName = string.Empty;
        private double xAxisModifier = 1;
        private double yAxisModifier = 1;
        private bool useCustomCurveColors = false;
        private bool showLegend = true;
        private bool showTallChart = true;
        private bool isAgeBasedMeasurement = true;
        #endregion // Private Members

        #region Events
        public event GadgetRefreshedHandler GadgetRefreshed;
        public event GadgetClosingHandler GadgetClosing;
        public event GadgetProcessingFinishedHandler GadgetProcessingFinished;
        public event GadgetRepositionEventHandler GadgetReposition;
        public event GadgetCheckForCancellationHandler GadgetCheckForCancellation;
        public event GadgetEventHandler GadgetDragStart;
        public event GadgetEventHandler GadgetDragStop;
        public event GadgetEventHandler GadgetDrag;
        public event GadgetAnchorSetEventHandler GadgetAnchorSetFromXml;
        #endregion // Events

        private delegate void SimpleCallback();
        private delegate void RenderFinishDelegate(DataTable dt);
        private delegate void RenderFinishWithErrorDelegate(string message);

        #region Constructors
        public NutritionChartControl(DashboardHelper dashboardHelper)
        {
            InitializeComponent();
            this.dashboardHelper = dashboardHelper;
            Construct();
        }

        public NutritionChartControl(DashboardHelper dashboardHelper, GrowthMeasurement measurement, GrowthReference reference)
        {
            InitializeComponent();
            this.measurement = measurement;
            this.reference = reference;
            this.dashboardHelper = dashboardHelper;
            Construct();
        }

        private void SetReference()
        {
            switch (this.reference)
            {
                case GrowthReference.CDC2000:
                    grpHeader.Text = "CDC 2000 Growth Reference";
                    break;
                case GrowthReference.CDCRecommended:
                    grpHeader.Text = "CDC Recommended Growth References";
                    break;
                case GrowthReference.WHO1978:
                    grpHeader.Text = "CDC/WHO 1978 Growth Reference";
                    break;
                case GrowthReference.WHO2006:
                    grpHeader.Text = "WHO Child Growth Standards";
                    break;
                case GrowthReference.WHO2007:
                    grpHeader.Text = "WHO Reference 2007";
                    break;
            }
        }

        private void SetMeasurement()
        {
            switch (this.measurement)
            {
                case GrowthMeasurement.BodyMassIndex:
                    cbxAgeField.Visibility = System.Windows.Visibility.Visible;
                    txtAgeField.Visibility = System.Windows.Visibility.Visible;
                    cbxMeasurementField.Visibility = System.Windows.Visibility.Visible;
                    txtMeasurementField.Text = "Body mass index (BMI) field:";
                    grpHeader.Text = grpHeader.Text + " (Body mass index for age)";
                    IsAgeBasedMeasurement = true;
                    break;
                case GrowthMeasurement.HeadCircumferenceAge:
                    cbxAgeField.Visibility = System.Windows.Visibility.Visible;
                    txtAgeField.Visibility = System.Windows.Visibility.Visible;
                    cbxMeasurementField.Visibility = System.Windows.Visibility.Visible;
                    txtMeasurementField.Text = "Head circumference field:";
                    grpHeader.Text = grpHeader.Text + " (Head circumference for age)";
                    IsAgeBasedMeasurement = true;
                    break;
                case GrowthMeasurement.HeightAge:
                    cbxAgeField.Visibility = System.Windows.Visibility.Visible;
                    txtAgeField.Visibility = System.Windows.Visibility.Visible;
                    cbxMeasurementField.Visibility = System.Windows.Visibility.Visible;
                    txtMeasurementField.Text = "Height field:";
                    grpHeader.Text = grpHeader.Text + " (Height for age)";
                    IsAgeBasedMeasurement = true;
                    break;
                case GrowthMeasurement.LengthAge:
                    cbxAgeField.Visibility = System.Windows.Visibility.Visible;
                    txtAgeField.Visibility = System.Windows.Visibility.Visible;
                    cbxMeasurementField.Visibility = System.Windows.Visibility.Visible;
                    txtMeasurementField.Text = "Length field:";
                    grpHeader.Text = grpHeader.Text + " (Length for age)";
                    IsAgeBasedMeasurement = true;
                    break;
                case GrowthMeasurement.WeightAge:
                    cbxAgeField.Visibility = System.Windows.Visibility.Visible;
                    txtAgeField.Visibility = System.Windows.Visibility.Visible;
                    cbxMeasurementField.Visibility = System.Windows.Visibility.Visible;
                    txtMeasurementField.Text = "Weight field:";
                    grpHeader.Text = grpHeader.Text + " (Weight for age)";
                    IsAgeBasedMeasurement = true;
                    break;
                case GrowthMeasurement.WeightHeight:
                    cbxHeightField.Visibility = System.Windows.Visibility.Visible;
                    txtHeightField.Visibility = System.Windows.Visibility.Visible;
                    cbxMeasurementField.Visibility = System.Windows.Visibility.Visible;
                    txtMeasurementField.Text = "Weight field:";
                    grpHeader.Text = grpHeader.Text + " (Weight for height)";
                    IsAgeBasedMeasurement = false;
                    break;
                case GrowthMeasurement.WeightLength:
                    cbxHeightField.Visibility = System.Windows.Visibility.Visible;
                    txtHeightField.Visibility = System.Windows.Visibility.Visible;
                    cbxMeasurementField.Visibility = System.Windows.Visibility.Visible;
                    txtMeasurementField.Text = "Weight field:";
                    grpHeader.Text = grpHeader.Text + " (Weight for length)";
                    IsAgeBasedMeasurement = false;
                    break;
            }
        }

        private void Construct()
        {
            SetReference();
            SetMeasurement();

            grpHeader.Text = string.Empty;
            plotter.Visibility = System.Windows.Visibility.Collapsed;

            ConfigCollapsedTriangle.MouseLeftButtonUp += new MouseButtonEventHandler(Triangle_MouseLeftButtonUp);
            ConfigExpandedTriangle.MouseLeftButtonUp += new MouseButtonEventHandler(Triangle_MouseLeftButtonUp);

            mnuSave.Click += new RoutedEventHandler(mnuSave_Click);
            mnuLabel.Click += new RoutedEventHandler(mnuLabel_Click);

            imgClose.MouseEnter += new MouseEventHandler(imgClose_MouseEnter);
            imgClose.MouseLeave += new MouseEventHandler(imgClose_MouseLeave);
            imgClose.MouseDown += new MouseButtonEventHandler(imgClose_MouseDown);

            FillComboboxes();

            //cbxGrowthReference.SelectionChanged += new SelectionChangedEventHandler(cbxGrowthReference_SelectionChanged);
            //cbxMeasurement.SelectionChanged += new SelectionChangedEventHandler(cbxMeasurement_SelectionChanged);
            cbxPatientId.SelectionChanged += new SelectionChangedEventHandler(cbxPatientId_SelectionChanged);
            cbxPatientIdField.SelectionChanged += new SelectionChangedEventHandler(cbxPatientIdField_SelectionChanged);

            this.IsProcessing = false;
            this.isMetric = false;

            //this.GadgetStatusUpdate += new GadgetStatusUpdateHandler(RequestUpdateStatusMessage);
            //this.GadgetCheckForCancellation += new GadgetCheckForCancellationHandler(IsCancelled);

            gadgetOptions = new GadgetParameters();
            gadgetOptions.ShouldIncludeFullSummaryStatistics = false;
            gadgetOptions.ShouldIncludeMissing = false;
            gadgetOptions.ShouldSortHighToLow = false;
            gadgetOptions.ShouldUseAllPossibleValues = false;
            gadgetOptions.StrataVariableNames = new List<string>();
        }
        #endregion // Constructors

        #region Event Handlers
        void cbxPatientId_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            loadingCombos = true;
            
            loadingCombos = false;
        }

        void cbxPatientIdField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            loadingCombos = true;
            if (cbxPatientIdField.SelectedIndex > -1)
            {
                List<string> distinctValues = dashboardHelper.GetDistinctValuesAsList(cbxPatientIdField.SelectedItem.ToString());
                distinctValues.Sort();
                cbxPatientId.ItemsSource = distinctValues;
            }
            loadingCombos = false;
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            RefreshResults();
        }

        void imgClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (GadgetClosing != null)
                GadgetClosing(this);
        }

        void imgClose_MouseLeave(object sender, MouseEventArgs e)
        {
            Uri uriSource = new Uri("Images/x.png", UriKind.Relative);
            imgClose.Source = new BitmapImage(uriSource);
        }

        void imgClose_MouseEnter(object sender, MouseEventArgs e)
        {
            Uri uriSource = new Uri("Images/x_over.png", UriKind.Relative);
            imgClose.Source = new BitmapImage(uriSource);
        }

        void mnuLabel_Click(object sender, RoutedEventArgs e)
        {
            
        }

        void mnuSave_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "JPEG Image (.jpg)|*.jpg";

            if (dlg.ShowDialog().Value)
            {
                BitmapSource img = (BitmapSource)ToImageSource(pnlChartContainer);

                FileStream stream = new FileStream(dlg.SafeFileName, FileMode.Create);
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(img));
                encoder.Save(stream);
                stream.Close();
            }
        }
        #endregion // Event Handlers

        #region Private Events

        private bool ShowTallChart
        {
            get
            {
                return this.showTallChart;
            }
            set
            {
                showTallChart = value;
            }
        }

        private bool ShowLegend
        {
            get
            {
                return this.showLegend;
            }
            set
            {
                showLegend = value;
            }
        }

        private bool UseCustomCurveColors
        {
            get
            {
                return this.useCustomCurveColors;
            }
            set
            {
                useCustomCurveColors = value;
            }
        }

        private Color CustomCurveColor
        {
            get
            {
                return this.customCurveColor;
            }
            set
            {
                this.customCurveColor = value;
            }
        }

        private bool IsMetric
        {
            get
            {
                return this.isMetric;
            }
            set
            {
                isMetric = value;
            }
        }
        #endregion // Private Events

        public UserControl AnchorLeft { get; set; }
        public UserControl AnchorTop { get; set; }
        public UserControl AnchorBottom { get; set; }
        public UserControl AnchorRight { get; set; }
        public Guid UniqueIdentifier { get; private set; }

        private bool IsAgeBasedMeasurement
        {
            get
            {
                return this.isAgeBasedMeasurement;
            }
            set
            {
                this.isAgeBasedMeasurement = value;
            }
        }

        /// <summary>
        /// Updates the variable names available in the gadget's properties
        /// </summary>
        public void UpdateVariableNames()
        {
            //FillComboboxes(true);
        }

        public void RefreshResults()
        {
            if (!loadingCombos &&                
                cbxPatientIdField.SelectedIndex >= 0 &&
                cbxPatientId.SelectedIndex >= 0 &&
                cbxGenderField.SelectedIndex >= 0 &&                
                cbxMeasurementField.SelectedIndex >= 0)
            {
                if (IsAgeBasedMeasurement && cbxAgeField.SelectedIndex == -1)
                {
                    return;
                }

                txtError.Text = string.Empty;
                txtError.Visibility = Visibility.Collapsed;

                SetGadgetToProcessingState();

                ShowLegend = (bool)checkboxShowLegend.IsChecked;
                ShowTallChart = (bool)checkboxShowTallChart.IsChecked;
                UseCustomCurveColors = (bool)checkboxBlackAndWhite.IsChecked;

                if (UseCustomCurveColors)
                {
                    CustomCurveColor = Color.FromRgb(0, 0, 0);
                }

                if (cbxUnits.SelectedIndex == 0)
                {
                    IsMetric = false;
                }
                else
                {
                    IsMetric = true;
                }

                Thickness margin = pnlChartContainer.Margin;
                margin.Top = 40;
                pnlChartContainer.Margin = margin;

                List<string> columnNames = new List<string>();
                string weightColumnName = string.Empty;
                string heightColumnName = string.Empty;
                string ageColumnName = string.Empty;
                string measurementColumnName = cbxMeasurementField.SelectedItem.ToString();
                if (cbxWeightField.SelectedIndex > -1)
                {
                    weightColumnName = cbxWeightField.SelectedItem.ToString();
                    columnNames.Add(weightColumnName);
                }
                if (cbxAgeField.SelectedIndex > -1)
                {
                    ageColumnName = cbxAgeField.SelectedItem.ToString();
                    columnNames.Add(ageColumnName);
                }
                if (cbxHeightField.SelectedIndex > -1)
                {
                    heightColumnName = cbxHeightField.SelectedItem.ToString();
                    columnNames.Add(heightColumnName);
                }
                string genderColumnName = cbxGenderField.SelectedItem.ToString();
                string patientIdColumnName = cbxPatientIdField.SelectedItem.ToString();

                patientLabel = cbxPatientId.SelectedItem.ToString();

                ChartSubTitle.Content = "ID: " + patientLabel;

                string customFilter = "([" + patientIdColumnName + "] = " + dashboardHelper.FormatValue(cbxPatientId.SelectedItem.ToString(), dashboardHelper.GetColumnType(patientIdColumnName)) + ")";
                                
                columnNames.Add(genderColumnName);
                columnNames.Add(patientIdColumnName);
                columnNames.Add(measurementColumnName);

                gadgetOptions.InputVariableList.Clear();
                gadgetOptions.InputVariableList.Add("showtallchart", ShowTallChart.ToString());
                gadgetOptions.InputVariableList.Add("gendercolumnname", genderColumnName);
                gadgetOptions.InputVariableList.Add("agecolumnname", ageColumnName);
                gadgetOptions.InputVariableList.Add("heightcolumnname", heightColumnName);
                gadgetOptions.InputVariableList.Add("weightcolumnname", weightColumnName);
                gadgetOptions.InputVariableList.Add("measurementcolumnname", measurementColumnName);
                gadgetOptions.InputVariableList.Add("patientidcolumnname", patientIdColumnName);

                KeyValuePair<List<string>, string> inputs = new KeyValuePair<List<string>, string>(columnNames, customFilter);

                worker = new BackgroundWorker();
                worker.DoWork += new System.ComponentModel.DoWorkEventHandler(Execute);
                worker.RunWorkerAsync(inputs);
            }            
        }

        /// <summary>
        /// Tells the gadget to begin processing output on a new worker thread
        /// </summary>
        /// <param name="sender">Object that fired the event</param>
        /// <param name="e">.NET supplied event parameters</param>
        private void Execute(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                KeyValuePair<List<string>, string> kvp = ((KeyValuePair<List<string>, string>)e.Argument);

                List<string> columnNames = kvp.Key;
                string customFilter = kvp.Value;

                DataTable dt = dashboardHelper.GenerateTable(columnNames, customFilter);
                string sortBy = string.Empty;

                if (gadgetOptions.InputVariableList.ContainsKey("agecolumnname"))
                {
                    sortBy = gadgetOptions.InputVariableList["agecolumnname"];
                    dt = dt.Select("", sortBy).CopyToDataTable();
                }

                if (dt.Rows.Count > 0)
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishDelegate(RenderFinish), dt);
                }
                else
                {
                    this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), DashboardSharedStrings.GADGET_MSG_NO_DATA);
                }
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(new RenderFinishWithErrorDelegate(RenderFinishWithError), ex.Message);
            }
        }

        /// <summary>
        /// Sets the gadget's state to 'finished with error' mode
        /// </summary>
        /// <param name="errorMessage">The error message to display</param>
        private void RenderFinishWithError(string errorMessage)
        {
            waitCursor.Visibility = Visibility.Hidden;
            
            txtError.Visibility = System.Windows.Visibility.Visible;
            txtError.Text = errorMessage;

            pnlChartContainer.Children.Clear();
        }

        private void SetStatusMessage(string statusMessage)
        {
            txtError.Visibility = System.Windows.Visibility.Visible;
            txtError.Text = statusMessage;
        }

        private void RenderFinish(DataTable dt)
        {
            txtError.Visibility = System.Windows.Visibility.Hidden;
            txtError.Text = string.Empty;

            string genderColumnName = gadgetOptions.InputVariableList["gendercolumnname"];
            string ageColumnName = gadgetOptions.InputVariableList["agecolumnname"];
            string heightColumnName = gadgetOptions.InputVariableList["heightcolumnname"];
            string weightColumnName = gadgetOptions.InputVariableList["weightcolumnname"];
            measurementColumnName = gadgetOptions.InputVariableList["measurementcolumnname"];

            string genderStr = dt.Rows[0][genderColumnName].ToString();
            if (genderStr.ToLowerInvariant().Equals("f"))
            {
                this.gender = Gender.Female;
            }
            else if (genderStr.ToLowerInvariant().Equals("m"))
            {
                this.gender = Gender.Male;
            }

            string xAxisColumnName = ageColumnName;
            string yAxisColumnName = measurementColumnName;

            switch (this.measurement)
            {
                case GrowthMeasurement.BodyMassIndex:
                    xAxisColumnName = ageColumnName;
                    break;
                case GrowthMeasurement.HeadCircumferenceAge:
                    xAxisColumnName = ageColumnName;
                    break;
                case GrowthMeasurement.HeightAge:
                    xAxisColumnName = ageColumnName;
                    break;
                case GrowthMeasurement.LengthAge:
                    xAxisColumnName = ageColumnName;
                    break;
                case GrowthMeasurement.WeightAge:
                    xAxisColumnName = ageColumnName;
                    break;
                case GrowthMeasurement.WeightHeight:
                    xAxisColumnName = heightColumnName;
                    break;
                case GrowthMeasurement.WeightLength:
                    xAxisColumnName = heightColumnName;
                    break;
            }

            List<double> xAxisData = new List<double>();
            List<double> yAxisData = new List<double>();

            try
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (!string.IsNullOrEmpty(row[xAxisColumnName].ToString()) && !string.IsNullOrEmpty(row[yAxisColumnName].ToString()))
                    {
                        xAxisData.Add(double.Parse(row[xAxisColumnName].ToString()));
                        yAxisData.Add(double.Parse(row[yAxisColumnName].ToString()));
                    }
                }
            }
            catch (FormatException)
            {
            }
            catch (ArgumentException ex)
            {
                SetStatusMessage(ex.Message);
                return;
            }


            if ((xAxisData.Count == 0 && yAxisData.Count == 0) || (xAxisData.Count != yAxisData.Count))
            {
                RenderFinishWithError(DashboardSharedStrings.GADGET_MSG_NO_DATA);
                SetGadgetToFinishedState();
                CollapseExpandConfigPanel();
                return;
            }

            List<IPlotterElement> graphsToRemove = new List<IPlotterElement>();
            foreach (IPlotterElement element in plotter.Children)
            {
                if (element is LineGraph || element is MarkerPointsGraph)
                {
                    graphsToRemove.Add(element);
                }
            }

            foreach (IPlotterElement element in graphsToRemove)
            {
                plotter.Children.Remove(element);
            }

            grdChart.Width = 800;
            pnlChartContainer.Width = 800;

            if (ShowTallChart)
            {
                grdChart.Height = 860;
                pnlChartContainer.Height = 860;
            }
            else
            {
                grdChart.Height = 560;
                pnlChartContainer.Height = 560;
            }

            PlotPercentiles(yAxisData, xAxisData, null);

            plotter.FitToView();
            if (ShowTallChart)
            {
                plotter.Height = 800;
            }
            else
            {
                plotter.Height = 500;
            }
            SetGadgetToFinishedState();
        }

        private List<GrowthCurvesPercentiles> LoadGrowthCurvesPercentiles()
        {
            var result = new List<GrowthCurvesPercentiles>();

            xAxisModifier = 1;
            yAxisModifier = 1;

            double[,] array = null;
            int lengthModifier = 11;
            
            string genderStr = "Male";
            string lengthUnits = "in";
            string weightUnits = "lbs";

            if (IsMetric)
            {
                lengthUnits = "cm";
                weightUnits = "kg";
            }

            if (gender.Equals(Gender.Female))
            {
                genderStr = "Female";
            }
            switch (measurement)
            {
                case GrowthMeasurement.BodyMassIndex:
                    //yAxisModifier = 0.393700787;
                    ChartTitle.Content = "Body mass index for age" + " (" + genderStr + ")";
                    yAxisTitle.Content = "Body mass index (BMI)";
                    xAxisTitle.Content = "Age (months)";
                    switch (reference)
                    {
                        case GrowthReference.CDC2000:
                            ChartTitle.Content = "CDC 2000 - " + ChartTitle.Content;
                            lengthModifier = 11;
                            switch (gender)
                            {
                                case Gender.Male:
                                    array = PercentileArrays.CDCBMI_male;
                                    break;
                                default:
                                    array = PercentileArrays.CDCBMI_female;
                                    break;
                            }
                            break;
                        case GrowthReference.WHO2006:
                            ChartTitle.Content = "WHO Child Growth Standards - " + ChartTitle.Content;
                            lengthModifier = 8;
                            switch (gender)
                            {
                                case Gender.Male:
                                    array = PercentileArrays.WHO2006BMI_male;
                                    break;
                                default:
                                    array = PercentileArrays.WHO2006BMI_female;
                                    break;
                            }
                            break;
                        case GrowthReference.WHO2007:
                            ChartTitle.Content = "WHO Reference 2007 - " + ChartTitle.Content;
                            lengthModifier = 16;
                            switch (gender)
                            {
                                case Gender.Male:
                                    array = PercentileArrays.WHO2007BMI_male;
                                    break;
                                default:
                                    array = PercentileArrays.WHO2007BMI_female;
                                    break;
                            }
                            break;
                        case GrowthReference.WHO1978:
                            return null; // WHO 1978 ref has no BMI
                    }
                    break;
                case GrowthMeasurement.HeightAge:
                    if (!IsMetric)
                    {
                        yAxisModifier = 0.393700787;
                    }
                    ChartTitle.Content = "Height for age" + " (" + genderStr + ")";
                    yAxisTitle.Content = "Height (" + lengthUnits + ")";
                    xAxisTitle.Content = "Age (months)";
                    switch (reference)
                    {
                        case GrowthReference.CDC2000:
                            ChartTitle.Content = "CDC 2000 - " + ChartTitle.Content;
                            lengthModifier = 10;
                            switch (gender)
                            {
                                case Gender.Male:
                                    array = PercentileArrays.CDCHA_male;
                                    break;
                                default:
                                    array = PercentileArrays.CDCHA_female;
                                    break;
                            }
                            break;
                        case GrowthReference.WHO2006:
                            ChartTitle.Content = "WHO Child Growth Standards - " + ChartTitle.Content;
                            lengthModifier = 8;
                            switch (gender)
                            {
                                case Gender.Male:
                                    array = PercentileArrays.WHO2006HA_male;
                                    break;
                                default:
                                    array = PercentileArrays.WHO2006HA_female;
                                    break;
                            }
                            break;
                        case GrowthReference.WHO2007:
                            ChartTitle.Content = "WHO Reference 2007 - " + ChartTitle.Content;
                            lengthModifier = 16;
                            switch (gender)
                            {
                                case Gender.Male:
                                    array = PercentileArrays.WHO2007HA_male;
                                    break;
                                default:
                                    array = PercentileArrays.WHO2007HA_female;
                                    break;
                            }
                            break;
                        case GrowthReference.WHO1978:
                            ChartTitle.Content = "CDC/WHO 1978 - " + ChartTitle.Content;
                            lengthModifier = 10;
                            switch (gender)
                            {
                                case Gender.Male:
                                    array = PercentileArrays.WHO1978HA_male;
                                    break;
                                default:
                                    array = PercentileArrays.WHO1978HA_female;
                                    break;
                            }
                            break;
                    }
                    break;
                case GrowthMeasurement.HeadCircumferenceAge:
                    if (!IsMetric)
                    {
                        yAxisModifier = 0.393700787;
                    }
                    ChartTitle.Content = "Head Circumference for age" + " (" + genderStr + ")";
                    yAxisTitle.Content = "Head Circumference (" + lengthUnits + ")";
                    xAxisTitle.Content = "Age (months)";
                    switch (reference)
                    {
                        case GrowthReference.CDC2000:
                            ChartTitle.Content = "CDC 2000 - " + ChartTitle.Content;
                            lengthModifier = 10;
                            switch (gender)
                            {
                                case Gender.Male:
                                    array = PercentileArrays.CDCHCA_male;
                                    break;
                                default:
                                    array = PercentileArrays.CDCHCA_female;
                                    break;
                            }
                            break;
                        case GrowthReference.WHO2006:
                            ChartTitle.Content = "WHO Child Growth Standards - " + ChartTitle.Content;
                            lengthModifier = 8;
                            switch (gender)
                            {
                                case Gender.Male:
                                    array = PercentileArrays.WHO2006HCA_male;
                                    break;
                                default:
                                    array = PercentileArrays.WHO2006HCA_female;
                                    break;
                            }
                            break;
                        case GrowthReference.WHO1978:
                            return null; // WHO 1978 ref has no HCA
                    }
                    break;
                case GrowthMeasurement.LengthAge:
                    if (!IsMetric)
                    {
                        yAxisModifier = 0.393700787;
                    }
                    ChartTitle.Content = "Length for age" + " (" + genderStr + ")";
                    yAxisTitle.Content = "Length (" + lengthUnits + ")";
                    xAxisTitle.Content = "Age (months)";
                    switch (reference)
                    {
                        case GrowthReference.CDC2000:
                            ChartTitle.Content = "CDC 2000 - " + ChartTitle.Content;
                            lengthModifier = 10;
                            switch (gender)
                            {
                                case Gender.Male:
                                    array = PercentileArrays.CDCLA_male;
                                    break;
                                default:
                                    array = PercentileArrays.CDCLA_female;
                                    break;
                            }
                            break;
                        case GrowthReference.WHO2006:
                            ChartTitle.Content = "WHO Child Growth Standards - " + ChartTitle.Content;
                            lengthModifier = 8;
                            switch (gender)
                            {
                                case Gender.Male:
                                    array = PercentileArrays.WHO2006LA_male;
                                    break;
                                default:
                                    array = PercentileArrays.WHO2006LA_female;
                                    break;
                            }
                            break;
                        case GrowthReference.WHO1978:
                            ChartTitle.Content = "CDC/WHO 1978 - " + ChartTitle.Content;
                            lengthModifier = 10;
                            switch (gender)
                            {
                                case Gender.Male:
                                    array = PercentileArrays.WHO1978LA_male;
                                    break;
                                default:
                                    array = PercentileArrays.WHO1978LA_female;
                                    break;
                            }
                            break;
                    }
                    break;
                case GrowthMeasurement.WeightAge:
                    if (!IsMetric)
                    {
                        yAxisModifier = 2.20462262;
                    }                    
                    ChartTitle.Content = "Weight for age" + " (" + genderStr + ")";
                    yAxisTitle.Content = "Weight (" + weightUnits + ")";
                    xAxisTitle.Content = "Age (months)";
                    switch (reference)
                    {
                        case GrowthReference.CDC2000:
                            ChartTitle.Content = "CDC 2000 - " + ChartTitle.Content;
                            lengthModifier = 10;
                            switch (gender)
                            {
                                case Gender.Male:
                                    array = PercentileArrays.CDCWA_male;
                                    break;
                                default:
                                    array = PercentileArrays.CDCWA_female;
                                    break;
                            }
                            break;
                        case GrowthReference.WHO2006:
                            ChartTitle.Content = "WHO Child Growth Standards - " + ChartTitle.Content;
                            lengthModifier = 8;
                            switch (gender)
                            {
                                case Gender.Male:
                                    array = PercentileArrays.WHO2006WA_male;
                                    break;
                                default:
                                    array = PercentileArrays.WHO2006WA_female;
                                    break;
                            }
                            break;
                        case GrowthReference.WHO2007:
                            ChartTitle.Content = "WHO Reference 2007 - " + ChartTitle.Content;
                            lengthModifier = 16;
                            switch (gender)
                            {
                                case Gender.Male:
                                    array = PercentileArrays.WHO2007WA_male;
                                    break;
                                default:
                                    array = PercentileArrays.WHO2007WA_female;
                                    break;
                            }
                            break;
                        case GrowthReference.WHO1978:
                            ChartTitle.Content = "CDC/WHO 1978 - " + ChartTitle.Content;
                            lengthModifier = 10;
                            switch (gender)
                            {
                                case Gender.Male:
                                    array = PercentileArrays.WHO1978WA_male;
                                    break;
                                default:
                                    array = PercentileArrays.WHO1978WA_female;
                                    break;
                            }
                            break;
                    }
                    break;
                case GrowthMeasurement.WeightHeight:
                    if (!IsMetric)
                    {
                        yAxisModifier = 2.20462262;
                        xAxisModifier = 0.393700787;
                    }
                    ChartTitle.Content = "Weight for height" + " (" + genderStr + ")";
                    yAxisTitle.Content = "Weight (" + weightUnits + ")";
                    xAxisTitle.Content = "Height (" + lengthUnits + ")";
                    switch (reference)
                    {
                        case GrowthReference.CDC2000:
                            ChartTitle.Content = "CDC 2000 - " + ChartTitle.Content;
                            lengthModifier = 10;
                            switch (gender)
                            {
                                case Gender.Male:
                                    array = PercentileArrays.CDCWH_male;
                                    break;
                                default:
                                    array = PercentileArrays.CDCWH_female;
                                    break;
                            }
                            break;
                        case GrowthReference.WHO2006:
                            ChartTitle.Content = "WHO Child Growth Standards - " + ChartTitle.Content;
                            lengthModifier = 8;
                            switch (gender)
                            {
                                case Gender.Male:
                                    array = PercentileArrays.WHO2006WH_male;
                                    break;
                                default:
                                    array = PercentileArrays.WHO2006WH_female;
                                    break;
                            }
                            break;
                        case GrowthReference.WHO1978:
                            ChartTitle.Content = "CDC/WHO 1978 - " + ChartTitle.Content;
                            lengthModifier = 10;
                            switch (gender)
                            {
                                case Gender.Male:
                                    array = PercentileArrays.WHO1978WH_male;
                                    break;
                                default:
                                    array = PercentileArrays.WHO1978WH_female;
                                    break;
                            }
                            break;
                    }
                    break;
                case GrowthMeasurement.WeightLength:
                    if (!IsMetric)
                    {
                        yAxisModifier = 2.20462262;
                        xAxisModifier = 0.393700787;
                    }
                    ChartTitle.Content = "Weight for length" + " (" + genderStr + ")";
                    yAxisTitle.Content = "Weight (" + weightUnits + ")";
                    xAxisTitle.Content = "Height (" + lengthUnits + ")";
                    switch (reference)
                    {
                        case GrowthReference.CDC2000:
                            ChartTitle.Content = "CDC 2000 - " + ChartTitle.Content;
                            lengthModifier = 10;
                            switch (gender)
                            {
                                case Gender.Male:
                                    array = PercentileArrays.CDCWL_male;
                                    break;
                                default:
                                    array = PercentileArrays.CDCWL_female;
                                    break;
                            }
                            break;
                        case GrowthReference.WHO2006:
                            ChartTitle.Content = "WHO Child Growth Standards - " + ChartTitle.Content;
                            lengthModifier = 8;
                            switch (gender)
                            {
                                case Gender.Male:
                                    array = PercentileArrays.WHO2006WL_male;
                                    break;
                                default:
                                    array = PercentileArrays.WHO2006WL_female;
                                    break;
                            }
                            break;
                        case GrowthReference.WHO1978:
                            ChartTitle.Content = "CDC/WHO 1978 - " + ChartTitle.Content;
                            lengthModifier = 10;
                            switch (gender)
                            {
                                case Gender.Male:
                                    array = PercentileArrays.WHO1978WL_male;
                                    break;
                                default:
                                    array = null;//WHO1978WL_female;
                                    break;
                            }
                            break;
                    }
                    break;
            }
            

            for (int i = 0; i < (array.Length / lengthModifier); i++)
            {
                double age = xAxisModifier * array[i, 0];
                double c01 = 0;
                double c1 = 0;
                double c3 = 0;
                double c5 = 0;
                double c10 = 0;
                double c15 = 0;
                double c25 = 0;
                double c50 = 0;
                double c75 = 0;
                double c85 = 0;
                double c90 = 0;
                double c95 = 0;
                double c97 = 0;
                double c99 = 0;
                double c999 = 0;

                if (lengthModifier == 11 && reference.Equals(GrowthReference.CDC2000))
                {
                    c3 = array[i, 1] * yAxisModifier;
                    c5 = array[i, 2] * yAxisModifier;
                    c10 = array[i, 3] * yAxisModifier;
                    c25 = array[i, 4] * yAxisModifier;
                    c50 = array[i, 5] * yAxisModifier;
                    c75 = array[i, 6] * yAxisModifier;
                    c85 = array[i, 7] * yAxisModifier;
                    c90 = array[i, 8] * yAxisModifier;
                    c95 = array[i, 9] * yAxisModifier;
                    c97 = array[i, 10] * yAxisModifier;

                    GrowthCurvesPercentiles growthCurves = new GrowthCurvesPercentiles(c3, c5, c10, c25, c50, c75,
                        c85, c90, c95, c97, age);

                    result.Add(growthCurves);
                }
                else if (lengthModifier == 10)
                {
                    c3 = array[i, 1] * yAxisModifier;
                    c5 = array[i, 2] * yAxisModifier;
                    c10 = array[i, 3] * yAxisModifier;
                    c25 = array[i, 4] * yAxisModifier;
                    c50 = array[i, 5] * yAxisModifier;
                    c75 = array[i, 6] * yAxisModifier;
                    c90 = array[i, 7] * yAxisModifier;
                    c95 = array[i, 8] * yAxisModifier;
                    c97 = array[i, 9] * yAxisModifier;

                    GrowthCurvesPercentiles growthCurves = new GrowthCurvesPercentiles(c3, c5, c10, c25, c50, c75,
                        c90, c95, c97, age);
                    result.Add(growthCurves);
                }
                else if (lengthModifier == 8 && reference.Equals(GrowthReference.WHO2006))
                {
                    c1 = array[i, 1] * yAxisModifier;
                    c3 = array[i, 2] * yAxisModifier;
                    c15 = array[i, 3] * yAxisModifier;
                    c50 = array[i, 4] * yAxisModifier;
                    c85 = array[i, 5] * yAxisModifier;
                    c97 = array[i, 6] * yAxisModifier;
                    c99 = array[i, 7] * yAxisModifier;

                    GrowthCurvesPercentiles growthCurves = new GrowthCurvesPercentiles(c1, c3, c15, c50, c85, c97, c99, age);
                    result.Add(growthCurves);
                }
                if (lengthModifier == 16)
                {
                    c01 = array[i, 1] * yAxisModifier;
                    c1 = array[i, 2] * yAxisModifier;
                    c3 = array[i, 3] * yAxisModifier;
                    c5 = array[i, 4] * yAxisModifier;
                    c10 = array[i, 5] * yAxisModifier;
                    c15 = array[i, 6] * yAxisModifier;
                    c25 = array[i, 7] * yAxisModifier;
                    c50 = array[i, 8] * yAxisModifier;
                    c75 = array[i, 9] * yAxisModifier;
                    c85 = array[i, 10] * yAxisModifier;
                    c90 = array[i, 11] * yAxisModifier;
                    c95 = array[i, 12] * yAxisModifier;
                    c97 = array[i, 13] * yAxisModifier;
                    c99 = array[i, 14] * yAxisModifier;
                    c999 = array[i, 15] * yAxisModifier;

                    GrowthCurvesPercentiles growthCurves = new GrowthCurvesPercentiles(c01, c1, c3, c5, c10, c15, c25, c50, c75,
                        c85, c90, c95, c97, c99, c999, age);

                    result.Add(growthCurves);
                }
            }

            return result;
        }

        private void PlotPercentiles(List<double> measure, List<double> age, List<double> score)
        {
            try
            {
                bool useCustomColor = UseCustomCurveColors;

                plotter.Visibility = System.Windows.Visibility.Visible;
                //plotter.RemoveUserElements();// = new ChartPlotter();
                // load growth curves from the appropriate file
                List<GrowthCurvesPercentiles> percentilesList = LoadGrowthCurvesPercentiles();

                double[] C01 = new double[percentilesList.Count];
                double[] C1 = new double[percentilesList.Count];
                double[] C3 = new double[percentilesList.Count];
                double[] C5 = new double[percentilesList.Count];
                double[] C10 = new double[percentilesList.Count];
                double[] C15 = new double[percentilesList.Count];
                double[] C25 = new double[percentilesList.Count];
                double[] C50 = new double[percentilesList.Count];
                double[] C75 = new double[percentilesList.Count];
                double[] C85 = new double[percentilesList.Count];
                double[] C90 = new double[percentilesList.Count];
                double[] C95 = new double[percentilesList.Count];
                double[] C97 = new double[percentilesList.Count];
                double[] C99 = new double[percentilesList.Count];
                double[] C999 = new double[percentilesList.Count];
                double[] ages = new double[percentilesList.Count];

                if (percentilesList.Count <= 0)
                {
                    return;
                }

                for (int i = 0; i < percentilesList.Count; ++i)
                {
                    C01[i] = percentilesList[i].C01;
                    C1[i] = percentilesList[i].C1;
                    C3[i] = percentilesList[i].C3;
                    C5[i] = percentilesList[i].C5;
                    C10[i] = percentilesList[i].C10;
                    C15[i] = percentilesList[i].C15;
                    C25[i] = percentilesList[i].C25;
                    C50[i] = percentilesList[i].C50;
                    C75[i] = percentilesList[i].C75;
                    C85[i] = percentilesList[i].C85;
                    C90[i] = percentilesList[i].C90;
                    C95[i] = percentilesList[i].C95;
                    C97[i] = percentilesList[i].C97;
                    C99[i] = percentilesList[i].C99;
                    C999[i] = percentilesList[i].C999;
                    ages[i] = percentilesList[i].xaxis;
                }

                var C01DataSource = new EnumerableDataSource<double>(C01);
                C01DataSource.SetYMapping(y => y);

                var C1DataSource = new EnumerableDataSource<double>(C1);
                C1DataSource.SetYMapping(y => y);

                var C3DataSource = new EnumerableDataSource<double>(C3);
                C3DataSource.SetYMapping(y => y);

                var C5DataSource = new EnumerableDataSource<double>(C5);
                C5DataSource.SetYMapping(y => y);

                var C10DataSource = new EnumerableDataSource<double>(C10);
                C10DataSource.SetYMapping(y => y);

                var C15DataSource = new EnumerableDataSource<double>(C15);
                C15DataSource.SetYMapping(y => y);

                var C25DataSource = new EnumerableDataSource<double>(C25);
                C25DataSource.SetYMapping(y => y);

                var C50DataSource = new EnumerableDataSource<double>(C50);
                C50DataSource.SetYMapping(y => y);

                var C75DataSource = new EnumerableDataSource<double>(C75);
                C75DataSource.SetYMapping(y => y);

                var C85DataSource = new EnumerableDataSource<double>(C85);
                C85DataSource.SetYMapping(y => y);

                var C90DataSource = new EnumerableDataSource<double>(C90);
                C90DataSource.SetYMapping(y => y);

                var C95DataSource = new EnumerableDataSource<double>(C95);
                C95DataSource.SetYMapping(y => y);

                var C97DataSource = new EnumerableDataSource<double>(C97);
                C97DataSource.SetYMapping(y => y);

                var C99DataSource = new EnumerableDataSource<double>(C99);
                C99DataSource.SetYMapping(y => y);

                var C999DataSource = new EnumerableDataSource<double>(C999);
                C999DataSource.SetYMapping(y => y);

                var ageDataSource = new EnumerableDataSource<double>(ages);
                ageDataSource.SetXMapping(x => x);

                var ageMeasurementDataSource = new EnumerableDataSource<double>(age);
                ageMeasurementDataSource.SetXMapping(x => x);

                //var scoreDataSource = new EnumerableDataSource<double>(score);
                //scoreDataSource.SetXYMapping(

                var measureDataSource = new EnumerableDataSource<double>(measure);
                measureDataSource.SetYMapping(y => y);

                CompositeDataSource compositeDataSource0 = new
                  CompositeDataSource(ageDataSource, C01DataSource);
                CompositeDataSource compositeDataSource1 = new
                  CompositeDataSource(ageDataSource, C1DataSource);
                CompositeDataSource compositeDataSource2 = new
                  CompositeDataSource(ageDataSource, C3DataSource);
                CompositeDataSource compositeDataSource3 = new
                  CompositeDataSource(ageDataSource, C5DataSource);
                CompositeDataSource compositeDataSource4 = new
                  CompositeDataSource(ageDataSource, C10DataSource);
                CompositeDataSource compositeDataSource5 = new
                  CompositeDataSource(ageDataSource, C15DataSource);
                CompositeDataSource compositeDataSource6 = new
                  CompositeDataSource(ageDataSource, C25DataSource);
                CompositeDataSource compositeDataSource7 = new
                  CompositeDataSource(ageDataSource, C50DataSource);
                CompositeDataSource compositeDataSource8 = new
                  CompositeDataSource(ageDataSource, C75DataSource);
                CompositeDataSource compositeDataSource9 = new
                  CompositeDataSource(ageDataSource, C85DataSource);
                CompositeDataSource compositeDataSource10 = new
                  CompositeDataSource(ageDataSource, C90DataSource);
                CompositeDataSource compositeDataSource11 = new
                  CompositeDataSource(ageDataSource, C95DataSource);
                CompositeDataSource compositeDataSource12 = new
                  CompositeDataSource(ageDataSource, C97DataSource);
                CompositeDataSource compositeDataSource13 = new
                  CompositeDataSource(ageDataSource, C99DataSource);
                CompositeDataSource compositeDataSource14 = new
                  CompositeDataSource(ageDataSource, C999DataSource);

                CompositeDataSource compositeDataSourceMeasurement = new
                  CompositeDataSource(ageMeasurementDataSource, measureDataSource);

                if (C999[1] > 0)
                {
                    if (useCustomColor)
                    {
                        plotter.AddLineGraph(compositeDataSource14,
                            new Pen(new SolidColorBrush(CustomCurveColor), 1),
                            new PenDescription("99.9th Percentile"));
                    }
                    else
                    {
                        plotter.AddLineGraph(compositeDataSource14,
                            new Pen(new SolidColorBrush(colorRed), 1),
                            new PenDescription("99.9th Percentile"));
                    }
                }

                if (C99[1] > 0)
                {
                    if (useCustomColor)
                    {
                        plotter.AddLineGraph(compositeDataSource13,
                            new Pen(new SolidColorBrush(CustomCurveColor), 1),
                            new PenDescription("99th Percentile"));
                    }
                    else
                    {
                        plotter.AddLineGraph(compositeDataSource13,
                            new Pen(new SolidColorBrush(colorBrown), 1),
                            new PenDescription("99th Percentile"));
                    }
                }

                if (C97[1] > 0)
                {
                    if (useCustomColor)
                    {
                        plotter.AddLineGraph(compositeDataSource12,
                          new Pen(new SolidColorBrush(CustomCurveColor), 1),
                          new PenDescription("97th Percentile"));
                    }
                    else
                    {
                        plotter.AddLineGraph(compositeDataSource12,
                          new Pen(new SolidColorBrush(colorRed), 1),
                          new PenDescription("97th Percentile"));
                    }
                }

                if (C95[1] > 0)
                {
                    if (useCustomColor)
                    {
                        plotter.AddLineGraph(compositeDataSource11,
                          new Pen(new SolidColorBrush(CustomCurveColor), 1),
                          new PenDescription("95th Percentile"));
                    }
                    else
                    {
                        plotter.AddLineGraph(compositeDataSource11,
                          new Pen(new SolidColorBrush(colorOrange), 1),
                          new PenDescription("95th Percentile"));
                    }
                }

                if (C90[1] > 0)
                {
                    if (useCustomColor)
                    {
                        plotter.AddLineGraph(compositeDataSource10,
                          new Pen(new SolidColorBrush(CustomCurveColor), 1),
                          new PenDescription("90th Percentile"));
                    }
                    else
                    {
                        plotter.AddLineGraph(compositeDataSource10,
                          new Pen(new SolidColorBrush(colorBrown), 1),
                          new PenDescription("90th Percentile"));
                    }
                }

                if (C85[1] > 0)
                {
                    if (useCustomColor)
                    {
                        plotter.AddLineGraph(compositeDataSource9,
                          new Pen(new SolidColorBrush(CustomCurveColor), 1),
                          new PenDescription("85th Percentile"));
                    }
                    else
                    {
                        plotter.AddLineGraph(compositeDataSource9,
                          new Pen(new SolidColorBrush(colorBlue), 1),
                          new PenDescription("85th Percentile"));
                    }
                }

                if (C75[1] > 0)
                {
                    if (useCustomColor)
                    {
                        plotter.AddLineGraph(compositeDataSource8,
                          new Pen(new SolidColorBrush(CustomCurveColor), 1),
                          new PenDescription("75th Percentile"));
                    }
                    else
                    {
                        plotter.AddLineGraph(compositeDataSource8,
                          new Pen(new SolidColorBrush(colorPurple), 1),
                          new PenDescription("75th Percentile"));
                    }
                }

                if (C50[1] > 0)
                {
                    if (useCustomColor)
                    {
                        plotter.AddLineGraph(compositeDataSource7,
                          new Pen(new SolidColorBrush(CustomCurveColor), 2),
                          new PenDescription("50th Percentile"));
                    }
                    else
                    {
                        plotter.AddLineGraph(compositeDataSource7,
                          new Pen(new SolidColorBrush(colorGreen), 1),
                          new PenDescription("50th Percentile"));
                    }
                }

                if (C25[1] > 0)
                {
                    if (useCustomColor)
                    {
                        plotter.AddLineGraph(compositeDataSource6,
                          new Pen(new SolidColorBrush(CustomCurveColor), 1),
                          new PenDescription("25th Percentile"));
                    }
                    else
                    {
                        plotter.AddLineGraph(compositeDataSource6,
                          new Pen(new SolidColorBrush(colorPurple), 1),
                          new PenDescription("25th Percentile"));
                    }
                }

                if (C15[1] > 0)
                {
                    if (useCustomColor)
                    {
                        plotter.AddLineGraph(compositeDataSource5,
                          new Pen(new SolidColorBrush(CustomCurveColor), 1),
                          new PenDescription("15th Percentile"));
                    }
                    else
                    {
                        plotter.AddLineGraph(compositeDataSource5,
                          new Pen(new SolidColorBrush(colorBlue), 1),
                          new PenDescription("15th Percentile"));
                    }
                }

                if (C10[1] > 0)
                {
                    if (useCustomColor)
                    {
                        plotter.AddLineGraph(compositeDataSource4,
                          new Pen(new SolidColorBrush(CustomCurveColor), 1),
                          new PenDescription("10th Percentile"));
                    }
                    else
                    {
                        plotter.AddLineGraph(compositeDataSource4,
                          new Pen(new SolidColorBrush(colorBrown), 1),
                          new PenDescription("10th Percentile"));
                    }
                }

                if (C5[1] > 0)
                {
                    if (useCustomColor)
                    {
                        plotter.AddLineGraph(compositeDataSource3,
                          new Pen(new SolidColorBrush(CustomCurveColor), 1),
                          new PenDescription("5th Percentile"));
                    }
                    else
                    {
                        plotter.AddLineGraph(compositeDataSource3,
                          new Pen(new SolidColorBrush(colorOrange), 1),
                          new PenDescription("5th Percentile"));
                    }
                }

                if (C3[1] > 0)
                {
                    if (useCustomColor)
                    {
                        plotter.AddLineGraph(compositeDataSource2,
                          new Pen(new SolidColorBrush(CustomCurveColor), 1),
                          new PenDescription("3rd Percentile"));
                    }
                    else
                    {
                        plotter.AddLineGraph(compositeDataSource2,
                          new Pen(new SolidColorBrush(colorRed), 1),
                          new PenDescription("3rd Percentile"));
                    }
                }

                if (C1[1] > 0)
                {
                    if (useCustomColor)
                    {
                        plotter.AddLineGraph(compositeDataSource1,
                      new Pen(new SolidColorBrush(CustomCurveColor), 1),
                      new PenDescription("1st Percentile"));
                    }
                    else
                    {
                        plotter.AddLineGraph(compositeDataSource1,
                      new Pen(new SolidColorBrush(colorBrown), 1),
                      new PenDescription("1st Percentile"));
                    }
                }

                if (C01[1] > 0)
                {
                    if (useCustomColor)
                    {
                        plotter.AddLineGraph(compositeDataSource0,
                      new Pen(new SolidColorBrush(CustomCurveColor), 1),
                      new PenDescription("0.1st Percentile"));
                    }
                    else
                    {
                        plotter.AddLineGraph(compositeDataSource0,
                      new Pen(new SolidColorBrush(colorRed), 1),
                      new PenDescription("0.1st Percentile"));
                    }
                }

                Pen patientPen = new Pen(new SolidColorBrush(colorBlack), 2);
                patientPen.DashStyle = DashStyles.Dash;

                    plotter.AddLineGraph(compositeDataSourceMeasurement,
                        patientPen,
                        new CirclePointMarker
                        {
                            Size = 5,
                            Pen = new Pen(new SolidColorBrush(colorBlack), 2),
                            Fill = Brushes.Transparent
                        },
                        new PenDescription(patientLabel));
                

                //if (config.ShowDataPointLabels)
                //{
                //    ///////////////////////////////                
                //    for (int i = 0; i < age.Count; i++)
                //    {
                //        List<double> ageLabel = new List<double>();
                //        List<double> measureLabel = new List<double>();
                //        string percentile = String.Format("{0:0.00}", score[i]);

                //        ageLabel.Add(age[i]);
                //        measureLabel.Add(measure[i]);

                //        var ageMeasurementDataSourceLabel = new EnumerableDataSource<double>(ageLabel);
                //        ageMeasurementDataSourceLabel.SetXMapping(x => x);

                //        var measureDataSourceLabel = new EnumerableDataSource<double>(measureLabel);
                //        measureDataSourceLabel.SetYMapping(y => y);

                //        ds.Add(new CompositeDataSource(ageMeasurementDataSourceLabel, measureDataSourceLabel));

                //        chart.Add(plotter.AddLineGraph(ds[i],
                //            null,
                //            new CenteredTextMarker
                //            {
                //                Text = " " + percentile + " "
                //            },
                //            new PenDescription(score[i].ToString())));
                //    }
                //    ////////////////////////////////
                //}

                //chart.MarkerGraph.DataSource = null;

                xAxis.ShowMinorTicks = true;
                yAxis.ShowMinorTicks = true;
                plotter.AxisGrid.DrawVerticalMinorTicks = false;
                plotter.AxisGrid.DrawHorizontalMinorTicks = false;
                plotter.Legend.LegendLeft = 45;
                plotter.Legend.LegendRight = Double.NaN;

                if (!ShowLegend)
                {
                    plotter.Legend.Visibility = Visibility.Hidden;
                }
                else
                {
                    plotter.Legend.Visibility = Visibility.Visible;
                }

                //if (config.ShowCoordinateCursor)
                //{
                    //plotter.Children.Add(new CursorCoordinateGraph());
                //}
                plotter.Viewport.FitToView();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public ImageSource ToImageSource(FrameworkElement obj)
        {
            // Save current canvas transform
            Transform transform = obj.LayoutTransform;

            // fix margin offset as well
            Thickness margin = obj.Margin;
            obj.Margin = new Thickness(0, 0,
                 margin.Right - margin.Left, margin.Bottom - margin.Top);

            // Get the size of canvas
            Size size = new Size(obj.ActualWidth, obj.ActualHeight);

            // force control to Update
            obj.Measure(size);
            obj.Arrange(new Rect(size));

            RenderTargetBitmap bmp = new RenderTargetBitmap(
                (int)obj.ActualWidth, (int)obj.ActualHeight, 96, 96, PixelFormats.Pbgra32);

            bmp.Render(obj);

            // return values as they were before
            obj.LayoutTransform = transform;
            obj.Margin = margin;
            return bmp;
        }

        private void ResetComboboxes()
        { 
        }

        void ConfigField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void FillComboboxes()
        {
            loadingCombos = true;
            
            cbxPatientId.Items.Clear();
            cbxPatientIdField.Items.Clear();
            cbxAgeField.Items.Clear();
            cbxGenderField.Items.Clear();
            cbxHeightField.Items.Clear();
            cbxWeightField.Items.Clear();
            cbxMeasurementField.Items.Clear();
            cbxUnits.Items.Clear();

            List<string> allFields = new List<string>();
            List<string> numberFields = new List<string>();
            ColumnDataType columnDataType = ColumnDataType.Boolean | ColumnDataType.DateTime | ColumnDataType.Numeric | ColumnDataType.Text | ColumnDataType.UserDefined;
            allFields = dashboardHelper.GetFieldsAsList(columnDataType);

            columnDataType = ColumnDataType.Numeric | ColumnDataType.UserDefined;
            numberFields = dashboardHelper.GetFieldsAsList(columnDataType);

            cbxPatientIdField.ItemsSource = allFields;
            cbxGenderField.ItemsSource = allFields;
            cbxAgeField.ItemsSource = numberFields;
            cbxWeightField.ItemsSource = numberFields;
            cbxHeightField.ItemsSource = numberFields;
            cbxMeasurementField.ItemsSource = numberFields;

            cbxUnits.Items.Add("Customary");
            cbxUnits.Items.Add("Metric");
            cbxUnits.SelectedIndex = 0;
            
            cbxPatientId.SelectedIndex = -1;
            cbxPatientIdField.SelectedIndex = -1;
            cbxAgeField.SelectedIndex = -1;
            cbxGenderField.SelectedIndex = -1;
            cbxHeightField.SelectedIndex = -1;
            cbxWeightField.SelectedIndex = -1;
            cbxMeasurementField.SelectedIndex = -1;
            
            loadingCombos = false;
        }

        void Triangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement source = sender as FrameworkElement;
            CollapseExpandConfigPanel();
        }

        private void CollapseExpandConfigPanel()
        {
            if (triangleCollapsed)
            {
                ConfigExpandedTriangle.Visibility = Visibility.Visible;
                ConfigCollapsedTriangle.Visibility = Visibility.Collapsed;
                ConfigCollapsedTitle.Visibility = Visibility.Collapsed;
                ConfigGrid.Height = Double.NaN;
                ConfigGrid.UpdateLayout();
            }
            else
            {
                ConfigCollapsedTriangle.Visibility = Visibility.Visible;
                ConfigExpandedTriangle.Visibility = Visibility.Collapsed;
                ConfigCollapsedTitle.Visibility = Visibility.Visible;
                ConfigGrid.Height = 50;
            }
            triangleCollapsed = !triangleCollapsed;
        }

        #region Private Properties
        private View View
        {
            get
            {
                return this.dashboardHelper.View;
            }
        }

        private IDbDriver Database
        {
            get
            {
                return this.dashboardHelper.Database;
            }
        }
        #endregion // Private Properties

        #region IGadget Members

        public bool IsProcessing
        {
            get
            {
                return isProcessing;
            }
            set
            {
                isProcessing = value;
            }
        }

        private void UpdateStatusMessage(string statusMessage)
        {
            //txtError.Visibility = System.Windows.Visibility.Visible;
            //txtError.Text = statusMessage;
        }

        /// <summary>
        /// Sets the gadget to its 'processing' state
        /// </summary>
        public void SetGadgetToProcessingState()
        {
            cbxAgeField.IsEnabled = false;
            cbxGenderField.IsEnabled = false;
            cbxHeightField.IsEnabled = false;
            cbxMeasurementField.IsEnabled = false;
            cbxPatientId.IsEnabled = false;
            cbxPatientIdField.IsEnabled = false;
            cbxUnits.IsEnabled = false;
            cbxWeightField.IsEnabled = false;
            checkboxShowLegend.IsEnabled = false;
            checkboxShowTallChart.IsEnabled = false;
            btnRun.IsEnabled = false;

            this.IsProcessing = true;
        }

        /// <summary>
        /// Sets the gadget to its 'finished' state
        /// </summary>
        public void SetGadgetToFinishedState()
        {
            cbxAgeField.IsEnabled = true;
            cbxGenderField.IsEnabled = true;
            cbxHeightField.IsEnabled = true;
            cbxMeasurementField.IsEnabled = true;
            cbxPatientId.IsEnabled = true;
            cbxPatientIdField.IsEnabled = true;
            cbxUnits.IsEnabled = true;
            cbxWeightField.IsEnabled = true;
            checkboxShowLegend.IsEnabled = true;
            checkboxShowTallChart.IsEnabled = true;
            btnRun.IsEnabled = true;

            this.IsProcessing = false;

            if (GadgetProcessingFinished != null)
                GadgetProcessingFinished(this);
        }

        /// <summary>
        /// Serializes the gadget into Xml
        /// </summary>
        /// <param name="doc">The Xml docment</param>
        /// <returns>XmlNode</returns>
        public XmlNode Serialize(XmlDocument doc)
        {
            string patientIdField = string.Empty;
            string genderField = string.Empty;
            string heightField = string.Empty;
            string weightField = string.Empty;
            string ageField = string.Empty;
            string measurementField = string.Empty;
            string patientIdValue = string.Empty;
            string units = string.Empty;            
            string chartType = string.Empty;

            // chart type should come first
            if (cbxPatientIdField.SelectedIndex > -1)
            {
                patientIdField = cbxPatientIdField.Text;
            }
            if (cbxGenderField.SelectedIndex > -1)
            {
                genderField = cbxGenderField.SelectedItem.ToString();
            }
            if (cbxHeightField.SelectedIndex > -1)
            {
                heightField = cbxHeightField.SelectedItem.ToString();
            }
            if (cbxWeightField.SelectedIndex > -1)
            {
                weightField = cbxWeightField.SelectedItem.ToString();
            }
            if (cbxAgeField.SelectedIndex > -1)
            {
                ageField = cbxAgeField.SelectedItem.ToString();
            }
            if (cbxMeasurementField.SelectedIndex > -1)
            {
                measurementField = cbxMeasurementField.SelectedItem.ToString();
            }
            if (cbxPatientId.SelectedIndex > -1)
            {
                patientIdValue = cbxPatientId.Text;
            }
            if (cbxUnits.SelectedIndex > -1)
            {
                units = cbxUnits.Text;
            }

            string xmlString =
            "<measurement>" + ((int)measurement).ToString() + "</measurement>" +
            "<reference>" + ((int)reference).ToString() + "</reference>" +
            "<patientIdField>" + patientIdField + "</patientIdField>" +
            "<patientIdValue>" + patientIdValue + "</patientIdValue>" +
            "<genderField>" + genderField + "</genderField>" +
            "<heightField>" + heightField + "</heightField>" +
            "<weightField>" + weightField + "</weightField>" +
            "<ageField>" + ageField + "</ageField>" +
            "<units>" + units + "</units>" +
            "<measurementField>" + measurementField + "</measurementField>" +
            "<showLegend>" + checkboxShowLegend.IsChecked + "</showLegend>" +
            "<showTallChart>" + checkboxShowTallChart.IsChecked + "</showTallChart>" +
            "<blackAndWhiteCurves>" + checkboxBlackAndWhite.IsChecked + "</blackAndWhiteCurves>";

            System.Xml.XmlElement element = doc.CreateElement("chartGadget");
            element.InnerXml = xmlString;

            System.Xml.XmlAttribute locationY = doc.CreateAttribute("top");
            System.Xml.XmlAttribute locationX = doc.CreateAttribute("left");
            System.Xml.XmlAttribute collapsed = doc.CreateAttribute("collapsed");
            System.Xml.XmlAttribute type = doc.CreateAttribute("gadgetType");

            locationY.Value = Canvas.GetTop(this).ToString("F0");
            locationX.Value = Canvas.GetLeft(this).ToString("F0");
            collapsed.Value = "false"; // currently no way to collapse the gadget, so leave this 'false' for now
            type.Value = "EpiDashboard.NutritionChartControl";

            element.Attributes.Append(locationY);
            element.Attributes.Append(locationX);
            element.Attributes.Append(collapsed);
            element.Attributes.Append(type);

            return element;
        }

        /// <summary>
        /// Creates the charting gadget from an Xml element
        /// </summary>
        /// <param name="element">The element from which to create the gadget</param>
        public void CreateFromXml(XmlElement element)
        {
            this.loadingCombos = true;

            foreach (XmlElement child in element.ChildNodes)
            {
                switch (child.Name.ToLowerInvariant())
                {
                    case "measurement":
                        measurement = ((GrowthMeasurement)Int32.Parse(child.InnerText));
                        break;
                    case "reference":
                        reference = ((GrowthReference)Int32.Parse(child.InnerText));
                        break;
                    case "patientidfield":
                        cbxPatientIdField.Text = child.InnerText;
                        break;
                    case "patientidvalue":
                        cbxPatientId.Text = child.InnerText;
                        break;                    
                    case "genderfield":
                        cbxGenderField.Text = child.InnerText;
                        break;
                    case "heightfield":
                        cbxHeightField.Text = child.InnerText;
                        break;
                    case "weightfield":
                        cbxWeightField.Text = child.InnerText;
                        break;
                    case "agefield":
                        cbxAgeField.Text = child.InnerText;
                        break;
                    case "measurementfield":
                        cbxMeasurementField.Text = child.InnerText;
                        break;
                    case "showlegend":
                        if (child.InnerText.ToLowerInvariant().Equals("true"))
                        {                            
                            checkboxShowLegend.IsChecked = true;
                        }
                        else
                        {
                            checkboxShowLegend.IsChecked = false;
                        }
                        break;
                    case "showtallchart":
                        if (child.InnerText.ToLowerInvariant().Equals("true"))
                        {
                            checkboxShowTallChart.IsChecked = true;
                        }
                        else
                        {
                            checkboxShowTallChart.IsChecked = false;
                        }
                        break;
                    case "blackandwhitecurves":
                        if (child.InnerText.ToLowerInvariant().Equals("true"))
                        {
                            checkboxBlackAndWhite.IsChecked = true;
                        }
                        else
                        {
                            checkboxBlackAndWhite.IsChecked = false;
                        }
                        break;
                }
            }

            foreach (XmlAttribute attribute in element.Attributes)
            {
                switch (attribute.Name.ToLowerInvariant())
                {
                    case "top":
                        Canvas.SetTop(this, double.Parse(attribute.Value));
                        break;
                    case "left":
                        Canvas.SetLeft(this, double.Parse(attribute.Value));
                        break;
                }
            }

            this.loadingCombos = false;

            SetReference();
            SetMeasurement();

            RefreshResults();
            CollapseExpandConfigPanel();
        }

        /// <summary>
        /// Converts the gadget's output to Html
        /// </summary>
        /// <returns></returns>
        public string ToHTML(string htmlFileName = "", int count = 0, bool useAlternatingColors = false, bool ForWeb = false)
        {
            StringBuilder htmlBuilder = new StringBuilder();

            htmlBuilder.AppendLine("<h2>" + ChartTitle.Content + "</h2>");
            htmlBuilder.AppendLine("<h3>" + ChartSubTitle.Content + "</h3>");

            htmlBuilder.AppendLine("<p><small>");

            if (cbxPatientId.SelectedIndex >= 0)
            {
                htmlBuilder.AppendLine("<em>Patient ID:</em> <strong>" + cbxPatientId.SelectedValue.ToString() + "</strong>");
                htmlBuilder.AppendLine("<br />");
            }

            if (cbxPatientIdField.SelectedIndex >= 0)
            {
                htmlBuilder.AppendLine("<em>Patient ID field:</em> <strong>" + cbxPatientIdField.Text + "</strong>");
                htmlBuilder.AppendLine("<br />");
            }
            if (cbxAgeField.SelectedIndex >= 0)
            {
                htmlBuilder.AppendLine("<em>Age variable:</em> <strong>" + cbxAgeField.Text + "</strong>");
                htmlBuilder.AppendLine("<br />");
            }
            if (cbxHeightField.SelectedIndex >= 0)
            {
                htmlBuilder.AppendLine("<em>Height variable:</em> <strong>" + cbxHeightField.Text + "</strong>");
                htmlBuilder.AppendLine("<br />");
            }
            if (cbxWeightField.SelectedIndex >= 0)
            {
                htmlBuilder.AppendLine("<em>Weight variable:</em> <strong>" + cbxWeightField.Text + "</strong>");
                htmlBuilder.AppendLine("<br />");
            }
            if (cbxMeasurementField.SelectedIndex >= 0)
            {
                htmlBuilder.AppendLine("<em>Measurement variable:</em> <strong>" + cbxMeasurementField.Text + "</strong>");
                htmlBuilder.AppendLine("<br />");
            }

            htmlBuilder.AppendLine("<em>Show legend:</em> <strong>" + checkboxShowLegend.IsChecked.ToString() + "</strong>");
            htmlBuilder.AppendLine("<br />");
            htmlBuilder.AppendLine("</small></p>");

            string imageFileName = string.Empty;

            if(htmlFileName.EndsWith(".html")) 
            {
                imageFileName = htmlFileName.Remove(htmlFileName.Length - 5, 5);
            }
            else if (htmlFileName.EndsWith(".htm"))
            {
                imageFileName = htmlFileName.Remove(htmlFileName.Length - 4, 4);
            }

            imageFileName = imageFileName + "_" + count.ToString() + ".png";
            
            BitmapSource img = (BitmapSource)ToImageSource(pnlChartContainer);
            FileStream stream = new FileStream(imageFileName, FileMode.Create);
            PngBitmapEncoder encoder = new PngBitmapEncoder();            
            encoder.Frames.Add(BitmapFrame.Create(img));
            encoder.Save(stream);
            stream.Close();

            htmlBuilder.AppendLine("<img src=\"" + imageFileName + "\" />");

            return htmlBuilder.ToString();
        }

        private string customOutputHeading;
        private string customOutputDescription;
        private string customOutputCaption;

        public string CustomOutputHeading
        {
            get
            {
                return this.customOutputHeading;
            }
            set
            {
                this.customOutputHeading = value;
            }
        }

        public string CustomOutputDescription
        {
            get
            {
                return this.customOutputDescription;
            }
            set
            {
                this.customOutputDescription = value;
            }
        }

        public string CustomOutputCaption
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

        #region Public Classes
        public class GrowthCurvesPercentiles
        {
            public double C01;
            public double C1;
            public double C3;
            public double C5;
            public double C10;
            public double C15;
            public double C25;
            public double C50;
            public double C75;
            public double C85;
            public double C90;
            public double C95;
            public double C97;
            public double C99;
            public double C999;
            public double xaxis;

            public GrowthCurvesPercentiles(double C1, double C3, double C15, double C50, double C85, double C97, double C99,
                double xaxis)
            {
                this.C1 = C1;
                this.C3 = C3;
                this.C15 = C15;                
                this.C50 = C50;                
                this.C85 = C85;
                this.C97 = C97;
                this.C99 = C99;
                this.xaxis = xaxis;
            }

            public GrowthCurvesPercentiles(double C3, double C5, double C10, double C25, double C50, double C75, double C85,
                double C90, double C95, double C97, double xaxis)
            {
                this.C3 = C3;
                this.C5 = C5;
                this.C10 = C10;
                this.C25 = C25;
                this.C50 = C50;
                this.C75 = C75;
                this.C85 = C85;
                this.C90 = C90;
                this.C95 = C95;
                this.C97 = C97;
                this.xaxis = xaxis;
            }

            public GrowthCurvesPercentiles(double C3, double C5, double C10, double C25, double C50, double C75, double C90,
                double C95, double C97, double xaxis)
            {
                this.C3 = C3;
                this.C5 = C5;
                this.C10 = C10;
                this.C25 = C25;
                this.C50 = C50;
                this.C75 = C75;
                this.C90 = C90;
                this.C95 = C95;
                this.C97 = C97;
                this.xaxis = xaxis;
            }

            public GrowthCurvesPercentiles(double C1, double C3, double C5, double C10, double C15, double C25, double C50, double C75, double C85, double C90,
                double C95, double C97, double C99, double xaxis)
            {
                this.C1 = C1;
                this.C3 = C3;
                this.C5 = C5;
                this.C10 = C10;
                this.C15 = C15;
                this.C25 = C25;
                this.C50 = C50;
                this.C75 = C75;
                this.C85 = C85;
                this.C90 = C90;
                this.C95 = C95;
                this.C97 = C97;
                this.C99 = C99;
                this.xaxis = xaxis;
            }

            public GrowthCurvesPercentiles(double C01, double C1, double C3, double C5, double C10, double C15, double C25, double C50, double C75, double C85, double C90,
            double C95, double C97, double C99, double C999, double xaxis)
            {
                this.C01 = C01;
                this.C1 = C1;
                this.C3 = C3;
                this.C5 = C5;
                this.C10 = C10;
                this.C15 = C15;
                this.C25 = C25;
                this.C50 = C50;
                this.C75 = C75;
                this.C85 = C85;
                this.C90 = C90;
                this.C95 = C95;
                this.C97 = C97;
                this.C99 = C99;
                this.C999 = C999;
                this.xaxis = xaxis;
            }
        }
        #endregion // Public Classes

        public bool DrawBorders { get; set; } 
    }
}
