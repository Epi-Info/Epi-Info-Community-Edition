using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;

using Epi;
using Epi.Data;
using EpiDashboard.Mapping.ShapeFileReader;

namespace EpiDashboard.Mapping
{

    public delegate void RecordSelectedHandler(int id);
    public delegate void TimeVariableSetHandler(string timeVariable);

    /// <summary>
    /// Interaction logic for MapControl.xaml
    /// </summary>
    public partial class MapControl : UserControl, IMapControl
    {
        public event EventHandler MapLoaded;
        public event RecordSelectedHandler RecordSelected;
        public event TimeVariableSetHandler TimeVariableSet;
        public event EventHandler MapDataChanged;

        private BackgroundWorker worker;
        private MapView _mapView;
        private MapPoint _rightClickedPoint;
        private System.Drawing.Color selectedColor;
        private double selectedRadius;
        private Esri.ArcGISRuntime.Geometry.LinearUnit selectedUnit;
        private delegate void RenderMapDelegate(string url);
        private delegate void SimpleDelegate();
        private delegate void DebugDelegate(string debugMsg);
        private object slider;
        private View view;
        private IDbDriver db;
        private DashboardHelper dashboardHelper;
        private DataFilteringControl dataFilteringControl;
        private DataRecodingControl dataRecodingControl;
        private ClusterLayerProvider clusterLayerProvider;
        private LayerList layerList;
        private LayerAdder layerAdder;
        private string currentTimeVariable;
        private Brush defaultBackgroundColor = Brushes.White;

        public MapControl(View view, IDbDriver db)
        {
            InitializeComponent();
            this.view = view;
            this.db = db;
            dashboardHelper = new DashboardHelper(view, db);

            worker = new System.ComponentModel.BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerAsync();
            MapContainer.SizeChanged += new SizeChangedEventHandler(MapContainer_SizeChanged);
            imgClose.MouseDown += new MouseButtonEventHandler(imgClose_MouseDown);

            this.MapBackground = Brushes.White;
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
            if (_mapView != null)
            {
                _mapView.Width = e.NewSize.Width;
                _mapView.Height = e.NewSize.Height;
            }
            if (dataFilteringControl != null)
            {
                Canvas.SetTop(dataFilteringControl, (e.NewSize.Height / 2.0) - (dataFilteringControl.ActualHeight / 2.0));
            }
            if (dataRecodingControl != null)
            {
                Canvas.SetTop(dataRecodingControl, (e.NewSize.Height / 2.0) - (dataRecodingControl.ActualHeight / 2.0));
            }
            if (layerList != null)
            {
                Canvas.SetLeft(layerList, (e.NewSize.Width / 2.0) - (layerList.ActualWidth / 2.0));
                Canvas.SetBottom(layerList, (layerList.ActualHeight * -1) + 30);
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (InternetAvailable())
            {
                this.Dispatcher.BeginInvoke(new SimpleDelegate(PrepareToLoad));
                System.Threading.Thread.Sleep(1000);
                this.Dispatcher.BeginInvoke(new SimpleDelegate(RenderMap));                
            }
        }

        private void PrepareToLoad()
        {
            txtInternet.Visibility = Visibility.Collapsed;
            txtLoading.Visibility = Visibility.Visible;
            waitCursor.Visibility = Visibility.Visible;
        }

        private void RenderMap()
        {
            txtLoading.Visibility = Visibility.Collapsed;
            waitCursor.Visibility = Visibility.Collapsed;
            LayerTypeSelector.Visibility = Visibility.Visible;

            BingLayer bingLayer = new BingLayer();
            bingLayer.ID = "Aua5s8kFcEZMx5lsd8Vkerz3frboU1CwzvOyzX_vgSnzsnbqV7xlQ4WTRUlN19_Q";
            bingLayer.MapStyle = BingLayer.LayerType.AerialWithLabels;

            GraphicsLayer pointLayer = new GraphicsLayer();
            pointLayer.ID = "pointLayer";

            GraphicsLayer zoneLayer = new GraphicsLayer();
            zoneLayer.ID = "zoneLayer";

            GraphicsLayer textLayer = new GraphicsLayer();
            textLayer.ID = "textLayer";

            GraphicsLayer clusterLayer = new GraphicsLayer();
            clusterLayer.ID = "clusterLayer";

            GraphicsLayer shapeFileLayer = new GraphicsLayer();
            shapeFileLayer.ID = "shapefileGraphicsLayer";

            ContextMenu menu = new ContextMenu();
            MenuItem mnuTimeLapse = new MenuItem();
            mnuTimeLapse.Header = "Create Time Lapse...";
            mnuTimeLapse.Click += new RoutedEventHandler(mnuTimeLapse_Click);
            menu.Items.Add(mnuTimeLapse);
            menu.Items.Add(new Separator());
            MenuItem mnuMarker = new MenuItem();
            mnuMarker.Header = "Add marker";
            mnuMarker.Click += new RoutedEventHandler(mnuMarker_Click);
            menu.Items.Add(mnuMarker);
            MenuItem mnuRadius = new MenuItem();
            mnuRadius.Header = "Add zone";
            mnuRadius.Click += new RoutedEventHandler(mnuRadius_Click);
            menu.Items.Add(mnuRadius);
            MenuItem mnuText = new MenuItem();
            mnuText.Header = "Add label";
            mnuText.Click += new RoutedEventHandler(mnuText_Click);
            menu.Items.Add(mnuText);
            MenuItem mnuClear = new MenuItem();
            mnuClear.Header = "Clear graphics";
            mnuClear.Click += new RoutedEventHandler(mnuClear_Click);
            menu.Items.Add(mnuClear);
            menu.Items.Add(new Separator());
            MenuItem mnuSave = new MenuItem();
            mnuSave.Header = "Save map as image...";
            mnuSave.Click += new RoutedEventHandler(mnuSave_Click);
            menu.Items.Add(mnuSave);

            _mapView = new MapView();
            _mapView.Background = Brushes.White;
            _mapView.Height = MapContainer.ActualHeight;
            _mapView.Width = MapContainer.ActualWidth;
            _mapView.ContextMenu = menu;
            _mapView.Map.Layers.Add(bingLayer);
            _mapView.Map.Layers.Add(shapeFileLayer);
            _mapView.Map.Layers.Add(pointLayer);
            _mapView.Map.Layers.Add(textLayer);
            _mapView.Map.Layers.Add(zoneLayer);
            _mapView.Map.Layers.Add(clusterLayer);
            _mapView.MouseMove += new MouseEventHandler(myMap_MouseMove);
            _mapView.MouseRightButtonDown += new MouseButtonEventHandler(myMap_MouseRightButtonDown);
            MapContainer.Children.Add(_mapView);

            //////ESRI.ArcGIS.Client.Behaviors.ConstrainExtentBehavior extentBehavior = new ESRI.ArcGIS.Client.Behaviors.ConstrainExtentBehavior();
            //////extentBehavior.ConstrainedExtent = new Envelope(new MapPoint(-20000000, -12000000), new MapPoint(20000000, 12000000));
            //////System.Windows.Interactivity.Interaction.GetBehaviors(_mapView).Add(extentBehavior);

            //////Navigation nav = new Navigation();
            //////nav.Margin = new Thickness(5);
            //////nav.HorizontalAlignment = HorizontalAlignment.Left;
            //////nav.VerticalAlignment = VerticalAlignment.Top;
            //////nav.Map = _mapView;
            //////MapContainer.Children.Add(nav);

            //////slider = new TimeSlider();
            //////slider.Name = "slider";
            //////slider.PlaySpeed = new TimeSpan(0, 0, 1);
            //////slider.Height = 20;
            //////slider.TimeMode = TimeMode.CumulativeFromStart;
            //////slider.MinimumValue = DateTime.Now.Subtract(TimeSpan.FromDays(7)).ToUniversalTime();
            //////slider.MaximumValue = DateTime.Now.ToUniversalTime();
            //////slider.Value = new TimeExtent(slider.MinimumValue, slider.MinimumValue.AddHours(2));
            //////slider.Intervals = TimeSlider.CreateTimeStopsByTimeInterval(new TimeExtent(slider.MinimumValue, slider.MaximumValue), new TimeSpan(0, 2, 0, 0));
            //////slider.Padding = new Thickness(0, 100, 0, 0);
            //////slider.ValueChanged += new EventHandler<TimeSlider.ValueChangedEventArgs>(slider_ValueChanged);
            //////areaSeries.Loaded += new RoutedEventHandler(areaSeries_Loaded);
            //////stkTimeLapse.Children.Add(slider);

            SetBackgroundColor(defaultBackgroundColor);

            AddSelectionCriteria();
            AddFormatOptions();
            AddLayerList();
            
            if (MapLoaded != null)
            {
                MapLoaded(this, new EventArgs());
            }
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

        private void SetBackgroundColor(Brush brush)
        {
            if (grdMapDef != null && _mapView != null)
            {
                grdMapDef.Background = brush;
                _mapView.Background = brush;
            }
        }

        void AddLayerList()
        {
            layerList = new LayerList(_mapView, view, db, dashboardHelper);
            layerList.Loaded += new RoutedEventHandler(layerList_Loaded);
            layerList.SizeChanged += new SizeChangedEventHandler(layerList_SizeChanged);
            layerList.MouseEnter += new MouseEventHandler(layerList_MouseEnter);
            layerList.MouseLeave += new MouseEventHandler(layerList_MouseLeave);
            MapContainer.Children.Add(layerList);

            layerAdder = new LayerAdder(_mapView.Map, view, db, dashboardHelper, layerList, this);
            layerAdder.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            Grid.SetRow(layerAdder, 0);
            grdMapDef.Children.Add(layerAdder);
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

            if (e.PreviousSize.Height < e.NewSize.Height)
            {
                DoubleAnimation anim = new DoubleAnimation();
                anim.From = Canvas.GetBottom(layerList);
                anim.To = (e.NewSize.Height * -1) + 30;
                anim.DecelerationRatio = 0.8;
                anim.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                layerList.BeginAnimation(Canvas.BottomProperty, anim);
            }
        }

        void layerList_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas.SetLeft(layerList, (MapContainer.ActualWidth / 2.0) - (layerList.ActualWidth / 2.0));
            Canvas.SetBottom(layerList, (layerList.ActualHeight * -1) + 30);
        }

        void AddSelectionCriteria()
        {
            dataFilteringControl = new DataFilteringControl(dashboardHelper);
            dataFilteringControl.MouseEnter += new MouseEventHandler(selectionCriteriaControl_MouseEnter);
            dataFilteringControl.MouseLeave += new MouseEventHandler(selectionCriteriaControl_MouseLeave);
            dataFilteringControl.SelectionCriteriaChanged += new EventHandler(selectionCriteriaControl_SelectionCriteriaChanged);
            dataFilteringControl.Loaded += new RoutedEventHandler(dataFilteringControl_Loaded);
            MapContainer.Children.Add(dataFilteringControl);

            DragCanvas.SetCanBeDragged(dataFilteringControl, false);
        }

        void dataFilteringControl_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas.SetRight(dataFilteringControl, -540);
            Canvas.SetTop(dataFilteringControl, (MapContainer.ActualHeight / 2.0) - (dataFilteringControl.ActualHeight / 2.0));
        }

        void selectionCriteriaControl_SelectionCriteriaChanged(object sender, EventArgs e)
        {
            if (MapDataChanged != null)
            {
                MapDataChanged(this, new EventArgs());
            }
        }

        void selectionCriteriaControl_MouseLeave(object sender, MouseEventArgs e)
        {
            DoubleAnimation anim = new DoubleAnimation();
            anim.From = Canvas.GetRight(dataFilteringControl);
            anim.To = -540;
            anim.DecelerationRatio = 0.8;
            anim.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            dataFilteringControl.BeginAnimation(Canvas.RightProperty, anim);
        }

        void selectionCriteriaControl_MouseEnter(object sender, MouseEventArgs e)
        {
            DoubleAnimation anim = new DoubleAnimation();
            anim.From = Canvas.GetRight(dataFilteringControl);
            anim.To = -20;
            anim.AccelerationRatio = 0.8;
            anim.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            dataFilteringControl.BeginAnimation(Canvas.RightProperty, anim);
        }

        void AddFormatOptions()
        {
            dataRecodingControl = new DataRecodingControl(dashboardHelper);
            dataRecodingControl.MouseEnter += new MouseEventHandler(dataRecodingControl_MouseEnter);
            dataRecodingControl.MouseLeave += new MouseEventHandler(dataRecodingControl_MouseLeave);
            dataRecodingControl.Loaded += new RoutedEventHandler(dataRecodingControl_Loaded);
            MapContainer.Children.Add(dataRecodingControl);
            
            DragCanvas.SetCanBeDragged(dataRecodingControl, false);
        }

        void dataRecodingControl_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas.SetLeft(dataRecodingControl, -385);
            Canvas.SetTop(dataRecodingControl, (MapContainer.ActualHeight / 2.0) - (dataRecodingControl.ActualHeight / 2.0));
        }

        void dataRecodingControl_MouseLeave(object sender, MouseEventArgs e)
        {
            DoubleAnimation anim = new DoubleAnimation();
            anim.From = Canvas.GetLeft(dataRecodingControl);
            anim.To = -385;
            anim.DecelerationRatio = 0.8;
            anim.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            dataRecodingControl.BeginAnimation(Canvas.LeftProperty, anim);
        }

        void dataRecodingControl_MouseEnter(object sender, MouseEventArgs e)
        {
            DoubleAnimation anim = new DoubleAnimation();
            anim.From = Canvas.GetLeft(dataRecodingControl);
            anim.To = -20;
            anim.AccelerationRatio = 0.8;
            anim.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            dataRecodingControl.BeginAnimation(Canvas.LeftProperty, anim);
        }

        void mnuTimeLapse_Click(object sender, RoutedEventArgs e)
        {
            TimeConfigDialog dialog = new TimeConfigDialog(view);
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

        void mnuSave_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG Image (.png)|*.png";

            if (dlg.ShowDialog().Value)
            {
                BitmapSource img = (BitmapSource)ToImageSource(MapContainer);

                FileStream stream = new FileStream(dlg.SafeFileName, FileMode.Create);
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(img));
                encoder.Save(stream);
                stream.Close();
                MessageBox.Show("Map image has been saved.", "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
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

        void mnuText_Click(object sender, RoutedEventArgs e)
        {
            MapTextDialog dialog = new MapTextDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                GraphicsLayer graphicsLayer = (GraphicsLayer)_mapView.Map.Layers["textLayer"];
                graphicsLayer.Graphics.Add(new Graphic() { Geometry = _rightClickedPoint, Symbol = dialog.MapText });
            }
        }

        void mnuClear_Click(object sender, RoutedEventArgs e)
        {
            ClearGraphics();
        }

        public void ClearGraphics()
        {
            grdTimeLapse.Visibility = Visibility.Collapsed;
            stkTimeLapse.Visibility = Visibility.Collapsed;
            chrtTimeLapse.Visibility = Visibility.Collapsed;

            ((GraphicsLayer)_mapView.Map.Layers["pointLayer"]).Graphics.Clear();
            ((GraphicsLayer)_mapView.Map.Layers["textLayer"]).Graphics.Clear();
            ((GraphicsLayer)_mapView.Map.Layers["zoneLayer"]).Graphics.Clear();
            ((GraphicsLayer)_mapView.Map.Layers["shapefileGraphicsLayer"]).Graphics.Clear();

            GraphicsLayer clusterLayer = _mapView.Map.Layers["clusterLayer"] as GraphicsLayer;
            if (clusterLayer != null)
            {
                _mapView.Map.Layers.Remove(clusterLayer);
            }
            clusterLayer = new GraphicsLayer();
            clusterLayer.ID = "clusterLayer";
            _mapView.Map.Layers.Add(clusterLayer);
        }

        void mnuMarker_Click(object sender, RoutedEventArgs e)
        {

        }

        void mnuRadius_Click(object sender, RoutedEventArgs e)
        {
            RadiusMapConfigDialog dialog = new RadiusMapConfigDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                selectedColor = dialog.SelectedColor;
                selectedRadius = dialog.Radius;
                selectedUnit = dialog.Unit;
                AddBufferedPoint();
            }
        }

        void myMap_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _rightClickedPoint = _mapView.ScreenToLocation(e.GetPosition(_mapView));
        }

        private void AddBufferedPoint()
        {
            GraphicsLayer graphicsLayer = _mapView.Map.Layers["zoneLayer"] as GraphicsLayer;

            double x = _rightClickedPoint.X;
            double y = _rightClickedPoint.Y;
            SpatialReference sRef = _rightClickedPoint.SpatialReference;

            _rightClickedPoint = new MapPoint(x, y, sRef);

            Graphic graphic = new Graphic()
            {
                Geometry = _rightClickedPoint,
                Symbol = SimplePointSymbol
            };

            graphic.ZIndex = 1;
            graphicsLayer.Graphics.Add(graphic);

            ////////////Esri.ArcGISRuntime.ArcGISServices. =
            ////////////  new Esri.ArcGISRuntime.ArcGISServices.FeatureServiceInfo("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");
            ////////////geometryService.BufferCompleted += GeometryService_BufferCompleted;
            ////////////geometryService.Failed += GeometryService_Failed;

            ////////////// If buffer spatial reference is GCS and unit is linear, geometry service will do geodesic buffering
            ////////////BufferParameters bufferParams = new BufferParameters()
            ////////////{
            ////////////    Unit = selectedUnit,
            ////////////    BufferSpatialReference = new SpatialReference(4326),
            ////////////    OutSpatialReference = _mapView.SpatialReference
            ////////////};
            
            ////////////bufferParams.Features.Add(graphic);
            ////////////bufferParams.Distances.Add(selectedRadius);

            ////////////geometryService.BufferAsync(bufferParams);
        }

        ////////////void GeometryService_BufferCompleted(object sender, GraphicsEventArgs args)
        ////////////{
        ////////////    IList<Graphic> results = args.Results;
        ////////////    GraphicsLayer graphicsLayer = _mapView.Map.Layers["zoneLayer"] as GraphicsLayer;

        ////////////    foreach (Graphic graphic in results)
        ////////////    {
        ////////////        graphic.Symbol = new SimpleFillSymbol() { Fill = new SolidColorBrush(Color.FromArgb(64, selectedColor.R, selectedColor.G, selectedColor.B)), BorderThickness = 0 };
        ////////////        graphicsLayer.Graphics.Add(graphic);
        ////////////    }
        ////////////}

        ////////////private void GeometryService_Failed(object sender, TaskFailedEventArgs e)
        ////////////{
        ////////////    MessageBox.Show("Geometry Service error: " + e.Error);
        ////////////}

        public void UnSubscribe()
        {
            if(_mapView != null)
            {
                    _mapView.MouseMove -= new MouseEventHandler(myMap_MouseMove);
            }
            this._mapView = null;
            this.worker = null;
        }
        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (_mapView != null)
            {
                if (_mapView.Map.Layers.Count > 0)
                {
                    if (ImageryRadioButton.IsChecked.Value)
                    {
                        ((BingLayer)_mapView.Map.Layers[0]).MapStyle = BingLayer.LayerType.AerialWithLabels;
                    }
                    else
                    {
                        ((BingLayer)_mapView.Map.Layers[0]).MapStyle = BingLayer.LayerType.Road;
                    }
                }
            }
        }

        private SimpleMarkerSymbol SimplePointSymbol
        {
            get
            {
                SimpleMarkerSymbol symbol = new SimpleMarkerSymbol();
                symbol.Color = System.Windows.Media.Color.FromRgb(0,0,0);
                symbol.Size = 3;
                symbol.Style = SimpleMarkerStyle.Circle;
                return symbol;
            }
        }

        void myMap_MouseMove(object sender, MouseEventArgs e)
        {
            MapView myMap = (MapView)e.Source;
            if (!myMap.IsFocused)
                myMap.Focus();
        }

        private bool InternetAvailable()
        {
            bool retval = false;
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("http://www.google.com");
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                retval = true;
            }
            catch { }
            return retval;
        }

        private void ShowDebugMessage(string debugMsg)
        {
            MessageBox.Show(debugMsg);
        }

        private List<KeyValuePair<DateTime, int>> areaChartDataPoints;

        public void OnDateRangeDefined(DateTime start, DateTime end, List<KeyValuePair<DateTime, int>> areaChartDataPoints)
        {
            this.areaChartDataPoints = areaChartDataPoints;

            _mapView.TimeExtent = new TimeExtent(start.AddHours(-1), end.AddHours(1));

            //slider.MinimumValue = _mapView.TimeExtent.Start;
            //slider.MaximumValue = _mapView.TimeExtent.End;
            //slider.Value = new TimeExtent(slider.MinimumValue, slider.MinimumValue.AddHours(2));
            //slider.Intervals = TimeSlider.CreateTimeStopsByTimeInterval(new TimeExtent(slider.MinimumValue, slider.MaximumValue), new TimeSpan(1, 0, 0, 0));
            
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

        void areaSeries_Loaded(object sender, RoutedEventArgs e)
        {
            if (areaChartDataPoints != null)
            {
                ////////////List<KeyValuePair<DateTime, int>> test = areaChartDataPoints.FindAll(delegate(KeyValuePair<DateTime, int> pair)
                ////////////{
                ////////////    ////////return pair.Key < slider.Value.End;
                ////////////});
                ////////////areaSeries.ItemsSource = test;
            }
        }

        ////////////void slider_ValueChanged(object sender, TimeSlider.ValueChangedEventArgs e)
        ////////////{
        ////////////    if (currentTimeVariable != null)
        ////////////    {
        ////////////        grdTimeLapse.Visibility = Visibility.Visible;
        ////////////        stkTimeLapse.Visibility = Visibility.Visible;
        ////////////        chrtTimeLapse.Visibility = Visibility.Visible;

        ////////////        txtSliderStartDate.Text = e.NewValue.Start.ToShortDateString();
        ////////////        txtSliderEndDate.Text = e.NewValue.End.ToShortDateString();

        ////////////        _mapView.TimeExtent = new TimeExtent(e.NewValue.Start, e.NewValue.End);
        ////////////        if (areaChartDataPoints != null)
        ////////////        {
        ////////////            List<KeyValuePair<DateTime, int>> test = areaChartDataPoints.FindAll(delegate(KeyValuePair<DateTime, int> pair)
        ////////////            {
        ////////////                return pair.Key < e.NewValue.End;
        ////////////            });
        ////////////            areaSeries.ItemsSource = test;
        ////////////        }
        ////////////    }
        ////////////}
        

        public void OnRecordSelected(int id)
        {
            if (RecordSelected != null)
            {
                RecordSelected(id);
            }
        }

    }

    public class CustomCoordinateList : IEnumerable<CustomCoordinate>
    {
        public List<CustomCoordinate> Coordinates = new List<CustomCoordinate>();

        #region IEnumerable<CustomCoordinate> Members

        public IEnumerator<CustomCoordinate> GetEnumerator()
        {
            return Coordinates.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Coordinates.GetEnumerator();
        }

        #endregion
    }

    public class CustomCoordinate
    {
        public CustomCoordinate() { }

        public CustomCoordinate(int recordId, double y, double x, DateTime? timeSpan)
        {
            this.RecordId = recordId;
            this.X = x;
            this.Y = y;
            if (timeSpan.HasValue)
                this.TimeSpan = timeSpan.Value;
        }

        public int RecordId { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public DateTime? TimeSpan { get; set; }
    }

    public class ExtendedGraphic : Graphic
    {
        public int RecordId { get; set; }
    }
}
