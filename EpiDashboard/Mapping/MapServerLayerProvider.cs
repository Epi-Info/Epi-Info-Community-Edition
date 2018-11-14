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
using Epi;
using Epi.Data;
using EpiDashboard.Mapping.ShapeFileReader;
using System.Windows.Controls.DataVisualization.Charting;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;

namespace EpiDashboard.Mapping
{

    public class MapServerLayerProvider : ILayerProvider
    {
        private Esri.ArcGISRuntime.Mapping.Map myMap;
        private Guid layerId;
        private string url;
        private int[] visibleLayers;

        public MapServerLayerProvider(Esri.ArcGISRuntime.Mapping.Map myMap)
        {
            this.myMap = myMap;
            this.layerId = Guid.NewGuid();
        }

        public void Refresh()
        {
            ArcGISDynamicMapServiceLayer layer = myMap.Layers[layerId.ToString()] as ArcGISDynamicMapServiceLayer;
            if (layer != null)
            {
                myMap.Layers.Remove(layer);
                RenderServerImage(this.url, this.visibleLayers);
            }
        }

        public void MoveUp()
        {
            Layer layer = myMap.Layers[layerId.ToString()];
            int currentIndex = myMap.Layers.IndexOf(layer);
            if (currentIndex < myMap.Layers.Count - 1)
            {
                myMap.Layers.Remove(layer);
                myMap.Layers.Insert(currentIndex + 1, layer);
            }
        }

        public void MoveDown()
        {
            Layer layer = myMap.Layers[layerId.ToString()];
            int currentIndex = myMap.Layers.IndexOf(layer);
            if (currentIndex > 1)
            {
                myMap.Layers.Remove(layer);
                myMap.Layers.Insert(currentIndex - 1, layer);
            }
        }

        public void RenderServerImage(string url, int[] visibleLayers)
        {
            this.url = url;

            ArcGISDynamicMapServiceLayer shapeLayer = myMap.Layers[layerId.ToString()] as ArcGISDynamicMapServiceLayer;
            if (shapeLayer != null)
            {
                myMap.Layers.Remove(shapeLayer);
            }

            shapeLayer = new ArcGISDynamicMapServiceLayer();
            shapeLayer.ID = layerId.ToString();
            shapeLayer.Url = url;
            if (visibleLayers != null)
            {
                shapeLayer.VisibleLayers = visibleLayers;
            }
            myMap.Layers.Add(shapeLayer);

            myMap.Extent = shapeLayer.FullExtent;
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
            symbol.Fill = brush;
            symbol.BorderBrush = new SolidColorBrush(Colors.Gray);
            symbol.BorderThickness = 1;
            return symbol;
        }

        #region ILayerProvider Members

        public void CloseLayer()
        {
            ArcGISDynamicMapServiceLayer graphicsLayer = myMap.Layers[layerId.ToString()] as ArcGISDynamicMapServiceLayer;
            if (graphicsLayer != null)
            {
                myMap.Layers.Remove(graphicsLayer);
            }
        }

        #endregion
    }
}
