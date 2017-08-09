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
    /// Interaction logic for LayerAdder.xaml
    /// </summary>
    public partial class LayerAdder : UserControl
    {

        private LayerList layerList;
        private LayerListItem item;
        Esri.ArcGISRuntime.Controls.Map myMap;
        Epi.View view;
        Epi.Data.IDbDriver db;
        DashboardHelper dashboardHelper;
        private MapControl mapControl;

        public LayerAdder(Esri.ArcGISRuntime.Controls.Map myMap, Epi.View view, Epi.Data.IDbDriver db, DashboardHelper dashboardHelper, LayerList layerList, MapControl mapControl)
        {
            InitializeComponent();

            this.myMap = myMap;
            this.view = view;
            this.db = db;
            this.dashboardHelper = dashboardHelper;
            this.layerList = layerList;
            this.mapControl = mapControl;

            AddItem();
        }

        private void AddItem()
        {
            this.item = new LayerListItem(myMap, view, db, dashboardHelper, mapControl);
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
