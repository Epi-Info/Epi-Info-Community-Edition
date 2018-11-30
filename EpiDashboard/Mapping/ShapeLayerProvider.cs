using System;
using System.IO;
using System.Windows.Media;

using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;

using EpiDashboard.Mapping.ShapeFileReader;

namespace EpiDashboard.Mapping
{

    public class ShapeLayerProvider : ILayerProvider
    {
        private Map map;

        private Guid layerId;
        private string fileName;

        public ShapeLayerProvider(Map myMap)
        {
            this.map = myMap;
            this.layerId = Guid.NewGuid();
        }

        public void Refresh()
        {
            Layer markerLayer = map.OperationalLayers[layerId.ToString()] as Layer;
            if (markerLayer != null)
            {
                //''markerLayer.ClearGraphics();
                RenderShape(this.fileName);
            }
        }

        public void MoveUp()
        {
            Layer layer = map.OperationalLayers[layerId.ToString()];
            int currentIndex = map.OperationalLayers.IndexOf(layer);
            if (currentIndex < map.OperationalLayers.Count - 1)
            {
                map.OperationalLayers.Remove(layer);
                map.OperationalLayers.Insert(currentIndex + 1, layer);
            }
        }

        public void MoveDown()
        {
            Layer layer = map.OperationalLayers[layerId.ToString()];
            int currentIndex = map.OperationalLayers.IndexOf(layer);
            if (currentIndex > 1)
            {
                map.OperationalLayers.Remove(layer);
                map.OperationalLayers.Insert(currentIndex - 1, layer);
            }
        }

        public void RenderShape(string fileName)
        {
            this.fileName = fileName;

            Layer shapeLayer = map.OperationalLayers[layerId.ToString()] as Layer;
            if (shapeLayer != null)
            {
                map.OperationalLayers.Remove(shapeLayer);
            }
            shapeLayer = new FeatureLayer();
            shapeLayer.Id = layerId.ToString();
            map.OperationalLayers.Add(shapeLayer);

            //Get the file info objects for the SHP and the DBF file selected by the user
            FileInfo shapeFile = new FileInfo(fileName);
            string directoryName = Path.GetDirectoryName(fileName);
            string dbfFilename = Path.GetFileName(fileName).ToLowerInvariant().Replace(".shp", ".dbf");
            string dbfFullPath = Path.Combine(directoryName, dbfFilename);
            FileInfo dbfFile = new FileInfo(dbfFullPath);
            if (!dbfFile.Exists)
            {
                System.Windows.MessageBox.Show("Associated DBF file not found");
                return;
            }

            //Read the SHP and DBF files into the ShapeFileReader
            ShapeFileReader.ShapeFile shapeFileReader = new ShapeFileReader.ShapeFile();
            if (shapeFile != null && dbfFile != null)
            {
                try
                {
                    shapeFileReader.Read(shapeFile, dbfFile);
                }
                catch (NotSupportedException e)
                {
                    System.Windows.MessageBox.Show(e.Message);
                    return;
                }
                catch
                {
                    System.Windows.MessageBox.Show(DashboardSharedStrings.DASHBOARD_MAP_N_POLYGONS_EXCEEDED);
                    return;
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Associated DBF file not found");
                return;
            }

            int recCount = shapeFileReader.Records.Count;
            int rgbFactor = 255 / recCount;
            int counter = 0;

            foreach (ShapeFileReader.ShapeFileRecord record in shapeFileReader.Records)
            {
                Graphic graphic = record.ToGraphic();
                if (graphic != null)
                {
                    graphic.Symbol = GetFillSymbol(System.Drawing.Color.FromArgb(192, 255, 255, 255));
                    //''shapeLayer.Graphics.Add(graphic);
                }
                counter += rgbFactor;
            }

            Envelope shapeFileExtent = shapeFileReader.GetExtent();

            if (shapeFileExtent.SpatialReference == null)
            {
                //''MapView map.Extent = shapeFileExtent;
            }
            else
            {
                //''if (shapeFileExtent.SpatialReference.WKID == 4326)
                {
                    //'' map.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(shapeFileExtent.XMin, shapeFileExtent.YMin)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(shapeFileExtent.XMax, shapeFileExtent.YMax)));
                }
            }
        }

        public string RenderShape()
        {
            Guid layerId = Guid.NewGuid();
            //Create the dialog allowing the user to select the "*.shp" and the "*.dbf" files
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "ESRI Shapefiles (*.shp)|*.shp";
            //ofd.Multiselect = true;

            if (ofd.ShowDialog().Value)
            {
                RenderShape(ofd.FileName);
                return ofd.FileName;
            }
            return null;
        }

        public SimpleFillSymbol GetFillSymbol(SolidColorBrush brush)
        {
            System.Drawing.Color fillColor = System.Drawing.Color.FromArgb(
                brush.Color.A,
                brush.Color.R,
                brush.Color.G,
                brush.Color.B            
            );

            SimpleFillSymbol symbol = GetFillSymbol(fillColor);

            return symbol;
        }

        public SimpleFillSymbol GetFillSymbol(System.Drawing.Color fillColor)
        {
            SimpleFillSymbol symbol = new SimpleFillSymbol();
            symbol.Color = fillColor;
            symbol.Outline.Color = System.Drawing.Color.Gray;
            symbol.Outline.Width = 1;
            return symbol;
        }


        #region ILayerProvider Members

        public void CloseLayer()
        {
            Layer graphicsLayer = map.OperationalLayers[layerId.ToString()] as Layer;
            if (graphicsLayer != null)
            {
                map.OperationalLayers.Remove(graphicsLayer);
            }
        }

        #endregion
    }
}
