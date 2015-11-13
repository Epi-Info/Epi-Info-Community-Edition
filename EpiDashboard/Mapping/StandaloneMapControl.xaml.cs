using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Geometry;
using Epi;
using Epi.Data;
using EpiDashboard.Controls;

namespace EpiDashboard.Mapping
{    

    public delegate List<object> DataSourceRequestedHandler();
    public delegate void MouseCoordinatesChangedHandler(double latitude, double longitude);
    public delegate void MapLoadedHandler(StandaloneMapControl mapForm, bool isTimeLapsePossible);

    /// <summary>
    /// Interaction logic for MapControl.xaml
    /// </summary>
    public partial class StandaloneMapControl : UserControl, IMapControl
    {
        public event MapLoadedHandler MapLoaded;
        public event RecordSelectedHandler RecordSelected;
        public event TimeVariableSetHandler TimeVariableSet;
        public event EventHandler MapDataChanged;
        public event DataSourceRequestedHandler DataSourceRequested;
        public event MouseCoordinatesChangedHandler MouseCoordinatesChanged;
        public event EventHandler ExpandRequested;
        public event EventHandler RestoreRequested;

        private BackgroundWorker worker;
        private BackgroundWorker openDefaultMapWorker;
        private Map myMap;
        private MapPoint rightClickedPoint;
        private delegate void RenderMapDelegate(string url);
        private delegate void SimpleDelegate();
        private delegate void DebugDelegate(string debugMsg);
        private TimeSlider slider;
        private DataFilteringControl dataFilteringControl;
        private LayerList layerList;
        private string currentTimeVariable;
        private Navigation nav;
        private string defaultMapPath = string.Empty;
        private MapBackgroundType defaultBackgroundType = MapBackgroundType.Satellite;
        private bool bypassInternetCheck;
        private Brush defaultBackgroundColor = Brushes.White;
        private bool hidePanels = false;
        private PointofInterestProperties pointofinterestproperties;
        private CaseClusterProperties caseclusterproperties;
        private DotDensityProperties dotdensityproperties;
        private ChoroplethProperties choroplethproperties;
       
        public double ResizedWidth { get; set; }
        public double ResizedHeight { get; set; }

        public StandaloneMapControl()
        {
            InitializeComponent();
            //btnBypass.Click += new RoutedEventHandler(btnBypass_Click);
            Construct();

            #region Translation
            txtPointOfInterest.Text = DashboardSharedStrings.GADGET_CONFIG_TITLE_SPOTMAP;
            txtCaseCluster.Text = DashboardSharedStrings.GADGET_CONFIG_TITLE_CLUSTER;
            txtChoropleth.Text = DashboardSharedStrings.GADGET_CONFIG_TITLE_CHOROPLETH;
            txtDotDensity.Text = DashboardSharedStrings.GADGET_CONFIG_TITLE_DOTDENSITY;

            ImageryRadioButton.Text = DashboardSharedStrings.GADGET_CONFIG_TITLE_SATELLITE;
            ImageryRadioButton_alt.Text = DashboardSharedStrings.GADGET_CONFIG_TITLE_SATELLITE;
            StreetsRadioButton.Text = DashboardSharedStrings.GADGET_CONFIG_TITLE_STREETS;
            StreetsRadioButton_alt.Text = DashboardSharedStrings.GADGET_CONFIG_TITLE_STREETS;
            BlankRadioButton.Text = DashboardSharedStrings.GADGET_CONFIG_TITLE_BLANK;
            BlankRadioButton_alt.Text = DashboardSharedStrings.GADGET_CONFIG_TITLE_BLANK;
            tooltipExpandHeader.Content = DashboardSharedStrings.MENU_FULLSCREEN;
            tooltipExpand.Text = DashboardSharedStrings.MENU_FULLSCREEN_MODE;
            tooltipRestoreHeader.Content = DashboardSharedStrings.MENU_WINDOW;
            tooltipRestore.Text = DashboardSharedStrings.MENU_WINDOW_MODE;

            tooltipSaveHeader.Content = DashboardSharedStrings.MENU_SAVE;
            tooltipSave.Text = DashboardSharedStrings.MENU_SAVE_MAP;

            tooltipOpenHeader.Content = DashboardSharedStrings.MENU_OPEN;
            tooltipOpen.Text = DashboardSharedStrings.MENU_OPEN_MAP;

            tooltipSaveAsImageHeader.Content = DashboardSharedStrings.MENU_SAVE_AS_IMAGE;
            tooltipSaveAsImage.Text = DashboardSharedStrings.MENU_SAVE_AS_IMAGE_MAP;

            tooltipAddDataLayerHeader.Content = DashboardSharedStrings.MENU_ADD_LAYER;
            tooltipAddDataLayer.Text = DashboardSharedStrings.MENU_ADD_LAYER_MAP;

            tooltipAddReferenceLayerHeader.Content = DashboardSharedStrings.MENU_ADD_BASELAYER ;
            tooltipAddReferenceLayer.Text = DashboardSharedStrings.MENU_ADD_BASELAYER_MAP;

            tooltipTimeLapseHeader.Content = DashboardSharedStrings.MENU_TIME_LAPSE;
            tooltipTimeLapse.Text = DashboardSharedStrings.MENU_TIME_LAPSE_MAP;

            #endregion
        }

        public StandaloneMapControl(Brush backgroundColor)
        {
            InitializeComponent();
            //btnBypass.Click += new RoutedEventHandler(btnBypass_Click);
            this.MapBackground = backgroundColor;
            Construct();
        }

        public StandaloneMapControl(Brush backgroundColor, string mapPath)
        {
            InitializeComponent();
            //btnBypass.Click += new RoutedEventHandler(btnBypass_Click);
            this.MapBackground = backgroundColor;
            this.defaultMapPath = mapPath;
            Construct();
        }

        public StandaloneMapControl(Brush backgroundColor, MapBackgroundType backgroundType, string mapPath, bool hidePanels = false)
        {
            InitializeComponent();
            //btnBypass.Click += new RoutedEventHandler(btnBypass_Click);
            this.MapBackground = backgroundColor;
            this.defaultMapPath = mapPath;
            this.defaultBackgroundType = backgroundType;
            this.hidePanels = hidePanels;
            Construct();
        }

        private void Construct()
        {
            worker = new System.ComponentModel.BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.RunWorkerAsync();
            MapContainer.SizeChanged += new SizeChangedEventHandler(MapContainer_SizeChanged);
            imgClose.MouseDown += new MouseButtonEventHandler(imgClose_MouseDown);
            imgCloseLayer.MouseDown += new MouseButtonEventHandler(imgCloseLayer_MouseDown);
        }

        public Brush MapBackground
        {
            get
            {
                return this.defaultBackgroundColor;
            }
            set
            {
                this.defaultBackgroundColor = value;
                SetBackgroundColor(value);
            }
        }

        void btnBypass_Click(object sender, RoutedEventArgs e)
        {
            bypassInternetCheck = true;
        }

        void myMap_Loaded(object sender, RoutedEventArgs e)
        {
            //if (!string.IsNullOrEmpty(defaultMapPath))
            //{
            //    this.Dispatcher.BeginInvoke(new SimpleDelegate(OpenDefaultMap));
            //}            
            if (!string.IsNullOrEmpty(defaultMapPath))
            {
                openDefaultMapWorker = new System.ComponentModel.BackgroundWorker();
                openDefaultMapWorker.DoWork += new DoWorkEventHandler(openDefaultMapWorker_DoWork);
                openDefaultMapWorker.RunWorkerAsync();
            }
        }

        void myMap_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }

        void imgCloseLayer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DoubleAnimation anim = new DoubleAnimation();
            anim.From = 1;
            anim.To = 0;
            anim.DecelerationRatio = 0.8;
            anim.Duration = new Duration(TimeSpan.FromSeconds(1));
            anim.Completed += new EventHandler(anim_Completed);
            grdLayerConfig.BeginAnimation(Grid.OpacityProperty, anim);
        }

        void anim_Completed(object sender, EventArgs e)
        {
            ILayerProperties layerProperties = (ILayerProperties)grdLayerConfigContainer.Children[0];
            layerProperties.CloseLayer();
            grdLayerConfigContainer.Children.Clear();
            grdLayerConfig.Visibility = Visibility.Collapsed;
        }

        void imgClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (TimeVariableSet != null)
            {
                currentTimeVariable = null;
                TimeVariableSet(null);
            }
            grdTimeLapse.Visibility = Visibility.Collapsed;
            stkTimeLapse.Visibility = Visibility.Collapsed;
            chrtTimeLapse.Visibility = Visibility.Collapsed;
        }

        void MapContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (myMap != null)
            {
                if (myMap.IsEnabled)
                {
                    myMap.Width = e.NewSize.Width;
                    myMap.Height = e.NewSize.Height;
                }
            }
            if (dataFilteringControl != null)
            {
                Canvas.SetTop(dataFilteringControl, (e.NewSize.Height / 2.0) - (dataFilteringControl.ActualHeight / 2.0));
            }
            if (layerList != null)
            {
                Canvas.SetLeft(layerList, (e.NewSize.Width / 2.0) - (layerList.ActualWidth / 2.0));
                Canvas.SetBottom(layerList, (layerList.ActualHeight * -1) + 30);
            }
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //if (!string.IsNullOrEmpty(defaultMapPath))
            //{
            //    openDefaultMapWorker = new System.ComponentModel.BackgroundWorker();
            //    openDefaultMapWorker.DoWork += new DoWorkEventHandler(openDefaultMapWorker_DoWork);
            //    openDefaultMapWorker.RunWorkerAsync();
            //}
            switch (this.defaultBackgroundType)
            {
                case MapBackgroundType.Satellite:
                    ToggleSatellite();
                    break;
                case MapBackgroundType.Street:
                    ToggleStreet();
                    break;
                case MapBackgroundType.None:
                    ToggleBlank();
                    break;
            }
        }

        private void ToggleSatellite()
        {
            ImageryRadioButton.Visibility = System.Windows.Visibility.Collapsed;
            ImageryRadioButton_alt.Visibility = System.Windows.Visibility.Visible;
            StreetsRadioButton_alt.Visibility = System.Windows.Visibility.Collapsed;
            StreetsRadioButton.Visibility = System.Windows.Visibility.Visible;
            BlankRadioButton_alt.Visibility = System.Windows.Visibility.Collapsed;
            BlankRadioButton.Visibility = System.Windows.Visibility.Visible;
        }

        private void ToggleStreet()
        {
            StreetsRadioButton.Visibility = System.Windows.Visibility.Collapsed;
            StreetsRadioButton_alt.Visibility = System.Windows.Visibility.Visible;
            ImageryRadioButton_alt.Visibility = System.Windows.Visibility.Collapsed;
            ImageryRadioButton.Visibility = System.Windows.Visibility.Visible;
            BlankRadioButton_alt.Visibility = System.Windows.Visibility.Collapsed;
            BlankRadioButton.Visibility = System.Windows.Visibility.Visible;
        }

        private void ToggleBlank()
        {
            BlankRadioButton.Visibility = System.Windows.Visibility.Collapsed;
            BlankRadioButton_alt.Visibility = System.Windows.Visibility.Visible;
            ImageryRadioButton_alt.Visibility = System.Windows.Visibility.Collapsed;
            ImageryRadioButton.Visibility = System.Windows.Visibility.Visible;
            StreetsRadioButton_alt.Visibility = System.Windows.Visibility.Collapsed;
            StreetsRadioButton.Visibility = System.Windows.Visibility.Visible;
        }

        void openDefaultMapWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(2000);
            this.Dispatcher.BeginInvoke(new SimpleDelegate(OpenDefaultMap));
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            //if (!bypassInternetCheck)
            //{
            //    this.Dispatcher.BeginInvoke(new SimpleDelegate(TryToConnect));
            //    while (!InternetAvailable())
            //    {
            //        if (bypassInternetCheck)
            //        {
            //            break;
            //        }
            //        System.Threading.Thread.Sleep(250);
            //    }
            //    this.Dispatcher.BeginInvoke(new SimpleDelegate(PrepareToLoad));
            //    System.Threading.Thread.Sleep(500);
            //}
            this.Dispatcher.BeginInvoke(new SimpleDelegate(PrepareToLoad));
            System.Threading.Thread.Sleep(2000);
            this.Dispatcher.BeginInvoke(new SimpleDelegate(RenderMap));
        }

        private void PrepareToLoad()
        {
            //txtInternet.Visibility = Visibility.Collapsed;
            txtLoading.Text = DashboardSharedStrings.MAP_LOADING;
            txtLoading.Visibility = Visibility.Visible;
            waitCursor.Visibility = Visibility.Visible;
        }

        private void TryToConnect()
        {
            //txtInternet.Visibility = Visibility.Visible;
            //txtLoading.Text = "Trying to connect...";
            //txtLoading.Visibility = Visibility.Visible;
            //waitCursor.Visibility = Visibility.Visible;
        }

        private void RenderMap()
        {
            try
            {
                txtLoading.Visibility = Visibility.Collapsed;
                waitCursor.Visibility = Visibility.Collapsed;

                ESRI.ArcGIS.Client.Bing.TileLayer layer = new TileLayer();
                layer.InitializationFailed += new EventHandler<EventArgs>(layer_InitializationFailed);

                if (ImageryRadioButton.Visibility == System.Windows.Visibility.Collapsed)
                {
                    layer.Token = Configuration.GetNewInstance().Settings.MapServiceKey;
                    layer.LayerStyle = TileLayer.LayerType.AerialWithLabels;
                }
                else if (StreetsRadioButton.Visibility == System.Windows.Visibility.Collapsed)
                {
                    layer.Token = Configuration.GetNewInstance().Settings.MapServiceKey;
                    layer.LayerStyle = TileLayer.LayerType.Road;
                }
                else
                {
                    layer.Token = null;
                }

                GraphicsLayer pointLayer = new GraphicsLayer();
                pointLayer.ID = "pointLayer";

                GraphicsLayer zoneLayer = new GraphicsLayer();
                zoneLayer.ID = "zoneLayer";

                GraphicsLayer textLayer = new GraphicsLayer();
                textLayer.ID = "textLayer";

                ContextMenu menu = new ContextMenu();

                MenuItem mnuMarker = new MenuItem();
                mnuMarker.Header = DashboardSharedStrings.GADGET_MAP_ADD_MARKER;
                mnuMarker.Click += new RoutedEventHandler(mnuMarker_Click);
                menu.Items.Add(mnuMarker);
                MenuItem mnuRadius = new MenuItem();
                mnuRadius.Header = DashboardSharedStrings.GADGET_MAP_ADD_ZONE;
                mnuRadius.Click += new RoutedEventHandler(mnuRadius_Click);
                menu.Items.Add(mnuRadius);
                MenuItem mnuText = new MenuItem();
                mnuText.Header = DashboardSharedStrings.GADGET_MAP_ADD_LABEL;
                mnuText.Click += new RoutedEventHandler(mnuText_Click);
                menu.Items.Add(mnuText);
                MenuItem mnuClear = new MenuItem();
                mnuClear.Header = DashboardSharedStrings.GADGET_MAP_REMOVE_LAYERS;
                mnuClear.Click += new RoutedEventHandler(mnuClear_Click);
                menu.Items.Add(mnuClear);

                myMap = new Map();
                myMap.Background = Brushes.White;
                myMap.Height = MapContainer.ActualHeight;
                myMap.Width = MapContainer.ActualWidth;
                myMap.WrapAround = true;
                myMap.ContextMenu = menu;
                myMap.Layers.Add(layer);
                myMap.Layers.Add(pointLayer);
                myMap.Layers.Add(textLayer);
                myMap.Layers.Add(zoneLayer);

                //<esri:ArcGISDynamicMapServiceLayer ID="California" Opacity="0.4" VisibleLayers="8,10"
                //    Url="http://maverick.arcgis.com/ArcGIS/rest/services/California/MapServer" />

                //ArcGISDynamicMapServiceLayer govData = new ArcGISDynamicMapServiceLayer();
                //govData.Url = "http://services.nationalmap.gov/ArcGIS/rest/services/govunits/MapServer";
                ////govData.Url = "http://server.arcgisonline.com/ArcGIS/rest/services/World_Topo_Map/MapServer";
                //govData.VisibleLayers = new int[] { 19,21,22 };
                //myMap.Layers.Add(govData);

                myMap.MouseMove += new MouseEventHandler(myMap_MouseMove);
                myMap.MouseRightButtonDown += new MouseButtonEventHandler(myMap_MouseRightButtonDown);
                myMap.Loaded += new RoutedEventHandler(myMap_Loaded);

                MapContainer.Children.Add(myMap);

                ESRI.ArcGIS.Client.Behaviors.ConstrainExtentBehavior extentBehavior = new ESRI.ArcGIS.Client.Behaviors.ConstrainExtentBehavior();
                extentBehavior.ConstrainedExtent = new Envelope(new MapPoint(int.MinValue, -12000000), new MapPoint(int.MaxValue, 12000000));
                System.Windows.Interactivity.Interaction.GetBehaviors(myMap).Add(extentBehavior);

                nav = new Navigation();
                nav.Margin = new Thickness(5);
                nav.HorizontalAlignment = HorizontalAlignment.Left;
                nav.VerticalAlignment = VerticalAlignment.Top;
                nav.Map = myMap;
                MapContainer.Children.Add(nav);

                slider = new TimeSlider();
                slider.Name = "slider";
                slider.PlaySpeed = new TimeSpan(0, 0, 1);
                slider.Height = 20;
                slider.TimeMode = TimeMode.CumulativeFromStart;
                slider.MinimumValue = DateTime.Now.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
                slider.MaximumValue = DateTime.Now.ToUniversalTime();
                slider.Value = new TimeExtent(slider.MinimumValue, slider.MinimumValue.AddHours(2));
                slider.Intervals = TimeSlider.CreateTimeStopsByTimeInterval(new TimeExtent(slider.MinimumValue, slider.MaximumValue), new TimeSpan(0, 2, 0, 0));
                slider.Padding = new Thickness(0, 100, 0, 0);
                slider.ValueChanged += new EventHandler<TimeSlider.ValueChangedEventArgs>(slider_ValueChanged);
                areaSeries.Loaded += new RoutedEventHandler(areaSeries_Loaded);
                stkTimeLapse.Children.Add(slider);

                //grdMapDef.Background = Brushes.Black;
                //myMap.Background = Brushes.Black;
                SetBackgroundColor(defaultBackgroundColor);

                AddLayerList();

                if (MapLoaded != null)
                {
                    MapLoaded(this, false);
                }
            }
            catch (Exception ex)
            {
                //
            }
            finally
            {
            }
        }

        private void SetBackgroundColor(Brush brush)
        {
            if (grdMapDef != null && myMap != null)
            {
                grdMapDef.Background = brush;
                myMap.Background = brush;
            }
        }

        void layer_InitializationFailed(object sender, EventArgs e)
        {
            myMap.IsEnabled = false;
            nav.Visibility = System.Windows.Visibility.Collapsed;
            //if (MessageBox.Show("Internet connection was lost. Try again to connect?", "Experiencing Internet issues", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            //{
            //    RenderMap();
            //}
        }

        private enum LayerType
        {
            CaseCluster,
            ChoroplethShapeFile,
            ChoroplethMapServer,
            Marker,
            Zone,
            Text,
            ChoroplethKml,
            DotDensityKml,
            DotDensityShapeFile,
            DotDensityMapServer,
            PointMap
        }

        private void mnuChoroplethMap_Click(object sender, RoutedEventArgs e)
        {
            GenerateShapeFileChoropleth();
        }

        private void mnuClusterMap_Click(object sender, RoutedEventArgs e)
        {
            GenerateCaseCluster();
        }

        private void mnuMarker_Click(object sender, RoutedEventArgs e)
        {
            AddLayer(LayerType.Marker);
        }

        private void mnuText_Click(object sender, RoutedEventArgs e)
        {
            AddLayer(LayerType.Text);
        }

        private void mnuRadius_Click(object sender, RoutedEventArgs e)
        {
            AddLayer(LayerType.Zone);
        }

        public string ProjectFilepath { get; set; }
        public DashboardHelper GetNewDashboardHelper()
        {
            List<object> resultArray = DataSourceRequested();
            if (resultArray != null)
            {
                object dataSource = resultArray[0];
                string dataMember = resultArray[1].ToString();
                DashboardHelper dashboardHelper;
                if (dataSource is Project)
                {
                    Project project = (Project)dataSource;
                    View view = project.GetViewByName(dataMember);
                    IDbDriver dbDriver = DBReadExecute.GetDataDriver(project.FilePath);
                    ProjectFilepath = project.FilePath;
                    dashboardHelper = new DashboardHelper(view, dbDriver);
                }
                else
                {
                    IDbDriver dbDriver = (IDbDriver)dataSource;
                    dashboardHelper = new DashboardHelper(dataMember, dbDriver);
                    ProjectFilepath = dashboardHelper.Database.DataSource;
                }
                dashboardHelper.PopulateDataSet();
                return dashboardHelper;
            }
            else
            {
                return null;
            }
        }

        private void AddLayer(LayerType layerType)
        {
            ILayerProperties layerProperties = null;

            switch (layerType)
            {
                case LayerType.Marker:
                    layerProperties = new MarkerProperties(myMap, rightClickedPoint);
                    break;
                case LayerType.Zone:
                    layerProperties = new ZoneProperties(myMap, rightClickedPoint);
                    break;
                case LayerType.Text:
                    layerProperties = new TextProperties(myMap, rightClickedPoint);
                    break;
                default:
                    if (DataSourceRequested != null)
                    {
                        List<object> resultArray = DataSourceRequested();
                        if (resultArray != null)
                        {
                            object dataSource = resultArray[0];
                            string dataMember = resultArray[1].ToString();
                            DashboardHelper dashboardHelper;
                            if (dataSource is Project)
                            {
                                Project project = (Project)dataSource;
                                View view = project.GetViewByName(dataMember);
                                IDbDriver dbDriver = DBReadExecute.GetDataDriver(project.FilePath);
                                dashboardHelper = new DashboardHelper(view, dbDriver);
                            }
                            else
                            {
                                IDbDriver dbDriver = (IDbDriver)dataSource;
                                dashboardHelper = new DashboardHelper(dataMember, dbDriver);
                            }
                            dashboardHelper.PopulateDataSet();

                            switch (layerType)
                            {
                                case LayerType.CaseCluster:
                                    layerProperties = new ClusterLayerProperties(myMap, dashboardHelper, this);
                                    break;
                                case LayerType.ChoroplethShapeFile:
                                    layerProperties = new ChoroplethShapeLayerProperties(myMap, dashboardHelper, this);
                                    break;
                                case LayerType.ChoroplethMapServer:
                                    layerProperties = new ChoroplethServerLayerProperties(myMap, dashboardHelper, this);
                                    break;
                                case LayerType.ChoroplethKml:
                                    layerProperties = new ChoroplethKmlLayerProperties(myMap, dashboardHelper, this);
                                    break;
                                case LayerType.DotDensityKml:
                                    layerProperties = new DotDensityKmlLayerProperties(myMap, dashboardHelper, this);
                                    break;
                                case LayerType.DotDensityShapeFile:
                                    layerProperties = new DotDensityLayerProperties(myMap, dashboardHelper, this);
                                    break;
                                case LayerType.DotDensityMapServer:
                                    layerProperties = new DotDensityServerLayerProperties(myMap, dashboardHelper, this);
                                    break;
                                case LayerType.PointMap:
                                    layerProperties = new PointLayerProperties(myMap, dashboardHelper, this);
                                    break;
                                default:
                                    layerProperties = new ClusterLayerProperties(myMap, dashboardHelper, this);
                                    break;
                            }
                        }
                    }
                    break;
            }
            if (layerProperties != null)
            {
                layerProperties.MapGenerated += new EventHandler(ILayerProperties_MapGenerated);
                layerProperties.FilterRequested += new EventHandler(ILayerProperties_FilterRequested);

                grdLayerConfig.Opacity = 0;
                grdLayerConfig.Margin = new Thickness(0);
                grdLayerConfig.Visibility = Visibility.Visible;
                grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
                //grdMapDef.Children.Add((UIElement)layerProperties);
                
                DoubleAnimation opacityAnim = new DoubleAnimation();
                opacityAnim.From = 0;
                opacityAnim.To = 1;
                opacityAnim.DecelerationRatio = 0.8;
                opacityAnim.Duration = new Duration(TimeSpan.FromSeconds(0.3));

                DoubleAnimation widthAnim = new DoubleAnimation();
                widthAnim.From = 830;
                widthAnim.To = 850;
                widthAnim.DecelerationRatio = 0.8;
                widthAnim.Duration = new Duration(TimeSpan.FromSeconds(0.3));

                DoubleAnimation heightAnim = new DoubleAnimation();
                heightAnim.From = 70;
                heightAnim.To = 90;
                heightAnim.DecelerationRatio = 0.8;
                heightAnim.Duration = new Duration(TimeSpan.FromSeconds(0.3));

                ThicknessAnimation thicknessAnim = new ThicknessAnimation();
                thicknessAnim.From = new Thickness(1);
                thicknessAnim.To = new Thickness(0);
                thicknessAnim.AccelerationRatio = 0.8;
                thicknessAnim.Duration = new Duration(TimeSpan.FromSeconds(0.3));

                grdLayerConfig.BeginAnimation(Grid.MarginProperty, thicknessAnim);
                grdLayerConfig.BeginAnimation(Grid.OpacityProperty, opacityAnim);
                grdLayerConfig.BeginAnimation(Grid.WidthProperty, widthAnim);
                grdLayerConfig.BeginAnimation(Grid.HeightProperty, heightAnim);
            }
        }

       public void ILayerProperties_FilterRequested(object sender, EventArgs e)
        {
            ILayerProperties layerProperties = (ILayerProperties)sender;
            DashboardHelper dashboardHelper = layerProperties.GetDashboardHelper();
            AddSelectionCriteria(dashboardHelper);
        }
       //--
       public void ILayerProperties_EditRequested(object sender, EventArgs e)
       {
           ILayerProperties layerProperties = (ILayerProperties)sender;

           if (layerProperties is PointLayerProperties)
           {
               PointLayerProperties pointlayerprop = (PointLayerProperties)layerProperties;
               if (pointlayerprop.FlagRunEdit == false)
               { GeneratePointofInterestMap(pointlayerprop); }
           }
           else if (layerProperties is ClusterLayerProperties)
           {
               ClusterLayerProperties clusterlayerprop = (ClusterLayerProperties)layerProperties;
               if (clusterlayerprop.FlagRunEdit == false)
               { GenerateCaseClusterMap(clusterlayerprop); }
           }
           else if (layerProperties is DotDensityLayerProperties)
           {
               DotDensityLayerProperties densitylayerprop = (DotDensityLayerProperties)layerProperties;
               if (densitylayerprop.FlagRunEdit == false)
               { GenerateShapeFileDotDensity(densitylayerprop); }
           }
           else if (layerProperties is DotDensityServerLayerProperties)
           {
               DotDensityServerLayerProperties densitylayerprop = (DotDensityServerLayerProperties)layerProperties;
               if (densitylayerprop.FlagRunEdit == false)
               { GenerateShapeFileDotDensity(densitylayerprop); }
           }
           else if (layerProperties is DotDensityKmlLayerProperties)
           {
               DotDensityKmlLayerProperties densitylayerprop = (DotDensityKmlLayerProperties)layerProperties;
               if (densitylayerprop.FlagRunEdit == false)
               { GenerateShapeFileDotDensity(densitylayerprop); }
           }
           else if (layerProperties is ChoroplethShapeLayerProperties)
           {
               ChoroplethShapeLayerProperties choroplethlayerprop = (ChoroplethShapeLayerProperties)layerProperties;
               if (choroplethlayerprop.FlagRunEdit == false)
               { GenerateShapeFileChoropleth(choroplethlayerprop); }
           }
           else if (layerProperties is ChoroplethServerLayerProperties)
           {
               ChoroplethServerLayerProperties choroplethserverlayerprop = (ChoroplethServerLayerProperties)layerProperties;
               if (choroplethserverlayerprop.FlagRunEdit == false)
               { GenerateShapeFileChoropleth(choroplethserverlayerprop); }
           }
           else if (layerProperties is ChoroplethKmlLayerProperties)
           {
               ChoroplethKmlLayerProperties choroplethkmllayerprop = (ChoroplethKmlLayerProperties)layerProperties;
               if (choroplethkmllayerprop.FlagRunEdit == false)
               { GenerateShapeFileChoropleth(choroplethkmllayerprop); }
           }
       }
       //--
       public void ILayerProperties_MapGenerated(object sender, EventArgs e)
        {
            if (grdLayerConfigContainer.Children.Contains((UIElement)sender))
            {
                ThicknessAnimation thicknessAnim = new ThicknessAnimation();
                thicknessAnim.From = new Thickness(0);
                thicknessAnim.To = new Thickness(0, 150, 0, 0);
                thicknessAnim.DecelerationRatio = 0.8;
                thicknessAnim.Duration = new Duration(TimeSpan.FromSeconds(1));

                DoubleAnimation layerConfigCompleteAnim = new DoubleAnimation();
                layerConfigCompleteAnim.From = 1;
                layerConfigCompleteAnim.To = 0;
                layerConfigCompleteAnim.DecelerationRatio = 0.8;
                layerConfigCompleteAnim.Duration = new Duration(TimeSpan.FromSeconds(1));
                layerConfigCompleteAnim.Completed += new EventHandler(layerConfigCompleteAnim_Completed);

                grdLayerConfig.BeginAnimation(Grid.OpacityProperty, layerConfigCompleteAnim);
                grdLayerConfig.BeginAnimation(Grid.MarginProperty, thicknessAnim);

                grdLayerConfigContainer.Children.Remove((UIElement)sender);
                layerList.AddListItem((ILayerProperties)sender, 0);
                
                if (((ILayerProperties)sender).LegendStackPanel != null)
                {
                    stkLegends.Children.Add(((ILayerProperties)sender).LegendStackPanel);
                }

            }
            else
            {
                if (sender is IReferenceLayerProperties)
                {
                    layerList.AddListItem((ILayerProperties)sender, 0);
                }
                if (((ILayerProperties)sender).LegendStackPanel != null)
                {
                    if (!stkLegends.Children.Contains(((ILayerProperties)sender).LegendStackPanel))
                        stkLegends.Children.Add(((ILayerProperties)sender).LegendStackPanel);
                }
            }
            if (MapLoaded != null)
            {
                MapLoaded(this, layerList.ClusterLayers.Count > 0);
                if (layerList.ClusterLayers.Count == 1)
                {
                    iconTimeLapse.Visibility = Visibility.Visible;
                }
                else
                {
                    iconTimeLapse.Visibility = Visibility.Collapsed;
            }
        }
        }

        void layerConfigCompleteAnim_Completed(object sender, EventArgs e)
        {
            grdLayerConfig.Margin = new Thickness(0);
            grdLayerConfig.Visibility = System.Windows.Visibility.Collapsed;
        }

        void AddLayerList()
        {
            if (layerList != null)
            {
                if (MapContainer.Children.Contains(layerList))
                {
                    MapContainer.Children.Remove(layerList);
                }
            }
            layerList = new LayerList(myMap, null, null, null);
            layerList.Loaded += new RoutedEventHandler(layerList_Loaded);
            layerList.SizeChanged += new SizeChangedEventHandler(layerList_SizeChanged);
            layerList.MouseEnter += new MouseEventHandler(layerList_MouseEnter);
            layerList.MouseLeave += new MouseEventHandler(layerList_MouseLeave);
            layerList.LayerClosed += new EventHandler(layerList_LayerClosed);

            if (hidePanels)
            {
                layerList.Visibility = System.Windows.Visibility.Collapsed;
            }

            MapContainer.Children.Add(layerList);
        }

        void layerList_LayerClosed(object sender, EventArgs e)
        {
            if (MapLoaded != null)
            {
                MapLoaded(this, layerList.ClusterLayers.Count > 0);

                if (layerList.ClusterLayers.Count == 1)
                {
                    iconTimeLapse.Visibility = Visibility.Visible;
                }
                else
                {
                    iconTimeLapse.Visibility = Visibility.Collapsed;
                }
            }
        }

        void layerList_MouseLeave(object sender, MouseEventArgs e)
        {
            DoubleAnimation anim = new DoubleAnimation();
            anim.From = Canvas.GetBottom(layerList);
            anim.To = (layerList.ActualHeight * -1) + 30;
            anim.DecelerationRatio = 0.8;
            anim.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            layerList.BeginAnimation(Canvas.BottomProperty, anim);
        }

        void layerList_MouseEnter(object sender, MouseEventArgs e)
        {
            DoubleAnimation anim = new DoubleAnimation();
            anim.From = Canvas.GetBottom(layerList);
            anim.To = -20;
            anim.AccelerationRatio = 0.8;
            anim.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            layerList.BeginAnimation(Canvas.BottomProperty, anim);
        }

        void layerList_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Canvas.SetLeft(layerList, (MapContainer.ActualWidth / 2.0) - (e.NewSize.Width / 2.0));

          //  if (e.PreviousSize.Height < e.NewSize.Height)
           // {
                DoubleAnimation anim = new DoubleAnimation();
                anim.From = double.IsNaN(Canvas.GetBottom(layerList)) ? 0 : Canvas.GetBottom(layerList);
                anim.To = (e.NewSize.Height * -1) + 30;
                anim.DecelerationRatio = 0.8;
                anim.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                layerList.BeginAnimation(Canvas.BottomProperty, anim);
           //}

            if(layerList.HasLayers)
            {
                iconLegend.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                iconLegend.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (layerList.ClusterLayers.Count == 1)
            {
                iconTimeLapse.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                iconTimeLapse.Visibility = System.Windows.Visibility.Collapsed;
            }
            if (layerList.ClusterLayers.Count == 0)
            {
                iconTimeLapse.Visibility = Visibility.Collapsed;
            }
        }

        void layerList_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas.SetLeft(layerList, (MapContainer.ActualWidth / 2.0) - (layerList.ActualWidth / 2.0));
            Canvas.SetBottom(layerList, (layerList.ActualHeight * -1) + 30);
        }

        void AddSelectionCriteria(DashboardHelper dashboardHelper)
        {
            dataFilteringControl = new DataFilteringControl(dashboardHelper);
            dataFilteringControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            dataFilteringControl.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            dataFilteringControl.IsClosable = true;
            dataFilteringControl.SelectionCriteriaChanged += new EventHandler(dataFilteringControl_SelectionCriteriaChanged);
            LayoutRoot.Children.Add(dataFilteringControl);

            DragCanvas.SetCanBeDragged(dataFilteringControl, false);
        }
       
        void dataFilteringControl_SelectionCriteriaChanged(object sender, EventArgs e)
        {
            if (MapDataChanged != null)
            {
                MapDataChanged(this, new EventArgs());
            }
        }

        void mnuTimeLapse_Click(object sender, RoutedEventArgs e)
        {
            CreateTimeLapse();
        }

        void mnuSave_Click(object sender, RoutedEventArgs e)
        {
            SaveAsImage();
        }

        void mnuSaveTemplate_Click(object sender, RoutedEventArgs e)
        {
            SaveMap();
        }

        void mnuOpenTemplate_Click(object sender, RoutedEventArgs e)
        {
            OpenMap();
        }

        public ImageSource ToImageSource(FrameworkElement obj)
        {
            // Save current canvas transform
            System.Windows.Media.Transform transform = obj.LayoutTransform;

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

        void mnuClear_Click(object sender, RoutedEventArgs e)
        {
            ClearGraphics();
        }

        public void ClearGraphics()
        {
            if (layerList != null)
            {
                layerList.CloseAllLayers();
            }
        }

        void myMap_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            rightClickedPoint = myMap.ScreenToMap(e.GetPosition(myMap));
        }

        public void UnSubscribe()
        {
            if (myMap != null)
            {
                myMap.MouseMove -= new MouseEventHandler(myMap_MouseMove);
            }
            this.myMap = null;
            this.worker = null;
        }

        void myMap_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                Map myMap = (Map)e.Source;
                if (!myMap.IsFocused)
                    myMap.Focus();
                MapPoint wmPoint = myMap.ScreenToMap(e.GetPosition(myMap));
                //MapPoint geoPoint = ESRI.ArcGIS.Client.Bing.Transform.WebMercatorToGeographic(wmPoint);
                if (MouseCoordinatesChanged != null)
                {
                    if (wmPoint != null)
                    {
                        MouseCoordinatesChanged(wmPoint.Y, wmPoint.X);
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private bool InternetAvailable()
        {
            bool retval = false;
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("http://www.google.com");
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                retval = true;
                //this.Dispatcher.Invoke(new DebugDelegate(ShowDebugMessage), retval);
            }
            catch (Exception ex)
            {

            }
            return retval;
        }

        private void ShowDebugMessage(string debugMsg)
        {
            MessageBox.Show(debugMsg);
        }

        private List<KeyValuePair<DateTime, int>> areaChartDataPoints;

        public void OnDateRangeDefined(DateTime start, DateTime end, List<KeyValuePair<DateTime, int>> areaChartDataPoints)
        {
            if (end.Equals(DateTime.MinValue))
            {
                return;
            }

            this.areaChartDataPoints = areaChartDataPoints;

            myMap.TimeExtent = new TimeExtent(start.AddHours(-1), end.AddHours(1));

            slider.MinimumValue = myMap.TimeExtent.Start;
            slider.MaximumValue = myMap.TimeExtent.End;
            slider.Value = new TimeExtent(slider.MinimumValue, slider.MinimumValue.AddHours(2));
            slider.Intervals = TimeSlider.CreateTimeStopsByTimeInterval(new TimeExtent(slider.MinimumValue, slider.MaximumValue), new TimeSpan(1, 0, 0, 0));

            if (layerList.ClusterLayers.Count == 1)
            {
                DateTimeAxis axis = new DateTimeAxis();
                axis.Orientation = AxisOrientation.X;
                axis.Visibility = Visibility.Hidden;
                axis.IntervalType = DateTimeIntervalType.Days;
                axis.FontSize = 0.1;
                areaSeries.IndependentAxis = axis;

                areaSeries.DependentValuePath = "Value";
                areaSeries.IndependentValuePath = "Key";
                areaSeries.ItemsSource = areaChartDataPoints;
                areaSeries.LegendItems.Clear();             
            }
        }

        void areaSeries_Loaded(object sender, RoutedEventArgs e)
        {
            if (areaChartDataPoints != null)
            {
                List<KeyValuePair<DateTime, int>> test = areaChartDataPoints.FindAll(delegate(KeyValuePair<DateTime, int> pair)
                {
                    return pair.Key < slider.Value.End;
                });
                areaSeries.ItemsSource = test;               
            }
        }

        void slider_ValueChanged(object sender, TimeSlider.ValueChangedEventArgs e)
        {
            if (currentTimeVariable != null)
            {
                grdLapse.Visibility = Visibility.Visible;
                grdTimeLapse.Visibility = Visibility.Visible;
                stkTimeLapse.Visibility = Visibility.Visible;
                if (layerList.ClusterLayers.Count == 1)
                {
                    chrtTimeLapse.Visibility = Visibility.Visible;
                    grdTimeLapseBorder.Height = 120;
                }
                else
                {
                    grdTimeLapseBorder.Height = 50;
                }

                txtSliderStartDate.Text = e.NewValue.Start.ToShortDateString();
                txtSliderEndDate.Text = e.NewValue.End.ToShortDateString();

                myMap.TimeExtent = new TimeExtent(e.NewValue.Start, e.NewValue.End);
                if (areaChartDataPoints != null)
                {
                    if (layerList.ClusterLayers.Count == 1)
                    {
                        List<KeyValuePair<DateTime, int>> test = areaChartDataPoints.FindAll(delegate(KeyValuePair<DateTime, int> pair)
                        {
                            return pair.Key < e.NewValue.End;
                        });
                        areaSeries.ItemsSource = test;
                    }
                }
            }
        }

        public void OnRecordSelected(int id)
        {
            if (RecordSelected != null)
            {
                RecordSelected(id);
            }
        }


        public void OpenDefaultMap()
        {
            OpenMap(defaultMapPath);
        }


        public void OpenMap()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Map Template|*.map7";

            if (dlg.ShowDialog().Value)
            {
                OpenMap(dlg.FileName);
            }
        }

        public void SetMap(string filePath)
        {
            this.defaultMapPath = filePath;
        }

        public void OpenMap(string filePath)
        {
            ClearGraphics();
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.Load(filePath);
            Stack<System.Xml.XmlElement> elementStack = new Stack<System.Xml.XmlElement>();

            foreach (System.Xml.XmlElement element in doc.DocumentElement.ChildNodes)
            {
                elementStack.Push(element);
            }
            while (elementStack.Count > 0)
            {
                System.Xml.XmlElement element = elementStack.Pop();
                if (element.Name.Equals("referenceLayer"))
                {
                    ILayerProperties layerProperties = (ILayerProperties)Activator.CreateInstance(Type.GetType(element.Attributes["layerType"].Value), new object[] { myMap });
                    layerProperties.MakeReadOnly();
                    layerProperties.CreateFromXml(element);
                    layerList.AddListItem(layerProperties, 0);
                }
                if (element.Name.Equals("graphicsLayer"))
                {
                    ILayerProperties layerProperties = (ILayerProperties)Activator.CreateInstance(Type.GetType(element.Attributes["layerType"].Value), new object[] { myMap, new MapPoint(double.Parse(element.Attributes["locationX"].Value), double.Parse(element.Attributes["locationY"].Value)) });
                    layerProperties.MakeReadOnly();
                    layerProperties.CreateFromXml(element);
                    layerList.AddListItem(layerProperties, 0);
                }
                if (element.Name.Equals("dataLayer"))
                {
                    DashboardHelper helper = new DashboardHelper();
                    string dataLayerConn = string.Empty;
                    string dataLayerTable = string.Empty;

                    foreach (System.Xml.XmlElement child in element.ChildNodes)
                    {
                        if (child.Name.Equals("dashboardHelper"))
                        {
                            try
                            {
                                helper.CreateFromXml(child);
                                helper.PopulateDataSet();
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



                        }
                    }

                    ILayerProperties layerProperties = (ILayerProperties)Activator.CreateInstance(Type.GetType(element.Attributes["layerType"].Value), new object[] { myMap, helper, this });
                    layerProperties.MakeReadOnly();
                    layerProperties.FilterRequested += new EventHandler(ILayerProperties_FilterRequested);
                    layerProperties.MapGenerated += new EventHandler(ILayerProperties_MapGenerated);
                    layerProperties.EditRequested += new EventHandler(ILayerProperties_EditRequested);
                    layerProperties.CreateFromXml(element);
                    layerList.AddListItem(layerProperties, 0);
                }
            }
            if (doc.DocumentElement.Attributes.Count > 0)
            {
                string baseMapType = doc.DocumentElement.Attributes["baseMapType"].Value;
                if (baseMapType.Equals("street"))
                {
                    ToggleStreet();
                }
                else if (baseMapType.Equals("satellite"))
                {
                    ToggleSatellite();
                }
                else
                {
                    ToggleBlank();
                }
            }
            SetBackgroundImageType();
        }

        public void SaveMap()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".map7";
            dlg.Filter = "Epi Info 7 Map File|*.map7";

            if (dlg.ShowDialog().Value)
            {
                System.Xml.XmlElement element = layerList.SerializeLayers();
                System.Xml.XmlAttribute baseMapType = element.OwnerDocument.CreateAttribute("baseMapType");
                if (ImageryRadioButton.Visibility == System.Windows.Visibility.Collapsed)
                {
                    baseMapType.Value = "satellite";
                }
                else if (StreetsRadioButton.Visibility == System.Windows.Visibility.Collapsed)
                {
                    baseMapType.Value = "street";
                }
                else
                {
                    baseMapType.Value = "blank";
                }
                element.Attributes.Append(baseMapType);

                System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(dlg.FileName, null);
                element.WriteTo(writer);
                writer.Close();
                MessageBox.Show(DashboardSharedStrings.MAP_SAVED, DashboardSharedStrings.SAVE_SUCCESSFUL, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void SaveAsImage()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG Image (.png)|*.png";

            if (dlg.ShowDialog().Value)
            {
                layerList.Visibility = System.Windows.Visibility.Collapsed;
                nav.Visibility = Visibility.Collapsed;

                BitmapSource img = (BitmapSource)ToImageSource(grdMapDef);

                FileStream stream = new FileStream(dlg.FileName, FileMode.Create);
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(img));
                encoder.Save(stream);
                stream.Close();

                layerList.Visibility = System.Windows.Visibility.Visible;
                nav.Visibility = System.Windows.Visibility.Visible;
                MessageBox.Show(DashboardSharedStrings.MAP_IMAGE_SAVED, DashboardSharedStrings.SAVE_SUCCESSFUL, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void AddShapeFileLayer()
        {
            ShapeLayerProperties layerProperties = new ShapeLayerProperties(myMap);
            layerProperties.MapGenerated += new EventHandler(ILayerProperties_MapGenerated);
            layerProperties.AddShapeFile();
        }

        public void AddMapServerLayer()
        {
            MapServerLayerProperties layerProperties = new MapServerLayerProperties(myMap);
            layerProperties.MapGenerated += new EventHandler(ILayerProperties_MapGenerated);
            layerProperties.AddServerImage();
        }

        public void AddKmlLayer()
        {
            KmlLayerProperties layerProperties = new KmlLayerProperties(myMap);
            layerProperties.MapGenerated += new EventHandler(ILayerProperties_MapGenerated);
            layerProperties.AddServerImage();
        }

        public void GenerateCaseCluster()
        {
            AddLayer(LayerType.CaseCluster);
        }

        private DashboardPopup popup;

        public void GeneratePointofInterestMap(PointLayerProperties pointlayerprop)
        {
            ILayerProperties layerProperties = null;
            DashboardHelper dashboardHelper ;

            popup = new DashboardPopup();
            popup.Parent = LayoutRoot;
                        
            //old config
            layerProperties = (PointLayerProperties) pointlayerprop;
         
            layerProperties.MapGenerated += new EventHandler(ILayerProperties_MapGenerated);
            layerProperties.FilterRequested += new EventHandler(ILayerProperties_FilterRequested);
            layerProperties.EditRequested += new EventHandler(ILayerProperties_EditRequested);
            pointofinterestproperties = new EpiDashboard.Controls.PointofInterestProperties(this, myMap, (PointLayerProperties) layerProperties);
          
            dashboardHelper = pointlayerprop.GetDashboardHelper();
            pointofinterestproperties.SetDashboardHelper(dashboardHelper);
            pointofinterestproperties.txtProjectPath.Text = dashboardHelper.Database.DbName;
            pointofinterestproperties.FillComboBoxes();
            pointofinterestproperties.SetFilter();

            pointofinterestproperties.txtDescription.Text = pointlayerprop.txtDescription.Text;
            pointofinterestproperties.cmbLatitude.Text = pointlayerprop.cbxLatitude.Text;
            pointofinterestproperties.cmbLongitude.Text = pointlayerprop.cbxLongitude.Text;
            pointofinterestproperties.cmbStyle.Text = pointlayerprop.cbxStyle.Text;
            pointofinterestproperties.rctSelectColor.Fill = pointlayerprop.rctColor.Fill;
            pointofinterestproperties.ColorSelected = pointlayerprop.rctColor.Fill;

            pointlayerprop.FlagRunEdit = true;
                        
            pointofinterestproperties.Width = 800;
            pointofinterestproperties.Height = 600;

            if ((System.Windows.SystemParameters.PrimaryScreenWidth / 1.2) > pointofinterestproperties.Width)
            {
                pointofinterestproperties.Width = (System.Windows.SystemParameters.PrimaryScreenWidth / 1.2);
            }

            if ((System.Windows.SystemParameters.PrimaryScreenHeight / 1.2) > pointofinterestproperties.Height)
            {
                pointofinterestproperties.Height = (System.Windows.SystemParameters.PrimaryScreenHeight / 1.2);
            }

            pointofinterestproperties.Cancelled += new EventHandler(properties_Cancelled);
            pointofinterestproperties.ChangesAccepted += new EventHandler(properties_ChangesAccepted);
           
            popup.Content = pointofinterestproperties;
            popup.Show();
        }

        public void GeneratePointofInterestMap()
        {
           ILayerProperties layerProperties = null;
           DashboardHelper dashboardHelper = new DashboardHelper();

           popup = new DashboardPopup();
           popup.Parent = LayoutRoot;
          
            //old config
            layerProperties = new PointLayerProperties(myMap, dashboardHelper, this);
            layerProperties.MapGenerated += new EventHandler(ILayerProperties_MapGenerated);
            layerProperties.FilterRequested += new EventHandler(ILayerProperties_FilterRequested);
            layerProperties.EditRequested += new EventHandler(ILayerProperties_EditRequested);
            pointofinterestproperties = new EpiDashboard.Controls.PointofInterestProperties(this, myMap, (PointLayerProperties) layerProperties);
            pointofinterestproperties.MapGenerated += new EventHandler(ILayerProperties_MapGenerated);
            
            //pointofinterestproperties.Width = 800;
            //pointofinterestproperties.Height = 600;
            if (ResizedWidth != 0 & ResizedHeight != 0)
            {
                double i_StandardHeight = System.Windows.SystemParameters.PrimaryScreenHeight;//Developer Desktop Width Where the Form is Designed
                double i_StandardWidth = System.Windows.SystemParameters.PrimaryScreenWidth; ////Developer Desktop Height Where the Form is Designed
                float f_HeightRatio = new float();
                float f_WidthRatio = new float();
                f_HeightRatio = (float)((float)ResizedHeight / (float)i_StandardHeight);
                f_WidthRatio = (float)((float)ResizedWidth / (float)i_StandardWidth);

                pointofinterestproperties.Height = (Convert.ToInt32(i_StandardHeight * f_HeightRatio)) / 1.07;
                pointofinterestproperties.Width = (Convert.ToInt32(i_StandardWidth * f_WidthRatio)) / 1.07;

            }
            else
            {
                pointofinterestproperties.Width = (System.Windows.SystemParameters.PrimaryScreenWidth / 1.07);
                pointofinterestproperties.Height = (System.Windows.SystemParameters.PrimaryScreenHeight / 1.15);
            }


            pointofinterestproperties.Cancelled += new EventHandler(properties_Cancelled);
            pointofinterestproperties.ChangesAccepted += new EventHandler(properties_ChangesAccepted);
            grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
            popup.Content = pointofinterestproperties;
            popup.Show();
        }

        public void GenerateCaseClusterMap()
        {
            ILayerProperties layerProperties = null;
            DashboardHelper dashboardHelper = new DashboardHelper();

            popup = new DashboardPopup();
            popup.Parent = LayoutRoot;
                        
            //old config
            layerProperties = new ClusterLayerProperties(myMap, dashboardHelper, this);
            layerProperties.MapGenerated += new EventHandler(ILayerProperties_MapGenerated);
            layerProperties.FilterRequested += new EventHandler(ILayerProperties_FilterRequested);
            layerProperties.EditRequested += new EventHandler(ILayerProperties_EditRequested);

            EpiDashboard.Controls.CaseClusterProperties caseclusterproperties = new EpiDashboard.Controls.CaseClusterProperties(this, myMap, (ClusterLayerProperties)layerProperties);
            caseclusterproperties.layerprop = (ClusterLayerProperties)layerProperties;
                       
            //caseclusterproperties.Width = 800;
            //caseclusterproperties.Height = 600;

            if (ResizedWidth != 0 & ResizedHeight != 0)
            {
                double i_StandardHeight = System.Windows.SystemParameters.PrimaryScreenHeight;//Developer Desktop Width Where the Form is Designed
                double i_StandardWidth = System.Windows.SystemParameters.PrimaryScreenWidth; ////Developer Desktop Height Where the Form is Designed
                float f_HeightRatio = new float();
                float f_WidthRatio = new float();
                f_HeightRatio = (float)((float)ResizedHeight / (float)i_StandardHeight);
                f_WidthRatio = (float)((float)ResizedWidth / (float)i_StandardWidth);

                caseclusterproperties.Height = (Convert.ToInt32(i_StandardHeight * f_HeightRatio)) / 1.07;
                caseclusterproperties.Width = (Convert.ToInt32(i_StandardWidth * f_WidthRatio)) / 1.07;

            }
            else
            {
                caseclusterproperties.Width = (System.Windows.SystemParameters.PrimaryScreenWidth / 1.07);
                caseclusterproperties.Height = (System.Windows.SystemParameters.PrimaryScreenHeight / 1.15);
            }

            caseclusterproperties.Cancelled += new EventHandler(properties_Cancelled);
            caseclusterproperties.ChangesAccepted += new EventHandler(properties_ChangesAccepted);
            grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
            popup.Content = caseclusterproperties;
            popup.Show();
        }

        public void GenerateCaseClusterMap(ClusterLayerProperties clusterlayerprop)
        {
            ILayerProperties layerProperties = null;
            DashboardHelper dashboardHelper;

            popup = new DashboardPopup();
            popup.Parent = LayoutRoot;

          
            //old config
            layerProperties = (ClusterLayerProperties)clusterlayerprop;
            layerProperties.MapGenerated += new EventHandler(ILayerProperties_MapGenerated);
            layerProperties.FilterRequested += new EventHandler(ILayerProperties_FilterRequested);
            layerProperties.EditRequested += new EventHandler(ILayerProperties_EditRequested);

            caseclusterproperties = new EpiDashboard.Controls.CaseClusterProperties(this, myMap, (ClusterLayerProperties) layerProperties);
            caseclusterproperties.layerprop = (ClusterLayerProperties)layerProperties;

            dashboardHelper = clusterlayerprop.GetDashboardHelper();
            caseclusterproperties.SetDashboardHelper(dashboardHelper);
            caseclusterproperties.txtProjectPath.Text = dashboardHelper.Database.DbName;
            caseclusterproperties.FillComboBoxes();
            caseclusterproperties.SetFilter();

            caseclusterproperties.txtDescription.Text = clusterlayerprop.txtDescription.Text;
            caseclusterproperties.cmbLatitude.Text = clusterlayerprop.cbxLatitude.Text;
            caseclusterproperties.cmbLongitude.Text = clusterlayerprop.cbxLongitude.Text;
            caseclusterproperties.rctSelectColor.Fill = clusterlayerprop.rctColor.Fill;
            caseclusterproperties.ColorSelected = clusterlayerprop.rctColor.Fill;

            clusterlayerprop.FlagRunEdit = true;
            
            caseclusterproperties.Width = 800;
            caseclusterproperties.Height = 600;

            if ((System.Windows.SystemParameters.PrimaryScreenWidth / 1.2) > caseclusterproperties.Width)
            {
                caseclusterproperties.Width = (System.Windows.SystemParameters.PrimaryScreenWidth / 1.2);
            }

            if ((System.Windows.SystemParameters.PrimaryScreenHeight / 1.2) > caseclusterproperties.Height)
            {
                caseclusterproperties.Height = (System.Windows.SystemParameters.PrimaryScreenHeight / 1.2);
            }

            caseclusterproperties.Cancelled += new EventHandler(properties_Cancelled);
            caseclusterproperties.ChangesAccepted += new EventHandler(properties_ChangesAccepted);
            popup.Content = caseclusterproperties;
            popup.Show();
        }

        //--


        public void GenerateShapeFileChoropleth()
        {
            ILayerProperties layerProperties = null; 
              
            //old config
            DashboardHelper dashboardHelper = new DashboardHelper();
            layerProperties = new ChoroplethShapeLayerProperties(myMap, dashboardHelper, this);
            layerProperties.MapGenerated += new EventHandler(ILayerProperties_MapGenerated);
            layerProperties.FilterRequested += new EventHandler(ILayerProperties_FilterRequested);
            layerProperties.EditRequested += new EventHandler(ILayerProperties_EditRequested);
            
            popup = new DashboardPopup();
            popup.Parent = LayoutRoot;
            EpiDashboard.Controls.ChoroplethProperties properties = new EpiDashboard.Controls.ChoroplethProperties(this, myMap);
            properties.choroplethShapeLayerProperties = (ChoroplethShapeLayerProperties)layerProperties;


            //properties.Width = 800;
            //properties.Height = 600;

            if (ResizedWidth != 0 & ResizedHeight != 0)
            {
                double i_StandardHeight = System.Windows.SystemParameters.PrimaryScreenHeight;//Developer Desktop Width Where the Form is Designed
                double i_StandardWidth = System.Windows.SystemParameters.PrimaryScreenWidth; ////Developer Desktop Height Where the Form is Designed
                float f_HeightRatio = new float();
                float f_WidthRatio = new float();
                f_HeightRatio = (float)((float)ResizedHeight / (float)i_StandardHeight);
                f_WidthRatio = (float)((float)ResizedWidth / (float)i_StandardWidth);

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
            grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
            popup.Content = properties;
            popup.Show();
        }
        public void GenerateShapeFileChoropleth(ChoroplethShapeLayerProperties choroplethlayerprop)
        {

            DashboardHelper dashboardHelper;
            ILayerProperties layerProperties = null;

            popup = new DashboardPopup();
            popup.Parent = LayoutRoot;
            
            //old config
            layerProperties = (ChoroplethShapeLayerProperties)choroplethlayerprop;
            layerProperties.MapGenerated += new EventHandler(ILayerProperties_MapGenerated);
            layerProperties.FilterRequested += new EventHandler(ILayerProperties_FilterRequested);
            layerProperties.EditRequested += new EventHandler(ILayerProperties_EditRequested);

            
            choroplethproperties = new EpiDashboard.Controls.ChoroplethProperties(this, myMap);
            choroplethproperties.choroplethShapeLayerProperties = choroplethlayerprop;
           
            choroplethproperties.Width = 800;
            choroplethproperties.Height = 600;

            dashboardHelper = choroplethlayerprop.GetDashboardHelper();
            choroplethproperties.SetDashboardHelper(dashboardHelper);
            choroplethproperties.txtProjectPath.Text = dashboardHelper.Database.DataSource;
            choroplethproperties.cmbClasses.Text = choroplethlayerprop.cbxClasses.Text;
            choroplethproperties.quintilesOption.IsChecked = choroplethlayerprop.flagQuantiles;
            if (choroplethlayerprop.flagQuantiles == true) { choroplethproperties.OnQuintileOptionChanged(); }
            choroplethproperties.ClearonMapServer();
            //  choroplethproperties.choroplethShapeLayerProvider = choroplethlayerprop.provider;
            choroplethproperties.thisProvider = choroplethlayerprop.provider;    

             choroplethproperties.radKML.IsEnabled = false;
             choroplethproperties.radMapServer.IsEnabled = false;
             choroplethproperties.radconnectmapserver.IsEnabled = false;
             choroplethproperties.radlocatemapserver.IsEnabled = false;
             choroplethproperties.radlocatemapserver.IsEnabled = false;
              choroplethproperties.panelBoundaries.IsEnabled = true;

              if (choroplethlayerprop.datafilters != null)
                 choroplethproperties.datafilters = choroplethlayerprop.datafilters;
              else
                 choroplethproperties.datafilters = dashboardHelper.DataFilters;

              choroplethproperties.rowFilterControl = new RowFilterControl(dashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, choroplethproperties.datafilters, true);
              choroplethproperties.rowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left; choroplethproperties.rowFilterControl.FillSelectionComboboxes();
              choroplethproperties.panelFilters.Children.Add(choroplethproperties.rowFilterControl);
              //choroplethproperties.txtNote.Text = "Note: Any filters set here are applied to this gadget only."; 

            if (string.IsNullOrEmpty(choroplethlayerprop.shapeFilePath) == false)
            {
                choroplethproperties.txtShapePath.Text = choroplethlayerprop.shapeFilePath;
                choroplethproperties.shapeAttributes = choroplethlayerprop.shapeAttributes;
                choroplethproperties.cmbShapeKey.Items.Clear();
                if (choroplethproperties.shapeAttributes == null)
                {
                    object[] shapeFileProperties = choroplethlayerprop.provider.LoadShapeFile(choroplethlayerprop.shapeFilePath);
                    choroplethproperties.shapeAttributes = (IDictionary<string, object>)shapeFileProperties[1];
                }
                if (choroplethproperties.shapeAttributes != null)
                {
                    foreach (string key in choroplethproperties.shapeAttributes.Keys)
                    { choroplethproperties.cmbShapeKey.Items.Add(key); }
                }
             }    


            if (choroplethlayerprop.classAttribList != null)
             { choroplethproperties.SetClassAttributes(choroplethlayerprop.classAttribList); }
            choroplethproperties.FillComboBoxes();
                       
            choroplethproperties.panelBoundaries.IsEnabled = true;
            choroplethproperties.radShapeFile.IsChecked = true;
            choroplethproperties.radShapeFile.IsEnabled = true;
            choroplethproperties.txtShapePath.IsEnabled = true;
            choroplethproperties.btnBrowse.IsEnabled = true;
            choroplethproperties.panelshape.IsEnabled = true;
            
            choroplethproperties.cmbShapeKey.Text = choroplethlayerprop.cbxShapeKey.Text;
            choroplethproperties.cmbDataKey.Text = choroplethlayerprop.cbxDataKey.Text;
            choroplethproperties.cmbValue.Text = choroplethlayerprop.cbxValue.Text;
            choroplethproperties.rctHighColor.Fill = choroplethlayerprop.rctHighColor.Fill;
            choroplethproperties.rctLowColor.Fill = choroplethlayerprop.rctLowColor.Fill;
            choroplethproperties.rctMissingColor.Fill = choroplethlayerprop.rctMissingColor.Fill;
                   
            choroplethlayerprop.FlagRunEdit = true;

            if ((System.Windows.SystemParameters.PrimaryScreenWidth / 1.2) > choroplethproperties.Width)
            {
                choroplethproperties.Width = (System.Windows.SystemParameters.PrimaryScreenWidth / 1.2);
            }

            if ((System.Windows.SystemParameters.PrimaryScreenHeight / 1.2) > choroplethproperties.Height)
            {
                choroplethproperties.Height = (System.Windows.SystemParameters.PrimaryScreenHeight / 1.2);
            }

            choroplethproperties.Cancelled += new EventHandler(properties_Cancelled);
            choroplethproperties.ChangesAccepted += new EventHandler(properties_ChangesAccepted);


            choroplethproperties.legTitle.Text = choroplethlayerprop.provider.LegendText;    



            popup.Content = choroplethproperties;
            popup.Show();
         }
        public void GenerateShapeFileChoropleth(ChoroplethServerLayerProperties choroplethServerlayerprop)
        {

            DashboardHelper dashboardHelper;
            ILayerProperties layerProperties = null;

            popup = new DashboardPopup();
            popup.Parent = LayoutRoot;

            //old config
            layerProperties = (ChoroplethServerLayerProperties)choroplethServerlayerprop;
            layerProperties.MapGenerated += new EventHandler(ILayerProperties_MapGenerated);
            layerProperties.FilterRequested += new EventHandler(ILayerProperties_FilterRequested);
            layerProperties.EditRequested += new EventHandler(ILayerProperties_EditRequested);
                       
            choroplethproperties = new EpiDashboard.Controls.ChoroplethProperties(this, myMap);
            choroplethproperties.choroplethServerLayerProperties = choroplethServerlayerprop;
            choroplethproperties.choroplethServerLayerProvider = choroplethServerlayerprop.provider;
            choroplethproperties.choroplethServerLayerProvider.CloseLayer();

            choroplethproperties.Width = 800;
            choroplethproperties.Height = 600;

            dashboardHelper = choroplethServerlayerprop.GetDashboardHelper();
            choroplethproperties.SetDashboardHelper(dashboardHelper);
            choroplethproperties.txtProjectPath.Text = dashboardHelper.Database.DataSource;

            choroplethproperties.cmbClasses.Text = choroplethServerlayerprop.cbxClasses.Text;
            choroplethproperties.quintilesOption.IsChecked = choroplethServerlayerprop.flagQuantiles;
            if (choroplethServerlayerprop.flagQuantiles == true) { choroplethproperties.OnQuintileOptionChanged(); }
            choroplethproperties.ClearonShapeFile();

            choroplethproperties.radShapeFile.IsEnabled = false;
            choroplethproperties.radKML.IsEnabled = false;

            choroplethproperties.panelBoundaries.IsEnabled = true;
            if (choroplethServerlayerprop.datafilters != null)
                choroplethproperties.datafilters = choroplethServerlayerprop.datafilters;
            else
                choroplethproperties.datafilters = dashboardHelper.DataFilters;

            choroplethproperties.rowFilterControl = new RowFilterControl(dashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, choroplethproperties.datafilters, true);
            choroplethproperties.rowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left; choroplethproperties.rowFilterControl.FillSelectionComboboxes();
            choroplethproperties.panelFilters.Children.Add(choroplethproperties.rowFilterControl);
            //choroplethproperties.txtNote.Text = "Note: Any filters set here are applied to this gadget only.";

            if (string.IsNullOrEmpty(choroplethServerlayerprop.shapeFilePath) == false)
            {
                choroplethproperties.radMapServer.IsChecked = true;
                if (choroplethServerlayerprop.shapeFilePath.ToLower() == "http://services.nationalmap.gov/arcgis/rest/services/govunits/mapserver/13" && string.IsNullOrEmpty(choroplethServerlayerprop.cbxMapFeatureText) == true)
                    choroplethServerlayerprop.cbxMapserverText = "NationalMap.gov - New York County Boundaries";
                else if (choroplethServerlayerprop.shapeFilePath.ToLower() == "http://services.nationalmap.gov/arcgis/rest/services/govunits/mapserver/19" && string.IsNullOrEmpty(choroplethServerlayerprop.cbxMapFeatureText) == true)
                    choroplethServerlayerprop.cbxMapserverText = "NationalMap.gov - Rhode Island Zip Code Boundaries";
                else if (choroplethServerlayerprop.shapeFilePath.ToLower() == "http://services.nationalmap.gov/arcgis/rest/services/govunits/mapserver/17" && string.IsNullOrEmpty(choroplethServerlayerprop.cbxMapFeatureText) == true)
                    choroplethServerlayerprop.cbxMapserverText = "NationalMap.gov - U.S. State Boundaries";
                else if (choroplethServerlayerprop.shapeFilePath.ToLower() == "http://services.nationalmap.gov/arcgis/rest/services/tnm_blank_us/mapserver/17" && string.IsNullOrEmpty(choroplethServerlayerprop.cbxMapFeatureText) == true)
                    choroplethServerlayerprop.cbxMapserverText = "NationalMap.gov - World Boundaries";
                else
                {
                    string lastchar = choroplethServerlayerprop.shapeFilePath.Substring(choroplethServerlayerprop.shapeFilePath.Length - 1, 1);
                    string lastonebeforechar = choroplethServerlayerprop.shapeFilePath.Substring(choroplethServerlayerprop.shapeFilePath.Length - 2, 1);
                    if (char.IsNumber(lastchar, 0) == true && char.IsNumber(lastonebeforechar,0) == false)
                        choroplethServerlayerprop.txtMapserverText = choroplethServerlayerprop.shapeFilePath.Substring(0, choroplethServerlayerprop.shapeFilePath.Length - 2);
                    else if (char.IsNumber(lastchar, 0) == true && char.IsNumber(lastonebeforechar, 0) == true)
                        choroplethServerlayerprop.txtMapserverText = choroplethServerlayerprop.shapeFilePath.Substring(0, choroplethServerlayerprop.shapeFilePath.Length - 3);
                } 

               
                if (string.IsNullOrEmpty(choroplethServerlayerprop.cbxMapserverText) == false)
                {
                    choroplethproperties.radconnectmapserver.IsChecked = true;
                    choroplethproperties.cbxmapserver.SelectionChanged -= choroplethproperties.cbxmapserver_SelectionChanged;
                    choroplethproperties.cbxmapserver.Text = choroplethServerlayerprop.cbxMapserverText;
                    choroplethproperties.ResetMapServer();
                    choroplethproperties.cbxmapserver.SelectionChanged += choroplethproperties.cbxmapserver_SelectionChanged;
                }
                else
                {
                    choroplethproperties.radlocatemapserver.IsChecked = true;
                    choroplethproperties.txtMapSeverpath.Text = choroplethServerlayerprop.txtMapserverText;
                    choroplethproperties.MapServerConnect();
                    choroplethproperties.cbxmapfeature.SelectionChanged -= choroplethproperties.cbxmapfeature_SelectionChanged;
                    choroplethproperties.cbxmapfeature.IsEditable = true;
                    choroplethproperties.cbxmapfeature.Text = choroplethServerlayerprop.cbxMapFeatureText;
                    int Selectedindex = -1;
                    for (int i = 0; i < choroplethproperties.cbxmapfeature.Items.Count; i++)
                    {
                        if (choroplethproperties.cbxmapfeature.Items[i].ToString() == choroplethServerlayerprop.cbxMapFeatureText)
                        { Selectedindex = i; break; }
                    }
                    choroplethproperties.cbxmapfeature.SelectedIndex = Selectedindex;
                    choroplethproperties.MapfeatureSelectionChange();
                    choroplethproperties.cbxmapfeature.SelectionChanged += choroplethproperties.cbxmapfeature_SelectionChanged;
                  }
              }
            
            if (choroplethServerlayerprop.classAttribList != null)
            { 
                choroplethproperties.SetClassAttributes(choroplethServerlayerprop.classAttribList); 
            }
            
            choroplethproperties.FillComboBoxes();

            choroplethproperties.cmbShapeKey.SelectedItem = choroplethServerlayerprop.cbxShapeKey.Text;
            choroplethproperties.cmbDataKey.Text = choroplethServerlayerprop.cbxDataKey.Text;
            choroplethproperties.cmbValue.Text = choroplethServerlayerprop.cbxValue.Text;
            choroplethproperties.rctHighColor.Fill = choroplethServerlayerprop.rctHighColor.Fill;
            choroplethproperties.rctLowColor.Fill = choroplethServerlayerprop.rctLowColor.Fill;

            choroplethproperties.rctMissingColor.Fill = choroplethServerlayerprop.rctMissingColor.Fill;
            choroplethServerlayerprop.FlagRunEdit = true;

            if ((System.Windows.SystemParameters.PrimaryScreenWidth / 1.2) > choroplethproperties.Width)
            {
                choroplethproperties.Width = (System.Windows.SystemParameters.PrimaryScreenWidth / 1.2);
            }

            if ((System.Windows.SystemParameters.PrimaryScreenHeight / 1.2) > choroplethproperties.Height)
            {
                choroplethproperties.Height = (System.Windows.SystemParameters.PrimaryScreenHeight / 1.2);
            }

            choroplethproperties.Cancelled += new EventHandler(properties_Cancelled);
            choroplethproperties.ChangesAccepted += new EventHandler(properties_ChangesAccepted);
            choroplethproperties.cmbShapeKey.SelectionChanged += new SelectionChangedEventHandler(choroplethproperties.cmbShapeKey_SelectionChanged);
            
            popup.Content = choroplethproperties;
            popup.Show();
        }

        public void GenerateShapeFileChoropleth(ChoroplethKmlLayerProperties choroplethKMLlayerprop)
        {

            DashboardHelper dashboardHelper;
            ILayerProperties layerProperties = null;

            popup = new DashboardPopup();
            popup.Parent = LayoutRoot;

            //old config
            layerProperties = (ChoroplethKmlLayerProperties)choroplethKMLlayerprop;
            layerProperties.MapGenerated += new EventHandler(ILayerProperties_MapGenerated);
            layerProperties.FilterRequested += new EventHandler(ILayerProperties_FilterRequested);
            layerProperties.EditRequested += new EventHandler(ILayerProperties_EditRequested);


            choroplethproperties = new EpiDashboard.Controls.ChoroplethProperties(this, myMap);
            choroplethproperties.choroplethKmlLayerProperties = choroplethKMLlayerprop;
            choroplethproperties.choroplethKmlLayerProvider = choroplethKMLlayerprop.provider;

            choroplethproperties.Width = 800;
            choroplethproperties.Height = 600;

            dashboardHelper = choroplethKMLlayerprop.GetDashboardHelper();
            choroplethproperties.SetDashboardHelper(dashboardHelper);
            choroplethproperties.txtProjectPath.Text = dashboardHelper.Database.DataSource;
            choroplethproperties.cmbClasses.Text = choroplethKMLlayerprop.cbxClasses.Text;
            choroplethproperties.quintilesOption.IsChecked = choroplethKMLlayerprop.flagQuantiles;
            if (choroplethKMLlayerprop.flagQuantiles == true) { choroplethproperties.OnQuintileOptionChanged(); }
            //choroplethproperties.ClearonMapServer();

            choroplethproperties.panelBoundaries.IsEnabled = true;
            if (choroplethKMLlayerprop.datafilters != null)
                choroplethproperties.datafilters = choroplethKMLlayerprop.datafilters;
            else
                choroplethproperties.datafilters = dashboardHelper.DataFilters;

            choroplethproperties.rowFilterControl = new RowFilterControl(dashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, choroplethproperties.datafilters, true);
            choroplethproperties.rowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left; choroplethproperties.rowFilterControl.FillSelectionComboboxes();
            choroplethproperties.panelFilters.Children.Add(choroplethproperties.rowFilterControl);
            //choroplethproperties.txtNote.Text = "Note: Any filters set here are applied to this gadget only.";

            choroplethproperties.choroplethKmlLayerProperties = choroplethKMLlayerprop;
            choroplethproperties.choroplethKmlLayerProvider = choroplethproperties.choroplethKmlLayerProperties.provider;
            if (string.IsNullOrEmpty(choroplethKMLlayerprop.shapeFilePath) == false)
            {
                choroplethproperties.radKML.IsChecked = true;
                choroplethproperties.txtKMLpath.Text = choroplethKMLlayerprop.shapeFilePath;
                if (choroplethproperties.choroplethKmlLayerProperties.curfeatureAttributes != null)
                {
                    foreach (string key in choroplethproperties.choroplethKmlLayerProperties.curfeatureAttributes.Keys)
                    { choroplethproperties.cmbShapeKey.Items.Add(key); }
                }
            }
            choroplethproperties.radShapeFile.IsEnabled = false;
            choroplethproperties.radMapServer.IsEnabled = false;

            if (choroplethKMLlayerprop.classAttribList != null)
            { choroplethproperties.SetClassAttributes(choroplethKMLlayerprop.classAttribList); }

            choroplethproperties.FillComboBoxes();
            choroplethproperties.cmbShapeKey.Text = choroplethKMLlayerprop.cbxShapeKey.Text;
            choroplethproperties.cmbDataKey.Text = choroplethKMLlayerprop.cbxDataKey.Text;
            choroplethproperties.cmbValue.Text = choroplethKMLlayerprop.cbxValue.Text;
            choroplethproperties.rctHighColor.Fill = choroplethKMLlayerprop.rctHighColor.Fill;
            choroplethproperties.rctLowColor.Fill = choroplethKMLlayerprop.rctLowColor.Fill;
            choroplethproperties.rctMissingColor.Fill = choroplethKMLlayerprop.rctMissingColor.Fill;

            choroplethKMLlayerprop.FlagRunEdit = true;

            if ((System.Windows.SystemParameters.PrimaryScreenWidth / 1.2) > choroplethproperties.Width)
            {
                choroplethproperties.Width = (System.Windows.SystemParameters.PrimaryScreenWidth / 1.2);
            }

            if ((System.Windows.SystemParameters.PrimaryScreenHeight / 1.2) > choroplethproperties.Height)
            {
                choroplethproperties.Height = (System.Windows.SystemParameters.PrimaryScreenHeight / 1.2);
            }

            choroplethproperties.Cancelled += new EventHandler(properties_Cancelled);
            choroplethproperties.ChangesAccepted += new EventHandler(properties_ChangesAccepted);

            popup.Content = choroplethproperties;
            popup.Show();
            
        }


        void properties_ChangesAccepted(object sender, EventArgs e)
        {
            //DashboardProperties properties = popup.Content as DashboardProperties;

            //if (DashboardHelper.IsUsingEpiProject)
            //{
            //    bool projectPathIsValid = true;
            //    Project tempProject = new Project();
            //    try
            //    {
            //        tempProject = new Project(properties.ProjectFileInfo.FullName);
            //    }
            //    catch
            //    {
            //        projectPathIsValid = false;
            //    }

            //    if (!projectPathIsValid)
            //    {
            //        //pnlError.Visibility = System.Windows.Visibility.Visible;
            //        //txtError.Text = "Error: The specified project path is not valid. Settings were not saved.";
            //        return;
            //    }

            //    if (!tempProject.Views.Contains(properties.FormName))
            //    {
            //        //pnlError.Visibility = System.Windows.Visibility.Visible;
            //        //txtError.Text = "Error: Form not found in project. Settings were not saved.";
            //        return;
            //    }

            //    if (DashboardHelper.IsUsingEpiProject)
            //    {
            //        //if (cmbTableName.Text.ToLower() == dashboardHelper.View.Name.ToLower() && dashboardHelper.View.Project.FilePath == txtProjectPath.Text)
            //        //{
            //        //    pnlWarning.Visibility = System.Windows.Visibility.Collapsed;
            //        //    txtWarning.Text = string.Empty;
            //        //}
            //    }

            //    DashboardHelper.ResetView(tempProject.Views[properties.FormName], false);
            //}
            //else if (!string.IsNullOrEmpty(properties.SqlQuery))
            //{
            //    DashboardHelper.SetCustomQuery(properties.SqlQuery);
            //}

            //this.CustomOutputConclusionText = properties.Conclusion;
            //this.CustomOutputHeading = properties.Title;
            //this.CustomOutputSummaryText = properties.Summary;
            //this.UseAlternatingColorsInOutput = properties.UseAlternatingColors;
            //this.ShowCanvasSummaryInfoInOutput = properties.ShowCanvasSummary;
            //this.ShowGadgetHeadingsInOutput = properties.ShowGadgetHeadings;
            //this.ShowGadgetSettingsInOutput = properties.ShowGadgetSettings;
            //this.SortGadgetsTopToBottom = properties.UseTopToBottomGadgetOrdering;

            //if (properties.DefaultChartWidth.HasValue)
            //{
            //    DefaultChartWidth = properties.DefaultChartWidth.Value;
            //}

            //if (properties.DefaultChartHeight.HasValue)
            //{
            //    DefaultChartHeight = properties.DefaultChartHeight.Value;
            //}

            popup.Close();
              
        }

        void properties_Cancelled(object sender, EventArgs e)
        {
            popup.Close();
        }

        public void GenerateMapServerChoropleth()
        {
            AddLayer(LayerType.ChoroplethMapServer);
        }

        public void GenerateKmlChoropleth()
        {
            AddLayer(LayerType.ChoroplethKml);
        }

        public void GeneratePointMap()
        {
            AddLayer(LayerType.PointMap);
        }

        public void GenerateKmlDotDensity()
        {
            AddLayer(LayerType.DotDensityKml);
        }

        public void GenerateShapeFileDotDensity()
        {
                   
            //ILayerProperties layerProperties = null;
            DashboardHelper dashboardHelper = new DashboardHelper();
            popup = new DashboardPopup();
            popup.Parent = LayoutRoot; 

            /*
            layerProperties = new DotDensityLayerProperties(myMap, dashboardHelper, this);
            layerProperties.MapGenerated += new EventHandler(ILayerProperties_MapGenerated);
            layerProperties.FilterRequested += new EventHandler(ILayerProperties_FilterRequested);
            layerProperties.EditRequested += new EventHandler(ILayerProperties_EditRequested); */
          
            EpiDashboard.Controls.DotDensityProperties properties = new EpiDashboard.Controls.DotDensityProperties(this, myMap);
           // properties.choroplethShapeLayerProperties = (DotDensityLayerProperties)layerProperties;
            properties.MapGenerated += new EventHandler(ILayerProperties_MapGenerated);
            properties.FilterRequested += new EventHandler(ILayerProperties_FilterRequested);
            properties.EditRequested += new EventHandler(ILayerProperties_EditRequested);

            //properties.Width = 800;
            //properties.Height = 600;

            if (ResizedWidth != 0 & ResizedHeight != 0)
            {
                double i_StandardHeight = System.Windows.SystemParameters.PrimaryScreenHeight;//Developer Desktop Width Where the Form is Designed
                double i_StandardWidth = System.Windows.SystemParameters.PrimaryScreenWidth; ////Developer Desktop Height Where the Form is Designed
                float f_HeightRatio = new float();
                float f_WidthRatio = new float();
                f_HeightRatio = (float)((float)ResizedHeight / (float)i_StandardHeight);
                f_WidthRatio = (float)((float)ResizedWidth / (float)i_StandardWidth);

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
                     
            popup.Content = properties;
            popup.Show();
        }

        public void GenerateShapeFileDotDensity(DotDensityLayerProperties densitylayerprop)
        {

            DashboardHelper dashboardHelper;
            popup = new DashboardPopup();
            popup.Parent = LayoutRoot;
            ILayerProperties layerProperties = null;

            layerProperties = (DotDensityLayerProperties)densitylayerprop;
            layerProperties.MapGenerated += new EventHandler(ILayerProperties_MapGenerated);
            layerProperties.FilterRequested += new EventHandler(ILayerProperties_FilterRequested);
            layerProperties.EditRequested += new EventHandler(ILayerProperties_EditRequested);

            dotdensityproperties = new EpiDashboard.Controls.DotDensityProperties(this, myMap);
            dotdensityproperties.layerprop = densitylayerprop;
            dotdensityproperties.provider = densitylayerprop.provider;

            dotdensityproperties.Width = 800;
            dotdensityproperties.Height = 600;

            dashboardHelper = densitylayerprop.GetDashboardHelper();
            dotdensityproperties.SetDashboardHelper(dashboardHelper);
            dotdensityproperties.txtProjectPath.Text = dashboardHelper.Database.DataSource; // dashboardHelper.Database.DbName;
            dotdensityproperties.panelBoundaries.IsEnabled = true;

            if (densitylayerprop.datafilters != null)
                dotdensityproperties.dataFilters = densitylayerprop.datafilters;
            else
                dotdensityproperties.dataFilters = dashboardHelper.DataFilters;

           // dotdensityproperties.dataFilters = dashboardHelper.DataFilters; //new DataFilters(dashboardHelper);
            dotdensityproperties.rowFilterControl = new RowFilterControl(dashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, dotdensityproperties.dataFilters, true);
            dotdensityproperties.rowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left; dotdensityproperties.rowFilterControl.FillSelectionComboboxes();
            dotdensityproperties.panelFilters.Children.Add(dotdensityproperties.rowFilterControl);

            //dotdensityproperties.txtNote.Text = "Note: Any filters set here are applied to this gadget only.";
            if  (string.IsNullOrEmpty(densitylayerprop.shapeFilePath) == false)
            {
                dotdensityproperties.radShapeFile.IsChecked = true;
                dotdensityproperties.txtShapePath.Text = densitylayerprop.shapeFilePath;
                dotdensityproperties.shapeAttributes = densitylayerprop.shapeAttributes;
                dotdensityproperties.cmbShapeKey.Items.Clear();
                if (dotdensityproperties.shapeAttributes == null)
                {
                    object[] shapeFileProperties = densitylayerprop.provider.LoadShapeFile(densitylayerprop.shapeFilePath);
                    dotdensityproperties.shapeAttributes = (IDictionary<string, object>)shapeFileProperties[1];
                }
                if (dotdensityproperties.shapeAttributes != null)
                {
                    foreach (string key in dotdensityproperties.shapeAttributes.Keys)
                    { dotdensityproperties.cmbShapeKey.Items.Add(key); }
                }
            }
           
            dotdensityproperties.FillComboBoxes();
            dotdensityproperties.cmbShapeKey.Text = densitylayerprop.cbxShapeKey.Text;
            dotdensityproperties.cmbDataKey.Text = densitylayerprop.cbxDataKey.Text;
            dotdensityproperties.cmbValue.Text = densitylayerprop.cbxValue.Text;
            dotdensityproperties.rctDotColor.Fill = densitylayerprop.rctDotColor.Fill;
            dotdensityproperties.txtDotValue.Text = densitylayerprop.txtDotValue.Text;

            densitylayerprop.FlagRunEdit = true;

            if ((System.Windows.SystemParameters.PrimaryScreenWidth / 1.2) > dotdensityproperties.Width)
            {
                dotdensityproperties.Width = (System.Windows.SystemParameters.PrimaryScreenWidth / 1.2);
            }

            if ((System.Windows.SystemParameters.PrimaryScreenHeight / 1.2) > dotdensityproperties.Height)
            {
                dotdensityproperties.Height = (System.Windows.SystemParameters.PrimaryScreenHeight / 1.2);
            }

            dotdensityproperties.Cancelled += new EventHandler(properties_Cancelled);
            dotdensityproperties.ChangesAccepted += new EventHandler(properties_ChangesAccepted);

            dotdensityproperties.panelmap.IsEnabled = false;
            dotdensityproperties.panelKml.IsEnabled = false;
            dotdensityproperties.radMapServer.IsEnabled = false;
            dotdensityproperties.radKML.IsEnabled = false;
            popup.Content = dotdensityproperties;
            popup.Show();
        }
        public void GenerateShapeFileDotDensity(DotDensityServerLayerProperties densitylayerprop)
        {

            DashboardHelper dashboardHelper;
            popup = new DashboardPopup();
            popup.Parent = LayoutRoot;
            ILayerProperties layerProperties = null;

            layerProperties = (DotDensityServerLayerProperties)densitylayerprop;
            layerProperties.MapGenerated += new EventHandler(ILayerProperties_MapGenerated);
            layerProperties.FilterRequested += new EventHandler(ILayerProperties_FilterRequested);
            layerProperties.EditRequested += new EventHandler(ILayerProperties_EditRequested);

            dotdensityproperties = new EpiDashboard.Controls.DotDensityProperties(this, myMap);

            dotdensityproperties.Width = 800;
            dotdensityproperties.Height = 600;

            dashboardHelper = densitylayerprop.GetDashboardHelper();
            dotdensityproperties.SetDashboardHelper(dashboardHelper);
           // dotdensityproperties.txtProjectPath.Text = dashboardHelper.Database.DataSource;// dashboardHelper.Database.DbName;
            dotdensityproperties.panelBoundaries.IsEnabled = true;
            if (densitylayerprop.datafilters != null)
                dotdensityproperties.dataFilters = densitylayerprop.datafilters;
            else
                dotdensityproperties.dataFilters = dashboardHelper.DataFilters;
            
            //dotdensityproperties.dataFilters = dashboardHelper.DataFilters; //new DataFilters(dashboardHelper);
            dotdensityproperties.rowFilterControl = new RowFilterControl(dashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, dotdensityproperties.dataFilters, true);
            dotdensityproperties.rowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left; dotdensityproperties.rowFilterControl.FillSelectionComboboxes();
            dotdensityproperties.panelFilters.Children.Add(dotdensityproperties.rowFilterControl);

            //dotdensityproperties.txtNote.Text = "Note: Any filters set here are applied to this gadget only.";

            dotdensityproperties.serverlayerprop = (DotDensityServerLayerProperties)layerProperties;
            dotdensityproperties.Mapprovider = dotdensityproperties.serverlayerprop.provider;
            densitylayerprop.shapeFilePath = densitylayerprop.shapeFilePath.Trim();

            if (string.IsNullOrEmpty (densitylayerprop.shapeFilePath) == false) 
            {
                dotdensityproperties.radMapServer.IsChecked = true;
                if (densitylayerprop.shapeFilePath.ToLower() == "http://services.nationalmap.gov/arcgis/rest/services/govunits/mapserver/13" && string.IsNullOrEmpty(densitylayerprop.cbxMapFeatureText) == true)
                    densitylayerprop.cbxMapserverText = "NationalMap.gov - New York County Boundaries";
                else if (densitylayerprop.shapeFilePath.ToLower() == "http://services.nationalmap.gov/arcgis/rest/services/govunits/mapserver/19" && string.IsNullOrEmpty(densitylayerprop.cbxMapFeatureText) == true)
                    densitylayerprop.cbxMapserverText = "NationalMap.gov - Rhode Island Zip Code Boundaries";
                else if (densitylayerprop.shapeFilePath.ToLower() == "http://services.nationalmap.gov/arcgis/rest/services/govunits/mapserver/17" && string.IsNullOrEmpty(densitylayerprop.cbxMapFeatureText) == true)
                    densitylayerprop.cbxMapserverText = "NationalMap.gov - U.S. State Boundaries";
                else if (densitylayerprop.shapeFilePath.ToLower() == "http://services.nationalmap.gov/arcgis/rest/services/tnm_blank_us/mapserver/17" && string.IsNullOrEmpty(densitylayerprop.cbxMapFeatureText) == true)
                    densitylayerprop.cbxMapserverText = "NationalMap.gov - World Boundaries";
                else
                {
                    string lastchar = densitylayerprop.shapeFilePath.Substring(densitylayerprop.shapeFilePath.Length - 1 , 1);
                    string lastonebeforechar = densitylayerprop.shapeFilePath.Substring(densitylayerprop.shapeFilePath.Length - 2, 1);
                    if (char.IsNumber(lastchar, 0) == true && char.IsNumber(lastonebeforechar, 0) == false)
                        densitylayerprop.txtMapserverText = densitylayerprop.shapeFilePath.Substring(0, densitylayerprop.shapeFilePath.Length - 2);
                    else if (char.IsNumber(lastchar, 0) == true && char.IsNumber(lastonebeforechar, 0) == true)
                        densitylayerprop.txtMapserverText = densitylayerprop.shapeFilePath.Substring(0, densitylayerprop.shapeFilePath.Length - 3);
                }

                if (string.IsNullOrEmpty(densitylayerprop.cbxMapserverText) == false)
                {
                    dotdensityproperties.radconnectmapserver.IsChecked = true;
                    dotdensityproperties.cbxmapserver.SelectionChanged -= dotdensityproperties.cbxmapserver_SelectionChanged;
                    dotdensityproperties.cbxmapserver.Text = densitylayerprop.cbxMapserverText;
                    dotdensityproperties.ResetMapServer();
                    dotdensityproperties.cbxmapserver.SelectionChanged += dotdensityproperties.cbxmapserver_SelectionChanged;
                    //dotdensityproperties.cbxmapserver.Text = densitylayerprop.cbxMapserverText;
                }
                else
                {
                    dotdensityproperties.radlocatemapserver.IsChecked = true;
                    dotdensityproperties.txtMapSeverpath.Text = densitylayerprop.txtMapserverText;
                    dotdensityproperties.MapServerConnect();
                    dotdensityproperties.cbxmapfeature.SelectionChanged -= dotdensityproperties.cbxmapfeature_SelectionChanged;
                    dotdensityproperties.cbxmapfeature.IsEditable = true;
                    dotdensityproperties.cbxmapfeature.Text = densitylayerprop.cbxMapFeatureText;
                    int Selectedindex = -1;
                    for (int i = 0; i < dotdensityproperties.cbxmapfeature.Items.Count; i++)
                    {
                        if (dotdensityproperties.cbxmapfeature.Items[i].ToString() == densitylayerprop.cbxMapFeatureText)
                        { Selectedindex = i; break; }
                    }
                    dotdensityproperties.cbxmapfeature.SelectedIndex = Selectedindex;
                    dotdensityproperties.MapfeatureSelectionChange();
                    dotdensityproperties.cbxmapfeature.SelectionChanged += dotdensityproperties.cbxmapfeature_SelectionChanged;

                   // dotdensityproperties.ResetMapServer();
                   // dotdensityproperties.cbxmapfeature.Text = densitylayerprop.cbxMapFeatureText;
                    
                }
                if (dotdensityproperties.serverlayerprop.curfeatureAttributes != null)
                {
                    foreach (string key in dotdensityproperties.serverlayerprop.curfeatureAttributes.Keys)
                    { dotdensityproperties.cmbShapeKey.Items.Add(key); }
                }
            }
            dotdensityproperties.txtProjectPath.Text = dashboardHelper.Database.DataSource;// dashboardHelper.Database.DbName;
            dotdensityproperties.FillComboBoxes();
            dotdensityproperties.cmbShapeKey.Text = densitylayerprop.cbxShapeKey.Text;
            dotdensityproperties.cmbDataKey.Text = densitylayerprop.cbxDataKey.Text;
            dotdensityproperties.cmbValue.Text = densitylayerprop.cbxValue.Text;
            dotdensityproperties.rctDotColor.Fill = densitylayerprop.rctDotColor.Fill;
            dotdensityproperties.txtDotValue.Text = densitylayerprop.txtDotValue.Text;

            densitylayerprop.FlagRunEdit = true;
           
            if ((System.Windows.SystemParameters.PrimaryScreenWidth / 1.2) > dotdensityproperties.Width)
            {
                dotdensityproperties.Width = (System.Windows.SystemParameters.PrimaryScreenWidth / 1.2);
            }

            if ((System.Windows.SystemParameters.PrimaryScreenHeight / 1.2) > dotdensityproperties.Height)
            {
                dotdensityproperties.Height = (System.Windows.SystemParameters.PrimaryScreenHeight / 1.2);
            }

            dotdensityproperties.Cancelled += new EventHandler(properties_Cancelled);
            dotdensityproperties.ChangesAccepted += new EventHandler(properties_ChangesAccepted);

            dotdensityproperties.panelshape.IsEnabled = false;
            dotdensityproperties.panelKml.IsEnabled = false;
            dotdensityproperties.radShapeFile.IsEnabled = false;
            dotdensityproperties.radKML.IsEnabled = false;
            popup.Content = dotdensityproperties;
            popup.Show();
           
        }
        public void GenerateShapeFileDotDensity(DotDensityKmlLayerProperties densitylayerprop)
        {

            DashboardHelper dashboardHelper;
            popup = new DashboardPopup();
            popup.Parent = LayoutRoot;
            ILayerProperties layerProperties = null;

            layerProperties = (DotDensityKmlLayerProperties)densitylayerprop;
            layerProperties.MapGenerated += new EventHandler(ILayerProperties_MapGenerated);
            layerProperties.FilterRequested += new EventHandler(ILayerProperties_FilterRequested);
            layerProperties.EditRequested += new EventHandler(ILayerProperties_EditRequested);

            dotdensityproperties = new EpiDashboard.Controls.DotDensityProperties(this, myMap);

            dotdensityproperties.Width = 800;
            dotdensityproperties.Height = 600;

            dashboardHelper = densitylayerprop.GetDashboardHelper();
            dotdensityproperties.SetDashboardHelper(dashboardHelper);
            dotdensityproperties.txtProjectPath.Text = dashboardHelper.Database.DataSource; // ProjectFilepath;//Database.DbName;
            dotdensityproperties.panelBoundaries.IsEnabled = true;
            if (densitylayerprop.datafilters != null)
                dotdensityproperties.dataFilters = densitylayerprop.datafilters;
            else
                dotdensityproperties.dataFilters = dashboardHelper.DataFilters;

           // dotdensityproperties.dataFilters = dashboardHelper.DataFilters; //new DataFilters(dashboardHelper);
            dotdensityproperties.rowFilterControl = new RowFilterControl(dashboardHelper, Dialogs.FilterDialogMode.ConditionalMode, dotdensityproperties.dataFilters, true);
            dotdensityproperties.rowFilterControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Left; dotdensityproperties.rowFilterControl.FillSelectionComboboxes();
            dotdensityproperties.panelFilters.Children.Add(dotdensityproperties.rowFilterControl);

            //dotdensityproperties.txtNote.Text = "Note: Any filters set here are applied to this gadget only.";

            dotdensityproperties.kmllayerprop = densitylayerprop;  
            dotdensityproperties.KMLprovider = dotdensityproperties.kmllayerprop.provider;
            if (string.IsNullOrEmpty(densitylayerprop.shapeFilePath) == false)
            {
                dotdensityproperties.radKML.IsChecked = true;
                dotdensityproperties.txtKMLpath.Text = densitylayerprop.shapeFilePath;
                if (dotdensityproperties.kmllayerprop.curfeatureAttributes != null)
                {
                    foreach (string key in dotdensityproperties.kmllayerprop.curfeatureAttributes.Keys)
                    { dotdensityproperties.cmbShapeKey.Items.Add(key); }
                }
            }
            dotdensityproperties.FillComboBoxes();
            dotdensityproperties.cmbShapeKey.Text = densitylayerprop.cbxShapeKey.Text;
            dotdensityproperties.cmbDataKey.Text = densitylayerprop.cbxDataKey.Text;
            dotdensityproperties.cmbValue.Text = densitylayerprop.cbxValue.Text;
            dotdensityproperties.rctDotColor.Fill = densitylayerprop.rctDotColor.Fill;
            dotdensityproperties.txtDotValue.Text = densitylayerprop.txtDotValue.Text;

            densitylayerprop.FlagRunEdit = true;

            if ((System.Windows.SystemParameters.PrimaryScreenWidth / 1.2) > dotdensityproperties.Width)
            {
                dotdensityproperties.Width = (System.Windows.SystemParameters.PrimaryScreenWidth / 1.2);
            }

            if ((System.Windows.SystemParameters.PrimaryScreenHeight / 1.2) > dotdensityproperties.Height)
            {
                dotdensityproperties.Height = (System.Windows.SystemParameters.PrimaryScreenHeight / 1.2);
            }

            dotdensityproperties.Cancelled += new EventHandler(properties_Cancelled);
            dotdensityproperties.ChangesAccepted += new EventHandler(properties_ChangesAccepted);

            dotdensityproperties.panelmap.IsEnabled = false;
            dotdensityproperties.panelshape.IsEnabled = false;
            dotdensityproperties.radMapServer.IsEnabled = false;
            dotdensityproperties.radShapeFile.IsEnabled = false;
            popup.Content = dotdensityproperties;
            popup.Show();

        }


        public void GenerateMapServerDotDensity()
        {
            AddLayer(LayerType.DotDensityMapServer);
        }

        public void CreateTimeLapse()
        {
            List<ILayerProperties> layers = layerList.Layers;
            List<DashboardHelper> dashboardHelpers = new List<DashboardHelper>();
            foreach (ILayerProperties layer in layers)
            {
                if (layer.GetDashboardHelper() != null)
                {
                    dashboardHelpers.Add(layer.GetDashboardHelper());
                }
            }
            TimeConfigDialog dialog = new TimeConfigDialog(dashboardHelpers);
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(dialog.TimeVariable))
                {
                    if (TimeVariableSet != null)
                    {
                        currentTimeVariable = dialog.TimeVariable;
                        TimeVariableSet(dialog.TimeVariable);
                    }
                }
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SetBackgroundImageType();
        }

        private void SetBackgroundImageType()
        {
            if (myMap != null)
            {
                if (myMap.Layers.Count > 0)
                {
                    if (myMap.Layers[0] is TileLayer)
                    {
                        if (ImageryRadioButton.Visibility == System.Windows.Visibility.Collapsed)
                        {
                            ((TileLayer)myMap.Layers[0]).Token = Configuration.GetNewInstance().Settings.MapServiceKey;
                            ((TileLayer)myMap.Layers[0]).LayerStyle = TileLayer.LayerType.AerialWithLabels;
                        }
                        else if (StreetsRadioButton.Visibility == System.Windows.Visibility.Collapsed)
                        {
                            ((TileLayer)myMap.Layers[0]).Token = Configuration.GetNewInstance().Settings.MapServiceKey;
                            ((TileLayer)myMap.Layers[0]).LayerStyle = TileLayer.LayerType.Road;
                        }
                        else
                        {
                            ((TileLayer)myMap.Layers[0]).Token = null;
                        }
                    }
                    ((TileLayer)myMap.Layers[0]).Refresh();
                }
            }
        }

        private void iconExpand_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ExpandRequested != null)
            {
                ExpandRequested(this, new EventArgs());
            }
            iconExpand.Visibility = System.Windows.Visibility.Collapsed;
            iconRestore.Visibility = System.Windows.Visibility.Visible;
        }

        private void iconRestore_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (RestoreRequested != null)
            {
                RestoreRequested(this, new EventArgs());
            }
            iconExpand.Visibility = System.Windows.Visibility.Visible;
            iconRestore.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void iconOpenMap_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenMap();
        }

        private void iconSaveMap_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SaveMap();
        }

        private void iconSaveAsImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SaveAsImage();
        }

        private void CollapseExpandLayerChooser()
        {
            if (grdLayerTypeChooser.Margin.Left == 0)
            {
                grdLayerTypeChooser.BeginAnimation(Grid.MarginProperty, new ThicknessAnimation(new Thickness(-310, 0, 0, 0), new Duration(TimeSpan.FromSeconds(0.2))));
            }
            else
            {
                grdLayerTypeChooser.BeginAnimation(Grid.MarginProperty, new ThicknessAnimation(new Thickness(0, 0, 0, 0), new Duration(TimeSpan.FromSeconds(0.2))));
            }
        }

        private void CollapseExpandLegend()
        {
            if (grdLegend.Margin.Left == 0)
            {
                grdLegend.BeginAnimation(Grid.MarginProperty, new ThicknessAnimation(new Thickness(-310, 0, 0, 0), new Duration(TimeSpan.FromSeconds(0.2))));
            }
            else
            {
                grdLegend.BeginAnimation(Grid.MarginProperty, new ThicknessAnimation(new Thickness(0, 0, 0, 0), new Duration(TimeSpan.FromSeconds(0.2))));

            }
        }

        private void iconAddDataLayer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CollapseExpandLayerChooser();
        }

        private void iconHideShowLegend_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CollapseExpandLegend();
        }

        private void imgChoropleth_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CollapseExpandLayerChooser();
            GenerateShapeFileChoropleth();
        }

        private void imgDotDensity_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CollapseExpandLayerChooser();
            GenerateShapeFileDotDensity();
        }
        private void imgPointofInterest_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CollapseExpandLayerChooser();
            GeneratePointofInterestMap();
        }
        private void imgCaseCluster_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CollapseExpandLayerChooser();
            GenerateCaseClusterMap();
        }
        private void ImageryRadioButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ToggleSatellite();
            SetBackgroundImageType();
        }

        private void StreetsRadioButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ToggleStreet();
            SetBackgroundImageType();
        }

        private void BlankRadioButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ToggleBlank();
            SetBackgroundImageType();
        }

        private void borderSat_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ToggleSatellite();
            SetBackgroundImageType();
        }

        private void borderStr_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ToggleStreet();
            SetBackgroundImageType();
        }

        private void borderBln_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ToggleBlank();
            SetBackgroundImageType();
        }

        EpiDashboard.Mapping.TimeLapse timeLaspe = null;               
        private void btn_TimeLapseClick(object sender, RoutedEventArgs e)
        {          
            if (!string.IsNullOrEmpty(timeLaspe.TimeVariable))
            {
                if (TimeVariableSet != null)
                {
                    currentTimeVariable = timeLaspe.TimeVariable;
                    timeLaspe.Closepopup();
                    TimeVariableSet(timeLaspe.TimeVariable);
                }               
           }            
        }
        private void iconTimeLapse_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            popup = new DashboardPopup();
            popup.Parent = LayoutRoot;
            List<ILayerProperties> layers = layerList.Layers;
            List<DashboardHelper> dashboardHelpers = new List<DashboardHelper>();
            foreach (ILayerProperties layer in layers)
            {
                if (layer.GetDashboardHelper() != null && layer.GetType().Name.Contains("ClusterLayerProperties"))
                {
                    dashboardHelpers.Add(layer.GetDashboardHelper());
                }
            }
            timeLaspe = new EpiDashboard.Mapping.TimeLapse(dashboardHelpers, this, myMap);
            timeLaspe.btnOK.Click += new RoutedEventHandler(btn_TimeLapseClick);
           
            timeLaspe.Width = 305;
            timeLaspe.Height = 160;

            timeLaspe.Cancelled += new EventHandler(properties_Cancelled);
            timeLaspe.ChangesAccepted += new EventHandler(properties_ChangesAccepted);
            popup.Content = timeLaspe;
            popup.Show();                  
        }

        private void iconReference_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DashboardHelper dashboardHelper = new DashboardHelper();
            popup = new DashboardPopup();
            popup.Parent = LayoutRoot;
            EpiDashboard.Controls.Referencelayer properties = new EpiDashboard.Controls.Referencelayer(this, myMap);
            properties.MapGenerated += new EventHandler(ILayerProperties_MapGenerated);
            properties.Width = 800;
            properties.Height = 600;

            if (ResizedWidth != 0 & ResizedHeight != 0)
            {
                double i_StandardHeight = System.Windows.SystemParameters.PrimaryScreenHeight;//Developer Desktop Width Where the Form is Designed
                double i_StandardWidth = System.Windows.SystemParameters.PrimaryScreenWidth; ////Developer Desktop Height Where the Form is Designed
                float f_HeightRatio = new float();
                float f_WidthRatio = new float();
                f_HeightRatio = (float)((float)ResizedHeight / (float)i_StandardHeight);
                f_WidthRatio = (float)((float)ResizedWidth / (float)i_StandardWidth);

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

            popup.Content = properties;
            popup.Show();

        }
       
    }


}