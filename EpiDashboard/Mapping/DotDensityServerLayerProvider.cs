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
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Toolkit;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;
using Epi;
using Epi.Data;

namespace EpiDashboard.Mapping
{
    public class DotDensityServerLayerProvider : ILayerProvider
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
        private string url;
        private bool flagupdatetoglfailed;

        public DotDensityServerLayerProvider(Map myMap)
        {
            this.myMap = myMap;
            this.layerId = Guid.NewGuid();
        }

      /*  public void MoveUp()
        {
            Layer layer = myMap.Layers[layerId.ToString()];
            Layer dotLayer = myMap.Layers[layerId.ToString() + "_dotLayer"];
            int currentIndex = myMap.Layers.IndexOf(layer);
            int currentDotIndex = myMap.Layers.IndexOf(dotLayer);
            if (currentIndex < myMap.Layers.Count - 1)
            {
                myMap.Layers.Remove(layer);
                myMap.Layers.Insert(currentIndex + 1, layer);
                //int currentDotIndex = myMap.Layers.IndexOf(dotLayer);
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

        public object[] LoadShapeFile(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                FeatureLayer graphicsLayer = myMap.Layers[layerId.ToString()] as FeatureLayer;
                if (graphicsLayer != null)
                {
                    myMap.Layers.Remove(graphicsLayer);
                }
                graphicsLayer = new FeatureLayer();
                graphicsLayer.ID = layerId.ToString();
                graphicsLayer.UpdateCompleted += new EventHandler(graphicsLayer_UpdateCompleted);
                graphicsLayer.Initialized += new EventHandler<EventArgs>(graphicsLayer_Initialized);
                graphicsLayer.InitializationFailed += new EventHandler<EventArgs>(graphicsLayer_InitializationFailed);
                graphicsLayer.UpdateFailed += new EventHandler<TaskFailedEventArgs>(graphicsLayer_UpdateFailed);

                if (url.Split('/')[0].Equals("NationalMap.gov - New York County Boundaries") || url.Equals("http://services.nationalmap.gov/ArcGIS/rest/services/govunits/MapServer/13"))
                {
                    graphicsLayer.Url = "http://services.nationalmap.gov/ArcGIS/rest/services/govunits/MapServer/13";
                    graphicsLayer.OutFields.Add("STATE_FIPSCODE");
                    graphicsLayer.OutFields.Add("COUNTY_NAME");
                    graphicsLayer.Where = "STATE_FIPSCODE = '36'";
                    myMap.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(-80, 45.1)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(-71, 40)));
                }
                else if (url.Split('/')[0].Equals("NationalMap.gov - Rhode Island Zip Code Boundaries") || url.Equals("http://services.nationalmap.gov/ArcGIS/rest/services/govunits/MapServer/19"))
                {
                    graphicsLayer.Url = "http://services.nationalmap.gov/ArcGIS/rest/services/govunits/MapServer/19";
                    graphicsLayer.OutFields.Add("STATE_FIPSCODE");
                    graphicsLayer.OutFields.Add("NAME");
                    graphicsLayer.Where = "STATE_FIPSCODE = '44'";
                    myMap.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(-72, 42.03)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(-71, 41)));
                }
                else if (url.Split('/')[0].Equals("NationalMap.gov - U.S. State Boundaries") || url.Equals("http://services.nationalmap.gov/ArcGIS/rest/services/govunits/MapServer/17"))
                {
                    graphicsLayer.Url = "http://services.nationalmap.gov/ArcGIS/rest/services/govunits/MapServer/17";
                    graphicsLayer.OutFields.Add("STATE_FIPSCODE");
                    graphicsLayer.OutFields.Add("STATE_ABBR");
                    graphicsLayer.OutFields.Add("STATE_NAME");
                    myMap.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(-196, 72)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(-61, 14.5)));
                }
                else if (url.Split('/')[0].Equals("NationalMap.gov - World Boundaries"))
                {
                    graphicsLayer.Url = "http://services.nationalmap.gov/ArcGIS/rest/services/TNM_Blank_US/MapServer/17";
                    myMap.Extent = graphicsLayer.FullExtent;
                }
                else
                {
                    graphicsLayer.Url = url;
                }
                myMap.Layers.Add(graphicsLayer);
                myMap.Cursor = Cursors.Wait;
                return new object[] { graphicsLayer };
            }
            else
                return null;
        }

        public object[] LoadShapeFile()
        {
            MapServerFeatureDialog dialog = new MapServerFeatureDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                object[] graphicslayer= null;
               graphicslayer= LoadShapeFile(dialog.ServerName + "/" + dialog.VisibleLayer);
               return graphicslayer;
            }
            else return null;
        }

        void graphicsLayer_UpdateFailed(object sender, TaskFailedEventArgs e)
        {
            //FeatureLayer graphicsLayer = myMap.Layers[layerId.ToString()] as FeatureLayer;
            //int x = 5;
            //x++;
            myMap.Cursor = Cursors.Arrow;
            flagupdatetoglfailed = true;
        }

        void graphicsLayer_InitializationFailed(object sender, EventArgs e)
        {
            //FeatureLayer graphicsLayer = myMap.Layers[layerId.ToString()] as FeatureLayer;
            //int x = 5;
            //x++;
            myMap.Cursor = Cursors.Arrow;
        }

        void graphicsLayer_Initialized(object sender, EventArgs e)
        {
            //FeatureLayer graphicsLayer = myMap.Layers[layerId.ToString()] as FeatureLayer;
            //int x = 5;
            //x++;
        }

        void graphicsLayer_UpdateCompleted(object sender, EventArgs e)
        {
            FeatureLayer graphicsLayer = myMap.Layers[layerId.ToString()] as FeatureLayer;
            flagupdatetoglfailed = false;
            if (graphicsLayer != null)
            {
                if (graphicsLayer.Graphics.Count > 0)
                {
                    if (FeatureLoaded != null)
                    {
                        FeatureLoaded(graphicsLayer.Url, graphicsLayer.Graphics[0].Attributes);
                    }
                }
            }
            myMap.Cursor = Cursors.Arrow;
        }

        public SimpleFillSymbol GetFillSymbol(SolidColorBrush brush)
        {
            SimpleFillSymbol symbol = new SimpleFillSymbol();
            symbol.Fill = brush;
            symbol.BorderBrush = new SolidColorBrush(Colors.Gray);
            symbol.BorderThickness = 1;
            return symbol;
        }

        public bool FlagUpdateToGLFailed
        {
            get { return flagupdatetoglfailed; }
            set { flagupdatetoglfailed = value; }
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
                SimpleMarkerSymbol symbol = new SimpleMarkerSymbol();
                symbol.Color = new SolidColorBrush(dotColor);
                symbol.Size = 5;
                symbol.Style = SimpleMarkerSymbol.SimpleMarkerStyle.Circle;
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


                GraphicsLayer graphicsLayer = myMap.Layers[layerId.ToString()] as GraphicsLayer;

                List<double> valueList = new List<double>();
                List<Graphic> graphicsToBeAdded = new List<Graphic>();
                for (int i = 0; i < graphicsLayer.Graphics.Count; i++)
                {
                    Graphic graphicFeature = graphicsLayer.Graphics[i];

                    double xmin = graphicFeature.Geometry.Extent.XMin;
                    double xmax = graphicFeature.Geometry.Extent.XMax;
                    double ymin = graphicFeature.Geometry.Extent.YMin;
                    double ymax = graphicFeature.Geometry.Extent.YMax;

                    Random rnd = new Random();

                    string shapeValue = graphicFeature.Attributes[shapeKey].ToString().Replace("'", "''").Trim();

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
                                graphic.Geometry = new MapPoint(rndX, rndY, graphicFeature.Geometry.SpatialReference);
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
                                        catch (Exception ex)
                                        {
                                            pointInGraphic = false;
                                        }

                                    }
                                }
                            }
                            graphicsToBeAdded.Add(graphic);
                        }
                    }
                    catch (Exception ex)
                    {
                        graphicValue = Double.PositiveInfinity;
                    }
                }

                SimpleRenderer renderer = new SimpleRenderer();
                renderer.Symbol = new SimpleFillSymbol()
                {
                    Fill = new SolidColorBrush(Colors.Transparent),
                    BorderBrush = new SolidColorBrush(Colors.Black),
                    BorderThickness = 1
                };

                graphicsLayer.Renderer = renderer;

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
            Layer shapeLayer = myMap.Layers[layerId.ToString()] as Layer;
            if (shapeLayer != null)
            {
                myMap.Layers.Remove(shapeLayer);
            }
            GraphicsLayer dotLayer = myMap.Layers[layerId.ToString() + "_dotLayer"] as GraphicsLayer;
            if (dotLayer != null)
            {
                myMap.Layers.Remove(dotLayer);
            }
        }

        #endregion
    }
}
