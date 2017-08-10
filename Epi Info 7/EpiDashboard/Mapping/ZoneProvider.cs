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

    public class ZoneProvider : ILayerProvider
    {
        private MapView _mapView;
        private Guid layerId;
        MapPoint point;
        double radius;
        Brush zoneColor;
        string units;


        public ZoneProvider(MapView  mapView, MapPoint point)
        {
            _mapView = mapView;
            this.point = point;
            this.layerId = Guid.NewGuid();
        }

        public void Refresh()
        {
            GraphicsLayer markerLayer = _mapView.Map.Layers[layerId.ToString()] as GraphicsLayer;
            if (markerLayer != null)
            {
                markerLayer.Graphics.Clear();
                RenderZone(this.radius, this.zoneColor, this.units);
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

        public void RenderZone(double radius, Brush zoneColor, string units)
        {
            this.radius = radius;
            this.zoneColor = zoneColor;
            this.units = units;

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

            AddBufferedPoint();
        }

        private void AddBufferedPoint()
        {
            GraphicsLayer graphicsLayer = _mapView.Map.Layers[layerId.ToString()] as GraphicsLayer;

            point.SpatialReference = _mapView.SpatialReference;
            Graphic graphic = new ESRI.ArcGIS.Client.Graphic()
            {
                Geometry = point,
                Symbol = SimplePointSymbol
            };
            graphic.SetZIndex(1);
            graphicsLayer.Graphics.Add(graphic);

            GeometryService geometryService =
              new GeometryService("http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Geometry/GeometryServer");
            geometryService.BufferCompleted += GeometryService_BufferCompleted;
            geometryService.Failed += GeometryService_Failed;

            // If buffer spatial reference is GCS and unit is linear, geometry service will do geodesic buffering
            BufferParameters bufferParams = new BufferParameters()
            {
                Unit = StandardUnit,
                BufferSpatialReference = new SpatialReference(4326),
                OutSpatialReference = myMap.SpatialReference
            };
            bufferParams.Features.Add(graphic);
            bufferParams.Distances.Add(radius);

            geometryService.BufferAsync(bufferParams);
        }

        private LinearUnit StandardUnit
        {
            get
            {
                switch (units)
                {
                    case "Kilometer": return LinearUnit.Kilometer;
                    case "Meter": return LinearUnit.Meter;
                    case "Mile": return LinearUnit.StatuteMile;
                    case "Yard": return LinearUnit.InternationalYard;
                    case "Foot": return LinearUnit.Foot;
                    default: return LinearUnit.StatuteMile;
                }
            }
        }

        void GeometryService_BufferCompleted(object sender, GraphicsEventArgs args)
        {
            IList<Graphic> results = args.Results;
            GraphicsLayer graphicsLayer = _mapView.Map.Layers[layerId.ToString()] as GraphicsLayer;

            foreach (Graphic graphic in results)
            {
                graphic.Symbol = new SimpleFillSymbol() { Fill = zoneColor, BorderThickness = 0 };
                graphicsLayer.Graphics.Add(graphic);
            }
        }

        private void GeometryService_Failed(object sender, TaskFailedEventArgs e)
        {
            Logger.Log("Geometry Service error: " + e.Error);
        }

        private Symbol SimplePointSymbol
        {
            get
            {
                SimpleMarkerSymbol symbol = new SimpleMarkerSymbol();
                symbol.Color = Brushes.Black;
                symbol.Size = 3;
                symbol.Style = SimpleMarkerSymbol.SimpleMarkerStyle.Circle;
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
