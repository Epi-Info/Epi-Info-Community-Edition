using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;

using EpiDashboard.Mapping.ShapeFileReader;

namespace EpiDashboard.Mapping
{
    public class ChoroplethShapeLayerProvider : ChoroplethLayerProvider, IChoroLayerProvider
    {
        public ChoroplethShapeLayerProvider(MapView clientMap) : base(clientMap)
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
                    ArcGIS_MapView.Map.Layers.Add(graphicsLayer);

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
                        ArcGIS_MapView.SetView(shapeFileExtent);
                    }
                    else
                    {
                        if (shapeFileExtent.SpatialReference.Wkid == 4326)
                        {
                            SpatialReference webMercator = new SpatialReference(102100);
                            SpatialReference WGS84 = new SpatialReference(4326);

                            MapPoint firstCornerWGS84 = (new MapPoint(shapeFileExtent.XMin, shapeFileExtent.YMin, WGS84));
                            MapPoint secondCornerWGS84 = (new MapPoint(shapeFileExtent.XMax, shapeFileExtent.YMax, WGS84));

                            MapPoint firstCorner = (MapPoint)Esri.ArcGISRuntime.Geometry.GeometryEngine.Project(firstCornerWGS84, webMercator);
                            MapPoint secondCorner = (MapPoint)Esri.ArcGISRuntime.Geometry.GeometryEngine.Project(secondCornerWGS84, webMercator);

                            ArcGIS_MapView.SetView(new Envelope(firstCorner, secondCorner));
                        }
                    }
                }
                else
                {
                    ArcGIS_MapView.SetView(graphicsLayer.FullExtent);
                }

                ///////////graphicsLayer.RenderingMode = GraphicsRenderingMode.Static; dpb error RenderingMode could not be changed after load. Set in above code?

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
            GraphicsLayer graphicsLayer = ArcGIS_MapView.Map.Layers[LayerId.ToString()] as GraphicsLayer;
            return graphicsLayer;
        }
    }
}
