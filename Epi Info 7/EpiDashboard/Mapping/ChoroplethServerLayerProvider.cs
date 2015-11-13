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

    public class ChoroplethServerLayerProvider : IChoroLayerProvider
    {
        #region Choropleth

        private Map _myMap;
        private DashboardHelper _dashboardHelper;
        private string shapeKey;
        private string dataKey;
        private string valueField;
        public Guid layerId;
        private Color lowColor;
        private Color highColor;
        private List<SolidColorBrush> colors;
        private int classCount;
        string missingText;
        string[,] rangeValues = new string[,] { { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" } };
        float[] quantileValues = new float[] { 5, 5, 5, 5, 5, 5, 5, 5, 5, 5 };
        private bool flagupdatetoglfailed;
        private bool _areRangesSet = false;
        public bool AreRangesSet { get { return _areRangesSet; } set { _areRangesSet = value; } }
        public ListLegendTextDictionary ListLegendText { get; set; }
        public bool RangesLoadedFromMapFile { get; set; }


        private ThematicItem _thematicItem;


        string _shapeKey;
        string _dataKey;
        string _valueField;
        string _missingText;
        List<SolidColorBrush> _colors;
        int _classCount;
        string[,] _rangeValues = new string[,] { { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" } };


        private ListLegendTextDictionary _listLegendText = new ListLegendTextDictionary();
        private CustomColorsDictionary _customColorsDictionary = new CustomColorsDictionary();
        private ClassRangesDictionary _classRangesDictionary = new ClassRangesDictionary();



        private List<double> _range;
        public List<double> Range
        {
            get { return _range; }
            set { _range = value; }
        }

        public event FeatureLoadedHandler FeatureLoaded;

        public ChoroplethServerLayerProvider(Map myMap)
        {
            this._myMap = myMap;
            this.layerId = Guid.NewGuid();
        }

        List<List<SolidColorBrush>> ColorList = new List<List<SolidColorBrush>>();
        int _colorShadeIndex = 0;

        int _lastGeneratedClassCount = 0;

        public string[,] RangeValues
        {
            get { return rangeValues; }
            set { rangeValues = value; }
        }

        public float[] QuantileValues
        {
            get { return quantileValues; }
            set { quantileValues = value; }
        }

        public bool UseCustomColors
        {
            get { return _useCustomColors; }
            set { _useCustomColors = value; }
        }

        public int RangeCount { get; set; }

        public bool FlagUpdateToGLFailed
        {
            get { return flagupdatetoglfailed; }
            set { flagupdatetoglfailed = value; }
        }

        public struct ThematicItem
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string CalcField { get; set; }
            public double Min { get; set; }
            public double Max { get; set; }
            public string MinName { get; set; }
            public string MaxName { get; set; }
            public List<double> RangeStarts { get; set; }

        }

        public void MoveUp()
        {
            Layer layer = _myMap.Layers[layerId.ToString()];
            int currentIndex = _myMap.Layers.IndexOf(layer);
            if (currentIndex < _myMap.Layers.Count - 1)
            {
                _myMap.Layers.Remove(layer);
                _myMap.Layers.Insert(currentIndex + 1, layer);
            }
        }

        public void MoveDown()
        {
            Layer layer = _myMap.Layers[layerId.ToString()];
            int currentIndex = _myMap.Layers.IndexOf(layer);
            if (currentIndex > 1)
            {
                _myMap.Layers.Remove(layer);
                _myMap.Layers.Insert(currentIndex - 1, layer);
            }
        }

        private Color GetColorAtPoint(Rectangle theRec, Point thePoint)
        {
            LinearGradientBrush br = (LinearGradientBrush)theRec.Fill;
            double y3 = thePoint.Y; double x3 = thePoint.X;
            double x1 = br.StartPoint.X * theRec.Width;
            double y1 = br.StartPoint.Y * theRec.Height;
            Point p1 = new Point(x1, y1);
            double x2 = br.EndPoint.X * theRec.Width;
            double y2 = br.EndPoint.Y * theRec.Height;
            Point p2 = new Point(x2, y2);
            Point p4 = new Point();
            if (y1 == y2)
            { p4 = new Point(x3, y1); }
            else if (x1 == x2)
            { p4 = new Point(x1, y3); }
            else
            {
                double m = (y2 - y1) / (x2 - x1);
                double m2 = -1 / m;
                double b = y1 - m * x1;
                double c = y3 - m2 * x3;
                double x4 = (c - b) / (m - m2);
                double y4 = m * x4 + b; p4 = new Point(x4, y4);
            }
            double d4 = dist(p4, p1, p2);
            double d2 = dist(p2, p1, p2);
            double x = d4 / d2;
            double max = br.GradientStops.Max(n => n.Offset);
            if (x > max) { x = max; }
            double min = br.GradientStops.Min(n => n.Offset);
            if (x < min) { x = min; }
            GradientStop gs0 = br.GradientStops.Where(n => n.Offset <= x).OrderBy(n => n.Offset).Last();
            GradientStop gs1 = br.GradientStops.Where(n => n.Offset >= x).OrderBy(n => n.Offset).First();
            float y = 0f;
            if (gs0.Offset != gs1.Offset)
            { y = (float)((x - gs0.Offset) / (gs1.Offset - gs0.Offset)); }
            Color cx = new Color();
            if (br.ColorInterpolationMode == ColorInterpolationMode.ScRgbLinearInterpolation)
            {
                float aVal = (gs1.Color.ScA - gs0.Color.ScA) * y + gs0.Color.ScA;
                float rVal = (gs1.Color.ScR - gs0.Color.ScR) * y + gs0.Color.ScR;
                float gVal = (gs1.Color.ScG - gs0.Color.ScG) * y + gs0.Color.ScG;
                float bVal = (gs1.Color.ScB - gs0.Color.ScB) * y + gs0.Color.ScB;
                cx = Color.FromScRgb(aVal, rVal, gVal, bVal);
            }
            else
            {
                byte aVal = (byte)((gs1.Color.A - gs0.Color.A) * y + gs0.Color.A);
                byte rVal = (byte)((gs1.Color.R - gs0.Color.R) * y + gs0.Color.R);
                byte gVal = (byte)((gs1.Color.G - gs0.Color.G) * y + gs0.Color.G);
                byte bVal = (byte)((gs1.Color.B - gs0.Color.B) * y + gs0.Color.B);
                cx = Color.FromArgb(aVal, rVal, gVal, bVal);
            }
            return cx;
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

        private void CreateColorList(Color lowColor, Color highColor)
        {
            LinearGradientBrush gradientBrush = new LinearGradientBrush(highColor, lowColor, 0);
            Rectangle temp = new Rectangle();
            temp.Width = 256;
            temp.Height = 256;
            temp.Fill = gradientBrush;

            ColorList = new List<List<SolidColorBrush>>();

            List<SolidColorBrush> BlueShades = new List<SolidColorBrush>();

            int rgbFactor = 255 / classCount;

            for (int j = 0; j < 256; j = j + rgbFactor)
            {
                Color color = GetColorAtPoint(temp, new Point(j, j));
                color.A = 0xF0;
                BlueShades.Add(new SolidColorBrush(color));
            }

            ColorList.Add(BlueShades);

            foreach (List<SolidColorBrush> brushList in ColorList)
            {
                brushList.Reverse();
            }

            _lastGeneratedClassCount = classCount;
        }

        private int GetRangeIndex(double val, List<double> ranges)
        {
            int index = classCount - 1;
            try
            {
                for (int r = 0; r < classCount - 1; r++)
                {
                    if (val >= ranges[r] && val < ranges[r + 1]) index = r;
                }
                return index;
            }

            catch (Exception ex)
            {
                return index;
            }

        }

        public struct Values
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Value { get; set; }
        }

        //private void ColorBlendCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //if (ColorBlendCombo != null)
        //{
        //    _colorShadeIndex = ColorBlendCombo.SelectedIndex;
        //    if (loadedData != null)
        //    {
        //        SetShapeRangeValues();
        //    }
        //}
        //}

        //private void ClassCountCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //if (ClassCountCombo != null)
        //{
        //    ComboBoxItem item = ClassCountCombo.SelectedItem as ComboBoxItem;
        //    _classCount = Convert.ToInt32(item.Content);
        //    if (loadedData != null)
        //    {
        //        SetShapeRangeValues();
        //    }
        //}
        //}

        public object[] LoadShapeFile(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                FeatureLayer graphicsLayer = _myMap.Layers[layerId.ToString()] as FeatureLayer;
                if (graphicsLayer != null)
                {
                    _myMap.Layers.Remove(graphicsLayer);
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
                    _myMap.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(-80, 45.1)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(-71, 40)));
                }
                else if (url.Split('/')[0].Equals("NationalMap.gov - Rhode Island Zip Code Boundaries") || url.Equals("http://services.nationalmap.gov/ArcGIS/rest/services/govunits/MapServer/19"))
                {
                    graphicsLayer.Url = "http://services.nationalmap.gov/ArcGIS/rest/services/govunits/MapServer/19";
                    graphicsLayer.OutFields.Add("STATE_FIPSCODE");
                    graphicsLayer.OutFields.Add("NAME");
                    graphicsLayer.Where = "STATE_FIPSCODE = '44'";
                    _myMap.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(-72, 42.03)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(-71, 41)));
                }
                else if (url.Split('/')[0].Equals("NationalMap.gov - U.S. State Boundaries") || url.Equals("http://services.nationalmap.gov/ArcGIS/rest/services/govunits/MapServer/17"))
                {
                    graphicsLayer.Url = "http://services.nationalmap.gov/ArcGIS/rest/services/govunits/MapServer/17";
                    graphicsLayer.OutFields.Add("STATE_FIPSCODE");
                    graphicsLayer.OutFields.Add("STATE_ABBR");
                    graphicsLayer.OutFields.Add("STATE_NAME");
                    _myMap.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(-196, 72)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(-61, 14.5)));
                }
                else if (url.Split('/')[0].Equals("NationalMap.gov - World Boundaries"))
                {
                    graphicsLayer.Url = "http://services.nationalmap.gov/ArcGIS/rest/services/TNM_Blank_US/MapServer/17";
                    _myMap.Extent = graphicsLayer.FullExtent;
                }
                else
                {
                    graphicsLayer.Url = url;
                }
                _myMap.Layers.Add(graphicsLayer);
                _myMap.Cursor = Cursors.Wait;
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
            GraphicsLayer graphicsLayer = _myMap.Layers[_layerId.ToString()] as GraphicsLayer;

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

        public string LegendText { get; set; }
        public bool UseCustomRanges { get; set; }

        void graphicsLayer_UpdateFailed(object sender, TaskFailedEventArgs e)
        {
            //FeatureLayer graphicsLayer = _myMap.Layers[layerId.ToString()] as FeatureLayer;
            //int x = 5;
            //x++;
            _myMap.Cursor = Cursors.Arrow;
            flagupdatetoglfailed = true;

        }

        void graphicsLayer_InitializationFailed(object sender, EventArgs e)
        {
            //FeatureLayer graphicsLayer = _myMap.Layers[layerId.ToString()] as FeatureLayer;
            //int x = 5;
            //x++;
            _myMap.Cursor = Cursors.Arrow;

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
            FeatureLayer graphicsLayer = _myMap.Layers[layerId.ToString()] as FeatureLayer;
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
            _myMap.Cursor = Cursors.Arrow;
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
                SetShapeRangeValues(_dashboardHelper, shapeKey, dataKey, valueField, colors, classCount, missingText);
            }
        }

        public Guid _layerId { get; set; }

        public void SetShapeRangeValues(DashboardHelper dashboardHelper, string shapeKey, string dataKey, string valueField, List<SolidColorBrush> colors, int classCount, string missingText)
        {
            try
            {
                this.classCount = classCount;
                this._dashboardHelper = dashboardHelper;
                this.shapeKey = shapeKey;
                this.dataKey = dataKey;
                this.valueField = valueField;
                this.lowColor = lowColor;
                this.highColor = highColor;
                this.missingText = missingText;

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
                    loadedData = dashboardHelper.GenerateFrequencyTable(gadgetOptions).First().Key;
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


                GraphicsLayer graphicsLayer = _myMap.Layers[layerId.ToString()] as GraphicsLayer;
                CreateColorList(lowColor, highColor);
                ThematicItem thematicItem = new ThematicItem() { Name = dataKey, Description = dataKey, CalcField = "" };
                List<double> valueList = new List<double>();
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

                    string graphicName = graphicFeature.Attributes[shapeKey].ToString();

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

                    if (graphicValue != Double.PositiveInfinity)
                    {
                        valueList.Add(graphicValue);
                    }
                }

                if (Range != null && Range.Count > 0)
                {
                    thematicItem.RangeStarts = Range;
                }
                else
                {
                    thematicItem.RangeStarts = new List<double>();

                    double totalRange = thematicItem.Max - thematicItem.Min;
                    double portion = totalRange / classCount;

                    thematicItem.RangeStarts.Add(thematicItem.Min);
                    double startRangeValue = thematicItem.Min;
                    IEnumerable<double> valueEnumerator =
                    from aValue in valueList
                    orderby aValue
                    select aValue;

                    if (valueList.Count > 0)
                    {
                        if (classCount == 2)
                        {
                            thematicItem.RangeStarts.Add(valueEnumerator.ElementAt(valueList.Count / 3));
                            thematicItem.RangeStarts.Add(valueEnumerator.ElementAt((valueList.Count * 2) / 3));
                        }
                        else if (classCount == 3)
                        {
                            thematicItem.RangeStarts.Add(valueEnumerator.ElementAt(valueList.Count / 4));
                            thematicItem.RangeStarts.Add(valueEnumerator.ElementAt((valueList.Count * 2) / 4));
                            thematicItem.RangeStarts.Add(valueEnumerator.ElementAt((valueList.Count * 3) / 4));
                        }
                        else if (classCount == 4)
                        {
                            thematicItem.RangeStarts.Add(valueEnumerator.ElementAt(valueList.Count / 5));
                            thematicItem.RangeStarts.Add(valueEnumerator.ElementAt((valueList.Count * 2) / 5));
                            thematicItem.RangeStarts.Add(valueEnumerator.ElementAt((valueList.Count * 3) / 5));
                            thematicItem.RangeStarts.Add(valueEnumerator.ElementAt((valueList.Count * 4) / 5));
                        }
                        else
                        {
                            thematicItem.RangeStarts.Add(valueEnumerator.ElementAt(valueList.Count / 6));
                            thematicItem.RangeStarts.Add(valueEnumerator.ElementAt((valueList.Count * 2) / 6));
                            thematicItem.RangeStarts.Add(valueEnumerator.ElementAt((valueList.Count * 3) / 6));
                            thematicItem.RangeStarts.Add(valueEnumerator.ElementAt((valueList.Count * 4) / 6));
                            thematicItem.RangeStarts.Add(valueEnumerator.ElementAt((valueList.Count * 5) / 6));
                        }
                    }
                }
                // Create graphic features and set symbol using the class range which contains the value 
                // List<SolidColorBrush> brushList = ColorList[_colorShadeIndex];
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
                        catch (Exception ex)
                        {
                            graphicValue = Double.PositiveInfinity;
                        }

                        if (graphicValue != Double.PositiveInfinity)
                        {
                            if (graphicFeature.Attributes.Keys.Contains("EpiInfoValCol"))
                            {
                                graphicFeature.Attributes["EpiInfoValCol"] = graphicValue;
                            }
                            else
                            {
                                graphicFeature.Attributes.Add("EpiInfoValCol", graphicValue);
                            }
                        }

                        int brushIndex = GetRangeIndex(graphicValue, thematicItem.RangeStarts);

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
                    ClassBreaksRenderer renderer = new ClassBreaksRenderer();
                    renderer.Field = "EpiInfoValCol";
                    renderer.DefaultSymbol = new SimpleFillSymbol()
                        {
                            Fill = colors[colors.Count - 1],
                            BorderBrush = new SolidColorBrush(Colors.Black),
                            BorderThickness = 1
                        };
                    renderer.Classes.Add(new ClassBreakInfo()
                    {
                        MinimumValue = thematicItem.Min,
                        MaximumValue = thematicItem.RangeStarts[1],
                        Symbol = new SimpleFillSymbol()
                            {
                                Fill = colors[0],
                                BorderBrush = new SolidColorBrush(Colors.Black),
                                BorderThickness = 1
                            }
                    });
                    renderer.Classes.Add(new ClassBreakInfo()
                    {
                        MinimumValue = thematicItem.RangeStarts[1],
                        MaximumValue = thematicItem.RangeStarts[2],
                        Symbol = new SimpleFillSymbol()
                        {
                            Fill = colors[1],
                            BorderBrush = new SolidColorBrush(Colors.Black),
                            BorderThickness = 1
                        }
                    });
                    if (classCount > 2)
                    {
                        double maxVal = 0.0, minVal = thematicItem.RangeStarts[2];
                        if (thematicItem.RangeStarts.Count == 3)
                        {
                            maxVal = thematicItem.Max;
                        }
                        else
                        {
                            maxVal = thematicItem.RangeStarts[3];
                        }
                        renderer.Classes.Add(new ClassBreakInfo()
                        {
                            MinimumValue = minVal,
                            MaximumValue = maxVal,
                            Symbol = new SimpleFillSymbol()
                            {
                                Fill = colors[2],
                                BorderBrush = new SolidColorBrush(Colors.Black),
                                BorderThickness = 1
                            }
                        });
                    }
                    if (classCount > 3)
                    {
                        double maxVal = 0.0, minVal = thematicItem.RangeStarts[3];
                        if (thematicItem.RangeStarts.Count == 4)
                        {
                            maxVal = thematicItem.Max;
                        }
                        else
                        {
                            maxVal = thematicItem.RangeStarts[4];
                        }
                        renderer.Classes.Add(new ClassBreakInfo()
                        {
                            MinimumValue = minVal,
                            MaximumValue = maxVal,
                            Symbol = new SimpleFillSymbol()
                            {
                                Fill = colors[3],
                                BorderBrush = new SolidColorBrush(Colors.Black),
                                BorderThickness = 1
                            }
                        });
                    }
                    if (classCount > 4)
                    {
                        double maxVal = 0.0, minVal = thematicItem.RangeStarts[4];
                        if (thematicItem.RangeStarts.Count == 5)
                        {
                            maxVal = thematicItem.Max;
                        }
                        else
                        {
                            maxVal = thematicItem.RangeStarts[5];
                        }
                        renderer.Classes.Add(new ClassBreakInfo()
                        {
                            MinimumValue = minVal,
                            MaximumValue = maxVal,
                            Symbol = new SimpleFillSymbol()
                            {
                                Fill = colors[4],
                                BorderBrush = new SolidColorBrush(Colors.Black),
                                BorderThickness = 1
                            }
                        });
                    }
                    if (classCount > 5)
                    {
                        double maxVal = 0.0, minVal = thematicItem.RangeStarts[5];
                        if (thematicItem.RangeStarts.Count == 6)
                        {
                            maxVal = thematicItem.Max;
                        }
                        else
                        {
                            maxVal = thematicItem.RangeStarts[6];
                        }

                        renderer.Classes.Add(new ClassBreakInfo()
                        {
                            MinimumValue = minVal,
                            MaximumValue = maxVal,
                            Symbol = new SimpleFillSymbol()
                            {
                                Fill = colors[5],
                                BorderBrush = new SolidColorBrush(Colors.Black),
                                BorderThickness = 1
                            }
                        });
                    }

                    if (classCount > 6)
                    {
                        double maxVal = 0.0, minVal = thematicItem.RangeStarts[6];
                        if (thematicItem.RangeStarts.Count == 7)
                        {
                            maxVal = thematicItem.Max;
                        }
                        else
                        {
                            maxVal = thematicItem.RangeStarts[7];
                        }

                        renderer.Classes.Add(new ClassBreakInfo()
                        {
                            MinimumValue = minVal,
                            MaximumValue = maxVal,
                            Symbol = new SimpleFillSymbol()
                            {
                                Fill = colors[6],
                                BorderBrush = new SolidColorBrush(Colors.Black),
                                BorderThickness = 1
                            }
                        });
                    }

                    if (classCount > 7)
                    {
                        double maxVal = 0.0, minVal = thematicItem.RangeStarts[7];
                        if (thematicItem.RangeStarts.Count == 8)
                        {
                            maxVal = thematicItem.Max;
                        }
                        else
                        {
                            maxVal = thematicItem.RangeStarts[8];
                        }

                        renderer.Classes.Add(new ClassBreakInfo()
                        {
                            MinimumValue = minVal,
                            MaximumValue = maxVal,
                            Symbol = new SimpleFillSymbol()
                            {
                                Fill = colors[7],
                                BorderBrush = new SolidColorBrush(Colors.Black),
                                BorderThickness = 1
                            }
                        });
                    }

                    if (classCount > 8)
                    {
                        double maxVal = 0.0, minVal = thematicItem.RangeStarts[8];
                        if (thematicItem.RangeStarts.Count == 9)
                        {
                            maxVal = thematicItem.Max;
                        }
                        else
                        {
                            maxVal = thematicItem.RangeStarts[9];
                        }

                        renderer.Classes.Add(new ClassBreakInfo()
                        {
                            MinimumValue = minVal,
                            MaximumValue = maxVal,
                            Symbol = new SimpleFillSymbol()
                            {
                                Fill = colors[8],
                                BorderBrush = new SolidColorBrush(Colors.Black),
                                BorderThickness = 1
                            }
                        });
                    }

                    if (classCount > 9)
                    {
                        double maxVal = 0.0, minVal = thematicItem.RangeStarts[9];
                        if (thematicItem.RangeStarts.Count == 7)
                        {
                            maxVal = thematicItem.Max;
                        }
                        else
                        {
                            maxVal = thematicItem.RangeStarts[7];
                        }

                        renderer.Classes.Add(new ClassBreakInfo()
                        {
                            MinimumValue = minVal,
                            MaximumValue = maxVal,
                            Symbol = new SimpleFillSymbol()
                            {
                                Fill = colors[9],
                                BorderBrush = new SolidColorBrush(Colors.Black),
                                BorderThickness = 1
                            }
                        });
                    }

                    renderer.Classes.Add(new ClassBreakInfo()
                    {
                        MinimumValue = thematicItem.RangeStarts[classCount - 1],
                        MaximumValue = thematicItem.Max,
                        Symbol = new SimpleFillSymbol()
                        {
                            Fill = colors[classCount],
                            BorderBrush = new SolidColorBrush(Colors.Black),
                            BorderThickness = 1
                        }
                    });

                    graphicsLayer.Renderer = renderer;
                }


                if (LegendStackPanel == null)
                {
                    LegendStackPanel = new StackPanel();
                }
                LegendStackPanel.Children.Clear();

                System.Windows.Controls.ListBox legendList = new System.Windows.Controls.ListBox();
                legendList.Margin = new Thickness(5);
                legendList.Background = Brushes.White;// new LinearGradientBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF), Color.FromArgb(0x7F, 0xFF, 0xFF, 0xFF), 45);
                legendList.BorderBrush = Brushes.Black;
                legendList.BorderThickness = new Thickness(3);
                //LegendTitle.Text = thematicItem.Description;

                Rectangle missingSwatchRect = new Rectangle()
                {
                    Width = 20,
                    Height = 20,
                    Stroke = new SolidColorBrush(Colors.Black),
                    Fill = colors[colors.Count - 1]
                };

                TextBlock missingClassTextBlock = new TextBlock();
                missingClassTextBlock.Text = String.Format("  " + missingText);
                StackPanel missingClassStackPanel = new StackPanel();
                missingClassStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
                missingClassStackPanel.Children.Add(missingSwatchRect);
                missingClassStackPanel.Children.Add(missingClassTextBlock);

                legendList.Items.Add(missingClassStackPanel);

                for (int c = 0; c < thematicItem.RangeStarts.Count; c++)
                {
                    Rectangle swatchRect = new Rectangle()
                    {
                        Width = 20,
                        Height = 20,
                        Stroke = new SolidColorBrush(Colors.Black),
                        Fill = colors[c]
                    };

                    TextBlock classTextBlock = new TextBlock();

                    // First classification
                    if (c == 0)
                        classTextBlock.Text =
                            String.Format("  Less than or equal to {0}", Math.Round(thematicItem.RangeStarts[1], 2)) +
                            " : " + ListLegendText.GetAt(c);
                    // Last classification
                    else if (c == thematicItem.RangeStarts.Count - 1)
                        classTextBlock.Text =
                            String.Format("  {0} and above", Math.Round(thematicItem.RangeStarts[c], 2)) + " : " +
                            ListLegendText.GetAt(c);
                    // Middle classifications
                    else
                    {
                        if (thematicItem.RangeStarts.Count > c + 1)
                        {
                            if (thematicItem.RangeStarts[c] == thematicItem.RangeStarts[c + 1])
                                classTextBlock.Text = String.Format("  Exactly {0}", Math.Round(thematicItem.RangeStarts[c], 2)) + " : " + ListLegendText.GetAt(c);
                            else
                                classTextBlock.Text = String.Format("  {0} to {1}", Math.Round(thematicItem.RangeStarts[c], 2), Math.Round(thematicItem.RangeStarts[c + 1], 2)) + " : " + ListLegendText.GetAt(c);
                        }
                        //else
                        //{
                        //    classTextBlock.Text = String.Format("  {0} to {1}", Math.Round(thematicItem.RangeStarts[c], 2), Math.Round(thematicItem.Max, 2));
                        //}
                    }

                    StackPanel classStackPanel = new StackPanel();
                    classStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
                    classStackPanel.Children.Add(swatchRect);
                    classStackPanel.Children.Add(classTextBlock);

                    legendList.Items.Add(classStackPanel);
                }

                TextBlock minTextBlock = new TextBlock();
                StackPanel minStackPanel = new StackPanel();
                minStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
                minTextBlock.Text = String.Format("Min: {0} ({1})", thematicItem.Min, string.IsNullOrEmpty(thematicItem.MinName) ? string.Empty : thematicItem.MinName.Trim());
                minStackPanel.Children.Add(minTextBlock);
                legendList.Items.Add(minStackPanel);

                TextBlock maxTextBlock = new TextBlock();
                StackPanel maxStackPanel = new StackPanel();
                maxStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
                maxTextBlock.Text = String.Format("Max: {0} ({1})", thematicItem.Max, string.IsNullOrEmpty(thematicItem.MaxName) ? string.Empty : thematicItem.MaxName.Trim());
                maxStackPanel.Children.Add(maxTextBlock);
                legendList.Items.Add(maxStackPanel);

                LegendStackPanel.Children.Add(legendList);

            }
            catch (Exception ex)
            {
            }
        }

        private DataTable GetLoadedData(DashboardHelper dashboardHelper, string dataKey, ref string valueField)
        {
            if (dashboardHelper == null)
            {
                return null;
            }

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
                loadedData = dashboardHelper.GenerateFrequencyTable(gadgetOptions).First().Key;
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
            return loadedData;
        }

        public void ResetRangeValues()
        {
            string valueField = this.valueField;

            DataTable loadedData = GetLoadedData(_dashboardHelper, dataKey, ref valueField);
            GraphicsLayer graphicsLayer = _myMap.Layers[layerId.ToString()] as GraphicsLayer;

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
                    catch (Exception ex)
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
            thematicItem.RangeStarts = new List<double>();

            double totalRange = thematicItem.Max - thematicItem.Min;
            double portion = totalRange / classCount;

            thematicItem.RangeStarts.Add(thematicItem.Min);
            double startRangeValue = thematicItem.Min;
            IEnumerable<double> valueEnumerator =
            from aValue in valueList
            orderby aValue
            select aValue;

            int increment = Convert.ToInt32(Math.Round((double)valueList.Count / (double)classCount));
            for (int i = increment; i < valueList.Count; i += increment)
            {
                double value = valueEnumerator.ElementAt(i);
                if (value < thematicItem.Min)
                    value = thematicItem.Min;
                thematicItem.RangeStarts.Add(value);
            }
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

            GraphicsLayer graphicsLayer = _myMap.Layers[layerId.ToString()] as GraphicsLayer;
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

            //return thematicItem;

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
            GraphicsLayer graphicsLayer = _myMap.Layers[layerId.ToString()] as GraphicsLayer;
            if (graphicsLayer != null)
            {
                _myMap.Layers.Remove(graphicsLayer);
                if (legendStackPanel != null)
                {
                    legendStackPanel.Children.Clear();
                }
            }
        }

        #endregion

        internal void SetRangeFromAttributeList(Dictionary<int, object> dictionary)
        {
            //if (classCount >= 2)
            //{
            //    Range.Add((double)dictionary[1])
            //}
        }


        private byte opacity;
        private bool _useCustomColors;

        public byte Opacity
        {
            get { return opacity; }
            set { opacity = value; }
        }


        #region ILayerProvider Members


        public CustomColorsDictionary CustomColorsDictionary
        {
            get { return _customColorsDictionary; }
            set { _customColorsDictionary = value; }
        }

        public ClassRangesDictionary ClassRangesDictionary
        {
            get { return _classRangesDictionary; }
        }

        public void PopulateRangeValues()
        {
            RangeCount = _thematicItem.RangeStarts.Count;
            Array.Clear(RangeValues, 0, RangeValues.Length);
            var RangeStarts = _thematicItem.RangeStarts;// RemoveOutOfRangeValues(thematicItem);
            for (int i = 0; i < RangeStarts.Count; i++)
            {
                RangeValues[i, 0] = RangeStarts[i].ToString();

                if (i < _thematicItem.RangeStarts.Count - 1)
                {
                    RangeValues[i, 1] = RangeStarts[i + 1].ToString();
                }
                else
                {
                    RangeValues[i, 1] = _thematicItem.Max.ToString();
                }

            }

        }

        public void PopulateRangeValues(DashboardHelper dashboardHelper, string shapeKey, string dataKey, string valueField,
                List<SolidColorBrush> colors, int classCount, string legendText)
        {
            _classCount = classCount;
            _dashboardHelper = dashboardHelper;
            _shapeKey = shapeKey;
            _dataKey = dataKey;
            _valueField = valueField;
            LegendText = legendText;
            _colors = colors;

            DataTable loadedData = GetLoadedData(dashboardHelper, dataKey, ref valueField);

            GraphicsLayer graphicsLayer = _myMap.Layers[_layerId.ToString()] as GraphicsLayer;
            _thematicItem = GetThematicItem(_classCount, loadedData, graphicsLayer);


            if (Range != null &&
                Range.Count > 0)
            {
                _thematicItem.RangeStarts = Range;
            }

            PopulateRangeValues();
            //return thematicItem;

        }

        #endregion
    }
}
