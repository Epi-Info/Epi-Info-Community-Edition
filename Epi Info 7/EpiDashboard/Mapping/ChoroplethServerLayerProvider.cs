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
        float[] quantileValues = new float[] { 5, 5, 5, 5, 5, 5, 5, 5, 5, 5 };
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
                
        private double dist(Point px, Point po, Point pf)
        {
            double d = Math.Sqrt((px.Y - po.Y) * (px.Y - po.Y) + (px.X - po.X) * (px.X - po.X));
            if (((px.Y < po.Y) && (pf.Y > po.Y)) || ((px.Y > po.Y) && (pf.Y < po.Y)) || ((px.Y == po.Y) && (px.X < po.X) && (pf.X > po.X)) || ((px.Y == po.Y) && (px.X > po.X) && (pf.X < po.X)))
            {
                d = -d;
            }
            return d;
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

        public void ResetRangeValues(string toString, string s, string toString1, int classCount)
        {
            _classCount = classCount;
            _shapeKey = shapeKey;
            _dataKey = dataKey;

            DataTable loadedData = GetLoadedData(_dashboardHelper, _dataKey, ref valueField);
            GraphicsLayer graphicsLayer = ArcGIS_Map.Layers[_layerId.ToString()] as GraphicsLayer;

            if (graphicsLayer == null) return;

            _thematicItem = GetThematicItem(_classCount, loadedData, graphicsLayer);
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
            //FeatureLayer graphicsLayer = _myMap.Layers[layerId.ToString()] as FeatureLayer;
            //int x = 5;
            //x++;
            ArcGIS_Map.Cursor = Cursors.Arrow;
            flagupdatetoglfailed = true;

        }

        void graphicsLayer_InitializationFailed(object sender, EventArgs e)
        {
            //FeatureLayer graphicsLayer = _myMap.Layers[layerId.ToString()] as FeatureLayer;
            //int x = 5;
            //x++;
            ArcGIS_Map.Cursor = Cursors.Arrow;

        }

        void graphicsLayer_Initialized(object sender, EventArgs e)
        {
            //FeatureLayer graphicsLayer = _myMap.Layers[layerId.ToString()] as FeatureLayer;
            //int x = 5;
            //x++;
        }

        //private void GenerateMap()
        //{
        //    EpiDashboard.Controls.ChoroplethProperties choroplethprop = new Controls.ChoroplethProperties(this.mapControl as StandaloneMapControl, this._myMap);

        //    //  choroplethprop.txtShapePath.Text = shapeFilePath;
        //    choroplethprop.txtProjectPath.Text = _dashboardHelper.Database.DataSource;
        //    choroplethprop.SetDashboardHelper(_dashboardHelper);
        //    choroplethprop.cmbClasses.Text = cbxClasses.Text;
        //    foreach (string str in cbxShapeKey.Items) { choroplethprop.cmbShapeKey.Items.Add(str); }
        //    foreach (string str in cbxDataKey.Items) { choroplethprop.cmbDataKey.Items.Add(str); }
        //    foreach (string str in cbxValue.Items) { choroplethprop.cmbValue.Items.Add(str); }
        //    choroplethprop.cmbShapeKey.SelectedItem = cbxShapeKey.Text;
        //    choroplethprop.cmbDataKey.SelectedItem = cbxDataKey.Text;
        //    choroplethprop.cmbValue.SelectedItem = cbxValue.Text;
        //    choroplethprop.rctHighColor.Fill = rctHighColor.Fill;
        //    choroplethprop.rctLowColor.Fill = rctLowColor.Fill;
        //    choroplethprop.rctMissingColor.Fill = rctMissingColor.Fill;
        //    choroplethprop.radShapeFile.IsChecked = true;

        //    choroplethprop.choroplethShapeLayerProvider = provider;
        //    choroplethprop.thisProvider = provider;

        //    choroplethprop.legTitle.Text = provider.LegendText;
        //    choroplethprop.SetDefaultRanges();
        //    choroplethprop.GetRangeValues(provider.RangeCount);
        //    choroplethprop.ListLegendText = provider.ListLegendText;
        //    //  choroplethprop.CustomColors = provider.CustomColors;     
        //    choroplethprop.RenderMap();
        //}    

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

        public void ResetRangeValues()
        {
            string valueField = this.valueField;

            DataTable loadedData = GetLoadedData(_dashboardHelper, dataKey, ref valueField);
            GraphicsLayer graphicsLayer = ArcGIS_Map.Layers[layerId.ToString()] as GraphicsLayer;

            if (graphicsLayer == null) return;

            ThematicItem thematicItem = GetThematicItem(classCount, loadedData, graphicsLayer);
        }

        public ThematicItem GetThematicItem(int classCount, DataTable loadedData, GraphicsLayer graphicsLayer)
        {
            ThematicItem thematicItem = new ThematicItem()
            {
                Name = this.dataKey,
                Description = this.dataKey,
                CalcField = ""
            };

            List<double> valueList = new List<double>();
            if (graphicsLayer != null)
            {
                for (int i = 0; i < graphicsLayer.Graphics.Count; i++)
                {
                    Graphic graphicFeature = graphicsLayer.Graphics[i];

                    string filterExpression = "";

                    if (dataKey.Contains(" ") || dataKey.Contains("$") || dataKey.Contains("#"))
                    {
                        filterExpression += "[";
                    }

                    filterExpression += dataKey;

                    if (dataKey.Contains(" ") || dataKey.Contains("$") || dataKey.Contains("#"))
                    {
                        filterExpression += "]";
                    }

                    filterExpression += " = '" + Convert.ToString(graphicFeature.Attributes[shapeKey]).Replace("'", "''").Trim() + "'";
                    double graphicValue = Double.PositiveInfinity;
                    try
                    {
                        graphicValue = Convert.ToDouble(loadedData.Select(filterExpression)[0][valueField]);
                    }
                    catch
                    {
                        graphicValue = Double.PositiveInfinity;
                    }

                    string graphicName = Convert.ToString(graphicFeature.Attributes[shapeKey]);

                    if (i == 0)
                    {
                        thematicItem.Min = Double.PositiveInfinity;
                        thematicItem.Max = Double.NegativeInfinity;
                        thematicItem.MinName = string.Empty;
                        thematicItem.MaxName = string.Empty;
                    }
                    else
                    {
                        if (graphicValue < thematicItem.Min) { thematicItem.Min = graphicValue; thematicItem.MinName = graphicName; }
                        if (graphicValue > thematicItem.Max && graphicValue != Double.PositiveInfinity) { thematicItem.Max = graphicValue; thematicItem.MaxName = graphicName; }
                    }

                    if (graphicValue < Double.PositiveInfinity)
                    {
                        valueList.Add(graphicValue);
                    }
                }
            }

            valueList.Sort();

            thematicItem.RangeStarts = CalculateThematicRange(classCount, thematicItem, valueList);

            return thematicItem;
        }

        public void PopulateRangeValues(DashboardHelper dashboardHelper, string shapeKey, string dataKey, string valueField, List<SolidColorBrush> colors, int classCount)
        {
            this.classCount = classCount;
            this._dashboardHelper = dashboardHelper;
            this.shapeKey = shapeKey;
            this.dataKey = dataKey;
            this.valueField = valueField;

            DataTable loadedData = GetLoadedData(dashboardHelper, dataKey, ref valueField);

            GraphicsLayer graphicsLayer = ArcGIS_Map.Layers[layerId.ToString()] as GraphicsLayer;
            ThematicItem thematicItem = GetThematicItem(classCount, loadedData, graphicsLayer);
            RangeCount = thematicItem.RangeStarts.Count;


            if (Range != null && Range.Count > 0)
            {
                thematicItem.RangeStarts = Range;
            }
            Array.Clear(RangeValues, 0, RangeValues.Length);
            for (int i = 0; i < thematicItem.RangeStarts.Count; i++)
            {
                RangeValues[i, 0] = thematicItem.RangeStarts[i].ToString();

                if (i < thematicItem.RangeStarts.Count - 1)
                {
                    RangeValues[i, 1] = thematicItem.RangeStarts[i + 1].ToString();
                }
                else
                {
                    RangeValues[i, 1] = thematicItem.Max.ToString();
                }
            }
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

        public void SetShapeRangeValues(DashboardHelper dashboardHelper, string shapeKey, string dataKey, string valueField, List<SolidColorBrush> colors, int classCount, string missingText)
        {
            try
            {
                _classCount = classCount;
                _dashboardHelper = dashboardHelper;
                _shapeKey = shapeKey;
                _dataKey = dataKey;
                _valueField = valueField;
                _colors = colors;
                _missingText = missingText;

                DataTable loadedData = GetLoadedData(dashboardHelper, dataKey, ref valueField);

                GraphicsLayer graphicsLayer = ArcGIS_Map.Layers[_layerId.ToString()] as GraphicsLayer;

                _thematicItem = GetThematicItem(classCount, loadedData, graphicsLayer);

                if (Range != null &&
                    Range.Count > 0)
                {
                    _thematicItem.RangeStarts = Range;
                    PopulateRangeValues();
                }
                else
                {
                    _thematicItem.RangeStarts = new List<double>() { classCount };
                    PopulateRangeValues();
                }


                if (graphicsLayer.Graphics != null && graphicsLayer.Graphics.Count > 0)
                {

                    for (int i = 0; i < graphicsLayer.Graphics.Count; i++)
                    {
                        Graphic graphicFeature = graphicsLayer.Graphics[i];

                        //string filterExpression = dataKey + " = '" + graphicFeature.Attributes[shapeKey].ToString().Trim() + "'";
                        string filterExpression = "";
                        if (dataKey.Contains(" ") || dataKey.Contains("$") || dataKey.Contains("#"))
                            filterExpression += "[";
                        filterExpression += dataKey;
                        if (dataKey.Contains(" ") || dataKey.Contains("$") || dataKey.Contains("#"))
                            filterExpression += "]";
                        filterExpression += " = '" + graphicFeature.Attributes[shapeKey].ToString().Replace("'", "''").Trim() + "'";

                        double graphicValue = Double.PositiveInfinity;
                        try
                        {
                            graphicValue = Convert.ToDouble(loadedData.Select(filterExpression)[0][valueField]);
                        }
                        catch (Exception)
                        {
                            graphicValue = Double.PositiveInfinity;
                        }

                        int brushIndex = GetRangeIndex(graphicValue, _thematicItem.RangeStarts);


                        SimpleFillSymbol symbol = new SimpleFillSymbol()
                        {
                            Fill = graphicValue == Double.PositiveInfinity ? colors[colors.Count - 1] : colors[brushIndex],
                            BorderBrush = new SolidColorBrush(Colors.Black),
                            BorderThickness = 1
                        };

                        graphicFeature.Symbol = symbol;

                        TextBlock t = new TextBlock();
                        t.Background = Brushes.White;
                        if (graphicValue == Double.PositiveInfinity)
                        {
                            t.Text = graphicFeature.Attributes[shapeKey].ToString().Trim() + " : No Data";
                        }
                        else
                        {
                            t.Text = graphicFeature.Attributes[shapeKey].ToString().Trim() + " : " + graphicValue.ToString();
                        }
                        t.FontSize = 14;
                        Border border = new Border();
                        border.BorderThickness = new Thickness(1);
                        Panel panel = new StackPanel();
                        panel.Children.Add(t);
                        border.Child = panel;

                        graphicFeature.MapTip = border;
                    }
                }


                SetLegendSection(colors, classCount, missingText, _thematicItem);

            }
            catch
            {
            }
        }


        public void PopulateRangeValues(DashboardHelper dashboardHelper, string shapeKey, string dataKey, string valueField, List<SolidColorBrush> colors, int classCount, string legendText)
        {
            _classCount = classCount;
            _dashboardHelper = dashboardHelper;
            _shapeKey = shapeKey;
            _dataKey = dataKey;
            _valueField = valueField;
            LegendText = legendText;
            _colors = colors;

            DataTable loadedData = GetLoadedData(dashboardHelper, dataKey, ref valueField);

            GraphicsLayer graphicsLayer = ArcGIS_Map.Layers[_layerId.ToString()] as GraphicsLayer;

            _thematicItem = GetThematicItem(_classCount, loadedData, graphicsLayer);

            if (Range != null &&
                Range.Count > 0)
            {
                _thematicItem.RangeStarts = Range;
            }

            PopulateRangeValues();
        }

        internal void SetRangeFromAttributeList(Dictionary<int, object> dictionary)
        {
            //if (classCount >= 2)
            //{
            //    Range.Add((double)dictionary[1])
            //}
        }


        #region ILayerProvider Members
        
        #endregion
    }
}
