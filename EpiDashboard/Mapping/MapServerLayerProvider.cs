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

    public class MapServerLayerProvider : ILayerProvider
    {
        private Map myMap;
        private Guid layerId;
        private string url;
        private int[] visibleLayers;

        public MapServerLayerProvider(Map myMap)
        {
            this.myMap = myMap;
            this.layerId = Guid.NewGuid();
        }

        public void Refresh()
        {
            Layer layer = myMap.OperationalLayers[layerId.ToString()] as Layer;
            if (layer != null)
            {
                myMap.OperationalLayers.Remove(layer);
                RenderServerImage(this.url, this.visibleLayers);
            }
        }

        public void MoveUp()
        {
            Layer layer = myMap.OperationalLayers[layerId.ToString()];
            int currentIndex = myMap.OperationalLayers.IndexOf(layer);
            if (currentIndex < myMap.OperationalLayers.Count - 1)
            {
                myMap.OperationalLayers.Remove(layer);
                myMap.OperationalLayers.Insert(currentIndex + 1, layer);
            }
        }

        public void MoveDown()
        {
            Layer layer = myMap.OperationalLayers[layerId.ToString()];
            int currentIndex = myMap.OperationalLayers.IndexOf(layer);
            if (currentIndex > 1)
            {
                myMap.OperationalLayers.Remove(layer);
                myMap.OperationalLayers.Insert(currentIndex - 1, layer);
            }
        }

        public void RenderServerImage(string url, int[] visibleLayers)
        {
            this.url = url;

            Layer shapeLayer = myMap.OperationalLayers[layerId.ToString()] as Layer;
            if (shapeLayer != null)
            {
                myMap.OperationalLayers.Remove(shapeLayer);
            }

            //'shapeLayer = new Layer()
            //shapeLayer.Id = layerId.ToString();
            //if (visibleLayers != null)
            //{
            //    //'shapeLayer.VisibleLayers = visibleLayers;
            //}
            //myMap.OperationalLayers.Add(shapeLayer);

            //myMap.FullExtent = shapeLayer.FullExtent;
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
            Layer graphicsLayer = myMap.OperationalLayers[layerId.ToString()] as Layer;
            if (graphicsLayer != null)
            {
                myMap.OperationalLayers.Remove(graphicsLayer);
            }
        }

        #endregion
    }
}
