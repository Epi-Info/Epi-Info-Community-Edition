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
        }

        public object[] LoadShapeFile(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                FileInfo shapeFile = new FileInfo(fileName);
                FileInfo dbfFile = new FileInfo(fileName.ToLower().Replace(".shp", ".dbf"));
                if (!dbfFile.Exists)
                {
                    System.Windows.MessageBox.Show("Associated DBF file not found");
                    return null;
                }

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

                GraphicsLayer graphicsLayer = ArcGIS_Map.Layers[_layerId.ToString()] as GraphicsLayer;
                if (graphicsLayer == null)
                {
                    graphicsLayer = new GraphicsLayer();
                    graphicsLayer.ID = _layerId.ToString();
                    ArcGIS_Map.Layers.Add(graphicsLayer);


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
                        ArcGIS_Map.Extent = shapeFileExtent;
                    }
                    else
                    {
                        if (shapeFileExtent.SpatialReference.WKID == 4326)
                        {
                            ArcGIS_Map.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(shapeFileExtent.XMin, shapeFileExtent.YMin)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(shapeFileExtent.XMax, shapeFileExtent.YMax)));
                        }
                    }
                }
                else
                {
                    ArcGIS_Map.Extent = graphicsLayer.FullExtent;
                }
                
                graphicsLayer.RenderingMode = GraphicsLayerRenderingMode.Static;
                
                return new object[] { fileName, graphicsLayer.Graphics[0].Attributes };
            }
            else return null;
        }

        public object[] LoadShapeFile()
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "ESRI Shapefiles (*.shp)|*.shp";

            if (ofd.ShowDialog().Value)
            {
                return LoadShapeFile(ofd.FileName);
            }
            else 
            {
                return null;
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

        override public string GetShapeValue(Graphic graphicFeature, string shapeValue)
        {
            shapeValue = graphicFeature.Attributes[_shapeKey].ToString().Replace("'", "''").Trim();
            return shapeValue;
        }

        #endregion

        #region ILayerProvider Members

        public void CloseLayer()
        {
            GraphicsLayer graphicsLayer = ArcGIS_Map.Layers[_layerId.ToString()] as GraphicsLayer;
            if (graphicsLayer != null)
            {
                ArcGIS_Map.Layers.Remove(graphicsLayer);
                if (_legendStackPanel != null)
                {
                    _legendStackPanel.Children.Clear();
                }
            }
        }

        #endregion
    }
}
