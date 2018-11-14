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
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;

namespace EpiDashboard.Mapping
{
    public class DotDensityKmlLayerProvider : ILayerProvider
    {
        public event FeatureLoadedHandler FeatureLoaded;

        #region DotDensity

        private Map myMap;
        private DashboardHelper dashboardHelper;
        private string shapeKey;
        private string dataKey;
        private string valueField;
        public Guid layerId;
        private Color dotColor;
        private int dotValue;
        private List<GraphicsLayer> graphicsLayers;
        private string url;

        public DotDensityKmlLayerProvider(Map myMap)
        {
            this.myMap = myMap;
            this.layerId = Guid.NewGuid();
            graphicsLayers = new List<GraphicsLayer>();
        }

       /* public void MoveUp()
        {
            Layer layer = myMap.Layers[layerId.ToString()];
            Layer dotLayer = myMap.Layers[layerId.ToString() + "_dotLayer"];
            int currentIndex = myMap.Layers.IndexOf(layer);
            int currentDotIndex = myMap.Layers.IndexOf(dotLayer);
            if (currentIndex < myMap.Layers.Count - 1)
            {
                myMap.Layers.Remove(layer);
                myMap.Layers.Insert(currentIndex + 1, layer);
                // int currentDotIndex = myMap.Layers.IndexOf(dotLayer);
                myMap.Layers.Remove(dotLayer);
                myMap.Layers.Insert(currentDotIndex + 1, dotLayer);
            }
        }*/

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

       /* public void MoveDown()
        {
            Layer layer = myMap.Layers[layerId.ToString()];
            Layer dotLayer = myMap.Layers[layerId.ToString() + "_dotLayer"];
            int currentIndex = myMap.Layers.IndexOf(layer);
            int currentDotIndex = myMap.Layers.IndexOf(dotLayer);
            if (currentIndex > 1)
            {
                myMap.Layers.Remove(layer);
                myMap.Layers.Insert(currentIndex - 1, layer);
                myMap.Layers.Remove(dotLayer);
                myMap.Layers.Insert(currentDotIndex - 1, dotLayer);
            }
        }*/

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

        public object[] LoadKml()
        {
            KmlDialog dialog = new KmlDialog();
            object[] kmlfile = null;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                kmlfile=LoadKml(dialog.ServerName);
                return kmlfile;
            }
            else return null;
        }

        public object[] LoadKml(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                this.url = url;
                KmlLayer shapeLayer = myMap.Layers[layerId.ToString()] as KmlLayer;
                if (shapeLayer != null)
                {
                    myMap.Layers.Remove(shapeLayer);
                }

                shapeLayer = new KmlLayer();
                shapeLayer.ID = layerId.ToString();
                shapeLayer.Url = new Uri(url);
                shapeLayer.Initialized += new EventHandler<EventArgs>(shapeLayer_Initialized);
                myMap.Layers.Add(shapeLayer);

                myMap.Extent = shapeLayer.FullExtent;
                return new object[] { shapeLayer };
            }
            else return null;
        }

        void shapeLayer_Initialized(object sender, EventArgs e)
        {
            KmlLayer shapeLayer = myMap.Layers[layerId.ToString()] as KmlLayer;
            Dictionary<string, object> allAttributes = new Dictionary<string, object>();

            FindGraphicsLayers(shapeLayer);
            if (graphicsLayers.Count > 0)
            {
                IDictionary<string, object> coreAttributes = graphicsLayers[0].Graphics[0].Attributes;
                foreach (KeyValuePair<string, object> attr in coreAttributes)
                {
                    allAttributes.Add(attr.Key, attr.Value);
                }
                if (graphicsLayers[0].Graphics[0].Attributes.ContainsKey("extendedData"))
                {
                    List<KmlExtendedData> eds = (List<KmlExtendedData>)graphicsLayers[0].Graphics[0].Attributes["extendedData"];
                    foreach (KmlExtendedData ed in eds)
                    {
                        allAttributes.Add(ed.Name, ed.Value);
                    }
                }
            }

            double xmin = graphicsLayers[0].Graphics[0].Geometry.Extent.XMin;
            double xmax = graphicsLayers[0].Graphics[0].Geometry.Extent.XMax;
            double ymin = graphicsLayers[0].Graphics[0].Geometry.Extent.YMin;
            double ymax = graphicsLayers[0].Graphics[0].Geometry.Extent.YMax;
            foreach (Graphic g in graphicsLayers[0].Graphics)
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

            myMap.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(xmin - 0.5, ymax + 0.5)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(xmax + 0.5, ymin - 0.5)));
            if (FeatureLoaded != null)
            {
                FeatureLoaded(url, allAttributes);
            }
        }

        private void FindGraphicsLayers(KmlLayer kmlLayer)
        {
            foreach (Layer layer in kmlLayer.ChildLayers)
            {
                if (layer is GraphicsLayer)
                {
                    graphicsLayers.Add((GraphicsLayer)layer);
                }
                else if (layer is KmlLayer)
                {
                    FindGraphicsLayers((KmlLayer)layer);
                }
            }
        }


        public SimpleFillSymbol GetFillSymbol(SolidColorBrush brush)
        {
            SimpleFillSymbol symbol = new SimpleFillSymbol();
            symbol.Fill = brush;
            symbol.BorderBrush = new SolidColorBrush(Colors.Gray);
            symbol.BorderThickness = 1;
            return symbol;
        }



        public void Refresh()
        {
            if (dashboardHelper != null)
            {
                SetShapeRangeValues(dashboardHelper, shapeKey, dataKey, valueField, dotColor, dotValue);
            }
        }

        private Symbol MarkerSymbol
        {
            get
            {
                Esri.ArcGISRuntime.Symbology.SimpleMarkerSymbol symbol = new Esri.ArcGISRuntime.Symbology.SimpleMarkerSymbol();
                symbol.Color = new SolidColorBrush(dotColor);
                symbol.Size = 5;
                symbol.Style = Esri.ArcGISRuntime.Symbology.SimpleMarkerSymbolStyle.Circle;
                return symbol;
            }
        }

        public void SetShapeRangeValues(DashboardHelper dashboardHelper, string shapeKey, string dataKey, string valueField, Color dotColor, int dotValue)
        {
            try
            {
                this.dotValue = dotValue;
                this.dashboardHelper = dashboardHelper;
                this.shapeKey = shapeKey;
                this.dataKey = dataKey;
                this.valueField = valueField;
                this.dotColor = dotColor;

                List<string> columnNames = new List<string>();
                if (dashboardHelper.IsUsingEpiProject)
                {
                    columnNames.Add("UniqueKey");
                }
                columnNames.Add(valueField);
                columnNames.Add(dataKey);

                DataTable loadedData;

                if (valueField.Equals("{Record Count}"))
                {
                    GadgetParameters gadgetOptions = new GadgetParameters();
                    gadgetOptions.MainVariableName = dataKey;

                    Dictionary<string, string> inputVariableList = new Dictionary<string, string>();
                    inputVariableList.Add("freqvar", dataKey);
                    inputVariableList.Add("allvalues", "false");
                    inputVariableList.Add("showconflimits", "false");
                    inputVariableList.Add("showcumulativepercent", "false");
                    inputVariableList.Add("includemissing", "false");
                    inputVariableList.Add("maxrows", "500");

                    gadgetOptions.InputVariableList = inputVariableList;
                    loadedData = dashboardHelper.GenerateFrequencyTableforMap(gadgetOptions).First().Key;
                    
                    foreach (DataRow dr in loadedData.Rows)
                    {
                        dr[0] = dr[0].ToString().Trim();
                    }
                    
                    valueField = "freq";
                }
                else
                {
                    loadedData = dashboardHelper.GenerateTable(columnNames);
                }


                GraphicsLayer graphicsLayer = graphicsLayers[0];

                List<double> valueList = new List<double>();
                List<Graphic> graphicsToBeAdded = new List<Graphic>();
                SpatialReference geoReference = new SpatialReference(4326);
                for (int i = 0; i < graphicsLayer.Graphics.Count; i++)
                {
                    Graphic graphicFeature = graphicsLayer.Graphics[i];

                    double xmin = graphicFeature.Geometry.Extent.XMin;
                    double xmax = graphicFeature.Geometry.Extent.XMax;
                    double ymin = graphicFeature.Geometry.Extent.YMin;
                    double ymax = graphicFeature.Geometry.Extent.YMax;

                    Random rnd = new Random();

                    string shapeValue = string.Empty;

                    if (!graphicFeature.Attributes.ContainsKey(shapeKey))
                    {
                        List<KmlExtendedData> eds = (List<KmlExtendedData>)graphicFeature.Attributes["extendedData"];
                        foreach (KmlExtendedData ed in eds)
                        {
                            if (ed.Name.Equals(shapeKey))
                            {
                                shapeValue = ed.Value.Replace("'", "''").Trim();
                            }
                        }
                    }
                    else
                    {
                        shapeValue = graphicFeature.Attributes[shapeKey].ToString().Replace("'", "''").Trim();
                    }
                    
                    string filterExpression = "";
                    if (dataKey.Contains(" ") || dataKey.Contains("$") || dataKey.Contains("#"))
                        filterExpression += "[";
                    filterExpression += dataKey;
                    if (dataKey.Contains(" ") || dataKey.Contains("$") || dataKey.Contains("#"))
                        filterExpression += "]";
                    filterExpression += " = '" + shapeValue + "'";

                    double graphicValue = Double.PositiveInfinity;
                    try
                    {
                        graphicValue = Convert.ToDouble(loadedData.Select(filterExpression)[0][valueField]);

                        for (int counter = 0; counter < graphicValue; counter += dotValue)
                        {
                            bool pointInGraphic = false;

                            ExtendedGraphic graphic = new ExtendedGraphic();
                            while (!pointInGraphic)
                            {
                                double rndX = xmin + (rnd.NextDouble() * (xmax - xmin));
                                double rndY = ymin + (rnd.NextDouble() * (ymax - ymin));
                                graphic.Geometry = new MapPoint(rndX, rndY, geoReference);
                                graphic.Symbol = MarkerSymbol;

                                bool foundBottomLeft = false;
                                bool foundBottomRight = false;
                                bool foundTopLeft = false;
                                bool foundTopRight = false;
                                foreach (ESRI.ArcGIS.Client.Geometry.PointCollection pc in ((ESRI.ArcGIS.Client.Geometry.Polygon)graphicFeature.Geometry).Rings)
                                {
                                    foundBottomLeft = pc.Any((point) => point.Y <= ((MapPoint)graphic.Geometry).Y && point.X <= ((MapPoint)graphic.Geometry).X);
                                    foundBottomRight = pc.Any((point) => point.Y <= ((MapPoint)graphic.Geometry).Y && point.X >= ((MapPoint)graphic.Geometry).X);
                                    foundTopLeft = pc.Any((point) => point.Y >= ((MapPoint)graphic.Geometry).Y && point.X <= ((MapPoint)graphic.Geometry).X);
                                    foundTopRight = pc.Any((point) => point.Y >= ((MapPoint)graphic.Geometry).Y && point.X >= ((MapPoint)graphic.Geometry).X);
                                    if (foundBottomLeft && foundBottomRight && foundTopLeft && foundTopRight)
                                    {
                                        try
                                        {
                                            MapPoint firstBottomLeft = pc.First((point) => point.Y <= ((MapPoint)graphic.Geometry).Y && point.X <= ((MapPoint)graphic.Geometry).X);
                                            MapPoint firstBottomRight = pc.First((point) => point.Y <= ((MapPoint)graphic.Geometry).Y && point.X >= ((MapPoint)graphic.Geometry).X);
                                            MapPoint firstTopLeft = pc.First((point) => point.Y >= ((MapPoint)graphic.Geometry).Y && point.X <= ((MapPoint)graphic.Geometry).X);
                                            MapPoint firstTopRight = pc.First((point) => point.Y >= ((MapPoint)graphic.Geometry).Y && point.X >= ((MapPoint)graphic.Geometry).X);

                                            int indexBL = pc.IndexOf(firstBottomLeft);
                                            int indexBR = pc.IndexOf(firstBottomRight);
                                            int indexTL = pc.IndexOf(firstTopLeft);
                                            int indexTR = pc.IndexOf(firstTopRight);

                                            MapPoint lastBottomLeft = pc.Last((point) => point.Y <= ((MapPoint)graphic.Geometry).Y && point.X <= ((MapPoint)graphic.Geometry).X);
                                            MapPoint lastBottomRight = pc.Last((point) => point.Y <= ((MapPoint)graphic.Geometry).Y && point.X >= ((MapPoint)graphic.Geometry).X);
                                            MapPoint lastTopLeft = pc.Last((point) => point.Y >= ((MapPoint)graphic.Geometry).Y && point.X <= ((MapPoint)graphic.Geometry).X);
                                            MapPoint lastTopRight = pc.Last((point) => point.Y >= ((MapPoint)graphic.Geometry).Y && point.X >= ((MapPoint)graphic.Geometry).X);

                                            int indexBL2 = pc.IndexOf(lastBottomLeft);
                                            int indexBR2 = pc.IndexOf(lastBottomRight);
                                            int indexTL2 = pc.IndexOf(lastTopLeft);
                                            int indexTR2 = pc.IndexOf(lastTopRight);

                                            if ((Math.Abs(indexTL - indexTR2) == 1 && Math.Abs(indexTR - indexBR2) == 1) || (Math.Abs(indexBL - indexTL2) == 1 && Math.Abs(indexTL - indexTR2) == 1) || (Math.Abs(indexBR - indexBL2) == 1 && Math.Abs(indexTR - indexBR2) == 1))
                                            {
                                                pointInGraphic = true;
                                                break;
                                            }
                                            else if ((Math.Abs(indexBL - indexBR2) == 1 && Math.Abs(indexTL - indexBL2) == 1) || (Math.Abs(indexTL - indexBL2) == 1 && Math.Abs(indexTR - indexTL2) == 1) || (Math.Abs(indexBR - indexTR2) == 1 && Math.Abs(indexTR - indexTL2) == 1))
                                            {
                                                pointInGraphic = true;
                                                break;
                                            }
                                        }
                                        catch
                                        {
                                            pointInGraphic = false;
                                        }
                                    }
                                }
                            }
                            graphicsToBeAdded.Add(graphic);
                        }

                    }
                    catch
                    {
                        graphicValue = Double.PositiveInfinity;
                    }
                }

                foreach (Graphic g in graphicsLayer.Graphics)
                {
                    SimpleFillSymbol symbol = new SimpleFillSymbol()
                    {
                        Fill = new SolidColorBrush(Colors.Transparent),
                        BorderBrush = new SolidColorBrush(Colors.Black),
                        BorderThickness = 1
                    };

                    g.Symbol = symbol;
                }

                GraphicsLayer dotLayer = myMap.Layers[layerId.ToString() + "_dotLayer"] as GraphicsLayer;
                int currentDotIndex = myMap.Layers.Count;
                if (dotLayer != null)
                {
                    currentDotIndex = myMap.Layers.IndexOf(dotLayer);
                    myMap.Layers.Remove(dotLayer);
                }
                dotLayer = new GraphicsLayer();
                dotLayer.ID = layerId.ToString() + "_dotLayer";
                myMap.Layers.Insert(currentDotIndex, dotLayer);
                foreach (Graphic g in graphicsToBeAdded)
                {
                    dotLayer.Graphics.Add(g);
                }

            }
            catch (Exception ex)
            {
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            throw new NotImplementedException();
        }

        private StackPanel legendStackPanel;

        public StackPanel LegendStackPanel
        {
            get
            {
                return legendStackPanel;
            }
            set
            {
                legendStackPanel = value;
            }
        }

        #endregion

        #region ILayerProvider Members

        public void CloseLayer()
        {
            KmlLayer shapeLayer = myMap.Layers[layerId.ToString()] as KmlLayer;
            if (shapeLayer != null)
            {
                myMap.Layers.Remove(shapeLayer);
            }
            GraphicsLayer dotLayer = myMap.Layers[layerId.ToString() + "_dotLayer"] as GraphicsLayer;
            if (dotLayer != null)
            {
                myMap.Layers.Remove(dotLayer);
            }
            if (LegendStackPanel != null)
            {
                LegendStackPanel.Children.Clear();
            }
        }

        #endregion
    }
}
