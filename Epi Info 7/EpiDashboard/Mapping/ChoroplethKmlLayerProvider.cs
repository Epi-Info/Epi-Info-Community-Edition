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

            GraphicsLayer graphicsLayer = GetGraphicsLayer(shapeLayer);

            if (graphicsLayer != null)
            {
                IDictionary<string, object> coreAttributes = graphicsLayer.Graphics[0].Attributes;
                Dictionary<string, object> allAttributes = new Dictionary<string, object>();
                
                foreach (KeyValuePair<string, object> attr in coreAttributes)
                {
                    allAttributes.Add(attr.Key, attr.Value);
                }

                if (graphicsLayer.Graphics[0].Attributes.ContainsKey("extendedData"))
                {
                    List<KmlExtendedData> eds = (List<KmlExtendedData>)graphicsLayer.Graphics[0].Attributes["extendedData"];
                    
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

            double xmin = graphicsLayer.Graphics[0].Geometry.Extent.XMin;
            double xmax = graphicsLayer.Graphics[0].Geometry.Extent.XMax;
            double ymin = graphicsLayer.Graphics[0].Geometry.Extent.YMin;
            double ymax = graphicsLayer.Graphics[0].Geometry.Extent.YMax;

            foreach (Graphic g in graphicsLayer.Graphics)
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

        override public string GetShapeValue(Graphic graphicFeature, string shapeValue)
        {
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

            return shapeValue;
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
