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
        private Map myMap;
        private Guid layerId;
        private string url;
        private int[] visibleLayers;
        //private List<GraphicsLayer> graphicsLayers;

        public KmlLayerProvider(Map myMap)
        {
            this.myMap = myMap;
            this.layerId = Guid.NewGuid();
            //graphicsLayers = new List<GraphicsLayer>();
        }

        public void Refresh()
        {
            KmlLayer layer = myMap.OperationalLayers[layerId.ToString()] as KmlLayer;
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

            KmlLayer shapeLayer = myMap.OperationalLayers[layerId.ToString()] as KmlLayer;
            if (shapeLayer != null)
            {
                myMap.OperationalLayers.Remove(shapeLayer);
            }

            shapeLayer = new KmlLayer(new Uri(url));
            shapeLayer.Id = layerId.ToString();
            //shapeLayer.Initialized += new EventHandler<EventArgs>(shapeLayer_Initialized);
            if (visibleLayers != null)
            {
                //shapeLayer.VisibleLayers = visibleLayers;
            }
            myMap.OperationalLayers.Add(shapeLayer);

            //myMap.Extent = shapeLayer.FullExtent;
        }

        void shapeLayer_Initialized(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            KmlLayer shapeLayer = myMap.OperationalLayers[layerId.ToString()] as KmlLayer;

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
            KmlLayer graphicsLayer = myMap.OperationalLayers[layerId.ToString()] as KmlLayer;
            if (graphicsLayer != null)
            {
                myMap.OperationalLayers.Remove(graphicsLayer);
            }
        }

        #endregion
    }
}
