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
    public class ChoroplethKmlLayerProvider : IChoroLayerProvider
    {
        public event FeatureLoadedHandler FeatureLoaded;

        #region Choropleth

        private List<GraphicsLayer> graphicsLayers;
        private string url;

        Map _myMap;
        DashboardHelper _dashboardHelper;
        string _shapeKey;
        string _dataKey;
        string _valueField;
        string _missingText;
        //  public Guid _layerId;
        List<SolidColorBrush> _colors;
        int _classCount;
        public bool AreRangesSet { get; set; }
        string[,] _rangeValues = new string[,] { { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" } };
        float[] _quantileValues = new float[] { 5, 5, 5, 5, 5, 5, 5, 5, 5, 5 };

        List<List<SolidColorBrush>> ColorList = new List<List<SolidColorBrush>>();
        int _colorShadeIndex = 0;

        int _lastGeneratedClassCount = 0;
        ThematicItem _thematicItem;

        private List<double> _range;
        public List<double> Range
        {
            get { return _range; }
            set { _range = value; }
        }

        private byte _opacity;

        public byte Opacity
        {
            get { return _opacity; }
            set { _opacity = value; }
        }

        private bool _asQuantiles = true;
        public bool AsQuantiles 
        {
            get { return _asQuantiles; }
            set { _asQuantiles = value; }
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

        private ListLegendTextDictionary _listLegendText = new ListLegendTextDictionary();
        private CustomColorsDictionary _customColorsDictionary = new CustomColorsDictionary();
        private ClassRangesDictionary _classRangesDictionary = new ClassRangesDictionary();


        public ListLegendTextDictionary ListLegendText
        {
            get { return _listLegendText; }
            set { _listLegendText = value; }
        }

        public ChoroplethKmlLayerProvider(Map myMap)
        {
            this._myMap = myMap;
            this._layerId = Guid.NewGuid();
            graphicsLayers = new List<GraphicsLayer>();
        }

        public DashboardHelper DashboardHelper { get; set; }

        public string[,] RangeValues
        {
            get { return _rangeValues; }
            set { _rangeValues = value; }
        }

        public int RangeCount { get; set; }

        public float[] QuantileValues
        {
            get { return _quantileValues; }
            set { _quantileValues = value; }
        }

        public void MoveUp()
        {
            Layer layer = _myMap.Layers[_layerId.ToString()];
            int currentIndex = _myMap.Layers.IndexOf(layer);
            if (currentIndex < _myMap.Layers.Count - 1)
            {
                _myMap.Layers.Remove(layer);
                _myMap.Layers.Insert(currentIndex + 1, layer);
            }
        }

        public void MoveDown()
        {
            Layer layer = _myMap.Layers[_layerId.ToString()];
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

            int rgbFactor = 255 / _classCount;

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

            _lastGeneratedClassCount = _classCount;
        }

        private int GetRangeIndex(double val, List<double> ranges)
        {
            int limit;
            limit = ranges.Count < _classCount ? ranges.Count : _classCount;

            int index = limit - 1;
            for (int r = 0; r < limit - 1; r++)
            {
                if (val >= ranges[r] && val < ranges[r + 1]) index = r;
            }
            return index;
        }

        public struct Values
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Value { get; set; }
        }

        public object[] LoadKml()
        {
            KmlDialog dialog = new KmlDialog();
            object[] kmlfile = null;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                kmlfile = LoadKml(dialog.ServerName);
                return kmlfile;
            }
            else return null;
        }

        public object[] LoadKml(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                this.url = url;
                KmlLayer shapeLayer = _myMap.Layers[_layerId.ToString()] as KmlLayer;
                if (shapeLayer != null)
                {
                    _myMap.Layers.Remove(shapeLayer);
                }

                shapeLayer = new KmlLayer();
                shapeLayer.ID = _layerId.ToString();
                shapeLayer.Url = new Uri(url);
                shapeLayer.Initialized += new EventHandler<EventArgs>(shapeLayer_Initialized);
                _myMap.Layers.Add(shapeLayer);

                _myMap.Extent = shapeLayer.FullExtent;
                return new object[] { shapeLayer };
            }
            else { return null; }

        }

        void shapeLayer_Initialized(object sender, EventArgs e)
        {
            KmlLayer shapeLayer = _myMap.Layers[_layerId.ToString()] as KmlLayer;

            FindGraphicsLayers(shapeLayer);
            if (graphicsLayers.Count > 0)
            {
                IDictionary<string, object> coreAttributes = graphicsLayers[0].Graphics[0].Attributes;
                Dictionary<string, object> allAttributes = new Dictionary<string, object>();
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

                if (FeatureLoaded != null)
                {
                    FeatureLoaded(url, allAttributes);
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

            _myMap.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(xmin - 0.5, ymax + 0.5)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(xmax + 0.5, ymin - 0.5)));
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
            if (_dashboardHelper != null)
            {
                SetShapeRangeValues(_dashboardHelper, _shapeKey, _dataKey, _valueField, _colors, _classCount, _missingText);
            }
        }


        public Guid _layerId { get; set; }

        public void SetShapeRangeValues(DashboardHelper dashboardHelper, string shapeKey, string dataKey, string valueField, List<SolidColorBrush> colors, int classCount, string missingText)
        {
            try
            {
                _classCount = classCount;
                _dashboardHelper = dashboardHelper;
                _shapeKey = shapeKey;
                _dataKey = dataKey;
                _valueField = valueField;
                //_lowColor = lowColor;
                //_highColor = highColor;
                _colors = colors;
                _missingText = missingText;
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


                GraphicsLayer graphicsLayer = graphicsLayers[0];
                //CreateColorList(lowColor, highColor);
                _thematicItem = new ThematicItem() { Name = dataKey, Description = dataKey, CalcField = "" };



                List<double> valueList = new List<double>();
                for (int i = 0; i < graphicsLayer.Graphics.Count; i++)
                {
                    Graphic graphicFeature = graphicsLayer.Graphics[i];

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
                    }
                    catch (Exception ex)
                    {
                        graphicValue = Double.PositiveInfinity;
                    }

                    string graphicName = shapeValue;

                    if (i == 0)
                    {
                        _thematicItem.Min = Double.PositiveInfinity;
                        _thematicItem.Max = Double.NegativeInfinity;
                        _thematicItem.MinName = string.Empty;
                        _thematicItem.MaxName = string.Empty;
                    }
                    else
                    {
                        if (graphicValue < _thematicItem.Min) { _thematicItem.Min = graphicValue; _thematicItem.MinName = graphicName; }
                        if (graphicValue > _thematicItem.Max && graphicValue != Double.PositiveInfinity) { _thematicItem.Max = graphicValue; _thematicItem.MaxName = graphicName; }
                    }

                    if (graphicValue < Double.PositiveInfinity)
                    {
                        valueList.Add(graphicValue);
                    }
                }
                if (Range != null &&
                  Range.Count > 0)
                {
                    _thematicItem.RangeStarts = Range;
                    PopulateRangeValues();
                }
                else
                {
                    _thematicItem.RangeStarts = new List<double>();

                    double totalRange = _thematicItem.Max - _thematicItem.Min;
                    double portion = totalRange / classCount;

                    _thematicItem.RangeStarts.Add(_thematicItem.Min);
                    double startRangeValue = _thematicItem.Min;
                    IEnumerable<double> valueEnumerator =
                    from aValue in valueList
                    orderby aValue
                    select aValue;

                    int increment = Convert.ToInt32(Math.Round((double)valueList.Count / (double)classCount));
                    for (int i = increment; i < valueList.Count; i += increment)
                    {
                        double value = valueEnumerator.ElementAt(i);
                        if (value < _thematicItem.Min)
                            value = _thematicItem.Min;
                        _thematicItem.RangeStarts.Add(value);
                    }
                }
                // Create graphic features and set symbol using the class range which contains the value 
                //List<SolidColorBrush> brushList = ColorList[_colorShadeIndex];
                if (graphicsLayer.Graphics != null && graphicsLayer.Graphics.Count > 0)
                {

                    for (int i = 0; i < graphicsLayer.Graphics.Count; i++)
                    {
                        Graphic graphicFeature = graphicsLayer.Graphics[i];

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
                            t.Text = shapeValue + " : No Data";
                        }
                        else
                        {
                            t.Text = shapeValue + " : " + graphicValue.ToString();
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


                SetLegendSection(classCount, colors, missingText, _thematicItem);

            }
            catch (Exception ex)
            {
            }
        }

        public void ResetRangeValues(string shapeKey, string dataKey, string valueField, int classCount)
        {
            _classCount = classCount;
            _shapeKey = shapeKey;
            _dataKey = dataKey;

            DataTable loadedData = GetLoadedData(_dashboardHelper, _dataKey, ref valueField);
            GraphicsLayer graphicsLayer = _myMap.Layers[_layerId.ToString()] as GraphicsLayer;

            if (graphicsLayer == null) return;

            _thematicItem = GetThematicItem(_classCount, loadedData, graphicsLayer);
        }

        //  xxxx
        public object[] LoadShapeFile()
        {
            throw new NotImplementedException();
        }

        private void SetLegendSection(int classCount, List<SolidColorBrush> brushList, string missingText, ThematicItem thematicItem)
        {
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
                Fill = brushList[brushList.Count - 1]
            };

            TextBlock titleTextBlock = new TextBlock();
            titleTextBlock.Text = LegendText;          //        String.Format("  " + "Title Text");
            StackPanel titleTextStackPanel = new StackPanel();
            titleTextStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
            //  titleTextStackPanel.Children.Add(missingSwatchRect);

            titleTextStackPanel.Children.Add(titleTextBlock);


            TextBlock missingClassTextBlock = new TextBlock();
            missingClassTextBlock.Text = String.Format("  " + missingText);
            StackPanel missingClassStackPanel = new StackPanel();
            missingClassStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
            missingClassStackPanel.Children.Add(missingSwatchRect);
            missingClassStackPanel.Children.Add(missingClassTextBlock);

            legendList.Items.Add(titleTextStackPanel);

            legendList.Items.Add(missingClassStackPanel);

            SetLegendText(brushList, classCount, thematicItem.RangeStarts, legendList);

            TextBlock minTextBlock = new TextBlock();
            StackPanel minStackPanel = new StackPanel();
            minStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
            minTextBlock.Text = thematicItem.MinName != null ? String.Format("Min: {0} ({1})", thematicItem.Min, thematicItem.MinName.Trim()) : String.Format("Min: {0} ({1})", thematicItem.Min, string.Empty);
            minStackPanel.Children.Add(minTextBlock);
            legendList.Items.Add(minStackPanel);

            TextBlock maxTextBlock = new TextBlock();
            StackPanel maxStackPanel = new StackPanel();
            maxStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
            maxTextBlock.Text = thematicItem.MaxName != null ? String.Format("Max: {0} ({1})", thematicItem.Max, thematicItem.MaxName.Trim()) : String.Format("Max: {0} ({1})", thematicItem.Max, string.Empty);
            maxStackPanel.Children.Add(maxTextBlock);
            legendList.Items.Add(maxStackPanel);

            LegendStackPanel.Children.Add(legendList);
        }

        private void SetLegendText(List<SolidColorBrush> brushList, int classCount, List<double> rangeStarts, System.Windows.Controls.ListBox legendList)
        {
            for (int c = 0; c < classCount; c++)
            {
                try
                {
                    Rectangle swatchRect = new Rectangle()
                    {
                        Width = 20,
                        Height = 20,
                        Stroke = new SolidColorBrush(Colors.Black),
                        Fill = brushList[c]
                    };

                    TextBlock classTextBlock = new TextBlock();

                    //if (c == 0)
                    //{
                    //    //if (thematicItem.RangeStarts[1] == thematicItem.Min)
                    //    //    classTextBlock.Text = String.Format("  Exactly {0}", Math.Round(thematicItem.RangeStarts[1], 2));
                    //    //else
                    //    //    classTextBlock.Text = String.Format("  Less than {0}", Math.Round(thematicItem.RangeStarts[1], 2));
                    //    if (thematicItem.RangeStarts[c] == thematicItem.RangeStarts[c + 1])
                    //        classTextBlock.Text = String.Format("  Exactly {0}", Math.Round(thematicItem.RangeStarts[c], 2));
                    //    else
                    //        classTextBlock.Text = String.Format("  {0} to {1}", Math.Round(thematicItem.RangeStarts[c], 2), Math.Round(thematicItem.RangeStarts[c + 1], 2));

                    //}
                    //else 
                    if (c == classCount - 1)
                        classTextBlock.Text = String.Format("  {0} and above", Math.Round(rangeStarts[c], 2)) + " : " +
                                              ListLegendText.GetAt(c + 1);
                    else if (rangeStarts.Count <= c + 1)
                    {
                        classTextBlock.Text = String.Format("  {0} and above", Math.Round(rangeStarts[c], 2)) + " : " +
                                              ListLegendText.GetAt(c + 1);
                    }
                    // Middle classifications
                    else
                    {
                        if (rangeStarts[c] == rangeStarts[c + 1])
                            classTextBlock.Text = String.Format("  Exactly {0}", Math.Round(rangeStarts[c], 2)) + " : " +
                                                  ListLegendText.GetAt(c + 1);
                        else
                            classTextBlock.Text =
                                String.Format("  {0} to {1}", Math.Round(rangeStarts[c], 2),
                                    Math.Round(rangeStarts[c + 1], 2)) + " : " + ListLegendText.GetAt(c + 1);
                    }

                    StackPanel classStackPanel = new StackPanel();
                    classStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
                    classStackPanel.Children.Add(swatchRect);
                    classStackPanel.Children.Add(classTextBlock);

                    legendList.Items.Add(classStackPanel);
                    if (rangeStarts.Count <= c + 1)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
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
            string valueField = _valueField;

            DataTable loadedData = GetLoadedData(_dashboardHelper, _dataKey, ref valueField);

            GraphicsLayer graphicsLayer = graphicsLayers[0];
            if (graphicsLayer == null) return;

            ThematicItem thematicItem = GetThematicItem(_classCount, loadedData, graphicsLayer);
        }

        public ThematicItem GetThematicItem(int classCount, DataTable loadedData, GraphicsLayer graphicsLayer)
        {

            ThematicItem thematicItem = new ThematicItem()
            {
                Name = _dataKey,
                Description = _dataKey,
                CalcField = ""
            };

            List<double> valueList = new List<double>();

            for (int i = 0; i < graphicsLayer.Graphics.Count; i++)
            {
                Graphic graphicFeature = graphicsLayer.Graphics[i];

                string shapeValue = string.Empty;

                if (!graphicFeature.Attributes.ContainsKey(_shapeKey))
                {
                    List<KmlExtendedData> eds = (List<KmlExtendedData>)graphicFeature.Attributes["extendedData"];
                    foreach (KmlExtendedData ed in eds)
                    {
                        if (ed.Name.Equals(_shapeKey))
                        {
                            shapeValue = ed.Value.Replace("'", "''").Trim();
                        }
                    }
                }
                else
                {
                    shapeValue = graphicFeature.Attributes[_shapeKey].ToString().Replace("'", "''").Trim();
                }


                string filterExpression = "";
                if (_dataKey.Contains(" ") || _dataKey.Contains("$") || _dataKey.Contains("#"))
                    filterExpression += "[";
                filterExpression += _dataKey;
                if (_dataKey.Contains(" ") || _dataKey.Contains("$") || _dataKey.Contains("#"))
                    filterExpression += "]";
                filterExpression += " = '" + shapeValue + "'";

                double graphicValue = Double.PositiveInfinity;
                try
                {
                    graphicValue = Convert.ToDouble(loadedData.Select(filterExpression)[0][_valueField]);
                }
                catch (Exception ex)
                {
                    graphicValue = Double.PositiveInfinity;
                }

                string graphicName = shapeValue;

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
            thematicItem.RangeStarts = CalculateThematicRange(classCount, thematicItem, valueList);

            return thematicItem;
        }

        private List<double> CalculateThematicRange(int classCount, ThematicItem thematicItem, List<double> valueList)
        {

            List<double> rangeStarts = new List<double>();

            if (RangesLoadedFromMapFile && RangeStartsFromMapFile != null)
            {
                // create rangStarts from map7 file  
                rangeStarts = this.RangeStartsFromMapFile;
            }
            else
            {

                double totalRange = thematicItem.Max - thematicItem.Min;
                double portion = totalRange / classCount;

                rangeStarts.Add(thematicItem.Min);
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
                    rangeStarts.Add(value);
                }
            }

            return rangeStarts;
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

            GraphicsLayer graphicsLayer = graphicsLayers[0];

            _thematicItem = GetThematicItem(_classCount, loadedData, graphicsLayer);

            if (Range != null &&
                Range.Count > 0)
            {
                _thematicItem.RangeStarts = Range;
            }

            PopulateRangeValues();
            //return thematicItem;

        }

        public void PopulateRangeValues()
        {
            RangeCount = _thematicItem.RangeStarts.Count;
            Array.Clear(RangeValues, 0, RangeValues.Length);
            var RangeStarts = _thematicItem.RangeStarts;
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
        private StackPanel legendStackPanel;
        private string _legendText;
        private bool _rangesLoadedFromMapFile;

        private bool _useCustomColors = false;


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

        public string LegendText
        {
            get { return _legendText; }
            set { _legendText = value; }
        }

        public bool UseCustomRanges { get; set; }

        public bool UseCustomColors
        {
            get { return _useCustomColors; }
            set { _useCustomColors = value; }
        }

        //public Dictionary<string, Color> ClassesColorsDictionary
        //{
        //    get { return _classesColorsDictionary; }
        //    set { _classesColorsDictionary = value; }
        //}

        public CustomColorsDictionary CustomColorsDictionary
        {
            get { return _customColorsDictionary; }
        }

        public ClassRangesDictionary ClassRangesDictionary
        {
            get { return _classRangesDictionary; }
        }

        public bool RangesLoadedFromMapFile
        {

            get { return _rangesLoadedFromMapFile; }
            set { _rangesLoadedFromMapFile = value; }
        }


        #endregion

        #region ILayerProvider Members

        public void CloseLayer()
        {
            KmlLayer shapeLayer = _myMap.Layers[_layerId.ToString()] as KmlLayer;
            if (shapeLayer != null)
            {
                _myMap.Layers.Remove(shapeLayer);
                if (legendStackPanel != null)
                {
                    legendStackPanel.Children.Clear();
                }
            }
        }

        #endregion



        public List<double> RangeStartsFromMapFile { get; set; }

        //public bool AreRangesSet { get; set; }

        //public byte Opacity { get; set; }




    }
}
