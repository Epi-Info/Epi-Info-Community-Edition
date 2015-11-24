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
    public class ChoroplethShapeLayerProvider : ChoroplethLayerProvider, IChoroLayerProvider
    {
        #region Choropleth
        
        public ChoroplethShapeLayerProvider(Map clientMap) : base(clientMap)
        {
            ClientMap = clientMap;
            this._layerId = Guid.NewGuid();
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

                GraphicsLayer graphicsLayer = ClientMap.Layers[_layerId.ToString()] as GraphicsLayer;
                if (graphicsLayer == null)
                {
                    graphicsLayer = new GraphicsLayer();
                    graphicsLayer.ID = _layerId.ToString();
                    ClientMap.Layers.Add(graphicsLayer);


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
                        ClientMap.Extent = shapeFileExtent;
                    }
                    else
                    {
                        if (shapeFileExtent.SpatialReference.WKID == 4326)
                        {
                            ClientMap.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(shapeFileExtent.XMin, shapeFileExtent.YMin)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(shapeFileExtent.XMax, shapeFileExtent.YMax)));
                        }
                    }
                }
                else
                {
                    ClientMap.Extent = graphicsLayer.FullExtent;
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

                GraphicsLayer graphicsLayer = ClientMap.Layers[_layerId.ToString()] as GraphicsLayer;
                if (graphicsLayer == null)
                {
                    graphicsLayer = new GraphicsLayer();
                    graphicsLayer.ID = ClientMap.ToString();
                    ClientMap.Layers.Add(graphicsLayer);
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
                    ClientMap.Extent = shapeFileExtent;
                }
                else
                {
                    if (shapeFileExtent.SpatialReference.WKID == 4326)
                    {
                        ClientMap.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(shapeFileExtent.XMin, shapeFileExtent.YMin)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(shapeFileExtent.XMax, shapeFileExtent.YMax)));
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
            GraphicsLayer graphicsLayer = ClientMap.Layers[_layerId.ToString()] as GraphicsLayer;

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
                catch
                {
                    graphicValue = Double.PositiveInfinity;
                }

                string graphicName = graphicFeature.Attributes[_shapeKey].ToString();

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

            valueList.Sort();

            thematicItem.RangeStarts = CalculateThematicRange(classCount, thematicItem, valueList);

            return thematicItem;
        }

        private List<double> RemoveOutOfRangeValues(ThematicItem thematicItem)
        {
            List<double> RangeList = new List<double>();

            foreach (var Item in thematicItem.RangeStarts)
            {
                if (Item >= thematicItem.Min && Item <= thematicItem.Max)
                {
                    RangeList.Add(Item);
                }

            }

            return RangeList;
        }

        private StackPanel _legendStackPanel;
        private string _legendText;
        private bool _rangesLoadedFromMapFile;

        public bool AsQuintile
        {
            get { return _asQuintile; }
            set { _asQuintile = value; }
        }

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

                GraphicsLayer graphicsLayer = ClientMap.Layers[_layerId.ToString()] as GraphicsLayer;

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

            GraphicsLayer graphicsLayer = ClientMap.Layers[_layerId.ToString()] as GraphicsLayer;

            _thematicItem = GetThematicItem(_classCount, loadedData, graphicsLayer);

            if (Range != null &&
                Range.Count > 0)
            {
                _thematicItem.RangeStarts = Range;
            }

            PopulateRangeValues();
        }

        #endregion

        #region ILayerProvider Members

        public void CloseLayer()
        {
            GraphicsLayer graphicsLayer = ClientMap.Layers[_layerId.ToString()] as GraphicsLayer;
            if (graphicsLayer != null)
            {
                ClientMap.Layers.Remove(graphicsLayer);
                if (_legendStackPanel != null)
                {
                    _legendStackPanel.Children.Clear();
                }
            }
        }

        #endregion
    }
}
