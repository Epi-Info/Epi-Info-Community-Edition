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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//using ESRI.ArcGIS.Client;
//using ESRI.ArcGIS.Client.Toolkit;
//using ESRI.ArcGIS.Client.Toolkit.DataSources;
//using ESRI.ArcGIS.Client.Bing;
//using ESRI.ArcGIS.Client.Geometry;
//using ESRI.ArcGIS.Client.Symbols;
//using ESRI.ArcGIS.Client.Tasks;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;

using Epi;
using Epi.Data;
using EpiDashboard.Mapping.ShapeFileReader;
using System.Windows.Controls.DataVisualization.Charting;

namespace EpiDashboard.Mapping
{

    public class KmlLayerProvider : ILayerProvider
    {
        private Map _map;
        private Esri.ArcGISRuntime.UI.Controls.MapView _mapView;
        private Guid layerId;
        private string url;
        private int[] visibleLayers;
        //private List<GraphicsLayer> graphicsLayers;

        //''public KmlLayerProvider(Esri.ArcGISRuntime.Mapping.Map map)
        public KmlLayerProvider(Esri.ArcGISRuntime.UI.Controls.MapView mapView)
        {
            this._map = mapView.Map;
            this._mapView = mapView;
            this.layerId = Guid.NewGuid();
            //graphicsLayers = new List<GraphicsLayer>();
        }

        public void Refresh()
        {
            KmlLayer layer = _map.OperationalLayers[layerId.ToString()] as KmlLayer;
            if (layer != null)
            {
                _map.OperationalLayers.Remove(layer);
                RenderServerImage(this.url, this.visibleLayers);
            }
        }

        public void MoveUp()
        {
            Layer layer = _map.OperationalLayers[layerId.ToString()];
            int currentIndex = _map.OperationalLayers.IndexOf(layer);
            if (currentIndex < _map.OperationalLayers.Count - 1)
            {
                _map.OperationalLayers.Remove(layer);
                _map.OperationalLayers.Insert(currentIndex + 1, layer);
            }
        }

        public void MoveDown()
        {
            Layer layer = _map.OperationalLayers[layerId.ToString()];
            int currentIndex = _map.OperationalLayers.IndexOf(layer);
            if (currentIndex > 1)
            {
                _map.OperationalLayers.Remove(layer);
                _map.OperationalLayers.Insert(currentIndex - 1, layer);
            }
        }

        public async void RenderServerImage(string url, int[] visibleLayers)
        {
            this.url = url;

            KmlLayer shapeLayer = _map.OperationalLayers[layerId.ToString()] as KmlLayer;
            if (shapeLayer != null)
            {
                _map.OperationalLayers.Remove(shapeLayer);
            }

            shapeLayer = new KmlLayer(new Uri(url));
            shapeLayer.Id = layerId.ToString();

            shapeLayer.Loaded += new EventHandler<EventArgs>(shapeLayer_Initialized);

            Esri.ArcGISRuntime.License license = ArcGISRuntimeEnvironment.GetLicense();
            //await shapeLayer.LoadAsync();

            if (visibleLayers != null)
            {
                //shapeLayer.VisibleLayers = visibleLayers;
            }

            // Create a graphics overlay.
            GraphicsOverlay graphicOverlay = new GraphicsOverlay();

            MapPoint oldFaithfulPoint = new MapPoint(-110.828140, 44.460458, SpatialReferences.Wgs84);
            Graphic oldFaithfulGraphic = new Graphic(oldFaithfulPoint);
            graphicOverlay.Graphics.Add(oldFaithfulGraphic);

            // Create a simple marker symbol - red, cross, size 12.
            SimpleMarkerSymbol mySymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, System.Drawing.Color.Red, 12);

            // Create a simple renderer based on the simple marker symbol.
            SimpleRenderer myRenderer = new SimpleRenderer(mySymbol);

            // Apply the renderer to the graphics overlay (all graphics use the same symbol).
            graphicOverlay.Renderer = myRenderer;

            _mapView.GraphicsOverlays.Add(graphicOverlay);

            GraphicsOverlay kmlGraphicsOverlay = new GraphicsOverlay();
            

            //kmlGraphicsOverlay.Graphics.Add()


            _mapView.GraphicsOverlays.Add(kmlGraphicsOverlay);

            //_map.OperationalLayers.Add(shapeLayer);

            Envelope _usEnvelope = new Envelope(-144.619561355187, 18.0328662832097, -66.0903762761083, 67.6390975806745, SpatialReferences.Wgs84);
            await _mapView.SetViewpointAsync(new Viewpoint(_usEnvelope));
            //myMap.Extent = shapeLayer.FullExtent;
        }

        void shapeLayer_Initialized(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            KmlLayer shapeLayer = _map.OperationalLayers[layerId.ToString()] as KmlLayer;

            FindGraphicsLayers(shapeLayer);
            int x = 5;
            x++;
        }

        private void FindGraphicsLayers(KmlLayer kmlLayer)
        {
            System.Diagnostics.Debug.Assert(true);
            //foreach (Layer layer in kmlLayer.ChildLayers)
            //{
            //    if (layer is GraphicsLayer)
            //    {
            //        graphicsLayers.Add((GraphicsLayer)layer);
            //    }
            //    else if (layer is KmlLayer)
            //    {
            //        FindGraphicsLayers((KmlLayer)layer);
            //    }
            //}
        }

        public KmlDialog RenderServerImage()
        {
            KmlDialog dialog = new KmlDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                RenderServerImage(dialog.ServerName, dialog.VisibleLayers);
                return dialog;
            }
            return null;
        }

        public SimpleFillSymbol GetFillSymbol(System.Drawing.Color fillColor)
        {
            SimpleFillSymbol symbol = new SimpleFillSymbol();
            symbol.Color = fillColor;
            symbol.Outline.Color = System.Drawing.Color.Gray;
            symbol.Outline.Width = 1;
            return symbol;
        }

        #region ILayerProvider Members

        public void CloseLayer()
        {
            KmlLayer graphicsLayer = _map.OperationalLayers[layerId.ToString()] as KmlLayer;
            if (graphicsLayer != null)
            {
                _map.OperationalLayers.Remove(graphicsLayer);
            }
        }

        #endregion
    }
}
