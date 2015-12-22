using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Toolkit.DataSources;
using ESRI.ArcGIS.Client.Toolkit.DataSources.Kml;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;

namespace EpiDashboard.Mapping
{
    public class ChoroplethKmlLayerProvider : ChoroplethLayerProvider, IChoroLayerProvider
    {
        public event FeatureLoadedHandler FeatureLoaded;

        string _kmlURL;

        public ChoroplethKmlLayerProvider(Map clientMap) : base(clientMap)
        {
            ArcGIS_Map = clientMap;
            this._layerId = Guid.NewGuid();
        }

        sealed override public object[] Load()
        {
            KmlDialog dialog = new KmlDialog();
            
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return Load(dialog.ServerName);
            }

            return null;
        }

        sealed override public object[] Load(string boundrySourceLocation)
        {
            if (!string.IsNullOrEmpty(boundrySourceLocation))
            {
                _kmlURL = boundrySourceLocation;
                KmlLayer shapeLayer = ArcGIS_Map.Layers[_layerId.ToString()] as KmlLayer;
                
                if (shapeLayer != null)
                {
                    ArcGIS_Map.Layers.Remove(shapeLayer);
                }
                
                shapeLayer = new KmlLayer();
                shapeLayer.ID = _layerId.ToString();
                shapeLayer.Url = new Uri(boundrySourceLocation);
                shapeLayer.Initialized += new EventHandler<EventArgs>(shapeLayer_Initialized);
                ArcGIS_Map.Layers.Add(shapeLayer);

                ArcGIS_Map.Extent = shapeLayer.FullExtent;
                return new object[] { shapeLayer };
            }
            else 
            { 
                return null; 
            }
        }

        void shapeLayer_Initialized(object sender, EventArgs e)
        {
            KmlLayer shapeLayer = ArcGIS_Map.Layers[_layerId.ToString()] as KmlLayer;
            GraphicsLayer graphicsLayer = GetGraphicsLayer(shapeLayer);

            if (graphicsLayer != null)
            {
                IDictionary<string, object> coreAttributes = graphicsLayer.Graphics[0].Attributes;
                Dictionary<string, object> allAttributes = new Dictionary<string, object>();
                
                foreach (KeyValuePair<string, object> attr in coreAttributes)
                {
                    allAttributes.Add(attr.Key, attr.Value);
                }

                AddSchemaDataAttributes(_kmlURL, graphicsLayer.Graphics);

                if (graphicsLayer.Graphics[0].Attributes.ContainsKey("extendedData"))
                {
                    List<KmlExtendedData> eds = (List<KmlExtendedData>)graphicsLayer.Graphics[0].Attributes["extendedData"];
                    
                    foreach (KmlExtendedData ed in eds)
                    {
                        allAttributes.Add(ed.Name, ed.Value);
                    }
                }

                if (FeatureLoaded != null)
                {
                    FeatureLoaded(_kmlURL, allAttributes);
                }
            }

            double xmin = graphicsLayer.Graphics[0].Geometry.Extent.XMin;
            double xmax = graphicsLayer.Graphics[0].Geometry.Extent.XMax;
            double ymin = graphicsLayer.Graphics[0].Geometry.Extent.YMin;
            double ymax = graphicsLayer.Graphics[0].Geometry.Extent.YMax;

            foreach (Graphic g in graphicsLayer.Graphics)
            {
                if (g.Geometry.Extent.XMin < xmin)
                    xmin = g.Geometry.Extent.XMin;

                if (g.Geometry.Extent.YMin < ymin)
                    ymin = g.Geometry.Extent.YMin;

                if (g.Geometry.Extent.XMax > xmax)
                    xmax = g.Geometry.Extent.XMax;

                if (g.Geometry.Extent.YMax > ymax)
                    ymax = g.Geometry.Extent.YMax;
            }

            ArcGIS_Map.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(xmin - 0.5, ymax + 0.5)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(xmax + 0.5, ymin - 0.5)));
        }

        private void AddSchemaDataAttributes(string _kmlURL, GraphicCollection graphics)
        {
            try
            {
                Dictionary<string, Dictionary<string, string>> extendedData = new Dictionary<string, Dictionary<string, string>>();

                using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(_kmlURL))
                {
                    while (reader.ReadToFollowing("Placemark"))
                    {
                        string description = string.Empty;
                        Dictionary<string, string> extendedData_SimpleData = new Dictionary<string, string>();

                        if (reader.ReadToFollowing("description"))
                        {
                            description = reader.ReadElementString();
                        }

                        if (reader.ReadToFollowing("SimpleData"))
                        {
                            while (reader.ReadToNextSibling("SimpleData"))
                            {

                                if (reader.MoveToFirstAttribute())
                                {
                                    extendedData_SimpleData.Add(reader.Value, reader.ReadElementString());
                                }
                            }
                        }

                        if (extendedData.ContainsKey(description) == false)
                        {
                            extendedData.Add(description, extendedData_SimpleData);
                        }
                    }
                }

                foreach (Graphic graphic in graphics)
                {
                    if (graphic.Attributes.ContainsKey("name"))
                    {
                        string name = (string)graphic.Attributes["name"];
                        string description = (string)graphic.Attributes["description"];

                        if (graphic.Attributes.ContainsKey("extendedData") == false)
                        {
                            graphic.Attributes.Add("extendedData", new List<KmlExtendedData>());
                        }

                        if (extendedData.ContainsKey(description))
                        {
                            foreach (KeyValuePair<string, string> kvp in extendedData[description])
                            {
                                KmlExtendedData datum = new KmlExtendedData();
                                datum.DisplayName = kvp.Key;
                                datum.Name = kvp.Key;
                                datum.Value = kvp.Value;
                                ((List<KmlExtendedData>)graphic.Attributes["extendedData"]).Add(datum);
                            }
                        }
                    }
                }
            }
            catch { }
        }

        override public string GetShapeValue(Graphic graphicFeature, string shapeValue)
        {
            if (!graphicFeature.Attributes.ContainsKey(_shapeKey))
            {
                List<KmlExtendedData> eds = (List<KmlExtendedData>)graphicFeature.Attributes["extendedData"];
                foreach (KmlExtendedData ed in eds)
                {
                    if (ed.Name.Equals(_shapeKey))
                    {
                        shapeValue = ed.Value.Replace("'", "''").Trim();
                        break;
                    }
                }
            }
            else
            {
                shapeValue = graphicFeature.Attributes[_shapeKey].ToString().Replace("'", "''").Trim();
            }

            return shapeValue;
        }

        override public GraphicsLayer GetGraphicsLayer()
        {
            KmlLayer kmlLayer = ArcGIS_Map.Layers[_layerId.ToString()] as KmlLayer;
            GraphicsLayer graphicsLayer = GetGraphicsLayer(kmlLayer as Layer);
            return graphicsLayer;
        }

        private GraphicsLayer GetGraphicsLayer(Layer givenLayer)
        {
            if(givenLayer is GroupLayer)
            {
                foreach (Layer layer in ((GroupLayer)givenLayer).ChildLayers)
                {
                    if (layer is GraphicsLayer)
                    {
                        return (GraphicsLayer)layer;
                    }
                    else if (layer is Layer)
                    {
                        return GetGraphicsLayer((Layer)layer);
                    }
                }
            }
            
            return null;
        }

        override public void CloseLayer()
        {
            KmlLayer shapeLayer = ArcGIS_Map.Layers[_layerId.ToString()] as KmlLayer;
            if (shapeLayer != null)
            {
                ArcGIS_Map.Layers.Remove(shapeLayer);
                
                if (LegendStackPanel != null)
                {
                    LegendStackPanel.Children.Clear();
                }
            }
        }
    }
}
