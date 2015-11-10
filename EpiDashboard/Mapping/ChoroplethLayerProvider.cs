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
using EpiDashboard.Mapping.ShapeFileReader;

namespace EpiDashboard.Mapping
{
    public class ChoroplethLayerProvider : ILayerProvider
    {
        #region Choropleth

        public enum ColorRampType
        {
            Auto,
            Custom
        }

        private ColorRampType _colorRampType = new ColorRampType();

        private bool _useCustomColors;        //   = true;

        //  private Dictionary<string, Color> _classesColorsDictionary = new Dictionary<string, Color>();






        Map _myMap;
        DashboardHelper _dashboardHelper;
        string _shapeKey;
        string _dataKey;
        string _valueField;
        string _missingText;
       public Guid _layerId;
        List<SolidColorBrush> _colors;
        int _classCount;
        int _colorShadeIndex = 0;
        int _lastGeneratedClassCount = 0;
        public bool AreRangesSet { get; set; }
        string[,] _rangeValues = new string[,] { { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" }, { "", "" } };

        float[] _quantileValues = new float[] { 5, 5, 5, 5, 5, 5, 5, 5, 5, 5 };
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


        private ListLegendTextDictionary _listLegendText = new ListLegendTextDictionary();
        private CustomColorsDictionary _customColorsDictionary = new CustomColorsDictionary();
        private ClassRangesDictionary _classRangesDictionary = new ClassRangesDictionary();


        public ListLegendTextDictionary ListLegendText
        {
            get { return _listLegendText; }
            set { _listLegendText = value; }
        }


        public ChoroplethLayerProvider(Map myMap)
        {
            _myMap = myMap;
            _layerId = Guid.NewGuid();
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
        private double Dist(Point px, Point po, Point pf)
        {
            double d = Math.Sqrt((px.Y - po.Y) * (px.Y - po.Y) + (px.X - po.X) * (px.X - po.X));
            if (((px.Y < po.Y) && (pf.Y > po.Y)) || ((px.Y > po.Y) && (pf.Y < po.Y)) || ((px.Y == po.Y) && (px.X < po.X) && (pf.X > po.X)) || ((px.Y == po.Y) && (px.X > po.X) && (pf.X < po.X)))
            {
                d = -d;
            }
            return d;
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

        public object[] LoadShapeFile(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                //Get the file info objects for the SHP and the DBF file selected by the user
                FileInfo shapeFile = new FileInfo(fileName);
                FileInfo dbfFile = new FileInfo(fileName.ToLower().Replace(".shp", ".dbf"));
                if (!dbfFile.Exists)
                {
                    System.Windows.MessageBox.Show("Associated DBF file not found");
                    return null;
                }

                //Read the SHP and DBF files into the ShapeFileReader
                ShapeFileReader.ShapeFile shapeFileReader = new ShapeFileReader.ShapeFile();
                if (shapeFile != null && dbfFile != null)
                {
                    shapeFileReader.Read(shapeFile, dbfFile);
                }
                else
                {
                    System.Windows.MessageBox.Show("Please select a SP and a DBF file to proceed.");
                    return null;
                }

                GraphicsLayer graphicsLayer = _myMap.Layers[_layerId.ToString()] as GraphicsLayer;
                if (graphicsLayer == null)
                {
                    graphicsLayer = new GraphicsLayer();
                    graphicsLayer.ID = _layerId.ToString();
                    _myMap.Layers.Add(graphicsLayer);


                    int recCount = shapeFileReader.Records.Count;
                    int rgbFactor = 255 / recCount;
                    int counter = 0;
                    foreach (ShapeFileReader.ShapeFileRecord record in shapeFileReader.Records)
                    {
                        Graphic graphic = record.ToGraphic();
                        if (graphic != null)
                        {
                            graphic.Symbol = GetFillSymbol(new SolidColorBrush(Color.FromArgb(240, 255, 255, 255)));
                            graphicsLayer.Graphics.Add(graphic);
                        }
                        counter += rgbFactor;
                    }
                }
                if (graphicsLayer.FullExtent == null)
                {
                    Envelope shapeFileExtent = shapeFileReader.GetExtent();
                    if (shapeFileExtent.SpatialReference == null)
                    {
                        _myMap.Extent = shapeFileExtent;
                    }
                    else
                    {
                        if (shapeFileExtent.SpatialReference.WKID == 4326)
                        {
                            _myMap.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(shapeFileExtent.XMin, shapeFileExtent.YMin)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(shapeFileExtent.XMax, shapeFileExtent.YMax)));
                        }
                    }
                }
                else
                {
                    _myMap.Extent = graphicsLayer.FullExtent;
                }
                graphicsLayer.RenderingMode = GraphicsLayerRenderingMode.Static;
                return new object[] { fileName, graphicsLayer.Graphics[0].Attributes };
            }
            else return null;
        }

        public object[] LoadShapeFile()
        {
            //Create the dialog allowing the user to select the "*.shp" and the "*.dbf" files
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "ESRI Shapefiles (*.shp)|*.shp";
            //ofd.Multiselect = true;

            if (ofd.ShowDialog().Value)
            {
                //Get the file info objects for the SHP and the DBF file selected by the user
                FileInfo shapeFile = new FileInfo(ofd.FileName);
                FileInfo dbfFile = new FileInfo(ofd.FileName.ToLower().Replace(".shp", ".dbf"));
                if (!dbfFile.Exists)
                {
                    System.Windows.MessageBox.Show("Associated DBF file not found");
                    return null;
                }
                foreach (string fname in ofd.FileNames)
                {
                    FileInfo fi = new FileInfo(fname);
                    if (fi.Extension.ToLower() == ".shp")
                    {
                        shapeFile = fi;
                    }
                    if (fi.Extension.ToLower() == ".dbf")
                    {
                        dbfFile = fi;
                    }
                }

                //Read the SHP and DBF files into the ShapeFileReader
                ShapeFileReader.ShapeFile shapeFileReader = new ShapeFileReader.ShapeFile();
                if (shapeFile != null && dbfFile != null)
                {
                    shapeFileReader.Read(shapeFile, dbfFile);
                }
                else
                {
                    System.Windows.MessageBox.Show("Please select a SP and a DBF file to proceed.");
                    return null;
                }

                GraphicsLayer graphicsLayer = _myMap.Layers[_layerId.ToString()] as GraphicsLayer;
                if (graphicsLayer == null)
                {
                    graphicsLayer = new GraphicsLayer();
                    graphicsLayer.ID = _layerId.ToString();
                    _myMap.Layers.Add(graphicsLayer);
                }

                int recCount = shapeFileReader.Records.Count;
                int rgbFactor = 255 / recCount;
                int counter = 0;
                //int minPoints = shapeFileReader.Records.Min(record => record.NumberOfPoints);
                //double meanPoints = shapeFileReader.Records.Average(record => record.NumberOfPoints);
                foreach (ShapeFileReader.ShapeFileRecord record in shapeFileReader.Records)
                {
                    Graphic graphic = record.ToGraphic();
                    if (graphic != null)
                    {
                        graphic.Symbol = GetFillSymbol(new SolidColorBrush(Color.FromArgb(240, 255, 255, 255)));
                        graphicsLayer.Graphics.Add(graphic);
                    }
                    counter += rgbFactor;
                }
                Envelope shapeFileExtent = shapeFileReader.GetExtent();
                if (shapeFileExtent.SpatialReference == null)
                {
                    _myMap.Extent = shapeFileExtent;
                }
                else
                {
                    if (shapeFileExtent.SpatialReference.WKID == 4326)
                    {
                        _myMap.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(shapeFileExtent.XMin, shapeFileExtent.YMin)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(shapeFileExtent.XMax, shapeFileExtent.YMax)));
                    }
                }
                graphicsLayer.RenderingMode = GraphicsLayerRenderingMode.Static;
                return new object[] { ofd.FileName, graphicsLayer.Graphics[0].Attributes };
            }
            else return null;
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

        public void SetShapeRangeValues(DashboardHelper dashboardHelper, string shapeKey, string dataKey, string valueField,
                List<SolidColorBrush> colors, int classCount, string missingText)
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

                GraphicsLayer graphicsLayer = _myMap.Layers[_layerId.ToString()] as GraphicsLayer;

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
            catch (Exception ex)
            {
            }
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
                gadgetOptions.ColumnNames = columnNames;
                Dictionary<string, string> inputVariableList = new Dictionary<string, string>();
                inputVariableList.Add("freqvar", dataKey);
                inputVariableList.Add("allvalues", "false");
                inputVariableList.Add("showconflimits", "false");
                inputVariableList.Add("showcumulativepercent", "false");
                inputVariableList.Add("includemissing", "false");
                inputVariableList.Add("maxrows", "500");

                gadgetOptions.InputVariableList = inputVariableList;
                //loadedData = dashboardHelper.GenerateFrequencyTable(gadgetOptions).First().Key;
                loadedData = dashboardHelper.GenerateFrequencyTableforMap(gadgetOptions).First().Key;
                foreach (DataRow dr in loadedData.Rows)
                {
                    dr[0] = dr[0].ToString().Trim();
                }
                _valueField = valueField = "freq";
            }
            else
            {
                loadedData = dashboardHelper.GenerateTable(columnNames);
            }
            return loadedData;
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

                string filterExpression = "";

                if (_dataKey.Contains(" ") || _dataKey.Contains("$") || _dataKey.Contains("#"))
                {
                    filterExpression += "[";
                }

                filterExpression += _dataKey;

                if (_dataKey.Contains(" ") || _dataKey.Contains("$") || _dataKey.Contains("#"))
                {
                    filterExpression += "]";
                }

                filterExpression += " = '" + graphicFeature.Attributes[_shapeKey].ToString().Replace("'", "''").Trim() + "'";

                double graphicValue = Double.PositiveInfinity;
                try
                {
                    graphicValue = Convert.ToDouble(loadedData.Select(filterExpression)[0][_valueField]);
                }
                catch (Exception ex)
                {
                    graphicValue = Double.PositiveInfinity;
                }

                string graphicName = graphicFeature.Attributes[_shapeKey].ToString();

                if (i == 0)
                {
                    thematicItem.Min = Double.PositiveInfinity;
                    thematicItem.Max = Double.NegativeInfinity;
                    //thematicItem.Min = Double.NegativeInfinity;
                    //thematicItem.Max = Double.PositiveInfinity;
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

            valueList.Sort();

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

            GraphicsLayer graphicsLayer = _myMap.Layers[_layerId.ToString()] as GraphicsLayer;
            _thematicItem = GetThematicItem(_classCount, loadedData, graphicsLayer);
            //RangeCount = thematicItem.RangeStarts.Count;
            //Array.Clear(RangeValues, 0, RangeValues.Length);
            //for (int i = 0; i < thematicItem.RangeStarts.Count; i++)
            //{
            //    RangeValues[i, 0] = thematicItem.RangeStarts[i].ToString();


            //    if (i < thematicItem.RangeStarts.Count - 1)
            //    {
            //        RangeValues[i, 1] = thematicItem.RangeStarts[i + 1].ToString();
            //    }
            //    else
            //    {
            //        RangeValues[i, 1] = thematicItem.Max.ToString();
            //    }

            //}
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
            var RangeStarts = _thematicItem.RangeStarts;// RemoveOutOfRangeValues(_thematicItem);
            for (int i = 0; i <  RangeStarts.Count; i++)
            {
                RangeValues[i, 0] =  RangeStarts[i].ToString();

                if (i < _thematicItem.RangeStarts.Count - 1)
                {
                    RangeValues[i, 1] =  RangeStarts[i + 1].ToString();
                }
                else
                {
                    RangeValues[i, 1] = _thematicItem.Max.ToString();
                }

            }
        }

        private List<double> RemoveOutOfRangeValues(ThematicItem _thematicItem)
        {
            List<double> RangeList = new List<double>();

            foreach (var Item in _thematicItem.RangeStarts)
            {
                if (Item >=_thematicItem.Min && Item <= _thematicItem.Max)
            {
                RangeList.Add(Item);
            }
            
            }

            return RangeList;
        }

        private StackPanel _legendStackPanel;
        private string _legendText;
        private bool _rangesLoadedFromMapFile;

        public StackPanel LegendStackPanel
        {
            get
            {
                return _legendStackPanel;
            }
            set
            {
                _legendStackPanel = value;
            }
        }

        public string LegendText
        {
            get { return _legendText; }
            set { _legendText = value; }
        }

        public ColorRampType CurrentColorRampType
        {
            get { return _colorRampType; }
            set { _colorRampType = value; }
        }

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

        #endregion

        #region ILayerProvider Members

        public void CloseLayer()
        {
            GraphicsLayer graphicsLayer = _myMap.Layers[_layerId.ToString()] as GraphicsLayer;
            if (graphicsLayer != null)
            {
                _myMap.Layers.Remove(graphicsLayer);
                if (_legendStackPanel != null)
                {
                    _legendStackPanel.Children.Clear();
                }
            }
        }

        #endregion

        public Color GetCustomColor(string rectangleControlName)
        {

            Color coo;

            try
            {
                //   coo = _classesColorsDictionary[rectangleControlName];    
                coo = _customColorsDictionary.GetWithKey(rectangleControlName);
            }
            catch (NullReferenceException ex)
            {

                throw new NullReferenceException(ex.Message);
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }


            return coo;

        }

        public void StoreColor(Rectangle rectangle, Color coo)
        {
            _customColorsDictionary.Add(rectangle.Name, coo);
        }

        public bool RangesLoadedFromMapFile
        {

            get { return _rangesLoadedFromMapFile; }
            set { _rangesLoadedFromMapFile = value; }
        }

        public List<double> RangeStartsFromMapFile { get; set; }
    }
}
