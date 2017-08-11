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

using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;

using Epi;
using Epi.Data;
using EpiDashboard.Mapping.ShapeFileReader;
using System.Windows.Controls.DataVisualization.Charting;

namespace EpiDashboard.Mapping
{

    public class PointLayerProvider : ILayerProvider
    {
        public event RecordSelectedHandler RecordSelected;
        public event DateRangeDefinedHandler DateRangeDefined;

        private MapView _mapView;
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
        private Color pointColor;
        private SpatialReference geoReference;
        private SimpleMarkerStyle style;
        private StackPanel legendStackPanel;


        public PointLayerProvider(MapView mapView)
        {
            this._mapView = mapView;
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
            GraphicsLayer pointLayer = _mapView.Map.Layers[layerId.ToString()] as GraphicsLayer;
            if (pointLayer != null)
            {
                pointLayer.Graphics.Clear();
                RenderPointMap
                (
                    this.dashboardHelper, 
                    this.latVar, 
                    this.longVar, 
                    this.pointColor, 
                    this.timeVar, 
                    this.style, 
                    this.description
                );
            }
        }

        public void MoveUp()
        {
            Layer layer = _mapView.Map.Layers[layerId.ToString()];
            int currentIndex = _mapView.Map.Layers.IndexOf(layer);
            if (currentIndex < _mapView.Map.Layers.Count - 1)
            {
                _mapView.Map.Layers.Remove(layer);
                _mapView.Map.Layers.Insert(currentIndex + 1, layer);
            }
        }

        public void MoveDown()
        {
            Layer layer = _mapView.Map.Layers[layerId.ToString()];
            int currentIndex = _mapView.Map.Layers.IndexOf(layer);
            if (currentIndex > 1)
            {
                _mapView.Map.Layers.Remove(layer);
                _mapView.Map.Layers.Insert(currentIndex - 1, layer);
            }
        }

        public void RenderPointMap(DashboardHelper dashboardHelper, string latVar, string longVar, Color pointColor, string timeVar, SimpleMarkerStyle style, string description)
        {
            this.dashboardHelper = dashboardHelper;
            this.latVar = latVar;
            this.longVar = longVar;
            this.timeVar = timeVar;
            this.style = style;
            this.description = description;
            this.pointColor = pointColor;

            GraphicsLayer pointLayer = _mapView.Map.Layers[layerId.ToString()] as GraphicsLayer;
            if (pointLayer != null)
            {
                pointLayer.Graphics.Clear();
            }
            else
            {
                pointLayer = new GraphicsLayer();
                pointLayer.ID = layerId.ToString();
                _mapView.Map.Layers.Add(pointLayer);
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
                {
                    ////////////graphic.TimeExtent = new TimeExtent(coordinateList.Coordinates[i].TimeSpan.Value);
                }
                else
                {
                    ////////////graphic.TimeExtent = new TimeExtent(DateTime.MinValue, DateTime.MaxValue);
                }
                
                //////////////graphic.MouseLeftButtonUp += new MouseButtonEventHandler(graphic_MouseLeftButtonUp);
                pointLayer.Graphics.Add(graphic);
            }

            if (LegendStackPanel == null)
            {
                LegendStackPanel = new StackPanel();
            }

            LegendStackPanel.Children.Clear();    

            if (string.IsNullOrEmpty(description))
            {
                description = SharedStrings.CASES;
            }

            if (!string.IsNullOrEmpty(description))
            {
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

                TextBlock symbolTextBlock = new TextBlock();
                switch (style)
                {
                    case SimpleMarkerStyle.Circle:
                        symbolTextBlock.Text = "●";
                        symbolTextBlock.FontSize = 16;
                        break;
                    case SimpleMarkerStyle.Cross:
                        symbolTextBlock.Text = "+";
                        symbolTextBlock.FontSize = 18;
                        symbolTextBlock.FontWeight = FontWeights.Bold;
                        break;
                    case SimpleMarkerStyle.Diamond:
                        symbolTextBlock.Text = "♦";
                        symbolTextBlock.FontSize = 17;
                        break;
                    case SimpleMarkerStyle.Square:
                        symbolTextBlock.Text = "■";
                        symbolTextBlock.FontSize = 16;
                        break;
                    case SimpleMarkerStyle.Triangle:
                        symbolTextBlock.Text = "▲";
                        symbolTextBlock.FontSize = 16;
                        break;
                    default:
                        symbolTextBlock.Text = "▲";
                        symbolTextBlock.FontSize = 16;
                        break;
                }
                symbolTextBlock.FontSize = 28;

                symbolTextBlock.VerticalAlignment = VerticalAlignment.Top;
                symbolTextBlock.Margin = new Thickness(0, 4, 7, 4);
                symbolTextBlock.Foreground = new SolidColorBrush(this.pointColor);
                
                StackPanel classStackPanel = new StackPanel();
                classStackPanel.Margin = new Thickness(10, 0, 10, 10);
                classStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
                classStackPanel.Children.Add(symbolTextBlock);
                classStackPanel.Children.Add(classTextBlock);

                legendList.Items.Add(classStackPanel);

                LegendStackPanel.Children.Add(legendList);
            }

            if (coordinateList.Coordinates.Count > 0)
            {
                //_mapView.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(minX - 0.01, minY - 0.01, geoReference)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(maxX + 0.01, maxY + 0.01, geoReference)));
                //if (!string.IsNullOrEmpty(timeVar))
                //{
                //    if (minTime != null && maxTime != null)
                //    {

                //        intervalCounts = new List<KeyValuePair<DateTime, int>>();
                //        DateTime previousInterval = DateTime.MinValue;
                //        IEnumerable<DateTime> intervals = TimeSlider.CreateTimeStopsByTimeInterval(new TimeExtent(minTime, maxTime), new TimeSpan(1, 0, 0, 0));
                //        foreach (DateTime interval in intervals)
                //        {
                //            int count = pointLayer.Graphics.Count(x => x.TimeExtent.Start <= interval && x.TimeExtent.Start >= previousInterval);
                //            intervalCounts.Add(new KeyValuePair<DateTime, int>(interval.Date, count));
                //            previousInterval = interval;
                //        }
                //        if (DateRangeDefined != null)
                //        {
                //            DateRangeDefined(minTime, maxTime, intervalCounts);
                //        }
                //    }
                //}
            }
        }

        private SimpleMarkerSymbol MarkerSymbol
        {
            get
            {
                SimpleMarkerSymbol symbol = new SimpleMarkerSymbol();
                symbol.Color = pointColor;
                symbol.Size = 15;
                symbol.Style = style;
                return symbol;
            }
        }

        private CustomCoordinateList GetCoordinates(DashboardHelper dashboardHelper, string latVar, string longVar, string timeVar)
        {
            List<string> columnNames = new List<string>();
            if (dashboardHelper.IsUsingEpiProject)
                columnNames.Add("UniqueKey");
            columnNames.Add(latVar);
            if (!columnNames.Exists(delegate(string s) { return s.Equals(longVar); }))
                columnNames.Add(longVar);
            if (!string.IsNullOrEmpty(timeVar))
            {
                if (!columnNames.Exists(delegate(string s) { return s.Equals(timeVar); }))
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

        #region ILayerProvider Members

        public void CloseLayer()
        {
            GraphicsLayer graphicsLayer = _mapView.Map.Layers[layerId.ToString()] as GraphicsLayer;
            if (graphicsLayer != null)
            {
                _mapView.Map.Layers.Remove(graphicsLayer);
                if (legendStackPanel != null)
                {
                    legendStackPanel.Children.Clear();
                }
            }
        }

        #endregion
    }
}
