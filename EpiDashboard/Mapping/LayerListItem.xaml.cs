using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;

namespace EpiDashboard.Mapping
{
    /// <summary>
    /// Interaction logic for LayerListItem.xaml
    /// </summary>
    public partial class LayerListItem : UserControl
    {
        private MapView _mapView;
        private Epi.View view;
        private Epi.Data.IDbDriver db;
        private DashboardHelper dashboardHelper;
        private ILayerProperties LayerProperties;
        private Color fontColor;

        public event EventHandler MapGenerated;
        private MapControl mapControl;

        public LayerListItem(MapView mapView, Epi.View view, Epi.Data.IDbDriver db, DashboardHelper dashboardHelper, MapControl mapControl)
        {
            InitializeComponent();

            _mapView = mapView;
            this.view = view;
            this.db = db;
            this.dashboardHelper = dashboardHelper;
            this.mapControl = mapControl;

            cbxLayerType.SelectionChanged += new SelectionChangedEventHandler(cbxLayerType_SelectionChanged);
            cbxLayerType.SelectedIndex = 0;
            lblLayerType.Foreground = new SolidColorBrush(FontColor);
        }

        void cbxLayerType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cbxLayerType.SelectedIndex)
            {
                case 0:
                    LayerProperties = new ClusterLayerProperties(_mapView, dashboardHelper, mapControl);
                    break;
                case 1:
                    LayerProperties = new ChoroplethShapeLayerProperties(_mapView, dashboardHelper, mapControl);
                    break;
                default:
                    LayerProperties = new ClusterLayerProperties(_mapView, dashboardHelper, mapControl);
                    break;
            }
            LayerProperties.MapGenerated += new EventHandler(layerProperties_MapGenerated);
            LayerProperties.FontColor = FontColor;
            AddLayerProperties((UserControl)LayerProperties);
        }

        public Color FontColor
        {
            get
            {
                if (fontColor == null)
                    return Colors.Black;
                else
                    return fontColor;
            }
            set
            {
                LayerProperties.FontColor = value;
                lblLayerType.Foreground = new SolidColorBrush(value);
                fontColor = value;
            }
        }

        void layerProperties_MapGenerated(object sender, EventArgs e)
        {
            if (MapGenerated != null)
            {
                MapGenerated(this, new EventArgs());
            }
        }

        public void CloseLayer()
        {
            LayerProperties.CloseLayer();
        }

        private void AddLayerProperties(UserControl ctrl)
        {
            grdPlaceholder.Children.Clear();
            grdPlaceholder.Children.Add(ctrl);
        }

        public ILayerProperties GetLayerPropertiesControl()
        {
            grdPlaceholder.Children.Remove((UIElement)LayerProperties);
            return LayerProperties;
        }
    }
}
