using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;


namespace EpiDashboard.Mapping
{
    public delegate void FeatureLoadedHandler(string serverName, IDictionary<string, object> featureAttributes);

    public class ChoroplethServerLayerProvider : ChoroplethLayerProvider , IChoroLayerProvider
    {
        public ChoroplethServerLayerProvider(MapView clientMapView) : base(clientMap)
        {
        }
        
        private bool _flagUpdateToGraphicsLayerFailed;
        public bool FlagUpdateToGLFailed
        {
            get { return _flagUpdateToGraphicsLayerFailed; }
            set { _flagUpdateToGraphicsLayerFailed = value; }
        }

        public event FeatureLoadedHandler FeatureLoaded;

        List<List<SolidColorBrush>> ColorList = new List<List<SolidColorBrush>>();

        override public object[] Load()
        {
            MapServerFeatureDialog dialog = new MapServerFeatureDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return Load(dialog.ServerName + "/" + dialog.VisibleLayer);
            }
            
            return null;
        }

        override public object[] Load(string boundrySourceLocation)
        {
            if (!string.IsNullOrEmpty(boundrySourceLocation))
            {
                FeatureLayer graphicsLayer = ArcGIS_MapView.Layers[LayerId.ToString()] as FeatureLayer;
                if (graphicsLayer != null)
                {
                    ArcGIS_MapView.Layers.Remove(graphicsLayer);
                }

                graphicsLayer = new FeatureLayer();
                graphicsLayer.ID = LayerId.ToString();
                graphicsLayer.UpdateCompleted += new EventHandler(graphicsLayer_UpdateCompleted);
                graphicsLayer.Initialized += new EventHandler<EventArgs>(graphicsLayer_Initialized);
                graphicsLayer.InitializationFailed += new EventHandler<EventArgs>(graphicsLayer_InitializationFailed);
                graphicsLayer.UpdateFailed += new EventHandler<TaskFailedEventArgs>(graphicsLayer_UpdateFailed);

                //if (boundrySourceLocation.Split('/')[0].Equals("NationalMap.gov - New York County Boundaries") || boundrySourceLocation.Equals("http://services.nationalmap.gov/ArcGIS/rest/services/govunits/MapServer/13"))
                //{
                //    graphicsLayer.Url = "http://services.nationalmap.gov/ArcGIS/rest/services/govunits/MapServer/13";
                //    graphicsLayer.OutFields.Add("STATE_FIPSCODE");
                //    graphicsLayer.OutFields.Add("COUNTY_NAME");
                //    graphicsLayer.Where = "STATE_FIPSCODE = '36'";
                //    ArcGIS_MapView.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(-80, 45.1)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(-71, 40)));
                //}
                //else if (boundrySourceLocation.Split('/')[0].Equals("NationalMap.gov - Rhode Island Zip Code Boundaries") || boundrySourceLocation.Equals("http://services.nationalmap.gov/ArcGIS/rest/services/govunits/MapServer/19"))
                //{
                //    graphicsLayer.Url = "http://services.nationalmap.gov/ArcGIS/rest/services/govunits/MapServer/19";
                //    graphicsLayer.OutFields.Add("STATE_FIPSCODE");
                //    graphicsLayer.OutFields.Add("NAME");
                //    graphicsLayer.Where = "STATE_FIPSCODE = '44'";
                //    ArcGIS_MapView.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(-72, 42.03)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(-71, 41)));
                //}
                //else if (boundrySourceLocation.Split('/')[0].Equals("NationalMap.gov - U.S. State Boundaries") || boundrySourceLocation.Equals("http://services.nationalmap.gov/ArcGIS/rest/services/govunits/MapServer/17"))
                //{
                //    graphicsLayer.Url = "http://services.nationalmap.gov/ArcGIS/rest/services/govunits/MapServer/17";
                //    graphicsLayer.OutFields.Add("STATE_FIPSCODE");
                //    graphicsLayer.OutFields.Add("STATE_ABBR");
                //    graphicsLayer.OutFields.Add("STATE_NAME");
                //    ArcGIS_MapView.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(-196, 72)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(-61, 14.5)));
                //}
                //else if (boundrySourceLocation.Split('/')[0].Equals("NationalMap.gov - World Boundaries"))
                //{
                //    graphicsLayer.Url = "http://services.nationalmap.gov/ArcGIS/rest/services/TNM_Blank_US/MapServer/17";
                //    ArcGIS_MapView.Extent = graphicsLayer.FullExtent;
                //}
                //else
                //{
                    graphicsLayer.Url = boundrySourceLocation;
                //}

                ArcGIS_MapView.Layers.Add(graphicsLayer);
                ArcGIS_MapView.Cursor = Cursors.Wait;

                return new object[] { boundrySourceLocation, graphicsLayer};
            }
            else
            {
                return null;
            }
        }

        void graphicsLayer_UpdateFailed(object sender, TaskFailedEventArgs e)
        {
            ArcGIS_MapView.Cursor = Cursors.Arrow;
            _flagUpdateToGraphicsLayerFailed = true;
        }

        void graphicsLayer_InitializationFailed(object sender, EventArgs e)
        {
            ArcGIS_MapView.Cursor = Cursors.Arrow;
        }

        void graphicsLayer_Initialized(object sender, EventArgs e)
        {
            //FeatureLayer graphicsLayer = __mapView.Map.Layers[layerId.ToString()] as FeatureLayer;
            //int x = 5;
            //x++;
        }

        void graphicsLayer_UpdateCompleted(object sender, EventArgs e)
        {
            FeatureLayer graphicsLayer = ArcGIS_MapView.Layers[LayerId.ToString()] as FeatureLayer;
            _flagUpdateToGraphicsLayerFailed = false;
            
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
            
            ArcGIS_MapView.Cursor = Cursors.Arrow;
        }

        override public string GetShapeValue(Graphic graphicFeature, string shapeValue)
        {
            shapeValue = graphicFeature.Attributes[_shapeKey].ToString().Replace("'", "''").Trim();
            return shapeValue;
        }

        override public GraphicsLayer GetGraphicsLayer()
        {
            GraphicsLayer graphicsLayer = ArcGIS_MapView.Layers[LayerId.ToString()] as GraphicsLayer;
            return graphicsLayer;
        }
    }
}
