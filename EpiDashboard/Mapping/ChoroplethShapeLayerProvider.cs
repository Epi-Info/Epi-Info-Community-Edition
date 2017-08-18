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
        public ChoroplethShapeLayerProvider(Map clientMap) : base(clientMap)
        {
        }

        override public object[] Load()
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "ESRI Shapefiles (*.shp)|*.shp";

            if (dialog.ShowDialog().Value)
            {
                return Load(dialog.FileName);
            }

            return null;
        }
        
        override public object[] Load(string boundrySourceLocation)
        {
            if (!string.IsNullOrEmpty(boundrySourceLocation))
            {
                FileInfo shapeFile = new FileInfo(boundrySourceLocation);
                string directoryName = System.IO.Path.GetDirectoryName(boundrySourceLocation);
                string dbfFilename = System.IO.Path.GetFileName(boundrySourceLocation).ToLowerInvariant().Replace(".shp", ".dbf");
                string dbfFullPath = System.IO.Path.Combine(directoryName, dbfFilename);
                FileInfo dbfFile = new FileInfo(dbfFullPath);

                if (!dbfFile.Exists)
                {
                    System.Windows.MessageBox.Show("Associated DBF file not found");
                    return null;
                }

                ShapeFileReader.ShapeFile shapeFileReader = new ShapeFileReader.ShapeFile();
                if (shapeFile != null && dbfFile != null)
                {
                    try
                    {
                        shapeFileReader.Read(shapeFile, dbfFile);
                    }
                    catch ( NotSupportedException e)
                    {
                        System.Windows.MessageBox.Show(e.Message);
                        return null;
                    }
                    catch
                    {
                        System.Windows.MessageBox.Show(DashboardSharedStrings.DASHBOARD_MAP_N_POLYGONS_EXCEEDED);
                        return null;
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Please select a SP and a DBF file to proceed.");
                    return null;
                }

                GraphicsLayer graphicsLayer = GetGraphicsLayer();
                
                if (graphicsLayer == null)
                {
                    graphicsLayer = new GraphicsLayer();
                    graphicsLayer.ID = LayerId.ToString();
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
                            ArcGIS_Map.Extent = new Envelope(
                                ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(shapeFileExtent.XMin, shapeFileExtent.YMin)), 
                                ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(shapeFileExtent.XMax, shapeFileExtent.YMax)));
                        }
                    }
                }
                else
                {
                    ArcGIS_Map.Extent = graphicsLayer.FullExtent;
                }
                
                graphicsLayer.RenderingMode = GraphicsLayerRenderingMode.Static;

                return new object[] { boundrySourceLocation, graphicsLayer.Graphics[0].Attributes };
            }
            else return null;
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

        override public string GetShapeValue(Graphic graphicFeature, string shapeValue)
        {
            shapeValue = graphicFeature.Attributes[_shapeKey].ToString().Replace("'", "''").Trim();
            return shapeValue;
        }

        override public GraphicsLayer GetGraphicsLayer() 
        {
            GraphicsLayer graphicsLayer = ArcGIS_Map.Layers[LayerId.ToString()] as GraphicsLayer;
            return graphicsLayer;
        }
    }
}
