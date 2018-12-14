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
using EpiDashboard.Mapping;
using System.Net;
using System.IO;
using Esri.ArcGISRuntime.UI.Controls;

namespace EpiDashboard.Controls
{
    /// <summary>
    /// Interaction logic for Referencelayer.xaml
    /// </summary>
    public partial class Referencelayer : UserControl
    {
        private EpiDashboard.Mapping.StandaloneMapControl mapControl;
        private Esri.ArcGISRuntime.Mapping.Map _map;
        private Esri.ArcGISRuntime.UI.Controls.MapView _mapView;
        public event EventHandler Cancelled;
        public event EventHandler ChangesAccepted;
        private EpiDashboard.Mapping.KmlLayerProvider KMLprovider;
        private EpiDashboard.Mapping.MapServerLayerProvider Mapprovider;
        public EpiDashboard.Mapping.ShapeLayerProperties layerprop;
        public EpiDashboard.Mapping.MapServerLayerProperties serverlayerprop;
        public EpiDashboard.Mapping.KmlLayerProperties kmllayerprop;
        private EpiDashboard.Mapping.ShapeLayerProvider provider;

        # region Public Properties
        public DashboardHelper DashboardHelper { get; private set; }

        public string MapServerName { get; set; }
        public int[] MapVisibleLayers { get; set; }
        public string KMLMapServerName { get; set; }
        public int[] KMLMapVisibleLayers { get; set; }
        #endregion

        public Referencelayer(EpiDashboard.Mapping.StandaloneMapControl mapControl, MapView mapView)
        {
            InitializeComponent();
            this.mapControl = mapControl;
            this._mapView = mapView;
            this._map = _mapView.Map;
            this.radShapeFile.IsChecked = true;
            mapControl.SizeChanged += mapControl_SizeChanged;

            #region Translation

            lblConfigExpandedTitle.Content = DashboardSharedStrings.BASE_LAYER;
            tbtnDataSource.Title = DashboardSharedStrings.BASE_LAYER;
            tbtnDataSource.Description = DashboardSharedStrings.MENU_ADD_BASELAYER_MAP;
            lblPanelHeader.Content = DashboardSharedStrings.BASE_LAYER;

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
            lblExampleKMLURL.Text = DashboardSharedStrings.GADGET_EXAMPLE_KMLFILE;

            btnOK.Content = DashboardSharedStrings.BUTTON_OK;
            btnCancel.Content = DashboardSharedStrings.BUTTON_CANCEL;

            #endregion //Translation
        }

        public void mapControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            mapControl.ResizedWidth = e.NewSize.Width;
            mapControl.ResizedHeight = e.NewSize.Height;
            if (mapControl.ResizedWidth != 0 & mapControl.ResizedHeight != 0)
            {
                double i_StandardHeight = System.Windows.SystemParameters.PrimaryScreenHeight;//Developer Desktop Width Where the Form is Designed
                double i_StandardWidth = System.Windows.SystemParameters.PrimaryScreenWidth; ////Developer Desktop Height Where the Form is Designed
                float f_HeightRatio = new float();
                float f_WidthRatio = new float();
                f_HeightRatio = (float)((float)mapControl.ResizedHeight / (float)i_StandardHeight);
                f_WidthRatio = (float)((float)mapControl.ResizedWidth / (float)i_StandardWidth);

                this.Height = (Convert.ToInt32(i_StandardHeight * f_HeightRatio)) / 1.16;
                this.Width = (Convert.ToInt32(i_StandardWidth * f_WidthRatio)) / 1.13;

            }
            else
            {
                this.Width = (System.Windows.SystemParameters.PrimaryScreenWidth / 1.13);
                this.Height = (System.Windows.SystemParameters.PrimaryScreenHeight / 1.16);
            }
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

        private void lbxserverfield_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbxserverfield.Items.Count > 0)
            {
                MapServerName = txtMapSeverpath.Text;
                if (lbxserverfield.SelectedIndex > -1)
                {
                    //if (lbxserverfield.SelectedIndex.Count() > 0)
                    //{
                    //    int[] visibleLayers = new int[lbxserverfield.SelectedItems.Count];
                    //    int counter = 0;
                    //    foreach (SubObject item in lbxserverfield.SelectedItems)
                    //    {
                    //        visibleLayers[counter] = item.id;
                    //        counter++;
                    //    }
                    //    MapVisibleLayers = visibleLayers;
                    //}
                    //else
                    //{
                    //    MapVisibleLayers = null;
                    //}

                    if (Mapprovider == null)
                    {
                        Mapprovider = new Mapping.MapServerLayerProvider(_map);
                    }
                    ILayerProperties layerProperties = null;
                    layerProperties = new MapServerLayerProperties(_map);                    
                    layerProperties.MapGenerated += new EventHandler(this.mapControl.ILayerProperties_MapGenerated);
                    this.serverlayerprop = (MapServerLayerProperties)layerProperties;
                    serverlayerprop.provider = Mapprovider;
                    this.mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
                }
                else
                {

                }
            }

        }

        private void radKML_Checked(object sender, RoutedEventArgs e)
        {
            panelshape.IsEnabled = false;
            panelmap.IsEnabled = false;
            panelKml.IsEnabled = true;
            txtShapePath.Text = string.Empty;
            txtKMLpath.Text = string.Empty;
            lbxserverfield.SelectedIndex = -1;
            txtMapSeverpath.Text = string.Empty;
            if (Mapprovider != null)
            {
                Mapprovider = null;
            }
            if (provider != null)
            {
                provider = null;
            }

        }

        private void btnKMLFile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "KML/KMZ Files (*.kml/*.kmz)|*.kml;*.kmz";
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtKMLpath.Text = dialog.FileName;
                KMLMapServerName = txtKMLpath.Text;
                if (KMLprovider == null)
                {
                    KMLprovider = new EpiDashboard.Mapping.KmlLayerProvider(_mapView);
                }
                ILayerProperties layerProperties = null;
                layerProperties = new EpiDashboard.Mapping.KmlLayerProperties(_map);               
                layerProperties.MapGenerated += new EventHandler(this.mapControl.ILayerProperties_MapGenerated);
                this.kmllayerprop = (KmlLayerProperties)layerProperties;
                kmllayerprop.provider = KMLprovider;
                this.mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Cancelled != null)
            {
                Cancelled(this, new EventArgs());
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (radShapeFile.IsChecked == true && provider != null)
            {
                provider.RenderShape(txtShapePath.Text);               
                layerprop.CheckMapGenerated();
            }
            else if (radMapServer.IsChecked == true && Mapprovider != null)
            {
                Mapprovider.RenderServerImage(MapServerName, MapVisibleLayers);
                serverlayerprop.lblServerName.Content = MapServerName;
                serverlayerprop.CheckMapGenerated();
            }
            else if (radKML.IsChecked == true)
            {
                try
                {
                    if (KMLprovider == null)
                    {
                        KMLMapServerName = txtKMLpath.Text;
                        if (KMLprovider == null)
                        {
                            KMLprovider = new Mapping.KmlLayerProvider(_mapView);
                        }
                        ILayerProperties layerProperties = null;
                        layerProperties = new KmlLayerProperties(_map);
                        layerProperties.MapGenerated += new EventHandler(this.mapControl.ILayerProperties_MapGenerated);
                        this.kmllayerprop = (KmlLayerProperties)layerProperties;
                        kmllayerprop.provider = KMLprovider;
                        this.mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
                    }

                    KMLprovider.RenderServerImage(KMLMapServerName, KMLMapVisibleLayers);
                    kmllayerprop.lblServerName.Content = KMLMapServerName;
                    kmllayerprop.CheckMapGenerated();
                }
                catch { }
            }
            if (ChangesAccepted != null)
            {
                ChangesAccepted(this, new EventArgs());
            }
        }

        private void btnMapserverlocate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string message = GetMessage(txtMapSeverpath.Text + "?f=json");
                System.Web.Script.Serialization.JavaScriptSerializer ser = new System.Web.Script.Serialization.JavaScriptSerializer();
                Rest rest = ser.Deserialize<Rest>(message);
                lbxserverfield.ItemsSource = rest.layers;

                if (rest.layers.Count > 0)
                {
                    lbxserverfield.SelectedIndex = 0;
                }
            }
            catch
            {
                lbxserverfield.DataContext = null;
                System.Windows.Forms.MessageBox.Show("Invalid map server");
            }
        }

        private void txtMapSeverpath_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnMapserverlocate.IsEnabled = txtMapSeverpath.Text.Length > 0;
        }

        private void radlocatemapserver_Checked(object sender, RoutedEventArgs e)
        {
            panelmapconnect.IsEnabled = false;
            panelmapserver.IsEnabled = true;
            panelmapconnect.IsEnabled = false;
        }

        private void radconnectmapserver_Checked(object sender, RoutedEventArgs e)
        {
            panelmapconnect.IsEnabled = true;
            panelmapserver.IsEnabled = false;
            panelmapconnect.IsEnabled = true;
            txtMapSeverpath.Text = string.Empty;
        }

        private void radMapServer_Checked(object sender, RoutedEventArgs e)
        {
            panelshape.IsEnabled = false;
            panelmap.IsEnabled = true;
            panelKml.IsEnabled = false;
            
            txtShapePath.Text = string.Empty;
            txtKMLpath.Text = string.Empty;
            
            lbxserverfield.SelectedIndex = -1;
            txtMapSeverpath.Text = string.Empty;
            if (KMLprovider != null)
            {
                KMLprovider = null;
            }
            if (provider != null)
            {
                provider = null;
            }

            panelmapconnect.IsEnabled = true;
            panelmapserver.IsEnabled = false;
            panelmapconnect.IsEnabled = true;
            panelmapserver.IsEnabled = true;
            txtMapSeverpath.IsEnabled = true;
            txtMapSeverpath.Text = string.Empty;
        }

        private void btnBrowseShape_Click(object sender, RoutedEventArgs e)
        {
            provider = new Mapping.ShapeLayerProvider(_map);          
            //Create the dialog allowing the user to select the "*.shp" and the "*.dbf" files
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "ESRI Shapefiles (*.shp)|*.shp";
            if (ofd.ShowDialog().Value)
            {
                //ofd.Multiselect = true;
                string retval = ofd.FileName;
                if (retval != null)
                {
                    txtShapePath.Text = retval;
                    ILayerProperties layerProperties = null;
                    //''layerProperties = new ShapeLayerProperties(map);                    
                    layerProperties.MapGenerated += new EventHandler(this.mapControl.ILayerProperties_MapGenerated);
                    this.layerprop = (ShapeLayerProperties)layerProperties;
                    layerprop.provider = provider;
                    layerprop.lblFileName.Content = retval;
                    this.mapControl.grdLayerConfigContainer.Children.Add((UIElement)layerProperties);
                }
            }
        }

        private void txtShapePath_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void radShapeFile_Checked(object sender, RoutedEventArgs e)
        {
            panelshape.IsEnabled = true;
            panelmap.IsEnabled = false;
            panelKml.IsEnabled = false;
            txtShapePath.Text = string.Empty;
            txtKMLpath.Text = string.Empty;
            txtMapSeverpath.Text = string.Empty;
            
            if (Mapprovider != null)
            {
                Mapprovider = null;
            }
            
            if (KMLprovider != null)
            {
                KMLprovider = null;
            }
        }

        public EventHandler MapGenerated { get; set; }
    }
}
