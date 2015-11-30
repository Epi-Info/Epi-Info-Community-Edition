using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;

namespace EpiDashboard.Mapping
{
    public delegate void FeatureLoadedHandler(string serverName, IDictionary<string, object> featureAttributes);

    public class ChoroplethServerLayerProvider : ChoroplethLayerProvider , IChoroLayerProvider
    {
        public ChoroplethServerLayerProvider(Map clientMap) : base(clientMap)
        {
            ArcGIS_Map = clientMap;
            this._layerId = Guid.NewGuid();
        }

        private string shapeKey;
        private string dataKey;
        private string valueField;
        public Guid layerId;

        private int classCount;

        string[,] rangeValues = new string[,] { { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" } };
        private bool flagupdatetoglfailed;


        private ListLegendTextDictionary _listLegendText = new ListLegendTextDictionary();
        private CustomColorsDictionary _customColorsDictionary = new CustomColorsDictionary();
        private ClassRangesDictionary _classRangesDictionary = new ClassRangesDictionary();

        public event FeatureLoadedHandler FeatureLoaded;

        List<List<SolidColorBrush>> ColorList = new List<List<SolidColorBrush>>();


        public bool FlagUpdateToGLFailed
        {
            get { return flagupdatetoglfailed; }
            set { flagupdatetoglfailed = value; }
        }

        public object[] LoadShapeFile(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                FeatureLayer graphicsLayer = ArcGIS_Map.Layers[layerId.ToString()] as FeatureLayer;
                if (graphicsLayer != null)
                {
                    ArcGIS_Map.Layers.Remove(graphicsLayer);
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
                    ArcGIS_Map.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(-80, 45.1)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(-71, 40)));
                }
                else if (url.Split('/')[0].Equals("NationalMap.gov - Rhode Island Zip Code Boundaries") || url.Equals("http://services.nationalmap.gov/ArcGIS/rest/services/govunits/MapServer/19"))
                {
                    graphicsLayer.Url = "http://services.nationalmap.gov/ArcGIS/rest/services/govunits/MapServer/19";
                    graphicsLayer.OutFields.Add("STATE_FIPSCODE");
                    graphicsLayer.OutFields.Add("NAME");
                    graphicsLayer.Where = "STATE_FIPSCODE = '44'";
                    ArcGIS_Map.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(-72, 42.03)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(-71, 41)));
                }
                else if (url.Split('/')[0].Equals("NationalMap.gov - U.S. State Boundaries") || url.Equals("http://services.nationalmap.gov/ArcGIS/rest/services/govunits/MapServer/17"))
                {
                    graphicsLayer.Url = "http://services.nationalmap.gov/ArcGIS/rest/services/govunits/MapServer/17";
                    graphicsLayer.OutFields.Add("STATE_FIPSCODE");
                    graphicsLayer.OutFields.Add("STATE_ABBR");
                    graphicsLayer.OutFields.Add("STATE_NAME");
                    ArcGIS_Map.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(-196, 72)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(-61, 14.5)));
                }
                else if (url.Split('/')[0].Equals("NationalMap.gov - World Boundaries"))
                {
                    graphicsLayer.Url = "http://services.nationalmap.gov/ArcGIS/rest/services/TNM_Blank_US/MapServer/17";
                    ArcGIS_Map.Extent = graphicsLayer.FullExtent;
                }
                else
                {
                    graphicsLayer.Url = url;
                }
                ArcGIS_Map.Layers.Add(graphicsLayer);
                ArcGIS_Map.Cursor = Cursors.Wait;
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
                object[] graphicslayer = null;
                graphicslayer = LoadShapeFile(dialog.ServerName + "/" + dialog.VisibleLayer);
                return graphicslayer;
            }
            else return null;
        }

        void graphicsLayer_UpdateFailed(object sender, TaskFailedEventArgs e)
        {
            ArcGIS_Map.Cursor = Cursors.Arrow;
            flagupdatetoglfailed = true;
        }

        void graphicsLayer_InitializationFailed(object sender, EventArgs e)
        {
            ArcGIS_Map.Cursor = Cursors.Arrow;
        }

        void graphicsLayer_Initialized(object sender, EventArgs e)
        {
            //FeatureLayer graphicsLayer = _myMap.Layers[layerId.ToString()] as FeatureLayer;
            //int x = 5;
            //x++;
        }

        void graphicsLayer_UpdateCompleted(object sender, EventArgs e)
        {
            FeatureLayer graphicsLayer = ArcGIS_Map.Layers[layerId.ToString()] as FeatureLayer;
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
            ArcGIS_Map.Cursor = Cursors.Arrow;
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
            if (_dashboardHelper != null)
            {
                SetShapeRangeValues(_dashboardHelper, _shapeKey, _dataKey, _valueField, _colors, _classCount, _missingText);
            }
        }

        override public string GetShapeValue(Graphic graphicFeature, string shapeValue)
        {
            shapeValue = graphicFeature.Attributes[_shapeKey].ToString().Replace("'", "''").Trim();
            return shapeValue;
        }

        #region ILayerProvider Members

        public void CloseLayer()
        {
            GraphicsLayer graphicsLayer = ArcGIS_Map.Layers[layerId.ToString()] as GraphicsLayer;
            if (graphicsLayer != null)
            {
                ArcGIS_Map.Layers.Remove(graphicsLayer);
                if (LegendStackPanel != null)
                {
                    LegendStackPanel.Children.Clear();
                }
            }
        }

        #endregion

        internal void SetRangeFromAttributeList(Dictionary<int, object> dictionary)
        {
        }

        #region ILayerProvider Members
        
        #endregion
    }
}
