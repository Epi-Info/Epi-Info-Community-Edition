﻿using System;
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
    public class DotDensityLayerProvider : ILayerProvider
    {
        public event FeatureLoadedHandler FeatureLoaded;

        #region DotDensity

        private Map myMap;
        private DashboardHelper dashboardHelper;
        private string shapeKey;
        private string dataKey;
        private string valueField;
        private Guid layerId;
        private Color dotColor;
        private int dotValue;
        private string url;

        public DotDensityLayerProvider(Map myMap)
        {
            this.myMap = myMap;
            this.layerId = Guid.NewGuid();
        }

        public void MoveUp()
        {
            Layer layer = myMap.Layers[layerId.ToString()];
            Layer dotLayer = myMap.Layers[layerId.ToString() + "_dotLayer"];
            int currentIndex = myMap.Layers.IndexOf(layer);
            int currentDotIndex = myMap.Layers.IndexOf(dotLayer);
            if (currentIndex < myMap.Layers.Count - 1)
            {
                myMap.Layers.Remove(layer);
                myMap.Layers.Insert(currentIndex + 1, layer);
                myMap.Layers.Remove(dotLayer);
                myMap.Layers.Insert(currentDotIndex + 1, dotLayer);
            }
        }

        public void MoveDown()
        {
            Layer layer = myMap.Layers[layerId.ToString()];
            Layer dotLayer = myMap.Layers[layerId.ToString() + "_dotLayer"];
            int currentIndex = myMap.Layers.IndexOf(layer);
            int currentDotIndex = myMap.Layers.IndexOf(dotLayer);
            if (currentIndex > 1)
            {
                myMap.Layers.Remove(layer);
                myMap.Layers.Insert(currentIndex - 1, layer);
                myMap.Layers.Remove(dotLayer);
                myMap.Layers.Insert(currentDotIndex - 1, dotLayer);
            }
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

                GraphicsLayer graphicsLayer = myMap.Layers[layerId.ToString()] as GraphicsLayer;
                if (graphicsLayer == null)
                {
                    graphicsLayer = new GraphicsLayer();
                    graphicsLayer.ID = layerId.ToString();
                    myMap.Layers.Add(graphicsLayer);
                }

                int recCount = shapeFileReader.Records.Count;
                int rgbFactor = 255 / recCount;
                int counter = 0;
                foreach (ShapeFileReader.ShapeFileRecord record in shapeFileReader.Records)
                {
                    Graphic graphic = record.ToGraphic();
                    if (graphic != null)
                    {
                        graphic.Symbol = GetFillSymbol(new SolidColorBrush(Color.FromArgb(192, 255, 255, 255)));
                        graphicsLayer.Graphics.Add(graphic);
                    }
                    counter += rgbFactor;
                }
                if (graphicsLayer.FullExtent == null)
                {
                    Envelope shapeFileExtent = shapeFileReader.GetExtent();
                    if (shapeFileExtent.SpatialReference == null)
                    {
                        myMap.Extent = shapeFileExtent;
                    }
                    else
                    {
                        if (shapeFileExtent.SpatialReference.WKID == 4326)
                        {
                            myMap.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(shapeFileExtent.XMin, shapeFileExtent.YMin)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(shapeFileExtent.XMax, shapeFileExtent.YMax)));
                        }
                    }
                }
                else
                {
                    myMap.Extent = graphicsLayer.FullExtent;
                }
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

                GraphicsLayer graphicsLayer = myMap.Layers[layerId.ToString()] as GraphicsLayer;
                if (graphicsLayer == null)
                {
                    graphicsLayer = new GraphicsLayer();
                    graphicsLayer.ID = layerId.ToString();
                    myMap.Layers.Add(graphicsLayer);
                }

                int recCount = shapeFileReader.Records.Count;
                int rgbFactor = 255 / recCount;
                int counter = 0;
                foreach (ShapeFileReader.ShapeFileRecord record in shapeFileReader.Records)
                {
                    Graphic graphic = record.ToGraphic();
                    if (graphic != null)
                    {
                        graphic.Symbol = GetFillSymbol(new SolidColorBrush(Color.FromArgb(192, 255, 255, 255)));
                        graphicsLayer.Graphics.Add(graphic);
                    }
                    counter += rgbFactor;
                }
                Envelope shapeFileExtent = shapeFileReader.GetExtent();
                if (shapeFileExtent.SpatialReference == null)
                {
                    myMap.Extent = shapeFileExtent;
                }
                else
                {
                    if (shapeFileExtent.SpatialReference.WKID == 4326)
                    {
                        myMap.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(shapeFileExtent.XMin, shapeFileExtent.YMin)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(shapeFileExtent.XMax, shapeFileExtent.YMax)));
                    }
                }

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
            if (dashboardHelper != null)
            {
                SetShapeRangeValues(dashboardHelper, shapeKey, dataKey, valueField, dotColor, dotValue);
            }
        }

        private Symbol MarkerSymbol
        {
            get
            {
                SimpleMarkerSymbol symbol = new SimpleMarkerSymbol();
                symbol.Color = new SolidColorBrush(dotColor);
                symbol.Size = 5;
                symbol.Style = SimpleMarkerSymbol.SimpleMarkerStyle.Circle;
                return symbol;
            }
        }

        public void SetShapeRangeValues(DashboardHelper dashboardHelper, string shapeKey, string dataKey, string valueField, Color dotColor, int dotValue)
        {
            try
            {
                this.dotValue = dotValue;
                this.dashboardHelper = dashboardHelper;
                this.shapeKey = shapeKey;
                this.dataKey = dataKey;
                this.valueField = valueField;
                this.dotColor = dotColor;

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
                    loadedData = dashboardHelper.GenerateFrequencyTableforMap(gadgetOptions).First().Key;
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


                GraphicsLayer graphicsLayer = myMap.Layers[layerId.ToString()] as GraphicsLayer;

                List<double> valueList = new List<double>();
                List<Graphic> graphicsToBeAdded = new List<Graphic>();
                SpatialReference geoReference = new SpatialReference(4326);
                for (int i = 0; i < graphicsLayer.Graphics.Count; i++)
                {
                    Graphic graphicFeature = graphicsLayer.Graphics[i];

                    double xmin = graphicFeature.Geometry.Extent.XMin;
                    double xmax = graphicFeature.Geometry.Extent.XMax;
                    double ymin = graphicFeature.Geometry.Extent.YMin;
                    double ymax = graphicFeature.Geometry.Extent.YMax;

                    Random rnd = new Random();

                    string shapeValue = graphicFeature.Attributes[shapeKey].ToString().Replace("'", "''").Trim();
                   
                    
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

                        for (int counter = 0; counter < graphicValue; counter += dotValue)
                        {
                            bool pointInGraphic = false;

                            ExtendedGraphic graphic = new ExtendedGraphic();
                            while (!pointInGraphic)
                            {
                                double rndX = xmin + (rnd.NextDouble() * (xmax - xmin));
                                double rndY = ymin + (rnd.NextDouble() * (ymax - ymin));
                                graphic.Geometry = new MapPoint(rndX, rndY, geoReference);
                                graphic.Symbol = MarkerSymbol;

                                bool foundBottomLeft = false;
                                bool foundBottomRight = false;
                                bool foundTopLeft = false;
                                bool foundTopRight = false;
                                foreach (ESRI.ArcGIS.Client.Geometry.PointCollection pc in ((ESRI.ArcGIS.Client.Geometry.Polygon)graphicFeature.Geometry).Rings)
                                {
                                    foundBottomLeft = pc.Any((point) => point.Y <= ((MapPoint)graphic.Geometry).Y && point.X <= ((MapPoint)graphic.Geometry).X);
                                    foundBottomRight = pc.Any((point) => point.Y <= ((MapPoint)graphic.Geometry).Y && point.X >= ((MapPoint)graphic.Geometry).X);
                                    foundTopLeft = pc.Any((point) => point.Y >= ((MapPoint)graphic.Geometry).Y && point.X <= ((MapPoint)graphic.Geometry).X);
                                    foundTopRight = pc.Any((point) => point.Y >= ((MapPoint)graphic.Geometry).Y && point.X >= ((MapPoint)graphic.Geometry).X);

                                    if (foundBottomLeft && foundBottomRight && foundTopLeft && foundTopRight)
                                    {
                                        try
                                        {
                                            MapPoint firstBottomLeft = pc.First((point) => point.Y <= ((MapPoint)graphic.Geometry).Y && point.X <= ((MapPoint)graphic.Geometry).X);
                                            MapPoint firstBottomRight = pc.First((point) => point.Y <= ((MapPoint)graphic.Geometry).Y && point.X >= ((MapPoint)graphic.Geometry).X);
                                            MapPoint firstTopLeft = pc.First((point) => point.Y >= ((MapPoint)graphic.Geometry).Y && point.X <= ((MapPoint)graphic.Geometry).X);
                                            MapPoint firstTopRight = pc.First((point) => point.Y >= ((MapPoint)graphic.Geometry).Y && point.X >= ((MapPoint)graphic.Geometry).X);

                                            int indexBL = pc.IndexOf(firstBottomLeft);
                                            int indexBR = pc.IndexOf(firstBottomRight);
                                            int indexTL = pc.IndexOf(firstTopLeft);
                                            int indexTR = pc.IndexOf(firstTopRight);

                                            MapPoint lastBottomLeft = pc.Last((point) => point.Y <= ((MapPoint)graphic.Geometry).Y && point.X <= ((MapPoint)graphic.Geometry).X);
                                            MapPoint lastBottomRight = pc.Last((point) => point.Y <= ((MapPoint)graphic.Geometry).Y && point.X >= ((MapPoint)graphic.Geometry).X);
                                            MapPoint lastTopLeft = pc.Last((point) => point.Y >= ((MapPoint)graphic.Geometry).Y && point.X <= ((MapPoint)graphic.Geometry).X);
                                            MapPoint lastTopRight = pc.Last((point) => point.Y >= ((MapPoint)graphic.Geometry).Y && point.X >= ((MapPoint)graphic.Geometry).X);

                                            int indexBL2 = pc.IndexOf(lastBottomLeft);
                                            int indexBR2 = pc.IndexOf(lastBottomRight);
                                            int indexTL2 = pc.IndexOf(lastTopLeft);
                                            int indexTR2 = pc.IndexOf(lastTopRight);

                                            if ((Math.Abs(indexTL - indexTR2) == 1 && Math.Abs(indexTR - indexBR2) == 1) || (Math.Abs(indexBL - indexTL2) == 1 && Math.Abs(indexTL - indexTR2) == 1) || (Math.Abs(indexBR - indexBL2) == 1 && Math.Abs(indexTR - indexBR2) == 1))
                                            {
                                                pointInGraphic = true;
                                                break;
                                            }
                                            else if ((Math.Abs(indexBL - indexBR2) == 1 && Math.Abs(indexTL - indexBL2) == 1) || (Math.Abs(indexTL - indexBL2) == 1 && Math.Abs(indexTR - indexTL2) == 1) || (Math.Abs(indexBR - indexTR2) == 1 && Math.Abs(indexTR - indexTL2) == 1))
                                            {
                                                pointInGraphic = true;
                                                break;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            pointInGraphic = false;
                                        }
                                    }
                                }
                            }
                            graphicsToBeAdded.Add(graphic);
                        }

                    }
                    catch (Exception ex)
                    {
                        graphicValue = Double.PositiveInfinity;
                    }
                }

                foreach (Graphic g in graphicsLayer.Graphics)
                {
                    SimpleFillSymbol symbol = new SimpleFillSymbol()
                    {
                        Fill = new SolidColorBrush(Colors.Transparent),
                        BorderBrush = new SolidColorBrush(Colors.Black),
                        BorderThickness = 1
                    };

                    g.Symbol = symbol;
                }

                GraphicsLayer dotLayer = myMap.Layers[layerId.ToString() + "_dotLayer"] as GraphicsLayer;
                int currentDotIndex = myMap.Layers.Count;
                if (dotLayer != null)
                {
                    currentDotIndex = myMap.Layers.IndexOf(dotLayer);
                    myMap.Layers.Remove(dotLayer);
                }
                dotLayer = new GraphicsLayer();
                dotLayer.ID = layerId.ToString() + "_dotLayer";
                myMap.Layers.Insert(currentDotIndex, dotLayer);
                foreach (Graphic g in graphicsToBeAdded)
                {
                    dotLayer.Graphics.Add(g);
                }

                if (LegendStackPanel == null)
                {
                    LegendStackPanel = new StackPanel();
                }

                LegendStackPanel.Children.Clear();    

                string description = "";

                if (string.IsNullOrEmpty(description))
                {
                    description = dotValue.ToString() + "  " + valueField;
                }

                System.Windows.Controls.ListBox legendList = new System.Windows.Controls.ListBox();
                legendList.Padding = new Thickness(0, 10, 0, 0);
                legendList.Background = Brushes.White;
                legendList.BorderBrush = Brushes.Black;
                legendList.BorderThickness = new Thickness(0);
                legendList.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);

                TextBlock classTextBlock = new TextBlock();
                classTextBlock.Text = description;
                classTextBlock.FontFamily = new FontFamily("Segoe");
                classTextBlock.FontSize = 12;
                classTextBlock.MaxWidth = 256;
                classTextBlock.TextWrapping = TextWrapping.Wrap;
                classTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                classTextBlock.VerticalAlignment = VerticalAlignment.Center;
                classTextBlock.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);

                Ellipse circle = new Ellipse();
                circle.Width = 14;
                circle.Height = 14;
                circle.VerticalAlignment = VerticalAlignment.Top;
                circle.Margin = new Thickness(0, 4, 7, 4);
                circle.Fill = new SolidColorBrush(dotColor);// this.clusterColor; //dpb

                StackPanel classStackPanel = new StackPanel();
                classStackPanel.Margin = new Thickness(10, 0, 10, 10);
                classStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
                classStackPanel.Children.Add(circle);
                classStackPanel.Children.Add(classTextBlock);

                legendList.Items.Add(classStackPanel);

                LegendStackPanel.Children.Add(legendList);
            }
            catch (Exception ex)
            {
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            throw new NotImplementedException();
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
            Layer shapeLayer = myMap.Layers[layerId.ToString()] as Layer;
            if (shapeLayer != null)
            {
                myMap.Layers.Remove(shapeLayer);
            }
            GraphicsLayer dotLayer = myMap.Layers[layerId.ToString() + "_dotLayer"] as GraphicsLayer;
            if (dotLayer != null)
            {
                myMap.Layers.Remove(dotLayer);
            }
        }

        #endregion
    }
}
