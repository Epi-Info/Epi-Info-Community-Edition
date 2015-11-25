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
using ESRI.ArcGIS.Client.Toolkit.DataSources;
using ESRI.ArcGIS.Client.Toolkit.DataSources.Kml;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;

namespace EpiDashboard.Mapping
{
    public class ChoroplethKmlLayerProvider : ChoroplethLayerProvider, IChoroLayerProvider
    {
        public event FeatureLoadedHandler FeatureLoaded;

        string _kmlURL;

        public ChoroplethKmlLayerProvider(Map clientMap) : base(clientMap)
        {
            ArcGIS_Map = clientMap;
            this._layerId = Guid.NewGuid();
        }

        //private double dist(Point px, Point po, Point pf)
        //{
        //    double d = Math.Sqrt((px.Y - po.Y) * (px.Y - po.Y) + (px.X - po.X) * (px.X - po.X));
        //    if (((px.Y < po.Y) && (pf.Y > po.Y)) || ((px.Y > po.Y) && (pf.Y < po.Y)) || ((px.Y == po.Y) && (px.X < po.X) && (pf.X > po.X)) || ((px.Y == po.Y) && (px.X > po.X) && (pf.X < po.X)))
        //    {
        //        d = -d;
        //    }
        //    return d;
        //}

        public object[] LoadKml()
        {
            KmlDialog dialog = new KmlDialog();
            object[] kmlfile = null;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                kmlfile = LoadKml(dialog.ServerName);
                return kmlfile;
            }
            else return null;
        }

        public object[] LoadKml(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                _kmlURL = url;
                KmlLayer shapeLayer = ArcGIS_Map.Layers[_layerId.ToString()] as KmlLayer;
                
                if (shapeLayer != null)
                {
                    ArcGIS_Map.Layers.Remove(shapeLayer);
                }
                
                shapeLayer = new KmlLayer();
                shapeLayer.ID = _layerId.ToString();
                shapeLayer.Url = new Uri(url);
                shapeLayer.Initialized += new EventHandler<EventArgs>(shapeLayer_Initialized);
                ArcGIS_Map.Layers.Add(shapeLayer);

                ArcGIS_Map.Extent = shapeLayer.FullExtent;
                return new object[] { shapeLayer };
            }
            else { return null; }

        }

        void shapeLayer_Initialized(object sender, EventArgs e)
        {
            KmlLayer shapeLayer = ArcGIS_Map.Layers[_layerId.ToString()] as KmlLayer;

            FindGraphicsLayers(shapeLayer);
            if (_graphicsLayers.Count > 0)
            {
                IDictionary<string, object> coreAttributes = _graphicsLayers[0].Graphics[0].Attributes;
                Dictionary<string, object> allAttributes = new Dictionary<string, object>();
                foreach (KeyValuePair<string, object> attr in coreAttributes)
                {
                    allAttributes.Add(attr.Key, attr.Value);
                }
                if (_graphicsLayers[0].Graphics[0].Attributes.ContainsKey("extendedData"))
                {
                    List<KmlExtendedData> eds = (List<KmlExtendedData>)_graphicsLayers[0].Graphics[0].Attributes["extendedData"];
                    foreach (KmlExtendedData ed in eds)
                    {
                        allAttributes.Add(ed.Name, ed.Value);
                    }
                }

                if (FeatureLoaded != null)
                {
                    FeatureLoaded(_kmlURL, allAttributes);
                }
            }

            double xmin = _graphicsLayers[0].Graphics[0].Geometry.Extent.XMin;
            double xmax = _graphicsLayers[0].Graphics[0].Geometry.Extent.XMax;
            double ymin = _graphicsLayers[0].Graphics[0].Geometry.Extent.YMin;
            double ymax = _graphicsLayers[0].Graphics[0].Geometry.Extent.YMax;
            foreach (Graphic g in _graphicsLayers[0].Graphics)
            {
                if (g.Geometry.Extent.XMin < xmin)
                    xmin = g.Geometry.Extent.XMin;

                if (g.Geometry.Extent.YMin < ymin)
                    ymin = g.Geometry.Extent.YMin;

                if (g.Geometry.Extent.XMax > xmax)
                    xmax = g.Geometry.Extent.XMax;

                if (g.Geometry.Extent.YMax > ymax)
                    ymax = g.Geometry.Extent.YMax;
            }

            ArcGIS_Map.Extent = new Envelope(ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(xmin - 0.5, ymax + 0.5)), ESRI.ArcGIS.Client.Bing.Transform.GeographicToWebMercator(new MapPoint(xmax + 0.5, ymin - 0.5)));
        }

        private void FindGraphicsLayers(KmlLayer kmlLayer)
        {
            foreach (Layer layer in kmlLayer.ChildLayers)
            {
                if (layer is GraphicsLayer)
                {
                    _graphicsLayers.Add((GraphicsLayer)layer);
                }
                else if (layer is KmlLayer)
                {
                    FindGraphicsLayers((KmlLayer)layer);
                }
            }
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


        public void ResetRangeValues(string shapeKey, string dataKey, string valueField, int classCount)
        {
            _classCount = classCount;
            _shapeKey = shapeKey;
            _dataKey = dataKey;

            DataTable loadedData = GetLoadedData(_dashboardHelper, _dataKey, ref valueField);

            KmlLayer kmlLayer = ArcGIS_Map.Layers[_layerId.ToString()] as KmlLayer;
            GraphicsLayer graphicsLayer = null;

            foreach (Layer layer in kmlLayer.ChildLayers)
            {
                if (layer is GraphicsLayer)
                {
                    graphicsLayer = layer as GraphicsLayer;
                    break;
                }
                else if (layer is KmlLayer)
                {
                    FindGraphicsLayers((KmlLayer)layer);
                }
            }

            if (graphicsLayer == null) return;

            _thematicItem = GetThematicItem(_classCount, loadedData, graphicsLayer);
        }

        //  xxxx
        public object[] LoadShapeFile()
        {
            throw new NotImplementedException();
        }

        private void SetLegendSection(int classCount, List<SolidColorBrush> brushList, string missingText, ThematicItem thematicItem)
        {
            if (LegendStackPanel == null)
            {
                LegendStackPanel = new StackPanel();
            }
            LegendStackPanel.Children.Clear();

            System.Windows.Controls.ListBox legendList = new System.Windows.Controls.ListBox();
            legendList.Margin = new Thickness(5);
            legendList.Background = Brushes.White;// new LinearGradientBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF), Color.FromArgb(0x7F, 0xFF, 0xFF, 0xFF), 45);
            legendList.BorderBrush = Brushes.Black;
            legendList.BorderThickness = new Thickness(3);
            //LegendTitle.Text = thematicItem.Description;

            Rectangle missingSwatchRect = new Rectangle()
            {
                Width = 20,
                Height = 20,
                Stroke = new SolidColorBrush(Colors.Black),
                Fill = brushList[brushList.Count - 1]
            };

            TextBlock titleTextBlock = new TextBlock();
            titleTextBlock.Text = LegendText;          //        String.Format("  " + "Title Text");
            StackPanel titleTextStackPanel = new StackPanel();
            titleTextStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
            //  titleTextStackPanel.Children.Add(missingSwatchRect);

            titleTextStackPanel.Children.Add(titleTextBlock);


            TextBlock missingClassTextBlock = new TextBlock();
            missingClassTextBlock.Text = String.Format("  " + missingText);
            StackPanel missingClassStackPanel = new StackPanel();
            missingClassStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
            missingClassStackPanel.Children.Add(missingSwatchRect);
            missingClassStackPanel.Children.Add(missingClassTextBlock);

            legendList.Items.Add(titleTextStackPanel);

            legendList.Items.Add(missingClassStackPanel);

            SetLegendText(brushList, classCount, thematicItem.RangeStarts, legendList);

            TextBlock minTextBlock = new TextBlock();
            StackPanel minStackPanel = new StackPanel();
            minStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
            minTextBlock.Text = thematicItem.MinName != null ? String.Format("Min: {0} ({1})", thematicItem.Min, thematicItem.MinName.Trim()) : String.Format("Min: {0} ({1})", thematicItem.Min, string.Empty);
            minStackPanel.Children.Add(minTextBlock);
            legendList.Items.Add(minStackPanel);

            TextBlock maxTextBlock = new TextBlock();
            StackPanel maxStackPanel = new StackPanel();
            maxStackPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;
            maxTextBlock.Text = thematicItem.MaxName != null ? String.Format("Max: {0} ({1})", thematicItem.Max, thematicItem.MaxName.Trim()) : String.Format("Max: {0} ({1})", thematicItem.Max, string.Empty);
            maxStackPanel.Children.Add(maxTextBlock);
            legendList.Items.Add(maxStackPanel);

            LegendStackPanel.Children.Add(legendList);
        }

        public void ResetRangeValues()
        {
            string valueField = _valueField;

            DataTable loadedData = GetLoadedData(_dashboardHelper, _dataKey, ref valueField);

            GraphicsLayer graphicsLayer = _graphicsLayers[0];
            if (graphicsLayer == null) return;

            ThematicItem thematicItem = GetThematicItem(_classCount, loadedData, graphicsLayer);
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

                string shapeValue = string.Empty;

                if (!graphicFeature.Attributes.ContainsKey(_shapeKey))
                {
                    List<KmlExtendedData> eds = (List<KmlExtendedData>)graphicFeature.Attributes["extendedData"];
                    foreach (KmlExtendedData ed in eds)
                    {
                        if (ed.Name.Equals(_shapeKey))
                        {
                            shapeValue = ed.Value.Replace("'", "''").Trim();
                        }
                    }
                }
                else
                {
                    shapeValue = graphicFeature.Attributes[_shapeKey].ToString().Replace("'", "''").Trim();
                }


                string filterExpression = "";
                if (_dataKey.Contains(" ") || _dataKey.Contains("$") || _dataKey.Contains("#"))
                    filterExpression += "[";
                filterExpression += _dataKey;
                if (_dataKey.Contains(" ") || _dataKey.Contains("$") || _dataKey.Contains("#"))
                    filterExpression += "]";
                filterExpression += " = '" + shapeValue + "'";

                double graphicValue = Double.PositiveInfinity;

                try
                {
                    DataRow[] rows = loadedData.Select(filterExpression);

                    if (rows.Length > 0)
                    {
                        object found = rows[0][_valueField];
                        string valueField;

                        if (found is string)
                        {
                            valueField = (string)found;
                            graphicValue = Convert.ToDouble(valueField);
                        }
                    }
                }
                catch { }

                string graphicName = shapeValue;

                if (i == 0)
                {
                    thematicItem.Min = Double.PositiveInfinity;
                    thematicItem.Max = Double.NegativeInfinity;
                    thematicItem.MinName = string.Empty;
                    thematicItem.MaxName = string.Empty;
                }
                else
                {
                    if (graphicValue < thematicItem.Min) { thematicItem.Min = graphicValue; thematicItem.MinName = graphicName; }
                    if (graphicValue > thematicItem.Max && graphicValue != Double.PositiveInfinity) { thematicItem.Max = graphicValue; thematicItem.MaxName = graphicName; }
                }

                if (graphicValue < Double.PositiveInfinity)
                {
                    valueList.Add(graphicValue);
                }
            }
            thematicItem.RangeStarts = CalculateThematicRange(classCount, thematicItem, valueList);

            return thematicItem;
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

                GraphicsLayer graphicsLayer = ArcGIS_Map.Layers[_layerId.ToString()] as GraphicsLayer;

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

            GraphicsLayer graphicsLayer = _graphicsLayers[0];

            _thematicItem = GetThematicItem(_classCount, loadedData, graphicsLayer);

            if (Range != null &&
                Range.Count > 0)
            {
                _thematicItem.RangeStarts = Range;
            }

            PopulateRangeValues();
        }
        #region ILayerProvider Members

        public void CloseLayer()
        {
            KmlLayer shapeLayer = ArcGIS_Map.Layers[_layerId.ToString()] as KmlLayer;
            if (shapeLayer != null)
            {
                ArcGIS_Map.Layers.Remove(shapeLayer);
                
                if (LegendStackPanel != null)
                {
                    LegendStackPanel.Children.Clear();
                }
            }
        }

        #endregion
    }
}
