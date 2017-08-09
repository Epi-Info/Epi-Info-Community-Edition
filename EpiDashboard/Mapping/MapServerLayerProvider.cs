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

using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;

using Epi;
using Epi.Data;
using EpiDashboard.Mapping.ShapeFileReader;
using System.Windows.Controls.DataVisualization.Charting;

namespace EpiDashboard.Mapping
{

    public class MapServerLayerProvider : ILayerProvider
    {
        private MapView _mapView;
        private Guid layerId;
        private string url;
        private int[] visibleLayers;

        public MapServerLayerProvider(MapView mapView)
        {
            this._mapView = mapView;
            this.layerId = Guid.NewGuid();
        }

        public void Refresh()
        {
            ArcGISDynamicMapServiceLayer layer = this._mapView.Map.Layers[layerId.ToString()] as ArcGISDynamicMapServiceLayer;
            if (layer != null)
            {
                _mapView.Map.Layers.Remove(layer);
                RenderServerImage(this.url, this.visibleLayers);
            }
        }

        public void MoveUp()
        {
            Layer layer = _mapView.Map.Layers[layerId.ToString()];
            int currentIndex = _mapView.Map.Layers.IndexOf(layer);
            if (currentIndex < _mapView.Map.Layers.Count - 1)
            {
                _mapView.Map.Layers.Remove(layer);
                _mapView.Map.Layers.Insert(currentIndex + 1, layer);
            }
        }

        public void MoveDown()
        {
            Layer layer = _mapView.Map.Layers[layerId.ToString()];
            int currentIndex = _mapView.Map.Layers.IndexOf(layer);
            if (currentIndex > 1)
            {
                _mapView.Map.Layers.Remove(layer);
                _mapView.Map.Layers.Insert(currentIndex - 1, layer);
            }
        }

        public void RenderServerImage(string url, int[] visibleLayers)
        {
            this.url = url;

            ArcGISDynamicMapServiceLayer shapeLayer = _mapView.Map.Layers[layerId.ToString()] as ArcGISDynamicMapServiceLayer;
            if (shapeLayer != null)
            {
                _mapView.Map.Layers.Remove(shapeLayer);
            }

            shapeLayer = new ArcGISDynamicMapServiceLayer();
            shapeLayer.ID = layerId.ToString();
            shapeLayer.ServiceUri = url;
            if (visibleLayers != null)
            {
                //////shapeLayer.VisibleLayers = visibleLayers;
            }
            _mapView.Map.Layers.Add(shapeLayer);

            //////_mapView.Extent = shapeLayer.FullExtent;
        }

        public MapServerDialog RenderServerImage()
        {
            MapServerDialog dialog = new MapServerDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                RenderServerImage(dialog.ServerName, dialog.VisibleLayers);
                return dialog;
            }
            return null;
        }

        public SimpleFillSymbol GetFillSymbol(SolidColorBrush brush)
        {
            SimpleFillSymbol symbol = new SimpleFillSymbol();
            symbol.Color = brush.Color;
            symbol.Style = SimpleFillStyle.Solid;
            return symbol;
        }

        #region ILayerProvider Members

        public void CloseLayer()
        {
            ArcGISDynamicMapServiceLayer graphicsLayer = _mapView.Map.Layers[layerId.ToString()] as ArcGISDynamicMapServiceLayer;
            if (graphicsLayer != null)
            {
                _mapView.Map.Layers.Remove(graphicsLayer);
            }
        }

        #endregion
    }
}
