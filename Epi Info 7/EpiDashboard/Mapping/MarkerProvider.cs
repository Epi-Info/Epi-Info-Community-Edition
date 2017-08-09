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

    public class MarkerProvider : ILayerProvider
    {
        private MapView _mapView;
        private Guid layerId;
        MapPoint point;
        int size; 
        Brush markerColor;
        SimpleMarkerStyle style;


        public MarkerProvider(MapView mapView, MapPoint point)
        {
            this._mapView = mapView;
            this.point = point;
            this.layerId = Guid.NewGuid();
        }

        public void Refresh()
        {
            GraphicsLayer markerLayer = _mapView.Map.Layers[layerId.ToString()] as GraphicsLayer;
            if (markerLayer != null)
            {
                markerLayer.Graphics.Clear();
                RenderMarker(this.size, this.markerColor, this.style);
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

        public void RenderMarker(int size, Brush markerColor, SimpleMarkerStyle style)
        {
            this.size = size;
            this.markerColor = markerColor;
            this.style = style;

            GraphicsLayer markerLayer = _mapView.Map.Layers[layerId.ToString()] as GraphicsLayer;
            if (markerLayer != null)
            {
                markerLayer.Graphics.Clear();
            }
            else
            {
                markerLayer = new GraphicsLayer();
                markerLayer.ID = layerId.ToString();
                _mapView.Map.Layers.Add(markerLayer);
            }

            markerLayer.Graphics.Add(new Graphic() { Geometry = point, Symbol = MarkerSymbol });
        }

        private Symbol MarkerSymbol
        {
            get
            {
                SimpleMarkerSymbol symbol = new SimpleMarkerSymbol();
                symbol.Color = ((SolidColorBrush)markerColor).Color;
                symbol.Size = size;
                symbol.Style = style;
                return symbol;
            }
        }

        #region ILayerProvider Members

        public void CloseLayer()
        {
            GraphicsLayer graphicsLayer = _mapView.Map.Layers[layerId.ToString()] as GraphicsLayer;
            if (graphicsLayer != null)
            {
                _mapView.Map.Layers.Remove(graphicsLayer);
            }
        }

        #endregion
    }
}
