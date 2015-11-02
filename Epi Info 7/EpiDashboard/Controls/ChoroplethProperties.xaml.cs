using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Epi;
using EpiDashboard.Mapping;
using System.Windows.Forms;
using System.Net;
using Control = System.Windows.Controls.Control;

// ReSharper disable All

namespace EpiDashboard.Controls
{
    /// <summary>
    /// Interaction logic for DashboardProperties.xaml
    /// </summary>
    public partial class ChoroplethProperties : System.Windows.Controls.UserControl
    {
        private EpiDashboard.Mapping.StandaloneMapControl _mapControl;
        private ESRI.ArcGIS.Client.Map _myMap;
        private DashboardHelper _dashboardHelper;
        public EpiDashboard.Mapping.ChoroplethLayerProvider _provider;
        private int _currentStratCount;
        private SolidColorBrush _currentColor_rampMissing;
        private SolidColorBrush _currentColor_rampStart;
        private SolidColorBrush _currentColor_rampEnd;
        private bool _initialRampCalc;
        public ChoroplethLayerProperties layerprop;

        public RowFilterControl rowFilterControl { get; set; }
        public DataFilters datafilters { get; set; }

        private System.Xml.XmlElement currentElement;
        public EpiDashboard.Mapping.ChoroplethKmlLayerProvider choroKMLprovider;
        public EpiDashboard.Mapping.ChoroplethServerLayerProvider choroMapprovider;
        public EpiDashboard.Mapping.ChoroplethServerLayerProperties choroserverlayerprop;
        public EpiDashboard.Mapping.ChoroplethKmlLayerProperties chorokmllayerprop;
        private string shapeFilePath;

        public ListLegendTextDictionary ListLegendText = new ListLegendTextDictionary();

        public IDictionary<string, object> shapeAttributes;
        private Dictionary<int, object> ClassAttribList = new Dictionary<int, object>();

        string _shapeKey;
        string _dataKey;
        string _value;

        private struct classAttributes
        {
            public Brush rctColor;
            public string quintile;
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

            #region Translation

            //Point of Interest Left Panel
            lblConfigExpandedTitle.Content = DashboardSharedStrings.GADGET_CONFIG_TITLE_CHOROPLETH;
            tbtnDataSource.Title = DashboardSharedStrings.GADGET_DATA_SOURCE;
            tbtnDataSource.Description = DashboardSharedStrings.GADGET_TABDESC_DATASOURCE;
            tbtnHTML.Title = DashboardSharedStrings.GADGET_TABBUTTON_VARIABLES;
            tbtnHTML.Description = DashboardSharedStrings.GADGET_TABDESC_MAP_VARIABLES;
            tbtnCharts.Title = DashboardSharedStrings.GADGET_TAB_COLORS_STYLES;
            tbtnCharts.Description = DashboardSharedStrings.GADGET_TABDESC_SETCOLORS;
            tbtnFilters.Title = DashboardSharedStrings.GADGET_TABBUTTON_FILTERS;
            tbtnFilters.Description = DashboardSharedStrings.GADGET_TABDESC_FILTERS_MAPS;

            //Data Source Panel
            tblockPanelDataSource.Content = DashboardSharedStrings.GADGET_DATA_SOURCE;
            tblockDataSource.Content = DashboardSharedStrings.GADGET_DATA_SOURCE;
            btnBrowse.Content = DashboardSharedStrings.BUTTON_BROWSE;
            lblBoundaries.Content = DashboardSharedStrings.GADGET_BOUNDARIES;
            radShapeFile.Content = DashboardSharedStrings.GADGET_SHAPEFILE;
            btnBrowseShape.Content = DashboardSharedStrings.BUTTON_BROWSE;
            radMapServer.Content = DashboardSharedStrings.GADGET_MAPSERVER;
            radconnectmapserver.Content = DashboardSharedStrings.GADGET_CONNECT_MAPSERVER;
            radlocatemapserver.Content = DashboardSharedStrings.GADGET_OTHER_MAPSERVER;
            lblURL.Content = DashboardSharedStrings.GADGET_URL;
            btnMapserverlocate.Content = DashboardSharedStrings.BUTTON_CONNECT;
            lblExampleMapServerURL.Content = DashboardSharedStrings.GADGET_EXAMPLE_MAPSERVER;
            lblSelectFeature.Content = DashboardSharedStrings.GADGET_SELECT_FEATURE;
            radKML.Content = DashboardSharedStrings.GADGET_KMLFILE;
            lblURLOfKMLFile.Content = DashboardSharedStrings.GADGET_KMLFILE_LOCATION;
            btnKMLFile.Content = DashboardSharedStrings.BUTTON_BROWSE;
            lblExampleKMLFile.Content = DashboardSharedStrings.GADGET_EXAMPLE_KMLFILE;

            tblockConnectionString.Content = DashboardSharedStrings.GADGET_CONNECTION_STRING;
            tblockSQLQuery.Content = DashboardSharedStrings.GADGET_SQL_QUERY;

            //Variables Panel
            tblockPanelVariables.Content = DashboardSharedStrings.GADGET_TABBUTTON_VARIABLES;
            tblockSelectVarData.Content = DashboardSharedStrings.GADGET_VARIABLES_CHORPLTH;
            lblFeature.Content = DashboardSharedStrings.GADGET_MAP_FEATURE;
            lblData.Content = DashboardSharedStrings.GADGET_MAP_DATA;
            lblValue.Content = DashboardSharedStrings.GADGET_MAP_VALUE;

            //Colors & Ranges Panel
            lblPanelHdrColorsAndStyles.Content = DashboardSharedStrings.GADGET_TAB_COLORS_RANGES;
            tblockColorsSubheader.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_COLORS;
            lblMissingExcluded.Content = DashboardSharedStrings.GADGET_MAP_COLOR_MISSING;
            lblColorRamp.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_COLORS;
            lblColorStart.Content = DashboardSharedStrings.GADGET_MAP_START;
            lblColorEnd.Content = DashboardSharedStrings.GADGET_MAP_END;
            tblockOpacity.Text = DashboardSharedStrings.GADGET_MAP_OPACITY;
            btnResetLegend.Content = DashboardSharedStrings.BUTTON_RESET_LEGEND;
            tblockRangesSubheader.Content = DashboardSharedStrings.GADGET_MAP_RANGES;
            lblClassBreaks.Content = DashboardSharedStrings.GADGET_MAP_CLASS_BREAKS;
            quintilesOption.Content = DashboardSharedStrings.GADGET_MAP_QUANTILES;
            lblLegTitle.Text = DashboardSharedStrings.GADGET_MAP_LEGEND_TITLE;
            tblockColorRamp.Text = DashboardSharedStrings.GADGET_PANELSUBHEADER_COLOR;
            tblockRange.Text = DashboardSharedStrings.GADGET_MAP_RANGE;
            tblockLegText.Text = DashboardSharedStrings.GADGET_MAP_LEGEND_TEXT;
            legendText0.Text = DashboardSharedStrings.GADGET_MAP_CHORPLTH_MISSEXCLUDED;

            //Filters Panel
            tblockPanelDataFilter.Content = DashboardSharedStrings.GADGET_PANELHEADER_DATA_FILTER;
            tblockSetDataFilter.Content = DashboardSharedStrings.GADGET_TABDESC_FILTERS_MAPS;
            tblockAnyFilterGadgetOnly.Content = DashboardSharedStrings.GADGET_MAP_SELECT_DATASOURCE;

            //Info Panel
            //lblCanvasInfo.Content = DashboardSharedStrings.GADGET_CANVAS_INFO;
            //tblockRows.Text = dashboardHelper.DataSet.Tables[0].Rows.Count.ToString() + DashboardSharedStrings.GADGET_INFO_UNFILTERED_ROWS;


            btnOK.Content = DashboardSharedStrings.BUTTON_OK;
            btnCancel.Content = DashboardSharedStrings.BUTTON_CANCEL;

            tbtnFilters.Title = DashboardSharedStrings.GADGET_TABBUTTON_FILTERS;
            tbtnFilters.Description = DashboardSharedStrings.GADGET_TABDESC_FILTERS;
            tblockColorsSubheader.Content = DashboardSharedStrings.GADGET_PANELSUBHEADER_COLORS;

            #endregion // Translation
        }

        public void mapControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _mapControl.ResizedWidth = e.NewSize.Width;
            _mapControl.ResizedHeight = e.NewSize.Height;
            if (_mapControl.ResizedWidth != 0 & _mapControl.ResizedHeight != 0)
            {
                double i_StandardHeight = System.Windows.SystemParameters.PrimaryScreenHeight;//Developer Desktop Width Where the Form is Designed
                double i_StandardWidth = System.Windows.SystemParameters.PrimaryScreenWidth; ////Developer Desktop Height Where the Form is Designed
                float f_HeightRatio = new float();
                float f_WidthRatio = new float();
                f_HeightRatio = (float)((float)_mapControl.ResizedHeight / (float)i_StandardHeight);
                f_WidthRatio = (float)((float)_mapControl.ResizedWidth / (float)i_StandardWidth);

                this.Height = (Convert.ToInt32(i_StandardHeight * f_HeightRatio)) / 1.07;
                this.Width = (Convert.ToInt32(i_StandardWidth * f_WidthRatio)) / 1.07;

            }
            else
            {
                this.Width = (System.Windows.SystemParameters.PrimaryScreenWidth / 1.07);
                this.Height = (System.Windows.SystemParameters.PrimaryScreenHeight / 1.15);
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
            panelHTML.Visibility = System.Windows.Visibility.Collapsed;
            panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Visible;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void tbtnCharts_Checked(object sender, RoutedEventArgs e)
        {
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelHTML.Visibility = System.Windows.Visibility.Collapsed;
            panelCharts.Visibility = System.Windows.Visibility.Visible;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Collapsed;
            SetDefaultRanges();
        }

        public void SetDefaultRanges()
        {
            if (cmbShapeKey.SelectedItem != null &&
                cmbDataKey.SelectedItem != null &&
                cmbValue.SelectedItem != null)
            {

                if (_provider != null && _provider.Opacity > 0)
                {
                    Opacity = _provider.Opacity;
                    sliderOpacity.Value = Opacity;
                }
                else if (choroKMLprovider != null && choroKMLprovider.Opacity > 0)
                {
                    Opacity = choroKMLprovider.Opacity;
                    sliderOpacity.Value = Opacity;
                }
                else if (choroserverlayerprop != null && choroserverlayerprop.Opacity > 0)
                {

                    Opacity = choroserverlayerprop.Opacity;
                    // Opacity = choroMapprovider.Opacity;
                    sliderOpacity.Value = Opacity;
                }
                else
                {
                    Opacity = Convert.ToByte(sliderOpacity.Value);
                }


                SetOpacity();

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

                UpdateColorsCollection();

                int classCount;
                if (!int.TryParse(((ComboBoxItem)cmbClasses.SelectedItem).Content.ToString(), out classCount))
                {
                    classCount = 4;
                }

                if (_provider != null)
                {
                    _provider.PopulateRangeValues(_dashboardHelper,
                       cmbShapeKey.SelectedItem.ToString(),
                       cmbDataKey.SelectedItem.ToString(),
                       cmbValue.SelectedItem.ToString(),
                       brushList,
                       classCount,
                        legTitle.Text);

                }
                else
                    if (choroKMLprovider != null)
                    {
                        choroKMLprovider.PopulateRangeValues(_dashboardHelper,
                           cmbShapeKey.SelectedItem.ToString(),
                           cmbDataKey.SelectedItem.ToString(),
                           cmbValue.SelectedItem.ToString(),
                           brushList,
                           classCount);
                    }
                SetRangeUISection();
            }
        }

        private void UpdateColorsCollection()
        {
            foreach (UIElement element in stratGrid.Children)
            {
                if (element is Rectangle)
                {
                    SolidColorBrush scBrush = ((Rectangle)element).Fill as SolidColorBrush;
                    Color color = scBrush.Color;
                    _provider.CustomColorsDictionary.Add(((Rectangle)element).Name, color);
                }
            }
        }

        private void UpdateRangesCollection()
        {
            foreach (UIElement element in stratGrid.Children)
            {
                if (element is System.Windows.Controls.TextBox)
                {

                    string elementName = ((System.Windows.Controls.TextBox)element).Name;

                    if (elementName.StartsWith("ramp"))
                    {

                        _provider.ClassRangesDictionary.Add(((System.Windows.Controls.TextBox)element).Name,
                            ((System.Windows.Controls.TextBox)element).Text);
                    }
                }
            }
        }


        private void tbtnHTML_Checked(object sender, RoutedEventArgs e)
        {
           
           if (!string.IsNullOrEmpty(txtProjectPath.Text) )
           {
                if( !string.IsNullOrEmpty(txtShapePath.Text) 
                    ||  cbxmapserver.SelectedIndex != -1   
                    ||    (!string.IsNullOrEmpty(txtMapSeverpath.Text) 
                    || (!string.IsNullOrEmpty(txtKMLpath.Text)  )))
                {
                   // btnOK.Visibility = Visibility.Hidden;
                    CheckButtonStates(sender as ToggleButton);
                    panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
                    panelHTML.Visibility = System.Windows.Visibility.Visible;
                    panelCharts.Visibility = System.Windows.Visibility.Collapsed;
                    panelInfo.Visibility = System.Windows.Visibility.Collapsed;
                    panelFilters.Visibility = System.Windows.Visibility.Collapsed;
                    if (choroserverlayerprop != null)
                        if (choroserverlayerprop.provider.FlagUpdateToGLFailed) { ResetShapeCombo(); }
                }
                else
                {
                    tbtnHTML.IsChecked = false;
                    MessageBoxResult result = System.Windows.MessageBox.Show(DashboardSharedStrings.GADGET_MAP_ADD_BOUNDARY, DashboardSharedStrings.ALERT, MessageBoxButton.OK);
              
                }
           }
           else
           {
               tbtnHTML.IsChecked = false;
               MessageBoxResult result = System.Windows.MessageBox.Show(DashboardSharedStrings.GADGET_MAP_ADD_DATA_SOURCE, DashboardSharedStrings.ALERT, MessageBoxButton.OK);
           
           }
        }
        private void tbtnFilters_Checked(object sender, RoutedEventArgs e)
        {
           // btnOK.Visibility = Visibility.Hidden;
            if (panelDataSource == null) return;
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Collapsed;
            panelHTML.Visibility = System.Windows.Visibility.Collapsed;
            panelCharts.Visibility = System.Windows.Visibility.Collapsed;
            panelInfo.Visibility = System.Windows.Visibility.Collapsed;
            panelFilters.Visibility = System.Windows.Visibility.Visible;
        }

        private void tbtnDataSource_Checked(object sender, RoutedEventArgs e)
        {
            if (btnOK != null)
            {
                btnOK.Visibility = Visibility.Visible;
            }
            if (panelDataSource == null) return;
            CheckButtonStates(sender as ToggleButton);
            panelDataSource.Visibility = System.Windows.Visibility.Visible;
            panelHTML.Visibility = System.Windows.Visibility.Collapsed;
            panelCharts.Visibility = System.Windows.Visibility.Collapsed;
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
            EnableOkbtn();
        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (cmbDataKey.SelectedIndex > -1 && cmbShapeKey.SelectedIndex > -1 && cmbValue.SelectedIndex > -1)
            {
                Addfilters();

                if ((_provider != null && !_provider.AreRangesSet) ||
                    (choroMapprovider != null && !choroMapprovider.AreRangesSet) ||
                    (choroKMLprovider != null && !choroKMLprovider.AreRangesSet)
                    )
                {
                    SetDefaultRanges();
                }

                //  ResetLegend_Click(this, new RoutedEventArgs());

                RenderMap();


                if (_provider != null && !ValidateRangeInput())
                    return;

                AddClassAttributes();

                int numclasses = Convert.ToInt32(cmbClasses.Text);
                bool flagquintiles = (bool)quintilesOption.IsChecked;

                if (radShapeFile.IsChecked == true && _provider != null)
                {


                    layerprop.SetValues(txtShapePath.Text, cmbShapeKey.Text, cmbDataKey.Text, cmbValue.Text,
                        cmbClasses.Text, rctHighColor.Fill, rctLowColor.Fill, rctMissingColor.Fill, shapeAttributes,
                        ClassAttribList,
                        flagquintiles, numclasses, legTitle.Text);


                }
                else if (radMapServer.IsChecked == true && choroMapprovider != null)
                {
                    choroserverlayerprop.SetValues(cmbShapeKey.Text, cmbDataKey.Text, cmbValue.Text, cmbClasses.Text, rctHighColor.Fill, rctLowColor.Fill, rctMissingColor.Fill, shapeAttributes, ClassAttribList, flagquintiles, numclasses, Opacity);
                    choroserverlayerprop.cbxMapserverText = cbxmapserver.Text;
                    choroserverlayerprop.txtMapserverText = txtMapSeverpath.Text;
                    choroserverlayerprop.cbxMapFeatureText = cbxmapfeature.Text;
                }

                else if (radKML.IsChecked == true && choroKMLprovider != null)
                { chorokmllayerprop.SetValues(cmbShapeKey.Text, cmbDataKey.Text, cmbValue.Text, cmbClasses.Text, rctHighColor.Fill, rctLowColor.Fill, rctMissingColor.Fill, shapeAttributes, ClassAttribList, flagquintiles, numclasses); }

                if (ChangesAccepted != null)
                {
                    ChangesAccepted(this, new EventArgs());
                }
            }
            else
            {
               // tbtnDataSource.IsChecked = true;
                MessageBoxResult result = System.Windows.MessageBox.Show(DashboardSharedStrings.GADGET_MAP_ADD_VARIABLES, DashboardSharedStrings.ALERT, MessageBoxButton.OK);
            }


        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            /* if (radShapeFile.IsChecked == true && _provider != null && layerprop.classAttribList == null) 
                 { layerprop.CloseLayer(); }
             else if (radMapServer.IsChecked == true && choroMapprovider != null && choroserverlayerprop.classAttribList == null)
                 { choroserverlayerprop.CloseLayer(); }
             else if (radKML.IsChecked == true && choroKMLprovider != null && chorokmllayerprop.classAttribList == null)
                 { chorokmllayerprop.CloseLayer(); } */

            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            //waitPanel.Visibility = Visibility.Visible;
           // waitPanel.statusText.Text = "Loading Data Set";

            waitCursor.Visibility = Visibility.Visible;
            _dashboardHelper = _mapControl.GetNewDashboardHelper();
            waitCursor.Visibility = Visibility.Collapsed;
           // waitPanel.Visibility = Visibility.Collapsed;
            
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
                panelFilters.Children.Add(rowFilterControl);
                //txtNote.Text = "Note: Any filters set here are applied to this gadget only.";
            }

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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }

        private void btnBrowseShapeFile_Click(object sender, RoutedEventArgs e)
        {
            _provider = new Mapping.ChoroplethLayerProvider(_myMap);
            object[] shapeFileProperties = _provider.LoadShapeFile();
            if (shapeFileProperties != null)
            {
                if (layerprop == null)
                {
                    ILayerProperties layerProperties = null;
                    layerProperties = new ChoroplethLayerProperties(_myMap, this.DashboardHelper, this._mapControl);
                    layerProperties.MapGenerated += new EventHandler(this._mapControl.ILayerProperties_MapGenerated);
                    layerProperties.FilterRequested += new EventHandler(this._mapControl.ILayerProperties_FilterRequested);
                    layerProperties.EditRequested += new EventHandler(this._mapControl.ILayerProperties_EditRequested);
                    this.layerprop = (ChoroplethLayerProperties)layerProperties;
                    this._mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
                }

                layerprop.provider = _provider;
                if (this.DashboardHelper != null)
                    layerprop.SetdashboardHelper(DashboardHelper);

                if (shapeFileProperties.Length == 2)
                {
                    txtShapePath.Text = shapeFileProperties[0].ToString();
                    layerprop.shapeFilePath = shapeFileProperties[0].ToString();
                    layerprop.shapeAttributes = (IDictionary<string, object>)shapeFileProperties[1];
                    shapeAttributes = (IDictionary<string, object>)shapeFileProperties[1];
                    if (shapeAttributes != null)
                    {
                        cmbShapeKey.Items.Clear();
                        layerprop.cbxShapeKey.Items.Clear();
                        foreach (string key in shapeAttributes.Keys)
                        {
                            cmbShapeKey.Items.Add(key);
                            layerprop.cbxShapeKey.Items.Add(key);
                        }
                    }
                }
            }
        }

        private void AddClassAttributes()
        {

            ClassAttribList.Clear();

            classAttributes ca1 = new classAttributes();
            ca1.rctColor = rctColor1.Fill;
            ca1.rampStart = rampStart01.Text;
            ca1.rampEnd = rampEnd01.Text;
            ca1.quintile = quintile01.Text;
            ca1.legendText = legendText1.Text;
            ClassAttribList.Add(1, ca1);

            classAttributes ca2 = new classAttributes();
            ca2.rctColor = rctColor2.Fill;
            ca2.rampStart = rampStart02.Text;
            ca2.rampEnd = rampEnd02.Text;
            ca2.quintile = quintile02.Text;
            ca2.legendText = legendText2.Text;
            ClassAttribList.Add(2, ca2);

            classAttributes ca3 = new classAttributes();
            ca3.rctColor = rctColor3.Fill;
            ca3.rampStart = rampStart03.Text;
            ca3.rampEnd = rampEnd03.Text;
            ca3.quintile = quintile03.Text;
            ca3.legendText = legendText3.Text;
            ClassAttribList.Add(3, ca3);

            classAttributes ca4 = new classAttributes();
            ca4.rctColor = rctColor4.Fill;
            ca4.rampStart = rampStart04.Text;
            ca4.rampEnd = rampEnd04.Text;
            ca4.quintile = quintile04.Text;
            ca4.legendText = legendText4.Text;
            ClassAttribList.Add(4, ca4);

            classAttributes ca5 = new classAttributes();
            ca5.rctColor = rctColor5.Fill;
            ca5.rampStart = rampStart05.Text;
            ca5.rampEnd = rampEnd05.Text;
            ca5.quintile = quintile05.Text;
            ca5.legendText = legendText5.Text;
            ClassAttribList.Add(5, ca5);

            classAttributes ca6 = new classAttributes();
            ca6.rctColor = rctColor6.Fill;
            ca6.rampStart = rampStart06.Text;
            ca6.rampEnd = rampEnd06.Text;
            ca6.quintile = quintile06.Text;
            ca6.legendText = legendText6.Text;
            ClassAttribList.Add(6, ca6);

            classAttributes ca7 = new classAttributes();
            ca7.rctColor = rctColor7.Fill;
            ca7.rampStart = rampStart07.Text;
            ca7.rampEnd = rampEnd07.Text;
            ca7.quintile = quintile07.Text;
            ca7.legendText = legendText7.Text;
            ClassAttribList.Add(7, ca7);

            classAttributes ca8 = new classAttributes();
            ca8.rctColor = rctColor8.Fill;
            ca8.rampStart = rampStart08.Text;
            ca8.rampEnd = rampEnd08.Text;
            ca8.quintile = quintile08.Text;
            ca8.legendText = legendText8.Text;
            ClassAttribList.Add(8, ca8);

            classAttributes ca9 = new classAttributes();
            ca9.rctColor = rctColor9.Fill;
            ca9.rampStart = rampStart09.Text;
            ca9.rampEnd = rampEnd09.Text;
            ca9.quintile = quintile09.Text;
            ca9.legendText = legendText9.Text;
            ClassAttribList.Add(9, ca9);

            classAttributes ca10 = new classAttributes();
            ca10.rctColor = rctColor10.Fill;
            ca10.rampStart = rampStart10.Text;
            ca10.rampEnd = rampEnd10.Text;
            ca10.quintile = quintile10.Text;
            ca10.legendText = legendText10.Text;
            ClassAttribList.Add(10, ca10);

            UpdateRangesCollection();

        }

        private bool ValidateInputValue(System.Windows.Controls.TextBox textBox)
        {
            double Value;
            if (double.TryParse(textBox.Text, out Value))
            {
                if (Value >= double.Parse(_provider.RangeValues[0, 0].ToString()) && Value <= double.Parse(_provider.RangeValues[_provider.RangeCount - 1, 1].ToString()))
                {
                    return true;
                }
            }
            return false;
        }

        private bool ValidateRangeInput()
        {
            int _rangeCount = _provider.RangeCount;

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

                if (key == 1)
                {
                    rctColor1.Fill = (Brush)itemvalue.rctColor;
                    rampStart01.Text = itemvalue.rampStart;
                    rampEnd01.Text = itemvalue.rampEnd;
                    quintile01.Text = itemvalue.quintile;
                    // legendText1.Text = itemvalue.legendText;
                }
                else if (key == 2)
                {
                    rctColor2.Fill = (Brush)itemvalue.rctColor;
                    rampStart02.Text = itemvalue.rampStart;
                    rampEnd02.Text = itemvalue.rampEnd;
                    quintile02.Text = itemvalue.quintile;
                    // legendText2.Text = itemvalue.legendText;
                }
                else if (key == 3)
                {
                    rctColor3.Fill = (Brush)itemvalue.rctColor;
                    rampStart03.Text = itemvalue.rampStart;
                    rampEnd03.Text = itemvalue.rampEnd;
                    quintile03.Text = itemvalue.quintile;
                    //  legendText3.Text = itemvalue.legendText;
                }
                else if (key == 4)
                {
                    rctColor4.Fill = (Brush)itemvalue.rctColor;
                    rampStart04.Text = itemvalue.rampStart;
                    rampEnd04.Text = itemvalue.rampEnd;
                    quintile04.Text = itemvalue.quintile;
                    // legendText4.Text = itemvalue.legendText;
                }
                else if (key == 5)
                {
                    rctColor5.Fill = (Brush)itemvalue.rctColor;
                    rampStart05.Text = itemvalue.rampStart;
                    rampEnd05.Text = itemvalue.rampEnd;
                    quintile05.Text = itemvalue.quintile;
                    // legendText5.Text = itemvalue.legendText;

                }
                else if (key == 6)
                {
                    rctColor6.Fill = (Brush)itemvalue.rctColor;
                    rampStart06.Text = itemvalue.rampStart;
                    rampEnd06.Text = itemvalue.rampEnd;
                    quintile06.Text = itemvalue.quintile;
                    // legendText6.Text = itemvalue.legendText;

                }
                else if (key == 7)
                {
                    rctColor7.Fill = (Brush)itemvalue.rctColor;
                    rampStart07.Text = itemvalue.rampStart;
                    rampEnd07.Text = itemvalue.rampEnd;
                    quintile07.Text = itemvalue.quintile;
                    // legendText7.Text = itemvalue.legendText;
                }
                else if (key == 8)
                {
                    rctColor8.Fill = (Brush)itemvalue.rctColor;
                    rampStart08.Text = itemvalue.rampStart;
                    rampEnd08.Text = itemvalue.rampEnd;
                    quintile08.Text = itemvalue.quintile;
                    // legendText8.Text = itemvalue.legendText;

                }
                else if (key == 9)
                {
                    rctColor9.Fill = (Brush)itemvalue.rctColor;
                    rampStart09.Text = itemvalue.rampStart;
                    rampEnd09.Text = itemvalue.rampEnd;
                    quintile09.Text = itemvalue.quintile;
                    // legendText9.Text = itemvalue.legendText;

                }
                else if (key == 10)
                {
                    rctColor10.Fill = (Brush)itemvalue.rctColor;
                    rampStart10.Text = itemvalue.rampStart;
                    rampEnd10.Text = itemvalue.rampEnd;
                    quintile10.Text = itemvalue.quintile;
                    //  legendText10.Text = itemvalue.legendText;
                }
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
                SetOpacity();

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

                UpdateColorsCollection();


                int classCount;
                if (!int.TryParse(cmbClasses.Text, out classCount))
                {
                    classCount = 4;
                }



                if (radShapeFile.IsChecked == true && _provider != null)
                {
                    _provider.Range = GetRangeValues(_provider.RangeCount);
                    _provider.ListLegendText = ListLegendText;
                    _provider.Opacity = this.Opacity;
                    _provider.SetShapeRangeValues(_dashboardHelper,
                        cmbShapeKey.SelectedItem.ToString(),
                        cmbDataKey.SelectedItem.ToString(),
                        cmbValue.SelectedItem.ToString(),
                        brushList,
                        classCount,
                        missingText);
                }
                else if (radMapServer.IsChecked == true && choroMapprovider != null)
                {
                    choroMapprovider.Range = GetRangeValues(choroMapprovider.RangeCount);
                    choroMapprovider.ListLegendText = ListLegendText;
                    choroMapprovider.Opacity = this.Opacity;
                    choroMapprovider.SetShapeRangeValues(_dashboardHelper,
                    cmbShapeKey.SelectedItem.ToString(),
                    cmbDataKey.SelectedItem.ToString(),
                    cmbValue.SelectedItem.ToString(),
                    brushList,
                    classCount,
                    missingText);
                }
                else if (radKML.IsChecked == true && choroKMLprovider != null)
                {
                    choroKMLprovider.Range = GetRangeValues(choroKMLprovider.RangeCount);
                    // choroKMLprovider.ListLegendText = ListLegendText;
                    choroKMLprovider.Opacity = this.Opacity;
                    choroKMLprovider.SetShapeRangeValues(_dashboardHelper,
                    cmbShapeKey.SelectedItem.ToString(),
                    cmbDataKey.SelectedItem.ToString(),
                    cmbValue.SelectedItem.ToString(),
                    brushList,
                    classCount,
                    missingText);
                }
                /* _provider.SetShapeRangeValues(_dashboardHelper, 
                     cmbShapeKey.SelectedItem.ToString(),
                     cmbDataKey.SelectedItem.ToString(),
                     cmbValue.SelectedItem.ToString(),
                     brushList,
                     classCount); */
            }
        }
        private void SetOpacity()
        {
            Color col = (rctColor0.Fill as SolidColorBrush).Color;
            col.A = Opacity;
            rctColor0.Fill = new SolidColorBrush(col);

            col = (rctColor1.Fill as SolidColorBrush).Color;
            col.A = Opacity;
            rctColor1.Fill = new SolidColorBrush(col);

            col = (rctColor2.Fill as SolidColorBrush).Color;
            col.A = Opacity;
            rctColor2.Fill = new SolidColorBrush(col);

            col = (rctColor3.Fill as SolidColorBrush).Color;
            col.A = Opacity;
            rctColor3.Fill = new SolidColorBrush(col);

            col = (rctColor4.Fill as SolidColorBrush).Color;
            col.A = Opacity;
            rctColor4.Fill = new SolidColorBrush(col);

            col = (rctColor5.Fill as SolidColorBrush).Color;
            col.A = Opacity;
            rctColor5.Fill = new SolidColorBrush(col);

            col = (rctColor6.Fill as SolidColorBrush).Color;
            col.A = Opacity;
            rctColor6.Fill = new SolidColorBrush(col);

            col = (rctColor7.Fill as SolidColorBrush).Color;
            col.A = Opacity;
            rctColor7.Fill = new SolidColorBrush(col);

            col = (rctColor8.Fill as SolidColorBrush).Color;
            col.A = Opacity;
            rctColor8.Fill = new SolidColorBrush(col);

            col = (rctColor9.Fill as SolidColorBrush).Color;
            col.A = Opacity;
            rctColor9.Fill = new SolidColorBrush(col);

            col = (rctColor10.Fill as SolidColorBrush).Color;
            col.A = Opacity;
            rctColor10.Fill = new SolidColorBrush(col);

            col = (rctMissingColor.Fill as SolidColorBrush).Color;
            col.A = Opacity;
            rctMissingColor.Fill = new SolidColorBrush(col);
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

            //int _rangeCount = _provider.RangeCount;
            double value = 0.0;

            if (RangeCount >= 2)
            {
                double.TryParse(rampStart01.Text, out value);
                Range.Add(value);

                //   ListLegendText.Add(legendText1.Name, legendText1.Text);

                double.TryParse(rampStart02.Text, out value);
                Range.Add(value);

                //   ListLegendText.Add(legendText2.Name, legendText2.Text);
            }


            if (RangeCount >= 3)
            {
                double.TryParse(rampStart03.Text, out value);
                Range.Add(value);
                // ListLegendText.Add(legendText3.Name, legendText3.Text);
            }
            if (RangeCount >= 4)
            {
                double.TryParse(rampStart04.Text, out value);
                Range.Add(value);
                // ListLegendText.Add(legendText4.Name, legendText4.Text);
            }
            if (RangeCount >= 5)
            {
                double.TryParse(rampStart05.Text, out value);
                Range.Add(value);
                // ListLegendText.Add(legendText5.Name, legendText5.Text);
            }
            if (RangeCount >= 6)
            {
                double.TryParse(rampStart06.Text, out value);
                Range.Add(value);
                // ListLegendText.Add(legendText6.Name, legendText6.Text);
            }
            if (RangeCount >= 7)
            {
                double.TryParse(rampStart07.Text, out value);
                Range.Add(value);
                // ListLegendText.Add(legendText7.Name, legendText7.Text);
            }
            if (RangeCount >= 8)
            {
                double.TryParse(rampStart08.Text, out value);
                Range.Add(value);
                // ListLegendText.Add(legendText8.Name, legendText8.Text);
            }
            if (RangeCount >= 9)
            {
                double.TryParse(rampStart09.Text, out value);
                Range.Add(value);
                // ListLegendText.Add(legendText9.Name, legendText9.Text);
            }
            if (RangeCount >= 10)
            {
                double.TryParse(rampStart10.Text, out value);
                Range.Add(value);
                // ListLegendText.Add(legendText10.Name, legendText10.Text);
            }

            return Range;

        }
        public StackPanel LegendStackPanel
        {
            get
            {
                return _provider.LegendStackPanel;
            }
        }
        private void rctColor0_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor0.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                _provider.CustomColorsDictionary.Add(rctColor0.Name, Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }
        private void rctColor1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor1.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                _provider.CustomColorsDictionary.Add(rctColor1.Name, Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }


        private void rctColor2_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor2.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));

                _provider.CustomColorsDictionary.Add(rctColor2.Name,
                    Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));

            }
        }

        private void rctColor3_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor3.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                _provider.CustomColorsDictionary.Add(rctColor3.Name, Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor4_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor4.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                _provider.CustomColorsDictionary.Add(rctColor4.Name, Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor5_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor5.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                _provider.CustomColorsDictionary.Add(rctColor5.Name, Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor6_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor6.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                _provider.CustomColorsDictionary.Add(rctColor6.Name, Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor7_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor7.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                _provider.CustomColorsDictionary.Add(rctColor7.Name, Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor8_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor8.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                _provider.CustomColorsDictionary.Add(rctColor8.Name, Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor9_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor9.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                _provider.CustomColorsDictionary.Add(rctColor9.Name, Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctColor10_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctColor10.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                _provider.CustomColorsDictionary.Add(rctColor10.Name, Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctMissingColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctMissingColor.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));

                _provider.CustomColorsDictionary.Add(rctMissingColor.Name, Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));

            }
        }

        private void rctLowColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctLowColor.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                _provider.CustomColorsDictionary.Add(rctLowColor.Name, Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void rctHighColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog dialog = new System.Windows.Forms.ColorDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rctHighColor.Fill = new SolidColorBrush(Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                _provider.CustomColorsDictionary.Add(rctHighColor.Name, Color.FromArgb(Opacity, dialog.Color.R, dialog.Color.G, dialog.Color.B));
            }
        }

        private void ResetLegend_Click(object sender, RoutedEventArgs e)
        {
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
                (_provider != null))
            {
                _provider.RangesLoadedFromMapFile = false;

                _provider.ResetRangeValues(
                    cmbShapeKey.SelectedItem.ToString(),
                    cmbDataKey.SelectedItem.ToString(),
                    cmbValue.SelectedItem.ToString(),
                    classCount
                    );
            }
            else if ((cmbShapeKey.SelectedItem != null && cmbDataKey.SelectedItem != null &&
                      cmbValue.SelectedItem != null) && (choroMapprovider != null))
            {
                choroMapprovider.ResetRangeValues();
            }
            else if ((cmbShapeKey.SelectedItem != null && cmbDataKey.SelectedItem != null &&
                      cmbValue.SelectedItem != null) && (choroKMLprovider != null))
            {
                choroKMLprovider.ResetRangeValues();
            }

            SolidColorBrush rampStart = (SolidColorBrush)rctLowColor.Fill;
            SolidColorBrush rampEnd = (SolidColorBrush)rctHighColor.Fill;

            if (cmbShapeKey.SelectedItem != null &&
                cmbDataKey.SelectedItem != null &&
                cmbValue.SelectedItem != null)
            {
                _provider.UseCustomColors = false;
                SetRangeUISection();
            }

            if (string.IsNullOrEmpty(legTitle.Text))
            {
                if (_provider.LegendText != null)
                    legTitle.Text = _provider.LegendText;
                else if (_value != null)
                    legTitle.Text = _value;

            }


        }



        public void SetVisibility(int stratCount, SolidColorBrush rampStart, SolidColorBrush rampEnd)
        {
            bool isNewColorRamp = true;

            //Color col = (Color)ColorConverter.ConvertFromString("Yellow");

            //SolidColorBrush rampMissing = new SolidColorBrush(col);

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

            if (isNewColorRamp) rctColor0.Fill = rampMissing;
            if (isNewColorRamp) rctColor1.Fill = rampStart;

            if (radShapeFile.IsChecked == true && _provider != null)
            {
                rampStart01.Text = _provider.RangeValues[0, 0];
                rampEnd01.Text = _provider.RangeValues[0, 1];
                quintile01.Text = _provider.QuantileValues[0].ToString();
            }
            else if (radMapServer.IsChecked == true && choroMapprovider != null)
            {
                rampStart01.Text = choroMapprovider.RangeValues[0, 0];
                rampEnd01.Text = choroMapprovider.RangeValues[0, 1];
                quintile01.Text = choroMapprovider.QuantileValues[0].ToString();
            }
            else if (radKML.IsChecked == true && choroKMLprovider != null)
            {
                rampStart01.Text = choroKMLprovider.RangeValues[0, 0];
                rampEnd01.Text = choroKMLprovider.RangeValues[0, 1];
                quintile01.Text = choroKMLprovider.QuantileValues[0].ToString();
            }


            Color coo = Color.FromArgb(Opacity, (byte)(rampStart.Color.R - ri), (byte)(rampStart.Color.G - gi),
                    (byte)(rampStart.Color.B - bi));

            Color dictColor = _provider.CustomColorsDictionary.GetWithKey(rctColor2.Name);


            if (_provider.UseCustomColors)
                coo = _provider.CustomColorsDictionary.GetWithKey(rctColor2.Name);
            else
            {
                // coo  = GetSwatch(rctColor2, rampStart, ri, gi, bi);
                coo = Color.FromArgb(Opacity, (byte)(rampStart.Color.R - ri), (byte)(rampStart.Color.G - gi),
                    (byte)(rampStart.Color.B - bi));

                _provider.StoreColor(rctColor2, coo);

            }


            if (radShapeFile.IsChecked == true && _provider != null)
            {
                rampStart02.Text = _provider.RangeValues[1, 0];
                rampEnd02.Text = _provider.RangeValues[1, 1];
                quintile02.Text = _provider.QuantileValues[1].ToString();
            }
            else if (radMapServer.IsChecked == true && choroMapprovider != null)
            {
                rampStart02.Text = choroMapprovider.RangeValues[1, 0];
                rampEnd02.Text = choroMapprovider.RangeValues[1, 1];
                quintile02.Text = choroMapprovider.QuantileValues[1].ToString();
            }
            else if (radKML.IsChecked == true && choroKMLprovider != null)
            {
                rampStart02.Text = choroKMLprovider.RangeValues[1, 0];
                rampEnd02.Text = choroKMLprovider.RangeValues[1, 1];
                quintile02.Text = choroKMLprovider.QuantileValues[1].ToString();
            }

            int i = 3;


            if (_provider.UseCustomColors)
                coo = _provider.GetCustomColor(rctColor3.Name);
            else
            {
                coo = Color.FromArgb(Opacity, (byte)(rampStart.Color.R - ri * 2), (byte)(rampStart.Color.G - gi * 2),
                    (byte)(rampStart.Color.B - bi * 2));
                _provider.StoreColor(rctColor3, coo);
            }

            rctColor3.Visibility = System.Windows.Visibility.Visible;
            rampStart03.Visibility = System.Windows.Visibility.Visible;
            centerText03.Visibility = System.Windows.Visibility.Visible;
            rampEnd03.Visibility = System.Windows.Visibility.Visible;
            quintile03.Visibility = System.Windows.Visibility.Visible;
            legendText3.Visibility = System.Windows.Visibility.Visible;
            if (i++ > stratCount)
            {
                coo = Color.FromArgb(Opacity, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                rctColor3.Visibility = System.Windows.Visibility.Hidden;
                rctColor3.Visibility = System.Windows.Visibility.Hidden;
                rampStart03.Visibility = System.Windows.Visibility.Hidden;
                centerText03.Visibility = System.Windows.Visibility.Hidden;
                rampEnd03.Visibility = System.Windows.Visibility.Hidden;
                quintile03.Visibility = System.Windows.Visibility.Hidden;
                legendText3.Visibility = System.Windows.Visibility.Hidden;
            }
            if (isNewColorRamp) rctColor3.Fill = new SolidColorBrush(coo);

            if (radShapeFile.IsChecked == true && _provider != null)
            {
                rampStart03.Text = _provider.RangeValues[i - 2, 0];
                rampEnd03.Text = _provider.RangeValues[i - 2, 1];
                quintile03.Text = _provider.QuantileValues[i - 2].ToString();
            }
            else if (radMapServer.IsChecked == true && choroMapprovider != null)
            {
                rampStart03.Text = choroMapprovider.RangeValues[i - 2, 0];
                rampEnd03.Text = choroMapprovider.RangeValues[i - 2, 1];
                quintile03.Text = choroMapprovider.QuantileValues[i - 2].ToString();
            }
            else if (radKML.IsChecked == true && choroKMLprovider != null)
            {
                rampStart03.Text = choroKMLprovider.RangeValues[i - 2, 0];
                rampEnd03.Text = choroKMLprovider.RangeValues[i - 2, 1];
                quintile03.Text = choroKMLprovider.QuantileValues[i - 2].ToString();
            }

            if (_provider.UseCustomColors)
                coo = _provider.GetCustomColor(rctColor4.Name);
            else
            {
                coo = Color.FromArgb(Opacity, (byte)(rampStart.Color.R - ri * 3), (byte)(rampStart.Color.G - gi * 3),
                    (byte)(rampStart.Color.B - bi * 3));
                _provider.StoreColor(rctColor4, coo);
            }
            rctColor4.Visibility = System.Windows.Visibility.Visible;
            rampStart04.Visibility = System.Windows.Visibility.Visible;
            centerText04.Visibility = System.Windows.Visibility.Visible;
            rampEnd04.Visibility = System.Windows.Visibility.Visible;
            quintile04.Visibility = System.Windows.Visibility.Visible;
            legendText4.Visibility = System.Windows.Visibility.Visible;
            if (i++ > stratCount)
            {
                coo = Color.FromArgb(Opacity, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                rctColor4.Visibility = System.Windows.Visibility.Hidden;
                rampStart04.Visibility = System.Windows.Visibility.Hidden;
                centerText04.Visibility = System.Windows.Visibility.Hidden;
                rampEnd04.Visibility = System.Windows.Visibility.Hidden;
                quintile04.Visibility = System.Windows.Visibility.Hidden;
                legendText4.Visibility = System.Windows.Visibility.Hidden;
            }
            if (isNewColorRamp) rctColor4.Fill = new SolidColorBrush(coo);

            if (radShapeFile.IsChecked == true && _provider != null)
            {
                rampStart04.Text = _provider.RangeValues[i - 2, 0];
                rampEnd04.Text = _provider.RangeValues[i - 2, 1];
                quintile04.Text = _provider.QuantileValues[i - 2].ToString();
            }
            else if (radMapServer.IsChecked == true && choroMapprovider != null)
            {
                rampStart04.Text = choroMapprovider.RangeValues[i - 2, 0];
                rampEnd04.Text = choroMapprovider.RangeValues[i - 2, 1];
                quintile04.Text = choroMapprovider.QuantileValues[i - 2].ToString();
            }
            else if (radKML.IsChecked == true && choroKMLprovider != null)
            {
                rampStart04.Text = choroKMLprovider.RangeValues[i - 2, 0];
                rampEnd04.Text = choroKMLprovider.RangeValues[i - 2, 1];
                quintile04.Text = choroKMLprovider.QuantileValues[i - 2].ToString();
            }
            if (_provider.UseCustomColors)
                coo = _provider.GetCustomColor(rctColor5.Name);
            else
            {
                coo = Color.FromArgb(Opacity, (byte)(rampStart.Color.R - ri * 4), (byte)(rampStart.Color.G - gi * 4),
                    (byte)(rampStart.Color.B - bi * 4));
                _provider.StoreColor(rctColor5, coo);
            }

            rctColor5.Visibility = System.Windows.Visibility.Visible;
            rampStart05.Visibility = System.Windows.Visibility.Visible;
            centerText05.Visibility = System.Windows.Visibility.Visible;
            rampEnd05.Visibility = System.Windows.Visibility.Visible;
            quintile05.Visibility = System.Windows.Visibility.Visible;
            legendText5.Visibility = System.Windows.Visibility.Visible;
            if (i++ > stratCount)
            {
                coo = Color.FromArgb(Opacity, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                rctColor5.Visibility = System.Windows.Visibility.Hidden;
                rampStart05.Visibility = System.Windows.Visibility.Hidden;
                centerText05.Visibility = System.Windows.Visibility.Hidden;
                rampEnd05.Visibility = System.Windows.Visibility.Hidden;
                quintile05.Visibility = System.Windows.Visibility.Hidden;
                legendText5.Visibility = System.Windows.Visibility.Hidden;
            }
            if (isNewColorRamp) rctColor5.Fill = new SolidColorBrush(coo);

            if (radShapeFile.IsChecked == true && _provider != null)
            {
                rampStart05.Text = _provider.RangeValues[i - 2, 0];
                rampEnd05.Text = _provider.RangeValues[i - 2, 1];
                quintile05.Text = _provider.QuantileValues[i - 2].ToString();
            }
            else if (radMapServer.IsChecked == true && choroMapprovider != null)
            {
                rampStart05.Text = choroMapprovider.RangeValues[i - 2, 0];
                rampEnd05.Text = choroMapprovider.RangeValues[i - 2, 1];
                quintile05.Text = choroMapprovider.QuantileValues[i - 2].ToString();
            }
            else if (radKML.IsChecked == true && choroKMLprovider != null)
            {
                rampStart05.Text = choroKMLprovider.RangeValues[i - 2, 0];
                rampEnd05.Text = choroKMLprovider.RangeValues[i - 2, 1];
                quintile05.Text = choroKMLprovider.QuantileValues[i - 2].ToString();
            }

            if (_provider.UseCustomColors)
                coo = _provider.GetCustomColor(rctColor6.Name);
            else
            {
                coo = Color.FromArgb(Opacity, (byte)(rampStart.Color.R - ri * 5), (byte)(rampStart.Color.G - gi * 5),
                    (byte)(rampStart.Color.B - bi * 5));
                _provider.StoreColor(rctColor6, coo);
            }

            rctColor6.Visibility = System.Windows.Visibility.Visible;
            rampStart06.Visibility = System.Windows.Visibility.Visible;
            centerText06.Visibility = System.Windows.Visibility.Visible;
            rampEnd06.Visibility = System.Windows.Visibility.Visible;
            quintile06.Visibility = System.Windows.Visibility.Visible;
            legendText6.Visibility = System.Windows.Visibility.Visible;
            if (i++ > stratCount)
            {
                coo = Color.FromArgb(Opacity, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                rctColor6.Visibility = System.Windows.Visibility.Hidden;
                rampStart06.Visibility = System.Windows.Visibility.Hidden;
                centerText06.Visibility = System.Windows.Visibility.Hidden;
                rampEnd06.Visibility = System.Windows.Visibility.Hidden;
                quintile06.Visibility = System.Windows.Visibility.Hidden;
                legendText6.Visibility = System.Windows.Visibility.Hidden;
            }
            if (isNewColorRamp) rctColor6.Fill = new SolidColorBrush(coo);

            if (radShapeFile.IsChecked == true && _provider != null)
            {
                rampStart06.Text = _provider.RangeValues[i - 2, 0];
                rampEnd06.Text = _provider.RangeValues[i - 2, 1];
                quintile06.Text = _provider.QuantileValues[i - 2].ToString();
            }
            else if (radMapServer.IsChecked == true && choroMapprovider != null)
            {
                rampStart06.Text = choroMapprovider.RangeValues[i - 2, 0];
                rampEnd06.Text = choroMapprovider.RangeValues[i - 2, 1];
                quintile06.Text = choroMapprovider.QuantileValues[i - 2].ToString();
            }
            else if (radKML.IsChecked == true && choroKMLprovider != null)
            {
                rampStart06.Text = choroKMLprovider.RangeValues[i - 2, 0];
                rampEnd06.Text = choroKMLprovider.RangeValues[i - 2, 1];
                quintile06.Text = choroKMLprovider.QuantileValues[i - 2].ToString();
            }

            //  Color xx = Color.FromRgb(44, 177, 199);    


            if (_provider.UseCustomColors)
                coo = _provider.GetCustomColor(rctColor7.Name);
            else
            {
                coo = Color.FromArgb(Opacity, (byte)(rampStart.Color.R - ri * 6), (byte)(rampStart.Color.G - gi * 6),
                    (byte)(rampStart.Color.B - bi * 6));
                _provider.StoreColor(rctColor7, coo);
            }
            rctColor7.Visibility = System.Windows.Visibility.Visible;
            rampStart07.Visibility = System.Windows.Visibility.Visible;
            centerText07.Visibility = System.Windows.Visibility.Visible;
            rampEnd07.Visibility = System.Windows.Visibility.Visible;
            quintile07.Visibility = System.Windows.Visibility.Visible;
            legendText7.Visibility = System.Windows.Visibility.Visible;
            if (i++ > stratCount)
            {
                coo = Color.FromArgb(Opacity, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                rctColor7.Visibility = System.Windows.Visibility.Hidden;
                rampStart07.Visibility = System.Windows.Visibility.Hidden;
                centerText07.Visibility = System.Windows.Visibility.Hidden;
                rampEnd07.Visibility = System.Windows.Visibility.Hidden;
                quintile07.Visibility = System.Windows.Visibility.Hidden;
                legendText7.Visibility = System.Windows.Visibility.Hidden;
            }
            if (isNewColorRamp) rctColor7.Fill = new SolidColorBrush(coo);

            if (radShapeFile.IsChecked == true && _provider != null)
            {
                rampStart07.Text = _provider.RangeValues[i - 2, 0];
                rampEnd07.Text = _provider.RangeValues[i - 2, 1];
                quintile07.Text = _provider.QuantileValues[i - 2].ToString();
            }
            else if (radMapServer.IsChecked == true && choroMapprovider != null)
            {
                rampStart07.Text = choroMapprovider.RangeValues[i - 2, 0];
                rampEnd07.Text = choroMapprovider.RangeValues[i - 2, 1];
                quintile07.Text = choroMapprovider.QuantileValues[i - 2].ToString();
            }
            else if (radKML.IsChecked == true && choroKMLprovider != null)
            {
                rampStart07.Text = choroKMLprovider.RangeValues[i - 2, 0];
                rampEnd07.Text = choroKMLprovider.RangeValues[i - 2, 1];
                quintile07.Text = choroKMLprovider.QuantileValues[i - 2].ToString();
            }

            if (_provider.UseCustomColors)
                coo = _provider.GetCustomColor(rctColor8.Name);
            else
            {
                coo = Color.FromArgb(Opacity, (byte)(rampStart.Color.R - ri * 7), (byte)(rampStart.Color.G - gi * 7),
                    (byte)(rampStart.Color.B - bi * 7));
                _provider.StoreColor(rctColor8, coo);
            }

            rctColor8.Visibility = System.Windows.Visibility.Visible;
            rampStart08.Visibility = System.Windows.Visibility.Visible;
            centerText08.Visibility = System.Windows.Visibility.Visible;
            rampEnd08.Visibility = System.Windows.Visibility.Visible;
            quintile08.Visibility = System.Windows.Visibility.Visible;
            legendText8.Visibility = System.Windows.Visibility.Visible;
            if (i++ > stratCount)
            {
                coo = Color.FromArgb(Opacity, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                rctColor8.Visibility = System.Windows.Visibility.Hidden;
                rampStart08.Visibility = System.Windows.Visibility.Hidden;
                centerText08.Visibility = System.Windows.Visibility.Hidden;
                rampEnd08.Visibility = System.Windows.Visibility.Hidden;
                quintile08.Visibility = System.Windows.Visibility.Hidden;
                legendText8.Visibility = System.Windows.Visibility.Hidden;
            }
            if (isNewColorRamp) rctColor8.Fill = new SolidColorBrush(coo);

            if (radShapeFile.IsChecked == true && _provider != null)
            {
                rampStart08.Text = _provider.RangeValues[i - 2, 0];
                rampEnd08.Text = _provider.RangeValues[i - 2, 1];
                quintile08.Text = _provider.QuantileValues[i - 2].ToString();
            }
            else if (radMapServer.IsChecked == true && choroMapprovider != null)
            {
                rampStart08.Text = choroMapprovider.RangeValues[i - 2, 0];
                rampEnd08.Text = choroMapprovider.RangeValues[i - 2, 1];
                quintile08.Text = choroMapprovider.QuantileValues[i - 2].ToString();
            }
            else if (radKML.IsChecked == true && choroKMLprovider != null)
            {
                rampStart08.Text = choroKMLprovider.RangeValues[i - 2, 0];
                rampEnd08.Text = choroKMLprovider.RangeValues[i - 2, 1];
                quintile08.Text = choroKMLprovider.QuantileValues[i - 2].ToString();
            }

            if (_provider.UseCustomColors)
                coo = _provider.GetCustomColor(rctColor9.Name);
            else
            {
                coo = Color.FromArgb(Opacity, (byte)(rampStart.Color.R - ri * 8), (byte)(rampStart.Color.G - gi * 8),
                    (byte)(rampStart.Color.B - bi * 8));
                _provider.StoreColor(rctColor9, coo);
            }

            rctColor9.Visibility = System.Windows.Visibility.Visible;
            rampStart09.Visibility = System.Windows.Visibility.Visible;
            centerText09.Visibility = System.Windows.Visibility.Visible;
            rampEnd09.Visibility = System.Windows.Visibility.Visible;
            quintile09.Visibility = System.Windows.Visibility.Visible;
            legendText9.Visibility = System.Windows.Visibility.Visible;
            if (i++ > stratCount)
            {
                coo = Color.FromArgb(Opacity, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                rctColor9.Visibility = System.Windows.Visibility.Hidden;
                rampStart09.Visibility = System.Windows.Visibility.Hidden;
                centerText09.Visibility = System.Windows.Visibility.Hidden;
                rampEnd09.Visibility = System.Windows.Visibility.Hidden;
                quintile09.Visibility = System.Windows.Visibility.Hidden;
                legendText9.Visibility = System.Windows.Visibility.Hidden;
            }
            if (isNewColorRamp) rctColor9.Fill = new SolidColorBrush(coo);

            if (radShapeFile.IsChecked == true && _provider != null)
            {
                rampStart09.Text = _provider.RangeValues[i - 2, 0];
                rampEnd09.Text = _provider.RangeValues[i - 2, 1];
                quintile09.Text = _provider.QuantileValues[i - 2].ToString();
            }
            else if (radMapServer.IsChecked == true && choroMapprovider != null)
            {
                rampStart09.Text = choroMapprovider.RangeValues[i - 2, 0];
                rampEnd09.Text = choroMapprovider.RangeValues[i - 2, 1];
                quintile09.Text = choroMapprovider.QuantileValues[i - 2].ToString();
            }
            else if (radKML.IsChecked == true && choroKMLprovider != null)
            {
                rampStart09.Text = choroKMLprovider.RangeValues[i - 2, 0];
                rampEnd09.Text = choroKMLprovider.RangeValues[i - 2, 1];
                quintile09.Text = choroKMLprovider.QuantileValues[i - 2].ToString();
            }

            if (_provider.UseCustomColors)
                coo = _provider.GetCustomColor(rctColor10.Name);
            else
            {
                coo = Color.FromArgb(Opacity, (byte)(rampStart.Color.R - ri * 6), (byte)(rampStart.Color.G - gi * 9),
                    (byte)(rampStart.Color.B - bi * 9));
                _provider.StoreColor(rctColor10, coo);
            }

            rctColor10.Visibility = System.Windows.Visibility.Visible;
            rampStart10.Visibility = System.Windows.Visibility.Visible;
            centerText10.Visibility = System.Windows.Visibility.Visible;
            rampEnd10.Visibility = System.Windows.Visibility.Visible;
            quintile10.Visibility = System.Windows.Visibility.Visible;
            legendText10.Visibility = System.Windows.Visibility.Visible;
            if (i++ > stratCount)
            {
                coo = Color.FromArgb(Opacity, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                rctColor10.Visibility = System.Windows.Visibility.Hidden;
                rampStart10.Visibility = System.Windows.Visibility.Hidden;
                centerText10.Visibility = System.Windows.Visibility.Hidden;
                rampEnd10.Visibility = System.Windows.Visibility.Hidden;
                quintile10.Visibility = System.Windows.Visibility.Hidden;
                legendText10.Visibility = System.Windows.Visibility.Hidden;
            }
            if (isNewColorRamp) rctColor10.Fill = new SolidColorBrush(coo);

            if (radShapeFile.IsChecked == true && _provider != null)
            {
                rampStart10.Text = _provider.RangeValues[i - 2, 0];
                rampEnd10.Text = _provider.RangeValues[i - 2, 1];
                quintile10.Text = _provider.QuantileValues[i - 2].ToString();
            }
            else if (radMapServer.IsChecked == true && choroMapprovider != null)
            {
                rampStart10.Text = choroMapprovider.RangeValues[i - 2, 0];
                rampEnd10.Text = choroMapprovider.RangeValues[i - 2, 1];
                quintile10.Text = choroMapprovider.QuantileValues[i - 2].ToString();
            }
            else if (radKML.IsChecked == true && choroKMLprovider != null)
            {
                rampStart10.Text = choroKMLprovider.RangeValues[i - 2, 0];
                rampEnd10.Text = choroKMLprovider.RangeValues[i - 2, 1];
                quintile10.Text = choroKMLprovider.QuantileValues[i - 2].ToString();
            }





            _initialRampCalc = false;
        }

        //private Color GetSwatch(Rectangle rectangle, SolidColorBrush rampStart, byte ri, byte gi, byte bi)
        //{
        //    Color coo;

        //    if (_provider.UseCustomColors)
        //        coo = _provider.GetCustomColor(rectangle.Name);
        //    else
        //        coo = Color.FromArgb(Opacity, (byte)(rampStart.Color.R - ri), (byte)(rampStart.Color.G - gi),
        //            (byte)(rampStart.Color.B - bi));

        //    return coo;


        //}

        private void CheckBox_Quantiles_Click(object sender, RoutedEventArgs e)
        {
            OnQuintileOptionChanged();
        }

        public void OnQuintileOptionChanged()
        {
            int widthQuintile = 100;
            int widthMinMax = 0;
            int widthCompare = 0;

            if (quintilesOption.IsChecked == false)
            {
                widthQuintile = 0;
                widthMinMax = 75;
                widthCompare = 50;
            }

            quintileColumn.Width = new GridLength(widthQuintile, GridUnitType.Pixel);

            rampStartColumn.Width = new GridLength(widthMinMax, GridUnitType.Pixel);
            rampCompareColumn.Width = new GridLength(widthCompare, GridUnitType.Pixel);
            rampEndColumn.Width = new GridLength(widthMinMax, GridUnitType.Pixel);
        }
        public void cmbShapeKey_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbShapeKey.SelectedItem != null)
            { _shapeKey = cmbShapeKey.SelectedItem.ToString(); }
            else
            { _shapeKey = ""; }
        }

        private void cmbDataKey_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDataKey.SelectedItem != null)
            { _dataKey = cmbDataKey.SelectedItem.ToString(); }
        }

        private void cmbValue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbValue.SelectedItem != null)
            { _value = cmbValue.SelectedItem.ToString(); }
        }

        private void SetRangeUISection()
        {
            SetOpacity();
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

            UpdateColorsCollection();


            int classCount;
            if (!int.TryParse(((ComboBoxItem)cmbClasses.SelectedItem).Content.ToString(), out classCount))
            {
                classCount = 4;
            }

            if (radShapeFile.IsChecked == true && _provider != null)
            {


                _provider.PopulateRangeValues();

                //  Leg Title        
                if (string.IsNullOrEmpty(legTitle.Text))
                    legTitle.Text = _provider.LegendText;
                else
                    _provider.LegendText = legTitle.Text;


                try
                {
                    // Class titles                        
                    for (int x = 1; x <= ChoroplethConstants.MAX_CLASS_DECRIPTION_COUNT; x++)
                    {
                        System.Windows.Controls.TextBox t = FindName(ChoroplethConstants.legendTextControlPrefix + x) as System.Windows.Controls.TextBox;
                        if (string.IsNullOrEmpty(t.Text))
                            t.Text = _provider.ListLegendText.GetWithKey(ChoroplethConstants.legendTextControlPrefix + x);
                        else
                            _provider.ListLegendText.Add(ChoroplethConstants.legendTextControlPrefix + x, t.Text);
                    }


                }
                catch (Exception)
                {

                    throw;
                }
                // Custom colors    



                _provider.AreRangesSet = true;
            }
            else if (radMapServer.IsChecked == true && choroMapprovider != null)
            {
                choroMapprovider.Range = GetRangeValuesFromAttributeList(choroserverlayerprop.classAttribList, classCount);
                choroMapprovider.PopulateRangeValues(_dashboardHelper,
                         cmbShapeKey.SelectedItem.ToString(),
                         cmbDataKey.SelectedItem.ToString(),
                         cmbValue.SelectedItem.ToString(),
                         brushList,
                         classCount);
                choroMapprovider.AreRangesSet = true;
            }
            else if (radKML.IsChecked == true && choroKMLprovider != null)
            {
                choroKMLprovider.PopulateRangeValues();
                choroKMLprovider.AreRangesSet = true;
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
        private void btnKMLFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "KML Files (*.kml)|*.kml|KMZ Files (*.kmz)|*.kmz";
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtKMLpath.Text = dialog.FileName;
                KMLMapServerName = txtKMLpath.Text;
            }

            if (choroKMLprovider == null)
            {
                choroKMLprovider = new Mapping.ChoroplethKmlLayerProvider(_myMap);
                choroKMLprovider.FeatureLoaded += new FeatureLoadedHandler(choroKMLprovider_FeatureLoaded);
            }

            object[] kmlFileProperties = choroKMLprovider.LoadKml(KMLMapServerName);
            if (kmlFileProperties != null)
            {
                if (chorokmllayerprop == null)
                {
                    ILayerProperties layerProperties = null;
                    layerProperties = new ChoroplethKmlLayerProperties(_myMap, this.DashboardHelper, this._mapControl);
                    layerProperties.MapGenerated += new EventHandler(this._mapControl.ILayerProperties_MapGenerated);
                    layerProperties.FilterRequested += new EventHandler(this._mapControl.ILayerProperties_FilterRequested);
                    layerProperties.EditRequested += new EventHandler(this._mapControl.ILayerProperties_EditRequested);
                    this.chorokmllayerprop = (ChoroplethKmlLayerProperties)layerProperties;
                    this._mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
                }
                chorokmllayerprop.shapeFilePath = KMLMapServerName;
                chorokmllayerprop.provider = choroKMLprovider;
                chorokmllayerprop.provider.FeatureLoaded += new FeatureLoadedHandler(chorokmllayerprop.provider_FeatureLoaded);
                if (this.DashboardHelper != null)
                    chorokmllayerprop.SetdashboardHelper(DashboardHelper);

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
                    choroserverlayerprop.cbxShapeKey.Items.Clear();
                    foreach (string key in featureAttributes.Keys)
                    {
                        cmbShapeKey.Items.Add(key);
                        choroserverlayerprop.cbxShapeKey.Items.Add(key);
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
                //  RenderMap();    

                legTitle.Text = _provider.LegendText;

            }
        }

        private void Addfilters()
        {
            this.datafilters = rowFilterControl.DataFilters;
            if (radShapeFile.IsChecked == true && this.layerprop != null) { layerprop.datafilters = rowFilterControl.DataFilters; }
            if (radMapServer.IsChecked == true && this.choroserverlayerprop != null) { choroserverlayerprop.datafilters = rowFilterControl.DataFilters; }
            if (radKML.IsChecked == true && chorokmllayerprop != null) { chorokmllayerprop.datafilters = rowFilterControl.DataFilters; }


            this.datafilters = rowFilterControl.DataFilters;
            _dashboardHelper.SetDatafilters(this.datafilters);

            /*
            string sfilterOperand = string.Empty;
            string[] shilowvars;
            string svarname;

            List<string> sconditionval = datafilters.GetFilterConditionsAsList();
           
            string strreadablecondition = datafilters.GenerateReadableDataFilterString().Trim();
            if (!(string.IsNullOrEmpty(strreadablecondition)))
            {
                if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_EQUAL_TO))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_EQUAL_TO;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_GREATER_THAN))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_GREATER_THAN;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_GREATER_THAN_OR_EQUAL))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_GREATER_THAN_OR_EQUAL;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_LESS_THAN))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_LESS_THAN;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_LESS_THAN_OR_EQUAL))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_LESS_THAN_OR_EQUAL;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_NOT_MISSING;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_MISSING))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_MISSING;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_NOT_EQUAL_TO))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_NOT_EQUAL_TO;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_OR))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_OR;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_LIKE))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_LIKE;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_AND))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_AND;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_IS_ANY_OF))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_IS_ANY_OF;
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_IS_NOT_ANY_OF))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_IS_NOT_ANY_OF;
                }
                if (!(strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_BETWEEN)))
                {
                    svarname = strreadablecondition.Substring(strreadablecondition.IndexOf("[") + 1, strreadablecondition.IndexOf("]") - strreadablecondition.IndexOf("[") - 1);
                    _dashboardHelper.AddDataFilterCondition(sfilterOperand, sconditionval[0].ToString(), svarname, ConditionJoinType.And);
                }
                else if (strreadablecondition.Contains(SharedStrings.FRIENDLY_OPERATOR_BETWEEN))
                {
                    sfilterOperand = SharedStrings.FRIENDLY_OPERATOR_BETWEEN;
                    string strcondition = strreadablecondition.Substring(0, strreadablecondition.IndexOf(sfilterOperand)).Trim();
                    string[] strVarstrings = strcondition.Split(' ');
                    svarname = strVarstrings[3].ToString();
                    string sValues = strreadablecondition.ToString().Substring(strreadablecondition.IndexOf(sfilterOperand) + sfilterOperand.Length, (strreadablecondition.ToString().Length) - (strreadablecondition.ToString().IndexOf(sfilterOperand) + sfilterOperand.Length)).Trim();
                    shilowvars = sValues.Split(' ');
                    _dashboardHelper.AddDataFilterCondition(sfilterOperand, shilowvars[0].ToString(), shilowvars[2].ToString(), svarname, ConditionJoinType.And);
                }
            } */

        }

        void choroKMLprovider_FeatureLoaded(string serverName, IDictionary<string, object> featureAttributes)
        {
            if (!string.IsNullOrEmpty(serverName))
            {
                shapeFilePath = serverName;
                if (featureAttributes != null)
                {
                    cmbShapeKey.Items.Clear();
                    chorokmllayerprop.cbxShapeKey.Items.Clear();
                    foreach (string key in featureAttributes.Keys)
                    {
                        cmbShapeKey.Items.Add(key);
                        chorokmllayerprop.cbxShapeKey.Items.Add(key);
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
            else if (!string.IsNullOrEmpty(txtMapSeverpath.Text) || cbxmapserver.SelectedIndex != -1)
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
            cmbShapeKey.Items.Clear();
            txtShapePath.Text = string.Empty;
            txtKMLpath.Text = string.Empty;
            cbxmapfeature.SelectedIndex = -1;
            cbxmapserver.SelectedIndex = -1;
            radlocatemapserver.IsChecked = false;
            radconnectmapserver.IsChecked = false;
            txtMapSeverpath.Text = string.Empty;
            if (choroMapprovider != null)
            {
                choroMapprovider.FeatureLoaded -= new FeatureLoadedHandler(choroMapprovider_FeatureLoaded);
                choroMapprovider = null;
            }
            if (_provider != null)
            {
                layerprop.CloseLayer();
                _provider = null;
            }
        }

        public void ClearonShapeFile()
        {
            panelshape.IsEnabled = true;
            panelmap.IsEnabled = false;
            panelKml.IsEnabled = false;
            cmbShapeKey.Items.Clear();
            txtShapePath.Text = string.Empty;
            txtKMLpath.Text = string.Empty;
            cbxmapfeature.SelectedIndex = -1;
            cbxmapserver.SelectedIndex = -1;
            radlocatemapserver.IsChecked = false;
            radconnectmapserver.IsChecked = false;
            txtMapSeverpath.Text = string.Empty;
            if (choroMapprovider != null)
            {
                choroMapprovider.FeatureLoaded -= new FeatureLoadedHandler(choroMapprovider_FeatureLoaded);
                choroMapprovider = null;
            }
            if (choroKMLprovider != null)
            {
                choroKMLprovider.FeatureLoaded -= new FeatureLoadedHandler(choroKMLprovider_FeatureLoaded);
                choroKMLprovider = null;
            }
        }

        public void ClearonMapServer()
        {
            panelshape.IsEnabled = false;
            panelmap.IsEnabled = true;
            panelKml.IsEnabled = false;
            cmbShapeKey.Items.Clear();
            txtShapePath.Text = string.Empty;
            txtKMLpath.Text = string.Empty;
            cbxmapfeature.SelectedIndex = -1;
            cbxmapserver.SelectedIndex = -1;
            txtMapSeverpath.Text = string.Empty;
            if (choroKMLprovider != null)
            {
                choroKMLprovider.FeatureLoaded -= new FeatureLoadedHandler(choroKMLprovider_FeatureLoaded);
                choroKMLprovider = null;
            }
            if (_provider != null)
            {
                layerprop.CloseLayer();
                _provider = null;
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
            else if (!string.IsNullOrEmpty(txtMapSeverpath.Text) || cbxmapserver.SelectedIndex != -1)
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
                    ClearonMapServer();
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
                    ClearonMapServer();
            }
            else if (!string.IsNullOrEmpty(txtMapSeverpath.Text) || cbxmapserver.SelectedIndex != -1)
            {
                return;
            }
            else
                ClearonMapServer();
        }

        private void radconnectmapserver_Checked(object sender, RoutedEventArgs e)
        {
            panelmapconnect.IsEnabled = true;
            panelmapserver.IsEnabled = false;
            panelmapconnect.IsEnabled = true;
            txtMapSeverpath.Text = string.Empty;
        }

        public void cbxmapserver_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnableOkbtn();
            ResetMapServer();
        }

        public void ResetMapServer()
        {
            if (cbxmapserver.SelectedIndex > -1)
            {
                MapServerName = ((ComboBoxItem)cbxmapserver.SelectedItem).Content.ToString();

                if (choroMapprovider == null)
                {
                    choroMapprovider = new Mapping.ChoroplethServerLayerProvider(_myMap);
                    choroMapprovider.FeatureLoaded += new FeatureLoadedHandler(choroMapprovider_FeatureLoaded);
                }
                object[] mapFileProperties = choroMapprovider.LoadShapeFile(MapServerName + "/" + MapVisibleLayer);
                if (mapFileProperties != null)
                {
                    if (this.choroserverlayerprop == null)
                    {
                        ILayerProperties layerProperties = null;
                        layerProperties = new ChoroplethServerLayerProperties(_myMap, this.DashboardHelper, this._mapControl);
                        layerProperties.MapGenerated += new EventHandler(this._mapControl.ILayerProperties_MapGenerated);
                        layerProperties.FilterRequested += new EventHandler(this._mapControl.ILayerProperties_FilterRequested);
                        layerProperties.EditRequested += new EventHandler(this._mapControl.ILayerProperties_EditRequested);
                        this.choroserverlayerprop = (ChoroplethServerLayerProperties)layerProperties;
                        this._mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
                    }
                    choroserverlayerprop.shapeFilePath = MapServerName;
                    choroserverlayerprop.provider = choroMapprovider;
                    choroserverlayerprop.provider.FeatureLoaded += new FeatureLoadedHandler(choroserverlayerprop.provider_FeatureLoaded);
                    if (this.DashboardHelper != null)
                        choroserverlayerprop.SetdashboardHelper(DashboardHelper);

                }
            }
        }

        private void radlocatemapserver_Checked(object sender, RoutedEventArgs e)
        {
            panelmapconnect.IsEnabled = false;
            panelmapserver.IsEnabled = true;
            panelmapconnect.IsEnabled = false;
            cbxmapserver.SelectedIndex = -1;
        }

        private void ResetShapeCombo()
        {
            //In case of map server
            if (radMapServer.IsChecked == true)
            {
                cmbShapeKey.Items.Clear();
                cmbShapeKey.SelectedIndex = -1;
                if (choroserverlayerprop != null)
                {
                    choroserverlayerprop.cbxShapeKey.Items.Clear();
                    choroserverlayerprop.cbxShapeKey.SelectedIndex = -1;
                }
                /*
                //set back choroMapprovider
                if (choroMapprovider == null)
                {
                    choroMapprovider = new Mapping.ChoroplethServerLayerProvider(_myMap);
                    choroMapprovider.FeatureLoaded += new FeatureLoadedHandler(choroMapprovider_FeatureLoaded);
                    choroserverlayerprop.provider.FeatureLoaded -= new FeatureLoadedHandler(choroserverlayerprop.provider_FeatureLoaded);
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
                if (rest.layers.Count > 0)
                    cbxmapfeature.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                cbxmapfeature.DataContext = null;
                System.Windows.Forms.MessageBox.Show("Invalid map server");
            }
        }

        private void txtMapSeverpath_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableOkbtn();
            btnMapserverlocate.IsEnabled = txtMapSeverpath.Text.Length > 0;
        }

        public void cbxmapfeature_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnableOkbtn();
            MapfeatureSelectionChange();
        }

        public void MapfeatureSelectionChange()
        {
            if (cbxmapfeature.Items.Count > 0)
            {
                MapServerName = txtMapSeverpath.Text;
                if (cbxmapfeature.SelectedIndex > -1)
                {
                    int visibleLayer = ((SubObject)cbxmapfeature.SelectedItem).id;
                    MapVisibleLayer = visibleLayer;
                    if (choroMapprovider == null)
                    {
                        choroMapprovider = new Mapping.ChoroplethServerLayerProvider(_myMap);
                        choroMapprovider.FeatureLoaded += new FeatureLoadedHandler(choroMapprovider_FeatureLoaded);
                    }
                    object[] mapFileProperties = choroMapprovider.LoadShapeFile(MapServerName + "/" + MapVisibleLayer);
                    if (mapFileProperties != null)
                    {
                        if (choroserverlayerprop == null)
                        {
                            ILayerProperties layerProperties = null;
                            layerProperties = new ChoroplethServerLayerProperties(_myMap, this.DashboardHelper, this._mapControl);
                            layerProperties.MapGenerated += new EventHandler(this._mapControl.ILayerProperties_MapGenerated);
                            layerProperties.FilterRequested += new EventHandler(this._mapControl.ILayerProperties_FilterRequested);
                            layerProperties.EditRequested += new EventHandler(this._mapControl.ILayerProperties_EditRequested);
                            this.choroserverlayerprop = (ChoroplethServerLayerProperties)layerProperties;
                            this._mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
                        }
                        choroserverlayerprop.shapeFilePath = MapServerName;
                        choroserverlayerprop.provider = choroMapprovider;
                        choroserverlayerprop.provider.FeatureLoaded += new FeatureLoadedHandler(choroserverlayerprop.provider_FeatureLoaded);
                        if (this.DashboardHelper != null)
                            choroserverlayerprop.SetdashboardHelper(DashboardHelper);

                    }
                }
                else
                {
                    MapVisibleLayer = -1;
                }
            }
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


        private void EnableOkbtn()
        {
            if (!string.IsNullOrEmpty(txtProjectPath.Text) && (!string.IsNullOrEmpty(txtShapePath.Text)
                       || cbxmapserver.SelectedIndex != -1
                       || (!string.IsNullOrEmpty(txtMapSeverpath.Text)
                       || (!string.IsNullOrEmpty(txtKMLpath.Text)))))
            {
                btnOK.IsEnabled = true;
            }
            else
            {
                btnOK.IsEnabled = false;
            }


        }

        private void EnableOkbtn(object sender, TextChangedEventArgs e)
        {
            EnableOkbtn();
        }

    }

}


