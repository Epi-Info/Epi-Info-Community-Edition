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

    public class KmlLayerProvider : ILayerProvider
    {
        private MapView _mapView;
        private Guid layerId;
        private string url;
        private int[] visibleLayers;
        private List<GraphicsLayer> graphicsLayers;

        public KmlLayerProvider(MapView mapView)
        {
            _mapView = mapView;
            this.layerId = Guid.NewGuid();
            graphicsLayers = new List<GraphicsLayer>();
        }

        public void Refresh()
        {
            KmlLayer layer = _mapView.Map.Layers[layerId.ToString()] as KmlLayer;
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

            KmlLayer shapeLayer = _mapView.Map.Layers[layerId.ToString()] as KmlLayer;
            if (shapeLayer != null)
            {
                _mapView.Map.Layers.Remove(shapeLayer);
            }

            shapeLayer = new KmlLayer();
            shapeLayer.ID = layerId.ToString();
            shapeLayer.SourceUri = url;
            ////////////////shapeLayer.Initialized += new EventHandler<EventArgs>(shapeLayer_Initialized);
            if (visibleLayers != null)
            {
                //shapeLayer.VisibleLayers = visibleLayers;
            }
            _mapView.Map.Layers.Add(shapeLayer);

            _mapView.SetView(shapeLayer.FullExtent);
        }

        void shapeLayer_Initialized(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            KmlLayer shapeLayer = _mapView.Map.Layers[layerId.ToString()] as KmlLayer;

            FindGraphicsLayers(shapeLayer);
            int x = 5;
            x++;
        }

        private void FindGraphicsLayers(KmlLayer kmlLayer)
        {
            ////////////foreach (Layer layer in kmlLayer.ChildLayers)
            ////////////{
            ////////////    if (layer is GraphicsLayer)
            ////////////    {
            ////////////        graphicsLayers.Add((GraphicsLayer)layer);
            ////////////    }
            ////////////    else if (layer is KmlLayer)
            ////////////    {
            ////////////        FindGraphicsLayers((KmlLayer)layer);
            ////////////    }
            ////////////}
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

        public SimpleFillSymbol GetFillSymbol(SolidColorBrush brush)
        {
            SimpleFillSymbol symbol = new SimpleFillSymbol()
            {
                Color = brush.Color,
                Style = SimpleFillStyle.Solid,
                Outline = new SimpleLineSymbol()
                {
                    Color = Colors.Gray,
                    Style = SimpleLineStyle.Solid,
                    Width = 1
                }
            };

            return symbol;
        }

        #region ILayerProvider Members

        public void CloseLayer()
        {
            KmlLayer graphicsLayer = _mapView.Map.Layers[layerId.ToString()] as KmlLayer;
            if (graphicsLayer != null)
            {
                _mapView.Map.Layers.Remove(graphicsLayer);
            }
        }

        #endregion
    }
}
