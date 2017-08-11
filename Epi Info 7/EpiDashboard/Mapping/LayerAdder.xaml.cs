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
    /// Interaction logic for LayerAdder.xaml
    /// </summary>
    public partial class LayerAdder : UserControl
    {

        private LayerList layerList;
        private LayerListItem item;
        private MapView _mapView;
        Epi.View view;
        Epi.Data.IDbDriver db;
        DashboardHelper dashboardHelper;
        private MapControl mapControl;

        public LayerAdder(MapView mapView, Epi.View view, Epi.Data.IDbDriver db, DashboardHelper dashboardHelper, LayerList layerList, MapControl mapControl)
        {
            InitializeComponent();

            _mapView = mapView;
            this.view = view;
            this.db = db;
            this.dashboardHelper = dashboardHelper;
            this.layerList = layerList;
            this.mapControl = mapControl;

            AddItem();
        }

        private void AddItem()
        {
            this.item = new LayerListItem(_mapView, view, db, dashboardHelper, mapControl);
            this.item.FontColor = Colors.White;
            item.MapGenerated += new EventHandler(item_MapGenerated);
            container.Children.Add(item);
        }

        void item_MapGenerated(object sender, EventArgs e)
        {
            LayerListItem senderItem = (LayerListItem)sender;
            if (senderItem == this.item)
            {
                container.Children.Remove(senderItem);
                layerList.AddListItem(senderItem.GetLayerPropertiesControl(), 0);
                AddItem();
            }
        }
    }
}
