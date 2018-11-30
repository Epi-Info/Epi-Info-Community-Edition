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

namespace EpiDashboard.Mapping
{
    /// <summary>
    /// Interaction logic for LayerListItem.xaml
    /// </summary>
    public partial class LayerListItem : UserControl
    {
        private ESRI.ArcGIS.Client.Map myMap;
        private Epi.View view;
        private Epi.Data.IDbDriver db;
        private DashboardHelper dashboardHelper;
        private ILayerProperties LayerProperties;
        private Color fontColor;

        public event EventHandler MapGenerated;
        private MapControl mapControl;

        public LayerListItem(ESRI.ArcGIS.Client.Map myMap, Epi.View view, Epi.Data.IDbDriver db, DashboardHelper dashboardHelper, MapControl mapControl)
        {
            InitializeComponent();

            this.myMap = myMap;
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
                    LayerProperties = new ClusterLayerProperties(myMap, dashboardHelper, mapControl);
                    break;
                case 1:
                    LayerProperties = new ChoroplethShapeLayerProperties(myMap, dashboardHelper, mapControl);
                    break;
                default:
                    LayerProperties = new ClusterLayerProperties(myMap, dashboardHelper, mapControl);
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
