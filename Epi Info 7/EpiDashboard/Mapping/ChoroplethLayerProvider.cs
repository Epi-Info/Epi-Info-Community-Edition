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
using EpiDashboard.Mapping.ShapeFileReader;

namespace EpiDashboard.Mapping
{
    public class ChoroplethLayerProvider
    {
        bool _useCustomColors;
        public bool UseCustomColors
        {
            get { return _useCustomColors; }
            set { _useCustomColors = value; }
        }
        
        public bool _asQuintile;

        public DashboardHelper _dashboardHelper;
        public string _shapeKey;
        public string _dataKey;
        public string _valueField;
        public string _missingText;
        public Guid _layerId { get; set; }
        public List<SolidColorBrush> _colors;
        public int _classCount;
        public string[,] _rangeValues = new string[,] { { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" } };

        public ThematicItem _thematicItem;

        public bool AreRangesSet { get; set; }

        public List<GraphicsLayer> _graphicsLayers;

        Map arcGIS_Map;
        public Map ArcGIS_Map
        {
            get { return arcGIS_Map; }
            set { arcGIS_Map = value; }
        }

        List<double> _range;
        public List<double> Range
        {
            get { return _range; }
            set { _range = value; }
        }

        byte _opacity;
        public byte Opacity
        {
            get { return _opacity; }
            set { _opacity = value; }
        }

        bool _asQuantiles = true;
        public bool AsQuantiles
        {
            get { return _asQuantiles; }
            set { _asQuantiles = value; }
        }

        ListLegendTextDictionary _listLegendText = new ListLegendTextDictionary();

        CustomColorsDictionary _customColorsDictionary = new CustomColorsDictionary();
        public CustomColorsDictionary CustomColorsDictionary
        {
            get { return _customColorsDictionary; }
        }

        ClassRangesDictionary _classRangesDictionary = new ClassRangesDictionary();
        public ClassRangesDictionary ClassRangesDictionary
        {
            get { return _classRangesDictionary; }
        }

        public ListLegendTextDictionary ListLegendText
        {
            get { return _listLegendText; }
            set { _listLegendText = value; }
        }

        public ChoroplethLayerProvider(Map myMap)
        {
            arcGIS_Map = myMap;
            _layerId = Guid.NewGuid();
            _graphicsLayers = new List<GraphicsLayer>();
        }

        public  DashboardHelper DashboardHelper { get; set; }

        public string[,] RangeValues
        {
            get { return _rangeValues; }
            set { _rangeValues = value; }
        }

        public int RangeCount { get; set; }

        public void MoveUp()
        {
            Layer layer = arcGIS_Map.Layers[_layerId.ToString()];
            int currentIndex = arcGIS_Map.Layers.IndexOf(layer);
            if (currentIndex < arcGIS_Map.Layers.Count - 1)
            {
                arcGIS_Map.Layers.Remove(layer);
                arcGIS_Map.Layers.Insert(currentIndex + 1, layer);
            }
        }

        public void MoveDown()
        {
            Layer layer = arcGIS_Map.Layers[_layerId.ToString()];
            int currentIndex = arcGIS_Map.Layers.IndexOf(layer);
            if (currentIndex > 1)
            {
                arcGIS_Map.Layers.Remove(layer);
                arcGIS_Map.Layers.Insert(currentIndex - 1, layer);
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
        
        bool _rangesLoadedFromMapFile;
        public bool RangesLoadedFromMapFile
        {
            get { return _rangesLoadedFromMapFile; }
            set { _rangesLoadedFromMapFile = value; }
        }

        public List<double> RangeStartsFromMapFile { get; set; }
        
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

        private StackPanel legendStackPanel;
        private string _legendText;

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

        public struct Values
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Value { get; set; }
        }

        public List<double> CalculateThematicRange(int classCount, ThematicItem thematicItem, List<double> valueList)
        {
            List<double> rangeStarts = new List<double>();

            if (RangesLoadedFromMapFile && RangeStartsFromMapFile != null)
            {
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
            }

            return rangeStarts;
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

        public void SetLegendSection(List<SolidColorBrush> colors, int classCount, string missingText, ThematicItem thematicItem)  //        string  legText  )    
        {
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
            minTextBlock.Text = thematicItem.MinName != null ? String.Format("Min: {0} ({1})", thematicItem.Min, thematicItem.MinName.Trim()) : String.Format("Min: {0} ({1})", thematicItem.Min, string.Empty);
            minTextBlock.MaxWidth = 256;
            minTextBlock.TextWrapping = TextWrapping.Wrap;
            minStackPanel.Children.Add(minTextBlock);
            minStackPanel.Margin = new Thickness(10, 5, 10, 5);
            legendList.Items.Add(minStackPanel);

            TextBlock maxTextBlock = new TextBlock();
            StackPanel maxStackPanel = new StackPanel();
            maxStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
            maxTextBlock.Text = thematicItem.MaxName != null ? String.Format("Max: {0} ({1})", thematicItem.Max, thematicItem.MaxName.Trim()) : String.Format("Max: {0} ({1})", thematicItem.Max, string.Empty);
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

                    if (c == classCount - 1)
                    {
                        classTextBlock.Text = String.Format("  {0} and above", Math.Round(rangeStarts[c], 2)) + " : " + ListLegendText.GetAt(c + 1);
                    }
                    else if (rangeStarts.Count <= c + 1)
                    {
                        classTextBlock.Text = String.Format("  {0} and above", Math.Round(rangeStarts[c], 2)) + " : " + ListLegendText.GetAt(c + 1);
                    }
                    else
                    {
                        if (rangeStarts[c] == rangeStarts[c + 1])
                        {
                            classTextBlock.Text = String.Format("  Exactly {0}", Math.Round(rangeStarts[c], 2)) + " : " + ListLegendText.GetAt(c + 1);
                        }
                        else
                        {
                            classTextBlock.Text = String.Format("  {0} to {1}", Math.Round(rangeStarts[c], 2), Math.Round(rangeStarts[c + 1], 2)) + " : " + ListLegendText.GetAt(c + 1);
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
    }
}
