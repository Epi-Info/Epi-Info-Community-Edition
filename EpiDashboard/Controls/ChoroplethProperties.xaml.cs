using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Epi;
using EpiDashboard.Mapping;
using System.Windows.Forms;
using System.Net;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Toolkit;

namespace EpiDashboard.Controls
{
    public class UIControls
    {
        public Rectangle rectangle;
        public System.Windows.Controls.TextBox rampStarts;
        public System.Windows.Controls.TextBlock centerTexts;
        public System.Windows.Controls.TextBox rampEnds;
        public System.Windows.Controls.TextBox legedTexts;
    }

    /// <summary>
    /// Interaction logic for DashboardProperties.xaml
    /// </summary>
    public partial class ChoroplethProperties : System.Windows.Controls.UserControl
    {
        private EpiDashboard.Mapping.StandaloneMapControl _mapControl;
        private ESRI.ArcGIS.Client.Map _myMap;
        private DashboardHelper _dashboardHelper;
        private int _currentStratCount;
        private SolidColorBrush _currentColor_rampMissing;
        private SolidColorBrush _currentColor_rampStart;
        private SolidColorBrush _currentColor_rampEnd;
        private bool _initialRampCalc;

        public IChoroLayerProvider thisProvider;

        public ChoroplethKmlLayerProvider choroplethKmlLayerProvider;
        public ChoroplethServerLayerProvider choroplethServerLayerProvider;
        public ChoroplethShapeLayerProvider choroplethShapeLayerProvider;

        public ChoroplethShapeLayerProperties choroplethShapeLayerProperties;
        public ChoroplethServerLayerProperties choroplethServerLayerProperties;
        public ChoroplethKmlLayerProperties choroplethKmlLayerProperties;

        public RowFilterControl rowFilterControl { get; set; }
        public DataFilters datafilters { get; set; }

        private System.Xml.XmlElement currentElement;
        private string shapeFilePath;

        public ListLegendTextDictionary ListLegendText = new ListLegendTextDictionary();

        public IDictionary<string, object> shapeAttributes;
        private Dictionary<int, object> ClassAttribList = new Dictionary<int, object>();
        public Envelope mapOriginalExtent;
        public List<string> layerAddednew = new List<string>();

        string _shapeKey;
        string _dataKey;
        string _value;

        private struct classAttributes
        {
            public Brush rctColor;
            public string rampStart;
            public string rampEnd;
            public string legendText;
        }

        public ChoroplethProperties(EpiDashboard.Mapping.StandaloneMapControl mapControl, ESRI.ArcGIS.Client.Map myMap)
        {
            InitializeComponent();
            _mapControl = mapControl;
            _myMap = myMap;
            mapControl.SizeChanged += mapControl_SizeChanged;

            Epi.ApplicationIdentity appId = new Epi.ApplicationIdentity(typeof(Configuration).Assembly);
            tblockCurrentEpiVersion.Text = "Epi Info " + appId.Version;

            if (int.TryParse(cmbClasses.Text, out _currentStratCount) == false)
            {
                _currentStratCount = 4;
            }

            _currentColor_rampMissing = (SolidColorBrush)rctMissingColor.Fill;
            _currentColor_rampStart = (SolidColorBrush)rctLowColor.Fill;
            _currentColor_rampEnd = (SolidColorBrush)rctHighColor.Fill;
            _initialRampCalc = true;
            mapOriginalExtent = myMap.Extent;

            #region Translation

            //Point of Interest Left Panel
            lblConfigExpandedTitle.Content = DashboardSharedStrings.GADGET_CONFIG_TITLE_CHOROPLETH;
            tbtnDataSource.Title = DashboardSharedStrings.GADGET_MAP_TABBUTTON_DATA_SOURCE;
            tbtnDataSource.Description = DashboardSharedStrings.GADGET_MAP_TABDESC_DATASOURCE;
            tbtnVariables.Title = DashboardSharedStrings.GADGET_MAP_TABBUTTON_VARIABLES;
            tbtnVariables.Description = DashboardSharedStrings.GADGET_MAP_TABDESC_VARIABLES;
            tbtnDisplay.Title = DashboardSharedStrings.GADGET_MAP_TABBUTTON_DISPLAY;
            tbtnDisplay.Description = DashboardSharedStrings.GADGET_MAP_TABDESC_DISPLAY;
            tbtnFilters.Title = DashboardSharedStrings.GADGET_MAP_TABBUTTON_FILTERS;
            tbtnFilters.Description = DashboardSharedStrings.GADGET_MAP_TABDESC_FILTERS;

            //Data Source Panel
            tblockPanelDataSource.Content = DashboardSharedStrings.GADGET_DATA_SOURCE;
            tblockDataSource.Content = DashboardSharedStrings.GADGET_DATA_SOURCE;
            btnBrowse.Content = DashboardSharedStrings.BUTTON_BROWSE;
            lblBoundaries.Content = DashboardSharedStrings.GADGET_BOUNDARIES;
            radShapeFile.Content = DashboardSharedStrings.GADGET_SHAPEFILE;
            btnBrowseShape.Content = DashboardSharedStrings.BUTTON_BROWSE;
            radMapServer.Content = DashboardSharedStrings.GADGET_MAPSERVER;
            lblURL.Content = DashboardSharedStrings.GADGET_URL;
            btnMapserverlocate.Content = DashboardSharedStrings.BUTTON_CONNECT;
            lblExampleMapServerURL.Text = DashboardSharedStrings.GADGET_EXAMPLE_MAPSERVER;
            lblSelectFeature.Content = DashboardSharedStrings.GADGET_SELECT_FEATURE;
            radKML.Content = DashboardSharedStrings.GADGET_KMLFILE;
            lblURLOfKMLFile.Content = DashboardSharedStrings.GADGET_KMLFILE_LOCATION;
            btnKMLFile.Content = DashboardSharedStrings.BUTTON_BROWSE;
            lblExampleKMLFile.Text = DashboardSharedStrings.GADGET_EXAMPLE_KMLFILE;

            tblockConnectionString.Content = DashboardSharedStrings.GADGET_CONNECTION_STRING;
            tblockSQLQuery.Content = DashboardSharedStrings.GADGET_SQL_QUERY;
            tblockLoadingData.Text = DashboardSharedStrings.GADGET_LOADING_DATA;

            //Variables Panel
            tblockPanelVariables.Content = DashboardSharedStrings.GADGET_TABBUTTON_VARIABLES;
            //tblockSelectVarData.Content = DashboardSharedStrings.GADGET_VARIABLES_CHORPLTH;
            lblFeature.Content = DashboardSharedStrings.GADGET_MAP_FEATURE;
            lblData.Content = DashboardSharedStrings.GADGET_MAP_DATA;
            lblValue.Content = DashboardSharedStrings.GADGET_MAP_VALUE;

            //Colors & Ranges Panel
            lblPanelHdrColorsAndStyles.Content = DashboardSharedStrings.GADGET_MAP_TABBUTTON_DISPLAY;
            tblockColorsSubheader.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_COLORS;
            lblMissingExcluded.Content = DashboardSharedStrings.GADGET_MAP_COLOR_MISSING;
            //lblColorRamp.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_COLORS;
            lblColorStart.Content = DashboardSharedStrings.GADGET_MAP_START;
            lblColorEnd.Content = DashboardSharedStrings.GADGET_MAP_END;
            tblockOpacity.Text = DashboardSharedStrings.GADGET_MAP_OPACITY;
            tblockRangesSubheader.Content = DashboardSharedStrings.GADGET_MAP_RANGES;
            lblClassBreaks.Content = DashboardSharedStrings.GADGET_MAP_CLASSES;
            quintilesOption.Content = DashboardSharedStrings.GADGET_MAP_QUANTILES;
            tblockLegendTitleSubheader.Content = DashboardSharedStrings.GADGET_MAP_LEGEND_TITLE;
            tblockColorRamp.Text = DashboardSharedStrings.GADGET_PANELSUBHEADER_COLOR;
            tblockRange.Text = DashboardSharedStrings.GADGET_MAP_RANGE;
            tblockLegText.Text = DashboardSharedStrings.GADGET_MAP_LEGEND_TEXT;
            legendText0.Text = DashboardSharedStrings.GADGET_MAP_CHORPLTH_MISSEXCLUDED;

            //Filters Panel
            tblockPanelDataFilter.Content = DashboardSharedStrings.GADGET_PANELHEADER_DATA_FILTER;
            tblockSetDataFilter.Content = DashboardSharedStrings.GADGET_TABDESC_FILTERS_MAPS;

            //Info Panel
            //lblCanvasInfo.Content = DashboardSharedStrings.GADGET_CANVAS_INFO;
            //tblockRows.Text = dashboardHelper.DataSet.Tables[0].Rows.Count.ToString() + DashboardSharedStrings.GADGET_INFO_UNFILTERED_ROWS;


            btnOK.Content = DashboardSharedStrings.BUTTON_OK;
            btnCancel.Content = DashboardSharedStrings.BUTTON_CANCEL;

            tblockColorsSubheader.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_COLORS;

            #endregion // Translation
        }

        public void mapControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _mapControl.ResizedWidth = e.NewSize.Width;
            _mapControl.ResizedHeight = e.NewSize.Height;
            if (_mapControl.ResizedWidth != 0 & _mapControl.ResizedHeight != 0)
            {
                //System.Windows.MessageBox.Show("1");
                //System.Windows.MessageBox.Show(_mapControl.ResizedWidth.ToString());
                //System.Windows.MessageBox.Show(_mapControl.ResizedHeight.ToString());
                double i_StandardHeight = System.Windows.SystemParameters.PrimaryScreenHeight;//Developer Desktop Width Where the Form is Designed
                double i_StandardWidth = System.Windows.SystemParameters.PrimaryScreenWidth; ////Developer Desktop Height Where the Form is Designed
                float f_HeightRatio = new float();
                float f_WidthRatio = new float();
                f_HeightRatio = (float)((float)_mapControl.ResizedHeight / (float)i_StandardHeight);
                f_WidthRatio = (float)((float)_mapControl.ResizedWidth / (float)i_StandardWidth);

                this.Height = (Convert.ToInt32(i_StandardHeight * f_HeightRatio)) / 1.16;
                this.Width = (Convert.ToInt32(i_StandardWidth * f_WidthRatio)) / 1.13;

            }
            else
            {
                //System.Windows.MessageBox.Show("2");
                this.Width = (System.Windows.SystemParameters.PrimaryScreenWidth / 1.2);
                this.Height = (System.Windows.SystemParameters.PrimaryScreenHeight / 1.2);
            }
        }


        public event EventHandler Cancelled;
        public event EventHandler ChangesAccepted;

        public DashboardHelper DashboardHelper { get; private set; }

        public string DataSource { get; set; }

        private byte _opacity = 240;

        public byte Opacity
        {
            get { return _opacity; }
            set { _opacity = value; }
        }

        public FileInfo ProjectFileInfo
        {
            get
            {
                FileInfo fi = new FileInfo(txtProjectPath.Text);
                return fi;
            }
            set
            {
                txtProjectPath.Text = value.FullName;
                panelDataSourceProject.Visibility = Visibility.Visible;
                panelDataSourceOther.Visibility = Visibility.Collapsed;
                panelDataSourceAdvanced.Visibility = Visibility.Collapsed;
            }
        }

        public string ConnectionString
        {
            get
            {
                return txtDataSource.Text;
            }
            set
            {
                txtDataSource.Text = value;
                panelDataSourceProject.Visibility = Visibility.Collapsed;
                panelDataSourceOther.Visibility = Visibility.Visible;
                panelDataSourceAdvanced.Visibility = Visibility.Collapsed;
            }
        }

        public string SqlQuery
        {
            get
            {
                return txtSQLQuery.Text;
            }
            set
            {
                txtSQLQuery.Text = value;
                panelDataSourceProject.Visibility = Visibility.Collapsed;
                panelDataSourceOther.Visibility = Visibility.Collapsed;
                panelDataSourceAdvanced.Visibility = Visibility.Visible;
            }
        }

        public string MapServerName { get; set; }
        public int MapVisibleLayer { get; set; }
        public string KMLMapServerName { get; set; }
        public int KMLMapVisibleLayer { get; set; }

        private void tbtnInfo_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Visible;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnDisplay_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Visible;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
            SetDefaultRanges();
            PropertyChanged_EnableDisable();
        }

        public void SetDefaultRanges()
        {
            if (cmbShapeKey.SelectedItem != null && cmbDataKey.SelectedItem != null && cmbValue.SelectedItem != null)
            {
                if (thisProvider != null && thisProvider.Opacity > 0)
                {
                    Opacity = thisProvider.Opacity;
                    sliderOpacity.Value = Opacity;
                }
                else if (choroplethKmlLayerProvider != null && choroplethKmlLayerProvider.Opacity > 0)
                {
                    Opacity = choroplethKmlLayerProvider.Opacity;
                    sliderOpacity.Value = Opacity;
                }
                else if (choroplethServerLayerProperties != null && choroplethServerLayerProperties.Opacity > 0)
                {
                    Opacity = choroplethServerLayerProperties.Opacity;
                    sliderOpacity.Value = Opacity;
                }
                else
                {
                    Opacity = Convert.ToByte(sliderOpacity.Value);
                }

                List<SolidColorBrush> brushList = new List<SolidColorBrush>()
                {
                    (SolidColorBrush) rctColor1.Fill,
                    (SolidColorBrush) rctColor2.Fill,
                    (SolidColorBrush) rctColor3.Fill,
                    (SolidColorBrush) rctColor4.Fill,
                    (SolidColorBrush) rctColor5.Fill,
                    (SolidColorBrush) rctColor6.Fill,
                    (SolidColorBrush) rctColor7.Fill,
                    (SolidColorBrush) rctColor8.Fill,
                    (SolidColorBrush) rctColor9.Fill,
                    (SolidColorBrush) rctColor10.Fill,
                    (SolidColorBrush) rctColor0.Fill
                };

                int classCount;
                if (!int.TryParse(((ComboBoxItem)cmbClasses.SelectedItem).Content.ToString(), out classCount))
                {
                    classCount = 4;
                }

                if (thisProvider != null)
                {
                    thisProvider.PopulateRangeValues(_dashboardHelper,
                        cmbShapeKey.SelectedItem.ToString(),
                        cmbDataKey.SelectedItem.ToString(),
                        cmbValue.SelectedItem.ToString(),
                        brushList,
                        classCount,
                        legTitle.Text);
                }

                SetRangeUISection();
            }

            UpdateRangesCollection();
        }

        private void UpdateColorsCollection()
        {
            foreach (UIElement element in stratGrid.Children)
            {
                if (element is Rectangle)
                {
                    SolidColorBrush scBrush = ((Rectangle)element).Fill as SolidColorBrush;
                    Color color = scBrush.Color;
                    thisProvider.CustomColorsDictionary.Add(((Rectangle)element).Name, color);
                }
            }
        }

        private void UpdateRangesCollection()
        {

            if (thisProvider == null)
                return;

            foreach (UIElement element in stratGrid.Children)
            {
                if (element is System.Windows.Controls.TextBox)
                {

                    string elementName = ((System.Windows.Controls.TextBox)element).Name;

                    if (elementName.StartsWith("ramp"))
                    {

                        thisProvider.ClassRangesDictionary.Add(((System.Windows.Controls.TextBox)element).Name,
                            ((System.Windows.Controls.TextBox)element).Text);
                    }
                }
            }
        }


        private void tbtnVariables_Checked(object sender, RoutedEventArgs e)
        {

            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelVariables.Visibility = System.Windows.Visibility.Visible;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
            if (choroplethServerLayerProperties != null)
                if (choroplethServerLayerProperties.provider.FlagUpdateToGLFailed) { ResetShapeCombo(); }


        }

        private void tbtnFilters_Checked(object sender, RoutedEventArgs e)
        {

            if (panelDataSource == null) return;
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Visible;
        }

        private void tbtnDataSource_Checked(object sender, RoutedEventArgs e)
        {
            if (panelDataSource == null) return;
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Visible;
            panelVariables.Visibility = System.Windows.Visibility.Collapsed;
            panelDisplay.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void CheckButtonStates(ToggleButton sender)
        {
            foreach (UIElement element in panelSidebar.Children)
            {
                if (element is ToggleButton)
                {
                    ToggleButton tbtn = element as ToggleButton;
                    if (tbtn != sender)
                    {
                        tbtn.IsChecked = false;
                    }
                }
            }
        }

        private void txtProjectPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            PropertyChanged_EnableDisable();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            string errorRoutine = "none";

            try
            {
                if (cmbDataKey.SelectedIndex > -1 && cmbShapeKey.SelectedIndex > -1 && cmbValue.SelectedIndex > -1)
                {
                    errorRoutine = "Addfilters";
                    Addfilters();

                    errorRoutine = "SetDefaultRanges";
                    //SetDefaultRanges();

                    //  ResetLegend_Click(this, new RoutedEventArgs());

                    errorRoutine = "RenderMap";
                    RenderMap();

                    errorRoutine = "AddClassAttributes";
                    AddClassAttributes();

                    errorRoutine = "none";

                    int numclasses = Convert.ToInt32(cmbClasses.Text);
                    bool flagquintiles = (bool)quintilesOption.IsChecked;

                    if (radShapeFile.IsChecked == true && thisProvider != null)
                    {
                        errorRoutine = "choroplethShapeLayerProperties.SetValues";
                        
                        choroplethShapeLayerProperties.SetValues(
                            txtShapePath.Text,
                            cmbShapeKey.Text,
                            cmbDataKey.Text,
                            cmbValue.Text,
                            cmbClasses.Text,
                            rctHighColor.Fill,
                            rctLowColor.Fill,
                            rctMissingColor.Fill,
                            shapeAttributes,
                            ClassAttribList,
                            flagquintiles,
                            numclasses,
                            legTitle.Text);
                    }
                    else if (radMapServer.IsChecked == true && choroplethServerLayerProvider != null)
                    {
                        errorRoutine = "choroplethServerLayerProperties.SetValues";
                        
                        choroplethServerLayerProperties.SetValues(
                            cmbShapeKey.Text,
                            cmbDataKey.Text,
                            cmbValue.Text,
                            cmbClasses.Text,
                            rctHighColor.Fill,
                            rctLowColor.Fill,
                            rctMissingColor.Fill,
                            shapeAttributes,
                            ClassAttribList,
                            flagquintiles,
                            numclasses,
                            Opacity);
                        
                        choroplethServerLayerProperties.txtMapserverText = txtMapSeverpath.Text;
                        choroplethServerLayerProperties.cbxMapFeatureText = cbxmapfeature.Text;
                    }

                    else if (radKML.IsChecked == true && choroplethKmlLayerProvider != null)
                    {
                        errorRoutine = "choroplethKmlLayerProperties.SetValues";
                        
                        choroplethKmlLayerProperties.SetValues(
                            cmbShapeKey.Text, 
                            cmbDataKey.Text, 
                            cmbValue.Text,
                            cmbClasses.Text, 
                            rctHighColor.Fill, 
                            rctLowColor.Fill, 
                            rctMissingColor.Fill, 
                            shapeAttributes,
                            ClassAttribList, 
                            flagquintiles, 
                            numclasses);
                    }

                    errorRoutine = "none";

                    if (ChangesAccepted != null)
                    {
                        ChangesAccepted(this, new EventArgs());
                    }

                    btnOK.IsEnabled = true;
                }
                else
                {
                    MessageBoxResult result = System.Windows.MessageBox.Show(DashboardSharedStrings.GADGET_MAP_ADD_VARIABLES, DashboardSharedStrings.ALERT, MessageBoxButton.OK);
                    btnOK.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error with map properties errorRoutine  --  " + errorRoutine + " -- " + ex.Message);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }

            foreach (string id in layerAddednew)
            {
                ESRI.ArcGIS.Client.GraphicsLayer graphicsLayer = _myMap.Layers[id] as ESRI.ArcGIS.Client.GraphicsLayer;
                if (graphicsLayer != null)
                    _myMap.Layers.Remove(graphicsLayer);
            }
            
            _myMap.Extent = mapOriginalExtent;
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            waitCursor.Visibility = Visibility.Visible;
            tblockLoadingData.Visibility = Visibility.Visible;

            _dashboardHelper = _mapControl.GetNewDashboardHelper();
            waitCursor.Visibility = Visibility.Collapsed;
            tblockLoadingData.Visibility = Visibility.Collapsed;

            if (_dashboardHelper != null)
            {
                this.DashboardHelper = _dashboardHelper;
                txtProjectPath.Text = _mapControl.ProjectFilepath;
                FillComboBoxes();
                panelBoundaries.IsEnabled = true;
                radShapeFile.IsChecked = true;
                this.datafilters = new DataFilters(_dashboardHelper);
                rowFilterControl = new RowFilterControl(_dashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, datafilters, true);
                rowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left; rowFilterControl.FillSelectionComboboxes();

                if (HasFilter())
                {
                    RemoveFilter();
                }

                panelFilters.Children.Add(rowFilterControl);
            }
        }

        private void RemoveFilter()
        {
            int ChildIndex = 0;

            foreach (var child in panelFilters.Children)
            {
                if (child.GetType().FullName.Contains("EpiDashboard.RowFilterControl"))
                {
                    panelFilters.Children.RemoveAt(ChildIndex);
                    break;
                }

                ChildIndex++;
            }
        }

        private bool HasFilter()
        {
            bool HasFilter = false;
            foreach (var child in panelFilters.Children)
            {
                if (child.GetType().FullName.Contains("EpiDashboard.RowFilterControl"))
                {
                    HasFilter = true;
                }
            }
            return HasFilter;
        }

        public void FillComboBoxes()
        {
            cmbDataKey.Items.Clear();
            cmbValue.Items.Clear();
            List<string> fields = _dashboardHelper.GetFieldsAsList(); // dashboardHelper.GetFormFields();
            ColumnDataType columnDataType = ColumnDataType.Numeric;
            List<string> numericFields = _dashboardHelper.GetFieldsAsList(columnDataType); //dashboardHelper.GetNumericFormFields();
            foreach (string field in fields)
            {
                if (!(field.ToUpper() == "RECSTATUS" || field.ToUpper() == "FKEY" || field.ToUpper() == "GLOBALRECORDID" || field.ToUpper() == "UNIQUEKEY" || field.ToUpper() == "FIRSTSAVETIME" || field.ToUpper() == "LASTSAVETIME" || field.ToUpper() == "SYSTEMDATE"))
                { cmbDataKey.Items.Add(field); }
            }
            foreach (string field in numericFields)
            {
                if (!(field.ToUpper() == "RECSTATUS" || field.ToUpper() == "UNIQUEKEY"))
                { cmbValue.Items.Add(field); }
            }
            cmbValue.Items.Insert(0, "{Record Count}");
        }
        
        public void ReFillValueComboBoxes()
        {
            cmbValue.Items.Clear();
            ColumnDataType columnDataType = ColumnDataType.Numeric;
            List<string> numericFields = _dashboardHelper.GetFieldsAsList(columnDataType); //dashboardHelper.GetNumericFormFields();

            foreach (string field in numericFields)
            {
                if (!(field.ToUpper() == "RECSTATUS" || field.ToUpper() == "UNIQUEKEY"))
                { 
                    cmbValue.Items.Add(field); 
                }
            }

            cmbValue.Items.Insert(0, "{Record Count}");
        }
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }

        private void AddClassAttributes()
        {
            ClassAttribList.Clear();

            classAttributes ca1 = new classAttributes();
            ca1.rctColor = rctColor1.Fill;
            ca1.rampStart = rampStart01.Text;
            ca1.rampEnd = rampEnd01.Text;
            ca1.legendText = legendText1.Text;
            ClassAttribList.Add(1, ca1);

            classAttributes ca2 = new classAttributes();
            ca2.rctColor = rctColor2.Fill;
            ca2.rampStart = rampStart02.Text;
            ca2.rampEnd = rampEnd02.Text;
            ca2.legendText = legendText2.Text;
            ClassAttribList.Add(2, ca2);

            classAttributes ca3 = new classAttributes();
            ca3.rctColor = rctColor3.Fill;
            ca3.rampStart = rampStart03.Text;
            ca3.rampEnd = rampEnd03.Text;
            ca3.legendText = legendText3.Text;
            ClassAttribList.Add(3, ca3);

            classAttributes ca4 = new classAttributes();
            ca4.rctColor = rctColor4.Fill;
            ca4.rampStart = rampStart04.Text;
            ca4.rampEnd = rampEnd04.Text;
            ca4.legendText = legendText4.Text;
            ClassAttribList.Add(4, ca4);

            classAttributes ca5 = new classAttributes();
            ca5.rctColor = rctColor5.Fill;
            ca5.rampStart = rampStart05.Text;
            ca5.rampEnd = rampEnd05.Text;
            ca5.legendText = legendText5.Text;
            ClassAttribList.Add(5, ca5);

            classAttributes ca6 = new classAttributes();
            ca6.rctColor = rctColor6.Fill;
            ca6.rampStart = rampStart06.Text;
            ca6.rampEnd = rampEnd06.Text;
            ca6.legendText = legendText6.Text;
            ClassAttribList.Add(6, ca6);

            classAttributes ca7 = new classAttributes();
            ca7.rctColor = rctColor7.Fill;
            ca7.rampStart = rampStart07.Text;
            ca7.rampEnd = rampEnd07.Text;
            ca7.legendText = legendText7.Text;
            ClassAttribList.Add(7, ca7);

            classAttributes ca8 = new classAttributes();
            ca8.rctColor = rctColor8.Fill;
            ca8.rampStart = rampStart08.Text;
            ca8.rampEnd = rampEnd08.Text;
            ca8.legendText = legendText8.Text;
            ClassAttribList.Add(8, ca8);

            classAttributes ca9 = new classAttributes();
            ca9.rctColor = rctColor9.Fill;
            ca9.rampStart = rampStart09.Text;
            ca9.rampEnd = rampEnd09.Text;
            ca9.legendText = legendText9.Text;
            ClassAttribList.Add(9, ca9);

            classAttributes ca10 = new classAttributes();
            ca10.rctColor = rctColor10.Fill;
            ca10.rampStart = rampStart10.Text;
            ca10.rampEnd = rampEnd10.Text;
            ca10.legendText = legendText10.Text;
            ClassAttribList.Add(10, ca10);

            UpdateRangesCollection();
        }

        private bool ValidateInputValue(System.Windows.Controls.TextBox textBox)
        {
            double Value;
            if (double.TryParse(textBox.Text, out Value))
            {
                if (Value >= double.Parse(thisProvider.RangeValues[0, 0].ToString()) && Value <= double.Parse(thisProvider.RangeValues[thisProvider.RangeCount - 1, 1].ToString()))
                {
                    return true;
                }
            }
            return false;
        }

        private bool ValidateRangeInput()
        {
            int _rangeCount = thisProvider.RangeCount;

            if (_rangeCount >= 2)
            {
                if (!(ValidateInputValue(rampStart01) && ValidateInputValue(rampEnd01)
                    && ValidateInputValue(rampStart02) && ValidateInputValue(rampEnd02)))
                {
                    System.Windows.MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }

            if (_rangeCount >= 3)
            {
                if (!(ValidateInputValue(rampStart03) && ValidateInputValue(rampEnd03)))
                {
                    System.Windows.MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }

            if (_rangeCount >= 4)
            {
                if (!(ValidateInputValue(rampStart04) && ValidateInputValue(rampEnd04)))
                {
                    System.Windows.MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }
            if (_rangeCount >= 5)
            {
                if (!(ValidateInputValue(rampStart05) && ValidateInputValue(rampEnd05)))
                {
                    System.Windows.MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }
            if (_rangeCount >= 6)
            {
                if (!(ValidateInputValue(rampStart06) && ValidateInputValue(rampEnd06)))
                {
                    System.Windows.MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }
            if (_rangeCount >= 7)
            {
                if (!(ValidateInputValue(rampStart07) && ValidateInputValue(rampEnd07)))
                {
                    System.Windows.MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }
            if (_rangeCount >= 8)
            {
                if (!(ValidateInputValue(rampStart08) && ValidateInputValue(rampEnd08)))
                {
                    System.Windows.MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }
            if (_rangeCount >= 9)
            {
                if (!(ValidateInputValue(rampStart09) && ValidateInputValue(rampEnd09)))
                {
                    System.Windows.MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }
            if (_rangeCount >= 10)
            {
                if (!(ValidateInputValue(rampStart10) && ValidateInputValue(rampEnd10)))
                {
                    System.Windows.MessageBox.Show("Incorrect input value.");
                    return false;
                }
            }
            return true;
        }

        public void SetClassAttributes(Dictionary<int, object> classAttrib)
        {

            foreach (int key in classAttrib.Keys)
            {
                var item = classAttrib.ElementAt(key - 1);
                classAttributes itemvalue = (classAttributes)item.Value;
            }
        }

        public void SetDashboardHelper(DashboardHelper dash)
        {
            _dashboardHelper = dash;
        }

        private HttpWebRequest CreateWebRequest(string endPoint)
        {
            var request = (HttpWebRequest)WebRequest.Create(endPoint);
            request.Method = "GET";
            request.ContentLength = 0;
            request.ContentType = "text/xml";
            return request;
        }

        public string GetMessage(string endPoint)
        {
            HttpWebRequest request = CreateWebRequest(endPoint);
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseValue = string.Empty;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }
                using (var responseStream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(responseStream))
                    {
                        responseValue = reader.ReadToEnd();
                    }
                }
                return responseValue;
            }
        }

        public void RenderMap()
        {
            if (cmbDataKey.SelectedIndex != -1 && cmbShapeKey.SelectedIndex != -1 && cmbValue.SelectedIndex != -1)
            {
                string shapeKey = cmbShapeKey.SelectedItem.ToString();
                string dataKey = cmbDataKey.SelectedItem.ToString();
                string value = cmbValue.SelectedItem.ToString();
                string missingText = legendText0.Text.ToString();
                Opacity = Convert.ToByte(sliderOpacity.Value);

                List<SolidColorBrush> brushList = new List<SolidColorBrush>() { 
                    (SolidColorBrush)rctColor1.Fill, 
                    (SolidColorBrush)rctColor2.Fill, 
                    (SolidColorBrush)rctColor3.Fill, 
                    (SolidColorBrush)rctColor4.Fill, 
                    (SolidColorBrush)rctColor5.Fill, 
                    (SolidColorBrush)rctColor6.Fill, 
                    (SolidColorBrush)rctColor7.Fill, 
                    (SolidColorBrush)rctColor8.Fill, 
                    (SolidColorBrush)rctColor9.Fill, 
                    (SolidColorBrush)rctColor10.Fill,  
                    (SolidColorBrush)rctColor0.Fill};

                int classCount;

                if (!int.TryParse(cmbClasses.Text, out classCount))
                {
                    classCount = 4;
                }

                if (thisProvider != null)
                {
                    thisProvider.Range = GetRangeValues(thisProvider.RangeCount);
                    thisProvider.ListLegendText = ListLegendText;
                    thisProvider.Opacity = this.Opacity;

                    thisProvider.SetShapeRangeValues(
                        _dashboardHelper,
                        cmbShapeKey.SelectedItem.ToString(),
                        cmbDataKey.SelectedItem.ToString(),
                        cmbValue.SelectedItem.ToString(),
                        brushList,
                        classCount,
                        missingText);
                }
            }
        }

        public List<double> GetRangeValues(int RangeCount)
        {
            List<double> Range = new List<double>();

            ListLegendText.Add(legendText1.Name, legendText1.Text);
            ListLegendText.Add(legendText2.Name, legendText2.Text);
            ListLegendText.Add(legendText3.Name, legendText3.Text);
            ListLegendText.Add(legendText4.Name, legendText4.Text);
            ListLegendText.Add(legendText5.Name, legendText5.Text);
            ListLegendText.Add(legendText6.Name, legendText6.Text);
            ListLegendText.Add(legendText7.Name, legendText7.Text);
            ListLegendText.Add(legendText8.Name, legendText8.Text);
            ListLegendText.Add(legendText9.Name, legendText9.Text);
            ListLegendText.Add(legendText10.Name, legendText10.Text);

            double value = 0.0;

            if (RangeCount >= 2)
            {
                value = 0.0;
                double.TryParse(rampStart01.Text, out value);
                Range.Add(value);


                value = 0.0;
                double.TryParse(rampStart02.Text, out value);
                Range.Add(value);
            }

            if (RangeCount >= 3)
            {
                value = 0.0;
                double.TryParse(rampStart03.Text, out value);
                Range.Add(value);
            }

            if (RangeCount >= 4)
            {
                value = 0.0;
                double.TryParse(rampStart04.Text, out value);
                Range.Add(value);
            }
            
            if (RangeCount >= 5)
            {
                value = 0.0;
                double.TryParse(rampStart05.Text, out value);
                Range.Add(value);
            }
            
            if (RangeCount >= 6)
            {
                value = 0.0;
                double.TryParse(rampStart06.Text, out value);
                Range.Add(value);
            }

            if (RangeCount >= 7)
            {
                value = 0.0;
                double.TryParse(rampStart07.Text, out value);
                Range.Add(value);
            }
            
            if (RangeCount >= 8)
            {
                value = 0.0;
                double.TryParse(rampStart08.Text, out value);
                Range.Add(value);
            }
            
            if (RangeCount >= 9)
            {
                value = 0.0;
                double.TryParse(rampStart09.Text, out value);
                Range.Add(value);
            }
            
            if (RangeCount >= 10)
            {
                value = 0.0;
                double.TryParse(rampStart10.Text, out value);
                Range.Add(value);
            }

            return Range;
        }

        public StackPanel LegendStackPanel
        {
            get
            {
                return thisProvider.LegendStackPanel;
            }
        }

        private void rctMissingColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctMissingColor.Fill = new SolidColorBrush(Color.FromRgb(dialog.Color.R, dialog.Color.G, dialog.Color.B));
                thisProvider.UseCustomColors = true;
                thisProvider.CustomColorsDictionary.Add(rctMissingColor.Name, Color.FromRgb(dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
            Reset_Legend();
        }

        private void rctLowColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctLowColor.Fill = new SolidColorBrush(Color.FromRgb(dialog.Color.R, dialog.Color.G, dialog.Color.B));
                thisProvider.UseCustomColors = true;
                thisProvider.CustomColorsDictionary.Add(rctLowColor.Name, Color.FromRgb(dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
            Reset_Legend();
        }

        private void rctHighColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();

            if (new System.Windows.Forms.ColorDialog().ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctHighColor.Fill = new SolidColorBrush(Color.FromRgb(dialog.Color.R, dialog.Color.G, dialog.Color.B));
                thisProvider.UseCustomColors = true;
                thisProvider.CustomColorsDictionary.Add(rctHighColor.Name, Color.FromRgb(dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
            Reset_Legend();
        }

        private void rctColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ((System.Windows.Shapes.Rectangle)sender).Fill = new SolidColorBrush(Color.FromRgb(dialog.Color.R, dialog.Color.G, dialog.Color.B));
                thisProvider.UseCustomColors = true;
                thisProvider.CustomColorsDictionary.Add(((System.Windows.Shapes.Rectangle)sender).Name, Color.FromRgb(dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void ClassCount_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (thisProvider != null)
            { 
                thisProvider.UseCustomRanges = false;
            }

            Reset_Legend();
        }

        private void Reset_Legend()
        {
            if (((ComboBoxItem)cmbClasses.SelectedItem).Content == null || thisProvider == null)
            {
                return;
            }

            if (rctLowColor == null || rctHighColor == null)
            {
                return;
            }

            int stratCount;


            if (int.TryParse(cmbClasses.Text, out stratCount) == false)
            {
                stratCount = 4;
            }

            int classCount;
            if (!int.TryParse(((ComboBoxItem)cmbClasses.SelectedItem).Content.ToString(), out classCount))
            {
                classCount = 4;
            }
            
            if ((cmbShapeKey.SelectedItem != null && cmbDataKey.SelectedItem != null && cmbValue.SelectedItem != null) &&
                (thisProvider != null))
            {
                if (thisProvider.AsQuantiles)
                {
                    thisProvider.RangesLoadedFromMapFile = false;

                    thisProvider.ResetRangeValues(
                        cmbShapeKey.SelectedItem.ToString(),
                        cmbDataKey.SelectedItem.ToString(),
                        cmbValue.SelectedItem.ToString(),
                        classCount
                        );
                }
            }

            SolidColorBrush rampStart = (SolidColorBrush)rctLowColor.Fill;
            SolidColorBrush rampEnd = (SolidColorBrush)rctHighColor.Fill;

            if (cmbShapeKey.SelectedItem != null && cmbDataKey.SelectedItem != null && cmbValue.SelectedItem != null)
            {
                thisProvider.UseCustomColors = false;
                thisProvider.UseCustomRanges = false;

                SetRangeUISection();
            }

            if (string.IsNullOrEmpty(legTitle.Text))
            {
                if (thisProvider.LegendText != null)
                {
                    legTitle.Text = thisProvider.LegendText;
                }
                else if (_value != null)
                {
                    legTitle.Text = _value;
                }
            }

            PropertyChanged_EnableDisable();
        }


        public void SetVisibility(int stratCount, SolidColorBrush rampStart, SolidColorBrush rampEnd)
        {
            if (thisProvider == null) return;
            
            bool isNewColorRamp = true;

            quintilesOption.IsChecked = thisProvider.AsQuantiles;

            SolidColorBrush rampMissing = (SolidColorBrush)rctMissingColor.Fill;

            if (Equals(rampStart, _currentColor_rampStart) &&
                Equals(rampEnd, _currentColor_rampEnd) &&
                Equals(rampMissing, _currentColor_rampMissing) &&
                stratCount == _currentStratCount)
            {
                if (_initialRampCalc == false)
                {
                    isNewColorRamp = false;
                }
            }

            _currentStratCount = stratCount;
            _currentColor_rampStart = rampStart;
            _currentColor_rampMissing = rampMissing;
            _currentColor_rampEnd = rampEnd;

            int rd = rampStart.Color.R - rampEnd.Color.R;
            int gd = rampStart.Color.G - rampEnd.Color.G;
            int bd = rampStart.Color.B - rampEnd.Color.B;

            byte ri = (byte)(rd / (stratCount - 1));
            byte gi = (byte)(gd / (stratCount - 1));
            byte bi = (byte)(bd / (stratCount - 1));

            if (thisProvider != null)
            {
                if (thisProvider.UseCustomColors)
                {
                    rctColor0.Fill = new SolidColorBrush(thisProvider.CustomColorsDictionary.GetWithKey(rctColor0.Name));
                    rctColor1.Fill = new SolidColorBrush(thisProvider.CustomColorsDictionary.GetWithKey(rctColor1.Name));
                }
                else
                {
                    rctColor0.Fill = rampMissing;
                    thisProvider.CustomColorsDictionary.Add(rctColor0.Name, rampMissing.Color);
                    rctColor1.Fill = rampStart;
                    thisProvider.CustomColorsDictionary.Add(rctColor1.Name, rampStart.Color);
                }

                if (thisProvider != null && quintilesOption.IsChecked == true)
                {
                    rampStart01.Text = thisProvider.RangeValues[0, 0];
                    rampEnd01.Text = thisProvider.RangeValues[0, 1];
                }

                Color color;
                int i = 2;
                int gradientControl = 1;

                List<UIControls> uics = CreateUIControlsList();

                foreach (UIControls uiControls in uics)
                {
                    if (thisProvider.UseCustomColors)
                    {
                        color = thisProvider.CustomColorsDictionary.GetWithKey(uiControls.rectangle.Name);
                    }
                    else
                    {
                        color = Color.FromArgb(255, 
                            (byte)(rampStart.Color.R - ri * gradientControl), 
                            (byte)(rampStart.Color.G - gi * gradientControl), 
                            (byte)(rampStart.Color.B - bi * gradientControl));

                        thisProvider.CustomColorsDictionary.Add(uiControls.rectangle.Name, color);
                    }

                    gradientControl++;

                    uiControls.rectangle.Visibility = System.Windows.Visibility.Visible;
                    uiControls.rampStarts.Visibility = System.Windows.Visibility.Visible;
                    uiControls.centerTexts.Visibility = System.Windows.Visibility.Visible;
                    uiControls.rampEnds.Visibility = System.Windows.Visibility.Visible;
                    uiControls.legedTexts.Visibility = System.Windows.Visibility.Visible;
                    
                    if (i++ > stratCount)
                    {
                        color = Color.FromArgb(Opacity, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                        uiControls.rectangle.Visibility = System.Windows.Visibility.Hidden;
                        uiControls.rampStarts.Visibility = System.Windows.Visibility.Hidden;
                        uiControls.centerTexts.Visibility = System.Windows.Visibility.Hidden;
                        uiControls.rampEnds.Visibility = System.Windows.Visibility.Hidden;
                        uiControls.legedTexts.Visibility = System.Windows.Visibility.Hidden;
                    }

                    if (isNewColorRamp) uiControls.rectangle.Fill = new SolidColorBrush(color);

                    if (thisProvider != null && quintilesOption.IsChecked == true)
                    {
                        if (thisProvider.UseCustomRanges)
                        {
                            uiControls.rampStarts.Text = thisProvider.ClassRangesDictionary.Dict[uiControls.rampStarts.Name];
                            uiControls.rampEnds.Text = thisProvider.ClassRangesDictionary.Dict[uiControls.rampEnds.Name];
                        }
                        else
                        {
                            uiControls.rampStarts.Text = thisProvider.RangeValues[i - 2, 0];
                            uiControls.rampEnds.Text = thisProvider.RangeValues[i - 2, 1];
                        }
                    }
                }

                EnableDisableClassRangeInput();
            }

            _initialRampCalc = false;
        }

        private void EnableDisableClassRangeInput()
        {
            foreach (UIElement element in stratGrid.Children)
            {
                if (element is System.Windows.Controls.TextBox)
                {
                    string elementName = ((System.Windows.Controls.TextBox)element).Name;

                    if (elementName.StartsWith("ramp"))
                    {
                        ((System.Windows.Controls.TextBox)element).IsEnabled = !(bool)quintilesOption.IsChecked;
                    }
                }
            }
        }

        private List<UIControls> CreateUIControlsList()
        {
            return new List<UIControls>()
            {
                new UIControls()
                {
                    centerTexts = centerText02,
                    legedTexts = legendText2,
                    rectangle = rctColor2,
                    rampEnds = rampEnd02,
                    rampStarts = rampStart02
                },
                new UIControls()
                {
                    centerTexts = centerText03,
                    legedTexts = legendText3,
                    rectangle = rctColor3,
                    rampEnds = rampEnd03,
                    rampStarts = rampStart03
                }, 
                new UIControls()
                {
                    centerTexts = centerText04,
                    legedTexts = legendText4,
                    rectangle = rctColor4,
                    rampEnds = rampEnd04,
                    rampStarts = rampStart04
                },
                new UIControls()
                {
                    centerTexts = centerText05,
                    legedTexts = legendText5,
                    rectangle = rctColor5,
                    rampEnds = rampEnd05,
                    rampStarts = rampStart05
                },
                new UIControls()
                {
                    centerTexts = centerText06,
                    legedTexts = legendText6,
                    rectangle = rctColor6,
                    rampEnds = rampEnd06,
                    rampStarts = rampStart06
                },
                new UIControls()
                {
                    centerTexts = centerText07,
                    legedTexts = legendText7,
                    rectangle = rctColor7,
                    rampEnds = rampEnd07,
                    rampStarts = rampStart07
                },
                new UIControls()
                {
                    centerTexts = centerText08,
                    legedTexts = legendText8,
                    rectangle = rctColor8,
                    rampEnds = rampEnd08,
                    rampStarts = rampStart08
                },
                new UIControls()
                {
                    centerTexts = centerText09,
                    legedTexts = legendText9,
                    rectangle = rctColor9,
                    rampEnds = rampEnd09,
                    rampStarts = rampStart09
                },
                new UIControls()
                {
                    centerTexts = centerText10,
                    legedTexts = legendText10,
                    rectangle = rctColor10,
                    rampEnds = rampEnd10 ,
                    rampStarts = rampStart10  
                },
            };
        }

        private void CheckBox_Quantiles_Click(object sender, RoutedEventArgs e)
        {
            OnQuintileOptionChanged();
        }

        public void OnQuintileOptionChanged()
        {
            if (thisProvider != null)
            { 
                thisProvider.AsQuantiles = (bool)quintilesOption.IsChecked;
            }

            int widthQuantile = 0;
            int widthMinMax = 75;
            int widthCompare = 50;

            if (quintilesOption.IsChecked == false)
            {

            }
            else
            {
                Reset_Legend();
            }

            quintileColumn.Width = new GridLength(widthQuantile, GridUnitType.Pixel);

            rampStartColumn.Width = new GridLength(widthMinMax, GridUnitType.Pixel);
            rampCompareColumn.Width = new GridLength(widthCompare, GridUnitType.Pixel);
            rampEndColumn.Width = new GridLength(widthMinMax, GridUnitType.Pixel);

            EnableDisableClassRangeInput();
        }
        
        public void cmbShapeKey_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbShapeKey.SelectedItem != null)
            {
                _shapeKey = cmbShapeKey.SelectedItem.ToString();
                cmbDataKey.IsEnabled = true;
            }
            else
            { 
                _shapeKey = ""; 
            }

            PropertyChanged_EnableDisable();
        }

        private void cmbDataKey_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReFillValueComboBoxes();

            if (cmbDataKey.SelectedItem != null)
            {
                _dataKey = cmbDataKey.SelectedItem.ToString();
                cmbValue.IsEnabled = true;
            }

            if (cmbValue.Items.Contains(_dataKey))
            {
                cmbValue.Items.Remove(_dataKey);
            }

            PropertyChanged_EnableDisable();
        }

        private void cmbValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbValue.SelectedItem != null)
            {
                _value = cmbValue.SelectedItem.ToString();
            }
            PropertyChanged_EnableDisable();
        }

        private void SetRangeUISection()
        {
            List<SolidColorBrush> brushList = new List<SolidColorBrush>() { 
                    (SolidColorBrush)rctColor1.Fill, 
                    (SolidColorBrush)rctColor2.Fill, 
                    (SolidColorBrush)rctColor3.Fill, 
                    (SolidColorBrush)rctColor4.Fill, 
                    (SolidColorBrush)rctColor5.Fill, 
                    (SolidColorBrush)rctColor6.Fill, 
                    (SolidColorBrush)rctColor7.Fill, 
                    (SolidColorBrush)rctColor8.Fill, 
                    (SolidColorBrush)rctColor9.Fill, 
                    (SolidColorBrush)rctColor10.Fill,  
                    (SolidColorBrush)rctColor0.Fill };

            int classCount;

            if (!int.TryParse(((ComboBoxItem)cmbClasses.SelectedItem).Content.ToString(), out classCount))
            {
                classCount = 4;
            }

            if (radShapeFile.IsChecked == true && thisProvider != null)
            {
                if (quintilesOption.IsChecked == true)
                {
                    thisProvider.PopulateRangeValues();
                }

                if (string.IsNullOrEmpty(legTitle.Text))
                {
                    legTitle.Text = thisProvider.LegendText;
                }
                else
                {
                    thisProvider.LegendText = legTitle.Text;
                }

                try
                {
                    for (int x = 1; x <= ChoroplethConstants.MAX_CLASS_DECRIPTION_COUNT; x++)
                    {
                        System.Windows.Controls.TextBox t = FindName(ChoroplethConstants.legendTextControlPrefix + x) as System.Windows.Controls.TextBox;
                        
                        if (string.IsNullOrEmpty(t.Text))
                        {
                            t.Text = thisProvider.ListLegendText.GetWithKey(ChoroplethConstants.legendTextControlPrefix + x);
                        }
                        else
                        {
                            thisProvider.ListLegendText.Add(ChoroplethConstants.legendTextControlPrefix + x, t.Text);
                    }
                    }
                }
                catch (Exception)
                {
                    throw;
                }

                thisProvider.AreRangesSet = true;
            }
            else if (radMapServer.IsChecked == true && choroplethServerLayerProvider != null)
            {
                if (thisProvider == null)
                {
                    thisProvider = choroplethServerLayerProvider;
                }

                if (quintilesOption.IsChecked == true)
                {
                    thisProvider.PopulateRangeValues();
                }

                if (string.IsNullOrEmpty(legTitle.Text))
                {
                    legTitle.Text = thisProvider.LegendText;
                }
                else
                {
                    thisProvider.LegendText = legTitle.Text;
                }

                try
                {
                    for (int x = 1; x <= ChoroplethConstants.MAX_CLASS_DECRIPTION_COUNT; x++)
                    {
                        System.Windows.Controls.TextBox t = FindName(ChoroplethConstants.legendTextControlPrefix + x) as System.Windows.Controls.TextBox;
                        if (string.IsNullOrEmpty(t.Text))
                        {
                            t.Text = thisProvider.ListLegendText.GetWithKey(ChoroplethConstants.legendTextControlPrefix + x);
                        }
                        else
                        {
                            thisProvider.ListLegendText.Add(ChoroplethConstants.legendTextControlPrefix + x, t.Text);
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }

                thisProvider = choroplethServerLayerProvider;
                thisProvider.AreRangesSet = true;
            }
            else if (radKML.IsChecked == true && choroplethKmlLayerProvider != null)
            {
                if (thisProvider == null)
                {
                    thisProvider = choroplethKmlLayerProvider;
                }

                if (quintilesOption.IsChecked == true)
                {
                    thisProvider.PopulateRangeValues();
                }

                if (string.IsNullOrEmpty(legTitle.Text))
                {
                    legTitle.Text = thisProvider.LegendText;
                }
                else
                {
                    thisProvider.LegendText = legTitle.Text;
                }

                try
                {
                    for (int x = 1; x <= ChoroplethConstants.MAX_CLASS_DECRIPTION_COUNT; x++)
                    {
                        System.Windows.Controls.TextBox t = FindName(ChoroplethConstants.legendTextControlPrefix + x) as System.Windows.Controls.TextBox;
                        if (string.IsNullOrEmpty(t.Text))
                        {
                            t.Text = thisProvider.ListLegendText.GetWithKey(ChoroplethConstants.legendTextControlPrefix + x);
                        }
                        else
                        {
                            thisProvider.ListLegendText.Add(ChoroplethConstants.legendTextControlPrefix + x, t.Text);
                    }
                    }
                }
                catch (Exception)
                {
                    throw;
                }

                thisProvider = choroplethKmlLayerProvider;
                thisProvider.AreRangesSet = true;
            }

            SolidColorBrush rampStart = (SolidColorBrush)rctLowColor.Fill;
            SolidColorBrush rampEnd = (SolidColorBrush)rctHighColor.Fill;

            SetVisibility(classCount, rampStart, rampEnd);
        }

        private List<double> GetRangeValuesFromAttributeList(Dictionary<int, object> dictionary, int classCount)
        {
            List<double> RangeAttr = new List<double>();
            double value = 0.0;

            if (dictionary == null)
                return RangeAttr;

            if (classCount >= 2 && dictionary.Count > 1)
            {
                classAttributes class1 = (classAttributes)dictionary[1];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);

                class1 = (classAttributes)dictionary[2];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);
            }

            if (classCount >= 3 && dictionary.Count > 2)
            {
                classAttributes class1 = (classAttributes)dictionary[3];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);
            }
            if (classCount >= 4 && dictionary.Count > 3)
            {
                classAttributes class1 = (classAttributes)dictionary[4];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);
            }
            if (classCount >= 5 && dictionary.Count > 4)
            {
                classAttributes class1 = (classAttributes)dictionary[5];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);
            }
            if (classCount >= 6 && dictionary.Count > 5)
            {
                classAttributes class1 = (classAttributes)dictionary[6];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);
            }
            if (classCount >= 7 && dictionary.Count > 6)
            {
                classAttributes class1 = (classAttributes)dictionary[7];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);
            }
            if (classCount >= 8 && dictionary.Count > 7)
            {
                classAttributes class1 = (classAttributes)dictionary[8];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);
            }
            if (classCount >= 9 && dictionary.Count > 8)
            {
                classAttributes class1 = (classAttributes)dictionary[9];
                double.TryParse(class1.rampStart, out value);
                RangeAttr.Add(value);
            }

            return RangeAttr;
        }

        private void btnBrowseShapeFile_Click(object sender, RoutedEventArgs e)
        {
            thisProvider = new Mapping.ChoroplethShapeLayerProvider(_myMap);
            object[] shapeFileProperties = thisProvider.Load();

            if (shapeFileProperties != null)
            {
                layerAddednew.Add(thisProvider._layerId.ToString());
                
                if (choroplethShapeLayerProperties == null)
                {
                    ILayerProperties layerProperties = null;
                    layerProperties = new ChoroplethShapeLayerProperties(_myMap, this.DashboardHelper, this._mapControl);
                    layerProperties.MapGenerated += new EventHandler(this._mapControl.ILayerProperties_MapGenerated);
                    layerProperties.FilterRequested += new EventHandler(this._mapControl.ILayerProperties_FilterRequested);
                    layerProperties.EditRequested += new EventHandler(this._mapControl.ILayerProperties_EditRequested);
                    this.choroplethShapeLayerProperties = (ChoroplethShapeLayerProperties)layerProperties;
                    this._mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
                }

                choroplethShapeLayerProperties.provider = thisProvider as ChoroplethShapeLayerProvider;

                if (this.DashboardHelper != null)
                {
                    choroplethShapeLayerProperties.SetdashboardHelper(DashboardHelper);
                }

                if (shapeFileProperties.Length == 2)
                {
                    txtShapePath.Text = shapeFileProperties[0].ToString();
                    choroplethShapeLayerProperties.shapeFilePath = shapeFileProperties[0].ToString();
                    choroplethShapeLayerProperties.shapeAttributes = (IDictionary<string, object>)shapeFileProperties[1];
                    shapeAttributes = (IDictionary<string, object>)shapeFileProperties[1];

                    if (shapeAttributes != null)
                    {
                        cmbShapeKey.Items.Clear();
                        choroplethShapeLayerProperties.cbxShapeKey.Items.Clear();
                        
                        foreach (string key in shapeAttributes.Keys)
                        {
                            cmbShapeKey.Items.Add(key);
                            choroplethShapeLayerProperties.cbxShapeKey.Items.Add(key);
                        }
                    }
                }
            }
        }

        private void btnKMLFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "KML/KMZ Files (*.kml/*.kmz)|*.kml;*.kmz";
            dialog.Multiselect = false;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtKMLpath.Text = dialog.FileName;
                KMLMapServerName = txtKMLpath.Text;
            }

            if (choroplethKmlLayerProvider == null)
            {
                choroplethKmlLayerProvider = new Mapping.ChoroplethKmlLayerProvider(_myMap);
                choroplethKmlLayerProvider.FeatureLoaded += new FeatureLoadedHandler(choroKMLprovider_FeatureLoaded);
            }

            thisProvider = choroplethKmlLayerProvider;

            object[] kmlFileProperties = choroplethKmlLayerProvider.Load(KMLMapServerName);

            if (kmlFileProperties != null)
            {
                layerAddednew.Add(choroplethKmlLayerProvider._layerId.ToString());
                
                if (choroplethKmlLayerProperties == null)
                {
                    ILayerProperties layerProperties = null;
                    layerProperties = new ChoroplethKmlLayerProperties(_myMap, this.DashboardHelper, this._mapControl);
                    layerProperties.MapGenerated += new EventHandler(this._mapControl.ILayerProperties_MapGenerated);
                    layerProperties.FilterRequested += new EventHandler(this._mapControl.ILayerProperties_FilterRequested);
                    layerProperties.EditRequested += new EventHandler(this._mapControl.ILayerProperties_EditRequested);
                    this.choroplethKmlLayerProperties = (ChoroplethKmlLayerProperties)layerProperties;
                    this._mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
                }
                
                choroplethKmlLayerProperties.shapeFilePath = KMLMapServerName;
                choroplethKmlLayerProperties.provider = choroplethKmlLayerProvider;
                choroplethKmlLayerProperties.provider.FeatureLoaded += new FeatureLoadedHandler(choroplethKmlLayerProperties.provider_FeatureLoaded);
                
                if (this.DashboardHelper != null)
                {
                    choroplethKmlLayerProperties.SetdashboardHelper(DashboardHelper);
                }
            }
        }

        public void MapFeatureSelectionChange()
        {
            if (cbxmapfeature.Items.Count > 0)
            {
                MapServerName = txtMapSeverpath.Text;
                
                if (cbxmapfeature.SelectedIndex > -1)
                {
                    int visibleLayer = ((SubObject)cbxmapfeature.SelectedItem).id;
                    MapVisibleLayer = visibleLayer;
                    
                    if (choroplethServerLayerProvider == null)
                    {
                        choroplethServerLayerProvider = new Mapping.ChoroplethServerLayerProvider(_myMap);
                        choroplethServerLayerProvider.FeatureLoaded += new FeatureLoadedHandler(choroMapprovider_FeatureLoaded);
                    }
                    
                    object[] mapFileProperties = choroplethServerLayerProvider.Load(MapServerName + "/" + MapVisibleLayer);
                    
                    if (mapFileProperties != null)
                    {
                        layerAddednew.Add(choroplethServerLayerProvider._layerId.ToString());
                        
                        if (choroplethServerLayerProperties == null)
                        {
                            ILayerProperties layerProperties = null;
                            layerProperties = new ChoroplethServerLayerProperties(_myMap, this.DashboardHelper, this._mapControl);
                            layerProperties.MapGenerated += new EventHandler(this._mapControl.ILayerProperties_MapGenerated);
                            layerProperties.FilterRequested += new EventHandler(this._mapControl.ILayerProperties_FilterRequested);
                            layerProperties.EditRequested += new EventHandler(this._mapControl.ILayerProperties_EditRequested);
                            this.choroplethServerLayerProperties = (ChoroplethServerLayerProperties)layerProperties;
                            this._mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
                        }
                        
                        choroplethServerLayerProperties.shapeFilePath = MapServerName;
                        choroplethServerLayerProperties.provider = choroplethServerLayerProvider;
                        choroplethServerLayerProperties.provider.FeatureLoaded += new FeatureLoadedHandler(choroplethServerLayerProperties.provider_FeatureLoaded);

                        if (this.DashboardHelper != null)
                        {
                            choroplethServerLayerProperties.SetdashboardHelper(DashboardHelper);
                        }
                    }
                }
                else
                {
                    MapVisibleLayer = -1;
                }
            }
        }

        private void Addfilters()
        {
            try
            {
                this.datafilters = rowFilterControl.DataFilters;

                if (radShapeFile.IsChecked == true && this.choroplethShapeLayerProperties != null) 
                { 
                    choroplethShapeLayerProperties.datafilters = rowFilterControl.DataFilters; 
                }
                
                if (radMapServer.IsChecked == true && this.choroplethServerLayerProperties != null) 
                {
                    choroplethServerLayerProperties.datafilters = rowFilterControl.DataFilters;
                }

                if (radKML.IsChecked == true && choroplethKmlLayerProperties != null)
                {
                    choroplethKmlLayerProperties.datafilters = rowFilterControl.DataFilters;
                }

                this.datafilters = rowFilterControl.DataFilters;
                _dashboardHelper.SetDatafilters(this.datafilters);
            }
            catch (Exception)
            {
                throw new Exception();
            }
        }

        public void choroMapprovider_FeatureLoaded(string serverName, IDictionary<string, object> featureAttributes)
        {
            if (!string.IsNullOrEmpty(serverName))
            {
                shapeFilePath = serverName;
                if (featureAttributes != null)
                {
                    cmbShapeKey.Items.Clear();
                    cmbShapeKey.SelectedIndex = -1;
                    choroplethServerLayerProperties.cbxShapeKey.Items.Clear();
                    foreach (string key in featureAttributes.Keys)
                    {
                        cmbShapeKey.Items.Add(key);
                        choroplethServerLayerProperties.cbxShapeKey.Items.Add(key);
                    }
                }
            }
            if (currentElement != null)
            {
                foreach (System.Xml.XmlElement child in currentElement.ChildNodes)
                {
                    if (child.Name.Equals("dataKey"))
                    {
                        cmbDataKey.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("shapeKey"))
                    {
                        cmbShapeKey.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("value"))
                    {
                        cmbValue.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("dotValue"))
                    {
                        // txtDotValue.Text = child.InnerText;
                    }
                    if (child.Name.Equals("dotColor"))
                    {
                        //rctDotColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                    }
                }

                legTitle.Text = thisProvider.LegendText;
            }
        }

        void choroKMLprovider_FeatureLoaded(string serverName, IDictionary<string, object> featureAttributes)
        {
            if (!string.IsNullOrEmpty(serverName))
            {
                shapeFilePath = serverName;
                if (featureAttributes != null)
                {
                    cmbShapeKey.Items.Clear();
                    choroplethKmlLayerProperties.cbxShapeKey.Items.Clear();
                    foreach (string key in featureAttributes.Keys)
                    {
                        cmbShapeKey.Items.Add(key);
                        choroplethKmlLayerProperties.cbxShapeKey.Items.Add(key);
                    }
                }
            }
            if (currentElement != null)
            {
                foreach (System.Xml.XmlElement child in currentElement.ChildNodes)
                {
                    if (child.Name.Equals("dataKey"))
                    {
                        cmbDataKey.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("shapeKey"))
                    {
                        cmbShapeKey.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("value"))
                    {
                        cmbValue.SelectedItem = child.InnerText;
                    }
                    if (child.Name.Equals("dotValue"))
                    {
                        //txtDotValue.Text = child.InnerText;
                    }
                    if (child.Name.Equals("dotColor"))
                    {
                        //rctDotColor.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(child.InnerText));
                    }
                }
                // RenderMap();
            }
        }


        private void radShapeFile_Checked(object sender, RoutedEventArgs e)
        {
            //check for edit mode
            //if (ClassAttribList.Count != 0) { return; }
            if (!string.IsNullOrEmpty(txtKMLpath.Text))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(DashboardSharedStrings.MAP_CHANGE_BOUNDARY_ALERT, DashboardSharedStrings.ALERT, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    radShapeFile.IsChecked = false;
                    radKML.IsChecked = true;
                    return;
                }
                else
                    ClearonShapeFile();
            }
            else if (!string.IsNullOrEmpty(txtMapSeverpath.Text))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(DashboardSharedStrings.MAP_CHANGE_BOUNDARY_ALERT, DashboardSharedStrings.ALERT, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    radShapeFile.IsChecked = false;
                    radMapServer.IsChecked = true;
                    return;
                }
                else
                    ClearonShapeFile();
            }
            else if (!string.IsNullOrEmpty(txtShapePath.Text))
            {
                return;
            }
            else
                ClearonShapeFile();
        }

        private void ClearonKML()
        {
            panelshape.IsEnabled = false;
            panelmap.IsEnabled = false;
            panelKml.IsEnabled = true;

            panelmapserver.IsEnabled = false;

            cmbShapeKey.Items.Clear();
            txtShapePath.Text = string.Empty;
            txtKMLpath.Text = string.Empty;
            cbxmapfeature.SelectedIndex = -1;
            txtMapSeverpath.Text = string.Empty;
            if (choroplethServerLayerProvider != null)
            {
                choroplethServerLayerProvider.FeatureLoaded -= new FeatureLoadedHandler(choroMapprovider_FeatureLoaded);
                choroplethServerLayerProvider = null;
            }
            if (thisProvider != null)
            {
                thisProvider.CloseLayer();
            }
        }

        public void ClearonShapeFile()
        {
            panelshape.IsEnabled = true;
            panelmap.IsEnabled = false;
            panelKml.IsEnabled = false;

            panelmapserver.IsEnabled = false;

            cmbShapeKey.Items.Clear();
            txtShapePath.Text = string.Empty;
            txtKMLpath.Text = string.Empty;
            cbxmapfeature.SelectedIndex = -1;
            txtMapSeverpath.Text = string.Empty;
            if (choroplethServerLayerProvider != null)
            {
                choroplethServerLayerProvider.FeatureLoaded -= new FeatureLoadedHandler(choroMapprovider_FeatureLoaded);
                choroplethServerLayerProvider = null;
            }
            if (choroplethKmlLayerProvider != null)
            {
                choroplethKmlLayerProvider.FeatureLoaded -= new FeatureLoadedHandler(choroKMLprovider_FeatureLoaded);
                choroplethKmlLayerProvider = null;
            }
        }

        public void ClearonMapServer()
        {
            panelshape.IsEnabled = false;
            panelmap.IsEnabled = true;
            panelKml.IsEnabled = false;

            panelmapserver.IsEnabled = true;

            cmbShapeKey.Items.Clear();
            txtShapePath.Text = string.Empty;
            txtKMLpath.Text = string.Empty;
            cbxmapfeature.SelectedIndex = -1;
            txtMapSeverpath.Text = string.Empty;
            
            if (choroplethKmlLayerProvider != null)
            {
                choroplethKmlLayerProvider.FeatureLoaded -= new FeatureLoadedHandler(choroKMLprovider_FeatureLoaded);
                choroplethKmlLayerProvider = null;
            }
            
            if (thisProvider != null)
            {
                choroplethShapeLayerProperties.CloseLayer();
            }
        }


        private void radKML_Checked(object sender, RoutedEventArgs e)
        {

            if (!string.IsNullOrEmpty(txtShapePath.Text))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(DashboardSharedStrings.MAP_CHANGE_BOUNDARY_ALERT, DashboardSharedStrings.ALERT, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    radKML.IsChecked = false;
                    radShapeFile.IsChecked = true;
                    return;
                }
                else
                    ClearonKML();
            }
            else if (!string.IsNullOrEmpty(txtMapSeverpath.Text))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(DashboardSharedStrings.MAP_CHANGE_BOUNDARY_ALERT, DashboardSharedStrings.ALERT, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    radKML.IsChecked = false;
                    radMapServer.IsChecked = true;
                    return;
                }
                else
                    ClearonKML();
            }
            else if (!string.IsNullOrEmpty(txtKMLpath.Text))
            {
                return;
            }
            else
                ClearonKML();
        }

        private void radMapServer_Checked(object sender, RoutedEventArgs e)
        {

            if (!string.IsNullOrEmpty(txtShapePath.Text))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(DashboardSharedStrings.MAP_CHANGE_BOUNDARY_ALERT, DashboardSharedStrings.ALERT, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    radMapServer.IsChecked = false;
                    radShapeFile.IsChecked = true;
                    return;
                }
                else
                {
                    ClearonMapServer();
                }
            }
            else if (!string.IsNullOrEmpty(txtKMLpath.Text))
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(DashboardSharedStrings.MAP_CHANGE_BOUNDARY_ALERT, DashboardSharedStrings.ALERT, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    radMapServer.IsChecked = false;
                    radKML.IsChecked = true;
                    return;
                }
                else
                {
                    ClearonMapServer();
                }
            }
            else if (!string.IsNullOrEmpty(txtMapSeverpath.Text) )
            {
                return;
            }
            else
            {
                ClearonMapServer();
            }
        }

        private void radconnectmapserver_Checked(object sender, RoutedEventArgs e)
        {
            panelmapconnect.IsEnabled = true;
            panelmapserver.IsEnabled = false;
            panelmapconnect.IsEnabled = true;
            txtMapSeverpath.Text = string.Empty;

            panelmapconnect.IsEnabled = false;
            panelmapserver.IsEnabled = true;
            panelmapconnect.IsEnabled = false;
        }

        public void cbxmapserver_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PropertyChanged_EnableDisable();
        }

        private void ResetShapeCombo()
        {
            //In case of map server
            if (radMapServer.IsChecked == true)
            {
                cmbShapeKey.Items.Clear();
                cmbShapeKey.SelectedIndex = -1;
                if (choroplethServerLayerProperties != null)
                {
                    choroplethServerLayerProperties.cbxShapeKey.Items.Clear();
                    choroplethServerLayerProperties.cbxShapeKey.SelectedIndex = -1;
                }
                /*
                //set back choroMapprovider
                if (choroMapprovider == null)
                {
                    choroMapprovider = new Mapping.ChoroplethServerLayerProvider(_myMap);
                    choroMapprovider.FeatureLoaded += new FeatureLoadedHandler(choroMapprovider_FeatureLoaded);
                    choroplethServerLayerProperties.provider.FeatureLoaded -= new FeatureLoadedHandler(choroplethServerLayerProperties.provider_FeatureLoaded);
                 } */
            }
        }

        private void btnMapserverlocate_Click(object sender, RoutedEventArgs e)
        {
            MapServerConnect();
        }

        public void MapServerConnect()
        {
            try
            {
                string message = GetMessage(txtMapSeverpath.Text + "?f=json");
                System.Web.Script.Serialization.JavaScriptSerializer ser = new System.Web.Script.Serialization.JavaScriptSerializer();
                Rest rest = ser.Deserialize<Rest>(message);
                cbxmapfeature.ItemsSource = rest.layers;
            }
            catch
            {
                cbxmapfeature.DataContext = null;
                System.Windows.Forms.MessageBox.Show("Invalid map server");
            }
        }

        private void txtMapSeverpath_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnMapserverlocate.IsEnabled = txtMapSeverpath.Text.Length > 0;
        }

        public void cbxmapfeature_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(sender is System.Windows.Controls.ComboBox)
            {
                if (((System.Windows.Controls.ComboBox)sender).SelectedIndex != -1)
                {
                    PropertyChanged_EnableDisable();
                }
            }

            MapFeatureSelectionChange();
        }

        private void Slider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender == null || txtOpacity == null)
            {
                return;
            }
            double value = ((Slider)sender).Value;
            double calculatedPercentage = (value - 100) / (255 - 100);
            txtOpacity.Text = string.Format("{0:N2}", calculatedPercentage * 100) + "%";

            brush.Opacity = calculatedPercentage;
        }

        private void PropertyChanged_EnableDisable()
        {
            if (btnOK == null) return;

            if (!string.IsNullOrEmpty(txtProjectPath.Text) && (!string.IsNullOrEmpty(txtShapePath.Text) || (!string.IsNullOrEmpty(txtMapSeverpath.Text) || (!string.IsNullOrEmpty(txtKMLpath.Text)))))
            {
                if (tbtnVariables.IsEnabled == false)
                {
                    tbtnVariables.IsEnabled = true;
                    tbtnVariables_Checked(this, new RoutedEventArgs());
                }

                if(cmbShapeKey.SelectedIndex != -1 && cmbDataKey.SelectedIndex != -1 && cmbValue.SelectedIndex != -1)
                {

                    if (tbtnDisplay.IsEnabled == false)
                    {
                        tbtnDisplay.IsEnabled = true;
                        tbtnDisplay_Checked(this, new RoutedEventArgs());
                    }
                    
                    if (IsMissingLimitValue() == false)
                    {
                        btnOK.IsEnabled = true; 
                    }
                    else
                    {
                        btnOK.IsEnabled = false;
                    }

                    tbtnDisplay.IsEnabled = true;
                    tbtnFilters.IsEnabled = true;
                    tbtnFilters.Visibility = Visibility.Visible;
                    return;                
                }
                else
                {
                    tbtnDisplay.IsEnabled = false;
                    tbtnFilters.IsEnabled = false;
                    btnOK.IsEnabled = false;
                }
            }
            else
            {
                tbtnVariables.IsEnabled = false;
                tbtnDisplay.IsEnabled = false;
                tbtnFilters.IsEnabled = false;
                btnOK.IsEnabled = false;
            }
        }

        private void PropertyChanged_EnableDisable(object sender, TextChangedEventArgs e)
        {
            PropertyChanged_EnableDisable();
        }

        private void rampValue_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((sender is System.Windows.Controls.TextBox) == false)
            {
                return;
            }

            thisProvider.UseCustomRanges = true;

            System.Windows.Controls.TextBox textBox = ((System.Windows.Controls.TextBox)sender);
            string newText = textBox.Text;
            float newValue = float.NaN;

            string name = textBox.Name;

            int classLevel = thisProvider.ClassRangesDictionary.GetClassLevelWithKey(name);
            ClassLimitType limit = thisProvider.ClassRangesDictionary.GetLimitTypeWithKey(name);

            if (float.TryParse(newText, out newValue) == false)
            {
                System.Windows.Controls.TextBox found = (System.Windows.Controls.TextBox)this.FindName(name);
                found.Text = thisProvider.ClassRangesDictionary.Dict[name];
                return;
            }

            UpdateRangesCollection();

            if (IsMissingLimitValue())
            {
                return;
            }

            List<ClassLimits> limits = thisProvider.ClassRangesDictionary.GetLimitValues();

            AdjustClassBreaks(limits, newValue, classLevel, limit);

            thisProvider.ClassRangesDictionary.SetRangesDictionary(limits);

            foreach (KeyValuePair<string, string> kvp in thisProvider.ClassRangesDictionary.Dict)
            {
                System.Windows.Controls.TextBox found = (System.Windows.Controls.TextBox)this.FindName(kvp.Key);
                found.Text = kvp.Value;
            }

            PropertyChanged_EnableDisable();
        }
                                
        private void AdjustClassBreaks(List<ClassLimits> classBreaks, float newValue, int classLevel, ClassLimitType limitType = ClassLimitType.Start)
        {
            ReplaceClassLimit(classBreaks, newValue, classLevel, limitType);

            if (limitType == ClassLimitType.Start)
            {
                if (classBreaks[classLevel].End <= newValue)
                {
                    limitType = ClassLimitType.End;
                    AdjustClassBreaks(classBreaks, (float)(newValue + 0.01), classLevel, limitType);
                }

                classLevel -= 1;
                if (classLevel < 0) return;

                if (classBreaks[classLevel].End != newValue)
                {
                    limitType = ClassLimitType.End;
                    AdjustClassBreaks(classBreaks, newValue, classLevel, limitType);
                }
            }
            else
            {
                if (classBreaks[classLevel].Start >= newValue)
                {
                    limitType = ClassLimitType.Start;
                    AdjustClassBreaks(classBreaks, (float)(newValue - 0.01), classLevel, limitType);
                }

                classLevel += 1;
                if (classLevel == classBreaks.Count) return;

                if (classBreaks[classLevel].Start != newValue)
                {
                    limitType = ClassLimitType.Start;
                    AdjustClassBreaks(classBreaks, newValue, classLevel, limitType);
                }
            }
        }

        private void ReplaceClassLimit(List<ClassLimits> classBreaks, float newValue, int classLevel, ClassLimitType limitType = ClassLimitType.Start)
        {
            if (limitType == ClassLimitType.Start)
            {
                classBreaks[classLevel].Start = newValue;
            }
            else
            {
                classBreaks[classLevel].End = newValue;
            }
        }

        private bool IsMissingLimitValue()
        {
            foreach (UIElement element in stratGrid.Children)
            {
                if (element is System.Windows.Controls.TextBox)
                {
                    string elementName = ((System.Windows.Controls.TextBox)element).Name;
                    Visibility elementVisibility = ((System.Windows.Controls.TextBox)element).Visibility;
                    if (elementName.StartsWith("ramp") && elementVisibility == Visibility.Visible)
                    {
                        float rangeValue;
                        if(float.TryParse(((System.Windows.Controls.TextBox)element).Text, out rangeValue) == false)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}


