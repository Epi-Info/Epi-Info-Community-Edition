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
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Toolkit.DataSources;
using ESRI.ArcGIS.Client.Toolkit.DataSources.Kml;
using EpiDashboard.Mapping.ShapeFileReader;
using System.Collections.ObjectModel;

namespace EpiDashboard.Mapping
{
    public abstract class ChoroplethLayerProvider
    {
        public ChoroplethLayerProvider(Map myMap)
        {
            ArcGIS_Map = myMap;
            if (_layerId == Guid.Empty)
            {
                _layerId = Guid.NewGuid();
            }
            _graphicsLayers = new List<GraphicsLayer>();
        }

        public bool _asQuintile;
        public DashboardHelper _dashboardHelper;
        public string _shapeKey;
        public string _dataKey;
        public string _valueField;
        public string _missingText;
        public List<SolidColorBrush> _colors;
        public int _classCount;
        public double _resolution;
        public double _centerPoint_X;
        public double _centerPoint_Y;
        public ThematicItem _thematicItem;
        public bool AreRangesSet { get; set; }
        public List<GraphicsLayer> _graphicsLayers;

        private Guid _layerId;
        public Guid LayerId 
        {
            get { return _layerId; }
            set { _layerId = value; }
        }

        public string[,] _rangeValues = new string[,] { { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" } };
        public string[,] RangeValues
        {
            get { return _rangeValues; }
            set { _rangeValues = value; }
        }

        Map arcGIS_Map;
        public Map ArcGIS_Map
        {
            get { return arcGIS_Map; }
            set { arcGIS_Map = value; }
        }

        bool _useCustomColors;
        public bool UseCustomColors
        {
            get { return _useCustomColors; }
            set { _useCustomColors = value; }
        }

        List<double> _rangeStarts_FromControls;
        public List<double> RangeStarts_FromControls
        {
            get { return _rangeStarts_FromControls; }
            set { _rangeStarts_FromControls = value; }
        }

        byte _opacity;
        public byte Opacity
        {
            get { return _opacity; }
            set { _opacity = value; }
        }

        bool _useQuantiles = true;
        public bool UseQuantiles
        {
            get { return _useQuantiles; }
            set { _useQuantiles = value; }
        }

        bool _showPolyLabels = false;
        public bool ShowPolyLabels
        {
            get { return _showPolyLabels; }
            set { _showPolyLabels = value; }
        }

        CustomColorsDictionary _customColorsDictionary = new CustomColorsDictionary();
        public CustomColorsDictionary CustomColorsDictionary
        {
            get { return _customColorsDictionary; }
        }

        ClassRangeDictionary _classRangesDictionary = new ClassRangeDictionary();
        public ClassRangeDictionary ClassRangesDictionary
        {
            get { return _classRangesDictionary; }
        }

        ListLegendTextDictionary _listLegendText = new ListLegendTextDictionary();
        public ListLegendTextDictionary ListLegendText
        {
            get { return _listLegendText; }
            set { _listLegendText = value; }
        }

        bool _rangesLoadedFromMapFile;
        public bool RangesLoadedFromMapFile
        {
            get { return _rangesLoadedFromMapFile; }
            set { _rangesLoadedFromMapFile = value; }
        }

        public DashboardHelper DashboardHelper { get; set; }
        public int RangeCount { get; set; }
        public List<double> RangeStartsFromMapFile { get; set; }

        private string _legendText;
        public string LegendText
        {
            get { return _legendText; }
            set { _legendText = value; }
        }

        private StackPanel _legendStackPanel;
        public StackPanel LegendStackPanel
        {
            get { return _legendStackPanel; }
            set { _legendStackPanel = value; }
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
            public int RowCount { get; set; }
            public Dictionary<double, ClassDescription> ClassDescriptions { get; set; }
        }

        public struct ClassDescription
        {
            public double Start { get; set; }
            public double End { get; set; }
            public double ValueCount { get; set; }
        }

        public struct Values
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Value { get; set; }
        }

        abstract public GraphicsLayer GetGraphicsLayer();
        abstract public string GetShapeValue(Graphic graphicFeature, string shapeValue);
        abstract public object[] Load();
        abstract public object[] Load(string boundrySourceLocation);

        virtual public void CloseLayer()
        {
            GraphicsLayer graphicsLayer = GetGraphicsLayer();
            if (graphicsLayer != null)
            {
                ArcGIS_Map.Layers.Remove(graphicsLayer);
                if (LegendStackPanel != null)
                {
                    LegendStackPanel.Children.Clear();
                }
            }
        }

        public void MoveUp()
        {
            Layer layer = ArcGIS_Map.Layers[_layerId.ToString()];
            int currentIndex = ArcGIS_Map.Layers.IndexOf(layer);
            if (currentIndex < ArcGIS_Map.Layers.Count - 1)
            {
                ArcGIS_Map.Layers.Remove(layer);
                ArcGIS_Map.Layers.Insert(currentIndex + 1, layer);
            }
        }

        public void MoveDown()
        {
            Layer layer = ArcGIS_Map.Layers[_layerId.ToString()];
            int currentIndex = ArcGIS_Map.Layers.IndexOf(layer);
            if (currentIndex > 1)
            {
                ArcGIS_Map.Layers.Remove(layer);
                ArcGIS_Map.Layers.Insert(currentIndex - 1, layer);
            }
        }

        public Color GetColorAtPoint(Rectangle theRec, Point thePoint)
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
            double d4 = Dist(p4, p1, p2);
            double d2 = Dist(p2, p1, p2);
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

        public double Dist(Point px, Point po, Point pf)
        {
            double d = Math.Sqrt((px.Y - po.Y) * (px.Y - po.Y) + (px.X - po.X) * (px.X - po.X));
            if (((px.Y < po.Y) && (pf.Y > po.Y)) || ((px.Y > po.Y) && (pf.Y < po.Y)) || ((px.Y == po.Y) && (px.X < po.X) && (pf.X > po.X)) || ((px.Y == po.Y) && (px.X > po.X) && (pf.X < po.X)))
            {
                d = -d;
            }
            return d;
        }

        public int GetRangeIndex(double val, List<double> ranges)
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

        public Dictionary<double, ClassDescription> CalculateQuantiles(int classCount, ThematicItem thematicItem, List<double> valueList)
        {
            List<double> uniqueValues = valueList
                .Select(e => e)
                .Distinct()
                .ToList();

            if (classCount > uniqueValues.Count)
            {
                if (uniqueValues.Count > 2)
                {
                    classCount = uniqueValues.Count;
                }
                else
                {
                    throw new System.ArgumentException(string.Format(DashboardSharedStrings.GADGET_MAP_NOT_ENOUGH_VALUES_TO_GENERATE_N_CLASSES, classCount));
                }
            }
            
            List<double> rangeStarts = new List<double>();

            double totalRange = thematicItem.Max - thematicItem.Min;
            double portion = totalRange / classCount;

            Dictionary<double, ClassDescription> classDetail = new Dictionary<double, ClassDescription>();

            IEnumerable<double> valueEnumerator =
                from aValue in valueList
                orderby aValue
                select aValue;

            double increment = (double)valueList.Count / (double)classCount;

            List<double> inClass = new List<double>();

            int ordinalStart = 0;
            int ordinalEnd = 0;
            int ordinalUnderCut = 0;
            int ordinalOverCut = 0;

            double valueUnderCut = valueEnumerator.ElementAt(ordinalUnderCut);
            double valueOverCut = valueEnumerator.ElementAt(ordinalOverCut);
            double cutpoint = (valueUnderCut + valueOverCut) / 2.0;

            for (int classOrdinal = 1; classOrdinal <= classCount; classOrdinal++)
            {
                if (classOrdinal == 1)
                {
                    ordinalStart = 0;
                    ordinalEnd = Convert.ToInt32(Convert.ToInt32(Math.Floor(increment * classOrdinal))) - 1;

                    cutpoint = Math.Round(valueEnumerator.ElementAt(ordinalStart), 4);
                }
                else
                {
                    ordinalStart = Convert.ToInt32(Convert.ToInt32(Math.Ceiling(increment * (classOrdinal - 1)))) - 1;
                    ordinalEnd = Convert.ToInt32(Convert.ToInt32(Math.Floor(increment * classOrdinal))) - 1;
                    ordinalUnderCut = Convert.ToInt32(Convert.ToInt32(Math.Floor(increment * (classOrdinal - 1)))) - 1;
                    ordinalOverCut = Convert.ToInt32(Convert.ToInt32(Math.Ceiling(increment * (classOrdinal - 1)))) - 1;

                    valueUnderCut = valueEnumerator.ElementAt(ordinalUnderCut);
                    valueOverCut = valueEnumerator.ElementAt(ordinalOverCut);

                    cutpoint = Math.Round((valueUnderCut + valueOverCut) / 2.0, 4);
                }
                
                ClassDescription description = new ClassDescription();
                description.Start = valueEnumerator.ElementAt(ordinalStart);
                description.End = valueEnumerator.ElementAt(ordinalEnd);
                description.ValueCount = 1 + ordinalEnd - ordinalStart;
                
                classDetail.Add(cutpoint, description);
                rangeStarts.Add(cutpoint);
            }

            return classDetail;
        }

        public List<double> CalculateRangeStarts(int classCount, ThematicItem thematicItem, List<double> valueList)
        {
            List<double> uniqueValues = valueList
                .Select(e => e)
                .Distinct()
                .ToList();

            if (classCount > uniqueValues.Count)
            {
                if (uniqueValues.Count > 1)
                {
                    classCount = uniqueValues.Count;
                }
                else
                {
                    throw new System.ArgumentException(string.Format(DashboardSharedStrings.GADGET_MAP_NOT_ENOUGH_VALUES_TO_GENERATE_N_CLASSES, 2));
                }
            }

            List<double> rangeStarts = new List<double>();

            double totalRange = thematicItem.Max - thematicItem.Min;
            double portion = totalRange / classCount;
            rangeStarts.Add(thematicItem.Min);
            double startRangeValue = thematicItem.Min;

            IEnumerable<double> valueEnumerator =
                from aValue in valueList
                orderby aValue
                select aValue;

            double increment = (double)valueList.Count / (double)classCount;

            for (double i = increment - 1.0; Math.Round(i, 4) < valueList.Count - 1; i += increment)
            {
                double value0 = valueEnumerator.ElementAt(Convert.ToInt32(Math.Floor(i)));
                double value1 = valueEnumerator.ElementAt(Convert.ToInt32(Math.Ceiling(i)));
                double value = (value1 + value0) / 2.0;

                if (value < thematicItem.Min)
                {
                    value = thematicItem.Min;
                }

                rangeStarts.Add(value);
            }

            return rangeStarts;
        }

        public string PopulateRangeValues()
        {
            try
            {
                RangeCount = _thematicItem.RangeStarts.Count;
                Array.Clear(RangeValues, 0, RangeValues.Length);
                var RangeStarts = _thematicItem.RangeStarts;

                for (int i = 0; i < RangeStarts.Count; i++)
                {
                    RangeValues[i, 0] = string.Format("{0:N2}", RangeStarts[i]);

                    if (i < _thematicItem.RangeStarts.Count - 1)
                    {
                        RangeValues[i, 1] = string.Format("{0:N2}", RangeStarts[i + 1]);
                    }
                    else
                    {
                        RangeValues[i, 1] = string.Format("{0:N2}", _thematicItem.Max);
                    }
                }

                ClassRangesDictionary.SetRangesDictionary(RangeValues);

                return string.Empty;
            }
            catch
            {
                return DashboardSharedStrings.DASHBOARD_MAP_UNABLE_TO_SET_THE_RANGE_VALUES;
            }
        }

        public string PopulateRangeValues(DashboardHelper dashboardHelper, string shapeKey, string dataKey, string valueField, List<SolidColorBrush> colors, int classCount, string legendText, bool? showPolyLabels = false)
        {
            _classCount = classCount;
            _dashboardHelper = dashboardHelper;
            _shapeKey = shapeKey;
            _dataKey = dataKey;
            _valueField = valueField;
            LegendText = legendText;
            _colors = colors;
            _showPolyLabels = (bool)showPolyLabels;

            try
            {
                DataTable loadedData = GetLoadedData(dashboardHelper, dataKey, ref valueField);
                GraphicsLayer graphicsLayer = GetGraphicsLayer();

                bool layerContainsGraphics = graphicsLayer.Graphics.Count == 0 ? false : true;

                if (layerContainsGraphics)
                {
                    _thematicItem = GetThematicItem(_classCount, loadedData, graphicsLayer);

                    return PopulateRangeValues();
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (ArgumentException exception)
            {
                return exception.Message;
            }
            catch
            {
                return string.Format(DashboardSharedStrings.GADGET_MAP_NOT_ENOUGH_VALUES_TO_GENERATE_N_CLASSES, classCount);
            }
        }

        public void ResetRangeValues(string shapeKey, string dataKey, string valueField, int classCount)
        {
            _classCount = classCount;
            _shapeKey = shapeKey;
            _dataKey = dataKey;

            DataTable loadedData = GetLoadedData(_dashboardHelper, _dataKey, ref valueField);
            GraphicsLayer graphicsLayer = graphicsLayer = GetGraphicsLayer();

            if (graphicsLayer == null) return;

            bool layerContainsGraphics = graphicsLayer.Graphics == null || graphicsLayer.Graphics.Count == 0 ? false : true;

            if (layerContainsGraphics)
            {
                _thematicItem = GetThematicItem(_classCount, loadedData, graphicsLayer);
                PopulateRangeValues();
            }
        }

        public void Refresh()
        {
            if (_dashboardHelper != null)
            {
                SetShapeRangeValues(_dashboardHelper, _shapeKey, _dataKey, _valueField, _colors, _classCount, _missingText, _legendText);
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

        public DataTable GetLoadedData(DashboardHelper dashboardHelper, string dataKey, ref string valueField)
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
                gadgetOptions.ShouldIgnoreRowLimits = true;
                loadedData = dashboardHelper.GenerateFrequencyTableforMap(gadgetOptions).First().Key;

                foreach (DataRow dr in loadedData.Rows)
                {
                    dr[0] = dr[0].ToString().Trim();
                }

                _valueField = "freq";
            }
            else
            {
                loadedData = dashboardHelper.GenerateTable(columnNames);
            }

            return loadedData;
        }

        public void SetLegendSection(List<SolidColorBrush> colors, int classCount, string missingText, ThematicItem thematicItem)
        {
            if (colors == null) return;

            if (LegendStackPanel == null)
            {
                LegendStackPanel = new StackPanel();
            }

            LegendStackPanel.Children.Clear();

            System.Windows.Controls.ListBox legendList = new System.Windows.Controls.ListBox();
            legendList.Padding = new Thickness(0, 10, 0, 0);
            legendList.Background = Brushes.White;
            legendList.BorderBrush = Brushes.Black;
            legendList.BorderThickness = new Thickness(0);

            Rectangle missingSwatchRect = new Rectangle()
            {
                Width = 20,
                Height = 20,
                Stroke = new SolidColorBrush(Colors.Black),
                Fill = colors[colors.Count - 1],
                Margin = new Thickness(0, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Top
            };

            TextBlock titleTextBlock = new TextBlock();
            titleTextBlock.Text = LegendText;
            titleTextBlock.FontWeight = FontWeights.Bold;
            titleTextBlock.MaxWidth = 256;
            titleTextBlock.TextWrapping = TextWrapping.Wrap;
            StackPanel titleTextStackPanel = new StackPanel();
            titleTextStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
            titleTextStackPanel.Children.Add(titleTextBlock);
            titleTextStackPanel.Margin = new Thickness(10, 0, 10, 10);
            legendList.Items.Add(titleTextStackPanel);

            TextBlock missingClassTextBlock = new TextBlock();
            missingClassTextBlock.Text = String.Format("  " + missingText);
            missingClassTextBlock.MaxWidth = 256;
            missingClassTextBlock.TextWrapping = TextWrapping.Wrap;
            StackPanel missingClassStackPanel = new StackPanel();
            missingClassStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
            missingClassStackPanel.Children.Add(missingSwatchRect);
            missingClassStackPanel.Children.Add(missingClassTextBlock);
            missingClassStackPanel.Margin = new Thickness(10, 0, 10, 5);
            legendList.Items.Add(missingClassStackPanel);

            SetLegendText(colors, classCount, thematicItem.RangeStarts, legendList);

            TextBlock minTextBlock = new TextBlock();
            StackPanel minStackPanel = new StackPanel();
            minStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
            minTextBlock.Text = thematicItem.MinName != null ? DashboardSharedStrings.DASHBOARD_MAP_MIN + String.Format(" {0} ({1})", thematicItem.Min, thematicItem.MinName.Trim()) : DashboardSharedStrings.DASHBOARD_MAP_MIN + String.Format(" {0} ({1})", thematicItem.Min, string.Empty);
            minTextBlock.MaxWidth = 256;
            minTextBlock.TextWrapping = TextWrapping.Wrap;
            minStackPanel.Children.Add(minTextBlock);
            minStackPanel.Margin = new Thickness(10, 5, 10, 5);
            legendList.Items.Add(minStackPanel);

            TextBlock maxTextBlock = new TextBlock();
            StackPanel maxStackPanel = new StackPanel();
            maxStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
            maxTextBlock.Text = thematicItem.MaxName != null ? DashboardSharedStrings.DASHBOARD_MAP_MAX + String.Format(" {0} ({1})", thematicItem.Max, thematicItem.MaxName.Trim()) : DashboardSharedStrings.DASHBOARD_MAP_MAX + String.Format(" {0} ({1})", thematicItem.Max, string.Empty);
            maxTextBlock.MaxWidth = 256;
            maxTextBlock.TextWrapping = TextWrapping.Wrap;
            maxStackPanel.Children.Add(maxTextBlock);
            maxStackPanel.Margin = new Thickness(10, 0, 10, 10);
            legendList.Items.Add(maxStackPanel);

            LegendStackPanel.Children.Add(legendList);
        }

        public void SetLegendText(List<SolidColorBrush> colors, int classCount, List<double> rangeStarts, System.Windows.Controls.ListBox legendList)
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
                        Fill = colors[c],
                        Margin = new Thickness(0, 0, 5, 0),
                        VerticalAlignment = VerticalAlignment.Top
                    };

                    TextBlock classTextBlock = new TextBlock();

                    string spacer = ListLegendText.GetAt(c + 1) == string.Empty ? "   " : " : ";

                    if (c == 0)
                    {
                        classTextBlock.Text = "  " + String.Format(DashboardSharedStrings.DASHBOARD_MAP_LESS_THAN_N, Math.Round(rangeStarts[c + 1], 2)) + spacer + ListLegendText.GetAt(c + 1);
                    }
                    else if (c == classCount - 1)
                    {
                        classTextBlock.Text = "  " + String.Format(DashboardSharedStrings.DASHBOARD_MAP_N_AND_ABOVE, Math.Round(rangeStarts[c], 2)) + spacer + ListLegendText.GetAt(c + 1);
                    }
                    else if (rangeStarts.Count <= c + 1)
                    {
                        classTextBlock.Text = "  " + String.Format(DashboardSharedStrings.DASHBOARD_MAP_N_AND_ABOVE, Math.Round(rangeStarts[c], 2)) + spacer + ListLegendText.GetAt(c + 1);
                    }
                    else
                    {
                        if (rangeStarts[c] == rangeStarts[c + 1])
                        {
                            classTextBlock.Text = "  " + String.Format(DashboardSharedStrings.DASHBOARD_MAP_EXACTLY_N, Math.Round(rangeStarts[c], 2)) + spacer + ListLegendText.GetAt(c + 1);
                        }
                        else
                        {
                            classTextBlock.Text = "  " + String.Format(DashboardSharedStrings.DASHBOARD_MAP_N_TO_N, Math.Round(rangeStarts[c], 2), Math.Round(rangeStarts[c + 1], 2)) + spacer + ListLegendText.GetAt(c + 1);
                        }
                    }

                    classTextBlock.MaxWidth = 256;
                    classTextBlock.TextWrapping = TextWrapping.Wrap;

                    StackPanel classStackPanel = new StackPanel();
                    classStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
                    classStackPanel.Children.Add(swatchRect);
                    classStackPanel.Children.Add(classTextBlock);
                    classStackPanel.Margin = new Thickness(10, 0, 10, 5);

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

        public ThematicItem GetThematicItem(int classCount, DataTable loadedData, GraphicsLayer graphicsLayer)
        {
            ThematicItem thematicItem = new ThematicItem()
            {
                Name = _dataKey,
                Description = _dataKey,
                CalcField = ""
            };

            List<double> valueList = new List<double>();
            List<string> usedShapeValues = new List<string>();

            thematicItem.RowCount = loadedData.Rows.Count;

            string filterExpression = "";

            for (int i = 0; i < graphicsLayer.Graphics.Count; i++)
            {
                Graphic graphicFeature = graphicsLayer.Graphics[i];

                if(graphicFeature.Symbol is TextSymbol) { continue; }
                
                string shapeValue = string.Empty;

                shapeValue = GetShapeValue(graphicFeature, shapeValue);

                if (usedShapeValues.Contains(shapeValue))
                {
                    continue;
                }

                usedShapeValues.Add(shapeValue);

                filterExpression = "";

                if (_dataKey.Contains(" ") || _dataKey.Contains("$") || _dataKey.Contains("#"))
                {
                    filterExpression += "[";
                }

                filterExpression += _dataKey;

                if (_dataKey.Contains(" ") || _dataKey.Contains("$") || _dataKey.Contains("#"))
                {
                    filterExpression += "]";
                }

                filterExpression += " = '" + shapeValue + "'";

                double graphicValue = Double.PositiveInfinity;

                try
                {
                    loadedData.CaseSensitive = false;
                    
                    DataRow[] rows = loadedData.Select(filterExpression);

                    if (rows.Length > 0)
                    {
                        object found = rows[0][_valueField];
                        string valueField;

                        if (found is string)
                        {
                            valueField = (string)found;
                            graphicValue = Convert.ToDouble(valueField);
                        }
                        else if (found is double)
                        {
                            graphicValue = (Double)found;
                        }
                        else
                        {
                            graphicValue = Convert.ToDouble(found);
                        }
                    }
                }
                catch { }

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
                    if (graphicValue < thematicItem.Min)
                    {
                        thematicItem.Min = graphicValue;
                        thematicItem.MinName = graphicName;
                    }

                    if (graphicValue > thematicItem.Max && graphicValue != Double.PositiveInfinity)
                    {
                        thematicItem.Max = graphicValue;
                        thematicItem.MaxName = graphicName;
                    }
                }

                if (graphicValue < Double.PositiveInfinity)
                {
                    valueList.Add(graphicValue);
                }
            }

            if(valueList.Count == 0)
            {
                //MessageBox.Show(string.Format(DashboardSharedStrings.GADGET_MAP_NOT_ENOUGH_VALUES_TO_GENERATE_N_CLASSES, classCount));
                throw new System.ArgumentException(string.Format(DashboardSharedStrings.GADGET_MAP_NOT_ENOUGH_VALUES_TO_GENERATE_N_CLASSES, classCount) 
                    + Environment.NewLine + Environment.NewLine
                    + "[" + filterExpression + "]?");
            }

            valueList.Sort(); 

            if (this.UseQuantiles)
            {
                thematicItem.RangeStarts = CalculateRangeStarts(classCount, thematicItem, valueList);
                // dpb thematicItem.ClassDescriptions = CalculateQuantiles(classCount, thematicItem, valueList);
            }
            else
            {
                thematicItem.RangeStarts = ClassRangesDictionary.RangeStarts;
            }

            return thematicItem;
        }

        public void SetShapeRangeValues(DashboardHelper dashboardHelper, string shapeKey, string dataKey, string valueField, List<SolidColorBrush> brushList, int classCount, string missingText, string legendText, bool? showPolyLabels = false)
        {
            try
            {
                _classCount = classCount;
                _dashboardHelper = dashboardHelper;
                _shapeKey = shapeKey;
                _dataKey = dataKey;
                _valueField = valueField;
                _missingText = missingText;
                _legendText = legendText;
                _showPolyLabels = (bool)showPolyLabels;

                if (brushList != null) _colors = brushList;

                DataTable loadedData = GetLoadedData(dashboardHelper, dataKey, ref valueField);

                if (_valueField == "{Record Count}") _valueField = "freq";

                GraphicsLayer graphicsLayer = GetGraphicsLayer() as GraphicsLayer;

                _thematicItem = GetThematicItem(classCount, loadedData, graphicsLayer);

                if ( this.UseQuantiles == false && RangeStarts_FromControls != null && RangeStarts_FromControls.Count > 0)
                {
                    _thematicItem.RangeStarts = RangeStarts_FromControls;
                    PopulateRangeValues();
                }
                else
                {
                    PopulateRangeValues();
                }

                GraphicCollection textGraphics = new GraphicCollection();

                if (graphicsLayer.Graphics != null && graphicsLayer.Graphics.Count > 0)
                {
                    for (int i = graphicsLayer.Graphics.Count -1; i > -1; i--)
                    {
                        if (graphicsLayer.Graphics[i].Symbol is TextSymbol)
                        {
                            graphicsLayer.Graphics.RemoveAt(i);
                        }
                    }
                }

                bool usePoleOfInaccessibility = true;

                if (graphicsLayer.Graphics != null && graphicsLayer.Graphics.Count > 0)
                {
                    for (int i = 0; i < graphicsLayer.Graphics.Count; i++)
                    {
                        Graphic graphicFeature = graphicsLayer.Graphics[i];

                        if (graphicFeature.Symbol is TextSymbol) continue;
                        if (graphicFeature.Attributes.Count == 0) continue;

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

                        filterExpression += " = '" + GetShapeValue(graphicFeature, shapeKey) + "'";

                        double graphicValue = Double.PositiveInfinity;
                        try
                        {
                            DataRow[] postFilterExpression = loadedData.Select(filterExpression);
                            
                            if(postFilterExpression.Length > 0)
                            {
                                graphicValue = Convert.ToDouble(postFilterExpression[0][_valueField]);
                            }
                        }
                        catch (Exception)
                        {
                            graphicValue = Double.PositiveInfinity;
                        }

                        int brushIndex = GetRangeIndex(graphicValue, _thematicItem.RangeStarts);

                        if (brushList != null)
                        {
                            Color color = ((SolidColorBrush)brushList[brushIndex]).Color;
                            Brush fill = new SolidColorBrush(Color.FromArgb(Opacity, color.R, color.G, color.B));

                            if (graphicValue == Double.PositiveInfinity)
                            {
                                color = ((SolidColorBrush)brushList[brushList.Count - 1]).Color;
                                fill = new SolidColorBrush(Color.FromArgb(Opacity, color.R, color.G, color.B));
                            }

                            SimpleFillSymbol symbol = new SimpleFillSymbol();

                            symbol.Fill = fill;
                            symbol.BorderBrush = new SolidColorBrush(Colors.Black);
                            symbol.BorderThickness = 1;
                            
                            graphicFeature.Symbol = symbol;
                        }

                        if (ShowPolyLabels)
                        {
                            TextSymbol textSymbol = new TextSymbol();
                            textSymbol.Foreground = new SolidColorBrush(Colors.Black);
                            textSymbol.FontSize = 11;
                            //textSymbol.Text = graphicFeature.Attributes[shapeKey].ToString().Trim();
                            string sShapekey = graphicFeature.Attributes[shapeKey].ToString().Trim();
                            int indexShapeKey = sShapekey.LastIndexOfAny(new char[] { ' ', '-' });
                            int iShapeKeyLen = sShapekey.Length;

                            //  Break up label if possible and put on new line.  Adjust offsets as needed.
                            if (indexShapeKey != -1) //& iShapeKeyLen > 10)
                            {
                                textSymbol.Text = sShapekey.Substring(0, indexShapeKey) + "\r\n    " + sShapekey.Substring(indexShapeKey + 1, (iShapeKeyLen - (indexShapeKey + 1)));
                                textSymbol.OffsetX = iShapeKeyLen / 0.8;
                                textSymbol.OffsetY = 6;
                            }
                            else
                            {
                                textSymbol.Text = sShapekey;
                                textSymbol.OffsetX = iShapeKeyLen / 0.4;
                                textSymbol.OffsetY = 3;
                            }
                            //Console.WriteLine(textSymbol.Text);
                            //textSymbol.OffsetX = textSymbol.Text.Length / 0.4;
                            textSymbol.OffsetY = textSymbol.FontSize / 2.0;

                            Envelope extentEnvelope = graphicFeature.Geometry.Extent;
                            MapPoint pole = extentEnvelope.GetCenter();

                            ObservableCollection<ESRI.ArcGIS.Client.Geometry.PointCollection> rings = new ObservableCollection<ESRI.ArcGIS.Client.Geometry.PointCollection>();

                            //if (graphicFeature.Attributes[shapeKey].ToString().Trim() == "Richmond Hill") usePoleOfInaccessibility = true;

                            if (usePoleOfInaccessibility)
                            {
                                if (graphicFeature.Geometry is ESRI.ArcGIS.Client.Geometry.Polygon)
                                {
                                    rings = ((ESRI.ArcGIS.Client.Geometry.Polygon)graphicFeature.Geometry).Rings;
                                    Tuple<double, double> coords;

                                    bool showDebugCells = true;

                                    if(showDebugCells)
                                    {
                                        double precision = 0.1;
                                        double denom = 64.0;

                                        precision = extentEnvelope.Width / denom < precision? extentEnvelope.Width / denom : precision;
                                        precision = extentEnvelope.Height / denom < precision ? extentEnvelope.Height / denom : precision;

                                        coords = PolyLabel.PoleOfInaccessibility(rings, precision, graphicsLayer: graphicsLayer);

                                        //Envelope extent = graphicFeature.Geometry.Extent;
                                        //Cell extentCell = new Cell(extent.XMin, extent.YMin, extent.Width / 2, rings);
                                        //PolyLabel.AddDebugGraphic(extentCell, graphicsLayer);
                                    }
                                    else
                                    {
                                        coords = PolyLabel.PoleOfInaccessibility(rings);
                                    }

                                    pole.X = coords.Item1;
                                    pole.Y = coords.Item2;
                                }

                                //usePoleOfInaccessibility = false;
                            }

                            Graphic textGraphic = new Graphic();

                            textGraphic.Symbol = textSymbol;
                            textGraphic.Geometry = pole;

                            textGraphics.Add(textGraphic);
                        }
                        
                        TextBlock t = new TextBlock();
                        t.Background = Brushes.White;

                        if (graphicValue == Double.PositiveInfinity)
                        {
                            t.Text = GetShapeValue(graphicFeature, shapeKey) + " " + DashboardSharedStrings.DASHBOARD_MAP_NO_DATA;
                        }
                        else
                        {
                            t.Text = GetShapeValue(graphicFeature, shapeKey) + " : " + graphicValue.ToString();
                        }
                        
                        t.FontSize = 14;
                        
                        Border border = new Border();
                        border.BorderThickness = new Thickness(1);
                        Panel panel = new StackPanel();
                        panel.Children.Add(t);
                        border.Child = panel;

                        graphicFeature.MapTip = border;

                        if (graphicFeature.Attributes.Keys.Contains("EpiInfoValCol"))
                        {
                            graphicFeature.Attributes["EpiInfoValCol"] = graphicValue;
                        }
                        else
                        {
                            graphicFeature.Attributes.Add("EpiInfoValCol", graphicValue);
                        }
                    }

                    graphicsLayer.Graphics.AddRange(textGraphics);
                                                            
                    if (graphicsLayer is FeatureLayer)
                    {
                        ClassBreaksRenderer renderer = new ClassBreaksRenderer();
                        renderer.Field = "EpiInfoValCol";

                        Color color = ((SolidColorBrush)brushList[brushList.Count - 1]).Color;
                        Brush fill = new SolidColorBrush(Color.FromArgb(Opacity, color.R, color.G, color.B));

                        renderer.DefaultSymbol = new SimpleFillSymbol()
                        {
                            Fill = fill,
                            BorderBrush = new SolidColorBrush(Colors.Black),
                            BorderThickness = 1
                        };

                        for (int i = 0; i < _thematicItem.RangeStarts.Count; i++)
                        {
                            ClassBreakInfo classBreakInfo = new ClassBreakInfo();
                            classBreakInfo.MinimumValue = double.Parse(RangeValues[i, 0]);
                            classBreakInfo.MaximumValue = double.Parse(RangeValues[i, 1]);

                            color = ((SolidColorBrush)brushList[i]).Color;
                            fill = new SolidColorBrush(Color.FromArgb(Opacity, color.R, color.G, color.B));

                            classBreakInfo.Symbol = new SimpleFillSymbol()
                            {
                                Fill = fill,
                                BorderBrush = new SolidColorBrush(Colors.Black),
                                BorderThickness = 1
                            };

                            renderer.Classes.Add(classBreakInfo);
                        }

                        graphicsLayer.Renderer = renderer;
                    }
                }

                SetLegendSection(brushList, classCount, missingText, _thematicItem);
            }
            catch
            {
            }
        }
    }
}
