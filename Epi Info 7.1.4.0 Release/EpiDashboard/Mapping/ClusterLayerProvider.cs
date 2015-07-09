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
using System.Windows.Controls.DataVisualization.Charting;

namespace EpiDashboard.Mapping
{
    public delegate void DateRangeDefinedHandler(DateTime start, DateTime end, List<KeyValuePair<DateTime, int>> intervalCounts);

    public class ClusterLayerProvider : ILayerProvider
    {
        public event RecordSelectedHandler RecordSelected;
        public event DateRangeDefinedHandler DateRangeDefined;

        private Map myMap;
        private List<KeyValuePair<DateTime, int>> intervalCounts;
        private double minX;
        private double minY;
        private double maxX;
        private double maxY;
        private DateTime minTime;
        private DateTime maxTime;
        private DashboardHelper dashboardHelper;
        private string latVar;
        private string longVar;
        private string timeVar;
        private string description;
        private Guid layerId;
        private SolidColorBrush clusterColor;
        private SpatialReference geoReference;
        private StackPanel legendStackPanel;


        public ClusterLayerProvider(Map myMap)
        {
            this.myMap = myMap;
            this.layerId = Guid.NewGuid();
            this.geoReference = new SpatialReference(4326);
        }

        public string TimeVar
        {
            set
            {
                this.timeVar = value;
                Refresh();
            }
        }

        public StackPanel LegendStackPanel
        {
            get { return this.legendStackPanel; }
            set { this.legendStackPanel = value; }
        }

        public void Refresh()
        {
            GraphicsLayer clusterLayer = myMap.Layers[layerId.ToString()] as GraphicsLayer;
            if (clusterLayer != null)
            {
                clusterLayer.ClearGraphics();
                RenderClusterMap(this.dashboardHelper, this.latVar, this.longVar, this.clusterColor, 
                    this.timeVar, this.description);
            }
        }

        public void MoveUp()
        {
            Layer layer = myMap.Layers[layerId.ToString()];
            int currentIndex = myMap.Layers.IndexOf(layer);
            if (currentIndex < myMap.Layers.Count - 1)
            {
                myMap.Layers.Remove(layer);
                myMap.Layers.Insert(currentIndex + 1, layer);
            }
        }

        public void MoveDown()
        {
            Layer layer = myMap.Layers[layerId.ToString()];
            int currentIndex = myMap.Layers.IndexOf(layer);
            if (currentIndex > 1)
            {
                myMap.Layers.Remove(layer);
                myMap.Layers.Insert(currentIndex - 1, layer);
            }
        }

        public void RenderClusterMap(DashboardHelper dashboardHelper, string latVar, string longVar, Brush clusterColor, 
            string timeVar, string description)
        {
            this.dashboardHelper = dashboardHelper;
            this.latVar = latVar;
            this.longVar = longVar;
            this.timeVar = timeVar;
            this.description = description;
            this.clusterColor = (SolidColorBrush)clusterColor;

            GraphicsLayer clusterLayer = myMap.Layers[layerId.ToString()] as GraphicsLayer;
            if (clusterLayer != null)
            {
                clusterLayer.Graphics.Clear();
            }
            else
            {
                clusterLayer = new GraphicsLayer();
                clusterLayer.ID = layerId.ToString();
                myMap.Layers.Add(clusterLayer);
            }

            CustomCoordinateList coordinateList = GetCoordinates(dashboardHelper, latVar, longVar, timeVar);
            for (int i = 0; i < coordinateList.Coordinates.Count; i++)
            {
                ExtendedGraphic graphic = new ExtendedGraphic()
                {
                    Geometry = new MapPoint(coordinateList.Coordinates[i].X, coordinateList.Coordinates[i].Y, geoReference),
                    RecordId = coordinateList.Coordinates[i].RecordId,
                    Symbol = MarkerSymbol
                };
                if (coordinateList.Coordinates[i].TimeSpan.HasValue)
                    graphic.TimeExtent = new TimeExtent(coordinateList.Coordinates[i].TimeSpan.Value);
                else
                    graphic.TimeExtent = new TimeExtent(DateTime.MinValue, DateTime.MaxValue);
                graphic.MouseLeftButtonUp += new MouseButtonEventHandler(graphic_MouseLeftButtonUp);
                clusterLayer.Graphics.Add(graphic);
            }
            Brush flareForeground;
            if (System.Drawing.Color.FromArgb(this.clusterColor.Color.A, this.clusterColor.Color.R, this.clusterColor.Color.G, this.clusterColor.Color.B).GetBrightness() > 0.5)
            {
                flareForeground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                flareForeground = new SolidColorBrush(Colors.White);
            }
            FlareClusterer clusterer = new FlareClusterer()
            {
                FlareBackground = clusterColor,
                FlareForeground = flareForeground,
                MaximumFlareCount = 10,
                Radius = 15,
                Gradient = ClustererGradient
            };
            clusterLayer.Clusterer = clusterer;
            
            if (LegendStackPanel == null)
            {
                LegendStackPanel = new StackPanel();
            }
            LegendStackPanel.Children.Clear();

            if (!string.IsNullOrEmpty(description))
            {
                System.Windows.Controls.ListBox legendList = new System.Windows.Controls.ListBox();
                legendList.Margin = new Thickness(5);
                legendList.Background = Brushes.White;// new LinearGradientBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF), Color.FromArgb(0x7F, 0xFF, 0xFF, 0xFF), 45);
                legendList.BorderBrush = Brushes.Black;
                legendList.BorderThickness = new Thickness(3);
                //LegendTitle.Text = thematicItem.Description;

                TextBlock classTextBlock = new TextBlock();
                classTextBlock.Text = "  " + description;
                classTextBlock.FontSize = 15;

                Ellipse circle = new Ellipse();
                circle.Width = 14;
                circle.Height = 14;
                circle.Fill = this.clusterColor;

                StackPanel classStackPanel = new StackPanel();
                classStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
                classStackPanel.Children.Add(circle);
                classStackPanel.Children.Add(classTextBlock);

                legendList.Items.Add(classStackPanel);

                LegendStackPanel.Children.Add(legendList);
            }

            
            if (coordinateList.Coordinates.Count > 0)
            {
                myMap.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(minX - 0.01, minY - 0.01, geoReference)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(maxX + 0.01, maxY + 0.01, geoReference)));
                if (!string.IsNullOrEmpty(timeVar))
                {
                    if (minTime != null && maxTime != null)
                    {

                        intervalCounts = new List<KeyValuePair<DateTime, int>>();
                        DateTime previousInterval = DateTime.MinValue;
                        IEnumerable<DateTime> intervals = 
                            TimeSlider.CreateTimeStopsByTimeInterval(new TimeExtent(minTime, maxTime), new TimeSpan(1, 0, 0, 0));
                        foreach (DateTime interval in intervals)
                        {
                            int count = clusterLayer.Graphics.Count(x => x.TimeExtent.Start <= interval && x.TimeExtent.Start >= previousInterval);
                            intervalCounts.Add(new KeyValuePair<DateTime, int>(interval.Date, count));
                            previousInterval = interval;
                        }
                        if (DateRangeDefined != null)
                        {
                            DateRangeDefined(minTime, maxTime, intervalCounts);
                        }
                    }
                }
            }
        }

        private Symbol MarkerSymbol
        {
            get
            {
                SimpleMarkerSymbol symbol = new SimpleMarkerSymbol();
                symbol.Color = clusterColor;
                symbol.Size = 15;
                symbol.Style = SimpleMarkerSymbol.SimpleMarkerStyle.Circle;
                return symbol;
            }
        }

        private CustomCoordinateList GetCoordinates(DashboardHelper dashboardHelper, string latVar, 
            string longVar, string timeVar)
        {
            List<string> columnNames = new List<string>();
            if (dashboardHelper.IsUsingEpiProject)
                columnNames.Add("UniqueKey");
            columnNames.Add(latVar);
            if (!columnNames.Exists(s => s.Equals(longVar)))
                columnNames.Add(longVar);
            if (!string.IsNullOrEmpty(timeVar))
            {
                if (!columnNames.Exists(s => s.Equals(timeVar)))
                    columnNames.Add(timeVar);
            }
            DataTable data = dashboardHelper.GenerateTable(columnNames);

            minTime = DateTime.MaxValue;
            maxTime = DateTime.MinValue;
            minX = double.MaxValue;
            maxX = double.MinValue;
            minY = double.MaxValue;
            maxY = double.MinValue;

            CustomCoordinateList coordinateList = new CustomCoordinateList();
            if (data != null)
            {
                foreach (DataRow row in data.Rows)
                {
                    if (row[latVar] != DBNull.Value && row[longVar] != DBNull.Value)
                    {
                        double latitude = double.Parse(row[latVar].ToString());
                        double longitude = double.Parse(row[longVar].ToString());
                        if (latitude <= 90 && latitude >= -90 && longitude <= 180 && longitude >= -180)
                        {
                            int uniqueKey = 0;
                            if (dashboardHelper.IsUsingEpiProject)
                                uniqueKey = (int)row["UniqueKey"];
                            if (string.IsNullOrEmpty(timeVar))
                            {
                                coordinateList.Coordinates.Add(new CustomCoordinate(uniqueKey, latitude, longitude, null));
                            }
                            else if (!data.Columns.Contains(timeVar))
                            {
                                coordinateList.Coordinates.Add(new CustomCoordinate(uniqueKey, latitude, longitude, null));
                            }
                            else
                            {
                                if (row[timeVar] != DBNull.Value)
                                {
                                    DateTime time = (DateTime)row[timeVar];
                                    minTime = minTime < time ? minTime : time;
                                    maxTime = maxTime > time ? maxTime : time;

                                    coordinateList.Coordinates.Add(new CustomCoordinate(uniqueKey, latitude, longitude, time));
                                }
                                else
                                {
                                    coordinateList.Coordinates.Add(new CustomCoordinate(uniqueKey, latitude, longitude, null));
                                }
                            }
                            minY = Math.Min(minY, latitude);
                            maxY = Math.Max(maxY, latitude);
                            minX = Math.Min(minX, longitude);
                            maxX = Math.Max(maxX, longitude);
                        }
                    }
                }
            }
            return coordinateList;
        }

        void graphic_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int recordId = ((ExtendedGraphic)sender).RecordId;
            if (RecordSelected != null)
            {
                RecordSelected(recordId);
            }
        }

        private LinearGradientBrush ClustererGradient
        {
            get
            {
                LinearGradientBrush brush = new LinearGradientBrush();
                brush.MappingMode = BrushMappingMode.RelativeToBoundingBox;
                brush.GradientStops.Add(new GradientStop(clusterColor.Color, 0));
                brush.GradientStops.Add(new GradientStop(clusterColor.Color, 1));
                return brush;
            }
        }

        #region ILayerProvider Members

        public void CloseLayer()
        {
            GraphicsLayer graphicsLayer = myMap.Layers[layerId.ToString()] as GraphicsLayer;
            if (graphicsLayer != null)
            {
                myMap.Layers.Remove(graphicsLayer);
                if (legendStackPanel != null)
                {
                    legendStackPanel.Children.Clear();
                }
            }
        }

        #endregion
    }
}
